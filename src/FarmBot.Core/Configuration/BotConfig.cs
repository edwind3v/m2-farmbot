using System.Text.Json;
using FarmBot.Core.Json;
using FarmBot.Core.Models;

namespace FarmBot.Core.Configuration;

public enum InputMode
{
    Auto,
    Ipc,
    SendInput
}

public sealed class TargetRule
{
    public EntityKind Kind { get; set; } = EntityKind.Unknown;
    public uint Vnum { get; set; }
    public string NameContains { get; set; } = "";
    public int Priority { get; set; }
}

public sealed class SkillSetting
{
    public int Slot { get; set; } = 1;
    public string Key { get; set; } = "1";
    public int CooldownMs { get; set; } = 5000;
    public int MpCost { get; set; } = 30;
    public float MinDistance { get; set; }
    public float MaxDistance { get; set; } = 2000;
    public int Priority { get; set; } = 1;
}

public sealed class BotConfig
{
    public List<string> WindowTitleContains { get; set; } = new() { "METIN2", "Metin2" };
    public string PipeName { get; set; } = "M2FarmBot";
    public string ServerHost { get; set; } = "192.168.1.100";
    public int ServerPort { get; set; } = 30003;
    public int ListenPort { get; set; } = 13003;
    public int TickMs { get; set; } = 100;
    public float AttackRange { get; set; } = 300;
    public int AttackIntervalMs { get; set; } = 450;
    public bool RangedAttackEnabled { get; set; }
    public float RangedAttackRadius { get; set; } = 2000;
    public int RangedAttackMaxTargets { get; set; } = 3;
    public int RangedAttackDelayMs { get; set; } = 400;
    public bool RangedAvoidAttacks { get; set; }
    public int TargetTimeoutMs { get; set; } = 20_000;
    public int StuckTimeoutMs { get; set; } = 350;
    public bool AttackAlsoPaths { get; set; } = true;
    public string AttackKey { get; set; } = "space";
    public float FarmRadius { get; set; } = 1500;
    public float FarmCenterX { get; set; }
    public float FarmCenterY { get; set; }
    public bool LockCenterOnStart { get; set; } = true;
    public bool LootEnabled { get; set; } = true;
    public float LootRange { get; set; } = 600;
    public bool LootBeforeNextTarget { get; set; } = true;
    public string PickupKey { get; set; } = "z";
    public int PickupIntervalMs { get; set; } = 200;
    public bool RangedPickupEnabled { get; set; }
    public bool FastPickupEnabled { get; set; }
    public int FastPickupDelayMs { get; set; } = 50;
    public bool ReturnToCenter { get; set; } = true;
    public float LeashSlack { get; set; } = 80;
    public int MoveIntervalMs { get; set; } = 500;
    public double FailsafeHpPercent { get; set; } = 15;
    public bool StopOnCorner { get; set; } = true;
    public int StateTimeoutMs { get; set; } = 3000;
    public bool HealEnabled { get; set; } = true;
    public double HealHpPercent { get; set; } = 40;
    public string HealKey { get; set; } = "F1";
    public int HealCooldownMs { get; set; } = 1500;
    public InputMode InputMode { get; set; } = InputMode.Auto;
    public bool IgnorePlayers { get; set; } = true;
    public bool HuntMonsters { get; set; } = true;
    public bool HuntStones { get; set; } = true;
    public float MaxFarmDistance { get; set; } = 8000;
    public float MapZoom { get; set; } = 100;
    public List<uint> IncludeVnums { get; set; } = new();
    public List<uint> ExcludeVnums { get; set; } = new();
    public List<string> NameBlacklist { get; set; } = new();
    public List<TargetRule> TargetRules { get; set; } = new();
    public List<SkillSetting> Skills { get; set; } = new();
    public string StartHotkey { get; set; } = "F9";
    public string StopHotkey { get; set; } = "F10";
    public string ClientPath { get; set; } = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";
    public ClientProfile ClientProfile { get; set; } = ClientProfile.Normal;
    public double PotionHpPercent { get; set; } = 75;
    public double PotionMpPercent { get; set; } = 75;
    public List<uint> PotionEnabledVnums { get; set; } = new();
    public List<uint> SkillEnabledIndexes { get; set; } = new();
    public List<uint> SkillBuffIndexes { get; set; } = new();
    public Dictionary<uint, int> SkillDelaySec { get; set; } = new();
    public bool SkillsDuringCombat { get; set; }

    public static BotConfig CreateDefault()
    {
        return new BotConfig
        {
            TargetRules = new List<TargetRule>
            {
                new() { Kind = EntityKind.Metin, Priority = 100 },
                new() { Kind = EntityKind.Boss, Priority = 80 },
                new() { Kind = EntityKind.Monster, Priority = 10 }
            },
            Skills = new List<SkillSetting>
            {
                new()
                {
                    Slot = 1,
                    Key = "1",
                    CooldownMs = 5000,
                    MpCost = 30,
                    MinDistance = 0,
                    MaxDistance = 2000,
                    Priority = 1
                }
            }
        };
    }

    public void Sanitize()
    {
        WindowTitleContains ??= new List<string> { "METIN2", "Metin2" };
        WindowTitleContains.RemoveAll(string.IsNullOrWhiteSpace);
        if (WindowTitleContains.Count == 0)
            WindowTitleContains.Add("METIN2");

        if (string.IsNullOrWhiteSpace(PipeName))
            PipeName = "M2FarmBot";
        PipeName = NormalizePipeName(PipeName);
        if (string.IsNullOrWhiteSpace(ServerHost))
            ServerHost = "192.168.1.100";
        ServerHost = ServerHost.Trim();
        ServerPort = Math.Clamp(ServerPort, 1, 65535);
        ListenPort = Math.Clamp(ListenPort, 1, 65535);

        TickMs = Math.Clamp(TickMs, 20, 2000);
        AttackRange = Math.Max(1, AttackRange);
        AttackIntervalMs = Math.Clamp(AttackIntervalMs, 50, 60_000);
        RangedAttackRadius = Math.Max(0, RangedAttackRadius);
        if (RangedAttackMaxTargets < 1)
            RangedAttackMaxTargets = 1;
        if (RangedAttackDelayMs < 1)
            RangedAttackDelayMs = 1;
        TargetTimeoutMs = Math.Clamp(TargetTimeoutMs, 500, 300_000);
        StuckTimeoutMs = Math.Clamp(StuckTimeoutMs, 200, 60_000);
        PickupIntervalMs = Math.Clamp(PickupIntervalMs, 50, 60_000);
        if (FastPickupDelayMs < 1)
            FastPickupDelayMs = 1;
        MoveIntervalMs = Math.Clamp(MoveIntervalMs, 50, 60_000);
        HealCooldownMs = Math.Clamp(HealCooldownMs, 50, 60_000);
        StateTimeoutMs = Math.Clamp(StateTimeoutMs, 200, 120_000);
        FailsafeHpPercent = Math.Clamp(FailsafeHpPercent, 0, 100);
        HealHpPercent = Math.Clamp(HealHpPercent, 0, 100);
        PotionHpPercent = Math.Clamp(PotionHpPercent, 0, 100);
        PotionMpPercent = Math.Clamp(PotionMpPercent, 0, 100);
        PotionEnabledVnums ??= new List<uint>();
        SkillEnabledIndexes ??= new List<uint>();
        SkillBuffIndexes ??= new List<uint>();
        SkillDelaySec ??= new Dictionary<uint, int>();
        foreach (var key in SkillDelaySec.Keys.ToList())
            SkillDelaySec[key] = Math.Clamp(SkillDelaySec[key], 0, 600);
        FarmRadius = Math.Max(0, FarmRadius);
        MaxFarmDistance = Math.Max(0, MaxFarmDistance);
        MapZoom = Math.Clamp(MapZoom <= 0 ? 100 : MapZoom, 20, 100);
        IncludeVnums ??= new List<uint>();
        ExcludeVnums ??= new List<uint>();
        LootRange = Math.Max(0, LootRange);
        LeashSlack = Math.Max(0, LeashSlack);
        NameBlacklist ??= new List<string>();
        TargetRules ??= new List<TargetRule>();
        Skills ??= new List<SkillSetting>();
        AttackKey ??= "";
        PickupKey ??= "";
        HealKey ??= "";
        StartHotkey = string.IsNullOrWhiteSpace(StartHotkey) ? "F9" : StartHotkey.Trim();
        StopHotkey = string.IsNullOrWhiteSpace(StopHotkey) ? "F10" : StopHotkey.Trim();
        ClientPath ??= "";
    }

    public BotConfig DeepCopy()
    {
        var json = JsonSerializer.Serialize(this, JsonDefaults.Instance);
        var copy = JsonSerializer.Deserialize<BotConfig>(json, JsonDefaults.Instance) ?? CreateDefault();
        copy.Sanitize();
        return copy;
    }

    public static string NormalizePipeName(string name)
    {
        name = name.Trim().Replace('/', '\\');
        const string prefix = @"\\.\pipe\";
        if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            name = name[prefix.Length..];
        return string.IsNullOrWhiteSpace(name) ? "M2FarmBot" : name;
    }
}

public static class ConfigStore
{
    public static string DefaultPath(string baseDirectory) => Path.Combine(baseDirectory, "config.json");

    public static BotConfig LoadOrCreate(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<BotConfig>(json, JsonDefaults.Instance);
                if (loaded != null)
                {
                    loaded.Sanitize();
                    return loaded;
                }
            }
        }
        catch
        {
            // A damaged config is replaced with defaults below.
        }

        var created = BotConfig.CreateDefault();
        created.Sanitize();
        Save(path, created);
        return created;
    }

    public static void Save(string path, BotConfig config)
    {
        config.Sanitize();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(config, JsonDefaults.Instance));
    }
}
