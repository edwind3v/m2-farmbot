using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public static class UnstuckPlanner
{
    public const float MinStepPx = 200f;
    public const float MaxStepPx = 500f;
    public const float LandClearPx = 90f;
    public const float CorridorPx = 90f;
    public const float PlayerMarginPx = 80f;
    public const float TargetMarginPx = 180f;

    public static bool IsStone(EntityState entity) =>
        entity.ResolvedType == ActorType.Stone || entity.Kind == EntityKind.Metin;

    public static EntityState? FindInteriorStone(
        PlayerState player,
        EntityState target,
        IReadOnlyList<EntityState> entities)
    {
        var pathLen = Hypot(target.X - player.X, target.Y - player.Y);
        if (pathLen < PlayerMarginPx + TargetMarginPx + 40f)
            return null;

        var tMin = PlayerMarginPx / pathLen;
        var tMax = 1f - TargetMarginPx / pathLen;
        EntityState? best = null;
        var bestDist = float.MaxValue;
        foreach (var entity in entities)
        {
            if (entity.Vid == target.Vid || !IsStone(entity))
                continue;

            var dist = DistanceToSegment(entity.X, entity.Y, player.X, player.Y, target.X, target.Y, out var t);
            if (t < tMin || t > tMax || dist > CorridorPx)
                continue;
            if (dist >= bestDist)
                continue;
            bestDist = dist;
            best = entity;
        }

        return best;
    }

    public static bool TryPickWaypoint(
        PlayerState player,
        EntityState target,
        IReadOnlyList<EntityState> entities,
        BotConfig config,
        int attempt,
        out float x,
        out float y)
    {
        var dx = target.X - player.X;
        var dy = target.Y - player.Y;
        var len = Hypot(dx, dy);
        float fx;
        float fy;
        float lx;
        float ly;
        if (len < 1f)
        {
            fx = 1f;
            fy = 0f;
            lx = 0f;
            ly = 1f;
        }
        else
        {
            fx = dx / len;
            fy = dy / len;
            lx = -fy;
            ly = fx;
        }

        var leftFirst = PreferLeft(player, lx, ly, FindInteriorStone(player, target, entities));
        var distances = DistancesFor(attempt);
        foreach (var (ox, oy) in DirectionOrder(attempt, leftFirst, fx, fy, lx, ly))
        {
            foreach (var dist in distances)
            {
                var px = player.X + ox * dist;
                var py = player.Y + oy * dist;
                if (!InFarm(px, py, config) || Occupied(px, py, target.Vid, entities))
                    continue;
                x = px;
                y = py;
                return true;
            }
        }

        x = player.X + (leftFirst ? lx : -lx) * 300f;
        y = player.Y + (leftFirst ? ly : -ly) * 300f;
        ClampToFarm(ref x, ref y, config);
        return Hypot(x - player.X, y - player.Y) > 1f;
    }

    private static bool PreferLeft(PlayerState player, float lx, float ly, EntityState? blocker)
    {
        if (blocker == null)
            return true;
        var left = Hypot(player.X + lx * 300f - blocker.X, player.Y + ly * 300f - blocker.Y);
        var right = Hypot(player.X - lx * 300f - blocker.X, player.Y - ly * 300f - blocker.Y);
        return left >= right;
    }

    private static float[] DistancesFor(int attempt) => attempt switch
    {
        0 => [300f, 220f, 450f],
        1 => [350f, 500f, 200f],
        _ => [250f, 400f, 200f]
    };

    private static IEnumerable<(float ox, float oy)> DirectionOrder(
        int attempt,
        bool leftFirst,
        float fx,
        float fy,
        float lx,
        float ly)
    {
        var prefer = leftFirst ? (lx, ly) : (-lx, -ly);
        var other = leftFirst ? (-lx, -ly) : (lx, ly);
        var back = (-fx, -fy);
        if (attempt <= 0)
        {
            yield return prefer;
            yield return other;
            yield return back;
        }
        else if (attempt == 1)
        {
            yield return other;
            yield return prefer;
            yield return back;
        }
        else
        {
            yield return back;
            yield return other;
            yield return prefer;
        }
    }

    private static bool Occupied(float x, float y, uint targetVid, IReadOnlyList<EntityState> entities)
    {
        var clear2 = LandClearPx * LandClearPx;
        foreach (var entity in entities)
        {
            if (entity.Vid == targetVid)
                continue;
            var dx = entity.X - x;
            var dy = entity.Y - y;
            if (dx * dx + dy * dy <= clear2)
                return true;
        }

        return false;
    }

    private static bool InFarm(float x, float y, BotConfig config)
    {
        if (config.FarmRadius <= 0)
            return true;
        var dx = x - config.FarmCenterX;
        var dy = y - config.FarmCenterY;
        return dx * dx + dy * dy <= config.FarmRadius * config.FarmRadius;
    }

    private static void ClampToFarm(ref float x, ref float y, BotConfig config)
    {
        if (config.FarmRadius <= 0)
            return;
        var dx = x - config.FarmCenterX;
        var dy = y - config.FarmCenterY;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist <= config.FarmRadius || dist < 1f)
            return;
        var scale = config.FarmRadius * 0.92f / dist;
        x = config.FarmCenterX + dx * scale;
        y = config.FarmCenterY + dy * scale;
    }

    public static float DistanceToSegment(
        float px,
        float py,
        float ax,
        float ay,
        float bx,
        float by,
        out float t)
    {
        var vx = bx - ax;
        var vy = by - ay;
        var len2 = vx * vx + vy * vy;
        if (len2 < 1f)
        {
            t = 0;
            return Hypot(px - ax, py - ay);
        }

        t = ((px - ax) * vx + (py - ay) * vy) / len2;
        var clamped = Math.Clamp(t, 0f, 1f);
        return Hypot(px - (ax + vx * clamped), py - (ay + vy * clamped));
    }

    private static float Hypot(float x, float y) => MathF.Sqrt(x * x + y * y);
}
