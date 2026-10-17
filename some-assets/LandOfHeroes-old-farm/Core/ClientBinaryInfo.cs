using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public sealed class ClientBinaryInfo
{
    public string Path { get; private set; }
    public ushort Machine { get; private set; }
    public ushort OptionalMagic { get; private set; }
    public int PointerSize { get; private set; }
    public ulong ImageBase { get; private set; }
    public uint TimeDateStamp { get; private set; }
    public bool Is64Bit { get { return PointerSize == 8; } }
    public string Architecture
    {
        get
        {
            if (Machine == 0x8664) return "x64";
            if (Machine == 0x014c) return "x86";
            if (Machine == 0xAA64) return "ARM64";
            return "Bilinmiyor (0x" + Machine.ToString("X4") + ")";
        }
    }
    public string DisplayName { get { return Architecture + " · " + PointerSize * 8 + " bit işaretçi"; } }

    public static ClientBinaryInfo Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new FileNotFoundException("İstemci EXE bulunamadı.", path);
        using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var reader = new BinaryReader(stream, Encoding.ASCII))
        {
            if (stream.Length < 256 || reader.ReadUInt16() != 0x5A4D) throw new InvalidDataException("Dosya geçerli bir Windows EXE değil.");
            stream.Position = 0x3c; int pe = reader.ReadInt32();
            if (pe < 64 || pe > stream.Length - 128) throw new InvalidDataException("EXE başlığı geçersiz.");
            stream.Position = pe;
            if (reader.ReadUInt32() != 0x00004550) throw new InvalidDataException("PE imzası bulunamadı.");
            ushort machine = reader.ReadUInt16();
            reader.ReadUInt16();
            uint stamp = reader.ReadUInt32();
            stream.Position = pe + 24;
            ushort magic = reader.ReadUInt16();
            int pointerSize;
            ulong imageBase;
            if (magic == 0x20b) { pointerSize = 8; stream.Position = pe + 24 + 24; imageBase = reader.ReadUInt64(); }
            else if (magic == 0x10b) { pointerSize = 4; stream.Position = pe + 24 + 28; imageBase = reader.ReadUInt32(); }
            else throw new InvalidDataException("Desteklenmeyen PE isteğe bağlı başlığı: 0x" + magic.ToString("X"));
            if ((machine == 0x8664 && pointerSize != 8) || (machine == 0x014c && pointerSize != 4))
                throw new InvalidDataException("EXE mimarisi ile işaretçi genişliği uyuşmuyor.");
            return new ClientBinaryInfo { Path = System.IO.Path.GetFullPath(path), Machine = machine, OptionalMagic = magic, PointerSize = pointerSize, ImageBase = imageBase, TimeDateStamp = stamp };
        }
    }

    public static bool TryRead(string path, out ClientBinaryInfo info)
    {
        try { info = Read(path); return true; } catch { info = null; return false; }
    }

    public static string FindBestExecutable(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return null;
        var candidates = new List<Tuple<int, string>>();
        IEnumerable<string> files;
        try { files = Directory.EnumerateFiles(folder, "*.exe", SearchOption.TopDirectoryOnly); }
        catch { return null; }
        bool packs = Directory.EnumerateFiles(folder, "*.idx", SearchOption.TopDirectoryOnly).Any() ||
                     Directory.EnumerateFiles(folder, "*.eix", SearchOption.TopDirectoryOnly).Any() ||
                     Directory.Exists(System.IO.Path.Combine(folder, "pack"));
        foreach (string file in files)
        {
            ClientBinaryInfo info;
            if (!TryRead(file, out info) || (info.Machine != 0x014c && info.Machine != 0x8664)) continue;
            var fi = new FileInfo(file); string name = System.IO.Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            int score = (int)Math.Min(80, fi.Length / (1024 * 1024));
            if (packs) score += 80;
            if (name.Contains("client") || name.Contains("metin") || name.Contains("game") || name.Contains("hero")) score += 50;
            if (name.Contains("patch") || name.Contains("config") || name.Contains("crash") || name.Contains("unins") || name == "onur") score -= 120;
            candidates.Add(Tuple.Create(score, file));
        }
        return candidates.OrderByDescending(x => x.Item1).ThenByDescending(x => new FileInfo(x.Item2).Length).Select(x => x.Item2).FirstOrDefault();
    }
}
