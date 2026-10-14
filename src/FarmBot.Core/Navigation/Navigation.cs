using FarmBot.Core.Configuration;
using FarmBot.Core.Geometry;
using FarmBot.Core.Models;

namespace FarmBot.Core.Navigation;

public readonly record struct FarmArea(float CenterX, float CenterY, float Radius)
{
    public bool Contains(float x, float y)
    {
        if (Radius <= 0)
            return true;
        var dx = x - CenterX;
        var dy = y - CenterY;
        return dx * dx + dy * dy <= Radius * Radius;
    }

    public static FarmArea FromConfig(BotConfig config) => new(config.FarmCenterX, config.FarmCenterY, config.FarmRadius);
}

public static class Navigator
{
    public static bool ShouldMove(float distance, BotConfig config) =>
        !config.AttackAlsoPaths && distance > config.AttackRange;

    public static bool OutsideLeash(PlayerState player, BotConfig config)
    {
        if (!config.ReturnToCenter || config.FarmRadius <= 0)
            return false;
        var distance = WorldGeometry.Distance(player.X, player.Y, config.FarmCenterX, config.FarmCenterY);
        return distance > config.LeashSlack;
    }
}
