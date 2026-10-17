using System;
public static class AutomationTiming
{
    public static int ClampDelay(int delay) { return Math.Max(1, Math.Min(1000, delay)); }
    // Never replay missed deadlines as a burst. Always yield at least one millisecond.
    public static long NextDue(long started, long completed, int delay, bool fast)
    {
        return fast ? Math.Max(completed + 1, started + ClampDelay(delay))
                    : completed + ClampDelay(delay);
    }
}
