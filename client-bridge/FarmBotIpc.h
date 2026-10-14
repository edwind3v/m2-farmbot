#pragma once

// Local IPC bridge for an authorized Metin2 client.
// The bot listens on \\.\pipe\M2FarmBot. This process connects, publishes one
// state frame per tick, and drains attack / move / skill / pickup / heal / stop commands.
//
// Frame layout: uint32 little-endian byte length, then UTF-8 JSON.
// Call Connect once, Publish once per client tick, and Poll until it returns false.

#include <stdint.h>
#include <string>

enum FarmBotEntityKind {
    FarmBotKind_Unknown = 0,
    FarmBotKind_Monster = 1,
    FarmBotKind_Metin = 2,
    FarmBotKind_Boss = 3,
    FarmBotKind_Npc = 4,
    FarmBotKind_Player = 5,
    FarmBotKind_Drop = 6
};

struct FarmBotPlayer {
    char name[64];
    int hp;
    int maxHp;
    int mp;
    int maxMp;
    float x;
    float y;
    float z;
    int level;
    unsigned int targetVid;
    char mapName[32];
};

struct FarmBotEntity {
    unsigned int vid;
    char name[64];
    int kind;
    int hp;
    int maxHp;
    float x;
    float y;
    float z;
    float distance;
    int screenX;
    int screenY;
    int onScreen;
};

struct FarmBotCommand {
    enum Type { None = 0, Attack, Move, Skill, Pickup, Heal, Stop } type;
    unsigned int vid;
    float x;
    float y;
    int screenX;
    int screenY;
    int onScreen;
    int slot;
    char key[16];
    char targetName[64];
};

class FarmBotIpc {
public:
    FarmBotIpc();
    ~FarmBotIpc();

    bool Connect(const wchar_t* pipeName = L"\\\\.\\pipe\\M2FarmBot");
    void Close();
    bool IsConnected() const;

    bool Publish(const FarmBotPlayer& player, const FarmBotEntity* entities, int count);
    bool Poll(FarmBotCommand& command);

private:
    bool WriteFrame(const char* bytes, int size);
    bool ReadAvailable();
    bool PopFrame(char* dest, int destSize, int& size);
    void* _pipe;
    std::string _rx;
};
