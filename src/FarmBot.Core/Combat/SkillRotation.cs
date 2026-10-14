using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public sealed class SkillRotation
{
    private readonly Dictionary<int, long> _lastUsed = new();

    public void Reset() => _lastUsed.Clear();

    public SkillSetting? Select(PlayerState player, EntityState target, IReadOnlyList<SkillSetting> skills, long nowMs)
    {
        SkillSetting? chosen = null;
        foreach (var skill in skills.OrderByDescending(skill => skill.Priority))
        {
            if (string.IsNullOrWhiteSpace(skill.Key))
                continue;
            if (player.Mp < skill.MpCost)
                continue;
            if (target.Distance < skill.MinDistance || target.Distance > skill.MaxDistance)
                continue;
            if (_lastUsed.TryGetValue(skill.Slot, out var last) && nowMs - last < skill.CooldownMs)
                continue;
            chosen = skill;
            break;
        }

        return chosen;
    }

    public void Mark(int slot, long nowMs) => _lastUsed[slot] = nowMs;
}
