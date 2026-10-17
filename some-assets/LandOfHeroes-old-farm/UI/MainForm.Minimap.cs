using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

// Live minimap: the radar shows the real map picture (extracted minimap.dds tiles) around the character, and a click
// opens the whole map like the game's M window. The client reports map name and local position; memory positions of
// the character and entities are mapped onto that picture with an offset calibrated from the two.
public partial class MainForm
{
    private QueuedPythonBridge mapBridge; private DateTime nextMapQuery=DateTime.MinValue,lastMapInfo=DateTime.MinValue;
    private MinimapAtlas atlas; private string atlasName,missingAtlas; private DateTime nextAtlasRetry=DateTime.MinValue;
    private bool mapCalibrated; private float mapOffX,mapOffY,lastMemX=float.NaN,lastMemY=float.NaN; private int mapYSign=1;
    private float mapPlayerX,mapPlayerY,calibX,calibY;private DateTime mapPlayerAt=DateTime.MinValue;private int calibSign=1,calibSamples,calibMismatch;
    private readonly List<PointF> groundItems=new List<PointF>();private DateTime groundRead=DateTime.MinValue;
    private float minimapZoom=0.5f; private Form bigMapForm; private Panel bigMapCanvas;
    private string MapInfoPath { get { return Path.Combine(runtimeFolder,"mapinfo.txt"); } }

    private bool mapUpdateRunning;
    private static string PackIndexFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","packindex"); } }
    private static string MapsFolder { get { return MinimapAtlas.SharedFolder; } }
    // Reads the decrypted pack indexes from the running game and extracts every map's minimap and wall map into
    // the maps folder. Run it again after a client patch; indexes not seen this time are kept from the last run.
    private async System.Threading.Tasks.Task UpdateMapsAsync(Action<string> report)
    {
        if(mapUpdateRunning)return;
        var process=memory.Field_0197?.Property_0268;
        if(process==null||process.HasExited){report("Önce oyunu açıp Oyuna bağlan düğmesine basın.");return;}
        mapUpdateRunning=true;
        try
        {
            var progress=new Progress<string>(report);IProgress<string> log=progress;
            int written=await System.Threading.Tasks.Task.Run(()=>
            {
                var packDir=PackExtractor.PackFolder(process);
                var indexes=PackExtractor.DumpIndexes(process,packDir,log.Report);
                foreach(var old in PackExtractor.LoadIndexes(PackIndexFolder))
                {
                    var idx=Path.Combine(packDir,old.Key+".idx");
                    var pak=Path.Combine(packDir,old.Key+".pak");
                    if(!indexes.ContainsKey(old.Key)&&File.Exists(idx)&&(new FileInfo(idx).Length-32)/288==old.Value.Count&&PackExtractor.ValidateIndex(pak,old.Value))indexes[old.Key]=old.Value;
                }
                PackExtractor.SaveIndexes(indexes,PackIndexFolder);
                return PackExtractor.ExtractMaps(packDir,indexes,MapsFolder,log.Report);
            });
            atlas?.Dispose();atlas=null;atlasName=null;mapCalibrated=false;
            report("Haritalar güncellendi · "+written+" dosya · "+MapsFolder);
        }
        catch(Exception ex){report("Harita güncelleme hatası: "+ex.Message);}
        finally{mapUpdateRunning=false;}
    }

    private sealed class MapCanvas : Panel { public MapCanvas(){DoubleBuffered=true;ResizeRedraw=true;} }

    private void InitializeMinimap()
    {
        radarPanel.Cursor=Cursors.Hand;
        radarPanel.MouseClick+=(s,e)=>
        {
            if(e.Button==MouseButtons.Right){minimapZoom=minimapZoom>=4f?0.5f:minimapZoom*2f;return;}
            if(atlas!=null)ShowBigMap();
        };
        new ToolTip().SetToolTip(radarPanel,"Sol tık: büyük harita (M) · Sağ tık: görünüm 0,5x / 1x / 2x / 4x");
    }

    // Called from the radar timer: keeps one small read-only query running for the map name and local position.
    private void PollMapInfo()
    {
        try
        {
            var process=memory.Field_0197?.Property_0268;
            if(process==null||process.HasExited){mapBridge?.Dispose();mapBridge=null;return;}
            if(mapBridge!=null&&mapBridge.ProcessId!=process.Id){mapBridge.Dispose();mapBridge=null;}
            if(mapBridge==null)mapBridge=new QueuedPythonBridge(process);
            if(mapBridge.Pending){mapBridge.Poll();}
            else if(DateTime.UtcNow>=nextMapQuery)
            {
                Directory.CreateDirectory(runtimeFolder);nextMapQuery=DateTime.UtcNow.AddSeconds(2);
                mapBridge.Queue(LocalPythonScript("import background,player\np=player.GetMainCharacterPosition()\nf.write(background.GetCurrentMapName()+'|'+str(int(p[0]))+'|'+str(int(p[1])))",MapInfoPath));
            }
        }
        catch{mapBridge?.Dispose();mapBridge=null;nextMapQuery=DateTime.UtcNow.AddSeconds(3);}
        float memX=IsX86ClientMode?mapPlayerX:memory.ReadFloat(AddressCatalog.Property_037B),memY=IsX86ClientMode?mapPlayerY:memory.ReadFloat(AddressCatalog.Property_037C);
        bool still=IsX86ClientMode||(!float.IsNaN(lastMemX)&&Math.Abs(memX-lastMemX)<30&&Math.Abs(memY-lastMemY)<30);lastMemX=memX;lastMemY=memY;
        try
        {
            if(!File.Exists(MapInfoPath))return;var written=File.GetLastWriteTimeUtc(MapInfoPath);if(written==lastMapInfo)return;
            var parts=File.ReadAllText(MapInfoPath).Split('|');float px,py;
            if(parts.Length!=3||!float.TryParse(parts[1],out px)||!float.TryParse(parts[2],out py))return;
            lastMapInfo=written;
            // A map without extracted pictures is retried now and then, so files added later show up without a restart.
            if(parts[0]!=atlasName||(atlas==null&&DateTime.UtcNow>=nextAtlasRetry))
            {
                nextAtlasRetry=DateTime.UtcNow.AddSeconds(10);
                if(farmAreaSelected&&!string.Equals(farmAreaMapName,parts[0],StringComparison.OrdinalIgnoreCase))ClearFarmArea(null);
                if(!string.Equals(atlasName,parts[0],StringComparison.OrdinalIgnoreCase))ClearMapEntityMemory();
                atlasName=parts[0];atlas?.Dispose();atlas=MinimapAtlas.Load(atlasName,MinimapAtlas.DefaultRoots());mapCalibrated=false;calibSamples=0;
                missingAtlas=atlas==null?atlasName:null;
            }
            mapPlayerX=px;mapPlayerY=py;mapPlayerAt=DateTime.UtcNow;TryApplyPackNpcMarker();
            if(IsX86ClientMode){mapYSign=1;mapOffX=0;mapOffY=0;mapCalibrated=atlas!=null;calibMismatch=0;return;}
            // The memory->map offset is constant per map. It is locked once two still readings agree, and only
            // replaced when several still readings consistently disagree, so a reading taken around a warp
            // (old position reported, new one in memory) can never shift the picture.
            int sign=Math.Abs(memY-py)<=Math.Abs(memY+py)?1:-1;float candX=memX-px,candY=sign>0?memY-py:memY+py;
            if(!still){}
            else if(!mapCalibrated)
            {
                if(calibSamples>0&&sign==calibSign&&Math.Abs(candX-calibX)<60&&Math.Abs(candY-calibY)<60){mapYSign=sign;mapOffX=candX;mapOffY=candY;mapCalibrated=true;calibMismatch=0;}
                calibX=candX;calibY=candY;calibSign=sign;calibSamples=1;
            }
            else if(sign!=mapYSign||Math.Abs(candX-mapOffX)>300||Math.Abs(candY-mapOffY)>300){if(++calibMismatch>=6){mapYSign=sign;mapOffX=candX;mapOffY=candY;calibMismatch=0;}}
            else calibMismatch=0;
        }
        catch(IOException){}
    }
    private bool TryClientMapPixel(out PointF pixel)
    {
        pixel=new PointF(mapPlayerX/MinimapAtlas.UnitsPerPixel,mapPlayerY/MinimapAtlas.UnitsPerPixel);
        return DateTime.UtcNow-mapPlayerAt<TimeSpan.FromSeconds(1.5);
    }
    // Ground items reported by the client, in map pixels (client positions are local map units).
    private void ReadGroundItems()
    {
        try
        {
            if(!File.Exists(GroundPath))return;var written=File.GetLastWriteTimeUtc(GroundPath);if(written==groundRead)return;groundRead=written;
            groundItems.Clear();
            foreach(var part in File.ReadAllText(GroundPath).Split(new[]{';'},StringSplitOptions.RemoveEmptyEntries))
            {var v=part.Split(',');float gx,gy;if(v.Length==3&&float.TryParse(v[1],out gx)&&float.TryParse(v[2],out gy))groundItems.Add(new PointF(gx/MinimapAtlas.UnitsPerPixel,gy/MinimapAtlas.UnitsPerPixel));}
            if(DateTime.UtcNow-written>TimeSpan.FromSeconds(3))groundItems.Clear();
        }
        catch(IOException){}
    }
    private PointF ToMapPixel(float memX,float memY)
    {
        float px=memX-mapOffX,py=mapYSign>0?memY-mapOffY:mapOffY-memY;
        return new PointF(px/MinimapAtlas.UnitsPerPixel,py/MinimapAtlas.UnitsPerPixel);
    }

    // Returns false when no map picture is available, so the classic radar is drawn instead.
    private bool TryRenderMinimap(List<EntityState> mobs)
    {
        PollMapInfo();
        if(atlas==null||!mapCalibrated)return false;
        int w=radarPanel.Width,h=radarPanel.Height;var bitmap=new Bitmap(w,h);
        using(var g=Graphics.FromImage(bitmap))
        {
            g.Clear(Color.FromArgb(8,10,18));g.SmoothingMode=SmoothingMode.AntiAlias;
            float cx=w/2f,cy=h/2f,r=Math.Min(w,h)/2f-6f;
            var me=ToMapPixel(TaskX,TaskY);
            float displayZoom=minimapZoom;
            if(farmAreaSelected&&string.Equals(farmAreaMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))
            {
                float needed=Math.Max(Math.Abs(farmAreaCenterMap.X-me.X),Math.Abs(farmAreaCenterMap.Y-me.Y))+farmAreaRadiusMap;
                if(needed>1)displayZoom=Math.Min(displayZoom,(r-7f)/needed);
            }
            displayZoom=Math.Max(0.06f,Math.Min(4f,displayZoom));
            using(var clip=new GraphicsPath()){clip.AddEllipse(cx-r,cy-r,r*2,r*2);g.SetClip(clip);}
            g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
            float half=r/displayZoom;
            g.DrawImage(atlas.Image,new RectangleF(cx-r,cy-r,r*2,r*2),new RectangleF(me.X-half,me.Y-half,half*2,half*2),GraphicsUnit.Pixel);
            PaintFarmArea(g,p=>new PointF(cx+(p.X-me.X)*displayZoom,cy+(p.Y-me.Y)*displayZoom),false);
            PaintRecordedRoute(g,p=>{var m=ToMapPixel(p.X,p.Y);return new PointF(cx+(m.X-me.X)*displayZoom,cy+(m.Y-me.Y)*displayZoom);},false);
            DrawMapMarkers(g,mobs,p=>new PointF(cx+(p.X-me.X)*displayZoom,cy+(p.Y-me.Y)*displayZoom),1f);
            g.ResetClip();
            using(var pen=new Pen(Color.FromArgb(210,198,168),2f))g.DrawEllipse(pen,cx-r,cy-r,r*2,r*2);
            using(var font=new Font("Segoe UI",7.5f))using(var brush=new SolidBrush(Color.FromArgb(200,236,230,218)))
                g.DrawString(atlas.Name+" · x"+displayZoom.ToString("0.##"),font,brush,4,h-16);
        }
        radarPanel.BackgroundImage?.Dispose();radarPanel.BackgroundImage=bitmap;radarPanel.BackgroundImageLayout=ImageLayout.Center;
        if(bigMapForm!=null&&bigMapForm.Visible)bigMapCanvas.Invalidate();
        return true;
    }

    private void DrawMapMarkers(Graphics g,List<EntityState> mobs,Func<PointF,PointF> toScreen,float size)
    {
        int self=memory.ReadInt32(AddressCatalog.Property_037E);
        foreach(var marker in MapEntitiesForDisplay(mobs))
        {
            var mob=marker.Entity;
            // Only NPCs, monsters and stones are drawn; other players are left off the map.
            if(mob.Property_03C9==self||(mob.Property_03CA!=1&&mob.Property_03CA!=AddressCatalog.Field_035C&&mob.Property_03CA!=AddressCatalog.Field_035B))continue;
            var s=toScreen(ToMapPixel(mob.X,mob.Y));int alpha=marker.Live?235:105;float markerSize=marker.Live?7f:5.5f;
            Color c=mob.Property_03CA==AddressCatalog.Field_035C?Color.FromArgb(alpha,230,70,60):mob.Property_03CA==AddressCatalog.Field_035B?Color.FromArgb(alpha,250,250,250):Color.FromArgb(alpha,95,112,255);
            using(var b=new SolidBrush(c))g.FillEllipse(b,s.X-markerSize*size/2f,s.Y-markerSize*size/2f,markerSize*size,markerSize*size);
            using(var pen=new Pen(Color.FromArgb(marker.Live?160:80,0,0,0),1f))g.DrawEllipse(pen,s.X-markerSize*size/2f,s.Y-markerSize*size/2f,markerSize*size,markerSize*size);
        }
        ReadGroundItems();
        foreach(var item in groundItems)
        {
            var s=toScreen(item);float r=4.5f*size;
            var diamond=new[]{new PointF(s.X,s.Y-r),new PointF(s.X+r,s.Y),new PointF(s.X,s.Y+r),new PointF(s.X-r,s.Y)};
            using(var b=new SolidBrush(Color.FromArgb(255,150,30)))g.FillPolygon(b,diamond);
            using(var pen=new Pen(Color.Black,1f))g.DrawPolygon(pen,diamond);
        }
        // The character as an arrow pointing where it faces.
        float memX=TaskX,memY=TaskY;
        var me=toScreen(ToMapPixel(memX,memY));
        double heading=IsX86ClientMode?0:NormalizeAngle(memory.ReadFloat(AddressCatalog.Property_0382))*Math.PI/180.0;
        var tip=toScreen(ToMapPixel(memX+(float)Math.Sin(heading)*100f,memY+(float)Math.Cos(heading)*100f));
        float dx=tip.X-me.X,dy=tip.Y-me.Y,len=(float)Math.Sqrt(dx*dx+dy*dy);if(len<0.001f){dx=0;dy=-1;len=1;}
        dx/=len;dy/=len;float a=8f*size;
        var arrow=new[]{new PointF(me.X+dx*a,me.Y+dy*a),new PointF(me.X-dx*a*0.6f-dy*a*0.6f,me.Y-dy*a*0.6f+dx*a*0.6f),new PointF(me.X-dx*a*0.6f+dy*a*0.6f,me.Y-dy*a*0.6f-dx*a*0.6f)};
        using(var b=new SolidBrush(Color.FromArgb(255,214,64)))g.FillPolygon(b,arrow);
        using(var pen=new Pen(Color.Black,1.2f))g.DrawPolygon(pen,arrow);
    }

    private void ShowBigMap()
    {
        if(bigMapForm==null||bigMapForm.IsDisposed)
        {
            Color mapBackground=nightTheme?Color.FromArgb(8,12,20):LightPage;
            bigMapForm=new Form{Text="Harita",Size=new Size(760,800),StartPosition=FormStartPosition.CenterParent,BackColor=mapBackground,ForeColor=nightTheme?NightText:LightText,ShowInTaskbar=false,KeyPreview=true,Opacity=1d,AllowTransparency=false,TransparencyKey=Color.Empty};
            bigMapCanvas=new MapCanvas{Dock=DockStyle.Fill,BackColor=mapBackground};
            bigMapCanvas.Paint+=(s,e)=>PaintBigMap(e.Graphics,bigMapCanvas.ClientSize);
            bigMapForm.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.M||(e.KeyCode==Keys.Escape&&mapWalkGoal<0&&string.IsNullOrEmpty(routeEditMode)))bigMapForm.Hide();};
            bigMapForm.VisibleChanged+=(s,e)=>{if(!bigMapForm.Visible)EndRouteEdit();};
            bigMapForm.FormClosing+=(s,e)=>{if(e.CloseReason==CloseReason.UserClosing){e.Cancel=true;bigMapForm.Hide();}};
            bigMapForm.Controls.Add(bigMapCanvas);
            HookBigMapInput();
        }
        bigMapForm.Text=dungeonNpcMarkerMode?"Harita · "+(atlas?.Name??"")+" · lobi NPC noktasına sol tıklayın · sağ tık/Esc: iptal":"Harita · "+(atlas?.Name??"")+" · sol tık: yürü · sürükle: dolaşma alanı · sağ tık: temizle · M: kapat";
        if(!bigMapForm.Visible)bigMapForm.Show(this);bigMapForm.Activate();
    }
    private void PaintBigMap(Graphics g,Size size)
    {
        if(atlas==null||!mapCalibrated)return;
        float scale=Math.Min((size.Width-20f)/atlas.Width,(size.Height-20f)/atlas.Height);
        float ox=(size.Width-atlas.Width*scale)/2f,oy=(size.Height-atlas.Height*scale)/2f;
        bigMapScale=scale;bigMapOffX=ox;bigMapOffY=oy;
        g.InterpolationMode=scale>=1?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBilinear;g.PixelOffsetMode=PixelOffsetMode.Half;
        g.DrawImage(atlas.Image,ox,oy,atlas.Width*scale,atlas.Height*scale);
        g.SmoothingMode=SmoothingMode.AntiAlias;
        PaintFarmArea(g,p=>new PointF(ox+p.X*scale,oy+p.Y*scale));
        PaintMapWalk(g,p=>new PointF(ox+p.X*scale,oy+p.Y*scale));
        PaintRecordedRoute(g,p=>{var m=ToMapPixel(p.X,p.Y);return new PointF(ox+m.X*scale,oy+m.Y*scale);},true);
        PaintDungeonNpcMarker(g,p=>new PointF(ox+p.X*scale,oy+p.Y*scale));
        DrawMapMarkers(g,radarEntities??new List<EntityState>(),p=>new PointF(ox+p.X*scale,oy+p.Y*scale),Math.Max(1f,Math.Min(2f,scale*2f)));
    }
}
