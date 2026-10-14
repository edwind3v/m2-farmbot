using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public enum CombatState
{
    Idle,
    AcquireTarget,
    Approach,
    FaceTarget,
    Attack,
    Wait,
    Verify,
    Recover
}

public enum NativeActionKind
{
    None,
    SetTarget,
    PressActor,
    Face,
    Attack,
    Shot,
    MoveTo,
    Stop,
    Pickup,
    LootLoop
}

public sealed record NativeCombatAction
{
    public NativeActionKind Kind { get; init; }
    public uint Vid { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public string Hits { get; init; } = "";

    public string Describe() => Kind switch
    {
        NativeActionKind.SetTarget => $"set_target vid={Vid}",
        NativeActionKind.PressActor => $"press_actor vid={Vid}",
        NativeActionKind.Face => $"face vid={Vid}",
        NativeActionKind.Attack => $"attack vid={Vid}",
        NativeActionKind.Shot => string.IsNullOrEmpty(Hits)
            ? $"shot vid={Vid}"
            : $"shot {Hits.Split(';').Length} targets",
        NativeActionKind.MoveTo => $"move ({X:0},{Y:0})",
        NativeActionKind.Stop => "stop",
        NativeActionKind.Pickup => "pickup",
        NativeActionKind.LootLoop => Vid != 0 ? "loot loop on" : "loot loop off",
        _ => "none"
    };

    public BotCommand ToCommand() => Kind switch
    {
        NativeActionKind.SetTarget => BotCommand.SetTarget(Vid),
        NativeActionKind.PressActor => BotCommand.PressActor(Vid, X, Y),
        NativeActionKind.Face => BotCommand.Face(Vid),
        NativeActionKind.Attack => new BotCommand { Kind = BotCommandKind.Attack, Vid = Vid, X = X, Y = Y },
        NativeActionKind.Shot => new BotCommand { Kind = BotCommandKind.Shot, Vid = Vid, X = X, Y = Y, Hits = Hits },
        NativeActionKind.MoveTo => BotCommand.Move(X, Y),
        NativeActionKind.Stop => BotCommand.Stop(),
        NativeActionKind.Pickup => BotCommand.Pickup(new EntityState(), ""),
        NativeActionKind.LootLoop => BotCommand.Stop(),
        _ => BotCommand.Stop()
    };

    public static NativeCombatAction SetTarget(uint vid) => new() { Kind = NativeActionKind.SetTarget, Vid = vid };
    public static NativeCombatAction PressActor(uint vid, float x, float y) => new() { Kind = NativeActionKind.PressActor, Vid = vid, X = x, Y = y };
    public static NativeCombatAction Face(uint vid) => new() { Kind = NativeActionKind.Face, Vid = vid };
    public static NativeCombatAction Attack(uint vid) => new() { Kind = NativeActionKind.Attack, Vid = vid };
    public static NativeCombatAction Shot(uint vid) => new() { Kind = NativeActionKind.Shot, Vid = vid };

    public static NativeCombatAction Shot(IReadOnlyList<EntityState> targets, float restoreX, float restoreY)
    {
        var first = targets[0];
        return new()
        {
            Kind = NativeActionKind.Shot,
            Vid = first.Vid,
            X = restoreX,
            Y = restoreY,
            Hits = BotCommand.EncodeHits(targets)
        };
    }
    public static NativeCombatAction MoveTo(float x, float y) => new() { Kind = NativeActionKind.MoveTo, X = x, Y = y };
    public static NativeCombatAction Stop() => new() { Kind = NativeActionKind.Stop };
    public static NativeCombatAction Pickup() => new() { Kind = NativeActionKind.Pickup };
    public static NativeCombatAction LootLoop(bool on) => new() { Kind = NativeActionKind.LootLoop, Vid = on ? 1u : 0 };
}

public sealed record CombatTick(
    CombatState State,
    EntityState? Target,
    NativeCombatAction? Action,
    string Reason,
    bool Enabled);
