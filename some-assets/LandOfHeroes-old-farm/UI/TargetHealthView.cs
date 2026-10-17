using System.Drawing;
using System.Windows.Forms;

public sealed class TargetHealthView : Control
{
    public TargetHealthSnapshot Snapshot { get; private set; }
    public TargetHealthView(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Height=78;}
    public void ShowHealth(TargetHealthSnapshot snapshot){Snapshot=snapshot;Invalidate();}
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);var s=Snapshot;
        string title=s==null?"Canlı hedef · oyunda bir taş / canavar / patron seç":(string.IsNullOrWhiteSpace(s.Name)?"Hedef":s.Name)+" · VID "+s.Vid;
        TextRenderer.DrawText(e.Graphics,title,Font,new Rectangle(8,4,Width-16,22),ForeColor,TextFormatFlags.EndEllipsis|TextFormatFlags.VerticalCenter);
        var bar=new Rectangle(8,31,Width-16,17);
        using(var background=new SolidBrush(Color.FromArgb(80,110,120)))e.Graphics.FillRectangle(background,bar);
        if(s?.Hp!=null)using(var fill=new SolidBrush(s.Hp>30?Color.FromArgb(57,72,171):Color.FromArgb(221,77,69)))e.Graphics.FillRectangle(fill,bar.X,bar.Y,bar.Width*s.Hp.Value/100f,bar.Height);
        string label=s==null?"Güncel hedef bilgisi bekleniyor":s.Hp.HasValue?"Can: %"+s.Hp.Value.ToString("0.#")+" · canlı oyun verisi":"Can: okunamadı · ölüm olarak değerlendirilmez";
        TextRenderer.DrawText(e.Graphics,label,Font,new Rectangle(8,51,Width-16,23),ForeColor,TextFormatFlags.EndEllipsis|TextFormatFlags.VerticalCenter);
    }
}
