using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
public partial class MainForm
{
 private bool farmAreaSelected,farmAreaDragging,farmAreaPinnedByMapSelection;
 private Point farmAreaDragStart,farmAreaDragCurrent;private PointF farmAreaCenterMap;private float farmAreaRadiusMap;private string farmAreaMapName;
 private void BeginFarmAreaSelection(MouseEventArgs e){if(dungeonNpcMarkerMode||e.Button!=MouseButtons.Left||atlas==null||!mapCalibrated)return;farmAreaDragging=true;farmAreaDragStart=e.Location;farmAreaDragCurrent=e.Location;bigMapCanvas.Capture=true;bigMapCanvas.Invalidate();}
 private void MoveFarmAreaSelection(MouseEventArgs e){if(!farmAreaDragging)return;farmAreaDragCurrent=e.Location;bigMapCanvas.Invalidate();}
 private void EndFarmAreaSelection(MouseEventArgs e)
 {
  if(!farmAreaDragging||e.Button!=MouseButtons.Left)return;farmAreaDragging=false;bigMapCanvas.Capture=false;farmAreaDragCurrent=e.Location;
  float sx=e.X-farmAreaDragStart.X,sy=e.Y-farmAreaDragStart.Y;if(sx*sx+sy*sy<100){bigMapCanvas.Invalidate();StartMapWalk(e.Location);return;}
  PointF center=BigMapScreenToMap(farmAreaDragStart),edge=BigMapScreenToMap(e.Location);
  float dx=edge.X-center.X,dy=edge.Y-center.Y;float maximum=Math.Max(5f,Math.Min(atlas.Width,atlas.Height)/2f-1f);float radius=Math.Min((float)Math.Sqrt(dx*dx+dy*dy),maximum);if(radius<5){bigMapCanvas.Invalidate();return;}
  // Preserve the dragged size and shift the circle only as much as needed to fit it on the map.
  center.X=Math.Max(radius,Math.Min(atlas.Width-1-radius,center.X));center.Y=Math.Max(radius,Math.Min(atlas.Height-1-radius,center.Y));
  farmAreaCenterMap=center;farmAreaRadiusMap=radius;farmAreaMapName=atlas.Name;farmAreaSelected=true;farmAreaPinnedByMapSelection=true;
  decimal units=(decimal)Math.Max((float)searchRange.Minimum,Math.Min((float)searchRange.Maximum,radius*MinimapAtlas.UnitsPerPixel));searchRange.Value=decimal.Round(units/500m)*500m;searchWhenEmptyCheck.Checked=true;searchPolicy.Reset();
  farmStatus.Text="Harita alanı seçildi · yarıçap "+(int)(farmAreaRadiusMap*MinimapAtlas.UnitsPerPixel)+" · bot çember içinde dolaşacak.";bigMapForm.Text="Harita · "+atlas.Name+" · alan seçildi · yarıçap "+(int)(farmAreaRadiusMap*MinimapAtlas.UnitsPerPixel);bigMapCanvas.Invalidate();
 }
 private PointF BigMapScreenToMap(Point p){float s=Math.Max(.001f,bigMapScale);return new PointF((p.X-bigMapOffX)/s,(p.Y-bigMapOffY)/s);}
 private PointF MapPixelToMemory(PointF p){return new PointF(p.X*MinimapAtlas.UnitsPerPixel+mapOffX,mapYSign>0?p.Y*MinimapAtlas.UnitsPerPixel+mapOffY:mapOffY-p.Y*MinimapAtlas.UnitsPerPixel);}
 private void SyncFarmAreaRadiusFromRange(){if(!farmAreaSelected||atlas==null||!string.Equals(farmAreaMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return;float maximum=Math.Max(5f,Math.Min(atlas.Width,atlas.Height)/2f-1f);farmAreaRadiusMap=Math.Min((float)searchRange.Value/MinimapAtlas.UnitsPerPixel,maximum);farmAreaCenterMap.X=Math.Max(farmAreaRadiusMap,Math.Min(atlas.Width-1-farmAreaRadiusMap,farmAreaCenterMap.X));farmAreaCenterMap.Y=Math.Max(farmAreaRadiusMap,Math.Min(atlas.Height-1-farmAreaRadiusMap,farmAreaCenterMap.Y));bigMapCanvas?.Invalidate();radarPanel?.Invalidate();}
 private bool TryGetFarmAreaMemory(out PointF center,out float radius){center=PointF.Empty;radius=0;if(!farmAreaSelected||atlas==null||!mapCalibrated||!string.Equals(farmAreaMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return false;center.X=farmAreaCenterMap.X*MinimapAtlas.UnitsPerPixel+mapOffX;center.Y=mapYSign>0?farmAreaCenterMap.Y*MinimapAtlas.UnitsPerPixel+mapOffY:mapOffY-farmAreaCenterMap.Y*MinimapAtlas.UnitsPerPixel;radius=farmAreaRadiusMap*MinimapAtlas.UnitsPerPixel;return radius>=100;}
 private static double DistanceSquared(float x1,float y1,float x2,float y2){double dx=x1-x2,dy=y1-y2;return dx*dx+dy*dy;}
 private void ClearFarmArea(string status){if(!farmAreaSelected&&!farmAreaDragging)return;farmAreaSelected=false;farmAreaDragging=false;farmAreaPinnedByMapSelection=false;farmAreaMapName=null;farmAreaRadiusMap=0;searchPolicy.Reset();if(!string.IsNullOrEmpty(status)&&farmStatus!=null)farmStatus.Text=status;if(bigMapForm!=null&&!bigMapForm.IsDisposed)bigMapForm.Text="Harita · "+(atlas?.Name??"")+" · sol tık: yürü · sürükle: dolaşma alanı · sağ tık: temizle · M: kapat";bigMapCanvas?.Invalidate();radarPanel?.Invalidate();}
 private void PaintFarmArea(Graphics g,Func<PointF,PointF> toScreen,bool showLabel=true)
 {
  PaintSavedFarmZones(g,toScreen,showLabel);
  PointF center;float radius;if(farmAreaDragging&&showLabel){center=BigMapScreenToMap(farmAreaDragStart);PointF edge=BigMapScreenToMap(farmAreaDragCurrent);float dx=edge.X-center.X,dy=edge.Y-center.Y;radius=(float)Math.Sqrt(dx*dx+dy*dy);}else{if(!farmAreaSelected||atlas==null||!string.Equals(farmAreaMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return;center=farmAreaCenterMap;radius=farmAreaRadiusMap;}
  PointF c=toScreen(center),rr=toScreen(new PointF(center.X+radius,center.Y));float sr=Math.Abs(rr.X-c.X);using(var fill=new SolidBrush(Color.FromArgb(42,59,130,246)))g.FillEllipse(fill,c.X-sr,c.Y-sr,sr*2,sr*2);using(var pen=new Pen(Color.FromArgb(235,96,165,250),2.5f){DashStyle=DashStyle.Dash})g.DrawEllipse(pen,c.X-sr,c.Y-sr,sr*2,sr*2);using(var dot=new SolidBrush(Color.FromArgb(255,210,235,255)))g.FillEllipse(dot,c.X-4,c.Y-4,8,8);if(showLabel&&!farmAreaDragging)using(var font=new Font("Segoe UI",9f,FontStyle.Bold))using(var text=new SolidBrush(Color.White)){string label="Dolaşma alanı · "+(int)(radius*MinimapAtlas.UnitsPerPixel);g.DrawString(label,font,text,c.X-sr,c.Y-sr-19);}
 }
}
