using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

internal static class Fixtures
{
    public static BotConfig Config()
    {
        var config = BotConfig.CreateDefault();
        config.HealEnabled = false;
        config.LockCenterOnStart = false;
        config.FarmCenterX = 0;
        config.FarmCenterY = 0;
        config.FarmRadius = 1500;
        config.AttackRange = 300;
        config.ClientProfile = ClientProfile.Arthion;
        config.Sanitize();
        return config;
    }

    public static PlayerState Player(float x = 0, float y = 0, int hp = 800, int maxHp = 1000)
    {
        return new PlayerState
        {
            Name = "Hero",
            Hp = hp,
            MaxHp = maxHp,
            Mp = 100,
            MaxMp = 200,
            X = x,
            Y = y,
            Level = 35,
            MapName = "map_a2"
        };
    }

    public static EntityState Mob(uint vid, string name, EntityKind kind, float x, float y, int hp = 100)
    {
        var dx = x;
        var dy = y;
        return new EntityState
        {
            Vid = vid,
            Name = name,
            Kind = kind,
            Type = EntityMapping.FromKind(kind),
            TypeSpecified = true,
            Vnum = kind == EntityKind.Metin ? 8001u : 101u,
            Hp = hp,
            MaxHp = Math.Max(hp, 1),
            X = x,
            Y = y,
            Distance = MathF.Sqrt(dx * dx + dy * dy),
            OnScreen = true,
            ScreenX = 400,
            ScreenY = 300
        };
    }

    public static WorldSnapshot World(params EntityState[] entities) =>
        World(Player(), entities);

    public static WorldSnapshot World(PlayerState player, params EntityState[] entities)
    {
        var adjusted = new EntityState[entities.Length];
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            var dx = entity.X - player.X;
            var dy = entity.Y - player.Y;
            adjusted[i] = entity with { Distance = MathF.Sqrt(dx * dx + dy * dy) };
        }

        return new WorldSnapshot
        {
            Player = player,
            Entities = adjusted
        };
    }
}
