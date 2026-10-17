#include <libloaderapi.h>
#pragma once
uintptr_t sdk_client = (uintptr_t)GetModuleHandleA(NULL);
class sdk
{
public:
    static DWORD* GetMainActorPtr()
    {
        DWORD* CPythonCharacterManager = *(DWORD**)(sdk_client + metin2client::C_LocalPlayer);
        if (!CPythonCharacterManager)return nullptr;
        DWORD* Instance = *(DWORD**)((DWORD)CPythonCharacterManager + metin2client::C_LocalAddr);
        if (!Instance)return nullptr;
        return Instance;
    }

    static void* İCPythonBackground()
    {
        DWORD mgr = *(DWORD*)(sdk_client + metin2client::C_CPythonBackground);
        if (!mgr) return nullptr;
        return (void*)mgr;
    }

    static void* İCPythonCharacterManager()
    {
        DWORD mgr = *(DWORD*)(sdk_client + metin2client::C_LocalPlayer);
        if (!mgr) return nullptr;
        return (void*)mgr;
    }

    template<typename T>
    static void Write(DWORD offset, T value)
    {
        DWORD* pActor = GetMainActorPtr();
        if (!pActor)
            return;

        *(T*)((DWORD)pActor + offset) = value;
    }

    template<typename T>
    static T Read(DWORD offset)
    {
        DWORD* pActor = GetMainActorPtr();
        if (!pActor)
            return T();

        return *(T*)((DWORD)pActor + offset);
    }

    static DWORD* CharacterManager()
    {
        DWORD* CPythonCharacterManager = *(DWORD**)(sdk_client + metin2client::C_LocalPlayer);
        if (!CPythonCharacterManager)return nullptr;
        return CPythonCharacterManager;
    }
    static void* iCPythonApplication()
    {
        DWORD inst = *(DWORD*)(sdk_client + metin2client::C_CPythonApplication);
        if (!inst) return nullptr;
        return (void*)inst;
    }
    static void* iPythonInstance()
    {
        DWORD CPythonPlayer = *(DWORD*)(sdk_client + metin2client::C_CPythonPlayer);
        if (!CPythonPlayer) return nullptr;
        return (void*)CPythonPlayer;
    }
    static void* iNetworkStreamInstance()
    {
        DWORD inst = *(DWORD*)(sdk_client + metin2client::C_CPythonNetworkStream);
        if (!inst) return nullptr;
        return (void*)inst;
    }
    static void* iCPythonPlayerInstance()
    {
        DWORD CPythonPlayerMgr = *(DWORD*)(sdk_client + metin2client::C_CPythonPlayer);
        if (!CPythonPlayerMgr)return nullptr;

        void* pInstance = (void*)(CPythonPlayerMgr + metin2client::C_Mng);
        if (!pInstance)return nullptr;

        return pInstance;
    }
};
