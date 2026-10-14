using System.Diagnostics;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Platform.Client;

namespace FarmBot;

public sealed class ClientSession : IDisposable
{
    private Process? _process;
    private bool _closing;

    public ClientSession(
        ClientSlot slot,
        int processId,
        FarmBotShell shell,
        MainForm form,
        ActionModuleHost host,
        AdapterActionSink sink)
    {
        Slot = slot;
        ProcessId = processId;
        Shell = shell;
        Form = form;
        Host = host;
        Sink = sink;
        WatchProcess();
    }

    public ClientSlot Slot { get; }
    public int ProcessId { get; }
    public FarmBotShell Shell { get; }
    public MainForm Form { get; }
    public ActionModuleHost Host { get; }
    public AdapterActionSink Sink { get; }
    public string Status { get; set; } = "launching";
    public long LastCredentialProbeAt { get; set; }
    public event Action? Ended;

    public void Close()
    {
        if (_closing)
            return;
        _closing = true;
        KillClient();
        if (!Form.IsDisposed)
        {
            if (Form.InvokeRequired)
                Form.BeginInvoke(Form.Close);
            else
                Form.Close();
        }
    }

    public void KillClient()
    {
        try
        {
            if (_process != null && !_process.HasExited)
                _process.Kill(entireProcessTree: true);
        }
        catch
        {
            // The client may already have exited.
        }

        try
        {
            if (ProcessId > 0)
            {
                using var process = Process.GetProcessById(ProcessId);
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void Dispose()
    {
        Close();
        _process?.Dispose();
    }

    private void WatchProcess()
    {
        try
        {
            _process = Process.GetProcessById(ProcessId);
            _process.EnableRaisingEvents = true;
            _process.Exited += (_, _) =>
            {
                Status = "idle";
                Ended?.Invoke();
                try
                {
                    if (!Form.IsDisposed && Form.IsHandleCreated)
                        Form.BeginInvoke(Form.Close);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            };
        }
        catch (ArgumentException)
        {
            Status = "idle";
        }
        catch (InvalidOperationException)
        {
            Status = "idle";
        }
    }
}
