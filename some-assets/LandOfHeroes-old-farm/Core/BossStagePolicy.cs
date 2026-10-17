using System;
public enum BossStep { WalkForward, Search, Approach, Hold, Cleared, Teleported }
public sealed class BossStagePolicy
{
    public float StopDistance = 300f;
    public float ForwardLimit = 4000f;
    public long ForwardTimeout = 20000;
    public long ClearDelay = 3000;
    public float WarpJump = 1500f;
    private bool started, seen;
    private float startX, startY, lastX, lastY;
    private long startedAt, emptySince = -1;
    public bool TargetSeen { get { return seen; } }
    public float StartX { get { return startX; } }
    public float StartY { get { return startY; } }
    public void Reset() { started = false; seen = false; emptySince = -1; }
    public BossStep Next(float x, float y, int monsters, float nearestDistance, long now)
    {
        if (!started) { started = true; startX = lastX = x; startY = lastY = y; startedAt = now; }
        double jump = Distance(x, y, lastX, lastY);
        lastX = x; lastY = y;
        // Wait attacks also jump next to monsters; only a jump out of an already empty room is the stage warp.
        if (seen && jump > WarpJump && emptySince >= 0) return BossStep.Teleported;
        if (monsters > 0) { seen = true; emptySince = -1; return nearestDistance > StopDistance ? BossStep.Approach : BossStep.Hold; }
        if (!seen)
            return Distance(x, y, startX, startY) < ForwardLimit && now - startedAt < ForwardTimeout ? BossStep.WalkForward : BossStep.Search;
        if (emptySince < 0) emptySince = now;
        return now - emptySince >= ClearDelay ? BossStep.Cleared : BossStep.Hold;
    }
    public static RoutePoint Ahead(float x, float y, float heading, float distance)
    {
        double r = heading * Math.PI / 180.0;
        return new RoutePoint { X = x + (float)(Math.Sin(r) * distance), Y = y + (float)(Math.Cos(r) * distance) };
    }
    private static double Distance(float x, float y, float tx, float ty) { double dx = x - tx, dy = y - ty; return Math.Sqrt(dx * dx + dy * dy); }
}
