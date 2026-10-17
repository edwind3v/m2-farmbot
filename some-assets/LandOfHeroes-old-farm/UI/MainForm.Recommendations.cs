using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
public partial class MainForm
{
    private const int TurboAttackDelay=275,TurboAttackTargetCap=4;
    private int clientTurboAttackDelay=TurboAttackDelay;
    private Guna2Button turboAttackButton;
    private bool turboAttackMode,turboTimingChange;
    private int turboPreviousDelay=190,turboPreviousTargetCap;
    private int dungeonPreviousDelay=190,dungeonPreviousTargetCap;
    private bool TurboAttackEnabled { get { return turboAttackMode; } }
    private int ActiveTurboAttackDelay { get { return taskDungeonRunning?DungeonAttackDelay:clientTurboAttackDelay; } }
    private int SavedNormalAttackDelay { get { return TurboAttackEnabled?turboPreviousDelay:attackDelayBar.Value; } }

    private void InitializeRecommendations()
    {
        tabDamage.AutoScroll=true;tabDamage.AutoScrollMinSize=new Size(0,860);fastTimingCheck.Top=658;
        turboAttackButton=new Guna2Button{Name="turboAttackButton",Text="TURBO WAIT: KAPALI",Location=new Point(24,686),Size=new Size(610,34),BorderRadius=7,FillColor=Color.FromArgb(40,62,77),ForeColor=Color.White,Font=new Font("Segoe UI Semibold",10f)};
        turboAttackButton.Click+=ToggleTurboAttack;tabDamage.Controls.Add(turboAttackButton);
        AddCaption(tabDamage,"İSTEMCİYE GÖRE STABİL VURUŞ ÖNERİLERİ",24,730,620,20,9f,StudioAccent);
        AddPreset("attackFastPreset","Seri · 180 ms",24,756,180,true);
        AddPreset("attackBalancedPreset","Önerilen · 190 ms",232,756,190,true);
        AddPreset("attackCalmPreset","Stabil · 200 ms",440,756,200,true);
        AddCaption(tabDamage,"Turbo kapalıyken normal vuruş aralığını buradan seçebilirsiniz.",24,792,620,34,9f,StudioMuted);
        AddPreset("pickupFastPreset","1 ms",344,450,1,false,88);AddPreset("pickupBalancedPreset","50 ms",445,450,50,false,88);AddPreset("pickupCalmPreset","150 ms",546,450,150,false,88);
        var timingStatus=new Label{Name="timingStatus",Location=new Point(22,568),Size=new Size(280,90),ForeColor=StudioMuted,Text="Saldırı ölçümü hazır."};leftPanel.Controls.Add(timingStatus);
        var timingTimer=new System.Windows.Forms.Timer(components){Interval=500};timingTimer.Tick+=(s,e)=>{if(automation!=null)timingStatus.Text="Tamamlanan çağrı: "+automation.CompletedAttackCalls+"\nSon tarama: "+automation.LastCycleMilliseconds+" ms\n"+(automation.LastError??"Çağrı sayısı gerçek hasar sayısı değildir.");};timingTimer.Start();
        attackDelayBar.ValueChanged+=(s,e)=>{if(automation!=null)OnAttackDelayChanged(s,null);};pickDelayBar.ValueChanged+=(s,e)=>{if(automation!=null)OnPickupDelayChanged(s,null);};attackRangeBar.ValueChanged+=(s,e)=>{if(automation!=null)OnAttackRangeChanged(s,null);};UpdateTurboAttackButton();
    }
    private void ToggleTurboAttack(object sender,EventArgs e){SetTurboAttackMode(!turboAttackMode);}
    private void SetTurboAttackMode(bool enabled)
    {
        if(enabled==turboAttackMode){UpdateTurboAttackButton();if(enabled)ApplyTurboAttackRuntime();return;}
        if(enabled)
        {
            if(attackDelayBar.Value!=ActiveTurboAttackDelay)turboPreviousDelay=attackDelayBar.Value;if(automation!=null)turboPreviousTargetCap=automation.MaxTargetsPerCycle;
            turboAttackMode=true;if(IsX86ClientMode)RequestX86BotState(false);approachOnlyCheck.Checked=true;if(!waitDmgRangeCheck.Checked)waitDmgCheck.Checked=true;turboTimingChange=true;attackDelayBar.Value=ActiveTurboAttackDelay;turboTimingChange=false;attackDelayBar.Enabled=false;fastTimingCheck.Checked=true;ApplyTurboAttackRuntime();
        }
        else
        {
            turboAttackMode=false;attackDelayBar.Enabled=!taskDungeonRunning;
            if(taskDungeonRunning)ApplyDungeonAttackSettings();else{turboTimingChange=true;attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,turboPreviousDelay));turboTimingChange=false;if(automation!=null){automation.AttackDelay=attackDelayBar.Value;automation.MaxTargetsPerCycle=turboPreviousTargetCap;}attackDelayLbl.Text="Bekleme (ms): "+attackDelayBar.Value;}
        }
        UpdateTurboAttackButton();sessionDirty=true;
    }
    private void UpdateTurboAttackButton(){if(turboAttackButton==null)return;turboAttackButton.Text=TurboAttackEnabled?"TURBO WAIT: AÇIK · "+ActiveTurboAttackDelay+" ms":"TURBO WAIT: KAPALI";turboAttackButton.FillColor=TurboAttackEnabled?Color.FromArgb(220,38,38):Color.FromArgb(40,62,77);}
    private void ApplyTurboAttackRuntime(){if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}if(!TurboAttackEnabled||automation==null)return;automation.FastTiming=true;automation.AttackDelay=ActiveTurboAttackDelay;automation.MaxTargetsPerCycle=TurboAttackTargetCap;attackDelayLbl.Text="Wait ritmi: "+ActiveTurboAttackDelay+" ms · TURBO";}
    private void CaptureAttackTimingForDungeon(){dungeonPreviousDelay=SavedNormalAttackDelay;dungeonPreviousTargetCap=automation==null?0:automation.MaxTargetsPerCycle;}
    private void RestoreAttackTimingAfterDungeon(){if(TurboAttackEnabled){attackDelayBar.Enabled=false;ApplyTurboAttackRuntime();return;}turboTimingChange=true;attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,dungeonPreviousDelay));turboTimingChange=false;attackDelayBar.Enabled=true;if(automation!=null){automation.AttackDelay=attackDelayBar.Value;automation.MaxTargetsPerCycle=dungeonPreviousTargetCap;}attackDelayLbl.Text="Bekleme (ms): "+attackDelayBar.Value;}
    private void AddPreset(string name,string caption,int x,int y,int delay,bool attack,int width=194)
    {
        var button=new Guna2Button{Name=name,Text=caption,Location=new Point(x,y),Size=new Size(width,30),BorderRadius=6,FillColor=Color.FromArgb(40,62,77),ForeColor=StudioText,Font=new Font("Segoe UI",9f)};
        button.Click+=(s,e)=>{if(attack){if(TurboAttackEnabled){turboPreviousDelay=delay;ApplyTurboAttackRuntime();}else attackDelayBar.Value=delay;fastTimingCheck.Checked=true;OnAttackDelayChanged(s,null);}else{pickDelayBar.Value=delay;OnPickupDelayChanged(s,null);}};tabDamage.Controls.Add(button);
    }
}
