using System;
// Arrival is latched to the target's position, not the player's residual motion.
public sealed class ApproachPolicy
{
    private bool arrived, hasPrevious;
    private int targetId;
    private float previousX, previousY, anchorX, anchorY;
    public bool ShouldMove(int id, float playerX, float playerY, float targetX, float targetY, float stopDistance)
    {
        if (id <= 0 || !Finite(playerX) || !Finite(playerY) || !Finite(targetX) || !Finite(targetY)
            || !Finite(stopDistance) || stopDistance <= 0) { Reset(); return false; }
        if (targetId != id) { Reset(); targetId = id; }
        if (arrived && Distance(anchorX,anchorY,targetX,targetY) > Math.Max(100,stopDistance))
        { arrived = false; hasPrevious = false; }
        double distance = Distance(playerX,playerY,targetX,targetY);
        double crossed = distance;
        if (hasPrevious) {
            double dx=playerX-previousX, dy=playerY-previousY, length=dx*dx+dy*dy;
            double t=length>0 ? Math.Max(0,Math.Min(1,((targetX-previousX)*dx+(targetY-previousY)*dy)/length)) : 0;
            crossed=Distance(previousX+dx*t,previousY+dy*t,targetX,targetY);
        }
        if (!arrived && (distance <= stopDistance || crossed <= stopDistance))
        { arrived=true; anchorX=targetX; anchorY=targetY; }
        previousX=playerX; previousY=playerY; hasPrevious=true;
        return !arrived;
    }
    public void Reset() { arrived=false; hasPrevious=false; targetId=0; }
    private static double Distance(double x,double y,double tx,double ty) { double dx=x-tx,dy=y-ty;return Math.Sqrt(dx*dx+dy*dy); }
    private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
}
