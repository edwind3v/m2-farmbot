using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    private TabControl attackSections;
    private TabPage attackGeneralPage,combatToolsPage;
    private CheckBox damageWatchCheck,allowRulesCheck;
    private NumericUpDown damageWatchSeconds,routeAttackMilliseconds;
    private ComboBox damageWatchAction;
    private Label combatWatchStatus,combatTestStatus;
    private Button combatRecordButton;
    private readonly DamageProgressWatch damageWatch=new DamageProgressWatch();
    private readonly List<string> combatTestRows=new List<string>();
    private bool combatRecording,rulesSafetyPaused;
    private int combatTestPid;
    private string combatTestMap;
    private long combatTestStarted,combatLastPulse,combatInitialCalls,combatLastPositionAt;
    private PointF combatLastPosition;
    private double combatPreviousHpAt;
    private float? combatPreviousHp;
    private int combatPreviousVid;
    private double combatObservedDrop;
    private void InitializeV10Workspace()
    {
        InitializeEmergencyStop();
        // Keep real controls in the main form tree: saved profiles and their existing event handlers still work.
        attackGeneralPage=new TabPage("Saldırı ayarları"){AutoScroll=true,BackColor=tabDamage.BackColor,ForeColor=tabDamage.ForeColor};
        foreach(Control c in tabDamage.Controls.Cast<Control>().ToArray())attackGeneralPage.Controls.Add(c);
        attackSections=new TabControl{Name="attackSections",Dock=DockStyle.Fill};tabDamage.Controls.Add(attackSections);attackSections.TabPages.Add(attackGeneralPage);
        mainTabs.TabPages.Remove(developerPage);developerPage.Text="İstemci eşleme";attackSections.TabPages.Add(developerPage);
        combatToolsPage=new TabPage("İzleme ve test"){AutoScroll=true,BackColor=tabDamage.BackColor,ForeColor=tabDamage.ForeColor};attackSections.TabPages.Add(combatToolsPage);
        BuildCombatTools();InitializeRoutePreflightUi();ReorganizeFeatureMenus();
        var timer=new System.Windows.Forms.Timer(components){Interval=200};timer.Tick+=(s,e)=>CombatObservationTick();timer.Start();
        mainTabs.SelectedIndexChanged+=(s,e)=>UpdateContextActionButton();UpdateContextActionButton();
    }
    private void BuildCombatTools()
    {
        var p=combatToolsPage;
        Action<string,int,int> text=(s,y,h)=>p.Controls.Add(new Label{Text=s,Left=16,Top=y,Width=480,Height=h,ForeColor=p.ForeColor});
        text("HASAR İLERLEMESİ · Seçili hedefin taze can bilgisini izler",16,32);
        damageWatchCheck=new CheckBox{Name="damageWatchCheck",Text="Saldırı açıkken can değişmiyorsa uyar",Left=16,Top=54,Width=460,Checked=true};
        damageWatchSeconds=new NumericUpDown{Name="damageWatchSeconds",Left=16,Top=94,Width=90,Minimum=3,Maximum=120,Value=10};
        damageWatchAction=new ComboBox{Name="damageWatchAction",Left=125,Top=94,Width=300,DropDownStyle=ComboBoxStyle.DropDownList};damageWatchAction.Items.AddRange(new object[]{"saniye sonra yalnızca uyar","saniye sonra tümünü durdur"});damageWatchAction.SelectedIndex=0;
        combatWatchStatus=new Label{Name="combatWatchStatus",Left=16,Top=132,Width=480,Height=60,Text="Saldırı ve doğrulanmış can verisi bekleniyor."};
        p.Controls.AddRange(new Control[]{damageWatchCheck,damageWatchSeconds,damageWatchAction,combatWatchStatus});
        text("ROTAYA ÖZEL SALDIRI ARALIĞI (ms)",204,26);
        var routeSettings=new Button{Name="openRouteCombatSettings",Left=16,Top=239,Width=255,Height=32,Text="Rota > Saldırı ve geçiş ayarları"};
        routeSettings.Click+=(s,e)=>{mainTabs.SelectedTab=tabRoute;tabRoute.ScrollControlIntoView(routeCombatPanel);};
        p.Controls.Add(routeSettings);text("Karma, yakın/uzak Wait, ms, menzil ve engel geçişleri Rota bölümündedir.\nAyarlar rota profiliyle birlikte saklanır.",278,48);
        text("GÜVENLİ TEST KAYDI · Saldırıyı veya hızı kendiliğinden değiştirmez",338,34);
        combatRecordButton=new Button{Name="combatRecordButton",Text="Ölçümü başlat",Left=16,Top=378,Width=145,Height=32};combatRecordButton.Click+=(s,e)=>{if(combatRecording)StopCombatRecording("Kullanıcı bitirdi.");else StartCombatRecording();};
        var export=new Button{Name="combatExportButton",Text="CSV dışa aktar",Left=174,Top=378,Width=145,Height=32};export.Click+=(s,e)=>ExportCombatRecording();
        combatTestStatus=new Label{Name="combatTestStatus",Left=16,Top=424,Width=480,Height=115,Text="Süre, seçili hedef canı, komut tamamlanması, motor/UI döngüsü ve konum değişimi aralığı kaydedilir.\nKomut tamamlanması gerçek vuruş sayısı değildir. Can düşüşü başka saldırılardan da kaynaklanabilir."};
        p.Controls.AddRange(new Control[]{combatRecordButton,export,combatTestStatus});
        allowRulesCheck=new CheckBox{Name="allowRulesCheck",Left=16,Top=550,Width=480,Text="Kontrol Merkezi otomasyon kurallarına izin ver",Checked=true};allowRulesCheck.CheckedChanged+=(s,e)=>rulesSafetyPaused=!allowRulesCheck.Checked;p.Controls.Add(allowRulesCheck);
        text("Acil durdurma bu izni kapatır. Kilidi açmak kuralları yeniden açmaz.\nKayıt en fazla 60 dakika sürer; kopma veya harita değişiminde biter.",589,48);
        p.AutoScrollMinSize=new Size(0,650);
    }
    private void ShowCombatTools(){mainTabs.SelectedTab=tabDamage;attackSections.SelectedTab=combatToolsPage;}
    private void SetAttackEditorsEnabled(bool enabled){if(attackGeneralPage==null){tabDamage.Enabled=enabled;return;}attackGeneralPage.Enabled=developerPage.Enabled=enabled;routeAttackMilliseconds.Enabled=enabled;}
    private void MoveMirroredOption(Control box,Control destination,int x,int y)
    {
        var old=box.Parent;var peers=old.Controls.Cast<Control>().Where(c=>ReferenceEquals(c.Tag,box)).ToArray();int dx=x-box.Left,dy=y-box.Top;
        destination.Controls.Add(box);box.Location=new Point(x,y);
        foreach(var c in peers){destination.Controls.Add(c);c.Location=new Point(c.Left+dx,c.Top+dy);}
    }
    private void HideMirroredOption(Control box){foreach(Control c in box.Parent.Controls)if(ReferenceEquals(c.Tag,box))c.Visible=false;box.Visible=false;}
    private void ReorganizeFeatureMenus()
    {
        int top=tabMain.Controls.Cast<Control>().Max(c=>c.Bottom)+20;
        tabMain.Controls.Add(new Label{Text="YÜRÜME HIZI · bağlantı doğrulanmadı; değer şu an uygulanmıyor",Left=20,Top=top,Width=475,Height=40});
        MoveMirroredOption(moveSpeedCheck,tabMain,20,top+45);tabMain.Controls.Add(moveSpeedValue);moveSpeedValue.Location=new Point(330,top+47);tabMain.AutoScrollMinSize=new Size(0,top+100);
        // Remove duplicate speed editing from visual/performance page; keep backing controls for profile compatibility.
        HideMirroredOption(performanceAttackSpeedCheck);HideMirroredOption(performanceMoveSpeedCheck);performanceAttackSpeedValue.Visible=performanceMoveSpeedValue.Visible=false;
        foreach(Control c in performancePage.Controls)
        {
            if(c.Name=="performanceBalancedSpeed"||c.Name=="performanceFastSpeed"||c is Label&&(c.Text=="300 sınırı yok"||c.Text=="50 — 300"))c.Visible=false;
            if(Convert.ToString(c.Tag)=="section-card"&&c.Controls.Cast<Control>().Any(t=>t.Text=="KARAKTER HIZLARI"))foreach(Control caption in c.Controls)if(caption is Label)caption.Text=caption.Font.Size>10?"HIZ AYARLARI TAŞINDI":"Saldırı hızı: Saldırı · Yürüme hızı: Hareket";
        }
        foreach(Control c in attackGeneralPage.Controls)if(c.Name=="attackBalancedSpeed"||c.Name=="attackFastSpeed"||c is Label&&c.Text=="50 — 300")c.Visible=false;
        int pickupTop=inventoryPage.Controls.Cast<Control>().Max(c=>c.Bottom)+18;
        inventoryPage.Controls.Add(new Label{Text="OTOMATİK EŞYA TOPLAMA",Left=20,Top=pickupTop,Width=460,Height=28});
        MoveMirroredOption(autoPickCheck,inventoryPage,20,pickupTop+34);inventoryPage.Controls.Add(pickDelayLbl);pickDelayLbl.Location=new Point(20,pickupTop+72);inventoryPage.Controls.Add(pickDelayBar);pickDelayBar.SetBounds(20,pickupTop+101,Math.Min(450,inventoryPage.ClientSize.Width-45),26);
        MoveMirroredOption(rangePickupCheck,inventoryPage,20,pickupTop+155);inventoryPage.Controls.Add(rangePickupInterval);rangePickupInterval.SetBounds(310,pickupTop+157,100,26);
        inventoryPage.Controls.Add(new Label{Text="Alan toplama tarama aralığı (ms)",Left=20,Top=pickupTop+190,Width=460,Height=26});inventoryPage.AutoScrollMinSize=new Size(0,pickupTop+240);
        foreach(Control c in performancePage.Controls)
        {
            if(c is Label&&(c.Text=="Tarama aralığı"||c.Text=="ms"))c.Visible=false;
            if(Convert.ToString(c.Tag)=="section-card"&&c.Controls.Cast<Control>().Any(t=>t.Text=="ALAN TOPLAMA"))foreach(Control caption in c.Controls)if(caption is Label)caption.Text=caption.Font.Size>10?"TOPLAMA TAŞINDI":"Otomatik eşya toplama ayarları: Envanter";
        }
        performancePage.Text="   Görsel / kamera";
        // Add explicit entry points for settings that previously hid behind the control center.
        var zindan=new Button{Name="dungeonEditorShortcut",Text="Zindan editörünü aç",Left=20,Top=taskPage.Controls.Cast<Control>().Max(c=>c.Bottom)+12,Width=230,Height=34};zindan.Click+=(s,e)=>OpenSelectedDungeonEditor();taskPage.Controls.Add(zindan);taskPage.AutoScrollMinSize=new Size(0,zindan.Bottom+15);
    }
    private void StartCombatRecording()
    {
        if(emergencyLatched||!IsClientReady()){combatTestStatus.Text="Önce bağlan ve güvenlik kilidini kaldır.";return;}
        if(combatTestRows.Count>0&&MessageBox.Show("Önceki ölçümün yerine yeni ölçüm başlatılsın mı? Gerekirse önce CSV dışa aktar.","Test kaydı",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        combatTestRows.Clear();combatTestRows.Add("UTC,Event,PID,Map,VID,HP_percent,Native_attack_interval_ms,Native_completed_commands,Motor_cycle_ms,UI_tick_ms,Position_change_interval_ms,Mode");
        combatTestPid=memory.Field_0197.Property_0268.Id;combatTestMap=atlasName;combatTestStarted=movementClock.ElapsedMilliseconds;combatInitialCalls=automation.CompletedAttackCalls;combatPreviousHp=null;combatPreviousVid=0;combatPreviousHpAt=0;combatObservedDrop=0;combatLastPosition=new PointF(TaskX,TaskY);combatLastPositionAt=combatTestStarted;
        combatRecording=true;combatRecordButton.Text="Ölçümü bitir";RecordActivity("Test","Pasif ölçüm başladı; saldırı ayarları değiştirilmedi.");
    }
    private void StopCombatRecording(string reason)
    {
        if(!combatRecording)return;combatRecording=false;combatRecordButton.Text="Ölçümü başlat";combatTestRows.Add(Csv(DateTime.UtcNow.ToString("o"))+","+Csv(reason)+",,,,,,,,,,");combatTestStatus.Text="Ölçüm bitti: "+reason+"\n"+(combatTestRows.Count-2)+" örnek · CSV dışa aktarabilirsin.";
        RecordActivity("Test",reason);
    }
    private static string Csv(string s){s=s??"";if(s.Length>0&&"=+-@".IndexOf(s[0])>=0)s="'"+s;return "\""+s.Replace("\"","\"\"")+"\"";}
    private void ExportCombatRecording()
    {
        if(combatTestRows.Count==0){combatTestStatus.Text="Henüz ölçüm yok.";return;}
        using(var d=new SaveFileDialog{Filter="CSV|*.csv",FileName="GamePilot-test-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".csv"})if(d.ShowDialog(this)==DialogResult.OK)try{File.WriteAllLines(d.FileName,combatTestRows,System.Text.Encoding.UTF8);}catch(Exception ex){MessageBox.Show(ex.Message);}
    }
    private void CombatObservationTick()
    {
        RefreshMountSkillGuards();
        if(IsDisposed||Disposing)return;
        UpdateContextActionButton();
        long now=movementClock.ElapsedMilliseconds,uiGap=combatLastPulse==0?0:now-combatLastPulse;combatLastPulse=now;
        if(emergencyLatched){damageWatch.Reset();return;}
        TargetHealthSnapshot snapshot=null;int pid=0;bool connected=false;
        try{var process=memory.Field_0197?.Property_0268;connected=clientConnected&&process!=null&&!process.HasExited;if(connected){pid=process.Id;snapshot=TargetHealthSnapshot.Parse(File.ReadAllText(TargetHealthPath),pid,atlasName,DateTime.UtcNow);}}catch{}
        bool attacking=automation.AttackEnabled||packEngineRunning;
        bool matching=snapshot!=null&&(automation.FocusTargetVid==0||automation.FocusTargetVid==snapshot.Vid);
        bool stalled=damageWatch.Observe(pid+"|"+atlasName,matching?snapshot.Vid:0,matching?snapshot.Hp:null,matching?snapshot.At:0,now,damageWatchCheck.Checked&&attacking,(int)damageWatchSeconds.Value*1000);
        if(stalled)
        {
            string reason="Hedef "+snapshot.Vid+" canı "+damageWatchSeconds.Value+" saniyedir değişmedi (ölçülen can %"+snapshot.Hp.Value.ToString("0.#")+").";combatWatchStatus.Text=reason;RecordActivity("Hasar ilerlemesi",reason);
            if(damageWatchAction.SelectedIndex==1){var ignored=EmergencyStopAsync(reason);return;}
        }
        else if(!attacking||!matching||!snapshot.Hp.HasValue)combatWatchStatus.Text=!attacking?"Saldırı kapalı; hasar süresi sayılmıyor.":"Taze/eşleşen hedef canı yok; hasar yok kararı verilmiyor.";
        else if(!damageWatch.Stalled)combatWatchStatus.Text="Hedef "+snapshot.Vid+" izleniyor · can %"+snapshot.Hp.Value.ToString("0.#")+" · eşik "+damageWatchSeconds.Value+" sn";
        if(!combatRecording)return;
        if(!connected||pid!=combatTestPid||atlasName!=combatTestMap){StopCombatRecording("Bağlantı veya harita değişti.");return;}
        if(now-combatTestStarted>=3600000||combatTestRows.Count>=18001){StopCombatRecording("Ölçüm süresi/örnek sınırı doldu.");return;}
        long positionGap=0;var position=new PointF(TaskX,TaskY);if(RoutePatrol.Distance(position.X,position.Y,combatLastPosition.X,combatLastPosition.Y)>=25){positionGap=now-combatLastPositionAt;combatLastPositionAt=now;combatLastPosition=position;}
        if(snapshot!=null&&snapshot.Hp.HasValue&&snapshot.At!=combatPreviousHpAt)
        {
            if(combatPreviousVid==snapshot.Vid&&combatPreviousHp.HasValue&&snapshot.At-combatPreviousHpAt<=1.5)combatObservedDrop+=Math.Max(0,combatPreviousHp.Value-snapshot.Hp.Value);
            combatPreviousHp=snapshot.Hp;combatPreviousVid=snapshot.Vid;combatPreviousHpAt=snapshot.At;
        }
        else if(snapshot==null||!snapshot.Hp.HasValue){combatPreviousHp=null;combatPreviousVid=0;}
        long calls=Math.Max(0,automation.CompletedAttackCalls-combatInitialCalls);
        string mode=packEngineRunning?"Pack (yerel motor sayacına dahil değil)":RouteOwnsCombat?"Rota":taskDungeonRunning?"Zindan":normalBotRequested?"Normal bot":automation.AttackEnabled?"Saldırı":"Saldırı kapalı";
        combatTestRows.Add(string.Join(",",new[]{Csv(DateTime.UtcNow.ToString("o")),Csv("sample"),pid.ToString(),Csv(atlasName),(snapshot?.Vid??0).ToString(),snapshot?.Hp?.ToString("R",CultureInfo.InvariantCulture)??"",packEngineRunning?"":automation.AttackDelay.ToString(),calls.ToString(),automation.LastCycleMilliseconds.ToString(),uiGap.ToString(),positionGap==0?"":positionGap.ToString(),Csv(mode)}));
        combatTestStatus.Text="Kayıt: "+((now-combatTestStarted)/1000)+" sn · "+(combatTestRows.Count-1)+" örnek\nGözlenen toplam can düşüşü: "+combatObservedDrop.ToString("0.##")+" yüzde puan\nTamamlanan komut: "+calls+" (gerçek vuruş sayısı değil)\nMotor döngüsü: "+automation.LastCycleMilliseconds+" ms · UI aralığı: "+uiGap+" ms";
    }
}
