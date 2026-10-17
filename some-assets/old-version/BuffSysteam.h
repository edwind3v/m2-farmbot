struct C_Buff
{
    int Type;
    float x;
    float y;
    float z;
    float ModelX;
    float ModelY;
    float ModelZ;
    float SkilRotation;
    float IsCharRotation;
    float Rotation;
    float C_ModelVectorX;
    float C_ModelVectorY;
    float C_ModelVectorZ;
    int BodyRecoil;
    int IsLvl;
    int IsDead;
    int IsChar;
    char Name[64] = { 0 };
    DWORD VID;
    DWORD BannedVID;
} C_Buff;
namespace Controller
{
    uintptr_t slave_base = 0;
    HANDLE hSlave = nullptr;
    template<typename T>
    T CReadProcessMemory(uintptr_t address)
    {
        T buffer{};
        if (!address || IsBadReadPtr((void*)address, sizeof(T)))
            return buffer;
        ReadProcessMemory(hSlave, (LPCVOID)address, &buffer, sizeof(T), nullptr);
        return buffer;
    }

    uintptr_t AdresSlave(uintptr_t pointer, std::vector<unsigned int> adres)
    {
        uintptr_t adres_c = pointer;
        for (unsigned int i = 0; i < adres.size(); ++i)
        {
            if (!adres_c || IsBadReadPtr((void*)adres_c, sizeof(uintptr_t)))
                return 0;
            adres_c = Controller::CReadProcessMemory<uintptr_t>(adres_c);
            if (!adres_c || IsBadReadPtr((void*)adres_c, sizeof(uintptr_t)))
                return 0;
            adres_c += adres[i];
        }
        return adres_c;
    }

    template<typename T>
    bool CWriteProcessMemory(uintptr_t address, T value)
    {
        if (!address || IsBadReadPtr((void*)address, sizeof(T)))
            return false;
        return WriteProcessMemory(hSlave, (LPVOID)address, &value, sizeof(T), nullptr);
    }
    bool AllModule()
    {
        HWND hwnd = GetForegroundWindow();
        DWORD pid = 0;
        GetWindowThreadProcessId(hwnd, &pid);

        if (!pid || pid == GetCurrentProcessId())
        {
            printf("[-] Kendi process secildi!\n");
            return false;
        }

        HANDLE h = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_CREATE_THREAD, FALSE, pid);
        if (!h)
        {
            printf("[-] OpenProcess fail: %d\n", GetLastError());
            return false;
        }

        Controller::hSlave = h;
        Controller::slave_base = sdk_client;
        printf("[+] Slave OK | Base: %X | PID: %d\n", (DWORD)Controller::slave_base, pid);
        return true;
    }
    namespace PowerSell
    {
        static DWORD GetDinputBase()
        {
            HMODULE hMods[1024];
            DWORD cbNeeded;
            if (EnumProcessModules(Controller::hSlave, hMods, sizeof(hMods), &cbNeeded))
            {
                for (DWORD i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
                {
                    char modName[MAX_PATH];
                    if (GetModuleFileNameExA(Controller::hSlave, hMods[i], modName, sizeof(modName)))
                    {
                        if (strstr(modName, "DINPUT8.dll") || strstr(modName, "dinput8.dll"))
                            return (DWORD)hMods[i];
                    }
                }
            }
            return 0;
        }
        static bool RunRemoteThread(void* pCode, DWORD waitMs = 3000)
        {
            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0,
                (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
            if (!hThread) return false;
            WaitForSingleObject(hThread, waitMs);
            CloseHandle(hThread);
            return true;
        }
        static bool IsSlaveAlive()
        {
            if (!Controller::hSlave || !Controller::slave_base) return false;
            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE) return false;
            return true;
        }

        bool PowerSellSendBattleAttack(UINT attack, DWORD vid)
        {
            if (!Controller::hSlave || !Controller::slave_base)
            {
                return false;
            }

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
            {
                if (exitCode != STILL_ACTIVE)
                {
                    return false;
                }
            }

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                UINT  attack;
                DWORD vid;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendBattlePacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.attack = attack;
            params.vid = vid;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params),
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams)
                return false;

            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55,                           // push ebp
                0x8B, 0xEC,                     // mov ebp, esp
                0x60,                           // pushad
                0xB8, 0x00, 0x00, 0x00, 0x00,  // mov eax, pParams
                0x8B, 0x48, 0x08,               // mov ecx, [eax+08]  ; baseAddr
                0x03, 0x48, 0x04,               // add ecx, [eax+04]  ; + networkStreamOffset
                0x8B, 0x09,                     // mov ecx, [ecx]     ; CPythonNetworkStream*
                0xFF, 0x70, 0x10,               // push [eax+10]      ; vid
                0xFF, 0x70, 0x0C,               // push [eax+0C]      ; attack
                0xFF, 0x10,                     // call [eax]         ; funcAddr
                0x61,                           // popad
                0x8B, 0xE5,                     // mov esp, ebp
                0x5D,                           // pop ebp
                0xC3                            // ret
            };

            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode)
            {
                VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
                return false;
            }

            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);
            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0, (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
            bool success = false;
            if (hThread)
            {
                WaitForSingleObject(hThread, 50);
                success = true;
                CloseHandle(hThread);
            }
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return success;
        }
        // ============== ClickSkillSlot =======================
        bool PowerSellSendChatPacket(const char* text, BYTE type)
        {
            if (!Controller::hSlave || !Controller::slave_base)
                return false;

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE)
                    return false;

            size_t textLen = strlen(text) + 1;
            void* pText = VirtualAllocEx(Controller::hSlave, NULL, textLen,
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pText) return false;
            WriteProcessMemory(Controller::hSlave, pText, text, textLen, NULL);

            struct Params {
                DWORD funcAddr;              // +00: C_SendChatPacket
                DWORD networkStreamOffset;   // +04: C_CPythonNetworkStream
                DWORD baseAddr;              // +08: slave_base
                DWORD textAddr;              // +0C: pText
                BYTE  type;                  // +10: type
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendChatPacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.textAddr = (DWORD)pText;
            params.type = type;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params),
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams)
            {
                VirtualFreeEx(Controller::hSlave, pText, 0, MEM_RELEASE);
                return false;
            }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55,                           // push ebp
                0x8B, 0xEC,                     // mov ebp, esp
                0x60,                           // pushad

                0xB8, 0x00, 0x00, 0x00, 0x00,  // mov eax, pParams [offset 5]

                // ecx = *(baseAddr + networkStreamOffset) = NetworkStream*
                0x8B, 0x48, 0x08,               // mov ecx, [eax+08] ; baseAddr
                0x03, 0x48, 0x04,               // add ecx, [eax+04] ; + networkStreamOffset
                0x8B, 0x09,                     // mov ecx, [ecx]    ; NetworkStream*

                0x85, 0xC9,                     // test ecx, ecx
                0x74, 0x0A,                     // jz exit

                // __thiscall: push type, push text, call
                0x0F, 0xB6, 0x50, 0x10,         // movzx edx, byte [eax+10] ; type
                0x52,                           // push edx
                0xFF, 0x70, 0x0C,               // push [eax+0C]             ; textAddr
                0xFF, 0x10,                     // call [eax]                ; funcAddr

                // exit:
                0x61,                           // popad
                0x8B, 0xE5,                     // mov esp, ebp
                0x5D,                           // pop ebp
                0xC3                            // ret
            };

            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code),
                MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode)
            {
                VirtualFreeEx(Controller::hSlave, pText, 0, MEM_RELEASE);
                VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
                return false;
            }

            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0,
                (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);

            bool success = false;
            if (hThread)
            {
                WaitForSingleObject(hThread, 3000);
                success = true;
                CloseHandle(hThread);
            }

            VirtualFreeEx(Controller::hSlave, pText, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);

            return success;
        }
        // ============== SendItemDrop =======================
        bool NetworkStreamSendItemDropPacket(int slot, DWORD count = 1)
        {
            if (!Controller::hSlave || !Controller::slave_base)
                return false;

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE)
                    return false;

            struct Sex {
                DWORD type;
                DWORD slot;
            };

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                Sex   pos;
                DWORD count;
            };

            Params params;
            params.funcAddr = Controller::slave_base + 0xEA060;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.pos.type = 1;
            params.pos.slot = (DWORD)slot;
            params.count = count;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params),
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;

            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55,                           // push ebp
                0x8B, 0xEC,                     // mov ebp, esp
                0x60,                           // pushad

                0xB8, 0x00, 0x00, 0x00, 0x00,  // mov eax, pParams [offset 5]

                // ecx = NetworkStream*
                0x8B, 0x48, 0x08,               // mov ecx, [eax+08] ; baseAddr
                0x03, 0x48, 0x04,               // add ecx, [eax+04] ; + networkStreamOffset
                0x8B, 0x09,                     // mov ecx, [ecx]    ; NetworkStream*

                0x85, 0xC9,                     // test ecx, ecx
                0x74, 0x0C,                     // jz exit

                0xFF, 0x70, 0x14,               // push [eax+14]     ; count
                0xFF, 0x70, 0x10,               // push [eax+10]     ; pos.slot
                0xFF, 0x70, 0x0C,               // push [eax+0C]     ; pos.type
                0xFF, 0x10,                     // call [eax]        ; funcAddr

                // exit:
                0x61,                           // popad
                0x8B, 0xE5,                     // mov esp, ebp
                0x5D,                           // pop ebp
                0xC3                            // ret
            };

            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code),
                MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode)
            {
                VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
                return false;
            }

            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0,
                (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);

            bool success = false;
            if (hThread)
            {
                WaitForSingleObject(hThread, 3000);
                success = true;
                CloseHandle(hThread);
            }

            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);

            return success;
        }
        // ============== SendSpecial =======================
        bool NetworkStreamSendSpecial(int nLen, void* pvBuf)
        {
            if (!Controller::hSlave || !Controller::slave_base)
                return false;

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE)
                    return false;


            void* pBuf = VirtualAllocEx(Controller::hSlave, NULL, nLen,
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pBuf) return false;
            WriteProcessMemory(Controller::hSlave, pBuf, pvBuf, nLen, NULL);

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD nLen;
                DWORD pvBuf;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendSpecial;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.nLen = (DWORD)nLen;
            params.pvBuf = (DWORD)pBuf;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params),
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams)
            {
                VirtualFreeEx(Controller::hSlave, pBuf, 0, MEM_RELEASE);
                return false;
            }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55,
                0x8B, 0xEC,
                0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x08,
                0xFF, 0x70, 0x10,
                0xFF, 0x70, 0x0C,
                0xFF, 0x10,
                0x61,
                0x8B, 0xE5,
                0x5D,
                0xC3
            };

            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code),
                MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode)
            {
                VirtualFreeEx(Controller::hSlave, pBuf, 0, MEM_RELEASE);
                VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
                return false;
            }

            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0,
                (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);

            bool success = false;
            if (hThread)
            {
                WaitForSingleObject(hThread, 3000);
                success = true;
                CloseHandle(hThread);
            }

            VirtualFreeEx(Controller::hSlave, pBuf, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);

            return success;
        }
        // ============== Pickup =======================
        //bool NetworkStreamSendItemPickUpPacket(DWORD vid)
        //{
        //    if (!IsSlaveAlive()) return false;

        //    struct Params {
        //        DWORD funcAddr;
        //        DWORD networkStreamOffset;
        //        DWORD baseAddr;
        //        DWORD vid;
        //    };

        //    Params params;
        //    params.funcAddr = Controller::slave_base + metin2client::C_SendItemPickUpPacket;
        //    params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
        //    params.baseAddr = Controller::slave_base;
        //    params.vid = vid;

        //    void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        //    if (!pParams) return false;
        //    WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

        //    BYTE code[] = {
        //        0x55, 0x8B, 0xEC, 0x60,
        //        0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
        //        0x8B, 0x48, 0x08,
        //        0x03, 0x48, 0x04,
        //        0x8B, 0x09,
        //        0x85, 0xC9,
        //        0x74, 0x06,
        //        0xFF, 0x70, 0x0C,
        //        0xFF, 0x10,
        //        0x61, 0x8B, 0xE5, 0x5D, 0xC3
        //    };
        //    *(DWORD*)&code[5] = (DWORD)pParams;

        //    void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        //    if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
        //    WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

        //    bool ok = RunRemoteThread(pCode);
        //    VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
        //    VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
        //    return ok;
        //}
        // ============== Whisper =======================
        bool NetworkStreamSendWhisperPacket(const char* targetName, const char* message)
        {
            if (!IsSlaveAlive() || !targetName || !message) return false;

            size_t nameLen = strlen(targetName) + 1;
            size_t msgLen = strlen(message) + 1;

            void* pName = VirtualAllocEx(Controller::hSlave, NULL, nameLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pName) return false;
            WriteProcessMemory(Controller::hSlave, pName, targetName, nameLen, NULL);

            void* pMsg = VirtualAllocEx(Controller::hSlave, NULL, msgLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pMsg) { VirtualFreeEx(Controller::hSlave, pName, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pMsg, message, msgLen, NULL);

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD nameAddr;
                DWORD msgAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendWhisperPacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.nameAddr = (DWORD)pName;
            params.msgAddr = (DWORD)pMsg;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) { VirtualFreeEx(Controller::hSlave, pName, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pMsg, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x08,
                0xFF, 0x70, 0x10,                // push msgAddr
                0xFF, 0x70, 0x0C,                // push nameAddr
                0xFF, 0x10,                      // call funcAddr
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pName, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pMsg, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pName, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pMsg, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== Refine =======================
        bool NetworkStreamSendRefinePacket(int slot, int chance)
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD slot;
                DWORD chance;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendRefinePacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.slot = (DWORD)slot;
            params.chance = (DWORD)chance;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x08,
                0xFF, 0x70, 0x10,                // push chance
                0xFF, 0x70, 0x0C,                // push slot
                0xFF, 0x10,                      // call funcAddr
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== Connect =======================
        bool NetworkStreamConnectTCP(const char* ip, int port)
        {
            if (!IsSlaveAlive() || !ip) return false;

            size_t ipLen = strlen(ip) + 1;
            void* pIp = VirtualAllocEx(Controller::hSlave, NULL, ipLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pIp) return false;
            WriteProcessMemory(Controller::hSlave, pIp, ip, ipLen, NULL);

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD ipAddr;
                DWORD port;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_ConnectTCP;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.ipAddr = (DWORD)pIp;
            params.port = (DWORD)port;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) { VirtualFreeEx(Controller::hSlave, pIp, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x08,
                0xFF, 0x70, 0x10,                // push port
                0xFF, 0x70, 0x0C,                // push ipAddr
                0xFF, 0x10,                      // call funcAddr
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pIp, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pIp, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== Discconnect =======================
        bool NetworkStreamDisconnect()
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_Disconnect;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x02,
                0xFF, 0x10,
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== SetPixel =======================
        //bool NetworkStreamSetPixelPosition(float fx, float fy)
        //{
        //    if (!IsSlaveAlive()) return false;

        //    struct Params {
        //        DWORD funcAddr;
        //        DWORD instanceOffset;   // C_GetMainActorPtr offset
        //        DWORD baseAddr;
        //        float fx;
        //        float fy;
        //    };

        //    Params params;
        //    params.funcAddr = Controller::slave_base + metin2client::C_SetPixelPosition;
        //    params.instanceOffset = metin2client::C_GetMainActorPtr;
        //    params.baseAddr = Controller::slave_base;
        //    params.fx = fx;
        //    params.fy = fy;

        //    void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        //    if (!pParams) return false;
        //    WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

        //    BYTE code[] = {
        //        0x55, 0x8B, 0xEC, 0x60,
        //        0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
        //        0x8B, 0x48, 0x08,                // mov ecx, baseAddr
        //        0x03, 0x48, 0x04,                // add ecx, instanceOffset
        //        0x8B, 0x09,                      // mov ecx, [ecx] = instance
        //        0x85, 0xC9,
        //        0x74, 0x08,
        //        0xFF, 0x70, 0x14,                // push fy
        //        0xFF, 0x70, 0x10,                // push fx
        //        0xFF, 0x10,                      // call funcAddr
        //        0x61, 0x8B, 0xE5, 0x5D, 0xC3
        //    };
        //    *(DWORD*)&code[5] = (DWORD)pParams;

        //    void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        //    if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
        //    WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

        //    bool ok = RunRemoteThread(pCode);
        //    VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
        //    VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
        //    return ok;
        //}
        // ============== ComboAttack =======================
        bool PythonPlayerPickCloseItem()
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD instanceOffset;
                DWORD baseAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_PickCloseItem;
            params.instanceOffset = metin2client::C_CPythonPlayer;
            params.baseAddr = Controller::slave_base;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x02,
                0xFF, 0x10,
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== XP Donate =======================
        bool NetworkStreamSendGuildOfferPacket(int exp)
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD exp;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendGuildOfferPacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.exp = (DWORD)exp;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,                // mov ecx, baseAddr
                0x03, 0x48, 0x04,                // add ecx, networkStreamOffset
                0x8B, 0x09,                      // mov ecx, [ecx] = NetworkStream*
                0x85, 0xC9,                      // test ecx, ecx
                0x74, 0x06,                      // jz exit
                0xFF, 0x70, 0x0C,                // push exp
                0xFF, 0x10,                      // call funcAddr
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== ComboAttack =======================
        int PythonPlayerGetStatus(DWORD dwType)
        {
            if (!IsSlaveAlive()) return -1;

            // Sonucu buraya yazacak
            void* pResult = VirtualAllocEx(Controller::hSlave, NULL, sizeof(int), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pResult) return -1;
            int zero = -1;
            WriteProcessMemory(Controller::hSlave, pResult, &zero, sizeof(int), NULL);

            struct Params {
                DWORD funcAddr;
                DWORD instanceOffset;
                DWORD baseAddr;
                DWORD dwType;
                DWORD resultAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_GetStatus;
            params.instanceOffset = metin2client::C_CPythonPlayer;
            params.baseAddr = Controller::slave_base;
            params.dwType = dwType;
            params.resultAddr = (DWORD)pResult;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) { VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE); return -1; }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC,
                0x53,                            // push ebx

                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,                // mov ecx, [eax+08] ; baseAddr
                0x03, 0x48, 0x04,                // add ecx, [eax+04] ; + instanceOffset
                0x8B, 0x09,                      // mov ecx, [ecx]    ; instance

                0x85, 0xC9,                      // test ecx, ecx
                0x74, 0x0C,                      // jz exit

                0xFF, 0x70, 0x0C,                // push [eax+0C]     ; dwType
                0xFF, 0x10,                      // call [eax]        ; funcAddr → eax = status

                // sonucu yaz: [resultAddr] = eax
                0x8B, 0x5D, 0xF8,                // mov ebx, ... (eax bozulmadan önce kaydet)
                // eax = return value, ebx = pParams tekrar lazım
                0x50,                            // push eax          ; status kaydet
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams  [29]
                0x8B, 0x58, 0x10,                // mov ebx, [eax+10] ; resultAddr
                0x58,                            // pop eax           ; status geri al
                0x89, 0x03,                      // mov [ebx], eax    ; *resultAddr = status

                // exit:
                0x5B,                            // pop ebx
                0x8B, 0xE5, 0x5D, 0xC3
            };

            *(DWORD*)&code[5] = (DWORD)pParams;
            *(DWORD*)&code[29] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return -1; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            RunRemoteThread(pCode);

            int status = -1;
            ReadProcessMemory(Controller::hSlave, pResult, &status, sizeof(int), NULL);

            VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return status;
        }

        int BuffGetPlayerHP()
        {
            int hp = PythonPlayerGetStatus(POINT_HP);
            int maxHp = PythonPlayerGetStatus(POINT_MAX_HP);
            if (maxHp <= 0) return 0;
            return (hp * 100) / maxHp;
        }

        int BuffGetPlayerSP()
        {
            int sp = PythonPlayerGetStatus(POINT_SP);
            int maxSp = PythonPlayerGetStatus(POINT_MAX_SP);
            if (maxSp <= 0) return 0;
            return (sp * 100) / maxSp;
        }

        int BuffGetPlayerXP()
        {
            int xp = PythonPlayerGetStatus(POINT_EXP);
            int maxXp = PythonPlayerGetStatus(POINT_NEXT_EXP);
            if (maxXp <= 0) return 0;
            return (xp * 100) / maxXp;
        }
        /*int hp = BuffGetPlayerHP();
        int sp = BuffGetPlayerSP();
        int xp = BuffGetPlayerXP();*/
        // ============== ComboAttack =======================
        bool PythonPlayerComboAttack()
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD instanceOffset;
                DWORD baseAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_ComboAttack;
            params.instanceOffset = metin2client::C_CPythonPlayer;
            params.baseAddr = Controller::slave_base;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x02,
                0xFF, 0x10,
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== SetAttackKeyState =======================
        bool PythonPlayerSetAttackKeyState(bool type)
        {
            if (!IsSlaveAlive()) return false;

            struct Params {
                DWORD funcAddr;
                DWORD instanceOffset;
                DWORD baseAddr;
                BYTE  type;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SetAttackKeyState;
            params.instanceOffset = metin2client::C_CPythonPlayer;
            params.baseAddr = Controller::slave_base;
            params.type = type ? 1 : 0;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC, 0x60,
                0xB8, 0x00, 0x00, 0x00, 0x00,
                0x8B, 0x48, 0x08,
                0x03, 0x48, 0x04,
                0x8B, 0x09,
                0x85, 0xC9,
                0x74, 0x06,
                0x0F, 0xB6, 0x50, 0x0C,
                0x52,
                0xFF, 0x10,
                0x61, 0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            bool ok = RunRemoteThread(pCode);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return ok;
        }
        // ============== GetTargetVID =======================
        DWORD PythonPlayerGetTargetVID()
        {
            if (!IsSlaveAlive()) return 0;

            void* pResult = VirtualAllocEx(Controller::hSlave, NULL, sizeof(DWORD), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pResult) return 0;
            DWORD zero = 0;
            WriteProcessMemory(Controller::hSlave, pResult, &zero, sizeof(DWORD), NULL);

            struct Params {
                DWORD funcAddr;
                DWORD instanceOffset;
                DWORD baseAddr;
                DWORD resultAddr;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_GetTargetVID;
            params.instanceOffset = metin2client::C_CPythonPlayer;
            params.baseAddr = Controller::slave_base;
            params.resultAddr = (DWORD)pResult;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) { VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE); return 0; }
            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55, 0x8B, 0xEC,
                0x53,                            // push ebx
                0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
                0x8B, 0x48, 0x08,                // mov ecx, baseAddr
                0x03, 0x48, 0x04,                // add ecx, instanceOffset
                0x8B, 0x09,                      // mov ecx, [ecx]
                0x85, 0xC9,
                0x74, 0x07,
                0xFF, 0x10,                      // call funcAddr → eax = VID
                0x8B, 0x58, 0x0C,                // mov ebx, [eax+0C] resultAddr
                0x89, 0x03,                      // mov [ebx], eax
                0x5B,                            // pop ebx
                0x8B, 0xE5, 0x5D, 0xC3
            };
            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode) { VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return 0; }
            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            RunRemoteThread(pCode);

            DWORD vid = 0;
            ReadProcessMemory(Controller::hSlave, pResult, &vid, sizeof(DWORD), NULL);

            VirtualFreeEx(Controller::hSlave, pResult, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
            return vid;
        }
        // ============== SendItemPickup =======================
        bool PowerSellSendItemPickUpPacket(DWORD vid)
        {
            if (!Controller::hSlave || !Controller::slave_base)
                return false;

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE)
                    return false;

            struct Params {
                DWORD funcAddr;
                DWORD networkStreamOffset;
                DWORD baseAddr;
                DWORD vid;
            };

            Params params;
            params.funcAddr = Controller::slave_base + metin2client::C_SendItemPickUpPacket;
            params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
            params.baseAddr = Controller::slave_base;
            params.vid = vid;

            void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params),
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!pParams) return false;

            WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

            BYTE code[] = {
                0x55,                           // push ebp
                0x8B, 0xEC,                     // mov ebp, esp
                0x60,                           // pushad

                0xB8, 0x00, 0x00, 0x00, 0x00,  // mov eax, pParams [offset 5]
                0x8B, 0x48, 0x08,               // mov ecx, [eax+08] ; baseAddr
                0x03, 0x48, 0x04,               // add ecx, [eax+04] ; + networkStreamOffset
                0x8B, 0x09,                     // mov ecx, [ecx]    ; NetworkStream*

                0x85, 0xC9,                     // test ecx, ecx
                0x74, 0x06,                     // jz exit

                0xFF, 0x70, 0x0C,               // push [eax+0C]     ; vid
                0xFF, 0x10,                     // call [eax]        ; funcAddr

                // exit:
                0x61,                           // popad
                0x8B, 0xE5,                     // mov esp, ebp
                0x5D,                           // pop ebp
                0xC3                            // ret
            };

            *(DWORD*)&code[5] = (DWORD)pParams;

            void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code),
                MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (!pCode)
            {
                VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
                return false;
            }

            WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

            HANDLE hThread = CreateRemoteThread(Controller::hSlave, NULL, 0,
                (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);

            bool success = false;
            if (hThread)
            {
                WaitForSingleObject(hThread, 3000);
                success = true;
                CloseHandle(hThread);
            }

            VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
            VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);

            return success;
        }
        // ============== ClickSkillSlot =======================
        bool PowerSellClickSkillSlot(DWORD dwSlotIndex)
        {
            if (!Controller::hSlave || !Controller::slave_base)
                return false;

            DWORD exitCode;
            if (GetExitCodeProcess(Controller::hSlave, &exitCode))
                if (exitCode != STILL_ACTIVE)
                    return false;

            DWORD keyOffset = 0;
            switch (dwSlotIndex)
            {
            case 0: keyOffset = Client.Key.KeyBoard_Slot1; break;
            case 1: keyOffset = Client.Key.KeyBoard_Slot2; break;
            case 2: keyOffset = Client.Key.KeyBoard_Slot3; break;
            case 3: keyOffset = Client.Key.KeyBoard_Slot4; break;
            default: return false;
            }

            HMODULE hMods[1024];
            DWORD cbNeeded;
            DWORD dinputBase = 0;

            if (EnumProcessModules(Controller::hSlave, hMods, sizeof(hMods), &cbNeeded))
            {
                for (DWORD i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
                {
                    char modName[MAX_PATH];
                    if (GetModuleFileNameExA(Controller::hSlave, hMods[i], modName, sizeof(modName)))
                    {
                        if (strstr(modName, "DINPUT8.dll") || strstr(modName, "dinput8.dll"))
                        {
                            dinputBase = (DWORD)hMods[i];
                            break;
                        }
                    }
                }
            }

            if (!dinputBase)
            {
                return false;
            }

            DWORD bgKeyAddr = dinputBase + Client.Key.KeyBoard_BackgroundKey;
            Controller::CWriteProcessMemory<DWORD>(bgKeyAddr, 0);

            DWORD keyAddr = dinputBase + keyOffset;
            Controller::CWriteProcessMemory<DWORD>(keyAddr, 128);
            Sleep(50);
            Controller::CWriteProcessMemory<DWORD>(keyAddr, 0);

            return true;
        }
        // ============== ClickSkillSlot =======================
        bool MetworkStreamClickSkillSlotv2(DWORD dwSlotIndex)
        {
            if (!IsSlaveAlive()) return false;

            DWORD keyOffset = 0;
            switch (dwSlotIndex)
            {
            case 0: keyOffset = Client.Key.KeyBoard_Slot1; break;
            case 1: keyOffset = Client.Key.KeyBoard_Slot2; break;
            case 2: keyOffset = Client.Key.KeyBoard_Slot3; break;
            case 3: keyOffset = Client.Key.KeyBoard_Slot4; break;
            default: return false;
            }

            DWORD dinputBase = GetDinputBase();
            if (!dinputBase) { printf(""); return false; }
            Controller::CWriteProcessMemory<DWORD>(dinputBase + Client.Key.KeyBoard_BackgroundKey, 0);
            Controller::CWriteProcessMemory<DWORD>(dinputBase + keyOffset, 128);
            Sleep(50);
            Controller::CWriteProcessMemory<DWORD>(dinputBase + keyOffset, 0);
            return true;
        }
        // ============== MoveTo =======================
        //bool NetworkStreamMoveToDestPixelPosition(Vector3* desk)
        //{
        //    if (!IsSlaveAlive() || !desk) return false;

        //    // Vector3'ü slave'e yaz
        //    void* pVec = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Vector3), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        //    if (!pVec) return false;
        //    WriteProcessMemory(Controller::hSlave, pVec, desk, sizeof(Vector3), NULL);

        //    struct Params {
        //        DWORD funcAddr;        // +00
        //        DWORD instanceOffset;  // +04: C_GetMainActorPtr
        //        DWORD baseAddr;        // +08
        //        DWORD vecAddr;         // +0C: pVec
        //    };

        //    Params params;
        //    params.funcAddr = Controller::slave_base + metin2client::C_MoveToDestPosition;
        //    params.instanceOffset = metin2client::C_GetMainActorPtr;
        //    params.baseAddr = Controller::slave_base;
        //    params.vecAddr = (DWORD)pVec;

        //    void* pParams = VirtualAllocEx(Controller::hSlave, NULL, sizeof(Params), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        //    if (!pParams) { VirtualFreeEx(Controller::hSlave, pVec, 0, MEM_RELEASE); return false; }
        //    WriteProcessMemory(Controller::hSlave, pParams, &params, sizeof(Params), NULL);

        //    BYTE code[] = {
        //        0x55, 0x8B, 0xEC, 0x60,
        //        0xB8, 0x00, 0x00, 0x00, 0x00,   // mov eax, pParams [5]
        //        0x8B, 0x48, 0x08,                // mov ecx, baseAddr
        //        0x03, 0x48, 0x04,                // add ecx, instanceOffset
        //        0x8B, 0x09,                      // mov ecx, [ecx] = instance
        //        0x85, 0xC9,                      // test ecx, ecx
        //        0x74, 0x08,                      // jz exit
        //        0xFF, 0x70, 0x0C,                // push vecAddr  ; Vector3*
        //        0x51,                            // push ecx      ; instance (DWORD)
        //        0xFF, 0x10,                      // call [eax]    ; funcAddr
        //        0x61, 0x8B, 0xE5, 0x5D, 0xC3
        //    };
        //    *(DWORD*)&code[5] = (DWORD)pParams;

        //    void* pCode = VirtualAllocEx(Controller::hSlave, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        //    if (!pCode) { VirtualFreeEx(Controller::hSlave, pVec, 0, MEM_RELEASE); VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE); return false; }
        //    WriteProcessMemory(Controller::hSlave, pCode, code, sizeof(code), NULL);

        //    bool ok = RunRemoteThread(pCode);
        //    VirtualFreeEx(Controller::hSlave, pVec, 0, MEM_RELEASE);
        //    VirtualFreeEx(Controller::hSlave, pParams, 0, MEM_RELEASE);
        //    VirtualFreeEx(Controller::hSlave, pCode, 0, MEM_RELEASE);
        //    return ok;
        //}
    }
}