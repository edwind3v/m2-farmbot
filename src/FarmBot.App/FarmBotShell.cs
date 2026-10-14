using FarmBot.Core.Bot;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Input;
using FarmBot.Core.Ipc;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;
using FarmBot.Platform.Client;
using FarmBot.Platform.Input;
using FarmBot.Platform.Window;

namespace FarmBot;

public sealed class FarmBotShell : IDisposable
{
    private readonly string _configPath;
    private readonly object _configGate = new();
    private readonly FileBotLog _log;
    private readonly List<BotLogEntry> _logLines = new();
    private BotConfig _config;
    private BotRuntime? _runtime;
    private PipeHost? _pipe;
    private IGameStateProvider? _provider;
    private ActionModuleHost? _actionHost;
    private AdapterActionSink? _actionSink;
    private readonly bool _ownsCompanion;
    private bool _live = true;
    private AtlasPixels? _atlas;
    private string _atlasMap = "";
    private long _atlasAt;
    private float _atlasX;
    private float _atlasY;
    private bool _potsArmed;

    public FarmBotShell()
        : this(ConfigStore.DefaultPath(AppContext.BaseDirectory), 0, null, null)
    {
    }

    public FarmBotShell(string configPath, int targetPid, ActionModuleHost? host, AdapterActionSink? sink)
    {
        _configPath = configPath;
        BoundClientPid = targetPid;
        _actionHost = host;
        _actionSink = sink;
        _ownsCompanion = host == null;
        _config = ConfigStore.LoadOrCreate(_configPath);
        _config.MaxFarmDistance = 8000;
        SyncTextsFromConfig();
        _log = new FileBotLog(Path.Combine(AppContext.BaseDirectory, "logs"));
        _log.Written += OnLogWritten;
        WireRuntime();
        _runtime?.StartLoop();
        View = _runtime?.Capture() ?? new BotView(false, BotPhase.Stopped, "stopped", null, null);
        _log.Info(targetPid > 0
            ? $"FarmBot ready for client PID {targetPid}."
            : "FarmBot ready. Live mode: Attach or Launch the local client, then Start.");
    }

    public event Action? Changed;
    public Func<string?>? PickClientPath { get; set; }

    public BotConfig Config
    {
        get { lock (_configGate) return _config; }
    }

    public BotView View { get; private set; }
    public IReadOnlyList<ClientProcessInfo> Clients { get; private set; } = Array.Empty<ClientProcessInfo>();
    public int SelectedClientPid { get; set; }
    public int BoundClientPid { get; private set; }
    public bool IsLive => _live;
    public bool CompanionConnected => _actionSink?.Available == true;
    public AdapterActionSink? ActionSink => _actionSink;

    public int ModeIndex
    {
        get => _live ? 1 : 0;
        set => SetLive(value > 0);
    }

    public string IncludeVnumsText { get; set; } = "";
    public string ExcludeVnumsText { get; set; } = "";
    public string BlacklistText { get; set; } = "";
    public string WindowTitleText { get; set; } = "";
    public string InputModeText { get; set; } = "Auto";

    public IReadOnlyList<BotLogEntry> LogLines
    {
        get
        {
            lock (_logLines)
                return _logLines.ToArray();
        }
    }

    public string CompanionStatus =>
        !_live ? "simulator"
        : CompanionConnected ? _actionSink!.Status
        : _actionHost?.Status ?? "companion not loaded";

    public string StatusLine
    {
        get
        {
            var view = View;
            var target = view.Target == null
                ? "no target"
                : $"VID {view.Target.Vid}  {EntityMapping.TypeName(view.Target.ResolvedType)}  {view.Target.Distance:0}";
            return view.Enabled
                ? $"{view.CombatState}  ·  {target}  ·  {view.Reason}"
                : $"{view.Phase}  ·  {view.Reason}";
        }
    }

    public bool RangedOnly => View.RangedOnly;

    public string StatusKind =>
        View.Phase == BotPhase.Failsafe ? "fail" : View.Enabled ? "run" : "";

    public string AtlasUrl { get; private set; } = "";

    public float MapViewRadius
    {
        get
        {
            var player = View.Snapshot?.Player;
            return AtlasPixels.MinimapViewRadius(
                _config.MapZoom,
                player?.MapWorldW ?? 0,
                player?.MapWorldH ?? 0);
        }
    }

    public void SetFarmRadius(float value)
    {
        lock (_configGate)
            _config.FarmRadius = Math.Clamp(value, 1000, 8000);
        Notify();
    }

    public void SetMapZoom(float value)
    {
        lock (_configGate)
            _config.MapZoom = Math.Clamp(value, 20, 100);
        Notify();
    }

    public bool ShowPotions
    {
        get
        {
            lock (_configGate)
                return _config.ClientProfile == ClientProfile.Arthion;
        }
    }

    public IReadOnlyList<PotionStack> Potions => View.Potions ?? Array.Empty<PotionStack>();

    public bool PotionEnabled(uint vnum)
    {
        lock (_configGate)
            return _config.PotionEnabledVnums.Contains(vnum);
    }

    private void ArmFoundPots()
    {
        if (_potsArmed)
            return;
        var pots = View.Potions;
        if (pots == null || pots.Count == 0)
            return;
        lock (_configGate)
        {
            if (_config.PotionEnabledVnums.Count == 0)
            {
                foreach (var pot in pots)
                    _config.PotionEnabledVnums.Add(pot.Vnum);
            }
            _potsArmed = true;
        }
    }

    public void TogglePotion(uint vnum)
    {
        lock (_configGate)
        {
            if (!_config.PotionEnabledVnums.Remove(vnum))
                _config.PotionEnabledVnums.Add(vnum);
        }

        SaveSettings();
    }

    public void SetPotionHpPercent(double value)
    {
        lock (_configGate)
            _config.PotionHpPercent = Math.Clamp(value, 0, 100);
        SaveSettings();
    }

    public void SetPotionMpPercent(double value)
    {
        lock (_configGate)
            _config.PotionMpPercent = Math.Clamp(value, 0, 100);
        SaveSettings();
    }

    public IReadOnlyList<uint> BossVnums()
    {
        lock (_configGate)
            return _config.TargetRules
                .Where(item => item.Kind == EntityKind.Boss && item.Vnum != 0)
                .Select(item => item.Vnum)
                .ToList();
    }

    public void Tick()
    {
        try
        {
            var runtime = _runtime;
            if (runtime == null)
                return;
            View = runtime.Capture();
            ArmFoundPots();
            RefreshAtlas();
            Notify();
        }
        catch (Exception ex)
        {
            _log.Error($"ui refresh failed: {ex.Message}");
        }
    }

    public void SetLive(bool live)
    {
        if (_live == live)
            return;
        if (_runtime?.Capture().Enabled == true)
            _runtime.Stop();
        PullConfig();
        _live = live;
        WireRuntime();
        _runtime?.StartLoop();
        _log.Info(live
            ? "live client mode — Attach or Launch, then Start"
            : "simulator mode");
        Tick();
    }

    public void AttachSelectedClient()
    {
        RefreshClientList();
        if (_actionHost == null || _actionSink == null)
        {
            _log.Warn("switch to Live client first");
            return;
        }

        var client = Clients.FirstOrDefault(item => item.ProcessId == SelectedClientPid);
        if (client == null)
        {
            _log.Warn("no running Metin2 client to attach");
            return;
        }

        if (!_actionHost.TryAttach(client.ProcessId, out var status))
        {
            _log.Error(status);
            return;
        }

        if (!_actionHost.WaitUntilListening(_actionSink))
        {
            _log.Error("companion loaded but pipe did not accept auth: " + _actionSink.Status);
            return;
        }

        _atlas = null;
        _atlasMap = "";
        _log.Info($"companion attached to PID {client.ProcessId}: {_actionSink.Status}");
        Notify();
    }

    public void LaunchClient()
    {
        if (_actionHost == null || _actionSink == null)
        {
            _log.Warn("switch to Live client first");
            return;
        }

        var path = _config.ClientPath;
        if (!File.Exists(path))
        {
            path = PickClientPath?.Invoke() ?? "";
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;
            lock (_configGate)
                _config.ClientPath = path;
        }

        if (!_actionHost.TryLaunch(path, out var status))
        {
            _log.Error(status);
            return;
        }

        if (!_actionHost.WaitUntilListening(_actionSink))
        {
            _log.Error("client launched but companion pipe did not accept auth: " + _actionSink.Status);
            return;
        }

        RefreshClientList();
        _atlas = null;
        _atlasMap = "";
        _log.Info($"companion launched {path}: {_actionSink.Status}");
        Notify();
    }

    public void StartBot()
    {
        PullConfig();
        var snapshot = _runtime?.Capture().Snapshot;
        if (_config.LockCenterOnStart && snapshot != null)
        {
            lock (_configGate)
            {
                _config.FarmCenterX = snapshot.Player.X;
                _config.FarmCenterY = snapshot.Player.Y;
            }
        }

        if (_live && !EnsureCompanion())
            return;

        ConfigStore.Save(_configPath, _config);
        _runtime?.SetEnabled(true);
        _log.Info("bot started");
    }

    public void StartRanged()
    {
        PullConfig();
        lock (_configGate)
            _config.RangedAttackEnabled = true;

        if (_live && !EnsureCompanion())
            return;

        ConfigStore.Save(_configPath, _config);
        _runtime?.SetRangedOnly(true);
        _log.Info("ranged attack started; follows the player");
    }

    public void StopBot() => _runtime?.Stop();

    public void ToggleRun()
    {
        if (_runtime?.Capture().Enabled == true)
            _runtime.Stop();
        else
            StartBot();
    }

    public void Panic() => _runtime?.Panic();

    public void SaveSettings()
    {
        PullConfig();
        ConfigStore.Save(_configPath, _config);
        _log.Info($"settings saved to {_configPath}");
        Notify();
    }

    public void PersistOnExit()
    {
        try
        {
            PullConfig();
            ConfigStore.Save(_configPath, _config);
        }
        catch (Exception ex)
        {
            _log.Error($"could not save config: {ex.Message}");
        }
    }

    public void AddRule()
    {
        lock (_configGate)
            _config.TargetRules.Add(new TargetRule { Kind = EntityKind.Monster, Priority = 10 });
        Notify();
    }

    public void RemoveRule(TargetRule rule)
    {
        lock (_configGate)
            _config.TargetRules.Remove(rule);
        Notify();
    }

    public void AddSkill()
    {
        lock (_configGate)
            _config.Skills.Add(new SkillSetting());
        Notify();
    }

    public void RemoveSkill(SkillSetting skill)
    {
        lock (_configGate)
            _config.Skills.Remove(skill);
        Notify();
    }

    public void Dispose()
    {
        _log.Written -= OnLogWritten;
        _runtime?.Stop();
        _runtime?.Dispose();
        if (_provider is IDisposable disposable)
            disposable.Dispose();
        _pipe?.Dispose();
        _actionSink?.Dispose();
        _actionSink = null;
        _actionHost = null;
    }

    private void RefreshAtlas()
    {
        var player = View.Snapshot?.Player;
        var map = player?.MapName ?? "";
        if (!_live || string.IsNullOrWhiteSpace(map))
        {
            AtlasUrl = "";
            return;
        }

        if (_atlas == null || !string.Equals(_atlasMap, map, StringComparison.OrdinalIgnoreCase))
        {
            if (!AtlasPixels.TryLoadFromClient(_config.ClientPath, map, player?.MapWorldW ?? 0, player?.MapWorldH ?? 0, out var atlas))
            {
                AtlasUrl = "";
                return;
            }

            _atlas = atlas;
            _atlasMap = map;
            _log.Info($"atlas {map} {atlas.Width}x{atlas.Height}  world {atlas.WorldW:0} x {atlas.WorldH:0}");
        }

        var x = player?.X ?? 0;
        var y = player?.Y ?? 0;
        var dx = x - _atlasX;
        var dy = y - _atlasY;
        if (AtlasUrl.Length > 0 && dx * dx + dy * dy < 40_000 && Environment.TickCount64 - _atlasAt < 500)
            return;
        _atlasAt = Environment.TickCount64;
        _atlasX = x;
        _atlasY = y;
        var worldW = player is { MapWorldW: >= 1000 } ? player.MapWorldW : _atlas.WorldW;
        var worldH = player is { MapWorldH: >= 1000 } ? player.MapWorldH : _atlas.WorldH;
        AtlasUrl = _atlas.ToCroppedBmpUrl(x, y, MapViewRadius, worldW, worldH);
    }

    private bool EnsureCompanion()
    {
        if (_actionSink?.Available == true)
            return true;
        if (BoundClientPid > 0)
            SelectedClientPid = BoundClientPid;
        RefreshClientList();
        if (Clients.Any(item => item.ProcessId == SelectedClientPid))
        {
            AttachSelectedClient();
            if (_actionSink?.Available == true)
                return true;
        }

        _log.Error("companion is not connected. Attach a running client or Launch the original exe first.");
        return false;
    }

    private void WireRuntime()
    {
        _runtime?.Dispose();
        if (_provider is IDisposable disposable)
            disposable.Dispose();
        _provider = null;
        if (_ownsCompanion)
        {
            _actionSink?.Dispose();
            _actionSink = null;
            _actionHost = null;
        }
        _pipe?.Dispose();
        _pipe = null;

        if (!_live)
        {
            var world = new SimulatedWorld();
            _provider = world;
            _runtime = new BotRuntime(world, world, _log, CopyConfig, () => false);
            return;
        }

        var live = new LiveGameStateProvider(_log, BoundClientPid);
        _provider = live;
        _actionHost ??= new ActionModuleHost { Profile = _config.ClientProfile };
        _actionSink ??= new AdapterActionSink(_actionHost.PipeName, _actionHost.Token);
        if (BoundClientPid > 0)
            SelectedClientPid = BoundClientPid;
        RefreshClientList();
        var native = new NativeGameControl(_actionSink, _log);
        _runtime = new BotRuntime(
            live,
            native,
            _log,
            CopyConfig,
            () => CursorProbe.IsInTopLeftCorner(),
            nativeCombat: true,
            _actionSink);
    }

    private void RefreshClientList()
    {
        var selected = SelectedClientPid;
        var clients = ActionModuleHost.ListClients(_config.ClientPath);
        Clients = clients;
        if (clients.Count == 0)
        {
            SelectedClientPid = 0;
            return;
        }

        if (clients.Any(item => item.ProcessId == selected))
            SelectedClientPid = selected;
        else
            SelectedClientPid = clients[0].ProcessId;
    }

    private void PullConfig()
    {
        lock (_configGate)
        {
            _config.IncludeVnums = ParseUints(IncludeVnumsText);
            _config.ExcludeVnums = ParseUints(ExcludeVnumsText);
            _config.NameBlacklist = Split(BlacklistText);
            _config.WindowTitleContains = Split(WindowTitleText);
            _config.InputMode = InputModeText switch
            {
                "IPC" => InputMode.Ipc,
                "SendInput" => InputMode.SendInput,
                _ => InputMode.Auto
            };
            _config.LootBeforeNextTarget = _config.LootEnabled;
            _config.MaxFarmDistance = 8000;
            _config.Sanitize();
        }
    }

    private BotConfig CopyConfig()
    {
        lock (_configGate)
            return _config.DeepCopy();
    }

    private void SyncTextsFromConfig()
    {
        IncludeVnumsText = string.Join(", ", _config.IncludeVnums);
        ExcludeVnumsText = string.Join(", ", _config.ExcludeVnums);
        BlacklistText = string.Join(", ", _config.NameBlacklist);
        WindowTitleText = string.Join(", ", _config.WindowTitleContains);
        InputModeText = _config.InputMode switch
        {
            InputMode.Ipc => "IPC",
            InputMode.SendInput => "SendInput",
            _ => "Auto"
        };
    }

    private void OnLogWritten(BotLogEntry entry)
    {
        lock (_logLines)
        {
            _logLines.Add(entry);
            if (_logLines.Count > 400)
                _logLines.RemoveRange(0, _logLines.Count - 300);
        }
    }

    private void Notify() => Changed?.Invoke();

    private static List<uint> ParseUints(string text) =>
        Split(text).Select(item => uint.TryParse(item, out var value) ? value : 0).Where(value => value != 0).ToList();

    private static List<string> Split(string text) =>
        (text ?? "").Split(new[] { ',', ';', '\n' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
}
