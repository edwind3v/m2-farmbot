// Optional smoke publisher. Build with the bridge and run it while FarmBot is in Live mode:
//   cl /nologo /EHsc /W4 /D FARM_BOT_IPC_SMOKE FarmBotIpc.cpp SmokeMain.cpp

#include "FarmBotIpc.h"

#ifdef FARM_BOT_IPC_SMOKE

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cstring>

int main() {
    FarmBotIpc ipc;
    if (!ipc.Connect()) {
        std::printf("could not connect to \\\\.\\pipe\\M2FarmBot\n");
        return 1;
    }

    FarmBotPlayer player;
    std::memset(&player, 0, sizeof(player));
    std::snprintf(player.name, sizeof(player.name), "BridgeSmoke");
    player.hp = 1000;
    player.maxHp = 1000;
    player.mp = 200;
    player.maxMp = 200;
    player.level = 1;
    std::snprintf(player.mapName, sizeof(player.mapName), "map_a2");

    FarmBotEntity entity;
    std::memset(&entity, 0, sizeof(entity));
    entity.vid = 42;
    std::snprintf(entity.name, sizeof(entity.name), "Wild Dog");
    entity.kind = FarmBotKind_Monster;
    entity.hp = 80;
    entity.maxHp = 80;
    entity.x = 120;
    entity.y = 10;
    entity.onScreen = 1;
    entity.screenX = 480;
    entity.screenY = 320;

    for (int i = 0; i < 50; ++i) {
        if (!ipc.Publish(player, &entity, 1)) {
            std::printf("publish failed\n");
            return 1;
        }
        FarmBotCommand command;
        while (ipc.Poll(command)) {
            std::printf("command type=%d vid=%u key=%s target=%s\n",
                static_cast<int>(command.type), command.vid, command.key, command.targetName);
        }
        Sleep(100);
    }
    return 0;
}

#endif
