#include <ImageHlp.h>

#define ed3d9		/*d3d9.dll*/XorStr<0xEC,9,0x4C39F2EF>("\x88\xDE\x8A\xD6\xDE\x95\x9E\x9F"+0x4C39F2EF).s
#define ei3GfxDx	/*i3GfxDx.dll*/XorStr<0x22,12,0x1D26D95A>("\x4B\x10\x63\x43\x5E\x63\x50\x07\x4E\x47\x40"+0x1D26D95A).s
#define ei3MathDx	/*i3MathDx.dll*/XorStr<0x95,13,0x9F9CD917>("\xFC\xA5\xDA\xF9\xED\xF2\xDF\xE4\xB3\xFA\xF3\xCC"+0x9F9CD917).s
#define ei3SceneDx	/*i3SceneDx.dll*/XorStr<0x23,14,0x680A62FA>("\x4A\x17\x76\x45\x42\x46\x4C\x6E\x53\x02\x49\x42\x43"+0x680A62FA).s
#define eWR		/*WarRock.exe*/XorStr<0x0B,12,0x4288E8DE>("\x5C\x6D\x7F\x5C\x60\x73\x7A\x3C\x76\x6C\x70"+0x4288E8DE).s
#define ePB		/*PointBlank.exe*/XorStr<0xFA,15,0x0CDB20D9>("\xAA\x94\x95\x93\x8A\xBD\x6C\x60\x6C\x68\x2A\x60\x7E\x62"+0x0CDB20D9).s
#define eKernel32	/*kernel32*/XorStr<0x41,9,0x1533420A>("\x2A\x27\x31\x2A\x20\x2A\x74\x7A"+0x1533420A).s
#define eNtdll		/*ntdll.dll*/XorStr<0x5D,10,0x5C5E14EA>("\x33\x2A\x3B\x0C\x0D\x4C\x07\x08\x09"+0x5C5E14EA).s
#define ed3d9		/*d3d9.dll*/XorStr<0xEC,9,0x4C39F2EF>("\x88\xDE\x8A\xD6\xDE\x95\x9E\x9F"+0x4C39F2EF).s
#define ed3d9_29	/*d3dx9_29.dll*/XorStr<0xEA,13,0xF4BEC287>("\x8E\xD8\x88\x95\xD7\xB0\xC2\xC8\xDC\x97\x98\x99"+0xF4BEC287).s
#define eCreateFont	/*D3DXCreateFontA*/XorStr<0x3C,16,0xE86E53B5>("\x78\x0E\x7A\x67\x03\x33\x27\x22\x30\x20\x00\x28\x26\x3D\x0B"+0xE86E53B5).s
#define eMenuCF		/*[RAML] Crossfire Hack*/XorStr<0xE4,22,0xD895417E>("\xBF\xB7\xA7\xAA\xA4\xB4\xCA\xA8\x9E\x82\x9D\x9C\x96\x98\x80\x96\xD4\xBD\x97\x94\x93"+0xD895417E).s
#define eSite		/*www.ramleague.net*/XorStr<0xC9,18,0x79C93728>("\xBE\xBD\xBC\xE2\xBF\xAF\xA2\xBC\xB4\xB3\xB4\xA1\xB0\xF8\xB9\xBD\xAD"+0x79C93728).s
#define eWarning	/*Unknown GM is trying to fuck you!*/XorStr<0xB3,34,0x714C0278>("\xE6\xDA\xDE\xD8\xD8\xCF\xD7\x9A\xFC\xF1\x9D\xD7\xCC\xE0\xB5\xB0\xBA\xAD\xAB\xA1\xE7\xBC\xA6\xEA\xAD\xB9\xAE\xA5\xEF\xA9\xBE\xA7\xF2"+0x714C0278).s

#ifndef UNICODE
#define UNICODE
#endif

#define NT_SUCCESS(x) ((x) >= 0)
#define STATUS_INFO_LENGTH_MISMATCH 0xc0000004

#define SystemHandleInformation 16
#define ObjectBasicInformation 0
#define ObjectNameInformation 1
#define ObjectTypeInformation 2

HANDLE _out = nullptr, _old_out = nullptr;
HANDLE _err = nullptr, _old_err = nullptr;
HANDLE _in = nullptr, _old_in = nullptr;

static int num = 0;

#define SUSPEND_BLACKHOLE 2000
#define Message2 "Suspend Dedected" 
#define TitleName "Black Hole"

typedef struct _LDR_MODULE
{
    LIST_ENTRY InLoadOrderModuleList;
    LIST_ENTRY InMemoryOrderModuleList;
    LIST_ENTRY InInitializationOrderModuleList;
    PVOID BaseAddress;
    PVOID EntryPoint;
    ULONG SizeOfImage;
}LDR_MODULE, * PLDR_MODULE;

typedef NTSTATUS(NTAPI* _NtQuerySystemInformation)(
    ULONG SystemInformationClass,
    PVOID SystemInformation,
    ULONG SystemInformationLength,
    PULONG ReturnLength
    );

typedef NTSTATUS(NTAPI* _NtDuplicateObject)(
    HANDLE SourceProcessHandle,
    HANDLE SourceHandle,
    HANDLE TargetProcessHandle,
    PHANDLE TargetHandle,
    ACCESS_MASK DesiredAccess,
    ULONG Attributes,
    ULONG Options
    );

typedef struct _LDR_DATA_TABLE_ENTRY {
    PVOID Reserved1[2];
    LIST_ENTRY InLoadOrderLinks;
    PVOID Reserved2[2];
    PVOID DllBase;
    PVOID EntryPoint;
    PVOID Reserved3;
    BYTE Reserved4[8];
    PVOID Reserved5[3];
    union {
        ULONG CheckSum;
        PVOID Reserved6;
    } DUMMYUNIONNAME;
    ULONG TimeDateStamp;
} LDR_DATA_TABLE_ENTRY, * PLDR_DATA_TABLE_ENTRY;

typedef NTSTATUS(NTAPI* _NtQueryObject)(
    HANDLE ObjectHandle,
    ULONG ObjectInformationClass,
    PVOID ObjectInformation,
    ULONG ObjectInformationLength,
    PULONG ReturnLength
    );

typedef struct _SYSTEM_HANDLE
{
    ULONG ProcessId;
    BYTE ObjectTypeNumber;
    BYTE Flags;
    USHORT Handle;
    PVOID Object;
    ACCESS_MASK GrantedAccess;
} SYSTEM_HANDLE, * PSYSTEM_HANDLE;

typedef struct _SYSTEM_HANDLE_INFORMATION
{
    ULONG HandleCount;
    SYSTEM_HANDLE Handles[1];
} SYSTEM_HANDLE_INFORMATION, * PSYSTEM_HANDLE_INFORMATION;

typedef enum _POOL_TYPE
{
    NonPagedPool,
    PagedPool,
    NonPagedPoolMustSucceed,
    DontUseThisType,
    NonPagedPoolCacheAligned,
    PagedPoolCacheAligned,
    NonPagedPoolCacheAlignedMustS
} POOL_TYPE, * PPOOL_TYPE;

typedef struct _OBJECT_TYPE_INFORMATION
{
    //UNICODE_STRING Name;
    ULONG TotalNumberOfObjects;
    ULONG TotalNumberOfHandles;
    ULONG TotalPagedPoolUsage;
    ULONG TotalNonPagedPoolUsage;
    ULONG TotalNamePoolUsage;
    ULONG TotalHandleTableUsage;
    ULONG HighWaterNumberOfObjects;
    ULONG HighWaterNumberOfHandles;
    ULONG HighWaterPagedPoolUsage;
    ULONG HighWaterNonPagedPoolUsage;
    ULONG HighWaterNamePoolUsage;
    ULONG HighWaterHandleTableUsage;
    ULONG InvalidAttributes;
    GENERIC_MAPPING GenericMapping;
    ULONG ValidAccess;
    BOOLEAN SecurityRequired;
    BOOLEAN MaintainHandleCount;
    USHORT MaintainTypeList;
    POOL_TYPE PoolType;
    ULONG PagedPoolUsage;
    ULONG NonPagedPoolUsage;
} OBJECT_TYPE_INFORMATION, * POBJECT_TYPE_INFORMATION;

typedef struct _SYSTEM_PROCESS_INFO
{
    ULONG                   NextEntryOffset;
    ULONG                   NumberOfThreads;
    LARGE_INTEGER           Reserved[3];
    LARGE_INTEGER           CreateTime;
    LARGE_INTEGER           UserTime;
    LARGE_INTEGER           KernelTime;
    //UNICODE_STRING          ImageName;
    ULONG                   BasePriority;
    HANDLE                  ProcessId;
    HANDLE                  InheritedFromProcessId;
}SYSTEM_PROCESS_INFO, * PSYSTEM_PROCESS_INFO;

typedef struct _PEB_LDR_DATA
{
    ULONG Length;
    BOOLEAN Initialized;
    PVOID SsHandle;
    LIST_ENTRY InLoadOrderModuleList;
    LIST_ENTRY InMemoryOrderModuleList;
    LIST_ENTRY InInitializationOrderModuleList;
}PEB_LDR_DATA, * PPEB_LDR_DATA;

typedef struct _PEB
{
    BYTE Reserved1[2];
    BYTE BeingDebugged;
    BYTE Reserved2[1];
    PVOID Reserved3[2];
    PPEB_LDR_DATA Ldr;
}PEB, * PPEB;
typedef UINT(__stdcall* threadFunc_t)(void*);
typedef struct _UNICODE_STRING
{
    USHORT Length;
    USHORT MaximumLength;
    PWSTR Buffer;
} UNICODE_STRING, * PUNICODE_STRING;
typedef struct _ModuleInfoNode
{
    LIST_ENTRY LoadOrder;
    LIST_ENTRY InitOrder;
    LIST_ENTRY MemoryOrder;
    HMODULE baseAddress;
    unsigned long entryPoint;
    unsigned int size;
    UNICODE_STRING fullPath;
    UNICODE_STRING name;
    unsigned long flags;
    unsigned short LoadCount;
    unsigned short TlsIndex;
    LIST_ENTRY HashTable;
    unsigned long timestamp;
} ModuleInfoNode, * pModuleInfoNode;
typedef struct _ProcessModuleInfo
{
    unsigned int size;
    unsigned int initialized;
    HANDLE SsHandle;
    LIST_ENTRY LoadOrder;
    LIST_ENTRY InitOrder;
    LIST_ENTRY MemoryOrder;
} ProcessModuleInfo, * pProcessModuleInfo;
#define UNLINK(x) (x).Blink->Flink = (x).Flink; \
	(x).Flink->Blink = (x).Blink;
using _RtlCreateUserThread = NTSTATUS(NTAPI*)(
    HANDLE ProcessHandle,
    PSECURITY_DESCRIPTOR SecurityDescriptor,
    BOOLEAN CreateSuspend,
    ULONG StackZeroBits,
    PULONG StackReserved,
    PULONG StackCommit,
    void* StartAddress,
    void* StartParameter,
    PHANDLE ThreadHandle,
    void* ClientID
    );

namespace AntiCheat
{
    // ============================== Thread ============================== //
    void Thread(void* Thread, HMODULE& hModule)
    {
        HMODULE NT_DLL = LoadLibrary("ntdll");
        uintptr_t SpoofedAddress = NULL;
        int DefaultThreadSize = 1000;
        srand(time(NULL));

        for (int i = 1; i < 4; i++) {
            SpoofedAddress |= (rand() & 0xFF) << i * 8;
            SpoofedAddress |= (rand() & 0xFF) << i * 8;
            SpoofedAddress |= (rand() & 0xFF) << i * 8;
        }
        while (SpoofedAddress > 0x7FFFFFFF) {
            SpoofedAddress -= 0x1000;
        }
        VirtualProtect((void*)SpoofedAddress, DefaultThreadSize, PAGE_EXECUTE_READWRITE, NULL);

        CONTEXT tContext;
        HANDLE  pHandle = nullptr;

        _RtlCreateUserThread KeThread = (_RtlCreateUserThread)(GetProcAddress(GetModuleHandle("ntdll"), "RtlCreateUserThread"));
        KeThread(GetCurrentProcess(), nullptr, TRUE, NULL, NULL, NULL, (PTHREAD_START_ROUTINE)SpoofedAddress, hModule, &pHandle, NULL); //create a thread & stop init everything

        tContext.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;
        GetThreadContext(pHandle, &tContext);

#ifdef _WIN32
        tContext.Eax = (ULONG32)Thread;
#else
        tContext.Rcx = (ULONG64)Thread;
#endif

        tContext.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;

        SetThreadContext(pHandle, &tContext);

        ResumeThread(pHandle);
    }

    // ============================== Header Old ============================== //

    /*void EraseHeaders(HINSTANCE hModule)
    {
        PIMAGE_DOS_HEADER pDoH;
        PIMAGE_NT_HEADERS pNtH;
        DWORD i, ersize, protect;

        if (!hModule) return;

        pDoH = (PIMAGE_DOS_HEADER)(hModule);
        pNtH = (PIMAGE_NT_HEADERS)((LONG)hModule + ((PIMAGE_DOS_HEADER)hModule)->e_lfanew);

        ersize = sizeof(IMAGE_DOS_HEADER);
        if (VirtualProtect(pDoH, ersize, PAGE_READWRITE, &protect))
        {
            for (i = 0; i < ersize; i++)
                *(BYTE*)((BYTE*)pDoH + i) = 0;
        }

        ersize = sizeof(IMAGE_NT_HEADERS);
        if (pNtH && VirtualProtect(pNtH, ersize, PAGE_READWRITE, &protect))
        {
            for (i = 0; i < ersize; i++)
                *(BYTE*)((BYTE*)pNtH + i) = 0;
        }
        return;
    }*/
    // ============================== Header New ============================== //
    void EraseHeaders(HMODULE hModule)
    {
        if (!hModule) return;

        PIMAGE_DOS_HEADER pDoH = (PIMAGE_DOS_HEADER)hModule;
        PIMAGE_NT_HEADERS pNtH = (PIMAGE_NT_HEADERS)((BYTE*)hModule + pDoH->e_lfanew);
        DWORD protect;
        DWORD dosHeaderSize = sizeof(IMAGE_DOS_HEADER);
        DWORD ntHeaderSize = sizeof(IMAGE_NT_HEADERS);
        if (VirtualProtect(pDoH, dosHeaderSize, PAGE_READWRITE, &protect))
        {
            ZeroMemory(pDoH, dosHeaderSize);
            VirtualProtect(pDoH, dosHeaderSize, protect, &protect);
        }
        if (pNtH && VirtualProtect(pNtH, ntHeaderSize, PAGE_READWRITE, &protect))
        {
            ZeroMemory(pNtH, ntHeaderSize);
            VirtualProtect(pNtH, ntHeaderSize, protect, &protect);
        }
    }

    // ============================== kernel 32 steal ============================== //
    /*typedef UINT(__stdcall* threadFunc_t)(void*);
    HANDLE CreateStealthThread(threadFunc_t pThreadFunc, void* pArgument)
    {
        BYTE* pK32 = (BYTE*)GetModuleHandle(eKernel32);
        BYTE* pPopRet = nullptr;
        DWORD oldProt;

        auto rva2va = [&](DWORD dwVA)
        {
            return (void*)((uintptr_t)pK32 + dwVA);
        };

        void* pWriteTarget = nullptr;

        auto pMz = (IMAGE_DOS_HEADER*)pK32;
        auto pNt = (IMAGE_NT_HEADERS32*)rva2va(pMz->e_lfanew);
        auto pCurSection = (IMAGE_SECTION_HEADER*)((uintptr_t)pNt + sizeof(IMAGE_NT_HEADERS32));

        for (int i = 0; i < pNt->FileHeader.NumberOfSections; ++i)
        {
            if (memcmp(".text", pCurSection->Name, 5) == 0)
            {
                pWriteTarget = (void*)((uintptr_t)rva2va(pCurSection->VirtualAddress) + pCurSection->Misc.VirtualSize - 6);
                break;
            }
            ++pCurSection;
        }

        if (!pWriteTarget) return NULL;

        uint8_t shellcode[] = "\x68\x00\x00\x00\x00\xC2";
        *(threadFunc_t*)(shellcode + 1) = pThreadFunc;

        VirtualProtect(pWriteTarget, 6, PAGE_EXECUTE_READWRITE, &oldProt);
        memcpy(pWriteTarget, shellcode, 6);
        VirtualProtect(pWriteTarget, 6, oldProt, &oldProt);

        return CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)pWriteTarget, pArgument, NULL, nullptr);
    }*/

    // ============================== Hidding ============================== //

    void HideModule(HINSTANCE hModule)
    {
        DWORD dwPEB_LDR_DATA = 0;
        _asm
        {
            pushad;
            pushfd;
            mov eax, fs: [30h]
                mov eax, [eax + 0Ch]
                mov dwPEB_LDR_DATA, eax

                InLoadOrderModuleList :
            mov esi, [eax + 0Ch]
                mov edx, [eax + 10h]

                LoopInLoadOrderModuleList :
                lodsd
                mov esi, eax
                mov ecx, [eax + 18h]
                cmp ecx, hModule
                jne SkipA
                mov ebx, [eax]
                mov ecx, [eax + 4]
                mov[ecx], ebx
                mov[ebx + 4], ecx
                jmp InMemoryOrderModuleList

                SkipA :
            cmp edx, esi
                jne LoopInLoadOrderModuleList

                InMemoryOrderModuleList :
            mov eax, dwPEB_LDR_DATA
                mov esi, [eax + 14h]
                mov edx, [eax + 18h]

                LoopInMemoryOrderModuleList :
                lodsd
                mov esi, eax
                mov ecx, [eax + 10h]
                cmp ecx, hModule
                jne SkipB
                mov ebx, [eax]
                mov ecx, [eax + 4]
                mov[ecx], ebx
                mov[ebx + 4], ecx
                jmp InInitializationOrderModuleList

                SkipB :
            cmp edx, esi
                jne LoopInMemoryOrderModuleList

                InInitializationOrderModuleList :
            mov eax, dwPEB_LDR_DATA
                mov esi, [eax + 1Ch]
                mov edx, [eax + 20h]

                LoopInInitializationOrderModuleList :
                lodsd
                mov esi, eax
                mov ecx, [eax + 08h]
                cmp ecx, hModule
                jne SkipC
                mov ebx, [eax]
                mov ecx, [eax + 4]
                mov[ecx], ebx
                mov[ebx + 4], ecx
                jmp Finished

                SkipC :
            cmp edx, esi
                jne LoopInInitializationOrderModuleList

                Finished :
            popfd;
            popad;
        }
    }

    // ============================== Module 2024 ============================== //

    void Module(HINSTANCE hModule)
    {
        PPEB_LDR_DATA pLdrData = NULL;
        PLIST_ENTRY pModuleListEntry = NULL;
        PLDR_DATA_TABLE_ENTRY pDataTableEntry = NULL;
        __asm
        {
            mov eax, fs: [0x30]
            mov eax, [eax + 0x0C]
            mov pLdrData, eax
        }
        if (pLdrData == NULL)
            return;
        pModuleListEntry = pLdrData->InLoadOrderModuleList.Flink;
        while (pModuleListEntry && pModuleListEntry != &pLdrData->InLoadOrderModuleList)
        {
            pDataTableEntry = CONTAINING_RECORD(pModuleListEntry, LDR_DATA_TABLE_ENTRY, InLoadOrderLinks);

            if (pDataTableEntry->DllBase == hModule)
            {
                pModuleListEntry->Blink->Flink = pModuleListEntry->Flink;
                pModuleListEntry->Flink->Blink = pModuleListEntry->Blink;
                break;
            }
            pModuleListEntry = pModuleListEntry->Flink;
        }
    }

    // ============================== Multi Cheat ============================== //
    bool Gamelımıt(int id)
    {
        std::string mutex_name = "Rex";
        mutex_name.push_back(id);
        HANDLE Mutex = OpenMutexA(MUTEX_ALL_ACCESS, 1, mutex_name.c_str());

        if (!Mutex || WaitForSingleObject(Mutex, 500) == WAIT_ABANDONED)
        {
            CreateMutexA(0, 1, mutex_name.c_str());
            Sleep(INFINITY);
            return true;
        }

        return false;
    }

    // ============================== Memory ============================== //
    void Hidden(HMODULE hMod)
    {
        ProcessModuleInfo* pmInfo;
        ModuleInfoNode* module;

        _asm
        {
            mov eax, fs: [18h]
            mov eax, [eax + 30h]
            mov eax, [eax + 0Ch]
            mov pmInfo, eax
        }

        module = (ModuleInfoNode*)(pmInfo->LoadOrder.Flink);

        while (module->baseAddress && module->baseAddress != hMod) {
            module = (ModuleInfoNode*)(module->LoadOrder.Flink);
        }

        if (!module->baseAddress) {
            // return 0;
        }

        UNLINK(module->LoadOrder);
        UNLINK(module->InitOrder);
        UNLINK(module->MemoryOrder);
        UNLINK(module->HashTable);
        memset(module->fullPath.Buffer, 0, module->fullPath.Length);
        memset(module, 0, sizeof(ModuleInfoNode));

        //return 1;
    }

    // ==============================  NTDLL HIDE ============================== //
    void Anti_Cheat(HINSTANCE hModule)
    {
        PEB* peb;
        LDR_MODULE* ldr;

        peb = (PEB*)__readfsdword(0x30);

        ldr = (LDR_MODULE*)peb->Ldr->InLoadOrderModuleList.Flink;

        while (ldr->BaseAddress != 0)
        {
            if (ldr->BaseAddress == hModule)
            {
                if (ldr->InLoadOrderModuleList.Blink != 0)
                    (ldr->InLoadOrderModuleList.Blink)->Flink = ldr->InLoadOrderModuleList.Flink;

                if (ldr->InLoadOrderModuleList.Blink != 0)
                    (ldr->InLoadOrderModuleList.Flink)->Blink = ldr->InLoadOrderModuleList.Blink;

                if (ldr->InInitializationOrderModuleList.Blink != 0)
                    (ldr->InInitializationOrderModuleList.Blink)->Flink = ldr->InInitializationOrderModuleList.Flink;

                if (ldr->InInitializationOrderModuleList.Flink != 0)
                    (ldr->InInitializationOrderModuleList.Flink)->Blink = ldr->InInitializationOrderModuleList.Blink;

                if (ldr->InMemoryOrderModuleList.Flink != 0)
                    (ldr->InMemoryOrderModuleList.Blink)->Flink = ldr->InMemoryOrderModuleList.Flink;

                if (ldr->InMemoryOrderModuleList.Flink != 0)
                    (ldr->InMemoryOrderModuleList.Flink)->Blink = ldr->InMemoryOrderModuleList.Blink;
            }
            ldr = (LDR_MODULE*)ldr->InLoadOrderModuleList.Flink;
        }
    }
    // ============================== Reneme DLL ============================== //
    void NihonX(void)
    {
        HMODULE GameModule = GetModuleHandleA("Ezgi.dll");
        if (GameModule == nullptr)
        {
            int result = MessageBox(nullptr, TEXT("Neden Yaptın Simdi Bunu Cezalandırmam Gerekiyor Seni Simdi :)"), nullptr, MB_ICONWARNING | MB_TOPMOST | MB_SETFOREGROUND | MB_OKCANCEL | MB_DEFBUTTON1);

            if (result == IDOK)
            {
                ShellExecuteA(0, "open", "https://discord.gg/wMtQ62jD", "", "", SW_SHOWNORMAL);
            }
            ExitProcess(0);
        }
    }
    // ============================== Suspend Dedected DLL ============================== //
    void Suspend() {
        DWORD lastTickCount = GetTickCount();
        while (true) {
            DWORD currentTickCount = GetTickCount();
            DWORD elapsedTime = currentTickCount - lastTickCount;

            if (elapsedTime > SUSPEND_BLACKHOLE) {
                MessageBoxA(NULL, Message2, TitleName, MB_ICONERROR | MB_OK);
                ExitProcess(0);
            }
            lastTickCount = currentTickCount;
            Sleep(1000);
        }
    }
    // ============================== Destroy ============================== //
    void Destroy(HMODULE hModule)
    {
        PIMAGE_THUNK_DATA pFirstThunkMirror, pOrigThunkMirror;

        IMAGE_DOS_HEADER* pDOS = (IMAGE_DOS_HEADER*)hModule;
        IMAGE_NT_HEADERS* pINH = (IMAGE_NT_HEADERS*)(hModule + pDOS->e_lfanew);

        if (pINH->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress)
        {
            IMAGE_IMPORT_DESCRIPTOR* pIID = (PIMAGE_IMPORT_DESCRIPTOR)(hModule + pINH->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);

            while (pIID->Characteristics)
            {
                pOrigThunkMirror = (PIMAGE_THUNK_DATA)(hModule + pIID->OriginalFirstThunk);
                pFirstThunkMirror = (PIMAGE_THUNK_DATA)(hModule + pIID->FirstThunk);

                pFirstThunkMirror->u1.Function = 0;
                pFirstThunkMirror->u1.AddressOfData = 0;
                pFirstThunkMirror->u1.Ordinal = 0;

                while (pOrigThunkMirror->u1.AddressOfData)
                {
                    pOrigThunkMirror->u1.Function = 0;
                    pOrigThunkMirror->u1.AddressOfData = 0;
                    pOrigThunkMirror->u1.Ordinal = 0;

                    pOrigThunkMirror++;
                    pFirstThunkMirror++;
                }

                pIID++;
            }

            pINH->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress = 0;
            pINH->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].Size = 0;

        }
    }
    // ============================== Sections ============================== //
    void Sections(HMODULE hModule)
    {
        PIMAGE_DOS_HEADER pImageDosHeader = (PIMAGE_DOS_HEADER)hModule;
        PIMAGE_NT_HEADERS pImageNtHeaders = (PIMAGE_NT_HEADERS)(hModule + pImageDosHeader->e_lfanew);
        PIMAGE_SECTION_HEADER pImageSectionHeader = IMAGE_FIRST_SECTION(pImageNtHeaders);

        for (int i = 0; i < pImageNtHeaders->FileHeader.NumberOfSections; ++i)
        {
            pImageSectionHeader[i].VirtualAddress = 0;
            pImageSectionHeader[i].SizeOfRawData = 0;
            pImageSectionHeader[i].PointerToRelocations = 0;
            pImageSectionHeader[i].Misc.PhysicalAddress = 0;
            pImageSectionHeader[i].Misc.VirtualSize = 0;
        }
    }
    // ============================== Threads ============================== //
    namespace Threads
    {
        // ============================== Anti-Thread ============================== //
        class SuspendThread {
        public:
            SuspendThread() {
                m_hThread = GetCurrentThread();
            }
            void PreventSuspend() {
                while (true) {
                    if (IsThreadSuspended()) {
                        ResumeThread(m_hThread);
                    }
                    Sleep(1000); // 1 saniye bekle
                }
            }
        private:
            bool IsThreadSuspended() {
                CONTEXT context;
                context.ContextFlags = CONTEXT_CONTROL;

                // Thread'in durumunu al
                if (GetThreadContext(m_hThread, &context)) {
                    return context.ContextFlags != CONTEXT_CONTROL;
                }
                return false;
            }
            HANDLE m_hThread;
        };
        // ============================== Hide Thread ============================== //
        bool HideThread(HMODULE hThread)
        {
            typedef NTSTATUS(NTAPI* pNtSetInformationThread)
                (HANDLE, UINT, PVOID, ULONG);
            NTSTATUS Status;

            pNtSetInformationThread NtSIT = (pNtSetInformationThread)
                GetProcAddress(GetModuleHandle(TEXT("ntdll.dll")),
                    "NtSetInformationThread");

            if (NtSIT == NULL)
                return false;
            if (hThread == NULL)
                Status = NtSIT(GetCurrentThread(),0x11, 0, 0);
            else
                Status = NtSIT(hThread, 0x11, 0, 0);

            if (Status != 0x00000000)
                return false;
            else
                return true;
        }
    }
}


DWORD WINAPI AntiCheatStarter(HMODULE hModule)
{
    while (true)
    {
        AntiCheat::Threads::SuspendThread AntiThread;
        AntiThread.PreventSuspend();
        AntiCheat::Suspend();
        Sleep(1000);
    }
}