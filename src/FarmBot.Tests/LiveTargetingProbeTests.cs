using FarmBot.Platform.Client;

namespace FarmBot.Tests;

public class LiveTargetingProbeTests
{
    [Fact]
    public void RecordsLiveSetTargetVerificationWithoutInvokingIt()
    {
        var report = LiveTargetingProbe.Inspect();
        var dump = report.Summary + "\n" + report.RemainingRequirement + "\n" + string.Join("\n", report.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\live-targeting.txt", dump);

        Assert.False(report.InvokedSetTarget, dump);
        Assert.False(report.NativeTargetingVerified, dump);
        Assert.Contains("window thread", report.RemainingRequirement, StringComparison.OrdinalIgnoreCase);
        if (report.ClientRunning && report.SetTargetVa != 0)
            Assert.NotEqual(0x4EA2F0u, report.SetTargetVa - report.ImageBase);
    }
}
