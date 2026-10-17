using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

// Click-to-walk on the big map: a double click picks a target, the character walks there on foot along an A* path
// over the map's wall data (the same navigator the dungeon runner uses). Right click or Esc cancels; M in the game
// window opens/closes the big map.
public partial class MainForm
{
    private Walker mapWalker;private int mapWalkGoal=-1;private PointF mapWalkPreciseTarget;private System.Windows.Forms.Timer mapWalkTimer,mapKeyTimer;private bool mapKeyDown;
    private float bigMapScale=1f,bigMapOffX,bigMapOffY;
    private string mapWalkStatus;

    private void InitializeMapWalk()
    {
        mapWalkTimer=new System.Windows.Forms.Timer(components){Interval=50};mapWalkTimer.Tick+=(s,e)=>MapWalkTick();
        // M toggles the big map while the connected game window is in front.
        mapKeyTimer=new System.Windows.Forms.Timer(components){Interval=60};
        mapKeyTimer.Tick+=(s,e)=>
        {
            bool down=(GetAsyncKeyState(0x4D)&0x8000)!=0;bool pressed=down&&!mapKeyDown;mapKeyDown=down;
            if(!pressed||!clientConnected)return;
            uint pid;GetWindowThreadProcessId(GetForegroundWindow(),out pid);
            if(memory.Field_0197?.Property_0268?.Id!=(int)pid)return;
            if(bigMapForm!=null&&bigMapForm.Visible)bigMapForm.Hide();else if(atlas!=null)ShowBigMap();
        };
        mapKeyTimer.Start();
        // While the player clicks in the game, the agent leaves the in-game cursor alone.
        var inputTimer=new System.Windows.Forms.Timer(components){Interval=100};DateTime lastMark=DateTime.MinValue;
        inputTimer.Tick+=(s,e)=>
        {
            bool down=(GetAsyncKeyState(0x01)&0x8000)!=0||(GetAsyncKeyState(0x02)&0x8000)!=0;
            if(!down||!clientConnected||DateTime.UtcNow-lastMark<TimeSpan.FromMilliseconds(250))return;
            uint pid;GetWindowThreadProcessId(GetForegroundWindow(),out pid);if(memory.Field_0197?.Property_0268?.Id!=(int)pid)return;
            lastMark=DateTime.UtcNow;
            try{System.IO.File.WriteAllText(UserInputPath,(DateTime.UtcNow-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds.ToString("R",System.Globalization.CultureInfo.InvariantCulture));}catch{}
        };
        inputTimer.Start();
    }

    private void HookBigMapInput()
    {
        bigMapCanvas.MouseDown+=(s,e)=>{routeMapClickConsumed=!string.IsNullOrEmpty(routeEditMode);if(!HandleRouteMapInput(e,0))BeginFarmAreaSelection(e);};
        bigMapCanvas.MouseMove+=(s,e)=>{if(!HandleRouteMapInput(e,1)&&!routeMapClickConsumed)MoveFarmAreaSelection(e);};
        bigMapCanvas.MouseUp+=(s,e)=>{if(!HandleRouteMapInput(e,2)&&!routeMapClickConsumed)EndFarmAreaSelection(e);};
        bigMapCanvas.MouseClick+=(s,e)=>{
            if(routeMapClickConsumed||!string.IsNullOrEmpty(routeEditMode)){routeMapClickConsumed=false;return;}
            if(e.Button==MouseButtons.Left&&CaptureDungeonNpcMarker(e.Location))return;
            if(e.Button!=MouseButtons.Right)return;
            if(dungeonNpcMarkerMode){CancelDungeonNpcMarkerSelection("İşaretleme iptal edildi.");return;}
            if(mapWalkGoal>=0)StopMapWalk("Yürüyüş iptal edildi.");else ClearFarmArea("Harita dolaşma alanı temizlendi.");
        };
        bigMapForm.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Escape&&!string.IsNullOrEmpty(routeEditMode)){EndRouteEdit();e.Handled=true;return;}if(e.KeyCode==Keys.Escape&&dungeonNpcMarkerMode)CancelDungeonNpcMarkerSelection("İşaretleme iptal edildi.");else if(e.KeyCode==Keys.Escape)StopMapWalk("Yürüyüş iptal edildi.");};
        bigMapCanvas.Cursor=Cursors.Cross;
    }

    private void StartMapWalk(Point screen)
    {
        if(atlas==null||!mapCalibrated||!clientConnected){SetMapWalkStatus("Önce oyuna bağlanın.");return;}
        if(taskDungeonRunning||Field_02DF||isFollowingRoute){SetMapWalkStatus("Görev, rota ya da hedef takibi çalışırken haritadan yürünemez.");return;}
        int gx=(int)((screen.X-bigMapOffX)/bigMapScale),gy=(int)((screen.Y-bigMapOffY)/bigMapScale);
        if(navMap!=atlas.Name){nav=new DungeonNavigator(atlas.Width,atlas.Height,atlas.IsBlocked);navMap=atlas.Name;}
        int cell=nav.Snap(gx,gy,10);
        if(cell<0){SetMapWalkStatus("Oraya yürünemez (duvar ya da harita dışı).");return;}
        int cx,cy;MapWalkPlayerCell(out cx,out cy);
        if(nav.FindPath(cx,cy,cell%nav.Width,cell/nav.Width)==null){SetMapWalkStatus("Oraya yürüyerek ulaşılamıyor (başka bir bölüm).");return;}
        mapWalker=new Walker();mapWalkGoal=cell;mapWalkPreciseTarget=MapPixelToMemory(new PointF(cell%nav.Width+0.5f,cell/nav.Width+0.5f));mapWalkTimer.Start();
        SetMapWalkStatus("Seçilen noktaya yürünüyor… (sağ tık / Esc: iptal)");
    }
    private void StopMapWalk(string text)
    {
        if(mapWalkGoal<0)return;
        mapWalkTimer.Stop();mapWalkGoal=-1;mapWalker=null;try{ReleaseMovement();}catch{}
        SetMapWalkStatus(text);
    }
    private void SetMapWalkStatus(string text){mapWalkStatus=text;if(bigMapForm!=null&&!bigMapForm.IsDisposed)bigMapForm.Text="Harita · "+(atlas?.Name??"")+(string.IsNullOrEmpty(text)?"":" · "+text);bigMapCanvas?.Invalidate();}
    private void MapWalkPlayerCell(out int cx,out int cy)
    {
        // Use the live memory position. The client map report is refreshed every two seconds and
        // made fast characters repath from an old cell, causing them to turn back and circle.
        var p=ToMapPixel(TaskX,TaskY);cx=(int)p.X;cy=(int)p.Y;
    }
    private void MapWalkTick()
    {
        if(mapWalkGoal<0){mapWalkTimer.Stop();return;}
        if(!clientConnected||atlas==null||navMap!=atlas.Name){StopMapWalk("Bağlantı ya da harita değişti; yürüyüş durdu.");return;}
        if(taskDungeonRunning||Field_02DF||isFollowingRoute){StopMapWalk("Başka bir hareket başladı; yürüyüş durdu.");return;}
        int cx,cy;MapWalkPlayerCell(out cx,out cy);
        if(ReachedCell(mapWalkGoal,cx,cy)){StopMapWalk("Hedefe varıldı.");return;}
        WalkTo(mapWalker,mapWalkGoal,TaskX,TaskY,cx,cy,movementClock.ElapsedMilliseconds,mapWalkPreciseTarget);
        if(mapWalker.Goal<0){StopMapWalk("Yol kapandı; yürüyüş durdu.");return;}
        bigMapCanvas?.Invalidate();
    }

    // Target marker and the remaining path on the big map.
    private void PaintMapWalk(Graphics g,Func<PointF,PointF> toScreen)
    {
        if(mapWalkGoal<0||nav==null)return;
        if(mapWalker?.Path!=null&&mapWalker.Path.Count>1)
        {
            var pts=mapWalker.Path.Where((c,i)=>i%3==0||i==mapWalker.Path.Count-1).Select(c=>toScreen(new PointF(c%nav.Width+0.5f,c/nav.Width+0.5f))).ToArray();
            using(var pen=new Pen(Color.FromArgb(220,255,214,64),2.5f){DashStyle=DashStyle.Dash,StartCap=LineCap.Round,EndCap=LineCap.Round})g.DrawLines(pen,pts);
        }
        var t=toScreen(new PointF(mapWalkGoal%nav.Width+0.5f,mapWalkGoal/nav.Width+0.5f));
        using(var ring=new Pen(Color.FromArgb(255,214,64),2.5f))g.DrawEllipse(ring,t.X-7,t.Y-7,14,14);
        using(var dot=new SolidBrush(Color.FromArgb(255,214,64)))g.FillEllipse(dot,t.X-2.5f,t.Y-2.5f,5,5);
    }
}
