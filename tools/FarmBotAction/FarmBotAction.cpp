#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <d3d9.h>
#include <psapi.h>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstdarg>
#include <cctype>
#include <cstring>
#include <map>
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
static unsigned kSendItemUseRva = 0x13A6F0;
static unsigned kGetItemIndexRva = 0;
static unsigned kGetItemIndexSlot = 0;
static unsigned kGetItemCountRva = 0;
static unsigned kGetItemCountSlot = 0;
static unsigned kSelectItemRva = 0;
static unsigned kItemMgrRva = 0;
static unsigned kGetSelectedRva = 0;
static unsigned kGetItemNameRva = 0;
static unsigned kGetItemTypeRva = 0;
static unsigned kGetItemSubTypeRva = 0;
static unsigned kGetAffectRva = 0;
static unsigned kGetValueRva = 0;
static unsigned kGetIconImageRva = 0;
static unsigned kPackMgrRva = 0;
static unsigned kPackGetRva = 0;
static unsigned kPackCtorRva = 0;
static unsigned kPackExistRva = 0;
static unsigned kResMgrRva = 0;
static unsigned kGetResRva = 0;
static unsigned kUseItemRva = 0;
static unsigned kGetSkillIndexRva = 0;
static unsigned kGetSkillLevelRva = 0;
static unsigned kClickSkillRva = 0;
static unsigned kGetSkillCoolRva = 0;
static unsigned kIsSkillCoolRva = 0;
static unsigned kIsSkillActiveRva = 0;
static unsigned kSendUseSkillRva = 0;
static unsigned kSkillMgrRva = 0;
static unsigned kGetSkillDataRva = 0;
static unsigned kIsStandingRva = 0;
static unsigned kIsToggleRva = 0;
static unsigned kIsNeedTargetRva = 0;
static unsigned kIsAttackSkillRva = 0;
static unsigned kIsMountingRva = 0;
static unsigned kIsAffectRva = 0;
static unsigned kSendChatRva = 0;
static unsigned kSendClickRva = 0xCE930;
static unsigned kItemSingletonRva = 0x482CAC;
static unsigned kGetCloseItemRva = 0xBB480;
static unsigned kGetCloseMoneyRva = 0xBB580;
static unsigned kTreeIncRva = 0xBA810;
static unsigned kMoveToDestRva = 0xA6BB0;
static unsigned kMoveToPixelRva = 0;
static unsigned kGetMainActorRva = 0;
static unsigned kPressActorRva = 0;
static unsigned kMousePressRva = 0;
static unsigned kChaseRva = 0;
static unsigned kApproachInstRva = 0;
static unsigned kAttackDestRva = 0;
static unsigned kInstAttackRva = 0;
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
static unsigned kPixelZDisp = 0x474;
static unsigned kActorTypeDisp = 0;
static unsigned kRaceDisp = 0;
static unsigned kVnumDisp = 0;
static unsigned kInstNameDisp = 0;
static unsigned kSelfVidDisp = 0;
static int kNegateY = 1;
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
#pragma pack(push, 1)
struct ItemPos { unsigned char window; unsigned short cell; };
#pragma pack(pop)
typedef int (__thiscall* FnSendItemUse)(void* self, unsigned posPacked, unsigned extra);
typedef void (__thiscall* FnSendClick)(void* self, unsigned vid);
typedef void* (__thiscall* FnTreeInc)(void* it);
typedef int (__thiscall* FnGetCloseItem)(void* self, float* pos, unsigned* vid, unsigned dist);
typedef int (__thiscall* FnMoveToDest)(void* inst, const void* dest);
typedef void (__thiscall* FnGetPixel)(void* inst, float* out);
typedef int (__thiscall* FnInRange)(void* me, const void* you);
typedef void* (__thiscall* FnGetMain)(void* self);
typedef void (__thiscall* FnAttackKey)(void* self, bool press);
typedef void (__thiscall* FnPressActor)(void* self, void* inst, unsigned vid, int isAuto);
typedef void (__thiscall* FnMousePress)(void* self, void* inst, int isAuto);
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
    int channelIndex;
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
static std::string g_potsJson = "[]";
static std::string g_skillsJson = "[]";
static std::string g_actorsJson = "[]";
static std::string g_affectsJson = "[]";
static std::string g_channelsJson = "[]";
static std::string g_charactersJson = "[]";
static std::string g_resolvedHost;
static std::string g_resolvedAuthHost;
static int g_resolvedPort;
static int g_resolvedAuthPort;
static int g_mounted;
static int g_greenRemain;
static int g_purpleRemain;
static unsigned g_lastVid;
static unsigned g_chaseVid;
static unsigned g_clickVid;
static unsigned g_walkVid;
static float g_chaseX;
static float g_chaseY;
static DWORD g_lastWalkMs;
static DWORD g_lastAtkMs;
static DWORD g_lastChaseMs;
static DWORD g_sidestepUntil;
static float g_stuckX;
static float g_stuckY;
static DWORD g_stuckAt;
static int g_nativeSidesteps;
static float g_sidestepFromX;
static float g_sidestepFromY;
static DWORD g_faceUntil;
static unsigned g_faceVid;
static float g_faceFromX;
static float g_faceFromY;
static DWORD g_lastFaceMs;
static unsigned g_meleeVid;
static unsigned g_meleeEngageVid;
static DWORD g_lastSkillsMs;
static int g_skillsReady;
static void* g_mainInst;
static char g_mainName[64];
static volatile long g_allowMelee;
static int g_captchaShown;
static int g_captchaHooked;
static DWORD g_captchaInstallAtMs;
static volatile long g_captchaBusy;
static volatile long g_captchaDumpBusy;
static volatile long g_captchaDumpStep;
static DWORD g_captchaDumpLastMs;
static int g_captchaDumpGot;
static char g_dumpVirt[260];
static char g_dumpDest[260];
static int g_dumpCaptured;
static BYTE* g_getTramp;
static BYTE g_getStolen[8];
static DWORD g_getProt;
static volatile long g_getHooked;
static DWORD g_scanUnstableMs;
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
static int g_hudRace = -1;
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
static char g_pyPhaseWindow[48];
static DWORD g_pyPhaseWindowMs;
static void* g_cachedMainStream;
static DWORD g_cachedMainStreamMs;
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
static int g_baerimAutoOff;
static int g_selectSent;
static DWORD g_selectSentAtMs;
static int g_layoutRecovered;
static int g_arthion;
static int g_baerim;
static DWORD g_baerimWorldAtMs;
static int NativeFarm() { return g_arthion || g_baerim; }
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

static HANDLE g_logFile = INVALID_HANDLE_VALUE;
static long g_logBusy = 0;

static void NativeLog(const char* fmt, ...)
{
    if (InterlockedCompareExchange(&g_logBusy, 1, 0) != 0)
        return;
    char body[400];
    va_list ap;
    va_start(ap, fmt);
    _vsnprintf_s(body, _TRUNCATE, fmt, ap);
    va_end(ap);
    SYSTEMTIME st;
    GetLocalTime(&st);
    char line[480];
    const int n = _snprintf_s(line, _TRUNCATE, "%02u:%02u:%02u.%03u %s\r\n",
        st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, body);
    if (g_logFile == INVALID_HANDLE_VALUE)
    {
        CreateDirectoryA("C:\\Temp", nullptr);
        g_logFile = CreateFileA("C:\\Temp\\farmbot-native.log",
            FILE_APPEND_DATA, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr,
            OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
        if (g_logFile != INVALID_HANDLE_VALUE)
        {
            LARGE_INTEGER size = {};
            if (GetFileSizeEx(g_logFile, &size) && size.QuadPart > 1500 * 1024)
            {
                CloseHandle(g_logFile);
                g_logFile = CreateFileA("C:\\Temp\\farmbot-native.log",
                    GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr,
                    CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
            }
        }
    }
    if (g_logFile != INVALID_HANDLE_VALUE && n > 0)
    {
        DWORD wrote = 0;
        WriteFile(g_logFile, line, (DWORD)n, &wrote, nullptr);
    }
    InterlockedExchange(&g_logBusy, 0);
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
        {
            slot = (int)(kGetStatusSlot / 4);
            g_getStatusSlot = slot;
        }
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

static void* CallGetMainActor();
static int CallIsMounted();

static int ReadInstanceRace(void* inst)
{
    if (!UserPtr(inst) || !kRaceDisp)
        return -1;
    unsigned race = 0xFFFFFFFFu;
    __try { race = *(unsigned*)((char*)inst + kRaceDisp); }
    __except (EXCEPTION_EXECUTE_HANDLER) { return -1; }
    return race <= 8 ? (int)race : -1;
}

static void RefreshHud(void* player)
{
    if (!player)
    {
        g_hudOk = false;
        g_hudRace = -1;
        g_mounted = 0;
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
    g_hudRace = ReadInstanceRace(CallGetMainActor());
    g_mounted = CallIsMounted();
}

static bool CallSetTarget(void* player, unsigned vid, int force)
{
    if (g_baerim)
        return false;
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

static unsigned LiveMapDisp();
static unsigned AliveMapDisp();

static void* MapFindVid(unsigned vid)
{
    void* mgr = CharMgr();
    const unsigned mapDisp = AliveMapDisp();
    if (!mgr || !vid || !mapDisp)
        return nullptr;
    __try
    {
        void** begin = *(void***)((char*)mgr + mapDisp);
        void** endp = *(void***)((char*)mgr + mapDisp + 4);
        if (UserPtr(begin) && UserPtr(endp) && endp > begin)
        {
            const unsigned count = (unsigned)(endp - begin);
            if (count >= 1 && count <= 1024)
                return nullptr;
        }
        void* head = *(void**)((char*)mgr + mapDisp + kMapHeadOff);
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

static unsigned LiveMapDisp()
{
    if (!g_baerim)
        return kMapDisp;
    if (kMapDisp >= 0x40)
        return kMapDisp;
    return 0x84;
}

static unsigned AliveMapDisp()
{
    if (g_baerim)
        return 0x14;
    return kMapDisp;
}

static unsigned ActorVecDisp()
{
    return g_baerim ? 0x84 : 0;
}

static bool LooksInstance(void* inst)
{
    if (!UserPtr(inst))
        return false;
    __try
    {
        void* vt = *(void**)inst;
        return UserPtr(vt);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static void* VectorFindVid(unsigned vid)
{
    void* mgr = CharMgr();
    const unsigned mapDisp = LiveMapDisp();
    if (!mgr || !vid || !mapDisp)
        return nullptr;
    __try
    {
        void** begin = *(void***)((char*)mgr + mapDisp);
        void** endp = *(void***)((char*)mgr + mapDisp + 4);
        if (!UserPtr(begin) || !UserPtr(endp) || endp < begin)
            return nullptr;
        unsigned count = (unsigned)(endp - begin);
        if (count > 1024)
            count = 1024;
        const unsigned step = g_baerim ? 2u : 1u;
        for (unsigned i = 0; i + (step - 1) < count; i += step)
        {
            void* inst = begin[i];
            unsigned got = 0;
            if (step == 2)
            {
                got = (unsigned)(uintptr_t)begin[i];
                inst = begin[i + 1];
                if (got == vid && LooksInstance(inst))
                    return inst;
                continue;
            }
            if (!LooksInstance(inst))
                continue;
            if (kSelfVidDisp)
                got = *(unsigned*)((char*)inst + kSelfVidDisp);
            if (got == vid)
                return inst;
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    return nullptr;
}

static void FillPlayerSelves(void* player, void** selves)
{
    if (g_baerim)
    {
        selves[0] = (char*)player + kThisAdj;
        selves[1] = player;
    }
    else
    {
        selves[0] = player;
        selves[1] = (char*)player + kThisAdj;
    }
}

static void* CallGetInstance(unsigned vid)
{
    if (!vid)
        return nullptr;
    void* mgr = CharMgr();
    if (mgr)
    {
        const unsigned adjs[2] = { kThisAdj, 0 };
        for (int a = 0; a < 2; ++a)
        {
            void* iabs = (char*)mgr + adjs[a];
            __try
            {
                void** vtbl = *(void***)iabs;
                if (!UserPtr(vtbl) || !UserPtr(vtbl[kGetInstSlot / 4]))
                    continue;
                void* inst = ((FnGetInst)vtbl[kGetInstSlot / 4])(iabs, vid);
                if (LooksInstance(inst))
                    return inst;
            }
            __except (EXCEPTION_EXECUTE_HANDLER)
            {
            }
        }
    }
    if (!g_baerim && NativeFarm())
    {
        void* fromMap = MapFindVid(vid);
        if (LooksInstance(fromMap))
            return fromMap;
    }
    return g_baerim ? VectorFindVid(vid) : nullptr;
}

static bool CallRva(unsigned rva, void* player, unsigned vid)
{
    if (!g_base || !player || !rva)
        return false;
    FnVid fn = (FnVid)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    void* selves[2];
    FillPlayerSelves(player, selves);
    for (int i = 0; i < 2; ++i)
    {
        if (!selves[i] || (i == 1 && selves[1] == selves[0]))
            continue;
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

static void JsonEscape(const char* in, char* out, int cap);
static bool CallSendPickup(unsigned vid);
static bool CallSendClick(void* player, unsigned vid);
static bool CallSendState(float x, float y, unsigned func);
static bool ReadMainPixel(float* x, float* y);
static bool ReadPixel(void* inst, float* x, float* y);
static void* MainInstance();
static void* CallGetMainActor();
static bool CallAttackKey(void* player, bool press);
static bool CallPressActor(void* player, unsigned vid);
static int SafeReserveClick(void* player, unsigned vid);
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
static int ComputeInWorld();
static int SafePyRun(const char* code);
static int SafePyRunAfterPeek(const char* code);
static int SafePyProbePhaseWindow();
static int PyPhaseWindowIs(const char* name);
static void* SafePyGetAttr(void* obj, const char* name);
static int EnsurePythonGil(int* outState);
static void ReleasePythonGil(int took, int state);
static int PyCanRun();
static void PyDec(void* obj);
static HMODULE PythonDll();
static int SafePyTypeName(void* obj, char* out, int cap);
static std::string JsonString(const std::string& src, const char* key);
static unsigned JsonU32(const std::string& src, const char* key);

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
           PhaseHas("Load") || PhaseHas("Game") ||
           WindowClassIs("LoginWindow") || WindowClassIs("SelectCharacterWindow");
}

static void MarkBaerimInGame(const char* why)
{
    g_sessionPhase = "Game";
    g_openedGameWindow = 1;
    g_inWorld = 1;
    g_loadFinished = 1;
    lstrcpynA(g_pyPhaseWindow, "GameWindow", (int)sizeof(g_pyPhaseWindow));
    if (why && why[0])
        NativeLog("baerim in-game: %s", why);
}

static void InferBaerimPhaseFromWindow()
{
    if (!g_baerim)
        return;
    // Once Game is confirmed, never touch Python phase windows again.
    if (g_inWorld || g_openedGameWindow || PyPhaseWindowIs("GameWindow"))
    {
        MarkBaerimInGame(nullptr);
        return;
    }
    // Fast path after StartGame: character name means the world stream is live.
    if (g_selectSent && !g_sessionCharacter.empty())
    {
        char why[96];
        _snprintf_s(why, _TRUNCATE, "character=%s after select", g_sessionCharacter.c_str());
        MarkBaerimInGame(why);
        return;
    }
    // Or a short grace after select even before the name string shows up.
    if (g_selectSent && g_selectSentAtMs && GetTickCount() - g_selectSentAtMs >= 1200)
    {
        MarkBaerimInGame("1.2s after select StartGame");
        return;
    }

    static DWORD s_probeMs;
    const DWORD now = GetTickCount();
    // At most one probe / 750ms. Never probe every RefreshSession tick (that crashed).
    if (!s_probeMs || now - s_probeMs >= 750)
    {
        s_probeMs = now;
        SafePyProbePhaseWindow();
    }

    if (PyPhaseWindowIs("GameWindow"))
    {
        MarkBaerimInGame("GameWindow");
        return;
    }
    if (PyPhaseWindowIs("LoadingWindow"))
    {
        g_sessionPhase = "Load";
        return;
    }
    if (PyPhaseWindowIs("SelectCharacterWindow"))
    {
        g_sessionPhase = "Select";
        return;
    }
    if (PyPhaseWindowIs("LoginWindow"))
    {
        g_sessionPhase = "Login";
        return;
    }
    if ((PhaseHas("Off") || PhaseHas("Hand") || g_sessionPhase.empty()) &&
        g_channelsJson.size() > 2)
        g_sessionPhase = "Login";
}

static bool IsLoopbackHost(const char* host)
{
    if (!host || !host[0])
        return true;
    return _stricmp(host, "127.0.0.1") == 0 ||
           _stricmp(host, "localhost") == 0 ||
           _stricmp(host, "::1") == 0;
}

static std::string ReadTempUtf8(const char* fileName)
{
    char path[MAX_PATH];
    if (!GetTempPathA(MAX_PATH, path))
        return "";
    char full[MAX_PATH];
    _snprintf_s(full, _TRUNCATE, "%s%s", path, fileName);
    FILE* f = nullptr;
    if (fopen_s(&f, full, "rb") != 0 || !f)
        return "";
    std::string out;
    char buf[1024];
    size_t n = 0;
    while ((n = fread(buf, 1, sizeof(buf), f)) > 0)
        out.append(buf, n);
    fclose(f);
    return out;
}

static int EscapePyPath(const char* full, char* pyPath, int cap)
{
    if (!full || !pyPath || cap < 4)
        return 0;
    int j = 0;
    for (int i = 0; full[i] && j < cap - 2; ++i)
    {
        if (full[i] == '\\')
            pyPath[j++] = '\\';
        pyPath[j++] = full[i];
    }
    pyPath[j] = 0;
    return j > 0 ? 1 : 0;
}

static int SafePyDumpChannels()
{
    // Cache — get_phase used to re-dump on every tick and abort python27.
    static DWORD s_lastDumpMs;
    const DWORD now = GetTickCount();
    if (g_channelsJson.size() > 2 && s_lastDumpMs && now - s_lastDumpMs < 5000)
        return 1;
    char path[MAX_PATH];
    if (!GetTempPathA(MAX_PATH, path))
        return 0;
    char full[MAX_PATH];
    _snprintf_s(full, _TRUNCATE, "%sfarmbot-channels.json", path);
    DeleteFileA(full);
    char errFull[MAX_PATH];
    _snprintf_s(errFull, _TRUNCATE, "%sfarmbot-channels.err", path);
    DeleteFileA(errFull);
    char pyPath[MAX_PATH * 2];
    char pyErr[MAX_PATH * 2];
    if (!EscapePyPath(full, pyPath, (int)sizeof(pyPath)) ||
        !EscapePyPath(errFull, pyErr, (int)sizeof(pyErr)))
        return 0;
    // Baerim: auth is not nested under REGION_DICT channel entries. Auth is the
    // first authlist host (firewall.baerim.eu) on port 30001. Prefer live attrs
    // (AUTH_PORT / AUTH_SERVER / authlist) then fall back to 30001.
    char code[6500];
    _snprintf_s(
        code,
        _TRUNCATE,
        "import json\n"
        "out=[];err=''\n"
        "def _auth_pair(auth):\n"
        " if not isinstance(auth,dict) or not auth: return '',0\n"
        " if ('ip' in auth) or ('host' in auth) or ('port' in auth) or ('tcp_port' in auth):\n"
        "  a=auth\n"
        " else:\n"
        "  ak=sorted(auth.keys())[0]\n"
        "  a=auth.get(ak) if isinstance(auth.get(ak),dict) else {}\n"
        " if not isinstance(a,dict): return '',0\n"
        " return (a.get('ip') or a.get('host') or ''), int(a.get('port') or a.get('tcp_port') or 0)\n"
        "def _pick_region(d):\n"
        " if not isinstance(d,dict) or not d: return {}\n"
        " for k in (0,'0',1,'1'):\n"
        "  if k in d and isinstance(d[k],dict): return d[k]\n"
        " k=sorted(d.keys())[0]\n"
        " return d[k] if isinstance(d.get(k),dict) else {}\n"
        "try:\n"
        " import serverInfo as si\n"
        " rd=getattr(si,'REGION_DICT',None) or getattr(si,'SERVER_DICT',None) or {}\n"
        " ra=getattr(si,'REGION_AUTH_SERVER_DICT',None) or getattr(si,'AUTH_SERVER_DICT',None) or {}\n"
        " r=_pick_region(rd)\n"
        " ra0=_pick_region(ra) if isinstance(ra,dict) else {}\n"
        " def_auth_host='';def_auth_port=0\n"
        " for name in ('AUTH_PORT','ACCOUNT_PORT','LOGIN_PORT','auth_port'):\n"
        "  v=getattr(si,name,None)\n"
        "  if v is not None:\n"
        "   try: def_auth_port=int(v); break\n"
        "   except: pass\n"
        " for name in ('AUTH_SERVER','ACCOUNT_SERVER','SERVER_IP','SERVER_IP_PROD'):\n"
        "  v=getattr(si,name,None)\n"
        "  if isinstance(v,str) and v and v not in ('127.0.0.1','localhost'):\n"
        "   def_auth_host=v; break\n"
        " al=getattr(si,'authlist',None) or getattr(si,'AUTH_LIST',None) or getattr(si,'AUTHLIST',None)\n"
        " if isinstance(al,(list,tuple)) and al:\n"
        "  for item in al:\n"
        "   if isinstance(item,str) and item and item not in ('127.0.0.1','localhost'):\n"
        "    def_auth_host=item; break\n"
        "   if isinstance(item,dict):\n"
        "    h=item.get('ip') or item.get('host') or ''\n"
        "    p=int(item.get('port') or item.get('tcp_port') or 0)\n"
        "    if h and h not in ('127.0.0.1','localhost'):\n"
        "     def_auth_host=h\n"
        "     if p>0: def_auth_port=p\n"
        "     break\n"
        " if def_auth_port<=0: def_auth_port=30001\n"
        " for sk in sorted(r.keys()):\n"
        "  s=r[sk]\n"
        "  if not isinstance(s,dict): continue\n"
        "  ch=s.get('channel') or s.get('channels') or {}\n"
        "  if not isinstance(ch,dict): ch={}\n"
        "  auth=s.get('auth') or s.get('account') or {}\n"
        "  if (not auth) and isinstance(ra0,dict):\n"
        "   auth=ra0.get(sk) or ra0.get(str(sk)) or {}\n"
        "  aip,aport=_auth_pair(auth if isinstance(auth,dict) else {})\n"
        "  if not aip: aip=def_auth_host\n"
        "  if aport<=0: aport=def_auth_port\n"
        "  sip=(s.get('ip') or s.get('host') or '')\n"
        "  for ck in sorted(ch.keys()):\n"
        "   c=ch[ck]\n"
        "   if not isinstance(c,dict): continue\n"
        "   ip=(c.get('ip') or c.get('host') or sip or aip or '')\n"
        "   port=int(c.get('tcp_port') or c.get('port') or 0)\n"
        "   name=(c.get('name') or ('CH-'+str(ck)))\n"
        "   ah=(aip or ip)\n"
        "   ap=aport if aport>0 else def_auth_port\n"
        "   if not ip or port<=0: continue\n"
        "   if ip in ('127.0.0.1','localhost','::1'): continue\n"
        "   if ap==port: ap=def_auth_port\n"
        "   out.append({'index':len(out),'name':str(name),'host':str(ip),'port':port,'authHost':str(ah),'authPort':int(ap)})\n"
        "except Exception as e:\n"
        " err=str(e)\n"
        "try:\n"
        " open(r'%s','wb').write(json.dumps(out))\n"
        "except Exception as e2:\n"
        " err=(err+'; '+str(e2)).strip()\n"
        "try:\n"
        " open(r'%s','wb').write(err or ('ok count=%%d'%%len(out)))\n"
        "except Exception:\n"
        " pass\n",
        pyPath,
        pyErr);
    if (!SafePyRunAfterPeek(code))
    {
        NativeLog("baerim channels: SafePyRunAfterPeek failed");
        g_channelsJson = "[]";
        return 0;
    }
    g_channelsJson = ReadTempUtf8("farmbot-channels.json");
    if (g_channelsJson.empty())
        g_channelsJson = "[]";
    s_lastDumpMs = now;
    const std::string dumpErr = ReadTempUtf8("farmbot-channels.err");
    if (!dumpErr.empty() && dumpErr.find("ok count=") == std::string::npos)
        NativeLog("baerim channels dump: %s json=%s", dumpErr.c_str(), g_channelsJson.c_str());
    else if (g_channelsJson.size() > 2)
        NativeLog("baerim channels ready: %s", dumpErr.empty() ? g_channelsJson.c_str() : dumpErr.c_str());
    return g_channelsJson.size() > 2 ? 1 : 0;
}

static int SafePyDumpCharacterNames()
{
    char path[MAX_PATH];
    if (!GetTempPathA(MAX_PATH, path))
        return 0;
    char full[MAX_PATH];
    _snprintf_s(full, _TRUNCATE, "%sfarmbot-characters.json", path);
    DeleteFileA(full);
    char pyPath[MAX_PATH * 2];
    int j = 0;
    for (int i = 0; full[i] && j < (int)sizeof(pyPath) - 2; ++i)
    {
        if (full[i] == '\\')
            pyPath[j++] = '\\';
        pyPath[j++] = full[i];
    }
    pyPath[j] = 0;
    char code[1600];
    _snprintf_s(
        code,
        _TRUNCATE,
        "import net,json\n"
        "names=[]\n"
        "for i in range(5):\n"
        " n=''\n"
        " try:\n"
        "  n=net.GetAccountCharacterSlotDataString(i,0) or ''\n"
        " except Exception:\n"
        "  try: n=net.GetAccountCharacterSlotData(i,0) or ''\n"
        "  except Exception: n=''\n"
        " if not isinstance(n,str): n=str(n) if n else ''\n"
        " names.append(n)\n"
        "open(r'%s','wb').write(json.dumps(names))\n",
        pyPath);
    if (!SafePyRunAfterPeek(code))
        return 0;
    g_charactersJson = ReadTempUtf8("farmbot-characters.json");
    if (g_charactersJson.empty())
        g_charactersJson = "[]";
    return g_charactersJson.size() > 2 ? 1 : 0;
}

static bool JsonPickChannel(const std::string& json, int channelIndex, int preferPort,
    std::string* host, int* port, std::string* authHost, int* authPort)
{
    if (!host || !port || !authHost || !authPort || json.size() < 2 || json[0] != '[')
        return false;
    // Prefer matching channelIndex, else preferPort, else first non-loopback.
    int bestScore = -1;
    std::string bestHost, bestAuth;
    int bestPort = 0, bestAuthPort = 0;
    size_t i = 0;
    int seen = 0;
    while (i < json.size())
    {
        size_t obj = json.find('{', i);
        if (obj == std::string::npos)
            break;
        size_t end = json.find('}', obj);
        if (end == std::string::npos)
            break;
        std::string piece = json.substr(obj, end - obj + 1);
        const int idx = (int)JsonU32(piece, "index");
        const int p = (int)JsonU32(piece, "port");
        const int ap = (int)JsonU32(piece, "authPort");
        std::string h = JsonString(piece, "host");
        std::string ah = JsonString(piece, "authHost");
        if (ah.empty())
            ah = h;
        int score = 0;
        if (channelIndex >= 0 && idx == channelIndex)
            score = 300;
        else if (preferPort > 0 && p == preferPort)
            score = 200;
        else if (seen == 0)
            score = 50;
        if (!IsLoopbackHost(h.c_str()))
            score += 20;
        else
            score -= 100;
        if (score > bestScore && !h.empty() && p > 0)
        {
            bestScore = score;
            bestHost = h;
            bestAuth = ah;
            bestPort = p;
            bestAuthPort = ap > 0 ? ap : p;
        }
        ++seen;
        i = end + 1;
    }
    if (bestScore < 0 || IsLoopbackHost(bestHost.c_str()))
        return false;
    *host = bestHost;
    *port = bestPort;
    *authHost = bestAuth.empty() ? bestHost : bestAuth;
    // Baerim: never use the channel TCP port as auth — auth is 30001.
    if (bestAuthPort <= 0 || bestAuthPort == bestPort)
        bestAuthPort = g_baerim ? 30001 : bestPort;
    *authPort = bestAuthPort;
    return true;
}

static bool ResolveBaerimLoginHosts(Command* cmd)
{
    if (!cmd)
        return false;
    SafePyDumpChannels();
    std::string host, authHost;
    int port = 0, authPort = 0;
    if (!JsonPickChannel(g_channelsJson, cmd->channelIndex, cmd->port, &host, &port, &authHost, &authPort))
    {
        NativeLog("baerim login: no live channel from serverInfo (channels=%s)",
            g_channelsJson.c_str());
        return false;
    }
    cmd->host = host;
    cmd->port = port;
    cmd->authHost = authHost;
    cmd->authPort = authPort;
    g_resolvedHost = host;
    g_resolvedPort = port;
    g_resolvedAuthHost = authHost;
    g_resolvedAuthPort = authPort;
    NativeLog("baerim login resolved host=%s:%d auth=%s:%d chIdx=%d",
        host.c_str(), port, authHost.c_str(), authPort, cmd->channelIndex);
    return true;
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
    void* player = Player();
    char name[64];
    if (player && ReadPlayerName(player, name, 64) && name[0])
        g_sessionCharacter = name;
    // Read character before phase inference so select→Game can flip immediately.
    InferBaerimPhaseFromWindow();
    if (g_openedGameWindow || g_inWorld)
    {
        g_sessionPhase = "Game";
        g_openedGameWindow = 1;
        g_inWorld = 1;
        g_startGame = 1;
        return; // no WindowClassIs / LoadingStepCount / char dumps in-world
    }
    ReadLoginSession();
    if (g_baerim && (PhaseHas("Select") || PyPhaseWindowIs("SelectCharacterWindow")))
    {
        static DWORD s_lastCharDumpMs;
        const DWORD now = GetTickCount();
        if (!s_lastCharDumpMs || now - s_lastCharDumpMs > 2000)
        {
            s_lastCharDumpMs = now;
            SafePyDumpCharacterNames();
        }
    }
    // Baerim: do not run LoadingStepCount/WindowClassIs — GIL from PeekMessage crashed.
    g_startGame = g_loadFinished || g_openedGameWindow;
    g_inWorld = ComputeInWorld();
    if (g_inWorld)
    {
        g_sessionPhase = "Game";
        g_openedGameWindow = 1;
    }
}

static int ComputeInWorld()
{
    // Baerim often leaves native phase on HandShake/Load while GameWindow is up.
    if (g_openedGameWindow || PyPhaseWindowIs("GameWindow"))
        return 1;
    if (g_selectSent && !g_sessionCharacter.empty() &&
        !PyPhaseWindowIs("LoginWindow") && !PyPhaseWindowIs("SelectCharacterWindow"))
        return 1;
    if (PyPhaseWindowIs("LoginWindow") || PyPhaseWindowIs("SelectCharacterWindow"))
        return 0;
    // Sticky "Load" from a stale LoadingWindow cache must not block entities.
    if (PhaseHas("Login") || PhaseHas("Select") ||
        PhaseHas("Logo") || PhaseHas("Intro") || PhaseHas("Cine") || PhaseHas("EMPIRE"))
        return 0;
    if (PhaseHas("Game") && (HasGameWindow() ||
        (g_hudOk && g_hudMaxHp > 0 && g_hudLevel >= 1 && g_hudLevel <= 250)))
        return 1;
    if (g_hudOk && g_hudMaxHp > 0 && g_hudLevel >= 1 && g_hudLevel <= 250 &&
        !g_sessionCharacter.empty())
        return 1;
    if (g_inWorld)
        return 1;
    return 0;
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
    // Cache the live MainStream — rescanning offsets + GetAttr every PeekMessage tick
    // is what made Baerim laggy/crashy after select.
    if (g_cachedMainStream && UserPtr(g_cachedMainStream))
    {
        const DWORD now = GetTickCount();
        if (g_inWorld || g_openedGameWindow || now - g_cachedMainStreamMs < 3000)
            return g_cachedMainStream;
    }
    // Baerim MainStream is at +0x178; stock Metin2 uses +0x160.
    static const unsigned kHandlerCandidates[] = {
        0x178, kPythonHandlerDisp, 0x160, 0x174, 0x17C, 0x170, 0x168, 0x15C, 0x164, 0x14C, 0x148, 0x158
    };
    void* bestMain = nullptr;
    void* stock = nullptr;
    for (unsigned i = 0; i < sizeof(kHandlerCandidates) / sizeof(kHandlerCandidates[0]); ++i)
    {
        const unsigned disp = kHandlerCandidates[i];
        void* handler = nullptr;
        __try
        {
            handler = *(void**)((char*)net + disp);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            handler = nullptr;
        }
        if (!UserPtr(handler))
            continue;
        if (!PyCanRun())
        {
            if ((disp == kPythonHandlerDisp || disp == 0x160) && !stock)
                stock = handler;
            continue;
        }
        // MainStream has SetSelectCharacterPhase / isAutoSelect. Prefer that over a random ptr
        // that merely has some curPhaseWindow-like slot (integer 0 still GetAttr-succeeds).
        void* setSel = SafePyGetAttr(handler, "SetSelectCharacterPhase");
        void* isAuto = SafePyGetAttr(handler, "isAutoSelect");
        void* wnd = SafePyGetAttr(handler, "curPhaseWindow");
        const int isMain = (setSel != nullptr) || (isAuto != nullptr);
        if (setSel)
            PyDec(setSel);
        if (isAuto)
            PyDec(isAuto);
        if (isMain && !bestMain)
            bestMain = handler;
        if (wnd)
        {
            char cls[48];
            cls[0] = 0;
            if (SafePyTypeName(wnd, cls, 48) && cls[0] &&
                (strstr(cls, "Select") || strstr(cls, "Login") || strstr(cls, "Load") ||
                 strstr(cls, "Game") || strstr(cls, "Logo") || strstr(cls, "Empire")))
            {
                PyDec(wnd);
                g_cachedMainStream = handler;
                g_cachedMainStreamMs = GetTickCount();
                return handler;
            }
            PyDec(wnd);
        }
        else
        {
            FnPyErrClear errClear = nullptr;
            HMODULE py = PythonDll();
            errClear = py ? (FnPyErrClear)GetProcAddress(py, "PyErr_Clear") : nullptr;
            if (errClear)
                errClear();
        }
        if ((disp == kPythonHandlerDisp || disp == 0x160) && !stock)
            stock = handler;
    }
    // Prefer a verified MainStream (has isAutoSelect / SetSelectCharacterPhase).
    // Bare +0x160 without those attrs is often wrong on Baerim and breaks select.
    void* found = bestMain ? bestMain : stock;
    if (found && bestMain)
    {
        g_cachedMainStream = found;
        g_cachedMainStreamMs = GetTickCount();
    }
    else if (found && !bestMain)
    {
        // Only keep unverified stock briefly — do not cache it.
        g_cachedMainStream = nullptr;
        g_cachedMainStreamMs = 0;
    }
    return found;
}

static HMODULE PythonDll()
{
    HMODULE py = GetModuleHandleA("python27.dll");
    if (!py)
        py = GetModuleHandleA("python26.dll");
    if (!py)
        py = GetModuleHandleA("python22.dll");
    if (!py)
        py = GetModuleHandleA("python37.dll");
    if (!py)
        py = GetModuleHandleA("python38.dll");
    if (!py)
        py = GetModuleHandleA("python39.dll");
    if (!py)
        py = GetModuleHandleA("python311.dll");
    if (!py)
    {
        HMODULE exe = GetModuleHandleA(nullptr);
        if (exe && (GetProcAddress(exe, "PyObject_CallMethod") || GetProcAddress(exe, "Py_Initialize")))
            py = exe;
    }
    return py;
}

static void* PyThreadStateCurrent()
{
    HMODULE py = PythonDll();
    if (!py)
        return nullptr;
    void* const* slot = (void* const*)GetProcAddress(py, "_PyThreadState_Current");
    if (!slot)
        return nullptr;
    void* tstate = nullptr;
    __try { tstate = *slot; }
    __except (EXCEPTION_EXECUTE_HANDLER) { tstate = nullptr; }
    return tstate;
}

static int PyCanRun()
{
    return PyThreadStateCurrent() != nullptr;
}

static void PyDec(void* obj)
{
    if (!obj || !PyCanRun())
        return;
    HMODULE py = PythonDll();
    typedef void (__cdecl* FnDec)(void*);
    FnDec dec = py ? (FnDec)GetProcAddress(py, "Py_DecRef") : nullptr;
    if (!dec)
        dec = py ? (FnDec)GetProcAddress(py, "Py_XDECREF") : nullptr;
    if (!dec)
        return;
    __try { dec(obj); }
    __except (EXCEPTION_EXECUTE_HANDLER) {}
}

static void* PyMod(const char* name)
{
    HMODULE py = PythonDll();
    if (!py || !name || !PyCanRun())
        return nullptr;
    typedef void* (__cdecl* FnImport)(const char*);
    FnImport imp = (FnImport)GetProcAddress(py, "PyImport_ImportModule");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!imp)
        return nullptr;
    __try
    {
        void* mod = imp(name);
        if (!mod && errClear)
            errClear();
        return mod;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return nullptr;
    }
}

static void* PyCall(void* obj, const char* name, const char* fmt, int a = 0, int b = 0)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !PyCanRun())
        return nullptr;
    FnPyCallMethod call = (FnPyCallMethod)GetProcAddress(py, "PyObject_CallMethod");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!call)
        return nullptr;
    char method[64];
    char format[8];
    lstrcpynA(method, name, 64);
    if (fmt && fmt[0])
        lstrcpynA(format, fmt, 8);
    else
        format[0] = 0;
    __try
    {
        void* result = nullptr;
        if (!format[0])
            result = call(obj, method, NULL);
        else if (format[1] == 0)
            result = call(obj, method, format, a);
        else
            result = call(obj, method, format, a, b);
        if (errClear)
            errClear();
        return result;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return nullptr;
    }
}

static long PyAsInt(void* obj)
{
    if (!obj)
        return 0;
    if (!PyCanRun())
    {
        PyDec(obj);
        return 0;
    }
    HMODULE py = PythonDll();
    typedef long (__cdecl* FnAsLong)(void*);
    FnAsLong asLong = py ? (FnAsLong)GetProcAddress(py, "PyInt_AsLong") : nullptr;
    if (!asLong)
        asLong = py ? (FnAsLong)GetProcAddress(py, "PyLong_AsLong") : nullptr;
    long v = 0;
    __try { v = asLong ? asLong(obj) : 0; }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    PyDec(obj);
    return v;
}

static void PyAsStr(void* obj, char* out, int cap)
{
    if (out && cap > 0)
        out[0] = 0;
    if (!obj || !out || cap < 2)
    {
        PyDec(obj);
        return;
    }
    if (!PyCanRun())
    {
        PyDec(obj);
        return;
    }
    HMODULE py = PythonDll();
    FnPyStringAsString asString = py ? (FnPyStringAsString)GetProcAddress(py, "PyString_AsString") : nullptr;
    typedef const char* (__cdecl* FnUtf8)(void*);
    FnUtf8 asUtf8 = py ? (FnUtf8)GetProcAddress(py, "PyUnicode_AsUTF8") : nullptr;
    __try
    {
        const char* text = asString ? asString(obj) : nullptr;
        if (!text && asUtf8)
            text = asUtf8(obj);
        if (text)
            lstrcpynA(out, text, cap);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    PyDec(obj);
}

static unsigned PyItemIndex(void* player, int slot)
{
    void* r = PyCall(player, "GetItemIndex", "i", slot);
    if (!r)
        r = PyCall(player, "GetItemIndex", "ii", 1, slot);
    long v = PyAsInt(r);
    return v > 0 ? (unsigned)v : 0;
}

static int PyItemCount(void* player, int slot)
{
    void* r = PyCall(player, "GetItemCount", "i", slot);
    if (!r)
        r = PyCall(player, "GetItemCount", "ii", 1, slot);
    long v = PyAsInt(r);
    return v > 0 ? (int)v : 1;
}

static void PySlotName(void* player, int slot, char* out, int cap)
{
    if (out && cap > 0)
        out[0] = 0;
    PyAsStr(PyCall(player, "GetItemName", "i", slot), out, cap);
    if (out && out[0])
        return;
    PyAsStr(PyCall(player, "GetItemName", "ii", 1, slot), out, cap);
}

static int GilEnter(int* state)
{
    if (state)
        *state = 0;
    return PyCanRun();
}

static void GilLeave(int have, int state)
{
    (void)have;
    (void)state;
}

static int ParseAffectPair(void* aff, int* apply, int* aval)
{
    if (!aff || !apply || !aval || !PyCanRun())
        return 0;
    HMODULE py = PythonDll();
    typedef int (__cdecl* FnParse)(void*, const char*, ...);
    FnParse parse = py ? (FnParse)GetProcAddress(py, "PyArg_ParseTuple") : nullptr;
    if (!parse)
        return 0;
    __try { return parse(aff, "ii", apply, aval) ? 1 : 0; }
    __except (EXCEPTION_EXECUTE_HANDLER) { return 0; }
}

static int SafePySetInt(void* obj, const char* name, long value)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !PyCanRun())
        return 0;
    FnPyIntFromLong fromLong = (FnPyIntFromLong)GetProcAddress(py, "PyInt_FromLong");
    if (!fromLong)
        fromLong = (FnPyIntFromLong)GetProcAddress(py, "PyLong_FromLong");
    typedef void* (__cdecl* FnPyBuildValue)(const char*, ...);
    FnPyBuildValue build = (FnPyBuildValue)GetProcAddress(py, "Py_BuildValue");
    FnPySetAttrString setAttr = (FnPySetAttrString)GetProcAddress(py, "PyObject_SetAttrString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!setAttr || (!fromLong && !build))
        return 0;
    __try
    {
        void* boxed = fromLong ? fromLong(value) : nullptr;
        if (!boxed && build)
            boxed = build("i", (int)value);
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
    if (!obj || !py || !name || !value || !PyCanRun())
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
    if (!obj || !py || !name || !PyCanRun())
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

static int SafePyCallMethodStr(void* obj, const char* name, const char* arg)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !arg || !PyCanRun())
        return 0;
    FnPyCallMethod call = (FnPyCallMethod)GetProcAddress(py, "PyObject_CallMethod");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!call)
        return 0;
    char method[64];
    lstrcpynA(method, name, 64);
    __try
    {
        void* result = call(obj, method, "s", arg);
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
    SafePySetInt(handler, "isAutoSelect", 0);
    SafePySetInt(handler, "isAutoLogin", 0);
    return SafePyCallMethod(handler, "SetCharacterSlot", "i", (int)slot);
}

static int SafePyPrepareLogin(const Command& cmd)
{
    int gilState = 0;
    const int took = EnsurePythonGil(&gilState);
    if (took < 0)
        return 0;
    void* handler = PythonHandler(NetStream());
    int ok = 0;
    if (handler)
    {
        const char* authHost = cmd.authHost.empty() ? cmd.host.c_str() : cmd.authHost.c_str();
        int authPort = cmd.authPort > 0 ? cmd.authPort : 0;
        if (g_baerim && (authPort <= 0 || authPort == cmd.port))
            authPort = 30001;
        if (authPort <= 0)
            authPort = cmd.port;
        SafePySetString(handler, "addr", cmd.host.c_str());
        SafePySetInt(handler, "port", cmd.port);
        SafePySetString(handler, "account_addr", authHost);
        SafePySetInt(handler, "account_port", authPort);
        SafePySetString(handler, "id", cmd.account.c_str());
        SafePySetString(handler, "pwd", cmd.password.c_str());
        // Baerim stock auto_login: isAutoSelect=1 so Open() presses Enter.
        SafePySetInt(handler, "isAutoLogin", 0);
        SafePySetInt(handler, "isAutoSelect", g_baerim ? 1 : 0);
        ok = SafePyCallMethod(handler, "SetCharacterSlot", "i", (int)cmd.slot);
    }
    ReleasePythonGil(took, gilState);
    return ok;
}

static int SafePyClickStart(void* net)
{
    void* handler = PythonHandler(net);
    if (!handler)
        return 0;
    void* wnd = SafePyGetAttr(handler, "curPhaseWindow");
    if (!wnd)
        return 0;
    return SafePyCallMethod(wnd, "StartGame", "", 0);
}

// PeekMessage hook often has no Python TLS — C API GetAttr/SetAttr/CallMethod no-op
// without GIL. Mirror WindowClassIs: Ensure GIL, then use MainStream @ net+0x160.
static int EnsurePythonGil(int* outState)
{
    if (outState)
        *outState = 0;
    if (PyCanRun())
        return 0; // already runnable; caller must not Release
    HMODULE py = PythonDll();
    FnPyGilEnsure gilEnsure = py ? (FnPyGilEnsure)GetProcAddress(py, "PyGILState_Ensure") : nullptr;
    FnPyGilRelease gilRelease = py ? (FnPyGilRelease)GetProcAddress(py, "PyGILState_Release") : nullptr;
    (void)gilRelease;
    if (!gilEnsure)
        return -1;
    int state = 0;
    __try
    {
        state = gilEnsure();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return -1;
    }
    if (!PyCanRun())
        return -1;
    if (outState)
        *outState = state;
    return 1; // took GIL; caller must Release
}

static void ReleasePythonGil(int took, int state)
{
    if (!took)
        return;
    HMODULE py = PythonDll();
    FnPyGilRelease gilRelease = py ? (FnPyGilRelease)GetProcAddress(py, "PyGILState_Release") : nullptr;
    if (!gilRelease)
        return;
    __try { gilRelease(state); }
    __except (EXCEPTION_EXECUTE_HANDLER) {}
}

// Baerim MainStream lives at net+0x178 (not stock 0x160). Prefer that, then a short scan.
static unsigned g_baerimHandlerDisp = 0x178;

static int TryStartGameOnHandler(void* handler, unsigned slot, char* clsOut, int clsCap)
{
    if (!handler)
        return 0;
    void* wnd = SafePyGetAttr(handler, "curPhaseWindow");
    if (!wnd)
        wnd = SafePyGetAttr(handler, "newPhaseWindow");
    if (!wnd)
        return 0;
    char cls[48];
    cls[0] = 0;
    SafePyTypeName(wnd, cls, 48);
    if (cls[0] && (_stricmp(cls, "int") == 0 || _stricmp(cls, "long") == 0 ||
                   _stricmp(cls, "NoneType") == 0))
    {
        PyDec(wnd);
        return 0;
    }
    if (cls[0] && !strstr(cls, "Select"))
    {
        PyDec(wnd);
        return 0;
    }
    SafePySetInt(handler, "isAutoSelect", 1);
    SafePyCallMethod(handler, "SetCharacterSlot", "i", (int)slot);
    SafePyCallMethod(wnd, "SelectSlot", "i", (int)slot);
    const int ok = SafePyCallMethod(wnd, "StartGame", "", 0) ||
                   SafePyCallMethod(wnd, "OnIMEReturn", "", 0);
    if (ok && clsOut && clsCap > 0)
        lstrcpynA(clsOut, cls[0] ? cls : "Select", clsCap);
    PyDec(wnd);
    return ok;
}

// Manual Enter = SelectCharacterWindow.StartGame.
static int SafePyBaerimSelectCharacter(unsigned slot)
{
    int gilState = 0;
    const int took = EnsurePythonGil(&gilState);
    if (took < 0)
    {
        NativeLog("baerim select: no GIL");
        return 0;
    }

    void* net = NetStream();
    int ok = 0;
    unsigned foundDisp = 0;
    char foundCls[48];
    foundCls[0] = 0;
    if (net)
    {
        // Known Baerim offset first (from successful select), then tiny fallback list.
        static const unsigned kDisp[] = { 0x178, 0x160, 0x174, 0x17C, 0x170, 0x168 };
        if (g_baerimHandlerDisp)
        {
            void* handler = nullptr;
            __try { handler = *(void**)((char*)net + g_baerimHandlerDisp); }
            __except (EXCEPTION_EXECUTE_HANDLER) { handler = nullptr; }
            if (UserPtr(handler) && TryStartGameOnHandler(handler, slot, foundCls, 48))
            {
                ok = 1;
                foundDisp = g_baerimHandlerDisp;
                g_cachedMainStream = handler;
                g_cachedMainStreamMs = GetTickCount();
            }
        }
        for (unsigned i = 0; !ok && i < sizeof(kDisp) / sizeof(kDisp[0]); ++i)
        {
            if (kDisp[i] == g_baerimHandlerDisp)
                continue;
            void* handler = nullptr;
            __try { handler = *(void**)((char*)net + kDisp[i]); }
            __except (EXCEPTION_EXECUTE_HANDLER) { handler = nullptr; }
            if (!UserPtr(handler))
                continue;
            if (!TryStartGameOnHandler(handler, slot, foundCls, 48))
                continue;
            ok = 1;
            foundDisp = kDisp[i];
            g_baerimHandlerDisp = foundDisp;
            g_cachedMainStream = handler;
            g_cachedMainStreamMs = GetTickCount();
        }
    }
    if (ok)
        NativeLog("baerim select StartGame disp=0x%X cls=%s slot=%u", foundDisp, foundCls, slot);
    else
        NativeLog("baerim select waiting for SelectCharacterWindow phase=%s",
            g_sessionPhase.empty() ? "?" : g_sessionPhase.c_str());
    ReleasePythonGil(took, gilState);
    return ok;
}

// Stock introLogin auto_login: isAutoSelect=1 + SetCharacterSlot before select Open().
static int SafePyBaerimArmAutoSelect(int slot)
{
    int gilState = 0;
    const int took = EnsurePythonGil(&gilState);
    if (took < 0)
        return 0;
    void* net = NetStream();
    int ok = 0;
    unsigned foundDisp = 0;
    static const unsigned kDisp[] = { 0x178, 0x160, 0x174, 0x17C, 0x170, 0x168 };
    if (net)
    {
        for (unsigned i = 0; i < sizeof(kDisp) / sizeof(kDisp[0]) && !ok; ++i)
        {
            const unsigned disp = kDisp[i];
            void* handler = nullptr;
            __try { handler = *(void**)((char*)net + disp); }
            __except (EXCEPTION_EXECUTE_HANDLER) { handler = nullptr; }
            if (!UserPtr(handler))
                continue;
            if (!SafePySetInt(handler, "isAutoSelect", 1))
                continue;
            SafePyCallMethod(handler, "SetCharacterSlot", "i", slot);
            g_cachedMainStream = handler;
            g_cachedMainStreamMs = GetTickCount();
            g_baerimHandlerDisp = disp;
            foundDisp = disp;
            ok = 1;
        }
    }
    if (ok)
        NativeLog("baerim arm isAutoSelect=1 disp=0x%X slot=%d", foundDisp, slot);
    else
        NativeLog("baerim arm isAutoSelect: no MainStream slot accepted SetInt");
    ReleasePythonGil(took, gilState);
    return ok;
}

static void* SafePyGetAttr(void* obj, const char* name)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !PyCanRun())
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
    if (!obj || !out || cap <= 1 || !py || !PyCanRun())
        return 0;
    out[0] = 0;
    FnPyGetAttrString getAttr = (FnPyGetAttrString)GetProcAddress(py, "PyObject_GetAttrString");
    FnPyStringAsString asString = (FnPyStringAsString)GetProcAddress(py, "PyString_AsString");
    typedef const char* (__cdecl* FnUtf8)(void*);
    FnUtf8 asUtf8 = (FnUtf8)GetProcAddress(py, "PyUnicode_AsUTF8");
    if (!asUtf8)
        asUtf8 = (FnUtf8)GetProcAddress(py, "PyBytes_AsString");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!getAttr || (!asString && !asUtf8))
        return 0;
    void* cls = nullptr;
    void* name = nullptr;
    int ok = 0;
    __try
    {
        cls = getAttr(obj, "__class__");
        if (!cls)
            return 0;
        name = getAttr(cls, "__name__");
        if (!name)
        {
            PyDec(cls);
            return 0;
        }
        const char* text = asString ? asString(name) : nullptr;
        if (!text && asUtf8)
            text = asUtf8(name);
        if (text)
        {
            lstrcpynA(out, text, cap);
            ok = out[0] ? 1 : 0;
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        if (errClear)
            errClear();
        ok = 0;
    }
    PyDec(name);
    PyDec(cls);
    return ok;
}

static int WindowClassIs(const char* name)
{
    if (!name || !name[0])
        return 0;
    HMODULE py = PythonDll();
    FnPyGilEnsure gilEnsure = py ? (FnPyGilEnsure)GetProcAddress(py, "PyGILState_Ensure") : nullptr;
    FnPyGilRelease gilRelease = py ? (FnPyGilRelease)GetProcAddress(py, "PyGILState_Release") : nullptr;
    int gil = 0;
    int took = 0;
    if (!PyCanRun() && gilEnsure && gilRelease)
    {
        __try
        {
            gil = gilEnsure();
            took = 1;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            took = 0;
        }
        if (!took || !PyCanRun())
            return 0;
    }
    else if (!PyCanRun())
        return 0;
    char cls[40];
    cls[0] = 0;
    void* handler = PythonHandler(NetStream());
    void* wnd = handler ? SafePyGetAttr(handler, "curPhaseWindow") : nullptr;
    const int match = (wnd && SafePyTypeName(wnd, cls, 40) && cls[0] && _stricmp(cls, name) == 0) ? 1 : 0;
    PyDec(wnd);
    if (took && gilRelease)
    {
        __try { gilRelease(gil); }
        __except (EXCEPTION_EXECUTE_HANDLER) {}
    }
    return match;
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
    PyDec(wnd);
    if (!steps)
        return -1;
    HMODULE py = PythonDll();
    typedef int (__cdecl* FnPyListSize)(void*);
    FnPyListSize size = py ? (FnPyListSize)GetProcAddress(py, "PyList_Size") : nullptr;
    if (!size)
        size = py ? (FnPyListSize)GetProcAddress(py, "PyObject_Size") : nullptr;
    FnPyErrClear errClear = py ? (FnPyErrClear)GetProcAddress(py, "PyErr_Clear") : nullptr;
    if (!size || !PyCanRun())
    {
        PyDec(steps);
        return -1;
    }
    int n = -1;
    __try
    {
        n = size(steps);
        if (n < 0 && errClear)
            errClear();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        n = -1;
    }
    PyDec(steps);
    return n;
}

static int SafePyRun(const char* code)
{
    HMODULE py = PythonDll();
    if (!py || !code || !PyCanRun())
        return 0;
    typedef int (__cdecl* FnPyRun)(const char*);
    FnPyRun run = (FnPyRun)GetProcAddress(py, "PyRun_SimpleString");
    if (!run)
        return 0;
    int ok = 0;
    __try
    {
        ok = run(code) == 0 ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        ok = 0;
    }
    return ok;
}

static int SafePyRunAfterPeek(const char* code)
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
    const int live = PyCanRun();
    int gil = 0;
    int took = 0;
    if (!live)
    {
        if (!gilEnsure || !gilRelease)
            return 0;
        __try
        {
            gil = gilEnsure();
            took = 1;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            took = 0;
        }
        if (!took || !PyCanRun())
            return 0;
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
    if (took && gilRelease)
    {
        __try { gilRelease(gil); }
        __except (EXCEPTION_EXECUTE_HANDLER) {}
    }
    return ok;
}

static int PyPhaseWindowIs(const char* name)
{
    if (!name || !name[0] || !g_pyPhaseWindow[0])
        return 0;
    return _stricmp(g_pyPhaseWindow, name) == 0 ? 1 : 0;
}

static int SafePyProbePhaseWindow()
{
    // Baerim has no net.GetMainStream — use GIL + PythonHandler (net+0x160).
    const DWORD now = GetTickCount();
    if (now - g_pyPhaseWindowMs < 400)
        return g_pyPhaseWindow[0] ? 1 : 0;
    g_pyPhaseWindowMs = now;
    int gilState = 0;
    const int took = EnsurePythonGil(&gilState);
    if (took < 0)
        return g_pyPhaseWindow[0] ? 1 : 0;
    void* handler = PythonHandler(NetStream());
    char cls[48];
    cls[0] = 0;
    void* wnd = handler ? SafePyGetAttr(handler, "curPhaseWindow") : nullptr;
    if (!wnd && handler)
        wnd = SafePyGetAttr(handler, "newPhaseWindow");
    int found = 0;
    if (wnd && SafePyTypeName(wnd, cls, 48) && cls[0])
    {
        if (_stricmp(g_pyPhaseWindow, cls) != 0)
            NativeLog("baerim phase window=%s nativePhase=%s",
                cls, g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str());
        lstrcpynA(g_pyPhaseWindow, cls, (int)sizeof(g_pyPhaseWindow));
        found = 1;
    }
    PyDec(wnd);
    ReleasePythonGil(took, gilState);
    return found || g_pyPhaseWindow[0] ? 1 : 0;
}

static int EscapePySingleQuoted(const char* in, char* out, int cap)
{
    if (!out || cap < 4)
        return 0;
    int j = 0;
    out[j++] = '\'';
    if (in)
    {
        for (int i = 0; in[i] && j < cap - 3; ++i)
        {
            if (in[i] == '\\' || in[i] == '\'')
                out[j++] = '\\';
            out[j++] = in[i];
        }
    }
    out[j++] = '\'';
    out[j] = 0;
    return 1;
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
        "  c=s.get('channel') or s.get('channels') or {}\n"
        "  i=sorted(c.keys())[0]\n"
        "  p=%d\n"
        "  for ck in c.keys():\n"
        "   if int(c[ck].get('tcp_port') or c[ck].get('port') or 0)==p:\n"
        "    i=ck\n"
        "    break\n"
        "  net.SetServerInfo('%%s, %%s' %% (s['name'], c[i]['name']))\n"
        "except:\n"
        " pass\n",
        g_loginPort);
    return SafePyRunAfterPeek(code);
}

static int SafePyCallMethod2Str(void* obj, const char* name, const char* a, const char* b)
{
    HMODULE py = PythonDll();
    if (!obj || !py || !name || !a || !b || !PyCanRun())
        return 0;
    FnPyCallMethod call = (FnPyCallMethod)GetProcAddress(py, "PyObject_CallMethod");
    FnPyErrClear errClear = (FnPyErrClear)GetProcAddress(py, "PyErr_Clear");
    if (!call)
        return 0;
    char method[64];
    lstrcpynA(method, name, 64);
    __try
    {
        void* result = call(obj, method, "ss", a, b);
        if (errClear)
            errClear();
        return result ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

// Exact channel-select + Log Me In (stock introLogin):
//   SetServerInfo
//   SetMarkServer(MARKADDR_DICT[region*1000+server*10])  — NOT the channel port
//   SetConnectInfo(channelIp, channelPort, authIp, authPort)
//   SetVersionId (Baerim DYNAMIC_VERSION)
//   SetLoginInfo + ConnectToAccountServer(channel, auth)
static int SafePyBaerimUiLogin(const Command& cmd)
{
    char acc[384], pwd[384];
    if (!EscapePySingleQuoted(cmd.account.c_str(), acc, (int)sizeof(acc)) ||
        !EscapePySingleQuoted(cmd.password.c_str(), pwd, (int)sizeof(pwd)))
        return 0;
    const int wantPort = cmd.port > 0 ? cmd.port : 0;
    const int wantIdx = cmd.channelIndex;

    // Prefer LoginWindow.SetChannel + Connect(id,pwd) — that already does mark/auth correctly.
    void* handler = PythonHandler(NetStream());
    if (handler)
    {
        SafePyPrepareLogin(cmd);
        void* wnd = SafePyGetAttr(handler, "curPhaseWindow");
        char cls[48];
        cls[0] = 0;
        if (wnd && SafePyTypeName(wnd, cls, 48) && strstr(cls, "Login"))
        {
            if (wantIdx >= 0)
                SafePyCallMethod(wnd, "SetChannel", "i", wantIdx);
            void* idEdit = SafePyGetAttr(wnd, "idEditLine");
            if (!idEdit)
                idEdit = SafePyGetAttr(wnd, "id");
            void* pwdEdit = SafePyGetAttr(wnd, "pwdEditLine");
            if (!pwdEdit)
                pwdEdit = SafePyGetAttr(wnd, "pwd");
            if (idEdit)
                SafePyCallMethodStr(idEdit, "SetText", cmd.account.c_str());
            if (pwdEdit)
                SafePyCallMethodStr(pwdEdit, "SetText", cmd.password.c_str());
            if (SafePyCallMethod2Str(wnd, "Connect", cmd.account.c_str(), cmd.password.c_str()))
            {
                NativeLog("baerim login via LoginWindow.Connect");
                return 1;
            }
            if (SafePyCallMethod(wnd, "__OnClickLoginButton", "", 0))
            {
                NativeLog("baerim login via LoginWindow.__OnClickLoginButton");
                return 1;
            }
        }
    }

    char code[4200];
    _snprintf_s(
        code,
        _TRUNCATE,
        "import net,os\n"
        "acc=%s;pwd=%s;wantPort=%d;wantIdx=%d\n"
        "ok=0;err='';detail=''\n"
        "try:\n"
        " import serverInfo,constInfo\n"
        " try:\n"
        "  if getattr(constInfo,'SEQUENCE_PACKET_ENABLE',0) and hasattr(net,'SetPacketSequenceMode'):\n"
        "   net.SetPacketSequenceMode()\n"
        " except: pass\n"
        " try:\n"
        "  if hasattr(net,'SetVersionId') and hasattr(constInfo,'GetVersionId'):\n"
        "   net.SetVersionId(constInfo.GetVersionId())\n"
        "   detail+='vid;'\n"
        " except Exception as ve:\n"
        "  detail+='vidFail;'\n"
        " rd=getattr(serverInfo,'REGION_DICT',{})\n"
        " regionID=0\n"
        " if 0 in rd: regionID=0\n"
        " elif '0' in rd: regionID='0'\n"
        " else: regionID=sorted(rd.keys())[0]\n"
        " r=rd[regionID]\n"
        " sk=sorted(r.keys())[0]\n"
        " s=r[sk]\n"
        " ch=s.get('channel') or s.get('channels') or {}\n"
        " ck=sorted(ch.keys())[0]\n"
        " if wantIdx>=0:\n"
        "  keys=sorted(ch.keys())\n"
        "  if wantIdx<len(keys): ck=keys[wantIdx]\n"
        " if wantPort>0:\n"
        "  for k in ch.keys():\n"
        "   if int(ch[k].get('tcp_port') or ch[k].get('port') or 0)==wantPort: ck=k;break\n"
        " c=ch[ck]\n"
        " chHost=(c.get('ip') or c.get('host') or s.get('ip') or s.get('host') or '')\n"
        " chPort=int(c.get('tcp_port') or c.get('port') or 0)\n"
        " authHost=chHost; authPort=30001\n"
        " try:\n"
        "  ra=serverInfo.REGION_AUTH_SERVER_DICT[regionID]\n"
        "  a=ra.get(sk) or ra.get(str(sk)) or ra[sorted(ra.keys())[0]]\n"
        "  authHost=a.get('ip') or a.get('host') or authHost\n"
        "  authPort=int(a.get('port') or a.get('tcp_port') or authPort)\n"
        " except: pass\n"
        " if authPort==chPort: authPort=30001\n"
        " markHost=chHost; markPort=chPort\n"
        " try:\n"
        "  sid=int(sk)\n"
        "  markKey=int(regionID)*1000+sid*10\n"
        "  md=serverInfo.MARKADDR_DICT\n"
        "  m=md.get(markKey) or md.get(str(markKey)) or md[sorted(md.keys())[0]]\n"
        "  markHost=m.get('ip') or m.get('host') or markHost\n"
        "  markPort=int(m.get('tcp_port') or m.get('port') or markPort)\n"
        "  detail+='mark;'\n"
        " except: detail+='markFallback;'\n"
        " try: net.SetServerInfo('%%s, %%s' %% (s.get('name',''), c.get('name','CH')))\n"
        " except: pass\n"
        " if hasattr(net,'SetMarkServer'): net.SetMarkServer(markHost,int(markPort))\n"
        " net.SetLoginInfo(acc,pwd)\n"
        " net.ConnectToAccountServer(chHost,int(chPort),authHost,int(authPort))\n"
        " ok=1\n"
        " detail+='ch=%%s:%%d auth=%%s:%%d mark=%%s:%%d'%%(chHost,chPort,authHost,authPort,markHost,markPort)\n"
        "except Exception as e:\n"
        " err=str(e)\n"
        "try:\n"
        " t=os.environ.get('TEMP','.')\n"
        " open(t+'\\\\farmbot-login.ok','wb').write('1' if ok else '0')\n"
        " open(t+'\\\\farmbot-login.err','wb').write(detail+'|'+err)\n"
        "except: pass\n",
        acc, pwd, wantPort, wantIdx);
    {
        char dir[MAX_PATH];
        if (GetTempPathA(MAX_PATH, dir))
        {
            char p1[MAX_PATH], p2[MAX_PATH];
            _snprintf_s(p1, _TRUNCATE, "%sfarmbot-login.err", dir);
            _snprintf_s(p2, _TRUNCATE, "%sfarmbot-login.ok", dir);
            DeleteFileA(p1);
            DeleteFileA(p2);
        }
    }
    if (!SafePyRunAfterPeek(code))
    {
        NativeLog("baerim login AfterPeek failed");
        return 0;
    }
    const std::string err = ReadTempUtf8("farmbot-login.err");
    const std::string ok = ReadTempUtf8("farmbot-login.ok");
    if (!err.empty())
        NativeLog("baerim login detail=%s", err.c_str());
    if (!ok.empty() && ok[0] == '1')
    {
        NativeLog("baerim login via channel-select+ConnectToAccountServer");
        return 1;
    }
    return 0;
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
    if (cmd.type != "get_phase" && cmd.type != "get_session" && cmd.type != "get_channels" &&
        cmd.type != "skip_intro" &&
        cmd.type != "login" && cmd.type != "select_character" && cmd.type != "enter_game")
        return false;

    if (cmd.type == "get_channels")
    {
        RefreshSession();
        const int ok = SafePyDumpChannels();
        g_lastOk = ok != 0;
        g_lastReason = ok ? "channels" : "serverInfo channels unavailable";
        return true;
    }

    if (cmd.type == "get_phase" || cmd.type == "get_session")
    {
        RefreshSession();
        if (g_baerim && (PhaseHas("Login") || WindowClassIs("LoginWindow") || PyPhaseWindowIs("LoginWindow")) &&
            g_channelsJson.size() <= 2)
            SafePyDumpChannels();
        g_lastOk = !g_sessionPhase.empty() || !g_sessionAccount.empty() ||
                   WindowClassIs("LoginWindow") || WindowClassIs("SelectCharacterWindow");
        g_lastReason = !g_sessionPhase.empty()
            ? g_sessionPhase
            : (g_sessionAccount.empty() ? "phase unread" : "account captured");
        return true;
    }

    if (cmd.type == "skip_intro")
    {
        if (g_baerim && g_channelsJson.size() <= 2)
            SafePyDumpChannels();
        RefreshSession();
        if (IsPastIntro() || PhaseHas("Login") || WindowClassIs("LoginWindow") || PyPhaseWindowIs("LoginWindow"))
        {
            if ((PhaseHas("Off") || g_sessionPhase.empty()) &&
                (WindowClassIs("LoginWindow") || PyPhaseWindowIs("LoginWindow")))
                g_sessionPhase = "Login";
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
        // Baerim shows LoginWindow itself — never call SetLoginPhase (recovered xref traps / corrupts).
        if (g_baerim)
        {
            if (g_channelsJson.size() <= 2)
                SafePyDumpChannels();
            InferBaerimPhaseFromWindow();
            g_skippedIntro = 1;
            const bool ready = PhaseHas("Login") || PhaseHas("Select") || PhaseHas("Load") ||
                               PhaseHas("Game") || g_channelsJson.size() > 2;
            g_lastOk = ready;
            if (ready && (PhaseHas("Off") || g_sessionPhase.empty()))
                g_sessionPhase = "Login";
            g_lastReason = g_sessionPhase.empty() ? "waiting for LoginWindow" : g_sessionPhase;
            return true;
        }
        const bool preLogin =
            g_sessionPhase.empty() || PhaseHas("Off") || PhaseHas("Logo") || PhaseHas("Intro") ||
            PhaseHas("Cine") || PhaseHas("EMPIRE");
        if (!preLogin)
        {
            g_lastReason = g_sessionPhase;
            return true;
        }
        if (!g_skippedIntro)
        {
            if (!kSetLoginPhaseRva)
            {
                RefreshSession();
                g_lastOk = IsPastIntro() || PhaseHas("Login");
                g_lastReason = g_sessionPhase.empty() ? "waiting for login phase" : g_sessionPhase;
                return true;
            }
            NativeLog("skip_intro SetLoginPhase from %s",
                g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str());
            if (!SafePhase((FnPhase)(g_base + kSetLoginPhaseRva), net))
            {
                g_skippedIntro = 1; // do not hammer a trapping RVA
                g_lastReason = "SetLoginPhase trapped";
                return true;
            }
            g_skippedIntro = 1;
        }
        RefreshSession();
        g_lastOk = IsPastIntro() || PhaseHas("Login");
        g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "skip_intro";
        return true;
    }

    if (cmd.type == "login")
    {
        RefreshSession();
        // HandShake is BEFORE account login completes — never treat it as already logged in.
        if (g_loginSent || PhaseHas("Select") || PhaseHas("Load") || PhaseHas("Game") ||
            WindowClassIs("SelectCharacterWindow") || PyPhaseWindowIs("SelectCharacterWindow"))
        {
            g_lastOk = true;
            g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "login already sent";
            return true;
        }
        // Baerim keeps native phase OffLine while LoginWindow is up. Once live
        // channels exist, SetMarkServer+Connect is safe — do not wait forever on phase.
        if (g_baerim && g_channelsJson.size() <= 2)
            SafePyDumpChannels();
        InferBaerimPhaseFromWindow();
        if ((PhaseHas("Off") || g_sessionPhase.empty()) &&
            (WindowClassIs("LoginWindow") || PyPhaseWindowIs("LoginWindow") ||
             (g_baerim && g_channelsJson.size() > 2)))
            g_sessionPhase = "Login";
        const bool onLogin =
            PhaseHas("Login") || WindowClassIs("LoginWindow") || PyPhaseWindowIs("LoginWindow") ||
            (g_baerim && g_channelsJson.size() > 2);
        if (!onLogin)
        {
            g_lastOk = false;
            g_lastReason = g_sessionPhase.empty() ? "login waiting for Login phase" : g_sessionPhase;
            NativeLog("login blocked phase=%s window=%s channels=%d",
                g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str(),
                g_pyPhaseWindow[0] ? g_pyPhaseWindow : "(none)",
                (int)g_channelsJson.size());
            return true;
        }

        Command loginCmd = cmd;
        if (g_baerim || IsLoopbackHost(loginCmd.host.c_str()) || loginCmd.host.empty() || loginCmd.port <= 0)
        {
            if (!ResolveBaerimLoginHosts(&loginCmd))
            {
                g_lastOk = false;
                g_lastReason = "baerim live serverInfo hosts unavailable (refusing 127.0.0.1)";
                return true;
            }
        }
        if (g_baerim && IsLoopbackHost(loginCmd.host.c_str()))
        {
            g_lastOk = false;
            g_lastReason = "refusing baerim login to loopback";
            return true;
        }

        g_openedGameWindow = 0;
        g_sawLoadSteps = 0;
        g_loadFinished = 0;
        g_inWorld = 0;
        g_loadNearEnd = 0;
        g_maxLoadSteps = 0;
        g_selectSent = 0;
        g_selectSentAtMs = 0;
        g_cachedMainStream = nullptr;
        g_cachedMainStreamMs = 0;
        g_pyPhaseWindow[0] = 0;
        g_pyPhaseWindowMs = 0;
        g_baerimWorldAtMs = 0;
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
        if (loginCmd.account.empty() || loginCmd.password.empty() || loginCmd.host.empty() || loginCmd.port <= 0)
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

        CaptureLogin(loginCmd.account.c_str(), loginCmd.password.c_str());
        g_loginPort = loginCmd.port;
        const char* authHost = loginCmd.authHost.empty() ? loginCmd.host.c_str() : loginCmd.authHost.c_str();
        int authPort = loginCmd.authPort > 0 ? loginCmd.authPort : 0;
        if (g_baerim && (authPort <= 0 || authPort == loginCmd.port))
            authPort = 30001;
        if (authPort <= 0)
            authPort = loginCmd.port;
        loginCmd.authHost = authHost;
        loginCmd.authPort = authPort;
        NativeLog("login send account=%s host=%s:%d auth=%s:%d slot=%d",
            loginCmd.account.c_str(), loginCmd.host.c_str(), loginCmd.port,
            authHost, authPort, loginCmd.slot);

        SafePyPrepareLogin(loginCmd);
        SafePyEnsureServerInfo();

        int connected = 0;
        if (g_baerim)
        {
            // Exact Log Me In path: LoginWindow.Connect(id,pwd) / MainStream.Connect
            // including SetVersionId + SetPacketSequenceMode.
            connected = SafePyBaerimUiLogin(loginCmd);
        }
        if (!connected && !g_baerim)
        {
            if (!SafeSetLogin(g_setLoginInfo ? g_setLoginInfo : setNet, net, loginCmd.account.c_str(), loginCmd.password.c_str()))
            {
                g_lastReason = "SetLoginInfo trapped";
                return true;
            }
            SafeSetLogin(setAcc, account, loginCmd.account.c_str(), loginCmd.password.c_str());
            if (!SafeConnect(connect, account, loginCmd.host.c_str(), loginCmd.port, authHost, authPort))
            {
                g_lastReason = "ConnectToAccountServer failed";
                return true;
            }
            connected = 1;
        }
        if (!connected)
        {
            g_lastOk = false;
            g_lastReason = "baerim login connect failed";
            NativeLog("baerim login failed after UI+net paths");
            return true;
        }
        // Arm stock Open()->StartGame on the real MainStream before select phase opens.
        if (g_baerim)
            SafePyBaerimArmAutoSelect(loginCmd.slot);
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
    RefreshSession();
    if (g_inWorld || g_selectSent || PhaseHas("Load") || PhaseHas("Game"))
    {
        g_selectSent = 1;
        g_lastOk = true;
        g_lastReason = g_sessionPhase.empty() ? "already entering" : g_sessionPhase;
        return true;
    }
    if (!PhaseHas("Select") && !WindowClassIs("SelectCharacterWindow"))
    {
        g_lastOk = false;
        g_lastReason = g_sessionPhase.empty() ? "select waiting for Select phase" : g_sessionPhase;
        return true;
    }
    const unsigned slot = (unsigned)cmd.slot;
    if (g_baerim)
    {
        // Prefer stock isAutoSelect Open()->StartGame (armed at login). If already on select,
        // OnIMEReturn/StartGame or net.DirectEnter (same as introselect OnUpdate).
        if (SafePyBaerimSelectCharacter(slot))
        {
            g_selectSent = 1;
            g_selectSentAtMs = GetTickCount();
            RefreshSession();
            g_lastOk = true;
            g_lastReason = "select sent";
            return true;
        }
        RefreshSession();
        g_lastOk = false;
        g_lastReason = "baerim select waiting for window";
        return true;
    }
    SafePyAutoSelect(net, slot);
    if (kDirectEnterModeSetRva)
        SafeDirectEnter((FnDirectEnterSet)(g_base + kDirectEnterModeSetRva), net, slot);
    if (kSelectCharacterRva && SafeSelectPacket((FnSelectCharacter)(g_base + kSelectCharacterRva), net, slot))
    {
        g_selectSent = 1;
        RefreshSession();
        g_lastOk = true;
        g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "select packet sent";
        return true;
    }
    if (SafePyClickStart(net))
    {
        g_selectSent = 1;
        RefreshSession();
        g_lastOk = true;
        g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "select sent";
        return true;
    }
    RefreshSession();
    g_lastOk = true;
    g_lastReason = "waiting for auto DirectEnter";
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

static bool CallSendItemUse(unsigned cell)
{
    void* net = NetStream();
    if (!g_base || !kSendItemUseRva || !net)
        return false;
    FnSendItemUse fn = (FnSendItemUse)(g_base + kSendItemUseRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    const unsigned packed = 1u | (cell << 8);
    int ok = 0;
    __try
    {
        ok = fn(net, packed, 0);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
    return ok != 0;
}

typedef unsigned (__thiscall* FnItemPosU32)(void* self, ItemPos pos);
typedef int (__thiscall* FnSelectItem)(void* self, unsigned vnum);
typedef void* (__thiscall* FnGetSelected)(void* self);
typedef const char* (__thiscall* FnItemCStr)(void* self);
typedef unsigned (__thiscall* FnItemU32)(void* self);
typedef unsigned (__thiscall* FnItemU32Arg)(void* self, int index);

static void* ItemDataMgr()
{
    if (!g_base || !kItemMgrRva)
        return nullptr;
    void* p = *(void**)(g_base + kItemMgrRva);
    return UserPtr(p) ? p : nullptr;
}

static unsigned CallItemPosU32(void* player, unsigned rva, unsigned slotOff, int cell)
{
    if (!player || !g_base)
        return 0;
    ItemPos pos;
    pos.window = 1;
    pos.cell = (unsigned short)cell;
    void* iabs = (char*)player + kThisAdj;
    if (g_baerim && slotOff)
    {
        void** vtbl = IAbsVtbl(player);
        if (vtbl && UserPtr(vtbl[slotOff / 4]))
        {
            unsigned v = 0;
            __try { v = ((FnItemPosU32)vtbl[slotOff / 4])(iabs, pos); }
            __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
            return v;
        }
    }
    if (!rva)
        return 0;
    FnItemPosU32 fn = (FnItemPosU32)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    unsigned v = 0;
    __try { v = fn(iabs, pos); }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    return v;
}

static unsigned NativeItemIndex(int slot)
{
    return CallItemPosU32(Player(), kGetItemIndexRva, kGetItemIndexSlot, slot);
}

static int NativeItemCount(int slot)
{
    unsigned v = CallItemPosU32(Player(), kGetItemCountRva, kGetItemCountSlot, slot);
    return v > 0 ? (int)v : 1;
}

static bool CallUseItem(unsigned cell)
{
    void* player = Player();
    if (!player || !kUseItemRva || !g_base)
        return false;
    typedef void (__thiscall* FnUse)(void* self, ItemPos pos);
    FnUse fn = (FnUse)(g_base + kUseItemRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    ItemPos pos;
    pos.window = 1;
    pos.cell = (unsigned short)cell;
    __try
    {
        fn((char*)player + kThisAdj, pos);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static void* NativeItemInfo(unsigned vnum, char* name, int nameCap, int* type, int* sub, int* apply, int* aval, int* v0, int* v1)
{
    if (name && nameCap > 0)
        name[0] = 0;
    if (type) *type = 0;
    if (sub) *sub = 0;
    if (apply) *apply = 0;
    if (aval) *aval = 0;
    if (v0) *v0 = 0;
    if (v1) *v1 = 0;
    void* mgr = ItemDataMgr();
    if (!mgr || !vnum)
        return nullptr;
    if (kSelectItemRva)
    {
        FnSelectItem select = (FnSelectItem)(g_base + kSelectItemRva);
        if (!IsBadCodePtr((FARPROC)select))
        {
            __try { select(mgr, vnum); }
            __except (EXCEPTION_EXECUTE_HANDLER) {}
        }
    }
    void* data = nullptr;
    if (kGetSelectedRva)
    {
        FnGetSelected selected = (FnGetSelected)(g_base + kGetSelectedRva);
        if (!IsBadCodePtr((FARPROC)selected))
        {
            __try { data = selected(mgr); }
            __except (EXCEPTION_EXECUTE_HANDLER) { data = nullptr; }
        }
    }
    if (!data)
        return nullptr;
    if (name && kGetItemNameRva)
    {
        FnItemCStr getName = (FnItemCStr)(g_base + kGetItemNameRva);
        if (!IsBadCodePtr((FARPROC)getName))
        {
            const char* text = nullptr;
            __try { text = getName(data); }
            __except (EXCEPTION_EXECUTE_HANDLER) { text = nullptr; }
            if (text && !IsBadStringPtrA(text, (UINT_PTR)nameCap))
                lstrcpynA(name, text, nameCap);
        }
    }
    if (type && kGetItemTypeRva)
    {
        FnItemU32 getType = (FnItemU32)(g_base + kGetItemTypeRva);
        if (!IsBadCodePtr((FARPROC)getType))
        {
            unsigned v = 0;
            __try { v = getType(data); }
            __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
            *type = (int)(v & 0xFF);
        }
    }
    if (sub && kGetItemSubTypeRva)
    {
        FnItemU32 getSub = (FnItemU32)(g_base + kGetItemSubTypeRva);
        if (!IsBadCodePtr((FARPROC)getSub))
        {
            unsigned v = 0;
            __try { v = getSub(data); }
            __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
            *sub = (int)(v & 0xFF);
        }
    }
    if (kGetValueRva)
    {
        FnItemU32Arg getVal = (FnItemU32Arg)(g_base + kGetValueRva);
        if (!IsBadCodePtr((FARPROC)getVal))
        {
            if (v0)
            {
                unsigned v = 0;
                __try { v = getVal(data, 0); }
                __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
                *v0 = (int)v;
            }
            if (v1)
            {
                unsigned v = 0;
                __try { v = getVal(data, 1); }
                __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
                *v1 = (int)v;
            }
        }
    }
    if (apply && aval && kGetAffectRva)
    {
        typedef int (__thiscall* FnAffect)(void* self, int index, int* pair);
        FnAffect getAff = (FnAffect)(g_base + kGetAffectRva);
        if (!IsBadCodePtr((FARPROC)getAff))
        {
            int pair[2] = { 0, 0 };
            __try { getAff(data, 0, pair); }
            __except (EXCEPTION_EXECUTE_HANDLER) { pair[0] = 0; pair[1] = 0; }
            if (pair[0] || pair[1])
            {
                *apply = pair[0];
                *aval = pair[1];
            }
        }
    }
    return data;
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
    out[0] = 0;
    __try
    {
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
            return true;
        if (size > 240)
            return false;
        const unsigned n = size < (unsigned)(cap - 1) ? size : (unsigned)(cap - 1);
        memcpy(out, p, n);
        out[n] = 0;
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        out[0] = 0;
        return false;
    }
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

static void* SafeVtblGetMain(void* self)
{
    if (!UserPtr(self))
        return nullptr;
    void* inst = nullptr;
    __try
    {
        void** vtbl = *(void***)self;
        if (UserPtr(vtbl) && UserPtr(vtbl[0x88 / 4]))
            inst = ((FnGetMain)vtbl[0x88 / 4])(self);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        inst = nullptr;
    }
    return LooksInstance(inst) ? inst : nullptr;
}

static void* CallGetMainActor()
{
    void* fromPtr = MainInstance();
    void* player = Player();
    if (g_baerim && player)
    {
        void* inst = SafeVtblGetMain((char*)player + kThisAdj);
        if (inst)
            return inst;
        inst = SafeVtblGetMain(player);
        if (inst)
            return inst;
    }
    if (!kGetMainActorRva || !g_base)
        return fromPtr;
    FnGetMain fn = (FnGetMain)(g_base + kGetMainActorRva);
    if (IsBadCodePtr((FARPROC)fn))
        return fromPtr;
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

static bool CallInstVoid(unsigned rva, void* inst)
{
    if (!rva || !inst || !g_base)
        return false;
    FnVoid fn = (FnVoid)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(inst);
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

static bool CallMoveToPixel(float x, float y)
{
    if (!g_base || !kMoveToPixelRva)
        return false;
    void* mains[2] = { CallGetMainActor(), MainInstance() };
    float pos[3] = { x, y, 0 };
    bool ok = false;
    for (int i = 0; i < 2; ++i)
    {
        void* me = mains[i];
        if (!me || (i == 1 && me == mains[0]))
            continue;
        ok = CallInstMove(kMoveToPixelRva, me, pos) || ok;
    }
    return ok;
}

static bool CallMoveToDest(unsigned vid, float x, float y)
{
    if (!g_base || (!kMoveToDestRva && !kMoveToPixelRva && !kApproachInstRva))
        return false;
    if (!vid && (x != 0.f || y != 0.f))
        return CallMoveToPixel(x, y);
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
        const float reach = g_baerim ? 280.f : 150.f;
        if (dx * dx + dy * dy <= reach * reach)
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
    if (!player || g_baerim)
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
    if (!player || g_baerim)
        return;
    CallAttackKey(player, false);
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

static unsigned BaerimReadU32(void* p, unsigned off)
{
    unsigned v = 0;
    if (!p)
        return 0;
    __try
    {
        v = *(unsigned*)((char*)p + off);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    return v;
}

static void BaerimClearMode(void* player)
{
    if (!player)
        return;
    __try
    {
        *(unsigned*)((char*)player + 0x54) = 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static void BaerimSetTargetVid(void* player, unsigned vid)
{
    if (!player)
        return;
    __try
    {
        *(unsigned*)((char*)player + 0x41A78) = vid;
        *(unsigned*)((char*)player + 0x419D4) = vid;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static bool BaerimSetAttackKey(void* player, bool press)
{
    if (!player)
        return false;
    __try
    {
        *((BYTE*)player + 0x64) = press ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
    if (!g_base || !kAttackKeyRva)
        return true;
    FnAttackKey fn = (FnAttackKey)(g_base + kAttackKeyRva);
    if (IsBadCodePtr((FARPROC)fn))
        return true;
    __try
    {
        fn(player, press);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
    return true;
}

static void BaerimReleaseChaseForMove(void* player)
{
    if (!g_baerim || !player)
        return;
    // PressActor / attack-key hold overrides MoveToPixel unless chase state is cleared first.
    BaerimSetAttackKey(player, false);
    BaerimClearMode(player);
    BaerimSetTargetVid(player, 0);
    ClearReserved(player);
}

static bool ReadActorRotation(void* inst, float* rot)
{
    if (!UserPtr(inst) || !rot || !kGraphicDisp)
        return false;
    __try
    {
        *rot = *(float*)((char*)inst + kGraphicDisp + 0x4A4);
        return !std::isnan(*rot);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static float NormalizeDeg(float deg)
{
    while (deg >= 360.f)
        deg -= 360.f;
    while (deg < 0.f)
        deg += 360.f;
    return deg;
}

static float FacingDegFromRotation(float rot)
{
    // Matches LiveGameStateProvider.MinimapFacing: facingDeg = 540 - rotation.
    return NormalizeDeg(540.f - rot);
}

static float BearingToTargetDeg(float mx, float my, float tx, float ty)
{
    // Same frame as movement-derived facing: Atan2(dx, dy).
    return NormalizeDeg(atan2f(tx - mx, ty - my) * (180.f / 3.14159265f));
}

static float AngleDeltaDeg(float a, float b)
{
    float d = fabsf(NormalizeDeg(a) - NormalizeDeg(b));
    if (d > 180.f)
        d = 360.f - d;
    return d;
}

static bool BaerimFacingTarget(void* main, float mx, float my, float tx, float ty, float* outDelta)
{
    float rot = 0.f;
    if (!ReadActorRotation(main, &rot))
        return false;
    const float facing = FacingDegFromRotation(rot);
    const float want = BearingToTargetDeg(mx, my, tx, ty);
    const float delta = AngleDeltaDeg(facing, want);
    if (outDelta)
        *outDelta = delta;
    return delta <= 50.f;
}

static bool FireNativeFace(void* player, float mx, float my, float tx, float ty, float dist)
{
    float fx = tx - mx;
    float fy = ty - my;
    float len = sqrtf(fx * fx + fy * fy);
    if (len < 1.f)
        return false;
    fx /= len;
    fy /= len;
    // Short step toward the target so the client turns the mount/body without leaving melee.
    float step = (len > 180.f) ? 140.f : (len * 0.5f);
    if (step < 50.f)
        step = 50.f;
    if (step > len - 20.f && len > 40.f)
        step = len - 20.f;
    const float sx = mx + fx * step;
    const float sy = my + fy * step;
    const DWORD now = GetTickCount();
    g_faceFromX = mx;
    g_faceFromY = my;
    g_faceUntil = now + 550;
    g_faceVid = g_clickVid;
    g_lastFaceMs = now;
    BaerimReleaseChaseForMove(player);
    const bool walked = CallMoveToPixel(sx, sy);
    NativeLog("BOT face step=%.0f xy=%.0f,%.0f ok=%d dist=%.0f mode=%u",
        step, sx, sy, walked ? 1 : 0, dist, BaerimReadU32(player, 0x54));
    return walked;
}

static bool FireNativeSidestep(void* player, float mx, float my, float tx, float ty, float dist)
{
    float fx = tx - mx;
    float fy = ty - my;
    float len = sqrtf(fx * fx + fy * fy);
    if (len < 1.f)
    {
        fx = 1.f;
        fy = 0.f;
        len = 1.f;
    }
    fx /= len;
    fy /= len;
    const float lx = -fy;
    const float ly = fx;
    const int n = g_nativeSidesteps;
    float ox = -fx;
    float oy = -fy;
    float step = 420.f;
    if (n == 1)
    {
        ox = -fx + lx;
        oy = -fy + ly;
        const float sl = sqrtf(ox * ox + oy * oy);
        if (sl > 1.f)
        {
            ox /= sl;
            oy /= sl;
        }
        step = 640.f;
    }
    else if (n == 2)
    {
        ox = -fx - lx;
        oy = -fy - ly;
        const float sl = sqrtf(ox * ox + oy * oy);
        if (sl > 1.f)
        {
            ox /= sl;
            oy /= sl;
        }
        step = 640.f;
    }
    else if (n == 3)
    {
        ox = lx;
        oy = ly;
        step = 680.f;
    }
    else if (n >= 4)
    {
        ox = -lx;
        oy = -ly;
        step = 680.f;
    }
    const float sx = mx + ox * step;
    const float sy = my + oy * step;
    const DWORD now = GetTickCount();
    g_nativeSidesteps++;
    g_sidestepFromX = mx;
    g_sidestepFromY = my;
    g_stuckX = mx;
    g_stuckY = my;
    g_stuckAt = now;
    g_sidestepUntil = now + 1600;
    BaerimReleaseChaseForMove(player);
    const bool walked = CallMoveToPixel(sx, sy);
    NativeLog("BOT native sidestep try=%d step=%.0f xy=%.0f,%.0f ok=%d stuckDist=%.0f mode=%u",
        g_nativeSidesteps, step, sx, sy, walked ? 1 : 0, dist, BaerimReadU32(player, 0x54));
    return walked;
}

static bool DriveToTarget(void* player, unsigned vid, float x, float y)
{
    (void)x;
    (void)y;
    if (!player || !vid)
        return false;
    g_clickVid = vid;
    if (!g_inWorld || PhaseHas("Load"))
        return false;
    void* live = CallGetInstance(g_clickVid);
    if (!LooksInstance(live))
    {
        if (g_baerim)
        {
            BaerimSetAttackKey(player, false);
            BaerimSetTargetVid(player, 0);
            ClearReserved(player);
        }
        g_chaseVid = 0;
        g_walkVid = 0;
        g_meleeVid = 0;
        g_meleeEngageVid = 0;
        InterlockedExchange(&g_allowMelee, 0);
        return false;
    }
    InterlockedExchange(&g_botCall, 1);

    if (g_baerim)
    {
        // AutoHunt chase/face: SetTarget + PressActor(isAuto). Baerim does not sustain
        // combo from PressActor alone — once InRange, hold SetAttackKeyState every tick.
        // MoveToPixel only for stuck sidesteps.
        void* main = CallGetMainActor();
        float mx = 0, my = 0, tx = 0, ty = 0;
        if (LooksInstance(main))
            ReadActorPixel(main, &mx, &my);
        ReadActorPixel(live, &tx, &ty);
        const float dx = mx - tx;
        const float dy = my - ty;
        const float dist = sqrtf(dx * dx + dy * dy);
        const DWORD now = GetTickCount();
        bool inRange = false;
        if (LooksInstance(main))
        {
            if (kInRangeRva)
                inRange = CallInRange(main, live);
            else
                inRange = dist <= 280.f;
        }
        InterlockedExchange(&g_allowMelee, inRange ? 1 : 0);
        g_walkVid = g_clickVid;

        if (g_sidestepUntil)
        {
            const float slippedX = mx - g_sidestepFromX;
            const float slippedY = my - g_sidestepFromY;
            const float slipped2 = slippedX * slippedX + slippedY * slippedY;
            if (slipped2 >= 140.f * 140.f)
            {
                g_sidestepUntil = 0;
                g_stuckX = mx;
                g_stuckY = my;
                g_stuckAt = now;
                g_meleeEngageVid = 0;
            }
            else if (now >= g_sidestepUntil)
            {
                g_sidestepUntil = 0;
                g_meleeEngageVid = 0;
                if (kMoveToPixelRva)
                {
                    FireNativeSidestep(player, mx, my, tx, ty, dist);
                    InterlockedExchange(&g_botCall, 0);
                    return false;
                }
            }
            else
            {
                InterlockedExchange(&g_botCall, 0);
                return false;
            }
        }

        if (!inRange && LooksInstance(main) && !g_sidestepUntil)
        {
            const float movedX = mx - g_stuckX;
            const float movedY = my - g_stuckY;
            if (!g_stuckAt || movedX * movedX + movedY * movedY > 80.f * 80.f)
            {
                g_stuckX = mx;
                g_stuckY = my;
                g_stuckAt = now;
            }
            else if (now - g_stuckAt >= 450 && kMoveToPixelRva)
            {
                FireNativeSidestep(player, mx, my, tx, ty, dist);
                g_meleeEngageVid = 0;
                InterlockedExchange(&g_botCall, 0);
                return false;
            }
        }
        else if (inRange)
        {
            g_stuckAt = 0;
            g_nativeSidesteps = 0;
        }

        const bool needEngage = g_meleeEngageVid != g_clickVid;
        const bool refreshChase = !inRange && g_lastWalkMs && now - g_lastWalkMs >= 1500;
        if (needEngage || refreshChase)
        {
            g_meleeEngageVid = g_clickVid;
            g_meleeVid = g_clickVid;
            g_faceUntil = 0;
            g_faceVid = g_clickVid;
            if (!inRange)
                BaerimSetAttackKey(player, false);
            BaerimSetTargetVid(player, g_clickVid);
            CallSetTarget(player, g_clickVid, 1);
            const bool pressed = CallPressActor(player, g_clickVid);
            g_stuckX = mx;
            g_stuckY = my;
            g_stuckAt = now;
            g_lastWalkMs = now;
            NativeLog("BOT chase vid=%u press=%d dist=%.0f inRange=%d refresh=%d mode=%u",
                g_clickVid, pressed ? 1 : 0, dist, inRange ? 1 : 0, refreshChase ? 1 : 0,
                BaerimReadU32(player, 0x54));
            if (!inRange)
            {
                InterlockedExchange(&g_botCall, 0);
                return pressed;
            }
        }

        if (!inRange)
        {
            InterlockedExchange(&g_botCall, 0);
            return false;
        }

        // In range: keep AttackKey held — this is what sustains swings on Baerim.
        BaerimSetTargetVid(player, g_clickVid);
        const bool keyed = BaerimSetAttackKey(player, true);
        if (!g_lastAtkMs || now - g_lastAtkMs >= 400)
        {
            g_lastAtkMs = now;
            NativeLog("BOT combo vid=%u key=%d dist=%.0f atkKey=%u mode=%u",
                g_clickVid, keyed ? 1 : 0, dist, BaerimReadU32(player, 0x64) & 0xFF,
                BaerimReadU32(player, 0x54));
        }
        InterlockedExchange(&g_botCall, 0);
        return true;
    }

    if (PixelInMelee(g_clickVid))
    {
        const bool already = g_walkVid == g_clickVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) != 0;
        InterlockedExchange(&g_allowMelee, 1);
        g_walkVid = g_clickVid;
        if (already)
        {
            const DWORD now = GetTickCount();
            if (!g_lastAtkMs || now - g_lastAtkMs >= 400)
            {
                g_lastAtkMs = now;
                CallAttackKey(player, true);
            }
            InterlockedExchange(&g_botCall, 0);
            return true;
        }
        CallSetTarget(player, g_clickVid, 1);
        SetAutoAttackVid(player, g_clickVid);
        const bool pressed = CallPressActor(player, g_clickVid);
        g_lastAtkMs = GetTickCount();
        CallAttackKey(player, true);
        NativeLog("BOT in melee press=%d (attack allowed) vid=%u", pressed ? 1 : 0, g_clickVid);
        InterlockedExchange(&g_botCall, 0);
        return pressed;
    }

    if (g_walkVid == g_clickVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) != 0)
    {
        CallAttackKey(player, true);
        InterlockedExchange(&g_botCall, 0);
        return true;
    }

    InterlockedExchange(&g_allowMelee, 0);
    const bool fresh = g_walkVid != g_clickVid;
    const DWORD now = GetTickCount();
    if (fresh)
    {
        g_walkVid = g_clickVid;
        g_lastWalkMs = now;
        HoldAttackKeyOff(player);
        CallSetTarget(player, g_clickVid, 1);
        SetAutoAttackVid(player, g_clickVid);
        const bool pressed = CallPressActor(player, g_clickVid);
        const bool reserved = CallRva(kReserveClickRva, player, g_clickVid);
        NativeLog("BOT click-walk vid=%u press=%d reserve=%d pressActor=0x%X reserve=0x%X",
            g_clickVid, pressed ? 1 : 0, reserved ? 1 : 0, kPressActorRva, kReserveClickRva);
        InterlockedExchange(&g_botCall, 0);
        return false;
    }

    if (now - g_lastWalkMs >= 1500)
    {
        g_lastWalkMs = now;
        SetAutoAttackVid(player, g_clickVid);
        CallPressActor(player, g_clickVid);
        CallRva(kReserveClickRva, player, g_clickVid);
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
    if (g_baerim)
        return LooksInstance(g_mainInst) ? g_mainInst : nullptr;
    void* mgr = CharMgr();
    if (!mgr)
        return nullptr;
    void* inst = *(void**)((char*)mgr + kMainPtrDisp);
    return LooksInstance(inst) ? inst : nullptr;
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
    void* selves[2];
    FillPlayerSelves(player, selves);
    for (int i = 0; i < 2; ++i)
    {
        if (!selves[i] || (i == 1 && selves[1] == selves[0]))
            continue;
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

static int SafePressActor(FnPressActor fn, void* self, void* target, unsigned vid)
{
    if (!fn || !self || !target || IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        // isAuto=true matches client AutoHunt (__OnPressActor(..., true)) — sustains chase/attack.
        fn(self, target, vid, 1);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int SafeReserveClick(void* player, unsigned vid)
{
    if (!g_base || !player || !vid || !kReserveClickRva)
        return 0;
    FnVid fn = (FnVid)(g_base + kReserveClickRva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    __try
    {
        fn(player, vid);
        return 1;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static bool CallPressActor(void* player, unsigned vid)
{
    if (!kPressActorRva || !player || !vid)
        return false;
    g_clickVid = vid;
    void* target = CallGetInstance(g_clickVid);
    if (!LooksInstance(target))
        return false;
    FnPressActor fn = (FnPressActor)(g_base + kPressActorRva);
    if (!g_base || IsBadCodePtr((FARPROC)fn))
        return false;
    if (g_baerim)
    {
        void* main = CallGetMainActor();
        if (!LooksInstance(main))
            return false;
        return SafePressActor(fn, player, main, g_clickVid) != 0;
    }
    void* selves[2];
    FillPlayerSelves(player, selves);
    for (int i = 0; i < 2; ++i)
    {
        if (!selves[i] || (i == 1 && selves[1] == selves[0]))
            continue;
        if (SafePressActor(fn, selves[i], target, g_clickVid))
            return true;
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

static void PauseChase(void* player, DWORD holdMs)
{
    g_sidestepUntil = GetTickCount() + holdMs;
    if (g_baerim)
        BaerimReleaseChaseForMove(player);
    else
        ClearReserved(player);
}

static void StopCombat(void* player)
{
    g_chaseVid = 0;
    g_walkVid = 0;
    g_clickVid = 0;
    g_faceUntil = 0;
    g_faceVid = 0;
    g_meleeVid = 0;
    g_meleeEngageVid = 0;
    InterlockedExchange(&g_allowMelee, 0);
    ClearReserved(player);
    if (g_baerim)
    {
        BaerimSetAttackKey(player, false);
        BaerimSetTargetVid(player, 0);
        return;
    }
    CallRva(kSetAutoRva, player, 0);
    SetAutoAttackVid(player, 0);
    HoldAttackKeyOff(player);
    CallSetTarget(player, 0, 1);
}

static void ChaseTick(void* player)
{
    if (!g_chaseVid || !player)
        return;
    if (!g_inWorld || PhaseHas("Load"))
    {
        g_chaseVid = 0;
        g_walkVid = 0;
        InterlockedExchange(&g_allowMelee, 0);
        return;
    }
    const DWORD now = GetTickCount();
    const bool holding = g_walkVid == g_chaseVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) != 0;
    if (g_baerim && !LooksInstance(CallGetInstance(g_chaseVid)))
    {
        BaerimSetAttackKey(player, false);
        BaerimSetTargetVid(player, 0);
        ClearReserved(player);
        g_chaseVid = 0;
        g_walkVid = 0;
        InterlockedExchange(&g_allowMelee, 0);
        return;
    }
    if (g_lastChaseMs && now - g_lastChaseMs < (g_baerim ? 50u : (holding ? 250u : 150u)))
        return;
    g_lastChaseMs = now;
    DriveToTarget(player, g_chaseVid, g_chaseX, g_chaseY);
}

static bool ExtractPackBlob(const char* virt, const char* dest);
static int PackExists(const char* virt);
static void* PackMgrPtr();
static int PngFileBytes(const unsigned char* p);
static bool WriteBytes(const char* path, const void* data, int bytes);
static bool DataAddr(const void* p);
static int LooksLikeTga(const unsigned char* p);

static int GuessMappedSize(void* mapped)
{
    if (!mapped)
        return 0;
    unsigned char* m = (unsigned char*)mapped;
    for (int off = 4; off <= 0x80; off += 4)
    {
        unsigned sz = 0;
        __try { sz = *(unsigned*)(m + off); }
        __except (EXCEPTION_EXECUTE_HANDLER) { sz = 0; }
        if (sz >= 64 && sz <= 2 * 1024 * 1024)
            return (int)sz;
    }
    return 0;
}

static void Head4(const unsigned char* p, unsigned* o)
{
    o[0] = o[1] = o[2] = o[3] = 0;
    if (!p)
        return;
    __try
    {
        o[0] = p[0];
        o[1] = p[1];
        o[2] = p[2];
        o[3] = p[3];
    }
    __except (EXCEPTION_EXECUTE_HANDLER) {}
}

static int __stdcall HookPackGetImpl(void* self, void* mapped, const char* name, const void** dataOut)
{
    typedef int (__thiscall* FnGet)(void* self, void* mapped, const char* name, const void** dataOut);
    int ok = 0;
    if (g_getTramp)
        ok = ((FnGet)g_getTramp)(self, mapped, name, dataOut);
    if (!name || !strstr(name, "captcha"))
        return ok;
    const unsigned char* p = (ok && dataOut) ? (const unsigned char*)*dataOut : nullptr;
    int n = PngFileBytes(p);
    if (n < 32)
        n = LooksLikeTga(p);
    if (n < 32)
        n = GuessMappedSize(mapped);
    unsigned h[4];
    Head4(p, h);
    NativeLog("dump bg hooked get %s ok=%d bytes=%d ptr=%p head=%02X%02X%02X%02X dest=%d",
        name, ok, n, p, h[0], h[1], h[2], h[3], g_dumpDest[0] ? 1 : 0);
    if (n >= 32 && p && g_dumpDest[0] && WriteBytes(g_dumpDest, p, n))
        g_dumpCaptured = n;
    return ok;
}

static __declspec(naked) int HookPackGet()
{
    __asm {
        push ebp
        mov ebp, esp
        push dword ptr [ebp+16]
        push dword ptr [ebp+12]
        push dword ptr [ebp+8]
        push ecx
        call HookPackGetImpl
        pop ebp
        ret 12
    }
}

static void RemoveGetCaptureHook()
{
    if (!g_getHooked || !g_base || !kPackGetRva)
        return;
    BYTE* get = g_base + kPackGetRva;
    DWORD old = 0;
    if (VirtualProtect(get, 8, PAGE_EXECUTE_READWRITE, &old))
    {
        memcpy(get, g_getStolen, 6);
        VirtualProtect(get, 8, old, &old);
        FlushInstructionCache(GetCurrentProcess(), get, 8);
    }
    if (g_getTramp)
    {
        VirtualFree(g_getTramp, 0, MEM_RELEASE);
        g_getTramp = nullptr;
    }
    g_getHooked = 0;
    NativeLog("dump bg get hook removed");
}

static int InstallGetCaptureHook()
{
    if (g_getHooked)
        return 1;
    if (!g_base || !kPackGetRva)
        return 0;
    BYTE* get = g_base + kPackGetRva;
    NativeLog("dump bg get bytes %02X %02X %02X %02X %02X %02X %02X %02X at %p",
        get[0], get[1], get[2], get[3], get[4], get[5], get[6], get[7], get);
    if (get[0] != 0x55 || get[1] != 0x8B || get[2] != 0xEC)
    {
        NativeLog("dump bg get prologue mismatch");
        return 0;
    }
    g_getTramp = (BYTE*)VirtualAlloc(nullptr, 32, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!g_getTramp)
        return 0;
    memcpy(g_getStolen, get, 6);
    memcpy(g_getTramp, get, 6);
    g_getTramp[6] = 0xE9;
    *(DWORD*)(g_getTramp + 7) = (DWORD)(get + 6) - (DWORD)(g_getTramp + 11);
    DWORD old = 0;
    if (!VirtualProtect(get, 8, PAGE_EXECUTE_READWRITE, &old))
    {
        VirtualFree(g_getTramp, 0, MEM_RELEASE);
        g_getTramp = nullptr;
        return 0;
    }
    g_getProt = old;
    const DWORD rel = (DWORD)(BYTE*)HookPackGet - (DWORD)(get + 5);
    get[0] = 0xE9;
    *(DWORD*)(get + 1) = rel;
    get[5] = 0x90;
    VirtualProtect(get, 8, old, &old);
    FlushInstructionCache(GetCurrentProcess(), get, 8);
    g_getHooked = 1;
    NativeLog("dump bg get hook installed at %p tramp=%p", get, g_getTramp);
    return 1;
}

static void* ClientGetResource(const char* virt)
{
    if (!g_base || !kResMgrRva || !kGetResRva || !virt || !virt[0])
        return nullptr;
    void* mgr = nullptr;
    __try { mgr = *(void**)(g_base + kResMgrRva); }
    __except (EXCEPTION_EXECUTE_HANDLER) { mgr = nullptr; }
    if (!UserPtr(mgr))
        return nullptr;
    typedef void* (__thiscall* FnGetRes)(void* self, const char* name);
    FnGetRes getRes = (FnGetRes)(g_base + kGetResRva);
    if (IsBadCodePtr((FARPROC)getRes))
        return nullptr;
    void* img = nullptr;
    __try { img = getRes(mgr, virt); }
    __except (EXCEPTION_EXECUTE_HANDLER) { img = nullptr; }
    return img;
}

static int DumpViaClientLoad(const char* virt, const char* dest)
{
    NativeLog("dump bg client-load %s exist=%d hooked=%d", virt, PackExists(virt), (int)g_getHooked);
    g_dumpCaptured = 0;
    _snprintf_s(g_dumpVirt, _TRUNCATE, "%s", virt);
    _snprintf_s(g_dumpDest, _TRUNCATE, "%s", dest);
    NativeLog("dump bg getRes start");
    void* img = ClientGetResource(virt);
    NativeLog("dump bg getRes done img=%p captured=%d", img, g_dumpCaptured);
    if (g_dumpCaptured > 0)
        return 1;
    char py[900];
    _snprintf_s(py, _TRUNCATE,
        "try:\n"
        " import app\n"
        " p='%s'\n"
        " n=app.OpenTextFile(p)\n"
        " f=open(r'C:\\Temp\\farmbot-captcha.log','ab')\n"
        " f.write('open ')\n"
        " f.write(p)\n"
        " f.write(' ')\n"
        " f.write(str(n))\n"
        " f.write('\\r\\n')\n"
        " f.close()\n"
        " if n: app.CloseTextFile(n)\n"
        "except Exception,e:\n"
        " try:\n"
        "  f=open(r'C:\\Temp\\farmbot-captcha.log','ab'); f.write('open fail '+str(e)+'\\r\\n'); f.close()\n"
        " except: pass\n"
        "try:\n"
        " import ui\n"
        " w=ui.ImageBox()\n"
        " w.LoadImage('%s')\n"
        " f=open(r'C:\\Temp\\farmbot-captcha.log','ab'); f.write('loadimage ok %s\\r\\n'); f.close()\n"
        "except Exception,e:\n"
        " try:\n"
        "  f=open(r'C:\\Temp\\farmbot-captcha.log','ab'); f.write('loadimage fail %s '+str(e)+'\\r\\n'); f.close()\n"
        " except: pass\n",
        virt, virt, virt, virt);
    NativeLog("dump bg OpenTextFile start");
    SafePyRun(py);
    NativeLog("dump bg OpenTextFile done captured=%d", g_dumpCaptured);
    g_dumpVirt[0] = 0;
    g_dumpDest[0] = 0;
    return g_dumpCaptured > 0;
}

static void DumpCaptchaStep()
{
    const long step = g_captchaDumpStep;
    if (step <= 0)
        return;
    const DWORD now = GetTickCount();
    if (g_captchaDumpLastMs && now - g_captchaDumpLastMs < 400)
        return;
    g_captchaDumpLastMs = now;
    char virt[260];
    char dest[260];
    virt[0] = 0;
    if (step >= 1 && step <= 14)
    {
        _snprintf_s(virt, _TRUNCATE, "d:/ymir work/ui/game/captcha/bg_%d.png", (int)step);
        _snprintf_s(dest, _TRUNCATE, "C:\\Temp\\captcha-bgs\\bg_%d.png", (int)step);
    }
    else
    {
        const char* extras[] = { "btn_norm.png", "btn_hover.png", "btn_select.png" };
        const int extra = (int)step - 15;
        if (extra < 0 || extra > 2)
        {
            NativeLog("dump bg done got=%d to C:\\Temp\\captcha-bgs", g_captchaDumpGot);
            g_captchaDumpStep = 0;
            RemoveGetCaptureHook();
            InterlockedExchange(&g_captchaDumpBusy, 0);
            return;
        }
        _snprintf_s(virt, _TRUNCATE, "d:/ymir work/ui/game/captcha/%s", extras[extra]);
        _snprintf_s(dest, _TRUNCATE, "C:\\Temp\\captcha-bgs\\%s", extras[extra]);
    }
    NativeLog("dump bg step %d %s", (int)step, virt);
    if (DumpViaClientLoad(virt, dest))
    {
        ++g_captchaDumpGot;
        NativeLog("dump bg ok %s", virt);
    }
    else
        NativeLog("dump bg miss %s", virt);
    g_captchaDumpStep = step + 1;
}

static void DumpCaptchaBegin()
{
    if (InterlockedCompareExchange(&g_captchaDumpBusy, 1, 0) != 0)
    {
        g_lastOk = true;
        g_lastReason = "dump already running";
        NativeLog("dump bg already running");
        return;
    }
    g_captchaDumpGot = 0;
    g_captchaDumpLastMs = GetTickCount();
    CreateDirectoryA("C:\\Temp", nullptr);
    CreateDirectoryA("C:\\Temp\\captcha-bgs", nullptr);
    NativeLog("dump bg armed packMgr=0x%X exist=0x%X resMgr=0x%X getRes=0x%X packGet=0x%X phase=%s",
        kPackMgrRva, kPackExistRva, kResMgrRva, kGetResRva, kPackGetRva,
        g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str());
    if (!InstallGetCaptureHook())
        NativeLog("dump bg get hook failed; OpenTextFile may still capture");
    g_captchaDumpStep = 1;
    g_lastOk = true;
    g_lastReason = "dump armed";
}

static void CaptchaTick()
{
    // Event-driven captcha: install hooks once, solve inside SetChallenge/Open.
    // Never poll with gc.get_objects — that aborted python27 after a few minutes.
    // Never RefreshSession/WindowClassIs from AfterPeek — GIL spam crashed ~1min in-world.
    if (!g_baerim)
        return;
    if (!g_inWorld || !g_openedGameWindow)
        return;
    const DWORD now = GetTickCount();
    if (!g_baerimWorldAtMs)
        g_baerimWorldAtMs = now;
    // Wait well after spawn/combat settle before installing Python captcha hooks.
    if (now - g_baerimWorldAtMs < 20000)
        return;
    if (g_captchaHooked)
    {
        // Lightweight status file written by the Python hook (no Py run here).
        FILE* sf = nullptr;
        int shown = 0;
        if (fopen_s(&sf, "C:\\Temp\\farmbot-captcha.state", "rb") == 0 && sf)
        {
            char c = 0;
            fread(&c, 1, 1, sf);
            fclose(sf);
            shown = (c == '1') ? 1 : 0;
        }
        if (shown && !g_captchaShown)
            NativeLog("captcha window open (hook)");
        if (!shown && g_captchaShown)
            NativeLog("captcha window closed");
        g_captchaShown = shown;
        return;
    }
    // Retry install at most every 30s until uiCaptcha is importable.
    if (g_captchaInstallAtMs && now - g_captchaInstallAtMs < 30000)
        return;
    if (InterlockedCompareExchange(&g_captchaBusy, 1, 0) != 0)
        return;
    g_captchaInstallAtMs = now;
    static const char kPyInstall[] = R"PY(
try:
 import constInfo, traceback
 try: import uiCaptcha
 except: uiCaptcha=None
 MAP={1:8,2:5,3:5,4:2,5:5,6:1,7:0,8:2,9:6,10:0,11:2,12:1,13:7,14:5,15:2,16:1,17:7,18:0,19:2,20:0,21:4,22:7,23:6,24:4,25:5,26:1,27:0,28:1,29:1,30:0,31:2,32:3,33:5,34:7,35:8,36:0,37:2,38:4,39:5,40:6,41:7,42:0,43:1,44:3,45:5,46:6,47:8}
 def _log(s):
  try:
   f=open(r'C:\Temp\farmbot-captcha.log','ab'); f.write(str(s)+'\r\n'); f.close()
  except: pass
 def _state(v):
  try:
   f=open(r'C:\Temp\farmbot-captcha.state','wb'); f.write('1' if v else '0'); f.close()
  except: pass
 def _hooked(v):
  try:
   f=open(r'C:\Temp\farmbot-captcha.hooked','wb'); f.write('1' if v else '0'); f.close()
  except: pass
 def _txt(v):
  if v is None: return ''
  try: return str(v)
  except:
   try: return unicode(v).encode('utf-8','replace')
   except: return ''
 def _cfg_row(cfg, cid):
  if not cfg or not cid: return None
  row=None
  try: row=cfg.get(cid)
  except: row=None
  if row is None:
   try: row=cfg.get(str(cid))
   except: row=None
  return row
 def _shown(w):
  if not w: return 0
  for x in (w, getattr(w,'main_window',None), getattr(w,'board',None)):
   try:
    if x and x.IsShow(): return 1
   except: pass
  return 0
 def _solve(wnd, how):
  if not wnd: return
  shown=_shown(wnd)
  _state(shown)
  if not shown:
   constInfo._farmbot_captcha_done=None
   return
  cid=int(getattr(wnd,'challengeId',0) or 0)
  cfg=getattr(uiCaptcha,'CAPTCHA_CONFIG',None) if uiCaptcha else None
  row=_cfg_row(cfg, cid)
  q=''
  bid=getattr(wnd,'backgroundId',None)
  if row:
   try: q=_txt(row.get('question'))
   except: q=''
   if bid is None:
    try: bid=row.get('bg')
    except: pass
  ans=MAP.get(cid)
  if getattr(constInfo,'_farmbot_cap_try',None)!=cid:
   constInfo._farmbot_cap_try=cid
   _log('map cid=%d bid=%r q=%r ans=%r how=%s'%(cid,bid,q,ans,how))
  done=getattr(constInfo,'_farmbot_captcha_done',None)
  if ans is None:
   if cid and getattr(constInfo,'_farmbot_cap_miss',None)!=cid:
    constInfo._farmbot_cap_miss=cid
    _log('miss cid=%d q=%r'%(cid,q))
  elif done!=cid:
   wnd.OnAreaClicked(int(ans))
   constInfo._farmbot_captcha_done=cid
   _log('ok cid=%d ans=%d q=%s how=%s'%(cid,int(ans),q.replace('\n',' '),how))
 def _install():
  if not uiCaptcha:
   _hooked(0)
   _log('install wait uiCaptcha')
   return 0
  cls=getattr(uiCaptcha,'CaptchaWindow',None)
  if not cls:
   _hooked(0)
   _log('install wait CaptchaWindow')
   return 0
  if getattr(cls,'_farmbot_hook',0):
   _hooked(1)
   return 1
  origSet=getattr(cls,'SetChallenge',None)
  origOpen=getattr(cls,'Open',None)
  origClose=getattr(cls,'Close',None)
  def _set(self,*a,**k):
   constInfo._farmbot_captcha_wnd=self
   r=origSet(self,*a,**k) if origSet else None
   try: _solve(self,'set')
   except: _log('solve set fail')
   return r
  def _open(self,*a,**k):
   constInfo._farmbot_captcha_wnd=self
   r=origOpen(self,*a,**k) if origOpen else None
   try: _solve(self,'open')
   except: _log('solve open fail')
   return r
  def _close(self,*a,**k):
   try:
    if getattr(constInfo,'_farmbot_captcha_wnd',None) is self:
     constInfo._farmbot_captcha_wnd=None
   except: pass
   _state(0)
   return origClose(self,*a,**k) if origClose else None
  try:
   if origSet: cls.SetChallenge=_set
   if origOpen: cls.Open=_open
   if origClose: cls.Close=_close
   cls._farmbot_hook=1
   _hooked(1)
   _log('hook installed event-driven')
  except Exception:
   _hooked(0)
   _log('hook fail')
   return 0
  # One-shot: if captcha already open, solve via interface attrs (no gc scan).
  wnd=getattr(constInfo,'_farmbot_captcha_wnd',None)
  how='hook'
  if not wnd:
   iface=None
   try: iface=constInfo.GetInterfaceInstance()
   except: iface=None
   if iface:
    for n in ('wndCaptcha','wndCatpcha','captchaWnd','captchaWatcher'):
     wnd=getattr(iface,n,None)
     if wnd:
      how='attr '+n
      constInfo._farmbot_captcha_wnd=wnd
      break
  if wnd:
   _solve(wnd, how)
  else:
   _state(0)
  return 1
 _install()
except Exception:
 try:
  f=open(r'C:\Temp\farmbot-captcha.log','ab'); f.write(traceback.format_exc()+'\r\n'); f.close()
 except: pass
 try:
  f=open(r'C:\Temp\farmbot-captcha.hooked','wb'); f.write('0'); f.close()
 except: pass
)PY";
    SafePyRunAfterPeek(kPyInstall);
    FILE* hf = nullptr;
    int hooked = 0;
    if (fopen_s(&hf, "C:\\Temp\\farmbot-captcha.hooked", "rb") == 0 && hf)
    {
        char c = 0;
        fread(&c, 1, 1, hf);
        fclose(hf);
        hooked = (c == '1') ? 1 : 0;
    }
    if (hooked)
    {
        g_captchaHooked = 1;
        NativeLog("captcha event hook armed phase=%s",
            g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str());
    }
    InterlockedExchange(&g_captchaBusy, 0);
}

static bool StartCombat(void* player, unsigned vid, float x, float y, bool click, bool autoAttack)
{
    (void)autoAttack;
    if (!NativeFarm())
    {
        g_chaseVid = 0;
        CallAttackKey(player, false);
        CallSetTarget(player, vid, 1);
        CallRva(kSetAutoRva, player, vid);
        CallRva(kReserveClickRva, player, vid);
        return true;
    }
    if (!click)
    {
        g_chaseVid = 0;
        CallSetTarget(player, vid, 1);
        return false;
    }
    if (g_baerim && g_chaseVid == vid && g_walkVid == vid)
    {
        g_chaseX = x;
        g_chaseY = y;
        return InterlockedCompareExchange(&g_allowMelee, 0, 0) != 0;
    }
    g_chaseVid = vid;
    g_chaseX = x;
    g_chaseY = y;
    g_stuckAt = 0;
    g_nativeSidesteps = 0;
    DriveToTarget(player, vid, x, y);
    return false;
}

static void SlashPath(char* path)
{
    for (int i = 0; path[i]; ++i)
    {
        if (path[i] == '\\')
            path[i] = '/';
    }
}

static void WritePotBmp(const char* path, unsigned char r, unsigned char g, unsigned char b)
{
    unsigned char hdr[54];
    memset(hdr, 0, 54);
    hdr[0] = 'B';
    hdr[1] = 'M';
    const unsigned size = 54 + 32 * 32 * 3;
    memcpy(hdr + 2, &size, 4);
    hdr[10] = 54;
    hdr[14] = 40;
    const unsigned dim = 32;
    memcpy(hdr + 18, &dim, 4);
    memcpy(hdr + 22, &dim, 4);
    hdr[26] = 1;
    hdr[28] = 24;
    FILE* f = nullptr;
    if (fopen_s(&f, path, "wb") != 0 || !f)
        return;
    fwrite(hdr, 1, 54, f);
    unsigned char px[3] = { b, g, r };
    for (int i = 0; i < 32 * 32; ++i)
        fwrite(px, 1, 3, f);
    fclose(f);
}

static const char* ClassifyPot(int type, int sub, int apply, unsigned vnum, const char* name)
{
    if (type == 1 || type == 2)
        return nullptr;
    char low[160];
    int w = 0;
    for (int i = 0; name && name[i] && w + 1 < 160; ++i)
        low[w++] = (char)tolower((unsigned char)name[i]);
    low[w] = 0;
    const int potName = strstr(low, "potion") || strstr(low, "iksir") ? 1 : 0;
    if (vnum >= 27100 && vnum <= 27102)
        return "green";
    if (vnum >= 27103 && vnum <= 27105)
        return "purple";
    if (potName && (strstr(low, "green") || strstr(low, "yesil") || strstr(low, "yeşil") ||
        strstr(low, "mov_speed") || strstr(low, "movement") || apply == 8))
        return "green";
    if (potName && (strstr(low, "purple") || strstr(low, "violet") || strstr(low, "att_speed") ||
        strstr(low, "attack speed") || strstr(low, "dxup") || strstr(low, "dx up") || apply == 7))
        return "purple";
    if ((vnum >= 27004 && vnum <= 27006) ||
        (potName && (strstr(low, "blue") || strstr(low, "mana") || strstr(low, "mavi") ||
            strstr(low, "sp potion") || strstr(low, "sp(") || strstr(low, "mp("))))
        return "mp";
    if ((vnum >= 27001 && vnum <= 27003) ||
        (potName && (strstr(low, "red") || strstr(low, "hp") || strstr(low, "life") ||
            strstr(low, "kirmizi") || strstr(low, "kırmızı") || strstr(low, "hp("))))
        return "hp";
    if (type == 3 && sub == 0 && potName)
        return "hp";
    return nullptr;
}

static bool DataAddr(const void* p)
{
    const uintptr_t a = (uintptr_t)p;
    return a >= 0x10000 && a < 0x7FFF0000;
}

static int LooksLikeTga(const unsigned char* p)
{
    if (!DataAddr(p))
        return 0;
    const unsigned char type = p[2];
    if (type != 2 && type != 10)
        return 0;
    const unsigned w = *(unsigned short*)(p + 12);
    const unsigned h = *(unsigned short*)(p + 14);
    const unsigned bpp = p[16];
    if (w < 8 || w > 2048 || h < 8 || h > 2048)
        return 0;
    if (bpp != 16 && bpp != 24 && bpp != 32)
        return 0;
    unsigned bytes = 18 + w * h * (bpp / 8) + 32;
    if (bytes < 64)
        bytes = 64;
    if (bytes > 65536)
        bytes = 65536;
    return (int)bytes;
}

static bool WriteBytes(const char* path, const void* data, int bytes)
{
    if (!path || !data || bytes <= 0)
        return false;
    FILE* f = nullptr;
    if (fopen_s(&f, path, "wb") != 0 || !f)
        return false;
    const size_t n = fwrite(data, 1, (size_t)bytes, f);
    fclose(f);
    return n == (size_t)bytes;
}

static bool FileBigEnough(const char* path, int minBytes)
{
    WIN32_FILE_ATTRIBUTE_DATA info;
    if (!GetFileAttributesExA(path, GetFileExInfoStandard, &info))
        return false;
    return info.nFileSizeLow >= (DWORD)minBytes && info.nFileSizeHigh == 0;
}

static bool PackGet(const char* virt, unsigned char* mapped, int mappedBytes);
static int NearbySize(const unsigned char* mapped, int mappedBytes, const unsigned char* data);

static int PngFileBytes(const unsigned char* p)
{
    int n = 0;
    __try
    {
        if (!p || p[0] != 0x89 || p[1] != 'P' || p[2] != 'N' || p[3] != 'G')
            return 0;
        int off = 8;
        for (int k = 0; k < 256; ++k)
        {
            const unsigned len = ((unsigned)p[off] << 24) | ((unsigned)p[off + 1] << 16) |
                ((unsigned)p[off + 2] << 8) | (unsigned)p[off + 3];
            if (len > 8u * 1024u * 1024u)
                return 0;
            if (p[off + 4] == 'I' && p[off + 5] == 'E' && p[off + 6] == 'N' && p[off + 7] == 'D')
            {
                n = off + 12 + (int)len;
                break;
            }
            off += 12 + (int)len;
            if (off <= 0 || off > 4 * 1024 * 1024)
                return 0;
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
    return n;
}

static bool ExtractPackBlob(const char* virt, const char* dest)
{
    unsigned char mapped[512];
    if (!virt || !virt[0] || !dest || !PackGet(virt, mapped, 512))
        return false;
    for (int i = 0; i + 4 <= 512; i += 4)
    {
        const unsigned char* p = *(const unsigned char**)(mapped + i);
        if (!DataAddr(p))
            continue;
        const int nearN = NearbySize(mapped, 400, p);
        const int png = PngFileBytes(p);
        if (png >= 32)
            return WriteBytes(dest, p, png);
        if (nearN >= 32)
        {
            int sig = 0;
            __try { sig = p[0] == 0x89 && p[1] == 'P' && p[2] == 'N' && p[3] == 'G' ? 1 : 0; }
            __except (EXCEPTION_EXECUTE_HANDLER) { sig = 0; }
            if (sig)
                return WriteBytes(dest, p, nearN);
        }
        const int tga = LooksLikeTga(p);
        if (tga > 0)
            return WriteBytes(dest, p, nearN > tga ? nearN : tga);
    }
    return false;
}

static bool ExtractPackFile(const char* virt, const char* dest)
{
    if (g_baerim)
        return ExtractPackBlob(virt, dest);
    if (!virt || !virt[0] || !dest || !g_base || !kPackGetRva || !kPackMgrRva)
        return false;
    if (FileBigEnough(dest, 64))
        return true;
    void* mgr = nullptr;
    __try { mgr = *(void**)(g_base + kPackMgrRva); }
    __except (EXCEPTION_EXECUTE_HANDLER) { mgr = nullptr; }
    if (!UserPtr(mgr))
        return false;
    typedef int (__thiscall* FnPackGet)(void* self, const char* name, unsigned len, void* mapped, int tryDisk);
    FnPackGet get = (FnPackGet)(g_base + kPackGetRva);
    if (IsBadCodePtr((FARPROC)get))
        return false;
    unsigned char mapped[320];
    memset(mapped, 0, sizeof(mapped));
    int ok = 0;
    __try { ok = get(mgr, virt, (unsigned)strlen(virt), mapped, 0); }
    __except (EXCEPTION_EXECUTE_HANDLER) { ok = 0; }
    if (!ok)
        return false;
    const unsigned char* data = nullptr;
    int bytes = 0;
    for (int i = 0; i + 4 <= 320; i += 4)
    {
        const unsigned char* p = *(const unsigned char**)(mapped + i);
        const int n = LooksLikeTga(p);
        if (n <= 0)
            continue;
        data = p;
        bytes = n;
        break;
    }
    if (!data)
        return false;
    return WriteBytes(dest, data, bytes);
}

static void* PackMgrPtr()
{
    void* mgr = nullptr;
    if (!g_base || !kPackMgrRva)
        return nullptr;
    __try { mgr = *(void**)(g_base + kPackMgrRva); }
    __except (EXCEPTION_EXECUTE_HANDLER) { mgr = nullptr; }
    return mgr;
}

static int PackExists(const char* virt)
{
    if (!virt || !virt[0] || !g_base || !kPackExistRva)
        return -1;
    void* mgr = PackMgrPtr();
    if (!UserPtr(mgr))
        return -1;
    typedef int (__thiscall* FnExist)(void* self, const char* name);
    FnExist exist = (FnExist)(g_base + kPackExistRva);
    if (IsBadCodePtr((FARPROC)exist))
        return -1;
    int ok = 0;
    __try { ok = exist(mgr, virt); }
    __except (EXCEPTION_EXECUTE_HANDLER) { return -1; }
    return ok ? 1 : 0;
}

static bool PackGet(const char* virt, unsigned char* mapped, int mappedBytes)
{
    if (!virt || !virt[0] || !mapped || mappedBytes < 64 || !g_base || !kPackGetRva || !kPackMgrRva)
        return false;
    void* mgr = PackMgrPtr();
    if (!UserPtr(mgr))
        return false;
    memset(mapped, 0, mappedBytes);
    int ok = 0;
    if (g_baerim)
    {
        if (kPackCtorRva)
        {
            typedef void (__thiscall* FnCtor)(void* self);
            FnCtor ctor = (FnCtor)(g_base + kPackCtorRva);
            if (!IsBadCodePtr((FARPROC)ctor))
            {
                __try { ctor(mapped); }
                __except (EXCEPTION_EXECUTE_HANDLER) { return false; }
            }
        }
        typedef int (__thiscall* FnMgrGet)(void* self, void* mappedFile, const char* name, const void** dataOut);
        FnMgrGet get = (FnMgrGet)(g_base + kPackGetRva);
        if (IsBadCodePtr((FARPROC)get))
            return false;
        const void* data = nullptr;
        __try { ok = get(mgr, mapped, virt, &data); }
        __except (EXCEPTION_EXECUTE_HANDLER) { ok = 0; data = nullptr; }
        if (ok && data && mappedBytes >= 8)
            *(const void**)(mapped + ((mappedBytes - 4) & ~3)) = data;
        return ok != 0;
    }
    typedef int (__thiscall* FnPackGet)(void* self, const char* name, unsigned len, void* mappedOut, int tryDisk);
    FnPackGet get = (FnPackGet)(g_base + kPackGetRva);
    if (IsBadCodePtr((FARPROC)get))
        return false;
    __try { ok = get(mgr, virt, (unsigned)strlen(virt), mapped, 0); }
    __except (EXCEPTION_EXECUTE_HANDLER) { ok = 0; }
    return ok != 0;
}

static int TextSpan(const unsigned char* p, int maxN)
{
    if (!DataAddr(p) || maxN < 12)
        return 0;
    int n = 0;
    __try
    {
        for (; n < maxN; ++n)
        {
            const unsigned char c = p[n];
            if (c == 0)
                break;
            if (c < 9 || (c > 13 && c < 32))
                break;
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
    return n;
}

static bool LooksLikeSubText(const unsigned char* p, int n)
{
    if (!p || n < 12)
        return false;
    char buf[220];
    const int take = n < 219 ? n : 219;
    memcpy(buf, p, (size_t)take);
    buf[take] = 0;
    for (int i = 0; buf[i]; ++i)
        buf[i] = (char)tolower((unsigned char)buf[i]);
    return strstr(buf, "subimage") != nullptr ||
        (strstr(buf, "title") && strstr(buf, "image"));
}

static int NearbySize(const unsigned char* mapped, int mappedBytes, const unsigned char* data)
{
    if (!mapped || !data)
        return 0;
    for (int i = 0; i + 4 <= mappedBytes; i += 4)
    {
        if (*(const unsigned char**)(mapped + i) != data)
            continue;
        const unsigned next = i + 8 <= mappedBytes ? *(unsigned*)(mapped + i + 4) : 0;
        const unsigned prev = i >= 4 ? *(unsigned*)(mapped + i - 4) : 0;
        if (next >= 12 && next <= 4u * 1024u * 1024u)
            return (int)next;
        if (prev >= 12 && prev <= 4u * 1024u * 1024u)
            return (int)prev;
    }
    return 0;
}

static bool ParseSubField(const char* text, const char* key, char* value, int cap)
{
    if (!text || !key || !value || cap < 2)
        return false;
    value[0] = 0;
    const char* p = text;
    const int keyLen = (int)strlen(key);
    while (*p)
    {
        while (*p == ' ' || *p == '\t' || *p == '\r' || *p == '\n')
            ++p;
        if (_strnicmp(p, key, keyLen) == 0 && (p[keyLen] == ' ' || p[keyLen] == '\t' || p[keyLen] == '\r' || p[keyLen] == '\n' || p[keyLen] == 0))
        {
            p += keyLen;
            while (*p == ' ' || *p == '\t')
                ++p;
            if (*p == '"' || *p == '\'')
                ++p;
            int n = 0;
            while (*p && *p != '\r' && *p != '\n' && *p != '"' && n < cap - 1)
                value[n++] = (char)tolower((unsigned char)*p++);
            while (n > 0 && (value[n - 1] == ' ' || value[n - 1] == '\t'))
                --n;
            value[n] = 0;
            return value[0] != 0;
        }
        while (*p && *p != '\n')
            ++p;
        if (*p == '\n')
            ++p;
    }
    return false;
}

static bool ParseSubImagePath(const char* text, char* image, int cap)
{
    if (!ParseSubField(text, "image", image, cap))
        return false;
    return strstr(image, ".tga") || strstr(image, ".dds") || strstr(image, ".sub");
}

static void JoinSearchPath(const char* subVirt, const char* image, char* out, int cap)
{
    if (!out || cap < 8)
        return;
    out[0] = 0;
    if (!image || !image[0])
        return;
    if (strchr(image, ':') || (image[0] == 'd' && image[1] == ':'))
    {
        strncpy_s(out, cap, image, _TRUNCATE);
        return;
    }
    if (image[0] == '/' || (image[0] == 'i' && image[1] == 'c'))
    {
        strncpy_s(out, cap, image, _TRUNCATE);
        return;
    }
    const char* slash = subVirt ? strrchr(subVirt, '/') : nullptr;
    if (!slash)
        slash = subVirt ? strrchr(subVirt, '\\') : nullptr;
    if (slash && subVirt)
    {
        int n = (int)(slash - subVirt + 1);
        if (n > 0 && n < cap - 1)
        {
            memcpy(out, subVirt, (size_t)n);
            out[n] = 0;
            strncat_s(out, cap, image, _TRUNCATE);
            return;
        }
    }
    _snprintf_s(out, cap, _TRUNCATE, "d:/ymir work/ui/%s", image);
}

static int DdsByteSize(const unsigned char* p)
{
    if (!DataAddr(p) || p[0] != 'D' || p[1] != 'D' || p[2] != 'S' || p[3] != ' ')
        return 0;
    const unsigned header = *(unsigned*)(p + 4);
    if (header != 124)
        return 0;
    const unsigned h = *(unsigned*)(p + 12);
    const unsigned w = *(unsigned*)(p + 16);
    if (w < 4 || w > 4096 || h < 4 || h > 4096)
        return 0;
    const unsigned pfFlags = *(unsigned*)(p + 80);
    const unsigned four = *(unsigned*)(p + 84);
    const unsigned bits = *(unsigned*)(p + 88);
    unsigned dataOff = 128;
    if (four == 0x30315844)
        dataOff = 148;
    const unsigned bx = (w + 3) / 4;
    const unsigned by = (h + 3) / 4;
    unsigned data = 0;
    if (pfFlags & 4)
        data = bx * by * (four == 0x31545844 ? 8u : 16u);
    else
    {
        const unsigned step = bits ? (bits + 7) / 8 : 4;
        data = w * h * step;
    }
    const unsigned total = dataOff + data;
    if (total < 128 || total > 8u * 1024u * 1024u)
        return 0;
    return (int)total;
}

static bool PackExtractBlob(const char* virt, const char* dest)
{
    if (!virt || !virt[0] || !dest)
        return false;
    unsigned char mapped[400];
    if (!PackGet(virt, mapped, 400))
        return false;
    const unsigned char* best = nullptr;
    int bestN = 0;
    int bestScore = 0;
    for (int i = 0; i + 4 <= 400; i += 4)
    {
        const unsigned char* p = *(const unsigned char**)(mapped + i);
        if (!DataAddr(p))
            continue;
        const int nTga = LooksLikeTga(p);
        const int nDds = DdsByteSize(p);
        const int nNear = NearbySize(mapped, 400, p);
        const int spanCap = nNear > 12 ? (nNear < 4096 ? nNear : 4096) : 800;
        const int nTxt = TextSpan(p, spanCap);
        int n = 0;
        int score = 0;
        if (nTga > 0)
        {
            n = nTga;
            score = 4;
        }
        else if (nDds > 0)
        {
            n = nNear >= nDds && nNear <= nDds * 2 ? nNear : nDds;
            score = 4;
        }
        else if (LooksLikeSubText(p, nTxt > 0 ? nTxt : nNear))
        {
            n = nNear >= 16 ? nNear : (nTxt >= 12 ? nTxt : 0);
            score = 3;
        }
        else if (nNear >= 16 && nNear <= 4 * 1024 * 1024 && nTxt >= 12)
        {
            n = nNear;
            score = 1;
        }
        if (score > bestScore || (score == bestScore && n > bestN))
        {
            best = p;
            bestN = n;
            bestScore = score;
            if (score >= 4)
                break;
        }
    }
    if (!best || bestN < 12)
        return false;
    return WriteBytes(dest, best, bestN);
}

static bool ExtractPackText(const char* virt, char* text, int cap)
{
    if (!text || cap < 16)
        return false;
    text[0] = 0;
    unsigned char mapped[400];
    if (!PackGet(virt, mapped, 400))
        return false;
    for (int i = 0; i + 4 <= 400; i += 4)
    {
        const unsigned char* p = *(const unsigned char**)(mapped + i);
        const int nNear = NearbySize(mapped, 400, p);
        const int n = TextSpan(p, nNear > 12 ? (nNear < cap - 1 ? nNear : cap - 1) : (cap - 1 < 800 ? cap - 1 : 800));
        if (n < 12)
            continue;
        const int take = n < cap - 1 ? n : cap - 1;
        memcpy(text, p, (size_t)take);
        text[take] = 0;
        return true;
    }
    return false;
}

static void AtlasDestPath(const char* dest, const char* image, char* atlasFile, int cap)
{
    if (!atlasFile || cap < 8)
        return;
    atlasFile[0] = 0;
    if (!dest || !image)
        return;
    strncpy_s(atlasFile, cap, dest, _TRUNCATE);
    char* slash = strrchr(atlasFile, '\\');
    if (!slash)
        slash = strrchr(atlasFile, '/');
    if (!slash)
        return;
    slash[1] = 0;
    const char* base = strrchr(image, '/');
    if (!base)
        base = strrchr(image, '\\');
    base = base ? base + 1 : image;
    strncat_s(atlasFile, cap, base, _TRUNCATE);
}

static bool ExtractAtlasBeside(const char* subVirt, const char* image, const char* dest)
{
    if (!image || !image[0] || !dest)
        return false;
    char atlasFile[MAX_PATH];
    AtlasDestPath(dest, image, atlasFile, MAX_PATH);
    if (!atlasFile[0])
        return false;
    if (FileBigEnough(atlasFile, 128) && (strstr(atlasFile, ".dds") || strstr(atlasFile, ".tga")))
        return true;
    char joined[260];
    JoinSearchPath(subVirt, image, joined, 260);
    char ui[260];
    _snprintf_s(ui, _TRUNCATE, "d:/ymir work/ui/%s", image);
    char uiSkill[260];
    _snprintf_s(uiSkill, _TRUNCATE, "d:/ymir work/ui/skill/%s", image);
    const char* tries[] = { joined, ui, uiSkill, image };
    for (int i = 0; i < 4; ++i)
    {
        if (!tries[i] || !tries[i][0])
            continue;
        if (PackExtractBlob(tries[i], atlasFile))
            return true;
        char low[260];
        int n = 0;
        for (; tries[i][n] && n < 259; ++n)
            low[n] = (char)tolower((unsigned char)tries[i][n]);
        low[n] = 0;
        if (PackExtractBlob(low, atlasFile))
            return true;
    }
    return false;
}

static bool ExtractSkillGraphic(const char* virt, const char* dest)
{
    if (!virt || !virt[0] || !dest)
        return false;
    char lower[260];
    int n = 0;
    for (; virt[n] && n < 259; ++n)
        lower[n] = (char)tolower((unsigned char)virt[n]);
    lower[n] = 0;
    DeleteFileA(dest);
    if (!PackExtractBlob(virt, dest) && !PackExtractBlob(lower, dest))
        return ExtractPackFile(virt, dest) || ExtractPackFile(lower, dest);
    char text[800];
    text[0] = 0;
    FILE* f = nullptr;
    if (fopen_s(&f, dest, "rb") == 0 && f)
    {
        const size_t got = fread(text, 1, 799, f);
        fclose(f);
        text[got] = 0;
    }
    if (LooksLikeTga((const unsigned char*)text) || DdsByteSize((const unsigned char*)text) > 0)
        return true;
    char image[260];
    if (!ParseSubImagePath(text, image, 260))
        return false;
    NativeLog("skill sub %s -> %s", virt, image);
    return ExtractAtlasBeside(virt, image, dest);
}

static void ReadItemIconPath(void* data, unsigned vnum, char* icon, int cap)
{
    if (!icon || cap < 8)
        return;
    icon[0] = 0;
    if (data)
        ReadStdString((char*)data + 0x64, icon, cap);
    if (!icon[0] && vnum)
        _snprintf_s(icon, cap, _TRUNCATE, "icon/item/%05u.tga", vnum);
}

static bool CopyIconFile(const char* src, const char* dst)
{
    if (!src || !src[0] || !dst)
        return false;
    FILE* in = nullptr;
    if (fopen_s(&in, src, "rb") != 0 || !in)
        return false;
    FILE* out = nullptr;
    if (fopen_s(&out, dst, "wb") != 0 || !out)
    {
        fclose(in);
        return false;
    }
    char buf[4096];
    size_t n;
    while ((n = fread(buf, 1, sizeof(buf), in)) > 0)
        fwrite(buf, 1, n, out);
    fclose(in);
    fclose(out);
    return true;
}

static void* ActorNode(void* node, unsigned off)
{
    return *(void**)((char*)node + off);
}

static int ActorNil(void* node)
{
    return !UserPtr(node) || *((BYTE*)node + kNodeIsNil) != 0;
}

static void* ActorSuccessor(void* node, void* head)
{
    if (!UserPtr(node) || node == head)
        return head;
    void* right = ActorNode(node, kNodeRight);
    if (!ActorNil(right))
    {
        void* n = right;
        while (!ActorNil(ActorNode(n, kNodeLeft)))
            n = ActorNode(n, kNodeLeft);
        return n;
    }
    void* cur = node;
    void* parent = ActorNode(cur, kNodeParent);
    for (int i = 0; i < 512 && UserPtr(parent) && parent != head; ++i)
    {
        if (cur != ActorNode(parent, kNodeRight))
            return parent;
        cur = parent;
        parent = ActorNode(cur, kNodeParent);
    }
    return head;
}

static int ReadActor(void* inst, unsigned vid, int skipType, char* json, int cap)
{
    if (!UserPtr(inst) || cap < 24)
        return 0;
    unsigned type = 0;
    unsigned vnum = 0;
    float x = 0, y = 0, z = 0;
    char name[64];
    name[0] = 0;
    __try
    {
        if (!vid && kSelfVidDisp)
            vid = *(unsigned*)((char*)inst + kSelfVidDisp);
        char* graphic = (char*)inst + kGraphicDisp;
        if (kActorTypeDisp)
        {
            type = *(unsigned*)(graphic + kActorTypeDisp);
            if (type > 10)
                type = *(unsigned char*)(graphic + kActorTypeDisp);
        }
        if (kVnumDisp)
            vnum = *(unsigned*)((char*)inst + kVnumDisp);
        else if (kRaceDisp)
            vnum = *(unsigned*)((char*)inst + kRaceDisp);
        x = *(float*)(graphic + kPixelXDisp);
        y = *(float*)(graphic + kPixelYDisp);
        z = kPixelZDisp ? *(float*)(graphic + kPixelZDisp) : 0.f;
        if (!vid)
        {
            const unsigned extras[2] = { 0x330, 0x32C };
            for (int i = 0; i < 2; ++i)
            {
                const unsigned guess = *(unsigned*)((char*)inst + extras[i]);
                if (guess && guess < 0x10000000)
                {
                    vid = guess;
                    break;
                }
            }
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
    if (kInstNameDisp)
        ReadStdString((char*)inst + kInstNameDisp, name, 64);
    if (!vid)
        return 0;
    if (kNegateY)
        y = -y;
    if (vnum >= 8000 && vnum < 9000)
        type = 2;
    else if (name[0])
    {
        for (const char* p = name; p[0] && p[1] && p[2] && p[3] && p[4]; ++p)
        {
            if ((p[0] | 32) == 'm' && (p[1] | 32) == 'e' && (p[2] | 32) == 't' &&
                (p[3] | 32) == 'i' && (p[4] | 32) == 'n')
            {
                type = 2;
                break;
            }
        }
    }
    if (type > 10)
        type = 0;
    (void)skipType;
    if (x != x || y != y || z != z || x > 2000000.f || x < -2000000.f || y > 2000000.f || y < -2000000.f)
    {
        x = 0;
        y = 0;
        z = 0;
    }
    char escaped[80];
    JsonEscape(name, escaped, 80);
    _snprintf_s(json, cap, _TRUNCATE,
        "{\"vid\":%u,\"type\":%u,\"vnum\":%u,\"x\":%.1f,\"y\":%.1f,\"z\":%.1f,\"name\":\"%s\"}",
        vid, type, vnum, x, y, z, escaped);
    return 1;
}

struct ActorScanOut
{
    char* buf;
    int cap;
    int len;
    int walked;
    int kept;
};

static int AppendActorJson(ActorScanOut* out, const char* row)
{
    if (!out || !out->buf || !row)
        return 0;
    const int n = (int)strlen(row);
    if (out->len + n + 2 >= out->cap)
        return 0;
    if (out->kept)
        out->buf[out->len++] = ',';
    memcpy(out->buf + out->len, row, (size_t)n);
    out->len += n;
    out->buf[out->len] = 0;
    return 1;
}

static int OutHasVid(ActorScanOut* out, unsigned vid)
{
    if (!out || !out->buf || !vid)
        return 0;
    char needle[40];
    _snprintf_s(needle, 40, _TRUNCATE, "{\"vid\":%u,", vid);
    return strstr(out->buf, needle) ? 1 : 0;
}

static int LooksLikeActorVector(void* mgr, unsigned disp)
{
    if (!mgr || !disp)
        return 0;
    __try
    {
        void** begin = *(void***)((char*)mgr + disp);
        void** endp = *(void***)((char*)mgr + disp + 4);
        if (!UserPtr(begin) || !UserPtr(endp) || endp <= begin)
            return 0;
        const unsigned count = (unsigned)(endp - begin);
        return count >= 1 && count <= 1024 ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static int InstanceOk(void* inst);
static void NoteIfMain(void* inst);

static int ScanActorsOnce(void* mgr, unsigned mapDisp, int skipType, ActorScanOut* out)
{
    if (!out || !out->buf || out->cap < 4 || !mgr || !mapDisp)
        return 0;
    out->walked = 0;
    out->kept = 0;
    out->len = 1;
    out->buf[0] = '[';
    out->buf[1] = 0;
    if (LooksLikeActorVector(mgr, mapDisp))
        return 0;
    unsigned vids[512];
    void* insts[512];
    unsigned n = 0;
    __try
    {
        void* head = *(void**)((char*)mgr + mapDisp + kMapHeadOff);
        if (!UserPtr(head))
            return 0;
        void* node = ActorNode(head, kNodeLeft);
        for (int i = 0; i < 512 && UserPtr(node) && node != head && n < 512; ++i)
        {
            if (!ActorNil(node))
            {
                vids[n] = *(unsigned*)((char*)node + kNodeKey);
                insts[n] = ActorNode(node, kNodeValue);
                ++n;
            }
            node = ActorSuccessor(node, head);
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
    int failOk = 0;
    int failRead = 0;
    int tEnemy = 0;
    int tStone = 0;
    int tPlayer = 0;
    for (unsigned i = 0; i < n && out->kept < 256; ++i)
    {
        ++out->walked;
        if (!vids[i] || !InstanceOk(insts[i]))
        {
            ++failOk;
            continue;
        }
        if (g_baerim)
            NoteIfMain(insts[i]);
        char row[320];
        if (!ReadActor(insts[i], vids[i], skipType, row, 320))
        {
            ++failRead;
            continue;
        }
        AppendActorJson(out, row);
        ++out->kept;
        if (strstr(row, "\"type\":2"))
            ++tStone;
        else if (strstr(row, "\"type\":0"))
            ++tEnemy;
        else if (strstr(row, "\"type\":6"))
            ++tPlayer;
    }
    if (g_baerim && n >= 8)
        g_scanUnstableMs = 0;
    static unsigned lastMap = 0;
    static int lastKept = -1;
    int delta = out->kept - lastKept;
    if (delta < 0)
        delta = -delta;
    if (mapDisp != lastMap || lastKept < 0 || delta >= 15)
    {
        lastMap = mapDisp;
        lastKept = out->kept;
        NativeLog("actors map +0x%X walked=%d kept=%d failOk=%d failRead=%d enemy=%d stone=%d player=%d",
            mapDisp, out->walked, out->kept, failOk, failRead, tEnemy, tStone, tPlayer);
    }
    return out->walked > 0 ? 1 : 0;
}

static int InstanceOk(void* inst)
{
    if (!UserPtr(inst))
        return 0;
    __try
    {
        BYTE* vt = *(BYTE**)inst;
        if (!UserPtr(vt))
            return 0;
        BYTE* fn = *(BYTE**)vt;
        return UserPtr(fn) ? 1 : 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
}

static void NoteIfMain(void* inst)
{
    if (!g_mainName[0] || !kGraphicDisp || !kActorTypeDisp || !kInstNameDisp)
        return;
    __try
    {
        char* graphic = (char*)inst + kGraphicDisp;
        unsigned type = *(unsigned char*)(graphic + kActorTypeDisp);
        if (type != 6)
            return;
        char name[64];
        name[0] = 0;
        if (!ReadStdString((char*)inst + kInstNameDisp, name, 64) || !name[0])
            return;
        if (_stricmp(name, g_mainName) == 0)
            g_mainInst = inst;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}

static int ScanActorVectorEx(void* mgr, unsigned vecDisp, int skipType, ActorScanOut* out, int append)
{
    (void)append;
    if (!out || !out->buf || out->cap < 4)
        return 0;
    out->walked = 0;
    out->kept = 0;
    out->len = 1;
    out->buf[0] = '[';
    out->buf[1] = 0;
    void* slots[1024];
    unsigned n = 0;
    __try
    {
        void** begin = *(void***)((char*)mgr + vecDisp);
        void** endp = *(void***)((char*)mgr + vecDisp + 4);
        if (!UserPtr(begin) || !UserPtr(endp) || endp < begin)
            return 0;
        unsigned count = (unsigned)(endp - begin);
        if (count == 0 || count > 4096)
            return 0;
        if (count > 1024)
            count = 1024;
        for (unsigned i = 0; i < count; ++i)
            slots[n++] = begin[i];
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return 0;
    }
    int failOk = 0;
    int failRead = 0;
    int tEnemy = 0;
    int tStone = 0;
    int tPlayer = 0;
    for (unsigned i = 0; i + 1 < n && out->kept < 512; i += 2)
    {
        ++out->walked;
        const unsigned vid = (unsigned)(uintptr_t)slots[i];
        void* inst = slots[i + 1];
        if (!vid || !LooksInstance(inst))
        {
            ++failOk;
            continue;
        }
        if (!g_mainInst)
            NoteIfMain(inst);
        char row[320];
        if (!ReadActor(inst, vid, skipType, row, 320))
        {
            ++failRead;
            continue;
        }
        AppendActorJson(out, row);
        ++out->kept;
        if (strstr(row, "\"type\":2"))
            ++tStone;
        else if (strstr(row, "\"type\":0"))
            ++tEnemy;
        else if (strstr(row, "\"type\":6"))
            ++tPlayer;
    }
    static int lastKept = -1;
    int delta = out->kept - lastKept;
    if (delta < 0)
        delta = -delta;
    if (lastKept < 0 || delta >= 15)
    {
        lastKept = out->kept;
        NativeLog("actors pairs walked=%d kept=%d failOk=%d failRead=%d enemy=%d stone=%d player=%d",
            out->walked, out->kept, failOk, failRead, tEnemy, tStone, tPlayer);
    }
    return out->walked > 0 ? 1 : 0;
}

static int ScanActorVector(void* mgr, unsigned vecDisp, int skipType, ActorScanOut* out)
{
    return ScanActorVectorEx(mgr, vecDisp, skipType, out, 0);
}

static void FinishActorJson(ActorScanOut* out)
{
    if (!out || !out->buf)
        return;
    if (out->len + 1 < out->cap)
    {
        out->buf[out->len++] = ']';
        out->buf[out->len] = 0;
    }
}

static void ScanActors()
{
    static char buf[256 * 1024];
    static unsigned lastWalked = 0;
    static unsigned lastKept = 0;
    static DWORD lastAt = 0;
    const DWORD now = GetTickCount();
    if (lastAt != 0 && now - lastAt < 800 && g_actorsJson.size() > 2)
        return;
    lastAt = now;
    if (g_baerim && !g_sessionCharacter.empty())
        strncpy_s(g_mainName, g_sessionCharacter.c_str(), _TRUNCATE);
    void* mgr = CharMgr();
    if (!mgr)
    {
        NativeLog("actors scan skip mgr=null graphic=0x%X", kGraphicDisp);
        return;
    }
    ActorScanOut out;
    out.buf = buf;
    out.cap = (int)sizeof(buf);
    const char* kind = "map";
    unsigned usedDisp = LiveMapDisp();
    if (g_baerim)
    {
        kind = "pairs";
        usedDisp = 0x84;
        ScanActorVectorEx(mgr, 0x84, 0, &out, 0);
    }
    else
    {
        if (!usedDisp)
        {
            NativeLog("actors scan skip mgr=%p map=0", mgr);
            return;
        }
        if (!ScanActorsOnce(mgr, usedDisp, 1, &out) || out.kept == 0)
            ScanActorsOnce(mgr, usedDisp, 0, &out);
    }
    FinishActorJson(&out);
    if (out.kept > 0 || g_actorsJson.size() <= 2)
        g_actorsJson = buf;
    int delta = out.kept - (int)lastKept;
    if (delta < 0)
        delta = -delta;
    if (lastKept == 0 || delta >= 15)
    {
        lastWalked = (unsigned)out.walked;
        lastKept = (unsigned)out.kept;
        NativeLog("actors scan %s map=0x%X graphic=0x%X type=+0x%X walked=%d kept=%d json=%d",
            kind, usedDisp, kGraphicDisp, kActorTypeDisp, out.walked, out.kept, out.len);
    }
}

static void ScanPots()
{
    g_potsJson = "[]";
    if (g_baerim)
        return;
    HMODULE py = PythonDll();
    const int nativeInv = kGetItemIndexRva ? 1 : 0;
    NativeLog("pots scan begin py=%p nativeIndex=0x%X nativeCount=0x%X itemMgr=0x%X",
        py, kGetItemIndexRva, kGetItemCountRva, kItemMgrRva);

    int gil = 0;
    int haveGil = 0;
    void* playerMod = nullptr;
    void* itemMod = nullptr;
    if (!nativeInv && py)
    {
        haveGil = GilEnter(&gil);
        playerMod = PyMod("player");
        itemMod = PyMod("item");
    }
    else if (!nativeInv && !py)
        return;
    int seen = 0;
    int kept = 0;
    struct Acc
    {
        unsigned vnum;
        int count;
        int slot;
        int duration;
        std::string name;
        std::string family;
        std::string icon;
        std::string tip;
    };
    std::map<unsigned, Acc> byVnum;
    char dir[MAX_PATH];
    GetTempPathA(MAX_PATH, dir);
    char iconDir[MAX_PATH];
    _snprintf_s(iconDir, _TRUNCATE, "%sfarmbot-icons", dir);
    CreateDirectoryA(iconDir, nullptr);

    if (nativeInv || playerMod)
    {
        for (int i = 0; i < 180; ++i)
        {
            unsigned vnum = nativeInv ? NativeItemIndex(i) : PyItemIndex(playerMod, i);
            if (!vnum || vnum > 2000000)
                continue;
            ++seen;
            int count = nativeInv ? NativeItemCount(i) : PyItemCount(playerMod, i);
            char name[160] = { 0 };
            char icon[260] = { 0 };
            int type = 0, sub = 0, apply = 0, aval = 0, v0 = 0, v1 = 0;
            void* itemData = nullptr;
            if (nativeInv)
                itemData = NativeItemInfo(vnum, name, 160, &type, &sub, &apply, &aval, &v0, &v1);
            else
            {
                PySlotName(playerMod, i, name, 160);
                if (itemMod)
                {
                    PyDec(PyCall(itemMod, "SelectItem", "i", (int)vnum));
                    if (!name[0])
                        PyAsStr(PyCall(itemMod, "GetItemName", nullptr), name, 160);
                    type = (int)PyAsInt(PyCall(itemMod, "GetItemType", nullptr));
                    sub = (int)PyAsInt(PyCall(itemMod, "GetItemSubType", nullptr));
                    void* aff = PyCall(itemMod, "GetAffect", "i", 0);
                    if (aff)
                    {
                        ParseAffectPair(aff, &apply, &aval);
                        PyDec(aff);
                    }
                    if (apply == 0)
                    {
                        void* aff1 = PyCall(itemMod, "GetAffect", "i", 1);
                        if (aff1)
                        {
                            ParseAffectPair(aff1, &apply, &aval);
                            PyDec(aff1);
                        }
                    }
                    v1 = (int)PyAsInt(PyCall(itemMod, "GetValue", "i", 1));
                    PyAsStr(PyCall(itemMod, "GetIconImageFileName", nullptr), icon, 260);
                }
            }
            if (!icon[0])
                ReadItemIconPath(itemData, vnum, icon, 260);
            const char* fam = ClassifyPot(type, sub, apply, vnum, name);
            NativeLog("pot slot=%d vnum=%u type=%d sub=%d apply=%d name=%s fam=%s",
                i, vnum, type, sub, apply, name, fam ? fam : "-");
            if (!fam)
                continue;
            Acc& acc = byVnum[vnum];
            if (acc.vnum == 0)
            {
                acc.vnum = vnum;
                acc.slot = i;
                acc.name = name[0] ? name : "potion";
                acc.family = fam;
                int dur = v1;
                if (dur < 15 || dur > 600)
                    dur = aval;
                if (dur >= 15 && dur <= 600)
                    acc.duration = dur;
                char tip[240];
                if (acc.family == "hp" && v0 > 0)
                    _snprintf_s(tip, _TRUNCATE, "%s | HP +%d", acc.name.c_str(), v0);
                else if (acc.family == "mp" && v0 > 0)
                    _snprintf_s(tip, _TRUNCATE, "%s | MP +%d", acc.name.c_str(), v0);
                else if (acc.family == "green")
                    _snprintf_s(tip, _TRUNCATE, "%s | Movement Speed%s%d | %ds",
                        acc.name.c_str(), aval ? " +" : " ", aval, acc.duration ? acc.duration : 90);
                else if (acc.family == "purple")
                    _snprintf_s(tip, _TRUNCATE, "%s | Attack Speed%s%d | %ds",
                        acc.name.c_str(), aval ? " +" : " ", aval, acc.duration ? acc.duration : 90);
                else
                    _snprintf_s(tip, _TRUNCATE, "%s", acc.name.c_str());
                acc.tip = tip;
                char tga[MAX_PATH];
                char bmp[MAX_PATH];
                _snprintf_s(tga, _TRUNCATE, "%s\\%u.tga", iconDir, vnum);
                _snprintf_s(bmp, _TRUNCATE, "%s\\%u.bmp", iconDir, vnum);
                if (icon[0] && ExtractPackFile(icon, tga))
                {
                    acc.icon = tga;
                    NativeLog("icon pack vnum=%u path=%s", vnum, icon);
                }
                else if (icon[0] && CopyIconFile(icon, tga))
                    acc.icon = tga;
                else
                {
                    unsigned char r = 196, g = 92, b = 92;
                    if (acc.family == "mp") { r = 74; g = 126; b = 196; }
                    else if (acc.family == "green") { r = 74; g = 160; b = 92; }
                    else if (acc.family == "purple") { r = 140; g = 92; b = 180; }
                    WritePotBmp(bmp, r, g, b);
                    acc.icon = bmp;
                    NativeLog("icon fallback vnum=%u path=%s", vnum, icon);
                }
            }
            acc.count += count > 0 ? count : 1;
            ++kept;
        }
    }
    PyDec(playerMod);
    PyDec(itemMod);
    GilLeave(haveGil, gil);

    std::string json = "[";
    bool first = true;
    for (std::map<unsigned, Acc>::iterator it = byVnum.begin(); it != byVnum.end(); ++it)
    {
        const Acc& acc = it->second;
        char nameEsc[192];
        char iconEsc[MAX_PATH * 2];
        JsonEscape(acc.name.c_str(), nameEsc, 192);
        JsonEscape(acc.icon.c_str(), iconEsc, (int)sizeof(iconEsc));
        char item[900];
        char tipEsc[280];
        JsonEscape(acc.tip.c_str(), tipEsc, 280);
        _snprintf_s(item, _TRUNCATE,
            "%s{\"v\":%u,\"n\":\"%s\",\"c\":%d,\"s\":%d,\"f\":\"%s\",\"i\":\"%s\",\"d\":%d,\"t\":\"%s\"}",
            first ? "" : ",",
            acc.vnum, nameEsc, acc.count, acc.slot, acc.family.c_str(), iconEsc, acc.duration, tipEsc);
        json += item;
        first = false;
    }
    json += "]";
    g_potsJson = json;
    NativeLog("pots scan py=%p native=%d player=%d seen=%d kept=%d json=%u",
        py, nativeInv, playerMod ? 1 : 0, seen, kept, (unsigned)g_potsJson.size());
}

typedef int (__thiscall* FnSkillSlotI)(void* self, unsigned slot);
typedef float (__thiscall* FnSkillCool)(void* self, unsigned slot);
typedef void (__thiscall* FnClickSkill)(void* self, unsigned slot);
typedef int (__thiscall* FnSendUseSkill)(void* self, unsigned index, unsigned vid);
typedef int (__thiscall* FnGetSkillData)(void* self, unsigned index, void** out);
typedef int (__thiscall* FnSkillPred)(void* data);
typedef int (__thiscall* FnIsMounting)(void* self);
typedef int (__thiscall* FnIsAffect)(void* self, unsigned type);
typedef void (__thiscall* FnSendChat)(void* self, const char* text, unsigned char type);

static void* SkillMgr()
{
    if (!g_base || !kSkillMgrRva)
        return nullptr;
    void* p = nullptr;
    __try { p = *(void**)(g_base + kSkillMgrRva); }
    __except (EXCEPTION_EXECUTE_HANDLER) { p = nullptr; }
    return UserPtr(p) ? p : nullptr;
}

static int NativeSkillIndex(unsigned slot)
{
    void* player = Player();
    if (!player || !kGetSkillIndexRva || !g_base)
        return 0;
    FnSkillSlotI fn = (FnSkillSlotI)(g_base + kGetSkillIndexRva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    int v = 0;
    __try { v = fn(player, slot); }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    return v;
}

static int NativeSkillLevel(unsigned slot)
{
    void* player = Player();
    if (!player || !kGetSkillLevelRva || !g_base)
        return 0;
    FnSkillSlotI fn = (FnSkillSlotI)(g_base + kGetSkillLevelRva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    int v = 0;
    __try { v = fn(player, slot); }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    return v;
}

static int NativeIsSkillCool(unsigned slot)
{
    void* player = Player();
    if (!player || !kIsSkillCoolRva || !g_base)
        return 0;
    FnSkillSlotI fn = (FnSkillSlotI)(g_base + kIsSkillCoolRva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    int v = 0;
    __try { v = fn(player, slot); }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    return v;
}

// Highlight / aSkill[slot].bActive — toggle currently on.
static int NativeIsSkillActive(unsigned slot)
{
    void* player = Player();
    if (!player || !kIsSkillActiveRva || !g_base)
        return -1;
    FnSkillSlotI fn = (FnSkillSlotI)(g_base + kIsSkillActiveRva);
    if (IsBadCodePtr((FARPROC)fn))
        return -1;
    int v = 0;
    __try { v = fn(player, slot); }
    __except (EXCEPTION_EXECUTE_HANDLER) { return -1; }
    return v ? 1 : 0;
}

static int NativeSkillCoolSec(unsigned slot)
{
    void* player = Player();
    if (!player || !kGetSkillCoolRva || !g_base)
        return 0;
    FnSkillCool fn = (FnSkillCool)(g_base + kGetSkillCoolRva);
    if (IsBadCodePtr((FARPROC)fn))
        return 0;
    float v = 0;
    __try { v = fn(player, slot); }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    if (v <= 0 || v > 3600)
        return 0;
    return (int)(v + 0.5f);
}

static void* NativeSkillData(unsigned index)
{
    void* mgr = SkillMgr();
    if (!mgr || !kGetSkillDataRva || !g_base)
        return nullptr;
    FnGetSkillData fn = (FnGetSkillData)(g_base + kGetSkillDataRva);
    if (IsBadCodePtr((FARPROC)fn))
        return nullptr;
    void* data = nullptr;
    int ok = 0;
    __try { ok = fn(mgr, index, &data); }
    __except (EXCEPTION_EXECUTE_HANDLER) { ok = 0; data = nullptr; }
    return ok && UserPtr(data) ? data : nullptr;
}

static int NativeSkillPred(unsigned rva, void* data)
{
    if (!rva || !data || !g_base)
        return -1;
    FnSkillPred fn = (FnSkillPred)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return -1;
    int v = 0;
    __try { v = fn(data); }
    __except (EXCEPTION_EXECUTE_HANDLER) { return -1; }
    return v ? 1 : 0;
}

static int SafeByte(void* p)
{
    int v = 0;
    if (!UserPtr(p))
        return 0;
    __try { v = *(unsigned char*)p; }
    __except (EXCEPTION_EXECUTE_HANDLER) { v = 0; }
    return v;
}

static bool SafeStdString(void* str, char* out, int cap)
{
    bool ok = false;
    if (!out || cap < 2)
        return false;
    out[0] = 0;
    __try { ok = ReadStdString(str, out, cap); }
    __except (EXCEPTION_EXECUTE_HANDLER) { ok = false; out[0] = 0; }
    return ok;
}

static void SanitizeName(char* name)
{
    if (!name)
        return;
    int n = 0;
    for (char* p = name; *p; ++p, ++n)
    {
        const unsigned char c = (unsigned char)*p;
        if (c < 32)
        {
            *p = 0;
            break;
        }
    }
    if (n < 1)
        name[0] = 0;
}

static bool LooksLikeSkillPath(const char* s)
{
    if (!s || !s[0])
        return false;
    char low[260];
    int i = 0;
    for (; s[i] && i < 259; ++i)
        low[i] = (char)tolower((unsigned char)s[i]);
    low[i] = 0;
    const int graphic = strstr(low, ".sub") || strstr(low, ".dds") || strstr(low, ".tga");
    if (!graphic)
        return false;
    return strstr(low, "skill") || strstr(low, "icon") || strstr(low, "ymir") ||
        strstr(low, "ui/") || strstr(low, "ui\\");
}

static bool ReadImageFileName(void* img, char* out, int cap)
{
    if (!UserPtr(img) || !out || cap < 8)
        return false;
    out[0] = 0;
    static const int kOff[] = { 8, 12, 16, 20, 24, 28, 32, 36, 48, 64 };
    for (int i = 0; i < (int)(sizeof(kOff) / sizeof(kOff[0])); ++i)
    {
        if (SafeStdString((char*)img + kOff[i], out, cap) && LooksLikeSkillPath(out))
            return true;
        out[0] = 0;
    }
    return false;
}

static void* ReadPtr(void* base, int off)
{
    void* p = nullptr;
    if (!UserPtr(base))
        return nullptr;
    __try { p = *(void**)((char*)base + off); }
    __except (EXCEPTION_EXECUTE_HANDLER) { p = nullptr; }
    return UserPtr(p) ? p : nullptr;
}

static const char* JobFolder(const char* job)
{
    if (!job || !job[0])
        return "";
    if (_stricmp(job, "WARRIOR") == 0) return "warrior";
    if (_stricmp(job, "ASSASSIN") == 0) return "assassin";
    if (_stricmp(job, "SURA") == 0) return "sura";
    if (_stricmp(job, "SHAMAN") == 0) return "shaman";
    if (_stricmp(job, "WOLFMAN") == 0) return "wolfman";
    if (_stricmp(job, "PASSIVE") == 0) return "passive";
    if (_stricmp(job, "78SKILL") == 0) return "78skill";
    if (_stricmp(job, "SUPPORT") == 0) return "common/support";
    if (_stricmp(job, "GUILD") == 0) return "common/guild";
    if (_stricmp(job, "HORSE") == 0) return "common/horse";
    return job;
}

struct SkillDescIcon
{
    unsigned index;
    char job[32];
    char name[64];
};

static SkillDescIcon g_desc[384];
static int g_descN;
static int g_descTried;

static void ParseSkillDescFile(const char* path)
{
    FILE* f = nullptr;
    if (fopen_s(&f, path, "rb") != 0 || !f)
        return;
    char line[1024];
    while (fgets(line, (int)sizeof(line), f) && g_descN < 384)
    {
        char* p = line;
        while (*p == ' ' || *p == '\t' || *p == '\r' || (unsigned char)*p == 0xEF ||
            (unsigned char)*p == 0xBB || (unsigned char)*p == 0xBF)
            ++p;
        if (*p == '#' || *p == 0)
            continue;
        char* tok[16];
        int n = 0;
        tok[n++] = p;
        for (char* c = p; *c && n < 16; ++c)
        {
            if (*c == '\t')
            {
                *c = 0;
                tok[n++] = c + 1;
            }
            else if (*c == '\r' || *c == '\n')
            {
                *c = 0;
                break;
            }
        }
        if (n <= 12)
            continue;
        const unsigned index = (unsigned)atoi(tok[0]);
        if (index == 0 || !tok[1][0] || !tok[12][0])
            continue;
        SkillDescIcon& row = g_desc[g_descN++];
        row.index = index;
        strncpy_s(row.job, tok[1], _TRUNCATE);
        strncpy_s(row.name, tok[12], _TRUNCATE);
        for (char* c = row.job; *c; ++c)
            if (*c == '\r' || *c == '\n')
                *c = 0;
        for (char* c = row.name; *c; ++c)
        {
            *c = (char)tolower((unsigned char)*c);
            if (*c == '\r' || *c == '\n' || *c == '.')
                *c = 0;
        }
    }
    fclose(f);
}

static void LoadSkillDescOnce()
{
    if (g_descTried)
        return;
    g_descTried = 1;
    char dir[MAX_PATH];
    GetTempPathA(MAX_PATH, dir);
    char iconDir[MAX_PATH];
    _snprintf_s(iconDir, _TRUNCATE, "%sfarmbot-icons", dir);
    CreateDirectoryA(iconDir, nullptr);
    char path[MAX_PATH];
    _snprintf_s(path, _TRUNCATE, "%s\\skilldesc.txt", iconDir);
    static const char* kFiles[] = {
        "locale/en/SkillDesc.txt", "locale/ae/SkillDesc.txt", "locale/ro/SkillDesc.txt",
        "locale/de/SkillDesc.txt", "locale/tr/SkillDesc.txt", "locale/pt/SkillDesc.txt",
        "locale/pl/SkillDesc.txt", "locale/hu/SkillDesc.txt", "locale/cz/SkillDesc.txt",
        "locale/es/SkillDesc.txt", "locale/it/SkillDesc.txt", "locale/fr/SkillDesc.txt",
        "locale/nl/SkillDesc.txt", "locale/gr/SkillDesc.txt", "locale/ru/SkillDesc.txt",
        "locale/en/skilldesc.txt", "locale/ae/skilldesc.txt", "SkillDesc.txt"
    };
    for (int i = 0; i < (int)(sizeof(kFiles) / sizeof(kFiles[0])); ++i)
    {
        DeleteFileA(path);
        if (!PackExtractBlob(kFiles[i], path) || !FileBigEnough(path, 64))
            continue;
        ParseSkillDescFile(path);
        NativeLog("skilldesc %s rows=%d", kFiles[i], g_descN);
        if (g_descN > 0)
            return;
    }
}

static bool SkillDescVirt(unsigned index, int variant, char* out, int cap)
{
    LoadSkillDescOnce();
    for (int i = 0; i < g_descN; ++i)
    {
        if (g_desc[i].index != index || !g_desc[i].name[0])
            continue;
        const char* folder = JobFolder(g_desc[i].job);
        if (!folder || !folder[0])
            folder = "shaman";
        if (variant == 0)
            _snprintf_s(out, cap, _TRUNCATE, "d:/ymir work/ui/skill/%s/%s_01.sub", folder, g_desc[i].name);
        else
            _snprintf_s(out, cap, _TRUNCATE, "d:/ymir work/ui/skill/%s/%s.sub", folder, g_desc[i].name);
        return true;
    }
    return false;
}

static int PathRank(const char* s)
{
    if (!s || !s[0])
        return 0;
    char low[260];
    int i = 0;
    for (; s[i] && i < 259; ++i)
        low[i] = (char)tolower((unsigned char)s[i]);
    low[i] = 0;
    if (strstr(low, ".sub"))
        return 3;
    if (strstr(low, ".dds"))
        return 2;
    if (strstr(low, ".tga") && (strstr(low, "skill") || strstr(low, "icon")))
        return 1;
    return 0;
}

static void CollectSkillIcon(void* data, unsigned index, char* icon, int cap)
{
    if (!icon || cap < 8)
        return;
    icon[0] = 0;
    int rank = 0;
    static int dumped;
    if (data)
    {
        for (int off = 0x10; off <= 0x800; off += 4)
        {
            char s[260];
            if (SafeStdString((char*)data + off, s, 260) && LooksLikeSkillPath(s))
            {
                const int r = PathRank(s);
                if (r > rank)
                {
                    strncpy_s(icon, cap, s, _TRUNCATE);
                    rank = r;
                    if (r >= 3)
                        break;
                }
            }
        }
        if (rank < 3)
        {
            for (int off = 0x20; off <= 0x800; off += 4)
            {
                void* img = ReadPtr(data, off);
                char s[260];
                if (!ReadImageFileName(img, s, 260))
                    continue;
                const int r = PathRank(s);
                if (r > rank)
                {
                    strncpy_s(icon, cap, s, _TRUNCATE);
                    rank = r;
                    if (r >= 3)
                        break;
                }
            }
        }
        if (dumped < 2)
        {
            NativeLog("skill icon src index=%u data=%p path=%s", index, data, icon);
            for (int off = 0x10; off <= 0x400; off += 4)
            {
                char s[260];
                if (SafeStdString((char*)data + off, s, 260) && s[0])
                    NativeLog("skill str +0x%X %s", off, s);
            }
            ++dumped;
        }
    }
    if (rank < 3)
    {
        char fromDesc[260];
        if (SkillDescVirt(index, 0, fromDesc, 260))
            strncpy_s(icon, cap, fromDesc, _TRUNCATE);
    }
}

static void StemFromPath(const char* virt, char* stem, int cap)
{
    if (!stem || cap < 2)
        return;
    stem[0] = 0;
    if (!virt || !virt[0])
        return;
    const char* slash = strrchr(virt, '/');
    const char* bslash = strrchr(virt, '\\');
    const char* file = virt;
    if (slash && slash + 1 > file)
        file = slash + 1;
    if (bslash && bslash + 1 > file)
        file = bslash + 1;
    strncpy_s(stem, cap, file, _TRUNCATE);
    char* dot = strrchr(stem, '.');
    if (dot)
        *dot = 0;
    const int n = (int)strlen(stem);
    if (n > 3 && stem[n - 3] == '_' && stem[n - 2] >= '0' && stem[n - 2] <= '9' &&
        stem[n - 1] >= '0' && stem[n - 1] <= '9')
        stem[n - 3] = 0;
}

static bool TryExtractSkillIcon(const char* virt, unsigned index, const char* dest)
{
    if (virt && virt[0] && ExtractSkillGraphic(virt, dest))
        return true;
    char alt[260];
    for (int v = 0; v < 2; ++v)
    {
        if (SkillDescVirt(index, v, alt, 260) && ExtractSkillGraphic(alt, dest))
            return true;
    }
    static const char* kJobs[] = {
        "warrior", "assassin", "sura", "shaman", "wolfman", "passive",
        "common", "common/support", "common/guild", "common/horse", "78skill"
    };
    char stem[64];
    StemFromPath(virt, stem, 64);
    if (stem[0] && ((stem[0] >= 'a' && stem[0] <= 'z') || (stem[0] >= 'A' && stem[0] <= 'Z')))
    {
        for (int j = 0; j < (int)(sizeof(kJobs) / sizeof(kJobs[0])); ++j)
        {
            _snprintf_s(alt, _TRUNCATE, "d:/ymir work/ui/skill/%s/%s_01.sub", kJobs[j], stem);
            if (ExtractSkillGraphic(alt, dest))
                return true;
            _snprintf_s(alt, _TRUNCATE, "d:/ymir work/ui/skill/%s/%s.sub", kJobs[j], stem);
            if (ExtractSkillGraphic(alt, dest))
                return true;
        }
    }
    _snprintf_s(alt, _TRUNCATE, "icon/skill/%05u.tga", index);
    if ((!virt || _stricmp(virt, alt) != 0) && ExtractSkillGraphic(alt, dest))
        return true;
    return false;
}

static bool SkillJunk(unsigned idx, const char* name, int type)
{
    if (type == 3)
        return true;
    if (idx >= 121 && idx <= 124)
        return true;
    if (!name || !name[0])
        return false;
    char low[160];
    int i = 0;
    for (; name[i] && i < 159; ++i)
        low[i] = (char)tolower((unsigned char)name[i]);
    low[i] = 0;
    return strstr(low, "mining") || strstr(low, "language") || strstr(low, "combo") ||
        strstr(low, "fishing") || strstr(low, "farming") || strstr(low, "leadership");
}

static int ActorTypeOf(void* inst)
{
    if (!UserPtr(inst) || !kGraphicDisp || !kActorTypeDisp)
        return -1;
    unsigned type = 0;
    __try
    {
        char* graphic = (char*)inst + kGraphicDisp;
        type = *(unsigned*)(graphic + kActorTypeDisp);
        if (type > 10)
            type = *(unsigned char*)(graphic + kActorTypeDisp);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return -1;
    }
    return (int)type;
}

static int CallIsMounted()
{
    // Baerim: player.IsMountingHorse → GetMainActor → *(BYTE*)(main+0x30).
    // Recovered "native" 0x41D7E0 is a Python helper and must not be thiscall'd.
    if (g_baerim)
    {
        void* main = CallGetMainActor();
        if (UserPtr(main))
        {
            __try
            {
                return *((BYTE*)main + 0x30) ? 1 : 0;
            }
            __except (EXCEPTION_EXECUTE_HANDLER)
            {
            }
        }
        return 0;
    }
    void* player = Player();
    if (player && kIsMountingRva && g_base && (kIsMountingRva < 0x410000 || kIsMountingRva > 0x420000))
    {
        FnIsMounting fn = (FnIsMounting)(g_base + kIsMountingRva);
        if (!IsBadCodePtr((FARPROC)fn))
        {
            void* selves[2];
            FillPlayerSelves(player, selves);
            for (int i = 0; i < 2; ++i)
            {
                if (!selves[i] || (i == 1 && selves[1] == selves[0]))
                    continue;
                __try
                {
                    return fn(selves[i]) ? 1 : 0;
                }
                __except (EXCEPTION_EXECUTE_HANDLER)
                {
                }
            }
        }
    }
    return ActorTypeOf(CallGetMainActor()) == 8 ? 1 : 0;
}

// Buff toggle re-cast is handled entirely in SkillMountPlanner (always double-click).
// No Python affect hooks / IsSkillActive polling — those crashed Baerim and never worked.
static HMODULE g_selfModule = nullptr;
static const int kMaxWatchAffects = 64;
static int g_watchAffectTypes[kMaxWatchAffects];
static int g_watchAffectCount;
static std::string g_castRemovedJson = "[]";

static void ClearAffectMirror()
{
    g_affectsJson = "[]";
    g_castRemovedJson = "[]";
}

static void WatchSkill(unsigned slot, int index)
{
    (void)slot;
    if (index <= 0 || g_watchAffectCount >= kMaxWatchAffects)
        return;
    for (int i = 0; i < g_watchAffectCount; ++i)
    {
        if (g_watchAffectTypes[i] == index)
            return;
    }
    g_watchAffectTypes[g_watchAffectCount++] = index;
}

static void WatchAffectType(int type)
{
    WatchSkill(0, type);
}

extern "C" __declspec(dllexport) void FarmBot_NoteAffect(int type, int present)
{
    (void)type;
    (void)present;
}

static void PublishNoteAffectPtr()
{
}

static void RefreshAffectsReadonly()
{
    g_affectsJson = "[]";
    g_castRemovedJson = "[]";
}

static unsigned EffectiveSendChatRva()
{
    // Baerim native SendChatPacket is thiscall ret 12: (text, type, scratch).
    // Reject Python helpers recovered in 0x41xxxx–0x42xxxx; layout recover unwraps the real RVA.
    if (g_baerim && kSendChatRva >= 0x410000 && kSendChatRva < 0x430000)
        return 0;
    return kSendChatRva;
}

static bool CallSendChatNative(const char* text)
{
    if (!text || !text[0] || !g_base)
        return false;
    void* net = NetStream();
    if (!net)
        return false;
    const unsigned chatRva = EffectiveSendChatRva();
    if (!chatRva)
        return false;
    if (g_baerim)
    {
        typedef void (__thiscall* FnSendChat3)(void* self, const char* msg, unsigned type, char* scratch);
        FnSendChat3 fn = (FnSendChat3)(g_base + chatRva);
        if (IsBadCodePtr((FARPROC)fn))
            return false;
        char scratch[32];
        memset(scratch, 0, sizeof(scratch));
        __try
        {
            fn(net, text, 0, scratch);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            return false;
        }
    }
    FnSendChat fn = (FnSendChat)(g_base + chatRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(net, text, 0);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static bool CallSendChat(const char* text)
{
    if (!text || !text[0])
        return false;
    // Native first — Python Peek can report success without the ride applying mid-skill.
    if (CallSendChatNative(text))
        return true;
    char code[192];
    _snprintf_s(code, _TRUNCATE, "import net\nnet.SendChatPacket('%s', 0)\n", text);
    if (SafePyRunAfterPeek(code) != 0)
        return true;
    _snprintf_s(code, _TRUNCATE, "import net\nnet.SendChatPacket('%s')\n", text);
    return SafePyRunAfterPeek(code) != 0;
}

// Remount: native /ride only. Python SendChat + GIL from the PeekMessage drain
// path can freeze the client after a few mount cycles.
static bool CallRideRemount()
{
    return CallSendChatNative("/ride");
}

static bool CallClickSkill(unsigned slot)
{
    void* player = Player();
    if (!player || !kClickSkillRva || !g_base)
        return false;
    FnClickSkill fn = (FnClickSkill)(g_base + kClickSkillRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    // Baerim ClickSkillSlot: ecx = player (not +4). Never fall back to SafePyRun —
    // GIL/ctypes from the game thread causes delayed CRT aborts.
    if (g_baerim)
    {
        __try
        {
            fn(player, slot);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            return false;
        }
    }
    void* selves[2];
    FillPlayerSelves(player, selves);
    for (int i = 0; i < 2; ++i)
    {
        if (!selves[i] || (i == 1 && selves[1] == selves[0]))
            continue;
        __try
        {
            fn(selves[i], slot);
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
    char code[96];
    _snprintf_s(code, _TRUNCATE, "import player\nplayer.ClickSkillSlot(%u)\n", slot);
    return SafePyRunAfterPeek(code) != 0;
}

static bool CallSendUseSkill(unsigned index, unsigned vid)
{
    void* net = NetStream();
    if (!net || !kSendUseSkillRva || !g_base)
        return false;
    FnSendUseSkill fn = (FnSendUseSkill)(g_base + kSendUseSkillRva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    __try
    {
        fn(net, index, vid);
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static void ScanSkills()
{
    const DWORD now = GetTickCount();
    if (g_chaseVid && g_skillsJson.size() > 2)
        return;
    // Baerim: after first good scan, reuse JSON for 3 minutes. Repeated GetSkillData /
    // IsToggleSkill probes were a prime suspect for delayed CRT heap aborts.
    const DWORD minGap = g_baerim ? 180000u : 10000u;
    if (g_skillsReady && g_lastSkillsMs && now - g_lastSkillsMs < minGap && g_skillsJson.size() > 2)
        return;
    g_skillsJson = "[]";
    g_watchAffectCount = 0;
    if (!kGetSkillIndexRva)
        return;
    const int extractIcons = !g_baerim && !g_skillsReady;
    if (!g_skillsReady)
        NativeLog("skills scan begin getIndex=0x%X getLevel=0x%X skillMgr=0x%X getData=0x%X",
            kGetSkillIndexRva, kGetSkillLevelRva, kSkillMgrRva, kGetSkillDataRva);
    char dir[MAX_PATH];
    GetTempPathA(MAX_PATH, dir);
    char iconDir[MAX_PATH];
    _snprintf_s(iconDir, _TRUNCATE, "%sfarmbot-icons", dir);
    CreateDirectoryA(iconDir, nullptr);
    std::string json = "[";
    bool first = true;
    int seen = 0;
    int kept = 0;
    for (unsigned slot = 0; slot < 255; ++slot)
    {
        const int index = NativeSkillIndex(slot);
        const int level = NativeSkillLevel(slot);
        if (index <= 0 || level <= 0)
            continue;
        ++seen;
        void* data = nullptr;
        char name[160] = { 0 };
        char icon[260] = { 0 };
        int type = 0;
        // Baerim: skip GetSkillData / name walks / IsToggle on rescans — index+level enough.
        const int needData = !g_baerim || !g_skillsReady;
        if (needData)
        {
            data = NativeSkillData((unsigned)index);
            type = data ? SafeByte(data) : 0;
            if (data)
            {
                for (int off = 0x10; off <= 0x40; off += 4)
                {
                    if (SafeStdString((char*)data + off, name, 160) && name[0] && !LooksLikeSkillPath(name))
                        break;
                    name[0] = 0;
                }
            }
        }
        SanitizeName(name);
        if (SkillJunk((unsigned)index, name, type))
            continue;
        WatchSkill(slot, index);
        const char* kind = "skill";
        if (!name[0])
            _snprintf_s(name, _TRUNCATE, "skill %d", index);
        int toggle = 0;
        int cool = 0;
        int duration = 0;
        if (!g_baerim)
        {
            if (data && kIsToggleRva)
            {
                const int t = NativeSkillPred(kIsToggleRva, data);
                if (t > 0)
                    toggle = 1;
            }
            if (extractIcons)
                CollectSkillIcon(data, (unsigned)index, icon, 260);
            const int coolDur = NativeSkillCoolSec(slot);
            if (NativeIsSkillCool(slot))
                cool = coolDur > 0 ? coolDur : 1;
            duration = coolDur > 0 ? coolDur : 0;
        }
        char tga[MAX_PATH];
        _snprintf_s(tga, _TRUNCATE, "%s\\skill-%u.sub", iconDir, (unsigned)index);
        char iconPath[MAX_PATH];
        iconPath[0] = 0;
        if (extractIcons && TryExtractSkillIcon(icon, (unsigned)index, tga))
            strncpy_s(iconPath, tga, _TRUNCATE);
        char nameEsc[192];
        char iconEsc[MAX_PATH * 2];
        JsonEscape(name, nameEsc, 192);
        JsonEscape(iconPath, iconEsc, (int)sizeof(iconEsc));
        char item[900];
        _snprintf_s(item, _TRUNCATE,
            "%s{\"x\":%d,\"s\":%u,\"n\":\"%s\",\"k\":\"%s\",\"l\":%d,\"c\":%d,\"r\":0,\"d\":%d,\"i\":\"%s\",\"t\":%d,\"a\":0}",
            first ? "" : ",",
            index, slot, nameEsc, kind, level, cool, duration, iconEsc, toggle);
        json += item;
        first = false;
        ++kept;
    }
    json += "]";
    g_skillsJson = json;
    g_skillsReady = kept > 0 ? 1 : g_skillsReady;
    g_lastSkillsMs = GetTickCount();
    if (kept > 0)
        NativeLog("skills scan seen=%d kept=%d json=%u", seen, kept, (unsigned)g_skillsJson.size());
}

static bool PyUseItem(unsigned cell)
{
    char code[96];
    _snprintf_s(code, _TRUNCATE, "import net\nnet.SendItemUsePacket(%u)\n", cell);
    return SafePyRun(code) != 0;
}

static void Execute(const Command& cmd)
{
    g_lastOk = false;
    g_lastReason = "not executed";
    g_lastVid = 0;
    if (cmd.type == "get_phase" || cmd.type == "get_session" || cmd.type == "skip_intro" ||
        cmd.type == "login" || cmd.type == "select_character" || cmd.type == "enter_game")
        RefreshSession();

    if (cmd.type == "dump_captcha_bg")
    {
        if (!g_baerim)
        {
            g_lastOk = false;
            g_lastReason = "dump bg is baerim-only";
            return;
        }
        DumpCaptchaBegin();
        return;
    }

    if (cmd.type == "loot_loop")
    {
        g_lootFast = (cmd.vid & 1) ? 1 : 0;
        g_lootRanged = (cmd.vid & 2) ? 1 : 0;
        g_lootOn = (g_lootFast || g_lootRanged) ? 1 : 0;
        g_lootDelay = cmd.x >= 1.f ? (DWORD)cmd.x : 50;
        if (g_baerim && g_lootDelay < 400)
            g_lootDelay = 400;
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

    if (cmd.type == "get_pots")
    {
        if (!g_inWorld)
        {
            g_potsJson = "[]";
            g_lastOk = true;
            g_lastReason = "not in world";
            return;
        }
        ScanPots();
        g_lastOk = true;
        g_lastReason = "pots";
        return;
    }

    if (cmd.type == "get_skills")
    {
        if (!g_inWorld)
        {
            g_skillsJson = "[]";
            g_lastOk = true;
            g_lastReason = "not in world";
            return;
        }
        ScanSkills();
        g_lastOk = true;
        g_lastReason = "skills";
        return;
    }

    if (cmd.type == "use_item")
    {
        RefreshSession();
        if (!g_inWorld)
        {
            g_lastReason = "not in world";
            return;
        }
        const unsigned cell = cmd.slot >= 0 ? (unsigned)cmd.slot : cmd.vid;
        NativeLog("use_item cell=%u useItem=0x%X sendUse=0x%X", cell, kUseItemRva, kSendItemUseRva);
        const int used = CallSendItemUse(cell) ? 1 : 0;
        const int playerUse = !used && CallUseItem(cell) ? 1 : 0;
        const int pyUse = !used && !playerUse && PyUseItem(cell) ? 1 : 0;
        NativeLog("use_item cell=%u send=%d player=%d py=%d", cell, used, playerUse, pyUse);
        if (used || playerUse || pyUse)
        {
            g_lastOk = true;
            g_lastReason = "SendItemUsePacket";
            g_lastVid = cell;
        }
        else
            g_lastReason = "SendItemUsePacket missing";
        return;
    }

    if (cmd.type == "use_skill")
    {
        RefreshSession();
        if (!g_inWorld)
        {
            g_lastReason = "not in world";
            return;
        }
        const unsigned slot = cmd.slot >= 0 ? (unsigned)cmd.slot : 0;
        const unsigned vid = cmd.vid;
        const int index = NativeSkillIndex(slot);
        WatchSkill(slot, index);
        NativeLog("use_skill slot=%u index=%d vid=%u click=0x%X send=0x%X",
            slot, index, vid, kClickSkillRva, kSendUseSkillRva);
        const int clicked = CallClickSkill(slot) ? 1 : 0;
        NativeLog("use_skill slot=%u click=%d", slot, clicked);
        if (clicked)
        {
            g_lastOk = true;
            g_lastReason = "ClickSkillSlot";
            g_lastVid = vid ? vid : (unsigned)index;
        }
        else
            g_lastReason = "skill use missing";
        return;
    }

    if (cmd.type == "is_skill_active")
    {
        RefreshSession();
        if (!g_inWorld)
        {
            g_lastReason = "not in world";
            return;
        }
        const unsigned slot = cmd.slot >= 0 ? (unsigned)cmd.slot : 0;
        // Native only — Python ctypes callbacks into the inject DLL crash Baerim CRT.
        const int active = NativeIsSkillActive(slot);
        if (active < 0)
        {
            g_lastOk = false;
            g_lastVid = 0;
            g_lastReason = "IsSkillActive missing";
            NativeLog("is_skill_active slot=%u fail rva=0x%X", slot, kIsSkillActiveRva);
            return;
        }
        g_lastOk = true;
        g_lastVid = (unsigned)(active ? 1 : 0);
        g_lastReason = active ? "active" : "inactive";
        NativeLog("is_skill_active slot=%u active=%d", slot, active);
        return;
    }

    if (cmd.type == "toggle_mount")
    {
        RefreshSession();
        if (!g_inWorld)
        {
            g_lastReason = "not in world";
            return;
        }
        // Baerim mount: /ride only. Slot 1 = dismount, 2 = remount.
        // Directed: skip if already in the desired state so retries never toggle the wrong way.
        g_mounted = CallIsMounted();
        const int mode = cmd.slot;
        const int wantMounted = (mode == 2) ? 1 : 0;
        if (g_mounted == wantMounted)
        {
            g_lastOk = true;
            g_lastReason = "already desired mount state";
            NativeLog("toggle_mount skip mode=%d already mounted=%d", mode, g_mounted);
            return;
        }
        // Both directions: native /ride only (no Python/GIL on the game-thread drain).
        int ok = 0;
        const char* via = "none";
        if (mode == 2)
        {
            if (CallRideRemount())
            {
                ok = 1;
                via = "ride-remount";
            }
        }
        else if (CallSendChatNative("/ride"))
        {
            ok = 1;
            via = "ride";
        }
        NativeLog("toggle_mount ok=%d mode=%d via=%s mounted=%d chat=0x%X",
            ok, mode, via, g_mounted, EffectiveSendChatRva());
        if (ok)
        {
            g_lastOk = true;
            g_lastReason = "SendChatPacket mount";
        }
        else
            g_lastReason = "mount toggle missing";
        return;
    }

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
        RefreshSession();
        // Skip GetStatus on login/select UI. Once in-world (or character after select),
        // never WindowClassIs — that GIL path from the HUD poll crashed ~1min in.
        if (g_baerim && !g_inWorld && !g_openedGameWindow)
        {
            const int onUi =
                PhaseHas("Login") || PhaseHas("Select") || PhaseHas("Logo") || PhaseHas("Intro") ||
                (PhaseHas("Load") && g_sessionCharacter.empty() && !g_selectSent) ||
                PyPhaseWindowIs("LoginWindow") || PyPhaseWindowIs("SelectCharacterWindow");
            if (onUi)
            {
                g_hudOk = false;
                ClearAffectMirror();
                g_lastOk = true;
                g_lastReason = !g_sessionPhase.empty() ? g_sessionPhase : "login ui";
                return;
            }
        }
        RefreshHud(player);
        if (g_inWorld || (g_hudOk && g_hudMaxHp > 0 && !g_sessionCharacter.empty()))
        {
            g_inWorld = 1;
            g_openedGameWindow = 1;
            g_sessionPhase = "Game";
            RefreshAffectsReadonly();
        }
        else
            ClearAffectMirror();
        g_lastOk = g_hudOk;
        g_lastReason = g_hudOk ? "hud" : (g_getStatusSlot < 0 ? "GetStatus vtable missing" : "GetStatus empty");
        return;
    }

    if (cmd.type == "get_entities")
    {
        if (!g_inWorld)
            g_inWorld = ComputeInWorld();
        if (!g_inWorld)
        {
            // One more chance: HUD already proves we are in game (Baerim HandShake lie).
            if (g_hudOk && g_hudMaxHp > 0 && !g_sessionCharacter.empty())
            {
                g_inWorld = 1;
                g_openedGameWindow = 1;
                g_sessionPhase = "Game";
            }
            else
            {
                g_chaseVid = 0;
                g_walkVid = 0;
                InterlockedExchange(&g_allowMelee, 0);
                g_actorsJson = "[]";
                g_lastOk = true;
                g_lastReason = "not in world";
                return;
            }
        }
        ScanActors();
        g_lastOk = true;
        g_lastReason = "entities";
        return;
    }

    if (cmd.type == "move")
    {
        PauseChase(player, 1600);
        // Entity JSON applies negateY for C#; MoveToPixel wants raw client pixels.
        const float mx = cmd.x;
        const float my = (g_baerim && kNegateY) ? -cmd.y : cmd.y;
        const bool walked = cmd.vid
            ? CallMoveToDest(cmd.vid, mx, my)
            : CallMoveToPixel(mx, my);
        NativeLog("BOT sidestep xy=%.0f,%.0f raw=%.0f,%.0f pixel=0x%X dest=0x%X ok=%d mode=%u",
            cmd.x, cmd.y, mx, my, kMoveToPixelRva, kMoveToDestRva, walked ? 1 : 0, BaerimReadU32(player, 0x54));
        if (!walked)
        {
            g_lastOk = false;
            g_lastReason = kMoveToPixelRva ? "MoveToDestPosition failed" : "MoveToDestPosition missing";
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
            if (!NativeFarm() && !CallGetInstance(hit.vid))
                continue;
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
            else if (NativeFarm() &&
                     CallSetTarget(player, hit.vid, 1) &&
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
        NativeLog("BOT cmd=%s vid=%u xy=%.0f,%.0f arthion=%d baerim=%d", cmd.type.c_str(), cmd.vid, cmd.x, cmd.y, g_arthion, g_baerim);
        const void* inst = CallGetInstance(cmd.vid);
        if (!NativeFarm())
        {
            if (!CallSetTarget(player, cmd.vid, 1))
            {
                g_lastReason = "SetTarget vtable missing";
                return;
            }
            StartCombat(player, cmd.vid, cmd.x, cmd.y, cmd.type == "attack" || cmd.type == "press_actor", false);
            g_lastVid = ReadTargetVid(player);
            g_lastOk = g_lastVid == cmd.vid;
            if (g_lastOk)
                g_lastReason = inst ? "target+approach+auto-attack" : "selected (instance lookup missed)";
            else
                g_lastReason = inst ? "SetTarget did not stick" : "VID not in live instance map";
            return;
        }
        const bool click = cmd.type == "attack" || cmd.type == "press_actor";
        if (!inst)
        {
            g_chaseVid = 0;
            g_lastOk = false;
            g_lastReason = "VID not live";
            return;
        }
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
        g_lastReason = "reserve-click walk";
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

static void BaerimBlockAutoLogin()
{
    // Only kill the client's logininfo auto-Connect. Never clear isAutoSelect —
    // that flag is how SelectCharacterWindow.Open() presses Enter (stock introLogin).
    if (!g_baerim || g_baerimAutoOff)
        return;
    if (!PythonDll())
        return;
    RefreshSession();
    if (PhaseHas("Game") || PhaseHas("Load") || PhaseHas("Select") || PhaseHas("Hand") || g_loginSent)
    {
        g_baerimAutoOff = 1;
        return;
    }
    void* handler = PythonHandler(NetStream());
    if (!handler)
        return;
    SafePySetInt(handler, "isAutoLogin", 0);
    g_baerimAutoOff = 1;
    NativeLog("baerim auto-Connect disabled phase=%s (isAutoSelect kept)",
        g_sessionPhase.empty() ? "(empty)" : g_sessionPhase.c_str());
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
    if (g_baerim)
        BaerimBlockAutoLogin();
    SafeDrain();
    MaybeAutoLoot(Player());
}

static void AfterPeek()
{
    CaptchaTick();
    DumpCaptchaStep();
}

static BOOL WINAPI HookPeekA(LPMSG msg, HWND hwnd, UINT min, UINT max, UINT remove)
{
    OnLoopPump(hwnd, min, max);
    const BOOL r = g_peekA(msg, hwnd, min, max, remove);
    AfterPeek();
    return r;
}

static BOOL WINAPI HookPeekW(LPMSG msg, HWND hwnd, UINT min, UINT max, UINT remove)
{
    OnLoopPump(hwnd, min, max);
    const BOOL r = g_peekW(msg, hwnd, min, max, remove);
    AfterPeek();
    return r;
}

static BOOL WINAPI HookGetA(LPMSG msg, HWND hwnd, UINT min, UINT max)
{
    OnLoopPump(hwnd, min, max);
    const BOOL r = g_getA(msg, hwnd, min, max);
    AfterPeek();
    return r;
}

static BOOL WINAPI HookGetW(LPMSG msg, HWND hwnd, UINT min, UINT max)
{
    OnLoopPump(hwnd, min, max);
    const BOOL r = g_getW(msg, hwnd, min, max);
    AfterPeek();
    return r;
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
    if (g_origAttackKey)
        g_origAttackKey(self, press);
}

static void __fastcall HookReserveClick(void* self, void*, unsigned vid)
{
    if (g_origReserveClick)
        g_origReserveClick(self, vid);
}

static void __fastcall HookSetTarget(void* self, void*, unsigned vid, int force)
{
    if (g_origSetTarget)
        g_origSetTarget(self, vid, force);
}

static void __fastcall HookCombo(void* self, void*)
{
    if (NativeFarm() && g_chaseVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) == 0)
        return;
    if (g_origCombo)
        g_origCombo(self);
}

static void __fastcall GuardCombo(void* self, void*)
{
    if (NativeFarm() && g_chaseVid && InterlockedCompareExchange(&g_allowMelee, 0, 0) == 0)
        return;
    FnVoid combo = (FnVoid)(g_base + kComboRva);
    if (combo && !IsBadCodePtr((FARPROC)combo))
        combo(self);
}

static void PatchUpdateComboCall()
{
    if (!NativeFarm() || !g_base || !kPlayerUpdateRva || !kComboRva || g_updateComboCall)
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
    if (g_origChase)
        g_origChase(self);
}

static int __fastcall HookInRange(void* me, void*, const void* you)
{
    return g_origInRange ? g_origInRange(me, you) : 0;
}

static int __fastcall HookMoveToDest(void* me, void*, const void* dest)
{
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
        if (!g_baerim && kPlayerUpdateRva && Trampoline((BYTE*)client + kPlayerUpdateRva, (void*)HookUpdate, (void**)&g_playerUpdate))
            ++g_iatHooks;
        if (!g_baerim && kSetLoginInfoRva && Trampoline((BYTE*)client + kSetLoginInfoRva, (void*)HookSetLoginInfo, (void**)&g_setLoginInfo))
            ++g_iatHooks;
        if (!NativeFarm())
            return;
        if (g_baerim)
        {
            NativeLog("hook AttackKey=0 Reserve=0 SetTarget=0 Combo=0 Chase=0 InRange=0 MoveTo=0 Update=0 baerim click-only");
            return;
        }
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
    if (!ReadFile(g_pipe, &length, 4, &read, nullptr) || read != 4 || length == 0 || length > 512 * 1024)
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
    kDirectEnterModeSetRva = 0;
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
    kSendItemUseRva = 0;
    kGetItemIndexRva = 0;
    kGetItemIndexSlot = 0;
    kGetItemCountRva = 0;
    kGetItemCountSlot = 0;
    kSelectItemRva = 0;
    kItemMgrRva = 0;
    kGetSelectedRva = 0;
    kGetItemNameRva = 0;
    kGetItemTypeRva = 0;
    kGetItemSubTypeRva = 0;
    kGetAffectRva = 0;
    kGetValueRva = 0;
    kGetIconImageRva = 0;
    kPackMgrRva = 0;
    kPackGetRva = 0;
    kPackCtorRva = 0;
    kPackExistRva = 0;
    kResMgrRva = 0;
    kGetResRva = 0;
    kUseItemRva = 0;
    kGetSkillIndexRva = 0;
    kGetSkillLevelRva = 0;
    kClickSkillRva = 0;
    kGetSkillCoolRva = 0;
    kIsSkillCoolRva = 0;
    kIsSkillActiveRva = 0;
    kSendUseSkillRva = 0;
    kSkillMgrRva = 0;
    kGetSkillDataRva = 0;
    kIsStandingRva = 0;
    kIsToggleRva = 0;
    kIsNeedTargetRva = 0;
    kIsAttackSkillRva = 0;
    kIsMountingRva = 0;
    kIsAffectRva = 0;
    kSendChatRva = 0;
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
    kMousePressRva = 0;
    kChaseRva = 0;
    kApproachInstRva = 0;
    kAttackDestRva = 0;
    kInstAttackRva = 0;
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
        else if (strncmp(line, "profile=", 8) == 0)
        {
            g_arthion = _strnicmp(line + 8, "arthion", 7) == 0 ? 1 : 0;
            g_baerim = _strnicmp(line + 8, "baerim", 6) == 0 ? 1 : 0;
            // Stock DirectEnterModeSet RVA is a crash landmine on Baerim.
            if (g_baerim)
                kDirectEnterModeSetRva = 0;
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
        else if (strncmp(line, "mousePress=", 11) == 0)
            kMousePressRva = ParseHex(line + 11);
        else if (strncmp(line, "chase=", 6) == 0)
            kChaseRva = ParseHex(line + 6);
        else if (strncmp(line, "approachInst=", 13) == 0)
            kApproachInstRva = ParseHex(line + 13);
        else if (strncmp(line, "attackDest=", 11) == 0)
            kAttackDestRva = ParseHex(line + 11);
        else if (strncmp(line, "instAttack=", 11) == 0)
            kInstAttackRva = ParseHex(line + 11);
        else if (strncmp(line, "getPixel=", 9) == 0)
            kGetPixelRva = ParseHex(line + 9);
        else if (strncmp(line, "inRange=", 8) == 0)
            kInRangeRva = ParseHex(line + 8);
        else if (strncmp(line, "pickClose=", 10) == 0)
            kPickCloseRva = ParseHex(line + 10);
        else if (strncmp(line, "getStatusSlot=", 14) == 0)
            kGetStatusSlot = ParseHex(line + 14);
        else if (strncmp(line, "getStatus=", 10) == 0)
            kGetStatusRva = ParseHex(line + 10);
        else if (strncmp(line, "getNameSlot=", 12) == 0)
            kGetNameSlot = ParseHex(line + 12);
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
        else if (strncmp(line, "sendUse=", 8) == 0)
            kSendItemUseRva = ParseHex(line + 8);
        else if (strncmp(line, "getItemIndexSlot=", 17) == 0)
            kGetItemIndexSlot = ParseHex(line + 17);
        else if (strncmp(line, "getItemIndex=", 13) == 0)
            kGetItemIndexRva = ParseHex(line + 13);
        else if (strncmp(line, "getItemCountSlot=", 17) == 0)
            kGetItemCountSlot = ParseHex(line + 17);
        else if (strncmp(line, "getItemCount=", 13) == 0)
            kGetItemCountRva = ParseHex(line + 13);
        else if (strncmp(line, "selectItem=", 11) == 0)
            kSelectItemRva = ParseHex(line + 11);
        else if (strncmp(line, "itemMgr=", 8) == 0)
            kItemMgrRva = ParseHex(line + 8);
        else if (strncmp(line, "getSelected=", 12) == 0)
            kGetSelectedRva = ParseHex(line + 12);
        else if (strncmp(line, "getItemName=", 12) == 0)
            kGetItemNameRva = ParseHex(line + 12);
        else if (strncmp(line, "getItemType=", 12) == 0)
            kGetItemTypeRva = ParseHex(line + 12);
        else if (strncmp(line, "getItemSubType=", 15) == 0)
            kGetItemSubTypeRva = ParseHex(line + 15);
        else if (strncmp(line, "getAffect=", 10) == 0)
            kGetAffectRva = ParseHex(line + 10);
        else if (strncmp(line, "getValue=", 9) == 0)
            kGetValueRva = ParseHex(line + 9);
        else if (strncmp(line, "getIconImage=", 13) == 0)
            kGetIconImageRva = ParseHex(line + 13);
        else if (strncmp(line, "packMgr=", 8) == 0)
            kPackMgrRva = ParseHex(line + 8);
        else if (strncmp(line, "packGet=", 8) == 0)
            kPackGetRva = ParseHex(line + 8);
        else if (strncmp(line, "packCtor=", 9) == 0)
            kPackCtorRva = ParseHex(line + 9);
        else if (strncmp(line, "packExist=", 10) == 0)
            kPackExistRva = ParseHex(line + 10);
        else if (strncmp(line, "useItem=", 8) == 0)
            kUseItemRva = ParseHex(line + 8);
        else if (strncmp(line, "getSkillIndex=", 14) == 0)
            kGetSkillIndexRva = ParseHex(line + 14);
        else if (strncmp(line, "getSkillLevel=", 14) == 0)
            kGetSkillLevelRva = ParseHex(line + 14);
        else if (strncmp(line, "clickSkill=", 11) == 0)
            kClickSkillRva = ParseHex(line + 11);
        else if (strncmp(line, "getSkillCool=", 13) == 0)
            kGetSkillCoolRva = ParseHex(line + 13);
        else if (strncmp(line, "isSkillCool=", 12) == 0)
            kIsSkillCoolRva = ParseHex(line + 12);
        else if (strncmp(line, "isSkillActive=", 14) == 0)
            kIsSkillActiveRva = ParseHex(line + 14);
        else if (strncmp(line, "sendUseSkill=", 13) == 0)
            kSendUseSkillRva = ParseHex(line + 13);
        else if (strncmp(line, "skillMgr=", 9) == 0)
            kSkillMgrRva = ParseHex(line + 9);
        else if (strncmp(line, "getSkillData=", 13) == 0)
            kGetSkillDataRva = ParseHex(line + 13);
        else if (strncmp(line, "isStanding=", 11) == 0)
            kIsStandingRva = ParseHex(line + 11);
        else if (strncmp(line, "isToggle=", 9) == 0)
            kIsToggleRva = ParseHex(line + 9);
        else if (strncmp(line, "isNeedTarget=", 13) == 0)
            kIsNeedTargetRva = ParseHex(line + 13);
        else if (strncmp(line, "isAttackSkill=", 14) == 0)
            kIsAttackSkillRva = ParseHex(line + 14);
        else if (strncmp(line, "isMounting=", 11) == 0)
            kIsMountingRva = ParseHex(line + 11);
        else if (strncmp(line, "isAffect=", 9) == 0)
            kIsAffectRva = ParseHex(line + 9);
        else if (strncmp(line, "sendChat=", 9) == 0)
            kSendChatRva = ParseHex(line + 9);
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
        else if (strncmp(line, "pixelZ=", 7) == 0)
            kPixelZDisp = ParseHex(line + 7);
        else if (strncmp(line, "actorType=", 10) == 0)
            kActorTypeDisp = ParseHex(line + 10);
        else if (strncmp(line, "race=", 5) == 0)
            kRaceDisp = ParseHex(line + 5);
        else if (strncmp(line, "vnum=", 5) == 0)
            kVnumDisp = ParseHex(line + 5);
        else if (strncmp(line, "instName=", 9) == 0)
            kInstNameDisp = ParseHex(line + 9);
        else if (strncmp(line, "selfVid=", 8) == 0)
            kSelfVidDisp = ParseHex(line + 8);
        else if (strncmp(line, "negateY=", 8) == 0)
            kNegateY = ParseHex(line + 8) ? 1 : 0;
        else if (strncmp(line, "loginId=", 8) == 0)
            kLoginIdDisp = ParseHex(line + 8);
        else if (strncmp(line, "loginPwd=", 9) == 0)
            kLoginPasswordDisp = ParseHex(line + 9);
    }
    fclose(f);
    if (g_baerim && !kGetNameSlot)
        kGetNameSlot = 0x14;
    if (g_baerim && !kSelfVidDisp)
        kSelfVidDisp = 0x334;
    if (!NativeFarm())
    {
        kSingletonRva = 0x482398;
        kCharMgrRva = 0x48239C;
        kNewAttackRva = 0xD0C70;
        kComboRva = 0;
        kNetStreamRva = 0x482BA0;
        kAccountConnectorRva = 0x484D24;
        kSetLoginInfoRva = 0x11C390;
        kAccountSetLoginInfoRva = 0x117660;
        kAccountConnectRva = 0x117510;
        kConnectGameServerRva = 0x11BD40;
        kSelectCharacterRva = 0x13DC60;
        kSetLoginPhaseRva = 0x13D240;
        kSetGamePhaseRva = 0x1367F0;
        kStartGameRva = 0x13CD40;
        kNotifyGamePhaseRva = 0x11C0C0;
        kDirectEnterModeSetRva = 0x11C800;
        kOnLogoCloseRva = 0x13F1A0;
        kAppSingletonRva = 0x484C8C;
        kPhaseDisp = 0xF8;
        kLoginIdDisp = 0xB0;
        kLoginPasswordDisp = 0xC8;
        kSendAttackRva = 0x134BE0;
        kSendStateRva = 0x134DA0;
        kSendPickupRva = 0x13B690;
        kSendItemUseRva = 0x13A6F0;
        kSendClickRva = 0xCE930;
        kItemSingletonRva = 0x482CAC;
        kGetCloseItemRva = 0xBB480;
        kGetCloseMoneyRva = 0xBB580;
        kTreeIncRva = 0xBA810;
        kMoveToDestRva = 0xA6BB0;
        kMoveToPixelRva = 0;
        kGetMainActorRva = 0;
        kPressActorRva = 0;
        kChaseRva = 0;
        kApproachInstRva = 0;
        kGetPixelRva = 0;
        kInRangeRva = 0;
        kMainPtrDisp = 0x0C;
        kMapDisp = 0;
        kMapHeadOff = 0;
        kNodeLeft = 0;
        kNodeParent = 0;
        kNodeRight = 0;
        kNodeIsNil = 0;
        kNodeKey = 0;
        kNodeValue = 0;
        kGraphicDisp = 0x1EC;
        kPixelXDisp = 0x46C;
        kPixelYDisp = 0x470;
        kAttackKeyRva = 0xD2030;
        kReserveClickRva = 0xD1B90;
        kSetAutoRva = 0xD1DF0;
        kThisAdj = 4;
        kSetTargetSlot = 0x60;
        kSetTargetRva = 0;
        kGetNameSlot = 0x14;
        kPlayerNameDisp = 0x14;
        kTargetDisp = 0x10814;
        kReservedModeDisp = 0x54;
        kVidReservedDisp = 0x10770;
        kPlayerUpdateRva = 0xCF2D0;
        kPickCloseRva = 0xD1280;
        kPickCloseMoneyRva = 0xD12F0;
        kGetStatusRva = 0xCDBB0;
        kGetStatusSlot = 0;
        kPointsDisp = 0;
        kPointXor = 0;
    }
    else if (kSendItemUseRva == 0x37D360)
        kSendItemUseRva = 0x369550;
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
        cmd.channelIndex = json.find("\"channelIndex\"") != std::string::npos
            ? (int)JsonU32(json, "channelIndex")
            : -1;
        if (!TokensEqual(JsonString(json, "token"), g_token))
        {
            WriteFrame("{\"ok\":false,\"type\":\"error\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"loop-iat\"}");
            continue;
        }

        g_lastOk = false;
        g_lastReason = "not executed";
        g_lastVid = 0;
        DWORD wait = WAIT_OBJECT_0;
        if (cmd.type == "get_pots")
        {
            if (!g_inWorld)
            {
                g_potsJson = "[]";
                g_lastOk = true;
                g_lastReason = "not in world";
            }
            else
            {
                ScanPots();
                g_lastOk = true;
                g_lastReason = "pots";
            }
        }
        else
        {
            ResetEvent(g_done);
            EnterCriticalSection(&g_lock);
            g_queue.push(cmd);
            LeaveCriticalSection(&g_lock);

            const bool waitReply = cmd.type == "get_phase" || cmd.type == "get_session" ||
                cmd.type == "login" || cmd.type == "select_character" || cmd.type == "enter_game" ||
                cmd.type == "skip_intro" || cmd.type == "get_atlas" || cmd.type == "get_hud" ||
                cmd.type == "get_entities" || cmd.type == "get_skills" || cmd.type == "use_item" ||
                cmd.type == "use_skill" || cmd.type == "is_skill_active" || cmd.type == "toggle_mount";
            if (waitReply)
            {
                const DWORD waitMs =
                    (cmd.type == "enter_game" || cmd.type == "login" || cmd.type == "select_character")
                        ? 15000
                        : 2000;
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
        char head[1536];
        _snprintf_s(
            head,
            _TRUNCATE,
            "{\"ok\":%s,\"type\":\"%s\",\"reason\":\"%s\",\"targetVid\":%u,\"convention\":\"loop-iat\",\"level\":%d,\"exp\":%d,\"nextExp\":%d,\"hp\":%d,\"maxHp\":%d,\"mp\":%d,\"maxMp\":%d,\"race\":%d,\"atlasWidth\":%u,\"atlasHeight\":%u,\"worldW\":%.1f,\"worldH\":%.1f,\"phase\":\"%s\",\"character\":\"%s\",\"account\":\"%s\",\"password\":\"%s\",\"characterSlot\":%d,\"startGame\":%s,\"inWorld\":%s",
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
            g_hudRace,
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
        std::string reply = head;
        reply += ",\"pots\":";
        reply += (cmd.type == "get_pots" && !g_potsJson.empty()) ? g_potsJson : "[]";
        reply += ",\"skills\":";
        reply += (cmd.type == "get_skills" && !g_skillsJson.empty()) ? g_skillsJson : "[]";
        reply += ",\"entities\":";
        reply += (cmd.type == "get_entities" && !g_actorsJson.empty()) ? g_actorsJson : "[]";
        reply += ",\"affects\":";
        reply += (cmd.type == "get_hud" && !g_affectsJson.empty()) ? g_affectsJson : "[]";
        reply += ",\"castRemoved\":";
        reply += (cmd.type == "get_hud" && !g_castRemovedJson.empty()) ? g_castRemovedJson : "[]";
        reply += ",\"channels\":";
        reply += (!g_channelsJson.empty() &&
                  (cmd.type == "get_channels" || cmd.type == "get_session" || cmd.type == "get_phase"))
            ? g_channelsJson : "[]";
        reply += ",\"characters\":";
        reply += (!g_charactersJson.empty() &&
                  (cmd.type == "get_session" || cmd.type == "get_phase" || cmd.type == "select_character"))
            ? g_charactersJson : "[]";
        char extra[96];
        _snprintf_s(extra, _TRUNCATE, ",\"greenRemain\":%d,\"purpleRemain\":%d,\"mounted\":%s}",
            g_greenRemain, g_purpleRemain, g_mounted ? "true" : "false");
        reply += extra;
        if (!WriteFrame(reply))
            break;
    }
    }
}

static DWORD WINAPI InitThread(void*)
{
    LoadConfig();
    PublishNoteAffectPtr();
    HMODULE client = FindClient();
    g_base = (BYTE*)client;
    InitAtlasMap();
    if (g_baerim)
    {
        // Layout recover+cache is the only address source. Reject Python helpers; no patch RVA inventing.
        kAttackDestRva = 0;
        if (kSendChatRva >= 0x410000 && kSendChatRva < 0x430000)
            kSendChatRva = 0;
        if (kIsMountingRva >= 0x410000 && kIsMountingRva < 0x430000)
            kIsMountingRva = 0;
        if (kIsAffectRva >= 0x410000 && kIsAffectRva < 0x430000)
            kIsAffectRva = 0;
        if (kGetAffectRva >= 0x410000 && kGetAffectRva < 0x430000)
            kGetAffectRva = 0;
        if (kGetValueRva >= 0x410000 && kGetValueRva < 0x430000)
            kGetValueRva = 0;
        // GetCloseItem must not alias GetPixelPosition (recover sometimes collapses them).
        if (kGetCloseItemRva && kGetCloseItemRva == kGetPixelRva)
            kGetCloseItemRva = 0;
        if (!kMoveToDestRva && kMoveToPixelRva)
            kMoveToDestRva = kMoveToPixelRva;
        NativeLog("baerim chat=0x%X mountHelper=0x%X isAffect=0x%X clickSkill=0x%X net=0x%X press=0x%X attackKey=0x%X inRange=0x%X movePix=0x%X getAff=0x%X",
            kSendChatRva, kIsMountingRva, kIsAffectRva, kClickSkillRva, kNetStreamRva, kPressActorRva, kAttackKeyRva, kInRangeRva, kMoveToPixelRva, kGetAffectRva);
    }
    InstallLoopHooks(client);
    NativeLog("loaded base=%p profile=%s pressActor=0x%X moveTo=0x%X movePixel=0x%X instAtk=0x%X inRange=0x%X getPixel=0x%X hooks=%d",
        g_base, g_arthion ? "arthion" : g_baerim ? "baerim" : "normal", kPressActorRva, kMoveToDestRva, kMoveToPixelRva, kInstAttackRva, kInRangeRva, kGetPixelRva, g_iatHooks);
    Serve();
    return 0;
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_selfModule = module;
        DisableThreadLibraryCalls(module);
        InitializeCriticalSection(&g_lock);
        g_done = CreateEventA(nullptr, FALSE, FALSE, nullptr);
        CreateThread(nullptr, 0, InitThread, nullptr, 0, nullptr);
    }
    return TRUE;
}
