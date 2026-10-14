using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Input;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Core.Bot;

public sealed record BotView(
    bool Enabled,
    BotPhase Phase,
    string Reason,
    WorldSnapshot? Snapshot,
    EntityState? Target);

public sealed class BotRuntime : IDisposable
{
    private readonly IGameStateSource _source;
    private readonly IGameControl _control;
    private readonly IBotLog _log;
    private readonly Func<BotConfig> _configProvider;
    private readonly Func<bool> _cornerProbe;
    private readonly BotBrain _brain = new();
    private readonly object _gate = new();
    private CancellationTokenSource? _cts;
    private Thread? _thread;
    private bool _enabled;
    private bool _everReceived;
    private long _lastStateAtMs;
    private BotPhase _phase = BotPhase.Stopped;
    private string _reason = "stopped";
    private WorldSnapshot? _latest;
    private EntityState? _target;

    public BotRuntime(
        IGameStateSource source,
        IGameControl control,
        IBotLog log,
        Func<BotConfig> configProvider,
        Func<bool>? cornerProbe = null)
    {
        _source = source;
        _control = control;
        _log = log;
        _configProvider = configProvider;
        _cornerProbe = cornerProbe ?? (() => false);
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
                _brain.Reset();
            _enabled = enabled;
            if (!enabled)
            {
                _phase = BotPhase.Stopped;
                _reason = "stopped";
                _target = null;
            }
        }
    }

    public void Stop()
    {
        var wasEnabled = false;
        lock (_gate)
            wasEnabled = _enabled;
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
        _log.Info("bot stopped");
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
            return new BotView(_enabled, _phase, _reason, _latest, _target);
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
        lock (_gate)
            enabled = _enabled;

        var corner = enabled && config.StopOnCorner && _cornerProbe();
        var context = new TickContext(enabled, corner, _everReceived, _lastStateAtMs);
        var decision = _brain.Tick(snapshot, config, context, nowMs);

        if (decision.Phase == BotPhase.Failsafe && enabled)
        {
            lock (_gate)
                _enabled = false;
            _log.Warn($"failsafe: {decision.Reason}");
        }

        if (decision.Command != null && (enabled || decision.Phase == BotPhase.Failsafe))
        {
            try
            {
                _control.Execute(decision.Command);
                if (decision.Command.Kind != BotCommandKind.Stop)
                    _log.Info(decision.Command.Describe());
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
            if (_enabled || decision.Phase == BotPhase.Failsafe)
            {
                _phase = decision.Phase;
                _reason = decision.Reason;
            }
        }
    }

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
            if (token.WaitHandle.WaitOne(wait))
                break;
        }
    }
}
