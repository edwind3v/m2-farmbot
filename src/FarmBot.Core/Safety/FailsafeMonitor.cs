using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Safety;

public static class FailsafeMonitor
{
    public static string? Reason(WorldSnapshot? snapshot, BotConfig config, TickContext context, long nowMs)
    {
        if (context.CornerTripped && config.StopOnCorner)
            return "cursor in the top-left corner";

        if (snapshot == null)
        {
            if (context.EverReceivedState && nowMs - context.LastStateAtMs > config.StateTimeoutMs)
                return "client state timed out";
            return null;
        }

        if (snapshot.Player.MaxHp > 0 && snapshot.Player.Hp <= 0)
            return "player is dead";

        if (nowMs - snapshot.ReceivedAtMs > config.StateTimeoutMs)
            return "client state timed out";

        if (snapshot.Player.MaxHp > 0 && snapshot.Player.HpPercent < config.FailsafeHpPercent)
            return "player HP below failsafe";

        return null;
    }
}
