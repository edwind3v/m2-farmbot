using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Minimal on/off switch (track + sliding knob). It mirrors an existing check box, so every feature keeps using the
// original control's Checked value and events while only the look changes.
public sealed class ToggleSwitch : Control
{
    private bool isChecked; private float knob; private readonly Timer animation = new Timer { Interval = 12 };
    public Color OnColor = Color.FromArgb(57, 72, 171), OffColor = Color.FromArgb(214, 219, 226), KnobColor = Color.White, DisabledBlendColor = Color.White;
    public event EventHandler CheckedChanged;

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
        BackColor = Color.Transparent; Size = new Size(30, 17); Cursor = Cursors.Hand; TabStop = true;
        animation.Tick += (s, e) => { float target = isChecked ? 1f : 0f; knob += (target - knob) * 0.35f; if (Math.Abs(target - knob) < 0.02f) { knob = target; animation.Stop(); } Invalidate(); };
    }
    public bool Checked
    {
        get { return isChecked; }
        set { if (isChecked == value) return; isChecked = value; animation.Start(); CheckedChanged?.Invoke(this, EventArgs.Empty); }
    }
    public void SetSilently(bool value) { if (isChecked == value) return; isChecked = value; knob = value ? 1f : 0f; Invalidate(); }
    protected override void OnClick(EventArgs e) { if (Enabled) { Focus(); Checked = !Checked; } base.OnClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)) { Checked = !Checked; e.Handled = true; } base.OnKeyDown(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void Dispose(bool disposing) { if (disposing) animation.Dispose(); base.Dispose(disposing); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        float h = Height - 1, w = Width - 1, r = h / 2f;
        Color track = Blend(OffColor, OnColor, knob); if (!Enabled) track = Blend(track, DisabledBlendColor, 0.5f);
        using (var path = new GraphicsPath())
        {
            path.AddArc(0, 0, h, h, 90, 180); path.AddArc(w - h, 0, h, h, 270, 180); path.CloseFigure();
            using (var brush = new SolidBrush(track)) g.FillPath(brush, path);
            if (Focused) using (var pen = new Pen(Color.FromArgb(90, OnColor), 1.5f)) g.DrawPath(pen, path);
        }
        float d = h - 4f, x = 2f + (w - h) * knob;
        using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0))) g.FillEllipse(shadow, x, 3f, d, d);
        using (var knobBrush = new SolidBrush(KnobColor)) g.FillEllipse(knobBrush, x, 2f, d, d);
    }
    private static Color Blend(Color a, Color b, float t)
    {
        t = Math.Max(0, Math.Min(1, t));
        return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }
}
