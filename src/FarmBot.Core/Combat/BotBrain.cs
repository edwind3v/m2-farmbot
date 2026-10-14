using FarmBot.Core.Configuration;
using FarmBot.Core.Looting;
using FarmBot.Core.Models;
using FarmBot.Core.Navigation;
using FarmBot.Core.Safety;

namespace FarmBot.Core.Combat;

public sealed class BotBrain
{
    private readonly SkillRotation _skills = new();
    private uint _sticky;
    private long _lastAttackMs;
    private long _lastMoveMs;
    private long _lastHealMs;
    private long _lastPickupMs;
    private bool _hasAttacked;
    private bool _hasMoved;
    private bool _hasHealed;
    private bool _hasPicked;
    private bool _failsafeLatched;
    private uint _rangedVid;
    private long _lastRangedMs;

    public void Reset()
    {
        _skills.Reset();
        _sticky = 0;
        _lastAttackMs = 0;
        _lastMoveMs = 0;
        _lastHealMs = 0;
        _lastPickupMs = 0;
        _hasAttacked = false;
        _hasMoved = false;
        _hasHealed = false;
        _hasPicked = false;
        _failsafeLatched = false;
        _rangedVid = 0;
        _lastRangedMs = 0;
    }

    public BrainDecision Tick(WorldSnapshot? snapshot, BotConfig config, TickContext context, long nowMs)
    {
        if (!context.Enabled)
            return new BrainDecision(BotPhase.Stopped, null, null, "stopped");

        var trip = FailsafeMonitor.Reason(snapshot, config, context, nowMs);
        if (trip != null)
        {
            if (_failsafeLatched)
                return new BrainDecision(BotPhase.Failsafe, null, null, trip);
            _failsafeLatched = true;
            return new BrainDecision(BotPhase.Failsafe, null, BotCommand.Stop(), trip);
        }

        if (snapshot == null)
            return new BrainDecision(BotPhase.WaitingForClient, null, null, "waiting for client state");

        var player = snapshot.Player;
        if (config.HealEnabled &&
            player.MaxHp > 0 &&
            player.HpPercent < config.HealHpPercent &&
            !string.IsNullOrWhiteSpace(config.HealKey) &&
            (!_hasHealed || nowMs - _lastHealMs >= config.HealCooldownMs))
        {
            _hasHealed = true;
            _lastHealMs = nowMs;
            return new BrainDecision(BotPhase.Attacking, null, BotCommand.Heal(config.HealKey), "heal");
        }

        var stickyAlive = snapshot.Entities.Any(entity =>
            entity.Vid == _sticky && entity.Hp > 0 && entity.IsCombatant);
        if (!stickyAlive)
            _sticky = 0;

        if (config.LootEnabled && config.LootBeforeNextTarget && _sticky == 0)
        {
            var drop = LootPolicy.NextDrop(snapshot, config);
            if (drop != null)
            {
                if (!_hasPicked || nowMs - _lastPickupMs >= config.PickupIntervalMs)
                {
                    _hasPicked = true;
                    _lastPickupMs = nowMs;
                    return new BrainDecision(BotPhase.Looting, drop, BotCommand.Pickup(drop, config.PickupKey), "loot");
                }

                return new BrainDecision(BotPhase.Looting, drop, null, "loot cooldown");
            }
        }

        if (config.RangedAttackEnabled)
            return TickRanged(snapshot, config, nowMs);

        var target = TargetSelector.Select(snapshot, config, _sticky);
        if (target != null)
        {
            _sticky = target.Vid;
            if (Navigator.ShouldMove(target.Distance, config))
            {
                if (!_hasMoved || nowMs - _lastMoveMs >= config.MoveIntervalMs)
                {
                    _hasMoved = true;
                    _lastMoveMs = nowMs;
                    return new BrainDecision(BotPhase.Moving, target, BotCommand.Move(target.X, target.Y), "approach");
                }

                return new BrainDecision(BotPhase.Moving, target, null, "approach cooldown");
            }

            var skill = _skills.Select(player, target, config.Skills, nowMs);
            if (skill != null)
            {
                _skills.Mark(skill.Slot, nowMs);
                return new BrainDecision(
                    BotPhase.Attacking,
                    target,
                    BotCommand.Skill(target, skill.Slot, skill.Key, skill.MpCost),
                    "skill");
            }

            if (!_hasAttacked || nowMs - _lastAttackMs >= config.AttackIntervalMs)
            {
                _hasAttacked = true;
                _lastAttackMs = nowMs;
                return new BrainDecision(BotPhase.Attacking, target, BotCommand.Attack(target, config.AttackKey), "attack");
            }

            return new BrainDecision(BotPhase.Attacking, target, null, "attack cooldown");
        }

        if (Navigator.OutsideLeash(player, config))
        {
            if (!_hasMoved || nowMs - _lastMoveMs >= config.MoveIntervalMs)
            {
                _hasMoved = true;
                _lastMoveMs = nowMs;
                return new BrainDecision(
                    BotPhase.Returning,
                    null,
                    BotCommand.Move(config.FarmCenterX, config.FarmCenterY),
                    "return to farm center");
            }

            return new BrainDecision(BotPhase.Returning, null, null, "return cooldown");
        }

        return new BrainDecision(BotPhase.Idle, null, null, "no targets in farm area");
    }

    private BrainDecision TickRanged(WorldSnapshot snapshot, BotConfig config, long nowMs)
    {
        var picked = TargetSelector.SelectMany(snapshot, config, config.RangedAttackMaxTargets, config.RangedAttackRadius);
        if (picked.Count == 0)
        {
            _rangedVid = 0;
            if (Navigator.OutsideLeash(snapshot.Player, config))
            {
                if (!_hasMoved || nowMs - _lastMoveMs >= config.MoveIntervalMs)
                {
                    _hasMoved = true;
                    _lastMoveMs = nowMs;
                    return new BrainDecision(
                        BotPhase.Returning,
                        null,
                        BotCommand.Move(config.FarmCenterX, config.FarmCenterY),
                        "return to farm center");
                }

                return new BrainDecision(BotPhase.Returning, null, null, "return cooldown");
            }

            return new BrainDecision(BotPhase.Idle, null, null, "no farmable targets in ranged radius");
        }

        EntityState? current = null;
        for (var i = 0; i < picked.Count; i++)
        {
            if (picked[i].Vid != _rangedVid)
                continue;
            current = picked[i];
            break;
        }

        if (_lastRangedMs != 0 && nowMs - _lastRangedMs < config.RangedAttackDelayMs)
            return new BrainDecision(BotPhase.Attacking, current ?? picked[0], null, "ranged delay");

        var target = current ?? picked[0];
        _rangedVid = target.Vid;
        _lastRangedMs = nowMs;
        return new BrainDecision(
            BotPhase.Attacking,
            target,
            BotCommand.Shot(picked, snapshot.Player.X, snapshot.Player.Y),
            $"ranged blast {picked.Count}");
    }
}
