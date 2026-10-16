using System.Diagnostics;
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
    private readonly Dictionary<string, string> _slotNotes = new();
    private readonly FileBotLog _log;
    private readonly SynchronizationContext? _sync = SynchronizationContext.Current;
    private LauncherState _state;
    private int _launchTicket;
    private string? _busySlotId;
    private string _busyStatus = "";
    private int _launchPid;
    private AdapterActionSink? _launchSink;
    private bool _disposed;

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
    public IReadOnlyList<ClientSlot> VisibleSlots => _state.Slots;
    public bool HasRunningSessions
    {
        get
        {
            lock (_gate)
                return _sessions.Count > 0 || _busySlotId != null;
        }
    }

    public string ProfileName
    {
        get => _state.Profile;
        set => SetProfile(SlotStore.ParseProfile(value));
    }

    public ClientProfile Profile
    {
        get => _state.SelectedProfile;
        set => SetProfile(value);
    }

    public string ClientPath
    {
        get => _state.ClientPath;
        set
        {
            _state.Active.ClientPath = value ?? "";
            _state.ClientPath = _state.Active.ClientPath;
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
            if (_busySlotId == slot.Id && _busyStatus.Length > 0)
                return _busyStatus;
            if (_sessions.TryGetValue(slot.Id, out var session))
                return session.Status;
            if (_slotNotes.TryGetValue(slot.Id, out var note))
                return note;
        }

        return "idle";
    }

    public bool IsRunning(ClientSlot slot)
    {
        lock (_gate)
            return _sessions.ContainsKey(slot.Id);
    }

    public bool IsBusy(ClientSlot slot)
    {
        lock (_gate)
            return _busySlotId == slot.Id;
    }

    public bool SlotsLocked
    {
        get
        {
            lock (_gate)
                return _busySlotId != null;
        }
    }

    public void Tick()
    {
        try
        {
            lock (_gate)
            {
                foreach (var session in _sessions.Values.ToArray())
                {
                    try
                    {
                        session.Shell.Tick();
                        ApplyLiveSession(session);
                    }
                    catch (Exception ex)
                    {
                        session.Status = "session error";
                        Status = "session error: " + ex.Message;
                    }
                }
            }

            Notify();
        }
        catch (Exception ex)
        {
            Status = "launcher tick: " + ex.Message;
        }
    }

    public void SetProfile(ClientProfile profile)
    {
        var name = SlotStore.ProfileName(profile);
        if (string.Equals(_state.Profile, name, StringComparison.Ordinal))
            return;
        _state.Profile = name;
        SlotStore.MirrorActive(_state);
        if (Editor != null && !_state.Slots.Contains(Editor))
        {
            Editor = null;
            EditorPassword = "";
        }

        if (_state.Slots.Count == 0)
            _state.Slots.Add(new ClientSlot());
        Status = profile switch
        {
            ClientProfile.Baerim => "Baerim mode — walk to the target, then attack.",
            ClientProfile.Arthion => "Arthion mode — walk to the target, then attack.",
            _ => "Normal mode — original Metin2 client loading and combat."
        };
        Persist();
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
        if (SlotsLocked)
            return;
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
        if (SlotsLocked)
            return;
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
        var cancelLaunch = false;
        lock (_gate)
        {
            _sessions.TryGetValue(slot.Id, out session);
            if (_busySlotId == slot.Id)
            {
                _launchTicket++;
                _busySlotId = null;
                _busyStatus = "";
                cancelLaunch = true;
            }
        }

        if (cancelLaunch)
            ReleaseLaunchProcess();
        session?.Close();
        Status = "closed " + slot.DisplayName;
        Notify();
    }

    public void Play(ClientSlot slot)
    {
        ClientSession? stale = null;
        lock (_gate)
        {
            if (_sessions.TryGetValue(slot.Id, out var existing))
            {
                if (existing.ClientAlive && !existing.Form.IsDisposed)
                {
                    existing.Form.BringToFront();
                    existing.Form.WindowState = FormWindowState.Normal;
                    Status = "already running";
                    Notify();
                    return;
                }

                _sessions.Remove(slot.Id);
                stale = existing;
            }
        }

        stale?.Close();

        if (SlotsLocked)
        {
            Status = "already opening a client";
            Notify();
            return;
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
        var profile = _state.SelectedProfile;
        var ticket = BeginLaunch(slot, "waiting for client to open...");
        Status = "waiting for client to open...";
        Notify();
        Task.Run(() => OpenClient(slot, path, profile, channel, ticket));
    }

    public void ShutdownAll()
    {
        ClientSession[] sessions;
        lock (_gate)
        {
            _launchTicket++;
            _busySlotId = null;
            _busyStatus = "";
            sessions = _sessions.Values.ToArray();
        }

        ReleaseLaunchProcess();
        foreach (var session in sessions)
            session.Close();
        Persist();
    }

    public void Dispose()
    {
        lock (_gate)
            _disposed = true;
        ShutdownAll();
    }

    private void RemoveSession(string slotId, bool killClient)
    {
        ClientSession? session;
        lock (_gate)
        {
            _sessions.TryGetValue(slotId, out session);
            if (_busySlotId == slotId)
            {
                _launchTicket++;
                _busySlotId = null;
                _busyStatus = "";
            }

            if (session != null)
                _sessions.Remove(slotId);
        }

        if (session == null)
        {
            Notify();
            return;
        }

        if (killClient)
            session.KillClient();
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

        if (_busySlotId != session.Slot.Id)
        {
            if (!string.IsNullOrWhiteSpace(session.Slot.CharacterName))
                session.Status = "in-game";
            else if (!string.IsNullOrWhiteSpace(session.Slot.Account))
                session.Status = "account captured";
        }
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

    private void OpenClient(ClientSlot slot, string path, ClientProfile profile, GameChannel channel, int ticket)
    {
        var launchedPid = 0;
        AdapterActionSink? launchedSink = null;
        var published = false;
        try
        {
            if (!IsCurrent(ticket))
                return;

            StopStrayClients(path, message => SetBusy(ticket, message));
            if (!IsCurrent(ticket))
                return;

            SetBusy(ticket, "starting client...");
            var pipe = LoaderOutput.UniquePipeName(slot.Id);
            var host = new ActionModuleHost(pipe) { Profile = profile };
            if (!host.TryLaunch(path, out var launchStatus, out var pid))
            {
                FailBusy(ticket, launchStatus);
                return;
            }

            launchedPid = pid;
            var sink = new AdapterActionSink(host.PipeName, host.Token);
            launchedSink = sink;
            if (!PublishLaunch(ticket, pid, sink))
            {
                KillPid(pid);
                sink.Dispose();
                return;
            }

            published = true;
            SetBusy(ticket, "connecting to client...");
            var listening = false;
            try
            {
                listening = host.WaitUntilListening(sink);
            }
            catch
            {
                listening = false;
            }

            if (!IsCurrent(ticket))
                return;

            if (!listening)
            {
                var status = "client launched but companion did not accept auth: " + sink.Status;
                if (!ClaimLaunch(ticket, out var ownedPid, out var ownedSink))
                    return;
                KillPid(ownedPid);
                ownedSink?.Dispose();
                FailBusy(ticket, status);
                return;
            }

            PostUi(() => FinishLaunch(slot, path, profile, channel, host, sink, pid, ticket));
        }
        catch (Exception ex)
        {
            if (!published)
            {
                KillPid(launchedPid);
                launchedSink?.Dispose();
            }
            else if (ClaimLaunch(ticket, out var ownedPid, out var ownedSink))
            {
                KillPid(ownedPid);
                ownedSink?.Dispose();
            }

            FailBusy(ticket, "launch failed: " + ex.Message);
        }
    }

    private void FinishLaunch(
        ClientSlot slot,
        string path,
        ClientProfile profile,
        GameChannel channel,
        ActionModuleHost host,
        AdapterActionSink sink,
        int pid,
        int ticket)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket || _disposed)
                return;
            _launchPid = 0;
            _launchSink = null;
        }

        MainForm? form = null;
        ClientSession? session = null;
        var published = false;
        try
        {
            var configPath = SlotStore.ConfigPath(_baseDirectory, slot.Id);
            var config = SlotStore.LoadBotConfig(_baseDirectory, slot);
            config.ClientPath = path;
            config.ClientProfile = profile;
            ConfigStore.Save(configPath, config);
            var shell = new FarmBotShell(configPath, pid, host, sink);
            form = new MainForm(shell, WindowTitle(slot));
            session = new ClientSession(slot, pid, shell, form, host, sink);
            form.SessionClosing = () => RemoveSession(slot.Id, killClient: true);
            session.Ended += () =>
            {
                session.Status = "client exited";
                Status = "client exited — launcher stays open";
                lock (_gate)
                {
                    if (_busySlotId == slot.Id && ticket == _launchTicket)
                    {
                        _launchTicket++;
                        _busySlotId = null;
                        _busyStatus = "";
                    }
                }

                Notify();
            };

            lock (_gate)
            {
                if (ticket != _launchTicket || _disposed)
                {
                    session.Dispose();
                    return;
                }

                _sessions[slot.Id] = session;
                published = true;
            }

            session.Status = "client opened";
            form.Show();
            if (!IsCurrent(ticket))
                return;

            var password = SlotStore.UnprotectPassword(slot.PasswordProtected);
            if (profile == ClientProfile.Baerim ||
                string.IsNullOrWhiteSpace(slot.Account) ||
                string.IsNullOrWhiteSpace(password))
            {
                session.Status = "client opened";
                Status = profile == ClientProfile.Baerim
                    ? "client opened — log in in-game, then start the bot"
                    : "client opened — log in manually, then save the slot";
                ClearBusy(ticket, null);
                Notify();
                return;
            }

            SetBusy(ticket, "logging in...");
            session.Status = "logging in...";
            Task.Run(() =>
            {
                var play = new QuickPlayOrchestrator(sink, _log);
                var ok = play.TryEnter(slot, channel, password, 180000, out var playStatus, message =>
                {
                    session.Status = message;
                    SetBusy(ticket, message);
                });
                if (!IsCurrent(ticket))
                    return;
                lock (_gate)
                {
                    if (!_sessions.ContainsKey(slot.Id))
                        return;
                }

                session.Status = string.IsNullOrWhiteSpace(playStatus)
                    ? (ok ? "in-game" : "quick-play failed")
                    : playStatus;
                Status = playStatus;
                if (!string.IsNullOrWhiteSpace(slot.CharacterName))
                {
                    try
                    {
                        if (!form.IsDisposed && form.IsHandleCreated)
                            form.BeginInvoke(() => form.Text = WindowTitle(slot));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    Persist();
                }

                ClearBusy(ticket, null);
                Notify();
            });
        }
        catch (Exception ex)
        {
            if (published)
            {
                lock (_gate)
                    _sessions.Remove(slot.Id);
            }

            if (session != null)
                session.Dispose();
            else
                form?.Dispose();
            KillPid(pid);
            try
            {
                sink.Dispose();
            }
            catch
            {
            }

            FailBusy(ticket, "launch failed: " + ex.Message);
        }
    }

    private int BeginLaunch(ClientSlot slot, string message)
    {
        lock (_gate)
        {
            _launchTicket++;
            _busySlotId = slot.Id;
            _busyStatus = message;
            _slotNotes.Remove(slot.Id);
            _launchPid = 0;
            _launchSink = null;
            return _launchTicket;
        }
    }

    private void SetBusy(int ticket, string message)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket || _busySlotId == null)
                return;
            _busyStatus = message;
        }

        Status = message;
        Notify();
    }

    private void FailBusy(int ticket, string message)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket)
                return;
            if (_busySlotId != null)
                _slotNotes[_busySlotId] = message;
            _busySlotId = null;
            _busyStatus = "";
        }

        Status = message;
        Notify();
    }

    private void ClearBusy(int ticket, string? note)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket)
                return;
            if (_busySlotId != null)
            {
                if (note == null)
                    _slotNotes.Remove(_busySlotId);
                else
                    _slotNotes[_busySlotId] = note;
            }

            _busySlotId = null;
            _busyStatus = "";
        }
    }

    private bool IsCurrent(int ticket)
    {
        lock (_gate)
            return ticket == _launchTicket && !_disposed;
    }

    private bool PublishLaunch(int ticket, int pid, AdapterActionSink sink)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket || _disposed)
                return false;
            _launchPid = pid;
            _launchSink = sink;
            return true;
        }
    }

    private bool ClaimLaunch(int ticket, out int pid, out AdapterActionSink? sink)
    {
        lock (_gate)
        {
            if (ticket != _launchTicket)
            {
                pid = 0;
                sink = null;
                return false;
            }

            pid = _launchPid;
            sink = _launchSink;
            _launchPid = 0;
            _launchSink = null;
            return true;
        }
    }

    private void ReleaseLaunchProcess()
    {
        int pid;
        AdapterActionSink? sink;
        lock (_gate)
        {
            pid = _launchPid;
            sink = _launchSink;
            _launchPid = 0;
            _launchSink = null;
        }

        KillPid(pid);
        sink?.Dispose();
    }

    private void PostUi(Action action)
    {
        if (_sync != null)
            _sync.Post(_ => action(), null);
        else
            action();
    }

    private static void KillPid(int pid)
    {
        if (pid <= 0)
            return;
        try
        {
            using var process = Process.GetProcessById(pid);
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
        }
    }

    private static void StopStrayClients(string exePath, Action<string>? phase = null)
    {
        var name = Path.GetFileNameWithoutExtension(exePath);
        if (string.IsNullOrWhiteSpace(name))
            return;
        var deadline = DateTime.UtcNow.AddSeconds(8);
        var killed = false;
        while (DateTime.UtcNow < deadline)
        {
            var leftover = Process.GetProcessesByName(name)
                .Concat(Process.GetProcessesByName("FarmBotLoad"))
                .ToArray();
            if (leftover.Length == 0)
                break;
            if (!killed)
                phase?.Invoke("closing leftover clients...");
            killed = true;

            foreach (var process in leftover)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(2000);
                }
                catch
                {
                }
                finally
                {
                    process.Dispose();
                }
            }

            Thread.Sleep(200);
        }

        // Only settle when we actually killed something; never burn a fixed 8s on a clean launch.
        if (killed)
            Thread.Sleep(400);
    }

    private static string WindowTitle(ClientSlot slot)
    {
        var account = string.IsNullOrWhiteSpace(slot.Account) ? "client" : slot.Account;
        var character = string.IsNullOrWhiteSpace(slot.CharacterName) ? "character" : slot.CharacterName;
        return $"FarmBot — {account} / {character}";
    }
}
