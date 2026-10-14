using FarmBot.Core.Bot;
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

public sealed class MainForm : Form
{
    private static readonly Color Back = Color.FromArgb(28, 30, 34);
    private static readonly Color Panel = Color.FromArgb(40, 43, 48);
    private static readonly Color Ink = Color.FromArgb(232, 232, 232);
    private static readonly Color Muted = Color.FromArgb(164, 168, 174);
    private static readonly Color Accent = Color.FromArgb(196, 163, 90);
    private static readonly Color Danger = Color.FromArgb(196, 92, 82);
    private static readonly Color Ok = Color.FromArgb(125, 171, 110);
    private static readonly Color Grid = Color.FromArgb(32, 34, 38);
    private static readonly Color Line = Color.FromArgb(62, 66, 72);

    private readonly string _configPath;
    private readonly object _configGate = new();
    private readonly FileBotLog _log;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 200 };
    private BotConfig _config;
    private BotRuntime? _runtime;
    private PipeHost? _pipe;
    private IGameStateProvider? _provider;

    private ComboBox _mode = null!;
    private Button _start = null!;
    private Button _stop = null!;
    private Label _status = null!;
    private Label _detail = null!;
    private Label _targetBanner = null!;
    private DataGridView _entities = null!;
    private TextBox _logBox = null!;
    private TextBox _windowTitle = null!;
    private TextBox _pipeName = null!;
    private ComboBox _inputMode = null!;
    private CheckBox _huntMonsters = null!;
    private CheckBox _huntStones = null!;
    private NumericUpDown _maxFarmDistance = null!;
    private TextBox _includeVnums = null!;
    private TextBox _excludeVnums = null!;
    private NumericUpDown _attackRange = null!;
    private NumericUpDown _attackInterval = null!;
    private TextBox _attackKey = null!;
    private CheckBox _attackPaths = null!;
    private NumericUpDown _farmRadius = null!;
    private NumericUpDown _centerX = null!;
    private NumericUpDown _centerY = null!;
    private CheckBox _lockCenter = null!;
    private CheckBox _lootEnabled = null!;
    private NumericUpDown _lootRange = null!;
    private TextBox _pickupKey = null!;
    private NumericUpDown _failsafeHp = null!;
    private CheckBox _stopOnCorner = null!;
    private CheckBox _healEnabled = null!;
    private NumericUpDown _healHp = null!;
    private TextBox _healKey = null!;
    private TextBox _blacklist = null!;
    private TextBox _startHotkey = null!;
    private TextBox _stopHotkey = null!;
    private DataGridView _rules = null!;
    private DataGridView _skills = null!;
    private bool _hotkeysRegistered;

    public MainForm()
    {
        _configPath = ConfigStore.DefaultPath(AppContext.BaseDirectory);
        _config = ConfigStore.LoadOrCreate(_configPath);
        _log = new FileBotLog(Path.Combine(AppContext.BaseDirectory, "logs"));
        _log.Written += OnLogWritten;
        BuildUi();
        ApplyConfigToControls();
        WireRuntime();
        _mode.SelectedIndexChanged += (_, _) => OnModeChanged();
        _runtime?.StartLoop();
        _timer.Tick += (_, _) => RefreshView();
        _timer.Start();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _log.Info("FarmBot ready. Simulator mode uses local test entities. Live mode reads 1.metin2client.exe through user-mode ReadProcessMemory.");
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        RegisterHotkeys();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _timer.Stop();
        UnregisterHotkeys();
        try
        {
            PullConfig();
            ConfigStore.Save(_configPath, _config);
        }
        catch (Exception ex)
        {
            _log.Error($"could not save config: {ex.Message}");
        }

        _runtime?.Stop();
        _runtime?.Dispose();
        if (_provider is IDisposable disposable)
            disposable.Dispose();
        _pipe?.Dispose();
        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        const int wmHotkey = 0x0312;
        if (m.Msg == wmHotkey)
        {
            var id = m.WParam.ToInt32();
            if (id == 1)
                ToggleRun();
            else if (id == 2)
                _runtime?.Panic();
        }

        base.WndProc(ref m);
    }

    private void BuildUi()
    {
        Text = "FarmBot";
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Back;
        ForeColor = Ink;
        ClientSize = new Size(1140, 760);
        MinimumSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterScreen;

        var entitiesHost = new Panel { Dock = DockStyle.Fill, BackColor = Back, Padding = new Padding(12, 8, 8, 8) };
        _entities = CreateGrid();
        _entities.Dock = DockStyle.Fill;
        _entities.Columns.Add("Vid", "VID");
        _entities.Columns.Add("Type", "Type");
        _entities.Columns.Add("Vnum", "VNUM");
        _entities.Columns.Add("X", "X");
        _entities.Columns.Add("Y", "Y");
        _entities.Columns.Add("Z", "Z");
        _entities.Columns.Add("Distance", "Distance");
        _entities.Columns.Add("Name", "Name");
        _entities.Columns["Name"]!.FillWeight = 140;
        entitiesHost.Controls.Add(_entities);

        var settingsHost = new Panel { Dock = DockStyle.Right, Width = 430, BackColor = Panel, Padding = new Padding(0) };
        var settings = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Panel };
        var row = 12;
        _windowTitle = TextRow(settings, ref row, "Window title contains");
        _pipeName = TextRow(settings, ref row, "Pipe name");
        _inputMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _inputMode.Items.AddRange(new object[] { "Auto", "IPC", "SendInput" });
        EditorRow(settings, ref row, "Input", _inputMode);
        _huntMonsters = CheckRow(settings, ref row, "Hunt monsters (type 0 ENEMY)");
        _huntStones = CheckRow(settings, ref row, "Hunt Metin stones (type 2 STONE)");
        _maxFarmDistance = NumberRow(settings, ref row, "Max farm distance", 0, 10000000, 0);
        _includeVnums = TextRow(settings, ref row, "Include VNUMs");
        _excludeVnums = TextRow(settings, ref row, "Exclude VNUMs");
        _attackRange = NumberRow(settings, ref row, "Attack range", 1, 100000, 0);
        _attackInterval = NumberRow(settings, ref row, "Attack interval ms", 50, 60000, 0);
        _attackKey = TextRow(settings, ref row, "Attack key");
        _attackPaths = CheckRow(settings, ref row, "Click / attack also paths");
        _farmRadius = NumberRow(settings, ref row, "Farm radius (0 = map)", 0, 10000000, 0);
        _centerX = NumberRow(settings, ref row, "Farm center X", -10000000, 10000000, 1);
        _centerY = NumberRow(settings, ref row, "Farm center Y", -10000000, 10000000, 1);
        _lockCenter = CheckRow(settings, ref row, "Lock center on start");
        _lootEnabled = CheckRow(settings, ref row, "Loot before next target");
        _lootRange = NumberRow(settings, ref row, "Loot range", 0, 100000, 0);
        _pickupKey = TextRow(settings, ref row, "Pickup key");
        _failsafeHp = NumberRow(settings, ref row, "Failsafe HP %", 0, 100, 0);
        _stopOnCorner = CheckRow(settings, ref row, "Stop if cursor is in the corner");
        _healEnabled = CheckRow(settings, ref row, "Heal when HP is low");
        _healHp = NumberRow(settings, ref row, "Heal below HP %", 0, 100, 0);
        _healKey = TextRow(settings, ref row, "Heal key");
        _blacklist = TextRow(settings, ref row, "Name blacklist");
        _startHotkey = TextRow(settings, ref row, "Start hotkey");
        _stopHotkey = TextRow(settings, ref row, "Panic hotkey");

        var rulesLabel = new Label
        {
            Text = "Target priority (higher wins)",
            Left = 12,
            Top = row,
            Width = 390,
            ForeColor = Muted
        };
        settings.Controls.Add(rulesLabel);
        row += 22;
        _rules = CreateGrid();
        _rules.Left = 12;
        _rules.Top = row;
        _rules.Width = 390;
        _rules.Height = 108;
        _rules.AllowUserToAddRows = true;
        _rules.AllowUserToDeleteRows = true;
        var kindColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Kind",
            Name = "Kind",
            FlatStyle = FlatStyle.Flat,
            DataSource = Enum.GetNames<EntityKind>()
        };
        _rules.Columns.Add(kindColumn);
        _rules.Columns.Add("Vnum", "VNUM");
        _rules.Columns.Add("Name", "Name contains");
        _rules.Columns.Add("Priority", "Priority");
        settings.Controls.Add(_rules);
        row += 116;

        var skillsLabel = new Label
        {
            Text = "Skill rotation",
            Left = 12,
            Top = row,
            Width = 390,
            ForeColor = Muted
        };
        settings.Controls.Add(skillsLabel);
        row += 22;
        _skills = CreateGrid();
        _skills.Left = 12;
        _skills.Top = row;
        _skills.Width = 390;
        _skills.Height = 96;
        _skills.AllowUserToAddRows = true;
        _skills.AllowUserToDeleteRows = true;
        foreach (var column in new[] { "Slot", "Key", "Cooldown", "MP", "Min", "Max", "Priority" })
            _skills.Columns.Add(column, column);
        settings.Controls.Add(_skills);
        row += 104;

        var save = new Button
        {
            Text = "Save settings",
            Left = 12,
            Top = row,
            Width = 160,
            Height = 32
        };
        StyleButton(save, Panel, Ink);
        save.Click += (_, _) => SaveSettings(rewirePipe: true);
        settings.Controls.Add(save);
        settings.AutoScrollMinSize = new Size(0, row + 56);
        settingsHost.Controls.Add(settings);

        var logHost = new Panel { Dock = DockStyle.Bottom, Height = 170, BackColor = Back, Padding = new Padding(12, 0, 12, 12) };
        _logBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(22, 23, 26),
            ForeColor = Ink,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 9f),
            WordWrap = false
        };
        logHost.Controls.Add(_logBox);

        var header = new Panel { Dock = DockStyle.Top, Height = 142, BackColor = Panel };
        var title = new Label
        {
            Text = "FarmBot",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Ink,
            AutoSize = true,
            Left = 16,
            Top = 14
        };
        var subtitle = new Label
        {
            Text = "Filter entities and select a farming target",
            ForeColor = Muted,
            AutoSize = true,
            Left = 132,
            Top = 24
        };
        _mode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
        _mode.Items.AddRange(new object[] { "Simulator", "Live client" });
        _mode.SelectedIndex = 0;
        _start = new Button { Text = "Start", Width = 96, Height = 32 };
        _stop = new Button { Text = "Stop", Width = 96, Height = 32, Enabled = false };
        StyleButton(_start, Accent, Color.FromArgb(28, 24, 16));
        StyleButton(_stop, Danger, Color.White);
        _start.Click += (_, _) => StartBot();
        _stop.Click += (_, _) => _runtime?.Stop();
        _status = new Label { AutoSize = false, Left = 16, Top = 58, Height = 22, ForeColor = Ink };
        _detail = new Label { AutoSize = false, Left = 16, Top = 80, Height = 20, ForeColor = Muted };
        _targetBanner = new Label { AutoSize = false, Left = 16, Top = 102, Height = 28, ForeColor = Accent, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };
        header.Resize += (_, _) => LayoutHeader(header);
        header.Controls.AddRange(new Control[] { title, subtitle, _mode, _start, _stop, _status, _detail, _targetBanner });

        Controls.Add(entitiesHost);
        Controls.Add(settingsHost);
        Controls.Add(logHost);
        Controls.Add(header);
        LayoutHeader(header);
    }

    private void LayoutHeader(Control header)
    {
        _stop.Left = header.Width - 112;
        _stop.Top = 16;
        _start.Left = header.Width - 216;
        _start.Top = 16;
        _mode.Left = header.Width - 400;
        _mode.Top = 18;
        _status.Width = header.Width - 32;
        _detail.Width = header.Width - 32;
        _targetBanner.Width = header.Width - 32;
    }

    private void ApplyConfigToControls()
    {
        _windowTitle.Text = string.Join(", ", _config.WindowTitleContains);
        _pipeName.Text = _config.PipeName;
        _huntMonsters.Checked = _config.HuntMonsters;
        _huntStones.Checked = _config.HuntStones;
        _maxFarmDistance.Value = Clamp(_config.MaxFarmDistance, _maxFarmDistance);
        _includeVnums.Text = string.Join(", ", _config.IncludeVnums);
        _excludeVnums.Text = string.Join(", ", _config.ExcludeVnums);
        _inputMode.SelectedIndex = _config.InputMode switch
        {
            InputMode.Ipc => 1,
            InputMode.SendInput => 2,
            _ => 0
        };
        _attackRange.Value = Clamp(_config.AttackRange, _attackRange);
        _attackInterval.Value = Clamp(_config.AttackIntervalMs, _attackInterval);
        _attackKey.Text = _config.AttackKey;
        _attackPaths.Checked = _config.AttackAlsoPaths;
        _farmRadius.Value = Clamp(_config.FarmRadius, _farmRadius);
        _centerX.Value = Clamp(_config.FarmCenterX, _centerX);
        _centerY.Value = Clamp(_config.FarmCenterY, _centerY);
        _lockCenter.Checked = _config.LockCenterOnStart;
        _lootEnabled.Checked = _config.LootBeforeNextTarget && _config.LootEnabled;
        _lootRange.Value = Clamp(_config.LootRange, _lootRange);
        _pickupKey.Text = _config.PickupKey;
        _failsafeHp.Value = Clamp(_config.FailsafeHpPercent, _failsafeHp);
        _stopOnCorner.Checked = _config.StopOnCorner;
        _healEnabled.Checked = _config.HealEnabled;
        _healHp.Value = Clamp(_config.HealHpPercent, _healHp);
        _healKey.Text = _config.HealKey;
        _blacklist.Text = string.Join(", ", _config.NameBlacklist);
        _startHotkey.Text = _config.StartHotkey;
        _stopHotkey.Text = _config.StopHotkey;
        FillRules();
        FillSkills();
    }

    private void PullConfig()
    {
        lock (_configGate)
        {
            _config.WindowTitleContains = Split(_windowTitle.Text);
            _config.PipeName = _pipeName.Text.Trim();
            _config.HuntMonsters = _huntMonsters.Checked;
            _config.HuntStones = _huntStones.Checked;
            _config.MaxFarmDistance = (float)_maxFarmDistance.Value;
            _config.IncludeVnums = ParseUints(_includeVnums.Text);
            _config.ExcludeVnums = ParseUints(_excludeVnums.Text);
            _config.InputMode = _inputMode.SelectedIndex switch
            {
                1 => InputMode.Ipc,
                2 => InputMode.SendInput,
                _ => InputMode.Auto
            };
            _config.AttackRange = (float)_attackRange.Value;
            _config.AttackIntervalMs = (int)_attackInterval.Value;
            _config.AttackKey = _attackKey.Text.Trim();
            _config.AttackAlsoPaths = _attackPaths.Checked;
            _config.FarmRadius = (float)_farmRadius.Value;
            _config.FarmCenterX = (float)_centerX.Value;
            _config.FarmCenterY = (float)_centerY.Value;
            _config.LockCenterOnStart = _lockCenter.Checked;
            _config.LootEnabled = _lootEnabled.Checked;
            _config.LootBeforeNextTarget = _lootEnabled.Checked;
            _config.LootRange = (float)_lootRange.Value;
            _config.PickupKey = _pickupKey.Text.Trim();
            _config.FailsafeHpPercent = (double)_failsafeHp.Value;
            _config.StopOnCorner = _stopOnCorner.Checked;
            _config.HealEnabled = _healEnabled.Checked;
            _config.HealHpPercent = (double)_healHp.Value;
            _config.HealKey = _healKey.Text.Trim();
            _config.NameBlacklist = Split(_blacklist.Text);
            _config.StartHotkey = _startHotkey.Text.Trim();
            _config.StopHotkey = _stopHotkey.Text.Trim();
            _config.TargetRules = ReadRules();
            _config.Skills = ReadSkills();
            _config.Sanitize();
        }
    }

    private BotConfig CopyConfig()
    {
        lock (_configGate)
            return _config.DeepCopy();
    }

    private void WireRuntime()
    {
        _runtime?.Dispose();
        if (_provider is IDisposable disposable)
            disposable.Dispose();
        _provider = null;
        _pipe?.Dispose();
        _pipe = null;

        if (_mode.SelectedIndex <= 0)
        {
            var world = new SimulatedWorld();
            _provider = world;
            _runtime = new BotRuntime(world, world, _log, CopyConfig, () => false);
            return;
        }

        var live = new LiveGameStateProvider(_log);
        _provider = live;
        var sendInput = new SendInputGameControl(
            () => GameWindowFinder.Find(CopyConfig().WindowTitleContains),
            _log);
        _runtime = new BotRuntime(
            live,
            sendInput,
            _log,
            CopyConfig,
            () => CursorProbe.IsInTopLeftCorner());
    }

    private void OnModeChanged()
    {
        var running = _runtime?.Capture().Enabled == true;
        if (running)
            _runtime?.Stop();
        PullConfig();
        WireRuntime();
        _runtime?.StartLoop();
        _log.Info(_mode.SelectedIndex <= 0 ? "simulator mode" : "live client mode — read-only process memory");
    }

    private void StartBot()
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
            _centerX.Value = Clamp(_config.FarmCenterX, _centerX);
            _centerY.Value = Clamp(_config.FarmCenterY, _centerY);
        }

        RegisterHotkeys();
        ConfigStore.Save(_configPath, _config);
        _runtime?.SetEnabled(true);
        _log.Info("bot started");
    }

    private void ToggleRun()
    {
        if (_runtime?.Capture().Enabled == true)
            _runtime.Stop();
        else
            StartBot();
    }

    private void SaveSettings(bool rewirePipe)
    {
        PullConfig();
        ConfigStore.Save(_configPath, _config);
        RegisterHotkeys();
        _ = rewirePipe;
        _log.Info($"settings saved to {_configPath}");
    }

    private void RefreshView()
    {
        if (_runtime == null)
            return;
        try
        {
            RefreshViewCore();
        }
        catch (Exception ex)
        {
            _log.Error($"ui refresh failed: {ex.Message}");
        }
    }

    private void RefreshViewCore()
    {
        PullConfig();
        var runtime = _runtime;
        if (runtime == null)
            return;
        var view = runtime.Capture();
        _start.Enabled = !view.Enabled;
        _stop.Enabled = view.Enabled;

        var player = view.Snapshot?.Player;
        var hp = player == null ? "no state" : $"HP {player.Hp}/{player.MaxHp} ({player.HpPercent:0}%)  MP {player.Mp}/{player.MaxMp}";
        var where = player == null ? "" : $"   pos {player.X:0},{player.Y:0},{player.Z:0}  {player.MapName}";
        _status.Text = $"{view.Phase}  ·  {view.Reason}";
        _status.ForeColor = view.Phase == BotPhase.Failsafe ? Danger : view.Enabled ? Ok : Ink;
        _targetBanner.Text = view.Target == null
            ? "Farming target: none"
            : $"Farming target  VID {view.Target.Vid}  {EntityMapping.TypeName(view.Target.ResolvedType)}  VNUM {view.Target.Vnum}  dist {view.Target.Distance:0}";
        _targetBanner.ForeColor = view.Target == null ? Muted : Accent;

        var feed = _provider?.Status ?? "no provider";
        var window = _mode.SelectedIndex <= 0
            ? "simulator"
            : GameWindowFinder.Find(_config.WindowTitleContains) is { } found
                ? $"window \"{found.Title}\""
                : "game window not found";
        _detail.Text = hp + where + "   " + feed + "   " + window;
        FillEntities(view);
    }

    private void FillEntities(BotView view)
    {
        var entities = view.Snapshot?.Entities ?? Array.Empty<EntityState>();
        _entities.Rows.Clear();
        foreach (var entity in entities.OrderBy(item => item.Distance))
        {
            var index = _entities.Rows.Add(
                entity.Vid.ToString(),
                EntityMapping.TypeName(entity.ResolvedType),
                entity.Vnum.ToString(),
                entity.X.ToString("0.0"),
                entity.Y.ToString("0.0"),
                entity.Z.ToString("0.0"),
                entity.Distance.ToString("0.0"),
                entity.Name);
            if (view.Target?.Vid == entity.Vid)
            {
                _entities.Rows[index].DefaultCellStyle.ForeColor = Accent;
                _entities.Rows[index].DefaultCellStyle.SelectionForeColor = Accent;
            }
        }
    }

    private void OnLogWritten(BotLogEntry entry)
    {
        if (IsDisposed)
            return;
        try
        {
            BeginInvoke(() =>
            {
                if (IsDisposed)
                    return;
                _logBox.AppendText($"{entry.Time:HH:mm:ss}  {entry.Level,-5}  {entry.Message}{Environment.NewLine}");
                var lines = _logBox.Lines;
                if (lines.Length > 400)
                    _logBox.Lines = lines[^300..];
            });
        }
        catch (InvalidOperationException)
        {
            // The window is already closing.
        }
    }

    private void RegisterHotkeys()
    {
        if (!IsHandleCreated)
            return;
        UnregisterHotkeys();
        const uint modNorepeat = 0x4000;
        if (TryParseKey(_config.StartHotkey, out var start))
        {
            if (!RegisterHotKey(Handle, 1, modNorepeat, (uint)start))
                _log.Warn($"could not register start hotkey {_config.StartHotkey}");
        }
        else
        {
            _log.Warn($"unknown start hotkey '{_config.StartHotkey}'");
        }

        if (TryParseKey(_config.StopHotkey, out var stop))
        {
            if (!RegisterHotKey(Handle, 2, modNorepeat, (uint)stop))
                _log.Warn($"could not register panic hotkey {_config.StopHotkey}");
        }
        else
        {
            _log.Warn($"unknown panic hotkey '{_config.StopHotkey}'");
        }

        _hotkeysRegistered = true;
    }

    private void UnregisterHotkeys()
    {
        if (!_hotkeysRegistered || !IsHandleCreated)
            return;
        UnregisterHotKey(Handle, 1);
        UnregisterHotKey(Handle, 2);
        _hotkeysRegistered = false;
    }

    private void FillRules()
    {
        _rules.Rows.Clear();
        foreach (var rule in _config.TargetRules)
        {
            var index = _rules.Rows.Add();
            _rules.Rows[index].Cells[0].Value = rule.Kind.ToString();
            _rules.Rows[index].Cells[1].Value = rule.Vnum == 0 ? "" : rule.Vnum.ToString();
            _rules.Rows[index].Cells[2].Value = rule.NameContains;
            _rules.Rows[index].Cells[3].Value = rule.Priority.ToString();
        }
    }

    private void FillSkills()
    {
        _skills.Rows.Clear();
        foreach (var skill in _config.Skills)
        {
            _skills.Rows.Add(
                skill.Slot.ToString(),
                skill.Key,
                skill.CooldownMs.ToString(),
                skill.MpCost.ToString(),
                skill.MinDistance.ToString("0"),
                skill.MaxDistance.ToString("0"),
                skill.Priority.ToString());
        }
    }

    private List<TargetRule> ReadRules()
    {
        var rules = new List<TargetRule>();
        foreach (DataGridViewRow row in _rules.Rows)
        {
            if (row.IsNewRow)
                continue;
            var kindText = Convert.ToString(row.Cells[0].Value);
            if (!Enum.TryParse<EntityKind>(kindText, true, out var kind))
                continue;
            _ = uint.TryParse(Convert.ToString(row.Cells[1].Value), out var vnum);
            var priority = int.TryParse(Convert.ToString(row.Cells[3].Value), out var parsed) ? parsed : 0;
            rules.Add(new TargetRule
            {
                Kind = kind,
                Vnum = vnum,
                NameContains = Convert.ToString(row.Cells[2].Value) ?? "",
                Priority = priority
            });
        }

        return rules;
    }

    private List<SkillSetting> ReadSkills()
    {
        var skills = new List<SkillSetting>();
        foreach (DataGridViewRow row in _skills.Rows)
        {
            if (row.IsNewRow)
                continue;
            var key = Convert.ToString(row.Cells[1].Value) ?? "";
            if (string.IsNullOrWhiteSpace(key))
                continue;
            skills.Add(new SkillSetting
            {
                Slot = ParseInt(row.Cells[0].Value, 1),
                Key = key.Trim(),
                CooldownMs = ParseInt(row.Cells[2].Value, 1000),
                MpCost = ParseInt(row.Cells[3].Value, 0),
                MinDistance = ParseInt(row.Cells[4].Value, 0),
                MaxDistance = ParseInt(row.Cells[5].Value, 2000),
                Priority = ParseInt(row.Cells[6].Value, 1)
            });
        }

        return skills;
    }

    private static int ParseInt(object? value, int fallback) =>
        int.TryParse(Convert.ToString(value), out var parsed) ? parsed : fallback;

    private static List<string> Split(string text) =>
        text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<uint> ParseUints(string text)
    {
        var values = new List<uint>();
        foreach (var part in Split(text))
        {
            if (uint.TryParse(part, out var value))
                values.Add(value);
        }

        return values;
    }

    private static decimal Clamp(double value, NumericUpDown input)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return input.Minimum;
        decimal converted;
        try
        {
            converted = (decimal)value;
        }
        catch (OverflowException)
        {
            return value > 0 ? input.Maximum : input.Minimum;
        }

        if (converted < input.Minimum)
            return input.Minimum;
        if (converted > input.Maximum)
            return input.Maximum;
        return converted;
    }

    private TextBox TextRow(Control parent, ref int row, string label)
    {
        var box = new TextBox { Width = 230 };
        StyleInput(box);
        EditorRow(parent, ref row, label, box);
        return box;
    }

    private NumericUpDown NumberRow(Control parent, ref int row, string label, decimal min, decimal max, int decimals)
    {
        var input = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            Width = 230,
            ThousandsSeparator = true
        };
        StyleInput(input);
        EditorRow(parent, ref row, label, input);
        return input;
    }

    private CheckBox CheckRow(Control parent, ref int row, string text)
    {
        var box = new CheckBox
        {
            Text = text,
            Left = 12,
            Top = row,
            Width = 390,
            ForeColor = Ink,
            BackColor = Panel
        };
        parent.Controls.Add(box);
        row += 28;
        return box;
    }

    private static void EditorRow(Control parent, ref int row, string label, Control editor)
    {
        var caption = new Label
        {
            Text = label,
            Left = 12,
            Top = row + 4,
            Width = 168,
            ForeColor = Muted,
            BackColor = Panel
        };
        editor.Left = 184;
        editor.Top = row;
        parent.Controls.Add(caption);
        parent.Controls.Add(editor);
        row += 30;
    }

    private DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            BackgroundColor = Grid,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Line,
            ColumnHeadersHeight = 28
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Panel;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Panel;
        grid.DefaultCellStyle.BackColor = Grid;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(62, 56, 40);
        grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.RowTemplate.Height = 24;
        typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(grid, true);
        return grid;
    }

    private static void StyleButton(Button button, Color back, Color fore)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Line;
        button.BackColor = back;
        button.ForeColor = fore;
        button.Cursor = Cursors.Hand;
    }

    private static void StyleInput(Control control)
    {
        control.BackColor = Color.FromArgb(24, 26, 30);
        control.ForeColor = Ink;
    }

    private static bool TryParseKey(string text, out Keys key) =>
        Enum.TryParse(text.Trim(), true, out key) && key != Keys.None;

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
