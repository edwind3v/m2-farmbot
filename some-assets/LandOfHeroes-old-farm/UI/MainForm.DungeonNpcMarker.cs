using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private Button flowNpcMarkerButton;
    private Label flowNpcMarkerStatus;
    private bool dungeonNpcMarkerMode;

    private void RefreshDungeonNpcMarkerStatus()
    {
        if(flowNpcMarkerStatus==null||taskSequence==null)return;
        flowNpcMarkerStatus.Text=taskSequence.LobbyNpcMarkerSet
            ?"Kayıtlı: "+taskSequence.LobbyNpcMapName+" · "+taskSequence.LobbyNpcMapX.ToString("0.0")+" , "+taskSequence.LobbyNpcMapY.ToString("0.0")
            :"Bu zindan için lobi NPC konumu işaretlenmedi.";
    }

    private void BeginDungeonNpcMarkerSelection()
    {
        if(taskCancellation!=null){MessageBox.Show("NPC konumunu işaretlemek için çalışan zindanı durdurun.");return;}
        if(!IsClientReady()||atlas==null||!mapCalibrated){MessageBox.Show("Önce oyuna bağlanın ve lobi haritasının yüklenmesini bekleyin.");return;}
        dungeonNpcMarkerMode=true;
        if(farmAreaDragging){farmAreaDragging=false;bigMapCanvas.Capture=false;}
        ShowBigMap();
        bigMapForm.Text="Harita · "+atlas.Name+" · "+ConfiguredDungeonNpcName(1)+" konumuna sol tıklayın · sağ tık/Esc: iptal";
        flowNpcMarkerStatus.Text="Büyük haritada NPC'nin durduğu noktaya sol tıklayın.";
        bigMapCanvas.Invalidate();
    }

    private bool CaptureDungeonNpcMarker(Point screen)
    {
        if(!dungeonNpcMarkerMode)return false;
        if(atlas==null||!mapCalibrated){CancelDungeonNpcMarkerSelection("Harita hazır değil; işaret kaydedilmedi.");return true;}
        PointF point=BigMapScreenToMap(screen);
        if(point.X<0||point.Y<0||point.X>=atlas.Width||point.Y>=atlas.Height)
        {
            flowNpcMarkerStatus.Text="Haritanın içindeki NPC noktasına tıklayın.";
            return true;
        }
        taskSequence.LobbyNpcMarkerSet=true;
        taskSequence.LobbyNpcMapName=atlas.Name;
        taskSequence.LobbyNpcMapX=point.X;
        taskSequence.LobbyNpcMapY=point.Y;
        dungeonNpcMarkerMode=false;
        AutoSaveTaskSequence();
        RefreshDungeonNpcMarkerStatus();
        bigMapForm.Text="Harita · "+atlas.Name+" · "+ConfiguredDungeonNpcName(1)+" lobi noktası kaydedildi";
        bigMapCanvas.Invalidate();
        RecordActivity("Zindan",taskSequence.Name+" · "+ConfiguredDungeonNpcName(1)+" lobi noktası kaydedildi.");
        return true;
    }

    private void CancelDungeonNpcMarkerSelection(string text)
    {
        dungeonNpcMarkerMode=false;
        RefreshDungeonNpcMarkerStatus();
        if(bigMapForm!=null&&!bigMapForm.IsDisposed)bigMapForm.Text="Harita · "+(atlas?.Name??"")+" · "+text;
        bigMapCanvas?.Invalidate();
    }

    private void PaintDungeonNpcMarker(Graphics g,Func<PointF,PointF> toScreen)
    {
        if(taskSequence==null||!taskSequence.LobbyNpcMarkerSet||atlas==null||
           !string.Equals(taskSequence.LobbyNpcMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return;
        PointF p=toScreen(new PointF(taskSequence.LobbyNpcMapX,taskSequence.LobbyNpcMapY));
        using(var fill=new SolidBrush(Color.FromArgb(72,255,72,190)))g.FillEllipse(fill,p.X-12,p.Y-12,24,24);
        using(var pen=new Pen(Color.FromArgb(255,255,90,205),3f)){pen.DashStyle=DashStyle.Dash;g.DrawEllipse(pen,p.X-12,p.Y-12,24,24);g.DrawLine(pen,p.X-17,p.Y,p.X+17,p.Y);g.DrawLine(pen,p.X,p.Y-17,p.X,p.Y+17);}
        using(var font=new Font("Segoe UI",9f,FontStyle.Bold))using(var brush=new SolidBrush(Color.White))
            g.DrawString(ConfiguredDungeonNpcName(1)+" · lobi",font,brush,p.X+15,p.Y-18);
    }

    private async Task<bool> MoveToDungeonLobbyNpcMarkerAsync(string npcName,CancellationToken token)
    {
        if(taskSequence==null||!taskSequence.LobbyNpcMarkerSet)return false;
        if(atlas==null||!mapCalibrated||!string.Equals(taskSequence.LobbyNpcMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))
        {
            ReleaseMovement();
            taskStatus.Text=npcName+" işareti bu haritaya ait değil · karakter rastgele yürütülmeyecek.";
            return false;
        }

        var visible=ReadVisualCandidateVids();
        if(visible.Count>0)
        {
            int found,x,y;
            string point=await GamePythonAsync(BuildNpcEngineTargetScript(npcName,visible),token);
            if(TryParseNpcPoint(point,out found,out x,out y))return false;
        }

        if(navMap!=atlas.Name){nav=new DungeonNavigator(atlas.Width,atlas.Height,atlas.IsBlocked);navMap=atlas.Name;}
        int goal=nav.Snap((int)Math.Round(taskSequence.LobbyNpcMapX),(int)Math.Round(taskSequence.LobbyNpcMapY),18);
        if(goal<0)
        {
            ReleaseMovement();
            taskStatus.Text=npcName+" işareti yürünülebilir zeminde değil · Zindan Akışı'ndan yeniden işaretleyin.";
            return false;
        }
        int cx,cy;MapWalkPlayerCell(out cx,out cy);
        List<int> path=nav.FindPath(cx,cy,goal%nav.Width,goal/nav.Width);
        if(path==null)
        {
            ReleaseMovement();
            taskStatus.Text=npcName+" işaretine güvenli rota yok · dağa/duvara yürünmedi.";
            return false;
        }

        DateTime deadline=DateTime.UtcNow.AddSeconds(40),planned=DateTime.UtcNow;
        long lastMoveAt=movementClock.ElapsedMilliseconds;float lastX=TaskX,lastY=TaskY;int stuck=0;
        try
        {
            while(DateTime.UtcNow<deadline)
            {
                token.ThrowIfCancellationRequested();
                if(atlas==null||!string.Equals(taskSequence.LobbyNpcMapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return false;
                float px=TaskX,py=TaskY;MapWalkPlayerCell(out cx,out cy);
                if(ReachedCell(goal,cx,cy))
                {
                    taskStatus.Text=npcName+" işaretine varıldı · canlı NPC oyun motorunda doğrulanıyor.";
                    return true;
                }
                long now=movementClock.ElapsedMilliseconds;
                if(Distance2D(px,py,lastX,lastY)>40){lastX=px;lastY=py;lastMoveAt=now;stuck=0;}
                else if(now-lastMoveAt>2500)
                {
                    lastMoveAt=now;
                    path=nav.FindPath(cx,cy,goal%nav.Width,goal/nav.Width);
                    if(path==null||++stuck>=3){taskStatus.Text=npcName+" işaretine giden yol kapandı · hareket durduruldu.";return false;}
                }
                else if(DateTime.UtcNow-planned>TimeSpan.FromSeconds(2))
                {
                    path=nav.FindPath(cx,cy,goal%nav.Width,goal/nav.Width);planned=DateTime.UtcNow;
                    if(path==null){taskStatus.Text=npcName+" işaretine güvenli rota kayboldu · hareket durduruldu.";return false;}
                }
                int nearest=0,best=int.MaxValue;
                for(int i=0;i<Math.Min(path.Count,40);i++){int dx=path[i]%nav.Width-cx,dy=path[i]/nav.Width-cy,d=dx*dx+dy*dy;if(d<best){best=d;nearest=i;}}
                if(nearest>0)path.RemoveRange(0,nearest);
                int next=path.Count>1?nav.LookAhead(path,8):goal;
                PointF target=CellToMemory(next);
                SteerDungeon(px,py,target.X,target.Y);
                taskStatus.Text=npcName+" radarda yok · kayıtlı lobi noktasına güvenli rotayla gidiliyor.";
                await Task.Delay(50,token);
            }
            taskStatus.Text=npcName+" işaretine varılamadı · hareket durduruldu.";
            return false;
        }
        finally{ReleaseMovement();}
    }
}
