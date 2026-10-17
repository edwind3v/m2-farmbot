using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using FarmBot.Core.Combat;
using FarmBot.Platform.Native;

namespace FarmBot.Platform.Client;

// Read decrypted pack filenames from the running client (the same list CEterPackManager
// already opened), then copy that map's minimap tiles and atlas.sub image from pack files.
public static class MapPackExtractor
{
    public sealed class Entry
    {
        public string Name = "";
        public long Offset, Packed, Size, Flag;
    }

    public readonly record struct Result(int Written, int Needles, int Parsed, bool Opened, string Detail)
    {
        public override string ToString() => Detail;
    }

    private static readonly byte[] XorKey = Encoding.ASCII.GetBytes("w317bfaoq07jrvmh");
    private static readonly byte[][] Needles =
    {
        Encoding.ASCII.GetBytes("minimap.dds"),
        Encoding.ASCII.GetBytes("atlas.sub"),
        Encoding.ASCII.GetBytes("_atlas.dds"),
        Encoding.ASCII.GetBytes("_atlas.tga")
    };
    private static readonly Regex TileName = new(
        @"^(?:.*[/\\])?(?<map>[^/\\]+)[/\\](?<area>\d{6})[/\\](?<file>minimap\.dds|minimap\.png|minimap\.bmp|attr\.atr)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static Result ExtractMap(int processId, string clientPath, string mapName)
    {
        if (processId <= 0 || string.IsNullOrWhiteSpace(mapName))
            return new Result(0, 0, 0, false, "no process/map");
        var root = File.Exists(clientPath) ? Path.GetDirectoryName(clientPath) ?? "" : clientPath ?? "";
        var packDir = Path.Combine(root, "pack");
        if (!Directory.Exists(packDir))
            return new Result(0, 0, 0, false, "no pack folder");
        var dest = AtlasPixels.CacheDir(mapName);
        if (dest.Length == 0)
            return new Result(0, 0, 0, false, "bad map name");
        Directory.CreateDirectory(dest);
        var aliases = new HashSet<string>(AtlasPixels.MapAliases(mapName), StringComparer.OrdinalIgnoreCase);
        var packs = Directory.GetFiles(packDir)
            .Select(p => new FileInfo(p))
            .Where(p => p.Length > 1024)
            .OrderBy(p => p.Length)
            .ToArray();
        if (packs.Length == 0)
            return new Result(0, 0, 0, false, "pack folder empty");
        var maxPak = packs.Max(p => p.Length);
        var hits = Scan(processId, maxPak, aliases, out var needles, out var opened);
        if (!opened)
            return new Result(0, 0, 0, false, "could not open client process for pack index");
        if (hits.Count == 0)
            return new Result(0, needles, 0, true, $"no pack index hits (filename needles={needles})");

        var written = 0;
        string? subText = null;
        string? subVirt = null;
        foreach (var entry in hits)
        {
            var data = ReadDecoded(packs, entry);
            if (data == null || data.Length < 12)
                continue;
            var name = entry.Name.Replace('\\', '/');
            var tile = TileName.Match(name);
            if (tile.Success && aliases.Contains(tile.Groups["map"].Value))
            {
                var dir = Path.Combine(dest, tile.Groups["area"].Value);
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, tile.Groups["file"].Value.ToLowerInvariant()), data);
                written++;
                continue;
            }

            if (name.EndsWith("atlas.sub", StringComparison.OrdinalIgnoreCase) && NameHasAlias(name, aliases))
            {
                File.WriteAllBytes(Path.Combine(dest, "atlas.sub"), data);
                subText = Encoding.ASCII.GetString(data);
                subVirt = name;
                written++;
                continue;
            }

            if ((name.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)) &&
                NameHasAlias(name, aliases))
            {
                var file = Path.GetFileName(name.Replace('/', Path.DirectorySeparatorChar));
                if (string.IsNullOrWhiteSpace(file))
                    file = AtlasPixels.SanitizeMapFolder(mapName) + "_atlas.dds";
                File.WriteAllBytes(Path.Combine(dest, file), data);
                written++;
            }
        }

        if (subText != null && TrySubImage(subText, out var image))
        {
            var joined = JoinImage(subVirt ?? "", image);
            var extra = hits.FirstOrDefault(e =>
                e.Name.Replace('\\', '/').EndsWith(image.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetFileName(e.Name.Replace('/', '\\')), Path.GetFileName(image.Replace('/', '\\')),
                    StringComparison.OrdinalIgnoreCase));
            if (extra != null)
            {
                var data = ReadDecoded(packs, extra);
                if (data != null && data.Length >= 16)
                {
                    var file = Path.GetFileName(image.Replace('/', '\\'));
                    if (string.IsNullOrWhiteSpace(file))
                        file = AtlasPixels.SanitizeMapFolder(mapName) + "_atlas.dds";
                    File.WriteAllBytes(Path.Combine(dest, file), data);
                    written++;
                }
            }
            else
            {
                _ = joined;
            }
        }

        return new Result(written, needles, hits.Count, true,
            written > 0
                ? $"unpacked {written} files from {hits.Count} pack names"
                : $"found {hits.Count} pack names but none decoded from disk");
    }

    private static bool NameHasAlias(string name, HashSet<string> aliases)
    {
        foreach (var alias in aliases)
        {
            if (name.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private static List<Entry> Scan(int pid, long maxPak, HashSet<string> aliases, out int needles, out bool opened)
    {
        needles = 0;
        opened = false;
        var found = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        var handle = IntPtr.Zero;
        foreach (var access in ProcessMemory.AccessAttempts)
        {
            handle = Win32.OpenProcess(access, false, pid);
            if (handle != IntPtr.Zero)
                break;
        }

        if (handle == IntPtr.Zero)
            return found.Values.ToList();
        opened = true;
        try
        {
            ulong addr = 0;
            while (addr < 0x80000000UL)
            {
                if (Win32.VirtualQueryEx(handle, (UIntPtr)addr, out var mbi, (UIntPtr)Marshal.SizeOf<Win32.MEMORY_BASIC_INFORMATION>()) == UIntPtr.Zero)
                    break;
                var size = mbi.RegionSize.ToUInt64();
                if (size == 0)
                    break;
                var next = mbi.BaseAddress.ToUInt64() + size;
                var typeOk = mbi.Type == Win32.MemPrivate || mbi.Type == Win32.MemMapped;
                if (mbi.State == Win32.MemCommit &&
                    typeOk &&
                    (mbi.Protect & Win32.PageGuard) == 0 &&
                    (mbi.Protect & Win32.PageWritable) != 0 &&
                    size > 256 && size <= 96UL * 1024 * 1024)
                {
                    var buf = new byte[(int)size];
                    if (Win32.ReadProcessMemory(handle, mbi.BaseAddress, buf, (UIntPtr)buf.Length, out var read) &&
                        read.ToUInt64() > 32)
                    {
                        Collect(buf, (int)read.ToUInt64(), maxPak, aliases, found, ref needles);
                    }
                }

                addr = next <= addr ? addr + 0x1000 : next;
            }
        }
        finally
        {
            Win32.CloseHandle(handle);
        }

        return found.Values.ToList();
    }

    private static void Collect(byte[] buf, int n, long maxPak, HashSet<string> aliases, Dictionary<string, Entry> found, ref int needles)
    {
        foreach (var needle in Needles)
        {
            var i = 0;
            while (i + needle.Length <= n)
            {
                var at = IndexOf(buf, i, n, needle);
                if (at < 0)
                    break;
                needles++;
                var pathStart = PathStart(buf, at);
                var path = ReadAscii(buf, pathStart, at + needle.Length - pathStart);
                i = at + needle.Length;
                if (path.Length < 12 || !NameHasAlias(path, aliases))
                    continue;
                if (!TryParseRecord(buf, pathStart, n, maxPak, path, out var entry))
                    continue;
                found.TryAdd(entry.Name, entry);
            }
        }
    }

    private static bool TryParseRecord(byte[] buf, int pathStart, int n, long maxPak, string path, out Entry entry)
    {
        entry = new Entry { Name = path };
        foreach (var rec in new[] { pathStart, pathStart & ~15 })
        {
            if (rec < 0)
                continue;
            if (TryFields64(buf, rec, n, maxPak, path, out entry))
                return true;
            if (TryFields32(buf, rec, n, maxPak, path, out entry))
                return true;
            if (TryFields32(buf, rec + 190, n, maxPak, path, out entry))
                return true;
            if (TryFields32(buf, rec + 256, n, maxPak, path, out entry))
                return true;
        }

        return false;
    }

    private static bool TryFields64(byte[] buf, int rec, int n, long maxPak, string path, out Entry entry)
    {
        entry = new Entry();
        var o = rec + 256;
        if (o + 32 > n)
            return false;
        var off = BitConverter.ToInt64(buf, o);
        var packed = BitConverter.ToInt64(buf, o + 8);
        var size = BitConverter.ToInt64(buf, o + 16);
        var flag = BitConverter.ToInt64(buf, o + 24);
        if (!Plausible(off, packed, size, flag, maxPak))
            return false;
        entry = new Entry { Name = path, Offset = off, Packed = packed, Size = size, Flag = flag };
        return true;
    }

    private static bool TryFields32(byte[] buf, int o, int n, long maxPak, string path, out Entry entry)
    {
        entry = new Entry();
        if (o < 0 || o + 20 > n)
            return false;
        var a = BitConverter.ToUInt32(buf, o);
        var b = BitConverter.ToUInt32(buf, o + 4);
        var c = BitConverter.ToUInt32(buf, o + 8);
        var d = BitConverter.ToUInt32(buf, o + 12);
        var e = BitConverter.ToUInt32(buf, o + 16);
        // TEterPackIndex after name: crc, packed, crc, real, position
        if (Plausible(e, b, d, 0, maxPak))
        {
            entry = new Entry { Name = path, Offset = e, Packed = b, Size = d, Flag = 0 };
            return true;
        }

        // position, packed, real
        if (Plausible(a, b, c, 0, maxPak))
        {
            entry = new Entry { Name = path, Offset = a, Packed = b, Size = c, Flag = 0 };
            return true;
        }

        if (Plausible(a, b, d, 0, maxPak))
        {
            entry = new Entry { Name = path, Offset = a, Packed = b, Size = d, Flag = 0 };
            return true;
        }

        return false;
    }

    private static bool Plausible(long off, long packed, long size, long flag, long maxPak) =>
        off >= 0 && packed >= 12 && packed <= 16L * 1024 * 1024 &&
        size >= 12 && size <= 16L * 1024 * 1024 &&
        off + packed <= maxPak && flag is >= 0 and <= 16;

    private static int PathStart(byte[] buf, int at)
    {
        var i = at;
        while (i > 0)
        {
            var c = buf[i - 1];
            if (c == 0)
                break;
            if (c is >= 32 and < 127)
                i--;
            else
                break;
        }

        return i;
    }

    private static string ReadAscii(byte[] buf, int start, int len)
    {
        if (start < 0 || len <= 0 || start + len > buf.Length)
            return "";
        while (len > 0 && buf[start + len - 1] == 0)
            len--;
        return Encoding.ASCII.GetString(buf, start, len);
    }

    private static int IndexOf(byte[] buf, int start, int end, byte[] needle)
    {
        var last = end - needle.Length;
        for (var i = start; i <= last; i++)
        {
            var ok = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (buf[i + j] != needle[j])
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                return i;
        }

        return -1;
    }

    private static bool TrySubImage(string text, out string image)
    {
        image = "";
        foreach (var key in new[] { "image", "dds", "tga" })
        {
            var i = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                continue;
            var rest = text[(i + key.Length)..];
            var start = rest.IndexOfAny(['"', '\'', 'd', '/', '\\', 'a', 'm', 'u', 'i']);
            if (start < 0)
                continue;
            rest = rest[start..].Trim().Trim('"', '\'');
            var end = rest.IndexOfAny(['\r', '\n', '"', '\'', ' ', '\t']);
            if (end > 0)
                rest = rest[..end];
            if (rest.Contains(".dds", StringComparison.OrdinalIgnoreCase) ||
                rest.Contains(".tga", StringComparison.OrdinalIgnoreCase))
            {
                image = rest;
                return true;
            }
        }

        return false;
    }

    private static string JoinImage(string subVirt, string image)
    {
        if (image.Contains(':') || image.StartsWith("d:", StringComparison.OrdinalIgnoreCase))
            return image;
        var slash = subVirt.LastIndexOfAny(['/', '\\']);
        if (slash > 0)
            return subVirt[..(slash + 1)] + image;
        return "d:/ymir work/ui/" + image;
    }

    private static byte[]? ReadDecoded(FileInfo[] packs, Entry entry)
    {
        foreach (var pack in packs)
        {
            if (entry.Offset + entry.Packed > pack.Length)
                continue;
            try
            {
                using var stream = pack.OpenRead();
                stream.Position = entry.Offset;
                var src = new byte[(int)entry.Packed];
                var read = 0;
                while (read < src.Length)
                {
                    var n = stream.Read(src, read, src.Length - read);
                    if (n <= 0)
                        return null;
                    read += n;
                }

                var data = Decode(src, entry);
                if (data != null && LooksUseful(data))
                    return data;
            }
            catch (IOException)
            {
            }
        }

        return null;
    }

    private static bool LooksUseful(byte[] data)
    {
        if (data.Length < 12)
            return false;
        if (data[0] == (byte)'D' && data[1] == (byte)'D' && data[2] == (byte)'S')
            return true;
        if (data[0] == 0x89 || data[0] == (byte)'B')
            return true;
        var text = 0;
        var n = Math.Min(data.Length, 80);
        for (var i = 0; i < n; i++)
        {
            var c = data[i];
            if (c is (byte)'\t' or (byte)'\n' or (byte)'\r' || c is >= 32 and < 127)
                text++;
        }

        return text * 10 >= n * 7;
    }

    public static byte[]? Decode(byte[] source, Entry entry)
    {
        if (source.Length == 0 || entry.Size < 0 || entry.Size > int.MaxValue)
            return null;
        if (LooksUseful(source) && (entry.Packed == entry.Size || entry.Size < 16))
            return source;
        if (entry.Packed == entry.Size)
            return source;
        if (entry.Flag == 0)
            return Lz4(source, (int)entry.Size) ?? Lzo1x(source, (int)entry.Size) ?? (LooksUseful(source) ? source : null);
        if (entry.Flag == 1)
        {
            var decrypted = (byte[])source.Clone();
            for (var i = 0; i < decrypted.Length; i++)
                decrypted[i] ^= XorKey[i & 15];
            return Lz4(decrypted, (int)entry.Size) ?? Lzo1x(decrypted, (int)entry.Size);
        }

        return Lzo1x(source, (int)entry.Size) ?? (LooksUseful(source) ? source : null);
    }

    private static byte[]? Lz4(byte[] src, int outSize)
    {
        var dst = new byte[outSize];
        var i = 0;
        var o = 0;
        try
        {
            while (i < src.Length)
            {
                var t = src[i++];
                var lit = t >> 4;
                if (lit == 15)
                {
                    int b;
                    do
                    {
                        b = src[i++];
                        lit += b;
                    } while (b == 255);
                }

                Buffer.BlockCopy(src, i, dst, o, lit);
                i += lit;
                o += lit;
                if (i >= src.Length || o >= outSize)
                    break;
                var off = src[i] | (src[i + 1] << 8);
                i += 2;
                var ml = t & 15;
                if (ml == 15)
                {
                    int b;
                    do
                    {
                        b = src[i++];
                        ml += b;
                    } while (b == 255);
                }

                ml += 4;
                if (off == 0 || off > o)
                    return null;
                for (var k = 0; k < ml; k++, o++)
                    dst[o] = dst[o - off];
            }
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }

        return o == outSize ? dst : null;
    }

    private static byte[]? Lzo1x(byte[] src, int outSize)
    {
        var dst = new byte[outSize];
        var ip = 0;
        var op = 0;
        try
        {
            int t = src[ip++];
            if (t > 17)
            {
                t -= 17;
                if (Copy(src, ref ip, dst, ref op, t) == null)
                    return null;
            }

            while (ip < src.Length && op < outSize)
            {
                t = src[ip++];
                if (t < 16)
                {
                    if (t == 0)
                    {
                        while (ip < src.Length && src[ip] == 0)
                        {
                            t += 255;
                            ip++;
                        }

                        t += 15 + src[ip++];
                    }

                    if (Copy(src, ref ip, dst, ref op, t + 3) == null)
                        return null;
                    continue;
                }

                int mOff;
                int mLen;
                if (t >= 64)
                {
                    mLen = (t >> 5) - 1;
                    mOff = (ip < src.Length ? src[ip++] : 0) | ((t >> 2) & 7) << 8;
                }
                else if (t >= 32)
                {
                    mLen = t & 31;
                    if (mLen == 0)
                    {
                        while (ip < src.Length && src[ip] == 0)
                        {
                            mLen += 255;
                            ip++;
                        }

                        mLen += 31 + src[ip++];
                    }

                    mOff = src[ip] | (src[ip + 1] << 8);
                    ip += 2;
                }
                else
                {
                    mOff = (t & 8) << 11;
                    mLen = t & 7;
                    if (mLen == 0)
                    {
                        while (ip < src.Length && src[ip] == 0)
                        {
                            mLen += 255;
                            ip++;
                        }

                        mLen += 7 + src[ip++];
                    }

                    mOff += src[ip] | (src[ip + 1] << 8);
                    ip += 2;
                    if (mOff == 0)
                        break;
                    mOff += 0x4000;
                }

                mOff++;
                mLen += 2;
                if (mOff > op || op + mLen > outSize)
                    return null;
                for (var k = 0; k < mLen; k++, op++)
                    dst[op] = dst[op - mOff];
                t &= 3;
                if (t != 0 && Copy(src, ref ip, dst, ref op, t) == null)
                    return null;
            }
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }

        return op == outSize ? dst : null;
    }

    private static byte[]? Copy(byte[] src, ref int ip, byte[] dst, ref int op, int n)
    {
        if (ip + n > src.Length || op + n > dst.Length)
            return null;
        Buffer.BlockCopy(src, ip, dst, op, n);
        ip += n;
        op += n;
        return dst;
    }
}
