using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public sealed class DungeonLiveEntity {
    public int Vid {get;set;} public int Vnum {get;set;} public int MotorType {get;set;}
    public string Name {get;set;} public string PackName {get;set;}
    public float Dx {get;set;} public float Dy {get;set;} public float X {get;set;} public float Y {get;set;} public float Distance {get;set;}
    public bool? Dead {get;set;}
    public string TypeText {get{return MotorType==1?"NPC":MotorType==AddressCatalog.Field_035B?"Taş":MotorType==AddressCatalog.Field_035C?"Canavar":"Diğer · tür "+MotorType;}}
    public string Hint {get{
        string n=((Name??"")+" "+(PackName??"")).ToLowerInvariant();
        return n.Contains("kapı")||n.Contains("kapi")||n.Contains("door")||n.Contains("gate")?"Kapı olabilir · ad çıkarımı":TypeText=="Taş"?"Kesilebilir / tetikleyici davranışı bilinmiyor":"Nesne davranışı henüz denenmedi";
    }}
    public override string ToString(){return Name+" · "+TypeText+" · VNUM "+Vnum+" · VID "+Vid;}
}
public static class DungeonInteractionEvidence {
    public static bool NewTarget(IEnumerable<int> before,IEnumerable<int> now){var previous=new HashSet<int>(before);return now.Any(v=>!previous.Contains(v));}
    public static bool ExpectedMap(string start,string current,string expected){return !string.IsNullOrWhiteSpace(current)&&!string.IsNullOrWhiteSpace(expected)&&current!=start&&current==expected;}
}
public sealed class DungeonLiveItem {public uint Id {get;set;} public long Count {get;set;} public int Slot {get;set;}}
public sealed class DungeonLiveSnapshot {
    public int ProcessId {get;set;} public int CharacterVid {get;set;} public string CharacterName {get;set;} public string Map {get;set;}
    public int SelectedVid {get;set;} public int HoverVid {get;set;} public DateTime CapturedUtc {get;set;}
    public bool ClickApi {get;set;} public bool GiveApi {get;set;} public bool Complete {get;set;} public bool InventoryComplete {get;set;}
    public string Dialog {get;set;} public List<DungeonLiveEntity> Entities {get;set;}=new List<DungeonLiveEntity>();
    public List<DungeonLiveItem> Items {get;set;}=new List<DungeonLiveItem>();
    public bool IsFresh(int pid,DateTime now){return pid==ProcessId&&CapturedUtc<=now&&now-CapturedUtc<=TimeSpan.FromSeconds(5);}
    public bool SameScene(DungeonLiveSnapshot other){return other!=null&&ProcessId==other.ProcessId&&CharacterVid==other.CharacterVid&&CharacterName==other.CharacterName&&Map==other.Map;}
    public DungeonCommand Capture(DungeonLiveEntity selected,DungeonCommandKind kind,int pid,DateTime now) {
        if(!IsFresh(pid,now))throw new InvalidOperationException("Canlı kayıt eski veya başka istemciye ait; yeniden tara.");
        var e=Entities.FirstOrDefault(x=>x.Vid==selected?.Vid&&x.Vnum==selected.Vnum&&x.MotorType==selected.MotorType&&x.Name==selected.Name);
        if(e==null)throw new InvalidOperationException("Seçilen nesne son canlı kayıtta yok.");
        if(string.IsNullOrWhiteSpace(Map)||e.MotorType<0||e.MotorType>255||float.IsNaN(e.X)||float.IsInfinity(e.X)||float.IsNaN(e.Y)||float.IsInfinity(e.Y)||float.IsNaN(e.Distance)||float.IsInfinity(e.Distance)||e.Distance<0)throw new InvalidOperationException("Nesnenin harita/tür/konum verisi geçersiz.");
        if(e.Vnum<=0&&string.IsNullOrWhiteSpace(e.Name))throw new InvalidOperationException("Nesnenin kalıcı kimliği okunamadı; VID tek başına kaydedilmez.");
        if(kind==DungeonCommandKind.ClearTargets&&e.MotorType!=AddressCatalog.Field_035B&&e.MotorType!=AddressCatalog.Field_035C)throw new InvalidOperationException("Bu motor türü kesme hedefi değil.");
        return new DungeonCommand{Kind=kind,Name=(e.Name??e.PackName)+" · "+DungeonCommand.Label(kind),TargetVnum=e.Vnum,TargetName=e.Vnum>0?"":e.Name,
            NativeTargetType=e.MotorType,TargetKind=e.TypeText=="Taş"?"Taş":e.TypeText=="Canavar"?"Canavar":e.MotorType==1?"NPC":"Tümü",
            ConstrainTargetArea=true,MapName=Map,X=e.X,Y=e.Y,TargetRadius=600};
    }
    public List<string> ChangesFrom(DungeonLiveSnapshot previous) {
        var result=new List<string>();
        if(!SameScene(previous)){result.Add("Yeni gözlem: "+CharacterName+" · "+Map+". Önceki harita ile ölüm/kaybolma kıyası yapılmadı.");return result;}
        var old=previous.Entities.ToDictionary(e=>e.Vid);var current=Entities.ToDictionary(e=>e.Vid);
        foreach(var e in Entities){DungeonLiveEntity before;if(!old.TryGetValue(e.Vid,out before)||before.Vnum!=e.Vnum||before.MotorType!=e.MotorType)result.Add("Yeni görülen: "+e);else if(e.Dead==true&&before.Dead!=true)result.Add("Ölüm durumu okundu: "+e);}
        if(Complete&&previous.Complete)foreach(var e in previous.Entities.Where(e=>!current.ContainsKey(e.Vid)))result.Add("Görünür listeden çıktı (ölüm kanıtı değil): "+e);
        if(InventoryComplete&&previous.InventoryComplete){var before=previous.Items.GroupBy(i=>i.Id).ToDictionary(g=>g.Key,g=>g.Sum(i=>i.Count));var after=Items.GroupBy(i=>i.Id).ToDictionary(g=>g.Key,g=>g.Sum(i=>i.Count));foreach(uint id in before.Keys.Union(after.Keys)){long a=before.ContainsKey(id)?before[id]:0,b=after.ContainsKey(id)?after[id]:0;if(a!=b)result.Add("Envanter ID "+id+": "+a+" → "+b);}}
        if(Dialog!=previous.Dialog)result.Add(Dialog=="none"?"NPC konuşması kapandı.":"NPC konuşma sayfası değişti; cevapları okuyabilirsin.");
        return result;
    }
    public static string BuildReadScript(IEnumerable<int> candidates,string dialogReader) {
        var candidateIds=candidates.Where(v=>v>0).Distinct().ToList();
        string vids=string.Join(",",candidateIds.Take(4096));
        string coverage=candidateIds.Count>0&&candidateIds.Count<=4096?"True":"False";
        return "import player,chr,background,net,json,math\n"+
            "for api in ('HasInstance','GetInstanceType','GetPixelPosition','GetNameByVID'):\n if not callable(getattr(chr,api,None)):raise RuntimeError('Canli hedef API bulunamadi')\n"+
            "here=player.GetMainCharacterPosition();me=player.GetMainCharacterIndex();mapname=background.GetCurrentMapName()\n"+
            "selected=0;hover=0\ntry:selected=int(player.GetTargetVID())\nexcept Exception:pass\ntry:hover=int(chr.PickAll())\nexcept Exception:pass\n"+
            "rows=[];skipped=0\nfor vid in sorted(set(["+vids+"]+[selected,hover])):\n if vid<=0 or vid==me:continue\n try:\n  if not chr.HasInstance(vid):continue\n  p=chr.GetPixelPosition(vid);dx=float(p[0]-here[0]);dy=float(p[1]-here[1]);distance=math.hypot(dx,dy)\n  if math.isnan(distance) or math.isinf(distance):raise ValueError('coordinate')\n  race=0;dead=None\n  try:race=int(chr.GetVirtualNumber(vid))\n  except Exception:pass\n  try:dead=bool(chr.IsDead(vid))\n  except Exception:pass\n  rows.append({'Vid':vid,'Vnum':race,'MotorType':int(chr.GetInstanceType(vid)),'Name':chr.GetNameByVID(vid) or '', 'Dx':dx,'Dy':dy,'Distance':distance,'Dead':dead})\n except Exception:skipped+=1\n"+
            "items=[];inventory_errors=0\nfor slot in range(min(1000,max(360,int(getattr(player,'INVENTORY_SLOT_COUNT',470))))):\n try:\n  itemid=int(player.GetItemIndex(slot))\n  if itemid:items.append({'Id':itemid,'Count':int(player.GetItemCount(slot)),'Slot':slot})\n except Exception:inventory_errors+=1\n"+
            dialogReader+
            "if background.GetCurrentMapName()!=mapname or player.GetMainCharacterIndex()!=me:raise RuntimeError('Gozlem sirasinda sahne degisti')\n"+
            "f.write(json.dumps({'CharacterVid':me,'CharacterName':chr.GetNameByVID(me) or '', 'Map':mapname,'SelectedVid':selected,'HoverVid':hover,'ClickApi':callable(getattr(net,'SendOnClickPacket',None)),'GiveApi':callable(getattr(net,'SendGiveItemPacket',None)),'Complete':skipped==0 and "+coverage+",'InventoryComplete':inventory_errors==0,'Entities':rows,'Items':items,'Dialog':dialog}))";
    }
}
