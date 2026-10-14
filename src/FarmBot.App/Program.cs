using FarmBot.Core.Logging;
using FarmBot.Platform.Client;

namespace FarmBot;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Any(item => string.Equals(item, "--live-scan", StringComparison.OrdinalIgnoreCase)))
        {
            RunLiveScan();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.Run(new MainForm());
    }

    private static void RunLiveScan()
    {
        var log = new ListLog();
        using var provider = new LiveGameStateProvider(log);
        var frame = provider.Capture(Environment.TickCount64);
        var lines = new List<string> { provider.Status };
        if (frame == null)
        {
            lines.Add("frame=null");
        }
        else
        {
            lines.Add($"fromLive={frame.FromLiveClient} count={frame.Entities.Count} mainVid={frame.Main?.Vid} source={frame.Source}");
            foreach (var entity in frame.Entities.Take(64))
                lines.Add($"VID {entity.Vid} type={(int)entity.Type}/{entity.Type} vnum={entity.Vnum} xyz={entity.X:0.0},{entity.Y:0.0},{entity.Z:0.0} dist={entity.Distance:0.0}");
        }

        foreach (var entry in log.Entries)
            lines.Add($"{entry.Level} {entry.Message}");
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllLines(@"C:\Temp\livescan.txt", lines);
        Console.WriteLine(string.Join(Environment.NewLine, lines));
    }
}
