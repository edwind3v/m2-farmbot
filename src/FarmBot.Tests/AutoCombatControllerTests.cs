using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class AutoCombatControllerTests
{
    [Fact]
    public void StopEmitsOneNativeStopAndBlocksFurtherActions()
    {
        var controller = new AutoCombatController();
        var world = Fixtures.World(Fixtures.Mob(11, "Wild Dog", EntityKind.Monster, 80, 0));
        var config = Fixtures.Config();
        config.Skills.Clear();

        var start = controller.Tick(world, config, true, 1_000);
        Assert.Equal(CombatState.Approach, start.State);
        Assert.Equal(11u, start.Target!.Vid);
        Assert.Equal(NativeActionKind.PressActor, start.Action!.Kind);

        var stop = controller.Tick(world, config, false, 1_100);
        Assert.Equal(CombatState.Idle, stop.State);
        Assert.Equal(NativeActionKind.Stop, stop.Action!.Kind);

        var again = controller.Tick(world, config, false, 1_200);
        Assert.Null(again.Action);
        Assert.Equal(0u, controller.TargetVid);
    }

    [Fact]
    public void AcquiresApproachesAndAttacksALiveMonster()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.AttackIntervalMs = 400;
        config.MoveIntervalMs = 100;

        var far = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 800, 0));
        var acquired = controller.Tick(far, config, true, 5_000);
        Assert.Equal(21u, acquired.Target!.Vid);
        Assert.Equal(NativeActionKind.PressActor, acquired.Action!.Kind);

        var approach = controller.Tick(far, config, true, 5_200);
        Assert.Equal(CombatState.Approach, approach.State);
        Assert.Null(approach.Action);

        var near = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 80, 0));
        var attack = controller.Tick(near, config, true, 5_400);
        Assert.Equal(CombatState.Attack, attack.State);
        Assert.Equal(NativeActionKind.PressActor, attack.Action!.Kind);
        Assert.NotEqual(NativeActionKind.Attack, attack.Action.Kind);

        var hold = controller.Tick(near, config, true, 5_500);
        Assert.Equal(CombatState.Attack, hold.State);
        Assert.Null(hold.Action);
        Assert.Contains("auto-attack", hold.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KeepsAttackingWhenStandingOnAMetin()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.MoveIntervalMs = 100;
        config.TargetTimeoutMs = 20_000;
        config.ClientProfile = ClientProfile.Baerim;

        var close = Fixtures.World(Fixtures.Mob(41, "Metin of Darkness", EntityKind.Metin, 40, 0));
        controller.Tick(close, config, true, 1_000);
        var attack = controller.Tick(close, config, true, 1_200);
        Assert.Equal(CombatState.Attack, attack.State);
        Assert.Equal(41u, attack.Target!.Vid);

        var onTop = Fixtures.World(Fixtures.Mob(41, "Metin of Darkness", EntityKind.Metin, 0, 0));
        var hold = controller.Tick(onTop, config, true, 1_400);
        Assert.Equal(CombatState.Attack, hold.State);
        Assert.Equal(41u, controller.TargetVid);
        Assert.DoesNotContain("left melee", hold.Reason, StringComparison.OrdinalIgnoreCase);

        var still = controller.Tick(onTop, config, true, 25_000);
        Assert.Equal(CombatState.Attack, still.State);
        Assert.Equal(41u, controller.TargetVid);
    }

    [Fact]
    public void DoesNotReclickTheSameBaerimVidWhileApproaching()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.MoveIntervalMs = 100;
        config.ClientProfile = ClientProfile.Baerim;

        var far = Fixtures.World(Fixtures.Mob(71, "Metin of Darkness", EntityKind.Metin, 600, 0));
        var acquired = controller.Tick(far, config, true, 1_000);
        Assert.Equal(NativeActionKind.PressActor, acquired.Action!.Kind);
        Assert.Equal(71u, acquired.Target!.Vid);

        var again = controller.Tick(far, config, true, 1_200);
        Assert.Equal(CombatState.Approach, again.State);
        Assert.Equal(71u, controller.TargetVid);
        Assert.Null(again.Action);

        var third = controller.Tick(far, config, true, 1_800);
        Assert.Null(third.Action);
        Assert.Equal(71u, controller.TargetVid);

        config.StuckTimeoutMs = 200;
        var still = controller.Tick(far, config, true, 2_200);
        Assert.Equal(CombatState.Approach, still.State);
        Assert.Equal(71u, controller.TargetVid);
        Assert.Null(still.Action);
        Assert.DoesNotContain("recover", still.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stop", still.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StaysInAttackAtStoneEdgeDistance()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.ClientProfile = ClientProfile.Baerim;

        var edge = Fixtures.World(Fixtures.Mob(81, "Metin of Darkness", EntityKind.Metin, 143, 0));
        controller.Tick(edge, config, true, 2_000);
        var attack = controller.Tick(edge, config, true, 2_200);
        Assert.Equal(CombatState.Attack, attack.State);
        Assert.Equal(81u, attack.Target!.Vid);
        Assert.DoesNotContain("left melee", attack.Reason, StringComparison.OrdinalIgnoreCase);

        var hold = controller.Tick(edge, config, true, 2_400);
        Assert.Equal(CombatState.Attack, hold.State);
        Assert.Equal(81u, controller.TargetVid);
        Assert.Null(hold.Action);
    }

    [Fact]
    public void WalksToATargetInsideFarmRangeButOutsideMelee()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.MoveIntervalMs = 100;

        var mid = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 220, 0));
        var acquired = controller.Tick(mid, config, true, 5_000);
        Assert.Equal(NativeActionKind.PressActor, acquired.Action!.Kind);
        var approach = controller.Tick(mid, config, true, 5_200);
        Assert.Equal(CombatState.Approach, approach.State);
        Assert.Null(approach.Action);
    }

    [Fact]
    public void DoesNotAttackOnTheSpotWhenOutsideArthionMeleeRange()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;
        config.MoveIntervalMs = 50;

        var justOutside = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 190, 0));
        var acquired = controller.Tick(justOutside, config, true, 5_000);
        Assert.Equal(NativeActionKind.PressActor, acquired.Action!.Kind);
        var tick = controller.Tick(justOutside, config, true, 5_200);
        Assert.Equal(CombatState.Approach, tick.State);
        Assert.Null(tick.Action);
    }

    [Fact]
    public void DetectsDeathAndSelectsTheNextFarmableTarget()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.AttackRange = 300;

        var first = Fixtures.World(
            Fixtures.Mob(31, "Dog", EntityKind.Monster, 40, 0),
            Fixtures.Mob(32, "Metin", EntityKind.Metin, 60, 0));
        controller.Tick(first, config, true, 8_000);
        Assert.Equal(32u, controller.TargetVid);

        var dead = Fixtures.World(
            Fixtures.Mob(31, "Dog", EntityKind.Monster, 40, 0),
            Fixtures.Mob(32, "Metin", EntityKind.Metin, 60, 0, hp: 0));
        controller.Tick(dead, config, true, 8_100);
        var verify = controller.Tick(dead, config, true, 8_200);
        Assert.True(verify.State is CombatState.Verify or CombatState.AcquireTarget or CombatState.Approach);

        var next = Fixtures.World(Fixtures.Mob(31, "Dog", EntityKind.Monster, 40, 0));
        CombatTick tick = verify;
        for (var t = 8_300; t <= 9_000 && controller.TargetVid != 31; t += 100)
            tick = controller.Tick(next, config, true, t);

        Assert.Equal(31u, controller.TargetVid);
        Assert.DoesNotContain("player", tick.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RangedModeShotsPriorityTargetsWithoutApproaching()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.RangedAttackEnabled = true;
        config.RangedAttackRadius = 2000;
        config.RangedAttackMaxTargets = 2;
        config.RangedAttackDelayMs = 400;
        var world = Fixtures.World(
            Fixtures.Mob(51, "Wild Dog", EntityKind.Monster, 800, 0),
            Fixtures.Mob(52, "Metin of Combat", EntityKind.Metin, 900, 0));

        var arm = controller.Tick(world, config, true, 10_000);
        Assert.Equal(NativeActionKind.Stop, arm.Action!.Kind);

        var first = controller.Tick(world, config, true, 10_100);
        Assert.Equal(NativeActionKind.Shot, first.Action!.Kind);
        Assert.False(first.Action.Avoid);
        Assert.Contains("52,", first.Action.Hits);
        Assert.Contains("51,", first.Action.Hits);
        Assert.Contains("blast", first.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("approach", first.Reason, StringComparison.OrdinalIgnoreCase);

        var wait = controller.Tick(world, config, true, 10_200);
        Assert.Null(wait.Action);
        Assert.Contains("delay", wait.Reason, StringComparison.OrdinalIgnoreCase);

        var second = controller.Tick(world, config, true, 10_550);
        Assert.Equal(NativeActionKind.Shot, second.Action!.Kind);
        Assert.Contains("52,", second.Action.Hits);
        Assert.Contains("51,", second.Action.Hits);
    }

    [Fact]
    public void RangedAvoidAttacksMarksTheShot()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.RangedAttackEnabled = true;
        config.RangedAvoidAttacks = true;
        config.RangedAttackMaxTargets = 1;
        var world = Fixtures.World(Fixtures.Mob(61, "Wild Dog", EntityKind.Monster, 800, 0));

        controller.Tick(world, config, true, 20_000);
        var shot = controller.Tick(world, config, true, 20_100);
        Assert.Equal(NativeActionKind.Shot, shot.Action!.Kind);
        Assert.True(shot.Action.Avoid);
    }

    [Fact]
    public void LootLoopCarriesFastPickupDelayAndRangedFlag()
    {
        var nearby = NativeCombatAction.LootLoop(true, 1, false, 300);
        Assert.Equal(NativeActionKind.LootLoop, nearby.Kind);
        Assert.Equal(1u, nearby.Vid);
        Assert.Equal(1f, nearby.X);
        Assert.False(nearby.Avoid);

        var ranged = NativeCombatAction.LootLoop(true, 25, true, 2000);
        Assert.Equal(2u, ranged.Vid);
        Assert.True(ranged.Avoid);
        Assert.Equal(25f, ranged.X);
        Assert.Equal(2000f, ranged.Y);
        Assert.Contains("ranged pickup", ranged.Describe(), StringComparison.OrdinalIgnoreCase);

        var both = NativeCombatAction.LootLoop(3, 10, 2000);
        Assert.Equal(3u, both.Vid);
        Assert.Contains("fast+ranged pickup", both.Describe(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RangedPickupDoesNotWalkTheCharacter()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.RangedAttackEnabled = true;
        config.RangedPickupEnabled = true;
        config.RangedAttackRadius = 2000;
        var world = Fixtures.World(
            Fixtures.Mob(51, "Wild Dog", EntityKind.Monster, 800, 0),
            Fixtures.Mob(90, "drop", EntityKind.Drop, 900, 0, hp: 1));

        controller.Tick(world, config, true, 30_000);
        var blast = controller.Tick(world, config, true, 30_200);
        Assert.Equal(NativeActionKind.Shot, blast.Action!.Kind);
        Assert.NotEqual(NativeActionKind.MoveTo, blast.Action.Kind);
    }

    [Fact]
    public void NeverSelectsPlayersOrNpcs()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        var world = Fixtures.World(
            Fixtures.Mob(41, "OtherPlayer", EntityKind.Player, 50, 0),
            Fixtures.Mob(42, "Guard", EntityKind.Npc, 60, 0));

        var tick = controller.Tick(world, config, true, 2_000);
        Assert.Equal(CombatState.AcquireTarget, tick.State);
        Assert.Null(tick.Target);
        Assert.Equal(0u, controller.TargetVid);
    }

    [Fact]
    public void DisconnectedSinkDoesNotPretendTheClientAttacked()
    {
        var sink = new DisconnectedActionSink();
        var action = NativeCombatAction.Attack(99);
        Assert.False(sink.Available);
        Assert.False(sink.TrySubmit(action));
        Assert.Contains("no external command interface", sink.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalModeUsesSetTargetThenAttackLikeTheLastPushedClient()
    {
        var controller = new AutoCombatController();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.ClientProfile = ClientProfile.Normal;
        config.AttackRange = 300;
        config.AttackIntervalMs = 400;
        config.MoveIntervalMs = 100;

        var far = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 800, 0));
        var acquired = controller.Tick(far, config, true, 5_000);
        Assert.Equal(NativeActionKind.SetTarget, acquired.Action!.Kind);

        var approach = controller.Tick(far, config, true, 5_200);
        Assert.Equal(CombatState.Approach, approach.State);
        Assert.Equal(NativeActionKind.PressActor, approach.Action!.Kind);

        var near = Fixtures.World(Fixtures.Mob(21, "Wild Dog", EntityKind.Monster, 80, 0));
        var face = controller.Tick(near, config, true, 5_400);
        Assert.Equal(CombatState.FaceTarget, face.State);
        Assert.Equal(NativeActionKind.Face, face.Action!.Kind);

        var armed = controller.Tick(near, config, true, 5_500);
        Assert.Equal(CombatState.Attack, armed.State);
        Assert.Equal(NativeActionKind.Face, armed.Action!.Kind);

        var attack = controller.Tick(near, config, true, 5_600);
        Assert.Equal(CombatState.Wait, attack.State);
        Assert.Equal(NativeActionKind.Attack, attack.Action!.Kind);
    }

    [Fact]
    public void RecordingSinkKeepsSubmittedGameplayActions()
    {
        var sink = new RecordingActionSink();
        Assert.True(sink.TrySubmit(NativeCombatAction.SetTarget(7)));
        Assert.True(sink.TrySubmit(NativeCombatAction.PressActor(7, 10, 20)));
        Assert.Equal(2, sink.Actions.Count);
        Assert.Equal(NativeActionKind.SetTarget, sink.Actions[0].Kind);
        Assert.Equal(7u, sink.Actions[1].Vid);
    }
}
