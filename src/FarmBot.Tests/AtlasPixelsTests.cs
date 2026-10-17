using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class AtlasPixelsTests
{
    private const string AtlasDds = @"E:\TeraBoxDownload\1.Client\pack\yw_etc\ymir work\ui\map_a2_atlas.dds";
    private const string ClientDir = @"E:\TeraBoxDownload\1.Client";

    [Fact]
    public void DecodesMapA2AtlasDds()
    {
        Assert.True(File.Exists(AtlasDds), AtlasDds);
        var bytes = File.ReadAllBytes(AtlasDds);
        Assert.True(AtlasPixels.TryDecodeDds(bytes, out var width, out var height, out var bgra));
        Assert.Equal(256, width);
        Assert.Equal(256, height);
        Assert.Equal(256 * 256 * 4, bgra.Length);
        Assert.Contains(bgra, b => b != 0);
    }

    [Fact]
    public void LoadsAtlasFromClientFolder()
    {
        Assert.True(Directory.Exists(ClientDir), ClientDir);
        Assert.True(AtlasPixels.TryLoadFromClient(ClientDir, "map_a2", 102400, 102400, out var atlas));
        Assert.True(atlas.HasImage);
        Assert.True(atlas.Width is >= 32 and <= 2048);
        Assert.True(atlas.WorldW >= 100000);
        Assert.True(AtlasPixels.TryWorldFromPointFile(ClientDir, "map_a2", out var w, out var h));
        Assert.True(w >= 147200);
        Assert.True(h >= 145500);
        var tight = atlas.ToCroppedBmpUrl(64000, 73000, 16000, w, h);
        var wide = atlas.ToCroppedBmpUrl(64000, 73000, 50000, w, h);
        Assert.NotEqual(tight, wide);
        Assert.InRange(AtlasPixels.MinimapViewRadius(w, h), 28000f, 36000f);
        Assert.True(AtlasPixels.MinimapViewRadius(w, h) < Math.Max(w, h) * 0.45f);
        Assert.Equal(280f, AtlasPixels.RadarUnitsPerPixel);
        Assert.True(AtlasPixels.TryWorldToMap(76800, 38400, 153600, 153600, 240, 240, out var mx, out var my));
        Assert.Equal(120f, mx, 1);
        Assert.Equal(60f, my, 1);
    }

    [Fact]
    public void ResolvesCurrentMapFromDumpCache()
    {
        var root = AtlasPixels.CacheRoot;
        Directory.CreateDirectory(root);
        var current = Path.Combine(root, "current.txt");
        string? previous = null;
        if (File.Exists(current))
            previous = File.ReadAllText(current);
        File.WriteAllText(current, "maps/map_a2\r\n");
        try
        {
            Assert.True(AtlasPixels.TryResolveMap("", out var map, out _, out _));
            Assert.Equal("map_a2", map);
            Assert.True(AtlasPixels.TryResolveMap("metin2_map_a2", out map, out _, out _));
            Assert.Equal("metin2_map_a2", map);
        }
        finally
        {
            try
            {
                if (previous != null)
                    File.WriteAllText(current, previous);
                else if (File.Exists(current))
                    File.Delete(current);
            }
            catch (IOException)
            {
            }
        }
    }

    [Fact]
    public void ParsesAtlasInfoAndAliases()
    {
        Assert.Equal("map_a2", AtlasPixels.SanitizeMapFolder(@"maps/map_a2"));
        Assert.Contains("metin2_map_a2", AtlasPixels.MapAliases("map_a2"));
        Assert.Contains("map_a2", AtlasPixels.MapAliases("metin2_map_a2"));
        var rows = AtlasPixels.ParseAtlasInfo(
            "metin2_map_a1\tYongan\t0\t0\t4\t5\n" +
            "map_a2 64000 0 4 4\n" +
            "# skip\n" +
            "bad line\n");
        Assert.Equal(2, rows.Count);
        Assert.Equal("metin2_map_a1", rows[0].Directory);
        Assert.Equal(4, rows[0].Width);
        Assert.Equal(5, rows[0].Height);
        Assert.Equal("map_a2", rows[1].Directory);
        Assert.Equal(4, rows[1].Width);
    }

    [Fact]
    public void LoadsAtlasFromDumpCache()
    {
        var root = AtlasPixels.CacheRoot;
        var dir = AtlasPixels.CacheDir("map_cache_test");
        var info = Path.Combine(root, "atlasinfo.txt");
        string? previous = null;
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(root);
        if (File.Exists(info))
            previous = File.ReadAllText(info);
        File.WriteAllText(info, "map_cache_test\tTest\t0\t0\t2\t1\n");
        File.WriteAllBytes(Path.Combine(dir, "map_cache_test_atlas.dds"), UncompressedDds(64, 64, 40, 80, 120));
        try
        {
            Assert.True(AtlasPixels.TryWorldFromAtlasInfo(root, "map_cache_test", out var w, out var h));
            Assert.Equal(51200f, w);
            Assert.Equal(25600f, h);
            Assert.True(AtlasPixels.TryLoadFromClient("", "map_cache_test", 0, 0, out var atlas));
            Assert.True(atlas.HasImage);
            Assert.Equal(64, atlas.Width);
            Assert.Equal(64, atlas.Height);
            Assert.Equal(51200f, atlas.WorldW);
            var full = atlas.ToFullBmpUrl();
            Assert.StartsWith("data:image/bmp;base64,", full);
            Assert.Equal(full, atlas.ToFullBmpUrl());
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch (IOException) { }
            try
            {
                if (previous != null)
                    File.WriteAllText(info, previous);
                else if (File.Exists(info))
                    File.Delete(info);
            }
            catch (IOException) { }
        }
    }

    [Fact]
    public void AssemblesMinimapTiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-tile-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "000000"));
        Directory.CreateDirectory(Path.Combine(dir, "001000"));
        File.WriteAllBytes(Path.Combine(dir, "000000", "minimap.dds"), UncompressedDds(256, 256, 10, 20, 30));
        File.WriteAllBytes(Path.Combine(dir, "001000", "minimap.dds"), UncompressedDds(256, 256, 200, 10, 10));
        try
        {
            Assert.True(AtlasPixels.TryAssembleTiles(dir, "map_a2", 0, 0, out var atlas));
            Assert.Equal(512, atlas.Width);
            Assert.Equal(256, atlas.Height);
            Assert.Equal(51200f, atlas.WorldW);
            Assert.Equal(25600f, atlas.WorldH);
            Assert.Equal(10, atlas.Bgra[0]);
            Assert.Equal(200, atlas.Bgra[256 * 4]);
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch (IOException) { }
        }
    }

    private static byte[] UncompressedDds(int width, int height, byte b, byte g, byte r)
    {
        var bytes = new byte[128 + width * height * 4];
        bytes[0] = (byte)'D';
        bytes[1] = (byte)'D';
        bytes[2] = (byte)'S';
        bytes[3] = (byte)' ';
        BitConverter.GetBytes(124).CopyTo(bytes, 4);
        BitConverter.GetBytes(height).CopyTo(bytes, 12);
        BitConverter.GetBytes(width).CopyTo(bytes, 16);
        for (var i = 0; i < width * height; i++)
        {
            var o = 128 + i * 4;
            bytes[o] = b;
            bytes[o + 1] = g;
            bytes[o + 2] = r;
            bytes[o + 3] = 255;
        }

        return bytes;
    }
}
