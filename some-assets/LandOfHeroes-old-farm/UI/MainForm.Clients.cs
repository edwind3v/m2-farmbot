using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Several game clients and several copies of this app: each app picks one client from the list (shown with its
// character name) and holds a per-client lock, so two apps never drive the same game. A client locked by another
// app is listed but cannot be chosen.
public partial class MainForm
{
    private sealed class ClientEntry
    {
        public int Pid;public string Name;public bool Taken;
        public override string ToString(){return (string.IsNullOrEmpty(Name)?"Karakter okunuyor…":Name)+" · PID "+Pid+(Taken?" · başka uygulamada":"");}
    }
    private ComboBox clientPicker;private System.Windows.Forms.Timer clientTimer;
    private readonly Dictionary<int,string> clientNames=new Dictionary<int,string>();private readonly HashSet<int> clientNameQueries=new HashSet<int>();
    private Mutex clientLock;private int clientLockPid;private int chosenPid;private int boundPid;private bool awaitingReplacementClient;

    private static string ClientLockName(int pid){return "Local\\Onur.Client."+pid;}
    private static bool LockedElsewhere(int pid,int ownPid)
    {
        if(pid==ownPid)return false;Mutex existing;
        if(!Mutex.TryOpenExisting(ClientLockName(pid),out existing))return false;existing.Dispose();return true;
    }
    private bool ClaimClient(int pid)
    {
        if(clientLock!=null&&clientLockPid==pid)return true;
        ReleaseClient();bool created;
        var m=new Mutex(true,ClientLockName(pid),out created);
        if(!created){m.Dispose();return false;}
        clientLock=m;clientLockPid=pid;return true;
    }
    private void ReleaseClient(){if(clientLock==null)return;try{clientLock.ReleaseMutex();}catch{}clientLock.Dispose();clientLock=null;clientLockPid=0;}

    private void InitializeClientPicker()
    {
        clientPicker=new ComboBox{Name="clientPicker",DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Segoe UI",9.5f)};
        leftPanel.Controls.Add(clientPicker);
        clientPicker.SelectionChangeCommitted+=async(s,e)=>
        {
            var entry=clientPicker.SelectedItem as ClientEntry;if(entry==null)return;
            if(entry.Taken){MessageBox.Show("Bu oyun başka bir Onur penceresi tarafından yönetiliyor.");RefreshClientList();return;}
            chosenPid=entry.Pid;boundPid=entry.Pid;awaitingReplacementClient=false;
            // Switching client: disconnect from the current one and connect to the chosen one.
            if(connectionRequested&&memory.Field_0197?.Property_0268?.Id!=entry.Pid){clientConnected=false;automation?.Stop();StopSessionLease();await ConnectAutomaticallyAsync();}
        };
        clientTimer=new System.Windows.Forms.Timer(components){Interval=3000};clientTimer.Tick+=(s,e)=>RefreshClientList();clientTimer.Start();
        FormClosed+=(s,e)=>ReleaseClient();
        RefreshClientList();
    }

    private void RefreshClientList()
    {
        if(clientPicker==null||clientPicker.DroppedDown)return;
        var processes=Process.GetProcessesByName(ClientProcessName).OrderBy(p=>p.Id).ToList();
        var entries=processes.Select(p=>{string n;clientNames.TryGetValue(p.Id,out n);return new ClientEntry{Pid=p.Id,Name=n,Taken=LockedElsewhere(p.Id,clientLockPid)};}).ToList();
        foreach(var p in processes)if(!clientNames.ContainsKey(p.Id)&&!clientNameQueries.Contains(p.Id))QueryClientName(p);
        int keep=(clientPicker.SelectedItem as ClientEntry)?.Pid??(boundPid!=0?boundPid:(chosenPid!=0?chosenPid:clientLockPid));
        clientPicker.BeginUpdate();clientPicker.Items.Clear();foreach(var e in entries)clientPicker.Items.Add(e);
        if(boundPid!=0&&entries.All(e=>e.Pid!=boundPid)){clientPicker.Items.Add("Bağlı istemci kapalı · PID "+boundPid);clientPicker.SelectedIndex=clientPicker.Items.Count-1;}
        else if(awaitingReplacementClient){clientPicker.Items.Add("Bu uygulamanın yeni istemcisi bekleniyor");clientPicker.SelectedIndex=clientPicker.Items.Count-1;}
        else if(entries.Count==0){clientPicker.Items.Add("Oyun açık değil");clientPicker.SelectedIndex=0;}
        else{int i=entries.FindIndex(e=>e.Pid==keep);if(i<0)i=Math.Max(0,entries.FindIndex(e=>!e.Taken));clientPicker.SelectedIndex=i;}
        clientPicker.EndUpdate();
    }

    // The character name of a client, read once through its own bridge (no memory attach needed).
    private async void QueryClientName(Process process)
    {
        clientNameQueries.Add(process.Id);
        string output=Path.Combine(runtimeFolder,"name-"+process.Id+".txt");
        try
        {
            Directory.CreateDirectory(runtimeFolder);
            string name=await Task.Run(()=>
            {
                using(var bridge=new QueuedPythonBridge(process))
                {
                    bridge.Queue(LocalPythonScript("import player\ntry:\n n=player.GetName() or player.GetMainCharacterName()\nexcept Exception:\n n=''\nf.write(n)",output));
                    var sw=Stopwatch.StartNew();while(!bridge.Poll()){if(sw.ElapsedMilliseconds>5000)return null;Thread.Sleep(50);}
                }
                for(int i=0;i<20&&!File.Exists(output);i++)Thread.Sleep(50);
                return File.Exists(output)?File.ReadAllText(output).Trim():null;
            });
            if(!string.IsNullOrEmpty(name))clientNames[process.Id]=name;
        }
        catch{}
        finally{clientNameQueries.Remove(process.Id);try{File.Delete(output);}catch{}}
        if(!clientNames.ContainsKey(process.Id))clientNames[process.Id]="";
        if(!IsDisposed)RefreshClientList();
    }

    // Once selected, this app stays on that exact PID and never falls through to another account.
    private void UpdateClientTitle(int pid){ /* Rastgele baslik korunur; istemci adi/PID sol menudeki secicide gosterilir. */ }
    private static int ResolveStrictBoundPid(IEnumerable<int> alivePids,int pid){return pid!=0&&alivePids.Contains(pid)?pid:0;}
    private Process PickClientProcess(out string reason)
    {
        reason=null;var clients=Process.GetProcessesByName(ClientProcessName).OrderBy(p=>p.Id).ToArray();
        if(boundPid!=0)
        {
            int exactPid=ResolveStrictBoundPid(clients.Select(c=>c.Id),boundPid);var exact=clients.FirstOrDefault(p=>p.Id==exactPid);
            if(exact==null){reason="Seçili istemci kapalı · başka hesaba geçilmeyecek.";return null;}
            if(LockedElsewhere(exact.Id,clientLockPid)){reason="Seçili oyun başka bir Onur penceresinde; listeden başka bir oyun seçin.";return null;}
            return exact;
        }
        if(awaitingReplacementClient){reason="Bu uygulamanın yeniden açtığı istemci bekleniyor · diğer hesaplara dokunulmayacak.";return null;}
        if(clients.Length==0){reason="Oyun bekleniyor; açık olunca bağlanılacak.";return null;}
        var free=clients.Where(c=>!LockedElsewhere(c.Id,clientLockPid)).ToList();
        if(free.Count==0){reason="Açık oyunların hepsi başka Onur pencerelerinde.";return null;}
        if(free.Count>1){reason="Birden fazla oyun açık: yönetilecek oyunun penceresine geçip Insert'e basın.";return null;}
        return free[0];
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]private static extern short GetAsyncKeyState(int key);
    [System.Runtime.InteropServices.DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
    [System.Runtime.InteropServices.DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
    private System.Windows.Forms.Timer insertTimer;private bool insertWasDown;
    private int FreeClientCount(){return Process.GetProcessesByName(ClientProcessName).Count(p=>!LockedElsewhere(p.Id,clientLockPid));}

    // "Oyuna bağlan" with several clients open and none chosen: wait until Insert is pressed inside a game window,
    // then drive exactly that client. Only a waiting Onur window listens, so each window claims the game it was given.
    private bool StartInsertPick()
    {
        bool chosenAlive=boundPid!=0&&Process.GetProcessesByName(ClientProcessName).Any(p=>p.Id==boundPid&&!LockedElsewhere(p.Id,clientLockPid));
        if(chosenAlive||FreeClientCount()<=1)return false;
        connectionStatus.Text="Yönetilecek oyunun penceresine geç ve Insert'e bas.";connectBtn.Text="Insert bekleniyor… (iptal)";
        insertWasDown=(GetAsyncKeyState(0x2D)&0x8000)!=0;
        if(insertTimer==null){insertTimer=new System.Windows.Forms.Timer(components){Interval=50};insertTimer.Tick+=async(s,e)=>await PollInsertPick();}
        insertTimer.Start();return true;
    }
    private void CancelInsertPick(){insertTimer?.Stop();connectBtn.Text="Oyuna bağlan";connectionStatus.Text="Seçim iptal edildi.";}
    private bool InsertPickActive{get{return insertTimer!=null&&insertTimer.Enabled;}}
    private async Task PollInsertPick()
    {
        bool down=(GetAsyncKeyState(0x2D)&0x8000)!=0;bool pressed=down&&!insertWasDown;insertWasDown=down;
        if(!pressed)return;
        uint pid;GetWindowThreadProcessId(GetForegroundWindow(),out pid);
        var game=Process.GetProcessesByName(ClientProcessName).FirstOrDefault(p=>p.Id==(int)pid);
        if(game==null){connectionStatus.Text="Insert bir oyun penceresinde basılmadı; oyun penceresine geçip tekrar bas.";return;}
        if(LockedElsewhere(game.Id,clientLockPid)){connectionStatus.Text="Bu oyun başka bir Onur penceresinde; başka bir oyunda Insert'e bas.";return;}
        insertTimer.Stop();chosenPid=game.Id;boundPid=game.Id;awaitingReplacementClient=false;connectionRequested=true;RefreshClientList();
        await ConnectAutomaticallyAsync();
    }
}
