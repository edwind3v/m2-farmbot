using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;

namespace FarmBot.Tests;

public class SkillControllerTests
{
    [Fact]
    public void DelayZeroFiresImmediatelyThenEvery300Ms()
    {
        var skill = Skill(3, 1);
        var config = Enabled(3, 0);
        var ctl = new SkillController();
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Null(ctl.Next(new[] { skill }, config, true, 1_200));
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_300)?.Slot);
    }

    [Fact]
    public void DelayZeroIgnoresClientCooltimeAndKind()
    {
        var skill = Skill(3, 1);
        skill.Kind = "buff";
        skill.CoolRemain = 8;
        skill.DurationSec = 30;
        var config = Enabled(3, 0);
        var ctl = new SkillController();
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_300)?.Slot);
    }

    [Fact]
    public void DelayNWaitsSinceLastUse()
    {
        var skill = Skill(3, 1);
        var config = Enabled(3, 5);
        var ctl = new SkillController();
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Null(ctl.Next(new[] { skill }, config, true, 4_000));
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 6_000)?.Slot);
    }

    [Fact]
    public void UncheckedSkillsNeverFire()
    {
        var skill = Skill(3, 1);
        var config = Fixtures.Config();
        config.SkillEnabledIndexes = new List<uint>();
        Assert.Null(new SkillController().Next(new[] { skill }, config, true, 1_000));
        config.SkillEnabledIndexes = new List<uint> { 9 };
        Assert.Null(new SkillController().Next(new[] { skill }, config, true, 1_000));
    }

    [Fact]
    public void UnmarkedSkillsNeedATarget()
    {
        var skill = Skill(3, 1);
        var config = Enabled(3, 0);
        Assert.Null(new SkillController().Next(new[] { skill }, config, false, 1_000));
        Assert.Equal(1, new SkillController().Next(new[] { skill }, config, true, 1_000)?.Slot);
    }

    [Fact]
    public void PeekDoesNotStampDelay()
    {
        var skill = Skill(3, 1);
        var config = Enabled(3, 5);
        var ctl = new SkillController();
        Assert.Equal(1, ctl.Peek(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Equal(1, ctl.Peek(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Equal(1, ctl.Next(new[] { skill }, config, true, 1_000)?.Slot);
        Assert.Null(ctl.Peek(new[] { skill }, config, true, 4_000));
    }

    [Fact]
    public void BuffCheckboxAllowsSelfCast()
    {
        var skill = Skill(94, 2);
        var config = Enabled(94, 0);
        config.SkillBuffIndexes = new List<uint> { 94 };
        Assert.Equal(2, new SkillController().Next(new[] { skill }, config, false, 1_000)?.Slot);
    }

    [Fact]
    public void EachSkillUsesItsOwnDelay()
    {
        var first = Skill(3, 1);
        var second = Skill(4, 2);
        var config = Fixtures.Config();
        config.SkillEnabledIndexes = new List<uint> { 3, 4 };
        config.SkillDelaySec = new Dictionary<uint, int> { [3] = 1, [4] = 1 };
        var ctl = new SkillController();
        Assert.Equal(1, ctl.Next(new[] { first, second }, config, true, 1_000)?.Slot);
        Assert.Equal(2, ctl.Next(new[] { first, second }, config, true, 1_100)?.Slot);
        Assert.Null(ctl.Next(new[] { first, second }, config, true, 1_700));
        Assert.Equal(1, ctl.Next(new[] { first, second }, config, true, 2_000)?.Slot);
        Assert.Equal(2, ctl.Next(new[] { first, second }, config, true, 2_100)?.Slot);
    }

    private static BotConfig Enabled(uint index, int delay)
    {
        var config = Fixtures.Config();
        config.SkillEnabledIndexes = new List<uint> { index };
        config.SkillDelaySec = new Dictionary<uint, int> { [index] = delay };
        return config;
    }

    private static LearnedSkill Skill(uint index, int slot) =>
        new() { Index = index, Slot = slot, Name = "skill", Kind = "attack", Level = 1 };
}
