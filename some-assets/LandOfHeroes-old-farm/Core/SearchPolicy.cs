using System;
public sealed class SearchPolicy
{
    private bool initialized,fixedCenter;
    private float centerX,centerY;
    private int point;
    private long pointStarted;
    public int CurrentPoint { get { return initialized ? point + 1 : 0; } }
    public void Reset() { initialized=false;fixedCenter=false; }
    public RoutePoint Next(float x,float y,float range,long now)
    {
        if(!Valid(x)||!Valid(y)||!Valid(range)||range<100){Reset();return null;}
        if(!initialized||fixedCenter)Initialize(x,y,now,false);
        return NextCore(x,y,range,now,0.92f);
    }
    public RoutePoint NextInArea(float x,float y,float selectedX,float selectedY,float range,long now)
    {
        if(!Valid(x)||!Valid(y)||!Valid(selectedX)||!Valid(selectedY)||!Valid(range)||range<100){Reset();return null;}
        if(!initialized||!fixedCenter||Math.Abs(centerX-selectedX)>1||Math.Abs(centerY-selectedY)>1)Initialize(selectedX,selectedY,now,true);
        return NextCore(x,y,range,now,1f);
    }
    private void Initialize(float x,float y,long now,bool selected){centerX=x;centerY=y;point=0;pointStarted=now;initialized=true;fixedCenter=selected;}
    private RoutePoint NextCore(float x,float y,float range,long now,float coverage)
    {
        float radius=Math.Min(range,200000)*coverage;
        var next=At(radius);double dx=x-next.X,dy=y-next.Y;
        long pointTimeout=(long)Math.Max(8000,Math.Min(30000,radius/600f*1000f));
        if(dx*dx+dy*dy<=Math.Pow(Math.Min(200,radius*0.12),2)||now-pointStarted>=pointTimeout)
        {point=(point+1)%8;pointStarted=now;next=At(radius);}
        return next;
    }
    private static bool Valid(float value){return !float.IsNaN(value)&&!float.IsInfinity(value);}
    private RoutePoint At(float radius) { double angle=point*Math.PI/4;return new RoutePoint{X=centerX+(float)Math.Cos(angle)*radius,Y=centerY+(float)Math.Sin(angle)*radius}; }
}

