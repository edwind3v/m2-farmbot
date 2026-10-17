using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

public enum PackTargetKind { Other, Npc, Monster, Stone, Boss }

public sealed class PackMobRecord
{
    public int Vnum { get; set; }
    public string Name { get; set; }
    public string LocaleName { get; set; }
    public int Rank { get; set; }
    public int Type { get; set; }
    public int Level { get; set; }
    public long MaxHp { get; set; }
    public int AttackSpeed { get; set; }
    public int MoveSpeed { get; set; }
    public int AttackRange { get; set; }
    public PackTargetKind Kind { get; set; }
    public override string ToString() { return (string.IsNullOrWhiteSpace(LocaleName)?Name:LocaleName)+" · "+Vnum+" · "+PackCombatPlanner.KindText(Kind); }
}

public sealed class PackItemRecord
{
    public uint Vnum { get; set; }
    public string Name { get; set; }
    public string LocaleName { get; set; }
    public int Type { get; set; }
    public int Subtype { get; set; }
    public override string ToString() { return (string.IsNullOrWhiteSpace(LocaleName)?Name:LocaleName)+" · "+Vnum; }
}

public sealed class PackAtlasRecord
{
    public string Directory { get; set; }
    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool HasNavigationData { get; set; }
    public string NavigationPath { get; set; }
    public override string ToString() { return (string.IsNullOrWhiteSpace(Name)?Directory:Name)+" · "+Directory; }
}

public sealed class PackMotionRecord
{
    public string RelativePath { get; set; }
    public string Name { get; set; }
    public double DurationSeconds { get; set; }
    public override string ToString() { return Name+" · "+DurationSeconds.ToString("0.###",CultureInfo.CurrentCulture)+" sn"; }
}

public sealed class PackSpawnRecord
{
    public string MapDirectory { get; set; }
    public string Source { get; set; }
    public int Vnum { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public PackMobRecord Mob { get; set; }
    public string DisplayName { get { return Mob==null?("VNUM "+Vnum):(string.IsNullOrWhiteSpace(Mob.LocaleName)?Mob.Name:Mob.LocaleName); } }
    public bool IsNpc { get { return Mob!=null&&Mob.Kind==PackTargetKind.Npc; } }
}

public sealed class PackDataCatalog
{
    private static readonly Regex MotionDurationRegex=new Regex("[\\\"']?MotionDuration[\\\"']?\\s*[:=]?\\s*([0-9]+(?:[\\.,][0-9]+)?)",RegexOptions.IgnoreCase|RegexOptions.Compiled);
    public string Root { get; private set; }
    public string LocaleFolder { get; private set; }
    public List<PackMobRecord> Mobs { get; private set; }=new List<PackMobRecord>();
    public List<PackItemRecord> Items { get; private set; }=new List<PackItemRecord>();
    public List<PackAtlasRecord> Maps { get; private set; }=new List<PackAtlasRecord>();
    public List<PackMotionRecord> Motions { get; private set; }=new List<PackMotionRecord>();
    public List<PackSpawnRecord> Spawns { get; private set; }=new List<PackSpawnRecord>();
    public Dictionary<int,PackMobRecord> MobByVnum { get; private set; }=new Dictionary<int,PackMobRecord>();
    public Dictionary<uint,PackItemRecord> ItemByVnum { get; private set; }=new Dictionary<uint,PackItemRecord>();

    public static PackDataCatalog Load(string root)
    {
        if(string.IsNullOrWhiteSpace(root))throw new ArgumentException("Pack klasörü boş olamaz.");
        root=Path.GetFullPath(root.Trim());
        if(!Directory.Exists(root))throw new DirectoryNotFoundException("Pack klasörü bulunamadı: "+root);
        var catalog=new PackDataCatalog{Root=root};
        catalog.LocaleFolder=FindLocaleFolder(root);
        catalog.Mobs=ReadArray(Path.Combine(catalog.LocaleFolder,"mob_proto.json"),ReadMob);
        catalog.Items=ReadArray(Path.Combine(catalog.LocaleFolder,"item_proto.json"),ReadItem);
        catalog.Maps=ReadArray(Path.Combine(catalog.LocaleFolder,"atlasinfo.json"),ReadMap);
        catalog.MobByVnum=catalog.Mobs.GroupBy(x=>x.Vnum).ToDictionary(g=>g.Key,g=>g.First());
        catalog.ItemByVnum=catalog.Items.GroupBy(x=>x.Vnum).ToDictionary(g=>g.Key,g=>g.First());
        foreach(var map in catalog.Maps)catalog.ResolveNavigation(map);
        catalog.Motions=catalog.ReadMotions();
        catalog.Spawns=catalog.ReadSpawns();
        return catalog;
    }

    public IEnumerable<PackSpawnRecord> FindNpcSpawns(string mapDirectory,int vnum,string name)
    {
        string map=NormalizeMapName(mapDirectory),query=(name??"").Trim();
        return Spawns.Where(s=>s.IsNpc&&MapMatches(s.MapDirectory,map))
            .Where(s=>vnum>0?s.Vnum==vnum:query.Length==0||Contains(s.DisplayName,query)||(s.Mob!=null&&Contains(s.Mob.Name,query)))
            .OrderBy(s=>s.Vnum).ThenBy(s=>s.X).ThenBy(s=>s.Y);
    }

    public IEnumerable<PackMobRecord> SearchMobs(string query,bool stones,bool monsters,bool bosses,bool npcs,int take=300)
    {
        query=(query??"").Trim();
        return Mobs.Where(m=>(stones&&m.Kind==PackTargetKind.Stone)||(monsters&&m.Kind==PackTargetKind.Monster)||(bosses&&m.Kind==PackTargetKind.Boss)||(npcs&&m.Kind==PackTargetKind.Npc))
            .Where(m=>query.Length==0||m.Vnum.ToString(CultureInfo.InvariantCulture)==query||Contains(m.Name,query)||Contains(m.LocaleName,query))
            .OrderBy(m=>m.Kind).ThenBy(m=>m.Level).ThenBy(m=>m.Vnum).Take(Math.Max(1,take));
    }

    public IEnumerable<PackItemRecord> SearchItems(string query,int take=300)
    {
        query=(query??"").Trim();
        return Items.Where(i=>query.Length==0||i.Vnum.ToString(CultureInfo.InvariantCulture)==query||Contains(i.Name,query)||Contains(i.LocaleName,query))
            .OrderBy(i=>i.Vnum).Take(Math.Max(1,take));
    }

    private static bool Contains(string value,string query){return (value??"").IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0;}
    private static string NormalizeMapName(string value){return (value??"").Replace('\\','/').Trim('/').ToLowerInvariant();}
    private static bool MapMatches(string left,string right)
    {
        left=NormalizeMapName(left);right=NormalizeMapName(right);
        if(left.Length==0||right.Length==0)return false;
        return left==right||left.EndsWith("/"+right,StringComparison.Ordinal)||right.EndsWith("/"+left,StringComparison.Ordinal);
    }

    private List<PackSpawnRecord> ReadSpawns()
    {
        var result=new List<PackSpawnRecord>();
        var names=new HashSet<string>(new[]{"npc.txt","regen.txt","boss.txt","stone.txt"},StringComparer.OrdinalIgnoreCase);
        IEnumerable<string> files;
        try{files=Directory.EnumerateFiles(Root,"*.txt",SearchOption.AllDirectories).Where(path=>names.Contains(Path.GetFileName(path))).ToArray();}
        catch{return result;}
        foreach(string path in files)
        {
            string map=Path.GetFileName(Path.GetDirectoryName(path));
            foreach(string raw in SafeLines(path))
            {
                string line=(raw??"").Trim();if(line.Length==0||line.StartsWith("//"))continue;
                var p=Regex.Split(line,"\\s+");if(p.Length<11)continue;
                // Group rows end in a group id, not a mob VNUM. Only direct rows are exact enough for navigation.
                string type=p[0].ToLowerInvariant();if(type!="m"&&type!="r"&&type!="s")continue;
                int x,y,vnum;if(!int.TryParse(p[1],NumberStyles.Integer,CultureInfo.InvariantCulture,out x)||!int.TryParse(p[2],NumberStyles.Integer,CultureInfo.InvariantCulture,out y)||!int.TryParse(p[10],NumberStyles.Integer,CultureInfo.InvariantCulture,out vnum)||vnum<=0)continue;
                PackMobRecord mob;MobByVnum.TryGetValue(vnum,out mob);
                result.Add(new PackSpawnRecord{MapDirectory=map,Source=Path.GetFileName(path),Vnum=vnum,X=x*MinimapAtlas.UnitsPerPixel,Y=y*MinimapAtlas.UnitsPerPixel,Mob=mob});
            }
        }
        return result.GroupBy(s=>NormalizeMapName(s.MapDirectory)+"|"+s.Vnum+"|"+s.X.ToString("0",CultureInfo.InvariantCulture)+"|"+s.Y.ToString("0",CultureInfo.InvariantCulture)).Select(g=>g.First()).ToList();
    }
    private static IEnumerable<string> SafeLines(string path){try{return File.ReadLines(path).ToArray();}catch{return new string[0];}}

    private static string FindLocaleFolder(string root)
    {
        var direct=new[]{Path.Combine(root,"locale","locale","tr"),Path.Combine(root,"locale","tr"),Path.Combine(root,"tr")};
        foreach(var folder in direct)if(File.Exists(Path.Combine(folder,"mob_proto.json")))return folder;
        string found=Directory.EnumerateFiles(root,"mob_proto.json",SearchOption.AllDirectories).FirstOrDefault();
        if(found==null)throw new FileNotFoundException("mob_proto.json bulunamadı. Çıkarılan packların ana klasörünü seçin.");
        return Path.GetDirectoryName(found);
    }

    private static List<T> ReadArray<T>(string path,Func<IDictionary<string,object>,T> converter)
    {
        if(!File.Exists(path))throw new FileNotFoundException(Path.GetFileName(path)+" bulunamadı.",path);
        var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=128};
        var raw=serializer.DeserializeObject(File.ReadAllText(path)) as object[];
        if(raw==null)throw new InvalidDataException(Path.GetFileName(path)+" geçerli bir JSON dizisi değil.");
        var result=new List<T>(raw.Length);
        foreach(var entry in raw){var dict=entry as IDictionary<string,object>;if(dict!=null)result.Add(converter(dict));}
        return result;
    }

    private static PackMobRecord ReadMob(IDictionary<string,object> d)
    {
        var m=new PackMobRecord{Vnum=Int(d,"vnum"),Name=Text(d,"name"),LocaleName=Text(d,"locale_name"),Rank=Int(d,"rank"),Type=Int(d,"type"),Level=Int(d,"level"),MaxHp=Long(d,"max_hp"),AttackSpeed=Int(d,"attack_speed"),MoveSpeed=Int(d,"move_speed"),AttackRange=Int(d,"attack_range")};
        m.Kind=PackCombatPlanner.Classify(m.Type,m.Rank);return m;
    }
    private static PackItemRecord ReadItem(IDictionary<string,object> d){return new PackItemRecord{Vnum=(uint)Math.Max(0,Long(d,"vnum")),Name=Text(d,"name"),LocaleName=Text(d,"locale_name"),Type=Int(d,"type"),Subtype=Int(d,"subtype")};}
    private static PackAtlasRecord ReadMap(IDictionary<string,object> d){return new PackAtlasRecord{Directory=Text(d,"directory"),Name=Text(d,"name"),X=Int(d,"x"),Y=Int(d,"y"),Width=Int(d,"width"),Height=Int(d,"height")};}
    private static string Text(IDictionary<string,object> d,string key){object value;return d.TryGetValue(key,out value)&&value!=null?Convert.ToString(value,CultureInfo.InvariantCulture):"";}
    private static int Int(IDictionary<string,object> d,string key){long value=Long(d,key);return value>int.MaxValue?int.MaxValue:value<int.MinValue?int.MinValue:(int)value;}
    private static long Long(IDictionary<string,object> d,string key){object value;if(!d.TryGetValue(key,out value)||value==null)return 0;long parsed;if(long.TryParse(Convert.ToString(value,CultureInfo.InvariantCulture),NumberStyles.Any,CultureInfo.InvariantCulture,out parsed))return parsed;try{return Convert.ToInt64(value,CultureInfo.InvariantCulture);}catch{return 0;}}

    private void ResolveNavigation(PackAtlasRecord map)
    {
        string relative=(map.Directory??"").Replace('/',Path.DirectorySeparatorChar);
        var candidates=new[]{Path.Combine(Root,relative),Path.Combine(Root,"map",relative),Path.Combine(Root,"maps",relative),Path.Combine(Root,"property",relative)};
        foreach(var path in candidates)
        {
            if(!Directory.Exists(path))continue;
            bool has=File.Exists(Path.Combine(path,"MapProperty.txt"))||Directory.EnumerateFiles(path,"attr.atr",SearchOption.AllDirectories).Any()||Directory.EnumerateFiles(path,"*.atr",SearchOption.AllDirectories).Any();
            if(has){map.HasNavigationData=true;map.NavigationPath=path;return;}
        }
        map.NavigationPath=candidates[0];
    }

    private List<PackMotionRecord> ReadMotions()
    {
        var result=new List<PackMotionRecord>();
        foreach(var baseName in new[]{"pc","pc2","pc3"})
        {
            string folder=Path.Combine(Root,baseName);if(!Directory.Exists(folder))continue;
            foreach(var path in Directory.EnumerateFiles(folder,"*.msa",SearchOption.AllDirectories))
            {
                string file=Path.GetFileNameWithoutExtension(path);string lower=file.ToLowerInvariant();
                if(!(lower.Contains("attack")||lower.Contains("combo")||lower.Contains("skill")))continue;
                try
                {
                    string text=File.ReadAllText(path);var match=MotionDurationRegex.Match(text);double duration;
                    if(!match.Success||!double.TryParse(match.Groups[1].Value.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out duration)||duration<=0||duration>30)continue;
                    string relative=path.Substring(Root.Length).TrimStart(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
                    result.Add(new PackMotionRecord{RelativePath=relative,Name=file,DurationSeconds=duration});
                }
                catch{}
            }
        }
        return result.OrderBy(m=>m.DurationSeconds).ThenBy(m=>m.RelativePath).ToList();
    }
}

public static class PackCombatPlanner
{
    public static PackTargetKind Classify(int type,int rank)
    {
        if(type==2)return PackTargetKind.Stone;
        if(type==1)return PackTargetKind.Npc;
        if(type==0&&rank>=4)return PackTargetKind.Boss;
        if(type==0)return PackTargetKind.Monster;
        return PackTargetKind.Other;
    }
    public static string KindText(PackTargetKind kind)
    {
        switch(kind){case PackTargetKind.Npc:return "NPC";case PackTargetKind.Monster:return "Canavar";case PackTargetKind.Stone:return "Taş";case PackTargetKind.Boss:return "Patron";default:return "Diğer";}
    }
    public static int SafeAttackIntervalMilliseconds(int attackSpeed,double motionDurationSeconds)
    {
        attackSpeed=Math.Max(1,Math.Min(1000,attackSpeed));
        if(double.IsNaN(motionDurationSeconds)||double.IsInfinity(motionDurationSeconds)||motionDurationSeconds<=0)motionDurationSeconds=0.5;
        double raw=motionDurationSeconds*1000.0*100.0/attackSpeed;
        return Math.Max(40,Math.Min(1000,(int)Math.Ceiling(raw*1.08)));
    }
    public static int[] BuildVnumFilter(IEnumerable<PackMobRecord> records,int maximum=1024)
    {
        return (records??Enumerable.Empty<PackMobRecord>()).Where(r=>r!=null&&(r.Kind==PackTargetKind.Monster||r.Kind==PackTargetKind.Stone||r.Kind==PackTargetKind.Boss))
            .OrderBy(r=>r.Kind==PackTargetKind.Stone?0:r.Kind==PackTargetKind.Boss?1:2)
            .Select(r=>r.Vnum).Where(v=>v>0).Distinct().Take(Math.Max(1,maximum)).ToArray();
    }
}
