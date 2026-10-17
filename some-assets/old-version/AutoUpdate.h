#pragma once
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
#include <Psapi.h>
#include <Memoryapi.h>
#include <Windows.h>
#include <iostream>

uintptr_t FindPattern(const char* moduleName, const char* pattern, const char* mask)
{
    MODULEINFO mInfo = { 0 };
    HMODULE hModule = GetModuleHandleA(moduleName);
    if (!hModule)
        return 0;

    GetModuleInformation(GetCurrentProcess(), hModule, &mInfo, sizeof(MODULEINFO));

    uintptr_t base = (uintptr_t)mInfo.lpBaseOfDll;
    DWORD size = mInfo.SizeOfImage;

    for (DWORD i = 0; i < size; ++i)
    {
        bool found = true;
        for (DWORD j = 0; mask[j]; ++j)
        {
            if (mask[j] != '?' && pattern[j] != *(char*)(base + i + j))
            {
                found = false;
                break;
            }
        }
        if (found)
            return base + i;
    }

    return 0;
}

uintptr_t GetRVA(const char* sig, const char* mask, int offset)
{
    uintptr_t addr = FindPattern(NULL, sig, mask);
    if (!addr) return 0;
    uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
    uintptr_t rva = *(uintptr_t*)(addr + offset) - base;
    return rva;
}
uintptr_t GetCallRVA(const char* sig, const char* mask, int offset, int size)
{
    uintptr_t addr = FindPattern(NULL, sig, mask);
    if (!addr) return 0;
    uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
    int relativeOffset = *(int*)(addr + offset);
    uintptr_t absolute = addr + size + relativeOffset;
    return absolute - base;
}


namespace Rohan2_SigMaker
{
    char C_LocalPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x0f\xb6\x81";
    char C_LocalPlayerMask[] = "xx????x????xxxxxxxx";

    char C_PythonPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\xff\x15\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\x83\xc1\x00\x68\x00\x00\x00\x00\x8b\x01\xff\x50\x00\x50\x68\x00\x00\x00\x00\xff\x15\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\xe8";
    char C_PythonPlayerMask[] = "xx????x????xx????xx????xx?xxxxxxxx????xx?x????xxxx?xx????xx????xx?xxxxxxxxxxxxxxxx????x";

}



namespace AlbaMt2_SigMaker
{
    char C_CPythonBackground[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\xff\x15\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x83\xe4";
    char C_CPythonBackgroundMask[] = "xx????x????xx????xx????xx?xxxxxxxxxxx";
    // == LocalPlayer == //
    char C_CPythonApplication[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\xe9\x00\x00\x00\x00\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\xe9\x00\x00\x00\x00\x55\x8b\xec\x51\x8d\x45\x00\x50\x6a\x00\xff\x75\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x84\xc0\x75\x00\x6a\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\x8b\xe5\x5d\xc3\xff\x75\x00\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x8b\xe5\x5d\xc3\xcc\xcc\x55";
    char C_CPythonApplicationMask[] = "xx????x????x????xx????x????x????xxxxxx?xx?xx?x????xx?xxx?x?x????xx?xxxxxx?xx????x????x????xxxxxxx";
    // == LocalPlayer == //
    char C_NetworkStream[] = "\x8b\x0d\x00\x00\x00\x00\x57\xff\x75\x00\xe8\x00\x00\x00\x00\x8b\xce";
    char C_NetworkStreamMask[] = "xx????xxx?x????xx";
    // == LocalPlayer == //
    char C_CPythonPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\xff\x15\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x6a\x00\x68\x00\x00\x00\x00\x64\xa1\x00\x00\x00\x00\x50\x83\xec\x00\x53";
    char C_CPythonPlayerMask[] = "xx????x????xx????xx????xx?xxxxxxxxxx?x????xx????xxx?x";
    // == LocalPlayer == //
    char C_LocalPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\xd1";
    char C_LocalPlayerMask[] = "xx????x????xxxxxxx????x????xxxxxxx";

    char C_CSendBattlePacket[] = "\xe8\x00\x00\x00\x00\x8b\xce\xe8\x00\x00\x00\x00\x84\xc0\x74\x00\x8b\xce";
    char C_CSendBattlePacketMask[] = "x????xxx????xxx?xx";

    char C_SendShootPacket[] = "\xe9\x00\x00\x00\x00\xcc\x55\x8b\xec\x8b\x45\x00\x89\x41";
    char C_SendShootPacketMask[] = "x????xxxxxx?xx";

    char C_SendAddFlyTargeting[] = "\xe8\x00\x00\x00\x00\x83\x6d\x00\x00\x75";
    char C_SendAddFlyTargetingMask[] = "x????xx??x";
}



namespace Lycia2_SigMaker
{
    // mov ecx, [addr] formatındaki genel pattern
    char C_LocalPlayer_New[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x53";
    char C_LocalPlayer_NewMask[] = "xx????x????xxxxxxxxx";
}

namespace Alves2_SigMaker
{
    // mov ecx, [addr] formatındaki genel pattern
    char C_LocalPlayer_New[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\xd1";
    char C_LocalPlayer_NewMask[] = "xx????x????xxxxxxx????x????xxxxxxx";
}

namespace CelestialWorld_SigMaker
{
    // mov ecx, [addr] formatındaki genel pattern
    char C_LocalPlayer_New[] = "\x8B\x0D\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x3B\xF0\x75";
    char C_LocalPlayer_NewMask[] = "xx????x????xxx";
}

namespace Elaris_SigMaker
{
    /*---------# PythonPlayers #---------update*/
    char CPythonPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\xff\x15\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x6a\x00\x68\x00\x00\x00\x00\x64\xa1\x00\x00\x00\x00\x50\x83\xec\x00\x53";
    char CPythonPlayerMask[] = "xx????x????xx????xx????xx?xxxxxxxxxx?x????xx????xxx?x";

    /*---------# LocalPlayer #---------update*/ // Yapıldı!! Çalısıyor
    char C_LocalPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x56";
    char C_LocalPlayerMask[] = "xx????x????xxxxxxx????x????xxxxxx";
}



namespace Rubinum2_Sigmaker
{
    /*---------# Entitylist #---------update*/
    char C_Entitylist[] = "\x8b\x3d\x00\x00\x00\x00\x3b\xfb\x74\x00\x90";
    char C_EntitylistMask[] = "xx????xxx?x";

    /*---------# PythonPlayers #---------update*/
    char CPythonPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x50\x68\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x83\xc4\x00\xc3\xcc\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x6a\x00\x68\x00\x00\x00\x00\x64\xa1\x00\x00\x00\x00\x50\x83\xec\x00\x53";
    char CPythonPlayerMask[] = "xx????x????xx????x????xx?xxxxxxxxxxx?x????xx????xxx?x";

    /*---------# LocalPlayer #---------update*/ // Yapıldı!!
    char C_LocalPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\x0d\x00\x00\x00\x00\xe9\x00\x00\x00\x00\xcc\xcc\xcc\xcc\xcc\x8b\xd1";
    char C_LocalPlayerMask[] = "xx????x????xxxxxxx????x????xxxxxxx";
}

namespace Origins2_SigMaker
{
    /*---------# PythonPlayers #---------update*/
    char CPythonPlayer[] = "\xa1\x00\x00\x00\x00\x5d\xc3\xcc\xcc\xcc\xcc\xcc\xcc\x55\x8b\xec\x6a\x00\x68\x00\x00\x00\x00\x64\xa1\x00\x00\x00\x00\x50\x83\xec\x00\xa1\x00\x00\x00\x00\x33\xc5\x50\x8d\x45\x00\x64\xa3\x00\x00\x00\x00\x89\x4d\x00\x8d\x45";
    char CPythonPlayerMask[] = "x????xxxxxxxxxxxx?x????xx????xxx?x????xxxxx?xx????xx?xx";

    /*---------# LocalPlayer #---------update*/ // Yapıldı!! Çalısıyor
    char C_LocalPlayer[] = "\x8b\x0d\x00\x00\x00\x00\xe8\x00\x00\x00\x00\x3b\xf0\x75\x00\x68";
    char C_LocalPlayerMask[] = "xx????x????xxx?x";
}
