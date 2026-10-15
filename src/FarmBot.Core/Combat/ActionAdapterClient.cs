using System.IO.Pipes;
using System.Runtime.Versioning;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public sealed class ActionAdapterClient : IDisposable
{
    private NamedPipeClientStream? _pipe;
    private readonly object _gate = new();

    public string PipeName { get; }
    public string Token { get; }
    public bool Connected { get; private set; }
    public string Status { get; private set; } = "adapter not connected";
    public ActionResponse LastResponse { get; private set; } = new();

    public ActionAdapterClient(string pipeName, string token)
    {
        PipeName = string.IsNullOrWhiteSpace(pipeName) ? ActionProtocol.DefaultPipeName : pipeName;
        Token = token ?? "";
    }

    public bool TryConnect(int timeoutMs = 500)
    {
        lock (_gate)
        {
            if (Connected && _pipe != null && _pipe.IsConnected)
                return true;

            DisposePipe();
            try
            {
                var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.None);
                pipe.Connect(timeoutMs);
                if (!Write(pipe, new ActionRequest { Type = ActionProtocol.Auth, Token = Token }) ||
                    !ActionProtocol.TryReadFrame(pipe, out var payload))
                {
                    pipe.Dispose();
                    Status = "adapter auth exchange failed";
                    return false;
                }

                var response = ActionProtocol.DecodeResponse(payload);
                if (!response.Ok)
                {
                    pipe.Dispose();
                    Status = "adapter rejected token: " + response.Reason;
                    return false;
                }

                _pipe = pipe;
                Connected = true;
                Status = "adapter connected (" + response.Convention + ")";
                return true;
            }
            catch (Exception ex)
            {
                Status = "adapter not listening: " + ex.Message;
                return false;
            }
        }
    }

    public ActionResponse SetTarget(uint vid, bool force = true) =>
        Submit(ActionProtocol.SetTarget, vid, force);

    public ActionResponse GetPhase() =>
        Submit(ActionProtocol.GetPhase, 0, true, updateStatus: false);

    public ActionResponse GetSession() =>
        Submit(ActionProtocol.GetSession, 0, true, updateStatus: false);

    public ActionResponse SkipIntro() =>
        Submit(ActionProtocol.SkipIntro, 0, true);

    public ActionResponse Login(string account, string password, string host, int port, string authHost, int authPort, int slot) =>
        Submit(new ActionRequest
        {
            Type = ActionProtocol.Login,
            Account = account,
            Password = password,
            Host = host,
            Port = port,
            AuthHost = authHost,
            AuthPort = authPort,
            Slot = slot
        });

    public ActionResponse SelectCharacter(int slot) =>
        Submit(new ActionRequest
        {
            Type = ActionProtocol.SelectCharacter,
            Slot = slot
        }, updateStatus: false);

    public ActionResponse EnterGame(bool setGamePhase = false) =>
        Submit(ActionProtocol.EnterGame, 0, setGamePhase, updateStatus: false);

    public ActionResponse Submit(string type, uint vid = 0, bool force = true) =>
        Submit(type, vid, force, updateStatus: true);

    public ActionResponse Submit(string type, uint vid, bool force, bool updateStatus) =>
        Submit(new ActionRequest { Type = type, Vid = vid, Force = force }, updateStatus);

    public ActionResponse Submit(ActionRequest request, bool updateStatus = true)
    {
        lock (_gate)
        {
            if (_pipe == null || !Connected)
                return new ActionResponse { Reason = Status };
            request.Token = Token;
            try
            {
                if (!Write(_pipe, request) ||
                    !ActionProtocol.TryReadFrame(_pipe, out var payload))
                {
                    Connected = false;
                    Status = "adapter I/O failed";
                    return new ActionResponse { Reason = Status };
                }

                var response = ActionProtocol.DecodeResponse(payload);
                LastResponse = response;
                if (updateStatus)
                {
                    Status = response.Ok
                        ? $"{request.Type} ok vid={response.TargetVid}"
                        : request.Type + " rejected: " + response.Reason;
                }

                return response;
            }
            catch (Exception ex)
            {
                Connected = false;
                Status = "adapter I/O failed: " + ex.Message;
                return new ActionResponse { Reason = Status };
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
            DisposePipe();
    }

    private void DisposePipe()
    {
        _pipe?.Dispose();
        _pipe = null;
        Connected = false;
    }

    private static bool Write(Stream stream, ActionRequest request)
    {
        var packet = ActionProtocol.Encode(request);
        stream.Write(packet, 0, packet.Length);
        stream.Flush();
        return true;
    }
}

public sealed class AdapterActionSink : INativeActionSink, IDisposable
{
    private readonly ActionAdapterClient _client;
    private readonly object _gate = new();
    private bool _tried;

    public AdapterActionSink(string? pipeName = null, string? token = null)
    {
        _client = new ActionAdapterClient(
            pipeName ?? ActionProtocol.DefaultPipeName,
            token ?? Environment.GetEnvironmentVariable("FARMBOT_ACTION_TOKEN") ?? "");
    }

    public ActionResponse LastSession => _client.LastResponse;

    public bool Available
    {
        get
        {
            Ensure();
            return _client.Connected;
        }
    }

    public string Status
    {
        get
        {
            Ensure();
            return _client.Connected ? _client.Status : "adapter not connected: " + _client.Status;
        }
    }

    public bool TrySubmit(NativeCombatAction action)
    {
        Ensure();
        if (!_client.Connected)
            return false;
        if (action.Kind == NativeActionKind.Shot)
        {
            return _client.Submit(new ActionRequest
            {
                Type = ActionProtocol.Shot,
                Vid = action.Vid,
                Force = true,
                X = action.X,
                Y = action.Y,
                Avoid = action.Avoid,
                Hits = action.Hits ?? ""
            }).Ok;
        }
        if (action.Kind == NativeActionKind.MoveTo)
        {
            return _client.Submit(new ActionRequest
            {
                Type = ActionProtocol.Move,
                X = action.X,
                Y = action.Y
            }).Ok;
        }
        if (action.Kind == NativeActionKind.LootLoop)
        {
            return _client.Submit(new ActionRequest
            {
                Type = ActionProtocol.LootLoop,
                Vid = action.Vid,
                Force = action.Vid != 0,
                X = action.X,
                Y = action.Y,
                Avoid = action.Avoid
            }).Ok;
        }

        var type = action.Kind switch
        {
            NativeActionKind.SetTarget => ActionProtocol.SetTarget,
            NativeActionKind.PressActor => ActionProtocol.PressActor,
            NativeActionKind.Face => ActionProtocol.Face,
            NativeActionKind.Attack => ActionProtocol.Attack,
            NativeActionKind.Stop => ActionProtocol.Stop,
            NativeActionKind.Pickup => ActionProtocol.Pickup,
            NativeActionKind.LootLoop => ActionProtocol.LootLoop,
            _ => ""
        };
        if (type.Length == 0)
            return false;
        return _client.Submit(new ActionRequest
        {
            Type = type,
            Vid = action.Vid,
            Force = action.Kind != NativeActionKind.LootLoop || action.Vid != 0,
            X = action.X,
            Y = action.Y
        }, updateStatus: action.Kind != NativeActionKind.Pickup).Ok;
    }

    public bool TryReadHud(out PlayerHudPoints hud)
    {
        hud = default;
        Ensure();
        if (!_client.Connected)
            return false;
        var response = _client.Submit(ActionProtocol.GetHud, 0, true, updateStatus: false);
        if (!_client.Connected)
            return false;
        var inWorld = QuickPlayOrchestrator.LooksInWorld(
            response.Phase,
            response.InWorld,
            response.Character,
            response.Level,
            response.MaxHp);
        var phase = response.Phase ?? "";
        var character = response.Character ?? "";
        var reason = response.Reason ?? "";
        uint? race = response.Race is >= 0 and <= 8 ? (uint)response.Race : null;
        if (response.MaxHp <= 0 || response.Level is < 1 or > 250)
        {
            hud = new PlayerHudPoints(0, 0, 0, 0, 0, 0, 0, inWorld, phase, character, reason, response.StartGame, race);
            return true;
        }

        hud = new PlayerHudPoints(
            response.Level,
            response.Exp,
            response.NextExp,
            response.Hp,
            response.MaxHp,
            response.Mp,
            response.MaxMp,
            inWorld,
            phase,
            character,
            reason,
            response.StartGame,
            race);
        return true;
    }

    public string LastAtlasStatus { get; private set; } = "";

    public bool TryReadAtlas(out AtlasPixels atlas)
    {
        atlas = new AtlasPixels();
        Ensure();
        if (!_client.Connected)
        {
            LastAtlasStatus = "adapter not connected";
            return false;
        }

        var response = _client.Submit(ActionProtocol.GetAtlas, 0, true, updateStatus: false);
        LastAtlasStatus = string.IsNullOrWhiteSpace(response.Reason) ? "no atlas reason" : response.Reason;
        if (!AtlasPixels.TryOpen(out atlas) || !atlas.HasImage)
            return false;
        if (atlas.WorldW <= 0 && response.WorldW > 0)
            atlas = new AtlasPixels
            {
                Width = atlas.Width,
                Height = atlas.Height,
                WorldW = response.WorldW,
                WorldH = response.WorldH,
                Bgra = atlas.Bgra
            };
        return true;
    }

    public bool TryConnect(int timeoutMs = 800)
    {
        lock (_gate)
        {
            _tried = true;
            return _client.TryConnect(timeoutMs);
        }
    }

    public ActionResponse GetPhase() => Session(_client.GetPhase);
    public ActionResponse GetSession() => Session(_client.GetSession);
    public ActionResponse SkipIntro() => Session(_client.SkipIntro);
    public ActionResponse EnterGame(bool setGamePhase = false) => Session(() => _client.EnterGame(setGamePhase));

    public ActionResponse Login(string account, string password, string host, int port, string authHost, int authPort, int slot)
    {
        Ensure();
        if (!_client.Connected && !_client.TryConnect(500))
            return new ActionResponse { Reason = Status };
        return _client.Login(account, password, host, port, authHost, authPort, slot);
    }

    public ActionResponse SelectCharacter(int slot)
    {
        Ensure();
        if (!_client.Connected && !_client.TryConnect(500))
            return new ActionResponse { Reason = Status };
        return _client.SelectCharacter(slot);
    }

    public bool TryReadPots(out PotionScan scan)
    {
        scan = new PotionScan();
        Ensure();
        if (!_client.Connected)
            return false;
        var response = _client.Submit(ActionProtocol.GetPots, 0, true, updateStatus: false);
        if (!response.Ok)
            return false;
        scan = new PotionScan
        {
            Pots = ParsePots(response.Pots),
            GreenRemain = response.GreenRemain,
            PurpleRemain = response.PurpleRemain
        };
        return true;
    }

    public bool TryUseItem(int slot)
    {
        Ensure();
        if (!_client.Connected || slot < 0)
            return false;
        return _client.Submit(new ActionRequest
        {
            Type = ActionProtocol.UseItem,
            Slot = slot,
            Force = true
        }, updateStatus: false).Ok;
    }

    public bool TryReadSkills(out SkillScan scan)
    {
        scan = new SkillScan();
        Ensure();
        if (!_client.Connected)
            return false;
        var response = _client.Submit(ActionProtocol.GetSkills, 0, true, updateStatus: false);
        if (!response.Ok)
            return false;
        scan = new SkillScan { Skills = ParseSkills(response.Skills) };
        return true;
    }

    public bool TryUseSkill(int slot, uint targetVid)
    {
        Ensure();
        if (!_client.Connected || slot < 0)
            return false;
        return _client.Submit(new ActionRequest
        {
            Type = ActionProtocol.UseSkill,
            Slot = slot,
            Vid = targetVid,
            Force = true
        }, updateStatus: false).Ok;
    }

    public bool TryDumpCaptchaBackgrounds()
    {
        Ensure();
        if (!_client.Connected)
            return false;
        return _client.Submit(new ActionRequest
        {
            Type = ActionProtocol.DumpCaptchaBg,
            Force = true
        }, updateStatus: false).Ok;
    }

    public bool TryReadEntities(out IReadOnlyList<EntityState> entities)
    {
        entities = Array.Empty<EntityState>();
        Ensure();
        if (!_client.Connected)
            return false;
        var response = _client.Submit(ActionProtocol.GetEntities, 0, true, updateStatus: false);
        if (!response.Ok)
            return false;
        entities = ParseActors(response.Entities);
        return true;
    }

    private static IReadOnlyList<PotionStack> ParsePots(System.Text.Json.JsonElement pots)
    {
        string json;
        if (pots.ValueKind == System.Text.Json.JsonValueKind.Array)
            json = pots.GetRawText();
        else if (pots.ValueKind == System.Text.Json.JsonValueKind.String)
            json = pots.GetString() ?? "";
        else
            return Array.Empty<PotionStack>();
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<PotionStack>();
        try
        {
            var rows = System.Text.Json.JsonSerializer.Deserialize<List<PotionWire>>(json, FarmBot.Core.Json.JsonDefaults.Instance);
            if (rows == null)
                return Array.Empty<PotionStack>();
            var list = new List<PotionStack>(rows.Count);
            foreach (var row in rows)
            {
                if (row.V == 0 || !TryFamily(row.F, out var family))
                    continue;
                list.Add(new PotionStack
                {
                    Vnum = row.V,
                    Name = row.N ?? "",
                    Count = row.C,
                    Slot = row.S,
                    Family = family,
                    Icon = ClientIconStore.DataUri(row.I, row.V),
                    DurationSec = row.D,
                    Tooltip = string.IsNullOrWhiteSpace(row.T) ? (row.N ?? "") : row.T
                });
            }

            return list;
        }
        catch
        {
            return Array.Empty<PotionStack>();
        }
    }

    private static bool TryFamily(string? value, out PotionFamily family)
    {
        family = PotionFamily.Hp;
        if (string.Equals(value, "hp", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(value, "mp", StringComparison.OrdinalIgnoreCase))
        {
            family = PotionFamily.Mp;
            return true;
        }
        if (string.Equals(value, "green", StringComparison.OrdinalIgnoreCase))
        {
            family = PotionFamily.Green;
            return true;
        }
        if (string.Equals(value, "purple", StringComparison.OrdinalIgnoreCase))
        {
            family = PotionFamily.Purple;
            return true;
        }

        return false;
    }

    private static IReadOnlyList<LearnedSkill> ParseSkills(System.Text.Json.JsonElement skills)
    {
        string json;
        if (skills.ValueKind == System.Text.Json.JsonValueKind.Array)
            json = skills.GetRawText();
        else if (skills.ValueKind == System.Text.Json.JsonValueKind.String)
            json = skills.GetString() ?? "";
        else
            return Array.Empty<LearnedSkill>();
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<LearnedSkill>();
        try
        {
            var rows = System.Text.Json.JsonSerializer.Deserialize<List<SkillWire>>(json, FarmBot.Core.Json.JsonDefaults.Instance);
            if (rows == null)
                return Array.Empty<LearnedSkill>();
            var list = new List<LearnedSkill>(rows.Count);
            foreach (var row in rows)
            {
                if (row.X == 0)
                    continue;
                list.Add(new LearnedSkill
                {
                    Index = row.X,
                    Slot = row.S,
                    Name = row.N ?? "",
                    Kind = string.Equals(row.K, "buff", StringComparison.OrdinalIgnoreCase) ? "buff" : "attack",
                    Level = row.L,
                    CoolRemain = row.C,
                    AffectRemain = row.R,
                    DurationSec = row.D,
                    Icon = ClientIconStore.DataUri(row.I, row.X)
                });
            }

            return list;
        }
        catch
        {
            return Array.Empty<LearnedSkill>();
        }
    }

    private static IReadOnlyList<EntityState> ParseActors(System.Text.Json.JsonElement actors)
    {
        string json;
        if (actors.ValueKind == System.Text.Json.JsonValueKind.Array)
            json = actors.GetRawText();
        else if (actors.ValueKind == System.Text.Json.JsonValueKind.String)
            json = actors.GetString() ?? "";
        else
            return Array.Empty<EntityState>();
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<EntityState>();
        try
        {
            var rows = System.Text.Json.JsonSerializer.Deserialize<List<ActorWire>>(json, FarmBot.Core.Json.JsonDefaults.Instance);
            if (rows == null || rows.Count == 0)
                return Array.Empty<EntityState>();
            var list = new List<EntityState>(rows.Count);
            foreach (var row in rows)
            {
                if (row.Vid == 0)
                    continue;
                var type = row.Type <= (uint)ActorType.Object ? (ActorType)row.Type : ActorType.Enemy;
                list.Add(new EntityState
                {
                    Vid = row.Vid,
                    Name = row.Name ?? "",
                    Type = type,
                    TypeSpecified = true,
                    Kind = EntityMapping.ToKind(type),
                    Vnum = row.Vnum,
                    X = row.X,
                    Y = row.Y,
                    Z = row.Z,
                    Hp = 1,
                    MaxHp = 1
                });
            }

            return list;
        }
        catch
        {
            return Array.Empty<EntityState>();
        }
    }

    private sealed class ActorWire
    {
        public uint Vid { get; set; }
        public uint Type { get; set; }
        public uint Vnum { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public string? Name { get; set; }
    }

    private sealed class SkillWire
    {
        public uint X { get; set; }
        public int S { get; set; }
        public string? N { get; set; }
        public string? K { get; set; }
        public int L { get; set; }
        public int C { get; set; }
        public int R { get; set; }
        public int D { get; set; }
        public string? I { get; set; }
    }

    private sealed class PotionWire
    {
        public uint V { get; set; }
        public string? N { get; set; }
        public int C { get; set; }
        public int S { get; set; }
        public string? F { get; set; }
        public string? I { get; set; }
        public int D { get; set; }
        public string? T { get; set; }
    }

    public void Dispose() => _client.Dispose();

    private ActionResponse Session(Func<ActionResponse> send)
    {
        Ensure();
        if (!_client.Connected)
            _client.TryConnect(500);
        if (!_client.Connected)
            return new ActionResponse { Reason = Status };
        return send();
    }

    private void Ensure()
    {
        lock (_gate)
        {
            if (_tried)
                return;
            _tried = true;
            if (!string.IsNullOrEmpty(_client.Token))
                _client.TryConnect();
        }
    }
}
