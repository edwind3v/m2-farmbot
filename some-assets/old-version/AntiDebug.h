#define _CRT_SECURE_NO_DEPRECATE
#include <Windows.h>
#include <iostream>
#include <tlhelp32.h>
#include <fstream>
#include <filesystem>
#include <direct.h>
#include <tchar.h>
#pragma optimize( "", off )

namespace fs = std::filesystem;

BOOL IsDebuggerPresentCheck()
{
    return IsDebuggerPresent();
}
namespace AntiDebug
{
    void WatchMemory()
    {
        if (IsDebuggerPresentCheck())
        {
            ExitProcess(1);
        }
        Sleep(1000);
    }
    bool Debugger() {
        unsigned int msw_value = 0;

#if defined(_M_IX86)
        __asm
        {
            mov eax, 0xCCCCCCCC
            smsw ax 
            mov msw_value, eax 
        }
        unsigned int highByte = (msw_value >> 24) & 0xFFu;
        unsigned int nextByte = (msw_value >> 16) & 0xFFu;

        if (highByte == 0xCCu && nextByte == 0xCCu)
            return true;
#endif

        return false; 
    }
    bool Mıxed() {
        bool mixFileDetected = false;
        for (const auto& entry : fs::directory_iterator(".")) {
            std::string extension = entry.path().extension().string();
            if (extension == ".mix" || extension == ".flt" || extension == ".m3d" ||
                extension == ".asi" || extension == ".bat" || extension == ".pyc" || extension == ".py") {
                mixFileDetected = true;
                break;
            }
        }

        if (mixFileDetected) {
            Sleep(1000);
            ExitProcess(0);
        }
        else {
        }

        return mixFileDetected;
    }
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
            Status = NtSIT(GetCurrentThread(),
                0x11, 
                0, 0);
        else
            Status = NtSIT(hThread, 0x11, 0, 0);

        if (Status != 0x00000000)
            return false;
        else
            return true;
    }
}