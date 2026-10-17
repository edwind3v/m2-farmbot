using System;
using System.Globalization;

public sealed class ReviveEscapePolicy
{
    public bool Dead{get;private set;}
    public bool NeedsEscape{get;private set;}
    public bool BlockAttack{get{return Dead||NeedsEscape;}}
    private bool initialized,observedDeath;private int serial,pid;private double observedAt;
    public bool Fresh(double now){return initialized&&now-observedAt<=2&&observedAt-now<=1;}
    public void Reset(){Dead=NeedsEscape=initialized=observedDeath=false;serial=pid=0;observedAt=0;}
    public bool Observe(string text,int expectedPid,double now)
    {
        var p=(text??"").Trim().Split('|');double stamp,hp,max;int nextSerial,sourcePid;
        if(p.Length!=5||!double.TryParse(p[0],NumberStyles.Float,CultureInfo.InvariantCulture,out stamp)||!double.TryParse(p[1],NumberStyles.Float,CultureInfo.InvariantCulture,out hp)||!double.TryParse(p[2],NumberStyles.Float,CultureInfo.InvariantCulture,out max)||!int.TryParse(p[3],out nextSerial)||!int.TryParse(p[4],out sourcePid)||sourcePid!=expectedPid||expectedPid<=0||double.IsNaN(stamp)||double.IsNaN(hp)||double.IsNaN(max)||double.IsInfinity(stamp)||double.IsInfinity(hp)||double.IsInfinity(max)||max<=0||now-stamp>2||stamp-now>1||nextSerial<0)return false;
        if(pid!=expectedPid){Reset();pid=expectedPid;}
        observedAt=stamp;
        bool isDead=hp<=0;
        if(!initialized){initialized=true;serial=nextSerial;Dead=isDead;observedDeath=isDead;return true;}
        if(isDead){Dead=true;observedDeath=true;NeedsEscape=false;}
        else
        {
            if(observedDeath||nextSerial>serial)NeedsEscape=true;
            Dead=false;observedDeath=false;
        }
        serial=nextSerial;return true;
    }
    public void Escaped(){NeedsEscape=false;}
}
