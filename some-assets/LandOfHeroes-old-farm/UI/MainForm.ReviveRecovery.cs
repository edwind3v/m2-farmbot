using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

public partial class MainForm
{
    private readonly ReviveEscapePolicy reviveEscape=new ReviveEscapePolicy();
    private System.Windows.Forms.Timer reviveRecoveryTimer;
    private int reviveRecoveryPid,reviveGoal=-1,reviveAttempt;private long reviveGoalUntil;private PointF revivePoint;
    private Walker reviveWalker=new Walker();private DungeonNavigator retreatNav;private string retreatNavMap;
    private long nextRetreatGuardWrite,reviveRetryAt;private DungeonNavigator reviveGoalNavigator;
    private void InitializeReviveRecovery()
    {
        reviveRecoveryTimer=new System.Windows.Forms.Timer(components){Interval=100};
        reviveRecoveryTimer.Tick+=(s,e)=>
        {
            try
            {
                RefreshReviveRecovery();
                if(!RouteOwnsCombat&&!taskDungeonRunning&&(normalBotRequested||dmgStartCheck.Checked||Field_02DF)&&IsClientReady())TryRunReviveEscape();
            }
            catch(Exception){if(reviveEscape.BlockAttack)automation.RecoveryBlocked=true;}
        };
        reviveRecoveryTimer.Start();
    }
    private void RefreshReviveRecovery()
    {
        bool active=RouteOwnsCombat||taskDungeonRunning||normalBotRequested||Field_02DF||(dmgStartCheck!=null&&dmgStartCheck.Checked);
        int pid=memory.Field_0197?.Property_0268?.Id??0;
        if(!active||pid<=0||IsX86ClientMode)
        {
            if(reviveEscape.BlockAttack)nextRetreatGuardWrite=0;
            reviveEscape.Reset();reviveRecoveryPid=pid;reviveGoal=-1;automation.RecoveryBlocked=false;WriteRetreatGuard(false);return;
        }
        if(reviveRecoveryPid!=pid){reviveRecoveryPid=pid;reviveEscape.Reset();reviveGoal=-1;reviveRetryAt=0;reviveAttempt=0;reviveWalker.Reset();}
        bool before=reviveEscape.BlockAttack,wasDead=reviveEscape.Dead;
        try{if(File.Exists(LifeStatePath))reviveEscape.Observe(File.ReadAllText(LifeStatePath),pid,(DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds);}catch(IOException){}
        automation.RecoveryBlocked=reviveEscape.BlockAttack;
        if(before!=reviveEscape.BlockAttack)nextRetreatGuardWrite=0;
        WriteRetreatGuard(reviveEscape.BlockAttack);
        if((!before&&reviveEscape.BlockAttack)||(!wasDead&&reviveEscape.Dead)){reviveGoal=-1;reviveRetryAt=0;reviveAttempt=0;reviveWalker.Reset();ReleaseMovement();if(IsClientReady())Method_02AF(0);approachTargetId=0;approachPolicy.Reset();approachProgress.Reset();}
    }
    private void WriteRetreatGuard(bool blocked)
    {
        long now=movementClock.ElapsedMilliseconds;
        if(now<nextRetreatGuardWrite)return;
        nextRetreatGuardWrite=now+500;
        try{Directory.CreateDirectory(runtimeFolder);File.WriteAllText(RetreatGuardPath,(blocked?(DateTime.UtcNow.AddSeconds(2)-new DateTime(1970,1,1)).TotalSeconds:0).ToString("R",CultureInfo.InvariantCulture));}catch(IOException){}
    }
    private void SetReviveStatus(string text){if(RouteOwnsCombat)routePatrolStatus.Text=text;else if(taskDungeonRunning)taskStatus.Text=text;else if(farmStatus!=null)farmStatus.Text=text;}
    private bool TryRunReviveEscape()
    {
        if(IsX86ClientMode||!reviveEscape.BlockAttack)return false;
        if(reviveEscape.Dead){ReleaseMovement();SetReviveStatus("Karakter öldü · canlanma bekleniyor (saldırı bekletildi).");return true;}
        if(RouteOwnsCombat&&!reviveEscape.Fresh((DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds)){ReleaseMovement();SetReviveStatus("Canlanma sonrası taze oyuncu canı bekleniyor; saldırı kapalı.");return true;}
        if(!IsClientReady()){ReleaseMovement();return true;}
        float x=TaskX,y=TaskY;var mobs=ReadRadarEntities().Where(e=>e.Property_03CA==AddressCatalog.Field_035C&&Distance2D(x,y,e.X,e.Y)<3500).ToList();
        if(mobs.Count==0||mobs.All(e=>Distance2D(x,y,e.X,e.Y)>=750))
        {
            if(RouteOwnsCombat&&!RequestRouteRecoveryMount())return true;
            ReleaseMovement();reviveEscape.Escaped();automation.RecoveryBlocked=false;nextRetreatGuardWrite=0;WriteRetreatGuard(false);reviveGoal=-1;reviveWalker.Reset();SetReviveStatus("Canlanma sonrası güvenli mesafe sağlandı · saldırıya devam ediliyor.");return false;
        }
        long now=movementClock.ElapsedMilliseconds;
        DungeonNavigator navigator=null;
        if(atlas!=null&&mapCalibrated)
        {
            if(nav!=null&&navMap==atlas.Name)navigator=nav;
            else
            {
                if(retreatNav==null||retreatNavMap!=atlas.Name){retreatNav=new DungeonNavigator(atlas.Width,atlas.Height,atlas.IsBlocked);retreatNavMap=atlas.Name;}
                navigator=retreatNav;
            }
        }
        if(navigator!=reviveGoalNavigator){reviveGoal=-1;reviveRetryAt=0;reviveGoalNavigator=navigator;}
        if(reviveGoal<0&&now<reviveRetryAt){ReleaseMovement();return true;}
        if(reviveGoal<0||now>=reviveGoalUntil||Distance2D(x,y,revivePoint.X,revivePoint.Y)<80)
        {
            reviveAttempt++;
            if(!TryReviveEscapeGoal(navigator,x,y,mobs,reviveAttempt,out revivePoint,out reviveGoal))
            {
                reviveRetryAt=now+1000;ReleaseMovement();SetReviveStatus("Canlandı · slotlardan uzaklaşmak için güvenli yol aranıyor (saldırı bekletildi).");return true;
            }
            reviveGoalUntil=now+3500;reviveWalker.Reset();reviveRouteWaypoint=null;
        }
        if(RouteOwnsCombat)MoveRouteRecovery(navigator,x,y,now);
        else if(navigator!=null)
        {
            // WalkTo uses the shared map. Preserve the dungeon's navigation instance for its next tick.
            var saved=nav;nav=navigator;
            try{int cx,cy;CellOf(x,y,out cx,out cy);WalkTo(reviveWalker,reviveGoal,x,y,cx,cy,now,revivePoint,60f);}finally{nav=saved;}
        }
        else SteerDungeon(x,y,revivePoint.X,revivePoint.Y);
        SetReviveStatus("Canlandı · slotlardan uzaklaşılıyor, güvenli mesafeden sonra saldırılacak.");return true;
    }
    private bool TryReviveEscapeGoal(DungeonNavigator navigator,float x,float y,List<EntityState> mobs,int attempt,out PointF goal,out int cell)
    {
        goal=PointF.Empty;cell=-1;float initial=mobs.Min(m=>Distance2D(x,y,m.X,m.Y)),best=initial+100;
        var origin=ToMapPixel(x,y);int cx=(int)origin.X,cy=(int)origin.Y;int floor=navigator==null?-1:navigator.FloorAt(cx,cy);
        PointF areaCenter=PointF.Empty;float areaRadius=0;bool limitArea=!RouteOwnsCombat&&!taskDungeonRunning&&TryGetFarmAreaMemory(out areaCenter,out areaRadius);
        foreach(float radius in new[]{1000f,1500f,650f})for(int i=0;i<16;i++)
        {
            double angle=(i*22.5+attempt*11.25)*Math.PI/180;
            var p=new PointF(x+(float)Math.Cos(angle)*radius,y+(float)Math.Sin(angle)*radius);int candidate=0;
            if(navigator!=null)
            {
                var pixel=ToMapPixel(p.X,p.Y);candidate=navigator.Snap((int)pixel.X,(int)pixel.Y,2);
                if(candidate<0||!IsDungeonTargetOnFloor(navigator,floor,candidate))continue;
                float px=candidate%navigator.Width*MinimapAtlas.UnitsPerPixel+50,py=candidate/navigator.Width*MinimapAtlas.UnitsPerPixel+50;
                p=new PointF(px+mapOffX,mapYSign>0?py+mapOffY:mapOffY-py);
            }
            if(limitArea&&Distance2D(p.X,p.Y,areaCenter.X,areaCenter.Y)>areaRadius)continue;
            if(RouteOwnsCombat&&(activeRouteOptions.Forbidden(p.X,p.Y,100)||!RouteSegmentSafe(x,y,p.X,p.Y)))continue;
            float clearance=mobs.Min(m=>Distance2D(p.X,p.Y,m.X,m.Y));
            if(clearance<750)continue;
            float score=clearance-Distance2D(x,y,p.X,p.Y)*.05f;
            if(score<=best)continue;
            if(navigator!=null&&navigator.FindPath(cx,cy,candidate%navigator.Width,candidate/navigator.Width)==null)continue;
            best=score;goal=p;cell=candidate;
        }
        return cell>=0;
    }
}
