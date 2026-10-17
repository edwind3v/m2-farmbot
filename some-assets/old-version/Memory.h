#include <vector>
#include <algorithm>
#include "Mem.h"
#include <iostream>

#pragma pack(push)
#pragma pack(1)
typedef struct cmd
{
    BYTE header;
    BYTE bType;
    DWORD dwVictimVID;
    BYTE bCRCMagicCubeProcPiece;
    BYTE bCRCMagicCubeFilePiece;
} TPacketCGAttack;

typedef struct command_move
{
    BYTE        bHeader;
    BYTE        bFunc;
    BYTE        bArg;
    BYTE        bRot;
    LONG        lX;
    LONG        lY;
    DWORD        dwTime;
} TPacketCGMove;


//#include "Local.h"

#define Six(x)  ( (float)(x) * (float)(M_PI / 180.f) )
#define M_PI		3.14159265358979323846f
float Angle = 0.0f;

typedef std::map<DWORD, DWORD*> PythonCharacterManager;
std::vector<Entity> GetEntities()
{
    std::vector<Entity> result;
    metin2client::Globals();
    DWORD* CharacterManager = reinterpret_cast<DWORD*>(Ez_game_base + metin2client::C_LocalPlayer);
    if (!IsBadReadPtr(CharacterManager, sizeof(DWORD))) {
        DWORD entityManager = *CharacterManager;
        if (!IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager))) {
            PythonCharacterManager m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

            for (auto& [id, Module] : m_kAliveInstMap)
            {
                if (!Module)
                    continue;

                Entity ent{};
                ent.VID = id;

                ent.x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
                ent.y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
                ent.z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);

                ent.ModelX = *(float*)((BYTE*)Module + metin2client::C_ModelCoordX);
                ent.ModelY = *(float*)((BYTE*)Module + metin2client::C_ModelCoordY);
                ent.ModelZ = *(float*)((BYTE*)Module + metin2client::C_ModelCoordZ);

                ent.IsLvl = *(int*)((BYTE*)Module + metin2client::C_Lvl);

                ent.BodyRecoil = *(int*)((BYTE*)Module + metin2client::C_BodyRecoil);

                ent.IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);

                ent.VID = *(DWORD*)((BYTE*)Module + metin2client::C_VIDS);

                ent.Rotation = *(float*)((BYTE*)Module + metin2client::C_IsRotation);

                ent.SkilRotation = *(float*)((BYTE*)Module + metin2client::C_IsSkillRotation);

                ent.IsCharRotation = *(float*)((BYTE*)Module + metin2client::C_IsCharRotation);

                ent.Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);

                ent.IsChar = *(int*)((BYTE*)Module + metin2client::C_IsCharacterType);

                char* namePtr = (char*)((BYTE*)Module + metin2client::C_Name);
                strncpy(ent.Name, namePtr, sizeof(ent.Name) - 1);
                ent.Name[sizeof(ent.Name) - 1] = '\0';

                result.push_back(ent);
            }
        }
        else {
            std::cout << /*[-] entityManager + 0x20 geçersiz adres.*/XorStrTR<0x47, 41, 0x2CAE8000>("\x1C\x65\x14\x6A\x2E\x22\x39\x27\x3B\x29\x1C\x33\x3D\x35\x32\x33\x25\x78\x72\x7A\x6B\x24\x6F\x6E\x7F\x07\x04\x85\x06\x16\x16\x0F\x1D\x48\x08\x0E\x19\x09\x1E\x40" + 0x2CAE8000).s << std::endl;
        }
    }
    else {
        std::cout << /*[-] base + 0x11AEE5C geçersiz.*/XorStrTR<0x47, 31, 0x2CAE8000>("\x1C\x65\x14\x6A\x29\x2D\x3E\x2B\x6F\x7B\x71\x62\x2B\x65\x64\x17\x12\x1D\x6C\x19\x7B\x3B\x38\xB9\x3A\x12\x12\x0B\x19\x4A" + 0x2CAE8000).s << std::endl;
    }

    return result;
}


float Dist(float x, float y, float Px, float Py) { return sqrt((Px - x) * (Px - x) + (Py - y) * (Py - y)); }
template <typename T> int Sign(T val) {
    return (T(0) < val) - (val < T(0));
}
template <typename T>
int InverseSign(T val) {
    return (val < T(0)) - (T(0) < val);
}


bool local = true;
bool entity = true;
bool Writes = true;
static bool knockBackLastState = false;
static bool AllesPlayer = false;
bool AutoKeyDown = false;
int counter = 0;
DWORD WINAPI MemoryManager(LPVOID param) {
    if (!PointModule)
    {
        metin2client = (DWORD)GetModuleHandleA(NULL);
        PointModule = TRUE;
    }
    while (true)
    {
        try
        {
            metin2client::Globals();
            /*metin2client::pCItemManagerInstance = sdk_client + metin2client::C_CItemManager;
            if (metin2client::pCItemManagerInstance != NULL)
            {
                metin2client::iCItemManagerInstance =*reinterpret_cast<DWORD*>(metin2client::pCItemManagerInstance);
            }
            metin2client::pCPythonItemInstance = sdk_client + metin2client::C_PythonItem;
            if (metin2client::pCPythonItemInstance != NULL)
            {
                metin2client::iCPythonItemInstance = *reinterpret_cast<DWORD*>(metin2client::pCPythonItemInstance);
            }*/
            for (int i = 0; i < 331; i += 4) {
                // ======================================== LocalPlayer ========================================//
                if (local)
                {
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_CoordsX, c_players.x);
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_CoordsY, c_players.y);
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_CoordsZ, c_players.z);
                }
                if (local)
                {
                    driver::ReadMemory<int>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_IsDead, c_players.IsDead);
                }
                if (local)
                {
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_IsRotation, c_players.Rotation);
                }
                if (local)
                {
                    driver::ReadMemory<int>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_IsCharacterType, c_players.IsChar);
                }
                if (local)
                {
                    driver::ReadMemory<DWORD>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_VIDS, c_players.VID);
                }
                // BannedVID
                if (local)
                {
                    driver::Banned<DWORD>(metin2client, metin2client::C_Localfarm, metin2client::C_Localfarm_addr, c_players.BannedVID);
                }
                // BannedVID
                if (local)
                {
                    driver::ReadMemory<int>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_BodyRecoil, c_players.BodyRecoil);
                }
                if (local)
                {
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_ModelVectorX, c_players.C_ModelVectorX);
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_ModelVectorY, c_players.C_ModelVectorY);
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_ModelVectorZ, c_players.C_ModelVectorZ);
                }
                if (local)
                {
                    driver::ReadMemory<float>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_IsSkillRotation, c_players.SkilRotation);
                }

                if (local)
                {
                    driver::ReadMemory<char[64]>(metin2client, metin2client::C_LocalPlayer, metin2client::C_LocalAddr, metin2client::C_Name, c_players.Name);
                }
   
                // ======================================== Writes ========================================//
                if (Writes)
                {
                    if (Keyboard::WSCheat)
                    {
                        // Yapıldı!!
                        if (Keyboard::C_SkillRange)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_SkillRange;
                            int values = 1;
                            ReadMemory<int>(Feature, &values, true);
                        }
                        // Yapıldı!!
                        if (Keyboard::C_AutoTarget)
                        {
                            *(int*)Ez_memory::Adres(Ez_game_base + metin2client::C_Localfarm, { metin2client::C_AutoTarget }) = 3;
                        }
                        // Yapıldı!!
                        if (Keyboard::C_Liberated)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_IsDead;
                            int values = 0;
                            ReadMemory<int>(Feature, &values, true);
                        }
                        // Yapıldı!!
                        if (Keyboard::C_Typer)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_IsRadarType;
                            int values = 2;
                            ReadMemory<int>(Feature, &values, true);
                        }
                    }
                    if (Keyboard::InstanceRefresh)
                    {
                        if (Keyboard::InstRefresh)
                        {
                            engine::InstanceRefresh();
                        }
                    }
                    if (Keyboard::HorseChanger)
                    {
                        if (Keyboard::HorseChanger_Ninja) // Ninja
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_Horse;
                            int a = 4;
                            ReadMemory<int>(Feature, &a, true);
                        }
                        if (Keyboard::HorseChanger_War == 1) //Savascı
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_Horse;
                            int a = 3;
                            ReadMemory<int>(Feature, &a, true);
                        }
                        if (Keyboard::HorseChanger_Shaman) // Saman
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_Horse;
                            int a = 2;
                            ReadMemory<int>(Feature, &a, true);
                        }
                        if (Keyboard::HorseChanger_Sura) // Sura
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_Horse;
                            int a = 7;
                            ReadMemory<int>(Feature, &a, true);
                        }
                    }
                    if (Keyboard::AntiFly)
                    {
                        if (Keyboard::AntiFly_Player)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_BodyRecoil;
                            int Fly = 1;
                            ReadMemory<int>(Feature, &Fly, true);
                        }
                    }
                    if (Keyboard::AutoAttacks)
                    {
                        if (Keyboard::C_AutoSpaceV2)
                        {
                            *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + 0x30290)) = 32768;
                        }
                        if (Keyboard::C_AutoRotation)
                        {
                            //engine::InstanceRefresh();

                            uintptr_t Feature = ReadMemory<uintptr_t>(
                                ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer)
                                + metin2client::C_LocalAddr
                                ) + metin2client::C_IsCharRotation;

                            float step = 10.4674683f;
                            for (float val = 0.0f; val <= 360.0f; val += step)
                            {
                                ReadMemory<float>(Feature, &val, true);
                                Sleep(30);
                            }


                        }

                    }

                    if (Keyboard::AttackSpeed)
                    {
                        if (Keyboard::C_AttackSpeed)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_AttackSpeed;
                            ReadMemory<float>(Feature, &Keyboard::C_AttackSpeedValue, true);
                        }
                        if (Keyboard::C_SwordRange)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_SwordRange;
                            ReadMemory<float>(Feature, &Keyboard::C_AttackSpeedValue, true);
                        }
                    }

                    if (Keyboard::MovementSpeed)
                    {
                        if (Keyboard::C_MovementSpeed)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_MovementSpeed;
                            ReadMemory<float>(Feature, &Keyboard::C_MovementSpeedValue, true);
                        }
                    }

                    if (Keyboard::MovementSpeed)
                    {
                        if (Keyboard::C_LegitSpeed)
                        {
                            uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_AttackAnimationSpeed;
                            ReadMemory<float>(Feature, &Keyboard::C_MovementSpeedValue, true);
                        }
                    }

                    if (Keyboard::MovementSpeed)
                    {
                        if (Keyboard::C_ModelVector)
                        {
                            uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_ModelVectorX;
                            float X = 0;
                            ReadMemory<float>(Feature_X, &X, true);
                            uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_ModelVectorY;
                            float Y = 0;
                            ReadMemory<float>(Feature_Y, &Y, true);
                            uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_ModelVectorZ;
                            float Z = 0;
                            ReadMemory<float>(Feature_Z, &Z, true);
                        }
                    }
                }
            }
            Sleep(1);
        }
        catch (...)
        {

        }
    }
}





// ============================================== name changer ============================================== //

static char C_SetPlayerName[65] = "";
static char C_SetPlayerMessage[160] = "";
float Wisp(Vector3 p1, Vector3 p2)
{
    return sqrtf((p1.x - p2.x) * (p1.x - p2.x) +
        (p1.y - p2.y) * (p1.y - p2.y) +
        (p1.z - p2.z) * (p1.z - p2.z));
}
DWORD WINAPI Set_NameChanger(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::Name)
            {
                if (Keyboard::C_NameChanger)
                {
                    uintptr_t localPlayerPtr = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                    uintptr_t localAddrPtr = ReadMemory<uintptr_t>(localPlayerPtr + metin2client::C_LocalAddr);
                    uintptr_t Feature_X = localAddrPtr + metin2client::C_Name;
                    char nameBuffer[64];
                    snprintf(nameBuffer, sizeof(nameBuffer), "德瓦德瓦德 | %05d", counter++);
                    memcpy((void*)Feature_X, nameBuffer, strlen(nameBuffer) + 1);
                    if (counter > 99999)
                        counter = 0;
                }
                if (Keyboard::C_NameApathe)
                {
                    uintptr_t localPlayerPtr = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                    if (!localPlayerPtr)
                        continue;

                    uintptr_t localAddrPtr = ReadMemory<uintptr_t>(localPlayerPtr + metin2client::C_LocalAddr);
                    if (!localAddrPtr)
                        continue;

                    uintptr_t Feature_X = localAddrPtr + metin2client::C_Name;

                    const char* newName = /*Apathe*/XorStrTR<0x47, 7, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29" + 0x2CAE8000).s;
                    memcpy(reinterpret_cast<void*>(Feature_X), newName, strlen(newName) + 1);
                }
                if (Keyboard::C_Lalaker1)
                {
                    uintptr_t localPlayerPtr = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                    if (!localPlayerPtr)
                        continue;

                    uintptr_t localAddrPtr = ReadMemory<uintptr_t>(localPlayerPtr + metin2client::C_LocalAddr);
                    if (!localAddrPtr)
                        continue;

                    uintptr_t Feature_X = localAddrPtr + metin2client::C_Name;

                    const char* newName = /*Lalaker1*/XorStrTR<0x47, 9, 0x2CAE8000>("\x0B\x29\x25\x2B\x20\x29\x3F\x7F" + 0x2CAE8000).s;
                    memcpy(reinterpret_cast<void*>(Feature_X), newName, strlen(newName) + 1);
                }

                // ======================================= Whisper =======================================

                if (Keyboard::C_WhisperSpawm)
                {
                    engine::NetworkStreamSendWhisperPacket(C_SetPlayerName, C_SetPlayerMessage);
                }
                if (Keyboard::C_WhisperEveryoneSpawm)
                {
                    Vector3 oldPosition;
                    engine::GetMainGetPixelPosition(&oldPosition);

                    for (const auto& ent : GetEntities())
                    {
                        if (ent.Type == 6) // Oyuncu
                        {
                            engine::NetworkStreamSendWhisperPacket(ent.Name, C_SetPlayerMessage);
                        }
                    }
                }
                if (Keyboard::C_WhisperArea)
                {
                    Vector3 myPos = { c_players.x, c_players.y, c_players.z };

                    for (const auto& target : GetEntities())
                    {
                        if (target.Type == 6)
                        {
                            Vector3 targetPos = { target.x, target.y, target.z };
                            float dist = Wisp(myPos, targetPos);
                            if (dist > Keyboard::C_WhisperDistance)
                                continue;

                            engine::NetworkStreamSendWhisperPacket(target.Name, C_SetPlayerMessage);
                        }

                    }
                }
            }
        }
        catch (...)
        {

        }
        Sleep(Keyboard::C_WhisperSpeed);
    }
    return true;
}

// ============================================== pickup ============================================== //



bool ilk_yazılan = false;
bool ikinci_yazılan = false;
DWORD WINAPI Set_AutoPickUps(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::AutoPickUp)
            {
                if (Keyboard::AutoPickUp)
                {
                    if (Keyboard::C_AutoLootV1)
                    {
                        engine::PickCloseItem();
                    }
                    if (Keyboard::C_AutoLootV2)
                    {
                        if (!ilk_yazılan)
                        {
                            *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + 0x30283)) = 32768;
                            ilk_yazılan = true;
                            ikinci_yazılan = false;
                        }
                        Sleep(1);
                        if (ilk_yazılan && !ikinci_yazılan)
                        {
                            *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + 0x30283)) = 0;
                            ikinci_yazılan = true;
                            ilk_yazılan = false;
                        }
                    }
                }
                if (Keyboard::AutoPickUp)
                {
                    if (Keyboard::C_RangeLoot)
                    {
                        auto groundItemList = engine::GetGroundItemList();
                        if (groundItemList.empty())
                        {
                            Sleep(300);
                            continue;
                        }

                        std::string playerName = engine::InstanceBaseGetNameString(sdk::GetMainActorPtr());

                        Vector3 playerPos = { c_players.x, -c_players.y, c_players.z };

                        for (const auto& it : groundItemList)
                        {
                            DWORD itemVID = it.first;
                            TGroundItemInstance* item = it.second;

                            if (!item)
                                continue;

                            if (!item->stOwnership.empty() &&
                                item->stOwnership != playerName)
                                continue;

                            Vector3 itemPos = { item->v3EndPosition.x,-item->v3EndPosition.y,item->v3EndPosition.z };

                            float distance = engine::CountDistanceTwoPoints(playerPos.x, playerPos.y, itemPos.x, itemPos.y);

                            if (fabs(itemPos.z - playerPos.z) > 200.f)
                                continue;

                            if (distance < 5000.f)
                            {
                                Vector3 oldPosition = playerPos;
                                Vector3 newPosition = itemPos;

                                size_t attackCount = 0;
                                std::vector<Vector3> distancePoints = engine::DivideTwoPointsByDistance(900.f, oldPosition, newPosition);

                                for (const auto& point : distancePoints)
                                {
                                    if (server_selectors == 1) { // Celestial World
                                        engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(point.x, point.y, point.z), c_players.Rotation, 0, 0, 0);
                                    }
                                    else if (server_selectors == 10) { // Alves2
                                        engine::NetworkStreamAlves2SendCharacterState(Vector3(point.x, point.y, point.z), c_players.Rotation, 0, 0);
                                    }
                                    else {
                                        engine::NetworkStreamSendCharacterStatePacket(Vector3(point.x, point.y, point.z), c_players.Rotation, 0, 0);
                                    }
                                }


                                if (g_SelectedItemIDs.empty())
                                {
                                    engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                    continue;
                                }

                                if (std::find(g_SelectedItemIDs.begin(), g_SelectedItemIDs.end(), item->dwVirtualNumber) != g_SelectedItemIDs.end())
                                {
                                    engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                }
                                // engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                distancePoints = engine::DivideTwoPointsByDistance(900.f, newPosition, oldPosition);

                                for (const auto& point : distancePoints)
                                {
                                    if (server_selectors == 1) { // Celestial World
                                        engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(point.x, point.y, point.z), c_players.Rotation, 0, 0, 0);
                                    }
                                    else if (server_selectors == 10) { // Alves2
                                        engine::NetworkStreamAlves2SendCharacterState(Vector3(point.x, point.y, point.z), c_players.Rotation, 255, 2);
                                    }
                                    else {
                                        engine::NetworkStreamSendCharacterStatePacket(Vector3(point.x, point.y, point.z), c_players.Rotation, 0, 0);
                                    }
                                }

                                attackCount++;

                            }
                        }
                    }
                    if (Keyboard::C_AutoLootv3)
                    {
                        auto groundItemList = engine::GetGroundItemList();
                        if (groundItemList.empty())
                        {
                            Sleep(1000);
                            continue;
                        }

                        std::string playerName = engine::InstanceBaseGetNameString(sdk::GetMainActorPtr());
                        Vector3 playerPos = { c_players.x, -c_players.y, c_players.z };

                        for (const auto& it : groundItemList)
                        {
                            DWORD itemVID = it.first;
                            TGroundItemInstance* item = it.second;

                            if (!item)
                                continue;

                            if (!item->stOwnership.empty() && item->stOwnership != playerName)
                                continue;

                            Vector3 itemPos = { item->v3EndPosition.x, -item->v3EndPosition.y, item->v3EndPosition.z };
                            float distance = engine::CountDistanceTwoPoints(playerPos.x, playerPos.y, itemPos.x, itemPos.y);

                            if (fabs(itemPos.z - playerPos.z) > 200.f)
                                continue;

                            if (distance < 300.f)
                            {
                                if (Keyboard::C_CollectLoot)
                                {
                                    engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                }
                                if (g_SelectedItemIDs.empty())
                                {
                                    engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                    continue;
                                }

                                if (std::find(g_SelectedItemIDs.begin(), g_SelectedItemIDs.end(), item->dwVirtualNumber) != g_SelectedItemIDs.end())
                                {
                                    engine::NetworkStreamSendItemPickUpPacket(itemVID);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (...)
        {

        }
        Sleep(1);
    }
    return true;
}

// ============================================== Mobber ============================================== //

DWORD WINAPI Set_AutoPelerin(HMODULE hModule)
{
    struct KeyToggler
    {
        DWORD offset;
        bool toggle = false;
        DWORD lastTick = 0;

        void Update(bool isKeyPressed)
        {
            if (isKeyPressed)
            {
                DWORD currentTick = GetTickCount();
                if (currentTick - lastTick > 100)
                {
                    toggle = !toggle;
                    *(DWORD*)((DWORD)GetModuleHandleA("DINPUT8.dll") + offset) = toggle ? 128 : 0;
                    lastTick = currentTick;
                }
            }
            else
            {
                *(DWORD*)((DWORD)GetModuleHandleA("DINPUT8.dll") + offset) = 0;
                toggle = false;
            }
        }
    };


    static KeyToggler slot1{ Client->Key->KeyBoard_Slot1 };
    static KeyToggler slot2{ Client->Key->KeyBoard_Slot2 };
    static KeyToggler slot3{ Client->Key->KeyBoard_Slot3 };
    static KeyToggler slot4{ Client->Key->KeyBoard_Slot4 };
    static KeyToggler f1{ Client->Key->KeyBoard_F1 };
    static KeyToggler f2{ Client->Key->KeyBoard_F2 };
    static KeyToggler f3{ Client->Key->KeyBoard_F3 };
    static KeyToggler f4{ Client->Key->KeyBoard_F4 };

    while (true)
    {
        try
        {

            if (Keyboard::Mobber)
            {
                slot1.Update(Keyboard::C_Slot1);
                slot2.Update(Keyboard::C_Slot2);
                slot3.Update(Keyboard::C_Slot3);
                slot4.Update(Keyboard::C_Slot4);
                f1.Update(Keyboard::C_F1);
                f2.Update(Keyboard::C_F2);
                f3.Update(Keyboard::C_F3);
                f4.Update(Keyboard::C_F4);
            }
            else
            {
                slot1.Update(false);
                slot2.Update(false);
                slot3.Update(false);
                slot4.Update(false);
                f1.Update(false);
                f2.Update(false);
                f3.Update(false);
                f4.Update(false);
            }
        }
        catch (...) {}

        Sleep(100);
    }
}

// ============================================== Wait Range ============================================== //



//DWORD WINAPI AutoRevive(HMODULE hModule)
//{
//    DWORD ölüm = 0;
//    while (true)
//    {
//        if (AutoRevives == 1)
//        {
//          
//        }
//        Sleep(100);
//    }
//}

float CalculateDistance(Vector3 p1, Vector3 p2)
{
    return sqrtf((p1.x - p2.x) * (p1.x - p2.x) +
        (p1.y - p2.y) * (p1.y - p2.y) +
        (p1.z - p2.z) * (p1.z - p2.z));
}
bool iptal = true;

//typedef void(__thiscall* DirectEnter)(DWORD This, UINT uChrSlotIndex);
//DirectEnter Enter = (DirectEnter)((sdk_client + 0x96BE80));

//void assembly(const Vector3& c_rkPPosDst, float fDstRot, UINT eFunc, UINT uArg)
//{
//    DWORD NetWorkStream = *(DWORD*)(sdk_client + 0x2BE46B4); // doğru
//    DWORD aaaaa = (sdk_client + 0x23CC370);
//    _asm
//    {
//        mov ecx, NetWorkStream // doğru
//        push uArg // doğru
//        push eFunc // doğru
//        push fDstRot // doğru çalısıyor sorunsuz
//        mov eax, c_rkPPosDst // suan doğru 
//        push eax // doğrı
//        call aaaaa // doğru
//    }
//}


bool Fun = true;
struct OrbitData {
    float angle;
    float speed;
    float radius;
};

std::vector<OrbitData> orbits;
static bool lastRangeWait = false;
static DWORD lastToggleTime = 0;
static bool extrawait = false;
bool DumyDmg = true;
bool NewRange = true;
bool NewStateRange = true;
DWORD WINAPI Set_WaitRange(HMODULE hModule)
{
    DWORD ölüm = 0;
    bool sendAttack = true;
    while (true)
    {
        try
        {
            if (Keyboard::C_DumyDamage)
            {
                auto entities = GetEntities();
                int closestIndex = -1;
                float closestDist = FLT_MAX;

                for (int i = 0; i < entities.size(); ++i)
                {
                    const auto& ent = entities[i];

                    if (ent.Type != 6 || ent.IsDead != 0)
                        continue;

                    if (ent.x == c_players.x && ent.y == c_players.y && ent.z == c_players.z)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                    if (dist < Keyboard::C_ClickToTeleport_Distance && dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }

                for (int i = 0; i < entities.size(); ++i)
                {
                    const auto& ent = entities[i];

                    if (ent.Type != 0 || ent.IsDead != 0)
                        continue;

                    if (ent.x == c_players.x && ent.y == c_players.y && ent.z == c_players.z)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                    if (dist >= Keyboard::C_WaitSpeed)
                        continue;

                    Vector3 EntityTarget = { ent.x, ent.y, ent.z };

                    float radius = 0.0f;
                    float rdn = -DEG2RAD(c_players.Rotation + Angle);

                    float offsetX = cos(rdn) * radius;
                    float offsetY = sin(rdn) * radius;

                    float TargetX, TargetY, TargetZ;

                    if (i == closestIndex)
                    {
                        float closerRadius = 30.0f;
                        float closerOffsetX = cos(rdn) * closerRadius;
                        float closerOffsetY = sin(rdn) * closerRadius;

                        TargetX = EntityTarget.x + closerOffsetX;
                        TargetY = EntityTarget.y + closerOffsetY;
                        TargetZ = EntityTarget.z;
                    }
                    else
                    {
                        TargetX = EntityTarget.x + offsetX;
                        TargetY = EntityTarget.y + offsetY;
                        TargetZ = EntityTarget.z;
                    }

                    Vector3 oldPosition;
                    engine::GetMainGetPixelPosition(&oldPosition);

                    Vector3 newpos = { TargetX, TargetY, TargetZ };


                    auto Teleport = [&](const Vector3& start, const Vector3& end)
                    {
                        std::vector<Vector3> dist = engine::DivideTwoPointsByDistance(5, start, end);

                        for (auto& point : dist)
                        {
                            engine::SendCharacterModel(point);
                            engine::NetworkStreamAlves2SendCharacterState(point, ent.Rotation, 0, 0);
                        }
                        engine::SendCharacterModel(end);
                        engine::NetworkStreamAlves2SendCharacterState(end, ent.Rotation, 0, 0);
                    };
                    Teleport(oldPosition, newpos);
                    engine::NetworkStreamSendAttackPacket(0, ent.VID);
                    Teleport(newpos, oldPosition);
                }

                Angle += 15.0f;
                if (Angle >= 360.0f)
                    Angle = 0.0f;
            }
            if (Keyboard::WaitHack)
            {
                auto entities = GetEntities();

                size_t EntityCout = entities.size();

                if (entities.empty())
                    continue;

                if (Keyboard::C_DieTimeEnable)
                {
                    bool isRangeWaitActive = Keyboard::C_RangeWait;
                    bool isDead = (c_players.IsDead != 0);
                    if (isDead)
                    {
                        if (isRangeWaitActive)
                            Keyboard::C_RangeWait = false;

                        if (ölüm == 0)
                        {
                            ölüm = GetTickCount64();
                        }
                        else if (GetTickCount64() - ölüm >= static_cast<ULONGLONG>(Keyboard::C_DieTime))
                        {
                            if (!isDead)
                            {
                                Keyboard::C_RangeWait = true;
                            }
                            ölüm = 0;
                        }
                    }
                    else
                    {
                        if (!Keyboard::C_RangeWait)
                            Keyboard::C_RangeWait = true;
                        ölüm = 0;
                    }
                }

               /* if (Keyboard::C_RangeWait) 
                {
                    DWORD now = GetTickCount();

                    if (now - lastToggleTime >= 10)
                    {
                        extrawait = !extrawait;
                        lastToggleTime = now;
                    }
                }
                else
                {
                    extrawait = false;
                }*/

                if (Keyboard::C_RangeWait)
                {
                    Vector3 oldPosition;
                    engine::GetMainGetPixelPosition(&oldPosition);

                    auto entities = GetEntities();
                    size_t EntityCount = entities.size();

                    const size_t MAX_ATTACKS_PER_FRAME = 10;
                    size_t attackCount = 0;

                    for (const auto& ent : entities)
                    {
                        if (attackCount >= MAX_ATTACKS_PER_FRAME)
                            break;

                        if (ent.VID == 0 || (ent.x == 0.f && ent.y == 0.f && ent.z == 0.f))
                            continue;

                        bool Type = (Keyboard::C_WaitMonster && ent.Type == 0) ||
                            (Keyboard::C_WaitStone && ent.Type == 2) ||
                            (Keyboard::C_WaitOther && ent.Type == 6);

                        if (!Type)
                            continue;

                        Vector3 newPosition = { ent.x, -ent.y, ent.z };
                        float distance = CalculateDistance(oldPosition, newPosition);

                        if (distance > Keyboard::C_WaitSpeed)
                            continue;

                        if (EntityCount >= 0)
                        {
                            if (Keyboard::C_RangeWait)
                            {
                                if (ent.IsDead == 0)
                                {
                                    if (NewStateRange)
                                    {
                                        vector< Vector3> distancePoints = engine::DivideTwoPointsByDistance(900, oldPosition, newPosition);
                                        int i = 0;
                                        for (vector< Vector3>::iterator it = distancePoints.begin(); it != distancePoints.end(); ++it)
                                        {
                                            if (server_selectors == 1) { // Celestial World
                                                engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0, 0);
                                            }
                                            else if (server_selectors == 10) { // Alves2
                                                engine::NetworkStreamAlves2SendCharacterState(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0);
                                            }
                                            else {
                                                engine::NetworkStreamSendCharacterStatePacket(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0);
                                            }
                                            i++;
                                        }
                                    }
                                    engine::NetworkStreamSendAttackPacket(0, ent.VID);

                                    if (NewStateRange)
                                    {
                                        vector< Vector3> distancePoints = engine::DivideTwoPointsByDistance(900, newPosition, oldPosition);
                                        int i = 0;
                                        for (vector< Vector3>::iterator it = distancePoints.begin(); it != distancePoints.end(); ++it)
                                        {

                                            if (server_selectors == 1) { // Celestial World
                                                engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0, 0);
                                            }
                                            else if (server_selectors == 10) { // Alves2
                                                engine::NetworkStreamAlves2SendCharacterState(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0);
                                            }
                                            else {
                                                engine::NetworkStreamSendCharacterStatePacket(Vector3(it->x, it->y, it->z), ent.Rotation, 0, 0);
                                            }
                                            i++;
                                        }
                                    }
                                    //std::vector<Vector3> distancePoints = engine::DivideTwoPointsByDistance(900, oldPosition, newPosition);
                                    //for (const auto& point : distancePoints)
                                    //{
                                    //    if (server_selectors == 1) { // Celestial World
                                    //        engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0, 0);
                                    //    }
                                    //    else if (server_selectors == 10) { // Alves2
                                    //        engine::NetworkStreamAlves2SendCharacterState(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
                                    //    }
                                    //    else {
                                    //        engine::NetworkStreamSendCharacterStatePacket(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
                                    //    }
                                    //}
                                    //engine::NetworkStreamSendAttackPacket(0, ent.VID);
                                    //distancePoints = engine::DivideTwoPointsByDistance(900, newPosition, oldPosition);
                                    //for (const auto& point : distancePoints)
                                    //{
                                    //    if (server_selectors == 1) { // Celestial World
                                    //        engine::NetworkStreamCelestialWorldSendCharacterState(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0, 0);
                                    //    }
                                    //    else if (server_selectors == 10) { // Alves2
                                    //        engine::NetworkStreamAlves2SendCharacterState(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
                                    //    }
                                    //    else {
                                    //        engine::NetworkStreamSendCharacterStatePacket(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
                                    //    }
                                    //}
                                    //attackCount++;
                                }
                                else if (ent.IsDead == 1)
                                {
                                    DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                                    if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                                    {
                                        continue;
                                    }
                                    DWORD entityManager = *CharacterManager;
                                    if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                                    {
                                        continue;
                                    }
                                    PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
                                    for (auto& [id, Module] : m_kAliveInstMap)
                                    {
                                        if (!Module)
                                            continue;
                                        if (id == ent.VID)
                                        {
                                            *(float*)((BYTE*)Module + metin2client::C_CoordsX) = 999999999999999.0f;
                                            *(float*)((BYTE*)Module + metin2client::C_CoordsY) = 999999999999999.0f;
                                            *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = 999999999999999.0f;
                                            *(int*)((BYTE*)Module + metin2client::C_IsDead) = 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (Keyboard::C_ProdDamage)
                {

                    DWORD VIDS = engine::GetTargetVID();
                    if (VIDS != 0)
                    {
                        engine::NetworkStreamSendAttackPacket(0, VIDS);
                    }
                }
                if (Keyboard::C_WaitNinja)
                {
                    Vector3 myPos = { c_players.x, c_players.y, c_players.z };

                    for (const auto& target : entities)
                    {
                        bool isValidType =
                            (Keyboard::C_WaitMonster && target.Type == 0) ||
                            (Keyboard::C_WaitStone && target.Type == 2) ||
                            (Keyboard::C_WaitOther && target.Type == 6);

                        if (!isValidType)
                            continue;

                        if (target.VID == 0)
                            continue;

                        Vector3 targetPos = { target.x, target.y, target.z };
                        float dist = CalculateDistance(myPos, targetPos);
                        if (dist > Keyboard::C_WaitSpeed)
                            continue;

                        if (target.IsDead == 0)
                        {
                            engine::NetworkStreamSendAddFlyTargetingPacket(target.VID, targetPos);
                            engine::SendShootPacket(0);
                        }
                        else if (target.IsDead == 1)
                        {
                            DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                            if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                            {
                                continue;
                            }
                            DWORD entityManager = *CharacterManager;
                            if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                            {
                                continue;
                            }
                            PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
                            for (auto& [id, Module] : m_kAliveInstMap)
                            {
                                if (!Module)
                                    continue;
                                if (id == target.VID)
                                {
                                    *(float*)((BYTE*)Module + metin2client::C_CoordsX) = 999999999999999.0f;
                                    *(float*)((BYTE*)Module + metin2client::C_CoordsY) = 999999999999999.0f;
                                    *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = 999999999999999.0f;
                                    break;
                                }
                            }
                        }
                    }

                    sendAttack = false;
                }
            }
        }
        catch (...)
        {
        }

        Sleep(Keyboard::C_WaitDistance);
    }

    return 0;
}

DWORD WINAPI Set_WaitDamage(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::WaitHack)
            {
                if (Keyboard::C_WaitDamage)
                {
                    Vector3 oldPosition;
                    engine::GetMainGetPixelPosition(&oldPosition);

                    for (const auto& ent : GetEntities())
                    {
                        if (ent.VID == 0 || (ent.x == 0.f && ent.y == 0.f && ent.z == 0.f))
                            continue;

                        bool isValidType =
                            (Keyboard::C_WaitMonster && ent.Type == 0) ||
                            (Keyboard::C_WaitStone && ent.Type == 2) ||
                            (Keyboard::C_WaitOther && ent.Type == 6);

                        if (!isValidType)
                            continue;

                        Vector3 newPosition = { ent.x, -ent.y, ent.z };
                        float distance = CalculateDistance(oldPosition, newPosition);
                        if (distance > Keyboard::C_WaitSpeed)
                            continue;

                        engine::NetworkStreamSendAttackPacket(0, ent.VID);
                    }
                }
            }
        }
        catch (...)
        {
        }

        Sleep(Keyboard::C_WaitDistance);
    }

    return 0;
}

//float CalculateDistance(Vector3 p1, Vector3 p2)
//{
//    return sqrtf((p1.x - p2.x) * (p1.x - p2.x) +
//        (p1.y - p2.y) * (p1.y - p2.y) +
//        (p1.z - p2.z) * (p1.z - p2.z));
//}
//bool iptal = true;
//DWORD WINAPI Set_WaitRange(HMODULE hModule)
//{
//    while (true)
//    {
//        try
//        {
//            if (!Keyboard::WaitHack)
//            {
//                Sleep(Keyboard::C_WaitSpeed);
//                continue;
//            }
//
//            auto entities = GetEntities();
//            Vector3 oldPosition;
//            engine::GetMainGetPixelPosition(&oldPosition);
//
//            for (const auto& ent : entities)
//            {
//                if (ent.VID == 0 || (ent.x == 0.f && ent.y == 0.f && ent.z == 0.f))
//                    continue;
//
//                bool isValidType =
//                    (Keyboard::C_WaitMonster && ent.Type == 0) ||
//                    (Keyboard::C_WaitStone && ent.Type == 2) ||
//                    (Keyboard::C_WaitOther && ent.Type == 6);
//
//                if (!isValidType)
//                    continue;
//
//                Vector3 newPosition = { ent.x, -ent.y, ent.z };
//                float distance = CalculateDistance(oldPosition, newPosition);
//                if (distance > Keyboard::C_WaitSpeed)
//                    continue;
//
//                bool sendAttack = true;
//
//                if (Keyboard::C_WaitNinja)
//                {
//                    Vector3 myPos = { c_players.x, c_players.y, c_players.z };
//
//                    for (const auto& target : entities)
//                    {
//                        if (target.VID == 0)
//                            continue;
//
//                        Vector3 targetPos = { target.x, target.y, target.z };
//                        float dist = CalculateDistance(myPos, targetPos);
//                        if (dist > Keyboard::C_WaitSpeed)
//                            continue;
//
//                        engine::NetworkStreamSendAddFlyTargetingPacket(target.VID, targetPos);
//                        engine::SendShootPacket(0);
//                    }
//
//                    sendAttack = false;
//                }
//
//                if (Keyboard::C_ProdDamage)
//                {
//                    DWORD VIDS = engine::GetTargetVID();
//                    if (VIDS != 0)
//                    {
//                        engine::NetworkStreamSendAttackPacket(0, VIDS);
//                    }
//                }
//
//                if (Keyboard::C_RangeWait)
//                {
//                    auto distancePoints = engine::DivideTwoPointsByDistance(500, oldPosition, newPosition);
//                    for (const auto& point : distancePoints)
//                    {
//                        engine::NetworkStreamSendCharacterStatePacket(point, 0, 0, 0);
//                    }
//                }
//
//                if (sendAttack)
//                {
//                    engine::NetworkStreamSendAttackPacket(0, ent.VID);
//                }
//            }
//
//        }
//        catch (...)
//        {
//        }
//
//        Sleep(Keyboard::C_WaitDistance);
//        Sleep(60);
//    }
//
//    return 0;
//}

// ============================================== Collision ============================================== //

DWORD WINAPI Set_Collision(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::Collision)
            {
                if (Keyboard::C_Collision_NoClip)
                {
                    uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_NoClip;
                    int value = 1;
                    ReadMemory<int>(Feature, &value, true);
                }
                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                {
                    Sleep(80);
                    continue;
                }
                DWORD entityManager = *CharacterManager;
                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                {
                    Sleep(80);
                    continue;
                }
                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;

                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);

                    if (id == c_players.VID)
                        continue;

                    // Npc Gecme
                    if (Keyboard::C_Collision_NPC && Type == 0)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_NoClip) = 1;
                    }
                    // Oyuncu Gecme
                    if (Keyboard::C_Collision_Player && Type == 6)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_NoClip) = 1;
                    }
                    // Tas Gecme
                    if (Keyboard::C_Collision_Stone && Type == 2)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_NoClip) = 1;
                    }
                }
            }
        }
        catch (...)
        {

        }

        Sleep(80);
    }

    return 0;
}

// ============================================== AntiFly ============================================== //

DWORD WINAPI Set_AntiFly(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::Collision)
            {
                if (Keyboard::C_Collision_NoClip)
                {
                    uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_NoClip;
                    int value = 1;
                    ReadMemory<int>(Feature, &value, true);
                }
                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                {
                    Sleep(80);
                    continue;
                }
                DWORD entityManager = *CharacterManager;
                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                {
                    Sleep(80);
                    continue;
                }
                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;

                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);

                    if (id == c_players.VID)
                        continue;

                    if (Keyboard::AntiFly_Npc && Type == 0)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_BodyRecoil) = 1;
                    }
                    if (Keyboard::AntiFly_Player && Type == 6)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_BodyRecoil) = 1;
                    }
                    if (Keyboard::AntiFly_Stone && Type == 2)
                    {
                        *(int*)((BYTE*)Module + metin2client::C_BodyRecoil) = 1;
                    }
                }
            }
        }
        catch (...)
        {

        }

        Sleep(80);
    }

    return 0;
}

// ============================================== Boost ============================================== //
//void SendCharacterStatePacket(Vector3 pos, float rot, uint32_t eFunc, uint32_t uArg) {
//    DWORD sendstatecall = (sdk_client + 0xDFB60);
//    DWORD netptr = *(DWORD*)(sdk_client + 0x458044);
//    Vector3 _pos;
//
//    try
//    {
//        __asm {
//            mov ecx, netptr
//            push uArg
//            push eFunc
//            push rot
//            lea eax, _pos
//            push eax
//            call sendstatecall
//        }
//    }
//    catch (...)
//    {
//
//    }
//}

static void SendCharacterStatePacket(const Vector3& position, float rotation)
{
    if (server_selectors == 1) { // Celestial World
        engine::NetworkStreamCelestialWorldSendCharacterState(position, rotation, 0, 0, 0);
    }
    else if (server_selectors == 10) { // Alves2
        engine::NetworkStreamAlves2SendCharacterState(position, rotation, 0, 0);
    }
    else {
        engine::NetworkStreamSendCharacterStatePacket(Vector3{ position.x, position.y, position.z }, rotation, 0, 0);
    }
}

bool CHRIsDead = false;
static void BoostV2()
{
    CHRIsDead = (c_players.IsDead == 0);
    if (!CHRIsDead)
        return;
    if (!(GetAsyncKeyState('W') & 0x8000) &&
        !(GetAsyncKeyState('A') & 0x8000) &&
        !(GetAsyncKeyState('S') & 0x8000) &&
        !(GetAsyncKeyState('D') & 0x8000))
    {
        return;
    }
    static int BoostCount = 0;
    Vector3 Position;
    Vector3 newPosition;
    engine::GetMainGetPixelPosition(&Position);
    float rotation = -DEG2RAD(c_players.Rotation - 180);

    int xx = Keyboard::BoostStep * sin(rotation);
    int yy = Keyboard::BoostStep * cos(rotation);
    newPosition.x = Position.x + xx;
    newPosition.y = Position.y - yy;
    newPosition.z = Position.z;

    engine::InstanceBaseSCRIPT_SetPixelPosition(newPosition.x, newPosition.y);
    if (BoostCount >= 1)
    {
        BoostCount = 0;
        if (server_selectors == 1) { // Celestial World
            engine::NetworkStreamCelestialWorldSendCharacterState(newPosition, rotation, 0, 0, 0);
        }
        else if (server_selectors == 10) { // Alves2
            engine::NetworkStreamAlves2SendCharacterState(newPosition, rotation, 0, 0);
        }
        else {
            engine::NetworkStreamSendCharacterStatePacket(Vector3{ newPosition.x, newPosition.y, newPosition.z }, rotation, 0, 0);
        }
    }
    else
    {
  if (server_selectors == 1) { // Celestial World
        engine::NetworkStreamCelestialWorldSendCharacterState(newPosition, rotation, 0, 0, 0);
    }
    else if (server_selectors == 10) { // Alves2
        engine::NetworkStreamAlves2SendCharacterState(newPosition, rotation, 0, 0);
    }
    else {
        engine::NetworkStreamSendCharacterStatePacket(Vector3{ newPosition.x, newPosition.y, newPosition.z }, rotation, 0, 0);
    }
        BoostCount++;
    }
}

//static void MoveToBoost()
//{
//    CHRIsDead = (c_players.IsDead == 0);
//    if (!CHRIsDead)
//        return;
//    if (!(GetAsyncKeyState('W') & 0x8000) &&
//        !(GetAsyncKeyState('A') & 0x8000) &&
//        !(GetAsyncKeyState('S') & 0x8000) &&
//        !(GetAsyncKeyState('D') & 0x8000))
//    {
//        return;
//    }
//    Vector3 currentPosition;
//    engine::GetMainGetPixelPosition(&currentPosition);
//
//    float rotation = -DEG2RAD(c_players.Rotation - 180);
//    float offsetX = Keyboard::BoostStep * sin(rotation);
//    float offsetY = Keyboard::BoostStep * cos(rotation);
//
//    Vector3 newPosition;
//    newPosition.x = currentPosition.x + offsetX;
//    newPosition.y = currentPosition.y - offsetY;
//    newPosition.z = currentPosition.z;
//
//    engine::InstanceBaseSCRIPT_SetPixelPosition(newPosition.x, newPosition.y);
//    SendCharacterStatePacket(newPosition, rotation);
//}


static void BoostV3()
{
    static int BoostCount = 0;

    if (!(GetAsyncKeyState('W') & 0x8000) &&!(GetAsyncKeyState('A') & 0x8000) &&!(GetAsyncKeyState('S') & 0x8000) && !(GetAsyncKeyState('D') & 0x8000))
    {
        return;
    }

    Vector3 Position;
    Vector3 newPosition;
    engine::GetMainGetPixelPosition(&Position);


    float rotation = -DEG2RAD(c_players.Rotation - 180);
    float xx = Keyboard::BoostStep * sin(rotation);
    float yy = Keyboard::BoostStep * cos(rotation);


    newPosition.x = Position.x + xx;
    newPosition.y = Position.y - yy;

    engine::InstanceBaseSCRIPT_SetPixelPosition(newPosition.x, newPosition.y);
    if (BoostCount >= 1)
    {
        BoostCount = 0;
        engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 1, 0);
    }
    else
    {
        engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
        BoostCount++;
    }
    engine::Refresh();
    engine::InstanceRefresh();
}

DWORD WINAPI Set_MoveSpeedBoost(HMODULE hModule)
{
    static float momentum = 0.0f;
    static DWORD lastDashTime = 0;
    const DWORD dashCooldown = 40;
    const float maxMomentum = 1.0f;
    const float momentumIncrease = 0.05f;
    const float momentumDecrease = 0.1f;

    while (true)
    {
        try
        {
            if (Keyboard::Boost)
            {
                if (Keyboard::C_BoostV1)
                {
                    if ((GetAsyncKeyState(VK_SPACE) & 0x8000) == 0)
                    {
                        //MoveToBoost();
                        BoostV2();
                    }  
                }
                if (Keyboard::C_BoostV2 && (GetAsyncKeyState(VK_SHIFT) & 0x8000))
                {
                    if (GetTickCount() - lastDashTime >= dashCooldown)
                    {
                        lastDashTime = GetTickCount();

                        if (momentum < maxMomentum)
                            momentum += momentumIncrease;
                        if (momentum > maxMomentum)
                            momentum = maxMomentum;

 
                        float playerRotation = -DEG2RAD(c_players.Rotation - 180);
                        float deltaX = Keyboard::BoostStep * momentum * sin(playerRotation);
                        float deltaY = Keyboard::BoostStep * momentum * cos(playerRotation);

                        c_players.x += deltaX;
                        c_players.y += deltaY;

                        uintptr_t localAddr = ReadMemory<uintptr_t>(
                            ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) +
                            metin2client::C_LocalAddr
                            );

                        ReadMemory<float>(localAddr + metin2client::C_CoordsX, &c_players.x, true);
                        ReadMemory<float>(localAddr + metin2client::C_CoordsY, &c_players.y, true);
                        ReadMemory<float>(localAddr + metin2client::C_CoordsZ, &c_players.z, true);
                    }
                    else
                    {
                        if (momentum > 0.0f)
                            momentum -= momentumDecrease;
                        if (momentum < 0.0f)
                            momentum = 0.0f;
                    }
                }
            }
            else
            {
                if (momentum > 0.0f)
                    momentum -= momentumDecrease;
                if (momentum < 0.0f)
                    momentum = 0.0f;
            }

            Sleep(Keyboard::BoostSpeed);
        }
        catch (...)
        {
        }
    }
}



// ============================================== Farmbot ============================================== //
std::vector<DWORD> KSBan;
std::vector<DWORD> BannedVIDS;
bool IsBanned(DWORD vid) {
    return std::find(BannedVIDS.begin(), BannedVIDS.end(), vid) != BannedVIDS.end();
}

float rotationAngle = 0.0f;

float SmoothRotate(float current, float target, float speed)
{
    float delta = target - current;
    while (delta > 180.0f) delta -= 360.0f;
    while (delta < -180.0f) delta += 360.0f;

    if (fabs(delta) < speed) return target;
    return current + (delta > 0 ? speed : -speed);
}

static void SetPixelPositionHorse(Vector3 pos)
{
    try
    {
        if (fabs(pos.x) < 0.001f && fabs(pos.y) < 0.001f && fabs(pos.z) < 0.001f)
            return;

        uintptr_t feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
        if (!feature) return;

        feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr1);
        if (!feature) return;

        feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr2);
        if (!feature) return;

        ReadMemory<float>(feature + metin2client::C_HorseCoordX, &pos.x, true);
        float yValue = -fabs(pos.y);
        ReadMemory<float>(feature + metin2client::C_HorseCoordY, &yValue, true);

    }
    catch (const std::exception& e)
    {
    }
    catch (...)
    {
    }
}

static void AutoMoveFarmBot()
{
    static int BoostCount = 0;

    auto entities = GetEntities();

    Vector3 playerPos;
    engine::GetMainGetPixelPosition(&playerPos);

    const Entity* nearestEnemy = nullptr;
    float nearestDist = FLT_MAX;

    for (const auto& ent : entities)
    {
        if (Keyboard::C_NoKs && ent.Type == 6)
        {
            Vector3 otherPlayerPos = { ent.x, -ent.y, ent.z };
            float dx = otherPlayerPos.x - playerPos.x;
            float dy = otherPlayerPos.y - playerPos.y;
            float dist = std::sqrt(dx * dx + dy * dy);

            if (dist <= 1000.0f)
            {
                nearestEnemy = nullptr;
                continue;
            }
        }
        if (ent.Type == 2)
        {
            Vector3 enemyPos = { ent.x, -ent.y, ent.z };
            float dx = enemyPos.x - playerPos.x;
            float dy = enemyPos.y - playerPos.y;
            float dist = std::sqrt(dx * dx + dy * dy);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestEnemy = &ent;
            }
        }
    }

    if (!nearestEnemy)
        return;

    Vector3 enemyPos = { nearestEnemy->x, -nearestEnemy->y, nearestEnemy->z };

    float dx = enemyPos.x - playerPos.x;
    float dy = enemyPos.y - playerPos.y;
    float distance = std::sqrt(dx * dx + dy * dy);
    float rotation = std::atan2(dy, dx);


    Vector3 newPosition = playerPos;
    if (distance > Keyboard::C_UltimateSpeed)
    {
        newPosition.x += Keyboard::BoostStep * std::cos(rotation);
        newPosition.y += Keyboard::BoostStep * std::sin(rotation);
    }
    else
    {
        newPosition = enemyPos;
    }

    if (Keyboard::Bot_Horse)
    {
        SetPixelPositionHorse(newPosition);
        if (distance <= Keyboard::C_UltimateSpeed * 1.5f)
        {
            if (BoostCount >= 1)
            {
                BoostCount = 0;
                engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 1, 0);
            }
            else
            {
                engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
                BoostCount++;
            }
        }
        else
        {
            engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
        }
    }

    if (Keyboard::Bot_Player)
    {
        engine::SetPixelPosition(newPosition);

        if (distance <= Keyboard::C_UltimateSpeed * 1.5f)
        {
            if (BoostCount >= 1)
            {
                BoostCount = 0;
                engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 1, 0);
            }
            else
            {
                engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
                BoostCount++;
            }
        }
        else
        {
            engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
        }
    }
}

//std::vector<Vector3> save_to_rota;
//int current_rota_index = 0;
//DWORD last_save_time = 0;
//DWORD last_move_time = 0;
//const DWORD save_interval = 2000;
//const DWORD move_interval = 25;
//
//void RotaBot()
//{
//    auto Entity = GetEntities();
//    for (const auto& ent : Entity)
//    {
//        Vector3 EnemyPos = { ent.x, -ent.y, ent.z };
//        Vector3 PlayerPos = { c_players.x,-c_players.y,c_players.z };
//
//        if (ent.Type == 2) // Metin2
//        {
//            uintptr_t addr = Ez_memory::Adres(Ez_game_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
//            if (addr)
//            {
//                *(DWORD*)addr = ent.VID;
//            }
//        }
//        engine::InstanceBaseNEW_MoveToDestPixelPositionDirection(&hedef);
//    }
//}

DWORD WINAPI Set_AllFarmbot(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::Bot)
            {
                auto entities = GetEntities();
                if (Keyboard::Bot)
                {
                    if (Keyboard::C_AllSohanBan)
                    {
                        //  Sohan Dagı ch4
                        BannedVIDS.push_back(3743);
                        BannedVIDS.push_back(3733);
                        BannedVIDS.push_back(3742);
                        BannedVIDS.push_back(3728);
                        BannedVIDS.push_back(3730); // 5
                        BannedVIDS.push_back(3722);
                        BannedVIDS.push_back(3744);
                        BannedVIDS.push_back(3723); // 8
                        BannedVIDS.push_back(3746);
                        BannedVIDS.push_back(3731);
                        BannedVIDS.push_back(3729);
                        BannedVIDS.push_back(3739);
                        BannedVIDS.push_back(3724);
                        BannedVIDS.push_back(3727);
                        BannedVIDS.push_back(3726);
                        BannedVIDS.push_back(3740);
                        BannedVIDS.push_back(3725);
                        BannedVIDS.push_back(3741);
                        BannedVIDS.push_back(3712);
                        BannedVIDS.push_back(3707);
                        BannedVIDS.push_back(3722);
                        BannedVIDS.push_back(3709);
                        BannedVIDS.push_back(3723);
                        BannedVIDS.push_back(3708);
                        BannedVIDS.push_back(3725);
                        BannedVIDS.push_back(3739);// 8
                        BannedVIDS.push_back(3738);
                        BannedVIDS.push_back(3748);
                        BannedVIDS.push_back(3717);
                        BannedVIDS.push_back(3737);
                        BannedVIDS.push_back(3718);
                        BannedVIDS.push_back(3719);
                        BannedVIDS.push_back(3720);
                        BannedVIDS.push_back(3736);
                        BannedVIDS.push_back(3735);
                        BannedVIDS.push_back(3721);// 8
                        BannedVIDS.push_back(3734);
                        BannedVIDS.push_back(3738);
                    }
                    if (Keyboard::C_AllOrman)
                    {
                        // Hayelet Orman (CH 4) 
                        BannedVIDS.push_back(5345);
                        BannedVIDS.push_back(5339);
                        BannedVIDS.push_back(5340);
                        BannedVIDS.push_back(5346);
                        BannedVIDS.push_back(5344);
                        BannedVIDS.push_back(5338);
                        BannedVIDS.push_back(5348);
                        BannedVIDS.push_back(5337);
                        BannedVIDS.push_back(5343);
                        BannedVIDS.push_back(5341);
                    }
                    if (Keyboard::C_AllVadiBan)
                    {
                        // Vadi (CH4)
                        BannedVIDS.push_back(3976);
                        BannedVIDS.push_back(3977);
                        BannedVIDS.push_back(3978);
                        BannedVIDS.push_back(3981);
                        BannedVIDS.push_back(3979);
                        BannedVIDS.push_back(3980);
                    }
                    if (Keyboard::C_UltimateFarm)
                    {
                        AutoMoveFarmBot();
                        //RotaBot();
                    }
                    if (Keyboard::C_WalkToFarm)
                    {
                        if (entities.empty())
                        {
                            Sleep(Keyboard::BOT_SPEED);
                            continue;
                        }

                        for (auto it = KSBan.begin(); it != KSBan.end(); )
                        {
                            DWORD banVID = *it;
                            auto found = std::find_if(entities.begin(), entities.end(), [&](const Entity& e) { return e.VID == banVID; });

                            if (found == entities.end())
                            {
                                it = KSBan.erase(it);
                                continue;
                            }

                            float distToPlayer = Dist(found->x, found->y, c_players.x, c_players.y);
                            if (distToPlayer > 500.0f)
                            {
                                it = KSBan.erase(it);
                            }
                            else
                            {
                                ++it;
                            }
                        }

                        float minDistance = FLT_MAX;
                        int selectedIndex = -1;

                        for (size_t i = 0; i < entities.size(); ++i)
                        {
                            const auto& ent = entities[i];

                            if (ent.VID == 0)
                                continue;

                            if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                                continue;
                            if (std::find(KSBan.begin(), KSBan.end(), ent.VID) != KSBan.end())
                                continue;

                            if (ent.IsDead != 0)
                                continue;

                            bool validType = (Keyboard::Bot_Npc && ent.Type == 0) || (Keyboard::Bot_Stone && ent.Type == 2);
                            if (!validType)
                                continue;

                            float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                selectedIndex = (int)i;
                            }
                        }

                        if (selectedIndex != -1)
                        {
                            const Entity& target = entities[selectedIndex];
                            float distToTarget = Dist(target.x, target.y, c_players.x, c_players.y);

                            bool cancelAttack = false;

                            if (Keyboard::C_NoKs && target.Type == 2 && distToTarget > 1000.0f)
                            {
                                for (const auto& other : entities)
                                {
                                    if (other.VID == 0) continue;
                                    if (other.Type != 6) continue;
                                    if (other.IsDead != 0) continue;

                                    float distPlayerToTarget = Dist(other.x, other.y, target.x, target.y);
                                    if (distPlayerToTarget < 1000.0f)
                                    {
                                        if (std::find(KSBan.begin(), KSBan.end(), target.VID) == KSBan.end())
                                            KSBan.push_back(target.VID);
                                        if (std::find(KSBan.begin(), KSBan.end(), other.VID) == KSBan.end())
                                            KSBan.push_back(other.VID);

                                        cancelAttack = true;
                                        break;
                                    }
                                }
                            }

                            if (!cancelAttack)
                            {
                                uintptr_t addr = Ez_memory::Adres(Ez_game_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
                                if (addr)
                                {
                                    *(DWORD*)addr = target.VID;
                                }
                            }
                        }
                    }
                    if (Keyboard::C_LegitRange)
                    {

                        float closestDistance = FLT_MAX;
                        int closestEnemyIndex = -1;

                        for (size_t i = 0; i < entities.size(); ++i) {
                            auto& ent = entities[i];
                            if (IsBanned(ent.VID) || ent.IsDead != 0)
                                continue;

                            float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);

                            bool isTargetType =
                                (Keyboard::Bot_Stone && ent.Type == 2) ||
                                (Keyboard::Bot_Npc && ent.Type == 0);

                            if (isTargetType && dist < Keyboard::BOT_DISTANCE && dist < closestDistance) {
                                closestDistance = dist;
                                closestEnemyIndex = (int)i;
                            }
                        }

                        if (closestEnemyIndex != -1) {
                            auto& enemy = entities[closestEnemyIndex];

                            float dx = enemy.x - c_players.x;
                            float dy = enemy.y - c_players.y;
                            c_players.Rotation = atan2f(dy, dx) * (180.0f / 3.14159265f);

                            float stepSize = 180.0f;
                            float targetX = c_players.x + ((fabs(enemy.x - c_players.x) < stepSize) ? (enemy.x - c_players.x) : Sign(enemy.x - c_players.x) * stepSize);
                            float targetY = c_players.y + ((fabs(enemy.y - c_players.y) < stepSize) ? (enemy.y - c_players.y) : Sign(enemy.y - c_players.y) * stepSize);
                            float targetZ = c_players.z + ((fabs(enemy.z - c_players.z) < stepSize) ? (enemy.z - c_players.z) : Sign(enemy.z - c_players.z) * stepSize);

                            if (Keyboard::Bot_Horse)
                            {
                                uintptr_t feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                                feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr1);
                                feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr2);

                                ReadMemory<float>(feature + metin2client::C_HorseCoordX, &targetX, true);
                                ReadMemory<float>(feature + metin2client::C_HorseCoordY, &targetY, true);
                                ReadMemory<float>(feature + metin2client::C_HorseCoordZ, &targetZ, true);
                            }

                            if (Keyboard::Bot_Player)
                            {

                                uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
                                ReadMemory<float>(Feature_X, &targetX, true);
                                uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
                                ReadMemory<float>(Feature_Y, &targetY, true);
                                uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
                                ReadMemory<float>(Feature_Z, &targetZ, true);
                            }

                            //*(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + 0x30290)) = 32768;
                        }
                    }
                    if (Keyboard::C_BotTeleport)
                    {

                        float minDistance = FLT_MAX;
                        Entity* nearestEntity = nullptr;

                        for (auto& ent : entities)
                        {
                            if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                                continue;

                            float legitDistance = Dist(ent.x, ent.y, c_players.x, c_players.y);

                            if (Keyboard::Bot_Stone)
                            {
                                if (legitDistance < Keyboard::BOT_DISTANCE &&
                                    ent.Type == 2 &&
                                    ent.IsDead == 0)
                                {
                                    if (legitDistance < minDistance)
                                    {
                                        minDistance = legitDistance;
                                        nearestEntity = &ent;
                                    }
                                }
                            }

                            if (Keyboard::Bot_Npc)
                            {
                                if (legitDistance < Keyboard::BOT_DISTANCE &&
                                    ent.Type == 0 &&
                                    ent.IsDead == 0)
                                {
                                    if (legitDistance < minDistance)
                                    {
                                        minDistance = legitDistance;
                                        nearestEntity = &ent;
                                    }
                                }
                            }
                        }

                        if (nearestEntity != nullptr)
                        {
                            if (Keyboard::Bot_Horse)
                            {
                                uintptr_t Feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                                Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr1);
                                Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr2);

                                ReadMemory<float>(Feature + metin2client::C_HorseCoordX, &nearestEntity->x, true);
                                ReadMemory<float>(Feature + metin2client::C_HorseCoordY, &nearestEntity->y, true);
                                ReadMemory<float>(Feature + metin2client::C_HorseCoordZ, &nearestEntity->z, true);
                            }
                            if (Keyboard::Bot_Player)
                            {

                                uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
                                ReadMemory<float>(Feature_X, &nearestEntity->x, true);
                                uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
                                ReadMemory<float>(Feature_Y, &nearestEntity->y, true);
                                uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
                                ReadMemory<float>(Feature_Z, &nearestEntity->z, true);
                            }
                            //*(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + 0x30290)) = 32768;
                        }
                    }
                    if (Keyboard::C_StateRange)
                    {

                        for (auto& ent : entities)
                        {
                            if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                                continue;

                            bool validType = (Keyboard::Bot_Npc && ent.Type == 0) || (Keyboard::Bot_Stone && ent.Type == 2);
                            if (!validType)
                                continue;

                            if (validType && ent.IsDead == 0)
                            {
                                float MinimapDistance = Dist(ent.x, ent.y, c_players.x, c_players.y);

                                if (MinimapDistance < Keyboard::BOT_DISTANCE)
                                {
                                    float targetX = ent.x;
                                    float targetY = ent.y;
                                    float targetZ = ent.z;

                                    float offset = Keyboard::BOT_DISTANCE;
                                    float angle = atan2(targetY - c_players.y, targetX - c_players.x);

                                    float newX = targetX - cos(angle) * offset;
                                    float newY = targetY - sin(angle) * offset;
                                    float newZ = targetZ;
                                    // Horse
                                    if (Keyboard::Bot_Horse)
                                    {
                                        uintptr_t FeatureBase = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                                        FeatureBase = ReadMemory<uintptr_t>(FeatureBase + metin2client::C_HorseAddr1);
                                        FeatureBase = ReadMemory<uintptr_t>(FeatureBase + metin2client::C_HorseAddr2);

                                        ReadMemory<float>(FeatureBase + metin2client::C_HorseCoordX, &newX, true);
                                        ReadMemory<float>(FeatureBase + metin2client::C_HorseCoordY, &newY, true);
                                        ReadMemory<float>(FeatureBase + metin2client::C_HorseCoordZ, &newZ, true);
                                    }
                                    if (Keyboard::Bot_Player)
                                    {
                                        uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
                                        ReadMemory<float>(Feature_X, &targetX, true);
                                        uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
                                        ReadMemory<float>(Feature_Y, &targetY, true);
                                        uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
                                        ReadMemory<float>(Feature_Z, &targetZ, true);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    if (Keyboard::C_KillAura)
                    {
                        int closestIndex = -1;
                        float closestDist = FLT_MAX;

                        for (int i = 0; i < entities.size(); i++)
                        {
                            auto& ent = entities[i];

                            if (ent.IsDead != 0) continue;
                            if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end()) continue;

                            bool validType = (Keyboard::Bot_Npc && ent.Type == 0) || (Keyboard::Bot_Stone && ent.Type == 2);
                            if (!validType)
                                continue;

                            float distToEnt = Dist(ent.x, ent.y, c_players.x, c_players.y);
                            if (distToEnt < Keyboard::BOT_DISTANCE && distToEnt < closestDist)
                            {
                                closestDist = distToEnt;
                                closestIndex = i;
                            }
                        }

                        if (closestIndex != -1)
                        {
                            auto& target = entities[closestIndex];

                            float dx = target.x - c_players.x;
                            float dy = target.y - c_players.y;
                            float targetRotation = atan2f(dy, dx) * (180.0f / 3.14159265f);

                            c_players.Rotation = SmoothRotate(c_players.Rotation, targetRotation, 5.0f);

                            float radius = 10;
                            float radian = -DEG2RAD(rotationAngle);
                            float offsetX = cos(radian) * radius;
                            float offsetY = sin(radian) * radius;

                            float newX = target.x + offsetX;
                            float newY = target.y + offsetY;
                            float newZ = target.z;

                            if (Keyboard::Bot_Horse)
                            {
                                uintptr_t featureBase = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
                                featureBase = ReadMemory<uintptr_t>(featureBase + metin2client::C_HorseAddr1);
                                featureBase = ReadMemory<uintptr_t>(featureBase + metin2client::C_HorseAddr2);

                                ReadMemory<float>(featureBase + metin2client::C_HorseCoordX, &newX, true);
                                ReadMemory<float>(featureBase + metin2client::C_HorseCoordY, &newY, true);
                                ReadMemory<float>(featureBase + metin2client::C_HorseCoordZ, &newZ, true);
                            }
                            if (Keyboard::Bot_Player)
                            {
                                uintptr_t localAddr = ReadMemory<uintptr_t>(
                                    ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) +
                                    metin2client::C_LocalAddr);

                                ReadMemory<float>(localAddr + metin2client::C_CoordsX, &newX, true);
                                ReadMemory<float>(localAddr + metin2client::C_CoordsY, &newY, true);
                                ReadMemory<float>(localAddr + metin2client::C_CoordsZ, &newZ, true);
                                ReadMemory<float>(localAddr + metin2client::C_IsRotation, &c_players.Rotation, true);
                            }
                            rotationAngle += 15.0f;
                            if (rotationAngle >= 360.0f) rotationAngle = 0.0f;
                        }
                    }
                    if (Keyboard::StoneBan)
                    {
                        DWORD clickedVID = c_players.BannedVID;

                        if (clickedVID != 0 && std::find(BannedVIDS.begin(), BannedVIDS.end(), clickedVID) == BannedVIDS.end())
                        {
                            BannedVIDS.push_back(clickedVID);
                        }
                    }
                    if (Keyboard::StoneClear)
                    {
                        BannedVIDS.clear();
                        Keyboard::StoneClear = false;
                    }
                    Sleep(Keyboard::BOT_SPEED);
                    continue;
                }
            }
        }
        catch (...)
        {
        
        }

        Sleep(Keyboard::BOT_SPEED); // Bot Hızı
    }

    return 0;
}

// ============================================== Buff Systeam ============================================== //

DWORD WINAPI Set_BuffSysteam(HMODULE hModule)
{
    //OpenConsole();

    while (!Controller::hSlave || !Controller::slave_base)
        Sleep(100);

    static DWORD lastAttack = 0;
    DWORD ölüm = 0;
    while (true)
    {
        try
        {
            auto entities = GetEntities();
            if (Keyboard::BuffBot)
            {
                uintptr_t playerPtr = Controller::CReadProcessMemory<uintptr_t>(Controller::CReadProcessMemory<uintptr_t>(Controller::slave_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr);
                C_Buff.x = Controller::CReadProcessMemory<float>(playerPtr + metin2client::C_ModelVectorX);
                C_Buff.y = Controller::CReadProcessMemory<float>(playerPtr + metin2client::C_ModelVectorY);
                C_Buff.z = Controller::CReadProcessMemory<float>(playerPtr + metin2client::C_ModelVectorZ);
                C_Buff.IsDead = Controller::CReadProcessMemory<int>(playerPtr + metin2client::C_IsDead);
                C_Buff.VID = Controller::CReadProcessMemory<DWORD>(playerPtr + metin2client::C_VIDS);
                C_Buff.IsCharRotation = Controller::CReadProcessMemory<float>(playerPtr + metin2client::C_IsCharRotation);
                C_Buff.IsLvl = Controller::CReadProcessMemory<int>(playerPtr + metin2client::C_Lvl);
                C_Buff.SkilRotation = Controller::CReadProcessMemory<float>(playerPtr + metin2client::C_IsSkillRotation);
                C_Buff.Type = Controller::CReadProcessMemory<int>(playerPtr + metin2client::C_IsRadarType);
            }
            if (Keyboard::C_BuffTarget)
            {

                uintptr_t addr = Controller::AdresSlave(Controller::slave_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
                if (addr)
                {
                    Controller::CWriteProcessMemory<DWORD>(addr, c_players.VID);
                }
            }
            if (Keyboard::C_BuffNoClip)
            {
                uintptr_t playerPtr = Controller::CReadProcessMemory<uintptr_t>(Controller::CReadProcessMemory<uintptr_t>(Controller::slave_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr);
                if (playerPtr)
                {
                    Controller::CWriteProcessMemory<int>(playerPtr + metin2client::C_NoClip, 1);
                }
            }
            if (Keyboard::C_BuffVector)
            {

                uintptr_t playerPtr = Controller::CReadProcessMemory<uintptr_t>(Controller::CReadProcessMemory<uintptr_t>(Controller::slave_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr);
                if (playerPtr)
                {
                    Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_ModelVectorX, 0.0f);
                    Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_ModelVectorY, 0.0f);
                    Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_ModelVectorZ, 0.0f);
                }
            }
            if (Keyboard::C_SkillSlotF1)
            {
                static DWORD lastSkill = 0;
                if (GetTickCount() - lastSkill >= 500)
                {
                    Controller::PowerSell::PowerSellClickSkillSlot(1);
                    lastSkill = GetTickCount();
                }
            }
            if (Keyboard::C_SkillSlotF2)
            {
                static DWORD lastSkill = 0;
                if (GetTickCount() - lastSkill >= 500)
                {
                    Controller::PowerSell::PowerSellClickSkillSlot(2);
                    lastSkill = GetTickCount();
                }
            }
            if (Keyboard::C_SkillSlotF3)
            {
                static DWORD lastSkill = 0;
                if (GetTickCount() - lastSkill >= 500)
                {
                    Controller::PowerSell::PowerSellClickSkillSlot(3);
                    lastSkill = GetTickCount();
                }
            }
            if (Keyboard::C_SkillSlotF4)
            {
                static DWORD lastSkill = 0;
                if (GetTickCount() - lastSkill >= 500)
                {
                    Controller::PowerSell::PowerSellClickSkillSlot(4);
                    lastSkill = GetTickCount();
                }
            }
            if (Keyboard::C_BuffAutoRevive)
            {
                if (C_Buff.IsDead == 1)
                {
                    if (ölüm == 0)
                    {
                        ölüm = GetTickCount();
                    }
                    else if (GetTickCount() - ölüm >= 11000) // 9 saniye
                    {
                        Controller::PowerSell::PowerSellSendChatPacket("/restart_here", 0);
                        ölüm = 0;
                    }
                }
                else
                {
                    ölüm = 0;
                }
            }
            if (Keyboard::C_BuffProDamage)
            {
                DWORD now = GetTickCount();

                if (now - lastAttack >= 100)
                {
                    DWORD VIDS = Controller::PowerSell::PythonPlayerGetTargetVID();
                    if (VIDS != 0)
                    {
                        Controller::PowerSell::PowerSellSendBattleAttack(0, VIDS);
                        lastAttack = now;
                    }
                }
            }
            if (Keyboard::C_BuffWaitDamage)
            {
                Vector3 oldPosition;
                engine::GetMainGetPixelPosition(&oldPosition);

                for (const auto& ent : GetEntities())
                {
                    if (ent.VID == 0 || (ent.x == 0.f && ent.y == 0.f && ent.z == 0.f))
                        continue;

                    bool isValidType =
                        (Keyboard::C_BuffDmgEntity && ent.Type == 0) ||
                        (Keyboard::C_BuffDmgStone && ent.Type == 2) ||
                        (Keyboard::C_BuffDmgOther && ent.Type == 6);

                    if (!isValidType)
                        continue;

                    Vector3 newPosition = { ent.x, -ent.y, ent.z };
                    float distance = CalculateDistance(oldPosition, newPosition);
                    if (distance > 5000.0f)
                        continue;

                    Controller::PowerSell::PowerSellSendBattleAttack(0, ent.VID);
                }
            }
            if (Keyboard::C_BuffWalkToCut)
            {
                if (entities.empty())
                {
                    Sleep(Keyboard::BOT_SPEED);
                    continue;
                }

                for (auto it = KSBan.begin(); it != KSBan.end(); )
                {
                    DWORD banVID = *it;
                    auto found = std::find_if(entities.begin(), entities.end(), [&](const Entity& e) { return e.VID == banVID; });

                    if (found == entities.end())
                    {
                        it = KSBan.erase(it);
                        continue;
                    }

                    float distToPlayer = Dist(found->x, found->y, c_players.x, c_players.y);
                    if (distToPlayer > 500.0f)
                    {
                        it = KSBan.erase(it);
                    }
                    else
                    {
                        ++it;
                    }
                }

                float minDistance = FLT_MAX;
                int selectedIndex = -1;

                for (size_t i = 0; i < entities.size(); ++i)
                {
                    const auto& ent = entities[i];

                    if (ent.VID == 0)
                        continue;

                    if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                        continue;
                    if (std::find(KSBan.begin(), KSBan.end(), ent.VID) != KSBan.end())
                        continue;

                    if (ent.IsDead != 0)
                        continue;

                    bool validType = (Keyboard::C_BuffWalkToEntity && ent.Type == 0) || (Keyboard::C_BuffWalkToStone && ent.Type == 2);
                    if (!validType)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        selectedIndex = (int)i;
                    }
                }

                if (selectedIndex != -1)
                {
                    const Entity& target = entities[selectedIndex];
                    float distToTarget = Dist(target.x, target.y, c_players.x, c_players.y);

                    bool cancelAttack = false;

                    if (Keyboard::C_NoKs && target.Type == 2 && distToTarget > 1000.0f)
                    {
                        for (const auto& other : entities)
                        {
                            if (other.VID == 0) continue;
                            if (other.Type != 6) continue;
                            if (other.IsDead != 0) continue;

                            float distPlayerToTarget = Dist(other.x, other.y, target.x, target.y);
                            if (distPlayerToTarget < 1000.0f)
                            {
                                if (std::find(KSBan.begin(), KSBan.end(), target.VID) == KSBan.end())
                                    KSBan.push_back(target.VID);
                                if (std::find(KSBan.begin(), KSBan.end(), other.VID) == KSBan.end())
                                    KSBan.push_back(other.VID);

                                cancelAttack = true;
                                break;
                            }
                        }
                    }

                    if (!cancelAttack)
                    {
                        uintptr_t addr = Controller::AdresSlave(Controller::slave_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
                        if (addr)
                        {
                            Controller::CWriteProcessMemory<DWORD>(addr, target.VID);
                        }
                    }
                }
            }
            if (Keyboard::C_BuffTeleport)
            {
                float minDistance = FLT_MAX;
                Entity* nearestEntity = nullptr;

                for (auto& ent : entities)
                {
                    if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                        continue;

                    float legitDistance = Dist(ent.x, ent.y, c_players.x, c_players.y);

                    if (Keyboard::C_BuffWalkToStone)
                    {
                        if (legitDistance < 95000.0f && ent.Type == 2 && ent.IsDead == 0)
                        {
                            if (legitDistance < minDistance)
                            {
                                minDistance = legitDistance;
                                nearestEntity = &ent;
                            }
                        }
                    }

                    if (Keyboard::C_BuffWalkToEntity)
                    {
                        if (legitDistance < Keyboard::BOT_DISTANCE &&
                            ent.Type == 0 &&
                            ent.IsDead == 0)
                        {
                            if (legitDistance < minDistance)
                            {
                                minDistance = legitDistance;
                                nearestEntity = &ent;
                            }
                        }
                    }
                }

                if (nearestEntity != nullptr)
                {
                    if (Keyboard::C_BuffTypeHorse)
                    {
                        uintptr_t horseX = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordX });
                        uintptr_t horseY = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordY });
                        uintptr_t horseZ = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordZ });

                        if (horseX) Controller::CWriteProcessMemory<float>(horseX, nearestEntity->x);
                        if (horseY) Controller::CWriteProcessMemory<float>(horseY, nearestEntity->y);
                        if (horseZ) Controller::CWriteProcessMemory<float>(horseZ, nearestEntity->z);
                    }
                    if (Keyboard::C_BuffTypePlayer)
                    {
                        uintptr_t playerPtr = Controller::CReadProcessMemory<uintptr_t>(Controller::CReadProcessMemory<uintptr_t>(Controller::slave_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr);
                        if (playerPtr)
                        {
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsX, nearestEntity->x);
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsY, nearestEntity->y);
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsZ, nearestEntity->z);
                        }
                    }
                }
            }
            if (Keyboard::C_BuffLegit)
            {

                float closestDistance = FLT_MAX;
                int closestEnemyIndex = -1;

                for (size_t i = 0; i < entities.size(); ++i) {
                    auto& ent = entities[i];
                    if (IsBanned(ent.VID) || ent.IsDead != 0)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);

                    bool isTargetType =
                        (Keyboard::C_BuffWalkToStone && ent.Type == 2) ||
                        (Keyboard::C_BuffWalkToEntity && ent.Type == 0);

                    if (isTargetType && dist < 95000.0f && dist < closestDistance) {
                        closestDistance = dist;
                        closestEnemyIndex = (int)i;
                    }
                }

                if (closestEnemyIndex != -1) {
                    auto& enemy = entities[closestEnemyIndex];

                    float dx = enemy.x - C_Buff.x;
                    float dy = enemy.y - C_Buff.y;
                    C_Buff.Rotation = atan2f(dy, dx) * (180.0f / 3.14159265f);

                    float stepSize = 180.0f;
                    float targetX = C_Buff.x + ((fabs(enemy.x - C_Buff.x) < stepSize) ? (enemy.x - C_Buff.x) : Sign(enemy.x - C_Buff.x) * stepSize);
                    float targetY = C_Buff.y + ((fabs(enemy.y - C_Buff.y) < stepSize) ? (enemy.y - C_Buff.y) : Sign(enemy.y - C_Buff.y) * stepSize);
                    float targetZ = C_Buff.z + ((fabs(enemy.z - C_Buff.z) < stepSize) ? (enemy.z - C_Buff.z) : Sign(enemy.z - C_Buff.z) * stepSize);

                    if (Keyboard::C_BuffTypeHorse)
                    {
                        uintptr_t horseX = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordX });
                        uintptr_t horseY = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordY });
                        uintptr_t horseZ = Controller::AdresSlave(Controller::slave_base + metin2client::C_LocalPlayer, { metin2client::C_HorseAddr1, metin2client::C_HorseAddr2, metin2client::C_HorseCoordZ });

                        if (horseX) Controller::CWriteProcessMemory<float>(horseX, targetX);
                        if (horseY) Controller::CWriteProcessMemory<float>(horseY, targetY);
                        if (horseZ) Controller::CWriteProcessMemory<float>(horseZ, targetZ);
                    }

                    if (Keyboard::C_BuffTypePlayer)
                    {
                        uintptr_t playerPtr = Controller::CReadProcessMemory<uintptr_t>(Controller::CReadProcessMemory<uintptr_t>(Controller::slave_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr);
                        if (playerPtr)
                        {
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsX, targetX);
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsY, targetY);
                            Controller::CWriteProcessMemory<float>(playerPtr + metin2client::C_CoordsZ, targetZ);
                        }
                    }
                }
            }
        }
        catch (...)
        {

        }
        Sleep(1);
    }

    CloseHandle(Controller::hSlave);
    Controller::hSlave = nullptr;
    Controller::slave_base = 0;
    FreeConsole();
    return 0;
}

// ============================================== Rota ============================================== //

std::vector<Vector3> save_to_rota;
int current_rota_index = 0;
DWORD last_save_time = 0;
DWORD last_move_time = 0;
const DWORD save_interval = 2000;
const DWORD move_interval = 25;


void RotaKaydet(const std::string& filename)
{
    std::ofstream file(filename);
    if (!file.is_open())
        return;

    for (const auto& pos : save_to_rota)
    {
        file << pos.x << " " << pos.y << " " << pos.z << "\n";
    }

    file.close();
}

Vector3 last_player_pos = { 0.f, 0.f, 0.f };
bool is_moving = false;
const float move_threshold = 1.0f;

bool ileri_git = false;
bool rota_basladi = false;

bool Rota_devam = true;

DWORD WINAPI Set_Rota(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::Route)
            {
                for (int i = 0; i < 231; i += 4)
                {
                    if (Keyboard::C_RouteSave)
                    {
                        RotaKaydet("Route.txt");
                    }
                    Vector3 Player;
                    engine::GetMainGetPixelPosition(&Player);
                    DWORD now = GetTickCount();

                    float dx = Player.x - last_player_pos.x;
                    float dy = Player.y - last_player_pos.y;
                    float dz = Player.z - last_player_pos.z;
                    float dist_sq = dx * dx + dy * dy + dz * dz;

                    is_moving = (dist_sq > move_threshold * move_threshold);
                    last_player_pos = Player;
                    if (Keyboard::C_RouteRecording && is_moving)
                    {
                        if (now - last_save_time > save_interval)
                        {
                            if (save_to_rota.empty() ||
                                (fabs(save_to_rota.back().x - Player.x) > 0.01f ||
                                    fabs(save_to_rota.back().y - Player.y) > 0.01f ||
                                    fabs(save_to_rota.back().z - Player.z) > 0.01f))
                            {
                                save_to_rota.push_back(Player);
                                last_save_time = now;
                            }
                        }
                    }
                    if (Keyboard::C_ClearRoute)
                    {
                        save_to_rota.clear();
                        current_rota_index = 0;
                        rota_basladi = false;
                        ileri_git = false;
                        Keyboard::C_ClearRoute = false;
                    }
                    if (Keyboard::C_PlayRoute && !save_to_rota.empty())
                    {
                        if (!rota_basladi)
                        {
                            current_rota_index = (int)save_to_rota.size() - 1;
                            ileri_git = false;
                            rota_basladi = true;
                        }
                        if (now - last_move_time > move_interval)
                        {
                            if (current_rota_index < 0) current_rota_index = 0;
                            if (current_rota_index >= (int)save_to_rota.size()) current_rota_index = (int)save_to_rota.size() - 1;

                            Vector3 hedef = save_to_rota[current_rota_index];

                            float dxh = Player.x - hedef.x;
                            float dyh = Player.y - hedef.y;
                            float dzh = Player.z - hedef.z;
                            float dist_target_sq = dxh * dxh + dyh * dyh + dzh * dzh;
                            const float target_reach_threshold = 50.0f * 50.0f;

                            if (dist_target_sq < target_reach_threshold)
                            {
                                if (ileri_git)
                                {
                                    current_rota_index++;
                                    if (current_rota_index >= (int)save_to_rota.size() - 1)
                                    {
                                        current_rota_index = (int)save_to_rota.size() - 1;
                                        ileri_git = false;
                                    }
                                }
                                else
                                {
                                    current_rota_index--;
                                    if (current_rota_index <= 0)
                                    {
                                        current_rota_index = 0;
                                        ileri_git = true;
                                    }
                                }
                            }
                            else
                            {
                                auto EntityList = GetEntities();
                                bool Rota_devam = true; 

                                for (const auto& ent : EntityList)
                                {
                                    Vector3 EnemyPos = { ent.x, -ent.y, ent.z };
                                    Vector3 PlayerPos = { c_players.x, -c_players.y, c_players.z };

                                    float Distance = Dist(ent.x, ent.y, c_players.x, c_players.y);

                                    if (Keyboard::C_UltimateRoute)
                                    {
                                        if (Distance < Keyboard::BOT_DISTANCE && ent.Type == 2)
                                        {
                                            if (ent.IsDead == 0)
                                            {
                                                Rota_devam = false;
                                                if (EntityList.empty())
                                                {
                                                    Sleep(Keyboard::BOT_SPEED);
                                                    continue;
                                                }

                                                for (auto it = KSBan.begin(); it != KSBan.end(); )
                                                {
                                                    DWORD banVID = *it;
                                                    auto found = std::find_if(EntityList.begin(), EntityList.end(), [&](const Entity& e) { return e.VID == banVID; });

                                                    if (found == EntityList.end())
                                                    {
                                                        it = KSBan.erase(it);
                                                        continue;
                                                    }

                                                    float distToPlayer = Dist(found->x, found->y, c_players.x, c_players.y);
                                                    if (distToPlayer > 500.0f)
                                                    {
                                                        it = KSBan.erase(it);
                                                    }
                                                    else
                                                    {
                                                        ++it;
                                                    }
                                                }

                                                float minDistance = FLT_MAX;
                                                int selectedIndex = -1;

                                                for (size_t i = 0; i < EntityList.size(); ++i)
                                                {
                                                    const auto& ent = EntityList[i];

                                                    if (ent.VID == 0)
                                                        continue;

                                                    if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                                                        continue;
                                                    if (std::find(KSBan.begin(), KSBan.end(), ent.VID) != KSBan.end())
                                                        continue;

                                                    if (ent.IsDead != 0)
                                                        continue;

                                                    bool validType = (Keyboard::Bot_Npc && ent.Type == 0) || (Keyboard::Bot_Stone && ent.Type == 2);
                                                    if (!validType)
                                                        continue;

                                                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                                                    if (dist < minDistance)
                                                    {
                                                        minDistance = dist;
                                                        selectedIndex = (int)i;
                                                    }
                                                }

                                                if (selectedIndex != -1)
                                                {
                                                    const Entity& target = EntityList[selectedIndex];
                                                    float distToTarget = Dist(target.x, target.y, c_players.x, c_players.y);

                                                    bool cancelAttack = false;

                                                    if (Keyboard::C_NoKs && target.Type == 2 && distToTarget > 1000.0f)
                                                    {
                                                        for (const auto& other : EntityList)
                                                        {
                                                            if (other.VID == 0) continue;
                                                            if (other.Type != 6) continue;
                                                            if (other.IsDead != 0) continue;

                                                            float distPlayerToTarget = Dist(other.x, other.y, target.x, target.y);
                                                            if (distPlayerToTarget < 1000.0f)
                                                            {
                                                                if (std::find(KSBan.begin(), KSBan.end(), target.VID) == KSBan.end())
                                                                    KSBan.push_back(target.VID);
                                                                if (std::find(KSBan.begin(), KSBan.end(), other.VID) == KSBan.end())
                                                                    KSBan.push_back(other.VID);

                                                                cancelAttack = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (!cancelAttack)
                                                    {
                                                        uintptr_t addr = Ez_memory::Adres(Ez_game_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
                                                        if (addr)
                                                        {
                                                            *(DWORD*)addr = target.VID;
                                                        }
                                                    }
                                                }
                                                continue;
                                            }
                                            else
                                            {
                                                Rota_devam = true;
                                            }
                                        }
                                    }
                                }


                                if (Rota_devam)
                                {
                                    engine::InstanceBaseNEW_MoveToDestPixelPositionDirection(&hedef);
                                }

                            }

                            last_move_time = now;
                        }
                    }
                    else
                    {
                        current_rota_index = 0;
                        rota_basladi = false;
                        ileri_git = false;
                    }
                }
            }
        }
        catch (...)
        {
        }

        Sleep(10);
    }
    return TRUE;
}



//DWORD WINAPI Set_AllFarmbotV2(HMODULE hModule)
//{
//    const DWORD loopSleepMs = 30; // çok hızlı ışınlanma için düşük sleep
//
//    while (true)
//    {
//        try
//        {
//            if (!Keyboard::C_BotTeleport)
//            {
//                Sleep(100);
//                continue;
//            }
//
//            auto Ent = GetEntities();
//
//            float minDistance = FLT_MAX;
//            Entity* nearestEntity = nullptr;
//
//            for (auto& ent : Ent)
//            {
//                // Banlı ise atla
//                if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
//                    continue;
//
//                // Uzaklık hesapla
//                float legitDistance = Dist(ent.x, ent.y, c_players.x, c_players.y);
//
//                if (legitDistance < 30.0f)
//                    continue;
//
//                // Stone veya NPC kontrolü
//                if (Keyboard::Bot_Stone && ent.Type == 2 && legitDistance < Keyboard::BOT_DISTANCE)
//                {
//                    if (legitDistance < minDistance)
//                    {
//                        minDistance = legitDistance;
//                        nearestEntity = &ent;
//                    }
//                }
//
//                if (Keyboard::Bot_Npc && ent.Type == 0 && legitDistance < Keyboard::BOT_DISTANCE)
//                {
//                    if (legitDistance < minDistance)
//                    {
//                        minDistance = legitDistance;
//                        nearestEntity = &ent;
//                    }
//                }
//            }
//
//            if (nearestEntity != nullptr)
//            {
//                uintptr_t Feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
//                Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr1);
//                Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr2);
//
//                // Direkt ışınla → ölü / diri fark etmez
//                *(float*)(Feature + metin2client::C_HorseCoordX) = nearestEntity->x;
//                *(float*)(Feature + metin2client::C_HorseCoordY) = nearestEntity->y;
//                *(float*)(Feature + metin2client::C_HorseCoordZ) = nearestEntity->z;
//            }
//        }
//        catch (...) {}
//
//        Sleep(loopSleepMs);
//    }
//
//    return 0;
//}



//DWORD WINAPI Set_AllFarmbotV2(HMODULE hModule)
//{
//    const DWORD loopSleepMs = 1; // mümkün olan en hızlı döngü
//
//    while (true)
//    {
//        try
//        {
//            if (!Keyboard::C_BotTeleport)
//            {
//                Sleep(100);
//                continue;
//            }
//
//            auto Ent = GetEntities();
//
//            for (auto& ent : Ent)
//            {
//                // banned listesi kontrolü
//                if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
//                    continue;
//
//                // hedef stone veya npc ise
//                if ((Keyboard::Bot_Stone && ent.Type == 2) ||
//                    (Keyboard::Bot_Npc && ent.Type == 0))
//                {
//                    // feature adresi
//                    uintptr_t Feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
//                    Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr1);
//                    Feature = ReadMemory<uintptr_t>(Feature + metin2client::C_HorseAddr2);
//
//                    // anında koordinat değiştir
//                    *(float*)(Feature + metin2client::C_HorseCoordX) = ent.x;
//                    *(float*)(Feature + metin2client::C_HorseCoordY) = ent.y;
//                    *(float*)(Feature + metin2client::C_HorseCoordZ) = ent.z;
//
//                    Sleep(1); // hemen diğer metine geç
//                }
//            }
//        }
//        catch (...)
//        {
//        }
//
//        Sleep(loopSleepMs);
//    }
//
//    return 0;
//}




// ============================================== Skills ============================================== //

struct HorseSkill {
    int slot;       // skill slotu
    DWORD cooldown; // cooldown süresi (ms)
    DWORD lastCast; // en son ne zaman basıldı
};

HorseSkill horseSkills[] = {
    { 1, 9000,  0 },
    { 2, 12000, 0 },
    { 3, 15000, 0 },
    { 4, 20000, 0 }
};

void ThreadSleep(DWORD ms)
{
    DWORD start = GetTickCount();
    while (GetTickCount() - start < ms)
    {
        Sleep(1);
    }
}

DWORD WINAPI Set_Skills(LPVOID)
{
    while (true)
    {
        try
        {
            if (Keyboard::Skills && Keyboard::C_HorseSkill)
            {
                DWORD now = GetTickCount();

                for (auto& hs : horseSkills)
                {

                    if ((hs.slot == 1 && !Keyboard::C_HorseSkillSlot1) ||
                        (hs.slot == 2 && !Keyboard::C_HorseSkillSlot2) ||
                        (hs.slot == 3 && !Keyboard::C_HorseSkillSlot3) ||
                        (hs.slot == 4 && !Keyboard::C_HorseSkillSlot4))
                        continue;

                    if (now - hs.lastCast >= hs.cooldown)
                    {
                        engine::NetworkStreamSendChatPacket(/*/ride*/XorStrTR<0x47, 6, 0x2CAE8000>("\x68\x3A\x20\x2E\x2E" + 0x2CAE8000).s, 0);
                        ThreadSleep(Keyboard::HorseTime);
                        engine::InstanceBase_ClickSkillSlot(hs.slot);
                        ThreadSleep(Keyboard::DefaultTime);
                        engine::NetworkStreamSendChatPacket(/*/ride*/XorStrTR<0x47, 6, 0x2CAE8000>("\x68\x3A\x20\x2E\x2E" + 0x2CAE8000).s, 0);
                        hs.lastCast = GetTickCount();
                    }
                }
            }
        }
        catch (...) {}

        Sleep(Keyboard::HorseTime);
    }
    return 0;
}

// ============================================== Skills v3 ============================================== //

DWORD WINAPI Set_SkillsV2(LPVOID)
{
    while (true)
    {
        try
        {
            if (Keyboard::Skills)
            {
                if (Keyboard::C_DefaultSkill)
                {
                    if (Keyboard::C_DefaultSkillSlot1)
                    {
                        engine::InstanceBase_ClickSkillSlot(1);
                    }
                    if (Keyboard::C_DefaultSkillSlot2)
                    {
                        engine::InstanceBase_ClickSkillSlot(2);
                    }
                    if (Keyboard::C_DefaultSkillSlot3)
                    {
                        engine::InstanceBase_ClickSkillSlot(3);
                    }
                    if (Keyboard::C_DefaultSkillSlot4)
                    {
                        engine::InstanceBase_ClickSkillSlot(4);
                    }
                }
            }
        }
        catch (...)
        {

        }
        Sleep(Keyboard::DefaultTime);
    }
}


// ==============================================  Anti-Ban ============================================== //


#include <unordered_map>
#include <queue>
#include <set>

DWORD WINAPI Set_AntiBans(HMODULE hModule)
{
    static std::unordered_map<DWORD, DWORD> LastNearTick;
    static std::unordered_map<DWORD, DWORD> lastSeenVIDs;

    static float lastPlayerX = 0;
    static float lastPlayerY = 0;

    while (true)
    {
        try
        {
            if (Keyboard::MiningDedect)
            {
                if (Keyboard::C_MiningLimitörs)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 1) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_Mining_Distance) continue;


                        if (Keyboard::C_MiningBeep && distance < Keyboard::C_Mining_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_MiningTeleport && distance < Keyboard::C_Mining_Distance)
                        {

                            uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
                            ReadMemory<float>(Feature_X, &ent.x, true);
                            uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
                            ReadMemory<float>(Feature_Y, &ent.y, true);
                            uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
                            ReadMemory<float>(Feature_Z, &ent.z, true);
                        }
                        if (Keyboard::C_AutoMining && distance < Keyboard::C_Mining_Distance)
                        {

                            engine::NetworkStreamPythonClickMining(ent.VID);
                        }
                        if (Keyboard::C_MiningShowBallon && distance < Keyboard::C_Mining_Distance)
                        {

                        }
                    }
                }
            }
            if (Keyboard::SpawmDedect)
            {
                if (Keyboard::C_PlayerSpawm)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 6) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_Portal_Distance) continue;


                        if (Keyboard::C_Portal_Sound && distance < Keyboard::C_Portal_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_PlayerSpawm_GameClose && distance < Keyboard::C_Portal_Distance)
                        {
                            exit(0);
                        }
                    }
                }
            }
            if (Keyboard::AreaDedect) // oyuncu
            {
                if (Keyboard::C_AreaLine)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 6) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_AreaDist) continue;


                        if (Keyboard::Area_Bep && distance < Keyboard::C_AreaDist)
                        {
                            Beep(100, 100);
                        }
                    }
                }
            }
            if (Keyboard::TeleportDedect)
            {
                if (Keyboard::C_Teleportss)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 6) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_Teleportss_Distance) continue;


                        if (Keyboard::C_Teleportss_Sound && distance < Keyboard::C_Teleportss_Distance)
                        {
                            float deltaX = abs(c_players.x - lastPlayerX);
                            float deltaY = abs(c_players.y - lastPlayerY);

                            if ((deltaX > 1000 || deltaY > 1000) && lastPlayerX != 0 && lastPlayerY != 0)
                            {
                                Beep(1500, 12300);
                            }

                            lastPlayerX = c_players.x;
                            lastPlayerY = c_players.y;
                        }
                        if (Keyboard::C_Teleportss_GameClose && distance < Keyboard::C_Teleportss_Distance)
                        {
                            float deltaX = abs(c_players.x - lastPlayerX);
                            float deltaY = abs(c_players.y - lastPlayerY);

                            if ((deltaX > 1000 || deltaY > 1000) && lastPlayerX != 0 && lastPlayerY != 0)
                            {
                                Beep(1500, 12300);
                                exit(1);
                            }

                            lastPlayerX = c_players.x;
                            lastPlayerY = c_players.y;
                        }
                    }
                }
            }
            if (Keyboard::PortalDedect)
            {
                if (Keyboard::C_Portal)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 16) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_Portal_Distance) continue;


                        if (Keyboard::C_Portal_Sound && distance < Keyboard::C_Portal_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_Portal_GameClose && distance < Keyboard::C_Portal_Distance)
                        {
                            exit(0);
                        }
                    }
                }
            }
            if (Keyboard::PlayerDedect)
            {
                if (Keyboard::C_Player_Dedect)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 6) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_Player_Dedect_Distance) continue;


                        if (Keyboard::C_Player_Dedect_Sound && distance < Keyboard::C_Player_Dedect_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_Player_Dedect_GameClose && distance < Keyboard::C_Player_Dedect_Distance)
                        {
                            exit(0);
                        }
                    }
                }
            }

            if (Keyboard::GMDedect)
            {
                if (Keyboard::C_GM_Dedect)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 6) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_GM_Dedect_Distance) continue;


                        if (Keyboard::C_GM_Dedect_Sound && distance < Keyboard::C_GM_Dedect_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_GM_Dedect_GameClose && distance < Keyboard::C_GM_Dedect_Distance)
                        {
                            exit(0);
                        }
                    }
                }
            }
            if (Keyboard::StoneDedect)
            {
                if (Keyboard::C_StoneDedect)
                {
                    auto Ent = GetEntities();
                    for (auto& ent : Ent)
                    {
                        if (ent.Type != 2) continue;

                        if (ent.VID == c_players.VID) continue;

                        float dx = abs(ent.x - c_players.x);
                        float dy = abs(ent.y - c_players.y);
                        if (dx < 1.0f && dy < 1.0f) continue;

                        float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
                        if (distance >= Keyboard::C_StoneDedect_Distance) continue;

                        // Yapıldı!!
                        if (Keyboard::C_StoneBeep && distance < Keyboard::C_StoneDedect_Distance)
                        {
                            Beep(100, 100);
                        }
                        if (Keyboard::C_StoneTeleport && distance < Keyboard::C_StoneDedect_Distance)
                        {
                            auto Ent = GetEntities();

                            float minDistance = FLT_MAX;
                            Entity* nearestEntity = nullptr;

                            for (auto& ent : Ent)
                            {
                                if (std::find(BannedVIDS.begin(), BannedVIDS.end(), ent.VID) != BannedVIDS.end())
                                    continue;

                                float legitDistance = Dist(ent.x, ent.y, c_players.x, c_players.y);

                                if (legitDistance < Keyboard::C_StoneDedect_Distance &&
                                    ent.Type == 2 &&
                                    ent.IsDead == 0)
                                {
                                    if (legitDistance < minDistance)
                                    {
                                        minDistance = legitDistance;
                                        nearestEntity = &ent;
                                    }
                                }
                            }

                            if (nearestEntity != nullptr)
                            {
                                uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
                                ReadMemory<float>(Feature_X, &nearestEntity->x, true);
                                uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
                                ReadMemory<float>(Feature_Y, &nearestEntity->y, true);
                                uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
                                ReadMemory<float>(Feature_Z, &nearestEntity->z, true);
                            }
                        }
                    }
                }
            }
        }
        catch (...) {}

        Sleep(500);
    }

    return 0;
}

// ============================================== Hittings ============================================== //

bool ilk_yazılan1 = false;
bool ikinci_yazılan1 = false;
bool test_a = true;
DWORD WINAPI Set_Hitting(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::WSCheat)
            {
                if (Keyboard::C_Hitting)
                {
                    if (!ilk_yazılan1)
                    {
                        uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_OneHit;
                        int ilk = 1;
                        ReadMemory<int>(Feature, &ilk, true);
                        ilk_yazılan = true;
                        ikinci_yazılan1 = false;
                    }
                    Sleep(1);
                    if (ilk_yazılan1 && !ikinci_yazılan1)
                    {
                        uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_OneHit;
                        int ilk = 2;
                        ReadMemory<int>(Feature, &ilk, true);
                        ikinci_yazılan1 = true;
                        ilk_yazılan1 = false;
                    }
                }
            }
        }
        catch (...)
        {

        }
        Sleep(1);
    }
    return true;
}

// ============================================== ClickTeleport ============================================== //

DWORD WINAPI Set_ClickTeleport(HMODULE hModule)
{
    static bool Everyone_Checks = false;
    static bool ModelCheck = false;
    static bool SendModel = false;

    while (true)
    {

        if (Keyboard::WSCheat)
        {
            // ==================================================== Click Teleport ====================================================
            if (Keyboard::C_ClickToTeleport && c_players.BannedVID != 0)
            {
                metin2client::Globals();

                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                {
                    continue;
                }

                DWORD entityManager = *CharacterManager;
                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                {
                    continue;
                }

                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;

                    if (id != c_players.BannedVID)
                        continue;

                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);
                    if (IsDead != 0)
                        continue;

                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
                    float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);

                    float dist = Dist(x, y, c_players.x, c_players.y);
                    if (dist > Keyboard::C_ClickToTeleport_Distance)
                        break;

                    engine::SendCharacterVector(Vector3(x, y, z));
                    break;
                }
            }
            // ==================================================== Everyone Check ====================================================
            if (GetAsyncKeyState(VK_CONTROL) & 0x8000)
            {
                if (!Everyone_Checks)
                {
                    Keyboard::C_EveryoneCheck = !Keyboard::C_EveryoneCheck;
                    Everyone_Checks = true;
                }
            }
            else
            {
                Everyone_Checks = false;
            }
            if (Keyboard::C_EveryoneCheck)
            {
                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                {
                    Sleep(1);
                    continue;
                }

                DWORD entityManager = *CharacterManager;
                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                {
                    Sleep(1);
                    continue;
                }

                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

                DWORD* playerModule = nullptr;
                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;
                    if (id == c_players.VID)
                    {
                        playerModule = Module;
                        break;
                    }
                }

                int closestIndex = -1;
                float closestDist = FLT_MAX;

                std::vector<std::pair<DWORD, DWORD*>> validEntities;

                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;

                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);
                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);

                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
                    float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);

                    if (Type != 6 || IsDead != 0)
                        continue;

                    if (x == c_players.x && y == c_players.y && z == c_players.z)
                        continue;

                    float dist = Dist(x, y, c_players.x, c_players.y);

                    if (dist < Keyboard::C_ClickToTeleport_Distance)
                    {
                        validEntities.emplace_back(id, Module);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestIndex = (int)(validEntities.size() - 1);
                        }
                    }
                }

                for (int i = 0; i < (int)validEntities.size(); ++i)
                {
                    DWORD id = validEntities[i].first;
                    DWORD* Module = validEntities[i].second;

                    *(float*)((BYTE*)Module + metin2client::C_CoordsX) = c_players.x;
                    *(float*)((BYTE*)Module + metin2client::C_CoordsY) = c_players.y;
                    *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = c_players.z;
                }
            }
            // ==================================================== CheckModel ====================================================
            if (GetAsyncKeyState(VK_SHIFT) & 0x8000)
            {
                if (!ModelCheck)
                {
                    Keyboard::C_CheckModel = !Keyboard::C_CheckModel;
                    ModelCheck = true;
                }
            }
            else
            {
                ModelCheck = false;
            }
            if (Keyboard::C_CheckModel)
            {
                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
                {
                    Sleep(1);
                    continue;
                }

                DWORD entityManager = *CharacterManager;
                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
                {
                    Sleep(1);
                    continue;
                }

                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);

                DWORD* playerModule = nullptr;
                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;
                    if (id == c_players.VID)
                    {
                        playerModule = Module;
                        break;
                    }
                }

                int closestIndex = -1;
                float closestDist = FLT_MAX;

                std::vector<std::pair<DWORD, DWORD*>> validEntities;

                for (auto& [id, Module] : m_kAliveInstMap)
                {
                    if (!Module)
                        continue;

                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);
                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);

                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
                    float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);

                    if (Type != 6 || IsDead != 0)
                        continue;

                    if (x == c_players.x && y == c_players.y && z == c_players.z)
                        continue;

                    float dist = Dist(x, y, c_players.x, c_players.y);

                    if (dist < Keyboard::C_ClickToTeleport_Distance)
                    {
                        validEntities.emplace_back(id, Module);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestIndex = (int)(validEntities.size() - 1);
                        }
                    }
                }

                for (int i = 0; i < (int)validEntities.size(); ++i)
                {
                    DWORD id = validEntities[i].first;
                    DWORD* Module = validEntities[i].second;

                    *(float*)((BYTE*)Module + metin2client::C_ModelCoordX) = c_players.x;
                    *(float*)((BYTE*)Module + metin2client::C_ModelCoordY) = c_players.y;
                    *(float*)((BYTE*)Module + metin2client::C_ModelCoordZ) = c_players.z;
                }
            }
            // ==================================================== Send Model ====================================================
            if (GetAsyncKeyState(VK_CAPITAL) & 0x8000)
            {
                if (!SendModel)
                {
                    Keyboard::C_SendModel = !Keyboard::C_SendModel;
                    SendModel = true;
                }
            }
            else
            {
                SendModel = false;
            }
            if (Keyboard::C_SendModel)
            {
                auto entities = GetEntities();
                int closestIndex = -1;
                float closestDist = FLT_MAX;

                for (int i = 0; i < entities.size(); ++i)
                {
                    const auto& ent = entities[i];

                    if (ent.Type != 6 || ent.IsDead != 0)
                        continue;

                    if (ent.x == c_players.x && ent.y == c_players.y && ent.z == c_players.z)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                    if (dist < Keyboard::C_ClickToTeleport_Distance && dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }

                for (int i = 0; i < entities.size(); ++i)
                {
                    const auto& ent = entities[i];

                    if (ent.Type != 6 || ent.IsDead != 0)
                        continue;

                    if (ent.x == c_players.x && ent.y == c_players.y && ent.z == c_players.z)
                        continue;

                    float dist = Dist(ent.x, ent.y, c_players.x, c_players.y);
                    if (dist >= Keyboard::C_ClickToTeleport_Distance)
                        continue;

                    Vector3 EntityTarget = { ent.x, ent.y, ent.z };

                    float radius = 0.0f;
                    float rdn = -DEG2RAD(c_players.Rotation + Angle);

                    float offsetX = cos(rdn) * radius;
                    float offsetY = sin(rdn) * radius;

                    float TargetX, TargetY, TargetZ;

                    if (i == closestIndex)
                    {
                        float closerRadius = 30.0f;
                        float closerOffsetX = cos(rdn) * closerRadius;
                        float closerOffsetY = sin(rdn) * closerRadius;

                        TargetX = EntityTarget.x + closerOffsetX;
                        TargetY = EntityTarget.y + closerOffsetY;
                        TargetZ = EntityTarget.z;
                    }
                    else
                    {
                        TargetX = EntityTarget.x + offsetX;
                        TargetY = EntityTarget.y + offsetY;
                        TargetZ = EntityTarget.z;
                    }

                    engine::SendCharacterModel(Vector3(TargetX, TargetY, TargetZ));
                }

                Angle += 15.0f;
                if (Angle >= 360.0f)
                    Angle = 0.0f;
            }
        }
        Sleep(1);
    }

    return 0;
}

// ============================================== SkillRotation ============================================== //
//bool set_skillss = true;
//
//DWORD WINAPI Set_SkillRotation(HMODULE hModule)
//{
//    while (true)
//    {
//        if (set_skillss)
//        {
//            DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
//            if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
//            {
//                Sleep(1);
//                continue;
//            }
//
//            DWORD entityManager = *CharacterManager;
//            if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
//            {
//                Sleep(1);
//                continue;
//            }
//
//            PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
//
//            DWORD* playerModule = nullptr;
//            for (auto& [id, Module] : m_kAliveInstMap)
//            {
//                if (!Module)
//                    continue;
//                if (id == c_players.VID)
//                {
//                    playerModule = Module;
//                    break;
//                }
//            }
//
//            int closestIndex = -1;
//            float closestDist = FLT_MAX;
//
//            std::vector<std::pair<DWORD, DWORD*>> validEntities;
//
//            for (auto& [id, Module] : m_kAliveInstMap)
//            {
//                if (!Module)
//                    continue;
//
//                int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);
//                int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);
//
//                float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
//                float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
//                float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);
//
//                if (Type != 0 || IsDead != 0)
//                    continue;
//
//                if (x == c_players.x && y == c_players.y && z == c_players.z)
//                    continue;
//
//                float dist = Dist(x, y, c_players.x, c_players.y);
//
//                if (dist < 2000)
//                {
//                    validEntities.emplace_back(id, Module);
//
//                    if (dist < closestDist)
//                    {
//                        closestDist = dist;
//                        closestIndex = (int)(validEntities.size() - 1);
//                    }
//                }
//            }
//
//            for (int i = 0; i < (int)validEntities.size(); ++i)
//            {
//                DWORD id = validEntities[i].first;
//                DWORD* Module = validEntities[i].second;
//
//                uintptr_t Feature = ReadMemory<uintptr_t>(
//                    ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer)
//                    + metin2client::C_LocalAddr
//                    ) + metin2client::C_IsCharRotation;
//
//                float step = 10.4674683f;
//                for (float val = 0.0f; val <= 360.0f; val += step)
//                {
//                    *(float*)((BYTE*)Module + metin2client::C_IsCharRotation) = val;
//                    Sleep(30);
//                }
//
//                const char* newName = "KralBot";
//                char* nameAddr = (char*)((BYTE*)Module + metin2client::C_Name);
//                if (nameAddr)
//                    strncpy_s(nameAddr, 64, newName, _TRUNCATE); 
//            }
//        }
//
//        Sleep(1);
//    }
//
//    return 0;
//}
//
void OpenConsoleV2()
{
    AllocConsole();
    freopen("CONOUT$", "w", stdout);
    freopen("CONIN$", "r", stdin);
    SetConsoleTitleA("EzgiAkbas | (CheatBlocker) - 2026");
}



//
//#include <windows.h>
//#include <tlhelp32.h>
//#include <vector>
//#include <stdio.h>
//
//
//// Client bilgileri
//struct ClientInfo
//{
//    DWORD processId;
//    DWORD baseAddress;
//    HANDLE hProcess;
//    std::string title;
//};
//
//std::vector<ClientInfo> g_otherClients;
//
//// Diğer client'ları bul
//void FindOtherClients()
//{
//    g_otherClients.clear();
//    DWORD myPID = GetCurrentProcessId();
//
//    HWND hwnd = NULL;
//    while ((hwnd = FindWindowExA(NULL, hwnd, NULL, NULL)) != NULL)
//    {
//        char title[256];
//        GetWindowTextA(hwnd, title, 256);
//
//        if (strstr(title, "Celestial World 2.0") || strstr(title, "METIN2"))
//        {
//            DWORD pid;
//            GetWindowThreadProcessId(hwnd, &pid);
//
//            if (pid == myPID)
//                continue;
//
//            HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
//            if (!hProcess)
//                continue;
//
//            HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
//            if (hSnapshot != INVALID_HANDLE_VALUE)
//            {
//                MODULEENTRY32 me32;
//                me32.dwSize = sizeof(MODULEENTRY32);
//
//                DWORD baseAddr = 0;
//                if (Module32First(hSnapshot, &me32))
//                {
//                    do
//                    {
//                        if (_stricmp(me32.szModule, "metin2client.bin") == 0)
//                        {
//                            baseAddr = (DWORD)me32.modBaseAddr;
//                            break;
//                        }
//                    } while (Module32Next(hSnapshot, &me32));
//                }
//
//                CloseHandle(hSnapshot);
//
//                if (baseAddr)
//                {
//                    ClientInfo info;
//                    info.processId = pid;
//                    info.baseAddress = baseAddr;
//                    info.hProcess = hProcess;
//                    info.title = title;
//                    g_otherClients.push_back(info);
//                    continue;
//                }
//            }
//
//            CloseHandle(hProcess);
//        }
//    }
//
//    printf("[+] %zu baska client bulundu\n", g_otherClients.size());
//    for (size_t i = 0; i < g_otherClients.size(); i++)
//    {
//        printf("  [%d] PID: %d | %s\n", i, g_otherClients[i].processId, g_otherClients[i].title.c_str());
//    }
//}
//
//int comboIndex = 0;
//DWORD lastAttackTime = 0;
//
//bool NetworkStreamSentBattleAttack(int clientIndex, UINT attack, DWORD vid)
//{
//    if (clientIndex < 0 || clientIndex >= g_otherClients.size())
//        return false;
//
//    ClientInfo& client = g_otherClients[clientIndex];
//    struct Params {
//        DWORD funcAddr;
//        DWORD networkStreamOffset;
//        DWORD baseAddr;
//        UINT attack;
//        DWORD vid;
//    };
//
//    Params params;
//    params.funcAddr = client.baseAddress + metin2client::C_SendBattlePacket;
//    params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
//    params.baseAddr = client.baseAddress;
//    params.attack = attack;
//    params.vid = vid;
//
//    void* pParams = VirtualAllocEx(client.hProcess, NULL, sizeof(Params),
//        MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
//    if (!pParams)
//        return false;
//
//    WriteProcessMemory(client.hProcess, pParams, &params, sizeof(Params), NULL);
//
//    BYTE code[] = {
//        0x55, 0x8B, 0xEC, 0x60,
//        0xB8, 0x00, 0x00, 0x00, 0x00,
//        0x8B, 0x48, 0x08,
//        0x03, 0x48, 0x04,
//        0x8B, 0x09,
//        0xFF, 0x70, 0x10,
//        0xFF, 0x70, 0x0C,
//        0xFF, 0x10,
//        0x61, 0x8B, 0xE5, 0x5D, 0xC3
//    };
//
//    *(DWORD*)&code[5] = (DWORD)pParams;
//
//    void* pCode = VirtualAllocEx(client.hProcess, NULL, sizeof(code),
//        MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
//    if (!pCode)
//    {
//        VirtualFreeEx(client.hProcess, pParams, 0, MEM_RELEASE);
//        return false;
//    }
//
//    WriteProcessMemory(client.hProcess, pCode, code, sizeof(code), NULL);
//
//    HANDLE hThread = CreateRemoteThread(client.hProcess, NULL, 0,
//        (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
//
//    if (hThread)
//    {
//        WaitForSingleObject(hThread, 50);
//        CloseHandle(hThread);
//    }
//
//    VirtualFreeEx(client.hProcess, pParams, 0, MEM_RELEASE);
//    VirtualFreeEx(client.hProcess, pCode, 0, MEM_RELEASE);
//
//    return hThread != NULL;
//}
//
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//
//    sdk_client = (DWORD)GetModuleHandleA("metin2client.bin");
//    Sleep(3000);
//    FindOtherClients();
//
//    if (g_otherClients.size() == 0)
//    {
//        printf("\n[!] Baska client yok!\n");
//        while (!(GetAsyncKeyState(VK_ESCAPE) & 0x8000)) Sleep(1);
//        return 0;
//    }
//
//    printf("\n[*] Slave client (0-%zu): ", g_otherClients.size() - 1);
//    int slaveIndex;
//    scanf_s("%d", &slaveIndex);
//
//    printf("[1] Normal (80ms)  - Guvenli, ~12 APS\n");
//    printf("[2] Hizli  (50ms)  - Orta, ~20 APS\n");
//    printf("[3] Cok Hizli (30ms) - Agresif, ~33 APS\n");
//    printf("[4] Maksimum (10ms) - Riskli, ~100 APS\n");
//    printf("[5] Custom - Kendin ayarla\n");
//    printf("\nSeciminiz (1-5): ");
//
//    int speedChoice;
//    scanf_s("%d", &speedChoice);
//
//    int attackDelay = 80;
//
//    switch (speedChoice)
//    {
//    case 1: attackDelay = 80; break;
//    case 2: attackDelay = 50; break;
//    case 3: attackDelay = 30; break;
//    case 4: attackDelay = 10; break;
//    case 5:
//        printf("Attack delay (ms, 10-200): ");
//        scanf_s("%d", &attackDelay);
//        break;
//    default:
//        attackDelay = 80;
//        break;
//    }
//
//    printf("\n[+] Attack delay: %dms (~%.1f APS)\n", attackDelay, 1000.0f / attackDelay);
//    printf("[SPACE] Baslat \n\n");
//
//    bool attacking = false;
//    comboIndex = 0;
//    int totalAttacks = 0;
//    DWORD startTime = 0;
//
//    while (true)
//    {
//        if (GetAsyncKeyState(VK_SPACE) & 0x8000)
//        {
//            attacking = !attacking;
//
//            if (attacking)
//            {
//                comboIndex = 0;
//                totalAttacks = 0;
//                startTime = GetTickCount();
//                lastAttackTime = 0;
//                printf("[+] Attack Basladı\n");
//            }
//            else
//            {
//                printf("[-] Attack Durdu\n");
//            }
//
//            Sleep(1);
//        }
//
//        if (GetAsyncKeyState(VK_ESCAPE) & 0x8000)
//            break;
//
//        if (attacking)
//        {
//            DWORD vid = engine::GetTargetVID();
//
//            if (vid != 0)
//            {
//                DWORD currentTime = GetTickCount();
//                if (currentTime - lastAttackTime >= attackDelay)
//                {
//                    UINT attack = comboIndex % 7;
//                    engine::NetworkStreamSendAttackPacket(attack, vid);
//                    CallRemoteAttackWithAnimation(slaveIndex, attack, vid);
//
//                    comboIndex++;
//                    totalAttacks++;
//                    lastAttackTime = currentTime;
//                    DWORD elapsed = currentTime - startTime;
//                    float aps = elapsed > 0 ? (totalAttacks * 1000.0f / elapsed) : 0;
//
//                    printf("\r[+] VID: %u | Combo: %u | Attacks: %d | APS: %.1f     ",
//                        vid, attack, totalAttacks, aps);
//                }
//            }
//            else
//            {
//                printf("\r[*] Hedef sec...                              ");
//                comboIndex = 0;
//            }
//        }
//
//        Sleep(1); 
//    }
//
//    for (auto& c : g_otherClients)
//        CloseHandle(c.hProcess);
//
//    FreeConsole();
//    return 0;
//}


//#include <windows.h>
//#include <tlhelp32.h>
//#include <stdio.h>
//
//struct ClientInfo {
//    DWORD baseAddress;
//    HANDLE hProcess;
//};
//
//ClientInfo g_slave;
//int g_combo = 0;
//DWORD g_lastAttack = 0;
//
//bool GetClient(HWND hwnd)
//{
//    DWORD pid;
//    GetWindowThreadProcessId(hwnd, &pid);
//
//    if (pid == GetCurrentProcessId())
//        return false;
//
//    HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
//    if (!hProcess)
//        return false;
//
//    HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
//    MODULEENTRY32 me32 = { sizeof(me32) };
//    DWORD baseAddr = 0;
//
//    if (Module32First(snap, &me32))
//    {
//        do {
//            if (_stricmp(me32.szModule, "metin2client.bin") == 0)
//            {
//                baseAddr = (DWORD)me32.modBaseAddr;
//                break;
//            }
//        } while (Module32Next(snap, &me32));
//    }
//
//    CloseHandle(snap);
//
//    if (!baseAddr)
//    {
//        CloseHandle(hProcess);
//        return false;
//    }
//
//    g_slave.baseAddress = baseAddr;
//    g_slave.hProcess = hProcess;
//    return true;
//}
//
//namespace Remote
//{
//    void RemoteAttack(UINT attack, DWORD vid)
//    {
//        struct { DWORD func, net, base; UINT atk; DWORD vid; } p = {
//            g_slave.baseAddress + metin2client::C_SendBattlePacket,
//            metin2client::C_CPythonNetworkStream,
//            g_slave.baseAddress,
//            attack,
//            vid
//        };
//
//        void* pParams = VirtualAllocEx(g_slave.hProcess, NULL, sizeof(p), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
//        WriteProcessMemory(g_slave.hProcess, pParams, &p, sizeof(p), NULL);
//
//        BYTE code[] = { 0x55, 0x8B, 0xEC, 0x60, 0xB8, 0, 0, 0, 0, 0x8B, 0x48, 0x08, 0x03, 0x48, 0x04, 0x8B, 0x09, 0xFF, 0x70, 0x10, 0xFF, 0x70, 0x0C, 0xFF, 0x10, 0x61, 0x8B, 0xE5, 0x5D, 0xC3 };
//        *(DWORD*)&code[5] = (DWORD)pParams;
//
//        void* pCode = VirtualAllocEx(g_slave.hProcess, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
//        WriteProcessMemory(g_slave.hProcess, pCode, code, sizeof(code), NULL);
//
//        HANDLE hThread = CreateRemoteThread(g_slave.hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
//        WaitForSingleObject(hThread, 50);
//        CloseHandle(hThread);
//
//        VirtualFreeEx(g_slave.hProcess, pParams, 0, MEM_RELEASE);
//        VirtualFreeEx(g_slave.hProcess, pCode, 0, MEM_RELEASE);
//    }
//
//    void RemoteMove(const Vector3& pos, float rot, BYTE func, BYTE arg)
//    {
//        struct {
//            DWORD funcAddr, netOffset, baseAddr;
//            Vector3 position;
//            float rotation;
//            BYTE eFunc, uArg, none;
//        } p = {
//            g_slave.baseAddress + metin2client::C_SendCharacterStatePacket,
//            metin2client::C_CPythonNetworkStream,
//            g_slave.baseAddress,
//            pos,
//            rot,
//            func,
//            arg,
//            0
//        };
//
//        void* pParams = VirtualAllocEx(g_slave.hProcess, NULL, sizeof(p), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
//        WriteProcessMemory(g_slave.hProcess, pParams, &p, sizeof(p), NULL);
//
//        BYTE code[] = {
//            0x55, 0x8B, 0xEC, 0x60, 0xB8, 0, 0, 0, 0,
//            0x8B, 0x48, 0x08, 0x03, 0x48, 0x04, 0x8B, 0x09,
//            0x8A, 0x50, 0x20, 0x88, 0x54, 0x24, 0xFC,
//            0x8A, 0x50, 0x1F, 0x88, 0x54, 0x24, 0xFB,
//            0x8A, 0x50, 0x1E, 0x88, 0x54, 0x24, 0xFA,
//            0xFF, 0x70, 0x1A, 0x8D, 0x50, 0x0C, 0x52,
//            0xFF, 0x10, 0x61, 0x8B, 0xE5, 0x5D, 0xC3
//        };
//        *(DWORD*)&code[5] = (DWORD)pParams;
//
//        void* pCode = VirtualAllocEx(g_slave.hProcess, NULL, sizeof(code), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
//        WriteProcessMemory(g_slave.hProcess, pCode, code, sizeof(code), NULL);
//
//        HANDLE hThread = CreateRemoteThread(g_slave.hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
//        WaitForSingleObject(hThread, 50);
//        CloseHandle(hThread);
//
//        VirtualFreeEx(g_slave.hProcess, pParams, 0, MEM_RELEASE);
//        VirtualFreeEx(g_slave.hProcess, pCode, 0, MEM_RELEASE);
//    }
//}
//
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//    sdk_client = (DWORD)GetModuleHandleA("metin2client.bin");
//
//    printf("[F5] Slave Sec | [ESC] Cikis\n");
//
//    // F5 - Slave seç
//    while (!(GetAsyncKeyState(VK_F5) & 0x8000))
//    {
//        if (GetAsyncKeyState(VK_ESCAPE) & 0x8000)
//        {
//            FreeConsole();
//            return 0;
//        }
//        Sleep(50);
//    }
//
//    if (GetClient(GetForegroundWindow()))
//        printf("[+] Slave OK\n");
//    else
//    {
//        printf("[-] Slave FAIL\n");
//        Sleep(2000);
//        FreeConsole();
//        return 0;
//    }
//
//    Sleep(500);
//    printf("[SPACE] Start/Stop | [ESC] Exit\n\n");
//
//    bool attacking = false;
//    bool wasSpace = false;
//
//    while (!(GetAsyncKeyState(VK_ESCAPE) & 0x8000))
//    {
//        bool space = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;
//        if (space && !wasSpace)
//        {
//            attacking = !attacking;
//            g_combo = 0;
//            g_lastAttack = 0;
//            printf(attacking ? "[+] START\n" : "[-] STOP\n");
//            Sleep(200);
//        }
//        wasSpace = space;
//
//        if (attacking && GetTickCount() - g_lastAttack >= 30)
//        {
//            auto entities = GetEntities();
//            if (!entities.empty())
//            {
//                for (const auto& ent : entities)
//                {
//                    Vector3 newPosition = { ent.x, -ent.y, ent.z };
//                    Vector3 oldPosition;
//                    engine::GetMainGetPixelPosition(&oldPosition);
//                    std::vector<Vector3> distancePoints = engine::DivideTwoPointsByDistance(900, oldPosition, newPosition);
//                    for (const auto& point : distancePoints)
//                    {
//                        Remote::RemoteMove(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
//                    }
//
//                    UINT attack = g_combo % 7;
//                    Remote::RemoteAttack(attack, ent.VID);
//                    distancePoints = engine::DivideTwoPointsByDistance(900, newPosition, oldPosition);
//                    for (const auto& point : distancePoints)
//                    {
//                        Remote::RemoteMove(Vector3(point.x, point.y, point.z), ent.Rotation, 0, 0);
//                    }
//                }
//
//                g_combo++;
//                g_lastAttack = GetTickCount();
//                printf("\r[Hedef: %d] [Combo: %u]     ", (int)entities.size(), g_combo % 7);
//            }
//        }
//
//        Sleep(1);
//    }
//
//    CloseHandle(g_slave.hProcess);
//    FreeConsole();
//    return 0;
//}

//#include <windows.h>
//#include <tlhelp32.h>
//#include <vector>
//#include <stdio.h>
//#include <string>
//
//// ==================== STRUCTURES ====================
//struct ClientInfo
//{
//    DWORD processId;
//    DWORD baseAddress;
//    HANDLE hProcess;
//    std::string title;
//    HWND hwnd;
//};
//
//std::vector<ClientInfo> g_slaveClients;
//DWORD g_myPID = 0;
//int g_comboIndex = 0;
//DWORD g_lastAttackTime = 0;
//
//// ==================== COLORS ====================
//void SetColor(int color)
//{
//    HANDLE h = GetStdHandle(STD_OUTPUT_HANDLE);
//    SetConsoleTextAttribute(h, color);
//}
//
//#define C_RED     12
//#define C_GREEN   10
//#define C_YELLOW  14
//#define C_CYAN    11
//#define C_WHITE   15
//#define C_GRAY    8
//
//// ==================== CLIENT MANAGEMENT ====================
//bool GetClientInfoFromHWND(HWND hwnd, ClientInfo& info)
//{
//    char title[256];
//    GetWindowTextA(hwnd, title, 256);
//
//    DWORD pid;
//    GetWindowThreadProcessId(hwnd, &pid);
//
//    // Kendi process'imiz mi?
//    if (pid == g_myPID)
//    {
//        SetColor(C_RED);
//        printf("[!] Bu kendi client'in! Başka bir pencereye tıkla.\n");
//        SetColor(C_WHITE);
//        return false;
//    }
//
//    // Zaten eklendi mi?
//    for (auto& slave : g_slaveClients)
//    {
//        if (slave.processId == pid)
//        {
//            SetColor(C_YELLOW);
//            printf("[!] Bu client zaten slave olarak ekli!\n");
//            SetColor(C_WHITE);
//            return false;
//        }
//    }
//
//    // Process'i aç
//    HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
//    if (!hProcess)
//    {
//        SetColor(C_RED);
//        printf("[!] Process açılamadı! (Admin olarak çalıştır)\n");
//        SetColor(C_WHITE);
//        return false;
//    }
//
//    // metin2client.bin modülünü bul
//    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
//    if (hSnapshot == INVALID_HANDLE_VALUE)
//    {
//        CloseHandle(hProcess);
//        SetColor(C_RED);
//        printf("[!] Module snapshot alınamadı!\n");
//        SetColor(C_WHITE);
//        return false;
//    }
//
//    MODULEENTRY32 me32;
//    me32.dwSize = sizeof(MODULEENTRY32);
//
//    DWORD baseAddr = 0;
//    if (Module32First(hSnapshot, &me32))
//    {
//        do
//        {
//            if (_stricmp(me32.szModule, "metin2client.bin") == 0)
//            {
//                baseAddr = (DWORD)me32.modBaseAddr;
//                break;
//            }
//        } while (Module32Next(hSnapshot, &me32));
//    }
//
//    CloseHandle(hSnapshot);
//
//    if (!baseAddr)
//    {
//        CloseHandle(hProcess);
//        SetColor(C_RED);
//        printf("[!] Bu bir Metin2 client'ı değil!\n");
//        SetColor(C_WHITE);
//        return false;
//    }
//
//    // Bilgileri doldur
//    info.processId = pid;
//    info.baseAddress = baseAddr;
//    info.hProcess = hProcess;
//    info.title = title;
//    info.hwnd = hwnd;
//
//    return true;
//}
//
//void AddSlaveClient()
//{
//    SetColor(C_CYAN);
//    printf("\n[*] Slave eklemek için:\n");
//    printf("    1. Slave olacak client penceresine tıkla\n");
//    printf("    2. F5'e bas\n");
//    printf("    3. Tüm slave'leri ekledikten sonra F6'ya bas\n\n");
//    SetColor(C_WHITE);
//
//    while (true)
//    {
//        // F5 - Slave ekle
//        if (GetAsyncKeyState(VK_F5) & 0x8000)
//        {
//            Sleep(200); // Debounce
//
//            // Aktif pencereyi al
//            HWND activeWindow = GetForegroundWindow();
//
//            if (!activeWindow)
//            {
//                SetColor(C_RED);
//                printf("[!] Aktif pencere bulunamadı!\n");
//                SetColor(C_WHITE);
//                continue;
//            }
//
//            ClientInfo info;
//            if (GetClientInfoFromHWND(activeWindow, info))
//            {
//                g_slaveClients.push_back(info);
//
//                SetColor(C_GREEN);
//                printf("[+] Slave #%d eklendi:\n", (int)g_slaveClients.size());
//                printf("    PID: %d\n", info.processId);
//                printf("    Title: %s\n", info.title.c_str());
//                SetColor(C_WHITE);
//            }
//
//            Sleep(500); // Anti-spam
//        }
//
//        // F6 - Devam et
//        if (GetAsyncKeyState(VK_F6) & 0x8000)
//        {
//            if (g_slaveClients.empty())
//            {
//                SetColor(C_RED);
//                printf("[!] En az 1 slave eklemen lazım!\n");
//                SetColor(C_WHITE);
//                Sleep(500);
//                continue;
//            }
//
//            break;
//        }
//
//        // ESC - İptal
//        if (GetAsyncKeyState(VK_ESCAPE) & 0x8000)
//        {
//            SetColor(C_YELLOW);
//            printf("\n[*] İptal edildi.\n");
//            SetColor(C_WHITE);
//
//            // Cleanup
//            for (auto& slave : g_slaveClients)
//                CloseHandle(slave.hProcess);
//
//            g_slaveClients.clear();
//            return;
//        }
//
//        Sleep(50);
//    }
//
//    SetColor(C_GREEN);
//    printf("\n[+] Toplam %d slave eklendi!\n", (int)g_slaveClients.size());
//    SetColor(C_WHITE);
//}
//
//void ShowSlaveList()
//{
//    SetColor(C_CYAN);
//    printf("\n╔════════════════════════════════════════════════════════╗\n");
//    printf("║                   SLAVE LİSTESİ                        ║\n");
//    printf("╚════════════════════════════════════════════════════════╝\n");
//    SetColor(C_WHITE);
//
//    for (size_t i = 0; i < g_slaveClients.size(); i++)
//    {
//        SetColor(C_YELLOW);
//        printf("  [%d] ", (int)i);
//        SetColor(C_WHITE);
//        printf("PID: %-6d | %s\n",
//            g_slaveClients[i].processId,
//            g_slaveClients[i].title.c_str());
//    }
//    printf("\n");
//}
//
//// ==================== REMOTE ATTACK ====================
//bool CallRemoteAttack(int clientIndex, UINT attack, DWORD vid)
//{
//    if (clientIndex < 0 || clientIndex >= g_slaveClients.size())
//        return false;
//
//    ClientInfo& client = g_slaveClients[clientIndex];
//
//    // Process hala açık mı?
//    DWORD exitCode;
//    if (GetExitCodeProcess(client.hProcess, &exitCode))
//    {
//        if (exitCode != STILL_ACTIVE)
//        {
//            SetColor(C_RED);
//            printf("\n[!] Slave #%d kapandı!\n", clientIndex);
//            SetColor(C_WHITE);
//            return false;
//        }
//    }
//
//    struct Params {
//        DWORD funcAddr;
//        DWORD networkStreamOffset;
//        DWORD baseAddr;
//        UINT attack;
//        DWORD vid;
//    };
//
//    Params params;
//    params.funcAddr = client.baseAddress + metin2client::C_SendBattlePacket;
//    params.networkStreamOffset = metin2client::C_CPythonNetworkStream;
//    params.baseAddr = client.baseAddress;
//    params.attack = attack;
//    params.vid = vid;
//
//    void* pParams = VirtualAllocEx(client.hProcess, NULL, sizeof(Params),
//        MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
//    if (!pParams)
//        return false;
//
//    WriteProcessMemory(client.hProcess, pParams, &params, sizeof(Params), NULL);
//
//    BYTE code[] = {
//        0x55,                           // push ebp
//        0x8B, 0xEC,                     // mov ebp, esp
//        0x60,                           // pushad
//        0xB8, 0x00, 0x00, 0x00, 0x00,  // mov eax, pParams
//        0x8B, 0x48, 0x08,               // mov ecx, [eax+08]
//        0x03, 0x48, 0x04,               // add ecx, [eax+04]
//        0x8B, 0x09,                     // mov ecx, [ecx]
//        0xFF, 0x70, 0x10,               // push [eax+10] (vid)
//        0xFF, 0x70, 0x0C,               // push [eax+0C] (attack)
//        0xFF, 0x10,                     // call [eax]
//        0x61,                           // popad
//        0x8B, 0xE5,                     // mov esp, ebp
//        0x5D,                           // pop ebp
//        0xC3                            // ret
//    };
//
//    *(DWORD*)&code[5] = (DWORD)pParams;
//
//    void* pCode = VirtualAllocEx(client.hProcess, NULL, sizeof(code),
//        MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
//    if (!pCode)
//    {
//        VirtualFreeEx(client.hProcess, pParams, 0, MEM_RELEASE);
//        return false;
//    }
//
//    WriteProcessMemory(client.hProcess, pCode, code, sizeof(code), NULL);
//
//    HANDLE hThread = CreateRemoteThread(client.hProcess, NULL, 0,
//        (LPTHREAD_START_ROUTINE)pCode, NULL, 0, NULL);
//
//    bool success = false;
//    if (hThread)
//    {
//        WaitForSingleObject(hThread, 50);
//        success = true;
//        CloseHandle(hThread);
//    }
//
//    VirtualFreeEx(client.hProcess, pParams, 0, MEM_RELEASE);
//    VirtualFreeEx(client.hProcess, pCode, 0, MEM_RELEASE);
//
//    return success;
//}
//
//// ==================== ATTACK SPEED MENU ====================
//int SelectAttackSpeed()
//{
//    SetColor(C_YELLOW);
//    printf("\n╔════════════════════════════════════════════════════════╗\n");
//    printf("║               ATTACK HIZ AYARI                         ║\n");
//    printf("╚════════════════════════════════════════════════════════╝\n");
//    SetColor(C_WHITE);
//
//    printf("\n");
//    SetColor(C_GREEN);  printf("[1] "); SetColor(C_WHITE);
//    printf("Normal (80ms)     - Güvenli, ~12 APS\n");
//
//    SetColor(C_CYAN);   printf("[2] "); SetColor(C_WHITE);
//    printf("Hızlı (50ms)      - Orta, ~20 APS\n");
//
//    SetColor(C_YELLOW); printf("[3] "); SetColor(C_WHITE);
//    printf("Çok Hızlı (30ms)  - Agresif, ~33 APS\n");
//
//    SetColor(C_RED);    printf("[4] "); SetColor(C_WHITE);
//    printf("Maksimum (10ms)   - Riskli, ~100 APS\n");
//
//    SetColor(C_GRAY);   printf("[5] "); SetColor(C_WHITE);
//    printf("Custom            - Kendin ayarla\n");
//
//    SetColor(C_YELLOW);
//    printf("\nSeçim (1-5): ");
//    SetColor(C_WHITE);
//
//    int choice;
//    scanf_s("%d", &choice);
//
//    int delay = 80;
//    switch (choice)
//    {
//    case 1: delay = 80; break;
//    case 2: delay = 50; break;
//    case 3: delay = 30; break;
//    case 4: delay = 10; break;
//    case 5:
//        printf("Delay (ms, 10-200): ");
//        scanf_s("%d", &delay);
//        if (delay < 10) delay = 10;
//        if (delay > 200) delay = 200;
//        break;
//    default:
//        delay = 80;
//    }
//
//    return delay;
//}
//
//// ==================== MAIN THREAD ====================
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//
//    g_myPID = GetCurrentProcessId();
//    sdk_client = (DWORD)GetModuleHandleA("metin2client.bin");
//
//    SetColor(C_CYAN);
//    printf("╔════════════════════════════════════════════════════════╗\n");
//    printf("║        MULTI-CLIENT ATTACK SYSTEM v2.0                ║\n");
//    printf("╚════════════════════════════════════════════════════════╝\n");
//    SetColor(C_WHITE);
//
//    Sleep(1000);
//
//    // Slave'leri ekle
//    AddSlaveClient();
//
//    if (g_slaveClients.empty())
//    {
//        FreeConsole();
//        return 0;
//    }
//
//    ShowSlaveList();
//
//    int attackDelay = SelectAttackSpeed();
//
//    SetColor(C_GREEN);
//    printf("\n[+] Ayarlar:\n");
//    printf("    Slave sayısı: %d\n", (int)g_slaveClients.size());
//    printf("    Attack delay: %dms (~%.1f APS)\n\n",
//        attackDelay, 1000.0f / attackDelay);
//    SetColor(C_WHITE);
//
//    SetColor(C_YELLOW);
//    printf("╔════════════════════════════════════════════════════════╗\n");
//    printf("║                  KONTROLLER                            ║\n");
//    printf("╠════════════════════════════════════════════════════════╣\n");
//    printf("║  [SPACE] - Attack Başlat/Durdur                        ║\n");
//    printf("║  [ESC]   - Çıkış                                       ║\n");
//    printf("╚════════════════════════════════════════════════════════╝\n\n");
//    SetColor(C_WHITE);
//
//    bool attacking = false;
//    int totalAttacks = 0;
//    int successCount = 0;
//    int failCount = 0;
//    DWORD startTime = 0;
//    bool wasSpacePressed = false;
//
//    while (true)
//    {
//        // SPACE - Toggle attack
//        bool spacePressed = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;
//        if (spacePressed && !wasSpacePressed)
//        {
//            attacking = !attacking;
//
//            if (attacking)
//            {
//                g_comboIndex = 0;
//                totalAttacks = 0;
//                successCount = 0;
//                failCount = 0;
//                startTime = GetTickCount();
//                g_lastAttackTime = 0;
//
//                SetColor(C_GREEN);
//                printf("\n[+] Attack BAŞLADI!\n\n");
//                SetColor(C_WHITE);
//            }
//            else
//            {
//                SetColor(C_RED);
//                printf("\n\n[−] Attack DURDU!\n");
//                SetColor(C_YELLOW);
//                printf("    Toplam: %d | Başarılı: %d | Başarısız: %d\n\n",
//                    totalAttacks, successCount, failCount);
//                SetColor(C_WHITE);
//            }
//
//            Sleep(200);
//        }
//        wasSpacePressed = spacePressed;
//
//        // ESC - Çıkış
//        if (GetAsyncKeyState(VK_ESCAPE) & 0x8000)
//        {
//            SetColor(C_YELLOW);
//            printf("\n\n[*] Çıkış...\n");
//            SetColor(C_WHITE);
//            break;
//        }
//
//        // Attack loop
//        if (attacking)
//        {
//            DWORD vid = engine::GetTargetVID();
//
//            if (vid != 0)
//            {
//                DWORD currentTime = GetTickCount();
//
//                if (currentTime - g_lastAttackTime >= attackDelay)
//                {
//                    UINT attack = g_comboIndex % 7;
//
//                    // Master attack
//                    engine::NetworkStreamSendAttackPacket(attack, vid);
//
//                    // Slave attacks
//                    int slaveSuccess = 0;
//                    for (int i = 0; i < g_slaveClients.size(); i++)
//                    {
//                        if (CallRemoteAttack(i, attack, vid))
//                            slaveSuccess++;
//                    }
//
//                    g_comboIndex++;
//                    totalAttacks++;
//                    successCount += slaveSuccess;
//                    failCount += (g_slaveClients.size() - slaveSuccess);
//                    g_lastAttackTime = currentTime;
//
//                    DWORD elapsed = currentTime - startTime;
//                    float aps = elapsed > 0 ? (totalAttacks * 1000.0f / elapsed) : 0;
//
//                    SetColor(C_CYAN);
//                    printf("\r[");
//                    SetColor(C_YELLOW);
//                    printf("VID: %-8u", vid);
//                    SetColor(C_CYAN);
//                    printf("] [");
//                    SetColor(C_GREEN);
//                    printf("Combo: %u", attack);
//                    SetColor(C_CYAN);
//                    printf("] [");
//                    SetColor(C_WHITE);
//                    printf("Total: %-5d", totalAttacks);
//                    SetColor(C_CYAN);
//                    printf("] [");
//                    SetColor(C_GREEN);
//                    printf("✓%d", successCount);
//                    SetColor(C_CYAN);
//                    printf("] [");
//                    SetColor(C_RED);
//                    printf("✗%d", failCount);
//                    SetColor(C_CYAN);
//                    printf("] [");
//                    SetColor(C_YELLOW);
//                    printf("APS: %.1f", aps);
//                    SetColor(C_CYAN);
//                    printf("]");
//                    SetColor(C_WHITE);
//                }
//            }
//            else
//            {
//                SetColor(C_YELLOW);
//                printf("\r[*] Hedef bekleniyor...                                              ");
//                SetColor(C_WHITE);
//                g_comboIndex = 0;
//            }
//        }
//
//        Sleep(1);
//    }
//
//    // Cleanup
//    for (auto& slave : g_slaveClients)
//        CloseHandle(slave.hProcess);
//
//    g_slaveClients.clear();
//    FreeConsole();
//    return 0;
//}



//struct DroppedItemData
//{
//    float x, y, z;
//    DWORD ItemID;
//    DWORD ItemVID;
//    int ItemCount;
//};
//
//namespace ItemManagers
//{
//    DWORD C_PythonItem = 0x2C335CC;
//    DWORD C_PythonItemMap = 0x10;    
//    DWORD C_PythonItemList = 0x0;
//    
//    DWORD C_PythonItemCoordX = 0x8;
//    DWORD C_PythonItemCoordY = 0xC;
//    DWORD C_PythonItemCoordZ = 0x10;
//}
//
//static DWORD* GetMainActorPtr()
//{
//    DWORD* CPythonCharacterManager = *(DWORD**)(sdk_client + ItemManagers::C_PythonItem);
//    if (!CPythonCharacterManager)
//        return nullptr;
//
//    DWORD* Instance = *(DWORD**)((DWORD)CPythonCharacterManager + ItemManagers::C_PythonItemMap);
//    if (!Instance)
//        return nullptr;
//
//    DWORD* FinalPointer = *(DWORD**)((DWORD)Instance + ItemManagers::C_PythonItemList);
//    if (!FinalPointer)
//        return nullptr;
//
//    return FinalPointer;
//}
//
//DWORD WINAPI adwadwadadawd21231312321(HMODULE hModule)
//{
//    OpenConsole();
//
//    while (true)
//    {
//        DWORD* itemPtr = GetMainActorPtr();
//        if (!itemPtr)
//        {
//            Sleep(500);
//            continue;
//        }
//
//        float x = *(float*)((DWORD)itemPtr + ItemManagers::C_PythonItemCoordX);
//        float y = *(float*)((DWORD)itemPtr + ItemManagers::C_PythonItemCoordY);
//        float z = *(float*)((DWORD)itemPtr + ItemManagers::C_PythonItemCoordZ);
//
//        printf("[ITEM] X: %.2f | Y: %.2f | Z: %.2f\n", x, y, z);
//        engine::NetworkStreamSendItemPickUpPacket(0x3F77);
//        Sleep(1000);
//    }
//
//    return 0;
//}

//namespace ItemManagers
//{
//    DWORD CPythonItem = 0x2C335CC;      // Python item manager base
//    DWORD CPython_MainClass = 0x20;     // Offset main class
//    DWORD CPython_List = 0x0;           // Offset item list
//    DWORD CPython_ItemX = 0x8;          // Item X
//    DWORD CPython_ItemY = 0xC;          // Item Y
//    DWORD CPython_ItemZ = 0x10;         // Item Z
//}
//
//DWORD WINAPI adwadwadadawd21231312321(HMODULE hModule)
//{
//    OpenConsole();
//    printf("[+] Item thread basladi!\n");
//
//    while (true)
//    {
//        DWORD* CPythonCharacterManager = *(DWORD**)(sdk_client + ItemManagers::CPythonItem);
//        if (!CPythonCharacterManager)
//        {
//            printf("[-] Step1 NULL!\n");
//            Sleep(1000);
//            continue;
//        }
//
//        printf("\n[+] Step1 OK: 0x%p\n", CPythonCharacterManager);
//        printf("[*] Tum pointer'lari test ediyorum...\n\n");
//        DWORD offsets[] = { 0x00, 0x04, 0x0C, 0x10, 0x14, 0x18, 0x30 };
//
//        for (int i = 0; i < sizeof(offsets) / sizeof(DWORD); i++)
//        {
//            DWORD offset = offsets[i];
//
//            __try
//            {
//                DWORD* Instance = *(DWORD**)((DWORD)CPythonCharacterManager + offset);
//
//                if (!Instance || Instance < (DWORD*)0x10000)
//                {
//                    printf("  [X] +0x%02X: NULL\n", offset);
//                    continue;
//                }
//
//                printf("  [?] +0x%02X: 0x%p -> ", offset, Instance);
//                DWORD* FinalPointer = *(DWORD**)((DWORD)Instance + 0x0);
//
//                if (!FinalPointer || FinalPointer < (DWORD*)0x10000)
//                {
//                    printf("Step3 NULL\n");
//                    continue;
//                }
//
//                printf("Step3 OK: 0x%p -> ", FinalPointer);
//                float x = *(float*)((DWORD)FinalPointer + 0x8);
//                float y = *(float*)((DWORD)FinalPointer + 0xC);
//                float z = *(float*)((DWORD)FinalPointer + 0x10);
//
//                if (x > -10000.0f && x < 10000.0f &&
//                    y > -10000.0f && y < 10000.0f &&
//                    z > -10000.0f && z < 10000.0f)
//                {
//                    printf("X:%.2f Y:%.2f Z:%.2f [BASARILI!]\n", x, y, z);
//                    printf("\n[!!!] DOGRU OFFSET BULUNDU: +0x%02X\n", offset);
//                    printf("[!!!] CPython_MainClass = 0x%02X yap!\n\n", offset);
//                }
//                else
//                {
//                    printf("X:%.2f Y:%.2f Z:%.2f [Mantıksız]\n", x, y, z);
//                }
//            }
//            __except (EXCEPTION_EXECUTE_HANDLER)
//            {
//                printf("  [X] +0x%02X: Exception!\n", offset);
//            }
//        }
//
//        Sleep(5000);
//    }
//
//    return 0;
//}


void Refine(int slot1, int sans)
{
    DWORD NetworkStream = *(DWORD*)(sdk_client + 0x37D704C);
    DWORD SendRefinePacket = (sdk_client + 0x99A2F0);
    _asm
    {
        mov ecx, NetworkStream
        push slot1
        push sans
        call SendRefinePacket
    }
}

struct FllngData {
    float angle;
    float speed;
    float radius;
};

std::vector<FllngData> fllng;
static DWORD fllngStartTime = 0;
static bool isScattered = false;
static float scatterProgress = 0.0f;
static std::vector<std::pair<float, float>> scatterDirs;
bool FunV1 = false;


static std::string cheatBuffer = "";
static DWORD lastKeyTime = 0;

void CheckCheatCode()
{
    DWORD now = GetTickCount();

    if (now - lastKeyTime > 2000)
        cheatBuffer = "";

    for (int key = 0x20; key <= 0x7E; key++)
    {
        if (GetAsyncKeyState(key) & 1)
        {
            char c = toupper((char)key);
            cheatBuffer += c;
            lastKeyTime = now;

            if (cheatBuffer.size() > 20)
                cheatBuffer = cheatBuffer.substr(cheatBuffer.size() - 20);

            if (cheatBuffer.find("FLLNG") != std::string::npos)
            {
                FunV1 = !FunV1;
                cheatBuffer = "";
                fllngStartTime = 0;
                isScattered = false;
                scatterProgress = 0.0f;
                scatterDirs.clear();
                fllng.clear();
            }
        }
    }
}

//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//    while (true)
//    {
//        CheckCheatCode();
//
//        if (FunV1)
//        {
//            try
//            {
//                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
//                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD))) { Sleep(1); continue; }
//
//                DWORD entityManager = *CharacterManager;
//                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager))) { Sleep(1); continue; }
//
//                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
//
//                std::vector<std::pair<DWORD, DWORD*>> validEntities;
//
//                for (auto& [id, Module] : m_kAliveInstMap)
//                {
//                    if (!Module) continue;
//                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);
//                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
//                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
//                    if (IsDead != 0) continue;
//                    if (x == c_players.x && y == c_players.y) continue;
//                    float dist = Dist(x, y, c_players.x, c_players.y);
//                    if (dist < Keyboard::C_ClickToTeleport_Distance)
//                        validEntities.emplace_back(id, Module);
//                }
//
//                const size_t MAX_ENTITIES = 10;
//                DWORD now = GetTickCount();
//
//                // Timer baslat
//                if (fllngStartTime == 0)
//                {
//                    fllngStartTime = now;
//                    isScattered = false;
//                    scatterProgress = 0.0f;
//                    scatterDirs.clear();
//                }
//
//                bool timeUp = (now - fllngStartTime >= 10000); // 10 saniye
//
//                if (timeUp && !isScattered)
//                {
//                    // Etrafa rastgele dagıt
//                    for (size_t i = 0; i < validEntities.size() && i < MAX_ENTITIES; ++i)
//                    {
//                        DWORD* Module = validEntities[i].second;
//                        float randX = (static_cast<float>(rand()) / RAND_MAX * 4000.0f) - 2000.0f;
//                        float randY = (static_cast<float>(rand()) / RAND_MAX * 4000.0f) - 2000.0f;
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsX) = c_players.x + randX;
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsY) = c_players.y + randY;
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = c_players.z;
//                    }
//
//                    isScattered = true;
//                    fllng.clear();
//                    scatterDirs.clear();
//                    scatterProgress = 0.0f;
//                }
//                else if (!timeUp)
//                {
//                    // Fllng ekle
//                    while (fllng.size() < validEntities.size() && fllng.size() < MAX_ENTITIES)
//                    {
//                        FllngData fd;
//                        fd.angle = static_cast<float>(rand()) / RAND_MAX * 2.0f * 3.14159265f;
//                        fd.speed = 0.01f + static_cast<float>(rand()) / RAND_MAX * 0.05f;
//                        fd.radius = 800.0f + static_cast<float>(rand()) / RAND_MAX * 500.0f;
//                        fllng.push_back(fd);
//                    }
//
//                    // Fllng uygula
//                    for (size_t i = 0; i < validEntities.size() && i < MAX_ENTITIES; ++i)
//                    {
//                        DWORD* Module = validEntities[i].second;
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsX) = c_players.x + fllng[i].radius * cosf(fllng[i].angle);
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsY) = c_players.y + fllng[i].radius * sinf(fllng[i].angle);
//                        *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = c_players.z;
//                        fllng[i].angle += fllng[i].speed;
//                        if (fllng[i].angle > 2.0f * 3.14159265f)
//                            fllng[i].angle -= 2.0f * 3.14159265f;
//                    }
//                }
//            }
//            catch (...) {}
//        }
//        else
//        {
//            fllngStartTime = 0;
//            isScattered = false;
//            scatterProgress = 0.0f;
//            scatterDirs.clear();
//            fllng.clear();
//        }
//        Sleep(1);
//    }
//}

//std::vector<OrbitData> orbits;
//DWORD WINAPI orbitsistemi(HMODULE hModule)
//{
//    OpenConsole();
//    while (true)
//    {
//        //static DWORD lastVID = 0; 
//        //DWORD click_to_vid = c_players.BannedVID;
//
//        //// sadece yeni bir hedef seçildiğinde yaz
//        //if (click_to_vid != 0 && click_to_vid != lastVID)
//        //{
//        //    printf("[ClickToVID] -> %u\n", click_to_vid);
//        //    lastVID = click_to_vid;
//        //}
//
//        if (Fun)
//        {
//            try
//            {
//                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
//                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
//                {
//                    Sleep(1);
//                    continue;
//                }
//
//                DWORD entityManager = *CharacterManager;
//                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
//                {
//                    Sleep(1);
//                    continue;
//                }
//
//                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
//
//                DWORD* playerModule = nullptr;
//                for (auto& [id, Module] : m_kAliveInstMap)
//                {
//                    if (!Module)
//                        continue;
//                    if (id == c_players.VID)
//                    {
//                        playerModule = Module;
//                        break;
//                    }
//                }
//
//                int closestIndex = -1;
//                float closestDist = FLT_MAX;
//
//                std::vector<std::pair<DWORD, DWORD*>> validEntities;
//
//                for (auto& [id, Module] : m_kAliveInstMap)
//                {
//                    if (!Module)
//                        continue;
//
//                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);
//                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);
//
//                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
//                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
//                    float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);
//
//                    float EntityRotationChanger = *(float*)((BYTE*)Module + metin2client::C_IsCharRotation);
//
//                    if (Type != 2 || IsDead != 0)
//                        continue;
//
//                    if (x == c_players.x && y == c_players.y && z == c_players.z)
//                        continue;
//
//                    float dist = Dist(x, y, c_players.x, c_players.y);
//
//                    if (dist < Keyboard::C_ClickToTeleport_Distance)
//                    {
//                        validEntities.emplace_back(id, Module);
//
//                        if (dist < closestDist)
//                        {
//                            closestDist = dist;
//                            closestIndex = (int)(validEntities.size() - 1);
//                        }
//                    }
//                }
//
//
//                const size_t MAX_ENTITIES = 50; 
//
//                if (orbits.size() < validEntities.size() && orbits.size() < MAX_ENTITIES) {
//                    for (size_t i = orbits.size(); i < validEntities.size() && i < MAX_ENTITIES; ++i) {
//                        OrbitData od;
//                        od.angle = static_cast<float>(rand()) / RAND_MAX * 2.0f * 3.14159265f;
//                        od.speed = 0.01f + static_cast<float>(rand()) / RAND_MAX * 0.05f;
//                        od.radius = 900.0f + static_cast<float>(rand()) / RAND_MAX * 200.0f;
//                        orbits.push_back(od);
//                    }
//                }
//
//                for (size_t i = 0; i < validEntities.size() && i < MAX_ENTITIES; ++i)
//                {
//                    DWORD* Module = validEntities[i].second;
//
//                    float newX = c_players.x + orbits[i].radius * cosf(orbits[i].angle);
//                    float newY = c_players.y + orbits[i].radius * sinf(orbits[i].angle);
//                    float newZ = c_players.z;
//
//                    *(float*)((BYTE*)Module + metin2client::C_CoordsX) = newX;
//                    *(float*)((BYTE*)Module + metin2client::C_CoordsY) = newY;
//                    *(float*)((BYTE*)Module + metin2client::C_CoordsZ) = newZ;
//
//                    orbits[i].angle += orbits[i].speed;
//                    if (orbits[i].angle > 2.0f * 3.14159265f)
//                        orbits[i].angle -= 2.0f * 3.14159265f;
//                }
//
//            }
//            catch (...)
//            {
//
//            }
//       }
//    }
//}


//void Horse(Vector2 Horsepos)
//{
//    try
//    {
//        float dx = Horsepos.x - c_players.x;
//        float dy = Horsepos.y - c_players.y;
//        c_players.Rotation = atan2f(dy, dx) * (180.0f / 3.14159265f);
//
//        float stepSize = 180.0f;
//        float targetX = c_players.x + ((fabs(Horsepos.x - c_players.x) < stepSize) ? (Horsepos.x - c_players.x) : Sign(Horsepos.x - c_players.x) * stepSize);
//        float targetY = c_players.y + ((fabs(Horsepos.y - c_players.y) < stepSize) ? (Horsepos.y - c_players.y) : Sign(Horsepos.y - c_players.y) * stepSize);
//
//        uintptr_t feature = ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer);
//        feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr1);
//        feature = ReadMemory<uintptr_t>(feature + metin2client::C_HorseAddr2);
//
//        ReadMemory<float>(feature + metin2client::C_HorseCoordX, &targetX, true);
//        ReadMemory<float>(feature + metin2client::C_HorseCoordY, &targetY, true);
//    }
//    catch (...)
//    {
//        // sessiz geç
//    }
//}
//
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//
//    while (true)
//    {
//        std::string MapName = engine::ApplicationBackground_GetCurrentMapName();
//        auto Alive = GetEntities();
//
//        if (!MapName.empty() && MapName == "metin2_map_deviltower1")
//        {
//            for (size_t i = 0; i < Alive.size(); ++i)
//            {
//                const auto& Entity = Alive[i];
//
//                Vector3 PlayerPos = { c_players.x, c_players.y, c_players.z };
//                Vector3 EntityPos = { Entity.x, Entity.y, Entity.z };
//
//                if (Entity.Type == 0)
//                {
//                    float Kat1Distance = Dist(EntityPos.x, EntityPos.y, PlayerPos.x, PlayerPos.y);
//                    if (Kat1Distance < 11000.0f)
//                    {
//                        Vector2 EntPos = { Entity.x, Entity.y };
//                        Horse(EntPos);
//                        std::this_thread::sleep_for(std::chrono::duration<double>(0.1)); // 0.1 saniye bekle
//                    }
//                }
//            }
//        }
//
//        std::this_thread::sleep_for(std::chrono::milliseconds(100)); // CPU'yu koru
//    }
//
//    return 0;
//}






// en azından çalısıyor :D
typedef void(__thiscall* DirectEnter)(DWORD This, UINT uChrSlotIndex);
DirectEnter Enter = (DirectEnter)((sdk_client + 0x96BE80));


//template<typename T>
//T ReadMemory(DWORD_PTR address, const T& def = T())
//{
//    if (!IsBadReadPtr(reinterpret_cast<void*>(address), sizeof(T)))
//    {
//        return *(T*)(address);
//    }
//    T asdd{};
//    return asdd;
//}


//typedef void(__thiscall* tCPythonBackgroundLocalPositionToGlobalPosition)(void* This, LONG& rLocalX, LONG& rLocalY);
//tCPythonBackgroundLocalPositionToGlobalPosition 
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//
//
//
//    while (true)
//    {
//        
//        Sleep(500);
//    }
//}

//TMapInfo* SafeBackgroundGlobalPositionToMapInfo(DWORD x, DWORD y)
//{
//    __try
//    {
//        return engine::BackgroundGlobalPositionToMapInfo(x, y);
//    }
//    __except (EXCEPTION_EXECUTE_HANDLER)
//    {
//        return nullptr;
//    }
//}


//static map<DWORD, TGroundItemInstance*> GetGroundItemList()
//{
//    map<DWORD, TGroundItemInstance*> vidList;
//    else
//    {
//        string player_name = GameFunctions::InstanceBaseGetNameString(GameFunctions::PlayerNEW_GetMainActorPtr());
//        TGroundItemInstanceMap m_GroundItemInstanceMap = *(TGroundItemInstanceMap*)(*reinterpret_cast<DWORD*>(*reinterpret_cast<DWORD*>(Globals::iCPythonItemInstance + 4) + 4));
//        for (TGroundItemInstanceMap::iterator itor = m_GroundItemInstanceMap.begin(); itor != m_GroundItemInstanceMap.end(); itor++)
//        {
//            string ownerShip = "";
//            try
//            {
//                ownerShip = itor->second->stOwnership;
//            }
//            catch (...)
//            {
//
//            }
//            if (ownerShip == "" || ownerShip == player_name)
//            {
//                vidList.insert(std::make_pair(itor->first, itor->second));
//            }
//        }
//    }
//    return vidList;
//}




typedef struct SItemTable_r156
{
    DWORD       dwVnum;
    DWORD       dwVnumRange;
    char        szName[24 + 1];
    char        szLocaleName[24 + 1];
    BYTE        bType;
    BYTE        bSubType;
    BYTE        bWeight;
    BYTE        bSize;
    DWORD       dwAntiFlags;
    DWORD       dwFlags;
    DWORD       dwWearFlags;
    DWORD       dwImmuneFlag;
    DWORD       dwIBuyItemPrice;
    DWORD		dwISellItemPrice;
} TItemTable;

typedef struct CItemData
{
    TItemTable m_ItemTable;
} TCItemData;

typedef struct CItemDataGlobal
{
    BYTE	unk2[60];
    TItemTable m_ItemTable;
} TCItemDataGlobal;


typedef std::map<DWORD, TCItemDataGlobal*> TItemMapGlobal;
typedef std::map<DWORD, TCItemData*> TItemMap;


std::map<DWORD, const char*> GetItemProtoNames()
{
    std::map<DWORD, const char*> itemsList;

    try
    {
        DWORD itemManager =*reinterpret_cast<DWORD*>(*reinterpret_cast<DWORD*>(metin2client::iCItemManagerInstance + 4) + 4);
        TItemMap& m_ItemMap = *reinterpret_cast<TItemMap*>(itemManager);
        for (auto& it : m_ItemMap)
        {
            TCItemData* pItem = it.second;
            if (!pItem)
                continue;

            const char* name = pItem->m_ItemTable.szLocaleName;

            if (!name || !name[0])
                name = pItem->m_ItemTable.szName;

            if (name && name[0])
                itemsList[it.first] = _strdup(name);
        }
    }
    catch (...)
    {
    }

    return itemsList;
}
// GetItemDate : e8 ? ? ? ? 89 45 ? 83 7d ? ? 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? e9 ? ? ? ? 68
// GetItemManager : 8b 0d ? ? ? ? 89 4d ? 8b 4d ? e8 ? ? ? ? 89 45 ? 83 7d ? ? 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? e9 ? ? ? ? 68
// GetString : e8 ? ? ? ? 89 85 ? ? ? ? 0f b6 8d ? ? ? ? 85 c9 74 ? 8b 95 ? ? ? ? 52 8d 85 ? ? ? ? 50 68 ? ? ? ? 68 ? ? ? ? 8d 8d ? ? ? ? 51 e8 ? ? ? ? 83 c4 ? eb ? 8b 95 ? ? ? ? 52 8d 85 ? ? ? ? 50 68 ? ? ? ? 68 ? ? ? ? 8d 8d ? ? ? ? 51 e8 ? ? ? ? 83 c4 ? 8d 95 ? ? ? ? 52 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 89 85 ? ? ? ? c6 45 ? ? 8d 8d ? ? ? ? e8 ? ? ? ? c7 45 ? ? ? ? ? 8d 8d ? ? ? ? e8 ? ? ? ? 8b 85 ? ? ? ? e9

//static bool ItemManagerGetItemDataPointer(DWORD dwItemID, DWORD** ppItemData)
//{
//    try
//    {
//        typedef bool(__thiscall* tItemManagerGetItemData)(void* This, DWORD dwItemID, DWORD** ppItemData);
//        DWORD calladdr = sdk_client + 0x25ADEF0;
//        DWORD CItemManagerInstance = *reinterpret_cast<DWORD*>(sdk_client + 0x2C432F0);
//        tItemManagerGetItemData ItemManagerGetItemDataPointer = reinterpret_cast<tItemManagerGetItemData>(calladdr);
//        return ItemManagerGetItemDataPointer((void*)CItemManagerInstance, dwItemID, ppItemData);
//    }
//    catch (...)
//    {
//        return false;
//    }
//}
//
//static const char* ItemDataGetName(DWORD dwItemID)
//{
//    try
//    {
//        typedef const char* (__thiscall* tItemDataGetName)(void* This);
//        DWORD calladdr = sdk_client + 0x2465CD0;
//        tItemDataGetName ItemDataGetName = reinterpret_cast<tItemDataGetName>(calladdr);
//
//        DWORD* pItemData = nullptr;
//        if (!ItemManagerGetItemDataPointer(dwItemID, &pItemData) || !pItemData)
//            return nullptr;
//
//        return ItemDataGetName(pItemData);
//    }
//    catch (...)
//    {
//        return nullptr;
//    }
//}
//static DWORD PlayerGetItemIndexV1(BYTE window_type, WORD cell)
//{
//    __try
//    {
//        void* playerBase = *(void**)(sdk_client + metin2client::C_CPythonPlayer);
//        if (!playerBase)
//            return 0;
//
//        void* player = (char*)playerBase + metin2client::C_Mng;
//        if (!player)
//            return 0;
//
//        DWORD* vtable = *(DWORD**)player;
//        if (!vtable)
//            return 0;
//
//        DWORD fnAddr = vtable[metin2client::C_ItemVTable / 4];
//        if (!fnAddr)
//            return 0;
//
//        struct TItemPos {
//            BYTE window_type;
//            WORD cell;
//        } pos;
//        pos.window_type = window_type;
//        pos.cell = cell;
//
//        typedef DWORD(__thiscall* tGetItemIndex)(void*, TItemPos);
//        tGetItemIndex GetItemIndex = (tGetItemIndex)fnAddr;
//
//        return GetItemIndex(player, pos);
//    }
//    __except (EXCEPTION_EXECUTE_HANDLER)
//    {
//        return 0;
//    }
//}
//std::unordered_map<DWORD, std::string> g_ItemProto;
//void LoadItemProto()
//{
//    std::ifstream file("C:\\Resource\\ProtoList\\item_proto_client.txt");
//    if (!file.is_open())
//    {
//        printf("[-] item_proto_client.txt acilamadi\n");
//        return;
//    }
//
//    g_ItemProto.clear();
//
//    std::string line;
//    while (std::getline(file, line))
//    {
//        if (line.empty())
//            continue;
//
//        std::istringstream iss(line);
//
//        DWORD vnum;
//        std::string name;
//
//        if (!(iss >> vnum))
//            continue;
//
//        std::getline(iss, name);
//
//        // tab / bosluk temizle
//        if (!name.empty() && (name[0] == '\t' || name[0] == ' '))
//            name.erase(0, 1);
//
//        g_ItemProto[vnum] = name;
//    }
//
//    printf("[+] %zu item proto yuklendi\n", g_ItemProto.size());
//}
//
//const char* GetItemNameFromProto(DWORD vnum)
//{
//    auto it = g_ItemProto.find(vnum);
//    if (it != g_ItemProto.end())
//        return it->second.c_str();
//
//    return "NOT FOUND IN TXT";
//}



//void ShowInventory()
//{
//    printf("=== INVENTORY sgh ===\n");
//
//    for (int i = 0; i < (45 * 5); i++)
//    {
//        DWORD vnum = engine::GetItemIndex(i);
//
//        if (vnum != 0)
//        {
//            const char* name = GetItemNameFromProto(vnum);
//            printf("[Slot %02d] %s (VNum: %u)\n", i, name ? name : "UNKNOWN", vnum);
//        }
//    }
//}


//static void NetworkStreamSendItemDropPacket(int slot, DWORD count = 1)
//{
//    try
//    {
//        typedef void(__thiscall* tSendItemDropPacket)(void* pThis, Sex pos, DWORD count);
//        tSendItemDropPacket SendItemDropPacket = (tSendItemDropPacket)(sdk_client + 0xEA060);
//
//        void* networkStream = sdk::iNetworkStreamInstance();
//        if (!networkStream)
//        {
//            return;
//        }
//
//        Sex pos(1, slot); 
//
//        SendItemDropPacket(networkStream, pos, count);
//
//    }
//    catch (...)
//    {
//    }
//}


std::string CleanName(const char* name)
{
    std::string result;
    for (int j = 0; name[j] != '\0'; j++)
    {
        unsigned char c = (unsigned char)name[j];
        switch (c)
        {
        case 0xE4: result += "ae"; break; 
        case 0xC4: result += "Ae"; break;  
        case 0xF6: result += "oe"; break;
        case 0xD6: result += "Oe"; break; 
        case 0xFC: result += "ue"; break;
        case 0xDC: result += "Ue"; break;
        case 0xDF: result += "ss"; break;
        case 0xFD: result += "i";  break;
        case 0xDD: result += "I";  break; 
        case 0xFE: result += "s";  break;
        case 0xDE: result += "S";  break;
        case 0xF0: result += "g";  break;
        case 0xD0: result += "G";  break;
        case 0xE7: result += "c";  break;
        case 0xC7: result += "C";  break;
        default:
            if (c >= 32 && c <= 126) result += (char)c;
            else return "";  
            break;
        }
    }
    return result;
}

DWORD WINAPI dungeona1(HMODULE hModule)
{
    OpenConsoleV2();

    while (true)
    {
        auto entities = GetEntities();
        for (int i = 0; i < (int)entities.size(); i++)
        {
            if (entities[i].Name == nullptr) continue;
            std::string clean = CleanName(entities[i].Name);
            if (clean.size() < 2) continue;
            printf("[%d] Type: %d | Name: %s\n", i, entities[i].Type, clean.c_str());
        }
        Sleep(2000);
    }
    return 0;
}


DWORD WINAPI dungeona(HMODULE hModule)
{
    OpenConsoleV2();

    while (true)
    {
       
    }
    return 0;
}

//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//
//    LoadItemProto(); // <-- SADECE 1 KEZ
//
//    while (true)
//    {
//        DWORD vnum = PlayerGetItemIndexV1(INVENTORY, 1);
//
//        if (vnum == 0)
//        {
//            printf("\r[Slot 01] EMPTY                     ");
//        }
//        else
//        {
//            const char* name = GetItemNameFromProto(vnum);
//
//            printf(
//                "\r[Slot 01] VNUM: %u (0x%X) | NAME: %s        ",
//                vnum, vnum, name
//            );
//        }
//
//        Sleep(300);
//    }
//    return 0;
//}



//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//    Sleep(3000);
//
//    while (true)
//    {
//        auto groundItemList = engine::GetGroundItemList();
//        if (groundItemList.empty())
//        {
//            Sleep(1000);
//            continue;
//        }
//
//        std::string playerName =
//            engine::InstanceBaseGetNameString(sdk::GetMainActorPtr());
//
//        Vector3 playerPos = { c_players.x, -c_players.y, c_players.z };
//
//        for (const auto& it : groundItemList)
//        {
//            DWORD itemVID = it.first;
//            TGroundItemInstance* item = it.second;
//
//            if (!item)
//                continue;
//
//            // ownership kontrolü
//            if (!item->stOwnership.empty() &&
//                item->stOwnership != playerName)
//                continue;
//
//            Vector3 itemPos = {
//                item->v3EndPosition.x,
//                -item->v3EndPosition.y,
//                item->v3EndPosition.z
//            };
//
//            float distance = engine::CountDistanceTwoPoints(
//                playerPos.x, playerPos.y,
//                itemPos.x, itemPos.y
//            );
//
//            if (fabs(itemPos.z - playerPos.z) > 200.f)
//                continue;
//
//            if (distance < 300.f)
//            {
//                engine::NetworkStreamSendItemPickUpPacket(itemVID);
//                continue;
//            }
//
//            if (distance < 5000.f)
//            {
//                std::vector<Vector3> steps =
//                    engine::DivideTwoPointsByDistance(300.f, playerPos, itemPos);
//
//                for (const Vector3& step : steps)
//                {
//                    if (fabs(step.z - playerPos.z) > 200.f)
//                        continue;
//
//                    engine::NetworkStreamSendCharacterStatePacket(
//                        step, 0.f, 0, 0
//                    );
//                }
//
//                engine::NetworkStreamSendItemPickUpPacket(itemVID);
//            }
//        }
//
//        Sleep(3000);
//    }
//
//    return 0;
//}




//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//    Sleep(3000);
//
//    while (true)
//    {
//
//        auto groundItems = engine::GetGroundItemList();
//
//        printf("Ground item count: %d\n", groundItems.size());
//
//        for (auto& it : groundItems)
//        {
//            DWORD vid = it.first;
//            TGroundItemInstance* item = it.second;
//
//            if (!item)
//                continue;
//
//            printf(
//                "VID: %u | VNUM: %u | OWNER: %s | POS: [%.2f %.2f %.2f]\n",
//                vid,
//                item->dwVirtualNumber,
//                item->stOwnership.c_str(),
//                item->v3EndPosition.x,
//                item->v3EndPosition.y,
//                item->v3EndPosition.z
//            );
//            engine::NetworkStreamSendItemPickUpPacket(18910600);
//        }
//
//
//        Sleep(3000);
//    }
//
//    return 0;
//}

/*__try
  {


  }
  __except (EXCEPTION_EXECUTE_HANDLER)
  {
      printf("[ERROR] Exception: 0x%08X\n", GetExceptionCode());
  }*/

//static bool NetworkStreamSendCommandPacket(DWORD a1, DWORD a2, const char* a3)
//{
//    try
//    {
//        typedef bool(__thiscall* tCPythonNetworkStreamSendCommandPacket)(void* NetworkStream, DWORD Arg1, DWORD Arg2, const char* Text);
//        tCPythonNetworkStreamSendCommandPacket SendCommand = (tCPythonNetworkStreamSendCommandPacket)(sdk_client + metin2client::C_SendCommandPacket);
//
//        void* pNetStream = sdk::iNetworkStreamInstance();
//        if (!pNetStream || !SendCommand)
//            return false;
//
//        SendCommand(pNetStream, a1, a2, "");
//    }
//    catch (...)
//    {
//        return false;
//    }
//}

static void TeleportPos(const Vector3& position, float rotation)
{
    engine::NetworkStreamSendCharacterStatePacket(Vector3{ position.x, position.y, position.z }, rotation, 0, 0);
}




//bool Yürü_ve_Kes = true;
//DWORD WINAPI dungeona(HMODULE hModule)
//{
//    OpenConsole();
//    while (true)
//    {
//
//        std::string MapName = engine::ApplicationBackground_GetCurrentMapName();
//        if (!MapName.empty())
//        {
//            printf("Map: %s\n", MapName.c_str()); // basarılı :d
//        }
//     
//
//
//
//        //auto Alive = GetEntities();
//        //for (size_t i = 0; i < Alive.size(); ++i)
//        //{
//        //    const auto& Entity = Alive[i];
//
//        //    Vector3 PlayerPos = { c_players.x , c_players.y,c_players.z };
//        //    Vector3 MiningPos = { Entity.x,Entity.y,Entity.z };
//
//
//        //    float Distance = Dist(MiningPos.x, MiningPos.y, PlayerPos.x, PlayerPos.y);
//
//        //    if (Distance < 50.0f)
//        //    {
//        //        if (Entity.Type == 1)
//        //        {
//
//
//        //           /* uintptr_t Feature_X = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsX;
//        //            ReadMemory<float>(Feature_X, &MiningPos.x, true);
//        //            uintptr_t Feature_Y = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsY;
//        //            ReadMemory<float>(Feature_Y, &MiningPos.y, true);
//        //            uintptr_t Feature_Z = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_CoordsZ;
//        //            ReadMemory<float>(Feature_Z, &MiningPos.z, true);*/
//
//        //            engine::NetworkStreamPythonClickMining(Entity.VID);
//
//        //            cout << "Entity X : " << Entity.VID << " / " << Alive.size() << "\n";
//        //            cout << "Entity VID : " << Entity.VID << " / " << Alive.size() << "\n";
//        //        }
//
//        //    }
//        //}
//        auto Alive = GetEntities();
//        if (MapName == "É(t♥ñ(t♥p@x♥☺")
//        {
// 
//           
//        }
//    }
//    Sleep(900);
//}




static void Boost()
{
    static int BoostCount = 0;

    if (!(GetAsyncKeyState('W') & 0x8000) &&
        !(GetAsyncKeyState('A') & 0x8000) &&
        !(GetAsyncKeyState('S') & 0x8000) &&
        !(GetAsyncKeyState('D') & 0x8000))
    {
        return;
    }

    Vector3 Position;
    Vector3 newPosition;
    engine::GetMainGetPixelPosition(&Position); 


    float rotation = -DEG2RAD(c_players.Rotation - 180);
    float xx = Keyboard::BoostStep * sin(rotation);
    float yy = Keyboard::BoostStep * cos(rotation);


    newPosition.x = Position.x + xx;
    newPosition.y = Position.y - yy;

    engine::SetPixelPosition(newPosition);

    if (BoostCount >= 1)
    {
        BoostCount = 0;
        engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 1, 0);
    }
    else
    {
        engine::NetworkStreamSendCharacterStatePacket(newPosition, rotation, 0, 0);
        BoostCount++;
    }
}

//static bool NetworkStreamSendItemPickUpPacket(DWORD vid)
//{
//    try
//    {
//        typedef bool(__thiscall* tNetworkStreamSendItemPickUpPacket)(void* This, DWORD vid);
//        DWORD calladdr = base + 0x99FA0;
//        DWORD NetworkStream = *(DWORD*)(base + 0x11987DC);
//        tNetworkStreamSendItemPickUpPacket NetworkStreamSendItemPickUpPacket = reinterpret_cast<tNetworkStreamSendItemPickUpPacket>(calladdr);
//        return NetworkStreamSendItemPickUpPacket((void*)NetworkStream, vid);
//    }
//    catch (...)
//    {
//        return false;
//    }
//}

//static void Fish(bool type)
//{
//    DWORD PythonPlayer = *(DWORD*)(sdk_client + 0x11AEE64);
//    DWORD call2 = sdk_client + 0xC0710;
//    unsigned char param = type;  // bool -> 1 byte
//
//    __asm
//    {
//        mov ecx, PythonPlayer
//        movzx eax, param    // eax = param (1-byte genişletildi)
//        push eax
//        call call2
//        add esp, 4          // stack cleanup
//    }
//}

static bool Fish(bool type)
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



//bool AutoFish = true;
//
//DWORD WINAPI test(HMODULE hModule)
//{
//    DWORD lastFishTime = 0;
//    bool pressed = false;
//
//    while (true)
//    {
//        if (AutoFish)
//        {
//            DWORD currentTime = GetTickCount();
//
//            // Eğer ilk kez başlıyorsa bas
//            if (!pressed)
//            {
//                engine::SetAttackKeyState(true);
//                lastFishTime = currentTime;
//                pressed = true;
//            }
//
//            // 0.1 saniye sonra bırak
//            if (pressed && currentTime - lastFishTime >= 100 && currentTime - lastFishTime < 3100)
//            {
//                engine::SetAttackKeyState(false);
//            }
//
//            // 3 saniye sonra tekrar bas
//            if (pressed && currentTime - lastFishTime >= 3000)
//            {
//                engine::SetAttackKeyState(true);
//                lastFishTime = currentTime; // zaman sıfırla
//            }
//        }
//
//        Sleep(1);
//    }
//}



//bool ilk_yazılan = false;
//bool ikinci_yazılan = false;
//bool test_a = true;
//DWORD WINAPI test(HMODULE hModule)
//{
//    while (true)
//    {
//        try
//        {
//            if (test_a)
//            {
//                if (!ilk_yazılan)
//                {
//                    uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_OneHit;
//                    int ilk = 1;
//                    ReadMemory<int>(Feature, &ilk, true);
//                    ilk_yazılan = true;
//                    ikinci_yazılan = false;
//                }
//                Sleep(1);
//                if (ilk_yazılan && !ikinci_yazılan)
//                {
//                    uintptr_t Feature = ReadMemory<uintptr_t>(ReadMemory<uintptr_t>(Ez_game_base + metin2client::C_LocalPlayer) + metin2client::C_LocalAddr) + metin2client::C_OneHit;
//                    int ilk = 2;
//                    ReadMemory<int>(Feature, &ilk, true);
//                    ikinci_yazılan = true;
//                    ilk_yazılan = false;
//                }
//            }
//        }
//        catch (...)
//        {
//
//        }
//        Sleep(1);
//    }
//    return true;
//}

// çalısıyor
//static void ConnectTCP(const std::string& ip, int port)
//{
//    DWORD CPythonNetworkStream = *(DWORD*)(sdk_client + 0x361030); 
//    DWORD Connect = sdk_client + 0x84AC0;
//
//    __asm
//    {
//        mov ecx, CPythonNetworkStream 
//        push port
//        lea eax, ip
//        push eax
//        call Connect
//    }
//}

//static void* iNetworkStreamInstance()
//{
//    DWORD inst = *(DWORD*)(sdk_client + 0x361030);
//    if (!inst) return nullptr;
//    return (void*)inst;
//}


//static void Disconnect()
//{
//    DWORD CPythonNetworkStream = *(DWORD*)(sdk_client + 0x361030);
//    DWORD DisconnectFunc = sdk_client + 0x85080;
//
//    __asm
//    {
//        push esi
//        mov esi, CPythonNetworkStream
//        mov ecx, esi
//        call DisconnectFunc
//        pop esi
//    }
//}


std::vector<Vector3> route = {
    {8799.56f, 9616.69f, 16512.5f},
    {8799.63f, 11471.4f, 16512.5f},
    {8996.43f, 12563.3f, 16512.5f},
    {9299.83f, 14433.8f, 16512.5f},
    {9188.73f, 16475.0f, 16389.5f},
    {9098.4f, 18517.3f, 16502.6f},
    {9008.81f, 20543.1f, 16488.0f},
    {8724.0f, 22537.1f, 16512.5f},
    {8510.16f, 24533.0f, 16512.5f},
    {8537.33f, 26560.6f, 16389.5f},
    {8564.72f, 28604.8f, 16518.0f},
    {8592.1f, 30648.9f, 16488.0f},
    {8619.27f, 32676.4f, 16512.5f},
    {9023.47f, 34657.5f, 16512.5f},
    {9519.66f, 36454.2f, 16389.6f},
    {10649.6f, 38146.4f, 16488.0f},
    {12395.5f, 38645.3f, 16512.5f},
    {14416.5f, 38790.1f, 16512.5f},
    {16457.9f, 38775.7f, 16409.7f},
    {18488.2f, 38863.8f, 16519.0f},
    {20512.1f, 38788.0f, 16488.0f},
    {22536.6f, 38892.4f, 16512.5f},
    {24580.4f, 38934.4f, 16512.5f},
    {26622.5f, 39011.5f, 16488.0f},
    {28643.1f, 39173.0f, 16519.0f},
    {30686.9f, 39137.7f, 16488.0f},
    {32730.7f, 39088.0f, 16512.5f},
    {34774.7f, 39063.7f, 16512.5f},
    {36818.9f, 39051.4f, 16488.0f}
};

std::vector<Vector3> ilk_gelen_kayıt;
int targetVID = 0; // hedeflenen entity'nin VID'si

//DWORD WINAPI test(HMODULE hModule)
//{
//    OpenConsole();
//
//    while (true)
//    {
//        auto entities = GetEntities();
//
//        // Eğer hedef yoksa yeni entity seç
//        if (ilk_gelen_kayıt.empty())
//        {
//            for (const auto& ent : entities)
//            {
//                if (ent.VID == 0 || (ent.x == 0.f && ent.y == 0.f && ent.z == 0.f))
//                    continue;
//
//                if (ent.Type == 0) // istediğin entity tipi
//                {
//                    Vector3 Entitypos = { ent.x, ent.y, ent.z };
//                    ilk_gelen_kayıt.push_back(Entitypos);
//                    targetVID = ent.VID; // hangi entityye gidiyoruz
//
//                    engine::InstanceBaseNEW_MoveToDestPixelPositionDirection(&ilk_gelen_kayıt[0]);
//                    break; // sadece ilk entityyi al
//                }
//            }
//        }
//
//        // Eğer hedef varsa, öldü mü kontrol et
//        if (!ilk_gelen_kayıt.empty())
//        {
//            auto target = std::find_if(entities.begin(), entities.end(),
//                [&](const auto& e) { return e.VID == targetVID; });
//
//            if (target != entities.end())
//            {
//                // Hedef hâlâ canlıysa bekle
//                while (target->IsDead == 0)
//                {
//                    Sleep(50);
//                    // entities tekrar güncellenmeli veya target güncellenmeli
//                }
//            }
//
//            // Hedef öldü → temizle
//            ilk_gelen_kayıt.clear();
//            targetVID = 0;
//        }
//
//        Sleep(100);
//    }
//
//    return 0;
//}



//static char dosya_yolu[512] = "C:\\Users\\Girl\\Desktop\\a.py";
//
//DWORD WINAPI test(LPVOID param) {
//    HMODULE hPy = LoadLibraryA("python27.dll");
//    if (!hPy) { /* hata: python27.dll yok */ }
//
//    try {
//        python27 py(hPy);
//        bool ok = pyscript_load("C:\\Users\\Girl\\Desktop\\multihack.py", py);
//        if (!ok) { /* hata mesajları konsolda yazılır */ }
//    }
//    catch (const Exception& e) {
//        std::cerr << "Init error: " << e.what() << "\n";
//    }
//    FreeLibrary(hPy);
//
//    return 0;
//}

//DWORD WINAPI test(HMODULE hModule)
//{
//    OpenConsole();
//    cout << "[*] Test thread started." << endl;
//    while (true)
//    {
//        try
//        {
//            std::string MapName = engine::ApplicationBackground_GetCurrentMapName();
//            if (MapName == "metin2_map_deviltower1")
//            {
//                DWORD* CharacterManager = reinterpret_cast<DWORD*>(metin2client + metin2client::C_LocalPlayer);
//                if (!CharacterManager || IsBadReadPtr(CharacterManager, sizeof(DWORD)))
//                { Sleep(1);continue;}
//
//                DWORD entityManager = *CharacterManager;
//                if (!entityManager || IsBadReadPtr((void*)(entityManager + metin2client::m_kAliveInstMap), sizeof(PythonCharacterManager)))
//                { Sleep(1); continue;}
//                
//                PythonCharacterManager& m_kAliveInstMap = *(PythonCharacterManager*)(entityManager + metin2client::m_kAliveInstMap);
//
//                DWORD* playerModule = nullptr;
//                for (auto& [id, Module] : m_kAliveInstMap)
//                {
//                    if (!Module)
//                        continue;
//                    if (id == c_players.VID)
//                    {
//                        playerModule = Module;
//                        break;
//                    }
//                }
//
//                int closestIndex = -1;
//                float closestDist = FLT_MAX;
//
//                std::vector<std::pair<DWORD, DWORD*>> validEntities;
//
//                for (auto& [id, Module] : m_kAliveInstMap)
//                {
//                    if (!Module)
//                        continue;
//
//                    int Type = *(int*)((BYTE*)Module + metin2client::C_IsRadarType);
//                    int IsDead = *(int*)((BYTE*)Module + metin2client::C_IsDead);
//
//                    float x = *(float*)((BYTE*)Module + metin2client::C_CoordsX);
//                    float y = *(float*)((BYTE*)Module + metin2client::C_CoordsY);
//                    float z = *(float*)((BYTE*)Module + metin2client::C_CoordsZ);
//
//                    if (Type != 6 || IsDead != 0)
//                        continue;
//
//                    if (x == c_players.x && y == c_players.y && z == c_players.z)
//                        continue;
//
//                    float dist = Dist(x, y, c_players.x, c_players.y);
//
//                    if (dist < Keyboard::C_ClickToTeleport_Distance)
//                    {
//                        validEntities.emplace_back(id, Module);
//
//                        if (dist < closestDist)
//                        {
//                            closestDist = dist;
//                            closestIndex = (int)(validEntities.size() - 1);
//                        }
//                    }
//                }
//
//                for (int i = 0; i < (int)validEntities.size(); ++i)
//                {
//                    DWORD id = validEntities[i].first;
//                    DWORD* Module = validEntities[i].second;
//                    std::cout << "[*] Valid entities: " << validEntities.size() << std::endl;
//
//                }
//            }
//        }
//        catch (...)
//        {
//            cout << "[!] Exception caught!" << endl;
//        }
//
//        Sleep(400);
//    }
//
//    return 0;
//}