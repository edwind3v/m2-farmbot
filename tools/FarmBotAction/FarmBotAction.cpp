#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <d3d9.h>
#include <psapi.h>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdarg>
#include <cstring>
#include <queue>
#include <string>
#include <vector>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "psapi.lib")

// In-process x86 adapter. Drains on CPythonApplication::Loop (PeekMessage/GetMessage)
// and calls existing CPythonPlayer methods on that same thread.

static unsigned kSingletonRva = 0x482398;
static unsigned kCharMgrRva = 0x48239C;
static unsigned kNewAttackRva = 0xD0C70;
static unsigned kComboRva = 0;
static unsigned kNetStreamRva = 0x482BA0;
static unsigned kAccountConnectorRva = 0x484D24;
static unsigned kSetLoginInfoRva = 0x11C390;
static unsigned kAccountSetLoginInfoRva = 0x117660;
static unsigned kAccountConnectRva = 0x117510;
static unsigned kConnectGameServerRva = 0x11BD40;
static unsigned kSelectCharacterRva = 0x13DC60;
static unsigned kSetLoginPhaseRva = 0x13D240;
static unsigned kSetGamePhaseRva = 0x1367F0;
static unsigned kStartGameRva = 0x13CD40;
static unsigned kNotifyGamePhaseRva = 0x11C0C0;
static unsigned kDirectEnterModeSetRva = 0x11C800;
static unsigned kOnLogoCloseRva = 0x13F1A0;
static unsigned kAppSingletonRva = 0x484C8C;
static unsigned kPhaseDisp = 0xF8;
static unsigned kLoginIdDisp = 0xB0;
static unsigned kLoginPasswordDisp = 0xC8;
static const unsigned kSelectedCharacterDisp = 0x797C;
static const unsigned kDirectEnterIsSetDisp = 0x79A4;
static const unsigned kDirectEnterSlotDisp = 0x79A8;
static const unsigned kStartGameFlagDisp = 0x8C;
static const unsigned kPythonHandlerDisp = 0x160;
static const unsigned kGameWindowDisp = 8;
static unsigned kSendAttackRva = 0x134BE0;
static unsigned kSendStateRva = 0x134DA0;
static unsigned kSendPickupRva = 0x13B690;
static unsigned kSendClickRva = 0xCE930;
static unsigned kItemSingletonRva = 0x482CAC;
static unsigned kGetCloseItemRva = 0xBB480;
static unsigned kGetCloseMoneyRva = 0xBB580;
static unsigned kTreeIncRva = 0xBA810;
static unsigned kMoveToDestRva = 0xA6BB0;
static unsigned kMoveToPixelRva = 0;
static unsigned kGetMainActorRva = 0;
static unsigned kPressActorRva = 0;
static unsigned kChaseRva = 0;
static unsigned kApproachInstRva = 0;
static unsigned kGetPixelRva = 0;
static unsigned kInRangeRva = 0;
static const unsigned kFuncWait = 0;
static const unsigned kFuncAttack = 2;
static unsigned kMainPtrDisp = 0x0C;
static unsigned kMapDisp = 0x20;
static unsigned kMapHeadOff = 0;
static unsigned kNodeLeft = 0;
static unsigned kNodeParent = 4;
static unsigned kNodeRight = 8;
static unsigned kNodeIsNil = 0x0D;
static unsigned kNodeKey = 0x10;
static unsigned kNodeValue = 0x14;
static unsigned kGraphicDisp = 0x1EC;
static unsigned kPixelXDisp = 0x46C;
static unsigned kPixelYDisp = 0x470;
static const float kAvoidStand = 250.f;
static const float kPickDistance = 300.f;
static unsigned kAttackKeyRva = 0xD2030;
static unsigned kReserveClickRva = 0xD1B90;
static unsigned kSetAutoRva = 0xD1DF0;
static unsigned kThisAdj = 4;
static unsigned kSetTargetSlot = 0;
static unsigned kSetTargetRva = 0;
static const unsigned kGetInstSlot = 8;
static unsigned kGetNameSlot = 0x14;
static const unsigned kOwnerNameDisp = 0x2FC;
static unsigned kPlayerNameDisp = 0x14;
static unsigned kTargetDisp = 0x10814;
static unsigned kReservedModeDisp = 0x54;
static unsigned kVidReservedDisp = 0x10770;
static unsigned kPlayerUpdateRva = 0xCF2D0;
static unsigned kPickCloseRva = 0xD1280;
static unsigned kPickCloseMoneyRva = 0xD12F0;
static unsigned kGetStatusRva = 0xCDBB0;
static unsigned kGetStatusSlot = 0;
static unsigned kPointsDisp = 0;
static unsigned kPointXor = 0;
static const unsigned kMiniMapRva = 0x482BA8;
static const unsigned kBackgroundRva = 0x48238C;
static const unsigned kLoadAtlasRva = 0xF9980;
static const unsigned kGetAtlasSizeRva = 0xF93B0;
static const unsigned kUpdateAtlasRva = 0xFC0D0;
static const unsigned kIsAtlasRva = 0xF9970;
static const unsigned kShowAtlasRva = 0xFB7B0;
static const unsigned kHideAtlasRva = 0xF9960;
static const unsigned kAtlasImageDisp = 0x26C;
static const unsigned kAtlasImageRefDisp = 0x1C;
static const unsigned kAtlasImage2Disp = 0x2CC;
static const unsigned kAtlasIsDisp = 0x170;
static const unsigned kAtlasWorldXDisp = 0x328;
static const unsigned kAtlasWorldYDisp = 0x32C;
static const unsigned kImageTexDisp = 0x3C;
static const unsigned kAtlasMagic = 0x534C5441;
static const unsigned kAtlasMax = 1024;
static const unsigned kAtlasPixels = kAtlasMax * kAtlasMax * 4;
static const unsigned kAtlasBytes = 32 + kAtlasPixels;
static const int kPointLevel = 1;
static const int kPointExp = 3;
static const int kPointNextExp = 4;
static const int kPointHp = 5;
static const int kPointMaxHp = 6;
static const int kPointSp = 7;
static const int kPointMaxSp = 8;

typedef void (__thiscall* FnSetTarget)(void* self, unsigned vid, int force);
typedef int (__thiscall* FnGetStatus)(void* self, int type);
typedef void* (__thiscall* FnGetInst)(void* self, unsigned vid);
typedef void (__thiscall* FnVoid)(void* self);
typedef void (__thiscall* FnNewAttack)(void* self);
typedef int (__thiscall* FnSendAttack)(void* self, unsigned mot, unsigned vid);
typedef int (__thiscall* FnSendState)(void* self, const float* pos, float rot, unsigned func, unsigned arg);
typedef int (__thiscall* FnSendPickup)(void* self, unsigned vid);
typedef void (__thiscall* FnSendClick)(void* self, unsigned vid);
typedef void* (__thiscall* FnTreeInc)(void* it);
typedef int (__thiscall* FnGetCloseItem)(void* self, float* pos, unsigned* vid, unsigned dist);
typedef int (__thiscall* FnMoveToDest)(void* inst, const void* dest);
typedef void (__thiscall* FnGetPixel)(void* inst, float* out);
typedef int (__thiscall* FnInRange)(void* me, const void* you);
typedef void* (__thiscall* FnGetMain)(void* self);
typedef void (__thiscall* FnAttackKey)(void* self, bool press);
typedef void (__thiscall* FnPressActor)(void* self, void* inst, unsigned vid, int isAuto);
typedef void (__thiscall* FnVid)(void* self, unsigned vid);
typedef int (__thiscall* FnLoadAtlas)(void* self);
typedef void (__thiscall* FnAtlasSize)(void* self, float* x, float* y);
typedef int (__thiscall* FnIsAtlas)(void* self);
typedef BOOL (WINAPI* FnPeek)(LPMSG, HWND, UINT, UINT, UINT);
typedef BOOL (WINAPI* FnGetMsg)(LPMSG, HWND, UINT, UINT);
typedef void (__thiscall* FnUpdate)(void* self);
typedef const char* (__thiscall* FnGetName)(void* self);
typedef void (__thiscall* FnSetLoginInfo)(void* self, const char* id, const char* pwd);
typedef int (__thiscall* FnAccountConnect)(void* self, const char* addr, int port, const char* accountAddr, int accountPort);
typedef void (__thiscall* FnConnectGameServer)(void* self, unsigned slot);
typedef int (__thiscall* FnSelectCharacter)(void* self, unsigned slot);
typedef void (__thiscall* FnDirectEnterSet)(void* self, unsigned slot);
typedef void (__thiscall* FnPhase)(void* self);
typedef unsigned char (__thiscall* FnNotifyGamePhase)(void* self);
typedef void* (__cdecl* FnPyIntFromLong)(long v);
typedef void* (__cdecl* FnPyStringFromString)(const char* v);
typedef int (__cdecl* FnPySetAttrString)(void* obj, const char* name, void* value);
typedef void* (__cdecl* FnPyGetAttrString)(void* obj, const char* name);
typedef void* (__cdecl* FnPyCallMethod)(void* obj, char* name, char* format, ...);
typedef void* (__cdecl* FnPyCallObject)(void* callable, void* args);
typedef void (__cdecl* FnPyErrClear)();
typedef int (__cdecl* FnPyGilEnsure)();
typedef void (__cdecl* FnPyGilRelease)(int state);
typedef char* (__cdecl* FnPyStringAsString)(void* obj);

struct ShotHit
{
    unsigned vid;
    float x;
    float y;
    int money;
    int take;
};

struct Command
{
    std::string type;
    unsigned vid;
    int force;
    int avoid;
    float x;
    float y;
    std::vector<ShotHit> hits;
    std::string account;
    std::string password;
    std::string host;
    std::string authHost;
    int port;
    int authPort;
    int slot;
};

static BYTE* g_base;
static std::string g_token;
static std::string g_pipeName = "FarmBotAction";
static HANDLE g_pipe = INVALID_HANDLE_VALUE;
static HANDLE g_done;
static CRITICAL_SECTION g_lock;
static std::queue<Command> g_queue;
static bool g_lastOk;
static std::string g_lastReason;
static unsigned g_lastVid;
static unsigned g_chaseVid;
static unsigned g_walkVid;
static float g_chaseX;
static float g_chaseY;
static DWORD g_lastWalkMs;
static volatile long g_allowMelee;
static BYTE* g_updateComboCall;
static volatile long g_busy;
static DWORD g_loopTid;
static int g_iatHooks;
static int g_getStatusSlot = -1;
static int g_hudLevel;
static int g_hudExp;
static int g_hudNextExp;
static int g_hudHp;
static int g_hudMaxHp;
static int g_hudMp;
static int g_hudMaxMp;
static bool g_hudOk;
static volatile long g_lootOn;
static volatile long g_lootFast;
static volatile long g_lootRanged;
static DWORD g_lootDelay = 200;
static float g_lootRadius = 300.f;
static LONGLONG g_lastLootMs;
static LONGLONG g_lastFastMs;
static LONGLONG g_lastRangedMs;
static HANDLE g_atlasMap;
static BYTE* g_atlasView;
static BYTE* g_atlasScratch;
static unsigned g_atlasWidth;
static unsigned g_atlasHeight;
static float g_atlasWorldW;
static float g_atlasWorldH;
static std::string g_sessionPhase;
static std::string g_sessionCharacter;
static std::string g_sessionAccount;
static std::string g_capturedAccount;
static std::string g_capturedPassword;
static int g_sessionCharacterSlot = -1;
static int g_startGame;
static int g_inWorld;
static int g_openedGameWindow;
static int g_skippedIntro;
static int g_loginPort;
static int g_sawLoadSteps;
static int g_loadFinished;
static int g_loadNearEnd;
static int g_maxLoadSteps;
static int g_loginSent;
static int g_selectSent;
static int g_layoutRecovered;
static unsigned g_atlasSeq;
static volatile long g_atlasCrash;

static FnPeek g_peekA;
static FnPeek g_peekW;
static FnGetMsg g_getA;
static FnGetMsg g_getW;
static FnUpdate g_playerUpdate;
static FnSetLoginInfo g_setLoginInfo;
static FnAttackKey g_origAttackKey;
static FnVid g_origReserveClick;
static FnSetTarget g_origSetTarget;
static FnVoid g_origCombo;
static FnVoid g_origChase;
static FnMoveToDest g_origMoveToDest;
static FnInRange g_origInRange;
static volatile long g_botCall;
static unsigned g_lastUpdateMode = 0xFFFFFFFFu;

static bool UserPtr(const void* p)
{
    const uintptr_t a = (uintptr_t)p;
    return a >= 0x10000 && a < 0x7FFF0000 && (a & 3) == 0;
}

static void NativeLog(const char* fmt, ...)
{
    char body[400];
    va_list ap;
    va_start(ap, fmt);
    _vsnprintf_s(body, _TRUNCATE, fmt, ap);
    va_end(ap);
    SYSTEMTIME st;
    GetLocalTime(&st);
    char line[480];
    _snprintf_s(line, _TRUNCATE, "%02u:%02u:%02u.%03u %s\r\n",
        st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, body);
    FILE* f = nullptr;
    if (fopen_s(&f, "C:\\Temp\\farmbot-native.log", "a") == 0 && f)
    {
        fputs(line, f);
        fclose(f);
    }
    char temp[MAX_PATH];
    if (GetTempPathA(MAX_PATH, temp))
    {
        char path[MAX_PATH];
        _snprintf_s(path, _TRUNCATE, "%sfarmbot-native.log", temp);
        if (fopen_s(&f, path, "a") == 0 && f)
        {
            fputs(line, f);
            fclose(f);
        }
    }
}

static void* Player()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kSingletonRva);
    return UserPtr(p) ? p : nullptr;
}

static void* CharMgr()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kCharMgrRva);
    return UserPtr(p) ? p : nullptr;
}

static void** IAbsVtbl(void* player)
{
    if (!player)
        return nullptr;
    void* iabs = (char*)player + kThisAdj;
    void** vtbl = *(void***)iabs;
    return UserPtr(vtbl) ? vtbl : nullptr;
}

static unsigned ReadTargetVid(void* player)
{
    if (!player)
        return 0;
    return *(unsigned*)((char*)player + kTargetDisp);
}

static int ReadPoint(void* player, int type)
{
    if (!player || !kPointsDisp || type < 0)
        return 0;
    __try
    {
        unsigned raw = *(unsigned*)((char*)player + kPointsDisp + type * 4);
        return (int)(raw ^ kPointXor);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int FindGetStatusSlot(void** vtbl)
{
    if (!g_base || !vtbl || !kGetStatusRva)
        return -1;
    void* want = g_base + kGetStatusRva;
    for (int i = 0; i < 32; ++i)
    {
        if (vtbl[i] == want)
            return i;
    }
    return -1;
}

static int CallGetStatus(void* player, int type)
{
    if (kGetStatusRva)
    {
        FnGetStatus fn = (FnGetStatus)(g_base + kGetStatusRva);
        if (!IsBadCodePtr((FARPROC)fn))
        {
            void* selves[2] = { (char*)player + kThisAdj, player };
            for (int i = 0; i < 2; ++i)
            {
                __try
                {
                    return fn(selves[i], type);
                }
                __except (EXCEPTION_EXECUTE_HANDLER)
                {
                }
            }
        }
    }
    void** vtbl = IAbsVtbl(player);
    if (vtbl)
    {
        int slot = -1;
        if (kGetStatusSlot)
            slot = (int)(kGetStatusSlot / 4);
        else
        {
            if (g_getStatusSlot < 0)
                g_getStatusSlot = FindGetStatusSlot(vtbl);
            slot = g_getStatusSlot;
        }
        if (slot >= 0 && UserPtr(vtbl[slot]))
        {
            void* iabs = (char*)player + kThisAdj;
            __try
            {
                return ((FnGetStatus)vtbl[slot])(iabs, type);
            }
            __except (EXCEPTION_EXECUTE_HANDLER)
            {
            }
        }
    }
    return ReadPoint(player, type);
}

static void RefreshHud(void* player)
{
    if (!player)
    {
        g_hudOk = false;
        return;
    }
    g_hudLevel = CallGetStatus(player, kPointLevel);
    g_hudExp = CallGetStatus(player, kPointExp);
    g_hudNextExp = CallGetStatus(player, kPointNextExp);
    g_hudHp = CallGetStatus(player, kPointHp);
    g_hudMaxHp = CallGetStatus(player, kPointMaxHp);
    g_hudMp = CallGetStatus(player, kPointSp);
    g_hudMaxMp = CallGetStatus(player, kPointMaxSp);
    g_hudOk = g_hudLevel >= 1 && g_hudLevel <= 250 && g_hudMaxHp > 0 && g_hudHp >= 0 && g_hudHp <= g_hudMaxHp;
}

static bool CallSetTarget(void* player, unsigned vid, int force)
{
    void* iabs = (char*)player + kThisAdj;
    __try
    {
        if (kSetTargetSlot)
        {
            void** vtbl = IAbsVtbl(player);
            if (vtbl && UserPtr(vtbl[kSetTargetSlot / 4]))
            {
                ((FnSetTarget)vtbl[kSetTargetSlot / 4])(iabs, vid, force);
                return true;
            }
        }
        if (kSetTargetRva)
        {
            FnSetTarget fn = (FnSetTarget)(g_base + kSetTargetRva);
            if (!IsBadCodePtr((FARPROC)fn))
            {
                fn(iabs, vid, force);
                return true;
            }
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
    return false;
}

static void* MapFindVid(unsigned vid)
{
    void* mgr = CharMgr();
    if (!mgr || !vid || !kMapDisp)
        return nullptr;
    __try
    {
        void* head = *(void**)((char*)mgr + kMapDisp + kMapHeadOff);
        if (!UserPtr(head))
            return nullptr;
        void* node = *(void**)((char*)head + kNodeParent);
        for (int i = 0; i < 512 && UserPtr(node) && node != head; ++i)
        {
            if (*((BYTE*)node + kNodeIsNil))
                break;
            unsigned key = *(unsigned*)((char*)node + kNodeKey);
            if (key == vid)
            {
                void* inst = *(void**)((char*)node + kNodeValue);
                return UserPtr(inst) ? inst : nullptr;
            }
            node = *(void**)((char*)node + (vid < key ? kNodeLeft : kNodeRight));
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    return nullptr;
}

static void* CallGetInstance(unsigned vid)
{
    void* fromMap = MapFindVid(vid);
    if (fromMap)
        return fromMap;
    void* mgr = CharMgr();
    if (!mgr)
        return nullptr;
    const unsigned adjs[2] = { 0, kThisAdj };
    for (int a = 0; a < 2; ++a)
    {
        void* iabs = (char*)mgr + adjs[a];
        __try
        {
            void** vtbl = *(void***)iabs;
            if (!UserPtr(vtbl) || !UserPtr(vtbl[kGetInstSlot / 4]))
                continue;
            void* inst = ((FnGetInst)vtbl[kGetInstSlot / 4])(iabs, vid);
            if (UserPtr(inst))
                return inst;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return nullptr;
}

static bool CallRva(unsigned rva, void* player, unsigned vid)
{
    if (!g_base || !player || !rva)
        return false;
    FnVid fn = (FnVid)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    void* selves[2] = { player, (char*)player + kThisAdj };
    for (int i = 0; i < 2; ++i)
    {
        __try
        {
            fn(selves[i], vid);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return false;
}

static bool CallPickClose(void* player)
{
    if (!kPickCloseRva)
        return false;
    FnVoid fn = (FnVoid)(g_base + kPickCloseRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(player);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallPickCloseMoney(void* player)
{
    if (!kPickCloseMoneyRva)
        return false;
    FnVoid fn = (FnVoid)(g_base + kPickCloseMoneyRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(player);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallSendPickup(unsigned vid);
static bool CallSendClick(void* player, unsigned vid);
static bool CallSendState(float x, float y, unsigned func);
static bool ReadMainPixel(float* x, float* y);
static bool ReadPixel(void* inst, float* x, float* y);
static void* MainInstance();
static void* CallGetMainActor();
static bool CallAttackKey(void* player, bool press);
static bool CallPressActor(void* player, unsigned vid);
static void ClearReserved(void* player);
static void CollectGroundItems(std::vector<ShotHit>* out, const char* me);
static void CollectClosestItem(float x, float y, float radius, std::vector<ShotHit>* out, const char* me);
static void CollectClosestMoney(float x, float y, float radius, std::vector<ShotHit>* out, const char* me);
static bool CallMoveToDest(unsigned vid, float x, float y);
static unsigned PickupNear(float x, float y, const std::vector<ShotHit>& items);
static LONGLONG NowMs();
static bool HasVid(const std::vector<ShotHit>& items, unsigned vid);

static bool ReadStdString(void* str, char* out, int cap);
static bool ReadPlayerName(void* player, char* out, int cap);
static int CanTakeGround(void* inst, const char* me);

static unsigned DrainGroundLoot(void* player, float px, float py, float radius)
{
    char me[64];
    me[0] = 0;
    ReadPlayerName(player, me, 64);
    std::vector<ShotHit> items;
    CollectGroundItems(&items, me);
    const float reach = radius > 1.f ? radius : 8000.f;
    CollectClosestItem(px, py, reach, &items, me);
    CollectClosestMoney(px, py, reach, &items, me);
    const float maxSq = reach * reach;
    unsigned sent = 0;
    for (size_t i = 0; i < items.size(); ++i)
    {
        const ShotHit& hit = items[i];
        const float dx = hit.x - px;
        const float dy = hit.y - py;
        const float distSq = dx * dx + dy * dy;
        // Fast stays inside the server pick radius. Ranged sends every VID the
        // client already has in CPythonItem — do not drop on our own distance.
        if (radius <= kPickDistance + 0.5f && distSq > maxSq)
            continue;
        if (!hit.take)
            continue;
        CallSendState(hit.x, hit.y, kFuncAttack);
        // SendClickItemPacket looks up proto/owner at instance+0x2FC and skips
        // yang and empty-owner drops. SendItemPickUpPacket is the real send.
        if (CallSendPickup(hit.vid))
            ++sent;
    }
    if (sent != 0)
    {
        float rx = px;
        float ry = py;
        ReadMainPixel(&rx, &ry);
        CallSendState(rx, ry, kFuncWait);
    }
    return sent;
}

static void MaybeAutoLoot(void* player)
{
    if (!player || (!g_lootFast && !g_lootRanged))
        return;
    __try
    {
        const LONGLONG now = NowMs();
        const DWORD delay = g_lootDelay < 1 ? 1 : g_lootDelay;
        float px = 0;
        float py = 0;
        ReadMainPixel(&px, &py);

        if (g_lootFast && (g_lastFastMs == 0 || now - g_lastFastMs >= delay))
        {
            g_lastFastMs = now;
            CallPickClose(player);
            CallPickCloseMoney(player);
            DrainGroundLoot(player, px, py, kPickDistance);
        }

        if (g_lootRanged && (g_lastRangedMs == 0 || now - g_lastRangedMs >= 1))
        {
            g_lastRangedMs = now;
            const float radius = g_lootRadius > 1.f ? g_lootRadius : 8000.f;
            DrainGroundLoot(player, px, py, radius);
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static void* MiniMap()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kMiniMapRva);
    return UserPtr(p) ? p : nullptr;
}

static bool InModule(const void* p, HMODULE mod)
{
    MODULEINFO info;
    if (!mod || !GetModuleInformation(GetCurrentProcess(), mod, &info, sizeof(info)))
        return false;
    const BYTE* b = (const BYTE*)p;
    const BYTE* base = (const BYTE*)info.lpBaseOfDll;
    return b >= base && b < base + info.SizeOfImage;
}

static IDirect3DTexture9* QueryTexture(void* p, HMODULE d3d)
{
    if (!d3d || !UserPtr(p) || !UserPtr(*(void**)p) || !InModule(*(void**)p, d3d))
        return nullptr;
    IDirect3DTexture9* tex = nullptr;
    if (FAILED(((IUnknown*)p)->QueryInterface(__uuidof(IDirect3DTexture9), (void**)&tex)) || !tex)
        return nullptr;
    D3DSURFACE_DESC desc;
    if (FAILED(tex->GetLevelDesc(0, &desc)) ||
        desc.Width < 64 || desc.Height < 64 || desc.Width > 4096 || desc.Height > 4096)
    {
        tex->Release();
        return nullptr;
    }
    return tex;
}

static void CopyRowBgra(BYTE* dst, const BYTE* src, unsigned w, unsigned format, unsigned srcPitch)
{
    if (format == 21 || format == 22)
    {
        memcpy(dst, src, w * 4);
        return;
    }
    for (unsigned x = 0; x < w; ++x)
    {
        BYTE* d = dst + x * 4;
        if (format == 23 && srcPitch >= w * 2)
        {
            const unsigned short pix = ((unsigned short*)src)[x];
            d[0] = (BYTE)((pix & 0x1F) << 3);
            d[1] = (BYTE)(((pix >> 5) & 0x3F) << 2);
            d[2] = (BYTE)(((pix >> 11) & 0x1F) << 3);
            d[3] = 255;
        }
        else if ((format == 24 || format == 25) && srcPitch >= w * 2)
        {
            const unsigned short pix = ((unsigned short*)src)[x];
            d[0] = (BYTE)((pix & 0x1F) << 3);
            d[1] = (BYTE)(((pix >> 5) & 0x1F) << 3);
            d[2] = (BYTE)(((pix >> 10) & 0x1F) << 3);
            d[3] = 255;
        }
        else if (srcPitch >= w * 4)
            memcpy(d, src + x * 4, 4);
        else
            memset(d, 0, 4);
    }
}

static bool CopyLocked8(const D3DLOCKED_RECT& lr, unsigned format, unsigned srcW, unsigned srcH, BYTE* dst, unsigned* outW, unsigned* outH)
{
    if (!lr.pBits || srcW < 64 || srcH < 64)
        return false;
    const unsigned w = srcW > kAtlasMax ? kAtlasMax : srcW;
    const unsigned h = srcH > kAtlasMax ? kAtlasMax : srcH;
    const unsigned bpp = (format == 23 || format == 24 || format == 25) ? 2u : 4u;
    const unsigned srcPitch = lr.Pitch ? (unsigned)lr.Pitch : srcW * bpp;
    BYTE* s = (BYTE*)lr.pBits;
    for (unsigned y = 0; y < h; ++y)
    {
        const unsigned sy = h == srcH ? y : y * srcH / h;
        CopyRowBgra(dst + y * w * 4, s + sy * srcPitch, w, format, srcPitch);
    }
    *outW = w;
    *outH = h;
    return true;
}

static int ReadableBytes(void* p, int want);

static bool IsD3D8Object(void* obj)
{
    HMODULE d3d8 = GetModuleHandleA("d3d8.dll");
    return d3d8 && UserPtr(obj) && UserPtr(*(void**)obj) && InModule(*(void**)obj, d3d8);
}

static bool CopyTexture8(void* tex, BYTE* dst, unsigned* outW, unsigned* outH)
{
    if (!IsD3D8Object(tex))
        return false;
    void** vt = *(void***)tex;
    if (IsBadCodePtr((FARPROC)vt[16]) || IsBadCodePtr((FARPROC)vt[18]) || IsBadCodePtr((FARPROC)vt[19]))
        return false;
    typedef HRESULT (__stdcall* FnDesc)(void*, unsigned, void*);
    typedef HRESULT (__stdcall* FnLock)(void*, unsigned, D3DLOCKED_RECT*, const void*, unsigned);
    typedef HRESULT (__stdcall* FnUnlock)(void*, unsigned);
    struct Desc8 { unsigned format, type, usage, pool, size, multi; unsigned width, height; };
    Desc8 desc = {};
    if (FAILED(((FnDesc)vt[16])(tex, 0, &desc)))
        return false;
    if (desc.width < 64 || desc.height < 64 || desc.width > 4096 || desc.height > 4096)
        return false;
    if (desc.format != 21 && desc.format != 22 && desc.format != 23)
        return false;
    D3DLOCKED_RECT lr = {};
    HRESULT hr = ((FnLock)vt[18])(tex, 0, &lr, nullptr, 0x10);
    if (FAILED(hr))
        hr = ((FnLock)vt[18])(tex, 0, &lr, nullptr, 0);
    if (FAILED(hr) || !lr.pBits)
        return false;
    const unsigned bpp = desc.format == 23 ? 2u : 4u;
    const unsigned need = desc.height * (lr.Pitch ? (unsigned)lr.Pitch : desc.width * bpp);
    const bool ok = ReadableBytes(lr.pBits, (int)need) >= (int)need &&
        CopyLocked8(lr, desc.format, desc.width, desc.height, dst, outW, outH);
    ((FnUnlock)vt[19])(tex, 0);
    return ok;
}

static void* QueryTexture8(void* p, HMODULE d3d8)
{
    if (!d3d8 || !UserPtr(p) || !UserPtr(*(void**)p) || !InModule(*(void**)p, d3d8))
        return nullptr;
    static const GUID kTex8 = {0xE4CDD575,0x2866,0x4d51,{0xB7,0x8F,0x5E,0xCB,0x73,0xF7,0xB9,0xB9}};
    IUnknown* unk = (IUnknown*)p;
    void* tex = nullptr;
    if (FAILED(unk->QueryInterface(kTex8, &tex)) || !tex)
        return nullptr;
    return tex;
}

static void ConsiderTexture(IDirect3DTexture9* tex, IDirect3DTexture9** best, unsigned* bestArea)
{
    if (!tex)
        return;
    D3DSURFACE_DESC desc;
    if (FAILED(tex->GetLevelDesc(0, &desc)))
    {
        tex->Release();
        return;
    }
    const unsigned area = desc.Width * desc.Height;
    if (area > *bestArea)
    {
        if (*best)
            (*best)->Release();
        *best = tex;
        *bestArea = area;
    }
    else
        tex->Release();
}

static int ReadableBytes(void* p, int want)
{
    MEMORY_BASIC_INFORMATION mbi;
    if (!p || !VirtualQuery(p, &mbi, sizeof(mbi)) || mbi.State != MEM_COMMIT)
        return 0;
    const SIZE_T left = (SIZE_T)mbi.BaseAddress + mbi.RegionSize - (SIZE_T)p;
    return left > (SIZE_T)want ? want : (int)left;
}

static void ScanImage(void* obj, int bytes, IDirect3DTexture9** best9, void** best8, unsigned* bestArea)
{
    HMODULE d3d9 = GetModuleHandleA("d3d9.dll");
    HMODULE d3d8 = GetModuleHandleA("d3d8.dll");
    HMODULE client = (HMODULE)g_base;
    const int n = ReadableBytes(obj, bytes) & ~3;
    BYTE* p = (BYTE*)obj;
    for (int i = 0; i + 4 <= n; i += 4)
    {
        void* child = *(void**)(p + i);
        ConsiderTexture(QueryTexture(child, d3d9), best9, bestArea);
        if (!*best9)
        {
            void* t8 = QueryTexture8(child, d3d8);
            if (t8)
            {
                if (*best8)
                    ((IUnknown*)*best8)->Release();
                *best8 = t8;
            }
        }
        if (!UserPtr(child) || !UserPtr(*(void**)child) || !client || !InModule(*(void**)child, client))
            continue;
        const int m = ReadableBytes(child, 0x100) & ~3;
        BYTE* o = (BYTE*)child;
        for (int j = 0; j + 4 <= m; j += 4)
        {
            void* inner = *(void**)(o + j);
            ConsiderTexture(QueryTexture(inner, d3d9), best9, bestArea);
            if (*best9)
                continue;
            void* t8 = QueryTexture8(inner, d3d8);
            if (t8)
            {
                if (*best8)
                    ((IUnknown*)*best8)->Release();
                *best8 = t8;
            }
        }
    }
}

static IDirect3DTexture9* FindAtlasTexture(void* mini, void** tex8)
{
    if (tex8)
        *tex8 = nullptr;
    if (!mini)
        return nullptr;
    IDirect3DTexture9* best9 = nullptr;
    void* best8 = nullptr;
    unsigned bestArea = 0;
    void* inst = (char*)mini + kAtlasImageDisp;
    void* image = *(void**)((char*)inst + kAtlasImageRefDisp);
    if (UserPtr(image))
        ScanImage(image, 0x180, &best9, &best8, &bestArea);
    if (!best9)
        ScanImage(inst, 0x40, &best9, &best8, &bestArea);
    if (!best9)
        ScanImage((char*)mini + kAtlasImage2Disp, 0x80, &best9, &best8, &bestArea);
    if (best9 && best8)
    {
        ((IUnknown*)best8)->Release();
        best8 = nullptr;
    }
    if (tex8)
        *tex8 = best8;
    return best9;
}

static bool CopyCpuImage(void* image, BYTE* dst, unsigned* outW, unsigned* outH)
{
    if (!image)
        return false;
    if (ReadableBytes(image, 0x70) >= 0x70)
    {
        const int w = *(int*)((char*)image + 0x68) - *(int*)((char*)image + 0x60);
        const int h = *(int*)((char*)image + 0x6C) - *(int*)((char*)image + 0x64);
        if (w >= 64 && h >= 64 && w <= (int)kAtlasMax && h <= (int)kAtlasMax)
        {
            const unsigned bgra = (unsigned)w * (unsigned)h * 4;
            const int n = ReadableBytes(image, 0x80) & ~3;
            BYTE* p = (BYTE*)image;
            for (int i = 0; i + 4 <= n; i += 4)
            {
                void* bits = *(void**)(p + i);
                if (!UserPtr(bits) || ReadableBytes(bits, (int)bgra) < (int)bgra)
                    continue;
                memcpy(dst, bits, bgra);
                *outW = (unsigned)w;
                *outH = (unsigned)h;
                return true;
            }
        }
    }
    const int n = ReadableBytes(image, 0x180) & ~3;
    BYTE* p = (BYTE*)image;
    for (int i = 0; i + 12 <= n; i += 4)
    {
        const unsigned w = *(unsigned*)(p + i);
        const unsigned h = *(unsigned*)(p + i + 4);
        void* bits = *(void**)(p + i + 8);
        if (w < 64 || h < 64 || w > kAtlasMax || h > kAtlasMax || !UserPtr(bits))
            continue;
        const unsigned bgra = w * h * 4;
        if (ReadableBytes(bits, (int)bgra) >= (int)bgra)
        {
            memcpy(dst, bits, bgra);
            *outW = w;
            *outH = h;
            return true;
        }
        const unsigned rgb565 = w * h * 2;
        if (ReadableBytes(bits, (int)rgb565) < (int)rgb565)
            continue;
        BYTE* s = (BYTE*)bits;
        for (unsigned y = 0; y < h; ++y)
        {
            for (unsigned x = 0; x < w; ++x)
            {
                const unsigned pix = ((unsigned short*)s)[y * w + x];
                BYTE* d = dst + (y * w + x) * 4;
                d[0] = (BYTE)((pix & 0x1F) << 3);
                d[1] = (BYTE)(((pix >> 5) & 0x3F) << 2);
                d[2] = (BYTE)(((pix >> 11) & 0x1F) << 3);
                d[3] = 255;
            }
        }
        *outW = w;
        *outH = h;
        return true;
    }
    return false;
}

static void ReconstructAtlasImage(void* mini)
{
    void* inst = (char*)mini + kAtlasImageDisp;
    if (!UserPtr(inst) || !UserPtr(*(void**)inst) || !g_base || !InModule(*(void**)inst, (HMODULE)g_base))
        return;
    void** vt = *(void***)inst;
    if (!UserPtr(vt) || !UserPtr(vt[2]) || !InModule(vt[2], (HMODULE)g_base))
        return;
    ((FnVoid)vt[2])(inst);
}

static void GuessWorldSize(void* mini, float* w, float* h)
{
    if (!mini || !w || !h)
        return;
    float bestA = 0, bestB = 0;
    BYTE* p = (BYTE*)mini;
    const int n = ReadableBytes(mini, 0x4000);
    for (int i = 0; i + 8 <= n; i += 4)
    {
        const float a = *(float*)(p + i);
        const float b = *(float*)(p + i + 4);
        if (a < 20000.f || a > 400000.f || b < 20000.f || b > 400000.f)
            continue;
        if (a >= bestA && b >= bestB * 0.25f)
        {
            bestA = a;
            bestB = b;
        }
    }
    if (bestA > 0)
    {
        *w = bestA;
        *h = bestB;
    }
}

static bool CopyTexture(IDirect3DTexture9* tex, BYTE* dst, unsigned* outW, unsigned* outH)
{
    D3DSURFACE_DESC desc;
    if (!tex || FAILED(tex->GetLevelDesc(0, &desc)))
        return false;
    IDirect3DDevice9* dev = nullptr;
    if (FAILED(tex->GetDevice(&dev)) || !dev)
        return false;
    IDirect3DSurface9* src = nullptr;
    IDirect3DSurface9* sys = nullptr;
    bool ok = false;
    if (SUCCEEDED(tex->GetSurfaceLevel(0, &src)) && src &&
        SUCCEEDED(dev->CreateOffscreenPlainSurface(desc.Width, desc.Height, D3DFMT_A8R8G8B8, D3DPOOL_SYSTEMMEM, &sys, nullptr)) && sys)
    {
        if (desc.Usage & D3DUSAGE_RENDERTARGET)
            ok = SUCCEEDED(dev->GetRenderTargetData(src, sys));
        else
        {
            IDirect3DSurface9* rt = nullptr;
            if (SUCCEEDED(dev->CreateRenderTarget(desc.Width, desc.Height, D3DFMT_A8R8G8B8, D3DMULTISAMPLE_NONE, 0, FALSE, &rt, nullptr)) && rt)
            {
                if (SUCCEEDED(dev->StretchRect(src, nullptr, rt, nullptr, D3DTEXF_LINEAR)))
                    ok = SUCCEEDED(dev->GetRenderTargetData(rt, sys));
                rt->Release();
            }
        }
        if (!ok)
        {
            D3DLOCKED_RECT lr = {};
            if (SUCCEEDED(tex->LockRect(0, &lr, nullptr, D3DLOCK_READONLY)))
            {
                sys->Release();
                sys = nullptr;
                const unsigned w = desc.Width > kAtlasMax ? kAtlasMax : desc.Width;
                const unsigned h = desc.Height > kAtlasMax ? kAtlasMax : desc.Height;
                BYTE* s = (BYTE*)lr.pBits;
                for (unsigned y = 0; y < h; ++y)
                {
                    const unsigned sy = h == desc.Height ? y : y * desc.Height / h;
                    memcpy(dst + y * w * 4, s + sy * lr.Pitch, w * 4);
                }
                tex->UnlockRect(0);
                *outW = w;
                *outH = h;
                src->Release();
                dev->Release();
                return true;
            }
        }
        if (ok)
        {
            D3DLOCKED_RECT lr = {};
            if (SUCCEEDED(sys->LockRect(&lr, nullptr, D3DLOCK_READONLY)))
            {
                const unsigned w = desc.Width > kAtlasMax ? kAtlasMax : desc.Width;
                const unsigned h = desc.Height > kAtlasMax ? kAtlasMax : desc.Height;
                BYTE* s = (BYTE*)lr.pBits;
                for (unsigned y = 0; y < h; ++y)
                {
                    const unsigned sy = h == desc.Height ? y : y * desc.Height / h;
                    BYTE* row = s + sy * lr.Pitch;
                    BYTE* drow = dst + y * w * 4;
                    for (unsigned x = 0; x < w; ++x)
                    {
                        const unsigned sx = w == desc.Width ? x : x * desc.Width / w;
                        memcpy(drow + x * 4, row + sx * 4, 4);
                    }
                }
                sys->UnlockRect();
                *outW = w;
                *outH = h;
            }
            else
                ok = false;
        }
    }
    if (sys)
        sys->Release();
    if (src)
        src->Release();
    dev->Release();
    return ok;
}

static bool WriteAtlasFile(const BYTE* pixels, unsigned width, unsigned height)
{
    char path[MAX_PATH];
    if (!GetTempPathA(MAX_PATH, path))
        return false;
    strcat_s(path, "FarmBotAtlas.bin");
    FILE* f = nullptr;
    if (fopen_s(&f, path, "wb") != 0 || !f)
        return false;
    unsigned hdr[8] = {};
    hdr[0] = kAtlasMagic;
    hdr[1] = g_atlasSeq;
    hdr[2] = width;
    hdr[3] = height;
    memcpy(hdr + 4, &g_atlasWorldW, 4);
    memcpy(hdr + 5, &g_atlasWorldH, 4);
    hdr[6] = 1;
    const bool ok = fwrite(hdr, 1, 32, f) == 32 &&
        fwrite(pixels, 1, width * height * 4, f) == width * height * 4;
    fclose(f);
    return ok;
}

static void ReadWorldSize(void* mini)
{
    g_atlasWorldW = *(float*)((char*)mini + kAtlasWorldXDisp);
    g_atlasWorldH = *(float*)((char*)mini + kAtlasWorldYDisp);
    if (g_atlasWorldW >= 10000.f && g_atlasWorldH >= 10000.f && g_atlasWorldW <= 500000.f && g_atlasWorldH <= 500000.f)
        return;
    const unsigned dwx = *(unsigned*)((char*)mini + kAtlasWorldXDisp);
    const unsigned dwy = *(unsigned*)((char*)mini + kAtlasWorldYDisp);
    if (dwx >= 10000 && dwy >= 10000 && dwx <= 500000 && dwy <= 500000)
    {
        g_atlasWorldW = (float)dwx;
        g_atlasWorldH = (float)dwy;
        return;
    }
    GuessWorldSize(mini, &g_atlasWorldW, &g_atlasWorldH);
}

static void CaptureAtlasInner()
{
    if (!g_hudOk || !Player())
    {
        g_lastReason = "atlas wait for world";
        return;
    }
    void* mini = MiniMap();
    if (!mini || ReadableBytes(mini, kAtlasWorldYDisp + 8) < (int)(kAtlasWorldYDisp + 8))
    {
        g_lastReason = "minimap singleton is null";
        return;
    }
    const float prevW = g_atlasWorldW;
    const float prevH = g_atlasWorldH;
    ReadWorldSize(mini);
    if (g_atlasWidth >= 64 && g_atlasScratch &&
        g_atlasWorldW == prevW && g_atlasWorldH == prevH && prevW >= 10000.f)
    {
        g_lastOk = true;
        g_lastReason = "atlas cached";
        return;
    }
    void* image = *(void**)((char*)mini + kAtlasImageDisp + kAtlasImageRefDisp);
    if (!UserPtr(image) || ReadableBytes(image, kImageTexDisp + 8) < (int)(kImageTexDisp + 8))
    {
        g_lastReason = "atlas image pointer is null";
        return;
    }
    if (!g_atlasScratch)
        g_atlasScratch = (BYTE*)VirtualAlloc(nullptr, kAtlasPixels, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    BYTE* pixels = g_atlasScratch;
    void* tex8 = *(void**)((char*)image + kImageTexDisp);
    bool copied = pixels && CopyTexture8(tex8, pixels, &g_atlasWidth, &g_atlasHeight);
    if (!copied && pixels)
        copied = CopyCpuImage(image, pixels, &g_atlasWidth, &g_atlasHeight);
    if (!copied)
    {
        g_atlasWidth = 0;
        g_atlasHeight = 0;
        char why[192];
        _snprintf_s(
            why,
            _TRUNCATE,
            "no atlas pixels tex=0x%p d3d8=%d img=0x%p world=%.0fx%.0f",
            tex8,
            IsD3D8Object(tex8),
            image,
            g_atlasWorldW,
            g_atlasWorldH);
        g_lastReason = why;
        return;
    }
    g_atlasSeq++;
    if (g_atlasView)
    {
        unsigned* hdr = (unsigned*)g_atlasView;
        hdr[0] = kAtlasMagic;
        hdr[1] = g_atlasSeq;
        hdr[2] = g_atlasWidth;
        hdr[3] = g_atlasHeight;
        memcpy(hdr + 4, &g_atlasWorldW, 4);
        memcpy(hdr + 5, &g_atlasWorldH, 4);
        hdr[6] = 1;
    }
    if (!WriteAtlasFile(pixels, g_atlasWidth, g_atlasHeight))
    {
        g_lastReason = "atlas file write failed";
        return;
    }
    g_lastOk = true;
    char ok[96];
    _snprintf_s(ok, _TRUNCATE, "atlas %ux%u world=%.0fx%.0f", g_atlasWidth, g_atlasHeight, g_atlasWorldW, g_atlasWorldH);
    g_lastReason = ok;
}

static void CaptureAtlas()
{
    g_lastOk = false;
    g_lastReason = "atlas from client files";
}

static void InitAtlasMap()
{
    g_atlasMap = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, kAtlasBytes, "Local\\FarmBotAtlas");
    if (g_atlasMap)
        g_atlasView = (BYTE*)MapViewOfFile(g_atlasMap, FILE_MAP_ALL_ACCESS, 0, 0, kAtlasBytes);
}

static bool CallPlayerVoid(unsigned rva, void* player)
{
    if (!rva || !g_base || !player)
        return false;
    FnVoid fn = (FnVoid)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    void* selves[2] = { player, (char*)player + kThisAdj };
    for (int i = 0; i < 2; ++i)
    {
        __try
        {
            fn(selves[i]);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return false;
}

static bool CallNewAttack(void* player)
{
    return CallPlayerVoid(kNewAttackRva, player);
}

static bool CallCombo(void* player)
{
    return CallPlayerVoid(kComboRva, player);
}

static void* NetStream()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kNetStreamRva);
    return UserPtr(p) ? p : nullptr;
}

static bool ReadStdString(void* str, char* out, int cap);
static bool ReadPlayerName(void* player, char* out, int cap);
static void CaptureLogin(const char* id, const char* pwd);
static int StartGameFlag(void* net);
static int IsPythonGameWindow();
static int LoadingStepCount();
static int WindowClassIs(const char* name);
static void* SafePyGetAttr(void* obj, const char* name);
static int SafePyTypeName(void* obj, char* out, int cap);

static void* AccountConnector()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kAccountConnectorRva);
    return UserPtr(p) ? p : nullptr;
}

static void* AppSingleton()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kAppSingletonRva);
    return UserPtr(p) ? p : nullptr;
}

static bool ReadPhase(char* out, int cap)
{
    g_sessionPhase = "";
    void* net = NetStream();
    if (!net || !ReadStdString((char*)net + kPhaseDisp, out, cap))
    {
        if (out && cap > 0)
            out[0] = 0;
        return false;
    }
    g_sessionPhase = out;
    return true;
}

static bool PhaseHas(const char* name)
{
    if (!name || g_sessionPhase.empty())
        return false;
    const char* p = g_sessionPhase.c_str();
    const size_t n = strlen(name);
    for (; *p; ++p)
    {
        size_t i = 0;
        while (i < n && p[i] && (p[i] | 32) == (name[i] | 32))
            ++i;
        if (i == n)
            return true;
    }
    return false;
}

static bool IsPastIntro()
{
    return PhaseHas("Login") || PhaseHas("Select") || PhaseHas("Hand") ||
           PhaseHas("Load") || PhaseHas("Game");
}

static void ReadLoginSession()
{
    if (g_sessionAccount.empty() && !g_capturedAccount.empty())
        g_sessionAccount = g_capturedAccount;
    void* net = NetStream();
    if (!net)
        return;
    char id[64];
    if (ReadStdString((char*)net + kLoginIdDisp, id, 64) && id[0])
        g_sessionAccount = id;
    char pwd[64];
    if (ReadStdString((char*)net + kLoginPasswordDisp, pwd, 64) && pwd[0])
        g_capturedPassword = pwd;
    if (PhaseHas("Select") || PhaseHas("Game") || PhaseHas("Load"))
    {
        const unsigned slot = *(unsigned*)((char*)net + kSelectedCharacterDisp);
        if (slot <= 4)
            g_sessionCharacterSlot = (int)slot;
    }
}

static int HasGameWindow()
{
    void* player = Player();
    if (!player)
        return 0;
    __try
    {
        void* gw = *(void**)((char*)player + kGameWindowDisp);
        return UserPtr(gw) ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static void RefreshSession()
{
    char phase[64];
    ReadPhase(phase, 64);
    ReadLoginSession();
    void* player = Player();
    char name[64];
    if (player && ReadPlayerName(player, name, 64) && name[0])
        g_sessionCharacter = name;
    if (PhaseHas("Load") || PhaseHas("Game"))
    {
        const int loading = WindowClassIs("LoadingWindow");
        const int gameWnd = WindowClassIs("GameWindow");
        if (loading)
        {
            const int steps = LoadingStepCount();
            if (steps > g_maxLoadSteps)
                g_maxLoadSteps = steps;
            if (steps > 0)
                g_sawLoadSteps = 1;
            if (steps > 0 && steps <= 3 && g_maxLoadSteps >= 8)
                g_loadNearEnd = 1;
            if (g_loadNearEnd && steps == 0)
                g_loadFinished = 1;
        }
        if (gameWnd && g_loadFinished)
            g_openedGameWindow = 1;
    }
    g_startGame = g_loadFinished;
    g_inWorld = (g_openedGameWindow && g_loadFinished) ? 1 : 0;
}

static int SafePhase(FnPhase fn, void* self)
{
    if (!fn || !self || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        fn(self);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafeSetLogin(FnSetLoginInfo fn, void* self, const char* id, const char* pwd)
{
    if (!fn || !self || !id || !pwd || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        fn(self, id, pwd);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafeConnect(FnAccountConnect fn, void* self, const char* addr, int port, const char* accountAddr, int accountPort)
{
    if (!fn || !self || !addr || !accountAddr || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        return fn(self, addr, port, accountAddr, accountPort);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static void SafeDirectEnter(FnDirectEnterSet fn, void* self, unsigned slot)
{
    if (!fn || !self || IsBadCodePtr((FARPROC)fn))
        return;
    __try
    {
        fn(self, slot);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static int SafeEnter(FnConnectGameServer enter, void* net, unsigned slot)
{
    if (!enter || !net || IsBadCodePtr((FARPROC)enter))
        return 0;
    __try
    {
        enter(net, slot);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafeSelectPacket(FnSelectCharacter fn, void* net, unsigned slot)
{
    if (!fn || !net || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        fn(net, slot);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int StartGameFlag(void* net)
{
    if (!net)
        return 0;
    __try
    {
        return *((unsigned*)((BYTE*)net + kStartGameFlagDisp)) != 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static unsigned DirectEnterSlot(void* net, unsigned fallback)
{
    if (!net)
        return fallback;
    __try
    {
        const unsigned slot = *(unsigned*)((BYTE*)net + kDirectEnterSlotDisp);
        return slot <= 4 ? slot : fallback;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return fallback;
    }
}

static int SafeStartGame(void* net)
{
    if (!net)
        return 0;
    FnPhase fn = (FnPhase)(g_base + kStartGameRva);
    if (fn && !IsBadCodePtr((FARPROC)fn))
    {
        __try
        {
            fn(net);
            return 1;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    __try
    {
        *((unsigned*)((BYTE*)net + kStartGameFlagDisp)) = 1;
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafeNotifyGamePhase(void* net)
{
    if (!net)
        return 0;
    FnNotifyGamePhase fn = (FnNotifyGamePhase)(g_base + kNotifyGamePhaseRva);
    if (!fn || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        return fn(net) ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static void* PythonHandler(void* net)
{
    if (!net)
        return nullptr;
    __try
    {
        void* handler = *(void**)((char*)net + kPythonHandlerDisp);
        return UserPtr(handler) ? handler : nullptr;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return nullptr;
    }
}

static HMODULE PythonDll()
{
    HMODULE py = GetModuleHandleA("python27.dll");
    if (!py)
        py = GetModuleHandleA("python26.dll");
    if (!py)
        py = GetModuleHandleA("python22.dll");
    if (!py)
    {
        HMODULE exe = GetModuleHandleA(nullptr);
        if (exe && (GetProcAddress(exe, "PyObject_CallMethod") || GetProcAddress(exe, "Py_Initialize")))
            py = exe;
    }
    return py;
}

static int SafePySetInt(void* obj, const char* name, long value)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name)
        return 0;
    FnPyIntFromLong fromLong = (FnPyIntFromLong)GetProcAddress(py, "PyInt_FromLong");
    FnPySetAttrString setAttr = (FnPySetAttrString)GetProcAddress(py, "PyObject_SetAttrString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!fromLong || !setAttr)
        return 0;
    __try
    {
        void* boxed = fromLong(value);
        if (!boxed)
            return 0;
        const int ok = setAttr(obj, name, boxed) == 0;
        if (errClear)
            errClear();
        return ok ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafePySetString(void* obj, const char* name, const char* value)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !value)
        return 0;
    FnPyStringFromString fromString = (FnPyStringFromString)GetProcAddress(py, "PyString_FromString");
    if (!fromString)
        fromString = (FnPyStringFromString)GetProcAddress(py, "PyUnicode_FromString");
    FnPySetAttrString setAttr = (FnPySetAttrString)GetProcAddress(py, "PyObject_SetAttrString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!fromString || !setAttr)
        return 0;
    __try
    {
        void* boxed = fromString(value);
        if (!boxed)
            return 0;
        const int ok = setAttr(obj, name, boxed) == 0;
        if (errClear)
            errClear();
        return ok ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafePyCallMethod(void* obj, const char* name, const char* format, int arg)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name)
        return 0;
    FnPyCallMethod call = (FnPyCallMethod)GetProcAddress(py, "PyObject_CallMethod");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!call)
        return 0;
    char method[64];
    char fmt[8];
    lstrcpynA(method, name, 64);
    if (format && format[0])
        lstrcpynA(fmt, format, 8);
    else
        fmt[0] = 0;
    __try
    {
        void* result = fmt[0] ? call(obj, method, fmt, arg) : call(obj, method, NULL);
        if (errClear)
            errClear();
        return result ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafePyAutoSelect(void* net, unsigned slot)
{
    void* handler = PythonHandler(net);
    if (!handler)
        return 0;
    SafePySetInt(handler, "isAutoSelect", 1);
    SafePySetInt(handler, "isAutoLogin", 0);
    return SafePyCallMethod(handler, "SetCharacterSlot", "i", (int)slot);
}

static int SafePyPrepareLogin(const Command& cmd)
{
    void* handler = PythonHandler(NetStream());
    if (!handler)
        return 0;
    const char* authHost = cmd.authHost.empty() ? cmd.host.c_str() : cmd.authHost.c_str();
    const int authPort = cmd.authPort > 0 ? cmd.authPort : cmd.port;
    SafePySetString(handler, "addr", cmd.host.c_str());
    SafePySetInt(handler, "port", cmd.port);
    SafePySetString(handler, "account_addr", authHost);
    SafePySetInt(handler, "account_port", authPort);
    SafePySetString(handler, "id", cmd.account.c_str());
    SafePySetString(handler, "pwd", cmd.password.c_str());
    return SafePyAutoSelect(NetStream(), (unsigned)cmd.slot);
}

static int SafePyClickStart(void* net)
{
    void* handler = PythonHandler(net);
    if (!handler)
        return 0;
    HMODULE py = PythonDll();
    FnPyGetAttrString getAttr = py ? (FnPyGetAttrString)GetProcAddress(py, "PyObject_GetAttrString") : nullptr;
    FnPyErrClear errClear = py ? (FnPyErrClear)GetProcAddress(py, "PyErr_Clear") : nullptr;
    if (!getAttr)
        return 0;
    void* wnd = nullptr;
    __try
    {
        wnd = getAttr(handler, "curPhaseWindow");
        if (errClear)
            errClear();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        wnd = nullptr;
    }
    if (!wnd)
        return 0;
    return SafePyCallMethod(wnd, "StartGame", "", 0);
}

static void* SafePyGetAttr(void* obj, const char* name)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name)
        return nullptr;
    FnPyGetAttrString getAttr = (FnPyGetAttrString)GetProcAddress(py, "PyObject_GetAttrString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!getAttr)
        return nullptr;
    __try
    {
        void* value = getAttr(obj, name);
        if (!value && errClear)
            errClear();
        return value;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return nullptr;
    }
}

static int SafePyTypeName(void* obj, char* out, int cap)
{
    HMODULE py = PythonDll();
    if (!obj || !out || cap <= 1 || !py)
        return 0;
    out[0] = 0;
    FnPyGetAttrString getAttr = (FnPyGetAttrString)GetProcAddress(py, "PyObject_GetAttrString");
    FnPyStringAsString asString = (FnPyStringAsString)GetProcAddress(py, "PyString_AsString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!getAttr || !asString)
        return 0;
    __try
    {
        void* cls = getAttr(obj, "__class__");
        if (!cls)
            return 0;
        void* name = getAttr(cls, "__name__");
        if (!name)
            return 0;
        const char* text = asString(name);
        if (!text)
            return 0;
        lstrcpynA(out, text, cap);
        return out[0] ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        if (errClear)
            errClear();
        return 0;
    }
}

static int WindowClassIs(const char* name)
{
    if (!name || !name[0])
        return 0;
    char cls[40];
    cls[0] = 0;
    void* handler = PythonHandler(NetStream());
    void* wnd = handler ? SafePyGetAttr(handler, "curPhaseWindow") : nullptr;
    if (!wnd || !SafePyTypeName(wnd, cls, 40) || !cls[0])
        return 0;
    return strcmp(cls, name) == 0 ? 1 : 0;
}

static int IsPythonGameWindow()
{
    return WindowClassIs("GameWindow");
}

static int LoadingStepCount()
{
    if (!WindowClassIs("LoadingWindow"))
        return -1;
    void* handler = PythonHandler(NetStream());
    if (!handler)
        return -1;
    void* wnd = SafePyGetAttr(handler, "curPhaseWindow");
    if (!wnd)
        return -1;
    void* steps = SafePyGetAttr(wnd, "loadStepList");
    if (!steps)
        return -1;
    HMODULE py = PythonDll();
    typedef int (__cdecl* FnPyListSize)(void*);
    FnPyListSize size = py ? (FnPyListSize)GetProcAddress(py, "PyList_Size") : nullptr;
    if (!size)
        size = py ? (FnPyListSize)GetProcAddress(py, "PyObject_Size") : nullptr;
    FnPyErrClear errClear = py ? (FnPyErrClear)GetProcAddress(py, "PyErr_Clear") : nullptr;
    if (!size)
        return -1;
    __try
    {
        const int n = size(steps);
        if (n < 0 && errClear)
            errClear();
        return n;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return -1;
    }
}

static int SafePyRun(const char* code)
{
    HMODULE py = PythonDll();
    if (!py || !code)
        return 0;
    typedef int (__cdecl* FnPyRun)(const char*);
    FnPyRun run = (FnPyRun)GetProcAddress(py, "PyRun_SimpleString");
    FnPyGilEnsure gilEnsure = (FnPyGilEnsure)GetProcAddress(py, "PyGILState_Ensure");
    FnPyGilRelease gilRelease = (FnPyGilRelease)GetProcAddress(py, "PyGILState_Release");
    if (!run)
        return 0;
    int gil = 0;
    int haveGil = 0;
    if (gilEnsure && gilRelease)
    {
        __try
        {
            gil = gilEnsure();
            haveGil = 1;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            haveGil = 0;
        }
    }
    int ok = 0;
    __try
    {
        ok = run(code) == 0 ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        ok = 0;
    }
    if (haveGil)
        gilRelease(gil);
    return ok;
}

static int SafePyEnsureServerInfo()
{
    char code[1600];
    _snprintf_s(
        code,
        _TRUNCATE,
        "import serverInfo, net\n"
        "try:\n"
        " r=serverInfo.REGION_DICT[0]\n"
        " n=(net.GetServerInfo() or '').split(',')[0].strip()\n"
        " ok=0\n"
        " for k in r.keys():\n"
        "  if r[k]['name']==n:\n"
        "   ok=1\n"
        "   break\n"
        " if not ok:\n"
        "  k=sorted(r.keys())[0]\n"
        "  s=r[k]\n"
        "  c=s['channel']\n"
        "  i=sorted(c.keys())[0]\n"
        "  p=%d\n"
        "  for ck in c.keys():\n"
        "   if int(c[ck].get('tcp_port',0))==p:\n"
        "    i=ck\n"
        "    break\n"
        "  net.SetServerInfo('%%s, %%s' %% (s['name'], c[i]['name']))\n"
        "except:\n"
        " pass\n",
        g_loginPort);
    return SafePyRun(code);
}

static int SafePyAskGamePhase(void* net)
{
    void* handler = PythonHandler(net);
    if (!handler)
        return 0;
    if (IsPythonGameWindow() && g_loadFinished)
    {
        g_openedGameWindow = 1;
        return 1;
    }

    const int called = SafePyCallMethod(handler, "SetGamePhase", "", 0);
    const int opened = IsPythonGameWindow();
    if (opened && g_loadFinished)
        g_openedGameWindow = 1;
    return called && opened;
}

static bool ExecuteSession(const Command& cmd)
{
    if (cmd.type != "get_phase" && cmd.type != "get_session" && cmd.type != "skip_intro" &&
        cmd.type != "login" && cmd.type != "select_character" && cmd.type != "enter_game")
        return false;

    if (cmd.type == "get_phase" || cmd.type == "get_session")
    {
        RefreshSession();
        g_lastOk = !g_sessionPhase.empty() || !g_sessionAccount.empty();
        g_lastReason = !g_sessionPhase.empty()
            ? g_sessionPhase
            : (g_sessionAccount.empty() ? "phase unread" : "account captured");
        return true;
    }

    if (cmd.type == "skip_intro")
    {
        if (IsPastIntro())
        {
            g_lastOk = true;
            g_lastReason = g_sessionPhase;
            return true;
        }
        void* net = NetStream();
        if (!net)
        {
            g_lastReason = "network stream is null";
            return true;
        }
        if (g_sessionPhase.empty())
        {
            g_lastReason = "intro not ready";
            return true;
        }
        if (!g_skippedIntro)
        {
            if (!kSetLoginPhaseRva)
            {
                RefreshSession();
                g_lastOk = IsPastIntro();
                g_lastReason = g_sessionPhase.empty() ? "waiting for login phase" : g_sessionPhase;
                return true;
            }
            if (!SafePhase((FnPhase)(g_base + kSetLoginPhaseRva), net))
            {
                g_lastReason = "SetLoginPhase trapped";
                return true;
            }
            g_skippedIntro = 1;
        }
        RefreshSession();
        g_lastOk = IsPastIntro();
        g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "skip_intro";
        return true;
    }

    if (cmd.type == "login")
    {
        if (g_loginSent || PhaseHas("Select") || PhaseHas("Load") || PhaseHas("Game"))
        {
            RefreshSession();
            g_lastOk = true;
            g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "login already sent";
            return true;
        }

        g_openedGameWindow = 0;
        g_sawLoadSteps = 0;
        g_loadFinished = 0;
        g_loadNearEnd = 0;
        g_maxLoadSteps = 0;
        g_selectSent = 0;
        void* net = NetStream();
        void* account = AccountConnector();
        if (!net)
        {
            g_lastReason = "network stream is null";
            return true;
        }
        if (!account)
        {
            g_lastReason = "account connector is null";
            return true;
        }
        if (cmd.account.empty() || cmd.password.empty() || cmd.host.empty() || cmd.port <= 0)
        {
            g_lastReason = "login needs account, password, host, port";
            return true;
        }

        FnSetLoginInfo setNet = (FnSetLoginInfo)(g_base + kSetLoginInfoRva);
        FnSetLoginInfo setAcc = (FnSetLoginInfo)(g_base + kAccountSetLoginInfoRva);
        FnAccountConnect connect = (FnAccountConnect)(g_base + kAccountConnectRva);
        if (IsBadCodePtr((FARPROC)setNet) || IsBadCodePtr((FARPROC)setAcc) ||
            IsBadCodePtr((FARPROC)connect))
        {
            g_lastReason = "login natives missing";
            return true;
        }

        CaptureLogin(cmd.account.c_str(), cmd.password.c_str());
        g_loginPort = cmd.port;
        if (!SafeSetLogin(g_setLoginInfo ? g_setLoginInfo : setNet, net, cmd.account.c_str(), cmd.password.c_str()))
        {
            g_lastReason = "SetLoginInfo trapped";
            return true;
        }
        SafeSetLogin(setAcc, account, cmd.account.c_str(), cmd.password.c_str());
        SafePyPrepareLogin(cmd);
        SafePyEnsureServerInfo();

        const char* authHost = cmd.authHost.empty() ? cmd.host.c_str() : cmd.authHost.c_str();
        const int authPort = cmd.authPort > 0 ? cmd.authPort : cmd.port;
        if (!SafeConnect(connect, account, cmd.host.c_str(), cmd.port, authHost, authPort))
        {
            g_lastReason = "ConnectToAccountServer failed";
            return true;
        }
        g_loginSent = 1;
        RefreshSession();
        g_lastOk = true;
        g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "login sent";
        return true;
    }

    if (cmd.type == "enter_game")
    {
        void* net = NetStream();
        if (!net)
        {
            g_lastReason = "network stream is null";
            return true;
        }
        if (g_inWorld)
        {
            g_lastOk = true;
            g_lastReason = g_sessionPhase.empty() ? "Game" : g_sessionPhase;
            return true;
        }
        if (!g_loadFinished)
        {
            RefreshSession();
            const int steps = LoadingStepCount();
            g_lastOk = true;
            if (steps > 0)
                g_lastReason = "still loading";
            else if (!g_sawLoadSteps)
                g_lastReason = "waiting for LoadData";
            else
                g_lastReason = "waiting for load steps";
            return true;
        }
        if (IsPythonGameWindow())
        {
            g_openedGameWindow = 1;
            g_inWorld = 1;
            g_lastOk = true;
            g_lastReason = g_sessionPhase.empty() ? "Game" : g_sessionPhase;
            return true;
        }
        if (!SafePyAskGamePhase(net))
        {
            g_lastReason = "waiting for GameWindow";
            RefreshSession();
            g_lastOk = true;
            return true;
        }
        RefreshSession();
        g_lastOk = g_inWorld != 0;
        g_lastReason = g_inWorld ? (g_sessionPhase.empty() ? "Game" : g_sessionPhase) : "GameWindow did not open";
        return true;
    }

    void* net = NetStream();
    if (!net)
    {
        g_lastReason = "network stream is null";
        return true;
    }
    if (g_inWorld || g_selectSent || PhaseHas("Load") || PhaseHas("Game"))
    {
        g_selectSent = 1;
        g_lastOk = true;
        g_lastReason = g_sessionPhase.empty() ? "already entering" : g_sessionPhase;
        return true;
    }
    SafePyAutoSelect(net, (unsigned)cmd.slot);
    if (!WindowClassIs("SelectCharacterWindow"))
    {
        RefreshSession();
        g_lastOk = true;
        g_lastReason = "waiting for SelectCharacterWindow";
        return true;
    }
    if (!SafePyClickStart(net))
    {
        RefreshSession();
        g_lastOk = true;
        g_lastReason = "waiting for auto DirectEnter";
        return true;
    }
    g_selectSent = 1;
    RefreshSession();
    g_lastOk = true;
    g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "select sent";
    return true;
}

static bool CallSendAttackPacket(unsigned vid)
{
    void* net = NetStream();
    if (!kSendAttackRva)
        return false;
    FnSendAttack fn = (FnSendAttack)(g_base + kSendAttackRva);
    if (!net || IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(net, 0, vid);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallSendState(float x, float y, unsigned func)
{
    void* net = NetStream();
    if (!kSendStateRva)
        return false;
    FnSendState fn = (FnSendState)(g_base + kSendStateRva);
    if (!net || IsBadCodePtr((FARPROC)fn))
        return false;
    float pos[3] = { x, y, 0 };
    __try
    {
        fn(net, pos, 0.f, func, 0);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallSendPickup(unsigned vid)
{
    void* net = NetStream();
    if (!kSendPickupRva)
        return false;
    FnSendPickup fn = (FnSendPickup)(g_base + kSendPickupRva);
    if (!net || IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(net, vid);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallSendClick(void* player, unsigned vid)
{
    if (!kSendClickRva)
        return false;
    FnSendClick fn = (FnSendClick)(g_base + kSendClickRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(player, vid);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool HasVid(const std::vector<ShotHit>& items, unsigned vid)
{
    for (size_t i = 0; i < items.size(); ++i)
    {
        if (items[i].vid == vid)
            return true;
    }
    return false;
}

static void* ItemMgr()
{
    if (!g_base || !kItemSingletonRva)
        return nullptr;
    void* p = *(void**)(g_base + kItemSingletonRva);
    return UserPtr(p) ? p : nullptr;
}

static LONGLONG NowMs()
{
    static LARGE_INTEGER freq;
    LARGE_INTEGER now;
    if (freq.QuadPart == 0)
        QueryPerformanceFrequency(&freq);
    QueryPerformanceCounter(&now);
    if (freq.QuadPart == 0)
        return GetTickCount();
    return now.QuadPart * 1000 / freq.QuadPart;
}

static bool ReadGroundHit(void* node, ShotHit* hit, const char* me)
{
    if (!UserPtr(node) || *((unsigned char*)node + 13) != 0)
        return false;
    unsigned vid = *(unsigned*)((char*)node + 0x10);
    void* inst = *(void**)((char*)node + 0x14);
    if (vid == 0 || !UserPtr(inst) || !hit)
        return false;
    const float ix = *(float*)((char*)inst + 8);
    const float iy = *(float*)((char*)inst + 0xC);
    if (ix != ix || iy != iy)
        return false;
    hit->vid = vid;
    hit->x = ix;
    // GetCloseItem xorps Y with -0.0 before comparing to GetPixelPosition.
    hit->y = -iy;
    hit->money = *(unsigned*)((char*)inst + 4) == 1 ? 1 : 0;
    hit->take = CanTakeGround(inst, me);
    return true;
}

static bool ReadStdString(void* str, char* out, int cap)
{
    if (!UserPtr(str) || !out || cap < 2)
        return false;
    const unsigned size = *(unsigned*)((char*)str + 0x10);
    const unsigned capacity = *(unsigned*)((char*)str + 0x14);
    const char* p = (const char*)str;
    if (capacity > 0xF)
    {
        p = *(const char**)str;
        const uintptr_t a = (uintptr_t)p;
        if (a < 0x10000 || a >= 0x7FFF0000)
            return false;
    }
    if (size == 0)
    {
        out[0] = 0;
        return true;
    }
    if (size > 48)
        return false;
    const unsigned n = size < (unsigned)(cap - 1) ? size : (unsigned)(cap - 1);
    memcpy(out, p, n);
    out[n] = 0;
    return true;
}

static bool ReadPlayerName(void* player, char* out, int cap)
{
    if (!player || !out || cap < 2)
        return false;
    out[0] = 0;
    if (kGetNameSlot)
    {
        __try
        {
            void* iabs = (char*)player + kThisAdj;
            void** vtbl = *(void***)iabs;
            if (UserPtr(vtbl) && UserPtr(vtbl[kGetNameSlot / 4]))
            {
                const char* name = ((FnGetName)vtbl[kGetNameSlot / 4])(iabs);
                const uintptr_t a = (uintptr_t)name;
                if (name && a >= 0x10000 && a < 0x7FFF0000)
                {
                    strncpy_s(out, cap, name, _TRUNCATE);
                    return true;
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return kPlayerNameDisp ? ReadStdString((char*)player + kPlayerNameDisp, out, cap) : false;
}

static int CanTakeGround(void* inst, const char* me)
{
    if (!UserPtr(inst))
        return 1;
    char owner[64];
    if (!ReadStdString((char*)inst + kOwnerNameDisp, owner, 64))
        return 1;
    if (owner[0] == 0)
        return 1;
    if (!me || me[0] == 0)
        return 0;
    return strcmp(owner, me) == 0 ? 1 : 0;
}

static void* ItemLeft(void* node) { return *(void**)node; }
static void* ItemRight(void* node) { return *(void**)((char*)node + 8); }
static void* ItemParent(void* node) { return *(void**)((char*)node + 4); }
static bool ItemNil(void* node) { return !UserPtr(node) || *((unsigned char*)node + 13) != 0; }

static void* ItemSuccessor(void* node, void* head)
{
    if (!UserPtr(node) || node == head)
        return head;
    void* right = ItemRight(node);
    if (!ItemNil(right))
    {
        void* n = right;
        while (!ItemNil(ItemLeft(n)))
            n = ItemLeft(n);
        return n;
    }
    void* cur = node;
    void* parent = ItemParent(cur);
    for (int i = 0; i < 512 && UserPtr(parent) && parent != head; ++i)
    {
        if (cur != ItemRight(parent))
            return parent;
        cur = parent;
        parent = ItemParent(cur);
    }
    return head;
}

static int SafeTreeInc(FnTreeInc inc, void** node)
{
    if (!inc || !node || IsBadCodePtr((FARPROC)inc))
        return 0;
    __try
    {
        inc(node);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static void CollectGroundItems(std::vector<ShotHit>* out, const char* me)
{
    out->clear();
    void* item = ItemMgr();
    if (!item || !g_base)
        return;
    void* head = *(void**)((char*)item + 4);
    if (!UserPtr(head))
        return;
    FnTreeInc inc = kTreeIncRva ? (FnTreeInc)(g_base + kTreeIncRva) : nullptr;
    void* node = *(void**)head;
    if (inc && !IsBadCodePtr((FARPROC)inc))
    {
        for (int i = 0; i < 512 && UserPtr(node) && node != head; ++i)
        {
            ShotHit hit;
            if (ReadGroundHit(node, &hit, me) && !HasVid(*out, hit.vid))
                out->push_back(hit);
            if (!SafeTreeInc(inc, &node))
                break;
        }
    }
    node = *(void**)head;
    for (int i = 0; i < 512 && UserPtr(node) && node != head; ++i)
    {
        ShotHit hit;
        if (ReadGroundHit(node, &hit, me) && !HasVid(*out, hit.vid))
            out->push_back(hit);
        node = ItemSuccessor(node, head);
    }
}

static bool CallGetClose(unsigned rva, float x, float y, unsigned radius, unsigned* vid)
{
    if (!rva)
        return false;
    void* item = ItemMgr();
    FnGetCloseItem fn = (FnGetCloseItem)(g_base + rva);
    if (!item || !vid || !g_base || IsBadCodePtr((FARPROC)fn))
        return false;
    float pos[3] = { x, y, 0 };
    unsigned out = 0;
    __try
    {
        const int ok = fn(item, pos, &out, radius);
        *vid = out;
        return ok && out != 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static void CollectClosest(unsigned rva, float x, float y, float radius, std::vector<ShotHit>* out, int moneyHint, const char* me)
{
    unsigned vid = 0;
    const unsigned dist = radius > 1.f ? (unsigned)radius : (unsigned)kPickDistance;
    if (!CallGetClose(rva, x, y, dist, &vid) || HasVid(*out, vid))
        return;
    void* item = ItemMgr();
    if (item && kTreeIncRva)
    {
        FnTreeInc inc = (FnTreeInc)(g_base + kTreeIncRva);
        if (!IsBadCodePtr((FARPROC)inc))
        {
            void* head = *(void**)((char*)item + 4);
            if (UserPtr(head))
            {
                void* node = *(void**)head;
                for (int i = 0; i < 512 && UserPtr(node) && node != head; ++i)
                {
                    ShotHit hit;
                    if (ReadGroundHit(node, &hit, me) && hit.vid == vid)
                    {
                        if (moneyHint)
                            hit.money = 1;
                        hit.take = 1;
                        out->push_back(hit);
                        return;
                    }
                    if (!SafeTreeInc(inc, &node))
                        break;
                }
            }
        }
    }
    ShotHit hit;
    hit.vid = vid;
    hit.x = x;
    hit.y = y;
    hit.money = moneyHint;
    hit.take = 1;
    out->push_back(hit);
}

static void CollectClosestItem(float x, float y, float radius, std::vector<ShotHit>* out, const char* me)
{
    CollectClosest(kGetCloseItemRva, x, y, radius, out, 0, me);
}

static void CollectClosestMoney(float x, float y, float radius, std::vector<ShotHit>* out, const char* me)
{
    CollectClosest(kGetCloseMoneyRva, x, y, radius, out, 1, me);
}

static void* CallGetMainActor()
{
    void* fromPtr = MainInstance();
    if (!kGetMainActorRva || !g_base)
        return fromPtr;
    FnGetMain fn = (FnGetMain)(g_base + kGetMainActorRva);
    if (IsBadCodePtr((FARPROC)fn))
        return fromPtr;
    void* player = Player();
    if (!player)
        return fromPtr;
    void* selves[2] = { (char*)player + kThisAdj, player };
    for (int i = 0; i < 2; ++i)
    {
        __try
        {
            void* inst = fn(selves[i]);
            if (UserPtr(inst))
                return inst;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return fromPtr;
}

static bool CallInstMove(unsigned rva, void* me, const void* arg)
{
    if (!rva || !me || !arg || !g_base)
        return false;
    FnMoveToDest fn = (FnMoveToDest)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(me, arg);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallGetPixel(void* inst, float* pos)
{
    if (!kGetPixelRva || !inst || !pos || !g_base)
        return false;
    FnGetPixel fn = (FnGetPixel)(g_base + kGetPixelRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(inst, pos);
        return pos[0] != 0.f || pos[1] != 0.f;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallInRange(void* me, void* you)
{
    if (!kInRangeRva || !me || !you || !g_base)
        return false;
    FnInRange fn = (FnInRange)(g_base + kInRangeRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        return fn(me, you) != 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallMoveToDest(unsigned vid, float x, float y)
{
    if (!g_base || (!kMoveToDestRva && !kMoveToPixelRva && !kApproachInstRva))
        return false;
    void* mains[2] = { CallGetMainActor(), MainInstance() };
    void* you = vid ? CallGetInstance(vid) : nullptr;
    float pos[3] = { 0, 0, 0 };
    bool havePixel = you && CallGetPixel(you, pos);
    if (!havePixel && (x != 0.f || y != 0.f))
    {
        pos[0] = x;
        pos[1] = y;
        pos[2] = 0;
        havePixel = true;
    }
    bool ok = false;
    for (int i = 0; i < 2; ++i)
    {
        void* me = mains[i];
        if (!me || (i == 1 && me == mains[0]) || me == you)
            continue;
        if (you)
            ok = CallInstMove(kMoveToDestRva, me, you) || ok;
        if (havePixel)
            ok = CallInstMove(kMoveToPixelRva, me, pos) || ok;
    }
    return ok;
}

static bool ReadActorPixel(void* inst, float* x, float* y)
{
    float pos[3] = { 0, 0, 0 };
    if (CallGetPixel(inst, pos))
    {
        *x = pos[0];
        *y = pos[1];
        return true;
    }
    return ReadPixel(inst, x, y);
}

static bool PixelInMelee(unsigned vid)
{
    if (!vid)
        return false;
    void* you = CallGetInstance(vid);
    if (!you)
        return false;
    void* mains[2] = { CallGetMainActor(), MainInstance() };
    for (int i = 0; i < 2; ++i)
    {
        void* me = mains[i];
        if (!me || me == you || (i == 1 && me == mains[0]))
            continue;
        float mx = 0, my = 0, tx = 0, ty = 0;
        if (!ReadActorPixel(me, &mx, &my) || !ReadActorPixel(you, &tx, &ty))
            continue;
        if ((mx == 0.f && my == 0.f) || (tx == 0.f && ty == 0.f))
            continue;
        const float dx = mx - tx;
        const float dy = my - ty;
        if (dx * dx + dy * dy <= 150.f * 150.f)
            return true;
    }
    return false;
}

static void LogPixels(const char* tag, unsigned vid)
{
    void* me = CallGetMainActor();
    if (!me)
        me = MainInstance();
    void* you = vid ? CallGetInstance(vid) : nullptr;
    float mx = 0, my = 0, tx = 0, ty = 0;
    if (me)
        ReadActorPixel(me, &mx, &my);
    if (you)
        ReadActorPixel(you, &tx, &ty);
    const float dx = mx - tx;
    const float dy = my - ty;
    NativeLog("%s vid=%u me=%p (%.0f,%.0f) you=%p (%.0f,%.0f) dist=%.0f bot=%d",
        tag, vid, me, mx, my, you, tx, ty, sqrtf(dx * dx + dy * dy), (int)g_botCall);
}

static unsigned AutoAttackVidDisp()
{
    return kReservedModeDisp >= 4 ? kReservedModeDisp - 4 : 0x54;
}

static void SetAutoAttackVid(void* player, unsigned vid)
{
    if (!player)
        return;
    const unsigned disp = AutoAttackVidDisp();
    __try
    {
        *(unsigned*)((char*)player + disp) = vid;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    if (kSetAutoRva)
        CallRva(kSetAutoRva, player, vid);
}

static void HoldAttackKeyOff(void* player)
{
    CallAttackKey(player, false);
    if (!player)
        return;
    __try
    {
        *((BYTE*)player + 0x68) = 0;
        if (kThisAdj)
        {
            *((BYTE*)((char*)player + kThisAdj) + 0x68) = 0;
            *((BYTE*)((char*)player - (int)kThisAdj) + 0x68) = 0;
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static BYTE ReadAtkKey(void* player)
{
    BYTE v = 0xFF;
    if (!player)
        return v;
    __try
    {
        v = *((BYTE*)player + 0x68);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    return v;
}

static bool DriveToTarget(void* player, unsigned vid, float x, float y)
{
    (void)x;
    (void)y;
    InterlockedExchange(&g_botCall, 1);
    HoldAttackKeyOff(player);

    if (PixelInMelee(vid))
    {
        InterlockedExchange(&g_allowMelee, 1);
        LogPixels("BOT melee", vid);
        CallSetTarget(player, vid, 1);
        SetAutoAttackVid(player, vid);
        const bool pressed = CallPressActor(player, vid);
        const bool reserved = CallRva(kReserveClickRva, player, vid);
        CallAttackKey(player, true);
        NativeLog("BOT in melee press=%d reserve=%d (attack allowed)", pressed ? 1 : 0, reserved ? 1 : 0);
        InterlockedExchange(&g_botCall, 0);
        return true;
    }

    InterlockedExchange(&g_allowMelee, 0);
    const bool fresh = g_walkVid != vid;
    const DWORD now = GetTickCount();
    const unsigned atk = ReadAtkKey(player);
    if (fresh)
    {
        g_walkVid = vid;
        g_lastWalkMs = now;
        CallSetTarget(player, vid, 1);
        SetAutoAttackVid(player, vid);
        const bool pressed = CallPressActor(player, vid);
        const bool reserved = CallRva(kReserveClickRva, player, vid);
        LogPixels("BOT click-walk", vid);
        NativeLog("BOT click-walk vid=%u press=%d reserve=%d atkKey=%u pressActor=0x%X reserve=0x%X",
            vid, pressed ? 1 : 0, reserved ? 1 : 0, atk, kPressActorRva, kReserveClickRva);
        InterlockedExchange(&g_botCall, 0);
        return false;
    }

    if (now - g_lastWalkMs >= 200)
    {
        g_lastWalkMs = now;
        HoldAttackKeyOff(player);
        SetAutoAttackVid(player, vid);
        CallRva(kReserveClickRva, player, vid);
        LogPixels("BOT keep click-walk", vid);
        NativeLog("BOT keep click-walk atkKey=%u", atk);
    }
    InterlockedExchange(&g_botCall, 0);
    return false;
}

static unsigned PickupNear(float x, float y, const std::vector<ShotHit>& items)
{
    const float maxSq = kPickDistance * kPickDistance;
    unsigned sent = 0;
    for (size_t i = 0; i < items.size(); ++i)
    {
        const float dx = items[i].x - x;
        const float dy = items[i].y - y;
        if (dx * dx + dy * dy > maxSq)
            continue;
        if (!items[i].take)
            continue;
        if (CallSendPickup(items[i].vid))
            ++sent;
    }
    return sent;
}

static bool ReadPixel(void* inst, float* x, float* y)
{
    if (!UserPtr(inst) || !x || !y || !kGraphicDisp || !kPixelXDisp)
        return false;
    __try
    {
        *x = *(float*)((char*)inst + kGraphicDisp + kPixelXDisp);
        *y = *(float*)((char*)inst + kGraphicDisp + kPixelYDisp);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static void* MainInstance()
{
    void* mgr = CharMgr();
    if (!mgr)
        return nullptr;
    void* inst = *(void**)((char*)mgr + kMainPtrDisp);
    return UserPtr(inst) ? inst : nullptr;
}

static bool ReadMainPixel(float* x, float* y)
{
    return ReadPixel(MainInstance(), x, y);
}

static void StandOff(float mobX, float mobY, float playerX, float playerY, float* x, float* y)
{
    float dx = playerX - mobX;
    float dy = playerY - mobY;
    float len = sqrtf(dx * dx + dy * dy);
    if (len < 1.f)
    {
        dx = 1.f;
        dy = 0.f;
        len = 1.f;
    }
    *x = mobX + dx / len * kAvoidStand;
    *y = mobY + dy / len * kAvoidStand;
}

static void ParseHits(Command* cmd, const std::string& src)
{
    cmd->hits.clear();
    size_t i = 0;
    while (i < src.size() && cmd->hits.size() < 1024)
    {
        const size_t semi = src.find(';', i);
        const std::string part = src.substr(i, semi == std::string::npos ? std::string::npos : semi - i);
        unsigned vid = 0;
        float x = 0;
        float y = 0;
        if (sscanf_s(part.c_str(), "%u,%f,%f", &vid, &x, &y) < 3 || vid == 0)
            break;
        ShotHit hit;
        hit.vid = vid;
        hit.x = x;
        hit.y = y;
        hit.money = 0;
        hit.take = 1;
        cmd->hits.push_back(hit);
        if (semi == std::string::npos)
            break;
        i = semi + 1;
    }
}

static bool CallAttackKey(void* player, bool press)
{
    if (!kAttackKeyRva)
        return false;
    FnAttackKey fn = (FnAttackKey)(g_base + kAttackKeyRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    void* selves[2] = { player, (char*)player + kThisAdj };
    for (int i = 0; i < 2; ++i)
    {
        __try
        {
            fn(selves[i], press);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return false;
}

static bool CallPressActor(void* player, unsigned vid)
{
    if (!kPressActorRva || !player || !vid)
        return false;
    void* main = CallGetMainActor();
    if (!main)
        main = MainInstance();
    if (!main)
        return false;
    FnPressActor fn = (FnPressActor)(g_base + kPressActorRva);
    if (!g_base || IsBadCodePtr((FARPROC)fn))
        return false;
    void* selves[2] = { player, (char*)player + kThisAdj };
    for (int i = 0; i < 2; ++i)
    {
        __try
        {
            fn(selves[i], main, vid, 1);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    return false;
}

static void ClearReserved(void* player)
{
    if (!player)
        return;
    __try
    {
        if (kReservedModeDisp)
            *(unsigned*)((char*)player + kReservedModeDisp) = 0;
        if (kVidReservedDisp)
            *(unsigned*)((char*)player + kVidReservedDisp) = 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static void StopCombat(void* player)
{
    g_chaseVid = 0;
    g_walkVid = 0;
    InterlockedExchange(&g_allowMelee, 0);
    CallRva(kSetAutoRva, player, 0);
    SetAutoAttackVid(player, 0);
    ClearReserved(player);
    HoldAttackKeyOff(player);
    CallSetTarget(player, 0, 1);
}

static void ChaseTick(void* player)
{
    if (!g_chaseVid || !player)
        return;
    DriveToTarget(player, g_chaseVid, g_chaseX, g_chaseY);
}

static bool StartCombat(void* player, unsigned vid, float x, float y, bool click, bool autoAttack)
{
    (void)autoAttack;
    if (!click)
    {
        g_chaseVid = 0;
        CallSetTarget(player, vid, 1);
        return false;
    }
    g_chaseVid = vid;
    g_chaseX = x;
    g_chaseY = y;
    DriveToTarget(player, vid, x, y);
    return false;
}

static void Execute(const Command& cmd)
{
    g_lastOk = false;
    g_lastReason = "not executed";
    g_lastVid = 0;
    if (cmd.type == "get_phase" || cmd.type == "get_session" || cmd.type == "skip_intro" ||
        cmd.type == "login" || cmd.type == "select_character" || cmd.type == "enter_game")
        RefreshSession();

    if (cmd.type == "loot_loop")
    {
        g_lootFast = (cmd.vid & 1) ? 1 : 0;
        g_lootRanged = (cmd.vid & 2) ? 1 : 0;
        g_lootOn = (g_lootFast || g_lootRanged) ? 1 : 0;
        g_lootDelay = cmd.x >= 1.f ? (DWORD)cmd.x : 50;
        g_lootRadius = cmd.y > 0.f ? cmd.y : 8000.f;
        if (!g_lootOn)
        {
            g_lastLootMs = 0;
            g_lastFastMs = 0;
            g_lastRangedMs = 0;
        }
        g_lastOk = true;
        if (!g_lootOn)
            g_lastReason = "loot loop off";
        else if (g_lootFast && g_lootRanged)
            g_lastReason = "fast+ranged pickup on";
        else if (g_lootRanged)
            g_lastReason = "ranged pickup on";
        else
            g_lastReason = "fast pickup on";
        return;
    }

    if (cmd.type == "get_atlas")
    {
        CaptureAtlas();
        return;
    }

    if (ExecuteSession(cmd))
        return;

    void* player = Player();
    if (!player)
    {
        g_lastReason = "player singleton is null";
        return;
    }

    if (cmd.type == "pickup")
    {
        if (cmd.vid != 0)
        {
            if (!CallSendPickup(cmd.vid))
            {
                g_lastReason = "SendItemPickUpPacket missing";
                return;
            }
            g_lastOk = true;
            g_lastReason = "SendItemPickUpPacket";
            g_lastVid = cmd.vid;
            return;
        }
        if (!CallPickClose(player))
        {
            g_lastReason = "PickCloseItem missing";
            return;
        }
        g_lastOk = true;
        g_lastReason = "PickCloseItem";
        g_lastVid = ReadTargetVid(player);
        return;
    }

    if (cmd.type == "get_hud")
    {
        RefreshHud(player);
        g_lastOk = g_hudOk;
        g_lastReason = g_hudOk ? "hud" : (g_getStatusSlot < 0 ? "GetStatus vtable missing" : "GetStatus empty");
        return;
    }

    if (cmd.type == "move")
    {
        if (!CallMoveToDest(cmd.vid, cmd.x, cmd.y))
        {
            g_lastReason = "MoveToDestPosition missing";
            return;
        }
        g_lastOk = true;
        g_lastReason = "MoveToDestPosition";
        return;
    }

    if (cmd.type == "stop" || cmd.vid == 0)
    {
        StopCombat(player);
        g_lastOk = true;
        g_lastReason = "stopped";
        return;
    }

    if (cmd.type == "shot")
    {
        std::vector<ShotHit> hits = cmd.hits;
        if (hits.empty() && cmd.vid != 0)
        {
            ShotHit hit;
            hit.vid = cmd.vid;
            hit.x = cmd.x;
            hit.y = cmd.y;
            hits.push_back(hit);
        }
        if (hits.empty())
        {
            g_lastReason = "no ranged targets";
            return;
        }
        CallAttackKey(player, false);
        CallRva(kSetAutoRva, player, 0);
        ClearReserved(player);
        float px = cmd.x;
        float py = cmd.y;
        ReadMainPixel(&px, &py);
        std::vector<ShotHit> loot;
        if (g_lootOn)
        {
            char me[64];
            me[0] = 0;
            ReadPlayerName(player, me, 64);
            CollectGroundItems(&loot, me);
        }
        unsigned sent = 0;
        unsigned picked = 0;
        for (size_t i = 0; i < hits.size(); ++i)
        {
            const ShotHit& hit = hits[i];
            float sx = hit.x;
            float sy = hit.y;
            if (cmd.avoid)
                StandOff(hit.x, hit.y, px, py, &sx, &sy);
            CallSendState(sx, sy, kFuncAttack);
            if (CallSendAttackPacket(hit.vid))
            {
                g_lastVid = hit.vid;
                ++sent;
            }
            else if (CallSetTarget(player, hit.vid, 1) &&
                     (CallNewAttack(player) || CallCombo(player) || CallAttackKey(player, true)))
            {
                g_lastVid = hit.vid;
                ++sent;
            }
            if (g_lootOn)
                picked += PickupNear(sx, sy, loot);
        }
        float rx = cmd.x;
        float ry = cmd.y;
        ReadMainPixel(&rx, &ry);
        CallSendState(rx, ry, kFuncWait);
        g_lastOk = sent != 0;
        char reason[80];
        _snprintf_s(reason, _TRUNCATE, "ranged blast %u/%u loot %u", sent, (unsigned)hits.size(), picked);
        g_lastReason = reason;
        return;
    }

    if (cmd.type == "set_target" || cmd.type == "face" || cmd.type == "press_actor" || cmd.type == "attack")
    {
        NativeLog("BOT cmd=%s vid=%u xy=%.0f,%.0f", cmd.type.c_str(), cmd.vid, cmd.x, cmd.y);
        const void* inst = CallGetInstance(cmd.vid);
        const bool click = cmd.type == "attack" || cmd.type == "press_actor";
        if (!click)
        {
            if (!CallSetTarget(player, cmd.vid, 1))
            {
                g_lastReason = "SetTarget vtable missing";
                return;
            }
            g_chaseVid = 0;
            g_lastVid = ReadTargetVid(player);
            g_lastOk = g_lastVid == cmd.vid;
            g_lastReason = g_lastOk ? "selected" : "SetTarget did not stick";
            return;
        }
        StartCombat(player, cmd.vid, cmd.x, cmd.y, true, false);
        g_lastVid = cmd.vid;
        g_lastOk = true;
        g_lastReason = inst ? "reserve-click walk" : "reserve-click walk (instance lookup missed)";
        return;
    }

    if (cmd.type == "attack_key")
    {
        if (!CallAttackKey(player, cmd.force != 0))
        {
            g_lastReason = "SetAttackKeyState missing";
            return;
        }
        g_lastVid = ReadTargetVid(player);
        g_lastOk = true;
        g_lastReason = cmd.force ? "attack key down" : "attack key up";
        return;
    }

    g_lastReason = "unsupported action";
}

static void Drain();

static void SafeDrain()
{
    __try
    {
        Drain();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        g_lastOk = false;
        g_lastReason = "native action trapped";
    }
}

static void Drain()
{
    if (InterlockedCompareExchange(&g_busy, 1, 0) != 0)
        return;
    EnterCriticalSection(&g_lock);
    while (!g_queue.empty())
    {
        Command cmd = g_queue.front();
        g_queue.pop();
        LeaveCriticalSection(&g_lock);
        Execute(cmd);
        SetEvent(g_done);
        EnterCriticalSection(&g_lock);
    }
    LeaveCriticalSection(&g_lock);
    ChaseTick(Player());
    InterlockedExchange(&g_busy, 0);
}

// CMSApplication::IsMessage is PeekMessage(NULL, 0, 0, PM_NOREMOVE).
// MessageProcess is GetMessage(NULL, 0, 0). Both run on CPythonApplication::Loop.
static void OnLoopPump(HWND hwnd, UINT min, UINT max)
{
    if (hwnd != nullptr || min != 0 || max != 0)
        return;
    g_loopTid = GetCurrentThreadId();
    SafeDrain();
    MaybeAutoLoot(Player());
}

static BOOL WINAPI HookPeekA(LPMSG msg, HWND hwnd, UINT min, UINT max, UINT remove)
{
    OnLoopPump(hwnd, min, max);
    return g_peekA(msg, hwnd, min, max, remove);
}

static BOOL WINAPI HookPeekW(LPMSG msg, HWND hwnd, UINT min, UINT max, UINT remove)
{
    OnLoopPump(hwnd, min, max);
    return g_peekW(msg, hwnd, min, max, remove);
}

static BOOL WINAPI HookGetA(LPMSG msg, HWND hwnd, UINT min, UINT max)
{
    OnLoopPump(hwnd, min, max);
    return g_getA(msg, hwnd, min, max);
}

static BOOL WINAPI HookGetW(LPMSG msg, HWND hwnd, UINT min, UINT max)
{
    OnLoopPump(hwnd, min, max);
    return g_getW(msg, hwnd, min, max);
}

static void CaptureLogin(const char* id, const char* pwd)
{
    if (id && id[0])
    {
        g_capturedAccount = id;
        g_sessionAccount = id;
    }
    if (pwd && pwd[0])
        g_capturedPassword = pwd;
}

static void __fastcall HookSetLoginInfo(void* self, void*, const char* id, const char* pwd)
{
    CaptureLogin(id, pwd);
    if (g_setLoginInfo)
        g_setLoginInfo(self, id, pwd);
}

static void __fastcall HookAttackKey(void* self, void*, bool press)
{
    NativeLog("%s AttackKey this=%p press=%d", g_botCall ? "BOT" : "GAME", self, press ? 1 : 0);
    if (g_origAttackKey)
        g_origAttackKey(self, press);
}

static void __fastcall HookReserveClick(void* self, void*, unsigned vid)
{
    unsigned mode = 0;
    __try
    {
        mode = *(unsigned*)((char*)self + 0x58);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    NativeLog("%s ReserveClick this=%p vid=%u beforeMode=%u", g_botCall ? "BOT" : "GAME", self, vid, mode);
    LogPixels(g_botCall ? "BOT ReserveClick" : "GAME ReserveClick", vid);
    if (g_origReserveClick)
        g_origReserveClick(self, vid);
}

static void __fastcall HookSetTarget(void* self, void*, unsigned vid, int force)
{
    NativeLog("%s SetTarget this=%p vid=%u force=%d", g_botCall ? "BOT" : "GAME", self, vid, force);
    if (!g_botCall)
        LogPixels("GAME SetTarget", vid);
    if (g_origSetTarget)
        g_origSetTarget(self, vid, force);
}

static void __fastcall HookCombo(void* self, void*)
{
    NativeLog("%s Combo this=%p allowMelee=%d chase=%u",
        g_botCall ? "BOT" : "GAME", self, (int)g_allowMelee, g_chaseVid);
    if (g_chaseVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) == 0)
        return;
    if (g_origCombo)
        g_origCombo(self);
}

static void __fastcall GuardCombo(void* self, void*)
{
    if (g_chaseVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) == 0)
        return;
    FnVoid combo = (FnVoid)(g_base + kComboRva);
    if (combo && !IsBadCodePtr((FARPROC)combo))
        combo(self);
}

static void PatchUpdateComboCall()
{
    if (!g_base || !kPlayerUpdateRva || !kComboRva || g_updateComboCall)
        return;
    BYTE* upd = g_base + kPlayerUpdateRva;
    for (int i = 0; i < 0x280; ++i)
    {
        if (upd[i] != 0xE8)
            continue;
        const int rel = *(int*)(upd + i + 1);
        if ((unsigned)(upd + i + 5 + rel - g_base) != kComboRva)
            continue;
        DWORD oldp = 0;
        if (!VirtualProtect(upd + i, 5, PAGE_EXECUTE_READWRITE, &oldp))
            return;
        const int nrel = (int)((BYTE*)GuardCombo - (upd + i + 5));
        upd[i] = 0xE8;
        *(int*)(upd + i + 1) = nrel;
        VirtualProtect(upd + i, 5, oldp, &oldp);
        g_updateComboCall = upd + i;
        NativeLog("patched Update Combo call +0x%X -> GuardCombo (skip in-place swing while walking)", i);
        return;
    }
    NativeLog("Update Combo call site not found in 0x280");
}

static void __fastcall HookChase(void* self, void*)
{
    unsigned mode = 0, rvid = 0;
    __try
    {
        mode = *(unsigned*)((char*)self + (kReservedModeDisp ? kReservedModeDisp : 0x58));
        rvid = kVidReservedDisp ? *(unsigned*)((char*)self + kVidReservedDisp) : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    NativeLog("%s Chase this=%p mode=%u rvid=%u", g_botCall ? "BOT" : "GAME", self, mode, rvid);
    LogPixels(g_botCall ? "BOT Chase" : "GAME Chase", rvid);
    if (g_origChase)
        g_origChase(self);
}

static int __fastcall HookInRange(void* me, void*, const void* you)
{
    int ok = 0;
    if (g_origInRange)
        ok = g_origInRange(me, you);
    static DWORD last = 0;
    const DWORD now = GetTickCount();
    if (now - last >= 200)
    {
        last = now;
        NativeLog("%s InRange me=%p you=%p -> %d", g_botCall ? "BOT" : "GAME", me, you, ok);
    }
    return ok;
}

static int __fastcall HookMoveToDest(void* me, void*, const void* dest)
{
    static DWORD last = 0;
    const DWORD now = GetTickCount();
    if (now - last >= 200)
    {
        last = now;
        NativeLog("%s MoveToDest me=%p dest=%p", g_botCall ? "BOT" : "GAME", me, dest);
    }
    return g_origMoveToDest ? g_origMoveToDest(me, dest) : 0;
}

static void __fastcall HookUpdate(void* self)
{
    g_loopTid = GetCurrentThreadId();
    SafeDrain();
    unsigned mode = 0, rvid = 0;
    __try
    {
        mode = *(unsigned*)((char*)self + (kReservedModeDisp ? kReservedModeDisp : 0x58));
        rvid = kVidReservedDisp ? *(unsigned*)((char*)self + kVidReservedDisp) : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    if (mode != g_lastUpdateMode)
    {
        NativeLog("GAME Update mode %u -> %u rvid=%u chaseVid=%u", g_lastUpdateMode, mode, rvid, g_chaseVid);
        g_lastUpdateMode = mode;
    }
    g_playerUpdate(self);
    MaybeAutoLoot(Player());
    RefreshHud(Player());
}

static BYTE* Unwrap(BYTE* p)
{
    for (int i = 0; i < 6 && p; ++i)
    {
        if (p[0] == 0xE9)
        {
            p = p + 5 + *(int*)(p + 1);
            continue;
        }
        if (p[0] == 0xEB)
        {
            p = p + 2 + (char)p[1];
            continue;
        }
        if (p[0] == 0xFF && p[1] == 0x25)
        {
            p = *(BYTE**)(*(DWORD*)(p + 2));
            continue;
        }
        break;
    }
    return p;
}

static unsigned StealLen(const BYTE* p)
{
    if (p[0] == 0x8B && p[1] == 0xFF && p[2] == 0x55 && p[3] == 0x8B && p[4] == 0xEC)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x83 && p[4] == 0xEC)
        return 6;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x5D)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x56 && p[4] == 0x8B && p[5] == 0x75)
        return 7;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x56 && p[4] == 0x8B && p[5] == 0xF1)
        return 7;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x56)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x53)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x51)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x8B && p[4] == 0x45)
        return 6;
    if (p[0] == 0x8B && p[1] == 0xFF && p[2] == 0x55 && p[3] == 0x8B && p[4] == 0xEC)
        return 5;
    return 0;
}

static bool Trampoline(void* target, void* hook, void** orig)
{
    if (!target || !hook || !orig || *orig)
        return *orig != nullptr;
    BYTE* src = Unwrap((BYTE*)target);
    if (!src)
        return false;
    const unsigned n = StealLen(src);
    if (n < 5)
        return false;
    BYTE* tramp = (BYTE*)VirtualAlloc(nullptr, 32, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!tramp)
        return false;
    memcpy(tramp, src, n);
    tramp[n] = 0xE9;
    *(DWORD*)(tramp + n + 1) = (DWORD)(src + n - (tramp + n + 5));
    DWORD oldp = 0;
    if (!VirtualProtect(src, n, PAGE_EXECUTE_READWRITE, &oldp))
        return false;
    src[0] = 0xE9;
    *(DWORD*)(src + 1) = (DWORD)((BYTE*)hook - (src + 5));
    for (unsigned i = 5; i < n; ++i)
        src[i] = 0x90;
    VirtualProtect(src, n, oldp, &oldp);
    FlushInstructionCache(GetCurrentProcess(), src, n);
    *orig = tramp;
    return true;
}

static bool SwapIat(HMODULE mod, FARPROC want, void* hook, void** orig)
{
    if (!mod || !want || !hook || !orig || *orig)
        return *orig != nullptr;
    BYTE* base = (BYTE*)mod;
    IMAGE_DOS_HEADER* dos = (IMAGE_DOS_HEADER*)base;
    if (dos->e_magic != IMAGE_DOS_SIGNATURE)
        return false;
    IMAGE_NT_HEADERS32* nt = (IMAGE_NT_HEADERS32*)(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE)
        return false;
    const DWORD rva = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
    if (!rva)
        return false;
    IMAGE_IMPORT_DESCRIPTOR* desc = (IMAGE_IMPORT_DESCRIPTOR*)(base + rva);
    for (; desc->FirstThunk; ++desc)
    {
        IMAGE_THUNK_DATA32* iat = (IMAGE_THUNK_DATA32*)(base + desc->FirstThunk);
        for (; iat->u1.Function; ++iat)
        {
            if ((FARPROC)iat->u1.Function != want)
                continue;
            DWORD oldp = 0;
            if (!VirtualProtect(&iat->u1.Function, sizeof(DWORD), PAGE_READWRITE, &oldp))
                return false;
            *orig = (void*)iat->u1.Function;
            iat->u1.Function = (DWORD)hook;
            VirtualProtect(&iat->u1.Function, sizeof(DWORD), oldp, &oldp);
            return true;
        }
    }
    return false;
}

static void InstallLoopHooks(HMODULE client)
{
    HMODULE user = GetModuleHandleA("user32.dll");
    if (!user)
        return;
    FARPROC peekA = GetProcAddress(user, "PeekMessageA");
    FARPROC peekW = GetProcAddress(user, "PeekMessageW");
    FARPROC getA = GetProcAddress(user, "GetMessageA");
    FARPROC getW = GetProcAddress(user, "GetMessageW");
    // Hook user32 first so Drain/RefreshSession run even when this exe's IAT is unused.
    if (Trampoline(peekA, (void*)HookPeekA, (void**)&g_peekA))
        ++g_iatHooks;
    if (Trampoline(peekW, (void*)HookPeekW, (void**)&g_peekW))
        ++g_iatHooks;
    if (Trampoline(getA, (void*)HookGetA, (void**)&g_getA))
        ++g_iatHooks;
    if (Trampoline(getW, (void*)HookGetW, (void**)&g_getW))
        ++g_iatHooks;
    if (client)
    {
        if (SwapIat(client, peekA, (void*)HookPeekA, (void**)&g_peekA))
            ++g_iatHooks;
        if (SwapIat(client, peekW, (void*)HookPeekW, (void**)&g_peekW))
            ++g_iatHooks;
        if (SwapIat(client, getA, (void*)HookGetA, (void**)&g_getA))
            ++g_iatHooks;
        if (SwapIat(client, getW, (void*)HookGetW, (void**)&g_getW))
            ++g_iatHooks;
        if (kPlayerUpdateRva && Trampoline((BYTE*)client + kPlayerUpdateRva, (void*)HookUpdate, (void**)&g_playerUpdate))
            ++g_iatHooks;
        if (kSetLoginInfoRva && Trampoline((BYTE*)client + kSetLoginInfoRva, (void*)HookSetLoginInfo, (void**)&g_setLoginInfo))
            ++g_iatHooks;
        NativeLog("hook AttackKey=%d Reserve=%d SetTarget=%d Combo=%d Chase=%d InRange=%d MoveTo=%d Update=%d",
            kAttackKeyRva && Trampoline((BYTE*)client + kAttackKeyRva, (void*)HookAttackKey, (void**)&g_origAttackKey) ? 1 : 0,
            kReserveClickRva && Trampoline((BYTE*)client + kReserveClickRva, (void*)HookReserveClick, (void**)&g_origReserveClick) ? 1 : 0,
            kSetTargetRva && Trampoline((BYTE*)client + kSetTargetRva, (void*)HookSetTarget, (void**)&g_origSetTarget) ? 1 : 0,
            kComboRva && Trampoline((BYTE*)client + kComboRva, (void*)HookCombo, (void**)&g_origCombo) ? 1 : 0,
            kChaseRva && Trampoline((BYTE*)client + kChaseRva, (void*)HookChase, (void**)&g_origChase) ? 1 : 0,
            kInRangeRva && Trampoline((BYTE*)client + kInRangeRva, (void*)HookInRange, (void**)&g_origInRange) ? 1 : 0,
            kMoveToDestRva && Trampoline((BYTE*)client + kMoveToDestRva, (void*)HookMoveToDest, (void**)&g_origMoveToDest) ? 1 : 0,
            g_playerUpdate ? 1 : 0);
        PatchUpdateComboCall();
    }
}

static bool JsonSpace(char c)
{
    return c == ' ' || c == '\t' || c == '\r' || c == '\n';
}

static void JsonEscape(const char* in, char* out, int cap)
{
    if (!out || cap < 2)
        return;
    int w = 0;
    if (!in)
        in = "";
    for (int i = 0; in[i] && w + 2 < cap; ++i)
    {
        const char c = in[i];
        if (c == '\\' || c == '"')
        {
            if (w + 3 >= cap)
                break;
            out[w++] = '\\';
            out[w++] = c;
        }
        else if ((unsigned char)c < 32)
        {
            out[w++] = ' ';
        }
        else
            out[w++] = c;
    }
    out[w] = 0;
}

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
    while (i < src.size() && JsonSpace(src[i]))
        ++i;
    if (i < src.size() && src[i] == '\"')
    {
        ++i;
        size_t end = i;
        while (end < src.size() && src[end] != '\"')
            ++end;
        return src.substr(i, end - i);
    }
    size_t end = i;
    while (end < src.size() && !JsonSpace(src[end]) && src[end] != ',' && src[end] != '}' && src[end] != ']')
        ++end;
    return src.substr(i, end - i);
}

static unsigned JsonU32(const std::string& src, const char* key)
{
    return (unsigned)strtoul(JsonString(src, key).c_str(), nullptr, 10);
}

static float JsonFloat(const std::string& src, const char* key)
{
    return (float)strtod(JsonString(src, key).c_str(), nullptr);
}

static bool JsonTrue(const std::string& src, const char* key)
{
    const std::string value = JsonString(src, key);
    return value == "true" || value == "1";
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

static unsigned ParseHex(const char* text)
{
    unsigned value = 0;
    if (!text)
        return 0;
    while (*text == ' ' || *text == '\t')
        text++;
    sscanf_s(text, "%x", &value);
    return value;
}

static void ClearLoginRvas()
{
    kNetStreamRva = 0;
    kAccountConnectorRva = 0;
    kSetLoginInfoRva = 0;
    kAccountSetLoginInfoRva = 0;
    kAccountConnectRva = 0;
    kConnectGameServerRva = 0;
    kSelectCharacterRva = 0;
    kSetLoginPhaseRva = 0;
    kSetGamePhaseRva = 0;
    kStartGameRva = 0;
    kNotifyGamePhaseRva = 0;
    kOnLogoCloseRva = 0;
    kPlayerUpdateRva = 0;
    kSingletonRva = 0;
    kCharMgrRva = 0;
    kGetNameSlot = 0;
    kNewAttackRva = 0;
    kComboRva = 0;
    kAttackKeyRva = 0;
    kSetTargetSlot = 0;
    kSetTargetRva = 0;
    kSendAttackRva = 0;
    kSendStateRva = 0;
    kSendPickupRva = 0;
    kGetStatusRva = 0;
    kGetStatusSlot = 0;
    kPointsDisp = 0;
    kPointXor = 0;
    kPickCloseRva = 0;
    kPickCloseMoneyRva = 0;
    kSetAutoRva = 0;
    kReserveClickRva = 0;
    kTargetDisp = 0;
    kItemSingletonRva = 0;
    kGetCloseItemRva = 0;
    kGetCloseMoneyRva = 0;
    kTreeIncRva = 0;
    kMoveToDestRva = 0;
    kMoveToPixelRva = 0;
    kGetMainActorRva = 0;
    kPressActorRva = 0;
    kChaseRva = 0;
    kApproachInstRva = 0;
    kGetPixelRva = 0;
    kInRangeRva = 0;
    kSendClickRva = 0;
    kReservedModeDisp = 0;
    kVidReservedDisp = 0;
}

static void LoadConfig()
{
    ClearLoginRvas();
    char dir[MAX_PATH];
    GetTempPathA(MAX_PATH, dir);
    char path[MAX_PATH];
    _snprintf_s(path, _TRUNCATE, "%sFarmBotAction_%lu.cfg", dir, GetCurrentProcessId());
    FILE* f = nullptr;
    if (fopen_s(&f, path, "r") != 0 || !f)
    {
        _snprintf_s(path, _TRUNCATE, "%sFarmBotAction.cfg", dir);
        if (fopen_s(&f, path, "r") != 0 || !f)
            return;
    }
    char line[512];
    while (fgets(line, sizeof(line), f))
    {
        if (strncmp(line, "pipe=", 5) == 0)
        {
            g_pipeName = line + 5;
            while (!g_pipeName.empty() && (g_pipeName.back() == '\n' || g_pipeName.back() == '\r'))
                g_pipeName.pop_back();
        }
        else if (strncmp(line, "token=", 6) == 0)
        {
            g_token = line + 6;
            while (!g_token.empty() && (g_token.back() == '\n' || g_token.back() == '\r'))
                g_token.pop_back();
        }
        else if (strncmp(line, "layout=recovered", 16) == 0)
        {
            g_layoutRecovered = 1;
            ClearLoginRvas();
        }
        else if (strncmp(line, "net=", 4) == 0)
            kNetStreamRva = ParseHex(line + 4);
        else if (strncmp(line, "account=", 8) == 0)
            kAccountConnectorRva = ParseHex(line + 8);
        else if (strncmp(line, "setLogin=", 9) == 0)
            kSetLoginInfoRva = ParseHex(line + 9);
        else if (strncmp(line, "accountSetLogin=", 16) == 0)
            kAccountSetLoginInfoRva = ParseHex(line + 16);
        else if (strncmp(line, "connect=", 8) == 0)
            kAccountConnectRva = ParseHex(line + 8);
        else if (strncmp(line, "direct=", 7) == 0)
            kConnectGameServerRva = ParseHex(line + 7);
        else if (strncmp(line, "select=", 7) == 0)
            kSelectCharacterRva = ParseHex(line + 7);
        else if (strncmp(line, "setLoginPhase=", 14) == 0)
            kSetLoginPhaseRva = ParseHex(line + 14);
        else if (strncmp(line, "setGamePhase=", 13) == 0)
            kSetGamePhaseRva = ParseHex(line + 13);
        else if (strncmp(line, "startGame=", 10) == 0)
            kStartGameRva = ParseHex(line + 10);
        else if (strncmp(line, "notifyGame=", 11) == 0)
            kNotifyGamePhaseRva = ParseHex(line + 11);
        else if (strncmp(line, "logoClose=", 10) == 0)
            kOnLogoCloseRva = ParseHex(line + 10);
        else if (strncmp(line, "phaseDisp=", 10) == 0)
        {
            const unsigned v = ParseHex(line + 10);
            if (v)
                kPhaseDisp = v;
        }
        else if (strncmp(line, "player=", 7) == 0)
            kSingletonRva = ParseHex(line + 7);
        else if (strncmp(line, "charMgr=", 8) == 0)
            kCharMgrRva = ParseHex(line + 8);
        else if (strncmp(line, "playerUpdate=", 13) == 0)
            kPlayerUpdateRva = ParseHex(line + 13);
        else if (strncmp(line, "playerName=", 11) == 0)
            kPlayerNameDisp = ParseHex(line + 11);
        else if (strncmp(line, "thisAdj=", 8) == 0)
            kThisAdj = ParseHex(line + 8);
        else if (strncmp(line, "setTargetSlot=", 14) == 0)
            kSetTargetSlot = ParseHex(line + 14);
        else if (strncmp(line, "setTarget=", 10) == 0)
            kSetTargetRva = ParseHex(line + 10);
        else if (strncmp(line, "targetDisp=", 11) == 0)
            kTargetDisp = ParseHex(line + 11);
        else if (strncmp(line, "attackKey=", 10) == 0)
            kAttackKeyRva = ParseHex(line + 10);
        else if (strncmp(line, "newAttack=", 10) == 0)
            kNewAttackRva = ParseHex(line + 10);
        else if (strncmp(line, "combo=", 6) == 0)
            kComboRva = ParseHex(line + 6);
        else if (strncmp(line, "setAuto=", 8) == 0)
            kSetAutoRva = ParseHex(line + 8);
        else if (strncmp(line, "reserve=", 8) == 0)
            kReserveClickRva = ParseHex(line + 8);
        else if (strncmp(line, "reservedMode=", 13) == 0)
            kReservedModeDisp = ParseHex(line + 13);
        else if (strncmp(line, "vidReserved=", 12) == 0)
            kVidReservedDisp = ParseHex(line + 12);
        else if (strncmp(line, "moveTo=", 7) == 0)
            kMoveToDestRva = ParseHex(line + 7);
        else if (strncmp(line, "movePixel=", 10) == 0)
            kMoveToPixelRva = ParseHex(line + 10);
        else if (strncmp(line, "getMain=", 8) == 0)
            kGetMainActorRva = ParseHex(line + 8);
        else if (strncmp(line, "pressActor=", 11) == 0)
            kPressActorRva = ParseHex(line + 11);
        else if (strncmp(line, "chase=", 6) == 0)
            kChaseRva = ParseHex(line + 6);
        else if (strncmp(line, "approachInst=", 13) == 0)
            kApproachInstRva = ParseHex(line + 13);
        else if (strncmp(line, "getPixel=", 9) == 0)
            kGetPixelRva = ParseHex(line + 9);
        else if (strncmp(line, "inRange=", 8) == 0)
            kInRangeRva = ParseHex(line + 8);
        else if (strncmp(line, "pickClose=", 10) == 0)
            kPickCloseRva = ParseHex(line + 10);
        else if (strncmp(line, "getStatus=", 10) == 0)
            kGetStatusRva = ParseHex(line + 10);
        else if (strncmp(line, "getStatusSlot=", 14) == 0)
            kGetStatusSlot = ParseHex(line + 14);
        else if (strncmp(line, "item=", 5) == 0)
            kItemSingletonRva = ParseHex(line + 5);
        else if (strncmp(line, "getClose=", 9) == 0)
            kGetCloseItemRva = ParseHex(line + 9);
        else if (strncmp(line, "points=", 7) == 0)
            kPointsDisp = ParseHex(line + 7);
        else if (strncmp(line, "xor=", 4) == 0)
            kPointXor = ParseHex(line + 4);
        else if (strncmp(line, "sendAttack=", 11) == 0)
            kSendAttackRva = ParseHex(line + 11);
        else if (strncmp(line, "sendState=", 10) == 0)
            kSendStateRva = ParseHex(line + 10);
        else if (strncmp(line, "sendPickup=", 11) == 0)
            kSendPickupRva = ParseHex(line + 11);
        else if (strncmp(line, "mainPtr=", 8) == 0)
            kMainPtrDisp = ParseHex(line + 8);
        else if (strncmp(line, "mapDisp=", 8) == 0)
            kMapDisp = ParseHex(line + 8);
        else if (strncmp(line, "mapHead=", 8) == 0)
            kMapHeadOff = ParseHex(line + 8);
        else if (strncmp(line, "nodeLeft=", 9) == 0)
            kNodeLeft = ParseHex(line + 9);
        else if (strncmp(line, "nodeParent=", 11) == 0)
            kNodeParent = ParseHex(line + 11);
        else if (strncmp(line, "nodeRight=", 10) == 0)
            kNodeRight = ParseHex(line + 10);
        else if (strncmp(line, "nodeIsNil=", 10) == 0)
            kNodeIsNil = ParseHex(line + 10);
        else if (strncmp(line, "nodeKey=", 8) == 0)
            kNodeKey = ParseHex(line + 8);
        else if (strncmp(line, "nodeValue=", 10) == 0)
            kNodeValue = ParseHex(line + 10);
        else if (strncmp(line, "graphic=", 8) == 0)
            kGraphicDisp = ParseHex(line + 8);
        else if (strncmp(line, "pixelX=", 7) == 0)
            kPixelXDisp = ParseHex(line + 7);
        else if (strncmp(line, "pixelY=", 7) == 0)
            kPixelYDisp = ParseHex(line + 7);
        else if (strncmp(line, "loginId=", 8) == 0)
            kLoginIdDisp = ParseHex(line + 8);
        else if (strncmp(line, "loginPwd=", 9) == 0)
            kLoginPasswordDisp = ParseHex(line + 9);
    }
    fclose(f);
}

static HMODULE FindClient()
{
    return GetModuleHandleA(nullptr);
}

static void ClosePipe()
{
    if (!g_pipe || g_pipe == INVALID_HANDLE_VALUE)
        return;
    FlushFileBuffers(g_pipe);
    DisconnectNamedPipe(g_pipe);
    CloseHandle(g_pipe);
    g_pipe = INVALID_HANDLE_VALUE;
}

static void Serve()
{
    char path[256];
    _snprintf_s(path, _TRUNCATE, "\\\\.\\pipe\\%s", g_pipeName.c_str());
    char readyName[256];
    _snprintf_s(readyName, _TRUNCATE, "Local\\FarmBotReady_%s", g_pipeName.c_str());
    HANDLE ready = CreateEventA(nullptr, TRUE, FALSE, readyName);

    for (;;)
    {
    ClosePipe();
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
        Sleep(200);
        continue;
    }
    if (ready)
        SetEvent(ready);

    if (!ConnectNamedPipe(g_pipe, nullptr) && GetLastError() != ERROR_PIPE_CONNECTED)
        continue;

    std::string json;
    if (!ReadFrame(&json))
        continue;
    if (JsonString(json, "type") != "auth" || !TokensEqual(JsonString(json, "token"), g_token))
    {
        WriteFrame("{\"ok\":false,\"type\":\"auth\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"vtable-thiscall\"}");
        continue;
    }
    char auth[256];
    _snprintf_s(auth, _TRUNCATE,
        "{\"ok\":true,\"type\":\"auth\",\"reason\":\"ok\",\"targetVid\":0,\"convention\":\"loop-iat-%d\"}",
        g_iatHooks);
    WriteFrame(auth);

    while (ReadFrame(&json))
    {
        Command cmd;
        cmd.type = JsonString(json, "type");
        cmd.vid = JsonU32(json, "vid");
        cmd.force = JsonTrue(json, "force") ? 1 : 0;
        cmd.avoid = JsonTrue(json, "avoid") ? 1 : 0;
        cmd.x = JsonFloat(json, "x");
        cmd.y = JsonFloat(json, "y");
        ParseHits(&cmd, JsonString(json, "hits"));
        cmd.account = JsonString(json, "account");
        cmd.password = JsonString(json, "password");
        cmd.host = JsonString(json, "host");
        cmd.authHost = JsonString(json, "authHost");
        cmd.port = (int)JsonU32(json, "port");
        cmd.authPort = (int)JsonU32(json, "authPort");
        cmd.slot = (int)JsonU32(json, "slot");
        if (!TokensEqual(JsonString(json, "token"), g_token))
        {
            WriteFrame("{\"ok\":false,\"type\":\"error\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"loop-iat\"}");
            continue;
        }

        g_lastOk = false;
        g_lastReason = "not executed";
        g_lastVid = 0;
        ResetEvent(g_done);
        EnterCriticalSection(&g_lock);
        g_queue.push(cmd);
        LeaveCriticalSection(&g_lock);

        const bool waitReply = cmd.type == "get_phase" || cmd.type == "get_session" ||
            cmd.type == "login" || cmd.type == "select_character" || cmd.type == "enter_game" ||
            cmd.type == "skip_intro" || cmd.type == "get_atlas";
        DWORD wait = WAIT_OBJECT_0;
        if (waitReply)
        {
            const DWORD waitMs = (cmd.type == "enter_game") ? 15000 : 2000;
            wait = WaitForSingleObject(g_done, waitMs);
        }
        else if (g_lastReason.empty() || g_lastReason == "not executed")
        {
            g_lastOk = true;
            g_lastVid = cmd.vid;
            g_lastReason = "queued";
        }
        if (wait != WAIT_OBJECT_0 && (g_lastReason.empty() || g_lastReason == "not executed"))
        {
            char why[192];
            _snprintf_s(
                why,
                _TRUNCATE,
                "game-loop timeout iat=%d tid=%lu",
                g_iatHooks,
                g_loopTid);
            g_lastReason = why;
            g_lastOk = false;
        }

        char reason[192];
        char phase[64];
        char character[64];
        char account[64];
        char password[80];
        JsonEscape(g_lastReason.c_str(), reason, 192);
        JsonEscape(g_sessionPhase.c_str(), phase, 64);
        JsonEscape(g_sessionCharacter.c_str(), character, 64);
        JsonEscape(g_sessionAccount.c_str(), account, 64);
        JsonEscape(g_capturedPassword.c_str(), password, 80);
        char reply[1536];
        _snprintf_s(
            reply,
            _TRUNCATE,
            "{\"ok\":%s,\"type\":\"%s\",\"reason\":\"%s\",\"targetVid\":%u,\"convention\":\"loop-iat\",\"level\":%d,\"exp\":%d,\"nextExp\":%d,\"hp\":%d,\"maxHp\":%d,\"mp\":%d,\"maxMp\":%d,\"atlasWidth\":%u,\"atlasHeight\":%u,\"worldW\":%.1f,\"worldH\":%.1f,\"phase\":\"%s\",\"character\":\"%s\",\"account\":\"%s\",\"password\":\"%s\",\"characterSlot\":%d,\"startGame\":%s,\"inWorld\":%s}",
            g_lastOk ? "true" : "false",
            cmd.type.c_str(),
            reason,
            g_lastVid,
            g_hudLevel,
            g_hudExp,
            g_hudNextExp,
            g_hudHp,
            g_hudMaxHp,
            g_hudMp,
            g_hudMaxMp,
            g_atlasWidth,
            g_atlasHeight,
            g_atlasWorldW,
            g_atlasWorldH,
            phase,
            character,
            account,
            password,
            g_sessionCharacterSlot,
            g_startGame ? "true" : "false",
            g_inWorld ? "true" : "false");
        if (!WriteFrame(reply))
            break;
    }
    }
}

static DWORD WINAPI InitThread(void*)
{
    LoadConfig();
    HMODULE client = FindClient();
    g_base = (BYTE*)client;
    InitAtlasMap();
    InstallLoopHooks(client);
    NativeLog("loaded base=%p setTarget=0x%X attackKey=0x%X combo=0x%X reserve=0x%X pressActor=0x%X chase=0x%X moveTo=0x%X inRange=0x%X update=0x%X hooks=%d comboPatch=%d",
        g_base, kSetTargetRva, kAttackKeyRva, kComboRva, kReserveClickRva, kPressActorRva, kChaseRva, kMoveToDestRva, kInRangeRva, kPlayerUpdateRva, g_iatHooks, g_updateComboCall ? 1 : 0);
    Serve();
    return 0;
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(module);
        InitializeCriticalSection(&g_lock);
        g_done = CreateEventA(nullptr, FALSE, FALSE, nullptr);
        CreateThread(nullptr, 0, InitThread, nullptr, 0, nullptr);
    }
    return TRUE;
}
