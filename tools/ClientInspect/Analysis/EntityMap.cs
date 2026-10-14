using System.Text;

namespace ClientInspect.Analysis;

public static class EntityMap
{
    public static IReadOnlyList<FunctionRef> Analyze(string path, List<Fact> facts)
    {
        var pe = new MappedPe(path);
        var functions = new List<FunctionRef>();
        var targets = new (string Needle, string Role)[]
        {
            ("CPythonCharacterManager::CreateInstance VID[%d] Race[%d]", "create CInstanceBase by VID and race"),
            ("CPythonCharacterManager::CreateInstance: VID[%d] - ALREADY EXIST", "duplicate-VID guard"),
            ("DeleteCharacterInstance: no vid by %d", "delete CInstanceBase by VID"),
            ("SelectCharacterInstance: no vid by %d", "select CInstanceBase by VID"),
            ("CNetworkActorManager::__AppendCharacterManagerActor(%d, %s)", "network actor -> character manager"),
            ("CNetworkActorManager::RemoveActor(dwVID=%d) - NOT EXIST VID", "remove network actor by VID"),
            ("CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID", "move network actor by VID"),
            ("CNetworkActorManager::UpdateActor(dwVID=%d) - NOT EXIST VID", "update network actor by VID"),
            ("CNetworkActorManager::SyncActor(dwVID=%d) - NOT EXIST VID", "sync network actor by VID"),
            ("StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]", "Metin stone detector"),
            ("GetPixelPosition", "instance position getter (Python/C++)"),
            ("GetInstanceType", "instance type getter"),
            ("IsEnemy", "enemy predicate"),
            ("IsNPC", "NPC predicate"),
            ("GetMainVID", "local player VID"),
            ("GetNameByVID", "name lookup"),
            ("TPacketGCCharacterAdditionalInfo name=%s vid=%d race=%d Error", "name/VID/race packet")
        };

        foreach (var (needle, role) in targets)
        {
            if (!pe.TryFindAscii(needle, out var va))
                continue;
            var sites = pe.FindImmediateXrefs(va);
            if (sites.Count == 0)
            {
                functions.Add(new FunctionRef(needle, Path.GetFileName(path), $"str 0x{va:X}", role, Array.Empty<string>()));
                continue;
            }

            var byFn = sites
                .Select(site => (Site: site, Fn: pe.FunctionStart(site)))
                .GroupBy(item => item.Fn)
                .OrderBy(group => group.Key)
                .ToList();
            foreach (var group in byFn)
            {
                var callers = pe.FindCallers(group.Key).Take(8).Select(addr => $"0x{addr:X}").ToList();
                functions.Add(new FunctionRef(
                    needle,
                    Path.GetFileName(path),
                    $"0x{group.Key:X}",
                    role + $" (string xrefs at {string.Join(", ", group.Select(item => "0x" + item.Site.ToString("X")))})",
                    callers));
            }
        }

        AddTypeConstants(pe, facts);
        AddStoneFacts(pe, facts, functions);
        AddPositionFacts(pe, facts, functions);
        AddStoreRetrieveFacts(facts);
        AddSharedImageFacts(path, facts);
        facts.Add(new Fact(
            "confirmed",
            $"{Path.GetFileName(path)} preferred base 0x{pe.ImageBase:X}. Function addresses below are file VAs at that base; the live release process has ASLR and may not match."));
        facts.Add(new Fact(
            "unresolved",
            "Full CInstanceBase member layout beyond the debug-confirmed displacements below is not claimed. Release ASLR and compile options can move fields. Live instance pointers were not read."));
        facts.Add(new Fact(
            "unresolved",
            "Entity enumeration has not been demonstrated. No runtime walk of m_kAliveInstMap was performed."));
        return functions;
    }

    private static void AddTypeConstants(MappedPe pe, List<Fact> facts)
    {
        var names = new[]
        {
            "INSTANCE_TYPE_PLAYER",
            "INSTANCE_TYPE_NPC",
            "INSTANCE_TYPE_ENEMY",
            "INSTANCE_TYPE_BUILDING",
            "INSTANCE_TYPE_OBJECT"
        };
        var recovered = new List<string>();
        foreach (var name in names)
        {
            if (!pe.TryFindAscii(name, out var va))
                continue;
            var value = pe.GuessIntConstantNear(va);
            recovered.Add(value != null ? $"{name}={value.Value}" : name);
        }

        if (recovered.Count > 0)
        {
            facts.Add(new Fact(
                "confirmed",
                "Python instance-type constants (push imm8 before the name pointer): " + string.Join(", ", recovered) + ". INSTANCE_TYPE_STONE is absent as a Python name."));
        }

        facts.Add(new Fact(
            "confirmed",
            "CActorInstance::EType is sequential and matches those exports: TYPE_ENEMY=0, TYPE_NPC=1, TYPE_STONE=2, TYPE_WARP=3, TYPE_DOOR=4, TYPE_BUILDING=5, TYPE_PC=6, TYPE_POLY=7, TYPE_HORSE=8, TYPE_GOTO=9, TYPE_OBJECT=10."));
    }

    private static void AddStoneFacts(MappedPe pe, List<Fact> facts, List<FunctionRef> functions)
    {
        facts.Add(new Fact(
            "confirmed",
            "StoneDetect is a CPythonNetworkStream::ServerCommand token. The handler string sits next to ObserverTeamInfo and StartStaminaConsume. Arguments are VID, distance 0-3, and angle. The client looks up that VID and plays a compass or firecracker effect. It does not classify stones."));
        facts.Add(new Fact(
            "confirmed",
            "IsStone exists in C++ as m_GraphicThingInstance.IsStone() (m_eActorType == TYPE_STONE). It is not exported to Python, so there is no IsStone / INSTANCE_TYPE_STONE string."));
        facts.Add(new Fact(
            "confirmed",
            "Ordinary monsters are TYPE_ENEMY / IsEnemy. Metin stones are TYPE_STONE=2 on the actor, copied from the server's SNetworkActorData.m_bType through SCreateData.m_bType. Race/VNUM is a separate field used for appearance and mob-proto lookup (m_eRaceType)."));
        if (functions.Any(item => item.Name.StartsWith("StoneDetect", StringComparison.Ordinal)))
        {
            facts.Add(new Fact(
                "confirmed",
                "StoneDetect has a code xref in this image (ServerCommand dispatcher). Runtime VID tables were not read."));
        }
    }

    private static void AddPositionFacts(MappedPe pe, List<Fact> facts, List<FunctionRef> functions)
    {
        facts.Add(new Fact(
            "confirmed",
            "chr.GetPixelPosition is METH_VARARGS (__cdecl PyCFunction(self, args)). It parses VID at args[0], calls GetInstancePtr, then NEW_GetPixelPosition, and returns Py_BuildValue(\"fff\", x, y, z)."));
        facts.Add(new Fact(
            "confirmed",
            "player.GetMainCharacterPosition is METH_VARARGS with no VID. It calls CPythonPlayer::NEW_GetMainActorPosition, which uses the main CInstanceBase or the camera center, and returns the same \"fff\" triple."));
        facts.Add(new Fact(
            "confirmed",
            "TPixelPosition is D3DXVECTOR3: three IEEE-754 floats, 12 bytes, x then y then z. Debug NEW_GetPixelPosition at 0x4D5E10 is __thiscall void(TPixelPosition* out) and copies those 12 bytes from the graphic object at CInstanceBase+0x1F8. NEW_GetCurPixelPositionRef sets x=+m_x, y=-m_y, z=+m_z."));
        facts.Add(new Fact(
            "confirmed",
            "Network spawn positions are LONG m_lPosX / m_lPosY on SCreateData. Create converts them with SCRIPT_SetPixelPosition((float)x, (float)y). MoveActor / SyncActor stay VID-keyed."));
        if (!functions.Any(item => item.Name == "GetPixelPosition" && item.Address.StartsWith("0x", StringComparison.Ordinal)))
        {
            facts.Add(new Fact(
                "unresolved",
                "GetPixelPosition has no immediate-string xref in .text (Python method name stored in a table, not pushed as a format string)."));
        }
    }

    private static void AddStoreRetrieveFacts(List<Fact> facts)
    {
        facts.Add(new Fact(
            "confirmed",
            "CPythonCharacterManager stores live instances in m_kAliveInstMap (std::map<DWORD, CInstanceBase*>). Debug CreateInstance/Select/Delete all do this+0x20 before map.find. GetInstancePtr is find(VID)->second. RegisterInstance inserts a pooled CInstanceBase under that VID. CreateInstance is RegisterInstance then CInstanceBase::Create(SCreateData); duplicate VID logs ALREADY EXIST."));
        facts.Add(new Fact(
            "confirmed",
            "SCreateData displacements verified in the debug CreateInstance/Append path: +0x00 m_bType (BYTE), +0x14 m_dwVID (DWORD), +0x18 m_dwRace (DWORD). m_isMain is the post-create SelectInstance flag (debug byte at +0x74 on that compiled struct)."));
        facts.Add(new Fact(
            "confirmed",
            "Relationships: VID is the map key and GetVirtualID. Race is m_dwRace (appearance / proto VNUM). Virtual number is m_dwVirtualNumber (special-case VNUMs). Type is CActorInstance::m_eActorType from the server m_bType. Position is the graphic-object pixel triple, not the network LONG pair after create."));
        facts.Add(new Fact(
            "confirmed",
            "CNetworkActorManager keeps a parallel std::map<DWORD, SNetworkActorData> (m_kNetActorDict). Append copies type/VID/race/name/LONG x,y into SCreateData and calls CreateInstance. Debug MoveActor looks up that dict at this+0x18."));
    }

    private static void AddSharedImageFacts(string debugPath, List<Fact> facts)
    {
        var releasePath = Path.Combine(Path.GetDirectoryName(debugPath) ?? "", "1.metin2client.exe");
        if (!File.Exists(releasePath))
            return;

        var shared = new[]
        {
            ".?AVCPythonCharacterManager@@",
            ".?AVCInstanceBase@@",
            ".?AVCNetworkActorManager@@",
            "INSTANCE_TYPE_ENEMY",
            "GetPixelPosition",
            "GetMainCharacterPosition",
            "HasInstance",
            "IsEnemy",
            "StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]",
            "CPythonCharacterManager::CreateInstance VID[%d] Race[%d]",
            "CNetworkActorManager::__AppendCharacterManagerActor(%d, %s)",
            "DeleteCharacterInstance: no vid by %d"
        };
        var present = new List<string>();
        var debugOnly = new List<string>();
        var releaseBytes = File.ReadAllBytes(releasePath);
        var debugBytes = File.ReadAllBytes(debugPath);
        foreach (var name in shared)
        {
            var needle = Encoding.ASCII.GetBytes(name);
            if (Contains(releaseBytes, needle) && Contains(debugBytes, needle))
                present.Add(name);
        }

        if (Contains(debugBytes, Encoding.ASCII.GetBytes("CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID")) &&
            !Contains(releaseBytes, Encoding.ASCII.GetBytes("CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID")))
            debugOnly.Add("MoveActor NOT EXIST VID (debug log)");

        facts.Add(new Fact(
            "confirmed",
            "Shared between debug and live release images (string presence): " + string.Join("; ", present) + "."));
        if (debugOnly.Count > 0)
        {
            facts.Add(new Fact(
                "confirmed",
                "Debug-only strings (stripped or absent in release): " + string.Join("; ", debugOnly) + ". Function VAs are not shared; release has ASLR."));
        }
    }

    private static bool Contains(byte[] hay, byte[] needle)
    {
        for (var i = 0; i + needle.Length < hay.Length; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (hay[i + j] != needle[j])
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                return true;
        }

        return false;
    }

    private sealed class MappedPe
    {
        private readonly byte[] _bytes;
        private readonly List<Section> _sections = new();
        public uint ImageBase { get; }

        public MappedPe(string path)
        {
            _bytes = File.ReadAllBytes(path);
            var eLfanew = BitConverter.ToInt32(_bytes, 0x3C);
            ImageBase = BitConverter.ToUInt32(_bytes, eLfanew + 24 + 28);
            var sections = BitConverter.ToUInt16(_bytes, eLfanew + 6);
            var optional = BitConverter.ToUInt16(_bytes, eLfanew + 20);
            var off = eLfanew + 24 + optional;
            for (var i = 0; i < sections; i++)
            {
                var at = off + i * 40;
                _sections.Add(new Section(
                    Encoding.ASCII.GetString(_bytes, at, 8).TrimEnd('\0'),
                    BitConverter.ToUInt32(_bytes, at + 12),
                    BitConverter.ToUInt32(_bytes, at + 8),
                    BitConverter.ToUInt32(_bytes, at + 20),
                    BitConverter.ToUInt32(_bytes, at + 16)));
            }
        }

        public bool TryFindAscii(string text, out uint va)
        {
            var needle = Encoding.ASCII.GetBytes(text);
            for (var i = 0; i + needle.Length < _bytes.Length; i++)
            {
                var ok = true;
                for (var j = 0; j < needle.Length; j++)
                {
                    if (_bytes[i + j] != needle[j])
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok)
                    continue;
                va = FileToVa(i);
                if (va != 0)
                    return true;
            }

            va = 0;
            return false;
        }

        public List<uint> FindImmediateXrefs(uint value)
        {
            var needle = BitConverter.GetBytes(value);
            var hits = new List<uint>();
            var text = _sections.FirstOrDefault(section => section.Name.StartsWith(".text", StringComparison.Ordinal));
            if (text == null)
                return hits;
            var start = (int)text.Raw;
            var end = Math.Min(_bytes.Length, start + (int)text.RawSize - 4);
            for (var i = start; i < end; i++)
            {
                if (_bytes[i] == needle[0] && _bytes[i + 1] == needle[1] && _bytes[i + 2] == needle[2] && _bytes[i + 3] == needle[3])
                    hits.Add(FileToVa(i));
            }

            return hits;
        }

        public uint FunctionStart(uint siteVa)
        {
            var off = VaToFile(siteVa);
            if (off < 8)
                return siteVa;
            var min = Math.Max(0, off - 0x800);
            for (var i = off; i >= min + 3; i--)
            {
                if (_bytes[i] == 0x55 && _bytes[i + 1] == 0x8B && _bytes[i + 2] == 0xEC &&
                    (i == 0 || _bytes[i - 1] == 0xCC || _bytes[i - 1] == 0xC3 || _bytes[i - 1] == 0xC2))
                    return FileToVa(i);
            }

            return siteVa;
        }

        public List<uint> FindCallers(uint functionVa)
        {
            var callers = new List<uint>();
            var text = _sections.FirstOrDefault(section => section.Name.StartsWith(".text", StringComparison.Ordinal));
            if (text == null)
                return callers;
            var start = (int)text.Raw;
            var end = Math.Min(_bytes.Length, start + (int)text.RawSize - 5);
            for (var i = start; i < end; i++)
            {
                if (_bytes[i] != 0xE8)
                    continue;
                var next = FileToVa(i + 5);
                var rel = BitConverter.ToInt32(_bytes, i + 1);
                if ((uint)(next + rel) == functionVa)
                    callers.Add(FunctionStart(FileToVa(i)));
            }

            return callers.Distinct().ToList();
        }

        public int? GuessIntConstantNear(uint stringVa)
        {
            foreach (var site in FindImmediateXrefs(stringVa))
            {
                var off = VaToFile(site);
                if (off < 16)
                    continue;
                for (var i = off - 12; i < off + 16 && i + 5 < _bytes.Length; i++)
                {
                    if (_bytes[i] == 0x6A && _bytes[i + 1] <= 16)
                        return _bytes[i + 1];
                    if (_bytes[i] == 0x68)
                    {
                        var imm = BitConverter.ToInt32(_bytes, i + 1);
                        if (imm is >= 0 and <= 16)
                            return imm;
                    }
                }
            }

            return null;
        }

        private uint FileToVa(int fileOffset)
        {
            foreach (var section in _sections)
            {
                if (fileOffset >= section.Raw && fileOffset < section.Raw + section.RawSize)
                    return ImageBase + section.Va + (uint)(fileOffset - section.Raw);
            }

            return 0;
        }

        private int VaToFile(uint va)
        {
            var rva = va - ImageBase;
            foreach (var section in _sections)
            {
                if (rva >= section.Va && rva < section.Va + Math.Max(section.VirtualSize, section.RawSize))
                    return (int)(section.Raw + (rva - section.Va));
            }

            return -1;
        }

        private sealed record Section(string Name, uint Va, uint VirtualSize, uint Raw, uint RawSize);
    }
}
