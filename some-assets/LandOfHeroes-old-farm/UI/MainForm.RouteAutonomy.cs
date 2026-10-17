using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

public partial class MainForm
{
    private readonly RouteStoneWatchdog routeStoneWatchdog=new RouteStoneWatchdog();
    private readonly RouteApproachWatchdog routeApproachWatchdog=new RouteApproachWatchdog();
    private readonly Dictionary<int,int> routeStoneOrdinals=new Dictionary<int,int>();
    private int routeStoneOrdinal,routeRecoveryVid;
    private PointF routeRecoveryPoint;
    private long routeRecoveryUntil;
    private bool routeRecoveryActive;

    private void ResetRouteAutonomy(bool full=true)
    {
        routeStoneWatchdog.Reset();routeApproachWatchdog.Reset();routeRecoveryActive=false;routeRecoveryVid=0;
        if(full){routeStoneOrdinals.Clear();routeStoneOrdinal=0;}
    }
    private int TrackRouteStone(EntityState target)
    {
        if(target==null||target.Property_03CA!=AddressCatalog.Field_035B)return 0;
        int ordinal;
        if(routeStoneOrdinals.TryGetValue(target.Property_03C9,out ordinal))return ordinal;
        ordinal=++routeStoneOrdinal;routeStoneOrdinals[target.Property_03C9]=ordinal;
        RecordActivity("Rota hedef",ordinal+". taşa gidiyor · "+(target.Property_03CD??"Taş")+" · VID "+target.Property_03C9);
        return ordinal;
    }
    private float? ReadRouteTargetHp(int vid)
    {
        try{
            if(!File.Exists(TargetHealthPath))return null;
            var snapshot=TargetHealthSnapshot.Parse(File.ReadAllText(TargetHealthPath),routeOwnerPid,routeRunningMap,DateTime.UtcNow);
            return snapshot!=null&&snapshot.Vid==vid?snapshot.Hp:null;
        }catch(IOException){return null;}catch(UnauthorizedAccessException){return null;}
    }
    private bool TryRouteRecoveryPoint(EntityState target,float x,float y,float close,int attempt,out PointF result)
    {
        result=PointF.Empty;
        double baseAngle=Math.Atan2(y-target.Y,x-target.X),radius=Math.Max(360,Math.Min(750,close+260));
        double turn=(45+Math.Min(3,attempt)*28)*Math.PI/180.0;
        foreach(double angle in new[]{baseAngle+turn,baseAngle-turn,baseAngle+Math.PI,baseAngle+turn*.55,baseAngle-turn*.55}){
            var candidate=new PointF(target.X+(float)(Math.Cos(angle)*radius),target.Y+(float)(Math.Sin(angle)*radius));
            if(activeRouteOptions.Forbidden(candidate.X,candidate.Y,100)||!RouteSegmentSafe(x,y,candidate.X,candidate.Y))continue;
            if(RoutePatrol.Distance(x,y,candidate.X,candidate.Y)<180)continue;
            result=candidate;return true;
        }
        return false;
    }
    private void SkipStuckRouteStone(EntityState target,long now,int ordinal,string reason)
    {
        SetRouteAttack(false);ReleaseMovement();ResetRouteContact();
        combatFocus.Abandon(target.Property_03C9,now,60000);routeStoneBatchVids.Remove(target.Property_03C9);
        routePursuitVid=0;routeRecoveryActive=false;routeRecoveryVid=0;routeStoneWatchdog.Reset();routeApproachWatchdog.Reset();
        routeLastProgress=now;routeLastPosition=new PointF(TaskX,TaskY);
        string message=(ordinal>0?ordinal+". taş":"Taş")+" takılma tespit edildi · "+reason+" · 60 saniye atlandı, başka hedefe gidiyor";
        routePatrolStatus.Text=message;RecordRecovery(message);
    }
    private bool HandleRouteStoneWatch(EntityState target,float x,float y,float close,bool stationary,long now)
    {
        if(target==null||target.Property_03CA!=AddressCatalog.Field_035B){routeStoneWatchdog.Reset();routeApproachWatchdog.Reset();routeRecoveryActive=false;return false;}
        int ordinal=TrackRouteStone(target);
        if(routeRecoveryActive&&routeRecoveryVid!=target.Property_03C9){routeRecoveryActive=false;routeStoneWatchdog.Reset();}
        if(routeRecoveryActive)
        {
            SetRouteAttack(false);ResetRouteContact();
            double distance=RoutePatrol.Distance(x,y,routeRecoveryPoint.X,routeRecoveryPoint.Y);
            if(distance<=70||now>=routeRecoveryUntil){ReleaseMovement();routeRecoveryActive=false;routeLastProgress=now;routeLastPosition=new PointF(x,y);routePatrolStatus.Text=ordinal+". taş · yeni konumdan yeniden deneniyor";return true;}
            RouteMoveTo(x,y,routeRecoveryPoint.X,routeRecoveryPoint.Y,now);
            routePatrolStatus.Text=ordinal+". taşta takılma tespit edildi · başka konuma gidiyor · deneme "+routeStoneWatchdog.Attempts+"/3";
            return true;
        }
        var approachAction=routeApproachWatchdog.Observe(true,target.Property_03C9,x,y,stationary,now);
        if(approachAction==RouteStoneWatchAction.Skip){SkipStuckRouteStone(target,now,ordinal,"yaklaşırken üç kez ilerleme olmadı");return true;}
        if(approachAction==RouteStoneWatchAction.Reposition){
            PointF approachPoint;
            if(!TryRouteRecoveryPoint(target,x,y,close,routeApproachWatchdog.Attempts,out approachPoint)){SkipStuckRouteStone(target,now,ordinal,"yaklaşma için güvenli konum bulunamadı");return true;}
            SetRouteAttack(false);ReleaseMovement();ResetRouteContact();routeRecoveryPoint=approachPoint;routeRecoveryVid=target.Property_03C9;routeRecoveryActive=true;routeRecoveryUntil=now+4500;
            routeLastProgress=now;routeLastPosition=new PointF(x,y);string approachMessage=ordinal+". taşa yaklaşırken takılma tespit edildi · başka konuma gidiyor · deneme "+routeApproachWatchdog.Attempts+"/3";
            routePatrolStatus.Text=approachMessage;RecordRecovery(approachMessage);return true;
        }
        var action=routeStoneWatchdog.Observe(true,target.Property_03C9,ReadRouteTargetHp(target.Property_03C9),stationary,now);
        if(action==RouteStoneWatchAction.None)return false;
        if(action==RouteStoneWatchAction.Skip){SkipStuckRouteStone(target,now,ordinal,"üç yeniden konumlanmada can ilerlemedi");return true;}
        PointF point;
        if(!TryRouteRecoveryPoint(target,x,y,close,routeStoneWatchdog.Attempts,out point)){SkipStuckRouteStone(target,now,ordinal,"güvenli yeniden konum bulunamadı");return true;}
        SetRouteAttack(false);ReleaseMovement();ResetRouteContact();routeRecoveryPoint=point;routeRecoveryVid=target.Property_03C9;routeRecoveryActive=true;routeRecoveryUntil=now+4500;
        routeLastProgress=now;routeLastPosition=new PointF(x,y);
        string message=ordinal+". taşta takılma tespit edildi · başka konuma gidiyor · deneme "+routeStoneWatchdog.Attempts+"/3";
        routePatrolStatus.Text=message;RecordRecovery(message);return true;
    }
}
