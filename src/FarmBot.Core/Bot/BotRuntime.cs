using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Input;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;
using FarmBot.Core.Safety;

namespace FarmBot.Core.Bot;

public sealed record BotView(
    bool Enabled,
    BotPhase Phase,
    string Reason,
    WorldSnapshot? Snapshot,
    EntityState? Target,
    CombatState CombatState = CombatState.Idle,
    string NativeInterface = "",
    bool RangedOnly = false);

public sealed class BotRuntime : IDisposable
{
    private readonly IGameStateSource _source;
    private readonly IGameControl _control;
    private readonly IBotLog _log;
    private readonly Func<BotConfig> _configProvider;
    private readonly Func<bool> _cornerProbe;
    private readonly INativeActionSink? _sink;
    private readonly bool _nativeCombat;
    private readonly BotBrain _brain = new();
    private readonly AutoCombatController _combat = new();
    private readonly object _gate = new();
    private CombatState _combatState = CombatState.Idle;
    private string _nativeInterface = "";
    private CancellationTokenSource? _cts;
    private Thread? _thread;
    private bool _enabled;
    private bool _rangedOnly;
    private bool _everReceived;
    private long _lastStateAtMs;
    private BotPhase _phase = BotPhase.Stopped;
    private string _reason = "stopped";
    private WorldSnapshot? _latest;
    private EntityState? _target;
    private bool _lootLoop;

    public BotRuntime(
        IGameStateSource source,
        IGameControl control,
        IBotLog log,
        Func<BotConfig> configProvider,
        Func<bool>? cornerProbe = null,
        bool nativeCombat = false,
        INativeActionSink? sink = null)
    {
        _source = source;
        _control = control;
        _log = log;
        _configProvider = configProvider;
        _cornerProbe = cornerProbe ?? (() => false);
        _nativeCombat = nativeCombat;
        _sink = sink;
    }

    public void StartLoop()
    {
        if (_thread != null)
            return;
        _cts = new CancellationTokenSource();
        _thread = new Thread(() => Loop(_cts.Token))
        {
            IsBackground = true,
            Name = "FarmBot"
        };
        _thread.Start();
    }

    public void SetEnabled(bool enabled)
    {
        lock (_gate)
        {
            if (enabled && !_enabled)
            {
                _brain.Reset();
                _combat.Reset();
            }
            _enabled = enabled;
            if (enabled)
                _rangedOnly = false;
            if (!enabled)
            {
                _rangedOnly = false;
                _phase = BotPhase.Stopped;
                _reason = "stopped";
                _target = null;
            }
        }
        SyncLootLoop(_configProvider(), enabled && !_rangedOnly);
    }

    public void SetRangedOnly(bool enabled)
    {
        lock (_gate)
        {
            if (enabled && (!_enabled || !_rangedOnly))
            {
                _brain.Reset();
                _combat.Reset();
            }
            _enabled = enabled;
            _rangedOnly = enabled;
            if (!enabled)
            {
                _phase = BotPhase.Stopped;
                _reason = "stopped";
                _target = null;
            }
        }
        SyncLootLoop(_configProvider(), false);
    }

    public void Stop()
    {
        bool wasEnabled;
        bool rangedOnly;
        lock (_gate)
        {
            wasEnabled = _enabled;
            rangedOnly = _rangedOnly;
        }
        SetEnabled(false);
        if (!wasEnabled)
            return;
        try
        {
            _control.Execute(BotCommand.Stop());
        }
        catch (Exception ex)
        {
            _log.Error($"stop failed: {ex.Message}");
        }
        _log.Info(rangedOnly ? "ranged attack stopped" : "bot stopped");
    }

    public void Panic()
    {
        SetEnabled(false);
        lock (_gate)
        {
            _phase = BotPhase.Failsafe;
            _reason = "panic hotkey";
        }
        try
        {
            _control.Execute(BotCommand.Stop());
        }
        catch (Exception ex)
        {
            _log.Error($"panic stop failed: {ex.Message}");
        }
        _log.Warn("panic stop");
    }

    public BotView Capture()
    {
        lock (_gate)
            return new BotView(_enabled, _phase, _reason, _latest, _target, _combatState, _nativeInterface, _rangedOnly);
    }

    public void TickOnce(BotConfig config, long nowMs, int dtMs)
    {
        config.Sanitize();
        if (_source is IAdvancingSource advancing)
            advancing.Advance(dtMs);

        var snapshot = _source.GetSnapshot(nowMs);
        if (snapshot != null)
        {
            _everReceived = true;
            _lastStateAtMs = snapshot.ReceivedAtMs;
        }

        bool enabled;
        bool rangedOnly;
        lock (_gate)
        {
            enabled = _enabled;
            rangedOnly = _rangedOnly;
        }

        if (rangedOnly && snapshot != null)
        {
            config.RangedAttackEnabled = true;
            config.FarmCenterX = snapshot.Player.X;
            config.FarmCenterY = snapshot.Player.Y;
            config.ReturnToCenter = false;
        }

        var corner = enabled && config.StopOnCorner && _cornerProbe();
        var context = new TickContext(enabled, corner, _everReceived, _lastStateAtMs);
        var decision = _nativeCombat
            ? TickNative(snapshot, config, context, nowMs)
            : _brain.Tick(snapshot, config, context, nowMs);

        if (decision.Phase == BotPhase.Failsafe && enabled)
        {
            lock (_gate)
            {
                _enabled = false;
                _rangedOnly = false;
            }
            _log.Warn($"failsafe: {decision.Reason}");
            SyncLootLoop(config, false);
        }

        if (decision.Command != null && (enabled || decision.Phase == BotPhase.Failsafe))
        {
            try
            {
                _control.Execute(decision.Command);
                if (decision.Command.Kind != BotCommandKind.Stop)
                {
                    var status = _sink?.Status ?? "";
                    if (status.Contains("rejected", StringComparison.OrdinalIgnoreCase) ||
                        status.Contains("not connected", StringComparison.OrdinalIgnoreCase) ||
                        status.Contains("I/O failed", StringComparison.OrdinalIgnoreCase))
                        _log.Warn($"{decision.Command.Describe()} failed: {status}");
                    else
                        _log.Info($"{decision.Command.Describe()} {status}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"control failed: {ex.Message}");
            }
        }

        lock (_gate)
        {
            _latest = snapshot;
            _target = decision.Target;
            _nativeInterface = _sink?.Status ?? "";
            if (_enabled || decision.Phase == BotPhase.Failsafe)
            {
                _phase = decision.Phase;
                _reason = decision.Reason;
            }
        }
    }

    private BrainDecision TickNative(WorldSnapshot? snapshot, BotConfig config, TickContext context, long nowMs)
    {
        if (!context.Enabled)
        {
            _combat.Reset();
            _combatState = CombatState.Idle;
            return new BrainDecision(BotPhase.Stopped, null, null, "stopped");
        }

        var trip = FailsafeMonitor.Reason(snapshot, config, context, nowMs);
        if (trip != null)
        {
            _combat.Reset();
            _combatState = CombatState.Idle;
            return new BrainDecision(BotPhase.Failsafe, null, BotCommand.Stop(), trip);
        }

        bool rangedOnly;
        lock (_gate)
            rangedOnly = _rangedOnly;
        SyncLootLoop(config, !rangedOnly);
        var tick = _combat.Tick(snapshot, config, context.Enabled, nowMs);
        _combatState = tick.State;
        return new BrainDecision(ToPhase(tick.State), tick.Target, tick.Action?.ToCommand(), tick.Reason);
    }

    private void SyncLootLoop(BotConfig config, bool enabled)
    {
        var want = enabled && config.LootEnabled && _sink != null;
        if (want == _lootLoop)
            return;
        if (!_sink!.TrySubmit(NativeCombatAction.LootLoop(want)))
            return;
        _lootLoop = want;
        _log.Info(want ? "loot loop: PickCloseItem every 200ms" : "loot loop off");
    }

    private static BotPhase ToPhase(CombatState state) => state switch
    {
        CombatState.Idle => BotPhase.Idle,
        CombatState.AcquireTarget => BotPhase.Idle,
        CombatState.Approach => BotPhase.Moving,
        CombatState.FaceTarget => BotPhase.Moving,
        CombatState.Attack => BotPhase.Attacking,
        CombatState.Wait => BotPhase.Attacking,
        CombatState.Verify => BotPhase.Attacking,
        CombatState.Recover => BotPhase.Moving,
        _ => BotPhase.Idle
    };

    public void Dispose()
    {
        _cts?.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(2));
        _cts?.Dispose();
        _thread = null;
    }

    private void Loop(CancellationToken token)
    {
        var previous = Environment.TickCount64;
        while (!token.IsCancellationRequested)
        {
            var now = Environment.TickCount64;
            var dt = (int)Math.Clamp(now - previous, 1, 1000);
            previous = now;
            BotConfig config;
            try
            {
                config = _configProvider();
                TickOnce(config, now, dt);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                config = BotConfig.CreateDefault();
            }

            var wait = Math.Max(1, config.TickMs);
            bool enabled;
            lock (_gate)
                enabled = _enabled;
            if (enabled && config.RangedAttackEnabled)
                wait = Math.Max(1, Math.Min(wait, config.RangedAttackDelayMs));
            if (token.WaitHandle.WaitOne(wait))
                break;
        }
    }
}
