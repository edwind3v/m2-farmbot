using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class MainForm
{
    private ComboBox taskDungeonSelector,flowDungeonSelector;
    private DungeonTaskCollection dungeonTasks=new DungeonTaskCollection();
    private bool dungeonSelectorChanging;
    private static string DungeonTasksPath{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","zindanlar.xml");}}

    private void InitializeDungeonProfiles()
    {
        LoadDungeonProfiles();
        taskDungeonSelector=new ComboBox{Name="taskDungeonSelector",Location=new Point(338,24),Size=new Size(205,30),DropDownStyle=ComboBoxStyle.DropDownList};
        var add=TaskButton("Yeni zindan",553,22,115);add.Name="newDungeonButton";
        taskDungeonSelector.SelectedIndexChanged+=(s,e)=>SelectDungeonProfile(taskDungeonSelector.SelectedIndex,false);
        add.Click+=(s,e)=>CreateDungeonProfile(true);
        taskPage.Controls.AddRange(new Control[]{taskDungeonSelector,add});
        RefreshDungeonProfileSelectors();RefreshTaskStageList(0);
    }

    private void BuildDungeonProfileStrip(TabPage page)
    {
        page.Controls.Add(PlainLabel("Zindan",16,8,90));
        flowDungeonSelector=new ComboBox{Name="flowDungeonSelector",Left=16,Top=32,Width=220,DropDownStyle=ComboBoxStyle.DropDownList};
        var add=PlainButton("Yeni zindan",244,30,96);add.Name="flowNewDungeonButton";
        var del=PlainButton("Sil",348,30,98);del.Name="flowDeleteDungeonButton";
        flowDungeonSelector.SelectedIndexChanged+=(s,e)=>SelectDungeonProfile(flowDungeonSelector.SelectedIndex,false);
        add.Click+=(s,e)=>CreateDungeonProfile(false);del.Click+=(s,e)=>DeleteDungeonProfile();
        page.Controls.AddRange(new Control[]{flowDungeonSelector,add,del});RefreshDungeonProfileSelectors();
    }

    private void LoadDungeonProfiles()
    {
        try
        {
            if(File.Exists(DungeonTasksPath))dungeonTasks=DungeonTaskCollectionStore.Load(DungeonTasksPath);
            else
            {
                if(string.IsNullOrWhiteSpace(taskSequence.Name)||taskSequence.Name=="Yeni görev")taskSequence.Name="Kellam Zindanı";
                dungeonTasks=new DungeonTaskCollection{Dungeons=new List<TaskSequence>{taskSequence},SelectedIndex=0};
            }
        }
        catch{dungeonTasks=new DungeonTaskCollection();}
        dungeonTasks=DungeonTaskCollection.Normalize(dungeonTasks);taskSequence=dungeonTasks.Dungeons[dungeonTasks.SelectedIndex];
    }

    private void RefreshDungeonProfileSelectors()
    {
        dungeonSelectorChanging=true;
        try
        {
            foreach(var combo in new[]{taskDungeonSelector,flowDungeonSelector})
            {
                if(combo==null)continue;combo.BeginUpdate();combo.Items.Clear();
                foreach(var dungeon in dungeonTasks.Dungeons)combo.Items.Add(dungeon.Name);
                combo.SelectedIndex=dungeonTasks.SelectedIndex;combo.EndUpdate();
            }
        }
        finally{dungeonSelectorChanging=false;}
    }

    private void SelectDungeonProfile(int index,bool openEditor)
    {
        if(dungeonSelectorChanging||index<0||index>=dungeonTasks.Dungeons.Count)return;
        if(taskCancellation!=null){RefreshDungeonProfileSelectors();MessageBox.Show("Çalışan zindan durdurulmadan seçim değiştirilemez.");return;}
        SaveDungeonCatalog();dungeonTasks.SelectedIndex=index;taskSequence=dungeonTasks.Dungeons[index];taskStagesDone.Clear();
        RefreshDungeonProfileSelectors();RefreshTaskStageList(0);RefreshFlowList();RefreshDungeonNpcMarkerStatus();SaveDungeonCatalog();
        if(taskStatus!=null)taskStatus.Text=taskSequence.Name+" seçildi.";
        RecordActivity("Zindan",taskSequence.Name+" seçildi.");if(openEditor)OpenSelectedDungeonEditor();
    }

    private void CreateDungeonProfile(bool openEditor)
    {
        if(taskCancellation!=null){MessageBox.Show("Yeni zindan eklemek için çalışan görevi durdurun.");return;}
        int number=dungeonTasks.Dungeons.Count+1;var dungeon=new TaskSequence{Name=number+". Zindan"};
        for(int i=0;i<dungeon.Stages.Count;i++)dungeon.Stages[i].Name=(i+1)+". aşama";
        dungeonTasks.Dungeons.Add(dungeon);SelectDungeonProfile(dungeonTasks.Dungeons.Count-1,openEditor);
    }

    private void DeleteDungeonProfile()
    {
        if(taskCancellation!=null||dungeonTasks.Dungeons.Count<=1)return;int index=dungeonTasks.SelectedIndex;
        if(index==0){MessageBox.Show("Kellam Zindanı korunur. Diğer zindanları silebilirsiniz.");return;}
        dungeonTasks.Dungeons.RemoveAt(index);dungeonTasks.SelectedIndex=Math.Max(0,index-1);taskSequence=dungeonTasks.Dungeons[dungeonTasks.SelectedIndex];
        RefreshDungeonProfileSelectors();RefreshTaskStageList(0);RefreshFlowList();RefreshDungeonNpcMarkerStatus();SaveDungeonCatalog();
        if(taskStatus!=null)taskStatus.Text=taskSequence.Name+" seçildi.";
    }

    private void OpenSelectedDungeonEditor()
    {
        ShowControlCenter();
        if(controlCenterTabs!=null)for(int i=0;i<controlCenterTabs.TabPages.Count;i++)if(controlCenterTabs.TabPages[i].Text=="Zindan Akışı"){controlCenterTabs.SelectedIndex=i;break;}
        RefreshDungeonProfileSelectors();if(controlCenterForm!=null)controlCenterForm.BringToFront();
    }

    private void SaveDungeonCatalog()
    {
        try
        {
            dungeonTasks=DungeonTaskCollection.Normalize(dungeonTasks);
            dungeonTasks.Dungeons[dungeonTasks.SelectedIndex]=taskSequence;
            DungeonTaskCollectionStore.Save(DungeonTasksPath,dungeonTasks);
        }
        catch{}
    }
}
