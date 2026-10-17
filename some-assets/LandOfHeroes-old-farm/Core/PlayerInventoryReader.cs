using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

// Layout comes from the executable's profile (found automatically); values are range-checked while reading.
public sealed class PlayerInventoryReader : IDisposable
{
    public const string SupportedHash = "803bdbdffd0ad1848a6e780a4c1db2454366b706483199787865fe96bee92631";
    private readonly IntPtr handle;
    private readonly long module;
    private readonly ClientProfile profile;
    private bool disposed;
    public string ExecutableHash { get; private set; }
    private readonly Dictionary<uint, string> skillNames = new Dictionary<uint, string>();
    private readonly Dictionary<uint, string> itemNames = new Dictionary<uint, string>();
    public int ProcessId { get; private set; }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] bytes, UIntPtr count, out UIntPtr read);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);

    public PlayerInventoryReader(Process process)
    {
        ProcessId = process.Id;
        string path = process.MainModule.FileName;
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
            ExecutableHash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        profile = ClientProfile.For(process);
        if (!profile.InventoryComplete) throw new InvalidOperationException("Bu oyun sürümünün envanter düzeni bulunamadı; Uyumluluk sekmesinden taramayı çalıştırın.");
        module = process.MainModule.BaseAddress.ToInt64();
        handle = OpenProcess(0x410, false, ProcessId); // query + read only
        if (handle == IntPtr.Zero) throw new InvalidOperationException("Oyuna okuma bağlantısı kurulamadı.");
    }
    private byte[] Read(long address, int count)
    {
        if (disposed || address < 65536 || count < 0 || count > 65536) throw new InvalidOperationException("Geçersiz okuma isteği.");
        var bytes = new byte[count]; UIntPtr actual;
        if (!ReadProcessMemory(handle, new IntPtr(address), bytes, (UIntPtr)(uint)count, out actual) || actual.ToUInt64() != (ulong)count)
            throw new InvalidOperationException("Oyun verisi okunamadı. Karaktere giriş yapıp yeniden bağlanın.");
        return bytes;
    }
    private long Pointer(long address) { return profile.PointerSize==4 ? BitConverter.ToUInt32(Read(address,4),0) : BitConverter.ToInt64(Read(address,8),0); }
    private uint UInt(long address) { return BitConverter.ToUInt32(Read(address, 4), 0); }
    private static string Decode(byte[] bytes)
    {
        int end = Array.IndexOf(bytes, (byte)0); if (end < 0) end = bytes.Length;
        try { return new UTF8Encoding(false, true).GetString(bytes, 0, end); }
        catch (DecoderFallbackException) { return Encoding.GetEncoding(1254).GetString(bytes, 0, end); }
    }
    private string SkillName(uint id)
    {
        string name; if (skillNames.TryGetValue(id, out name)) return name;
        long manager = Pointer(module + profile.SkillManager), head = Pointer(manager + 8), node = Pointer(head + 8);
        var visited = new HashSet<long>();
        while (node != head && Read(node + 0x19, 1)[0] == 0)
        {
            if (!visited.Add(node) || visited.Count > 64) throw new InvalidOperationException("Beceri listesi doğrulanamadı.");
            uint key = UInt(node + 0x20);
            if (id == key)
            {
                long str = node + 0x60, size = Pointer(str + 16), capacity = Pointer(str + 24);
                if (size < 0 || size > 256 || capacity < size) throw new InvalidOperationException("Beceri adı doğrulanamadı.");
                name = Decode(Read(capacity > 15 ? Pointer(str) : str, (int)size));
                skillNames[id] = name; return name;
            }
            node = Pointer(node + (id < key ? 0 : 16));
        }
        return "Beceri #" + id;
    }
    private string ItemName(uint id)
    {
        string name; if (itemNames.TryGetValue(id, out name)) return name;
        long manager = Pointer(module + profile.ItemManager), head = Pointer(manager + 16), buckets = Pointer(manager + 32);
        ulong mask = unchecked((ulong)Pointer(manager + 56));
        if (mask > 0x1000000) throw new InvalidOperationException("Eşya listesi doğrulanamadı.");
        ulong hash = 0xcbf29ce484222325;
        foreach (byte b in BitConverter.GetBytes(id)) hash = unchecked((hash ^ b) * 0x100000001b3);
        long bucket = buckets + (long)(hash & mask) * 16, first = Pointer(bucket), node = Pointer(bucket + 8);
        var visited = new HashSet<long>();
        while (node != head)
        {
            if (!visited.Add(node) || visited.Count > 128) throw new InvalidOperationException("Eşya listesi doğrulanamadı.");
            if (UInt(node + 16) == id)
            {
                name = Decode(Read(Pointer(node + 24) + 0x181, 64)); itemNames[id] = name; return name;
            }
            if (node == first) break;
            node = Pointer(node + 8);
        }
        return "Eşya #" + id;
    }
    public PlayerInventorySnapshot Capture()
    {
        long player = Pointer(module + profile.Player);
        if (player < 65536) throw new InvalidOperationException("Karakter verisi hazır değil.");
        var result = new PlayerInventorySnapshot();
        byte[] skills = Read(player + profile.SkillArray, 255 * 40);
        for (int slot = 0; slot < 255; slot++)
        {
            int o = slot * 40; uint id = BitConverter.ToUInt32(skills, o); if (id == 0) continue;
            uint active = BitConverter.ToUInt32(skills, o + 36), grade = BitConverter.ToUInt32(skills, o + 8), level = BitConverter.ToUInt32(skills, o + 12);
            float cooldown = BitConverter.ToSingle(skills, o + 28);
            if (id > 1000000 || active > 1 || grade > 10 || level > 255 || float.IsNaN(cooldown) || float.IsInfinity(cooldown) || cooldown < 0)
                throw new InvalidOperationException("Beceri değerleri doğrulanamadı.");
            result.Skills.Add(new DetectedSkill { Slot = slot, Id = id, Name = SkillName(id), Level = level, Grade = grade, Active = active != 0, Cooldown = cooldown });
        }
        int stride = profile.ItemStride; byte[] items = Read(player + profile.ItemArray, 470 * stride);
        for (int slot = 0; slot < 470; slot++)
        {
            int o = slot * stride; uint id = BitConverter.ToUInt32(items, o), count = BitConverter.ToUInt32(items, o + 4);
            if (id == 0) continue;
            result.Items.Add(new DetectedItem { Slot = slot, Id = id, Count = count, Name = ItemName(id) });
        }
        if (Pointer(module + profile.Player) != player) throw new InvalidOperationException("Karakter değişti; listeyi yeniden okuyun.");
        result.CapturedAt = DateTime.Now; return result;
    }
    public void Dispose() { if (!disposed) { disposed = true; if (handle != IntPtr.Zero) CloseHandle(handle); } }
}
public sealed class PlayerInventorySnapshot
{
    public List<DetectedSkill> Skills { get; } = new List<DetectedSkill>();
    public List<DetectedItem> Items { get; } = new List<DetectedItem>();
    public DateTime CapturedAt { get; set; }
}
public sealed class DetectedSkill
{
    public int Slot { get; set; }
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint Level { get; set; }
    public uint Grade { get; set; }
    public bool Active { get; set; }
    public float Cooldown { get; set; }
}
public sealed class DetectedItem
{
    public int Slot { get; set; }
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint Count { get; set; }
}
