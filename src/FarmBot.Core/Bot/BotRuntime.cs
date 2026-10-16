using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Geometry;
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
    int PurpleRemain = 0,
    IReadOnlyList<LearnedSkill>? Skills = null,
    string GamePhase = "");

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
    private readonly SkillController _skills = new();
    private readonly SkillMountPlanner _skillMount = new();
    private bool _mounted;
    private IReadOnlyList<int> _affects = Array.Empty<int>();
    private IReadOnlyList<int> _castRemoved = Array.Empty<int>();
    private IReadOnlyList<PotionStack> _potionStacks = Array.Empty<PotionStack>();
    private IReadOnlyList<LearnedSkill> _learnedSkills = Array.Empty<LearnedSkill>();
    private int _greenRemain;
    private int _purpleRemain;
    private long _potsAtMs;
    private bool _potsInGame;
    private bool _potsSeenLive;
    private int _potsEmptyTries;
    private long _skillsAtMs;
    private bool _skillsInGame;
    private bool _skillsSeenLive;
    private int _skillsEmptyTries;
    private readonly string _cacheRoot;
    private string _account;
    private string _character;
    private string _kitFingerprint = "";
    private bool _inWorld;
    private string _gamePhase = "";
    private string _phaseLog = "";
    private string _phaseKey = "";
    private long _phaseLogAtMs;
    private CombatState _loggedCombatState = CombatState.Idle;
    private uint _loggedCombatVid;
    private long _entitiesAtMs;
    private int _lastNativeActorCount = -1;
    private IReadOnlyList<EntityState> _lastNativeActors = Array.Empty<EntityState>();
    private float _lastOriginX;
    private float _lastOriginY;
    private float _lastOriginZ;

    public BotRuntime(
        IGameStateSource source,
        IGameControl control,
        IBotLog log,
        Func<BotConfig> configProvider,
        Func<bool>? cornerProbe = null,
        bool nativeCombat = false,
        INativeActionSink? sink = null,
        string? cacheRoot = null,
        string account = "",
        string character = "")
    {
        _source = source;
        _control = control;
        _log = log;
        _configProvider = configProvider;
        _cornerProbe = cornerProbe ?? (() => false);
        _nativeCombat = nativeCombat;
        _sink = sink;
        _cacheRoot = cacheRoot ?? "";
        _account = account ?? "";
        _character = character ?? "";
        LoadKit();
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
            return new BotView(_enabled, _phase, _reason, _latest, _target, _combatState, _nativeInterface, _rangedOnly, _potionStacks, _greenRemain, _purpleRemain, _learnedSkills, _gamePhase);
    }

    public void TickOnce(BotConfig config, long nowMs, int dtMs)
    {
        config.Sanitize();
        if (_source is IAdvancingSource advancing)
            advancing.Advance(dtMs);

        var snapshot = OverlayHud(_source.GetSnapshot(nowMs), config, nowMs);
        MaybeUpdateIdentity(snapshot);
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
        TickSkills(snapshot, config, nowMs);
        if (_skillMount.Busy && _combat.TargetVid != 0 && !_combat.AttacksPaused)
            _combat.PauseAttacks();
        if (!_skillMount.Busy && _combat.AttacksPaused)
            _combat.ResumeAttacks();
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
        var vid = tick.Target?.Vid ?? 0;
        if (tick.Action != null && (tick.State != _loggedCombatState || vid != _loggedCombatVid))
        {
            _loggedCombatState = tick.State;
            _loggedCombatVid = vid;
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
        if (config.ClientProfile == ClientProfile.Baerim)
            delay = Math.Max(400, delay);
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
        CombatState.Sidestep => BotPhase.Moving,
        _ => BotPhase.Idle
    };

    public void Dispose()
    {
        PersistKit();
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
            {
                var pickupWait = config.FastPickupEnabled ? config.FastPickupDelayMs : 50;
                if (config.ClientProfile == ClientProfile.Baerim)
                    pickupWait = Math.Max(400, pickupWait);
                wait = Math.Max(1, Math.Min(wait, pickupWait));
            }
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
        if ((config.ClientProfile != ClientProfile.Arthion && config.ClientProfile != ClientProfile.Baerim) ||
            _sink == null)
            return;
        ClientIconStore.ClientPath = config.ClientPath ?? "";
        if (!_inWorld)
        {
            if (_potsInGame)
            {
                _potsInGame = false;
                _potsSeenLive = false;
                _potsEmptyTries = 0;
                _potsAtMs = 0;
                PersistKit();
            }
            return;
        }

        var interval = !_potsInGame ? 0 : CharacterKitStore.PotionPollMs;
        if (nowMs - _potsAtMs >= interval && _sink.TryReadPots(out var scan))
        {
            var first = !_potsInGame;
            _potsAtMs = nowMs;
            _potsInGame = true;
            IReadOnlyList<PotionStack> previous;
            lock (_gate)
                previous = _potionStacks;
            if (scan.Pots.Count > 0)
            {
                _potsEmptyTries = 0;
                _potsSeenLive = true;
                var merged = CharacterKitStore.MergePots(scan.Pots, previous);
                lock (_gate)
                    _potionStacks = merged;
                if (first)
                    _log.Info($"pots loaded {merged.Count} stacks");
            }
            else if (_potsSeenLive || _potsEmptyTries++ >= 8)
            {
                _potsSeenLive = true;
                lock (_gate)
                    _potionStacks = Array.Empty<PotionStack>();
            }
            else if (first)
                _log.Info("pots scan empty, will retry after stats");

            lock (_gate)
            {
                _greenRemain = scan.GreenRemain;
                _purpleRemain = scan.PurpleRemain;
            }
            _potions.NoteNativeRemain(scan.GreenRemain, scan.PurpleRemain, nowMs);
            PersistKit();
        }

        if (!_potsSeenLive)
            return;
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

    private void TickSkills(WorldSnapshot? snapshot, BotConfig config, long nowMs)
    {
        if ((config.ClientProfile != ClientProfile.Arthion && config.ClientProfile != ClientProfile.Baerim) ||
            _sink == null)
            return;
        if (!_inWorld)
        {
            if (_skillsInGame)
            {
                _skillsInGame = false;
                _skillsSeenLive = false;
                _skillsEmptyTries = 0;
                _skillsAtMs = 0;
                PersistKit();
            }
            _affects = Array.Empty<int>();
            _castRemoved = Array.Empty<int>();
            if (_skillMount.Busy)
                _skillMount.Reset();
            return;
        }

        // Always refresh from the live client (kit cache alone had IsToggle/IsActive=false forever).
        var interval = !_skillsInGame ? 0 : CharacterKitStore.SkillPollMs;
        if (nowMs - _skillsAtMs >= interval && _sink.TryReadSkills(out var scan))
        {
            var first = !_skillsInGame;
            _skillsAtMs = nowMs;
            _skillsInGame = true;
            IReadOnlyList<LearnedSkill> previous;
            lock (_gate)
                previous = _learnedSkills;
            if (scan.Skills.Count > 0)
            {
                _skillsEmptyTries = 0;
                _skillsSeenLive = true;
                var merged = CharacterKitStore.MergeSkills(scan.Skills, previous);
                lock (_gate)
                    _learnedSkills = merged;
                if (first)
                {
                    var toggles = merged.Count(s => s.IsToggle);
                    var lit = merged.Count(s => s.IsActive);
                    _log.Info($"skills loaded {merged.Count} toggles={toggles} active={lit}");
                }
            }
            else if (_skillsSeenLive || _skillsEmptyTries++ >= 8)
            {
                _skillsSeenLive = true;
                if (previous.Count == 0)
                {
                    lock (_gate)
                        _learnedSkills = Array.Empty<LearnedSkill>();
                }
            }
            else if (first)
                _log.Info("skills scan empty, will retry after stats");

            IReadOnlyList<LearnedSkill> noted;
            lock (_gate)
                noted = _learnedSkills;
            _skills.NoteAffectRemain(noted, nowMs);
            PersistKit();
        }

        bool active;
        IReadOnlyList<LearnedSkill> skills;
        lock (_gate)
        {
            active = _enabled;
            skills = _learnedSkills;
        }
        if (!active || !_skillsSeenLive)
        {
            if (_skillMount.Busy)
                _skillMount.Reset();
            return;
        }

        var hasTarget = _combat.TargetVid != 0;
        if (_skillMount.Busy)
        {
            DriveSkillMount(null, config, nowMs);
            return;
        }

        if (hasTarget && !config.SkillsDuringCombat)
            return;
        var due = _skills.Peek(skills, config, hasTarget, nowMs);
        if (due == null)
            return;
        var buff = config.SkillBuffIndexes != null && config.SkillBuffIndexes.Contains(due.Index);
        if (!buff && !hasTarget)
            return;
        DriveSkillMount(due, config, nowMs);
        if (_skillMount.Busy)
            _skills.NoteUsed(due, nowMs);
    }

    private void DriveSkillMount(LearnedSkill? due, BotConfig config, long nowMs)
    {
        if (_sink == null)
            return;
        var skill = due ?? _skillMount.Skill;
        var skillIndex = skill?.Index ?? 0;
        var hasAffect = skillIndex != 0 && _affects.Contains((int)skillIndex);
        var isBuff = skillIndex != 0
            && config.SkillBuffIndexes != null
            && config.SkillBuffIndexes.Contains(skillIndex);
        // Recast decision: buff slot currently highlighted (IsSkillActive). Do not trust
        // IsToggleSkill from kit cache — it was stuck false and skipped the second click.
        var toggleWasActive = false;
        if (due != null && !_skillMount.Busy && isBuff)
        {
            if (_sink.TryIsSkillActive(due.Slot, out var active))
                toggleWasActive = active;
            else
                toggleWasActive = due.IsActive || due.IsToggle;
            _log.Info($"skill prep index={due.Index} slot={due.Slot} buff=1 toggleFlag={due.IsToggle} highlight={toggleWasActive}");
        }
        var affectRemoved = isBuff && _castRemoved.Count > 0
            || skillIndex != 0 && _castRemoved.Contains((int)skillIndex);
        // Planner "isToggle" here means "may need second click" = buff with highlight on.
        var action = _skillMount.Tick(due, _mounted, hasAffect, affectRemoved, isToggle: isBuff, nowMs, toggleWasActive);

        switch (action.Command)
        {
            case SkillMountCommand.Dismount:
                if (_sink.TryToggleMount(wantMounted: false))
                    _log.Info("dismount for skill");
                else
                    _log.Warn("dismount failed");
                break;
            case SkillMountCommand.Remount:
                if (_sink.TryToggleMount(wantMounted: true))
                    _log.Info($"remount after skill (mounted={_mounted} phase={action.Phase})");
                else
                    _log.Warn("remount failed");
                break;
            case SkillMountCommand.UseSkill:
                if (action.Skill == null)
                    break;
                var buff = config.SkillBuffIndexes != null && config.SkillBuffIndexes.Contains(action.Skill.Index);
                if (_sink.TryUseSkill(action.Skill.Slot, 0))
                    _log.Info($"use skill slot={action.Skill.Slot} index={action.Skill.Index} buff={buff} highlight={toggleWasActive} mounted={_mounted} phase={action.Phase}");
                else
                    _log.Warn($"use skill failed slot={action.Skill.Slot} index={action.Skill.Index}");
                break;
        }
    }

    private void LoadKit()
    {
        if (string.IsNullOrWhiteSpace(_cacheRoot))
            return;
        var path = CharacterKitStore.FilePath(_cacheRoot, _account, _character);
        if (!CharacterKitStore.TryLoad(path, out var kit))
            return;
        lock (_gate)
        {
            if (kit.Pots.Count > 0)
                _potionStacks = kit.Pots;
            if (kit.Skills.Count > 0)
                _learnedSkills = kit.Skills;
        }

        _kitFingerprint = CharacterKitStore.Fingerprint(kit.Pots, kit.Skills);
        _log.Info($"kit cache loaded {_account}/{_character} pots={kit.Pots.Count} skills={kit.Skills.Count}");
    }

    private void MaybeUpdateIdentity(WorldSnapshot? snapshot)
    {
        if (string.IsNullOrWhiteSpace(_cacheRoot))
            return;
        var name = snapshot?.Player.Name?.Trim() ?? "";
        if (name.Length == 0 || string.Equals(_character, name, StringComparison.Ordinal))
            return;
        _character = name;
        if (_potsSeenLive || _skillsSeenLive)
        {
            _kitFingerprint = "";
            PersistKit();
            return;
        }

        var path = CharacterKitStore.FilePath(_cacheRoot, _account, _character);
        if (CharacterKitStore.TryLoad(path, out var kit) && (kit.Pots.Count > 0 || kit.Skills.Count > 0))
        {
            lock (_gate)
            {
                if (!_potsSeenLive && kit.Pots.Count > 0)
                    _potionStacks = kit.Pots;
                if (!_skillsSeenLive && kit.Skills.Count > 0)
                    _learnedSkills = kit.Skills;
            }

            _kitFingerprint = CharacterKitStore.Fingerprint(kit.Pots, kit.Skills);
            _log.Info($"kit cache loaded {_account}/{_character} pots={kit.Pots.Count} skills={kit.Skills.Count}");
            return;
        }

        _kitFingerprint = "";
        PersistKit();
    }

    private void PersistKit()
    {
        if (string.IsNullOrWhiteSpace(_cacheRoot))
            return;
        IReadOnlyList<PotionStack> pots;
        IReadOnlyList<LearnedSkill> skills;
        string account;
        string character;
        bool seen;
        lock (_gate)
        {
            pots = _potionStacks;
            skills = _learnedSkills;
            account = _account;
            character = _character;
            seen = _potsSeenLive || _skillsSeenLive;
        }

        if (pots.Count == 0 && skills.Count == 0 && !seen)
            return;
        var fingerprint = CharacterKitStore.Fingerprint(pots, skills);
        lock (_gate)
        {
            if (fingerprint == _kitFingerprint)
                return;
        }

        try
        {
            var path = CharacterKitStore.FilePath(_cacheRoot, account, character);
            CharacterKitStore.Save(path, new CharacterKit
            {
                Account = account,
                Character = character,
                Pots = pots.Select(CharacterKitStore.CopyPot).ToList(),
                Skills = skills.Select(CharacterKitStore.ForCache).ToList()
            });
            lock (_gate)
                _kitFingerprint = fingerprint;
        }
        catch (Exception ex)
        {
            _log.Warn($"kit cache save failed: {ex.Message}");
        }
    }

    private WorldSnapshot? OverlayHud(WorldSnapshot? snapshot, BotConfig config, long nowMs)
    {
        if (snapshot == null || _sink == null)
            return snapshot;
        if (!_sink.TryReadHud(out var hud))
        {
            NoteGamePhase(null, nowMs);
            return snapshot;
        }

        _inWorld = hud.InWorld;
        _mounted = hud.Mounted;
        if (hud.Affects != null)
            _affects = hud.Affects;
        if (hud.CastRemoved != null)
            _castRemoved = hud.CastRemoved;
        NoteGamePhase(hud, nowMs);
        if (!_inWorld || hud.MaxHp <= 0)
            return snapshot;
        snapshot = snapshot with
        {
            Player = snapshot.Player with
            {
                Level = hud.Level,
                Exp = hud.Exp,
                NextExp = hud.NextExp,
                Hp = hud.Hp,
                MaxHp = hud.MaxHp,
                Mp = hud.Mp,
                MaxMp = hud.MaxMp,
                Race = hud.Race ?? snapshot.Player.Race
            }
        };
        if (config.ClientProfile != ClientProfile.Baerim)
            return snapshot;
        if (nowMs - _entitiesAtMs < 800 && _lastNativeActors.Count > 0)
        {
            return snapshot with
            {
                Player = snapshot.Player with { X = _lastOriginX, Y = _lastOriginY, Z = _lastOriginZ },
                Entities = _lastNativeActors
            };
        }
        _entitiesAtMs = nowMs;
        if (!_sink.TryReadEntities(out var actors) || actors.Count == 0)
        {
            if (_lastNativeActors.Count > 0)
            {
                return snapshot with
                {
                    Player = snapshot.Player with { X = _lastOriginX, Y = _lastOriginY, Z = _lastOriginZ },
                    Entities = _lastNativeActors
                };
            }
            if (_lastNativeActorCount != 0)
            {
                _lastNativeActorCount = 0;
                _log.Info($"native entities 0 map actors character={hud.Character}");
            }
            return snapshot;
        }
        var me = actors.FirstOrDefault(item =>
                     item.Type == ActorType.Player &&
                     !string.IsNullOrWhiteSpace(hud.Character) &&
                     string.Equals(item.Name, hud.Character, StringComparison.OrdinalIgnoreCase))
                 ?? actors.FirstOrDefault(item => item.Type == ActorType.Player);
        var originX = me?.X ?? snapshot.Player.X;
        var originY = me?.Y ?? snapshot.Player.Y;
        var originZ = me?.Z ?? snapshot.Player.Z;
        var withDistance = actors
            .Select(item => item with
            {
                Distance = WorldGeometry.Distance(originX, originY, item.X, item.Y)
            })
            .ToList();
        _lastNativeActors = withDistance;
        _lastOriginX = originX;
        _lastOriginY = originY;
        _lastOriginZ = originZ;
        if (_lastNativeActorCount != withDistance.Count)
        {
            _lastNativeActorCount = withDistance.Count;
            _log.Info($"native entities {withDistance.Count} map actors character={hud.Character}");
        }

        return snapshot with
        {
            Player = snapshot.Player with { X = originX, Y = originY, Z = originZ },
            Entities = withDistance
        };
    }

    private void NoteGamePhase(PlayerHudPoints? hud, long nowMs)
    {
        string phase;
        string line;
        string inWorld;
        string character;
        if (hud == null)
        {
            phase = "(unread)";
            inWorld = "no";
            character = "-";
            line = "game phase=(unread) inWorld=no pots=off skills=off hud=no";
        }
        else
        {
            phase = string.IsNullOrWhiteSpace(hud.Value.Phase) ? "(empty)" : hud.Value.Phase.Trim();
            inWorld = hud.Value.InWorld ? "yes" : "no";
            character = string.IsNullOrWhiteSpace(hud.Value.Character) ? "-" : hud.Value.Character.Trim();
            var reason = string.IsNullOrWhiteSpace(hud.Value.Reason) ? "-" : hud.Value.Reason.Trim();
            var hudText = hud.Value.MaxHp > 0
                ? $"hp {hud.Value.Hp}/{hud.Value.MaxHp} lv {hud.Value.Level}"
                : "no";
            line =
                $"game phase={phase} inWorld={inWorld} startGame={(hud.Value.StartGame ? "yes" : "no")} pots={(hud.Value.InWorld ? "on" : "off")} skills={(hud.Value.InWorld ? "on" : "off")} hud={hudText} character={character} reason={reason}";
        }

        _gamePhase = phase;
        var key = $"{phase}|{inWorld}|{character}";
        if (string.Equals(key, _phaseKey, StringComparison.Ordinal) && nowMs - _phaseLogAtMs < 5000)
            return;
        _phaseKey = key;
        _phaseLog = line;
        _phaseLogAtMs = nowMs;
        _log.Info(line);
    }
}
