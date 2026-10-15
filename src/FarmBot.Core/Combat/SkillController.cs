using FarmBot.Core.Configuration;

namespace FarmBot.Core.Combat;

public sealed class SkillController
{
    public const int SpacingMs = 300;

    private readonly Dictionary<uint, long> _lastUseMs = new();

    public LearnedSkill? Next(
        IReadOnlyList<LearnedSkill> skills,
        BotConfig config,
        bool hasTarget,
        long nowMs)
    {
        if ((config.ClientProfile != ClientProfile.Arthion && config.ClientProfile != ClientProfile.Baerim) ||
            skills == null || skills.Count == 0)
            return null;

        var enabled = config.SkillEnabledIndexes ?? new List<uint>();
        if (enabled.Count == 0)
            return null;
        var delays = config.SkillDelaySec ?? new Dictionary<uint, int>();
        var buffs = config.SkillBuffIndexes ?? new List<uint>();

        LearnedSkill? pick = null;
        var oldest = long.MaxValue;
        foreach (var skill in skills)
        {
            if (skill.Index == 0 || !enabled.Contains(skill.Index))
                continue;
            var buff = buffs.Contains(skill.Index);
            if (!buff && !hasTarget)
                continue;
            if (!DelayOk(skill.Index, delays, nowMs))
                continue;
            if (!_lastUseMs.TryGetValue(skill.Index, out var last))
                last = 0;
            if (pick != null && last >= oldest)
                continue;
            pick = skill;
            oldest = last;
        }

        return pick == null ? null : Use(pick, nowMs);
    }

    public void NoteAffectRemain(IReadOnlyList<LearnedSkill> skills, long nowMs)
    {
    }

    private bool DelayOk(uint index, Dictionary<uint, int> delays, long nowMs)
    {
        if (!_lastUseMs.TryGetValue(index, out var last) || last <= 0)
            return true;
        if (!delays.TryGetValue(index, out var sec) || sec <= 0)
            return nowMs - last >= SpacingMs;
        return nowMs - last >= sec * 1000L;
    }

    private LearnedSkill Use(LearnedSkill skill, long nowMs)
    {
        _lastUseMs[skill.Index] = nowMs;
        return skill;
    }
}
