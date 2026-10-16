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
    /// <summary>From client skill.IsToggleSkill — first click removes, second re-applies.</summary>
    public bool IsToggle { get; set; }
    /// <summary>From client IsSkillActive — toggle highlight currently on.</summary>
    public bool IsActive { get; set; }

    public bool IsBuff =>
        string.Equals(Kind, "buff", StringComparison.OrdinalIgnoreCase);
}

public sealed class SkillScan
{
    public IReadOnlyList<LearnedSkill> Skills { get; set; } = Array.Empty<LearnedSkill>();
}
