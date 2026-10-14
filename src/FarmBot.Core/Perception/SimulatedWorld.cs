using FarmBot.Core.Entities;
using FarmBot.Core.Geometry;
using FarmBot.Core.Input;
using FarmBot.Core.Models;

namespace FarmBot.Core.Perception;

public sealed class SimulatedWorld : IGameStateSource, IGameStateProvider, IGameControl, IAdvancingSource
{
    private sealed class Mob
    {
        public uint Vid;
        public string Name = "";
        public EntityKind Kind;
        public ActorType Type;
        public uint Vnum;
        public int Hp;
        public int MaxHp;
        public float X;
        public float Y;
        public float Z;
    }

    private readonly List<Mob> _mobs = new();
    private readonly object _gate = new();
    private WorldSnapshot _snapshot = new();
    private uint _nextVid = 1000;
    private float _x;
    private float _y;
    private float _destX;
    private float _destY;
    private float _facingDeg;
    private bool _moving;
    private int _hp = 800;
    private int _maxHp = 1000;
    private int _mp = 120;
    private int _maxMp = 200;
    private uint _targetVid;
    private const float Speed = 600f;

    public string Name => "Simulator";
    public string Status => "simulator — representative entities, not the live client";
    public bool FromLiveClient => false;

    public SimulatedWorld() : this(representative: true)
    {
    }

    public static SimulatedWorld Classic() => new(representative: false);

    public SimulatedWorld(bool representative)
    {
        _mobs.Add(new Mob { Vid = 1, Name = "Wild Dog", Kind = EntityKind.Monster, Type = ActorType.Enemy, Vnum = 101, Hp = 120, MaxHp = 120, X = 180, Y = 40 });
        _mobs.Add(new Mob { Vid = 2, Name = "Metin of Combat", Kind = EntityKind.Metin, Type = ActorType.Stone, Vnum = 8001, Hp = 100, MaxHp = 400, X = 260, Y = -20 });
        _mobs.Add(new Mob { Vid = 3, Name = "Hungry Wolf", Kind = EntityKind.Monster, Type = ActorType.Enemy, Vnum = 102, Hp = 90, MaxHp = 90, X = 2500, Y = 2500 });
        if (representative)
        {
            _mobs.Add(new Mob { Vid = 4, Name = "Weapon Shop", Kind = EntityKind.Npc, Type = ActorType.Npc, Vnum = 9001, Hp = 1, MaxHp = 1, X = 90, Y = -80 });
            _mobs.Add(new Mob { Vid = 5, Name = "OtherPlayer", Kind = EntityKind.Player, Type = ActorType.Player, Vnum = 0, Hp = 800, MaxHp = 800, X = 140, Y = 120 });
            _mobs.Add(new Mob { Vid = 6, Name = "Young Tiger", Kind = EntityKind.Monster, Type = ActorType.Enemy, Vnum = 103, Hp = 150, MaxHp = 150, X = 420, Y = 60 });
            _mobs.Add(new Mob { Vid = 7, Name = "Metin of Greed", Kind = EntityKind.Metin, Type = ActorType.Stone, Vnum = 8002, Hp = 200, MaxHp = 200, X = 520, Y = -40 });
            _mobs.Add(new Mob { Vid = 8, Name = "Alpha Wolf", Kind = EntityKind.Boss, Type = ActorType.Enemy, Vnum = 1091, Hp = 800, MaxHp = 800, X = 300, Y = 90 });
        }

        Rebuild();
    }

    public GameFrame? Capture(long nowMs)
    {
        var snapshot = GetSnapshot(nowMs);
        return snapshot == null ? null : EntityMapping.ToFrame(snapshot, Name, FromLiveClient);
    }

    public void Advance(int dtMs)
    {
        if (dtMs < 0)
            dtMs = 0;
        lock (_gate)
        {
            if (_moving)
            {
                var dx = _destX - _x;
                var dy = _destY - _y;
                var distance = MathF.Sqrt(dx * dx + dy * dy);
                var step = Speed * dtMs / 1000f;
                if (distance <= step || distance < 1f)
                {
                    _x = _destX;
                    _y = _destY;
                    _moving = false;
                }
                else
                {
                    _facingDeg = MathF.Atan2(dx, dy) * (180f / MathF.PI);
                    _x += dx / distance * step;
                    _y += dy / distance * step;
                }
            }

            Rebuild();
        }
    }

    public WorldSnapshot? GetSnapshot(long nowMs)
    {
        lock (_gate)
            return _snapshot with { ReceivedAtMs = nowMs };
    }

    public void Execute(BotCommand command)
    {
        lock (_gate)
        {
            switch (command.Kind)
            {
                case BotCommandKind.Attack:
                    Damage(command.Vid, 40);
                    _targetVid = command.Vid;
                    break;
                case BotCommandKind.Skill:
                    Damage(command.Vid, 100);
                    _mp = Math.Max(0, _mp - command.MpCost);
                    _targetVid = command.Vid;
                    break;
                case BotCommandKind.Move:
                    _destX = command.X;
                    _destY = command.Y;
                    _moving = true;
                    break;
                case BotCommandKind.Pickup:
                    _mobs.RemoveAll(mob => mob.Kind == EntityKind.Drop && (command.Vid == 0 || mob.Vid == command.Vid));
                    break;
                case BotCommandKind.Heal:
                    _hp = Math.Min(_maxHp, _hp + 250);
                    break;
                case BotCommandKind.Stop:
                    _moving = false;
                    break;
            }

            Rebuild();
        }
    }

    private void Damage(uint vid, int amount)
    {
        var mob = _mobs.FirstOrDefault(item => item.Vid == vid && item.Kind != EntityKind.Drop);
        if (mob == null)
            return;
        mob.Hp -= amount;
        if (mob.Hp > 0)
            return;
        _mobs.Remove(mob);
        _mobs.Add(new Mob
        {
            Vid = _nextVid++,
            Name = mob.Name + " drop",
            Kind = EntityKind.Drop,
            Type = ActorType.Object,
            Vnum = mob.Vnum,
            Hp = 1,
            MaxHp = 1,
            X = mob.X,
            Y = mob.Y,
            Z = mob.Z
        });
    }

    private void Rebuild()
    {
        var entities = new List<EntityState>(_mobs.Count);
        foreach (var mob in _mobs.OrderBy(item => item.Vid))
        {
            var distance = WorldGeometry.Distance(_x, _y, 0, mob.X, mob.Y, mob.Z);
            entities.Add(new EntityState
            {
                Vid = mob.Vid,
                Name = mob.Name,
                Kind = mob.Kind,
                Type = mob.Type,
                TypeSpecified = true,
                Vnum = mob.Vnum,
                Hp = mob.Hp,
                MaxHp = mob.MaxHp,
                X = mob.X,
                Y = mob.Y,
                Z = mob.Z,
                Distance = distance,
                ScreenX = 640 + (int)MathF.Round((mob.X - _x) * 0.5f),
                ScreenY = 360 + (int)MathF.Round((mob.Y - _y) * 0.5f),
                OnScreen = distance < 1800
            });
        }

        _snapshot = new WorldSnapshot
        {
            Player = new PlayerState
            {
                Name = "Simulator",
                Hp = _hp,
                MaxHp = _maxHp,
                Mp = _mp,
                MaxMp = _maxMp,
                X = _x,
                Y = _y,
                Level = 35,
                Exp = 12400,
                NextExp = 18000,
                TargetVid = _targetVid,
                MapName = "simulator",
                FacingDeg = _facingDeg
            },
            Entities = entities
        };
    }
}
