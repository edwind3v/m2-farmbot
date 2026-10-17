using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

public partial class MainForm
{
    private long routeResumeAfter;
    private bool routeDeathRecovery;
    private string routeRecoveryId="";
    private RoutePoint reviveRouteWaypoint;
    private string RouteIntentPath{get{return Path.Combine(runtimeFolder,"route-intent.json");}}
    private void WriteRouteIntent(bool active)
    {
        var request=new{Pid=routeOwnerPid,Vid=routeOwnerVid,Map=routeRunningMap,Token=routeMotionToken,Active=active,
            Until=(DateTime.UtcNow.AddSeconds(2)-new DateTime(1970,1,1)).TotalSeconds};
        Directory.CreateDirectory(runtimeFolder);
        RouteRuntimeFile.TryWrite(RouteIntentPath,new JavaScriptSerializer().Serialize(request));
    }
    private void PauseRoutePatrol(string reason,int retryMilliseconds=2500)
    {
        if(!RouteOwnsCombat)return;
        // Preserve the cursor/target and user intent. No movement during retry backoff.
        try{SetRouteAttack(false);}catch{automation.AttackEnabled=false;}
        try{ReleaseMovement();}catch{}
        routeResumeAfter=movementClock.ElapsedMilliseconds+retryMilliseconds;
        routeLastProgress=movementClock.ElapsedMilliseconds;
        routePatrolStatus.Text="Rota beklemede · "+reason+"\nKapatmak için Rotayı durdur veya acil durdurma.";
    }
    private bool HandleRouteDeathRecovery()
    {
        if(!reviveEscape.BlockAttack)return false;
        SetRouteAttack(false);
        if(!routeDeathRecovery){
            routeDeathRecovery=true;
            ResetRouteContact();
            routeRecoveryId=Guid.NewGuid().ToString("N");
            // Keep an existing group's departure; otherwise save the last walking position.
            routePatrol.BeginDetour(TaskX,TaskY);routePatrol.EndDetour();
            ResetCombatFocus();routePursuitVid=0;routeStoneBatchVids.Clear();routeStoneBatchCenter=null;
            ResetRouteAutonomy(false);
            routeHealthReturn=null;routeHealthGuard=new RouteHealthGuard();routeNavigationPath=null;
            reviveRouteWaypoint=null;
        }
        bool blocked=TryRunReviveEscape();
        routeLastProgress=movementClock.ElapsedMilliseconds;routeLastPosition=new PointF(TaskX,TaskY);
        if(!blocked){routeDeathRecovery=false;reviveRouteWaypoint=null;routeNavigationPath=null;}
        // Resume on the next tick, never attack in a recovery tick.
        return true;
    }
    private bool RequestRouteRecoveryMount()
    {
        // Do not pulse inactive/active: a repeated Ctrl+G would dismount again.
        WriteRouteMotion(true,recovery:true,mount:true);
        try{
            string json;
            if(RouteRuntimeFile.TryRead(RouteMotionStatusPath,out json)){
                var state=new JavaScriptSerializer().Deserialize<RouteMountReport>(json);
                double now=(DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds;
                if(state!=null&&state.Pid==routeOwnerPid&&state.Token==routeMotionToken&&state.RecoveryId==routeRecoveryId&&state.State=="recovery-mounted"&&state.Mounted==true&&now-state.At<=1&&state.At-now<=1)return true;
            }
        }catch(IOException){}catch(ArgumentException){}catch(InvalidOperationException){}
        routePatrolStatus.Text="Canlandı · slotlardan uzak · Ctrl+G ile bineğe binme doğrulanıyor. Binek yoksa bekler.";
        return false;
    }
    private void MoveRouteRecovery(DungeonNavigator navigator,float x,float y,long now)
    {
        if(reviveRouteWaypoint==null||RoutePatrol.Distance(x,y,reviveRouteWaypoint.X,reviveRouteWaypoint.Y)<80){
            reviveRouteWaypoint=new RoutePoint{X=revivePoint.X,Y=revivePoint.Y};
            if(navigator!=null){
                var start=ToMapPixel(x,y);var path=navigator.FindPath((int)start.X,(int)start.Y,reviveGoal%navigator.Width,reviveGoal/navigator.Width);
                if(path==null)throw new InvalidOperationException("Slotlardan uzaklaşmak için yürünebilir yol bekleniyor.");
                foreach(int cell in path){
                    var p=MapPixelToMemory(new PointF(cell%navigator.Width+.5f,cell/navigator.Width+.5f));
                    if(RoutePatrol.Distance(x,y,p.X,p.Y)<160)continue;
                    reviveRouteWaypoint=new RoutePoint{X=p.X,Y=p.Y};break;
                }
            }
        }
        if(!RouteSegmentSafe(x,y,reviveRouteWaypoint.X,reviveRouteWaypoint.Y))throw new InvalidOperationException("Kaçış yolu yasak alanı kesiyor; güvenli yol bekleniyor.");
        CheckRouteProgress(x,y,now);CheckRouteEngineError();
        movementOutput?.Stop();
        WriteRouteMotion(true,reviveRouteWaypoint.X,reviveRouteWaypoint.Y,recovery:true);
    }
    private sealed class RouteMountReport
    {
        public int Pid{get;set;}public string Token{get;set;}public string State{get;set;}
        public string RecoveryId{get;set;}
        public double At{get;set;}public bool? Mounted{get;set;}
    }
}
