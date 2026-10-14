using FarmBot.Core.Entities;

namespace FarmBot.Core.Models;

public sealed record PlayerState
{
    public string Name { get; init; } = "";
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public int Mp { get; init; }
    public int MaxMp { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public int Level { get; init; }
    public int Exp { get; init; }
    public int NextExp { get; init; }
    public uint TargetVid { get; init; }
    public string MapName { get; init; } = "";
    public float FacingDeg { get; init; }

    public double HpPercent => MaxHp > 0 ? 100.0 * Hp / MaxHp : 0;
}

public sealed record EntityState
{
    public uint Vid { get; init; }
    public string Name { get; init; } = "";
    public EntityKind Kind { get; init; }
    public ActorType Type { get; init; }
    public bool TypeSpecified { get; init; }
    public uint Vnum { get; init; }
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public float Distance { get; init; }
    public int ScreenX { get; init; }
    public int ScreenY { get; init; }
    public bool OnScreen { get; init; }

    public ActorType ResolvedType => TypeSpecified ? Type : EntityMapping.FromKind(Kind);

    public bool IsCombatant =>
        Kind is EntityKind.Monster or EntityKind.Metin or EntityKind.Boss ||
        ResolvedType is ActorType.Enemy or ActorType.Stone;
}

public sealed record WorldSnapshot
{
    public long ReceivedAtMs { get; init; }
    public PlayerState Player { get; init; } = new();
    public IReadOnlyList<EntityState> Entities { get; init; } = Array.Empty<EntityState>();
}
