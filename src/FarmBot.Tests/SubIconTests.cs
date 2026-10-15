using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class SubIconTests
{
    [Fact]
    public void ParsesOfficialSubImageText()
    {
        const string text = """
            title subimage
            version 1.0
            image shaman_skill.dds
            left 32
            top 0
            right 64
            bottom 32
            """;
        Assert.True(SubIcon.TryParse(text, out var image, out var left, out var top, out var right, out var bottom));
        Assert.Equal("shaman_skill.dds", image);
        Assert.Equal(32, left);
        Assert.Equal(0, top);
        Assert.Equal(64, right);
        Assert.Equal(32, bottom);
    }

    [Fact]
    public void CropsDdsAtlasUsingSubCoordinates()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-sub-tests");
        Directory.CreateDirectory(dir);
        var atlas = Path.Combine(dir, "shaman_skill.dds");
        var sub = Path.Combine(dir, "skill-106.sub");
        File.WriteAllBytes(atlas, UncompressedDds(8, 8, Checker()));
        File.WriteAllText(sub, """
            title subimage
            image shaman_skill.dds
            left 2
            top 2
            right 6
            bottom 6
            """);
        Assert.True(SubIcon.TryLoad(sub, out var icon));
        Assert.Equal(4, icon.Width);
        Assert.Equal(4, icon.Height);
        Assert.Equal(80, icon.Bgra[0]);
        Assert.Equal(60, icon.Bgra[1]);
        Assert.Equal(180, icon.Bgra[2]);
        var uri = ClientIconStore.DataUri(sub, 106);
        Assert.StartsWith("data:image/png;base64,", uri);
    }

    [Fact]
    public void DecodesDxt1Block()
    {
        var bytes = Dxt1Red(4, 4);
        Assert.True(DdsImage.TryDecode(bytes, out var image));
        Assert.Equal(4, image.Width);
        Assert.Equal(4, image.Height);
        Assert.True(image.Bgra[2] > 200);
        Assert.True(image.Bgra[1] < 40);
        Assert.True(image.Bgra[0] < 40);
    }

    private static byte[] Checker()
    {
        var pixels = new byte[8 * 8 * 4];
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            var o = (y * 8 + x) * 4;
            var inside = x is >= 2 and < 6 && y is >= 2 and < 6;
            pixels[o] = inside ? (byte)(40 + x * 20) : (byte)0;
            pixels[o + 1] = inside ? (byte)(y * 30) : (byte)0;
            pixels[o + 2] = inside ? (byte)180 : (byte)255;
            pixels[o + 3] = 255;
        }

        return pixels;
    }

    private static byte[] UncompressedDds(int width, int height, byte[] bgra)
    {
        var bytes = new byte[128 + bgra.Length];
        bytes[0] = (byte)'D';
        bytes[1] = (byte)'D';
        bytes[2] = (byte)'S';
        bytes[3] = (byte)' ';
        BitConverter.GetBytes(124).CopyTo(bytes, 4);
        BitConverter.GetBytes(height).CopyTo(bytes, 12);
        BitConverter.GetBytes(width).CopyTo(bytes, 16);
        BitConverter.GetBytes(32).CopyTo(bytes, 76);
        BitConverter.GetBytes(0x41).CopyTo(bytes, 80);
        BitConverter.GetBytes(32).CopyTo(bytes, 88);
        BitConverter.GetBytes(0x00FF0000).CopyTo(bytes, 92);
        BitConverter.GetBytes(0x0000FF00).CopyTo(bytes, 96);
        BitConverter.GetBytes(0x000000FF).CopyTo(bytes, 100);
        BitConverter.GetBytes(0xFF000000).CopyTo(bytes, 104);
        Buffer.BlockCopy(bgra, 0, bytes, 128, bgra.Length);
        return bytes;
    }

    private static byte[] Dxt1Red(int width, int height)
    {
        var blocksX = (width + 3) / 4;
        var blocksY = (height + 3) / 4;
        var bytes = new byte[128 + blocksX * blocksY * 8];
        bytes[0] = (byte)'D';
        bytes[1] = (byte)'D';
        bytes[2] = (byte)'S';
        bytes[3] = (byte)' ';
        BitConverter.GetBytes(124).CopyTo(bytes, 4);
        BitConverter.GetBytes(height).CopyTo(bytes, 12);
        BitConverter.GetBytes(width).CopyTo(bytes, 16);
        BitConverter.GetBytes(32).CopyTo(bytes, 76);
        BitConverter.GetBytes(4).CopyTo(bytes, 80);
        BitConverter.GetBytes(0x31545844).CopyTo(bytes, 84);
        for (var i = 128; i < bytes.Length; i += 8)
        {
            bytes[i] = 0x00;
            bytes[i + 1] = 0xF8;
            bytes[i + 2] = 0x00;
            bytes[i + 3] = 0xF8;
        }

        return bytes;
    }
}
