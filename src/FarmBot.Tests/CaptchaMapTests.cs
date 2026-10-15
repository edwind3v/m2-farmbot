using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class CaptchaMapTests
{
    [Fact]
    public void LoadsAllFortySevenChallengesWithValidButtons()
    {
        Assert.Equal(47, CaptchaMap.Challenges.Count);
        Assert.Equal(Enumerable.Range(1, 47), CaptchaMap.Challenges.Select(item => item.Id));
        Assert.All(CaptchaMap.Challenges, item =>
        {
            Assert.InRange(item.Button, 0, 8);
            Assert.InRange(item.Bg, 1, 14);
            Assert.False(string.IsNullOrWhiteSpace(item.Question));
        });
    }

    [Fact]
    public void MapsKnownIconAndSceneChallenges()
    {
        Assert.True(CaptchaMap.TryGetButton(1, out var fish));
        Assert.Equal(8, fish);
        Assert.True(CaptchaMap.TryGetButton(34, out var potion));
        Assert.Equal(7, potion);
        Assert.True(CaptchaMap.TryGetButton(2, out var tail));
        Assert.Equal(5, tail);
        Assert.True(CaptchaMap.TryGetButton(23, out var bag));
        Assert.Equal(6, bag);
    }
}
