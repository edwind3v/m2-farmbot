using System.Runtime.InteropServices;

namespace FarmBot.Ui;

internal static class UiTheme
{
    public static readonly Color Canvas = Color.FromArgb(19, 21, 28);
    public static readonly Color Card = Color.FromArgb(24, 27, 36);
    public static readonly Color CardInner = Color.FromArgb(28, 32, 43);
    public static readonly Color Hairline = Color.FromArgb(52, 50, 58);
    public static readonly Color Ink = Color.FromArgb(236, 232, 223);
    public static readonly Color Muted = Color.FromArgb(139, 144, 160);
    public static readonly Color Accent = Color.FromArgb(196, 120, 58);
    public static readonly Color AccentSoft = Color.FromArgb(217, 160, 106);
    public static readonly Color Danger = Color.FromArgb(196, 92, 74);
    public static readonly Color Ok = Color.FromArgb(111, 158, 114);
    public static readonly Color Hp = Color.FromArgb(196, 92, 74);
    public static readonly Color Mp = Color.FromArgb(90, 140, 196);
    public static readonly Color Exp = Color.FromArgb(201, 162, 74);
    public static readonly Color Enemy = Color.FromArgb(196, 92, 74);
    public static readonly Color Stone = Color.FromArgb(236, 232, 223);
    public static readonly Color Boss = Color.FromArgb(201, 162, 74);

    public static Font Ui(float size, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", size, style, GraphicsUnit.Point);

    public static Font Mono(float size) =>
        new("Cascadia Mono", size, FontStyle.Regular, GraphicsUnit.Point);

    public static void StyleForm(Form form)
    {
        form.BackColor = Canvas;
        form.ForeColor = Ink;
        form.Font = Ui(9f);
        form.FormBorderStyle = FormBorderStyle.Sizable;
        form.MaximizeBox = true;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.HandleCreated += (_, _) => ApplyDarkFrame(form.Handle);
    }

    public static void StyleButton(Button button, Color back, Color fore, bool accentBorder = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = accentBorder ? Accent : Hairline;
        button.BackColor = back;
        button.ForeColor = fore;
        button.Cursor = Cursors.Hand;
        button.Font = Ui(9f);
        button.Height = 28;
        button.UseVisualStyleBackColor = false;
    }

    public static void StylePill(Button button, Color back, Color fore) =>
        StyleButton(button, back, fore, back == Accent);

    public static void StyleField(Control control)
    {
        control.BackColor = CardInner;
        control.ForeColor = Ink;
        control.Font = Ui(9f);
        if (control is TextBox box)
            box.BorderStyle = BorderStyle.FixedSingle;
        if (control is ComboBox combo)
        {
            combo.FlatStyle = FlatStyle.Flat;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        if (control is NumericUpDown numeric)
            numeric.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = CardInner;
        grid.BorderStyle = BorderStyle.None;
        grid.AllowUserToResizeRows = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Hairline;
        grid.ColumnHeadersHeight = 28;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Card;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Card;
        grid.DefaultCellStyle.BackColor = CardInner;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 196, 120, 58);
        grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.RowTemplate.Height = 24;
        typeof(DataGridView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(grid, true);
    }

    public static void EnableSmooth(Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(control, true);
    }

    public static void FillRound(Graphics g, Color color, Rectangle bounds, int radius)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, bounds);
    }

    public static System.Drawing.Drawing2D.GraphicsPath Round(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddRectangle(bounds);
        return path;
    }

    public static void ApplyDarkFrame(IntPtr handle)
    {
        try
        {
            var on = 1;
            DwmSetWindowAttribute(handle, 20, ref on, sizeof(int));
            var caption = ColorRef(Card);
            var text = ColorRef(Ink);
            var border = ColorRef(Accent);
            DwmSetWindowAttribute(handle, 35, ref caption, sizeof(int));
            DwmSetWindowAttribute(handle, 36, ref text, sizeof(int));
            DwmSetWindowAttribute(handle, 34, ref border, sizeof(int));
        }
        catch (DllNotFoundException)
        {
        }
    }

    private static int ColorRef(Color color) => color.R | (color.G << 8) | (color.B << 16);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
}
