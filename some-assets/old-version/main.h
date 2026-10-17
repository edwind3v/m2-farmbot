#include <windows.h>
#include <fstream>
#include <string>
#include <vector>
#include <d3d9.h>
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "winmm.lib")//time

//detours
#include "detours.h"
#if defined _M_X64
#pragma comment(lib, "detours.X64/detours.lib")
#elif defined _M_IX86
#pragma comment(lib, "detours.X86/detours.lib")
#endif


#pragma warning (disable: 4244) 

template <int XORSTART, int BUFLEN, int XREFKILLER>

class XorStrTR {
private:
	XorStrTR();
public:
	char s[BUFLEN];
	XorStrTR(const char* xs);
	~XorStrTR() {
		for (int i = 0; i < BUFLEN; i++) s[i] = 0;
	}
};
template <int XORSTART, int BUFLEN, int XREFKILLER>
XorStrTR<XORSTART, BUFLEN, XREFKILLER>::XorStrTR(const char* xs) {
	int xvalue = XORSTART; int i = 0;
	for (; i < (BUFLEN - 1); i++) {
		s[i] = xs[i - XREFKILLER] ^ xvalue;
		xvalue += 1;
		xvalue %= 256;
	}
	s[BUFLEN - 1] = 0;
}




//==========================================================================================================================
		
int wallhacktype = 0;
bool aimbot = 1;
int aimkey = 1;
DWORD Daimkey = VK_SHIFT;
int aimsens = 2;
int aimfov = 3;
int aimheight = 3;
bool autoshoot = 0;
unsigned int asdelay = 49;
bool IsPressed = false;
DWORD astime = timeGetTime();

//==========================================================================================================================

HMODULE Hand;

UINT Stride;
D3DVERTEXBUFFER_DESC vdesc;

IDirect3DVertexDeclaration9* pDecl;
D3DVERTEXELEMENT9 decl[MAXD3DDECLLENGTH];
UINT numElements;

UINT mStartregister;
UINT mVectorCount;

IDirect3DVertexShader9* vShader;
UINT vSize;

IDirect3DPixelShader9* pShader;
UINT pSize;

IDirect3DTexture9 *texture;
D3DSURFACE_DESC sDesc;
DWORD qCRC;
D3DLOCKED_RECT pLockedRect;

D3DVIEWPORT9 Viewport;
float ScreenCX;
float ScreenCY;

int countStride = -1;
int countnumElements = -1;
int countNumVertices = -1;
int countvSize = -1;
int countpSize = -1;
int countmStartregister = -1;
int countmVectorCount = -1;

LPDIRECT3DTEXTURE9 Red, Green, Blue, Yellow;

bool ShowMenu = false;
bool info = true;
bool is_imgui_initialised = false;
bool sax = false;
bool is_wndproc_initialised = false;
DWORD wndproctime = timeGetTime();

//==========================================================================================================================

// getdir & log
using namespace std;
char dlldir[320];
char* GetDirFile(char *name)
{
	static char pldir[320];
	strcpy_s(pldir, dlldir);
	strcat_s(pldir, name);
	return pldir;
}



DWORD QuickChecksum(DWORD *pData, int size)
{
	if (!pData) { return 0x0; }

	DWORD sum;
	DWORD tmp;
	sum = *pData;

	for (int i = 1; i < (size / 4); i++)
	{
		tmp = pData[i];
		tmp = (DWORD)(sum >> 29) + tmp;
		tmp = (DWORD)(sum >> 17) + tmp;
		sum = (DWORD)(sum << 3) ^ tmp;
	}

	return sum;
}

HWND game_hwnd = NULL;
BOOL CALLBACK find_game_hwnd(HWND hwnd, LPARAM game_pid) {
	DWORD hwnd_pid = NULL;
	GetWindowThreadProcessId(hwnd, &hwnd_pid);
	if (hwnd_pid != game_pid)
		return TRUE;
	game_hwnd = hwnd;

	return FALSE;
}

//==========================================================================================================================

HRESULT GenerateTexture(IDirect3DDevice9 *pDevice, IDirect3DTexture9 **ppD3Dtex, DWORD colour32)
{
	if (FAILED(pDevice->CreateTexture(8, 8, 1, 0, D3DFMT_A4R4G4B4, D3DPOOL_MANAGED, ppD3Dtex, NULL)))
		return E_FAIL;

	WORD colour16 = ((WORD)((colour32 >> 28) & 0xF) << 12)
		| (WORD)(((colour32 >> 20) & 0xF) << 8)
		| (WORD)(((colour32 >> 12) & 0xF) << 4)
		| (WORD)(((colour32 >> 4) & 0xF) << 0);

	D3DLOCKED_RECT d3dlr;
	(*ppD3Dtex)->LockRect(0, &d3dlr, 0, 0);
	WORD *pDst16 = (WORD*)d3dlr.pBits;

	for (int xy = 0; xy < 8 * 8; xy++)
		*pDst16++ = colour16;

	(*ppD3Dtex)->UnlockRect(0);

	return S_OK;
}
