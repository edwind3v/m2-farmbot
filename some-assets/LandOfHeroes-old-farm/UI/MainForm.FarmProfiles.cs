using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

public partial class MainForm
{
    private Label farmProfileStatus;
    private string FarmProfilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"onur-farm-profili.ini"); } }
    private void InitializeFarmProfiles()
    {
        AddSectionCard(performancePage,"AYAR PROFİLİ","Hız, saldırı, toplama ve hareket ayarlarını tek dosyada saklar.",28,830,650,154);
        var save=NewPresetButton("Profili kaydet",50,920,170);var load=NewPresetButton("Profili yükle",238,920,170);var optimized=NewPresetButton("Önerilen farm",426,920,210);
        save.Name="saveFarmProfile";load.Name="loadFarmProfile";optimized.Name="optimizedFarmProfile";
        save.Click+=(s,e)=>SaveFarmProfile();load.Click+=(s,e)=>LoadFarmProfile(false);optimized.Click+=(s,e)=>ApplyOptimizedFarmProfile();
        performancePage.Controls.AddRange(new Control[]{save,load,optimized});
        farmProfileStatus=new Label{Name="farmProfileStatus",Text="Profil hazır",Location=new Point(50,966),Size=new Size(580,24),ForeColor=StudioMuted};performancePage.Controls.Add(farmProfileStatus);
        StyleControls(performancePage);if(File.Exists(FarmProfilePath))LoadFarmProfile(true);
    }
    private void ApplyOptimizedFarmProfile()
    {
        if(TurboAttackEnabled){turboPreviousDelay=190;ApplyTurboAttackRuntime();}else attackDelayBar.Value=190;fastTimingCheck.Checked=true;attackSpeedValue.Value=200;moveSpeedValue.Value=260;
        attackSpeedCheck.Checked=true;moveSpeedCheck.Checked=true;rangePickupCheck.Checked=true;rangePickupInterval.Value=250;searchWhenEmptyCheck.Checked=true;
        farmProfileStatus.Text="Önerilen: 190 ms · saldırı 200 · yürüme 260";sessionDirty=true;UpdatePerformanceStatus();
    }
    private void SaveFarmProfile()
    {
        try { File.WriteAllLines(FarmProfilePath,new[]{"version=2","attack_delay="+SavedNormalAttackDelay,"turbo_mode="+TurboAttackEnabled,"attack_range="+attackRangeBar.Value,"pickup_delay="+pickDelayBar.Value,"fast_timing="+fastTimingCheck.Checked,"attack_speed="+attackSpeedValue.Value,"move_speed="+moveSpeedValue.Value,"attack_speed_on="+attackSpeedCheck.Checked,"move_speed_on="+moveSpeedCheck.Checked,"range_pickup="+rangePickupCheck.Checked,"range_pickup_interval="+rangePickupInterval.Value,"camera_distance="+cameraDistanceCheck.Checked,"camera_distance_value="+cameraDistanceValue.Value,"approach_only="+approachOnlyCheck.Checked,"approach_distance="+approachDistance.Value,"search_empty="+searchWhenEmptyCheck.Checked,"search_range="+searchRange.Value,"walk_stone="+walkMetinCheck.Checked,"walk_monster="+walkCanavarCheck.Checked});farmProfileStatus.Text="Profil kaydedildi · açılışta otomatik yüklenir"; }
        catch(Exception ex){farmProfileStatus.Text="Profil kaydedilemedi: "+ex.Message;}
    }
    private void LoadFarmProfile(bool automatic)
    {
        try { if(!File.Exists(FarmProfilePath)){farmProfileStatus.Text="Kaydedilmiş profil yok";return;}var map=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);foreach(var line in File.ReadAllLines(FarmProfilePath)){int p=line.IndexOf('=');if(p>0)map[line.Substring(0,p)]=line.Substring(p+1);}
            SetValue(map,"attack_delay",attackDelayBar);SetValue(map,"attack_range",attackRangeBar);SetValue(map,"pickup_delay",pickDelayBar);SetValue(map,"attack_speed",attackSpeedValue);SetValue(map,"move_speed",moveSpeedValue);SetValue(map,"range_pickup_interval",rangePickupInterval);SetValue(map,"camera_distance_value",cameraDistanceValue);SetValue(map,"approach_distance",approachDistance);SetValue(map,"search_range",searchRange);if(searchRange.Value<10000)searchRange.Value=20000;
            SetCheck(map,"fast_timing",fastTimingCheck);bool turbo;if(map.ContainsKey("turbo_mode")&&bool.TryParse(map["turbo_mode"],out turbo))SetTurboAttackMode(turbo);SetCheck(map,"attack_speed_on",attackSpeedCheck);SetCheck(map,"move_speed_on",moveSpeedCheck);SetCheck(map,"range_pickup",rangePickupCheck);SetCheck(map,"camera_distance",cameraDistanceCheck);SetCheck(map,"approach_only",approachOnlyCheck);SetCheck(map,"search_empty",searchWhenEmptyCheck);SetCheck(map,"walk_stone",walkMetinCheck);SetCheck(map,"walk_monster",walkCanavarCheck);
            farmProfileStatus.Text=automatic?"Kayıtlı profil otomatik yüklendi":"Profil yüklendi";sessionDirty=true;UpdatePerformanceStatus(); }
        catch(Exception ex){farmProfileStatus.Text="Profil yüklenemedi: "+ex.Message;}
    }
    private static void SetValue(Dictionary<string,string> map,string key,NumericUpDown control){decimal v;if(map.ContainsKey(key)&&decimal.TryParse(map[key],NumberStyles.Integer,CultureInfo.InvariantCulture,out v))control.Value=Math.Max(control.Minimum,Math.Min(control.Maximum,v));}
    private static void SetValue(Dictionary<string,string> map,string key,Guna.UI2.WinForms.Guna2TrackBar control){int v;if(map.ContainsKey(key)&&int.TryParse(map[key],out v))control.Value=Math.Max(control.Minimum,Math.Min(control.Maximum,v));}
    private static void SetCheck(Dictionary<string,string> map,string key,Guna.UI2.WinForms.Guna2CheckBox control){bool v;if(map.ContainsKey(key)&&bool.TryParse(map[key],out v))control.Checked=v;}
}