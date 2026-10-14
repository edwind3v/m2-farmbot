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
        Assert.True(atlas.Width is >= 32 and <= 256);
        Assert.True(atlas.WorldW >= 100000);
        Assert.True(AtlasPixels.TryWorldFromPointFile(ClientDir, "map_a2", out var w, out var h));
        Assert.True(w >= 147200);
        Assert.True(h >= 145500);
        var tight = atlas.ToCroppedBmpUrl(64000, 73000, 16000, w, h);
        var wide = atlas.ToCroppedBmpUrl(64000, 73000, 50000, w, h);
        Assert.NotEqual(tight, wide);
        Assert.True(AtlasPixels.MinimapViewRadius(100, w, h) > AtlasPixels.MinimapViewRadius(20, w, h) * 1.5);
    }
}
