using System;
using System.Collections.Generic;
using System.Linq;

// Cursor and detour state contain no game or UI side effects.
public sealed class RoutePatrol
{
    public readonly List<RoutePoint> Points;
    public int Index { get; private set; }
    public int Direction { get; private set; } = 1;
    public bool Detouring { get; private set; }
    public bool Returning { get; private set; }
    public RoutePoint ReturnPoint { get; private set; }
    public float Reach = 80;
    private RoutePoint previousReturnPosition;
    private RoutePoint previousWalkPosition;
    private int stableAimIndex=-1,stableAimDirection;
    private RoutePoint previousAimPosition;
    public RoutePatrol(IEnumerable<RoutePoint> points,float x,float y)
    {
        Points=points.Where(p=>p!=null&&Finite(p.X)&&Finite(p.Y)).Select(p=>new RoutePoint{X=p.X,Y=p.Y}).ToList();
        for(int i=Points.Count-1;i>0;i--)if(Distance(Points[i].X,Points[i].Y,Points[i-1].X,Points[i-1].Y)<1)Points.RemoveAt(i);
        if(Points.Count<2)throw new ArgumentException("Rota için en az iki geçerli nokta gerekli.");
        Index=Enumerable.Range(0,Points.Count).OrderBy(i=>Distance(x,y,Points[i].X,Points[i].Y)).First();
        double nearest=Distance(x,y,Points[Index].X,Points[Index].Y);RoutePoint projection=null;
        for(int i=1;i<Points.Count;i++){
            var a=Points[i-1];var b=Points[i];double dx=b.X-a.X,dy=b.Y-a.Y,length=dx*dx+dy*dy;if(length<1)continue;
            double t=Math.Max(0,Math.Min(1,((x-a.X)*dx+(y-a.Y)*dy)/length));
            var p=new RoutePoint{X=(float)(a.X+t*dx),Y=(float)(a.Y+t*dy)};double distance=Distance(x,y,p.X,p.Y);
            if(distance<nearest-.001){nearest=distance;projection=p;Index=i;}
        }
        if(nearest>Reach){ReturnPoint=projection??new RoutePoint{X=Points[Index].X,Y=Points[Index].Y};Returning=true;}
    }
    public RoutePoint Next(float x,float y)
    {
        if(Detouring)return null;
        if(Returning)
        {
            bool crossed=previousReturnPosition!=null&&SegmentDistance(ReturnPoint.X,ReturnPoint.Y,previousReturnPosition,new RoutePoint{X=x,Y=y})<=Reach;
            previousReturnPosition=new RoutePoint{X=x,Y=y};
            if(Distance(x,y,ReturnPoint.X,ReturnPoint.Y)>Reach&&!crossed)return ReturnPoint;
            Returning=false;ReturnPoint=null;previousReturnPosition=null;
        }
        // Bounded even for a legacy file full of duplicate points.
        for(int n=0;n<Points.Count&&ReachedOrPassed(x,y);n++)
        {
            int oldDirection=Direction;
            if(Index==Points.Count-1)Direction=-1;
            else if(Index==0)Direction=1;
            Index+=Direction;
            if(Direction!=oldDirection)break; // Never consume a reverse leg with forward samples.
        }
        previousWalkPosition=new RoutePoint{X=x,Y=y};
        return Points[Index];
    }
    private bool ReachedOrPassed(float x,float y) {
        var end=Points[Index];int previous=Index-Direction;
        int neighbor=previous>=0&&previous<Points.Count?previous:Index+Direction;
        double arrival=Reach;
        if(neighbor>=0&&neighbor<Points.Count)arrival=Math.Max(5,Math.Min(arrival,Distance(end.X,end.Y,Points[neighbor].X,Points[neighbor].Y)*.35));
        if(Distance(x,y,end.X,end.Y)<=arrival)return true;
        // Crossing is physical evidence between consecutive samples, not an unbounded half-plane.
        if(previousWalkPosition==null)return false;
        if(SegmentDistance(end.X,end.Y,previousWalkPosition,new RoutePoint{X=x,Y=y})<=arrival)return true;
        if(previous<0||previous>=Points.Count)return false;
        var start=Points[previous];double dx=end.X-start.X,dy=end.Y-start.Y,length=dx*dx+dy*dy;
        if(length<1)return false;
        double before=((previousWalkPosition.X-start.X)*dx+(previousWalkPosition.Y-start.Y)*dy)/length;
        double after=((x-start.X)*dx+(y-start.Y)*dy)/length;
        // A physical crossing of this endpoint's plane, inside the route corridor.
        // A stationary actor or a remote half-plane cannot consume a waypoint.
        return before<1&&after>=1&&SegmentDistance(x,y,start,end)<=Reach&&
            SegmentDistance(previousWalkPosition.X,previousWalkPosition.Y,start,end)<=Reach;
    }
    public RoutePoint Aim(float x,float y,float lookAhead=350) {
        if(Detouring||Returning){stableAimIndex=-1;previousAimPosition=null;return Next(x,y);}
        if(stableAimIndex>=0&&stableAimDirection==Direction){
            var stable=Points[stableAimIndex];
            bool arrived=Distance(x,y,stable.X,stable.Y)<=5||
                (previousAimPosition!=null&&SegmentDistance(stable.X,stable.Y,previousAimPosition,new RoutePoint{X=x,Y=y})<=5);
            // The combined straight leg was physically reached: consume its
            // intermediate markers even if sparse host samples missed one.
            if(arrived&&(stableAimIndex-Index)*Direction>=0)Index=stableAimIndex;
        }
        previousAimPosition=new RoutePoint{X=x,Y=y};
        var point=Next(x,y);if(point==null||Returning)return point;
        if(stableAimIndex>=0&&stableAimDirection==Direction&&(stableAimIndex-Index)*Direction>=0)
            return Points[stableAimIndex];
        stableAimIndex=Index;stableAimDirection=Direction;
        if(Distance(x,y,point.X,point.Y)>=lookAhead)return point;
        int previous=Index-Direction;var start=previous>=0&&previous<Points.Count?Points[previous]:new RoutePoint{X=x,Y=y};
        double ax=point.X-start.X,ay=point.Y-start.Y,al=Math.Sqrt(ax*ax+ay*ay);
        for(int i=Index+Direction;i>=0&&i<Points.Count;i+=Direction){
            var next=Points[i];double bx=next.X-point.X,by=next.Y-point.Y,bl=Math.Sqrt(bx*bx+by*by);
            if(bl<1)continue;
            // Only co-linear recorded segments are combined: never skip a real corner.
            if(al<1||(ax*bx+ay*by)/(al*bl)<0.985)break;
            point=next;stableAimIndex=i;ax=bx;ay=by;al=bl;if(Distance(x,y,point.X,point.Y)>=lookAhead)break;
        }
        return point;
    }
    public void BeginDetour(float x,float y)
    {
        // Save ONE departure point and cursor until the fight and return finish.
        if(Detouring||Returning)return;
        stableAimIndex=-1;previousAimPosition=null;
        previousReturnPosition=null;
        ReturnPoint=new RoutePoint{X=x,Y=y};Detouring=true;
        previousWalkPosition=null;
    }
    public void EndDetour(){if(!Detouring)return;Detouring=false;Returning=true;previousReturnPosition=null;}
    public void RejoinClosestLine(float x,float y)
    {
        double best=double.MaxValue;RoutePoint join=null;int endpoint=Index;
        for(int i=1;i<Points.Count;i++){
            var a=Points[i-1];var b=Points[i];double dx=b.X-a.X,dy=b.Y-a.Y,length=dx*dx+dy*dy;
            if(length<1)continue;
            double t=Math.Max(0,Math.Min(1,((x-a.X)*dx+(y-a.Y)*dy)/length));
            var p=new RoutePoint{X=(float)(a.X+t*dx),Y=(float)(a.Y+t*dy)};double d=Distance(x,y,p.X,p.Y);
            if(d<best){best=d;join=p;endpoint=Direction>0?i:i-1;}
        }
        if(join==null)join=new RoutePoint{X=Points[Index].X,Y=Points[Index].Y};
        Index=endpoint;Detouring=false;Returning=Distance(x,y,join.X,join.Y)>Reach;
        stableAimIndex=-1;previousAimPosition=null;
        ReturnPoint=Returning?join:null;previousReturnPosition=null;
        previousWalkPosition=null;
    }
    public bool NearCurrentRoute(float x,float y,float playerX,float playerY,float range)
    {
        if(Returning||Detouring)return false;
        return NearRouteCorridor(x,y,playerX,playerY,range);
    }
    public bool NearRouteCorridor(float x,float y,float playerX,float playerY,float range)
    {
        // The selected range is the radius of a disk swept over the recorded
        // route.  Eligibility therefore depends on the route corridor, not on
        // a second circle centred on the player's current position.  The live
        // radar remains the finite observation boundary.
        if(range<=0||!Finite(range)||!Finite(x)||!Finite(y))return false;
        return DistanceToLine(x,y)<=range;
    }
    public double DistanceToLine(float x,float y) {
        double best=double.MaxValue;
        for(int i=1;i<Points.Count;i++)best=Math.Min(best,SegmentDistance(x,y,Points[i-1],Points[i]));
        return best;
    }
    public static double SegmentDistance(float x,float y,RoutePoint a,RoutePoint b)
    {
        double dx=b.X-a.X,dy=b.Y-a.Y,length=dx*dx+dy*dy;
        double t=length==0?0:Math.Max(0,Math.Min(1,((x-a.X)*dx+(y-a.Y)*dy)/length));
        return Distance(x,y,a.X+t*dx,a.Y+t*dy);
    }
    public static double Distance(double x,double y,double tx,double ty){double dx=x-tx,dy=y-ty;return Math.Sqrt(dx*dx+dy*dy);}
    private static bool Finite(float v){return !float.IsNaN(v)&&!float.IsInfinity(v);}
}
