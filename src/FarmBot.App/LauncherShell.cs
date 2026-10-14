using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Logging;
using FarmBot.Platform.Client;

namespace FarmBot;

public sealed class LauncherShell : IDisposable
{
    private readonly string _baseDirectory;
    private readonly string _slotsPath;
    private readonly object _gate = new();
    private readonly Dictionary<string, ClientSession> _sessions = new();
    private readonly FileBotLog _log;
    private readonly SynchronizationContext? _sync = SynchronizationContext.Current;
    private LauncherState _state;

    public LauncherShell()
    {
        _baseDirectory = AppContext.BaseDirectory;
        _slotsPath = SlotStore.DefaultPath(_baseDirectory);
        _state = SlotStore.LoadOrCreate(_slotsPath);
        _log = new FileBotLog(Path.Combine(_baseDirectory, "logs"));
        if (_state.Slots.Count == 0)
        {
            _state.Slots.Add(new ClientSlot());
            Persist();
        }
    }

    public event Action? Changed;
    public Func<string?>? PickClientPath { get; set; }

    public LauncherState State => _state;
    public string ClientPath
    {
        get => _state.ClientPath;
        set
        {
            _state.ClientPath = value ?? "";
            Persist();
            Notify();
        }
    }

    public string Status { get; private set; } = "Pick a slot and press Play.";
    public ClientSlot? Editor { get; private set; }
    public string EditorPassword { get; set; } = "";

    public IReadOnlyList<GameChannel> Channels
    {
        get
        {
            var parsed = ServerInfoParser.FromClientPath(_state.ClientPath);
            if (parsed.Count > 0)
                return parsed;
            var config = BotConfig.CreateDefault();
            return ServerInfoParser.Fallback(config.ServerHost, config.ServerPort, 30001);
        }
    }

    public string SessionStatus(ClientSlot slot)
    {
        lock (_gate)
        {
            if (_sessions.TryGetValue(slot.Id, out var session))
                return session.Status;
        }

        return "idle";
    }

    public bool IsRunning(ClientSlot slot)
    {
        lock (_gate)
            return _sessions.ContainsKey(slot.Id);
    }

    public void Tick()
    {
        lock (_gate)
        {
            foreach (var session in _sessions.Values.ToArray())
            {
                session.Shell.Tick();
                ApplyLiveSession(session);
            }
        }

        Notify();
    }

    public void AddSlot()
    {
        var slot = new ClientSlot();
        _state.Slots.Add(slot);
        Editor = slot;
        EditorPassword = "";
        Persist();
        Notify();
    }

    public void EditSlot(ClientSlot slot)
    {
        Editor = slot;
        EditorPassword = "";
        Notify();
    }

    public void CancelEdit()
    {
        Editor = null;
        EditorPassword = "";
        Notify();
    }

    public void SaveEditor()
    {
        if (Editor == null)
            return;
        if (!string.IsNullOrEmpty(EditorPassword))
            Editor.PasswordProtected = SlotStore.ProtectPassword(EditorPassword);
        var channel = Channels.FirstOrDefault(item => item.Index == Editor.ChannelIndex);
        if (channel != null)
            Editor.ChannelName = channel.Name;
        Editor = null;
        EditorPassword = "";
        Persist();
        Status = "slot saved";
        Notify();
    }

    public void DeleteSlot(ClientSlot slot)
    {
        CloseSlot(slot);
        _state.Slots.Remove(slot);
        if (Editor?.Id == slot.Id)
            CancelEdit();
        Persist();
        Notify();
    }

    public void CloseSlot(ClientSlot slot)
    {
        ClientSession? session;
        lock (_gate)
            _sessions.TryGetValue(slot.Id, out session);
        session?.Close();
        Status = "closed " + slot.DisplayName;
        Notify();
    }

    public void Play(ClientSlot slot)
    {
        lock (_gate)
        {
            if (_sessions.ContainsKey(slot.Id))
            {
                var existing = _sessions[slot.Id];
                if (!existing.Form.IsDisposed)
                {
                    existing.Form.BringToFront();
                    existing.Form.WindowState = FormWindowState.Normal;
                    Status = "already running";
                    Notify();
                    return;
                }
            }
        }

        var path = _state.ClientPath;
        if (!File.Exists(path))
        {
            path = PickClientPath?.Invoke() ?? "";
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Status = "select the Metin2 client executable first";
                Notify();
                return;
            }

            _state.ClientPath = path;
            Persist();
        }

        var channel = Channels.FirstOrDefault(item => item.Index == slot.ChannelIndex) ?? Channels.FirstOrDefault();
        if (channel == null)
        {
            Status = "no channel list was parsed from serverinfo.py";
            Notify();
            return;
        }

        slot.ChannelName = channel.Name;
        var pipe = LoaderOutput.UniquePipeName(slot.Id);
        var host = new ActionModuleHost(pipe);
        if (!host.TryLaunch(path, out var launchStatus, out var pid))
        {
            Status = launchStatus;
            Notify();
            return;
        }

        var sink = new AdapterActionSink(host.PipeName, host.Token);
        if (!host.WaitUntilListening(sink))
        {
            Status = "client launched but companion did not accept auth: " + sink.Status;
            try
            {
                using var process = System.Diagnostics.Process.GetProcessById(pid);
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            sink.Dispose();
            Notify();
            return;
        }

        var configPath = SlotStore.ConfigPath(_baseDirectory, slot.Id);
        var config = SlotStore.LoadBotConfig(_baseDirectory, slot);
        config.ClientPath = path;
        ConfigStore.Save(configPath, config);
        var shell = new FarmBotShell(configPath, pid, host, sink);
        var form = new MainForm(shell, WindowTitle(slot));
        var session = new ClientSession(slot, pid, shell, form, host, sink);
        form.SessionClosing = () => RemoveSession(slot.Id, killClient: true);
        session.Ended += () => RemoveSession(slot.Id, killClient: false);
        lock (_gate)
            _sessions[slot.Id] = session;
        session.Status = "launching";
        form.Show();
        Status = "launched PID " + pid;
        Notify();

        var password = SlotStore.UnprotectPassword(slot.PasswordProtected);
        if (string.IsNullOrWhiteSpace(slot.Account) || string.IsNullOrWhiteSpace(password))
        {
            session.Status = "in-game";
            Status = "client opened — log in manually, then save the slot";
            Notify();
            return;
        }

        Task.Run(() =>
        {
            var play = new QuickPlayOrchestrator(sink, _log);
            var ok = play.TryEnter(slot, channel, password, 180000, out var playStatus);
            session.Status = ok ? "in-game" : "quick-play failed";
            Status = playStatus;
            if (!string.IsNullOrWhiteSpace(slot.CharacterName))
            {
                form.BeginInvoke(() => form.Text = WindowTitle(slot));
                Persist();
            }

            Notify();
        });
    }

    public void ShutdownAll()
    {
        ClientSession[] sessions;
        lock (_gate)
            sessions = _sessions.Values.ToArray();
        foreach (var session in sessions)
            session.Close();
        Persist();
    }

    public void Dispose()
    {
        ShutdownAll();
    }

    private void RemoveSession(string slotId, bool killClient)
    {
        ClientSession? session;
        lock (_gate)
            _sessions.TryGetValue(slotId, out session);
        if (session == null)
            return;
        if (killClient)
            session.KillClient();
        lock (_gate)
            _sessions.Remove(slotId);
        session.Status = "idle";
        Persist();
        Notify();
    }

    private void ApplyLiveSession(ClientSession session)
    {
        var info = session.Sink.LastSession;
        if (session.Sink.Available &&
            Environment.TickCount64 - session.LastCredentialProbeAt >= 1500)
        {
            session.LastCredentialProbeAt = Environment.TickCount64;
            info = session.Sink.GetSession();
        }

        var name = session.Shell.View.Snapshot?.Player?.Name;
        if (!SlotStore.ApplyLiveCredentials(session.Slot, info, name))
            return;

        if (!string.IsNullOrWhiteSpace(session.Slot.CharacterName))
            session.Status = "in-game";
        else if (!string.IsNullOrWhiteSpace(session.Slot.Account))
            session.Status = "account captured";
        session.Form.Text = WindowTitle(session.Slot);
        Persist();
    }

    private void Persist()
    {
        try
        {
            SlotStore.Save(_slotsPath, _state);
        }
        catch (Exception ex)
        {
            Status = "could not save slots: " + ex.Message;
        }
    }

    private void Notify()
    {
        if (_sync != null)
            _sync.Post(_ => Changed?.Invoke(), null);
        else
            Changed?.Invoke();
    }

    private static string WindowTitle(ClientSlot slot)
    {
        var account = string.IsNullOrWhiteSpace(slot.Account) ? "client" : slot.Account;
        var character = string.IsNullOrWhiteSpace(slot.CharacterName) ? "character" : slot.CharacterName;
        return $"FarmBot — {account} / {character}";
    }
}
