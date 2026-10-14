using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class ExternalCommandSurfaceTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void ReleaseImageHasNoSupportedExternalCommandInterface()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var report = ExternalCommandSurface.Inspect(ReleaseClient);
        var dump = report.Summary + Environment.NewLine +
                   "exports=" + string.Join(",", report.Exports) + Environment.NewLine +
                   "imports=" + string.Join(",", report.RelevantImports) + Environment.NewLine +
                   string.Join(Environment.NewLine, report.Findings.Select(item =>
                       $"{item.Kind} {item.Name}={(item.Present ? "yes" : "no")}"));
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\extcmd.txt", dump);

        Assert.Contains("SetTarget", report.Findings.Where(item => item.Present).Select(item => item.Name));
        Assert.False(report.HasExternalCommandInterface, dump);
        Assert.Empty(report.Exports);
    }
}
