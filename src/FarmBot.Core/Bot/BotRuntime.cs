using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Input;
using FarmBot.Core.Logging;
using FarmBot.Core.Looting;
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
    bool RangedOnly = false,
    IReadOnlyList<PotionStack>? Potions = null,
    int GreenRemain = 0,
    int PurpleRemain = 0);

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
    private uint _lootMask;
    private int _lootDelay;
    private bool _lootRanged;
    private float _lootRadius;
    private readonly PotionController _potions = new();
    private IReadOnlyList<PotionStack> _potionStacks = Array.Empty<PotionStack>();
    private int _greenRemain;
    private int _purpleRemain;
    private long _potsAtMs;
    private bool _potsInGame;
    private int _potsEmptyTries;

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
        var cts = new CancellationTokenSource();
        _cts = cts;
        var token = cts.Token;
        _thread = new Thread(() =>
        {
            try
            {
                Loop(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _log.Error($"bot loop failed: {ex.Message}");
            }
        })
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
        SyncLootLoop(_configProvider(), enabled);
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
        SyncLootLoop(_configProvider(), enabled);
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
            return new BotView(_enabled, _phase, _reason, _latest, _target, _combatState, _nativeInterface, _rangedOnly, _potionStacks, _greenRemain, _purpleRemain);
    }

    public void TickOnce(BotConfig config, long nowMs, int dtMs)
    {
        config.Sanitize();
        if (_source is IAdvancingSource advancing)
            advancing.Advance(dtMs);

        var snapshot = OverlayHud(_source.GetSnapshot(nowMs));
        TickPotions(snapshot, config, nowMs);
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
            config.FarmRadius = 0;
            config.ReturnToCenter = false;
        }

        var corner = enabled && config.StopOnCorner && _cornerProbe();
        SyncLootLoop(config, enabled);
        if (config.RangedPickupEnabled && snapshot != null)
            SubmitRangedPickups(snapshot, config);
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
        var tick = _combat.Tick(snapshot, config, context.Enabled, nowMs);
        _combatState = tick.State;
        if (tick.Action != null)
        {
            _log.Info($"combat {tick.State} {tick.Action.Describe()} dist={tick.Target?.Distance:0} {tick.Reason}");
        }
        return new BrainDecision(ToPhase(tick.State), tick.Target, tick.Action?.ToCommand(), tick.Reason);
    }

    private void SyncLootLoop(BotConfig config, bool enabled)
    {
        bool rangedOnly;
        lock (_gate)
            rangedOnly = _rangedOnly;
        if (_sink == null)
            return;
        var mask = LootPolicy.LoopMask(config, enabled, rangedOnly);
        var delay = config.FastPickupEnabled ? Math.Max(1, config.FastPickupDelayMs) : 50;
        var radius = LootPolicy.PickupRadius(config);
        var want = mask != 0;
        var ranged = (mask & LootPolicy.RangedLoop) != 0;
        if (want == _lootLoop && delay == _lootDelay && ranged == _lootRanged && Math.Abs(radius - _lootRadius) < 0.5f && mask == _lootMask)
            return;
        if (!_sink.TrySubmit(NativeCombatAction.LootLoop(mask, delay, radius)))
            return;
        _lootLoop = want;
        _lootMask = mask;
        _lootDelay = delay;
        _lootRanged = ranged;
        _lootRadius = radius;
        _log.Info(want
            ? NativeCombatAction.LootLoop(mask, delay, radius).Describe()
            : "loot loop off");
    }

    private void SubmitRangedPickups(WorldSnapshot snapshot, BotConfig config)
    {
        if (_sink == null)
            return;
        var radius = LootPolicy.PickupRadius(config);
        var sent = 0;
        foreach (var entity in snapshot.Entities)
        {
            if (entity.Kind != EntityKind.Drop || entity.Vid == 0 || entity.Distance > radius)
                continue;
            if (!_sink.TrySubmit(NativeCombatAction.PickupVid(entity.Vid)))
                break;
            if (++sent >= 12)
                break;
        }
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
        var cts = Interlocked.Exchange(ref _cts, null);
        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        _thread?.Join(TimeSpan.FromSeconds(2));
        _thread = null;
        try
        {
            cts?.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
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
            if (config.FastPickupEnabled || config.RangedPickupEnabled)
                wait = Math.Max(1, Math.Min(wait, config.FastPickupEnabled ? config.FastPickupDelayMs : 50));
            if (WaitCancelled(token, wait))
                break;
        }
    }

    private static bool WaitCancelled(CancellationToken token, int waitMs)
    {
        try
        {
            return token.IsCancellationRequested || token.WaitHandle.WaitOne(Math.Max(1, waitMs));
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    private void TickPotions(WorldSnapshot? snapshot, BotConfig config, long nowMs)
    {
        if (config.ClientProfile != ClientProfile.Arthion || _sink == null)
            return;
        if (snapshot == null || snapshot.Player.MaxHp <= 0)
        {
            if (_potsInGame)
            {
                _potsInGame = false;
                _potsEmptyTries = 0;
                _potsAtMs = 0;
                lock (_gate)
                    _potionStacks = Array.Empty<PotionStack>();
            }
            return;
        }

        int have;
        lock (_gate)
            have = _potionStacks.Count;
        var interval = !_potsInGame ? 0 : (have == 0 && _potsEmptyTries < 12 ? 1000 : 8000);
        if (nowMs - _potsAtMs >= interval && _sink.TryReadPots(out var scan))
        {
            var first = !_potsInGame;
            _potsAtMs = nowMs;
            _potsInGame = true;
            if (scan.Pots.Count > 0)
            {
                _potsEmptyTries = 12;
                if (first)
                    _log.Info($"pots loaded {scan.Pots.Count} stacks");
            }
            else if (_potsEmptyTries < 12)
            {
                _potsEmptyTries++;
                if (first)
                    _log.Info("pots scan empty, will retry after stats");
            }
            lock (_gate)
            {
                if (scan.Pots.Count > 0 || have == 0)
                    _potionStacks = scan.Pots;
                _greenRemain = scan.GreenRemain;
                _purpleRemain = scan.PurpleRemain;
            }
            _potions.NoteNativeRemain(scan.GreenRemain, scan.PurpleRemain, nowMs);
        }

        IReadOnlyList<PotionStack> pots;
        lock (_gate)
            pots = _potionStacks;
        var slot = _potions.NextSlot(pots, snapshot?.Player, config,
            _potions.GreenRemainSec(nowMs) > 0 ? _potions.GreenRemainSec(nowMs) : _greenRemain,
            _potions.PurpleRemainSec(nowMs) > 0 ? _potions.PurpleRemainSec(nowMs) : _purpleRemain,
            nowMs);
        if (slot != null && _sink.TryUseItem(slot.Value))
            _log.Info($"use pot slot={slot.Value}");
        lock (_gate)
        {
            _greenRemain = _potions.GreenRemainSec(nowMs);
            _purpleRemain = _potions.PurpleRemainSec(nowMs);
        }
    }

    private WorldSnapshot? OverlayHud(WorldSnapshot? snapshot)
    {
        if (snapshot == null || _sink == null || !_sink.TryReadHud(out var hud))
            return snapshot;
        return snapshot with
        {
            Player = snapshot.Player with
            {
                Level = hud.Level,
                Exp = hud.Exp,
                NextExp = hud.NextExp,
                Hp = hud.Hp,
                MaxHp = hud.MaxHp,
                Mp = hud.Mp,
                MaxMp = hud.MaxMp
            }
        };
    }
}
