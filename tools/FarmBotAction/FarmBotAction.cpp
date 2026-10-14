#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <d3d9.h>
#include <psapi.h>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <queue>
#include <string>
#include <vector>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "psapi.lib")

// In-process x86 adapter. Drains on CPythonApplication::Loop (PeekMessage/GetMessage)
// and calls existing CPythonPlayer methods on that same thread.

static const unsigned kSingletonRva = 0x482398;
static const unsigned kCharMgrRva = 0x48239C;
static const unsigned kNewAttackRva = 0xD0C70;
static const unsigned kNetStreamRva = 0x482BA0;
static const unsigned kSendAttackRva = 0x134BE0;
static const unsigned kSendStateRva = 0x134DA0;
static const unsigned kFuncWait = 0;
static const unsigned kFuncAttack = 2;
static const unsigned kMainPtrDisp = 0x0C;
static const unsigned kGraphicDisp = 0x1EC;
static const unsigned kPixelXDisp = 0x46C;
static const unsigned kPixelYDisp = 0x470;
static const float kAvoidStand = 250.f;
static const unsigned kAttackKeyRva = 0xD2030;
static const unsigned kReserveClickRva = 0xD1B90;
static const unsigned kSetAutoRva = 0xD1DF0;
static const unsigned kThisAdj = 4;
static const unsigned kSetTargetSlot = 0x60;
static const unsigned kGetInstSlot = 8;
static const unsigned kTargetDisp = 0x10814;
static const unsigned kReservedModeDisp = 0x54;
static const unsigned kVidReservedDisp = 0x10770;
static const unsigned kPlayerUpdateRva = 0xCF2D0;
static const unsigned kPickCloseRva = 0xD1280;
static const unsigned kGetStatusRva = 0xCDBB0;
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
typedef void (__thiscall* FnAttackKey)(void* self, bool press);
typedef void (__thiscall* FnVid)(void* self, unsigned vid);
typedef int (__thiscall* FnLoadAtlas)(void* self);
typedef void (__thiscall* FnAtlasSize)(void* self, float* x, float* y);
typedef int (__thiscall* FnIsAtlas)(void* self);
typedef BOOL (WINAPI* FnPeek)(LPMSG, HWND, UINT, UINT, UINT);
typedef BOOL (WINAPI* FnGetMsg)(LPMSG, HWND, UINT, UINT);
typedef void (__thiscall* FnUpdate)(void* self);

struct ShotHit
{
    unsigned vid;
    float x;
    float y;
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
static DWORD g_lastLootMs;
static HANDLE g_atlasMap;
static BYTE* g_atlasView;
static BYTE* g_atlasScratch;
static unsigned g_atlasWidth;
static unsigned g_atlasHeight;
static float g_atlasWorldW;
static float g_atlasWorldH;
static unsigned g_atlasSeq;
static volatile long g_atlasCrash;

static FnPeek g_peekA;
static FnPeek g_peekW;
static FnGetMsg g_getA;
static FnGetMsg g_getW;
static FnUpdate g_playerUpdate;

static bool UserPtr(const void* p)
{
    const uintptr_t a = (uintptr_t)p;
    return a >= 0x10000 && a < 0x7FFF0000 && (a & 3) == 0;
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

static int FindGetStatusSlot(void** vtbl)
{
    if (!g_base || !vtbl)
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
    void** vtbl = IAbsVtbl(player);
    if (!vtbl)
        return 0;
    if (g_getStatusSlot < 0)
        g_getStatusSlot = FindGetStatusSlot(vtbl);
    if (g_getStatusSlot < 0 || !UserPtr(vtbl[g_getStatusSlot]))
        return 0;
    void* iabs = (char*)player + kThisAdj;
    return ((FnGetStatus)vtbl[g_getStatusSlot])(iabs, type);
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
    void** vtbl = IAbsVtbl(player);
    if (!vtbl || !UserPtr(vtbl[kSetTargetSlot / 4]))
        return false;
    void* iabs = (char*)player + kThisAdj;
    ((FnSetTarget)vtbl[kSetTargetSlot / 4])(iabs, vid, force);
    return true;
}

static void* CallGetInstance(unsigned vid)
{
    void* mgr = CharMgr();
    if (!mgr)
        return nullptr;
    void* iabs = (char*)mgr + kThisAdj;
    void** vtbl = *(void***)iabs;
    if (!UserPtr(vtbl) || !UserPtr(vtbl[kGetInstSlot / 4]))
        return nullptr;
    void* inst = ((FnGetInst)vtbl[kGetInstSlot / 4])(iabs, vid);
    return UserPtr(inst) ? inst : nullptr;
}

static bool CallRva(unsigned rva, void* player, unsigned vid)
{
    if (!g_base || !player)
        return false;
    FnVid fn = (FnVid)(g_base + rva);
    if (IsBadCodePtr((FARPROC)fn))
        return false;
    fn(player, vid);
    return true;
}

static bool CallPickClose(void* player)
{
    FnVoid fn = (FnVoid)(g_base + kPickCloseRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    fn(player);
    return true;
}

static void MaybeAutoLoot(void* player)
{
    if (!g_lootOn || !player)
        return;
    const DWORD now = GetTickCount();
    if (g_lastLootMs != 0 && now - g_lastLootMs < 200)
        return;
    g_lastLootMs = now;
    CallPickClose(player);
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

static bool CallNewAttack(void* player)
{
    FnNewAttack fn = (FnNewAttack)(g_base + kNewAttackRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    fn(player);
    return true;
}

static void* NetStream()
{
    if (!g_base)
        return nullptr;
    void* p = *(void**)(g_base + kNetStreamRva);
    return UserPtr(p) ? p : nullptr;
}

static bool CallSendAttackPacket(unsigned vid)
{
    void* net = NetStream();
    FnSendAttack fn = (FnSendAttack)(g_base + kSendAttackRva);
    if (!net || IsBadCodePtr((FARPROC)fn))
        return false;
    fn(net, 0, vid);
    return true;
}

static bool CallSendState(float x, float y, unsigned func)
{
    void* net = NetStream();
    FnSendState fn = (FnSendState)(g_base + kSendStateRva);
    if (!net || IsBadCodePtr((FARPROC)fn))
        return false;
    float pos[3] = { x, y, 0 };
    fn(net, pos, 0.f, func, 0);
    return true;
}

static bool ReadPixel(void* inst, float* x, float* y)
{
    if (!UserPtr(inst) || !x || !y)
        return false;
    *x = *(float*)((char*)inst + kGraphicDisp + kPixelXDisp);
    *y = *(float*)((char*)inst + kGraphicDisp + kPixelYDisp);
    return true;
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
        cmd->hits.push_back(hit);
        if (semi == std::string::npos)
            break;
        i = semi + 1;
    }
}

static bool CallAttackKey(void* player, bool press)
{
    FnAttackKey fn = (FnAttackKey)(g_base + kAttackKeyRva);
    if (!g_base || !player || IsBadCodePtr((FARPROC)fn))
        return false;
    fn(player, press);
    return true;
}

static void ClearReserved(void* player)
{
    if (!player)
        return;
    *(unsigned*)((char*)player + kReservedModeDisp) = 0;
    *(unsigned*)((char*)player + kVidReservedDisp) = 0;
}

static void StopCombat(void* player)
{
    CallRva(kSetAutoRva, player, 0);
    ClearReserved(player);
    CallAttackKey(player, false);
    CallSetTarget(player, 0, 1);
}

static void StartCombat(void* player, unsigned vid, bool)
{
    CallAttackKey(player, false);
    CallSetTarget(player, vid, 1);
    CallRva(kSetAutoRva, player, vid);
    CallRva(kReserveClickRva, player, vid);
}

static void Execute(const Command& cmd)
{
    g_lastOk = false;
    g_lastReason = "not executed";
    g_lastVid = 0;

    if (cmd.type == "loot_loop")
    {
        g_lootOn = cmd.vid != 0 ? 1 : 0;
        if (!g_lootOn)
            g_lastLootMs = 0;
        g_lastOk = true;
        g_lastReason = g_lootOn ? "loot loop on" : "loot loop off";
        return;
    }

    if (cmd.type == "get_atlas")
    {
        CaptureAtlas();
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
        unsigned sent = 0;
        for (size_t i = 0; i < hits.size(); ++i)
        {
            const ShotHit& hit = hits[i];
            if (!CallGetInstance(hit.vid))
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
        }
        float rx = cmd.x;
        float ry = cmd.y;
        ReadMainPixel(&rx, &ry);
        CallSendState(rx, ry, kFuncWait);
        g_lastOk = sent != 0;
        char reason[80];
        _snprintf_s(reason, _TRUNCATE, "ranged blast %u/%u", sent, (unsigned)hits.size());
        g_lastReason = reason;
        return;
    }

    if (cmd.type == "set_target" || cmd.type == "face" || cmd.type == "press_actor" || cmd.type == "attack")
    {
        const void* inst = CallGetInstance(cmd.vid);
        if (!CallSetTarget(player, cmd.vid, 1))
        {
            g_lastReason = "SetTarget vtable missing";
            return;
        }
        StartCombat(player, cmd.vid, cmd.type == "attack");
        g_lastVid = ReadTargetVid(player);
        g_lastOk = g_lastVid == cmd.vid;
        if (g_lastOk)
            g_lastReason = inst ? "target+approach+auto-attack" : "selected (instance lookup missed)";
        else
            g_lastReason = inst ? "SetTarget did not stick" : "VID not in live instance map";
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
    InterlockedExchange(&g_busy, 0);
}

// CMSApplication::IsMessage is PeekMessage(NULL, 0, 0, PM_NOREMOVE).
// MessageProcess is GetMessage(NULL, 0, 0). Both run on CPythonApplication::Loop.
static void OnLoopPump(HWND hwnd, UINT min, UINT max)
{
    if (hwnd != nullptr || min != 0 || max != 0)
        return;
    g_loopTid = GetCurrentThreadId();
    Drain();
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

static void __fastcall HookUpdate(void* self)
{
    g_loopTid = GetCurrentThreadId();
    Drain();
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
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x56)
        return 5;
    if (p[0] == 0x55 && p[1] == 0x8B && p[2] == 0xEC && p[3] == 0x53)
        return 5;
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
    }
    if (Trampoline(peekA, (void*)HookPeekA, (void**)&g_peekA))
        ++g_iatHooks;
    if (Trampoline(peekW, (void*)HookPeekW, (void**)&g_peekW))
        ++g_iatHooks;
    if (Trampoline(getA, (void*)HookGetA, (void**)&g_getA))
        ++g_iatHooks;
    if (Trampoline(getW, (void*)HookGetW, (void**)&g_getW))
        ++g_iatHooks;
    if (client && Trampoline((BYTE*)client + kPlayerUpdateRva, (void*)HookUpdate, (void**)&g_playerUpdate))
        ++g_iatHooks;
}

static bool JsonSpace(char c)
{
    return c == ' ' || c == '\t' || c == '\r' || c == '\n';
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

static void LoadConfig()
{
    char path[MAX_PATH];
    GetTempPathA(MAX_PATH, path);
    strcat_s(path, "FarmBotAction.cfg");
    FILE* f = nullptr;
    if (fopen_s(&f, path, "r") != 0 || !f)
        return;
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
    }
    fclose(f);
}

static HMODULE FindClient()
{
    HMODULE found = GetModuleHandleA("1.metin2client.exe");
    if (found)
        return found;
    found = GetModuleHandleA("1.Debug-metin2client.exe");
    if (found)
        return found;
    return GetModuleHandleA(nullptr);
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
        return;

    char readyName[256];
    _snprintf_s(readyName, _TRUNCATE, "Local\\FarmBotReady_%s", g_pipeName.c_str());
    HANDLE ready = CreateEventA(nullptr, TRUE, FALSE, readyName);
    if (ready)
        SetEvent(ready);

    if (!ConnectNamedPipe(g_pipe, nullptr) && GetLastError() != ERROR_PIPE_CONNECTED)
        return;

    std::string json;
    if (!ReadFrame(&json))
        return;
    if (JsonString(json, "type") != "auth" || !TokensEqual(JsonString(json, "token"), g_token))
    {
        WriteFrame("{\"ok\":false,\"type\":\"auth\",\"reason\":\"unauthorized\",\"targetVid\":0,\"convention\":\"vtable-thiscall\"}");
        return;
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

        const DWORD wait = WaitForSingleObject(g_done, 2000);
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

        char reply[768];
        _snprintf_s(
            reply,
            _TRUNCATE,
            "{\"ok\":%s,\"type\":\"%s\",\"reason\":\"%s\",\"targetVid\":%u,\"convention\":\"loop-iat\",\"level\":%d,\"exp\":%d,\"nextExp\":%d,\"hp\":%d,\"maxHp\":%d,\"mp\":%d,\"maxMp\":%d,\"atlasWidth\":%u,\"atlasHeight\":%u,\"worldW\":%.1f,\"worldH\":%.1f}",
            g_lastOk ? "true" : "false",
            cmd.type.c_str(),
            g_lastReason.c_str(),
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
            g_atlasWorldH);
        if (!WriteFrame(reply))
            break;
    }
}

static DWORD WINAPI InitThread(void*)
{
    LoadConfig();
    HMODULE client = FindClient();
    g_base = (BYTE*)client;
    InitAtlasMap();
    InstallLoopHooks(client);
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
