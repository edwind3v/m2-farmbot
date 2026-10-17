using System;
using System.Collections.Generic;

// Navigation on a map's attribute grid (1 cell = 100 game units, bit 0 = blocked).
//  - Rooms: 8-connected walkable regions. Rooms whose bounding boxes lie close together form one floor (the maze floor
//    is many corridor pieces linked by portals, so they belong together); separate floors are far apart on the map.
//  - Paths: A* that keeps away from walls (clearance cost), simplified to waypoints.
//  - Exploration: cells near visited positions count as seen; the next goal is the nearest unseen cell of the room,
//    then the room's dead ends (portal candidates) that were not visited yet.
public sealed class DungeonNavigator
{
    public const int MinRoomCells = 40, FloorGap = 70, SeenRadius = 30;
    readonly int width, height;
    readonly bool[] walk;
    readonly byte[] clearance;
    readonly int[] room;      // room id per cell, -1 = not walkable / too small
    readonly int[] roomFloor; // floor id per room
    readonly List<int[]> roomBounds = new List<int[]>(); // minX,minY,maxX,maxY,cells
    readonly bool[] seen;
    readonly List<int> visitedEnds = new List<int>();
    public int Width { get { return width; } }
    public int Height { get { return height; } }
    public int RoomCount { get { return roomBounds.Count; } }
    public int FloorCount { get; private set; }

    public DungeonNavigator(int width, int height, Func<int, int, bool> blocked)
    {
        this.width = width; this.height = height;
        walk = new bool[width * height]; room = new int[width * height]; seen = new bool[width * height];
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) walk[y * width + x] = !blocked(x, y);
        clearance = ComputeClearance();
        LabelRooms();
        roomFloor = GroupFloors();
    }

    byte[] ComputeClearance()
    {
        var c = new byte[walk.Length]; var queue = new Queue<int>();
        for (int i = 0; i < walk.Length; i++) { if (!walk[i]) { c[i] = 0; queue.Enqueue(i); } else c[i] = 255; }
        while (queue.Count > 0)
        {
            int i = queue.Dequeue(), x = i % width, y = i / width;
            if (c[i] >= 6) continue;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + (d == 0 ? 1 : d == 1 ? -1 : 0), ny = y + (d == 2 ? 1 : d == 3 ? -1 : 0);
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                int n = ny * width + nx; if (c[n] <= c[i] + 1) continue;
                c[n] = (byte)(c[i] + 1); queue.Enqueue(n);
            }
        }
        return c;
    }

    void LabelRooms()
    {
        for (int i = 0; i < room.Length; i++) room[i] = -1;
        var stack = new Stack<int>(); var cells = new List<int>();
        for (int start = 0; start < walk.Length; start++)
        {
            if (!walk[start] || room[start] != -1) continue;
            cells.Clear(); stack.Push(start); room[start] = -2;
            int minX = width, minY = height, maxX = 0, maxY = 0;
            while (stack.Count > 0)
            {
                int i = stack.Pop(), x = i % width, y = i / width; cells.Add(i);
                minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy; if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    int n = ny * width + nx; if (walk[n] && room[n] == -1) { room[n] = -2; stack.Push(n); }
                }
            }
            int id = cells.Count >= MinRoomCells ? roomBounds.Count : -1;
            foreach (int i in cells) room[i] = id;
            if (id >= 0) roomBounds.Add(new[] { minX, minY, maxX, maxY, cells.Count });
        }
    }

    int[] GroupFloors()
    {
        int n = roomBounds.Count; var parent = new int[n]; for (int i = 0; i < n; i++) parent[i] = i;
        Func<int, int> find = null; find = i => parent[i] == i ? i : (parent[i] = find(parent[i]));
        for (int a = 0; a < n; a++) for (int b = a + 1; b < n; b++)
        {
            var A = roomBounds[a]; var B = roomBounds[b];
            int gx = Math.Max(0, Math.Max(A[0] - B[2], B[0] - A[2])), gy = Math.Max(0, Math.Max(A[1] - B[3], B[1] - A[3]));
            if (Math.Max(gx, gy) <= FloorGap) parent[find(a)] = find(b);
        }
        var ids = new Dictionary<int, int>(); var floor = new int[n];
        for (int i = 0; i < n; i++) { int r = find(i); if (!ids.ContainsKey(r)) ids[r] = ids.Count; floor[i] = ids[r]; }
        FloorCount = ids.Count; return floor;
    }

    public bool InBounds(int x, int y) { return x >= 0 && y >= 0 && x < width && y < height; }
    public bool Walkable(int x, int y) { return InBounds(x, y) && walk[y * width + x]; }
    public int RoomAt(int x, int y) { return InBounds(x, y) ? room[y * width + x] : -1; }
    public int FloorOfRoom(int r) { return r >= 0 && r < roomFloor.Length ? roomFloor[r] : -1; }
    // The character can stand on a blocked edge cell; the nearest room cell decides.
    public int FloorAt(int x, int y) { int c = Snap(x, y, 4); return c < 0 ? -1 : FloorOfRoom(room[c]); }
    public int[] RoomBounds(int r) { return roomBounds[r]; }

    // Nearest walkable room cell within radius, or -1.
    public int Snap(int x, int y, int radius)
    {
        int best = -1, bestD = int.MaxValue;
        for (int dy = -radius; dy <= radius; dy++) for (int dx = -radius; dx <= radius; dx++)
        {
            int nx = x + dx, ny = y + dy; if (!InBounds(nx, ny)) continue; int i = ny * width + nx;
            if (room[i] < 0) continue; int d = dx * dx + dy * dy; if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    // A* from (sx,sy) to (tx,ty) within one room; returns cell indices or null.
    public List<int> FindPath(int sx, int sy, int tx, int ty)
    {
        int s = Snap(sx, sy, 6), t = Snap(tx, ty, 8); if (s < 0 || t < 0 || room[s] != room[t]) return null;
        var g = new Dictionary<int, float>(); var from = new Dictionary<int, int>(); var open = new SortedSet<Tuple<float, int>>();
        g[s] = 0; open.Add(Tuple.Create(H(s, t), s)); int expanded = 0;
        while (open.Count > 0 && expanded++ < 400000)
        {
            var top = open.Min; open.Remove(top); int c = top.Item2; if (c == t) return Rebuild(from, t);
            int cx = c % width, cy = c / width; float gc = g[c];
            for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue; int nx = cx + dx, ny = cy + dy; if (!InBounds(nx, ny)) continue;
                int n = ny * width + nx; if (room[n] != room[s]) continue;
                if (dx != 0 && dy != 0 && (!walk[cy * width + nx] || !walk[ny * width + cx])) continue;
                float step = (dx != 0 && dy != 0 ? 1.414f : 1f) + Math.Max(0, 4 - clearance[n]) * 1.5f;
                float ng = gc + step; float old;
                if (g.TryGetValue(n, out old) && old <= ng) continue;
                if (g.ContainsKey(n)) open.Remove(Tuple.Create(old + H(n, t), n));
                g[n] = ng; from[n] = c; open.Add(Tuple.Create(ng + H(n, t), n));
            }
        }
        return null;
    }
    float H(int a, int b) { int dx = Math.Abs(a % width - b % width), dy = Math.Abs(a / width - b / width); return Math.Max(dx, dy) + 0.414f * Math.Min(dx, dy); }
    static List<int> Rebuild(Dictionary<int, int> from, int t) { var p = new List<int> { t }; int c; while (from.TryGetValue(t, out c)) { p.Add(c); t = c; } p.Reverse(); return p; }

    // Straight walkable line (used to look ahead along a path).
    public bool LineClear(int a, int b)
    {
        int x0 = a % width, y0 = a / width, x1 = b % width, y1 = b / width, dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx + dy;
        while (true)
        {
            int i = y0 * width + x0; if (!walk[i] || clearance[i] < 2) return false; if (x0 == x1 && y0 == y1) return true;
            int e2 = 2 * err; if (e2 >= dy) { err += dy; x0 += sx; } if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
    // Farthest path cell (within maxCells) that can be walked to straight from the first cell.
    public int LookAhead(List<int> path, int maxCells)
    {
        int best = Math.Min(1, path.Count - 1);
        for (int i = 1; i < path.Count && i <= maxCells; i++) if (LineClear(path[0], path[i])) best = i;
        return path[best];
    }

    public void MarkSeen(int x, int y)
    {
        for (int dy = -SeenRadius; dy <= SeenRadius; dy++) for (int dx = -SeenRadius; dx <= SeenRadius; dx++)
            if (dx * dx + dy * dy <= SeenRadius * SeenRadius && InBounds(x + dx, y + dy)) seen[(y + dy) * width + x + dx] = true;
        int here = Snap(x, y, 3); if (here >= 0) foreach (var end in DeadEnds(room[here])) if (Math.Abs(end % width - x) <= 4 && Math.Abs(end / width - y) <= 4 && !visitedEnds.Contains(end)) visitedEnds.Add(end);
    }
    public void ResetSeen() { Array.Clear(seen, 0, seen.Length); visitedEnds.Clear(); }

    // Next exploration goal in the character's room: nearest unseen open cell, else an unvisited dead end; -1 when done.
    public int NextExploreGoal(int x, int y)
    {
        int s = Snap(x, y, 6); if (s < 0) return -1;
        var dist = Bfs(s); int best = -1, bestD = int.MaxValue;
        foreach (var kv in dist) if (!seen[kv.Key] && clearance[kv.Key] >= 2 && kv.Value < bestD) { bestD = kv.Value; best = kv.Key; }
        if (best >= 0) return best;
        foreach (var end in DeadEnds(room[s])) if (!visitedEnds.Contains(end)) { int d; if (dist.TryGetValue(end, out d) && d < bestD) { bestD = d; best = end; } }
        return best;
    }
    Dictionary<int, int> Bfs(int s)
    {
        var dist = new Dictionary<int, int> { { s, 0 } }; var q = new Queue<int>(); q.Enqueue(s); int r = room[s];
        while (q.Count > 0)
        {
            int c = q.Dequeue(), cx = c % width, cy = c / width;
            for (int d = 0; d < 4; d++)
            {
                int nx = cx + (d == 0 ? 1 : d == 1 ? -1 : 0), ny = cy + (d == 2 ? 1 : d == 3 ? -1 : 0); if (!InBounds(nx, ny)) continue;
                int n = ny * width + nx; if (room[n] != r || dist.ContainsKey(n)) continue; dist[n] = dist[c] + 1; q.Enqueue(n);
            }
        }
        return dist;
    }
    readonly Dictionary<int, List<int>> deadEndCache = new Dictionary<int, List<int>>();
    // Corridor ends: cells farthest from the room centre along the room (local maxima of BFS distance from the room's
    // most central cell), kept only when well apart. On the maze floor these are where the portals sit.
    public List<int> DeadEnds(int r)
    {
        if (r < 0) return new List<int>(); List<int> cached; if (deadEndCache.TryGetValue(r, out cached)) return cached;
        var b = roomBounds[r]; int centre = Snap((b[0] + b[2]) / 2, (b[1] + b[3]) / 2, Math.Max(b[2] - b[0], b[3] - b[1]));
        var ends = new List<int>(); if (centre < 0) { deadEndCache[r] = ends; return ends; }
        var dist = Bfs(centre);
        foreach (var kv in dist)
        {
            if (clearance[kv.Key] < 2) continue; int cx = kv.Key % width, cy = kv.Key / width; bool peak = true;
            for (int dy = -6; dy <= 6 && peak; dy++) for (int dx = -6; dx <= 6 && peak; dx++) { int v; int n = (cy + dy) * width + cx + dx; if (n >= 0 && n < clearance.Length && clearance[n] >= 2 && dist.TryGetValue(n, out v) && v > kv.Value) peak = false; }
            if (peak && kv.Value >= 15 && ends.TrueForAll(e => Math.Abs(e % width - cx) + Math.Abs(e / width - cy) > 20)) ends.Add(kv.Key);
        }
        deadEndCache[r] = ends; return ends;
    }
}
