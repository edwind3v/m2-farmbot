namespace FarmBot.Core.Entities;

public interface IGameStateProvider
{
    string Name { get; }
    string Status { get; }
    bool FromLiveClient { get; }
    GameFrame? Capture(long nowMs);
}
