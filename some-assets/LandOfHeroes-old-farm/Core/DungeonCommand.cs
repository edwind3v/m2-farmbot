using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[TypeConverter(typeof(DungeonEnumConverter))]
public enum DungeonCommandKind { Wait, Move, ClearTargets, CollectItem, GiveItem, UseItem, Interact, Dialogue, WaitMap, WaitTarget, WaitTargetGone, If, Else, EndIf, Repeat, EndRepeat, Notify, Stop, RepeatWhile, BreakRepeat }
[TypeConverter(typeof(DungeonEnumConverter))]
public enum DungeonCondition { ItemAvailable, TargetExists, MapEquals, HealthBelow, FreeInventorySlots }
[TypeConverter(typeof(DungeonEnumConverter))]
public enum DungeonInteractionResult { Sent, DialogueOpened, TargetGone, TargetAppeared, MapChanged }
public sealed class DungeonEnumConverter:EnumConverter {
    public DungeonEnumConverter(Type type):base(type){}
    public override object ConvertTo(ITypeDescriptorContext c,CultureInfo culture,object value,Type destination) {
        return destination==typeof(string)?DungeonCommand.Label(value):base.ConvertTo(c,culture,value,destination);
    }
    public override object ConvertFrom(ITypeDescriptorContext c,CultureInfo culture,object value) {
        if(value is string)foreach(object v in Enum.GetValues(EnumType))if(DungeonCommand.Label(v)==(string)value)return v;
        return base.ConvertFrom(c,culture,value);
    }
}
[Serializable]
public sealed class DungeonCommand {
    [DisplayName("Komut"),Category("1 · İşlem")]
    public DungeonCommandKind Kind {get;set;}=DungeonCommandKind.Wait;
    [DisplayName("Açıklama / ad"),Category("1 · İşlem")]
    public string Name {get;set;}="";
    [DisplayName("Zaman aşımı (ms)"),Description("Sonuç doğrulanmazsa görev durur. Eşya verme otomatik tekrarlanmaz."),Category("1 · İşlem")]
    public int TimeoutMilliseconds {get;set;}=60000;
    [DisplayName("Bekleme / sakin süre (ms)"),Description("Bekle komutunun süresi; hedef temizlemede hedef kalmadıktan sonraki sakin süre."),Category("1 · İşlem")]
    public int WaitMilliseconds {get;set;}=1500;
    [DisplayName("Hedef adı"),Description("Boş bırakılırsa tür/VNUM filtresi kullanılır. Ad girilirse tam ad eşleşir."),Category("2 · Hedef")]
    public string TargetName {get;set;}="";
    [DisplayName("Hedef türü"),TypeConverter(typeof(DungeonTargetKindConverter)),Description("Taş, Canavar, NPC veya Tümü. Kesme komutunda Tümü kullanılamaz."),Category("2 · Hedef")]
    public string TargetKind {get;set;}="NPC";
    [DisplayName("Hedef VNUM"),Description("Canlı VID değildir. Pack/hedef seç düğmesi ile doldurabilirsiniz. 0: yalnız ad/tür."),Category("2 · Hedef")]
    public int TargetVnum {get;set;}
    [DisplayName("Okunan motor türü (-1: genel filtre)"),Description("Canlı gözlemden aktarılır. NPC gibi görünen kapıların gerçek motor türünü korur."),Category("2 · Hedef")]
    public int NativeTargetType {get;set;}=-1;
    [DisplayName("Hedefi işaretli alanla sınırla"),Description("Aynı VNUM/ad başka odadaysa ona gitmez. Harita ve merkez çevresi kontrol edilir."),Category("2 · Hedef")]
    public bool ConstrainTargetArea {get;set;}
    [DisplayName("Hedef alanı yarıçapı"),Category("4 · Konum")]
    public int TargetRadius {get;set;}=600;
    [DisplayName("Tıklama sonrası doğrulama"),Category("7 · Tıklama sonucu")]
    public DungeonInteractionResult InteractionResult {get;set;}
    [DisplayName("Doğacak hedef VNUM"),Description("Tıklamadan önceki nesneler sayılmaz; bu VNUM ile yeni doğan bir nesne beklenir."),Category("7 · Tıklama sonucu")]
    public int ResultTargetVnum {get;set;}
    [DisplayName("Geçilecek harita"),Category("7 · Tıklama sonucu")]
    public string ResultMapName {get;set;}="";
    [DisplayName("Eşya ID"),Category("3 · Eşya")]
    public uint ItemId {get;set;}
    [DisplayName("Eşya adedi"),Description("Toplama/bekleme koşulunda gereken adet; hedefe vermede tek işlem adedi."),Category("3 · Eşya")]
    public int Count {get;set;}=1;
    [DisplayName("Harita adı"),Category("4 · Konum")]
    public string MapName {get;set;}="";
    [DisplayName("X (oyun koordinatı)"),Category("4 · Konum")]
    public float X {get;set;}
    [DisplayName("Y (oyun koordinatı)"),Category("4 · Konum")]
    public float Y {get;set;}
    [DisplayName("Yaklaşma mesafesi"),Category("4 · Konum")]
    public int Range {get;set;}=180;
    [DisplayName("Saldırı biçimi"),TypeConverter(typeof(DungeonAttackModeConverter)),Description("Yakın veya Menzilli."),Category("5 · Saldırı")]
    public string AttackMode {get;set;}="Yakın";
    [DisplayName("Saldırı aralığı (ms)"),Category("5 · Saldırı")]
    public int AttackMilliseconds {get;set;}=180;
    [DisplayName("Menzilli saldırı mesafesi"),Category("5 · Saldırı")]
    public int AttackRange {get;set;}=2500;
    [DisplayName("En az bir hedef görmeden tamamlanmasın"),Category("5 · Saldırı")]
    public bool RequireTargetSeen {get;set;}=true;
    [DisplayName("Kesilecek hedef sayısı (0: hepsi)"),Category("5 · Saldırı")]
    public int KillLimit {get;set;}
    [DisplayName("Koşul"),Category("6 · Koşul / tekrar")]
    public DungeonCondition Condition {get;set;}=DungeonCondition.ItemAvailable;
    [DisplayName("Koşulu tersine çevir"),Category("6 · Koşul / tekrar")]
    public bool Negate {get;set;}
    [DisplayName("Can yüzdesi eşiği"),Category("6 · Koşul / tekrar")]
    public int HealthPercent {get;set;}=30;
    [DisplayName("Tekrar sayısı"),Category("6 · Koşul / tekrar")]
    public int RepeatCount {get;set;}=1;
    [Browsable(false)]
    public List<NpcDialogueStep> DialogueSteps {get;set;}=new List<NpcDialogueStep>();
    public DungeonCommand Copy() {
        var copy=(DungeonCommand)MemberwiseClone();copy.DialogueSteps=(DialogueSteps??new List<NpcDialogueStep>()).Select(s=>s.Copy()).ToList();return copy;
    }
    public static string Label(object v) {
        string[] command={"Bekle","Konuma git","Hedefleri temizle","Eşyayı topla / envanterde bekle","Eşyayı hedefe ver (sürükle)","Envanter eşyasını kullan","Hedefe tıkla","NPC cevap sırasını uygula","Haritayı bekle","Hedefin doğmasını bekle","Hedefin kaybolmasını bekle","Eğer koşul doğruysa","Değilse","Koşul sonu","Tekrar başlat","Tekrar sonu","Bilgi mesajı","Görevi bitir","Koşul sağlandıkça tekrarla","Tekrardan çık"};
        string[] condition={"Eşya envanterde var","Hedef haritada var","Harita adı eşit","Can eşiğin altında","Envanterde boş yuva var"};
        if(v is DungeonCommandKind)return command[(int)(DungeonCommandKind)v];
        if(v is DungeonCondition)return condition[(int)(DungeonCondition)v];
        if(v is DungeonInteractionResult)return new[]{"Yalnız gönderimi doğrula","NPC konuşmasının açılmasını bekle","Tıklanan nesnenin kaybolmasını bekle","Yeni nesnenin doğmasını bekle","Harita değişimini bekle"}[(int)(DungeonInteractionResult)v];
        return Convert.ToString(v);
    }
    public override string ToString(){return Label(Kind)+(string.IsNullOrWhiteSpace(Name)?"":" · "+Name);}
}
public sealed class DungeonTargetKindConverter:StringConverter {
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context){return true;}
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context){return true;}
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context){return new StandardValuesCollection(new[]{"Taş","Canavar","NPC","Tümü"});}
}
public sealed class DungeonAttackModeConverter:StringConverter {
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context){return true;}
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context){return true;}
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context){return new StandardValuesCollection(new[]{"Yakın","Menzilli"});}
}
public sealed class DungeonFlowPlan {
    public readonly List<DungeonCommand> Commands;
    public readonly Dictionary<int,int> End=new Dictionary<int,int>(),Alternate=new Dictionary<int,int>();
    public DungeonFlowPlan(IEnumerable<DungeonCommand> source) {
        Commands=(source??Enumerable.Empty<DungeonCommand>()).Select(c=>c==null?null:c.Copy()).ToList();
        if(Commands.Count==0||Commands.Count>1000)throw new InvalidOperationException("Komut akışı 1–1000 adım içermeli.");
        var stack=new Stack<int>();
        var breaks=new Dictionary<int,int>();
        for(int i=0;i<Commands.Count;i++) {
            var c=Commands[i];if(c==null)throw new InvalidOperationException("Boş komut var.");
            if(c.NativeTargetType < -1||c.NativeTargetType>255||c.TargetRadius<30||c.TargetRadius>20000)throw new InvalidOperationException("Motor türü veya hedef alanı geçersiz.");
            if(c.ConstrainTargetArea&&string.IsNullOrWhiteSpace(c.MapName))throw new InvalidOperationException("İşaretli hedef alanının haritası gerekli.");
            if(c.Kind==DungeonCommandKind.Interact&&c.InteractionResult==DungeonInteractionResult.TargetAppeared&&c.ResultTargetVnum<=0)throw new InvalidOperationException("Tıklama sonrası doğacak hedefin VNUM değerini seçin.");
            if(c.Kind==DungeonCommandKind.Interact&&c.InteractionResult==DungeonInteractionResult.MapChanged&&string.IsNullOrWhiteSpace(c.ResultMapName))throw new InvalidOperationException("Tıklama sonrası geçilecek haritayı belirtin.");
            if(c.Kind==DungeonCommandKind.ClearTargets&&c.NativeTargetType>=0&&c.NativeTargetType!=AddressCatalog.Field_035B&&c.NativeTargetType!=AddressCatalog.Field_035C)throw new InvalidOperationException("Kesme komutunda NPC/kapı/oyuncu motor türü kullanılamaz.");
            if(c.Kind==DungeonCommandKind.ClearTargets&&c.NativeTargetType>=0&&c.NativeTargetType!=(c.TargetKind=="Taş"?AddressCatalog.Field_035B:AddressCatalog.Field_035C))throw new InvalidOperationException("Kesme filtresi okunan motor türüyle uyuşmuyor.");
            if(c.KillLimit<0||c.KillLimit>100000)throw new InvalidOperationException("Kesilecek hedef sayısı geçersiz.");
            if(c.AttackMode!="Yakın"&&c.AttackMode!="Menzilli")throw new InvalidOperationException("Saldırı biçimi Yakın veya Menzilli olmalı.");
            if(c.TargetKind!="Taş"&&c.TargetKind!="Canavar"&&c.TargetKind!="NPC"&&c.TargetKind!="Tümü")throw new InvalidOperationException("Hedef türü Taş, Canavar, NPC veya Tümü olmalı.");
            if(c.TimeoutMilliseconds<100||c.TimeoutMilliseconds>3600000||c.WaitMilliseconds<0||c.WaitMilliseconds>600000||c.Count<1||c.Count>1000000||c.Range<30||c.Range>9500||c.AttackMilliseconds<1||c.AttackMilliseconds>1000||c.AttackRange<100||c.AttackRange>9500||c.HealthPercent<0||c.HealthPercent>100||float.IsNaN(c.X)||float.IsInfinity(c.X)||float.IsNaN(c.Y)||float.IsInfinity(c.Y))
                throw new InvalidOperationException((i+1)+". komutta süre/mesafe/adet geçersiz.");
            if((c.Kind==DungeonCommandKind.GiveItem||c.Kind==DungeonCommandKind.CollectItem||c.Kind==DungeonCommandKind.UseItem||((c.Kind==DungeonCommandKind.If||c.Kind==DungeonCommandKind.RepeatWhile)&&c.Condition==DungeonCondition.ItemAvailable))&&c.ItemId==0)throw new InvalidOperationException((i+1)+". komutta eşya seçilmeli.");
            if(c.Kind==DungeonCommandKind.ClearTargets&&(c.TargetKind!="Taş"&&c.TargetKind!="Canavar"))throw new InvalidOperationException("Hedef temizlemede Taş veya Canavar seçin.");
            if((c.Kind==DungeonCommandKind.GiveItem||c.Kind==DungeonCommandKind.Interact||c.Kind==DungeonCommandKind.Dialogue)&&c.TargetVnum<=0&&string.IsNullOrWhiteSpace(c.TargetName))throw new InvalidOperationException((i+1)+". komutta belirli hedef seçin.");
            if((c.Kind==DungeonCommandKind.WaitMap||((c.Kind==DungeonCommandKind.If||c.Kind==DungeonCommandKind.RepeatWhile)&&c.Condition==DungeonCondition.MapEquals))&&string.IsNullOrWhiteSpace(c.MapName))throw new InvalidOperationException("Harita koşulu için harita adı gerekli.");
            if(c.Kind==DungeonCommandKind.Dialogue&&(c.DialogueSteps==null||c.DialogueSteps.Count==0))throw new InvalidOperationException("NPC cevap sırası boş.");
            if(c.Kind==DungeonCommandKind.If||c.Kind==DungeonCommandKind.Repeat||c.Kind==DungeonCommandKind.RepeatWhile) {
                if(stack.Count>=8)throw new InvalidOperationException("En fazla 8 iç içe koşul/tekrar kullanılabilir.");
                if(c.Kind!=DungeonCommandKind.If&&(c.RepeatCount<1||c.RepeatCount>100))throw new InvalidOperationException("Tekrar sayısı / koşullu tekrar üst sınırı 1–100 olmalı.");
                stack.Push(i);
            } else if(c.Kind==DungeonCommandKind.BreakRepeat) {
                int[] parents=stack.Where(s=>Commands[s].Kind==DungeonCommandKind.Repeat||Commands[s].Kind==DungeonCommandKind.RepeatWhile).ToArray();
                if(parents.Length==0)throw new InvalidOperationException("Tekrardan çık komutu bir tekrar bloğu içinde olmalı.");
                breaks[i]=parents[0];
            } else if(c.Kind==DungeonCommandKind.Else) {
                if(stack.Count==0||Commands[stack.Peek()].Kind!=DungeonCommandKind.If||Alternate.ContainsKey(stack.Peek()))throw new InvalidOperationException("Değilse komutunun eşleşen Eğer başlangıcı yok.");
                Alternate[stack.Peek()]=i;
            } else if(c.Kind==DungeonCommandKind.EndIf||c.Kind==DungeonCommandKind.EndRepeat) {
                if(stack.Count==0)throw new InvalidOperationException("Koşul/tekrar sonunun başlangıcı yok.");
                int start=stack.Pop();if((Commands[start].Kind==DungeonCommandKind.If)!=(c.Kind==DungeonCommandKind.EndIf))throw new InvalidOperationException("Koşul ve tekrar blokları çapraz kapanamaz.");
                End[start]=i;End[i]=start;if(Alternate.ContainsKey(start))End[Alternate[start]]=i;
            }
        }
        if(stack.Count>0)throw new InvalidOperationException("Koşul/tekrar bloğu kapanmamış.");
        foreach(var item in breaks)End[item.Key]=End[item.Value];
    }
    public async Task RunAsync(Func<DungeonCommand,CancellationToken,Task> execute,Func<DungeonCommand,CancellationToken,Task<bool>> condition,Action<int,DungeonCommand> progress,CancellationToken token) {
        var remaining=new Dictionary<int,int>();int steps=0;
        for(int i=0;i<Commands.Count;i++) {
            token.ThrowIfCancellationRequested();if(++steps>100000)throw new InvalidOperationException("Güvenli toplam adım sınırı aşıldı.");
            var c=Commands[i];progress(i,c);
            switch(c.Kind) {
                case DungeonCommandKind.If:
                    bool matched=await condition(c,token);if(c.Negate)matched=!matched;
                    if(!matched)i=Alternate.ContainsKey(i)?Alternate[i]:End[i];break;
                case DungeonCommandKind.Else:i=End[i];break;
                case DungeonCommandKind.Repeat:remaining[i]=c.RepeatCount;break;
                case DungeonCommandKind.RepeatWhile:
                    bool loopMatch=await condition(c,token);if(c.Negate)loopMatch=!loopMatch;
                    if(!loopMatch)i=End[i];else remaining[i]=c.RepeatCount;break;
                case DungeonCommandKind.EndRepeat:
                    int start=End[i];
                    if(Commands[start].Kind==DungeonCommandKind.RepeatWhile) {
                        bool again=await condition(Commands[start],token);if(Commands[start].Negate)again=!again;
                        if(!again){remaining.Remove(start);break;}
                    }
                    if(--remaining[start]>0)i=start;
                    else{remaining.Remove(start);if(Commands[start].Kind==DungeonCommandKind.RepeatWhile)throw new InvalidOperationException("Koşullu tekrar üst sınırına ulaştı; sonsuz döngüye girilmedi.");}break;
                case DungeonCommandKind.BreakRepeat:
                    remaining.Remove(End[End[i]]);i=End[i];break;
                case DungeonCommandKind.EndIf:break;
                case DungeonCommandKind.Stop:return;
                default:await execute(c,token);break;
            }
        }
    }
}
