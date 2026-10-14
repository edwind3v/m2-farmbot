namespace FarmBot.Ui;

internal class CardPanel : Panel
{
    public int CornerRadius { get; set; } = 0;
    public Color CardColor { get; set; } = UiTheme.Card;

    public CardPanel()
    {
        UiTheme.EnableSmooth(this);
        BackColor = UiTheme.Canvas;
        Padding = new Padding(12);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;
        using (var fill = new SolidBrush(CardColor))
            g.FillRectangle(fill, bounds);
        using var pen = new Pen(UiTheme.Hairline);
        g.DrawRectangle(pen, bounds);
    }
}
