using System.Buffers.Binary;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Tests;

public class ServerStreamTests
{
    [Fact]
    public void GamePhaseBuildsNearbyMobsFromCharacterPackets()
    {
        var stream = new Metin2EntityStream();
        stream.Ingest(Phase(4));
        stream.Ingest(MainCharacter(vid: 100, name: "Hero", x: 1000, y: 2000));
        stream.Ingest(Points(hp: 800, maxHp: 1000, mp: 120, maxMp: 200, level: 35));
        stream.Ingest(Phase(5));

        var dog = new byte[37];
        dog[0] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(dog.AsSpan(1), 7);
        BinaryPrimitives.WriteInt32LittleEndian(dog.AsSpan(9), 1100);
        BinaryPrimitives.WriteInt32LittleEndian(dog.AsSpan(13), 1900);
        dog[21] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(dog.AsSpan(22), 101);
        stream.Ingest(dog.AsSpan(0, 10));
        stream.Ingest(dog.AsSpan(10));
        stream.Ingest(Name(7, "Wild Dog"));

        var stone = new byte[37];
        stone[0] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(stone.AsSpan(1), 8);
        BinaryPrimitives.WriteInt32LittleEndian(stone.AsSpan(9), 1400);
        BinaryPrimitives.WriteInt32LittleEndian(stone.AsSpan(13), 2000);
        stone[21] = 2;
        stream.Ingest(stone);
        stream.Ingest(Name(8, "Metin of Combat"));

        var moved = new byte[24];
        moved[0] = 3;
        BinaryPrimitives.WriteUInt32LittleEndian(moved.AsSpan(4), 7);
        BinaryPrimitives.WriteInt32LittleEndian(moved.AsSpan(8), 1050);
        BinaryPrimitives.WriteInt32LittleEndian(moved.AsSpan(12), 1950);
        stream.Ingest(moved);

        var snapshot = stream.GetSnapshot(0);
        Assert.NotNull(snapshot);
        Assert.Equal("Hero", snapshot.Player.Name);
        Assert.Equal(800, snapshot.Player.Hp);
        Assert.Equal(1000, snapshot.Player.MaxHp);
        Assert.Equal(1000, snapshot.Player.X);
        Assert.Equal(2000, snapshot.Player.Y);

        var mob = Assert.Single(snapshot.Entities, entity => entity.Vid == 7);
        Assert.Equal("Wild Dog", mob.Name);
        Assert.Equal(EntityKind.Monster, mob.Kind);
        Assert.Equal(1050, mob.X);
        Assert.Equal(1950, mob.Y);

        var metin = Assert.Single(snapshot.Entities, entity => entity.Vid == 8);
        Assert.Equal(EntityKind.Metin, metin.Kind);
        Assert.Equal("Metin of Combat", metin.Name);

        var delete = new byte[5];
        delete[0] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(delete.AsSpan(1), 8);
        stream.Ingest(delete);
        snapshot = stream.GetSnapshot(0);
        Assert.NotNull(snapshot);
        Assert.DoesNotContain(snapshot.Entities, entity => entity.Vid == 8);
        Assert.Contains("game", stream.Status);
    }

    [Fact]
    public void UnknownHeaderStopsParsingWithoutThrowing()
    {
        var stream = new Metin2EntityStream();
        stream.Ingest(Phase(5));
        stream.Ingest(new byte[] { 200, 1, 2, 3 });
        Assert.Contains("lost sync", stream.Status);
        Assert.NotNull(stream.GetSnapshot(0));
    }

    private static byte[] Phase(byte phase) => new byte[] { 0xFD, phase };

    private static byte[] MainCharacter(uint vid, string name, int x, int y)
    {
        var packet = new byte[47];
        packet[0] = 15;
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(1), vid);
        var bytes = System.Text.Encoding.ASCII.GetBytes(name);
        bytes.CopyTo(packet.AsSpan(9));
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(34), x);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(38), y);
        return packet;
    }

    private static byte[] Points(int hp, int maxHp, int mp, int maxMp, int level)
    {
        var packet = new byte[1021];
        packet[0] = 16;
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1 + 4), level);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1 + 20), hp);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1 + 24), maxHp);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1 + 28), mp);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(1 + 32), maxMp);
        return packet;
    }

    private static byte[] Name(uint vid, string name)
    {
        var packet = new byte[71];
        packet[0] = 136;
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(1), vid);
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(packet.AsSpan(5));
        return packet;
    }
}
