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
using FarmBot.Core.Looting;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Tests;

public class ProtocolTests
{
    [Fact]
    public void EncodesLoginAndSelectCharacter()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.Login,
            Token = "tok",
            Account = "acc",
            Password = "secret",
            Host = "192.168.1.100",
            Port = 30003,
            AuthHost = "192.168.1.100",
            AuthPort = 30001,
            Slot = 2
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"type\": \"login\"", json);
        Assert.Contains("\"account\": \"acc\"", json);
        Assert.Contains("\"password\": \"secret\"", json);
        Assert.Contains("\"slot\": 2", json);

        var select = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.SelectCharacter,
            Token = "tok",
            Slot = 1
        });
        var selectJson = Encoding.UTF8.GetString(select, 4, select.Length - 4);
        Assert.Contains("\"type\": \"select_character\"", selectJson);
    }

    [Fact]
    public void DecodesGetSessionAccountWithoutUsingCharacterAsAccount()
    {
        var json = """{"ok":true,"type":"get_session","reason":"Game","phase":"Game","character":"Hero","account":"player1","password":"pw","characterSlot":2}""";
        var response = ActionProtocol.DecodeResponse(Encoding.UTF8.GetBytes(json));
        Assert.Equal("player1", response.Account);
        Assert.Equal("Hero", response.Character);
        Assert.Equal("pw", response.Password);
        Assert.Equal(2, response.CharacterSlot);
        Assert.NotEqual(response.Character, response.Account);
    }

    [Fact]
    public void LoadingPhaseDoesNotRequestAnotherCharacterSelect()
    {
        Assert.False(QuickPlayOrchestrator.NeedsCharacterSelect("Loading"));
        Assert.True(QuickPlayOrchestrator.IsEnteringWorld("Loading"));
        Assert.True(QuickPlayOrchestrator.NeedsCharacterSelect("Select"));
        Assert.False(QuickPlayOrchestrator.NeedsCharacterSelect("Handshake"));
        Assert.False(QuickPlayOrchestrator.IsGame("Loading"));
        Assert.True(QuickPlayOrchestrator.IsGame("Game"));
        Assert.False(QuickPlayOrchestrator.IsGame("GameLoading"));
        Assert.False(QuickPlayOrchestrator.IsEnteringWorld("Login"));
        Assert.False(QuickPlayOrchestrator.IsGame("Login"));
        Assert.True(QuickPlayOrchestrator.LooksInWorld("", false, "lockedin", 21, 2780));
        Assert.False(QuickPlayOrchestrator.LooksInWorld("", false, "", 21, 2780));
        Assert.False(QuickPlayOrchestrator.LooksInWorld("Login", false, "lockedin", 21, 2780));
        Assert.False(QuickPlayOrchestrator.LooksInWorld("Loading", false, "lockedin", 21, 2780));
        Assert.True(QuickPlayOrchestrator.LooksInWorld("Game", false, "lockedin", 21, 2780));
    }

    [Fact]
    public void StartGameNativeSetsTheLoadingFlagNotThePhaseString()
    {
        Assert.Equal(0x13CD40u, LoginNativeContract.StartGameRva);
        Assert.Equal(0x8Cu, LoginNativeContract.StartGameFlagDisplacement);
        Assert.Equal(0x1367F0u, LoginNativeContract.SetGamePhaseRva);
        Assert.Equal(0x11C0C0u, LoginNativeContract.NotifyGamePhaseRva);
        Assert.Equal(0x13CC00u, LoginNativeContract.SendEnterGamePacketRva);
        Assert.Equal(0x160u, LoginNativeContract.PythonHandlerDisplacement);
        Assert.Equal(0x8u, LoginNativeContract.GameWindowDisplacement);
    }

    [Fact]
    public void DecodesStartGameFlagAndInWorld()
    {
        var json = """{"ok":true,"type":"get_phase","phase":"Loading","startGame":true,"inWorld":false}""";
        var response = ActionProtocol.DecodeResponse(Encoding.UTF8.GetBytes(json));
        Assert.Equal("Loading", response.Phase);
        Assert.True(response.StartGame);
        Assert.False(response.InWorld);

        var world = ActionProtocol.DecodeResponse(Encoding.UTF8.GetBytes(
            """{"ok":true,"type":"enter_game","phase":"Game","startGame":false,"inWorld":true}"""));
        Assert.True(world.InWorld);
        Assert.False(world.StartGame);

        Assert.False(QuickPlayOrchestrator.IsGame("Loading"));
        Assert.True(QuickPlayOrchestrator.IsEnteringWorld("Loading"));
        Assert.False(QuickPlayOrchestrator.IsEnteringWorld("Login"));
        Assert.False(QuickPlayOrchestrator.IsGame("Login"));
    }

    [Fact]
    public void DecodesHudReplyAccountWithoutConfusingItForCharacter()
    {
        var json = """{"ok":true,"type":"get_hud","reason":"hud","character":"Hero","account":"loginid","password":"secret","characterSlot":0}""";
        var response = ActionProtocol.DecodeResponse(Encoding.UTF8.GetBytes(json));
        Assert.Equal("loginid", response.Account);
        Assert.Equal("Hero", response.Character);
        Assert.Equal("secret", response.Password);
        Assert.Equal(0, response.CharacterSlot);
    }

    [Fact]
    public void EncodesShotWithVid()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.Shot,
            Token = "tok",
            Vid = 42,
            Force = true
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"type\": \"shot\"", json);
        Assert.Contains("\"vid\": 42", json);
    }

    [Fact]
    public void EncodesShotHitsAndRestorePosition()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.Shot,
            Token = "tok",
            Vid = 11,
            X = 100,
            Y = 200,
            Hits = "11,64000,73000;22,64100,73100"
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"hits\": \"11,64000,73000;22,64100,73100\"", json);
        Assert.Contains("\"x\": 100", json);
        Assert.Contains("\"y\": 200", json);
    }

    [Fact]
    public void EncodesShotAvoidFlag()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.Shot,
            Token = "tok",
            Vid = 11,
            Avoid = true,
            Hits = "11,64000,73000"
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"avoid\": true", json);
    }

    [Fact]
    public void SanitizeKeepsOneMsDelayAndMoreThanTwentyTargets()
    {
        var config = Fixtures.Config();
        config.RangedAttackDelayMs = 1;
        config.RangedAttackMaxTargets = 50;
        config.Sanitize();
        Assert.Equal(1, config.RangedAttackDelayMs);
        Assert.Equal(50, config.RangedAttackMaxTargets);
    }

    [Fact]
    public void RangedPickupUsesAttackRadiusLikeRangedShots()
    {
        var config = Fixtures.Config();
        config.RangedPickupEnabled = true;
        config.RangedAttackRadius = 2000;
        config.LootRange = 600;
        Assert.True(LootPolicy.SpoofPickup(config));
        Assert.Equal(2000f, LootPolicy.PickupRadius(config));
    }

    [Fact]
    public void FastAndRangedPickupLoopsAreIndependentAndStartWithoutTheBot()
    {
        var config = Fixtures.Config();
        config.FastPickupEnabled = true;
        config.RangedPickupEnabled = false;
        Assert.Equal(LootPolicy.FastLoop, LootPolicy.LoopMask(config, false, false));

        config.FastPickupEnabled = false;
        config.RangedPickupEnabled = true;
        Assert.Equal(LootPolicy.RangedLoop, LootPolicy.LoopMask(config, false, false));

        config.FastPickupEnabled = true;
        Assert.Equal(LootPolicy.FastLoop | LootPolicy.RangedLoop, LootPolicy.LoopMask(config, false, false));

        config.FastPickupEnabled = false;
        config.RangedPickupEnabled = false;
        config.LootEnabled = true;
        Assert.Equal(0u, LootPolicy.LoopMask(config, false, false));
        Assert.Equal(LootPolicy.FastLoop, LootPolicy.LoopMask(config, true, false));
    }

    [Fact]
    public void SanitizeKeepsOneMsFastPickupDelay()
    {
        var config = Fixtures.Config();
        config.FastPickupDelayMs = 1;
        config.Sanitize();
        Assert.Equal(1, config.FastPickupDelayMs);
    }

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
    public void EncodesLootLoopDelayRadiusAndRangedFlag()
    {
        var packet = ActionProtocol.Encode(new ActionRequest
        {
            Type = ActionProtocol.LootLoop,
            Token = "tok",
            Vid = 1,
            Force = true,
            X = 1,
            Y = 2000,
            Avoid = true
        });
        var json = Encoding.UTF8.GetString(packet, 4, packet.Length - 4);
        Assert.Contains("\"x\": 1", json);
        Assert.Contains("\"y\": 2000", json);
        Assert.Contains("\"avoid\": true", json);
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
