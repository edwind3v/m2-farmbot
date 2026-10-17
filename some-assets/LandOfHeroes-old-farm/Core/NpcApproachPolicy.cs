using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

// NPC interaction has its own stand-off range; combat's close-contact range never applies here.
public static class NpcApproachPolicy
{
    public const float MinimumDistance=300f,MaximumDistance=500f,PreferredDistance=400f;
    public static bool Ready(float distance){return !float.IsNaN(distance)&&!float.IsInfinity(distance)&&distance>=MinimumDistance&&distance<=MaximumDistance;}
    static float Distance(PointF a,PointF b){float x=a.X-b.X,y=a.Y-b.Y;return (float)Math.Sqrt(x*x+y*y);}
    public static bool TryPlan(PointF origin,PointF npc,IEnumerable<Tuple<PointF,float>> obstacles,Func<PointF,bool> blocked,int attempt,out List<PointF> path)
    {
        path=null;
        if(float.IsNaN(origin.X)||float.IsInfinity(origin.X)||float.IsNaN(origin.Y)||float.IsInfinity(origin.Y)||float.IsNaN(npc.X)||float.IsInfinity(npc.X)||float.IsNaN(npc.Y)||float.IsInfinity(npc.Y))return false;
        const float unit=100f;
        float ox=(float)Math.Floor((Math.Min(origin.X,npc.X)-1600)/unit)*unit,oy=(float)Math.Floor((Math.Min(origin.Y,npc.Y)-1600)/unit)*unit;
        int width=(int)Math.Ceiling((Math.Max(origin.X,npc.X)+1600-ox)/unit),height=(int)Math.Ceiling((Math.Max(origin.Y,npc.Y)+1600-oy)/unit);
        if(width<1||height<1||width>256||height>256)return false;
        var actors=(obstacles??Enumerable.Empty<Tuple<PointF,float>>()).ToList();
        Func<int,int,PointF> point=(x,y)=>new PointF(ox+x*unit+50,oy+y*unit+50);
        var nav=new DungeonNavigator(width,height,(x,y)=>{
            var p=point(x,y);
            return (blocked!=null&&blocked(p))||Distance(p,npc)<230||actors.Any(a=>Distance(p,a.Item1)<a.Item2+80);
        });
        int sx=(int)Math.Floor((origin.X-ox)/unit),sy=(int)Math.Floor((origin.Y-oy)/unit);
        double baseAngle=Math.Atan2(origin.Y-npc.Y,origin.X-npc.X);
        for(int i=0;i<16;i++)
        {
            // Try the near side first; a stalled route changes the approach side instead of reversing blindly.
            int turn=i==0?0:(i%2==1?(i+1)/2:-(i/2));
            double angle=baseAngle+(turn*22.5+Math.Max(0,attempt)*45)*Math.PI/180;
            var desired=new PointF(npc.X+(float)Math.Cos(angle)*PreferredDistance,npc.Y+(float)Math.Sin(angle)*PreferredDistance);
            int tx=(int)Math.Floor((desired.X-ox)/unit),ty=(int)Math.Floor((desired.Y-oy)/unit);
            if(!nav.Walkable(tx,ty))continue;
            var goal=point(tx,ty);if(!Ready(Distance(goal,npc))||(attempt>0&&Distance(origin,goal)<150))continue;
            var cells=nav.FindPath(sx,sy,tx,ty);if(cells==null)continue;
            path=cells.Select(c=>point(c%width,c/width)).ToList();return true;
        }
        return false;
    }
}

public sealed class NpcApproachProgress
{
    PointF last;long progressedAt,recoveryAt;bool started;
    public void Reset(){started=false;}
    public bool Observe(PointF position,long now)
    {
        if(!started){started=true;last=position;progressedAt=now;recoveryAt=now-4000;return false;}
        float dx=position.X-last.X,dy=position.Y-last.Y;
        if(dx*dx+dy*dy>=35*35){last=position;progressedAt=now;return false;}
        if(now-progressedAt<1800||now-recoveryAt<4000)return false;
        recoveryAt=progressedAt=now;return true;
    }
}
