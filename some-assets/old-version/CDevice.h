#define DirectDevice        IDirect3DDevice9*
#define DirectDevice2       LPDIRECT3DDEVICE9
#define DirectTexture       LPDIRECT3DTEXTURE9
#define EndSceneIndex       42
#define ResetIndex          16

#define DirectX8Device 0x460C1C

class Device {
public:
    static DirectDevice2 pDevice;
};
DirectDevice2 Device::pDevice = NULL;