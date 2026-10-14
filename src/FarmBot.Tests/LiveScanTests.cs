using FarmBot.Core.Logging;
using FarmBot.Platform.Client;

namespace FarmBot.Tests;

public class LiveScanTests
{
    [Fact]
    public void LiveAttachReportsConcreteStatus()
    {
        var log = new ListLog();
        using var provider = new LiveGameStateProvider(log);
        var frame = provider.Capture(Environment.TickCount64);
        Directory.CreateDirectory(@"C:\Temp");
        var text = provider.Status + Environment.NewLine +
                   "frame=" + (frame == null ? "null" : $"{frame.Entities.Count} ents from {frame.Source}") +
                   Environment.NewLine +
                   string.Join(Environment.NewLine, log.Entries.Select(item => item.Level + " " + item.Message));
        File.WriteAllText(@"C:\Temp\livestatus.txt", text);
        Assert.False(string.IsNullOrWhiteSpace(provider.Status));
        if (frame != null)
        {
            Assert.True(frame.FromLiveClient);
            Assert.DoesNotContain("simulator", frame.Source, StringComparison.OrdinalIgnoreCase);
        }
    }
}
