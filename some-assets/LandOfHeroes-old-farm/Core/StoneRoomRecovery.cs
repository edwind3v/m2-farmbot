using System;

// Actual target HP progress, rather than the number of attack commands sent.
public sealed class StoneRoomRecovery
{
    public const long StalledMilliseconds=4000,UnknownHpMilliseconds=6000,CooldownMilliseconds=6000;
    private int targetVid;private long progressAt,nextRetry;private float? lastHp;
    public int Attempts{get;private set;}
    public void Reset(){targetVid=0;progressAt=nextRetry=0;lastHp=null;Attempts=0;}
    public bool Observe(bool eligible,int vid,float? hp,bool inRange,long now)
    {
        if(!eligible||vid<=0){Reset();return false;}
        if(targetVid!=vid){Reset();targetVid=vid;progressAt=now;}
        if(hp.HasValue&&(float.IsNaN(hp.Value)||float.IsInfinity(hp.Value)||hp.Value<0||hp.Value>100))hp=null;
        if(hp.HasValue)
        {
            if(!lastHp.HasValue||hp.Value<lastHp.Value-0.001f||hp.Value>lastHp.Value+0.001f)progressAt=now;
            lastHp=hp;
        }
        if(!inRange){progressAt=now;return false;}
        long timeout=lastHp.HasValue?StalledMilliseconds:UnknownHpMilliseconds;
        if(now<nextRetry||now-progressAt<timeout)return false;
        progressAt=now;nextRetry=now+CooldownMilliseconds;Attempts++;return true;
    }
}
