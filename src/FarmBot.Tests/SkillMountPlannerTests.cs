using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class SkillMountPlannerTests
{
    [Fact]
    public void StartsInactive_OneOnClick_RemountOnlyAfterActive()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(new[] { skill }, true, null, null, 1_000).Command);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, null, null, t).Command); // Decide, no probe yet
        Assert.Equal(SkillMountPhase.Decide, planner.Phase);

        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, t + 10).Command); // on
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, false, t + 50).Command);
        Assert.True(planner.Busy);

        var done = planner.Tick(null, false, true, false, t + 100);
        Assert.Equal(SkillMountCommand.Remount, done.Command);
        Assert.Equal(skill.Index, done.Confirmed!.Index);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, true, false, t + 200).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void StartsActive_OffEdgeThenOnEdge_ThenRemount()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, null, null, t); // Decide

        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, true, false, t + 10).Command); // off
        Assert.Equal(SkillMountPhase.WaitOff, planner.Phase);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, true, false, t + 50).Command); // still on
        Assert.Equal(
            SkillMountCommand.UseSkill,
            planner.Tick(null, false, true, false, t + 10 + SkillMountPlanner.ClickCooldownMs).Command); // retry off

        var onClick = planner.Tick(null, false, false, false, t + 500); // off observed → on
        Assert.Equal(SkillMountCommand.UseSkill, onClick.Command);
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, false, t + 550).Command);
        var remount = planner.Tick(null, false, true, false, t + 600);
        Assert.Equal(SkillMountCommand.Remount, remount.Command);
        Assert.NotNull(remount.Confirmed);
    }

    [Fact]
    public void Batch_BerserkConfirmedBeforeAuraClick_AuraOnBeforeRemount()
    {
        var berserk = Skill(3, 3);
        var aura = Skill(4, 4);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { berserk, aura }, true, null, null, 1_000);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, null, null, t);

        // Berserk was on → off → on
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, true, false, t + 10).Command);
        Assert.Equal(berserk.Index, planner.Skill!.Index);
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, t + 20).Command);

        var afterBerserk = planner.Tick(null, false, true, false, t + 30);
        Assert.Equal(SkillMountCommand.None, afterBerserk.Command); // Decide next (aura) — no click same tick
        Assert.Equal(SkillMountPhase.Decide, planner.Phase);
        Assert.Equal(aura.Index, planner.Skill!.Index);
        Assert.Equal(berserk.Index, afterBerserk.Confirmed!.Index);

        // Aura was off → on only
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, t + 40).Command);
        Assert.Equal(aura.Index, planner.Skill!.Index);
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        // Still off → no remount
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, false, t + 50).Command);
        Assert.True(planner.Busy);

        var remount = planner.Tick(null, false, true, false, t + 60);
        Assert.Equal(SkillMountCommand.Remount, remount.Command);
        Assert.Equal(aura.Index, remount.Confirmed!.Index);
    }

    [Fact]
    public void WaitOff_StuckActive_DoesNotRemount()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, true, false, t); // Decide → off click

        for (var i = 0; i < 20; i++)
        {
            var at = t + SkillMountPlanner.ClickCooldownMs * (i + 1);
            var cmd = planner.Tick(null, false, true, false, at).Command;
            Assert.True(cmd == SkillMountCommand.None || cmd == SkillMountCommand.UseSkill);
            Assert.NotEqual(SkillMountCommand.Remount, cmd);
            Assert.Equal(SkillMountPhase.WaitOff, planner.Phase);
            Assert.True(planner.Busy);
        }
    }

    [Fact]
    public void WaitOn_StillOff_NeverRemounts()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, false, false, false, 1_000); // Decide→on
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        for (var i = 0; i < 20; i++)
        {
            var at = 1_000 + SkillMountPlanner.ClickCooldownMs * (i + 1);
            var cmd = planner.Tick(null, false, false, false, at).Command;
            Assert.NotEqual(SkillMountCommand.Remount, cmd);
            Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);
            Assert.True(planner.Busy);
        }
    }

    [Fact]
    public void NoOnClickWhileCooling()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        // Unmounted, inactive, but cooling → Decide goes to WaitOn without clicking.
        Assert.Equal(SkillMountCommand.None, planner.Tick(new[] { skill }, false, false, true, 1_000).Command);
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, true, 1_200).Command);
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, 1_400).Command);
    }

    [Fact]
    public void NoOffRetryWhileCooling()
    {
        var skill = Skill(3, 3);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, false, true, false, 1_000); // off click
        Assert.Equal(SkillMountPhase.WaitOff, planner.Phase);

        // Still on + cooling past cooldown → no retry click
        Assert.Equal(
            SkillMountCommand.None,
            planner.Tick(null, false, true, true, 1_000 + SkillMountPlanner.ClickCooldownMs).Command);

        // Cool cleared → retry off
        Assert.Equal(
            SkillMountCommand.UseSkill,
            planner.Tick(null, false, true, false, 1_000 + SkillMountPlanner.ClickCooldownMs * 2).Command);
    }

    [Fact]
    public void SkipAnimRemountsAfterOnConfirmed()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000, skipAnimHorse: true);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, null, null, castAt, skipAnimHorse: true);
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, castAt + 10, skipAnimHorse: true).Command);

        // First active sample latches confirm time; remount after SkipAnimWaitMs.
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, true, false, castAt + 50, skipAnimHorse: true).Command);
        Assert.Equal(
            SkillMountCommand.Remount,
            planner.Tick(null, false, true, false, castAt + 50 + SkillMountPlanner.SkipAnimWaitMs, skipAnimHorse: true).Command);
    }

    [Fact]
    public void SkipAnim_DoesNotRemountWhileStillOff()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000, skipAnimHorse: true);
        var castAt = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, false, false, castAt, skipAnimHorse: true);

        Assert.NotEqual(
            SkillMountCommand.Remount,
            planner.Tick(null, false, false, false, castAt + SkillMountPlanner.SkipAnimWaitMs, skipAnimHorse: true).Command);
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);
        Assert.True(planner.Busy);
    }

    [Fact]
    public void BatchSkipOn_RemountsBetweenBuffs()
    {
        var a = Skill(3, 1);
        var b = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { a, b }, true, null, null, 1_000, skipAnimHorse: true);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        planner.Tick(null, false, null, null, t, skipAnimHorse: true);

        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, t + 10, skipAnimHorse: true).Command);
        var confirmA = t + 50;
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, true, false, confirmA, skipAnimHorse: true).Command);
        var remountA = planner.Tick(null, false, true, false, confirmA + SkillMountPlanner.SkipAnimWaitMs, skipAnimHorse: true);
        Assert.Equal(SkillMountCommand.Remount, remountA.Command);
        Assert.Equal(a.Index, remountA.Confirmed!.Index);

        var dismountAt = confirmA + SkillMountPlanner.SkipAnimWaitMs + 50;
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, true, false, dismountAt, skipAnimHorse: true).Command);

        var castB = dismountAt + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, castB, skipAnimHorse: true).Command);
        Assert.Equal(b.Index, planner.Skill!.Index);
        var confirmB = castB + 50;
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, true, false, confirmB, skipAnimHorse: true).Command);
        Assert.Equal(
            SkillMountCommand.Remount,
            planner.Tick(null, false, true, false, confirmB + SkillMountPlanner.SkipAnimWaitMs, skipAnimHorse: true).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, true, false, confirmB + 500, skipAnimHorse: true).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void RemountRetriesUntilMounted()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000);
        var t = 1_000 + SkillMountPlanner.DismountWaitMs;
        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(null, false, false, false, t).Command);
        var remountAt = t + 50;
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, true, false, remountAt).Command);
        Assert.Equal(SkillMountCommand.Remount, planner.Tick(null, false, true, false, remountAt + SkillMountPlanner.RemountWaitMs).Command);
        Assert.Equal(SkillMountCommand.None, planner.Tick(null, true, true, false, remountAt + SkillMountPlanner.RemountWaitMs + 100).Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void AbortsIfDismountNeverSucceeds()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();
        planner.Tick(new[] { skill }, true, null, null, 1_000);

        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, null, null, 1_000 + SkillMountPlanner.DismountWaitMs).Command);
        Assert.Equal(SkillMountCommand.Dismount, planner.Tick(null, true, null, null, 1_000 + SkillMountPlanner.DismountWaitMs * 2).Command);
        var done = planner.Tick(null, true, null, null, 1_000 + SkillMountPlanner.DismountWaitMs * 3);
        Assert.Equal(SkillMountCommand.None, done.Command);
        Assert.False(planner.Busy);
    }

    [Fact]
    public void UnmountedTurnsOnThenIdlesWhenActive()
    {
        var skill = Skill(4, 2);
        var planner = new SkillMountPlanner();

        Assert.Equal(SkillMountCommand.UseSkill, planner.Tick(new[] { skill }, false, false, false, 1_000).Command);
        Assert.Equal(SkillMountPhase.WaitOn, planner.Phase);

        Assert.Equal(SkillMountCommand.None, planner.Tick(null, false, false, false, 1_200).Command);
        var done = planner.Tick(null, false, true, false, 1_300);
        Assert.Equal(SkillMountCommand.None, done.Command);
        Assert.Equal(skill.Index, done.Confirmed!.Index);
        Assert.False(planner.Busy);
    }

    private static LearnedSkill Skill(uint index, int slot) =>
        new() { Index = index, Slot = slot, Name = "skill", Kind = "buff", Level = 1, IsToggle = true };
}
