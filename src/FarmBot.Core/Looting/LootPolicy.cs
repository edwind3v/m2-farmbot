using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Looting;

public static class LootPolicy
{
    public static float PickupRadius(BotConfig config)
    {
        if (config.RangedPickupEnabled)
            return Math.Max(config.RangedAttackRadius, config.LootRange);
        if (config.FastPickupEnabled)
            return Math.Max(config.LootRange, CombatNativeContract.PickDistance);
        return CombatNativeContract.PickDistance;
    }

    public const uint FastLoop = 1;
    public const uint RangedLoop = 2;

    public static bool SpoofPickup(BotConfig config) => config.RangedPickupEnabled;

    public static bool WantsPickup(BotConfig config) =>
        config.LootEnabled || config.RangedPickupEnabled || config.FastPickupEnabled;

    public static uint LoopMask(BotConfig config, bool botEnabled, bool rangedOnly)
    {
        uint mask = 0;
        if (config.FastPickupEnabled)
            mask |= FastLoop;
        if (config.RangedPickupEnabled)
            mask |= RangedLoop;
        if (botEnabled && config.LootEnabled && !rangedOnly && (mask & FastLoop) == 0)
            mask |= FastLoop;
        return mask;
    }

    public static bool InPickupRange(float distance) =>
        distance <= CombatNativeContract.PickDistance;

    public static EntityState? NextDrop(WorldSnapshot snapshot, BotConfig config)
    {
        if (!WantsPickup(config))
            return null;

        var radius = PickupRadius(config);
        EntityState? nearest = null;
        foreach (var entity in snapshot.Entities)
        {
            if (entity.Kind != EntityKind.Drop || entity.Distance > radius)
                continue;
            if (nearest == null || entity.Distance < nearest.Distance)
                nearest = entity;
        }

        return nearest;
    }
}
