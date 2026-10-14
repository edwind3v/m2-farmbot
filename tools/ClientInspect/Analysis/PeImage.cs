using System.Text;

namespace ClientInspect.Analysis;

public static class PeImage
{
    public static PeSummary Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 64 || bytes[0] != (byte)'M' || bytes[1] != (byte)'Z')
            throw new InvalidDataException("not a PE image");

        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        if (eLfanew <= 0 || eLfanew + 24 + 70 >= bytes.Length)
            throw new InvalidDataException("PE header is truncated");
        if (BitConverter.ToUInt32(bytes, eLfanew) != 0x00004550)
            throw new InvalidDataException("missing PE signature");

        var machine = BitConverter.ToUInt16(bytes, eLfanew + 4);
        var sectionCount = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optionalSize = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var magic = BitConverter.ToUInt16(bytes, eLfanew + 24);
        var imageBase = magic == 0x20B
            ? BitConverter.ToUInt32(bytes, eLfanew + 24 + 24)
            : BitConverter.ToUInt32(bytes, eLfanew + 24 + 28);
        var dllChars = BitConverter.ToUInt16(bytes, eLfanew + 24 + 70);
        var sectionOff = eLfanew + 24 + optionalSize;
        var sections = new List<string>();
        for (var i = 0; i < sectionCount; i++)
        {
            var off = sectionOff + i * 40;
            if (off + 8 > bytes.Length)
                break;
            sections.Add(Encoding.ASCII.GetString(bytes, off, 8).TrimEnd('\0'));
        }

        return new PeSummary(
            Path: path,
            FileSize: bytes.Length,
            Machine: MachineName(machine),
            Is32Bit: magic == 0x10B,
            ImageBase: imageBase,
            DllCharacteristics: $"0x{dllChars:X4}",
            Aslr: (dllChars & 0x0040) != 0,
            Nx: (dllChars & 0x0100) != 0,
            Sections: sections);
    }

    public static IReadOnlyList<string> FindAscii(string path, IEnumerable<string> needles)
    {
        var text = Encoding.ASCII.GetString(File.ReadAllBytes(path));
        var hits = new List<string>();
        foreach (var needle in needles)
        {
            if (text.Contains(needle, StringComparison.Ordinal))
                hits.Add(needle);
        }

        return hits;
    }

    private static string MachineName(ushort machine) => machine switch
    {
        0x014C => "IMAGE_FILE_MACHINE_I386 (32-bit)",
        0x8664 => "IMAGE_FILE_MACHINE_AMD64 (64-bit)",
        _ => $"0x{machine:X}"
    };
}
