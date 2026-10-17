using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Map-driven dungeon runner. No timers, recorded paths or headings decide anything: the live map (minimap atlas +
// attribute grid) tells which floor the character is on, and every tick picks the most important job there:
//   quest dialog > symbol to NPC > quest ground loot > monsters/stones of this room > final Kellam talk > exploring.
// Floors are rooms of the attribute map (the maze floor is its nearby corridor pieces). A new floor completes a stage;
// leaving the dungeon map for the lobby completes the run, which restarts when "8. aşamadan sonra yeniden başlat" is on.
public partial class MainForm
{
    private const string LobbyMap="map_dungeon_lobby";
    private const int SmartDungeonStage=9;
    private DungeonNavigator nav;private string navMap,mapsExtractedFor;
    private int taskUiStage;private bool dungeonEndChosen;
    private static string FloorLogPath{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","zindan-katlari.txt");}}
    // Every floor change is written down (room, floor, position) so a wrong stage count can be traced afterwards.
    private void LogFloor(int floor,int cx,int cy)
    {
        try
        {
            int cell=nav.Snap(cx,cy,4);int room=cell<0?-1:nav.RoomAt(cell%nav.Width,cell/nav.Width);var b=room<0?null:nav.RoomBounds(room);
            Directory.CreateDirectory(Path.GetDirectoryName(FloorLogPath));
            File.AppendAllText(FloorLogPath,DateTime.Now.ToString("HH:mm:ss")+" aşama "+taskUiStage+" · kat "+floor+" · oda "+room+(b==null?"":" ("+b[0]+","+b[1]+"-"+b[2]+","+b[3]+")")+" · konum "+cx+","+cy+Environment.NewLine);
        }
        catch{}
    }

    private sealed class Walker
    {
        public int TargetVid;public int Goal=-1;public List<int> Path;public DateTime PathAt;public long MovedAt;public float LastX,LastY;public int Stuck;
        public long SidestepUntil;public float SideX,SideY;public int Side=1;
        public void Reset(){TargetVid=0;Goal=-1;Path=null;Stuck=0;SidestepUntil=0;}
        public void Retarget(int vid,float x,float y,long now)
        {
            if(TargetVid==vid)return;
            Reset();TargetVid=vid;LastX=x;LastY=y;MovedAt=now;
        }
    }

    private void SetUiStage(int number){SetBossAttack(false);if(taskUiStage!=number)sessionDirty=true;taskUiStage=number;ApplyDungeonAttackSettings();ApplyDungeonTraversalSettings();taskStages?.Invalidate();RecordActivity("Zindan",number+". aşama başladı.");}
    private void ApplyDungeonTraversalSettings(){var s=CurrentDungeonStageSetting(taskUiStage);bool enabled=taskUiStage==8&&(s==null||s.EnableObstaclePass);wallCheck.Checked=enabled;binekWallCheck.Checked=enabled;}
    private PointF CellToMemory(int cell)
    {
        float px=(cell%nav.Width)*MinimapAtlas.UnitsPerPixel+50f,py=(cell/nav.Width)*MinimapAtlas.UnitsPerPixel+50f;
        return new PointF(px+mapOffX,mapYSign>0?py+mapOffY:mapOffY-py);
    }
    private int CellOf(float memX,float memY,out int cx,out int cy){var p=ToMapPixel(memX,memY);cx=(int)p.X;cy=(int)p.Y;return cy*(nav?.Width??0)+cx;}

    private void ConfigureSmartDungeon()
    {
        ConfigureDungeonCombatStage(false,false);
        // The path is computed on the wall map, so obstacle passing would only cut through walls.
        wallCheck.Checked=false;binekWallCheck.Checked=false;
        taskDungeonStage=SmartDungeonStage;sessionDirty=true;
    }

    private async Task RunSmartDungeonAsync(CancellationToken token)
    {
        ResetConfiguredDialogue();
        ConfigureSmartDungeon();
        var symbolPriority=new DungeonSymbolPriority();var npcTracker=new DungeonNpcTracker();bool finalNpcEngaged=false;
        var stoneRecovery=new StoneRoomRecovery();int recoveryVid=0,recoveryCell=-1;PointF recoveryPoint=PointF.Empty;long recoveryUntil=0;int healthVid=0;float? stoneHp=null;DateTime nextStoneHealth=DateTime.MinValue;
        var walker=new Walker();PathFollower stageRoute=null;int routeStage=-1;int floor=-1,candidate=-1,candidateTicks=0;bool inDungeon=false;string lastLoot=null;
        DateTime nextPulse=DateTime.MinValue,nextDialogCheck=DateTime.MinValue,nextLootClick=DateTime.MinValue,nextKellamTalk=DateTime.MinValue,nextNpcCameraScan=DateTime.MinValue,nextShamanClose=DateTime.MinValue;
        int npcCameraAttempt=0;
        if(taskUiStage<1)SetUiStage(1);
        while(true)
        {
            token.ThrowIfCancellationRequested();
            if(DateTime.UtcNow>=nextPulse){await SessionPulseAsync();nextPulse=DateTime.UtcNow.AddSeconds(1);}
            await Task.Delay(60,token);
            if(DateTime.UtcNow>=nextShamanClose){nextShamanClose=DateTime.UtcNow.AddMilliseconds(700);await CloseAccidentalCharacterWindowAsync(token);}
            if(TryRunReviveEscape()){stoneRecovery.Reset();recoveryUntil=0;walker.Reset();continue;}
            if(MountSkillBusy){ReleaseMovement();taskStatus.Text="Oto beceri: binek/beceri geçişi bekleniyor.";continue;}
            if(atlasName==null){taskStatus.Text="Harita bilgisi bekleniyor…";continue;}

            // ---- Lobby: start (or restart) the run with Kellam ----
            if(atlasName==LobbyMap)
            {
                ResetCombatFocus();
                dungeonEndChosen=false;
                if(inDungeon)
                {
                    inDungeon=false;floor=-1;finalNpcEngaged=false;ReleaseMovement();SetBossAttack(false);
                    CompleteTaskStage(Math.Max(taskUiStage,8),"Zindan tamamlandı · girişe dönüldü.");
                    if(!taskLoop.Checked){taskStatus.Text="Zindan tamamlandı · girişe dönüldü.";return;}
                    taskStagesDone.Clear();
                }
                SetUiStage(1);ReleaseMovement();SetBossAttack(false);
                await CloseAccidentalCharacterWindowAsync(token);
                await StartDungeonWithKellamAsync(token);
                taskStatus.Text="Kellam görevi onaylandı · zindan haritası yükleniyor.";
                // Entry is confirmed by the client; allow the radar/map display to catch up, with a deadline.
                var mapDisplayUntil=DateTime.UtcNow.AddSeconds(10);
                while(atlasName==LobbyMap&&DateTime.UtcNow<mapDisplayUntil){token.ThrowIfCancellationRequested();await SessionPulseAsync();await Task.Delay(250,token);}
                continue;
            }
            if(atlas==null)
            {
                // Without the floor's picture and wall map nothing can be planned: extract the maps from the game's
                // pack files once (per map name) instead of waiting forever.
                if(missingAtlas==atlasName&&mapsExtractedFor!=atlasName)
                {
                    mapsExtractedFor=atlasName;ReleaseMovement();SetBossAttack(false);
                    taskStatus.Text=atlasName+" haritasının dosyası yok · oyunun pack dosyalarından çıkarılıyor…";
                    await UpdateMapsAsync(m=>taskStatus.Text=m);continue;
                }
                taskStatus.Text=missingAtlas==atlasName?atlasName+" haritası bulunamadı ("+MapsFolder+"); 'Haritaları güncelle' düğmesine basın.":atlasName+" haritası yükleniyor…";
                continue;
            }
            if(!mapCalibrated){taskStatus.Text=atlasName+" haritası konumlandırılıyor…";continue;}
            if(navMap!=atlas.Name)
            {
                nav=new DungeonNavigator(atlas.Width,atlas.Height,atlas.IsBlocked);navMap=atlas.Name;floor=-1;
                ReleaseMovement();SetBossAttack(false);await Task.Delay(900,token);await CloseAccidentalCharacterWindowAsync(token);continue;
            }
            if(!inDungeon){inDungeon=true;CompleteTaskStage(1,"1. aşama tamamlandı · zindana girildi.");if(taskUiStage<2)SetUiStage(2);}

            float x=TaskX,y=TaskY;int cx,cy;CellOf(x,y,out cx,out cy);
            PointF clientPixel;if(TryClientMapPixel(out clientPixel)){cx=(int)clientPixel.X;cy=(int)clientPixel.Y;}
            int here=nav.FloorAt(cx,cy);
            // A floor counts only when it is read the same three times in a row (no flicker on room edges).
            if(here>=0&&here!=floor){if(here==candidate)candidateTicks++;else{candidate=here;candidateTicks=1;}
                if(candidateTicks>=3)
                {
                    if(floor>=0){CompleteTaskStage(taskUiStage,taskUiStage+". aşama tamamlandı · yeni kat.");SetUiStage(Math.Min(8,taskUiStage+1));}
                    LogFloor(here,cx,cy);
                    stoneRecovery.Reset();recoveryVid=0;recoveryUntil=0;healthVid=0;stoneHp=null;nextStoneHealth=DateTime.MinValue;
                    floor=here;finalNpcEngaged=false;nav.ResetSeen();walker.Reset();ReleaseMovement();SetBossAttack(false);
                    await Task.Delay(900,token);await CloseAccidentalCharacterWindowAsync(token);continue;
                }}
            if(floor<0){taskStatus.Text="Kat belirleniyor…";continue;}
            nav.MarkSeen(cx,cy);
            long now=movementClock.ElapsedMilliseconds;
            var radar=ReadRadarEntities();
            var hostiles=radar.Where(e=>IsDungeonCombatTarget(taskUiStage,e.Property_03CA)).ToList();
            string state="";try{if(File.Exists(DungeonStatePath))state=DungeonSymbolPriority.ReadScopedMessage(File.ReadAllText(DungeonStatePath).Trim(),taskUiStage);}catch(IOException){}
            bool previouslyBlocked=symbolPriority.BlockCombat;symbolPriority.Observe(taskUiStage,state);
            if(symbolPriority.BlockCombat){SetBossAttack(false);ResetCombatFocus();if(!previouslyBlocked){ReleaseMovement();walker.Reset();stageRoute=null;}}
            string stageText=taskUiStage+". aşama · ";
            if(routeStage!=taskUiStage)
            {
                routeStage=taskUiStage;var stageSetting=CurrentDungeonStageSetting(taskUiStage);
                stageRoute=new PathFollower(stageSetting==null?null:stageSetting.Path){Reach=110f};
            }

            // 1. Quest dialog
            if(DateTime.UtcNow>=nextDialogCheck)
            {
                nextDialogCheck=DateTime.UtcNow.AddMilliseconds(600);
                var dialog=await ReadQuestDialogAsync(token);
                if(dialog!=null){stoneRecovery.Reset();recoveryUntil=0;SetBossAttack(false);ReleaseMovement();await AnswerDungeonDialogAsync(dialog,token);continue;}
            }
            string npcName=ConfiguredDungeonNpcName(taskUiStage);
            string npcSnapshot="";DateTime npcWritten=DateTime.MinValue;
            try{if(File.Exists(DungeonNpcPath)){npcWritten=File.GetLastWriteTimeUtc(DungeonNpcPath);npcSnapshot=File.ReadAllText(DungeonNpcPath);}}catch(IOException){}
            EntityState kellam=npcTracker.Update(taskUiStage,npcSnapshot,npcWritten,DateTime.UtcNow,npcName,radar,mapOffX,mapOffY,mapYSign);
            if(kellam==null&&symbolPriority.NpcVid>0)kellam=radar.FirstOrDefault(e=>e.Property_03C9==symbolPriority.NpcVid&&e.Property_03CA==1);
            bool hasSymbol=symbolPriority.HasSymbol;
            if(symbolPriority.Delivered){SetBossAttack(false);ReleaseMovement();taskStatus.Text=stageText+"sembol NPC'ye verildi · sonraki kat bekleniyor.";continue;}
            if(kellam==null)kellam=radar.Where(e=>e.Property_03CA==1&&!string.IsNullOrWhiteSpace(e.Property_03CD)&&e.Property_03CD.IndexOf(npcName,StringComparison.OrdinalIgnoreCase)>=0).OrderBy(e=>e.Distance).FirstOrDefault();
            if(state.StartsWith("4. asama: sembol Kellama veriliyor")){SetBossAttack(false);ReleaseMovement();taskStatus.Text=stageText+"sembol Kellam'a veriliyor.";continue;}
            // 2. Symbol in the inventory: no more attacks or loot, straight to Kellam (explored for when not seen yet)
            if(hasSymbol)
            {
                SetBossAttack(false);
                if(kellam!=null)
                {
                    walker.Retarget(kellam.Property_03C9,x,y,now);
                    float d=Distance2D(x,y,kellam.X,kellam.Y);
                    if(d<=KellamStopDistance){ReleaseMovement();taskStatus.Text=stageText+"Kellam yanında, sembol veriliyor.";continue;}
                    int kx,ky;int kc=CellOf(kellam.X,kellam.Y,out kx,out ky);
                    WalkTo(walker,kc,x,y,cx,cy,now,new PointF(kellam.X,kellam.Y),KellamStopDistance);taskStatus.Text=stageText+"sembol envanterde · Kellam'a gidiliyor ("+(int)d+").";continue;
                }
                if(DateTime.UtcNow>=nextNpcCameraScan)
                {
                    nextNpcCameraScan=DateTime.UtcNow.AddSeconds(1.5);await RotateCameraForNpcSearchAsync(npcName,++npcCameraAttempt,token);continue;
                }
                if(walker.Goal<0||ReachedCell(walker.Goal,cx,cy)){walker.Reset();walker.Goal=nav.NextExploreGoal(cx,cy);}
                if(walker.Goal>=0)WalkTo(walker,walker.Goal,x,y,cx,cy,now);else ReleaseMovement();
                taskStatus.Text=stageText+"sembol envanterde · Kellam aranıyor (saldırı kapalı).";continue;
            }
            var targets=hostiles.Select(e=>new{e,cell=CellOf(e.X,e.Y,out _,out _),distance=Distance2D(x,y,e.X,e.Y)}).Where(t=>IsDungeonTargetOnFloor(nav,floor,t.cell)||t.distance<=3500f).OrderBy(t=>t.distance).ToList();
            var focused=ChooseCombatTarget(targets.Select(t=>t.e),"dungeon:"+taskUiStage+":"+floor);
            if(combatFocus.Target!=null&&!targets.Any(t=>t.e.Property_03C9==focused.Property_03C9))
                targets.Add(new{e=focused,cell=CellOf(focused.X,focused.Y,out _,out _),distance=Distance2D(x,y,focused.X,focused.Y)});
            if(focused==null)targets.Clear();
            if(targets.Count==0){stoneRecovery.Reset();recoveryVid=healthVid=0;recoveryUntil=0;stoneHp=null;if(walker.TargetVid!=0)walker.Reset();}
            // 3. Ground loot: walk onto the item on the map (its position comes from the client); the client picks it up
            if(state.StartsWith("4. asama: yerde esya:")&&ShouldCollectDungeonLoot(taskUiStage,targets.Count))
            {
                SetBossAttack(false);
                var p=state.Split(':');int idist,wx,wy,ix,iy;
                if(p.Length>=8&&int.TryParse(p[5],out idist)&&int.TryParse(p[6],out wx)&&int.TryParse(p[7],out wy))
                {
                    int itemCell=(wy/100)*nav.Width+(wx/100);
                    if(idist<=150){ReleaseMovement();walker.Reset();}
                    else if(nav.FindPath(cx,cy,wx/100,wy/100)!=null||walker.Goal==itemCell)WalkTo(walker,itemCell,x,y,cx,cy,now);
                    else if(int.TryParse(p[3],out ix)&&int.TryParse(p[4],out iy)&&(p[2]!=lastLoot||DateTime.UtcNow>=nextLootClick)){lastLoot=p[2];nextLootClick=DateTime.UtcNow.AddMilliseconds(1500);try{await ClickGameCursorAsync(ix,iy,0,token);}catch(InvalidOperationException){}}
                    taskStatus.Text=stageText+"yerdeki eşyaya gidiliyor ("+idist+").";
                }
                continue;
            }
            lastLoot=null;
            // 4. Monsters and stones of this room
            if(targets.Count>0&&!finalNpcEngaged)
            {
                var t=targets.Find(v=>v.e.Property_03C9==focused?.Property_03C9)??targets[0];
                walker.Retarget(t.e.Property_03C9,x,y,now);float d=t.distance;SetBossAttack(ShouldAttackDungeonTarget(taskUiStage,d));
                bool recoverStone=taskUiStage==3&&DungeonUsesRangedAttack(taskUiStage)&&!DungeonAttackDisabled(taskUiStage)&&t.e.Property_03CA==AddressCatalog.Field_035B;
                if(recoverStone)
                {
                    if(healthVid!=t.e.Property_03C9){healthVid=t.e.Property_03C9;stoneHp=null;nextStoneHealth=DateTime.MinValue;recoveryUntil=0;}
                    if(DateTime.UtcNow>=nextStoneHealth)
                    {
                        nextStoneHealth=DateTime.UtcNow.AddMilliseconds(600);stoneHp=await ReadStoneHealthAsync(healthVid,token);
                        now=movementClock.ElapsedMilliseconds;
                    }
                    if(stoneRecovery.Observe(true,healthVid,stoneHp,d<=DungeonHoldDistance,now)&&TryStoneRecoveryGoal(t.e,x,y,cx,cy,floor,stoneRecovery.Attempts,out recoveryCell,out recoveryPoint))
                    {
                        recoveryVid=healthVid;recoveryUntil=now+1800;walker.Reset();walker.Retarget(recoveryVid,x,y,now);ReleaseMovement();
                    }
                    if(recoveryVid==healthVid&&now<recoveryUntil)
                    {
                        if(Distance2D(x,y,recoveryPoint.X,recoveryPoint.Y)>60)
                        {
                            WalkTo(walker,recoveryCell,x,y,cx,cy,now,recoveryPoint,60f);
                            taskStatus.Text=stageText+"taşta vuruş ilerlemiyor · menzilli vuruş için yer değiştiriliyor.";continue;
                        }
                        recoveryUntil=0;ReleaseMovement();walker.Reset();
                    }
                }
                else{stoneRecovery.Reset();recoveryUntil=0;healthVid=0;stoneHp=null;}

                if(d<=DungeonHoldDistance){ReleaseMovement();walker.Reset();taskStatus.Text=stageText+targets.Count+" hedef kesiliyor ("+(int)d+") · Turbo WAIT.";continue;}
                WalkTo(walker,t.cell,x,y,cx,cy,now,new PointF(t.e.X,t.e.Y));taskStatus.Text=stageText+"hedefe yürünüyor ("+(int)d+") · "+targets.Count+" hedef.";continue;
            }
            // 5. Once the exit NPC is found, approach/talk/retry is one job. Cooldown never resumes exploration.
            var dialogueStage=CurrentDungeonStageSetting(taskUiStage);
            bool customTalk=dialogueStage!=null&&dialogueStage.CustomDialogueEnabled;
            if((customTalk||DungeonNpcTracker.ShouldTalkAtExit(taskUiStage,IsMazeFloor(floor)))&&(kellam!=null||npcTracker.NpcVid>0||finalNpcEngaged))
            {
                if(customTalk&&ConfiguredDialogue(dialogueStage).Complete){SetBossAttack(false);ReleaseMovement();taskStatus.Text=stageText+"NPC cevap sırası tamamlandı; sonraki kat bekleniyor.";continue;}
                SetBossAttack(false);stageRoute=null;
                if(!finalNpcEngaged){ReleaseMovement();walker.Reset();}
                finalNpcEngaged=true;
                ReleaseMovement();
                if(DateTime.UtcNow>=nextKellamTalk)
                {
                    nextKellamTalk=DateTime.UtcNow.AddSeconds(1);
                    taskStatus.Text=stageText+npcName+" yanında · konuşma açılıyor.";
                    try{await ClickNpcVerifiedAsync(npcName,token,null,npcTracker.NpcVid>0?npcTracker.NpcVid:(kellam?.Property_03C9??0),kellam,atlasName);}catch(TimeoutException ex){taskStatus.Text=stageText+ex.Message;}
                }
                else taskStatus.Text=stageText+npcName+" yanında · konuşma/ışınlanma bekleniyor.";
                continue;
            }
            SetBossAttack(taskUiStage!=3);
            // Follow the recorded floor route between groups. As soon as a new target appears,
            // the target branch above takes over; after that group dies, this resumes at the next waypoint.
            if(stageRoute!=null&&!stageRoute.Done&&stageRoute.Near(x,y,3500f))
            {
                var waypoint=stageRoute.Next(x,y);
                if(waypoint!=null)
                {
                    walker.Reset();SteerDungeon(x,y,waypoint.X,waypoint.Y);
                    taskStatus.Text=stageText+"sonraki slot grubuna gidiliyor · rota "+(stageRoute.Index+1)+" / "+stageRoute.Count+".";
                    continue;
                }
            }
            // 6. Explore the rest of the floor (corridor ends hold the maze portals)
            if(kellam==null&&(customTalk||DungeonNpcTracker.ShouldTalkAtExit(taskUiStage,IsMazeFloor(floor)))&&DateTime.UtcNow>=nextNpcCameraScan)
            {
                nextNpcCameraScan=DateTime.UtcNow.AddSeconds(1.5);await RotateCameraForNpcSearchAsync(npcName,++npcCameraAttempt,token);continue;
            }
            if(walker.Goal<0||ReachedCell(walker.Goal,cx,cy)){walker.Reset();walker.Goal=nav.NextExploreGoal(cx,cy);}
            if(walker.Goal>=0){WalkTo(walker,walker.Goal,x,y,cx,cy,now);taskStatus.Text=stageText+"kat keşfediliyor"+(IsMazeFloor(floor)?" (labirent, Kellam aranıyor)":"")+".";continue;}
            ReleaseMovement();taskStatus.Text=stageText+"kat temizlendi, ışınlanma bekleniyor.";
        }
    }

    // Ordinary drops never delay the next live group; the symbol floor keeps quest-loot priority.
    private static bool ShouldCollectDungeonLoot(int stage,int liveTargets){return stage==4;}
    private static bool IsDungeonCombatTarget(int stage,int type)
    {
        return type==AddressCatalog.Field_035B||(stage!=3&&type==AddressCatalog.Field_035C);
    }


    private static bool IsDungeonTargetOnFloor(DungeonNavigator navigator,int floor,int cell)
    {
        if(navigator==null||floor<0||cell<0)return false;
        int x=cell%navigator.Width,y=cell/navigator.Width,s=navigator.Snap(x,y,12);
        return s>=0&&navigator.FloorOfRoom(navigator.RoomAt(s%navigator.Width,s/navigator.Width))==floor;
    }

    private bool IsMazeFloor(int floor)
    {
        int rooms=0;for(int r=0;r<nav.RoomCount;r++)if(nav.FloorOfRoom(r)==floor)rooms++;
        return rooms>=3;
    }
    private bool ReachedCell(int cell,int cx,int cy){return Math.Abs(cell%nav.Width-cx)<=2&&Math.Abs(cell/nav.Width-cy)<=2;}

    // Follows an A* path on the wall map toward the goal cell; repaths every 2 s and sidesteps when stuck.
    private void WalkTo(Walker w,int goal,float x,float y,int cx,int cy,long now,PointF? preciseTarget=null,float? stopDistance=null)
    {
        // A new goal must not inherit a detour or stale stuck timer from the defeated target.
        if(goal!=w.Goal){w.SidestepUntil=0;w.Stuck=0;w.MovedAt=now;w.LastX=x;w.LastY=y;}
        if(now<w.SidestepUntil){SteerDungeon(x,y,w.SideX,w.SideY);return;}
        if(goal!=w.Goal||w.Path==null||DateTime.UtcNow-w.PathAt>TimeSpan.FromSeconds(2))
        {
            if(goal!=w.Goal)w.Stuck=0;
            w.Goal=goal;w.Path=nav.FindPath(cx,cy,goal%nav.Width,goal/nav.Width);w.PathAt=DateTime.UtcNow;
            if(w.Path==null){var m=CellToMemory(goal);SteerDungeon(x,y,m.X,m.Y);return;}
        }
        // Drop the part of the path already behind the character.
        int nearest=0,best=int.MaxValue;for(int i=0;i<Math.Min(w.Path.Count,40);i++){int dx=w.Path[i]%nav.Width-cx,dy=w.Path[i]/nav.Width-cy,dd=dx*dx+dy*dy;if(dd<best){best=dd;nearest=i;}}
        if(nearest>0)w.Path.RemoveRange(0,nearest);
        if(w.Path.Count<=1)
        {
            // A snapped navigation cell is not the actor's position. Finish the approach in world coordinates.
            if(preciseTarget.HasValue&&Distance2D(x,y,preciseTarget.Value.X,preciseTarget.Value.Y)>(stopDistance??DungeonHoldDistance))
                SteerDungeon(x,y,preciseTarget.Value.X,preciseTarget.Value.Y);
            else ReleaseMovement();
            return;
        }
        var target=CellToMemory(nav.LookAhead(w.Path,8));
        if(Distance2D(x,y,w.LastX,w.LastY)>40){w.LastX=x;w.LastY=y;w.MovedAt=now;}
        else if(now-w.MovedAt>2500)
        {
            w.MovedAt=now;w.Path=null;
            if(++w.Stuck>=3){w.Reset();return;}
            var side=BossStagePolicy.Ahead(x,y,NormalizeAngle(GetTargetAngle(x,y,target.X,target.Y)+90f*w.Side),300f);w.Side=-w.Side;
            w.SideX=side.X;w.SideY=side.Y;w.SidestepUntil=now+700;ReleaseMovement();return;
        }
        SteerDungeon(x,y,target.X,target.Y);
    }

    // Dungeon dialogs, answered on the dialog object once its buttons are shown (never while the text is written):
    // end of the run at Kellam: "Tozsuz Topraklar Zindanı'nı tamamladınız!" (first of two, next to "İptal") ->
    // "Devam" -> "Hayır" (back to the lobby). Captions decide; when they are missing the order does. A dialog that fits
    // none of these is closed through its own "İptal"/"Kapat" answer, so nothing unexpected gets confirmed.
    private async Task AnswerDungeonDialogAsync(QuestDialogInfo dialog,CancellationToken token)
    {
        var stage=CurrentDungeonStageSetting(taskUiStage);
        if(stage!=null&&stage.CustomDialogueEnabled){await ApplyConfiguredAnswerAsync(stage,ConfiguredDialogue(stage),dialog,token);return;}
        if(!dialog.Ready){taskStatus.Text=taskUiStage+". aşama · görev penceresi yazılıyor, bekleniyor.";return;}
        int answers=dialog.Answers.Count,index;string text;
        int finished=dialog.FindContaining("tamamla"),no=dialog.Find("hay"),close=dialog.Find("iptal","kapat","vazgeç","çık");
        if(finished>=0){index=finished;dungeonEndChosen=true;}
        else if(no>=0)index=no;
        else if(answers==0)index=-1;
        else if(answers==2&&!dungeonEndChosen){index=dialog.Answers[0].Index;dungeonEndChosen=true;}
        else if(answers==2)index=dialog.Answers[1].Index;
        else if(answers==1)index=dialog.Answers[0].Index;
        else index=close>=0?close:dialog.Answers[answers-1].Index;
        text=index<0?"Devam":dialog.CaptionOf(index);
        bool ok=await PressQuestDialogAsync(dialog,index,token);
        taskStatus.Text=taskUiStage+". aşama · görev penceresi: "+text+(ok?" seçildi.":" (pencere değişti).");
        await Task.Delay(500,token);
    }
}

