using System.IO.Pipes;

namespace FarmBot.Core.Combat;

public sealed class ActionAdapterClient : IDisposable
{
    private NamedPipeClientStream? _pipe;
    private readonly object _gate = new();

    public string PipeName { get; }
    public string Token { get; }
    public bool Connected { get; private set; }
    public string Status { get; private set; } = "adapter not connected";

    public ActionAdapterClient(string pipeName, string token)
    {
        PipeName = string.IsNullOrWhiteSpace(pipeName) ? ActionProtocol.DefaultPipeName : pipeName;
        Token = token ?? "";
    }

    public bool TryConnect(int timeoutMs = 500)
    {
        lock (_gate)
        {
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

    public ActionResponse Submit(string type, uint vid = 0, bool force = true) =>
        Submit(type, vid, force, updateStatus: true);

    public ActionResponse Submit(string type, uint vid, bool force, bool updateStatus)
    {
        lock (_gate)
        {
            if (_pipe == null || !Connected)
                return new ActionResponse { Reason = Status };
            if (!Write(_pipe, new ActionRequest { Type = type, Token = Token, Vid = vid, Force = force }) ||
                !ActionProtocol.TryReadFrame(_pipe, out var payload))
            {
                Connected = false;
                Status = "adapter I/O failed";
                return new ActionResponse { Reason = Status };
            }

            var response = ActionProtocol.DecodeResponse(payload);
            if (updateStatus)
            {
                Status = response.Ok
                    ? $"{type} ok vid={response.TargetVid}"
                    : type + " rejected: " + response.Reason;
            }

            return response;
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
        var type = action.Kind switch
        {
            NativeActionKind.SetTarget => ActionProtocol.SetTarget,
            NativeActionKind.PressActor => ActionProtocol.PressActor,
            NativeActionKind.Face => ActionProtocol.Face,
            NativeActionKind.Attack => ActionProtocol.Attack,
            NativeActionKind.Stop => ActionProtocol.Stop,
            _ => ""
        };
        if (type.Length == 0)
            return false;
        return _client.Submit(type, action.Vid).Ok;
    }

    public bool TryReadHud(out PlayerHudPoints hud)
    {
        hud = default;
        Ensure();
        if (!_client.Connected)
            return false;
        var response = _client.Submit(ActionProtocol.GetHud, 0, true, updateStatus: false);
        if (!response.Ok || response.MaxHp <= 0 || response.Level is < 1 or > 250)
            return false;
        hud = new PlayerHudPoints(response.Level, response.Exp, response.NextExp, response.Hp, response.MaxHp, response.Mp, response.MaxMp);
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

    public void Dispose() => _client.Dispose();

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
