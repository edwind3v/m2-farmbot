using FarmBot.Core.Bot;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Tests;

public class SimulatorTests
{
    [Fact]
    public void KillsTheMetinThenLootsAndAttacksTheDog()
    {
        var log = new ListLog();
        var world = SimulatedWorld.Classic();
        var config = Fixtures.Config();
        config.FarmRadius = 1500;
        var runtime = new BotRuntime(world, world, log, () => config);
        runtime.SetEnabled(true);

        var now = 20_000L;
        for (var i = 0; i < 40; i++)
        {
            runtime.TickOnce(config, now, 100);
            now += 100;
        }

        var snapshot = runtime.Capture().Snapshot;
        Assert.NotNull(snapshot);
        Assert.Single(snapshot!.Entities);
        Assert.Equal("Hungry Wolf", snapshot.Entities[0].Name);
        Assert.Equal(90, snapshot.Entities[0].Hp);
        Assert.Contains(log.Entries, entry => entry.Message.Contains("skill", StringComparison.OrdinalIgnoreCase) && entry.Message.Contains("vid=2"));
        Assert.Contains(log.Entries, entry => entry.Message.StartsWith("pickup ", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(log.Entries, entry => entry.Message.StartsWith("attack ", StringComparison.OrdinalIgnoreCase) && entry.Message.Contains("vid=1"));
        runtime.Dispose();
    }

    [Fact]
    public void RepresentativeWorldFillsHudAndColoredMapEntities()
    {
        var world = new SimulatedWorld();
        var snapshot = world.GetSnapshot(0);
        Assert.NotNull(snapshot);
        Assert.Equal("Simulator", snapshot!.Player.Name);
        Assert.Equal(35, snapshot.Player.Level);
        Assert.Equal(12400, snapshot.Player.Exp);
        Assert.Equal(18000, snapshot.Player.NextExp);
        Assert.Equal(800, snapshot.Player.Hp);
        Assert.Equal(1000, snapshot.Player.MaxHp);
        Assert.Equal(120, snapshot.Player.Mp);
        Assert.Equal(200, snapshot.Player.MaxMp);
        Assert.Contains(snapshot.Entities, item => item.Kind == EntityKind.Monster && item.Name == "Wild Dog");
        Assert.Contains(snapshot.Entities, item => item.Kind == EntityKind.Metin && item.Vnum == 8001);
        Assert.Contains(snapshot.Entities, item => item.Kind == EntityKind.Boss && item.Name == "Alpha Wolf");
    }
}
