using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;

public partial class MainForm
{
    private sealed class DeveloperClientProbe : IDisposable
    {
        public readonly Process Process;
        public readonly QueuedPythonBridge Bridge;
        public readonly string OutputPath;
        public DateTime QueuedAtUtc;
        public DateTime LastReadUtc;
        public string MapName="";
        public string ServerInfo="";
        public int AttackSpeed;
        public bool InGame;

        public DeveloperClientProbe(Process process,string outputPath)
        {
            Process=process;OutputPath=outputPath;Bridge=new QueuedPythonBridge(process);
        }
        public void Dispose()
        {
            try{Bridge.Dispose();}catch{}
            try{Process.Dispose();}catch{}
            try{File.Delete(OutputPath);}catch{}
        }
    }

    private readonly Dictionary<int,DeveloperClientProbe> developerClientProbes=new Dictionary<int,DeveloperClientProbe>();
    private TabPage developerPage;
    private Guna2CheckBox developerLinkedAttackCheck,waitExploitCheck,hybridWaitCheck;
    private NumericUpDown developerLinkedAttackCap,developerWaitDelay,developerWaitRange,hybridWaitInterval;
    private Button developerWaitButton;
    private Label developerLinkedAttackStatus,waitExploitStatus,hybridWaitStatus;
    private System.Windows.Forms.Timer developerLinkedAttackTimer;
    private bool developerWaitRunning;
    private long developerWaitStartedCalls;
    private int developerSelectedBaseSpeed;
    private int developerDonorSpeed;
    private int developerLinkedAttackSpeed;
    private int developerMatchedClients=1;
    private DateTime developerActivationNotBeforeUtc=DateTime.MinValue;
    private bool waitExploitApplying,waitExploitPreviousDeveloperRequested;
    private bool[] waitExploitPreviousModes,waitExploitPreviousFilters;
    private bool waitExploitPreviousFastTiming;
    private int waitExploitPreviousDelay,waitExploitPreviousTargetCap;
    private decimal waitExploitPreviousCap;

    private bool DeveloperLinkedAttackRequested { get{return developerLinkedAttackCheck!=null&&developerLinkedAttackCheck.Checked;} }
    private bool DeveloperLinkedAttackActive { get{return DeveloperLinkedAttackRequested&&developerMatchedClients==2&&developerLinkedAttackSpeed>0;} }
    private int DeveloperLinkedAttackSpeed { get{return developerLinkedAttackSpeed;} }
    private bool WaitExploitEnabled { get{return waitExploitCheck!=null&&waitExploitCheck.Checked;} }
    private bool HybridWaitRequested { get{return RouteOwnsCombat?activeRouteOptions.HybridAttack:hybridWaitCheck!=null&&hybridWaitCheck.Checked;} }
    private const int HybridCloseDistance=140;
    private bool HybridWaitRuntimeActive
    {
        get{return !RouteUsesAttackQueue&&HybridWaitRequested&&dmgStartCheck!=null&&dmgStartCheck.Checked&&automation!=null&&automation.AttackEnabled&&
            (automation.WaitDamage||automation.WaitDamageRange||automation.WaitArrow);}
    }

    private void InitializeDeveloperLinkedAttack()
    {
        developerPage=new TabPage("Geliştirici"){Name="developerPage",BackColor=StudioSurface,ForeColor=StudioText,AutoScroll=true};
        mainTabs.TabPages.Add(developerPage);
        AddCaption(developerPage,"İki istemci Wait testi",28,26,620,36,20f,StudioText);
        AddCaption(developerPage,"Karakter hızına yazmaz; pasif istemci eşleşmesini kontrollü Wait ritmine dönüştürür.",28,67,650,42,9.5f,StudioMuted);

        AddSectionCard(developerPage,"EŞLEŞME","Bağlı istemci saldırandır. Diğer istemciye hedef, hareket veya saldırı komutu gönderilmez.",28,126,650,80);
        developerLinkedAttackCheck=NewOption("developerLinkedAttackCheck","Pasif istemci eşleşmesini Wait'e bağla",50,220,410);
        developerLinkedAttackCap=NewNumber("developerLinkedAttackCap",530,216,105,50,int.MaxValue,50,5);
        developerPage.Controls.AddRange(new Control[]{developerLinkedAttackCheck,developerLinkedAttackCap});
        AddCaption(developerPage,"Wait hız hedefi",425,221,102,24,9f,StudioMuted);

        AddSectionCard(developerPage,"WAIT METİN TESTİ","Doğal karakter hızını korur; yalnızca yakındaki tek metin için Wait aralığını değiştirir.",28,292,650,80);
        AddCaption(developerPage,"Wait tabanı",50,388,130,24,9f,StudioMuted);
        developerWaitDelay=NewNumber("developerWaitDelay",185,384,105,200,1000,275,25);
        AddCaption(developerPage,"ms",298,389,40,24,9f,StudioMuted);
        AddCaption(developerPage,"Metin menzili",365,388,120,24,9f,StudioMuted);
        developerWaitRange=NewNumber("developerWaitRange",500,384,120,100,1500,500,50);
        developerWaitButton=NewPresetButton("WAIT TESTİNİ BAŞLAT",50,444,260);developerWaitButton.Name="developerWaitButton";
        developerLinkedAttackStatus=new Label{Name="developerLinkedAttackStatus",Text="Kapalı · önce eşleşmeyi etkinleştir",Location=new Point(50,504),Size=new Size(590,92),ForeColor=StudioMuted,BackColor=Color.Transparent};
        developerPage.Controls.AddRange(new Control[]{developerWaitDelay,developerWaitRange,developerWaitButton,developerLinkedAttackStatus});
        developerLinkedAttackCheck.CheckedChanged+=(s,e)=>
        {
            sessionDirty=true;
            if(!developerLinkedAttackCheck.Checked){StopDeveloperWait("Kapalı · önce eşleşmeyi etkinleştir");ResetDeveloperLinkedAttack("Kapalı · önce eşleşmeyi etkinleştir",true);DisposeDeveloperClientProbes();}
            else developerLinkedAttackStatus.Text="Aynı kanal ve haritadaki pasif istemci bekleniyor…";
            UpdatePerformanceStatus();
        };
        developerLinkedAttackCap.ValueChanged+=(s,e)=>{if(developerWaitRunning&&automation!=null)automation.AttackDelay=DeveloperEffectiveWaitDelay();RefreshDeveloperLinkedAttackState();};
        developerWaitDelay.ValueChanged+=(s,e)=>{if(developerWaitRunning&&automation!=null)automation.AttackDelay=DeveloperEffectiveWaitDelay();RefreshDeveloperLinkedAttackState();};
        developerWaitRange.ValueChanged+=(s,e)=>{if(developerWaitRunning&&automation!=null)automation.AttackRange=(int)developerWaitRange.Value;};
        developerWaitButton.Click+=(s,e)=>{if(developerWaitRunning)StopDeveloperWait("Test durduruldu · eşleşme hazır");else StartDeveloperWait();};
        developerLinkedAttackTimer=new System.Windows.Forms.Timer(components){Interval=750};
        developerLinkedAttackTimer.Tick+=(s,e)=>PollDeveloperLinkedAttackClients();
        developerLinkedAttackTimer.Start();
        waitExploitCheck=NewOption("waitExploitCheck","WAIT EXPLOIT · 500",344,252,290);
        waitExploitStatus=new Label{Name="waitExploitStatus",Text="Kapali",Location=new Point(344,280),Size=new Size(290,22),ForeColor=StudioMuted,BackColor=Color.Transparent};
        waitExploitCheck.CheckedChanged+=OnWaitExploitChanged;
        tabDamage.Controls.AddRange(new Control[]{waitExploitCheck,waitExploitStatus});
        hybridWaitCheck=NewOption("hybridWaitCheck","KARMA VURUŞ · normal animasyon + Wait",24,278,305);
        AddCaption(tabDamage,"Normal saldırı: Space basılı · hedef bitene kadar",24,318,305,24,9f,StudioMuted);
        hybridWaitInterval=NewNumber("hybridWaitInterval",185,312,105,750,5000,1500,250);
        hybridWaitInterval.Visible=false;hybridWaitInterval.Enabled=false;
        hybridWaitStatus=new Label{Name="hybridWaitStatus",Text="Kapalı · yalnız Wait çalışır",Location=new Point(24,352),Size=new Size(305,48),ForeColor=StudioMuted,BackColor=Color.Transparent};
        tabDamage.Controls.AddRange(new Control[]{hybridWaitCheck,hybridWaitInterval,hybridWaitStatus});
        hybridWaitCheck.CheckedChanged+=(s,e)=>{sessionDirty=true;PublishHybridAttackIntent();SaveAutomationSelections();UpdateHybridWaitStatus();UpdatePerformanceStatus();};
        hybridWaitInterval.ValueChanged+=(s,e)=>{sessionDirty=true;SaveAutomationSelections();UpdateHybridWaitStatus();};
        foreach(var mode in new[]{waitDmgCheck,waitDmgRangeCheck,waitArrowCheck,dmgStartCheck})mode.CheckedChanged+=(s,e)=>UpdateHybridWaitStatus();
        UpdateHybridWaitStatus();
        var hybridIntentTimer=new System.Windows.Forms.Timer(components){Interval=150};hybridIntentTimer.Tick+=(s,e)=>{if(clientConnected)PublishHybridAttackIntent();};hybridIntentTimer.Start();
        developerPage.AutoScrollMinSize=new Size(0,720);
        StyleControls(developerPage);
        foreach(Control control in new Control[]{developerLinkedAttackCheck,developerLinkedAttackCap,developerWaitDelay,developerWaitRange,developerWaitButton,developerLinkedAttackStatus})control.BringToFront();
        FormClosed+=(s,e)=>{StopDeveloperWait("Kapatıldı");DisposeDeveloperClientProbes();};
    }

    private void UpdateHybridWaitStatus()
    {
        if(hybridWaitStatus==null)return;
        if(!HybridWaitRequested){hybridWaitStatus.Text="Kapalı · yalnız Wait çalışır";return;}
        bool waitSelected=RouteOwnsCombat||(waitDmgCheck!=null&&waitDmgCheck.Checked)||(waitDmgRangeCheck!=null&&waitDmgRangeCheck.Checked)||(waitArrowCheck!=null&&waitArrowCheck.Checked);
        if(!waitSelected){hybridWaitStatus.Text="Hazır değil · bir çevre Wait modu seç";return;}
        if(RouteUsesAttackQueue)hybridWaitStatus.Text="Rota · normal vurus oyun kuyrugundan + mesafede Wait";
        else if(HybridWaitRuntimeActive)hybridWaitStatus.Text="Aktif · hedefin dibinde Space basılı + Wait · hedef bitene kadar";
        else hybridWaitStatus.Text="Hazır · saldırıyı başlatınca normal vuruş eklenir";
    }
    private void PublishHybridAttackIntent(){
        try{
            if(!clientConnected||memory==null)return;
            bool active=!emergencyLatched&&!MountSkillBusy&&HybridWaitRuntimeActive;
            Directory.CreateDirectory(runtimeFolder);
            double expiry=(DateTime.UtcNow.AddMilliseconds(800)-new DateTime(1970,1,1)).TotalSeconds;
            string content=(memory.Field_0197?.Property_0268?.Id??0)+"|"+expiry.ToString("R",CultureInfo.InvariantCulture)+"|"+(active?"1":"0")+"|"+(active?HybridTargetVidList():"");
            string pending=HybridTargetPath+".next";File.WriteAllText(pending,content);
            if(File.Exists(HybridTargetPath))File.Replace(pending,HybridTargetPath,null);else File.Move(pending,HybridTargetPath);
        }catch(IOException){}catch(UnauthorizedAccessException){}catch(InvalidOperationException){}
    }

    private void OnWaitExploitChanged(object sender,EventArgs e)
    {
        if(waitExploitApplying)return;
        waitExploitApplying=true;
        try{if(WaitExploitEnabled)EnableWaitExploit();else DisableWaitExploit();}
        finally{waitExploitApplying=false;}
    }

    private void EnableWaitExploit()
    {
        if(developerWaitRunning)StopDeveloperWait("Saldiri sekmesindeki Wait exploit etkin.");
        waitExploitPreviousModes=new[]{waitDmgRangeCheck.Checked,waitDmgCheck.Checked,targetDmgRangeCheck.Checked,targetDmgCheck.Checked,waitArrowCheck.Checked,targetArrowCheck.Checked};
        waitExploitPreviousFilters=new[]{dmgStoneCheck.Checked,dmgMonsterCheck.Checked,dmgPlayerCheck.Checked};
        waitExploitPreviousFastTiming=fastTimingCheck.Checked;
        waitExploitPreviousDelay=automation==null?attackDelayBar.Value:automation.AttackDelay;
        waitExploitPreviousTargetCap=automation==null?0:automation.MaxTargetsPerCycle;
        waitExploitPreviousCap=developerLinkedAttackCap.Value;
        waitExploitPreviousDeveloperRequested=developerLinkedAttackCheck.Checked;
        developerLinkedAttackCap.Value=Math.Min(developerLinkedAttackCap.Maximum,500);
        developerLinkedAttackCheck.Checked=true;
        waitDmgCheck.Checked=true;
        dmgStoneCheck.Checked=true;dmgMonsterCheck.Checked=true;dmgPlayerCheck.Checked=false;
        fastTimingCheck.Checked=true;
        SetWaitExploitControlsEnabled(false);
        ApplyTargetFilters();ApplyWaitExploitRuntime();
    }

    private void DisableWaitExploit()
    {
        SetWaitExploitControlsEnabled(true);
        if(waitExploitPreviousModes!=null&&waitExploitPreviousModes.Length==6)
        {
            waitDmgRangeCheck.Checked=waitExploitPreviousModes[0];waitDmgCheck.Checked=waitExploitPreviousModes[1];
            targetDmgRangeCheck.Checked=waitExploitPreviousModes[2];targetDmgCheck.Checked=waitExploitPreviousModes[3];
            waitArrowCheck.Checked=waitExploitPreviousModes[4];targetArrowCheck.Checked=waitExploitPreviousModes[5];
        }
        if(waitExploitPreviousFilters!=null&&waitExploitPreviousFilters.Length==3)
        {dmgStoneCheck.Checked=waitExploitPreviousFilters[0];dmgMonsterCheck.Checked=waitExploitPreviousFilters[1];dmgPlayerCheck.Checked=waitExploitPreviousFilters[2];}
        fastTimingCheck.Checked=waitExploitPreviousFastTiming;
        if(waitExploitPreviousCap>=developerLinkedAttackCap.Minimum&&waitExploitPreviousCap<=developerLinkedAttackCap.Maximum)developerLinkedAttackCap.Value=waitExploitPreviousCap;
        if(!waitExploitPreviousDeveloperRequested)developerLinkedAttackCheck.Checked=false;
        ApplyTargetFilters();
        if(automation!=null)
        {
            automation.WaitDamageRange=waitDmgRangeCheck.Checked;automation.WaitDamage=waitDmgCheck.Checked;
            automation.TargetDamageRange=targetDmgRangeCheck.Checked;automation.TargetDamage=targetDmgCheck.Checked;
            automation.WaitArrow=waitArrowCheck.Checked;automation.TargetArrow=targetArrowCheck.Checked;
            automation.AttackDelay=waitExploitPreviousDelay;automation.MaxTargetsPerCycle=waitExploitPreviousTargetCap;automation.FastTiming=fastTimingCheck.Checked;
            automation.AttackEnabled=dmgStartCheck.Checked;
            if(automation.AttackEnabled||automation.AutoPickup)automation.Start();else automation.Stop();
        }
        if(waitExploitStatus!=null)waitExploitStatus.Text="Kapali";
    }

    private void SetWaitExploitControlsEnabled(bool enabled)
    {
        foreach(Control control in new Control[]{waitDmgRangeCheck,waitDmgCheck,targetDmgRangeCheck,targetDmgCheck,waitArrowCheck,targetArrowCheck,dmgStoneCheck,dmgMonsterCheck,dmgPlayerCheck,attackDelayBar,fastTimingCheck})control.Enabled=enabled;
    }

    private void ApplyWaitExploitRuntime()
    {
        if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}
        if(!WaitExploitEnabled||automation==null)return;
        if(IsX86ClientMode)
        {
            automation.AttackEnabled=false;if(!automation.AutoPickup)automation.Stop();
            if(waitExploitStatus!=null)waitExploitStatus.Text="Bu istemcide Wait koprusu hazir degil";
            return;
        }
        if(!DeveloperLinkedAttackActive)
        {
            automation.AttackEnabled=false;if(!automation.AutoPickup)automation.Stop();
            if(waitExploitStatus!=null)waitExploitStatus.Text="500 modu · iki istemci eslesmesi bekleniyor";
            return;
        }
        automation.GroundStoneWait=false;
        automation.WaitDamage=true;automation.WaitDamageRange=false;automation.TargetDamage=false;automation.TargetDamageRange=false;automation.WaitArrow=false;automation.TargetArrow=false;
        automation.IncludeStones=true;automation.IncludeMonsters=true;automation.IncludePlayers=false;
        automation.AttackRange=attackRangeBar.Value;automation.AttackDelay=DeveloperEffectiveWaitDelay();automation.MaxTargetsPerCycle=0;automation.FastTiming=true;
        automation.AttackEnabled=dmgStartCheck.Checked;
        if(automation.AttackEnabled||automation.AutoPickup)automation.Start();else automation.Stop();
        if(waitExploitStatus!=null)waitExploitStatus.Text=(automation.AttackEnabled?"Aktif":"Hazir")+" · "+automation.AttackDelay+" ms · tas + canavar + patron";
    }

    private void PollDeveloperLinkedAttackClients()
    {
        if(!DeveloperLinkedAttackRequested)return;
        var selected=memory.Field_0197?.Property_0268;
        if(!connectionRequested||!clientConnected||selected==null)
        {
            ResetDeveloperLinkedAttack("Seçili saldıran istemcinin bağlantısı bekleniyor…",false);return;
        }
        try
        {
            var ids=Process.GetProcessesByName(ClientProcessName).Select(p=>p.Id).ToList();
            foreach(int stale in developerClientProbes.Keys.Where(id=>!ids.Contains(id)).ToList())
            {
                developerClientProbes[stale].Dispose();developerClientProbes.Remove(stale);
            }
            foreach(int pid in ids)
            {
                DeveloperClientProbe probe;
                if(!developerClientProbes.TryGetValue(pid,out probe))
                {
                    probe=new DeveloperClientProbe(Process.GetProcessById(pid),Path.Combine(runtimeFolder,"developer-client-"+pid+".txt"));
                    developerClientProbes.Add(pid,probe);
                }
                PollDeveloperClientProbe(probe);
            }
            RefreshDeveloperLinkedAttackState();
        }
        catch(Exception ex){ResetDeveloperLinkedAttack("Geliştirici testi beklemede: "+ex.Message,false);}
    }

    private void PollDeveloperClientProbe(DeveloperClientProbe probe)
    {
        if(probe.Process.HasExited)return;
        if(probe.Bridge.Pending)
        {
            if(!probe.Bridge.Poll())
            {
                if(DateTime.UtcNow-probe.QueuedAtUtc>TimeSpan.FromSeconds(6))probe.Bridge.Cancel();
                return;
            }
            if(File.Exists(probe.OutputPath))
            {
                var parts=File.ReadAllText(probe.OutputPath).Split('|');int speed;
                if(parts.Length==4&&int.TryParse(parts[2],out speed)&&speed>0)
                {
                    probe.MapName=parts[0].Trim();probe.ServerInfo=parts[1].Trim();probe.AttackSpeed=speed;probe.InGame=parts[3].Trim()=="1";probe.LastReadUtc=DateTime.UtcNow;
                }
            }
        }
        if(!probe.Bridge.Pending&&DateTime.UtcNow-probe.QueuedAtUtc>=TimeSpan.FromSeconds(1))
        {
            try{File.Delete(probe.OutputPath);}catch{}
            const string body="import sys\n_pl=_bg=_net=_ch=None\nfor _m in list(sys.modules.values()):\n try:\n  if _pl is None and hasattr(_m,'GetStatus') and hasattr(_m,'GetMainCharacterPosition'):_pl=_m\n  if _bg is None and hasattr(_m,'GetCurrentMapName'):_bg=_m\n  if _net is None and hasattr(_m,'GetServerInfo'):_net=_m\n  if _ch is None and hasattr(_m,'HasInstance') and hasattr(_m,'GetInstanceType'):_ch=_m\n except Exception:pass\nif _pl is None or _bg is None or _net is None or _ch is None:raise RuntimeError('client identity unavailable')\n_vid=int(_pl.GetMainCharacterIndex());_ingame=1 if _vid>0 and _ch.HasInstance(_vid) else 0\nf.write(str(_bg.GetCurrentMapName())+'|'+str(_net.GetServerInfo())+'|'+str(int(_pl.GetStatus(_pl.ATT_SPEED)))+'|'+str(_ingame))";
            probe.Bridge.Queue(LocalPythonScript(body,probe.OutputPath));probe.QueuedAtUtc=DateTime.UtcNow;
        }
    }

    private void RefreshDeveloperLinkedAttackState()
    {
        if(!DeveloperLinkedAttackRequested)return;
        var selectedProcess=memory.Field_0197?.Property_0268;
        DeveloperClientProbe selected;
        if(selectedProcess==null||!developerClientProbes.TryGetValue(selectedProcess.Id,out selected)||!FreshDeveloperProbe(selected))
        {
            ResetDeveloperLinkedAttack("Seçili istemcinin kanal ve harita bilgisi okunuyor…",false);return;
        }
        var donor=developerClientProbes.Values.Where(p=>p.Process.Id!=selected.Process.Id&&FreshDeveloperProbe(p)&&
            string.Equals(p.MapName,selected.MapName,StringComparison.OrdinalIgnoreCase)&&
            string.Equals(p.ServerInfo,selected.ServerInfo,StringComparison.OrdinalIgnoreCase)).OrderByDescending(p=>p.LastReadUtc).FirstOrDefault();
        if(donor==null)
        {
            ResetDeveloperLinkedAttack("Aynı kanal ve haritadaki pasif istemci bekleniyor · "+selected.ServerInfo+" · "+selected.MapName,false);return;
        }
        if(DateTime.UtcNow<developerActivationNotBeforeUtc)
        {
            developerLinkedAttackStatus.Text="Güvenli yeniden eşleşme bekleniyor…";return;
        }
        if(developerSelectedBaseSpeed<=0)developerSelectedBaseSpeed=selected.AttackSpeed;
        int combined=CombineDeveloperAttackSpeed(developerSelectedBaseSpeed,new[]{donor.AttackSpeed},(int)developerLinkedAttackCap.Value);
        bool changed=developerMatchedClients!=2||developerLinkedAttackSpeed!=combined||developerDonorSpeed!=donor.AttackSpeed;
        developerMatchedClients=2;developerDonorSpeed=donor.AttackSpeed;developerLinkedAttackSpeed=combined;
        long calls=automation==null?0:Math.Max(0,automation.CompletedAttackCalls-developerWaitStartedCalls);
        int waitDelay=DeveloperEffectiveWaitDelay();
        developerLinkedAttackStatus.Text=(developerWaitRunning?"WAIT çalışıyor":"Hazır · eşleşti")+" · "+selected.ServerInfo+" · "+selected.MapName+"\nDoğal hız "+developerSelectedBaseSpeed+" · pasif "+donor.AttackSpeed+" · hedef "+combined+" · Wait "+waitDelay+" ms"+(developerWaitRunning?" · çağrı "+calls:" · başlatılabilir");
        if(WaitExploitEnabled)ApplyWaitExploitRuntime();
        if(changed)sessionDirty=true;
        UpdateHybridWaitStatus();
        UpdatePerformanceStatus();
    }

    private static bool FreshDeveloperProbe(DeveloperClientProbe probe)
    {
        return probe!=null&&probe.InGame&&DateTime.UtcNow-probe.LastReadUtc<TimeSpan.FromSeconds(4)&&!string.IsNullOrWhiteSpace(probe.MapName)&&!string.IsNullOrWhiteSpace(probe.ServerInfo)&&probe.AttackSpeed>0;
    }

    private void ResetDeveloperLinkedAttack(string status,bool immediate)
    {
        bool wasActive=DeveloperLinkedAttackActive;
        if(developerWaitRunning)StopDeveloperWait(status);
        developerMatchedClients=1;developerSelectedBaseSpeed=0;developerDonorSpeed=0;developerLinkedAttackSpeed=0;
        if(wasActive&&!immediate)developerActivationNotBeforeUtc=DateTime.UtcNow.AddSeconds(3);
        else if(immediate)developerActivationNotBeforeUtc=DateTime.MinValue;
        if(developerLinkedAttackStatus!=null)developerLinkedAttackStatus.Text=status;
        if(WaitExploitEnabled)ApplyWaitExploitRuntime();
        if(wasActive)sessionDirty=true;
        UpdateHybridWaitStatus();
        UpdatePerformanceStatus();
    }

    private void StartDeveloperWait()
    {
        if(RouteOwnsCombat||isRecordingRoute){developerLinkedAttackStatus.Text="Önce rota kaydını/takibini durdur.";return;}
        if(WaitExploitEnabled){developerLinkedAttackStatus.Text="Başlatılamadı · saldırı sekmesindeki Wait exploit açık";return;}
        if(automation==null||!connectionRequested||!clientConnected||memory.Field_0197?.Property_0268==null)
        {developerLinkedAttackStatus.Text="Başlatılamadı · önce saldıran istemciye bağlan";return;}
        if(IsX86ClientMode){developerLinkedAttackStatus.Text="Başlatılamadı · Wait köprüsü bu istemcide hazır değil";return;}
        if(!DeveloperLinkedAttackActive){developerLinkedAttackStatus.Text="Başlatılamadı · aynı kanal ve haritada pasif istemci eşleşmesi gerekli";return;}
        if(taskDungeonRunning||normalBotRequested||Field_02DF||dmgStartCheck.Checked||autoPickCheck.Checked||automation.AttackEnabled||automation.AutoPickup)
        {developerLinkedAttackStatus.Text="Başlatılamadı · normal bot, saldırı, toplama ve zindan kapalı olmalı";return;}
        automation.GroundStoneWait=false;
        automation.WaitDamage=true;automation.WaitDamageRange=false;automation.TargetDamage=false;automation.TargetDamageRange=false;automation.WaitArrow=false;automation.TargetArrow=false;
        automation.IncludeStones=true;automation.IncludeMonsters=false;automation.IncludePlayers=false;
        automation.AttackRange=(int)developerWaitRange.Value;automation.AttackDelay=DeveloperEffectiveWaitDelay();automation.MaxTargetsPerCycle=1;automation.FastTiming=true;
        developerWaitStartedCalls=automation.CompletedAttackCalls;developerWaitRunning=true;automation.AttackEnabled=true;automation.Start();
        developerWaitButton.Text="WAIT TESTİNİ DURDUR";RefreshDeveloperLinkedAttackState();
    }

    private void StopDeveloperWait(string status)
    {
        bool wasRunning=developerWaitRunning;developerWaitRunning=false;
        if(developerWaitButton!=null)developerWaitButton.Text="WAIT TESTİNİ BAŞLAT";
        if(automation!=null)
        {
            automation.AttackEnabled=false;if(!automation.AutoPickup)automation.Stop();
            automation.GroundStoneWait=taskDungeonRunning&&taskUiStage==3;
            automation.WaitDamage=waitDmgCheck!=null&&waitDmgCheck.Checked;automation.WaitDamageRange=waitDmgRangeCheck!=null&&waitDmgRangeCheck.Checked;
            automation.TargetDamage=targetDmgCheck!=null&&targetDmgCheck.Checked;automation.TargetDamageRange=targetDmgRangeCheck!=null&&targetDmgRangeCheck.Checked;
            automation.WaitArrow=waitArrowCheck!=null&&waitArrowCheck.Checked;automation.TargetArrow=targetArrowCheck!=null&&targetArrowCheck.Checked;
            ApplyTargetFilters();automation.AttackRange=attackRangeBar.Value;automation.AttackDelay=TurboAttackEnabled?ActiveTurboAttackDelay:attackDelayBar.Value;
            automation.MaxTargetsPerCycle=TurboAttackEnabled?TurboAttackTargetCap:0;automation.FastTiming=TurboAttackEnabled||fastTimingCheck.Checked;
        }
        if(developerLinkedAttackStatus!=null)developerLinkedAttackStatus.Text=status;
    }

    private int DeveloperEffectiveWaitDelay()
    {
        return CalculateDeveloperLinkedWaitDelay((int)developerWaitDelay.Value,developerSelectedBaseSpeed,developerLinkedAttackSpeed);
    }

    internal static int CombineDeveloperAttackSpeed(int selectedSpeed,IEnumerable<int> donorSpeeds,int cap)
    {
        int safeCap=Math.Max(1,cap);long total=Math.Max(1,selectedSpeed);
        foreach(int speed in donorSpeeds??Enumerable.Empty<int>())if(speed>0)total=Math.Min((long)safeCap,total+speed);
        return (int)Math.Min(safeCap,total);
    }

    internal static int CalculateDeveloperLinkedWaitDelay(int baseDelay,int naturalSpeed,int targetSpeed)
    {
        if(baseDelay<=0||naturalSpeed<=0||targetSpeed<=0)return Math.Max(1,baseDelay);
        return Math.Max(1,Math.Min(2000,(int)Math.Round(baseDelay*(double)naturalSpeed/targetSpeed)));
    }

    private void DisposeDeveloperClientProbes()
    {
        foreach(var probe in developerClientProbes.Values)probe.Dispose();developerClientProbes.Clear();
    }
}
