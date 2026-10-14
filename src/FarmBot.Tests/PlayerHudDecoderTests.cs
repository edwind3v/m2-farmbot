using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class PlayerHudDecoderTests
{
    [Fact]
    public void AcceptsRealGetStatusPoints()
    {
        Assert.True(PlayerHudDecoder.LooksLikeHud(
            level: 120,
            hp: 7840,
            maxHp: 8120,
            mp: 980,
            maxMp: 1640,
            exp: 185_400,
            nextExp: 2_140_000));
    }

    [Fact]
    public void RejectsTwoSlotShiftWhereExpIsReadAsLevel()
    {
        Assert.False(PlayerHudDecoder.LooksLikeHud(
            level: 165,
            hp: 980,
            maxHp: 1640,
            mp: 40,
            maxMp: 100,
            exp: 7840,
            nextExp: 8120));
    }

    [Fact]
    public void DiscoveryRejectsZeroHpSoUnknownHudIsNotDead()
    {
        Assert.False(PlayerHudDecoder.LooksLikeHud(
            level: 12,
            hp: 0,
            maxHp: 800,
            mp: 100,
            maxMp: 200,
            exp: 10,
            nextExp: 1000,
            allowDead: false));
        Assert.True(PlayerHudDecoder.LooksLikeHud(
            level: 12,
            hp: 0,
            maxHp: 800,
            mp: 100,
            maxMp: 200,
            exp: 10,
            nextExp: 1000,
            allowDead: true));
    }

    [Fact]
    public void RejectsXorOfEmptyTable()
    {
        var empty = PlayerHudDecoder.XorOfZero;
        Assert.False(PlayerHudDecoder.LooksLikeHud(empty, empty, empty, empty, empty, empty, empty));
    }

    [Fact]
    public void AtlasBmpHasBitmapHeader()
    {
        var bgra = new byte[4 * 4 * 4];
        for (var i = 0; i < bgra.Length; i += 4)
        {
            bgra[i] = 40;
            bgra[i + 1] = 80;
            bgra[i + 2] = 120;
            bgra[i + 3] = 255;
        }

        var bmp = AtlasPixels.ToBmp(bgra, 4, 4);
        Assert.Equal((byte)'B', bmp[0]);
        Assert.Equal((byte)'M', bmp[1]);
        Assert.True(bmp.Length > 54);
    }
}
