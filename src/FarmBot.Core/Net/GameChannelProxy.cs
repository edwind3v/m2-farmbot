using System.Net;
using System.Net.Sockets;
using FarmBot.Core.Logging;
using FarmBot.Core.Perception;

namespace FarmBot.Core.Net;

/// <summary>
/// Local forwarder. The game client connects here instead of the channel port,
/// and the bytes the server sends are parsed into nearby entities.
/// </summary>
public sealed class GameChannelProxy : IDisposable
{
    private readonly Metin2EntityStream _stream;
    private readonly IBotLog _log;
    private readonly object _gate = new();
    private TcpListener? _listener;
    private TcpClient? _session;
    private Thread? _thread;
    private volatile bool _stop;
    private int _clientConnected;

    public GameChannelProxy(Metin2EntityStream stream, IBotLog log)
    {
        _stream = stream;
        _log = log;
    }

    public Metin2EntityStream Stream => _stream;

    public bool ClientConnected => Volatile.Read(ref _clientConnected) == 1;

    public void Start(string host, int port, int listenPort)
    {
        lock (_gate)
        {
            if (_thread != null)
                return;
            _stop = false;
            _listener = new TcpListener(IPAddress.Loopback, listenPort);
            _listener.Start();
            _thread = new Thread(() => AcceptLoop(host, port))
            {
                IsBackground = true,
                Name = "FarmBotChannel"
            };
            _thread.Start();
        }

        _log.Info(
            $"listening on 127.0.0.1:{listenPort}, forwarding to {host}:{port}. " +
            $"In serverinfo.py set the channel host to 127.0.0.1 and the port to {listenPort}, then log in again.");
    }

    public void Dispose()
    {
        _stop = true;
        TcpListener? listener;
        TcpClient? session;
        Thread? thread;
        lock (_gate)
        {
            listener = _listener;
            session = _session;
            thread = _thread;
            _listener = null;
            _session = null;
            _thread = null;
        }

        try { listener?.Stop(); } catch { }
        try { session?.Close(); } catch { }
        thread?.Join(TimeSpan.FromSeconds(1));
    }

    private void AcceptLoop(string host, int port)
    {
        while (!_stop)
        {
            TcpClient client;
            try
            {
                client = _listener!.AcceptTcpClient();
            }
            catch
            {
                break;
            }

            client.NoDelay = true;
            lock (_gate)
                _session = client;
            Volatile.Write(ref _clientConnected, 1);
            _stream.Reset();
            _log.Info("game client connected");
            TcpClient? upstream = null;
            try
            {
                upstream = new TcpClient { NoDelay = true };
                upstream.Connect(host, port);
                var fromServer = upstream.GetStream();
                var fromClient = client.GetStream();
                var serverPump = new Thread(() => Pump(fromServer, fromClient, parse: true, client.Close))
                {
                    IsBackground = true,
                    Name = "FarmBotServerBytes"
                };
                serverPump.Start();
                Pump(fromClient, fromServer, parse: false, upstream.Close);
                serverPump.Join(TimeSpan.FromSeconds(1));
            }
            catch (Exception ex) when (!_stop)
            {
                _log.Error($"server stream: {ex.Message}");
            }
            finally
            {
                try { upstream?.Close(); } catch { }
                try { client.Close(); } catch { }
                Volatile.Write(ref _clientConnected, 0);
                lock (_gate)
                {
                    if (ReferenceEquals(_session, client))
                        _session = null;
                }

                if (!_stop)
                    _log.Info("game client disconnected");
            }
        }
    }

    private void Pump(NetworkStream from, NetworkStream to, bool parse, Action closePeer)
    {
        var buffer = new byte[16384];
        try
        {
            while (!_stop)
            {
                var read = from.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
                if (parse)
                    _stream.Ingest(buffer.AsSpan(0, read));
                to.Write(buffer, 0, read);
            }
        }
        catch
        {
            // The other side closed the socket.
        }
        finally
        {
            try { closePeer(); } catch { }
        }
    }
}
