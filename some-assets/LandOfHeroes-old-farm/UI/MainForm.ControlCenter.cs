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
using Guna.UI2.WinForms;

public partial class MainForm
{
    private Guna2Button controlCenterButton;
    private Form controlCenterForm, overlayForm;
    private Panel controlCenterHeader;
    private TabControl controlCenterTabs;
    private DataGridView clientDashboard;
    private ComboBox profileCombo, priorityCombo, patrolCombo, skillModeCombo;
    private TextBox whitelistBox, blacklistBox, ruleValue, ruleArgument, inventoryMatch;
    private ComboBox ruleCondition, ruleAction, inventoryRuleAction;
    private NumericUpDown ruleCooldown, inventoryThreshold, inventoryInterval;
    private CheckBox ruleEnabled;
    private ListBox ruleList, zoneList, normalSkillList, bossSkillList, inventoryRuleList, activityList, flowList;
    private Label statsLabel, healthLabel, pointLabel, overlayStatus, overlayTarget;
    private TextBox flowName;
    private ComboBox flowNpc, flowItem;
    private ComboBox flowAttack, flowTransition;
    private NumericUpDown flowRetry, flowWait, flowAttackRange, flowAttackWait;
    private CheckBox flowEnabled, flowTeleport, flowObstacle;
    private System.Windows.Forms.Timer advancedTimer;
    private AdvancedState advancedState = new AdvancedState();
    private readonly List<string> activityEntries = new List<string>();
    private readonly Dictionary<int,EntityState> observedHostiles = new Dictionary<int,EntityState>();
    private readonly Dictionary<uint,long> observedItemCounts = new Dictionary<uint,long>();
    private DateTime lastHostileSeenUtc = DateTime.UtcNow, currentTargetSinceUtc = DateTime.UtcNow, nextHealthReadUtc = DateTime.MinValue, nextZoneChangeUtc = DateTime.MinValue;
    private bool advancedPulseBusy, observedConnection, previousConnection;
    private int currentObservedTarget, patrolZoneIndex, inventoryOccupied;
    private double currentHealthPercent = -1;
    private static string AdvancedStatePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","gelismis-ayarlar.xml"); } }
    private static string ProfileFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","profiller"); } }
    private static string ActivityFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","gunluk"); } }

    private void InitializeControlCenter()
    {
        try { if(File.Exists(AdvancedStatePath)) advancedState=AdvancedStore.Load<AdvancedState>(AdvancedStatePath)??new AdvancedState(); } catch { advancedState=new AdvancedState(); }
        advancedState.Normalize();
        controlCenterButton=TaskButton("Kontrol Merkezi",20,678,246);
        controlCenterButton.Name="controlCenterButton";
        controlCenterButton.Click+=(s,e)=>ShowControlCenter();
        leftPanel.Controls.Add(controlCenterButton);
        controlCenterButton.BringToFront();
        advancedTimer=new System.Windows.Forms.Timer(components){Interval=1000};
        advancedTimer.Tick+=async(s,e)=>await AdvancedPulseAsync();
        advancedTimer.Start();
        Shown+=(s,e)=>_ = EnsurePackCatalogLoadedAsync();
        FormClosed+=(s,e)=>{SaveAdvancedState();try{overlayForm?.Close();controlCenterForm?.Close();}catch{}};
        RecordActivity("Sistem","Kontrol Merkezi hazır.");
    }

    private void ShowControlCenter()
    {
        if(controlCenterForm!=null&&!controlCenterForm.IsDisposed){controlCenterForm.Show();controlCenterForm.BringToFront();return;}
        controlCenterForm=new Form {
            Text="Onur · Kontrol Merkezi",ClientSize=new Size(1120,720),MinimumSize=new Size(1136,759),MaximumSize=new Size(1136,759),FormBorderStyle=FormBorderStyle.FixedSingle,MaximizeBox=false,
            StartPosition=FormStartPosition.CenterParent,BackColor=StudioBackground,ForeColor=StudioText,
            Font=new Font("Segoe UI",9.5f),ShowInTaskbar=false
        };
        controlCenterHeader=new Panel{Dock=DockStyle.Top,Height=72};
        var centerTitle=new Label{Text="Kontrol Merkezi",Left=24,Top=12,Width=420,Height=28,Font=new Font("Segoe UI Semibold",16f)};
        var centerInfo=new Label{Text="İstemci, otomasyon, zindan ve analiz ayarları",Left=26,Top=42,Width=560,Height=20,Font=new Font("Segoe UI",9f)};
        controlCenterHeader.Controls.AddRange(new Control[]{centerTitle,centerInfo});
        controlCenterTabs=new TabControl{Dock=DockStyle.Fill,Font=new Font("Segoe UI Semibold",9.5f),DrawMode=TabDrawMode.OwnerDrawFixed,SizeMode=TabSizeMode.Fixed,ItemSize=new Size(168,42),Padding=new Point(16,6)};
        controlCenterTabs.DrawItem+=DrawControlCenterTab;
        controlCenterForm.Controls.Add(controlCenterTabs);controlCenterForm.Controls.Add(controlCenterHeader);
        BuildClientDashboardPage();BuildRulesPage();BuildTargetAreaPage();
        BuildSkillInventoryPage();BuildDungeonFlowPage();BuildActivityPage();
        StyleProfessionalTree(controlCenterForm);
        ApplyControlCenterAppearance();
        ApplyAdvancedStateToUi();
        RefreshDungeonProfileSelectors();
        controlCenterForm.FormClosing+=(s,e)=>{if(e.CloseReason==CloseReason.UserClosing){e.Cancel=true;controlCenterForm.Hide();SaveAdvancedState();}};
        controlCenterForm.Show(this);RefreshClientDashboard();RefreshProfileList();
        if(advancedState.OverlayEnabled&&(overlayForm==null||overlayForm.IsDisposed))ToggleOverlay();
    }

    private TabPage NewControlPage(string title)
    {
        var page=new TabPage(title){BackColor=StudioSurface,ForeColor=StudioText,Padding=new Padding(12),AutoScroll=true};
        controlCenterTabs.TabPages.Add(page);return page;
    }
    private static Button PlainButton(string text,int left,int top,int width=130)
    {
        return new Button{Text=text,Left=left,Top=top,Width=width,Height=34,FlatStyle=FlatStyle.Flat};
    }
    private static Label PlainLabel(string text,int left,int top,int width=220)
    {
        return new Label{Text=text,Left=left,Top=top,Width=width,Height=24};
    }

    private void BuildClientDashboardPage()
    {
        var page=NewControlPage("İstemciler / Profiller");
        page.Controls.Add(PlainLabel("Her satır ayrı PID'dir. Bu uygulama yalnızca seçtiğiniz PID'yi yönetir.",16,14,780));
        clientDashboard=new DataGridView{Left=16,Top=44,Width=1048,Height=430,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
        clientDashboard.Columns.Add("pid","PID");clientDashboard.Columns.Add("character","Karakter");clientDashboard.Columns.Add("state","Durum");clientDashboard.Columns.Add("map","Harita");clientDashboard.Columns.Add("bot","Çalışan iş");
        page.Controls.Add(clientDashboard);
        var refresh=PlainButton("Listeyi yenile",16,490,145);var bind=PlainButton("Seçili PID'ye bağlan",170,490,190);var start=PlainButton("Botu başlat / durdur",370,490,180);var dungeon=PlainButton("Zindanı başlat / durdur",560,490,190);
        refresh.Click+=(s,e)=>RefreshClientDashboard();bind.Click+=async(s,e)=>await BindDashboardClientAsync();start.Click+=(s,e)=>ToggleNormalBot();
        dungeon.Click+=async(s,e)=>{if(taskCancellation!=null)StopTaskSequence("Kontrol Merkezinden durduruldu.");else await RunTaskSequenceAsync(false);};
        page.Controls.AddRange(new Control[]{refresh,bind,start,dungeon});
        page.Controls.Add(PlainLabel("Karakter / görev profili",16,552,200));
        profileCombo=new ComboBox{Left=16,Top=580,Width=300,Height=30,DropDownStyle=ComboBoxStyle.DropDown};
        var save=PlainButton("Kaydet",326,578,100);var load=PlainButton("Yükle",436,578,100);var remove=PlainButton("Sil",546,578,85);var export=PlainButton("Dışa aktar",641,578,120);var import=PlainButton("İçe aktar",771,578,120);
        save.Click+=(s,e)=>SaveNamedProfile();load.Click+=(s,e)=>LoadNamedProfile();remove.Click+=(s,e)=>DeleteNamedProfile();export.Click+=(s,e)=>ExportNamedProfile();import.Click+=(s,e)=>ImportNamedProfile();
        page.Controls.AddRange(new Control[]{profileCombo,save,load,remove,export,import});
    }

    private void BuildRulesPage()
    {
        var page=NewControlPage("Kurallar");
        page.Controls.Add(PlainLabel("Koşul gerçekleştiğinde eylem çalışır; bekleme süresi aynı kuralın art arda tetiklenmesini önler.",16,14,1000));
        ruleList=new ListBox{Left=16,Top=48,Width=520,Height=500};page.Controls.Add(ruleList);int x=560;
        page.Controls.Add(PlainLabel("Koşul",x,48));
        ruleCondition=new ComboBox{Left=x,Top=74,Width=250,DropDownStyle=ComboBoxStyle.DropDownList};
        ruleCondition.Items.AddRange(new object[]{"Can (%) altında","Hedef yok (sn)","Envanter doluluk (%) üstünde","Bağlantı koptu","Takılma sayısı üstünde","Çalışma süresi (dk) üstünde","Eşya adedi altında"});ruleCondition.SelectedIndex=0;page.Controls.Add(ruleCondition);
        page.Controls.Add(PlainLabel("Değer",x+270,48,120));ruleValue=new TextBox{Left=x+270,Top=74,Width=130,Text="35"};page.Controls.Add(ruleValue);
        page.Controls.Add(PlainLabel("Eylem",x,122));
        ruleAction=new ComboBox{Left=x,Top=148,Width=250,DropDownStyle=ComboBoxStyle.DropDownList};
        ruleAction.Items.AddRange(new object[]{"Uyarı yaz","Normal botu başlat","Normal botu durdur","Zindanı başlat","Zindanı durdur","Bağlantıyı yenile","Eşya kullanımına ekle","Profil yükle","Ekran bilgisini aç"});ruleAction.SelectedIndex=0;page.Controls.Add(ruleAction);
        page.Controls.Add(PlainLabel("Eylem değeri",x+270,122,130));ruleArgument=new TextBox{Left=x+270,Top=148,Width=210};page.Controls.Add(ruleArgument);
        page.Controls.Add(PlainLabel("Tekrar bekleme (sn)",x,196,200));ruleCooldown=new NumericUpDown{Left=x,Top=222,Width=120,Minimum=1,Maximum=86400,Value=10};page.Controls.Add(ruleCooldown);
        ruleEnabled=new CheckBox{Left=x+150,Top=222,Width=180,Text="Kural etkin",Checked=true};page.Controls.Add(ruleEnabled);
        var add=PlainButton("Yeni ekle",x,278,120);var update=PlainButton("Güncelle",x+130,278,120);var del=PlainButton("Sil",x+260,278,100);var save=PlainButton("Tümünü kaydet",x,326,230);
        add.Click+=(s,e)=>AddRule();update.Click+=(s,e)=>UpdateRule();del.Click+=(s,e)=>RemoveRule();save.Click+=(s,e)=>SaveAdvancedState();ruleList.SelectedIndexChanged+=(s,e)=>LoadSelectedRule();
        page.Controls.AddRange(new Control[]{add,update,del,save});
        var note=PlainLabel("Eşya adedi koşulunda değer alanına ID:adet yazın. Eşya kullanımında eylem değerine ID:saniye yazın.",x,385,480);note.Height=60;page.Controls.Add(note);
    }

    private void BuildTargetAreaPage()
    {
        var page=NewControlPage("Hedef / Alan");
        page.Controls.Add(PlainLabel("Hedef önceliği",16,14,180));
        priorityCombo=new ComboBox{Left=16,Top=40,Width=240,DropDownStyle=ComboBoxStyle.DropDownList};priorityCombo.Items.AddRange(new object[]{"Taş > Canavar","Canavar > Taş","Boss > Taş > Canavar","En yakın"});page.Controls.Add(priorityCombo);
        page.Controls.Add(PlainLabel("Beyaz liste · ad, VID, taş, canavar; virgülle ayırın",16,84,500));whitelistBox=new TextBox{Left=16,Top=110,Width=500,Height=74,Multiline=true,ScrollBars=ScrollBars.Vertical};page.Controls.Add(whitelistBox);
        page.Controls.Add(PlainLabel("Kara liste · ulaşılamayan veya istenmeyen hedefler",16,196,500));blacklistBox=new TextBox{Left=16,Top=222,Width=500,Height=74,Multiline=true,ScrollBars=ScrollBars.Vertical};page.Controls.Add(blacklistBox);
        var saveTarget=PlainButton("Hedef ayarlarını kaydet",16,310,220);saveTarget.Click+=(s,e)=>{SyncAdvancedStateFromUi();SaveAdvancedState();RecordActivity("Hedef","Hedef öncelikleri güncellendi.");};page.Controls.Add(saveTarget);
        int x=550;page.Controls.Add(PlainLabel("Büyük haritada çizilen alanı ekleyin",x,14,480));
        zoneList=new ListBox{Left=x,Top=40,Width=500,Height=280};page.Controls.Add(zoneList);
        var add=PlainButton("Gezi alanı ekle",x,334,145);var exclude=PlainButton("Yasak alan ekle",x+155,334,145);var activate=PlainButton("Seçileni etkinleştir",x+310,334,170);
        var remove=PlainButton("Alanı sil",x,378,145);var toggle=PlainButton("Etkin / kapalı",x+155,378,145);
        add.Click+=(s,e)=>AddCurrentFarmZone(false);exclude.Click+=(s,e)=>AddCurrentFarmZone(true);activate.Click+=(s,e)=>ActivateSelectedZone();remove.Click+=(s,e)=>RemoveSelectedZone();toggle.Click+=(s,e)=>ToggleSelectedZone();
        page.Controls.AddRange(new Control[]{add,exclude,activate,remove,toggle});
        page.Controls.Add(PlainLabel("Bölgeler arası dolaşma",x,434,200));patrolCombo=new ComboBox{Left=x,Top=460,Width=200,DropDownStyle=ComboBoxStyle.DropDownList};patrolCombo.Items.AddRange(new object[]{"Sıralı","Rastgele","Tek alan"});page.Controls.Add(patrolCombo);
        var start=PlainButton("Başlangıç noktası",x,510,155);var ret=PlainButton("Dönüş noktası",x+165,510,155);var clear=PlainButton("Noktaları temizle",x+330,510,150);
        start.Click+=(s,e)=>CaptureFarmPoint(true);ret.Click+=(s,e)=>CaptureFarmPoint(false);clear.Click+=(s,e)=>{advancedState.StartPoint=null;advancedState.ReturnPoint=null;UpdatePointLabel();SaveAdvancedState();};
        pointLabel=PlainLabel("",x,558,500);pointLabel.Height=56;page.Controls.AddRange(new Control[]{start,ret,clear,pointLabel});
    }
}
