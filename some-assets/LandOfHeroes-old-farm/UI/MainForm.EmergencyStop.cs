using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private const int EmergencyHotkeyId=0x4F03;
    private volatile bool emergencyLatched;
    private bool emergencyStopping,emergencyHotkeyRegistered;
    private Button emergencyButton,emergencyUnlock;
    private ComboBox emergencyHotkeyCombo;
    private Label emergencyStatus;
    private bool emergencyCleanupConfirmed=true;
    private CancellationTokenSource emergencyOperations=new CancellationTokenSource();
    private string EmergencyHotkeyFile {get{return Path.Combine(Environment.GetEnvironmentVariable("ONUR_AUTOMATION_SETTINGS_DIR")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur"),"emergency-key.txt");}}
    private string EmergencyPath {get{return Path.Combine(runtimeFolder,"emergency.stop");}}
    private void InitializeEmergencyStop()
    {
        emergencyButton=new Button{Name="emergencyStopButton",Text="TÜMÜNÜ DURDUR",Left=286,Top=9,Width=164,Height=30,BackColor=Color.Firebrick,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
        emergencyHotkeyCombo=new ComboBox{Name="emergencyHotkeyCombo",Left=458,Top=12,Width=82,DropDownStyle=ComboBoxStyle.DropDownList};
        emergencyHotkeyCombo.Items.Add("Pause");foreach(string key in GlobalHotkeyOptions().Where(k=>k!="Kapali"))emergencyHotkeyCombo.Items.Add(key);
        emergencyUnlock=new Button{Name="emergencyUnlock",Text="Kilidi aç",Left=548,Top=9,Width=82,Height=30,Enabled=false};
        emergencyStatus=new Label{Name="emergencyStatus",Left=286,Top=43,Width=345,Height=19,AutoEllipsis=true,Text="Acil durdurma hazırlanıyor"};
        headerPanel.Controls.AddRange(new Control[]{emergencyButton,emergencyHotkeyCombo,emergencyUnlock,emergencyStatus});
        emergencyButton.Click+=async(s,e)=>await EmergencyStopAsync("Kullanıcı tümünü durdurdu.");emergencyUnlock.Click+=(s,e)=>UnlockEmergency();
        emergencyHotkeyCombo.SelectedIndexChanged+=(s,e)=>RegisterEmergencyHotkey();string saved="Pause";try{if(File.Exists(EmergencyHotkeyFile))saved=File.ReadAllText(EmergencyHotkeyFile).Trim();}catch{}emergencyHotkeyCombo.SelectedItem=emergencyHotkeyCombo.Items.Contains(saved)?saved:"Pause";
        FormClosed+=(s,e)=>{if(IsHandleCreated)UnregisterHotKey(Handle,EmergencyHotkeyId);};
    }
    private void RegisterEmergencyHotkey()
    {
        UnregisterHotKey(Handle,EmergencyHotkeyId);emergencyHotkeyRegistered=false;
        string chosen=emergencyHotkeyCombo.Text;uint modifiers=0,key=0;
        if(chosen=="Pause")key=(uint)Keys.Pause;else TryParseGlobalHotkey(chosen,out modifiers,out key);
        if(chosen==movementHotkeyCombo.Text||chosen==dungeonHotkeyCombo.Text){emergencyStatus.Text="Bu tuş hareket/zindan için kullanılıyor; başka tuş seç.";return;}
        emergencyHotkeyRegistered=key!=0&&RegisterHotKey(Handle,EmergencyHotkeyId,modifiers|ModNoRepeat,key);
        emergencyStatus.Text=emergencyHotkeyRegistered?chosen+" · her pencerede tüm otomasyonları durdurur":"Tuş kaydedilemedi; başka tuş seç veya kırmızı düğmeyi kullan.";
        if(emergencyHotkeyRegistered)try{Directory.CreateDirectory(Path.GetDirectoryName(EmergencyHotkeyFile));File.WriteAllText(EmergencyHotkeyFile,chosen);}catch{emergencyStatus.Text+=" · tuş tercihi kaydedilemedi";}
    }
    private async Task EmergencyStopAsync(string reason)
    {
        if(emergencyStopping)return;
        emergencyLatched=true;emergencyStopping=true;emergencyUnlock.Enabled=false;
        if(automation!=null){automation.EmergencyBlocked=true;automation.AttackEnabled=false;automation.AutoPickup=false;}
        var errors=new List<string>();
        Action<string,Action> stop=(name,action)=>{try{action();}catch(Exception ex){errors.Add(name+": "+ex.GetType().Name);}};
        stop("Güvenlik işareti",()=>{Directory.CreateDirectory(runtimeFolder);File.WriteAllText(EmergencyPath,"stop");});
        stop("Bekleyen işlemler",()=>emergencyOperations.Cancel());
        stop("Oturum",()=>StopSessionLease());
        stop("Zindan",()=>StopTaskSequence(reason));
        resumeTaskAfterRestart=resumeNormalBotAfterRestart=false;resumeDungeonStage=0;recoveryGeneration++;
        stop("Rota",()=>StopRoutePatrol(reason));stop("Rota kaydı",()=>StopRouteRecording(false));stop("Rota düzenleme",()=>EndRouteEdit());
        stop("Ön kontrol",()=>routeCheckCancellation?.Cancel());
        stop("Normal bot",()=>StopNormalBotByUser());stop("Harita yürüyüşü",()=>StopMapWalk(reason));stop("Geliştirici testi",()=>StopDeveloperWait(reason));
        stop("Oturum anahtarı",()=>sessionAuto.Checked=false);stop("Saldırı anahtarı",()=>dmgStartCheck.Checked=false);stop("Toplama",()=>autoPickCheck.Checked=false);
        stop("Otomatik zindan",()=>taskAutoStart.Checked=false);stop("Hareket",()=>ReleaseMovement());stop("Motor",()=>automation?.Stop());
        stop("Yardımcı otomasyonlar",()=>{rangePickupCheck.Checked=false;hybridWaitCheck.Checked=false;waitExploitCheck.Checked=false;developerLinkedAttackCheck.Checked=false;attackSpeedCheck.Checked=false;moveSpeedCheck.Checked=false;SetAutoCloseShaman(false);allowRulesCheck.Checked=false;});
        stop("Hedef",()=>ResetCombatFocus());StopCombatRecording(reason);
        mainTabs.Enabled=false;connectBtn.Enabled=false;startStopFarmBtn.Enabled=false;
        emergencyStatus.Text="KİLİTLİ · durdurma komutlarının sonucu bekleniyor";
        try
        {
            // Cleanup alone is permitted through the latch. Already executing native calls cannot be recalled.
            await StopPackEngineAsync(reason);
            if(!packStopConfirmed)errors.Add("Pack motoru durma onayı alınamadı");
            if(memory.Field_0197?.Property_0268!=null&&!memory.Field_0197.Property_0268.HasExited)
            {
                string result=await GamePythonAsync("import player\nplayer.SetAttackKeyState(0)\nf.write('stopped')",CancellationToken.None,true);
                if(result!="stopped")errors.Add("İstemci saldırı bırakma onayı alınamadı");
            }
        }
        catch(Exception ex){errors.Add("İstemci temizliği: "+ex.GetType().Name);}
        finally
        {
            stop("Son hareket bırakma",()=>ReleaseMovement());stop("Son motor durdurma",()=>automation?.Stop());
            if(automation!=null&&automation.WorkerAlive)errors.Add("Bekleyen yerel motor çağrısı henüz tamamlanmadı");
            emergencyStopping=false;emergencyUnlock.Enabled=true;
            emergencyCleanupConfirmed=errors.Count==0;
            emergencyStatus.Text=errors.Count==0?"KİLİTLİ · hiçbir işlem kendiliğinden başlamaz":"KİLİTLİ · bazı durdurma onayları alınamadı; geçmişi kontrol et";
            RecordActivity("Acil durdur",reason+(errors.Count==0?" · yerel motorlar durdu.":" · "+string.Join("; ",errors)));
        }
    }
    private void UnlockEmergency()
    {
        if(!emergencyLatched||emergencyStopping)return;
        if(!emergencyCleanupConfirmed&&MessageBox.Show("Bazı oyun içi durdurma komutları onaylanmadı. Oyunu elle kontrol et. Buna rağmen yalnızca kilidi kaldırmak istiyor musun?","Durdurma onayı eksik",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
        if(npcBridge!=null&&npcBridge.Pending)try{npcBridge.Poll();}catch{emergencyStatus.Text="Köprü durumu doğrulanamadı; kilit korunuyor.";return;}
        if(taskCancellation!=null||sessionPulseBusy||packEngineRunning||packStartPending||automation.WorkerAlive||npcNativeBusy||npcBridge!=null&&npcBridge.Pending){emergencyStatus.Text="Önce bekleyen görev/köprü kapanmalı; kilit korunuyor.";return;}
        try{if(File.Exists(EmergencyPath))File.Delete(EmergencyPath);}catch(Exception ex){emergencyStatus.Text="Kilit kaldırılamadı: "+ex.GetType().Name;return;}
        // Unlock authorizes only future explicit starts, not any previously checked master switch.
        dmgStartCheck.Checked=false;autoPickCheck.Checked=false;sessionAuto.Checked=false;taskAutoStart.Checked=false;
        resumeNormalBotAfterRestart=resumeTaskAfterRestart=false;resumeDungeonStage=0;
        automation.AttackEnabled=false;automation.AutoPickup=false;automation.EmergencyBlocked=false;
        emergencyOperations.Dispose();emergencyOperations=new CancellationTokenSource();
        emergencyLatched=false;mainTabs.Enabled=true;connectBtn.Enabled=true;emergencyUnlock.Enabled=false;UpdateContextActionButton();
        emergencyStatus.Text="Kilit açıldı · istediğin görevi ayrıca başlat";RecordActivity("Acil durdur","Kilit kullanıcı tarafından kaldırıldı; otomatik devam yok.");
    }
    private string GuardGameScript(string body)
    {
        return "import os\nif os.path.exists("+PythonLiteral(EmergencyPath)+"):\n f.write('EMERGENCY_STOP')\n return\n"+body;
    }
}
