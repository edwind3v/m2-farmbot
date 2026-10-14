using System.Text;

namespace FarmBot.Core.Combat;

public sealed record PlayerActionLayout(
    uint PreferredImageBase,
    uint PlayerSingletonRva,
    uint SetTargetRva,
    uint GetTargetVidRva,
    uint SetAttackKeyStateRva,
    uint ComboAttackRva,
    uint TargetVidDisplacement,
    uint SetTargetThisAdjustment,
    uint SetTargetVtableSlot,
    string SetTargetPrologue,
    IReadOnlyList<string> Evidence)
{
    public bool HasSetTarget =>
        PlayerSingletonRva is > 0x1000 and < 0x800000 &&
        (SetTargetVtableSlot != 0 || SetTargetRva is > 0 and < 0x800000);
}

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

        evidence.Add("MSVC x86 thiscall: ecx=CPythonPlayer*, args right-to-left, callee pops.");
        evidence.Add("SetTarget(DWORD vid, BOOL force=TRUE): thiscall, ret 8. Requires main actor, CanChangeTarget, live targetable VID. Sends target packet only.");
        evidence.Add("NEW_Attack(): thiscall, no args. Keeps __CanAttack, range, bow shot, shop, and facing checks. Does not move melee.");
        evidence.Add("SetAttackKeyState(bool): thiscall, ret 4. Sets m_isAtkKey; fishing intercept if pressed. Attack is applied later on the game thread.");
        evidence.Add("__OnPressActor(CInstanceBase&, DWORD, bool): private thiscall, ret 12. Ordinary click path: SetTarget, then attack-in-range or reserve-click approach.");
        evidence.Add("CPythonPlayer : CSingleton<CPythonPlayer> + IAbstractPlayer. Both singletons are process-lifetime; methods are game-thread only.");
        evidence.Add("Ordinary schedule: window thread Loop() → MessageProcess/Process() → player.Update / mouse / Python player.SetTarget.");
        evidence.Add("An adapter pipe worker is not that thread. SetTarget must not be thiscall'd from it.");
        evidence.Add("Release RVAs are recovered from METH_VARARGS wrappers. Debug 0x4EA* addresses are not used.");

        return new PlayerActionLayout(pe.ImageBase, singleton, setTargetRva, getTarget.native, attackKey.native, combo.native, targetDisp, virt.thisAdj, virt.slot, prologue, evidence);
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
