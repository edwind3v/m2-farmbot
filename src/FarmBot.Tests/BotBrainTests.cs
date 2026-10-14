using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class BotBrainTests
{
    private static TickContext Running => new(true, false, true, 0);

    [Fact]
    public void UsesAReadySkillBeforeTheBasicAttack()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        var world = Fixtures.World(Fixtures.Mob(2, "Metin of Combat", EntityKind.Metin, 120, 0));
        world = world with { ReceivedAtMs = 20_000 };

        var first = brain.Tick(world, config, Running, 20_000);
        var second = brain.Tick(world, config, Running, 20_200);

        Assert.Equal(BotCommandKind.Skill, first.Command!.Kind);
        Assert.Equal("1", first.Command.Key);
        Assert.Equal(2u, first.Command.Vid);
        Assert.Equal(BotCommandKind.Attack, second.Command!.Kind);
        Assert.Equal(2u, second.Command.Vid);
    }

    [Fact]
    public void MovesWhenTheTargetIsOutsideRangeAndPathingIsDisabled()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        config.AttackAlsoPaths = false;
        config.AttackRange = 200;
        config.Skills.Clear();
        var world = Fixtures.World(Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 1000, 0)) with { ReceivedAtMs = 5_000 };

        var first = brain.Tick(world, config, Running, 5_000);
        var second = brain.Tick(world, config, Running, 5_100);

        Assert.Equal(BotCommandKind.Move, first.Command!.Kind);
        Assert.Equal(1000, first.Command.X);
        Assert.Null(second.Command);
        Assert.Equal(BotPhase.Moving, second.Phase);
    }

    [Fact]
    public void RangedAttackCyclesShotsInsteadOfWalkingIn()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        config.Skills.Clear();
        config.RangedAttackEnabled = true;
        config.RangedAttackRadius = 2000;
        config.RangedAttackMaxTargets = 2;
        config.RangedAttackDelayMs = 300;
        var world = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 800, 0),
            Fixtures.Mob(2, "Metin of Combat", EntityKind.Metin, 900, 0)) with { ReceivedAtMs = 5_000 };

        var first = brain.Tick(world, config, Running, 5_000);
        var wait = brain.Tick(world, config, Running, 5_100);
        var second = brain.Tick(world, config, Running, 5_400);

        Assert.Equal(BotCommandKind.Shot, first.Command!.Kind);
        Assert.False(first.Command.Avoid);
        Assert.Contains("2,", first.Command.Hits);
        Assert.Contains("1,", first.Command.Hits);
        Assert.Null(wait.Command);
        Assert.Equal(BotCommandKind.Shot, second.Command!.Kind);
        Assert.Contains("2,", second.Command.Hits);
        Assert.Contains("1,", second.Command.Hits);
    }

    [Fact]
    public void LootsADropBeforeChoosingTheNextMonster()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        var world = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 80, 0),
            Fixtures.Mob(9, "Metin drop", EntityKind.Drop, 30, 0, hp: 1)) with { ReceivedAtMs = 5_000 };

        var decision = brain.Tick(world, config, Running, 5_000);

        Assert.Equal(BotPhase.Looting, decision.Phase);
        Assert.Equal(BotCommandKind.Pickup, decision.Command!.Kind);
        Assert.Equal(9u, decision.Command.Vid);
    }

    [Fact]
    public void ReturnsToTheFarmCenterWhenNothingIsLeft()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        config.FarmCenterX = 0;
        config.FarmCenterY = 0;
        config.LeashSlack = 50;
        var world = Fixtures.World() with
        {
            ReceivedAtMs = 5_000,
            Player = Fixtures.Player(x: 400, y: 20)
        };

        var decision = brain.Tick(world, config, Running, 5_000);

        Assert.Equal(BotPhase.Returning, decision.Phase);
        Assert.Equal(BotCommandKind.Move, decision.Command!.Kind);
        Assert.Equal(0, decision.Command.X);
        Assert.Equal(0, decision.Command.Y);
    }

    [Fact]
    public void UnknownHudDoesNotCountAsDead()
    {
        var brain = new BotBrain();
        var world = Fixtures.World() with
        {
            ReceivedAtMs = 8_000,
            Player = Fixtures.Player(hp: 0, maxHp: 0)
        };

        var decision = brain.Tick(world, Fixtures.Config(), Running, 8_000);

        Assert.NotEqual(BotPhase.Failsafe, decision.Phase);
        Assert.NotEqual("player is dead", decision.Reason);
    }

    [Fact]
    public void FailsafeStopsOnceWhenHpIsLow()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        config.FailsafeHpPercent = 15;
        var world = Fixtures.World() with
        {
            ReceivedAtMs = 8_000,
            Player = Fixtures.Player(hp: 10, maxHp: 100)
        };

        var first = brain.Tick(world, config, Running, 8_000);
        var second = brain.Tick(world, config, Running, 8_100);

        Assert.Equal(BotPhase.Failsafe, first.Phase);
        Assert.Equal(BotCommandKind.Stop, first.Command!.Kind);
        Assert.Equal("player HP below failsafe", first.Reason);
        Assert.Null(second.Command);
    }

    [Fact]
    public void FailsafeTripsWhenStateIsStale()
    {
        var brain = new BotBrain();
        var config = Fixtures.Config();
        config.StateTimeoutMs = 1000;
        var world = Fixtures.World() with { ReceivedAtMs = 1_000 };

        var decision = brain.Tick(world, config, Running, 3_000);

        Assert.Equal(BotPhase.Failsafe, decision.Phase);
        Assert.Equal("client state timed out", decision.Reason);
    }

    [Fact]
    public void WaitsWhenTheClientHasNotPublishedState()
    {
        var brain = new BotBrain();
        var decision = brain.Tick(null, Fixtures.Config(), new TickContext(true, false, false, 0), 1_000);

        Assert.Equal(BotPhase.WaitingForClient, decision.Phase);
        Assert.Null(decision.Command);
    }
}
