using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

[Serializable]
public sealed class AdvancedState
{
    public string TargetPriority { get; set; } = "Taş > Canavar";
    public string TargetWhitelist { get; set; } = "";
    public string TargetBlacklist { get; set; } = "";
    public string PatrolMode { get; set; } = "Sıralı";
    public string SkillMode { get; set; } = "Normal";
    public bool OverlayEnabled { get; set; }
    public List<FarmZone> Zones { get; set; } = new List<FarmZone>();
    public List<AutomationRule> Rules { get; set; } = new List<AutomationRule>();
    public List<InventoryRule> InventoryRules { get; set; } = new List<InventoryRule>();
    public List<uint> NormalSkillOrder { get; set; } = new List<uint>();
    public List<uint> BossSkillOrder { get; set; } = new List<uint>();
    public FarmPoint StartPoint { get; set; }
    public FarmPoint ReturnPoint { get; set; }
    public AdvancedStats Stats { get; set; } = new AdvancedStats();
    public string PackRoot { get; set; } = "";

    public void Normalize()
    {
        if (Zones == null) Zones = new List<FarmZone>();
        if (Rules == null) Rules = new List<AutomationRule>();
        if (InventoryRules == null) InventoryRules = new List<InventoryRule>();
        if (NormalSkillOrder == null) NormalSkillOrder = new List<uint>();
        if (BossSkillOrder == null) BossSkillOrder = new List<uint>();
        if (Stats == null) Stats = new AdvancedStats();
        if (string.IsNullOrWhiteSpace(TargetPriority)) TargetPriority = "Taş > Canavar";
        if (string.IsNullOrWhiteSpace(PatrolMode)) PatrolMode = "Sıralı";
        if (string.IsNullOrWhiteSpace(SkillMode)) SkillMode = "Normal";
    }
}

[Serializable]
public sealed class FarmZone
{
    public string Name { get; set; } = "Alan";
    public string MapName { get; set; } = "";
    public float MapX { get; set; }
    public float MapY { get; set; }
    public float RadiusMap { get; set; }
    public float MemoryX { get; set; }
    public float MemoryY { get; set; }
    public float RadiusMemory { get; set; }
    public bool Exclusion { get; set; }
    public bool Enabled { get; set; } = true;
    public override string ToString() { return (Enabled ? "● " : "○ ") + (Exclusion ? "YASAK · " : "GEZİ · ") + Name + " · " + (int)RadiusMemory; }
}

[Serializable]
public sealed class FarmPoint
{
    public string MapName { get; set; } = "";
    public float X { get; set; }
    public float Y { get; set; }
    public override string ToString() { return MapName + " · " + (int)X + ", " + (int)Y; }
}

[Serializable]
public sealed class AutomationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public bool Enabled { get; set; } = true;
    public string Condition { get; set; } = "Hedef yok (sn)";
    public string Value { get; set; } = "10";
    public string Action { get; set; } = "Uyarı yaz";
    public string Argument { get; set; } = "";
    public int CooldownSeconds { get; set; } = 10;
    [XmlIgnore] public DateTime LastRunUtc { get; set; } = DateTime.MinValue;
    public override string ToString() { return (Enabled ? "● " : "○ ") + Condition + " " + Value + " → " + Action + (string.IsNullOrWhiteSpace(Argument) ? "" : " · " + Argument); }
}

[Serializable]
public sealed class InventoryRule
{
    public bool Enabled { get; set; } = true;
    public string Match { get; set; } = "";
    public string Action { get; set; } = "Koru";
    public int Threshold { get; set; } = 1;
    public int IntervalSeconds { get; set; } = 5;
    public override string ToString() { return (Enabled ? "● " : "○ ") + Match + " → " + Action + " · " + Threshold; }
}

[Serializable]
public sealed class AdvancedStats
{
    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    public int Targets { get; set; }
    public int Stones { get; set; }
    public int Monsters { get; set; }
    public long ItemsGained { get; set; }
    public long ItemsUsed { get; set; }
    public int Recoveries { get; set; }
    public int Reconnects { get; set; }
    public double TotalKillMilliseconds { get; set; }
    public int TimedKills { get; set; }
}

[Serializable]
public sealed class IntervalEntry
{
    public uint Id { get; set; }
    public int Seconds { get; set; }
}

[Serializable]
public sealed class RuntimeControlSetting
{
    public string Name { get; set; }
    public string Value { get; set; }
}

[Serializable]
public sealed class AutomationSelectionProfile
{
    public List<IntervalEntry> Skills { get; set; } = new List<IntervalEntry>();
    public List<IntervalEntry> Items { get; set; } = new List<IntervalEntry>();
    public List<RuntimeControlSetting> Controls { get; set; } = new List<RuntimeControlSetting>();
    public bool TurboEnabled { get; set; }
    public bool AreaSelected { get; set; }
    public string AreaMap { get; set; } = "";
    public float AreaX { get; set; }
    public float AreaY { get; set; }
    public float AreaRadius { get; set; }
    public bool SessionEnabled { get; set; }
    public bool AutoLogin { get; set; }
    public bool AutoEnter { get; set; }
    public bool AutoRevive { get; set; }
    public bool AutoCloak { get; set; }
    public bool RestartGame { get; set; } = true;
    public bool AutoCloseShaman { get; set; } = true;
    public bool SkillMountCycle { get; set; } = true;
    public int CloakSeconds { get; set; } = 10;
    public int SkillSeconds { get; set; } = 2;
    public int ItemSeconds { get; set; } = 300;
}

[Serializable]
public sealed class AdvancedProfile
{
    public string Name { get; set; } = "Profil";
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public AdvancedState State { get; set; } = new AdvancedState();
    public List<IntervalEntry> Skills { get; set; } = new List<IntervalEntry>();
    public List<IntervalEntry> Items { get; set; } = new List<IntervalEntry>();
    public bool WalkStones { get; set; } = true;
    public bool WalkMonsters { get; set; } = true;
    public bool AttackStones { get; set; } = true;
    public bool AttackMonsters { get; set; } = true;
    public bool SearchWhenEmpty { get; set; } = true;
    public decimal SearchRange { get; set; } = 20000;
    public decimal ApproachDistance { get; set; } = 100;
    public bool ApproachOnly { get; set; } = true;
    public TaskSequence Task { get; set; } = new TaskSequence();
}

public static class AdvancedStore
{
    public static void Save<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temporary = path + ".tmp";
        using (var stream = File.Create(temporary)) new XmlSerializer(typeof(T)).Serialize(stream, value);
        if (File.Exists(path)) File.Replace(temporary, path, path + ".bak", true);
        else File.Move(temporary, path);
    }
    public static T Load<T>(string path) where T : class
    {
        using (var stream = File.OpenRead(path)) return new XmlSerializer(typeof(T)).Deserialize(stream) as T;
    }
}
