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
    public void RejectsXorOfEmptyTable()
    {
        var empty = PlayerHudDecoder.XorOfZero;
        Assert.False(PlayerHudDecoder.LooksLikeHud(empty, empty, empty, empty, empty, empty, empty));
    }
}
