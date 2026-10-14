using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Ui;

internal sealed class MiniMapControl : CardPanel
{
    private WorldSnapshot? _snapshot;
    private EntityState? _target;
    private float _farmRadius = 1500;
    private HashSet<uint> _bossVnums = new();
    private readonly ToolTip _tip = new() { ShowAlways = true, AutoPopDelay = 4000 };
    private string _lastTip = "";

    public MiniMapControl()
    {
        Height = 260;
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        UiTheme.EnableSmooth(this);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        MouseMove += OnHover;
        MouseLeave += (_, _) => _tip.Hide(this);
    }

    public void Bind(WorldSnapshot? snapshot, EntityState? target, float farmRadius, IEnumerable<uint>? bossVnums)
    {
        _snapshot = snapshot;
        _target = target;
        _farmRadius = farmRadius > 1 ? farmRadius : 1500;
        _bossVnums = bossVnums?.ToHashSet() ?? new HashSet<uint>();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var map = MapBounds;
        using (var brush = new SolidBrush(Color.FromArgb(255, 14, 16, 22)))
            g.FillEllipse(brush, map);
        using (var ring = new Pen(UiTheme.Hairline, 1f))
            g.DrawEllipse(ring, map);

        var player = _snapshot?.Player;
        var radius = Math.Max(200f, _farmRadius);
        var cx = map.X + map.Width / 2f;
        var cy = map.Y + map.Height / 2f;
        var scale = (map.Width / 2f - 8) / radius;

        using (var farmPen = new Pen(Color.FromArgb(70, 196, 120, 58), 1f))
            g.DrawEllipse(farmPen, cx - radius * scale, cy - radius * scale, radius * scale * 2, radius * scale * 2);

        if (_snapshot != null)
        {
            foreach (var entity in _snapshot.Entities)
            {
                if (!TryColor(entity, out var color))
                    continue;
                var dx = (entity.X - (player?.X ?? 0)) * scale;
                var dy = (entity.Y - (player?.Y ?? 0)) * scale;
                if (dx * dx + dy * dy > (map.Width / 2f - 6) * (map.Width / 2f - 6))
                    continue;
                var size = entity.Vid == _target?.Vid ? 7f : 5f;
                var rect = new RectangleF(cx + dx - size / 2, cy - dy - size / 2, size, size);
                using var fill = new SolidBrush(color);
                g.FillEllipse(fill, rect);
                if (entity.Vid == _target?.Vid)
                {
                    using var pen = new Pen(UiTheme.Accent, 1.4f);
                    g.DrawEllipse(pen, rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
                }
            }
        }

        var chevron = new[]
        {
            new PointF(cx, cy - 6),
            new PointF(cx + 5, cy + 5),
            new PointF(cx, cy + 2),
            new PointF(cx - 5, cy + 5)
        };
        using (var playerBrush = new SolidBrush(UiTheme.Accent))
            g.FillPolygon(playerBrush, chevron);

        using var caption = UiTheme.Ui(8f);
        using var muted = new SolidBrush(UiTheme.Muted);
        g.DrawString("mobs   stones   bosses", caption, muted, map.X + 8, map.Bottom + 6);
        using var red = new SolidBrush(UiTheme.Enemy);
        using var white = new SolidBrush(UiTheme.Stone);
        using var gold = new SolidBrush(UiTheme.Boss);
        g.FillEllipse(red, map.X + 10, map.Bottom + 9, 6, 6);
        g.FillEllipse(white, map.X + 68, map.Bottom + 9, 6, 6);
        g.FillEllipse(gold, map.X + 132, map.Bottom + 9, 6, 6);
    }

    private bool TryColor(EntityState entity, out Color color)
    {
        if (entity.Kind == EntityKind.Boss || _bossVnums.Contains(entity.Vnum))
        {
            color = UiTheme.Boss;
            return true;
        }

        if (entity.ResolvedType == ActorType.Stone || entity.Kind == EntityKind.Metin)
        {
            color = UiTheme.Stone;
            return true;
        }

        if (entity.ResolvedType == ActorType.Enemy || entity.Kind is EntityKind.Monster)
        {
            color = UiTheme.Enemy;
            return true;
        }

        color = default;
        return false;
    }

    private void OnHover(object? sender, MouseEventArgs e)
    {
        if (_snapshot == null || !MapBounds.Contains(e.Location))
        {
            if (_lastTip.Length > 0)
            {
                _tip.Hide(this);
                _lastTip = "";
            }
            return;
        }

        var hit = HitTest(e.Location);
        var text = hit == null
            ? ""
            : string.IsNullOrWhiteSpace(hit.Name)
                ? $"{EntityMapping.TypeName(hit.ResolvedType)}  VNUM {hit.Vnum}  {hit.Distance:0}"
                : $"{hit.Name}  VNUM {hit.Vnum}  {hit.Distance:0}";
        if (text == _lastTip)
            return;
        _lastTip = text;
        if (text.Length == 0)
            _tip.Hide(this);
        else
            _tip.Show(text, this, e.X + 12, e.Y + 12);
    }

    private EntityState? HitTest(Point point)
    {
        var player = _snapshot?.Player;
        var map = MapBounds;
        var radius = Math.Max(200f, _farmRadius);
        var cx = map.X + map.Width / 2f;
        var cy = map.Y + map.Height / 2f;
        var scale = (map.Width / 2f - 8) / radius;
        EntityState? best = null;
        var bestDist = 12f;
        foreach (var entity in _snapshot?.Entities ?? Array.Empty<EntityState>())
        {
            if (!TryColor(entity, out _))
                continue;
            var dx = (entity.X - (player?.X ?? 0)) * scale;
            var dy = (entity.Y - (player?.Y ?? 0)) * scale;
            var px = cx + dx;
            var py = cy - dy;
            var dist = MathF.Sqrt((point.X - px) * (point.X - px) + (point.Y - py) * (point.Y - py));
            if (dist < bestDist)
            {
                bestDist = dist;
                best = entity;
            }
        }

        return best;
    }

    private Rectangle MapBounds
    {
        get
        {
            var size = Math.Min(Width - Padding.Horizontal, Height - Padding.Vertical - 22);
            var x = (Width - size) / 2;
            return new Rectangle(x, Padding.Top, size, size);
        }
    }
}
