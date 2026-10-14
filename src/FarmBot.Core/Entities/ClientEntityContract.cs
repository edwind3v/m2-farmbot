using System.Text;

namespace FarmBot.Core.Entities;

public sealed record ClientContractFinding(string Name, bool Present, int? Value);

public sealed record ClientContractReport(
    string Path,
    bool ImageFound,
    IReadOnlyList<ClientContractFinding> Findings)
{
    public bool MatchesCompiledTypes =>
        ImageFound &&
        Findings.Any(item => item.Name == "INSTANCE_TYPE_ENEMY" && item.Value == (int)ActorType.Enemy) &&
        Findings.Any(item => item.Name == "INSTANCE_TYPE_PLAYER" && item.Value == (int)ActorType.Player);
}

public static class ClientEntityContract
{
    public static readonly string[] RequiredMarkers =
    {
        "INSTANCE_TYPE_ENEMY",
        "INSTANCE_TYPE_NPC",
        "INSTANCE_TYPE_PLAYER",
        "INSTANCE_TYPE_BUILDING",
        "INSTANCE_TYPE_OBJECT",
        "GetPixelPosition",
        "GetMainCharacterPosition",
        ".?AVCPythonCharacterManager@@",
        ".?AVCInstanceBase@@"
    };

    public static ClientContractReport ValidateImage(string path)
    {
        if (!File.Exists(path))
            return new ClientContractReport(path, false, Array.Empty<ClientContractFinding>());

        var bytes = File.ReadAllBytes(path);
        var findings = new List<ClientContractFinding>();
        foreach (var name in RequiredMarkers)
        {
            if (!TryFindAscii(bytes, name, out var fileOffset))
            {
                findings.Add(new ClientContractFinding(name, false, null));
                continue;
            }

            int? value = null;
            if (name.StartsWith("INSTANCE_TYPE_", StringComparison.Ordinal))
                value = GuessImm8NearString(bytes, fileOffset);
            findings.Add(new ClientContractFinding(name, true, value));
        }

        return new ClientContractReport(path, true, findings);
    }

    private static bool TryFindAscii(byte[] hay, string text, out int offset)
    {
        var needle = Encoding.ASCII.GetBytes(text);
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

            if (!ok)
                continue;
            offset = i;
            return true;
        }

        offset = -1;
        return false;
    }

    private static int? GuessImm8NearString(byte[] bytes, int stringFileOffset)
    {
        var va = FileToPreferredVa(bytes, stringFileOffset);
        if (va == 0)
            return null;
        var needle = BitConverter.GetBytes(va);
        var text = FindSection(bytes, ".text");
        if (text == null)
            return null;
        var start = (int)text.Value.Raw;
        var end = Math.Min(bytes.Length - 4, start + (int)text.Value.RawSize);
        for (var i = start; i < end; i++)
        {
            if (bytes[i] != needle[0] || bytes[i + 1] != needle[1] || bytes[i + 2] != needle[2] || bytes[i + 3] != needle[3])
                continue;
            for (var k = 1; k <= 16 && i - k >= 0; k++)
            {
                if (bytes[i - k] == 0x6A)
                    return bytes[i - k + 1];
            }
        }

        return null;
    }

    private static uint FileToPreferredVa(byte[] bytes, int fileOffset)
    {
        if (bytes.Length < 0x40)
            return 0;
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        if (eLfanew < 0 || eLfanew + 0x60 >= bytes.Length)
            return 0;
        var imageBase = BitConverter.ToUInt32(bytes, eLfanew + 24 + 28);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            if (at + 40 > bytes.Length)
                break;
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            var va = BitConverter.ToUInt32(bytes, at + 12);
            if (fileOffset >= raw && fileOffset < raw + rawSize)
                return imageBase + va + (uint)(fileOffset - raw);
        }

        return 0;
    }

    private static (uint Va, uint Raw, uint RawSize)? FindSection(byte[] bytes, string name)
    {
        if (bytes.Length < 0x40)
            return null;
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var sectionName = Encoding.ASCII.GetString(bytes, at, 8).TrimEnd('\0');
            if (!sectionName.StartsWith(name, StringComparison.Ordinal))
                continue;
            return (
                BitConverter.ToUInt32(bytes, at + 12),
                BitConverter.ToUInt32(bytes, at + 20),
                BitConverter.ToUInt32(bytes, at + 16));
        }

        return null;
    }
}
