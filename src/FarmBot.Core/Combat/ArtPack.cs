using System.Buffers.Binary;

namespace FarmBot.Core.Combat;

public readonly record struct ArtPackEntry(
    uint Hash,
    uint Hash2,
    byte Type,
    uint Offset,
    uint StoredSize,
    uint RealSize,
    uint Crc,
    uint Trail0,
    uint Trail1,
    uint Trail2);

public sealed class ArtPack : IDisposable
{
    public const int RecordSize = 39;
    public const ushort Version = 1;

    private readonly FileStream _stream;
    private readonly ArtPackEntry[] _entries;
    private readonly Dictionary<uint, int> _byHash;

    public string Path { get; }
    public IReadOnlyList<ArtPackEntry> Entries => _entries;

    private ArtPack(string path, FileStream stream, ArtPackEntry[] entries)
    {
        Path = path;
        _stream = stream;
        _entries = entries;
        _byHash = new Dictionary<uint, int>(entries.Length * 2);
        for (var i = 0; i < entries.Length; i++)
        {
            _byHash.TryAdd(entries[i].Hash, i);
            _byHash.TryAdd(entries[i].Hash2, i);
        }
    }

    public static bool TryOpen(string path, out ArtPack pack)
    {
        pack = null!;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;
        FileStream? stream = null;
        try
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (stream.Length < 6)
                return false;
            Span<byte> hdr = stackalloc byte[6];
            if (stream.Read(hdr) != 6)
                return false;
            var ver = BinaryPrimitives.ReadUInt16LittleEndian(hdr);
            var count = BinaryPrimitives.ReadInt32LittleEndian(hdr.Slice(2));
            if (ver != Version || count < 0 || count > 200_000)
                return false;
            var index = new byte[checked(count * RecordSize)];
            if (count > 0 && stream.Read(index) != index.Length)
                return false;
            var entries = new ArtPackEntry[count];
            for (var i = 0; i < count; i++)
            {
                var o = i * RecordSize;
                entries[i] = new ArtPackEntry(
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 4)),
                    index[o + 10],
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 11)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 15)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 19)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 23)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 27)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 31)),
                    BinaryPrimitives.ReadUInt32LittleEndian(index.AsSpan(o + 35)));
            }

            pack = new ArtPack(path, stream, entries);
            stream = null;
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        finally
        {
            stream?.Dispose();
        }
    }

    public bool TryGet(uint hash, out ArtPackEntry entry)
    {
        if (_byHash.TryGetValue(hash, out var i))
        {
            entry = _entries[i];
            return true;
        }

        entry = default;
        return false;
    }

    public bool TryReadStored(in ArtPackEntry entry, out byte[] data)
    {
        data = Array.Empty<byte>();
        if (entry.StoredSize == 0 || entry.StoredSize > 16 * 1024 * 1024)
            return false;
        if (entry.Offset + entry.StoredSize > (ulong)_stream.Length)
            return false;
        data = new byte[entry.StoredSize];
        _stream.Seek(entry.Offset, SeekOrigin.Begin);
        return _stream.Read(data, 0, data.Length) == data.Length;
    }

    public void Dispose() => _stream.Dispose();

    public static uint Fnv8(ReadOnlySpan<byte> eight, uint seed = 0x811C9DC5)
    {
        if (eight.Length < 8)
            return 0;
        var h = (seed ^ eight[0]) * 0x01000193;
        for (var i = 1; i < 8; i++)
            h = (h ^ eight[i]) * 0x01000193;
        return h;
    }
}
