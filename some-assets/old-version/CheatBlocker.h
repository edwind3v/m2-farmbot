#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <string>
#include <vector>

namespace Config
{
    constexpr DWORD CB_LOG_OFFSET = 0x7CC10;
    constexpr DWORD GF_POOL_OFFSET = 0x8185A;
    constexpr DWORD GF_CHECKER_OFFSET = 0x81865;
    constexpr DWORD GF_VALIDATION_OFFSET = 0x7FF3A;

    constexpr DWORD WATCHER_INTERVAL_MS = 2000;
    constexpr DWORD CB_CHECK_INTERVAL = 30;
    constexpr DWORD GF_CHECK_INTERVAL = 15;

    constexpr DWORD TARGET_LOG_LEVEL = 4;
}

namespace State
{
    HMODULE hCBModule = nullptr;
    HMODULE hGFModule = nullptr;
    CRITICAL_SECTION csModuleLock;
    volatile bool bCBHooked = false;
    volatile bool bShuttingDown = false;
}

namespace Prototypes
{
    typedef HMODULE(WINAPI* fnLoadLibraryA)(LPCSTR);
    typedef int(__stdcall* fnCB_L_Function)(unsigned int*, void*);
    typedef BOOL(__thiscall* fnValidationCheck)(LPVOID);
    typedef DWORD(WINAPI* fnGetTickCount)();
    typedef HWND(WINAPI* fnFindWindowExA)(HWND, HWND, LPCSTR, LPCSTR);

    fnLoadLibraryA pOrigLoadLibrary = nullptr;
    fnCB_L_Function pOrigCB_L = nullptr;
    fnGetTickCount pOrigGetTickCount = nullptr;
    fnFindWindowExA pOrigFindWindowExA = nullptr;
}

namespace Logger
{
    enum class Level { INFO, SUCCESS, WARNING, EX, DEBUG };

    HANDLE hConsole = nullptr;
    CRITICAL_SECTION csConsoleLock;

    bool Initialize()
    {
        if (!AllocConsole()) return false;

        FILE* dummy;
        freopen_s(&dummy, "CONOUT$", "w", stdout);
        freopen_s(&dummy, "CONIN$", "r", stdin);

        hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        SetConsoleTitleA("Anti-Cheat Protection System v2.0");

        COORD bufferSize = { 140, 600 };
        SetConsoleScreenBufferSize(hConsole, bufferSize);

        InitializeCriticalSection(&csConsoleLock);
        return true;
    }

    void Log(Level level, const char* format, ...)
    {
        if (!hConsole || State::bShuttingDown) return;

        EnterCriticalSection(&csConsoleLock);

        SYSTEMTIME st;
        GetLocalTime(&st);

        const char* levelStr;
        WORD color;

        switch (level)
        {
        case Level::SUCCESS:
            levelStr = "[OK]  ";
            color = FOREGROUND_GREEN | FOREGROUND_INTENSITY;
            break;
        case Level::WARNING:
            levelStr = "[WARN]";
            color = FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_INTENSITY;
            break;
        case Level::EX:
            levelStr = "[ERR] ";
            color = FOREGROUND_RED | FOREGROUND_INTENSITY;
            break;
        case Level::DEBUG:
            levelStr = "[DBG] ";
            color = FOREGROUND_BLUE | FOREGROUND_GREEN | FOREGROUND_INTENSITY;
            break;
        default:
            levelStr = "[INFO]";
            color = FOREGROUND_BLUE | FOREGROUND_INTENSITY;
        }

        printf("[%02d:%02d:%02d] ", st.wHour, st.wMinute, st.wSecond);

        SetConsoleTextAttribute(hConsole, color);
        printf("%s ", levelStr);
        SetConsoleTextAttribute(hConsole, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);

        va_list args;
        va_start(args, format);
        vprintf(format, args);
        va_end(args);

        printf("\n");

        LeaveCriticalSection(&csConsoleLock);
    }

    void Cleanup()
    {
        DeleteCriticalSection(&csConsoleLock);
    }
}

namespace Memory
{
    bool IsValidAddress(LPVOID address)
    {
        if (!address) return false;

        MEMORY_BASIC_INFORMATION mbi;
        if (VirtualQuery(address, &mbi, sizeof(mbi)) == 0)
            return false;

        return (mbi.State == MEM_COMMIT) &&
            (mbi.Protect & (PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_READWRITE));
    }

    bool SafePatch(LPVOID address, LPCVOID data, SIZE_T size, const char* name)
    {
        if (!IsValidAddress(address))
        {
            Logger::Log(Logger::Level::DEBUG, "Invalid address for %s", name);
            return false;
        }

        DWORD oldProtect;
        if (!VirtualProtect(address, size, PAGE_EXECUTE_READWRITE, &oldProtect))
        {
            Logger::Log(Logger::Level::DEBUG, "VirtualProtect failed for %s", name);
            return false;
        }

        memcpy(address, data, size);
        FlushInstructionCache(GetCurrentProcess(), address, size);
        VirtualProtect(address, size, oldProtect, &oldProtect);

        Logger::Log(Logger::Level::SUCCESS, "Patched %s at 0x%p", name, address);
        return true;
    }

    bool SafeRead(LPVOID address, LPVOID buffer, SIZE_T size)
    {
        __try
        {
            if (!IsValidAddress(address)) return false;
            memcpy(buffer, address, size);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            return false;
        }
    }
}

namespace HookEngine
{
    struct Trampoline
    {
        BYTE code[64];
        SIZE_T size;
    };

    bool InstallInlineHook(LPVOID target, LPVOID detour, LPVOID* original, const char* name)
    {
        if (!Memory::IsValidAddress(target))
        {
            Logger::Log(Logger::Level::DEBUG, "Invalid target for %s", name);
            return false;
        }
        DWORD oldProtect;
        if (!VirtualProtect(target, 32, PAGE_EXECUTE_READWRITE, &oldProtect))
        {
            Logger::Log(Logger::Level::DEBUG, "Protection change failed for %s", name);
            return false;
        }
        BYTE* trampoline = (BYTE*)VirtualAlloc(NULL, 64, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        if (!trampoline)
        {
            VirtualProtect(target, 32, oldProtect, &oldProtect);
            Logger::Log(Logger::Level::DEBUG, "Trampoline allocation failed for %s", name);
            return false;
        }
        SIZE_T copySize = 5;
        BYTE* src = (BYTE*)target;
        if (src[0] == 0xE9 || src[0] == 0xE8)
            copySize = 5;
        else if (src[0] == 0xFF && src[1] == 0x25)
            copySize = 6;

        memcpy(trampoline, target, copySize);
        trampoline[copySize] = 0xE9;
        *(DWORD*)(trampoline + copySize + 1) = ((DWORD)target + copySize) - ((DWORD)trampoline + copySize + 5);
        FlushInstructionCache(GetCurrentProcess(), trampoline, 64);
        src[0] = 0xE9;
        *(DWORD*)(src + 1) = (DWORD)detour - (DWORD)target - 5;
        for (SIZE_T i = 5; i < copySize; i++)
            src[i] = 0x90;

        FlushInstructionCache(GetCurrentProcess(), target, 32);
        VirtualProtect(target, 32, oldProtect, &oldProtect);

        if (original)
            *original = trampoline;

        Logger::Log(Logger::Level::SUCCESS, "Hooked %s", name);
        return true;
    }

    bool PatchCall(DWORD callAddr, LPVOID newFunc, const char* name)
    {
        LPVOID address = (LPVOID)callAddr;
        if (!Memory::IsValidAddress(address))
        {
            Logger::Log(Logger::Level::EX, "Invalid call address for %s", name);
            return false;
        }

        BYTE opcode;
        if (!Memory::SafeRead(address, &opcode, 1) || opcode != 0xE8)
        {
            Logger::Log(Logger::Level::WARNING, "Not a CALL at %s (opcode: 0x%02X)", name, opcode);
            return false;
        }

        DWORD newOffset = (DWORD)newFunc - (callAddr + 5);
        return Memory::SafePatch((LPVOID)(callAddr + 1), &newOffset, sizeof(DWORD), name);
    }

    bool InstallIATHook(LPCSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* origFunc)
    {
        HMODULE hModule = GetModuleHandle(NULL);
        if (!hModule) return false;

        PIMAGE_DOS_HEADER dosHeader = (PIMAGE_DOS_HEADER)hModule;
        if (dosHeader->e_magic != IMAGE_DOS_SIGNATURE) return false;

        PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)((BYTE*)hModule + dosHeader->e_lfanew);
        if (ntHeaders->Signature != IMAGE_NT_SIGNATURE) return false;

        PIMAGE_IMPORT_DESCRIPTOR importDesc = (PIMAGE_IMPORT_DESCRIPTOR)((BYTE*)hModule +
            ntHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);

        while (importDesc->Name)
        {
            char* modName = (char*)((BYTE*)hModule + importDesc->Name);
            if (_stricmp(modName, moduleName) == 0)
            {
                HMODULE targetMod = GetModuleHandleA(moduleName);
                if (!targetMod) break;

                LPVOID origFuncAddr = GetProcAddress(targetMod, funcName);
                if (!origFuncAddr) break;

                PIMAGE_THUNK_DATA thunk = (PIMAGE_THUNK_DATA)((BYTE*)hModule + importDesc->FirstThunk);

                while (thunk->u1.Function)
                {
                    if (thunk->u1.Function == (DWORD_PTR)origFuncAddr)
                    {
                        DWORD oldProtect;
                        VirtualProtect(&thunk->u1.Function, sizeof(LPVOID), PAGE_READWRITE, &oldProtect);

                        if (origFunc)
                            *origFunc = (LPVOID)thunk->u1.Function;

                        thunk->u1.Function = (DWORD_PTR)hookFunc;
                        VirtualProtect(&thunk->u1.Function, sizeof(LPVOID), oldProtect, &oldProtect);

                        Logger::Log(Logger::Level::SUCCESS, "IAT hook: %s!%s", moduleName, funcName);
                        return true;
                    }
                    thunk++;
                }
            }
            importDesc++;
        }

        return false;
    }
}
namespace Handlers
{
    int __stdcall CB_L_Handler(unsigned int* p1, void* p2)
    {
        return 0; 
    }

    BOOL __fastcall ValidationHandler(LPVOID pThis, LPVOID edx)
    {
        return TRUE;
    }

    BOOL __fastcall PollHandler(LPVOID pThis, LPVOID edx)
    {
        return TRUE;
    }

    BOOL __fastcall AntiCheatHandler(LPVOID pThis, LPVOID edx)
    {
        return TRUE;
    }

    DWORD WINAPI GetTickCountHandler()
    {
        return Prototypes::pOrigGetTickCount ? Prototypes::pOrigGetTickCount() : 0;
    }

    HWND WINAPI FindWindowExHandler(HWND p1, HWND p2, LPCSTR p3, LPCSTR p4)
    {
        return NULL;
    }
}
namespace Protection
{
    namespace CB
    {
        bool PatchLogLevel(DWORD newValue)
        {
            if (!State::hCBModule)
            {
                Logger::Log(Logger::Level::WARNING, "CB module not loaded");
                return false;
            }

            DWORD* logLevel = (DWORD*)((DWORD)State::hCBModule + Config::CB_LOG_OFFSET);
            return Memory::SafePatch(logLevel, &newValue, sizeof(DWORD), "CB Log Level");
        }

        bool HookCBL()
        {
            EnterCriticalSection(&State::csModuleLock);

            if (State::bCBHooked)
            {
                LeaveCriticalSection(&State::csModuleLock);
                return true;
            }

            if (!State::hCBModule)
            {
                LeaveCriticalSection(&State::csModuleLock);
                return false;
            }

            LPVOID cbFunc = GetProcAddress(State::hCBModule, "?_CB_L_@@YGHPAIPAUCB_X_DATA@CHEAT_BLOCKER@@@Z");
            if (!cbFunc)
            {
                Logger::Log(Logger::Level::WARNING, "CB_L function not found");
                LeaveCriticalSection(&State::csModuleLock);
                return false;
            }

            if (HookEngine::InstallInlineHook(cbFunc, Handlers::CB_L_Handler,
                (LPVOID*)&Prototypes::pOrigCB_L, "CB_L"))
            {
                State::bCBHooked = true;
                LeaveCriticalSection(&State::csModuleLock);
                return true;
            }

            LeaveCriticalSection(&State::csModuleLock);
            return false;
        }

        void Initialize()
        {
            Logger::Log(Logger::Level::INFO, "Initializing CB protection");
            PatchLogLevel(Config::TARGET_LOG_LEVEL);
            Sleep(100);
            HookCBL();
        }

        bool Verify()
        {
            if (!State::hCBModule) return true;

            DWORD* logLevel = (DWORD*)((DWORD)State::hCBModule + Config::CB_LOG_OFFSET);
            DWORD current;

            if (Memory::SafeRead(logLevel, &current, sizeof(DWORD)))
            {
                if (current != Config::TARGET_LOG_LEVEL)
                {
                    Logger::Log(Logger::Level::WARNING, "CB log modified, restoring");
                    return PatchLogLevel(Config::TARGET_LOG_LEVEL);
                }
                return true;
            }

            return false;
        }
    }

    namespace Gameforge
    {
        struct PatchInfo
        {
            DWORD offset;
            LPVOID handler;
            const char* name;
        };

        PatchInfo patches[] = {
            { Config::GF_VALIDATION_OFFSET, Handlers::ValidationHandler, "GF Validation" },
            { Config::GF_POOL_OFFSET, Handlers::PollHandler, "GF Poll" },
            { Config::GF_CHECKER_OFFSET, Handlers::AntiCheatHandler, "GF AntiCheat" }
        };

        void InstallHooks()
        {
            if (!State::hGFModule)
            {
                Logger::Log(Logger::Level::WARNING, "GF module not loaded");
                return;
            }

            Logger::Log(Logger::Level::INFO, "Installing GF hooks");

            for (auto& patch : patches)
            {
                HookEngine::PatchCall((DWORD)State::hGFModule + patch.offset,
                    patch.handler, patch.name);
                Sleep(50);
            }
        }

        bool Verify()
        {
            if (!State::hGFModule) return true;

            bool needsRepair = false;

            for (auto& patch : patches)
            {
                DWORD callAddr = (DWORD)State::hGFModule + patch.offset;
                BYTE opcode;

                if (Memory::SafeRead((LPVOID)callAddr, &opcode, 1) && opcode == 0xE8)
                {
                    DWORD currentOffset, expectedOffset;
                    if (Memory::SafeRead((LPVOID)(callAddr + 1), &currentOffset, sizeof(DWORD)))
                    {
                        expectedOffset = (DWORD)patch.handler - (callAddr + 5);
                        if (currentOffset != expectedOffset)
                        {
                            Logger::Log(Logger::Level::WARNING, "%s hook removed, reinstalling", patch.name);
                            HookEngine::PatchCall(callAddr, patch.handler, patch.name);
                            needsRepair = true;
                            Sleep(50);
                        }
                    }
                }
            }

            return !needsRepair;
        }
    }
}
HMODULE WINAPI LoadLibraryHook(LPCSTR lpFileName)
{
    HMODULE hMod = Prototypes::pOrigLoadLibrary ?
        Prototypes::pOrigLoadLibrary(lpFileName) : LoadLibraryA(lpFileName);

    if (hMod && lpFileName)
    {
        const char* fileName = strrchr(lpFileName, '\\');
        fileName = fileName ? fileName + 1 : lpFileName;

        Logger::Log(Logger::Level::INFO, "Module loaded: %s", fileName);

        if (_stricmp(fileName, "cb.cbm") == 0)
        {
            EnterCriticalSection(&State::csModuleLock);
            if (!State::hCBModule)
            {
                State::hCBModule = hMod;
                Logger::Log(Logger::Level::SUCCESS, "CB module detected");
                LeaveCriticalSection(&State::csModuleLock);
                Sleep(150);
                Protection::CB::Initialize();
            }
            else
            {
                LeaveCriticalSection(&State::csModuleLock);
            }
        }
        else if (_stricmp(fileName, "gameforge_api.dll") == 0)
        {
            EnterCriticalSection(&State::csModuleLock);
            if (!State::hGFModule)
            {
                State::hGFModule = hMod;
                Logger::Log(Logger::Level::SUCCESS, "GF module detected");
                LeaveCriticalSection(&State::csModuleLock);
                Sleep(150);
                Protection::Gameforge::InstallHooks();
            }
            else
            {
                LeaveCriticalSection(&State::csModuleLock);
            }
        }
    }

    return hMod;
}

DWORD WINAPI WatcherThread(LPVOID param)
{
    Logger::Log(Logger::Level::INFO, "Watcher thread started");

    DWORD tickCount = 0;

    while (!State::bShuttingDown)
    {
        Sleep(Config::WATCHER_INTERVAL_MS);
        tickCount++;
        if (tickCount % Config::CB_CHECK_INTERVAL == 0)
        {
            if (Protection::CB::Verify())
                Logger::Log(Logger::Level::DEBUG, "CB: OK");
        }
        if (tickCount % Config::GF_CHECK_INTERVAL == 0)
        {
            if (Protection::Gameforge::Verify())
                Logger::Log(Logger::Level::DEBUG, "GF: OK");
        }
    }

    Logger::Log(Logger::Level::INFO, "Watcher thread terminated");
    return 0;
}

void Initialize()
{
    __try
    {
        InitializeCriticalSection(&State::csModuleLock);

        if (!Logger::Initialize())
        {
            MessageBoxA(NULL, "Console initialization failed", "Error", MB_ICONERROR);
            return;
        }
        Logger::Log(Logger::Level::INFO, "Process ID: %d", GetCurrentProcessId());
        HMODULE hKernel32 = GetModuleHandleA("kernel32.dll");
        if (hKernel32)
        {
            LPVOID loadLib = GetProcAddress(hKernel32, "LoadLibraryA");
            if (loadLib)
            {
                HookEngine::InstallInlineHook(loadLib, LoadLibraryHook,
                    (LPVOID*)&Prototypes::pOrigLoadLibrary, "LoadLibraryA");
            }
        }
        Logger::Log(Logger::Level::INFO, "Scanning loaded modules");
        HMODULE hCB = GetModuleHandleA("cb.cbm");
        if (hCB)
        {
            State::hCBModule = hCB;
            Logger::Log(Logger::Level::SUCCESS, "CB module found");
            Sleep(150);
            Protection::CB::Initialize();
        }
        HMODULE hGF = GetModuleHandleA("gameforge_api.dll");
        if (hGF)
        {
            State::hGFModule = hGF;
            Logger::Log(Logger::Level::SUCCESS, "GF module found");
            Sleep(150);
            Protection::Gameforge::InstallHooks();
        }
        HANDLE hThread = CreateThread(NULL, 0, WatcherThread, NULL, 0, NULL);
        if (hThread)
        {
            Logger::Log(Logger::Level::SUCCESS, "Protection system active");
            CloseHandle(hThread);
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        MessageBoxA(NULL, "Critical initialization error", "Fatal Error", MB_ICONERROR);
    }
}
