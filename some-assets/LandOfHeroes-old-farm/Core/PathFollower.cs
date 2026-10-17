using System;
using System.Collections.Generic;
public sealed class PathFollower
{
    private readonly List<RoutePoint> points;
    private int index = -1;
    public float Reach = 150f;
    public PathFollower(List<RoutePoint> points) { this.points = points ?? new List<RoutePoint>(); }
    public int Count { get { return points.Count; } }
    public int Index { get { return Math.Max(0, index); } }
    public bool Started { get { return index >= 0; } }
    public bool Done { get { return points.Count == 0 || index >= points.Count; } }
    public bool Near(float x, float y, float maxDistance)
    {
        foreach (var p in points) if (Distance(x, y, p.X, p.Y) <= maxDistance) return true;
        return false;
    }
    public RoutePoint Next(float x, float y)
    {
        if (points.Count == 0) return null;
        if (index < 0)
        {
            int best = 0; double bestDistance = double.MaxValue;
            for (int i = 0; i < points.Count; i++) { double d = Distance(x, y, points[i].X, points[i].Y); if (d < bestDistance) { bestDistance = d; best = i; } }
            index = best;
        }
        while (index < points.Count && Distance(x, y, points[index].X, points[index].Y) <= Reach) index++;
        while (index + 1 < points.Count && Distance(x, y, points[index + 1].X, points[index + 1].Y) < Distance(x, y, points[index].X, points[index].Y)
               && Distance(points[index].X, points[index].Y, points[index + 1].X, points[index + 1].Y) < Distance(x, y, points[index].X, points[index].Y)) index++;
        return index < points.Count ? points[index] : null;
    }
    public void Skip() { if (index < points.Count) index++; }
    public static bool ShouldRecord(RoutePoint last, float x, float y, float spacing)
    {
        if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y) || (x == 0 && y == 0)) return false;
        return last == null || Distance(x, y, last.X, last.Y) >= spacing;
    }
    private static double Distance(float x, float y, float tx, float ty) { double dx = x - tx, dy = y - ty; return Math.Sqrt(dx * dx + dy * dy); }
}
