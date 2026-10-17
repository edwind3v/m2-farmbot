using System;

// Observed target HP is evidence of progress, not proof of our hit count or damage attribution.
public sealed class DamageProgressWatch
{
    private string identity;
    private float lastHp;
    private double sampleAt;
    private long lastObservation,progressAt;
    private bool valid,reported;
    public bool Stalled {get{return reported;}}
    public void Reset(){identity=null;valid=reported=false;sampleAt=0;}
    public bool Observe(string scope,int vid,float? hp,double at,long now,bool attacking,int timeoutMilliseconds)
    {
        if(!attacking||vid<=0||!hp.HasValue||!RouteProfile.Finite(hp.Value)||hp<=0||hp>100||double.IsNaN(at)||double.IsInfinity(at)){Reset();return false;}
        string key=scope+"|"+vid;
        if(!valid||identity!=key||now-lastObservation>1500||at<sampleAt){identity=key;lastHp=hp.Value;sampleAt=at;lastObservation=progressAt=now;valid=true;reported=false;return false;}
        if(at==sampleAt)return false;
        lastObservation=now;sampleAt=at;
        if(hp.Value!=lastHp){progressAt=now;reported=false;lastHp=hp.Value;return false;}
        if(!reported&&now-progressAt>=Math.Max(3000,timeoutMilliseconds)){reported=true;return true;}
        return false;
    }
}
