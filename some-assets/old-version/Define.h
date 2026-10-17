#define DEG2RAD(x)  ( (float)(x) * (float)(M_PI / 180.f) )
#define M_PI		3.14159265358979323846f


typedef struct C_Entity
{
    int Type;
    float x;
    float y;
    float z;
    float ModelX;
    float ModelY;
    float ModelZ;
    float Rotation;
    float SkilRotation;
    float IsCharRotation;
    int IsLvl;
    int IsDead;
    int IsChar;
    int BodyRecoil;
    char Name[64];
    DWORD VID;
    DWORD BannedVID;
} Entity;



struct c_entity
{
    int Type;
    float x;
    float y;
    float z;
    float ModelX;
    float ModelY;
    float ModelZ;
    float SkilRotation;
    float IsCharRotation;
    float Rotation;
    int IsLvl;
    int IsDead;
    int IsChar;
    char* Name;
    int BodyRecoil;
    DWORD VID;
} c_entity[355];



struct c_players
{
    int Type;
    float x;
    float y;
    float z;
    float ModelX;
    float ModelY;
    float ModelZ;
    float SkilRotation;
    float IsCharRotation;
    float Rotation;
    float C_ModelVectorX;
    float C_ModelVectorY;
    float C_ModelVectorZ;
    int BodyRecoil;
    int IsLvl;
    int IsDead;
    int IsChar;
    char Name[64] = { 0 };
    DWORD VID;
    DWORD BannedVID;
} c_players;


struct MoveToDest
{
    float x, y, z;
};

struct D3DVECTOR3
{
    float x, y, z;
};

class Vector2
{
public:
    Vector2() : x(0.f), y(0.f) {}
    Vector2(float _x, float _y) : x(_x), y(_y) {}
    ~Vector2() {}

    float x;
    float y;

    inline float Dot(Vector2 v)
    {
        return x * v.x + y * v.y;
    }

    inline float Distance(Vector2 v)
    {
        return sqrtf(powf(v.x - x, 2.0f) + powf(v.y - y, 2.0f));
    }

    Vector2 operator+(Vector2 v)
    {
        return Vector2(x + v.x, y + v.y);
    }

    Vector2 operator-(Vector2 v)
    {
        return Vector2(x - v.x, y - v.y);
    }

    Vector2 operator+(const Vector2& v) const
    {
        return Vector2(x + v.x, y + v.y);
    }

    Vector2 Cnv(float offsetX, float offsetY) const {
        return *this + Vector2(offsetX, offsetY);
    }
};

class Vector3
{
public:
    Vector3() : x(0.f), y(0.f), z(0.f) {}
    Vector3(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {}
    ~Vector3() {}
    float x;
    float y;
    float z;
    inline float Dot(Vector3 v)
    {
        return x * v.x + y * v.y + z * v.z;
    }
    inline float Distance(Vector3 v)
    {
        return sqrtf(powf(v.x - x, 2.0f) + powf(v.y - y, 2.0f) + powf(v.z - z, 2.0f));
    }
    Vector3 operator+(Vector3 v)
    {
        return Vector3(x + v.x, y + v.y, z + v.z);
    }
    Vector3 operator-(Vector3 v)
    {
        return Vector3(x - v.x, y - v.y, z - v.z);
    }
    Vector3 operator+(const Vector3& v) const
    {
        return Vector3(x + v.x, y + v.y, z + v.z);
    }
    Vector3 Cnv(float offsetX, float offsetY, float offsetZ) const {
        return *this + Vector3(offsetX, offsetY, offsetZ);
    }
};

std::string Red1 = std::string(/*|cFF0000*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x7B\x7C\x7D\x7E" + 0x2CAE8000).s);
std::string Green1 = std::string(/*|c00FF00*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x0D\x0A\x7D\x7E" + 0x2CAE8000).s);
std::string Blue1 = std::string(/*|c0000FF*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x7B\x7C\x0B\x08" + 0x2CAE8000).s);
std::string Yellow1 = std::string(/*|cFFFF00*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x0D\x0A\x7D\x7E" + 0x2CAE8000).s);
std::string Cyan1 = std::string(/*|c00FFFF*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x0D\x0A\x0B\x08" + 0x2CAE8000).s);
std::string Magenta1 = std::string(/*|cFF00FF*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x7B\x7C\x0B\x08" + 0x2CAE8000).s);
std::string White1 = std::string(/*|cFFFFFF*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x0D\x0A\x0B\x08" + 0x2CAE8000).s);
std::string Black1 = std::string(/*|c000000*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x79\x7A\x7B\x7C\x7D\x7E" + 0x2CAE8000).s);
std::string Orange1 = std::string(/*|cFFA500*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x0A\x79\x7D\x7E" + 0x2CAE8000).s);
std::string Purple1 = std::string(/*|c800080*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x71\x7A\x7B\x7C\x75\x7E" + 0x2CAE8000).s);
std::string LightBlue1 = std::string(/*|cADD8E6*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x08\x0E\x0F\x74\x08\x78" + 0x2CAE8000).s);
std::string Pink1 = std::string(/*|cFFC0CB*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x0F\x0C\x08\x7C\x0E\x0C" + 0x2CAE8000).s);
std::string Brown1 = std::string(/*|cA52A2A*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x08\x7F\x79\x0D\x7F\x0F" + 0x2CAE8000).s);
std::string Gray1 = std::string(/*|c808080*/XorStrTR<0x47, 9, 0x2CAE8000>("\x3B\x2B\x71\x7A\x73\x7C\x75\x7E" + 0x2CAE8000).s);



//#define MAX_NAME_LEN 51
//enum Windows
//{
//    RESERVED_WINDOW,
//    INVENTORY,
//    EQUIPMENT,
//    SAFEBOX,
//    MALL,
//    DRAGON_SOUL_INVENTORY,
//    GROUND,
//    BELT_INVENTORY,
//
//    WINDOW_TYPE_MAX,
//};
//#define WORD_MAX 0xffff
//typedef struct SItemPos
//{
//    BYTE window_type;
//    WORD cell;
//    SItemPos()
//    {
//        window_type = INVENTORY;
//        cell = WORD_MAX;
//    }
//    SItemPos(BYTE _window_type, WORD _cell)
//    {
//        window_type = _window_type;
//        cell = _cell;
//    }
//} TItemPos;


enum Windows
{
    RESERVED_WINDOW,
    INVENTORY,
    EQUIPMENT,
    SAFEBOX,
    MALL,
    DRAGON_SOUL_INVENTORY,
    GROUND,
    BELT_INVENTORY,
    WINDOW_TYPE_MAX,
};

// TItemPos Struct
#define WORD_MAX 0xffff
typedef struct SItemPos
{
    BYTE window_type;
    WORD cell;

    SItemPos()
    {
        window_type = INVENTORY;
        cell = WORD_MAX;
    }

    SItemPos(BYTE _window_type, WORD _cell)
    {
        window_type = _window_type;
        cell = _cell;
    }
} TItemPos;

#pragma pack(push, 1)
struct Sex
{
    BYTE window_type;  
    WORD cell;        

    Sex() : window_type(0), cell(0) {}
    Sex(BYTE wt, WORD c) : window_type(wt), cell(c) {}
};
#pragma pack(pop)


enum
{
    POINT_NONE = 0,
    POINT_LEVEL = 1,
    POINT_VOICE = 2,
    POINT_EXP = 3,
    POINT_NEXT_EXP = 4,
    POINT_HP = 5,
    POINT_MAX_HP = 6,
    POINT_SP = 7,
    POINT_MAX_SP = 8,
    POINT_STAMINA = 9,
    POINT_MAX_STAMINA = 10,
};


// GetNames : e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? a1 ? ? ? ? 83 c0 ? 89 45 ? 8b 4d ? 51 8b 55 ? 8b 02 8b 4d ? 8b 50 ? ff d2 89 45

typedef struct SGroundItemInstance
{
    DWORD                   Instance;
    DWORD                   dwVirtualNumber;
    D3DXVECTOR3             v3EndPosition;
    D3DXVECTOR3             v3RotationAxis;
    D3DXQUATERNION          qEnd;
    D3DXVECTOR3             v3Center;
    DWORD* ThingInstance;
    DWORD                   dwStartTime;
    DWORD                   dwEndTime;
    DWORD                   eDropSoundType;
    std::string             stOwnership;
} TGroundItemInstance;

typedef struct SGroundItemInstanceGlobal
{
    DWORD                   Instance;
    DWORD                   dwVirtualNumber;
    D3DXVECTOR3             v3EndPosition;
    D3DXVECTOR3             v3RotationAxis;
    D3DXQUATERNION          qEnd;
    D3DXVECTOR3             v3Center;
    DWORD* ThingInstance;
    DWORD                   dwStartTime;
    DWORD                   dwEndTime;
    DWORD                   eDropSoundType;
    BYTE                    unk2[756];
    char                    stOwnership[24];
} TGroundItemInstanceGlobal;

typedef std::map<DWORD, TGroundItemInstance*>        TGroundItemInstanceMap;
typedef std::map<DWORD, TGroundItemInstanceGlobal*> TGroundItemInstanceMapGlobal;