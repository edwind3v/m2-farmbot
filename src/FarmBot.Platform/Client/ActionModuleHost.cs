using System.Diagnostics;
using System.Text;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;

namespace FarmBot.Platform.Client;

public sealed record ClientProcessInfo(int ProcessId, string Name, string Path)
{
    public override string ToString() => $"{Name} PID {ProcessId}";
}

public sealed class ActionModuleHost
{
    public string Token { get; } = ActionProtocol.NewToken();
    public string PipeName { get; }
    public ClientProfile Profile { get; set; } = ClientProfile.Normal;
    public int LastProcessId { get; private set; }
    public string Status { get; private set; } = "companion not loaded";

    public ActionModuleHost(string? pipeName = null)
    {
        PipeName = string.IsNullOrWhiteSpace(pipeName)
            ? LoaderOutput.UniquePipeName()
            : pipeName.Trim();
    }

    public static IReadOnlyList<ClientProcessInfo> ListClients(string? clientPath = null)
    {
        var list = new List<ClientProcessInfo>();
        foreach (var name in LiveGameStateProvider.ProcessNamesFor(clientPath))
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    var path = "";
                    try { path = process.MainModule?.FileName ?? ""; }
                    catch { /* High-integrity module path may be hidden from this viewer. */ }
                    list.Add(new ClientProcessInfo(process.Id, process.ProcessName, path));
                }
            }
        }

        return list;
    }

    public bool TryAttach(int processId, out string status)
    {
        LastProcessId = processId;
        var exePath = "";
        try
        {
            using var process = Process.GetProcessById(processId);
            exePath = process.MainModule?.FileName ?? "";
        }
        catch
        {
        }

        return RunLoader($"--attach {processId}", out status, 20000, exePath);
    }

    public bool TryLaunch(string exePath, out string status) =>
        TryLaunch(exePath, out status, out _);

    public bool TryLaunch(string exePath, out string status, out int processId)
    {
        processId = 0;
        LastProcessId = 0;
        if (!File.Exists(exePath))
        {
            status = Status = "client executable not found: " + exePath;
            return false;
        }

        var workdir = Path.GetDirectoryName(exePath) ?? "";
        if (!RunLoader($"--launch \"{exePath}\" --workdir \"{workdir}\"", out status, 45000, exePath))
            return false;
        if (!LoaderOutput.TryParseLaunchedPid(status, out processId))
        {
            status = Status = "client launched but FarmBotLoad did not report a PID: " + status;
            return false;
        }

        LastProcessId = processId;
        return true;
    }

    public bool WaitUntilListening(AdapterActionSink sink, int timeoutMs = 15000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        using var ready = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\FarmBotReady_" + PipeName);
        while (DateTime.UtcNow < deadline)
        {
            if (ready.WaitOne(50) && sink.TryConnect(400))
            {
                Status = sink.Status;
                return true;
            }
        }

        Status = sink.Status;
        return sink.Available;
    }

    private bool RunLoader(string extraArgs, out string status, int waitMs = 20000, string? clientPath = null)
    {
        var load = Resolve("FarmBotLoad.exe");
        var dll = Resolve("FarmBotAction.dll");
        if (load.Length == 0 || dll.Length == 0)
        {
            BuildNative();
            load = Resolve("FarmBotLoad.exe");
            dll = Resolve("FarmBotAction.dll");
        }

        if (load.Length == 0 || dll.Length == 0)
        {
            status = Status = "FarmBotAction.dll / FarmBotLoad.exe are not built";
            return false;
        }

        var psi = new ProcessStartInfo
        {
            FileName = load,
            Arguments = LoaderArgs(extraArgs, dll, clientPath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null)
        {
            status = Status = "FarmBotLoad failed to start";
            return false;
        }

        if (!process.WaitForExit(waitMs))
        {
            try { process.Kill(true); } catch { /* ignore */ }
            status = Status = "FarmBotLoad timed out";
            return false;
        }

        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
        {
            status = Status = "FarmBotLoad failed: " + output.Trim();
            return false;
        }

        status = Status = output.Trim();
        return true;
    }

    private string LoaderArgs(string extraArgs, string dll, string? clientPath)
    {
        var args = $"{extraArgs} --dll \"{dll}\" --token {Token} --pipe {PipeName}";
        var layout = Profile == ClientProfile.Arthion
            ? WriteLayoutFile(clientPath)
            : Profile == ClientProfile.Baerim
                ? WriteBaerimLayoutFile(clientPath)
                : "";
        if (layout.Length > 0)
            args += " --layout \"" + layout + "\"";
        return args;
    }

    private string WriteLayoutFile(string? exePath)
    {
        if (Profile != ClientProfile.Arthion)
            return "";
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return "";
        try
        {
            var login = PlayerActionLayoutRecoverer.RecoverLogin(exePath);
            var player = PlayerActionLayoutRecoverer.Recover(exePath);
            var entity = ClientReleaseLayout.Recover(exePath);
            var text = new StringBuilder(login.ToNativeConfig());
            text.Append("profile=arthion\n");
            var playerRva = player.PlayerSingletonRva;
            AppendHex(text, "player", playerRva);
            AppendHex(text, "playerName", player.PlayerNameDisplacement);
            AppendHex(text, "charMgr", entity.SingletonRva);
            AppendHex(text, "thisAdj", player.SetTargetThisAdjustment);
            AppendHex(text, "setTarget", player.SetTargetRva);
            AppendHex(text, "setTargetSlot", player.SetTargetVtableSlot);
            AppendHex(text, "targetDisp", player.TargetVidDisplacement);
            AppendHex(text, "attackKey", player.SetAttackKeyStateRva);
            AppendHex(text, "newAttack", player.NewAttackRva);
            AppendHex(text, "pickClose", player.PickCloseItemRva);
            AppendHex(text, "getStatus", player.GetStatusRva);
            AppendHex(text, "getStatusSlot", player.GetStatusVtableSlot);
            AppendHex(text, "sendAttack", player.SendAttackPacketRva);
            AppendHex(text, "sendState", player.SendStatePacketRva);
            AppendHex(text, "sendPickup", player.SendPickupPacketRva);
            AppendHex(text, "sendUse", player.SendItemUsePacketRva);
            AppendHex(text, "getItemIndex", player.GetItemIndexRva);
            AppendHex(text, "getItemCount", player.GetItemCountRva);
            AppendHex(text, "selectItem", player.SelectItemRva);
            AppendHex(text, "itemMgr", player.ItemManagerRva);
            AppendHex(text, "getSelected", player.GetSelectedRva);
            AppendHex(text, "getItemName", player.GetItemNameRva);
            AppendHex(text, "getItemType", player.GetItemTypeRva);
            AppendHex(text, "getItemSubType", player.GetItemSubTypeRva);
            AppendHex(text, "getAffect", player.GetAffectRva);
            AppendHex(text, "getValue", player.GetValueRva);
            AppendHex(text, "getIconImage", player.GetIconImageRva);
            AppendHex(text, "packMgr", 0x3956A14u);
            AppendHex(text, "packGet", 0x5054A0u);
            AppendHex(text, "useItem", player.UseItemRva);
            AppendHex(text, "getSkillIndex", player.GetSkillIndexRva);
            AppendHex(text, "getSkillLevel", player.GetSkillLevelRva);
            AppendHex(text, "clickSkill", player.ClickSkillSlotRva);
            AppendHex(text, "getSkillCool", player.GetSkillCoolTimeRva);
            AppendHex(text, "isSkillCool", player.IsSkillCoolTimeRva);
            AppendHex(text, "isSkillActive", player.IsSkillActiveRva);
            AppendHex(text, "sendUseSkill", player.SendUseSkillPacketRva);
            AppendHex(text, "skillMgr", player.SkillManagerRva);
            AppendHex(text, "getSkillData", player.GetSkillDataRva);
            AppendHex(text, "isStanding", player.IsStandingSkillRva);
            AppendHex(text, "isToggle", player.IsToggleSkillRva);
            AppendHex(text, "isNeedTarget", player.IsNeedTargetRva);
            AppendHex(text, "isAttackSkill", player.IsAttackSkillRva);
            AppendHex(text, "isMounting", player.IsMountingHorseRva);
            AppendHex(text, "isAffect", player.IsAffectRva);
            AppendHex(text, "sendChat", player.SendChatPacketRva);
            AppendHex(text, "combo", player.ComboAttackRva);
            AppendHex(text, "setAuto", player.SetAutoRva);
            AppendHex(text, "reserve", player.ReserveClickRva);
            AppendHex(text, "reservedMode", player.ReservedModeDisplacement);
            AppendHex(text, "vidReserved", player.VidReservedDisplacement);
            AppendHex(text, "moveTo", player.MoveToDestRva);
            AppendHex(text, "movePixel", player.MoveToPixelRva);
            AppendHex(text, "getMain", player.GetMainActorRva);
            AppendHex(text, "pressActor", player.OnPressActorRva);
            AppendHex(text, "chase", player.ChaseActorRva);
            AppendHex(text, "approachInst", player.ApproachInstRva);
            AppendHex(text, "playerUpdate", player.PlayerUpdateRva);
            AppendHex(text, "getPixel", player.GetPixelRva);
            AppendHex(text, "inRange", player.InRangeRva);
            AppendHex(text, "item", player.ItemSingletonRva);
            AppendHex(text, "getClose", player.GetCloseItemRva);
            AppendHex(text, "points", player.StatusPointsDisplacement);
            if (player.StatusPointsDisplacement != 0)
                text.Append("xor=").Append(player.PointXorMask.ToString("X")).Append('\n');
            AppendHex(text, "mainPtr", entity.MainPointerDisplacement);
            AppendHex(text, "graphic", entity.GraphicDisplacement);
            AppendHex(text, "pixelX", entity.PixelXDisplacement);
            AppendHex(text, "pixelY", entity.PixelYDisplacement);
            AppendHex(text, "mapDisp", entity.MapDisplacement);
            AppendHex(text, "mapHead", entity.MapHeadDisplacement);
            AppendHex(text, "nodeLeft", entity.NodeLeft);
            AppendHex(text, "nodeParent", entity.NodeParent);
            AppendHex(text, "nodeRight", entity.NodeRight);
            AppendHex(text, "nodeIsNil", entity.NodeIsNil);
            AppendHex(text, "nodeKey", entity.NodeKey);
            AppendHex(text, "nodeValue", entity.NodeValue);
            var path = Path.Combine(Path.GetTempPath(), "FarmBotAction_layout_" + Guid.NewGuid().ToString("N") + ".cfg");
            File.WriteAllText(path, text.ToString());
            return path;
        }
        catch
        {
            return "";
        }
    }

    private string WriteBaerimLayoutFile(string? exePath)
    {
        if (Profile != ClientProfile.Baerim)
            return "";
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return "";
        try
        {
            var recovered = BaerimActionLayoutRecoverer.Recover(exePath);
            var player = recovered.Player;
            var entity = recovered.Entity;
            var text = new StringBuilder(recovered.Login.ToNativeConfig());
            text.Append("profile=baerim\n");
            AppendHex(text, "player", player.PlayerSingletonRva);
            AppendHex(text, "playerName", player.PlayerNameDisplacement);
            AppendHex(text, "charMgr", entity.SingletonRva);
            AppendHex(text, "thisAdj", player.SetTargetThisAdjustment);
            AppendHex(text, "setTarget", player.SetTargetRva);
            AppendHex(text, "setTargetSlot", player.SetTargetVtableSlot);
            AppendHex(text, "targetDisp", player.TargetVidDisplacement);
            AppendHex(text, "attackKey", player.SetAttackKeyStateRva);
            AppendHex(text, "newAttack", player.NewAttackRva);
            AppendHex(text, "pickClose", player.PickCloseItemRva);
            AppendHex(text, "getStatus", player.GetStatusRva);
            AppendHex(text, "getStatusSlot", player.GetStatusVtableSlot);
            AppendHex(text, "getNameSlot", 0x14u);
            AppendHex(text, "sendAttack", player.SendAttackPacketRva);
            AppendHex(text, "sendState", player.SendStatePacketRva);
            AppendHex(text, "sendPickup", player.SendPickupPacketRva);
            AppendHex(text, "sendUse", player.SendItemUsePacketRva);
            if (recovered.GetItemIndexVtableSlot == 0)
                AppendHex(text, "getItemIndex", player.GetItemIndexRva);
            AppendHex(text, "getItemIndexSlot", recovered.GetItemIndexVtableSlot);
            if (recovered.GetItemCountVtableSlot == 0)
                AppendHex(text, "getItemCount", player.GetItemCountRva);
            AppendHex(text, "getItemCountSlot", recovered.GetItemCountVtableSlot);
            AppendHex(text, "selectItem", player.SelectItemRva);
            AppendHex(text, "itemMgr", player.ItemManagerRva);
            AppendHex(text, "getSelected", player.GetSelectedRva);
            AppendHex(text, "getItemName", player.GetItemNameRva);
            AppendHex(text, "getItemType", player.GetItemTypeRva);
            AppendHex(text, "getItemSubType", player.GetItemSubTypeRva);
            AppendHex(text, "getAffect", player.GetAffectRva);
            AppendHex(text, "getValue", player.GetValueRva);
            AppendHex(text, "getIconImage", player.GetIconImageRva);
            AppendHex(text, "packMgr", recovered.PackMgrRva);
            AppendHex(text, "packGet", recovered.PackGetRva);
            AppendHex(text, "packCtor", recovered.PackMappedCtorRva);
            AppendHex(text, "packExist", recovered.PackExistRva);
            AppendHex(text, "useItem", player.UseItemRva);
            AppendHex(text, "getSkillIndex", player.GetSkillIndexRva);
            AppendHex(text, "getSkillLevel", player.GetSkillLevelRva);
            AppendHex(text, "clickSkill", player.ClickSkillSlotRva);
            AppendHex(text, "getSkillCool", player.GetSkillCoolTimeRva);
            AppendHex(text, "isSkillCool", recovered.IsSkillCoolTimeRva);
            AppendHex(text, "isSkillActive", player.IsSkillActiveRva);
            AppendHex(text, "sendUseSkill", player.SendUseSkillPacketRva);
            AppendHex(text, "skillMgr", player.SkillManagerRva);
            AppendHex(text, "getSkillData", player.GetSkillDataRva);
            AppendHex(text, "isStanding", player.IsStandingSkillRva);
            AppendHex(text, "isToggle", player.IsToggleSkillRva);
            AppendHex(text, "isNeedTarget", player.IsNeedTargetRva);
            AppendHex(text, "isAttackSkill", player.IsAttackSkillRva);
            AppendHex(text, "isMounting", player.IsMountingHorseRva);
            AppendHex(text, "isAffect", player.IsAffectRva);
            AppendHex(text, "sendChat", player.SendChatPacketRva);
            AppendHex(text, "combo", player.ComboAttackRva);
            AppendHex(text, "setAuto", player.SetAutoRva);
            AppendHex(text, "reserve", player.ReserveClickRva);
            AppendHex(text, "reservedMode", player.ReservedModeDisplacement);
            AppendHex(text, "vidReserved", player.VidReservedDisplacement);
            AppendHex(text, "moveTo", player.MoveToDestRva);
            AppendHex(text, "movePixel", player.MoveToPixelRva);
            AppendHex(text, "getMain", player.GetMainActorRva);
            AppendHex(text, "pressActor", player.OnPressActorRva);
            AppendHex(text, "chase", player.ChaseActorRva);
            AppendHex(text, "approachInst", player.ApproachInstRva);
            AppendHex(text, "playerUpdate", player.PlayerUpdateRva);
            AppendHex(text, "getPixel", player.GetPixelRva);
            AppendHex(text, "inRange", player.InRangeRva);
            AppendHex(text, "item", player.ItemSingletonRva);
            AppendHex(text, "getClose", player.GetCloseItemRva);
            AppendHex(text, "points", player.StatusPointsDisplacement);
            if (player.StatusPointsDisplacement != 0)
                text.Append("xor=").Append(player.PointXorMask.ToString("X")).Append('\n');
            AppendHex(text, "mainPtr", entity.MainPointerDisplacement);
            AppendHex(text, "graphic", entity.GraphicDisplacement);
            AppendHex(text, "pixelX", entity.PixelXDisplacement);
            AppendHex(text, "pixelY", entity.PixelYDisplacement);
            AppendHex(text, "pixelZ", entity.PixelZDisplacement);
            AppendHex(text, "actorType", entity.ActorTypeDisplacement);
            AppendHex(text, "race", entity.RaceDisplacement);
            AppendHex(text, "vnum", entity.VirtualNumberDisplacement);
            AppendHex(text, "instName", entity.InstanceNameDisplacement);
            AppendHex(text, "selfVid", entity.SelfVidDisplacement);
            if (entity.NegatePixelY)
                text.Append("negateY=1\n");
            AppendHex(text, "mapDisp", entity.MapDisplacement);
            AppendHex(text, "mapHead", entity.MapHeadDisplacement);
            AppendHex(text, "nodeLeft", entity.NodeLeft);
            AppendHex(text, "nodeParent", entity.NodeParent);
            AppendHex(text, "nodeRight", entity.NodeRight);
            AppendHex(text, "nodeIsNil", entity.NodeIsNil);
            AppendHex(text, "nodeKey", entity.NodeKey);
            AppendHex(text, "nodeValue", entity.NodeValue);
            var path = Path.Combine(Path.GetTempPath(), "FarmBotAction_layout_" + Guid.NewGuid().ToString("N") + ".cfg");
            File.WriteAllText(path, text.ToString());
            return path;
        }
        catch
        {
            return "";
        }
    }

    private static void AppendHex(StringBuilder text, string key, uint value)
    {
        if (value != 0)
            text.Append(key).Append('=').Append(value.ToString("X")).Append('\n');
    }

    private static string Resolve(string fileName)
    {
        string? newest = null;
        foreach (var dir in new[]
                 {
                     AppContext.BaseDirectory,
                     Path.Combine(AppContext.BaseDirectory, "native"),
                     Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "FarmBotAction")),
                     @"C:\WORk\m2 farm bot\tools\FarmBotAction"
                 })
        {
            var path = Path.Combine(dir, fileName);
            if (!File.Exists(path))
                continue;
            if (newest == null || File.GetLastWriteTimeUtc(path) > File.GetLastWriteTimeUtc(newest))
                newest = path;
        }

        return newest ?? "";
    }

    private static void BuildNative()
    {
        var script = Resolve("build.ps1");
        if (script.Length == 0)
            script = @"C:\WORk\m2 farm bot\tools\FarmBotAction\build.ps1";
        if (!File.Exists(script))
            return;
        var psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var build = Process.Start(psi);
        build?.WaitForExit(90000);
    }
}
