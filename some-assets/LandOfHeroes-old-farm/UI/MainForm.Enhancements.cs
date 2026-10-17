using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Guna.UI2.WinForms;

public partial class MainForm
{
    private TabPage performancePage;
    private Guna2CheckBox rangePickupCheck, attackSpeedCheck, moveSpeedCheck, performanceAttackSpeedCheck, performanceMoveSpeedCheck, cameraDistanceCheck, hidePetsCheck, hideMountsCheck, hideEffectsCheck, hideShineCheck;
    private NumericUpDown rangePickupInterval, attackSpeedValue, moveSpeedValue, performanceAttackSpeedValue, performanceMoveSpeedValue, cameraDistanceValue;
    private bool syncingSpeedControls;
    private Label performanceStatus;
    private Label visualCatalogStatus;
    private Label packIntegrationStatus;
    private Button packFolderButton,packRefreshButton;
    private PackVisualCatalog visualCatalog;

    private void InitializeEnhancements()
    {
        performancePage = new TabPage("Performans") { Name="performancePage", BackColor=StudioSurface, ForeColor=StudioText, AutoScroll=true };
        mainTabs.TabPages.Add(performancePage);
        AddCaption(performancePage,"Performans ve alan kontrolü",28,26,620,36,20f,StudioText);
        AddCaption(performancePage,"Kurtarılan DLL bağlantıları, doğrulanmış istemci yöntemleriyle çalışır.",28,67,650,24,9.5f,StudioMuted);

        AddSectionCard(performancePage,"ALAN TOPLAMA","Yakındaki görünür eşyaları tek döngüde toplar.",28,112,650,142);
        rangePickupCheck=NewOption("rangePickupCheck","Çevredeki alanın hepsini topla",50,195,360);
        performancePage.Controls.Add(rangePickupCheck);
        AddCaption(performancePage,"Tarama aralığı",435,196,120,24,9f,StudioMuted);
        rangePickupInterval=NewNumber("rangePickupInterval",555,192,92,50,2000,250,50);
        performancePage.Controls.Add(rangePickupInterval);
        AddCaption(performancePage,"ms",650,196,34,24,9f,StudioMuted);

        AddSectionCard(tabDamage,"KARAKTER HIZLARI","Hız bağlantısı doğrulanmadı; hız değişikliği şu an uygulanmıyor.",24,474,620,166);
        attackSpeedCheck=NewOption("attackSpeedCheck","Saldırı hızını uygula",46,566,260);
        moveSpeedCheck=NewOption("moveSpeedCheck","Yürüme hızını uygula",46,616,260);
        tabDamage.Controls.AddRange(new Control[]{attackSpeedCheck,moveSpeedCheck});
        attackSpeedValue=NewNumber("attackSpeedValue",346,562,180,50,decimal.MaxValue,180,5);
        moveSpeedValue=NewNumber("moveSpeedValue",346,612,92,50,300,250,5);
        tabDamage.Controls.AddRange(new Control[]{attackSpeedValue,moveSpeedValue});
        AddCaption(tabDamage,"50 — 300",448,617,84,24,9f,StudioMuted);
        var balanced=NewPresetButton("Dengeli",540,558,92); balanced.Name="attackBalancedSpeed";balanced.Click+=(s,e)=>ApplySpeedPreset(170,220);
        var fast=NewPresetButton("Hızlı",540,608,92); fast.Name="attackFastSpeed";fast.Click+=(s,e)=>ApplySpeedPreset(200,260);
        tabDamage.Controls.AddRange(new Control[]{balanced,fast});

        AddSectionCard(performancePage,"KARAKTER HIZLARI","Hız bağlantısı doğrulanmadı; hız değişikliği şu an uygulanmıyor.",28,278,650,206);
        performanceAttackSpeedCheck=NewOption("performanceAttackSpeedCheck","Saldırı hızını uygula",50,365,270);
        performanceMoveSpeedCheck=NewOption("performanceMoveSpeedCheck","Yürüme hızını uygula",50,430,270);
        performanceAttackSpeedValue=NewNumber("performanceAttackSpeedValue",350,361,100,50,decimal.MaxValue,180,5);
        performanceMoveSpeedValue=NewNumber("performanceMoveSpeedValue",350,426,100,50,300,250,5);
        var performanceBalanced=NewPresetButton("Dengeli",470,359,92);performanceBalanced.Name="performanceBalancedSpeed";performanceBalanced.Click+=(s,e)=>ApplySpeedPreset(170,220);
        var performanceFast=NewPresetButton("Hızlı",470,424,92);performanceFast.Name="performanceFastSpeed";performanceFast.Click+=(s,e)=>ApplySpeedPreset(200,260);
        performancePage.Controls.AddRange(new Control[]{performanceAttackSpeedCheck,performanceMoveSpeedCheck,performanceAttackSpeedValue,performanceMoveSpeedValue,performanceBalanced,performanceFast});
        AddCaption(performancePage,"300 sınırı yok",574,366,100,24,9f,StudioMuted);AddCaption(performancePage,"50 — 300",574,431,78,24,9f,StudioMuted);

        AddSectionCard(performancePage,"KAMERA UZAKLIĞI","Mevcut kamera açısını koruyarak görüş mesafesini büyütür.",28,650,650,80);
        cameraDistanceCheck=NewOption("cameraDistanceCheck","Kamera uzaklığını uygula",50,742,300);
        cameraDistanceValue=NewNumber("cameraDistanceValue",370,738,125,1500,50000,15000,500);
        performancePage.Controls.AddRange(new Control[]{cameraDistanceCheck,cameraDistanceValue});
        AddCaption(performancePage,"1.500 — 50.000",510,743,150,24,9f,StudioMuted);
        AddSectionCard(performancePage,"GÖRSEL SADELİK","Pack kimlikleri ve istemcinin görüntü API'leriyle çalışır.",28,790,650,80);
        hidePetsCheck=NewOption("hidePetsCheck","Petleri gizle",50,882,270);
        hideMountsCheck=NewOption("hideMountsCheck","Binekleri gizle",350,882,270);
        hideEffectsCheck=NewOption("hideEffectsCheck","Gelişmiş savaş efektlerini gizle",50,927,285);
        hideShineCheck=NewOption("hideShineCheck","Zırh ve silah parlamasını gizle",350,927,290);
        visualCatalogStatus=new Label{Name="visualCatalogStatus",Text="Pack pet/binek listesi hazırlanıyor…",Location=new Point(50,974),Size=new Size(590,44),ForeColor=StudioMuted,BackColor=Color.Transparent};
        packIntegrationStatus=new Label{Name="packIntegrationStatus",Text="Pack verileri uygulamaya otomatik bağlanıyor…",Location=new Point(50,1022),Size=new Size(590,42),ForeColor=StudioMuted,BackColor=Color.Transparent};
        packFolderButton=NewPresetButton("Pack klasörünü değiştir",50,1070,210);packFolderButton.Name="packFolderButton";packFolderButton.Click+=async(s,e)=>await SelectAutomaticPackRootAsync();
        packRefreshButton=NewPresetButton("Pack verilerini yenile",280,1070,190);packRefreshButton.Name="packRefreshButton";packRefreshButton.Click+=async(s,e)=>{packCatalog=null;await EnsurePackCatalogLoadedAsync();};
        performancePage.Controls.AddRange(new Control[]{hidePetsCheck,hideMountsCheck,hideEffectsCheck,hideShineCheck,visualCatalogStatus,packIntegrationStatus,packFolderButton,packRefreshButton});
        AddSectionCard(performancePage,"ÇALIŞMA DURUMU","Etkin seçenekler oyun bağlantısı sürdüğü sürece uygulanır.",28,510,650,110);
        performanceStatus=new Label { Name="performanceStatus",Text="Hazır · tüm seçenekler kapalı",Location=new Point(50,600),Size=new Size(590,28),ForeColor=StudioMuted,BackColor=Color.Transparent };
        performancePage.Controls.Add(performanceStatus);

        EventHandler changed=(s,e)=> { sessionDirty=true;UpdatePerformanceStatus(); };
        rangePickupCheck.CheckedChanged+=changed;rangePickupInterval.ValueChanged+=changed;cameraDistanceCheck.CheckedChanged+=changed;cameraDistanceValue.ValueChanged+=changed;
        hidePetsCheck.CheckedChanged+=changed;hideMountsCheck.CheckedChanged+=changed;hideEffectsCheck.CheckedChanged+=changed;hideShineCheck.CheckedChanged+=changed;
        attackSpeedCheck.CheckedChanged+=(s,e)=>{SyncSpeedControls(true);changed(s,e);};moveSpeedCheck.CheckedChanged+=(s,e)=>{SyncSpeedControls(true);changed(s,e);};
        attackSpeedValue.ValueChanged+=(s,e)=>{SyncSpeedControls(true);changed(s,e);};moveSpeedValue.ValueChanged+=(s,e)=>{SyncSpeedControls(true);changed(s,e);};
        performanceAttackSpeedCheck.CheckedChanged+=(s,e)=>{SyncSpeedControls(false);changed(s,e);};performanceMoveSpeedCheck.CheckedChanged+=(s,e)=>{SyncSpeedControls(false);changed(s,e);};
        performanceAttackSpeedValue.ValueChanged+=(s,e)=>{SyncSpeedControls(false);changed(s,e);};performanceMoveSpeedValue.ValueChanged+=(s,e)=>{SyncSpeedControls(false);changed(s,e);};
        SyncSpeedControls(true);performancePage.AutoScrollMinSize=new Size(0,1280);
        StyleControls(performancePage);
        StyleControls(tabDamage);
        mainTabs.SelectedIndexChanged+=(s,e)=>BeginInvoke((Action)EnsureEnhancementControlsVisible);
        Shown+=(s,e)=>BeginInvoke((Action)EnsureEnhancementControlsVisible);
        EnsureEnhancementControlsVisible();
    }

    private void EnsureEnhancementControlsVisible()
    {
        foreach(Control page in new Control[]{tabDamage,performancePage})
            foreach(Control card in page.Controls)if(Convert.ToString(card.Tag)=="section-card")card.SendToBack();
        foreach(Control control in new Control[]{attackSpeedCheck,moveSpeedCheck,attackSpeedValue,moveSpeedValue,performanceAttackSpeedCheck,performanceMoveSpeedCheck,performanceAttackSpeedValue,performanceMoveSpeedValue,rangePickupCheck,rangePickupInterval,cameraDistanceCheck,cameraDistanceValue,hidePetsCheck,hideMountsCheck,hideEffectsCheck,hideShineCheck,visualCatalogStatus,packIntegrationStatus,packFolderButton,packRefreshButton})
            if(control!=null){control.Visible=true;control.Enabled=true;control.BringToFront();}
        foreach(string name in new[]{"attackBalancedSpeed","attackFastSpeed","performanceBalancedSpeed","performanceFastSpeed","saveFarmProfile","loadFarmProfile","optimizedFarmProfile"})
        {
            var controls=Controls.Find(name,true);if(controls.Length>0){controls[0].Visible=true;controls[0].Enabled=true;controls[0].BringToFront();}
        }
    }
    private async Task LoadVisualCatalogAsync(string selectedRoot=null)
    {
        string root=selectedRoot;
        if(string.IsNullOrWhiteSpace(root))root=!string.IsNullOrWhiteSpace(advancedState?.PackRoot)?advancedState.PackRoot:DefaultPackEngineRoot();
        if(string.IsNullOrWhiteSpace(root)){if(visualCatalogStatus!=null)visualCatalogStatus.Text="Pack klasörü seçildiğinde pet ve binek listesi otomatik okunur.";return;}
        try
        {
            var loaded=await Task.Run(()=>PackVisualCatalog.Load(root));
            if(IsDisposed||Disposing)return;visualCatalog=loaded;sessionDirty=true;
            if(visualCatalogStatus!=null)visualCatalogStatus.Text="Pack doğrulaması · "+loaded.MountVnums.Count+" binek modeli · "+loaded.PetVnums.Count+" pet modeli";
        }
        catch(Exception ex){if(!IsDisposed&&!Disposing&&visualCatalogStatus!=null)visualCatalogStatus.Text="Pack görüntü listesi okunamadı: "+ex.Message;}
    }
    private void SyncSpeedControls(bool fromAttackPage)
    {
        if(syncingSpeedControls)return;syncingSpeedControls=true;
        try
        {
            if(fromAttackPage){performanceAttackSpeedCheck.Checked=attackSpeedCheck.Checked;performanceMoveSpeedCheck.Checked=moveSpeedCheck.Checked;performanceAttackSpeedValue.Value=attackSpeedValue.Value;performanceMoveSpeedValue.Value=moveSpeedValue.Value;}
            else{attackSpeedCheck.Checked=performanceAttackSpeedCheck.Checked;moveSpeedCheck.Checked=performanceMoveSpeedCheck.Checked;attackSpeedValue.Value=performanceAttackSpeedValue.Value;moveSpeedValue.Value=performanceMoveSpeedValue.Value;}
        }
        finally{syncingSpeedControls=false;}
    }
    private Button NewPresetButton(string text,int x,int y,int width)
    {
        return new Button { Text=text,Location=new Point(x,y),Size=new Size(width,34),FlatStyle=FlatStyle.Flat,BackColor=StudioRaised,ForeColor=StudioText };
    }
    private void ApplySpeedPreset(int attack,int movement)
    {
        attackSpeedValue.Value=attack;moveSpeedValue.Value=movement;
        attackSpeedCheck.Checked=true;moveSpeedCheck.Checked=true;
        sessionDirty=true;UpdatePerformanceStatus();
    }
    private Guna2CheckBox NewOption(string name,string text,int x,int y,int width)
    {
        return new Guna2CheckBox { Name=name,Text=text,Location=new Point(x,y),Size=new Size(width,30),ForeColor=StudioText,Font=new Font("Segoe UI Semibold",10f) };
    }
    private NumericUpDown NewNumber(string name,int x,int y,int width,int min,decimal max,int value,int increment)
    {
        return new NumericUpDown { Name=name,Location=new Point(x,y),Size=new Size(width,30),Minimum=min,Maximum=max,Value=value,Increment=increment,BackColor=StudioRaised,ForeColor=StudioText,BorderStyle=BorderStyle.FixedSingle,TextAlign=HorizontalAlignment.Center };
    }
    private void AddSectionCard(Control parent,string title,string detail,int x,int y,int width,int height)
    {
        var card=new Panel { Location=new Point(x,y),Size=new Size(width,80),BackColor=StudioRaised,Tag="section-card" };
        card.Paint+=(s,e)=> { using(var pen=new Pen(StudioBorder))e.Graphics.DrawRectangle(pen,0,0,card.Width-1,card.Height-1); };
        AddCaption(card,title,20,16,width-40,22,9f,StudioAccent);
        AddCaption(card,detail,20,42,width-40,38,9f,StudioMuted);
        parent.Controls.Add(card);card.SendToBack();
    }
    private bool EnhancementActive
    {
        get { return RouteOwnsCombat||(rangePickupCheck!=null&&rangePickupCheck.Checked)||(attackSpeedCheck!=null&&attackSpeedCheck.Checked)||(moveSpeedCheck!=null&&moveSpeedCheck.Checked)||(cameraDistanceCheck!=null&&cameraDistanceCheck.Checked)||VisualSettingsActive||HybridWaitRequested||taskDungeonRunning||normalBotRequested||Field_02DF||(dmgStartCheck!=null&&dmgStartCheck.Checked)||autoCloseShaman; }
    }
    private bool VisualSettingsActive { get { return (hidePetsCheck!=null&&hidePetsCheck.Checked)||(hideMountsCheck!=null&&hideMountsCheck.Checked)||(hideEffectsCheck!=null&&hideEffectsCheck.Checked)||(hideShineCheck!=null&&hideShineCheck.Checked); } }
    private void UpdatePerformanceStatus()
    {
        if(performanceStatus==null)return;
        var active=new System.Collections.Generic.List<string>();
        if(rangePickupCheck.Checked)active.Add("alan toplama");
        if(attackSpeedCheck.Checked)active.Add("saldırı hızı uygulanmadı");
        if(moveSpeedCheck.Checked)active.Add("yürüme hızı uygulanmadı");
        if(cameraDistanceCheck.Checked)active.Add("kamera "+cameraDistanceValue.Value);
        if(hidePetsCheck.Checked)active.Add("petler gizli");
        if(hideMountsCheck.Checked)active.Add("binekler gizli");
        if(hideEffectsCheck.Checked)active.Add("savaş efektleri sade");
        if(hideShineCheck.Checked)active.Add("parlamalar gizli");
        if(hybridWaitCheck!=null&&hybridWaitCheck.Checked)active.Add(HybridWaitRuntimeActive?"karma Wait aktif":"karma Wait hazır");
        if(DeveloperLinkedAttackActive)active.Add("geliştirici Wait hedefi "+DeveloperLinkedAttackSpeed);
        else if(DeveloperLinkedAttackRequested)active.Add("geliştirici eşleşme bekliyor");
        performanceStatus.Text=active.Count==0?"Hazır · tüm seçenekler kapalı":"Etkin · "+string.Join("  •  ",active);
    }
}






