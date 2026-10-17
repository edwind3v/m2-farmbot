#pragma once
#pragma once
#include <string>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <map>
#include <functional>
#include <Windows.h>
namespace fs = std::filesystem;

void CreateResourceFolders()
{
    if (!fs::exists("C:\\Resource"))
        fs::create_directory("C:\\Resource");

    if (!fs::exists("C:\\Resource\\ProtoList"))
        fs::create_directory("C:\\Resource\\ProtoList");

    if (!fs::exists("C:\\Resource\\Radar"))
        fs::create_directory("C:\\Resource\\Radar");

    if (!fs::exists("C:\\Resource\\Config"))
        fs::create_directory("C:\\Resource\\Config");

    if (!fs::exists("C:\\Resource\\Render"))
        fs::create_directory("C:\\Resource\\Render");

    if (!fs::exists("C:\\Resource\\Keyboard"))
        fs::create_directory("C:\\Resource\\Keyboard");
}




namespace Config
{
    static void CreateFolder()
    {
        if (!std::filesystem::exists("configs"))
            std::filesystem::create_directory("configs");
    }

    static std::vector<std::string> GetConfigs()
    {
        std::vector<std::string> list;
        CreateFolder();
        for (auto& entry : std::filesystem::directory_iterator("configs"))
        {
            if (entry.path().extension() == ".cfg")
                list.push_back(entry.path().stem().string());
        }
        return list;
    }

    // ===================== SAVE =====================
    static bool Save(const std::string& name)
    {
        CreateFolder();
        std::string path = "configs/" + name + ".cfg";
        std::ofstream file(path);
        if (!file.is_open()) return false;

        auto wb = [&](const char* key, bool val) {
            file << key << "=" << (val ? "1" : "0") << "\n";
        };
        auto wi = [&](const char* key, int val) {
            file << key << "=" << val << "\n";
        };
        auto wf = [&](const char* key, float val) {
            file << key << "=" << val << "\n";
        };

        // =========== Genel ===========
        wb("AutoAttacks", Keyboard::AutoAttacks);
        wb("C_AutoSpaceV1", Keyboard::C_AutoSpaceV1);
        wb("C_AutoSpaceV2", Keyboard::C_AutoSpaceV2);
        wb("C_AutoSpaceV3", Keyboard::C_AutoSpaceV3);
        wb("C_AutoRotation", Keyboard::C_AutoRotation);

        // =========== Mobber ===========
        wb("Mobber", Keyboard::Mobber);
        wb("C_Slot1", Keyboard::C_Slot1);
        wb("C_Slot2", Keyboard::C_Slot2);
        wb("C_Slot3", Keyboard::C_Slot3);
        wb("C_Slot4", Keyboard::C_Slot4);
        wb("C_F1", Keyboard::C_F1);
        wb("C_F2", Keyboard::C_F2);
        wb("C_F3", Keyboard::C_F3);
        wb("C_F4", Keyboard::C_F4);

        // =========== ProDamage ===========
        wb("ProDamage", Keyboard::ProDamage);
        wb("C_ProdDamage", Keyboard::C_ProdDamage);

        // =========== Wait ===========
        wb("WaitHack", Keyboard::WaitHack);
        wb("C_RangeWait", Keyboard::C_RangeWait);
        wb("C_RangeWaitLine", Keyboard::C_RangeWaitLine);
        wb("C_WaitDamage", Keyboard::C_WaitDamage);
        wb("C_DumyDamage", Keyboard::C_DumyDamage);
        wb("C_DieTimeEnable", Keyboard::C_DieTimeEnable);
        wi("C_DieTime", Keyboard::C_DieTime);
        wb("C_WaitMonster", Keyboard::C_WaitMonster);
        wb("C_WaitBoss", Keyboard::C_WaitBoss);
        wb("C_WaitStone", Keyboard::C_WaitStone);
        wb("C_WaitOther", Keyboard::C_WaitOther);
        wb("C_WaitNinja", Keyboard::C_WaitNinja);
        wi("C_WaitSpeed", Keyboard::C_WaitSpeed);
        wf("C_WaitDistance", Keyboard::C_WaitDistance);

        // =========== Target ===========
        wb("AttackOnTarget", Keyboard::AttackOnTarget);
        wb("C_RangeTarget", Keyboard::C_RangeTarget);

        // =========== AutoPot ===========
        wb("AutoPot", Keyboard::AutoPot);
        wb("C_PotSlot2", Keyboard::C_PotSlot2);
        wb("C_PotSlot3", Keyboard::C_PotSlot3);
        wb("C_PotSlot4", Keyboard::C_PotSlot4);
        wb("C_PotSlot5", Keyboard::C_PotSlot5);
        wb("C_PotSlot6", Keyboard::C_PotSlot6);
        wb("C_PotSlot7", Keyboard::C_PotSlot7);
        wb("C_PotSlot8", Keyboard::C_PotSlot8);
        wb("C_PotSlot9", Keyboard::C_PotSlot9);

        // =========== AutoPickUp ===========
        wb("AutoPickUp", Keyboard::AutoPickUp);
        wb("C_AutoLootV1", Keyboard::C_AutoLootV1);
        wb("C_AutoLootV2", Keyboard::C_AutoLootV2);
        wb("C_AutoLootv3", Keyboard::C_AutoLootv3);
        wb("C_RangeLoot", Keyboard::C_RangeLoot);
        wb("C_CollectLoot", Keyboard::C_CollectLoot);
        wb("C_Blacksmith", Keyboard::C_Blacksmith);
        wi("C_BlacksmithSpeed", Keyboard::C_BlacksmithSpeed);
        wb("C_Blacksmith_CheckAll", Keyboard::C_Blacksmith_CheckAll);


        wb("C_Blacksmith_Slot1", Keyboard::C_Blacksmith_Slot1);
        wb("C_Blacksmith_Slot2", Keyboard::C_Blacksmith_Slot2);
        wb("C_Blacksmith_Slot3", Keyboard::C_Blacksmith_Slot3);
        wb("C_Blacksmith_Slot4", Keyboard::C_Blacksmith_Slot4);
        wb("C_Blacksmith_Slot5", Keyboard::C_Blacksmith_Slot5);
        wb("C_Blacksmith_Slot6", Keyboard::C_Blacksmith_Slot6);
        wb("C_Blacksmith_Slot7", Keyboard::C_Blacksmith_Slot7);
        wb("C_Blacksmith_Slot8", Keyboard::C_Blacksmith_Slot8);
        wb("C_Blacksmith_Slot9", Keyboard::C_Blacksmith_Slot9);
        wb("C_Blacksmith_Slot10", Keyboard::C_Blacksmith_Slot10);
        wb("C_Blacksmith_Slot11", Keyboard::C_Blacksmith_Slot11);
        wb("C_Blacksmith_Slot12", Keyboard::C_Blacksmith_Slot12);
        wb("C_Blacksmith_Slot13", Keyboard::C_Blacksmith_Slot13);
        wb("C_Blacksmith_Slot14", Keyboard::C_Blacksmith_Slot14);
        wb("C_Blacksmith_Slot15", Keyboard::C_Blacksmith_Slot15);
        wb("C_Blacksmith_Slot16", Keyboard::C_Blacksmith_Slot16);
        wb("C_Blacksmith_Slot17", Keyboard::C_Blacksmith_Slot17);
        wb("C_Blacksmith_Slot18", Keyboard::C_Blacksmith_Slot18);
        wb("C_Blacksmith_Slot19", Keyboard::C_Blacksmith_Slot19);
        wb("C_Blacksmith_Slot20", Keyboard::C_Blacksmith_Slot20);

        // =========== KnockBack ===========
        wb("KnockBack", Keyboard::KnockBack);
        wb("C_KnockBack_Entity", Keyboard::C_KnockBack_Entity);
        wb("C_KnockBack_Player", Keyboard::C_KnockBack_Player);
        wb("C_KnockBack_Stone", Keyboard::C_KnockBack_Stone);

        // =========== Collision ===========
        wb("Collision", Keyboard::Collision);
        wb("C_Collision_NoClip", Keyboard::C_Collision_NoClip);
        wb("C_Collision_NPC", Keyboard::C_Collision_NPC);
        wb("C_Collision_Player", Keyboard::C_Collision_Player);
        wb("C_Collision_Stone", Keyboard::C_Collision_Stone);
        wb("C_Collision_Portal", Keyboard::C_Collision_Portal);

        // =========== Boost ===========
        wb("Boost", Keyboard::Boost);
        wb("C_BoostV1", Keyboard::C_BoostV1);
        wb("C_BoostV2", Keyboard::C_BoostV2);
        wi("BoostSpeed", Keyboard::BoostSpeed);
        wi("BoostStep", Keyboard::BoostStep);

        // =========== Name ===========
        wb("Name", Keyboard::Name);
        wb("C_NameChanger", Keyboard::C_NameChanger);
        wb("C_NameApathe", Keyboard::C_NameApathe);
        wb("C_Lalaker1", Keyboard::C_Lalaker1);
        wb("C_WhisperSpawm", Keyboard::C_WhisperSpawm);
        wb("C_WhisperEveryoneSpawm", Keyboard::C_WhisperEveryoneSpawm);
        wb("C_WhisperArea", Keyboard::C_WhisperArea);
        wf("C_WhisperDistance", Keyboard::C_WhisperDistance);
        wi("C_WhisperSpeed", Keyboard::C_WhisperSpeed);

        // =========== AutoCH ===========
        wb("AutoCH", Keyboard::AutoCH);
        wb("AutoCH_Order", Keyboard::AutoCH_Order);
        wb("AutoCH_Random", Keyboard::AutoCH_Random);
        wi("AutoCH_Time", Keyboard::AutoCH_Time);
        wb("Auto_CH1", Keyboard::Auto_CH1);
        wb("Auto_CH2", Keyboard::Auto_CH2);
        wb("Auto_CH3", Keyboard::Auto_CH3);
        wb("Auto_CH4", Keyboard::Auto_CH4);
        wb("Auto_CH5", Keyboard::Auto_CH5);
        wb("Auto_CH6", Keyboard::Auto_CH6);

        // =========== Bot ===========
        wb("Bot", Keyboard::Bot);
        wi("BOT_SPEED", Keyboard::BOT_SPEED);
        wi("BOT_DISTANCE", Keyboard::BOT_DISTANCE);
        wb("Bot_Npc", Keyboard::Bot_Npc);
        wb("Bot_Stone", Keyboard::Bot_Stone);
        wb("Bot_Mineral", Keyboard::Bot_Mineral);
        wb("Bot_Horse", Keyboard::Bot_Horse);
        wb("Bot_Player", Keyboard::Bot_Player);
        wb("C_NoKs", Keyboard::C_NoKs);
        wb("C_WalkToFarm", Keyboard::C_WalkToFarm);
        wb("C_UltimateFarm", Keyboard::C_UltimateFarm);
        wb("C_UltimateRoute", Keyboard::C_UltimateRoute);
        wi("C_UltimateSpeed", Keyboard::C_UltimateSpeed);
        wb("C_LegitRange", Keyboard::C_LegitRange);
        wb("C_BotTeleport", Keyboard::C_BotTeleport);
        wb("C_StateRange", Keyboard::C_StateRange);
        wb("C_KillAura", Keyboard::C_KillAura);
        wb("StoneBan", Keyboard::StoneBan);
        wb("StoneClear", Keyboard::StoneClear);
        wb("C_AllVadiBan", Keyboard::C_AllVadiBan);
        wb("C_AllSohanBan", Keyboard::C_AllSohanBan);
        wb("C_AllOrman", Keyboard::C_AllOrman);

        // =========== Skills ===========
        wb("Skills", Keyboard::Skills);
        wi("HorseTime", Keyboard::HorseTime);
        wi("DefaultTime", Keyboard::DefaultTime);
        wb("C_HorseSkill", Keyboard::C_HorseSkill);
        wb("C_HorseSkillSlot1", Keyboard::C_HorseSkillSlot1);
        wb("C_HorseSkillSlot2", Keyboard::C_HorseSkillSlot2);
        wb("C_HorseSkillSlot3", Keyboard::C_HorseSkillSlot3);
        wb("C_HorseSkillSlot4", Keyboard::C_HorseSkillSlot4);
        wb("C_DefaultSkill", Keyboard::C_DefaultSkill);
        wb("C_DefaultSkillSlot1", Keyboard::C_DefaultSkillSlot1);
        wb("C_DefaultSkillSlot2", Keyboard::C_DefaultSkillSlot2);
        wb("C_DefaultSkillSlot3", Keyboard::C_DefaultSkillSlot3);
        wb("C_DefaultSkillSlot4", Keyboard::C_DefaultSkillSlot4);

        // =========== AttackSpeed ===========
        wb("AttackSpeed", Keyboard::AttackSpeed);
        wb("C_SwordRange", Keyboard::C_SwordRange);
        wf("C_AttackSpeedValue", Keyboard::C_AttackSpeedValue);
        wb("C_AttackSpeed", Keyboard::C_AttackSpeed);

        // =========== MovementSpeed ===========
        wb("MovementSpeed", Keyboard::MovementSpeed);
        wb("C_LegitSpeed", Keyboard::C_LegitSpeed);
        wb("C_ModelVector", Keyboard::C_ModelVector);
        wf("C_MovementSpeedValue", Keyboard::C_MovementSpeedValue);
        wb("C_MovementSpeed", Keyboard::C_MovementSpeed);

        // =========== AntiFly ===========
        wb("AntiFly", Keyboard::AntiFly);
        wb("AntiFly_Player", Keyboard::AntiFly_Player);
        wb("AntiFly_Stone", Keyboard::AntiFly_Stone);
        wb("AntiFly_Npc", Keyboard::AntiFly_Npc);

        // =========== AutoRevive ===========
        wb("AutoRevive", Keyboard::AutoRevive);
        wi("AutoRevive_Time", Keyboard::AutoRevive_Time);
        wb("AutoReviveGF", Keyboard::AutoReviveGF);

        // =========== HorseChanger ===========
        wb("HorseChanger", Keyboard::HorseChanger);
        wb("HorseChanger_Ninja", Keyboard::HorseChanger_Ninja);
        wb("HorseChanger_Sura", Keyboard::HorseChanger_Sura);
        wb("HorseChanger_War", Keyboard::HorseChanger_War);
        wb("HorseChanger_Shaman", Keyboard::HorseChanger_Shaman);

        // =========== ChatSpawmmer ===========
        wb("ChatSpawmmer", Keyboard::ChatSpawmmer);
        wb("ChatSpawmmer_Rainbow", Keyboard::ChatSpawmmer_Rainbow);
        wb("ChatSpawmmer_Green", Keyboard::ChatSpawmmer_Green);
        wb("ChatSpawmmer_Yellow", Keyboard::ChatSpawmmer_Yellow);
        wb("ChatSpawmmer_Ping", Keyboard::ChatSpawmmer_Ping);
        wb("ChatSpawmmer_Blue", Keyboard::ChatSpawmmer_Blue);
        wb("ChatSpawmmer_DarkBlue", Keyboard::ChatSpawmmer_DarkBlue);

        // =========== XPDonate ===========
        wb("XPDonate", Keyboard::XPDonate);
        wb("C_XPDonate_Lonca", Keyboard::C_XPDonate_Lonca);
        wi("XPDonate_Limit", Keyboard::XPDonate_Limit);
        wi("XPDonate_Speed", Keyboard::XPDonate_Speed);
        wb("XPDonate_Level", Keyboard::XPDonate_Level);
        wi("XPDonate_lvl", Keyboard::XPDonate_lvl);

        // =========== Zoom ===========
        wb("CZoom", Keyboard::CZoom);
        wb("C_Zoom", Keyboard::C_Zoom);
        wi("C_ZoomValue", Keyboard::C_ZoomValue);

        // =========== EffectChanger ===========
        wb("EffectChanger", Keyboard::EffectChanger);
        wb("C_EffectGM", Keyboard::C_EffectGM);

        // =========== GMDedect ===========
        wb("GMDedect", Keyboard::GMDedect);
        wb("C_GM_Dedect", Keyboard::C_GM_Dedect);
        wb("C_GM_Dedect_Sound", Keyboard::C_GM_Dedect_Sound);
        wb("C_GM_Dedect_GameClose", Keyboard::C_GM_Dedect_GameClose);
        wi("C_GM_Dedect_Distance", Keyboard::C_GM_Dedect_Distance);

        // =========== StoneDedect ===========
        wb("StoneDedect", Keyboard::StoneDedect);
        wb("C_StoneDedect", Keyboard::C_StoneDedect);
        wi("C_StoneDedect_Distance", Keyboard::C_StoneDedect_Distance);
        wb("C_StoneBeep", Keyboard::C_StoneBeep);
        wb("C_StoneTeleport", Keyboard::C_StoneTeleport);
        wb("C_StoneDrawLine", Keyboard::C_StoneDrawLine);
        wb("C_Stone3DBox", Keyboard::C_Stone3DBox);

        // =========== PlayerDedect ===========
        wb("PlayerDedect", Keyboard::PlayerDedect);
        wb("C_Player_Dedect", Keyboard::C_Player_Dedect);
        wb("C_Player_Dedect_Sound", Keyboard::C_Player_Dedect_Sound);
        wb("C_Player_Dedect_GameClose", Keyboard::C_Player_Dedect_GameClose);
        wi("C_Player_Dedect_Distance", Keyboard::C_Player_Dedect_Distance);

        // =========== Mining ===========
        wb("MiningDedect", Keyboard::MiningDedect);
        wb("C_MiningTeleport", Keyboard::C_MiningTeleport);
        wb("C_MiningAuto", Keyboard::C_MiningAuto);
        wb("C_MiningBeep", Keyboard::C_MiningBeep);
        wb("C_AutoMining", Keyboard::C_AutoMining);
        wb("C_MiningShowBallon", Keyboard::C_MiningShowBallon);
        wi("C_Mining_Distance", Keyboard::C_Mining_Distance);

        // =========== Portal ===========
        wb("PortalDedect", Keyboard::PortalDedect);
        wb("C_Portal", Keyboard::C_Portal);
        wb("C_Portal_Sound", Keyboard::C_Portal_Sound);
        wb("C_Portal_GameClose", Keyboard::C_Portal_GameClose);
        wi("C_Portal_Distance", Keyboard::C_Portal_Distance);

        // =========== Teleport ===========
        wb("TeleportDedect", Keyboard::TeleportDedect);
        wb("C_Teleportss", Keyboard::C_Teleportss);
        wb("C_Teleportss_Sound", Keyboard::C_Teleportss_Sound);
        wb("C_Teleportss_GameClose", Keyboard::C_Teleportss_GameClose);
        wi("C_Teleportss_Distance", Keyboard::C_Teleportss_Distance);

        // =========== Area ===========
        wb("AreaDedect", Keyboard::AreaDedect);
        wb("C_AreaLine", Keyboard::C_AreaLine);
        wi("C_AreaDist", Keyboard::C_AreaDist);
        wb("Area_Bep", Keyboard::Area_Bep);

        // =========== SpawmDedect ===========
        wb("SpawmDedect", Keyboard::SpawmDedect);
        wb("C_PlayerSpawm", Keyboard::C_PlayerSpawm);
        wb("C_PlayerSpawm_Sound", Keyboard::C_PlayerSpawm_Sound);
        wb("C_PlayerSpawm_GameClose", Keyboard::C_PlayerSpawm_GameClose);
        wi("C_PlayerSpawm_Distance", Keyboard::C_PlayerSpawm_Distance);

        // =========== WSCheat ===========
        wb("WSCheat", Keyboard::WSCheat);
        wb("C_SkillRange", Keyboard::C_SkillRange);
        wb("C_Liberated", Keyboard::C_Liberated);
        wb("C_StrongBody", Keyboard::C_StrongBody);
        wb("C_AutoTarget", Keyboard::C_AutoTarget);
        wb("C_Typer", Keyboard::C_Typer);
        wb("C_Hitting", Keyboard::C_Hitting);
        wb("C_ClickToTeleport", Keyboard::C_ClickToTeleport);
        wb("C_EveryoneCheck", Keyboard::C_EveryoneCheck);
        wb("C_CheckModel", Keyboard::C_CheckModel);
        wb("C_SendModel", Keyboard::C_SendModel);
        wf("C_ClickToTeleport_Distance", Keyboard::C_ClickToTeleport_Distance);

        // =========== Minimap ===========
        wb("Minimap", Keyboard::Minimap);
        wb("C_ShowMap", Keyboard::C_ShowMap);
        wb("C_ShowNPC", Keyboard::C_ShowNPC);
        wb("C_ShowStone", Keyboard::C_ShowStone);
        wb("C_ShowEntity", Keyboard::C_ShowEntity);
        wb("C_ShowBoss", Keyboard::C_ShowBoss);

        // =========== Route ===========
        wb("Route", Keyboard::Route);
        wb("C_RouteRecording", Keyboard::C_RouteRecording);
        wb("C_PlayRoute", Keyboard::C_PlayRoute);

        file.close();
        return true;
    }

    // ===================== LOAD =====================
    static bool Load(const std::string& name)
    {
        std::string path = "configs/" + name + ".cfg";
        std::ifstream file(path);
        if (!file.is_open()) return false;

        // key=value mapine oku
        std::map<std::string, std::string> data;
        std::string line;
        while (std::getline(file, line))
        {
            if (line.empty() || line[0] == '#') continue;
            auto pos = line.find('=');
            if (pos == std::string::npos) continue;
            std::string key = line.substr(0, pos);
            std::string val = line.substr(pos + 1);
            data[key] = val;
        }
        file.close();

        // Yardımcı lambda'lar
        auto rb = [&](const char* key, bool& var) {
            auto it = data.find(key);
            if (it != data.end()) var = (it->second == "1");
        };
        auto ri = [&](const char* key, int& var) {
            auto it = data.find(key);
            if (it != data.end()) var = std::stoi(it->second);
        };
        auto rf = [&](const char* key, float& var) {
            auto it = data.find(key);
            if (it != data.end()) var = std::stof(it->second);
        };

        // =========== Genel ===========
        rb("AutoAttacks", Keyboard::AutoAttacks);
        rb("C_AutoSpaceV1", Keyboard::C_AutoSpaceV1);
        rb("C_AutoSpaceV2", Keyboard::C_AutoSpaceV2);
        rb("C_AutoSpaceV3", Keyboard::C_AutoSpaceV3);
        rb("C_AutoRotation", Keyboard::C_AutoRotation);

        // =========== Mobber ===========
        rb("Mobber", Keyboard::Mobber);
        rb("C_Slot1", Keyboard::C_Slot1);
        rb("C_Slot2", Keyboard::C_Slot2);
        rb("C_Slot3", Keyboard::C_Slot3);
        rb("C_Slot4", Keyboard::C_Slot4);
        rb("C_F1", Keyboard::C_F1);
        rb("C_F2", Keyboard::C_F2);
        rb("C_F3", Keyboard::C_F3);
        rb("C_F4", Keyboard::C_F4);

        // =========== ProDamage ===========
        rb("ProDamage", Keyboard::ProDamage);
        rb("C_ProdDamage", Keyboard::C_ProdDamage);

        // =========== Wait ===========
        rb("WaitHack", Keyboard::WaitHack);
        rb("C_RangeWait", Keyboard::C_RangeWait);
        rb("C_RangeWaitLine", Keyboard::C_RangeWaitLine);
        rb("C_WaitDamage", Keyboard::C_WaitDamage);
        rb("C_DumyDamage", Keyboard::C_DumyDamage);
        rb("C_DieTimeEnable", Keyboard::C_DieTimeEnable);
        ri("C_DieTime", Keyboard::C_DieTime);
        rb("C_WaitMonster", Keyboard::C_WaitMonster);
        rb("C_WaitBoss", Keyboard::C_WaitBoss);
        rb("C_WaitStone", Keyboard::C_WaitStone);
        rb("C_WaitOther", Keyboard::C_WaitOther);
        rb("C_WaitNinja", Keyboard::C_WaitNinja);
        ri("C_WaitSpeed", Keyboard::C_WaitSpeed);
        rf("C_WaitDistance", Keyboard::C_WaitDistance);

        // =========== Target ===========
        rb("AttackOnTarget", Keyboard::AttackOnTarget);
        rb("C_RangeTarget", Keyboard::C_RangeTarget);

        // =========== AutoPot ===========
        rb("AutoPot", Keyboard::AutoPot);
        rb("C_PotSlot2", Keyboard::C_PotSlot2);
        rb("C_PotSlot3", Keyboard::C_PotSlot3);
        rb("C_PotSlot4", Keyboard::C_PotSlot4);
        rb("C_PotSlot5", Keyboard::C_PotSlot5);
        rb("C_PotSlot6", Keyboard::C_PotSlot6);
        rb("C_PotSlot7", Keyboard::C_PotSlot7);
        rb("C_PotSlot8", Keyboard::C_PotSlot8);
        rb("C_PotSlot9", Keyboard::C_PotSlot9);

        // =========== AutoPickUp ===========
        rb("AutoPickUp", Keyboard::AutoPickUp);
        rb("C_AutoLootV1", Keyboard::C_AutoLootV1);
        rb("C_AutoLootV2", Keyboard::C_AutoLootV2);
        rb("C_AutoLootv3", Keyboard::C_AutoLootv3);
        rb("C_RangeLoot", Keyboard::C_RangeLoot);
        rb("C_CollectLoot", Keyboard::C_CollectLoot);
        rb("C_Blacksmith", Keyboard::C_Blacksmith);
        ri("C_BlacksmithSpeed", Keyboard::C_BlacksmithSpeed);
        rb("C_Blacksmith_CheckAll", Keyboard::C_Blacksmith_CheckAll);
        rb("C_Blacksmith_Slot1", Keyboard::C_Blacksmith_Slot1);
        rb("C_Blacksmith_Slot2", Keyboard::C_Blacksmith_Slot2);
        rb("C_Blacksmith_Slot3", Keyboard::C_Blacksmith_Slot3);
        rb("C_Blacksmith_Slot4", Keyboard::C_Blacksmith_Slot4);
        rb("C_Blacksmith_Slot5", Keyboard::C_Blacksmith_Slot5);
        rb("C_Blacksmith_Slot6", Keyboard::C_Blacksmith_Slot6);
        rb("C_Blacksmith_Slot7", Keyboard::C_Blacksmith_Slot7);
        rb("C_Blacksmith_Slot8", Keyboard::C_Blacksmith_Slot8);
        rb("C_Blacksmith_Slot9", Keyboard::C_Blacksmith_Slot9);
        rb("C_Blacksmith_Slot10", Keyboard::C_Blacksmith_Slot10);
        rb("C_Blacksmith_Slot11", Keyboard::C_Blacksmith_Slot11);
        rb("C_Blacksmith_Slot12", Keyboard::C_Blacksmith_Slot12);
        rb("C_Blacksmith_Slot13", Keyboard::C_Blacksmith_Slot13);
        rb("C_Blacksmith_Slot14", Keyboard::C_Blacksmith_Slot14);
        rb("C_Blacksmith_Slot15", Keyboard::C_Blacksmith_Slot15);
        rb("C_Blacksmith_Slot16", Keyboard::C_Blacksmith_Slot16);
        rb("C_Blacksmith_Slot17", Keyboard::C_Blacksmith_Slot17);
        rb("C_Blacksmith_Slot18", Keyboard::C_Blacksmith_Slot18);
        rb("C_Blacksmith_Slot19", Keyboard::C_Blacksmith_Slot19);
        rb("C_Blacksmith_Slot20", Keyboard::C_Blacksmith_Slot20);

        // =========== KnockBack ===========
        rb("KnockBack", Keyboard::KnockBack);
        rb("C_KnockBack_Entity", Keyboard::C_KnockBack_Entity);
        rb("C_KnockBack_Player", Keyboard::C_KnockBack_Player);
        rb("C_KnockBack_Stone", Keyboard::C_KnockBack_Stone);

        // =========== Collision ===========
        rb("Collision", Keyboard::Collision);
        rb("C_Collision_NoClip", Keyboard::C_Collision_NoClip);
        rb("C_Collision_NPC", Keyboard::C_Collision_NPC);
        rb("C_Collision_Player", Keyboard::C_Collision_Player);
        rb("C_Collision_Stone", Keyboard::C_Collision_Stone);
        rb("C_Collision_Portal", Keyboard::C_Collision_Portal);

        // =========== Boost ===========
        rb("Boost", Keyboard::Boost);
        rb("C_BoostV1", Keyboard::C_BoostV1);
        rb("C_BoostV2", Keyboard::C_BoostV2);
        ri("BoostSpeed", Keyboard::BoostSpeed);
        ri("BoostStep", Keyboard::BoostStep);

        // =========== Name ===========
        rb("Name", Keyboard::Name);
        rb("C_NameChanger", Keyboard::C_NameChanger);
        rb("C_NameApathe", Keyboard::C_NameApathe);
        rb("C_Lalaker1", Keyboard::C_Lalaker1);
        rb("C_WhisperSpawm", Keyboard::C_WhisperSpawm);
        rb("C_WhisperEveryoneSpawm", Keyboard::C_WhisperEveryoneSpawm);
        rb("C_WhisperArea", Keyboard::C_WhisperArea);
        rf("C_WhisperDistance", Keyboard::C_WhisperDistance);
        ri("C_WhisperSpeed", Keyboard::C_WhisperSpeed);

        // =========== AutoCH ===========
        rb("AutoCH", Keyboard::AutoCH);
        rb("AutoCH_Order", Keyboard::AutoCH_Order);
        rb("AutoCH_Random", Keyboard::AutoCH_Random);
        ri("AutoCH_Time", Keyboard::AutoCH_Time);
        rb("Auto_CH1", Keyboard::Auto_CH1);
        rb("Auto_CH2", Keyboard::Auto_CH2);
        rb("Auto_CH3", Keyboard::Auto_CH3);
        rb("Auto_CH4", Keyboard::Auto_CH4);
        rb("Auto_CH5", Keyboard::Auto_CH5);
        rb("Auto_CH6", Keyboard::Auto_CH6);

        // =========== Bot ===========
        rb("Bot", Keyboard::Bot);
        ri("BOT_SPEED", Keyboard::BOT_SPEED);
        ri("BOT_DISTANCE", Keyboard::BOT_DISTANCE);
        rb("Bot_Npc", Keyboard::Bot_Npc);
        rb("Bot_Stone", Keyboard::Bot_Stone);
        rb("Bot_Mineral", Keyboard::Bot_Mineral);
        rb("Bot_Horse", Keyboard::Bot_Horse);
        rb("Bot_Player", Keyboard::Bot_Player);
        rb("C_NoKs", Keyboard::C_NoKs);
        rb("C_WalkToFarm", Keyboard::C_WalkToFarm);
        rb("C_UltimateFarm", Keyboard::C_UltimateFarm);
        rb("C_UltimateRoute", Keyboard::C_UltimateRoute);
        ri("C_UltimateSpeed", Keyboard::C_UltimateSpeed);
        rb("C_LegitRange", Keyboard::C_LegitRange);
        rb("C_BotTeleport", Keyboard::C_BotTeleport);
        rb("C_StateRange", Keyboard::C_StateRange);
        rb("C_KillAura", Keyboard::C_KillAura);
        rb("StoneBan", Keyboard::StoneBan);
        rb("StoneClear", Keyboard::StoneClear);
        rb("C_AllVadiBan", Keyboard::C_AllVadiBan);
        rb("C_AllSohanBan", Keyboard::C_AllSohanBan);
        rb("C_AllOrman", Keyboard::C_AllOrman);

        // =========== Skills ===========
        rb("Skills", Keyboard::Skills);
        ri("HorseTime", Keyboard::HorseTime);
        ri("DefaultTime", Keyboard::DefaultTime);
        rb("C_HorseSkill", Keyboard::C_HorseSkill);
        rb("C_HorseSkillSlot1", Keyboard::C_HorseSkillSlot1);
        rb("C_HorseSkillSlot2", Keyboard::C_HorseSkillSlot2);
        rb("C_HorseSkillSlot3", Keyboard::C_HorseSkillSlot3);
        rb("C_HorseSkillSlot4", Keyboard::C_HorseSkillSlot4);
        rb("C_DefaultSkill", Keyboard::C_DefaultSkill);
        rb("C_DefaultSkillSlot1", Keyboard::C_DefaultSkillSlot1);
        rb("C_DefaultSkillSlot2", Keyboard::C_DefaultSkillSlot2);
        rb("C_DefaultSkillSlot3", Keyboard::C_DefaultSkillSlot3);
        rb("C_DefaultSkillSlot4", Keyboard::C_DefaultSkillSlot4);

        // =========== AttackSpeed ===========
        rb("AttackSpeed", Keyboard::AttackSpeed);
        rb("C_SwordRange", Keyboard::C_SwordRange);
        rf("C_AttackSpeedValue", Keyboard::C_AttackSpeedValue);
        rb("C_AttackSpeed", Keyboard::C_AttackSpeed);

        // =========== MovementSpeed ===========
        rb("MovementSpeed", Keyboard::MovementSpeed);
        rb("C_LegitSpeed", Keyboard::C_LegitSpeed);
        rb("C_ModelVector", Keyboard::C_ModelVector);
        rf("C_MovementSpeedValue", Keyboard::C_MovementSpeedValue);
        rb("C_MovementSpeed", Keyboard::C_MovementSpeed);

        // =========== AntiFly ===========
        rb("AntiFly", Keyboard::AntiFly);
        rb("AntiFly_Player", Keyboard::AntiFly_Player);
        rb("AntiFly_Stone", Keyboard::AntiFly_Stone);
        rb("AntiFly_Npc", Keyboard::AntiFly_Npc);

        // =========== AutoRevive ===========
        rb("AutoRevive", Keyboard::AutoRevive);
        ri("AutoRevive_Time", Keyboard::AutoRevive_Time);
        rb("AutoReviveGF", Keyboard::AutoReviveGF);

        // =========== HorseChanger ===========
        rb("HorseChanger", Keyboard::HorseChanger);
        rb("HorseChanger_Ninja", Keyboard::HorseChanger_Ninja);
        rb("HorseChanger_Sura", Keyboard::HorseChanger_Sura);
        rb("HorseChanger_War", Keyboard::HorseChanger_War);
        rb("HorseChanger_Shaman", Keyboard::HorseChanger_Shaman);

        // =========== ChatSpawmmer ===========
        rb("ChatSpawmmer", Keyboard::ChatSpawmmer);
        rb("ChatSpawmmer_Rainbow", Keyboard::ChatSpawmmer_Rainbow);
        rb("ChatSpawmmer_Green", Keyboard::ChatSpawmmer_Green);
        rb("ChatSpawmmer_Yellow", Keyboard::ChatSpawmmer_Yellow);
        rb("ChatSpawmmer_Ping", Keyboard::ChatSpawmmer_Ping);
        rb("ChatSpawmmer_Blue", Keyboard::ChatSpawmmer_Blue);
        rb("ChatSpawmmer_DarkBlue", Keyboard::ChatSpawmmer_DarkBlue);

        // =========== XPDonate ===========
        rb("XPDonate", Keyboard::XPDonate);
        rb("C_XPDonate_Lonca", Keyboard::C_XPDonate_Lonca);
        ri("XPDonate_Limit", Keyboard::XPDonate_Limit);
        ri("XPDonate_Speed", Keyboard::XPDonate_Speed);
        rb("XPDonate_Level", Keyboard::XPDonate_Level);
        ri("XPDonate_lvl", Keyboard::XPDonate_lvl);

        // =========== Zoom ===========
        rb("CZoom", Keyboard::CZoom);
        rb("C_Zoom", Keyboard::C_Zoom);
        ri("C_ZoomValue", Keyboard::C_ZoomValue);

        // =========== EffectChanger ===========
        rb("EffectChanger", Keyboard::EffectChanger);
        rb("C_EffectGM", Keyboard::C_EffectGM);

        // =========== GMDedect ===========
        rb("GMDedect", Keyboard::GMDedect);
        rb("C_GM_Dedect", Keyboard::C_GM_Dedect);
        rb("C_GM_Dedect_Sound", Keyboard::C_GM_Dedect_Sound);
        rb("C_GM_Dedect_GameClose", Keyboard::C_GM_Dedect_GameClose);
        ri("C_GM_Dedect_Distance", Keyboard::C_GM_Dedect_Distance);

        // =========== StoneDedect ===========
        rb("StoneDedect", Keyboard::StoneDedect);
        rb("C_StoneDedect", Keyboard::C_StoneDedect);
        ri("C_StoneDedect_Distance", Keyboard::C_StoneDedect_Distance);
        rb("C_StoneBeep", Keyboard::C_StoneBeep);
        rb("C_StoneTeleport", Keyboard::C_StoneTeleport);
        rb("C_StoneDrawLine", Keyboard::C_StoneDrawLine);
        rb("C_Stone3DBox", Keyboard::C_Stone3DBox);

        // =========== PlayerDedect ===========
        rb("PlayerDedect", Keyboard::PlayerDedect);
        rb("C_Player_Dedect", Keyboard::C_Player_Dedect);
        rb("C_Player_Dedect_Sound", Keyboard::C_Player_Dedect_Sound);
        rb("C_Player_Dedect_GameClose", Keyboard::C_Player_Dedect_GameClose);
        ri("C_Player_Dedect_Distance", Keyboard::C_Player_Dedect_Distance);

        // =========== Mining ===========
        rb("MiningDedect", Keyboard::MiningDedect);
        rb("C_MiningTeleport", Keyboard::C_MiningTeleport);
        rb("C_MiningAuto", Keyboard::C_MiningAuto);
        rb("C_MiningBeep", Keyboard::C_MiningBeep);
        rb("C_AutoMining", Keyboard::C_AutoMining);
        rb("C_MiningShowBallon", Keyboard::C_MiningShowBallon);
        ri("C_Mining_Distance", Keyboard::C_Mining_Distance);

        // =========== Portal ===========
        rb("PortalDedect", Keyboard::PortalDedect);
        rb("C_Portal", Keyboard::C_Portal);
        rb("C_Portal_Sound", Keyboard::C_Portal_Sound);
        rb("C_Portal_GameClose", Keyboard::C_Portal_GameClose);
        ri("C_Portal_Distance", Keyboard::C_Portal_Distance);

        // =========== Teleport ===========
        rb("TeleportDedect", Keyboard::TeleportDedect);
        rb("C_Teleportss", Keyboard::C_Teleportss);
        rb("C_Teleportss_Sound", Keyboard::C_Teleportss_Sound);
        rb("C_Teleportss_GameClose", Keyboard::C_Teleportss_GameClose);
        ri("C_Teleportss_Distance", Keyboard::C_Teleportss_Distance);

        // =========== Area ===========
        rb("AreaDedect", Keyboard::AreaDedect);
        rb("C_AreaLine", Keyboard::C_AreaLine);
        ri("C_AreaDist", Keyboard::C_AreaDist);
        rb("Area_Bep", Keyboard::Area_Bep);

        // =========== SpawmDedect ===========
        rb("SpawmDedect", Keyboard::SpawmDedect);
        rb("C_PlayerSpawm", Keyboard::C_PlayerSpawm);
        rb("C_PlayerSpawm_Sound", Keyboard::C_PlayerSpawm_Sound);
        rb("C_PlayerSpawm_GameClose", Keyboard::C_PlayerSpawm_GameClose);
        ri("C_PlayerSpawm_Distance", Keyboard::C_PlayerSpawm_Distance);

        // =========== WSCheat ===========
        rb("WSCheat", Keyboard::WSCheat);
        rb("C_SkillRange", Keyboard::C_SkillRange);
        rb("C_Liberated", Keyboard::C_Liberated);
        rb("C_StrongBody", Keyboard::C_StrongBody);
        rb("C_AutoTarget", Keyboard::C_AutoTarget);
        rb("C_Typer", Keyboard::C_Typer);
        rb("C_Hitting", Keyboard::C_Hitting);
        rb("C_ClickToTeleport", Keyboard::C_ClickToTeleport);
        rb("C_EveryoneCheck", Keyboard::C_EveryoneCheck);
        rb("C_CheckModel", Keyboard::C_CheckModel);
        rb("C_SendModel", Keyboard::C_SendModel);
        rf("C_ClickToTeleport_Distance", Keyboard::C_ClickToTeleport_Distance);

        // =========== Minimap ===========
        rb("Minimap", Keyboard::Minimap);
        rb("C_ShowMap", Keyboard::C_ShowMap);
        rb("C_ShowNPC", Keyboard::C_ShowNPC);
        rb("C_ShowStone", Keyboard::C_ShowStone);
        rb("C_ShowEntity", Keyboard::C_ShowEntity);
        rb("C_ShowBoss", Keyboard::C_ShowBoss);

        // =========== Route ===========
        rb("Route", Keyboard::Route);
        rb("C_RouteRecording", Keyboard::C_RouteRecording);
        rb("C_PlayRoute", Keyboard::C_PlayRoute);

        return true;
    }

    static bool Delete(const std::string& name)
    {
        std::string path = "configs/" + name + ".cfg";
        return std::filesystem::remove(path);
    }

} 

static int    selectedItem = -1;
static char   cfgNameBuf[64] = "";
static float  notifTimer = 0.0f;
static char   notifMsg[64] = "";

static std::vector<std::string> cfgFiles;
static bool cfgListDirty = true;

auto RefreshCfgList = [&]()
{
    cfgFiles = Config::GetConfigs();
    cfgListDirty = false;
    selectedItem = -1;
};