#define PIPE_NAME "\\\\.\\pipe\\Apathe"
static int server_selectors = 2;

class metin2client
{
public:
	static DWORD C_Entitylist;
	static DWORD m_kAliveInstMap;
	static DWORD C_LocalPlayer;
	static DWORD C_Localfarm;

	static DWORD C_Spoffer;
	static DWORD C_Spoffer_Code;

	static DWORD C_HorseAddr1;
	static DWORD C_HorseAddr2;
	static DWORD C_HorseCoordX;
	static DWORD C_HorseCoordY;
	static DWORD C_HorseCoordZ;

	static DWORD C_LocalAddr;

	static DWORD C_Localfarm_addr;

	static DWORD C_AutoTarget;

	static DWORD C_NoClip;
	static DWORD C_AttackSpeed;
	static DWORD C_MovementSpeed;
	static DWORD C_SwordRange;
	static DWORD C_QuickAge;
	static DWORD C_BodyRecoil;
	static DWORD C_OneHit;
	static DWORD C_AttackAnimation;
	static DWORD C_AttackAnimationSpeed;
	static DWORD C_Horse;
	static DWORD C_SkillRange;

	static DWORD C_Name;
	static DWORD C_Lvl;

	// NoClip v2
	static DWORD C_ModelCoordY;
	static DWORD C_ModelCoordX;
	static DWORD C_ModelCoordZ;

	static DWORD C_ModelVectorX;
	static DWORD C_ModelVectorY;
	static DWORD C_ModelVectorZ;

	static DWORD C_VIDS;
	static DWORD C_CoordsY;
	static DWORD C_CoordsX;
	static DWORD C_CoordsZ;
	static DWORD C_IsDead;
	static DWORD C_IsRotation;
	static DWORD C_IsRadarType;
	static DWORD C_IsSkillRotation;
	static DWORD C_IsCharRotation;
	static DWORD C_IsRangeRotation;
	static DWORD C_IsCharacterType;

	// ws cheats

	static DWORD C_IsKnockback;
	static DWORD C_IsAllKnockBack;

	// ınstance manager
	static DWORD C_CPythonNetworkStream;
	static DWORD C_CPythonApplication;
	static DWORD C_CPythonBackground;
	static DWORD C_CPythonPlayer;
	static DWORD C_ItemVTable;
	static DWORD C_Hearder;
	static DWORD C_Mng;

	// packed
	static DWORD C_SetAttackKeyState;
	static DWORD C_Refresh;
	static DWORD C_SendItemUsePacket;
	static DWORD C_GetTargetVID;
	static DWORD C_SendChatPacket;
	static DWORD C_SendWhisperPacket;
	static DWORD C_GetStatus;
	static DWORD C_SendGuildOfferPacket;
	static DWORD C_MoveToDestPosition;
	static DWORD C_SendBattlePacket;
	static DWORD C_SendSpecial;
	static DWORD C_SendCharacterStatePacket; // Testpaketi
	static DWORD C_SetCameraMaxDistance;
	static DWORD C_GetCurrentMapName;
	static DWORD C_SetArmor;
	static DWORD C_SetAffect;
	static DWORD C_SendAddFlyTargeting;
	static DWORD C_SendShootPacket;
	static DWORD C_SendOnClickPacket;
	static DWORD C_SendItemPickUpPacket;
	static DWORD C_GlobalPositionToMapInfo;
	static DWORD C_SendCommandPacket;
	// Yeniler
	static DWORD C_Disconnect;
	static DWORD C_ConnectTCP;
	// Yeninin Yenisi En iyisi
	static DWORD C_SetPixelPosition;
	static DWORD C_GetPixelPosition;

	// Yeni ler
	static DWORD C_PickCloseItem;
	static DWORD C_ComboAttack;
	static DWORD C_ClickSkillSlot;
	static DWORD C_SendRefinePacket;
	static DWORD C_SendShopSellPacketNew;

	// GroundItem
	static DWORD C_GetNames;
	static DWORD C_PythonItem;
	static DWORD C_CItemManager;

	static DWORD iCItemManagerInstance;
	static DWORD pCItemManagerInstance;
	static DWORD pCPythonItemInstance;
	static DWORD iCPythonItemInstance;

	static void Globals()
	{
		// Lucas2Maraz
		if (server_selectors == 7)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20;


			C_LocalPlayer = 0x604B80;

			C_Localfarm = 0x604B50;

			C_HorseAddr1 = 0x14;
			C_HorseAddr2 = 0x18;

			C_HorseCoordX = 0x484;
			C_HorseCoordY = 0x488;
			C_HorseCoordZ = 0x48C;

			C_LocalAddr = 0x14;

			C_Localfarm_addr = 0x78;
			C_AutoTarget = 0x88;

			C_NoClip = 0x800;
			C_AttackSpeed = 0x738;
			C_MovementSpeed = 0x734;
			C_SwordRange = 0x730;
			C_QuickAge = 0x72C;
			C_BodyRecoil = 0x63C;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x5C0; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x690;

			C_SkillRange = 0x5C0 + 0x3A;

			C_Name = 0x20;
			C_Lvl = 0x58;

			C_ModelVectorX = 0x31C;
			C_ModelVectorY = 0x320;
			C_ModelVectorZ = 0x324;

			C_ModelCoordY = 0x288;
			C_ModelCoordX = 0x284;
			C_ModelCoordZ = 0x28C;

			C_VIDS = 0x7F8;

			C_CoordsY = 0x708;
			C_CoordsX = 0x704;
			C_CoordsZ = 0x70C;

			C_IsDead = 0x6E0;
			C_IsRotation = 0x754;
			C_IsSkillRotation = 0x5EC;
			C_IsCharRotation = 0x744;
			C_IsRangeRotation = 0x358C;
			C_IsRadarType = 0x694;
			C_IsCharacterType = 0x698;

			// ınstance
			C_CPythonNetworkStream = 0x604B60;
			C_CPythonApplication = 0x605170;
			C_CPythonBackground = 0x604B38;
			C_CPythonPlayer = 0x604B50;
			C_ItemVTable = 0x64;
			C_Hearder = 0x08;
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0x127EF0; // C_SetAttackKeyState (cheat engine string kısmında) 
			C_SendItemUsePacket = 0x110400;
			C_SendChatPacket = 0x1031C0;
			C_GetStatus = 0x117CB0;
			C_SendGuildOfferPacket = 0x106C70;
			C_MoveToDestPosition = 0x8CFE0;
			C_SendBattlePacket = 0x105B20;
			C_SendSpecial = 0x105B80;
			C_SendCharacterStatePacket = 0x102FF0;
			C_SendAddFlyTargeting = 0x105EE0;
			C_SendShootPacket = 0x105E90;
			C_SetCameraMaxDistance = 0x1ACED0;
			C_GetCurrentMapName = 0xA8D40;
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0x126E70;
			C_ComboAttack = 0x127730;
			C_ClickSkillSlot = 0x12F830;
			C_Refresh = 0xB0450;
			C_SetPixelPosition = 0x8D340;
			C_GetPixelPosition = 0x210D60;
			C_PickCloseItem = 0x126B70;
			C_SendItemPickUpPacket = 0x110A10;
			C_SendOnClickPacket = 0x103F00;
			C_GlobalPositionToMapInfo = 0xA8AA0;
			C_SendCommandPacket = 0x241D150;
			C_SendRefinePacket = 0x108ED0;
			C_SendShopSellPacketNew = 0x110380;
			// Item Ground
			C_GetNames = 0x7E9F0;
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) 
			C_PythonItem = 0x604B58; // (CPythonPlayer::_OnPressItem String) 
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}
		// Dirilis2Han
		if (server_selectors == 6)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x24; // Yapıldı!!


			C_LocalPlayer = 0x406918; // Yapıldı!! : 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b d1

			C_Localfarm = C_LocalPlayer + 0x14; // Yapıldı!!

			C_HorseAddr1 = 0x10; // Yapıldı!!
			C_HorseAddr2 = 0x8; //Yapıldı!!

			C_HorseCoordX = 0x544; // Yapıldı!!
			C_HorseCoordY = 0x548; // YapıldlI!!
			C_HorseCoordZ = 0x54C; // Yapıldı!!

			C_LocalAddr = 0x10; // Yapıldı!!

			C_Localfarm_addr = 0x84; // Yapıldı!!
			C_AutoTarget = 0x88; // Yapıldı!!

			C_NoClip = 0x3658; // Yapıldı!!
			C_AttackSpeed = 0x3588; // Yapıldı!!
			C_MovementSpeed = 0x3584; // Yapıldı!!
			C_SwordRange = 0x3580; // Yapıldı!!
			C_QuickAge = 0x357C; // Yapıldı!!
			C_BodyRecoil = 0x63C;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x3410; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x34E0; // YapıldI!!

			C_SkillRange = 0x3410 + 0x3A; // Yapıldı!!

			C_Name = 0x20;
			C_Lvl = 0x58;

			C_ModelVectorX = 0x30AC; // YapıldI!!
			C_ModelVectorY = 0x30B0; // YapıldI!!
			C_ModelVectorZ = 0x30B4; // YapıldI!!

			C_ModelCoordY = 0x3018; // Yapıldı!!
			C_ModelCoordX = 0x3014; // Yapıldı!!
			C_ModelCoordZ = 0x301C; // YapıldI!!

			C_VIDS = 0x3650; // Yapıldı!!

			C_CoordsY = 0x3558; // YapıldI!!
			C_CoordsX = 0x3554; // YapıldI!!
			C_CoordsZ = 0x355C; // Yapılldı!!

			C_IsDead = 0x3530; // YapıldI!
			C_IsRotation = 0x3594; // Yapıldı!!
			C_IsSkillRotation = 0x343C; // Yapıldı!!
			C_IsCharRotation = 0x35A4; // YapılldI!!
			C_IsRangeRotation = 0x358C; // Yapıldı!!
			C_IsRadarType = 0x34E4; // Yapıldı!!
			C_IsCharacterType = 0x34E8; // Yapıldı!!

			// ınstance
			C_CPythonNetworkStream = 0x3B77D4; // Yapıldı!! 8b 0d ? ? ? ? 57 ff 75 ? e8 ? ? ? ? 8b ce
			C_CPythonApplication = 0x605170;
			C_CPythonBackground = 0x604B38;
			C_CPythonPlayer = 0x40692C; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? ff 75 ? 8b 0d ? ? ? ? e8
			C_ItemVTable = 0x64;
			C_Hearder = 0x08; // Yapıldı!!
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0xE4A90; // C_SetAttackKeyState (cheat engine string kısmında) Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 68
			C_SendItemUsePacket = 0x110400;
			C_SendChatPacket = 0x1031C0;
			C_GetStatus = 0xDD020; // Yapıldı!! : 55 8b ec 8b 45 ? 3d ? ? ? ? 72 ? 68
			C_SendGuildOfferPacket = 0x106C70;
			C_MoveToDestPosition = 0x8CFE0;
			C_SendBattlePacket = 0xCBAC0; // YapıldI!! 8b ce e8 ? ? ? ? 84 c0 74 ? 8b ce e8 ? ? ? ? 50 e8
			C_SendSpecial = 0xCBB30; // Yapıldı!! e8 ? ? ? ? 84 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? 32 c0 5e 8b e5 5d c2 ? ? 8b ce e8 ? ? ? ? 5e 8b e5 5d c2 ? ? cc cc cc cc cc cc cc cc 55 8b ec 56
			C_SendCharacterStatePacket = 0xC8850; // Yapıldı!! 55 8b ec 83 e4 ? 83 ec ? a1 ? ? ? ? 33 c4 89 44 24 ? 53 56 57 89 4c 24
			C_SendAddFlyTargeting = 0xCBEA0; // Yapıldı!! : e8 ? ? ? ? ff 4c 24
			C_SendShootPacket = 0xCBE50; // Yapıldı!! : e9 ? ? ? ? cc 55 8b ec 8b 45 ? 89 41 ? 8b 45
			C_SetCameraMaxDistance = 0x1ACED0;
			C_GetCurrentMapName = 0xA8D40;
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0xE3940; // YapıldI!! : 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc e9
			C_ComboAttack = 0x127730;
			C_ClickSkillSlot = 0x12F830;
			C_Refresh = 0xB0450;
			C_SetPixelPosition = 0x8D340;
			C_GetPixelPosition = 0x210D60;
			C_PickCloseItem = 0x126B70;
			C_SendItemPickUpPacket = 0x110A10;
			C_SendOnClickPacket = 0x103F00;
			C_GlobalPositionToMapInfo = 0xA8AA0;
			C_SendCommandPacket = 0x241D150;
			C_SendRefinePacket = 0x108ED0;
			C_SendShopSellPacketNew = 0x110380;
			// Item Ground
			C_GetNames = 0x7E9F0;
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) 
			C_PythonItem = 0x604B58; // (CPythonPlayer::_OnPressItem String) 
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}
		// Rohan2
		if (server_selectors == 5)
		{
			C_Entitylist = 0x20; // LocalPlayer : 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 0f b6 81

			m_kAliveInstMap = 0x24; // LocalPlayer : e8 ? ? ? ? 84 c0 75 ? 8d 8e ? ? ? ? 5e

			C_LocalPlayer = GetRVA(Rohan2_SigMaker::C_LocalPlayer, Rohan2_SigMaker::C_LocalPlayerMask, 2);


			C_Localfarm = GetRVA(Rohan2_SigMaker::C_PythonPlayer, Rohan2_SigMaker::C_PythonPlayerMask, 2);
			// 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc c2
			//C_Localfarm = 0x604B50; // YapıldI!!8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 8b 0d ? ? ? ? 83 c1 ? 68 ? ? ? ? 8b 01 ff 50 ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8

			C_HorseAddr1 = 0x10; // YapıldI!!
			C_HorseAddr2 = 0x8; // Yapıldı!!

			C_HorseCoordX = 0x544; // YapıldI!!
			C_HorseCoordY = 0x548; // Yapıldı!!
			C_HorseCoordZ = 0x54C; // Yapıldı!!

			C_LocalAddr = 0x10; //YapıldI!!

			C_Localfarm_addr = 0x50; // Yapıldı!!
			C_AutoTarget = 0x54; // Yapıldı!!

			C_NoClip = 0x96C; // YapıldI!!
			C_AttackSpeed = 0x894; // Yapıldı!!
			C_MovementSpeed = 0x890; // Yapıldı!!
			C_SwordRange = 0x88C; // Yapıldı!!
			C_QuickAge = 0x888; // YapıldI!!
			C_BodyRecoil = 0x834;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x71C; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x7EC; // Yapıldı!!

			C_SkillRange = 0x71C + 0x3A; // Yapıldı!!

			C_Name = 0x10; // yapıldı!!
			C_Lvl = 0x4C; // Yapıldı!!

			C_ModelVectorX = 0x3B8; // Yapıldı!!
			C_ModelVectorY = 0x3BC; // Yapıldı!!
			C_ModelVectorZ = 0x3C0; // Yapıldı!!

			C_ModelCoordY = 0x324; // yapıldı!!
			C_ModelCoordX = 0x320; // Yapıldı!!
			C_ModelCoordZ = 0x328; // Yapıldı!!

			C_VIDS = 0x964; // Yapıldı!!

			C_CoordsY = 0x864; // Yapıdı!!
			C_CoordsX = 0x860; // Yapıldı!!
			C_CoordsZ = 0x868; // Yapıldı!!

			C_IsDead = 0x83C; // Yapıldı!!
			C_IsRotation = 0x8B0; // Yapıldı!!
			C_IsSkillRotation = 0x748; // Yapıldı!!
			C_IsCharRotation = 0x8A0; // Yapıldı!!
			C_IsRangeRotation = 0x358C; 
			C_IsRadarType = 0x7F0; // Yapılldı!!
			C_IsCharacterType = 0x7F4; // Yapıldı!!

			// ınstance
			C_CPythonNetworkStream = 0x604B60;
			C_CPythonApplication = 0x605170;
			C_CPythonBackground = 0x604B38;
			C_CPythonPlayer = 0x604B50;
			C_ItemVTable = 0x64;
			C_Hearder = 0x08;
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0x127EF0; // C_SetAttackKeyState (cheat engine string kısmında) 
			C_SendItemUsePacket = 0x110400;
			C_SendChatPacket = 0x1031C0;
			C_GetStatus = 0x117CB0;
			C_SendGuildOfferPacket = 0x106C70;
			C_MoveToDestPosition = 0x8CFE0;
			C_SendBattlePacket = 0x105B20;
			C_SendSpecial = 0x105B80;
			C_SendCharacterStatePacket = 0x102FF0;
			C_SendAddFlyTargeting = 0x105EE0;
			C_SendShootPacket = 0x105E90;
			C_SetCameraMaxDistance = 0x1ACED0;
			C_GetCurrentMapName = 0xA8D40;
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0x126E70;
			C_ComboAttack = 0x127730;
			C_ClickSkillSlot = 0x12F830;
			C_Refresh = 0xB0450;
			C_SetPixelPosition = 0x8D340;
			C_GetPixelPosition = 0x210D60;
			C_PickCloseItem = 0x126B70;
			C_SendItemPickUpPacket = 0x110A10;
			C_SendOnClickPacket = 0x103F00;
			C_GlobalPositionToMapInfo = 0xA8AA0;
			C_SendCommandPacket = 0x241D150;
			C_SendRefinePacket = 0x108ED0;
			C_SendShopSellPacketNew = 0x110380;
			// Item Ground
			C_GetNames = 0x7E9F0;
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) 
			C_PythonItem = 0x391CEC0; // (CPythonPlayer::_OnPressItem String) // Yapıldı : 8b 0d ? ? ? ? 8d 45 ? 50 56 e8 ? ? ? ? 85 c0 74 ? 8d 45
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}
		// AlbaMt2
		if (server_selectors == 4)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20; // Yapıldı!!

			C_LocalPlayer = GetRVA(AlbaMt2_SigMaker::C_LocalPlayer, AlbaMt2_SigMaker::C_LocalPlayerMask,2); // 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b d1
			C_Localfarm = C_LocalPlayer + 0x4; // Yapıldı!!

			C_HorseAddr1 = 0x14; // Yapıldı!!
			C_HorseAddr2 = 0x8; // Yapıldı!!

			C_HorseCoordX = 0x468; // Yapıldı!!
			C_HorseCoordY = 0x46C; // Yapıldı!!
			C_HorseCoordZ = 0x470; // Yapıldı!!

			C_LocalAddr = 0x14; // Yapıldı!!

			C_Localfarm_addr = 0x90; // Yapıldı!!
			C_AutoTarget = 0x94; // Yapıldı!!

			C_NoClip = 0x8D8; // Yapıldı!!
			C_AttackSpeed = 0x818; // Yapıldı!!
			C_MovementSpeed = 0x814; // Yapıldı!!
			C_SwordRange = 0x810; // Yapıldı!!
			C_QuickAge = 0x80C; // Yapıldı!!
			C_BodyRecoil = 0x7B8;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x6A4; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x774; // Yapıldı!!

			C_SkillRange = 0x6A4 + 0x3A; // Yapıldı!!

			C_Name = 0x10; // Yapıldı!!
			C_Lvl = 0x3C; // Yapıldı!!

			C_ModelVectorX = 0x418; // Yapıldı!!
			C_ModelVectorY = 0x41C; // Yapıldı!!
			C_ModelVectorZ = 0x420; // Yapıldı!!



			C_ModelCoordY = 0x384; // Yapıldı!!
			C_ModelCoordX = 0x380; // Yapıldı!!
			C_ModelCoordZ = 0x388; // Yapıldı!!

			C_VIDS = 0x8D0; // Yapıldı!!

			C_CoordsY = 0x7E8; // Yapıldı!!
			C_CoordsX = 0x7E4; // Yapıldı!!
			C_CoordsZ = 0x7EC; // Yapıldı!!

			C_IsDead = 0x7C0; // Yapıldı!!
			C_IsRotation = 0x834; // Yapıldı!!
			C_IsSkillRotation = 0x5EC;
			C_IsCharRotation = 0x824; // Yapıldı!!
			C_IsRangeRotation = 0x81C; // Yapıldı!!
			C_IsRadarType = 0x778; // Yapıldı!!
			C_IsCharacterType = 0x77C; // Yapıldı!!

			// ınstance
			C_CPythonNetworkStream = GetRVA(AlbaMt2_SigMaker::C_NetworkStream, AlbaMt2_SigMaker::C_NetworkStreamMask, 2); // Yapıldı!! : 8b 0d ? ? ? ? 57 ff 75 ? e8 ? ? ? ? 8b ce
			C_CPythonApplication = GetRVA(AlbaMt2_SigMaker::C_CPythonApplication, AlbaMt2_SigMaker::C_CPythonApplicationMask, 2); // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55
			C_CPythonBackground = GetRVA(AlbaMt2_SigMaker::C_CPythonBackground, AlbaMt2_SigMaker::C_CPythonBackgroundMask, 2); // Yapıldı!! 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 e4
			C_CPythonPlayer = GetRVA(AlbaMt2_SigMaker::C_CPythonPlayer, AlbaMt2_SigMaker::C_CPythonPlayerMask,2);
			//C_CPythonPlayer = 0x604B50;
			C_ItemVTable = 0x64;
			C_Hearder = 0x08;
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0xA95E0; // C_SetAttackKeyState (cheat engine string kısmında)  e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 68
			C_SendItemUsePacket = 0x9A380; // Yapıldı!!  : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc 55 8b ec 83 ec ? 56
			C_SendChatPacket = 0x8E260; // Yapıldı!! e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75
			C_GetStatus = 0xA38E0; // Yapıldı!! 55 8b ec 8b 45 ? 3d ? ? ? ? 72 ? 68
			C_SendGuildOfferPacket = 0x91D00; // YapılldI!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 8d 45 ? 50
			C_MoveToDestPosition = 0x500D0; /// Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc 55
			C_SendBattlePacket = GetCallRVA(AlbaMt2_SigMaker::C_CSendBattlePacket, AlbaMt2_SigMaker::C_CSendBattlePacketMask, 1, 5); // Yapıldı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? 84 c0 74 ? 8b ce
			C_SendSpecial = 0x90BA0; // Yapıldı!!
			C_SendCharacterStatePacket = 0x8DF70; // Yapıldı!! : 55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 53 56 57 89 4d ? eb ? d6 d7 01 e8
			C_SendAddFlyTargeting = GetCallRVA(AlbaMt2_SigMaker::C_SendAddFlyTargeting, AlbaMt2_SigMaker::C_SendAddFlyTargetingMask, 1, 5);
			C_SendShootPacket = 0x90EC0; // Yapıldı!! : e9 ? ? ? ? cc 55 8b ec 8b 45 ? 89 41
			C_SetCameraMaxDistance = 0x109FB0; // Yapıldı!! : e8 ? ? ? ? 83 c4 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc
			C_GetCurrentMapName = 0x5DE50; // Yapıldı!!
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0xA8560; // Yapıldı!! : 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc e9
			C_ComboAttack = 0xA8E30; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8b 0d
			C_ClickSkillSlot = 0xBF180; // YapıldI!! e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55
			C_Refresh = 0x60C90; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0
			C_SetPixelPosition = 0x503F0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 5f
			C_GetPixelPosition = 0x50450; // Yapıldı!! : ff 50 ? 85 c0 75 ? e8
			C_PickCloseItem = 0xA8130; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51
			C_SendItemPickUpPacket = 0x9A8C0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 56 8b 75 ? b8 ? ? ? ? 56 c6 45 ? ? 66 89 45 ? ff 15 ? ? ? ? 83 c4 ? 83 e8 ? 74 ? 83 e8 ? 0f 85
			C_SendOnClickPacket = 0x8F0E0; // YapıldI!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 56 8b 75 ? b8 ? ? ? ? 56 c6 45 ? ? 66 89 45 ? ff 15 ? ? ? ? 83 c4 ? 83 e8 ? 74 ? 83 e8 ? 75
			C_GlobalPositionToMapInfo = 0xA8AA0;
			C_SendCommandPacket = 0x241D150;
			C_SendRefinePacket = 0x93660; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 8b 0d
			C_SendShopSellPacketNew = 0x9A2D0;
			// Item Ground
			C_GetNames = 0x60C90; // Yapıldı!! : e8 ? ? ? ? 85 c0 75 ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 8b c8 e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc 55
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) Yapıldı!! : 8b 0d ? ? ? ? 50 56 e8 ? ? ? ? 84 c0 0f 84
			C_PythonItem = 0x39CDAC; // (CPythonPlayer::_OnPressItem String)  Yapıldı!! : 8b 0d ? ? ? ? 8d 45 ? 50 56 e8 ? ? ? ? 85 c0 74 ? 8b 4d ? 8d 45
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}

		// Lycia2
		if (server_selectors == 3)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20;

			uintptr_t LocalPlayer = FindPattern(NULL, Lycia2_SigMaker::C_LocalPlayer_New, Lycia2_SigMaker::C_LocalPlayer_NewMask);

			if (LocalPlayer)
			{
				uintptr_t absoluteAddress = *(uintptr_t*)(LocalPlayer + 2);
				uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
				uintptr_t rva = absoluteAddress - base;

				C_LocalPlayer = rva;
			}
			//C_LocalPlayer = 0x37D87C; // YapıldI!!

			C_Localfarm = C_LocalPlayer + 0x38; // Yapıldı!!

			C_HorseAddr1 = 0x14; // YapıldI!!
			C_HorseAddr2 = 0xC; // Yapıldı!!

			C_HorseCoordX = 0x17CC; // Yapıldı!!
			C_HorseCoordY = 0x17D0; // YapıldI!!
			C_HorseCoordZ = 0x17D4;

			C_LocalAddr = 0x14; // YapıldI!!

			C_Localfarm_addr = 0x64; // Yapıldı!!
			C_AutoTarget = 0x6C; // Yapıldı!!

			C_NoClip = 0x1C50; // Yapıldı!!
			C_AttackSpeed = 0x1B80; // Yapıldı!!
			C_MovementSpeed = 0x1B7C; // Yapıldı!!
			C_SwordRange = 0x1B78; // Yapıldı!!
			C_QuickAge = 0x1B74; // Yapıldı!!
			C_BodyRecoil = 0x1B20;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x1A0C; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x1ADC; // Yapıldı!!

			C_SkillRange = 0x1A0C + 0x3A; // Yapıldı!!

			C_Name = 0x14; //7 Yapıldı!!
			C_Lvl = 0x50; // Yapıldı!!

			C_ModelVectorX = 0x384; // Yapıldı!!
			C_ModelVectorY = 0x388; // Yapıldı!!
			C_ModelVectorZ = 0x38C; // Yapıldı!!

			C_ModelCoordY = 0x420; // yapıldı!!
			C_ModelCoordX = 0x41C; // Yapıldı!!
			C_ModelCoordZ = 0x424; // YapıldI!!

			C_VIDS = 0x1C48; // Yapıldı!!

			C_CoordsY = 0x1B50; // Yapıldı!!
			C_CoordsX = 0x1B4C; // Yapıldı!!
			C_CoordsZ = 0x1B54; // Yapıldı!!

			C_IsDead = 0x1B28; // Yapıldı!!
			C_IsRotation = 0x1B9C; // Yapıldı!!
			C_IsSkillRotation = 0x1A38; // Yapıldı!!
			C_IsCharRotation = 0x1B8C; // Yapıldı!!
			C_IsRangeRotation = 0x358C;
			C_IsRadarType = 0x1AE0; // YapıldI!!
			C_IsCharacterType = 0x1AE4; // YapıldI!!

			// ınstance
			C_CPythonNetworkStream = 0x37FDF8; // Yapıldı!! 8b 0d ? ? ? ? 57 ff 75 ? e8 ? ? ? ? 8b ce
			C_CPythonApplication = 0x3821F0; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc ff 35
			C_CPythonBackground = 0x37D8AC; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8b 0d ? ? ? ? 8d 45 ? 50 8d 45 ? 50 8d 45
			C_CPythonPlayer = 0x37D8BC; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 8b 0d ? ? ? ? 83 c1 ? 8b 01 ff 90 ? ? ? ? 8b c8 33 c0 85 c9 74 ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc 8b 0d ? ? ? ? e8
			C_ItemVTable = 0x64;
			C_Hearder = 0x08; // yapıldI!!
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0x127EF0; // C_SetAttackKeyState (cheat engine string kısmında) 
			C_SendItemUsePacket = 0x1099E0; // Yapıldı!!
			C_SendChatPacket = 0x105090; // Yapıldı!! e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8
			C_GetStatus = 0x958C0; // Yapıldı!! 55 8b ec 8b 45 ? 3d ? ? ? ? 72 ? 68
			C_SendGuildOfferPacket = 0x105EA0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8b 0d ? ? ? ? 8d 45
			C_MoveToDestPosition = 0x6E630; // Yapıldı!! e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc
			C_SendBattlePacket = 0x1047D0; // Yapıldı!! e8 ? ? ? ? 8b ce e8 ? ? ? ? 84 c0 0f 84 ? ? ? ? 8b ce
			C_SendSpecial = 0x106B90; // yapıldI!! e8 ? ? ? ? 84 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? 32 c0 5e 8b e5 5d c2 ? ? 8b ce e8 ? ? ? ? 5e 8b e5 5d c2 ? ? 55 8b ec 83 ec ? 56 8b f1 e8 ? ? ? ? 84 c0 74
			C_SendCharacterStatePacket = 0x104C40; // Yapıldı!! :  55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 53 56 57 89 4d
			C_SendAddFlyTargeting = 0x105970; // Yapıldı!! e8 ? ? ? ? 5f 5e 8b e5 5d c3 68
			C_SendShootPacket = 0x106A70; // Yapıldı!! e9 ? ? ? ? cc 55 8b ec 8b 45 ? 6a
			C_SetCameraMaxDistance = 0x13C3C0; // Yapıldı!! e8 ? ? ? ? 83 c4 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? a1
			C_GetCurrentMapName = 0x72150; // Yapıldı!!e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8b 0d ? ? ? ? 8d 45 ? 50 8d 45 ? 50 8d 45
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0x991E0; // YapıldI!! 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc 55 8b ec 56 ff 75 ? 8b f1 8b 4d
			C_ComboAttack = 0x98720; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? 83 c1
			C_ClickSkillSlot = 0xB2DB0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d
			C_Refresh = 0xB0450;
			C_SetPixelPosition = 0x6ECA0; // Yapıldı!! e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 5f 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc
			C_GetPixelPosition = 0x6EBF0; // Yapıldı!! : 8b e5 5d c3 8b 0d ? ? ? ? ff 75 ? 83 c1 ? 8b 01 ff 50 ? 85 c0 74 ? 8d 4d
			C_PickCloseItem = 0x98D50; // Yapıldı!!: e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8b 0d
			C_SendItemPickUpPacket = 0x109920; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55
			C_SendOnClickPacket = 0x106580; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 6a ? 68 ? ? ? ? 64 a1 ? ? ? ? 50 83 ec
			C_GlobalPositionToMapInfo = 0xA8AA0;
			C_SendCommandPacket = 0x241D150;
			C_SendRefinePacket = 0x106950; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 56
			C_SendShopSellPacketNew = 0x109F70; // yapıldı!!
			// Item Ground
			C_GetNames = 0x7E9F0;
			C_CItemManager = 0x37D8C4; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? c3 8b c8 e8 ? ? ? ? 52 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? c3 8b c8 e8 ? ? ? ? 50
			C_PythonItem = 0x37FF30; // (CPythonPlayer::_OnPressItem String) Yapıldı! : 8b 0d ? ? ? ? 8d 45 ? 50 56 e8
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}
		
		// Alves2
		if (server_selectors == 2)
		{
			C_Entitylist = 0x24;

			m_kAliveInstMap = 0x20; // Yapıldı!! 


			/*uintptr_t LocalPlayer = FindPattern(NULL, Alves2_SigMaker::C_LocalPlayer_New, Alves2_SigMaker::C_LocalPlayer_NewMask);

			if (LocalPlayer)
			{
				uintptr_t absoluteAddress = *(uintptr_t*)(LocalPlayer + 2);
				uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
				uintptr_t rva = absoluteAddress - base;

				C_LocalPlayer = rva;
			}*/
			C_LocalPlayer = 0x3C2D68; // Yapıldı!! : 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b d1

			C_Localfarm = C_LocalPlayer + 0x1C; // Yapıldı!!.

			C_HorseAddr1 = 0x14; // Yapıldı!! 
			C_HorseAddr2 = 0x8; // Yapıldı!!

			C_HorseCoordX = 0x39C; // Yapıldı!!
			C_HorseCoordY = 0x3A0; // Yapıldı!!
			C_HorseCoordZ = 0x3A4; // Yapıldı!!

			C_LocalAddr = 0x14; // Yapıldı!! 

			C_Localfarm_addr = 0x80; // Yapıldı!! 
			C_AutoTarget = 0x84; // Yapıldı!!

			C_NoClip = 0x6E8; // Yapıldı!!
			C_AttackSpeed = 0x628; // Yapıldı!!
			C_MovementSpeed = 0x624; // Yapıldı!!
			C_SwordRange = 0x620; // Yapıldı!!
			C_QuickAge = 0x61C; // Yapıldı!!
			C_BodyRecoil = 0x5BC;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x4B4; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x584; // Yapıldı!!

			C_SkillRange = 0x4B4 + 0x3A; // Yapıldı!!

			C_Name = 0x10; // Yapıldı!!
			C_Lvl = 0x4C;

			C_ModelVectorX = 0x2F4; // Yapıldı!!
			C_ModelVectorY = 0x2F8; // Yapıldı!!
			C_ModelVectorZ = 0x2FC; // Yapıldı!!

			C_ModelCoordY = 0x260; // Yapıldı!!
			C_ModelCoordX = 0x25C; // Yapıldı!!
			C_ModelCoordZ = 0x264; // Yapıldı!!

			C_VIDS = 0x6E0; // Yapıldı!!

			C_CoordsY = 0x5F8; // Yapıldı!!
			C_CoordsX = 0x5F4; // Yapıldı!
			C_CoordsZ = 0x5FC; // Yapıldı!!

			C_IsDead = 0x5D0; // Yapılldı!!
			C_IsRotation = 0x644; // Yapıldı!!
			C_IsSkillRotation = 0x4E0; // Yapıldı!!
			C_IsCharRotation = 0x634; // Yapıldı!!
			C_IsRangeRotation = 0x358C;
			C_IsRadarType = 0x588; // Yapıldı!!
			C_IsCharacterType = 0x58C; // yapıldı!!

			// ınstance
			C_CPythonNetworkStream = 0x388518; // Yapıldı!! : 8b 0d ? ? ? ? 57 ff 75 ? e8 ? ? ? ? 8b ce
			C_CPythonApplication = 0x38856C; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55
			C_CPythonBackground = 0x388520; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 e4 ? 83 ec
			C_CPythonPlayer = 0x3C2D84; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? ff 75 ? 8b 0d ? ? ? ? e8
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0xBFE30; // C_SetAttackKeyState (cheat engine string kısmında) : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 68
			C_SendItemUsePacket = 0xB4160; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 56 
			C_SendChatPacket = 0xAA040; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75
			C_SendWhisperPacket = 0xAA9C0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 51
			C_GetStatus = 0xB8D30; // Yapıldı!! : 55 8b ec 8b 45 ? 3d ? ? ? ? 72 ? 68
			C_SendGuildOfferPacket = 0xAD9F0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 8d 45 ? 50
			C_MoveToDestPosition = 0x6BFD0; // Yapılı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc
			C_SendBattlePacket = 0xAC8A0; // Yapıldı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? 84 c0 74 ? 8b ce
			C_SendCharacterStatePacket = 0xA9B40; // Yapıldı!! : 55 8b ec 83 e4 ? 83 ec ? a1 ? ? ? ? 33 c4 89 44 24 ? 53 56 57 89 4c 24
			C_SendAddFlyTargeting = 0xACC90; // Yapıldı!! : e8 ? ? ? ? ff 4c 24
			C_SendShootPacket = 0xACC40; // Yapıldı!! : e9 ? ? ? ? cc 55 8b ec 8b 45 ? 89 41
			C_SetCameraMaxDistance = 0x1617C0; // Yapıldı!! : e8 ? ? ? ? 83 c4 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc
			C_GetCurrentMapName = 0x79F70; // Yapıldı!! : e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 e4 ? 83 ec
			C_SetArmor = 0x64090; // Yapılı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? e8 ? ? ? ? 5e
			C_SetAffect = 0x80D10; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 8b 55
			C_GetTargetVID = 0xBED70; // Yapıldı!! : 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc e9
			C_ComboAttack = 0xBF650; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8b 0d
			C_ClickSkillSlot = 0xD71B0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55
			C_Refresh = 0x7D940; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0
			C_SetPixelPosition = 0x6C2F0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 5f
			C_GetPixelPosition = 0x6C350; // Yapıldı!! : 55 8b ec 81 c1 ? ? ? ? e8 ? ? ? ? 8b 4d
			C_PickCloseItem = 0xBE9E0; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51
			C_SendItemPickUpPacket = 0x242C5C0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 56
			C_SendOnClickPacket = 0xAADC0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 56
			C_GlobalPositionToMapInfo = 0x79D70; // Yapıldı!!: e8 ? ? ? ? 85 c0 74 ? 83 78 ? ? 72
			C_SendCommandPacket = 0x241D150; // Yok!
			C_SendRefinePacket = 0xAF400; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 8b 0d
            // Yeniler
			C_ConnectTCP = 0x1F30D0; // Yapıldı!! : e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? ff 75 ? 8b 0d ? ? ? ? ff 75 ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec
			C_Disconnect = 0xA29B0; // Yapılı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? 5e e9 ? ? ? ? cc cc cc cc
			
			// Mining Bot
		}
		// Celestial Worldd
		if (server_selectors == 1)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20; // Yapıldı!!


			uintptr_t LocalPlayer = FindPattern(NULL, CelestialWorld_SigMaker::C_LocalPlayer_New, CelestialWorld_SigMaker::C_LocalPlayer_NewMask);

			if (LocalPlayer)
			{
				uintptr_t absoluteAddress = *(uintptr_t*)(LocalPlayer + 2);
				uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
				uintptr_t rva = absoluteAddress - base;

				C_LocalPlayer = rva;
			}
			//C_LocalPlayer = 0x45806C; // Yapıldı!! 8b 0d ? ? ? ? e8 ? ? ? ? 3b f0 75 ? 68

			C_Localfarm = C_LocalPlayer + 0x40;

			C_HorseAddr1 = 0x14; // Yapılı!!
			C_HorseAddr2 = 0x8; // Yapıldı!! 

			C_HorseCoordX = 0x540; // YapıldI!!
			C_HorseCoordY = 0x544; // Yapılı!!
			C_HorseCoordZ = 0x548; // Yapıldı!!

			C_LocalAddr = 0x14; // Yapıldı!! 

			C_Localfarm_addr = 0x50; // Yapıldı!!
			C_AutoTarget = 0x54; // Yapıldı!!

			C_NoClip = 0x780; // Yapıldı!!
			C_AttackSpeed = 0x6A8; // Yapıldı!!
			C_MovementSpeed = 0x6A4; // Yapıldı!!
			C_SwordRange = 0x6A0; // Yapıldı!!
			C_QuickAge = 0x69C; // Yapıldı!!
			C_BodyRecoil = 0x63C;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x528; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x5F8; // Yapıldı!!

			C_SkillRange = 0x528 + 0x3A; // Yapıldı!!

			C_Name = 0x14; // Yapıldı!! 
			C_Lvl = 0x44; // yapıldı!!

			C_ModelVectorX = 0x1D0; // Yapıldı!!
			C_ModelVectorY = 0x1D4; // Yapıldı!!
			C_ModelVectorZ = 0x1D8; // YapıldI!!

			C_ModelCoordY = 0x13C; // YapıldI!!
			C_ModelCoordX = 0x138; // Yapılı!!
			C_ModelCoordZ = 0x140; // Yapıldı!!

			C_VIDS = 0x764; // Yapıldı!!

			C_CoordsY = 0x678; // Yapıldı!!
			C_CoordsX = 0x674; // Yapılı!!
			C_CoordsZ = 0x67C; // Yapılı!!

			C_IsDead = 0x64C;// YapıldI!!
			C_IsRotation = 0x6C4; // Yapılı!!
			C_IsSkillRotation = 0x554; // Yapıldı!!
			C_IsCharRotation = 0x6B4; // Yapılı!!
			C_IsRadarType = 0x5FC; // Yapıldı!!
			C_IsCharacterType = 0x600; // Yapıldı!!

			// ınstance
			C_CPythonNetworkStream = 0x458044; // Yapıldı!! : ff 90 ? ? ? ? 83 7d ? ? 74
			C_CPythonApplication = 0x45C4C8; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc ff 35
			C_CPythonBackground = 0x45804C; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8b 0d ? ? ? ? 8d 45 ? 50 8d 45 ? 50 8d 45
			C_CPythonPlayer = 0x4580B0; // Yapıldı!!: 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 51 56 8b 75
			C_ItemVTable = 0x64; // YapıldI!!
			C_Hearder = 0xDC; // Yapıldı!! 
			C_Mng = 0x4; // Yapıldı!!

			// packed
			C_SetAttackKeyState = 0xFB910; // C_SetAttackKeyState (cheat engine string kısmında) : Yapıldı! :: 8b 0d ? ? ? ? 0f 95 ? 0f b6 ? 50 e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55
			C_SendItemUsePacket = 0xEA2F0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc 55 8b ec 83 ec ? 56 8b 75 ? b8 ? ? ? ? 56 c6 45 ? ? 66 89 45 ? c6 45 
			C_SendChatPacket = 0xDFCD0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc 55 8b ec 51
			C_GetStatus = 0xF5B70; // Yapıldı!! : 55 8b ec 8b 45 ? 83 ec ? 3d 
			C_SendGuildOfferPacket = 0xE0940; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8b 0d 
			C_MoveToDestPosition = 0x80610; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 
			C_SendBattlePacket = 0xDF970; // Yapıldı!! : 55 8b ec 83 ec ? 56 8b f1 e8 ? ? ? ? 84 c0 75 ? b0 ? 5e 8b e5 5d c2 ? ? 8a 45
			C_SendSpecial = 0x14FF20; // Yapıldı : e8 ? ? ? ? 84 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? 32 c0 5e 8b e5 5d c2 ? ? 8b ce e8 ? ? ? ? 5e 8b e5 5d c2 ? ? 55 8b ec 83 ec
			C_SendCharacterStatePacket = 0xDFB60; // Yapıldı!! : 55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 53 56 57 89 4d ? eb ? d6 d7 01 8b ? ? ? ? e8 
			C_SendAddFlyTargeting = 0xE0400; // Yapıldı!! : e8 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? a3 ? ? ? ? 5e 
			C_SendShootPacket = 0xE19A0; // YapıldI!! : e9 ? ? ? ? cc c2
			C_SetCameraMaxDistance = 0x153610; // Yapıldı!! : e8 ? ? ? ? 83 c4 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? a1 
			C_GetCurrentMapName = 0x94C40; // Yapıldı!! : e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 ec ? 8b 0d ? ? ? ? 8d 45 ? 50 8d 45 ? 50 8d 45 
			C_SetArmor = 0x71450; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc e9 
			C_SetAffect = 0x7BFC0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5f 5e 8b e5 5d c3 cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 
			C_GetTargetVID = 0xF9E90; //  Yapıldı!! : 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 56 8b f1
			C_ComboAttack = 0xF9EA0; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? c6 45 
			C_ClickSkillSlot = 0x103F70; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? c6 45 
			C_Refresh = 0x9AB90; // Yapıldı!!:  e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0 
			C_SetPixelPosition = 0x80F00; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 5f 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc 55
			C_GetPixelPosition = 0x210D60; // ( GF den bakılıcak)
			C_PickCloseItem = 0xFA7B0; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8b 0d 
			C_SendItemPickUpPacket = 0xEA290; //  YapıldI!!: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 56 8b 75 ? b8 ? ? ? ? 56 c6 45 ? ? 66 89 45 ? c6 45
			C_SendOnClickPacket = 0xE1060; // Yapıldı!!: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 6a ? 68 ? ? ? ? 64 a1 ? ? ? ? 50 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 56 57 
			C_GlobalPositionToMapInfo = 0x94C80; // YapıldI!! : e8 ? ? ? ? 85 c0 74 ? 83 78 ? ? 8b c8
			C_SendCommandPacket = 0x241D150; //  (GF de var burda yok)
			C_SendRefinePacket = 0xE1570; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 e8 
			C_SendShopSellPacketNew = 0xEA9B0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? ff 75 ? 8b 0d ? ? ? ? ff 75 ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8b 45
			// Item Ground
			C_GetNames = 0x6F5D0; //  e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8b 0d ? ? ? ? ff 75 ? 83 c1 ? 8b 01 ff 50 ? 85 c0 75 ? 68
			C_CItemManager = 0x4580B8; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? c3 8b c8 e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? c7 45 ? ? ? ? ? 50 6a ? ff 75 ? c7 45
			C_PythonItem = 0x45C488; // 8b 0d ? ? ? ? 8d 45 ? 50 e8 ? ? ? ? 84 c0 74 ? ff 75 ? 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 6a ? 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 55 8b ec 51
			// Yniler
			C_ConnectTCP = 0xCC150; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 56 8b 75 ? 50 6a ? 56 e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 5e 8b e5 5d c3 8d 45 ? 50 6a ? 56 e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8d 45 ? 50 6a ? 56 e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8d 45 ? 50 6a ? 56 e8 ? ? ? ? 83 c4 ? 84 c0 74 ? ff 75 ? 8b 0d ? ? ? ? ff 75 ? ff 75 ? ff 75 ? e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? e8
			C_Disconnect = 0xCC790; // Yapıldı!!: e8 ? ? ? ? 8b ce e8 ? ? ? ? 5e e9 ? ? ? ? cc cc cc cc
		}
		// Elaris
		if (server_selectors == 0)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20; // Yapıldı!!


			uintptr_t LocalPlayer = FindPattern(NULL, Elaris_SigMaker::C_LocalPlayer, Elaris_SigMaker::C_LocalPlayerMask);
			if (LocalPlayer)
			{
				DWORD offset = *(DWORD*)(LocalPlayer + 2);
				uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
				uintptr_t rva = offset - base;
				C_LocalPlayer = rva;
			}
			//C_LocalPlayer = 0x604B80; // Yapıldı! : 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 8b 0d ? ? ? ? e9 ? ? ? ? cc cc cc cc cc 56

			uintptr_t CPythonPlayer = FindPattern(NULL, Elaris_SigMaker::CPythonPlayer, Elaris_SigMaker::CPythonPlayerMask);
			if (CPythonPlayer)
			{
				DWORD offset = *(DWORD*)(LocalPlayer + 2);
				uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
				uintptr_t rva = offset - base;

				C_Localfarm = rva;
			}

			//C_Localfarm = 0x604B50; // Yapıldı!! : 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 6a ? 68 ? ? ? ? 64 a1 ? ? ? ? 50 83 ec ? 53

			C_HorseAddr1 = 0x14; // Yapıldı!! 
			C_HorseAddr2 = 0x18; // Yapıldı!! 

			C_HorseCoordX = 0x484; // Yapıldı!!
			C_HorseCoordY = 0x488; // Yapıldı!!
			C_HorseCoordZ = 0x48C; // Yapıldı!!

			C_LocalAddr = 0x14; // Yapıldı!!

			C_Localfarm_addr = 0x78; // Yapıldı!! 
			C_AutoTarget = 0x88; 

			C_NoClip = 0x800; // Yapıldı!
			C_AttackSpeed = 0x738; // Yapıldı!!
			C_MovementSpeed = 0x734; // Yapıldı!!
			C_SwordRange = 0x730; // Yapıldı!!
			C_QuickAge = 0x72C; // Yapıldı!!
			C_BodyRecoil = 0x63C;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x5C0; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x690; // Yapıldı!!

			C_SkillRange = 0x5C0 + 0x3A; // YapıldI!!

			C_Name = 0x20; // Yapıldı!!
			C_Lvl = 0x58; // Yapıldı!!

			C_ModelVectorX = 0x31C; // Yapıldı!!
			C_ModelVectorY = 0x320; // YapıldI!!
			C_ModelVectorZ = 0x324; // Yapıldı!!

			C_ModelCoordY = 0x288; // Yapıldı!!
			C_ModelCoordX = 0x284; /// YapıldI!!
			C_ModelCoordZ = 0x28C; // Yapıldı!!

			C_VIDS = 0x7FC; // Yapıldı!

			C_CoordsY = 0x708; // yapıldI!!
			C_CoordsX = 0x704; // Yapıldı!!
			C_CoordsZ = 0x70C; // Yapıldı!!

			C_IsDead = 0x6E0; // Yapıldı!!
			C_IsRotation = 0x754; // Yapıldı!!
			C_IsSkillRotation = 0x5EC; // Yapıldı!!
			C_IsCharRotation = 0x744; // Yapıldı!!
			C_IsRangeRotation = 0x358C;
			C_IsRadarType = 0x694; // Yapıldı!!
			C_IsCharacterType = 0x698; // Yapıldı!!

			// ınstance
			C_CPythonNetworkStream = 0x604B60; // Yapıldı! : 8b 0d ? ? ? ? 57 ff 75 ? e8 ? ? ? ? 8b ce
			C_CPythonApplication = 0x605170; // Yapıldı!! 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55
			C_CPythonBackground = 0x604B38;  // Yapıldı!!: 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 e4
			C_CPythonPlayer = 0x604B50; // Yapıldı!! 8b 0d ? ? ? ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 6a ? 68 ? ? ? ? 64 a1 ? ? ? ? 50 83 ec ? 53
			C_ItemVTable = 0x64; 
			C_Hearder = 0x08; /// Yapıldı!! 
			C_Mng = 0x4; 

			// packed
			C_SetAttackKeyState = 0x127EF0; // C_SetAttackKeyState (cheat engine string kısmında) : Yapıldı! :: 8b 0d ? ? ? ? 0f 95 ? 0f b6 ? 50 e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55
			C_SendItemUsePacket = 0x110400; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc 55 8b ec 83 ec ? 56 8b 75 ? b8 ? ? ? ? 56 c6 45 ? ? 66 89 45 ? c6 45
			C_SendChatPacket = 0x1031C0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a
			C_GetStatus = 0x117CB0; // Yapıldı!! : 55 8b ec 8b 45 ? 3d ? ? ? ? 72 ? 68
			C_SendGuildOfferPacket = 0x106C70; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 8d 45 ? 50
			C_MoveToDestPosition = 0x8CFE0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd 5e e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc
			C_SendBattlePacket = 0x105B20; // Yapıldı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? 84 c0 74 ? 8b ce e8 ? ? ? ? 50
			C_SendSpecial = 0x105B80; // Yapıldı : e8 ? ? ? ? 84 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? 32 c0 5e 8b e5 5d c2 ? ? 8b ce e8 ? ? ? ? 5e 8b e5 5d c2 ? ? cc 55 8b ec 8b 55
			C_SendCharacterStatePacket = 0x102FF0; // Yapıldı!! : 55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 53 56 57 89 4d ? eb ? d6 d7 01 e8
			C_SendAddFlyTargeting = 0x105EE0; // Yapıldı!! : e8 ? ? ? ? 83 6d ? ? 75
			C_SendShootPacket = 0x105E90; // YapıldI!! : e9 ? ? ? ? cc 55 8b ec 8b 45 ? 89 41 ? 8b 45
			C_SetCameraMaxDistance = 0x1ACED0; // Yapıldı!! : e8 ? ? ? ? 83 c4 ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc
			C_GetCurrentMapName = 0xA8D40; // Yapıldı!! : e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc 55 8b ec 83 e4
			C_SetArmor = 0x7F350; // Yapıldı!! : e8 ? ? ? ? 8b ce e8 ? ? ? ? e8 ? ? ? ? 5e
			C_SetAffect = 0xB6790; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 8b 55
			C_GetTargetVID = 0x126E70; //  Yapıldı!! : 8b 81 ? ? ? ? c3 cc cc cc cc cc cc cc cc cc e9
			C_ComboAttack = 0x127730; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? 8b 0d
			C_ClickSkillSlot = 0x12F830; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8 ? ? ? ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55
			C_Refresh = 0xB0450; // Yapıldı!!:  e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0
			C_SetPixelPosition = 0x8D340; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 5f 33 cd
			C_GetPixelPosition = 0x210D60; // ( GF den bakılıcak)
			C_PickCloseItem = 0x126B70; // Yapıldı!! : e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? e8
			C_SendItemPickUpPacket = 0x110A10; //  YapıldI!!: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec ? 56 8b 75 ? b8
			C_SendOnClickPacket = 0x103F00; // Yapıldı!!: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 51 56
			C_GlobalPositionToMapInfo = 0xA8AA0; // YapıldI!! : e8 ? ? ? ? 85 c0 74 ? 83 78 ? ? 8b 48
			C_SendCommandPacket = 0x241D150; //  (GF de var burda yok)
			C_SendRefinePacket = 0x108ED0; // Yapıldı!! :e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 8b 0d
			C_SendShopSellPacketNew = 0x110380; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 5e 8b e5 5d c3 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 55 8b ec 83 ec
			// Item Ground
			C_GetNames = 0x7E9F0; //  e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? c3 cc cc cc cc cc cc cc cc cc cc 55 8b ec 51
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) : Yapıldı!! :  8b 0d ? ? ? ? e8 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? 85 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? e8
			C_PythonItem = 0x604B58; // (CPythonPlayer::_OnPressItem String) 8b 0d ? ? ? ? 8d 45 ? 50 56 e8 ? ? ? ? 85 c0 74 ? 8b 4d ? 8d 45
			// Yniler
			C_ConnectTCP = 0xFA060; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 74 ? e8
			C_Disconnect = 0x1050E0; // Yapıldı!!: e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 8b 0d ? ? ? ? e8 ? ? ? ? e9 ? ? ? ? 56
		}

		// Alınıcak kod
		if (server_selectors == 89992323)
		{
			C_Entitylist = 0x20;

			m_kAliveInstMap = 0x20; 


			C_LocalPlayer = 0x604B80; 

			C_Localfarm = 0x604B50;

			C_HorseAddr1 = 0x14; 
			C_HorseAddr2 = 0x18; 

			C_HorseCoordX = 0x484; 
			C_HorseCoordY = 0x488; 
			C_HorseCoordZ = 0x48C; 

			C_LocalAddr = 0x14; 

			C_Localfarm_addr = 0x78;
			C_AutoTarget = 0x88;

			C_NoClip = 0x800; 
			C_AttackSpeed = 0x738; 
			C_MovementSpeed = 0x734;
			C_SwordRange = 0x730; 
			C_QuickAge = 0x72C; 
			C_BodyRecoil = 0x63C;  // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
			C_AttackAnimationSpeed = 0x5C0; // + 11 gidersek bize Skill Rotation verir
			C_Horse = 0x528;
			C_OneHit = 0x690; 

			C_SkillRange = 0x5C0 + 0x3A;

			C_Name = 0x20; 
			C_Lvl = 0x58; 

			C_ModelVectorX = 0x31C; 
			C_ModelVectorY = 0x320; 
			C_ModelVectorZ = 0x324;

			C_ModelCoordY = 0x288;
			C_ModelCoordX = 0x284;
			C_ModelCoordZ = 0x28C;

			C_VIDS = 0x7F8;

			C_CoordsY = 0x708; 
			C_CoordsX = 0x704;
			C_CoordsZ = 0x70C;

			C_IsDead = 0x6E0;
			C_IsRotation = 0x754; 
			C_IsSkillRotation = 0x5EC; 
			C_IsCharRotation = 0x744; 
			C_IsRangeRotation = 0x358C;
			C_IsRadarType = 0x694; 
			C_IsCharacterType = 0x698; 

			// ınstance
			C_CPythonNetworkStream = 0x604B60; 
			C_CPythonApplication = 0x605170; 
			C_CPythonBackground = 0x604B38;  
			C_CPythonPlayer = 0x604B50; 
			C_ItemVTable = 0x64;
			C_Hearder = 0x08; 
			C_Mng = 0x4;

			// packed
			C_SetAttackKeyState = 0x127EF0; // C_SetAttackKeyState (cheat engine string kısmında) 
			C_SendItemUsePacket = 0x110400; 
			C_SendChatPacket = 0x1031C0; 
			C_GetStatus = 0x117CB0; 
			C_SendGuildOfferPacket = 0x106C70;
			C_MoveToDestPosition = 0x8CFE0; 
			C_SendBattlePacket = 0x105B20;
			C_SendSpecial = 0x105B80; 
			C_SendCharacterStatePacket = 0x102FF0; 
			C_SendAddFlyTargeting = 0x105EE0; 
			C_SendShootPacket = 0x105E90; 
			C_SetCameraMaxDistance = 0x1ACED0; 
			C_GetCurrentMapName = 0xA8D40; 
			C_SetArmor = 0x7F350;
			C_SetAffect = 0xB6790;
			C_GetTargetVID = 0x126E70; 
			C_ComboAttack = 0x127730; 
			C_ClickSkillSlot = 0x12F830; 
			C_Refresh = 0xB0450; 
			C_SetPixelPosition = 0x8D340;
			C_GetPixelPosition = 0x210D60;
			C_PickCloseItem = 0x126B70; 
			C_SendItemPickUpPacket = 0x110A10; 
			C_SendOnClickPacket = 0x103F00; 
			C_GlobalPositionToMapInfo = 0xA8AA0; 
			C_SendCommandPacket = 0x241D150; 
			C_SendRefinePacket = 0x108ED0; 
			C_SendShopSellPacketNew = 0x110380; 
			// Item Ground
			C_GetNames = 0x7E9F0; 
			C_CItemManager = 0x604B78; // (CPythonItem Yaz Arat Cıkan Herhangi biri ) 
			C_PythonItem = 0x604B58; // (CPythonPlayer::_OnPressItem String) 
			// Yniler
			C_ConnectTCP = 0xFA060; // 
			C_Disconnect = 0x1050E0; // 
		}
		//// Gameforge
		//if (server_selectors == 2)
		//{
		//	C_Entitylist = 0x20;

		//	m_kAliveInstMap = 0x20; // LocalPlayer : a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc 55 8b ec 83 ec ? 56

		//	C_LocalPlayer = 0x2DBCBA4; // EnableSkip : 81 c1 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec

		//	C_Localfarm = C_LocalPlayer + 0x60; // Yapıldı!!

		//	C_HorseAddr1 = 0x14; // Yapıldı!!
		//	C_HorseAddr2 = 0xC; // Yapıldı!!

		//	C_HorseCoordX = 0x4CC; // Yapıldı!!
		//	C_HorseCoordY = 0x4D0; // Yapıldı!!
		//	C_HorseCoordZ = 0x4D4; // Yapıldı!!

		//	C_LocalAddr = 0x14; // Yapıldı!! 

		//	C_Localfarm_addr = 0x5C; // Yapıldı
		//	C_AutoTarget = 0x60; // Yapıldı!!

		//	C_NoClip = 0x8E8; // Yapıldı!!
		//	C_AttackSpeed = 0x7F0; // Yapıldı!!
		//	C_MovementSpeed = 0x7EC; // Yapıldı!!
		//	C_SwordRange = 0x7E8; // Yapıldı!!
		//	C_QuickAge = 0x7E4; // Yapıldı!!
		//	C_BodyRecoil = 0x1BBC; // Bulunumu (Karakterimizin x degerinden - yaparak 14 saymak)
		//	C_AttackAnimationSpeed = 0x630; // + 11 gidersek bize Skill Rotation verir
		//	C_Horse = 0x528;
		//	C_OneHit = 0x738; // Yapıldı!!

		//	C_SkillRange = 0x630 + 0x3A; // Yapıldı!! : 89 87 ? ? ? ? b8 ? ? ? ? 8b 4e

		//	C_Name = 0x18; // Yapıldı!!
		//	C_Lvl = 0x54; // Yapıldı!!

		//	C_ModelCoordY = 0x2F8; // Yapıldı!!
		//	C_ModelCoordX = 0x2F4; // Yapıldı!! : 
		//	C_ModelCoordZ = 0x2FC; // Yapıldı!!

		//	C_VIDS = 0x8E0; // Yapıldı!!

		//	C_CoordsY = 0x7C0; // Yapıldı!!
		//	C_CoordsX = 0x7BC; // Yapıldı!!
		//	C_CoordsZ = 0x7C4; // Yapıldı!!

		//	C_IsDead = 0x788; // Yapıldı!!
		//	C_IsRotation = 0x82C; // Yapıldı!!
		//	C_IsSkillRotation = 0x65C; // Yapıldı!!
		//	C_IsCharRotation = 0x81C; // Yapıldı!!
		//  		C_IsRangeRotation = 0x358C; 
		//	C_IsRadarType = 0x73C; // Yapıldı!!
		//	C_IsCharacterType = 0x740; // Yapıldı!!

		//	// ınstance
		//	C_CPythonNetworkStream = 0x2C43094; // Yapıldı!! 8b 0d ? ? ? ? 89 4d ? 8b 55 ? 52 8b 45 ? 50 8b 4d ? e8 ? ? ? ? 8b 4d ? 8a 91
		//	C_CPythonApplication = 0x2C46BEC; // Yapıldı!! :: 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 5d c3 cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 0f b6 ? 51 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc 55 8b ec 83 ec
		//	C_CPythonBackground = 0x2C431EC; // Yapıldı!! :: a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 51 a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? e8 ? ? ? ? eb ? 8d 45
		//	C_CPythonPlayer = 0x2C432E8; // Yapıldı!! :: a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? a1 ? ? ? ? 89 45 ? 8b 4d ? 8a 91 ? ? ? ? 88 55 ? 0f b6 45 ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 c7 45
		//	C_ItemVTable = 0x58;
		//	C_Hearder = 0xDC;
		//	C_Mng = 0x4;

		//	// packed
		//	C_SetAttackKeyState = 0x257EF40; // C_SetAttackKeyState (cheat engine string kısmında) : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? ff 75
		//	C_SendItemUsePacket = 0x242C640; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc 55 8b ec 83 ec ? c6 45 ? ? b8 ? ? ? ? 66 89 45 ? c6 45 ? ? b9
		//	C_SendChatPacket = 0x241CE10; // YapıldI!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? a1 ? ? ? ? 89 45 ? 8b 4d ? 51 8b 4d ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? c7 45
		//	C_GetStatus = 0x2467090; // Yapıldı!! : e9 ? ? ? ? 90 90 0f b7 45
		//	C_SendGuildOfferPacket = 0x241EEA0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? a1 ? ? ? ? 89 45 ? 8b 4d ? 51 8b 4d ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? 8d 45
		//	C_MoveToDestPosition = 0x26F4300; // Yapıldı :: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 68 ? ? ? ? 6a ? 6a ? 68 ? ? ? ? 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 89 45 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 55 ? 52 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 45 ? 50 ff 15 ? ? ? ? 83 c4 ? 6a ? 68 ? ? ? ? 8b 4d ? 51 ff 15 ? ? ? ? 83 c4 ? 6a
		//	C_SendBattlePacket = 0x241C0D0; // Yapıldı!! : e8 ? ? ? ? 8b 4d ? 8a 91
		//	C_SendSpecial = 0x2424020; // Yapıldı :e8 ? ? ? ? 0f b6 ? 85 c0 75 ? 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? 32 c0 eb ? 8b 4d ? e8 ? ? ? ? 8b e5 5d cc
		//	C_SendCharacterStatePacket = 0x241CC70; // Yapıldı! :: 55 8b ec 83 ec ? a1 ? ? ? ? 33 c5 89 45 ? 89 4d ? c6 45 ? ? 8d 45 ? 50 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c9 75 ? 32 c0 e9 ? ? ? ? 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 d2 75
		//	C_SendAddFlyTargeting = 0x241DB70; // YapıldI!! :: e8 ? ? ? ? 6a ? 8b 4d ? e8 ? ? ? ? 8b 4d ? e8 ? ? ? ? 8b e5
		//	C_SendShootPacket = 0x2423E30; // Yapıldı!! :: e8 ? ? ? ? 8b e5 5d c2 ? ? cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 51 89 4d ? 8b e5
		//	C_SetCameraMaxDistance = 0x24F0470; // fix
		//	C_GetCurrentMapName = 0x2305930; // Yapıldı!! :: e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 51 a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 50 68 ? ? ? ? ff 15 ? ? ? ? 83 c4 ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? e8 ? ? ? ? eb ? 8d 45
		//	C_SetArmor = 0x22BE7A0; // YapıldI!! :: e8 ? ? ? ? 8b 4d ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55
		//	C_SetAffect = 0x12AB3B0; // Yok :D
		//	C_GetTargetVID = 0x247353A; // YapıldI!! :: 8b 80 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 89 4d ? 8b 45 ? 50
		//	C_ComboAttack = 0x131A4E0; // Yok!!
		//	C_ClickSkillSlot = 0x248ACA0; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc 55 8b ec 51 a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 50
		//	C_Refresh = 0x2312530; // Yapıldı!! :: e8 ? ? ? ? 89 45 ? 83 7d ? ? 75 ? e8 ? ? ? ? eb ? e8
		//	C_SetPixelPosition = 0x22CF220; // Yapıldı! :: e8 ? ? ? ? e8 ? ? ? ? 8b 4d ? 33 cd e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc 55
		//	C_GetPixelPosition = 0x22CF140; // Yapıldı!! : 55 8b ec 51 89 4d ? 8b 4d ? 81 c1 ? ? ? ? e8 ? ? ? ? 8b 4d ? 8b 10
		//	C_PickCloseItem = 0x2387920; // Yok!!
		//	C_SendItemPickUpPacket = 0x242C5C0; // Yapıldı!! : 55 8b ec 83 ec ? 89 4d ? c6 45 ? ? 8d 45 ? 50 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c9 75 ? 32 c0 eb ? 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 d2 75 ? b0 ? eb ? c6 45 ? ? 8b 45
		//	C_SendOnClickPacket = 0x2420EF0; // Yapıldı! :: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 6a
		//	C_GlobalPositionToMapInfo = 0x23059A0; // Yapıldı!!: e8 ? ? ? ? 89 45 ? 83 7d ? ? 74 ? 8b 4d ? e8 ? ? ? ? 89 45 ? 8b 45
		//	C_SendCommandPacket = 0x241D150; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? e9 ? ? ? ? c6 45
		//	C_SendRefinePacket = 0xAF400; // Yapıldı!! : e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc 55 8b ec 51 8d 45 ? 50 6a ? ff 75 ? e8 ? ? ? ? 83 c4 ? 84 c0 75 ? 6a ? e8 ? ? ? ? 83 c4 ? 8b e5 5d c3 ff 75 ? 8b 0d ? ? ? ? e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc 8b 0d
		//	// Item Ground
		//	C_GetNames = 0x22BB700; // YapıldI!! : e8 ? ? ? ? 50 68 ? ? ? ? e8 ? ? ? ? 83 c4 ? eb ? 8b 55
		//	C_CItemManager = 0x2C432F0; // Yapıldı!! : 8b 0d ? ? ? ? 89 4d ? 6a ? 8d 55 ? 52 8b 45 ? 50 8b 4d ? e8 ? ? ? ? 0f b6 ? 85 c9 74 ? 6a ? 8b 4d ? e8 ? ? ? ? 89 45 ? 8b 4d
		//	C_PythonItem = 0x2C46BCC; // Yapıldı!! : a1 ? ? ? ? 89 45 ? 8b 4d ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc 55 8b ec 83 ec ? 89 4d ? a1 ? ? ? ? 89 45 ? 8b 4d ? 8b 51
		//	// Yniler
		//	C_ConnectTCP = 0x23ED940; //  Yapıldı!! :: e8 ? ? ? ? e8 ? ? ? ? 8b e5 5d c3 cc cc cc cc cc cc cc cc cc cc cc cc cc cc cc 55 8b ec 83 ec ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? e9 ? ? ? ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? 8d 45 ? 50 6a ? 8b 4d ? 51 e8 ? ? ? ? 83 c4 ? 0f b6 ? 85 d2 75 ? 6a ? e8 ? ? ? ? 83 c4 ? eb ? 8d 45
		//	C_Disconnect = 0x23EE310; // Yapıldı!! :: e8 ? ? ? ? 8b 4d ? e8 ? ? ? ? e8 ? ? ? ? e8
		//}
	}
};


DWORD metin2client::C_Entitylist = 0;
DWORD metin2client::m_kAliveInstMap = 0;
DWORD metin2client::C_LocalPlayer = 0;
DWORD metin2client::C_Localfarm = 0;

DWORD metin2client::C_Spoffer = 0;
DWORD metin2client::C_Spoffer_Code = 0;

DWORD metin2client::C_HorseAddr1 = 0;
DWORD metin2client::C_HorseAddr2 = 0;
DWORD metin2client::C_HorseCoordX = 0;
DWORD metin2client::C_HorseCoordY = 0;
DWORD metin2client::C_HorseCoordZ = 0;

DWORD metin2client::C_LocalAddr = 0;

DWORD metin2client::C_Localfarm_addr = 0;
DWORD metin2client::C_AutoTarget = 0;

DWORD metin2client::C_NoClip = 0;
DWORD metin2client::C_AttackSpeed = 0;
DWORD metin2client::C_MovementSpeed = 0;
DWORD metin2client::C_SwordRange = 0;
DWORD metin2client::C_QuickAge = 0;
DWORD metin2client::C_BodyRecoil = 0;
DWORD metin2client::C_OneHit = 0;
DWORD metin2client::C_AttackAnimation = 0;
DWORD metin2client::C_AttackAnimationSpeed = 0;
DWORD metin2client::C_Horse = 0;

DWORD metin2client::C_SkillRange = 0;

DWORD metin2client::C_Name = 0;
DWORD metin2client::C_Lvl = 0;

DWORD metin2client::C_ModelCoordY = 0;
DWORD metin2client::C_ModelCoordX = 0;
DWORD metin2client::C_ModelCoordZ = 0;

DWORD metin2client::C_ModelVectorX = 0;
DWORD metin2client::C_ModelVectorY = 0;
DWORD metin2client::C_ModelVectorZ = 0;

DWORD metin2client::C_VIDS = 0;
DWORD metin2client::C_CoordsY = 0;
DWORD metin2client::C_CoordsX = 0;
DWORD metin2client::C_CoordsZ = 0;
DWORD metin2client::C_IsDead = 0;
DWORD metin2client::C_IsRotation = 0;
DWORD metin2client::C_IsRadarType = 0;
DWORD metin2client::C_IsSkillRotation = 0;
DWORD metin2client::C_IsCharRotation = 0;
DWORD metin2client::C_IsRangeRotation = 0;
DWORD metin2client::C_IsCharacterType = 0;

DWORD metin2client::C_IsKnockback = 0;
DWORD metin2client::C_IsAllKnockBack = 0;

// ınstance
DWORD metin2client::C_CPythonNetworkStream = 0;
DWORD metin2client::C_CPythonApplication = 0;
DWORD metin2client::C_CPythonBackground = 0;
DWORD metin2client::C_CPythonPlayer = 0;
DWORD metin2client::C_ItemVTable = 0;
DWORD metin2client::C_Hearder = 0;
DWORD metin2client::C_Mng = 0;

DWORD metin2client::C_SetAttackKeyState = 0;
DWORD metin2client::C_Refresh = 0;
DWORD metin2client::C_SendItemUsePacket = 0;
DWORD metin2client::C_SendChatPacket = 0;
DWORD metin2client::C_SendWhisperPacket = 0;
DWORD metin2client::C_GetStatus = 0;
DWORD metin2client::C_SendGuildOfferPacket = 0;
DWORD metin2client::C_MoveToDestPosition = 0;
DWORD metin2client::C_SendBattlePacket = 0;
DWORD metin2client::C_SendSpecial = 0;
DWORD metin2client::C_SendCharacterStatePacket = 0;
DWORD metin2client::C_SendAddFlyTargeting = 0;
DWORD metin2client::C_SendShootPacket = 0;
DWORD metin2client::C_SetCameraMaxDistance = 0;
DWORD metin2client::C_GetCurrentMapName;
DWORD metin2client::C_SetArmor = 0;
DWORD metin2client::C_SetAffect = 0;
DWORD metin2client::C_GetTargetVID = 0;
DWORD metin2client::C_GetPixelPosition = 0;
DWORD metin2client::C_SendItemPickUpPacket = 0;
DWORD metin2client::C_GlobalPositionToMapInfo = 0;
DWORD metin2client::C_SendCommandPacket = 0;
// Yeniler

DWORD metin2client::C_ConnectTCP = 0;
DWORD metin2client::C_Disconnect = 0;

// Yeniler

DWORD metin2client::C_PickCloseItem = 0;
DWORD metin2client::C_ComboAttack = 0;
DWORD metin2client::C_SetPixelPosition = 0;
DWORD metin2client::C_ClickSkillSlot = 0;
DWORD metin2client::C_SendShopSellPacketNew = 0;
// Demirci
DWORD metin2client::C_SendRefinePacket = 0;

// Mining
DWORD metin2client::C_SendOnClickPacket = 0;

// ItemGround
DWORD metin2client::C_GetNames = 0;
DWORD metin2client::C_PythonItem = 0;
DWORD metin2client::C_CItemManager = 0;

DWORD metin2client::iCItemManagerInstance = NULL;
DWORD metin2client::pCItemManagerInstance = NULL;
DWORD metin2client::pCPythonItemInstance = NULL;
DWORD metin2client::iCPythonItemInstance = NULL;


struct AutoKey
{
	DWORD KeyBoard_AutoAttack = 0x30290;
	DWORD KeyBoard_Slot1 = 0x3025A;
	DWORD KeyBoard_Slot2 = 0x3025B;
	DWORD KeyBoard_Slot3 = 0x3025C;
	DWORD KeyBoard_Slot4 = 0x3025D;

	DWORD KeyBoard_BackgroundKey = 0x30234; 

	DWORD KeyBoard_Enter = 0x30274; 

	DWORD KeyBoard_F1 = 0x30293;
	DWORD KeyBoard_F2 = 0x30294;
	DWORD KeyBoard_F3 = 0x30295;
	DWORD KeyBoard_F4 = 0x30296;
	__forceinline AutoKey* operator->() { return this; }
};
struct Clients
{
	AutoKey Key;
	__forceinline Clients* operator->() { return this; }
};
inline Clients Client;

static std::string SelectGame = "Select";
namespace Keyboard
{
	// * ================== Genel ====================== * //
	// Yapıldı!!
	static bool AutoAttacks = false;
	static bool AutoAttack_Window = false;

	static bool C_AutoSpaceV1 = false;
	static bool C_AutoSpaceV2 = false;
	static bool C_AutoSpaceV3 = false;
	static bool C_AutoRotation = false;

	// * ================== Mobber ====================== * //
	// Yapıldı!!
	static bool Mobber = false;
	static bool Mobber_Window = false;

	static bool C_Slot1 = false;
	static bool C_Slot2 = false;
	static bool C_Slot3 = false;
	static bool C_Slot4 = false;

	static bool C_F1 = false;
	static bool C_F2 = false;
	static bool C_F3 = false;
	static bool C_F4 = false;

	// * ================== Pro Damage ====================== * //
	static bool ProDamage = false;
	static bool ProDamage_Window = false;

	static bool C_ProdDamage = false;


	// * ================== Wait ====================== * //
	// Yapıldı!!
	static bool WaitHack = false;
	static bool WaitHack_Window = false;

	static bool C_RangeWait = false;
	static bool C_RangeWaitLine = false;

	static bool C_WaitDamage = false;

	static bool C_DumyDamage = false;

	static bool C_DieTimeEnable = false;
	static int C_DieTime = 9000;

	int Wait_GuiSizeX = 230;
	int Wait_GuiSizeY = 85;


	static bool C_WaitMonster = false;
	static bool C_WaitBoss = false;
	static bool C_WaitStone = false;
	static bool C_WaitOther = false;

	static bool C_WaitNinja = false;

	static int C_WaitSpeed = 500;
	static float C_WaitDistance = 300.0f;

	// * ================== Target ====================== * //

	static bool AttackOnTarget = false;
	static bool AttackOnTarget_Window = false;

	static bool C_RangeTarget = false;

	// * ================== pot ====================== * //
	// Yapıldı!!
	static bool AutoPot = false;
	static bool AutoPot_Window = false;

	static bool C_PotSlot9 = false;
	static bool C_PotSlot8 = false;
	static bool C_PotSlot7 = false;
	static bool C_PotSlot6 = false;
	static bool C_PotSlot5 = false;
	static bool C_PotSlot4 = false;
	static bool C_PotSlot3 = false;
	static bool C_PotSlot2 = false;

	// * ================== pickup ====================== * //
	// Yapıldı!!
	static bool AutoPickUp = false;
	static bool AutoPickUp_Window = false;

	static bool C_AutoLootV1 = false;
	static bool C_AutoLootV2 = false;
	static bool C_AutoLootv3 = false;
	static bool C_RangeLoot = false;
	static bool C_CollectLoot = false;



	int Pickup_GuiSizeX = 230;
	int Pickup_GuiSizeY = 85;

	static bool C_Blacksmith = false;
	static int C_BlacksmithSpeed = 1200;

	static bool C_Blacksmith_CheckAll = false;

	static bool C_Blacksmith_Slot1 = false;
	static bool C_Blacksmith_Slot2 = false;
	static bool C_Blacksmith_Slot3 = false;
	static bool C_Blacksmith_Slot4 = false;
	static bool C_Blacksmith_Slot5 = false;
	static bool C_Blacksmith_Slot6 = false;
	static bool C_Blacksmith_Slot7 = false;
	static bool C_Blacksmith_Slot8 = false;
	static bool C_Blacksmith_Slot9 = false;
	static bool C_Blacksmith_Slot10 = false;
	static bool C_Blacksmith_Slot11 = false;
	static bool C_Blacksmith_Slot12 = false;
	static bool C_Blacksmith_Slot13 = false;
	static bool C_Blacksmith_Slot14 = false;
	static bool C_Blacksmith_Slot15 = false;
	static bool C_Blacksmith_Slot16 = false;
	static bool C_Blacksmith_Slot17 = false;
	static bool C_Blacksmith_Slot18 = false;
	static bool C_Blacksmith_Slot19 = false;
	static bool C_Blacksmith_Slot20 = false;

	// Inventory

	static int C_SellSloot = 1;

	// * ================== KnockBack ====================== * //

	static bool KnockBack = false;
	static bool KnockBack_Window = false;

	static bool C_KnockBack_Entity = false;
	static bool C_KnockBack_Player = false;
	static bool C_KnockBack_Stone = false;

	// * ================== Collision ====================== * //

	// yapıldı!!
	static bool Collision = false;
	static bool Collision_Window = false;

	static bool C_Collision_NoClip = false;
	static bool C_Collision_NPC = false;
	static bool C_Collision_Player = false;
	static bool C_Collision_Stone = false;
	static bool C_Collision_Portal = false;
	

	// * ================== Boost ====================== * //
	// Yapıldı!!
	static bool Boost = false;
	static bool Boost_Window = false;

	static bool C_BoostV1 = false;
	static bool C_BoostV2 = false;

	static int BoostSpeed = 15;
	static int BoostStep = 40;

	// * ================== Name ====================== * //
	// Yapıldı!!
	static bool Name = false;
	static bool Name_Window = false;

	static bool C_NameChanger = false;
	static bool C_NameApathe = false;
	static bool C_Lalaker1 = false;

	// Whisper

	static bool C_WhisperSpawm = false;
	static bool C_WhisperEveryoneSpawm = false;
	static bool C_WhisperArea = false;
	static float C_WhisperDistance = 1000;
	static int C_WhisperSpeed = 900;

	int Name_GuiSizeX = 230;
	int Name_GuiSizeY = 85;

	// * ================== Sonra Yapılacak! ====================== * //

	static bool AutoCH = false;
	static bool AutoCH_Window = false;

	static bool AutoCH_Order = false;
	static bool AutoCH_Random = false;
	static int AutoCH_Time = 120000;

	int AutoCH_GuiSizeX = 230;
	int AutoCH_GuiSizeY = 85;

	static bool Auto_CH1 = false;
	static bool Auto_CH2 = false;
	static bool Auto_CH3 = false;
	static bool Auto_CH4 = false;
	static bool Auto_CH5 = false;
	static bool Auto_CH6 = false;

	static bool AutoLogin = false;
	static bool AutoLogin_Window = false;

	// * ================== Farm Özellikleri ====================== * //
	// Bitti
	static bool Bot = false;
	static bool Bot_Winddow;

	int Bot_GuiSizeX = 230;
	int Bot_GuiSizeY = 85;

	static int BOT_SPEED = 40;
	static int BOT_DISTANCE = 1000;

	static bool Bot_Npc = false;
	static bool Bot_Stone = false;
	static bool Bot_Mineral = false;

	static bool Bot_Horse = false;
	static bool Bot_Player = false;
	
	static bool C_NoKs = false;

	static bool C_WalkToFarm = false;
	static bool C_UltimateFarm = false;

	static bool C_UltimateRoute = false;

	static int C_UltimateSpeed = 50;

	static bool C_LegitRange = false;
	static bool C_BotTeleport = false;
	static bool C_StateRange = false;
	static bool C_KillAura = false;

	static bool StoneBan = false;
	static bool StoneClear = false;

	// Rohan2
	static bool C_AllVadiBan = false;
	static bool C_AllSohanBan = false;
	static bool C_AllOrman = false;

	// * ================== Skills ====================== * //
	// Yapıldı!!
	static bool Skills = false;
	static bool Skills_Window = false;

	static int HorseTime = 100;
	static int DefaultTime = 100;

	static bool C_HorseSkill = false;
	static bool C_HorseSkillSlot1 = false;
	static bool C_HorseSkillSlot2 = false;
	static bool C_HorseSkillSlot3 = false;
	static bool C_HorseSkillSlot4 = false;

	static bool C_DefaultSkill = false;
	static bool C_DefaultSkillSlot1 = false;
	static bool C_DefaultSkillSlot2 = false;
	static bool C_DefaultSkillSlot3 = false;
	static bool C_DefaultSkillSlot4 = false;

	// * ================== AttackSpeed ====================== * //
	// Yapıldı!!
	static bool AttackSpeed = false;
	static bool AttackSpeed_Window = false;

	static bool C_SwordRange = false;

	static float C_AttackSpeedValue = 1.7;
	static bool C_AttackSpeed = false;


	// * ================== AttackSpeed ====================== * //
	// Yapıldı!!
	static bool MovementSpeed = false;
	static bool MovementSpeed_Window = false;

	static bool C_LegitSpeed = false;
	static bool C_ModelVector = false;

	static float C_MovementSpeedValue = 1.7;
	static bool C_MovementSpeed = false;

	// * ================== AttackSpeed ====================== * //
	static bool AntiFly = false;
	static bool AntiFly_Window = false;

	static bool AntiFly_Player = false;
	static bool AntiFly_Stone = false;
	static bool AntiFly_Npc = false;


	// * ================== AttackSpeed ====================== * //
	static bool AutoRevive = false;
	static bool AutoRevive_Window = false;
	
	static int AutoRevive_Time = 9000;

	static bool AutoReviveGF = false;

	// * ================== HorseChanger ====================== * //
	static bool HorseChanger = false;
	static bool HorseChanger_Window = false;

	static bool HorseChanger_Ninja = false;
	static bool HorseChanger_Sura = false;
	static bool HorseChanger_War = false;
	static bool HorseChanger_Shaman = false;

	// * ================== ChatSpawmmer ====================== * //
	static bool ChatSpawmmer = false;
	static bool ChatSpawmmer_Window = false;

	static bool ChatSpawmmer_Rainbow = false;

	static bool ChatSpawmmer_Green = false;
	static bool ChatSpawmmer_Yellow = false;
	static bool ChatSpawmmer_Ping = false;
	static bool ChatSpawmmer_Blue = false;
	static bool ChatSpawmmer_DarkBlue = false;

	// * ================== XP Donate ====================== * //
	static bool XPDonate = false;
	static bool XPDonate_Window = false;

	static bool C_XPDonate_Lonca = false;
	static int XPDonate_Limit = 100;
	static int XPDonate_Speed = 800;

	static bool XPDonate_Level = false;
	static int XPDonate_lvl = 50;


	// * ================== Refresh ====================== * //
	static bool InstanceRefresh = false;
	static bool  InstanceRefresh_Window = false;

	static bool InstRefresh = false;

	// * ================== Zoom ====================== * //
	static bool CZoom = false;
	static bool CZoom_Window = false;

	static bool C_Zoom = false;
	static int C_ZoomValue = 2500;

	// * ================== Effect ====================== * //
	static bool EffectChanger = false;
	static bool EffectChanger_Window = false;

	static bool C_EffectGM = false;


	// * ================== Dedectors ====================== * //
	static bool GMDedect = false;
	static bool GMDedect_Window = false;

	static bool C_GM_Dedect = false;

	static bool C_GM_Dedect_Sound = false;

	static bool C_GM_Dedect_GameClose = false;

	static int C_GM_Dedect_Distance = 1000;

	// * ================== Stone ====================== * //
	static bool StoneDedect = false;
	static bool StoneDedect_Window = false;

	static bool C_StoneDedect = false;
	static int C_StoneDedect_Distance = 1000;

	static bool C_StoneBeep = false;
	static bool C_StoneTeleport = false;

	static bool C_StoneDrawLine = false;
	static bool C_Stone3DBox = false;

	int Stone_GuiSizeX = 230;
	int Stone_GuiSizeY = 85;


	// * ================== Player ====================== * //
	static bool PlayerDedect = false;
	static bool PlayerDedect_Window = false;

	static bool C_Player_Dedect = false;

	static bool C_Player_Dedect_Sound = false;

	static bool C_Player_Dedect_GameClose = false;

	static int C_Player_Dedect_Distance = 1000;

	// * ================== Mining ====================== * //
	static bool MiningDedect = false;
	static bool MiningDedect_Window = false;


	int Mining_GuiSizeX = 230;
	int Mining_GuiSizeY = 85;

	static bool C_MiningTeleport = false;
	static bool C_MiningAuto = false;

	static bool C_MiningLimitörs = false;

	static bool C_MiningBeep = false;
	static bool C_AutoMining = false;

	static bool C_MiningShowBallon = false;

	static int C_Mining_Distance = 100.0f;

	// * ================== Portal ====================== * //
	static bool PortalDedect = false;
	static bool PortalDedect_Window = false;

	static bool C_Portal = false;

	static bool C_Portal_Sound = false;

	static bool C_Portal_GameClose = false;

	static int C_Portal_Distance = 1000;


	// * ================== Teleport ====================== * //
	static bool TeleportDedect = false;
	static bool TeleportDedect_Window = false;

	static bool C_Teleportss = false;

	static bool C_Teleportss_Sound = false;

	static bool C_Teleportss_GameClose = false;

	static int C_Teleportss_Distance = 1000;

	// * ================== Area ====================== * //
	static bool AreaDedect = false;
	static bool AreaDedect_Window = false;

	static bool C_AreaLine = false;

	static int C_AreaDist = 1000;
	static bool Area_Bep = true;

	// * ================== PM ====================== * //
	static bool PMDedect = false;
	static bool PMDedect_Window = false;

	// * ================== SPAWM ====================== * //
	static bool SpawmDedect = false;
	static bool SpawmDedect_Window = false;

	static bool C_PlayerSpawm = false;

	static bool C_PlayerSpawm_Sound = false;
	static bool C_PlayerSpawm_GameClose = false;

	static int C_PlayerSpawm_Distance = 1000;

	// * ================== Auto Dungeon- helper ====================== * //
	static bool AutoDungeon = false;
	static bool AutoDungeon_Window = false;

	// * ================== Auto Dungeon- helper ====================== * //
	static bool WSCheat = false;
	static bool WSCheat_Window = false;

	static bool C_SkillRange = false;
	static bool C_Liberated = false;
	static bool C_StrongBody = false;
	static bool C_AutoTarget = false;
	static bool C_Typer = false;
	static bool C_Hitting = false;

	int WS_GuiSizeX = 230;
	int WS_GuiSizeY = 230;


	static bool C_ClickToTeleport = false;
	static bool C_EveryoneCheck = false;
	static bool C_CheckModel = false;
	static bool C_SendModel = false;
	

	static float C_ClickToTeleport_Distance = 2000;

	// * ================== Loader ====================== * //
	static bool Loader = false;
	static bool Loader_Window = false;

	// * ================== Fish Bot ====================== * //
	static bool Fish = false;
	static bool Fish_Window = false;

	// * ================== Minimap ====================== * //
	static bool Minimap = false;
	static bool Minimap_Window = false;

	static bool C_ShowMap = true;

	static bool C_ShowNPC = false;
	static bool C_ShowStone = false;

	static bool C_ShowEntity = false;
	static bool C_ShowBoss = false;

	// * ================== Route ====================== * //
	static bool Route = false;
	static bool Route_Window = false;

	static bool C_RouteRecording = false;
	static bool C_PlayRoute = false;
	static bool C_ClearRoute = false;
	static bool C_RouteSave = false;

	// * ================== Buff Yeni ====================== * //

	static bool BuffBot = false;
	static bool Buff_Window = false;

	int C_BuffWindowSizeX = 295;

	static bool C_BuffTarget = false;
	static bool C_BuffNoClip = false;
	static bool C_BuffVector = false;

	static bool C_SkillSlotF1 = false;
	static bool C_SkillSlotF2 = false;
	static bool C_SkillSlotF3 = false;
	static bool C_SkillSlotF4 = false;
	static bool C_BuffAutoRevive = false;

	// buff farm
	static bool C_BuffWalkToCut = false;
	static bool C_BuffTeleport = false;
	static bool C_BuffLegit = false;
	static bool C_BuffWalkToStone = false;
	static bool C_BuffWalkToEntity = false;

	static bool C_BuffTypeHorse = false;
	static bool C_BuffTypePlayer = false;

	static bool C_BuffProDamage = false;
	static bool C_BuffWaitDamage = false;
	static bool C_BuffDmgStone = false;
	static bool C_BuffDmgEntity = false;
	static bool C_BuffDmgOther = false;
}


