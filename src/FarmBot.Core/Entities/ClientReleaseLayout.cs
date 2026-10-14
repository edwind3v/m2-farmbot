using System.Text;

namespace FarmBot.Core.Entities;

public sealed record ReleaseMethod(string Name, uint NameRva, uint FunctionRva);

public sealed record ReleaseLayout(
    string Path,
    uint PreferredImageBase,
    uint CreateInstanceRva,
    uint MapDisplacement,
    uint SingletonRva,
    uint MainPointerDisplacement,
    uint GraphicDisplacement,
    uint RaceDisplacement,
    uint VirtualNumberDisplacement,
    uint ActorTypeDisplacement,
    uint SelfVidDisplacement,
    uint PixelXDisplacement,
    uint PixelYDisplacement,
    uint PixelZDisplacement,
    bool NegatePixelY,
    uint ProbeStringRva,
    uint MapHeadDisplacement,
    uint MapSizeDisplacement,
    uint NodeLeft,
    uint NodeParent,
    uint NodeRight,
    uint NodeIsNil,
    uint NodeKey,
    uint NodeValue,
    uint InstanceNameDisplacement,
    IReadOnlyList<ReleaseMethod> Methods,
    IReadOnlyList<string> Evidence)
{
    /// <summary>
    /// Actor-type and pixel displacements are relative to the embedded graphic object
    /// (CInstanceBase + GraphicDisplacement). Race, VNUM, and self-VID are relative to CInstanceBase.
    /// </summary>
    public bool CanEnumerate =>
        SingletonRva != 0 &&
        MapDisplacement != 0 &&
        GraphicDisplacement != 0 &&
        RaceDisplacement != 0 &&
        VirtualNumberDisplacement != 0 &&
        ActorTypeDisplacement != 0 &&
        PixelXDisplacement != 0 &&
        PixelYDisplacement != 0 &&
        PixelZDisplacement != 0 &&
        NodeKey != 0 &&
        NodeValue != 0;
}

public static class ClientReleaseLayout
{
    public static ReleaseLayout Recover(string path)
    {
        var pe = MappedPe.Load(path);
        var evidence = new List<string>();
        var methods = RecoverMethods(pe, evidence);

        var create = FindStringFunction(pe, "CPythonCharacterManager::CreateInstance VID[%d] Race[%d]");
        evidence.Add(create == 0
            ? "CreateInstance log string has no .text function start."
            : $"CreateInstance log function RVA 0x{create:X}.");

        var mapDisp = FindMapDisplacement(pe, create, evidence);
        var singleton = FindSingletonRva(pe, methods, evidence);
        var mainDisp = FindMainPointerDisplacement(pe, methods, evidence);

        uint graphic = 0, race = 0, vnum = 0, actorType = 0, selfVid = 0, px = 0, py = 0, pz = 0, instanceName = 0;
        var negateY = false;
        RecoverInstanceFields(pe, methods, evidence, ref graphic, ref race, ref vnum, ref actorType, ref selfVid, ref px, ref py, ref pz, ref negateY, ref instanceName);

        uint nodeLeft = 0, nodeParent = 4, nodeRight = 8, nodeIsNil = 0, nodeKey = 0, nodeValue = 0;
        RecoverMapNode(pe, create, evidence, ref nodeLeft, ref nodeParent, ref nodeRight, ref nodeIsNil, ref nodeKey, ref nodeValue);

        var probe = 0u;
        if (pe.TryFindAscii("CPythonCharacterManager::CreateInstance VID[%d] Race[%d]", out var probeVa))
            probe = probeVa - pe.ImageBase;

        return new ReleaseLayout(
            path,
            pe.ImageBase,
            create,
            mapDisp,
            singleton,
            mainDisp,
            graphic,
            race,
            vnum,
            actorType,
            selfVid,
            px,
            py,
            pz,
            negateY,
            probe,
            MapHeadDisplacement: 0,
            MapSizeDisplacement: 4,
            nodeLeft,
            nodeParent,
            nodeRight,
            nodeIsNil,
            nodeKey,
            nodeValue,
            instanceName,
            methods,
            evidence);
    }

    private static List<ReleaseMethod> RecoverMethods(MappedPe pe, List<string> evidence)
    {
        var names = new[]
        {
            "GetPixelPosition", "GetInstanceType", "GetRace", "GetVirtualNumber",
            "HasInstance", "GetMainCharacterPosition", "GetMainVID", "GetVID",
            "GetNameByVID"
        };
        var list = new List<ReleaseMethod>();
        foreach (var name in names)
        {
            if (!pe.TryFindAscii(name, out var va))
            {
                evidence.Add($"method name '{name}' missing.");
                continue;
            }

            foreach (var site in pe.FindImmediateXrefs(va))
            {
                var off = pe.VaToFile(site);
                if (off < 8)
                    continue;
                var flags = pe.ReadU32(off - 8);
                var fn = pe.ReadU32(off + 4);
                if (flags != 1 || fn < pe.ImageBase)
                    continue;
                var rva = fn - pe.ImageBase;
                list.Add(new ReleaseMethod(name, va - pe.ImageBase, rva));
                evidence.Add($"Python method {name} -> fn RVA 0x{rva:X}.");
                break;
            }
        }

        return list;
    }

    private static uint FindStringFunction(MappedPe pe, string text)
    {
        if (!pe.TryFindAscii(text, out var va))
            return 0;
        var sites = pe.FindImmediateXrefs(va);
        return sites.Count == 0 ? 0 : pe.FunctionStart(sites[0]) - pe.ImageBase;
    }

    private static uint FindMapDisplacement(MappedPe pe, uint createRva, List<string> evidence)
    {
        if (createRva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + createRva);
        if (off < 0)
            return 0;

        // RegisterInstance / GetInstancePtr compare _Isnil at node+0x0D, then key at +0x10.
        // The map object is the lea [reg+disp8] immediately before that check.
        for (var i = off; i < off + 0x900 && i + 4 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0x80 || pe.Byte(i + 2) != 0x0D || pe.Byte(i + 3) != 0x00)
                continue;
            var isNilMod = pe.Byte(i + 1);
            if ((isNilMod & 0xC0) != 0x40)
                continue;

            for (var j = i; j >= off && i - j < 0x50; j--)
            {
                if (pe.Byte(j) != 0x8D)
                    continue;
                var modrm = pe.Byte(j + 1);
                if ((modrm & 0xC0) != 0x40)
                    continue;
                var disp = pe.Byte(j + 2);
                if (disp is 0 or > 0x40)
                    continue;
                evidence.Add($"Alive-instance map at this+0x{disp:X} (lea before _Isnil check at RVA 0x{pe.FileToRva(i):X}).");
                return disp;
            }
        }

        evidence.Add("No lea-before-_Isnil map displacement found near CreateInstance.");
        return 0;
    }

    private static uint FindSingletonRva(MappedPe pe, IReadOnlyList<ReleaseMethod> methods, List<string> evidence)
    {
        foreach (var name in new[] { "HasInstance", "GetPixelPosition", "GetMainVID" })
        {
            var method = methods.FirstOrDefault(item => item.Name == name);
            if (method == null)
                continue;
            var hits = CollectGlobalLoads(pe, pe.ImageBase + method.FunctionRva, 0x120);
            foreach (var addr in hits)
            {
                var rva = addr - pe.ImageBase;
                if (rva < 0x1000)
                    continue;
                evidence.Add($"{name} loads [0x{addr:X}] (RVA 0x{rva:X}) as a candidate singleton.");
                return rva;
            }
        }

        evidence.Add("No singleton pointer load recovered from Python wrappers.");
        return 0;
    }

    private static uint FindMainPointerDisplacement(MappedPe pe, IReadOnlyList<ReleaseMethod> methods, List<string> evidence)
    {
        var method = methods.FirstOrDefault(item => item.Name == "GetMainVID");
        if (method == null)
            return 0;
        var native = FirstRelativeCall(pe, method.FunctionRva, 0x40);
        if (native == 0)
            return 0;
        var body = SkipInt3(pe, native);
        var off = pe.VaToFile(pe.ImageBase + body);
        if (off < 0)
            return 0;
        if (pe.Byte(off) == 0x8B && pe.Byte(off + 1) == 0x41 && pe.Byte(off + 3) == 0xC3)
        {
            var disp = pe.Byte(off + 2);
            evidence.Add($"GetMainInstancePtr is mov eax,[ecx+0x{disp:X}]; ret at RVA 0x{body:X}.");
            return disp;
        }

        evidence.Add($"GetMainInstancePtr at RVA 0x{body:X} is not a one-instruction [ecx+disp8] getter.");
        return 0;
    }

    private static void RecoverInstanceFields(
        MappedPe pe,
        IReadOnlyList<ReleaseMethod> methods,
        List<string> evidence,
        ref uint graphic,
        ref uint race,
        ref uint vnum,
        ref uint actorType,
        ref uint selfVid,
        ref uint px,
        ref uint py,
        ref uint pz,
        ref bool negateY,
        ref uint instanceName)
    {
        var typeWrap = FollowAfterGetInstancePtr(pe, methods, "GetInstanceType", evidence);
        var raceWrap = FollowAfterGetInstancePtr(pe, methods, "GetRace", evidence);
        var vnumWrap = FollowAfterGetInstancePtr(pe, methods, "GetVirtualNumber", evidence);
        var posWrap = FollowAfterGetInstancePtr(pe, methods, "GetPixelPosition", evidence);

        var typeFn = SkipInt3(pe, typeWrap);
        var raceFn = SkipInt3(pe, raceWrap);
        var vnumFn = SkipInt3(pe, vnumWrap);
        var posFn = SkipInt3(pe, posWrap);

        graphic = FirstAddEcX(pe, typeFn != 0 ? typeFn : posFn, evidence, "embedded CActorInstance");
        race = FirstLoadFromEcx(pe, raceFn, evidence, "m_dwRace");
        vnum = FirstLoadFromEcx(pe, vnumFn, evidence, "m_dwVirtualNumber");
        selfVid = FindSiblingDwordGetter(pe, raceFn, race, evidence);

        actorType = FollowGraphicMember(pe, typeFn, graphic, evidence, "m_eActorType");
        RecoverPixelFields(pe, posFn, graphic, evidence, ref px, ref py, ref pz, ref negateY);
        instanceName = RecoverInstanceName(pe, methods, evidence);
    }

    private static uint FollowAfterGetInstancePtr(MappedPe pe, IReadOnlyList<ReleaseMethod> methods, string name, List<string> evidence)
    {
        var method = methods.FirstOrDefault(item => item.Name == name);
        if (method == null)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + method.FunctionRva);
        if (off < 0)
            return 0;

        var afterVtbl = -1;
        for (var i = off; i < off + 0x80 && i + 3 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xFF && pe.Byte(i + 1) == 0x50 && pe.Byte(i + 2) == 0x08)
            {
                afterVtbl = i + 3;
                break;
            }
        }

        uint call = 0;
        if (afterVtbl >= 0)
        {
            call = FirstRelativeCallAtFile(pe, afterVtbl, 0x40);
        }
        else
        {
            for (var i = off; i < off + 0x40 && i + 6 < pe.Length; i++)
            {
                if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0xC8 && pe.Byte(i + 2) == 0xE8)
                {
                    call = (uint)(pe.FileToVa(i + 7) + pe.ReadI32(i + 3) - pe.ImageBase);
                    break;
                }
            }

            if (call == 0)
                call = LastSimpleGetterCall(pe, off, 0x40);
        }
        if (call != 0)
            evidence.Add($"{name} native at RVA 0x{call:X}" + (afterVtbl >= 0 ? " (after GetInstancePtr vtbl+8)." : "."));
        else
            evidence.Add($"{name} native call was not recovered.");
        return call;
    }

    private static uint FollowGraphicMember(MappedPe pe, uint rva, uint graphic, List<string> evidence, string label)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x20 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE9 && pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (target < pe.ImageBase)
                continue;
            var body = SkipInt3(pe, target - pe.ImageBase);
            var disp = FirstLoadFromEcx(pe, body, evidence, label);
            if (disp != 0)
                evidence.Add($"{label} is [CActorInstance+0x{disp:X}] (CInstanceBase+0x{graphic + disp:X}).");
            return disp;
        }

        return FirstLoadFromEcx(pe, rva, evidence, label);
    }

    private static void RecoverPixelFields(
        MappedPe pe,
        uint rva,
        uint graphic,
        List<string> evidence,
        ref uint px,
        ref uint py,
        ref uint pz,
        ref bool negateY)
    {
        if (rva == 0)
            return;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return;
        uint impl = 0;
        for (var i = off; i < off + 0x20 && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (target < pe.ImageBase)
                continue;
            impl = SkipInt3(pe, target - pe.ImageBase);
            break;
        }

        if (impl == 0)
        {
            evidence.Add("NEW_GetPixelPosition did not call a position implementation.");
            return;
        }

        var implOff = pe.VaToFile(pe.ImageBase + impl);
        if (implOff < 0)
            return;
        var loads = new List<uint>();
        for (var i = implOff; i < implOff + 0x40 && i + 8 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x0F && pe.Byte(i + 1) == 0x57)
                negateY = true;
            if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x81 || pe.Byte(i + 1) == 0x91))
                loads.Add(pe.ReadU32(i + 2));
            if (pe.Byte(i) == 0xF3 && pe.Byte(i + 1) == 0x0F && pe.Byte(i + 2) == 0x10 &&
                (pe.Byte(i + 3) == 0x81 || pe.Byte(i + 3) == 0x91))
                loads.Add(pe.ReadU32(i + 4));
        }

        if (loads.Count < 3)
        {
            evidence.Add($"Position impl RVA 0x{impl:X} did not yield three float displacements.");
            return;
        }

        px = loads[0];
        py = loads[1];
        pz = loads[2];
        evidence.Add($"NEW_GetCurPixelPositionRef reads m_x/m_y/m_z at graphic+0x{px:X}/+0x{py:X}/+0x{pz:X} (CInstanceBase+0x{graphic + px:X}). Y is {(negateY ? "negated" : "copied")}.");
    }

    private static uint SkipInt3(MappedPe pe, uint rva)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return rva;
        var i = off;
        while (i < off + 8 && pe.Byte(i) == 0xCC)
            i++;
        return pe.FileToRva(i);
    }

    private static uint FirstRelativeCall(MappedPe pe, uint rva, int window)
    {
        var off = pe.VaToFile(pe.ImageBase + rva);
        return off < 0 ? 0 : FirstRelativeCallAtFile(pe, off, window);
    }

    private static uint FirstRelativeCallAtFile(MappedPe pe, int off, int window)
    {
        for (var i = off; i < off + window && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (target < pe.ImageBase)
                continue;
            if (IsImportThunk(pe, target))
                continue;
            return target - pe.ImageBase;
        }

        return 0;
    }

    private static uint LastSimpleGetterCall(MappedPe pe, int off, int window)
    {
        uint last = 0;
        for (var i = off; i < off + window && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) != 0xE8)
                continue;
            var target = (uint)(pe.FileToVa(i + 5) + pe.ReadI32(i + 1));
            if (target < pe.ImageBase || IsImportThunk(pe, target))
                continue;
            var body = SkipInt3(pe, target - pe.ImageBase);
            var bodyOff = pe.VaToFile(pe.ImageBase + body);
            if (bodyOff < 0)
                continue;
            if (pe.Byte(bodyOff) == 0x8B && (pe.Byte(bodyOff + 1) == 0x81 || pe.Byte(bodyOff + 1) == 0x41))
                last = body;
        }

        return last;
    }

    private static bool IsImportThunk(MappedPe pe, uint va)
    {
        var off = pe.VaToFile(va);
        return off >= 0 && pe.Byte(off) == 0xFF && pe.Byte(off + 1) == 0x25;
    }

    private static uint FindSiblingDwordGetter(MappedPe pe, uint raceRva, uint raceDisp, List<string> evidence)
    {
        if (raceRva == 0 || raceDisp == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + raceRva);
        if (off < 0)
            return 0;
        for (var i = off - 16; i < off + 24 && i + 6 < pe.Length; i++)
        {
            if (i < 0)
                continue;
            if (pe.Byte(i) != 0x8B || pe.Byte(i + 1) != 0x81 || pe.Byte(i + 6) != 0xC3)
                continue;
            var disp = pe.ReadU32(i + 2);
            if (disp == raceDisp || disp is 0 or > 0x400)
                continue;
            evidence.Add($"m_dwVID candidate mov eax,[ecx+0x{disp:X}] next to GetRace.");
            return disp;
        }

        return 0;
    }

    private static uint RecoverInstanceName(MappedPe pe, IReadOnlyList<ReleaseMethod> methods, List<string> evidence)
    {
        var method = methods.FirstOrDefault(item => item.Name == "GetNameByVID");
        if (method == null)
        {
            evidence.Add("GetNameByVID Python method was not recovered.");
            return 0;
        }

        var off = pe.VaToFile(pe.ImageBase + method.FunctionRva);
        if (off < 0)
            return 0;
        var reserved = new HashSet<uint> { 0x174, 0x178, 0x17C, 0x1EC };
        for (var i = off; i < off + 0xC0 && i + 6 < pe.Length; i++)
        {
            var callOff = i;
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0xC8 && pe.Byte(i + 2) == 0xE8)
                callOff = i + 2;
            else if (pe.Byte(i) != 0xE8 && pe.Byte(i) != 0xE9)
                continue;
            var target = (uint)(pe.FileToVa(callOff + 5) + pe.ReadI32(callOff + 1));
            if (target < pe.ImageBase)
                continue;
            var body = FollowJmp(pe, SkipInt3(pe, target - pe.ImageBase));
            var disp = StdStringDisplacement(pe, body);
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0xC8)
                evidence.Add($"GetNameString RVA 0x{body:X} bytes={pe.Hex(pe.ImageBase + body, 24)} disp=0x{disp:X}.");
            if (disp == 0 || reserved.Contains(disp) || disp is < 0x10 or > 0x8000)
                continue;
            evidence.Add($"CInstanceBase::m_stName at +0x{disp:X} from GetNameByVID call RVA 0x{body:X}.");
            return disp;
        }

        evidence.Add("CInstanceBase::m_stName displacement was not recovered; minimap tooltips will use VNUM.");
        return 0;
    }

    private static uint FollowJmp(MappedPe pe, uint rva)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return rva;
        if (pe.Byte(off) == 0xE9)
        {
            var dest = (uint)(pe.FileToVa(off + 5) + pe.ReadI32(off + 1));
            if (dest > pe.ImageBase)
                return SkipInt3(pe, dest - pe.ImageBase);
        }

        return rva;
    }

    private static uint StdStringDisplacement(MappedPe pe, uint rva)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x30 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8D && (pe.Byte(i + 1) == 0x81 || pe.Byte(i + 1) == 0x89))
                return pe.ReadU32(i + 2);
            if (pe.Byte(i) == 0x8D && (pe.Byte(i + 1) == 0x41 || pe.Byte(i + 1) == 0x49))
                return pe.Byte(i + 2);
            if (pe.Byte(i) == 0x81 && pe.Byte(i + 1) == 0xC1)
                return pe.ReadU32(i + 2);
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
                return pe.Byte(i + 2);
            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xB9 && pe.Byte(i + 6) == 0x10)
                return pe.ReadU32(i + 2) - 16;
        }

        return 0;
    }

    private static uint FirstAddEcX(MappedPe pe, uint rva, List<string> evidence, string label)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x40 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x81 && pe.Byte(i + 1) == 0xC1)
            {
                var disp = pe.ReadU32(i + 2);
                evidence.Add($"{label}: add ecx, 0x{disp:X} at RVA 0x{rva:X}.");
                return disp;
            }

            if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x81)
            {
                var disp = pe.ReadU32(i + 2);
                evidence.Add($"{label}: lea ecx,[ecx+0x{disp:X}] at RVA 0x{rva:X}.");
                return disp;
            }

            if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x89)
            {
                var disp = pe.ReadU32(i + 2);
                evidence.Add($"{label}: lea ecx,[ecx+0x{disp:X}] at RVA 0x{rva:X}.");
                return disp;
            }

            if (pe.Byte(i) == 0x83 && pe.Byte(i + 1) == 0xC1)
            {
                var disp = pe.Byte(i + 2);
                evidence.Add($"{label}: add ecx, 0x{disp:X} at RVA 0x{rva:X}.");
                return disp;
            }

            if (pe.Byte(i) == 0x8D && pe.Byte(i + 1) == 0x49)
            {
                var disp = pe.Byte(i + 2);
                evidence.Add($"{label}: lea ecx,[ecx+0x{disp:X}] at RVA 0x{rva:X}.");
                return disp;
            }
        }

        return 0;
    }

    private static uint FirstLoadFromEcx(MappedPe pe, uint rva, List<string> evidence, string label)
    {
        if (rva == 0)
            return 0;
        var off = pe.VaToFile(pe.ImageBase + rva);
        if (off < 0)
            return 0;
        for (var i = off; i < off + 0x50 && i + 6 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x81)
            {
                var disp = pe.ReadU32(i + 2);
                evidence.Add($"{label}: mov eax,[ecx+0x{disp:X}].");
                return disp;
            }

            if (pe.Byte(i) == 0x8B && pe.Byte(i + 1) == 0x41)
            {
                var disp = pe.Byte(i + 2);
                evidence.Add($"{label}: mov eax,[ecx+0x{disp:X}].");
                return disp;
            }

            if (pe.Byte(i) == 0x0F && pe.Byte(i + 1) == 0xB6 && pe.Byte(i + 2) == 0x41)
            {
                var disp = pe.Byte(i + 3);
                evidence.Add($"{label}: movzx eax,byte [ecx+0x{disp:X}].");
                return disp;
            }
        }

        return 0;
    }

    private static void RecoverMapNode(
        MappedPe pe,
        uint createRva,
        List<string> evidence,
        ref uint left,
        ref uint parent,
        ref uint right,
        ref uint isNil,
        ref uint key,
        ref uint value)
    {
        if (createRva == 0)
        {
            evidence.Add("Map node layout not recovered from CreateInstance.");
            return;
        }

        var off = pe.VaToFile(pe.ImageBase + createRva);
        if (off < 0)
            return;

        for (var i = off; i < off + 0x900 && i + 4 < pe.Length; i++)
        {
            // cmp byte [r32+0x0D], 0  then later cmp/mov of [r32+0x10]
            if (pe.Byte(i) != 0x80 || pe.Byte(i + 2) != 0x0D || pe.Byte(i + 3) != 0x00)
                continue;
            if ((pe.Byte(i + 1) & 0xC0) != 0x40)
                continue;

            for (var j = i; j < i + 0x20 && j + 3 < pe.Length; j++)
            {
                if (pe.Byte(j) == 0x3B && (pe.Byte(j + 1) & 0xC0) == 0x40 && pe.Byte(j + 2) == 0x10)
                    key = 0x10;
                if (pe.Byte(j) == 0x8B && (pe.Byte(j + 1) & 0xC0) == 0x40 && pe.Byte(j + 2) == 0x14)
                    value = 0x14;
            }

            if (key != 0 && value != 0)
            {
                isNil = 0x0D;
                left = 0;
                parent = 4;
                right = 8;
                evidence.Add("std::map node: _Isnil+0x0D, VID key+0x10, CInstanceBase*+0x14; links +0/+4/+8 from RegisterInstance/GetInstancePtr.");
                return;
            }
        }

        evidence.Add("Map node key/value were not recovered from an _Isnil check.");
    }

    private static List<uint> CollectGlobalLoads(MappedPe pe, uint va, int window)
    {
        var hits = new List<uint>();
        var off = pe.VaToFile(va);
        if (off < 0)
            return hits;
        for (var i = off; i < off + window && i + 5 < pe.Length; i++)
        {
            if (pe.Byte(i) == 0xA1)
                hits.Add(pe.ReadU32(i + 1));
            if (pe.Byte(i) == 0x8B && (pe.Byte(i + 1) == 0x0D || pe.Byte(i + 1) == 0x15 || pe.Byte(i + 1) == 0x35))
                hits.Add(pe.ReadU32(i + 2));
        }

        return hits.Where(item => item > pe.ImageBase).Distinct().ToList();
    }

    private sealed class MappedPe
    {
        private readonly byte[] _bytes;
        private readonly List<Section> _sections = new();
        public uint ImageBase { get; private set; }
        public int Length => _bytes.Length;

        public static MappedPe Load(string path) => new(path);

        private MappedPe(string path)
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
            foreach (var section in _sections)
            {
                var start = (int)section.Raw;
                var end = Math.Min(_bytes.Length - 4, start + (int)section.RawSize);
                for (var i = start; i < end; i++)
                {
                    if (_bytes[i] == needle[0] && _bytes[i + 1] == needle[1] && _bytes[i + 2] == needle[2] && _bytes[i + 3] == needle[3])
                        hits.Add(FileToVa(i));
                }
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

        public string Hex(uint va, int count)
        {
            var off = VaToFile(va);
            if (off < 0)
                return "";
            var n = Math.Min(count, Length - off);
            return Convert.ToHexString(_bytes, off, n);
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

        public uint FileToRva(int fileOffset) => FileToVa(fileOffset) - ImageBase;

        public int VaToFile(uint va)
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
