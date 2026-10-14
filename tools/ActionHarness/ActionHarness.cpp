#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <queue>
#include <string>
#include <vector>

// 32-bit MSVC thiscall harness. Not the game. Validates SetTarget ABI + authenticated IPC.

class FakePythonPlayer
{
public:
    volatile unsigned int m_dwTargetVID;
    bool m_hasMain;
    bool m_canChangeTarget;

    FakePythonPlayer()
        : m_dwTargetVID(0), m_hasMain(true), m_canChangeTarget(true)
    {
        m_alive.push_back(Actor(101, 0));
        m_alive.push_back(Actor(8001, 2));
    }

    // MSVC x86: ecx=this, [esp+4]=vid, [esp+8]=force, ret 8
    void SetTarget(unsigned int vid, int forceChange)
    {
        (void)forceChange;
        if (!m_hasMain || !m_canChangeTarget)
        {
            m_lastReason = "precondition failed: no main or cannot change target";
            return;
        }
        if (vid == 0)
        {
            m_dwTargetVID = 0;
            m_lastReason = "cleared";
            return;
        }
        const Actor* actor = Find(vid);
        if (!actor)
        {
            m_dwTargetVID = 0;
            m_lastReason = "invalid VID";
            return;
        }
        if (actor->type == 6 || actor->type == 1)
        {
            m_dwTargetVID = 0;
            m_lastReason = "player/NPC rejected";
            return;
        }
        m_dwTargetVID = vid;
        m_lastReason = "selected";
        InterlockedIncrement(&g_thiscallHits);
    }

    bool Has(unsigned int vid) const { return Find(vid) != nullptr; }
    std::string LastReason() const { return m_lastReason; }

    static volatile long g_thiscallHits;

private:
    struct Actor
    {
        unsigned int vid;
        int type;
        Actor(unsigned int v, int t) : vid(v), type(t) {}
    };

    std::vector<Actor> m_alive;
    std::string m_lastReason;

    const Actor* Find(unsigned int vid) const
    {
        for (size_t i = 0; i < m_alive.size(); ++i)
            if (m_alive[i].vid == vid)
                return &m_alive[i];
        return nullptr;
    }
};

volatile long FakePythonPlayer::g_thiscallHits = 0;

struct Command
{
    std::string type;
    std::string token;
    unsigned int vid;
    int force;
};

static std::string g_token;
static std::string g_pipeName = "FarmBotAction";
static HANDLE g_pipe = INVALID_HANDLE_VALUE;
static CRITICAL_SECTION g_lock;
static std::queue<Command> g_queue;
static FakePythonPlayer g_player;
static HANDLE g_wake;

static std::string JsonString(const std::string& src, const char* key)
{
    const std::string needle = std::string("\"") + key + "\"";
    const size_t at = src.find(needle);
    if (at == std::string::npos)
        return "";
    size_t i = src.find(':', at + needle.size());
    if (i == std::string::npos)
        return "";
    ++i;
    while (i < src.size() && (src[i] == ' ' || src[i] == '\"'))
        ++i;
    size_t end = i;
    while (end < src.size() && src[end] != '\"' && src[end] != ',' && src[end] != '}' && src[end] != ' ')
        ++end;
    return src.substr(i, end - i);
}

static unsigned int JsonU32(const std::string& src, const char* key)
{
    return (unsigned int)strtoul(JsonString(src, key).c_str(), nullptr, 10);
}

static bool JsonTrue(const std::string& src, const char* key)
{
    const std::string value = JsonString(src, key);
    return value == "true" || value == "1";
}

static bool WriteFrame(const std::string& json)
{
    if (g_pipe == INVALID_HANDLE_VALUE)
        return false;
    const DWORD length = (DWORD)json.size();
    DWORD written = 0;
    if (!WriteFile(g_pipe, &length, 4, &written, nullptr) || written != 4)
        return false;
    return WriteFile(g_pipe, json.data(), length, &written, nullptr) && written == length;
}

static bool ReadFrame(std::string* json)
{
    DWORD length = 0, read = 0;
    if (!ReadFile(g_pipe, &length, 4, &read, nullptr) || read != 4 || length == 0 || length > 65536)
        return false;
    json->assign(length, 0);
    return ReadFile(g_pipe, &(*json)[0], length, &read, nullptr) && read == length;
}

static bool TokensEqual(const std::string& left, const std::string& right)
{
    if (left.size() != right.size())
        return false;
    unsigned char diff = 0;
    for (size_t i = 0; i < left.size(); ++i)
        diff |= (unsigned char)(left[i] ^ right[i]);
    return diff == 0;
}

static void GameLoop()
{
    for (;;)
    {
        WaitForSingleObject(g_wake, 50);
        EnterCriticalSection(&g_lock);
        while (!g_queue.empty())
        {
            Command cmd = g_queue.front();
            g_queue.pop();
            LeaveCriticalSection(&g_lock);
            if (cmd.type == "set_target")
                g_player.SetTarget(cmd.vid, cmd.force);
            EnterCriticalSection(&g_lock);
        }
        LeaveCriticalSection(&g_lock);
    }
}

static DWORD WINAPI GameThread(void*)
{
    GameLoop();
    return 0;
}

static void Serve()
{
    char path[256];
    _snprintf_s(path, _TRUNCATE, "\\\\.\\pipe\\%s", g_pipeName.c_str());
    g_pipe = CreateNamedPipeA(
        path,
        PIPE_ACCESS_DUPLEX,
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
        1,
        4096,
        4096,
        0,
        nullptr);
    if (g_pipe == INVALID_HANDLE_VALUE)
    {
        printf("create pipe failed %lu\n", GetLastError());
        return;
    }
    printf("harness listening on %s thiscall=1\n", path);
    fflush(stdout);
    char readyName[256];
    _snprintf_s(readyName, _TRUNCATE, "Local\\FarmBotReady_%s", g_pipeName.c_str());
    HANDLE ready = CreateEventA(nullptr, TRUE, FALSE, readyName);
    if (ready)
        SetEvent(ready);
    if (!ConnectNamedPipe(g_pipe, nullptr) && GetLastError() != ERROR_PIPE_CONNECTED)
    {
        printf("connect failed %lu\n", GetLastError());
        return;
    }

    std::string json;
    if (!ReadFrame(&json))
        return;
    if (JsonString(json, "type") != "auth" || !TokensEqual(JsonString(json, "token"), g_token))
    {
        WriteFrame("{\"ok\":false,\"type\":\"auth\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"thiscall\"}");
        return;
    }
    WriteFrame("{\"ok\":true,\"type\":\"auth\",\"reason\":\"ok\",\"targetVid\":0,\"convention\":\"thiscall-x86\"}");

    while (ReadFrame(&json))
    {
        Command cmd;
        cmd.type = JsonString(json, "type");
        cmd.token = JsonString(json, "token");
        cmd.vid = JsonU32(json, "vid");
        cmd.force = JsonTrue(json, "force") ? 1 : 0;
        if (!TokensEqual(cmd.token, g_token))
        {
            WriteFrame("{\"ok\":false,\"type\":\"set_target\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"thiscall-x86\"}");
            continue;
        }
        if (cmd.type != "set_target")
        {
            WriteFrame("{\"ok\":false,\"type\":\"error\",\"reason\":\"only set_target is enabled\",\"targetVid\":0,\"convention\":\"thiscall-x86\"}");
            continue;
        }

        EnterCriticalSection(&g_lock);
        g_queue.push(cmd);
        LeaveCriticalSection(&g_lock);
        SetEvent(g_wake);

        for (int i = 0; i < 50 && g_player.m_dwTargetVID != cmd.vid && cmd.vid != 0; ++i)
            Sleep(1);
        if (cmd.vid == 0)
            Sleep(2);

        char reply[512];
        const bool ok = (cmd.vid == 0 && g_player.m_dwTargetVID == 0) || g_player.m_dwTargetVID == cmd.vid;
        _snprintf_s(
            reply,
            _TRUNCATE,
            "{\"ok\":%s,\"type\":\"set_target\",\"reason\":\"%s\",\"targetVid\":%u,\"convention\":\"thiscall-x86 hits=%ld\"}",
            ok ? "true" : "false",
            g_player.LastReason().c_str(),
            g_player.m_dwTargetVID,
            FakePythonPlayer::g_thiscallHits);
        if (!WriteFrame(reply))
            break;
    }
}

int main(int argc, char** argv)
{
    for (int i = 1; i < argc; ++i)
    {
        if (strcmp(argv[i], "--token") == 0 && i + 1 < argc)
            g_token = argv[++i];
        else if (strcmp(argv[i], "--pipe") == 0 && i + 1 < argc)
            g_pipeName = argv[++i];
    }
    if (g_token.empty())
    {
        printf("usage: ActionHarness --token HEX --pipe NAME\n");
        return 2;
    }

    InitializeCriticalSection(&g_lock);
    g_wake = CreateEventA(nullptr, FALSE, FALSE, nullptr);
    HANDLE thread = CreateThread(nullptr, 0, GameThread, nullptr, 0, nullptr);
    Serve();
    CloseHandle(thread);
    return 0;
}
