using FarmBot.Core.Combat;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;

namespace FarmBot.Core.Input;

public sealed class NativeGameControl : IGameControl
{
    private readonly INativeActionSink _sink;
    private readonly IBotLog _log;

    public NativeGameControl(INativeActionSink sink, IBotLog log)
    {
        _sink = sink;
        _log = log;
    }

    public INativeActionSink Sink => _sink;

    public void Execute(BotCommand command)
    {
        var action = ToAction(command);
        if (action == null)
            return;
        if (action.Kind == NativeActionKind.Stop && !_sink.Available)
            return;
        if (_sink.TrySubmit(action))
            return;
        _log.Warn($"native action '{action.Describe()}' not delivered: {_sink.Status}");
    }

    private static NativeCombatAction? ToAction(BotCommand command) => command.Kind switch
    {
        BotCommandKind.SetTarget => NativeCombatAction.SetTarget(command.Vid),
        BotCommandKind.PressActor => NativeCombatAction.PressActor(command.Vid, command.X, command.Y),
        BotCommandKind.Face => NativeCombatAction.Face(command.Vid),
        BotCommandKind.Attack => NativeCombatAction.Attack(command.Vid),
        BotCommandKind.Shot => new NativeCombatAction
        {
            Kind = NativeActionKind.Shot,
            Vid = command.Vid,
            X = command.X,
            Y = command.Y,
            Hits = command.Hits,
            Avoid = command.Avoid
        },
        BotCommandKind.Move => NativeCombatAction.MoveTo(command.X, command.Y),
        BotCommandKind.Stop => NativeCombatAction.Stop(),
        BotCommandKind.Pickup => NativeCombatAction.Pickup(),
        _ => null
    };
}
