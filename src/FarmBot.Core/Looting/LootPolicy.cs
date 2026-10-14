using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Looting;

public static class LootPolicy
{
    public static EntityState? NextDrop(WorldSnapshot snapshot, BotConfig config)
    {
        if (!config.LootEnabled)
            return null;

        EntityState? nearest = null;
        foreach (var entity in snapshot.Entities)
        {
            if (entity.Kind != EntityKind.Drop || entity.Distance > config.LootRange)
                continue;
            if (nearest == null || entity.Distance < nearest.Distance)
                nearest = entity;
        }

        return nearest;
    }
}
