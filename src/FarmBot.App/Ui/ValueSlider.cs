namespace FarmBot.Ui;

internal sealed class ValueSlider : UserControl
{
    private float _value;
    private bool _drag;

    public string Caption { get; set; } = "";
    public float Minimum { get; set; }
    public float Maximum { get; set; } = 100;
    public string Suffix { get; set; } = "";
    public int Decimals { get; set; }

    public float Value
    {
        get => _value;
        set
        {
            var next = Math.Clamp(value, Minimum, Maximum);
            if (Math.Abs(next - _value) < 0.0001f)
                return;
            _value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ValueChanged;

    public ValueSlider()
    {
        Height = 44;
        UiTheme.EnableSmooth(this);
        BackColor = UiTheme.Card;
        ForeColor = UiTheme.Ink;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var title = UiTheme.Ui(8.5f);
        using var valueFont = UiTheme.Ui(8.5f, FontStyle.Bold);
        using var muted = new SolidBrush(UiTheme.Muted);
        using var ink = new SolidBrush(UiTheme.Ink);
        g.DrawString(Caption, title, muted, 0, 2);
        var text = Decimals > 0 ? Value.ToString("0." + new string('0', Decimals)) : Value.ToString("0");
        var label = string.IsNullOrEmpty(Suffix) ? text : text + " " + Suffix;
        var size = g.MeasureString(label, valueFont);
        g.DrawString(label, valueFont, ink, Width - size.Width, 2);

        var track = TrackBounds;
        UiTheme.FillRound(g, UiTheme.CardInner, track, 5);
        var fill = track with { Width = Math.Max(10, (int)(track.Width * Ratio)) };
        using (var bar = new SolidBrush(UiTheme.Accent))
            g.FillRectangle(bar, fill);
        var thumb = ThumbBounds;
        using var thumbBrush = new SolidBrush(UiTheme.AccentSoft);
        g.FillRectangle(thumbBrush, thumb);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _drag = true;
        SetFromX(e.X);
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_drag)
            SetFromX(e.X);
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _drag = false;
        base.OnMouseUp(e);
    }

    private void SetFromX(int x)
    {
        var track = TrackBounds;
        var ratio = track.Width <= 0 ? 0 : Math.Clamp((x - track.X) / (float)track.Width, 0, 1);
        Value = Minimum + (Maximum - Minimum) * ratio;
    }

    private float Ratio => Maximum <= Minimum ? 0 : (Value - Minimum) / (Maximum - Minimum);

    private Rectangle TrackBounds => new(0, 26, Math.Max(20, Width), 8);

    private Rectangle ThumbBounds
    {
        get
        {
            var track = TrackBounds;
            var x = track.X + (int)(track.Width * Ratio) - 7;
            return new Rectangle(Math.Clamp(x, track.X, track.Right - 14), 22, 14, 14);
        }
    }
}
