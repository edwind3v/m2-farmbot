using System;

public partial class MainForm
{
    private int routeCollisionHoldVid,routeStationaryTargetVid;
    private bool UpdateRouteContact(EntityState target,float x,float y,float close)
    {
        int vid=target.Property_03C9;
        double distance=RoutePatrol.Distance(x,y,target.X,target.Y);
        // Keep the selected collision bypass active for the whole approach.
        // A stone may legitimately be behind or partly inside map geometry;
        // braking hundreds of units early leaves the character pinned to the
        // wall even though the user enabled obstacle traversal.  Latch the
        // brake only after attack distance has actually been reached, then
        // retain it for this target so small knockback cannot alternate
        // pass-through/solid while attacking.
        if(routeCollisionHoldVid!=vid){
            routeCollisionHoldVid=0;
            routeStationaryTargetVid=0;
        }
        if(distance<=close)routeCollisionHoldVid=vid;
        ApplyCollisionFlags(true);
        bool stationary=distance<=close||(routeStationaryTargetVid==vid&&distance<=close+120);
        routeStationaryTargetVid=stationary?vid:0;
        return stationary;
    }
    private void ResetRouteContact()
    {
        routeCollisionHoldVid=routeStationaryTargetVid=0;
        ApplyCollisionFlags();
    }
}
