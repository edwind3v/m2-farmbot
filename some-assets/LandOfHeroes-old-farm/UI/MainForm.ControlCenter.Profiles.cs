using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    private string SafeProfileName(string value){var invalid=Path.GetInvalidFileNameChars();return new string((value??"Profil").Trim().Select(c=>invalid.Contains(c)?'_':c).ToArray());}
    private string ProfilePath(string name){return Path.Combine(ProfileFolder,SafeProfileName(name)+".onurprofil.xml");}
    private void RefreshProfileList()
    {
        if(profileCombo==null)return;string keep=profileCombo.Text;Directory.CreateDirectory(ProfileFolder);profileCombo.Items.Clear();
        foreach(var f in Directory.GetFiles(ProfileFolder,"*.onurprofil.xml"))profileCombo.Items.Add(Path.GetFileNameWithoutExtension(f));profileCombo.Text=keep;
        if(string.IsNullOrWhiteSpace(profileCombo.Text)){string n;clientNames.TryGetValue(boundPid,out n);profileCombo.Text=string.IsNullOrWhiteSpace(n)?"Landof-"+(boundPid==0?"Profil":boundPid.ToString()):n;}
    }
    private AdvancedProfile CaptureProfile(string name)
    {
        SyncAdvancedStateFromUi();
        return new AdvancedProfile{Name=name,State=advancedState,
            Skills=selectedSkills.Select(p=>new IntervalEntry{Id=p.Key,Seconds=p.Value}).ToList(),
            Items=scheduledItems.Select(p=>new IntervalEntry{Id=p.Key,Seconds=p.Value}).ToList(),
            WalkStones=walkMetinCheck.Checked,WalkMonsters=walkCanavarCheck.Checked,
            AttackStones=dmgStoneCheck.Checked,AttackMonsters=dmgMonsterCheck.Checked,
            SearchWhenEmpty=searchWhenEmptyCheck.Checked,SearchRange=searchRange.Value,
            ApproachDistance=approachDistance.Value,ApproachOnly=approachOnlyCheck.Checked,Task=taskSequence};
    }
    private void ApplyProfile(AdvancedProfile p)
    {
        if(p==null)return;advancedState=p.State??new AdvancedState();advancedState.Normalize();
        selectedSkills.Clear();foreach(var v in p.Skills??new List<IntervalEntry>())selectedSkills[v.Id]=Math.Max(1,v.Seconds);
        scheduledItems.Clear();foreach(var v in p.Items??new List<IntervalEntry>())scheduledItems[v.Id]=Math.Max(1,v.Seconds);
        walkMetinCheck.Checked=p.WalkStones;walkCanavarCheck.Checked=p.WalkMonsters;dmgStoneCheck.Checked=p.AttackStones;dmgMonsterCheck.Checked=p.AttackMonsters;
        searchWhenEmptyCheck.Checked=p.SearchWhenEmpty;searchRange.Value=Math.Max(searchRange.Minimum,Math.Min(searchRange.Maximum,p.SearchRange));
        approachDistance.Value=Math.Max(approachDistance.Minimum,Math.Min(approachDistance.Maximum,p.ApproachDistance));approachOnlyCheck.Checked=p.ApproachOnly;
        taskSequence=TaskSequence.Normalize(p.Task??new TaskSequence());dungeonTasks.Dungeons[dungeonTasks.SelectedIndex]=taskSequence;ApplyAdvancedStateToUi();RefreshDungeonProfileSelectors();RefreshTaskStageList(0);AutoSaveTaskSequence();sessionDirty=true;SaveAdvancedState();
    }
    private void SaveNamedProfile(){string name=profileCombo.Text.Trim();if(name.Length==0)return;AdvancedStore.Save(ProfilePath(name),CaptureProfile(name));RefreshProfileList();profileCombo.Text=name;RecordActivity("Profil",name+" kaydedildi.");}
    private void LoadNamedProfile(){LoadProfileByName(profileCombo.Text);}
    private void LoadProfileByName(string name){try{string path=ProfilePath(name);if(!File.Exists(path))return;ApplyProfile(AdvancedStore.Load<AdvancedProfile>(path));if(profileCombo!=null)profileCombo.Text=name;RecordActivity("Profil",name+" yüklendi.");}catch(Exception ex){RecordActivity("Profil","Yüklenemedi: "+ex.Message);}}
    private void DeleteNamedProfile(){string path=ProfilePath(profileCombo.Text);if(File.Exists(path))File.Delete(path);RefreshProfileList();}
    private void ExportNamedProfile(){string name=profileCombo.Text.Trim();if(name.Length==0)return;using(var d=new SaveFileDialog{Filter="Onur profil dosyası|*.onurprofil.xml",FileName=SafeProfileName(name)+".onurprofil.xml"})if(d.ShowDialog(controlCenterForm)==DialogResult.OK)AdvancedStore.Save(d.FileName,CaptureProfile(name));}
    private void ImportNamedProfile(){using(var d=new OpenFileDialog{Filter="Onur profil dosyası|*.onurprofil.xml"})if(d.ShowDialog(controlCenterForm)==DialogResult.OK){var p=AdvancedStore.Load<AdvancedProfile>(d.FileName);ApplyProfile(p);AdvancedStore.Save(ProfilePath(p.Name),p);RefreshProfileList();profileCombo.Text=p.Name;}}
}