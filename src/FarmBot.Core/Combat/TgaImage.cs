using System.Buffers.Binary;
using System.IO.Compression;

namespace FarmBot.Core.Combat;

public sealed class TgaImage
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte[] Bgra { get; init; } = Array.Empty<byte>();

    public bool HasPixels => Width > 0 && Height > 0 && Bgra.Length >= Width * Height * 4;

    public static bool TryLoadFile(string path, out TgaImage image)
    {
        image = new TgaImage();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;
        try
        {
            return TryDecode(File.ReadAllBytes(path), out image);
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static bool TryDecode(ReadOnlySpan<byte> bytes, out TgaImage image)
    {
        image = new TgaImage();
        if (bytes.Length >= 2 && bytes[0] == (byte)'B' && bytes[1] == (byte)'M')
            return TryDecodeBmp(bytes, out image);
        if (bytes.Length >= 4 && bytes[0] == (byte)'D' && bytes[1] == (byte)'D' &&
            bytes[2] == (byte)'S' && bytes[3] == (byte)' ')
            return DdsImage.TryDecode(bytes, out image);
        if (bytes.Length >= 8 && bytes[1] == 'P' && bytes[2] == 'N' && bytes[3] == 'G')
            return false;
        return TryDecodeTga(bytes, out image);
    }

    public TgaImage Crop(int left, int top, int right, int bottom)
    {
        if (!HasPixels)
            return this;
        left = Math.Clamp(left, 0, Width);
        top = Math.Clamp(top, 0, Height);
        right = Math.Clamp(right, left, Width);
        bottom = Math.Clamp(bottom, top, Height);
        var w = right - left;
        var h = bottom - top;
        if (w <= 0 || h <= 0)
            return new TgaImage();
        var pixels = new byte[w * h * 4];
        for (var y = 0; y < h; y++)
            Buffer.BlockCopy(Bgra, ((top + y) * Width + left) * 4, pixels, y * w * 4, w * 4);
        return new TgaImage { Width = w, Height = h, Bgra = pixels };
    }

    public string ToPngDataUri()
    {
        if (!HasPixels)
            return "";
        return "data:image/png;base64," + Convert.ToBase64String(ToPng());
    }

    public byte[] ToPng()
    {
        if (!HasPixels)
            return Array.Empty<byte>();
        var raw = new byte[Height * (1 + Width * 4)];
        for (var y = 0; y < Height; y++)
        {
            var src = y * Width * 4;
            var dst = y * (1 + Width * 4);
            raw[dst] = 0;
            for (var x = 0; x < Width; x++)
            {
                var i = src + x * 4;
                var o = dst + 1 + x * 4;
                raw[o] = Bgra[i + 2];
                raw[o + 1] = Bgra[i + 1];
                raw[o + 2] = Bgra[i];
                raw[o + 3] = Bgra[i + 3];
            }
        }

        var zlib = Zlib(raw);
        using var ms = new MemoryStream(80 + zlib.Length);
        using var w = new BinaryWriter(ms);
        w.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        WriteChunk(w, "IHDR", Ihdr(Width, Height));
        WriteChunk(w, "IDAT", zlib);
        WriteChunk(w, "IEND", Array.Empty<byte>());
        return ms.ToArray();
    }

    public static string DataUriFromFile(string? path)
    {
        if (!TryLoadFile(path ?? "", out var image) || !image.HasPixels)
            return "";
        return image.ToPngDataUri();
    }

    private static bool TryDecodeTga(ReadOnlySpan<byte> bytes, out TgaImage image)
    {
        image = new TgaImage();
        if (bytes.Length < 18)
            return false;
        var idLen = bytes[0];
        var colorMap = bytes[1];
        var type = bytes[2];
        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(12));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(14));
        var bpp = bytes[16];
        var desc = bytes[17];
        if (colorMap != 0 || width is < 1 or > 512 || height is < 1 or > 512)
            return false;
        if (type is not (2 or 10) || bpp is not (16 or 24 or 32))
            return false;
        var pixels = new byte[width * height * 4];
        var src = 18 + idLen;
        if (src > bytes.Length)
            return false;
        var ok = type == 2
            ? ReadUncompressed(bytes.Slice(src), pixels, width, height, bpp)
            : ReadRle(bytes.Slice(src), pixels, width, height, bpp);
        if (!ok)
            return false;
        if ((desc & 0x20) == 0)
            FlipVertical(pixels, width, height);
        image = new TgaImage { Width = width, Height = height, Bgra = pixels };
        return true;
    }

    private static bool TryDecodeBmp(ReadOnlySpan<byte> bytes, out TgaImage image)
    {
        image = new TgaImage();
        if (bytes.Length < 54)
            return false;
        var off = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(10));
        var width = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(18));
        var height = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(22));
        var bpp = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(28));
        if (width is < 1 or > 512 || height is < 1 or > 512 || bpp is not (24 or 32) || off < 54)
            return false;
        var srcH = height;
        var flip = srcH > 0;
        if (srcH < 0)
            srcH = -srcH;
        var stride = ((width * bpp + 31) / 32) * 4;
        if (off + stride * srcH > bytes.Length)
            return false;
        var pixels = new byte[width * srcH * 4];
        for (var y = 0; y < srcH; y++)
        {
            var row = flip ? srcH - 1 - y : y;
            var src = off + row * stride;
            for (var x = 0; x < width; x++)
            {
                var i = (y * width + x) * 4;
                var p = src + x * (bpp / 8);
                pixels[i] = bytes[p];
                pixels[i + 1] = bytes[p + 1];
                pixels[i + 2] = bytes[p + 2];
                pixels[i + 3] = bpp == 32 ? bytes[p + 3] : (byte)255;
            }
        }

        image = new TgaImage { Width = width, Height = srcH, Bgra = pixels };
        return true;
    }

    private static bool ReadUncompressed(ReadOnlySpan<byte> src, byte[] dest, int width, int height, int bpp)
    {
        var step = bpp / 8;
        var need = width * height * step;
        if (src.Length < need)
            return false;
        var i = 0;
        for (var p = 0; p < need; p += step)
            WritePixel(dest, ref i, src.Slice(p, step), bpp);
        return true;
    }

    private static bool ReadRle(ReadOnlySpan<byte> src, byte[] dest, int width, int height, int bpp)
    {
        var step = bpp / 8;
        var i = 0;
        var o = 0;
        var count = width * height;
        while (i < count && o < src.Length)
        {
            var packet = src[o++];
            var run = (packet & 0x7F) + 1;
            if ((packet & 0x80) != 0)
            {
                if (o + step > src.Length)
                    return false;
                var pixel = src.Slice(o, step);
                o += step;
                for (var n = 0; n < run && i < count; n++)
                    WritePixel(dest, ref i, pixel, bpp);
            }
            else
            {
                if (o + run * step > src.Length)
                    return false;
                for (var n = 0; n < run && i < count; n++)
                {
                    WritePixel(dest, ref i, src.Slice(o, step), bpp);
                    o += step;
                }
            }
        }

        return i == count;
    }

    private static void WritePixel(byte[] dest, ref int i, ReadOnlySpan<byte> src, int bpp)
    {
        var o = i * 4;
        if (bpp == 16)
        {
            var v = (ushort)(src[0] | src[1] << 8);
            dest[o] = (byte)((v & 0x1F) * 255 / 31);
            dest[o + 1] = (byte)((v >> 5 & 0x1F) * 255 / 31);
            dest[o + 2] = (byte)((v >> 10 & 0x1F) * 255 / 31);
            dest[o + 3] = (v & 0x8000) != 0 ? (byte)255 : (byte)0;
        }
        else if (bpp == 24)
        {
            dest[o] = src[0];
            dest[o + 1] = src[1];
            dest[o + 2] = src[2];
            dest[o + 3] = 255;
        }
        else
        {
            dest[o] = src[0];
            dest[o + 1] = src[1];
            dest[o + 2] = src[2];
            dest[o + 3] = src.Length > 3 ? src[3] : (byte)255;
        }

        i++;
    }

    private static void FlipVertical(byte[] pixels, int width, int height)
    {
        var row = width * 4;
        var tmp = new byte[row];
        for (var y = 0; y < height / 2; y++)
        {
            var a = y * row;
            var b = (height - 1 - y) * row;
            Buffer.BlockCopy(pixels, a, tmp, 0, row);
            Buffer.BlockCopy(pixels, b, pixels, a, row);
            Buffer.BlockCopy(tmp, 0, pixels, b, row);
        }
    }

    private static byte[] Ihdr(int width, int height)
    {
        var b = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(b, width);
        BinaryPrimitives.WriteInt32BigEndian(b.AsSpan(4), height);
        b[8] = 8;
        b[9] = 6;
        return b;
    }

    private static void WriteChunk(BinaryWriter w, string type, byte[] data)
    {
        var name = System.Text.Encoding.ASCII.GetBytes(type);
        var crcSrc = new byte[name.Length + data.Length];
        Buffer.BlockCopy(name, 0, crcSrc, 0, 4);
        Buffer.BlockCopy(data, 0, crcSrc, 4, data.Length);
        Span<byte> len = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(len, data.Length);
        w.Write(len);
        w.Write(name);
        w.Write(data);
        BinaryPrimitives.WriteInt32BigEndian(len, (int)Crc32(crcSrc));
        w.Write(len);
    }

    private static byte[] Zlib(byte[] raw)
    {
        using var ms = new MemoryStream();
        ms.WriteByte(0x78);
        ms.WriteByte(0x01);
        using (var def = new DeflateStream(ms, CompressionLevel.Fastest, true))
            def.Write(raw, 0, raw.Length);
        var adler = Adler32(raw);
        ms.WriteByte((byte)(adler >> 24));
        ms.WriteByte((byte)(adler >> 16));
        ms.WriteByte((byte)(adler >> 8));
        ms.WriteByte((byte)adler);
        return ms.ToArray();
    }

    private static uint Adler32(byte[] data)
    {
        uint a = 1, b = 0;
        foreach (var v in data)
        {
            a = (a + v) % 65521;
            b = (b + a) % 65521;
        }

        return b << 16 | a;
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var v in data)
            crc = CrcTable[(crc ^ v) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFFu;
    }

    private static readonly uint[] CrcTable = CreateCrc();

    private static uint[] CreateCrc()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var c = i;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            table[i] = c;
        }

        return table;
    }
}
