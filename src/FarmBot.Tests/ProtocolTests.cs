using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Ipc;
using FarmBot.Core.Json;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Tests;

public class ProtocolTests
{
    [Fact]
    public void EncodesLootLoopWithVidAndForce()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.LootLoop,
            Token = "tok",
            Vid = 1,
            Force = true
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"type\": \"loot_loop\"", json);
        Assert.Contains("\"vid\": 1", json);
        Assert.Contains("\"force\": true", json);
    }

    [Fact]
    public void FrameBufferReassemblesPartialReads()
    {
        var payload = Encoding.UTF8.GetBytes("{\"type\":\"state\"}");
        var packet = FrameBuffer.Encode(payload);
        var second = FrameBuffer.Encode(Encoding.UTF8.GetBytes("{\"type\":\"hello\"}"));
        var combined = packet.Concat(second).ToArray();

        var buffer = new FrameBuffer();
        buffer.Append(combined.AsSpan(0, 3));
        Assert.False(buffer.TryPop(out _));
        buffer.Append(combined.AsSpan(3, 6));
        Assert.False(buffer.TryPop(out _));
        buffer.Append(combined.AsSpan(9));

        Assert.True(buffer.TryPop(out var first));
        Assert.Equal(payload, first);
        Assert.True(buffer.TryPop(out var next));
        Assert.Equal("{\"type\":\"hello\"}", Encoding.UTF8.GetString(next));
        Assert.False(buffer.TryPop(out _));
    }

    [Fact]
    public void AcceptsNumericKindAndFractionalScreenCoordinates()
    {
        const string json = """
            {"type":"state","player":{"name":"A","hp":9,"maxHp":10,"mp":3,"maxMp":4,"x":0,"y":0,"level":2,"mapName":"map_a2"},"entities":[{"vid":9,"name":"Stone","kind":2,"hp":5,"maxHp":5,"x":10,"y":0,"screenX":11.6,"screenY":8.2,"onScreen":true}]}
            """;
        var state = JsonSerializer.Deserialize<StateMessage>(json, JsonDefaults.Instance);
        var snapshot = SnapshotNormalizer.FromState(state!, 50);

        Assert.Equal(EntityKind.Metin, snapshot.Entities[0].Kind);
        Assert.Equal(12, snapshot.Entities[0].ScreenX);
        Assert.Equal(8, snapshot.Entities[0].ScreenY);
        Assert.Equal(10f, snapshot.Entities[0].Distance, 3);
        Assert.Equal("map_a2", snapshot.Player.MapName);
    }

    [Fact]
    public async Task LiveClientStateSelectsTheMetinAndReceivesAttack()
    {
        var pipeName = "M2FarmBotTest" + Guid.NewGuid().ToString("N");
        var log = new ListLog();
        using var host = new PipeHost(pipeName, log);
        host.Start();

        using var client = await ConnectAsync(pipeName);
        WriteJson(client, new HelloMessage { Client = "metin2-test", SupportsCommands = true, Protocol = 1 });
        WriteJson(client, new StateMessage
        {
            Player = new PlayerDto { Name = "Hero", Hp = 900, MaxHp = 1000, Mp = 100, MaxMp = 200, MapName = "map_a2" },
            Entities = new List<EntityDto>
            {
                new() { Vid = 1, Name = "Wild Dog", Kind = EntityKind.Monster, Hp = 80, MaxHp = 80, X = 40, Y = 0, OnScreen = true, ScreenX = 420, ScreenY = 300 },
                new() { Vid = 2, Name = "Metin of Combat", Kind = EntityKind.Metin, Hp = 400, MaxHp = 400, X = 220, Y = 0, OnScreen = true, ScreenX = 500, ScreenY = 310 }
            }
        });

        var snapshot = await WaitForSnapshot(host);
        Assert.True(host.SupportsCommands);
        Assert.Equal(2, snapshot.Entities.Count);

        var config = Fixtures.Config();
        config.Skills.Clear();
        config.FarmRadius = 5000;
        var decision = new BotBrain().Tick(snapshot, config, new TickContext(true, false, true, snapshot.ReceivedAtMs), Environment.TickCount64);
        Assert.Equal(BotCommandKind.Attack, decision.Command!.Kind);
        Assert.Equal(2u, decision.Command.Vid);

        Assert.True(host.TrySend(decision.Command));
        var command = ReadCommand(client);
        Assert.Equal("attack", command.Name);
        Assert.Equal(2u, command.Vid);
        Assert.Equal("Metin of Combat", command.TargetName);
        Assert.Contains(log.Entries, entry => entry.Message.Contains("client hello", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<NamedPipeClientStream> ConnectAsync(string pipeName)
    {
        var started = Environment.TickCount64;
        while (Environment.TickCount64 - started < 5000)
        {
            var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await client.ConnectAsync(250);
                return client;
            }
            catch
            {
                await client.DisposeAsync();
                await Task.Delay(30);
            }
        }

        throw new TimeoutException($"Pipe {pipeName} did not accept a connection.");
    }

    private static async Task<WorldSnapshot> WaitForSnapshot(PipeHost host)
    {
        var started = Environment.TickCount64;
        while (Environment.TickCount64 - started < 3000)
        {
            if (host.Latest != null)
                return host.Latest;
            await Task.Delay(20);
        }

        throw new TimeoutException("Client state was not published.");
    }

    private static CommandMessage ReadCommand(NamedPipeClientStream client)
    {
        var buffer = new byte[8192];
        var frames = new FrameBuffer();
        var until = Environment.TickCount64 + 3000;
        while (Environment.TickCount64 < until)
        {
            if (!PeekNamedPipe(client.SafePipeHandle, IntPtr.Zero, 0, IntPtr.Zero, out var available, IntPtr.Zero))
                throw new IOException("Could not inspect the command pipe.");
            if (available == 0)
            {
                Thread.Sleep(20);
                continue;
            }

            var read = client.Read(buffer, 0, (int)Math.Min(buffer.Length, available));
            if (read <= 0)
                throw new IOException("Pipe closed before a command arrived.");
            frames.Append(buffer.AsSpan(0, read));
            if (!frames.TryPop(out var frame))
                continue;
            var command = JsonSerializer.Deserialize<CommandMessage>(frame, JsonDefaults.Instance);
            if (command != null)
                return command;
        }

        throw new TimeoutException("Command was not received.");
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool PeekNamedPipe(
        SafePipeHandle handle,
        IntPtr buffer,
        uint bufferSize,
        IntPtr bytesRead,
        out uint bytesAvailable,
        IntPtr bytesLeft);

    private static void WriteJson<T>(Stream stream, T value)
    {
        var packet = FrameBuffer.EncodeJson(value);
        stream.Write(packet, 0, packet.Length);
        stream.Flush();
    }
}
