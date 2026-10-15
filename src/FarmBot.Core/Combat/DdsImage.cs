using System.Buffers.Binary;

namespace FarmBot.Core.Combat;

public static class DdsImage
{
    public static bool TryDecode(ReadOnlySpan<byte> bytes, out TgaImage image)
    {
        image = new TgaImage();
        if (bytes.Length < 128 ||
            bytes[0] != (byte)'D' || bytes[1] != (byte)'D' || bytes[2] != (byte)'S' || bytes[3] != (byte)' ')
            return false;
        var headerSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(4));
        if (headerSize != 124)
            return false;
        var height = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(12));
        var width = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(16));
        if (width is < 1 or > 4096 || height is < 1 or > 4096)
            return false;
        var pfFlags = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(80));
        var fourCc = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(84));
        var bitCount = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(88));
        var dataOff = 128;
        if (fourCc == 0x30315844)
            dataOff = 148;
        if (bytes.Length < dataOff)
            return false;
        var pixels = new byte[width * height * 4];
        var src = bytes.Slice(dataOff);
        var ok = (pfFlags & 4) != 0
            ? TryDecodeDxt(src, pixels, width, height, fourCc)
            : TryDecodeUncompressed(src, pixels, width, height, bitCount, bytes);
        if (!ok)
            return false;
        image = new TgaImage { Width = width, Height = height, Bgra = pixels };
        return true;
    }

    private static bool TryDecodeDxt(ReadOnlySpan<byte> src, byte[] dest, int width, int height, uint fourCc)
    {
        var blocksX = Math.Max(1, (width + 3) / 4);
        var blocksY = Math.Max(1, (height + 3) / 4);
        var blockSize = fourCc == 0x31545844 ? 8 : 16;
        if (src.Length < blocksX * blocksY * blockSize)
            return false;
        var i = 0;
        for (var by = 0; by < blocksY; by++)
        for (var bx = 0; bx < blocksX; bx++, i += blockSize)
        {
            if (fourCc == 0x31545844)
                DecodeDxt1(src.Slice(i, 8), dest, width, height, bx * 4, by * 4);
            else if (fourCc == 0x33545844)
                DecodeDxt3(src.Slice(i, 16), dest, width, height, bx * 4, by * 4);
            else if (fourCc == 0x35545844)
                DecodeDxt5(src.Slice(i, 16), dest, width, height, bx * 4, by * 4);
            else
                return false;
        }

        return true;
    }

    private static bool TryDecodeUncompressed(
        ReadOnlySpan<byte> src, byte[] dest, int width, int height, int bitCount, ReadOnlySpan<byte> header)
    {
        var rMask = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(88 + 4));
        var gMask = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(88 + 8));
        var bMask = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(88 + 12));
        var aMask = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(88 + 16));
        if (bitCount == 32 && rMask == 0 && gMask == 0 && bMask == 0)
        {
            rMask = 0x00FF0000;
            gMask = 0x0000FF00;
            bMask = 0x000000FF;
            aMask = 0xFF000000;
        }

        var step = (bitCount + 7) / 8;
        if (step is < 3 or > 4 || src.Length < width * height * step)
            return false;
        var p = 0;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            uint v = src[p];
            v |= (uint)src[p + 1] << 8;
            v |= (uint)src[p + 2] << 16;
            if (step == 4)
                v |= (uint)src[p + 3] << 24;
            p += step;
            var o = (y * width + x) * 4;
            dest[o] = Channel(v, bMask);
            dest[o + 1] = Channel(v, gMask);
            dest[o + 2] = Channel(v, rMask);
            dest[o + 3] = aMask == 0 ? (byte)255 : Channel(v, aMask);
        }

        return true;
    }

    private static byte Channel(uint value, uint mask)
    {
        if (mask == 0)
            return 0;
        var shift = 0;
        while (((mask >> shift) & 1) == 0)
            shift++;
        var bits = 0;
        var m = mask >> shift;
        while ((m & 1) != 0)
        {
            bits++;
            m >>= 1;
        }

        var n = (value & mask) >> shift;
        return bits >= 8 ? (byte)n : (byte)(n * 255 / ((1 << bits) - 1));
    }

    private static void DecodeDxt1(ReadOnlySpan<byte> block, byte[] dest, int width, int height, int x, int y)
    {
        Span<byte> colors = stackalloc byte[16];
        UnpackColors(block, colors, false);
        var lookup = BinaryPrimitives.ReadUInt32LittleEndian(block.Slice(4));
        WriteBlock(dest, width, height, x, y, colors, lookup, default, default, 0);
    }

    private static void DecodeDxt3(ReadOnlySpan<byte> block, byte[] dest, int width, int height, int x, int y)
    {
        Span<byte> colors = stackalloc byte[16];
        UnpackColors(block.Slice(8), colors, true);
        var lookup = BinaryPrimitives.ReadUInt32LittleEndian(block.Slice(12));
        WriteBlock(dest, width, height, x, y, colors, lookup, block, default, 0);
    }

    private static void DecodeDxt5(ReadOnlySpan<byte> block, byte[] dest, int width, int height, int x, int y)
    {
        Span<byte> colors = stackalloc byte[16];
        UnpackColors(block.Slice(8), colors, true);
        var lookup = BinaryPrimitives.ReadUInt32LittleEndian(block.Slice(12));
        Span<byte> alphas = stackalloc byte[8];
        alphas[0] = block[0];
        alphas[1] = block[1];
        if (alphas[0] > alphas[1])
        {
            for (var i = 1; i <= 6; i++)
                alphas[i + 1] = (byte)(((7 - i) * alphas[0] + i * alphas[1]) / 7);
        }
        else
        {
            for (var i = 1; i <= 4; i++)
                alphas[i + 1] = (byte)(((5 - i) * alphas[0] + i * alphas[1]) / 5);
            alphas[6] = 0;
            alphas[7] = 255;
        }

        ulong alphaBits = block[2];
        alphaBits |= (ulong)block[3] << 8;
        alphaBits |= (ulong)block[4] << 16;
        alphaBits |= (ulong)block[5] << 24;
        alphaBits |= (ulong)block[6] << 32;
        alphaBits |= (ulong)block[7] << 40;
        WriteBlock(dest, width, height, x, y, colors, lookup, default, alphas, alphaBits);
    }

    private static void UnpackColors(ReadOnlySpan<byte> block, Span<byte> colors, bool dxt3Or5)
    {
        var c0 = BinaryPrimitives.ReadUInt16LittleEndian(block);
        var c1 = BinaryPrimitives.ReadUInt16LittleEndian(block.Slice(2));
        Rgb565(c0, colors, 0);
        Rgb565(c1, colors, 4);
        if (c0 > c1 || dxt3Or5)
        {
            Mix(colors, 8, 0, 4, 2, 1);
            Mix(colors, 12, 0, 4, 1, 2);
        }
        else
        {
            Mix(colors, 8, 0, 4, 1, 1);
            colors[12] = 0;
            colors[13] = 0;
            colors[14] = 0;
            colors[15] = 0;
        }
    }

    private static void Rgb565(ushort c, Span<byte> colors, int o)
    {
        colors[o] = (byte)((c & 0x1F) * 255 / 31);
        colors[o + 1] = (byte)(((c >> 5) & 0x3F) * 255 / 63);
        colors[o + 2] = (byte)(((c >> 11) & 0x1F) * 255 / 31);
        colors[o + 3] = 255;
    }

    private static void Mix(Span<byte> colors, int dest, int a, int b, int wa, int wb)
    {
        var t = wa + wb;
        colors[dest] = (byte)((colors[a] * wa + colors[b] * wb) / t);
        colors[dest + 1] = (byte)((colors[a + 1] * wa + colors[b + 1] * wb) / t);
        colors[dest + 2] = (byte)((colors[a + 2] * wa + colors[b + 2] * wb) / t);
        colors[dest + 3] = 255;
    }

    private static void WriteBlock(
        byte[] dest, int width, int height, int x, int y, Span<byte> colors, uint lookup,
        ReadOnlySpan<byte> dxt3Alpha, ReadOnlySpan<byte> dxt5Alpha, ulong dxt5Bits)
    {
        for (var py = 0; py < 4; py++)
        for (var px = 0; px < 4; px++)
        {
            var gx = x + px;
            var gy = y + py;
            if (gx >= width || gy >= height)
                continue;
            var idx = (int)((lookup >> (2 * (py * 4 + px))) & 3);
            var o = (gy * width + gx) * 4;
            dest[o] = colors[idx * 4];
            dest[o + 1] = colors[idx * 4 + 1];
            dest[o + 2] = colors[idx * 4 + 2];
            dest[o + 3] = colors[idx * 4 + 3];
            if (dxt3Alpha.Length >= 8)
            {
                var a = (int)(BinaryPrimitives.ReadUInt16LittleEndian(dxt3Alpha.Slice(py * 2)) >> (px * 4)) & 0xF;
                dest[o + 3] = (byte)(a * 17);
            }
            else if (dxt5Alpha.Length >= 8)
            {
                var a = (int)((dxt5Bits >> (3 * (py * 4 + px))) & 7);
                dest[o + 3] = dxt5Alpha[a];
            }
        }
    }
}
