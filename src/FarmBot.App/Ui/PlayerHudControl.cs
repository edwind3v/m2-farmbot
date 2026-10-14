using FarmBot.Core.Models;

namespace FarmBot.Ui;

internal sealed class PlayerHudControl : CardPanel
{
    private PlayerState? _player;

    public PlayerHudControl()
    {
        Height = 148;
        Dock = DockStyle.Top;
        Padding = new Padding(14, 12, 14, 12);
        UiTheme.EnableSmooth(this);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void Bind(PlayerState? player)
    {
        _player = player;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        var player = _player;
        var name = string.IsNullOrWhiteSpace(player?.Name) ? "—" : player!.Name;
        var level = player is { Level: > 0 } ? player.Level.ToString() : "—";

        using var title = UiTheme.Ui(11f, FontStyle.Bold);
        using var small = UiTheme.Ui(8.5f);
        using var ink = new SolidBrush(UiTheme.Ink);
        using var muted = new SolidBrush(UiTheme.Muted);
        using var accent = new SolidBrush(UiTheme.Accent);
        g.DrawString("PLAYER", small, accent, Padding.Left, 10);
        g.DrawString(name, title, ink, Padding.Left, 26);
        g.DrawString("Lv " + level, small, muted, Padding.Left, 48);

        var y = 70;
        DrawMeter(g, y, "EXP", player?.Exp ?? 0, player?.NextExp ?? 0, UiTheme.Exp);
        DrawMeter(g, y + 22, "HP", player?.Hp ?? 0, player?.MaxHp ?? 0, UiTheme.Hp);
        DrawMeter(g, y + 44, "MP", player?.Mp ?? 0, player?.MaxMp ?? 0, UiTheme.Mp);
    }

    private void DrawMeter(Graphics g, int y, string label, int current, int max, Color color)
    {
        using var font = UiTheme.Ui(8f);
        using var muted = new SolidBrush(UiTheme.Muted);
        using var ink = new SolidBrush(UiTheme.Ink);
        g.DrawString(label, font, muted, Padding.Left, y);
        var text = max > 0 ? $"{current} / {max}" : "—";
        var size = g.MeasureString(text, font);
        var right = Width - Padding.Right;
        g.DrawString(text, font, ink, right - size.Width, y);
        var track = new Rectangle(Padding.Left + 36, y + 4, (int)(right - size.Width - Padding.Left - 44), 6);
        using (var back = new SolidBrush(UiTheme.CardInner))
            g.FillRectangle(back, track);
        using (var edge = new Pen(UiTheme.Hairline))
            g.DrawRectangle(edge, track);
        if (max > 0 && current > 0)
        {
            var fill = track with { Width = Math.Max(2, (int)(track.Width * Math.Clamp(current / (double)max, 0, 1))) };
            using var bar = new SolidBrush(color);
            g.FillRectangle(bar, fill);
        }
    }
}
