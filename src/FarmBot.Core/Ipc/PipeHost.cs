using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text.Json;
using FarmBot.Core.Json;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;
using Microsoft.Win32.SafeHandles;

namespace FarmBot.Core.Ipc;

public sealed class PipeHost : IDisposable
{
    private readonly object _sync = new();
    private readonly ConcurrentQueue<byte[]> _outgoing = new();
    private readonly IBotLog _log;
    private readonly string _pipeName;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;
    private NamedPipeServerStream? _pipe;
    private WorldSnapshot? _latest;
    private bool _supportsCommands;
    private int _protocol;

    public PipeHost(string pipeName, IBotLog log)
    {
        _pipeName = string.IsNullOrWhiteSpace(pipeName) ? "M2FarmBot" : pipeName;
        _log = log;
    }

    public string PipeName => _pipeName;
    public int Protocol => _protocol;

    public bool IsConnected
    {
        get
        {
            lock (_sync)
                return _pipe is { IsConnected: true };
        }
    }

    public bool SupportsCommands
    {
        get
        {
            lock (_sync)
                return _supportsCommands;
        }
    }

    public WorldSnapshot? Latest
    {
        get
        {
            lock (_sync)
                return _latest;
        }
    }

    public void Start()
    {
        if (_acceptLoop != null)
            return;
        _cts = new CancellationTokenSource();
        _acceptLoop = Task.Run(() => AcceptLoop(_cts.Token));
        _log.Info($"listening on \\\\.\\pipe\\{_pipeName}");
    }

    public bool TrySend(BotCommand command)
    {
        NamedPipeServerStream? pipe;
        lock (_sync)
            pipe = _pipe;
        if (pipe == null || !pipe.IsConnected)
            return false;

        _outgoing.Enqueue(FrameBuffer.EncodeJson(CommandMessage.From(command)));
        return true;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        lock (_sync)
        {
            try { _pipe?.Dispose(); } catch { /* closing the accept call */ }
            _pipe = null;
        }

        try { _acceptLoop?.Wait(TimeSpan.FromSeconds(2)); }
        catch { /* the accept loop ends on cancellation */ }
        _cts?.Dispose();
        _acceptLoop = null;
    }

    private async Task AcceptLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var pipe = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await pipe.WaitForConnectionAsync(token);
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                pipe.Dispose();
                break;
            }
            catch (Exception ex)
            {
                pipe.Dispose();
                _log.Warn($"pipe accept failed: {ex.Message}");
                try { await Task.Delay(200, token); }
                catch (OperationCanceledException) { break; }
                continue;
            }

            lock (_sync)
            {
                _pipe = pipe;
                _supportsCommands = false;
            }

            _log.Info("authorized client connected");
            try
            {
                await ReadLoop(pipe, token);
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidDataException)
            {
                _log.Warn($"client connection closed: {ex.Message}");
            }
            finally
            {
                lock (_sync)
                {
                    if (ReferenceEquals(_pipe, pipe))
                        _pipe = null;
                    _supportsCommands = false;
                }
                while (_outgoing.TryDequeue(out _))
                {
                }
                pipe.Dispose();
                _log.Info("authorized client disconnected");
            }
        }
    }

    private async Task ReadLoop(NamedPipeServerStream pipe, CancellationToken token)
    {
        // One thread owns the pipe. A pending read overlapped with a write on the same
        // duplex pipe deadlocks on Windows, so commands are queued and flushed here.
        var buffer = new byte[8192];
        var frames = new FrameBuffer();
        while (!token.IsCancellationRequested && pipe.IsConnected)
        {
            while (_outgoing.TryDequeue(out var packet))
            {
                pipe.Write(packet, 0, packet.Length);
                pipe.Flush();
            }

            if (!PeekNamedPipe(pipe.SafePipeHandle, IntPtr.Zero, 0, IntPtr.Zero, out var available, IntPtr.Zero))
                break;
            if (available == 0)
            {
                try { await Task.Delay(10, token); }
                catch (OperationCanceledException) { break; }
                continue;
            }

            var read = pipe.Read(buffer, 0, (int)Math.Min(buffer.Length, available));
            if (read <= 0)
                break;

            frames.Append(buffer.AsSpan(0, read));
            while (frames.TryPop(out var payload))
                HandlePayload(payload);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool PeekNamedPipe(
        SafePipeHandle handle,
        IntPtr buffer,
        uint bufferSize,
        IntPtr bytesRead,
        out uint bytesAvailable,
        IntPtr bytesLeft);

    private void HandlePayload(byte[] payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
            if (string.Equals(type, "hello", StringComparison.OrdinalIgnoreCase))
            {
                var hello = root.Deserialize<HelloMessage>(JsonDefaults.Instance) ?? new HelloMessage();
                lock (_sync)
                {
                    _supportsCommands = hello.SupportsCommands;
                    _protocol = hello.Protocol;
                }
                _log.Info($"client hello '{hello.Client}' protocol {hello.Protocol} commands={hello.SupportsCommands}");
                return;
            }

            if (!string.Equals(type, "state", StringComparison.OrdinalIgnoreCase))
            {
                _log.Debug($"ignored ipc message '{type}'");
                return;
            }

            var state = root.Deserialize<StateMessage>(JsonDefaults.Instance);
            if (state?.Player == null)
            {
                _log.Warn("state message missing player");
                return;
            }

            var snapshot = SnapshotNormalizer.FromState(state, Environment.TickCount64);
            lock (_sync)
                _latest = snapshot;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            _log.Warn($"ignored malformed ipc payload: {ex.Message}");
        }
    }
}
