using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class PotionControllerTests
{
    [Fact]
    public void UsesSelectedRedWhenHpAtOrBelowPercent()
    {
        var pots = new[] { Hp(27001, 10, 3) };
        var config = Fixtures.Config();
        config.PotionHpPercent = 75;
        config.PotionEnabledVnums = new List<uint> { 27001 };
        var player = Fixtures.Player(hp: 750, maxHp: 1000);
        var next = new PotionController().NextSlot(pots, player, config, 0, 0, 5_000);
        Assert.Equal(3, next);
    }

    [Fact]
    public void DoesNotUseRedWhenHpAbovePercent()
    {
        var pots = new[] { Hp(27001, 10, 3) };
        var config = Fixtures.Config();
        config.PotionHpPercent = 75;
        config.PotionEnabledVnums = new List<uint> { 27001 };
        var player = Fixtures.Player(hp: 800, maxHp: 1000);
        Assert.Null(new PotionController().NextSlot(pots, player, config, 0, 0, 5_000));
    }

    [Fact]
    public void UsesSelectedBlueWhenMpAtOrBelowPercent()
    {
        var pots = new[] { Mp(27004, 8, 5) };
        var config = Fixtures.Config();
        config.PotionMpPercent = 50;
        config.PotionEnabledVnums = new List<uint> { 27004 };
        var player = Fixtures.Player(hp: 1000, maxHp: 1000) with { Mp = 50, MaxMp = 200 };
        Assert.Equal(5, new PotionController().NextSlot(pots, player, config, 0, 0, 5_000));
    }

    [Fact]
    public void ReusesGreenWhenAffectExpired()
    {
        var pots = new[] { Speed(50821, PotionFamily.Green, 20, 2) };
        var config = Fixtures.Config();
        config.PotionEnabledVnums = new List<uint> { 50821 };
        var ctl = new PotionController();
        var player = Fixtures.Player(hp: 1000, maxHp: 1000) with { Mp = 200, MaxMp = 200 };
        Assert.Equal(2, ctl.NextSlot(pots, player, config, 0, 0, 1_000));
        Assert.Null(ctl.NextSlot(pots, player, config, 0, 0, 1_500));
        Assert.Equal(2, ctl.NextSlot(pots, player, config, 0, 0, 1_000 + 20_000));
    }

    [Fact]
    public void UsesAnyPotWhenNoneAreToggledOff()
    {
        var pots = new[] { Hp(27001, 10, 3) };
        var config = Fixtures.Config();
        config.PotionHpPercent = 75;
        config.PotionEnabledVnums = new List<uint>();
        var player = Fixtures.Player(hp: 750, maxHp: 1000);
        Assert.Equal(3, new PotionController().NextSlot(pots, player, config, 0, 0, 5_000));
    }

    [Fact]
    public void IgnoresUnselectedPots()
    {
        var pots = new[] { Hp(27001, 10, 1), Hp(27003, 10, 2) };
        var config = Fixtures.Config();
        config.PotionHpPercent = 100;
        config.PotionEnabledVnums = new List<uint> { 27003 };
        var player = Fixtures.Player(hp: 10, maxHp: 1000);
        Assert.Equal(2, new PotionController().NextSlot(pots, player, config, 0, 0, 5_000));
    }

    private static PotionStack Hp(uint vnum, int count, int slot) =>
        new() { Vnum = vnum, Name = "Red Potion", Count = count, Slot = slot, Family = PotionFamily.Hp };

    private static PotionStack Mp(uint vnum, int count, int slot) =>
        new() { Vnum = vnum, Name = "Blue Potion", Count = count, Slot = slot, Family = PotionFamily.Mp };

    private static PotionStack Speed(uint vnum, PotionFamily family, int duration, int slot) =>
        new()
        {
            Vnum = vnum,
            Name = family == PotionFamily.Green ? "Green Potion" : "Purple Potion",
            Count = 5,
            Slot = slot,
            Family = family,
            DurationSec = duration
        };
}
