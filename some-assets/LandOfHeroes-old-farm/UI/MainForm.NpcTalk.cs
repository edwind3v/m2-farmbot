using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

// Dungeon NPCs are identified by live name/type and opened through the client interaction API.
// Legacy screen helpers below are not used by dungeon NPC discovery or opening.
// Quest buttons are invoked on the matching live dialog, after its text/buttons are ready.
// Python locals do not keep closed dialogs alive in the game's global namespace.
public partial class MainForm
{
    [DllImport("user32.dll")]private static extern bool GetCursorPos(out NativePoint p);
    [DllImport("user32.dll")]private static extern bool ScreenToClient(IntPtr h,ref NativePoint p);
    private QueuedPythonBridge npcBridge;private int npcCall;

    private sealed class QuestAnswer{public int Index;public bool Icon;public string Caption;}
    private sealed class QuestDialogInfo
    {
        public string Key;public string Snapshot;public bool NextShown;public int NextType;public List<QuestAnswer> Answers=new List<QuestAnswer>();
        // The answers / Next button appear only when the text has been written out; until then nothing may be pressed.
        public bool Ready{get{return Answers.Count>0||NextShown;}}
        public int Find(params string[] prefixes){var a=Answers.FirstOrDefault(x=>prefixes.Any(p=>Norm(x.Caption).StartsWith(p)));return a==null?-1:a.Index;}
        public int FindContaining(params string[] parts){var a=Answers.FirstOrDefault(x=>parts.Any(p=>Norm(x.Caption).Contains(p)));return a==null?-1:a.Index;}
        public string CaptionOf(int index){var a=Answers.FirstOrDefault(x=>x.Index==index);return a==null?"":a.Caption.Trim();}
        public override string ToString(){return Answers.Count==0?(NextShown?"Devam":"metin yazılıyor"):string.Join(" / ",Answers.Select(a=>(a.Icon?"[simge] ":"")+a.Caption.Trim()));}
        private static string Norm(string s){return (s??"").Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));}
    }

    private const string FindMainStream="import sys\nfr=sys._getframe();ms=None\nfor _i in range(64):\n if fr is None:break\n _c=fr.f_locals.get('mainStream')\n if _c is not None and hasattr(_c,'curPhaseWindow'):ms=_c;break\n fr=fr.f_back\n";
    // Hides closed-but-still-shown quest dialogs (board already destroyed, no longer in the interface) and drops what
    // kept them alive: the last unhandled error's traceback and globals left by older versions of this app.
    private const string QuestSweep=
        "import gc,uiQuest\n"+
        "sys.last_traceback=None;sys.last_value=None;sys.last_type=None\n"+
        "_live=set();_live_known=False\n"+
        "try:\n _live=set(id(x) for x in ms.curPhaseWindow.interface.wndQuestWindow.values());_live_known=True\nexcept Exception:pass\n"+
        "_g=globals()\n"+
        "for _n in ('best','d'):\n try:\n  _v=_g.get(_n)\n  if isinstance(_v,uiQuest.QuestDialog) or (isinstance(_v,tuple) and any(isinstance(_x,uiQuest.QuestDialog) for _x in _v)):_g.pop(_n,None)\n except Exception:pass\n"+
        "if _live_known:\n for _o in gc.get_objects():\n  try:\n   if isinstance(_o,uiQuest.QuestDialog) and id(_o) not in _live and getattr(_o,'board',0) is None and _o.IsShow():_o.Hide()\n  except Exception:pass\n";
    // The pack's Destroy clears the board but does not Hide the root window. A retained
    // dialog can therefore still intercept input until GC. Hide only on its normal close;
    // retain the client's Unlock / RemoveQuestDialog handlers, with no strong-reference cycle.
    private const string NpcDialogCloseCleanup=
        "if best is not None:\n try:\n  _dialog=best[1]\n  if not getattr(_dialog,'_onur_close_cleanup_installed',False):\n   from _weakref import ref\n   def _onur_hide_closed_dialog(_ref=ref(_dialog)):\n    _closed=_ref()\n    if _closed is not None:\n     try:_closed.Hide()\n     except Exception:pass\n   _dialog.AddOnCloseEvent(_onur_hide_closed_dialog)\n   _dialog._onur_close_cleanup_installed=True\n except Exception:pass\n";
    // Newest visible quest dialog as best=(key,dialog) or None.
    private const string DialogState=FindMainStream+"best=None\ntry:\n for k,d in ms.curPhaseWindow.interface.wndQuestWindow.items():\n  if d.IsShow() and (best is None or k>best[0]):best=(k,d)\nexcept Exception:best=None\n";
    // "key<TAB>nextShown<TAB>nextType" and one "index<TAB>icon<TAB>caption" line per visible answer, or "none".
    // Includes object identity and contents: quest keys can be reused by the next page/run.
    private const string DialogSnapshot=
        "def _dialog_snapshot(best):\n"+
        " if best is None:return 'none'\n"+
        " d=best[1];bn=getattr(d,'btnNext',None);nx=1 if bn is not None and bn.IsShow() else 0\n"+
        " key=str(best[0])+':'+str(id(d))+':'+str(getattr(d,'descIndex',-1))\n"+
        " rows=[key+'\\t'+str(nx)+'\\t'+str(getattr(d,'nextButtonType',-1) if nx else -1)]\n"+
        " for i,b in enumerate(getattr(d,'btnAnswer',None) or []):\n"+
        "  if not b.IsShow():continue\n"+
        "  t=''\n"+
        "  for get in (lambda:b.ButtonText.GetText(),lambda:b.GetText()):\n"+
        "   try:\n    t=str(get());break\n   except Exception:pass\n"+
        "  rows.append(str(i)+'\\t'+('1' if getattr(b,'ButtonIcon',None) is not None else '0')+'\\t'+t.replace('\\t',' ').replace('\\n',' '))\n"+
        " return '\\n'.join(rows).strip()\n";
    private const string DialogReport=DialogSnapshot+"f.write(_dialog_snapshot(best))\n";

    // Runs a script on the game's main thread. The body writes its result with f.write; all its names stay local.
    private async Task<string> GamePythonAsync(string body,CancellationToken token,bool emergencyCleanup=false)
    {
        if(emergencyLatched&&!emergencyCleanup)throw new OperationCanceledException("Acil durdurma kilidi etkin.");
        var process=memory.Field_0197?.Property_0268;if(process==null||process.HasExited)throw new InvalidOperationException("Oyun bulunamadı.");
        if(npcBridge!=null&&npcBridge.ProcessId!=process.Id){npcBridge.Dispose();npcBridge=null;}
        if(npcBridge==null)npcBridge=new QueuedPythonBridge(process);
        // A background sweep may still be running on the same bridge: let it finish instead of failing the task.
        for(var busy=Stopwatch.StartNew();npcBridge.Pending&&!npcBridge.Poll();await Task.Delay(30,token))
            if(busy.ElapsedMilliseconds>15000){npcBridge.Dispose();npcBridge=new QueuedPythonBridge(process);break;}
        Directory.CreateDirectory(runtimeFolder);var output=Path.Combine(runtimeFolder,"npc-"+(++npcCall%4)+".txt");try{File.Delete(output);}catch{}
        token.ThrowIfCancellationRequested();
        if(emergencyLatched&&!emergencyCleanup)throw new OperationCanceledException("Acil durdurma kilidi etkin.");
        npcBridge.Queue(LocalPythonScript(emergencyCleanup?body:GuardGameScript(body),output));
        var watch=Stopwatch.StartNew();
        try{while(!npcBridge.Poll()){token.ThrowIfCancellationRequested();if(watch.ElapsedMilliseconds>15000)throw new TimeoutException("Oyun komuta yanıt vermedi.");await Task.Delay(30,token);}}
        catch(InvalidOperationException){npcBridge.Dispose();npcBridge=null;throw;}
        for(int i=0;i<40&&!File.Exists(output);i++)await Task.Delay(25,token);
        if(emergencyLatched&&!emergencyCleanup)throw new OperationCanceledException("Acil durdurma kilidi etkin.");
        try{string result=File.Exists(output)?File.ReadAllText(output).Trim():"";if(result=="EMERGENCY_STOP")throw new OperationCanceledException("Acil durdurma kilidi etkin.");return result;}catch(IOException){return "";}
    }
    private static string PythonFileLiteral(string value)
    {
        var s=new System.Text.StringBuilder("'");
        foreach(char ch in value??"")
        {
            if(ch=='\\')s.Append("\\\\");
            else if(ch=='\'')s.Append("\\'");
            else if(ch>=32&&ch<=126)s.Append(ch);
            else s.Append("\\u"+((int)ch).ToString("x4"));
        }
        return s.Append("'").ToString();
    }
    // Wraps a script body in a function that writes to `output`, so nothing is left in the game's globals.
    public static string LocalPythonScript(string body,string output)
    {
        return "def _onur_call(f):\n"+string.Join("\n",body.Split('\n').Select(l=>" "+l))+"\n pass\n"+
            "try:\n _onur_f=globals().get('old_open',open)("+PythonFileLiteral(output)+",'w',encoding='utf-8')\n"+
            "except TypeError:\n _onur_f=open("+PythonFileLiteral(output)+",'w')\n"+
            "try:\n _onur_call(_onur_f)\nexcept Exception as _onur_e:\n _onur_f.write('ERR '+type(_onur_e).__name__)\n"+
            "finally:\n _onur_f.close()\n del _onur_call,_onur_f\n";
    }

    private async Task<QuestDialogInfo> ReadQuestDialogAsync(CancellationToken token)
    {
        var r=await GamePythonAsync(DialogState+NpcDialogCloseCleanup+DialogReport,token);
        return ParseQuestDialogReport(r);
    }
    private static QuestDialogInfo ParseQuestDialogReport(string r)
    {
        // Python text files use CRLF on Windows, while its in-memory snapshot uses LF.
        // Normalize BEFORE saving the snapshot that will guard the next button press.
        r=(r??"").Replace("\r\n","\n").Replace("\r","\n").Trim();
        if(r.Length==0||r=="none"||r.StartsWith("ERR"))return null;
        var lines=r.Replace("\r","").Split('\n');var head=lines[0].Split('\t');int type;
        if(head.Length<3)return null;
        var info=new QuestDialogInfo{Key=head[0],Snapshot=r,NextShown=head[1]=="1",NextType=int.TryParse(head[2],out type)?type:-1};
        foreach(var line in lines.Skip(1))
        {
            var p=line.Split(new[]{'\t'},3);int index;
            if(p.Length==3&&int.TryParse(p[0],out index))info.Answers.Add(new QuestAnswer{Index=index,Icon=p[1]=="1",Caption=p[2]});
        }
        return info;
    }
    // Clears invisible leftover dialogs once (on connect), so the player's clicks reach the game again.
    private async Task SweepQuestDialogsAsync(CancellationToken token)
    {
        try{await GamePythonAsync(FindMainStream+QuestSweep+"f.write('ok')",token);}catch(Exception ex) when(!(ex is OperationCanceledException)){}
    }

    // Presses an answer (index >= 0) or the Next/OK button (index -1) of the dialog that is still `key`.
    private async Task<bool> PressQuestDialogAsync(QuestDialogInfo dialog,int index,CancellationToken token)
    {
        var done=await GamePythonAsync(BuildQuestPressScript(dialog,index),token);
        if(done.StartsWith("ERR",StringComparison.Ordinal))throw new InvalidOperationException("NPC cevap düğmesi uygulanamadı: "+done);
        return done=="ok";
    }
    private static string BuildQuestPressScript(QuestDialogInfo dialog,int index)
    {
        if(dialog==null||index< -1)throw new ArgumentException("NPC cevap seçimi geçersiz.");
        string action=index>=0
            ?" bs=d.btnAnswer or []\n if "+index+"<len(bs) and bs["+index+"].IsShow():\n  d.ClickAnswerEvent("+index+");f.write('ok')\n else:f.write('changed')\n"
            :" bn=getattr(d,'btnNext',None)\n if bn is not None and bn.IsShow():\n  bn.CallEvent();f.write('ok')\n else:f.write('changed')\n";
        return DialogState+DialogSnapshot+"if best is not None and _dialog_snapshot(best)=="+PythonLiteral(dialog.Snapshot)+":\n d=best[1]\n"+action+"else:f.write('changed')";
    }

    private const uint WmMouseMove=0x0200;
    private IntPtr gameViewportHandle;
    private int gameViewportClientWidth,gameViewportClientHeight;
    private Size gameViewportSize=Size.Empty;

    private async Task<Size> GetGameViewportSizeAsync(CancellationToken token)
    {
        var handle=TaskGameWindow();NativeRect rect;
        if(handle==IntPtr.Zero||!GetClientRect(handle,out rect))return Size.Empty;
        int clientWidth=rect.Right-rect.Left,clientHeight=rect.Bottom-rect.Top;
        if(handle==gameViewportHandle&&clientWidth==gameViewportClientWidth&&clientHeight==gameViewportClientHeight&&gameViewportSize.Width>0&&gameViewportSize.Height>0)return gameViewportSize;
        int width=clientWidth,height=clientHeight;
        try
        {
            var value=await GamePythonAsync("import wndMgr\nf.write(str(int(wndMgr.GetScreenWidth()))+'|'+str(int(wndMgr.GetScreenHeight())))",token);
            var parts=(value??"").Split('|');int parsedWidth,parsedHeight;
            if(parts.Length==2&&int.TryParse(parts[0],out parsedWidth)&&int.TryParse(parts[1],out parsedHeight)&&parsedWidth>0&&parsedHeight>0){width=parsedWidth;height=parsedHeight;}
        }
        catch(Exception ex)when(!(ex is OperationCanceledException)){}
        gameViewportHandle=handle;gameViewportClientWidth=clientWidth;gameViewportClientHeight=clientHeight;gameViewportSize=new Size(width,height);
        return gameViewportSize;
    }

    private void PostGameCursor(IntPtr handle,int x,int y){PostMessage(handle,WmMouseMove,UIntPtr.Zero,new IntPtr((y<<16)|(x&0xffff)));}
    // Puts the game's cursor back under the real mouse (only when the real mouse is over the game window).
    private void RestoreGameCursor(IntPtr handle)
    {
        NativePoint p;NativeRect r;
        if(!GetCursorPos(out p)||!ScreenToClient(handle,ref p)||!GetClientRect(handle,out r))return;
        if(p.X>=0&&p.Y>=0&&p.X<r.Right&&p.Y<r.Bottom)PostGameCursor(handle,p.X,p.Y);
    }
    // Converts the game's render-space point to this window's current client size before dispatching the click.
    private async Task<string> ClickGameCursorAsync(int gameX,int gameY,int vid,CancellationToken token)
    {
        var handle=TaskGameWindow();NativeRect rect;
        if(handle==IntPtr.Zero||!GetClientRect(handle,out rect))throw new InvalidOperationException("Oyun penceresi bulunamadı.");
        var viewport=await GetGameViewportSizeAsync(token);
        if(viewport.Width<=0||viewport.Height<=0)viewport=new Size(rect.Right,rect.Bottom);
        if(gameX<0||gameY<0||gameX>=viewport.Width||gameY>=viewport.Height)return "miss ekran dışında";
        var client=GameCoordinateMapper.ToClient(gameX,gameY,viewport.Width,viewport.Height,rect.Right-rect.Left,rect.Bottom-rect.Top);
        try
        {
            PostGameCursor(handle,client.X,client.Y);await Task.Delay(120,token);
            if(vid>0)
            {
                var picked=await GamePythonAsync("import chr,grp\ngrp.SetCursorPosition("+gameX+","+gameY+")\nf.write(str(chr.PickAll()))",token);
                if(picked!=vid.ToString())return "miss "+picked;
                var point=new IntPtr((client.Y<<16)|(client.X&0xffff));
                try
                {
                    if(!PostMessage(handle,0x0201,new UIntPtr(1),point))return "miss tıklama gönderilemedi";
                    await Task.Delay(120,token);
                }
                finally{PostMessage(handle,0x0202,UIntPtr.Zero,point);}
                await Task.Delay(120,token);
                return "clicked";
            }
            return await GamePythonAsync("import grp,player\ngrp.SetCursorPosition("+gameX+","+gameY+")\nplayer.SetMouseState(player.MBT_LEFT,player.MBS_PRESS)\nplayer.SetMouseState(player.MBT_LEFT,player.MBS_CLICK)\nf.write('clicked')",token);
        }
        finally{RestoreGameCursor(handle);}
    }

    // A valid NPC without a selectable screen point is still a target to approach.
    private static bool TryParseNpcPoint(string report,out int vid,out int x,out int y)
    {
        vid=0;x=y=-1;var parts=(report??"").Split('|');
        if(parts.Length<2||!int.TryParse(parts[0],out vid)||vid<=0)return false;
        if(parts.Length==2&&parts[1]=="none")return true;
        return parts.Length==3&&int.TryParse(parts[1],out x)&&int.TryParse(parts[2],out y);
    }

    [DllImport("user32.dll")]private static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")]private static extern bool SetCursorPos(int x,int y);
    [DllImport("user32.dll")]private static extern bool IsIconic(IntPtr h);
    [DllImport("user32.dll")]private static extern bool ShowWindow(IntPtr h,int command);
    [DllImport("user32.dll")]private static extern IntPtr WindowFromPoint(NativePoint p);
    [DllImport("user32.dll")]private static extern IntPtr GetAncestor(IntPtr h,uint flags);
    [DllImport("user32.dll")]private static extern bool BlockInput(bool block);
    [DllImport("user32.dll")]private static extern bool BringWindowToTop(IntPtr h);

    private async Task<string> ClickFocusedNpcAsync(int x,int y,int vid,CancellationToken token)
    {
        var handle=TaskGameWindow();var previous=GetForegroundWindow();NativePoint original;NativeRect rect;
        if(handle==IntPtr.Zero||!GetClientRect(handle,out rect))return "miss pencere yok";
        var viewport=await GetGameViewportSizeAsync(token);
        if(viewport.Width<=0||viewport.Height<=0)viewport=new Size(rect.Right,rect.Bottom);
        var client=GameCoordinateMapper.ToClient(x,y,viewport.Width,viewport.Height,rect.Right-rect.Left,rect.Bottom-rect.Top);
        var target=new NativePoint{X=client.X,Y=client.Y};
        if(!GetCursorPos(out original))return await ClickGameCursorAsync(x,y,vid,token);
        if(IsIconic(handle))ShowWindow(handle,9);
        if(!ClientToScreen(handle,ref target))return "miss ekran konumu yok";
        bool moved=false,blocked=false;
        try
        {
            BringWindowToTop(handle);SetForegroundWindow(handle);blocked=BlockInput(true);await Task.Delay(60,token);
            if(GetForegroundWindow()!=handle){BringWindowToTop(handle);SetForegroundWindow(handle);await Task.Delay(60,token);}
            if(GetForegroundWindow()!=handle)return "miss oyun öne alınamadı";
            if(!SetCursorPos(target.X,target.Y))return "miss imleç taşınamadı";
            moved=true;await Task.Delay(50,token);
            if(GetAncestor(WindowFromPoint(target),2)!=handle)return "miss oyun örtülü";
            return await ClickGameCursorAsync(x,y,vid,token);
        }
        finally
        {
            if(moved)SetCursorPos(original.X,original.Y);
            if(previous!=IntPtr.Zero&&previous!=handle&&GetForegroundWindow()==handle)SetForegroundWindow(previous);
            if(blocked)BlockInput(false);
        }
    }

    // Opens the NPC's dialog with a click that is verified on the game side. Returns when the dialog is shown.
    // NPC identification must not depend on combat filters, zero terrain height or missing radar names.
    // The client interpreter validates these candidate VIDs by live instance type and configured name.
    private List<int> ReadNpcCandidateVids()
    {
        var ids=new HashSet<int>();
        try
        {
            if(IsX86ClientMode)
            {
                foreach(var entity in ReadRadarEntities())
                    if(entity.Property_03CA==1&&entity.Property_03C9>0)ids.Add(entity.Property_03C9);
            }
            else
            {
                long address=Convert.ToInt64(AddressCatalog.Property_0386.Replace(",",""),16);
                long begin=memory.ReadInt64("0x"+address.ToString("X")),end=memory.ReadInt64("0x"+(address+8).ToString("X"));
                int typeOffset=Convert.ToInt32(AddressCatalog.Field_0378.Replace(",",""),16),vidOffset=Convert.ToInt32(AddressCatalog.Field_0377.Replace(",",""),16);
                if(begin>0&&end>=begin&&(end-begin)%8==0&&(end-begin)/8<=16384)
                    for(long entry=begin;entry<end;entry+=8)
                    {
                        long actor=memory.ReadInt64("0x"+entry.ToString("X"));if(actor<=0)continue;
                        if(memory.ReadInt32("0x"+(actor+typeOffset).ToString("X"))!=1)continue;
                        int vid=memory.ReadInt32("0x"+(actor+vidOffset).ToString("X"));if(vid>0)ids.Add(vid);
                    }
            }
        }
        catch{}
        return ids.OrderBy(v=>v).ToList();
    }
    private List<int> ReadVisualCandidateVids()
    {
        var ids=new HashSet<int>();
        try
        {
            if(IsX86ClientMode)
            {
                foreach(var entity in ReadRadarEntities())if(entity.Property_03C9>0)ids.Add(entity.Property_03C9);
            }
            else
            {
                long address=Convert.ToInt64(AddressCatalog.Property_0386.Replace(",",""),16);
                long begin=memory.ReadInt64("0x"+address.ToString("X")),end=memory.ReadInt64("0x"+(address+8).ToString("X"));
                int vidOffset=Convert.ToInt32(AddressCatalog.Field_0377.Replace(",",""),16);
                if(begin>0&&end>=begin&&(end-begin)%8==0&&(end-begin)/8<=16384)
                    for(long entry=begin;entry<end;entry+=8)
                    {
                        long actor=memory.ReadInt64("0x"+entry.ToString("X"));if(actor<=0)continue;
                        int vid=memory.ReadInt32("0x"+(actor+vidOffset).ToString("X"));if(vid>0)ids.Add(vid);
                    }
            }
        }
        catch{}
        return ids.OrderBy(v=>v).ToList();
    }
    private static string NpcIdentityScript()
    {
        string source;
        using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.SessionAgent.py"))
        using(var reader=new StreamReader(stream))source=reader.ReadToEnd();
        int start=source.IndexOf("# BEGIN NPC IDENTITY HELPER",StringComparison.Ordinal),end=source.IndexOf("# END NPC IDENTITY HELPER",StringComparison.Ordinal);
        if(start<0||end<=start)throw new InvalidOperationException("NPC doğrulama kodu bulunamadı.");
        return source.Substring(start,end-start)+"\n";
    }
    private static string BuildNpcPointScript(string npcName,IEnumerable<int> candidates,int preferredVid=0)
    {
        return NpcIdentityScript()+"import chr,grp,app,player\n"+
            "def distance(v):\n try:return float(player.GetCharacterDistance(v))\n except Exception:\n  try:\n   p=player.GetMainCharacterPosition();q=chr.GetPixelPosition(v);return ((p[0]-q[0])**2+(p[1]-q[1])**2)**0.5\n  except Exception:return 1e30\n"+
            "matches=[]\nfor v in ["+string.Join(",",candidates)+"]:\n try:\n  if _onur_npc_matches(v,"+PythonLiteral(npcName)+"):matches.append((0 if v=="+preferredVid+" else 1,distance(v),v))\n except Exception:continue\nvid=min(matches)[2] if matches else 0\nold=app.GetCursorPosition();pt=None\n"+
            "try:\n if vid:\n  for h in (120,90,150,60,180,40,20):\n   b=chr.GetProjectPosition(vid,h)\n   for dx in (0,-10,10,-20,20):\n    grp.SetCursorPosition(int(b[0]+dx),int(b[1]))\n    if chr.PickAll()==vid:pt=(int(b[0]+dx),int(b[1]));break\n   if pt:break\n"+
            "finally:\n grp.SetCursorPosition(int(old[0]),int(old[1]))\nf.write(str(vid)+'|'+(str(pt[0])+'|'+str(pt[1]) if pt else 'none'))";
    }
    private async Task ClickNpcVerifiedAsync(string npcName,CancellationToken token,DateTime? ignoredDeadline=null,int preferredVid=0,EntityState preferredTarget=null,string expectedMap=null)
    {
        int attempt=0,sentCount=0,lastVid=0;DateTime nextMapGuard=DateTime.MinValue;var approach=new NpcApproachState();
        await SweepQuestDialogsAsync(token);
        // An already-open manual conversation must be consumed before any lobby movement.
        if(await ReadQuestDialogAsync(token)!=null)return;
        if(string.Equals(expectedMap,LobbyMap,StringComparison.OrdinalIgnoreCase))await MoveToDungeonLobbyNpcMarkerAsync(npcName,token);
        DateTime deadline=ignoredDeadline??DateTime.UtcNow.AddSeconds(60);
        string lastResult="NPC henüz doğrulanmadı";
        while(DateTime.UtcNow<deadline)
        {
            attempt++;token.ThrowIfCancellationRequested();
            if(TryRunReviveEscape()){await Task.Delay(100,token);continue;}
            if(!string.IsNullOrEmpty(expectedMap)&&DateTime.UtcNow>=nextMapGuard)
            {
                nextMapGuard=DateTime.UtcNow.AddSeconds(1);
                string currentMap=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token);
                if(!string.IsNullOrEmpty(currentMap)&&!currentMap.StartsWith("ERR")&&currentMap!=expectedMap)return;
            }

            if(await ReadQuestDialogAsync(token)!=null){taskStatus.Text=npcName+" penceresi zaten açık.";return;}
            // Raw actor IDs bypass radar filters; the client revalidates every ID.
            var npcs=ReadVisualCandidateVids();
            var point=await GamePythonAsync(BuildNpcEngineTargetScript(npcName,npcs,preferredVid),token);
            int vidValue,pointX,pointY;
            if(!TryParseNpcPoint(point,out vidValue,out pointX,out pointY))
            {
                ReleaseMovement();lastResult="canlı NPC kimliği bulunamadı ("+point+")";
                taskStatus.Text=npcName+" oyun motorunda aranıyor · "+attempt+". deneme; fare kullanılmıyor.";
                await Task.Delay(350,token);continue;
            }
            if(lastVid!=vidValue){lastVid=vidValue;sentCount=0;}
            // Try in-place first: an NPC at 100 units must not make the character back away to 300.
            ReleaseMovement();
            string result=await OpenNpcThroughEngineAsync(npcName,new[]{vidValue},vidValue,expectedMap,token);
            lastResult=result;
            if(result=="mapchanged")return;
            if(result=="already"){taskStatus.Text=npcName+" konuşma penceresi açık.";return;}
            if(result.StartsWith("sent|",StringComparison.Ordinal))
            {
                sentCount++;taskStatus.Text=npcName+" · motor etkileşimi gönderildi; konuşma penceresi bekleniyor ("+sentCount+"/3).";
                for(int wait=0;wait<20;wait++)
                {
                    await Task.Delay(150,token);
                    if(await ReadQuestDialogAsync(token)!=null){taskStatus.Text=npcName+" konuşma penceresi fare kullanılmadan açıldı.";return;}
                }
                if(sentCount>=3)throw new TimeoutException(npcName+" için motor komutu 3 kez gönderildi ama konuşma doğrulanamadı. Fare tıklaması yapılmadı; NPC/oyun görev koşullarını kontrol edin.");
                continue;
            }
            if(result=="missing"){await Task.Delay(200,token);continue;}
            if(result!="far")throw new InvalidOperationException(npcName+" motor etkileşimi kullanılamıyor: "+result+". Fareye geçilmedi.");
            // Only an actually distant live NPC requires walking.
            var target=await ReadVerifiedNpcPositionAsync(npcName,vidValue,token);
            if(target==null){ReleaseMovement();taskStatus.Text=npcName+" konumu doğrulanamadı · yeniden aranıyor.";await Task.Delay(150,token);continue;}
            await MoveNpcToTalkPositionAsync(target,approach,token);
            await Task.Delay(100,token); // Revalidate live identity, distance and map before the next interaction.
        }
        ReleaseMovement();throw new TimeoutException(npcName+" açılamadı: "+lastResult+". Kayıtlı lobi işaretini ve NPC adını kontrol edin; fare kullanılmadı.");
    }

    // Starts a fresh dungeon run at Kellam. Dialogs are recognised by their buttons:
    //   menu (the dungeon list; the flame run has the flame icon) -> flame option · a two-answer question before the
    //   menu (resume after a disconnect) -> "Hayır" · two answers after the menu (confirmation) -> "Evet" ·
    //   only "Devam"/"Tamam" -> pressed (after the confirmation this ends the talk).
    // A dialog whose text is still being written is left alone until its buttons appear; nothing is ever closed.
    private async Task StartDungeonWithKellamAsync(CancellationToken token)
    {
        var customStage=CurrentDungeonStageSetting(1);
        if(customStage!=null&&customStage.CustomDialogueEnabled){await RunConfiguredNpcEntryAsync(customStage,token);return;}
        string npcName=ConfiguredDungeonNpcName(1);
        bool menuChosen=false,confirmed=false;string handled=null;int handledIndex=-2;
        var quietSince=DateTime.UtcNow.AddSeconds(-3);var handledAt=DateTime.UtcNow;var nextMapCheck=DateTime.MinValue;
        while(true)
        {
            token.ThrowIfCancellationRequested();
            if(DateTime.UtcNow>=nextMapCheck)
            {
                var map=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token);
                if(map.Length>0&&!map.StartsWith("ERR")&&map!=LobbyMap)return;
                nextMapCheck=DateTime.UtcNow.AddSeconds(1);
            }
            var d=await ReadQuestDialogAsync(token);
            if(d==null)
            {
                // A closed confirmation is not proof of entry. Give the warp time, then reopen the NPC.
                if(DateTime.UtcNow-quietSince>TimeSpan.FromSeconds(confirmed?15:2.5))
                {
                    menuChosen=false;confirmed=false;handled=null;
                    await ClickNpcVerifiedAsync(npcName,token,null,0,null,LobbyMap);quietSince=DateTime.UtcNow;
                }
                await Task.Delay(250,token);continue;
            }
            quietSince=DateTime.UtcNow;
            if(!d.Ready){taskStatus.Text=npcName+" · pencere metni yazılıyor, bekleniyor…";await Task.Delay(200,token);continue;}
            bool repeated=d.Snapshot==handled;
            if(repeated&&DateTime.UtcNow-handledAt<TimeSpan.FromMilliseconds(750)){await Task.Delay(200,token);continue;}
            bool nextMenu=menuChosen,nextConfirmed=confirmed;
            string text;int index;
            // Retry the same action on the same page, never reinterpret it as the following confirmation.
            if(repeated){index=handledIndex;text=index<0?"Devam":d.CaptionOf(index);}
            else index=ChooseKellamAnswer(d,ref nextMenu,ref nextConfirmed,out text);
            if(index==-2){taskStatus.Text=npcName+" · beklenmeyen pencere ("+text+"); bekleniyor.";await Task.Delay(500,token);continue;}
            if(!await PressQuestDialogAsync(d,index,token)){await Task.Delay(250,token);continue;}
            menuChosen=nextMenu;confirmed=nextConfirmed;
            handled=d.Snapshot;handledIndex=index;handledAt=DateTime.UtcNow;taskStatus.Text=npcName+" · "+text+".";
            await Task.Delay(100,token);
        }
    }

    // The answer for one ready Kellam dialog: an answer index, -1 for the Next/OK button, -2 when it fits nothing.
    private static int ChooseKellamAnswer(QuestDialogInfo d,ref bool menuChosen,ref bool confirmed,out string text)
    {
        int answers=d.Answers.Count,index;
        int flame=d.FindContaining("alev","ateş");
        int icon=d.Answers.Where(a=>a.Icon).Select(a=>a.Index).DefaultIfEmpty(-1).First();
        int yes=d.Find("evet","tamam","kabul","başla","gir"),no=d.Find("hay");
        if(answers>=2&&!menuChosen&&(icon>=0||flame>=0||answers>=3))
        {
            // Dungeon list: the flame-icon entry; captions without icons fall back to the 2nd entry.
            index=flame>=0?flame:icon;if(index<0)index=d.Answers[Math.Min(1,answers-1)].Index;
            text="alevli seçenek ("+d.CaptionOf(index)+")";menuChosen=true;
        }
        else if(answers==2&&!menuChosen){index=no>=0?no:d.Answers[1].Index;text="kaldığın yerden devam sorusu: "+d.CaptionOf(index);}
        else if(answers>=2&&menuChosen&&!confirmed){index=yes>=0?yes:d.Answers[0].Index;text="onay: "+d.CaptionOf(index);confirmed=true;}
        else if(answers==1){index=d.Answers[0].Index;text="tek seçenek: "+d.CaptionOf(index);if(menuChosen)confirmed=true;}
        else if(answers==0){index=-1;text="Devam";}
        else{index=yes>=0?yes:-2;text=d.ToString();}
        return index;
    }
}

