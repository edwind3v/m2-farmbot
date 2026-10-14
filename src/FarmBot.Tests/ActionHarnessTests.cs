using FarmBot.Core.Combat;
using System.Diagnostics;

namespace FarmBot.Tests;

[Collection("ActionHarness")]
public class ActionHarnessTests
{
    [Fact]
    public void SetTargetSelectsAKnownMonsterVidThroughThiscallIpc()
    {
        var exe = BuildHarness();
        var token = ActionProtocol.NewToken();
        var pipe = "FarmBotActionTest" + Guid.NewGuid().ToString("N")[..8];
        using var ready = CreateReadyEvent(pipe);
        using var process = StartHarness(exe, token, pipe);
        Assert.NotNull(process);
        try
        {
            Assert.True(WaitForReady(process!, ready, 8000), "harness did not start listening");

            using var client = new ActionAdapterClient(pipe, token);
            Assert.True(client.TryConnect(3000), client.Status);

            var selected = client.SetTarget(101);
            var unknown = client.SetTarget(999);
            var results =
                $"auth={client.Connected}\n" +
                $"select101 ok={selected.Ok} vid={selected.TargetVid} reason={selected.Reason} conv={selected.Convention}\n" +
                $"select999 ok={unknown.Ok} vid={unknown.TargetVid} reason={unknown.Reason}\n";
            Directory.CreateDirectory(@"C:\Temp");
            File.WriteAllText(@"C:\Temp\action-harness.txt", results);

            Assert.True(selected.Ok, results);
            Assert.Equal(101u, selected.TargetVid);
            Assert.Contains("thiscall", selected.Convention, StringComparison.OrdinalIgnoreCase);
            Assert.False(unknown.Ok, results);
            Assert.Contains("invalid VID", unknown.Reason, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process!.HasExited)
                process.Kill(entireProcessTree: true);
        }
    }

    [Fact]
    public void RejectsAnUnauthenticatedAdapterClient()
    {
        var exe = BuildHarness();
        var pipe = "FarmBotActionAuth" + Guid.NewGuid().ToString("N")[..8];
        using var ready = CreateReadyEvent(pipe);
        using var process = StartHarness(exe, ActionProtocol.NewToken(), pipe);
        Assert.NotNull(process);
        try
        {
            Assert.True(WaitForReady(process!, ready, 8000));
            using var client = new ActionAdapterClient(pipe, "wrong-token");
            Assert.False(client.TryConnect(3000));
            Assert.Contains("rejected", client.Status, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process!.HasExited)
                process.Kill(entireProcessTree: true);
        }
    }

    [Fact]
    public void AdapterSinkRejectsActionsWhenAdapterIsMissing()
    {
        using var sink = new AdapterActionSink("missing-pipe", "token");
        Assert.False(sink.TrySubmit(NativeCombatAction.SetTarget(101)));
        Assert.False(sink.TrySubmit(NativeCombatAction.Attack(101)));
        Assert.False(sink.TrySubmit(NativeCombatAction.PressActor(101, 0, 0)));
        Assert.False(sink.Available);
    }

    private static readonly object BuildGate = new();

    private static string BuildHarness()
    {
        lock (BuildGate)
        {
            var script = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "ActionHarness", "build.ps1"));
            if (!File.Exists(script))
                script = @"C:\WORk\m2 farm bot\tools\ActionHarness\build.ps1";
            Assert.True(File.Exists(script), script);
            var exe = Path.Combine(Path.GetDirectoryName(script)!, "ActionHarness.exe");
            var cpp = Path.Combine(Path.GetDirectoryName(script)!, "ActionHarness.cpp");
            if (File.Exists(exe) && File.Exists(cpp) && File.GetLastWriteTimeUtc(exe) >= File.GetLastWriteTimeUtc(cpp))
                return exe;

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var build = Process.Start(psi);
            Assert.NotNull(build);
            var output = build!.StandardOutput.ReadToEnd() + build.StandardError.ReadToEnd();
            if (!build.WaitForExit(60000))
                throw new TimeoutException("ActionHarness build timed out.\n" + output);
            Assert.True(File.Exists(exe), output);
            return exe;
        }
    }

    private static Process? StartHarness(string exe, string token, string pipe) =>
        Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            Arguments = $"--token {token} --pipe {pipe}",
            UseShellExecute = false,
            CreateNoWindow = true
        });

    private static EventWaitHandle CreateReadyEvent(string pipe) =>
        new(false, EventResetMode.ManualReset, @"Local\FarmBotReady_" + pipe);

    private static bool WaitForReady(Process process, EventWaitHandle ready, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline && !process.HasExited)
        {
            if (ready.WaitOne(50))
                return true;
        }

        return ready.WaitOne(0);
    }
}
