namespace FarmBot.Core.Combat;

public enum SkillMountPhase
{
    Idle,
    Dismounting,
    WaitToggle,
    WaitAnim,
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
    SkillMountPhase Phase);

/// <summary>
/// Dismount → cast → (toggle that was already on: cast again) → remount.
/// Double-click only when IsToggleSkill and IsSkillActive (highlight) at sequence start.
/// </summary>
public sealed class SkillMountPlanner
{
    public const int DismountWaitMs = 500;
    public const int AnimWaitMs = 1000;
    public const int RemountWaitMs = 1500;
    public const int AffectSettleMs = 700;
    public const int RemountMaxTries = 5;
    public const int DismountMaxTries = 3;

    private SkillMountPhase _phase = SkillMountPhase.Idle;
    private LearnedSkill? _skill;
    private bool _wasMounted;
    private bool _needsToggleRecast;
    private bool _didToggleRecast;
    private int _dismountTries;
    private int _remountTries;
    private long _phaseAtMs;
    private long _lastToggleMs;
    private long _lastSkillMs;

    public SkillMountPhase Phase => _phase;
    public bool Busy => _phase != SkillMountPhase.Idle;
    public LearnedSkill? Skill => _skill;

    public SkillMountAction Tick(
        LearnedSkill? due,
        bool mounted,
        bool skillAffectPresent,
        bool skillAffectRemoved,
        bool isToggle,
        long nowMs,
        bool toggleWasActive = false)
    {
        _ = skillAffectPresent;
        _ = skillAffectRemoved;
        return _phase switch
        {
            SkillMountPhase.Idle => OnIdle(due, mounted, isToggle, toggleWasActive, nowMs),
            SkillMountPhase.Dismounting => OnDismounting(mounted, nowMs),
            SkillMountPhase.WaitToggle => OnWaitToggle(nowMs),
            SkillMountPhase.WaitAnim => OnWaitAnim(nowMs),
            SkillMountPhase.Remounting => OnRemounting(mounted, nowMs),
            _ => Action(SkillMountCommand.None)
        };
    }

    public void Reset()
    {
        _phase = SkillMountPhase.Idle;
        _skill = null;
        _wasMounted = false;
        _needsToggleRecast = false;
        _didToggleRecast = false;
        _dismountTries = 0;
        _remountTries = 0;
        _phaseAtMs = 0;
        _lastToggleMs = 0;
        _lastSkillMs = 0;
    }

    private SkillMountAction OnIdle(
        LearnedSkill? due,
        bool mounted,
        bool isToggle,
        bool toggleWasActive,
        long nowMs)
    {
        if (due == null)
            return Action(SkillMountCommand.None);
        _skill = due;
        _wasMounted = mounted;
        // Only re-click if toggle was already highlighted (on → off → on).
        _needsToggleRecast = isToggle && toggleWasActive;
        _didToggleRecast = false;
        _dismountTries = 0;
        _remountTries = 0;
        if (mounted)
        {
            _dismountTries = 1;
            return Enter(SkillMountPhase.Dismounting, SkillMountCommand.Dismount, nowMs, stampToggle: true);
        }

        return Cast(nowMs);
    }

    private SkillMountAction OnDismounting(bool mounted, long nowMs)
    {
        if (nowMs - _lastToggleMs < DismountWaitMs)
            return Action(SkillMountCommand.None);
        if (!mounted)
            return Cast(nowMs);
        if (_dismountTries >= DismountMaxTries)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        _dismountTries++;
        _lastToggleMs = nowMs;
        return Action(SkillMountCommand.Dismount);
    }

    private SkillMountAction OnWaitToggle(long nowMs)
    {
        if (nowMs - _lastSkillMs < AffectSettleMs)
            return Action(SkillMountCommand.None);

        if (_needsToggleRecast && !_didToggleRecast)
        {
            _didToggleRecast = true;
            _lastSkillMs = nowMs;
            return Enter(SkillMountPhase.WaitAnim, SkillMountCommand.UseSkill, nowMs);
        }

        return _wasMounted
            ? Enter(SkillMountPhase.WaitAnim, SkillMountCommand.None, nowMs)
            : Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
    }

    private SkillMountAction OnWaitAnim(long nowMs)
    {
        if (nowMs - _lastSkillMs < AnimWaitMs)
            return Action(SkillMountCommand.None);
        if (_wasMounted)
        {
            _remountTries = 0;
            return Enter(SkillMountPhase.Remounting, SkillMountCommand.Remount, nowMs, stampToggle: true);
        }

        return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
    }

    private SkillMountAction OnRemounting(bool mounted, long nowMs)
    {
        if (mounted)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        if (nowMs - _lastToggleMs < RemountWaitMs)
            return Action(SkillMountCommand.None);
        if (_remountTries >= RemountMaxTries)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);

        _remountTries++;
        _lastToggleMs = nowMs;
        return Action(SkillMountCommand.Remount);
    }

    private SkillMountAction Cast(long nowMs)
    {
        _lastSkillMs = nowMs;
        return Enter(SkillMountPhase.WaitToggle, SkillMountCommand.UseSkill, nowMs);
    }

    private SkillMountAction Enter(
        SkillMountPhase phase,
        SkillMountCommand command,
        long nowMs,
        bool stampToggle = false)
    {
        _phase = phase;
        _phaseAtMs = nowMs;
        if (stampToggle)
            _lastToggleMs = nowMs;
        if (phase == SkillMountPhase.Idle)
        {
            _skill = null;
            _wasMounted = false;
            _needsToggleRecast = false;
            _didToggleRecast = false;
            _dismountTries = 0;
            _remountTries = 0;
        }

        return Action(command);
    }

    private SkillMountAction Action(SkillMountCommand command) =>
        new(command, _skill, _phase);
}
