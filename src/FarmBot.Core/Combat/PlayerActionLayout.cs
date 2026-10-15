using System.Text;

namespace FarmBot.Core.Combat;

public sealed record PlayerActionLayout(
    uint PreferredImageBase,
    uint PlayerSingletonRva,
    uint SetTargetRva,
    uint GetTargetVidRva,
    uint SetAttackKeyStateRva,
    uint ComboAttackRva,
    uint PickCloseItemRva,
    uint TargetVidDisplacement,
    uint SetTargetThisAdjustment,
    uint SetTargetVtableSlot,
    string SetTargetPrologue,
    uint StatusPointsDisplacement,
    uint PlayerNameDisplacement,
    uint PointXorMask,
    IReadOnlyList<string> Evidence)
{
    public bool HasSetTarget =>
        PlayerSingletonRva is > 0x1000 and < 0x8000000 &&
        (SetTargetVtableSlot != 0 || SetTargetRva is > 0 and < 0x8000000);

    public bool HasPlayerHud => StatusPointsDisplacement != 0 || PlayerNameDisplacement != 0;

    public int PointLevel { get; init; } = 1;
    public int PointExp { get; init; } = 3;
    public int PointNextExp { get; init; } = 4;
    public int PointHp { get; init; } = 5;
    public int PointMaxHp { get; init; } = 6;
    public int PointSp { get; init; } = 7;
    public int PointMaxSp { get; init; } = 8;
    public uint NewAttackRva { get; init; }
    public uint GetStatusRva { get; init; }
    public uint GetStatusVtableSlot { get; init; }
    public uint SendAttackPacketRva { get; init; }
    public uint SendStatePacketRva { get; init; }
    public uint SendPickupPacketRva { get; init; }
    public uint SendItemUsePacketRva { get; init; }
    public uint GetItemIndexRva { get; init; }
    public uint GetItemCountRva { get; init; }
    public uint SelectItemRva { get; init; }
    public uint GetItemNameRva { get; init; }
    public uint GetItemTypeRva { get; init; }
    public uint GetItemSubTypeRva { get; init; }
    public uint GetAffectRva { get; init; }
    public uint GetValueRva { get; init; }
    public uint GetIconImageRva { get; init; }
    public uint UseItemRva { get; init; }
    public uint ItemManagerRva { get; init; }
    public uint GetSelectedRva { get; init; }
    public uint ItemSingletonRva { get; init; }
    public uint GetCloseItemRva { get; init; }
    public uint SetAutoRva { get; init; }
    public uint ReserveClickRva { get; init; }
    public uint ReservedModeDisplacement { get; init; }
    public uint VidReservedDisplacement { get; init; }
    public uint MoveToDestRva { get; init; }
    public uint MoveToPixelRva { get; init; }
    public uint GetMainActorRva { get; init; }
    public uint OnPressActorRva { get; init; }
    public uint ChaseActorRva { get; init; }
    public uint ApproachInstRva { get; init; }
    public uint PlayerUpdateRva { get; init; }
    public uint GetPixelRva { get; init; }
    public uint InRangeRva { get; init; }
    public uint GetSkillIndexRva { get; init; }
    public uint GetSkillLevelRva { get; init; }
    public uint ClickSkillSlotRva { get; init; }
    public uint GetSkillCoolTimeRva { get; init; }
    public uint IsSkillCoolTimeRva { get; init; }
    public uint SendUseSkillPacketRva { get; init; }
    public uint SkillManagerRva { get; init; }
    public uint GetSkillDataRva { get; init; }
    public uint IsStandingSkillRva { get; init; }
    public uint IsNeedTargetRva { get; init; }
    public uint IsAttackSkillRva { get; init; }
}

public sealed record LoginNativeLayout(
    uint NetStreamRva,
    uint AccountConnectorRva,
    uint SetLoginInfoRva,
    uint AccountSetLoginInfoRva,
    uint AccountConnectRva,
    uint DirectEnterRva,
    uint SelectCharacterRva,
    uint SetLoginPhaseRva,
    uint OnLogoCloseRva,
    uint PhaseDisplacement,
    uint SetGamePhaseRva,
    uint StartGameRva,
    uint NotifyGamePhaseRva,
    uint SendEnterGamePacketRva,
    IReadOnlyList<string> Evidence)
{
    public bool CanLogin =>
        NetStreamRva is > 0x1000 and < 0x8000000 &&
        AccountConnectorRva is > 0x1000 and < 0x8000000 &&
        SetLoginInfoRva is > 0 and < 0x8000000 &&
        AccountConnectRva is > 0 and < 0x8000000 &&
        DirectEnterRva is > 0 and < 0x8000000;

    public string ToNativeConfig()
    {
        var text = new StringBuilder();
        text.Append("layout=recovered\n");
        Line(text, "net", NetStreamRva);
        Line(text, "account", AccountConnectorRva);
        Line(text, "setLogin", SetLoginInfoRva);
        Line(text, "accountSetLogin", AccountSetLoginInfoRva);
        Line(text, "connect", AccountConnectRva);
        Line(text, "direct", DirectEnterRva);
        Line(text, "select", SelectCharacterRva);
        Line(text, "setLoginPhase", SetLoginPhaseRva);
        Line(text, "setGamePhase", SetGamePhaseRva);
        Line(text, "startGame", StartGameRva);
        Line(text, "notifyGame", NotifyGamePhaseRva);
        Line(text, "enterGame", SendEnterGamePacketRva);
        Line(text, "logoClose", OnLogoCloseRva);
        Line(text, "phaseDisp", PhaseDisplacement);
        Line(text, "loginId", LoginIdDisplacement);
        Line(text, "loginPwd", LoginPasswordDisplacement);
        return text.ToString();
    }

    public uint LoginIdDisplacement { get; init; }
    public uint LoginPasswordDisplacement { get; init; }

    private static void Line(StringBuilder text, string key, uint value)
    {
        if (value != 0)
            text.Append(key).Append('=').Append(value.ToString("X")).Append('\n');
    }
}

public sealed record MiniMapLayout(
    uint SingletonRva,
    uint ShowAtlasRva,
    uint HideAtlasRva,
    uint LoadAtlasRva,
    uint UpdateAtlasRva,
    uint RenderAtlasRva,
    uint IsShowAtlasRva,
    IReadOnlyList<string> Evidence);

public static class PlayerActionLayoutRecoverer
{
    public static PlayerActionLayout Recover(string path)
    {
        var pe = ActionPe.Load(path);
        var evidence = new List<string>();
        var setTarget = RecoverNamedNative(pe, "SetTarget", evidence);
        var getTarget = RecoverNamedNative(pe, "GetTargetVID", evidence);
        var attackKey = RecoverNamedNative(pe, "SetAttackKeyState", evidence);
        var combo = RecoverNamedNative(pe, "ComboAttack", evidence);
        var pickClose = RecoverNamedNative(pe, "PickCloseItem", evidence);
        if (pickClose.native == 0)
            pickClose = RecoverNamedNative(pe, "PickCloseItemVector", evidence);
        var newAttack = RecoverNamedNative(pe, "NEW_Attack", evidence);
        var sendAttack = RecoverLoginExport(pe, "SendAttackPacket", evidence);
        if (sendAttack.native == 0)
            sendAttack = RecoverLoginExport(pe, "SendBattleAttackPacket", evidence);
        var sendState = RecoverLoginExport(pe, "SendCharacterStatePacket", evidence);
        var sendPickup = RecoverLoginExport(pe, "SendItemPickUpPacket", evidence);
        var sendUse = RecoverLoginExport(pe, "SendItemUsePacket", evidence);
        var singleton = PreferInImage(pe, getTarget.singleton, setTarget.singleton, attackKey.singleton, newAttack.singleton);
        if (singleton == 0)
            evidence.Add("CPythonPlayer singleton was not recovered from SetTarget/GetTargetVID wrappers.");
        var getStatus = RecoverNamedNativePrefer(pe, "GetStatus", singleton, evidence);
        var statusVirt = RecoverVirtualDispatch(pe, getStatus.wrapper);
        if (statusVirt.slot != 0)
            evidence.Add($"player.GetStatus is virtual: ecx=CPythonPlayer*+0x{statusVirt.thisAdj:X}, call [vtable+0x{statusVirt.slot:X}].");
        var getStatusRva = statusVirt.slot == 0 && getStatus.singleton == singleton ? getStatus.native : 0;
        var targetDisp = RecoverEcxLoad(pe, getTarget.native);
        if (targetDisp != 0)
            evidence.Add($"GetTargetVID reads [ecx+0x{targetDisp:X}] (m_dwTargetVID).");
        var virt = RecoverVirtualDispatch(pe, setTarget.wrapper);
        if (virt.slot != 0)
        {
            evidence.Add($"player.SetTarget is virtual: ecx=CPythonPlayer*+0x{virt.thisAdj:X}, call [vtable+0x{virt.slot:X}].");
            evidence.Add("The following E8 in that wrapper is Py_BuildNone, not CPythonPlayer::SetTarget.");
        }

        var setTargetSlot = virt.slot;
        var afterLea = NativeCallAfterThisAdjust(pe, setTarget.wrapper);
        var setTargetRva = virt.slot != 0 ? 0u : afterLea != 0 ? afterLea : virt.thisAdj != 0 ? 0u : setTarget.native;
        if (afterLea != 0 && virt.slot == 0)
            evidence.Add($"player.SetTarget is thiscall after lea ecx,[ecx+0x{virt.thisAdj:X}] native RVA 0x{afterLea:X}.");
        var prologue = virt.slot == 0 ? pe.Hex(pe.ImageBase + setTarget.native, 16) : "";
        var attackVirt = RecoverVirtualDispatch(pe, attackKey.wrapper);
        var attackKeyRva = attackVirt.slot == 0 ? attackKey.native : 0;
        if (sendAttack.native == 0)
            sendAttack = (RecoverFunctionFromUniqueString(pe, "Send Battle Attack Packet Error", evidence), sendAttack.singleton, sendAttack.wrapper);
        if (sendState.native == 0)
            sendState = (RecoverFunctionFromUniqueString(pe, "CPythonNetworkStream::SendCharacterStatePacket", evidence), sendState.singleton, sendState.wrapper);
        if (sendAttack.native == 0)
        {
            var send = RecoverNetworkSend(pe, sendPickup.native, evidence);
            foreach (var size in new byte[] { 6, 7, 8 })
            {
                var rva = RecoverSiblingPacket(pe, send, sendPickup.native, size, 8, evidence, "SendAttackPacket");
                if (rva == 0)
                    continue;
                sendAttack = (rva, sendAttack.singleton, sendAttack.wrapper);
                break;
            }
        }
        if (sendAttack.native == 0)
            sendAttack = (RecoverPacketByHeader(pe, sendPickup.native, CombatNativeContract.AttackPacketHeader, evidence, "SendAttackPacket"), sendAttack.singleton, sendAttack.wrapper);
        if (sendState.native == 0)
            sendState = (RecoverPacketByHeader(pe, sendPickup.native, 7, evidence, "SendCharacterStatePacket"), sendState.singleton, sendState.wrapper);
        var setAuto = TryNamed(pe, new[] { "SetToAutoAttack", "SetAutoAttack", "SetAutoAttackTargetVID" }, singleton, evidence);
        var reserve = TryNamed(pe, new[] { "ReserveClickActor", "SetReservedDestPixelPosition", "SetClickActor" }, singleton, evidence);
        var loot = RecoverItemFromPickClose(pe, pickClose.native, singleton, evidence);
        var moveTo = TryNamed(pe, new[]
        {
            "NEW_MoveToDestPixelPositionDirection",
            "NEW_MoveToDestPixelPosition",
            "NEW_MoveToMousePickedPosition",
            "SetMoveToDestPixelPosition",
            "MoveToDestPixelPosition"
        }, singleton, evidence);
        if (moveTo.native == 0)
            moveTo = TryNamed(pe, new[] { "MoveToDestPosition", "SetMoveToDestPosition", "SetMoveToDestPixelPosition" }, 0, evidence);
        var methods = NeighborMethods(pe, combo.wrapper != 0 ? combo.wrapper : setTarget.wrapper, singleton);
        if (methods.Count > 0)
            evidence.Add("player methods: " + string.Join(", ", methods.ConvertAll(item => item.name)));
        foreach (var method in methods)
        {
            if (moveTo.native == 0 && method.name.IndexOf("MoveToDest", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var native = NativeCallAfterSingleton(pe, method.wrapper, singleton == 0 ? 0 : pe.ImageBase + singleton);
                if (native != 0 && !LooksLikePythonHelper(pe, pe.VaToFile(pe.ImageBase + native)))
                {
                    moveTo = (native, singleton, method.wrapper);
                    evidence.Add($"player.{method.name} native RVA 0x{native:X} used as MoveToDest.");
                }
            }

            if (newAttack.native == 0 && method.name is "NEW_Attack" or "Attack" or "StartAttack")
            {
                var native = NativeCallAfterSingleton(pe, method.wrapper, singleton == 0 ? 0 : pe.ImageBase + singleton);
                if (native != 0)
                {
                    newAttack = (native, singleton, method.wrapper);
                    evidence.Add($"player.{method.name} native RVA 0x{native:X} used as NEW_Attack.");
                }
            }
        }

        if (moveTo.native != 0 && LooksLikePythonHelper(pe, pe.VaToFile(pe.ImageBase + moveTo.native)))
        {
            evidence.Add($"rejected MoveToDest RVA 0x{moveTo.native:X} (python helper, not CInstanceBase/CPythonPlayer).");
            moveTo = (0, 0, 0);
        }

        if (moveTo.native != 0 && loot.item != 0 && moveTo.singleton == loot.item)
        {
            evidence.Add($"rejected MoveToDest RVA 0x{moveTo.native:X} (item singleton 0x{loot.item:X}, not player).");
            moveTo = (0, 0, 0);
        }

        var getMain = RecoverGetMainActor(pe, setTargetRva, evidence);
        var fromMove = RecoverMoveToActor(pe, setTargetRva, evidence);
        if (fromMove != 0)
            moveTo = (fromMove, 0, 0);
        var movePixel = fromMove != 0 ? RecoverMoveToPixel(pe, fromMove, evidence) : 0;
        var getPixel = fromMove != 0 ? RecoverGetPixel(pe, fromMove, evidence) : 0;
        var reserved = RecoverReserveClick(pe, combo.native, evidence);
        if (reserved.rva != 0)
            reserve = (reserved.rva, singleton, 0);
        var pressActor = RecoverOnPressActor(pe, setTargetRva, setTarget.wrapper, reserved.rva != 0 ? reserved.rva : reserve.native, evidence);
        var chase = RecoverChaseActor(pe, getMain, reserved.vidDisp, reserved.rva, evidence);
        var approachInst = RecoverApproachInst(pe, chase, fromMove, evidence);
        var inRange = RecoverInRangeCheck(pe, chase, approachInst, combo.native, evidence);
        var playerUpdate = RecoverPlayerUpdate(pe, getMain, reserved.modeDisp != 0 ? reserved.modeDisp : 0x58u, evidence);
        var hud = RecoverPlayerHud(pe, singleton, getStatusRva != 0 ? getStatusRva : getStatus.native, evidence);
        var points = RecoverPointIds(pe, evidence);

        evidence.Add("MSVC x86 thiscall: ecx=CPythonPlayer*, args right-to-left, callee pops.");
        evidence.Add("SetTarget(DWORD vid, BOOL force=TRUE): thiscall, ret 8. Requires main actor, CanChangeTarget, live targetable VID. Sends target packet only.");
        evidence.Add("NEW_Attack(): thiscall, no args. Keeps __CanAttack, range, bow shot, shop, and facing checks. Does not move melee.");
        evidence.Add("SetAttackKeyState(bool): thiscall, ret 4. Sets m_isAtkKey; fishing intercept if pressed. Attack is applied later on the game thread.");
        evidence.Add("__OnPressActor(CInstanceBase&, DWORD, bool): private thiscall, ret 12. Ordinary click path: SetTarget, then attack-in-range or reserve-click approach.");
        evidence.Add("CPythonPlayer : CSingleton<CPythonPlayer> + IAbstractPlayer. Both singletons are process-lifetime; methods are game-thread only.");
        evidence.Add("Ordinary schedule: window thread Loop() → MessageProcess/Process() → player.Update / mouse / Python player.SetTarget.");
        evidence.Add("An adapter pipe worker is not that thread. SetTarget must not be thiscall'd from it.");
        evidence.Add("Release RVAs are recovered from METH_VARARGS wrappers. Debug 0x4EA* addresses are not used.");

        if (pickClose.native != 0)
            evidence.Add($"CPythonPlayer::PickCloseItem native RVA 0x{pickClose.native:X}.");
        else
            evidence.Add("PickCloseItem Python wrapper was not recovered.");

        var mini = RecoverMiniMap(pe, evidence);
        evidence.Add($"MiniMap singleton=0x{mini.singleton:X} load=0x{mini.load:X} update=0x{mini.update:X} render=0x{mini.render:X}.");
        var getItemIndex = RecoverItemMethod(pe, "GetItemIndex", singleton, evidence);
        var getItemCount = RecoverItemMethod(pe, "GetItemCount", singleton, evidence);
        var selectItem = RecoverItemMethod(pe, "SelectItem", 0, evidence);
        var getItemName = RecoverItemMethod(pe, "GetItemName", selectItem.singleton, evidence);
        var getItemType = RecoverItemMethod(pe, "GetItemType", selectItem.singleton, evidence);
        var getItemSubType = RecoverItemMethod(pe, "GetItemSubType", selectItem.singleton, evidence);
        var getAffect = RecoverItemMethod(pe, "GetAffect", selectItem.singleton, evidence);
        var getValue = RecoverItemMethod(pe, "GetValue", selectItem.singleton, evidence);
        var getIcon = RecoverItemMethod(pe, "GetIconImage", selectItem.singleton, evidence);
        var useItem = RecoverItemMethod(pe, "UseItem", singleton, evidence);
        var getNameOnItem = CallAfterMovEcxEax(pe, getItemName.wrapper);
        var getTypeOnItem = CallAfterMovEcxEax(pe, getItemType.wrapper);
        var getSubOnItem = CallAfterMovEcxEax(pe, getItemSubType.wrapper);
        var getSelected = FirstE8AfterEcxLoad(pe, getItemType.wrapper != 0 ? getItemType.wrapper : getItemName.wrapper);
        if (getSelected == 0)
            getSelected = FirstCallAllowShort(pe, getItemType.wrapper != 0 ? getItemType.wrapper : getItemName.wrapper, selectItem.singleton == 0 ? 0 : pe.ImageBase + selectItem.singleton);
        if (getNameOnItem != 0)
            getItemName = (getNameOnItem, getItemName.singleton, getItemName.wrapper);
        if (getTypeOnItem != 0)
            getItemType = (getTypeOnItem, getItemType.singleton, getItemType.wrapper);
        if (getSubOnItem != 0)
            getItemSubType = (getSubOnItem, getItemSubType.singleton, getItemSubType.wrapper);
        var getIconOnItem = CallAfterMovEcxEax(pe, getIcon.wrapper);
        if (getIconOnItem != 0)
            getIcon = (getIconOnItem, getIcon.singleton, getIcon.wrapper);
        evidence.Add($"itemMgr=0x{selectItem.singleton:X} getSelected=0x{getSelected:X} getName=0x{getItemName.native:X} getType=0x{getItemType.native:X} getSub=0x{getItemSubType.native:X} getIcon=0x{getIcon.native:X} useItem=0x{useItem.native:X}.");
        var getSkillIndex = RecoverItemMethod(pe, "GetSkillIndex", singleton, evidence);
        var getSkillLevel = RecoverItemMethod(pe, "GetSkillLevel", singleton, evidence);
        var clickSkill = RecoverItemMethod(pe, "ClickSkillSlot", singleton, evidence);
        var getSkillCool = RecoverItemMethod(pe, "GetSkillCoolTime", singleton, evidence);
        var isSkillCool = RecoverItemMethod(pe, "IsSkillCoolTime", singleton, evidence);
        var sendUseSkill = RecoverLoginExport(pe, "SendUseSkillPacket", evidence);
        if (sendUseSkill.native == 0)
        {
            var native = RecoverUseSkillFromClick(pe, clickSkill.native, sendUse.singleton, evidence);
            sendUseSkill = (native, sendUse.singleton, 0);
        }
        var getSkillName = RecoverItemMethod(pe, "GetSkillName", 0, evidence);
        var isStanding = RecoverItemMethod(pe, "IsStandingSkill", getSkillName.singleton, evidence);
        var isNeedTarget = RecoverItemMethod(pe, "IsNeedTarget", getSkillName.singleton, evidence);
        var isAttackSkill = RecoverItemMethod(pe, "IsAttackSkill", getSkillName.singleton, evidence);
        var standingFn = CallOnLocalEcx(pe, isStanding.wrapper, evidence, "IsStandingSkill");
        var needTargetFn = CallOnLocalEcx(pe, isNeedTarget.wrapper, evidence, "IsNeedTarget");
        var attackSkillFn = CallOnLocalEcx(pe, isAttackSkill.wrapper, evidence, "IsAttackSkill");
        evidence.Add($"skills getIndex=0x{getSkillIndex.native:X} getLevel=0x{getSkillLevel.native:X} click=0x{clickSkill.native:X} cool=0x{getSkillCool.native:X} isCool=0x{isSkillCool.native:X} sendUse=0x{sendUseSkill.native:X} skillMgr=0x{getSkillName.singleton:X} getData=0x{getSkillName.native:X} standing=0x{standingFn:X}.");

        return new PlayerActionLayout(pe.ImageBase, singleton, setTargetRva, getTarget.native, attackKeyRva, combo.native, pickClose.native, targetDisp, virt.thisAdj != 0 ? virt.thisAdj : attackVirt.thisAdj, setTargetSlot, prologue, hud.points, hud.name != 0 ? hud.name : 0x14, hud.xor, evidence)
        {
            PointLevel = points.level,
            PointExp = points.exp,
            PointNextExp = points.next,
            PointHp = points.hp,
            PointMaxHp = points.maxHp,
            PointSp = points.sp,
            PointMaxSp = points.maxSp,
            NewAttackRva = newAttack.native,
            GetStatusRva = getStatusRva,
            GetStatusVtableSlot = statusVirt.slot,
            SendAttackPacketRva = sendAttack.native,
            SendStatePacketRva = sendState.native,
            SendPickupPacketRva = sendPickup.native,
            SendItemUsePacketRva = sendUse.native,
            GetItemIndexRva = getItemIndex.native,
            GetItemCountRva = getItemCount.native,
            SelectItemRva = selectItem.native,
            GetItemNameRva = getItemName.native,
            GetItemTypeRva = getItemType.native,
            GetItemSubTypeRva = getItemSubType.native,
            GetAffectRva = getAffect.native,
            GetValueRva = getValue.native,
            GetIconImageRva = getIcon.native,
            UseItemRva = useItem.native,
            ItemManagerRva = selectItem.singleton,
            GetSelectedRva = getSelected,
            ItemSingletonRva = loot.item,
            GetCloseItemRva = loot.getClose,
            SetAutoRva = setAuto.native,
            ReserveClickRva = reserve.native,
            ReservedModeDisplacement = reserved.modeDisp,
            VidReservedDisplacement = reserved.vidDisp,
            MoveToDestRva = moveTo.native,
            MoveToPixelRva = movePixel,
            GetMainActorRva = getMain,
            OnPressActorRva = pressActor,
            ChaseActorRva = chase,
            ApproachInstRva = approachInst,
            PlayerUpdateRva = playerUpdate,
            GetPixelRva = getPixel,
            InRangeRva = inRange,
            GetSkillIndexRva = getSkillIndex.native,
            GetSkillLevelRva = getSkillLevel.native,
            ClickSkillSlotRva = clickSkill.native,
            GetSkillCoolTimeRva = getSkillCool.native,
            IsSkillCoolTimeRva = isSkillCool.native,
            SendUseSkillPacketRva = sendUseSkill.native,
            SkillManagerRva = getSkillName.singleton,
            GetSkillDataRva = getSkillName.native,
            IsStandingSkillRva = standingFn,
            IsNeedTargetRva = needTargetFn,
            IsAttackSkillRva = attackSkillFn
        };
    }

    public static LoginNativeLayout RecoverLogin(string path)
    {
        var pe = ActionPe.Load(path);
        var evidence = new List<string>();
        var setLogin = RecoverLoginExport(pe, "SetLoginInfo", evidence);
        var connectAuth = RecoverLoginExport(pe, "ConnectToAccountServer", evidence);
        var direct = RecoverLoginExport(pe, "DirectEnter", evidence);
        var select = RecoverLoginExport(pe, "SendSelectCharacterPacket", evidence);
        var logoClose = RecoverLoginExport(pe, "OnLogoClose", evidence);
        var setPhase = RecoverFunctionFromUniqueString(pe, "## Network - Login Phase ##", evidence);
        if (setPhase == 0)
            setPhase = RecoverLoginExport(pe, "SetLoginPhase", evidence).native;
        var setGamePhase = RecoverFunctionFromUniqueString(pe, "## Network - Game Phase ##", evidence);
        if (setGamePhase == 0)
            setGamePhase = RecoverLoginExport(pe, "SetGamePhase", evidence).native;
        var setLoadingPhase = RecoverFunctionFromUniqueString(pe, "## Network - Loading Phase ##", evidence);
        if (setLoadingPhase == 0)
            setLoadingPhase = RecoverLoginExport(pe, "SetLoadingPhase", evidence).native;
        var startGame = RecoverLoginExport(pe, "StartGame", evidence);
        var enterGamePkt = RecoverLoginExport(pe, "SendEnterGamePacket", evidence);
        var setHandler = RecoverLoginExport(pe, "SetHandler", evidence);
        var setGameWindow = RecoverNamedNative(pe, "SetGameWindow", evidence);
        var phaseDisp = RecoverPhaseDisplacement(pe, setPhase, evidence);
        var accountSetLogin = SecondThiscallAfterSingleton(pe, setLogin.wrapper, setLogin.singleton, evidence, "SetLoginInfo");
        var netRva = PreferInImage(pe, setLogin.singleton, direct.singleton, select.singleton);
        var accountRva = PreferInImage(pe, connectAuth.singleton, accountSetLogin == 0 ? 0 : SecondImageLoad(pe, setLogin.wrapper, setLogin.singleton));

        var startGameNative = PreferLoginNative(pe, startGame.native, startGame.wrapper, connectAuth.native);
        var notifyGame = RecoverNotifyGamePhase(pe, evidence);
        var enterGameNative = PreferLoginNative(pe, enterGamePkt.native, enterGamePkt.wrapper, connectAuth.native);
        evidence.Add($"SetGamePhase RVA 0x{setGamePhase:X} SetLoadingPhase RVA 0x{setLoadingPhase:X} StartGame native 0x{startGameNative:X} NotifyGamePhase 0x{notifyGame:X} SendEnterGamePacket 0x{enterGameNative:X}.");
        if (startGameNative != 0)
            evidence.Add($"StartGame bytes={pe.Hex(pe.ImageBase + startGameNative, 16)}.");
        if (setHandler.native != 0)
            evidence.Add($"SetHandler native 0x{setHandler.native:X} wrapper=0x{setHandler.wrapper:X} bytes={pe.Hex(pe.ImageBase + setHandler.native, 32)}.");
        if (setGameWindow.native != 0)
            evidence.Add($"SetGameWindow native 0x{setGameWindow.native:X} wrapper=0x{setGameWindow.wrapper:X} bytes={pe.Hex(pe.ImageBase + setGameWindow.native, 48)}.");
        evidence.Add("net.SetLoginInfo writes CPythonNetworkStream and CAccountConnector credentials.");
        evidence.Add("net.ConnectToAccountServer is CAccountConnector::Connect(channel, auth).");
        evidence.Add("net.DirectEnter is CPythonNetworkStream::ConnectGameServer(slot) and sets DirectEnter mode.");
        evidence.Add("Login/Handshake skip Python UI when DirectEnter mode is set.");
        evidence.Add("LoadingWindow.__StartGame sets CPythonNetworkStream+0x8C; 0x11C0C0 then PyCalls MainStream.SetGamePhase to open GameWindow.");
        var loginStr = RecoverLoginStringDisps(pe, setLogin.native, evidence);

        return new LoginNativeLayout(
            netRva,
            accountRva,
            setLogin.native,
            accountSetLogin,
            connectAuth.native,
            direct.native,
            select.native,
            setPhase,
            logoClose.native,
            phaseDisp,
            setGamePhase,
            startGameNative,
            notifyGame,
            enterGameNative,
            evidence)
        {
            LoginIdDisplacement = loginStr.id,
            LoginPasswordDisplacement = loginStr.pwd
        };
    }

    public static MiniMapLayout RecoverMiniMap(string path)
    {
        var pe = ActionPe.Load(path);
        var evidence = new List<string>();
        var found = RecoverMiniMap(pe, evidence);
        return new MiniMapLayout(found.singleton, found.show, found.hide, found.load, found.update, found.render, found.isShow, evidence);
    }

    private static (uint singleton, uint show, uint hide, uint load, uint update, uint render, uint isShow) RecoverMiniMap(ActionPe pe, List<string> evidence)
    {
        foreach (var name in new[]
                 {
                     "ShowAtlasWindow", "HideAtlasWindow", "ShowAtlas", "HideAtlas", "isShowAtlas",
                     "LoadAtlas", "UpdateAtlas", "RenderAtlas", "isAtlas", "atlas.sub", "atlas.dds",
                     "atlas.tga", "miniMap"
                 })
        {
            if (pe.TryFindAscii(name, out var va))
                evidence.Add($"string '{name}' VA 0x{va:X}");
            else
                evidence.Add($"string '{name}' missing");
        }

        if (pe.TryFindAscii("LoadAtlas", out var cluster))
        {
            var cursor = cluster > 0x80 ? cluster - 0x80 : cluster;
            var end = cluster + 0x180;
            while (cursor < end)
            {
                var text = pe.CString(cursor);
                if (text.Length >= 3 && text.Length < 40 && text.All(ch => ch is >= (char)0x20 and < (char)0x7F))
                    evidence.Add($"atlas-cluster '{text}' VA 0x{cursor:X}");
                cursor += (uint)Math.Max(1, text.Length + 1);
            }
        }

        var load = RecoverMiniMapNative(pe, "LoadAtlas", evidence);
        var update = RecoverMiniMapNative(pe, "UpdateAtlas", evidence);
        var render = RecoverMiniMapNative(pe, "RenderAtlas", evidence);
        var isShow = RecoverMiniMapNative(pe, "isShowAtlas", evidence);
        var show = RecoverMiniMapNative(pe, "ShowAtlas", evidence);
        if (show.native == 0)
            show = RecoverMiniMapNative(pe, "ShowAtlasWindow", evidence);
        var hide = RecoverMiniMapNative(pe, "HideAtlas", evidence);
        if (hide.native == 0)
            hide = RecoverMiniMapNative(pe, "HideAtlasWindow", evidence);
        var size = RecoverMiniMapNative(pe, "GetAtlasSize", evidence);
        RecoverMiniMapNative(pe, "GetAtlasInfo", evidence);
        RecoverMiniMapNative(pe, "IsAtlas", evidence);
        if (load.native != 0)
        {
            evidence.Add($"LoadAtlas native bytes {pe.Hex(pe.ImageBase + load.native, 320)}");
            evidence.Add("LoadAtlas ecx-disp " + string.Join(", ", EcxDisplacements(pe, load.native).Select(d => $"0x{d:X}")));
            RecoverAtlasImageStore(pe, load.native, evidence);
        }
        if (size.native != 0)
        {
            evidence.Add($"GetAtlasSize native bytes {pe.Hex(pe.ImageBase + size.native, 96)}");
            evidence.Add("GetAtlasSize ecx-disp " + string.Join(", ", EcxDisplacements(pe, size.native).Select(d => $"0x{d:X}")));
        }
        foreach (var name in new[] { "atlas", "Atlas", "maps/%s", "minimap", "MiniMap_Atlas" })
        {
            if (pe.TryFindAscii(name, out var va))
                evidence.Add($"path-string '{name}' VA 0x{va:X}");
        }
        var singleton = PreferInImage(pe, load.singleton, update.singleton, render.singleton, isShow.singleton, show.singleton, hide.singleton);
        return (singleton, show.native, hide.native, load.native, update.native, render.native, isShow.native);
    }

    private static (uint native, uint singleton) RecoverMiniMapNative(ActionPe pe, string name, List<string> evidence)
    {
        var found = RecoverNamedNative(pe, name, evidence);
        if (found.wrapper == 0)
            return (0, 0);
        var native = FirstCallAfterSingletonLoad(pe, found.wrapper, out var singleton);
        if (native == 0)
            native = found.native;
        if (singleton == 0)
            singleton = found.singleton;
        evidence.Add($"miniMap.{name} wrapper=0x{found.wrapper:X} native=0x{native:X} singleton=0x{singleton:X} bytes={pe.Hex(pe.ImageBase + found.wrapper, 48)}");
        return (native, singleton);
    }

    private static void RecoverAtlasImageStore(ActionPe pe, uint loadRva, List<string> evidence)
    {
        var off = pe.VaToFile(pe.ImageBase + loadRva);
        if (off < 0)
            return;
        for (var i = off; i < off + 0x200 && i + 11 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x8D || pe.Byte(i + 1) != 0x8F || pe.ReadU32(i + 2) != 0x26C || pe.Byte(i + 6) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 11) + pe.ReadI32(i + 7));
            if (!pe.IsCode(target))
                continue;
            evidence.Add($"SetImage RVA 0x{target - pe.ImageBase:X} bytes={pe.Hex(target, 80)}");
            if (i + 16 < pe.Length && pe.Byte(i + 11) == 0x8B && pe.Byte(i + 12) == 0xCE && pe.Byte(i + 13) == 0xE8)
            {
                var imgFn = (uint)(pe.FileToVa(i + 18) + pe.ReadI32(i + 14));
                evidence.Add($"ImageAfterSet RVA 0x{imgFn - pe.ImageBase:X} bytes={pe.Hex(imgFn, 64)}");
                evidence.Add("ImageAfterSet ecx-disp " + string.Join(", ", EcxDisplacements(pe, imgFn - pe.ImageBase).Select(d => $"0x{d:X}")));
            }
            var fn = pe.VaToFile(target);
            for (var j = fn; j < fn + 0x60 && j + 6 < pe.Length; j++)
            {
                if (pe.Byte(j) == 0x89 && pe.Byte(j + 1) is 0x41 or 0x46 or 0x4E or 0x4F)
                    evidence.Add($"SetImage store [reg+0x{pe.Byte(j + 2):X}]");
                if (pe.Byte(j) == 0x89 && pe.Byte(j + 1) is 0x81 or 0x86 or 0x87)
                    evidence.Add($"SetImage store [reg+0x{pe.ReadU32(j + 2):X}]");
            }
        }
    }

    private static List<uint> EcxDisplacements(ActionPe pe, uint rva)
    {
        var found = new List<uint>();
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return found;
        for (var i = off; i < off + 0x180 && i + 6 < pe.Length; i++)
        {
            var b0 = pe.Byte(i);
            var b1 = pe.Byte(i + 1);
            if ((b0 == 0x8B || b0 == 0x89 || b0 == 0x8D || b0 == 0xD9 || b0 == 0xF3) &&
                (b1 == 0x81 || b1 == 0x89 || b1 == 0x80 || b1 == 0xB1 || b1 == 0x88))
            {
                var disp = pe.ReadU32(i + 2);
                if (disp is > 4 and < 0x20000 && !found.Contains(disp))
                    found.Add(disp);
            }
            if (b0 == 0x8B && b1 is 0x41 or 0x49 or 0x51 or 0x59 or 0x69 or 0x71 or 0x79)
            {
                var disp = pe.Byte(i + 2);
                if (disp > 4 && !found.Contains(disp))
                    found.Add(disp);
            }
        }
        return found;
    }

    private static uint FirstCallAfterSingletonLoad(ActionPe pe, uint wrapperRva, out uint singletonRva)
    {
        singletonRva = 0;
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        var start = off;
        for (var i = off; i < off + 0x40 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x8B || (pe.Byte(i + 1) != 0x0D && pe.Byte(i + 1) != 0x15))
                continue;
            var addr = pe.ReadU32(i + 2);
            if (!pe.IsVa(addr))
                continue;
            singletonRva = addr - pe.ImageBase;
            start = i + 6;
            break;
        }

        for (var i = start; i < start + 0x20 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (pe.IsCode(target))
                return target - pe.ImageBase;
        }

        return 0;
    }

    private static uint PreferInImage(ActionPe pe, params uint[] rvas)
    {
        foreach (var rva in rvas)
        {
            if (rva != 0 && pe.IsRva(rva))
                return rva;
        }

        return 0;
    }

    private static (uint native, uint singleton, uint wrapper) TryNamed(
        ActionPe pe, IEnumerable<string> names, uint preferSingleton, List<string> evidence)
    {
        foreach (var name in names)
        {
            var probe = new List<string>();
            var found = preferSingleton != 0
                ? RecoverNamedNativePrefer(pe, name, preferSingleton, probe)
                : RecoverNamedNative(pe, name, probe);
            if (found.native == 0 && found.wrapper == 0)
                continue;
            evidence.AddRange(probe);
            return found;
        }

        evidence.Add("none of " + string.Join("/", names) + " recovered.");
        return (0, 0, 0);
    }

    private static uint LastSubstantialCall(ActionPe pe, uint rva)
    {
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        uint last = 0;
        for (var i = off; i < off + 0xA0 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || !pe.IsCode(target) || LooksLikePythonStub(pe, destOff))
                continue;
            last = target;
        }

        return last;
    }

    private static bool FunctionWritesHeader(ActionPe pe, int fnOff, byte header)
    {
        for (var i = fnOff; i < fnOff + 0x70 && i + 3 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xC6 && pe.Byte(i + 1) == 0x45 && pe.Byte(i + 3) == header)
                return true;
            if (pe.Byte(i) == 0xC7 && pe.Byte(i + 1) == 0x45 && pe.Byte(i + 3) == header)
                return true;
            if (pe.Byte(i) == 0xC6 && pe.Byte(i + 1) is 0x00 or 0x01 or 0x02 or 0x03 or 0x06 or 0x07 &&
                pe.Byte(i + 2) == header)
                return true;
        }

        return false;
    }

    private static uint RecoverPacketByHeader(ActionPe pe, uint siblingNative, byte header, List<string> evidence, string name)
    {
        if (siblingNative == 0)
            return 0;
        var sendVa = LastSubstantialCall(pe, siblingNative);
        if (sendVa == 0)
        {
            evidence.Add($"{name}: no Send() callee on sibling 0x{siblingNative:X}.");
            return 0;
        }

        foreach (var call in FindRelativeTransfersTo(pe, sendVa))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            var fnOff = FunctionStart(pe, callOff);
            var fnRva = pe.FileToVa(fnOff) - pe.ImageBase;
            if (fnRva == siblingNative || !pe.IsRva(fnRva))
                continue;
            if (!FunctionWritesHeader(pe, fnOff, header))
                continue;
            evidence.Add($"{name} RVA 0x{fnRva:X} shares Send() with 0x{siblingNative:X}, header {header}.");
            return fnRva;
        }

        evidence.Add($"{name}: no sibling Send() caller writes header {header}.");
        return 0;
    }

    private static uint RecoverNetworkSend(ActionPe pe, uint packetNative, List<string> evidence)
    {
        if (packetNative == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + packetNative);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x80 && i + 8 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x6A)
                continue;
            var size = pe.Byte(i + 1);
            if (size is < 4 or > 40)
                continue;
            for (var j = i + 2; j < i + 18 && j + 5 < pe.Length; j++)
            {
                if (pe.Byte(j) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(j + 5) + pe.ReadI32(j + 1));
                var destOff = pe.VaToFile(target);
                if (destOff < 0 || !pe.IsCode(target) || LooksLikePythonStub(pe, destOff) || LooksLikePythonHelper(pe, destOff))
                    continue;
                evidence.Add($"network Send() RVA 0x{target - pe.ImageBase:X} size {size} from packet 0x{packetNative:X}.");
                return target - pe.ImageBase;
            }
        }

        evidence.Add($"no Send() after size-push in packet 0x{packetNative:X}.");
        return 0;
    }

    private static uint RecoverSiblingPacket(
        ActionPe pe, uint sendRva, uint siblingNative, byte wantSize, ushort retBytes, List<string> evidence, string name)
    {
        if (sendRva == 0)
            return 0;
        uint found = 0;
        var bestScore = -1;
        foreach (var call in pe.FindRelativeCallsTo(pe.ImageBase + sendRva))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            var fnOff = FunctionStart(pe, callOff);
            var fnRva = pe.FileToVa(fnOff) - pe.ImageBase;
            if (fnRva == siblingNative || !pe.IsRva(fnRva))
                continue;
            var dist = fnRva > siblingNative ? fnRva - siblingNative : siblingNative - fnRva;
            if (dist > 0x20000)
                continue;
            if (LastImm8PushBefore(pe, fnOff, callOff) != wantSize)
                continue;
            if (!FunctionHasRet(pe, fnOff, retBytes))
                continue;
            var score = 1;
            if (LooksLikeBattleAttack(pe, fnOff))
                score += 4;
            if (found == 0 || score > bestScore || (score == bestScore && fnRva < found))
            {
                found = fnRva;
                bestScore = score;
            }
        }

        if (found != 0)
            evidence.Add($"{name} RVA 0x{found:X} shares Send() with 0x{siblingNative:X}, size {wantSize}.");
        else
            evidence.Add($"{name}: no Send() caller size {wantSize} ret {retBytes}.");
        return found;
    }

    private static uint RecoverOnPressActor(ActionPe pe, uint setTargetRva, uint wrapperRva, uint reserveRva, List<string> evidence)
    {
        var found = RecoverOnPressActorFromCalls(pe, setTargetRva, wrapperRva, setTargetRva, evidence, "SetTarget");
        if (found != 0)
            return found;
        if (reserveRva != 0)
            found = RecoverOnPressActorFromCalls(pe, reserveRva, wrapperRva, setTargetRva, evidence, "ReserveClick");
        return found;
    }

    private static uint RecoverOnPressActorFromCalls(ActionPe pe, uint targetRva, uint wrapperRva, uint setTargetRva, List<string> evidence, string via)
    {
        if (targetRva == 0)
            return 0;
        foreach (var call in FindRelativeTransfersTo(pe, pe.ImageBase + targetRva))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            var prologue = NearestPrologue(pe, callOff, 0x400);
            if (prologue < 0)
                continue;
            var fnRva = pe.FileToVa(prologue) - pe.ImageBase;
            if (fnRva == wrapperRva || fnRva == setTargetRva || fnRva == targetRva || !pe.IsRva(fnRva))
                continue;
            var end = NextFunction(pe, prologue + 4);
            if (!HasRet12(pe, callOff, end))
                continue;
            evidence.Add($"__OnPressActor RVA 0x{fnRva:X} calls {via}, ret 12.");
            return fnRva;
        }

        return 0;
    }

    private static (uint rva, uint modeDisp, uint vidDisp) RecoverReserveClick(ActionPe pe, uint comboRva, List<string> evidence)
    {
        if (comboRva == 0)
            return default;
        var off = pe.VaToFile(pe.ImageBase + comboRva);
        if (off < 0)
            return default;
        for (var i = off; i < off + 0x280 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var destOff = pe.VaToFile((uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1)));
            if (destOff < 0)
                continue;
            if (pe.Byte(destOff) != 0x55 || pe.Byte(destOff + 1) != 0x8B || pe.Byte(destOff + 2) != 0xEC)
                continue;
            if (pe.Byte(destOff + 3) != 0x8B || pe.Byte(destOff + 4) != 0x45 || pe.Byte(destOff + 5) != 0x08)
                continue;
            if (pe.Byte(destOff + 6) != 0xC7 || pe.Byte(destOff + 7) != 0x41)
                continue;
            if (pe.Byte(destOff + 9) != 0x03 || pe.Byte(destOff + 10) != 0 || pe.Byte(destOff + 11) != 0 || pe.Byte(destOff + 12) != 0)
                continue;
            if (pe.Byte(destOff + 13) != 0x89 || pe.Byte(destOff + 14) != 0x81)
                continue;
            if (!FunctionHasRet(pe, destOff, 4))
                continue;
            var rva = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1) - pe.ImageBase);
            var modeDisp = pe.Byte(destOff + 8);
            var vidDisp = pe.ReadU32(destOff + 15);
            evidence.Add($"ReserveClickActor RVA 0x{rva:X} mode=+0x{modeDisp:X} vid=+0x{vidDisp:X} from Combo/NEW_Attack (TeraBox 0xD1B90 equivalent).");
            return (rva, modeDisp, vidDisp);
        }

        return default;
    }

    private static uint RecoverGetMainActor(ActionPe pe, uint setTargetRva, List<string> evidence)
    {
        if (setTargetRva == 0)
            return 0;
        foreach (var call in FindRelativeTransfersTo(pe, pe.ImageBase + setTargetRva))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            var prologue = NearestPrologue(pe, callOff, 0x400);
            if (prologue < 0)
                continue;
            for (var i = prologue; i < callOff && i + 5 < pe.Length; i++)
            {
                if (pe.Byte(i) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                var destOff = pe.VaToFile(target);
                if (destOff < 0)
                    continue;
                if (pe.Byte(destOff) == 0xFF && pe.Byte(destOff + 1) == 0x71 && pe.Byte(destOff + 2) == 0x28 &&
                    pe.Byte(destOff + 3) == 0x83 && pe.Byte(destOff + 4) == 0xC1 && pe.Byte(destOff + 5) == 0xFC)
                {
                    var rva = target - pe.ImageBase;
                    evidence.Add($"IAbstractPlayer::GetMainActor thunk RVA 0x{rva:X} (push [ecx+0x28], this-4, map lookup).");
                    return rva;
                }
            }
        }

        return 0;
    }

    private static uint RecoverGetPixel(ActionPe pe, uint moveActorRva, List<string> evidence)
    {
        var off = pe.VaToFile(pe.ImageBase + moveActorRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x40 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var destOff = pe.VaToFile(target);
            if (destOff < 0)
                continue;
            if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC &&
                pe.Byte(destOff + 3) == 0x81 && pe.Byte(destOff + 4) == 0xC1)
            {
                var rva = target - pe.ImageBase;
                evidence.Add($"CInstanceBase::GetPixelPosition RVA 0x{rva:X} from MoveToDestActor.");
                return rva;
            }
        }

        return 0;
    }

    private static uint RecoverMoveToPixel(ActionPe pe, uint moveActorRva, List<string> evidence)
    {
        var off = pe.VaToFile(pe.ImageBase + moveActorRva);
        if (off < 0)
            return 0;
        var seen = 0;
        for (var i = off; i < off + 0x40 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var rva = target - pe.ImageBase;
            var destOff = pe.VaToFile(target);
            if (destOff < 0)
                continue;
            seen++;
            if (seen < 2)
                continue;
            if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC &&
                FunctionHasRet(pe, destOff, 4))
            {
                evidence.Add($"CInstanceBase::MoveToDestPixelPosition RVA 0x{rva:X} from MoveToDestActor.");
                return rva;
            }
        }

        return 0;
    }

    private static uint RecoverMoveToActor(ActionPe pe, uint setTargetRva, List<string> evidence)
    {
        if (setTargetRva == 0)
            return 0;
        foreach (var call in FindRelativeTransfersTo(pe, pe.ImageBase + setTargetRva))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            for (var i = callOff + 5; i < callOff + 0x20 && i + 5 < pe.Length; i++)
            {
                if (pe.Byte(i) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                var rva = target - pe.ImageBase;
                if (rva == setTargetRva || !pe.IsRva(rva))
                    continue;
                var destOff = pe.VaToFile(target);
                if (destOff < 0 || LooksLikePythonHelper(pe, destOff))
                    continue;
                if (pe.Byte(destOff) != 0x55 || pe.Byte(destOff + 1) != 0x8B || pe.Byte(destOff + 2) != 0xEC)
                    continue;
                if (!FunctionHasRet(pe, destOff, 4) || !CallsGetPixelPosition(pe, destOff))
                    continue;
                evidence.Add($"CInstanceBase::MoveToDestActor RVA 0x{rva:X} after SetTarget (this=main, arg=target instance).");
                return rva;
            }
        }

        return 0;
    }

    private static bool CallsGetPixelPosition(ActionPe pe, int fnOff)
    {
        for (var i = fnOff; i < fnOff + 0x40 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var destOff = pe.VaToFile((uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1)));
            if (destOff < 0)
                continue;
            if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC &&
                pe.Byte(destOff + 3) == 0x81 && pe.Byte(destOff + 4) == 0xC1)
                return true;
        }

        return false;
    }

    private static bool HasPushDisplacement(ActionPe pe, int from, int to, uint disp)
    {
        var end = Math.Min(pe.Length - 6, to);
        for (var i = Math.Max(0, from); i < end; i++)
        {
            if (pe.Byte(i) != 0xFF)
                continue;
            var mod = pe.Byte(i + 1);
            if (mod is 0xB0 or 0xB1 or 0xB2 or 0xB3 or 0xB6 or 0xB7 && pe.ReadU32(i + 2) == disp)
                return true;
        }

        return false;
    }

    private static bool CallsRva(ActionPe pe, int from, int to, uint rva)
    {
        if (rva == 0)
            return false;
        var end = Math.Min(pe.Length - 5, to);
        for (var i = Math.Max(0, from); i < end; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            if ((uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1) - pe.ImageBase) == rva)
                return true;
        }

        return false;
    }

    private static uint RecoverChaseActor(ActionPe pe, uint getMainRva, uint vidDisp, uint reserveRva, List<string> evidence)
    {
        if (vidDisp == 0 || getMainRva == 0)
            return 0;
        foreach (var call in FindRelativeTransfersTo(pe, pe.ImageBase + getMainRva))
        {
            var callOff = pe.VaToFile(call);
            if (callOff < 0)
                continue;
            var prologue = NearestPrologue(pe, callOff, 0x20);
            if (prologue < 0)
                continue;
            if (!HasPushDisplacement(pe, prologue, prologue + 0x20, vidDisp))
                continue;
            var rva = pe.FileToVa(prologue) - pe.ImageBase;
            if (!pe.IsRva(rva))
                continue;
            evidence.Add($"CPythonPlayer reserved-VID chase RVA 0x{rva:X} (GetMainActor, then MoveToDest when out of range).");
            return rva;
        }

        if (reserveRva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + reserveRva);
        if (off < 0)
            return 0;
        var end = Math.Min(pe.Length - 6, off + 0x200);
        for (var i = off + 0x10; i < end; i++)
        {
            if (pe.Byte(i) != 0x55 || pe.Byte(i + 1) != 0x8B || pe.Byte(i + 2) != 0xEC)
                continue;
            if (!CallsRva(pe, i, i + 0x20, getMainRva))
                continue;
            if (!HasPushDisplacement(pe, i, i + 0x20, vidDisp))
                continue;
            var rva = pe.FileToVa(i) - pe.ImageBase;
            if (!pe.IsRva(rva))
                continue;
            evidence.Add($"CPythonPlayer reserved-VID chase RVA 0x{rva:X} after ReserveClickActor.");
            return rva;
        }

        return 0;
    }

    private static uint RecoverApproachInst(ActionPe pe, uint chaseRva, uint moveToRva, List<string> evidence)
    {
        if (chaseRva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + chaseRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x80 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var rva = target - pe.ImageBase;
            if (rva == chaseRva || rva == moveToRva || !pe.IsRva(rva))
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff < 0)
                continue;
            if (pe.Byte(destOff) != 0x55 || pe.Byte(destOff + 1) != 0x8B || pe.Byte(destOff + 2) != 0xEC)
                continue;
            if (pe.Byte(destOff + 3) != 0x83 || pe.Byte(destOff + 4) != 0xEC)
                continue;
            if (!FunctionHasRet(pe, destOff, 4))
                continue;
            evidence.Add($"CInstanceBase out-of-range approach RVA 0x{rva:X} from reserved-VID chase.");
            return rva;
        }

        return 0;
    }

    private static uint RecoverInRangeCheck(ActionPe pe, uint chaseRva, uint approachInstRva, uint comboRva, List<string> evidence)
    {
        uint found = 0;
        if (chaseRva != 0 && approachInstRva != 0)
        {
            var off = pe.VaToFile(pe.ImageBase + chaseRva);
            if (off < 0)
                off = pe.Length;
            uint prev = 0;
            for (var i = off; i < off + 0x80 && i + 5 < pe.Length; i++)
            {
                if (pe.Byte(i) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                var rva = target - pe.ImageBase;
                if (rva == approachInstRva && prev != 0)
                {
                    found = prev;
                    break;
                }

                var destOff = pe.VaToFile(target);
                if (destOff < 0)
                    continue;
                if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC &&
                    pe.Byte(destOff + 3) == 0x51 && pe.Byte(destOff + 4) == 0xF3 && pe.Byte(destOff + 5) == 0x0F &&
                    pe.Byte(destOff + 6) == 0x10)
                    prev = rva;
            }
        }

        if (found == 0 && comboRva != 0)
        {
            var off = pe.VaToFile(pe.ImageBase + comboRva);
            if (off < 0)
                off = pe.Length;
            for (var i = off; i < off + 0x180 && i + 5 < pe.Length; i++)
            {
                if (pe.Byte(i) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                var destOff = pe.VaToFile(target);
                if (destOff < 0)
                    continue;
                if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC &&
                    pe.Byte(destOff + 3) == 0x51 && pe.Byte(destOff + 4) == 0xF3 && pe.Byte(destOff + 5) == 0x0F &&
                    pe.Byte(destOff + 6) == 0x10)
                {
                    found = target - pe.ImageBase;
                    break;
                }
            }
        }

        if (found != 0 && pe.IsRva(found))
            evidence.Add($"CInstanceBase melee-range check RVA 0x{found:X} (150px default; walk if false, attack if true).");
        return pe.IsRva(found) ? found : 0;
    }

    private static uint RecoverPlayerUpdate(ActionPe pe, uint getMainRva, uint modeDisp, List<string> evidence)
    {
        if (getMainRva == 0 || modeDisp == 0 || modeDisp > 0xFF)
            return 0;
        var center = pe.VaToFile(pe.ImageBase + getMainRva);
        if (center < 0)
            return 0;
        var start = Math.Max(0, center - 0x1000);
        var end = Math.Min(pe.Length - 7, center + 0x20000);
        for (var i = start; i < end; i++)
        {
            if (pe.Byte(i) != 0x8B || pe.Byte(i + 2) != (byte)modeDisp)
                continue;
            if (pe.Byte(i + 1) is not (0x4B or 0x4E or 0x4F or 0x48 or 0x49))
                continue;
            if (pe.Byte(i + 3) != 0x49 || pe.Byte(i + 4) != 0x83 || pe.Byte(i + 5) != 0xF9 || pe.Byte(i + 6) != 0x04)
                continue;
            var prologue = NearestPrologue(pe, i, 0x80);
            if (prologue < 0 || !CallsRva(pe, prologue, i + 1, getMainRva))
                continue;
            var rva = pe.FileToVa(prologue) - pe.ImageBase;
            if (!pe.IsRva(rva))
                continue;
            evidence.Add($"CPythonPlayer::Update RVA 0x{rva:X} switches on reserved mode +0x{modeDisp:X}.");
            return rva;
        }

        return 0;
    }

    private static uint RecoverReserveFromPress(
        ActionPe pe, uint pressRva, uint setTargetRva, uint comboRva, List<string> evidence)
    {
        var off = pe.VaToFile(pe.ImageBase + pressRva);
        if (off < 0)
            return 0;
        var afterSet = -1;
        for (var i = off; i < off + 0x280 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            if ((uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1)) - pe.ImageBase == setTargetRva)
                afterSet = i + 5;
        }

        if (afterSet < 0)
            return 0;
        for (var i = afterSet; i < off + 0x280 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var rva = target - pe.ImageBase;
            if (rva == setTargetRva || rva == comboRva || rva == pressRva || !pe.IsRva(rva))
                continue;
            var dist = rva > pressRva ? rva - pressRva : pressRva - rva;
            if (dist > 0x8000)
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || LooksLikePythonHelper(pe, destOff))
                continue;
            if (pe.Byte(destOff) != 0x55 || pe.Byte(destOff + 1) != 0x8B || pe.Byte(destOff + 2) != 0xEC)
                continue;
            if (!FunctionHasRet(pe, destOff, 4))
                continue;
            evidence.Add($"ReserveClickActor RVA 0x{rva:X} from __OnPressActor after SetTarget.");
            return rva;
        }

        return 0;
    }

    private static int NextFunction(ActionPe pe, int from)
    {
        var end = Math.Min(pe.Length - 3, from + 0x800);
        for (var i = from; i < end; i++)
        {
            if (pe.Byte(i) == 0xCC && pe.Byte(i + 1) == 0x55 && pe.Byte(i + 2) == 0x8B && pe.Byte(i + 3) == 0xEC)
                return i + 1;
        }

        return end;
    }

    private static int NearestPrologue(ActionPe pe, int callOff, int maxBack)
    {
        var start = Math.Max(0, callOff - maxBack);
        for (var i = callOff; i >= start; i--)
        {
            if (pe.Byte(i) == 0x55 && pe.Byte(i + 1) == 0x8B && pe.Byte(i + 2) == 0xEC)
                return i;
        }

        return -1;
    }

    private static bool HasRet12(ActionPe pe, int from, int to)
    {
        var end = Math.Min(pe.Length - 2, to);
        for (var i = Math.Max(0, from); i < end; i++)
        {
            if (pe.Byte(i) == 0xC2 && pe.Byte(i + 1) == 0x0C && pe.Byte(i + 2) == 0)
                return true;
        }

        return false;
    }

    private static byte LastImm8PushBefore(ActionPe pe, int from, int to)
    {
        byte size = 0;
        for (var i = from; i + 1 < to; i++)
        {
            if (pe.Byte(i) == 0x6A)
                size = pe.Byte(i + 1);
        }

        return size;
    }

    private static bool LooksLikeBattleAttack(ActionPe pe, int fnOff)
    {
        var sawByte = false;
        var sawDword = false;
        for (var i = fnOff; i < fnOff + 0x24 && i + 3 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8A && pe.Byte(i + 1) == 0x45 && pe.Byte(i + 2) == 0x08)
                sawByte = true;
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x45 && pe.Byte(i + 2) == 0x0C)
                sawDword = true;
        }

        return sawByte && sawDword;
    }

    private static List<(string name, uint wrapper)> NeighborMethods(ActionPe pe, uint knownWrapper, uint playerSingletonRva)
    {
        var result = new List<(string name, uint wrapper)>();
        if (knownWrapper == 0)
            return result;
        var preferVa = playerSingletonRva == 0 ? 0 : pe.ImageBase + playerSingletonRva;
        foreach (var site in pe.FindImmediateXrefs(pe.ImageBase + knownWrapper))
        {
            var methOff = pe.VaToFile(site);
            if (methOff < 4)
                continue;
            var flags = pe.ReadU32(methOff + 4);
            if ((flags & 0xFF) is not (1 or 2 or 4 or 8 or 9))
                continue;
            var table = methOff - 4;
            for (var i = -80; i <= 80; i++)
            {
                var at = table + i * 16;
                if (at < 0 || at + 12 >= pe.Length)
                    continue;
                var namePtr = pe.ReadU32(at);
                var fn = pe.ReadU32(at + 4);
                var fl = pe.ReadU32(at + 8);
                if ((fl & 0xFF) is not (1 or 2 or 4 or 8 or 9) || !pe.IsCode(fn) || !pe.IsVa(namePtr))
                    continue;
                var name = pe.CString(namePtr, 80);
                if (name.Length is < 2 or > 64)
                    continue;
                if (preferVa != 0)
                {
                    var loaded = FirstGlobalLoad(pe, fn - pe.ImageBase);
                    if (loaded != 0 && loaded != preferVa)
                        continue;
                }

                if (result.Exists(item => item.wrapper == fn - pe.ImageBase))
                    continue;
                result.Add((name, fn - pe.ImageBase));
            }

            if (result.Count > 3)
                break;
        }

        return result;
    }

    private static uint FirstCallRva(ActionPe pe, uint fnRva)
    {
        var off = pe.VaToFile(pe.ImageBase + fnRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x40 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (pe.IsCode(target))
                return target - pe.ImageBase;
        }

        return 0;
    }

    private static bool FunctionHasRet(ActionPe pe, int fnOff, ushort bytes)
    {
        for (var i = fnOff; i < fnOff + 0xC0 && i + 3 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xC2 && pe.Byte(i + 1) == (byte)bytes && pe.Byte(i + 2) == (byte)(bytes >> 8))
                return true;
        }

        return false;
    }

    private static bool FunctionPushesImm8(ActionPe pe, int fnOff, byte imm)
    {
        for (var i = fnOff; i < fnOff + 0x80 && i + 1 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x6A && pe.Byte(i + 1) == imm)
                return true;
        }

        return false;
    }

    private static bool FunctionCalls(ActionPe pe, int fnOff, uint targetVa)
    {
        for (var i = fnOff; i < fnOff + 0xA0 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var dest = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (dest == targetVa)
                return true;
        }

        return false;
    }

    private static (uint native, uint singleton, uint wrapper) RecoverNamedNative(ActionPe pe, string name, List<string> evidence)
    {
        var foundString = false;
        foreach (var va in pe.FindAsciiZ(name))
        {
            foundString = true;
            foreach (var site in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(site);
                if (off < 0 || !TryMethodTable(pe, off, out var fn))
                    continue;
                var wrapperRva = fn - pe.ImageBase;
                var singleton = FirstGlobalLoad(pe, wrapperRva);
                var native = NativeCallAfterSingleton(pe, wrapperRva, singleton);
                var singletonRva = singleton == 0 ? 0 : singleton - pe.ImageBase;
                evidence.Add($"Python {name} wrapper RVA 0x{wrapperRva:X} singletonRva=0x{singletonRva:X} nativeRVA=0x{native:X} bytes={pe.Hex(pe.ImageBase + wrapperRva, 96)}.");
                return (native, pe.IsRva(singletonRva) ? singletonRva : 0, wrapperRva);
            }
        }

        evidence.Add(foundString
            ? $"Python method '{name}' string exists but no METH_VARARGS table entry was accepted."
            : $"'{name}' string missing.");
        return (0, 0, 0);
    }

    private static (uint thisAdj, uint slot) RecoverVirtualDispatch(ActionPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return (0, 0);
        uint thisAdj = 0, slot = 0;
        var leaAt = -1;
        for (var i = off; i < off + 0x100 && i + 7 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
            {
                thisAdj = pe.Byte(i + 2);
                leaAt = i;
            }
            if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x49)
            {
                thisAdj = pe.Byte(i + 2);
                leaAt = i;
            }
            if (leaAt < 0 || i < leaAt)
                continue;
            if (slot == 0 && pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x50)
                slot = pe.Byte(i + 4);
            if (slot == 0 && pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x90)
                slot = pe.ReadU32(i + 4);
            if (slot == 0 && pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x11 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x52)
                slot = pe.Byte(i + 4);
            if (slot == 0 && pe.Byte(i) == 0xFF && pe.Byte(i + 1) == 0x51)
                slot = pe.Byte(i + 2);
            if (slot == 0 && pe.Byte(i) == 0xFF && pe.Byte(i + 1) == 0x91)
                slot = pe.ReadU32(i + 2);
        }

        return (thisAdj, slot);
    }

    private static uint NativeCallAfterThisAdjust(ActionPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x100 && i + 8 < pe.Length; i++)
        {
            var lea = pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x49 ||
                      pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1;
            if (!lea)
                continue;
            for (var j = i + 3; j < i + 18 && j + 5 < pe.Length; j++)
            {
                if (pe.Byte(j) == 0xFF && (pe.Byte(j + 1) is 0x50 or 0x51 or 0x90 or 0x91))
                    return 0;
                if (pe.Byte(j) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(j + 5) + pe.ReadI32(j + 1));
                var destOff = pe.VaToFile(target);
                if (destOff < 0 || !pe.IsCode(target) || LooksLikePythonStub(pe, destOff))
                    continue;
                return target - pe.ImageBase;
            }
        }

        return 0;
    }

    private static bool TryMethodTable(ActionPe pe, int off, out uint fn)
    {
        fn = 0;
        var meth = pe.ReadU32(off + 4);
        var flags = pe.ReadU32(off + 8);
        if ((flags & 0xFF) == 1 && pe.IsCode(meth))
        {
            fn = meth;
            return true;
        }

        if (off < 8)
            return false;
        flags = pe.ReadU32(off - 8);
        meth = pe.ReadU32(off + 4);
        if (flags == 1 && pe.IsCode(meth))
        {
            fn = meth;
            return true;
        }

        return false;
    }

    private static uint FindSingletonFromGetMainCharacterPosition(ActionPe pe, List<string> evidence)
    {
        if (!pe.TryFindAscii("GetMainCharacterPosition", out var va))
            return 0;
        foreach (var site in pe.FindImmediateXrefs(va))
        {
            var off = pe.VaToFile(site);
            if (off < 0 || !TryMethodTable(pe, off, out var fn))
                continue;
            var addr = FirstGlobalLoad(pe, fn - pe.ImageBase);
            if (addr != 0)
            {
                evidence.Add($"GetMainCharacterPosition singleton RVA 0x{addr - pe.ImageBase:X}.");
                return addr - pe.ImageBase;
            }
        }

        return 0;
    }

    private static uint FirstGlobalLoad(ActionPe pe, uint rva)
    {
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x80 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xA1)
            {
                var addr = pe.ReadU32(i + 1);
                if (pe.IsData(addr))
                    return addr;
            }

            if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                       pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D))
            {
                var addr = pe.ReadU32(i + 2);
                if (pe.IsData(addr))
                    return addr;
            }
        }

        return 0;
    }

    private static uint NativeCallAfterSingleton(ActionPe pe, uint wrapperRva, uint singletonVa)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;

        var start = off;
        if (singletonVa != 0)
        {
            for (var i = off; i < off + 0x140 && i + 6 < pe.Length; i++)
            {
                if (pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == singletonVa)
                {
                    start = i + 5;
                    break;
                }

                if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                           pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D) &&
                    pe.ReadU32(i + 2) == singletonVa)
                {
                    start = i + 6;
                    break;
                }
            }
        }

        var first = SubstantialThiscall(pe, start, off + 0x160);
        if (first != 0)
            return first;

        // playerSetTarget calls PyTuple_GetInteger before Instance(); skip that first E8.
        if (RelativeCall(pe, off, off + 0xC0) == 0)
            return 0;
        var afterFirst = off;
        for (var i = off; i < off + 0xC0 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            afterFirst = i + 5;
            break;
        }

        return SubstantialThiscall(pe, afterFirst, off + 0x100);
    }

    private static uint SubstantialThiscall(ActionPe pe, int start, int end)
    {
        return SubstantialThiscall(pe, start, end, skipPythonHelper: false);
    }

    private static uint SubstantialThiscall(ActionPe pe, int start, int end, bool skipPythonHelper)
    {
        for (var i = start; i < end && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (!pe.IsCode(target))
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || (pe.Byte(destOff) == 0xFF && pe.Byte(destOff + 1) == 0x25))
                continue;
            if (LooksLikePythonStub(pe, destOff))
                continue;
            if (skipPythonHelper && LooksLikePythonHelper(pe, destOff))
                continue;
            var rva = target - pe.ImageBase;
            if (skipPythonHelper && rva is 0x608290 or 0x608470 or 0x6081F0)
                continue;
            return rva;
        }

        return 0;
    }

    private static uint NativeCallSkippingPython(ActionPe pe, uint wrapperRva, uint singletonVa)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;

        var start = off;
        if (singletonVa != 0)
        {
            for (var i = off; i < off + 0xC0 && i + 6 < pe.Length; i++)
            {
                if (pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == singletonVa)
                {
                    start = i + 5;
                    break;
                }

                if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                           pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D) &&
                    pe.ReadU32(i + 2) == singletonVa)
                {
                    start = i + 6;
                    break;
                }
            }
        }

        var found = SubstantialThiscall(pe, start, off + 0x160, skipPythonHelper: true);
        if (found != 0)
            return found;
        return SubstantialThiscall(pe, off, off + 0x160, skipPythonHelper: true);
    }

    private static bool LooksLikePythonHelper(ActionPe pe, int off)
    {
        if (off < 0 || LooksLikePythonStub(pe, off))
            return true;
        return pe.Byte(off) == 0x55 && pe.Byte(off + 1) == 0x8B && pe.Byte(off + 2) == 0xEC &&
               pe.Byte(off + 3) == 0xFF && pe.Byte(off + 4) == 0x75 && pe.Byte(off + 5) == 0x08;
    }

    private static bool LooksLikePythonStub(ActionPe pe, int off)
    {
        if (pe.Byte(off) == 0xA1 && pe.Byte(off + 10) == 0xC3)
            return true;
        if (pe.Byte(off) == 0xA1 && pe.Byte(off + 5) == 0xC3)
            return true;
        var length = 0;
        for (var i = off; i < off + 48 && i < pe.Length; i++)
        {
            length++;
            if (pe.Byte(i) == 0xC3)
                break;
        }

        return length > 0 && length < 16;
    }

    private static uint PreferLoginNative(ActionPe pe, uint exportNative, uint wrapperRva, uint collideRva)
    {
        var followed = 0u;
        if (wrapperRva != 0)
        {
            followed = RelativeCall(
                pe,
                pe.VaToFile(pe.ImageBase + wrapperRva),
                pe.VaToFile(pe.ImageBase + wrapperRva) + 0x40);
        }

        if (LooksLikeStartGameFlag(pe, followed))
            return followed;
        if (LooksLikeStartGameFlag(pe, exportNative))
            return exportNative;
        if (exportNative != 0 && exportNative != collideRva)
            return exportNative;
        return followed != 0 ? followed : exportNative;
    }

    private static bool LooksLikeStartGameFlag(ActionPe pe, uint rva)
    {
        if (rva == 0)
            return false;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0 || off + 11 > pe.Length)
            return false;
        return pe.Byte(off) == 0xC7 && pe.ReadU32(off + 2) == 0x8C && pe.Byte(off + 10) == 0xC3;
    }

    private static uint RelativeCall(ActionPe pe, int start, int end)
    {
        for (var i = start; i < end && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (!pe.IsCode(target))
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff >= 0 && pe.Byte(destOff) == 0xFF && pe.Byte(destOff + 1) == 0x25)
                continue;
            return target - pe.ImageBase;
        }

        return 0;
    }

    private static (uint native, uint singleton, uint wrapper) RecoverLoginExport(ActionPe pe, string name, List<string> evidence)
    {
        var found = RecoverNamedNative(pe, name, evidence);
        var singleton = found.singleton != 0 ? found.singleton : FirstImageLoad(pe, found.wrapper);
        var native = found.native;
        if (singleton != 0)
            native = NativeCallAfterSingleton(pe, found.wrapper, pe.ImageBase + singleton);
        if (native == 0)
            native = found.native;
        evidence.Add($"Login export {name} wrapper=0x{found.wrapper:X} singleton=0x{singleton:X} native=0x{native:X}.");
        return (native, singleton, found.wrapper);
    }

    private static uint FirstImageLoad(ActionPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x160 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xA1 && pe.IsVa(pe.ReadU32(i + 1)))
                return pe.ReadU32(i + 1) - pe.ImageBase;
            if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                       pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D) &&
                pe.IsVa(pe.ReadU32(i + 2)))
                return pe.ReadU32(i + 2) - pe.ImageBase;
        }

        return 0;
    }

    private static uint SecondImageLoad(ActionPe pe, uint wrapperRva, uint firstSingletonRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        var seen = 0;
        for (var i = off; i < off + 0x180 && i + 6 < pe.Length; i++)
        {
            uint addr = 0;
            if (pe.Byte(i) == 0xA1)
                addr = pe.ReadU32(i + 1);
            else if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                           pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D))
                addr = pe.ReadU32(i + 2);
            if (addr == 0 || !pe.IsVa(addr))
                continue;
            var rva = addr - pe.ImageBase;
            if (rva == firstSingletonRva)
                continue;
            seen++;
            if (seen >= 1)
                return rva;
        }

        return 0;
    }

    private static uint SecondThiscallAfterSingleton(ActionPe pe, uint wrapperRva, uint singletonRva, List<string> evidence, string name)
    {
        if (wrapperRva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        var end = off + 0x90;
        for (var i = off; i < off + 0x90 && i < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xC3)
                end = i + 1;
        }

        var singletonVa = singletonRva == 0 ? 0 : pe.ImageBase + singletonRva;
        var seen = 0;
        var start = off;
        if (singletonVa != 0)
        {
            for (var i = off; i + 6 < end; i++)
            {
                if (pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == singletonVa)
                    start = i + 5;
                if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15) &&
                    pe.ReadU32(i + 2) == singletonVa)
                    start = i + 6;
            }
        }

        for (var i = start; i + 5 < end; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (!pe.IsCode(target))
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || LooksLikePythonStub(pe, destOff))
                continue;
            if (pe.Byte(destOff) == 0xC2 || pe.Byte(destOff) == 0xC3)
                continue;
            if (pe.Byte(destOff) == 0xFF && pe.Byte(destOff + 1) == 0x25)
                continue;
            seen++;
            if (seen != 2)
                continue;
            var rva = target - pe.ImageBase;
            evidence.Add($"Second {name} thiscall RVA 0x{rva:X}.");
            return rva;
        }

        return 0;
    }

    private static uint RecoverNotifyGamePhase(ActionPe pe, List<string> evidence)
    {
        if (!pe.TryFindAscii("SetGamePhase", out var va))
        {
            evidence.Add("'SetGamePhase' string missing.");
            return 0;
        }

        foreach (var site in pe.FindImmediateXrefs(va))
        {
            var off = pe.VaToFile(site);
            if (off < 0)
                continue;
            for (var i = off; i > off - 0x40 && i > 16; i--)
            {
                if (pe.Byte(i) != 0x56 || pe.Byte(i + 1) != 0x8B || pe.Byte(i + 2) != 0xF1)
                    continue;
                if (pe.Byte(i + 3) != 0x83 || pe.Byte(i + 4) != 0xBE || pe.ReadU32(i + 5) != 0x8C)
                    continue;
                var rva = pe.FileToVa(i) - pe.ImageBase;
                evidence.Add($"NotifyGamePhase RVA 0x{rva:X} PyCalls MainStream.SetGamePhase when +0x8C is set.");
                return rva;
            }
        }

        evidence.Add("'SetGamePhase' has no flag-gated PyCall wrapper.");
        return 0;
    }

    private static uint RecoverFunctionFromUniqueString(ActionPe pe, string text, List<string> evidence)
    {
        if (!pe.TryFindAscii(text, out var va))
        {
            evidence.Add($"'{text}' string missing.");
            return 0;
        }

        foreach (var site in pe.FindImmediateXrefs(va))
        {
            var off = pe.VaToFile(site);
            if (off < 0)
                continue;
            var start = -1;
            for (var i = off; i > off - 0x180 && i > 16; i--)
            {
                if (pe.Byte(i) != 0xCC || pe.Byte(i + 1) == 0xCC)
                    continue;
                start = i + 1;
                break;
            }

            if (start < 0)
            {
                for (var i = off; i > off - 0x80 && i > 16; i--)
                {
                    if (pe.Byte(i) == 0x55 && pe.Byte(i + 1) == 0x8B && pe.Byte(i + 2) == 0xEC)
                    {
                        start = i;
                        break;
                    }
                }
            }

            if (start < 0)
                continue;
            var rva = pe.FileToVa(start) - pe.ImageBase;
            if (!pe.IsRva(rva))
                continue;
            evidence.Add($"'{text}' xref function RVA 0x{rva:X}.");
            return rva;
        }

        evidence.Add($"'{text}' has no accepted code xref.");
        return 0;
    }

    private static (uint id, uint pwd) RecoverLoginStringDisps(ActionPe pe, uint rva, List<string> evidence)
    {
        if (rva == 0)
            return (0, 0);
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return (0, 0);
        var found = new List<uint>();
        for (var i = off; i < off + 0x120 && i + 6 < pe.Length; i++)
        {
            uint disp = 0;
            if (pe.Byte(i) == 0x8D && (pe.Byte(i + 1) == 0x81 || pe.Byte(i + 1) == 0x86 ||
                                       pe.Byte(i + 1) == 0x87 || pe.Byte(i + 1) == 0x88 ||
                                       pe.Byte(i + 1) == 0x89 || pe.Byte(i + 1) == 0x8B ||
                                       pe.Byte(i + 1) == 0x8E || pe.Byte(i + 1) == 0x8F))
                disp = pe.ReadU32(i + 2);
            else if (pe.Byte(i) == 0x81 && (pe.Byte(i + 1) == 0xC1 || pe.Byte(i + 1) == 0xC6))
                disp = pe.ReadU32(i + 2);
            if (disp is < 0x80 or > 0x400)
                continue;
            if (!found.Contains(disp))
                found.Add(disp);
            if (found.Count >= 2)
                break;
        }

        var id = found.Count > 0 ? found[0] : 0;
        var pwd = found.Count > 1 ? found[1] : 0;
        if (id != 0)
            evidence.Add($"SetLoginInfo m_stID +0x{id:X}" + (pwd != 0 ? $" m_stPassword +0x{pwd:X}." : "."));
        else
            evidence.Add("SetLoginInfo login-string displacements were not recovered.");
        return (id, pwd);
    }

    private static uint RecoverPhaseDisplacement(ActionPe pe, uint setLoginPhaseRva, List<string> evidence)
    {
        if (setLoginPhaseRva == 0)
            return 0;
        var loginVas = pe.FindAsciiZ("Login").ToHashSet();
        if (loginVas.Count == 0)
            return 0;
        var fnOff = pe.VaToFile(pe.ImageBase + setLoginPhaseRva);
        if (fnOff < 0)
            return 0;
        for (var i = fnOff; i < fnOff + 0x180 && i + 7 < pe.Length; i++)
        {
            if (!loginVas.Contains(pe.ReadU32(i)))
                continue;
            for (var j = i; j > i - 24 && j > fnOff; j--)
            {
                if (pe.Byte(j) == 0x8D && (pe.Byte(j + 1) == 0x81 || pe.Byte(j + 1) == 0x86 ||
                                           pe.Byte(j + 1) == 0x87 || pe.Byte(j + 1) == 0x8E ||
                                           pe.Byte(j + 1) == 0x8F))
                {
                    var disp = pe.ReadU32(j + 2);
                    if (disp is > 0x40 and < 0x2000)
                    {
                        evidence.Add($"m_strPhase displacement 0x{disp:X} from lea in SetLoginPhase.");
                        return disp;
                    }
                }

                if (pe.Byte(j) == 0x8D && pe.Byte(j + 1) == 0x8D)
                {
                    var disp = pe.ReadU32(j + 2);
                    if (disp is > 0x40 and < 0x2000)
                    {
                        evidence.Add($"m_strPhase displacement 0x{disp:X} from [ebp+disp] in SetLoginPhase.");
                        return disp;
                    }
                }
            }
        }

        evidence.Add("m_strPhase displacement was not recovered from SetLoginPhase.");
        return 0;
    }

    private static uint RecoverEcxLoad(ActionPe pe, uint rva)
    {
        if (rva == 0)
            return 0;
        rva = SkipInt3(pe, rva);
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        if (pe.Byte(off) == 0xE9)
        {
            var dest = (uint)(pe.FileToVa(off + 5) + pe.ReadI32(off + 1));
            if (!pe.IsCode(dest))
                return 0;
            rva = SkipInt3(pe, dest - pe.ImageBase);
            off = pe.VaToFile(pe.ImageBase + rva);
            if (off < 0)
                return 0;
        }

        for (var i = off; i < off + 0x20 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x41)
                return pe.Byte(i + 2);
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x81)
                return pe.ReadU32(i + 2);
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01)
                return 0;
        }

        return 0;
    }

    private static uint SkipInt3(ActionPe pe, uint rva)
    {
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return rva;
        while (pe.Byte(off) == 0xCC)
        {
            off++;
            rva++;
        }

        return rva;
    }

    private static (int level, int exp, int next, int hp, int maxHp, int sp, int maxSp) RecoverPointIds(ActionPe pe, List<string> evidence)
    {
        var next = ReadModuleInt(pe, "NEXT_EXP");
        var maxHp = ReadModuleInt(pe, "MAX_HP");
        var maxSp = ReadModuleInt(pe, "MAX_SP");
        if (next is > 0 and < 64 && maxHp is > 0 and < 64 && maxSp is > 0 and < 64)
        {
            var hp = maxHp.Value - 1;
            var exp = next.Value - 1;
            var level = next.Value - 3;
            if (level < 1)
                level = 1;
            evidence.Add($"Python player.GetStatus ids: LEVEL={level} EXP={exp} NEXT_EXP={next} HP={hp} MAX_HP={maxHp} SP={maxSp.Value - 1} MAX_SP={maxSp}.");
            return (level, exp, next.Value, hp, maxHp.Value, maxSp.Value - 1, maxSp.Value);
        }

        evidence.Add("Python POINT_* module constants were not recovered; using official LEVEL=1 EXP=3 HP=5 SP=7.");
        return (1, 3, 4, 5, 6, 7, 8);
    }

    private static int? ReadModuleInt(ActionPe pe, string name)
    {
        foreach (var va in pe.FindAsciiZ(name))
        {
            foreach (var xref in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(xref);
                if (off < 0)
                    continue;
                for (var k = 1; k <= 24 && off - k >= 1; k++)
                {
                    if (pe.Byte(off - k) == 0x6A)
                        return pe.Byte(off - k + 1);
                    if (pe.Byte(off - k) == 0x68)
                    {
                        var imm = pe.ReadU32(off - k + 1);
                        if (imm is > 0 and < 64)
                            return (int)imm;
                    }
                }
            }
        }

        return null;
    }

    private static (uint points, uint name, uint xor) RecoverPlayerHud(
        ActionPe pe, uint singletonRva, uint getStatusNative, List<string> evidence)
    {
        const uint knownMagic = 0xE73AC1DA;
        uint xor = 0;
        uint pointRel = 0;
        uint getPointVa = 0;
        foreach (var site in pe.FindImmediateXrefs(knownMagic))
        {
            var off = pe.VaToFile(site);
            if (off < 0)
                continue;
            var window = Math.Max(0, off - 24);
            for (var i = window; i < off + 8 && i + 7 < pe.Length; i++)
            {
                if (!TryScaledIndex(pe, i, out var disp))
                    continue;
                if (pe.Byte(i) == 0x89)
                    continue;
                pointRel = disp;
                xor = knownMagic;
                getPointVa = pe.FileToVa(FunctionStart(pe, i));
                evidence.Add($"GetPoint/GetStatus xor 0x{knownMagic:X8} loads [ecx+index*4+0x{disp:X}] at VA 0x{pe.FileToVa(i):X}.");
                break;
            }

            if (pointRel != 0)
                break;
        }

        if (pointRel == 0)
            TryFindGetPointXor(pe, getStatusNative, evidence, ref pointRel, ref xor, ref getPointVa);
        if (pointRel == 0)
            ScanXorGetPoint(pe, evidence, ref pointRel, ref xor, ref getPointVa);

        var points = pointRel;
        uint getStatusVa = 0;
        if (getPointVa != 0)
        {
            var status = FindGetStatusThisAdjustment(pe, getPointVa, evidence);
            if (status.disp != 0)
            {
                // IAbstractPlayer is at CPythonPlayer+4; the virtual GetStatus this is already +4.
                const uint abstractThis = 4;
                points = abstractThis + status.disp + pointRel;
                getStatusVa = status.functionVa;
                evidence.Add($"CPythonPlayer::GetStatus (VA 0x{getStatusVa:X}) adjusts IAbstractPlayer this by +0x{status.disp:X}; m_alPoint is CPythonPlayer+0x{points:X}.");
            }
            else
                evidence.Add($"GetStatus this-adjust was not found; using GetPoint displacement 0x{pointRel:X} as CPythonPlayer-relative.");
        }

        var name = RecoverPlayerNameDisplacement(pe, singletonRva, getPointVa, getStatusVa, evidence);
        if (points != 0)
            evidence.Add($"Player HUD RPM: name @ +0x{name:X}, m_alPoint @ +0x{points:X}, xor 0x{xor:X8}.");
        else
            evidence.Add("Player HUD status-point displacement was not recovered.");
        return (points, name, points == 0 ? 0 : xor);
    }

    private static void ScanXorGetPoint(
        ActionPe pe, List<string> evidence, ref uint pointRel, ref uint xor, ref uint getPointVa)
    {
        uint bestDisp = 0, bestXor = 0, bestVa = 0;
        var bestSpan = int.MaxValue;
        for (var i = 0; i + 12 < pe.Length; i++)
        {
            if (!TryScaledIndex(pe, i, out var disp) || pe.Byte(i) == 0x89)
                continue;
            if (disp is < 0x80 or > 0x40000)
                continue;
            var after = i + (pe.Byte(i + 1) == 0x84 ? 7 : 4);
            uint foundXor;
            if (pe.Byte(after) == 0x35)
                foundXor = pe.ReadU32(after + 1);
            else if (pe.Byte(after) == 0x81 && pe.Byte(after + 1) == 0xF0)
                foundXor = pe.ReadU32(after + 2);
            else
                continue;
            if (foundXor is 0 or 0xCCCCCCCC)
                continue;
            var retAt = after + (pe.Byte(after) == 0x35 ? 5 : 6);
            var hasRet = false;
            for (var r = retAt; r < retAt + 16 && r + 2 < pe.Length; r++)
            {
                if (pe.Byte(r) == 0xC3 || (pe.Byte(r) == 0xC2 && pe.Byte(r + 1) == 0x04))
                {
                    hasRet = true;
                    break;
                }
            }
            if (!hasRet)
                continue;
            var fnOff = FunctionStart(pe, i);
            var span = i - fnOff;
            if (span is < 0 or > 0x30)
                continue;
            var fnVa = pe.FileToVa(fnOff);
            if (!pe.IsCode(fnVa) || span >= bestSpan)
                continue;
            bestSpan = span;
            bestDisp = disp;
            bestXor = foundXor;
            bestVa = fnVa;
        }

        if (bestDisp == 0)
            return;
        pointRel = bestDisp;
        xor = bestXor;
        getPointVa = bestVa;
        evidence.Add($"GetPoint/GetStatus xor 0x{xor:X8} loads [ecx+index*4+0x{pointRel:X}] at VA 0x{getPointVa:X} (scanned).");
    }

    private static (uint item, uint getClose) RecoverItemFromPickClose(
        ActionPe pe, uint pickNative, uint playerSingletonRva, List<string> evidence)
    {
        if (pickNative == 0)
            return (0, 0);
        var off = pe.VaToFile(pe.ImageBase + pickNative);
        if (off < 0)
            return (0, 0);
        var playerVa = playerSingletonRva == 0 ? 0 : pe.ImageBase + playerSingletonRva;
        uint item = 0;
        uint getClose = 0;
        for (var i = off; i < off + 0x160 && i + 6 < pe.Length; i++)
        {
            uint addr = 0;
            if (pe.Byte(i) == 0xA1)
                addr = pe.ReadU32(i + 1);
            else if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 ||
                                           pe.Byte(i + 1) == 0x35 || pe.Byte(i + 1) == 0x3D))
                addr = pe.ReadU32(i + 2);
            if (addr != 0 && pe.IsVa(addr) && addr != playerVa)
            {
                var rva = addr - pe.ImageBase;
                if (item == 0 && pe.IsRva(rva))
                    item = rva;
            }

            if (pe.Byte(i) != 0xE8 || item == 0 || getClose != 0)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || !pe.IsCode(target) || LooksLikePythonStub(pe, destOff))
                continue;
            getClose = target - pe.ImageBase;
        }

        if (item != 0)
            evidence.Add($"CPythonItem singleton RVA 0x{item:X} GetCloseItem RVA 0x{getClose:X} from PickCloseItem.");
        else
            evidence.Add("CPythonItem singleton was not recovered from PickCloseItem.");
        return (item, getClose);
    }

    private static void TryFindGetPointXor(
        ActionPe pe, uint getStatusNative, List<string> evidence,
        ref uint pointRel, ref uint xor, ref uint getPointVa)
    {
        if (getStatusNative == 0)
            return;
        var off = pe.VaToFile(pe.ImageBase + getStatusNative);
        if (off < 0)
            return;
        for (var i = off; i < off + 0x80 && i + 12 < pe.Length; i++)
        {
            if (!TryScaledIndex(pe, i, out var disp) || pe.Byte(i) == 0x89)
                continue;
            var after = i + ((pe.Byte(i + 1) == 0x84) ? 7 : 4);
            if (pe.Byte(after) != 0x35)
                continue;
            var foundXor = pe.ReadU32(after + 1);
            if (foundXor == 0 || disp is < 0x20 or > 0x40000)
                continue;
            pointRel = disp;
            xor = foundXor;
            getPointVa = pe.FileToVa(FunctionStart(pe, i));
            evidence.Add($"GetPoint/GetStatus xor 0x{xor:X8} loads [ecx+index*4+0x{disp:X}] from GetStatus native.");
            return;
        }
    }

    private static bool TryScaledIndex(ActionPe pe, int off, out uint disp)
    {
        disp = 0;
        // mov r32, [ecx+r32*4+disp32]  8B / 89  84  81/8E/8A/8B/88/89
        if ((pe.Byte(off) == 0x8B || pe.Byte(off) == 0x89) && pe.Byte(off + 1) == 0x84 &&
            (pe.Byte(off + 2) & 0xC7) == 0x81)
        {
            disp = pe.ReadU32(off + 3);
            return disp is > 0 and < 0x400000;
        }

        // mov r32, [ecx+r32*4+disp8]  8B / 89  44  81
        if ((pe.Byte(off) == 0x8B || pe.Byte(off) == 0x89) && pe.Byte(off + 1) == 0x44 &&
            (pe.Byte(off + 2) & 0xC7) == 0x81)
        {
            disp = pe.Byte(off + 3);
            return disp != 0;
        }

        return false;
    }

    private static int FunctionStart(ActionPe pe, int off)
    {
        var start = Math.Max(0, off - 0x80);
        for (var i = off; i >= start; i--)
        {
            if (pe.Byte(i) == 0x55 && pe.Byte(i + 1) == 0x8B && pe.Byte(i + 2) == 0xEC)
                return i;
        }

        for (var i = off; i > start; i--)
        {
            if (pe.Byte(i) == 0xCC && pe.Byte(i + 1) != 0xCC)
                return i + 1;
            if (pe.Byte(i) == 0xC3 || (pe.Byte(i) == 0xC2 && pe.Byte(i + 2) == 0x00))
                return i + (pe.Byte(i) == 0xC2 ? 3 : 1);
        }

        return off;
    }

    private static (uint disp, uint functionVa) FindGetStatusThisAdjustment(ActionPe pe, uint getPointVa, List<string> evidence)
    {
        var getPointFile = pe.VaToFile(getPointVa);
        if (getPointFile < 0)
            return (0, 0);
        foreach (var call in FindRelativeTransfersTo(pe, getPointVa))
        {
            var off = pe.VaToFile(call);
            if (off < 0)
                continue;
            var start = Math.Max(0, off - 0x30);
            for (var i = start; i < off && i + 6 < pe.Length; i++)
            {
                uint disp = 0;
                if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x81)
                    disp = pe.ReadU32(i + 2);
                else if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x49)
                    disp = pe.Byte(i + 2);
                else if (pe.Byte(i) == 0x81 && pe.Byte(i + 1) == 0xC1)
                    disp = pe.ReadU32(i + 2);
                else if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
                    disp = pe.Byte(i + 2);
                if (disp == 0)
                    continue;
                var functionVa = pe.FileToVa(FunctionStart(pe, i));
                evidence.Add($"GetStatus +0x{disp:X} before GetPoint at VA 0x{call:X} (fn 0x{functionVa:X}).");
                return (disp, functionVa);
            }
        }

        return (0, 0);
    }

    private static List<uint> FindRelativeTransfersTo(ActionPe pe, uint targetVa)
    {
        var hits = new List<uint>();
        // Walk a coarse window around GetPoint; callers are nearby in the same TU.
        var center = pe.VaToFile(targetVa);
        if (center < 0)
            return hits;
        var start = Math.Max(0, center - 0x8000);
        var end = Math.Min(pe.Length - 5, center + 0x8000);
        for (var i = start; i < end; i++)
        {
            if (pe.Byte(i) != 0xE8 && pe.Byte(i) != 0xE9)
                continue;
            var dest = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (dest == targetVa)
                hits.Add(pe.FileToVa(i));
        }

        if (hits.Count == 0)
        {
            // Broader pass from image mapping via xref of the relative encoding is expensive;
            // try a second window covering typical CPythonPlayer.cpp span.
            start = Math.Max(0, center - 0x40000);
            end = Math.Min(pe.Length - 5, center + 0x40000);
            for (var i = start; i < end; i++)
            {
                if (pe.Byte(i) != 0xE8 && pe.Byte(i) != 0xE9)
                    continue;
                var dest = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                if (dest == targetVa)
                    hits.Add(pe.FileToVa(i));
            }
        }

        return hits;
    }

    private static uint RecoverPlayerNameDisplacement(ActionPe pe, uint singletonRva, uint getPointVa, uint getStatusVa, List<string> evidence)
    {
        var name = RecoverNamedNativePrefer(pe, "GetName", singletonRva, evidence);
        var wrapper = name.wrapper;
        if (wrapper != 0)
        {
            var virt = RecoverVirtualDispatch(pe, wrapper);
            if (virt.slot != 0)
                evidence.Add($"player.GetName is virtual: ecx=CPythonPlayer*+0x{virt.thisAdj:X}, call [vtable+0x{virt.slot:X}].");
            var fromNative = StdStringThis(pe, name.native);
            if (fromNative != 0)
            {
                evidence.Add($"CPythonPlayer::GetName m_stName at +0x{fromNative:X} from native RVA 0x{name.native:X}.");
                return fromNative;
            }
        }

        var fromVtable = NameFromAbstractVtable(pe, getPointVa, getStatusVa, evidence);
        if (fromVtable != 0)
            return fromVtable;

        evidence.Add("CPythonPlayer::GetName string displacement was not recovered.");
        return 0;
    }

    private static uint CallAfterMovEcxEax(ActionPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0xA0 && i + 7 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x8B || pe.Byte(i + 1) != 0xC8 || pe.Byte(i + 2) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 7) + pe.ReadI32(i + 3));
            if (pe.IsCode(target))
                return target - pe.ImageBase;
        }

        return 0;
    }

    private static uint FirstE8AfterEcxLoad(ActionPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0 || off + 11 >= pe.Length)
            return 0;
        if (pe.Byte(off) != 0x8B || pe.Byte(off + 1) != 0x0D || pe.Byte(off + 6) != 0xE8)
            return 0;
        var target = (uint)(pe.FileToVa(off + 11) + pe.ReadI32(off + 7));
        if (!pe.IsCode(target))
            return 0;
        var rva = target - pe.ImageBase;
        return rva is 0x608290 or 0x608470 or 0x6081F0 ? 0 : rva;
    }

    private static uint FirstCallAllowShort(ActionPe pe, uint wrapperRva, uint singletonVa)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        var start = off;
        if (singletonVa != 0)
        {
            for (var i = off; i < off + 0x40 && i + 6 < pe.Length; i++)
            {
                if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x0D && pe.ReadU32(i + 2) == singletonVa)
                {
                    start = i + 6;
                    break;
                }
            }
        }

        for (var i = start; i < off + 0x40 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (!pe.IsCode(target))
                continue;
            var rva = target - pe.ImageBase;
            if (rva is 0x608290 or 0x608470 or 0x6081F0)
                continue;
            return rva;
        }

        return 0;
    }

    private static uint CallOnLocalEcx(ActionPe pe, uint wrapperRva, List<string> evidence, string name)
    {
        if (wrapperRva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return 0;
        for (var i = off; i + 8 < off + 0xC0 && i + 8 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x8B || pe.Byte(i + 1) != 0x4D || pe.Byte(i + 3) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 8) + pe.ReadI32(i + 4));
            if (!pe.IsCode(target))
                continue;
            var rva = target - pe.ImageBase;
            if (rva is 0x608290 or 0x608470 or 0x6081F0)
                continue;
            evidence.Add($"Skill flag {name} thiscall RVA 0x{rva:X}.");
            return rva;
        }

        return 0;
    }

    private static uint RecoverUseSkillFromClick(ActionPe pe, uint clickRva, uint netSingletonRva, List<string> evidence)
    {
        if (clickRva == 0)
            return 0;
        var netVa = netSingletonRva == 0 ? 0 : pe.ImageBase + netSingletonRva;
        var seen = new HashSet<uint>();
        var queue = new Queue<(uint rva, int depth)>();
        queue.Enqueue((clickRva, 0));
        uint nearby = 0;
        while (queue.Count > 0)
        {
            var (rva, depth) = queue.Dequeue();
            if (!seen.Add(rva) || depth > 2)
                continue;
            var off = pe.VaToFile(pe.ImageBase + rva);
            if (off < 0)
                continue;
            var end = Math.Min(off + 0x500, pe.Length);
            var loadsNet = false;
            var calls = new List<uint>();
            for (var i = off; i + 5 < end; i++)
            {
                if (pe.Byte(i) == 0xC3)
                    break;
                if (pe.Byte(i) == 0xC2)
                    break;
                if (netVa != 0 &&
                    ((pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == netVa) ||
                     (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x0D && pe.ReadU32(i + 2) == netVa)))
                    loadsNet = true;
                if (pe.Byte(i) != 0xE8)
                    continue;
                var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
                if (!pe.IsCode(target))
                    continue;
                var trva = target - pe.ImageBase;
                if (trva is 0x608290 or 0x608470 or 0x6081F0)
                    continue;
                calls.Add(trva);
            }

            if (loadsNet)
            {
                foreach (var trva in calls)
                {
                    if (trva is > 0x360000 and < 0x37A000)
                    {
                        evidence.Add($"SendUseSkillPacket via ClickSkillSlot RVA 0x{trva:X} (net load in 0x{rva:X}).");
                        return trva;
                    }

                    if (nearby == 0)
                        nearby = trva;
                }
            }

            if (depth < 2)
            {
                foreach (var trva in calls)
                    queue.Enqueue((trva, depth + 1));
            }
        }

        if (nearby != 0)
            evidence.Add($"SendUseSkillPacket nearby fallback RVA 0x{nearby:X}.");
        else
            evidence.Add("SendUseSkillPacket was not recovered from ClickSkillSlot.");
        return nearby;
    }

    private static (uint native, uint singleton, uint wrapper) RecoverItemMethod(
        ActionPe pe, string name, uint preferSingletonRva, List<string> evidence)
    {
        var preferVa = preferSingletonRva == 0 ? 0 : pe.ImageBase + preferSingletonRva;
        (uint native, uint singleton, uint wrapper) fallback = (0, 0, 0);
        foreach (var va in pe.FindAsciiZ(name))
        {
            foreach (var site in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(site);
                if (off < 0 || !TryMethodTable(pe, off, out var fn))
                    continue;
                var wrapperRva = fn - pe.ImageBase;
                var singleton = FirstGlobalLoad(pe, wrapperRva);
                var native = NativeCallSkippingPython(pe, wrapperRva, singleton);
                var singletonRva = singleton == 0 ? 0 : singleton - pe.ImageBase;
                var candidate = (native, pe.IsRva(singletonRva) ? singletonRva : 0u, wrapperRva);
                var bytes = pe.Hex(pe.ImageBase + wrapperRva, 128);
                if (preferVa != 0 && singleton == preferVa)
                {
                    evidence.Add($"Python {name} wrapper RVA 0x{wrapperRva:X} nativeRVA=0x{native:X} bytes={bytes}.");
                    return candidate;
                }

                if (fallback.wrapper == 0)
                {
                    fallback = candidate;
                    evidence.Add($"Python {name} wrapper RVA 0x{wrapperRva:X} nativeRVA=0x{native:X} singleton=0x{singletonRva:X} bytes={bytes}.");
                }
            }
        }

        if (fallback.wrapper == 0)
            evidence.Add($"Python method '{name}' was not recovered.");
        return fallback;
    }

    private static (uint native, uint singleton, uint wrapper) RecoverNamedNativePrefer(
        ActionPe pe, string name, uint preferSingletonRva, List<string> evidence)
    {
        var preferVa = preferSingletonRva == 0 ? 0 : pe.ImageBase + preferSingletonRva;
        (uint native, uint singleton, uint wrapper) fallback = (0, 0, 0);
        var foundString = false;
        foreach (var va in pe.FindAsciiZ(name))
        {
            foundString = true;
            foreach (var site in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(site);
                if (off < 0 || !TryMethodTable(pe, off, out var fn))
                    continue;
                var wrapperRva = fn - pe.ImageBase;
                var singleton = FirstGlobalLoad(pe, wrapperRva);
                var native = NativeCallAfterSingleton(pe, wrapperRva, singleton);
                var singletonRva = singleton == 0 ? 0 : singleton - pe.ImageBase;
                var candidate = (native, pe.IsRva(singletonRva) ? singletonRva : 0u, wrapperRva);
                if (preferVa != 0 && singleton == preferVa)
                {
                    evidence.Add($"Python {name} (player singleton) wrapper RVA 0x{wrapperRva:X} nativeRVA=0x{native:X}.");
                    return candidate;
                }

                if (fallback.wrapper == 0)
                    fallback = candidate;
            }
        }

        if (fallback.wrapper != 0)
            evidence.Add($"Python {name} wrapper RVA 0x{fallback.wrapper:X} nativeRVA=0x{fallback.native:X} (no singleton filter match).");
        else
            evidence.Add(foundString
                ? $"Python method '{name}' string exists but no METH_VARARGS table entry was accepted."
                : $"'{name}' string missing.");
        return fallback;
    }

    private static uint NameFromAbstractVtable(ActionPe pe, uint getPointVa, uint getStatusVa, List<string> evidence)
    {
        var statusCandidates = new HashSet<uint>();
        if (getStatusVa != 0)
            statusCandidates.Add(getStatusVa);
        if (getPointVa != 0)
        {
            statusCandidates.Add(getPointVa);
            foreach (var call in FindRelativeTransfersTo(pe, getPointVa))
            {
                var start = pe.FileToVa(FunctionStart(pe, pe.VaToFile(call)));
                if (pe.IsCode(start))
                    statusCandidates.Add(start);
            }
        }

        foreach (var statusVa in statusCandidates)
        {
            if (statusVa != 0)
                evidence.Add($"GetStatus candidate VA 0x{statusVa:X} bytes={pe.Hex(statusVa, 24)} xrefs={pe.FindImmediateXrefs(statusVa).Count}.");
            foreach (var target in EnumerateVtableEntries(pe, statusVa))
            {
                var nameVa = target.nameVa;
                if (!pe.IsCode(nameVa))
                    continue;
                var disp = StdStringThis(pe, nameVa - pe.ImageBase);
                if (disp == 0)
                {
                    evidence.Add($"IAbstractPlayer GetName VA 0x{nameVa:X} bytes={pe.Hex(nameVa, 32)} had no std::string this.");
                    continue;
                }

                // Virtual GetName this is CPythonPlayer+4.
                var absolute = disp + 4;
                evidence.Add($"IAbstractPlayer vtable GetStatus VA 0x{target.statusVa:X} GetName VA 0x{nameVa:X} m_stName CPythonPlayer+0x{absolute:X} (IAbstract+0x{disp:X}).");
                return absolute;
            }
        }

        return 0;
    }

    private static IEnumerable<(uint statusVa, uint nameVa)> EnumerateVtableEntries(ActionPe pe, uint statusVa)
    {
        if (statusVa == 0)
            yield break;
        var keys = new HashSet<uint> { statusVa };
        foreach (var site in FindRelativeTransfersTo(pe, statusVa))
        {
            var off = pe.VaToFile(site);
            if (off >= 0 && pe.Byte(off) == 0xE9)
                keys.Add(site);
        }

        foreach (var key in keys)
        {
            foreach (var xref in pe.FindImmediateXrefs(key))
            {
                var off = pe.VaToFile(xref);
                if (off < 0)
                    continue;
                yield return (key, pe.ReadU32(off + 4));
            }
        }
    }

    private static uint StdStringThis(ActionPe pe, uint rva)
    {
        if (rva == 0)
            return 0;
        rva = SkipInt3(pe, rva);
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        if (pe.Byte(off) == 0xE9)
        {
            var dest = (uint)(pe.FileToVa(off + 5) + pe.ReadI32(off + 1));
            if (pe.IsCode(dest))
            {
                rva = SkipInt3(pe, dest - pe.ImageBase);
                off = pe.VaToFile(pe.ImageBase + rva);
                if (off < 0)
                    return 0;
            }
        }

        for (var i = off; i < off + 0x40 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8D && (pe.Byte(i + 1) == 0x81 || pe.Byte(i + 1) == 0x89))
            {
                var disp = pe.ReadU32(i + 2);
                if (disp is > 8 and < 0x400000)
                    return disp;
            }

            if (pe.Byte(i) == 0x8D && (pe.Byte(i + 1) == 0x41 || pe.Byte(i + 1) == 0x49))
            {
                var disp = pe.Byte(i + 2);
                if (disp is > 8)
                    return disp;
            }

            if (pe.Byte(i) == 0x81 && pe.Byte(i + 1) == 0xC1)
            {
                var disp = pe.ReadU32(i + 2);
                if (disp is > 8 and < 0x400000)
                    return disp;
            }

            // cmp dword [ecx+disp], 16  — inlined MSVC std::string::c_str capacity test
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xB9 && pe.Byte(i + 6) == 0x10)
            {
                var capDisp = pe.ReadU32(i + 2);
                if (capDisp >= 16)
                    return capDisp - 16;
            }
        }

        return 0;
    }

    private sealed class ActionPe
    {
        private readonly byte[] _bytes;
        private readonly List<(uint Va, uint Raw, uint RawSize)> _sections = new();
        public uint ImageBase { get; }
        public uint SizeOfImage { get; }
        public int Length => _bytes.Length;

        public static ActionPe Load(string path) => new(path);

        private ActionPe(string path)
        {
            _bytes = File.ReadAllBytes(path);
            var eLfanew = BitConverter.ToInt32(_bytes, 0x3C);
            ImageBase = BitConverter.ToUInt32(_bytes, eLfanew + 24 + 28);
            SizeOfImage = BitConverter.ToUInt32(_bytes, eLfanew + 24 + 56);
            var sections = BitConverter.ToUInt16(_bytes, eLfanew + 6);
            var optional = BitConverter.ToUInt16(_bytes, eLfanew + 20);
            var off = eLfanew + 24 + optional;
            for (var i = 0; i < sections; i++)
            {
                var at = off + i * 40;
                _sections.Add((
                    BitConverter.ToUInt32(_bytes, at + 12),
                    BitConverter.ToUInt32(_bytes, at + 20),
                    BitConverter.ToUInt32(_bytes, at + 16)));
            }
        }

        public bool IsVa(uint va) => va >= ImageBase + 0x1000 && va < ImageBase + SizeOfImage;
        public bool IsRva(uint rva) => rva >= 0x1000 && rva < SizeOfImage;
        public bool IsCode(uint va) => IsVa(va) && VaToFile(va) >= 0;
        public bool IsData(uint va) => IsVa(va);

        public string Hex(uint va, int count)
        {
            var off = VaToFile(va);
            if (off < 0)
                return "";
            var n = Math.Min(count, Length - off);
            return Convert.ToHexString(_bytes, off, n);
        }

        public bool TryFindAscii(string text, out uint va)
        {
            foreach (var found in FindAsciiZ(text))
            {
                va = found;
                return true;
            }

            va = 0;
            return false;
        }

        public IEnumerable<uint> FindAsciiZ(string text)
        {
            var needle = Encoding.ASCII.GetBytes(text + "\0");
            var start = 0;
            while (start + needle.Length <= _bytes.Length)
            {
                var idx = _bytes.AsSpan(start).IndexOf(needle);
                if (idx < 0)
                    yield break;
                idx += start;
                if (idx == 0 || _bytes[idx - 1] < 0x20 || _bytes[idx - 1] > 0x7E)
                {
                    var va = FileToVa(idx);
                    if (va != 0)
                        yield return va;
                }

                start = idx + 1;
            }
        }

        public List<uint> FindImmediateXrefs(uint value)
        {
            var needle = BitConverter.GetBytes(value);
            var hits = new List<uint>();
            foreach (var section in _sections)
            {
                var start = (int)section.Raw;
                var end = Math.Min(_bytes.Length - 4, start + (int)section.RawSize);
                for (var i = start; i < end; i++)
                {
                    if (_bytes[i] == needle[0] && _bytes[i + 1] == needle[1] &&
                        _bytes[i + 2] == needle[2] && _bytes[i + 3] == needle[3])
                        hits.Add(FileToVa(i));
                }
            }

            return hits;
        }

        public List<uint> FindRelativeCallsTo(uint targetVa)
        {
            var hits = new List<uint>();
            foreach (var section in _sections)
            {
                var start = (int)section.Raw;
                var end = Math.Min(_bytes.Length - 5, start + (int)section.RawSize);
                for (var i = start; i < end; i++)
                {
                    if (_bytes[i] != 0xE8)
                        continue;
                    var dest = (uint)(FileToVa(i + 5) + ReadI32(i + 1));
                    if (dest == targetVa)
                        hits.Add(FileToVa(i));
                }
            }

            return hits;
        }

        public string CString(uint va, int max = 64)
        {
            var off = VaToFile(va);
            if (off < 0)
                return "";
            var end = off;
            while (end < Length && end - off < max && _bytes[end] != 0)
                end++;
            return end > off ? Encoding.ASCII.GetString(_bytes, off, end - off) : "";
        }

        public byte Byte(int file) => file >= 0 && file < _bytes.Length ? _bytes[file] : (byte)0;
        public uint ReadU32(int file) => file >= 0 && file + 4 <= _bytes.Length ? BitConverter.ToUInt32(_bytes, file) : 0;
        public int ReadI32(int file) => file >= 0 && file + 4 <= _bytes.Length ? BitConverter.ToInt32(_bytes, file) : 0;

        public uint FileToVa(int fileOffset)
        {
            foreach (var section in _sections)
            {
                if (fileOffset >= section.Raw && fileOffset < section.Raw + section.RawSize)
                    return ImageBase + section.Va + (uint)(fileOffset - section.Raw);
            }

            return 0;
        }

        public int VaToFile(uint va)
        {
            var rva = va - ImageBase;
            foreach (var section in _sections)
            {
                if (rva >= section.Va && rva < section.Va + Math.Max(section.RawSize, 1u))
                    return (int)(section.Raw + (rva - section.Va));
            }

            return -1;
        }
    }
}
