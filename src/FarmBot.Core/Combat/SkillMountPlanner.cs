namespace FarmBot.Core.Combat;

public enum SkillMountPhase
{
    Idle,
    Dismounting,
    /// <summary>Read live active to choose off-then-on vs on-only.</summary>
    Decide,
    /// <summary>Off-click sent (or retrying); waiting for inactive edge.</summary>
    WaitOff,
    /// <summary>Inactive confirmed; waiting for cool clear then on-click / active edge.</summary>
    WaitOn,
    Remounting
}

public enum SkillMountCommand
{
    None,
    Dismount,
    Remount,
    UseSkill
}

public readonly record struct SkillMountAction(
    SkillMountCommand Command,
    LearnedSkill? Skill,
    SkillMountPhase Phase,
    LearnedSkill? Confirmed = null);

/// <summary>
/// Dismount → for each buff, ensure active via live IsSkillActive (+ cool when known):
/// if on, one off-click until off; wait cool clear; one on-click until on → remount.
/// Never remounts while the current buff is still off. Clicks are edge-triggered, not timer-advanced.
/// </summary>
public sealed class SkillMountPlanner
{
    public const int DismountWaitMs = 500;
    public const int SkipAnimWaitMs = 400;
    public const int RemountWaitMs = 1500;
    /// <summary>Minimum gap between retry clicks when game ignored the last one.</summary>
    public const int ClickCooldownMs = 400;
    public const int RemountMaxTries = 5;
    public const int DismountMaxTries = 3;

    // Back-compat aliases for older tests / callers.
    public const int ActivePollMs = ClickCooldownMs;
    public const int ActiveTimeoutMs = 30_000;
    public const int AffectSettleMs = ClickCooldownMs;
    public const int AnimWaitMs = 1500;
    public const int InterSkillWaitMs = ActiveTimeoutMs;
    public const int ClickMaxTries = 64;

    private SkillMountPhase _phase = SkillMountPhase.Idle;
    private readonly List<LearnedSkill> _batch = new();
    private int _index;
    private bool _wasMounted;
    private bool _skipAnimHorse;
    private bool _onClickSent;
    private long _onConfirmedMs;
    private int _dismountTries;
    private int _remountTries;
    private long _phaseAtMs;
    private long _lastToggleMs;
    private long _lastClickMs;
    private bool _moreAfterRemount;

    public SkillMountPhase Phase => _phase;
    public bool Busy => _phase != SkillMountPhase.Idle;
    public LearnedSkill? Skill => Current;
    public IReadOnlyList<LearnedSkill> SessionSkills => _batch;

    private LearnedSkill? Current =>
        _index >= 0 && _index < _batch.Count ? _batch[_index] : null;

    public SkillMountAction Tick(
        LearnedSkill? due,
        bool mounted,
        bool skillAffectPresent,
        bool skillAffectRemoved,
        bool isToggle,
        long nowMs,
        bool toggleWasActive = false,
        bool skipAnimHorse = false) =>
        Tick(
            due == null ? null : new[] { due },
            mounted,
            skillActive: null,
            skillCool: null,
            nowMs,
            skipAnimHorse);

    public SkillMountAction Tick(
        IReadOnlyList<LearnedSkill>? dueBatch,
        bool mounted,
        bool? skillActive,
        long nowMs,
        IReadOnlyList<bool>? toggleWasActive = null,
        bool skipAnimHorse = false) =>
        Tick(dueBatch, mounted, skillActive, skillCool: null, nowMs, skipAnimHorse);

    public SkillMountAction Tick(
        IReadOnlyList<LearnedSkill>? dueBatch,
        bool mounted,
        bool? skillActive,
        bool? skillCool,
        long nowMs,
        bool skipAnimHorse = false)
    {
        return _phase switch
        {
            SkillMountPhase.Idle => OnIdle(dueBatch, mounted, skillActive, skillCool, skipAnimHorse, nowMs),
            SkillMountPhase.Dismounting => OnDismounting(mounted, skillActive, skillCool, nowMs),
            SkillMountPhase.Decide => OnDecide(skillActive, skillCool, nowMs),
            SkillMountPhase.WaitOff => OnWaitOff(skillActive, skillCool, nowMs),
            SkillMountPhase.WaitOn => OnWaitOn(skillActive, skillCool, nowMs),
            SkillMountPhase.Remounting => OnRemounting(mounted, nowMs),
            _ => Action(SkillMountCommand.None)
        };
    }

    public void Reset()
    {
        _phase = SkillMountPhase.Idle;
        ClearBatch();
    }

    private SkillMountAction OnIdle(
        IReadOnlyList<LearnedSkill>? dueBatch,
        bool mounted,
        bool? skillActive,
        bool? skillCool,
        bool skipAnimHorse,
        long nowMs)
    {
        if (dueBatch == null || dueBatch.Count == 0)
            return Action(SkillMountCommand.None);

        _batch.Clear();
        foreach (var skill in dueBatch)
        {
            if (skill == null || skill.Index == 0)
                continue;
            _batch.Add(skill);
        }

        if (_batch.Count == 0)
            return Action(SkillMountCommand.None);

        _index = 0;
        _wasMounted = mounted;
        _skipAnimHorse = skipAnimHorse && mounted;
        _moreAfterRemount = false;
        ResetSkillLatches();
        _dismountTries = 0;
        _remountTries = 0;
        if (mounted)
        {
            _dismountTries = 1;
            return Enter(SkillMountPhase.Dismounting, SkillMountCommand.Dismount, nowMs, stampToggle: true);
        }

        _phase = SkillMountPhase.Decide;
        _phaseAtMs = nowMs;
        return OnDecide(skillActive, skillCool, nowMs);
    }

    private void ResetSkillLatches()
    {
        _onClickSent = false;
        _onConfirmedMs = 0;
        _lastClickMs = 0;
    }

    private SkillMountAction OnDismounting(bool mounted, bool? skillActive, bool? skillCool, long nowMs)
    {
        if (nowMs - _lastToggleMs < DismountWaitMs)
            return Action(SkillMountCommand.None);
        if (!mounted)
        {
            _phase = SkillMountPhase.Decide;
            _phaseAtMs = nowMs;
            return OnDecide(skillActive, skillCool, nowMs);
        }
        if (_dismountTries >= DismountMaxTries)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        _dismountTries++;
        _lastToggleMs = nowMs;
        return Action(SkillMountCommand.Dismount);
    }

    private SkillMountAction OnDecide(bool? skillActive, bool? skillCool, long nowMs)
    {
        // Need a live read before choosing off vs on.
        if (skillActive == null)
            return Action(SkillMountCommand.None);

        if (skillActive == true)
            return ClickOff(nowMs);

        // Already off → go straight to ensure-on (respect cool).
        return EnterApply(skillCool, nowMs);
    }

    private SkillMountAction OnWaitOff(bool? skillActive, bool? skillCool, long nowMs)
    {
        if (skillActive == false)
            return EnterApply(skillCool, nowMs);

        // Still on: one retry click after cooldown when cool allows (or cool unknown).
        if (skillActive == true && CoolAllowsClick(skillCool) && CanRetryClick(nowMs))
            return ClickOff(nowMs);

        return Action(SkillMountCommand.None);
    }

    private SkillMountAction EnterApply(bool? skillCool, long nowMs)
    {
        _onClickSent = false;
        _onConfirmedMs = 0;
        return OnWaitOn(skillActive: false, skillCool, nowMs, enteredFresh: true);
    }

    private SkillMountAction OnWaitOn(bool? skillActive, bool? skillCool, long nowMs, bool enteredFresh = false)
    {
        if (enteredFresh)
            _phase = SkillMountPhase.WaitOn;

        // On-click not sent yet: wait for cool clear, then click once.
        if (!_onClickSent)
        {
            if (!CoolAllowsClick(skillCool))
                return enteredFresh
                    ? Enter(SkillMountPhase.WaitOn, SkillMountCommand.None, nowMs)
                    : Action(SkillMountCommand.None);

            // First entry from Decide/WaitOff may click immediately; retries use cooldown.
            if (_lastClickMs == 0 || CanRetryClick(nowMs) || enteredFresh)
                return ClickOn(nowMs);

            return Action(SkillMountCommand.None);
        }

        // After on-click: only finish when we observe active==true (never on timeout).
        if (skillActive == true)
        {
            if (_onConfirmedMs == 0)
                _onConfirmedMs = nowMs;

            if (_skipAnimHorse && _wasMounted)
            {
                if (nowMs - _onConfirmedMs < SkipAnimWaitMs)
                    return Action(SkillMountCommand.None);
                var done = Current;
                return BeginRemount(more: HasMoreSkills(), nowMs, confirmed: done);
            }

            return FinishCurrentSkill(nowMs);
        }

        // Still off after on-click: retry only after cooldown + cool clear. Never remount.
        if (skillActive == false && CoolAllowsClick(skillCool) && CanRetryClick(nowMs))
            return ClickOn(nowMs);

        return Action(SkillMountCommand.None);
    }

    private SkillMountAction FinishCurrentSkill(long nowMs)
    {
        var done = Current;
        if (HasMoreSkills())
        {
            _index++;
            ResetSkillLatches();
            return Enter(SkillMountPhase.Decide, SkillMountCommand.None, nowMs, confirmed: done);
        }

        if (_wasMounted)
            return BeginRemount(more: false, nowMs, confirmed: done);

        return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs, confirmed: done);
    }

    private SkillMountAction BeginRemount(bool more, long nowMs, LearnedSkill? confirmed = null)
    {
        _moreAfterRemount = more;
        _remountTries = 0;
        return Enter(SkillMountPhase.Remounting, SkillMountCommand.Remount, nowMs, stampToggle: true, confirmed: confirmed);
    }

    private SkillMountAction OnRemounting(bool mounted, long nowMs)
    {
        if (mounted)
        {
            if (_moreAfterRemount)
            {
                _moreAfterRemount = false;
                _index++;
                ResetSkillLatches();
                _dismountTries = 1;
                return Enter(SkillMountPhase.Dismounting, SkillMountCommand.Dismount, nowMs, stampToggle: true);
            }

            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        }

        if (nowMs - _lastToggleMs < RemountWaitMs)
            return Action(SkillMountCommand.None);
        if (_remountTries >= RemountMaxTries)
        {
            if (_moreAfterRemount)
            {
                _moreAfterRemount = false;
                _index++;
                ResetSkillLatches();
                _dismountTries = 0;
                return Enter(SkillMountPhase.Decide, SkillMountCommand.None, nowMs);
            }

            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        }

        _remountTries++;
        _lastToggleMs = nowMs;
        return Action(SkillMountCommand.Remount);
    }

    private bool HasMoreSkills() => _index + 1 < _batch.Count;

    private static bool CoolAllowsClick(bool? skillCool) => skillCool != true;

    private bool CanRetryClick(long nowMs) =>
        nowMs - _lastClickMs >= ClickCooldownMs;

    private SkillMountAction ClickOff(long nowMs)
    {
        _onClickSent = false;
        _onConfirmedMs = 0;
        _lastClickMs = nowMs;
        return Enter(SkillMountPhase.WaitOff, SkillMountCommand.UseSkill, nowMs);
    }

    private SkillMountAction ClickOn(long nowMs)
    {
        _onClickSent = true;
        _onConfirmedMs = 0;
        _lastClickMs = nowMs;
        return Enter(SkillMountPhase.WaitOn, SkillMountCommand.UseSkill, nowMs);
    }

    private SkillMountAction Enter(
        SkillMountPhase phase,
        SkillMountCommand command,
        long nowMs,
        bool stampToggle = false,
        LearnedSkill? confirmed = null)
    {
        var phaseChanged = phase != _phase;
        _phase = phase;
        if (phaseChanged)
            _phaseAtMs = nowMs;
        if (stampToggle)
            _lastToggleMs = nowMs;
        if (phase == SkillMountPhase.Idle)
            ClearBatch();

        return Action(command, confirmed);
    }

    private void ClearBatch()
    {
        _batch.Clear();
        _index = 0;
        _wasMounted = false;
        _skipAnimHorse = false;
        _dismountTries = 0;
        _remountTries = 0;
        _moreAfterRemount = false;
        _lastClickMs = 0;
        _phaseAtMs = 0;
        _lastToggleMs = 0;
        ResetSkillLatches();
    }

    private SkillMountAction Action(SkillMountCommand command, LearnedSkill? confirmed = null) =>
        new(command, Current, _phase, confirmed);
}
