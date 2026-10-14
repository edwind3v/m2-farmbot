using FarmBot.Core.Geometry;
using FarmBot.Core.Models;

namespace FarmBot.Core.Entities;

public static class EntityManager
{
    public static Entity WithDistance(Entity entity, Entity? main)
    {
        if (main == null)
            return entity;
        return entity with
        {
            Distance = WorldGeometry.Distance(main.X, main.Y, main.Z, entity.X, entity.Y, entity.Z)
        };
    }

    public static IReadOnlyList<Entity> WithDistances(IEnumerable<Entity> entities, Entity? main)
    {
        return entities.Select(entity => WithDistance(entity, main)).ToList();
    }

    public static IReadOnlyList<Entity> NearbyMonsters(IEnumerable<Entity> entities, Entity? main, FarmFilter filter) =>
        Filter(entities, main, filter, ActorType.Enemy);

    public static IReadOnlyList<Entity> NearbyStones(IEnumerable<Entity> entities, Entity? main, FarmFilter filter) =>
        Filter(entities, main, filter, ActorType.Stone);

    public static IReadOnlyList<Entity> Filter(IEnumerable<Entity> entities, Entity? main, FarmFilter filter, ActorType? requiredType = null)
    {
        var result = new List<Entity>();
        foreach (var raw in entities)
        {
            var entity = WithDistance(raw, main);
            if (!IsEligible(entity, filter, requiredType))
                continue;
            result.Add(entity);
        }

        return result.OrderBy(item => item.Distance).ToList();
    }

    public static Entity? SelectTarget(IEnumerable<Entity> entities, Entity? main, FarmFilter filter, uint stickyVid = 0)
    {
        var eligible = new List<(Entity Entity, int Priority)>();
        foreach (var raw in entities)
        {
            var entity = WithDistance(raw, main);
            if (!IsEligible(entity, filter, requiredType: null))
                continue;
            var priority = PriorityOf(entity, filter);
            if (priority == null)
                continue;
            eligible.Add((entity, priority.Value));
        }

        if (eligible.Count == 0)
            return null;

        var best = eligible
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Entity.Distance)
            .ThenBy(item => item.Entity.Vid)
            .First();

        if (stickyVid != 0)
        {
            foreach (var item in eligible)
            {
                if (item.Entity.Vid == stickyVid && item.Priority >= best.Priority)
                    return item.Entity;
            }
        }

        return best.Entity;
    }

    public static IReadOnlyList<Entity> SelectTargets(
        IEnumerable<Entity> entities,
        Entity? main,
        FarmFilter filter,
        int maxCount,
        float maxPlayerDistance = 0)
    {
        if (maxCount <= 0)
            return Array.Empty<Entity>();

        var eligible = new List<(Entity Entity, int Priority)>();
        foreach (var raw in entities)
        {
            var entity = WithDistance(raw, main);
            if (!IsEligible(entity, filter, requiredType: null))
                continue;
            if (maxPlayerDistance > 0 && entity.Distance > maxPlayerDistance)
                continue;
            var priority = PriorityOf(entity, filter);
            if (priority == null)
                continue;
            eligible.Add((entity, priority.Value));
        }

        return eligible
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.Entity.Distance)
            .ThenBy(item => item.Entity.Vid)
            .Take(maxCount)
            .Select(item => item.Entity)
            .ToList();
    }

    public static int? PriorityOf(Entity entity, FarmFilter filter)
    {
        int? best = null;
        foreach (var rule in filter.Rules)
        {
            if (rule.Vnum != 0 && rule.Vnum != entity.Vnum)
                continue;
            if (rule.Kind != EntityKind.Unknown && rule.Kind != EntityMapping.ToKind(entity.Type))
                continue;
            if (!string.IsNullOrWhiteSpace(rule.NameContains) &&
                entity.Name.IndexOf(rule.NameContains, StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            if (best == null || rule.Priority > best)
                best = rule.Priority;
        }

        if (best != null)
            return best;
        if (entity.IsStone && filter.HuntStones)
            return filter.DefaultStonePriority;
        if (entity.IsMonster && filter.HuntMonsters)
            return filter.DefaultMonsterPriority;
        return null;
    }

    public static bool IsEligible(Entity entity, FarmFilter filter, ActorType? requiredType)
    {
        if (entity.Hp <= 0)
            return false;
        if (requiredType != null && entity.Type != requiredType)
            return false;
        if (entity.IsMonster && !filter.HuntMonsters)
            return false;
        if (entity.IsStone && !filter.HuntStones)
            return false;
        if (!entity.IsFarmable)
            return false;
        if (filter.ExcludeVnums.Contains(entity.Vnum))
            return false;
        if (filter.IncludeVnums.Count > 0 && !filter.IncludeVnums.Contains(entity.Vnum))
            return false;
        if (filter.MaxDistance > 0 && entity.Distance > filter.MaxDistance)
            return false;
        if (filter.FarmRadius > 0)
        {
            var dx = entity.X - filter.FarmCenterX;
            var dy = entity.Y - filter.FarmCenterY;
            if (dx * dx + dy * dy > filter.FarmRadius * filter.FarmRadius)
                return false;
        }

        return true;
    }
}
