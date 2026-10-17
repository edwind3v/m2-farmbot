using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;

// Restarts only the client owned by this app, logs back in with this app's saved account and resumes its active bot.
public partial class MainForm
{
    private TextBox gameExePath;private Guna2CheckBox gameRestartCheck;private System.Windows.Forms.Timer gameWatchTimer;
    private AutomationSelectionProfile normalRecoverySettings;private int restartedRootPid;
    private int watchedPid;private bool resumeTaskAfterRestart,resumeNormalBotAfterRestart;private int resumeDungeonStage;private DateTime restartedAt=DateTime.MinValue;private int recoveryGeneration;
    private static string GamePathFile{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","oyun-exe.txt");}}

    private void InitializeGameRestart()
    {
        var caption=new Label{Text="Oyun exe",Location=new Point(24,698),Size=new Size(90,24),ForeColor=StudioMuted};
        gameExePath=new TextBox{Name="gameExePath",Location=new Point(114,698),Size=new Size(390,24)};
        var browse=TaskButton("Seç",516,690,120);browse.Name="gameExeBrowse";
        gameRestartCheck=new Guna2CheckBox{Name="gameRestartCheck",Text="Oyun kapanırsa yeniden aç, hesaba gir ve aktif botu sürdür",Location=new Point(24,738),Size=new Size(620,30),ForeColor=StudioText,Checked=true};
        tabMain.Controls.AddRange(new Control[]{caption,gameExePath,browse,gameRestartCheck});
        try{gameExePath.Text=File.Exists(GamePathFile)?File.ReadAllText(GamePathFile).Trim():"";}catch{}
        gameExePath.Leave+=(s,e)=>{SaveGamePath();LoadAutomationSelections();};
        browse.Click+=(s,e)=>
        {
            using(var d=new OpenFileDialog{Filter="Oyun|*.exe",FileName=Path.GetFileName(gameExePath.Text),InitialDirectory=File.Exists(gameExePath.Text)?Path.GetDirectoryName(gameExePath.Text):""})
                if(d.ShowDialog(this)==DialogResult.OK){gameExePath.Text=d.FileName;SaveGamePath();LoadAutomationSelections();}
        };
        gameRestartCheck.CheckedChanged+=(s,e)=>SaveAutomationSelections();
        var sessionPage=mainTabs.TabPages[4];sessionPage.AutoScroll=true;sessionPage.AutoScrollMinSize=new Size(0,850);
        var sessionExeCaption=new Label{Text="Oyun EXE'si · kapanırsa bu dosya açılır",Left=24,Top=636,Width=610,Height=24,ForeColor=StudioMuted};
        var sessionExe=new TextBox{Name="sessionGameExePath",Left=24,Top=670,Width=460,Height=28,Text=gameExePath.Text};
        var sessionBrowse=TaskButton("EXE seç",500,660,130);sessionBrowse.Name="sessionGameExeBrowse";sessionBrowse.Click+=(s,e)=>browse.PerformClick();
        var sessionRestart=new CheckBox{Name="sessionGameRestartCheck",Text="Oyun kapanırsa aktif botu devam ettir",Left=24,Top=716,Width=610,Height=30,Checked=gameRestartCheck.Checked,ForeColor=StudioText};
        sessionExe.Leave+=(s,e)=>{gameExePath.Text=sessionExe.Text.Trim();SaveGamePath();LoadAutomationSelections();};
        gameExePath.TextChanged+=(s,e)=>{if(sessionExe.Text!=gameExePath.Text)sessionExe.Text=gameExePath.Text;};
        sessionRestart.CheckedChanged+=(s,e)=>{if(gameRestartCheck.Checked!=sessionRestart.Checked)gameRestartCheck.Checked=sessionRestart.Checked;};
        gameRestartCheck.CheckedChanged+=(s,e)=>{if(sessionRestart.Checked!=gameRestartCheck.Checked)sessionRestart.Checked=gameRestartCheck.Checked;};
        sessionPage.Controls.AddRange(new Control[]{sessionExeCaption,sessionExe,sessionBrowse,sessionRestart});
        LoadAutomationSelections();
        gameWatchTimer=new System.Windows.Forms.Timer(components){Interval=3000};gameWatchTimer.Tick+=(s,e)=>{try{WatchGame();}catch(Exception ex){RecoveryStatus("Devam işlemi yeniden denenecek: "+ex.Message);}};gameWatchTimer.Start();
    }
    private bool ResumeAfterFailure(){if(taskSequence.UseCommandFlow||gameRestartCheck==null||!gameRestartCheck.Checked)return false;resumeDungeonStage=Math.Max(1,taskUiStage);resumeTaskAfterRestart=true;return true;}
    private void SaveGamePath(){try{Directory.CreateDirectory(Path.GetDirectoryName(GamePathFile));File.WriteAllText(GamePathFile,gameExePath.Text.Trim());}catch{}}
    private void RecoveryStatus(string text)
    {
        if(resumeTaskAfterRestart&&taskStatus!=null)taskStatus.Text=text;
        if((normalBotRequested||resumeNormalBotAfterRestart)&&farmStatus!=null)farmStatus.Text=text;
        if(connectionStatus!=null)connectionStatus.Text=text;
    }

    private void WatchGame()
    {
        if(emergencyLatched)return;
        var process=memory.Field_0197?.Property_0268;bool alive=false;
        try{alive=process!=null&&!process.HasExited;}catch{}
        if(alive){watchedPid=process.Id;if(normalBotRequested&&IsClientReady()&&!resumeNormalBotAfterRestart)normalRecoverySettings=CaptureAutomationSelections();}
        bool recoveryWanted=taskDungeonRunning||resumeTaskAfterRestart||normalBotRequested||resumeNormalBotAfterRestart;
        if(!alive&&watchedPid!=0&&gameRestartCheck.Checked&&recoveryWanted&&DateTime.UtcNow-restartedAt>TimeSpan.FromSeconds(60))
        {
            if(taskDungeonRunning){resumeDungeonStage=Math.Max(1,taskUiStage);resumeTaskAfterRestart=true;SaveAutomationSelections();StopTaskSequence("Oyun kapandı · "+resumeDungeonStage+". aşamadan devam etmek için yeniden açılıyor…",true);}
            if(normalBotRequested){resumeNormalBotAfterRestart=true;StopMovementFarm();}
            RestartGame();return;
        }
        PointF here;
        if(resumeTaskAfterRestart&&clientConnected&&!taskDungeonRunning&&taskCancellation==null&&TryClientMapPixel(out here))
        {
            resumeTaskAfterRestart=false;taskStatus.Text="Oyun yeniden açıldı · göreve devam ediliyor.";
            var start=RunTaskSequenceAsync(true);
        }
        if(resumeNormalBotAfterRestart&&clientConnected&&!taskDungeonRunning&&IsClientReady())
        {
            if(normalRecoverySettings!=null)ApplyAutomationSelections(normalRecoverySettings);resumeNormalBotAfterRestart=false;normalBotRequested=true;StartNormalBotCore();farmStatus.Text="Oyun yeniden açıldı · saldırı ve hareket botu devam ediyor.";
        }
    }

    private void RestartGame()
    {
        string exe=gameExePath.Text.Trim();if(exe.Length==0)exe=ClientExe;
        restartedAt=DateTime.UtcNow;
        if(!File.Exists(exe)){RecoveryStatus("Oyun kapandı ama seçilen exe bulunamadı.");return;}
        if(string.IsNullOrEmpty(sessionProfile.Username)||string.IsNullOrEmpty(sessionProfile.Password))RecoveryStatus("Oyun kapandı; otomatik giriş için Oturum sekmesine hesap bilgilerini girin.");
        var before=Process.GetProcessesByName(ClientProcessName).Select(p=>p.Id).ToList();
        int generation=++recoveryGeneration;
        ReleaseClient();chosenPid=0;boundPid=0;awaitingReplacementClient=true;
        try
        {
            var started=Process.Start(new ProcessStartInfo(exe){WorkingDirectory=Path.GetDirectoryName(exe),UseShellExecute=false});
            restartedRootPid=started==null?0:started.Id;
            if(started!=null&&!started.HasExited&&started.ProcessName.Equals(ClientProcessName,StringComparison.OrdinalIgnoreCase)&&ClaimClient(started.Id))
            {chosenPid=started.Id;boundPid=started.Id;awaitingReplacementClient=false;}
            WaitForNewClient(before,generation);
        }
        catch(Exception ex){awaitingReplacementClient=false;RecoveryStatus("Oyun yeniden açılamadı: "+ex.Message);return;}
        sessionAuto.Checked=true;autoLogin.Checked=true;autoEnter.Checked=true;
        clientConnected=false;connectionRequested=true;sessionDirty=true;
        RecoveryStatus("Oyun yeniden açıldı · bu uygulamanın istemcisi ve hesabı bekleniyor…");
    }
    private async void WaitForNewClient(System.Collections.Generic.List<int> before,int generation)
    {
        for(int i=0;i<40&&boundPid==0&&generation==recoveryGeneration;i++)
        {
            await System.Threading.Tasks.Task.Delay(1500);
            if(IsDisposed||Disposing||generation!=recoveryGeneration)return;
            try
            {
                var parents=ClientRestartOwnership.ReadParents();
                var fresh=Process.GetProcessesByName(ClientProcessName).Where(p=>!before.Contains(p.Id)&&ClientRestartOwnership.IsOwnedDescendant(p.Id,restartedRootPid,parents)&&!LockedElsewhere(p.Id,clientLockPid)).OrderBy(p=>p.Id).FirstOrDefault();
                if(fresh!=null&&!fresh.HasExited&&ClaimClient(fresh.Id)){chosenPid=fresh.Id;boundPid=fresh.Id;awaitingReplacementClient=false;RefreshClientList();}
            }
            catch(InvalidOperationException){}
            catch(System.ComponentModel.Win32Exception){}
        }
        if(generation==recoveryGeneration&&boundPid==0)RecoveryStatus("Yeniden açılan istemci bekleniyor · başka hesaba geçilmeyecek.");
    }

    private bool LoginAssistActive
    {
        get
        {
            if(!connectionRequested||sessionAuto==null||!sessionAuto.Checked||!autoLogin.Checked)return false;
            try{var p=memory.Field_0197?.Property_0268;return p!=null&&!p.HasExited;}catch{return false;}
        }
    }
}
