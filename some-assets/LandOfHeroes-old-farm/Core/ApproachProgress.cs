using System;
public sealed class ApproachProgress
{
    private int id;
    private long started, lastProgress;
    private double best;
    public void Reset() { id=0; }
    public bool CanContinue(int target,double distance,long now)
    {
        if(double.IsNaN(distance)||double.IsInfinity(distance)||distance<0)return false;
        if(target!=id) { id=target;started=lastProgress=now;best=distance;return true; }
        if(distance<best-20){best=distance;lastProgress=now;}
        return distance<=best+250 && now-lastProgress<2000 && now-started<15000;
    }
}
