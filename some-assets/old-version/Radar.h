#define STB_IMAGE_IMPLEMENTATION


static ImTextureID C_SelectMapTexcure = nullptr;
static int C_ImageWidth = 0;
static int C_ImageHeight = 0;
static bool C_GetRadarTexcureLoading = false;
static std::string LastMapName = "";

static float C_RadarZoom = 1.0f;
static std::vector<Vector3> mouse_rota_points;
static bool is_following_route = false;
static bool shiftWasPressed = false;

static bool isDragging = false;
static ImVec2 dragStartPos;
static ImVec2 mapPanOffset = ImVec2(0, 0);

static char FindInventory[65] = "";
static char FındCollect[65] = "";

struct TeleportMarker {
    Vector3 position;
    float createdTime;
    bool active;
};
static TeleportMarker g_TeleportMarker = { Vector3(0, 0, 0), 0.0f, false };

std::map<int, std::string> g_ItemNames;
std::vector<std::string> g_ItemListFormatted; 
std::vector<const char*> g_ItemListPtrs;

std::string ConvertWin1254ToAscii(const std::string& input) {
    std::string result;
    result.reserve(input.size());

    for (unsigned char c : input) {
        switch (c) {
        case 0xFD: result += 'i'; break; 
        case 0xDD: result += 'I'; break; 
        case 0xFE: result += 's'; break; 
        case 0xDE: result += 'S'; break; 
        case 0xF0: result += 'g'; break;
        case 0xD0: result += 'G'; break; 
        case 0xFC: result += 'u'; break;
        case 0xDC: result += 'U'; break;
        case 0xF6: result += 'o'; break;
        case 0xD6: result += 'O'; break;
        case 0xE7: result += 'c'; break;
        case 0xC7: result += 'C'; break;
        default:
            if (c < 128) result += c; 
            else result += '?'; 
            break;
        }
    }

    return result;
}

void LoadItemNames() {
    g_ItemNames.clear();
    g_ItemListFormatted.clear();
    g_ItemListPtrs.clear();

    std::string fullPath = "C:\\Resource\\ProtoList\\item_names.txt";
    std::ifstream file(fullPath);

    if (file.is_open()) {
        std::string line;

        while (std::getline(file, line)) {
            if (line.empty()) continue;
            while (!line.empty() && (line.back() == '\r' || line.back() == '\n'))
                line.pop_back();

            if (line.empty()) continue;

            std::istringstream iss(line);
            int vnum;
            std::string name;

            if (iss >> vnum) {
                std::getline(iss, name);
                size_t first = name.find_first_not_of(" \t");
                if (first != std::string::npos)
                    name = name.substr(first);

                size_t last = name.find_last_not_of(" \t\r\n");
                if (last != std::string::npos)
                    name = name.substr(0, last + 1);
                name = ConvertWin1254ToAscii(name);

                g_ItemNames[vnum] = name;

                char formatted[256];
                sprintf_s(formatted, "[%d] %s", vnum, name.c_str());
                g_ItemListFormatted.push_back(formatted);
            }
        }
        file.close();

        for (auto& str : g_ItemListFormatted) {
            g_ItemListPtrs.push_back(str.c_str());
        }
    }
}


std::vector<std::string> g_InventoryItems;
std::vector<const char*> g_InventoryItemPtrs;
bool inventoryLoaded = false;

struct InventoryItem {
    int slot;
    DWORD vnum;
    std::string display;
};

std::vector<InventoryItem> g_Inventory;
std::vector<int> g_FilteredIndices;

const char* GetItemNameFromProto(DWORD vnum)
{
    auto it = g_ItemNames.find(vnum);
    if (it != g_ItemNames.end()) {
        return it->second.c_str();
    }
    return nullptr; 
}

void FilterInventory(const char* search)
{
    g_InventoryItemPtrs.clear();
    g_FilteredIndices.clear(); 

    if (strlen(search) == 0) {
        for (size_t i = 0; i < g_Inventory.size(); i++) {
            g_InventoryItemPtrs.push_back(g_Inventory[i].display.c_str());
            g_FilteredIndices.push_back(i);
        }
    }
    else {
        for (size_t i = 0; i < g_Inventory.size(); i++) { 
            if (strstr(g_Inventory[i].display.c_str(), search) != nullptr) {
                g_InventoryItemPtrs.push_back(g_Inventory[i].display.c_str());
                g_FilteredIndices.push_back(i);
            }
        }
    }
}

void LoadInventory()
{
    g_Inventory.clear(); 
    g_InventoryItems.clear(); 
    g_InventoryItemPtrs.clear();

    for (int i = 0; i < 45; i++)
    {
        DWORD vnum = engine::GetItemIndex(i);
        if (vnum != 0)
        {
            auto it = g_ItemNames.find(vnum);
            const char* name = (it != g_ItemNames.end()) ? it->second.c_str() : "(Null)";

            char buffer[256];
            sprintf_s(buffer, "[Slot %02d] %s", i, name);

            InventoryItem item;
            item.slot = i;
            item.vnum = vnum;
            item.display = buffer;
            g_Inventory.push_back(item);

            g_InventoryItems.push_back(std::string(buffer));
        }
    }

    for (auto& item : g_InventoryItems) {
        g_InventoryItemPtrs.push_back(item.c_str());
    }
}

static int selected = 0;
static bool itemsLoaded = false;
static char lastSearch[256] = "";
std::vector<std::string> g_FilteredItemList;
std::vector<const char*> g_FilteredItemPtrs;

void FilterItems(const char* searchText) {
    g_FilteredItemList.clear();
    g_FilteredItemPtrs.clear();

    if (strlen(searchText) == 0) {
        g_FilteredItemList = g_ItemListFormatted;
    }
    else {
        std::string search = searchText;
        std::transform(search.begin(), search.end(), search.begin(), ::tolower);

        for (const auto& item : g_ItemListFormatted) {
            std::string itemLower = item;
            std::transform(itemLower.begin(), itemLower.end(), itemLower.begin(), ::tolower);
            if (itemLower.find(search) != std::string::npos) {
                g_FilteredItemList.push_back(item);
            }
        }
    }
    for (auto& str : g_FilteredItemList) {
        g_FilteredItemPtrs.push_back(str.c_str());
    }
}

static std::vector<std::string> g_SelectedItemsList;
static std::vector<const char*> g_SelectedItemsPtrs;
static std::vector<DWORD> g_SelectedItemIDs; 
static int selectedRight = 0;




struct Waypoint {
	float x;
	float y;
};
std::vector<Waypoint> cordsMaps;
std::string GetExeDirectory()
{
	char buffer[MAX_PATH];
	GetModuleFileNameA(NULL, buffer, MAX_PATH);
	std::string path(buffer);
	return path.substr(0, path.find_last_of("\\/"));
}
ImTextureID LoadTextureFromFileDX9(IDirect3DDevice9* pDevice, const char* filename, int* out_width, int* out_height)
{
	IDirect3DTexture9* texture = nullptr;
	D3DXIMAGE_INFO info{};

	DWORD fileAttr = GetFileAttributesA(filename);
	if (fileAttr == INVALID_FILE_ATTRIBUTES)
	{
		return nullptr;
	}
	HRESULT hr = D3DXCreateTextureFromFileExA(pDevice, filename, D3DX_DEFAULT, D3DX_DEFAULT, D3DX_DEFAULT, 0, D3DFMT_A8R8G8B8, D3DPOOL_MANAGED, D3DX_FILTER_LINEAR, D3DX_FILTER_LINEAR, 0, &info, nullptr, &texture);
	if (FAILED(hr))
		return nullptr;

	*out_width = info.Width;
	*out_height = info.Height;
	return (ImTextureID)texture;
}

void DrawImagePos(ImTextureID texture, const ImVec2& size, const ImVec2& pos)
{
	ImGuiWindow* window = ImGui::GetCurrentWindow();
	if (window->SkipItems)
		return;
	ImVec2 p2(pos.x + size.x, pos.y + size.y);
	window->DrawList->AddImage(
		texture,
		pos,
		p2,
		ImVec2(0, 0),
		ImVec2(1, 1),
		IM_COL32_WHITE
	);
}

void rotateVector(float& x, float& y, float angle, float originX = 0, float originY = 0) {
    x -= originX;
    y -= originY;
    float tx = x, ty = y;
    float fCos = cos(angle);
    float fSin = sin(angle);
    tx = x * fCos - y * fSin;
    ty = x * fSin + y * fCos;
    x = tx + originX;
    y = ty + originY;
}

static void DrawCircle(int x, int y, int radius, ImColor color, int segments)
{
	ImGui::GetWindowDrawList()->AddCircleFilled(ImVec2(x, y), radius, color, segments);
	ImGui::GetWindowDrawList()->AddCircle(ImVec2(x, y), radius, ImColor(255, 255, 255), segments);
}

