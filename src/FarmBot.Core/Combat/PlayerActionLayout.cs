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
        PlayerSingletonRva is > 0x1000 and < 0x800000 &&
        (SetTargetVtableSlot != 0 || SetTargetRva is > 0 and < 0x800000);

    public bool HasPlayerHud => StatusPointsDisplacement != 0 || PlayerNameDisplacement != 0;

    public int PointLevel { get; init; } = 1;
    public int PointExp { get; init; } = 3;
    public int PointNextExp { get; init; } = 4;
    public int PointHp { get; init; } = 5;
    public int PointMaxHp { get; init; } = 6;
    public int PointSp { get; init; } = 7;
    public int PointMaxSp { get; init; } = 8;
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
        var singleton = PreferInImage(pe, getTarget.singleton, setTarget.singleton, attackKey.singleton);
        if (singleton == 0)
            singleton = FindSingletonFromGetMainCharacterPosition(pe, evidence);
        var targetDisp = RecoverEcxLoad(pe, getTarget.native);
        if (targetDisp != 0)
            evidence.Add($"GetTargetVID reads [ecx+0x{targetDisp:X}] (m_dwTargetVID).");
        var virt = RecoverVirtualDispatch(pe, setTarget.wrapper);
        if (virt.slot != 0)
        {
            evidence.Add($"player.SetTarget is virtual: ecx=CPythonPlayer*+0x{virt.thisAdj:X}, call [vtable+0x{virt.slot:X}].");
            evidence.Add("The following E8 in that wrapper is Py_BuildNone, not CPythonPlayer::SetTarget.");
        }

        var setTargetRva = virt.slot == 0 ? setTarget.native : 0;
        var prologue = virt.slot == 0 ? pe.Hex(pe.ImageBase + setTarget.native, 16) : "";
        var hud = RecoverPlayerHud(pe, singleton, evidence);
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

        return new PlayerActionLayout(pe.ImageBase, singleton, setTargetRva, getTarget.native, attackKey.native, combo.native, pickClose.native, targetDisp, virt.thisAdj, virt.slot, prologue, hud.points, hud.name, hud.xor, evidence)
        {
            PointLevel = points.level,
            PointExp = points.exp,
            PointNextExp = points.next,
            PointHp = points.hp,
            PointMaxHp = points.maxHp,
            PointSp = points.sp,
            PointMaxSp = points.maxSp
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
        for (var i = off; i < off + 0x80 && i + 7 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
                thisAdj = pe.Byte(i + 2);
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x50)
                slot = pe.Byte(i + 4);
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x01 && pe.Byte(i + 2) == 0xFF && pe.Byte(i + 3) == 0x90)
                slot = pe.ReadU32(i + 4);
        }

        return (thisAdj, slot);
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

            if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15))
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
            for (var i = off; i < off + 0x80 && i + 6 < pe.Length; i++)
            {
                if (pe.Byte(i) == 0xA1 && pe.ReadU32(i + 1) == singletonVa)
                {
                    start = i + 5;
                    break;
                }

                if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15) &&
                    pe.ReadU32(i + 2) == singletonVa)
                {
                    start = i + 6;
                    break;
                }
            }
        }

        var first = SubstantialThiscall(pe, start, off + 0x100);
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
            return target - pe.ImageBase;
        }

        return 0;
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

    private static (uint points, uint name, uint xor) RecoverPlayerHud(ActionPe pe, uint singletonRva, List<string> evidence)
    {
        const uint knownMagic = 0xE73AC1DA;
        var xor = knownMagic;
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
                getPointVa = pe.FileToVa(FunctionStart(pe, i));
                evidence.Add($"GetPoint/GetStatus xor 0x{knownMagic:X8} loads [ecx+index*4+0x{disp:X}] at VA 0x{pe.FileToVa(i):X}.");
                break;
            }

            if (pointRel != 0)
                break;
        }

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
        return (points, name, xor);
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
        public bool IsData(uint va) => IsVa(va) && VaToFile(va) >= 0;

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
