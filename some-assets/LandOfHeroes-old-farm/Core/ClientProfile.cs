using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// Everything the app needs from a particular game executable, found automatically so a client patch needs no new
// build. Values are RVAs / structure offsets read from the exe itself:
//  - singletons (Python launcher, player, item and skill managers): MSVC RTTI names never change, so the
//    CSingleton<T> vtable is located from its type name and the constructor that stores `this` into the
//    singleton global gives the global's address;
//  - Python enqueue/run functions: byte signatures with the build-specific displacements masked out;
//  - structure offsets: taken from the instructions that use them.
// Results are cached per executable SHA-256 under %LocalAppData%\Onur\profiles.
public sealed class ClientProfile
{
    public string Sha256, ExePath, Architecture;public long ExeSize;public DateTime FoundAt;
    public int ProfileVersion=2, PointerSize;public ulong PreferredImageBase;
    public bool Is64Bit{get{return PointerSize==8;}}
    public long PythonEnqueue, PythonRun, PythonLauncher, Player, ItemManager, SkillManager;
    public int SkillArray, ItemArray, ItemStride;
    public readonly List<string> Report=new List<string>();
    public bool PythonComplete{get{return PythonEnqueue>0&&PythonRun>0&&PythonLauncher>0;}}
    public bool InventoryComplete{get{return Player>0&&ItemManager>0&&SkillManager>0&&SkillArray>0&&ItemArray>0&&ItemStride>0;}}
    public bool Complete{get{return PythonComplete&&InventoryComplete;}}

    static readonly Dictionary<string,ClientProfile> cache=new Dictionary<string,ClientProfile>();
    public static string Folder{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","profiles");}}

    // Profile of the executable a process runs: cache -> saved file -> discovery (then saved).
    public static ClientProfile For(Process process,bool rediscover=false){return ForFile(process.MainModule.FileName,rediscover);}
    // Same for an executable on disk (a newly installed client can be prepared before it is started).
    public static ClientProfile ForFile(string path,bool rediscover=false)
    {
        string sha=Hash(path);
        lock(cache)
        {
            ClientProfile p;
            if(!rediscover&&cache.TryGetValue(sha,out p))return p;
            var file=Path.Combine(Folder,sha+".txt");
            if(!rediscover&&File.Exists(file)){p=Load(file);if(p!=null&&p.Complete){cache[sha]=p;return p;}}
            p=Discover(path,sha);
            Directory.CreateDirectory(Folder);p.Save(file);
            if(p.Complete)cache[sha]=p;
            return p;
        }
    }
    public static string Hash(string path){using(var s=File.OpenRead(path))using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(s)).Replace("-","").ToLowerInvariant();}

    void Save(string file)
    {
        var lines=new List<string>{"profile_version="+ProfileVersion,"sha="+Sha256,"exe="+ExePath,"size="+ExeSize,"found="+FoundAt.ToString("o"),
            "architecture="+(Architecture??"unknown"),"pointer_size="+PointerSize,"image_base="+PreferredImageBase.ToString("X"),
            "python_enqueue="+PythonEnqueue.ToString("X"),"python_run="+PythonRun.ToString("X"),"python_launcher="+PythonLauncher.ToString("X"),
            "player="+Player.ToString("X"),"item_manager="+ItemManager.ToString("X"),"skill_manager="+SkillManager.ToString("X"),
            "skill_array="+SkillArray.ToString("X"),"item_array="+ItemArray.ToString("X"),"item_stride="+ItemStride.ToString("X")};
        File.WriteAllLines(file,lines);
    }
    static ClientProfile Load(string file)
    {
        try
        {
            var d=File.ReadAllLines(file).Where(l=>l.Contains("=")).ToDictionary(l=>l.Substring(0,l.IndexOf('=')),l=>l.Substring(l.IndexOf('=')+1));
            Func<string,long> hex=k=>Convert.ToInt64(d[k],16);
            var binary=File.Exists(d["exe"])?ClientBinaryInfo.Read(d["exe"]):null;
            return new ClientProfile{ProfileVersion=d.ContainsKey("profile_version")?int.Parse(d["profile_version"]):1,
                Sha256=d["sha"],ExePath=d["exe"],ExeSize=long.Parse(d["size"]),FoundAt=DateTime.Parse(d["found"]),
                Architecture=d.ContainsKey("architecture")?d["architecture"]:(binary==null?"unknown":binary.Architecture),
                PointerSize=d.ContainsKey("pointer_size")?int.Parse(d["pointer_size"]):(binary==null?0:binary.PointerSize),
                PreferredImageBase=d.ContainsKey("image_base")?Convert.ToUInt64(d["image_base"],16):(binary==null?0:binary.ImageBase),
                PythonEnqueue=hex("python_enqueue"),PythonRun=hex("python_run"),PythonLauncher=hex("python_launcher"),Player=hex("player"),
                ItemManager=hex("item_manager"),SkillManager=hex("skill_manager"),SkillArray=(int)hex("skill_array"),ItemArray=(int)hex("item_array"),ItemStride=(int)hex("item_stride")};
        }
        catch{return null;}
    }

    // ---- discovery ----
    sealed class Image
    {
        public byte[] Data;public ulong Base;public int TextStart,TextEnd;public ClientBinaryInfo Info;public bool Is64{get{return Info.PointerSize==8;}}
        public static Image Map(string path)
        {
            var info=ClientBinaryInfo.Read(path);var b=File.ReadAllBytes(path);int pe=BitConverter.ToInt32(b,0x3c);int n=BitConverter.ToUInt16(b,pe+6),opt=BitConverter.ToUInt16(b,pe+20);
            var img=new Image{Base=info.ImageBase,Info=info};int sec=pe+24+opt;int size=0;
            for(int i=0;i<n;i++){int o=sec+i*40;size=Math.Max(size,BitConverter.ToInt32(b,o+12)+Math.Max(BitConverter.ToInt32(b,o+8),BitConverter.ToInt32(b,o+16)));}
            img.Data=new byte[size];
            for(int i=0;i<n;i++)
            {
                int o=sec+i*40;string name=Encoding.ASCII.GetString(b,o,8).TrimEnd('\0');int vs=BitConverter.ToInt32(b,o+8),va=BitConverter.ToInt32(b,o+12),rs=BitConverter.ToInt32(b,o+16),raw=BitConverter.ToInt32(b,o+20);
                int copy=Math.Min(Math.Min(vs==0?rs:vs,rs),Math.Max(0,b.Length-raw));
                if(raw>=0&&va>=0&&raw<b.Length&&va<img.Data.Length&&copy>0)Buffer.BlockCopy(b,raw,img.Data,va,Math.Min(copy,img.Data.Length-va));
                if(name==".text"){img.TextStart=va;img.TextEnd=va+vs;}
            }
            return img;
        }
        public int Rel(int at){return at+4+BitConverter.ToInt32(Data,at);}
    }
    static List<int> Find(byte[] data,int from,int to,string signature)
    {
        var parts=signature.Split(' ');var pat=new byte[parts.Length];var mask=new bool[parts.Length];
        for(int i=0;i<parts.Length;i++){mask[i]=parts[i]!="??";if(mask[i])pat[i]=Convert.ToByte(parts[i],16);}
        var hits=new List<int>();
        for(int i=from;i<=to-pat.Length;i++)
        {
            if(data[i]!=pat[0])continue;int k=1;while(k<pat.Length&&(!mask[k]||data[i+k]==pat[k]))k++;
            if(k==pat.Length)hits.Add(i);
        }
        return hits;
    }
    static int IndexOf(byte[] data,byte[] needle,int start)
    {
        for(int i=start;i<=data.Length-needle.Length;i++){int k=0;while(k<needle.Length&&data[i+k]==needle[k])k++;if(k==needle.Length)return i;}
        return -1;
    }
    // CSingleton<T>: type descriptor -> complete object locator -> vtable -> constructor "lea rax,[vtable]; mov [rcx],rax; ... mov [global],rcx".
    static long SingletonGlobal(Image img,string type)
    {
        var name=Encoding.ASCII.GetBytes(".?AV?$CSingleton@V"+type+"@@@@\0");int s=IndexOf(img.Data,name,0);if(s<0)return 0;int td=s-16;
        var tdBytes=BitConverter.GetBytes(td);
        for(int p=IndexOf(img.Data,tdBytes,0);p>=0;p=IndexOf(img.Data,tdBytes,p+1))
        {
            int col=p-12;if(col<0)continue;
            if(BitConverter.ToInt32(img.Data,col)!=1||BitConverter.ToInt32(img.Data,col+4)!=0||BitConverter.ToInt32(img.Data,col+20)!=col)continue;
            var meta=BitConverter.GetBytes(img.Base+(ulong)col);
            for(int m=IndexOf(img.Data,meta,0);m>=0;m=IndexOf(img.Data,meta,m+1))
            {
                int vtable=m+8;
                foreach(int at in Find(img.Data,img.TextStart,img.TextEnd,"48 8D 05 ?? ?? ?? ?? 48 89 01"))
                {
                    if(img.Rel(at+3)!=vtable)continue;
                    for(int k=at+10;k<at+40;k++)if(img.Data[k]==0x48&&img.Data[k+1]==0x89&&img.Data[k+2]==0x0D)return img.Rel(k+3);
                }
            }
        }
        return 0;
    }
    static long SingletonGlobal32(Image img,string type)
    {
        var name=Encoding.ASCII.GetBytes(".?AV?$CSingleton@V"+type+"@@@@\0");int s=IndexOf(img.Data,name,0);if(s<0)return 0;int td=s-8;
        var tdBytes=BitConverter.GetBytes(unchecked((uint)(img.Base+(ulong)td)));
        for(int p=IndexOf(img.Data,tdBytes,0);p>=0;p=IndexOf(img.Data,tdBytes,p+1))
        {
            int col=p-12;if(col<0||BitConverter.ToInt32(img.Data,col)!=0)continue;
            var colBytes=BitConverter.GetBytes(unchecked((uint)(img.Base+(ulong)col)));
            for(int m=IndexOf(img.Data,colBytes,0);m>=0;m=IndexOf(img.Data,colBytes,m+1))
            {
                uint vtable=unchecked((uint)(img.Base+(ulong)(m+4)));var vb=BitConverter.GetBytes(vtable);
                for(int q=IndexOf(img.Data,vb,img.TextStart);q>=0&&q<img.TextEnd;q=IndexOf(img.Data,vb,q+1))
                {
                    if(q<2||img.Data[q-2]!=0xC7||img.Data[q-1]!=0x01)continue;
                    for(int k=q+4;k<Math.Min(q+80,img.TextEnd-6);k++)
                    {
                        uint absolute=0;
                        if(img.Data[k]==0x89&&img.Data[k+1]==0x0D)absolute=BitConverter.ToUInt32(img.Data,k+2);
                        else if(img.Data[k]==0xA3)absolute=BitConverter.ToUInt32(img.Data,k+1);
                        if(absolute>=img.Base&&absolute<img.Base+(ulong)img.Data.Length)return (long)(absolute-img.Base);
                    }
                }
            }
        }
        return 0;
    }
    static void DiscoverX86(ClientProfile p,Image img,Func<string,string,List<int>> sig)
    {
        p.PythonLauncher=SingletonGlobal32(img,"CPythonLauncher");p.Report.Add(Line("Python başlatıcı (CPythonLauncher)",p.PythonLauncher,1));
        p.Player=SingletonGlobal32(img,"CPythonPlayer");p.Report.Add(Line("Oyuncu (CPythonPlayer)",p.Player,1));
        p.ItemManager=SingletonGlobal32(img,"CItemManager");p.Report.Add(Line("Eşya yöneticisi (CItemManager)",p.ItemManager,1));
        p.SkillManager=SingletonGlobal32(img,"CPythonSkill");p.Report.Add(Line("Beceri yöneticisi (CPythonSkill)",p.SkillManager,1));
        var skill=sig("skill32","8D 04 80 8B 84 81 ?? ?? ?? ?? C3");
        if(skill.Count>0)p.SkillArray=skill.Min(h=>BitConverter.ToInt32(img.Data,h+6));p.Report.Add(Line("Beceri dizisi konumu",p.SkillArray,skill.Count));
        var item=sig("item32","0F B7 43 01 6B C0 ?? 05 ?? ?? ?? ?? 03 C7");
        if(item.Count>0){var strides=item.Select(h=>(int)img.Data[h+6]).GroupBy(v=>v).OrderByDescending(g=>g.Count()).First();p.ItemStride=strides.Key;p.ItemArray=item.Where(h=>img.Data[h+6]==p.ItemStride).Min(h=>BitConverter.ToInt32(img.Data,h+8));}
        p.Report.Add(Line("Eşya kayıt boyutu",p.ItemStride,item.Count));p.Report.Add(Line("Envanter konumu",p.ItemArray,item.Count));
        p.Report.Add("• x86 Python çalışma fonksiyonları canlı modül dışa aktarımlarından otomatik aranacak.");
    }
    static ClientProfile Discover(string path,string sha)
    {
        var info=ClientBinaryInfo.Read(path);
        var p=new ClientProfile{Sha256=sha,ExePath=path,ExeSize=new FileInfo(path).Length,FoundAt=DateTime.Now,Architecture=info.Architecture,PointerSize=info.PointerSize,PreferredImageBase=info.ImageBase};
        var img=Image.Map(path);
        p.Report.Add("✓ Mimari: "+info.DisplayName+" · tercih edilen taban 0x"+info.ImageBase.ToString("X"));
        Func<string,string,List<int>> sig=(label,s)=>Find(img.Data,img.TextStart,img.TextEnd,s);
        if(!img.Is64){DiscoverX86(p,img,sig);return p;}
        var enq=sig("enqueue","48 8B 05 ?? ?? ?? ?? 4C 8B CA 4C 8B C1 48 8D 15 ?? ?? ?? ?? 48 8B C8 E9");
        if(enq.Count==1)p.PythonEnqueue=enq[0];p.Report.Add(Line("Python iş sırası fonksiyonu",p.PythonEnqueue,enq.Count));
        var run=sig("run","48 83 EC 38 4C 8B 41 10 48 8B C2 4D 8B C8 48 C7 44 24 20 00 00 00 00 48 8B C8 BA 01 01 00 00 FF 15");
        if(run.Count==1)p.PythonRun=run[0];p.Report.Add(Line("Python çalıştırma fonksiyonu",p.PythonRun,run.Count));
        p.PythonLauncher=SingletonGlobal(img,"CPythonLauncher");p.Report.Add(Line("Python başlatıcı (CPythonLauncher)",p.PythonLauncher,1));
        p.Player=SingletonGlobal(img,"CPythonPlayer");p.Report.Add(Line("Oyuncu (CPythonPlayer)",p.Player,1));
        p.ItemManager=SingletonGlobal(img,"CItemManager");p.Report.Add(Line("Eşya yöneticisi (CItemManager)",p.ItemManager,1));
        p.SkillManager=SingletonGlobal(img,"CPythonSkill");p.Report.Add(Line("Beceri yöneticisi (CPythonSkill)",p.SkillManager,1));
        // Skill slot array: the smallest field offset used by the slot accessors is the array start.
        var skill=sig("skill","48 8D 04 80 8B 84 C1 ?? ?? ?? ?? C3");
        if(skill.Count>0)p.SkillArray=skill.Min(h=>BitConverter.ToInt32(img.Data,h+7));p.Report.Add(Line("Beceri dizisi konumu",p.SkillArray,skill.Count));
        // Item slots: "movzx eax,[rbx+1]; imul rax,rax,stride; add rax,base": stride agreed by all sites, inventory = smallest base.
        var item=sig("item","0F B7 43 01 48 6B C0 ?? 48 05 ?? ?? ?? ?? 48 03 C7");
        if(item.Count>0)
        {
            var strides=item.Select(h=>(int)img.Data[h+7]).GroupBy(v=>v).OrderByDescending(g=>g.Count()).First();
            p.ItemStride=strides.Key;p.ItemArray=item.Where(h=>img.Data[h+7]==p.ItemStride).Min(h=>BitConverter.ToInt32(img.Data,h+10));
        }
        p.Report.Add(Line("Eşya kayıt boyutu",p.ItemStride,item.Count));p.Report.Add(Line("Envanter konumu",p.ItemArray,item.Count));
        return p;
    }
    static string Line(string what,long value,int hits){return (value>0?"✓ ":"✗ ")+what+": "+(value>0?"0x"+value.ToString("X"):"bulunamadı")+(hits>1?" ("+hits+" eşleşmeden seçildi)":"");}

    public override string ToString()
    {
        var sb=new StringBuilder();
        sb.AppendLine("EXE: "+ExePath);sb.AppendLine("SHA-256: "+Sha256);sb.AppendLine("Mimari: "+(Architecture??"unknown")+" · "+PointerSize*8+" bit işaretçi");sb.AppendLine("Boyut: "+ExeSize.ToString("N0")+" bayt");
        if(Report.Count>0)foreach(var l in Report)sb.AppendLine(l);
        else{sb.AppendLine("(kayıtlı profil "+FoundAt.ToString("g")+")");
            foreach(var kv in new[]{("Python iş sırası",PythonEnqueue),("Python çalıştırma",PythonRun),("Python başlatıcı",PythonLauncher),("Oyuncu",Player),("Eşya yöneticisi",ItemManager),("Beceri yöneticisi",SkillManager),("Beceri dizisi",(long)SkillArray),("Envanter",(long)ItemArray),("Eşya kayıt boyutu",(long)ItemStride)})
                sb.AppendLine((kv.Item2>0?"✓ ":"✗ ")+kv.Item1+": 0x"+kv.Item2.ToString("X"));}
        return sb.ToString();
    }
}
