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
}
