using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

public sealed class RouteCheckIssue
{
    public int PointIndex {get;set;}
    public bool Error {get;set;}
    public string Message {get;set;}
    public override string ToString(){return (Error?"HATA · ":"BİLGİ · ")+(PointIndex>=0?"Nokta "+(PointIndex+1)+" · ":"")+Message;}
}
public sealed class RouteCheckResult
{
    public List<RouteCheckIssue> Issues {get;set;}=new List<RouteCheckIssue>();
    public int Segments {get;set;}
    public bool Passed {get{return !Issues.Exists(i=>i.Error);}}
}
public static class RoutePreflight
{
    public static RouteCheckResult Check(RouteProfile profile,int width,int height,Func<int,int,bool> wall,Func<RoutePoint,PointF> toGrid,Func<int,int,PointF> toMemory,CancellationToken token)
    {
        var result=new RouteCheckResult();
        try{profile.Validate(true);}catch(Exception ex){result.Issues.Add(new RouteCheckIssue{PointIndex=-1,Error=true,Message=ex.Message});}
        token.ThrowIfCancellationRequested();
        var nav=new DungeonNavigator(width,height,(x,y)=>{token.ThrowIfCancellationRequested();var p=toMemory(x,y);return (!profile.ObstaclePass&&wall(x,y))||profile.Forbidden(p.X,p.Y,171);});
        if(profile.ObstaclePass)result.Issues.Add(new RouteCheckIssue{PointIndex=-1,Message="Engel geçişi seçili: harita duvarları yok sayıldı; yasak alanlar korunuyor. İstemcinin geçişi kabul etmesi bu ön kontrolden anlaşılmaz."});
        else if(profile.MountedObstaclePass)result.Issues.Add(new RouteCheckIssue{PointIndex=-1,Message="Binek geçişi yalnız canlı binek durumu okununca uygulanır; bu ön kontrol normal yürüyüş engellerini korur."});
        var coordinates=new List<PointF>();
        for(int i=0;i<profile.Points.Count;i++)
        {
            token.ThrowIfCancellationRequested();var p=profile.Points[i];var g=toGrid(p);coordinates.Add(g);
            if(g.X<0||g.Y<0||g.X>=width||g.Y>=height||!nav.Walkable((int)g.X,(int)g.Y))result.Issues.Add(new RouteCheckIssue{PointIndex=i,Error=true,Message="Harita dışında, duvarda veya yasak alan güvenlik payında."});
        }
        var cache=new Dictionary<string,bool>();
        for(int i=1;i<coordinates.Count;i++)
        {
            token.ThrowIfCancellationRequested();var a=coordinates[i-1];var b=coordinates[i];
            if(!nav.InBounds((int)a.X,(int)a.Y)||!nav.InBounds((int)b.X,(int)b.Y))continue;
            string key=(int)a.X+","+(int)a.Y+":"+(int)b.X+","+(int)b.Y;
            bool reachable;if(!cache.TryGetValue(key,out reachable)){reachable=nav.FindPath((int)a.X,(int)a.Y,(int)b.X,(int)b.Y)!=null;cache[key]=reachable;}
            result.Segments++;
            if(!reachable)result.Issues.Add(new RouteCheckIssue{PointIndex=i,Error=true,Message="Önceki noktadan yürünebilir yol yok."});
            else if(profile.SegmentForbidden(profile.Points[i-1],profile.Points[i],100)||!nav.LineClear((int)a.Y*width+(int)a.X,(int)b.Y*width+(int)b.X))result.Issues.Add(new RouteCheckIssue{PointIndex=i,Message="Doğrudan çizgi engelli; rota motoru dolanma yolu kullanacak."});
        }
        if(profile.Retreat&&profile.SafePoint!=null)
        {
            var safe=toGrid(profile.SafePoint);
            if(!nav.InBounds((int)safe.X,(int)safe.Y)||!nav.Walkable((int)safe.X,(int)safe.Y))result.Issues.Add(new RouteCheckIssue{PointIndex=-1,Error=true,Message="Güvenli nokta yürünebilir değil."});
            else for(int i=0;i<coordinates.Count;i++){token.ThrowIfCancellationRequested();var p=coordinates[i];if(nav.FindPath((int)p.X,(int)p.Y,(int)safe.X,(int)safe.Y)==null){result.Issues.Add(new RouteCheckIssue{PointIndex=i,Error=true,Message="Bu noktadan can korumasının güvenli noktasına yol yok."});break;}}
        }
        return result;
    }
}
