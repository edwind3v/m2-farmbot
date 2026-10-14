namespace FarmBot.Core.Combat;

public interface INativeActionSink
{
    bool Available { get; }
    string Status { get; }
    bool TrySubmit(NativeCombatAction action);
}
