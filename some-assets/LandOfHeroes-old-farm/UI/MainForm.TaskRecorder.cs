using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Guna.UI2.WinForms;

public partial class MainForm
{
    [StructLayout(LayoutKind.Sequential)]private struct NativePoint{public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]private struct NativeRect{public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll")]private static extern bool GetClientRect(IntPtr hWnd,out NativeRect rect);
    [DllImport("user32.dll")]private static extern bool ClientToScreen(IntPtr hWnd,ref NativePoint point);
    [DllImport("user32.dll")]private static extern bool PostMessage(IntPtr hWnd,uint message,UIntPtr wParam,IntPtr lParam);
    private TabPage taskPage; private TaskSequence taskSequence=new TaskSequence(); private ListBox taskStages;
    private Label taskStatus; private Guna2CheckBox taskLoop,taskAutoStart; private Guna2Button taskRun,taskStop;
    private bool taskDungeonRunning; private int taskDungeonStage;
    private bool autoCloseShaman=true;private Guna2Button closeShamanButton;
    private readonly System.Collections.Generic.HashSet<int> taskStagesDone=new System.Collections.Generic.HashSet<int>();
    private static string TaskAutoSavePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","gorev-otomatik.onurgorev.xml"); } }
    private CancellationTokenSource taskCancellation;
    private static readonly Color TaskDoneColor=Color.FromArgb(229,233,255),TaskActiveColor=Color.FromArgb(218,228,255);

    private void InitializeTaskRecorder()
    {
        taskPage=new TabPage("Görev kaydı"){Name="taskPage",BackColor=StudioSurface,ForeColor=StudioText,AutoScroll=true};mainTabs.TabPages.Add(taskPage);
        mainTabs.SelectedIndexChanged+=(s,e)=>UpdateContextActionButton();mainTabs.Selected+=(s,e)=>UpdateContextActionButton();mainTabs.MouseUp+=(s,e)=>UpdateContextActionButton();
        var title=new Label{Text="8 aşamalı zindan görevi",Location=new Point(26,22),AutoSize=true,Font=new Font("Segoe UI Semibold",18f),ForeColor=StudioText};
        var info=new Label{Text="Harita tabanlı akıllı sistem · kat haritadan tanınır, yol duvarlara göre hesaplanır.",Location=new Point(28,74),Size=new Size(640,30),ForeColor=StudioMuted};
        taskStages=new ListBox{Name="taskStages",Location=new Point(28,112),Size=new Size(640,250),BackColor=Color.FromArgb(9,16,28),ForeColor=StudioText,BorderStyle=BorderStyle.FixedSingle,DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=30,Font=new Font("Segoe UI",10f)};
        taskStages.DrawItem+=DrawTaskStage;
        taskRun=TaskButton("Görevi çalıştır",28,380,190);taskStop=TaskButton("Durdur",228,380,130);var save=TaskButton("Görevi kaydet",368,380,145);var load=TaskButton("Görev aç",523,380,145);
        taskAutoStart=new Guna2CheckBox{Name="taskAutoStart",Text="Oyun açılınca zindanı otomatik başlat",Location=new Point(28,436),Size=new Size(330,30),ForeColor=StudioText,Checked=false};
        taskLoop=new Guna2CheckBox{Name="taskLoop",Text="8. aşamadan sonra yeniden başlat",Location=new Point(368,436),Size=new Size(300,30),ForeColor=StudioText,Checked=true};
        taskStatus=new Label{Name="taskStatus",Text="Hazır · Oyuna bağlanıp görevi çalıştırın.",Location=new Point(28,474),Size=new Size(640,60),ForeColor=StudioMuted};
        var mapUpdate=TaskButton("Haritaları güncelle",28,540,190);
        var mapInfo=new Label{Text="Pack haritalarını yeniden çıkarır.",Location=new Point(228,540),Size=new Size(200,42),ForeColor=StudioMuted};
        closeShamanButton=TaskButton("Şaman kapatma: AÇIK",438,540,230);closeShamanButton.Name="closeShamanButton";
        UpdateShamanCloseToggle();
        mapUpdate.Click+=async(s,e)=>{mapUpdate.Enabled=false;try{await UpdateMapsAsync(m=>taskStatus.Text=m);}finally{mapUpdate.Enabled=true;}};
        closeShamanButton.Click+=(s,e)=>SetAutoCloseShaman(!autoCloseShaman);
        taskPage.Controls.AddRange(new Control[]{title,info,taskStages,taskRun,taskStop,save,load,taskLoop,taskAutoStart,taskStatus,mapUpdate,mapInfo,closeShamanButton});
        try{if(File.Exists(TaskAutoSavePath))taskSequence=TaskSequenceStore.Load(TaskAutoSavePath);}catch{taskSequence=new TaskSequence();}
        InitializeDungeonProfiles();
        save.Click+=(s,e)=>SaveTaskSequence();load.Click+=(s,e)=>LoadTaskSequence();
        taskRun.Click+=async(s,e)=>await RunTaskSequenceAsync(false);taskStop.Click+=(s,e)=>{resumeDungeonStage=0;StopTaskSequence("Kullanıcı durdurdu.");};
        taskAutoStart.CheckedChanged+=(s,e)=>{if(!taskAutoStart.Checked)StopTaskSequence("Otomatik zindan durduruldu.");};
        sessionDirty=true;
        RefreshTaskStageList(0);UpdateContextActionButton();
    }
    private void UpdateShamanCloseToggle()
    {
        if(closeShamanButton==null)return;
        closeShamanButton.Text="Şaman kapatma: "+(autoCloseShaman?"AÇIK":"KAPALI");
        closeShamanButton.FillColor=autoCloseShaman?StudioAccent:StudioSurface;
        closeShamanButton.ForeColor=autoCloseShaman?Color.White:StudioText;
    }
    private void SetAutoCloseShaman(bool enabled)
    {
        autoCloseShaman=enabled;UpdateShamanCloseToggle();sessionDirty=true;SaveAutomationSelections();
        if(taskStatus!=null)taskStatus.Text=enabled?"Şaman penceresi her açıldığında otomatik kapatılacak.":"Şaman penceresini otomatik kapatma kapalı.";
    }
    private void DrawTaskStage(object sender,DrawItemEventArgs e)
    {
        if(e.Index<0||e.Index>=taskStages.Items.Count)return;
        int number=e.Index+1;bool done=taskStagesDone.Contains(number),active=taskDungeonRunning&&taskUiStage==number;
        Color back=done?TaskDoneColor:active?TaskActiveColor:(e.State&DrawItemState.Selected)!=0?Color.FromArgb(236,240,246):taskStages.BackColor;
        using(var brush=new SolidBrush(back))e.Graphics.FillRectangle(brush,e.Bounds);
        string suffix=done?"  ✓ tamamlandı":active?"  ▶ çalışıyor":"";
        TextRenderer.DrawText(e.Graphics,taskStages.Items[e.Index]+suffix,taskStages.Font,new Rectangle(e.Bounds.X+8,e.Bounds.Y,e.Bounds.Width-8,e.Bounds.Height),done?Color.FromArgb(43,55,142):StudioText,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
    }
    private void CompleteTaskStage(int number,string text){taskStagesDone.Add(number);taskStatus.Text=text;taskStages?.Invalidate();}
    private Guna2Button TaskButton(string text,int x,int y,int w){return new Guna2Button{Text=text,Location=new Point(x,y),Size=new Size(w,42),BorderRadius=7,FillColor=StudioAccent,ForeColor=Color.Black};}
    private TaskStage SelectedTaskStage(){return taskStages!=null&&taskStages.SelectedIndex>=0?taskSequence.Stages[taskStages.SelectedIndex]:null;}
    private void RefreshTaskStageList(int selected){taskStages.BeginUpdate();taskStages.Items.Clear();foreach(var st in taskSequence.Stages)taskStages.Items.Add(st.Number.ToString("00")+"  "+st.Name);taskStages.EndUpdate();taskStages.SelectedIndex=Math.Max(0,Math.Min(7,selected));}
    private IntPtr TaskGameWindow(){try{return memory.Field_0197?.Property_0268?.MainWindowHandle??IntPtr.Zero;}catch{return IntPtr.Zero;}}
    private Rectangle TaskGameBounds(){var h=TaskGameWindow();NativeRect r;var p=new NativePoint();if(h==IntPtr.Zero||!GetClientRect(h,out r)||!ClientToScreen(h,ref p))return Rectangle.Empty;return new Rectangle(p.X,p.Y,r.Right-r.Left,r.Bottom-r.Top);}
    private void AutoSaveTaskSequence(){try{Directory.CreateDirectory(Path.GetDirectoryName(TaskAutoSavePath));TaskSequenceStore.Save(TaskAutoSavePath,taskSequence);SaveDungeonCatalog();}catch{}}
    private void SaveTaskSequence(){using(var d=new SaveFileDialog{Filter="Onur görev dosyası|*.onurgorev.xml",FileName="gorev.onurgorev.xml"})if(d.ShowDialog(this)==DialogResult.OK){TaskSequenceStore.Save(d.FileName,taskSequence);AutoSaveTaskSequence();taskStatus.Text="Görev kaydedildi: "+d.FileName;}}
    private void LoadTaskSequence(){using(var d=new OpenFileDialog{Filter="Onur görev dosyası|*.onurgorev.xml"})if(d.ShowDialog(this)==DialogResult.OK){taskSequence=TaskSequenceStore.Load(d.FileName);AutoSaveTaskSequence();RefreshTaskStageList(0);taskStatus.Text="Görev açıldı: "+d.FileName;}}
    private int EffectiveDungeonAttackDelay(TaskStage stage){int value=stage!=null?stage.WaitAttackMilliseconds:0;if(value<=0)value=taskSequence?.WaitAttackMilliseconds??275;return Math.Max(1,Math.Min(1000,value));}
    private int EffectiveDungeonRangedAttackRange(TaskStage stage){int value=stage!=null?stage.RangedAttackRange:0;if(value<=0)value=DungeonAttackRange;return Math.Max(100,Math.Min(9500,value));}
    private int DungeonAttackDelay{get{return EffectiveDungeonAttackDelay(CurrentDungeonStageSetting(taskUiStage));}}
    private Form CreateDungeonWaitDialog()
    {
        var dialog=new Form{Name="dungeonWaitDialog",Text="Zindan Wait ayarı",ClientSize=new Size(410,190),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterParent,BackColor=StudioBackground,ForeColor=StudioText,ShowInTaskbar=false};
        var title=new Label{Text="Varsayılan Wait kaç ms olsun?",Left=24,Top=22,Width=360,Height=30,Font=new Font("Segoe UI Semibold",13f),ForeColor=StudioText};
        var note=new Label{Text="Kat için özel süre varsa Zindan Editörü'ndeki değer kullanılır.",Left=24,Top=58,Width=370,Height=24,ForeColor=StudioMuted};
        var value=new NumericUpDown{Name="dungeonWaitMilliseconds",Left=24,Top=92,Width=150,Height=30,Minimum=1,Maximum=1000,Increment=5,Value=Math.Max(1,Math.Min(1000,taskSequence?.WaitAttackMilliseconds??275)),BackColor=StudioSurface,ForeColor=StudioText,TextAlign=HorizontalAlignment.Center};
        var unit=new Label{Text="ms",Left=182,Top=96,Width=45,Height=24,ForeColor=StudioMuted};
        var start=TaskButton("Zindanı başlat",230,88,155);start.Name="dungeonWaitStart";
        // Guna2Button's DialogResult alone does not propagate to its owning modal Form.
        start.Click+=(s,e)=>{dialog.ValidateChildren();dialog.DialogResult=DialogResult.OK;dialog.Close();};
        var cancel=new Button{Name="dungeonWaitCancel",Text="Vazgeç",Left=230,Top=136,Width=155,Height=34,DialogResult=DialogResult.Cancel,FlatStyle=FlatStyle.Flat,ForeColor=StudioMuted};
        dialog.Controls.AddRange(new Control[]{title,note,value,unit,start,cancel});dialog.AcceptButton=start;dialog.CancelButton=cancel;
        return dialog;
    }
    private bool PromptDungeonWaitMilliseconds()
    {
        using(var dialog=CreateDungeonWaitDialog())
        {
            if(dialog.ShowDialog(this)!=DialogResult.OK)return false;
            taskSequence.WaitAttackMilliseconds=(int)((NumericUpDown)dialog.Controls["dungeonWaitMilliseconds"]).Value;
            AutoSaveTaskSequence();taskStatus.Text="Varsayılan Zindan Wait değeri kaydedildi · "+taskSequence.WaitAttackMilliseconds+" ms";return true;
        }
    }
    private float TaskX{get{return IsX86ClientMode?mapPlayerX:memory.ReadFloat(AddressCatalog.Property_037B);}}
    private float TaskY{get{return IsX86ClientMode?mapPlayerY:memory.ReadFloat(AddressCatalog.Property_037C);}}
    private async Task RunTaskSequenceAsync(bool silent=false)
    {
        if(taskSequence.UseCommandFlow){if(silent&&commandFlowStopped)return;if(!silent)commandFlowStopped=false;}
        if(emergencyLatched)return;
        if(npcDialogueEditorOpen)return;
        if(taskCancellation!=null)return;
        if(!IsClientReady()){if(!silent)MessageBox.Show("Önce oyuna bağlanın.");return;}
        if(!silent&&!PromptDungeonWaitMilliseconds())return;
        if(normalBotRequested||Field_02DF)StopNormalBotByUser();
        if(packEngineRunning)await StopPackEngineAsync("Zindan başlatılıyor; çakışan pack saldırısı durduruldu.");
        if(RouteOwnsCombat)StopRoutePatrol("Zindan başlatılıyor; rota durduruldu.");
        if(isRecordingRoute)StopRouteRecording(false);
        ReleaseMovement();
        int initialStage=silent&&resumeDungeonStage>0?Math.Max(1,Math.Min(8,resumeDungeonStage)):1;
        dungeonBaseSettings=CaptureAutomationSelections();SaveAutomationSelections();SaveFarmProfile();
        CaptureAttackTimingForDungeon();taskCancellation=new CancellationTokenSource();taskDungeonRunning=true;sessionDirty=true;taskStagesDone.Clear();for(int i=1;i<initialStage;i++)taskStagesDone.Add(i);SetUiStage(initialStage);resumeDungeonStage=0;taskRun.Enabled=false;UpdateContextActionButton();try{if(File.Exists(StatusPath))File.Delete(StatusPath);if(File.Exists(DungeonStatePath))File.Delete(DungeonStatePath);if(File.Exists(DungeonNpcPath))File.Delete(DungeonNpcPath);}catch{}
        StopMovementFarm();ReleaseMovement();dmgStartCheck.Checked=false;autoPickCheck.Checked=false;
        taskStatus.Text="Harita okunuyor…";
        try
        {
            int retry=0;
            while(true)
            {
                try { if(taskSequence.UseCommandFlow)await RunDungeonCommandFlowAsync(taskCancellation.Token);else await RunSmartDungeonAsync(taskCancellation.Token); break; }
                catch(Exception ex) when((ex is TimeoutException||ex is InvalidOperationException)&&!taskCancellation.IsCancellationRequested)
                {
                    // Replaying a failed hand-in from the beginning could spend another item.
                    if(taskSequence.UseCommandFlow)throw;
                    var current=taskSequence.Stages[Math.Max(0,Math.Min(taskSequence.Stages.Count-1,taskUiStage-1))];
                    if(retry>=Math.Max(0,current.RetryCount))throw;
                    retry++;RecordActivity("Zindan",current.Name+" yeniden deneniyor · "+retry+" / "+current.RetryCount+" · "+ex.Message);
                    taskStatus.Text=current.Name+" yeniden deneniyor · "+retry+" / "+current.RetryCount;
                    ReleaseMovement();SetBossAttack(false);await Task.Delay(1000,taskCancellation.Token);
                }
            }
        }
        catch(InvalidOperationException ex){taskStatus.Text="Görev durdu: "+ex.Message+(ResumeAfterFailure()?" · oyuna dönülünce devam edilecek.":"");}
        catch(OperationCanceledException){taskStatus.Text="Görev durduruldu.";}
        catch(TimeoutException ex){taskStatus.Text="Görev durdu: "+ex.Message+(ResumeAfterFailure()?" · oyuna dönülünce devam edilecek.":"");}
        finally
        {
            taskDungeonRunning=false;taskDungeonStage=0;taskUiStage=0;sessionDirty=true;taskRun.Enabled=true;taskCancellation.Dispose();taskCancellation=null;
            // The player gets the game back: no Wait attack, auto pickup or movement keeps running after the task.
            try{ReleaseMovement();StopMovementFarm();SetBossAttack(false);autoPickCheck.Checked=false;wallCheck.Checked=false;binekWallCheck.Checked=false;RestoreAttackTimingAfterDungeon();}catch{}
            taskStages?.Invalidate();UpdateContextActionButton();
        }
    }
    private void ConfigureDungeonCombatStage(bool stones,bool attack=true)
    {
        walkMetinCheck.Checked=stones;walkCanavarCheck.Checked=!stones;approachOnlyCheck.Checked=true;
        dmgStoneCheck.Checked=stones;dmgMonsterCheck.Checked=!stones;dmgPlayerCheck.Checked=false;
        ApplyDungeonAttackSettings();dmgStartCheck.Checked=attack;autoPickCheck.Checked=true;
        sessionAuto.Checked=true;sessionDirty=true;
        wallCheck.Checked=false;binekWallCheck.Checked=false;searchWhenEmptyCheck.Checked=false;
        // A moderate Wait range with a per-cycle target cap; hitting everything up to the maximum range crashed the client.
        attackRangeBar.Value=Math.Min(attackRangeBar.Maximum,DungeonEffectiveAttackRange);automation.AttackRange=attackRangeBar.Value;automation.MaxTargetsPerCycle=DungeonMaxTargets;
        StopMovementFarm();ReleaseMovement();
    }
    private const int DungeonAttackRange=2500,DungeonCloseAttackRange=250,DungeonMaxTargets=TurboAttackTargetCap;
    private const float DungeonCloseHoldDistance=120f;
    private static bool ShouldAttackDungeonTarget(int stage,float distance){return stage!=3||(!float.IsNaN(distance)&&!float.IsInfinity(distance)&&distance>=0&&distance<=DungeonCloseAttackRange);}
    private TaskStage CurrentDungeonStageSetting(int stage){return taskSequence!=null&&stage>=1&&stage<=taskSequence.Stages.Count?taskSequence.Stages[stage-1]:null;}
    private bool DungeonUsesRangedAttack(int stage){var s=CurrentDungeonStageSetting(stage);return stage!=3&&s!=null&&s.AttackMode=="Menzilli";}
    private bool DungeonAttackDisabled(int stage){var s=CurrentDungeonStageSetting(stage);return s!=null&&(!s.Enabled||s.AttackMode=="Kapalı");}
    private int DungeonEffectiveAttackRange{get{return DungeonUsesRangedAttack(taskUiStage)?EffectiveDungeonRangedAttackRange(CurrentDungeonStageSetting(taskUiStage)):DungeonCloseAttackRange;}}
    private float DungeonHoldDistance{get{if(HybridWaitRequested)return HybridCloseDistance;if(!DungeonUsesRangedAttack(taskUiStage))return taskUiStage==3?220f:DungeonCloseHoldDistance;float range=DungeonEffectiveAttackRange;return Math.Max(100f,Math.Min(range-50f,range*0.72f));}}
    private void ApplyDungeonAttackSettings()
    {
        bool ranged=DungeonUsesRangedAttack(taskUiStage);
        // The four-stone floor never spends movement or Wait cycles on spawned monsters.
        bool stonesOnly=taskUiStage==3;
        walkMetinCheck.Checked=stonesOnly;walkCanavarCheck.Checked=!stonesOnly;
        dmgStoneCheck.Checked=true;dmgMonsterCheck.Checked=!stonesOnly;dmgPlayerCheck.Checked=false;
        ApplyTargetFilters();
        waitDmgRangeCheck.Checked=ranged;waitDmgCheck.Checked=!ranged;
        targetDmgRangeCheck.Checked=false;targetDmgCheck.Checked=false;waitArrowCheck.Checked=false;targetArrowCheck.Checked=false;
        // A restored/unchanged checkbox does not raise CheckedChanged. Push the mode to the
        // actual engine every time floor settings are applied, even when the UI already agrees.
        automation.WaitDamage=!ranged;automation.WaitDamageRange=ranged;
        automation.TargetDamageRange=false;automation.TargetDamage=false;automation.WaitArrow=false;automation.TargetArrow=false;
        attackRangeBar.Value=Math.Min(attackRangeBar.Maximum,DungeonEffectiveAttackRange);
        OnAttackRangeChanged(attackRangeBar,null);
        attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,DungeonAttackDelay));
        OnAttackDelayChanged(attackDelayBar,null);UpdateTurboAttackButton();
    }
    private const float KellamStopDistance=180f;
    private void SetBossAttack(bool on)
    {
        if(DungeonAttackDisabled(taskUiStage))on=false;
        bool wasOn=dmgStartCheck.Checked||(!IsX86ClientMode&&automation.AttackEnabled);
        // Reapply the combat mode only on the off -> on edge. Rewriting six checkboxes every
        // dungeon tick caused visible Wait open/close churn around range and floor boundaries.
        if(on&&!wasOn)ApplyDungeonAttackSettings();
        if(dmgStartCheck.Checked!=on)dmgStartCheck.Checked=on;
        if(!IsX86ClientMode)
        {
            automation.GroundStoneWait=taskDungeonRunning&&taskUiStage==3;
            automation.AttackEnabled=on;
            if(on){automation.FastTiming=true;automation.AttackDelay=DungeonAttackDelay;automation.Start();}
        }
    }
    private static float Distance2D(float x,float y,float tx,float ty){float dx=x-tx,dy=y-ty;return (float)Math.Sqrt(dx*dx+dy*dy);}
    private void SteerDungeon(float x,float y,float tx,float ty)
    {
        if(emergencyLatched){ReleaseMovement();return;}
        float to=GetTargetAngle(x,y,tx,ty),current=NormalizeAngle(memory.ReadFloat(AddressCatalog.Property_0382)),diff=Method_02C1(current,to);
        if(Math.Abs(diff)>8f){ReleaseMovement();Method_02C0(NormalizeAngle(current+Math.Sign(diff)*Math.Min(Math.Abs(diff),40f)));return;}
        if(Math.Abs(diff)>0.1f)Method_02C0(to);
        movementOutput.Move(movementClock.ElapsedMilliseconds);
    }
    private async Task WaitPositionStableAsync(CancellationToken token)
    {
        float lx=memory.ReadFloat(AddressCatalog.Property_037B),ly=memory.ReadFloat(AddressCatalog.Property_037C);var stable=DateTime.UtcNow;var limit=DateTime.UtcNow.AddSeconds(30);
        while(DateTime.UtcNow<limit&&(DateTime.UtcNow-stable).TotalMilliseconds<1000)
        {
            await Task.Delay(250,token);float x=memory.ReadFloat(AddressCatalog.Property_037B),y=memory.ReadFloat(AddressCatalog.Property_037C);
            if(Distance2D(x,y,lx,ly)>30)stable=DateTime.UtcNow;lx=x;ly=y;
        }
    }

    private void StopTaskSequence(string text,bool forRecovery=false){if(!forRecovery){resumeTaskAfterRestart=false;resumeDungeonStage=0;}taskCancellation?.Cancel();taskStatus.Text=text;}
}















