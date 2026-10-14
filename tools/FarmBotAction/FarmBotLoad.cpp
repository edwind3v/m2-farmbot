#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#pragma comment(lib, "user32.lib")
#include <cstdio>
#include <cstring>
#include <string>

// x86 helper: load FarmBotAction.dll into the user's own 32-bit Metin2 process.

static void WriteConfig(const std::string& pipe, const std::string& token)
{
    char path[MAX_PATH];
    GetTempPathA(MAX_PATH, path);
    strcat_s(path, "FarmBotAction.cfg");
    FILE* f = nullptr;
    if (fopen_s(&f, path, "w") == 0 && f)
    {
        fprintf(f, "pipe=%s\ntoken=%s\n", pipe.c_str(), token.c_str());
        fclose(f);
    }
}

static bool Inject(HANDLE process, const char* dllPath)
{
    const size_t bytes = strlen(dllPath) + 1;
    void* remote = VirtualAllocEx(process, nullptr, bytes, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    if (!remote)
        return false;
    if (!WriteProcessMemory(process, remote, dllPath, bytes, nullptr))
        return false;
    HMODULE kernel = GetModuleHandleA("kernel32.dll");
    FARPROC load = GetProcAddress(kernel, "LoadLibraryA");
    HANDLE thread = CreateRemoteThread(process, nullptr, 0, (LPTHREAD_START_ROUTINE)load, remote, 0, nullptr);
    if (!thread)
        return false;
    WaitForSingleObject(thread, 15000);
    CloseHandle(thread);
    return true;
}

static BOOL CALLBACK MatchPidWindow(HWND hwnd, LPARAM ctx)
{
    DWORD* found = (DWORD*)ctx;
    DWORD pid = 0;
    GetWindowThreadProcessId(hwnd, &pid);
    if (pid != found[0] || !IsWindowVisible(hwnd))
        return TRUE;
    char title[128];
    if (GetWindowTextA(hwnd, title, 128) <= 0)
        return TRUE;
    found[1] = 1;
    return FALSE;
}

static bool WaitForWindow(DWORD pid, DWORD timeoutMs)
{
    const DWORD start = GetTickCount();
    while (GetTickCount() - start < timeoutMs)
    {
        DWORD found[2] = { pid, 0 };
        EnumWindows(MatchPidWindow, (LPARAM)found);
        if (found[1])
            return true;
        Sleep(200);
    }
    return false;
}

static const DWORD kAccess =
    PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION |
    PROCESS_VM_WRITE | PROCESS_VM_READ;

int main(int argc, char** argv)
{
    std::string mode;
    DWORD pid = 0;
    std::string exe;
    std::string workdir;
    std::string dll;
    std::string token;
    std::string pipe = "FarmBotAction";

    for (int i = 1; i < argc; ++i)
    {
        if (strcmp(argv[i], "--attach") == 0 && i + 1 < argc)
        {
            mode = "attach";
            pid = (DWORD)atoi(argv[++i]);
        }
        else if (strcmp(argv[i], "--launch") == 0 && i + 1 < argc)
        {
            mode = "launch";
            exe = argv[++i];
        }
        else if (strcmp(argv[i], "--dll") == 0 && i + 1 < argc)
            dll = argv[++i];
        else if (strcmp(argv[i], "--token") == 0 && i + 1 < argc)
            token = argv[++i];
        else if (strcmp(argv[i], "--pipe") == 0 && i + 1 < argc)
            pipe = argv[++i];
        else if (strcmp(argv[i], "--workdir") == 0 && i + 1 < argc)
            workdir = argv[++i];
    }

    if (dll.empty() || token.empty() || mode.empty())
    {
        printf("usage: FarmBotLoad --attach PID|--launch EXE --dll PATH --token HEX --pipe NAME\n");
        return 2;
    }

    char fullDll[MAX_PATH];
    if (!GetFullPathNameA(dll.c_str(), MAX_PATH, fullDll, nullptr))
    {
        printf("dll path failed\n");
        return 3;
    }
    if (GetFileAttributesA(fullDll) == INVALID_FILE_ATTRIBUTES)
    {
        printf("dll missing\n");
        return 3;
    }

    char injectDll[MAX_PATH];
    char tempDir[MAX_PATH];
    GetTempPathA(MAX_PATH, tempDir);
    _snprintf_s(injectDll, _TRUNCATE, "%sFarmBotAction_%lu_%lu.dll", tempDir, GetCurrentProcessId(), GetTickCount());
    if (!CopyFileA(fullDll, injectDll, FALSE))
        strcpy_s(injectDll, fullDll);

    WriteConfig(pipe, token);

    if (mode == "launch")
    {
        STARTUPINFOA si = {};
        PROCESS_INFORMATION pi = {};
        si.cb = sizeof(si);
        std::string cmd = "\"" + exe + "\"";
        std::string dir = workdir.empty()
            ? exe.substr(0, exe.find_last_of("\\/"))
            : workdir;
        char cmdBuf[1024];
        _snprintf_s(cmdBuf, _TRUNCATE, "%s", cmd.c_str());
        if (!CreateProcessA(exe.c_str(), cmdBuf, nullptr, nullptr, FALSE, 0, nullptr,
                            dir.empty() ? nullptr : dir.c_str(), &si, &pi))
        {
            printf("create process failed %lu\n", GetLastError());
            return 4;
        }
        CloseHandle(pi.hThread);
        if (!WaitForWindow(pi.dwProcessId, 30000))
        {
            printf("client window did not appear; injecting anyway\n");
            fflush(stdout);
        }
        if (!Inject(pi.hProcess, injectDll))
        {
            printf("inject failed %lu\n", GetLastError());
            CloseHandle(pi.hProcess);
            return 5;
        }
        CloseHandle(pi.hProcess);
        printf("launched pid=%lu\n", pi.dwProcessId);
        fflush(stdout);
        return 0;
    }

    HANDLE process = OpenProcess(kAccess, FALSE, pid);
    if (!process)
    {
        printf("open process failed %lu\n", GetLastError());
        return 4;
    }
    if (!Inject(process, injectDll))
    {
        printf("inject failed %lu\n", GetLastError());
        CloseHandle(process);
        return 5;
    }
    CloseHandle(process);
    printf("attached pid=%lu\n", pid);
    fflush(stdout);
    return 0;
}
