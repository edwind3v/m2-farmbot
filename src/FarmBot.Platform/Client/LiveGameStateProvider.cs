using FarmBot.Core.Entities;
using FarmBot.Core.Geometry;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Platform.Client;

public sealed class LiveGameStateProvider : IGameStateProvider, IGameStateSource, IDisposable
{
    public static readonly string[] ProcessNames = { "1.metin2client", "1.Debug-metin2client" };
    public const int MaxEntities = 512;

    private readonly IBotLog? _log;
    private readonly object _gate = new();
    private ProcessMemory? _memory;
    private ReleaseLayout? _layout;
    private string _layoutPath = "";
    private bool _loggedRead;
    private bool _loggedEnum;
    private int _attachedPid = -1;

    public LiveGameStateProvider(IBotLog? log = null)
    {
        _log = log;
    }

    public string Name => "Live client";
    public bool FromLiveClient => true;
    public string Status { get; private set; } = "live client was not probed yet";

    public GameFrame? Capture(long nowMs)
    {
        lock (_gate)
            return CaptureLocked(nowMs);
    }

    public WorldSnapshot? GetSnapshot(long nowMs)
    {
        var frame = Capture(nowMs);
        return frame == null ? null : EntityMapping.ToSnapshot(frame);
    }

    public void Dispose()
    {
        lock (_gate)
            Close("disposed");
    }

    private GameFrame? CaptureLocked(long nowMs)
    {
        if (_memory != null && _memory.HasExited())
        {
            Log($"client PID {_attachedPid} closed; releasing handle");
            Close("client closed");
        }

        if (_memory == null)
        {
            if (!ProcessMemory.TryAttach(ProcessNames, out var memory, out var attachStatus) || memory == null)
            {
                Status = attachStatus;
                return null;
            }

            _memory = memory;
            _attachedPid = memory.ProcessId;
            if (!EnsureLayout(memory, out var layoutStatus))
            {
                Status = layoutStatus;
                Close(layoutStatus);
                return null;
            }

            if (_layout is { ProbeStringRva: > 0 } &&
                _memory.TryRead(_memory.ImageBase + _layout.ProbeStringRva, 24, out var probe) &&
                System.Text.Encoding.ASCII.GetString(probe).StartsWith("CPythonCharacterManager", StringComparison.Ordinal))
            {
                attachStatus += "; probe string read from live image";
            }

            Status = attachStatus;
            if (!_loggedRead)
            {
                _loggedRead = true;
                Log($"actual ReadProcessMemory succeeded: {Status}");
            }
        }

        var layout = _layout;
        var process = _memory;
        if (layout == null || process == null || !layout.CanEnumerate)
        {
            Status = layout == null
                ? "release layout was not recovered from the running image"
                : "release layout is incomplete; live walk is not enabled. " + string.Join(" ", layout.Evidence.TakeLast(3));
            return null;
        }

        if (!process.TryReadU32(process.ImageBase + layout.SingletonRva, out var manager) || !IsUserPointer(manager))
        {
            Status = $"{process.ProcessName} PID {process.ProcessId}: CPythonCharacterManager singleton at image+0x{layout.SingletonRva:X} is null or unreadable (not in game world)";
            return Empty(nowMs, process);
        }

        if (!process.TryReadU32(manager + layout.MapDisplacement + layout.MapHeadDisplacement, out var head) ||
            !IsUserPointer(head))
        {
            Status = $"manager 0x{manager:X} map head at +0x{layout.MapDisplacement:X} is invalid";
            return Empty(nowMs, process);
        }

        process.TryReadU32(manager + layout.MapDisplacement + layout.MapSizeDisplacement, out var reportedSize);
        if (reportedSize > MaxEntities)
            reportedSize = MaxEntities;

        if (!process.TryRead(head + layout.NodeIsNil, 1, out var headNil) || headNil[0] == 0)
        {
            Status = $"map sentinel 0x{head:X} is not _Isnil; refusing to walk an unvalidated tree";
            return Empty(nowMs, process);
        }

        Entity? main = null;
        if (layout.MainPointerDisplacement != 0 &&
            process.TryReadU32(manager + layout.MainPointerDisplacement, out var mainPtr) &&
            IsUserPointer(mainPtr) &&
            TryReadInstance(process, layout, 0, mainPtr, 0, 0, 0, out var mainEntity) &&
            mainEntity != null)
        {
            main = mainEntity;
        }

        var entities = new List<Entity>(Math.Max(8, (int)reportedSize));
        if (!process.TryReadU32(head + layout.NodeLeft, out var node) || !IsUserPointer(node))
        {
            Status = $"manager 0x{manager:X} map begin is invalid";
            return Frame(nowMs, process, main, entities);
        }

        var visited = new HashSet<uint>();
        var walked = 0;
        while (node != 0 && node != head && walked < MaxEntities && visited.Add(node))
        {
            if (!process.TryRead(node + layout.NodeIsNil, 1, out var isNil))
                break;
            if (isNil[0] == 0 &&
                process.TryReadU32(node + layout.NodeKey, out var vid) &&
                process.TryReadU32(node + layout.NodeValue, out var instance) &&
                IsUserPointer(instance) &&
                TryReadInstance(process, layout, vid, instance, main?.X ?? 0, main?.Y ?? 0, main?.Z ?? 0, out var entity) &&
                entity != null)
            {
                entities.Add(entity);
            }

            walked++;
            if (reportedSize != 0 && walked >= reportedSize)
                break;
            if (!TrySuccessor(process, layout, node, head, out node))
                break;
        }

        if (main == null && entities.Count > 0)
            main = entities.FirstOrDefault(item => item.Type == ActorType.Player);

        var withDistance = main == null
            ? entities
            : EntityManager.WithDistances(entities, main).ToList();

        Status = $"{process.ProcessName} PID {process.ProcessId} image=0x{process.ImageBase:X}: read {withDistance.Count} live instances from m_kAliveInstMap (manager 0x{manager:X}, reported {reportedSize})";
        if (!_loggedEnum && withDistance.Count > 0)
        {
            _loggedEnum = true;
            Log($"actual entity walk: {Status}");
        }

        return Frame(nowMs, process, main, withDistance);
    }

    private bool EnsureLayout(ProcessMemory memory, out string status)
    {
        var path = ResolveLayoutPath(memory);
        if (path.Length == 0)
        {
            status = "running client image path is unavailable; cannot recover release layout";
            return false;
        }

        if (_layout != null && string.Equals(_layoutPath, path, StringComparison.OrdinalIgnoreCase))
        {
            status = "layout cached";
            return _layout.CanEnumerate;
        }

        _layout = ClientReleaseLayout.Recover(path);
        _layoutPath = path;
        if (!_layout.CanEnumerate)
        {
            status = $"layout from {path} is incomplete: " + string.Join(" ", _layout.Evidence.TakeLast(4));
            _log?.Warn(status);
            return false;
        }

        status = $"layout recovered from {path} singletonRva=0x{_layout.SingletonRva:X} map=+0x{_layout.MapDisplacement:X}";
        Log(status);
        return true;
    }

    private static string ResolveLayoutPath(ProcessMemory memory)
    {
        if (File.Exists(memory.ImagePath))
            return memory.ImagePath;
        var fallback = memory.ProcessName.Contains("Debug", StringComparison.OrdinalIgnoreCase)
            ? @"E:\TeraBoxDownload\1.Client\1.Debug-metin2client.exe"
            : @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";
        return File.Exists(fallback) ? fallback : "";
    }

    private bool TryReadInstance(
        ProcessMemory memory,
        ReleaseLayout layout,
        uint mapVid,
        uint instance,
        float originX,
        float originY,
        float originZ,
        out Entity? entity)
    {
        entity = null;
        if (!memory.TryReadU32(instance + layout.GraphicDisplacement, out _))
            return false;

        if (!memory.TryReadU32(instance + layout.GraphicDisplacement + layout.ActorTypeDisplacement, out var typeRaw))
            return false;
        if (typeRaw > (uint)ActorType.Object)
            return false;

        if (!memory.TryReadU32(instance + layout.RaceDisplacement, out var race))
            return false;
        if (!memory.TryReadU32(instance + layout.VirtualNumberDisplacement, out var vnum))
            return false;
        if (!memory.TryReadF32(instance + layout.GraphicDisplacement + layout.PixelXDisplacement, out var x))
            return false;
        if (!memory.TryReadF32(instance + layout.GraphicDisplacement + layout.PixelYDisplacement, out var y))
            return false;
        if (!memory.TryReadF32(instance + layout.GraphicDisplacement + layout.PixelZDisplacement, out var z))
            return false;
        if (layout.NegatePixelY)
            y = -y;

        var vid = mapVid;
        if (vid == 0 && layout.SelfVidDisplacement != 0 &&
            memory.TryReadU32(instance + layout.SelfVidDisplacement, out var selfVid))
            vid = selfVid;
        if (vid == 0)
            return false;
        if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
            return false;
        if (Math.Abs(x) > 2_000_000 || Math.Abs(y) > 2_000_000)
            return false;

        var type = (ActorType)typeRaw;
        var vnumOrRace = vnum != 0 ? vnum : race;
        var distance = originX == 0 && originY == 0 && originZ == 0
            ? 0
            : WorldGeometry.Distance(originX, originY, originZ, x, y, z);
        entity = new Entity
        {
            Vid = vid,
            Type = type,
            Vnum = vnumOrRace,
            X = x,
            Y = y,
            Z = z,
            Distance = distance
        };
        return true;
    }

    private bool TrySuccessor(ProcessMemory memory, ReleaseLayout layout, uint node, uint head, out uint next)
    {
        next = 0;
        if (memory.TryReadU32(node + layout.NodeRight, out var right) &&
            IsUserPointer(right) &&
            memory.TryRead(right + layout.NodeIsNil, 1, out var rightNil) &&
            rightNil[0] == 0)
        {
            return TryLeftmost(memory, layout, right, out next);
        }

        var current = node;
        var guard = 0;
        while (guard++ < MaxEntities)
        {
            if (!memory.TryReadU32(current + layout.NodeParent, out var parent) || !IsUserPointer(parent))
                return false;
            if (parent == head)
                return false;
            if (!memory.TryReadU32(parent + layout.NodeRight, out var parentRight))
                return false;
            if (current != parentRight)
            {
                next = parent;
                return true;
            }

            current = parent;
        }

        return false;
    }

    private static bool TryLeftmost(ProcessMemory memory, ReleaseLayout layout, uint node, out uint leftmost)
    {
        leftmost = node;
        for (var i = 0; i < MaxEntities; i++)
        {
            if (!memory.TryReadU32(leftmost + layout.NodeLeft, out var left) || !IsUserPointer(left))
                return true;
            if (!memory.TryRead(left + layout.NodeIsNil, 1, out var isNil) || isNil[0] != 0)
                return true;
            leftmost = left;
        }

        return false;
    }

    private static bool IsUserPointer(uint address) => address >= 0x10000 && address < 0x7FFF0000 && (address & 3) == 0;

    private static GameFrame Empty(long nowMs, ProcessMemory process) =>
        Frame(nowMs, process, null, Array.Empty<Entity>());

    private static GameFrame Frame(long nowMs, ProcessMemory process, Entity? main, IReadOnlyList<Entity> entities) =>
        new()
        {
            ReceivedAtMs = nowMs,
            Main = main,
            Entities = entities,
            Source = $"{process.ProcessName}:{process.ProcessId}",
            FromLiveClient = true
        };

    private void Close(string reason)
    {
        _memory?.Dispose();
        _memory = null;
        _attachedPid = -1;
        _loggedEnum = false;
        if (!string.IsNullOrEmpty(reason))
            Status = reason;
    }

    private void Log(string message) => _log?.Info(message);
}
