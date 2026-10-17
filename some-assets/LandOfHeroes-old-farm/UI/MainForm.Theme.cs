using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Guna.UI2.WinForms;

// Compact, minimal light look. Runs once after every panel is built: the window is scaled down, every colour is
// moved to a neutral light palette by its role, check boxes are shown as switches, and only the main actions keep a
// filled accent button. Controls, handlers and features are untouched; the original check boxes keep working behind
// their switches.
public partial class MainForm
{
    private const float CompactScale=0.75f;
    private static readonly Color LightPage=Color.FromArgb(243,246,251),LightSurface=Color.White,LightRaised=Color.FromArgb(247,249,253),
        LightBorder=Color.FromArgb(216,223,235),LightText=Color.FromArgb(15,23,42),LightMuted=Color.FromArgb(100,116,139),
        LightAccent=Color.FromArgb(57,72,171),LightAccentHover=Color.FromArgb(43,55,142),LightAccentSoft=Color.FromArgb(237,240,255);

    private void ApplyCompactLightTheme()
    {
        SuspendLayout();
        MinimumSize=Size.Empty;MaximumSize=Size.Empty;
        var tabs=mainTabs.ItemSize;
        Scale(new SizeF(CompactScale,CompactScale));
        mainTabs.ItemSize=new Size((int)(tabs.Width*CompactScale),(int)(tabs.Height*CompactScale));
        ClientSize=new Size((int)(1280*CompactScale),(int)(860*CompactScale));MinimumSize=Size;MaximumSize=Size;
        BackColor=LightPage;ForeColor=LightText;
        Restyle(this);
        StyleHeader();
        StyleTabs();
        clientPicker.SetBounds(connectBtn.Left,connectBtn.Top-36,connectBtn.Width,24);
        // Client list drawn flat and white like the other inputs.
        clientPicker.DrawMode=DrawMode.OwnerDrawFixed;clientPicker.ItemHeight=18;clientPicker.FlatStyle=FlatStyle.Flat;clientPicker.BackColor=Color.White;
        clientPicker.DrawItem+=(s,e)=>
        {
            if(e.Index<0)return;bool selected=(e.State&DrawItemState.Selected)!=0&&(e.State&DrawItemState.ComboBoxEdit)==0;
            using(var back=new SolidBrush(selected?CurrentThemeAccentSoft:CurrentThemeInput))e.Graphics.FillRectangle(back,e.Bounds);
            TextRenderer.DrawText(e.Graphics,clientPicker.Items[e.Index].ToString(),clientPicker.Font,new Rectangle(e.Bounds.X+6,e.Bounds.Y,e.Bounds.Width-6,e.Bounds.Height),CurrentThemeText,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
        };
        leftPanel.FillColor=LightSurface;leftPanel.BorderColor=LightBorder;leftPanel.BorderThickness=1;leftPanel.ShadowDecoration.Enabled=false;
        PolishInputs(this);
        StyleButtons(this);
        MirrorCheckBoxes(this);
        radarPanel.BackgroundImage=RadarPlaceholder(radarPanel.Width,radarPanel.Height);radarPanel.BackgroundImageLayout=ImageLayout.Center;
        foreach(TabPage page in mainTabs.TabPages)ResolveOverlaps(page);
        // Left column order: radar, client list, connect, start, then the status lines.
        clientPicker.SetBounds(radarPanel.Left,radarPanel.Bottom+10,radarPanel.Width,24);
        connectBtn.Top=clientPicker.Bottom+10;startStopFarmBtn.Top=connectBtn.Bottom+10;
        if(connectionStatus!=null)connectionStatus.Top=startStopFarmBtn.Bottom+12;
        var timingLines=leftPanel.Controls.Find("timingStatus",true);if(timingLines.Length>0&&connectionStatus!=null)timingLines[0].Top=connectionStatus.Bottom+12;
        ResolveOverlaps(leftPanel);
        // Task page: the scaled title must not run into its description line.
        foreach(Control c in taskPage.Controls)if(c is Label l&&l.Font.Size>=12f){foreach(Control d in taskPage.Controls)if(d is Label info&&info!=l&&info.Top>l.Top&&info.Top<l.Bottom+4)info.Top=l.Bottom+4;}
        ResumeLayout(true);
    }

    private void StyleHeader()
    {
        headerPanel.FillColor=LightSurface;headerPanel.BackColor=LightSurface;
        foreach(Control c in headerPanel.Controls)
        {
            if(!(c is Label))continue;c.BackColor=LightSurface;
            if(c!=rndTitleLbl&&c!=pidLbl&&c.Top>rndTitleLbl.Bottom-4)c.Visible=false; // the breadcrumb line is noise
        }
        rndTitleLbl.Text="Onur";rndTitleLbl.ForeColor=LightText;rndTitleLbl.Font=new Font("Segoe UI Semibold",15f);
        rndTitleLbl.Top=(headerPanel.Height-rndTitleLbl.Height)/2;
        pidLbl.ForeColor=LightMuted;pidLbl.Font=new Font("Consolas",9f);
        var line=new Panel{BackColor=LightBorder,Height=1,Dock=DockStyle.Bottom};headerPanel.Controls.Add(line);
    }

    private void StyleTabs()
    {
        mainTabs.TabMenuBackColor=LightPage;
        mainTabs.TabButtonIdleState.FillColor=LightPage;mainTabs.TabButtonIdleState.ForeColor=LightMuted;mainTabs.TabButtonIdleState.InnerColor=LightPage;
        mainTabs.TabButtonHoverState.FillColor=Color.FromArgb(238,240,243);mainTabs.TabButtonHoverState.ForeColor=LightText;mainTabs.TabButtonHoverState.InnerColor=Color.FromArgb(238,240,243);
        mainTabs.TabButtonSelectedState.FillColor=LightSurface;mainTabs.TabButtonSelectedState.ForeColor=LightText;mainTabs.TabButtonSelectedState.InnerColor=LightAccent;
        var font=new Font("Segoe UI",9f);var bold=new Font("Segoe UI Semibold",9f);
        mainTabs.TabButtonIdleState.Font=font;mainTabs.TabButtonHoverState.Font=font;mainTabs.TabButtonSelectedState.Font=bold;
        string[] names={"Hareket","Saldırı","Rota","Envanter","Oturum","Performans","Geliştirici","Uyumluluk","Zindan görevi"};
        for(int i=0;i<mainTabs.TabPages.Count&&i<names.Length;i++)mainTabs.TabPages[i].Text="   "+names[i];
        var icons=new ImageList{ImageSize=new Size(16,16),ColorDepth=ColorDepth.Depth32Bit};
        for(int i=0;i<9;i++)icons.Images.Add(TabIcon(i));
        mainTabs.ImageList=icons;for(int i=0;i<mainTabs.TabPages.Count&&i<9;i++)mainTabs.TabPages[i].ImageIndex=i;
    }

    private static Font Scaled(Font f){return f==null?null:new Font(f.FontFamily,Math.Max(7.5f,f.Size*0.8f),f.Style);}
    private static float Luma(Color c){return (0.299f*c.R+0.587f*c.G+0.114f*c.B)/255f;}
    private static bool IsAccent(Color c){return (c.G>=130&&c.G>c.R+35&&c.G>c.B+5)||(c.B>=130&&c.B>c.R+35);}
    private static Color MapFill(Color c,Color fallback)
    {
        if(c.A==0||c==Color.Transparent)return c;
        if(IsAccent(c))return LightAccent;
        float l=Luma(c);
        if(l<0.07f)return LightPage;if(l<0.13f)return LightSurface;if(l<0.30f)return LightRaised;
        return l>0.85f?c:fallback;
    }
    private static Color MapText(Color c)
    {
        if(c.A==0)return c;if(IsAccent(c))return LightAccent;
        float l=Luma(c);if(l>0.75f)return LightText;if(l>0.45f)return LightMuted;return c;
    }

    private static void Restyle(Control parent)
    {
        foreach(Control c in parent.Controls)
        {
            c.Font=Scaled(c.Font);
            if(c is Guna2Panel gp){gp.FillColor=MapFill(gp.FillColor,LightSurface);gp.BackColor=MapFill(gp.BackColor,LightPage);gp.BorderColor=LightBorder;gp.ShadowDecoration.Enabled=false;}
            else if(c is Guna2TrackBar gt){gt.ThumbColor=LightAccent;gt.FillColor=Color.FromArgb(226,229,234);gt.HoverState.ThumbColor=LightAccentHover;}
            else if(c is DataGridView grid)
            {
                grid.BackgroundColor=LightSurface;grid.GridColor=LightBorder;grid.EnableHeadersVisualStyles=false;grid.BorderStyle=BorderStyle.None;
                grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;
                grid.DefaultCellStyle.BackColor=LightSurface;grid.DefaultCellStyle.ForeColor=LightText;
                grid.DefaultCellStyle.SelectionBackColor=LightAccentSoft;grid.DefaultCellStyle.SelectionForeColor=LightText;
                grid.AlternatingRowsDefaultCellStyle.BackColor=LightSurface;grid.AlternatingRowsDefaultCellStyle.ForeColor=LightText;
                grid.ColumnHeadersDefaultCellStyle.BackColor=LightSurface;grid.ColumnHeadersDefaultCellStyle.ForeColor=LightMuted;
                grid.RowTemplate.Height=(int)(grid.RowTemplate.Height*CompactScale)+4;
                foreach(DataGridViewColumn col in grid.Columns)col.Width=(int)(col.Width*CompactScale);
                foreach(DataGridViewRow row in grid.Rows)row.DefaultCellStyle.ForeColor=MapText(row.DefaultCellStyle.ForeColor.IsEmpty?LightText:row.DefaultCellStyle.ForeColor);
            }
            else if(c is TextBox||c is NumericUpDown||c is ComboBox){c.BackColor=Color.White;c.ForeColor=LightText;}
            else if(c is ListBox lb)
            {
                lb.BackColor=Color.White;lb.ForeColor=LightText;lb.BorderStyle=BorderStyle.FixedSingle;
                if(lb.DrawMode!=DrawMode.Normal)lb.ItemHeight=Math.Max(18,(int)(lb.ItemHeight*CompactScale));
            }
            else if(c is TabPage tp){tp.BackColor=LightSurface;tp.ForeColor=LightText;}
            else if(c.Name=="radarPanel"){}
            else if(c is Label lab)
            {
                if(!lab.AutoSize){int need=TextRenderer.MeasureText("Ag",lab.Font).Height+2;if(lab.Height<need)lab.Height=need;}
                if(lab.BackColor!=Color.Transparent)lab.BackColor=MapFill(lab.BackColor,lab.BackColor);
                // Upper-case section captions become small grey headings instead of accent-coloured shouting.
                bool caption=lab.Text.Length>3&&lab.Text==lab.Text.ToUpper(new System.Globalization.CultureInfo("tr-TR"))&&lab.Text.Any(char.IsLetter);
                lab.ForeColor=caption?LightMuted:MapText(lab.ForeColor);
                if(caption)lab.Font=new Font("Segoe UI Semibold",Math.Max(7.5f,lab.Font.Size-0.5f));
            }
            else if(!(c is Guna2Button)&&!(c is Guna2CheckBox))
            {
                c.BackColor=MapFill(c.BackColor,c.BackColor);c.ForeColor=MapText(c.ForeColor);
            }
            Restyle(c);
        }
    }

    // Primary actions are filled accent buttons; everything else is a quiet outline button.
    private static readonly HashSet<string> PrimaryButtons=new HashSet<string>{"connectBtn","startStopFarmBtn"};
    private void StyleButtons(Control parent)
    {
        foreach(Control c in parent.Controls)
        {
            if(c is Guna2Button gb)
            {
                bool primary=PrimaryButtons.Contains(gb.Name)||gb==taskRun;
                gb.BorderRadius=8;gb.ShadowDecoration.Enabled=false;gb.Cursor=Cursors.Hand;gb.Font=new Font("Segoe UI Semibold",8.5f);
                if(primary){gb.FillColor=LightAccent;gb.ForeColor=Color.White;gb.BorderThickness=0;gb.HoverState.FillColor=LightAccentHover;gb.HoverState.ForeColor=Color.White;}
                else{gb.FillColor=Color.White;gb.ForeColor=LightText;gb.BorderColor=LightBorder;gb.BorderThickness=1;gb.HoverState.FillColor=LightRaised;gb.HoverState.ForeColor=LightText;gb.HoverState.BorderColor=Color.FromArgb(200,205,212);}
                gb.DisabledState.FillColor=Color.FromArgb(243,244,246);gb.DisabledState.ForeColor=Color.FromArgb(156,163,175);gb.DisabledState.BorderColor=LightBorder;
            }
            else if(c is Button b)
            {
                b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=LightBorder;b.FlatAppearance.BorderSize=1;
                b.FlatAppearance.MouseOverBackColor=LightRaised;b.FlatAppearance.MouseDownBackColor=Color.FromArgb(240,242,245);
                b.BackColor=Color.White;b.ForeColor=LightText;b.Cursor=Cursors.Hand;b.Font=new Font("Segoe UI Semibold",8.5f);
            }
            StyleButtons(c);
        }
    }

    // Inputs: borderless fields inside rounded outlines drawn by their parent; the outline turns accent while focused.
    private static readonly HashSet<Control> outlinedParents=new HashSet<Control>();
    private static void PolishInputs(Control parent)
    {
        foreach(Control c in parent.Controls)
        {
            if(c is TextBox tb&&!tb.Multiline){tb.BorderStyle=BorderStyle.None;Outline(tb);}
            else if(c is NumericUpDown nu){nu.BorderStyle=BorderStyle.None;Outline(nu);}
            else if(c is ComboBox cb){cb.FlatStyle=FlatStyle.Flat;cb.BackColor=Color.White;}
            PolishInputs(c);
        }
    }
    private static void Outline(Control field)
    {
        var parent=field.Parent;if(parent==null)return;
        field.Enter+=(s,e)=>parent.Invalidate();field.Leave+=(s,e)=>parent.Invalidate();
        if(!outlinedParents.Add(parent))return;
        parent.Paint+=(s,e)=>
        {
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            foreach(Control c in parent.Controls)
            {
                if(!c.Visible||!((c is TextBox t&&t.BorderStyle==BorderStyle.None&&!t.Multiline)||(c is NumericUpDown n&&n.BorderStyle==BorderStyle.None)))continue;
                var r=Rectangle.Inflate(c.Bounds,6,5);bool focus=c.Focused||c.ContainsFocus;
                using(var path=Rounded(r,7))
                {
                    using(var fill=new SolidBrush(CurrentThemeInput))e.Graphics.FillPath(fill,path);
                    if(focus)using(var ring=new Pen(Color.FromArgb(60,CurrentThemeAccent),3f))e.Graphics.DrawPath(ring,path);
                    using(var pen=new Pen(focus?CurrentThemeAccent:CurrentThemeBorder,1f))e.Graphics.DrawPath(pen,path);
                }
            }
        };
        parent.Invalidate();
    }
    private static GraphicsPath Rounded(Rectangle r,int radius)
    {
        var p=new GraphicsPath();int d=radius*2;
        p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;
    }

    // Check boxes are drawn as switches; the original (hidden) box keeps its value, events and enabled state.
    private static readonly MethodInfo ControlGetState=typeof(Control).GetMethod("GetState",BindingFlags.Instance|BindingFlags.NonPublic);
    private static bool OwnVisible(Control c){return (bool)ControlGetState.Invoke(c,new object[]{2});}
    private void MirrorCheckBoxes(Control parent)
    {
        foreach(Control c in parent.Controls.Cast<Control>().ToList())
        {
            if((c is Guna2CheckBox||c is CheckBox)&&!(parent is DataGridView))MirrorCheckBox(c);
            else MirrorCheckBoxes(c);
        }
    }
    private void MirrorCheckBox(Control box)
    {
        Func<bool> get=()=>box is CheckBox cb?cb.Checked:((Guna2CheckBox)box).Checked;
        Action<bool> set=v=>{if(box is CheckBox cb)cb.Checked=v;else((Guna2CheckBox)box).Checked=v;};
        var bounds=box.Bounds;var parent=box.Parent;
        var sw=new ToggleSwitch{OnColor=CurrentThemeAccent,OffColor=CurrentThemeBorder,Location=new Point(bounds.Left,bounds.Top+(bounds.Height-17)/2),Enabled=box.Enabled};
        var caption=new Label{Text=box.Text,AutoSize=false,AutoEllipsis=true,Location=new Point(bounds.Left+38,bounds.Top),Size=new Size(CaptionWidth(box,bounds),Math.Max(bounds.Height,18)),
            TextAlign=ContentAlignment.MiddleLeft,Font=new Font("Segoe UI",Math.Max(8f,box.Font.Size)),ForeColor=CurrentThemeText,BackColor=Color.Transparent,Cursor=Cursors.Hand,Enabled=box.Enabled};
        sw.SetSilently(get());
        bool syncing=false;
        sw.CheckedChanged+=(s,e)=>{if(syncing)return;syncing=true;try{set(sw.Checked);}finally{syncing=false;}sw.SetSilently(get());};
        caption.Click+=(s,e)=>{if(sw.Enabled)sw.Checked=!sw.Checked;};
        EventHandler changed=(s,e)=>{if(!syncing)sw.SetSilently(get());};
        if(box is CheckBox plain)plain.CheckedChanged+=changed;else((Guna2CheckBox)box).CheckedChanged+=changed;
        box.TextChanged+=(s,e)=>caption.Text=box.Text;
        box.EnabledChanged+=(s,e)=>{sw.Enabled=box.Enabled;caption.Enabled=box.Enabled;caption.ForeColor=box.Enabled?CurrentThemeText:CurrentThemeMuted;};
        // Check box visibility is only decided while the panels are built (before this pass), so the box can simply
        // be hidden: it keeps its value and events, the switch shows it.
        sw.Visible=caption.Visible=OwnVisible(box);
        box.TabStop=false;box.Visible=false;
        // Some panels re-show their options later; the box is hidden again at once and its switch shown instead.
        box.VisibleChanged+=(s,e)=>{if(OwnVisible(box)){box.Visible=false;sw.Visible=caption.Visible=true;}};
        sw.Tag=box;caption.Tag=box;parent.Controls.Add(sw);parent.Controls.Add(caption);sw.BringToFront();caption.BringToFront();
    }

    // After scaling, captions grew to fit their font and switches replaced boxes, so a few controls ended up on top
    // of each other. Each visible control that overlaps one placed above it is moved just below that control.
    private static void ResolveOverlaps(Control parent)
    {
        var items=parent.Controls.Cast<Control>().Where(c=>OwnVisible(c)&&c.Width>2&&c.Height>2).OrderBy(c=>c.Top).ThenBy(c=>c.Left).ToList();
        for(int i=0;i<items.Count;i++)
        {
            bool moved=true;int guard=0;
            while(moved&&guard++<20)
            {
                moved=false;var a=items[i];
                for(int j=0;j<i;j++)
                {
                    var b=items[j];var ra=Rectangle.Inflate(a.Bounds,-1,-1);
                    if(!ra.IntersectsWith(b.Bounds))continue;
                    // A switch and its caption, or a control sitting inside a section card, belong together.
                    if(Convert.ToString(b.Tag)=="section-card"&&b.Bounds.Contains(a.Bounds))continue;
                    a.Top=b.Bottom+4;moved=true;
                }
            }
        }
    }

    // A switch caption is as wide as its text needs (within the panel), never narrower than the original box text area.
    private static int CaptionWidth(Control box,Rectangle bounds)
    {
        int text=TextRenderer.MeasureText(box.Text,new Font("Segoe UI",Math.Max(8f,box.Font.Size))).Width+6;
        return Math.Max(20,Math.Max(bounds.Width-38,text));
    }

    private static Bitmap TabIcon(int kind)
    {
        var b=new Bitmap(16,16);
        using(var g=Graphics.FromImage(b))
        using(var pen=new Pen(LightMuted,1.5f){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round})
        using(var brush=new SolidBrush(LightMuted))
        {
            g.SmoothingMode=SmoothingMode.AntiAlias;
            switch(kind)
            {
                case 0:g.DrawEllipse(pen,1.5f,1.5f,13,13);g.FillPolygon(brush,new[]{new PointF(8,4),new PointF(10,8),new PointF(8,12),new PointF(6,8)});break;
                case 1:g.DrawLine(pen,3,13,12,4);g.DrawLine(pen,9.5f,3.5f,12.5f,6.5f);g.DrawLine(pen,3.5f,10,6,12.5f);break;
                case 2:g.DrawBezier(pen,3,13,3,5,13,11,13,3);g.FillEllipse(brush,1.5f,11.5f,3,3);g.FillEllipse(brush,11.5f,1.5f,3,3);break;
                case 3:g.DrawRectangle(pen,2.5f,5.5f,11,8.5f);g.DrawArc(pen,5,2,6,6,180,180);break;
                case 4:g.DrawEllipse(pen,5,1.5f,6,6);g.DrawArc(pen,2.5f,9,11,10,180,180);break;
                case 5:g.DrawArc(pen,1.5f,3.5f,13,13,180,180);g.DrawLine(pen,8,10,11.5f,6);break;
                case 6:g.DrawPolygon(pen,new[]{new PointF(8,1.5f),new PointF(13.5f,4),new PointF(13,10),new PointF(8,14.5f),new PointF(3,10),new PointF(2.5f,4)});break;
                default:g.DrawRectangle(pen,2.5f,1.5f,11,13);g.DrawLine(pen,5,6,11,6);g.DrawLine(pen,5,9,11,9);g.DrawLine(pen,5,12,9,12);break;
            }
        }
        return b;
    }

    private static Bitmap RadarPlaceholder(int w,int h)
    {
        var b=new Bitmap(Math.Max(1,w),Math.Max(1,h));
        using(var g=Graphics.FromImage(b))
        {
            g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(CurrentThemeInput);
            float cx=w/2f,cy=h/2f,r=Math.Min(w,h)/2f-8f;
            using(var disc=new SolidBrush(CurrentThemeNight?NightRaised:Color.FromArgb(243,246,248)))g.FillEllipse(disc,cx-r,cy-r,r*2,r*2);
            using(var pen=new Pen(CurrentThemeBorder,1f)){for(int k=1;k<=3;k++){float rr=r*k/3f;g.DrawEllipse(pen,cx-rr,cy-rr,rr*2,rr*2);}g.DrawLine(pen,cx-r,cy,cx+r,cy);g.DrawLine(pen,cx,cy-r,cx,cy+r);}
            using(var sweep=new LinearGradientBrush(new PointF(cx,cy-r),new PointF(cx+r,cy),Color.FromArgb(70,CurrentThemeAccent),Color.FromArgb(0,CurrentThemeAccent)))g.FillPie(sweep,cx-r,cy-r,r*2,r*2,-90,50);
            using(var dot=new SolidBrush(CurrentThemeAccent))g.FillEllipse(dot,cx-3,cy-3,6,6);
            using(var font=new Font("Segoe UI",8f))using(var text=new SolidBrush(CurrentThemeMuted))
            {var fmt=new StringFormat{Alignment=StringAlignment.Center};g.DrawString("Oyuna bağlanınca canlı harita",font,text,new RectangleF(0,cy+r*0.45f,w,30),fmt);}
        }
        return b;
    }
}
