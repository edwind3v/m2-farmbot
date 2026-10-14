using FarmBot.Core.Geometry;
using FarmBot.Core.Ipc;
using FarmBot.Core.Models;

namespace FarmBot.Core.Perception;

public interface IGameStateSource
{
    WorldSnapshot? GetSnapshot(long nowMs);
}

public interface IAdvancingSource
{
    void Advance(int dtMs);
}

public static class SnapshotNormalizer
{
    public static WorldSnapshot FromState(StateMessage message, long receivedAtMs)
    {
        var dto = message.Player ?? new PlayerDto();
        var player = new PlayerState
        {
            Name = dto.Name ?? "",
            Hp = dto.Hp,
            MaxHp = dto.MaxHp,
            Mp = dto.Mp,
            MaxMp = dto.MaxMp,
            X = dto.X,
            Y = dto.Y,
            Z = dto.Z,
            Level = dto.Level,
            TargetVid = dto.TargetVid,
            MapName = dto.MapName ?? ""
        };

        var entities = new List<EntityState>();
        foreach (var entity in message.Entities ?? new List<EntityDto>())
        {
            var distance = WorldGeometry.Distance(player.X, player.Y, entity.X, entity.Y);
            if (distance <= 0.01f && entity.Distance > 0)
                distance = entity.Distance;

            entities.Add(new EntityState
            {
                Vid = entity.Vid,
                Name = entity.Name ?? "",
                Kind = entity.Kind,
                Hp = entity.Hp,
                MaxHp = entity.MaxHp,
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Distance = distance,
                ScreenX = (int)MathF.Round(entity.ScreenX),
                ScreenY = (int)MathF.Round(entity.ScreenY),
                OnScreen = entity.OnScreen
            });
        }

        return new WorldSnapshot
        {
            ReceivedAtMs = receivedAtMs,
            Player = player,
            Entities = entities
        };
    }
}

public sealed class PipeStateSource : IGameStateSource
{
    private readonly PipeHost _host;

    public PipeStateSource(PipeHost host)
    {
        _host = host;
    }

    public WorldSnapshot? GetSnapshot(long nowMs) => _host.Latest;
}
