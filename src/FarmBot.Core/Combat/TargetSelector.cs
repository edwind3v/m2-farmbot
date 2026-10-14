using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public static class TargetSelector
{
    public static EntityState? Select(WorldSnapshot snapshot, BotConfig config, uint stickyVid)
    {
        var filter = FarmFilter.FromConfig(config);
        var main = EntityMapping.FromPlayer(snapshot.Player);
        var entities = new List<Entity>();
        foreach (var state in snapshot.Entities)
        {
            if (IsBlacklisted(state, config))
                continue;
            if (state.Kind == EntityKind.Player && config.IgnorePlayers)
                continue;
            entities.Add(EntityMapping.FromState(state));
        }

        var picked = EntityManager.SelectTarget(entities, main, filter, stickyVid);
        if (picked == null)
            return null;
        return snapshot.Entities.FirstOrDefault(item => item.Vid == picked.Vid);
    }

    public static IReadOnlyList<EntityState> SelectMany(
        WorldSnapshot snapshot,
        BotConfig config,
        int maxCount,
        float maxPlayerDistance = 0,
        bool followPlayer = false)
    {
        var filter = FarmFilter.FromConfig(config);
        if (followPlayer)
            filter = filter.Around(snapshot.Player.X, snapshot.Player.Y);
        var main = EntityMapping.FromPlayer(snapshot.Player);
        var entities = new List<Entity>();
        foreach (var state in snapshot.Entities)
        {
            if (IsBlacklisted(state, config))
                continue;
            if (state.Kind == EntityKind.Player && config.IgnorePlayers)
                continue;
            entities.Add(EntityMapping.FromState(state));
        }

        var picked = EntityManager.SelectTargets(entities, main, filter, maxCount, maxPlayerDistance);
        if (picked.Count == 0)
            return Array.Empty<EntityState>();

        var byVid = snapshot.Entities.ToDictionary(item => item.Vid);
        var result = new List<EntityState>(picked.Count);
        foreach (var entity in picked)
        {
            if (byVid.TryGetValue(entity.Vid, out var state))
                result.Add(state);
        }

        return result;
    }

    public static int? MatchPriority(EntityState entity, IReadOnlyList<TargetRule> rules)
    {
        int? best = null;
        foreach (var rule in rules)
        {
            if (rule.Kind != EntityKind.Unknown && rule.Kind != entity.Kind)
                continue;
            if (!string.IsNullOrWhiteSpace(rule.NameContains) &&
                entity.Name.IndexOf(rule.NameContains, StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            if (best == null || rule.Priority > best)
                best = rule.Priority;
        }

        return best;
    }

    private static bool IsBlacklisted(EntityState entity, BotConfig config)
    {
        foreach (var banned in config.NameBlacklist)
        {
            if (!string.IsNullOrWhiteSpace(banned) &&
                entity.Name.Contains(banned, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
