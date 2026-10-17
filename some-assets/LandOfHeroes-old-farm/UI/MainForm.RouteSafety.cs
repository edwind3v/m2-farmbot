using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    private RouteHealthGuard routeHealthGuard;
    private RoutePoint routeHealthReturn;
    private long routeUnknownHealthAt=-1;
    private DungeonNavigator routeNavigator;
    private readonly List<RouteZone> routeTemporaryBlocks=new List<RouteZone>();
    private int routeRecoveryAttempts;
    private PointF routeLastSteer;
    private RoutePoint routeRecoveryOrigin;
    private bool routeNamesBusy;
    private long routeNamesAt;
    private long routeMetadataWaitAt=-1;
    private int routeRunGeneration;
    private readonly Dictionary<int,RouteActorInfo> routeActorInfo=new Dictionary<int,RouteActorInfo>();
    private sealed class RouteActorInfo { public string Name;public int Vnum;public long At; }
    private bool RouteStones { get { return (activeRouteOptions?.StoneMonsterSplash??false)||(activeRouteOptions?.Mixed??false)||RotaMetinCheck.Checked; } }
    private bool RouteMonsters { get { return !(activeRouteOptions?.StoneMonsterSplash??false)&&((activeRouteOptions?.Mixed??false)||RotaCanavarCheck.Checked); } }
    private int RouteAttackInterval { get { return activeRouteOptions?.AttackMilliseconds??180; } }
    private void PrepareRouteSafety()
    {
        var p=CaptureRouteProfile();if(string.IsNullOrEmpty(p.Map))p.Map=atlasName;p.Validate(true);
        if((p.Zones.Count>0||p.Retreat)&&(atlas==null||!mapCalibrated))throw new InvalidOperationException("Yasak alan ve can koruması için haritanın yürünebilirlik verisi ve kalibrasyonu gerekli.");
        if(p.Forbidden(TaskX,TaskY,100))throw new InvalidOperationException("Karakter yasak alan içinde/kenarında; önce elle güvenli alana çık.");
        activeRouteOptions=p;routeRunGeneration++;routeActorInfo.Clear();routeNamesAt=0;routeMetadataWaitAt=-1;
        routeIgnoreTerrain=p.ObstaclePass;routeTraversalAt=0;
        routeHealthGuard=new RouteHealthGuard();routeHealthReturn=null;routeUnknownHealthAt=-1;routeNavigator=null;routeTemporaryBlocks.Clear();routeRecoveryAttempts=0;routeRecoveryOrigin=null;
        EndRouteEdit();if(routeToolsForm!=null&&!routeToolsForm.IsDisposed)routeToolsForm.Close();
        routeLastSteer=new PointF(TaskX,TaskY);
    }
    private bool HandleRouteHealth(float x,float y,long now)
    {
        if(!activeRouteOptions.Retreat)return false;
        float? hp=null;try{hp=PlayerHealthSnapshot.Parse(File.ReadAllText(TargetHealthPath),routeOwnerPid,routeRunningMap,DateTime.UtcNow);}catch(IOException){}
        var safe=activeRouteOptions.SafePoint;bool atSafe=RoutePatrol.Distance(x,y,safe.X,safe.Y)<=100;
        var action=routeHealthGuard.Evaluate(hp,atSafe,activeRouteOptions.LowHealth,activeRouteOptions.ResumeHealth);
        if(action==RouteHealthAction.Hold)
        {
            SetRouteAttack(false);ReleaseMovement();routeLastProgress=now;routeLastPosition=new PointF(x,y);
            if(routeUnknownHealthAt<0)routeUnknownHealthAt=now;
            if(now-routeUnknownHealthAt>=10000){PauseRoutePatrol("Taze oyuncu canı bekleniyor; veri gelince otomatik devam eder.",0);return true;}
            routePatrolStatus.Text="Can koruması: taze oyuncu canı bekleniyor; hareket ve saldırı durdu.";return true;
        }
        routeUnknownHealthAt=-1;
        if(action==RouteHealthAction.Retreat||action==RouteHealthAction.Recover)
        {
            SetRouteAttack(false);
            if(routeHealthReturn==null){routeHealthReturn=new RoutePoint{X=x,Y=y};routeLastProgress=now;routeLastPosition=new PointF(x,y);routeNavigationPath=null;RecordActivity("Can koruması","Can %"+hp.Value.ToString("0")+"; mevcut rota/hedef duraklatıldı, güvenli noktaya çekiliyor.");}
            if(action==RouteHealthAction.Recover){ReleaseMovement();routeLastProgress=now;routeLastPosition=new PointF(x,y);routePatrolStatus.Text="Güvenli noktada can toparlanması bekleniyor · devam eşiği %"+activeRouteOptions.ResumeHealth;}
            else{RouteMoveTo(x,y,safe.X,safe.Y,now);routePatrolStatus.Text="Düşük can: güvenli noktaya çekiliyor; saldırı kapalı.";}
            return true;
        }
        if(routeHealthReturn!=null)
        {
            SetRouteAttack(false);
            if(RoutePatrol.Distance(x,y,routeHealthReturn.X,routeHealthReturn.Y)>80){RouteMoveTo(x,y,routeHealthReturn.X,routeHealthReturn.Y,now);routePatrolStatus.Text="Can toparlandı; bırakılan konuma dönülüyor.";return true;}
            routeHealthReturn=null;routeNavigationPath=null;routeLastProgress=now;routeLastPosition=new PointF(x,y);RecordActivity("Can koruması","Bırakılan konuma dönüldü; aynı rota/hedef devam ediyor.");
        }
        return false;
    }
    private void RecoverRoutePath(float x,float y,long now)
    {
        ReleaseMovement();SetRouteAttack(false);
        if(!activeRouteOptions.Recovery||atlas==null||!mapCalibrated)throw new InvalidOperationException("Yolda ilerleme yok; alternatif yol için harita verisi veya kurtarma seçeneği yok.");
        if(routeRecoveryAttempts>=3)throw new InvalidOperationException("Üç alternatif yol denemesinde ilerlenemedi; canlı hedef atlanmadan durdu.");
        double distance=RoutePatrol.Distance(x,y,routeLastSteer.X,routeLastSteer.Y);
        if(distance<50)throw new InvalidOperationException("İlerleme yok ve güvenli alternatif yön belirlenemedi.");
        double ahead=Math.Min(distance,250);
        routeTemporaryBlocks.Add(new RouteZone{X=x+(float)((routeLastSteer.X-x)*ahead/distance),Y=y+(float)((routeLastSteer.Y-y)*ahead/distance),Radius=80});
        routeRecoveryAttempts++;if(routeRecoveryOrigin==null)routeRecoveryOrigin=new RoutePoint{X=x,Y=y};
        routeNavigator=null;routeNavigationPath=null;routeLastProgress=now;routeLastPosition=new PointF(x,y);
        RecordActivity("Rota kurtarma","Takılma yönü geçici engel yapıldı; alternatif yol "+routeRecoveryAttempts+"/3 deneniyor.");
    }
    private bool RouteCellBlocked(int x,int y)
    {
        var p=MapPixelToMemory(new PointF(x+.5f,y+.5f));return (!routeIgnoreTerrain&&atlas.IsBlocked(x,y))||activeRouteOptions.Forbidden(p.X,p.Y,171)||routeTemporaryBlocks.Any(z=>z.Contains(p.X,p.Y,71));
    }
    private void ResetRouteRecoveryAfterProgress(float x,float y)
    {
        if(routeRecoveryOrigin==null||RoutePatrol.Distance(x,y,routeRecoveryOrigin.X,routeRecoveryOrigin.Y)<800)return;
        routeRecoveryOrigin=null;routeRecoveryAttempts=0;routeTemporaryBlocks.Clear();routeNavigator=null;routeNavigationPath=null;RecordActivity("Rota kurtarma","Engel bölgesinden çıkıldı; normal rota devam ediyor.");
    }
    private bool RouteSegmentSafe(float x,float y,float tx,float ty)
    {
        return !activeRouteOptions.SegmentForbidden(new RoutePoint{X=x,Y=y},new RoutePoint{X=tx,Y=ty},100);
    }
    private bool RouteWalkSegmentSafe(float x,float y,float tx,float ty)
    {
        if(!RouteSegmentSafe(x,y,tx,ty))return false;
        if(routeNavigator==null)return true;
        var a=ToMapPixel(x,y);var b=ToMapPixel(tx,ty);
        int samples=(int)Math.Ceiling(RoutePatrol.Distance(a.X,a.Y,b.X,b.Y)*4);
        for(int i=1;i<=samples;i++){float t=(float)i/samples;int cx=(int)Math.Floor(a.X+(b.X-a.X)*t),cy=(int)Math.Floor(a.Y+(b.Y-a.Y)*t);if(!routeNavigator.Walkable(cx,cy))return false;}
        return true;
    }
    private bool RouteBoss(EntityState e)
    {
        RouteActorInfo info;PackMobRecord mob;
        if(routeActorInfo.TryGetValue(e.Property_03C9,out info)&&movementClock.ElapsedMilliseconds-info.At<=3000&&packCatalog!=null&&packCatalog.MobByVnum.TryGetValue(info.Vnum,out mob))return mob.Kind==PackTargetKind.Boss;
        return RouteProfile.Matches(e.Property_03CD,activeRouteOptions.BossNames);
    }
    private IEnumerable<EntityState> RouteCandidates(List<EntityState> live,float x,float y,long now)
    {
        // Simple route: live radar type + position, never a pack-name lookup or metadata wait.
        var eligible=live.Where(t=>!activeRouteOptions.Forbidden(t.X,t.Y,100));
        // Refresh the locked actor position too; NearCurrentRoute deliberately returns false during a detour.
        return eligible.Where(t=>combatFocus.Target?.Property_03C9==t.Property_03C9||routePatrol.NearRouteCorridor(t.X,t.Y,x,y,routeFarmRange))
            .OrderBy(t=>t.Property_03CA==AddressCatalog.Field_035B?0:1).ThenBy(t=>RoutePatrol.Distance(x,y,t.X,t.Y));
    }
    private string DescribeRouteRadar(List<EntityState> radar,List<EntityState> candidates,float x,float y)
    {
        string counts="Rota koridoru: yarıçap "+(int)routeFarmRange+" · toplam genişlik "+(int)(routeFarmRange*2)+"\nRadar: "+radar.Count(t=>t.Property_03CA==AddressCatalog.Field_035B)+" taş · "+radar.Count(t=>t.Property_03CA==AddressCatalog.Field_035C)+" canavar\n";
        var nearest=radar.Where(t=>RouteAttackPolicy.CanPursue(t.Property_03CA,RouteStones,RouteMonsters,activeRouteOptions.StoneMonsterSplash))
            .OrderBy(t=>RoutePatrol.Distance(x,y,t.X,t.Y)).FirstOrDefault();
        if(nearest==null)return counts+"Seçtiğin türde canlı radar hedefi yok.";
        int distance=(int)Math.Ceiling(RoutePatrol.Distance(x,y,nearest.X,nearest.Y));
        int line=(int)Math.Ceiling(routePatrol.DistanceToLine(nearest.X,nearest.Y));
        string detail="En yakın VID "+nearest.Property_03C9+": sana "+distance+", çizgiye "+line+" birim. ";
        if(activeRouteOptions.Forbidden(nearest.X,nearest.Y,100))return counts+detail+"İşaretlediğin yasak alan içinde.";
        if(routePatrol.DistanceToLine(nearest.X,nearest.Y)>routeFarmRange)return counts+detail+"Rota koridorunun dışında; yarıçapı artır.";
        return counts+detail+"Rota çizgisi koridorunda · Seçilebilir hedef: "+candidates.Count+".";
    }
    private bool HoldForRouteMetadata(List<EntityState> live,float x,float y,long now)
    {
        bool required=activeRouteOptions.Priority==1||!string.IsNullOrWhiteSpace(activeRouteOptions.Names);
        bool pending=required&&combatFocus.Target==null&&live.Any(t=>!activeRouteOptions.Forbidden(t.X,t.Y,100)&&routePatrol.NearRouteCorridor(t.X,t.Y,x,y,routeFarmRange)&&(!routeActorInfo.ContainsKey(t.Property_03C9)||now-routeActorInfo[t.Property_03C9].At>3000));
        if(!pending){routeMetadataWaitAt=-1;return false;}
        if(routeMetadataWaitAt<0)routeMetadataWaitAt=now;
        SetRouteAttack(false);ReleaseMovement();routeLastProgress=now;routeLastPosition=new PointF(x,y);
        if(now-routeMetadataWaitAt>10000)PauseRoutePatrol("Hedef adı/sınıfı bekleniyor; doğrulanınca otomatik devam eder.",0);
        else routePatrolStatus.Text="Yakındaki hedeflerin adı/sınıfı bekleniyor; hedef atlamamak için rota duraklatıldı.";
        return true;
    }
    private async void RequestRouteNames(List<EntityState> live,long now)
    {
        if(routeNamesBusy||now<routeNamesAt)return;
        var ids=live.Where(t=>RoutePatrol.Distance(TaskX,TaskY,t.X,t.Y)<=routeFarmRange*2).OrderBy(t=>routeActorInfo.ContainsKey(t.Property_03C9)?routeActorInfo[t.Property_03C9].At:long.MinValue).Take(128).Select(t=>t.Property_03C9).ToArray();if(ids.Length==0)return;
        routeNamesBusy=true;routeNamesAt=now+1000;int generation=routeRunGeneration,pid=routeOwnerPid;string map=routeRunningMap;
        try
        {
            string script="import chr,background,json\nrows=[]\nfor v in ["+string.Join(",",ids)+"]:\n try:\n  if chr.HasInstance(v): rows.append(dict(Vid=v,Name=str(chr.GetNameByVID(v)),Vnum=int(chr.GetVirtualNumber(v))))\n except Exception: pass\nf.write(json.dumps(dict(Map=background.GetCurrentMapName(),Rows=rows),ensure_ascii=True))";
            string result=await GamePythonAsync(script,CancellationToken.None);
            if(IsDisposed||!RouteOwnsCombat||generation!=routeRunGeneration||routeOwnerPid!=pid||routeRunningMap!=map)return;
            var report=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<RouteNamesReport>(result);if(report==null||report.Map!=map||report.Rows==null)return;
            foreach(var row in report.Rows.Where(r=>ids.Contains(r.Vid)))routeActorInfo[row.Vid]=new RouteActorInfo{Name=row.Name,Vnum=row.Vnum,At=movementClock.ElapsedMilliseconds};
            foreach(int id in routeActorInfo.Where(p=>movementClock.ElapsedMilliseconds-p.Value.At>5000).Select(p=>p.Key).ToList())routeActorInfo.Remove(id);
        }
        catch(Exception ex){if(!IsDisposed&&RouteOwnsCombat&&generation==routeRunGeneration)RecordActivity("Rota hedef adı","Okunamadı; ad/öncelik filtresi için doğrulanmamış hedef seçilmeyecek: "+ex.Message);}
        finally{routeNamesBusy=false;}
    }
    public sealed class RouteNamesReport { public string Map {get;set;} public List<RouteNameRow> Rows {get;set;} }
    public sealed class RouteNameRow { public int Vid {get;set;} public string Name {get;set;} public int Vnum {get;set;} }
}
