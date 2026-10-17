using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// Only the game-confirmed name/VID is authoritative; native radar records may have no name or Z=0.
public sealed class DungeonNpcTracker
{
    private int stage=-1;
    private DateTime observedAt=DateTime.MinValue;
    private EntityState target;
    public int NpcVid { get; private set; }
    public EntityState Update(int currentStage,string snapshot,DateTime writtenAt,DateTime now,string name,IEnumerable<EntityState> radar,float offsetX,float offsetY,int ySign)
    {
        if(stage!=currentStage){stage=currentStage;NpcVid=0;target=null;observedAt=DateTime.MinValue;}
        if(now-observedAt>TimeSpan.FromSeconds(2)){NpcVid=0;target=null;}
        var fields=(snapshot??"").Trim().Split('|');int scope,vid;float px,py;
        if(writtenAt>now||now-writtenAt>TimeSpan.FromSeconds(2)||fields.Length<2||!int.TryParse(fields[0],out scope)||scope!=stage||!int.TryParse(fields[1],out vid)||vid<0)return target;
        if(vid==0&&fields.Length==2){NpcVid=0;target=null;observedAt=writtenAt;return null;}
        if(vid>0&&fields.Length==4&&float.TryParse(fields[2],NumberStyles.Float,CultureInfo.InvariantCulture,out px)&&float.TryParse(fields[3],NumberStyles.Float,CultureInfo.InvariantCulture,out py)&&!float.IsNaN(px)&&!float.IsInfinity(px)&&!float.IsNaN(py)&&!float.IsInfinity(py))
        {
            NpcVid=vid;observedAt=writtenAt;
            target=new EntityState{Property_03C9=vid,Property_03CA=1,Property_03CD=name,X=px+offsetX,Y=py*(ySign<0?-1:1)+offsetY};return target;
        }
        if(vid>0&&fields.Length==3&&fields[2]=="none")
        {
            NpcVid=vid;observedAt=writtenAt;target=(radar??Enumerable.Empty<EntityState>()).FirstOrDefault(e=>e.Property_03C9==vid&&e.Property_03CA==1);return target;
        }
        return target;
    }
    // A final room may contain several navigation regions; that never makes it a symbol maze.
    public static bool ShouldTalkAtExit(int stage,bool isMaze){return stage>=8||(stage!=4&&!isMaze);}
}
