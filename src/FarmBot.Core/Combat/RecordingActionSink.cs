namespace FarmBot.Core.Combat;

public sealed class RecordingActionSink : INativeActionSink
{
    private readonly object _gate = new();
    private readonly List<NativeCombatAction> _actions = new();
    private bool _available = true;

    public bool Available
    {
        get { lock (_gate) return _available; }
        set { lock (_gate) _available = value; }
    }

    public string Status => Available ? "recording native actions" : "native action sink disconnected";

    public IReadOnlyList<NativeCombatAction> Actions
    {
        get { lock (_gate) return _actions.ToList(); }
    }

    public bool TrySubmit(NativeCombatAction action)
    {
        if (action.Kind == NativeActionKind.None)
            return false;
        lock (_gate)
        {
            if (!_available)
                return false;
            _actions.Add(action);
            return true;
        }
    }

    public void Clear()
    {
        lock (_gate)
            _actions.Clear();
    }
}

public sealed class DisconnectedActionSink : INativeActionSink
{
    public bool Available => false;
    public string Status => ExternalCommandSurface.NoExternalInterface;
    public bool TrySubmit(NativeCombatAction action) => false;
}
