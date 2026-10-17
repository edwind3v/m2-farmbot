using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

// Presentation only. Existing controls retain their handlers, values and state text.
public partial class MainForm
{
    // Light palette (the compact theme pass in MainForm.Theme.cs maps the remaining hard-coded colours to it).
    private static readonly Color StudioBackground = Color.FromArgb(243, 246, 251);
    private static readonly Color StudioSurface = Color.FromArgb(255, 255, 255);
    private static readonly Color StudioRaised = Color.FromArgb(247, 249, 253);
    private static readonly Color StudioBorder = Color.FromArgb(216, 223, 235);
    private static readonly Color StudioAccent = Color.FromArgb(57, 72, 171);
    private static readonly Color StudioText = Color.FromArgb(15, 23, 42);
    private static readonly Color StudioMuted = Color.FromArgb(100, 116, 139);

    private void ApplyStudioTemplate()
    {
        SuspendLayout();
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(1080, 820);
        MinimumSize = Size;
        MaximumSize = Size;
        BackColor = StudioBackground;
        Font = new Font("Segoe UI", 10f);
        ForeColor = StudioText;

        headerPanel.Dock = DockStyle.None;
        headerPanel.SetBounds(0, 0, 1080, 88);
        headerPanel.FillColor = StudioSurface;
        headerPanel.BackColor = StudioSurface;
        rndTitleLbl.SetBounds(28, 21, 440, 28);
        rndTitleLbl.Font = new Font("Segoe UI Semibold", 17f);
        rndTitleLbl.ForeColor = StudioText;
        rndTitleLbl.AutoSize = false;
        guna2HtmlLabel1.SetBounds(744, 29, 38, 24);
        guna2HtmlLabel1.Font = new Font("Segoe UI", 10f);
        guna2HtmlLabel1.ForeColor = StudioMuted;
        pidLbl.SetBounds(788, 29, 125, 24);
        pidLbl.AutoSize = false;
        pidLbl.ForeColor = StudioAccent;
        pidLbl.Font = new Font("Consolas", 11f);
        guna2ControlBox2.SetBounds(966, 22, 42, 32);
        guna2ControlBox1.SetBounds(1016, 22, 42, 32);
        foreach (var box in new[] { guna2ControlBox1, guna2ControlBox2 })
        {
            box.FillColor = StudioSurface;
            box.IconColor = StudioMuted;
        }
        AddCaption(headerPanel, "ONUR  /  KONTROL MERKEZİ", 29, 59, 520, 17, 8f, StudioMuted);

        leftPanel.Dock = DockStyle.None;
        leftPanel.SetBounds(24, 112, 324, 682);
        leftPanel.FillColor = StudioSurface;
        leftPanel.BackColor = StudioSurface;
        leftPanel.BorderRadius = 12;
        AddCaption(leftPanel, "CANLI RADAR", 22, 21, 270, 24, 12f, StudioText);
        AddCaption(leftPanel, "Konum ve yakındaki hedefler", 22, 50, 270, 20, 9f, StudioMuted);
        radarPanel.SetBounds(22, 89, 280, 280);
        radarPanel.BackColor = Color.FromArgb(8, 10, 18);
        connectBtn.SetBounds(22, 398, 280, 46);
        startStopFarmBtn.SetBounds(22, 458, 280, 46);
        AddCaption(leftPanel, "Oyuna bağlan → Kellam görevini başlat", 22, 524, 280, 20, 8.5f, StudioMuted);

        mainTabs.Dock = DockStyle.None;
        mainTabs.SetBounds(372, 112, 684, 682);
        mainTabs.Alignment = TabAlignment.Top;
        mainTabs.ItemSize = new Size(222, 48);
        mainTabs.TabMenuBackColor = StudioBackground;
        mainTabs.TabButtonIdleState.FillColor = StudioSurface;
        mainTabs.TabButtonIdleState.ForeColor = StudioMuted;
        mainTabs.TabButtonIdleState.Font = new Font("Segoe UI Semibold", 10f);
        mainTabs.TabButtonSelectedState.FillColor = StudioSurface;
        mainTabs.TabButtonSelectedState.ForeColor = StudioAccent;
        mainTabs.TabButtonSelectedState.InnerColor = StudioAccent;
        mainTabs.TabButtonSelectedState.Font = new Font("Segoe UI Semibold", 10f);
        mainTabs.TabButtonHoverState.FillColor = Color.FromArgb(32, 45, 62);
        mainTabs.TabButtonHoverState.ForeColor = StudioText;
        foreach (TabPage page in mainTabs.TabPages)
        {
            page.BackColor = StudioSurface;
            page.ForeColor = StudioText;
            page.Padding = new Padding(20);
            page.AutoScroll = true;
        }
        tabMain.Text = "01   Genel";
        tabDamage.Text = "02   Saldırı ve Toplama";
        tabRoute.Text = "03   Rota";

        AddCaption(tabMain, "Hareket ve hedef takibi", 24, 24, 570, 34, 18f, StudioText);
        AddCaption(tabMain, "Hedef türünü ve hareket seçeneklerini belirle.", 24, 68, 610, 24, 10f, StudioMuted);
        AddCaption(tabMain, "YÜRÜME HEDEFİ", 24, 126, 280, 24, 9f, StudioAccent);
        Place(walkMetinCheck, 24, 168, 280);
        Place(walkCanavarCheck, 344, 168, 280);
        AddCaption(tabMain, "ENGEL SEÇENEKLERİ", 24, 244, 280, 24, 9f, StudioAccent);
        Place(wallCheck, 24, 286, 280);
        Place(binekWallCheck, 344, 286, 280);
        AddCaption(tabMain, "Takibi radar panelinden başlatıp durdurabilirsin.", 24, 394, 600, 30, 10f, StudioMuted);

        AddCaption(tabDamage, "Saldırı ve toplama", 24, 20, 580, 32, 18f, StudioText);
        Place(targetDmgRangeCheck, 24, 74, 300);
        Place(waitDmgRangeCheck, 344, 74, 300);
        Place(targetDmgCheck, 24, 112, 300);
        Place(waitDmgCheck, 344, 112, 300);
        Place(targetArrowCheck, 24, 150, 300);
        Place(waitArrowCheck, 344, 150, 300);
        Place(dmgStoneCheck, 24, 206, 180);
        Place(dmgMonsterCheck, 224, 206, 180);
        Place(dmgPlayerCheck, 424, 206, 180);
        Place(dmgStartCheck, 24, 252, 270);
        attackRangeLbl.SetBounds(24, 304, 300, 22);
        attackRangeBar.SetBounds(24, 334, 290, 24);
        attackDelayLbl.SetBounds(344, 304, 300, 22);
        attackDelayBar.SetBounds(344, 334, 290, 24);
        Place(autoPickCheck, 24, 402, 290);
        pickDelayLbl.SetBounds(344, 388, 290, 22);
        pickDelayBar.SetBounds(344, 420, 290, 24);

        AddCaption(tabRoute, "Rota yönetimi", 24, 20, 600, 34, 18f, StudioText);
        AddCaption(tabRoute, "Rotanı kaydet, dosyadan aç ve tekrar izle.", 24, 64, 610, 24, 10f, StudioMuted);
        addRouteBtn.SetBounds(24, 112, 194, 46);
        clearRouteBtn.SetBounds(234, 112, 194, 46);
        startRouteMoveBtn.SetBounds(444, 112, 194, 46);
        saveRouteBtn.SetBounds(24, 176, 300, 44);
        loadRouteBtn.SetBounds(340, 176, 298, 44);
        AddCaption(tabRoute, "ROTA SEÇENEKLERİ", 24, 258, 580, 22, 9f, StudioAccent);
        Place(RotaMetinCheck, 24, 302, 290);
        Place(RotaCanavarCheck, 344, 302, 290);
        Place(farmOnRouteCheckBox, 24, 352, 290);
        farmRangeLabel.SetBounds(24, 410, 570, 24);
        farmRangeTrackBar.SetBounds(24, 444, 614, 26);

        StyleControls(this);
        ResumeLayout(true);
    }

    private static void Place(Control control, int x, int y, int width)
    {
        control.AutoSize = false;
        control.SetBounds(x, y, width, 30);
    }

    private static void AddCaption(Control parent, string text, int x, int y, int width, int height, float size, Color color)
    {
        parent.Controls.Add(new Label {
            UseMnemonic = false, Text = text, Location = new Point(x, y), Size = new Size(width, height),
            Font = new Font("Segoe UI", size), ForeColor = color, BackColor = Color.Transparent
        });
    }

    private static void StyleControls(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Guna2Button button)
            {
                button.BorderRadius = 8;
                button.FillColor = StudioAccent;
                button.ForeColor = StudioBackground;
                button.Font = new Font("Segoe UI Semibold", 10f);
                button.HoverState.FillColor = Color.FromArgb(43, 55, 142);
            }
            if (c is Guna2CheckBox check)
            {
                check.Font = new Font("Segoe UI", 10f);
                check.ForeColor = StudioText;
                check.CheckedState.FillColor = StudioAccent;
                check.CheckedState.BorderColor = StudioAccent;
                check.UncheckedState.BorderThickness = 1;
                check.UncheckedState.BorderRadius = 3;
                check.CheckedState.BorderThickness = 1;
                check.CheckedState.BorderRadius = 3;
                check.UncheckedState.FillColor = StudioSurface;
                check.UncheckedState.BorderColor = StudioMuted;
            }
            if (c is Guna2TrackBar track)
            {
                track.ThumbColor = StudioAccent;
                track.FillColor = Color.FromArgb(53, 68, 87);
            }
            if (c is Label label && !string.IsNullOrEmpty(label.Name) && label.Name != "rndTitleLbl" && label.Name != "pidLbl")
            {
                label.ForeColor = StudioMuted;
                label.Font = new Font("Segoe UI", 10f);
                label.AutoSize = false;
            }
            StyleControls(c);
        }
    }
    private void FinalizeProfessionalLayout()
    {
        SuspendLayout();
        MinimumSize=Size.Empty;MaximumSize=Size.Empty;ClientSize=new Size(1280,860);MinimumSize=Size;MaximumSize=Size;
        headerPanel.SetBounds(0,0,1280,86);headerPanel.FillColor=Color.FromArgb(11,18,31);headerPanel.BackColor=Color.FromArgb(11,18,31);
        rndTitleLbl.Text="ONUR  CONTROL SUITE";rndTitleLbl.SetBounds(30,18,500,30);rndTitleLbl.Font=new Font("Segoe UI Semibold",18f);
        foreach(Control control in headerPanel.Controls)
            if(control is Label && control!=rndTitleLbl && control!=pidLbl && control.Top>48)control.Text="OTOMASYON  /  OPERASYON  /  İZLEME";
        guna2HtmlLabel1.SetBounds(1010,28,38,24);pidLbl.SetBounds(1050,28,100,24);
        guna2ControlBox2.SetBounds(1170,21,42,34);guna2ControlBox1.SetBounds(1220,21,42,34);

        leftPanel.SetBounds(20,106,286,730);leftPanel.BorderRadius=14;leftPanel.FillColor=StudioSurface;leftPanel.BorderColor=StudioBorder;leftPanel.BorderThickness=1;
        radarPanel.SetBounds(20,88,246,246);radarPanel.BackColor=Color.FromArgb(6,10,19);
        connectBtn.SetBounds(20,370,246,48);startStopFarmBtn.SetBounds(20,432,246,48);
        connectionStatus?.SetBounds(20,506,246,48);
        var timing=leftPanel.Controls.Find("timingStatus",true);if(timing.Length>0)timing[0].SetBounds(20,570,246,100);
        foreach(Control c in leftPanel.Controls)if(c is Label && c.Top==524)c.Visible=false;

        mainTabs.SetBounds(330,106,930,730);mainTabs.Alignment=TabAlignment.Left;mainTabs.ItemSize=new Size(188,54);mainTabs.Padding=new Point(18,7);
        mainTabs.TabMenuBackColor=Color.FromArgb(11,18,31);
        mainTabs.TabButtonIdleState.FillColor=Color.FromArgb(11,18,31);mainTabs.TabButtonIdleState.ForeColor=StudioMuted;
        mainTabs.TabButtonSelectedState.FillColor=StudioRaised;mainTabs.TabButtonSelectedState.ForeColor=StudioAccent;mainTabs.TabButtonSelectedState.InnerColor=StudioAccent;
        mainTabs.TabButtonHoverState.FillColor=Color.FromArgb(24,37,58);mainTabs.TabButtonHoverState.ForeColor=StudioText;
        string[] names={"GENEL","SALDIRI","ROTA","ENVANTER","OTURUM","PERFORMANS","GELİŞTİRİCİ","UYUMLULUK","GÖREV KAYDI"};
        for(int i=0;i<mainTabs.TabPages.Count&&i<names.Length;i++){
            mainTabs.TabPages[i].Text=(i+1).ToString("00")+"   "+names[i];
            mainTabs.TabPages[i].BackColor=StudioSurface;mainTabs.TabPages[i].Padding=new Padding(22);mainTabs.TabPages[i].AutoScroll=true;
        }

        StyleProfessionalTree(this);
        EnsureEnhancementControlsVisible();
        ResumeLayout(true);
    }

    private static void StyleProfessionalTree(Control parent)
    {
        foreach(Control c in parent.Controls)
        {
            if(c is Guna2Button gb){gb.BorderRadius=8;gb.FillColor=StudioAccent;gb.ForeColor=Color.White;gb.Font=new Font("Segoe UI Semibold",9.5f);gb.HoverState.FillColor=Color.FromArgb(43,55,142);}
            else if(c is Button b){b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=StudioBorder;b.FlatAppearance.BorderSize=1;b.BackColor=StudioRaised;b.ForeColor=StudioText;b.Font=new Font("Segoe UI Semibold",9f);}
            else if(c is TextBox tb){tb.BackColor=Color.FromArgb(10,17,29);tb.ForeColor=StudioText;tb.BorderStyle=BorderStyle.FixedSingle;}
            else if(c is NumericUpDown n){n.BackColor=Color.FromArgb(10,17,29);n.ForeColor=StudioText;}
            else if(c is DataGridView grid){grid.BackgroundColor=Color.FromArgb(9,15,26);grid.GridColor=StudioBorder;grid.DefaultCellStyle.BackColor=StudioSurface;grid.AlternatingRowsDefaultCellStyle.BackColor=StudioRaised;grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(10,17,29);grid.ColumnHeadersDefaultCellStyle.ForeColor=StudioAccent;grid.RowTemplate.Height=28;}
            StyleProfessionalTree(c);
        }
    }
}






