using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Ui;

internal sealed class SettingsPanel : UserControl
{
    private readonly Panel _host = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Card };
    private readonly List<Button> _segments = new();
    private Control[] _pages = Array.Empty<Control>();
    public readonly CheckBox HuntMonsters = Switch("Hunt monsters");
    public readonly CheckBox HuntStones = Switch("Hunt Metin stones");
    public readonly CheckBox LockCenter = Switch("Lock farm center on start");
    public readonly CheckBox AttackPaths = Switch("Attack also paths");
    public readonly CheckBox LootEnabled = Switch("Loot before next target");
    public readonly CheckBox StopOnCorner = Switch("Stop if cursor is in the corner");
    public readonly CheckBox HealEnabled = Switch("Heal when HP is low");
    public readonly ValueSlider MaxFarmDistance = Slider("Max farm distance", 0, 8000);
    public readonly ValueSlider FarmRadius = Slider("Farm radius", 0, 8000);
    public readonly ValueSlider AttackRange = Slider("Attack range", 1, 2000);
    public readonly ValueSlider AttackInterval = Slider("Attack interval", 50, 2000, "ms");
    public readonly ValueSlider LootRange = Slider("Loot range", 0, 2000);
    public readonly ValueSlider FailsafeHp = Slider("Failsafe HP", 0, 100, "%");
    public readonly ValueSlider HealHp = Slider("Heal below HP", 0, 100, "%");
    public readonly TextBox IncludeVnums = Field();
    public readonly TextBox ExcludeVnums = Field();
    public readonly NumericUpDown CenterX = Number(-10_000_000, 10_000_000, 1);
    public readonly NumericUpDown CenterY = Number(-10_000_000, 10_000_000, 1);
    public readonly TextBox AttackKey = Field();
    public readonly TextBox PickupKey = Field();
    public readonly TextBox HealKey = Field();
    public readonly TextBox Blacklist = Field();
    public readonly TextBox StartHotkey = Field();
    public readonly TextBox StopHotkey = Field();
    public readonly TextBox WindowTitle = Field();
    public readonly TextBox PipeName = Field();
    public readonly ComboBox InputMode = Combo("Auto", "IPC", "SendInput");
    public readonly DataGridView Rules = new();
    public readonly DataGridView Skills = new();
    public readonly Button Save = new() { Text = "Save", Width = 88, Height = 28 };

    public SettingsPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Canvas;
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(4, 4, 4, 0),
            BackColor = UiTheme.Card,
            WrapContents = false
        };
        _pages = new Control[] { FarmPage(), CombatPage(), SafetyPage(), AdvancedPage() };
        var titles = new[] { "Farm", "Combat", "Safety", "Advanced" };
        for (var i = 0; i < titles.Length; i++)
        {
            var index = i;
            var button = new Button
            {
                Text = titles[i],
                Width = 88,
                Height = 26,
                Margin = new Padding(2, 0, 2, 0)
            };
            button.Click += (_, _) => ShowPage(index);
            _segments.Add(button);
            bar.Controls.Add(button);
        }

        UiTheme.StyleButton(Save, Color.FromArgb(33, 28, 23), UiTheme.Ink, true);
        UiTheme.StyleGrid(Rules);
        UiTheme.StyleGrid(Skills);
        Rules.AllowUserToAddRows = true;
        Rules.AllowUserToDeleteRows = true;
        Skills.AllowUserToAddRows = true;
        Skills.AllowUserToDeleteRows = true;
        Rules.Columns.Add(new DataGridViewComboBoxColumn
        {
            HeaderText = "Kind",
            Name = "Kind",
            FlatStyle = FlatStyle.Flat,
            DataSource = Enum.GetNames<EntityKind>()
        });
        Rules.Columns.Add("Vnum", "VNUM");
        Rules.Columns.Add("Name", "Name");
        Rules.Columns.Add("Priority", "Pri");
        foreach (var column in new[] { "Slot", "Key", "Cooldown", "MP", "Min", "Max", "Pri" })
            Skills.Columns.Add(column, column);

        foreach (var page in _pages)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            _host.Controls.Add(page);
        }

        Controls.Add(_host);
        Controls.Add(bar);
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        for (var i = 0; i < _pages.Length; i++)
        {
            _pages[i].Visible = i == index;
            var on = i == index;
            UiTheme.StyleButton(_segments[i], on ? UiTheme.CardInner : UiTheme.Card, on ? UiTheme.AccentSoft : UiTheme.Muted, on);
            _segments[i].Font = UiTheme.Ui(8f, FontStyle.Bold);
        }

        if (_pages[index] is Panel panel)
            panel.AutoScrollPosition = Point.Empty;
    }

    public event EventHandler? SaveClicked
    {
        add => Save.Click += value;
        remove => Save.Click -= value;
    }

    public void LoadFrom(BotConfig config)
    {
        HuntMonsters.Checked = config.HuntMonsters;
        HuntStones.Checked = config.HuntStones;
        LockCenter.Checked = config.LockCenterOnStart;
        AttackPaths.Checked = config.AttackAlsoPaths;
        LootEnabled.Checked = config.LootBeforeNextTarget && config.LootEnabled;
        StopOnCorner.Checked = config.StopOnCorner;
        HealEnabled.Checked = config.HealEnabled;
        MaxFarmDistance.Value = config.MaxFarmDistance;
        FarmRadius.Value = config.FarmRadius;
        AttackRange.Value = config.AttackRange;
        AttackInterval.Value = config.AttackIntervalMs;
        LootRange.Value = config.LootRange;
        FailsafeHp.Value = (float)config.FailsafeHpPercent;
        HealHp.Value = (float)config.HealHpPercent;
        IncludeVnums.Text = string.Join(", ", config.IncludeVnums);
        ExcludeVnums.Text = string.Join(", ", config.ExcludeVnums);
        CenterX.Value = Clamp(config.FarmCenterX, CenterX);
        CenterY.Value = Clamp(config.FarmCenterY, CenterY);
        AttackKey.Text = config.AttackKey;
        PickupKey.Text = config.PickupKey;
        HealKey.Text = config.HealKey;
        Blacklist.Text = string.Join(", ", config.NameBlacklist);
        StartHotkey.Text = config.StartHotkey;
        StopHotkey.Text = config.StopHotkey;
        WindowTitle.Text = string.Join(", ", config.WindowTitleContains);
        PipeName.Text = config.PipeName;
        InputMode.SelectedIndex = config.InputMode switch
        {
            Core.Configuration.InputMode.Ipc => 1,
            Core.Configuration.InputMode.SendInput => 2,
            _ => 0
        };
        Rules.Rows.Clear();
        foreach (var rule in config.TargetRules)
        {
            var index = Rules.Rows.Add();
            Rules.Rows[index].Cells[0].Value = rule.Kind.ToString();
            Rules.Rows[index].Cells[1].Value = rule.Vnum == 0 ? "" : rule.Vnum.ToString();
            Rules.Rows[index].Cells[2].Value = rule.NameContains;
            Rules.Rows[index].Cells[3].Value = rule.Priority.ToString();
        }

        Skills.Rows.Clear();
        foreach (var skill in config.Skills)
        {
            Skills.Rows.Add(
                skill.Slot.ToString(),
                skill.Key,
                skill.CooldownMs.ToString(),
                skill.MpCost.ToString(),
                skill.MinDistance.ToString("0"),
                skill.MaxDistance.ToString("0"),
                skill.Priority.ToString());
        }
    }

    public void SaveTo(BotConfig config)
    {
        config.HuntMonsters = HuntMonsters.Checked;
        config.HuntStones = HuntStones.Checked;
        config.LockCenterOnStart = LockCenter.Checked;
        config.AttackAlsoPaths = AttackPaths.Checked;
        config.LootEnabled = LootEnabled.Checked;
        config.LootBeforeNextTarget = LootEnabled.Checked;
        config.StopOnCorner = StopOnCorner.Checked;
        config.HealEnabled = HealEnabled.Checked;
        config.MaxFarmDistance = MaxFarmDistance.Value;
        config.FarmRadius = FarmRadius.Value;
        config.AttackRange = AttackRange.Value;
        config.AttackIntervalMs = (int)AttackInterval.Value;
        config.LootRange = LootRange.Value;
        config.FailsafeHpPercent = FailsafeHp.Value;
        config.HealHpPercent = HealHp.Value;
        config.IncludeVnums = ParseUints(IncludeVnums.Text);
        config.ExcludeVnums = ParseUints(ExcludeVnums.Text);
        config.FarmCenterX = (float)CenterX.Value;
        config.FarmCenterY = (float)CenterY.Value;
        config.AttackKey = AttackKey.Text.Trim();
        config.PickupKey = PickupKey.Text.Trim();
        config.HealKey = HealKey.Text.Trim();
        config.NameBlacklist = Split(Blacklist.Text);
        config.StartHotkey = StartHotkey.Text.Trim();
        config.StopHotkey = StopHotkey.Text.Trim();
        config.WindowTitleContains = Split(WindowTitle.Text);
        config.PipeName = PipeName.Text.Trim();
        config.InputMode = InputMode.SelectedIndex switch
        {
            1 => Core.Configuration.InputMode.Ipc,
            2 => Core.Configuration.InputMode.SendInput,
            _ => Core.Configuration.InputMode.Auto
        };
        config.TargetRules = ReadRules();
        config.Skills = ReadSkills();
        config.Sanitize();
    }

    public IReadOnlyList<uint> BossVnums() =>
        ReadRules().Where(item => item.Kind == EntityKind.Boss && item.Vnum != 0).Select(item => item.Vnum).ToList();

    private Panel FarmPage() => Stack(
        HuntMonsters, HuntStones, LockCenter,
        FarmRadius, MaxFarmDistance,
        Labeled("Include VNUMs", IncludeVnums),
        Labeled("Exclude VNUMs", ExcludeVnums),
        Labeled("Center X", CenterX),
        Labeled("Center Y", CenterY));

    private Panel CombatPage() => Stack(
        AttackRange, AttackInterval, AttackPaths,
        LootEnabled, LootRange,
        Labeled("Attack key", AttackKey),
        Labeled("Pickup key", PickupKey));

    private Panel SafetyPage() => Stack(
        FailsafeHp, StopOnCorner, HealEnabled, HealHp,
        Labeled("Heal key", HealKey),
        Labeled("Name blacklist", Blacklist),
        Labeled("Start hotkey", StartHotkey),
        Labeled("Panic hotkey", StopHotkey));

    private Panel AdvancedPage()
    {
        var stack = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Card, Padding = new Padding(10, 8, 10, 8) };
        var y = 8;
        Add(stack, Labeled("Window title", WindowTitle), ref y);
        Add(stack, Labeled("Pipe name", PipeName), ref y);
        Add(stack, Labeled("Input", InputMode), ref y);
        var rulesLabel = Caption("Target priority");
        rulesLabel.Location = new Point(0, y);
        stack.Controls.Add(rulesLabel);
        y += 18;
        Rules.Location = new Point(0, y);
        Rules.Size = new Size(380, 88);
        stack.Controls.Add(Rules);
        y += 96;
        var skillsLabel = Caption("Skill rotation");
        skillsLabel.Location = new Point(0, y);
        stack.Controls.Add(skillsLabel);
        y += 18;
        Skills.Location = new Point(0, y);
        Skills.Size = new Size(380, 80);
        stack.Controls.Add(Skills);
        y += 88;
        Save.Location = new Point(0, y);
        stack.Controls.Add(Save);
        return stack;
    }

    private List<TargetRule> ReadRules()
    {
        var rules = new List<TargetRule>();
        foreach (DataGridViewRow row in Rules.Rows)
        {
            if (row.IsNewRow)
                continue;
            if (!Enum.TryParse<EntityKind>(Convert.ToString(row.Cells[0].Value), true, out var kind))
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
        foreach (DataGridViewRow row in Skills.Rows)
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

    private static Panel Stack(params Control[] controls)
    {
        var host = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Card, Padding = new Padding(10, 8, 10, 8) };
        var y = 8;
        foreach (var control in controls)
            Add(host, control, ref y);
        host.AutoScrollPosition = Point.Empty;
        return host;
    }

    private static void Add(Control host, Control child, ref int y)
    {
        child.Width = Math.Max(200, host.ClientSize.Width - 24);
        if (child.Height < 22)
            child.Height = child is ValueSlider ? 44 : 24;
        child.Location = new Point(8, y);
        child.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        host.Controls.Add(child);
        y += child.Height + 8;
    }

    private static Control Labeled(string caption, Control editor)
    {
        var row = new Panel { Height = 40, BackColor = UiTheme.Card };
        var label = Caption(caption);
        label.Location = new Point(0, 0);
        editor.Location = new Point(0, 16);
        editor.Width = 360;
        editor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        row.Controls.Add(label);
        row.Controls.Add(editor);
        return row;
    }

    private static Label Caption(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            ForeColor = UiTheme.Muted,
            BackColor = UiTheme.Card,
            Font = UiTheme.Ui(8f)
        };

    private static CheckBox Switch(string text)
    {
        var box = new CheckBox
        {
            Text = text,
            AutoSize = false,
            Height = 22,
            ForeColor = UiTheme.Ink,
            BackColor = UiTheme.Card,
            Font = UiTheme.Ui(9f)
        };
        return box;
    }

    private static ValueSlider Slider(string caption, float min, float max, string suffix = "")
    {
        return new ValueSlider
        {
            Caption = caption,
            Minimum = min,
            Maximum = max,
            Suffix = suffix,
            Width = 360
        };
    }

    private static TextBox Field()
    {
        var box = new TextBox { Width = 360, Height = 22 };
        UiTheme.StyleField(box);
        return box;
    }

    private static ComboBox Combo(params string[] items)
    {
        var box = new ComboBox { Width = 360 };
        box.Items.AddRange(items);
        UiTheme.StyleField(box);
        return box;
    }

    private static NumericUpDown Number(decimal min, decimal max, int decimals)
    {
        var input = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            ThousandsSeparator = true,
            Width = 360
        };
        UiTheme.StyleField(input);
        return input;
    }

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

    private static int ParseInt(object? value, int fallback) =>
        int.TryParse(Convert.ToString(value), out var parsed) ? parsed : fallback;

    private static decimal Clamp(double value, NumericUpDown input)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return input.Minimum;
        try
        {
            var converted = (decimal)value;
            if (converted < input.Minimum)
                return input.Minimum;
            if (converted > input.Maximum)
                return input.Maximum;
            return converted;
        }
        catch (OverflowException)
        {
            return value > 0 ? input.Maximum : input.Minimum;
        }
    }
}
