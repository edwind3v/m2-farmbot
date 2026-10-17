using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

public sealed class RouteZone
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Radius { get; set; }
    public bool Contains(float x,float y,float margin=0) { return RoutePatrol.Distance(x,y,X,Y)<=Radius+margin; }
    public bool Intersects(RoutePoint a,RoutePoint b,float margin=0) { return RoutePatrol.SegmentDistance(X,Y,a,b)<=Radius+margin; }
}

// Separate from the existing global settings: loading a route must not alter another bot.
public sealed class RouteProfile
{
    public int Version { get; set; } = 1;
    public string Name { get; set; } = "Yeni rota";
    public string Map { get; set; } = "";
    public List<RoutePoint> Points { get; set; } = new List<RoutePoint>();
    public List<RouteZone> Zones { get; set; } = new List<RouteZone>();
    public bool Farm { get; set; }
    public bool Stones { get; set; } = true;
    public bool Monsters { get; set; }
    public bool Mixed { get; set; }
    public int Range { get; set; } = 3000;
    public int StopDistance { get; set; } = 250;
    public int AttackMilliseconds { get; set; } = 180;
    public bool HybridAttack { get; set; }
    public bool RangedWait { get; set; }
    public bool TargetedAttack { get; set; }
    public bool FinishNearbyStones { get; set; } = true;
    public int StoneBatchRange { get; set; } = 1000;
    public int AttackRange { get; set; } = 250;
    public bool ObstaclePass { get; set; }
    public bool MountedObstaclePass { get; set; }
    public bool StoneMonsterSplash { get; set; }
    public bool RangePickup { get; set; } = true;
    // 0 nearest, 1 boss/stone/monster, 2 stone/monster, 3 monster/stone.
    public int Priority { get; set; }
    public string Names { get; set; } = "";
    public string BossNames { get; set; } = "";
    public bool Retreat { get; set; }
    public int LowHealth { get; set; } = 30;
    public int ResumeHealth { get; set; } = 70;
    public RoutePoint SafePoint { get; set; }
    public bool Recovery { get; set; } = true;
    public bool Forbidden(float x,float y,float margin=0) { return Zones.Any(z=>z.Contains(x,y,margin)); }
    public bool SegmentForbidden(RoutePoint a,RoutePoint b,float margin=0) { return Zones.Any(z=>z.Intersects(a,b,margin)); }
    public static bool Finite(float n) { return !float.IsNaN(n)&&!float.IsInfinity(n); }
    static bool ValidPoint(RoutePoint p) { return p!=null&&Finite(p.X)&&Finite(p.Y); }
    public void Validate(bool runnable=false)
    {
        if(Version!=1||string.IsNullOrWhiteSpace(Name)||Name.Length>100||Map==null||Map.Length>256)throw new InvalidDataException("Profil adı veya sürümü geçersiz.");
        if(Points==null||Points.Count>20000||Points.Any(p=>!ValidPoint(p))||Zones==null||Zones.Count>100||Zones.Any(z=>z==null||!Finite(z.X)||!Finite(z.Y)||!Finite(z.Radius)||z.Radius<50||z.Radius>1000000))throw new InvalidDataException("Rota noktası veya yasak alan geçersiz.");
        if(Range<1||Range>1000000||StopDistance<100||StopDistance>300||AttackMilliseconds<20||AttackMilliseconds>1000||Priority<0||Priority>3||LowHealth<1||ResumeHealth>100||ResumeHealth<=LowHealth)throw new InvalidDataException("Rota ayarları veya can eşikleri geçersiz.");
        if(AttackRange<100||AttackRange>100000)throw new InvalidDataException("Rota vuruş menzili 100–100000 arasında olmalı.");
        if(StoneBatchRange<100||StoneBatchRange>9500)throw new InvalidDataException("Taş grubu mesafesi 100–9500 arasında olmalı.");
        if((Names??"").Length>4000||(BossNames??"").Length>4000||SafePoint!=null&&!ValidPoint(SafePoint))throw new InvalidDataException("Hedef adları veya güvenli nokta geçersiz.");
        if(runnable&&(Points.Count<2||string.IsNullOrWhiteSpace(Map)||Points.Any(p=>Forbidden(p.X,p.Y,100))))throw new InvalidDataException("En az iki nokta ve doğru harita gerekli; rota noktaları yasak alanın en az 100 birim dışında olmalı.");
        if(Retreat&&(SafePoint==null||Forbidden(SafePoint.X,SafePoint.Y,100)))throw new InvalidDataException("Cana göre çekilme için yasak alan dışında güvenli nokta işaretle.");
    }
    public static bool Matches(string name,string tokens)
    {
        return (tokens??"").Split(new[]{',',';','\n','\r'},StringSplitOptions.RemoveEmptyEntries).Select(t=>t.Trim()).Where(t=>t.Length>0).Any(t=>string.Equals(name,t,StringComparison.OrdinalIgnoreCase));
    }
    public bool AcceptName(string name) { return string.IsNullOrWhiteSpace(Names)||Matches(name,Names); }
    public int Rank(bool boss,bool stone)
    {
        if(Priority==1)return boss?0:stone?1:2;
        if(Priority==2)return stone?0:1;
        if(Priority==3)return stone?1:0;
        return 0;
    }
    public RouteProfile Copy() { return new JavaScriptSerializer().Deserialize<RouteProfile>(new JavaScriptSerializer().Serialize(this)); }
}

public static class RouteProfileStore
{
    public static RouteProfile Load(string path)
    {
        if(new FileInfo(path).Length>4000000)throw new InvalidDataException("Profil dosyası çok büyük.");
        var result=new JavaScriptSerializer{MaxJsonLength=4000000}.Deserialize<RouteProfile>(File.ReadAllText(path));
        if(result==null)throw new InvalidDataException("Profil boş.");result.Validate();return result;
    }
    public static void Save(string path,RouteProfile value)
    {
        value.Validate();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            File.WriteAllText(temporary,new JavaScriptSerializer{MaxJsonLength=4000000}.Serialize(value));
            if(File.Exists(path))File.Replace(temporary,path,path+".bak");else File.Move(temporary,path);
        }
        finally { if(File.Exists(temporary))File.Delete(temporary); }
    }
}

public sealed class PlayerHealthSnapshot
{
    public int Pid { get; set; }
    public string Map { get; set; }
    public double At { get; set; }
    public float? PlayerHp { get; set; }
    public bool? Mounted { get; set; }
    public static PlayerHealthSnapshot Fresh(string json,int pid,string map,DateTime now)
    {
        try{
            var s=new JavaScriptSerializer().Deserialize<PlayerHealthSnapshot>(json);
            double seconds=(now-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds;
            if(s==null||pid<=0||s.Pid!=pid||string.IsNullOrEmpty(map)||s.Map!=map||double.IsNaN(s.At)||double.IsInfinity(s.At)||seconds<s.At||seconds-s.At>2)return null;
            return s;
        }catch{return null;}
    }
    public static float? Parse(string json,int pid,string map,DateTime now)
    {
        try
        {
            var s=new JavaScriptSerializer().Deserialize<PlayerHealthSnapshot>(json);
            double seconds=(now-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds;
            if(s==null||pid<=0||s.Pid!=pid||string.IsNullOrEmpty(map)||s.Map!=map||double.IsNaN(s.At)||double.IsInfinity(s.At)||seconds<s.At||seconds-s.At>2||!s.PlayerHp.HasValue||!RouteProfile.Finite(s.PlayerHp.Value)||s.PlayerHp<0||s.PlayerHp>100)return null;
            return s.PlayerHp;
        }
        catch{return null;}
    }
}

public enum RouteHealthAction { Continue, Hold, Retreat, Recover }
public sealed class RouteHealthGuard
{
    public bool Withdrawing { get; private set; }
    private bool recovering;
    public RouteHealthAction Evaluate(float? hp,bool atSafePoint,int low,int resume)
    {
        if(!hp.HasValue||!RouteProfile.Finite(hp.Value)||hp<=0||hp>100)return RouteHealthAction.Hold;
        if(hp<=low)Withdrawing=true;
        if(!Withdrawing)return RouteHealthAction.Continue;
        recovering=atSafePoint;
        if(recovering&&hp>=resume){Withdrawing=false;recovering=false;return RouteHealthAction.Continue;}
        return recovering?RouteHealthAction.Recover:RouteHealthAction.Retreat;
    }
}
