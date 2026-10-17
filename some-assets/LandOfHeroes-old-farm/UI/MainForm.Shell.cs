using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;

// Uygulama kabuğu: kompakt pencere, üst çubuk, gruplu sol menü ve her bölümde alt sekmeler.
// Yalnızca görünüm katmanıdır: var olan kontroller, olayları ve değerleriyle birlikte yeni yerlerine taşınır;
// hiçbir özelliğin çalışma kodu değişmez. Kontrol Merkezi ayrı pencere yerine ilgili menülerin alt sekmesi olur.
public partial class MainForm
{
    private const int ShellWindowWidth=1280,ShellWindowHeight=760,ShellHeaderHeight=64,ShellSidebarWidth=208,ShellRadarWidth=342,ShellPageMargin=30,ShellActionsHeight=144;
    private sealed class ShellSub{public string Title;public TabPage Page;public TabControl Inner;public TabPage InnerPage;}
    private sealed class ShellSection{public string Title,Subtitle,Group;public int Icon,LastSub;public bool GroupStart;public readonly List<ShellSub> Subs=new List<ShellSub>();
        public ShellSection Add(string title,TabPage page){if(page!=null)Subs.Add(new ShellSub{Title=title,Page=page});return this;}}

    private ShellSurface shellSidebar,shellActions,shellRadarPane,shellContent,shellPageHost,shellAttackHost;
    private ShellNav shellNav;private ShellTabs shellTabs;private Label shellTitle,shellSubtitle;
    private ShellBrand shellBrand;private ShellStatus shellStatus;private ShellIconButton shellThemeButton;
    private TabPage shellOverviewPage;private Timer shellStatusTimer;
    private readonly List<ShellSection> shellSections=new List<ShellSection>();
    private readonly List<TabPage> shellFlowPages=new List<TabPage>();
    private readonly HashSet<Control> shellReflowPending=new HashSet<Control>(),shellHooked=new HashSet<Control>();
    private bool shellBuilt,shellSyncing;private int shellCurrent=-1;

    protected override void OnLoad(EventArgs e)
    {
        if(!shellBuilt){shellBuilt=true;BuildShell();}
        base.OnLoad(e);
        var area=Screen.FromPoint(Cursor.Position).WorkingArea;
        Location=new Point(area.Left+Math.Max(0,(area.Width-Width)/2),area.Top+Math.Max(0,(area.Height-Height)/2));
    }

    private void BuildShell()
    {
        SuspendLayout();
        var startPage=mainTabs.SelectedTab;
        ShellEnsureControlCenter();

        // Sayfalar: ana sekmeler ve Kontrol Merkezi'nin sekmeleri tek bir sekme kümesinde toplanır.
        var known=new HashSet<TabPage>{tabMain,tabDamage,tabRoute,inventoryPage,performancePage,compatibilityPage,taskPage};
        TabPage sessionPage=mainTabs.TabPages.Cast<TabPage>().FirstOrDefault(p=>!known.Contains(p));
        var centerPages=controlCenterTabs.TabPages.Cast<TabPage>().ToList();
        foreach(var p in centerPages){controlCenterTabs.TabPages.Remove(p);p.AutoScroll=true;p.Padding=Padding.Empty;mainTabs.TabPages.Add(p);}
        Func<string,TabPage> center=t=>centerPages.FirstOrDefault(p=>p.Text==t);
        TabPage clientsPage=center("İstemciler / Profiller"),rulesPage=center("Kurallar"),areaPage=center("Hedef / Alan"),
            skillsPage=center("Beceri / Envanter"),flowPage=center("Zindan Akışı"),activityPage=center("Günlük / İstatistik");
        shellOverviewPage=new TabPage("Genel bakış"){Name="shellOverviewPage",AutoScroll=true,BackColor=LightSurface};mainTabs.TabPages.Add(shellOverviewPage);
        var attackPages=attackSections!=null?attackSections.TabPages.Cast<TabPage>().ToList():new List<TabPage>();

        // Menü yapısı: her özellik kendi bölümünde, ilgili ayarlar aynı bölümün alt sekmelerinde.
        Section("Genel bakış","Sabit radar, canlı bağlantı ve operasyon güvenliği",0,null).Add("Genel bakış",shellOverviewPage);
        Section("Hareket","Hedef türü, takip, dolaşma ve hedef alanları",1,"OTOMASYON").Add("Hareket",tabMain).Add("Hedef ve alan",areaPage);
        var attack=Section("Saldırı","Vuruş modları, zamanlama ve karakter hızları",2,null);
        string[] attackTitles={"Saldırı","İstemci eşleme","İzleme ve test"};
        if(attackPages.Count==0)attack.Add("Saldırı",tabDamage);
        for(int i=0;i<attackPages.Count;i++)attack.Subs.Add(new ShellSub{Title=i<attackTitles.Length?attackTitles[i]:attackPages[i].Text,Page=tabDamage,Inner=attackSections,InnerPage=attackPages[i]});
        Section("Rota","Rota kaydı, profiller ve güvenlik kontrolü",3,null).Add("Rota",tabRoute);
        Section("Zindan","Aşamalı zindan görevi ve akış düzenleyici",4,null).Add("Görev",taskPage).Add("Akış düzenleyici",flowPage);
        Section("Envanter","Beceriler, eşyalar ve otomatik toplama",5,"KARAKTER").Add("Envanter",inventoryPage).Add("Beceri sırası ve kurallar",skillsPage);
        Section("Oturum","Hesap, otomatik giriş ve yeniden başlatma",6,null).Add("Oturum",sessionPage);
        Section("Otomasyon","Koşul ve eylem kuralları, istemciler ve profiller",7,"SİSTEM").Add("Kurallar",rulesPage).Add("İstemciler ve profiller",clientsPage);
        Section("Görsel","Kamera, görsel sadelik ve pack verileri",8,null).Add("Görsel ve performans",performancePage);
        Section("Uyumluluk","İstemci analizi, adresler ve pack çıkarma",9,null).Add("Uyumluluk",compatibilityPage);
        Section("Günlük","Etkinlik günlüğü ve oturum istatistikleri",10,null).Add("Günlük ve istatistik",activityPage);
        shellSections.RemoveAll(s=>s.Subs.Count==0);

        ShellBuildFrame();
        ShellBuildOverview();
        ShellPreparePages(centerPages,attackPages,sessionPage);

        mainTabs.SelectedIndexChanged+=(s,e)=>ShellSyncFromTabs();
        if(attackSections!=null)attackSections.SelectedIndexChanged+=(s,e)=>ShellSyncFromTabs();
        themeToggleButton.Click+=(s,e)=>ShellApplyTheme();
        ApplyAppearanceTheme(nightTheme,false);ShellApplyTheme();

        int section=shellSections.FindIndex(s=>s.Subs.Any(x=>x.Page==startPage));
        ShellSelect(section<0?0:section,Math.Max(0,section<0?0:shellSections[section].Subs.FindIndex(x=>x.Page==startPage)));
        shellStatusTimer=new Timer(components){Interval=400};shellStatusTimer.Tick+=(s,e)=>ShellUpdateStatus();shellStatusTimer.Start();ShellUpdateStatus();
        ResumeLayout(true);
        foreach(var page in shellFlowPages)ShellQueueReflow(page);
    }

    private ShellSection Section(string title,string subtitle,int icon,string group)
    {
        var s=new ShellSection{Title=title,Subtitle=subtitle,Icon=icon,Group=group,GroupStart=group!=null};shellSections.Add(s);return s;
    }

    // Kontrol Merkezi sayfaları ayrı pencere açılmadan bir kez kurulur; pencere görünmez kalır.
    private void ShellEnsureControlCenter()
    {
        if(controlCenterForm!=null&&!controlCenterForm.IsDisposed)return;
        controlCenterForm=new Form{Text="Onur · Kontrol Merkezi",FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,
            Location=new Point(-32000,-32000),Size=new Size(Width,Height),BackColor=StudioBackground,ForeColor=StudioText,Font=new Font("Segoe UI",8.75f)};
        controlCenterHeader=new Panel{Dock=DockStyle.Top,Height=0};
        controlCenterTabs=new TabControl{Dock=DockStyle.Fill,Font=new Font("Segoe UI",8.75f)};
        controlCenterForm.Controls.Add(controlCenterTabs);controlCenterForm.Controls.Add(controlCenterHeader);
        BuildClientDashboardPage();BuildRulesPage();BuildTargetAreaPage();
        BuildSkillInventoryPage();BuildDungeonFlowPage();BuildActivityPage();
        ApplyAdvancedStateToUi();
        RefreshDungeonProfileSelectors();
        controlCenterForm.FormClosing+=(s,e)=>{if(e.CloseReason==CloseReason.UserClosing){e.Cancel=true;controlCenterForm.Hide();SaveAdvancedState();}};
        // Kontrol Merkezi'ni açmak isteyen yerler (ör. "Zindan editörünü aç") pencere yerine ilgili sekmeye gelir.
        controlCenterForm.VisibleChanged+=(s,e)=>
        {
            if(!controlCenterForm.Visible)return;
            controlCenterForm.Hide();
            BeginInvoke((Action)(()=>{int i=shellSections.FindIndex(x=>x.Title=="Zindan");if(i>=0)ShellSelect(i,1);Activate();}));
        };
        // Diyaloglar bu pencereyi sahip olarak kullanır; ana pencerenin üstünde ortalansınlar.
        Action follow=()=>{if(!controlCenterForm.IsDisposed&&!controlCenterForm.Visible)controlCenterForm.Bounds=Bounds;};
        Move+=(s,e)=>follow();Resize+=(s,e)=>follow();Shown+=(s,e)=>follow();
        RefreshClientDashboard();RefreshProfileList();
        if(advancedState.OverlayEnabled&&(overlayForm==null||overlayForm.IsDisposed))ToggleOverlay();
    }

    private void ShellBuildFrame()
    {
        MinimumSize=Size.Empty;MaximumSize=Size.Empty;StartPosition=FormStartPosition.Manual;
        ClientSize=new Size(ShellWindowWidth,ShellWindowHeight);MinimumSize=Size;MaximumSize=Size;
        Padding=Padding.Empty;

        // Üst çubuk
        headerPanel.Dock=DockStyle.None;headerPanel.SetBounds(1,1,ShellWindowWidth-2,ShellHeaderHeight);
        headerPanel.BorderRadius=0;headerPanel.BorderThickness=0;headerPanel.ShadowDecoration.Enabled=false;
        foreach(Control c in headerPanel.Controls.Cast<Control>().ToList())
            if(c is Label||c is Guna2HtmlLabel)c.Visible=false;
        themeToggleButton.Visible=false;
        shellBrand=new ShellBrand{Bounds=new Rectangle(24,0,304,ShellHeaderHeight),Text="ONUR CONTROL"};
        shellStatus=new ShellStatus{Bounds=new Rectangle(334,0,240,ShellHeaderHeight)};
        int right=ShellWindowWidth-2-12;
        guna2ControlBox1.SetBounds(right-34,17,34,30);
        guna2ControlBox2.SetBounds(right-34-4-34,17,34,30);
        shellThemeButton=new ShellIconButton{Bounds=new Rectangle(guna2ControlBox2.Left-12-32,17,32,30),Icon=20,AccessibleName="Tema"};
        shellThemeButton.Click+=(s,e)=>{ApplyAppearanceTheme(!nightTheme,true);ShellApplyTheme();};
        var tip=new ToolTip(components);tip.SetToolTip(shellThemeButton,"Gece / gündüz teması");
        emergencyUnlock.SetBounds(shellThemeButton.Left-12-78,17,78,30);
        emergencyButton.SetBounds(emergencyUnlock.Left-8-132,17,132,30);
        emergencyButton.Text="TÜMÜNÜ DURDUR";emergencyButton.Cursor=Cursors.Hand;emergencyButton.FlatStyle=FlatStyle.Flat;emergencyButton.FlatAppearance.BorderSize=0;
        emergencyButton.Font=new Font("Segoe UI Semibold",8.25f);
        emergencyUnlock.Font=new Font("Segoe UI Semibold",8.25f);emergencyUnlock.Cursor=Cursors.Hand;
        tip.SetToolTip(emergencyButton,"Her pencerede tüm otomasyonları hemen durdurur");
        shellStatus.Left=emergencyButton.Left-16-shellStatus.Width;
        headerPanel.Controls.AddRange(new Control[]{shellBrand,shellStatus,shellThemeButton});
        foreach(Control c in new Control[]{shellBrand,shellStatus,shellThemeButton,emergencyButton,emergencyUnlock,guna2ControlBox1,guna2ControlBox2})c.BringToFront();

        // Sol menü ve alt eylem alanı
        int bodyTop=1+ShellHeaderHeight,bodyHeight=ShellWindowHeight-2-ShellHeaderHeight;
        shellSidebar=new ShellSurface{Name="shellSidebar",Bounds=new Rectangle(1,bodyTop,ShellSidebarWidth,bodyHeight),BorderRight=true};
        shellNav=new ShellNav{Bounds=new Rectangle(0,8,ShellSidebarWidth-1,bodyHeight-ShellActionsHeight-8)};
        foreach(var s in shellSections)shellNav.Items.Add(new ShellNavItem{Text=s.Title,Icon=s.Icon,GroupStart=s.GroupStart,Group=s.Group});
        shellNav.ItemClicked+=(s,e)=>{var sec=shellSections[shellNav.ClickedIndex];ShellSelect(shellNav.ClickedIndex,sec.LastSub);};
        shellActions=new ShellSurface{Bounds=new Rectangle(0,bodyHeight-ShellActionsHeight,ShellSidebarWidth-1,ShellActionsHeight),BorderTop=true};
        int inner=ShellSidebarWidth-1-32;
        clientPicker.Parent=shellActions;clientPicker.SetBounds(16,14,inner,24);
        connectBtn.Parent=shellActions;connectBtn.SetBounds(16,46,inner,32);
        startStopFarmBtn.Parent=shellActions;startStopFarmBtn.SetBounds(16,84,inner,32);
        foreach(var b in new[]{connectBtn,startStopFarmBtn}){b.Font=new Font("Segoe UI Semibold",8f);b.BorderRadius=8;b.Cursor=Cursors.Hand;}
        shellSidebar.Controls.Add(shellNav);shellSidebar.Controls.Add(shellActions);
        leftPanel.Visible=false;
        if(controlCenterButton!=null)controlCenterButton.Visible=false;

        // İçerik: başlık, alt sekmeler ve sayfa
        ShellBuildFixedRadarPane(bodyTop,bodyHeight);
        shellContent=new ShellSurface{Name="shellContent",Bounds=new Rectangle(1+ShellSidebarWidth+ShellRadarWidth,bodyTop,ShellWindowWidth-2-ShellSidebarWidth-ShellRadarWidth,bodyHeight)};
        shellTitle=new Label{AutoSize=false,UseMnemonic=false,Bounds=new Rectangle(ShellPageMargin,16,560,30),Font=new Font("Segoe UI Semibold",16f),BackColor=Color.Transparent,ForeColor=LightText};
        shellSubtitle=new Label{AutoSize=false,UseMnemonic=false,AutoEllipsis=true,Bounds=new Rectangle(ShellPageMargin+1,48,shellContent.Width-2*ShellPageMargin,18),Font=new Font("Segoe UI",9f),BackColor=Color.Transparent,ForeColor=LightMuted};
        shellTabs=new ShellTabs{Bounds=new Rectangle(ShellPageMargin-10,76,shellContent.Width-2*(ShellPageMargin-10),36)};
        shellTabs.TabClicked+=(s,e)=>ShellSelect(shellCurrent,shellTabs.ClickedIndex);
        shellPageHost=new ShellSurface{Name="shellPageHost"};
        shellContent.Controls.AddRange(new Control[]{shellTitle,shellSubtitle,shellTabs,shellPageHost});
        Controls.Add(shellContent);Controls.Add(shellRadarPane);Controls.Add(shellSidebar);
        mainTabs.Dock=DockStyle.None;mainTabs.Alignment=TabAlignment.Top;mainTabs.Parent=shellPageHost;
        shellPageHost.Resize+=(s,e)=>ShellFitTabs(mainTabs,shellPageHost);
        ShellLayoutHost(true);

        // Saldırı alt bölümleri aynı şekilde: kendi sekme başlıkları gizlenir, alt sekmeler üstten seçilir.
        if(attackSections!=null)
        {
            tabDamage.AutoScroll=false;tabDamage.Padding=Padding.Empty;
            shellAttackHost=new ShellSurface{Name="shellAttackHost",Dock=DockStyle.Fill};
            tabDamage.Controls.Add(shellAttackHost);attackSections.Dock=DockStyle.None;attackSections.Parent=shellAttackHost;
            shellAttackHost.Resize+=(s,e)=>ShellFitTabs(attackSections,shellAttackHost);
            ShellFitTabs(attackSections,shellAttackHost);
        }
    }

    // Canlı radar, menü ve çalışma alanından bağımsız sabit bir sol pencere olarak her bölümde görünür.
    private void ShellBuildFixedRadarPane(int bodyTop,int bodyHeight)
    {
        shellRadarPane=new ShellSurface{Name="shellRadarPane",Bounds=new Rectangle(1+ShellSidebarWidth,bodyTop,ShellRadarWidth,bodyHeight),BorderRight=true};
        var title=new Label{AutoSize=false,UseMnemonic=false,Text="CANLI RADAR",Bounds=new Rectangle(20,18,302,24),Font=new Font("Segoe UI Semibold",11.5f),ForeColor=LightText,BackColor=Color.Transparent};
        var subtitle=new Label{AutoSize=false,UseMnemonic=false,Text="Sabit izleme penceresi",Bounds=new Rectangle(20,43,302,18),Font=new Font("Segoe UI",8.5f),ForeColor=LightMuted,BackColor=Color.Transparent};
        var radarWindow=new ShellCard{Name="fixedRadarWindow",Bounds=new Rectangle(18,76,306,342),BackColor=LightRaised};
        radarWindow.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="RADAR / CANLI",Bounds=new Rectangle(12,11,282,20),Font=new Font("Segoe UI Semibold",7.5f),ForeColor=LightMuted,BackColor=Color.Transparent});
        radarPanel.Parent=radarWindow;radarPanel.SetBounds(10,42,286,286);
        if(!clientConnected){var old=radarPanel.BackgroundImage;radarPanel.BackgroundImage=RadarPlaceholder(radarPanel.Width,radarPanel.Height);radarPanel.BackgroundImageLayout=ImageLayout.Center;if(old!=null)old.Dispose();}

        var connection=new ShellCard{Name="fixedConnectionCard",Bounds=new Rectangle(18,436,306,98),BackColor=LightRaised};
        connection.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="BAĞLANTI",Bounds=new Rectangle(14,12,278,18),Font=new Font("Segoe UI Semibold",7.5f),ForeColor=LightMuted,BackColor=Color.Transparent});
        if(connectionStatus!=null){connectionStatus.Parent=connection;connectionStatus.AutoSize=false;connectionStatus.AutoEllipsis=true;connectionStatus.SetBounds(14,38,278,46);connectionStatus.BackColor=Color.Transparent;}

        var timingCard=new ShellCard{Name="fixedTimingCard",Bounds=new Rectangle(18,548,306,118),BackColor=LightRaised};
        timingCard.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="OPERASYON ÖLÇÜMÜ",Bounds=new Rectangle(14,12,278,18),Font=new Font("Segoe UI Semibold",7.5f),ForeColor=LightMuted,BackColor=Color.Transparent});
        var timing=leftPanel.Controls.Find("timingStatus",true).FirstOrDefault();
        if(timing!=null){timing.Parent=timingCard;timing.SetBounds(14,38,278,64);timing.BackColor=Color.Transparent;}
        shellRadarPane.Controls.AddRange(new Control[]{title,subtitle,radarWindow,connection,timingCard});
    }
    // Sekme kontrolünün başlık şeridi kabın dışında kalacak şekilde yerleştirilir; yalnızca sayfa alanı görünür.
    private static void ShellFitTabs(TabControl tabs,Control host)
    {
        if(host.ClientSize.Width<=0||host.ClientSize.Height<=0)return;
        var handle=tabs.Handle;
        for(int pass=0;pass<2;pass++)
        {
            var d=tabs.DisplayRectangle;
            tabs.SetBounds(-d.X,-d.Y,host.ClientSize.Width+(tabs.Width-d.Width),host.ClientSize.Height+(tabs.Height-d.Height));
        }
    }

    private void ShellLayoutHost(bool withTabs)
    {
        int top=withTabs?112:76;
        shellTabs.Visible=withTabs;
        shellPageHost.BorderTop=!withTabs;
        shellPageHost.SetBounds(0,top,shellContent.Width,shellContent.Height-top);
        ShellFitTabs(mainTabs,shellPageHost);
    }

    // Genel bakış: sabit radar penceresini açıklayan operasyon ve güvenlik kartları.
    private void ShellBuildOverview()
    {
        var page=shellOverviewPage;
        page.AutoScroll=false;
        int width=ShellWindowWidth-2-ShellSidebarWidth-ShellRadarWidth-SystemInformation.VerticalScrollBarWidth-2*ShellPageMargin;
        var summary=OverviewCard(page,"OPERASYON MERKEZİ","Radar soldaki sabit pencerede her zaman canlı kalır",ShellPageMargin,18,width,112);
        summary.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="Bölümler arasında geçiş yaptığınızda yalnızca sağ çalışma alanı değişir. Radar, bağlantı ve ölçüm paneli yerinden oynamaz.",Bounds=new Rectangle(16,62,width-32,34),Font=new Font("Segoe UI",9f),ForeColor=LightText,BackColor=Color.Transparent});

        int gap=14,cardWidth=(width-gap)/2;
        var stop=OverviewCard(page,"GÜVENLİK","Acil durdurma kısayolu",ShellPageMargin,146,cardWidth,176);
        emergencyHotkeyCombo.Parent=stop;emergencyHotkeyCombo.SetBounds(16,64,150,26);emergencyHotkeyCombo.FlatStyle=FlatStyle.Flat;
        emergencyStatus.Parent=stop;emergencyStatus.AutoSize=false;emergencyStatus.AutoEllipsis=true;emergencyStatus.SetBounds(16,102,cardWidth-32,50);emergencyStatus.BackColor=Color.Transparent;
        emergencyStatus.Font=new Font("Segoe UI",8.25f);emergencyStatus.ForeColor=LightMuted;

        var flow=OverviewCard(page,"ÇALIŞMA AKIŞI","Temiz ve sabit kontrol düzeni",ShellPageMargin+cardWidth+gap,146,cardWidth,176);
        flow.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="01  İstemciyi sol alttan seç\r\n02  Oyuna bağlan\r\n03  Sağ menüden çalışma modunu aç",Bounds=new Rectangle(16,62,cardWidth-32,88),Font=new Font("Segoe UI",9f),ForeColor=LightText,BackColor=Color.Transparent});

        var fixedInfo=OverviewCard(page,"SABİT RADAR PENCERESİ","Canlı harita tüm ekranlarda görünür",ShellPageMargin,338,width,120);
        fixedInfo.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text="Sol tık büyük haritayı açar. Sağ tık radar ölçeğini değiştirir. Rota, saldırı veya zindan sayfası açıkken radar kesintisiz izlenebilir.",Bounds=new Rectangle(16,62,width-32,42),Font=new Font("Segoe UI",8.75f),ForeColor=LightText,BackColor=Color.Transparent});
        ShellPolishLists(page);
    }

    private static ShellCard OverviewCard(Control page,string title,string subtitle,int x,int y,int w,int h)
    {
        var card=new ShellCard{Bounds=new Rectangle(x,y,w,h),BackColor=LightRaised};
        card.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,Text=title,Bounds=new Rectangle(16,14,w-32,16),Font=new Font("Segoe UI Semibold",7.75f),ForeColor=LightMuted,BackColor=Color.Transparent});
        card.Controls.Add(new Label{AutoSize=false,UseMnemonic=false,AutoEllipsis=true,Text=subtitle,Bounds=new Rectangle(16,32,w-32,18),Font=new Font("Segoe UI",8.5f),ForeColor=LightText,BackColor=Color.Transparent});
        page.Controls.Add(card);return card;
    }

    // Sayfaların düzeni: tekrar eden başlıklar gizlenir, Kontrol Merkezi sayfaları tek sütuna alınır,
    // kalan onay kutuları anahtara dönüşür ve her sayfa üstten alta düzgün aralıklarla yeniden dizilir.
    private void ShellPreparePages(List<TabPage> centerPages,List<TabPage> attackPages,TabPage sessionPage)
    {
        var duplicateTitles=new HashSet<string>{"Hareket ve hedef takibi","Hedef türünü ve hareket seçeneklerini belirle.","Saldırı ve toplama","Rota yönetimi","Rotanı kaydet, dosyadan aç ve tekrar izle.",
            "Beceriler ve envanter","Hesap ve karakter","Performans ve alan kontrolü","Otomatik uyumluluk"};
        var pages=new List<TabPage>{tabMain,tabRoute,inventoryPage,sessionPage,performancePage,compatibilityPage,taskPage};
        pages.AddRange(attackPages);pages.AddRange(centerPages);
        int avail=ShellAvailableRight();
        var splits=new Dictionary<string,int>{{"Kurallar",540},{"Hedef / Alan",530},{"Beceri / Envanter",575},{"Zindan Akışı",470},{"Günlük / İstatistik",730}};
        foreach(var page in pages.Where(p=>p!=null).Distinct())
        {
            foreach(Control c in page.Controls)if(c is Label l&&duplicateTitles.Contains(l.Text.Trim()))l.Visible=false;
            bool fromCenter=centerPages.Contains(page);
            if(fromCenter)
            {
                page.Font=new Font("Segoe UI",8.5f);
                int split;if(splits.TryGetValue(page.Text,out split))ShellStackColumns(page,split);
                foreach(Control c in page.Controls)
                {
                    if(c is ListBox||c is DataGridView)c.Height=Math.Min(c.Height,c is DataGridView?230:200);
                    if(c is Button b&&!(c is Guna2Button)){b.Height=30;b.Font=new Font("Segoe UI Semibold",8.25f);}
                    if(c is Label lab&&lab.Text.Length==0&&lab.Height>200)lab.Height=200;
                    if(c is Label big&&big.Font.Size>9.5f)big.Font=new Font("Segoe UI",9f,big.Font.Style);
                }
                ShellWrapRows(page,avail);
                ShellTrimCaptions(page);
            }
            foreach(Control c in page.Controls)
                if(c.Right>avail&&(c is Label||c is TextBox||c is ListBox||c is DataGridView||c is ComboBox||c is Guna2TrackBar||c is TargetHealthView))c.Width=Math.Max(60,avail-c.Left);
            ShellMirrorRemainingChecks(page);
            if(fromCenter)PolishInputs(page);
            ShellPolishLists(page);
            page.AutoScrollMinSize=Size.Empty;page.Padding=Padding.Empty;
            shellFlowPages.Add(page);ShellHookPage(page);
            ShellReflow(page);
        }
    }

    // Sayfaya sığmayan yan yana kontroller (ör. düğme sıraları) alt satıra kayar.
    private static void ShellWrapRows(Control page,int avail)
    {
        var items=page.Controls.Cast<Control>().OrderBy(c=>c.Top).ThenBy(c=>c.Left).ToList();
        var rows=new List<List<Control>>();
        foreach(var c in items){var row=rows.LastOrDefault();if(row!=null&&Math.Abs(c.Top-row[0].Top)<=8)row.Add(c);else rows.Add(new List<Control>{c});}
        foreach(var row in rows)
        {
            if(row.Max(c=>c.Right)<=avail)continue;
            var ordered=row.OrderBy(c=>c.Left).ToList();int start=ordered[0].Left,x=start,y=ordered[0].Top,lineHeight=0;
            int extra=0;
            foreach(var c in ordered)
            {
                if(x>start&&x+c.Width>avail){y+=lineHeight+8;extra+=lineHeight+8;x=start;lineHeight=0;}
                c.Location=new Point(x,y);x+=c.Width+10;lineHeight=Math.Max(lineHeight,c.Height);
            }
            if(extra>0)foreach(var c in items)if(!row.Contains(c)&&c.Top>row[0].Top)c.Top+=extra;
        }
    }

    // Yanındaki kontrolün altına uzanan kısa başlık etiketleri metinleri kadar daraltılır.
    private static void ShellTrimCaptions(Control page)
    {
        var all=page.Controls.Cast<Control>().ToList();
        foreach(var l in all.OfType<Label>())
        {
            if(l.Text.Length==0||l.Text.Contains("\n"))continue;
            int need=TextRenderer.MeasureText(l.Text,l.Font).Width+6;if(need>=l.Width)continue;
            if(all.Any(o=>o!=l&&o.Left>l.Left&&o.Left<l.Right&&o.Top<l.Bottom&&l.Top<o.Bottom))l.Width=need;
        }
    }

    // Düz açılır listeler ve seçimi yumuşak vurgulu listeler.
    private void ShellPolishLists(Control parent)
    {
        foreach(Control c in parent.Controls)
        {
            if(c is ComboBox combo)
            {
                combo.FlatStyle=FlatStyle.Flat;
                if(combo.DropDownStyle==ComboBoxStyle.DropDownList&&combo.DrawMode==DrawMode.Normal)
                {
                    combo.DrawMode=DrawMode.OwnerDrawFixed;combo.ItemHeight=Math.Max(18,combo.Font.Height+4);
                    combo.DrawItem+=(s,e)=>
                    {
                        var cb=(ComboBox)s;if(e.Index<0)return;
                        bool edit=(e.State&DrawItemState.ComboBoxEdit)!=0,selected=!edit&&(e.State&DrawItemState.Selected)!=0;
                        using(var back=new SolidBrush(selected?ShellTheme.AccentSoft:cb.BackColor))e.Graphics.FillRectangle(back,e.Bounds);
                        TextRenderer.DrawText(e.Graphics,cb.GetItemText(cb.Items[e.Index]),cb.Font,new Rectangle(e.Bounds.X+4,e.Bounds.Y,e.Bounds.Width-4,e.Bounds.Height),
                            cb.Enabled?ShellTheme.Text:ShellTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
                    };
                }
            }
            else if(c is ListBox list&&list.DrawMode==DrawMode.Normal)
            {
                list.DrawMode=DrawMode.OwnerDrawFixed;list.ItemHeight=22;list.IntegralHeight=false;list.BorderStyle=BorderStyle.FixedSingle;
                list.DrawItem+=(s,e)=>
                {
                    var lb=(ListBox)s;
                    using(var back=new SolidBrush(lb.BackColor))e.Graphics.FillRectangle(back,e.Bounds);
                    if(e.Index<0)return;
                    bool selected=(e.State&DrawItemState.Selected)!=0;
                    if(selected)using(var soft=new SolidBrush(ShellTheme.AccentSoft))e.Graphics.FillRectangle(soft,e.Bounds);
                    if(selected)using(var bar=new SolidBrush(ShellTheme.Accent))e.Graphics.FillRectangle(bar,e.Bounds.X,e.Bounds.Y+4,3,e.Bounds.Height-8);
                    TextRenderer.DrawText(e.Graphics,lb.GetItemText(lb.Items[e.Index]),lb.Font,new Rectangle(e.Bounds.X+10,e.Bounds.Y,e.Bounds.Width-14,e.Bounds.Height),
                        lb.Enabled?ShellTheme.Text:ShellTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
                };
                list.SelectedIndexChanged+=(s,e)=>((ListBox)s).Invalidate();
            }
            if(!(c is DataGridView))ShellPolishLists(c);
        }
    }

    private int ShellAvailableRight(){return ShellWindowWidth-2-ShellSidebarWidth-ShellRadarWidth-SystemInformation.VerticalScrollBarWidth-ShellPageMargin;}

    // İki sütunlu Kontrol Merkezi sayfası: sağ sütun sol sütunun altına taşınır.
    private static void ShellStackColumns(Control page,int split)
    {
        var all=page.Controls.Cast<Control>().ToList();
        var left=all.Where(c=>c.Left<split).ToList();var rightCol=all.Where(c=>c.Left>=split).ToList();
        if(left.Count==0||rightCol.Count==0)return;
        int leftMin=left.Min(c=>c.Left),leftBottom=left.Max(c=>c.Bottom),rightTop=rightCol.Min(c=>c.Top),rightMin=rightCol.Min(c=>c.Left);
        foreach(var c in rightCol){c.Left=c.Left-rightMin+leftMin;c.Top=c.Top-rightTop+leftBottom+28;}
    }

    // Henüz anahtara dönüşmemiş görünür onay kutuları da aynı anahtar görünümünü alır (kutu arkada çalışmaya devam eder).
    private void ShellMirrorRemainingChecks(Control parent)
    {
        foreach(Control c in parent.Controls.Cast<Control>().ToList())
        {
            if((c is CheckBox||c is Guna2CheckBox)&&OwnVisible(c)&&!(parent is DataGridView))MirrorCheckBox(c);
            else if(!(c is DataGridView)&&!(c is TabControl))ShellMirrorRemainingChecks(c);
        }
    }

    private void ShellHookPage(TabPage page)
    {
        page.ControlAdded+=(s,e)=>{ShellHookChild(page,e.Control);ShellQueueReflow(page);};
        page.ControlRemoved+=(s,e)=>ShellQueueReflow(page);
        foreach(Control c in page.Controls)ShellHookChild(page,c);
    }
    private void ShellHookChild(TabPage page,Control c)
    {
        if(!shellHooked.Add(c))return;
        c.VisibleChanged+=(s,e)=>ShellQueueReflow(page);
        c.SizeChanged+=(s,e)=>ShellQueueReflow(page);
    }
    private void ShellQueueReflow(Control page)
    {
        if(!IsHandleCreated||!shellReflowPending.Add(page))return;
        BeginInvoke((Action)(()=>{shellReflowPending.Remove(page);if(!page.IsDisposed)ShellReflow(page);}));
    }

    // Satır satır yeniden dizme: aynı hizadaki kontroller satır olarak birlikte kalır; her satır, üstünde yatay olarak
    // çakıştığı en alttaki kontrolün hemen altına iner (aralıklar 4–18 px). Böylece üst üste binmeler ve büyük boşluklar kalkar.
    private void ShellReflow(Control page)
    {
        var kids=page.Controls.Cast<Control>().ToList();
        if(kids.Count==0)return;
        page.SuspendLayout();
        try
        {
            // Anahtar ve yazısı aynı hizada dursun.
            foreach(var sw in kids.OfType<ToggleSwitch>())
            {
                if(sw.Tag==null)continue;
                var caption=kids.OfType<Label>().FirstOrDefault(l=>ReferenceEquals(l.Tag,sw.Tag));
                if(caption!=null){caption.Top=sw.Top+(sw.Height-caption.Height)/2;caption.Left=sw.Right+8;}
            }
            var orig=kids.ToDictionary(c=>c,c=>c.Bounds);
            var visible=kids.Where(c=>OwnVisible(c)&&c.Width>2&&c.Height>2).OrderBy(c=>c.Top).ThenBy(c=>c.Left).ToList();
            if(visible.Count==0)return;
            int originY=page is ScrollableControl sc?sc.AutoScrollPosition.Y:0,originX=page is ScrollableControl sx?sx.AutoScrollPosition.X:0;
            int shiftX=originX+ShellPageMargin-visible.Min(c=>c.Left);
            var rows=new List<List<Control>>();
            foreach(var c in visible)
            {
                var row=rows.LastOrDefault();
                if(row!=null&&c.Top<=orig[row[0]].Top+8&&c.Top<orig[row[0]].Bottom&&!row.Any(m=>orig[m].Left<c.Right&&c.Left<orig[m].Right))row.Add(c);else rows.Add(new List<Control>{c});
            }
            var placed=new List<Control>();var newBounds=new Dictionary<Control,Rectangle>();
            int previousTop=originY+18;
            foreach(var row in rows)
            {
                int rowOrigTop=row.Min(c=>orig[c].Top),rowTop=previousTop;
                foreach(var m in row)
                {
                    var mo=orig[m];int offset=mo.Top-rowOrigTop;
                    foreach(var q in placed)
                    {
                        var qo=orig[q];var qn=newBounds[q];
                        if(mo.Left+shiftX>=qn.Right||qn.Left>=mo.Right+shiftX)continue;
                        int gap=Math.Max(4,Math.Min(18,mo.Top-qo.Bottom));
                        rowTop=Math.Max(rowTop,qn.Bottom+gap-offset);
                    }
                }
                foreach(var m in row){var mo=orig[m];newBounds[m]=new Rectangle(mo.Left+shiftX,rowTop+(mo.Top-rowOrigTop),mo.Width,mo.Height);placed.Add(m);}
                previousTop=rowTop;
            }
            foreach(var pair in newBounds)if(pair.Key.Bounds!=pair.Value)pair.Key.Bounds=pair.Value;
            // Gizli kontroller en yakın üstteki görünür kontrolle birlikte kayar; görünür olduklarında doğru yerde açılırlar.
            foreach(var h in kids.Where(c=>!newBounds.ContainsKey(c)))
            {
                var ho=orig[h];
                var anchor=visible.Where(v=>orig[v].Top<=ho.Top).OrderByDescending(v=>orig[v].Top).FirstOrDefault();
                var next=visible.Where(v=>orig[v].Top>ho.Top).OrderBy(v=>orig[v].Top).FirstOrDefault();
                int top;
                if(anchor!=null&&next!=null&&orig[next].Top>orig[anchor].Top)
                {
                    // Sıra korunur: gizli kontrol, üstündeki ve altındaki görünür kontrollerin arasında kalır.
                    double t=(ho.Top-orig[anchor].Top)/(double)(orig[next].Top-orig[anchor].Top);
                    top=newBounds[anchor].Top+(int)Math.Round(t*(newBounds[next].Top-newBounds[anchor].Top));
                    top=Math.Min(top,newBounds[next].Top-1);
                }
                else if(anchor!=null)top=ho.Top+newBounds[anchor].Top-orig[anchor].Top;
                else top=Math.Min(ho.Top,newBounds[next].Top-1);
                h.Location=new Point(ho.Left+shiftX,top);
            }
            // Kaydırma alanı içeriğin altında biraz boşluk bırakır.
            int bottom=kids.Max(c=>c.Bottom)-originY;
            var size=new Size(0,bottom+24);if(page is ScrollableControl scroller&&scroller.AutoScrollMinSize!=size)scroller.AutoScrollMinSize=size;
        }
        finally{page.ResumeLayout(true);}
    }

    private void ShellSelect(int section,int sub)
    {
        if(section<0||section>=shellSections.Count)return;
        var s=shellSections[section];if(s.Subs.Count==0)return;
        sub=Math.Max(0,Math.Min(sub,s.Subs.Count-1));var target=s.Subs[sub];
        shellSyncing=true;
        try
        {
            if(target.Inner!=null&&target.Inner.SelectedTab!=target.InnerPage)target.Inner.SelectedTab=target.InnerPage;
            if(mainTabs.SelectedTab!=target.Page)mainTabs.SelectedTab=target.Page;
        }
        finally{shellSyncing=false;}
        ShellShow(section,sub);
        UpdateContextActionButton();
    }

    private void ShellShow(int section,int sub)
    {
        var s=shellSections[section];s.LastSub=sub;
        bool changed=shellCurrent!=section;shellCurrent=section;
        shellNav.SelectedIndex=section;
        shellTitle.Text=s.Title;shellSubtitle.Text=s.Subtitle;
        if(changed||shellTabs.SelectedIndex!=sub)shellTabs.SetItems(s.Subs.Select(x=>x.Title),sub);
        bool withTabs=s.Subs.Count>1;if(shellTabs.Visible!=withTabs||changed)ShellLayoutHost(withTabs);
    }

    // Kod bir sekmeyi kendisi seçerse (ör. "İzleme ve test"i açan düğme) menü de onu gösterir.
    private void ShellSyncFromTabs()
    {
        if(shellSyncing||shellNav==null)return;
        for(int pass=0;pass<2;pass++)
            for(int i=0;i<shellSections.Count;i++)
                for(int j=0;j<shellSections[i].Subs.Count;j++)
                {
                    var x=shellSections[i].Subs[j];
                    if(x.Page!=mainTabs.SelectedTab)continue;
                    if(pass==0&&x.Inner!=null&&x.Inner.SelectedTab!=x.InnerPage)continue;
                    ShellShow(i,j);return;
                }
    }

    private void ShellUpdateStatus()
    {
        if(shellStatus==null)return;
        string pid=pidLbl!=null?pidLbl.Text.Trim():"";
        shellStatus.SetState(clientConnected,clientConnected?("Bağlı"+(pid.Length>0&&pid!="0"?" · PID "+pid:"")):"Bağlı değil");
    }

    private void ShellApplyTheme()
    {
        bool night=nightTheme;ShellTheme.Night=night;
        ShellTheme.Page=night?NightPage:LightPage;ShellTheme.Surface=night?NightSurface:LightSurface;ShellTheme.Raised=night?NightRaised:LightRaised;
        ShellTheme.Hover=night?Color.FromArgb(28,40,58):Color.FromArgb(238,240,243);ShellTheme.Border=night?NightBorder:LightBorder;
        ShellTheme.Text=night?NightText:LightText;ShellTheme.Muted=night?NightMuted:LightMuted;
        ShellTheme.Accent=night?NightAccent:LightAccent;ShellTheme.AccentSoft=night?NightAccentSoft:LightAccentSoft;
        BackColor=ShellTheme.Border;
        headerPanel.FillColor=ShellTheme.Surface;headerPanel.BackColor=ShellTheme.Surface;
        shellSidebar.BackColor=ShellTheme.Page;shellNav.BackColor=ShellTheme.Page;shellActions.BackColor=ShellTheme.Page;shellRadarPane.BackColor=ShellTheme.Page;
        foreach(var c in new Control[]{shellContent,shellPageHost,shellTabs})c.BackColor=ShellTheme.Surface;
        if(shellAttackHost!=null)shellAttackHost.BackColor=ShellTheme.Surface;
        shellTitle.ForeColor=ShellTheme.Text;shellSubtitle.ForeColor=ShellTheme.Muted;
        foreach(TabPage p in mainTabs.TabPages)p.BackColor=ShellTheme.Surface;
        if(attackSections!=null)foreach(TabPage p in attackSections.TabPages)p.BackColor=ShellTheme.Surface;
        clientPicker.BackColor=night?NightInput:Color.White;
        emergencyButton.BackColor=ShellTheme.Danger;emergencyButton.ForeColor=Color.White;
        emergencyButton.FlatAppearance.MouseOverBackColor=ShellTheme.DangerHover;emergencyButton.FlatAppearance.MouseDownBackColor=ShellTheme.DangerHover;
        emergencyStatus.ForeColor=ShellTheme.Muted;
        guna2ControlBox2.HoverState.FillColor=ShellTheme.Hover;guna2ControlBox2.HoverState.IconColor=ShellTheme.Text;
        guna2ControlBox1.HoverState.FillColor=ShellTheme.Danger;guna2ControlBox1.HoverState.IconColor=Color.White;
        foreach(var box in new[]{guna2ControlBox1,guna2ControlBox2}){box.FillColor=ShellTheme.Surface;box.IconColor=ShellTheme.Muted;}
        shellThemeButton.Icon=night?21:20;
        foreach(var c in new Control[]{shellSidebar,shellNav,shellActions,shellRadarPane,shellContent,shellPageHost,shellTabs,shellBrand,shellStatus,shellThemeButton,headerPanel})c.Invalidate(true);
        if(shellOverviewPage!=null)shellOverviewPage.Invalidate(true);
    }
}

