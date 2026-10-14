namespace FarmBot.Ui;

internal sealed class DisclosurePanel : UserControl
{
    private readonly Button _header;
    private readonly Panel _bodyHost;
    private readonly int _bodyHeight;
    private bool _expanded;

    public DisclosurePanel(string title, Control body, int bodyHeight, bool expanded = false)
    {
        _bodyHeight = bodyHeight;
        _expanded = expanded;
        Height = 36;
        Dock = DockStyle.Top;
        BackColor = UiTheme.Canvas;
        UiTheme.EnableSmooth(this);

        _header = new Button
        {
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.Card,
            ForeColor = UiTheme.Ink,
            Font = UiTheme.Ui(9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _header.FlatAppearance.BorderSize = 0;
        _header.Click += (_, _) => Expanded = !Expanded;

        body.Dock = DockStyle.Fill;
        _bodyHost = new Panel { Dock = DockStyle.Fill, Visible = expanded, BackColor = UiTheme.Card };
        _bodyHost.Controls.Add(body);

        Controls.Add(_bodyHost);
        Controls.Add(_header);
        Title = title;
        ApplyHeight();
    }

    public string Title
    {
        get => _header.Tag as string ?? "";
        set
        {
            _header.Tag = value;
            _header.Text = (_expanded ? "  ▾  " : "  ▸  ") + value;
        }
    }

    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
                return;
            _expanded = value;
            _bodyHost.Visible = value;
            Title = Title;
            ApplyHeight();
            ExpandedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ExpandedChanged;

    private void ApplyHeight() =>
        Height = _expanded ? 36 + _bodyHeight : 36;
}
