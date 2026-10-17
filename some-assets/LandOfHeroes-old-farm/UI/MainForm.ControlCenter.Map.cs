using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

public partial class MainForm
{
    private void PaintSavedFarmZones(Graphics g,Func<PointF,PointF> toScreen,bool showLabel)
    {
        if(atlas==null||advancedState?.Zones==null)return;
        foreach(var z in advancedState.Zones.Where(v=>v.Enabled&&string.Equals(v.MapName,atlas.Name,StringComparison.OrdinalIgnoreCase)))
        {
            var center=new PointF(z.MapX,z.MapY);var c=toScreen(center);var rr=toScreen(new PointF(center.X+z.RadiusMap,center.Y));
            float radius=Math.Abs(rr.X-c.X);Color line=z.Exclusion?Color.FromArgb(235,245,87,87):Color.FromArgb(215,86,104,220);
            Color fill=z.Exclusion?Color.FromArgb(34,245,87,87):Color.FromArgb(30,86,104,220);
            using(var brush=new SolidBrush(fill))g.FillEllipse(brush,c.X-radius,c.Y-radius,radius*2,radius*2);
            using(var pen=new Pen(line,2f){DashStyle=z.Exclusion?DashStyle.Dot:DashStyle.Dash})g.DrawEllipse(pen,c.X-radius,c.Y-radius,radius*2,radius*2);
            if(showLabel)using(var font=new Font("Segoe UI",8f,FontStyle.Bold))using(var text=new SolidBrush(line))g.DrawString(z.Name,font,text,c.X-radius,c.Y-radius-16);
        }
    }
}