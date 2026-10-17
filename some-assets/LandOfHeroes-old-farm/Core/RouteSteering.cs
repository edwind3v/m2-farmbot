using System;
public static class RouteSteering {
    public static bool CanKeepMoving(float angleDifference){return !float.IsNaN(angleDifference)&&!float.IsInfinity(angleDifference)&&Math.Abs(angleDifference)<=55f;}
    public static float Turn(float difference){return Math.Sign(difference)*Math.Min(Math.Abs(difference),45f);}
}
