using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class SkillMountPlannerTests
{
    [Fact]
    public void UnmountedCastsImmediatelyAndDoesNotRemount()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        var cast = planner.Tick(skill, false, false, false, isToggle: false, 1_000);
        Assert.Equal(SkillMountCommand.UseSkill, cast.Command);

        var settled = planner.Tick(null, false, false, false, false, 1_000 + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.None, settled.Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void MountedWaitsUntilUnmountedBeforeCast()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(skill, true, true, false, true, 1_000, toggleWasActive: true).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, true, false, true, 1_400).Command);
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, true, false, true, 1_000 + SkillMountPlanner.DismountWaitMs).Command);

        var cast = planner.Tick(null, false, true, false, true, 1_000 + SkillMountPlanner.DismountWaitMs * 2);
        Assert.Equal(SkillMountCommand.UseSkill, cast.Command);
    }

    [Fact]
    public void ToggleAlreadyOnDoubleCastsThenRemounts()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();

        planner.Tick(skill, true, false, false, isToggle: true, 1_000, toggleWasActive: true);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, true, castAt).Command);

        var recast = planner.Tick(null, false, false, false, true, castAt + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.UseSkill, recast.Command);
        Assert.Equal(SkillMountPhase.WaitAnim, planner.Phase);

        var remountAt = castAt + SkillMountPlanner.AffectSettleMs + SkillMountPlanner.AnimWaitMs;
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, false, true, remountAt).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, false, false, true, remountAt + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void ToggleAlreadyOffCastsOnceOnly()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();

        planner.Tick(skill, true, false, false, isToggle: true, 1_000, toggleWasActive: false);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, true, castAt).Command);

        var after = planner.Tick(null, false, false, false, true, castAt + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.None, after.Command);
        Assert.Equal(SkillMountPhase.WaitAnim, planner.Phase);
    }

    [Fact]
    public void NonToggleDoesNotDoubleCast()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, isToggle: false, 1_000, toggleWasActive: true);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, false, castAt).Command);

        var after = planner.Tick(null, false, false, false, false, castAt + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.None, after.Command);
        Assert.Equal(SkillMountPhase.WaitAnim, planner.Phase);
    }

    [Fact]
    public void RemountRetriesUntilMounted()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, false, 1_000);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, false, false, false, castAt);
        planner.Tick(null, false, false, false, false, castAt + SkillMountPlanner.AffectSettleMs);
        var remountAt = castAt + SkillMountPlanner.AnimWaitMs;

        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, false, false, remountAt).Command);
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, false, false, remountAt + SkillMountPlanner.RemountWaitMs).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, false, false, false, remountAt + SkillMountPlanner.RemountWaitMs + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void AbortsIfDismountNeverSucceeds()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, false, 1_000);

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, false, false, false, 1_000 + SkillMountPlanner.DismountWaitMs).Command);
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, false, false, false, 1_000 + SkillMountPlanner.DismountWaitMs * 2).Command);
        var done = planner.Tick(null, true, false, false, false, 1_000 + SkillMountPlanner.DismountWaitMs * 3);
        Assert.Equal(SkillMountCommand.None, done.Command);
        Assert.False(planner.Busy);
    }

    private static LearnedSkill Skill(uint index, int slot) =>
        new() { Index = index, Slot = slot, Name = "skill", Kind = "buff", Level = 1, IsToggle = true };
}
