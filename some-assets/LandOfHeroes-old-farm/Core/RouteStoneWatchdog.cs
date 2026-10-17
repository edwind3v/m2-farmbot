public enum RouteStoneWatchAction { None, Reposition, Skip }

// A route target is healthy only while its real server HP is changing. Repeated
// attack commands or tiny character movements must never hide a stalled stone.
public sealed class RouteStoneWatchdog
{
    private readonly StoneRoomRecovery progress=new StoneRoomRecovery();
    public int Attempts { get { return progress.Attempts; } }
    public void Reset(){progress.Reset();}
    public RouteStoneWatchAction Observe(bool stone,int vid,float? hp,bool inAttackPosition,long now)
    {
        if(!progress.Observe(stone,vid,hp,inAttackPosition,now))return RouteStoneWatchAction.None;
        return progress.Attempts>=3?RouteStoneWatchAction.Skip:RouteStoneWatchAction.Reposition;
    }
}

// Separately catches wall/queue stalls before attack distance is reached. Small
// coordinate jitter is not accepted as progress.
public sealed class RouteApproachWatchdog
{
    public const long StalledMilliseconds=6000;
    public const float ProgressDistance=80;
    private int vid;private long progressAt;private float x,y;
    public int Attempts { get; private set; }
    public void Reset(){vid=0;progressAt=0;x=y=0;Attempts=0;}
    public RouteStoneWatchAction Observe(bool stone,int targetVid,float playerX,float playerY,bool stationary,long now)
    {
        if(!stone||targetVid<=0||stationary){Reset();return RouteStoneWatchAction.None;}
        if(vid!=targetVid){Reset();vid=targetVid;x=playerX;y=playerY;progressAt=now;return RouteStoneWatchAction.None;}
        if(RoutePatrol.Distance(playerX,playerY,x,y)>=ProgressDistance){x=playerX;y=playerY;progressAt=now;return RouteStoneWatchAction.None;}
        if(now-progressAt<StalledMilliseconds)return RouteStoneWatchAction.None;
        progressAt=now;Attempts++;return Attempts>=3?RouteStoneWatchAction.Skip:RouteStoneWatchAction.Reposition;
    }
}
