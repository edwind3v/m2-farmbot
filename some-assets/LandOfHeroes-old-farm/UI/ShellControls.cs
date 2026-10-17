using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Uygulama kabuğunun kendi çizdiği parçaları: renkler, yüzeyler, sol menü, alt sekmeler, marka, durum rozeti ve ikon düğmesi.
// Hiçbiri özellik kodu içermez; yalnızca görünüm ve gezinme sağlar.
internal static class ShellTheme
{
    public static Color Page=Color.FromArgb(243,246,251),Surface=Color.White,Raised=Color.FromArgb(247,249,253),Hover=Color.FromArgb(237,240,247),
        Border=Color.FromArgb(216,223,235),Text=Color.FromArgb(15,23,42),Muted=Color.FromArgb(100,116,139),
        Accent=Color.FromArgb(57,72,171),AccentSoft=Color.FromArgb(237,240,255),Danger=Color.FromArgb(190,42,55),DangerHover=Color.FromArgb(159,31,43);
    public static bool Night;

    public static GraphicsPath Round(RectangleF r,float radius)
    {
        var p=new GraphicsPath();float d=Math.Max(1f,radius*2f);
        if(radius<=0){p.AddRectangle(r);return p;}
        p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;
    }

    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);
    // Başlık çubuğundaki kendi çizilen parçalar da pencereyi sürükleyebilsin.
    public static void DragWindow(Control from)
    {
        var form=from.FindForm();if(form==null)return;
        ReleaseCapture();SendMessage(form.Handle,0xA1,(IntPtr)2,IntPtr.Zero);
    }

    // 16 px çizgi ikonlar; menüdeki her bölümün kendi simgesi.
    public static void DrawIcon(Graphics g,int kind,RectangleF box,Color color)
    {
        var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;g.TranslateTransform(box.X+(box.Width-16)/2f,box.Y+(box.Height-16)/2f);
        using(var pen=new Pen(color,1.6f){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round})
        using(var brush=new SolidBrush(color))
        {
            switch(kind)
            {
                case 0: // genel bakış
                    g.DrawRectangle(pen,1.5f,1.5f,5.5f,5.5f);g.DrawRectangle(pen,9f,1.5f,5.5f,5.5f);g.DrawRectangle(pen,1.5f,9f,5.5f,5.5f);g.DrawRectangle(pen,9f,9f,5.5f,5.5f);break;
                case 1: // hareket
                    g.DrawEllipse(pen,1.5f,1.5f,13,13);g.FillPolygon(brush,new[]{new PointF(8,3.8f),new PointF(10.2f,8),new PointF(8,12.2f),new PointF(5.8f,8)});break;
                case 2: // saldırı
                    g.DrawLine(pen,3,13,12.5f,3.5f);g.DrawLine(pen,9.5f,3.5f,12.5f,3.5f);g.DrawLine(pen,12.5f,3.5f,12.5f,6.5f);g.DrawLine(pen,2.5f,9.5f,6.5f,13.5f);break;
                case 3: // rota
                    g.DrawBezier(pen,3,13,3,6,13,10,13,3);g.FillEllipse(brush,1,11,4,4);g.FillEllipse(brush,11,1,4,4);break;
                case 4: // zindan
                    g.DrawLine(pen,2,14.5f,14,14.5f);g.DrawLine(pen,3,14.5f,3,5);g.DrawLine(pen,13,14.5f,13,5);g.DrawLine(pen,3,5,8,1.5f);g.DrawLine(pen,8,1.5f,13,5);g.DrawArc(pen,6,8.5f,4,4,180,180);g.DrawLine(pen,6,10.5f,6,14.5f);g.DrawLine(pen,10,10.5f,10,14.5f);break;
                case 5: // envanter
                    g.DrawRectangle(pen,2.5f,5.5f,11,9);g.DrawArc(pen,5,1.5f,6,7,180,180);break;
                case 6: // oturum
                    g.DrawEllipse(pen,5,1.5f,6,6);g.DrawArc(pen,2.5f,9.5f,11,10,180,180);break;
                case 7: // otomasyon
                    g.DrawPolygon(pen,new[]{new PointF(9,1.5f),new PointF(3.5f,9),new PointF(8,9),new PointF(7,14.5f),new PointF(12.5f,7),new PointF(8,7)});break;
                case 8: // görsel
                    g.DrawBezier(pen,1.5f,8,4,3.5f,12,3.5f,14.5f,8);g.DrawBezier(pen,1.5f,8,4,12.5f,12,12.5f,14.5f,8);g.DrawEllipse(pen,6,6,4,4);break;
                case 9: // uyumluluk
                    g.DrawPolygon(pen,new[]{new PointF(8,1.5f),new PointF(13.5f,3.5f),new PointF(13,9.5f),new PointF(8,14.5f),new PointF(3,9.5f),new PointF(2.5f,3.5f)});g.DrawLines(pen,new[]{new PointF(5.5f,8),new PointF(7.3f,9.8f),new PointF(10.5f,6)});break;
                case 10: // günlük
                    g.DrawRectangle(pen,2.5f,1.5f,11,13);g.DrawLine(pen,5,5.5f,11,5.5f);g.DrawLine(pen,5,8.5f,11,8.5f);g.DrawLine(pen,5,11.5f,9,11.5f);break;
                case 20: // ay
                    g.DrawArc(pen,2,2,12,12,-30,280);g.DrawArc(pen,5.5f,0.5f,10,10,95,140);break;
                case 21: // güneş
                    g.DrawEllipse(pen,5,5,6,6);
                    for(int i=0;i<8;i++){double a=i*Math.PI/4;g.DrawLine(pen,8+(float)Math.Cos(a)*5.2f,8+(float)Math.Sin(a)*5.2f,8+(float)Math.Cos(a)*7f,8+(float)Math.Sin(a)*7f);}
                    break;
            }
        }
        g.Restore(state);
    }
}

// Kenar çizgisi isteğe bağlı, düz renkli kap.
internal class ShellSurface : Control
{
    public bool BorderRight,BorderTop,BorderBottom;
    public ShellSurface(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);}
    protected override void OnPaint(PaintEventArgs e)
    {
        using(var pen=new Pen(ShellTheme.Border))
        {
            if(BorderRight)e.Graphics.DrawLine(pen,Width-1,0,Width-1,Height);
            if(BorderTop)e.Graphics.DrawLine(pen,0,0,Width,0);
            if(BorderBottom)e.Graphics.DrawLine(pen,0,Height-1,Width,Height-1);
        }
        base.OnPaint(e);
    }
}

// Yuvarlak köşeli bilgi kartı. "section-card" etiketi sayesinde gece/gündüz teması kartı kendi rengine boyar.
internal sealed class ShellCard : Panel
{
    public ShellCard(){Tag="section-card";SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);}
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent!=null?Parent.BackColor:ShellTheme.Surface);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using(var path=ShellTheme.Round(new RectangleF(0.5f,0.5f,Width-1.5f,Height-1.5f),10))
        using(var fill=new SolidBrush(BackColor))using(var pen=new Pen(ShellTheme.Border))
        {e.Graphics.FillPath(fill,path);e.Graphics.DrawPath(pen,path);}
    }
}

internal sealed class ShellNavItem
{
    public string Text;public int Icon;public bool GroupStart;public string Group;
}

// Sol menü: gruplu, ikonlu, seçili öğe vurgulu.
internal sealed class ShellNav : Control
{
    public readonly List<ShellNavItem> Items=new List<ShellNavItem>();
    private int selected=-1,hover=-1;
    private const int ItemHeight=30,ItemGap=1,GroupGap=20;
    public event EventHandler ItemClicked;
    public int ClickedIndex{get;private set;}
    public ShellNav()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);
        Font=new Font("Segoe UI",9f);Cursor=Cursors.Hand;TabStop=true;
    }
    public int SelectedIndex{get{return selected;}set{if(selected==value)return;selected=value;Invalidate();}}
    private Rectangle ItemBounds(int index)
    {
        int y=4;
        for(int i=0;i<Items.Count;i++)
        {
            if(Items[i].GroupStart)y+=GroupGap;
            if(i==index)return new Rectangle(10,y,Width-20,ItemHeight);
            y+=ItemHeight+ItemGap;
        }
        return Rectangle.Empty;
    }
    private int HitTest(Point p){for(int i=0;i<Items.Count;i++)if(ItemBounds(i).Contains(p))return i;return -1;}
    protected override void OnMouseMove(MouseEventArgs e){int h=HitTest(e.Location);if(h!=hover){hover=h;Invalidate();}Cursor=h>=0?Cursors.Hand:Cursors.Default;base.OnMouseMove(e);}
    protected override void OnMouseLeave(EventArgs e){hover=-1;Invalidate();base.OnMouseLeave(e);}
    protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){int h=HitTest(e.Location);if(h>=0){Focus();ClickedIndex=h;ItemClicked?.Invoke(this,EventArgs.Empty);}}base.OnMouseDown(e);}
    protected override bool IsInputKey(Keys keyData){return keyData==Keys.Up||keyData==Keys.Down||base.IsInputKey(keyData);}
    protected override void OnKeyDown(KeyEventArgs e)
    {
        int next=selected;if(e.KeyCode==Keys.Down)next=Math.Min(Items.Count-1,selected+1);else if(e.KeyCode==Keys.Up)next=Math.Max(0,selected-1);
        if(next!=selected&&next>=0){ClickedIndex=next;ItemClicked?.Invoke(this,EventArgs.Empty);e.Handled=true;}
        base.OnKeyDown(e);
    }
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;
        using(var bold=new Font(Font.FontFamily,Font.Size,FontStyle.Bold))
        using(var caption=new Font("Segoe UI Semibold",7.25f))
        {
            for(int i=0;i<Items.Count;i++)
            {
                var item=Items[i];var r=ItemBounds(i);
                if(item.GroupStart&&!string.IsNullOrEmpty(item.Group))
                    TextRenderer.DrawText(g,item.Group,caption,new Rectangle(r.X+12,r.Y-GroupGap+4,r.Width-12,GroupGap-6),ShellTheme.Muted,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
                bool sel=i==selected,hov=i==hover&&!sel;
                if(sel||hov)
                    using(var path=ShellTheme.Round(r,8))using(var fill=new SolidBrush(sel?ShellTheme.AccentSoft:ShellTheme.Hover))g.FillPath(fill,path);
                if(sel)using(var bar=ShellTheme.Round(new RectangleF(r.X,r.Y+8,3,r.Height-16),1.5f))using(var accent=new SolidBrush(ShellTheme.Accent))g.FillPath(accent,bar);
                ShellTheme.DrawIcon(g,item.Icon,new RectangleF(r.X+10,r.Y,18,r.Height),sel?ShellTheme.Accent:ShellTheme.Muted);
                TextRenderer.DrawText(g,item.Text,sel?bold:Font,new Rectangle(r.X+38,r.Y,r.Width-42,r.Height),sel||hov?ShellTheme.Text:ShellTheme.Muted,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPadding);
                if(sel&&Focused)using(var focus=new Pen(Color.FromArgb(90,ShellTheme.Accent))){var fr=r;fr.Inflate(-1,-1);using(var path=ShellTheme.Round(fr,7))g.DrawPath(focus,path);}
            }
        }
    }
}

// İçerik başlığının altındaki alt sekmeler (alt çizgili).
internal sealed class ShellTabs : Control
{
    private readonly List<string> items=new List<string>();private readonly List<Rectangle> bounds=new List<Rectangle>();
    private int selected,hover=-1;
    public event EventHandler TabClicked;public int ClickedIndex{get;private set;}
    public ShellTabs(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);Font=new Font("Segoe UI Semibold",8.75f);}
    public void SetItems(IEnumerable<string> texts,int select){items.Clear();items.AddRange(texts);selected=select;hover=-1;Measure();Invalidate();}
    public int SelectedIndex{get{return selected;}set{if(selected==value)return;selected=value;Invalidate();}}
    private void Measure()
    {
        bounds.Clear();int x=0;
        foreach(string t in items){int w=TextRenderer.MeasureText(t,Font).Width+20;bounds.Add(new Rectangle(x,0,w,Height));x+=w+4;}
    }
    protected override void OnResize(EventArgs e){Measure();base.OnResize(e);}
    private int HitTest(Point p){for(int i=0;i<bounds.Count;i++)if(bounds[i].Contains(p))return i;return -1;}
    protected override void OnMouseMove(MouseEventArgs e){int h=HitTest(e.Location);if(h!=hover){hover=h;Invalidate();}Cursor=h>=0?Cursors.Hand:Cursors.Default;base.OnMouseMove(e);}
    protected override void OnMouseLeave(EventArgs e){hover=-1;Invalidate();base.OnMouseLeave(e);}
    protected override void OnMouseDown(MouseEventArgs e){int h=HitTest(e.Location);if(h>=0&&e.Button==MouseButtons.Left){ClickedIndex=h;TabClicked?.Invoke(this,EventArgs.Empty);}base.OnMouseDown(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(BackColor);
        using(var line=new Pen(ShellTheme.Border))g.DrawLine(line,0,Height-1,Width,Height-1);
        for(int i=0;i<items.Count&&i<bounds.Count;i++)
        {
            var r=bounds[i];bool sel=i==selected;
            if(i==hover&&!sel)using(var path=ShellTheme.Round(new RectangleF(r.X+2,r.Y+4,r.Width-4,r.Height-10),6))using(var b=new SolidBrush(ShellTheme.Hover)){g.SmoothingMode=SmoothingMode.AntiAlias;g.FillPath(b,path);}
            TextRenderer.DrawText(g,items[i],Font,new Rectangle(r.X,r.Y,r.Width,r.Height-3),sel?ShellTheme.Text:ShellTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
            if(sel)using(var accent=new SolidBrush(ShellTheme.Accent))g.FillRectangle(accent,r.X+8,Height-2,r.Width-16,2);
        }
    }
}

// Logo işareti ve uygulama adı.
internal sealed class ShellBrand : Control
{
    public string Subtitle="kodbizde.com / turkmmo.com / onurergat.com / #www.onurergat.com.tr";
    public ShellBrand(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;Text="Onur";}
    protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left)ShellTheme.DragWindow(this);base.OnMouseDown(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        float top=(Height-28)/2f;
        using(var path=ShellTheme.Round(new RectangleF(0,top,28,28),8))using(var fill=new LinearGradientBrush(new PointF(0,top),new PointF(28,top+28),ShellTheme.Accent,ControlPaint.Dark(ShellTheme.Accent,0.08f)))g.FillPath(fill,path);
        using(var pen=new Pen(Color.White,2.2f)){g.DrawEllipse(pen,7.5f,top+7.5f,13,13);}
        using(var dot=new SolidBrush(Color.White))g.FillEllipse(dot,12.2f,top+12.2f,3.6f,3.6f);
        using(var name=new Font("Segoe UI Semibold",12.5f))using(var sub=new Font("Segoe UI",6.25f))
        {
            TextRenderer.DrawText(g,Text,name,new Point(36,(int)top-3),ShellTheme.Text,TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g,Subtitle,sub,new Point(37,(int)top+16),ShellTheme.Muted,TextFormatFlags.NoPadding);
        }
    }
}

// Bağlantı durumu rozeti.
internal sealed class ShellStatus : Control
{
    private bool connected;private string detail="";
    public ShellStatus(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;Font=new Font("Segoe UI Semibold",8.25f);}
    public void SetState(bool isConnected,string text){if(connected==isConnected&&detail==text)return;connected=isConnected;detail=text;Invalidate();}
    protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left)ShellTheme.DragWindow(this);base.OnMouseDown(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        int w=Math.Min(Width-1,TextRenderer.MeasureText(detail,Font).Width+34);var r=new RectangleF(Width-1-w,(Height-26)/2f,w,26);
        Color tone=connected?ShellTheme.Accent:ShellTheme.Muted;
        using(var path=ShellTheme.Round(r,13))using(var fill=new SolidBrush(connected?ShellTheme.AccentSoft:ShellTheme.Raised))using(var pen=new Pen(connected?Color.FromArgb(80,ShellTheme.Accent):ShellTheme.Border)){g.FillPath(fill,path);g.DrawPath(pen,path);}
        if(connected)using(var halo=new SolidBrush(Color.FromArgb(60,tone)))g.FillEllipse(halo,r.X+8,r.Y+8,10,10);
        using(var dot=new SolidBrush(tone))g.FillEllipse(dot,r.X+10.5f,r.Y+10.5f,5,5);
        TextRenderer.DrawText(g,detail,Font,new Rectangle((int)r.X+22,(int)r.Y,(int)r.Width-28,(int)r.Height),connected?ShellTheme.Text:ShellTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis);
    }
}

// Başlık çubuğundaki ikon düğmesi (tema geçişi).
internal sealed class ShellIconButton : Control
{
    public int Icon;private bool hover,down;
    public ShellIconButton(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw|ControlStyles.SupportsTransparentBackColor|ControlStyles.Selectable,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;TabStop=true;}
    protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hover=false;down=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnMouseDown(MouseEventArgs e){down=true;Invalidate();base.OnMouseDown(e);}
    protected override void OnMouseUp(MouseEventArgs e){down=false;Invalidate();base.OnMouseUp(e);}
    protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter){OnClick(EventArgs.Empty);e.Handled=true;}base.OnKeyDown(e);}
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;var r=new RectangleF(0.5f,0.5f,Width-1.5f,Height-1.5f);
        using(var path=ShellTheme.Round(r,8))
        {
            if(hover||down)using(var fill=new SolidBrush(down?ShellTheme.AccentSoft:ShellTheme.Hover))g.FillPath(fill,path);
            using(var pen=new Pen(Focused?ShellTheme.Accent:ShellTheme.Border))g.DrawPath(pen,path);
        }
        ShellTheme.DrawIcon(g,Icon,new RectangleF(0,0,Width,Height),hover?ShellTheme.Text:ShellTheme.Muted);
    }
}
