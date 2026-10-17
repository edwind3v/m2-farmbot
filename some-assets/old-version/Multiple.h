#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <vector>
#include <wchar.h>

#pragma comment(lib,"ntdll.lib")

static PVOID GetLib(LPCSTR LibraryName, LPCSTR ProcName)
{
    auto hModule = GetModuleHandleA(LibraryName);
    if (hModule == NULL)
        return nullptr;
    return GetProcAddress(hModule, ProcName);
}

namespace Close
{
    bool Mutant(const wchar_t* name)
    {
        _NtQuerySystemInformation NtQuerySystemInformation = (_NtQuerySystemInformation)GetLib("ntdll.dll", "NtQuerySystemInformation");
        _NtDuplicateObject NtDuplicateObject = (_NtDuplicateObject)GetLib("ntdll.dll", "NtDuplicateObject");
        _NtQueryObject NtQueryObject = (_NtQueryObject)GetLib("ntdll.dll", "NtQueryObject");

        NTSTATUS status;

        ULONG handleInfoSize = 0x10000;
        PSYSTEM_HANDLE_INFORMATION handleInfo = (PSYSTEM_HANDLE_INFORMATION)malloc(handleInfoSize);

        ULONG pid = 0;
        HANDLE processHandle = GetCurrentProcess();
        ULONG i;
        while ((status = NtQuerySystemInformation(
            SystemHandleInformation,
            handleInfo,
            handleInfoSize,
            NULL
        )) == STATUS_INFO_LENGTH_MISMATCH)
            handleInfo = (PSYSTEM_HANDLE_INFORMATION)realloc(handleInfo, handleInfoSize *= 2);
        if (!NT_SUCCESS(status))
        {
            return false;
        }

        bool closed = false;
        for (i = 0; i < handleInfo->HandleCount; i++)
        {
            if (closed)
                break;

            SYSTEM_HANDLE handle = handleInfo->Handles[i];
            HANDLE dupHandle = NULL;
            POBJECT_TYPE_INFORMATION objectTypeInfo;
            PVOID objectNameInfo;
            UNICODE_STRING objectName;
            ULONG returnLength;
            if (!NT_SUCCESS(NtDuplicateObject(processHandle, (HANDLE)handle.Handle, GetCurrentProcess(), &dupHandle, 0, 0, 0)))
                continue;
            objectTypeInfo = (POBJECT_TYPE_INFORMATION)malloc(0x1000);
            if (!NT_SUCCESS(NtQueryObject(dupHandle, ObjectTypeInformation, objectTypeInfo, 0x1000, NULL)))
            {
                CloseHandle(dupHandle);
                continue;
            }
            if (handle.GrantedAccess == 0x0012019f)
            {
                free(objectTypeInfo);
                CloseHandle(dupHandle);
                continue;
            }

            objectNameInfo = malloc(0x1000);
            if (!NT_SUCCESS(NtQueryObject(dupHandle, ObjectNameInformation, objectNameInfo, 0x1000, &returnLength)))
            {
                objectNameInfo = realloc(objectNameInfo, returnLength);
                if (!NT_SUCCESS(NtQueryObject(dupHandle, ObjectNameInformation, objectNameInfo, returnLength, NULL)))
                {
                    free(objectTypeInfo);
                    free(objectNameInfo);
                    CloseHandle(dupHandle);
                    continue;
                }
            }
            objectName = *(PUNICODE_STRING)objectNameInfo;
            if (objectName.Length && lstrcmpiW(objectName.Buffer, name) == 0)
            {
                CloseHandle((HANDLE)handle.Handle);
                closed = true;
            }

            free(objectTypeInfo);
            free(objectNameInfo);
            CloseHandle(dupHandle);

        }

        free(handleInfo);
        CloseHandle(processHandle);
        return closed;
    }
}

//bool Multiple = true;
//DWORD WINAPI MultipleGame(HMODULE hModule)
//{
//    while (true)
//    {
//        if (Multiple)
//        {
//            Close::Mutant(L"\\Sessions\\1\\BaseNamedObjects\\hl2_singleton_mutex");
//        }
//    }
//}


void AllUser(std::wstring newNamess)
{
    const wchar_t* basePath = L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
    HKEY hKey;
    if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, basePath, 0, KEY_READ | KEY_WRITE | KEY_WOW64_64KEY, &hKey) != ERROR_SUCCESS) {
        std::wcerr << L"[!] ProfileList açılamadı.\n";
        return;
    }

    DWORD index = 0;
    wchar_t nameBuf[512];

    while (true)
    {
        DWORD nameBufLen = sizeof(nameBuf) / sizeof(nameBuf[0]);
        FILETIME ft;
        LONG ret = RegEnumKeyExW(hKey, index++, nameBuf, &nameBufLen, nullptr, nullptr, nullptr, &ft);
        if (ret == ERROR_NO_MORE_ITEMS) break;
        if (ret != ERROR_SUCCESS) continue;

        std::wstring sid(nameBuf, nameBufLen);

        if (sid.rfind(L"S-1-5-21-", 0) != 0) continue;

        HKEY hSub = nullptr;
        ret = RegOpenKeyExW(hKey, sid.c_str(), 0, KEY_READ | KEY_WRITE, &hSub);
        if (ret != ERROR_SUCCESS) continue;

        DWORD flags = RRF_RT_REG_EXPAND_SZ | RRF_RT_REG_SZ;
        DWORD type = 0;
        DWORD cbData = 0;

        ret = RegGetValueW(hSub, nullptr, L"ProfileImagePath", flags, &type, nullptr, &cbData);
        if (ret != ERROR_SUCCESS) { RegCloseKey(hSub); continue; }

        std::vector<wchar_t> buf(cbData / sizeof(wchar_t) + 2);
        ret = RegGetValueW(hSub, nullptr, L"ProfileImagePath", flags, &type, buf.data(), &cbData);
        if (ret != ERROR_SUCCESS) { RegCloseKey(hSub); continue; }

        buf[cbData / sizeof(wchar_t)] = L'\0';
        std::wstring rawPath(buf.data());

        std::wstring newName = newNamess;
        size_t sepPos = rawPath.find_last_of(L"\\/");
        std::wstring newRaw;
        if (sepPos == std::wstring::npos)
            newRaw = newName;
        else
            newRaw = rawPath.substr(0, sepPos + 1) + newName;

        DWORD writeType = (type == REG_EXPAND_SZ) ? REG_EXPAND_SZ : REG_SZ;

        ret = RegSetValueExW(hSub, L"ProfileImagePath", 0, writeType,
            reinterpret_cast<const BYTE*>(newRaw.c_str()),
            static_cast<DWORD>((newRaw.size() + 1) * sizeof(wchar_t)));

        if (ret == ERROR_SUCCESS)



            RegCloseKey(hSub);
    }

    RegCloseKey(hKey);

}

void Create()
{
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    freopen_s(&f, "CONIN$", "r", stdin);
    std::cout << "[+] Console opened successfully!\n";
}




static bool Multiple = false;
DWORD WINAPI MultipleGame(HMODULE hModule)
{
    //Create();
    while (true)
    {
        if (Multiple)
        {
            // Kullanıcı Profil'i degisti!
            std::wstring  WindowName = L"Girl1";
            AllUser(WindowName);
            std::this_thread::sleep_for(std::chrono::milliseconds(10000)); // 20 mili saniye
            std::wstring OrjinalChanger = []() { std::wifstream f(L"Apathe.txt"); std::wstring s; std::getline(f, s); return s; }();
            AllUser(OrjinalChanger);
            Multiple = false;
        }
    }
}