using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private const int WmHotkey=0x0312,MovementHotkeyId=0x4F01,DungeonHotkeyId=0x4F02;
    private const uint ModAlt=0x0001,ModControl=0x0002,ModShift=0x0004,ModNoRepeat=0x4000;
    [DllImport("user32.dll",SetLastError=true)]private static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
    [DllImport("user32.dll",SetLastError=true)]private static extern bool UnregisterHotKey(IntPtr window,int id);

    private ComboBox movementHotkeyCombo,dungeonHotkeyCombo;
    private Label movementHotkeyStatus,dungeonHotkeyStatus;
    private bool hotkeyUiChanging,movementHotkeyRegistered,dungeonHotkeyRegistered;
    private static string GlobalHotkeyPath { get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","global-hotkeys.txt");} }

    private void InitializeGlobalHotkeys()
    {
        AddCaption(tabMain,"GLOBAL BASLAT / DURDUR TUSU",404,616,230,20,8.5f,StudioMuted);
        movementHotkeyCombo=CreateHotkeyCombo("movementHotkeyCombo",404,640);
        movementHotkeyStatus=CreateHotkeyStatus("movementHotkeyStatus",404,668);
        movementHotkeyStatus.Width=230;
        tabMain.Controls.AddRange(new Control[]{movementHotkeyCombo,movementHotkeyStatus});

        AddCaption(taskPage,"GLOBAL ZINDAN BASLAT / DURDUR TUSU",28,606,310,20,8.5f,StudioMuted);
        dungeonHotkeyCombo=CreateHotkeyCombo("dungeonHotkeyCombo",28,632);
        dungeonHotkeyStatus=CreateHotkeyStatus("dungeonHotkeyStatus",228,634);
        taskPage.Controls.AddRange(new Control[]{dungeonHotkeyCombo,dungeonHotkeyStatus});
        taskPage.AutoScrollMinSize=new Size(0,740);

        movementHotkeyCombo.SelectedIndexChanged+=(s,e)=>ApplyHotkeySelection(true);
        dungeonHotkeyCombo.SelectedIndexChanged+=(s,e)=>ApplyHotkeySelection(false);
        LoadGlobalHotkeys();
        FormClosed+=(s,e)=>ReleaseGlobalHotkeys();
    }

    private ComboBox CreateHotkeyCombo(string name,int x,int y)
    {
        var box=new ComboBox{Name=name,Left=x,Top=y,Width=190,Height=30,DropDownStyle=ComboBoxStyle.DropDownList,BackColor=StudioSurface,ForeColor=StudioText};
        foreach(string option in GlobalHotkeyOptions())box.Items.Add(option);
        box.SelectedIndex=0;return box;
    }

    private Label CreateHotkeyStatus(string name,int x,int y)
    {
        return new Label{Name=name,Text="Kapali · uygulama arkadayken de calisir",Left=x,Top=y,Width=410,Height=25,ForeColor=StudioMuted,AutoEllipsis=true};
    }

    private static IEnumerable<string> GlobalHotkeyOptions()
    {
        yield return "Kapali";
        for(int i=1;i<=12;i++)yield return "F"+i;
        for(int i=1;i<=12;i++)yield return "Ctrl+F"+i;
        for(int i=1;i<=12;i++)yield return "Alt+F"+i;
        for(int i=1;i<=12;i++)yield return "Shift+F"+i;
        for(int i=0;i<=9;i++)yield return "NumPad"+i;
    }

    private void ApplyHotkeySelection(bool movement)
    {
        if(hotkeyUiChanging||movementHotkeyCombo==null||dungeonHotkeyCombo==null)return;
        var box=movement?movementHotkeyCombo:dungeonHotkeyCombo;var other=movement?dungeonHotkeyCombo:movementHotkeyCombo;
        var status=movement?movementHotkeyStatus:dungeonHotkeyStatus;int id=movement?MovementHotkeyId:DungeonHotkeyId;
        UnregisterHotKey(Handle,id);if(movement)movementHotkeyRegistered=false;else dungeonHotkeyRegistered=false;
        string selected=box.SelectedItem as string??"Kapali";
        if(selected=="Kapali"){status.Text="Kapali · uygulama arkadayken de calisir";SaveGlobalHotkeys();return;}
        if(emergencyHotkeyRegistered&&selected==emergencyHotkeyCombo.Text){RejectHotkeySelection(box,status,"Bu tuş acil durdurma için ayrıldı.");return;}
        if(string.Equals(selected,other.SelectedItem as string,StringComparison.OrdinalIgnoreCase))
        {RejectHotkeySelection(box,status,"Bu tus diger kisayolda kullaniliyor.");return;}
        uint modifiers,key;
        if(!TryParseGlobalHotkey(selected,out modifiers,out key)||!RegisterHotKey(Handle,id,modifiers|ModNoRepeat,key))
        {RejectHotkeySelection(box,status,"Bu tus Windows veya baska bir uygulama tarafindan kullaniliyor.");return;}
        if(movement)movementHotkeyRegistered=true;else dungeonHotkeyRegistered=true;
        status.Text=selected+" aktif · uygulama arkadayken de calisir";SaveGlobalHotkeys();
    }

    private void RejectHotkeySelection(ComboBox box,Label status,string message)
    {
        hotkeyUiChanging=true;box.SelectedItem="Kapali";hotkeyUiChanging=false;status.Text=message;SaveGlobalHotkeys();
    }

    internal static bool TryParseGlobalHotkey(string text,out uint modifiers,out uint key)
    {
        modifiers=0;key=0;if(string.IsNullOrWhiteSpace(text)||text=="Kapali")return false;
        string token=text.Trim();
        if(token.StartsWith("Ctrl+",StringComparison.OrdinalIgnoreCase)){modifiers=ModControl;token=token.Substring(5);}
        else if(token.StartsWith("Alt+",StringComparison.OrdinalIgnoreCase)){modifiers=ModAlt;token=token.Substring(4);}
        else if(token.StartsWith("Shift+",StringComparison.OrdinalIgnoreCase)){modifiers=ModShift;token=token.Substring(6);}
        if(token.StartsWith("F",StringComparison.OrdinalIgnoreCase))
        {int number;if(int.TryParse(token.Substring(1),out number)&&number>=1&&number<=12){key=(uint)((int)Keys.F1+number-1);return true;}}
        if(token.StartsWith("NumPad",StringComparison.OrdinalIgnoreCase))
        {int number;if(int.TryParse(token.Substring(6),out number)&&number>=0&&number<=9){key=(uint)((int)Keys.NumPad0+number);return true;}}
        return false;
    }

    private void LoadGlobalHotkeys()
    {
        string movement="Kapali",dungeon="Kapali";
        try
        {
            if(File.Exists(GlobalHotkeyPath))foreach(string line in File.ReadAllLines(GlobalHotkeyPath))
            {if(line.StartsWith("movement=",StringComparison.OrdinalIgnoreCase))movement=line.Substring(9).Trim();else if(line.StartsWith("dungeon=",StringComparison.OrdinalIgnoreCase))dungeon=line.Substring(8).Trim();}
        }
        catch{}
        hotkeyUiChanging=true;
        movementHotkeyCombo.SelectedItem=movementHotkeyCombo.Items.Contains(movement)?movement:"Kapali";
        dungeonHotkeyCombo.SelectedItem=dungeonHotkeyCombo.Items.Contains(dungeon)?dungeon:"Kapali";
        hotkeyUiChanging=false;
        ApplyHotkeySelection(true);ApplyHotkeySelection(false);
    }

    private void SaveGlobalHotkeys()
    {
        if(hotkeyUiChanging||movementHotkeyCombo==null||dungeonHotkeyCombo==null)return;
        try{Directory.CreateDirectory(Path.GetDirectoryName(GlobalHotkeyPath));File.WriteAllLines(GlobalHotkeyPath,new[]{"movement="+(movementHotkeyCombo.SelectedItem??"Kapali"),"dungeon="+(dungeonHotkeyCombo.SelectedItem??"Kapali")});}catch{}
    }

    private void ReleaseGlobalHotkeys()
    {
        try{if(IsHandleCreated){UnregisterHotKey(Handle,MovementHotkeyId);UnregisterHotKey(Handle,DungeonHotkeyId);UnregisterHotKey(Handle,EmergencyHotkeyId);}}catch{}
        movementHotkeyRegistered=false;dungeonHotkeyRegistered=false;
    }

    protected override void WndProc(ref Message message)
    {
        if(message.Msg==WmHotkey)
        {
            int id=message.WParam.ToInt32();
            if(id==EmergencyHotkeyId&&emergencyHotkeyRegistered){var ignored=EmergencyStopAsync("Global acil durdurma tuşu.");return;}
            if(id==MovementHotkeyId&&movementHotkeyRegistered){BeginInvoke((Action)ToggleMovementFromHotkey);return;}
            if(id==DungeonHotkeyId&&dungeonHotkeyRegistered){BeginInvoke((Action)ToggleDungeonFromHotkey);return;}
        }
        base.WndProc(ref message);
    }

    private void ToggleMovementFromHotkey()
    {
        if(IsDisposed||Disposing)return;ToggleNormalBot();
        if(movementHotkeyStatus!=null)movementHotkeyStatus.Text=(movementHotkeyCombo.SelectedItem??"")+" · "+(normalBotRequested||Field_02DF?"bot calisiyor":"bot durdu");
    }

    private async void ToggleDungeonFromHotkey()
    {
        if(IsDisposed||Disposing)return;
        if(taskCancellation!=null||taskDungeonRunning){resumeDungeonStage=0;StopTaskSequence("Kisayolla durduruldu.");}
        else await RunTaskSequenceAsync(true);
        if(dungeonHotkeyStatus!=null)dungeonHotkeyStatus.Text=(dungeonHotkeyCombo.SelectedItem??"")+" · "+(taskCancellation!=null||taskDungeonRunning?"zindan calisiyor":"zindan durdu");
    }
}
