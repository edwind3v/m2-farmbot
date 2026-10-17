using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

public partial class MainForm {
    private bool commandFlowRunning;
    private bool commandFlowStopped;
    private readonly JavaScriptSerializer commandJson=new JavaScriptSerializer{MaxJsonLength=4000000};
    private sealed class FlowTarget { public int Vid;public float X,Y,Distance; }
    private string FlowFilterScript(DungeonCommand c,int locked=0) {
        string vids=string.Join(",",new[]{locked}.Concat(ReadVisualCandidateVids()).Concat(ReadRadarEntities().Select(e=>e.Property_03C9)).Where(v=>v>0).Distinct().Take(4096));
        int kind=c.NativeTargetType>=0?c.NativeTargetType:c.TargetKind=="Taş"?AddressCatalog.Field_035B:c.TargetKind=="Canavar"?AddressCatalog.Field_035C:c.TargetKind=="NPC"?1:-1;
        return "import player,chr,background,math,json\nrows=[]\nme=player.GetMainCharacterIndex();here=player.GetMainCharacterPosition()\n"+
            "for api in ('HasInstance','GetInstanceType','GetPixelPosition'):\n if not callable(getattr(chr,api,None)):raise RuntimeError('Hedef API bulunamadi')\n"+
            (c.TargetVnum<=0?"":"if not callable(getattr(chr,'GetVirtualNumber',None)):raise RuntimeError('Hedef VNUM API bulunamadi')\n")+
            (string.IsNullOrWhiteSpace(c.TargetName)?"":"if not callable(getattr(chr,'GetNameByVID',None)):raise RuntimeError('Hedef ad API bulunamadi')\n")+
            (c.ConstrainTargetArea?"if background.GetCurrentMapName()!="+PythonLiteral(c.MapName)+":raise RuntimeError('Hedef haritasi farkli')\n":"")+
            "for vid in ["+vids+"]:\n try:\n  if vid==me or not chr.HasInstance(vid):continue\n"+
            (kind<0?"":"  if chr.GetInstanceType(vid)!="+kind+":continue\n")+
            (c.TargetVnum<=0?"":"  if int(chr.GetVirtualNumber(vid))!="+c.TargetVnum+":continue\n")+
            (string.IsNullOrWhiteSpace(c.TargetName)?"":"  if (chr.GetNameByVID(vid) or '').strip().lower()!="+PythonLiteral(c.TargetName.Trim().ToLowerInvariant())+":continue\n")+
            "  dead=getattr(chr,'IsDead',None)\n  if callable(dead) and dead(vid):continue\n"+
            "  p=chr.GetPixelPosition(vid);dx=float(p[0]-here[0]);dy=float(p[1]-here[1]);d=math.hypot(dx,dy)\n  if math.isnan(d) or math.isinf(d):continue\n"+
            (c.ConstrainTargetArea?"  if "+BuildTargetAreaPredicate(c,TaskX,TaskY,mapYSign)+":continue\n":"")+
            "  rows.append({'vid':vid,'dx':dx,'dy':dy,'distance':d})\n except Exception:continue\n";
    }
    private static string BuildTargetAreaPredicate(DungeonCommand c,float playerX,float playerY,int ySign){return "math.hypot(dx+"+playerX.ToString(CultureInfo.InvariantCulture)+"-"+c.X.ToString(CultureInfo.InvariantCulture)+",dy*"+(ySign<0?-1:1)+"+"+playerY.ToString(CultureInfo.InvariantCulture)+"-"+c.Y.ToString(CultureInfo.InvariantCulture)+")>"+c.TargetRadius;}
    private async Task<List<FlowTarget>> FlowTargetsAsync(DungeonCommand c,CancellationToken token,int locked=0) {
        string report=await GamePythonAsync(FlowFilterScript(c,locked)+"f.write(json.dumps(rows))",token);
        var result=new List<FlowTarget>();
        if(string.IsNullOrWhiteSpace(report))throw new InvalidOperationException("Hedef tarama sonucu okunamadı.");
        foreach(Dictionary<string,object> row in (System.Collections.IEnumerable)commandJson.DeserializeObject(report)) {
            float dx=Convert.ToSingle(row["dx"]),dy=Convert.ToSingle(row["dy"]);float x=TaskX+dx,y=TaskY+dy*(mapYSign<0?-1:1);
            if(!IsFiniteCoordinate(x)||!IsFiniteCoordinate(y))continue;
            result.Add(new FlowTarget{Vid=Convert.ToInt32(row["vid"]),X=x,Y=y,Distance=Convert.ToSingle(row["distance"])});
        }
        return result.OrderBy(t=>t.Distance).ToList();
    }
    private const string FlowInventoryScript="import player,json\nrows=[];total=0;slot=-1;free=0\nfor api in ('GetItemIndex','GetItemCount'):\n if not callable(getattr(player,api,None)):raise RuntimeError('Envanter API bulunamadi')\nfor i in range(max(360,int(getattr(player,'INVENTORY_SLOT_COUNT',470)))):\n try:\n  v=int(player.GetItemIndex(i))\n  if v==0:free+=1\n  if v==wanted:\n   total+=int(player.GetItemCount(i))\n   if slot<0:slot=i\n except Exception:continue\n";
    private async Task<Dictionary<string,object>> FlowInventoryAsync(uint itemId,CancellationToken token) {
        string report=await GamePythonAsync("wanted="+itemId+"\n"+FlowInventoryScript+"f.write(json.dumps({'slot':slot,'total':total,'free':free}))",token);
        return commandJson.Deserialize<Dictionary<string,object>>(report);
    }
    private void FlowGuard(int owner,CancellationToken token,bool allowLoading=false) {
        token.ThrowIfCancellationRequested();
        var process=memory.Field_0197?.Property_0268;
        if(emergencyLatched||process==null||process.HasExited||process.Id!=owner||(!allowLoading&&!IsClientReady()))throw new InvalidOperationException("Bağlı karakter/oyun değişti veya hazır değil; komut akışı durdu.");
    }
    private async Task FlowPauseAsync(int owner,CancellationToken token) {
        FlowGuard(owner,token);await SessionPulseAsync();RefreshMountSkillGuards();
        while(MountSkillBusy){ReleaseMovement();FlowGuard(owner,token);await Task.Delay(100,token);}
        string health=await GamePythonAsync("import player\nf.write('alive' if player.GetStatus(player.HP)>0 and player.GetStatus(player.MAX_HP)>0 else 'dead')",token);
        if(health!="alive")throw new InvalidOperationException("Karakter canlı/hazır değil; komut akışı durdu.");
    }
    private void FlowAttack(DungeonCommand c,int vid,bool on) {
        if(!on){automation.AttackEnabled=false;dmgStartCheck.Checked=false;automation.FocusTargetVid=0;return;}
        bool ranged=c.AttackMode=="Menzilli";
        waitDmgRangeCheck.Checked=ranged;waitDmgCheck.Checked=!ranged;
        dmgStoneCheck.Checked=c.TargetKind=="Taş";dmgMonsterCheck.Checked=c.TargetKind=="Canavar";dmgPlayerCheck.Checked=false;
        targetDmgCheck.Checked=targetDmgRangeCheck.Checked=waitArrowCheck.Checked=targetArrowCheck.Checked=false;
        ApplyTargetFilters();
        attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,c.AttackMilliseconds));
        attackRangeBar.Value=Math.Max(attackRangeBar.Minimum,Math.Min(attackRangeBar.Maximum,ranged?c.AttackRange:250));
        dmgStartCheck.Checked=true;
        automation.WaitDamage=!ranged;automation.WaitDamageRange=ranged;automation.TargetDamage=automation.TargetDamageRange=automation.WaitArrow=automation.TargetArrow=false;
        automation.AttackDelay=c.AttackMilliseconds;automation.AttackRange=ranged?c.AttackRange:250;
        automation.FocusTargetVid=vid;automation.GroundStoneWait=c.TargetKind=="Taş"&&!ranged;automation.AttackEnabled=true;automation.Start();
    }
    private void FlowWalk(Walker walker,float tx,float ty,int stop) {
        if(MountSkillBusy){ReleaseMovement();return;}
        float x=TaskX,y=TaskY;if(Distance2D(x,y,tx,ty)<=stop){ReleaseMovement();return;}
        if(atlas==null||!mapCalibrated)throw new InvalidOperationException("Bu haritada güvenli yol verisi yok. Pack haritasını yükleyin; rastgele yürünmedi.");
        if(nav==null||navMap!=atlas.Name){nav=new DungeonNavigator(atlas.Width,atlas.Height,atlas.IsBlocked);navMap=atlas.Name;walker.Reset();}
        int cx,cy,gx,gy;CellOf(x,y,out cx,out cy);int goal=CellOf(tx,ty,out gx,out gy);
        if(gx<0||gy<0||gx>=nav.Width||gy>=nav.Height)throw new InvalidOperationException("Hedef harita sınırı dışında.");
        if(walker.Path==null||walker.Goal!=goal||DateTime.UtcNow-walker.PathAt>TimeSpan.FromSeconds(2)) {
            var path=nav.FindPath(cx,cy,gx,gy);
            if(path==null)throw new InvalidOperationException("Hedefe güvenli yol bulunamadı; duvar içinden yürünmedi.");
            walker.Goal=goal;walker.Path=path;walker.PathAt=DateTime.UtcNow;
        }
        WalkTo(walker,goal,x,y,cx,cy,movementClock.ElapsedMilliseconds,new PointF(tx,ty),stop);
    }
    private async Task<bool> FlowConditionAsync(DungeonCommand c,CancellationToken token) {
        switch(c.Condition) {
            case DungeonCondition.ItemAvailable:return Convert.ToInt64((await FlowInventoryAsync(c.ItemId,token))["total"])>=c.Count;
            case DungeonCondition.FreeInventorySlots:return Convert.ToInt64((await FlowInventoryAsync(0,token))["free"])>=c.Count;
            case DungeonCondition.TargetExists:return (await FlowTargetsAsync(c,token)).Count>0;
            case DungeonCondition.MapEquals:return (await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token))==c.MapName;
            case DungeonCondition.HealthBelow:
                double hp;return double.TryParse(await GamePythonAsync("import player\nm=player.GetStatus(player.MAX_HP)\nf.write(str(100.0*player.GetStatus(player.HP)/m if m else 101))",token),NumberStyles.Float,CultureInfo.InvariantCulture,out hp)&&hp<c.HealthPercent;
        }
        return false;
    }
    private async Task FlowInteractAsync(DungeonCommand c,FlowTarget target,CancellationToken token,int owner=0) {
        string script=(owner>0?"import os\nif os.getpid()!="+owner+":raise RuntimeError('Istemci degisti')\n":"")+FlowFilterScript(c,target.Vid)+"if player.GetStatus(player.HP)<=0 or player.GetStatus(player.MAX_HP)<=0:raise RuntimeError('Karakter hazir degil')\n"+"matches=[r for r in rows if r['vid']=="+target.Vid+" and r['distance']<=500]\n"+
            "if not matches:\n f.write('not_ready')\n return\nimport net\nnet.SendOnClickPacket("+target.Vid+")\nf.write('sent')";
        if(await GamePythonAsync(script,token)!="sent")throw new InvalidOperationException("Hedef tıklama kimliği/mesafesi değişti; tıklanmadı.");
    }
    private async Task ExecuteFlowCommandAsync(DungeonCommand c,int owner,CancellationToken token) {
        FlowAttack(c,0,false);ReleaseMovement();
        if(c.Kind==DungeonCommandKind.Notify){RecordActivity("Zindan komutu",c.Name);return;}
        if(c.Kind==DungeonCommandKind.Wait){await Task.Delay(c.WaitMilliseconds,token);FlowGuard(owner,token);return;}
        var watch=Stopwatch.StartNew();var walker=new Walker();int locked=0,killed=0;bool seen=false;long emptyAt=-1;long before=-1;bool sent=false;var spawnBefore=new HashSet<int>();
        string startingMap=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token);
        while(watch.ElapsedMilliseconds<c.TimeoutMilliseconds) {
            bool waitingMap=c.Kind==DungeonCommandKind.WaitMap||(c.Kind==DungeonCommandKind.Interact&&sent&&c.InteractionResult==DungeonInteractionResult.MapChanged);
            if(waitingMap){
                FlowGuard(owner,token,true);
                if(!IsClientReady()){await Task.Delay(150,token);continue;}
                string phase=await GamePythonAsync(FindMainStream+"f.write('game' if ms is not None and type(ms.curPhaseWindow).__name__=='GameWindow' else 'loading')",token);
                if(phase!="game"){await Task.Delay(150,token);continue;}
            }
            await FlowPauseAsync(owner,token);
            string currentMap=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token);
            if(c.Kind!=DungeonCommandKind.WaitMap&&!(c.Kind==DungeonCommandKind.Interact&&sent&&c.InteractionResult==DungeonInteractionResult.MapChanged)&&!string.IsNullOrWhiteSpace(startingMap)&&currentMap!=startingMap)throw new InvalidOperationException("Komut sırasında harita değişti; sonraki işlemler gönderilmedi. Harita geçişini ayrı bekleme komutuyla tanımlayın.");
            if(c.Kind==DungeonCommandKind.Interact&&sent) {
                if(c.InteractionResult==DungeonInteractionResult.MapChanged){if(DungeonInteractionEvidence.ExpectedMap(startingMap,currentMap,c.ResultMapName))return;}
                else if(c.InteractionResult==DungeonInteractionResult.DialogueOpened){if(await ReadQuestDialogAsync(token)!=null)return;}
                else if(c.InteractionResult==DungeonInteractionResult.TargetGone){if(await GamePythonAsync("import chr\nf.write('gone' if not chr.HasInstance("+locked+") else 'present')",token)=="gone")return;}
                else if(c.InteractionResult==DungeonInteractionResult.TargetAppeared){var expected=ResultSpawnFilter(c);if(DungeonInteractionEvidence.NewTarget(spawnBefore,(await FlowTargetsAsync(expected,token)).Select(t=>t.Vid)))return;}
                await Task.Delay(150,token);continue;
            }
            if(c.Kind==DungeonCommandKind.WaitMap){if(currentMap==c.MapName)return;}
            else if(c.Kind==DungeonCommandKind.Move) {
                if((string.IsNullOrWhiteSpace(c.MapName)||currentMap==c.MapName)&&Distance2D(TaskX,TaskY,c.X,c.Y)<=c.Range)return;
                if(!string.IsNullOrWhiteSpace(c.MapName)&&currentMap!=c.MapName)throw new InvalidOperationException("Konuma git komutunun haritası farklı.");
                FlowWalk(walker,c.X,c.Y,c.Range);
            }
            else if(c.Kind==DungeonCommandKind.CollectItem||c.Kind==DungeonCommandKind.UseItem) {
                var inventory=await FlowInventoryAsync(c.ItemId,token);long total=Convert.ToInt64(inventory["total"]);
                if(c.Kind==DungeonCommandKind.CollectItem) {
                    if(total>=c.Count)return;
                    await GamePythonAsync("import player\nplayer.PickCloseItemAll()\nf.write('pickup')",token);
                }else{
                    if(sent&&total<before)return;
                    if(!sent){
                        if(total<c.Count)throw new InvalidOperationException("Kullanılacak eşya envanterde yok.");
                        before=total;
                        string r=await GamePythonAsync("wanted="+c.ItemId+"\n"+FlowInventoryScript+"import net\nif slot<0:\n f.write('missing')\n return\nnet.SendItemUsePacket(slot)\nf.write('sent')",token);
                        if(r!="sent")throw new InvalidOperationException("Eşya yuvası değişti; kullanılmadı.");sent=true;
                    }
                }
            }
            else {
                var targets=await FlowTargetsAsync(c,token,locked);
                if(c.Kind==DungeonCommandKind.WaitTarget){if(targets.Count>0)return;}
                else if(c.Kind==DungeonCommandKind.WaitTargetGone){if(targets.Count==0)return;}
                else if(c.Kind==DungeonCommandKind.ClearTargets) {
                    if(locked>0&&!targets.Any(t=>t.Vid==locked)) {
                        string gone=await GamePythonAsync("import chr\nv="+locked+"\ndead=getattr(chr,'IsDead',None)\nf.write('gone' if not chr.HasInstance(v) or (callable(dead) and dead(v)) else 'alive')",token);
                        if(gone!="gone")throw new InvalidOperationException("Kilitli hedef hâlâ var ama filtre/kimlik değişti; başka hedefe geçilmedi.");
                        killed++;locked=0;walker.Reset();FlowAttack(c,0,false);
                        if(c.KillLimit>0&&killed>=c.KillLimit)return;
                    }
                    if(targets.Count>0){seen=true;emptyAt=-1;}
                    if(targets.Count==0) {
                        FlowAttack(c,0,false);ReleaseMovement();
                        if(c.KillLimit==0&&(!c.RequireTargetSeen||seen)){if(emptyAt<0)emptyAt=watch.ElapsedMilliseconds;if(watch.ElapsedMilliseconds-emptyAt>=c.WaitMilliseconds)return;}
                    }else{
                        var target=targets.FirstOrDefault(t=>t.Vid==locked)??targets[0];
                        if(locked!=target.Vid){locked=target.Vid;walker.Reset();}
                        int stop=HybridWaitRequested?120:c.AttackMode=="Menzilli"?Math.Max(100,c.AttackRange*7/10):120;
                        FlowAttack(c,locked,target.Distance<=(c.AttackMode=="Menzilli"?c.AttackRange:250));
                        if(target.Distance>stop)FlowWalk(walker,target.X,target.Y,stop);else ReleaseMovement();
                    }
                }else{
                    var target=targets.FirstOrDefault(t=>t.Vid==locked)??targets.FirstOrDefault();
                    if(c.Kind==DungeonCommandKind.GiveItem&&sent) {
                        if(Convert.ToInt64((await FlowInventoryAsync(c.ItemId,token))["total"])<before)return;
                    }else if(target!=null){
                        locked=target.Vid;
                        if(target.Distance>Math.Min(350,c.Range)){FlowWalk(walker,target.X,target.Y,Math.Min(350,c.Range));}
                        else{
                            ReleaseMovement();
                            if(c.Kind==DungeonCommandKind.GiveItem) {
                                before=Convert.ToInt64((await FlowInventoryAsync(c.ItemId,token))["total"]);
                                if(before<c.Count)throw new InvalidOperationException("Hedefe verilecek eşya envanterde yok.");
                                string result=await GamePythonAsync("import os\nif os.getpid()!="+owner+":raise RuntimeError('Istemci degisti')\n"+FlowFilterScript(c,locked)+"valid=[r for r in rows if r['vid']=="+locked+" and r['distance']<=500]\nif not valid:\n f.write('not_ready')\n return\n"+
                                    "wanted="+c.ItemId+"\n"+FlowInventoryScript+"import net\nif slot<0 or int(player.GetItemCount(slot))<"+c.Count+":\n f.write('missing')\n return\nnet.SendGiveItemPacket("+locked+",getattr(player,'INVENTORY',0),slot,"+c.Count+")\nf.write('sent')",token);
                                if(result!="sent")throw new InvalidOperationException("Eşya/hedef doğrulanamadı; verme isteği gönderilmedi.");sent=true;
                            }else if(c.Kind==DungeonCommandKind.Interact){
                                if(c.InteractionResult==DungeonInteractionResult.DialogueOpened&&await ReadQuestDialogAsync(token)!=null)throw new InvalidOperationException("Zaten açık bir konuşma var; tıklama sonucuyla karıştırılmadı. Önce kapatın.");
                                if(c.InteractionResult==DungeonInteractionResult.TargetAppeared)spawnBefore=new HashSet<int>((await FlowTargetsAsync(ResultSpawnFilter(c),token)).Select(t=>t.Vid));
                                await FlowInteractAsync(c,target,token,owner);sent=true;if(c.InteractionResult==DungeonInteractionResult.Sent)return;
                            }
                            else if(c.Kind==DungeonCommandKind.Dialogue){
                                if(await ReadQuestDialogAsync(token)==null)await FlowInteractAsync(c,target,token,owner);
                                await RunNpcDialogueStepsAsync(c.DialogueSteps,s=>taskStatus.Text=s,token);return;
                            }
                        }
                    }
                }
            }
            await Task.Delay(150,token);
        }
        throw new TimeoutException(c+" doğrulanamadı; "+(sent?"komut otomatik tekrarlanmadı, envanteri/hedefi kontrol edin.":"zaman aşımı."));
    }
    private static DungeonCommand ResultSpawnFilter(DungeonCommand c){var expected=c.Copy();expected.NativeTargetType=-1;expected.TargetKind="Tümü";expected.TargetName="";expected.TargetVnum=c.ResultTargetVnum;return expected;}
    private async Task RunDungeonCommandFlowAsync(CancellationToken token) {
        commandFlowStopped=true;
        var plan=new DungeonFlowPlan(taskSequence.Commands);
        if(IsX86ClientMode)throw new InvalidOperationException("Komut akışının hareket bağlantısı x86 istemcide henüz doğrulanmadı.");
        int owner=memory.Field_0197.Property_0268.Id;commandFlowRunning=true;taskDungeonStage=0;sessionAuto.Checked=true;sessionDirty=true;
        wallCheck.Checked=binekWallCheck.Checked=false;autoPickCheck.Checked=false;
        try {
            await plan.RunAsync((c,t)=>ExecuteFlowCommandAsync(c,owner,t),async(c,t)=>{await FlowPauseAsync(owner,t);return await FlowConditionAsync(c,t);},
                (i,c)=>{taskStatus.Text="Komut "+(i+1)+"/"+plan.Commands.Count+" · "+c;RecordActivity("Zindan akışı",taskStatus.Text);},token);
            taskStatus.Text="Komut akışı tamamlandı.";
        }catch(Exception ex) when(!(ex is OperationCanceledException)){
            throw new InvalidOperationException("Komut akışı durdu: "+ex.Message,ex);
        }finally{commandFlowRunning=false;commandFlowStopped=true;resumeTaskAfterRestart=false;resumeDungeonStage=0;FlowAttack(new DungeonCommand(),0,false);ReleaseMovement();}
    }
}
