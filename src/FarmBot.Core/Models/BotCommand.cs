namespace FarmBot.Core.Models;

public enum BotCommandKind
{
    Attack,
    Shot,
    Move,
    Skill,
    Pickup,
    Heal,
    Stop,
    SetTarget,
    PressActor,
    Face
}

public enum BotPhase
{
    Stopped,
    WaitingForClient,
    Idle,
    Moving,
    Attacking,
    Looting,
    Returning,
    Failsafe
}

public sealed record BotCommand
{
    public BotCommandKind Kind { get; init; }
    public uint Vid { get; init; }
    public string TargetName { get; init; } = "";
    public float X { get; init; }
    public float Y { get; init; }
    public int ScreenX { get; init; }
    public int ScreenY { get; init; }
    public bool OnScreen { get; init; }
    public int SkillSlot { get; init; }
    public string Key { get; init; } = "";
    public int MpCost { get; init; }
    public string Hits { get; init; } = "";

    public string Describe() => Kind switch
    {
        BotCommandKind.Attack => $"attack {TargetName} vid={Vid}",
        BotCommandKind.Shot => string.IsNullOrEmpty(Hits)
            ? $"shot {TargetName} vid={Vid}"
            : $"shot {Hits.Split(';').Length} targets",
        BotCommandKind.Move => $"move ({X:0},{Y:0})",
        BotCommandKind.Skill => $"skill slot={SkillSlot} key={Key} on {TargetName} vid={Vid}",
        BotCommandKind.Pickup => $"pickup {TargetName} vid={Vid}",
        BotCommandKind.Heal => $"heal key={Key}",
        BotCommandKind.Stop => "stop",
        BotCommandKind.SetTarget => $"set_target vid={Vid}",
        BotCommandKind.PressActor => $"press_actor vid={Vid}",
        BotCommandKind.Face => $"face vid={Vid}",
        _ => Kind.ToString()
    };

    public static BotCommand Attack(EntityState target, string key) => new()
    {
        Kind = BotCommandKind.Attack,
        Vid = target.Vid,
        TargetName = target.Name,
        X = target.X,
        Y = target.Y,
        ScreenX = target.ScreenX,
        ScreenY = target.ScreenY,
        OnScreen = target.OnScreen,
        Key = key
    };

    public static BotCommand Shot(EntityState target) => Shot(new[] { target }, target.X, target.Y);

    public static BotCommand Shot(IReadOnlyList<EntityState> targets, float restoreX, float restoreY)
    {
        var first = targets[0];
        return new()
        {
            Kind = BotCommandKind.Shot,
            Vid = first.Vid,
            TargetName = targets.Count == 1 ? first.Name : $"{targets.Count} targets",
            X = restoreX,
            Y = restoreY,
            ScreenX = first.ScreenX,
            ScreenY = first.ScreenY,
            OnScreen = first.OnScreen,
            Hits = EncodeHits(targets)
        };
    }

    public static string EncodeHits(IReadOnlyList<EntityState> targets)
    {
        var parts = new string[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var item = targets[i];
            parts[i] = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2}",
                item.Vid,
                item.X,
                item.Y);
        }

        return string.Join(";", parts);
    }

    public static BotCommand Move(float x, float y) => new()
    {
        Kind = BotCommandKind.Move,
        X = x,
        Y = y
    };

    public static BotCommand Skill(EntityState target, int slot, string key, int mpCost) => new()
    {
        Kind = BotCommandKind.Skill,
        Vid = target.Vid,
        TargetName = target.Name,
        X = target.X,
        Y = target.Y,
        ScreenX = target.ScreenX,
        ScreenY = target.ScreenY,
        OnScreen = target.OnScreen,
        SkillSlot = slot,
        Key = key,
        MpCost = mpCost
    };

    public static BotCommand Pickup(EntityState drop, string key) => new()
    {
        Kind = BotCommandKind.Pickup,
        Vid = drop.Vid,
        TargetName = drop.Name,
        X = drop.X,
        Y = drop.Y,
        ScreenX = drop.ScreenX,
        ScreenY = drop.ScreenY,
        OnScreen = drop.OnScreen,
        Key = key
    };

    public static BotCommand Heal(string key) => new()
    {
        Kind = BotCommandKind.Heal,
        Key = key
    };

    public static BotCommand Stop() => new()
    {
        Kind = BotCommandKind.Stop
    };

    public static BotCommand SetTarget(uint vid) => new()
    {
        Kind = BotCommandKind.SetTarget,
        Vid = vid
    };

    public static BotCommand PressActor(uint vid, float x, float y) => new()
    {
        Kind = BotCommandKind.PressActor,
        Vid = vid,
        X = x,
        Y = y
    };

    public static BotCommand Face(uint vid) => new()
    {
        Kind = BotCommandKind.Face,
        Vid = vid
    };
}

public sealed record BrainDecision(BotPhase Phase, EntityState? Target, BotCommand? Command, string Reason);

public readonly record struct TickContext(bool Enabled, bool CornerTripped, bool EverReceivedState, long LastStateAtMs);
