#include <unordered_map>
#include "main.h" 
#include "imgui\imgui.h"
#include "imgui\imgui_impl_dx9.h"
#include "imgui\imgui_impl_win32.h"

//detours
#include "detours.h"
#if defined _M_X64
#pragma comment(lib, "detours.X64/detours.lib")
#elif defined _M_IX86
#pragma comment(lib, "detours.X86/detours.lib")
#endif

#if defined(PYTHON_ENABLE)
#ifdef _DEBUG
#undef _DEBUG
#include <Python.h>
#pragma comment(lib,"python27.lib")
#define _DEBUG
#else
#include <Python.h>
#pragma comment(lib,"python27.lib")
#endif
#else
typedef unsigned long       PyObject;
typedef unsigned long       PyMethodDef;
typedef int                 Py_ssize_t;
#endif

#include "Hook.h"
#include "Gui.h"
#include "SoundEng.h"
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "d3dx9.lib")

#include <windows.h>
#include <map>
#include <d3d9.h>
#include <d3dx9.h>
#include "imgui/imgui_internal.h"
#include "Define.h"
#include "AutoUpdate.h"
#include "Keyboard.h"
#include "Cfg.h"
#include "XOr.h"
#include "use.h"
#include "Python.h"
#include "PythonExtension.h"
#include "RGB.h"
#include "Matrix.h"
#include "Web.h"
#include "AntiCheat.h"
#include "AntiDebug.h"
#include "Multiple.h"
#include "Degsın.h"
#include "Login.h"
#include "Radar.h"
#include "BuffSysteam.h"
#include "Shread.h"
#include "Memory.h"
#include "CheatBlocker.h"
#include "CDevice.h"
#include "CRender.h"

HRESULT APIENTRY DrawIndexedPrimitive_hook(IDirect3DDevice9* pDevice, D3DPRIMITIVETYPE Type, INT BaseVertexIndex, UINT MinVertexIndex, UINT NumVertices, UINT startIndex, UINT primCount)
{
	///*if (!Device::pDevice)
	//	Device::pDevice = pDevice;*/
	//if (Keyboard::Name)
	//{
	//	if(Keyboard::C_WhisperArea)
	//	{
	//		Draw::rangewait(pDevice, c_players.x, c_players.y, c_players.z, ImColor(250, 0, 112), Keyboard::C_WhisperDistance);
	//	}
	//}
	//if (Keyboard::WaitHack)
	//{
	//	if (Keyboard::C_RangeWaitLine)
	//	{
	//		//Draw::rangewait(pDevice, c_players.x, c_players.y, c_players.z, ImColor(255, 5, 130), Keyboard::C_WaitSpeed);
	//		//CRender::FilledCircle3D(c_players.x,c_players.y,c_players.z,500.0f,60,D3DCOLOR_RGBA(255, 0, 0, 150));
	//	}
	//}

	//if (Keyboard::AreaDedect)
	//{
	//	if (Keyboard::C_AreaLine)
	//	{
	//		Draw::rangewait(pDevice, c_players.x, c_players.y, c_players.z, ImColor(5, 229, 250), Keyboard::C_AreaDist);
	//	}
	//}

	//if (Keyboard::StoneDedect)
	//{
	//	if (Keyboard::C_StoneDrawLine)
	//	{
	//		/*D3DXVECTOR3 Coord(c_players.x, c_players.y, c_players.z);
	//		ModelInfo.clear();

	//		for (auto& ent : GetEntities())
	//		{
	//			if (ent.x == 0.0f && ent.y == 0.0f && ent.z == 0.0f)
	//				continue;

	//			if (ent.Type != 2)
	//				continue;

	//			float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
	//			if (distance >= Keyboard::C_StoneDedect_Distance)
	//				continue;

	//			ModelInfo_t info;
	//			info.Coord2D = D3DXVECTOR3(ent.x, ent.y, 0.0f);
	//			info.Coord3D = D3DXVECTOR3(ent.x, ent.y, ent.z);
	//			info.Offset = 0;
	//			info.TEAM = 0;
	//			info.Index = 0;
	//			info.CrosshairDistance = distance;

	//			ModelInfo.push_back(info);
	//		}
	//		for (auto& model : ModelInfo)
	//		{
	//			Draw::Cube3D(pDevice, Coord, model.Coord3D, ImColor(9, 224, 142));

	//			if (Keyboard::C_Stone3DBox)
	//				Draw::Box3D(pDevice, model.Coord3D, D3DCOLOR_ARGB(255, 255, 0, 0));
	//		}*/
	//		for (auto& ent : GetEntities())
	//		{
	//			D3DXVECTOR3 Entity = { ent.x,ent.y,ent.z };
	//			if (Entity.x == 0.0f && Entity.y == 0.0f && Entity.z == 0.0f)
	//				continue;

	//			if (ent.Type != 2)
	//				continue;

	//			float distance = Dist(Entity.x, Entity.y, c_players.x, c_players.y);
	//			if (distance >= Keyboard::C_StoneDedect_Distance)
	//				continue;

	//			WorldToScreenV2(pDevice, &Entity);
	//		}
	//	}
	//	if (Keyboard::C_Stone3DBox)
	//	{
	//		D3DXVECTOR3 Coord(c_players.x, c_players.y, c_players.z);

	//		for (auto& ent : GetEntities())
	//		{
	//			if (ent.Type == 2)
	//			{
	//				float distance = Dist(ent.x, ent.y, c_players.x, c_players.y);
	//				if (distance >= Keyboard::C_StoneDedect_Distance) continue;

	//				D3DXVECTOR3 enemyPos(ent.x, ent.y, ent.z);
	//				Draw::Box3D(pDevice, enemyPos, D3DCOLOR_ARGB(255, 255, 0, 0),/*Hacker*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0F\x29\x2A\x21\x2E\x3E" + 0x2CAE8000).s);
	//			}
	//		}
	//		ModelInfo.clear();
	//	}
	//}

	return DrawIndexedPrimitive_orig(pDevice, Type, BaseVertexIndex, MinVertexIndex, NumVertices, startIndex, primCount);
}


namespace render_ui
{
	void Buttons()
	{
		static bool General = true;
		static bool Automaticly = false;
		static bool Dedectors = false;
		static bool Helpers = false;
		static bool Settings = false;

		// General
		if (UI::OuterButtonToolTip(ImVec2(17.30f, 66), ImVec2(36, 25), General, Low,  /*General*/XorStrTR<0x47, 8, 0x2CAE8000>("\x00\x2D\x27\x2F\x39\x2D\x21" + 0x2CAE8000).s))
		{
			General = true;
			Automaticly = false;
			Dedectors = false;
			Helpers = false;
			Settings = false;
			ModeSelect = 0; // General seçili
		}
		ImGui::SetCursorPos(ImVec2(27, 70));
		ImGui::Image((void*)Sword, ImVec2(17, 17));

		// Automaticly
		if (UI::OuterButtonToolTip(ImVec2(17.30f, 94), ImVec2(36, 25), Automaticly, Low, /*Automaticly*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3D\x3D\x25\x26\x2D\x39\x27\x2C\x3C\x28" + 0x2CAE8000).s))
		{
			Automaticly = true;
			General = false;
			Dedectors = false;
			Helpers = false;
			Settings = false;
			ModeSelect = 1; // Automaticly seçili
		}
		ImGui::SetCursorPos(ImVec2(24, 96.50));
		ImGui::Image((void*)AutoFarm, ImVec2(20, 20));

		// Dedectors
		if (UI::OuterButtonToolTip(ImVec2(17.30f, 122), ImVec2(36, 25), Dedectors, Low, /*Dedectors*/XorStrTR<0x47, 10, 0x2CAE8000>("\x03\x2D\x2D\x2F\x28\x38\x22\x3C\x3C" + 0x2CAE8000).s))
		{
			Dedectors = true;
			General = false;
			Automaticly = false;
			Helpers = false;
			Settings = false;
			ModeSelect = 2; // Dedectors seçili
		}
		ImGui::SetCursorPos(ImVec2(25, 124));
		ImGui::Image((void*)Online, ImVec2(20, 20));

		// Helper
		if (UI::OuterButtonToolTip(ImVec2(17.30f, 150), ImVec2(36, 25), Helpers, Low, /*Helper*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0F\x2D\x25\x3A\x2E\x3E" + 0x2CAE8000).s))
		{
			Helpers = true;
			General = false;
			Automaticly = false;
			Dedectors = false;
			Settings = false;
			ModeSelect = 3; // Helpers seçili
		}
		ImGui::SetCursorPos(ImVec2(26.10, 154));
		ImGui::Image((void*)Helper, ImVec2(16, 16));

		// Settings
		if (UI::OuterButtonToolTip(ImVec2(17.30f, 178), ImVec2(36, 25), Settings, Low, /*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3C" + 0x2CAE8000).s))
		{
			Settings = true;
			General = false;
			Automaticly = false;
			Dedectors = false;
			Helpers = false;
			ModeSelect = 4; // Settings seçili
		}
		ImGui::SetCursorPos(ImVec2(26.10, 182));
		ImGui::Image((void*)Setting, ImVec2(16, 16));
	}

	void CH_Render()
	{
		// ch changer
		static bool CH1 = false;
		if (UI::NumberButton("1", ImVec2(250, 70-5), ImVec2(19, 18), CH1, Blood, /*CH 1*/XorStrTR<0x47, 7, 0x2CAE8000>("\x04\x00\x69\x62\x7A\x65" + 0x2CAE8000).s))
		{
			CH1 = !CH1;
		}

		static bool CH2 = false;
		if (UI::NumberButton("2", ImVec2(250, 96 - 7), ImVec2(19, 18), CH2, Blood, /*CH (2)*/XorStrTR<0x47, 7, 0x2CAE8000>("\x04\x00\x69\x62\x79\x65" + 0x2CAE8000).s))
		{
			CH2 = !CH2;
		}

		static bool CH3 = false;
		if (UI::NumberButton("3", ImVec2(250, 96 + 26 - 10), ImVec2(19, 18), CH3, Blood, /*CH (3)*/XorStrTR<0x47, 7, 0x2CAE8000>("\x04\x00\x69\x62\x78\x65" + 0x2CAE8000).s))
		{
			CH3 = !CH3;
		}

		static bool CH4 = false;
		if (UI::NumberButton("4", ImVec2(250, 96 + 26 + 26 - 13), ImVec2(19, 18), CH4, Blood, /*CH (4)*/XorStrTR<0x47, 7, 0x2CAE8000>("\x04\x00\x69\x62\x7F\x65" + 0x2CAE8000).s))
		{
			CH4 = !CH4;
		}
	}

	void GameStatus(bool inGame)
	{
		ImVec2 iconPos(238, 25); 
		ImVec2 iconSize(24, 24);  

		ImGui::SetCursorPos(iconPos);

		if (inGame)
			ImGui::Image((void*)HLOpen, iconSize);
		else
			ImGui::Image((void*)HLClose, iconSize);
	}

}

namespace CheatRender
{
	void Cheat_Render()
	{
		ImVec2 childPos(66, 64);
		ImVec2 childSize(178, 168);
		ImVec2 SettingSize(187, 168);

		ImGui::SetCursorPos(childPos);

		// Low fontunu kullan
		ImGui::PushFont(Low);

		ImGui::PushStyleColor(ImGuiCol_ChildBg, ImVec4(0.0f, 0.0f, 0.0f, 0.0f)); // transparent

		//ImGui::PushStyleColor(ImGuiCol_ChildBg, ImVec4(1.0f, 1.0f, 1.0f, 0.5f)); 


		switch (ModeSelect)
		{
		case 0:
			if (ImGui::BeginChild(/*ChildPanel_General*/XorStrTR<0x47, 19, 0x2CAE8000>("\x04\x20\x20\x26\x2F\x1C\x2C\x20\x2A\x3C\x0E\x15\x36\x3A\x30\x24\x36\x34" + 0x2CAE8000).s, childSize, true, ImGuiWindowFlags_NoScrollbar))
			{
				// Sol ilk 2
				if (UI::SexyButton(/*Auto Attack*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x0D\x39\x3A\x2E\x33\x3A" + 0x2CAE8000).s, ImVec2(3.90, 4), ImVec2(80, 20), Keyboard::AutoAttacks, Low, &Keyboard::AutoAttack_Window))
				{
					Keyboard::AutoAttacks = !Keyboard::AutoAttacks;
				}
				// Sağ 2
				if (UI::SexyButton(/*Mobber*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0A\x27\x2B\x28\x2E\x3E" + 0x2CAE8000).s, ImVec2(93.0f, 4), ImVec2(80, 20), Keyboard::Mobber, Low, &Keyboard::Mobber_Window))
				{
					Keyboard::Mobber = !Keyboard::Mobber;
				}
				// Sol 3
				if (UI::SexyButton(/*Sniping*/XorStrTR<0x47, 8, 0x2CAE8000>("\x14\x26\x20\x3A\x22\x22\x2A" + 0x2CAE8000).s, ImVec2(3.90, 31), ImVec2(80, 20), Keyboard::WaitHack, Low, &Keyboard::WaitHack_Window))
				{
					Keyboard::WaitHack = !Keyboard::WaitHack;
				}
				// Sağ 4
				if (UI::SexyButton(/*Atk On Trgt*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3C\x22\x6A\x04\x22\x6D\x1A\x3D\x37\x25" + 0x2CAE8000).s, ImVec2(93.0f, 31), ImVec2(80, 20), Keyboard::AttackOnTarget, Low, &Keyboard::AttackOnTarget_Window))
				{
					Keyboard::AttackOnTarget = !Keyboard::AttackOnTarget;
				}
				// Sol 5
				if (UI::SexyButton(/*Auto Pot*/XorStrTR<0x47, 9, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x1C\x22\x3A" + 0x2CAE8000).s, ImVec2(3.90, 58), ImVec2(80, 20), Keyboard::AutoPot, Low, &Keyboard::AutoPot_Window))
				{
					Keyboard::AutoPot = !Keyboard::AutoPot;
				}
				// Sağ 6
				if (UI::SexyButton(/*Auto Pickup*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x1C\x24\x2D\x24\x25\x21" + 0x2CAE8000).s, ImVec2(93.0f, 58), ImVec2(80, 20), Keyboard::AutoPickUp, Low, &Keyboard::AutoPickUp_Window))
				{
					Keyboard::AutoPickUp = !Keyboard::AutoPickUp;
				}
				// Sol 7
				if (UI::SexyButton(/*KnockBack*/XorStrTR<0x47, 10, 0x2CAE8000>("\x0C\x26\x26\x29\x20\x0E\x2C\x2D\x24" + 0x2CAE8000).s, ImVec2(3.90, 86), ImVec2(80, 20), Keyboard::KnockBack, Low, &Keyboard::KnockBack_Window))
				{
					Keyboard::KnockBack = !Keyboard::KnockBack;
				}
				// Sağ 8
				if (UI::SexyButton(/*Collision*/XorStrTR<0x47, 10, 0x2CAE8000>("\x04\x27\x25\x26\x22\x3F\x24\x21\x21" + 0x2CAE8000).s, ImVec2(93.0f, 86), ImVec2(80, 20), Keyboard::Collision, Low, &Keyboard::Collision_Window))
				{
					Keyboard::Collision = !Keyboard::Collision;
				}
				// Sol 9
				if (UI::SexyButton(/*Boost*/XorStrTR<0x47, 6, 0x2CAE8000>("\x05\x27\x26\x39\x3F" + 0x2CAE8000).s, ImVec2(3.90, 113), ImVec2(80, 20), Keyboard::Boost, Low, &Keyboard::Boost_Window))
				{
					Keyboard::Boost = !Keyboard::Boost;
				}
				// Sağ 10
				if (UI::SexyButton(/*Name*/XorStrTR<0x47, 5, 0x2CAE8000>("\x09\x29\x24\x2F" + 0x2CAE8000).s, ImVec2(93.0f, 113), ImVec2(80, 20), Keyboard::Name, Low, &Keyboard::Name_Window))
				{
					Keyboard::Name = !Keyboard::Name;
				}
				// Sol 11
				if (UI::SexyButton(/*Auto CH*/XorStrTR<0x47, 8, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x0F\x05" + 0x2CAE8000).s, ImVec2(3.90, 141), ImVec2(80, 20), Keyboard::AutoCH, Low, &Keyboard::AutoCH_Window))
				{
					Keyboard::AutoCH = !Keyboard::AutoCH;
				}
				// Sağ 12
				if (UI::SexyButton(/*Auto Login*/XorStrTR<0x47, 11, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x29\x26\x3E" + 0x2CAE8000).s, ImVec2(93.0f, 141), ImVec2(80, 20), Keyboard::AutoLogin, Low, &Keyboard::AutoLogin_Window))
				{
					Keyboard::AutoLogin = !Keyboard::AutoLogin;
				}
				ImGui::EndChild();
			}
			break;
		case 1:
			if (ImGui::BeginChild(/*ChildPanel_Automaticly*/XorStrTR<0x47, 23, 0x2CAE8000>("\x04\x20\x20\x26\x2F\x1C\x2C\x20\x2A\x3C\x0E\x13\x26\x20\x3A\x3B\x36\x2C\x30\x39\x37\x25" + 0x2CAE8000).s, childSize, true, ImGuiWindowFlags_NoScrollbar))
			{
				// Sol 1
				if (UI::SexyButton(/*FarmBot*/XorStrTR<0x47, 8, 0x2CAE8000>("\x01\x29\x3B\x27\x09\x23\x39" + 0x2CAE8000).s, ImVec2(3.90, 4), ImVec2(80, 20), Keyboard::Bot, Low, &Keyboard::Bot_Winddow))
				{
					Keyboard::Bot = !Keyboard::Bot;
				}
				// Sağ 2 
				if (UI::SexyButton(/*Skills*/XorStrTR<0x47, 7, 0x2CAE8000>("\x14\x23\x20\x26\x27\x3F" + 0x2CAE8000).s, ImVec2(93.0f, 4), ImVec2(80, 20), Keyboard::Skills, Low, &Keyboard::Skills_Window))
				{
					Keyboard::Skills = !Keyboard::Skills;
				}
				// Sol 3 
				if (UI::SexyButton(/*AttackSpeed*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3C\x3D\x2B\x28\x27\x1E\x3E\x2A\x35\x35" + 0x2CAE8000).s, ImVec2(3.90, 31), ImVec2(80, 20), Keyboard::AttackSpeed, Low, &Keyboard::AttackSpeed_Window))
				{
					Keyboard::AttackSpeed = !Keyboard::AttackSpeed;
				}
				// Sağ 4
				if (UI::SexyButton(/*Move Speed*/XorStrTR<0x47, 11, 0x2CAE8000>("\x0A\x27\x3F\x2F\x6B\x1F\x3D\x2B\x2A\x34" + 0x2CAE8000).s, ImVec2(93.0f, 31), ImVec2(80, 20), Keyboard::MovementSpeed, Low, &Keyboard::MovementSpeed_Window))
				{
					Keyboard::MovementSpeed = !Keyboard::MovementSpeed;
				}
				// Sol 5
				if (UI::SexyButton(/*AntiFly*/XorStrTR<0x47, 8, 0x2CAE8000>("\x06\x26\x3D\x23\x0D\x20\x34" + 0x2CAE8000).s, ImVec2(3.90, 58), ImVec2(80, 20), Keyboard::AntiFly, Low, &Keyboard::AntiFly_Window))
				{
					Keyboard::AntiFly = !Keyboard::AntiFly;
				}
				// Sağ 6
				if (UI::SexyButton(/*AutoRevive*/XorStrTR<0x47, 11, 0x2CAE8000>("\x06\x3D\x3D\x25\x19\x29\x3B\x27\x39\x35" + 0x2CAE8000).s, ImVec2(93.0f, 58), ImVec2(80, 20), Keyboard::AutoRevive, Low, &Keyboard::AutoRevive_Window))
				{
					Keyboard::AutoRevive = !Keyboard::AutoRevive;
				}
				// Sol 7
				if (UI::SexyButton(/*Horse*/XorStrTR<0x47, 6, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E" + 0x2CAE8000).s, ImVec2(3.90, 86), ImVec2(80, 20), Keyboard::HorseChanger, Low, &Keyboard::HorseChanger_Window))
				{
					Keyboard::HorseChanger = !Keyboard::HorseChanger;
				}
				// Sağ 8
				if (UI::SexyButton(/*Chat*/XorStrTR<0x47, 5, 0x2CAE8000>("\x04\x20\x28\x3E" + 0x2CAE8000).s, ImVec2(93.0f, 86), ImVec2(80, 20), Keyboard::ChatSpawmmer, Low, &Keyboard::ChatSpawmmer_Window))
				{
					Keyboard::ChatSpawmmer = !Keyboard::ChatSpawmmer;
				}
				// Sol 9
				if (UI::SexyButton(/*Donate*/XorStrTR<0x47, 7, 0x2CAE8000>("\x03\x27\x27\x2B\x3F\x29" + 0x2CAE8000).s, ImVec2(3.90, 113), ImVec2(80, 20), Keyboard::XPDonate, Low, &Keyboard::XPDonate_Window))
				{
					Keyboard::XPDonate = !Keyboard::XPDonate;
				}
				// Sağ 10
				if (UI::SexyButton(/*Refresh*/XorStrTR<0x47, 8, 0x2CAE8000>("\x15\x2D\x2F\x38\x2E\x3F\x25" + 0x2CAE8000).s, ImVec2(93.0f, 113), ImVec2(80, 20), Keyboard::InstanceRefresh, Low, &Keyboard::InstanceRefresh_Window))
				{
					Keyboard::InstanceRefresh = !Keyboard::InstanceRefresh;
				}
				// Sol 11
				if (UI::SexyButton(/*Zoom*/XorStrTR<0x47, 5, 0x2CAE8000>("\x1D\x27\x26\x27" + 0x2CAE8000).s, ImVec2(3.90, 141), ImVec2(80, 20), Keyboard::CZoom, Low, &Keyboard::CZoom_Window))
				{
					Keyboard::CZoom = !Keyboard::CZoom;
				}
				// Sağ 12
				if (UI::SexyButton(/*Effect*/XorStrTR<0x47, 7, 0x2CAE8000>("\x02\x2E\x2F\x2F\x28\x38" + 0x2CAE8000).s, ImVec2(93.0f, 141), ImVec2(80, 20), Keyboard::EffectChanger, Low, &Keyboard::EffectChanger_Window))
				{
					Keyboard::EffectChanger = !Keyboard::EffectChanger;
				}
				ImGui::EndChild();
			}
			break;
		case 2:
			if (ImGui::BeginChild(/*ChildPanel_Dedectors*/XorStrTR<0x47, 21, 0x2CAE8000>("\x04\x20\x20\x26\x2F\x1C\x2C\x20\x2A\x3C\x0E\x16\x36\x30\x30\x35\x23\x37\x2B\x29" + 0x2CAE8000).s, childSize, true, ImGuiWindowFlags_NoScrollbar))
			{
				// Sol 1
				if (UI::SexyButton(/*GM Dedect*/XorStrTR<0x47, 10, 0x2CAE8000>("\x00\x05\x69\x0E\x2E\x28\x28\x2D\x3B" + 0x2CAE8000).s, ImVec2(3.90, 4), ImVec2(80, 20), Keyboard::GMDedect, Low, &Keyboard::GMDedect_Window))
				{
					Keyboard::GMDedect = !Keyboard::GMDedect;
				}
				// Sağ 2 
				if (UI::SexyButton(/*Stone Dedect*/XorStrTR<0x47, 13, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x09\x2B\x2B\x35\x32\x26" + 0x2CAE8000).s, ImVec2(93.0f, 4), ImVec2(80, 20), Keyboard::StoneDedect, Low, &Keyboard::StoneDedect_Window))
				{
					Keyboard::StoneDedect = !Keyboard::StoneDedect;
				}
				// Sol 3 
				if (UI::SexyButton(/*Player Dedect*/XorStrTR<0x47, 14, 0x2CAE8000>("\x17\x24\x28\x33\x2E\x3E\x6D\x0A\x2A\x34\x34\x31\x27" + 0x2CAE8000).s, ImVec2(3.90, 31), ImVec2(80, 20), Keyboard::PlayerDedect, Low, &Keyboard::PlayerDedect_Window))
				{
					Keyboard::PlayerDedect = !Keyboard::PlayerDedect;
				}
				// Sağ 4
				if (UI::SexyButton(/*Mining*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0A\x21\x27\x23\x25\x2B" + 0x2CAE8000).s, ImVec2(93.0f, 31), ImVec2(80, 20), Keyboard::MiningDedect, Low, &Keyboard::MiningDedect_Window))
				{
					Keyboard::MiningDedect = !Keyboard::MiningDedect;
				}
				// Sol 5
				if (UI::SexyButton(/*Portal*/XorStrTR<0x47, 7, 0x2CAE8000>("\x17\x27\x3B\x3E\x2A\x20" + 0x2CAE8000).s, ImVec2(3.90, 58), ImVec2(80, 20), Keyboard::PortalDedect, Low, &Keyboard::PortalDedect_Window))
				{
					Keyboard::PortalDedect = !Keyboard::PortalDedect;
				}
				// Sağ 6
				if (UI::SexyButton(/*Teleport*/XorStrTR<0x47, 9, 0x2CAE8000>("\x13\x2D\x25\x2F\x3B\x23\x3F\x3A" + 0x2CAE8000).s, ImVec2(93.0f, 58), ImVec2(80, 20), Keyboard::TeleportDedect, Low, &Keyboard::TeleportDedect_Window))
				{
					Keyboard::TeleportDedect = !Keyboard::TeleportDedect;
				}
				// Sol 7
				if (UI::SexyButton(/*Area Dedect*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3A\x2C\x2B\x6B\x08\x28\x2A\x2A\x33\x25" + 0x2CAE8000).s, ImVec2(3.90, 86), ImVec2(80, 20), Keyboard::AreaDedect, Low, &Keyboard::AreaDedect_Window))
				{
					Keyboard::AreaDedect = !Keyboard::AreaDedect;
				}
				// Sağ 8
				if (UI::SexyButton(/*PM Dedect*/XorStrTR<0x47, 10, 0x2CAE8000>("\x17\x05\x69\x0E\x2E\x28\x28\x2D\x3B" + 0x2CAE8000).s, ImVec2(93.0f, 86), ImVec2(80, 20), Keyboard::PMDedect, Low, &Keyboard::PMDedect_Window))
				{
					Keyboard::PMDedect = !Keyboard::PMDedect;
				}
				// Sol 9
				if (UI::SexyButton(/*Spawm Dedect*/XorStrTR<0x47, 13, 0x2CAE8000>("\x14\x38\x28\x3D\x26\x6C\x09\x2B\x2B\x35\x32\x26" + 0x2CAE8000).s, ImVec2(3.90, 113), ImVec2(80, 20), Keyboard::SpawmDedect, Low, &Keyboard::SpawmDedect_Window))
				{
					Keyboard::SpawmDedect = !Keyboard::SpawmDedect;
				}
				//// Sağ 10
				//if (UI::SexyButton("Refresh", ImVec2(93.0f, 113), ImVec2(78, 20), Keyboard::InstanceRefresh, Low, &Keyboard::InstanceRefresh_Window))
				//{
				//	Keyboard::InstanceRefresh = !Keyboard::InstanceRefresh;
				//}
				//// Sol 11
				//if (UI::SexyButton("Zoom", ImVec2(3.90, 141), ImVec2(78, 20), Keyboard::CZoom, Low, &Keyboard::CZoom_Window))
				//{
				//	Keyboard::CZoom = !Keyboard::CZoom;
				//}
				//// Sağ 12
				//if (UI::SexyButton("Effect", ImVec2(93.0f, 141), ImVec2(78, 20), Keyboard::EffectChanger, Low, &Keyboard::EffectChanger_Window))
				//{
				//	Keyboard::EffectChanger = !Keyboard::EffectChanger;
				//}
				ImGui::EndChild();
			}
			break;
		case 3:
			if (ImGui::BeginChild(/*ChildPanel_Helpers*/XorStrTR<0x47, 19, 0x2CAE8000>("\x04\x20\x20\x26\x2F\x1C\x2C\x20\x2A\x3C\x0E\x1A\x36\x38\x25\x33\x25\x2B" + 0x2CAE8000).s, childSize, true, ImGuiWindowFlags_NoScrollbar))
			{
				// Sol 1
				if (UI::SexyButton(/*Auto Dungeon*/XorStrTR<0x47, 13, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x08\x38\x20\x28\x35\x3E\x3C" + 0x2CAE8000).s, ImVec2(3.90, 4), ImVec2(80, 20), Keyboard::AutoDungeon, Low, &Keyboard::AutoDungeon_Window))
				{
					Keyboard::AutoDungeon = !Keyboard::AutoDungeon;
				}
				// Sağ 2
				if (UI::SexyButton(/*WS*/XorStrTR<0x47, 3, 0x2CAE8000>("\x10\x1B" + 0x2CAE8000).s, ImVec2(93.0f, 4), ImVec2(78, 20), Keyboard::WSCheat, Low, &Keyboard::WSCheat_Window))
				{
					Keyboard::WSCheat = !Keyboard::WSCheat;
				}
				// Sol 3
				if (UI::SexyButton(/*Loader*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0B\x27\x28\x2E\x2E\x3E" + 0x2CAE8000).s, ImVec2(3.90, 31), ImVec2(80, 20), Keyboard::Loader, Low, &Keyboard::Loader_Window))
				{
					Keyboard::Loader = !Keyboard::Loader;
				}
				// Sağ 4
				if (UI::SexyButton(/*Fish*/XorStrTR<0x47, 5, 0x2CAE8000>("\x01\x21\x3A\x22" + 0x2CAE8000).s, ImVec2(93.0f, 31), ImVec2(80, 20), Keyboard::Fish, Low, &Keyboard::Fish_Window))
				{
					Keyboard::Fish = !Keyboard::Fish;
				}
				// Sol 5
				if (UI::SexyButton(/*Minimap*/XorStrTR<0x47, 8, 0x2CAE8000>("\x0A\x21\x27\x23\x26\x2D\x3D" + 0x2CAE8000).s, ImVec2(3.90, 58), ImVec2(80, 20), Keyboard::Minimap, Low, &Keyboard::Minimap_Window))
				{
					Keyboard::Minimap = !Keyboard::Minimap;
				}
				// Sağ 6
				if (UI::SexyButton(/*Route*/XorStrTR<0x47, 6, 0x2CAE8000>("\x15\x27\x3C\x3E\x2E" + 0x2CAE8000).s, ImVec2(93.0f, 58), ImVec2(80, 20), Keyboard::Route, Low, &Keyboard::Route_Window))
				{
					Keyboard::Route = !Keyboard::Route;
				}
				// Sol 7
				if (UI::SexyButton(/*Buff*/XorStrTR<0x47, 5, 0x2CAE8000>("\x05\x3D\x2F\x2C" + 0x2CAE8000).s, ImVec2(3.90, 86), ImVec2(80, 20), Keyboard::BuffBot, Low, &Keyboard::Buff_Window))
				{
					Keyboard::BuffBot = !Keyboard::BuffBot;
				}
				ImGui::EndChild();
			}
			break;
		case 4:
			ImGui::SetCursorPos(ImVec2(ImGui::GetCursorPosX() - 5, ImGui::GetCursorPosY() -1));
			if (ImGui::BeginChild(XorStrTR<0x47, 20, 0x2CAE8000>("\x04\x20\x20\x26\x2F\x1C\x2C\x20\x2A\x3C\x0E\x01\x36\x20\x21\x3F\x39\x3F\x2A" + 0x2CAE8000).s, SettingSize, true, ImGuiWindowFlags_NoScrollbar))
			{
				//ImGui::Text(/*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3D" + 0x2CAE8000).s);
				
				
				if (cfgListDirty)
					RefreshCfgList();

				std::vector<const char*> listItems;
				for (auto& s : cfgFiles) listItems.push_back(s.c_str());
				int listItemCount = (int)listItems.size();


				UI::HLListBoxIcon(/*##config*/XorStrTR<0x47, 9, 0x2CAE8000>("\x64\x6B\x2A\x25\x25\x2A\x24\x29" + 0x2CAE8000).s,ImVec2(0, 0),(int)SettingSize.x,(int)SettingSize.y - 53,listItems.data(),listItemCount,&selectedItem,Low,(ImTextureID)Folder,(ImTextureID)ScrollGrap,(ImTextureID)ArrowUP,(ImTextureID)ArrowDown,"Config");
				 

				if (ImGui::IsItemHovered() && ImGui::IsMouseClicked(0))
				{
					if (selectedItem >= 0 && selectedItem < listItemCount)
						strncpy_s(cfgNameBuf, cfgFiles[selectedItem].c_str(), sizeof(cfgNameBuf) - 1);
				}

				if (ImGui::IsItemHovered() && ImGui::IsMouseDoubleClicked(0))
				{
					if (selectedItem >= 0 && selectedItem < listItemCount)
					{
						if (Config::Load(cfgFiles[selectedItem]))
						{
							notifTimer = 2.5f;
						}
						else
						{
							notifTimer = 2.5f;
						}
					}
				}

				if (notifTimer > 0.0f)
				{
					notifTimer -= ImGui::GetIO().DeltaTime;
					ImGui::SetCursorPos(ImVec2(8, 117));
					ImGui::TextColored(ImVec4(0.3f, 1.0f, 0.4f, notifTimer / 2.5f), notifMsg);
				}
				 
				UI::HLTextBox(/*##configss*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x2A\x25\x25\x2A\x24\x29\x3C\x23" + 0x2CAE8000).s, ImVec2(8, 143 - 23 - 3), 173, 20, cfgNameBuf, sizeof(cfgNameBuf), Low, /*Write Config Name*/XorStrTR<0x47, 18, 0x2CAE8000>("\x10\x3A\x20\x3E\x2E\x6C\x0E\x21\x21\x36\x38\x35\x73\x1A\x34\x3B\x32" + 0x2CAE8000).s);

				if (UI::SexyButton(/*x*/XorStrTR<0x47, 2, 0x2CAE8000>("\x3F" + 0x2CAE8000).s, ImVec2(8, 143), ImVec2(25, 20), false, Low, nullptr))
				{
					std::string saveName = (strlen(cfgNameBuf) > 0) ? cfgNameBuf : /*default*/XorStrTR<0x47, 8, 0x2CAE8000>("\x23\x2D\x2F\x2B\x3E\x20\x39" + 0x2CAE8000).s;

					if (Config::Save(saveName))
					{
						notifTimer = 2.5f;
						RefreshCfgList();
					}
					else
					{
						notifTimer = 2.5f;
					}
				}

				if (UI::SexyButton(/*__*/XorStrTR<0x47, 3, 0x2CAE8000>("\x18\x17" + 0x2CAE8000).s, ImVec2(155, 143), ImVec2(25, 20), false, Low, nullptr))
				{
					if (selectedItem >= 0 && selectedItem < listItemCount)
					{
						std::string deleteName = cfgFiles[selectedItem];
						Config::Delete(deleteName);
						notifTimer = 2.5f;
						memset(cfgNameBuf, 0, sizeof(cfgNameBuf)); 
						RefreshCfgList();
					}
					else
					{
						notifTimer = 2.0f;
					}
				}

				if (UI::SexyButton(/*Refresh*/XorStrTR<0x47, 8, 0x2CAE8000>("\x15\x2D\x2F\x38\x2E\x3F\x25" + 0x2CAE8000).s, ImVec2(70, 143), ImVec2(55, 20), false, Low, nullptr))
				{
					std::string loadName = "";

					if (strlen(cfgNameBuf) > 0)
						loadName = cfgNameBuf;
					else if (selectedItem >= 0 && selectedItem < listItemCount)
						loadName = cfgFiles[selectedItem];

					if (!loadName.empty())
					{
						if (Config::Load(loadName))
						{
							notifTimer = 2.5f;
						}
						else
						{
							notifTimer = 2.5f;
						}
					}
					else
					{
						RefreshCfgList();
						notifTimer = 1.5f;
					}
				}
				 ImGui::EndChild();
			}
			break;
		default:
			break;
		}

		ImGui::PopStyleColor();
		ImGui::PopFont(); 
	}

}


LPDIRECT3DTEXTURE9 g_pTexture = nullptr;

bool LoadTextureFromFileWithD3DX9(LPDIRECT3DDEVICE9 pDevice, const char* filePath)
{
	if (!pDevice || !filePath)
		return false;

	if (FAILED(D3DXCreateTextureFromFileA(pDevice, filePath, &g_pTexture)))
	{
		return false;
	}
	return true;
}

//=====================================================================================================================

static bool FixKey = true;
HRESULT APIENTRY EndScene_hook(IDirect3DDevice9* pDevice)
{
	CreateResourceFolders();
	if (pDevice == nullptr) return EndScene_orig(pDevice);
	if (!is_imgui_initialised)
	{
		if (FixKey)
		{
			GenerateTexture(pDevice, &Red, D3DCOLOR_ARGB(255, 255, 0, 0));
			GenerateTexture(pDevice, &Green, D3DCOLOR_RGBA(0, 255, 0, 255));
			GenerateTexture(pDevice, &Blue, D3DCOLOR_ARGB(255, 0, 0, 255));
			GenerateTexture(pDevice, &Yellow, D3DCOLOR_ARGB(255, 255, 255, 0));
			IMGUI_CHECKVERSION();
			ImGui::CreateContext();
			ImGuiIO& io = ImGui::GetIO(); (void)io;

			Ariel = io.Fonts->AddFontFromFileTTF("C:\\Windows\\Fonts\\arialbd.ttf", 18.0f);
			Low = io.Fonts->AddFontFromFileTTF("C:\\Windows\\Fonts\\arialbd.ttf", 11.0f);
			Blood = io.Fonts->AddFontFromFileTTF("C:\\Windows\\Fonts\\arialbd.ttf", 14.0f);
			Windows = io.Fonts->AddFontFromFileTTF("C:\\Windows\\Fonts\\arialbd.ttf", 14.0f);
			io.Fonts->Build();

			ImGui::GetIO().WantCaptureMouse || ImGui::GetIO().WantTextInput || ImGui::GetIO().WantCaptureKeyboard;
			io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
			D3DDEVICE_CREATION_PARAMETERS d3dcp;
			pDevice->GetCreationParameters(&d3dcp);
			game_hwnd = d3dcp.hFocusWindow;

			if (game_hwnd != NULL)
			{
				game_wndproc = (WNDPROC)SetWindowLongPtr(game_hwnd, GWLP_WNDPROC, (LONG_PTR)WndProc);
				ImGui_ImplWin32_Init(game_hwnd);
				ImGui_ImplDX9_Init(pDevice);
				ImGui::GetIO().ImeWindowHandle = game_hwnd;
				engine::ShowBalloonV2(game_hwnd,/*Apathe.net*/XorStrTR<0x47, 11, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x63\x20\x2A\x24" + 0x2CAE8000).s, /*Apathe (@Ezgi) | Free Version*/XorStrTR<0x47, 30, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x6D\x66\x0F\x15\x2B\x35\x3A\x7D\x75\x2A\x77\x1E\x2B\x3F\x3E\x7C\x0B\x3B\x2D\x13\x08\x0D\x0D" + 0x2CAE8000).s);
				PlaySoundA((LPCSTR)TRVoice::Apathe_Hosgeldiniz, NULL, SND_MEMORY | SND_ASYNC);
				is_imgui_initialised = true;
			}
		}
	}

	ImGui_ImplDX9_NewFrame();
	ImGui_ImplWin32_NewFrame();
	ImGui::NewFrame();


	static auto is_down = false;
	static auto is_clicked = false;
	if (GetAsyncKeyState(VK_INSERT))
	{
		is_clicked = false;
		is_down = true;
	}
	else if (!GetAsyncKeyState(VK_INSERT) && is_down)
	{
		is_clicked = true;
		is_down = false;
	}
	else {
		is_clicked = false;
		is_down = false;
	}

	//show menu
	if (is_clicked)
	{
		ShowMenu = !ShowMenu;
	}

	if (ShowMenu)
		ImGui::GetIO().MouseDrawCursor = 1;
	else
		ImGui::GetIO().MouseDrawCursor = 0;

	if (Background == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Background, sizeof(HL::HL_Background), &Background);

	if (Sword == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Sword, sizeof(HL::HL_Sword), &Sword);

	if (AutoFarm == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_AutoMaticly, sizeof(HL::HL_AutoMaticly), &AutoFarm);

	if (Online == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Online, sizeof(HL::HL_Online), &Online);

	if (Helper == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Helper, sizeof(HL::HL_Helper), &Helper);

	if (Setting == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Setting, sizeof(HL::HL_Setting), &Setting);

	if (HLOpen == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Open, sizeof(HL::HL_Open), &HLOpen);

	if (HLClose == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Close, sizeof(HL::HL_Close), &HLClose);

	if (HLCloseV2 == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_CloseV2, sizeof(HL::HL_CloseV2), &HLCloseV2);

	if (HLFature == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Fature, sizeof(HL::HL_Fature), &HLFature);

	if (Pelerin == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HLPelerin, sizeof(HL::HLPelerin), &Pelerin);

	if (Redpot == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HLRedPot, sizeof(HL::HLRedPot), &Redpot);

	if (Bluepot == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HLBluepot, sizeof(HL::HLBluepot), &Bluepot);

	// Listbox
	if (ScrollGrap == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_ScrollGrap, sizeof(HL::HL_ScrollGrap), &ScrollGrap);

	if (ArrowUP == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Arrow_UP, sizeof(HL::HL_Arrow_UP), &ArrowUP);

	if (ArrowDown == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Arrow_Down, sizeof(HL::HL_Arrow_Down), &ArrowDown);

	if (Folder == NULL)
		D3DXCreateTextureFromFileInMemory(pDevice, HL::HL_Folder, sizeof(HL::HL_Folder), &Folder);

	if (FixKey)
	{
		if (ShowMenu)
		{
			ImGui::SetNextWindowSize(ImVec2(300, 272));
			ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0, 0, 0, 0));
			ImGui::PushStyleColor(ImGuiCol_Border, ImVec4(0, 0, 0, 0));

			if (ImGui::Begin(/** Apathe*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x08\x3A\x2A\x38\x25\x2B" + 0x2CAE8000).s, nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_NoBringToFrontOnFocus | ImGuiWindowFlags_NoBackground))
			{
				ImVec2 pos = ImGui::GetWindowPos();
				ImDrawList* draw = ImGui::GetWindowDrawList();
				// Background
				draw->AddImage((void*)Background, ImVec2(pos.x + 30, pos.y + 1), ImVec2(pos.x + 30 + 270, pos.y + 1 + 270));
				// Txt
				ImVec2 textPos = ImVec2(pos.x + 123, pos.y + 27);
				ImGui::PushFont(Ariel);
				ImGui::GetWindowDrawList()->AddText(textPos, ImColor(191, 191, 191), /*Apathe.net*/XorStrTR<0x47, 11, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x63\x20\x2A\x24" + 0x2CAE8000).s);

				if (UI::CloseButton("X", ImVec2(63, 27), ImVec2(20, 20), true, Low, /*Game exit*/XorStrTR<0x47, 10, 0x2CAE8000>("\x00\x29\x24\x2F\x6B\x29\x35\x27\x3B" + 0x2CAE8000).s))
				{
					exit(0);
				}
				CheatRender::Cheat_Render();

				// CH
				render_ui::CH_Render();
				// Game Status
				render_ui::GameStatus(true);
				// UI Buttons
				render_ui::Buttons();

				// ============================================= Genel ========================================== //
				// Attack Windows
				if (Keyboard::AutoAttack_Window)
				{
					int X = 190;
					int Y = 140+25;
					if (WindowCreate::WindowCloser(/*##Apathe_AutoAttack*/XorStrTR<0x47, 20, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x24\x26\x3C\x15\x21\x22\x36\x3B\x32" + 0x2CAE8000).s, /** Auto Attack*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x0F\x3B\x24\x30\x31\x38" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AutoAttack_Window))
					{
						//UI::HLNumberBox("##Attack_Speed", ImVec2(96, 40), 35, 20, &Keyboard::C_AttackSpeedValue, Low);
						// Sol
						if (UI::SexyButtonV2(/*Auto Attack*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x0D\x39\x3A\x2E\x33\x3A" + 0x2CAE8000).s, ImVec2(10, 90 + 25+25), ImVec2(178, 20), Keyboard::AutoAttacks, Low))
						{
							Keyboard::AutoAttacks = !Keyboard::AutoAttacks;
						}


						UI::HLToggleButton(/*#1*/XorStrTR<0x47, 3, 0x2CAE8000>("\x64\x79" + 0x2CAE8000).s, &Keyboard::C_AutoSpaceV1, /** Auto Space v1*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1D\x3F\x31\x32\x37\x73\x22\x64" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /*Auto Space : (v1)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x1F\x3D\x2F\x2C\x35\x71\x68\x73\x7C\x23\x67\x7E" + 0x2CAE8000).s);
						UI::HLToggleButton(/*#2*/XorStrTR<0x47, 3, 0x2CAE8000>("\x64\x7A" + 0x2CAE8000).s, &Keyboard::C_AutoSpaceV2, /** Auto Space v2*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1D\x3F\x31\x32\x37\x73\x22\x67" + 0x2CAE8000).s, ImVec2(10, 65), 178, 19, /*Auto Space : (v2)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x1F\x3D\x2F\x2C\x35\x71\x68\x73\x7C\x23\x64\x7E" + 0x2CAE8000).s);
						UI::HLToggleButton(/*#3*/XorStrTR<0x47, 3, 0x2CAE8000>("\x64\x7B" + 0x2CAE8000).s, &Keyboard::C_AutoSpaceV3, /** Auto Space v3*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1D\x3F\x31\x32\x37\x73\x22\x66" + 0x2CAE8000).s, ImVec2(10, 90), 178, 19, /*Auto Space : (v3)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x1F\x3D\x2F\x2C\x35\x71\x68\x73\x7C\x23\x65\x7E" + 0x2CAE8000).s);
						UI::HLToggleButton(/*#4*/XorStrTR<0x47, 3, 0x2CAE8000>("\x64\x7C" + 0x2CAE8000).s, &Keyboard::C_AutoRotation, /** Change Character Direction*/XorStrTR<0x47, 29, 0x2CAE8000>("\x6D\x68\x0A\x22\x2A\x22\x2A\x2B\x6F\x13\x39\x33\x21\x35\x36\x22\x32\x2A\x79\x1E\x32\x2E\x38\x3D\x2B\x09\x0E\x0C" + 0x2CAE8000).s, ImVec2(10, 90+25), 178, 19, /*Change Character Direction*/XorStrTR<0x47, 27, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x31\x23\x33\x30\x20\x30\x24\x77\x1C\x30\x28\x3E\x3F\x29\x37\x30\x0E" + 0x2CAE8000).s);


						//UI::HLCheckBox("##autospacev1", &Keyboard::C_AutoSpaceV1, ImVec2(100, 40), 19, 19, "Auto Space : (v1)");
						//UI::HLCheckBox("##autospacev2", &Keyboard::C_AutoSpaceV2, ImVec2(130, 40), 19, 19, "Auto Space : (v2)");
						//UI::HLCheckBox("##autorotation", &Keyboard::C_AutoRotation, ImVec2(160, 40), 19, 19, "Change Character Direction");
					}
					ImGui::End();
				}
				// Mobber Window
				if (Keyboard::Mobber_Window)
				{
					int X = 180; // X sağ doğru genislik
					int Y = 165; // Y Alta Doğru Genislik :D
					if (WindowCreate::WindowCloser(/*##Apathe_Mobber*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x1D\x3E\x30\x31\x31\x27" + 0x2CAE8000).s, /** Mobber*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x04\x25\x29\x2E\x28\x3C" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Mobber_Window))
					{
						// Image 1
						UI::HLBackground(ImVec2(13, 41), 35, 35);

						ImGui::SetCursorPos(ImVec2(16, 44));
						ImGui::Image((void*)Pelerin, ImVec2(30, 30));


						//// Sağ F1
						UI::HLCheckBox(/*##f1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x2F\x7B" + 0x2CAE8000).s, &Keyboard::C_F1, ImVec2(55, 47), 23, 23, /*Select : (F1)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x17\x63\x7A" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##f2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x2F\x78" + 0x2CAE8000).s, &Keyboard::C_F2, ImVec2(88, 47), 23, 23, /*Select : (F2)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x17\x60\x7A" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##f3*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x2F\x79" + 0x2CAE8000).s, &Keyboard::C_F3, ImVec2(120, 47), 23, 23, /*Select : (F3)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x17\x61\x7A" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##f4*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x2F\x7E" + 0x2CAE8000).s, &Keyboard::C_F4, ImVec2(150, 47), 23, 23, /*Select : (F4]*/XorStrTR<0x47, 14, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x17\x66\x0E" + 0x2CAE8000).s);

						// Image 2
						UI::HLBackground(ImVec2(13, 85), 35, 35);

						ImGui::SetCursorPos(ImVec2(16, 88));
						ImGui::Image((void*)Pelerin, ImVec2(30, 30));

						//// Sağ Slot
						int Y = 90;
						UI::HLCheckBox(/*##slot1*/XorStrTR<0x47, 8, 0x2CAE8000>("\x64\x6B\x3A\x26\x24\x38\x7C" + 0x2CAE8000).s, &Keyboard::C_Slot1, ImVec2(55, Y), 23, 23, /*Select : (Slot 1)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x02\x3E\x3C\x20\x75\x67\x7E" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##slot2*/XorStrTR<0x47, 8, 0x2CAE8000>("\x64\x6B\x3A\x26\x24\x38\x7F" + 0x2CAE8000).s, &Keyboard::C_Slot2, ImVec2(88, Y), 23, 23, /*Select : (Slot 2)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x02\x3E\x3C\x20\x75\x64\x7E" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##slot3*/XorStrTR<0x47, 8, 0x2CAE8000>("\x64\x6B\x3A\x26\x24\x38\x7E" + 0x2CAE8000).s, &Keyboard::C_Slot3, ImVec2(120, Y), 23, 23, /*Select : (Slot 3)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x02\x3E\x3C\x20\x75\x65\x7E" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##slot4*/XorStrTR<0x47, 8, 0x2CAE8000>("\x64\x6B\x3A\x26\x24\x38\x79" + 0x2CAE8000).s, &Keyboard::C_Slot4, ImVec2(150, Y), 23, 23, /*Select : (Slot 4)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x74\x6F\x78\x02\x3E\x3C\x20\x75\x62\x7E" + 0x2CAE8000).s);

						// Orta
						if (UI::SexyButtonV2(/*Mobber*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0A\x27\x2B\x28\x2E\x3E" + 0x2CAE8000).s, ImVec2(55, 130), ImVec2(78, 20), Keyboard::Mobber, Low))
						{
							Keyboard::Mobber = !Keyboard::Mobber;
						}
					}
					ImGui::End();
				}

				// Wait Hack
				if (Keyboard::WaitHack_Window)
				{
					if (WindowCreate::WindowCloser(/*##wait_exploit*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x3E\x2B\x22\x38\x12\x2B\x37\x20\x3D\x3D\x3A\x20" + 0x2CAE8000).s, /** Sniping*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x1A\x24\x22\x3C\x24\x20\x28" + 0x2CAE8000).s, Keyboard::Wait_GuiSizeX, Keyboard::Wait_GuiSizeY, Keyboard::Wait_GuiSizeX + 10, Keyboard::Wait_GuiSizeY + 10, Windows, &Keyboard::WaitHack_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##Sniping::auto::tabs*/XorStrTR<0x47, 22, 0x2CAE8000>("\x64\x6B\x1A\x24\x22\x3C\x24\x20\x28\x6A\x6B\x33\x26\x20\x3A\x6C\x6D\x2C\x38\x38\x28" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Wait*/XorStrTR<0x47, 5, 0x2CAE8000>("\x10\x29\x20\x3E" + 0x2CAE8000).s))
							{
								Keyboard::Wait_GuiSizeX = 230;
							
								if (UI::SexyButtonV2(/*Sniping*/XorStrTR<0x47, 8, 0x2CAE8000>("\x14\x26\x20\x3A\x22\x22\x2A" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::WaitHack, Low))
								{
									Keyboard::WaitHack = !Keyboard::WaitHack;
								}
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_WaitDamage, ImVec2(95, 60), 19, 19, /*Wait Damage*/XorStrTR<0x47, 12, 0x2CAE8000>("\x10\x29\x20\x3E\x6B\x08\x2C\x23\x2E\x37\x34" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_RangeWait, ImVec2(120,60), 19, 19, /*Range Sniping*/XorStrTR<0x47, 14, 0x2CAE8000>("\x15\x29\x27\x2D\x2E\x6C\x1E\x20\x26\x20\x38\x3C\x34" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_ProdDamage, ImVec2(120+25, 60), 19, 19, /*Pro Damage (100x)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x17\x3A\x26\x6A\x0F\x2D\x20\x2F\x28\x35\x71\x7A\x62\x64\x65\x2E\x7E" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_RangeWaitLine, ImVec2(120 + 25 + 25,60), 19, 19, /*Draw Lines*/XorStrTR<0x47, 11, 0x2CAE8000>("\x03\x3A\x28\x3D\x6B\x00\x24\x20\x2A\x23" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_WaitNinja, ImVec2(120 + 25 + 25 + 25, 60), 19, 19, /*Ninja (Wait Range)*/XorStrTR<0x47, 19, 0x2CAE8000>("\x09\x21\x27\x20\x2A\x6C\x65\x19\x2E\x39\x25\x72\x01\x35\x3B\x31\x32\x71" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Exploit*/XorStrTR<0x47, 8, 0x2CAE8000>("\x02\x30\x39\x26\x24\x25\x39" + 0x2CAE8000).s))
							{
								Keyboard::Wait_GuiSizeX = 230;
								if (UI::SexyButtonV2(/*Sniping*/XorStrTR<0x47, 8, 0x2CAE8000>("\x14\x26\x20\x3A\x22\x22\x2A" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::WaitHack, Low))
								{
									Keyboard::WaitHack = !Keyboard::WaitHack;
								}
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_DumyDamage, ImVec2(95, 60), 19, 19, "* Dummy Range");
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3D" + 0x2CAE8000).s))
							{
								Keyboard::Wait_GuiSizeX = 345;
								if (UI::SexyButtonV2(/*Sniping*/XorStrTR<0x47, 8, 0x2CAE8000>("\x14\x26\x20\x3A\x22\x22\x2A" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::WaitHack, Low))
								{
									Keyboard::WaitHack = !Keyboard::WaitHack;
								}
								UI::HLNumberBoxFloat(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_WaitDistance, Low, /*Wait Speed (Value)*/XorStrTR<0x47, 19, 0x2CAE8000>("\x10\x29\x20\x3E\x6B\x1F\x3D\x2B\x2A\x34\x71\x7A\x05\x35\x39\x23\x32\x71" + 0x2CAE8000).s);
								UI::HLNumberBoxInt(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, ImVec2(135, 60), 35, 20, &Keyboard::C_WaitSpeed, Low, /*Range (Distance)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x15\x29\x27\x2D\x2E\x6C\x65\x0A\x26\x23\x25\x33\x3D\x37\x30\x7F" + 0x2CAE8000).s);
								UI::HLNumberBoxInt(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, ImVec2(135 + 40, 60), 40, 20, &Keyboard::C_DieTime, Low, /*Die (Time)*/XorStrTR<0x47, 11, 0x2CAE8000>("\x03\x21\x2C\x6A\x63\x18\x24\x23\x2A\x79" + 0x2CAE8000).s);
								int x = 45; // Mennüyü oldugu gibi kaydırıyorum
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_WaitMonster, ImVec2(175 + x, 60), 19, 19, /*NPC (Select)*/XorStrTR<0x47, 13, 0x2CAE8000>("\x09\x18\x0A\x6A\x63\x1F\x28\x22\x2A\x33\x25\x7B" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_WaitBoss, ImVec2(200 + x, 60), 19, 19, /*Boss (Select)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x05\x27\x3A\x39\x6B\x64\x1E\x2B\x23\x35\x32\x26\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_WaitStone, ImVec2(225 + x, 60), 19, 19, /*Stone (Select)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x65\x1D\x2A\x3C\x34\x31\x27\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_WaitOther, ImVec2(250 + x, 60), 19, 19, /*Other (Select)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x08\x3C\x21\x2F\x39\x6C\x65\x1D\x2A\x3C\x34\x31\x27\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_DieTimeEnable, ImVec2(275+x,60), 19, 19, /*If you die, it checks the Cooldown Interval and toggles it on or off accordingly.*/XorStrTR<0x47, 82, 0x2CAE8000>("\x0E\x2E\x69\x33\x24\x39\x6D\x2A\x26\x35\x7D\x72\x3A\x20\x75\x35\x3F\x3D\x3A\x31\x28\x7C\x29\x36\x3A\x40\x22\x0D\x0C\x08\x01\x09\x10\x06\x49\x23\x05\x18\x08\x1C\x19\x11\x1D\x52\x12\x1A\x11\x56\x03\x17\x1E\x1D\x17\x19\x0E\x5E\x16\xF4\xA1\xED\xED\xA4\xEA\xF4\xA7\xE7\xEF\xEC\xAB\xED\xEE\xED\xE0\xE2\xF5\xFB\xFD\xF3\xF9\xEF\xB9" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				//// Wait Hack Window
				//if (Keyboard::WaitHack_Window)
				//{
				//	int X = 230;
				//	int Y = 110;
				//	if (WindowCreate::WindowCloser("##Apathe_WaitHack", "* Wait Hack", X, Y, X + 10, Y + 10, Windows, &Keyboard::WaitHack_Window))
				//	{
				//		// Sol
				//		if (UI::SexyButtonV2("WaitHack", ImVec2(10, 40), ImVec2(78, 20), Keyboard::WaitHack, Low))
				//		{
				//			Keyboard::WaitHack = !Keyboard::WaitHack;
				//		}

				//		UI::HLNumberBoxInt("##awitSpeed", ImVec2(96, 40), 35, 20, &Keyboard::C_WaitSpeed, Low, "Range (Distance)");

				//		UI::HLNumberBoxFloat("##WaitRange", ImVec2(136, 40), 35, 20, &Keyboard::C_WaitDistance, Low, "Wait Speed (Value)");

				//		UI::HLNumberBoxInt("##DieTime", ImVec2(136 + 40, 40), 40, 20, &Keyboard::C_DieTime, Low, "Die (Time)");

				//		UI::HLCheckBox("##1", &Keyboard::C_WaitMonster, ImVec2(10, 65), 19, 19, "NPC");
				//		UI::HLCheckBox("##2", &Keyboard::C_WaitBoss, ImVec2(36, 65), 19, 19, "Boss");
				//		UI::HLCheckBox("##3", &Keyboard::C_WaitStone, ImVec2(36 + 26, 65), 19, 19, "Stone");
				//		UI::HLCheckBox("##4", &Keyboard::C_WaitOther, ImVec2(36 + 26 + 26, 65), 19, 19, "Other");
				//		UI::HLCheckBox("##5", &Keyboard::C_RangeWait, ImVec2(36 + 26 + 26 + 26, 65), 19, 19, "Range Wait");
				//		UI::HLCheckBox("##6", &Keyboard::C_RangeWaitLine, ImVec2(36 + 26 + 26 + 26 + 26, 65), 19, 19, "Draw Lines");
				//		UI::HLCheckBox("##7", &Keyboard::C_WaitNinja, ImVec2(36 + 26 + 26 + 26 + 26 + 26, 65), 19, 19, "Ninja");

				//		// Yeni
				//		UI::HLCheckBox("##ProDamage", &Keyboard::C_ProdDamage, ImVec2(10, 90), 19, 19, "Pro Damage (100x)");

				//		UI::HLCheckBox("##12", &Keyboard::C_DieTimeEnable, ImVec2(36, 90), 19, 19, "If you die, it checks the Cooldown Interval and toggles it on or off accordingly.");

				//	}
				//	ImGui::End();
				//}

				// Attack On Target
				if (Keyboard::AttackOnTarget_Window)
				{
					int X = 150;
					int Y = 65;
					if (WindowCreate::WindowCloser(/*##Apathe_AtkOnTarget*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x25\x39\x1C\x3A\x01\x37\x25\x3F\x3C\x2E" + 0x2CAE8000).s, /** Attack On Target*/XorStrTR<0x47, 19, 0x2CAE8000>("\x6D\x68\x08\x3E\x3F\x2D\x2E\x25\x6F\x1F\x3F\x72\x07\x35\x27\x31\x32\x2C" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AttackOnTarget_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Atk On Target*/XorStrTR<0x47, 14, 0x2CAE8000>("\x06\x3C\x22\x6A\x04\x22\x6D\x1A\x2E\x22\x36\x37\x27" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::AutoAttacks, Low))
						{
							Keyboard::AttackOnTarget = !Keyboard::AttackOnTarget;
						}
						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_RangeTarget, ImVec2(100, 40), 19, 19, /*Range Target*/XorStrTR<0x47, 13, 0x2CAE8000>("\x15\x29\x27\x2D\x2E\x6C\x19\x2F\x3D\x37\x34\x26" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Auto Pot Inventory
				if (Keyboard::AutoPot_Window)
				{
					int X = 180; // X sağ doğru genislik
					int Y = 165; // Y Alta Doğru Genislik :D
					if (WindowCreate::WindowCloser(/*##Apathe_Autopot*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x24\x26\x3C\x24\x3A\x22" + 0x2CAE8000).s, /** Auto Pot Inventory*/XorStrTR<0x47, 21, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1E\x20\x24\x71\x1B\x3D\x22\x30\x38\x23\x37\x2B\x23" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AutoPot_Window))
					{
						// Image 1
						UI::HLBackground(ImVec2(13, 41), 35, 35);

						ImGui::SetCursorPos(ImVec2(16, 44));
						ImGui::Image((void*)Redpot, ImVec2(30, 30));


						//// Sağ F1
						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_PotSlot9, ImVec2(55, 47), 23, 23, /*Inventory Slot : (9)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0E\x26\x3F\x2F\x25\x38\x22\x3C\x36\x70\x02\x3E\x3C\x20\x75\x6C\x77\x70\x60\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_PotSlot8, ImVec2(88, 47), 23, 23, /*Inventory Slot : (8)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0E\x26\x3F\x2F\x25\x38\x22\x3C\x36\x70\x02\x3E\x3C\x20\x75\x6C\x77\x70\x61\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_PotSlot7, ImVec2(120, 47), 23, 23, /*Inventory Slot : (7)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0E\x26\x3F\x2F\x25\x38\x22\x3C\x36\x70\x02\x3E\x3C\x20\x75\x6C\x77\x70\x6E\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_PotSlot6, ImVec2(150, 47), 23, 23, /*Inventory Slot : (6)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0E\x26\x3F\x2F\x25\x38\x22\x3C\x36\x70\x02\x3E\x3C\x20\x75\x6C\x77\x70\x6F\x73" + 0x2CAE8000).s);

						// Image 2
						UI::HLBackground(ImVec2(13, 85), 35, 35);

						ImGui::SetCursorPos(ImVec2(16, 88));
						ImGui::Image((void*)Bluepot, ImVec2(30, 30));

						//// Sağ Slot
						int Y = 90;
						UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_PotSlot5, ImVec2(55, Y), 23, 23, /*"Inventory Slot : (Slot 5)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x65\x01\x27\x3C\x2E\x22\x39\x21\x3D\x29\x71\x01\x3F\x3B\x21\x76\x6D\x78\x71\x09\x37\x33\x29\x7E\x6A\x49" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_PotSlot4, ImVec2(88, Y), 23, 23, /*"Inventory Slot : (Slot 4)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x65\x01\x27\x3C\x2E\x22\x39\x21\x3D\x29\x71\x01\x3F\x3B\x21\x76\x6D\x78\x71\x09\x37\x33\x29\x7E\x6B\x49" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_PotSlot3, ImVec2(120, Y), 23, 23, /*"Inventory Slot : (Slot 3)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x65\x01\x27\x3C\x2E\x22\x39\x21\x3D\x29\x71\x01\x3F\x3B\x21\x76\x6D\x78\x71\x09\x37\x33\x29\x7E\x6C\x49" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_PotSlot2, ImVec2(150, Y), 23, 23, /*"Inventory Slot : (Slot 2)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x65\x01\x27\x3C\x2E\x22\x39\x21\x3D\x29\x71\x01\x3F\x3B\x21\x76\x6D\x78\x71\x09\x37\x33\x29\x7E\x6D\x49" + 0x2CAE8000).s);

						// Orta
						if (UI::SexyButtonV2(/*Auto pot*/XorStrTR<0x47, 9, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x3C\x22\x3A" + 0x2CAE8000).s, ImVec2(55, 130), ImVec2(78, 20), Keyboard::AutoPot, Low))
						{
							Keyboard::AutoPot = !Keyboard::AutoPot;
						}
					}
					ImGui::End();
				}
				// Auto Pickup
				if (Keyboard::AutoPickUp_Window)
				{
					if (WindowCreate::WindowCloser(/*picking*/XorStrTR<0x47, 8, 0x2CAE8000>("\x37\x21\x2A\x21\x22\x22\x2A" + 0x2CAE8000).s,/** Pickup Bot*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x19\x23\x28\x27\x38\x3E\x6F\x12\x3E\x26" + 0x2CAE8000).s, Keyboard::Pickup_GuiSizeX, Keyboard::Pickup_GuiSizeY, Keyboard::Pickup_GuiSizeX + 10, Keyboard::Pickup_GuiSizeY + 10, Windows, &Keyboard::AutoPickUp_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##pick_tab*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x39\x23\x28\x27\x12\x3A\x2E\x32" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/** Pickup*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x19\x23\x28\x27\x38\x3E" + 0x2CAE8000).s))
							{
								Keyboard::Pickup_GuiSizeX = 475;
								Keyboard::Pickup_GuiSizeY = 265;

								// Sol
								float AllTabY = 240;
								if (UI::SexyButtonV2(/*Pickup*/XorStrTR<0x47, 7, 0x2CAE8000>("\x17\x21\x2A\x21\x3E\x3C" + 0x2CAE8000).s, ImVec2(10, AllTabY), ImVec2(78, 20), Keyboard::AutoPickUp, Low))
								{
									Keyboard::AutoPickUp = !Keyboard::AutoPickUp;
								}
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_AutoLootV1, ImVec2(98, AllTabY), 19, 19, /*Auto Loot (V1)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x21\x3B\x70\x79\x04\x62\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_AutoLootV2, ImVec2(125, AllTabY), 19, 19, /*Auto Loot (V2)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x21\x3B\x70\x79\x04\x61\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_AutoLootv3, ImVec2(152, AllTabY), 19, 19, /*Auto Loot v3 (Default)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x21\x3B\x70\x27\x61\x73\x7C\x11\x33\x31\x39\x2C\x36\x2F\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_RangeLoot, ImVec2(179, AllTabY), 19, 19, /*Pickup Range (Ultimate)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x17\x21\x2A\x21\x3E\x3C\x6D\x1C\x2E\x3E\x36\x37\x73\x7C\x00\x3A\x23\x31\x34\x3B\x2F\x39\x74" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_CollectLoot, ImVec2(179+27, AllTabY), 19, 19, /*Collect the selected*/XorStrTR<0x47, 21, 0x2CAE8000>("\x04\x27\x25\x26\x2E\x2F\x39\x6E\x3B\x38\x34\x72\x20\x31\x39\x33\x34\x2C\x3C\x3E" + 0x2CAE8000).s);

								UI::HLTextBox(/*##-1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7B" + 0x2CAE8000).s, ImVec2(16, 60), 386, 20, FındCollect, sizeof(FındCollect), Low, /*Search*/XorStrTR<0x47, 7, 0x2CAE8000>("\x14\x2D\x28\x38\x28\x24" + 0x2CAE8000).s);

								if (UI::SexyButtonV2(/*Refresh*/XorStrTR<0x47, 8, 0x2CAE8000>("\x15\x2D\x2F\x38\x2E\x3F\x25" + 0x2CAE8000).s, ImVec2(405, 60), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									LoadItemNames();
									FilterItems(FındCollect);
									g_SelectedItemsList.clear();
									g_SelectedItemsPtrs.clear();
									g_SelectedItemIDs.clear(); 
									selectedRight = 0;
								}

								if (!itemsLoaded) {
									LoadItemNames();
									FilterItems("");
									itemsLoaded = true;
								}

								if (strcmp(lastSearch, FındCollect) != 0) {
									FilterItems(FındCollect);
									strcpy_s(lastSearch, FındCollect);
									selected = 0;
								}

								if (!g_FilteredItemPtrs.empty()) {
									UI::HLListBox(/*##ItemList*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x00\x3E\x2E\x21\x01\x27\x3C\x24" + 0x2CAE8000).s, ImVec2(10, 85), 231, 150, g_FilteredItemPtrs.data(), (int)g_FilteredItemPtrs.size(), &selected, Low, nullptr);
									if (selected >= 0 && selected < g_FilteredItemList.size()) {
									}
								}

								UI::HLListBox(/*##SelectedItemList*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x1A\x2F\x27\x29\x2E\x3A\x2A\x34\x18\x26\x36\x39\x19\x3F\x24\x2C" + 0x2CAE8000).s, ImVec2(240, 85), 231, 150,
									g_SelectedItemsPtrs.empty() ? nullptr : g_SelectedItemsPtrs.data(),
									(int)g_SelectedItemsPtrs.size(), &selectedRight, Low, nullptr);

								if (UI::SexyButtonV2(/*Add >*/XorStrTR<0x47, 6, 0x2CAE8000>("\x06\x2C\x2D\x6A\x75" + 0x2CAE8000).s, ImVec2(245, AllTabY), ImVec2(100, 20), false, Low))
								{
									if (selected >= 0 && selected < g_FilteredItemList.size()) {
										std::string selectedItem = g_FilteredItemList[selected];
										if (std::find(g_SelectedItemsList.begin(), g_SelectedItemsList.end(), selectedItem) == g_SelectedItemsList.end()) {
											g_SelectedItemsList.push_back(selectedItem);

											size_t start = selectedItem.find("[");
											size_t end = selectedItem.find("]");
											if (start != std::string::npos && end != std::string::npos) {
												std::string idStr = selectedItem.substr(start + 1, end - start - 1);
												DWORD itemID = std::stoul(idStr);
												g_SelectedItemIDs.push_back(itemID);
											}

											g_SelectedItemsPtrs.clear();
											for (auto& item : g_SelectedItemsList) {
												g_SelectedItemsPtrs.push_back(item.c_str());
											}
										}
									}
								}

								ImGui::SameLine();

								if (UI::SexyButtonV2(/*< Remove*/XorStrTR<0x47, 9, 0x2CAE8000>("\x7B\x68\x1B\x2F\x26\x23\x3B\x2B" + 0x2CAE8000).s, ImVec2(370, AllTabY), ImVec2(100, 20), false, Low))
								{
									if (selectedRight >= 0 && selectedRight < g_SelectedItemsList.size()) {
										g_SelectedItemsList.erase(g_SelectedItemsList.begin() + selectedRight);
										g_SelectedItemIDs.erase(g_SelectedItemIDs.begin() + selectedRight); // ID'yi de sil

										g_SelectedItemsPtrs.clear();
										for (auto& item : g_SelectedItemsList) {
											g_SelectedItemsPtrs.push_back(item.c_str());
										}

										if (selectedRight >= g_SelectedItemsList.size() && selectedRight > 0) {
											selectedRight = g_SelectedItemsList.size() - 1;
										}
									}
								}

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/** Blacksmith*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0B\x26\x2A\x2F\x26\x3D\x22\x39\x25\x3A" + 0x2CAE8000).s))
							{
								Keyboard::Pickup_GuiSizeX = 235;
								Keyboard::Pickup_GuiSizeY = 110;
								// Sol
								if (UI::SexyButtonV2(/*Blacksmith*/XorStrTR<0x47, 11, 0x2CAE8000>("\x05\x24\x28\x29\x20\x3F\x20\x27\x3B\x38" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::C_Blacksmith, Low))
								{
									Keyboard::C_Blacksmith = !Keyboard::C_Blacksmith;
								}
								UI::HLNumberBoxInt(/*##upd*/XorStrTR<0x47, 6, 0x2CAE8000>("\x64\x6B\x3C\x3A\x2F" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_BlacksmithSpeed, Low,/*Blacksmith Speed (Updrage)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x05\x24\x28\x29\x20\x3F\x20\x27\x3B\x38\x71\x01\x23\x31\x30\x32\x77\x70\x0C\x2A\x3F\x2E\x3C\x39\x3A\x49" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_CheckAll, ImVec2(135, 60), 19, 19, /** Check All*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x0A\x22\x2E\x2F\x26\x6E\x0E\x3C\x3D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot2, ImVec2(160, 60), 19, 19, /** Update Item (Slot 2)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x69\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot3, ImVec2(185, 60), 19, 19, /** Update Item (Slot 3)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x68\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot4, ImVec2(210, 60), 19, 19, /** Update Item (Slot 4)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6F\x75" + 0x2CAE8000).s);
								// Alt Bord
								int AltBord = 25;
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot5, ImVec2(10, 85), 19, 19,/** Update Item (Slot 5)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6E\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot6, ImVec2(35, 85), 19, 19, /** Update Item (Slot 6)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6D\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot7, ImVec2(60, 85), 19, 19, /** Update Item (Slot 7)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6C\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot8, ImVec2(85, 85), 19, 19, /** Update Item (Slot 8)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x63\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##9*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x70" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot9, ImVec2(110, 85), 19, 19, /** Update Item (Slot 9)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x62\x75" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##10*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x7A" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot10, ImVec2(135, 85), 19, 19,/** Update Item (Slot 10)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6A\x6C\x74" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##11*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x7B" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot11, ImVec2(160, 85), 19, 19, /** Update Item (Slot 11)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6A\x6D\x74" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##12*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x78" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot12, ImVec2(185, 85), 19, 19, /** Update Item (Slot 12)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6A\x6E\x74" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##13*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x79" + 0x2CAE8000).s, &Keyboard::C_Blacksmith_Slot13, ImVec2(210, 85), 19, 19, /** Update Item (Slot 13)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x6D\x68\x1C\x3A\x2F\x2D\x39\x2B\x6F\x19\x25\x37\x3E\x74\x7D\x05\x3B\x37\x2D\x7A\x6A\x6F\x74" + 0x2CAE8000).s);
								static bool lastCheckAll = false;
								if (Keyboard::C_Blacksmith_CheckAll != lastCheckAll)
								{
									lastCheckAll = Keyboard::C_Blacksmith_CheckAll;

									bool* slots[] = {
										&Keyboard::C_Blacksmith_Slot2,  &Keyboard::C_Blacksmith_Slot3,  &Keyboard::C_Blacksmith_Slot4,
										&Keyboard::C_Blacksmith_Slot5,  &Keyboard::C_Blacksmith_Slot6,  &Keyboard::C_Blacksmith_Slot7,
										&Keyboard::C_Blacksmith_Slot8,  &Keyboard::C_Blacksmith_Slot9,  &Keyboard::C_Blacksmith_Slot10,
										&Keyboard::C_Blacksmith_Slot11, &Keyboard::C_Blacksmith_Slot12, &Keyboard::C_Blacksmith_Slot13,
										&Keyboard::C_Blacksmith_Slot14
									};

									for (auto slot : slots)
										*slot = Keyboard::C_Blacksmith_CheckAll; 
								}


	
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Inventory*/XorStrTR<0x47, 10, 0x2CAE8000>("\x0E\x26\x3F\x2F\x25\x38\x22\x3C\x36" + 0x2CAE8000).s))
							{
								Keyboard::Pickup_GuiSizeX = 390;
								Keyboard::Pickup_GuiSizeY = 240;
								if (UI::SexyButtonV2(/*Refresh*/XorStrTR<0x47, 8, 0x2CAE8000>("\x15\x2D\x2F\x38\x2E\x3F\x25" + 0x2CAE8000).s, ImVec2(175, 60), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									LoadItemNames();
									LoadInventory();
									FilterInventory(FindInventory);
									selected = 0;
									inventoryLoaded = true;
								}

								UI::HLTextBox(/*##-1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7B" + 0x2CAE8000).s, ImVec2(16, 60), 160 - 6, 20, FindInventory, sizeof(FindInventory), Low, /*Search*/XorStrTR<0x47, 7, 0x2CAE8000>("\x14\x2D\x28\x38\x28\x24" + 0x2CAE8000).s);

								if (!inventoryLoaded) {
									LoadItemNames();
									LoadInventory();
									FilterInventory("");
									inventoryLoaded = true;
								}

								if (strcmp(lastSearch, FindInventory) != 0) {
									FilterInventory(FindInventory);
									strcpy_s(lastSearch, FindInventory);
									selected = 0;
								}

								if (!g_InventoryItemPtrs.empty()) {
									UI::HLListBox(/*##InventoryList*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x00\x24\x3D\x29\x23\x3A\x20\x22\x28\x1E\x3A\x27\x21" + 0x2CAE8000).s, ImVec2(10, 85), 236 - 5, 150,
										g_InventoryItemPtrs.data(), (int)g_InventoryItemPtrs.size(),
										&selected, Low, nullptr);
								}

								if (UI::SexyButtonV2("Sell", ImVec2(244, 85), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									if (selected >= 0 && selected < (int)g_FilteredIndices.size())
									{
										int inventoryIndex = g_FilteredIndices[selected];
										int slot = g_Inventory[inventoryIndex].slot;
										DWORD vnum = g_Inventory[inventoryIndex].vnum;
										auto it = g_ItemNames.find(vnum);
										const char* itemName = (it != g_ItemNames.end()) ? it->second.c_str() : /*(Null)*/XorStrTR<0x47, 7, 0x2CAE8000>("\x6F\x06\x3C\x26\x27\x65" + 0x2CAE8000).s;
										engine::NetworkStreamSendShopSellPacket(slot, Keyboard::C_SellSloot);
										Sleep(300);
										LoadInventory();
										FilterInventory(FindInventory);
										if (selected >= (int)g_FilteredIndices.size()) selected = 0;
									}
									else
									{
									}
								}

								UI::HLNumberBoxInt("##3", ImVec2(313, 85), 73, 20, &Keyboard::C_SellSloot, Low, "Count");

								// Drop
								if (UI::SexyButtonV2("Drop", ImVec2(244, 60), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt("##2", ImVec2(313, 60), 73, 20, &Keyboard::BoostSpeed, Low, "Drop");
								// Use
								if (UI::SexyButtonV2("Use", ImVec2(244, 85+25), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt("##4", ImVec2(313, 85+25), 73, 20, &Keyboard::BoostSpeed, Low, "Use");
								// Böl
								if (UI::SexyButtonV2("Plenty", ImVec2(244, 85 + 25 + 25), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt("##5", ImVec2(313, 85 + 25 + 25), 73, 20, &Keyboard::BoostSpeed, Low, "Plenty ");
								// All
								if (UI::SexyButtonV2("All", ImVec2(244, 85 + 25 + 25 + 25), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt("##6", ImVec2(313, 85 + 25 + 25 + 25), 73, 20, &Keyboard::BoostSpeed, Low, "All");
								// Update + 
								UI::HLCheckBox("##8", &Keyboard::C_Blacksmith_Slot5, ImVec2(255, 85 + 25 + 25 + 25 + 25), 19, 19,"Update Item Slot (1)");
								UI::HLCheckBox("##9", &Keyboard::C_Blacksmith_Slot5, ImVec2(255 + 24, 85 + 25 + 25 + 25 + 25), 19, 19, "Update Item Slot (2)");
								UI::HLCheckBox("##10", &Keyboard::C_Blacksmith_Slot5, ImVec2(255 + 24+24, 85 + 25 + 25 + 25 + 25), 19, 19, "Update Item Slot (3)");
								UI::HLCheckBox("##11", &Keyboard::C_Blacksmith_Slot5, ImVec2(255 + 24 + 24+24, 85 + 25 + 25 + 25 + 25), 19, 19, "Update Item Slot (4)");
								UI::HLCheckBox("##12", &Keyboard::C_Blacksmith_Slot5, ImVec2(255 + 24 + 24 + 24+24, 85 + 25 + 25 + 25 + 25), 19, 19, "Update Item Slot (5)");
								// A+
								if (UI::SexyButtonV2("+", ImVec2(255 + 24 , + 25 + 25 + 160), ImVec2(65, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
							}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				//if (Keyboard::AutoPickUp_Window)
				//{
				//	int X = 150;
				//	int Y = 64;
				//	if (WindowCreate::WindowCloser(/*##Apathe_Autopickups*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x24\x26\x3C\x24\x3C\x35\x3C\x2D\x29\x29" + 0x2CAE8000).s, /** Pickup (Beta)*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x19\x23\x28\x27\x38\x3E\x6F\x78\x13\x37\x27\x35\x7C" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AutoPickUp_Window))
				//	{
				//		// Sol
				//		if (UI::SexyButtonV2(/*Auto pickup*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x3C\x24\x2D\x24\x25\x21" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::AutoAttacks, Low))
				//		{
				//			Keyboard::AutoPickUp = !Keyboard::AutoPickUp;
				//		}
				//		UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_AutoLootV1, ImVec2(98, 40), 19, 19, /*Auto Loot (V1)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x21\x3B\x70\x79\x04\x62\x7D" + 0x2CAE8000).s);
				//		UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_AutoLootV2, ImVec2(125, 40), 19, 19, /*Auto Loot (V2)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x00\x22\x21\x3B\x70\x79\x04\x61\x7D" + 0x2CAE8000).s);
				//	}
				//	ImGui::End();
				//}
				// KnockBack
				if (Keyboard::KnockBack_Window)
				{
					int X = 190;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Apathe_KnockBack*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x1B\x3F\x3D\x30\x3F\x17\x37\x34\x33" + 0x2CAE8000).s, /** KnockBack*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x02\x24\x24\x2F\x26\x0C\x2E\x33\x3A" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::KnockBack_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*KnockBack*/XorStrTR<0x47, 10, 0x2CAE8000>("\x0C\x26\x26\x29\x20\x0E\x2C\x2D\x24" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::KnockBack, Low))
						{
							Keyboard::KnockBack = !Keyboard::KnockBack;
						}
						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_KnockBack_Entity, ImVec2(98, 40), 19, 19, /*KnockBack : (Entity)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0C\x26\x26\x29\x20\x0E\x2C\x2D\x24\x70\x6B\x72\x7B\x11\x3B\x22\x3E\x2C\x20\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_KnockBack_Player, ImVec2(125, 40), 19, 19, /*KnockBack : (Player)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0C\x26\x26\x29\x20\x0E\x2C\x2D\x24\x70\x6B\x72\x7B\x04\x39\x37\x2E\x3D\x2B\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_KnockBack_Stone, ImVec2(152, 40), 19, 19, /*KnockBack : (Stone)*/XorStrTR<0x47, 20, 0x2CAE8000>("\x0C\x26\x26\x29\x20\x0E\x2C\x2D\x24\x70\x6B\x72\x7B\x07\x21\x39\x39\x3D\x70" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Collision
				if (Keyboard::Collision_Window)
				{
					int X = 210;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Apathe_Collision*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x13\x3E\x3E\x3F\x3D\x26\x3F\x38\x36" + 0x2CAE8000).s, /** Collision*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x0A\x25\x27\x20\x24\x3D\x26\x3F\x3F" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Collision_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Collision*/XorStrTR<0x47, 10, 0x2CAE8000>("\x04\x27\x25\x26\x22\x3F\x24\x21\x21" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::Collision, Low))
						{
							Keyboard::Collision = !Keyboard::Collision;
						}
						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_Collision_NoClip, ImVec2(98, 40), 19, 19, /*NoClip (Everyone)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x09\x27\x0A\x26\x22\x3C\x6D\x66\x0A\x26\x34\x20\x2A\x3B\x3B\x33\x7E" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_Collision_NPC, ImVec2(125, 40), 19, 19, /*Collision : (Entity)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x04\x27\x25\x26\x22\x3F\x24\x21\x21\x70\x6B\x72\x7B\x11\x3B\x22\x3E\x2C\x20\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_Collision_Player, ImVec2(152, 40), 19, 19, /*Collision : (Player)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x04\x27\x25\x26\x22\x3F\x24\x21\x21\x70\x6B\x72\x7B\x04\x39\x37\x2E\x3D\x2B\x73" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_Collision_Stone, ImVec2(152 + 27, 40), 19, 19, /*Collision : (Stone)*/XorStrTR<0x47, 20, 0x2CAE8000>("\x04\x27\x25\x26\x22\x3F\x24\x21\x21\x70\x6B\x72\x7B\x07\x21\x39\x39\x3D\x70" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Boost
				if (Keyboard::Boost_Window)
				{
					int X = 230;
					int Y = 65;
					if (WindowCreate::WindowCloser(/*##Apathe_Boost*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x12\x3E\x3D\x20\x20" + 0x2CAE8000).s, /** Boost*/XorStrTR<0x47, 8, 0x2CAE8000>("\x6D\x68\x0B\x25\x24\x3F\x39" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Boost_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Boost*/XorStrTR<0x47, 6, 0x2CAE8000>("\x05\x27\x26\x39\x3F" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::Boost, Low))
						{
							Keyboard::Boost = !Keyboard::Boost;
						}

						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_BoostV1, ImVec2(178, 40), 19, 19, /*Boost : (V1)*/XorStrTR<0x47, 13, 0x2CAE8000>("\x05\x27\x26\x39\x3F\x6C\x77\x6E\x67\x06\x60\x7B" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_BoostV2, ImVec2(205, 40), 19, 19, /*Boost : (V2 Hold down Shift)*/XorStrTR<0x47, 29, 0x2CAE8000>("\x05\x27\x26\x39\x3F\x6C\x77\x6E\x67\x06\x63\x72\x1B\x3B\x39\x32\x77\x3C\x36\x2D\x35\x7C\x0E\x36\x36\x06\x15\x4B" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, ImVec2(96, 40), 35, 20, &Keyboard::BoostSpeed, Low, /*Boost Speed*/XorStrTR<0x47, 12, 0x2CAE8000>("\x05\x27\x26\x39\x3F\x6C\x1E\x3E\x2A\x35\x35" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, ImVec2(136, 40), 35, 20, &Keyboard::BoostStep, Low, /*Boost Step*/XorStrTR<0x47, 11, 0x2CAE8000>("\x05\x27\x26\x39\x3F\x6C\x1E\x3A\x2A\x20" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Name Changer
				if (Keyboard::Name_Window)
				{
					if (WindowCreate::WindowCloser(/*##Apathe_Name*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x1E\x30\x3F\x36" + 0x2CAE8000).s, /** Name - Whisper*/XorStrTR<0x47, 17, 0x2CAE8000>("\x6D\x68\x07\x2B\x26\x29\x6D\x63\x6F\x07\x39\x3B\x20\x24\x30\x24" + 0x2CAE8000).s, Keyboard::Name_GuiSizeX, Keyboard::Name_GuiSizeY, Keyboard::Name_GuiSizeX + 10, Keyboard::Name_GuiSizeY + 10, Windows, &Keyboard::Name_Window))
					{
						//// Sol
						//if (UI::SexyButtonV2("Name", ImVec2(10, 40), ImVec2(78, 20), Keyboard::Name, Low))
						//{
						//	Keyboard::Name = !Keyboard::Name;
						//}
						//UI::HLCheckBox("##NameChanger", &Keyboard::C_NameChanger, ImVec2(100, 40), 19, 19, "Name Changer : (used when recording video)");

						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##chat_tab*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x2A\x22\x2A\x38\x12\x3A\x2E\x32" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Global*/XorStrTR<0x47, 7, 0x2CAE8000>("\x00\x24\x26\x28\x2A\x20" + 0x2CAE8000).s))
							{
								Keyboard::Name_GuiSizeX = 165;

								if (UI::SexyButtonV2(/*Name*/XorStrTR<0x47, 5, 0x2CAE8000>("\x09\x29\x24\x2F" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Name, Low))
								{
									Keyboard::Name = !Keyboard::Name;
								}
								UI::HLCheckBox(/*##0*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x79" + 0x2CAE8000).s, &Keyboard::C_NameChanger, ImVec2(95, 60), 19, 19, /*Name Changer : (used when recording video - Apathe)*/XorStrTR<0x47, 52, 0x2CAE8000>("\x09\x29\x24\x2F\x6B\x0F\x25\x2F\x21\x37\x34\x20\x73\x6E\x75\x7E\x22\x2B\x3C\x3E\x7B\x2B\x35\x3B\x31\x40\x13\x07\x00\x0B\x17\x02\x0E\x06\x0E\x4A\x1D\x05\x09\x0B\x00\x50\x5C\x52\x32\x04\x14\x02\x1F\x1D\x50" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_NameApathe, ImVec2(120, 60), 19, 19, /*Changes your Character's Name To Apathe*/XorStrTR<0x47, 40, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x3E\x6E\x36\x3F\x24\x20\x73\x17\x3D\x37\x25\x39\x3A\x2E\x3E\x2E\x7A\x2D\x7F\x2E\x00\x0F\x06\x44\x31\x09\x47\x29\x19\x0B\x1F\x04\x08" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s,&Keyboard::C_Lalaker1, ImVec2(145, 60), 19, 19, /*Changes your Character's Name To Lalaker1*/XorStrTR<0x47, 42, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x3E\x6E\x36\x3F\x24\x20\x73\x17\x3D\x37\x25\x39\x3A\x2E\x3E\x2E\x7A\x2D\x7F\x2E\x00\x0F\x06\x44\x31\x09\x47\x24\x08\x06\x0A\x07\x08\x1C\x5E" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Whisper*/XorStrTR<0x47, 8, 0x2CAE8000>("\x10\x20\x20\x39\x3B\x29\x3F" + 0x2CAE8000).s))
							{
								Keyboard::Name_GuiSizeX = 295;

								if (UI::SexyButtonV2(/*Name*/XorStrTR<0x47, 5, 0x2CAE8000>("\x09\x29\x24\x2F" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Name, Low))
								{
									Keyboard::Name = !Keyboard::Name;
								}
								UI::HLTextBox(/*##-1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7B" + 0x2CAE8000).s, ImVec2(95, 60), 60, 20, C_SetPlayerName, sizeof(C_SetPlayerName), Low, /*Write The Player's Name*/XorStrTR<0x47, 24, 0x2CAE8000>("\x10\x3A\x20\x3E\x2E\x6C\x19\x26\x2A\x70\x01\x3E\x32\x2D\x30\x24\x70\x2B\x79\x14\x3A\x31\x38" + 0x2CAE8000).s);
								UI::HLTextBox(/*##-2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x78" + 0x2CAE8000).s, ImVec2(160, 60), 60, 20, C_SetPlayerMessage, sizeof(C_SetPlayerMessage), Low, /*The Message You Want To Write*/XorStrTR<0x47, 30, 0x2CAE8000>("\x13\x20\x2C\x6A\x06\x29\x3E\x3D\x2E\x37\x34\x72\x0A\x3B\x20\x76\x00\x39\x37\x2E\x7B\x08\x32\x7E\x08\x12\x08\x16\x06" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##-3*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x79" + 0x2CAE8000).s, &Keyboard::C_WhisperSpawm, ImVec2(225, 60), 19, 19, /*Spawm Bot*/XorStrTR<0x47, 10, 0x2CAE8000>("\x14\x38\x28\x3D\x26\x6C\x0F\x21\x3B" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##-4*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7E" + 0x2CAE8000).s, &Keyboard::C_WhisperEveryoneSpawm, ImVec2(250, 60), 19, 19, /*Sends The Message You Wrote To Everyone*/XorStrTR<0x47, 40, 0x2CAE8000>("\x14\x2D\x27\x2E\x38\x6C\x19\x26\x2A\x70\x1C\x37\x20\x27\x34\x31\x32\x78\x00\x35\x2E\x7C\x0A\x2C\x30\x14\x04\x42\x37\x0B\x45\x23\x11\x0D\x1B\x13\x04\x02\x08" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##-5*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7F" + 0x2CAE8000).s, &Keyboard::C_WhisperArea, ImVec2(250+25, 60), 19, 19, /*Sends a Message To Anyone Entering The Area*/XorStrTR<0x47, 44, 0x2CAE8000>("\x14\x2D\x27\x2E\x38\x6C\x2C\x6E\x02\x35\x22\x21\x32\x33\x30\x76\x03\x37\x79\x1B\x35\x25\x32\x30\x3A\x40\x24\x0C\x17\x01\x17\x0F\x09\x0F\x49\x3E\x03\x09\x4D\x2F\x1D\x15\x10" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3D" + 0x2CAE8000).s))
							{
								Keyboard::Name_GuiSizeX = 200;

								if (UI::SexyButtonV2(/*Name*/XorStrTR<0x47, 5, 0x2CAE8000>("\x09\x29\x24\x2F" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Name, Low))
								{
									Keyboard::Name = !Keyboard::Name;
								}
								UI::HLNumberBoxInt(/*##+1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x62\x7B" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_WhisperSpeed, Low, /*You set the spawn speed*/XorStrTR<0x47, 24, 0x2CAE8000>("\x1E\x27\x3C\x6A\x38\x29\x39\x6E\x3B\x38\x34\x72\x20\x24\x34\x21\x39\x78\x2A\x2A\x3E\x39\x39" + 0x2CAE8000).s);
								UI::HLNumberBoxFloat(/*##+2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x62\x78" + 0x2CAE8000).s, ImVec2(135, 60), 35, 20, &Keyboard::C_WhisperDistance, Low, /** You Determine the Spawm Area*/XorStrTR<0x47, 31, 0x2CAE8000>("\x6D\x68\x10\x25\x3E\x6C\x09\x2B\x3B\x35\x23\x3F\x3A\x3A\x30\x76\x23\x30\x3C\x7A\x08\x2C\x3C\x29\x32\x40\x20\x10\x06\x05" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// ============================================= Bot ========================================== //
				// CH Changer
				if (Keyboard::AutoCH_Window)
				{
					if (WindowCreate::WindowCloser(/*##auto_ch*/XorStrTR<0x47, 10, 0x2CAE8000>("\x64\x6B\x28\x3F\x3F\x23\x12\x2D\x27" + 0x2CAE8000).s, /** CH Changer*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0A\x02\x6B\x0F\x25\x2F\x21\x37\x34\x20" + 0x2CAE8000).s, Keyboard::AutoCH_GuiSizeX, Keyboard::AutoCH_GuiSizeY, Keyboard::AutoCH_GuiSizeX + 10, Keyboard::AutoCH_GuiSizeY + 10, Windows, &Keyboard::AutoCH_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::auto::tabs*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x2C\x3B\x3B\x3F\x6B\x68\x27\x35\x37\x25" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*CH Changer*/XorStrTR<0x47, 11, 0x2CAE8000>("\x04\x00\x69\x09\x23\x2D\x23\x29\x2A\x22" + 0x2CAE8000).s))
							{
								Keyboard::AutoCH_GuiSizeX = 250;
								// Sol
								if (UI::SexyButtonV2(/*CH*/XorStrTR<0x47, 3, 0x2CAE8000>("\x04\x00" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::AutoCH, Low))
								{
									Keyboard::AutoCH = !Keyboard::AutoCH;
								}
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::Auto_CH1, ImVec2(95, 60), 19, 19, /*Change Ch (1)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x63\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::Auto_CH2, ImVec2(120, 60), 19, 19, /*Change Ch (2)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x60\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::Auto_CH3, ImVec2(120+25, 60), 19, 19, /*Change Ch (3)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x61\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::Auto_CH4, ImVec2(120 + 25+25, 60), 19, 19, /*Change Ch (4)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x66\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::Auto_CH5, ImVec2(120 + 25 + 25+25, 60), 19, 19, /*Change Ch (5)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x67\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::Auto_CH6, ImVec2(120 + 25 + 25+25+25, 60), 19, 19, /*Change Ch (6)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x04\x20\x28\x24\x2C\x29\x6D\x0D\x27\x70\x79\x64\x7A" + 0x2CAE8000).s);

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Auto CH*/XorStrTR<0x47, 8, 0x2CAE8000>("\x06\x3D\x3D\x25\x6B\x0F\x05" + 0x2CAE8000).s))
							{
								Keyboard::AutoCH_GuiSizeX = 190;
								// Sol
								if (UI::SexyButtonV2(/*CH*/XorStrTR<0x47, 3, 0x2CAE8000>("\x04\x00" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::AutoCH, Low))
								{
									Keyboard::AutoCH = !Keyboard::AutoCH;
								}
								UI::HLNumberBoxInt(/*##cs_time*/XorStrTR<0x47, 10, 0x2CAE8000>("\x64\x6B\x2A\x39\x14\x38\x24\x23\x2A" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::AutoCH_Time, Low, /*Ch Changes Respectively (Speed)*/XorStrTR<0x47, 32, 0x2CAE8000>("\x04\x20\x69\x09\x23\x2D\x23\x29\x2A\x23\x71\x00\x36\x27\x25\x33\x34\x2C\x30\x2C\x3E\x30\x24\x7E\x77\x33\x11\x07\x06\x00\x4C" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::AutoCH_Order, ImVec2(138, 60), 19, 19, /*Order*/XorStrTR<0x47, 6, 0x2CAE8000>("\x08\x3A\x2D\x2F\x39" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::AutoCH_Random, ImVec2(138+27, 60), 19, 19, /*Random (Changer)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x15\x29\x27\x2E\x24\x21\x6D\x66\x0C\x38\x30\x3C\x34\x31\x27\x7F" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}
							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// ============================================= Bot ========================================== //
				// Farmbot
				if (Keyboard::Bot_Winddow)
				{
					if (WindowCreate::WindowCloser(/*##farm_bot*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x2F\x2B\x39\x21\x12\x2C\x20\x24" + 0x2CAE8000).s, /** Automatic*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x20\x2F\x3B\x39\x32" + 0x2CAE8000).s, Keyboard::Bot_GuiSizeX, Keyboard::Bot_GuiSizeY, Keyboard::Bot_GuiSizeX + 10, Keyboard::Bot_GuiSizeY + 10, Windows, &Keyboard::Bot_Winddow))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::auto::tabs*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x2C\x3B\x3B\x3F\x6B\x68\x27\x35\x37\x25" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Teleport*/XorStrTR<0x47, 9, 0x2CAE8000>("\x13\x2D\x25\x2F\x3B\x23\x3F\x3A" + 0x2CAE8000).s))
							{
								Keyboard::Bot_GuiSizeX = 230;
								// Sol
								if (UI::SexyButtonV2(/*Bot*/XorStrTR<0x47, 4, 0x2CAE8000>("\x05\x27\x3D" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_LegitRange, ImVec2(95, 60), 19, 19, /** Legit Range*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x05\x2F\x2C\x25\x39\x6E\x1D\x31\x3F\x35\x36" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_BotTeleport, ImVec2(120, 60), 19, 19, /** Teleport*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x1D\x2F\x27\x29\x3D\x21\x3D\x24" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_StateRange, ImVec2(120+25, 60), 19, 19, /** State Range*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x1A\x3E\x2A\x38\x28\x6E\x1D\x31\x3F\x35\x36" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_KillAura, ImVec2(120 + 25+25, 60), 19, 19, /** Kill Aura*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x02\x23\x27\x20\x6D\x0F\x3A\x22\x30" + 0x2CAE8000).s);

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Walk*/XorStrTR<0x47, 5, 0x2CAE8000>("\x10\x29\x25\x21" + 0x2CAE8000).s))
							{
								Keyboard::Bot_GuiSizeX = 230;
								// Sol
								if (UI::SexyButtonV2(/*Bot*/XorStrTR<0x47, 4, 0x2CAE8000>("\x05\x27\x3D" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt(/*##walk_Speedsss*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x3E\x2B\x27\x27\x12\x1D\x3F\x35\x34\x36\x20\x27\x26" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_UltimateSpeed, Low, /*Ultimate Speed*/XorStrTR<0x47, 15, 0x2CAE8000>("\x12\x24\x3D\x23\x26\x2D\x39\x2B\x6F\x03\x21\x37\x36\x30" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_WalkToFarm, ImVec2(135, 60), 19, 19, /*Walk To (Cut)*/XorStrTR<0x47, 14, 0x2CAE8000>("\x10\x29\x25\x21\x6B\x18\x22\x6E\x67\x13\x24\x26\x7A" + 0x2CAE8000).s);
								UI::HLCheckBox(/** Ultimate*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x1C\x26\x3F\x25\x20\x2F\x3B\x35" + 0x2CAE8000).s, &Keyboard::C_UltimateFarm, ImVec2(160, 60), 19, 19, /*Ultimate (Walk)*/XorStrTR<0x47, 16, 0x2CAE8000>("\x12\x24\x3D\x23\x26\x2D\x39\x2B\x6F\x78\x06\x33\x3F\x3F\x7C" + 0x2CAE8000).s);
								UI::HLCheckBox(/** Route*/XorStrTR<0x47, 8, 0x2CAE8000>("\x6D\x68\x1B\x25\x3E\x38\x28" + 0x2CAE8000).s, &Keyboard::C_UltimateRoute, ImVec2(185, 60), 19, 19, /*Use by Route*/XorStrTR<0x47, 13, 0x2CAE8000>("\x12\x3B\x2C\x6A\x29\x35\x6D\x1C\x20\x25\x25\x37" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3D" + 0x2CAE8000).s))
							{
								Keyboard::Bot_GuiSizeX = 325;
								if (UI::SexyButtonV2(/*Bot*/XorStrTR<0x47, 4, 0x2CAE8000>("\x05\x27\x3D" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(30, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLNumberBoxInt(/*##-1*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x7B" + 0x2CAE8000).s, ImVec2(45, 60), 35, 20, &Keyboard::BOT_SPEED, Low, /*Set All Speed (Changer)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x14\x2D\x3D\x6A\x0A\x20\x21\x6E\x1C\x20\x34\x37\x37\x74\x7D\x15\x3F\x39\x37\x3D\x3E\x2E\x74" + 0x2CAE8000).s);
								UI::HLNumberBoxInt(/*##-2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x78" + 0x2CAE8000).s, ImVec2(50+40-5, 60), 35, 20, &Keyboard::BOT_DISTANCE, Low, /*Set All Distance (Changer)*/XorStrTR<0x47, 27, 0x2CAE8000>("\x14\x2D\x3D\x6A\x0A\x20\x21\x6E\x0B\x39\x22\x26\x32\x3A\x36\x33\x77\x70\x1A\x32\x3A\x32\x3A\x3B\x2D\x49" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::Bot_Npc, ImVec2(125 , 60), 19, 19, /*Type : (NPC)*/XorStrTR<0x47, 13, 0x2CAE8000>("\x13\x31\x39\x2F\x6B\x76\x6D\x66\x01\x00\x12\x7B" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::Bot_Stone, ImVec2(130 + 20, 60), 19, 19, /*Type : (Stone)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x13\x31\x39\x2F\x6B\x76\x6D\x66\x1C\x24\x3E\x3C\x36\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::Bot_Mineral, ImVec2(130+20+25, 60), 19, 19, /*Type : (Mineral)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x13\x31\x39\x2F\x6B\x76\x6D\x66\x02\x39\x3F\x37\x21\x35\x39\x7F" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_NoKs, ImVec2(130 + 20 + 25+25, 60), 19, 19, /*No KS (Player)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x09\x27\x69\x01\x18\x6C\x65\x1E\x23\x31\x28\x37\x21\x7D" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::StoneBan, ImVec2(130 + 20 + 25 + 25+25, 60), 19, 19, /*Stone Ban*/XorStrTR<0x47, 10, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x0F\x2F\x21" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::StoneClear, ImVec2(130 + 20 + 25 + 25 + 25+25, 60), 19, 19, /*Stone Clear (Clears VID List)*/XorStrTR<0x47, 30, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x0E\x22\x2A\x31\x23\x72\x7B\x17\x39\x33\x36\x2A\x2A\x7A\x0D\x15\x19\x7E\x13\x09\x12\x16\x4A" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::Bot_Horse, ImVec2(130 + 20 + 25 + 25 + 25 + 25+25, 60), 19, 19, /*Type : (Horse)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x13\x31\x39\x2F\x6B\x76\x6D\x66\x07\x3F\x23\x21\x36\x7D" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::Bot_Player, ImVec2(130 + 20 + 25 + 25 + 25 + 25 + 25+25, 60), 19, 19, /*Type : (Player)*/XorStrTR<0x47, 16, 0x2CAE8000>("\x13\x31\x39\x2F\x6B\x76\x6D\x66\x1F\x3C\x30\x2B\x36\x26\x7C" + 0x2CAE8000).s);

							   ImGui::EndTabItem();
							}
							if (ImGui::BeginTabItem("Rohan"))
							{
								Keyboard::Bot_GuiSizeX = 200;
								// Sol
								if (UI::SexyButtonV2(/*Bot*/XorStrTR<0x47, 4, 0x2CAE8000>("\x05\x27\x3D" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Bot, Low))
								{
									Keyboard::Bot = !Keyboard::Bot;
								}
								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_AllVadiBan, ImVec2(95, 60), 19, 19, "* Vadi (CH4 Ban)");
								UI::HLCheckBox(/*##-2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x78" + 0x2CAE8000).s, &Keyboard::C_AllSohanBan, ImVec2(120, 60), 19, 19, "* Sohan (CH4 Ban)");
								UI::HLCheckBox(/*##-2*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x64\x78" + 0x2CAE8000).s, &Keyboard::C_AllOrman, ImVec2(120+25, 60), 19, 19, "* Orman (CH4 Ban)");

								ImGui::EndTabItem();
							}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				//// Bot - farm
				//if (Keyboard::Bot_Winddow)
				//{
				//	int X = 310;
				//	int Y = 85;
				//	if (WindowCreate::WindowCloser("##Apathe_Bot", "* FarmBot", X, Y, X + 10, Y + 10, Windows, &Keyboard::Bot_Winddow))
				//	{
				//		// Sol
				//		if (UI::SexyButtonV2("Bot", ImVec2(10, 40), ImVec2(78, 20), Keyboard::Bot, Low))
				//		{
				//			Keyboard::Bot = !Keyboard::Bot;
				//		}

				//		UI::HLNumberBoxInt("##BOT_SPEED", ImVec2(96, 40), 35, 20, &Keyboard::BOT_SPEED, Low, "Set All Speed (Changer)");

				//		UI::HLNumberBoxInt("##BOT_DISTANCE", ImVec2(136, 40), 35, 20, &Keyboard::BOT_DISTANCE, Low, "Set All Distance (Changer)");

				//		UI::HLCheckBox("##1", &Keyboard::Bot_Npc, ImVec2(136 + 43, 40), 19, 19, "Type : (NPC)");
				//		UI::HLCheckBox("##2", &Keyboard::Bot_Stone, ImVec2(136 + 43 + 26, 40), 19, 19, "Type : (Stone)");
				//		UI::HLCheckBox("##3", &Keyboard::Bot_Mineral, ImVec2(136 + 43 + 26 + 26, 40), 19, 19, "Type : (Mineral)");

				//		UI::HLCheckBox("##10", &Keyboard::Bot_Horse, ImVec2(136 + 43 + 26 + 26 + 26, 40), 19, 19, "Type : (Horse)");
				//		UI::HLCheckBox("##11", &Keyboard::Bot_Player, ImVec2(136 + 43 + 26 + 26 + 26+26, 40), 19, 19, "Type : (Player)");



				//		UI::HLCheckBox("##7", &Keyboard::C_RangeWaitLine, ImVec2(35 + 25 + 25, 65), 19, 19, "NoKS");
				//		UI::HLCheckBox("##8", &Keyboard::StoneBan, ImVec2(35 + 25 + 25 + 25, 65), 19, 19, "Stone Ban");
				//		UI::HLCheckBox("##9", &Keyboard::StoneClear, ImVec2(35 + 25 + 25 + 25 + 25, 65), 19, 19, "Stone Clear");
				//	}
				//	ImGui::End();
				//}
				// Skills
				if (Keyboard::Skills_Window)
				{
					int X = 250;
					int Y = 85;
					if (WindowCreate::WindowCloser(/*##Apathe_Skills"*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x03\x3A\x3B\x3F\x38\x26\x74" + 0x2CAE8000).s, /** Skills*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x1A\x21\x22\x20\x21\x3D" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Skills_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Skills*/XorStrTR<0x47, 7, 0x2CAE8000>("\x14\x23\x20\x26\x27\x3F" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::Skills, Low))
						{
							Keyboard::Skills = !Keyboard::Skills;
						}

						UI::HLNumberBoxInt(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(96, 40), 35, 20, &Keyboard::HorseTime, Low, /** Horse Time*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x01\x25\x39\x3F\x28\x6E\x1B\x39\x3C\x37" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, ImVec2(136, 40), 35, 20, &Keyboard::DefaultTime, Low, /** Def Time*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x2F\x2D\x6C\x19\x27\x22\x35" + 0x2CAE8000).s);

						UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_HorseSkill, ImVec2(136 + 43, 40), 19, 19, /*Mode : (Horse)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x0A\x27\x2D\x2F\x6B\x76\x6D\x66\x07\x3F\x23\x21\x36\x7D" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_DefaultSkill, ImVec2(136 + 43 + 26, 40), 19, 19, /*Mode : (Default)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x0A\x27\x2D\x2F\x6B\x76\x6D\x66\x0B\x35\x37\x33\x26\x38\x21\x7F" + 0x2CAE8000).s);

						UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_HorseSkillSlot1, ImVec2(10, 65), 19, 19, /*Horse Skill : (Slot1)*/XorStrTR<0x47, 22, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E\x6C\x1E\x25\x26\x3C\x3D\x72\x69\x74\x7D\x05\x3B\x37\x2D\x6B\x72" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_HorseSkillSlot2, ImVec2(35, 65), 19, 19, /*Horse Skill : (Slot2)*/XorStrTR<0x47, 22, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E\x6C\x1E\x25\x26\x3C\x3D\x72\x69\x74\x7D\x05\x3B\x37\x2D\x68\x72" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_HorseSkillSlot3, ImVec2(35 + 25, 65), 19, 19, /*Horse Skill : (Slot3)*/XorStrTR<0x47, 22, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E\x6C\x1E\x25\x26\x3C\x3D\x72\x69\x74\x7D\x05\x3B\x37\x2D\x69\x72" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_HorseSkillSlot4, ImVec2(35 + 25 + 25, 65), 19, 19, /*Horse Skill : (Slot4*/XorStrTR<0x47, 21, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E\x6C\x1E\x25\x26\x3C\x3D\x72\x69\x74\x7D\x05\x3B\x37\x2D\x6E" + 0x2CAE8000).s);

						UI::HLCheckBox(/*##9*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x70" + 0x2CAE8000).s, &Keyboard::C_DefaultSkillSlot1, ImVec2(35 + 25 + 25 + 25, 65), 19, 19, /*Default Skill : (Slot1)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x03\x2D\x2F\x2B\x3E\x20\x39\x6E\x1C\x3B\x38\x3E\x3F\x74\x6F\x76\x7F\x0B\x35\x35\x2F\x6D\x74" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##10*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x7A" + 0x2CAE8000).s, &Keyboard::C_DefaultSkillSlot2, ImVec2(35 + 25 + 25 + 25 + 25, 65), 19, 19, /*Default Skill : (Slot2)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x03\x2D\x2F\x2B\x3E\x20\x39\x6E\x1C\x3B\x38\x3E\x3F\x74\x6F\x76\x7F\x0B\x35\x35\x2F\x6E\x74" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##11*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x7B" + 0x2CAE8000).s, &Keyboard::C_DefaultSkillSlot3, ImVec2(35 + 25 + 25 + 25 + 25 + 25, 65), 19, 19, /*Default Skill : (Slot3)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x03\x2D\x2F\x2B\x3E\x20\x39\x6E\x1C\x3B\x38\x3E\x3F\x74\x6F\x76\x7F\x0B\x35\x35\x2F\x6F\x74" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##12*/XorStrTR<0x47, 5, 0x2CAE8000>("\x64\x6B\x78\x78" + 0x2CAE8000).s, &Keyboard::C_DefaultSkillSlot4, ImVec2(35 + 25 + 25 + 25 + 25 + 25 + 25, 65), 19, 19, /*Default Skill : (Slot4)*/XorStrTR<0x47, 24, 0x2CAE8000>("\x03\x2D\x2F\x2B\x3E\x20\x39\x6E\x1C\x3B\x38\x3E\x3F\x74\x6F\x76\x7F\x0B\x35\x35\x2F\x68\x74" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Attack Speed
				if (Keyboard::AttackSpeed_Window)
				{
					int X = 198;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Apathe_AttackSpeed*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x25\x26\x32\x37\x3E\x05\x27\x3D\x3C\x3E" + 0x2CAE8000).s, /** AttackSpeed*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x3E\x3F\x2D\x2E\x25\x1C\x20\x34\x37\x37" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AttackSpeed_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*AttackSpeed*/XorStrTR<0x47, 12, 0x2CAE8000>("\x06\x3C\x3D\x2B\x28\x27\x1E\x3E\x2A\x35\x35" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::AttackSpeed, Low))
						{
							Keyboard::AttackSpeed = !Keyboard::AttackSpeed;
						}

						UI::HLNumberBoxFloat(/*##Attack_SpeedValue*/XorStrTR<0x47, 20, 0x2CAE8000>("\x64\x6B\x08\x3E\x3F\x2D\x2E\x25\x10\x03\x21\x37\x36\x30\x03\x37\x3B\x2D\x3C" + 0x2CAE8000).s, ImVec2(96, 40), 35, 20, &Keyboard::C_AttackSpeedValue, Low, /** Attack Speed - Sword Range (Changer)*/XorStrTR<0x47, 39, 0x2CAE8000>("\x6D\x68\x08\x3E\x3F\x2D\x2E\x25\x6F\x03\x21\x37\x36\x30\x75\x7B\x77\x0B\x2E\x35\x29\x38\x7D\x0C\x3E\x0E\x06\x07\x43\x4C\x26\x0E\x06\x06\x0E\x0F\x19\x45" + 0x2CAE8000).s);

						//UI::HLNumberBox("##Attack_Speed", ImVec2(96, 40), 35, 20, &Keyboard::C_AttackSpeedValue, Low);


						UI::HLCheckBox(/*##Attack_Speed*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x08\x3E\x3F\x2D\x2E\x25\x10\x03\x21\x37\x36\x30" + 0x2CAE8000).s, &Keyboard::C_AttackSpeed, ImVec2(100 + 43, 40), 19, 19, /*Attack Speed*/XorStrTR<0x47, 13, 0x2CAE8000>("\x06\x3C\x3D\x2B\x28\x27\x6D\x1D\x3F\x35\x34\x36" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##Sword_Ranges*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x1A\x3D\x24\x3E\x29\x11\x1D\x31\x3F\x35\x36\x27" + 0x2CAE8000).s, &Keyboard::C_SwordRange, ImVec2(130 + 43, 40), 19, 19, /*Sword Range*/XorStrTR<0x47, 12, 0x2CAE8000>("\x14\x3F\x26\x38\x2F\x6C\x1F\x2F\x21\x37\x34" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Movement Speed
				if (Keyboard::MovementSpeed_Window)
				{
					int X = 198 +43;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Apathe_MovementSpeed*/XorStrTR<0x47, 23, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x1D\x3E\x24\x36\x39\x30\x38\x23\x0B\x29\x3F\x3E\x38" + 0x2CAE8000).s, /** Move Speed*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x04\x25\x3D\x29\x6D\x1D\x3F\x35\x34\x36" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::MovementSpeed_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Move Speed*/XorStrTR<0x47, 11, 0x2CAE8000>("\x0A\x27\x3F\x2F\x6B\x1F\x3D\x2B\x2A\x34" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::MovementSpeed, Low))
						{
							Keyboard::MovementSpeed = !Keyboard::MovementSpeed;
						}

						UI::HLNumberBoxFloat(/*##MovementSpeedss*/XorStrTR<0x47, 18, 0x2CAE8000>("\x64\x6B\x04\x25\x3D\x29\x20\x2B\x21\x24\x02\x22\x36\x31\x31\x25\x24" + 0x2CAE8000).s, ImVec2(96, 40), 35, 20, &Keyboard::C_MovementSpeedValue, Low, /** Move Speed (Changer)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x04\x25\x3D\x29\x6D\x1D\x3F\x35\x34\x36\x73\x7C\x16\x3E\x36\x36\x3E\x3F\x29\x75" + 0x2CAE8000).s);
						//UI::HLNumberBox("##MovementSpeedss", ImVec2(96, 40), 35, 20, &Keyboard::C_MovementSpeedValue, Low);


						UI::HLCheckBox(/*##Movement_Speed*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x04\x25\x3D\x29\x20\x2B\x21\x24\x0E\x01\x23\x31\x30\x32" + 0x2CAE8000).s, &Keyboard::C_MovementSpeed, ImVec2(143, 40), 19, 19, /*Movement Speed*/XorStrTR<0x47, 15, 0x2CAE8000>("\x0A\x27\x3F\x2F\x26\x29\x23\x3A\x6F\x03\x21\x37\x36\x30" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##Legit_Speeds*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x05\x2F\x2C\x25\x39\x11\x1C\x20\x34\x37\x37\x27" + 0x2CAE8000).s, &Keyboard::C_LegitSpeed, ImVec2(173, 40), 19, 19, /*Legit Speed*/XorStrTR<0x47, 12, 0x2CAE8000>("\x0B\x2D\x2E\x23\x3F\x6C\x1E\x3E\x2A\x35\x35" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##no_npc*/XorStrTR<0x47, 9, 0x2CAE8000>("\x64\x6B\x27\x25\x14\x22\x3D\x2D" + 0x2CAE8000).s, &Keyboard::C_ModelVector, ImVec2(173 + 30, 40), 19, 19, /*Model - Vector (Passes through npc and trees)*/XorStrTR<0x47, 46, 0x2CAE8000>("\x0A\x27\x2D\x2F\x27\x6C\x60\x6E\x19\x35\x32\x26\x3C\x26\x75\x7E\x07\x39\x2A\x29\x3E\x2F\x7D\x2A\x37\x12\x0E\x17\x04\x0C\x45\x08\x17\x0B\x49\x0B\x05\x08\x4D\x1A\x1D\x15\x14\x01\x5A" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// AntiFlty
				if (Keyboard::AntiFly_Window)
				{
					int X = 198;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Apathe_AntiFly*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x08\x3A\x2A\x38\x25\x2B\x10\x11\x3F\x26\x3A\x12\x39\x2F" + 0x2CAE8000).s, /** AntiFly*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x08\x24\x3F\x25\x0B\x22\x36" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AntiFly_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*AntiFly*/XorStrTR<0x47, 8, 0x2CAE8000>("\x06\x26\x3D\x23\x0D\x20\x34" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::AntiFly, Low))
						{
							Keyboard::AntiFly = !Keyboard::AntiFly;
						}
						UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::AntiFly_Player, ImVec2(100, 40), 19, 19, /*Player (Anti-Fly)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x17\x24\x28\x33\x2E\x3E\x6D\x66\x0E\x3E\x25\x3B\x7E\x12\x39\x2F\x7E" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::AntiFly_Stone, ImVec2(100 + 30, 40), 19, 19, /*Stone (Anti-Fly)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x65\x0F\x21\x24\x38\x7F\x15\x38\x2C\x7F" + 0x2CAE8000).s);
						UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::AntiFly_Npc, ImVec2(100 + 30 + 30, 40), 19, 19, /*Npc (Anti-Fly)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x09\x38\x2A\x6A\x63\x0D\x23\x3A\x26\x7D\x17\x3E\x2A\x7D" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// AutoRevive
				if (Keyboard::AutoRevive_Window)
				{
					int X = 158;
					int Y = 64;
					if (WindowCreate::WindowCloser(/*##Auto_Revive*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x08\x3F\x3F\x23\x12\x1C\x2A\x26\x38\x24\x36" + 0x2CAE8000).s, /** Auto Revive*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1C\x2A\x26\x38\x24\x36" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AutoRevive_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*AutoRevive*/XorStrTR<0x47, 11, 0x2CAE8000>("\x06\x3D\x3D\x25\x19\x29\x3B\x27\x39\x35" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::AutoRevive, Low))
						{
							Keyboard::AutoRevive = !Keyboard::AutoRevive;
						}
						UI::HLNumberBoxInt(/*##AutoRevive_Tim*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x08\x3F\x3F\x23\x1F\x2B\x39\x39\x27\x37\x0C\x00\x3C\x3B" + 0x2CAE8000).s, ImVec2(96, 40), 35, 20, &Keyboard::AutoRevive_Time, Low, /** Revive Time*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x1B\x2F\x3D\x25\x3B\x2B\x6F\x04\x38\x3F\x36" + 0x2CAE8000).s);
						UI::HLCheckBox(/*asd*/XorStrTR<0x47, 4, 0x2CAE8000>("\x26\x3B\x2D" + 0x2CAE8000).s, &Keyboard::AutoReviveGF, ImVec2(96+40, 40), 19, 19, /*GF Auto Revive*/XorStrTR<0x47, 15, 0x2CAE8000>("\x00\x0E\x69\x0B\x3E\x38\x22\x6E\x1D\x35\x27\x3B\x25\x31" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Horse Window
				if (Keyboard::HorseChanger_Window)
				{
					int X = 190;
					int Y = 160;
					if (WindowCreate::WindowCloser(/*##Horse_Changer*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x11\x0C\x38\x30\x3C\x34\x31\x27" + 0x2CAE8000).s, /** Horse*/XorStrTR<0x47, 8, 0x2CAE8000>("\x6D\x68\x01\x25\x39\x3F\x28" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::HorseChanger_Window))
					{
						if (UI::SexyButtonV2(/*Horse*/XorStrTR<0x47, 6, 0x2CAE8000>("\x0F\x27\x3B\x39\x2E" + 0x2CAE8000).s, ImVec2(10, 115 + 25), ImVec2(178, 20), Keyboard::HorseChanger, Low))
						{
							Keyboard::HorseChanger = !Keyboard::HorseChanger;
						}


						UI::HLToggleButton(/*##HorseChanger_Ninja*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x0D\x27\x31\x3F\x35\x36\x26\x0A\x18\x3E\x36\x33\x3B" + 0x2CAE8000).s, &Keyboard::HorseChanger_Ninja, /** Ninja Horse*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x07\x23\x25\x26\x2C\x6E\x07\x3F\x23\x21\x36" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** Girl*/XorStrTR<0x47, 7, 0x2CAE8000>("\x6D\x68\x0E\x23\x39\x20" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##HorseChanger_Sura*/XorStrTR<0x47, 20, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x0D\x27\x31\x3F\x35\x36\x26\x0A\x05\x22\x2A\x38" + 0x2CAE8000).s, &Keyboard::HorseChanger_Sura, /** Sura Horse*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x1A\x3F\x39\x2D\x6D\x06\x20\x22\x22\x37" + 0x2CAE8000).s, ImVec2(10, 65), 178, 19, /** Girl*/XorStrTR<0x47, 7, 0x2CAE8000>("\x6D\x68\x0E\x23\x39\x20" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##HorseChanger_War*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x0D\x27\x31\x3F\x35\x36\x26\x0A\x01\x36\x2A" + 0x2CAE8000).s, &Keyboard::HorseChanger_War, /** Fighter Horse*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x0F\x23\x2C\x24\x39\x2B\x3D\x70\x19\x3D\x21\x27\x30" + 0x2CAE8000).s, ImVec2(10, 90), 178, 19, /** Girl*/XorStrTR<0x47, 7, 0x2CAE8000>("\x6D\x68\x0E\x23\x39\x20" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##HorseChanger_Shaman*/XorStrTR<0x47, 22, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x0D\x27\x31\x3F\x35\x36\x26\x0A\x05\x3F\x39\x34\x3B\x35" + 0x2CAE8000).s, &Keyboard::HorseChanger_Shaman, /** Shaman Horse*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x1A\x22\x2A\x21\x2C\x20\x6F\x18\x3E\x20\x20\x31" + 0x2CAE8000).s, ImVec2(10, 90 + 25), 178, 19, /** Girl*/XorStrTR<0x47, 7, 0x2CAE8000>("\x6D\x68\x0E\x23\x39\x20" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Chat
				if (Keyboard::ChatSpawmmer_Window)
				{
					int X = 190;
					int Y = 190;
					if (WindowCreate::WindowCloser(/*##Chat_Color*/XorStrTR<0x47, 13, 0x2CAE8000>("\x64\x6B\x0A\x22\x2A\x38\x12\x0D\x20\x3C\x3E\x20" + 0x2CAE8000).s, /** Chat Spawmmer*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x0A\x22\x2A\x38\x6D\x1D\x3F\x31\x26\x3F\x3E\x31\x27" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::ChatSpawmmer_Window))
					{
						if (UI::SexyButtonV2(/*Chat*/XorStrTR<0x47, 5, 0x2CAE8000>("\x04\x20\x28\x3E" + 0x2CAE8000).s, ImVec2(10, 115 + 25 + 25), ImVec2(178, 20), Keyboard::ChatSpawmmer, Low))
						{
							Keyboard::ChatSpawmmer = !Keyboard::ChatSpawmmer;
						}


						UI::HLToggleButton(/*##chat_blue*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x2A\x22\x2A\x38\x12\x2C\x23\x25\x34" + 0x2CAE8000).s, &Keyboard::ChatSpawmmer_Blue, /** Spawm Blue*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x1A\x3A\x2A\x3B\x20\x6E\x0D\x3C\x24\x37" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /*Writer in Blue*/XorStrTR<0x47, 15, 0x2CAE8000>("\x10\x3A\x20\x3E\x2E\x3E\x6D\x27\x21\x70\x13\x3E\x26\x31" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##chat_ping*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x2A\x22\x2A\x38\x12\x3E\x26\x3E\x36" + 0x2CAE8000).s, &Keyboard::ChatSpawmmer_Ping, /** Spawm Ping*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x1A\x3A\x2A\x3B\x20\x6E\x1F\x39\x3F\x35" + 0x2CAE8000).s, ImVec2(10, 65), 178, 19, /**Writer in Ping*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x1F\x3B\x23\x3F\x29\x3F\x6E\x26\x3E\x71\x02\x3A\x3A\x32" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##chat_yellow*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x2A\x22\x2A\x38\x12\x37\x2A\x3C\x3D\x3D\x24" + 0x2CAE8000).s, &Keyboard::ChatSpawmmer_Yellow, /** Spawm Yellow*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x1A\x3A\x2A\x3B\x20\x6E\x16\x35\x3D\x3E\x3C\x23" + 0x2CAE8000).s, ImVec2(10, 90), 178, 19, /**Writer in Yellow*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x1F\x3B\x23\x3F\x29\x3F\x6E\x26\x3E\x71\x0B\x36\x38\x39\x39\x20" + 0x2CAE8000).s);
						UI::HLToggleButton(/*##HorseChanger_Shaman*/XorStrTR<0x47, 22, 0x2CAE8000>("\x64\x6B\x01\x25\x39\x3F\x28\x0D\x27\x31\x3F\x35\x36\x26\x0A\x05\x3F\x39\x34\x3B\x35" + 0x2CAE8000).s, &Keyboard::ChatSpawmmer_Green, /** Shaman Green*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x1A\x22\x2A\x21\x2C\x20\x6F\x17\x23\x37\x36\x3A" + 0x2CAE8000).s, ImVec2(10, 90 + 25), 178, 19, /**Writer in Green*/XorStrTR<0x47, 17, 0x2CAE8000>("\x6D\x1F\x3B\x23\x3F\x29\x3F\x6E\x26\x3E\x71\x15\x21\x31\x30\x38" + 0x2CAE8000).s);

						UI::HLTextBox(/*##chat_txt*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x2A\x22\x2A\x38\x12\x3A\x37\x24" + 0x2CAE8000).s, ImVec2(10, 115 + 25), 178, 20, chat_spawmner_message, sizeof(chat_spawmner_message), Low, /**The Message you want to send*/XorStrTR<0x47, 30, 0x2CAE8000>("\x6D\x1C\x21\x2F\x6B\x01\x28\x3D\x3C\x31\x36\x37\x73\x2D\x3A\x23\x77\x2F\x38\x34\x2F\x7C\x29\x31\x7F\x13\x04\x0C\x07" + 0x2CAE8000).s);

					}
					ImGui::End();
				}
				// XP Donate
				if (Keyboard::XPDonate_Window)
				{
					int X = 230;
					int Y = 85;
					if (WindowCreate::WindowCloser(/*##XP_Donate*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x11\x1A\x14\x08\x22\x20\x2E\x24\x34" + 0x2CAE8000).s, /** XP Donate*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x11\x1A\x6B\x08\x22\x20\x2E\x24\x34" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::XPDonate_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::donate::tabs*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x29\x21\x21\x31\x25\x37\x69\x6E\x21\x37\x35\x2B" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*XP*/XorStrTR<0x47, 3, 0x2CAE8000>("\x1F\x18" + 0x2CAE8000).s))
							{

								// Sol
								if (UI::SexyButtonV2(/*Donate*/XorStrTR<0x47, 7, 0x2CAE8000>("\x03\x27\x27\x2B\x3F\x29" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::XPDonate, Low))
								{
									Keyboard::XPDonate = !Keyboard::XPDonate;
								}

								UI::HLNumberBoxInt(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::XPDonate_Limit, Low, /** Donate xp to your guild*/XorStrTR<0x47, 26, 0x2CAE8000>("\x6D\x68\x0D\x25\x25\x2D\x39\x2B\x6F\x28\x21\x72\x27\x3B\x75\x2F\x38\x2D\x2B\x7A\x3C\x29\x34\x32\x3B" + 0x2CAE8000).s);

								UI::HLNumberBoxInt(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, ImVec2(140, 60), 35, 20, &Keyboard::XPDonate_Speed, Low, /** You increase the speed of donation*/XorStrTR<0x47, 37, 0x2CAE8000>("\x6D\x68\x10\x25\x3E\x6C\x24\x20\x2C\x22\x34\x33\x20\x31\x75\x22\x3F\x3D\x79\x29\x2B\x39\x38\x3A\x7F\x0F\x07\x42\x07\x0B\x0B\x07\x13\x01\x06\x04" + 0x2CAE8000).s);

								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_XPDonate_Lonca, ImVec2(140 + 44, 60), 19, 19, /** Donate (Auto)*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x0D\x25\x25\x2D\x39\x2B\x6F\x78\x10\x27\x27\x3B\x7C" + 0x2CAE8000).s);

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Extra*/XorStrTR<0x47, 6, 0x2CAE8000>("\x02\x30\x3D\x38\x2A" + 0x2CAE8000).s))
							{
								// Sol
								if (UI::SexyButtonV2(/*Donate*/XorStrTR<0x47, 7, 0x2CAE8000>("\x03\x27\x27\x2B\x3F\x29" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::XPDonate, Low))
								{
									Keyboard::XPDonate = !Keyboard::XPDonate;
								}

								UI::HLNumberBoxInt(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::XPDonate_lvl, Low, /** Gives xp based on your level*/XorStrTR<0x47, 31, 0x2CAE8000>("\x6D\x68\x0E\x23\x3D\x29\x3E\x6E\x37\x20\x71\x30\x32\x27\x30\x32\x77\x37\x37\x7A\x22\x33\x28\x2C\x7F\x0C\x04\x14\x06\x08" + 0x2CAE8000).s);


								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::XPDonate_Level, ImVec2(138, 60), 19, 19, /** Donate (Lvl)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x0D\x25\x25\x2D\x39\x2B\x6F\x78\x1D\x24\x3F\x7D" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}
							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				//// XP Donate
				//if (Keyboard::XPDonate_Window)
				//{
				//	int X = 208;
				//	int Y = 65;
				//	if (WindowCreate::WindowCloser("##XP_Donate", "* XP Donate", X, Y, X + 10, Y + 10, Windows, &Keyboard::XPDonate_Window))
				//	{
				//		// Sol
				//		if (UI::SexyButtonV2("XP Donate", ImVec2(10, 40), ImVec2(78, 20), Keyboard::XPDonate, Low))
				//		{
				//			Keyboard::XPDonate = !Keyboard::XPDonate;
				//		}


				//	}
				//	ImGui::End();
				//}
				// Character Refresh
				if (Keyboard::InstanceRefresh_Window)
				{
					int X = 150;
					int Y = 65;
					if (WindowCreate::WindowCloser(/*##Char_Refresh*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x0A\x22\x2A\x3E\x12\x1C\x2A\x36\x23\x37\x20\x3C" + 0x2CAE8000).s,/** Refresh*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x1B\x2F\x2D\x3E\x28\x3D\x27" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::InstanceRefresh_Window))
					{
						// Sol
						if (UI::SexyButtonV2(/*Refresh*/XorStrTR<0x47, 8, 0x2CAE8000>("\x15\x2D\x2F\x38\x2E\x3F\x25" + 0x2CAE8000).s, ImVec2(10, 40), ImVec2(78, 20), Keyboard::InstanceRefresh, Low))
						{
							Keyboard::InstanceRefresh = !Keyboard::InstanceRefresh;
						}
						UI::HLCheckBox(/*##refresh_ınstance*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x3B\x2F\x2D\x3E\x28\x3D\x27\x0F\x60\x3C\x20\x20\x34\x38\x34\x3D" + 0x2CAE8000).s, &Keyboard::InstRefresh, ImVec2(100, 40), 19, 19, /*Constantly renews the character*/XorStrTR<0x47, 32, 0x2CAE8000>("\x04\x27\x27\x39\x3F\x2D\x23\x3A\x23\x29\x71\x20\x36\x3A\x30\x21\x24\x78\x2D\x32\x3E\x7C\x3E\x36\x3E\x12\x00\x01\x17\x01\x17" + 0x2CAE8000).s);
					}
					ImGui::End();
				}
				// Zoom
				if (Keyboard::CZoom_Window)
				{
					int X = 190;
					int Y = 120;
					if (WindowCreate::WindowCloser(/*##camera_zoom*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x2A\x2B\x26\x29\x3F\x2F\x10\x2A\x3E\x3D\x3E" + 0x2CAE8000).s, /** Zoom Camera*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x13\x25\x24\x21\x6D\x0D\x2E\x3D\x34\x20\x32" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::CZoom_Window))
					{
						if (UI::SexyButtonV2(/*Zoom*/XorStrTR<0x47, 5, 0x2CAE8000>("\x1D\x27\x26\x27" + 0x2CAE8000).s, ImVec2(10, 93), ImVec2(178, 20), Keyboard::CZoom, Low))
						{
							Keyboard::CZoom = !Keyboard::CZoom;
						}


						UI::HLToggleButton(/*##zoom_cameras*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x33\x25\x24\x21\x12\x2D\x2E\x3D\x34\x20\x32\x27" + 0x2CAE8000).s, &Keyboard::C_Zoom, /** Camera Zoom*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x0A\x2B\x26\x29\x3F\x2F\x6F\x0A\x3E\x3D\x3E" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** Raises your character's camera*/XorStrTR<0x47, 33, 0x2CAE8000>("\x6D\x68\x1B\x2B\x22\x3F\x28\x3D\x6F\x29\x3E\x27\x21\x74\x36\x3E\x36\x2A\x38\x39\x2F\x39\x2F\x79\x2C\x40\x02\x03\x0E\x01\x17\x07" + 0x2CAE8000).s);
						UI::HLNumberBoxInt(/*##zoom_limit*/XorStrTR<0x47, 13, 0x2CAE8000>("\x64\x6B\x33\x25\x24\x21\x12\x22\x26\x3D\x38\x26" + 0x2CAE8000).s, ImVec2(10, 65), 178, 20, &Keyboard::C_ZoomValue, Low, /** Change zoom value*/XorStrTR<0x47, 20, 0x2CAE8000>("\x6D\x68\x0A\x22\x2A\x22\x2A\x2B\x6F\x2A\x3E\x3D\x3E\x74\x23\x37\x3B\x2D\x3C" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Effect
				if (Keyboard::EffectChanger_Window)
				{
					int X = 190;
					int Y = 120;
					if (WindowCreate::WindowCloser(/*##effect_changer*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x2C\x2C\x2D\x29\x2E\x3A\x10\x33\x39\x33\x3D\x33\x30\x24" + 0x2CAE8000).s, /** Effect*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x0C\x2C\x2D\x29\x2E\x3A" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::EffectChanger_Window))
					{
						if (UI::SexyButtonV2(/*Effect*/XorStrTR<0x47, 7, 0x2CAE8000>("\x02\x2E\x2F\x2F\x28\x38" + 0x2CAE8000).s, ImVec2(10, 93), ImVec2(178, 20), Keyboard::EffectChanger, Low))
						{
							Keyboard::EffectChanger = !Keyboard::EffectChanger;
						}


						UI::HLToggleButton(/*##effect_gm*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x2C\x2C\x2D\x29\x2E\x3A\x10\x37\x3C" + 0x2CAE8000).s, &Keyboard::C_EffectGM, /** GM Tag*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x0E\x07\x6B\x18\x2C\x29" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /**GM Logo on top of your press indicates*/XorStrTR<0x47, 40, 0x2CAE8000>("\x6D\x0F\x04\x6A\x07\x23\x2A\x21\x6F\x3F\x3F\x72\x27\x3B\x25\x76\x38\x3E\x79\x23\x34\x29\x2F\x7E\x2F\x12\x04\x11\x10\x44\x0C\x08\x03\x01\x0A\x0B\x1F\x09\x1E" + 0x2CAE8000).s);

						UI::HLTextBox(/*##effect_txts*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x2C\x2C\x2D\x29\x2E\x3A\x10\x24\x29\x26\x20" + 0x2CAE8000).s, ImVec2(10, 65), 178, 20, EffectID, sizeof(EffectID), Low, /*Enter effect code*/XorStrTR<0x47, 18, 0x2CAE8000>("\x02\x26\x3D\x2F\x39\x6C\x28\x28\x29\x35\x32\x26\x73\x37\x3A\x32\x32" + 0x2CAE8000).s);

					}
					ImGui::End();
				}

				// ============================================== Dedectors ============================================== //
				// GM Dedect
				if (Keyboard::GMDedect_Window)
				{
					int X = 190;
					int Y = 168;
					if (WindowCreate::WindowCloser(/*##c_gm_dedector*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x2A\x15\x2C\x21\x12\x2A\x2A\x34\x34\x31\x27\x3B\x27" + 0x2CAE8000).s, /** GM Dedect*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x0E\x07\x6B\x08\x28\x2A\x2A\x33\x25" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::GMDedect_Window))
					{
						if (UI::SexyButtonV2(/*GM*/XorStrTR<0x47, 3, 0x2CAE8000>("\x00\x05" + 0x2CAE8000).s, ImVec2(10, 93 + 30 + 20), ImVec2(178, 20), Keyboard::GMDedect, Low))
						{
							Keyboard::GMDedect = !Keyboard::GMDedect;
						}


						UI::HLToggleButton(/*##gm_Playerss*/XorStrTR<0x47, 14, 0x2CAE8000>("\x64\x6B\x2E\x27\x14\x1C\x21\x2F\x36\x35\x23\x21\x20" + 0x2CAE8000).s, &Keyboard::C_GM_Dedect, /** Player*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x28\x3C" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** If it gets close to your character, it will be detected*/XorStrTR<0x47, 58, 0x2CAE8000>("\x6D\x68\x00\x2C\x6B\x25\x39\x6E\x28\x35\x25\x21\x73\x37\x39\x39\x24\x3D\x79\x2E\x34\x7C\x24\x31\x2A\x12\x41\x01\x0B\x05\x17\x07\x04\x1C\x0C\x18\x47\x4C\x04\x1A\x4F\x07\x18\x1E\x1F\x54\x17\x13\x57\x1C\x1C\x0E\x1E\x1F\x09\x1B\x1B" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##gm_dedect_sounds*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x2E\x27\x14\x28\x28\x2A\x2A\x33\x25\x0D\x20\x3B\x20\x38\x33\x2B" + 0x2CAE8000).s, &Keyboard::C_GM_Dedect_Sound, /** Play Beep*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x0C\x2A\x35\x21" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 19, /** Makes sound*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x04\x2B\x20\x29\x3E\x6E\x3C\x3F\x24\x3C\x37" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##gm_dedect_game_closer*/XorStrTR<0x47, 24, 0x2CAE8000>("\x64\x6B\x2E\x27\x14\x28\x28\x2A\x2A\x33\x25\x0D\x34\x35\x38\x33\x08\x3B\x35\x35\x28\x39\x2F" + 0x2CAE8000).s, &Keyboard::C_GM_Dedect_GameClose, /** Game Close*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0E\x2B\x26\x29\x6D\x0D\x23\x3F\x22\x37" + 0x2CAE8000).s, ImVec2(10, 40 + 25 + 25), 178, 19, /** Closes The Game*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x0A\x26\x24\x3F\x28\x3D\x6F\x04\x39\x37\x73\x13\x34\x3B\x32" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*gm_dedect_limit*/XorStrTR<0x47, 16, 0x2CAE8000>("\x20\x25\x16\x2E\x2E\x28\x28\x2D\x3B\x0F\x3D\x3B\x3E\x3D\x21" + 0x2CAE8000).s, ImVec2(10, 65 + 25 + 25), 178, 20, &Keyboard::C_GM_Dedect_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Stone
				if (Keyboard::StoneDedect_Window)
				{
					if (WindowCreate::WindowCloser(/*##c_stone_dedector*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x2A\x15\x38\x38\x22\x20\x2A\x0F\x35\x37\x37\x31\x36\x22\x38\x2A" + 0x2CAE8000).s, /** Stone Dedect*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x1A\x3E\x24\x22\x28\x6E\x0B\x35\x35\x37\x30\x20" + 0x2CAE8000).s, Keyboard::Stone_GuiSizeX, Keyboard::Stone_GuiSizeY, Keyboard::Stone_GuiSizeX + 10, Keyboard::Stone_GuiSizeY + 10, Windows, &Keyboard::StoneDedect_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##stonne::auto::tabs*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x3A\x3E\x24\x22\x23\x2B\x75\x6A\x30\x27\x27\x3B\x6F\x6C\x23\x39\x3B\x29" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Dedect*/XorStrTR<0x47, 7, 0x2CAE8000>("\x03\x2D\x2D\x2F\x28\x38" + 0x2CAE8000).s))
							{
								Keyboard::Stone_GuiSizeX = 230;
								// Sol
								if (UI::SexyButtonV2(/*Stone*/XorStrTR<0x47, 6, 0x2CAE8000>("\x14\x3C\x26\x24\x2E" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::StoneDedect, Low))
								{
									Keyboard::StoneDedect = !Keyboard::StoneDedect;
								}
								UI::HLNumberBoxInt(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_StoneDedect_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_StoneDedect, ImVec2(135, 60), 19, 19, /*Select (Stone Dedect) - If it gets close to your character, it will be detected*/XorStrTR<0x47, 80, 0x2CAE8000>("\x14\x2D\x25\x2F\x28\x38\x6D\x66\x1C\x24\x3E\x3C\x36\x74\x11\x33\x33\x3D\x3A\x2E\x72\x7C\x70\x7E\x16\x06\x41\x0B\x17\x44\x02\x03\x13\x1B\x49\x09\x07\x03\x1E\x0B\x4F\x04\x1E\x52\x0A\x1B\x00\x04\x57\x1B\x11\x1B\x09\x1D\x1E\x0A\x1A\xF2\xAD\xA2\xEA\xF0\xA5\xF1\xEE\xE4\xE5\xAA\xE9\xE9\xAD\xEA\xEA\xE4\xF4\xF1\xE7\xF1\xF1" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_StoneBeep, ImVec2(135+25, 60), 19, 19, /*Stone Dedect (Beep)*/XorStrTR<0x47, 20, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x09\x2B\x2B\x35\x32\x26\x73\x7C\x17\x33\x32\x28\x70" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_StoneTeleport, ImVec2(135 + 25 + 25, 60), 19, 19, /*Stone Dedect (Teleport Auto)*/XorStrTR<0x47, 29, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x09\x2B\x2B\x35\x32\x26\x73\x7C\x01\x33\x3B\x3D\x29\x35\x29\x28\x7D\x1F\x2A\x14\x0E\x4B" + 0x2CAE8000).s);


								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Esp*/XorStrTR<0x47, 4, 0x2CAE8000>("\x02\x3B\x39" + 0x2CAE8000).s))
							{
								Keyboard::Stone_GuiSizeX = 230;
								// Sol
								if (UI::SexyButtonV2(/*Stone*/XorStrTR<0x47, 6, 0x2CAE8000>("\x14\x3C\x26\x24\x2E" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::StoneDedect, Low))
								{
									Keyboard::StoneDedect = !Keyboard::StoneDedect;
								}
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_StoneDrawLine, ImVec2(95, 60), 19, 19, /*Stone (Draw Line)*/XorStrTR<0x47, 18, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x65\x0A\x3D\x31\x26\x72\x1F\x3D\x3B\x33\x7E" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_Stone3DBox, ImVec2(95+25, 60), 19, 19, /*Stone (3D Box)*/XorStrTR<0x47, 15, 0x2CAE8000>("\x14\x3C\x26\x24\x2E\x6C\x65\x7D\x0B\x70\x13\x3D\x2B\x7D" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}
							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// PlayerDedector
				if (Keyboard::PlayerDedect_Window)
				{
					int X = 190;
					int Y = 168;
					if (WindowCreate::WindowCloser(/*##c_player_dedector*/XorStrTR<0x47, 20, 0x2CAE8000>("\x64\x6B\x2A\x15\x3B\x20\x2C\x37\x2A\x22\x0E\x36\x36\x30\x30\x35\x23\x37\x2B" + 0x2CAE8000).s, /** Player Dedect*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x28\x3C\x6F\x14\x34\x36\x36\x37\x21" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::PlayerDedect_Window))
					{
						if (UI::SexyButtonV2(/*Player*/XorStrTR<0x47, 7, 0x2CAE8000>("\x17\x24\x28\x33\x2E\x3E" + 0x2CAE8000).s, ImVec2(10, 93 + 30 + 20), ImVec2(178, 20), Keyboard::PlayerDedect, Low))
						{
							Keyboard::PlayerDedect = !Keyboard::PlayerDedect;
						}


						UI::HLToggleButton(/*##Playerss*/XorStrTR<0x47, 11, 0x2CAE8000>("\x64\x6B\x19\x26\x2A\x35\x28\x3C\x3C\x23" + 0x2CAE8000).s, &Keyboard::C_Player_Dedect, /** Player*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x28\x3C" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /**If it gets close to your character, it will be detected*/XorStrTR<0x47, 57, 0x2CAE8000>("\x6D\x01\x2F\x6A\x22\x38\x6D\x29\x2A\x24\x22\x72\x30\x38\x3A\x25\x32\x78\x2D\x35\x7B\x25\x32\x2B\x2D\x40\x02\x0A\x02\x16\x04\x05\x13\x0D\x1B\x46\x4B\x05\x19\x4E\x18\x19\x1D\x1E\x53\x16\x10\x56\x13\x1D\x0D\x1F\x18\x08\x18\x1A" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##dedect_sounds*/XorStrTR<0x47, 16, 0x2CAE8000>("\x64\x6B\x2D\x2F\x2F\x29\x2E\x3A\x10\x23\x3E\x27\x3D\x30\x26" + 0x2CAE8000).s, &Keyboard::C_Player_Dedect_Sound, /** Play Beep*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x0C\x2A\x35\x21" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 19, /**Makes sound*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x05\x28\x21\x2E\x3F\x6D\x3D\x20\x25\x3F\x36" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##dedect_game_closer*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x2D\x2F\x2F\x29\x2E\x3A\x10\x37\x30\x3F\x36\x0B\x36\x3A\x38\x2B\x3C\x28" + 0x2CAE8000).s, &Keyboard::C_Player_Dedect_GameClose, /** Game Close*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0E\x2B\x26\x29\x6D\x0D\x23\x3F\x22\x37" + 0x2CAE8000).s, ImVec2(10, 40 + 25 + 25), 178, 19, /**Closes The Game*/XorStrTR<0x47, 17, 0x2CAE8000>("\x6D\x0B\x25\x25\x38\x29\x3E\x6E\x1B\x38\x34\x72\x14\x35\x38\x33" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##dedect_limit*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x2D\x2F\x2F\x29\x2E\x3A\x10\x3C\x38\x3F\x3A\x20" + 0x2CAE8000).s, ImVec2(10, 65 + 25 + 25), 178, 20, &Keyboard::C_Player_Dedect_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Mining Dedect 
				if (Keyboard::MiningDedect_Window)
				{
					if (WindowCreate::WindowCloser(/** Mining Dedect*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x04\x23\x25\x25\x23\x29\x6F\x14\x34\x36\x36\x37\x21" + 0x2CAE8000).s,/*Mining Dedectors*/XorStrTR<0x47, 17, 0x2CAE8000>("\x0A\x21\x27\x23\x25\x2B\x6D\x0A\x2A\x34\x34\x31\x27\x3B\x27\x25" + 0x2CAE8000).s, Keyboard::Mining_GuiSizeX, Keyboard::Stone_GuiSizeY, Keyboard::Mining_GuiSizeX + 10, Keyboard::Mining_GuiSizeY + 10, Windows, &Keyboard::MiningDedect_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##dedects*/XorStrTR<0x47, 10, 0x2CAE8000>("\x64\x6B\x2D\x2F\x2F\x29\x2E\x3A\x3C" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Dedect*/XorStrTR<0x47, 7, 0x2CAE8000>("\x03\x2D\x2D\x2F\x28\x38" + 0x2CAE8000).s))
							{
								Keyboard::Stone_GuiSizeX = 230;
								// Sol
								if (UI::SexyButtonV2(/*Mining*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0A\x21\x27\x23\x25\x2B" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::MiningDedect, Low))
								{
									Keyboard::MiningDedect = !Keyboard::MiningDedect;
								}
								UI::HLNumberBoxInt(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, ImVec2(95, 60), 35, 20, &Keyboard::C_Mining_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_MiningLimitörs, ImVec2(135, 60), 19, 19, /** Dedect Limitors*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x0D\x2F\x2F\x29\x2E\x3A\x6F\x1C\x38\x3F\x3A\x20\x3A\x24\x24" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_MiningBeep, ImVec2(135 + 25, 60), 19, 19, /** Mining (Sound Beep)*/XorStrTR<0x47, 22, 0x2CAE8000>("\x6D\x68\x04\x23\x25\x25\x23\x29\x6F\x78\x02\x3D\x26\x3A\x31\x76\x15\x3D\x3C\x2A\x72" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_MiningTeleport, ImVec2(135 + 25 + 25, 60), 19, 19, /** Mining Teleport*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x04\x23\x25\x25\x23\x29\x6F\x04\x34\x3E\x36\x24\x3A\x24\x23" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_AutoMining, ImVec2(135 + 25 + 25 + 25, 60), 19, 19, /** Auto Mining*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x03\x26\x3E\x38\x3C\x34" + 0x2CAE8000).s);

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Settings*/XorStrTR<0x47, 9, 0x2CAE8000>("\x14\x2D\x3D\x3E\x22\x22\x2A\x3D" + 0x2CAE8000).s))
							{
								if (UI::SexyButtonV2(/*Mining*/XorStrTR<0x47, 7, 0x2CAE8000>("\x0A\x21\x27\x23\x25\x2B" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::MiningDedect, Low))
								{
									Keyboard::MiningDedect = !Keyboard::MiningDedect;
								}
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_MiningLimitörs, ImVec2(95, 60), 19, 19, /** Show Ballon*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x1A\x22\x24\x3B\x6D\x0C\x2E\x3C\x3D\x3D\x3D" + 0x2CAE8000).s);
								ImGui::EndTabItem();
							}
							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				 
				// Portak
				if (Keyboard::PortalDedect_Window)
				{
					int X = 190;
					int Y = 168;
					if (WindowCreate::WindowCloser(/*##c_portaks*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x2A\x15\x3B\x23\x3F\x3A\x2E\x3B\x22" + 0x2CAE8000).s, /** Portal Dedect*/XorStrTR<0x47, 16, 0x2CAE8000>("\x6D\x68\x19\x25\x39\x38\x2C\x22\x6F\x14\x34\x36\x36\x37\x21" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::PortalDedect_Window))
					{
						if (UI::SexyButtonV2(/*Portal*/XorStrTR<0x47, 7, 0x2CAE8000>("\x17\x27\x3B\x3E\x2A\x20" + 0x2CAE8000).s, ImVec2(10, 93 + 30 + 20), ImVec2(178, 20), Keyboard::PortalDedect, Low))
						{
							Keyboard::PortalDedect = !Keyboard::PortalDedect;
						}


						UI::HLToggleButton(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_Portal, /** Portal*/XorStrTR<0x47, 9, 0x2CAE8000>("\x6D\x68\x19\x25\x39\x38\x2C\x22" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** Detects portals around*/XorStrTR<0x47, 25, 0x2CAE8000>("\x6D\x68\x0D\x2F\x3F\x29\x2E\x3A\x3C\x70\x21\x3D\x21\x20\x34\x3A\x24\x78\x38\x28\x34\x29\x33\x3A" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_Portal_Sound, /** Play Beep*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x0C\x2A\x35\x21" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 19, /** Makes sound*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x04\x2B\x20\x29\x3E\x6E\x3C\x3F\x24\x3C\x37" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_Portal_GameClose, /** Game Close*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0E\x2B\x26\x29\x6D\x0D\x23\x3F\x22\x37" + 0x2CAE8000).s, ImVec2(10, 40 + 25 + 25), 178, 19, /** Closes The Game*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x0A\x26\x24\x3F\x28\x3D\x6F\x04\x39\x37\x73\x13\x34\x3B\x32" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, ImVec2(10, 65 + 25 + 25), 178, 20, &Keyboard::C_Portal_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Teleport
				if (Keyboard::TeleportDedect_Window)
				{
					int X = 190;
					int Y = 168;
					if (WindowCreate::WindowCloser(/*##c_tele*/XorStrTR<0x47, 9, 0x2CAE8000>("\x64\x6B\x2A\x15\x3F\x29\x21\x2B" + 0x2CAE8000).s, /** Teleport Dedect*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x1D\x2F\x27\x29\x3D\x21\x3D\x24\x71\x16\x36\x30\x30\x35\x23" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::TeleportDedect_Window))
					{
						if (UI::SexyButtonV2(/*Teleport*/XorStrTR<0x47, 9, 0x2CAE8000>("\x13\x2D\x25\x2F\x3B\x23\x3F\x3A" + 0x2CAE8000).s, ImVec2(10, 93 + 30 + 20), ImVec2(178, 20), Keyboard::TeleportDedect, Low))
						{
							Keyboard::TeleportDedect = !Keyboard::TeleportDedect;
						}


						UI::HLToggleButton(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_Teleportss, /** Teleports*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x1D\x2F\x27\x29\x3D\x21\x3D\x24\x22" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** Detects Teleport around*/XorStrTR<0x47, 26, 0x2CAE8000>("\x6D\x68\x0D\x2F\x3F\x29\x2E\x3A\x3C\x70\x05\x37\x3F\x31\x25\x39\x25\x2C\x79\x3B\x29\x33\x28\x30\x3B" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_Teleportss_Sound, /** Play Beep*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x0C\x2A\x35\x21" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 19, /** Makes sound*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x04\x2B\x20\x29\x3E\x6E\x3C\x3F\x24\x3C\x37" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_Teleportss_GameClose, /** Game Close*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0E\x2B\x26\x29\x6D\x0D\x23\x3F\x22\x37" + 0x2CAE8000).s, ImVec2(10, 40 + 25 + 25), 178, 19, /** Closes The Game*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x0A\x26\x24\x3F\x28\x3D\x6F\x04\x39\x37\x73\x13\x34\x3B\x32" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, ImVec2(10, 65 + 25 + 25), 178, 20, &Keyboard::C_Teleportss_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Area üstüne PM dedect gelicek

				// Area
				if (Keyboard::AreaDedect_Window)
				{
					int X = 190;
					int Y = 116;
					if (WindowCreate::WindowCloser(/*##123*/XorStrTR<0x47, 6, 0x2CAE8000>("\x64\x6B\x78\x78\x78" + 0x2CAE8000).s, /** Area Dedect*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x38\x2E\x2D\x6D\x0A\x2A\x34\x34\x31\x27" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::AreaDedect_Window))
					{
						if (UI::SexyButtonV2(/*Area*/XorStrTR<0x47, 5, 0x2CAE8000>("\x06\x3A\x2C\x2B" + 0x2CAE8000).s, ImVec2(10, 93), ImVec2(178, 20), Keyboard::AreaDedect, Low))
						{
							Keyboard::AreaDedect = !Keyboard::AreaDedect;
						}


						UI::HLToggleButton(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, &Keyboard::C_AreaLine, /** Draw Lines*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0D\x38\x2A\x3B\x6D\x02\x26\x3E\x34\x21" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19, /** Draws a line around the character*/XorStrTR<0x47, 36, 0x2CAE8000>("\x6D\x68\x0D\x38\x2A\x3B\x3E\x6E\x2E\x70\x3D\x3B\x3D\x31\x75\x37\x25\x37\x2C\x34\x3F\x7C\x29\x36\x3A\x40\x02\x0A\x02\x16\x04\x05\x13\x0D\x1B" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##5*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7C" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 20, &Keyboard::C_AreaDist, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// Spawmdedect
				if (Keyboard::SpawmDedect_Window)
				{
					int X = 190;
					int Y = 168;
					if (WindowCreate::WindowCloser(/*##spawm_dedector*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3A\x3A\x2A\x3B\x20\x11\x2B\x35\x35\x37\x30\x20\x3A\x24" + 0x2CAE8000).s,/** Spawm Dedect*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x1A\x3A\x2A\x3B\x20\x6E\x0B\x35\x35\x37\x30\x20" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::SpawmDedect_Window))
					{
						if (UI::SexyButtonV2(/*Spawm*/XorStrTR<0x47, 6, 0x2CAE8000>("\x14\x38\x28\x3D\x26" + 0x2CAE8000).s, ImVec2(10, 93 + 30 + 20), ImVec2(178, 20), Keyboard::SpawmDedect, Low))
						{
							Keyboard::SpawmDedect = !Keyboard::SpawmDedect;
						}


						UI::HLToggleButton(/*##6*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7F" + 0x2CAE8000).s, &Keyboard::C_PlayerSpawm, /** Player Spawm*/XorStrTR<0x47, 15, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x28\x3C\x6F\x03\x21\x33\x24\x39" + 0x2CAE8000).s, ImVec2(10, 40), 178, 19,/** Detects Teleport around*/XorStrTR<0x47, 26, 0x2CAE8000>("\x6D\x68\x0D\x2F\x3F\x29\x2E\x3A\x3C\x70\x05\x37\x3F\x31\x25\x39\x25\x2C\x79\x3B\x29\x33\x28\x30\x3B" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##7*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7E" + 0x2CAE8000).s, &Keyboard::C_PlayerSpawm_Sound, /** Play Beep*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x0C\x2A\x35\x21" + 0x2CAE8000).s, ImVec2(10, 40 + 25), 178, 19, /** Makes sound*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x04\x2B\x20\x29\x3E\x6E\x3C\x3F\x24\x3C\x37" + 0x2CAE8000).s);

						UI::HLToggleButton(/*##8*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x71" + 0x2CAE8000).s, &Keyboard::C_PlayerSpawm_GameClose, /** Game Close*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0E\x2B\x26\x29\x6D\x0D\x23\x3F\x22\x37" + 0x2CAE8000).s, ImVec2(10, 40 + 25 + 25), 178, 19,/** Closes The Game*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x0A\x26\x24\x3F\x28\x3D\x6F\x04\x39\x37\x73\x13\x34\x3B\x32" + 0x2CAE8000).s);

						UI::HLNumberBoxInt(/*##9*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x70" + 0x2CAE8000).s, ImVec2(10, 65 + 25 + 25), 178, 20, &Keyboard::C_PlayerSpawm_Distance, Low, /** Distance*/XorStrTR<0x47, 11, 0x2CAE8000>("\x6D\x68\x0D\x23\x38\x38\x2C\x20\x2C\x35" + 0x2CAE8000).s);


					}
					ImGui::End();
				}
				// ============================================== Helper ============================================== //
				// Auto Dungeon
				if (Keyboard::AutoDungeon_Window)
				{

				}
				// WS Cheat
				if (Keyboard::WSCheat_Window)
				{
					int X = 230;
					int Y = 230;
					if (WindowCreate::WindowCloser(/*##ws_cheats*/XorStrTR<0x47, 12, 0x2CAE8000>("\x64\x6B\x3E\x39\x14\x2F\x25\x2B\x2E\x24\x22" + 0x2CAE8000).s, /** WS Hack*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x1E\x19\x6B\x04\x2C\x2D\x24" + 0x2CAE8000).s, Keyboard::WS_GuiSizeX, Keyboard::WS_GuiSizeY, Keyboard::WS_GuiSizeX + 10, Keyboard::WS_GuiSizeY + 10, Windows, &Keyboard::WSCheat_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::cheats::tabs*/XorStrTR<0x47, 19, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x2E\x26\x2A\x31\x25\x21\x69\x6E\x21\x37\x35\x2B" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Main*/XorStrTR<0x47, 5, 0x2CAE8000>("\x0A\x29\x20\x24" + 0x2CAE8000).s))
							{
								Keyboard::WS_GuiSizeY = 230;
								UI::HLToggleButton(/*##skil_range*/XorStrTR<0x47, 13, 0x2CAE8000>("\x64\x6B\x3A\x21\x22\x20\x12\x3C\x2E\x3E\x36\x37" + 0x2CAE8000).s, &Keyboard::C_SkillRange, /** Skill Range*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x1A\x21\x22\x20\x21\x6E\x1D\x31\x3F\x35\x36" + 0x2CAE8000).s, ImVec2(10, 58), 215, 19, /** Skills shot from a distance*/XorStrTR<0x47, 30, 0x2CAE8000>("\x6D\x68\x1A\x21\x22\x20\x21\x3D\x6F\x23\x39\x3D\x27\x74\x33\x24\x38\x35\x79\x3B\x7B\x38\x34\x2D\x2B\x01\x0F\x01\x06" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##skil_Liberated*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3A\x21\x22\x20\x12\x02\x26\x32\x34\x20\x32\x20\x30\x32" + 0x2CAE8000).s, &Keyboard::C_Liberated, /** Liberated*/XorStrTR<0x47, 12, 0x2CAE8000>("\x6D\x68\x05\x23\x29\x29\x3F\x2F\x3B\x35\x35" + 0x2CAE8000).s, ImVec2(10, 58 + 25), 215, 19, /** Your character looks dead*/XorStrTR<0x47, 28, 0x2CAE8000>("\x6D\x68\x10\x25\x3E\x3E\x6D\x2D\x27\x31\x23\x33\x30\x20\x30\x24\x77\x34\x36\x35\x30\x2F\x7D\x3A\x3A\x01\x05" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##skil_StrongBody*/XorStrTR<0x47, 18, 0x2CAE8000>("\x64\x6B\x3A\x21\x22\x20\x12\x1D\x3B\x22\x3E\x3C\x34\x16\x3A\x32\x2E" + 0x2CAE8000).s, &Keyboard::C_StrongBody, /** Strong Body*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x1A\x3E\x39\x23\x23\x29\x6F\x12\x3E\x36\x2A" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25), 215, 19, /** Adds blue area to character*/XorStrTR<0x47, 30, 0x2CAE8000>("\x6D\x68\x08\x2E\x2F\x3F\x6D\x2C\x23\x25\x34\x72\x32\x26\x30\x37\x77\x2C\x36\x7A\x38\x34\x3C\x2C\x3E\x03\x15\x07\x11" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##skil_AutoTarget*/XorStrTR<0x47, 18, 0x2CAE8000>("\x64\x6B\x3A\x21\x22\x20\x12\x0F\x3A\x24\x3E\x06\x32\x26\x32\x33\x23" + 0x2CAE8000).s, &Keyboard::C_AutoTarget, /** Auto Target*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x08\x3F\x3F\x23\x6D\x1A\x2E\x22\x36\x37\x27" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25), 215, 19, /** Click To Auto Target*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x0A\x26\x22\x2F\x26\x6E\x1B\x3F\x71\x13\x26\x20\x3A\x76\x03\x39\x2B\x3D\x3E\x28" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*"##skil_Type*/XorStrTR<0x47, 13, 0x2CAE8000>("\x65\x6B\x6A\x39\x20\x25\x21\x11\x1B\x29\x21\x37" + 0x2CAE8000).s, &Keyboard::C_Typer, /** Enemy*/XorStrTR<0x47, 8, 0x2CAE8000>("\x6D\x68\x0C\x24\x2E\x21\x34" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25), 215, 19, /** Return to enemy*/XorStrTR<0x47, 18, 0x2CAE8000>("\x6D\x68\x1B\x2F\x3F\x39\x3F\x20\x6F\x24\x3E\x72\x36\x3A\x30\x3B\x2E" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##skil_hitting*/XorStrTR<0x47, 15, 0x2CAE8000>("\x64\x6B\x3A\x21\x22\x20\x12\x26\x26\x24\x25\x3B\x3D\x33" + 0x2CAE8000).s, &Keyboard::C_Hitting,/** Hitting*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x01\x23\x3F\x38\x24\x20\x28" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25 + 25), 215, 19, /** Hitting Type 2*/XorStrTR<0x47, 17, 0x2CAE8000>("\x6D\x68\x01\x23\x3F\x38\x24\x20\x28\x70\x05\x2B\x23\x31\x75\x64" + 0x2CAE8000).s); // Yapıldı!!
								if (UI::SexyButtonV2(/*WS*/XorStrTR<0x47, 3, 0x2CAE8000>("\x10\x1B" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25 + 25 + 25), ImVec2(217, 20), Keyboard::WSCheat, Low))
								{
									Keyboard::WSCheat = !Keyboard::WSCheat;
								}
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Kill*/XorStrTR<0x47, 5, 0x2CAE8000>("\x0C\x21\x25\x26" + 0x2CAE8000).s))
							{
								Keyboard::WS_GuiSizeY = 210;
								UI::HLToggleButton(/*##teleport_click*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3D\x2F\x27\x29\x3D\x21\x3D\x24\x0E\x31\x3F\x3D\x36\x3D" + 0x2CAE8000).s, &Keyboard::C_ClickToTeleport, /** Click Warp*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0A\x26\x22\x2F\x26\x6E\x18\x31\x23\x22" + 0x2CAE8000).s, ImVec2(10, 58), 215, 19, /** When you click you will be teleported*/XorStrTR<0x47, 40, 0x2CAE8000>("\x6D\x68\x1E\x22\x2E\x22\x6D\x37\x20\x25\x71\x31\x3F\x3D\x36\x3D\x77\x21\x36\x2F\x7B\x2B\x34\x32\x33\x40\x03\x07\x43\x10\x00\x0A\x02\x18\x06\x18\x1F\x09\x09" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##teleport_everyonecheck*/XorStrTR<0x47, 25, 0x2CAE8000>("\x64\x6B\x3D\x2F\x27\x29\x3D\x21\x3D\x24\x0E\x37\x25\x31\x27\x2F\x38\x36\x3C\x39\x33\x39\x3E\x35" + 0x2CAE8000).s, &Keyboard::C_EveryoneCheck, /** Pull Check*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x19\x3F\x27\x20\x6D\x0D\x27\x35\x32\x39" + 0x2CAE8000).s, ImVec2(10, 58 + 25), 215, 19, /** Pull Everyone (CTRL)*/XorStrTR<0x47, 23, 0x2CAE8000>("\x6D\x68\x19\x3F\x27\x20\x6D\x0B\x39\x35\x23\x2B\x3C\x3A\x30\x76\x7F\x1B\x0D\x08\x17\x75" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##teleport_modelcheck*/XorStrTR<0x47, 22, 0x2CAE8000>("\x64\x6B\x3D\x2F\x27\x29\x3D\x21\x3D\x24\x0E\x3F\x3C\x30\x30\x3A\x34\x30\x3C\x39\x30" + 0x2CAE8000).s, &Keyboard::C_CheckModel, /** Model Check*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x04\x25\x2F\x29\x21\x6E\x0C\x38\x34\x31\x38" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25), 215, 19, /** Attracts the character's model (Shift)*/XorStrTR<0x47, 41, 0x2CAE8000>("\x6D\x68\x08\x3E\x3F\x3E\x2C\x2D\x3B\x23\x71\x26\x3B\x31\x75\x35\x3F\x39\x2B\x3B\x38\x28\x38\x2C\x78\x13\x41\x0F\x0C\x00\x00\x0A\x47\x40\x3A\x02\x02\x0A\x19\x47" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLToggleButton(/*##teleport_sendmodel*/XorStrTR<0x47, 21, 0x2CAE8000>("\x64\x6B\x3D\x2F\x27\x29\x3D\x21\x3D\x24\x0E\x21\x36\x3A\x31\x3B\x38\x3C\x3C\x36" + 0x2CAE8000).s, &Keyboard::C_SendModel, /** Send Model*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x1A\x2F\x25\x28\x6D\x03\x20\x34\x34\x3E" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25), 215, 19, /** Send to other players (Shift)*/XorStrTR<0x47, 32, 0x2CAE8000>("\x6D\x68\x1A\x2F\x25\x28\x6D\x3A\x20\x70\x3E\x26\x3B\x31\x27\x76\x27\x34\x38\x23\x3E\x2E\x2E\x7E\x77\x33\x09\x0B\x05\x10\x4C" + 0x2CAE8000).s); // Yapıldı!!
								UI::HLNumberBoxFloat(/*##teleport_click_dist*/XorStrTR<0x47, 22, 0x2CAE8000>("\x64\x6B\x3D\x2F\x27\x29\x3D\x21\x3D\x24\x0E\x31\x3F\x3D\x36\x3D\x08\x3C\x30\x29\x2F" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25), 215, 19, &Keyboard::C_ClickToTeleport_Distance, Low, /** All (Distance)*/XorStrTR<0x47, 17, 0x2CAE8000>("\x6D\x68\x08\x26\x27\x6C\x65\x0A\x26\x23\x25\x33\x3D\x37\x30\x7F" + 0x2CAE8000).s);
								if (UI::SexyButtonV2(/*WS*/XorStrTR<0x47, 3, 0x2CAE8000>("\x10\x1B" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25 + 25), ImVec2(217, 20), Keyboard::WSCheat, Low))
								{
									Keyboard::WSCheat = !Keyboard::WSCheat;
								}
								ImGui::EndTabItem();
							}

							//if (ImGui::BeginTabItem(/*Lonca*/XorStrTR<0x47, 6, 0x2CAE8000>("\x0B\x27\x27\x29\x2A" + 0x2CAE8000).s))
							//{
							//	ImGui::EndTabItem();
							//}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// Loader
				if (Keyboard::Loader)
				{

				}
				// Fish
				if (Keyboard::Fish_Window)
				{
					int X = 230;
					int Y = 210;
					if (WindowCreate::WindowCloser(/*##fish*/XorStrTR<0x47, 7, 0x2CAE8000>("\x64\x6B\x2F\x23\x38\x24" + 0x2CAE8000).s,/** Fish (Bot)*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x0F\x23\x38\x24\x6D\x66\x0D\x3F\x25\x7B" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Fish_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::fish::tabs*/XorStrTR<0x47, 17, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x2B\x27\x3C\x38\x6B\x68\x27\x35\x37\x25" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Classic*/XorStrTR<0x47, 8, 0x2CAE8000>("\x04\x24\x28\x39\x38\x25\x2E" + 0x2CAE8000).s))
							{

								if (UI::SexyButtonV2(/*Fish*/XorStrTR<0x47, 5, 0x2CAE8000>("\x01\x21\x3A\x22" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25 + 25), ImVec2(217, 20), Keyboard::WSCheat, Low))
								{
									Keyboard::Fish = !Keyboard::Fish;
								}
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Update*/XorStrTR<0x47, 7, 0x2CAE8000>("\x12\x38\x2D\x2B\x3F\x29" + 0x2CAE8000).s))
							{

								if (UI::SexyButtonV2(/*Fish*/XorStrTR<0x47, 5, 0x2CAE8000>("\x01\x21\x3A\x22" + 0x2CAE8000).s, ImVec2(10, 58 + 25 + 25 + 25 + 25 + 25), ImVec2(217, 20), Keyboard::WSCheat, Low))
								{
									Keyboard::Fish = !Keyboard::Fish;
								}
								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Fish Filter*/XorStrTR<0x47, 12, 0x2CAE8000>("\x01\x21\x3A\x22\x6B\x0A\x24\x22\x3B\x35\x23" + 0x2CAE8000).s))
							{
								ImGui::EndTabItem();
							}

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// Route
				if (Keyboard::Route_Window)
				{
					int X = 230;
					int Y = 85;
					if (WindowCreate::WindowCloser(/*##routes*/XorStrTR<0x47, 9, 0x2CAE8000>("\x64\x6B\x3B\x25\x3E\x38\x28\x3D" + 0x2CAE8000).s, /** Route*/XorStrTR<0x47, 8, 0x2CAE8000>("\x6D\x68\x1B\x25\x3E\x38\x28" + 0x2CAE8000).s, X, Y, X + 10, Y + 10, Windows, &Keyboard::Route_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##ws::route::tabs*/XorStrTR<0x47, 18, 0x2CAE8000>("\x64\x6B\x3E\x39\x71\x76\x3F\x21\x3A\x24\x34\x68\x69\x20\x34\x34\x24" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem(/*Route*/XorStrTR<0x47, 6, 0x2CAE8000>("\x15\x27\x3C\x3E\x2E" + 0x2CAE8000).s))
							{

								// Sol
								if (UI::SexyButtonV2(/*Route*/XorStrTR<0x47, 6, 0x2CAE8000>("\x15\x27\x3C\x3E\x2E" + 0x2CAE8000).s, ImVec2(10, 60), ImVec2(78, 20), Keyboard::Route, Low))
								{
									Keyboard::Route = !Keyboard::Route;
								}

								UI::HLCheckBox(/*##1*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x78" + 0x2CAE8000).s, &Keyboard::C_RouteRecording, ImVec2(95, 60), 19, 19, /** Save Route (Start)*/XorStrTR<0x47, 21, 0x2CAE8000>("\x6D\x68\x1A\x2B\x3D\x29\x6D\x1C\x20\x25\x25\x37\x73\x7C\x06\x22\x36\x2A\x2D\x73" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##2*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7B" + 0x2CAE8000).s, &Keyboard::C_PlayRoute, ImVec2(120, 60), 19, 19, /** Play Route*/XorStrTR<0x47, 13, 0x2CAE8000>("\x6D\x68\x19\x26\x2A\x35\x6D\x1C\x20\x25\x25\x37" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##3*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7A" + 0x2CAE8000).s, &Keyboard::C_ClearRoute, ImVec2(120+25, 60), 19, 19, /** Clear Route*/XorStrTR<0x47, 14, 0x2CAE8000>("\x6D\x68\x0A\x26\x2E\x2D\x3F\x6E\x1D\x3F\x24\x26\x36" + 0x2CAE8000).s);
								UI::HLCheckBox(/*##4*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x7D" + 0x2CAE8000).s, &Keyboard::C_RouteSave, ImVec2(120 + 25+25, 60), 19, 19, /** Save Route (File)*/XorStrTR<0x47, 20, 0x2CAE8000>("\x6D\x68\x1A\x2B\x3D\x29\x6D\x1C\x20\x25\x25\x37\x73\x7C\x13\x3F\x3B\x3D\x70" + 0x2CAE8000).s);

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem(/*Dungeon*/XorStrTR<0x47, 8, 0x2CAE8000>("\x03\x3D\x27\x2D\x2E\x23\x23" + 0x2CAE8000).s))
							{

								ImGui::EndTabItem();
							}

						/*	if (ImGui::BeginTabItem("Fish Filter"))
							{
								ImGui::EndTabItem();
							}*/

							ImGui::EndTabBar();
						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
				// Route
				if (Keyboard::Buff_Window)
				{
					//int X = 295;
					int Y = 85;
					if (WindowCreate::WindowCloser(/*##b*/XorStrTR<0x47, 4, 0x2CAE8000>("\x64\x6B\x2B" + 0x2CAE8000).s,/** Buffbot*/XorStrTR<0x47, 10, 0x2CAE8000>("\x6D\x68\x0B\x3F\x2D\x2A\x2F\x21\x3B" + 0x2CAE8000).s, Keyboard::C_BuffWindowSizeX, Y, Keyboard::C_BuffWindowSizeX + 10, Y + 10, Windows, &Keyboard::Route_Window))
					{
						ImGui::SetCursorPos(ImVec2(10, 36));
						ImGui::PushStyleColor(ImGuiCol_Tab, ImVec4(ImColor(35, 36, 37)));
						ImGui::PushStyleColor(ImGuiCol_TabHovered, ImVec4(ImColor(63, 64, 62)));
						ImGui::PushStyleColor(ImGuiCol_TabActive, ImVec4(ImColor(39, 41, 44)));
						ImGui::PushFont(Low);
						if (ImGui::BeginTabBar(/*##client*/XorStrTR<0x47, 9, 0x2CAE8000>("\x64\x6B\x2A\x26\x22\x29\x23\x3A" + 0x2CAE8000).s))
						{
							if (ImGui::BeginTabItem("Buff"))
							{
								Keyboard::C_BuffWindowSizeX = 295;
								if (UI::SexyButtonV2(SelectGame.c_str(), ImVec2(10, 60), ImVec2(78, 20), Keyboard::BuffBot, Low))
								{
									Keyboard::BuffBot = !Keyboard::BuffBot;
									if (Keyboard::BuffBot)
									{
										SelectGame = "Start (F5)";
										std::thread([]()
											{
												while (!(GetAsyncKeyState(VK_F5) & 0x8000))
													Sleep(50);

												SelectGame = "Select Game (F5)";

												if (!Controller::AllModule())
												{
													Keyboard::BuffBot = false;
													SelectGame = "Select (F5)";
												}
												else
												{
													SelectGame = "Done";
												}
											}).detach();
									}
									else
									{
										SelectGame = "Select (F5)";
										CloseHandle(Controller::hSlave);
										Controller::hSlave = nullptr;
										Controller::slave_base = 0;
									}
								}
								UI::HLCheckBox("##1", &Keyboard::C_BuffTarget, ImVec2(95, 60), 19, 19, "Buff (Auto Target)");
								UI::HLCheckBox("##2", &Keyboard::C_BuffNoClip, ImVec2(120, 60), 19, 19, "Buff (No Clip)");
								UI::HLCheckBox("##3", &Keyboard::C_BuffVector, ImVec2(120 + 25, 60), 19, 19, "Buff (Vector)");
								UI::HLCheckBox("##4", &Keyboard::C_SkillSlotF1, ImVec2(120 + 25 + 25, 60), 19, 19, "Skill Slot (1)");
								UI::HLCheckBox("##5", &Keyboard::C_SkillSlotF2, ImVec2(120 + 25 + 25+25, 60), 19, 19, "Skill Slot (2)");
								UI::HLCheckBox("##6", &Keyboard::C_SkillSlotF3, ImVec2(120 + 25 + 25+25+25, 60), 19, 19, "Skill Slot (3)");
								UI::HLCheckBox("##7", &Keyboard::C_SkillSlotF4, ImVec2(120 + 25 + 25 + 25 + 25 + 25, 60), 19, 19, "Skill Slot (4)");
								UI::HLCheckBox("##8", &Keyboard::C_BuffAutoRevive, ImVec2(120 + 25 + 25 + 25 + 25 + 25+25, 60), 19, 19, "Buff (Auto Revive)");

								ImGui::EndTabItem();
							}

							if (ImGui::BeginTabItem("Farming"))
							{
								Keyboard::C_BuffWindowSizeX = 295-25;
								if (UI::SexyButtonV2(SelectGame.c_str(), ImVec2(10, 60), ImVec2(78, 20), Keyboard::BuffBot, Low))
								{
									Keyboard::BuffBot = !Keyboard::BuffBot;
									if (Keyboard::BuffBot)
									{
										SelectGame = "Start (F5)";
										std::thread([]()
											{
												while (!(GetAsyncKeyState(VK_F5) & 0x8000))
													Sleep(50);

												SelectGame = "Select Game (F5)";

												if (!Controller::AllModule())
												{
													Keyboard::BuffBot = false;
													SelectGame = "Select (F5)";
												}
												else
												{
													SelectGame = "Done";
												}
											}).detach();
									}
									else
									{
										SelectGame = "Select (F5)";
										CloseHandle(Controller::hSlave);
										Controller::hSlave = nullptr;
										Controller::slave_base = 0;
									}
								}
								UI::HLCheckBox("##1", &Keyboard::C_BuffWalkToCut, ImVec2(95, 60), 19, 19, "Cut (You cut it while walking)");
								UI::HLCheckBox("##2", &Keyboard::C_BuffTeleport, ImVec2(120, 60), 19, 19, "Teleport (Slide Cuts)");
								UI::HLCheckBox("##3", &Keyboard::C_BuffLegit, ImVec2(120 + 25, 60), 19, 19, "Legit (Vector)");
								UI::HLCheckBox("##4", &Keyboard::C_BuffWalkToStone, ImVec2(120 + 25 + 25, 60), 19, 19, "Type (Stone)");
								UI::HLCheckBox("##5", &Keyboard::C_BuffWalkToEntity, ImVec2(120 + 25 + 25 + 25, 60), 19, 19, "Type (Entity)");
								UI::HLCheckBox("##6", &Keyboard::C_BuffTypeHorse, ImVec2(120 + 25 + 25 + 25 + 25, 60), 19, 19, "Type (Horse Mode)");
								UI::HLCheckBox("##7", &Keyboard::C_BuffTypePlayer, ImVec2(120 + 25 + 25 + 25 + 25 + 25, 60), 19, 19, "Type (Player Mode)");
								//UI::HLCheckBox("##8", &Keyboard::C_BuffAutoRevive, ImVec2(120 + 25 + 25 + 25 + 25 + 25 + 25, 60), 19, 19, "Buff (Auto Revive)");
								ImGui::EndTabItem();
							}
							if (ImGui::BeginTabItem("Dmg"))
							{
								Keyboard::C_BuffWindowSizeX = 295 - 25-25-25;
								if (UI::SexyButtonV2(SelectGame.c_str(), ImVec2(10, 60), ImVec2(78, 20), Keyboard::BuffBot, Low))
								{
									Keyboard::BuffBot = !Keyboard::BuffBot;
									if (Keyboard::BuffBot)
									{
										SelectGame = "Start (F5)";
										std::thread([]()
											{
												while (!(GetAsyncKeyState(VK_F5) & 0x8000))
													Sleep(50);

												SelectGame = "Select Game (F5)";

												if (!Controller::AllModule())
												{
													Keyboard::BuffBot = false;
													SelectGame = "Select (F5)";
												}
												else
												{
													SelectGame = "Done";
												}
											}).detach();
									}
									else
									{
										SelectGame = "Select (F5)";
										CloseHandle(Controller::hSlave);
										Controller::hSlave = nullptr;
										Controller::slave_base = 0;
									}
								}
								UI::HLCheckBox("##1", &Keyboard::C_BuffProDamage, ImVec2(95, 60), 19, 19, "Pro Damage (100x)");
								UI::HLCheckBox("##2", &Keyboard::C_BuffWaitDamage, ImVec2(120, 60), 19, 19, "Wait Damage (It Attacks The Objects Around It)");
								UI::HLCheckBox("##3", &Keyboard::C_BuffDmgStone, ImVec2(120 + 25, 60), 19, 19, "Type (Stone)");
								UI::HLCheckBox("##4", &Keyboard::C_BuffDmgEntity, ImVec2(120 + 25 + 25, 60), 19, 19, "Type (Entity)");
								UI::HLCheckBox("##5", &Keyboard::C_BuffDmgOther, ImVec2(120 + 25 + 25 + 25, 60), 19, 19, "Type (Other)");
								ImGui::EndTabItem();
							}
							ImGui::EndTabBar();

						}
						ImGui::PopStyleColor(3);
						ImGui::PopFont();
					}
					ImGui::End();
				}
			}

			ImGui::End();
			ImGui::PopStyleColor(2);
			/*ImGui::SetNextWindowSize(ImVec2(300, 272));
			ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0, 0, 0, 0));

			if (ImGui::Begin("* Apathe", nullptr,
				ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoScrollbar))
			{
				ImVec2 pos = ImGui::GetWindowPos();
				ImDrawList* draw = ImGui::GetWindowDrawList();
				ImGui::SetCursorPos(ImVec2(30, 1));
				draw->AddImage((void*)Background,ImVec2(pos.x + 30, pos.y + 1),ImVec2(pos.x + 30 + 270, pos.y + 1 + 270));

			}

			ImGui::End();
			ImGui::PopStyleColor();*/


		}
	}
	if (ImGui::Begin("#Main", reinterpret_cast<bool*>(true), ImVec2(0, 0), 0.0f, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoNav | ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_NoInputs | ImGuiWindowFlags_NoSavedSettings));
	{
		ImGui::SetWindowPos(ImVec2(0, 0), ImGuiCond_Always);
		ImGui::SetWindowSize(ImVec2(ImGui::GetIO().DisplaySize.x, ImGui::GetIO().DisplaySize.y), ImGuiCond_Always);

		for (auto& model : ModelInfo)
		{
			int x = static_cast<int>(model.Coord2D.x);
			int y = static_cast<int>(model.Coord2D.y);

			if (Keyboard::C_StoneDrawLine)
			{
				ImGui::GetBackgroundDrawList()->AddLine(ImVec2(ImGui::GetIO().DisplaySize.x / 2, ImGui::GetIO().DisplaySize.y), ImVec2(x, y), IM_COL32(255, 0, 0, 255), 1.0f);
			}
		}
		ModelInfo.clear();
	}

	// MiniMap
	if (Keyboard::Minimap_Window)
	{
		int X = 350;
		int Y = 350;
		if (WindowCreate::WindowCloser("##map", "* Radar", X, Y, X + 10, Y + 10, Windows, &Keyboard::Minimap_Window))
		{
			ImGuiWindow* window = ImGui::GetCurrentWindow();
			if (window)
			{
				window->Flags &= ~ImGuiWindowFlags_NoMove;
			}

			std::string MapName = engine::ApplicationBackground_GetCurrentMapName();
			if (!MapName.empty() && MapName != LastMapName)
			{
				LastMapName = MapName;
				C_GetRadarTexcureLoading = false;
				C_SelectMapTexcure = nullptr;
			}
			if (!C_GetRadarTexcureLoading && !MapName.empty())
			{
				std::string radarPath = "C:\\Resource\\Radar\\" + MapName + ".jpg";
				C_SelectMapTexcure = LoadTextureFromFileDX9(pDevice, radarPath.c_str(), &C_ImageWidth, &C_ImageHeight);
				C_GetRadarTexcureLoading = true;
			}

			/*if (!C_GetRadarTexcureLoading)
			{
				C_SelectMapTexcure = LoadTextureFromFileDX9(pDevice, "C:\\Resource\\Radar\\metin2_map_c1.jpg", &C_ImageWidth, &C_ImageHeight);
				C_GetRadarTexcureLoading = true;
			}*/


			if (C_SelectMapTexcure)
			{
				ImVec2 windowPos = ImGui::GetWindowPos();
				ImVec2 contentMin = ImGui::GetWindowContentRegionMin();
				ImVec2 contentMax = ImGui::GetWindowContentRegionMax();
				ImVec2 contentSize = ImVec2(contentMax.x - contentMin.x, contentMax.y - contentMin.y);
				ImVec2 contentPos = ImVec2(windowPos.x + contentMin.x, windowPos.y + contentMin.y);

				ImGuiIO& io = ImGui::GetIO();
				if (ImGui::IsWindowHovered() && io.KeyCtrl)C_RadarZoom += io.MouseWheel * 0.1f; C_RadarZoom = ImClamp(C_RadarZoom, 0.5f, 3.0f);

				ImVec2 clipMin = ImVec2(windowPos.x + contentMin.x, windowPos.y + contentMin.y + 26.0f);
				ImVec2 clipMax = ImVec2(windowPos.x + contentMax.x, windowPos.y + contentMax.y);
				ImGui::GetWindowDrawList()->PushClipRect(clipMin, clipMax, true);

				float playerMapX = abs(c_players.x) / 100.0f;
				float playerMapY = abs(c_players.y) / 100.0f;
				float centerX = contentSize.x / 2.0f;
				float centerY = contentSize.y / 2.0f;

				ImVec2 mapOffset = ImVec2(centerX - playerMapX * C_RadarZoom + mapPanOffset.x, centerY - playerMapY * C_RadarZoom + mapPanOffset.y);
				ImVec2 mapDrawPos = ImVec2(contentPos.x + mapOffset.x, contentPos.y + mapOffset.y);

				if (Keyboard::C_ShowMap)
				{
					DrawImagePos(C_SelectMapTexcure, ImVec2(C_ImageWidth * C_RadarZoom, C_ImageHeight * C_RadarZoom), mapDrawPos);
				}

				ImVec2 playerScreenPos = ImVec2(contentPos.x + centerX + mapPanOffset.x, contentPos.y + centerY + mapPanOffset.y);
				float arrowSize = 15.0f;
				float playerRotation = -DEG2RAD(c_players.Rotation - 180);
				ImVec2 center = playerScreenPos;
				ImVec2 pos1 = ImVec2(center.x, center.y - arrowSize);
				ImVec2 pos2 = ImVec2(center.x - arrowSize / 2, center.y + arrowSize / 2);
				ImVec2 pos3 = ImVec2(center.x + arrowSize / 2, center.y + arrowSize / 2);
				rotateVector(pos1.x, pos1.y, playerRotation, center.x, center.y);
				rotateVector(pos2.x, pos2.y, playerRotation, center.x, center.y);
				rotateVector(pos3.x, pos3.y, playerRotation, center.x, center.y);

				ImGui::GetWindowDrawList()->AddTriangleFilled(pos1, pos2, pos3, ImColor(250, 0, 12));
				ImGui::GetWindowDrawList()->AddTriangle(pos1, pos2, pos3, ImColor(0, 0, 0), 2.0f);

				float sizeFactor = 70.0f * C_RadarZoom;
				ImVec2 rectMin = ImVec2(center.x - sizeFactor, center.y - sizeFactor);
				ImVec2 rectMax = ImVec2(center.x + sizeFactor, center.y + sizeFactor);

				ImGui::GetWindowDrawList()->AddRectFilled(rectMin, rectMax, IM_COL32(5, 1, 5, 64), sizeFactor * 20.5f);
				ImGui::GetWindowDrawList()->AddCircle(center, sizeFactor, ImColor(250, 0, 142), 64, 2.0f);

				ImVec2 mousePos = ImGui::GetMousePos();
				bool isMouseInMapArea = (mousePos.x >= clipMin.x && mousePos.x <= clipMax.x && mousePos.y >= clipMin.y && mousePos.y <= clipMax.y);

				if (isMouseInMapArea)
				{
					if (window)
					{
						window->Flags |= ImGuiWindowFlags_NoMove;
					}

					bool ctrlPressed = GetAsyncKeyState(VK_CONTROL) & 0x8000;

					if (!ctrlPressed && ImGui::IsMouseClicked(0))
					{
						isDragging = true;
						dragStartPos = ImGui::GetMousePos();
					}

					if (ctrlPressed && ImGui::IsMouseClicked(0))
					{
						float mouseMapX = (mousePos.x - mapDrawPos.x) / C_RadarZoom;
						float mouseMapY = (mousePos.y - mapDrawPos.y) / C_RadarZoom;

						float targetX = mouseMapX * 100.0f;
						float targetY = mouseMapY * 100.0f; 

						g_TeleportMarker.position = Vector3(targetX, targetY, c_players.z);
						g_TeleportMarker.createdTime = ImGui::GetTime();
						g_TeleportMarker.active = true;
					}
				}
				else
				{
					if (window)
					{
						window->Flags &= ~ImGuiWindowFlags_NoMove;
					}
				}

				if (ImGui::IsMouseReleased(0))
				{
					isDragging = false;
				}

				if (isDragging && ImGui::IsMouseDown(0))
				{
					ImVec2 currentMousePos = ImGui::GetMousePos();
					ImVec2 delta = ImVec2(currentMousePos.x - dragStartPos.x, currentMousePos.y - dragStartPos.y);

					mapPanOffset.x += delta.x;
					mapPanOffset.y += delta.y;

					dragStartPos = currentMousePos;
				}

				if (isMouseInMapArea)
				{
					if (ImGui::IsMouseClicked(1))
					{
						float mouseMapX = (mousePos.x - mapDrawPos.x) / C_RadarZoom;
						float mouseMapY = (mousePos.y - mapDrawPos.y) / C_RadarZoom;

						float targetX = mouseMapX * 100.0f;
						float targetY = abs(mouseMapY * 100.0f);
						float targetZ = c_players.z;
						mouse_rota_points.push_back(Vector3(targetX, targetY, targetZ));
					}
				}

				if (g_TeleportMarker.active)
				{
					float elapsedTime = ImGui::GetTime() - g_TeleportMarker.createdTime;

					if (elapsedTime >= 3.0f)
					{
						int TmpCount = 0;
						int Count, Crap;
						Vector3 DestPos;
						DestPos.x = g_TeleportMarker.position.x;
						DestPos.y = g_TeleportMarker.position.y;
						DestPos.z = g_TeleportMarker.position.z;

						Vector3 TempPos = engine::GetTempPosition(DestPos, Count);

						while (TmpCount < Count)
						{
							TempPos = engine::GetTempPosition(DestPos, Crap);

							engine::InstanceBaseSCRIPT_SetPixelPosition(TempPos.x, TempPos.y);

							if (server_selectors == 1) {
								engine::NetworkStreamCelestialWorldSendCharacterState(TempPos, c_players.Rotation, 0, 0, 0);
								engine::NetworkStreamCelestialWorldSendCharacterState(g_TeleportMarker.position, c_players.Rotation, 0, 0, 0);
							}
							else if (server_selectors == 10) {
								engine::NetworkStreamAlves2SendCharacterState(TempPos, c_players.Rotation, 0, 0);
								engine::NetworkStreamAlves2SendCharacterState(g_TeleportMarker.position, c_players.Rotation, 0, 0);
							}
							else {
								engine::NetworkStreamSendCharacterStatePacket(Vector3{ TempPos.x, TempPos.y, TempPos.z }, c_players.Rotation, 0, 0);
								engine::NetworkStreamSendCharacterStatePacket(Vector3{ g_TeleportMarker.position.x, g_TeleportMarker.position.y, g_TeleportMarker.position.z }, c_players.Rotation, 0, 0);
							}

							engine::RadarSetPixelPosition(TempPos);
							engine::RadarSetPixelPosition(g_TeleportMarker.position);

							TmpCount += 1;
						}
						g_TeleportMarker.active = false;
					}
					else
					{
						float pointMapX = abs(g_TeleportMarker.position.x) / 100.0f;
						float pointMapY = abs(g_TeleportMarker.position.y) / 100.0f;
						int pointScreenX = (int)(contentPos.x + centerX + (pointMapX - playerMapX) * C_RadarZoom + mapPanOffset.x);
						int pointScreenY = (int)(contentPos.y + centerY + (pointMapY - playerMapY) * C_RadarZoom + mapPanOffset.y);

						float progress = elapsedTime / 3.0f;
						float pulseRadius = 15.0f + sin(elapsedTime * 5.0f) * 5.0f;

						int alpha = (int)(255 * (1.0f - progress));
						ImGui::GetWindowDrawList()->AddCircleFilled(ImVec2(pointScreenX, pointScreenY),pulseRadius,IM_COL32(255, 215, 0, alpha / 2),32);

						ImGui::GetWindowDrawList()->AddCircleFilled(ImVec2(pointScreenX, pointScreenY),8,ImColor(255, 215, 0),16);

						ImGui::GetWindowDrawList()->AddCircle(ImVec2(pointScreenX, pointScreenY),8,ImColor(255, 255, 255),16,2.0f);

						char countdown[32];
						sprintf_s(countdown, "%.1f", 3.0f - elapsedTime);
						ImVec2 textSize = ImGui::CalcTextSize(countdown);
						ImGui::GetWindowDrawList()->AddText(ImVec2(pointScreenX - textSize.x / 2, pointScreenY - 25),ImColor(255, 255, 255),countdown);
					}
				}

				if (is_following_route && !mouse_rota_points.empty())
				{
					Vector3 current_target = mouse_rota_points[0];

					float dx = c_players.x - current_target.x;
					float dy = abs(c_players.y) - current_target.y;
					float dz = c_players.z - current_target.z;
					float distance = sqrt(dx * dx + dy * dy + dz * dz);

					static int log_counter = 0;
					bool should_log = (log_counter++ % 500 == 0);
					if (distance < 1500.0f)
					{
						mouse_rota_points.erase(mouse_rota_points.begin());
						log_counter = 0;

						if (mouse_rota_points.empty())
						{
							is_following_route = false;
						}
						else
						{
						}
					}
					else
					{
						engine::InstanceBaseNEW_MoveToDestPixelPositionDirection(&current_target);
					}
				}

				if (!mouse_rota_points.empty())
				{
					for (size_t i = 0; i < mouse_rota_points.size(); ++i)
					{
						Vector3& point = mouse_rota_points[i];

						float pointMapX = abs(point.x) / 100.0f;
						float pointMapY = abs(point.y) / 100.0f;
						int pointScreenX = (int)(contentPos.x + centerX + (pointMapX - playerMapX) * C_RadarZoom + mapPanOffset.x);
						int pointScreenY = (int)(contentPos.y + centerY + (pointMapY - playerMapY) * C_RadarZoom + mapPanOffset.y);
						ImColor pointColor = (i == 0 && is_following_route) ? ImColor(255, 0, 0) : ImColor(0, 100, 255);

						ImGui::GetWindowDrawList()->AddCircleFilled(ImVec2(pointScreenX, pointScreenY), 6, pointColor, 16);
						ImGui::GetWindowDrawList()->AddCircle(ImVec2(pointScreenX, pointScreenY), 6, ImColor(255, 255, 255), 16, 2.0f);

						if (i < mouse_rota_points.size() - 1)
						{
							Vector3& nextPoint = mouse_rota_points[i + 1];
							float nextMapX = abs(nextPoint.x) / 100.0f;
							float nextMapY = abs(nextPoint.y) / 100.0f;
							int nextScreenX = (int)(contentPos.x + centerX + (nextMapX - playerMapX) * C_RadarZoom + mapPanOffset.x);
							int nextScreenY = (int)(contentPos.y + centerY + (nextMapY - playerMapY) * C_RadarZoom + mapPanOffset.y);

							ImGui::GetWindowDrawList()->AddLine(ImVec2(pointScreenX, pointScreenY), ImVec2(nextScreenX, nextScreenY), ImColor(0, 150, 255), 3.0f);
						}
					}
				}

				auto entities = GetEntities();
				for (const auto& ent : entities)
				{
					if (ent.x == c_players.x && ent.y == c_players.y)
						continue;

					float entityMapX = abs(ent.x) / 100.0f;
					float entityMapY = abs(ent.y) / 100.0f;
					int entityScreenX = (int)(contentPos.x + centerX + (entityMapX - playerMapX) * C_RadarZoom + mapPanOffset.x);
					int entityScreenY = (int)(contentPos.y + centerY + (entityMapY - playerMapY) * C_RadarZoom + mapPanOffset.y);

					if (ent.IsDead == 0)
					{
						if (Keyboard::C_ShowNPC && ent.Type == 1)
						{
							DrawCircle(entityScreenX, entityScreenY, 4, ImColor(255, 0, 60), 16);
						}
						if (Keyboard::C_ShowStone && ent.Type == 2)
						{
							DrawCircle(entityScreenX, entityScreenY, 4, ImColor(130, 19, 4), 16);
						}
						if (Keyboard::C_ShowEntity && ent.Type == 0)
						{
							DrawCircle(entityScreenX, entityScreenY, 4, ImColor(0, 17, 255), 16);
						}
						if (Keyboard::C_ShowBoss && ent.Type == 4)
						{
							DrawCircle(entityScreenX, entityScreenY, 4, ImColor(206, 5, 237), 16);
						}
					}
				}

				ImGui::GetWindowDrawList()->PopClipRect();

				float topButtonY = 8.3f;
				float checkboxY = 8.3f;
				float centerX_UI = contentSize.x / 2.0f;

				float CleartButtonX = centerX_UI - 35;
				if (UI::SexyButtonV2("Clr", ImVec2(CleartButtonX + 9, topButtonY + 325), ImVec2(32, 18), Keyboard::SpawmDedect, Low))
				{
					is_following_route = false;
					mouse_rota_points.clear();
				}

				if (UI::SexyButtonV2(" -", ImVec2(55 + 9 + 15, topButtonY), ImVec2(18, 18), Keyboard::SpawmDedect, Low))
				{
					C_RadarZoom -= 0.2f;
					C_RadarZoom = ImClamp(C_RadarZoom, 0.5f, 3.0f);
				}

				if (UI::SexyButtonV2(" +", ImVec2(75 + 10 + 17, topButtonY), ImVec2(18, 18), Keyboard::SpawmDedect, Low))
				{
					C_RadarZoom += 0.2f;
					C_RadarZoom = ImClamp(C_RadarZoom, 0.5f, 3.0f);
				}

				float startButtonX = centerX_UI - 35;
				if (UI::SexyButtonV2("Start", ImVec2(startButtonX - 130, topButtonY + 325), ImVec2(70, 18), Keyboard::SpawmDedect, Low))
				{
					if (!mouse_rota_points.empty())
					{
						is_following_route = true;
					}
					else
					{
					}
				}

				float stopButtonX = contentSize.x - 82;
				if (UI::SexyButtonV2("Stop", ImVec2(stopButtonX + 18, topButtonY + 325), ImVec2(70, 18), Keyboard::SpawmDedect, Low))
				{
					is_following_route = false;
				}

				float ResettButtonX = centerX_UI - 35;
				if (UI::SexyButtonV2("Rst", ImVec2(ResettButtonX + 40, topButtonY + 325), ImVec2(32, 18), Keyboard::SpawmDedect, Low))
				{
					mapPanOffset = ImVec2(0, 0);
				}

				UI::HLCheckBox("#1", &Keyboard::C_ShowMap, ImVec2(centerX_UI, checkboxY), 19, 19, "Show Map");
				UI::HLCheckBox("#2", &Keyboard::C_ShowNPC, ImVec2(centerX_UI - 22, checkboxY), 19, 19, "Show NPC");
				UI::HLCheckBox("#3", &Keyboard::C_ShowStone, ImVec2(centerX_UI - 44, checkboxY), 19, 19, "Show Stone");
				UI::HLCheckBox("#4", &Keyboard::C_ShowEntity, ImVec2(centerX_UI + 22, checkboxY), 19, 19, "Show Entity");
				UI::HLCheckBox("#5", &Keyboard::C_ShowBoss, ImVec2(centerX_UI + 44, checkboxY), 19, 19, "Show Boss");
			}
			else
			{
			}
		}
		ImGui::End();
	}

	/*AntiDebug::WatchMemory();
	AntiDebug::Mıxed();
	if (AntiDebug::Debugger()) {
		exit(0);
		return 1;
	}
	else {
	}*/
	ImGui::EndFrame();
	ImGui::Render();
	ImGui_ImplDX9_RenderDrawData(ImGui::GetDrawData());

	return EndScene_orig(pDevice);
}

//static bool check_konturol = true;
//DWORD WINAPI Set_CheckerWeb(HMODULE hModule)
//{
//	while (check_konturol)
//	{
//		try
//		{
//			const std::string url = User::CheckAllUser;
//			int pd = GetPhysicalDriveNumberFromVolume('C');
//			if (pd >= 0) {
//				if (QueryPhysicalDriveDescriptor(pd, User::s_model, User::s_serial, User::s_size)) {
//					if (checkLogin(User::DC_BaranKılıc, User::s_serial, url)) {
//						FixKey = true;
//						check_konturol = false;
//					}
//					else {
//						MessageBox(
//							NULL,
//							/*No matching account found in the system.\nPlease verify your credentials and try again.*/XorStrTR<0x47, 88, 0x2CAE8000>("\x09\x27\x69\x27\x2A\x38\x2E\x26\x26\x3E\x36\x72\x32\x37\x36\x39\x22\x36\x2D\x7A\x3D\x33\x28\x30\x3B\x40\x08\x0C\x43\x10\x0D\x03\x47\x1B\x10\x19\x1F\x09\x00\x40\x33\x1E\x21\x1E\x16\x15\x06\x13\x57\x0E\x1C\x08\x12\x1A\x04\x5E\x06\xEF\xF4\xF0\xA3\xE7\xF7\xE3\xE3\xED\xE7\xFE\xE2\xED\xE1\xFD\xAF\xF1\xFF\xF6\xB3\xE0\xE7\xEF\xB7\xF9\xFE\xFB\xF2\xF2\xB3" + 0x2CAE8000).s,
//							/*Apathe – Login*/XorStrTR<0x47, 15, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x6D\x5D\x6F\x1C\x3E\x35\x3A\x3A" + 0x2CAE8000).s,
//							MB_OK | MB_ICONERROR | MB_TOPMOST
//						);
//						exit(0);
//					}
//				}
//				else {
//					MessageBox(
//						NULL,
//						/*Failed to retrieve Windows information.\nPlease make sure your storage Windows is accessible.*/XorStrTR<0x47, 94, 0x2CAE8000>("\x01\x29\x20\x26\x2E\x28\x6D\x3A\x20\x70\x23\x37\x27\x26\x3C\x33\x21\x3D\x79\x0D\x32\x32\x39\x31\x28\x13\x41\x0B\x0D\x02\x0A\x14\x0A\x09\x1D\x03\x04\x02\x43\x32\x01\x20\x1D\x17\x12\x07\x10\x56\x1A\x19\x12\x1F\x5B\x0F\x08\x0C\x1A\xA0\xF8\xED\xF6\xF6\xA5\xF5\xF3\xE7\xFB\xEB\xEC\xE9\xAD\xD9\xE6\xFE\xF5\xFD\xE4\xE7\xB5\xFF\xE4\xB8\xF8\xF9\xF8\xF9\xEE\xED\xF6\xC2\xCD\xC7\x8D" + 0x2CAE8000).s,
//						/*Apathe – Windows*/XorStrTR<0x47, 17, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x6D\x5D\x6F\x07\x38\x3C\x37\x3B\x22\x25" + 0x2CAE8000).s,
//						MB_OK | MB_ICONERROR | MB_TOPMOST
//					);
//					exit(0);
//				}
//			}
//			else {
//				MessageBox(
//					NULL,
//					/*Unable to detect the system drive.\nPlease check your hardware configuration.*/XorStrTR<0x47, 78, 0x2CAE8000>("\x12\x26\x28\x28\x27\x29\x6D\x3A\x20\x70\x35\x37\x27\x31\x36\x22\x77\x2C\x31\x3F\x7B\x2F\x24\x2D\x2B\x05\x0C\x42\x07\x16\x0C\x10\x02\x46\x35\x04\x3B\x00\x08\x0F\x1C\x15\x51\x11\x1B\x11\x16\x1D\x57\x01\x16\x0F\x09\x5C\x15\x1F\x0D\xE4\xF6\xE3\xF1\xE1\xA5\xE5\xE8\xE6\xEF\xE3\xEC\xF9\xFF\xEF\xFB\xF9\xFE\xFC\xBD" + 0x2CAE8000).s,
//					/*Apathe – Critical*/XorStrTR<0x47, 18, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29\x6D\x5D\x6F\x13\x23\x3B\x27\x3D\x36\x37\x3B" + 0x2CAE8000).s,
//					MB_OK | MB_ICONERROR | MB_TOPMOST
//				);
//				exit(0);
//			}
//		}
//		catch (const std::exception& e)
//		{
//
//		}
//		catch (...)
//		{
//
//		}
//
//		Sleep(800);
//	}
//
//	return 0;
//}


DWORD WINAPI D3Dimgui(LPVOID lpParameter)
{
	while (!GetModuleHandleA("d3d9.dll")) {
		Sleep(200);
	}

	IDirect3D9* d3d = NULL;
	IDirect3DDevice9* d3ddev = NULL;

	HWND tmpWnd = CreateWindowA("BUTTON", "DX", WS_SYSMENU | WS_MINIMIZEBOX, CW_USEDEFAULT, CW_USEDEFAULT, 300, 300, NULL, NULL, Hand, NULL);
	if (tmpWnd == NULL)
	{
		return 0;
	}

	d3d = Direct3DCreate9(D3D_SDK_VERSION);
	if (d3d == NULL)
	{
		DestroyWindow(tmpWnd);
		return 0;
	}

	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp));
	d3dpp.Windowed = TRUE;
	d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
	d3dpp.hDeviceWindow = tmpWnd;
	d3dpp.BackBufferFormat = D3DFMT_UNKNOWN;

	HRESULT result = d3d->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, tmpWnd, D3DCREATE_SOFTWARE_VERTEXPROCESSING, &d3dpp, &d3ddev);
	if (result != D3D_OK)
	{
		d3d->Release();
		DestroyWindow(tmpWnd);
		return 0;
	}

#if defined _M_X64
	DWORD64* dVtable = (DWORD64*)d3ddev;
	dVtable = (DWORD64*)dVtable[0];
#elif defined _M_IX86
	DWORD* dVtable = (DWORD*)d3ddev;
	dVtable = (DWORD*)dVtable[0]; 
#endif
	SetStreamSource_orig = (SetStreamSource)dVtable[100];
	SetVertexDeclaration_orig = (SetVertexDeclaration)dVtable[87];
	SetVertexShaderConstantF_orig = (SetVertexShaderConstantF)dVtable[94];
	SetVertexShader_orig = (SetVertexShader)dVtable[92];
	SetPixelShader_orig = (SetPixelShader)dVtable[107];

	DrawIndexedPrimitive_orig = (DrawIndexedPrimitive)dVtable[82];
	DrawPrimitive_orig = (DrawPrimitive)dVtable[81];
	SetTexture_orig = (SetTexture)dVtable[65];
	Present_orig = (Present)dVtable[17];
	EndScene_orig = (EndScene)dVtable[42];
	Reset_orig = (Reset)dVtable[16];
	
	Sleep(1500); // bekleme süresi 

	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(LPVOID&)SetStreamSource_orig, (PBYTE)SetStreamSource_hook);
	DetourAttach(&(LPVOID&)SetVertexDeclaration_orig, (PBYTE)SetVertexDeclaration_hook);
	DetourAttach(&(LPVOID&)SetVertexShaderConstantF_orig, (PBYTE)SetVertexShaderConstantF_hook);
	DetourAttach(&(LPVOID&)SetVertexShader_orig, (PBYTE)SetVertexShader_hook);
	DetourAttach(&(LPVOID&)SetPixelShader_orig, (PBYTE)SetPixelShader_hook);

	DetourAttach(&(LPVOID&)DrawIndexedPrimitive_orig, (PBYTE)DrawIndexedPrimitive_hook);
	DetourAttach(&(LPVOID&)DrawPrimitive_orig, (PBYTE)DrawPrimitive_hook);
	DetourAttach(&(LPVOID&)SetTexture_orig, (PBYTE)SetTexture_hook);
	DetourAttach(&(LPVOID&)Present_orig, (PBYTE)Present_hook);
	DetourAttach(&(LPVOID&)EndScene_orig, (PBYTE)EndScene_hook);
	DetourAttach(&(LPVOID&)Reset_orig, (PBYTE)Reset_hook);
	DetourTransactionCommit();
	

	d3ddev->Release();
	d3d->Release();
	DestroyWindow(tmpWnd);

	return 1;
}

//==========================================================================================================================

static bool check_konturol = true;
DWORD WINAPI Set_CheckerWeb(HMODULE hModule)
{
	while (check_konturol)
	{
		try
		{
			static char gelen_key[65] = "cips";
			std::string KeyURL = /*https://pastebin.com/raw/2kpEnhHw*/XorStrTR<0x47, 34, 0x2CAE8000>("\x2F\x3C\x3D\x3A\x38\x76\x62\x61\x3F\x31\x22\x26\x36\x36\x3C\x38\x79\x3B\x36\x37\x74\x2E\x3C\x29\x70\x52\x0A\x12\x26\x0A\x0D\x2E\x10" + 0x2CAE8000).s;

			std::string KeyList = DownloadString(KeyURL);
			if (KeyList.empty())
			{
				MessageBox(NULL, /*[+] Key list could not be downloaded.*/XorStrTR<0x47, 38, 0x2CAE8000>("\x1C\x63\x14\x6A\x00\x29\x34\x6E\x23\x39\x22\x26\x73\x37\x3A\x23\x3B\x3C\x79\x34\x34\x28\x7D\x3C\x3A\x40\x05\x0D\x14\x0A\x09\x09\x06\x0C\x0C\x0E\x45" + 0x2CAE8000).s, /*Apathe*/XorStrTR<0x47, 7, 0x2CAE8000>("\x06\x38\x28\x3E\x23\x29" + 0x2CAE8000).s, MB_OK);
				check_konturol = false;
				exit(0);
				return 0;
			}

			if (IsKeyValid(gelen_key, KeyList))
			{
				//MessageBoxW(NULL, L"[+] Login Success", L"Apathe", MB_OK | MB_ICONINFORMATION);
				FixKey = true;
				//Multiple = true;
				check_konturol = false;
				return 0;
			}
			else
			{
				MessageBox(NULL, /*Merhaba! Hile Fixlendigi icin Giriş Yapamassınız\nHello, you cannot log in because the cheat has been fixed.*/XorStrTR<0x47, 109, 0x2CAE8000>("\x0A\x2D\x3B\x22\x2A\x2E\x2C\x6F\x6F\x18\x38\x3E\x36\x74\x13\x3F\x2F\x34\x3C\x34\x3F\x35\x3A\x37\x7F\x09\x02\x0B\x0D\x44\x22\x0F\x15\x01\x36\x4A\x32\x0D\x1D\x0F\x02\x11\x02\x01\x42\x1A\x44\x0C\x2B\x16\x31\x1F\x17\x10\x12\x52\x5F\xF9\xEE\xF7\xA3\xE7\xE4\xE8\xE9\xE7\xFD\xAA\xE7\xE3\xEA\xAE\xE6\xFE\xB1\xF0\xF6\xF7\xF4\xE3\xE4\xFD\xB9\xEE\xF3\xF9\xBD\xFD\xF7\xC5\xC0\xD6\x83\xCC\xC4\xD5\x87\xCA\xCC\xCF\xC5\x8C\xCB\xC7\xD7\xD5\xD5\x9C" + 0x2CAE8000).s,/*Error*/XorStrTR<0x47, 6, 0x2CAE8000>("\x02\x3A\x3B\x25\x39" + 0x2CAE8000).s, MB_OK);
				check_konturol = false;
				return 0;
			}
		}
		catch (const std::exception& e)
		{
			std::cout << /*[!] Exception:*/XorStrTR<0x47, 15, 0x2CAE8000>("\x1C\x69\x14\x6A\x0E\x34\x2E\x2B\x3F\x24\x38\x3D\x3D\x6E" + 0x2CAE8000).s << e.what() << std::endl;
		}
		catch (...)
		{
			std::cout << /*[!] Unknown exception caught.*/XorStrTR<0x47, 30, 0x2CAE8000>("\x1C\x69\x14\x6A\x1E\x22\x26\x20\x20\x27\x3F\x72\x36\x2C\x36\x33\x27\x2C\x30\x35\x35\x7C\x3E\x3F\x2A\x07\x09\x16\x4D" + 0x2CAE8000).s << std::endl;
		}

		Sleep(800);
	}

	return 0;
}

//class Exception : public std::runtime_error {
//public:
//	explicit Exception(const std::string& msg) : std::runtime_error(msg) {}
//};
//
//// --------------------- python27 loader ---------------------
//class python27 {
//public:
//	explicit python27(HMODULE pythonModule) {
//		loader(pythonModule);
//		if (p_PyEval_InitThreads) p_PyEval_InitThreads();
//	}
//
//	// returns NEW reference (caller must DecRef)
//	void* CallFunc(const char* moduleName, const char* funcName) {
//		void* g = AcquireGIL();
//		void* mod = p_PyImport_ImportModule(moduleName);
//		if (!mod) { ReleaseGIL(g); throw Exception(std::string("Failed to import module ") + moduleName); }
//		void* func = p_PyObject_GetAttrString(mod, funcName);
//		if (!func) { p_Py_DecRef(mod); ReleaseGIL(g); throw Exception(std::string("Failed to get func ") + funcName); }
//		void* result = p_PyObject_CallObject(func, nullptr);
//		p_Py_DecRef(func);
//		p_Py_DecRef(mod);
//		ReleaseGIL(g);
//		if (!result) throw Exception(std::string("Call failed ") + funcName);
//		return result; // new ref
//	}
//
//	// Call function obj with tuple args
//	void* CallFuncWithTuple(void* funcObj, void* args) {
//		if (!funcObj || !args) throw Exception("CallFuncWithTuple: funcObj or args is null");
//		void* g = AcquireGIL();
//		void* result = p_PyObject_CallObject(funcObj, args);
//		ReleaseGIL(g);
//		if (!result) throw Exception("CallFuncWithTuple failed");
//		return result; // caller must DecRef
//	}
//
//	// GIL helpers
//	void* AcquireGIL() {
//		if (p_PyGILState_Ensure) return p_PyGILState_Ensure();
//		return nullptr;
//	}
//	void ReleaseGIL(void* state) {
//		if (p_PyGILState_Release && state) p_PyGILState_Release(state);
//	}
//
//	// tuple/float/dec helpers
//	int TupleSize(void* obj) { return p_PyTuple_Size ? (int)p_PyTuple_Size(obj) : 0; }
//	void* TupleGetItem(void* obj, int idx) { return p_PyTuple_GetItem ? p_PyTuple_GetItem(obj, idx) : nullptr; }
//	double FloatAsDouble(void* obj) { return p_PyFloat_AsDouble ? p_PyFloat_AsDouble(obj) : 0.0; }
//	void DecRef(void* obj) { if (p_Py_DecRef && obj) p_Py_DecRef(obj); }
//
//	// tuple / long helpers
//	void* Tuple_New(long size) { if (!p_PyTuple_New) throw Exception("PyTuple_New not loaded"); return p_PyTuple_New(size); }
//	int Tuple_SetItem(void* tuple, long idx, void* item) { if (!p_PyTuple_SetItem) throw Exception("PyTuple_SetItem not loaded"); return p_PyTuple_SetItem(tuple, idx, item); }
//	void* Long_FromLong(long value) { if (!p_PyLong_FromLong) throw Exception("PyLong_FromLong not loaded"); return p_PyLong_FromLong(value); }
//
//	// --------------------- Function pointers (public for internal usage) ---------------------
//	void* (*p_PyImport_ImportModule)(const char*) = nullptr;
//	void* (*p_PyObject_GetAttrString)(void*, const char*) = nullptr;
//
//private:
//	int (*p_PyRun_SimpleStringFlags)(const char*, void*) = nullptr;
//	void* (*p_PyGILState_Ensure)() = nullptr;
//	void (*p_PyGILState_Release)(void*) = nullptr;
//	void (*p_PyEval_InitThreads)() = nullptr;
//	void* (*p_PyObject_CallObject)(void*, void*) = nullptr;
//	long (*p_PyLong_AsLong)(void*) = nullptr;
//	void (*p_Py_DecRef)(void*) = nullptr;
//	long (*p_PyTuple_Size)(void*) = nullptr;
//	void* (*p_PyTuple_GetItem)(void*, int) = nullptr;
//	double (*p_PyFloat_AsDouble)(void*) = nullptr;
//
//	void* (*p_PyTuple_New)(long) = nullptr;
//	int (*p_PyTuple_SetItem)(void*, long, void*) = nullptr;
//	void* (*p_PyLong_FromLong)(long) = nullptr;
//
//	void loader(HMODULE pythonModule) {
//		if (!pythonModule) throw Exception("pythonModule is null");
//
//		p_PyRun_SimpleStringFlags = reinterpret_cast<decltype(p_PyRun_SimpleStringFlags)>(GetProcAddress(pythonModule, "PyRun_SimpleStringFlags"));
//		p_PyGILState_Ensure = reinterpret_cast<decltype(p_PyGILState_Ensure)>(GetProcAddress(pythonModule, "PyGILState_Ensure"));
//		p_PyGILState_Release = reinterpret_cast<decltype(p_PyGILState_Release)>(GetProcAddress(pythonModule, "PyGILState_Release"));
//		p_PyEval_InitThreads = reinterpret_cast<decltype(p_PyEval_InitThreads)>(GetProcAddress(pythonModule, "PyEval_InitThreads"));
//		p_PyImport_ImportModule = reinterpret_cast<decltype(p_PyImport_ImportModule)>(GetProcAddress(pythonModule, "PyImport_ImportModule"));
//		p_PyObject_GetAttrString = reinterpret_cast<decltype(p_PyObject_GetAttrString)>(GetProcAddress(pythonModule, "PyObject_GetAttrString"));
//		p_PyObject_CallObject = reinterpret_cast<decltype(p_PyObject_CallObject)>(GetProcAddress(pythonModule, "PyObject_CallObject"));
//		p_PyLong_AsLong = reinterpret_cast<decltype(p_PyLong_AsLong)>(GetProcAddress(pythonModule, "PyLong_AsLong"));
//		p_Py_DecRef = reinterpret_cast<decltype(p_Py_DecRef)>(GetProcAddress(pythonModule, "Py_DecRef"));
//		p_PyTuple_Size = reinterpret_cast<decltype(p_PyTuple_Size)>(GetProcAddress(pythonModule, "PyTuple_Size"));
//		p_PyTuple_GetItem = reinterpret_cast<decltype(p_PyTuple_GetItem)>(GetProcAddress(pythonModule, "PyTuple_GetItem"));
//		p_PyFloat_AsDouble = reinterpret_cast<decltype(p_PyFloat_AsDouble)>(GetProcAddress(pythonModule, "PyFloat_AsDouble"));
//		p_PyTuple_New = reinterpret_cast<decltype(p_PyTuple_New)>(GetProcAddress(pythonModule, "PyTuple_New"));
//		p_PyTuple_SetItem = reinterpret_cast<decltype(p_PyTuple_SetItem)>(GetProcAddress(pythonModule, "PyTuple_SetItem"));
//		p_PyLong_FromLong = reinterpret_cast<decltype(p_PyLong_FromLong)>(GetProcAddress(pythonModule, "PyLong_FromLong"));
//
//		if (!p_PyGILState_Ensure || !p_PyGILState_Release || !p_PyImport_ImportModule ||
//			!p_PyObject_GetAttrString || !p_PyObject_CallObject || !p_Py_DecRef || !p_PyTuple_Size ||
//			!p_PyTuple_GetItem || !p_PyFloat_AsDouble || !p_PyTuple_New || !p_PyTuple_SetItem || !p_PyLong_FromLong)
//		{
//			throw Exception("Failed to get required Python function addresses (check DLL exports and bitness)");
//		}
//	}
//};
//
//// --------------------- SendRefinePacket ---------------------
//void SendRefinePacket(python27& py, int arg1, int arg2) {
//	try {
//		void* gil = py.AcquireGIL();
//		void* args = py.Tuple_New(2);
//		py.Tuple_SetItem(args, 0, py.Long_FromLong(arg1));
//		py.Tuple_SetItem(args, 1, py.Long_FromLong(arg2));
//
//		void* mod = py.p_PyImport_ImportModule("net");
//		if (!mod) throw Exception("Failed to import net module");
//
//		void* func = py.p_PyObject_GetAttrString(mod, "SendRefinePacket");
//		if (!func) { py.DecRef(mod); throw Exception("Failed to get net.SendRefinePacket"); }
//
//		void* result = py.CallFuncWithTuple(func, args);
//		if (result) py.DecRef(result);
//
//		py.DecRef(func);
//		py.DecRef(mod);
//		py.DecRef(args);
//		py.ReleaseGIL(gil);
//	}
//	catch (const Exception& e) {
//		std::cout << "SendRefinePacket error: " << e.what() << std::endl;
//	}
//}
//
//
//
//double PickupItem(python27& py) {
//	double value = 0.0;
//
//	void* result = nullptr;
//	try {
//		result = py.CallFunc("player", "PickCloseItem"); // new ref
//	}
//	catch (const Exception& e) {
//		std::cout << "PickupItem CallFunc error: " << e.what() << "\n";
//		return value;
//	}
//
//	if (!result) return value;
//
//	void* gil = py.AcquireGIL();
//	value = py.FloatAsDouble(result);
//	py.ReleaseGIL(gil);
//
//	py.DecRef(result);
//	return value;
//}
//
//// --------------------- PrintTupleResult ---------------------
//void PrintTupleResult(python27& py, void* result) {
//	if (!result) return;
//	void* gil = py.AcquireGIL();
//	int tupleSize = py.TupleSize(result);
//	if (tupleSize >= 3) {
//		void* xObj = py.TupleGetItem(result, 0);
//		void* yObj = py.TupleGetItem(result, 1);
//		void* zObj = py.TupleGetItem(result, 2);
//
//		float x = xObj ? py.FloatAsDouble(xObj) : 0.0;
//		float y = yObj ? py.FloatAsDouble(yObj) : 0.0;
//		float z = zObj ? py.FloatAsDouble(zObj) : 0.0;
//
//		std::cout << "x: " << x << " | y: " << y << " | z: " << z << std::endl;
//	}
//	else {
//		std::cout << "Unexpected result (not a tuple or <3)\n";
//	}
//	py.ReleaseGIL(gil);
//	py.DecRef(result);
//}
//
//// --------------------- FindModule ---------------------
//void FindModule(python27& py, std::initializer_list<const char*> args) {
//	std::vector<const char*> v(args);
//	if (v.size() < 2) {
//		std::cout << "FindModule: need [module, function]\n";
//		return;
//	}
//	const char* moduleName = v[0];
//	const char* funcName = v[1];
//
//	while (!(GetAsyncKeyState(VK_HOME) & 1)) {
//		void* result = nullptr;
//		try {
//			result = py.CallFunc(moduleName, funcName); // new ref
//		}
//		catch (const Exception& e) {
//			std::cout << "CallFunc error: " << e.what() << "\n";
//			std::this_thread::sleep_for(std::chrono::milliseconds(500));
//			continue;
//		}
//
//		PrintTupleResult(py, result);
//		std::this_thread::sleep_for(std::chrono::milliseconds(200));
//	}
//}
//
//// --------------------- PythonThread ---------------------
//DWORD WINAPI PythonThread(LPVOID lpParam) {
//	(void)lpParam;
//	AllocConsole();
//	FILE* fp;
//	freopen_s(&fp, "CONOUT$", "w", stdout);
//	freopen_s(&fp, "CONIN$", "r", stdin);
//	SetConsoleTitleA("Python Inject Console");
//
//	HMODULE pythonDLL = LoadLibraryA("python27.dll");
//	if (!pythonDLL) {
//		DWORD err = GetLastError();
//		char buf[256] = { 0 };
//		FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, nullptr, err, 0, buf, sizeof(buf), nullptr);
//		std::cout << "Failed to load python27.dll, Err: " << err << " - " << buf << "\n";
//		return 1;
//	}
//
//	try {
//		python27 py(pythonDLL);
//
//		//// örnek: PickupItem loop
//		//try {
//		//	while (!(GetAsyncKeyState(VK_HOME) & 1)) {
//		//		double picked = PickupItem(py);
//		//		if (picked != 0.0) {
//		//			std::cout << "Picked item value: " << picked << std::endl;
//		//		}
//		//		std::this_thread::sleep_for(std::chrono::milliseconds(200));
//		//	}
//		//}
//		//catch (const Exception& ex) {
//		//	std::cout << "Python error: " << ex.what() << std::endl;
//		//}
//		int refineSlot = 0;
//		int refineType = 0;
//
//		SendRefinePacket(py, refineSlot, refineType); // bu kadar basit
//		FindModule(py, { "player", "GetMainCharacterPosition" });
//
//	}
//	catch (const Exception& ex) {
//		std::cout << "Python error: " << ex.what() << std::endl;
//	}
//
//	FreeLibrary(pythonDLL);
//	FreeConsole();
//	return 0;
//}


//class Exception : public std::runtime_error {
//public:
//	explicit Exception(const std::string& msg) : std::runtime_error(msg) {}
//};
//class python27 {
//public:
//	explicit python27(HMODULE pythonModule) {
//		loader(pythonModule);
//		if (p_PyEval_InitThreads) p_PyEval_InitThreads();
//	}
//	void* Function(const char* Module, const char* funnction) {
//		void* g = AcquireGIL();
//		void* mod = p_PyImport_ImportModule(Module);
//		if (!mod) { ReleaseGIL(g); throw Exception(std::string("Failed to import module ") + Module); }
//		void* func = p_PyObject_GetAttrString(mod, funnction);
//		if (!func) { p_Py_DecRef(mod); ReleaseGIL(g); throw Exception(std::string("Failed to get func ") + funnction); }
//		void* result = p_PyObject_CallObject(func, nullptr);
//		p_Py_DecRef(func);
//		p_Py_DecRef(mod);
//		ReleaseGIL(g);
//		if (!result) throw Exception(std::string("Call failed ") + funnction);
//		return result;
//	}
//	void* WithTuple(void* Obj, void* Argument) {
//		if (!Obj || !Argument) throw Exception("WithTuple: Obj or args is null");
//		void* g = AcquireGIL();
//		void* result = p_PyObject_CallObject(Obj, Argument);
//		ReleaseGIL(g);
//		if (!result) throw Exception("WithTuple failed");
//		return result;
//	}
//	void* AcquireGIL() {
//		if (p_PyGILState_Ensure) return p_PyGILState_Ensure();
//		return nullptr;
//	}
//	void ReleaseGIL(void* state) {
//		if (p_PyGILState_Release && state) p_PyGILState_Release(state);
//	}
//	int TupleSize(void* obj) { return p_PyTuple_Size ? (int)p_PyTuple_Size(obj) : 0; }
//	void* TupleGetItem(void* obj, int idx) { return p_PyTuple_GetItem ? p_PyTuple_GetItem(obj, idx) : nullptr; }
//	double FloatAsDouble(void* obj) { return p_PyFloat_AsDouble ? p_PyFloat_AsDouble(obj) : 0.0; }
//	void DecRef(void* obj) { if (p_Py_DecRef && obj) p_Py_DecRef(obj); }
//	void* Tuple_New(long size) { if (!p_PyTuple_New) throw Exception("PyTuple_New not loaded"); return p_PyTuple_New(size); }
//	int Tuple_SetItem(void* tuple, long idx, void* item) { if (!p_PyTuple_SetItem) throw Exception("PyTuple_SetItem not loaded"); return p_PyTuple_SetItem(tuple, idx, item); }
//	void* Long_FromLong(long value) { if (!p_PyLong_FromLong) throw Exception("PyLong_FromLong not loaded"); return p_PyLong_FromLong(value); }
//	void* (*p_PyImport_ImportModule)(const char*) = nullptr;
//	void* (*p_PyObject_GetAttrString)(void*, const char*) = nullptr;
//
//private:
//	int (*p_PyRun_SimpleStringFlags)(const char*, void*) = nullptr;
//	void* (*p_PyGILState_Ensure)() = nullptr;
//	void (*p_PyGILState_Release)(void*) = nullptr;
//	void (*p_PyEval_InitThreads)() = nullptr;
//	void* (*p_PyObject_CallObject)(void*, void*) = nullptr;
//	long (*p_PyLong_AsLong)(void*) = nullptr;
//	void (*p_Py_DecRef)(void*) = nullptr;
//	long (*p_PyTuple_Size)(void*) = nullptr;
//	void* (*p_PyTuple_GetItem)(void*, int) = nullptr;
//	double (*p_PyFloat_AsDouble)(void*) = nullptr;
//
//	void* (*p_PyTuple_New)(long) = nullptr;
//	int (*p_PyTuple_SetItem)(void*, long, void*) = nullptr;
//	void* (*p_PyLong_FromLong)(long) = nullptr;
//
//	void loader(HMODULE pythonModule) {
//		if (!pythonModule) throw Exception("module is null");
//
//		p_PyRun_SimpleStringFlags = reinterpret_cast<decltype(p_PyRun_SimpleStringFlags)>(GetProcAddress(pythonModule, "PyRun_SimpleStringFlags"));
//		p_PyGILState_Ensure = reinterpret_cast<decltype(p_PyGILState_Ensure)>(GetProcAddress(pythonModule, "PyGILState_Ensure"));
//		p_PyGILState_Release = reinterpret_cast<decltype(p_PyGILState_Release)>(GetProcAddress(pythonModule, "PyGILState_Release"));
//		p_PyEval_InitThreads = reinterpret_cast<decltype(p_PyEval_InitThreads)>(GetProcAddress(pythonModule, "PyEval_InitThreads"));
//		p_PyImport_ImportModule = reinterpret_cast<decltype(p_PyImport_ImportModule)>(GetProcAddress(pythonModule, "PyImport_ImportModule"));
//		p_PyObject_GetAttrString = reinterpret_cast<decltype(p_PyObject_GetAttrString)>(GetProcAddress(pythonModule, "PyObject_GetAttrString"));
//		p_PyObject_CallObject = reinterpret_cast<decltype(p_PyObject_CallObject)>(GetProcAddress(pythonModule, "PyObject_CallObject"));
//		p_PyLong_AsLong = reinterpret_cast<decltype(p_PyLong_AsLong)>(GetProcAddress(pythonModule, "PyLong_AsLong"));
//		p_Py_DecRef = reinterpret_cast<decltype(p_Py_DecRef)>(GetProcAddress(pythonModule, "Py_DecRef"));
//		p_PyTuple_Size = reinterpret_cast<decltype(p_PyTuple_Size)>(GetProcAddress(pythonModule, "PyTuple_Size"));
//		p_PyTuple_GetItem = reinterpret_cast<decltype(p_PyTuple_GetItem)>(GetProcAddress(pythonModule, "PyTuple_GetItem"));
//		p_PyFloat_AsDouble = reinterpret_cast<decltype(p_PyFloat_AsDouble)>(GetProcAddress(pythonModule, "PyFloat_AsDouble"));
//		p_PyTuple_New = reinterpret_cast<decltype(p_PyTuple_New)>(GetProcAddress(pythonModule, "PyTuple_New"));
//		p_PyTuple_SetItem = reinterpret_cast<decltype(p_PyTuple_SetItem)>(GetProcAddress(pythonModule, "PyTuple_SetItem"));
//		p_PyLong_FromLong = reinterpret_cast<decltype(p_PyLong_FromLong)>(GetProcAddress(pythonModule, "PyLong_FromLong"));
//
//		if (!p_PyGILState_Ensure || !p_PyGILState_Release || !p_PyImport_ImportModule ||
//			!p_PyObject_GetAttrString || !p_PyObject_CallObject || !p_Py_DecRef || !p_PyTuple_Size ||
//			!p_PyTuple_GetItem || !p_PyFloat_AsDouble || !p_PyTuple_New || !p_PyTuple_SetItem || !p_PyLong_FromLong)
//		{
//			throw Exception("export gg!");
//		}
//	}
//};
//namespace engsdk
//{
//	double PythonPlayerPickCloseItem(python27& py, double eh, const char* Module, const char* Name) {
//		void* result = nullptr;
//		try {
//			result = py.Function(Module, Name);
//		}
//		catch (const Exception& e) {
//			return eh;
//		}
//		if (!result) return eh;
//		void* GIL = py.AcquireGIL();
//		eh = py.FloatAsDouble(result);
//		py.ReleaseGIL(GIL);
//		py.DecRef(result);
//		return eh;
//	}
//}
//
//DWORD WINAPI engine_packet(LPVOID lpParam) {
//	HMODULE py27 = LoadLibraryA("python27.dll");
//	if (!py27) {
//		DWORD Error = GetLastError();
//		char buf[256] = { 0 };
//		return 1;
//	}
//
//	try {
//		python27 py(py27);
//
//		while (true)
//		{
//			engsdk::PythonPlayerPickCloseItem(py, 10.10, "player", "PickCloseItem");
//		}
//	}
//	catch (const Exception& ex) {
//		std::cout << "gg!: " << ex.what() << std::endl;
//	}
//
//	FreeLibrary(py27);
//	FreeConsole();
//	return 0;
//}


//bool One = false;
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wparam, LPARAM lparam) {
//	MSG* msg = reinterpret_cast<MSG*>(lparam);
//
//	if (!One && msg && msg->message == (WM_USER + 432)) {
//		One = true;
//		HMODULE hModule = nullptr;
//		if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)&ViewMatrix, &hModule)) {
//			return CallNextHookEx(0, code, wparam, lparam);
//		}
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(100, 100))
//		{
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)D3Dimgui, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CheckerWeb, 0, 0, 0);
//				//		AntiCheat::Thread((void*)D3Dimgui, hModule);
//	
//				////AntiCheat::Thread((void*)MultipleGame, hModule);
//	
//				//AntiCheat::Thread((void*)Set_CheckerWeb, hModule);
//	
//				////AntiCheat::Thread((void*)MemoryManager, hModule);
//		}
//	}
//
//	return CallNextHookEx(0, code, wparam, lparam);
//}

DWORD WINAPI ApiPipe(HMODULE hModule)
{
	while (true)
	{
		HANDLE hPipe = CreateFile(PIPE_NAME, GENERIC_READ, 0, NULL, OPEN_EXISTING, 0, NULL);
		if (hPipe == INVALID_HANDLE_VALUE) {
			continue;
		}
		static int value;
		DWORD bytesRead = 0;
		if (ReadFile(hPipe, &value, sizeof(int), &bytesRead, NULL)) {
			server_selectors = value;
			CloseHandle(hPipe);
			AntiCheat::Thread((void*)D3Dimgui, hModule);
			//AntiCheat::Thread((void*)MultipleGame, hModule);
			AntiCheat::Thread((void*)MemoryManager, hModule);
			AntiCheat::Thread((void*)Set_AutoPickUps, hModule);
			AntiCheat::Thread((void*)Set_AutoPelerin, hModule);
			AntiCheat::Thread((void*)Set_Blacksmith, hModule);
			AntiCheat::Thread((void*)Set_ChatSpawmner, hModule);
			AntiCheat::Thread((void*)Set_NameChanger, hModule);
			AntiCheat::Thread((void*)Set_WaitRange, hModule);
			AntiCheat::Thread((void*)Set_WaitDamage, hModule);
			AntiCheat::Thread((void*)Set_Item_Inventory, hModule);
			AntiCheat::Thread((void*)Set_Collision, hModule);
			AntiCheat::Thread((void*)Set_AntiFly, hModule);
			AntiCheat::Thread((void*)Set_MoveSpeedBoost, hModule);
			AntiCheat::Thread((void*)Set_SetMaxDistance, hModule);
			AntiCheat::Thread((void*)Set_ComboAttack, hModule);
			AntiCheat::Thread((void*)Set_Rota, hModule);
			AntiCheat::Thread((void*)Set_AllFarmbot, hModule);
			AntiCheat::Thread((void*)Set_Skills, hModule);
			AntiCheat::Thread((void*)Set_SkillsV2, hModule);
			AntiCheat::Thread((void*)Set_AutoRevive, hModule);
			AntiCheat::Thread((void*)Set_DonateLonca, hModule);
			AntiCheat::Thread((void*)Set_Affect, hModule);
			AntiCheat::Thread((void*)Set_AntiBans, hModule);
			AntiCheat::Thread((void*)Set_CHChanger, hModule);
			AntiCheat::Thread((void*)Set_ClickTeleport, hModule);
			AntiCheat::Thread((void*)Set_StrongBody, hModule);
			AntiCheat::Thread((void*)Set_Hitting, hModule);
			AntiCheat::Thread((void*)Set_BuffSysteam, hModule);
			break;
		}
	}
	return 0;
}


static bool initialized = false;

extern "C" BOOL APIENTRY DllMain(HINSTANCE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	{
		if (initialized) return TRUE; // 🔥 tekrar çalışmayı engelle
		initialized = true;

		DisableThreadLibraryCalls(hModule);

		AntiDebug::HideThread(hModule);
		AntiCheat::Module(hModule);
		AntiCheat::Sections(hModule);
		AntiCheat::Destroy(hModule);
		AntiCheat::Hidden(hModule);
		AntiCheat::Anti_Cheat(hModule);
		AntiCheat::EraseHeaders(hModule);
		AntiCheat::HideModule(hModule);

		if (Beep(200, 200))
		{
			//AntiCheat::Thread((void*)ApiPipe, hModule);

			AntiCheat::Thread((void*)D3Dimgui, hModule);
			//AntiCheat::Thread((void*)MultipleGame, hModule);
			AntiCheat::Thread((void*)MemoryManager, hModule);
			AntiCheat::Thread((void*)Set_AutoPickUps, hModule);
			AntiCheat::Thread((void*)Set_AutoPelerin, hModule);
			AntiCheat::Thread((void*)Set_Blacksmith, hModule);
			AntiCheat::Thread((void*)Set_ChatSpawmner, hModule);
			AntiCheat::Thread((void*)Set_NameChanger, hModule);
			AntiCheat::Thread((void*)Set_WaitRange, hModule);
			AntiCheat::Thread((void*)Set_WaitDamage, hModule);
			AntiCheat::Thread((void*)Set_Item_Inventory, hModule);
			AntiCheat::Thread((void*)Set_Collision, hModule);
			AntiCheat::Thread((void*)Set_AntiFly, hModule);
			AntiCheat::Thread((void*)Set_MoveSpeedBoost, hModule);
			AntiCheat::Thread((void*)Set_SetMaxDistance, hModule);
			AntiCheat::Thread((void*)Set_ComboAttack, hModule);
			AntiCheat::Thread((void*)Set_Rota, hModule);
			AntiCheat::Thread((void*)Set_AllFarmbot, hModule);
			AntiCheat::Thread((void*)Set_Skills, hModule);
			AntiCheat::Thread((void*)Set_SkillsV2, hModule);
			AntiCheat::Thread((void*)Set_AutoRevive, hModule);
			AntiCheat::Thread((void*)Set_DonateLonca, hModule);
			AntiCheat::Thread((void*)Set_Affect, hModule);
			AntiCheat::Thread((void*)Set_AntiBans, hModule);
			AntiCheat::Thread((void*)Set_CHChanger, hModule);
			AntiCheat::Thread((void*)Set_ClickTeleport, hModule);
			AntiCheat::Thread((void*)Set_StrongBody, hModule);
			AntiCheat::Thread((void*)Set_Hitting, hModule);
			AntiCheat::Thread((void*)Set_BuffSysteam, hModule);
		}
	}
	return TRUE;
}

//extern "C" BOOL APIENTRY DllMain(HINSTANCE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
//{
//	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
//	{
//		AntiDebug::HideThread(hModule);
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(200, 200))
//		{
//			AntiCheat::Thread((void*)ApiPipe, hModule);
//			//AntiCheat::Thread((void*)MultipleGame, hModule);
//			//AntiCheat::Thread((void*)D3Dimgui, hModule);
//			//AntiCheat::Thread((void*)MemoryManager, hModule);
//			//AntiCheat::Thread((void*)Set_AutoPickUps, hModule);
//			//AntiCheat::Thread((void*)Set_AutoPelerin, hModule);
//			//AntiCheat::Thread((void*)Set_Blacksmith, hModule);
//			//AntiCheat::Thread((void*)Set_ChatSpawmner, hModule);
//			//AntiCheat::Thread((void*)Set_NameChanger, hModule);
//			//AntiCheat::Thread((void*)Set_WaitRange, hModule);
//			//AntiCheat::Thread((void*)Set_WaitDamage, hModule);
//			//AntiCheat::Thread((void*)Set_Item_Inventory, hModule); // degil
//			//AntiCheat::Thread((void*)Set_Collision, hModule);
//			//AntiCheat::Thread((void*)Set_AntiFly, hModule);
//			//AntiCheat::Thread((void*)Set_MoveSpeedBoost, hModule);
//			//AntiCheat::Thread((void*)Set_SetMaxDistance, hModule);
//			//AntiCheat::Thread((void*)Set_ComboAttack, hModule);
//			//AntiCheat::Thread((void*)Set_Rota, hModule);
//			////AntiCheat::Thread((void*)Set_AllFarmbot, hModule);
//			//AntiCheat::Thread((void*)Set_Skills, hModule);
//			//AntiCheat::Thread((void*)Set_SkillsV2, hModule);
//			//AntiCheat::Thread((void*)Set_AutoRevive, hModule);
//			//AntiCheat::Thread((void*)Set_DonateLonca, hModule);
//			//AntiCheat::Thread((void*)Set_Affect, hModule);
//			//AntiCheat::Thread((void*)Set_AntiBans, hModule);
//			//AntiCheat::Thread((void*)Set_CHChanger, hModule);
//			//AntiCheat::Thread((void*)Set_ClickTeleport, hModule);
//			//AntiCheat::Thread((void*)Set_StrongBody, hModule);
//			//AntiCheat::Thread((void*)Set_Hitting, hModule);
//			//AntiCheat::Thread((void*)Set_BuffSysteam, hModule);
//
//		}
//	}
//	return TRUE;
//}
//
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wParam, LPARAM lParam) {
//	return CallNextHookEx(NULL, code, wParam, lParam);
//}

//extern "C" BOOL APIENTRY DllMain(HINSTANCE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
//{
//	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
//	{
//		AntiDebug::HideThread(hModule);
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(200, 200))
//		{
//			// CheatBlocker
//			//Core::Initialize();
//			//CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Core::Initialize, NULL, 0, NULL);
//			//AntiCheat::Thread((void*)Core::Initialize, hModule);
//			AntiCheat::Thread((void*)D3Dimgui, hModule);
//			AntiCheat::Thread((void*)ApiPipe, hModule); 
//				AntiCheat::Thread((void*)dungeona, hModule);
//			
//
//			//AntiCheat::Thread((void*)MultipleGame, hModule);
//
//			//AntiCheat::Thread((void*)Set_CheckerWeb, hModule);
//
//			//AntiCheat::Thread((void*)MemoryManager, hModule);
//			//// ============================================== Packet ============================================== //
//
//			//AntiCheat::Thread((void*)Set_AutoPickUps, hModule);
//			//AntiCheat::Thread((void*)Set_AutoPelerin, hModule);
//			//AntiCheat::Thread((void*)Set_Blacksmith, hModule);
//
//			//AntiCheat::Thread((void*)Set_ChatSpawmner, hModule);
//			//AntiCheat::Thread((void*)Set_NameChanger, hModule);
//			//AntiCheat::Thread((void*)Set_WaitRange, hModule);
//			//AntiCheat::Thread((void*)Set_WaitDamage, hModule);
//			//AntiCheat::Thread((void*)Set_Item_Inventory, hModule);
//			//AntiCheat::Thread((void*)Set_Collision, hModule);
//			//AntiCheat::Thread((void*)Set_AntiFly, hModule);
//			//AntiCheat::Thread((void*)Set_MoveSpeedBoost, hModule);
//
//			//// ============================================== Packet ============================================== //
//			//AntiCheat::Thread((void*)Set_SetMaxDistance, hModule);
//			//AntiCheat::Thread((void*)Set_ComboAttack, hModule);
//			//AntiCheat::Thread((void*)Set_Rota, hModule);
//
//
//			//AntiCheat::Thread((void*)Set_AllFarmbot, hModule);
//			//AntiCheat::Thread((void*)Set_Skills, hModule);
//			//AntiCheat::Thread((void*)Set_SkillsV2, hModule);
//			//AntiCheat::Thread((void*)Set_AutoRevive, hModule);
//			//AntiCheat::Thread((void*)Set_DonateLonca, hModule);
//			//AntiCheat::Thread((void*)Set_Affect, hModule);
//
//			//AntiCheat::Thread((void*)Set_AntiBans, hModule);
//
//			//// ============================================== WS Cheats ============================================== //
//
//			//AntiCheat::Thread((void*)Set_CHChanger, hModule);
//			//AntiCheat::Thread((void*)Set_CHChanger, hModule);
//
//			//AntiCheat::Thread((void*)Set_ClickTeleport, hModule);
//			//AntiCheat::Thread((void*)Set_StrongBody, hModule);
//			//AntiCheat::Thread((void*)Set_Hitting, hModule);
//			//AntiCheat::Thread((void*)Set_BuffSysteam, hModule);
//
//			////AntiCheat::Thread((void*)adwadwadadawd21231312321, hModule);
//
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)dungeona, 0, 0, 0);
//
//			//============================================== Python ============================================== //
//			//char modulePath[MAX_PATH];
//			//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//			//	std::string path(modulePath);
//			//	size_t pos = path.find_last_of("\\/");
//			//	if (pos != std::string::npos) {
//			//		path = path.substr(0, pos);
//			//		path += "\\python27.dll";
//
//			//		HMODULE python = LoadLibraryA(path.c_str());
//			//		if (!python) {
//			//			return FALSE;
//			//		}
//			//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//			//	}
//			//}
//		}
//	}
//	return TRUE;
//}
//
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wParam, LPARAM lParam) {
//	return CallNextHookEx(NULL, code, wParam, lParam);
//}


//bool One = false;
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wparam, LPARAM lparam) {
//	MSG* msg = reinterpret_cast<MSG*>(lparam);
//
//	if (!One && msg && msg->message == (WM_USER + 432)) {
//		One = true;
//		HMODULE hModule = nullptr;
//		if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)&ViewMatrix, &hModule)) {
//			return CallNextHookEx(0, code, wparam, lparam);
//		}
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(100, 100))
//		{
//			AntiCheat::Thread((void*)D3Dimgui, hModule);
//
//			//AntiCheat::Thread((void*)MultipleGame, hModule);
//
//			AntiCheat::Thread((void*)Set_CheckerWeb, hModule);
//
//			AntiCheat::Thread((void*)MemoryManager, hModule);
//			// ============================================== Packet ============================================== //
//
//			AntiCheat::Thread((void*)Set_AutoPickUps, hModule);
//			AntiCheat::Thread((void*)Set_AutoPelerin, hModule);
//
//			AntiCheat::Thread((void*)Set_ChatSpawmner, hModule);
//			AntiCheat::Thread((void*)Set_NameChanger, hModule);
//			AntiCheat::Thread((void*)Set_WaitRange, hModule);
//			AntiCheat::Thread((void*)Set_WaitDamage, hModule);
//			AntiCheat::Thread((void*)Set_Item_Inventory, hModule);
//			AntiCheat::Thread((void*)Set_Collision, hModule);
//			AntiCheat::Thread((void*)Set_AntiFly, hModule);
//			AntiCheat::Thread((void*)Set_MoveSpeedBoost, hModule);
//
//			// ============================================== Packet ============================================== //
//			AntiCheat::Thread((void*)Set_SetMaxDistance, hModule);
//			AntiCheat::Thread((void*)Set_ComboAttack, hModule);
//			AntiCheat::Thread((void*)Set_Rota, hModule);
//
//
//			AntiCheat::Thread((void*)Set_AllFarmbot, hModule);
//			AntiCheat::Thread((void*)Set_Skills, hModule);
//			AntiCheat::Thread((void*)Set_SkillsV2, hModule);
//			AntiCheat::Thread((void*)Set_AutoRevive, hModule);
//			AntiCheat::Thread((void*)Set_DonateLonca, hModule);
//			AntiCheat::Thread((void*)Set_Affect, hModule);
//
//			AntiCheat::Thread((void*)Set_AntiBans, hModule);
//
//			// ============================================== WS Cheats ============================================== //
//
//			AntiCheat::Thread((void*)Set_CHChanger, hModule);
//			AntiCheat::Thread((void*)Set_CHChanger, hModule);
//
//			AntiCheat::Thread((void*)Set_ClickTeleport, hModule);
//			AntiCheat::Thread((void*)Set_StrongBody, hModule);
//			AntiCheat::Thread((void*)Set_Hitting, hModule);
//
//
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)dungeona, 0, 0, 0);
//
//			//============================================== Python ============================================== //
//			//char modulePath[MAX_PATH];
//			//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//			//	std::string path(modulePath);
//			//	size_t pos = path.find_last_of("\\/");
//			//	if (pos != std::string::npos) {
//			//		path = path.substr(0, pos);
//			//		path += "\\python27.dll";
//
//			//		HMODULE python = LoadLibraryA(path.c_str());
//			//		if (!python) {
//			//			return FALSE;
//			//		}
//			//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//			//	}
//			//}
//		}
//	}
//
//	return CallNextHookEx(0, code, wparam, lparam);
//}



//bool One = false;
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wparam, LPARAM lparam) {
//	MSG* msg = reinterpret_cast<MSG*>(lparam);
//
//	if (!One && msg && msg->message == (WM_USER + 432)) {
//		One = true;
//		HMODULE hModule = nullptr;
//		if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)&ViewMatrix, &hModule)) {
//			return CallNextHookEx(0, code, wparam, lparam);
//		}
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(100, 100))
//		{
//			CreateThread(0, 0, D3Dimgui, 0, 0, 0); //init our hooks
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CheckerWeb, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MemoryManager, 0, 0, 0);
//
//			// ============================================== Packet ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPickUps, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPelerin, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ChatSpawmner, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_NameChanger, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitRange, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitDamage, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Item_Inventory, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Collision, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiFly, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_MoveSpeedBoost, 0, 0, 0);
//
//			// ============================================== Packet ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SetMaxDistance, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ComboAttack, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Rota, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbot, 0, 0, 0);
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbotV2, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Skills, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SkillsV2, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoRevive, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_DonateLonca, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Affect, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiBans, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChanger, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChangerTime, 0, 0, 0);
//
//			// ============================================== WS Cheats ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ClickTeleport, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_StrongBody, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Hitting, 0, 0, 0);
//
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)test, 0, 0, 0);
//
//			//============================================== Python ============================================== //
//			//char modulePath[MAX_PATH];
//			//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//			//	std::string path(modulePath);
//			//	size_t pos = path.find_last_of("\\/");
//			//	if (pos != std::string::npos) {
//			//		path = path.substr(0, pos);
//			//		path += "\\python27.dll";
//
//			//		HMODULE python = LoadLibraryA(path.c_str());
//			//		if (!python) {
//			//			return FALSE;
//			//		}
//			//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//			//	}
//			//}
//			
//		}
//	}
//
//	return CallNextHookEx(0, code, wparam, lparam);
//}


//extern "C" BOOL APIENTRY DllMain(HINSTANCE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
//{
//
//	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
//	{
//		AntiCheat::Threads::HideThread(hModule);
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(200, 200))
//		{
//			
//		}
//	}
//	return TRUE;
//}
//extern "C" __declspec(dllexport) int ViewMatrix(int code, WPARAM wParam, LPARAM lParam) {
//	return CallNextHookEx(NULL, code, wParam, lParam);
//}
//
//
//bool One = false;
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wparam, LPARAM lparam) {
//	MSG* msg = reinterpret_cast<MSG*>(lparam);
//
//	if (!One && msg && msg->message == (WM_USER + 432)) {
//		One = true;
//		HMODULE hModule = nullptr;
//		if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)&ViewMatrix, &hModule)) {
//			return CallNextHookEx(0, code, wparam, lparam);
//		}
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(100, 100))
//		{
//			CreateThread(0, 0, D3Dimgui, 0, 0, 0); //init our hooks
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CheckerWeb, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MemoryManager, 0, 0, 0);
//
//			// ============================================== Packet ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPickUps, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPelerin, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ChatSpawmner, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_NameChanger, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitRange, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitDamage, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Item_Inventory, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Collision, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiFly, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_MoveSpeedBoost, 0, 0, 0);
//
//			// ============================================== Packet ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SetMaxDistance, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ComboAttack, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Rota, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbot, 0, 0, 0);
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbotV2, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Skills, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SkillsV2, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoRevive, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_DonateLonca, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Affect, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiBans, 0, 0, 0);
//
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChanger, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChangerTime, 0, 0, 0);
//
//			// ============================================== WS Cheats ============================================== //
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ClickTeleport, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_StrongBody, 0, 0, 0);
//			CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Hitting, 0, 0, 0);
//
//			//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)test, 0, 0, 0);
//
//			//============================================== Python ============================================== //
//			//char modulePath[MAX_PATH];
//			//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//			//	std::string path(modulePath);
//			//	size_t pos = path.find_last_of("\\/");
//			//	if (pos != std::string::npos) {
//			//		path = path.substr(0, pos);
//			//		path += "\\python27.dll";
//
//			//		HMODULE python = LoadLibraryA(path.c_str());
//			//		if (!python) {
//			//			return FALSE;
//			//		}
//			//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//			//	}
//			//}
//		}
//	}
//
//	return CallNextHookEx(0, code, wparam, lparam);
//}



//bool One = false;
//extern "C" __declspec(dllexport) LRESULT CALLBACK ViewMatrix(int code, WPARAM wparam, LPARAM lparam) {
//	MSG* msg = reinterpret_cast<MSG*>(lparam);
//
//	if (!One && msg && msg->message == (WM_USER + 432)) {
//		One = true;
//		HMODULE hModule = nullptr;
//		if (!GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)&ViewMatrix, &hModule)) {
//			return CallNextHookEx(0, code, wparam, lparam);
//		}
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//		if (Beep(100, 100))
//		{
//			//CreateThread(0, 0, D3Dimgui, 0, 0, 0); 
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CheckerWeb, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MemoryManager, 0, 0, 0);
//
//		//// ============================================== Packet ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPickUps, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPelerin, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ChatSpawmner, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_NameChanger, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitRange, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitDamage, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Item_Inventory, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Collision, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiFly, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_MoveSpeedBoost, 0, 0, 0);
//
//		//// ============================================== Packet ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SetMaxDistance, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ComboAttack, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Rota, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbot, 0, 0, 0);
//		////CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbotV2, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Skills, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SkillsV2, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoRevive, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_DonateLonca, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Affect, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiBans, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChanger, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChangerTime, 0, 0, 0);
//
//		//// ============================================== WS Cheats ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ClickTeleport, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_StrongBody, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Hitting, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)test, 0, 0, 0);
//
//		//============================================== Python ============================================== //
//		//char modulePath[MAX_PATH];
//		//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//		//	std::string path(modulePath);
//		//	size_t pos = path.find_last_of("\\/");
//		//	if (pos != std::string::npos) {
//		//		path = path.substr(0, pos);
//		//		path += "\\python27.dll";
//
//		//		HMODULE python = LoadLibraryA(path.c_str());
//		//		if (!python) {
//		//			return FALSE;
//		//		}
//		//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//		//	}
//		//}
//		}
//	}
//
//	return CallNextHookEx(0, code, wparam, lparam);
//}
//
//
//
//BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
//{
//	switch (ul_reason_for_call)
//	{
//	case DLL_PROCESS_ATTACH:
//		Hand = hModule;
//		AntiCheat::Threads::HideThread(hModule);
//		AntiCheat::Module(hModule);
//		AntiCheat::Sections(hModule);
//		AntiCheat::Destroy(hModule);
//		AntiCheat::Hidden(hModule);
//		DisableThreadLibraryCalls(hModule);
//		AntiCheat::Anti_Cheat(hModule);
//		AntiCheat::EraseHeaders(hModule);
//		AntiCheat::HideModule(hModule);
//
//		CreateThread(0, 0, D3Dimgui, 0, 0, 0); //init our hooks
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CheckerWeb, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MemoryManager, 0, 0, 0);
//
//		//// ============================================== Packet ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPickUps, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoPelerin, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ChatSpawmner, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_NameChanger, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitRange, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_WaitDamage, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Item_Inventory, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Collision, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiFly, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_MoveSpeedBoost, 0, 0, 0);
//
//		//// ============================================== Packet ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SetMaxDistance, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ComboAttack, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Rota, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbot, 0, 0, 0);
//		////CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AllFarmbotV2, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Skills, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_SkillsV2, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AutoRevive, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_DonateLonca, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Affect, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_AntiBans, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChanger, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_CHChangerTime, 0, 0, 0);
//
//		//// ============================================== WS Cheats ============================================== //
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_ClickTeleport, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_StrongBody, 0, 0, 0);
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Set_Hitting, 0, 0, 0);
//
//		//CreateThread(0, 0, (LPTHREAD_START_ROUTINE)test, 0, 0, 0);
//
//		//============================================== Python ============================================== //
//		//char modulePath[MAX_PATH];
//		//if (GetModuleFileNameA(NULL, modulePath, MAX_PATH)) {
//		//	std::string path(modulePath);
//		//	size_t pos = path.find_last_of("\\/");
//		//	if (pos != std::string::npos) {
//		//		path = path.substr(0, pos);
//		//		path += "\\python27.dll";
//
//		//		HMODULE python = LoadLibraryA(path.c_str());
//		//		if (!python) {
//		//			return FALSE;
//		//		}
//		//		//CreateThread(NULL, 0, python_script, python, 0, NULL);
//		//	}
//		//}
//		break;
//	case DLL_PROCESS_DETACH:
//		// Uninitialize wndProc
//		//SetWindowLongPtr(game_hwnd, GWLP_WNDPROC, reinterpret_cast<LONG>(WndProc));
//		// Uninitialize ImGui
//		//ImGui_ImplWin32_Shutdown();
//		//ImGui_ImplDX9_Shutdown();
//		//ImGui::DestroyContext();
//		break;
//	case DLL_THREAD_ATTACH:
//	case DLL_THREAD_DETACH:
//		break;
//	}
//	return TRUE;
//}



