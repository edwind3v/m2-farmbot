using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;

namespace FarmBot.Platform.Client;

internal static class MiniMapAtlasReader
{
    private const int AtlasMax = 1024;
    private const int MiniScan = 0x600;
    private const int MaxPixelReads = 8;
    private static readonly uint[] ImageOffs =
    {
        0x26C + 0x1C,
        0x328 + 0x1C,
        0x398 + 0x1C,
        0x2CC
    };

    public static bool TryCopy(ProcessMemory memory, float worldW, float worldH, out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        var baerim = SlotStore.IsBaerimPath(memory.ImagePath);
        var miniMapRva = baerim ? 0x3C184A8u : 0x482BA8u;
        var worldOff = baerim ? 0x418u : 0x328u;
        if (!memory.TryReadU32(memory.ImageBase + miniMapRva, out var mini) || !IsUser(mini))
            return false;
        if (memory.TryReadF32(mini + worldOff, out var mw) &&
            memory.TryReadF32(mini + worldOff + 4, out var mh) &&
            mw is >= 10000f and <= 500000f && mh is >= 10000f and <= 500000f)
        {
            if (worldW < 1000)
                worldW = mw;
            if (worldH < 1000)
                worldH = mh;
        }

        if (!memory.TryRead(mini, MiniScan, out var blob) || blob.Length < 0x80)
            return false;

        Candidate? best = null;
        var pixelReads = 0;
        void consider(int width, int height, byte[] bgra)
        {
            if (!LooksLikeSize(width, height) || bgra.Length < width * height * 4 || !HasTexture(bgra))
                return;
            var next = new Candidate(width, height, bgra);
            if (best == null || Score(next, worldW) > Score(best, worldW))
                best = next;
        }

        foreach (var off in ImageOffs)
        {
            if (off + 4 > (uint)blob.Length)
                continue;
            var image = BitConverter.ToUInt32(blob, (int)off);
            if (TryCopyCpu(memory, image, ref pixelReads, out var width, out var height, out var bgra))
                consider(width, height, bgra);
        }

        if (TryCopyCpu(memory, mini, ref pixelReads, out var miniW, out var miniH, out var miniBgra))
            consider(miniW, miniH, miniBgra);

        for (var i = 0; i + 4 <= blob.Length; i += 4)
        {
            var ptr = BitConverter.ToUInt32(blob, i);
            if (!IsUser(ptr))
                continue;
            if (TryCopyCpu(memory, ptr, ref pixelReads, out var width, out var height, out var bgra))
                consider(width, height, bgra);
            if (TryCopyDds(memory, ptr, out width, out height, out bgra))
                consider(width, height, bgra);
            if (memory.TryReadU32(ptr + 0x1C, out var image) &&
                TryCopyCpu(memory, image, ref pixelReads, out width, out height, out bgra))
                consider(width, height, bgra);
        }

        if (best == null)
            return false;
        if (worldW < 1000)
            worldW = best.Width * 100f;
        if (worldH < 1000)
            worldH = best.Height * 100f;
        atlas = new AtlasPixels
        {
            Width = best.Width,
            Height = best.Height,
            WorldW = worldW,
            WorldH = worldH,
            Bgra = best.Pixels
        };
        return atlas.HasImage;
    }

    private static bool TryCopyCpu(
        ProcessMemory memory,
        uint image,
        ref int pixelReads,
        out int width,
        out int height,
        out byte[] bgra)
    {
        width = 0;
        height = 0;
        bgra = Array.Empty<byte>();
        if (!IsAlignedUser(image) || pixelReads >= MaxPixelReads)
            return false;
        if (!memory.TryRead(image, 0x180, out var obj) || obj.Length < 0x70)
            return false;

        var rectW = BitConverter.ToInt32(obj, 0x68) - BitConverter.ToInt32(obj, 0x60);
        var rectH = BitConverter.ToInt32(obj, 0x6C) - BitConverter.ToInt32(obj, 0x64);
        if (LooksLikeSize(rectW, rectH))
        {
            var n = Math.Min(0x80, obj.Length) & ~3;
            for (var i = 0; i + 4 <= n; i += 4)
            {
                var bits = BitConverter.ToUInt32(obj, i);
                if (TryReadPixels(memory, bits, rectW, rectH, ref pixelReads, out bgra))
                {
                    width = rectW;
                    height = rectH;
                    return true;
                }
            }
        }

        for (var i = 0; i + 12 <= obj.Length; i += 4)
        {
            var w = BitConverter.ToInt32(obj, i);
            var h = BitConverter.ToInt32(obj, i + 4);
            if (!LooksLikeSize(w, h))
                continue;
            var bits = BitConverter.ToUInt32(obj, i + 8);
            if (!TryReadPixels(memory, bits, w, h, ref pixelReads, out bgra))
                continue;
            width = w;
            height = h;
            return true;
        }

        return false;
    }

    private static bool TryReadPixels(
        ProcessMemory memory,
        uint bits,
        int width,
        int height,
        ref int pixelReads,
        out byte[] bgra)
    {
        bgra = Array.Empty<byte>();
        if (!IsUser(bits) || pixelReads >= MaxPixelReads)
            return false;
        pixelReads++;
        if (TryCopyDds(memory, bits, out var ddsW, out var ddsH, out bgra) && ddsW == width && ddsH == height)
            return true;
        var bgraLen = width * height * 4;
        if (memory.TryRead(bits, bgraLen, out var raw) && raw.Length == bgraLen)
        {
            bgra = raw;
            return true;
        }

        var rgb565 = width * height * 2;
        if (memory.TryRead(bits, rgb565, out var packed) && packed.Length == rgb565)
        {
            bgra = FromRgb565(packed, width, height);
            return true;
        }

        var dxt1 = ((width + 3) / 4) * ((height + 3) / 4) * 8;
        if (memory.TryRead(bits, dxt1, out var dxt) &&
            dxt.Length == dxt1 &&
            AtlasPixels.TryDecodeDds(WrapDds(width, height, 0x31545844, dxt), out _, out _, out bgra))
            return true;

        var dxt5 = ((width + 3) / 4) * ((height + 3) / 4) * 16;
        return memory.TryRead(bits, dxt5, out var dxtAlpha) &&
               dxtAlpha.Length == dxt5 &&
               AtlasPixels.TryDecodeDds(WrapDds(width, height, 0x35545844, dxtAlpha), out _, out _, out bgra);
    }

    private static bool TryCopyDds(ProcessMemory memory, uint address, out int width, out int height, out byte[] bgra)
    {
        width = 0;
        height = 0;
        bgra = Array.Empty<byte>();
        if (!IsUser(address) || !memory.TryRead(address, 128, out var header) || header.Length < 128)
            return false;
        if (header[0] != (byte)'D' || header[1] != (byte)'D' || header[2] != (byte)'S')
            return false;
        var h = BitConverter.ToInt32(header, 12);
        var w = BitConverter.ToInt32(header, 16);
        if (!LooksLikeSize(w, h))
            return false;
        var flags = BitConverter.ToUInt32(header, 80);
        var fourcc = BitConverter.ToUInt32(header, 84);
        var payload = w * h * 4;
        if ((flags & 4) != 0 && fourcc == 0x31545844)
            payload = ((w + 3) / 4) * ((h + 3) / 4) * 8;
        else if ((flags & 4) != 0 && fourcc == 0x35545844)
            payload = ((w + 3) / 4) * ((h + 3) / 4) * 16;
        if (payload <= 0 || payload > AtlasMax * AtlasMax * 4)
            return false;
        if (!memory.TryRead(address, 128 + payload, out var bytes) || bytes.Length < 128 + payload)
            return false;
        return AtlasPixels.TryDecodeDds(bytes, out width, out height, out bgra);
    }

    private static byte[] WrapDds(int width, int height, uint fourcc, byte[] payload)
    {
        var bytes = new byte[128 + payload.Length];
        bytes[0] = (byte)'D';
        bytes[1] = (byte)'D';
        bytes[2] = (byte)'S';
        bytes[3] = (byte)' ';
        BitConverter.GetBytes(124).CopyTo(bytes, 4);
        BitConverter.GetBytes(height).CopyTo(bytes, 12);
        BitConverter.GetBytes(width).CopyTo(bytes, 16);
        BitConverter.GetBytes(32).CopyTo(bytes, 76);
        BitConverter.GetBytes(4).CopyTo(bytes, 80);
        BitConverter.GetBytes(fourcc).CopyTo(bytes, 84);
        Buffer.BlockCopy(payload, 0, bytes, 128, payload.Length);
        return bytes;
    }

    private static byte[] FromRgb565(byte[] packed, int width, int height)
    {
        var bgra = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            var pix = BitConverter.ToUInt16(packed, i * 2);
            var d = i * 4;
            bgra[d] = (byte)((pix & 0x1F) << 3);
            bgra[d + 1] = (byte)(((pix >> 5) & 0x3F) << 2);
            bgra[d + 2] = (byte)(((pix >> 11) & 0x1F) << 3);
            bgra[d + 3] = 255;
        }

        return bgra;
    }

    private static bool LooksLikeSize(int width, int height)
    {
        if (width < 64 || height < 64 || width > AtlasMax || height > AtlasMax)
            return false;
        return OkDim(width) && OkDim(height);
    }

    private static bool OkDim(int value) =>
        value is 64 or 128 or 192 or 256 or 384 or 512 or 768 or 1024 || value % 256 == 0;

    private static bool HasTexture(byte[] bgra)
    {
        var colors = new HashSet<int>();
        for (var i = 0; i + 4 <= bgra.Length && colors.Count < 16; i += 64)
            colors.Add(BitConverter.ToInt32(bgra, i));
        return colors.Count >= 8;
    }

    private static int Score(Candidate image, float worldW)
    {
        var expected = worldW >= 1000 ? (int)(worldW / 100f) : 0;
        var area = image.Width * image.Height;
        var square = image.Width == image.Height ? 1_000_000 : 0;
        var close = expected > 0 ? 500_000 - Math.Abs(image.Width - expected) * 2000 : 0;
        return area + square + close;
    }

    private static bool IsUser(uint address) => address >= 0x10000 && address < 0x7FFF0000;

    private static bool IsAlignedUser(uint address) => IsUser(address) && (address & 3) == 0;

    private sealed record Candidate(int Width, int Height, byte[] Pixels);
}
