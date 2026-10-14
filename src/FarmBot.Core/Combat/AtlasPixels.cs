using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;

namespace FarmBot.Core.Combat;

public sealed class AtlasPixels
{
    public const string MapName = @"Local\FarmBotAtlas";
    public const uint Magic = 0x534C5441;
    public const int HeaderBytes = 32;

    public int Width { get; init; }
    public int Height { get; init; }
    public float WorldW { get; init; }
    public float WorldH { get; init; }
    public byte[] Bgra { get; init; } = Array.Empty<byte>();

    public bool HasImage => Width >= 32 && Height >= 32 && Bgra.Length >= Width * Height * 4;

    public static bool TryOpen(out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        var path = Path.Combine(Path.GetTempPath(), "FarmBotAtlas.bin");
        if (TryReadBytes(ReadAllOrEmpty(path), out atlas))
            return true;
        return OperatingSystem.IsWindows() && TryReadMap(out atlas);
    }

    private static byte[] ReadAllOrEmpty(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
        }
        catch (IOException)
        {
            return Array.Empty<byte>();
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool TryReadMap(out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        try
        {
            using var map = MemoryMappedFile.OpenExisting(MapName, MemoryMappedFileRights.Read);
            using var view = map.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            if (view.Capacity < HeaderBytes)
                return false;
            var bytes = new byte[(int)Math.Min(view.Capacity, HeaderBytes + 1024 * 1024 * 4)];
            view.ReadArray(0, bytes, 0, bytes.Length);
            return TryReadBytes(bytes, out atlas);
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool TryReadBytes(byte[] bytes, out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        if (bytes.Length < HeaderBytes)
            return false;
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != Magic)
            return false;
        var width = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(8));
        var height = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(12));
        var worldW = BitConverter.ToSingle(bytes, 16);
        var worldH = BitConverter.ToSingle(bytes, 20);
        var ready = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24));
        if (ready == 0 || width is < 32 or > 1024 || height is < 32 or > 1024)
            return false;
        var pixels = width * height * 4;
        if (bytes.Length < HeaderBytes + pixels)
            return false;
        var bgra = new byte[pixels];
        Buffer.BlockCopy(bytes, HeaderBytes, bgra, 0, pixels);
        atlas = new AtlasPixels
        {
            Width = width,
            Height = height,
            WorldW = worldW,
            WorldH = worldH,
            Bgra = bgra
        };
        return true;
    }

    public static bool TryLoadFromClient(string clientPath, string mapName, float worldW, float worldH, out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        if (string.IsNullOrWhiteSpace(clientPath) || string.IsNullOrWhiteSpace(mapName))
            return false;
        var root = File.Exists(clientPath) ? Path.GetDirectoryName(clientPath) : clientPath;
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return false;
        var dds = FindAtlasDds(root, mapName);
        if (dds == null || !TryDecodeDds(File.ReadAllBytes(dds), out var width, out var height, out var bgra))
            return false;
        CropToSub(root, mapName, ref width, ref height, ref bgra);
        if (worldW < 1000 || worldH < 1000)
            TryWorldFromPointFile(root, mapName, out worldW, out worldH);
        atlas = new AtlasPixels
        {
            Width = width,
            Height = height,
            WorldW = worldW,
            WorldH = worldH,
            Bgra = bgra
        };
        return atlas.HasImage;
    }

    public static bool TryWorldFromPointFile(string clientRoot, string mapName, out float worldW, out float worldH)
    {
        worldW = 0;
        worldH = 0;
        var locale = Path.Combine(clientRoot, "pack", "locale", "locale");
        if (!Directory.Exists(locale))
            return false;
        foreach (var lang in Directory.EnumerateDirectories(locale))
        {
            var path = Path.Combine(lang, "map", mapName + "_point.txt");
            if (!File.Exists(path))
                continue;
            float maxX = 0, maxY = 0;
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    continue;
                if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x))
                    continue;
                if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y))
                    continue;
                maxX = Math.Max(maxX, Math.Abs(x));
                maxY = Math.Max(maxY, Math.Abs(y));
            }

            if (maxX < 1000 || maxY < 1000)
                continue;
            worldW = (float)Math.Ceiling(maxX / 25600f) * 25600f;
            worldH = (float)Math.Ceiling(maxY / 25600f) * 25600f;
            return true;
        }

        return false;
    }

    private static string? FindAtlasDds(string clientRoot, string mapName)
    {
        var file = mapName.Trim() + "_atlas.dds";
        var roots = new[]
        {
            Path.Combine(clientRoot, "pack", "yw_etc", "ymir work", "ui"),
            Path.Combine(clientRoot, "pack", "yw_etc", "ymir work", "ui", "atlas_resize"),
            Path.Combine(clientRoot, "ymir work", "ui")
        };
        foreach (var dir in roots)
        {
            var path = Path.Combine(dir, file);
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static void CropToSub(string clientRoot, string mapName, ref int width, ref int height, ref byte[] bgra)
    {
        var sub = Path.Combine(clientRoot, "pack", "yw_etc", "ymir work", "ui", "atlas", mapName, "atlas.sub");
        if (!File.Exists(sub))
            return;
        var text = File.ReadAllText(sub);
        if (!TrySubInt(text, "left", out var left) || !TrySubInt(text, "top", out var top) ||
            !TrySubInt(text, "right", out var right) || !TrySubInt(text, "bottom", out var bottom))
            return;
        var w = right - left;
        var h = bottom - top;
        if (w < 32 || h < 32 || w > width || h > height || left < 0 || top < 0)
            return;
        var cropped = new byte[w * h * 4];
        for (var y = 0; y < h; y++)
            Buffer.BlockCopy(bgra, ((top + y) * width + left) * 4, cropped, y * w * 4, w * 4);
        width = w;
        height = h;
        bgra = cropped;
    }

    private static bool TrySubInt(string text, string key, out int value)
    {
        value = 0;
        foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                continue;
            var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && int.TryParse(parts[1], out value))
                return true;
        }

        return false;
    }

    public static bool TryDecodeDds(byte[] bytes, out int width, out int height, out byte[] bgra)
    {
        width = 0;
        height = 0;
        bgra = Array.Empty<byte>();
        if (bytes.Length < 128 || bytes[0] != (byte)'D' || bytes[1] != (byte)'D' || bytes[2] != (byte)'S')
            return false;
        height = BitConverter.ToInt32(bytes, 12);
        width = BitConverter.ToInt32(bytes, 16);
        if (width is < 32 or > 2048 || height is < 32 or > 2048)
            return false;
        var flags = BitConverter.ToUInt32(bytes, 80);
        var fourcc = BitConverter.ToUInt32(bytes, 84);
        bgra = new byte[width * height * 4];
        if ((flags & 4) != 0 && fourcc == 0x31545844)
            return DecodeDxt1(bytes.AsSpan(128), width, height, bgra);
        if ((flags & 4) != 0 && fourcc == 0x35545844)
            return DecodeDxt5(bytes.AsSpan(128), width, height, bgra);
        if (bytes.Length >= 128 + width * height * 4)
        {
            Buffer.BlockCopy(bytes, 128, bgra, 0, width * height * 4);
            return true;
        }

        bgra = Array.Empty<byte>();
        return false;
    }

    private static bool DecodeDxt1(ReadOnlySpan<byte> src, int width, int height, byte[] dst)
    {
        var blocksX = (width + 3) / 4;
        var blocksY = (height + 3) / 4;
        if (src.Length < blocksX * blocksY * 8)
            return false;
        Span<uint> col = stackalloc uint[4];
        var i = 0;
        for (var by = 0; by < blocksY; by++)
        {
            for (var bx = 0; bx < blocksX; bx++)
            {
                var c0 = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(i, 2));
                var c1 = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(i + 2, 2));
                var idx = BinaryPrimitives.ReadUInt32LittleEndian(src.Slice(i + 4, 4));
                i += 8;
                col[0] = Rgb565(c0);
                col[1] = Rgb565(c1);
                if (c0 > c1)
                {
                    col[2] = LerpRgb(col[0], col[1], 2, 1);
                    col[3] = LerpRgb(col[0], col[1], 1, 2);
                }
                else
                {
                    col[2] = LerpRgb(col[0], col[1], 1, 1);
                    col[3] = 0;
                }

                WriteDxtBlock(dst, width, height, bx, by, idx, col);
            }
        }

        return true;
    }

    private static bool DecodeDxt5(ReadOnlySpan<byte> src, int width, int height, byte[] dst)
    {
        var blocksX = (width + 3) / 4;
        var blocksY = (height + 3) / 4;
        if (src.Length < blocksX * blocksY * 16)
            return false;
        Span<uint> col = stackalloc uint[4];
        var i = 0;
        for (var by = 0; by < blocksY; by++)
        {
            for (var bx = 0; bx < blocksX; bx++)
            {
                i += 8;
                var c0 = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(i, 2));
                var c1 = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(i + 2, 2));
                var idx = BinaryPrimitives.ReadUInt32LittleEndian(src.Slice(i + 4, 4));
                i += 8;
                col[0] = Rgb565(c0);
                col[1] = Rgb565(c1);
                col[2] = LerpRgb(col[0], col[1], 2, 1);
                col[3] = LerpRgb(col[0], col[1], 1, 2);
                WriteDxtBlock(dst, width, height, bx, by, idx, col);
            }
        }

        return true;
    }

    private static void WriteDxtBlock(byte[] dst, int width, int height, int bx, int by, uint idx, ReadOnlySpan<uint> col)
    {
        for (var py = 0; py < 4; py++)
        {
            for (var px = 0; px < 4; px++)
            {
                var x = bx * 4 + px;
                var y = by * 4 + py;
                var ci = (int)(idx & 3);
                idx >>= 2;
                if ((uint)x >= (uint)width || (uint)y >= (uint)height)
                    continue;
                var c = col[ci];
                var o = (y * width + x) * 4;
                dst[o] = (byte)c;
                dst[o + 1] = (byte)(c >> 8);
                dst[o + 2] = (byte)(c >> 16);
                dst[o + 3] = 255;
            }
        }
    }

    private static uint Rgb565(ushort c)
    {
        var r = (uint)((c >> 11) & 31) * 255 / 31;
        var g = (uint)((c >> 5) & 63) * 255 / 63;
        var b = (uint)(c & 31) * 255 / 31;
        return b | (g << 8) | (r << 16);
    }

    private static uint LerpRgb(uint a, uint b, int wa, int wb)
    {
        var r = (((a >> 16) & 255) * (uint)wa + ((b >> 16) & 255) * (uint)wb) / 3;
        var g = (((a >> 8) & 255) * (uint)wa + ((b >> 8) & 255) * (uint)wb) / 3;
        var bl = ((a & 255) * (uint)wa + (b & 255) * (uint)wb) / 3;
        return bl | (g << 8) | (r << 16);
    }

    public static float MinimapViewRadius(float zoom, float worldW, float worldH)
    {
        zoom = Math.Clamp(zoom, 20, 100);
        var map = Math.Max(worldW, worldH);
        if (map < 1000)
            map = 102400f;
        var close = Math.Clamp(map * 0.10f, 14000f, 28000f);
        var wide = Math.Clamp(map * 0.38f, 36000f, 80000f);
        if (wide < close + 8000f)
            wide = close + 8000f;
        var t = (zoom - 20f) / 80f;
        return close + t * (wide - close);
    }

    public string ToCroppedBmpUrl(float worldX, float worldY, float viewRadius, float worldW = 0, float worldH = 0, int outSize = 256)
    {
        if (!HasImage)
            return "";
        outSize = Math.Clamp(outSize, 64, 512);
        var pixels = new byte[outSize * outSize * 4];
        var w = worldW >= 1000 ? worldW : WorldW;
        var h = worldH >= 1000 ? worldH : WorldH;
        var hasWorld = w >= 1000 && h >= 1000;
        if (worldY < 0)
            worldY = -worldY;
        viewRadius = Math.Max(200, viewRadius);
        var cx = hasWorld ? worldX / w * (Width - 1) : Width * 0.5f;
        var cy = hasWorld ? worldY / h * (Height - 1) : Height * 0.5f;
        var halfX = hasWorld ? Math.Max(4f, viewRadius / w * Width) : Math.Max(4f, Width * viewRadius / 102400f);
        var halfY = hasWorld ? Math.Max(4f, viewRadius / h * Height) : Math.Max(4f, Height * viewRadius / 102400f);
        for (var y = 0; y < outSize; y++)
        {
            var v = (y + 0.5f) / outSize * 2f - 1f;
            for (var x = 0; x < outSize; x++)
            {
                var u = (x + 0.5f) / outSize * 2f - 1f;
                var sx = (int)Math.Clamp(cx + u * halfX, 0, Width - 1);
                var sy = (int)Math.Clamp(cy + v * halfY, 0, Height - 1);
                var src = (sy * Width + sx) * 4;
                var dst = (y * outSize + x) * 4;
                pixels[dst] = Bgra[src];
                pixels[dst + 1] = Bgra[src + 1];
                pixels[dst + 2] = Bgra[src + 2];
                pixels[dst + 3] = 255;
            }
        }

        return "data:image/bmp;base64," + Convert.ToBase64String(ToBmp(pixels, outSize, outSize));
    }

    public static byte[] ToBmp(byte[] bgra, int width, int height)
    {
        var row = (width * 4 + 3) & ~3;
        var pixels = new byte[row * height];
        for (var y = 0; y < height; y++)
        {
            var src = (height - 1 - y) * width * 4;
            Buffer.BlockCopy(bgra, src, pixels, y * row, width * 4);
        }

        var file = 14 + 40 + pixels.Length;
        using var ms = new MemoryStream(file);
        using var w = new BinaryWriter(ms, Encoding.ASCII, true);
        w.Write((byte)'B');
        w.Write((byte)'M');
        w.Write(file);
        w.Write(0);
        w.Write(54);
        w.Write(40);
        w.Write(width);
        w.Write(height);
        w.Write((short)1);
        w.Write((short)32);
        w.Write(0);
        w.Write(pixels.Length);
        w.Write(0);
        w.Write(0);
        w.Write(0);
        w.Write(0);
        w.Write(pixels);
        return ms.ToArray();
    }
}
