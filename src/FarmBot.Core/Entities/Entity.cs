using FarmBot.Core.Models;

namespace FarmBot.Core.Entities;

public sealed record Entity
{
    public uint Vid { get; init; }
    public ActorType Type { get; init; }
    public uint Vnum { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public float Distance { get; init; }
    public string Name { get; init; } = "";
    public int Hp { get; init; } = 1;

    public bool IsMonster => Type == ActorType.Enemy;
    public bool IsStone => Type == ActorType.Stone;
    public bool IsPlayer => Type == ActorType.Player;
    public bool IsNpc => Type == ActorType.Npc;
    public bool IsFarmable => IsMonster || IsStone;
}

public sealed record GameFrame
{
    public long ReceivedAtMs { get; init; }
    public Entity? Main { get; init; }
    public IReadOnlyList<Entity> Entities { get; init; } = Array.Empty<Entity>();
    public PlayerState? Player { get; init; }
    public string Source { get; init; } = "";
    public bool FromLiveClient { get; init; }
}
