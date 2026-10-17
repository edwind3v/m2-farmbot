using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private TabPage inventoryPage;
    private DataGridView skillGrid, inventoryGrid;
    private Label inventoryStatus, inventoryAutomationStatus;
    private Button inventoryRefresh;
    private CheckBox inventoryLive;
    private bool inventoryReading;
    private TextBox skillSearch, inventorySearch;
    private readonly List<DetectedSkill> visibleSkills=new List<DetectedSkill>();
    private readonly List<DetectedItem> visibleItems=new List<DetectedItem>();

    private void InitializeInventoryPanel()
    {
        mainTabs.ItemSize = new Size(165, 48);
        tabMain.Text = "Genel"; tabDamage.Text = "Saldırı ve Toplama"; tabRoute.Text = "Rota";
        inventoryPage = new TabPage("Beceri / Envanter") { BackColor=StudioSurface, ForeColor=StudioText, AutoScroll=true, AutoScrollMinSize=new Size(0,820) };
        mainTabs.TabPages.Add(inventoryPage);
        AddCaption(inventoryPage, "Beceriler ve envanter", 20, 16, 610, 34, 18f, StudioText);
        inventoryRefresh = new Button { Text = "Oyundan oku", Location = new Point(20, 60), Size = new Size(150, 34), FlatStyle = FlatStyle.Flat, BackColor = StudioSurface, ForeColor = StudioAccent };
        inventoryRefresh.Click += async (s, e) => await RefreshInventoryAsync();
        inventoryLive = new CheckBox { Text = "5 saniyede bir yenile", Location = new Point(190, 65), AutoSize = true, ForeColor = StudioText };
        inventoryStatus = new Label { Text = "Bağlandıktan sonra oyundan oku düğmesine basın.", Location = new Point(20, 102), Size = new Size(630, 40), ForeColor = StudioMuted };
        inventoryPage.Controls.AddRange(new Control[] { inventoryRefresh, inventoryLive, inventoryStatus });
        AddCaption(inventoryPage,"BECERİLER",20,147,150,22,9f,StudioAccent);
        skillSearch=SearchBox("skillSearch",310,140);skillSearch.TextChanged+=(s,e)=>PopulateSkillGrid();
        skillGrid=CreateInventoryGrid(20,178,630,150);
        skillGrid.Columns.Add("slot", "Yuva"); skillGrid.Columns.Add("name", "Beceri"); skillGrid.Columns.Add("level", "Seviye"); skillGrid.Columns.Add("grade", "Kademe"); skillGrid.Columns.Add("state", "Durum"); skillGrid.Columns.Add("cooldown", "Toplam süre (sn)");
        skillGrid.Columns[1].FillWeight = 210;
        AddCaption(inventoryPage,"ENVANTER VE TAKILI EŞYALAR",20,420,260,22,9f,StudioAccent);
        inventorySearch=SearchBox("inventorySearch",310,413);inventorySearch.TextChanged+=(s,e)=>PopulateInventoryGrid();
        inventoryGrid=CreateInventoryGrid(20,451,630,150);
        inventoryGrid.Columns.Add("slot", "Yuva"); inventoryGrid.Columns.Add("name", "Eşya"); inventoryGrid.Columns.Add("id", "Eşya ID (VNUM)"); inventoryGrid.Columns.Add("count", "Adet"); inventoryGrid.Columns[1].FillWeight = 300;
        var timer = new System.Windows.Forms.Timer(components) { Interval = 5000 };
        timer.Tick += async (s, e) => { if (inventoryLive.Checked && mainTabs.SelectedTab == inventoryPage) await RefreshInventoryAsync(); };
        timer.Start();
    }
    private TextBox SearchBox(string name,int x,int y)
    {
        var box=new TextBox{Name=name,Left=x,Top=y,Width=340,Height=28,BackColor=StudioBackground,ForeColor=StudioText,BorderStyle=BorderStyle.FixedSingle};
        inventoryPage.Controls.Add(box);return box;
    }
    private DataGridView CreateInventoryGrid(int x, int y, int width, int height)
    {
        var grid = new DataGridView { Location = new Point(x, y), Size = new Size(width, height), ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false, BackgroundColor = StudioBackground, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9f) };
        grid.DefaultCellStyle.BackColor = StudioSurface; grid.DefaultCellStyle.ForeColor = StudioText;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 73, 76);
        grid.DefaultCellStyle.SelectionForeColor = StudioText;
        grid.ColumnHeadersDefaultCellStyle.BackColor = StudioBackground; grid.ColumnHeadersDefaultCellStyle.ForeColor = StudioAccent;
        inventoryPage.Controls.Add(grid); return grid;
    }
    private async Task RefreshInventoryAsync()
    {
        if (inventoryReading) return;
        inventoryReading = true; inventoryRefresh.Enabled = false;
        inventoryStatus.Text = "Oyundan okunuyor…";
        skillGrid.Rows.Clear(); inventoryGrid.Rows.Clear();
        try
        {
            var process = memory.Field_0197.Property_0268;
            if (process == null || process.HasExited) throw new InvalidOperationException("Önce sol panelden oyuna bağlanın.");
            int pid = process.Id;
            var snapshot = IsX86ClientMode
                ? await CaptureX86InventoryAsync()
                : await Task.Run(() => { using (var reader = new PlayerInventoryReader(process)) return reader.Capture(); });
            if (IsDisposed || Disposing) return;
            if (memory.Field_0197.Property_0268 == null || memory.Field_0197.Property_0268.Id != pid || process.HasExited)
                throw new InvalidOperationException("Oyun bağlantısı değişti; yeniden okuyun.");
            foreach(var item in snapshot.Items)item.Name=PackItemName(item.Id,item.Name);
            visibleSkills.Clear();visibleSkills.AddRange(snapshot.Skills);
            visibleItems.Clear();visibleItems.AddRange(snapshot.Items);
            ObserveInventorySnapshot(snapshot);
            PopulateSkillGrid();PopulateInventoryGrid();
            inventoryStatus.Text = snapshot.Skills.Count + " beceri, " + snapshot.Items.Count + " dolu eşya yuvası • " + snapshot.CapturedAt.ToString("HH:mm:ss") + "\nToplam süre kalan süre değildir. Yuva numaraları oyunun iç numaralarıdır.";
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing) { visibleSkills.Clear();visibleItems.Clear();skillGrid.Rows.Clear(); inventoryGrid.Rows.Clear(); inventoryStatus.Text = ex.Message; inventoryLive.Checked = false; }
        }
        finally { inventoryReading = false; if (!IsDisposed && !Disposing) inventoryRefresh.Enabled = true; }
    }
    private static bool MatchesSearch(string query,params object[] values)
    {
        if(string.IsNullOrWhiteSpace(query))return true;string q=query.Trim();
        return values.Any(v=>(v==null?"":v.ToString()).IndexOf(q,StringComparison.CurrentCultureIgnoreCase)>=0);
    }
    private void PopulateSkillGrid()
    {
        if(skillGrid==null)return;skillGrid.Rows.Clear();string q=skillSearch?.Text??"";
        foreach(var skill in visibleSkills.Where(v=>MatchesSearch(q,v.Name,v.Id,v.Slot,v.Level,v.Grade)))
        {int row=skillGrid.Rows.Add(skill.Slot,skill.Name,skill.Level,skill.Grade,skill.Active?"Etkin":skill.Level==0?"Seviye yok":"Etkin değil",skill.Cooldown.ToString("0.##"));skillGrid.Rows[row].Tag=skill;}
        MarkScheduledRows();
    }
    private void PopulateInventoryGrid()
    {
        if(inventoryGrid==null)return;inventoryGrid.Rows.Clear();string q=inventorySearch?.Text??"";
        foreach(var item in visibleItems.Where(v=>MatchesSearch(q,v.Name,v.Id,v.Slot,v.Count)))
        {int row=inventoryGrid.Rows.Add(item.Slot,item.Name,item.Id,item.Count);inventoryGrid.Rows[row].Tag=item;}
        MarkScheduledRows();
    }
}

