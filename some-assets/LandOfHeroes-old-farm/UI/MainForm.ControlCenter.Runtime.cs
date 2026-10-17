using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private async Task AdvancedPulseAsync()
    {
        if(advancedPulseBusy||IsDisposed||Disposing)return;advancedPulseBusy=true;
        try
        {
            bool connected=false;try{connected=clientConnected&&memory.Field_0197?.Property_0268!=null&&!memory.Field_0197.Property_0268.HasExited;}catch{}
            if(observedConnection&&connected&&!previousConnection){advancedState.Stats.Reconnects++;RecordActivity("Bağlantı","İstemci yeniden bağlandı.");}
            observedConnection=true;previousConnection=connected;if(connected)ObserveTargets();
            if(advancedState.Rules.Any(r=>r.Enabled&&r.Condition=="Can (%) altında")&&connected&&DateTime.UtcNow>=nextHealthReadUtc)
            {
                nextHealthReadUtc=DateTime.UtcNow.AddSeconds(4);
                try{string hp=await GamePythonAsync("import player\nh=player.GetStatus(player.HP)\nm=player.GetStatus(player.MAX_HP)\nf.write(str((100.0*h/m) if m else -1))",CancellationToken.None);double parsed;if(double.TryParse(hp,NumberStyles.Float,CultureInfo.InvariantCulture,out parsed))currentHealthPercent=parsed;}catch{}
            }
            await EvaluateRulesAsync(connected);UpdateStatsUi();UpdateOverlay();RefreshClientDashboard();
        }
        finally{advancedPulseBusy=false;}
    }
    private void ObserveTargets()
    {
        List<EntityState> now;try{now=ReadRadarEntities().Where(e=>e.Property_03CA==AddressCatalog.Field_035B||e.Property_03CA==AddressCatalog.Field_035C).ToList();}catch{return;}
        if(now.Count>0)lastHostileSeenUtc=DateTime.UtcNow;bool active=normalBotRequested||Field_02DF||taskDungeonRunning||dmgStartCheck.Checked;
        if(active)foreach(var gone in observedHostiles.Values.Where(e=>now.All(n=>n.Property_03C9!=e.Property_03C9)).ToList())
        {
            advancedState.Stats.Targets++;if(gone.Property_03CA==AddressCatalog.Field_035B)advancedState.Stats.Stones++;else advancedState.Stats.Monsters++;
            if(gone.Property_03C9==currentObservedTarget){advancedState.Stats.TotalKillMilliseconds+=(DateTime.UtcNow-currentTargetSinceUtc).TotalMilliseconds;advancedState.Stats.TimedKills++;currentObservedTarget=0;}
        }
        observedHostiles.Clear();foreach(var e in now)observedHostiles[e.Property_03C9]=e;
        int target=approachTargetId;if(target!=0&&target!=currentObservedTarget){currentObservedTarget=target;currentTargetSinceUtc=DateTime.UtcNow;RecordActivity("Hedef","Yeni hedef · VID "+target);}
    }
    private async Task EvaluateRulesAsync(bool connected)
    {
        if(emergencyLatched||rulesSafetyPaused)return;
        foreach(var r in advancedState.Rules.Where(v=>v.Enabled).ToList())
        {
            if((DateTime.UtcNow-r.LastRunUtc).TotalSeconds<Math.Max(1,r.CooldownSeconds)||!RuleConditionTrue(r,connected))continue;
            r.LastRunUtc=DateTime.UtcNow;await ExecuteRuleAsync(r);
        }
    }
    private bool RuleConditionTrue(AutomationRule r,bool connected)
    {
        double value;double.TryParse((r.Value??"").Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out value);
        switch(r.Condition)
        {
            case "Can (%) altında":return currentHealthPercent>=0&&currentHealthPercent<value;
            case "Hedef yok (sn)":return (DateTime.UtcNow-lastHostileSeenUtc).TotalSeconds>=value;
            case "Envanter doluluk (%) üstünde":return inventoryOccupied*100.0/470.0>=value;
            case "Bağlantı koptu":return connectionRequested&&!connected;
            case "Takılma sayısı üstünde":return advancedState.Stats.Recoveries>=value;
            case "Çalışma süresi (dk) üstünde":return (DateTime.UtcNow-advancedState.Stats.StartedUtc).TotalMinutes>=value;
            case "Eşya adedi altında":
                var p=(r.Value??"").Split(':');uint id;long count;if(p.Length!=2||!uint.TryParse(p[0],out id)||!long.TryParse(p[1],out count))return false;long have;observedItemCounts.TryGetValue(id,out have);return have<count;
            default:return false;
        }
    }
    private async Task ExecuteRuleAsync(AutomationRule r)
    {
        if(emergencyLatched)return;
        RecordActivity("Kural",r.Condition+" → "+r.Action);
        switch(r.Action)
        {
            case "Uyarı yaz":break;
            case "Normal botu başlat":if(!normalBotRequested&&!Field_02DF)ToggleNormalBot();break;
            case "Normal botu durdur":if(normalBotRequested||Field_02DF)StopNormalBotByUser();break;
            case "Zindanı başlat":if(taskCancellation==null){var ignored=RunTaskSequenceAsync(true);}break;
            case "Zindanı durdur":StopTaskSequence("Otomasyon kuralı durdurdu.");break;
            case "Bağlantıyı yenile":connectionRequested=true;await ConnectAutomaticallyAsync();break;
            case "Eşya kullanımına ekle":
                var p=(r.Argument??"").Split(':');uint id;int sec;if(p.Length>0&&uint.TryParse(p[0],out id)){if(!(p.Length>1&&int.TryParse(p[1],out sec)))sec=5;scheduledItems[id]=Math.Max(1,sec);sessionAuto.Checked=true;sessionDirty=true;}break;
            case "Profil yükle":LoadProfileByName(r.Argument);break;
            case "Ekran bilgisini aç":if(overlayForm==null||overlayForm.IsDisposed)ToggleOverlay();break;
        }
    }

    private void RecordRecovery(string message){advancedState.Stats.Recoveries++;RecordActivity("Kurtarma",message);UpdateStatsUi();}
    private void RecordActivity(string category,string message)
    {
        string line=DateTime.Now.ToString("HH:mm:ss")+" · "+category+" · "+message;activityEntries.Add(line);if(activityEntries.Count>500)activityEntries.RemoveAt(0);
        if(routeHistoryList!=null&&!routeHistoryList.IsDisposed){routeHistoryList.Items.Add(line);while(routeHistoryList.Items.Count>500)routeHistoryList.Items.RemoveAt(0);routeHistoryList.TopIndex=Math.Max(0,routeHistoryList.Items.Count-1);}
        if(activityList!=null&&!activityList.IsDisposed){activityList.Items.Add(line);while(activityList.Items.Count>500)activityList.Items.RemoveAt(0);activityList.TopIndex=Math.Max(0,activityList.Items.Count-1);}
        try{Directory.CreateDirectory(ActivityFolder);File.AppendAllText(Path.Combine(ActivityFolder,DateTime.Today.ToString("yyyy-MM-dd")+".log"),line+Environment.NewLine);}catch{}
    }
    private void UpdateStatsUi()
    {
        if(advancedState?.Stats==null)return;var s=advancedState.Stats;double hours=Math.Max(1.0/3600,(DateTime.UtcNow-s.StartedUtc).TotalHours);double average=s.TimedKills==0?0:s.TotalKillMilliseconds/s.TimedKills/1000.0;
        string text="Çalışma: "+(DateTime.UtcNow-s.StartedUtc).ToString("hh\\:mm\\:ss")+
            "\nSaatlik hedef: "+(s.Targets/hours).ToString("0.0")+
            "\nToplam hedef: "+s.Targets+"\nTaş: "+s.Stones+"\nCanavar: "+s.Monsters+
            "\nToplanan eşya: "+s.ItemsGained+"\nKullanılan eşya: "+s.ItemsUsed+
            "\nOrtalama kesim: "+average.ToString("0.0")+" sn\nTakılma kurtarma: "+s.Recoveries+"\nYeniden bağlantı: "+s.Reconnects+
            "\nEnvanter: "+inventoryOccupied+" / 470";
        if(statsLabel!=null)statsLabel.Text=text;if(healthLabel!=null)healthLabel.Text=currentHealthPercent<0?"Can: okunmadı":"Can: %"+currentHealthPercent.ToString("0.0");
    }
    private void ResetAdvancedStats(){advancedState.Stats=new AdvancedStats();observedHostiles.Clear();RecordActivity("İstatistik","Sayaçlar sıfırlandı.");SaveAdvancedState();}
    private void ExportActivityLog(){using(var d=new SaveFileDialog{Filter="Metin günlüğü|*.txt",FileName="onur-gunluk-"+DateTime.Now.ToString("yyyyMMdd-HHmm")+".txt"})if(d.ShowDialog(controlCenterForm)==DialogResult.OK)File.WriteAllLines(d.FileName,activityEntries);}

    private void ToggleOverlay()
    {
        if(overlayForm!=null&&!overlayForm.IsDisposed){overlayForm.Close();overlayForm=null;advancedState.OverlayEnabled=false;SaveAdvancedState();return;}
        overlayForm=new Form{FormBorderStyle=FormBorderStyle.None,ClientSize=new System.Drawing.Size(350,124),TopMost=true,ShowInTaskbar=false,BackColor=System.Drawing.Color.FromArgb(10,17,29),ForeColor=System.Drawing.Color.White,Opacity=.92,StartPosition=FormStartPosition.Manual};
        var area=Screen.PrimaryScreen.WorkingArea;overlayForm.Location=new System.Drawing.Point(area.Right-overlayForm.Width-14,area.Top+14);
        overlayStatus=new Label{Left=12,Top=10,Width=326,Height=25,Font=new System.Drawing.Font("Segoe UI Semibold",10f),ForeColor=System.Drawing.Color.FromArgb(117,132,255)};
        overlayTarget=new Label{Left=12,Top=38,Width=326,Height=22};var toggle=PlainButton("Bot başlat / durdur",12,75,170);var close=PlainButton("Gizle",194,75,70);
        toggle.Click+=(s,e)=>ToggleNormalBot();close.Click+=(s,e)=>ToggleOverlay();overlayForm.Controls.AddRange(new Control[]{overlayStatus,overlayTarget,toggle,close});overlayForm.Show(this);advancedState.OverlayEnabled=true;SaveAdvancedState();UpdateOverlay();
    }
    private void UpdateOverlay()
    {
        if(overlayForm==null||overlayForm.IsDisposed)return;var s=advancedState.Stats;
        overlayStatus.Text=(clientConnected?"BAĞLI":"BAĞLANTI BEKLENİYOR")+" · "+(taskDungeonRunning?"ZİNDAN":normalBotRequested||Field_02DF?"BOT ÇALIŞIYOR":"BEKLİYOR");
        overlayTarget.Text="PID "+boundPid+" · Hedef "+(currentObservedTarget==0?"yok":currentObservedTarget.ToString())+" · Kesim "+s.Targets+" · "+(DateTime.UtcNow-s.StartedUtc).ToString("hh\\:mm\\:ss");
    }
}
