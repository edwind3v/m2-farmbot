#pragma once
#include <cstdint>
#include <strsafe.h>
#include <processthreadsapi.h>
#include <vector>
#include <libloaderapi.h>
#include <WinBase.h>
#include <WinUser.h>

HANDLE MEMORY = OpenProcess(PROCESS_ALL_ACCESS, 0, GetCurrentProcessId());
DWORD metin2client;
BOOL PointModule = FALSE;

uintptr_t Ez_game_base = (uintptr_t)GetModuleHandleA(NULL);
namespace Ez_memory
{
    uintptr_t Adres(uintptr_t pointer, std::vector<unsigned int> adres)
    {
        uintptr_t adres_c = pointer;
        for (unsigned int i = 0; i < adres.size(); ++i)
        {
            if (adres_c == 0 || IsBadReadPtr((void*)adres_c, sizeof(uintptr_t))) {
                return 0;
            }
            adres_c = *(uintptr_t*)adres_c;
            if (adres_c == 0 || IsBadReadPtr((void*)adres_c, sizeof(uintptr_t))) {
                return 0;
            }
            adres_c += adres[i];
        }
        return adres_c;
    }
}
template <typename T>
T ReadMemory(DWORD_PTR address, T* value = nullptr, bool isWrite = false) {
    if (isWrite) {
        if (!IsBadWritePtr(reinterpret_cast<void*>(address), sizeof(T))) {
            *(T*)address = *value;
            return *value;
        }
    }
    else {
        if (!IsBadReadPtr(reinterpret_cast<void*>(address), sizeof(T))) {
            return *(T*)address;
        }
    }
    T def{};
    return def;
}

uintptr_t GetModuleBaseAddress(const char* moduleName) {
    HMODULE hModule = GetModuleHandle(moduleName);
    if (!hModule) {
        MessageBox(NULL, /*Modül bulunamadı!*/XorStrTR<0x47, 18, 0x2CAE8000>("\x0A\x27\x2D\xB6\x27\x6C\x2F\x3B\x23\x25\x3F\x33\x3E\x35\x31\x67\x76" + 0x2CAE8000).s, /*Hata*/XorStrTR<0x47, 5, 0x2CAE8000>("\x0F\x29\x3D\x2B" + 0x2CAE8000).s, MB_OK);
        return 0;
    }
    return (uintptr_t)hModule;
}

namespace driver
{
    template <typename T>
    bool ReadMemory(DWORD moduleHandle, DWORD baseClient, DWORD addr, DWORD offsets, T& value)
    {
        DWORD off1, off2;
        if (!ReadProcessMemory(MEMORY, (LPCVOID)(moduleHandle + baseClient), &off1, sizeof(DWORD), NULL))
            return false;
        if (!ReadProcessMemory(MEMORY, (LPCVOID)(off1 + addr), &off2, sizeof(DWORD), NULL))
            return false;
        if (!ReadProcessMemory(MEMORY, (LPCVOID)(off2 + offsets), &value, sizeof(T), NULL))
            return false;

        return true;
    }


    template <typename T>
    bool Banned(DWORD moduleHandle, DWORD baseClient, DWORD addr, T& value)
    {
        DWORD off1;
        if (!ReadProcessMemory(MEMORY, (LPCVOID)(moduleHandle + baseClient), &off1, sizeof(DWORD), NULL))
            return false;

        if (!ReadProcessMemory(MEMORY, (LPCVOID)(off1 + addr), &value, sizeof(T), NULL))
            return false;

        return true;
    }

    void WriteAssembly(uintptr_t address, int newValue) {
        DWORD oldProtect;
        if (VirtualProtect((LPVOID)address, sizeof(int), PAGE_EXECUTE_READWRITE, &oldProtect)) {
            memcpy((void*)address, &newValue, sizeof(int));

            VirtualProtect((LPVOID)address, sizeof(int), oldProtect, &oldProtect);
        }
        else {
            MessageBox(NULL, /*Bellek koruma ayarı değiştirilemedi!*/XorStrTR<0x47, 37, 0x2CAE8000>("\x05\x2D\x25\x26\x2E\x27\x6D\x25\x20\x22\x24\x3F\x32\x74\x34\x2F\x36\x2A\x68\x7A\x3F\x39\x42\x37\x00\x14\x08\x10\x0A\x08\x00\x0B\x02\x0C\x00\x4B" + 0x2CAE8000).s, /*Hata*/XorStrTR<0x47, 5, 0x2CAE8000>("\x0F\x29\x3D\x2B" + 0x2CAE8000).s, MB_OK);
        }
    }
}
