#pragma once


static int ModeSelect = 0;
ImFont* Ariel;
ImFont* Low;
ImFont* Blood;
ImFont* Windows;

static int OnlyDigits(ImGuiInputTextCallbackData* data)
{
	if (data->EventChar == 0) return 0;
	if (data->EventChar >= '0' && data->EventChar <= '9') return 0;
	if (data->EventChar == '\n' || data->EventChar == '\r') return 1;
	return 1;
}

namespace UI
{

	bool HLListBoxIcon(const char* id, ImVec2 localPos, int width, int height,const char** items, int items_count, int* current_item, ImFont* Low,ImTextureID icon1Tex = nullptr,ImTextureID scrollGrabTex = nullptr,ImTextureID arrowUpTex = nullptr,ImTextureID arrowDownTex = nullptr,const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(18, 19, 22), 0.0f); // Background

		float scrollbarWidth = 10.0f;
		float arrowSize = 12.0f;

		float scrollbarX = pos.x + size.x - scrollbarWidth - 2;
		draw->AddRectFilled(
			ImVec2(scrollbarX - 2, pos.y + 2),
			ImVec2(scrollbarX + scrollbarWidth + 2, pos.y + size.y - 2),
			ImColor(30, 30, 30, 255),
			0.0f
		);

		static std::map<ImGuiID, float> scrollMap;
		if (scrollMap.find(iid) == scrollMap.end())
			scrollMap[iid] = 0.0f;
		float& manualScrollY = scrollMap[iid];

		float itemHeight = 30.0f;
		float iconSize = 18.0f;
		float iconsArea = icon1Tex ? (iconSize + 6.0f) : 0.0f;

		float totalHeight = itemHeight * items_count;
		float visibleHeight = size.y;
		float contentWidth = size.x - scrollbarWidth - 8 - iconsArea;
		float maxScroll = ImMax(0.0f, totalHeight - visibleHeight);

		bool changed = false;
		ImGui::PushFont(Low);

		draw->PushClipRect(pos, ImVec2(pos.x + size.x - scrollbarWidth - 4, pos.y + size.y), true);

		for (int i = 0; i < items_count; i++)
		{
			ImGui::PushID(i);

			float itemY = pos.y + (i * itemHeight) - manualScrollY;

			if (itemY + itemHeight < pos.y || itemY > pos.y + size.y)
			{
				ImGui::PopID();
				continue;
			}

			ImVec2 itemPos = ImVec2(pos.x + 4, itemY);
			ImVec2 itemSize = ImVec2(contentWidth, itemHeight);

			bool hovered = ImGui::IsMouseHoveringRect(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y));
			bool selected = (*current_item == i);

			ImU32 bgColor = ImColor(29, 29, 29); // Arka Plan
			draw->AddRectFilled(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y), bgColor, 0.0f);

			if (i < items_count - 1)
			{
				draw->AddLine(
					ImVec2(itemPos.x, itemPos.y + itemSize.y),
					ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y),
					ImColor(35, 35, 35, 255),
					1.0f
				);
			}

			if (selected)
			{
				draw->AddRectFilled(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y), ImColor(25, 25, 25), 0.0f);

				float arrowY = itemPos.y + itemSize.y * 0.5f;
				draw->AddTriangleFilled(
					ImVec2(itemPos.x + 6, arrowY),
					ImVec2(itemPos.x + 10, arrowY - 3),
					ImVec2(itemPos.x + 10, arrowY + 3),
					ImColor(255, 255, 255)
				);
				draw->AddTriangleFilled(
					ImVec2(itemPos.x + itemSize.x - 6, arrowY),
					ImVec2(itemPos.x + itemSize.x - 10, arrowY - 3),
					ImVec2(itemPos.x + itemSize.x - 10, arrowY + 3),
					ImColor(200, 200, 200, 255)
				);
			}

			ImVec2 txtSize = ImGui::CalcTextSize(items[i]);
			float text_y = itemPos.y + (itemSize.y - txtSize.y) * 0.5f;
			float text_x = selected ? itemPos.x + 16 : itemPos.x + 8;
			draw->AddText(ImVec2(text_x, text_y), IM_COL32(200, 200, 200, 255), items[i]);


			if (icon1Tex)
			{
				float iconDrawSize = 18.0f; 
				float iconX = itemPos.x + contentWidth + 3.0f;
				float iconY = itemPos.y + (itemHeight - iconDrawSize) * 0.5f;

				ImVec2 mn = ImVec2(iconX, iconY);
				ImVec2 mx = ImVec2(iconX + iconDrawSize, iconY + iconDrawSize);
				bool hov = ImGui::IsMouseHoveringRect(mn, mx);


				draw->AddRectFilled(
					ImVec2(itemPos.x + contentWidth, itemPos.y),
					ImVec2(itemPos.x + contentWidth + iconsArea, itemPos.y + itemHeight),
					selected ? ImColor(25, 25, 25) : ImColor(29, 29, 29), 0.0f);

				draw->AddImage(icon1Tex, mn, mx, ImVec2(0, 0), ImVec2(1, 1),
					hov ? IM_COL32(255, 255, 255, 255) : IM_COL32(255, 255, 255, 200));

				if (hov && ImGui::IsMouseClicked(0))
					MessageBoxA(NULL, items[i], "Selected", MB_OK | MB_ICONINFORMATION);
			}


			if (hovered && ImGui::IsMouseClicked(0))
			{
				*current_item = i;
				changed = true;
			}

			ImGui::PopID();
		}

		draw->PopClipRect();
		ImGui::PopFont();

		float scrollbarHeight = size.y - (arrowSize * 2) - 4;
		float scrollbarRatio = ImMin(1.0f, visibleHeight / totalHeight);
		float scrollbarGrabHeight = ImMax(20.0f, scrollbarHeight * scrollbarRatio);
		float scrollRatio = maxScroll > 0 ? (manualScrollY / maxScroll) : 0.0f;
		float scrollbarGrabY = pos.y + arrowSize + 2 + (scrollbarHeight - scrollbarGrabHeight) * scrollRatio;


		ImVec2 upArrowPos = ImVec2(scrollbarX, pos.y + 2);
		ImRect upArrowRect(upArrowPos, ImVec2(upArrowPos.x + scrollbarWidth, upArrowPos.y + arrowSize));
		bool upHovered = ImGui::IsMouseHoveringRect(upArrowRect.Min, upArrowRect.Max);

		if (arrowUpTex)
		{
			draw->AddImage(arrowUpTex, upArrowRect.Min, upArrowRect.Max, ImVec2(0, 0), ImVec2(1, 1),
				upHovered ? IM_COL32(255, 255, 255, 255) : IM_COL32(180, 180, 180, 200));
		}
		else
		{
			draw->AddRectFilled(upArrowRect.Min, upArrowRect.Max, upHovered ? ImColor(60, 60, 60, 255) : ImColor(45, 45, 45, 255), 0.0f);
			ImVec2 upCenter = ImVec2(scrollbarX + scrollbarWidth * 0.5f, pos.y + 2 + arrowSize * 0.5f);
			draw->AddTriangleFilled(
				ImVec2(upCenter.x, upCenter.y - 3),
				ImVec2(upCenter.x - 3, upCenter.y + 2),
				ImVec2(upCenter.x + 3, upCenter.y + 2),
				ImColor(150, 150, 150, 255)
			);
		}

		if (upHovered && ImGui::IsMouseClicked(0))
			manualScrollY = ImMax(0.0f, manualScrollY - itemHeight);


		ImVec2 downArrowPos = ImVec2(scrollbarX, pos.y + size.y - arrowSize - 2);
		ImRect downArrowRect(downArrowPos, ImVec2(downArrowPos.x + scrollbarWidth, downArrowPos.y + arrowSize));
		bool downHovered = ImGui::IsMouseHoveringRect(downArrowRect.Min, downArrowRect.Max);

		if (arrowDownTex)
		{
			draw->AddImage(arrowDownTex, downArrowRect.Min, downArrowRect.Max, ImVec2(0, 0), ImVec2(1, 1),
				downHovered ? IM_COL32(255, 255, 255, 255) : IM_COL32(180, 180, 180, 200));
		}
		else
		{
			draw->AddRectFilled(downArrowRect.Min, downArrowRect.Max, downHovered ? ImColor(60, 60, 60, 255) : ImColor(45, 45, 45, 255), 0.0f);
			ImVec2 downCenter = ImVec2(scrollbarX + scrollbarWidth * 0.5f, downArrowPos.y + arrowSize * 0.5f);
			draw->AddTriangleFilled(
				ImVec2(downCenter.x, downCenter.y + 3),
				ImVec2(downCenter.x - 3, downCenter.y - 2),
				ImVec2(downCenter.x + 3, downCenter.y - 2),
				ImColor(150, 150, 150, 255)
			);
		}

		if (downHovered && ImGui::IsMouseClicked(0))
			manualScrollY = ImMin(maxScroll, manualScrollY + itemHeight);


		if (scrollbarRatio < 1.0f)
		{
			// Listbox sinirini asmamasi icin clamp
			float grabMinY = ImClamp(scrollbarGrabY, pos.y + arrowSize + 2, pos.y + size.y - arrowSize - 2 - scrollbarGrabHeight);
			float grabMaxY = ImMin(grabMinY + scrollbarGrabHeight, pos.y + size.y - arrowSize - 2);

			ImVec2 grabMin = ImVec2(scrollbarX + 1, grabMinY);
			ImVec2 grabMax = ImVec2(scrollbarX + scrollbarWidth - 1, grabMaxY);

			draw->PushClipRect(
				ImVec2(scrollbarX, pos.y + arrowSize + 2),
				ImVec2(scrollbarX + scrollbarWidth, pos.y + size.y - arrowSize - 2),
				true);

			if (scrollGrabTex)
				draw->AddImage(scrollGrabTex, grabMin, grabMax, ImVec2(0, 0), ImVec2(1, 1), IM_COL32(255, 255, 255, 255));
			else
				draw->AddRectFilled(grabMin, grabMax, ImColor(230, 120, 30, 255), 2.0f);

			draw->PopClipRect();

		}

		if (ImGui::IsMouseHoveringRect(pos, ImVec2(pos.x + size.x, pos.y + size.y)))
		{
			float wheel = ImGui::GetIO().MouseWheel;
			if (wheel != 0.0f)
			{
				manualScrollY -= wheel * itemHeight * 2.0f;
				manualScrollY = ImClamp(manualScrollY, 0.0f, maxScroll);
			}
		}

		if (tooltip != nullptr && ImGui::IsMouseHoveringRect(pos, ImVec2(pos.x + size.x, pos.y + size.y)))
			ImGui::SetTooltip("%s", tooltip);

		return changed;
	}

    // ============================================ Digerleri ==========================================================
	
	bool HLListBox(const char* id, ImVec2 localPos, int width, int height, const char** items, int items_count, int* current_item, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(18,19,22), 0.0f); // Background

		float scrollbarWidth = 10.0f;
		float arrowSize = 12.0f;

		float scrollbarX = pos.x + size.x - scrollbarWidth - 2;
		draw->AddRectFilled(
			ImVec2(scrollbarX - 2, pos.y + 2),
			ImVec2(scrollbarX + scrollbarWidth + 2, pos.y + size.y - 2),
			ImColor(30, 30, 30, 255),
			0.0f
		);

		static std::map<ImGuiID, float> scrollMap;
		if (scrollMap.find(iid) == scrollMap.end())
		{
			scrollMap[iid] = 0.0f;
		}
		float& manualScrollY = scrollMap[iid];

		float itemHeight = 26.0f;
		float totalHeight = itemHeight * items_count;
		float visibleHeight = size.y;
		float contentWidth = size.x - scrollbarWidth - 8; 
		float maxScroll = ImMax(0.0f, totalHeight - visibleHeight);

		bool changed = false;
		ImGui::PushFont(Low);

		draw->PushClipRect(pos, ImVec2(pos.x + size.x - scrollbarWidth - 4, pos.y + size.y), true);

		for (int i = 0; i < items_count; i++)
		{
			ImGui::PushID(i);

			float itemY = pos.y + (i * itemHeight) - manualScrollY;

			if (itemY + itemHeight < pos.y || itemY > pos.y + size.y)
			{
				ImGui::PopID();
				continue;
			}

			ImVec2 itemPos = ImVec2(pos.x + 4, itemY);
			ImVec2 itemSize = ImVec2(contentWidth, itemHeight);

			bool hovered = ImGui::IsMouseHoveringRect(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y));
			bool selected = (*current_item == i);

			ImU32 bgColor = ImColor(29,29,29);/// Arka Plan
			draw->AddRectFilled(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y), bgColor, 0.0f);

			if (i < items_count - 1)
			{
				draw->AddLine(
					ImVec2(itemPos.x, itemPos.y + itemSize.y),
					ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y),
					ImColor(35, 35, 35, 255),
					1.0f
				);
			}

			if (selected)
			{
				draw->AddRectFilled(itemPos, ImVec2(itemPos.x + itemSize.x, itemPos.y + itemSize.y), ImColor(25,25,25), 0.0f); // Clicked Color

				float arrowY = itemPos.y + itemSize.y * 0.5f;
				draw->AddTriangleFilled(
					ImVec2(itemPos.x + 6, arrowY),
					ImVec2(itemPos.x + 10, arrowY - 3),
					ImVec2(itemPos.x + 10, arrowY + 3),
					ImColor(255,255,255)
				);

				draw->AddTriangleFilled(
					ImVec2(itemPos.x + itemSize.x - 6, arrowY),
					ImVec2(itemPos.x + itemSize.x - 10, arrowY - 3),
					ImVec2(itemPos.x + itemSize.x - 10, arrowY + 3),
					ImColor(200, 200, 200, 255)
				);
			}

			ImVec2 txtSize = ImGui::CalcTextSize(items[i]);
			float text_y = itemPos.y + (itemSize.y - txtSize.y) * 0.5f;
			float text_x = selected ? itemPos.x + 16 : itemPos.x + 8;
			ImU32 textColor = IM_COL32(200, 200, 200, 255);
			draw->AddText(ImVec2(text_x, text_y), textColor, items[i]);

			if (hovered && ImGui::IsMouseClicked(0))
			{
				*current_item = i;
				changed = true;
			}

			ImGui::PopID();
		}

		draw->PopClipRect();
		ImGui::PopFont();

		float scrollbarHeight = size.y - (arrowSize * 2) - 4;
		float scrollbarRatio = ImMin(1.0f, visibleHeight / totalHeight);
		float scrollbarGrabHeight = ImMax(20.0f, scrollbarHeight * scrollbarRatio);
		float scrollRatio = maxScroll > 0 ? (manualScrollY / maxScroll) : 0.0f;
		float scrollbarGrabY = pos.y + arrowSize + 2 + (scrollbarHeight - scrollbarGrabHeight) * scrollRatio;

		ImVec2 upArrowPos = ImVec2(scrollbarX, pos.y + 2);
		ImRect upArrowRect(upArrowPos, ImVec2(upArrowPos.x + scrollbarWidth, upArrowPos.y + arrowSize));
		bool upHovered = ImGui::IsMouseHoveringRect(upArrowRect.Min, upArrowRect.Max);

		draw->AddRectFilled(upArrowRect.Min, upArrowRect.Max, upHovered ? ImColor(60, 60, 60, 255) : ImColor(45, 45, 45, 255), 0.0f);

		ImVec2 upCenter = ImVec2(scrollbarX + scrollbarWidth * 0.5f, pos.y + 2 + arrowSize * 0.5f);
		draw->AddTriangleFilled(
			ImVec2(upCenter.x, upCenter.y - 3),
			ImVec2(upCenter.x - 3, upCenter.y + 2),
			ImVec2(upCenter.x + 3, upCenter.y + 2),
			ImColor(150, 150, 150, 255)
		);

		if (upHovered && ImGui::IsMouseClicked(0))
		{
			manualScrollY = ImMax(0.0f, manualScrollY - itemHeight);
		}

		ImVec2 downArrowPos = ImVec2(scrollbarX, pos.y + size.y - arrowSize - 2);
		ImRect downArrowRect(downArrowPos, ImVec2(downArrowPos.x + scrollbarWidth, downArrowPos.y + arrowSize));
		bool downHovered = ImGui::IsMouseHoveringRect(downArrowRect.Min, downArrowRect.Max);

		draw->AddRectFilled(downArrowRect.Min, downArrowRect.Max, downHovered ? ImColor(60, 60, 60, 255) : ImColor(45, 45, 45, 255), 0.0f);

		ImVec2 downCenter = ImVec2(scrollbarX + scrollbarWidth * 0.5f, downArrowPos.y + arrowSize * 0.5f);
		draw->AddTriangleFilled(
			ImVec2(downCenter.x, downCenter.y + 3),
			ImVec2(downCenter.x - 3, downCenter.y - 2),
			ImVec2(downCenter.x + 3, downCenter.y - 2),
			ImColor(150, 150, 150, 255)
		);

		if (downHovered && ImGui::IsMouseClicked(0))
		{
			manualScrollY = ImMin(maxScroll, manualScrollY + itemHeight);
		}

		if (scrollbarRatio < 1.0f)
		{
			draw->AddRectFilled(
				ImVec2(scrollbarX + 1, scrollbarGrabY),
				ImVec2(scrollbarX + scrollbarWidth - 1, scrollbarGrabY + scrollbarGrabHeight),
				ImColor(230, 120, 30, 255),
				2.0f
			);
		}

		if (ImGui::IsMouseHoveringRect(pos, ImVec2(pos.x + size.x, pos.y + size.y)))
		{
			float wheel = ImGui::GetIO().MouseWheel;
			if (wheel != 0.0f)
			{
				manualScrollY -= wheel * itemHeight * 2.0f; 
				manualScrollY = ImClamp(manualScrollY, 0.0f, maxScroll);
			}
		}

		if (tooltip != nullptr && ImGui::IsMouseHoveringRect(pos, ImVec2(pos.x + size.x, pos.y + size.y)))
		{
			ImGui::SetTooltip("%s", tooltip);
		}

		return changed;
	}


	// ============================================ Digerleri ==========================================================
	void HLBackground(ImVec2 localPos, int width, int height)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2(width, height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));

		ImDrawList* draw = ImGui::GetWindowDrawList();

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x, pos.y + size.y),
			ImColor(255, 255, 255), 0.0f);

		ImColor col_top = ImColor(3, 3, 3);
		ImColor col_bottom = ImColor(4, 4, 4);
		draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y),
			col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1),
			ImVec2(pos.x + size.x - 1, pos.y + size.y - 1),
			ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		ImU32 topColor = ImColor(192, 137, 126);
		ImU32 bottomColor = ImColor(106, 70, 95);
		float midY = (pos.y + pos.y + size.y) * 0.5f;

		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);


	}
	//orjinal xd
	//void HLNumberBox(const char* id, ImVec2 localPos, int width, int height, int* value, ImFont* Low)
	//{
	//	ImGuiWindow* window = ImGui::GetCurrentWindow();
	//	if (window->SkipItems) return;

	//	ImVec2 winPos = window->Pos;
	//	ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
	//	ImVec2 size = ImVec2((float)width, (float)height);
	//	ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));

	//	ImGui::ItemAdd(bb, ImGui::GetID(id));
	//	ImDrawList* draw = ImGui::GetWindowDrawList();

	//	draw->AddRect(ImVec2(pos.x + 1, pos.y + 1),
	//		ImVec2(pos.x + size.x - 1, pos.y + size.y - 1),
	//		ImColor(0, 0, 0), 0.0f, 0, 1.0f);

	//	ImU32 topColor = ImColor(192, 137, 126);
	//	ImU32 bottomColor = ImColor(106, 70, 95);
	//	float midY = (pos.y + pos.y + size.y) * 0.5f;

	//	 Üst-alt çizgi efekti
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

	//	 Buffer yönetimi
	//	struct Buf { char data[32]; bool init; };
	//	static std::unordered_map<ImGuiID, Buf> s_buffers;
	//	ImGuiID iid = ImGui::GetID(id);
	//	Buf& b = s_buffers[iid];
	//	if (!b.init) { snprintf(b.data, sizeof(b.data), "%d", *value); b.init = true; }


	//	ImGui::SetCursorScreenPos(pos);
	//	ImGui::InvisibleButton((std::string(id) + "_invisible").c_str(), size);
	//	if (ImGui::IsItemClicked()) // sadece tıklayınca
	//		ImGui::SetKeyboardFocusHere();


	//	 InputText alanı
	//	ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
	//	ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);
	//	ImGui::SetCursorScreenPos(inPos);
	//	ImGui::PushItemWidth(inSize.x);

	//	float text_h = ImGui::GetTextLineHeight();
	//	float pad_y = ImMax(0.0f, (inSize.y - text_h) * 0.5f);
	//	ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(6, pad_y));
	//	ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 0.0f);
	//	ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f);

	//	ImGuiInputTextFlags flags = ImGuiInputTextFlags_CallbackCharFilter
	//		| ImGuiInputTextFlags_AutoSelectAll
	//		| ImGuiInputTextFlags_CharsNoBlank;

	//	ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0)); // arka plan siyah
	//	ImGui::PushFont(Low);
	//	bool edited = ImGui::InputText(id, b.data, sizeof(b.data), flags, OnlyDigits);
	//	ImGui::PopFont();
	//	ImGui::PopStyleColor(); // FrameBg

	//	ImGui::PopStyleVar(3);
	//	ImGui::PopItemWidth();

	//	if (edited || ImGui::IsItemDeactivatedAfterEdit())
	//		*value = (b.data[0] ? atoi(b.data) : 0);
	//}

	/*bool HLNumberBoxInt(const char* id, ImVec2 localPos, int width, int height, int* value, ImFont* Low)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		ImU32 topColor = ImColor(152, 152, 151);
		ImU32 bottomColor = ImColor(100, 100, 100);
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		struct Buf { char data[32]; bool init; };
		static std::unordered_map<ImGuiID, Buf> s_buffers;
		Buf& b = s_buffers[iid];
		if (!b.init) { snprintf(b.data, sizeof(b.data), "%d", *value); b.init = true; }

		ImGui::SetCursorScreenPos(pos);
		ImGui::InvisibleButton(id, size);
		if (ImGui::IsItemClicked())
			ImGui::SetKeyboardFocusHere();

		ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
		ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);

		ImGui::SetCursorScreenPos(inPos);
		ImGui::PushItemWidth(inSize.x);
		ImGui::PushFont(Low);

		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));

		char tempID[64]; sprintf(tempID, "%s_input", id);

		ImVec2 txtSize = ImGui::CalcTextSize(b.data);
		float offset_y = (inSize.y - txtSize.y) * 0.5f;
		ImGui::SetCursorScreenPos(ImVec2(inPos.x, inPos.y + offset_y));

		bool edited = ImGui::InputText(tempID, b.data, sizeof(b.data),
			ImGuiInputTextFlags_CharsDecimal | ImGuiInputTextFlags_AutoSelectAll);

		ImGui::PopStyleVar();
		ImGui::PopStyleColor(2);
		ImGui::PopItemWidth();
		ImGui::PopFont();

		if (edited || ImGui::IsItemDeactivatedAfterEdit())
			*value = (b.data[0] ? atoi(b.data) : 0);

		return true;
	}*/

	bool HLTextBox(const char* id, ImVec2 localPos, int width, int height, char* buffer, size_t buffer_size, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		// Border
		ImU32 topColor = ImColor(152, 152, 151);
		ImU32 bottomColor = ImColor(100, 100, 100);
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		// Invisible button & focus
		ImGui::SetCursorScreenPos(pos);
		ImGui::InvisibleButton(id, size);
		if (ImGui::IsItemClicked())
			ImGui::SetKeyboardFocusHere();

		ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
		ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);

		ImGui::SetCursorScreenPos(inPos);
		ImGui::PushItemWidth(inSize.x);
		ImGui::PushFont(Low);

		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));

		char tempID[64]; sprintf(tempID, "%s_input", id);

		// Text vertical align
		ImVec2 txtSize = ImGui::CalcTextSize(buffer);
		float offset_y = (inSize.y - txtSize.y) * 0.5f;
		ImGui::SetCursorScreenPos(ImVec2(inPos.x, inPos.y + offset_y));

		bool edited = ImGui::InputText(tempID, buffer, buffer_size,
			ImGuiInputTextFlags_AutoSelectAll);

		ImGui::PopStyleVar();
		ImGui::PopStyleColor(2);
		ImGui::PopItemWidth();
		ImGui::PopFont();

		// Tooltip
		if (tooltip != nullptr && ImGui::IsItemHovered())
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0, 0, 0, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always);
			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return edited;
	}


	bool HLNumberBoxFloat(const char* id, ImVec2 localPos, int width, int height, float* value, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		// Border
		ImU32 topColor = ImColor(152, 152, 151);
		ImU32 bottomColor = ImColor(100, 100, 100);
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		// Buffer
		struct Buf { char data[32]; bool init; };
		static std::unordered_map<ImGuiID, Buf> s_buffers;
		Buf& b = s_buffers[iid];
		if (!b.init) { snprintf(b.data, sizeof(b.data), "%.3f", *value); b.init = true; }

		// Invisible button ve focus
		ImGui::SetCursorScreenPos(pos);
		ImGui::InvisibleButton(id, size);
		if (ImGui::IsItemClicked())
			ImGui::SetKeyboardFocusHere();

		ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
		ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);

		ImGui::SetCursorScreenPos(inPos);
		ImGui::PushItemWidth(inSize.x);
		ImGui::PushFont(Low);

		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));

		char tempID[64]; sprintf(tempID, "%s_input", id);

		// Calculate vertical offset to center text exactly
		ImVec2 txtSize = ImGui::CalcTextSize(b.data);
		float offset_y = (inSize.y - txtSize.y) * 0.5f;
		ImGui::SetCursorScreenPos(ImVec2(inPos.x, inPos.y + offset_y));

		bool edited = ImGui::InputText(tempID, b.data, sizeof(b.data),
			ImGuiInputTextFlags_CharsDecimal | ImGuiInputTextFlags_AutoSelectAll);

		ImGui::PopStyleVar();
		ImGui::PopStyleColor(2);
		ImGui::PopItemWidth();
		ImGui::PopFont();

		if (edited || ImGui::IsItemDeactivatedAfterEdit())
			*value = (b.data[0] ? static_cast<float>(atof(b.data)) : 0.0f);

		// Tooltip
		if (tooltip != nullptr && ImGui::IsItemHovered())
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0, 0, 0, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always);
			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return true;
	}

	bool HLNumberBoxInt(const char* id, ImVec2 localPos, int width, int height, int* value, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		ImU32 topColor = ImColor(152, 152, 151);
		ImU32 bottomColor = ImColor(100, 100, 100);
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		struct Buf { char data[32]; bool init; };
		static std::unordered_map<ImGuiID, Buf> s_buffers;
		Buf& b = s_buffers[iid];
		if (!b.init) { snprintf(b.data, sizeof(b.data), "%d", *value); b.init = true; }

		ImGui::SetCursorScreenPos(pos);
		ImGui::InvisibleButton(id, size);
		if (ImGui::IsItemClicked())
			ImGui::SetKeyboardFocusHere();

		ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
		ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);

		ImGui::SetCursorScreenPos(inPos);
		ImGui::PushItemWidth(inSize.x);
		ImGui::PushFont(Low);

		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0, 0, 0, 0));
		ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));

		char tempID[64]; sprintf(tempID, "%s_input", id);

		ImVec2 txtSize = ImGui::CalcTextSize(b.data);
		float offset_y = (inSize.y - txtSize.y) * 0.5f;
		ImGui::SetCursorScreenPos(ImVec2(inPos.x, inPos.y + offset_y));

		bool edited = ImGui::InputText(tempID, b.data, sizeof(b.data),
			ImGuiInputTextFlags_CharsDecimal | ImGuiInputTextFlags_AutoSelectAll);

		ImGui::PopStyleVar();
		ImGui::PopStyleColor(2);
		ImGui::PopItemWidth();
		ImGui::PopFont();

		if (edited || ImGui::IsItemDeactivatedAfterEdit())
			*value = (b.data[0] ? atoi(b.data) : 0);

		// Tooltip
		if (tooltip != nullptr && ImGui::IsItemHovered())
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0, 0, 0, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always);
			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return true;
	}


	//bool HLNumberBoxFloat(const char* id, ImVec2 localPos, int width, int height, float* value, ImFont* Low)
	//{
	//	ImGuiWindow* window = ImGui::GetCurrentWindow();
	//	if (window->SkipItems) return false;

	//	ImVec2 winPos = window->Pos;
	//	ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
	//	ImVec2 size = ImVec2((float)width, (float)height);
	//	ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
	//	ImGuiID iid = window->GetID(id);
	//	ImGui::ItemAdd(bb, iid);

	//	ImDrawList* draw = ImGui::GetWindowDrawList();
	//	draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

	//	// Border
	//	ImU32 topColor = ImColor(152, 152, 151);
	//	ImU32 bottomColor = ImColor(100, 100, 100);
	//	float midY = (pos.y + pos.y + size.y) * 0.5f;
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

	//	// Buffer
	//	struct Buf { char data[32]; bool init; };
	//	static std::unordered_map<ImGuiID, Buf> s_buffers;
	//	Buf& b = s_buffers[iid];
	//	if (!b.init) { snprintf(b.data, sizeof(b.data), "%.3f", *value); b.init = true; }

	//	// Invisible button ve focus
	//	ImGui::SetCursorScreenPos(pos);
	//	ImGui::InvisibleButton(id, size);
	//	if (ImGui::IsItemClicked())
	//		ImGui::SetKeyboardFocusHere();

	//	ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
	//	ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);

	//	ImGui::SetCursorScreenPos(inPos);
	//	ImGui::PushItemWidth(inSize.x);
	//	ImGui::PushFont(Low);

	//	ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
	//	ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, ImVec4(0, 0, 0, 0));
	//	ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(0, 0));

	//	char tempID[64]; sprintf(tempID, "%s_input", id);

	//	// Calculate vertical offset to center text exactly
	//	ImVec2 txtSize = ImGui::CalcTextSize(b.data);
	//	float offset_y = (inSize.y - txtSize.y) * 0.5f;
	//	ImGui::SetCursorScreenPos(ImVec2(inPos.x, inPos.y + offset_y));

	//	bool edited = ImGui::InputText(tempID, b.data, sizeof(b.data),
	//		ImGuiInputTextFlags_CharsDecimal | ImGuiInputTextFlags_AutoSelectAll);

	//	ImGui::PopStyleVar();
	//	ImGui::PopStyleColor(2);
	//	ImGui::PopItemWidth();
	//	ImGui::PopFont();

	//	if (edited || ImGui::IsItemDeactivatedAfterEdit())
	//		*value = (b.data[0] ? static_cast<float>(atof(b.data)) : 0.0f);

	//	return true;
	//}






	// edit
	void HLNumberBox(const char* id, ImVec2 localPos, int width, int height, int* value, ImFont* Low)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));
		ImGuiID iid = window->GetID(id);
		ImGui::ItemAdd(bb, iid);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		ImU32 topColor = ImColor(152, 152, 151); // üst border rengi
		ImU32 bottomColor = ImColor(100, 100, 100);  // alt border rengi
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		struct Buf { char data[32]; bool init; };
		static std::unordered_map<ImGuiID, Buf> s_buffers;
		Buf& b = s_buffers[iid];
		if (!b.init) { snprintf(b.data, sizeof(b.data), "%d", *value); b.init = true; }

		ImGui::SetCursorScreenPos(pos);
		ImGui::InvisibleButton(id, size);
		if (ImGui::IsItemClicked())
			ImGui::SetKeyboardFocusHere();

		ImVec2 inPos = ImVec2(pos.x + 2, pos.y + 2);
		ImVec2 inSize = ImVec2(size.x - 4, size.y - 4);
		ImGui::SetCursorScreenPos(inPos);
		ImGui::PushItemWidth(inSize.x);

		float text_h = ImGui::GetTextLineHeight();
		float pad_y = ImMax(0.0f, (inSize.y - text_h) * 0.5f + 3.0f);

		ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(6, pad_y));
		ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 0.0f);
		ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 0.0f);
		ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(0, 0, 0, 0));
		ImGui::PushFont(Low);
		bool edited = ImGui::InputText(id, b.data, sizeof(b.data), ImGuiInputTextFlags_CallbackCharFilter | ImGuiInputTextFlags_AutoSelectAll | ImGuiInputTextFlags_CharsNoBlank, OnlyDigits);
		ImGui::PopFont();
		ImGui::PopStyleColor();
		ImGui::PopStyleVar(3);
		ImGui::PopItemWidth();

		if (edited || ImGui::IsItemDeactivatedAfterEdit())
			*value = (b.data[0] ? atoi(b.data) : 0);
	}


	//bool HLSliderFloat(const char* id, float* v, float v_min, float v_max, const char* label, ImVec2 localPos, int width, int height, const char* tooltip = nullptr)
	//{
	//	ImGuiWindow* window = ImGui::GetCurrentWindow();
	//	if (window->SkipItems) return false;

	//	ImVec2 winPos = window->Pos;
	//	ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
	//	ImVec2 size = ImVec2((float)width, (float)height);
	//	ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));

	//	ImGui::ItemAdd(bb, ImGui::GetID(id));
	//	ImGui::SetCursorScreenPos(pos);

	//	bool hovered = ImGui::InvisibleButton(id, size) || ImGui::IsItemHovered();
	//	ImVec2 mouse = ImGui::GetIO().MousePos;
	//	if (hovered && ImGui::IsMouseDown(0))
	//	{
	//		float pct = (mouse.x - pos.x - 10.0f) / (size.x - 32.0f);
	//		if (pct < 0.0f) pct = 0.0f;
	//		if (pct > 1.0f) pct = 1.0f;
	//		*v = v_min + pct * (v_max - v_min);
	//	}

	//	ImDrawList* draw = ImGui::GetWindowDrawList();
	//	draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(13, 14, 15), ImColor(13, 14, 15), ImColor(16, 18, 19), ImColor(16, 18, 19));
	//	draw->AddRect(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(0, 0, 0), 0.0f, 0, 1.0f);

	//	ImU32 topColor = ImColor(152, 152, 151);
	//	ImU32 botColor = ImColor(100, 100, 100);
	//	float midY = (pos.y + pos.y + size.y) * 0.5f;
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), botColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), botColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
	//	draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), botColor, 0.8f);
	//	ImGui::PushFont(Low);
	//	ImVec2 textSize = ImGui::CalcTextSize(label ? label : "");
	//	ImVec2 textPos = ImVec2(pos.x + 8.0f, pos.y + (size.y - textSize.y) / 2.0f);
	//	if (textSize.x > size.x * 0.4f) textSize.x = size.x * 0.4f; 
	//	draw->AddText(textPos, ImColor(191, 191, 191), label ? label : "");
	//	ImGui::PopFont();
	//	float circleDiameter = size.y * 0.72f;
	//	float circleRadius = circleDiameter * 0.5f;
	//	float pct = (*v - v_min) / (v_max - v_min);
	//	float cx = pos.x + 10.0f + pct * (size.x - 32.0f);
	//	float cy = pos.y + size.y * 0.5f;
	//	ImVec2 center(cx, cy);

	//	draw->AddCircleFilled(ImVec2(center.x, center.y + 1.4f), circleRadius + 1.6f, ImColor(0, 0, 0, 100));
	//	draw->AddCircleFilled(center, circleRadius, ImColor(46, 48, 41));
	//	draw->AddCircleFilled(center, circleRadius - 1.5f, ImColor(76, 78, 71));
	//	draw->AddCircleFilled(ImVec2(center.x - (circleRadius - 1.5f) * 0.35f, center.y - (circleRadius - 1.5f) * 0.35f), (circleRadius - 1.5f) * 0.45f, ImColor(255, 255, 255, 60));
	//	draw->AddCircle(center, circleRadius - 1.5f, ImColor(0, 0, 0, 40), 32, 0.9f);

	//	char buf[32]; sprintf(buf, "%.2f", *v);
	//	ImVec2 valSize = ImGui::CalcTextSize(buf);
	//	ImVec2 valBgMin(pos.x + size.x - valSize.x - 12, pos.y + (size.y - valSize.y) / 2 - 2);
	//	ImVec2 valBgMax(valBgMin.x + valSize.x + 4, valBgMin.y + valSize.y + 4);
	//	draw->AddRectFilled(valBgMin, valBgMax, ImColor(14, 14, 14), 3.0f);
	//	draw->AddText(ImVec2(valBgMin.x + 2, valBgMin.y + 2), ImColor(191, 191, 191), buf);

	//	if (hovered && tooltip != nullptr)
	//	{
	//		ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0, 0, 0, 0.6f));
	//		ImVec2 mousePos = ImGui::GetMousePos();
	//		ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always);
	//		ImGui::BeginTooltip();
	//		ImGui::PushFont(Low);
	//		ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
	//		ImGui::PopFont();
	//		ImGui::EndTooltip();
	//		ImGui::PopStyleColor();
	//	}

	//	return true;
	//}


	bool HLToggleButton(const char* id, bool* v, const char* label, ImVec2 localPos, int width, int height, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems) return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2((float)width, (float)height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));

		ImGui::ItemAdd(bb, ImGui::GetID(id));
		ImGui::SetCursorScreenPos(pos);

		// Interaction
		bool pressed = ImGui::InvisibleButton(id, size);
		bool hovered = ImGui::IsItemHovered();
		if (pressed)
			*v = !*v;

		ImDrawList* draw = ImGui::GetWindowDrawList();

		// Background (same stil)
		ImColor col_top = ImColor(13, 14, 15);
		ImColor col_bottom = ImColor(16, 18, 19);
		draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y), col_top, col_top, col_bottom, col_bottom);
		draw->AddRect(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		// Dış border çizgileri (aynı palet)
		ImU32 topColor = ImColor(152, 152, 151);
		ImU32 bottomColor = ImColor(100, 100, 100);
		float midY = (pos.y + pos.y + size.y) * 0.5f;
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		// Label (solda sabit)
		ImGui::PushFont(Low);
		ImVec2 textSize = ImGui::CalcTextSize(label ? label : "");
		ImVec2 textPos = ImVec2(pos.x + 8.0f, pos.y + (size.y - textSize.y) / 2.0f);
		draw->AddText(textPos, ImColor(191, 191, 191), label ? label : "");
		ImGui::PopFont();

		float circleDiameter = size.y * 0.72f;
		float circleRadius = circleDiameter * 0.5f;
		float rightPadding = 6.0f;
		float borderTh = 1.5f;
		float shadowOffsetY = 1.4f;

		float cx = pos.x + size.x - circleRadius - rightPadding;
		float cy = pos.y + size.y * 0.5f;
		ImVec2 center(cx, cy);

		ImColor fillCol = *v ? ImColor(76, 78, 71) : ImColor(40, 43, 48);
		ImColor borderCol = *v ? ImColor(46, 48, 41) : ImColor(100, 100, 100);
		ImColor shadowCol = ImColor(0, 0, 0, 100);
		ImColor glossCol = ImColor(255, 255, 255, 60);

		draw->AddCircleFilled(ImVec2(center.x, center.y + shadowOffsetY), circleRadius + 1.6f, shadowCol);

		draw->AddCircleFilled(center, circleRadius, borderCol);

		float innerRadius = circleRadius - (borderTh + 0.0f);
		if (innerRadius < 1.0f) innerRadius = circleRadius * 0.0f;
		draw->AddCircleFilled(center, innerRadius, fillCol);

		ImVec2 glossCenter(center.x - innerRadius * 0.35f, center.y - innerRadius * 0.35f);
		float glossRadius = innerRadius * 0.45f;
		draw->AddCircleFilled(glossCenter, glossRadius, glossCol);

		ImColor innerEdge = ImColor(0, 0, 0, 40);
		draw->AddCircle(center, innerRadius, innerEdge, 32, 0.9f);

		if (hovered && tooltip != nullptr)
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0, 0, 0, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always);
			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return *v;
	}





	bool HLCheckBox(const char* id, bool* v, ImVec2 localPos, int width, int height, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pos = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 size = ImVec2(width, height);
		ImRect bb(pos, ImVec2(pos.x + size.x, pos.y + size.y));

		ImGui::ItemAdd(bb, ImGui::GetID(id));
		ImGui::SetCursorScreenPos(pos);
		if (ImGui::InvisibleButton(id, size))
			*v = !*v;

		bool hovered = ImGui::IsItemHovered();

		ImDrawList* draw = ImGui::GetWindowDrawList();

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x, pos.y + size.y), ImColor(255, 255, 255), 0.0f); //7 beyaz

		ImColor col_top = ImColor(13, 14, 15); // arka plan
		ImColor col_bottom = ImColor(16, 18, 19);// arka plan
		draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y),
			col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1),
			ImVec2(pos.x + size.x - 1, pos.y + size.y - 1),
			ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		ImU32 topColor = ImColor(152, 152, 151); // üst border rengi
		ImU32 bottomColor = ImColor(100, 100, 100);  // alt border rengi
		float midY = (pos.y + pos.y + size.y) * 0.5f;

		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topColor, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), bottomColor, 0.8f);

		if (*v)
		{
			ImColor checkCol = ImColor(56, 58, 61); // Checkbox color
			draw->AddRectFilled(ImVec2(pos.x + 5, pos.y + 5),
				ImVec2(pos.x + size.x - 5, pos.y + size.y - 5),
				checkCol, 0.0f);
		}

		if (hovered && tooltip != nullptr)
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.0f, 0.0f, 0.0f, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always, ImVec2(0, 0));

			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return *v;
	}
	bool NumberButton(const char* label, const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(3, 3, 3) : ImColor(0, 0, 0);
		ImColor col_bottom = active ? ImColor(4, 4, 4) : ImColor(2, 2, 2);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(bb.Min.x + 1, bb.Min.y + 1),
			ImVec2(bb.Max.x - 1, bb.Max.y - 1),
			ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		ImU32 topColor = ImColor(152, 152, 151); // üst border rengi
		ImU32 bottomColor = ImColor(100, 100, 100);  // alt border rengi
		float midY = (bb.Min.y + bb.Max.y) * 0.5f;

		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Min.x, midY), topColor, 0.80f);
		draw->AddLine(ImVec2(bb.Min.x, midY), ImVec2(bb.Min.x, bb.Max.y), bottomColor, 0.80f);
		draw->AddLine(ImVec2(bb.Max.x, bb.Min.y), ImVec2(bb.Max.x, midY), topColor, 0.80f);
		draw->AddLine(ImVec2(bb.Max.x, midY), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 0.80f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Max.x, bb.Min.y), topColor, 0.80f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Max.y), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 0.80f);


		ImGui::PushFont(Low);
		ImVec2 textSize = ImGui::CalcTextSize(label);
		ImVec2 textPos = ImVec2(
			pMin.x + (size.x - textSize.x) / 2,
			pMin.y + (size.y - textSize.y) / 2
		);
		draw->AddText(textPos, ImColor(191, 191, 191), label);
		ImGui::PopFont();

		if (hovered && tooltip != nullptr)
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.0f, 0.0f, 0.0f, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always, ImVec2(0, 0));

			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return pressed;
	}

	// ============================================ Buttonlar ==========================================================
	bool SexyButtonV2(const char* label, const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(13, 13, 13) : ImColor(13, 13, 13);
		ImColor col_bottom = active ? ImColor(111, 112, 110) : ImColor(50, 50, 50);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(bb.Min.x + 1, bb.Min.y + 1), ImVec2(bb.Max.x - 1, bb.Max.y - 1), ImColor(0, 0, 0), 0.0f, 0, 1.0f);


		ImU32 topColor = ImColor(152, 152, 151); // üst border rengi
		ImU32 bottomColor = ImColor(100, 100, 100);  // alt border rengi
		float midY = (bb.Min.y + bb.Max.y) * 0.5f;

		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Min.x, midY), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, midY), ImVec2(bb.Min.x, bb.Max.y), bottomColor, 1.0f);
		draw->AddLine(ImVec2(bb.Max.x, bb.Min.y), ImVec2(bb.Max.x, midY), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Max.x, midY), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Max.x, bb.Min.y), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Max.y), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.0f);

		ImGui::PushFont(Low);
		ImVec2 textSize = ImGui::CalcTextSize(label);
		ImVec2 textPos = ImVec2(
			pMin.x + (size.x - textSize.x) / 2,
			pMin.y + (size.y - textSize.y) / 2
		);
		draw->AddText(textPos, ImColor(191, 191, 191), label);
		ImGui::PopFont();

		return pressed;
	}

	bool SexyButton(const char* label, const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low, bool* pOpenWindow = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		if (pOpenWindow && hovered && ImGui::IsMouseClicked(1))
		{
			*pOpenWindow = !(*pOpenWindow);
		}

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(13, 13, 13) : ImColor(13, 13, 13);
		ImColor col_bottom = active ? ImColor(111, 112, 110) : ImColor(50, 50, 50);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(bb.Min.x + 1, bb.Min.y + 1), ImVec2(bb.Max.x - 1, bb.Max.y - 1), ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		ImU32 topColor = ImColor(152, 152, 151); // üst border rengi
		ImU32 bottomColor = ImColor(100, 100, 100);  // alt border rengi
		float midY = (bb.Min.y + bb.Max.y) * 0.5f;

		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Min.x, midY), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, midY), ImVec2(bb.Min.x, bb.Max.y), bottomColor, 1.0f);
		draw->AddLine(ImVec2(bb.Max.x, bb.Min.y), ImVec2(bb.Max.x, midY), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Max.x, midY), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Max.x, bb.Min.y), topColor, 1.0f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Max.y), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.0f);

		ImGui::PushFont(Low);
		ImVec2 textSize = ImGui::CalcTextSize(label);
		ImVec2 textPos = ImVec2(
			pMin.x + (size.x - textSize.x) / 2,
			pMin.y + (size.y - textSize.y) / 2
		);
		draw->AddText(textPos, ImColor(255, 255, 255), label);
		ImGui::PopFont();

		return pressed;
	}

	bool SexButton(const char* label, const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(144, 184, 204) : ImColor(35, 48, 66);
		ImColor col_bottom = active ? ImColor(35, 48, 66) : ImColor(144, 184, 204);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(bb.Min.x + 1, bb.Min.y + 1), ImVec2(bb.Max.x - 1, bb.Max.y - 1), ImColor(0, 0, 0), 0.0f, 0, 1.0f);

		ImU32 topColor = ImColor(192, 137, 126);
		ImU32 bottomColor = ImColor(106, 70, 95);
		float midY = (bb.Min.y + bb.Max.y) * 0.5f;

		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Min.x, midY), topColor, 1.5f);
		draw->AddLine(ImVec2(bb.Min.x, midY), ImVec2(bb.Min.x, bb.Max.y), bottomColor, 1.5f);
		draw->AddLine(ImVec2(bb.Max.x, bb.Min.y), ImVec2(bb.Max.x, midY), topColor, 1.5f);
		draw->AddLine(ImVec2(bb.Max.x, midY), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.5f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Min.y), ImVec2(bb.Max.x, bb.Min.y), topColor, 1.5f);
		draw->AddLine(ImVec2(bb.Min.x, bb.Max.y), ImVec2(bb.Max.x, bb.Max.y), bottomColor, 1.5f);

		ImGui::PushFont(Low);
		ImVec2 textSize = ImGui::CalcTextSize(label);
		ImVec2 textPos = ImVec2(
			pMin.x + (size.x - textSize.x) / 2,
			pMin.y + (size.y - textSize.y) / 2
		);
		draw->AddText(textPos, ImColor(255, 255, 255), label);
		ImGui::PopFont();

		return pressed;
	}



	bool CloseButton(const char* label, const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();
		ImColor col_top = active ? ImColor(63, 30, 38) : ImColor(51, 6, 15);
		ImColor col_bottom = active ? ImColor(51, 6, 15) : ImColor(63, 30, 38);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(
			ImVec2(bb.Min.x + 1, bb.Min.y + 1),
			ImVec2(bb.Max.x - 1, bb.Max.y - 1),
			ImColor(49, 29, 35),
			0.0f, 0, 1.0f
		);
		draw->AddRect(bb.Min, bb.Max, ImColor(192, 137, 126), 0.0f, 0, 1.5f);

		ImVec2 textSize = ImGui::CalcTextSize(label);
		ImVec2 textPos = ImVec2(
			pMin.x + (size.x - textSize.x) / 2,
			pMin.y + (size.y - textSize.y) / 2
		);
		draw->AddText(textPos, ImColor(193, 27, 67), label);

		if (hovered && tooltip != nullptr)
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.0f, 0.0f, 0.0f, 0.6f));
			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always, ImVec2(0, 0));

			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();
			ImGui::PopStyleColor();
		}

		return pressed;
	}
	// sağ tab buttonları
	bool OuterButtonToolTip(const ImVec2& localPos, const ImVec2& size, bool active, ImFont* Low, const char* tooltip = nullptr)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);

		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(23, 23, 26) : ImColor(32, 32, 32);
		ImColor col_bottom = active ? ImColor(39, 39, 42) : ImColor(42, 45, 47);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(ImVec2(bb.Min.x + 1, bb.Min.y + 1),
			ImVec2(bb.Max.x - 1, bb.Max.y - 1),
			ImColor(8, 13, 32),
			0.0f, 0, 1.0f);

		draw->AddRect(bb.Min, bb.Max, ImColor(150, 152, 150), 0.0f, 0, 1.5f); // dış border

		if (hovered && tooltip != nullptr)
		{
			ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.0f, 0.0f, 0.0f, 0.6f));

			ImVec2 mousePos = ImGui::GetMousePos();
			ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always, ImVec2(0, 0));

			ImGui::BeginTooltip();
			ImGui::PushFont(Low);
			ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
			ImGui::PopFont();
			ImGui::EndTooltip();

			ImGui::PopStyleColor();
		}

		return pressed;
	}


	bool OuterButton(const ImVec2& localPos, const ImVec2& size, bool active)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window->SkipItems)
			return false;

		ImVec2 winPos = window->Pos;
		ImVec2 pMin = ImVec2(winPos.x + localPos.x, winPos.y + localPos.y);
		ImVec2 pMax = ImVec2(pMin.x + size.x, pMin.y + size.y);
		ImRect bb(pMin, pMax);


		ImRect bb_local(localPos, ImVec2(localPos.x + size.x, localPos.y + size.y));
		ImGui::ItemSize(bb_local);
		if (!ImGui::ItemAdd(bb, 0))
			return false;

		bool hovered = ImGui::IsMouseHoveringRect(bb.Min, bb.Max);
		bool pressed = hovered && ImGui::IsMouseClicked(0);

		ImDrawList* draw = ImGui::GetWindowDrawList();

		ImColor col_top = active ? ImColor(13, 22, 32) : ImColor(10, 16, 21);
		ImColor col_bottom = active ? ImColor(35, 59, 83) : ImColor(13, 22, 32);
		draw->AddRectFilledMultiColor(bb.Min, bb.Max, col_top, col_top, col_bottom, col_bottom);

		draw->AddRect(
			ImVec2(bb.Min.x + 1, bb.Min.y + 1),
			ImVec2(bb.Max.x - 1, bb.Max.y - 1),
			ImColor(8, 13, 32),
			0.0f, 0, 1.0f
		);


		draw->AddRect(bb.Min, bb.Max, ImColor(23, 34, 46), 0.0f, 0, 1.5f);
		return pressed;
	}
}


namespace WindowCreate
{
	bool Window(const char* title, const char* name, int width, int height, int imgui_w, int imgui_h, ImFont* Font)
	{
		ImGui::SetNextWindowSize(ImVec2(imgui_w, imgui_h));
		ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0, 0, 0, 0));
		if (!ImGui::Begin(title, nullptr, ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoScrollWithMouse | ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoBackground))
		{
			ImGui::End();
			return false;
		}

		ImVec2 basePos = ImGui::GetCursorScreenPos();
		ImDrawList* draw = ImGui::GetWindowDrawList();

		const float pad = -2.50f;
		ImVec2 pos = ImVec2(basePos.x + pad, basePos.y + pad);
		ImVec2 size(width, height);

		draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y),
			ImColor(8, 13, 21), ImColor(8, 13, 21),
			ImColor(10, 16, 26), ImColor(10, 16, 26));

		const float textH = 28.f;
		const float sepH = 1.f;

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x - 0.5f, pos.y + textH), ImColor(14, 19, 26));
		draw->AddRectFilled(ImVec2(pos.x, pos.y + textH), ImVec2(pos.x + size.x - 0.5f, pos.y + textH + sepH), ImColor(121, 78, 74));

		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		ImGui::PushFont(Font);
		draw->AddText(ImVec2(pos.x + 10, pos.y + 6.10f), ImColor(191, 191, 191), name);
		ImGui::PopFont();


		ImU32 topCol = ImColor(121, 78, 74);
		ImU32 botCol = ImColor(58, 41, 41);
		float midY = pos.y + size.y * 0.5f;

		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), botCol, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), botCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), botCol, 0.8f);

		ImGui::Dummy(size);
		return true;
	}
	// Yeni Window penceresi
	bool WindowCloser(const char* title, const char* name, int width, int height, int imgui_w, int imgui_h, ImFont* Font, bool* pOpen)
	{
		ImGui::SetNextWindowSize(ImVec2(imgui_w, imgui_h));
		ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0, 0, 0, 0));
		if (!ImGui::Begin(title, nullptr, ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoScrollWithMouse | ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoBackground))
		{
			ImGui::End();
			return false;
		}

		ImVec2 basePos = ImGui::GetCursorScreenPos();
		ImDrawList* draw = ImGui::GetWindowDrawList();

		const float pad = -4.0f;
		ImVec2 pos = ImVec2(basePos.x + pad, basePos.y + pad);
		ImVec2 size(width, height);

		draw->AddRectFilledMultiColor(pos, ImVec2(pos.x + size.x, pos.y + size.y),
			ImColor(18, 19, 20), ImColor(18, 19, 20),
			ImColor(22, 23, 24), ImColor(25, 26, 27));

		const float textH = 28.f;
		const float sepH = 1.f;

		draw->AddRectFilled(pos, ImVec2(pos.x + size.x - 0.5f, pos.y + textH), ImColor(24, 26, 29)); // arka plan üst
		draw->AddRectFilled(ImVec2(pos.x, pos.y + textH), ImVec2(pos.x + size.x - 0.5f, pos.y + textH + sepH), ImColor(152, 152, 151));

		draw->AddRect(ImVec2(pos.x + 1, pos.y + 1), ImVec2(pos.x + size.x - 1, pos.y + size.y - 1), ImColor(0, 0, 0));

		ImGui::PushFont(Font);
		draw->AddText(ImVec2(pos.x + 10, pos.y + 6.1f), ImColor(191, 191, 191), name); // yazı rengi
		ImGui::PopFont();

		const char* closeLabel = "X";
		ImVec2 closeSize = ImGui::CalcTextSize(closeLabel);
		ImVec2 closePos(pos.x + size.x - closeSize.x - 10, pos.y + 6.1f);
		//draw->AddText(closePos, ImColor(112, 120, 112), closeLabel); // yazı rengi

		if (HLCloseV2)
		{
			ImVec2 iconSize = ImVec2(closeSize.x * 1.30f, closeSize.y * 1.0f);
			ImVec2 iconMin = closePos;
			ImVec2 iconMax = ImVec2(closePos.x + iconSize.x, closePos.y + iconSize.y);

			draw->AddImage(
				HLCloseV2,
				iconMin, iconMax,
				ImVec2(0, 0),
				ImVec2(1, 1),
				IM_COL32(255, 255, 255, 255)
			);

			bool hovered = ImGui::IsMouseHoveringRect(iconMin, iconMax);
			const char* tooltip = /*Close Window*/XorStrTR<0x47, 13, 0x2CAE8000>("\x04\x24\x26\x39\x2E\x6C\x1A\x27\x21\x34\x3E\x25" + 0x2CAE8000).s;

			if (hovered && tooltip != nullptr)
			{
				ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.0f, 0.0f, 0.0f, 0.6f));

				ImVec2 mousePos = ImGui::GetMousePos();
				ImGui::SetNextWindowPos(ImVec2(mousePos.x, mousePos.y + 20), ImGuiCond_Always, ImVec2(0, 0));
				ImGui::BeginTooltip();
				ImGui::PushFont(Low); 
				ImGui::TextColored(ImVec4(1, 1, 1, 1), "%s", tooltip);
				ImGui::PopFont();
				ImGui::EndTooltip();
				ImGui::PopStyleColor();
			}
		}


		ImVec2 mouse = ImGui::GetIO().MousePos;
		if (pOpen && ImGui::IsMouseClicked(0))
		{
			ImRect closeRect(closePos, ImVec2(closePos.x + closeSize.x, closePos.y + closeSize.y));
			if (closeRect.Contains(mouse))
				*pOpen = false;
		}

		ImU32 topCol = ImColor(152, 152, 151); // üst border
		ImU32 botCol = ImColor(100, 100, 100); // alt border
		float midY = pos.y + size.y * 0.5f;

		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x, midY), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, midY), ImVec2(pos.x, pos.y + size.y), botCol, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, pos.y), ImVec2(pos.x + size.x, midY), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x + size.x, midY), ImVec2(pos.x + size.x, pos.y + size.y), botCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y), ImVec2(pos.x + size.x, pos.y), topCol, 0.8f);
		draw->AddLine(ImVec2(pos.x, pos.y + size.y), ImVec2(pos.x + size.x, pos.y + size.y), botCol, 0.8f);

		ImGui::Dummy(size);
		return true;
	}



}