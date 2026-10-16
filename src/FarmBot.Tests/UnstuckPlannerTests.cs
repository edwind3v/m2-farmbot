using FarmBot.Core.Combat;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class UnstuckPlannerTests
{
    [Fact]
    public void IgnoresAMobOnTheFarSideOfTheTarget()
    {
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var mob = Fixtures.Mob(2, "Dog", EntityKind.Monster, 780, 0);

        Assert.Null(UnstuckPlanner.FindInteriorStone(player, stone, [stone, mob]));
    }

    [Fact]
    public void IgnoresAStoneSittingNextToTheTarget()
    {
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var neighbor = Fixtures.Mob(2, "Metin", EntityKind.Metin, 640, 40);

        Assert.Null(UnstuckPlanner.FindInteriorStone(player, stone, [stone, neighbor]));
    }

    [Fact]
    public void FindsAStoneOnTheOpenStretchOfThePath()
    {
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var blocker = Fixtures.Mob(2, "Metin", EntityKind.Metin, 280, 0);

        var found = UnstuckPlanner.FindInteriorStone(player, stone, [stone, blocker]);
        Assert.NotNull(found);
        Assert.Equal(2u, found.Vid);
    }

    [Fact]
    public void DoesNotTreatAMobOnThePathAsABlocker()
    {
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var mob = Fixtures.Mob(2, "Dog", EntityKind.Monster, 280, 0);

        Assert.Null(UnstuckPlanner.FindInteriorStone(player, stone, [stone, mob]));
    }

    [Fact]
    public void StepsAwayFromAnInteriorStone()
    {
        var config = Fixtures.Config();
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var blocker = Fixtures.Mob(2, "Metin", EntityKind.Metin, 280, 80);

        Assert.True(UnstuckPlanner.TryPickWaypoint(player, stone, [stone, blocker], config, 0, out var x, out var y));
        Assert.True(y < 0, $"expected a step away from +Y blocker, got ({x:0},{y:0})");
    }

    [Fact]
    public void KeepsTheWaypointInsideTheFarmRadius()
    {
        var config = Fixtures.Config();
        config.FarmCenterX = 0;
        config.FarmCenterY = 0;
        config.FarmRadius = 260;
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);

        Assert.True(UnstuckPlanner.TryPickWaypoint(player, stone, [stone], config, 0, out var x, out var y));
        Assert.True(x * x + y * y <= 260 * 260, $"waypoint ({x:0},{y:0}) left the farm");
    }

    [Fact]
    public void DoesNotLandOnAnotherEntity()
    {
        var config = Fixtures.Config();
        var player = Fixtures.Player();
        var stone = Fixtures.Mob(1, "Metin", EntityKind.Metin, 600, 0);
        var occupant = Fixtures.Mob(3, "Dog", EntityKind.Monster, 0, 300);

        Assert.True(UnstuckPlanner.TryPickWaypoint(player, stone, [stone, occupant], config, 0, out var x, out var y));
        var dx = x - occupant.X;
        var dy = y - occupant.Y;
        Assert.True(dx * dx + dy * dy > UnstuckPlanner.LandClearPx * UnstuckPlanner.LandClearPx,
            $"landed on occupant at ({x:0},{y:0})");
    }
}
