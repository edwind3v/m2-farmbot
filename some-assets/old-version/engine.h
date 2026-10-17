#include <minwindef.h>
#include <vector>
#include <windows.h>
#include <shellapi.h>
#include <strsafe.h>
#include "sdk.h"


#define IDB_PNG4  101
class engine
{
public:
    static bool NetworkStreamSendAttackPacket(UINT dwAttack, DWORD dwVID)
    {
        try
        {
            typedef void(__thiscall* tSendBattleAttackPacket)(void* This, UINT dwAttack, DWORD dwVID);
            tSendBattleAttackPacket SendPacket = (tSendBattleAttackPacket)(sdk_client + metin2client::C_SendBattlePacket);

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!pNetStream || !SendPacket)
                return false;

            SendPacket(pNetStream, dwAttack, dwVID);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    static bool NetworkStreamSendSpecial(int nLen, void* pvBuf)
    {
        try
        {
            typedef bool(__thiscall* Special)(void* This, int nLen, void* pvBuf);
            Special Send = (Special)(sdk_client + metin2client::C_SendSpecial);
            void* NkStream = sdk::iNetworkStreamInstance();
            if (!NkStream || !Send)
                return false;

            return Send(NkStream, nLen, pvBuf);
        }
        catch (...) 
        { 
            return false; 
        }
    }
  /*  auto entities = GetEntities();
    for (int i = 0; i < entities.size(); ++i)
    {
        const auto& ent = entities[i];
        TPacketCGAttack SendBattlePacket{};
        SendBattlePacket.header = metin2client::C_SendSpecial;
        SendBattlePacket.bType = 0;
        SendBattlePacket.dwVictimVID = ent.VID;
        SendBattlePacket.bCRCMagicCubeProcPiece = 0;
        SendBattlePacket.bCRCMagicCubeFilePiece = 0;
        NetworkStreamSendSpecial(sizeof(TPacketCGAttack), &SendBattlePacket);
    }
    Sleep(10);*/
    static bool NetworkStreamSendCommandPacket(DWORD a1, DWORD a2, const char* a3)
    {
        try
        {
            typedef bool(__thiscall* tCPythonNetworkStreamSendCommandPacket)(void* NetworkStream, DWORD Arg1, DWORD Arg2, const char* Text);
            tCPythonNetworkStreamSendCommandPacket SendCommand = (tCPythonNetworkStreamSendCommandPacket)(sdk_client + metin2client::C_SendCommandPacket);

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!pNetStream || !SendCommand)
                return false;

            SendCommand(pNetStream, a1, a2, "");
        }
        catch (...)
        {
            return false;
        }
    }


    // GF Item Manager : a1 ? ? ? ? 89 45 ? 6a ? 8d 4d ? 51 8b 55 ? 52 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c0 0f 84 ? ? ? ? c7 45 ? ? ? ? ? c7 45
    // GF : 55 8b ec 83 ec ? 89 4d ? c6 45 ? ? 8d 45 ? 50 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c9 75 ? 32 c0 eb ? 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 d2 75 ? b0 ? eb ? c6 45 ? ? 8b 45
    // GF : 8b ec 83 ec ? 89 4d ? c6 45 ? ? 8d 45 ? 50 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c9 75 ? 32 c0 eb ? 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 d2 75 ? b0 ? eb ? c6 45 ? ? 8b 45
    // GF Item Instance : 8b 15 ? ? ? ? 89 95 ? ? ? ? 8d 45 ? 50 8b 8d ? ? ? ? e8 ? ? ? ? 0f b6 ? 85 c9 0f 84 ? ? ? ? 8d 55 ? 52 8d 45 ? 50 8b 4d ? 83 c1 ? e8 ? ? ? ? 8b 4d ? 83 c1 ? 89 4d ? 8b 55 ? 89 95 ? ? ? ? 8b 45 ? 8b 08 89 8d ? ? ? ? 8b 95 ? ? ? ? 89 95
    // GF ItemInstance v2 : 8b 15 ? ? ? ? 89 55 ? b8
    // GF Device : a1 ? ? ? ? 57 6a ? 68
    // GF Device : ff 51 ? 85 c0 78 ? 83 c6
    // GF PythonApp : 8b 15 ? ? ? ? 89 55 ? 8b 4d ? e8 ? ? ? ? 8b 4d ? 89 41 ? 8b 15 ? ? ? ? 89 55 ? 8b 4d ? e8 ? ? ? ? 8b 4d ? 89 41 ? 8b 15 ? ? ? ? 89 55 ? 8b 4d ? e8 ? ? ? ? 8b 4d ? 89 41 ? 8b 15

    // GF : COnnectGameDirect : 55 8b ec 83 ec ? 89 4d ? 0f b7 45 ? 8b 4d
    // GF CommectDirect : 0f b7 91 ? ? ? ? 3b c2
    // GF : ConnectDirect : e8 ? ? ? ? 0f b6 ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc 55 8b ec 51
    static bool NetworkStreamSendItemPickUpPacket(DWORD VID)
    {
        try
        {
            typedef bool(__thiscall* SendItemPickUpPacket)(void* This, DWORD vid); // yapıldı :d
            SendItemPickUpPacket AutoPickup = (SendItemPickUpPacket)(sdk_client + metin2client::C_SendItemPickUpPacket); // bilmiyorum :d belki yanlış

            void* pNetStream = sdk::iNetworkStreamInstance(); // %100 de bir milyon doğru
            if (!pNetStream || !AutoPickup)
                return false;

            AutoPickup(pNetStream, VID);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

     // =========================================== Yeni Item Ground systeam ================================================
    static const char* InstanceBaseGetNameString(DWORD* instance)
    {
        if (instance == NULL)
            return "";

        typedef const char* (__thiscall* tCInstanceBaseGetNameString)(void* This);
        DWORD calladdr = sdk_client + metin2client::C_GetNames;
        tCInstanceBaseGetNameString func = reinterpret_cast<tCInstanceBaseGetNameString>(calladdr);

        const char* result = NULL;
        __try
        {
            result = func((void*)instance);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            result = "";
        }
        return result ? result : "";
    }
    static map<DWORD, TGroundItemInstance*> GetGroundItemList()
    {
        map<DWORD, TGroundItemInstance*> vidList;
        try
        {
            // Gameforge 4) + 4)
            // Celestial Word 4) + 4)
            // Elaris 4) + 8)
            string player_name = engine::InstanceBaseGetNameString(sdk::GetMainActorPtr());
            TGroundItemInstanceMap m_GroundItemInstanceMap = *(TGroundItemInstanceMap*)(*reinterpret_cast<DWORD*>(*reinterpret_cast<DWORD*>(metin2client::iCPythonItemInstance + 4) + 8));
            for (TGroundItemInstanceMap::iterator itor = m_GroundItemInstanceMap.begin(); itor != m_GroundItemInstanceMap.end(); itor++)
            {
                string ownerShip = "";
                try
                {
                    ownerShip = itor->second->stOwnership;
                }
                catch (...)
                {

                }
                if (ownerShip == "" || ownerShip == player_name)
                {
                    vidList.insert(std::make_pair(itor->first, itor->second));
                }
            }
        }
        catch (...)
        {

        }
        return vidList;
    }

    static Vector3 GetTempPosition(Vector3 DestPos, int& count)
    {
        Vector3 PlayerPos;
        engine::GetMainGetPixelPosition(&PlayerPos);
        int distance_x = abs(DestPos.x - PlayerPos.x);
        int distance_y = abs(DestPos.y - PlayerPos.y);
        int squared = sqrt((distance_x * distance_x) + (distance_y * distance_y));
        if (squared < 180)
        {
            count = 1;
        }
        else
        {
            count = squared / 180;
        }

        Vector3 result;
        result.x = PlayerPos.x + (DestPos.x - PlayerPos.x) / count;
        result.y = PlayerPos.y + (DestPos.y - PlayerPos.y) / count;
        result.z = 0;
        return result;
    }

    static void NetworkStreamSendShopSellPacket(int slot, BYTE count = 1)
    {
        try
        {
            typedef void(__thiscall* tSendShopSellPacket)(void* pThis, BYTE slot, BYTE count);

            void* networkStream = sdk::iNetworkStreamInstance();
            if (!networkStream)
            {
                return;
            }

            tSendShopSellPacket SendShopSellPacket = (tSendShopSellPacket)(sdk_client + metin2client::C_SendShopSellPacketNew);
            SendShopSellPacket(networkStream, (BYTE)slot, count);
        }
        catch (...)
        {
        }
    }

    static void NetworkStreamSendItemDropPacket(int slot, DWORD count = 1)
    {
        try
        {
            typedef void(__thiscall* tSendItemDropPacket)(void* pThis, Sex pos, DWORD count);
            tSendItemDropPacket SendItemDropPacket = (tSendItemDropPacket)(sdk_client + 0xEA060);

            void* networkStream = sdk::iNetworkStreamInstance();
            if (!networkStream)
            {
                return;
            }

            Sex pos(1, slot);

            SendItemDropPacket(networkStream, pos, count);

        }
        catch (...)
        {
        }
    }

    static DWORD GetItemIndex(int slot)
    {
        try
        {
            DWORD playerBase = *(DWORD*)(sdk_client + metin2client::C_CPythonPlayer);
            if (!playerBase)
                return 0;
            DWORD vtable = *(DWORD*)(playerBase + metin2client::C_Mng);
            if (!vtable)
                return 0;

            Sex pos(1, slot);

            typedef DWORD(__thiscall* tGetItemIndex)(void* pThis, Sex pos);
            DWORD funcAddr = *(DWORD*)(vtable + metin2client::C_ItemVTable);
            if (!funcAddr)
                return 0;
            tGetItemIndex GetItemIndex = (tGetItemIndex)funcAddr;
            void* pThis = (void*)(playerBase + metin2client::C_Mng);
            return GetItemIndex(pThis, pos);
        }
        catch (...)
        {

        }
    }
    static DWORD PlayerGetItemIndexV1(BYTE window_type, WORD cell)
    {
        __try
        {
            void* playerBase = *(void**)(sdk_client + metin2client::C_CPythonPlayer);
            if (!playerBase)
                return 0;

            void* player = (char*)playerBase + metin2client::C_Mng;
            if (!player)
                return 0;

            DWORD* vtable = *(DWORD**)player;
            if (!vtable)
                return 0;

            DWORD fnAddr = vtable[metin2client::C_ItemVTable / 4];
            if (!fnAddr)
                return 0;

            typedef DWORD(__thiscall* tGetItemIndex)(void*, BYTE, WORD);
            tGetItemIndex GetItemIndex = (tGetItemIndex)fnAddr;

            return GetItemIndex(player, window_type, cell);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            return 0;
        }
    }

    // =========================================== extra packet ================================================
    static bool InstanceBaseSCRIPT_SetPixelPosition(float x, float y)
    {
        try
        {
            typedef void(__thiscall* tSetPixelPosition)(void* This, float fx, float fy);
            tSetPixelPosition SetPixel = (tSetPixelPosition)(sdk_client + metin2client::C_SetPixelPosition);

            void* pInstance = sdk::GetMainActorPtr();
            if (!pInstance || !SetPixel)
                return false;

            SetPixel(pInstance, x, y);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }
    static bool NetworkStreamInstanceBaseNEW_GetPixelPosition(Vector3* pPixelPosition)
    {
        try
        {
            typedef void(*GetPixelPosition)(void* instance, Vector3* pPixelPosition);
            GetPixelPosition Pixel = (GetPixelPosition)(sdk_client + metin2client::C_GetPixelPosition);

            void* pInstance = sdk::GetMainActorPtr();
            if (!pInstance || !Pixel)
                return false;

            Pixel(pInstance, pPixelPosition);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }


    static void SetAttackSpeedSafe(int value)
    {
        try
        {
            typedef void(__thiscall* tSetAttackSpeed)(void* This, UINT uAtkSpd);
            tSetAttackSpeed CSetAttackSpeed = (tSetAttackSpeed)(sdk_client + 0x571B0); 

            void* pInstance = sdk::GetMainActorPtr;
            if (!CSetAttackSpeed || !pInstance) return;

            CSetAttackSpeed(pInstance, value);
        }
        catch (...)
        {
     
        }
    }
    static bool NetworkStreamSendChatPacket(const char* text, BYTE Type)
    {
        try
        {
            typedef bool(__thiscall* tSendChatPacket)(void* This, const char* c_szChat, BYTE byType);
            tSendChatPacket SendPacket = (tSendChatPacket)(sdk_client + metin2client::C_SendChatPacket);

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!pNetStream || !SendPacket || !text)
                return false;

            return SendPacket(pNetStream, text, Type);
        }
        catch (...)
        {
            return false;
        }
    }

    /*static TMapInfo* BackgroundGlobalPositionToMapInfo(DWORD dwGlobalX, DWORD dwGlobalY)
    {
        typedef TMapInfo* (__thiscall* tCPythonBackgroundGlobalPositionToMapInfo)(void* This, DWORD dwGlobalX, DWORD dwGlobalY, int unk);
        tCPythonBackgroundGlobalPositionToMapInfo fn = (tCPythonBackgroundGlobalPositionToMapInfo)(sdk_client + 0x23059A0);

        if (!fn)
            return nullptr;

        void* pBackground = sdk::İCPythonBackground();
        if (!pBackground)
            return nullptr;

        __try
        {
            return fn(pBackground, dwGlobalX, dwGlobalY, 0);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            return nullptr;
        }
    }*/

    static void NetworkStreamSendRefinePacket(int slot1, int sans)
    {
        try
        {
            typedef void(__thiscall* tRefine)(void* This, int slot, int chance);
            tRefine SendRefinePacket = (tRefine)(sdk_client + metin2client::C_SendRefinePacket);

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!SendRefinePacket || !pNetStream) return;

            SendRefinePacket(pNetStream, slot1, sans);
        }
        catch (...)
        {
        }
    }

    static void NetworkStreamSendWhisperPacket(const char* targetName, const char* message)
    {
        try
        {
            typedef bool(__thiscall* tSendWhisper)(void* This, const char* name, const char* c_szChat);
            tSendWhisper SendWhisper = (tSendWhisper)(sdk_client + metin2client::C_SendWhisperPacket);

            void* pNetworkStream = sdk::iNetworkStreamInstance();
            if (!pNetworkStream || !SendWhisper)
                return;

            SendWhisper(pNetworkStream, targetName, message);
        }
        catch (...)
        {
        }
    }

    static bool ConnectTCP(const std::string& ip, int port)
    {
        try
        {
            typedef void(__thiscall* tConnectTCP)(void* This, const char* ip, int port);

            tConnectTCP ConnectFunc = (tConnectTCP)(sdk_client + metin2client::C_ConnectTCP);

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!pNetStream || !ConnectFunc)
                return false;

            ConnectFunc(pNetStream, ip.c_str(), port);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    static bool Disconnect()
    {
        try
        {
            typedef void(__thiscall* tDisconnect)(void* This);
            tDisconnect DisconnectFunc = (tDisconnect)(sdk_client + metin2client::C_Disconnect); // Yapıldı!!

            void* pNetStream = sdk::iNetworkStreamInstance();
            if (!pNetStream || !DisconnectFunc)
                return false;

            DisconnectFunc(pNetStream);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    static void CharacterManager_SetPixelPosition(float fx, float fy)
    {
        try
        {
            typedef void(__thiscall* tSCRIPT_SetPixelPosition)(void* This, float fx, float fy);
            tSCRIPT_SetPixelPosition SetPixelPos = (tSCRIPT_SetPixelPosition)(sdk_client + metin2client::C_SetPixelPosition);

            void* pInstance = sdk::GetMainActorPtr;
            if (!SetPixelPos || !pInstance) return;

            SetPixelPos(pInstance, fx, fy);
        }
        catch (...) {}
    }

    static void PickCloseItem()
    {
        try
        {
            typedef void(__thiscall* tPickCloseItem)(void* This);
            tPickCloseItem PickItem = (tPickCloseItem)(sdk_client + metin2client::C_PickCloseItem);

            void* pInstance = sdk::iPythonInstance();
            if (!PickItem || !pInstance) return;

            PickItem(pInstance);
        }
        catch (...) {}
    }

    static void ComboAttack()
    {
        try
        {
            typedef void(__thiscall* tComboAttack)(void* This);
            tComboAttack Combo = (tComboAttack)(sdk_client + metin2client::C_ComboAttack);

            void* pInstance = sdk::iPythonInstance();
            if (!Combo || !pInstance) return;

            Combo(pInstance);
        }
        catch (...) {}
    }

    static void Refresh()
    {
        try
        {
            typedef void(__thiscall* tRefresh)(void* This);
            tRefresh Combo = (tRefresh)(sdk_client + metin2client::C_Refresh);

            void* pInstance = sdk::İCPythonCharacterManager();
            if (!Combo || !pInstance) return;

            Combo(pInstance);
        }
        catch (...) {}
    }

    static void InstanceRefresh()
    {
        try
        {
            typedef void(__thiscall* tInstanceRefresh)(void* This);
            tInstanceRefresh Refresh = (tInstanceRefresh)(sdk_client + metin2client::C_Refresh);
            void* pInstance = sdk::İCPythonCharacterManager;
            if (!Refresh || !pInstance) return;
            Refresh(pInstance);
        }
        catch (...) {}
    }
    static DWORD GetTargetVID()
    {
        try
        {
            typedef DWORD(__thiscall* tCPythonPlayerGetTargetVID)(void* This);
            tCPythonPlayerGetTargetVID GetVID = (tCPythonPlayerGetTargetVID)(sdk_client + metin2client::C_GetTargetVID);

            void* Instance = sdk::iPythonInstance();
            if (!Instance || !GetVID)
                return 0;

            return GetVID(Instance);
        }
        catch (...)
        {
            return 0;
        }
    }

    static bool SetAttackKeyState(bool type)
    {
        try
        {
            typedef void(__thiscall* tFishFunc)(void* This, bool isPressed);
            tFishFunc FishFunc = reinterpret_cast<tFishFunc>(sdk_client + metin2client::C_SetAttackKeyState);
            void* pThis = sdk::iPythonInstance();
            if (!pThis || !FishFunc)
                return false;
            FishFunc(pThis, type);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }
    static bool PythonPlayerSetAttackKeyState(bool type)
    {
        try
        {
            typedef void(__thiscall* tFishFunc)(void* This, bool isPressed);
            tFishFunc FishFunc = reinterpret_cast<tFishFunc>(sdk_client + metin2client::C_SetAttackKeyState);
            void* pThis = sdk::iPythonInstance();
            if (!pThis || !FishFunc)
                return false;
            FishFunc(pThis, type);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    static std::string ApplicationBackground_GetCurrentMapName()
    {
        try
        {
            typedef const char* (__thiscall* CPythonBackground_etCurrentMapName)(void* This);
            CPythonBackground_etCurrentMapName Name = (CPythonBackground_etCurrentMapName)(sdk_client + metin2client::C_GetCurrentMapName);
            void* Instance = sdk::İCPythonBackground();
            if (!Instance || !Name)
                return "";
            const char* MapName = Name(Instance);
            return std::string(MapName);
        }
        catch (...)
        {
            return "";
        }
    }

    static Vector3 InstanceBaseNEW_GetPixelPosition(Vector3* pOut)
    {
        try
        {
            typedef void(__thiscall* tCInstanceBaseNEW_GetPixelPosition)(void* thisPtr, Vector3* pOut);
            tCInstanceBaseNEW_GetPixelPosition GetPixel = (tCInstanceBaseNEW_GetPixelPosition)(sdk_client + metin2client::C_GetPixelPosition);

            void* Instance = sdk::GetMainActorPtr();
            if (!Instance || !GetPixel)
            {
                if (pOut)
                    *pOut = Vector3{ 0.f, 0.f, 0.f };
                return { 0.f, 0.f, 0.f };
            }

            Vector3 vector{};
            GetPixel(Instance, &vector);

            if (pOut)
                *pOut = vector;

            return vector;
        }
        catch (...) {
            if (pOut)
                *pOut = Vector3{ 0.f, 0.f, 0.f };
            return Vector3{ 0.f, 0.f, 0.f };
        }
    }

    static float CalculateRotation(const Vector3& from, const Vector3& to)
    {
        float dx = to.x - from.x;
        float dy = to.y - from.y;
        float angle = atan2f(dy, dx);
        return angle;
    }


    static BOOL ShowBalloonV2(HWND hWnd, const char* title, const char* text)
    {
        if (!hWnd) return FALSE;

        NOTIFYICONDATA nfidata = { sizeof(NOTIFYICONDATA) };
        nfidata.hWnd = hWnd;
        nfidata.uFlags = NIF_INFO; 
        nfidata.uVersion = NOTIFYICON_VERSION_4;
        nfidata.dwInfoFlags = NIIF_NONE; 
        nfidata.uTimeout = 3000; 

        StringCchCopyA(nfidata.szInfo, ARRAYSIZE(nfidata.szInfo), text);
        StringCchCopyA(nfidata.szInfoTitle, ARRAYSIZE(nfidata.szInfoTitle), title);

        return Shell_NotifyIcon(NIM_ADD, &nfidata);
    }

    static BOOL ShowBalloon(HWND hWnd, const char* title, const char* text, HICON hIcon)
    {
        BOOL bRes = FALSE;
        if (hWnd != NULL)
        {
            NOTIFYICONDATA nfidata = { sizeof(NOTIFYICONDATA) };
            nfidata.cbSize = sizeof(nfidata);
            nfidata.hWnd = hWnd;
            nfidata.hIcon = (HICON)LoadImage(GetModuleHandle(NULL), MAKEINTRESOURCE(IDB_PNG4), IMAGE_ICON, GetSystemMetrics(SM_CXSMICON), GetSystemMetrics(SM_CYSMICON), LR_DEFAULTCOLOR);
            nfidata.uFlags = NIF_ICON | NIF_INFO | NIF_TIP;
            if (hIcon)
            {
                nfidata.hBalloonIcon = hIcon;
            }
            nfidata.uVersion = NOTIFYICON_VERSION_4;
            nfidata.dwInfoFlags = NIIF_USER | NIIF_LARGE_ICON;
            nfidata.hBalloonIcon = LoadIcon(GetModuleHandle(NULL), MAKEINTRESOURCE(IDB_PNG4));
            nfidata.uTimeout = 500;
            //nfidata.hBalloonIcon = LoadIcon(NULL, MAKEINTRESOURCE(IDB_PNG1));;
            /*nfidata.dwInfoFlags = NIIF_INFO;*/
            StringCchCopy(nfidata.szInfo, ARRAYSIZE(nfidata.szInfo), text);
            StringCchCopy(nfidata.szInfoTitle, ARRAYSIZE(nfidata.szInfoTitle), title);
            bRes = Shell_NotifyIcon(NIM_ADD, &nfidata);

        }
        return bRes;
    }

    static std::vector<Vector3> DivideTwoPointsByDistance(float distance, const Vector3& start, const Vector3& end)
    {
        std::vector<Vector3> path;

        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float dz = end.z - start.z;
        float totalDist = sqrtf(dx * dx + dy * dy + dz * dz);

        if (totalDist < distance)
        {
            path.push_back(end);
            return path;
        }

        int steps = static_cast<int>(totalDist / distance);
        for (int i = 1; i <= steps; ++i)
        {
            float t = static_cast<float>(i) / steps;
            Vector3 p = {
                start.x + dx * t,
                start.y + dy * t,
                start.z + dz * t
            };
            path.push_back(p);
        }

        return path;
    }

    static bool NetworkStreamAlves2SendCharacterState(const Vector3& c_rkPPosDst, float fDstRot, unsigned int eFunc, unsigned int uArg)
    {
        try
        {
            typedef void(__thiscall* TSendCharacterState)(void* pInstance, const Vector3& c_rkPPosDst, float Rotation, unsigned int a4, unsigned int a5);
            TSendCharacterState SendCharacterState = (TSendCharacterState)(sdk_client + metin2client::C_SendCharacterStatePacket);


            void* NetworkStream = sdk::iNetworkStreamInstance();
            if (!NetworkStream || !SendCharacterState)
                return false;

            SendCharacterState(NetworkStream, c_rkPPosDst, fDstRot, eFunc, uArg);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    static bool NetworkStreamCelestialWorldSendCharacterState(const Vector3& c_rkPPosDst, float fDstRot, unsigned __int8 eFunc, unsigned __int8 uArg, char none)
    {
        try
        {
            typedef void(__thiscall* TSendCharacterState)(void* pInstance, const Vector3& kPos, float Rotation, unsigned __int8 a4, unsigned __int8 a5, char a6);
            TSendCharacterState SendCharacterState = (TSendCharacterState)(sdk_client + metin2client::C_SendCharacterStatePacket);


            void* NetworkStream = sdk::iNetworkStreamInstance();
            if (!NetworkStream || !SendCharacterState)
                return false;

            SendCharacterState(NetworkStream, c_rkPPosDst, fDstRot, eFunc, uArg, none);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }
    void GameForge(const Vector3& c_rkPPosDst, float fDstRot, UINT eFunc, UINT uArg)
    {
        DWORD NetWorkStream = *(DWORD*)(sdk_client + metin2client::C_CPythonNetworkStream); // doğru
        DWORD aaaaa = (sdk_client + metin2client::C_SendCharacterStatePacket);
        _asm
        {
            mov ecx, NetWorkStream // doğru
            push uArg // doğru
            push eFunc // doğru
            push fDstRot // doğru çalısıyor sorunsuz
            mov eax, c_rkPPosDst // suan doğru 
            push eax // doğrı
            call aaaaa // doğru
        }
    }
    static bool NetworkStreamSendCharacterStatePacket(const Vector3& c_rkPPosDst, float fDstRot, UINT eFunc, UINT uArg)
    {
        try
        {
            typedef void(__thiscall* tSendCharacterState)(void* pInstance, const Vector3& kPos, float z, UINT eFunc, UINT uArg);
            tSendCharacterState Send = (tSendCharacterState)(sdk_client + metin2client::C_SendCharacterStatePacket);

            void* NetworkStream = sdk::iNetworkStreamInstance();
            if (!NetworkStream || !Send)
                return false;

            Send(NetworkStream, c_rkPPosDst, fDstRot, eFunc, uArg);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }


    static bool NetworkStreamSendAddFlyTargetingPacket(DWORD dwTargetVID, const Vector3& targetPos)
    {
        try
        {
            typedef bool(__thiscall* tSendAddFlyTargeting)(void* pThis, DWORD dwTargetVID, const Vector3& kPPosTarget);
            tSendAddFlyTargeting SendFly = (tSendAddFlyTargeting)(sdk_client + metin2client::C_SendAddFlyTargeting); 

            void* pNetworkStream = sdk::iNetworkStreamInstance();
            if (!pNetworkStream || !SendFly)
                return false;

            return SendFly(pNetworkStream, dwTargetVID, targetPos);
        }
        catch (...)
        {
            return false;
        }
    }
    static bool SendShootPacket(UINT Attack)
    {
        try
        {
            typedef bool(__thiscall* tSendShootPacket)(void* This, UINT Attack);
            tSendShootPacket SendShoot = (tSendShootPacket)(sdk_client + metin2client::C_SendShootPacket);

            void* pNetwork = sdk::iNetworkStreamInstance();
            if (!pNetwork || !SendShoot)
                return false;

            return SendShoot(pNetwork, Attack);
        }
        catch (...) {
            return false;
        }
    }

    static void InstanceBase__SetAffect2(int type1, int type2, int type3)
    {
        try
        {
            typedef void(__thiscall* tSetAffect)(void* This, int, int, int);
            tSetAffect SetAffect = (tSetAffect)(sdk_client + metin2client::C_SetAffect);

            void* pInstance = (void*)sdk::CharacterManager();
            if (!pInstance || !SetAffect)
                return;

            SetAffect(pInstance, type1, type2, type3);
        }
        catch (...)
        {

        }
    }

    static void InstanceBase__SetAffect(int type1, int type2, int type3)
    {
        try
        {
            DWORD Effects = (DWORD)sdk::CharacterManager();
            if (!Effects) return;

            DWORD addr = sdk_client + metin2client::C_SetAffect;

            __asm
            {
                mov ecx, Effects
                push type1
                push type2
                push type3
                call addr
            }
        }
        catch (...) {}
    }

    static void SetArmor(int armorID)
    {
        try
        {
            typedef void(__thiscall* tSetArmor)(void* This, int ID, int bilinmeyen);
            tSetArmor Set = (tSetArmor)(sdk_client + metin2client::C_SetArmor);

            void* Instance = (void*)sdk::GetMainActorPtr();
            if (!Instance || !Set)
                return;

            Set(Instance, armorID, 0);
        }
        catch (...) {}
    }

    static void InstanceBase_ClickSkillSlot(DWORD dwSlotIndex)
    {
        try
        {
            typedef void(__thiscall* tCPythonPlayerClickSkillSlot)(void* This, DWORD dwSlotIndex);
            tCPythonPlayerClickSkillSlot ClickSkill = (tCPythonPlayerClickSkillSlot)(sdk_client + metin2client::C_ClickSkillSlot);

            void* pInstance = sdk::iPythonInstance();
            if (!pInstance || !ClickSkill)
                return;

            ClickSkill(pInstance, dwSlotIndex);
        }
        catch (...)
        {
        }
    }
    static void NetworkStreamSendItemUsePacket(TItemPos itemIndex)
    {
        try
        {
            typedef void(__thiscall* tSendItemUsePacket)(void* This, TItemPos itemIndex);
            tSendItemUsePacket SendPacket = (tSendItemUsePacket)(sdk_client + metin2client::C_SendItemUsePacket);

            void* NetworkInstance = sdk::iNetworkStreamInstance();
            if (!NetworkInstance || !SendPacket)
                return;

            SendPacket(NetworkInstance, itemIndex);
        }
        catch (...)
        {

        }
    }

    static bool NetworkStreamPythonClickMining(DWORD vid)
    {
        try
        {
            typedef bool(__thiscall* tClickMining)(void* This, DWORD vid);
            tClickMining ClickMiningFunc = reinterpret_cast<tClickMining>(sdk_client + metin2client::C_SendOnClickPacket);

            void* pNetworkStream = sdk::iNetworkStreamInstance();
            if (!ClickMiningFunc || !pNetworkStream)
                return false;

            return ClickMiningFunc(pNetworkStream, vid);
        }
        catch (...)
        {
            return false;
        }
    }

    static void NetworkStreamSendGuildOfferPacket(int exp)
    {
        try
        {
            typedef void(__thiscall* tSendGuildOfferPacket)(void* This, int exp);
            tSendGuildOfferPacket SendXP = (tSendGuildOfferPacket)(sdk_client + metin2client::C_SendGuildOfferPacket);

            void* Instance = sdk::iNetworkStreamInstance();
            if (!Instance || !SendXP)
                return;

            SendXP(Instance, exp);
        }
        catch (...)
        {
            return;
        }
    }

    static bool InstanceBaseNEW_MoveToDestPixelPositionDirection(Vector3* desk)
    {
        try
        {
            typedef bool(__thiscall* MoveToDestPosition)(DWORD instance, Vector3* pos);
            MoveToDestPosition SetPixel = (MoveToDestPosition)(sdk_client + metin2client::C_MoveToDestPosition);

            DWORD* instance = sdk::GetMainActorPtr();
            if (!instance || !desk)return false;

            bool result = SetPixel((DWORD)instance, desk);
            return result;
        }
        catch (...)
        {
            return false;
        }
    }



    // GetStatus
    static int GetStatus(DWORD dwType)
    {
        try
        {
            typedef int(__thiscall* tCPythonPlayerGetStatus)(void* This, DWORD dwType);
            tCPythonPlayerGetStatus Status = (tCPythonPlayerGetStatus)(sdk_client + metin2client::C_GetStatus);

            void* PythonInstance = sdk::iCPythonPlayerInstance();
            if (!PythonInstance || !Status)return -1;

            return Status(PythonInstance, dwType);
        }
        catch (...)
        {
            return 0;
        }
    }
    static int GetPlayerHP()
    {
        int hp = GetStatus(POINT_HP);
        int maxHp = GetStatus(POINT_MAX_HP);
        return (hp * 100) / maxHp;
    }

    static int GetPlayerSP()
    {
        int mp = GetStatus(POINT_SP);
        int maxMp = GetStatus(POINT_MAX_SP);
        return (mp * 100) / maxMp;
    }
    static int GetPlayerXP()
    {
        int mp = GetStatus(POINT_EXP);
        int maxMp = GetStatus(POINT_NEXT_EXP);
        return (mp * 100) / maxMp;
    }

    static Vector3 GetMainGetPixelPosition(Vector3* pOut)
    {
        try
        {
            Vector3 vector = {
                c_players.x,
                -c_players.y,
                c_players.z
            };

            if (pOut)
                *pOut = vector;

            return vector;
        }
        catch (...)
        {
            if (pOut)
                *pOut = Vector3{ 0.f, 0.f, 0.f };
            return Vector3{ 0.f, 0.f, 0.f };
        }
    }
    static Vector3 NEW_GetPixelPosition(Vector3* pOut, int count)
    {
        try
        {
            if (count < 0 || count >= 331)
            {
                if (pOut)
                    *pOut = Vector3{ 0.f, 0.f, 0.f };
                return { 0.f, 0.f, 0.f };
            }

            Vector3 vector = {
                c_entity[count].x,
                -c_entity[count].y,
                c_entity[count].z
            };

            if (pOut)
                *pOut = vector;

            return vector;
        }
        catch (...)
        {
            if (pOut)
                *pOut = Vector3{ 0.f, 0.f, 0.f };
            return Vector3{ 0.f, 0.f, 0.f };
        }
    }

    static bool ApplicationSetCameraMaxDistance(float distance)
    {
        try
        {
            using tSetCameraMaxDistance = bool(__thiscall*)(void* This, float);

            tSetCameraMaxDistance SetDist = reinterpret_cast<tSetCameraMaxDistance>(sdk_client + metin2client::C_SetCameraMaxDistance);

            void* instance = sdk::iCPythonApplication();

            if (!SetDist || !instance)
                return false;

            return SetDist(instance, distance);
        }
        catch (...)
        {
            return false;
        }
    }

    static void PlayerCollision(int pos)
    {
        try
        {
            sdk::Write<float>(metin2client::C_NoClip, pos);
        }
        catch (...)
        {
        }
    }

    static void SendCharacterVector(Vector3 pos)
    {
        try
        {
            sdk::Write<float>(metin2client::C_CoordsX, pos.x);
            sdk::Write<float>(metin2client::C_CoordsY, pos.y);
            sdk::Write<float>(metin2client::C_CoordsZ, pos.z);
        }
        catch (...)
        {
        }
    }

    static void InstanceBase_AttackRange(Vector3 pos)
    {
        try
        {
            sdk::Write<float>(metin2client::C_ModelCoordX, pos.x);
            sdk::Write<float>(metin2client::C_ModelCoordY, -fabs(pos.y));
            sdk::Write<float>(metin2client::C_ModelCoordZ, pos.z);
        }
        catch (...)
        {
        }
    }

    static void InstanceBase_SetPlayerRotation(float angle)
    {
        try
        {
            sdk::Write<float>(metin2client::C_IsRangeRotation, angle);
        }
        catch (...)
        {
        }
    }
    static void SendCharacterModel(Vector3 pos)
    {
        try
        {
            sdk::Write<float>(metin2client::C_ModelCoordX, pos.x);
            sdk::Write<float>(metin2client::C_ModelCoordY, -fabs(pos.y));
            sdk::Write<float>(metin2client::C_ModelCoordZ, pos.z);
        }
        catch (...)
        {
        }
    }
    static void SetPixelPosition(Vector3 pos)
    {
        try
        {
            if (fabs(pos.x) < 0.001f && fabs(pos.y) < 0.001f && fabs(pos.z) < 0.001f)
                return;
            sdk::Write<float>(metin2client::C_CoordsX, pos.x);
            sdk::Write<float>(metin2client::C_CoordsY, -fabs(pos.y));
            sdk::Write<float>(metin2client::C_CoordsX, pos.z);
        }
        catch (...)
        {
        }
    }
    static void SetPixelPosition(float x, float y)
    {
        try
        {
            sdk::Write<float>(metin2client::C_CoordsX, x);
            sdk::Write<float>(metin2client::C_CoordsY, -fabs(y));
        }
        catch (...)
        {
        }
    }

    static void RadarSetPixelPosition(Vector3 pos)
    {
        try
        {
            if (fabs(pos.x) < 0.001f && fabs(pos.y) < 0.001f && fabs(pos.z) < 0.001f)
                return;
            sdk::Write<float>(metin2client::C_CoordsX, pos.x);
            sdk::Write<float>(metin2client::C_CoordsY, -fabs(pos.y));
            sdk::Write<float>(metin2client::C_CoordsZ, pos.z); 
        }
        catch (...)
        {
        }
    }
    static void NetworkStreamSendCharacterStateNewPacket(Vector3 pos)
    {
        try
        {
            if (fabs(pos.x) < 0.001f && fabs(pos.y) < 0.001f && fabs(pos.z) < 0.001f)
                return;
            sdk::Write<float>(metin2client::C_ModelCoordX, pos.x);
            sdk::Write<float>(metin2client::C_ModelCoordY, -fabs(pos.y));
            sdk::Write<float>(metin2client::C_ModelCoordZ, pos.z);
        }
        catch (...)
        {
        }
    }
    static int CountDistanceTwoPoints(D3DVECTOR startPoint, D3DVECTOR endPoint)
    {
        return sqrt(pow(endPoint.x - startPoint.x, 2) +
            pow(endPoint.y - startPoint.y, 2) * 1.0);
    }
    static int CountDistanceTwoPoints(int x1, int y1, int x2, int y2)
    {
        return sqrt(pow(x2 - x1, 2) +
            pow(y2 - y1, 2) * 1.0);
    }
};