using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private CheckBox sessionAuto, autoLogin, autoEnter, autoRevive, autoCloak;
    private CheckBox skillMountCycleCheck;
    private string MountSkillStatePath { get { return Path.Combine(runtimeFolder,"mount-skill.txt"); } }
    private NumericUpDown cloakInterval, skillSeconds, itemSeconds;
    private readonly Dictionary<uint,int> selectedSkills=new Dictionary<uint,int>();
    private readonly Dictionary<uint,int> scheduledItems = new Dictionary<uint,int>();
    private bool loadingAutomationSelections;private AutomationSelectionProfile dungeonBaseSettings;
    private QueuedPythonBridge sessionBridge;
    private bool sessionDirty=true, sessionPulseBusy;
    private DateTime sessionQueuedAt, sessionRetryAfter;
    private readonly string runtimeFolder=Path.Combine(Path.GetTempPath(),"Onur",Guid.NewGuid().ToString("N"));
    private string LeasePath { get { return Path.Combine(runtimeFolder,"lease.txt"); } }
    private string NpcPath { get { return Path.Combine(runtimeFolder,"npcs.txt"); } }
    private string VisualPath { get { return Path.Combine(runtimeFolder,"visuals.txt"); } }
    private string HybridTargetPath { get { return Path.Combine(runtimeFolder,"hybrid-targets.txt"); } }
    private string WarpPath { get { return Path.Combine(runtimeFolder,"warp.txt"); } }
    private string GroundPath { get { return Path.Combine(runtimeFolder,"ground.txt"); } }
    private string UserInputPath { get { return Path.Combine(runtimeFolder,"input.txt"); } }
    private string NpcVidList() { return string.Join(",",ReadNpcCandidateVids()); }
    private string HybridTargetVidList()
    {
        try
        {
            if(automation!=null&&automation.FocusTargetVid>0)return automation.FocusTargetVid.ToString(CultureInfo.InvariantCulture);
            if(combatFocus.Target!=null)return combatFocus.Target.Property_03C9.ToString(CultureInfo.InvariantCulture);
            return string.Join(",",ReadRadarEntities().Where(e=>(dmgStoneCheck.Checked&&e.Property_03CA==AddressCatalog.Field_035B)||
                (dmgMonsterCheck.Checked&&e.Property_03CA==AddressCatalog.Field_035C)||(dmgPlayerCheck.Checked&&e.Property_03CA==AddressCatalog.Field_035A))
                .OrderBy(e=>e.Property_03C9==approachTargetId?0:1).ThenBy(e=>e.Distance).Select(e=>e.Property_03C9).Where(v=>v>0).Distinct().Take(512));
        }
        catch{return "";}
    }
    private string StatusPath { get { return Path.Combine(runtimeFolder,"status.txt"); } }
    private string DungeonNpcPath { get { return Path.Combine(runtimeFolder,"dungeon-npc.txt"); } }
    private string LifeStatePath { get { return Path.Combine(runtimeFolder,"life-state.txt"); } }
    private string RetreatGuardPath { get { return Path.Combine(runtimeFolder,"retreat-guard.txt"); } }
    private string DungeonStatePath { get { return Path.Combine(runtimeFolder,"dungeon-state.txt"); } }

    private void InitializeSessionAutomation()
    {
        var page=mainTabs.TabPages[4];
        foreach(Control c in page.Controls)if(c.Top>=524)c.Visible=false;
        sessionStatus.SetBounds(24,418,610,42);
        sessionAuto=SessionOption(page,"Seçili otomasyonları çalıştır",24,472,600);
        autoLogin=SessionOption(page,"Kopunca hesapla giriş",24,510,300);
        autoEnter=SessionOption(page,"Karakterle oyuna gir",340,510,290);
        autoRevive=SessionOption(page,"Ölünce burada canlan",24,548,300);
        autoCloak=SessionOption(page,"Cesaret pelerini",340,548,210);
        cloakInterval=new NumericUpDown { Left=550,Top=550,Width=78,Minimum=1,Maximum=600,Value=10,BackColor=StudioBackground,ForeColor=StudioText };
        page.Controls.Add(cloakInterval);cloakInterval.ValueChanged+=(s,e)=>{sessionDirty=true;SaveAutomationSelections();};
        AddCaption(page,"Pelerin aralığı saniyedir. Becerileri/eşyaları Envanter sekmesinden seç.",24,590,610,26,9f,StudioMuted);
        skillMountCycleCheck=SessionOption(page,"Oto beceri: binekten in → beceriyi kullan → tekrar bin",24,626,610);
        skillMountCycleCheck.Checked=true;
        page.AutoScrollMinSize=new Size(0,680);
        var skillButton=new Button { Name="skillScheduleButton",Text="Seçili beceriyi çalıştır / çıkar",Left=20,Top=342,Width=220,Height=34,FlatStyle=FlatStyle.Flat,ForeColor=StudioAccent };
        skillButton.Click+=(s,e)=> {
            var skill=skillGrid.CurrentRow?.Tag as DetectedSkill;
            if(skill==null)return;
            bool adding=!selectedSkills.ContainsKey(skill.Id);
            if(adding){selectedSkills[skill.Id]=(int)skillSeconds.Value;sessionAuto.Checked=true;}else selectedSkills.Remove(skill.Id);
            sessionDirty=true;MarkScheduledRows();SaveAutomationSelections();inventoryStatus.Text=skill.Name+(adding?" · "+skillSeconds.Value+" saniyede bir denenecek · otomasyon açıldı.":" otomatikten çıkarıldı.");
        };
        skillSeconds=new NumericUpDown{Name="skillSeconds",Left=360,Top=346,Width=90,Minimum=1,Maximum=86400,Value=2,BackColor=StudioBackground,ForeColor=StudioText};
        skillSeconds.ValueChanged+=(s,e)=>{var skill=skillGrid.CurrentRow?.Tag as DetectedSkill;if(skill!=null&&selectedSkills.ContainsKey(skill.Id)){selectedSkills[skill.Id]=(int)skillSeconds.Value;sessionDirty=true;SaveAutomationSelections();inventoryStatus.Text=skill.Name+" · yeni aralık "+skillSeconds.Value+" saniye.";}};

        AddCaption(inventoryPage,"beceri saniyesi",462,347,170,25,9f,StudioMuted);
        var itemButton=new Button { Name="itemScheduleButton",Text="Seçili eşyayı kullan / çıkar",Left=20,Top=615,Width=220,Height=34,FlatStyle=FlatStyle.Flat,ForeColor=StudioAccent };
        itemButton.Click+=(s,e)=> {
            var item=inventoryGrid.CurrentRow?.Tag as DetectedItem;if(item==null)return;
            if(item.Slot<0) { inventoryStatus.Text="Geçerli bir envanter eşyası seçin.";return; }
            bool adding=!scheduledItems.ContainsKey(item.Id);
            if(adding){scheduledItems[item.Id]=(int)itemSeconds.Value;sessionAuto.Checked=true;}else scheduledItems.Remove(item.Id);
            sessionDirty=true;MarkScheduledRows();SaveAutomationSelections();inventoryStatus.Text=item.Name+" · ID "+item.Id+(adding?" · "+itemSeconds.Value+" saniyede bir kullanılacak · otomasyon açıldı.":" · zamanlamadan çıkarıldı.");
        };
        itemSeconds=new NumericUpDown{Name="itemSeconds",Left=360,Top=619,Width=90,Minimum=1,Maximum=86400,Value=300,BackColor=StudioBackground,ForeColor=StudioText};
        itemSeconds.ValueChanged+=(s,e)=>{var item=inventoryGrid.CurrentRow?.Tag as DetectedItem;if(item!=null&&scheduledItems.ContainsKey(item.Id)){scheduledItems[item.Id]=(int)itemSeconds.Value;sessionAuto.Checked=true;sessionDirty=true;SaveAutomationSelections();inventoryStatus.Text=item.Name+" · ID "+item.Id+" · yeni aralık "+itemSeconds.Value+" saniye.";}};

        inventoryAutomationStatus=new Label{Name="inventoryAutomationStatus",Text="Gerçek kullanım durumu burada gösterilir.",Left=20,Top=665,Width=630,Height=52,ForeColor=StudioAccent};
        var saveSelections=new Button{Name="saveAutomationSelections",Text="Seçimleri kaydet",Left=20,Top=730,Width=240,Height=34,FlatStyle=FlatStyle.Flat,ForeColor=StudioAccent};
        saveSelections.Click+=(s,e)=>{if(SaveAutomationSelections())inventoryAutomationStatus.Text="Envanter, beceri ve oturum seçimleri kaydedildi.";};
        inventoryPage.Controls.AddRange(new Control[]{skillButton,skillSeconds,itemButton,itemSeconds,saveSelections,inventoryAutomationStatus});
        skillGrid.CellDoubleClick+=(s,e)=>{if(e.RowIndex>=0)skillButton.PerformClick();};
        inventoryGrid.CellDoubleClick+=(s,e)=>{if(e.RowIndex>=0)itemButton.PerformClick();};
        AddCaption(inventoryPage,"eşya saniyesi",462,620,170,25,9f,StudioMuted);
        var timer=new System.Windows.Forms.Timer(components){Interval=1000};timer.Tick+=async(s,e)=>await SessionPulseAsync();timer.Start();
        FormClosed+=(s,e)=>SaveAutomationSelections();

    }
    private CheckBox SessionOption(TabPage page,string text,int left,int top,int width)
    {
        var c=new CheckBox { Text=text,Left=left,Top=top,Width=width,Height=28,ForeColor=StudioText };page.Controls.Add(c);
        c.CheckedChanged+=(s,e)=> { sessionDirty=true;if(sessionAuto!=null&&!sessionAuto.Checked)StopSessionLease();SaveAutomationSelections(); };return c;
    }
    private string AutomationSelectionPath()
    {
        string exe=gameExePath?.Text??"";string identity=exe.Trim().ToLowerInvariant()+"|"+(sessionProfile?.Username??"").Trim().ToLowerInvariant()+"|"+(sessionProfile?.CharacterName??"").Trim().ToLowerInvariant();
        using(var sha=SHA256.Create()){string key=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(identity))).Replace("-","").Substring(0,24);string folder=Environment.GetEnvironmentVariable("ONUR_AUTOMATION_SETTINGS_DIR");if(string.IsNullOrWhiteSpace(folder))folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","otomasyonlar");return Path.Combine(folder,key+".xml");}
    }
    private static readonly string[] SavedRuntimeControls={"attackDelayBar","attackRangeBar","pickDelayBar","fastTimingCheck","attackSpeedCheck","attackSpeedValue","moveSpeedCheck","moveSpeedValue","rangePickupCheck","rangePickupInterval","cameraDistanceCheck","cameraDistanceValue","hidePetsCheck","hideMountsCheck","hideEffectsCheck","hideShineCheck","hybridWaitCheck","hybridWaitInterval","approachOnlyCheck","approachDistance","searchWhenEmptyCheck","searchRange","walkMetinCheck","walkCanavarCheck","dmgStoneCheck","dmgMonsterCheck","dmgPlayerCheck","waitDmgRangeCheck","waitDmgCheck","targetDmgRangeCheck","targetDmgCheck","waitArrowCheck","targetArrowCheck","autoPickCheck"};
    private AutomationSelectionProfile CaptureAutomationSelections()
    {
        var p=new AutomationSelectionProfile{Skills=selectedSkills.Select(x=>new IntervalEntry{Id=x.Key,Seconds=x.Value}).ToList(),Items=scheduledItems.Select(x=>new IntervalEntry{Id=x.Key,Seconds=x.Value}).ToList(),SessionEnabled=sessionAuto.Checked,AutoLogin=autoLogin.Checked,AutoEnter=autoEnter.Checked,AutoRevive=autoRevive.Checked,AutoCloak=autoCloak.Checked,RestartGame=gameRestartCheck==null||gameRestartCheck.Checked,AutoCloseShaman=autoCloseShaman,CloakSeconds=(int)cloakInterval.Value,SkillSeconds=(int)skillSeconds.Value,ItemSeconds=(int)itemSeconds.Value,TurboEnabled=TurboAttackEnabled,AreaSelected=farmAreaSelected,AreaMap=farmAreaMapName??"",AreaX=farmAreaCenterMap.X,AreaY=farmAreaCenterMap.Y,AreaRadius=farmAreaRadiusMap};
        foreach(string name in SavedRuntimeControls)
        {
            var c=Controls.Find(name,true).FirstOrDefault();string value=null;
            if(c is Guna.UI2.WinForms.Guna2CheckBox gc)value=gc.Checked.ToString();else if(c is CheckBox cb)value=cb.Checked.ToString();
            else if(c is NumericUpDown number)value=number.Value.ToString(CultureInfo.InvariantCulture);else if(c is Guna.UI2.WinForms.Guna2TrackBar bar)value=(name=="attackDelayBar"?SavedNormalAttackDelay:bar.Value).ToString(CultureInfo.InvariantCulture);
            if(value!=null)p.Controls.Add(new RuntimeControlSetting{Name=name,Value=value});
        }
        p.SkillMountCycle=skillMountCycleCheck==null||skillMountCycleCheck.Checked;
        return p;
    }
    private void ApplyAutomationSelections(AutomationSelectionProfile p)
    {
        if(p==null)return;loadingAutomationSelections=true;
        try
        {
            selectedSkills.Clear();foreach(var x in p.Skills??new List<IntervalEntry>())selectedSkills[x.Id]=Math.Max(1,x.Seconds);
            scheduledItems.Clear();foreach(var x in p.Items??new List<IntervalEntry>())scheduledItems[x.Id]=Math.Max(1,x.Seconds);
            SetTurboAttackMode(false);
            foreach(var setting in p.Controls??new List<RuntimeControlSetting>())
            {
                if(!SavedRuntimeControls.Contains(setting.Name))continue;var c=Controls.Find(setting.Name,true).FirstOrDefault();bool enabled;decimal value;
                if(c is Guna.UI2.WinForms.Guna2CheckBox gc&&bool.TryParse(setting.Value,out enabled))gc.Checked=enabled;
                else if(c is CheckBox cb&&bool.TryParse(setting.Value,out enabled))cb.Checked=enabled;
                else if(c is NumericUpDown number&&decimal.TryParse(setting.Value,NumberStyles.Number,CultureInfo.InvariantCulture,out value))number.Value=Math.Max(number.Minimum,Math.Min(number.Maximum,value));
                else if(c is Guna.UI2.WinForms.Guna2TrackBar bar&&decimal.TryParse(setting.Value,NumberStyles.Integer,CultureInfo.InvariantCulture,out value))bar.Value=(int)Math.Max(bar.Minimum,Math.Min(bar.Maximum,value));
            }
            SetTurboAttackMode(p.TurboEnabled);
            autoCloseShaman=p.AutoCloseShaman;UpdateShamanCloseToggle();
            if(skillMountCycleCheck!=null)skillMountCycleCheck.Checked=p.SkillMountCycle;
            sessionAuto.Checked=p.SessionEnabled;autoLogin.Checked=p.AutoLogin;autoEnter.Checked=p.AutoEnter;autoRevive.Checked=p.AutoRevive;autoCloak.Checked=p.AutoCloak;
            cloakInterval.Value=Math.Max(cloakInterval.Minimum,Math.Min(cloakInterval.Maximum,p.CloakSeconds));skillSeconds.Value=Math.Max(skillSeconds.Minimum,Math.Min(skillSeconds.Maximum,p.SkillSeconds));itemSeconds.Value=Math.Max(itemSeconds.Minimum,Math.Min(itemSeconds.Maximum,p.ItemSeconds));
            farmAreaSelected=p.AreaSelected&&p.AreaRadius>0;farmAreaPinnedByMapSelection=farmAreaSelected;farmAreaMapName=p.AreaMap;farmAreaCenterMap=new System.Drawing.PointF(p.AreaX,p.AreaY);farmAreaRadiusMap=p.AreaRadius;
            if(gameRestartCheck!=null)gameRestartCheck.Checked=p.RestartGame;MarkScheduledRows();sessionDirty=true;
        }
        finally{loadingAutomationSelections=false;}
    }
    private bool SaveAutomationSelections()
    {
        if(loadingAutomationSelections||sessionAuto==null||skillSeconds==null||itemSeconds==null)return false;
        try
        {
            var p=CaptureAutomationSelections();
            if(taskDungeonRunning&&dungeonBaseSettings!=null){p.Controls=dungeonBaseSettings.Controls;p.TurboEnabled=dungeonBaseSettings.TurboEnabled;p.AreaSelected=dungeonBaseSettings.AreaSelected;p.AreaMap=dungeonBaseSettings.AreaMap;p.AreaX=dungeonBaseSettings.AreaX;p.AreaY=dungeonBaseSettings.AreaY;p.AreaRadius=dungeonBaseSettings.AreaRadius;}
            AdvancedStore.Save(AutomationSelectionPath(),p);return true;
        }
        catch(Exception ex){if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text="Seçimler kaydedilemedi: "+ex.Message;return false;}
    }
    private void LoadAutomationSelections()
    {
        if(sessionAuto==null||skillSeconds==null||itemSeconds==null)return;string path=AutomationSelectionPath();if(!File.Exists(path))return;
        try
        {
            ApplyAutomationSelections(AdvancedStore.Load<AutomationSelectionProfile>(path));if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text="Kayıtlı envanter, beceri ve bot ayarları yüklendi.";
        }
        catch(Exception ex){if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text="Kayıtlı seçimler yüklenemedi: "+ex.Message;}
        finally{loadingAutomationSelections=false;}
    }
    private void MarkScheduledRows()
    {
        foreach(DataGridViewRow row in skillGrid.Rows) { var value=row.Tag as DetectedSkill;if(value!=null)row.DefaultCellStyle.ForeColor=selectedSkills.ContainsKey(value.Id)?StudioAccent:StudioText; }
        foreach(DataGridViewRow row in inventoryGrid.Rows) { var value=row.Tag as DetectedItem;if(value!=null)row.DefaultCellStyle.ForeColor=scheduledItems.ContainsKey(value.Id)?StudioAccent:StudioText; }
    }
    private void StopSessionLease()
    {
        try { if(Directory.Exists(runtimeFolder))File.WriteAllText(LeasePath,"0"); } catch { }
        sessionBridge?.Cancel();sessionDirty=true;
    }
    private async Task SessionPulseAsync()
    {
        if(emergencyLatched){StopSessionLease();return;}
        if(sessionPulseBusy || IsDisposed || Disposing)return;
        sessionPulseBusy=true;
        try
        {
            var currentProcess=memory.Field_0197?.Property_0268;
            bool currentAlive=false;
            try { currentAlive=currentProcess!=null && !currentProcess.HasExited; } catch { }
            if(sessionBridge!=null && (!currentAlive || sessionBridge.ProcessId!=currentProcess.Id)) { sessionBridge.Dispose();sessionBridge=null;sessionDirty=true; }
            if(sessionBridge!=null && sessionBridge.Pending)
            {
                if(!sessionBridge.Poll()) {
                    if(DateTime.UtcNow-sessionQueuedAt>TimeSpan.FromSeconds(12)) { StopSessionLease();sessionBridge.Dispose();sessionBridge=null;sessionDirty=true;sessionRetryAfter=DateTime.UtcNow.AddSeconds(1);sessionStatus.Text="Oyun istemcisi yanıt vermedi; bağlantı köprüsü yeniden kuruluyor…";connectionStatus.Text="İstemci köprüsü yeniden kuruluyor…"; }
                    return;
                }
            }
            if((!sessionAuto.Checked && !EnhancementActive && targetHealthTimer==null) || !connectionRequested || !(clientConnected || LoginAssistActive)) { StopSessionLease();return; }
            var process=memory.Field_0197?.Property_0268;
            if(process==null || process.HasExited){StopSessionLease();return;}
            if(DateTime.UtcNow<sessionRetryAfter)return;
            if(sessionBridge==null || sessionBridge.ProcessId!=process.Id)
            {
                sessionBridge?.Dispose();sessionBridge=null;
                var newBridge=new QueuedPythonBridge(process);
                if(IsDisposed || Disposing || (!sessionAuto.Checked && !EnhancementActive && targetHealthTimer==null) || !connectionRequested || memory.Field_0197?.Property_0268?.Id!=process.Id){newBridge.Dispose();return;}
                sessionBridge=newBridge;sessionDirty=true;
            }
            Directory.CreateDirectory(runtimeFolder);
            double expiry=(DateTime.UtcNow.AddSeconds(15)-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds;
            File.WriteAllText(LeasePath,expiry.ToString("R",CultureInfo.InvariantCulture));
            if(taskDungeonRunning) { try { File.WriteAllText(NpcPath,NpcVidList()); } catch { } }
            if(VisualSettingsActive) { try { File.WriteAllText(VisualPath,string.Join(",",ReadVisualCandidateVids())); } catch { } }
            PublishHybridAttackIntent();UpdateHybridWaitStatus();
            if(sessionDirty)
            {
                string script;
                using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.SessionAgent.py"))using(var reader=new StreamReader(stream))script=reader.ReadToEnd();
                using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.MountSkillCycle.py"))using(var reader=new StreamReader(stream))script=reader.ReadToEnd()+"\n"+script;
                using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.NormalAttackHold.py"))using(var reader=new StreamReader(stream))script=reader.ReadToEnd()+"\n"+script;
                using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.RouteMotion.py"))using(var reader=new StreamReader(stream))script=reader.ReadToEnd()+"\n"+script;
                script=script.Replace("__ONUR_CONFIG__",BuildSessionConfig());
                sessionBridge.Queue(script);sessionQueuedAt=DateTime.UtcNow;sessionDirty=false;
                sessionStatus.Text="Otomasyon oyunun güncelleme döngüsüne ekleniyor…";
                if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text="Seçimler oyuna aktarılıyor…";
            }
            else if(File.Exists(StatusPath))
            {
                string status=File.ReadAllText(StatusPath);sessionStatus.Text=status;
                if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text="Gerçek durum: "+status;
                if(status.StartsWith("Kurulum yeniden")) { sessionDirty=true;sessionRetryAfter=DateTime.UtcNow.AddSeconds(5); }
            }
        }
        catch(Exception ex)
        {
            StopSessionLease();sessionRetryAfter=DateTime.UtcNow.AddSeconds(5);
            if(!IsDisposed&&!Disposing){sessionStatus.Text="Otomasyon yeniden denenecek: "+ex.Message;if(inventoryAutomationStatus!=null)inventoryAutomationStatus.Text=sessionStatus.Text;}
        }
        finally { sessionPulseBusy=false; }
    }
    public static string PythonLiteral(string value)
    {
        var s=new StringBuilder("'");
        for(int i=0;i<(value??"").Length;i++) {
            int cp=char.ConvertToUtf32(value,i);if(cp>65535)i++;
            if(cp>=32&&cp<=126&&cp!=39&&cp!=92)s.Append((char)cp);
            else s.Append(cp<=65535?"\\u"+cp.ToString("x4"):"\\U"+cp.ToString("x8"));
        }
        return s.Append("'").ToString();
    }
    private string BuildSessionConfig()
    {
        bool master=sessionAuto.Checked;
        bool routeRangePickup=RouteOwnsCombat&&(activeRouteOptions?.RangePickup??false);
        bool allowAttackSpeedOverride=attackSpeedCheck.Checked&&!taskDungeonRunning;
        decimal effectiveAttackSpeed=attackSpeedValue.Value;
        string entityVids=NpcVidList();var stageSetting=CurrentDungeonStageSetting(taskUiStage);
        string petVnums=visualCatalog==null?"":string.Join(",",visualCatalog.PetVnums.OrderBy(v=>v));
        string mountVnums=visualCatalog==null?"":string.Join(",",visualCatalog.MountVnums.OrderBy(v=>v));
        return "{'pack_ai':"+(packEngineRunning&&packEngineOwnsAi?"True":"False")+",'pack_stone_attack':"+(packEngineRunning&&!packEngineOwnsAi?"True":"False")+",'pack_ai_type':"+packOwnedAiType+",'pack_ai_vnums':["+string.Join(",",packOwnedVnums??new int[0])+"],'skill_mount':"+(skillMountCycleCheck==null||skillMountCycleCheck.Checked?"True":"False")+",'mount_skill_state':"+PythonLiteral(MountSkillStatePath)+",'username':"+PythonLiteral(sessionProfile.Username)+",'password':"+PythonLiteral(sessionProfile.Password)+",'character':"+PythonLiteral(sessionProfile.CharacterName)+
            ",'login':"+(master&&autoLogin.Checked?"True":"False")+",'enter':"+(master&&autoEnter.Checked?"True":"False")+",'revive':"+(master&&autoRevive.Checked?"True":"False")+",'route_revive':"+PythonLiteral(RouteIntentPath)+",'cloak':"+(master&&autoCloak.Checked?"True":"False")+
",'cloak_interval':"+cloakInterval.Value.ToString(CultureInfo.InvariantCulture)+",'skill_order':["+(master?string.Join(",",ActiveSkillOrder()):"")+"],'skills':{"+(master?string.Join(",",selectedSkills.Select(p=>p.Key+":"+p.Value)):"")+"},'items':{"+(master?string.Join(",",scheduledItems.Select(p=>p.Key+":"+p.Value)):"")+"}"+
            ",'range_pickup':"+(rangePickupCheck.Checked||routeRangePickup?"True":"False")+",'route_range_pickup':"+(routeRangePickup?"True":"False")+",'range_pickup_interval':"+rangePickupInterval.Value.ToString(CultureInfo.InvariantCulture)+
            ",'attack_speed':"+(allowAttackSpeedOverride?"True":"False")+",'attack_speed_value':"+effectiveAttackSpeed.ToString(CultureInfo.InvariantCulture)+
            ",'move_speed':"+(moveSpeedCheck.Checked?"True":"False")+",'move_speed_value':"+moveSpeedValue.Value.ToString(CultureInfo.InvariantCulture)+
            ",'camera_distance':"+(cameraDistanceCheck.Checked?"True":"False")+",'camera_distance_value':"+cameraDistanceValue.Value.ToString(CultureInfo.InvariantCulture)+
            ",'hide_pets':"+(hidePetsCheck.Checked?"True":"False")+",'hide_mounts':"+(hideMountsCheck.Checked?"True":"False")+",'hide_effects':"+(hideEffectsCheck.Checked?"True":"False")+",'hide_shine':"+(hideShineCheck.Checked?"True":"False")+
            ",'pet_vnums':["+petVnums+"],'mount_vnums':["+mountVnums+"],'visuals':"+PythonLiteral(VisualPath)+
            ",'hybrid_attack':"+(HybridWaitRequested&&!RouteUsesAttackQueue?"True":"False")+",'hybrid_close':"+HybridCloseDistance+",'hybrid_types':["+AddressCatalog.Field_035B+","+AddressCatalog.Field_035C+"],'hybrid_attack_interval':"+hybridWaitInterval.Value.ToString(CultureInfo.InvariantCulture)+",'hybrid_targets':"+PythonLiteral(HybridTargetPath)+
            ",'close_shaman':"+(autoCloseShaman?"True":"False")+",'dungeon_auto':"+(taskDungeonRunning?"True":"False")+",'dungeon_stage':"+taskDungeonStage+",'dungeon_ui_stage':"+taskUiStage+",'dungeon_npc':"+PythonLiteral(ConfiguredDungeonNpcName(taskUiStage))+",'dungeon_item_id':"+(stageSetting?.RequiredItemId??0)+",'entity_vids':["+entityVids+"]"+
            ",'combat_focus_request':"+PythonLiteral(CombatFocusRequestPath)+",'combat_focus_state':"+PythonLiteral(CombatFocusStatePath)+
            ",'route_motion':"+PythonLiteral(RouteMotionPath)+",'route_motion_status':"+PythonLiteral(RouteMotionStatusPath)+
            ",'target_health':"+PythonLiteral(TargetHealthPath)+",'emergency_stop':"+PythonLiteral(EmergencyPath)+
            ",'life_state':"+PythonLiteral(LifeStatePath)+",'retreat_guard':"+PythonLiteral(RetreatGuardPath)+",'client_pid':"+(memory.Field_0197?.Property_0268?.Id??0)+
            ",'npcs':"+PythonLiteral(NpcPath)+",'warp':"+PythonLiteral(WarpPath)+",'mapinfo':"+PythonLiteral(MapInfoPath)+",'input':"+PythonLiteral(UserInputPath)+",'ground':"+PythonLiteral(GroundPath)+",'lease':"+PythonLiteral(LeasePath)+",'status':"+PythonLiteral(StatusPath)+",'dungeon_state':"+PythonLiteral(DungeonStatePath)+",'dungeon_npc_state':"+PythonLiteral(DungeonNpcPath)+"}";
    }
}










