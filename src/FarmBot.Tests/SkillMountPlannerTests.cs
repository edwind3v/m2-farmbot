using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class SkillMountPlannerTests
{
    [Fact]
    public void UnmountedCastsImmediatelyAndDoesNotRemount()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        var cast = planner.Tick(skill, false, false, isBuff: false, 1_000);
        Assert.Equal(SkillMountCommand.UseSkill, cast.Command);

        var settled = planner.Tick(null, false, false, false, 1_000 + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.None, settled.Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void MountedWaitsUntilUnmountedBeforeCast()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(skill, true, true, true, 1_000).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, true, true, 1_400).Command);
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, true, true, 1_000 + SkillMountPlanner.DismountWaitMs).Command);

        var cast = planner.Tick(null, false, true, true, 1_000 + SkillMountPlanner.DismountWaitMs * 2);
        Assert.Equal(SkillMountCommand.UseSkill, cast.Command);
    }

    [Fact]
    public void RecastsWhenBuffWasOnAndDisappears_UsingCastTimeSnapshot()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();

        planner.Tick(skill, true, false, true, 1_000);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, true, true, castAt).Command);

        var recast = planner.Tick(null, false, false, true, castAt + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.UseSkill, recast.Command);
        Assert.Equal(SkillMountPhase.WaitAnim, planner.Phase);

        var remountAt = castAt + SkillMountPlanner.AffectSettleMs + SkillMountPlanner.AnimWaitMs;
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, true, remountAt).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, false, true, remountAt + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void BuffRecastsWhenMirrorMissedHadAffect()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();

        planner.Tick(skill, true, false, true, 1_000);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, false, true, castAt);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, true, castAt + SkillMountPlanner.AffectSettleMs).Command);
        var recast = planner.Tick(null, false, false, true, castAt + SkillMountPlanner.AffectPollMs);
        Assert.Equal(SkillMountCommand.UseSkill, recast.Command);

        var remountAt = castAt + SkillMountPlanner.AffectPollMs + SkillMountPlanner.AnimWaitMs;
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, true, remountAt).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, false, true, remountAt + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void RemountRetriesUntilMounted()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, 1_000);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, false, false, castAt);
        planner.Tick(null, false, false, false, castAt + SkillMountPlanner.AffectSettleMs);
        var remountAt = castAt + SkillMountPlanner.AnimWaitMs;

        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, false, remountAt).Command);
        Assert.Equal(SkillMountPhase.Remounting, planner.Phase);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, false, remountAt + 300).Command);
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, false, false, remountAt + SkillMountPlanner.RemountWaitMs).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, false, false, remountAt + SkillMountPlanner.RemountWaitMs + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void RemountStopsWhenHudReportsMounted()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, 1_000);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, false, false, castAt);
        planner.Tick(null, false, false, false, castAt + SkillMountPlanner.AffectSettleMs);
        var remountAt = castAt + SkillMountPlanner.AnimWaitMs;
        planner.Tick(null, false, false, false, remountAt);

        var done = planner.Tick(null, true, false, false, remountAt + 100);
        Assert.Equal(SkillMountCommand.None, done.Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void BuffDoesNotRecastWhenAffectAppearsAfterCast()
    {
        var skill = Skill(94, 3);
        var planner = new SkillMountPlanner();

        planner.Tick(skill, false, false, true, 1_000);
        var after = planner.Tick(null, false, true, true, 1_000 + SkillMountPlanner.AffectSettleMs);
        Assert.Equal(SkillMountCommand.None, after.Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void AbortsIfDismountNeverSucceeds()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(skill, true, false, false, 1_000);

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, false, false, 1_000 + SkillMountPlanner.DismountWaitMs).Command);
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, false, false, 1_000 + SkillMountPlanner.DismountWaitMs * 2).Command);
        var done = planner.Tick(null, true, false, false, 1_000 + SkillMountPlanner.DismountWaitMs * 3);
        Assert.Equal(SkillMountCommand.None, done.Command);
        Assert.False(planner.Busy);
    }

    private static LearnedSkill Skill(uint index, int slot) =>
        new() { Index = index, Slot = slot, Name = "skill", Kind = "buff", Level = 1 };
}
