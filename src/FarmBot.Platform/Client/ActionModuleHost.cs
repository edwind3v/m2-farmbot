using System.Diagnostics;
using FarmBot.Core.Combat;

namespace FarmBot.Platform.Client;

public sealed record ClientProcessInfo(int ProcessId, string Name, string Path)
{
    public override string ToString() => $"{Name} PID {ProcessId}";
}

public sealed class ActionModuleHost
{
    public string Token { get; } = ActionProtocol.NewToken();
    public string PipeName { get; }
    public int LastProcessId { get; private set; }
    public string Status { get; private set; } = "companion not loaded";

    public ActionModuleHost(string? pipeName = null)
    {
        PipeName = string.IsNullOrWhiteSpace(pipeName)
            ? LoaderOutput.UniquePipeName()
            : pipeName.Trim();
    }

    public static IReadOnlyList<ClientProcessInfo> ListClients()
    {
        var list = new List<ClientProcessInfo>();
        foreach (var name in LiveGameStateProvider.ProcessNames)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    var path = "";
                    try { path = process.MainModule?.FileName ?? ""; }
                    catch { /* High-integrity module path may be hidden from this viewer. */ }
                    list.Add(new ClientProcessInfo(process.Id, process.ProcessName, path));
                }
            }
        }

        return list;
    }

    public bool TryAttach(int processId, out string status)
    {
        LastProcessId = processId;
        return RunLoader($"--attach {processId}", out status, 20000);
    }

    public bool TryLaunch(string exePath, out string status) =>
        TryLaunch(exePath, out status, out _);

    public bool TryLaunch(string exePath, out string status, out int processId)
    {
        processId = 0;
        LastProcessId = 0;
        if (!File.Exists(exePath))
        {
            status = Status = "client executable not found: " + exePath;
            return false;
        }

        var workdir = Path.GetDirectoryName(exePath) ?? "";
        if (!RunLoader($"--launch \"{exePath}\" --workdir \"{workdir}\"", out status, 45000))
            return false;
        if (!LoaderOutput.TryParseLaunchedPid(status, out processId))
        {
            status = Status = "client launched but FarmBotLoad did not report a PID: " + status;
            return false;
        }

        LastProcessId = processId;
        return true;
    }

    public bool WaitUntilListening(AdapterActionSink sink, int timeoutMs = 15000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        using var ready = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\FarmBotReady_" + PipeName);
        while (DateTime.UtcNow < deadline)
        {
            if (ready.WaitOne(50) && sink.TryConnect(400))
            {
                Status = sink.Status;
                return true;
            }
        }

        Status = sink.Status;
        return sink.Available;
    }

    private bool RunLoader(string extraArgs, out string status, int waitMs = 20000)
    {
        var load = Resolve("FarmBotLoad.exe");
        var dll = Resolve("FarmBotAction.dll");
        if (load.Length == 0 || dll.Length == 0)
        {
            BuildNative();
            load = Resolve("FarmBotLoad.exe");
            dll = Resolve("FarmBotAction.dll");
        }

        if (load.Length == 0 || dll.Length == 0)
        {
            status = Status = "FarmBotAction.dll / FarmBotLoad.exe are not built";
            return false;
        }

        var psi = new ProcessStartInfo
        {
            FileName = load,
            Arguments = $"{extraArgs} --dll \"{dll}\" --token {Token} --pipe {PipeName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null)
        {
            status = Status = "FarmBotLoad failed to start";
            return false;
        }

        if (!process.WaitForExit(waitMs))
        {
            try { process.Kill(true); } catch { /* ignore */ }
            status = Status = "FarmBotLoad timed out";
            return false;
        }

        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
        {
            status = Status = "FarmBotLoad failed: " + output.Trim();
            return false;
        }

        status = Status = output.Trim();
        return true;
    }

    private static string Resolve(string fileName)
    {
        string? newest = null;
        foreach (var dir in new[]
                 {
                     AppContext.BaseDirectory,
                     Path.Combine(AppContext.BaseDirectory, "native"),
                     Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "FarmBotAction")),
                     @"C:\WORk\m2 farm bot\tools\FarmBotAction"
                 })
        {
            var path = Path.Combine(dir, fileName);
            if (!File.Exists(path))
                continue;
            if (newest == null || File.GetLastWriteTimeUtc(path) > File.GetLastWriteTimeUtc(newest))
                newest = path;
        }

        return newest ?? "";
    }

    private static void BuildNative()
    {
        var script = Resolve("build.ps1");
        if (script.Length == 0)
            script = @"C:\WORk\m2 farm bot\tools\FarmBotAction\build.ps1";
        if (!File.Exists(script))
            return;
        var psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var build = Process.Start(psi);
        build?.WaitForExit(90000);
    }
}
