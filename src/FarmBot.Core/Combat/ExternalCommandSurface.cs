using System.Text;

namespace FarmBot.Core.Combat;

public sealed record ExternalCommandFinding(string Kind, string Name, bool Present);

public sealed record ExternalCommandReport(
    string Path,
    IReadOnlyList<string> Exports,
    IReadOnlyList<string> RelevantImports,
    IReadOnlyList<ExternalCommandFinding> Findings,
    bool HasExternalCommandInterface)
{
    public string Summary => HasExternalCommandInterface
        ? "A supported external command interface was found in the release image."
        : "No supported external command interface exists in the release image. CPythonPlayer methods are in-process only.";
}

public static class ExternalCommandSurface
{
    public const string NoExternalInterface =
        "no external command interface: SetTarget/NEW_Attack/SetAttackKeyState are in-process C++/Python only; the release image exports nothing and does not host a pipe, COM, or WM_COPYDATA command server";

    public const string RemainingGameThreadDrain =
        "minimum remaining integration: an x86 module already on the client window thread (CPythonApplication::Loop/Process, CPythonPlayer::Update) must drain the authenticated FarmBotAction queue and thiscall SetTarget; the IPC worker thread is not that thread; the stock image has no drain, export, or hook point";

    private static readonly string[] InterfaceStrings =
    {
        "CreateNamedPipe", "CreateNamedPipeA", "CreateNamedPipeW",
        @"\\.\pipe\", "\\\\.\\pipe\\",
        "WM_COPYDATA", "RegisterWindowMessage", "RegisterWindowMessageA",
        "CoRegisterClassObject", "CoCreateInstance", "IDispatch", "IClassFactory",
        "CreateMailslot", "CreateFileMapping", "OpenFileMapping",
        "FarmBot", "FarmBotBridge", "M2FarmBot",
        "PyRun_SimpleString", "PyRun_String", "PyEval_EvalCode",
        "command interface", "ExecConsole", "RunConsole"
    };

    private static readonly string[] RelevantImportNames =
    {
        "CreateNamedPipeA", "CreateNamedPipeW",
        "RegisterWindowMessageA", "RegisterWindowMessageW",
        "CoRegisterClassObject", "CoRegisterPSClsid",
        "CreateMailslotA", "CreateMailslotW",
        "OpenFileMappingA", "OpenFileMappingW"
    };

    public static ExternalCommandReport Inspect(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var exports = ReadExports(bytes);
        var imports = ReadImports(bytes);
        var findings = new List<ExternalCommandFinding>();

        foreach (var name in InterfaceStrings)
        {
            findings.Add(new ExternalCommandFinding("string", name, ContainsAscii(bytes, name)));
        }

        foreach (var name in RelevantImportNames)
        {
            findings.Add(new ExternalCommandFinding("import", name, imports.Contains(name)));
        }

        findings.Add(new ExternalCommandFinding("export-table", "any export", exports.Count > 0));
        findings.Add(new ExternalCommandFinding("python-method", "SetTarget", ContainsAscii(bytes, "SetTarget")));
        findings.Add(new ExternalCommandFinding("python-method", "SetAttackKeyState", ContainsAscii(bytes, "SetAttackKeyState")));
        findings.Add(new ExternalCommandFinding("python-method", "ComboAttack", ContainsAscii(bytes, "ComboAttack")));
        findings.Add(new ExternalCommandFinding("python-method", "NEW_Attack", ContainsAscii(bytes, "NEW_Attack")));

        var hasInterface = exports.Count > 0
            || findings.Any(item => item.Kind == "import" && item.Present)
            || findings.Any(item => item.Kind == "string" && item.Present &&
                item.Name is "CreateNamedPipeA" or "CreateNamedPipeW" or @"\\.\pipe\" or "WM_COPYDATA"
                    or "CoRegisterClassObject" or "FarmBot" or "FarmBotBridge" or "M2FarmBot");

        return new ExternalCommandReport(path, exports, imports.Where(RelevantImportNames.Contains).ToList(), findings, hasInterface);
    }

    private static bool ContainsAscii(byte[] bytes, string text)
    {
        var needle = Encoding.ASCII.GetBytes(text);
        for (var i = 0; i + needle.Length < bytes.Length; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (bytes[i + j] != needle[j])
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

    private static List<string> ReadExports(byte[] bytes)
    {
        var names = new List<string>();
        if (!TryPe(bytes, out var eLfanew, out var optional, out _, out var sections))
            return names;
        var magic = BitConverter.ToUInt16(bytes, eLfanew + 24);
        var exportRvaOff = magic == 0x20B ? eLfanew + 24 + 112 : eLfanew + 24 + 96;
        if (exportRvaOff + 8 > bytes.Length)
            return names;
        var exportRva = BitConverter.ToUInt32(bytes, exportRvaOff);
        if (exportRva == 0)
            return names;
        var file = RvaToFile(exportRva, sections);
        if (file < 0 || file + 40 > bytes.Length)
            return names;
        var namesRva = BitConverter.ToUInt32(bytes, file + 32);
        var count = BitConverter.ToInt32(bytes, file + 24);
        var namesFile = RvaToFile(namesRva, sections);
        if (namesFile < 0)
            return names;
        for (var i = 0; i < count && namesFile + i * 4 + 4 <= bytes.Length; i++)
        {
            var nameRva = BitConverter.ToUInt32(bytes, namesFile + i * 4);
            var nameFile = RvaToFile(nameRva, sections);
            if (nameFile < 0)
                continue;
            names.Add(ReadAsciiZ(bytes, nameFile));
        }

        return names;
    }

    private static List<string> ReadImports(byte[] bytes)
    {
        var names = new List<string>();
        if (!TryPe(bytes, out var eLfanew, out _, out _, out var sections))
            return names;
        var magic = BitConverter.ToUInt16(bytes, eLfanew + 24);
        var importRvaOff = magic == 0x20B ? eLfanew + 24 + 120 : eLfanew + 24 + 104;
        if (importRvaOff + 8 > bytes.Length)
            return names;
        var importRva = BitConverter.ToUInt32(bytes, importRvaOff);
        if (importRva == 0)
            return names;
        var file = RvaToFile(importRva, sections);
        if (file < 0)
            return names;
        for (var desc = file; desc + 20 <= bytes.Length; desc += 20)
        {
            var nameRva = BitConverter.ToUInt32(bytes, desc + 12);
            var firstThunk = BitConverter.ToUInt32(bytes, desc);
            if (nameRva == 0 && firstThunk == 0)
                break;
            var lookup = firstThunk != 0 ? firstThunk : BitConverter.ToUInt32(bytes, desc + 16);
            var lookFile = RvaToFile(lookup, sections);
            if (lookFile < 0)
                continue;
            for (var thunk = lookFile; thunk + 4 <= bytes.Length; thunk += 4)
            {
                var value = BitConverter.ToUInt32(bytes, thunk);
                if (value == 0)
                    break;
                if ((value & 0x80000000) != 0)
                    continue;
                var hintFile = RvaToFile(value, sections);
                if (hintFile < 0)
                    continue;
                names.Add(ReadAsciiZ(bytes, hintFile + 2));
            }
        }

        return names.Distinct().ToList();
    }

    private static bool TryPe(byte[] bytes, out int eLfanew, out int optional, out int sectionCount, out List<(uint Va, uint Raw, uint RawSize)> sections)
    {
        eLfanew = 0;
        optional = 0;
        sectionCount = 0;
        sections = new List<(uint, uint, uint)>();
        if (bytes.Length < 64 || bytes[0] != (byte)'M' || bytes[1] != (byte)'Z')
            return false;
        eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        if (eLfanew + 24 >= bytes.Length)
            return false;
        sectionCount = BitConverter.ToUInt16(bytes, eLfanew + 6);
        optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        for (var i = 0; i < sectionCount; i++)
        {
            var at = off + i * 40;
            if (at + 40 > bytes.Length)
                break;
            sections.Add((
                BitConverter.ToUInt32(bytes, at + 12),
                BitConverter.ToUInt32(bytes, at + 20),
                BitConverter.ToUInt32(bytes, at + 16)));
        }

        return true;
    }

    private static int RvaToFile(uint rva, List<(uint Va, uint Raw, uint RawSize)> sections)
    {
        foreach (var section in sections)
        {
            if (rva >= section.Va && rva < section.Va + Math.Max(section.RawSize, 1))
                return (int)(section.Raw + (rva - section.Va));
        }

        return -1;
    }

    private static string ReadAsciiZ(byte[] bytes, int offset)
    {
        var end = offset;
        while (end < bytes.Length && bytes[end] != 0)
            end++;
        return Encoding.ASCII.GetString(bytes, offset, end - offset);
    }
}
