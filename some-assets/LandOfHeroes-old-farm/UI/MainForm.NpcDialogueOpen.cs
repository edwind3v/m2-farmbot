using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    private GameCallBridge npcNativeBridge;
    private bool npcNativeBusy;
    private async Task<string> OpenNpcThroughEngineAsync(string name,IEnumerable<int> candidates,int preferredVid,string expectedMap,CancellationToken token)
    {
        string result=await GamePythonAsync(BuildNpcOpenOnMapScript(name,candidates,preferredVid,expectedMap),token);
        if(!result.StartsWith("native|",StringComparison.Ordinal))return result;
        var parts=result.Split('|');int vid,main,race;
        if(parts.Length!=4||!int.TryParse(parts[1],out vid)||!int.TryParse(parts[2],out main)||!int.TryParse(parts[3],out race))throw new InvalidOperationException("NPC motor doğrulama raporu geçersiz.");
        if(npcNativeBusy)throw new InvalidOperationException("Önceki NPC motor isteği bekleniyor.");
        if(npcNativeBridge==null)npcNativeBridge=new GameCallBridge(memory);
        npcNativeBusy=true;
        try
        {
            using(var linked=CancellationTokenSource.CreateLinkedTokenSource(token,emergencyOperations.Token))
            {
                bool sent=await Task.Run(()=>NativeNpcInteraction.Open(memory,npcNativeBridge,vid,main,race,linked.Token));
                linked.Token.ThrowIfCancellationRequested();
                if(!sent)throw new InvalidOperationException("NPC motor etkileşimi tamamlanmadı: "+npcNativeBridge.Property_03EE);
                return "sent|"+vid;
            }
        }
        finally{npcNativeBusy=false;}
    }
    private static string NpcEngineCandidatesScript(string name,IEnumerable<int> candidates,int preferredVid)
    {
        var ids=(candidates??Enumerable.Empty<int>()).Concat(new[]{preferredVid}).Where(v=>v>0).Distinct();
        return NpcIdentityScript()+"import chr,player,math\n"+
            "matches=[]\nfor v in ["+string.Join(",",ids)+"]:\n try:\n  if not _onur_npc_matches(v,"+PythonLiteral(name)+"):continue\n  try:distance=float(player.GetCharacterDistance(v))\n  except Exception:\n   p=chr.GetPixelPosition(v);m=player.GetMainCharacterPosition();distance=((p[0]-m[0])**2+(p[1]-m[1])**2)**0.5\n  if math.isnan(distance) or math.isinf(distance) or distance<0:continue\n  matches.append((0 if v=="+preferredVid+" else 1,distance,v))\n except Exception:continue\n";
    }
    private static string BuildNpcEngineTargetScript(string name,IEnumerable<int> candidates,int preferredVid=0)
    {
        // Never depend on projection, camera, visibility or a mouse pick.
        return NpcEngineCandidatesScript(name,candidates,preferredVid)+"f.write(str(min(matches)[2] if matches else 0)+'|none')\n";
    }
    private static string BuildNpcOpenOnMapScript(string name,IEnumerable<int> candidates,int preferredVid,string expectedMap)
    {
        string guard=string.IsNullOrWhiteSpace(expectedMap)?"":"import background\nif background.GetCurrentMapName()!="+PythonLiteral(expectedMap)+":\n f.write('mapchanged')\n return\n";
        return guard+BuildNpcOpenInPlaceScript(name,candidates,preferredVid);
    }
    private static string BuildNpcOpenInPlaceScript(string name,IEnumerable<int> candidates,int preferredVid)
    {
        // The identity check and interaction run together on the client main thread.
        // Do not reuse a projected screen position or cause automatic walking.
        return DialogState+"if best is not None:\n f.write('already')\n return\n"+NpcEngineCandidatesScript(name,candidates,preferredVid)+
            "import net\n"+
            "near=[m for m in matches if m[1]<=500.0]\n"+
            "if not near:\n f.write('far' if matches else 'missing')\n return\n"+
            "vid=min(near)[2]\nclick=getattr(net,'SendOnClickPacket',None)\n"+
            "if callable(click):\n click(vid)\nelse:\n f.write('native|'+str(vid)+'|'+str(player.GetMainCharacterIndex())+'|'+str(chr.GetVirtualNumber(vid)))\n return\n"+
            "f.write('sent|'+str(vid))\n";
    }
    private async Task OpenNpcDialogueInPlaceAsync(TaskStage stage,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(await ReadQuestDialogAsync(token)!=null)return;
        string name=string.IsNullOrWhiteSpace(stage.NpcName)?"Kellam":stage.NpcName;
        string result=await OpenNpcThroughEngineAsync(name,ReadVisualCandidateVids(),stage.NpcVid,null,token);
        if(result=="already")return;
        if(!result.StartsWith("sent|",StringComparison.Ordinal))
            throw new InvalidOperationException(result=="far"?name+" uzakta. NPC'nin yanına gel; editör karakteri yürütmez.":result=="missing"?name+" yakında doğrulanamadı. Seçili NPC'yi kontrol et veya konuşmayı elle aç.":"NPC açma komutu uygulanamadı ("+result+"). Konuşmayı elle açıp sırayı çalıştırabilirsin.");
        var watch=System.Diagnostics.Stopwatch.StartNew();
        while(watch.ElapsedMilliseconds<6000)
        {
            await Task.Delay(200,token);
            if(await ReadQuestDialogAsync(token)!=null)return;
        }
        throw new TimeoutException(name+" komutu gönderildi ama konuşma açılmadı. NPC'nin yanına gelip elle aç; rastgele ekran tıklaması yapılmadı.");
    }
    private Task RunNpcDialogueStepsAsync(IEnumerable<NpcDialogueStep> steps,Action<string> status,CancellationToken token)
    {
        return NpcDialogueRunner.RunAsync(steps,async t=>{
            var dialog=await ReadQuestDialogAsync(t);return dialog==null?null:DialoguePage(dialog);
        },(page,index,t)=>PressQuestDialogAsync(ParseQuestDialogReport(page.Snapshot),index,t),status,token);
    }
}
