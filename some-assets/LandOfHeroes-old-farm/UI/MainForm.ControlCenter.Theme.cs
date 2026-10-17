using System;
using System.Drawing;
using System.Windows.Forms;

public partial class MainForm
{
    private void DrawControlCenterTab(object sender,DrawItemEventArgs e)
    {
        if(controlCenterTabs==null||e.Index<0||e.Index>=controlCenterTabs.TabPages.Count)return;
        bool selected=e.Index==controlCenterTabs.SelectedIndex;
        Color surface=CurrentThemeNight?NightSurface:LightSurface;
        Color raised=CurrentThemeNight?NightRaised:LightRaised;
        Color back=selected?surface:raised;
        using(var brush=new SolidBrush(back))e.Graphics.FillRectangle(brush,e.Bounds);
        if(selected)using(var accent=new SolidBrush(CurrentThemeAccent))e.Graphics.FillRectangle(accent,e.Bounds.Left,e.Bounds.Bottom-3,e.Bounds.Width,3);
        var bounds=new Rectangle(e.Bounds.Left+8,e.Bounds.Top,e.Bounds.Width-16,e.Bounds.Height-3);
        TextRenderer.DrawText(e.Graphics,controlCenterTabs.TabPages[e.Index].Text,new Font("Segoe UI Semibold",9f),bounds,selected?CurrentThemeText:CurrentThemeMuted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
    }

    private void ApplyControlCenterAppearance()
    {
        if(controlCenterForm==null||controlCenterForm.IsDisposed)return;
        Color page=nightTheme?NightPage:LightPage;
        Color surface=nightTheme?NightSurface:LightSurface;
        Color raised=nightTheme?NightRaised:LightRaised;
        Color input=nightTheme?NightInput:Color.White;
        Color border=nightTheme?NightBorder:LightBorder;
        Color text=nightTheme?NightText:LightText;
        Color muted=nightTheme?NightMuted:LightMuted;
        Color accent=nightTheme?NightAccent:LightAccent;
        Color hover=nightTheme?NightAccentHover:LightAccentHover;
        Color soft=nightTheme?NightAccentSoft:LightAccentSoft;
        controlCenterForm.SuspendLayout();
        controlCenterForm.BackColor=page;controlCenterForm.ForeColor=text;
        if(controlCenterHeader!=null)
        {
            controlCenterHeader.BackColor=surface;
            foreach(Control c in controlCenterHeader.Controls)c.ForeColor=c.Top<38?text:muted;
        }
        if(controlCenterTabs!=null)
        {
            controlCenterTabs.BackColor=page;controlCenterTabs.ForeColor=text;
            foreach(TabPage tab in controlCenterTabs.TabPages){tab.BackColor=surface;tab.ForeColor=text;}
            controlCenterTabs.Invalidate();
        }
        ApplyAppearanceToTree(controlCenterForm,page,surface,raised,input,border,text,muted,accent,hover,soft);
        foreach(Control c in controlCenterForm.Controls)ApplyControlCenterDetails(c,border,accent,soft,text,muted,surface,raised,input);
        controlCenterForm.ResumeLayout(true);controlCenterForm.Invalidate(true);
    }

    private static void ApplyControlCenterDetails(Control parent,Color border,Color accent,Color soft,Color text,Color muted,Color surface,Color raised,Color input)
    {
        foreach(Control c in parent.Controls)
        {
            if(c is Button button)
            {
                button.Height=Math.Max(button.Height,34);button.FlatStyle=FlatStyle.Flat;button.Cursor=Cursors.Hand;
                button.FlatAppearance.BorderColor=border;button.FlatAppearance.BorderSize=1;
                button.FlatAppearance.MouseOverBackColor=soft;button.FlatAppearance.MouseDownBackColor=soft;
                button.BackColor=input;button.ForeColor=text;button.Font=new Font("Segoe UI Semibold",8.5f);
            }
            else if(c is DataGridView grid)
            {
                grid.EnableHeadersVisualStyles=false;grid.BorderStyle=BorderStyle.None;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
                grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.ColumnHeadersHeight=36;grid.RowTemplate.Height=30;
                grid.BackgroundColor=surface;grid.GridColor=border;grid.DefaultCellStyle.SelectionBackColor=soft;grid.DefaultCellStyle.SelectionForeColor=text;
            }
            else if(c is ListBox list){list.BorderStyle=BorderStyle.FixedSingle;list.IntegralHeight=false;}
            else if(c is Label label&&label.Font.Size<10f){label.ForeColor=muted;}
            ApplyControlCenterDetails(c,border,accent,soft,text,muted,surface,raised,input);
        }
    }
}