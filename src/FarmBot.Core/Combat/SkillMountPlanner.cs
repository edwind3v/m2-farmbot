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
/// Dismount → cast → optional toggle re-cast → wait → remount until mounted.
/// Remount keeps sending /ride while still unmounted; never give up early.
/// </summary>
public sealed class SkillMountPlanner
{
    public const int DismountWaitMs = 500;
    public const int AnimWaitMs = 1000;
    /// <summary>Gap between remount /ride attempts. Long enough for mount HUD to update
    /// before another /ride (which would toggle the mount back off).</summary>
    public const int RemountWaitMs = 1500;
    public const int AffectSettleMs = 300;
    public const int AffectPollMs = 800;
    public const int RemountMaxTries = 5;
    public const int DismountMaxTries = 3;

    private SkillMountPhase _phase = SkillMountPhase.Idle;
    private LearnedSkill? _skill;
    private bool _wasMounted;
    private bool _isBuff;
    private bool _hadAffect;
    private bool _toggleChecked;
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
        bool isBuff,
        long nowMs)
    {
        return _phase switch
        {
            SkillMountPhase.Idle => OnIdle(due, mounted, skillAffectPresent, isBuff, nowMs),
            SkillMountPhase.Dismounting => OnDismounting(mounted, skillAffectPresent, nowMs),
            SkillMountPhase.WaitToggle => OnWaitToggle(skillAffectPresent, nowMs),
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
        _isBuff = false;
        _hadAffect = false;
        _toggleChecked = false;
        _dismountTries = 0;
        _remountTries = 0;
        _phaseAtMs = 0;
        _lastToggleMs = 0;
        _lastSkillMs = 0;
    }

    private SkillMountAction OnIdle(
        LearnedSkill? due,
        bool mounted,
        bool skillAffectPresent,
        bool isBuff,
        long nowMs)
    {
        if (due == null)
            return Action(SkillMountCommand.None);
        _skill = due;
        _wasMounted = mounted;
        _isBuff = isBuff;
        _hadAffect = skillAffectPresent;
        _toggleChecked = false;
        _dismountTries = 0;
        _remountTries = 0;
        if (mounted)
        {
            _dismountTries = 1;
            return Enter(SkillMountPhase.Dismounting, SkillMountCommand.Dismount, nowMs, stampToggle: true);
        }

        return Cast(skillAffectPresent, nowMs);
    }

    private SkillMountAction OnDismounting(bool mounted, bool skillAffectPresent, long nowMs)
    {
        if (nowMs - _lastToggleMs < DismountWaitMs)
            return Action(SkillMountCommand.None);
        if (!mounted)
            return Cast(skillAffectPresent, nowMs);
        if (_dismountTries >= DismountMaxTries)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        _dismountTries++;
        _lastToggleMs = nowMs;
        return Action(SkillMountCommand.Dismount);
    }

    private SkillMountAction OnWaitToggle(bool skillAffectPresent, long nowMs)
    {
        var elapsed = nowMs - _lastSkillMs;
        if (elapsed < AffectSettleMs)
            return Action(SkillMountCommand.None);

        if (!_toggleChecked)
        {
            if (_hadAffect && !skillAffectPresent)
            {
                _toggleChecked = true;
                _lastSkillMs = nowMs;
                return Enter(SkillMountPhase.WaitAnim, SkillMountCommand.UseSkill, nowMs);
            }

            if (elapsed < AffectPollMs)
            {
                if (_hadAffect && skillAffectPresent)
                    return Action(SkillMountCommand.None);
                if (_isBuff && !skillAffectPresent)
                    return Action(SkillMountCommand.None);
                _toggleChecked = true;
            }
            else if (_isBuff && !skillAffectPresent)
            {
                _toggleChecked = true;
                _lastSkillMs = nowMs;
                return Enter(SkillMountPhase.WaitAnim, SkillMountCommand.UseSkill, nowMs);
            }
            else
            {
                _toggleChecked = true;
            }
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
            // First remount attempt immediately; keep retrying in OnRemounting until mounted.
            _remountTries = 0;
            return Enter(SkillMountPhase.Remounting, SkillMountCommand.Remount, nowMs, stampToggle: true);
        }

        return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
    }

    private SkillMountAction OnRemounting(bool mounted, long nowMs)
    {
        // Done only when HUD says mounted — keep retrying otherwise.
        if (mounted)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);
        if (nowMs - _lastToggleMs < RemountWaitMs)
            return Action(SkillMountCommand.None);
        if (_remountTries >= RemountMaxTries)
            return Enter(SkillMountPhase.Idle, SkillMountCommand.None, nowMs);

        _remountTries++;
        _lastToggleMs = nowMs;
        // First Remount was already sent when entering this phase (tries was 0 → Enter stamped).
        // Subsequent ticks send again while still unmounted.
        return Action(SkillMountCommand.Remount);
    }

    private SkillMountAction Cast(bool skillAffectPresent, long nowMs)
    {
        _hadAffect = skillAffectPresent;
        _lastSkillMs = nowMs;
        _toggleChecked = false;
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
            _isBuff = false;
            _hadAffect = false;
            _toggleChecked = false;
            _dismountTries = 0;
            _remountTries = 0;
        }

        return Action(command);
    }

    private SkillMountAction Action(SkillMountCommand command) =>
        new(command, _skill, _phase);
}
