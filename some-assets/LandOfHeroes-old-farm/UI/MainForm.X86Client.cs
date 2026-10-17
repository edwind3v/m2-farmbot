using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    private bool x86BotBusy,x86BotDesired,x86BotApplied,x86RadarBusy,x86BotTickBusy;
    private int x86TargetVid;
    private DateTime x86NextMoveClick=DateTime.MinValue;
    private System.Windows.Forms.Timer x86BotTimer;
    private bool IsX86ClientMode { get { return clientConnected&&memory.Field_0197!=null&&!memory.Field_0197.Property_026A; } }
    private const string X86AliasBootstrap=
        "import sys\n_g=sys.modules.get('game')\nif _g is None:raise RuntimeError('game window is not ready')\n"+
        "for _n in ('app','background','chat','chr','chrmgr','event','grp','item','net','player','skill','effect','exchange','guild','ime','messenger','pack','quest','systemSetting','textTail','wndMgr'):\n _m=getattr(_g,_n,None)\n if _m is not None:sys.modules[_n]=_m\n"+
        "_im=getattr(_g,'interfaceModule',None)\nif _im is not None:\n for _n in ('uiQuest','uiAutoHunt','miniMap'):\n  _m=getattr(_im,_n,None)\n  if _m is not None:sys.modules[_n]=_m\n";

    private async Task EnsureX86AliasesAsync(CancellationToken token)
    {
        var process=memory.Field_0197==null?null:memory.Field_0197.Property_0268;
        if(process==null||process.HasExited)throw new InvalidOperationException("x86 oyun istemcisi kapalı.");
        using(var bridge=new QueuedPythonBridge(process))
        {
            bridge.Queue(X86AliasBootstrap);
            DateTime until=DateTime.UtcNow.AddSeconds(5);
            while(!bridge.Poll()){token.ThrowIfCancellationRequested();if(DateTime.UtcNow>=until)throw new TimeoutException("x86 oyun API eşlemesi zaman aşımına uğradı.");await Task.Delay(35,token);}
        }
    }

    private string X86StopBody()
    {
        return X86AliasBootstrap+
            "try:_g.player.ClearTarget()\nexcept:pass\n"+
            "try:_g.player.EndMouseWalking()\nexcept:pass\n"+
            "try:_g.player.EndKeyWalkingImmediately()\nexcept:pass\n"+
            "try:_g.player.SetAttackKeyState(0)\nexcept:pass\n"+
            "f.write('stopped')\n";
    }

    private string X86EngageBody(int vid)
    {
        return X86AliasBootstrap+
            "_v="+vid.ToString(CultureInfo.InvariantCulture)+"\n_pl=_g.player\n_ch=_g.chr\n"+
            "if getattr(getattr(globals().get('_onur_agent'), 'mount_cycle', None),'busy',False):\n f.write('skillbusy')\n return\n"+
            "if not _ch.HasInstance(_v):\n f.write('gone')\nelse:\n"+
            " _d=float(_pl.GetCharacterDistance(_v))\n"+
            " if _d<=350.0:\n"+
            "  try:_pl.SetTarget(_v)\n  except:pass\n"+
            "  try:_g.net.SendOnClickPacket(_v)\n  except:pass\n"+
            "  try:_pl.ComboAttack()\n  except:pass\n"+
            "  try:_pl.SetAttackKeyState(1)\n  except:pass\n"+
            "  f.write('attack|'+str(int(_d)))\n"+
            " else:\n"+
            "  try:_pl.SetAttackKeyState(0)\n  except:pass\n"+
            "  import grp,app\n  _old=app.GetCursorPosition()\n  _pt=None\n  _raw=None\n"+
            "  try:\n"+
            "   for _h in (90,60,120,40,150,20):\n"+
            "    _b=_ch.GetProjectPosition(_v,_h)\n"+
            "    if _b and len(_b)>=2:\n"+
            "     if _raw is None:_raw=(int(_b[0]),int(_b[1]))\n"+
            "     for _dx in (0,-10,10,-20,20):\n"+
            "      grp.SetCursorPosition(int(_b[0]+_dx),int(_b[1]))\n"+
            "      if _ch.PickAll()==_v:_pt=(int(_b[0]+_dx),int(_b[1]));break\n"+
            "     if _pt:break\n"+
            "  finally:grp.SetCursorPosition(int(_old[0]),int(_old[1]))\n"+
            "  if _pt:f.write('click|'+str(_pt[0])+'|'+str(_pt[1])+'|'+str(int(_d)))\n"+
            "  elif _raw:f.write('move|'+str(_raw[0])+'|'+str(_raw[1])+'|'+str(int(_d)))\n"+
            "  else:f.write('no_point|'+str(int(_d)))\n";
    }

    private void EnsureX86BotTimer()
    {
        if(x86BotTimer!=null)return;
        x86BotTimer=new System.Windows.Forms.Timer(components){Interval=180};
        x86BotTimer.Tick+=async (s,e)=>await X86BotTickAsync();
    }

    private async void RequestX86BotState(bool enabled)
    {
        x86BotDesired=enabled;
        if(!enabled)
        {
            x86BotTimer?.Stop();x86TargetVid=0;x86BotApplied=false;
            if(!IsX86ClientMode||x86BotBusy)return;
            x86BotBusy=true;
            try{await GamePythonAsync(X86StopBody(),CancellationToken.None);if(farmStatus!=null)farmStatus.Text="x86 saldırı ve hareket botu durduruldu.";}
            catch(Exception ex){if(farmStatus!=null)farmStatus.Text="x86 bot durdurma: "+ex.Message;}
            finally{x86BotBusy=false;}
            return;
        }
        if(!IsX86ClientMode)return;
        EnsureX86BotTimer();x86BotApplied=true;x86BotTimer.Start();
        if(farmStatus!=null)farmStatus.Text="x86 yerel bot çalışıyor · canlı hedef aranıyor.";
    }

    private async Task X86BotTickAsync()
    {
        if(x86BotTickBusy||!x86BotDesired||!IsX86ClientMode)return;
        x86BotTickBusy=true;
        try
        {
            bool mobs=(dmgMonsterCheck!=null&&dmgMonsterCheck.Checked)||(walkCanavarCheck!=null&&walkCanavarCheck.Checked);
            bool stones=(dmgStoneCheck!=null&&dmgStoneCheck.Checked)||(walkMetinCheck!=null&&walkMetinCheck.Checked);
            if(!mobs&&!stones){mobs=true;stones=true;}
            PointF areaCenter;float areaRadius;bool area=TryGetFarmAreaMemory(out areaCenter,out areaRadius);
            double range=searchRange==null?20000.0:(double)searchRange.Value;
            var candidates=radarEntities.Where(e=>(mobs&&e.Property_03CA==AddressCatalog.Field_035C)||(stones&&e.Property_03CA==AddressCatalog.Field_035B))
                .Where(e=>e.Distance<=range)
                .Where(e=>!area||DistanceSquared(e.X,e.Y,areaCenter.X,areaCenter.Y)<=areaRadius*areaRadius)
                .OrderBy(e=>e.Distance).ToList();
            var target=candidates.FirstOrDefault(e=>e.Property_03C9==x86TargetVid)??candidates.FirstOrDefault();
            if(target==null)
            {
                x86TargetVid=0;
                if(farmStatus!=null)farmStatus.Text="x86 yerel bot çalışıyor · menzil içinde hedef taranıyor.";
                return;
            }
            x86TargetVid=target.Property_03C9;
            string report=await GamePythonAsync(X86EngageBody(x86TargetVid),CancellationToken.None);
            if(report=="gone")
            {
                radarEntities=radarEntities.Where(e=>e.Property_03C9!=x86TargetVid).ToList();x86TargetVid=0;
                if(farmStatus!=null)farmStatus.Text="Hedef bitti · sıradaki gruba geçiliyor.";
                return;
            }
            if(report.StartsWith("ERR"))throw new InvalidOperationException(report);
            string[] parts=(report??"").Split('|');int distance=parts.Length>1&&int.TryParse(parts[parts.Length-1],out distance)?distance:(int)target.Distance;
            if(parts.Length>0&&parts[0]=="attack")
            {
                x86BotApplied=true;
                if(farmStatus!=null)farmStatus.Text="x86 çevre vuruş · hedef "+x86TargetVid+" · "+distance+" birim · 180 ms";
                return;
            }
            if((parts[0]=="click"||parts[0]=="move")&&parts.Length>=4&&DateTime.UtcNow>=x86NextMoveClick)
            {
                int x,y;if(!int.TryParse(parts[1],out x)||!int.TryParse(parts[2],out y))return;
                int clickVid=parts[0]=="click"?x86TargetVid:0;
                if(clickVid==0)
                {
                    NativeRect rect;var handle=TaskGameWindow();
                    if(handle==IntPtr.Zero||!GetClientRect(handle,out rect))throw new InvalidOperationException("x86 oyun penceresi hazır değil.");
                    x=Math.Max(80,Math.Min(rect.Right-80,x));y=Math.Max(100,Math.Min(rect.Bottom-120,y));
                }
                x86NextMoveClick=DateTime.UtcNow.AddMilliseconds(220);
                string click=await ClickFocusedNpcAsync(x,y,clickVid,CancellationToken.None);
                if(farmStatus!=null)farmStatus.Text=click=="clicked"?"x86 hedefe yaklaşıyor · "+distance+" birim":"x86 hedef tıklaması yenileniyor · "+click;
                return;
            }
            if(farmStatus!=null)farmStatus.Text="x86 hedef ekrana alınıyor · "+distance+" birim";
        }
        catch(Exception ex)
        {
            x86TargetVid=0;
            if(farmStatus!=null)farmStatus.Text="x86 bot yeniden deniyor: "+ex.Message;
        }
        finally{x86BotTickBusy=false;}
    }

    private async Task RefreshX86RadarAsync()
    {
        if(x86RadarBusy||!IsX86ClientMode)return;x86RadarBusy=true;
        try
        {
            string body=X86AliasBootstrap+
                "import __builtin__ as _bi\n_pl=_g.player;_ch=_g.chr;_base=int(_pl.GetMainCharacterIndex())\n_s=getattr(_bi,'_onur_x86_radar',None)\n"+
                "if _s is None or _s.get('base')!=_base:\n _ranges=[(1,300000)]\n if _base>300000:_ranges.append((max(1,_base-250000),_base+250000))\n _s={'base':_base,'ranges':_ranges,'ri':0,'cur':_ranges[0][0],'e':{}};setattr(_bi,'_onur_x86_radar',_s)\n"+
                "_lo,_hi=_s['ranges'][_s['ri']];_start=_s['cur'];_end=min(_hi,_start+20000)\nfor _v in xrange(_start,_end):\n try:\n  if not _ch.HasInstance(_v):\n   _s['e'].pop(_v,None);continue\n  _t=int(_ch.GetInstanceType(_v))\n  if _t not in (0,1,2,6):continue\n  _p=_ch.GetPixelPosition(_v)\n  if not _p:continue\n  _s['e'][_v]=(_t,int(_ch.GetVirtualNumber(_v)),float(_p[0]),float(_p[1]),float(_p[2]),float(_pl.GetCharacterDistance(_v)),str(_ch.GetNameByVID(_v)).replace('|',' ').replace('\\n',' '))\n except:pass\n"+
                "_s['cur']=_end\nif _end>=_hi:\n _s['ri']=(_s['ri']+1)%len(_s['ranges']);_s['cur']=_s['ranges'][_s['ri']][0]\n_rows=[]\nfor _v,_a in sorted(_s['e'].items(),key=lambda _x:_x[1][5])[:500]:_rows.append(str(_v)+'|'+str(_a[0])+'|'+str(_a[1])+'|'+str(_a[2])+'|'+str(_a[3])+'|'+str(_a[4])+'|'+str(_a[5])+'|'+_a[6])\nf.write('\\n'.join(_rows))\n";
            string report=await GamePythonAsync(body,CancellationToken.None);var list=new List<EntityState>();
            foreach(string line in (report??"").Replace("\r","").Split('\n'))
            {
                string[] a=line.Split(new[]{'|'},8);int vid,type,vnum;float x,y,z,d;
                if(a.Length<7||!int.TryParse(a[0],out vid)||!int.TryParse(a[1],out type)||!int.TryParse(a[2],out vnum)||!float.TryParse(a[3],NumberStyles.Float,CultureInfo.InvariantCulture,out x)||!float.TryParse(a[4],NumberStyles.Float,CultureInfo.InvariantCulture,out y)||!float.TryParse(a[5],NumberStyles.Float,CultureInfo.InvariantCulture,out z)||!float.TryParse(a[6],NumberStyles.Float,CultureInfo.InvariantCulture,out d))continue;
                list.Add(new EntityState{Property_03C9=vid,Property_03CA=type,Property_03CB=0,X=x,Y=y,Z=z,Distance=d,Property_03CD=a.Length>7?a[7]:""});
            }
            radarEntities=list.OrderBy(e=>e.Distance).ToList();RenderRadar(radarEntities);
        }
        catch(Exception ex){if(farmStatus!=null)farmStatus.Text="x86 radar: "+ex.Message;}finally{x86RadarBusy=false;}
    }
    private async Task<PlayerInventorySnapshot> CaptureX86InventoryAsync()
    {
        string body=X86AliasBootstrap+"_pl=_g.player;_it=_g.item;_sk=_g.skill;_rows=[]\n"+
            "for _slot in xrange(255):\n try:\n  _id=int(_pl.GetSkillIndex(_slot))\n  if not _id:continue\n  try:_name=str(_sk.GetSkillName(_id))\n  except:_name='Beceri #'+str(_id)\n  _name=_name.replace('|',' ').replace('\\n',' ')\n  _rows.append('S|'+str(_slot)+'|'+str(_id)+'|'+str(int(_pl.GetSkillLevel(_slot)))+'|'+str(int(_pl.GetSkillGrade(_slot)))+'|'+('1' if _pl.IsSkillActive(_slot) else '0')+'|'+_name)\n except:pass\n"+
            "for _slot in xrange(470):\n try:\n  _id=int(_pl.GetItemIndex(_slot))\n  if not _id:continue\n  _count=int(_pl.GetItemCount(_slot))\n  try:_it.SelectItem(_id);_name=str(_it.GetItemName())\n  except:_name='Esya #'+str(_id)\n  _rows.append('I|'+str(_slot)+'|'+str(_id)+'|'+str(_count)+'|'+_name.replace('|',' ').replace('\\n',' '))\n except:pass\nf.write('\\n'.join(_rows))\n";
        string report=await GamePythonAsync(body,CancellationToken.None);var snapshot=new PlayerInventorySnapshot{CapturedAt=DateTime.Now};
        foreach(string line in (report??"").Replace("\r","").Split('\n'))
        {
            string[] a=line.Split('|');int slot;uint id;if(a.Length<4||!int.TryParse(a[1],out slot)||!uint.TryParse(a[2],out id))continue;
            if(a[0]=="S"&&a.Length>=7){uint level,grade;uint.TryParse(a[3],out level);uint.TryParse(a[4],out grade);snapshot.Skills.Add(new DetectedSkill{Slot=slot,Id=id,Level=level,Grade=grade,Active=a[5]=="1",Cooldown=0,Name=string.Join("|",a.Skip(6))});}
            else if(a[0]=="I"&&a.Length>=5){uint count;uint.TryParse(a[3],out count);snapshot.Items.Add(new DetectedItem{Slot=slot,Id=id,Count=count,Name=string.Join("|",a.Skip(4))});}
        }
        return snapshot;
    }
}
