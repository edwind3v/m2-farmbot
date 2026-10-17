using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;

public partial class MainForm
{
    private Guna2CheckBox approachOnlyCheck;
    private Guna2CheckBox fastTimingCheck;
    private NumericUpDown approachDistance;
    private Label farmStatus;
    private System.Windows.Forms.Timer movementTimer;
    private readonly ApproachPolicy approachPolicy = new ApproachPolicy();
    private int approachTargetId;
    private Guna2CheckBox searchWhenEmptyCheck;
    private NumericUpDown searchRange;
    private readonly SearchPolicy searchPolicy = new SearchPolicy();
    private readonly System.Diagnostics.Stopwatch movementClock = System.Diagnostics.Stopwatch.StartNew();
    private bool nearPulse;
    private long farmRetryAfter;
    private readonly System.Collections.Generic.Dictionary<int,long> blockedTargets = new System.Collections.Generic.Dictionary<int,long>();
    private MovementOutput movementOutput;
    private System.Threading.Timer movementWatchdog;
    private volatile bool movementExpired;
    private string movementFailure;
    private readonly ApproachProgress approachProgress = new ApproachProgress();
    private bool searchingEmpty;
    private Guna2Button normalBotButton;
    private bool normalBotRequested;
    private bool MovementUsesWaitProtocol{get{return TurboAttackEnabled||waitDmgCheck.Checked||waitDmgRangeCheck.Checked||waitArrowCheck.Checked;}}
    private System.Windows.Forms.Timer normalBotKeepAlive;

    private void InitializeFarmOptions()
    {
        tabMain.AutoScroll=true;tabMain.AutoScrollMinSize=new Size(0,860);
        // The movement-only option controls the game's auto-attack target only.
        // Wait/Target combat settings and AttackEnabled remain independent.
        foreach (Control label in tabMain.Controls)
            if (label is Label && label.Top == 394) label.Visible = false;
        approachOnlyCheck = new Guna2CheckBox {
            Name = "approachOnlyCheck", Text = "Yanına git ve dur · düz vuruş yapma",
            AutoSize = false, Location = new Point(24, 350), Size = new Size(610, 30)
        };
        tabMain.Controls.Add(approachOnlyCheck);
        AddCaption(tabMain, "Çevre ve seçili hedef saldırıları bağımsız çalışır.", 24, 385, 610, 24, 9f, StudioMuted);
        AddCaption(tabMain, "Durma mesafesi", 24, 421, 180, 24, 10f, StudioMuted);
        approachDistance = new NumericUpDown {
            Name = "approachDistance", Minimum = 25, Maximum = 1000, Increment = 25, Value = 100,
            Location = new Point(205, 419), Size = new Size(115, 28),
            BackColor = StudioSurface, ForeColor = StudioText
        };
        tabMain.Controls.Add(approachDistance);
        farmStatus = new Label { Name = "farmStatus", Text = "Hazır", Location = new Point(24, 460),
            Size = new Size(610, 26), ForeColor = StudioMuted, AutoEllipsis = true };
        tabMain.Controls.Add(farmStatus);
        fastTimingCheck = new Guna2CheckBox {
            Name = "fastTimingCheck", Text = "Seri zamanlama · ek beklemeyi azalt",
            AutoSize = false, Location = new Point(24, 468), Size = new Size(610, 30)
        };
        tabDamage.Controls.Add(fastTimingCheck);
        fastTimingCheck.CheckedChanged += (s, e) => { if (automation != null) automation.FastTiming = fastTimingCheck.Checked; };
        approachOnlyCheck.CheckedChanged += (s, e) => {
            approachPolicy.Reset(); approachTargetId = 0;
            if (Field_02DF && IsClientReady()) { Method_02AF(0); ReleaseMovement(); }
        };
        movementOutput = new MovementOutput((forward,pressed) => {
            string address=forward ? AddressCatalog.Property_0380 : AddressCatalog.Property_0381;
            string value=pressed ? (forward ? "1" : "256") : "0";
            if (!memory.WriteValue(address,"2bytes",value)) throw new InvalidOperationException("Hareket komutu oyuna yazılamadı.");
        }, System.Threading.Thread.Sleep);
        movementWatchdog = new System.Threading.Timer(_ => {
            try { if (movementOutput.Expire(movementClock.ElapsedMilliseconds)) movementExpired=true; }
            catch(Exception ex) { movementFailure=ex.Message; movementExpired=true; }
        }, null, 50, 50);
        movementTimer = new System.Windows.Forms.Timer(components) { Interval = 25 };
        movementTimer.Tick += OnMovementTick;
        InitializeReviveRecovery();
        searchWhenEmptyCheck = new Guna2CheckBox { Name="searchWhenEmptyCheck", Text="Hedef yoksa menzil içinde dolaş", Location=new Point(24,506), Size=new Size(610,30) };
        tabMain.Controls.Add(searchWhenEmptyCheck);
        AddCaption(tabMain,"Arama yarıçapı",24,547,180,24,10f,StudioMuted);
        searchRange = new NumericUpDown { Name="searchRange", Minimum=500, Maximum=200000, Increment=500, Value=20000, Location=new Point(205,544), Size=new Size(135,28), BackColor=StudioSurface, ForeColor=StudioText };
        tabMain.Controls.Add(searchRange);
        AddCaption(tabMain,"Başlangıç çevresinde tur atar; hedef görünce takibe geçer.\nBir noktada ilerleyemezse 8 saniye sonra sıradakini dener.",24,584,610,40,9f,StudioMuted);
        normalBotButton=TaskButton("Saldırı ve hareket botunu başlat",24,632,360);normalBotButton.Name="normalBotButton";
        normalBotButton.Click+=(s,e)=>ToggleNormalBot();tabMain.Controls.Add(normalBotButton);
        normalBotKeepAlive=new System.Windows.Forms.Timer(components){Interval=1000};normalBotKeepAlive.Tick+=(s,e)=>MaintainNormalBot();normalBotKeepAlive.Start();
        searchWhenEmptyCheck.CheckedChanged += (s,e) => { searchPolicy.Reset(); if (Field_02DF && IsClientReady()) ReleaseMovement(); };
        searchRange.ValueChanged += (s,e) => { searchPolicy.Reset(); SyncFarmAreaRadiusFromRange(); };
        approachDistance.ValueChanged += (s,e) => approachPolicy.Reset();
        StyleControls(tabMain);
        StyleControls(tabDamage);
    }

    private void UpdateContextActionButton()
    {
        if(startStopFarmBtn==null||mainTabs==null)return;
        if(emergencyLatched){startStopFarmBtn.Text="Acil durdurma kilidi etkin";startStopFarmBtn.Enabled=false;return;}
        startStopFarmBtn.Enabled=mainTabs.SelectedTab==tabMain||mainTabs.SelectedTab==tabDamage||mainTabs.SelectedTab==tabRoute||mainTabs.SelectedTab==taskPage;
        if(mainTabs.SelectedTab==tabDamage){bool owned=RouteOwnsCombat||taskDungeonRunning||normalBotRequested||Field_02DF||developerWaitRunning||packEngineRunning;startStopFarmBtn.Enabled=!owned;startStopFarmBtn.Text=owned?"Saldırı görev / test yönetiminde":dmgStartCheck.Checked?"Saldırıyı durdur":"Saldırıyı başlat";return;}
        if(mainTabs.SelectedTab==tabRoute){startStopFarmBtn.Text=RouteOwnsCombat?"Rotayı durdur":"Rotayı başlat";return;}
        if(!startStopFarmBtn.Enabled){startStopFarmBtn.Text="Bu menü ayar ve araç içerir";return;}
        bool dungeon=taskPage!=null&&mainTabs.SelectedTab==taskPage;
        startStopFarmBtn.Text=dungeon
            ? (taskCancellation!=null?"Kellam görevini durdur":"Kellam görevini başlat")
            : (normalBotRequested||Field_02DF?"Saldırı ve hareket botunu durdur":"Saldırı ve hareket botunu başlat");
    }

    private void ToggleNormalBot()
    {
        if(emergencyLatched)return;
        if(RouteOwnsCombat||isRecordingRoute){MessageBox.Show("Önce rota kaydını/takibini durdur.");return;}
        if(npcDialogueEditorOpen)return;
        if(normalBotRequested||Field_02DF){StopNormalBotByUser();return;}
        if(taskCancellation!=null){farmStatus.Text="Kellam görevi çalışırken normal bot başlatılamaz.";return;}
        if(!IsClientReady()){MessageBox.Show("Önce oyuna bağlanın.");return;}
        normalBotRequested=true;EnsureNormalBotWaitMode();ApplyTargetFilters();normalRecoverySettings=CaptureAutomationSelections();SaveFarmProfile();SaveAutomationSelections();StartNormalBotCore();RecordActivity("Bot","Saldırı ve hareket botu başlatıldı.");UpdateContextActionButton();
    }
    private void EnsureNormalBotWaitMode()
    {
        approachOnlyCheck.Checked=true;
        if(!waitDmgCheck.Checked&&!waitDmgRangeCheck.Checked&&!waitArrowCheck.Checked)waitDmgCheck.Checked=true;
    }
    private void StartNormalBotCore()
    {
        if(!normalBotRequested||!IsClientReady())return;
        EnsureNormalBotWaitMode();searchWhenEmptyCheck.Checked=true;
        if(IsX86ClientMode)
        {
            Field_02DF=false;movementTimer.Stop();RequestX86BotState(!MovementUsesWaitProtocol);
            normalBotButton.Text="Saldırı ve hareket botunu durdur";farmStatus.Text="x86 saldırı ve hareket botu başlatılıyor…";UpdateContextActionButton();return;
        }
        Field_02DF=true;movementTimer.Start();dmgStartCheck.Checked=true;
        normalBotButton.Text="Saldırı ve hareket botunu durdur";farmStatus.Text="Saldırı ve hareket botu çalışıyor.";UpdateContextActionButton();
    }
    private void MaintainNormalBot()
    {
        if(emergencyLatched)return;
        if(!normalBotRequested||taskCancellation!=null||IsDisposed||Disposing)return;
        if(IsX86ClientMode)
        {
            if(!IsClientReady()){farmStatus.Text="x86 oyun/karakter bekleniyor · bot kapanmadı.";return;}
            RequestX86BotState(!MovementUsesWaitProtocol);return;
        }
        if(!Field_02DF){Field_02DF=true;movementTimer.Start();farmStatus.Text="Hareket yeniden başlatıldı · bot sürekli çalışıyor.";}
        if(!searchWhenEmptyCheck.Checked)searchWhenEmptyCheck.Checked=true;
        if(!IsClientReady()){farmStatus.Text="Oyun/karakter bekleniyor · bot kapanmadı.";return;}
        if(!dmgStartCheck.Checked)dmgStartCheck.Checked=true;
        automation.AttackEnabled=true;
        if(!automation.WorkerAlive)automation.Start();
    }

    private void StopNormalBotByUser()
    {
        normalBotRequested=false;resumeNormalBotAfterRestart=false;StopMovementFarm();ApplyTargetFilters();
        if(dmgStartCheck!=null)dmgStartCheck.Checked=false;
        RequestX86BotState(false);
        if(normalBotButton!=null)normalBotButton.Text="Saldırı ve hareket botunu başlat";
        if(farmStatus!=null)farmStatus.Text="Saldırı ve hareket botu durduruldu.";
        UpdateContextActionButton();
    }

    private void OnMovementTick(object sender, EventArgs e)
    {
        if(MountSkillBusy){ReleaseMovement();return;}
        if (!Field_02DF) return;
        if(reviveEscape.BlockAttack)return;
        if(movementExpired) { movementExpired=false; PauseMovementFarm(movementFailure ?? "Hareket gecikti; yeniden deneniyor.", 1000); movementFailure=null; return; }
        if (movementClock.ElapsedMilliseconds < farmRetryAfter) return;
        try
        {
            if (!IsClientReady()) { PauseMovementFarm("Oyun/karakter bekleniyor; takip açık.", 1000); return; }
            PointF selectedCenter;float selectedRadius;
            bool selectedAreaActive=TryGetFarmAreaMemory(out selectedCenter,out selectedRadius);
            if(!selectedAreaActive)selectedAreaActive=TryGetAdvancedStartArea(out selectedCenter,out selectedRadius);
            foreach(var expired in blockedTargets.Where(p=>p.Value<=movementClock.ElapsedMilliseconds).Select(p=>p.Key).ToList())blockedTargets.Remove(expired);
            var liveTargets = ReadTargetEntities().Where(m =>
                (walkMetinCheck.Checked && m.Property_03CA == AddressCatalog.Field_035B) ||
                (walkCanavarCheck.Checked && m.Property_03CA == AddressCatalog.Field_035C))
                .Where(IsTargetInsideFarmConfiguration).ToList();
            var rememberedTargets=selectedAreaActive&&atlas!=null
                ?MapEntitiesForDisplay(liveTargets).Select(v=>v.Entity).Where(m=>
                    (walkMetinCheck.Checked&&m.Property_03CA==AddressCatalog.Field_035B)||
                    (walkCanavarCheck.Checked&&m.Property_03CA==AddressCatalog.Field_035C)).ToList()
                :liveTargets;
            var allTargets = OrderFarmTargets(rememberedTargets.Where(IsTargetInsideFarmConfiguration));
            var targets = allTargets
                .Where(m => !blockedTargets.ContainsKey(m.Property_03C9) || blockedTargets[m.Property_03C9] <= movementClock.ElapsedMilliseconds);
            var target = ChooseCombatTarget(targets.OrderBy(m=>m.Property_03C9==approachTargetId?0:1),"farm",liveTargets);
            if(target!=null&&(!IsTargetInsideFarmConfiguration(target)||(!walkMetinCheck.Checked&&target.Property_03CA==AddressCatalog.Field_035B)))
            {
                ResetCombatFocus();target=ChooseCombatTarget(targets,"farm",liveTargets);
            }
            if (target == null)
            {
                Method_02AF(0); approachTargetId = 0; approachPolicy.Reset(); approachProgress.Reset();
                if(allTargets.Count>0)
                {
                    ReleaseMovement();searchingEmpty=false;
                    long wait=Math.Max(0,blockedTargets.Where(p=>allTargets.Any(t=>t.Property_03C9==p.Key)).Select(p=>p.Value-movementClock.ElapsedMilliseconds).DefaultIfEmpty(0).Min());
                    farmStatus.Text="Oval içindeki "+allTargets.Count+" hedef bekliyor · ulaşım "+Math.Max(1,(int)Math.Ceiling(wait/1000d))+" sn sonra yeniden denenecek.";
                    return;
                }
                TrySelectNextPatrolZone();
                if(TryMoveToReturnPoint())return;
                if (searchWhenEmptyCheck.Checked) {
                    if(!searchingEmpty) { ReleaseMovement(); searchPolicy.Reset(); searchingEmpty=true; }
                    float px=memory.ReadFloat(AddressCatalog.Property_037B), py=memory.ReadFloat(AddressCatalog.Property_037C);
                    var waypoint=selectedAreaActive
                        ? searchPolicy.NextInArea(px,py,selectedCenter.X,selectedCenter.Y,selectedRadius,movementClock.ElapsedMilliseconds)
                        : searchPolicy.Next(px,py,(float)searchRange.Value,movementClock.ElapsedMilliseconds);
                    if (waypoint != null) {
                        Method_02C2(px,py,waypoint.X,waypoint.Y);
                        farmStatus.Text=(selectedAreaActive?"Seçilen harita alanında dolaşılıyor":"Geniş alanda hedef aranıyor")+" · tur noktası "+searchPolicy.CurrentPoint+" / 8 · menzil "+(int)(selectedAreaActive?selectedRadius:(float)searchRange.Value);
                    }
                    else { ReleaseMovement(); farmStatus.Text="Konum okunamadı; hareket durdu."; }
                } else { searchingEmpty=false;searchPolicy.Reset();ReleaseMovement();farmStatus.Text = "Uygun hedef bekleniyor."; }
                return;
            }
            if(searchingEmpty)ReleaseMovement();
            searchingEmpty=false;searchPolicy.Reset();
            approachTargetId = target.Property_03C9;
            bool targetLive=liveTargets.Any(m=>m.Property_03C9==approachTargetId);
            if (!approachOnlyCheck.Checked&&!MovementUsesWaitProtocol&&targetLive)
            {
                Method_02AF(approachTargetId); farmStatus.Text = "Normal hedef takibi çalışıyor · ovalde kalan "+allTargets.Count+"."; return;
            }
            // Do not populate the automatic attack target. Explicit Wait/Target attacks are untouched.
            if(!HybridWaitRuntimeActive)Method_02AF(0);
            float x = memory.ReadFloat(AddressCatalog.Property_037B);
            float y = memory.ReadFloat(AddressCatalog.Property_037C);
            float closeDistance=HybridWaitRequested?Math.Min((float)approachDistance.Value,HybridCloseDistance):(float)approachDistance.Value;
            // A high-speed crossing must not latch arrival outside the held-attack range.
            if(HybridWaitRequested&&RoutePatrol.Distance(x,y,target.X,target.Y)>HybridCloseDistance+50)approachPolicy.Reset();
            if (approachPolicy.ShouldMove(approachTargetId, x, y, target.X, target.Y, closeDistance))
            {
                double dx=target.X-x, dy=target.Y-y;
                if(!approachProgress.CanContinue(approachTargetId,Math.Sqrt(dx*dx+dy*dy),movementClock.ElapsedMilliseconds)) {
                    blockedTargets[approachTargetId] = movementClock.ElapsedMilliseconds + (selectedAreaActive?5000:15000);
                    PauseMovementFarm("Hedefe giden yol geçici kapalı · oval bitmeden yeniden denenecek.", 500); return;
                }
                bool near = dx*dx+dy*dy < Math.Pow((double)approachDistance.Value+200,2);
                nearPulse = !nearPulse;
                if (!HybridWaitRequested&&near && !nearPulse) ReleaseMovement();
                else Method_02C2(x, y, target.X, target.Y);
                farmStatus.Text = (targetLive?"Hedefe":"Haritada hatırlanan hedefe")+" yaklaşılıyor · ovalde kalan "+allTargets.Count;
            }
            else
            {
                ReleaseMovement(); farmStatus.Text = "Hedefin yanında · saldırılıyor · ovalde kalan "+allTargets.Count;
            }
        }
        catch (Exception ex)
        {
            PauseMovementFarm("Hareket yeniden denenecek: " + ex.Message, 2000);
        }
    }

    private void PauseMovementFarm(string message, int delay)
    {
        try { Method_02AF(0); ReleaseMovement(); } catch { }
        approachProgress.Reset(); approachPolicy.Reset(); approachTargetId = 0; nearPulse = false; searchingEmpty=false;
        farmRetryAfter = movementClock.ElapsedMilliseconds + delay;
        farmStatus.Text = message;
        if(message.IndexOf("ulaşılamadı",StringComparison.OrdinalIgnoreCase)>=0||message.IndexOf("takıl",StringComparison.OrdinalIgnoreCase)>=0||message.IndexOf("gecikti",StringComparison.OrdinalIgnoreCase)>=0)RecordRecovery(message);
    }

    private void ReleaseMovement()
    {
        try {if(RouteOwnsCombat)StopRouteEngineMotion();}
        finally {movementOutput?.Stop();}
    }

    private void StopMovementFarm()
    {
        ResetCombatFocus();
        bool wasRunning = Field_02DF;
        Field_02DF = false;
        movementTimer?.Stop(); blockedTargets.Clear(); farmRetryAfter=0; approachProgress.Reset(); searchPolicy.Reset(); nearPulse=false; searchingEmpty=false; approachTargetId = 0; approachPolicy.Reset();
        if (normalBotButton != null) normalBotButton.Text = normalBotRequested ? "Oyun bekleniyor · bot devam edecek" : "Saldırı ve hareket botunu başlat";
        UpdateContextActionButton();
        if (wasRunning)
        {
            try { Method_02AF(0); ReleaseMovement(); }
            catch (Exception ex) { if (farmStatus != null) farmStatus.Text = ex.Message; }
        }
    }
}



