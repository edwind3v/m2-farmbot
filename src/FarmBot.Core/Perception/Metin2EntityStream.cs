using System.Buffers.Binary;
using FarmBot.Core.Geometry;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;

namespace FarmBot.Core.Perception;

/// <summary>
/// Turns the server's character add, move, delete, and name packets into a world snapshot.
/// Sizes match the running client. Bytes are not encrypted on this server after the handshake.
/// </summary>
public sealed class Metin2EntityStream : IGameStateSource
{
    private const int PointLevel = 1;
    private const int PointHp = 5;
    private const int PointMaxHp = 6;
    private const int PointSp = 7;
    private const int PointMaxSp = 8;

    private readonly IBotLog? _log;
    private readonly object _gate = new();
    private readonly List<byte> _pending = new();
    private readonly Dictionary<uint, Actor> _actors = new();
    private Metin2Phase _phase = Metin2Phase.Handshake;
    private bool _synced = true;
    private bool _received;
    private int _lostHeader = -1;
    private long _lastPacketMs;
    private uint _playerVid;
    private string _playerName = "";
    private int _playerHp;
    private int _playerMaxHp;
    private int _playerMp;
    private int _playerMaxMp;
    private float _playerX;
    private float _playerY;
    private float _playerZ;
    private int _playerLevel;

    public Metin2EntityStream(IBotLog? log = null)
    {
        _log = log;
    }

    public string Status
    {
        get
        {
            lock (_gate)
            {
                if (!_synced)
                    return $"lost sync in {PhaseName(_phase)} at header {_lostHeader}";
                var combatants = 0;
                foreach (var actor in _actors.Values)
                {
                    if (actor.Kind is EntityKind.Monster or EntityKind.Metin or EntityKind.Boss)
                        combatants++;
                }

                return $"{PhaseName(_phase)} · {combatants} combatants · {_actors.Count} nearby";
            }
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _pending.Clear();
            _actors.Clear();
            _phase = Metin2Phase.Handshake;
            _synced = true;
            _received = false;
            _lostHeader = -1;
            _playerVid = 0;
            _playerName = "";
            _playerHp = 0;
            _playerMaxHp = 0;
            _playerMp = 0;
            _playerMaxMp = 0;
            _playerX = 0;
            _playerY = 0;
            _playerZ = 0;
            _playerLevel = 0;
        }
    }

    public void Ingest(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return;

        lock (_gate)
        {
            if (!_synced)
                return;

            _pending.AddRange(data.ToArray());
            Drain();
        }
    }

    public WorldSnapshot? GetSnapshot(long nowMs)
    {
        lock (_gate)
        {
            if (!_received)
                return null;

            var player = new PlayerState
            {
                Name = _playerName,
                Hp = _playerHp,
                MaxHp = _playerMaxHp,
                Mp = _playerMp,
                MaxMp = _playerMaxMp,
                X = _playerX,
                Y = _playerY,
                Z = _playerZ,
                Level = _playerLevel
            };

            var entities = new List<EntityState>(_actors.Count);
            foreach (var actor in _actors.Values)
            {
                entities.Add(new EntityState
                {
                    Vid = actor.Vid,
                    Name = actor.Name,
                    Kind = actor.Kind,
                    Hp = actor.Hp,
                    MaxHp = actor.MaxHp,
                    X = actor.X,
                    Y = actor.Y,
                    Z = actor.Z,
                    Distance = WorldGeometry.Distance(_playerX, _playerY, actor.X, actor.Y)
                });
            }

            return new WorldSnapshot
            {
                ReceivedAtMs = _lastPacketMs == 0 ? nowMs : _lastPacketMs,
                Player = player,
                Entities = entities
            };
        }
    }

    private void Drain()
    {
        var offset = 0;
        while (_synced && offset < _pending.Count)
        {
            var header = _pending[offset];
            var size = Metin2PacketTables.ForPhase(_phase)[header];
            if (size <= 0)
            {
                _synced = false;
                _lostHeader = header;
                _log?.Warn($"server stream lost sync in {PhaseName(_phase)} at header {header}");
                break;
            }

            if (_pending.Count - offset < size)
                break;

            var packet = _pending.GetRange(offset, size).ToArray();
            offset += size;
            _received = true;
            _lastPacketMs = Environment.TickCount64;
            Apply(packet);
        }

        if (offset > 0)
            _pending.RemoveRange(0, offset);
    }

    private void Apply(byte[] packet)
    {
        var header = packet[0];
        if (header == Metin2PacketTables.PhaseHeader && packet.Length >= 2)
        {
            var next = (Metin2Phase)packet[1];
            if (next == Metin2Phase.Loading)
                _actors.Clear();
            if (next != _phase && next is >= Metin2Phase.Handshake and <= Metin2Phase.Game)
            {
                _phase = next;
                _log?.Info($"channel phase {PhaseName(next)}");
            }

            return;
        }

        switch (header)
        {
            case Metin2PacketTables.CharacterAdd:
            case Metin2PacketTables.CharacterAdd2:
                ApplyAdd(packet);
                break;
            case Metin2PacketTables.CharacterAdditionalInfo:
                ApplyName(packet);
                break;
            case Metin2PacketTables.CharacterMove:
                ApplyMove(packet);
                break;
            case Metin2PacketTables.CharacterDelete:
                if (packet.Length >= 5)
                    _actors.Remove(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(1)));
                break;
            case Metin2PacketTables.CharacterDead:
                ApplyDead(packet);
                break;
            case Metin2PacketTables.MainCharacter:
            case Metin2PacketTables.MainCharacterExtended:
                ApplyMain(packet);
                break;
            case Metin2PacketTables.Points:
                ApplyPoints(packet);
                break;
            case Metin2PacketTables.PointChange:
                ApplyPointChange(packet);
                break;
        }
    }

    private void ApplyAdd(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 24)
            return;

        var vid = BinaryPrimitives.ReadUInt32LittleEndian(packet[1..]);
        var x = BinaryPrimitives.ReadInt32LittleEndian(packet[9..]);
        var y = BinaryPrimitives.ReadInt32LittleEndian(packet[13..]);
        var z = BinaryPrimitives.ReadInt32LittleEndian(packet[17..]);
        var kind = KindOf(packet[21]);
        if (vid != 0 && vid == _playerVid)
        {
            _playerX = x;
            _playerY = y;
            _playerZ = z;
            return;
        }

        var actor = GetOrCreate(vid);
        actor.Kind = kind;
        actor.Race = BinaryPrimitives.ReadUInt16LittleEndian(packet[22..]);
        actor.X = x;
        actor.Y = y;
        actor.Z = z;
        if (packet.Length >= 62)
        {
            var name = ReadName(packet, 37, 25);
            if (name.Length > 0)
                actor.Name = name;
        }
    }

    private void ApplyName(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 30)
            return;
        var vid = BinaryPrimitives.ReadUInt32LittleEndian(packet[1..]);
        if (vid == 0 || vid == _playerVid)
            return;
        var actor = GetOrCreate(vid);
        var name = ReadName(packet, 5, 25);
        if (name.Length > 0)
            actor.Name = name;
    }

    private void ApplyMove(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 16)
            return;
        var vid = BinaryPrimitives.ReadUInt32LittleEndian(packet[4..]);
        var x = BinaryPrimitives.ReadInt32LittleEndian(packet[8..]);
        var y = BinaryPrimitives.ReadInt32LittleEndian(packet[12..]);
        if (vid != 0 && vid == _playerVid)
        {
            _playerX = x;
            _playerY = y;
            return;
        }

        if (_actors.TryGetValue(vid, out var actor))
        {
            actor.X = x;
            actor.Y = y;
        }
    }

    private void ApplyDead(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 5)
            return;
        var vid = BinaryPrimitives.ReadUInt32LittleEndian(packet[1..]);
        if (_actors.TryGetValue(vid, out var actor))
            actor.Hp = 0;
    }

    private void ApplyMain(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 46)
            return;
        _playerVid = BinaryPrimitives.ReadUInt32LittleEndian(packet[1..]);
        _playerName = ReadName(packet, 9, 25);
        _playerX = BinaryPrimitives.ReadInt32LittleEndian(packet[34..]);
        _playerY = BinaryPrimitives.ReadInt32LittleEndian(packet[38..]);
        _playerZ = BinaryPrimitives.ReadInt32LittleEndian(packet[42..]);
        _actors.Remove(_playerVid);
    }

    private void ApplyPoints(ReadOnlySpan<byte> packet)
    {
        _playerLevel = Point(packet, PointLevel);
        _playerHp = Point(packet, PointHp);
        _playerMaxHp = Point(packet, PointMaxHp);
        _playerMp = Point(packet, PointSp);
        _playerMaxMp = Point(packet, PointMaxSp);
    }

    private void ApplyPointChange(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 17)
            return;
        var vid = BinaryPrimitives.ReadUInt32LittleEndian(packet[4..]);
        if (_playerVid != 0 && vid != _playerVid)
            return;
        var value = BinaryPrimitives.ReadInt32LittleEndian(packet[13..]);
        switch (packet[8])
        {
            case PointLevel:
                _playerLevel = value;
                break;
            case PointHp:
                _playerHp = value;
                break;
            case PointMaxHp:
                _playerMaxHp = value;
                break;
            case PointSp:
                _playerMp = value;
                break;
            case PointMaxSp:
                _playerMaxMp = value;
                break;
        }
    }

    private Actor GetOrCreate(uint vid)
    {
        if (!_actors.TryGetValue(vid, out var actor))
        {
            actor = new Actor { Vid = vid, Hp = 1, MaxHp = 1 };
            _actors[vid] = actor;
        }

        return actor;
    }

    private static int Point(ReadOnlySpan<byte> packet, int index)
    {
        var offset = 1 + index * 4;
        if (packet.Length < offset + 4)
            return 0;
        return BinaryPrimitives.ReadInt32LittleEndian(packet[offset..]);
    }

    private static EntityKind KindOf(byte type) => type switch
    {
        0 => EntityKind.Monster,
        1 => EntityKind.Npc,
        2 => EntityKind.Metin,
        6 => EntityKind.Player,
        _ => EntityKind.Unknown
    };

    private static string ReadName(ReadOnlySpan<byte> packet, int offset, int max)
    {
        if (offset >= packet.Length)
            return "";
        var end = offset;
        var limit = Math.Min(packet.Length, offset + max);
        while (end < limit && packet[end] != 0)
            end++;
        if (end == offset)
            return "";
        var chars = new char[end - offset];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Decode1250(packet[offset + i]);
        return new string(chars);
    }

    private static char Decode1250(byte value)
    {
        if (value < 0x80)
            return (char)value;
        const string high =
            "\u20AC\u0081\u201A\u0083\u201E\u2026\u2020\u2021\u0088\u2030\u0160\u2039\u015A\u0164\u017D\u0179" +
            "\u0090\u2018\u2019\u201C\u201D\u2022\u2013\u2014\u0098\u2122\u0161\u203A\u015B\u0165\u017E\u017A" +
            "\u00A0\u02C7\u02D8\u0141\u00A4\u0104\u00A6\u00A7\u00A8\u00A9\u015E\u00AB\u00AC\u00AD\u00AE\u017B" +
            "\u00B0\u00B1\u02DB\u0142\u00B4\u00B5\u00B6\u00B7\u00B8\u0105\u015F\u00BB\u013D\u02DD\u013E\u017C" +
            "\u0154\u00C1\u00C2\u0102\u00C4\u0139\u0106\u00C7\u010C\u00C9\u0118\u00CB\u011A\u00CD\u00CE\u010E" +
            "\u0110\u0143\u0147\u00D3\u00D4\u0150\u00D6\u00D7\u0158\u016E\u00DA\u0170\u00DC\u00DD\u0162\u00DF" +
            "\u0155\u00E1\u00E2\u0103\u00E4\u013A\u0107\u00E7\u010D\u00E9\u0119\u00EB\u011B\u00ED\u00EE\u010F" +
            "\u0111\u0144\u0148\u00F3\u00F4\u0151\u00F6\u00F7\u0159\u016F\u00FA\u0171\u00FC\u00FD\u0163\u02D9";
        return high[value - 0x80];
    }

    private static string PhaseName(Metin2Phase phase) => phase switch
    {
        Metin2Phase.Handshake => "handshake",
        Metin2Phase.Login => "login",
        Metin2Phase.Select => "select",
        Metin2Phase.Loading => "loading",
        Metin2Phase.Game => "game",
        _ => "closed"
    };

    private sealed class Actor
    {
        public uint Vid;
        public string Name = "";
        public EntityKind Kind;
        public int Race;
        public int Hp;
        public int MaxHp;
        public float X;
        public float Y;
        public float Z;
    }
}
