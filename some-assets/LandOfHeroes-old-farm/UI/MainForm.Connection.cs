using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private bool connecting, connectionRequested, clientConnected;
    private DateTime nextConnectionAttempt;
    private Label connectionStatus;
    private bool questSweepBusy; private DateTime nextQuestSweep;
    // Invisible leftover quest dialogs block the middle of the game screen for the mouse; they are hidden on connect
    // and every few seconds while connected.
    private async Task SweepQuestDialogsOnceAsync()
    {
        if (questSweepBusy || taskDungeonRunning || npcDialogueEditorOpen) return;
        questSweepBusy = true; nextQuestSweep = DateTime.UtcNow.AddSeconds(5);
        try { await SweepQuestDialogsAsync(System.Threading.CancellationToken.None); }
        catch(OperationCanceledException){}
        finally { questSweepBusy = false; }
    }
    private void InitializeConnectionSupervisor()
    {
        connectBtn.Text = "Oyuna bağlan";
        connectionStatus = new Label { Text = "Oyun otomatik bulunur.", Location = new Point(22, 543), Size = new Size(280, 24), ForeColor = StudioMuted, AutoEllipsis = true };
        leftPanel.Controls.Add(connectionStatus);
        var timer = new System.Windows.Forms.Timer(components) { Interval = 3000 };
        timer.Tick += async (s,e) => {
            if (emergencyLatched || !connectionRequested || connecting || DateTime.UtcNow < nextConnectionAttempt) return;
            bool alive = false;
            try { alive = clientConnected && memory.Field_0197?.Property_0268 != null && !memory.Field_0197.Property_0268.HasExited; } catch { }
            if (!alive) await ConnectAutomaticallyAsync();
            else
            {
                if ((automation.AttackEnabled || automation.AutoPickup)) automation.Start();
                // The running task sweeps on every dialog read; otherwise keep the player's clicks free here.
                if (!taskDungeonRunning && !questSweepBusy && DateTime.UtcNow >= nextQuestSweep) await SweepQuestDialogsOnceAsync();
            }
        };
        timer.Start();
    }
    private async void OnConnectClick(object sender, EventArgs e)
    {
        if(emergencyLatched)return;
        if(connectionRequested) {
            connectionRequested=false;clientConnected=false;StopNormalBotByUser();StopSessionLease();ReleaseClient();chosenPid=0;boundPid=0;awaitingReplacementClient=false;RefreshClientList();
            dmgStartCheck.Checked=false;autoPickCheck.Checked=false;automation?.Stop();
            connectBtn.Text="Oyuna bağlan";connectionStatus.Text="Bağlantı kullanıcı tarafından kesildi.";return;
        }
        if(InsertPickActive){CancelInsertPick();return;}
        if(StartInsertPick())return;
        connectionRequested = true;
        await ConnectAutomaticallyAsync();
    }
    private async Task ConnectAutomaticallyAsync()
    {
        if(emergencyLatched)return;
        if (connecting) return;
        connecting = true; connectBtn.Enabled = false;
        bool attack = dmgStartCheck.Checked, pickup = autoPickCheck.Checked;
        try
        {
            string reason0;var process = PickClientProcess(out reason0);
            if (process == null) throw new InvalidOperationException(reason0);
            if (!ClaimClient(process.Id)) throw new InvalidOperationException("Bu oyun başka bir Onur penceresinde; listeden başka bir oyun seçin.");
            chosenPid = process.Id;boundPid = process.Id;awaitingReplacementClient=false;
            if (clientConnected && memory.Field_0197?.Property_0268?.Id == process.Id && !process.HasExited) { connectionStatus.Text = "Oyun bağlantısı hazır."; return; }
            clientConnected = false; clientTurboAttackDelay=TurboAttackDelay; connectBtn.Text = "Bağlanıyor…";
            connectionStatus.Text = "Oyun adresleri okunuyor…";
            automation?.Stop();
            if (automation != null && automation.WorkerAlive) throw new InvalidOperationException("Önceki çağrının bitmesi bekleniyor; yeniden denenecek.");
            string reason;
            if (!memory.Method_015D(process.Id, out reason)) throw new InvalidOperationException("Oyun bağlantısı kurulamadı: " + reason);
            await AddressCatalog.ResolveAddressesAsync(memory);
            if (IsDisposed || Disposing || emergencyLatched) return;
            bool x86Client = !memory.Field_0197.Property_026A;
            if (x86Client)
            {
                // For WOW64 clients the live Python queue is the verified connection
                // boundary. Core x64 pointers stay disabled and cannot be used by mistake.
                using (var probe = new QueuedPythonBridge(process))
                {
                    probe.Queue("pass");
                    var until = DateTime.UtcNow.AddSeconds(5);
                    while (!probe.Poll())
                    {
                        if (DateTime.UtcNow >= until) throw new InvalidOperationException("x86 Python bağlantısı zaman aşımına uğradı.");
                        await Task.Delay(40);
                    }
                }
                await EnsureX86AliasesAsync(System.Threading.CancellationToken.None);
                if(emergencyLatched)return;
                automation = new AutomationEngine(memory){EmergencyBlocked=emergencyLatched};
                ApplyTargetFilters();
                automation.AttackEnabled = false; automation.AutoPickup = false;
                clientConnected = true; connectBtn.Text = "Bağlantıyı kes"; pidLbl.Text = process.Id.ToString(); RefreshClientList(); UpdateClientTitle(process.Id);
                radarEntities = new System.Collections.Generic.List<EntityState>(); RenderRadar(radarEntities); StartRadarWorker();
                connectionStatus.Text = "x86 oyun bağlantısı hazır · Python köprüsü etkin.";
                await SweepQuestDialogsOnceAsync();
                return;
            }
            if (process.HasExited || AddressCatalog.Field_0368 == 0 || AddressCatalog.Field_036F == 0 || AddressCatalog.Field_036E == 0 || AddressCatalog.Field_0370 == "0x0")
                throw new InvalidOperationException("Oyun adresleri hazır değil; yeniden denenecek.");
            // This measured rhythm applies only to the verified Land of Heroes builds.
            // A dungeon retains the interval entered by its user; other clients keep their old preset.
            var attackProfile = await Task.Run(() => ClientProfile.For(process));
            if(emergencyLatched)return;
            clientTurboAttackDelay = GameCallBridge.IsVerifiedClientHash(attackProfile.Sha256) ? 200 : TurboAttackDelay;
            if (TurboAttackEnabled)
            {
                turboTimingChange=true;attackDelayBar.Value=ActiveTurboAttackDelay;turboTimingChange=false;
                UpdateTurboAttackButton();
            }
            // A new bridge must never reuse executable memory from a previous game process.
            automation = new AutomationEngine(memory);
            ApplyTargetFilters();
            automation.FastTiming = fastTimingCheck.Checked;
            automation.AttackRange = attackRangeBar.Value; automation.AttackDelay = TurboAttackEnabled ? ActiveTurboAttackDelay : attackDelayBar.Value; automation.PickupDelay = pickDelayBar.Value; automation.MaxTargetsPerCycle = TurboAttackEnabled ? TurboAttackTargetCap : 0;
            automation.WaitDamage = waitDmgCheck.Checked; automation.WaitDamageRange = waitDmgRangeCheck.Checked;
            automation.TargetDamage = targetDmgCheck.Checked; automation.TargetDamageRange = targetDmgRangeCheck.Checked;
            automation.WaitArrow = waitArrowCheck.Checked; automation.TargetArrow = targetArrowCheck.Checked;
            attack = dmgStartCheck.Checked; pickup = autoPickCheck.Checked;
            automation.AttackEnabled = WaitExploitEnabled?false:attack; automation.AutoPickup = pickup;
            if ((!WaitExploitEnabled&&attack) || pickup) automation.Start();
            clientConnected = true; connectBtn.Text = "Bağlantıyı kes"; pidLbl.Text = process.Id.ToString(); RefreshClientList(); UpdateClientTitle(process.Id);
            if(WaitExploitEnabled)ApplyWaitExploitRuntime();
            var initialRadar=ReadRadarEntities();radarEntities=initialRadar;RenderRadar(initialRadar);
            connectionStatus.Text = "Oyun bağlantısı hazır · "+initialRadar.Count+" varlık okundu."; StartRadarWorker();
            await SweepQuestDialogsOnceAsync();
        }
        catch (Exception ex)
        {
            clientConnected = false;
            if (!IsDisposed && !Disposing) { connectBtn.Text = connectionRequested ? "Bağlantıyı kes" : "Oyuna bağlan"; connectionStatus.Text = ex.Message; }
            // Preserve intent during transient connection loss; loop waits for valid pointers.
            if (automation != null) { automation.AttackEnabled = dmgStartCheck.Checked; automation.AutoPickup = autoPickCheck.Checked; }
            nextConnectionAttempt = DateTime.UtcNow.AddSeconds(5);
        }
        finally { connecting = false; if (!IsDisposed && !Disposing) connectBtn.Enabled = !emergencyLatched; }
    }
}


