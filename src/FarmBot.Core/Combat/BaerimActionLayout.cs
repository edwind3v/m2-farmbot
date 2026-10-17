using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FarmBot.Core.Entities;

namespace FarmBot.Core.Combat;

public sealed record BaerimNativeLayout(
    PlayerActionLayout Player,
    LoginNativeLayout Login,
    ReleaseLayout Entity,
    uint GetItemIndexVtableSlot,
    uint GetItemCountVtableSlot,
    uint IsSkillCoolTimeRva,
    uint PackMgrRva,
    uint PackGetRva,
    uint PackMappedCtorRva,
    uint PackExistRva,
    IReadOnlyList<string> Evidence);

public static class BaerimActionLayoutRecoverer
{
    /// <summary>Bump when cached cfg keys/semantics change (shared with ActionModuleHost).</summary>
    public const int LayoutCacheVersion = 11;

    public static string LayoutCachePath(string exePath)
    {
        var info = new FileInfo(exePath);
        var key = info.FullName + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture) +
                  "|v" + LayoutCacheVersion;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..16];
        return Path.Combine(Path.GetTempPath(), "FarmBotAction_layout_baerim_" + hash + ".cfg");
    }

    /// <summary>
    /// Instant load of the cfg written at inject time. Never parses the PE — that takes ~30s
    /// on baerim.exe and must not run on the bot tick thread.
    /// </summary>
    public static bool TryLoadCachedLayouts(string exePath, out ReleaseLayout entity, out PlayerActionLayout player)
    {
        entity = FallbackEntityLayout(exePath);
        player = FallbackPlayerLayout();
        try
        {
            var cachePath = LayoutCachePath(exePath);
            if (!File.Exists(cachePath) || new FileInfo(cachePath).Length < 64)
                return false;
            var map = ParseCfgHex(File.ReadAllText(cachePath));
            if (Hex(map, "charMgr") == 0 || Hex(map, "graphic") == 0 || Hex(map, "mapDisp") == 0)
                return false;
            entity = EntityFromCfg(exePath, map);
            player = PlayerFromCfg(map);
            return entity.CanEnumerate;
        }
        catch
        {
            return false;
        }
    }

    public static ReleaseLayout FallbackEntityLayout(string path) =>
        EntityFromCfg(path, new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["charMgr"] = 0x3C0FCC0,
            ["graphic"] = 0x3F4,
            ["pixelX"] = 0x514,
            ["pixelY"] = 0x518,
            ["pixelZ"] = 0x51C,
            ["actorType"] = 0x470,
            ["race"] = 0x338,
            ["vnum"] = 0x33C,
            ["instName"] = 0x60,
            ["selfVid"] = 0x334,
            ["negateY"] = 1,
            ["mapDisp"] = 0x84,
            ["nodeParent"] = 4,
            ["nodeRight"] = 8,
            ["nodeIsNil"] = 0xD,
            ["nodeKey"] = 0x10,
            ["nodeValue"] = 0x14
        });

    public static PlayerActionLayout FallbackPlayerLayout() =>
        PlayerFromCfg(new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["player"] = 0x3C0FCBC,
            ["playerName"] = 0x14,
            ["getStatusSlot"] = 0x10,
            ["thisAdj"] = 4
        });

    private static ReleaseLayout EntityFromCfg(string path, IReadOnlyDictionary<string, uint> map) =>
        new(
            path,
            PreferredImageBase: 0,
            CreateInstanceRva: 0,
            MapDisplacement: Hex(map, "mapDisp"),
            SingletonRva: Hex(map, "charMgr"),
            MainPointerDisplacement: Hex(map, "mainPtr"),
            GraphicDisplacement: Hex(map, "graphic"),
            RaceDisplacement: Hex(map, "race"),
            VirtualNumberDisplacement: Hex(map, "vnum"),
            ActorTypeDisplacement: Hex(map, "actorType"),
            SelfVidDisplacement: Hex(map, "selfVid"),
            PixelXDisplacement: Hex(map, "pixelX"),
            PixelYDisplacement: Hex(map, "pixelY"),
            PixelZDisplacement: Hex(map, "pixelZ"),
            NegatePixelY: Hex(map, "negateY") != 0,
            ProbeStringRva: 0,
            MapHeadDisplacement: Hex(map, "mapHead"),
            MapSizeDisplacement: Hex(map, "mapSize"),
            NodeLeft: Hex(map, "nodeLeft"),
            NodeParent: Hex(map, "nodeParent"),
            NodeRight: Hex(map, "nodeRight"),
            NodeIsNil: Hex(map, "nodeIsNil"),
            NodeKey: Hex(map, "nodeKey"),
            NodeValue: Hex(map, "nodeValue"),
            InstanceNameDisplacement: Hex(map, "instName"),
            Methods: Array.Empty<ReleaseMethod>(),
            Evidence: new[] { "baerim layout cfg/cache" });

    private static PlayerActionLayout PlayerFromCfg(IReadOnlyDictionary<string, uint> map) =>
        new(
            PreferredImageBase: 0,
            PlayerSingletonRva: Hex(map, "player"),
            SetTargetRva: Hex(map, "setTarget"),
            GetTargetVidRva: 0,
            SetAttackKeyStateRva: Hex(map, "attackKey"),
            ComboAttackRva: Hex(map, "combo"),
            PickCloseItemRva: Hex(map, "pickClose"),
            TargetVidDisplacement: Hex(map, "targetDisp"),
            SetTargetThisAdjustment: Hex(map, "thisAdj"),
            SetTargetVtableSlot: Hex(map, "setTargetSlot"),
            SetTargetPrologue: "",
            StatusPointsDisplacement: Hex(map, "points"),
            PlayerNameDisplacement: Hex(map, "playerName"),
            PointXorMask: Hex(map, "xor"),
            Evidence: new[] { "baerim layout cfg/cache" })
        {
            GetStatusVtableSlot = Hex(map, "getStatusSlot"),
            GetStatusRva = Hex(map, "getStatus"),
            SendAttackPacketRva = Hex(map, "sendAttack"),
            SendPickupPacketRva = Hex(map, "sendPickup"),
            SendItemUsePacketRva = Hex(map, "sendUse"),
            OnPressActorRva = Hex(map, "pressActor"),
            MoveToDestRva = Hex(map, "moveTo"),
            MoveToPixelRva = Hex(map, "movePixel"),
            GetPixelRva = Hex(map, "getPixel"),
            SendChatPacketRva = Hex(map, "sendChat"),
            ItemManagerRva = Hex(map, "itemMgr"),
            SkillManagerRva = Hex(map, "skillMgr")
        };

    private static uint Hex(IReadOnlyDictionary<string, uint> map, string key) =>
        map.TryGetValue(key, out var value) ? value : 0;

    private static Dictionary<string, uint> ParseCfgHex(string text)
    {
        var map = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            var eq = line.IndexOf('=');
            if (eq <= 0)
                continue;
            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();
            if (key.Equals("layout", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("profile", StringComparison.OrdinalIgnoreCase))
                continue;
            if (uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
                map[key] = hex;
        }

        return map;
    }

    public static BaerimNativeLayout Recover(string path)
    {
        var player = PlayerActionLayoutRecoverer.Recover(path);
        var login = PlayerActionLayoutRecoverer.RecoverLogin(path);
        var entity = ClientReleaseLayout.Recover(path);
        var evidence = new List<string>(player.Evidence);
        var pe = BaerimPe.Load(path);
        var map = RecoverAliveMap(pe, entity.SingletonRva, evidence);
        if (map != 0 && map != entity.MapDisplacement)
        {
            entity = entity with { MapDisplacement = map };
            evidence.Add($"Baerim alive-instance map this+0x{map:X}.");
        }
        if (entity.MapDisplacement < 0x40)
        {
            entity = entity with { MapDisplacement = 0x84 };
            evidence.Add("Baerim alive-instance map defaulted to RegisterInstance this+0x84.");
        }

        var status = RecoverPlayerMethod(pe, "GetStatus", player.PlayerSingletonRva, evidence);
        var statusVirt = RecoverVirtualDispatch(pe, status.wrapper);
        if (statusVirt.slot != 0)
        {
            player = player with { GetStatusVtableSlot = statusVirt.slot, GetStatusRva = 0 };
            evidence.Add($"Baerim GetStatus virtual [vtable+0x{statusVirt.slot:X}].");
        }

        var index = RecoverPlayerMethod(pe, "GetItemIndex", player.PlayerSingletonRva, evidence);
        var count = RecoverPlayerMethod(pe, "GetItemCount", player.PlayerSingletonRva, evidence);
        var indexVirt = RecoverVirtualDispatch(pe, index.wrapper);
        var countVirt = RecoverVirtualDispatch(pe, count.wrapper);
        if (indexVirt.slot != 0)
            evidence.Add($"Baerim GetItemIndex virtual [vtable+0x{indexVirt.slot:X}].");
        if (countVirt.slot != 0)
            evidence.Add($"Baerim GetItemCount virtual [vtable+0x{countVirt.slot:X}].");

        var cool = RecoverPlayerMethod(pe, "IsSkillCoolTime", player.PlayerSingletonRva, evidence);
        var coolDirect = FirstCallAfterSingleton(pe, cool.wrapper, player.PlayerSingletonRva == 0 ? 0 : pe.ImageBase + player.PlayerSingletonRva);
        var wrapOff = cool.wrapper == 0 ? -1 : pe.VaToFile(pe.ImageBase + cool.wrapper);
        var pythonParse = wrapOff < 0 ? 0 : RelativeCall(pe, wrapOff, wrapOff + 0x28);
        var pythonParse2 = wrapOff < 0 ? 0 : RelativeCall(pe, wrapOff + 0x14, wrapOff + 0x2C);
        var coolRva = player.IsSkillCoolTimeRva;
        if (coolDirect != 0 && coolDirect != pythonParse && coolDirect != pythonParse2)
        {
            coolRva = coolDirect;
            evidence.Add($"Baerim IsSkillCoolTime thiscall 0x{coolDirect:X}.");
        }
        else if (coolRva == pythonParse || coolRva == pythonParse2)
        {
            coolRva = 0;
            evidence.Add("Baerim IsSkillCoolTime python helper discarded.");
        }

        var active = RecoverPlayerMethod(pe, "IsSkillActive", player.PlayerSingletonRva, evidence);
        var activeDirect = FirstCallAfterSingleton(pe, active.wrapper, player.PlayerSingletonRva == 0 ? 0 : pe.ImageBase + player.PlayerSingletonRva);
        var activeWrapOff = active.wrapper == 0 ? -1 : pe.VaToFile(pe.ImageBase + active.wrapper);
        var activePy = activeWrapOff < 0 ? 0 : RelativeCall(pe, activeWrapOff, activeWrapOff + 0x28);
        var activePy2 = activeWrapOff < 0 ? 0 : RelativeCall(pe, activeWrapOff + 0x14, activeWrapOff + 0x2C);
        var activeRva = player.IsSkillActiveRva;
        if (activeDirect != 0 && activeDirect != activePy && activeDirect != activePy2)
        {
            activeRva = activeDirect;
            evidence.Add($"Baerim IsSkillActive thiscall 0x{activeDirect:X}.");
        }
        else if (activeRva == activePy || activeRva == activePy2)
        {
            activeRva = 0;
            evidence.Add("Baerim IsSkillActive python helper discarded.");
        }

        // SendChatPacket / IsMountingHorse recovery often lands on Python helpers (0x41xxxx–0x42xxxx).
        // Unwrap SendChat native thiscall via net-stream singleton (same pattern as IsSkillCool).
        // IsMountingHorse → discard; runtime uses main actor horse flag at +0x30.
        var chatRva = RecoverSendChatNative(pe, login.NetStreamRva, player.SendChatPacketRva, evidence);
        if (chatRva == 0 || IsPythonHelperRva(chatRva))
        {
            chatRva = 0;
            evidence.Add("Baerim SendChatPacket python helper discarded.");
        }

        var mountRva = player.IsMountingHorseRva;
        if (mountRva == 0 || IsPythonHelperRva(mountRva))
        {
            mountRva = 0;
            evidence.Add("Baerim IsMountingHorse python helper discarded; runtime uses main+0x30.");
        }

        // MoveToDest often recovers as a Python helper; pixel mover is enough for sidesteps.
        if (player.MoveToDestRva == 0 && player.MoveToPixelRva != 0)
        {
            player = player with { MoveToDestRva = player.MoveToPixelRva };
            evidence.Add($"Baerim MoveToDest aliased to MoveToPixel 0x{player.MoveToPixelRva:X}.");
        }

        var getAffect = player.GetAffectRva;
        var getValue = player.GetValueRva;
        if (IsPythonHelperRva(getAffect))
        {
            getAffect = 0;
            evidence.Add("Baerim GetAffect python helper discarded.");
        }
        if (IsPythonHelperRva(getValue))
        {
            getValue = 0;
            evidence.Add("Baerim GetValue python helper discarded.");
        }
        var getClose = player.GetCloseItemRva;
        if (getClose != 0 && getClose == player.GetPixelRva)
        {
            getClose = 0;
            evidence.Add("Baerim GetCloseItem discarded (aliased GetPixelPosition).");
        }

        // SetDiffuseColor recovered as a Python helper; SetGameRenderState is a 1-float
        // thiscall that rets 4. Calling either from the render hook stack-smashes the client.
        if (player.SetDiffuseColorRva != 0 || player.SetGameRenderStateRva != 0)
            evidence.Add("Baerim farm circle discarded setDiffuse/setGameRs (wrong arity); color is screen+8.");

        player = player with
        {
            IsSkillCoolTimeRva = coolRva,
            IsSkillActiveRva = activeRva,
            SendChatPacketRva = chatRva,
            IsMountingHorseRva = mountRva,
            GetAffectRva = getAffect,
            GetValueRva = getValue,
            GetCloseItemRva = getClose,
            SetDiffuseColorRva = 0,
            SetGameRenderStateRva = 0,
            SetInterfaceRenderStateRva = 0
        };

        var pack = RecoverPack(pe, evidence);
        return new BaerimNativeLayout(
            player,
            login,
            entity,
            indexVirt.slot,
            countVirt.slot,
            coolRva,
            pack.mgr,
            pack.get,
            pack.ctor,
            pack.exist,
            evidence);
    }

    private static bool IsPythonHelperRva(uint rva) => rva is >= 0x410000 and < 0x430000;

    private static uint RecoverSendChatNative(BaerimPe pe, uint netStreamRva, uint fallbackRva, List<string> evidence)
    {
        var netVa = netStreamRva == 0 ? 0u : pe.ImageBase + netStreamRva;
        foreach (var va in pe.FindAsciiZ("SendChatPacket"))
        {
            foreach (var site in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(site);
                if (off < 0 || !TryMethodTable(pe, off, out var fn))
                    continue;
                var wrapperRva = fn - pe.ImageBase;
                var singleton = FirstGlobalLoad(pe, wrapperRva);
                var preferVa = netVa != 0 ? netVa : singleton;
                if (preferVa == 0)
                    continue;
                var direct = FirstNativeCallAfterSingleton(pe, wrapperRva, preferVa);
                if (direct == 0 || IsPythonHelperRva(direct))
                    continue;
                var singletonRva = singleton == 0 ? 0 : singleton - pe.ImageBase;
                evidence.Add($"Baerim SendChatPacket thiscall 0x{direct:X} (wrapper 0x{wrapperRva:X} load 0x{singletonRva:X}).");
                return direct;
            }
        }

        foreach (var marker in new[]
                 {
                     "CPythonNetworkStream::SendChatPacket",
                     "SendChatPacket Error",
                     "Send Chat Packet Error"
                 })
        {
            var fromString = RecoverFunctionFromUniqueString(pe, marker, evidence);
            if (fromString != 0 && !IsPythonHelperRva(fromString))
            {
                evidence.Add($"Baerim SendChatPacket from '{marker}' 0x{fromString:X}.");
                return fromString;
            }
        }

        if (fallbackRva != 0 && !IsPythonHelperRva(fallbackRva))
            return fallbackRva;
        return 0;
    }

    private static uint RecoverFunctionFromUniqueString(BaerimPe pe, string text, List<string> evidence)
    {
        var hits = pe.FindAsciiZ(text).ToList();
        if (hits.Count == 0)
            return 0;
        if (hits.Count > 1)
        {
            evidence.Add($"Baerim '{text}' not unique ({hits.Count} hits).");
            return 0;
        }

        foreach (var site in pe.FindImmediateXrefs(hits[0]))
        {
            var off = pe.VaToFile(site);
            if (off < 0)
                continue;
            var start = FunctionStartFile(pe, off);
            if (start < 0)
                continue;
            var rva = pe.FileToVa(start) - pe.ImageBase;
            if (pe.IsCode(pe.ImageBase + rva) && !IsPythonHelperRva(rva))
                return rva;
        }

        return 0;
    }

    private static uint FirstNativeCallAfterSingleton(BaerimPe pe, uint wrapperRva, uint singletonVa)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0 || singletonVa == 0)
            return 0;
        var start = -1;
        for (var i = off; i < off + 0x140 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == singletonVa)
            {
                start = i + 5;
                break;
            }

            if (pe.Byte(i) == 0x8B &&
                (pe.Byte(i + 1) is 0x0D or 0x15 or 0x35 or 0x3D) &&
                pe.ReadU32(i + 2) == singletonVa)
            {
                start = i + 6;
                break;
            }
        }

        // Do not scan from wrapper entry — that picks unrelated E8 targets when net is never loaded.
        if (start < 0)
            return 0;

        for (var i = start; i < off + 0x160 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (!pe.IsCode(target))
                continue;
            var destOff = pe.VaToFile(target);
            if (destOff < 0 || (pe.Byte(destOff) == 0xFF && pe.Byte(destOff + 1) == 0x25))
                continue;
            var rva = target - pe.ImageBase;
            if (IsPythonHelperRva(rva) || !LooksLikeNativeThiscall(pe, destOff))
                continue;
            return rva;
        }

        return 0;
    }

    private static bool LooksLikeNativeThiscall(BaerimPe pe, int destOff)
    {
        if (destOff < 0 || destOff + 3 >= pe.Length)
            return false;
        // Typical MSVC thiscall prologues.
        if (pe.Byte(destOff) == 0x55 && pe.Byte(destOff + 1) == 0x8B && pe.Byte(destOff + 2) == 0xEC)
            return true;
        if (pe.Byte(destOff) == 0x53 && pe.Byte(destOff + 1) == 0x56) // push ebx; push esi
            return true;
        if (pe.Byte(destOff) == 0x56 && pe.Byte(destOff + 1) == 0x57) // push esi; push edi
            return true;
        if (pe.Byte(destOff) == 0x8B && pe.Byte(destOff + 1) == 0xFF) // mov edi, edi hotpatch
            return true;
        return false;
    }

    private static (uint native, uint singleton, uint wrapper) RecoverPlayerMethod(
        BaerimPe pe, string name, uint preferSingletonRva, List<string> evidence)
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
                var singletonRva = singleton == 0 ? 0 : singleton - pe.ImageBase;
                var candidate = (0u, pe.IsRva(singletonRva) ? singletonRva : 0u, wrapperRva);
                if (preferVa != 0 && singleton == preferVa)
                    return candidate;
                if (fallback.wrapper == 0)
                    fallback = candidate;
            }
        }

        if (fallback.wrapper == 0)
            evidence.Add($"Baerim Python '{name}' was not recovered.");
        return fallback;
    }

    private static bool TryMethodTable(BaerimPe pe, int off, out uint fn)
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

    private static uint FirstGlobalLoad(BaerimPe pe, uint rva)
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

    private static uint RecoverAliveMap(BaerimPe pe, uint singletonRva, List<string> evidence)
    {
        var fromRegister = RecoverMapFromRegisterInstance(pe, evidence);
        if (fromRegister != 0)
            return fromRegister;

        uint found = 0;
        if (singletonRva != 0)
        {
            foreach (var site in pe.FindImmediateXrefs(pe.ImageBase + singletonRva))
            {
                var start = FunctionStartFile(pe, pe.VaToFile(site));
                if (start < 0)
                    continue;
                var disp = AliveMapDisplacement(pe, start, start + 0x400);
                if (disp < 0x40)
                    continue;
                if (found == 0)
                    found = disp;
            }
        }

        if (found == 0)
            evidence.Add("Baerim alive-instance map displacement was not recovered.");
        return found;
    }

    private static uint RecoverMapFromRegisterInstance(BaerimPe pe, List<string> evidence)
    {
        uint create = 0;
        foreach (var text in new[]
                 {
                     "CPythonCharacterManager::CreateInstance VID[%d] Race[%d]",
                     "CPythonCharacterManager::CreateInstance"
                 })
        {
            if (!pe.TryFindAscii(text, out var va))
                continue;
            var sites = pe.FindImmediateXrefs(va);
            if (sites.Count == 0)
                continue;
            var start = FunctionStartFile(pe, pe.VaToFile(sites[0]));
            if (start < 0)
                continue;
            create = pe.FileToVa(start) - pe.ImageBase;
            break;
        }

        if (create == 0)
            return 0;
        var createOff = pe.VaToFile(pe.ImageBase + create);
        var register = RelativeCall(pe, createOff, createOff + 0x40);
        var body = register != 0 ? register : create;
        var off = pe.VaToFile(pe.ImageBase + body);
        if (off < 0)
            return 0;
        for (var i = off; i + 6 < off + 0x80 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x8D || (pe.Byte(i + 1) & 0xC0) != 0x80)
                continue;
            var disp = pe.ReadU32(i + 2);
            if (disp is < 0x40 or > 0xC0)
                continue;
            evidence.Add($"Baerim RegisterInstance alive-map this+0x{disp:X}.");
            return disp;
        }

        return 0;
    }

    private static uint AliveMapDisplacement(BaerimPe pe, int off, int end)
    {
        for (var i = off; i < end && i + 4 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x80 || pe.Byte(i + 2) != 0x0D || pe.Byte(i + 3) != 0x00)
                continue;
            if ((pe.Byte(i + 1) & 0xC0) != 0x40)
                continue;
            for (var j = i; j >= off && i - j < 0x50; j--)
            {
                if (pe.Byte(j) == 0x8D && (pe.Byte(j + 1) & 0xC0) == 0x40)
                {
                    var disp = pe.Byte(j + 2);
                    if (disp is >= 0x10 and <= 0x40)
                        return disp;
                }

                if (pe.Byte(j) == 0x83 && pe.Byte(j + 1) == 0xC1)
                {
                    var disp = pe.Byte(j + 2);
                    if (disp is >= 0x10 and <= 0x40)
                        return disp;
                }
            }
        }

        return 0;
    }

    private static (uint thisAdj, uint slot) RecoverVirtualDispatch(BaerimPe pe, uint wrapperRva)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0)
            return (0, 0);
        uint thisAdj = 0;
        var leaAt = -1;
        var lim = Math.Min(pe.Length - 7, off + 0xC0);
        for (var i = off; i < lim; i++)
        {
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
            {
                thisAdj = pe.Byte(i + 2);
                leaAt = i;
                break;
            }

            if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x49)
            {
                thisAdj = pe.Byte(i + 2);
                leaAt = i;
                break;
            }
        }

        if (leaAt < 0)
            return (0, 0);

        var end = lim;
        for (var i = leaAt + 3; i < lim; i++)
        {
            if (pe.Byte(i) == 0xC3 || pe.Byte(i) == 0xC2)
            {
                end = i;
                break;
            }
        }

        for (var i = leaAt; i + 6 < end; i++)
        {
            if (pe.Byte(i) != 0x8B || pe.Byte(i + 1) != 0x01 || pe.Byte(i + 2) != 0x8B)
                continue;
            if (pe.Byte(i + 5) == 0xFF && pe.Byte(i + 6) == 0xD0 &&
                pe.Byte(i + 3) is 0x40 or 0x48 or 0x50 or 0x58 or 0x70 or 0x78)
                return (thisAdj, pe.Byte(i + 4));
            if (i + 9 < end && pe.Byte(i + 8) == 0xFF && pe.Byte(i + 9) == 0xD0 &&
                pe.Byte(i + 3) is 0x80 or 0x88 or 0x90 or 0x98 or 0xB0 or 0xB8)
                return (thisAdj, pe.ReadU32(i + 4));
        }

        uint slot = 0;
        for (var i = leaAt; i + 6 < end; i++)
        {
            if (slot == 0 && pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x50)
                slot = pe.Byte(i + 4);
            if (slot == 0 && pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x90)
                slot = pe.ReadU32(i + 4);
            if (slot == 0 && pe.Byte(i) == 0xFF && pe.Byte(i + 1) == 0x51)
                slot = pe.Byte(i + 2);
            if (slot == 0 && pe.Byte(i) == 0xFF && pe.Byte(i + 1) == 0x91)
                slot = pe.ReadU32(i + 2);
            if (slot != 0)
                return (thisAdj, slot);
        }

        for (var i = leaAt; i + 7 < end; i++)
        {
            if (i < 2 || pe.Byte(i - 2) != 0x8B || pe.Byte(i - 1) != 0x01 || pe.Byte(i) != 0x8B)
                continue;
            if (pe.Byte(i + 1) is 0x40 or 0x48 or 0x50 or 0x58 or 0x70 or 0x78)
                return (thisAdj, pe.Byte(i + 2));
            if (pe.Byte(i + 1) is 0x80 or 0x88 or 0x90 or 0x98 or 0xB0 or 0xB8)
                return (thisAdj, pe.ReadU32(i + 2));
        }

        return (thisAdj, 0);
    }

    private static uint FirstCallAfterSingleton(BaerimPe pe, uint wrapperRva, uint singletonVa)
    {
        var off = pe.VaToFile(pe.ImageBase + wrapperRva);
        if (off < 0 || singletonVa == 0)
            return 0;
        var start = off;
        for (var i = off; i < off + 0x80 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x0D && pe.ReadU32(i + 2) == singletonVa)
            {
                start = i + 6;
                break;
            }
        }

        return RelativeCall(pe, start, off + 0xA0);
    }

    private static uint RelativeCall(BaerimPe pe, int start, int end)
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

    private static int FunctionStartFile(BaerimPe pe, int off)
    {
        for (var i = off; i > off - 0x400 && i > 16; i--)
        {
            if (pe.Byte(i) == 0xCC && pe.Byte(i + 1) != 0xCC)
                return i + 1;
        }

        for (var i = off; i > off - 0x80 && i > 16; i--)
        {
            if (pe.Byte(i) == 0x55 && pe.Byte(i + 1) == 0x8B && pe.Byte(i + 2) == 0xEC)
                return i;
        }

        return -1;
    }

    private static (uint mgr, uint get, uint ctor, uint exist) RecoverPack(BaerimPe pe, List<string> evidence)
    {
        var exist = RecoverPlayerMethod(pe, "IsExistFile", 0, evidence);
        var open = RecoverPlayerMethod(pe, "OpenTextFile", 0, evidence);
        uint mgr = exist.singleton;
        uint packGet = 0;
        uint ctor = 0;
        uint existNative = mgr == 0 || exist.wrapper == 0
            ? 0
            : FirstCallAfterSingleton(pe, exist.wrapper, pe.ImageBase + mgr);
        if (open.wrapper != 0 && mgr != 0)
        {
            var off = pe.VaToFile(pe.ImageBase + open.wrapper);
            var mgrVa = pe.ImageBase + mgr;
            if (off >= 0)
            {
                var lim = Math.Min(pe.Length - 6, off + 0x180);
                for (var i = off; i < lim; i++)
                {
                    if (pe.Byte(i) != 0x8B || pe.Byte(i + 1) != 0x0D || pe.ReadU32(i + 2) != mgrVa)
                        continue;
                    packGet = RelativeCall(pe, i + 6, i + 0x28);
                    for (var j = i - 5; j >= off && i - j < 0x30; j--)
                    {
                        if (pe.Byte(j) != 0xE8)
                            continue;
                        var dest = (uint)(pe.FileToVa(j + 5) + pe.ReadI32(j + 1));
                        if (pe.IsCode(dest))
                            ctor = dest - pe.ImageBase;
                        break;
                    }

                    break;
                }
            }
        }

        if (mgr == 0 || packGet == 0)
            evidence.Add("Baerim CEterPackManager::Get was not recovered.");
        else
            evidence.Add($"Baerim packMgr=0x{mgr:X} packGet=0x{packGet:X} packCtor=0x{ctor:X} packExist=0x{existNative:X}.");
        return (mgr, packGet, ctor, existNative);
    }

    private sealed class BaerimPe
    {
        private readonly byte[] _bytes;
        private readonly List<(uint Va, uint Raw, uint RawSize)> _sections = new();
        public uint ImageBase { get; }
        public uint SizeOfImage { get; }
        public int Length => _bytes.Length;

        public static BaerimPe Load(string path) => new(path);

        private BaerimPe(string path)
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
