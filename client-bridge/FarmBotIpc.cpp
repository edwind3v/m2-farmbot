#include "FarmBotIpc.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <string>
#include <vector>

namespace {

void AppendEscaped(std::string& out, const char* text) {
    out.push_back('"');
    if (text != nullptr) {
        for (const unsigned char* cursor = reinterpret_cast<const unsigned char*>(text); *cursor != 0; ++cursor) {
            unsigned char c = *cursor;
            switch (c) {
            case '"': out += "\\\""; break;
            case '\\': out += "\\\\"; break;
            case '\n': out += "\\n"; break;
            case '\r': out += "\\r"; break;
            default:
                if (c < 0x20) {
                    char hex[8];
                    std::snprintf(hex, sizeof(hex), "\\u%04x", c);
                    out += hex;
                } else {
                    out.push_back(static_cast<char>(c));
                }
                break;
            }
        }
    }
    out.push_back('"');
}

const char* KindName(int kind) {
    switch (kind) {
    case FarmBotKind_Monster: return "monster";
    case FarmBotKind_Metin: return "metin";
    case FarmBotKind_Boss: return "boss";
    case FarmBotKind_Npc: return "npc";
    case FarmBotKind_Player: return "player";
    case FarmBotKind_Drop: return "drop";
    default: return "unknown";
    }
}

std::string JsonString(const std::string& json, const char* key) {
    std::string pattern = std::string("\"") + key + "\"";
    size_t pos = json.find(pattern);
    if (pos == std::string::npos)
        return "";
    pos = json.find(':', pos + pattern.size());
    if (pos == std::string::npos)
        return "";
    pos = json.find('"', pos + 1);
    if (pos == std::string::npos)
        return "";
    std::string value;
    for (size_t i = pos + 1; i < json.size(); ++i) {
        if (json[i] == '\\' && i + 1 < json.size()) {
            value.push_back(json[++i]);
            continue;
        }
        if (json[i] == '"')
            break;
        value.push_back(json[i]);
    }
    return value;
}

double JsonNumber(const std::string& json, const char* key, double fallback) {
    std::string pattern = std::string("\"") + key + "\"";
    size_t pos = json.find(pattern);
    if (pos == std::string::npos)
        return fallback;
    pos = json.find(':', pos + pattern.size());
    if (pos == std::string::npos)
        return fallback;
    ++pos;
    while (pos < json.size() && (json[pos] == ' ' || json[pos] == '\n' || json[pos] == '\r' || json[pos] == '\t'))
        ++pos;
    char* end = nullptr;
    double value = std::strtod(json.c_str() + pos, &end);
    if (end == json.c_str() + pos)
        return fallback;
    return value;
}

bool ParseCommand(const std::string& json, FarmBotCommand& command) {
    std::memset(&command, 0, sizeof(command));
    std::string name = JsonString(json, "name");
    if (name == "attack") command.type = FarmBotCommand::Attack;
    else if (name == "move") command.type = FarmBotCommand::Move;
    else if (name == "skill") command.type = FarmBotCommand::Skill;
    else if (name == "pickup") command.type = FarmBotCommand::Pickup;
    else if (name == "heal") command.type = FarmBotCommand::Heal;
    else if (name == "stop") command.type = FarmBotCommand::Stop;
    else return false;

    command.vid = static_cast<unsigned int>(JsonNumber(json, "vid", 0));
    command.x = static_cast<float>(JsonNumber(json, "x", 0));
    command.y = static_cast<float>(JsonNumber(json, "y", 0));
    command.screenX = static_cast<int>(JsonNumber(json, "screenX", 0));
    command.screenY = static_cast<int>(JsonNumber(json, "screenY", 0));
    command.onScreen = JsonNumber(json, "onScreen", 0) != 0 ? 1 : 0;
    command.slot = static_cast<int>(JsonNumber(json, "slot", 0));
    std::snprintf(command.key, sizeof(command.key), "%s", JsonString(json, "key").c_str());
    std::snprintf(command.targetName, sizeof(command.targetName), "%s", JsonString(json, "targetName").c_str());
    return true;
}

}  // namespace

FarmBotIpc::FarmBotIpc() : _pipe(nullptr) {}

FarmBotIpc::~FarmBotIpc() { Close(); }

bool FarmBotIpc::IsConnected() const {
    return _pipe != nullptr && _pipe != INVALID_HANDLE_VALUE;
}

bool FarmBotIpc::Connect(const wchar_t* pipeName) {
    Close();
    HANDLE pipe = CreateFileW(pipeName, GENERIC_READ | GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, 0, nullptr);
    if (pipe == INVALID_HANDLE_VALUE)
        return false;
    DWORD mode = PIPE_READMODE_BYTE;
    SetNamedPipeHandleState(pipe, &mode, nullptr, nullptr);
    _pipe = pipe;
    const char hello[] = "{\"type\":\"hello\",\"client\":\"metin2\",\"supportsCommands\":true,\"protocol\":1}";
    if (!WriteFrame(hello, static_cast<int>(sizeof(hello) - 1))) {
        Close();
        return false;
    }
    return true;
}

void FarmBotIpc::Close() {
    if (_pipe != nullptr && _pipe != INVALID_HANDLE_VALUE)
        CloseHandle(static_cast<HANDLE>(_pipe));
    _pipe = nullptr;
    _rx.clear();
}

bool FarmBotIpc::Publish(const FarmBotPlayer& player, const FarmBotEntity* entities, int count) {
    if (!IsConnected())
        return false;
    if (count < 0)
        count = 0;
    if (count > 1024)
        count = 1024;

    std::string json;
    json.reserve(256 + static_cast<size_t>(count) * 160);
    json += "{\"type\":\"state\",\"player\":{";
    json += "\"name\":";
    AppendEscaped(json, player.name);
    char numbers[160];
    std::snprintf(numbers, sizeof(numbers),
        ",\"hp\":%d,\"maxHp\":%d,\"mp\":%d,\"maxMp\":%d,\"x\":%.3f,\"y\":%.3f,\"z\":%.3f,\"level\":%d,\"targetVid\":%u,\"mapName\":",
        player.hp, player.maxHp, player.mp, player.maxMp, player.x, player.y, player.z, player.level, player.targetVid);
    json += numbers;
    AppendEscaped(json, player.mapName);
    json += "},\"entities\":[";
    for (int i = 0; i < count; ++i) {
        const FarmBotEntity& entity = entities[i];
        if (i > 0)
            json += ',';
        json += "{\"vid\":";
        std::snprintf(numbers, sizeof(numbers), "%u,\"name\":", entity.vid);
        json += numbers;
        AppendEscaped(json, entity.name);
        std::snprintf(numbers, sizeof(numbers),
            ",\"kind\":\"%s\",\"hp\":%d,\"maxHp\":%d,\"x\":%.3f,\"y\":%.3f,\"z\":%.3f,\"distance\":%.3f,\"screenX\":%d,\"screenY\":%d,\"onScreen\":%s}",
            KindName(entity.kind), entity.hp, entity.maxHp, entity.x, entity.y, entity.z, entity.distance,
            entity.screenX, entity.screenY, entity.onScreen ? "true" : "false");
        json += numbers;
    }
    json += "]}";
    if (!WriteFrame(json.data(), static_cast<int>(json.size()))) {
        Close();
        return false;
    }
    return true;
}

bool FarmBotIpc::Poll(FarmBotCommand& command) {
    if (!IsConnected())
        return false;
    for (;;) {
        if (!ReadAvailable()) {
            Close();
            return false;
        }
        char frame[64 * 1024];
        int size = 0;
        if (!PopFrame(frame, static_cast<int>(sizeof(frame)), size))
            return false;
        std::string json(frame, frame + size);
        if (ParseCommand(json, command))
            return true;
    }
}

bool FarmBotIpc::WriteFrame(const char* bytes, int size) {
    if (!IsConnected() || size < 0)
        return false;
    uint32_t length = static_cast<uint32_t>(size);
    uint8_t header[4] = {
        static_cast<uint8_t>(length),
        static_cast<uint8_t>(length >> 8),
        static_cast<uint8_t>(length >> 16),
        static_cast<uint8_t>(length >> 24)
    };
    DWORD written = 0;
    HANDLE pipe = static_cast<HANDLE>(_pipe);
    if (!WriteFile(pipe, header, 4, &written, nullptr) || written != 4)
        return false;
    written = 0;
    if (size == 0)
        return true;
    if (!WriteFile(pipe, bytes, static_cast<DWORD>(size), &written, nullptr) || written != static_cast<DWORD>(size))
        return false;
    return true;
}

bool FarmBotIpc::ReadAvailable() {
    HANDLE pipe = static_cast<HANDLE>(_pipe);
    DWORD available = 0;
    if (!PeekNamedPipe(pipe, nullptr, 0, nullptr, &available, nullptr))
        return false;
    if (available == 0)
        return true;
    std::vector<char> temp(available);
    DWORD read = 0;
    if (!ReadFile(pipe, temp.data(), available, &read, nullptr))
        return false;
    _rx.append(temp.data(), temp.data() + read);
    return true;
}

bool FarmBotIpc::PopFrame(char* dest, int destSize, int& size) {
    size = 0;
    if (_rx.size() < 4)
        return false;
    uint32_t length = static_cast<uint8_t>(_rx[0])
        | (static_cast<uint32_t>(static_cast<uint8_t>(_rx[1])) << 8)
        | (static_cast<uint32_t>(static_cast<uint8_t>(_rx[2])) << 16)
        | (static_cast<uint32_t>(static_cast<uint8_t>(_rx[3])) << 24);
    if (length > 1024u * 1024u) {
        Close();
        return false;
    }
    if (_rx.size() < static_cast<size_t>(4 + length))
        return false;
    if (static_cast<int>(length) > destSize)
        return false;
    std::memcpy(dest, _rx.data() + 4, length);
    _rx.erase(0, 4 + length);
    size = static_cast<int>(length);
    return true;
}
