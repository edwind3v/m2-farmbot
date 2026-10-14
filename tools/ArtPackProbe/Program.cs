using System.Text;

var exe = File.ReadAllBytes(@"E:\Arthion\game\Arthion.exe");
var pack = File.ReadAllBytes(@"E:\Arthion\game\pack\bin.art");
var off = (int)BitConverter.ToUInt32(pack, 6 + 11);
var c0 = BitConverter.ToUInt32(pack, off);
var c1 = BitConverter.ToUInt32(pack, off + 4);
Console.WriteLine($"cipher {c0:X8} {c1:X8}");

var pe = new PeMap(exe);
var ranges = pe.Sections.Where(s => s.Name is ".rdata" or ".data" or ".text").ToList();
var hits = 0;
foreach (var s in ranges)
{
    var start = (int)s.Raw;
    var end = start + (int)Math.Min(s.RSize, s.VSize);
    end = Math.Min(end, exe.Length - 16);
    for (var i = start; i + 16 <= end; i += 4)
    {
        var key = new uint[]
        {
            BitConverter.ToUInt32(exe, i),
            BitConverter.ToUInt32(exe, i + 4),
            BitConverter.ToUInt32(exe, i + 8),
            BitConverter.ToUInt32(exe, i + 12)
        };
        var p = TeaDecrypt(c0, c1, key);
        if (p.y != 0x5A4F434D && p.y != 0x5A4C4943)
            continue;
        hits++;
        Console.WriteLine($"HIT {s.Name}+0x{i - start:X} file=0x{i:X} rva=0x{pe.FileToRva(i):X} key={key[0]:X8} {key[1]:X8} {key[2]:X8} {key[3]:X8} -> {p.y:X8} {p.z:X8}");
        if (hits >= 8)
            break;
    }
    if (hits >= 8)
        break;
}
Console.WriteLine($"done hits={hits}");

static (uint y, uint z) TeaDecrypt(uint y, uint z, uint[] key)
{
    const uint delta = 0x9E3779B9;
    var sum = unchecked(delta * 32);
    for (var n = 0; n < 32; n++)
    {
        z -= ((y << 4 ^ y >> 5) + y) ^ (sum + key[sum >> 11 & 3]);
        sum -= delta;
        y -= ((z << 4 ^ z >> 5) + z) ^ (sum + key[sum & 3]);
    }
    return (y, z);
}

sealed class PeMap
{
    public List<Sect> Sections { get; } = [];
    public PeMap(byte[] pe)
    {
        var e = BitConverter.ToInt32(pe, 0x3C);
        var n = BitConverter.ToUInt16(pe, e + 6);
        var opt = BitConverter.ToUInt16(pe, e + 20);
        var s = e + 24 + opt;
        for (var i = 0; i < n; i++)
        {
            var at = s + i * 40;
            Sections.Add(new Sect(
                Encoding.ASCII.GetString(pe, at, 8).TrimEnd('\0'),
                BitConverter.ToUInt32(pe, at + 8),
                BitConverter.ToUInt32(pe, at + 12),
                BitConverter.ToUInt32(pe, at + 16),
                BitConverter.ToUInt32(pe, at + 20)));
        }
    }
    public uint FileToRva(int file)
    {
        foreach (var s in Sections)
            if (file >= s.Raw && file < s.Raw + Math.Max(s.RSize, 1))
                return s.Va + (uint)(file - s.Raw);
        return (uint)file;
    }
}

readonly record struct Sect(string Name, uint VSize, uint Va, uint RSize, uint Raw);
