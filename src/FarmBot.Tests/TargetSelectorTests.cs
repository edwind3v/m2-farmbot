using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class TargetSelectorTests
{
    [Fact]
    public void PrefersMetinOverCloserMonster()
    {
        var snapshot = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 40, 0),
            Fixtures.Mob(2, "Metin of Combat", EntityKind.Metin, 250, 0));

        var selected = TargetSelector.Select(snapshot, Fixtures.Config(), stickyVid: 0);

        Assert.NotNull(selected);
        Assert.Equal(2u, selected!.Vid);
    }

    [Fact]
    public void StaysOutsideTheFarmRadius()
    {
        var config = Fixtures.Config();
        config.FarmRadius = 100;
        var snapshot = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 40, 0),
            Fixtures.Mob(3, "Hungry Wolf", EntityKind.Monster, 800, 0));

        var selected = TargetSelector.Select(snapshot, config, stickyVid: 0);

        Assert.Equal(1u, selected!.Vid);
    }

    [Fact]
    public void KeepsStickyTargetUntilAHigherPriorityAppears()
    {
        var config = Fixtures.Config();
        var dog = Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 40, 0);
        var first = TargetSelector.Select(Fixtures.World(dog), config, stickyVid: 0);
        Assert.Equal(1u, first!.Vid);

        var second = TargetSelector.Select(
            Fixtures.World(dog, Fixtures.Mob(2, "Metin of Combat", EntityKind.Metin, 200, 0)),
            config,
            stickyVid: first.Vid);

        Assert.Equal(2u, second!.Vid);
    }

    [Fact]
    public void SkipsBlacklistedNames()
    {
        var config = Fixtures.Config();
        config.NameBlacklist.Add("Dog");
        var snapshot = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 20, 0),
            Fixtures.Mob(4, "Bear", EntityKind.Monster, 80, 0));

        var selected = TargetSelector.Select(snapshot, config, stickyVid: 0);

        Assert.Equal("Bear", selected!.Name);
    }

    [Fact]
    public void SelectManyKeepsFarmPriorityAndCapsCountInsideRangedRadius()
    {
        var config = Fixtures.Config();
        config.RangedAttackEnabled = true;
        config.RangedAttackRadius = 400;
        config.RangedAttackMaxTargets = 2;
        var snapshot = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 40, 0),
            Fixtures.Mob(2, "Metin of Combat", EntityKind.Metin, 250, 0),
            Fixtures.Mob(3, "Hungry Wolf", EntityKind.Monster, 180, 0),
            Fixtures.Mob(4, "Far Bear", EntityKind.Monster, 900, 0));

        var picked = TargetSelector.SelectMany(snapshot, config, config.RangedAttackMaxTargets, config.RangedAttackRadius);

        Assert.Equal(new uint[] { 2, 1 }, picked.Select(item => item.Vid).ToArray());
    }

    [Fact]
    public void SelectManyFollowsThePlayerInsteadOfTheLockedFarmCenter()
    {
        var config = Fixtures.Config();
        config.FarmCenterX = 0;
        config.FarmCenterY = 0;
        config.FarmRadius = 200;
        config.RangedAttackRadius = 400;
        config.RangedAttackMaxTargets = 2;
        var snapshot = new WorldSnapshot
        {
            Player = Fixtures.Player(x: 2000, y: 0),
            Entities = new[]
            {
                Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 40, 0),
                Fixtures.Mob(8, "Wolf", EntityKind.Monster, 2300, 0)
            }
        };

        var locked = TargetSelector.SelectMany(snapshot, config, 2, 400);
        var follow = TargetSelector.SelectMany(snapshot, config, 2, 400, followPlayer: true);

        Assert.Empty(locked);
        Assert.Equal(8u, follow[0].Vid);
    }

    [Fact]
    public void NameRuleOutranksTheGenericMonsterRule()
    {
        var config = Fixtures.Config();
        config.TargetRules.Add(new TargetRule
        {
            Kind = EntityKind.Monster,
            NameContains = "Alpha",
            Priority = 50
        });
        var snapshot = Fixtures.World(
            Fixtures.Mob(1, "Wild Dog", EntityKind.Monster, 20, 0),
            Fixtures.Mob(5, "Alpha Wolf", EntityKind.Monster, 180, 0));

        var selected = TargetSelector.Select(snapshot, config, stickyVid: 0);

        Assert.Equal(5u, selected!.Vid);
    }
}
