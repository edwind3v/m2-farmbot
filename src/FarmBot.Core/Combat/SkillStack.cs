namespace FarmBot.Core.Combat;

public sealed class LearnedSkill
{
    public uint Index { get; set; }
    public int Slot { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "attack";
    public int Level { get; set; }
    public int CoolRemain { get; set; }
    public int AffectRemain { get; set; }
    public int DurationSec { get; set; }
    public string Icon { get; set; } = "";

    public bool IsBuff =>
        string.Equals(Kind, "buff", StringComparison.OrdinalIgnoreCase);
}

public sealed class SkillScan
{
    public IReadOnlyList<LearnedSkill> Skills { get; set; } = Array.Empty<LearnedSkill>();
}
