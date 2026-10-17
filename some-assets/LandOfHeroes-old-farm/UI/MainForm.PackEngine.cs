using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private TextBox packRootBox,packTargetSearch,packItemSearch;
    private Label packCatalogStatus,packCombatStatus,packRhythmStatus,packMapStatus,packSmartStatus;
    private CheckBox packExplicitEnable,packStoneFilter,packMonsterFilter,packBossFilter,packNpcFilter,packSmartWatch;
    private CheckedListBox packTargetList;
    private ComboBox packAiMode,packMotionCombo,packMapCombo,packDungeonNpc,packDungeonBoss,packDungeonItem,packItemCombo;
    private NumericUpDown packAttackSpeed,packStuckSeconds,packSkillSlot;
    private Button packStartButton,packDungeonDraftButton,packItemRuleButton;
    private PackDataCatalog packCatalog;
    private readonly HashSet<int> packSelectedVnums=new HashSet<int>();
    private System.Windows.Forms.Timer packSmartTimer;
    private bool packCatalogLoading,packEngineRunning,packEngineOwnsAi,packSmartBusy;
    private int packOwnedAiType,packLastTargetVid;
    private int[] packOwnedVnums=new int[0];
    private float packLastTargetDistance=float.MaxValue;
    private float? packLastTargetHp;
    private DateTime packLastTargetProgressUtc=DateTime.UtcNow;

    private void BuildPackEnginePage()
    {
        var page=NewControlPage("Pack Motoru");page.Name="packEnginePage";page.AutoScrollMinSize=new Size(0,1180);
        page.Controls.Add(PlainLabel("PACK KATALOĞU · yalnızca seçtiğiniz çıkarılmış dosyaları okur",16,14,720));
        packRootBox=new TextBox{Name="packRootBox",Left=16,Top=42,Width=780,Height=28,Text=string.IsNullOrWhiteSpace(advancedState.PackRoot)?DefaultPackEngineRoot():advancedState.PackRoot};
        var browse=PlainButton("Klasör seç",808,39,110);browse.Name="packBrowseButton";browse.Click+=(s,e)=>BrowsePackRoot();
        var load=PlainButton("Verileri yükle",928,39,120);load.Name="packLoadButton";load.Click+=async(s,e)=>await LoadPackCatalogAsync();
        packCatalogStatus=PlainLabel("Henüz yüklenmedi.",16,76,1032);packCatalogStatus.Name="packCatalogStatus";packCatalogStatus.Height=38;
        page.Controls.AddRange(new Control[]{packRootBox,browse,load,packCatalogStatus});

        var combatBox=NewPackGroup("1–4 · Yerleşik saldırı, kesin hedef ve akıllı gözetim",16,122,520,492);
        combatBox.Controls.Add(PlainLabel("Hedef ara · ad veya VNUM",16,30,250));
        packTargetSearch=new TextBox{Name="packTargetSearch",Left=16,Top=56,Width=290,Height=28};packTargetSearch.TextChanged+=(s,e)=>RefreshPackTargetList();
        var search=PlainButton("Ara",316,53,70);search.Click+=(s,e)=>RefreshPackTargetList();
        var all=PlainButton("Görüneni seç",394,53,104);all.Click+=(s,e)=>CheckVisiblePackTargets(true);
        packStoneFilter=PackCheck("Taş",16,92,true);packMonsterFilter=PackCheck("Canavar",86,92,true);packBossFilter=PackCheck("Patron",182,92,true);packNpcFilter=PackCheck("NPC",270,92,false);
        foreach(var check in new[]{packStoneFilter,packMonsterFilter,packBossFilter,packNpcFilter})check.CheckedChanged+=(s,e)=>RefreshPackTargetList();
        packTargetList=new CheckedListBox{Name="packTargetList",Left=16,Top=120,Width=482,Height=170,CheckOnClick=true,IntegralHeight=false};
        packTargetList.ItemCheck+=(s,e)=>BeginInvoke((Action)(()=>UpdatePackSelectedVnums()));
        combatBox.Controls.AddRange(new Control[]{packTargetSearch,search,all,packStoneFilter,packMonsterFilter,packBossFilter,packNpcFilter,packTargetList});
        combatBox.Controls.Add(PlainLabel("Çalışma biçimi",16,302,180));
        packAiMode=new ComboBox{Name="packAiMode",Left=16,Top=328,Width=260,DropDownStyle=ComboBoxStyle.DropDownList};
        packAiMode.Items.AddRange(new object[]{"Eşya PlayerAI · tür 0","EXP PlayerAI · tür 1","Taş kuyruğu + normal vuruş"});packAiMode.SelectedIndex=1;
        packSmartWatch=PackCheck("Takılan hedefi izle ve tekrar sırala",288,331,true);packSmartWatch.Width=210;
        packExplicitEnable=PackCheck("Pack motorunu bilerek etkinleştir",16,370,false);packExplicitEnable.Name="packExplicitEnable";packExplicitEnable.Width=230;
        packStuckSeconds=new NumericUpDown{Name="packStuckSeconds",Left=350,Top=368,Width=70,Minimum=3,Maximum=60,Value=7};
        combatBox.Controls.Add(PlainLabel("Takılma sn",426,370,72));
        packStartButton=PlainButton("Motoru başlat",16,408,150);packStartButton.Name="packStartButton";packStartButton.Click+=async(s,e)=>{if(packEngineRunning)await StopPackEngineAsync("Kullanıcı durdurdu.");else await StartPackEngineAsync();};
        var stoneOnce=PlainButton("Taşı bir kez sırala",176,408,158);stoneOnce.Name="packStoneOnceButton";stoneOnce.Click+=async(s,e)=>await QueuePackStoneOnceAsync();
        var stop=PlainButton("Acil durdur",344,408,154);stop.Click+=async(s,e)=>await StopPackEngineAsync("Acil durduruldu.");
        packCombatStatus=PlainLabel("Mevcut Wait, hareket ve zindan motorları değişmedi.",16,450,482);packCombatStatus.Name="packCombatStatus";packCombatStatus.Height=34;
        combatBox.Controls.AddRange(new Control[]{packAiMode,packSmartWatch,packExplicitEnable,packStuckSeconds,packStartButton,stoneOnce,stop,packCombatStatus});

        var right=NewPackGroup("3 · MSA hareket süresinden güvenli ritim",552,122,512,238);
        right.Controls.Add(PlainLabel("Hareket dosyası",16,30,220));
        packMotionCombo=new ComboBox{Name="packMotionCombo",Left=16,Top=56,Width=474,DropDownStyle=ComboBoxStyle.DropDownList};packMotionCombo.SelectedIndexChanged+=(s,e)=>RefreshPackRhythm();
        right.Controls.Add(PlainLabel("Oyundaki ATT_SPEED",16,98,170));
        packAttackSpeed=new NumericUpDown{Name="packAttackSpeed",Left=16,Top=124,Width=110,Minimum=1,Maximum=1000,Value=170};
        var readSpeed=PlainButton("Oyundan oku",136,121,126);readSpeed.Click+=async(s,e)=>await ReadPackAttackSpeedAsync();
        var apply=PlainButton("Wait'e açıkça uygula",272,121,180);apply.Name="packApplyRhythmButton";apply.Click+=(s,e)=>ApplyPackRhythmToWait();
        packAttackSpeed.ValueChanged+=(s,e)=>RefreshPackRhythm();
        packRhythmStatus=PlainLabel("MSA seçildiğinde güvenli aralık hesaplanır.",16,166,474);packRhythmStatus.Name="packRhythmStatus";packRhythmStatus.Height=42;
        right.Controls.AddRange(new Control[]{packMotionCombo,packAttackSpeed,readSpeed,apply,packRhythmStatus});

        var smart=NewPackGroup("4 · Can / mesafe / ilerleme gözetimi",552,374,512,240);
        packSmartStatus=PlainLabel("Motor çalışınca hedef VID, mesafe ve hedef tahtasındaki can izlenir.",16,34,474);packSmartStatus.Name="packSmartStatus";packSmartStatus.Height=86;
        var note=PlainLabel("Takılma algılanırsa yalnız taş kuyruğu modunda hedef yeniden sıraya alınır. PlayerAI modunda yalnız raporlanır; oyun davranışı zorla değiştirilmez.",16,128,474);note.Height=64;
        smart.Controls.AddRange(new Control[]{packSmartStatus,note});

        var mapBox=NewPackGroup("5 · Harita ve rota verisi",16,632,520,246);
        mapBox.Controls.Add(PlainLabel("Atlas haritası",16,30,180));
        packMapCombo=new ComboBox{Name="packMapCombo",Left=16,Top=56,Width=482,DropDownStyle=ComboBoxStyle.DropDownList};packMapCombo.SelectedIndexChanged+=(s,e)=>RefreshPackMapStatus();
        var preview=PlainButton("Haritayı önizle",16,98,160);preview.Name="packMapPreviewButton";preview.Click+=(s,e)=>PreviewSelectedPackMap();
        var current=PlainButton("Oyundaki haritayı bul",186,98,190);current.Click+=async(s,e)=>await SelectCurrentPackMapAsync();
        packMapStatus=PlainLabel("Atlas ve attr.atr/MapProperty bilgileri burada doğrulanır.",16,146,482);packMapStatus.Name="packMapStatus";packMapStatus.Height=70;
        mapBox.Controls.AddRange(new Control[]{packMapCombo,preview,current,packMapStatus});

        var dungeon=NewPackGroup("6 · Packtan zindan taslağı",552,632,512,330);
        dungeon.Controls.Add(PlainLabel("Giriş NPC",16,28,145));packDungeonNpc=PackDropDown("packDungeonNpc",16,52,230);
        dungeon.Controls.Add(PlainLabel("Patron",256,28,145));packDungeonBoss=PackDropDown("packDungeonBoss",256,52,230);
        dungeon.Controls.Add(PlainLabel("Gerekli eşya",16,94,145));packDungeonItem=PackDropDown("packDungeonItem",16,118,470);
        packDungeonDraftButton=PlainButton("Seçili verilerle yeni zindan taslağı",16,164,300);packDungeonDraftButton.Name="packDungeonDraftButton";packDungeonDraftButton.Click+=(s,e)=>CreatePackDungeonDraft();
        var open=PlainButton("Zindan editörünü aç",326,164,160);open.Click+=(s,e)=>OpenSelectedDungeonEditor();
        var dungeonNote=PlainLabel("Harita, NPC VNUM, patron VNUM ve gerekli eşya VNUM'u profile sabitlenir. Sonra Zindan Akışı sayfasında aşamaları düzenleyebilirsiniz.",16,210,470);dungeonNote.Height=82;
        dungeon.Controls.AddRange(new Control[]{packDungeonNpc,packDungeonBoss,packDungeonItem,packDungeonDraftButton,open,dungeonNote});

        var inventory=NewPackGroup("7 · Packtan eşya ve beceri işlemleri",16,896,520,248);
        inventory.Controls.Add(PlainLabel("Eşya ara",16,28,145));packItemSearch=new TextBox{Name="packItemSearch",Left=16,Top=52,Width=230};packItemSearch.TextChanged+=(s,e)=>RefreshPackItemCombo();
        packItemCombo=PackDropDown("packItemCombo",256,52,242);
        packItemRuleButton=PlainButton("Envanter kuralına ekle",16,94,190);packItemRuleButton.Name="packItemRuleButton";packItemRuleButton.Click+=(s,e)=>AddPackItemRule();
        var use=PlainButton("Oyunda bir kez kullan",216,94,180);use.Name="packItemUseButton";use.Click+=async(s,e)=>await UseSelectedPackItemAsync();
        inventory.Controls.Add(PlainLabel("Beceri yuvası",16,144,140));packSkillSlot=new NumericUpDown{Name="packSkillSlot",Left=16,Top=170,Width=90,Minimum=0,Maximum=255,Value=0};
        var skill=PlainButton("Beceriyi bir kez kullan",116,167,190);skill.Name="packSkillUseButton";skill.Click+=async(s,e)=>await UsePackSkillAsync();
        inventory.Controls.AddRange(new Control[]{packItemSearch,packItemCombo,packItemRuleButton,use,packSkillSlot,skill});

        page.Controls.AddRange(new Control[]{combatBox,right,smart,mapBox,dungeon,inventory});
        packSmartTimer=new System.Windows.Forms.Timer(components){Interval=1000};packSmartTimer.Tick+=async(s,e)=>await PackSmartPulseAsync();
    }

    private static GroupBox NewPackGroup(string text,int x,int y,int w,int h){return new GroupBox{Text=text,Left=x,Top=y,Width=w,Height=h,Padding=new Padding(10)};}
    private static CheckBox PackCheck(string text,int x,int y,bool value){return new CheckBox{Text=text,Left=x,Top=y,Width=92,Height=24,Checked=value};}
    private static ComboBox PackDropDown(string name,int x,int y,int width){return new ComboBox{Name=name,Left=x,Top=y,Width=width,DropDownStyle=ComboBoxStyle.DropDown,AutoCompleteMode=AutoCompleteMode.SuggestAppend,AutoCompleteSource=AutoCompleteSource.ListItems};}
    private static string DefaultPackEngineRoot(){string path=@"C:\Program Files\LandOfHeroesEU\Yeni klasör\Cikarilan-Packlar";return Directory.Exists(path)?path:"";}

    private void BrowsePackRoot(){using(var dialog=new FolderBrowserDialog{Description="Çıkarılan-Packlar ana klasörünü seçin",SelectedPath=packRootBox?.Text??advancedState.PackRoot})if(dialog.ShowDialog(this)==DialogResult.OK){if(packRootBox!=null)packRootBox.Text=dialog.SelectedPath;}}

    private async Task EnsurePackCatalogLoadedAsync()
    {
        if(packCatalog!=null||packCatalogLoading)return;
        string root=!string.IsNullOrWhiteSpace(advancedState?.PackRoot)?advancedState.PackRoot:DefaultPackEngineRoot();
        if(string.IsNullOrWhiteSpace(root)){SetPackIntegrationStatus("Pack klasörü bulunamadı · Görsel Ayarlar bölümünden bir kez seçin.");return;}
        await LoadPackCatalogCoreAsync(root,false);
    }

    private async Task SelectAutomaticPackRootAsync()
    {
        string selected=!string.IsNullOrWhiteSpace(advancedState?.PackRoot)?advancedState.PackRoot:DefaultPackEngineRoot();
        using(var dialog=new FolderBrowserDialog{Description="Çıkarılan-Packlar ana klasörünü seçin",SelectedPath=selected})
        {
            if(dialog.ShowDialog(this)!=DialogResult.OK)return;
            await LoadPackCatalogCoreAsync(dialog.SelectedPath,true);
        }
    }

    private async Task LoadPackCatalogAsync()
    {
        string root=packRootBox==null?advancedState.PackRoot:packRootBox.Text.Trim();
        await LoadPackCatalogCoreAsync(root,true);
    }

    private async Task LoadPackCatalogCoreAsync(string root,bool userInitiated)
    {
        if(packCatalogLoading)return;packCatalogLoading=true;SetPackIntegrationStatus("Pack verileri otomatik hazırlanıyor…");
        try
        {
            var loaded=await Task.Run(()=>PackDataCatalog.Load(root));
            if(IsDisposed||Disposing)return;packCatalog=loaded;advancedState.PackRoot=loaded.Root;SaveAdvancedState();
            await LoadVisualCatalogAsync(loaded.Root);
            string status="Pack otomatik etkin · "+loaded.Mobs.Count.ToString("N0")+" hedef · "+loaded.Items.Count.ToString("N0")+" eşya · "+loaded.Maps.Count+" harita · "+loaded.Spawns.Count.ToString("N0")+" kesin konum";
            SetPackIntegrationStatus(status);if(packMapCombo!=null)BindPackCatalog();EnrichVisibleInventoryFromPack();TryApplyPackNpcMarker();
            RecordActivity("Pack",status+(userInitiated?" · kullanıcı klasörü":""));
        }
        catch(Exception ex){if(!IsDisposed&&!Disposing)SetPackIntegrationStatus("Pack verileri okunamadı: "+ex.Message);}
        finally{packCatalogLoading=false;}
    }

    private void SetPackIntegrationStatus(string text)
    {
        if(packCatalogStatus!=null)packCatalogStatus.Text=text;
        if(packIntegrationStatus!=null)packIntegrationStatus.Text=text;
    }

    private string PackItemName(uint id,string fallback)
    {
        PackItemRecord record;
        if(packCatalog!=null&&packCatalog.ItemByVnum.TryGetValue(id,out record))
        {
            string name=string.IsNullOrWhiteSpace(record.LocaleName)?record.Name:record.LocaleName;
            if(!string.IsNullOrWhiteSpace(name))return name;
        }
        return fallback??"";
    }

    private void EnrichVisibleInventoryFromPack()
    {
        if(packCatalog==null)return;
        foreach(var item in visibleItems)item.Name=PackItemName(item.Id,item.Name);
        PopulateInventoryGrid();PopulateDungeonItemChoices();
    }

    private void TryApplyPackNpcMarker()
    {
        if(packCatalog==null||taskSequence==null||taskSequence.LobbyNpcMarkerSet||string.IsNullOrWhiteSpace(atlasName))return;
        var stage=CurrentDungeonStageSetting(1);if(stage==null)return;
        var matches=packCatalog.FindNpcSpawns(atlasName,stage.NpcVnum,stage.NpcName).ToList();if(matches.Count==0)return;
        var spawn=matches.OrderBy(s=>Math.Abs(s.X-mapPlayerX)+Math.Abs(s.Y-mapPlayerY)).First();
        taskSequence.LobbyNpcMarkerSet=true;taskSequence.LobbyNpcMapName=atlasName;
        taskSequence.LobbyNpcMapX=spawn.X/MinimapAtlas.UnitsPerPixel;taskSequence.LobbyNpcMapY=spawn.Y/MinimapAtlas.UnitsPerPixel;
        AutoSaveTaskSequence();RefreshDungeonNpcMarkerStatus();
        RecordActivity("Pack","NPC konumu otomatik bağlandı · "+spawn.DisplayName+" · "+atlasName);
    }

    private void BindPackCatalog()
    {
        if(packMapCombo==null||packMotionCombo==null||packDungeonNpc==null||packDungeonBoss==null||packDungeonItem==null)return;
        packMapCombo.BeginUpdate();packMapCombo.Items.Clear();foreach(var map in packCatalog.Maps.OrderBy(m=>m.ToString()))packMapCombo.Items.Add(map);packMapCombo.EndUpdate();if(packMapCombo.Items.Count>0)packMapCombo.SelectedIndex=0;
        packMotionCombo.BeginUpdate();packMotionCombo.Items.Clear();foreach(var motion in packCatalog.Motions.Take(3000))packMotionCombo.Items.Add(motion);packMotionCombo.EndUpdate();if(packMotionCombo.Items.Count>0)packMotionCombo.SelectedIndex=0;
        BindPackCombo(packDungeonNpc,packCatalog.Mobs.Where(m=>m.Kind==PackTargetKind.Npc).OrderBy(m=>m.LocaleName));
        BindPackCombo(packDungeonBoss,packCatalog.Mobs.Where(m=>m.Kind==PackTargetKind.Boss).OrderBy(m=>m.Level).ThenBy(m=>m.LocaleName));
        BindPackCombo(packDungeonItem,packCatalog.Items.OrderBy(i=>i.LocaleName));
        RefreshPackTargetList();RefreshPackItemCombo();RefreshPackMapStatus();RefreshPackRhythm();
    }
    private static void BindPackCombo<T>(ComboBox box,IEnumerable<T> values){box.BeginUpdate();box.Items.Clear();foreach(var value in values)box.Items.Add(value);box.EndUpdate();if(box.Items.Count>0)box.SelectedIndex=0;}

    private void RefreshPackTargetList()
    {
        if(packTargetList==null||packCatalog==null)return;
        packTargetList.BeginUpdate();packTargetList.Items.Clear();
        foreach(var mob in packCatalog.SearchMobs(packTargetSearch.Text,packStoneFilter.Checked,packMonsterFilter.Checked,packBossFilter.Checked,packNpcFilter.Checked))
            packTargetList.Items.Add(mob,packSelectedVnums.Contains(mob.Vnum));
        packTargetList.EndUpdate();
    }
    private void UpdatePackSelectedVnums(){if(packTargetList==null)return;packSelectedVnums.Clear();foreach(var item in packTargetList.CheckedItems.OfType<PackMobRecord>())packSelectedVnums.Add(item.Vnum);}
    private void CheckVisiblePackTargets(bool value){for(int i=0;i<packTargetList.Items.Count;i++)packTargetList.SetItemChecked(i,value);BeginInvoke((Action)UpdatePackSelectedVnums);}

    private IEnumerable<PackMobRecord> EffectivePackTargets()
    {
        if(packCatalog==null)return Enumerable.Empty<PackMobRecord>();
        var selected=packSelectedVnums.Select(v=>{PackMobRecord m;return packCatalog.MobByVnum.TryGetValue(v,out m)?m:null;}).Where(m=>m!=null).ToList();
        if(selected.Count>0)return selected;
        return packCatalog.SearchMobs("",packStoneFilter.Checked,packMonsterFilter.Checked,packBossFilter.Checked,false,5000);
    }

    private bool packStartPending,packStopConfirmed=true;
    private async Task StartPackEngineAsync()
    {
        if(emergencyLatched)return;
        if(!packExplicitEnable.Checked){packCombatStatus.Text="Önce “Pack motorunu bilerek etkinleştir” kutusunu işaretleyin.";return;}
        if(packCatalog==null){packCombatStatus.Text="Önce pack verilerini yükleyin.";return;}
        if(taskCancellation!=null||normalBotRequested||dmgStartCheck.Checked||RouteOwnsCombat||isRecordingRoute){packCombatStatus.Text="Çakışmayı önlemek için rota, normal bot, Wait saldırısı veya zindanı önce durdurun.";return;}
        int[] vnums=PackCombatPlanner.BuildVnumFilter(EffectivePackTargets());if(vnums.Length==0){packCombatStatus.Text="Taş, canavar veya patron hedefi seçilmedi.";return;}
        try
        {
            if(packAiMode.SelectedIndex<2)
            {
                int aiType=packAiMode.SelectedIndex==0?0:1;string list=string.Join(",",vnums.Select(v=>v.ToString(CultureInfo.InvariantCulture)));
                packStartPending=true;packOwnedAiType=aiType;packOwnedVnums=vnums;
                string result=await GamePythonAsync("import app\nif app.IsPlayerAIAttached():\n f.write('busy')\nelse:\n app.TriggerPlayerAI("+aiType+",["+list+"])\n if app.IsPlayerAIAttached():globals()['_onur_pack_owner']="+PythonLiteral(EmergencyPath)+"\n f.write('started' if app.IsPlayerAIAttached() else 'failed')",CancellationToken.None);
                if(result!="started"){packCombatStatus.Text=result=="busy"?"Oyunun yerleşik PlayerAI sistemi zaten çalışıyor; sahipliği alınmadı.":"PlayerAI başlatılamadı.";return;}
                packEngineOwnsAi=true;packOwnedAiType=aiType;packOwnedVnums=vnums;
            }
            else
            {
                await GamePythonAsync("import player\nplayer.AttackStone()\nplayer.SetAttackKeyState(1)\nf.write('started')",CancellationToken.None);
                packEngineOwnsAi=false;packOwnedVnums=vnums;
            }
            packEngineRunning=true;packStartButton.Text="Motoru durdur";packSmartTimer.Start();packLastTargetVid=0;packLastTargetDistance=float.MaxValue;packLastTargetHp=null;packLastTargetProgressUtc=DateTime.UtcNow;
            sessionDirty=true;
            packCombatStatus.Text="Aktif · "+vnums.Length+" VNUM · mevcut Wait/hareket/zindan kapalı ve korunuyor.";RecordActivity("Pack","Yerleşik motor başlatıldı · "+vnums.Length+" VNUM");
        }
        catch(Exception ex){packCombatStatus.Text="Başlatılamadı: "+ex.Message;await StopPackEngineAsync("Başlatma hatası.");}
        finally{packStartPending=false;}
    }

    private async Task StopPackEngineAsync(string reason)
    {
        packSmartTimer?.Stop();
        packStopConfirmed=true;
        try
        {
            if(packEngineOwnsAi||packStartPending)
            {
                string list=string.Join(",",packOwnedVnums.Select(v=>v.ToString(CultureInfo.InvariantCulture)));
                string result=await GamePythonAsync("import app\nif globals().get('_onur_pack_owner')=="+PythonLiteral(EmergencyPath)+":\n if app.IsPlayerAIAttached():app.TriggerPlayerAI("+packOwnedAiType+",["+list+"])\n if app.IsPlayerAIAttached():\n  f.write('unconfirmed')\n else:\n  globals().pop('_onur_pack_owner',None)\n  f.write('stopped')\nelse: f.write('stopped')",CancellationToken.None,true);packStopConfirmed=result=="stopped";
            }
            else if(packEngineRunning)packStopConfirmed=await GamePythonAsync("import player\nplayer.SetAttackKeyState(0)\nf.write('stopped')",CancellationToken.None,true)=="stopped";
        }
        catch{packStopConfirmed=false;}
        packEngineRunning=false;packEngineOwnsAi=false;packOwnedVnums=new int[0];if(packStartButton!=null)packStartButton.Text="Motoru başlat";if(packCombatStatus!=null)packCombatStatus.Text=reason;
        sessionDirty=true;
    }
    private void StopPackEngineForDispose(){if(!packEngineRunning)return;try{var ignored=StopPackEngineAsync("Uygulama kapanıyor.");}catch{}}

    private async Task QueuePackStoneOnceAsync()
    {
        try{string result=await GamePythonAsync("import player\nplayer.AttackStone()\nf.write('ok')",CancellationToken.None);packCombatStatus.Text=result=="ok"?"En yakın taş oyunun kendi kuyruğuna eklendi.":"Taş sıraya alınamadı.";}catch(Exception ex){packCombatStatus.Text=ex.Message;}
    }

    private async Task PackSmartPulseAsync()
    {
        if(!packEngineRunning||!packSmartWatch.Checked||packSmartBusy)return;packSmartBusy=true;
        try
        {
            string report=await GamePythonAsync("import player\nv=player.GetTargetVID()\nd=-1\nif v:\n try:d=player.GetCharacterDistance(v)\n except Exception:pass\nf.write(str(v)+'|'+str(d))",CancellationToken.None);
            var parts=report.Split('|');int vid;float distance;if(parts.Length<2||!int.TryParse(parts[0],out vid)||!float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out distance))return;
            float? hp=vid>0?await ReadStoneHealthAsync(vid,CancellationToken.None):null;
            bool progressed=vid!=packLastTargetVid||(distance>=0&&distance+60<packLastTargetDistance)||(hp.HasValue&&packLastTargetHp.HasValue&&hp.Value+0.1f<packLastTargetHp.Value);
            if(progressed)packLastTargetProgressUtc=DateTime.UtcNow;
            double idle=(DateTime.UtcNow-packLastTargetProgressUtc).TotalSeconds;
            packSmartStatus.Text="VID: "+vid+" · mesafe: "+(distance<0?"?":distance.ToString("0"))+" · can: "+(hp.HasValue?hp.Value.ToString("0.0")+"%":"?")+"\nİlerleme: "+(progressed?"var":idle.ToString("0")+" sn yok");
            if(idle>=(double)packStuckSeconds.Value)
            {
                if(packAiMode.SelectedIndex==2){await QueuePackStoneOnceAsync();packSmartStatus.Text+=" · taş yeniden sıraya alındı";}
                else packSmartStatus.Text+=" · PlayerAI takılması raporlandı";
                packLastTargetProgressUtc=DateTime.UtcNow;
            }
            packLastTargetVid=vid;packLastTargetDistance=distance;packLastTargetHp=hp;
        }
        catch(Exception ex){packSmartStatus.Text="Gözetim bekliyor: "+ex.Message;}
        finally{packSmartBusy=false;}
    }

    private async Task ReadPackAttackSpeedAsync()
    {
        try{string value=await GamePythonAsync("import player\nf.write(str(player.GetStatus(player.ATT_SPEED)))",CancellationToken.None);decimal parsed;if(decimal.TryParse(value,NumberStyles.Any,CultureInfo.InvariantCulture,out parsed))packAttackSpeed.Value=Math.Max(packAttackSpeed.Minimum,Math.Min(packAttackSpeed.Maximum,parsed));else packRhythmStatus.Text="ATT_SPEED okunamadı.";}catch(Exception ex){packRhythmStatus.Text=ex.Message;}
    }
    private int CurrentPackInterval(){var motion=packMotionCombo?.SelectedItem as PackMotionRecord;return PackCombatPlanner.SafeAttackIntervalMilliseconds((int)packAttackSpeed.Value,motion?.DurationSeconds??0.5);}
    private void RefreshPackRhythm(){if(packRhythmStatus==null)return;var motion=packMotionCombo?.SelectedItem as PackMotionRecord;packRhythmStatus.Text="Önerilen güvenli aralık: "+CurrentPackInterval()+" ms"+(motion==null?"":" · hareket "+motion.DurationSeconds.ToString("0.###",CultureInfo.CurrentCulture)+" sn");}
    private void ApplyPackRhythmToWait(){int delay=CurrentPackInterval();attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,delay));OnAttackDelayChanged(attackDelayBar,null);packRhythmStatus.Text="Mevcut Wait beklemesine açık kullanıcı isteğiyle "+attackDelayBar.Value+" ms uygulandı.";RecordActivity("Pack","MSA ritmi Wait'e uygulandı · "+attackDelayBar.Value+" ms");}

    private void RefreshPackMapStatus()
    {
        if(packMapStatus==null)return;var map=packMapCombo?.SelectedItem as PackAtlasRecord;if(map==null){packMapStatus.Text="Harita seçilmedi.";return;}
        packMapStatus.Text=(map.HasNavigationData?"Rota verisi hazır":"Rota verisi eksik")+" · "+map.Width+"×"+map.Height+" bölge · dünya başlangıcı "+map.X+", "+map.Y+"\n"+map.NavigationPath;
    }
    private async Task SelectCurrentPackMapAsync()
    {
        if(packCatalog==null)return;try{string current=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",CancellationToken.None);for(int i=0;i<packMapCombo.Items.Count;i++){var map=packMapCombo.Items[i] as PackAtlasRecord;if(map!=null&&(string.Equals(map.Directory,current,StringComparison.OrdinalIgnoreCase)||map.Directory.EndsWith("/"+current,StringComparison.OrdinalIgnoreCase))){packMapCombo.SelectedIndex=i;return;}}packMapStatus.Text="Oyundaki "+current+" atlas listesinde bulunamadı.";}catch(Exception ex){packMapStatus.Text=ex.Message;}
    }
    private void PreviewSelectedPackMap()
    {
        var map=packMapCombo?.SelectedItem as PackAtlasRecord;if(map==null||packCatalog==null)return;MinimapAtlas preview=null;
        try
        {
            var roots=new[]{packCatalog.Root,Path.Combine(packCatalog.Root,"map"),Path.Combine(packCatalog.Root,"maps")};preview=MinimapAtlas.Load(map.Directory,roots);if(preview==null){packMapStatus.Text="Bu haritada 000000/minimap veya attr alanları bulunamadı.";return;}
            var form=new Form{Text="Pack haritası · "+map,Size=new Size(800,800),StartPosition=FormStartPosition.CenterParent,BackColor=Color.Black,ShowInTaskbar=false};
            var picture=new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,Image=new Bitmap(preview.Image)};form.Controls.Add(picture);form.FormClosed+=(s,e)=>picture.Image.Dispose();form.Show(this);
        }
        catch(Exception ex){packMapStatus.Text="Önizleme açılamadı: "+ex.Message;}
        finally{preview?.Dispose();}
    }

    private void CreatePackDungeonDraft()
    {
        if(packCatalog==null)return;if(taskCancellation!=null){MessageBox.Show("Yeni taslak için çalışan zindanı durdurun.");return;}
        var map=packMapCombo.SelectedItem as PackAtlasRecord;var npc=packDungeonNpc.SelectedItem as PackMobRecord;var boss=packDungeonBoss.SelectedItem as PackMobRecord;var item=packDungeonItem.SelectedItem as PackItemRecord;
        var draft=new TaskSequence{Name=(map==null?"Pack":(string.IsNullOrWhiteSpace(map.Name)?map.Directory:map.Name))+" Zindanı",SourceMapName=map?.Directory??""};
        var first=draft.Stages[0];first.Name=(npc==null?"Giriş NPC":npc.LocaleName)+" ve giriş";first.NpcName=npc?.LocaleName??"";first.NpcVnum=npc?.Vnum??0;first.NpcVid=0;first.RequiredItemName=item?.LocaleName??"";first.RequiredItemId=item?.Vnum??0;
        draft.Stages[2].Name="Pack taş temizliği";draft.Stages[2].TargetKind="Taş";
        for(int i=4;i<8;i++){draft.Stages[i].Name=(boss?.LocaleName??"Patron")+" · "+(i-3);draft.Stages[i].TargetName=boss?.LocaleName??"";draft.Stages[i].TargetVnum=boss?.Vnum??0;draft.Stages[i].TargetKind="Patron";}
        dungeonTasks.Dungeons.Add(draft);SelectDungeonProfile(dungeonTasks.Dungeons.Count-1,false);SaveDungeonCatalog();packCombatStatus.Text=draft.Name+" taslağı oluşturuldu.";RecordActivity("Pack","Zindan taslağı oluşturuldu · "+draft.Name);OpenSelectedDungeonEditor();
    }

    private void RefreshPackItemCombo(){if(packItemCombo==null||packCatalog==null)return;BindPackCombo(packItemCombo,packCatalog.SearchItems(packItemSearch.Text,250));}
    private void AddPackItemRule(){var item=packItemCombo?.SelectedItem as PackItemRecord;if(item==null)return;advancedState.InventoryRules.Add(new InventoryRule{Match=item.Vnum.ToString(CultureInfo.InvariantCulture),Action="Kullan",Threshold=1,IntervalSeconds=5,Enabled=true});BindAdvancedLists();SaveAdvancedState();RecordActivity("Pack",item+" envanter kuralına eklendi.");}
    private async Task UseSelectedPackItemAsync()
    {
        var item=packItemCombo?.SelectedItem as PackItemRecord;if(item==null)return;var live=visibleItems.FirstOrDefault(i=>i.Id==item.Vnum);if(live==null){packCatalogStatus.Text=item+" bağlı karakterin okunan envanterinde yok. Önce Beceri / Envanter sayfasında “Oyundan oku” kullanın.";return;}
        try{await GamePythonAsync("import net\nnet.SendItemUsePacket("+live.Slot+")\nf.write('ok')",CancellationToken.None);RecordActivity("Pack",item+" bir kez kullanıldı.");}catch(Exception ex){packCatalogStatus.Text=ex.Message;}
    }
    private async Task UsePackSkillAsync(){try{await GamePythonAsync("import player\nplayer.ClickSkillSlot("+((int)packSkillSlot.Value)+")\nf.write('ok')",CancellationToken.None);RecordActivity("Pack","Beceri yuvası kullanıldı · "+packSkillSlot.Value);}catch(Exception ex){packCatalogStatus.Text=ex.Message;}}
}
