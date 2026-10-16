using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public sealed class AutoCombatController
{
    private const float MoveEpsilonPx2 = 30f * 30f;

    private CombatState _state = CombatState.Idle;
    private uint _targetVid;
    private long _targetAcquiredMs;
    private long _stateEnteredMs;
    private long _lastActionMs;
    private long _lastAttackMs;
    private float _lastPlayerX;
    private float _lastPlayerY;
    private long _lastMoveSeenMs;
    private int _recoveries;
    private bool _stopEmitted;
    private bool _rangedArmed;
    private bool _attacksPaused;
    private bool _pauseStopEmitted;
    private uint _armedVid;
    private float _sidestepFromX;
    private float _sidestepFromY;
    private float _sidestepToX;
    private float _sidestepToY;

    public CombatState State => _state;
    public uint TargetVid => _targetVid;
    public bool AttacksPaused => _attacksPaused;

    public void Reset()
    {
        _state = CombatState.Idle;
        _targetVid = 0;
        _targetAcquiredMs = 0;
        _stateEnteredMs = 0;
        _lastActionMs = 0;
        _lastAttackMs = 0;
        _lastPlayerX = 0;
        _lastPlayerY = 0;
        _lastMoveSeenMs = 0;
        _recoveries = 0;
        _stopEmitted = false;
        _rangedArmed = false;
        _attacksPaused = false;
        _pauseStopEmitted = false;
        _armedVid = 0;
        _sidestepFromX = 0;
        _sidestepFromY = 0;
        _sidestepToX = 0;
        _sidestepToY = 0;
    }

    public CombatTick Tick(WorldSnapshot? snapshot, BotConfig config, bool enabled, long nowMs)
    {
        config.Sanitize();
        if (!enabled)
        {
            var stop = !_stopEmitted;
            Reset();
            _stopEmitted = true;
            return new CombatTick(CombatState.Idle, null, stop ? NativeCombatAction.Stop() : null, "stopped", false);
        }

        _stopEmitted = false;
        if (_attacksPaused)
            return OnPaused(snapshot, config, nowMs);
        if (snapshot == null)
            return Enter(CombatState.Idle, nowMs, null, null, "waiting for live entities");

        if (config.RangedAttackEnabled)
            return TickRanged(snapshot, config, nowMs);

        _rangedArmed = false;
        WatchMovement(snapshot.Player, nowMs);

        var target = CurrentTarget(snapshot, config);
        if (target != null && !IsLegalPveTarget(target))
            target = null;

        return _state switch
        {
            CombatState.Idle => OnIdle(snapshot, config, nowMs),
            CombatState.AcquireTarget => OnAcquire(snapshot, config, nowMs),
            CombatState.Approach => OnApproach(snapshot, config, target, nowMs),
            CombatState.FaceTarget => OnFace(target, nowMs),
            CombatState.Attack => OnAttack(snapshot, config, target, nowMs),
            CombatState.Wait => OnWait(config, target, nowMs),
            CombatState.Verify => OnVerify(snapshot, config, target, nowMs),
            CombatState.Recover => OnRecover(snapshot, config, nowMs),
            CombatState.Sidestep => OnSidestep(snapshot, config, target, nowMs),
            _ => Enter(CombatState.Idle, nowMs, null, null, "unknown state")
        };
    }

    public void PauseAttacks()
    {
        if (_attacksPaused)
            return;
        _attacksPaused = true;
        _pauseStopEmitted = false;
        _armedVid = 0;
    }

    public void ResumeAttacks()
    {
        if (!_attacksPaused)
            return;
        _attacksPaused = false;
        _pauseStopEmitted = false;
        _armedVid = 0;
    }

    private CombatTick OnPaused(WorldSnapshot? snapshot, BotConfig config, long nowMs)
    {
        EntityState? target = null;
        if (snapshot != null)
        {
            target = CurrentTarget(snapshot, config);
            if (target == null)
            {
                _targetVid = 0;
                _state = CombatState.AcquireTarget;
            }
        }

        NativeCombatAction? stop = null;
        if (!_pauseStopEmitted)
        {
            _pauseStopEmitted = true;
            stop = NativeCombatAction.Stop();
        }

        return Stay(target, stop, "skill pause");
    }

    private CombatTick TickRanged(WorldSnapshot snapshot, BotConfig config, long nowMs)
    {
        if (!_rangedArmed)
        {
            _rangedArmed = true;
            _targetVid = 0;
            _lastAttackMs = 0;
            _recoveries = 0;
            return Enter(CombatState.Idle, nowMs, null, NativeCombatAction.Stop(), "ranged attack on");
        }

        var picked = TargetSelector.SelectMany(
            snapshot,
            config,
            config.RangedAttackMaxTargets,
            config.RangedAttackRadius,
            followPlayer: config.FarmRadius <= 0);
        if (picked.Count == 0)
        {
            _targetVid = 0;
            return Enter(CombatState.AcquireTarget, nowMs, null, null, "no farmable targets in ranged radius");
        }

        EntityState? current = null;
        for (var i = 0; i < picked.Count; i++)
        {
            if (picked[i].Vid != _targetVid)
                continue;
            current = picked[i];
            break;
        }

        if (_lastAttackMs != 0 && nowMs - _lastAttackMs < config.RangedAttackDelayMs)
            return Stay(current ?? picked[0], null, "waiting for ranged delay");

        var target = current ?? picked[0];
        _targetVid = target.Vid;
        _lastAttackMs = nowMs;
        return Enter(CombatState.Attack, nowMs, target,
            NativeCombatAction.Shot(picked, snapshot.Player.X, snapshot.Player.Y, config.RangedAvoidAttacks),
            $"ranged blast {picked.Count}");
    }

    private CombatTick OnIdle(WorldSnapshot snapshot, BotConfig config, long nowMs) =>
        OnAcquire(snapshot, config, nowMs);

    private CombatTick OnAcquire(WorldSnapshot snapshot, BotConfig config, long nowMs)
    {
        var picked = TargetSelector.Select(snapshot, config, 0);
        if (picked == null || !IsLegalPveTarget(picked))
            return Enter(CombatState.AcquireTarget, nowMs, null, null, "no farmable monster or Metin in range");

        _targetVid = picked.Vid;
        _targetAcquiredMs = nowMs;
        _recoveries = 0;
        _armedVid = 0;
        NoteMove(snapshot.Player, nowMs);
        var acquire = UseArthion(config)
            ? NativeCombatAction.PressActor(picked.Vid, picked.X, picked.Y)
            : NativeCombatAction.SetTarget(picked.Vid);
        return Enter(CombatState.Approach, nowMs, picked, acquire, $"acquired VID {picked.Vid}");
    }

    private CombatTick OnApproach(WorldSnapshot snapshot, BotConfig config, EntityState? target, long nowMs)
    {
        if (target == null)
            return Enter(CombatState.Verify, nowMs, null, null, "target missing while approaching");
        if (TimedOut(config, nowMs) && !InCombatRange(target, config))
            return Enter(CombatState.Recover, nowMs, target, null, "target timeout");
        if (!InCombatRange(target, config) && IsStuck(snapshot.Player, config, nowMs))
            return TrySidestep(snapshot, config, target, nowMs, "stuck approaching");
        if (UseArthion(config))
        {
            if (InMelee(target, config))
            {
                _armedVid = target.Vid;
                _lastAttackMs = nowMs;
                _targetAcquiredMs = nowMs;
                return Enter(CombatState.Attack, nowMs, target,
                    NativeCombatAction.PressActor(target.Vid, target.X, target.Y), "in melee, keep walking until native range");
            }

            return Stay(target, null, $"approaching VID {target.Vid} dist {target.Distance:0}");
        }

        if (InRange(target, config))
            return Enter(CombatState.FaceTarget, nowMs, target, NativeCombatAction.Face(target.Vid), "in attack range");

        if (ShouldEmit(nowMs, config.MoveIntervalMs))
        {
            _lastActionMs = nowMs;
            _armedVid = 0;
            return Stay(target, NativeCombatAction.PressActor(target.Vid, target.X, target.Y),
                $"approach VID {target.Vid} dist {target.Distance:0}");
        }

        return Stay(target, null, $"approaching VID {target.Vid} dist {target.Distance:0}");
    }

    private CombatTick OnFace(EntityState? target, long nowMs)
    {
        if (target == null)
            return Enter(CombatState.Verify, nowMs, null, null, "target lost before facing");
        return Enter(CombatState.Attack, nowMs, target, NativeCombatAction.Face(target.Vid), $"face VID {target.Vid}");
    }

    private CombatTick OnAttack(WorldSnapshot snapshot, BotConfig config, EntityState? target, long nowMs)
    {
        if (target == null)
            return Enter(CombatState.Verify, nowMs, null, null, "target lost before attack");
        if (UseArthion(config))
        {
            if (!InMelee(target, config))
            {
                _armedVid = 0;
                return Enter(CombatState.Approach, nowMs, target, null, "left melee range");
            }

            _targetAcquiredMs = nowMs;
            if (_armedVid == target.Vid)
                return Stay(target, null, $"auto-attacking VID {target.Vid}");

            _armedVid = target.Vid;
            _lastAttackMs = nowMs;
            return Stay(target, NativeCombatAction.PressActor(target.Vid, target.X, target.Y), $"auto-attack VID {target.Vid}");
        }

        if (!InRange(target, config))
            return Enter(CombatState.Approach, nowMs, target, NativeCombatAction.PressActor(target.Vid, target.X, target.Y), "left attack range");

        _lastAttackMs = nowMs;
        return Enter(CombatState.Wait, nowMs, target, NativeCombatAction.Attack(target.Vid), $"attack VID {target.Vid}");
    }

    private CombatTick OnWait(BotConfig config, EntityState? target, long nowMs)
    {
        if (nowMs - _stateEnteredMs < config.AttackIntervalMs)
            return Stay(target, null, "waiting for attack interval");
        return Enter(CombatState.Verify, nowMs, target, null, "verify after attack");
    }

    private CombatTick OnVerify(WorldSnapshot snapshot, BotConfig config, EntityState? target, long nowMs)
    {
        if (target == null || IsDead(target))
        {
            _targetVid = 0;
            return Enter(CombatState.AcquireTarget, nowMs, null, null, "target dead or missing; selecting next");
        }

        if (TimedOut(config, nowMs) && !InCombatRange(target, config))
            return Enter(CombatState.Recover, nowMs, target, null, "target timeout");
        if (InCombatRange(target, config))
            return Enter(CombatState.Attack, nowMs, target, null, "target still attackable");
        return Enter(CombatState.Approach, nowMs, target, null, "target still alive out of range");
    }

    private CombatTick OnRecover(WorldSnapshot snapshot, BotConfig config, long nowMs)
    {
        var target = CurrentTarget(snapshot, config);
        if (target == null || _recoveries >= 3 ||
            (TimedOut(config, nowMs) && !InCombatRange(target, config)))
            return DropTarget(nowMs, config, "recover: drop target");

        return TrySidestep(snapshot, config, target, nowMs, "recover: sidestep");
    }

    private CombatTick OnSidestep(WorldSnapshot snapshot, BotConfig config, EntityState? target, long nowMs)
    {
        if (target == null)
            return Enter(CombatState.Verify, nowMs, null, null, "target missing during sidestep");
        if (InCombatRange(target, config))
        {
            _recoveries = 0;
            _armedVid = 0;
            return Enter(CombatState.Attack, nowMs, target,
                NativeCombatAction.PressActor(target.Vid, target.X, target.Y), "sidestep reached melee");
        }

        var movedX = snapshot.Player.X - _sidestepFromX;
        var movedY = snapshot.Player.Y - _sidestepFromY;
        if (movedX * movedX + movedY * movedY > MoveEpsilonPx2)
        {
            _recoveries = 0;
            _armedVid = 0;
            NoteMove(snapshot.Player, nowMs);
            return Enter(CombatState.Approach, nowMs, target,
                NativeCombatAction.PressActor(target.Vid, target.X, target.Y), "sidestep done, resume chase");
        }

        if (TimedOut(config, nowMs) && !InCombatRange(target, config))
            return DropTarget(nowMs, config, "sidestep timeout, drop target");
        if (IsStuck(snapshot.Player, config, nowMs))
            return TrySidestep(snapshot, config, target, nowMs, "sidestep still stuck");
        if (ShouldEmit(nowMs, Math.Max(400, config.MoveIntervalMs)))
        {
            _lastActionMs = nowMs;
            return Stay(target, NativeCombatAction.MoveTo(_sidestepToX, _sidestepToY), "sidestep retry");
        }

        return Stay(target, null, "sidestep wait");
    }

    private CombatTick TrySidestep(
        WorldSnapshot snapshot,
        BotConfig config,
        EntityState target,
        long nowMs,
        string reason)
    {
        _recoveries++;
        if (_recoveries >= 3)
            return DropTarget(nowMs, config, "unstuck failed, drop target");

        if (!UnstuckPlanner.TryPickWaypoint(
                snapshot.Player,
                target,
                snapshot.Entities,
                config,
                _recoveries - 1,
                out var x,
                out var y))
            return DropTarget(nowMs, config, "no sidestep waypoint, drop target");

        _sidestepFromX = snapshot.Player.X;
        _sidestepFromY = snapshot.Player.Y;
        _sidestepToX = x;
        _sidestepToY = y;
        _armedVid = 0;
        NoteMove(snapshot.Player, nowMs);
        return Enter(CombatState.Sidestep, nowMs, target, NativeCombatAction.MoveTo(x, y),
            $"{reason} to {x:0},{y:0}");
    }

    private CombatTick DropTarget(long nowMs, BotConfig config, string reason)
    {
        _targetVid = 0;
        _armedVid = 0;
        return Enter(CombatState.AcquireTarget, nowMs, null,
            UseArthion(config) ? null : NativeCombatAction.Stop(), reason);
    }

    private EntityState? CurrentTarget(WorldSnapshot snapshot, BotConfig config)
    {
        if (_targetVid == 0)
            return null;
        var found = snapshot.Entities.FirstOrDefault(item => item.Vid == _targetVid);
        if (found == null)
            return null;
        if (!IsLegalPveTarget(found))
            return null;
        if (IsDead(found))
            return null;
        return found;
    }

    private static bool IsLegalPveTarget(EntityState entity)
    {
        if (entity.ResolvedType is ActorType.Player or ActorType.Npc)
            return false;
        if (entity.Kind is EntityKind.Player or EntityKind.Npc)
            return false;
        return entity.ResolvedType is ActorType.Enemy or ActorType.Stone ||
               entity.Kind is EntityKind.Monster or EntityKind.Metin or EntityKind.Boss;
    }

    private static bool IsDead(EntityState entity) => entity.Hp <= 0;

    private static bool InRange(EntityState target, BotConfig config) =>
        target.Distance <= config.AttackRange;

    private static bool InMelee(EntityState target, BotConfig config) =>
        target.Distance <= Math.Min(config.AttackRange, 180f);

    private static bool InCombatRange(EntityState target, BotConfig config) =>
        UseArthion(config) ? InMelee(target, config) : InRange(target, config);

    private static bool UseArthion(BotConfig config) =>
        config.ClientProfile == ClientProfile.Arthion ||
        config.ClientProfile == ClientProfile.Baerim;

    private bool TimedOut(BotConfig config, long nowMs) =>
        _targetVid != 0 && _targetAcquiredMs != 0 && nowMs - _targetAcquiredMs >= config.TargetTimeoutMs;

    private bool IsStuck(PlayerState player, BotConfig config, long nowMs)
    {
        if (_lastMoveSeenMs == 0)
            return false;
        var dx = player.X - _lastPlayerX;
        var dy = player.Y - _lastPlayerY;
        if (dx * dx + dy * dy > MoveEpsilonPx2)
            return false;
        return nowMs - _lastMoveSeenMs >= config.StuckTimeoutMs;
    }

    private void WatchMovement(PlayerState player, long nowMs)
    {
        var dx = player.X - _lastPlayerX;
        var dy = player.Y - _lastPlayerY;
        if (dx * dx + dy * dy > MoveEpsilonPx2)
            NoteMove(player, nowMs);
    }

    private void NoteMove(PlayerState player, long nowMs)
    {
        _lastPlayerX = player.X;
        _lastPlayerY = player.Y;
        _lastMoveSeenMs = nowMs;
    }

    private bool ShouldEmit(long nowMs, int intervalMs) =>
        _lastActionMs == 0 || nowMs - _lastActionMs >= intervalMs;

    private CombatTick Enter(CombatState state, long nowMs, EntityState? target, NativeCombatAction? action, string reason)
    {
        if (_state != state)
        {
            _state = state;
            _stateEnteredMs = nowMs;
            if (action != null)
                _lastActionMs = nowMs;
        }

        return new CombatTick(state, target, action, reason, true);
    }

    private CombatTick Stay(EntityState? target, NativeCombatAction? action, string reason) =>
        new(_state, target, action, reason, true);
}
