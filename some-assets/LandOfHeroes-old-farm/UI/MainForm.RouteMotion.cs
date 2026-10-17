using System;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

public partial class MainForm
{
    private int routeOwnerVid;
    private string routeMotionToken="";
    private bool routeMotionRequested;
    private string RouteMotionPath {get{return Path.Combine(runtimeFolder,"route-motion.json");}}
    private string RouteMotionStatusPath {get{return Path.Combine(runtimeFolder,"route-motion-status.json");}}
    private void WriteRouteMotion(bool active,float x=0,float y=0,int targetVid=0,bool recovery=false,bool mount=false,bool hold=false,float holdDistance=370)
    {
        var request=new {Pid=routeOwnerPid,Vid=routeOwnerVid,Map=routeRunningMap,Token=routeMotionToken,Active=active,X=x,Y=y,TargetVid=targetVid,HoldAtTarget=hold,HoldDistance=holdDistance,Recovery=recovery,RecoveryMount=mount,RecoveryId=routeRecoveryId,Arrival=recovery?60:5,
            Until=(DateTime.UtcNow.AddMilliseconds(800)-new DateTime(1970,1,1)).TotalSeconds};
        Directory.CreateDirectory(runtimeFolder);
        // Do not rename/delete a snapshot while the interpreter holds it open.
        // A transient lock is retried next route tick; the old lease expires independently.
        if(RouteRuntimeFile.TryWrite(RouteMotionPath,new JavaScriptSerializer().Serialize(request)))routeMotionRequested=active;
        else if(active)routeMotionRequested=true; // A failed stop must remain retryable.
    }
    private void StopRouteEngineMotion()
    {
        if(!routeMotionRequested)return;
        WriteRouteMotion(false);
    }
    private void RequestRouteEngineMotion(float x,float y)
    {
        if(emergencyLatched||MountSkillBusy){StopRouteEngineMotion();return;}
        CheckRouteEngineError();
        WriteRouteMotion(true,x,y);
    }
    private void RequestRouteAttackQueue(int vid,bool hold=false,float holdDistance=370)
    {
        if(emergencyLatched||MountSkillBusy){StopRouteEngineMotion();return;}
        if(vid<=0)throw new InvalidOperationException("Kuyruk hedefi gecersiz.");
        CheckRouteEngineError();
        // One leased request selects ONE owner: joystick OR the game's queue.
        // Do not send an inactive/active pulse while already attacking this VID.
        movementOutput?.Stop();
        WriteRouteMotion(true,targetVid:vid,hold:hold,holdDistance:holdDistance);
    }
    private void CheckRouteEngineError()
    {
        // Fail closed on an explicit error from this run. Old-run telemetry cannot stop a new run.
        try{
            if(File.Exists(RouteMotionStatusPath)&&DateTime.UtcNow-File.GetLastWriteTimeUtc(RouteMotionStatusPath)<TimeSpan.FromSeconds(2)){
                RouteMotionReport state=null;
                try {string json;if(RouteRuntimeFile.TryRead(RouteMotionStatusPath,out json))state=new JavaScriptSerializer().Deserialize<RouteMotionReport>(json);}
                catch(ArgumentException){} // Telemetry can be read while the agent rewrites it.
                catch(InvalidOperationException){}
                if(state!=null&&state.Pid==routeOwnerPid&&state.Token==routeMotionToken&&!string.IsNullOrEmpty(state.Error))
                    throw new InvalidOperationException("Rota hareket motoru: "+state.Error);
            }
        }catch(IOException){}
    }
    private sealed class RouteMotionReport
    {
        public int Pid {get;set;}public string Token {get;set;}public string Error {get;set;}
    }
}
