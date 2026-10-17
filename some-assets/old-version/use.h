#include <windows.h>
#include <iostream>
#include <map>
#include <memory>
#include <string>
using namespace std;
#include <thread>
#include "engine.h"

static char chat_spawmner_message[65] = "";
DWORD WINAPI Set_ChatSpawmner(HMODULE hModule)
{
    while (true)
    {
        try
        {
            if (Keyboard::ChatSpawmmer)
            {
                if (Keyboard::ChatSpawmmer_Green) // yapıldı!!
                {
                    std::string message = std::string(/*|c00FF00*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x0D\x0A\x7D\x7E" + 0x2CAE8000).s) + chat_spawmner_message + /*|r|H|h*/XorStrTR<0x47, 7, 0x2CAE8000>("\x3B\x3A\x35\x02\x37\x24" + 0x2CAE8000).s;
                    engine::NetworkStreamSendChatPacket(message.c_str(), 0);
                }
                if (Keyboard::ChatSpawmmer_Yellow) // Yapıldı!!
                {
                    std::string message = std::string(/*|cFFFF00*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x0D\x0A\x7D\x7E" + 0x2CAE8000).s) + chat_spawmner_message + /*|r|H|h*/XorStrTR<0x47, 7, 0x2CAE8000>("\x3B\x3A\x35\x02\x37\x24" + 0x2CAE8000).s;
                    engine::NetworkStreamSendChatPacket(message.c_str(), 0);
                }
                if (Keyboard::ChatSpawmmer_Ping) // Yapıldı!!
                {
                    std::string message = std::string(/*|cFFC0CB*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x08\x7C\x0E\x0C" + 0x2CAE8000).s) + chat_spawmner_message + /*|r|H|h*/XorStrTR<0x47, 7, 0x2CAE8000>("\x3B\x3A\x35\x02\x37\x24" + 0x2CAE8000).s;
                    engine::NetworkStreamSendChatPacket(message.c_str(), 0);
                }
                if (Keyboard::ChatSpawmmer_Blue) // Yapıldı!!
                {
                    std::string message = std::string(/*|c0000FF*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x7B\x7C\x0B\x08" + 0x2CAE8000).s) + chat_spawmner_message + /*|r|H|h*/XorStrTR<0x47, 7, 0x2CAE8000>("\x3B\x3A\x35\x02\x37\x24" + 0x2CAE8000).s;
                    engine::NetworkStreamSendChatPacket(message.c_str(), 0);
                }
            }
        }
        catch (...)
        {

        }
        Sleep(800);
    }
    return true;
}

static char EffectID[65] = "1, 0, -1";
static bool AllEffectID = false;

DWORD WINAPI Set_Affect(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::EffectChanger && !AllEffectID)
            {
                char temp[65];
                strcpy(temp, EffectID);

                int ids[3] = { 0 };
                int index = 0;

                char* token = strtok(temp, ",");
                while (token != nullptr && index < 3)
                {
                    ids[index++] = atoi(token);
                    token = strtok(nullptr, ",");
                }

                engine::InstanceBase__SetAffect(ids[0], ids[1], ids[2]);
                AllEffectID = true;
            }
            else if (!Keyboard::EffectChanger)
            {
                AllEffectID = false;
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }

        Sleep(800);
    }

    return TRUE;
}

// ============================================== AutoRevive ============================================== //



DWORD WINAPI Set_Item_Inventory(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::AutoPot)
            {
                if (Keyboard::C_PotSlot2 == 2) // Slot3
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 2));
                }
                if (Keyboard::C_PotSlot3 == 3) // Slot4
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 3));
                }
                if (Keyboard::C_PotSlot4 == 4) // Slot5
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 4));
                }
                if (Keyboard::C_PotSlot5 == 5) // Slot6
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 5));
                }
                if (Keyboard::C_PotSlot6 == 6) // Slot7
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 6));
                }
                if (Keyboard::C_PotSlot7 == 7) // Slot8
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 7));
                }
                if (Keyboard::C_PotSlot8 == 8) // Slot9
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 8));
                }
                if (Keyboard::C_PotSlot9 == 9) // Slot10
                {
                    engine::NetworkStreamSendItemUsePacket(TItemPos(INVENTORY, 9));
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
        Sleep(3000);
    }
    return true;
}


// ============================================== Demirci ============================================== //


DWORD WINAPI Set_Blacksmith(HMODULE hModule)
{
    //OpenConsole();

    bool* UpgradeSlot[20] = {
        &Keyboard::C_Blacksmith_Slot1,  &Keyboard::C_Blacksmith_Slot2,
        &Keyboard::C_Blacksmith_Slot3,  &Keyboard::C_Blacksmith_Slot4,
        &Keyboard::C_Blacksmith_Slot5,  &Keyboard::C_Blacksmith_Slot6,
        &Keyboard::C_Blacksmith_Slot7,  &Keyboard::C_Blacksmith_Slot8,
        &Keyboard::C_Blacksmith_Slot9,  &Keyboard::C_Blacksmith_Slot10,
        &Keyboard::C_Blacksmith_Slot11, &Keyboard::C_Blacksmith_Slot12,
        &Keyboard::C_Blacksmith_Slot13, &Keyboard::C_Blacksmith_Slot14,
        &Keyboard::C_Blacksmith_Slot15, &Keyboard::C_Blacksmith_Slot16,
        &Keyboard::C_Blacksmith_Slot17, &Keyboard::C_Blacksmith_Slot18,
        &Keyboard::C_Blacksmith_Slot19, &Keyboard::C_Blacksmith_Slot20
    };

    while (true)
    {
        if (Keyboard::C_Blacksmith)
        {
            for (int i = 0; i < 20; ++i)
            {
                if (*UpgradeSlot[i])
                    engine::NetworkStreamSendRefinePacket((int)i, (int)0);
            }
        }
        Sleep(Keyboard::C_BlacksmithSpeed);
    }
}


// ============================================== AutoRevive ============================================== //
DWORD WINAPI Set_AutoRevive(HMODULE hModule)
{
    DWORD ölüm = 0;
    while (true)
    {
        if (Keyboard::AutoRevive)
        {
            if (c_players.IsDead == 1)
            {
                if (ölüm == 0)
                {
                    ölüm = GetTickCount();
                }
                else if (GetTickCount() - ölüm >= Keyboard::AutoRevive_Time) // 9 saniye
                {
                    engine::NetworkStreamSendChatPacket(/*/restart_here*/XorStrTR<0x47, 14, 0x2CAE8000>("\x68\x3A\x2C\x39\x3F\x2D\x3F\x3A\x10\x38\x34\x20\x36" + 0x2CAE8000).s, 0);
                    ölüm = 0;
                }
            }
            else
            {
                ölüm = 0;
            }
        }
        if (Keyboard::AutoReviveGF)
        {
            engine::NetworkStreamSendCommandPacket(5, 1, "");
        }
        Sleep(400);
    }
}
// ============================================== LoncaXP Donate ============================================== //
DWORD WINAPI Set_DonateLonca(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::XPDonate)
            {
                if (Keyboard::C_XPDonate_Lonca)
                {
                    int XP = engine::GetPlayerXP();
                    if (XP >= Keyboard::XPDonate_Limit)
                    {
                        engine::NetworkStreamSendGuildOfferPacket(XP);
                    }
                }
                if (Keyboard::XPDonate_Level)
                {
                    if (c_players.IsLvl == Keyboard::XPDonate_lvl)  // <-- burada '==' olmalı
                    {
                        int XP = engine::GetPlayerXP();
                        if (XP >= Keyboard::XPDonate_Limit)
                        {
                            engine::NetworkStreamSendGuildOfferPacket(XP);
                        }
                    }
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
        Sleep(Keyboard::XPDonate_Speed);
    }
    return true;
}

//// ============================================== Rota ============================================== //
//
//std::vector<Vector3> save_to_rota;
//int current_rota_index = 0;
//DWORD last_save_time = 0;
//DWORD last_move_time = 0;
//const DWORD save_interval = 2000;
//const DWORD move_interval = 25;
//
//
//void RotaKaydet(const std::string& filename)
//{
//    std::ofstream file(filename);
//    if (!file.is_open())
//        return;
//
//    for (const auto& pos : save_to_rota)
//    {
//        file << pos.x << " " << pos.y << " " << pos.z << "\n";
//    }
//
//    file.close();
//}
//
//Vector3 last_player_pos = { 0.f, 0.f, 0.f };
//bool is_moving = false;
//const float move_threshold = 1.0f;
//
//bool ileri_git = false;
//bool rota_basladi = false;
//
//bool Rota_devam = true;
//
//DWORD WINAPI Set_Rota(HMODULE hModule)
//{
//    while (true)
//    {
//        try
//        {
//            if (Keyboard::Route)
//            {
//                for (int i = 0; i < 231; i += 4)
//                {
//                    if (Keyboard::C_RouteSave)
//                    {
//                        RotaKaydet("Route.txt");
//                    }
//                    Vector3 Player;
//                    engine::GetMainGetPixelPosition(&Player);
//                    DWORD now = GetTickCount();
//
//                    float dx = Player.x - last_player_pos.x;
//                    float dy = Player.y - last_player_pos.y;
//                    float dz = Player.z - last_player_pos.z;
//                    float dist_sq = dx * dx + dy * dy + dz * dz;
//
//                    is_moving = (dist_sq > move_threshold * move_threshold);
//                    last_player_pos = Player;
//                    if (Keyboard::C_RouteRecording && is_moving)
//                    {
//                        if (now - last_save_time > save_interval)
//                        {
//                            if (save_to_rota.empty() ||
//                                (fabs(save_to_rota.back().x - Player.x) > 0.01f ||
//                                    fabs(save_to_rota.back().y - Player.y) > 0.01f ||
//                                    fabs(save_to_rota.back().z - Player.z) > 0.01f))
//                            {
//                                save_to_rota.push_back(Player);
//                                last_save_time = now;
//                            }
//                        }
//                    }
//                    if (Keyboard::C_ClearRoute)
//                    {
//                        save_to_rota.clear();
//                        current_rota_index = 0;
//                        rota_basladi = false;
//                        ileri_git = false;
//                        Keyboard::C_ClearRoute = false;
//                    }
//                    if (Keyboard::C_PlayRoute && !save_to_rota.empty())
//                    {
//                        if (!rota_basladi)
//                        {
//                            current_rota_index = (int)save_to_rota.size() - 1;
//                            ileri_git = false;
//                            rota_basladi = true;
//                        }
//                        if (now - last_move_time > move_interval)
//                        {
//                            if (current_rota_index < 0) current_rota_index = 0;
//                            if (current_rota_index >= (int)save_to_rota.size()) current_rota_index = (int)save_to_rota.size() - 1;
//
//                            Vector3 hedef = save_to_rota[current_rota_index];
//
//                            float dxh = Player.x - hedef.x;
//                            float dyh = Player.y - hedef.y;
//                            float dzh = Player.z - hedef.z;
//                            float dist_target_sq = dxh * dxh + dyh * dyh + dzh * dzh;
//                            const float target_reach_threshold = 50.0f * 50.0f;
//
//                            if (dist_target_sq < target_reach_threshold)
//                            {
//                                if (ileri_git)
//                                {
//                                    current_rota_index++;
//                                    if (current_rota_index >= (int)save_to_rota.size() - 1)
//                                    {
//                                        current_rota_index = (int)save_to_rota.size() - 1;
//                                        ileri_git = false;
//                                    }
//                                }
//                                else
//                                {
//                                    current_rota_index--;
//                                    if (current_rota_index <= 0)
//                                    {
//                                        current_rota_index = 0;
//                                        ileri_git = true;
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                auto Entity = GetEntities();
//                                for (const auto& ent : Entity)
//                                {
//                                    Vector3 EnemyPos = { ent.x, -ent.y, ent.z };
//                                    Vector3 PlayerPos = { c_players.x, -c_players.y, c_players.z };
//
//                                    float Distance = Dist(ent.x, ent.y, c_players.x, c_players.y); // 2D mesafe
//
//                                    bool Rota_devam = true;
//
//                                    if (Distance < 1000.0f)
//                                    {
//                                        // Metin bulursa dur
//                                        Rota_devam = false;
//
//                                        if (ent.Type == 2) // Metin2
//                                        {
//                                            if (ent.IsDead != 0) // Ölüyse tekrar hareket edebilir
//                                            {
//                                                uintptr_t addr = Ez_memory::Adres(Ez_game_base + metin2client::C_Localfarm, { metin2client::C_Localfarm_addr });
//                                                if (addr)
//                                                {
//                                                    *(DWORD*)addr = ent.VID;
//                                                }
//                                                Rota_devam = true;
//                                            }
//                                        }
//                                    }
//
//                                    if (Rota_devam)
//                                    {
//                                        engine::InstanceBaseNEW_MoveToDestPixelPositionDirection(&hedef);
//                                    }
//                                }
//
//                            }
//
//                            last_move_time = now;
//                        }
//                    }
//                    else
//                    {
//                        current_rota_index = 0;
//                        rota_basladi = false;
//                        ileri_git = false;
//                    }
//                }
//            }
//        }
//        catch (...)
//        {
//        }
//
//        Sleep(10);
//    }
//    return TRUE;
//}


// ============================================== Strong Body ============================================== //

DWORD WINAPI Set_StrongBody(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::WSCheat)
            {
                if (Keyboard::C_StrongBody)
                {
                    engine::InstanceBase__SetAffect(1, 16, -1); // Güçlü Beden
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
        Sleep(800);
    }
    return true;
}


// ============================================== SetCameraMax Body ============================================== //

DWORD WINAPI Set_SetMaxDistance(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::CZoom)
            {
                if (Keyboard::C_Zoom)
                {
                    engine::ApplicationSetCameraMaxDistance(Keyboard::C_ZoomValue);
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
        Sleep(800);
    }
    return true;
}


// ============================================== CH Changer ============================================== //

struct ServerInfo {
    std::string ip;
    std::vector<int> ports;
};

std::map<std::string, ServerInfo> servers = {
    { /*ZenitAbys*/XorStrTR<0x47, 10, 0x2CAE8000>("\x1D\x2D\x27\x23\x3F\x0D\x2F\x37\x3C" + 0x2CAE8000).s, {
        /*37.187.253.95*/XorStrTR<0x47, 14, 0x2CAE8000>("\x74\x7F\x67\x7B\x73\x7B\x63\x7C\x7A\x63\x7F\x6B\x66" + 0x2CAE8000).s,
        { 13001, 13101, 13201, 13301 } // CH1 → CH4
    } }
};




std::string selectedServer = /*ZenitAbys*/XorStrTR<0x47, 10, 0x2CAE8000>("\x1D\x2D\x27\x23\x3F\x0D\x2F\x37\x3C" + 0x2CAE8000).s;
DWORD WINAPI Set_CHChanger(HMODULE hModule)
{
    while (true)
    {
        if (Keyboard::AutoCH)
        {
            auto it = servers.find(selectedServer);
            if (it != servers.end())
            {
                ServerInfo server = it->second;

                if (Keyboard::Auto_CH1)
                {
                    engine::Disconnect();
                    engine::ConnectTCP(server.ip.c_str(), server.ports[0]); // CH1 : 16000
                    std::this_thread::sleep_for(std::chrono::milliseconds(10000)); // 20 mili saniye
                    *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + Client->Key->KeyBoard_Enter)) = 128;
                    Keyboard::Auto_CH1 = false;
                }

                if (Keyboard::Auto_CH2)
                {
                    engine::Disconnect();
                    engine::ConnectTCP(server.ip.c_str(), server.ports[1]); // CH2 : 18000
                    std::this_thread::sleep_for(std::chrono::milliseconds(10000)); // 20 mili saniye
                    *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + Client->Key->KeyBoard_Enter)) = 128;
                    Keyboard::Auto_CH2 = false;
                }
                if (Keyboard::Auto_CH3)
                {
                    engine::Disconnect();
                    engine::ConnectTCP(server.ip.c_str(), server.ports[2]); // CH3 : 18000
                    std::this_thread::sleep_for(std::chrono::milliseconds(10000)); // 20 mili saniye
                    *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + Client->Key->KeyBoard_Enter)) = 128;
                    Keyboard::Auto_CH3 = false;
                }
                if (Keyboard::Auto_CH4)
                {
                    engine::Disconnect();
                    engine::ConnectTCP(server.ip.c_str(), server.ports[3]); // CH4 : 18000
                    std::this_thread::sleep_for(std::chrono::milliseconds(10000)); // 20 mili saniye
                    *(DWORD*)(((DWORD)GetModuleHandleA("DINPUT8.dll") + Client->Key->KeyBoard_Enter)) = 128;
                    Keyboard::Auto_CH4 = false;
                }
            }
        }

        Sleep(50);
    }
    return 0;
}

int currentCH = 0;
DWORD WINAPI Set_CHChangerTime(HMODULE hModule)
{
    srand((unsigned)time(0));
    DWORD lastSwitch = GetTickCount(); 

    while (true)
    {
        if (Keyboard::AutoCH) 
        {
            if (Keyboard::AutoCH_Order)
            {
                auto it = servers.find(selectedServer);
                if (it != servers.end())
                {
                    ServerInfo server = it->second;

                    DWORD now = GetTickCount();
                    if (now - lastSwitch >= 120000)
                    {
                        engine::Disconnect();
                        engine::ConnectTCP(server.ip.c_str(), server.ports[currentCH]);

                        // Sonraki CH'ye geç
                        currentCH++;
                        if (currentCH >= server.ports.size())
                            currentCH = 0;

                        lastSwitch = now;
                    }
                }
            }
            if (Keyboard::AutoCH_Random)
            {
                auto it = servers.find(selectedServer);
                if (it != servers.end())
                {
                    ServerInfo server = it->second;

                    DWORD now = GetTickCount();
                    if (now - lastSwitch >= 120000) 
                    {
                        int newCH = rand() % server.ports.size();
                        if (newCH == currentCH)
                            newCH = (newCH + 1) % server.ports.size();

                        currentCH = newCH;
                        engine::Disconnect();
                        engine::ConnectTCP(server.ip.c_str(), server.ports[currentCH]);

                        lastSwitch = now;
                    }
                }
           }
        }

        Sleep(50); 
    }

    return 0;
}

// ============================================== Combo Attack ============================================== //

DWORD WINAPI Set_ComboAttack(HMODULE hModule)
{
    while (true)
    {
        __try
        {
            if (Keyboard::AutoAttacks)
            {
                if (Keyboard::C_AutoSpaceV1)
                {
                    engine::ComboAttack();
                }
                if (Keyboard::C_AutoSpaceV3)
                {
                    engine::SetAttackKeyState(true);
                }
            }   
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
        Sleep(800);
    }
    return true;
}
