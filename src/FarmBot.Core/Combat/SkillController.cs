using FarmBot.Core.Configuration;

namespace FarmBot.Core.Combat;

public sealed class SkillController
{
    public const int SpacingMs = 300;

    private readonly Dictionary<uint, long> _lastUseMs = new();

    public LearnedSkill? Peek(
        IReadOnlyList<LearnedSkill> skills,
        BotConfig config,
        bool hasTarget,
        long nowMs) =>
        Pick(skills, config, hasTarget, nowMs, buffsOnly: false);

    public IReadOnlyList<LearnedSkill> PeekDueBuffs(
        IReadOnlyList<LearnedSkill> skills,
        BotConfig config,
        long nowMs)
    {
        if ((config.ClientProfile != ClientProfile.Arthion && config.ClientProfile != ClientProfile.Baerim) ||
            skills == null || skills.Count == 0)
            return Array.Empty<LearnedSkill>();

        var enabled = config.SkillEnabledIndexes ?? new List<uint>();
        if (enabled.Count == 0)
            return Array.Empty<LearnedSkill>();
        var delays = config.SkillDelaySec ?? new Dictionary<uint, int>();
        var buffs = config.SkillBuffIndexes ?? new List<uint>();
        if (buffs.Count == 0)
            return Array.Empty<LearnedSkill>();

        var due = new List<LearnedSkill>();
        var dueIndexes = new HashSet<uint>();
        var delayGroups = new HashSet<int>();
        foreach (var skill in skills)
        {
            if (skill.Index == 0 || !enabled.Contains(skill.Index) || !buffs.Contains(skill.Index))
                continue;
            if (!DelayOk(skill.Index, delays, nowMs))
                continue;
            due.Add(skill);
            dueIndexes.Add(skill.Index);
            delayGroups.Add(DelaySecOf(skill.Index, delays));
        }

        if (due.Count == 0)
            return due;

        // Same configured delay → one ground session (dismount → all → remount),
        // even if last-use stamps drifted a bit apart.
        foreach (var skill in skills)
        {
            if (skill.Index == 0 || dueIndexes.Contains(skill.Index))
                continue;
            if (!enabled.Contains(skill.Index) || !buffs.Contains(skill.Index))
                continue;
            if (!delayGroups.Contains(DelaySecOf(skill.Index, delays)))
                continue;
            due.Add(skill);
            dueIndexes.Add(skill.Index);
        }

        return due;
    }

    private static int DelaySecOf(uint index, Dictionary<uint, int> delays) =>
        delays.TryGetValue(index, out var sec) && sec > 0 ? sec : 0;

    public LearnedSkill? Next(
        IReadOnlyList<LearnedSkill> skills,
        BotConfig config,
        bool hasTarget,
        long nowMs)
    {
        var pick = Pick(skills, config, hasTarget, nowMs, buffsOnly: false);
        return pick == null ? null : Use(pick, nowMs);
    }

    public void NoteUsed(LearnedSkill skill, long nowMs) => Use(skill, nowMs);

    private LearnedSkill? Pick(
        IReadOnlyList<LearnedSkill> skills,
        BotConfig config,
        bool hasTarget,
        long nowMs,
        bool buffsOnly)
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
            if (buffsOnly && !buff)
                continue;
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

        return pick;
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
