using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class TgaImageTests
{
    [Fact]
    public void DecodesUncompressed32BitTgaAndRoundtripsPng()
    {
        var tga = UncompressedTga(2, 2, new byte[]
        {
            0, 0, 255, 255,
            0, 255, 0, 255,
            255, 0, 0, 255,
            255, 255, 255, 128
        });
        Assert.True(TgaImage.TryDecode(tga, out var image));
        Assert.Equal(2, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(255, image.Bgra[2]);
        Assert.Equal(255, image.Bgra[4 + 1]);
        var png = image.ToPng();
        Assert.True(png.Length > 32);
        Assert.Equal(0x89, png[0]);
        Assert.Equal((byte)'P', png[1]);
        Assert.StartsWith("data:image/png;base64,", image.ToPngDataUri());
    }

    [Fact]
    public void OpensArthionIconPackIndex()
    {
        const string pack = @"E:\Arthion\game\pack\icon.art";
        if (!File.Exists(pack))
            return;
        Assert.True(ArtPack.TryOpen(pack, out var art));
        using (art)
        {
            Assert.Equal(8253, art.Entries.Count);
            Assert.True(art.Entries[0].Offset > 6);
            Assert.True(art.TryReadStored(art.Entries[0], out var blob));
            Assert.Equal((int)art.Entries[0].StoredSize, blob.Length);
            Assert.False(TgaImage.TryDecode(blob, out _));
        }
    }

    [Fact]
    public void Fnv8MatchesReversedClientUnroll()
    {
        var eight = "icon/ite"u8.ToArray();
        uint h = 0x811C9DC5;
        h = (h ^ eight[0]) * 0x01000193;
        for (var i = 1; i < 8; i++)
            h = (h ^ eight[i]) * 0x01000193;
        Assert.Equal(h, ArtPack.Fnv8(eight));
    }

    private static byte[] UncompressedTga(int width, int height, byte[] bgra)
    {
        var bytes = new byte[18 + bgra.Length];
        bytes[2] = 2;
        bytes[12] = (byte)width;
        bytes[14] = (byte)height;
        bytes[16] = 32;
        bytes[17] = 0x20;
        Buffer.BlockCopy(bgra, 0, bytes, 18, bgra.Length);
        return bytes;
    }
}
