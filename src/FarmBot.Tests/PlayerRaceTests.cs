using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class PlayerRaceTests
{
    [Theory]
    [InlineData(0u, "warrior", "male", "Warrior")]
    [InlineData(1u, "ninja", "female", "Ninja")]
    [InlineData(5u, "ninja", "male", "Ninja")]
    [InlineData(8u, "lycan", "male", "Lycan")]
    public void Maps_standard_metin2_races(uint race, string classId, string sex, string label)
    {
        Assert.True(PlayerRace.TryMap(race, out var info));
        Assert.Equal(classId, info.ClassId);
        Assert.Equal(sex, info.Sex);
        Assert.Equal(label, info.Label);
        Assert.StartsWith("avatars/", PlayerRace.AvatarUrl(race));
    }

    [Fact]
    public void Unknown_race_has_no_avatar()
    {
        Assert.False(PlayerRace.TryMap(null, out _));
        Assert.False(PlayerRace.TryMap(99, out _));
        Assert.Equal("", PlayerRace.AvatarUrl(null));
        Assert.Equal("", PlayerRace.Caption(99));
    }
}
