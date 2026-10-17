
float GetDistance(float Xx, float Yy, float xX, float yY)
{
    return sqrtf((yY - Yy) * (yY - Yy) + (xX - Xx) * (xX - Xx));
}

LPD3DXLINE g_Line = nullptr;
LPD3DXLINE	 Line1;
LPD3DXLINE   Line2;
LPD3DXLINE   Line3;



void DrawLine2(LPDIRECT3DDEVICE9 pDevice, int fromX, int fromY, int toX, int toY, DWORD Color)
{
    if (!g_Line)
        D3DXCreateLine(pDevice, &g_Line);

    D3DXVECTOR2 line[2] = { { (float)fromX, (float)fromY }, { (float)toX, (float)toY } };
    g_Line->Begin();
    g_Line->Draw(line, 2, Color);
    g_Line->End();
}
void DrawLine3D(LPDIRECT3DDEVICE9 pDev, int fromX, int fromY, int toX, int toY, DWORD Color)
{
    if (!Line1)
        D3DXCreateLine(pDev, &Line1);

    static D3DXVECTOR2 Line[2];
    Line[0].x = fromX;
    Line[0].y = fromY;
    Line[1].x = toX;
    Line[1].y = toY;
    Line1->Begin();
    Line1->Draw(Line, 2, Color);
    Line1->End();
}

D3DXVECTOR3 GetMidPoint(const D3DXVECTOR3& V1, const D3DXVECTOR3& V2)
{
    return D3DXVECTOR3(
        (V1.x + V2.x) * 0.5f,
        (V1.y + V2.y) * 0.5f,
        (V1.z + V2.z) * 0.5f
    );
}


struct ModelInfo_t {
    D3DXVECTOR3 Coord2D;
    D3DXVECTOR3 Coord3D;
    int Offset;
    int TEAM;
    int Index;
    float CrosshairDistance;
};

std::vector<ModelInfo_t> ModelInfo;

bool WorldToScreenV2(IDirect3DDevice9* Device, D3DXVECTOR3* inOut)
{
    if (!Device || !inOut)
        return false;

    D3DVIEWPORT9 viewport;
    D3DXMATRIX projection, view, world;
    D3DXVECTOR3 vWorldLocation = *inOut; 
    D3DXVECTOR3 vScreenCoord;

    Device->GetViewport(&viewport);
    Device->GetTransform(D3DTS_VIEW, &view);
    Device->GetTransform(D3DTS_PROJECTION, &projection);
    Device->GetTransform(D3DTS_WORLD, &world);

    D3DXVec3Project(&vScreenCoord, &vWorldLocation, &viewport, &projection, &view, &world);

    if (vScreenCoord.z > 0.0f && vScreenCoord.z < 1.0f)
    {
        if (vScreenCoord.x >= 0 && vScreenCoord.x <= viewport.Width &&
            vScreenCoord.y >= 0 && vScreenCoord.y <= viewport.Height)
        {
            ModelInfo_t model;
            model.Coord2D = vScreenCoord;
            model.Coord3D = vWorldLocation;
            ModelInfo.push_back(model);

            *inOut = vScreenCoord;
            return true;
        }
    }

    return false;
}

//void WorldToScreenV2(IDirect3DDevice9* Device, float x, float y, float z)
//{
//    if (!Device)
//        return;
//
//    D3DVIEWPORT9 viewport;
//    D3DXMATRIX projection, view, world;
//    D3DXVECTOR3 vWorldLocation(x, y, z);
//    D3DXVECTOR3 vScreenCoord;
//
//    Device->GetViewport(&viewport);
//    Device->GetTransform(D3DTS_VIEW, &view);
//    Device->GetTransform(D3DTS_PROJECTION, &projection);
//    Device->GetTransform(D3DTS_WORLD, &world);
//
//    D3DXVec3Project(&vScreenCoord, &vWorldLocation, &viewport, &projection, &view, &world);
//
//    if (vScreenCoord.z > 0.0f && vScreenCoord.z < 1.0f)
//    {
//        if (vScreenCoord.x >= 0 && vScreenCoord.x <= viewport.Width &&
//            vScreenCoord.y >= 0 && vScreenCoord.y <= viewport.Height)
//        {
//            ModelInfo_t model;
//            model.Coord2D = vScreenCoord;
//            model.Coord3D = vWorldLocation;
//            ModelInfo.push_back(model);
//        }
//    }
//}

bool w2s(LPDIRECT3DDEVICE9 pDevice, D3DXVECTOR3* inOut)
{
    D3DXVECTOR3 screenPos;
    D3DXMATRIX view, projection, world;
    D3DVIEWPORT9 viewport;

    pDevice->GetTransform(D3DTS_VIEW, &view);
    pDevice->GetTransform(D3DTS_PROJECTION, &projection);
    pDevice->GetTransform(D3DTS_WORLD, &world);
    pDevice->GetViewport(&viewport);

    D3DXVec3Project(&screenPos, inOut, &viewport, &projection, &view, &world);

    if (screenPos.z >= 1.0f)
        return false;

    *inOut = screenPos;
    return true;
}



static std::string string_To_UTF8(const std::string& str)
{
    int nwLen = ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, NULL, 0);

    wchar_t* pwBuf = new wchar_t[nwLen + 1];
    ZeroMemory(pwBuf, nwLen * 2 + 2);

    ::MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), pwBuf, nwLen);

    int nLen = ::WideCharToMultiByte(CP_UTF8, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

    char* pBuf = new char[nLen + 1];
    ZeroMemory(pBuf, nLen + 1);

    ::WideCharToMultiByte(CP_UTF8, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

    std::string retStr(pBuf);

    delete[]pwBuf;
    delete[]pBuf;

    pwBuf = NULL;
    pBuf = NULL;

    return retStr;
}
static std::string WStringToUTF8(const wchar_t* lpwcszWString)
{
    char* pElementText;
    int iTextLen = ::WideCharToMultiByte(CP_UTF8, 0, (LPWSTR)lpwcszWString, -1, NULL, 0, NULL, NULL);
    pElementText = new char[iTextLen + 1];
    memset((void*)pElementText, 0, (iTextLen + 1) * sizeof(char));
    ::WideCharToMultiByte(CP_UTF8, 0, (LPWSTR)lpwcszWString, -1, pElementText, iTextLen, NULL, NULL);
    std::string strReturn(pElementText);
    delete[] pElementText;
    return strReturn;
}
static std::wstring MBytesToWString(const char* lpcszString)
{
    int len = strlen(lpcszString);
    int unicodeLen = ::MultiByteToWideChar(CP_ACP, 0, lpcszString, -1, NULL, 0);
    wchar_t* pUnicode = new wchar_t[unicodeLen + 1];
    memset(pUnicode, 0, (unicodeLen + 1) * sizeof(wchar_t));
    ::MultiByteToWideChar(CP_ACP, 0, lpcszString, -1, (LPWSTR)pUnicode, unicodeLen);
    std::wstring wString = (wchar_t*)pUnicode;
    delete[] pUnicode;
    return wString;
}

RGBA GlassBlue = { 65,105,225,80 };
RGBA glassyellow = { 255,255,0,160 };

RGBA blue = { 0,96,192,255 };

static void DrawFilledRect(int x, int y, int w, int h, RGBA* color)
{
    ImGui::GetOverlayDrawList()->AddRectFilled(ImVec2(x, y), ImVec2(x + w, y + h), ImGui::ColorConvertFloat4ToU32(ImVec4(color->R / 255.0, color->G / 255.0, color->B / 255.0, color->A / 255.0)), 0, 0);
}
static void DrawLine(int x1, int y1, int x2, int y2, RGBA* color, int thickness)
{
    ImGui::GetOverlayDrawList()->AddLine(ImVec2(x1, y1), ImVec2(x2, y2), ImGui::ColorConvertFloat4ToU32(ImVec4(color->R / 255.0, color->G / 255.0, color->B / 255.0, color->A / 255.0)), thickness);
}

static void DrawNewText(int x, int y, RGBA* color, const char* str)
{
    ImFont a;
    std::string utf_8_1 = std::string(str);
    std::string utf_8_2 = string_To_UTF8(utf_8_1);
    ImGui::GetOverlayDrawList()->AddText(ImVec2(x, y), ImGui::ColorConvertFloat4ToU32(ImVec4(color->R / 255.0, color->G / 255.0, color->B / 255.0, color->A / 255.0)), utf_8_2.c_str());
}

void DrawP(LPDIRECT3DDEVICE9 Device, int baseX, int baseY, int baseW, int baseH, D3DCOLOR Cor)
{
    D3DRECT BarRect = { baseX, baseY, baseX + baseW, baseY + baseH };
    Device->Clear(1, &BarRect, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, Cor, 0, 0);
}
void DrawCornerBox(LPDIRECT3DDEVICE9 Device, int x, int y, int w, int h, int borderPx, DWORD borderColor)
{
    DrawP(Device, x - (w / 2), (y - h + borderPx), w / 3, borderPx, borderColor); //bottom 
    DrawP(Device, x - (w / 2) + w - w / 3, (y - h + borderPx), w / 3, borderPx, borderColor); //bottom 
    DrawP(Device, x - (w / 2), (y - h + borderPx), borderPx, w / 3, borderColor); //left 
    DrawP(Device, x - (w / 2), (y - h + borderPx) + h - w / 3, borderPx, w / 3, borderColor); //left 
    DrawP(Device, x - (w / 2), y, w / 3, borderPx, borderColor); //top 
    DrawP(Device, x - (w / 2) + w - w / 3, y, w / 3, borderPx, borderColor); //top 
    DrawP(Device, (x + w - borderPx) - (w / 2), (y - h + borderPx), borderPx, w / 3, borderColor);//right 
    DrawP(Device, (x + w - borderPx) - (w / 2), (y - h + borderPx) + h - w / 3, borderPx, w / 3, borderColor);//right 
}




namespace Draw
{
    void Draw3DBox(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& playerPos, float height, float halfWidth, DWORD color)
    {
        D3DVIEWPORT9 viewport;
        D3DXMATRIX projection, view, world;
        pDevice->GetTransform(D3DTS_VIEW, &view);
        pDevice->GetTransform(D3DTS_PROJECTION, &projection);
        pDevice->GetTransform(D3DTS_WORLD, &world);
        pDevice->GetViewport(&viewport);

        D3DXVECTOR3 vHead = playerPos + D3DXVECTOR3(0, height, 0);
        D3DXVECTOR3 vFeet = playerPos;

        D3DXVECTOR3 worldPoints[8] =
        {
            { vFeet.x - halfWidth, vFeet.y, vFeet.z - halfWidth },
            { vFeet.x + halfWidth, vFeet.y, vFeet.z - halfWidth },
            { vFeet.x + halfWidth, vFeet.y, vFeet.z + halfWidth },
            { vFeet.x - halfWidth, vFeet.y, vFeet.z + halfWidth },

            { vHead.x - halfWidth, vHead.y, vHead.z - halfWidth }, 
            { vHead.x + halfWidth, vHead.y, vHead.z - halfWidth },
            { vHead.x + halfWidth, vHead.y, vHead.z + halfWidth }, 
            { vHead.x - halfWidth, vHead.y, vHead.z + halfWidth } 
        };

        D3DXVECTOR3 screen[8];
        for (int i = 0; i < 8; i++)
        {
            D3DXVec3Project(&screen[i], &worldPoints[i], &viewport, &projection, &view, &world);
            if (screen[i].z >= 1.0f) return; 
        }

        DrawLine3D(pDevice, screen[0].x, screen[0].y, screen[1].x, screen[1].y, color);
        DrawLine3D(pDevice, screen[1].x, screen[1].y, screen[2].x, screen[2].y, color);
        DrawLine3D(pDevice, screen[2].x, screen[2].y, screen[3].x, screen[3].y, color);
        DrawLine3D(pDevice, screen[3].x, screen[3].y, screen[0].x, screen[0].y, color);

        DrawLine3D(pDevice, screen[4].x, screen[4].y, screen[5].x, screen[5].y, color);
        DrawLine3D(pDevice, screen[5].x, screen[5].y, screen[6].x, screen[6].y, color);
        DrawLine3D(pDevice, screen[6].x, screen[6].y, screen[7].x, screen[7].y, color);
        DrawLine3D(pDevice, screen[7].x, screen[7].y, screen[4].x, screen[4].y, color);

        DrawLine3D(pDevice, screen[0].x, screen[0].y, screen[4].x, screen[4].y, color);
        DrawLine3D(pDevice, screen[1].x, screen[1].y, screen[5].x, screen[5].y, color);
        DrawLine3D(pDevice, screen[2].x, screen[2].y, screen[6].x, screen[6].y, color);
        DrawLine3D(pDevice, screen[3].x, screen[3].y, screen[7].x, screen[7].y, color);
    }

    void Draw3DCornerBoxv2(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& enemyPos, float boxHeight = 90.0f, float boxHalfWidth = 50.0f, int borderPx = 2, ImColor color = ImColor(255, 0, 0, 255))
    {
        D3DXVECTOR3 top = enemyPos + D3DXVECTOR3(0, 0, boxHeight);
        D3DXVECTOR3 bottom = enemyPos;

        D3DXVECTOR3 BoxPoints[8] = {
            { bottom.x - boxHalfWidth, bottom.y - boxHalfWidth, bottom.z },
            { bottom.x + boxHalfWidth, bottom.y - boxHalfWidth, bottom.z }, 
            { bottom.x + boxHalfWidth, bottom.y + boxHalfWidth, bottom.z }, 
            { bottom.x - boxHalfWidth, bottom.y + boxHalfWidth, bottom.z }, 
            { top.x - boxHalfWidth, top.y - boxHalfWidth, top.z },          
            { top.x + boxHalfWidth, top.y - boxHalfWidth, top.z },          
            { top.x + boxHalfWidth, top.y + boxHalfWidth, top.z },         
            { top.x - boxHalfWidth, top.y + boxHalfWidth, top.z }          
        };
        bool visible = true;
        for (int i = 0; i < 8; i++)
        {
            if (!WorldToScreenV2(pDevice, &BoxPoints[i]))
            {
                visible = false;
                break;
            }
        }
        if (!visible) return;

        int w = (int)((BoxPoints[1].x - BoxPoints[0].x) / 2);
        int h = (int)((BoxPoints[4].y - BoxPoints[0].y) / 2);


        DrawLine2(pDevice, (int)BoxPoints[0].x, (int)BoxPoints[0].y, (int)BoxPoints[0].x + w, (int)BoxPoints[0].y, color); 
        DrawLine2(pDevice, (int)BoxPoints[0].x, (int)BoxPoints[0].y, (int)BoxPoints[0].x, (int)BoxPoints[0].y + h, color);

        DrawLine2(pDevice, (int)BoxPoints[1].x - w, (int)BoxPoints[1].y, (int)BoxPoints[1].x, (int)BoxPoints[1].y, color); 
        DrawLine2(pDevice, (int)BoxPoints[1].x, (int)BoxPoints[1].y, (int)BoxPoints[1].x, (int)BoxPoints[1].y + h, color); 

        DrawLine2(pDevice, (int)BoxPoints[2].x - w, (int)BoxPoints[2].y, (int)BoxPoints[2].x, (int)BoxPoints[2].y, color); 
        DrawLine2(pDevice, (int)BoxPoints[2].x, (int)BoxPoints[2].y - h, (int)BoxPoints[2].x, (int)BoxPoints[2].y, color); 

        DrawLine2(pDevice, (int)BoxPoints[3].x, (int)BoxPoints[3].y - h, (int)BoxPoints[3].x, (int)BoxPoints[3].y, color); 
        DrawLine2(pDevice, (int)BoxPoints[3].x, (int)BoxPoints[3].y, (int)BoxPoints[3].x + w, (int)BoxPoints[3].y, color); 

        DrawLine2(pDevice, (int)BoxPoints[4].x, (int)BoxPoints[4].y, (int)BoxPoints[0].x, (int)BoxPoints[0].y, color);
        DrawLine2(pDevice, (int)BoxPoints[5].x, (int)BoxPoints[5].y, (int)BoxPoints[1].x, (int)BoxPoints[1].y, color);
        DrawLine2(pDevice, (int)BoxPoints[6].x, (int)BoxPoints[6].y, (int)BoxPoints[2].x, (int)BoxPoints[2].y, color);
        DrawLine2(pDevice, (int)BoxPoints[7].x, (int)BoxPoints[7].y, (int)BoxPoints[3].x, (int)BoxPoints[3].y, color);
    }

    void Draw3DBox(LPDIRECT3DDEVICE9 pDev, D3DXVECTOR3 Head, D3DXVECTOR3 Feet, DWORD Color)
    {
        float xz = 14.0f; 
        float height = Head.y - Feet.y; 
        D3DXVECTOR3 Pos1 = D3DXVECTOR3(Feet.x - xz, Feet.y, Feet.z - xz); 
        D3DXVECTOR3 Pos2 = D3DXVECTOR3(Feet.x + xz, Feet.y, Feet.z - xz); 
        D3DXVECTOR3 Pos3 = D3DXVECTOR3(Feet.x + xz, Feet.y, Feet.z + xz); 
        D3DXVECTOR3 Pos4 = D3DXVECTOR3(Feet.x - xz, Feet.y, Feet.z + xz); 

        D3DXVECTOR3 Pos5 = D3DXVECTOR3(Pos1.x, Feet.y + height, Pos1.z);
        D3DXVECTOR3 Pos6 = D3DXVECTOR3(Pos2.x, Feet.y + height, Pos2.z);
        D3DXVECTOR3 Pos7 = D3DXVECTOR3(Pos3.x, Feet.y + height, Pos3.z);
        D3DXVECTOR3 Pos8 = D3DXVECTOR3(Pos4.x, Feet.y + height, Pos4.z);

        if (WorldToScreenV2(pDev, &Pos1) && WorldToScreenV2(pDev, &Pos2) &&
            WorldToScreenV2(pDev, &Pos3) && WorldToScreenV2(pDev, &Pos4) &&
            WorldToScreenV2(pDev, &Pos5) && WorldToScreenV2(pDev, &Pos6) &&
            WorldToScreenV2(pDev, &Pos7) && WorldToScreenV2(pDev, &Pos8))
        {
            DrawLine3D(pDev, Pos1.x, Pos1.y, Pos2.x, Pos2.y, Color);
            DrawLine3D(pDev, Pos2.x, Pos2.y, Pos3.x, Pos3.y, Color);
            DrawLine3D(pDev, Pos3.x, Pos3.y, Pos4.x, Pos4.y, Color);
            DrawLine3D(pDev, Pos4.x, Pos4.y, Pos1.x, Pos1.y, Color);

            DrawLine3D(pDev, Pos5.x, Pos5.y, Pos6.x, Pos6.y, Color);
            DrawLine3D(pDev, Pos6.x, Pos6.y, Pos7.x, Pos7.y, Color);
            DrawLine3D(pDev, Pos7.x, Pos7.y, Pos8.x, Pos8.y, Color);
            DrawLine3D(pDev, Pos8.x, Pos8.y, Pos5.x, Pos5.y, Color);

            DrawLine3D(pDev, Pos1.x, Pos1.y, Pos5.x, Pos5.y, Color);
            DrawLine3D(pDev, Pos2.x, Pos2.y, Pos6.x, Pos6.y, Color);
            DrawLine3D(pDev, Pos3.x, Pos3.y, Pos7.x, Pos7.y, Color);
            DrawLine3D(pDev, Pos4.x, Pos4.y, Pos8.x, Pos8.y, Color);
        }
    }


    //void Cube3D(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& myPos, const D3DXVECTOR3& enemyPos, ImColor color = D3DCOLOR_ARGB(255, 255, 0, 0))
    //{
    //    D3DXVECTOR3 start = myPos;
    //    D3DXVECTOR3 end = enemyPos;
    //    if (WorldToScreenV2(pDevice, &start) && WorldToScreenV2(pDevice, &end))
    //    {
    //        DrawLine2(pDevice,(int)start.x, (int)start.y,(int)end.x, (int)end.y,color);
    //        Draw3DBox(pDevice, start,end, color);

    //        ModelInfo_t modelStart;
    //        modelStart.Coord2D = start;
    //        modelStart.Coord3D = myPos;
    //        ModelInfo.push_back(modelStart);

    //        ModelInfo_t modelEnd;
    //        modelEnd.Coord2D = end;
    //        modelEnd.Coord3D = enemyPos;
    //        ModelInfo.push_back(modelEnd);
    //    }
    //}

    void Line3D(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& myPos, const D3DXVECTOR3& enemyPos, DWORD color = D3DCOLOR_ARGB(255, 255, 0, 0))
    {
        D3DXVECTOR3 start = myPos;
        D3DXVECTOR3 end = enemyPos;

        if (WorldToScreenV2(pDevice, &start) && WorldToScreenV2(pDevice, &end))
        {
            DrawLine2(
                pDevice,
                (int)start.x, (int)start.y,
                (int)end.x, (int)end.y,
                color
            );
        }
    }


    static void DrawGetName(int x, int y, RGBA* back_color, RGBA* main_color, const char* pText, ...)
    {
        va_list va_alist;
        char buf[1024] = { 0 };
        va_start(va_alist, pText);
        _vsnprintf_s(buf, sizeof(buf), pText, va_alist);
        va_end(va_alist);
        std::string text = WStringToUTF8(MBytesToWString(buf).c_str());

        const ImVec2 text_dimension = ImGui::CalcTextSize(text.c_str());
        const float text_width = text_dimension.x + 5.f;
        const float mid_width = x - (text_width / 2.f);
        RGBA green_color = { 128,224,0,255 };

        DrawFilledRect(mid_width + 2.f, y - 25.f, text_width + 10.f, text_dimension.y + 5.f, back_color);
        DrawLine(mid_width, y - 25.f, mid_width, (y - 25.f) + text_dimension.y + 5.f, &green_color, 3.f);
        DrawNewText(mid_width + 6.f, (y - 25.f) + 2.f, main_color, text.c_str());

    }
    void Box3D(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& enemyPos, ImColor color = D3DCOLOR_ARGB(255, 255, 0, 0), const std::string& text = "")
    {
        float boxHeight = 180.0f;
        float boxHalfWidth = 75.0f;
        int borderPx = 0.50;

        D3DXVECTOR3 corners[8] = {
            { enemyPos.x - boxHalfWidth, enemyPos.y - boxHalfWidth, enemyPos.z },
            { enemyPos.x + boxHalfWidth, enemyPos.y - boxHalfWidth, enemyPos.z },
            { enemyPos.x - boxHalfWidth, enemyPos.y + boxHalfWidth, enemyPos.z },
            { enemyPos.x + boxHalfWidth, enemyPos.y + boxHalfWidth, enemyPos.z },
            { enemyPos.x - boxHalfWidth, enemyPos.y - boxHalfWidth, enemyPos.z + boxHeight },
            { enemyPos.x + boxHalfWidth, enemyPos.y - boxHalfWidth, enemyPos.z + boxHeight },
            { enemyPos.x - boxHalfWidth, enemyPos.y + boxHalfWidth, enemyPos.z + boxHeight },
            { enemyPos.x + boxHalfWidth, enemyPos.y + boxHalfWidth, enemyPos.z + boxHeight }
        };

        D3DXVECTOR3 screen[8];
        for (int i = 0; i < 8; i++)
        {
            screen[i] = corners[i];
            if (!WorldToScreenV2(pDevice, &screen[i]))
                return;
        }
        DrawLine2(pDevice, (int)screen[0].x, (int)screen[0].y, (int)screen[1].x, (int)screen[1].y, color);
        DrawLine2(pDevice, (int)screen[1].x, (int)screen[1].y, (int)screen[3].x, (int)screen[3].y, color);
        DrawLine2(pDevice, (int)screen[3].x, (int)screen[3].y, (int)screen[2].x, (int)screen[2].y, color);
        DrawLine2(pDevice, (int)screen[2].x, (int)screen[2].y, (int)screen[0].x, (int)screen[0].y, color);

        DrawLine2(pDevice, (int)screen[4].x, (int)screen[4].y, (int)screen[5].x, (int)screen[5].y, color);
        DrawLine2(pDevice, (int)screen[5].x, (int)screen[5].y, (int)screen[7].x, (int)screen[7].y, color);
        DrawLine2(pDevice, (int)screen[7].x, (int)screen[7].y, (int)screen[6].x, (int)screen[6].y, color);
        DrawLine2(pDevice, (int)screen[6].x, (int)screen[6].y, (int)screen[4].x, (int)screen[4].y, color);

        DrawLine2(pDevice, (int)screen[0].x, (int)screen[0].y, (int)screen[4].x, (int)screen[4].y, color);
        DrawLine2(pDevice, (int)screen[1].x, (int)screen[1].y, (int)screen[5].x, (int)screen[5].y, color);
        DrawLine2(pDevice, (int)screen[2].x, (int)screen[2].y, (int)screen[6].x, (int)screen[6].y, color);
        DrawLine2(pDevice, (int)screen[3].x, (int)screen[3].y, (int)screen[7].x, (int)screen[7].y, color);
        //if (!text.empty())
        //{
        //    int textX = (int)((screen[4].x + screen[5].x + screen[6].x + screen[7].x) / 4.0f);
        //    int textY = (int)((screen[4].y + screen[5].y + screen[6].y + screen[7].y) / 4.0f) - 15; // biraz üstüne
        //    DrawTextAWrapper(pDevice, text.c_str(), textX, textY, D3DCOLOR_ARGB(255, 255, 255, 255));
        //}
        ModelInfo.clear();
    }
    void Cube3D(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& myPos, const D3DXVECTOR3& enemyPos, ImColor color = ImColor(255, 0, 0, 0))
    {
        D3DXVECTOR3 start = myPos;
        D3DXVECTOR3 end = enemyPos;

        if (WorldToScreenV2(pDevice, &start) && WorldToScreenV2(pDevice, &end))
        {
            DrawLine2(pDevice, (int)start.x, (int)start.y, (int)end.x, (int)end.y, color);

            ModelInfo_t modelStart;
            modelStart.Coord2D = start;
            modelStart.Coord3D = myPos;
            ModelInfo.push_back(modelStart);

            ModelInfo_t modelEnd;
            modelEnd.Coord2D = end;
            modelEnd.Coord3D = enemyPos;
            ModelInfo.push_back(modelEnd);
        }
    }




    void DrawPlayerName(LPDIRECT3DDEVICE9 pDevice,const D3DXVECTOR3& worldPos,RGBA* back_color,RGBA* main_color,const char* name)
    {
        D3DXVECTOR3 screenPos = worldPos;

        if (!w2s(pDevice, &screenPos))
            return;
        int screenX = static_cast<int>(screenPos.x);
        int screenY = static_cast<int>(screenPos.y);

        DrawGetName(screenX, screenY, back_color, main_color, "%s", name);
    }



    void Draw3DCircle(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& rootPos, float radius, DWORD color = D3DCOLOR_ARGB(255, 255, 255, 255))
    {
        const int segments = 8;
        D3DXVECTOR2 screenPos[segments];
        bool isVisible = true;

        for (int i = 0; i < segments; ++i)
        {
            float angle = (i * 2.0f * D3DX_PI) / segments;
            D3DXVECTOR3 circlePoint = {
                rootPos.x + radius * cosf(angle),
                rootPos.y + radius * sinf(angle),
                rootPos.z
            };

            if (!w2s(pDevice, &circlePoint))
            {
                isVisible = false;
                break;
            }

            screenPos[i] = D3DXVECTOR2(circlePoint.x, circlePoint.y);
        }

        if (!isVisible)
            return;

        for (int i = 0; i < segments; ++i)
        {
            DrawLine2(
                pDevice,
                (int)screenPos[i].x, (int)screenPos[i].y,
                (int)screenPos[(i + 1) % segments].x, (int)screenPos[(i + 1) % segments].y,
                color
            );
        }
    }

    void Line4D(LPDIRECT3DDEVICE9 pDevice, const D3DXVECTOR3& myPos, const D3DXVECTOR3& enemyPos, DWORD color = D3DCOLOR_ARGB(255, 255, 0, 0))
    {
        D3DXVECTOR3 start = myPos;
        D3DXVECTOR3 end = enemyPos;

        if (w2s(pDevice, &start) && w2s(pDevice, &end))
        {
            DrawLine2(
                pDevice,
                (int)start.x, (int)start.y,
                (int)end.x, (int)end.y,
                color
            );

            float radius = 5.0f;
            DWORD circleColor = D3DCOLOR_ARGB(255, 255, 255, 0);
            Draw3DCircle(pDevice, enemyPos, radius, circleColor);
        }
    }


    void rangewait(LPDIRECT3DDEVICE9 pDevice, float x, float y, float z, DWORD color, float radius)
    {
        D3DXVECTOR3 pos = { x, y, z };
        Draw3DCircle(pDevice, pos, radius, color);
    }

}

