using FarmBot.Core.Models;

namespace FarmBot.Core.Entities;

public static class EntityMapping
{
    public static ActorType FromKind(EntityKind kind) => kind switch
    {
        EntityKind.Monster or EntityKind.Boss => ActorType.Enemy,
        EntityKind.Metin => ActorType.Stone,
        EntityKind.Npc => ActorType.Npc,
        EntityKind.Player => ActorType.Player,
        _ => ActorType.Object
    };

    public static EntityKind ToKind(ActorType type) => type switch
    {
        ActorType.Enemy => EntityKind.Monster,
        ActorType.Stone => EntityKind.Metin,
        ActorType.Npc => EntityKind.Npc,
        ActorType.Player => EntityKind.Player,
        _ => EntityKind.Unknown
    };

    public static string TypeName(ActorType type) => type switch
    {
        ActorType.Enemy => "ENEMY",
        ActorType.Npc => "NPC",
        ActorType.Stone => "STONE",
        ActorType.Player => "PLAYER",
        ActorType.Building => "BUILDING",
        ActorType.Object => "OBJECT",
        _ => type.ToString().ToUpperInvariant()
    };

    public static Entity FromState(EntityState state)
    {
        var type = state.TypeSpecified ? state.Type : FromKind(state.Kind);
        return new Entity
        {
            Vid = state.Vid,
            Type = type,
            Vnum = state.Vnum,
            X = state.X,
            Y = state.Y,
            Z = state.Z,
            Distance = state.Distance,
            Name = state.Name,
            Hp = state.Hp
        };
    }

    public static Entity FromPlayer(PlayerState player) => new()
    {
        Vid = 0,
        Type = ActorType.Player,
        Vnum = 0,
        X = player.X,
        Y = player.Y,
        Z = player.Z,
        Name = player.Name,
        Hp = player.Hp
    };

    public static EntityState ToState(Entity entity) => new()
    {
        Vid = entity.Vid,
        Name = entity.Name,
        Kind = ToKind(entity.Type),
        Type = entity.Type,
        TypeSpecified = true,
        Vnum = entity.Vnum,
        Hp = entity.Hp,
        MaxHp = Math.Max(entity.Hp, 1),
        X = entity.X,
        Y = entity.Y,
        Z = entity.Z,
        Distance = entity.Distance
    };

    public static GameFrame ToFrame(WorldSnapshot snapshot, string source, bool fromLiveClient)
    {
        var main = FromPlayer(snapshot.Player);
        var entities = EntityManager.WithDistances(snapshot.Entities.Select(FromState), main);
        return new GameFrame
        {
            ReceivedAtMs = snapshot.ReceivedAtMs,
            Main = main,
            Entities = entities,
            Player = snapshot.Player,
            Source = source,
            FromLiveClient = fromLiveClient
        };
    }

    public static WorldSnapshot ToSnapshot(GameFrame frame)
    {
        var main = frame.Main;
        var player = frame.Player;
        return new WorldSnapshot
        {
            ReceivedAtMs = frame.ReceivedAtMs,
            Player = new PlayerState
            {
                Name = FirstNonEmpty(player?.Name, main?.Name),
                Hp = player is { MaxHp: > 0 } ? player.Hp : 0,
                MaxHp = player is { MaxHp: > 0 } ? player.MaxHp : 0,
                Mp = player is { MaxMp: > 0 } ? player.Mp : 0,
                MaxMp = player is { MaxMp: > 0 } ? player.MaxMp : 0,
                X = main?.X ?? player?.X ?? 0,
                Y = main?.Y ?? player?.Y ?? 0,
                Z = main?.Z ?? player?.Z ?? 0,
                Level = player?.Level ?? 0,
                Exp = player?.Exp ?? 0,
                NextExp = player?.NextExp ?? 0,
                TargetVid = player?.TargetVid ?? 0,
                MapName = player?.MapName ?? "",
                FacingDeg = player?.FacingDeg ?? 0
            },
            Entities = frame.Entities.Select(ToState).ToList()
        };
    }

    private static string FirstNonEmpty(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) ? a! : b ?? "";
}
