using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

// Land of Heroes packs: the .idx files are encrypted, but the running client keeps every decrypted index in memory as
// a 32-byte header followed by 288-byte records (char name[256]; u64 offset; u64 packedSize; u64 size; u64 flag).
// In the .pak, flag 0 is a plain LZ4 block and flag 1 is the same block protected by the client's repeating
// 16-byte XOR key. This class reads the indexes from the game's memory (read-only), stores them, and extracts files.
public static class PackExtractor
{
    public sealed class Entry { public string Name; public long Offset, Packed, Size, Flag; }
    public sealed class PendingEntry
    {
        public string PackName;
        public Entry Item;
        public string TargetPath;
    }
    public sealed class ExtractionResult
    {
        public int Written;
        public int Failed;
        public readonly List<PendingEntry> Pending = new List<PendingEntry>();
        public readonly List<string> Errors = new List<string>();
    }
    const int Rec = 288, Head = 32;
    static readonly byte[] Flag1Key = Encoding.ASCII.GetBytes("w317bfaoq07jrvmh");

    [StructLayout(LayoutKind.Sequential)] struct MBI { public IntPtr BaseAddress, AllocationBase; public uint AllocationProtect; public IntPtr RegionSize; public uint State, Protect, Type; }
    [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
    [DllImport("kernel32.dll")] static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, IntPtr size, out IntPtr read);
    [DllImport("kernel32.dll")] static extern IntPtr VirtualQueryEx(IntPtr h, IntPtr addr, out MBI mbi, IntPtr len);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);

    public static string PackFolder(Process game) { return Path.Combine(Path.GetDirectoryName(game.MainModule.FileName), "pack"); }

    // Reads every decrypted index found in the game's memory. Returns pack name -> entries.
    public static Dictionary<string, List<Entry>> DumpIndexes(Process game, string packDir, Action<string> log)
    {
        var expected = new Dictionary<string, long>(); long maxPak = 0;
        foreach (var idx in Directory.GetFiles(packDir, "*.idx"))
        {
            var name = Path.GetFileNameWithoutExtension(idx); var pak = Path.Combine(packDir, name + ".pak"); if (!File.Exists(pak)) continue;
            expected[name] = (new FileInfo(idx).Length - Head) / Rec; maxPak = Math.Max(maxPak, new FileInfo(pak).Length);
        }
        var runs = new List<List<Entry>>();
        var h = OpenProcess(0x0410, false, game.Id);
        try
        {
            long addr = 0;
            while (addr < 0x7FFFFFFFFFFF)
            {
                MBI m; if (VirtualQueryEx(h, new IntPtr(addr), out m, new IntPtr(Marshal.SizeOf(typeof(MBI)))) == IntPtr.Zero) break;
                long size = m.RegionSize.ToInt64();
                if (m.State == 0x1000 && m.Type == 0x20000 && (m.Protect & 0x100) == 0 && (m.Protect & 0x04) != 0 && size < 1024L * 1024 * 1024)
                {
                    var buf = new byte[size]; IntPtr read;
                    if (ReadProcessMemory(h, m.BaseAddress, buf, new IntPtr(size), out read))
                        for (int i = 0; i + Rec * 2 <= (int)read;)
                        {
                            if (!Valid(buf, i, maxPak) || !Valid(buf, i + Rec, maxPak)) { i += 16; continue; }
                            int count = 0; while (Valid(buf, i + count * Rec, maxPak)) count++;
                            runs.Add(Enumerable.Range(0, count).Select(k => Read(buf, i + k * Rec)).ToList()); i += count * Rec;
                        }
                }
                addr = m.BaseAddress.ToInt64() + size;
            }
        }
        finally { CloseHandle(h); }
        log?.Invoke(runs.Count + " kayıt bloğu bulundu, paketlerle eşleştiriliyor…");
        var found = new Dictionary<string, List<Entry>>();
        // Pass 1: the window tiles the .pak exactly. Pass 2 (only with stronger decoding proof): entries fit without overlap.
        for (int pass = 1; pass <= 2; pass++)
            foreach (var pack in expected.Keys.OrderBy(k => k))
            {
                if (found.ContainsKey(pack) || expected[pack] <= 0) continue;
                var pak = Path.Combine(packDir, pack + ".pak"); long len = new FileInfo(pak).Length; int n = (int)expected[pack];
                foreach (var run in runs)
                {
                    if (found.ContainsKey(pack)) break;
                    for (int w = 0; w + n <= run.Count; w++)
                    {
                        if (run[w].Offset + run[w].Packed > len) continue;
                        var window = run.GetRange(w, n);
                        // Archive names are accepted only after every transformed record decodes against that exact
                        // .pak. A few map packs intentionally share offsets and counts, so sampling is not sufficient.
                        bool ok = pass == 1 ? Tiles(window, len) && Decodes(pak, window, int.MaxValue, true) : FitsWithoutOverlap(window, len) && Decodes(pak, window, int.MaxValue, false);
                        if (!ok) continue;
                        found[pack] = window; log?.Invoke(pack + ": " + n + " dosya"); break;
                    }
                }
            }
        var missing = expected.Keys.Where(k => !found.ContainsKey(k)).OrderBy(k => k).ToList();
        if (missing.Count > 0) log?.Invoke("Bellekte bulunamayan paketler: " + string.Join(", ", missing));
        return found;
    }

    static bool Valid(byte[] b, int i, long maxPak)
    {
        if (i + Rec > b.Length) return false;
        int n = 0; while (n < 256 && b[i + n] != 0) { byte c = b[i + n]; if (c < 0x20 || c == 0x7f) return false; n++; }
        if (n < 3 || n == 256) return false;
        long off = BitConverter.ToInt64(b, i + 256), packed = BitConverter.ToInt64(b, i + 264), size = BitConverter.ToInt64(b, i + 272), flag = BitConverter.ToInt64(b, i + 280);
        return off >= 0 && off < maxPak && packed >= 0 && packed <= maxPak && size >= 0 && size < (1L << 32) && flag >= 0 && flag < 256;
    }
    static Entry Read(byte[] b, int i)
    {
        int n = 0; while (b[i + n] != 0) n++;
        return new Entry { Name = Encoding.GetEncoding(1254).GetString(b, i, n), Offset = BitConverter.ToInt64(b, i + 256), Packed = BitConverter.ToInt64(b, i + 264), Size = BitConverter.ToInt64(b, i + 272), Flag = BitConverter.ToInt64(b, i + 280) };
    }
    static bool Tiles(List<Entry> window, long len)
    {
        var s = window.OrderBy(e => e.Offset).ToList();
        if (s[s.Count - 1].Offset + s[s.Count - 1].Packed != len) return false;
        for (int k = 0; k + 1 < s.Count; k++) if (s[k].Offset + s[k].Packed != s[k + 1].Offset) return false;
        return true;
    }
    static bool FitsWithoutOverlap(List<Entry> window, long len)
    {
        var s = window.OrderBy(e => e.Offset).ToList();
        if (s[s.Count - 1].Offset + s[s.Count - 1].Packed > len) return false;
        for (int k = 0; k + 1 < s.Count; k++) if (s[k].Offset + s[k].Packed > s[k + 1].Offset) return false;
        return true;
    }
    // Decodes transformation-requiring entries spread over the full window. Sampling always includes the first and
    // last candidate; this prevents a shorter archive from being mistaken for a same-sized slice of another index.
    static bool Decodes(string pak, List<Entry> window, int probes, bool allowNone)
    {
        var transformed = window.Where(e => (e.Flag == 1 || (e.Flag == 0 && e.Packed != e.Size)) && e.Packed >= 0 && e.Size >= 0 && e.Packed <= int.MaxValue && e.Size <= int.MaxValue).ToList();
        if (transformed.Count == 0) return allowNone;
        int take = Math.Min(Math.Max(1, probes), transformed.Count);
        try
        {
            using (var s = File.OpenRead(pak))
                for (int k = 0; k < take; k++)
                {
                    int index = take == 1 ? 0 : (int)((long)k * (transformed.Count - 1) / (take - 1));
                    var e = transformed[index];
                    if (e.Offset < 0 || e.Offset + e.Packed > s.Length) return false;
                    var src = new byte[(int)e.Packed]; s.Position = e.Offset;
                    int read = 0;
                    while (read < src.Length)
                    {
                        int count = s.Read(src, read, src.Length - read); if (count <= 0) return false; read += count;
                    }
                    var decoded = DecodeEntry(src, e);
                    if (decoded == null || decoded.LongLength != e.Size) return false;
                }
            return true;
        }
        catch { return false; }
    }

    // Cached indexes are accepted only when every entry still belongs to the selected .pak and decodes completely.
    public static bool ValidateIndex(string pak, List<Entry> entries)
    {
        if (string.IsNullOrWhiteSpace(pak) || !File.Exists(pak) || entries == null || entries.Count == 0) return false;
        long len = new FileInfo(pak).Length;
        if (!FitsWithoutOverlap(entries, len)) return false;
        if (entries.Any(e => e.Flag != 0 && e.Flag != 1)) return false;
        return Decodes(pak, entries, int.MaxValue, true);
    }

    public static void SaveIndexes(Dictionary<string, List<Entry>> indexes, string dir)
    {
        Directory.CreateDirectory(dir);
        foreach (var kv in indexes) File.WriteAllLines(Path.Combine(dir, kv.Key + ".tsv"), kv.Value.Select(e => e.Name + "\t" + e.Offset + "\t" + e.Packed + "\t" + e.Size + "\t" + e.Flag), Encoding.UTF8);
    }
    public static Dictionary<string, List<Entry>> LoadIndexes(string dir)
    {
        var result = new Dictionary<string, List<Entry>>();
        if (!Directory.Exists(dir)) return result;
        foreach (var tsv in Directory.GetFiles(dir, "*.tsv"))
            result[Path.GetFileNameWithoutExtension(tsv)] = File.ReadAllLines(tsv).Select(l => l.Split('\t')).Where(f => f.Length == 5)
                .Select(f => new Entry { Name = f[0], Offset = long.Parse(f[1]), Packed = long.Parse(f[2]), Size = long.Parse(f[3]), Flag = long.Parse(f[4]) }).ToList();
        return result;
    }

    static readonly Regex MapFile = new Regex(@"^(?<map>.+)/(?<area>\d{6})/(?<file>attr\.atr|minimap\.dds|minimap\.png|[^/]*minimap\.bmp)$", RegexOptions.IgnoreCase);
    // Extracts attr.atr and the minimap picture of every map area into mapsDir/<map>/<area>/. Returns files written.
    public static int ExtractMaps(string packDir, Dictionary<string, List<Entry>> indexes, string mapsDir, Action<string> log)
    {
        int written = 0, skipped = 0;
        foreach (var kv in indexes)
        {
            var files = kv.Value.Where(e => MapFile.IsMatch(e.Name.Replace('\\', '/'))).ToList(); if (files.Count == 0) continue;
            var pak = Path.Combine(packDir, kv.Key + ".pak"); if (!File.Exists(pak)) continue;
            using (var s = File.OpenRead(pak))
                foreach (var e in files)
                {
                    var src = new byte[e.Packed]; s.Position = e.Offset; s.Read(src, 0, src.Length);
                    var data = DecodeEntry(src, e); if (data == null) { skipped++; continue; }
                    var m = MapFile.Match(e.Name.Replace('\\', '/'));
                    var target = Path.Combine(mapsDir, m.Groups["map"].Value.Replace('/', Path.DirectorySeparatorChar), m.Groups["area"].Value, m.Groups["file"].Value.ToLowerInvariant().EndsWith("minimap.bmp") ? "minimap.bmp" : m.Groups["file"].Value.ToLowerInvariant());
                    Directory.CreateDirectory(Path.GetDirectoryName(target)); File.WriteAllBytes(target, data); written++;
                }
            log?.Invoke(kv.Key + ": " + files.Count + " harita dosyası");
        }
        log?.Invoke("Yazılan " + written + " dosya" + (skipped > 0 ? " · çözülemeyen " + skipped : ""));
        return written;
    }

    // Extracts the selected packs into outputDir/<pack name>/<original virtual path>.
    // Plain LZ4 and the client's XOR-protected LZ4 entries are handled directly. Unknown future flag values are
    // returned in Pending so the caller may try the running client's read-only pack API.
    public static ExtractionResult ExtractSelectedPacks(string packDir, Dictionary<string, List<Entry>> indexes, IEnumerable<string> selectedPacks, string outputDir, Action<string> log)
    {
        if (string.IsNullOrWhiteSpace(packDir) || !Directory.Exists(packDir)) throw new DirectoryNotFoundException("Pack klasörü bulunamadı.");
        if (string.IsNullOrWhiteSpace(outputDir)) throw new ArgumentException("Çıktı klasörü seçilmedi.");
        Directory.CreateDirectory(outputDir);
        var result = new ExtractionResult();
        var selected = new HashSet<string>(selectedPacks ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var packName in selected.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            List<Entry> entries;
            if (!indexes.TryGetValue(packName, out entries))
            {
                result.Failed++;
                result.Errors.Add(packName + ": çözülen dizin bulunamadı");
                log?.Invoke("✗ " + packName + ": dizin bulunamadı");
                continue;
            }
            var pak = Path.Combine(packDir, packName + ".pak");
            if (!File.Exists(pak))
            {
                result.Failed++;
                result.Errors.Add(packName + ": .pak dosyası bulunamadı");
                log?.Invoke("✗ " + packName + ": .pak bulunamadı");
                continue;
            }
            int before = result.Written, pendingBefore = result.Pending.Count, failedBefore = result.Failed;
            using (var stream = File.OpenRead(pak))
            {
                foreach (var entry in entries)
                {
                    string target;
                    try { target = SafePackTarget(outputDir, packName, entry.Name); }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        result.Errors.Add(packName + " / " + entry.Name + ": " + ex.Message);
                        continue;
                    }
                    if (entry.Flag != 0 && entry.Flag != 1)
                    {
                        result.Pending.Add(new PendingEntry { PackName = packName, Item = entry, TargetPath = target });
                        continue;
                    }
                    try
                    {
                        if (entry.Offset < 0 || entry.Packed < 0 || entry.Size < 0 || entry.Offset + entry.Packed > stream.Length || entry.Packed > int.MaxValue || entry.Size > int.MaxValue)
                            throw new InvalidDataException("Geçersiz boyut veya konum.");
                        byte[] data;
                        if (entry.Packed == 0 && entry.Size == 0) data = Array.Empty<byte>();
                        else
                        {
                            var source = new byte[(int)entry.Packed];
                            stream.Position = entry.Offset;
                            int read = 0;
                            while (read < source.Length)
                            {
                                int count = stream.Read(source, read, source.Length - read);
                                if (count <= 0) throw new EndOfStreamException();
                                read += count;
                            }
                            data = DecodeEntry(source, entry);
                            if (data == null) throw new InvalidDataException(entry.Flag == 1 ? "Şifreli LZ4 verisi çözülemedi." : "LZ4 verisi çözülemedi.");
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.WriteAllBytes(target, data);
                        result.Written++;
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        result.Errors.Add(packName + " / " + entry.Name + ": " + ex.Message);
                    }
                }
            }
            log?.Invoke(packName + ": " + (result.Written - before) + " doğrudan · " + (result.Pending.Count - pendingBefore) + " istemciyle çözülecek · " + (result.Failed - failedBefore) + " hatalı");
        }
        return result;
    }

    public static string SafePackTarget(string outputDir, string packName, string virtualName)
    {
        var packRoot = Path.GetFullPath(Path.Combine(outputDir, SafeSegment(packName)));
        var parts = (virtualName ?? "").Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p != "." && p != "..").Select(SafeSegment).Where(p => p.Length > 0).ToList();
        if (parts.Count == 0) parts.Add("unnamed.bin");
        string target = packRoot;
        foreach (var part in parts) target = Path.Combine(target, part);
        target = Path.GetFullPath(target);
        var prefix = packRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Güvensiz dosya yolu reddedildi.");
        return target;
    }

    static string SafeSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = (value ?? "").Select(c => c == ':' || invalid.Contains(c) ? '_' : c).ToArray();
        var safe = new string(chars).Trim().TrimEnd('.');
        if (safe.Length == 0 || safe == "." || safe == "..") safe = "_";
        var stem = safe.Split('.')[0];
        string[] reserved = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        if (reserved.Contains(stem, StringComparer.OrdinalIgnoreCase)) safe = "_" + safe;
        return safe;
    }

    public static byte[] DecodeEntry(byte[] source, Entry entry)
    {
        if (source == null || entry == null || entry.Size < 0 || entry.Size > int.MaxValue) return null;
        if (entry.Flag == 0)
        {
            if (entry.Packed == entry.Size) return source;
            return Lz4(source, (int)entry.Size);
        }
        if (entry.Flag == 1)
        {
            var decrypted = (byte[])source.Clone();
            for (int i = 0; i < decrypted.Length; i++) decrypted[i] ^= Flag1Key[i & 15];
            return Lz4(decrypted, (int)entry.Size);
        }
        return null;
    }

    public static byte[] Lz4(byte[] src, int outSize)
    {
        var dst = new byte[outSize]; int i = 0, o = 0;
        try
        {
            while (i < src.Length)
            {
                int t = src[i++], lit = t >> 4; if (lit == 15) { int b; do { b = src[i++]; lit += b; } while (b == 255); }
                Buffer.BlockCopy(src, i, dst, o, lit); i += lit; o += lit; if (i >= src.Length || o >= outSize) break;
                int off = src[i] | (src[i + 1] << 8); i += 2; int ml = t & 15; if (ml == 15) { int b; do { b = src[i++]; ml += b; } while (b == 255); } ml += 4;
                if (off == 0 || off > o) return null; for (int k = 0; k < ml; k++, o++) dst[o] = dst[o - off];
            }
        }
        catch (IndexOutOfRangeException) { return null; }
        catch (ArgumentException) { return null; }
        return o == outSize ? dst : null;
    }
}
