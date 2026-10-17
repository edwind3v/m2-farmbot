class CRender
{
public:
	CRender();
	~CRender();

	static D3DMATRIX WorldStateCopy;

	static void String(int x, int y, D3DCOLOR colour, ID3DXFont* font, bool outlined, char* string, ...);
	static int  StringWidth(ID3DXFont* font, char* string);
	static void Line(int x, int y, int x2, int y2, D3DCOLOR colour);
	static void FilledBox(int x, int y, int width, int height, D3DCOLOR colour);
	static void FilledBoxOutlined(int x, int y, int width, int height, int thickness, D3DCOLOR colour, D3DCOLOR outlinecolor);
	static void BorderedBox(int x, int y, int width, int height, int thickness, D3DCOLOR colour);
	static void BorderedBoxOutlined(int x, int y, int width, int height, int thickness, D3DCOLOR colour, D3DCOLOR outlinecolor);
	static void GradientBox(int x, int y, int width, int height, D3DCOLOR colour, D3DCOLOR color2, bool vertical);
	static void GradientBoxOutlined(int x, int y, int width, int height, int thickness, D3DCOLOR colour, D3DCOLOR color2, D3DCOLOR outlinecolor, bool vertical);
	static void Circle(int x, int y, int z, int radius, int points, D3DCOLOR colour);
	static void FilledCircle(int x, int y, int radius, int points, D3DCOLOR colour);

	static void Line3D(float x, float y, float z, float ex, float ey, float ez, D3DCOLOR colour);
	static void Circle3D(float fx, float fy, float fz, float radius, int points, D3DCOLOR colour);
	static void Box3D(float x, float y, float z, float ex, float ey, float ez, D3DCOLOR colour);
	static void Cube3D(float sx, float sy, float sz, float ex, float ey, float ez, D3DCOLOR colour);
	static void FilledCircle3D(float x, float y, float z, float radius, int points, D3DCOLOR colour);

	static void RenderBox(LPD3DXMESH ms_lpSphereMesh, float fx, float fy, float fz, float fRadius, D3DFILLMODE d3dFillMode, D3DCOLOR colour);
	static void RenderSphere(LPD3DXMESH ms_lpSphereMesh, float fx, float fy, float fz, float fRadius, D3DFILLMODE d3dFillMode, D3DCOLOR colour);

	static int FrameRate();
};
struct SD3DVertex
{
	float x, y, z, rhw;
	DWORD colour;
};

struct SVertex3D
{
	float x, y, z;
	DWORD colour;
};

CRender::CRender() {}
CRender::~CRender() {}

D3DMATRIX CRender::WorldStateCopy;

// ============================================================
// Saved render states
// ============================================================
static DWORD s_textureFactor, s_colorArg1, s_colorArg2, s_colorOp;
static DWORD s_alphaArg1, s_alphaArg2, s_alphaOp;
static DWORD s_alphaBlend, s_srcBlend, s_destBlend;
static DWORD s_fillMode, s_cullMode, s_lighting;
static DWORD s_dwVS;
static IDirect3DBaseTexture9* s_pTexture0 = nullptr;
static IDirect3DBaseTexture9* s_pTexture1 = nullptr;

static void SaveAndSet3DStates(D3DCOLOR colour)
{
	LPDIRECT3DDEVICE9 pDev = Device::pDevice;
	if (!pDev) return;

	// Save
	pDev->GetFVF(&s_dwVS);
	pDev->GetTexture(0, &s_pTexture0);
	pDev->GetTexture(1, &s_pTexture1);
	pDev->GetRenderState(D3DRS_CULLMODE, &s_cullMode);
	pDev->GetRenderState(D3DRS_FILLMODE, &s_fillMode);
	pDev->GetRenderState(D3DRS_LIGHTING, &s_lighting);
	pDev->GetRenderState(D3DRS_TEXTUREFACTOR, &s_textureFactor);
	pDev->GetTextureStageState(0, D3DTSS_COLORARG1, &s_colorArg1);
	pDev->GetTextureStageState(0, D3DTSS_COLORARG2, &s_colorArg2);
	pDev->GetTextureStageState(0, D3DTSS_COLOROP, &s_colorOp);
	pDev->GetTextureStageState(0, D3DTSS_ALPHAARG1, &s_alphaArg1);
	pDev->GetTextureStageState(0, D3DTSS_ALPHAARG2, &s_alphaArg2);
	pDev->GetTextureStageState(0, D3DTSS_ALPHAOP, &s_alphaOp);
	pDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &s_alphaBlend);
	pDev->GetRenderState(D3DRS_SRCBLEND, &s_srcBlend);
	pDev->GetRenderState(D3DRS_DESTBLEND, &s_destBlend);

	// Set
	pDev->SetTexture(0, NULL);
	pDev->SetTexture(1, NULL);
	pDev->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
	pDev->SetRenderState(D3DRS_FILLMODE, D3DFILL_SOLID);
	pDev->SetRenderState(D3DRS_LIGHTING, FALSE);
	pDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
	pDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_SRCALPHA);
	pDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA);
	pDev->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_DIFFUSE);
	pDev->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
	pDev->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_DIFFUSE);
	pDev->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);
}

static void Restore3DStates()
{
	LPDIRECT3DDEVICE9 pDev = Device::pDevice;
	if (!pDev) return;

	pDev->SetFVF(s_dwVS);
	pDev->SetRenderState(D3DRS_CULLMODE, s_cullMode);
	pDev->SetRenderState(D3DRS_FILLMODE, s_fillMode);
	pDev->SetRenderState(D3DRS_LIGHTING, s_lighting);
	pDev->SetRenderState(D3DRS_TEXTUREFACTOR, s_textureFactor);
	pDev->SetTextureStageState(0, D3DTSS_COLORARG1, s_colorArg1);
	pDev->SetTextureStageState(0, D3DTSS_COLORARG2, s_colorArg2);
	pDev->SetTextureStageState(0, D3DTSS_COLOROP, s_colorOp);
	pDev->SetTextureStageState(0, D3DTSS_ALPHAARG1, s_alphaArg1);
	pDev->SetTextureStageState(0, D3DTSS_ALPHAARG2, s_alphaArg2);
	pDev->SetTextureStageState(0, D3DTSS_ALPHAOP, s_alphaOp);
	pDev->SetRenderState(D3DRS_ALPHABLENDENABLE, s_alphaBlend);
	pDev->SetRenderState(D3DRS_SRCBLEND, s_srcBlend);
	pDev->SetRenderState(D3DRS_DESTBLEND, s_destBlend);


	pDev->SetTexture(0, s_pTexture0);
	pDev->SetTexture(1, s_pTexture1);
	if (s_pTexture0) { s_pTexture0->Release(); s_pTexture0 = nullptr; }
	if (s_pTexture1) { s_pTexture1->Release(); s_pTexture1 = nullptr; }
}


void CRender::Line3D(float x, float y, float z, float ex, float ey, float ez, D3DCOLOR colour)
{
	if (!Device::pDevice) return;

	SVertex3D verts[2] =
	{
		{ x,  y,  z,  colour },
		{ ex, ey, ez, colour }
	};

	SaveAndSet3DStates(colour);
	Device::pDevice->SetFVF(D3DFVF_XYZ | D3DFVF_DIFFUSE);
	Device::pDevice->DrawPrimitiveUP(D3DPT_LINELIST, 1, verts, sizeof(SVertex3D));
	Restore3DStates();
}

void CRender::Box3D(float sx, float sy, float sz, float ex, float ey, float ez, D3DCOLOR colour)
{
	if (!Device::pDevice) return;

	Line3D(sx, sy, sz, ex, sy, sz, colour);
	Line3D(ex, sy, sz, ex, ey, sz, colour);
	Line3D(ex, ey, sz, sx, ey, sz, colour);
	Line3D(sx, ey, sz, sx, sy, sz, colour);

	Line3D(sx, sy, ez, ex, sy, ez, colour);
	Line3D(ex, sy, ez, ex, ey, ez, colour);
	Line3D(ex, ey, ez, sx, ey, ez, colour);
	Line3D(sx, ey, ez, sx, sy, ez, colour);

	Line3D(sx, sy, sz, sx, sy, ez, colour);
	Line3D(ex, sy, sz, ex, sy, ez, colour);
	Line3D(ex, ey, sz, ex, ey, ez, colour);
	Line3D(sx, ey, sz, sx, ey, ez, colour);
}


void CRender::Cube3D(float sx, float sy, float sz, float ex, float ey, float ez, D3DCOLOR colour)
{
	Box3D(sx, sy, sz, ex, ey, ez, colour);
}


void CRender::FilledCircle3D(float x, float y, float z, float radius, int points, D3DCOLOR colour)
{
	if (!Device::pDevice) return;

	std::vector<SVertex3D> verts(points + 1);
	float delta = 2.0f * D3DX_PI / float(points);
	float theta = 0.0f;

	for (int i = 0; i < points; i++)
	{
		verts[i].x = x + radius * cosf(theta);
		verts[i].y = y + radius * sinf(theta);
		verts[i].z = z;
		verts[i].colour = colour;
		theta += delta;
	}
	verts[points] = verts[0];

	Device::pDevice->SetTexture(0, NULL);
	Device::pDevice->SetTexture(1, NULL);
	Device::pDevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
	Device::pDevice->SetRenderState(D3DRS_FILLMODE, D3DFILL_SOLID);
	Device::pDevice->SetRenderState(D3DRS_LIGHTING, FALSE);
	Device::pDevice->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
	Device::pDevice->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_SRCALPHA);
	Device::pDevice->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA);
	Device::pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_DIFFUSE);
	Device::pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
	Device::pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_DIFFUSE);
	Device::pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);
	Device::pDevice->SetFVF(D3DFVF_XYZ | D3DFVF_DIFFUSE);
	Device::pDevice->DrawPrimitiveUP(D3DPT_LINESTRIP, points, verts.data(), sizeof(SVertex3D));
}

void CRender::Circle(int x, int y, int z, int radius, int points, D3DCOLOR colour)
{
	if (!Device::pDevice) return;

	std::vector<SD3DVertex> verts(points + 1);
	for (int i = 0; i <= points; i++)
	{
		verts[i] = {
			x + radius * cosf(D3DX_PI * (i / (points / 2.0f))),
			y - radius * sinf(D3DX_PI * (i / (points / 2.0f))),
			(float)z,
			1.0f,
			colour
		};
	}
	Device::pDevice->SetFVF(D3DFVF_XYZRHW | D3DFVF_DIFFUSE);
	Device::pDevice->DrawPrimitiveUP(D3DPT_LINESTRIP, points, verts.data(), sizeof(SD3DVertex));
}

void CRender::RenderSphere(LPD3DXMESH lpMesh, float fx, float fy, float fz, float fRadius, D3DFILLMODE d3dFillMode, D3DCOLOR colour)
{
	if (!Device::pDevice || !lpMesh) return;

	D3DXMATRIX matScale, matTrans, matWorld;
	D3DXMatrixScaling(&matScale, fRadius, fRadius, fRadius);
	D3DXMatrixTranslation(&matTrans, fx, fy, fz);
	matWorld = matScale * matTrans;

	SaveAndSet3DStates(colour);
	Device::pDevice->SetRenderState(D3DRS_FILLMODE, d3dFillMode);
	Device::pDevice->SetTransform(D3DTS_WORLD, &matWorld);

	LPDIRECT3DINDEXBUFFER9  lpIB;
	LPDIRECT3DVERTEXBUFFER9 lpVB;
	lpMesh->GetIndexBuffer(&lpIB);
	lpMesh->GetVertexBuffer(&lpVB);

	Device::pDevice->SetFVF(lpMesh->GetFVF());
	Device::pDevice->SetIndices(lpIB);
	Device::pDevice->SetStreamSource(0, lpVB, 0, 24);
	Device::pDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST, 0, 0,
		lpMesh->GetNumVertices(), 0, lpMesh->GetNumFaces());

	lpIB->Release();
	lpVB->Release();
	Restore3DStates();
}

void CRender::RenderBox(LPD3DXMESH lpMesh, float fx, float fy, float fz, float fRadius, D3DFILLMODE d3dFillMode, D3DCOLOR colour)
{
	if (!Device::pDevice || !lpMesh) return;

	D3DXMATRIX matScale, matTrans, matWorld;
	D3DXMatrixScaling(&matScale, fRadius, fRadius, fRadius);
	D3DXMatrixTranslation(&matTrans, fx, fy, fz);
	matWorld = matScale * matTrans;

	SaveAndSet3DStates(colour);
	Device::pDevice->SetRenderState(D3DRS_FILLMODE, d3dFillMode);
	Device::pDevice->SetTransform(D3DTS_WORLD, &matWorld);

	LPDIRECT3DINDEXBUFFER9  lpIB;
	LPDIRECT3DVERTEXBUFFER9 lpVB;
	lpMesh->GetIndexBuffer(&lpIB);
	lpMesh->GetVertexBuffer(&lpVB);

	Device::pDevice->SetFVF(lpMesh->GetFVF());
	Device::pDevice->SetIndices(lpIB);
	Device::pDevice->SetStreamSource(0, lpVB, 0, 24);
	Device::pDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST, 0, 0,
		lpMesh->GetNumVertices(), 0, lpMesh->GetNumFaces());

	lpIB->Release();
	lpVB->Release();
	Restore3DStates();
}

