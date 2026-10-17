using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    private System.Windows.Forms.Timer routePatrolTimer;
    private RoutePatrol routePatrol;
    private Label routePatrolStatus;
    private Label routeRadarStatus;
    private long routeNextScanReport;
    private NumericUpDown routeStopDistance;
    private string routeMapName="",routeRunningMap="";
    private int routeOwnerPid,routePreviousDelay,routePreviousCap;
    private int routePreviousMovementLease;
    private bool routeAttackActive,routePreviousFast,routePreviousDamagePageEnabled;
    private long routeLastProgress;
    private PointF routeLastPosition;
    private long routeNextDraw;
    private List<int> routeNavigationPath;
    private int routeNavigationGoal=-1;
    private long routeNavigationAt;
    private TargetHealthView routeTargetHealth;
    private int routePursuitVid;
    private readonly HashSet<int> routeStoneBatchVids=new HashSet<int>();
    private RoutePoint routeStoneBatchCenter;
    private bool RouteOwnsCombat { get { return isFollowingRoute&&routePatrol!=null; } }
    private bool RouteUsesAttackQueue {get{return RouteOwnsCombat&&(!activeRouteOptions.RangedWait||activeRouteOptions.HybridAttack);}}

    private void InitializeRoutePatrol()
    {
        addRouteBtn.Text="Rota kaydını başlat";
        farmRangeLabel.Text="Rota koridor yarıçapı: "+(int)routeFarmRange+" · toplam genişlik: "+(int)(routeFarmRange*2);
        farmOnRouteCheckBox.Text="Yakın hedefleri kes · rota saldırısı";
        farmOnRouteCheckBox.Width=Math.Max(390,farmOnRouteCheckBox.Width);
        int top=tabRoute.Controls.Cast<Control>().Max(c=>c.Bottom)+16;
        top=InitializeRouteCombatEditors(top);
        routePatrolStatus=new Label{Name="routePatrolStatus",Left=20,Top=top+70,Width=610,Height=78,Text="Kayıt başlat → oyunda yürü → kaydı bitir → rotayı başlat.\nNoktalar küçük ve büyük haritada görünür; rota uçlarda geri döner.",ForeColor=tabRoute.ForeColor};
        var caption=new Label{Left=20,Top=top+8,Width=265,Height=26,Text="Yakın saldırıyı açma mesafesi",ForeColor=tabRoute.ForeColor};
        routeStopDistance=new NumericUpDown{Name="routeStopDistance",Left=295,Top=top+6,Width=100,Minimum=100,Maximum=300,Increment=25,Value=250};
        var note=new Label{Left=20,Top=top+38,Width=610,Height=24,Text="A → B → A · rota koridorundaki taşları kes → ilk ayrıldığın noktaya dön → devam.",ForeColor=tabRoute.ForeColor};
        note.Width=routePatrolStatus.Width=Math.Max(280,tabRoute.ClientSize.Width-50);
        note.Anchor=routePatrolStatus.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
        tabRoute.Controls.AddRange(new Control[]{caption,routeStopDistance,note,routePatrolStatus});
        routeRadarStatus=new Label{Name="routeRadarStatus",Left=20,Top=routePatrolStatus.Bottom+8,Width=routePatrolStatus.Width,Height=66,
            Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,ForeColor=tabRoute.ForeColor,Text="Canlı radar taraması: rota başlayınca hedef sayısı, mesafe ve seçilmeme nedeni burada görünür."};
        tabRoute.Controls.Add(routeRadarStatus);
        tabRoute.AutoScroll=true;tabRoute.AutoScrollMinSize=new Size(0,routeRadarStatus.Bottom+16);
        routeTargetHealth=AddTargetHealthView(tabRoute,"routeTargetHealth");
        routePatrolTimer=new System.Windows.Forms.Timer(components){Interval=100};routePatrolTimer.Tick+=(s,e)=>RoutePatrolTick();
        InitializeRouteTools();
    }
    private void ToggleRouteRecording()
    {
        if(isRecordingRoute){StopRouteRecording(true);return;}
        if(RouteOwnsCombat||normalBotRequested||Field_02DF||taskCancellation!=null){MessageBox.Show("Rota kaydı için çalışan hareket/bot görevini durdur.");return;}
        if(!IsClientReady()||string.IsNullOrWhiteSpace(atlasName)){MessageBox.Show("Önce oyuna bağlan ve harita bilgisinin gelmesini bekle.");return;}
        if(routePoints.Count>0&&MessageBox.Show("Yeni kayıt mevcut noktaları temizleyecek. Önce kaydetmediysen rota kaybolabilir. Devam edilsin mi?","Yeni rota",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        ResetRouteExtras();routePoints.Clear();routeMapName=atlasName;routeOwnerPid=memory.Field_0197.Property_0268.Id;
        isRecordingRoute=true;addRouteBtn.Text="Kaydı bitir";RecordRoutePosition(true);routePatrolTimer.Start();RefreshRouteDrawing();
    }
    private void RecordRoutePosition(bool endpoint=false)
    {
        float x=TaskX,y=TaskY;
        if(PathFollower.ShouldRecord(routePoints.LastOrDefault(),x,y,endpoint?1:100))routePoints.Add(new RoutePoint{X=x,Y=y});
        routePatrolStatus.Text="Kayıt açık · "+routePoints.Count+" nokta · "+routeMapName+"\nOyunda yürüdükçe haritaya yeni işaretler eklenir.";
        if(routePoints.Count>=20000)StopRouteRecording(false);
    }
    private void StopRouteRecording(bool endpoint)
    {
        if(!isRecordingRoute)return;
        if(endpoint&&routePoints.Count<20000&&IsClientReady()&&atlasName==routeMapName&&memory.Field_0197.Property_0268.Id==routeOwnerPid)RecordRoutePosition(true);
        isRecordingRoute=false;addRouteBtn.Text="Rota kaydını başlat";routePatrolTimer.Stop();
        routePatrolStatus.Text="Kayıt tamamlandı · "+routePoints.Count+" nokta. Taş/canavar seçimini yapıp rotayı başlat.";RefreshRouteDrawing();
    }
    private void ToggleRoutePatrol()
    {
        if(emergencyLatched)return;
        if(RouteOwnsCombat){StopRoutePatrol("Rota kullanıcı tarafından durduruldu.");return;}
        if(npcDialogueEditorOpen||taskCancellation!=null||normalBotRequested||Field_02DF||developerWaitRunning||packEngineRunning){MessageBox.Show("Rota için çalışan görev/bot veya diğer hareket motorunu durdur.");return;}
        if(isRecordingRoute){MessageBox.Show("Önce rota kaydını bitir.");return;}
        if(!IsClientReady()||string.IsNullOrWhiteSpace(atlasName)){MessageBox.Show("Önce oyuna bağlan ve harita bilgisini bekle.");return;}
        if(IsX86ClientMode){MessageBox.Show("Bu rota saldırı bağlantısı mevcut 64 bit istemci için hazır.");return;}
        if(routePoints.Count<2){MessageBox.Show("En az iki rota noktası kaydet veya rota dosyası aç.");return;}
        if(!string.IsNullOrEmpty(routeMapName)&&routeMapName!=atlasName){MessageBox.Show("Bu rota "+routeMapName+" haritasına ait. Doğru haritaya geç.");return;}
        if(farmAlongRoute&&!routeOptions.StoneMonsterSplash&&!routeOptions.Mixed&&!RotaMetinCheck.Checked&&!RotaCanavarCheck.Checked){MessageBox.Show("Rota için Taş veya Canavar seç.");return;}
        try{PrepareRouteSafety();}catch(Exception ex){MessageBox.Show(ex.Message,"Rota başlatılamadı");return;}
        StopMapWalk("Rota takibi başladı.");
        routePreviousDelay=attackDelayBar.Value;routePreviousCap=automation.MaxTargetsPerCycle;routePreviousFast=automation.FastTiming;routePreviousDamagePageEnabled=tabDamage.Enabled;
        routeOldWall=wallCheck.Checked;routeOldMountWall=binekWallCheck.Checked;
        try {routePatrol=new RoutePatrol(routePoints,TaskX,TaskY);}
        catch(ArgumentException ex){MessageBox.Show(ex.Message,"Geçersiz rota");return;}
        routeOwnerPid=memory.Field_0197.Property_0268.Id;routeRunningMap=atlasName;
        routeOwnerVid=memory.ReadInt32(AddressCatalog.Property_037A+AddressCatalog.Field_0377);
        if(routeOwnerVid<=0){routePatrol=null;MessageBox.Show("Karakter kimliği okunamadı; rota başlatılmadı.");return;}
        routeMotionToken=Guid.NewGuid().ToString("N");routeMotionRequested=false;
        ReleaseMovement();
        routePreviousMovementLease=movementOutput.LeaseMilliseconds;movementOutput.LeaseMilliseconds=600;movementExpired=false;
        ResetCombatFocus();isFollowingRoute=true;routeAttackActive=false;routePursuitVid=0;sessionDirty=true;
        routeStoneBatchVids.Clear();routeStoneBatchCenter=null;
        ResetRouteAutonomy();
        routeCollisionHoldVid=routeStationaryTargetVid=0;
        routeResumeAfter=0;routeDeathRecovery=false;reviveRouteWaypoint=null;reviveEscape.Reset();reviveRecoveryPid=routeOwnerPid;
        routeNavigationPath=null;routeNavigationGoal=-1;
        dmgStartCheck.Checked=false;attackDelayBar.Value=Math.Max(attackDelayBar.Minimum,Math.Min(attackDelayBar.Maximum,RouteAttackInterval));SetAttackEditorsEnabled(false);
        SetRouteEditingEnabled(false);startRouteMoveBtn.Text="Rotayı durdur";
        wallCheck.Checked=activeRouteOptions.ObstaclePass;binekWallCheck.Checked=activeRouteOptions.MountedObstaclePass;
        routeLastPosition=new PointF(TaskX,TaskY);routeLastProgress=movementClock.ElapsedMilliseconds;
        ApplyRouteAttackRuntime();routePatrolTimer.Start();RefreshRouteDrawing();
        WriteRouteIntent(true);
        routeNextScanReport=0;routeRadarStatus.Text=farmAlongRoute?"Rota koridoru hazır · yarıçap "+(int)routeFarmRange+" · toplam genişlik "+(int)(routeFarmRange*2)+" · çizgi boyunca taranıyor":"Yalnız yürüyüş: Yakın hedefleri kes kapalı; taş/canavar seçilmez.";
        routePatrolStatus.Text="V34 · rota koridor taraması · yarıçap "+(int)routeFarmRange+" · toplam genişlik "+(int)(routeFarmRange*2)+" · "+routeRunningMap;
        RecordActivity("Rota sürüm","V34 · "+Application.ExecutablePath+" · hedef takibi="+farmAlongRoute+" · taş="+RouteStones+" · canavar="+RouteMonsters+" · koridor yarıçapı="+routeFarmRange+" · toplam genişlik="+(routeFarmRange*2)+" · seçili hedef="+activeRouteOptions.TargetedAttack+" · menzilli="+activeRouteOptions.RangedWait+" · pickup="+activeRouteOptions.RangePickup+" · vuruş menzili="+activeRouteOptions.AttackRange+" · taş grubu="+activeRouteOptions.FinishNearbyStones+" / "+activeRouteOptions.StoneBatchRange);
    }
    private void SetRouteEditingEnabled(bool value)
    {
        foreach(Control c in new Control[]{addRouteBtn,clearRouteBtn,loadRouteBtn,RotaMetinCheck,RotaCanavarCheck,farmOnRouteCheckBox,farmRangeTrackBar,routeStopDistance})c.Enabled=value;
        if(routeToolsButton!=null)routeToolsButton.Enabled=value;
        if(routeCombatPanel!=null)routeCombatPanel.Enabled=value;
    }
    private void ApplyRouteAttackRuntime()
    {
        if(!RouteOwnsCombat||automation==null)return;
        automation.AttackEnabled=false;
        automation.WaitDamage=!activeRouteOptions.TargetedAttack&&!activeRouteOptions.RangedWait;automation.WaitDamageRange=!activeRouteOptions.TargetedAttack&&activeRouteOptions.RangedWait;
        automation.TargetDamage=activeRouteOptions.TargetedAttack&&!activeRouteOptions.RangedWait;automation.TargetDamageRange=activeRouteOptions.TargetedAttack&&activeRouteOptions.RangedWait;
        automation.WaitArrow=automation.TargetArrow=false;
        automation.GroundStoneWait=RouteStones&&!activeRouteOptions.RangedWait&&!activeRouteOptions.TargetedAttack;automation.IncludeStones=RouteStones;
        automation.IncludeMonsters=RouteMonsters||activeRouteOptions.StoneMonsterSplash;automation.IncludePlayers=false;
        automation.FocusStoneMonsterSplash=activeRouteOptions.StoneMonsterSplash;
        automation.AttackDelay=RouteAttackInterval;automation.AttackRange=activeRouteOptions.AttackRange;automation.MaxTargetsPerCycle=activeRouteOptions.StoneMonsterSplash?8:1;automation.FastTiming=true;
        automation.AttackEnabled=routeAttackActive&&farmAlongRoute&&combatFocus.Target!=null&&!reviveEscape.BlockAttack;
        attackDelayLbl.Text="Rota "+(activeRouteOptions.TargetedAttack?"seçili hedef":"çevre")+" vuruşu: "+RouteAttackInterval+" ms";
        if(automation.AttackEnabled||automation.AutoPickup)automation.Start();
    }
    private void SetRouteAttack(bool enabled)
    {
        routeAttackActive=enabled;if(dmgStartCheck.Checked!=enabled)dmgStartCheck.Checked=enabled;ApplyRouteAttackRuntime();PublishHybridAttackIntent();
    }
    private void StopRoutePatrol(string reason)
    {
        if(!RouteOwnsCombat)return;
        try{WriteRouteIntent(false);}catch{}
        routeAttackActive=false;automation.AttackEnabled=false;
        try{SetRouteAttack(false);}catch{}
        automation.FocusStoneMonsterSplash=false;
        routeDeathRecovery=false;reviveRouteWaypoint=null;routeResumeAfter=0;
        // A disconnected client may reject release writes; cleanup must still finish.
        try{ReleaseMovement();}catch{}
        movementOutput.LeaseMilliseconds=routePreviousMovementLease;
        try{ResetCombatFocus();}catch{}
        routeStoneBatchVids.Clear();routeStoneBatchCenter=null;
        ResetRouteAutonomy();
        isFollowingRoute=false;routePatrol=null;routeRunGeneration++;routePatrolTimer.Stop();SetRouteEditingEnabled(true);SetAttackEditorsEnabled(routePreviousDamagePageEnabled);
        routeCollisionHoldVid=routeStationaryTargetVid=0;
        wallCheck.Checked=routeOldWall;binekWallCheck.Checked=routeOldMountWall;
        ApplyCollisionFlags();
        attackDelayBar.Value=routePreviousDelay;OnAttackDelayChanged(null,null);
        automation.WaitDamage=waitDmgCheck.Checked;automation.WaitDamageRange=waitDmgRangeCheck.Checked;automation.TargetDamage=targetDmgCheck.Checked;automation.TargetDamageRange=targetDmgRangeCheck.Checked;automation.WaitArrow=waitArrowCheck.Checked;automation.TargetArrow=targetArrowCheck.Checked;
        automation.AttackRange=attackRangeBar.Value;automation.MaxTargetsPerCycle=routePreviousCap;automation.FastTiming=routePreviousFast;ApplyTargetFilters();OnAutomationChanged(null,null);
        startRouteMoveBtn.Text="Rotayı başlat";routePatrolStatus.Text=reason;sessionDirty=true;RefreshRouteDrawing();
        routeRadarStatus.Text="Radar taraması durdu.";
    }
    private void RoutePatrolTick()
    {
        try
        {
            if(!isRecordingRoute&&!RouteOwnsCombat){routePatrolTimer.Stop();return;}
            if(emergencyLatched){StopRoutePatrol("Acil durdurma: rota kapatıldı.");return;}
            if(RouteOwnsCombat)WriteRouteIntent(true);
            if(!IsClientReady()||memory.Field_0197.Property_0268.Id!=routeOwnerPid||atlasName!=(isRecordingRoute?routeMapName:routeRunningMap))
            {if(isRecordingRoute)StopRouteRecording(false);else PauseRoutePatrol("Bağlantı veya rota haritası bekleniyor; doğru istemci/harita gelince devam eder.",0);return;}
            if(isRecordingRoute){RecordRoutePosition();RefreshRouteDrawing();return;}
            if(movementClock.ElapsedMilliseconds<routeResumeAfter)return;
            RefreshReviveRecovery();
            if(MountSkillBusy){SetRouteAttack(false);ReleaseMovement();routeLastProgress=movementClock.ElapsedMilliseconds;routeLastPosition=new PointF(TaskX,TaskY);return;}
            if(movementExpired){movementExpired=false;PauseRoutePatrol("Hareket yanıtı gecikti; otomatik yeniden denenecek.");return;}
            if(HandleRouteDeathRecovery())return;
            float x=TaskX,y=TaskY;long now=movementClock.ElapsedMilliseconds;
            UpdateRouteTraversal(now);
            if(activeRouteOptions.Forbidden(x,y,100)){PauseRoutePatrol("Yasak alan sınırı: güvenli konuma çıkılması bekleniyor.",0);return;}
            if(HandleRouteHealth(x,y,now)){RefreshRouteDrawing();return;}
            EntityState target=null;
            var live=new List<EntityState>();
            if(farmAlongRoute&&!routePatrol.Returning)
            {
                var radar=ReadRadarEntities();
                live=radar.Where(t=>RouteAttackPolicy.CanPursue(t.Property_03CA,RouteStones,RouteMonsters,activeRouteOptions.StoneMonsterSplash)).ToList();
                var locked=live.FirstOrDefault(t=>t.Property_03C9==combatFocus.Target?.Property_03C9);
                if(locked!=null&&activeRouteOptions.Forbidden(locked.X,locked.Y,100)){PauseRoutePatrol("Kilitli hedef yasak alanda; güvenli konuma gelmesi bekleniyor.",0);return;}
                IEnumerable<EntityState> candidates=RouteCandidates(live,x,y,now).ToList();
                if(now>=routeNextScanReport){
                    routeNextScanReport=now+500;
                    routeRadarStatus.Text=DescribeRouteRadar(radar,candidates.ToList(),x,y);
                }
                var areaCandidates=candidates.ToList();
                // Lock living actors. Only confirmed completion can advance the batch.
                int lockedVid=combatFocus.Target?.Property_03C9??0;
                if(routePatrol.Detouring)candidates=lockedVid>0?
                    areaCandidates.Where(t=>t.Property_03C9==lockedVid):
                    RouteStoneBatchCandidates(areaCandidates);
                target=ChooseCombatTarget(candidates,"route",live,true);
                if(target==null&&routePatrol.Detouring&&activeRouteOptions.FinishNearbyStones&&routeStoneBatchCenter!=null){
                    routeStoneBatchVids.Remove(routePursuitVid);
                    target=ChooseCombatTarget(RouteStoneBatchCandidates(areaCandidates),"route",live,true);
                }
                if(target!=null&&!routePatrol.Detouring&&activeRouteOptions.FinishNearbyStones&&target.Property_03CA==AddressCatalog.Field_035B){
                    routeStoneBatchCenter=new RoutePoint{X=target.X,Y=target.Y};routeStoneBatchVids.Clear();
                    foreach(var item in areaCandidates.Where(t=>t.Property_03CA==AddressCatalog.Field_035B&&RoutePatrol.Distance(t.X,t.Y,target.X,target.Y)<=activeRouteOptions.StoneBatchRange&&!automation.IsConfirmedDeadTarget(t.Property_03C9,t.Property_03CA)))
                        routeStoneBatchVids.Add(item.Property_03C9);
                    routeStoneBatchVids.Add(target.Property_03C9);
                }
            }
            else routeRadarStatus.Text=!farmAlongRoute?"Yalnız yürüyüş: Yakın hedefleri kes kapalı; taş/canavar seçilmez.":"Ayrıldığın rota noktasına dönülüyor; dönüş bitince radar taraması devam eder.";
            if(target!=null)
            {
                if(!live.Any(t=>t.Property_03C9==target.Property_03C9)||automation.IsConfirmedDeadTarget(target.Property_03C9,target.Property_03CA)){
                    SetRouteAttack(false);ReleaseMovement();routeLastProgress=now;routeLastPosition=new PointF(x,y);
                    ResetRouteAutonomy(false);
                    routePatrolStatus.Text="Hedefin ölüm/kayıp doğrulaması bekleniyor; eski konum etrafında yürünmüyor.";return;
                }
                if(!routePatrol.Detouring||routePursuitVid!=target.Property_03C9){routePursuitVid=target.Property_03C9;routePatrol.BeginDetour(x,y);routeNavigationPath=null;routeNavigationGoal=-1;routeLastPosition=new PointF(x,y);routeLastProgress=now;TrackRouteStone(target);RecordActivity("Rota hedef",(target.Property_03CD??"")+" VID "+target.Property_03C9+" koridordan seçildi · çizgiye "+(int)routePatrol.DistanceToLine(target.X,target.Y)+" / "+(int)routeFarmRange+" · karaktere "+(int)RoutePatrol.Distance(x,y,target.X,target.Y));}
                if(routePatrol.DistanceToLine(target.X,target.Y)>routeFarmRange*2)
                {PauseRoutePatrol("Hedef rota sınırının dışında; geri gelmesi bekleniyor.",0);return;}
                float close=activeRouteOptions.RangedWait&&!HybridWaitRequested?Math.Max(100,activeRouteOptions.AttackRange*.72f):Math.Min((float)routeStopDistance.Value,activeRouteOptions.AttackRange);
                if(HybridWaitRequested&&!RouteUsesAttackQueue)close=Math.Min(close,HybridCloseDistance);
                bool stationary=UpdateRouteContact(target,x,y,close);
                if(HandleRouteStoneWatch(target,x,y,close,stationary,now)){RefreshRouteDrawing();return;}
                // The game's auto-attack queue still applies its own collision/pathing
                // while it approaches a target.  That fights the explicit obstacle-pass
                // setting and produces tiny stop/start steps at walls.  With traversal
                // enabled, keep the verified route joystick as the sole movement owner
                // until contact, then hand over to the queue only for stationary attack.
                bool directObstacleApproach=RouteUsesAttackQueue&&routeIgnoreTerrain&&!stationary;
                if(RouteUsesAttackQueue){
                    if(!RouteSegmentSafe(x,y,target.X,target.Y)){PauseRoutePatrol("Hedef yolu yasak alanı kesiyor; güvenli yol bekleniyor.",0);return;}
                    if(directObstacleApproach)RouteMoveTo(x,y,target.X,target.Y,now);
                    else RequestRouteAttackQueue(target.Property_03C9,stationary,close+120);
                }
                if(stationary&&RouteSegmentSafe(x,y,target.X,target.Y))
                {
                    if(!RouteUsesAttackQueue)ReleaseMovement();
                    SetRouteAttack(RoutePatrol.Distance(x,y,target.X,target.Y)<=activeRouteOptions.AttackRange);routeLastProgress=now;routeLastPosition=new PointF(x,y);
                    routePatrolStatus.Text="Taş/hedef "+target.Property_03C9+" bitene kadar sabit saldırı · "+RouteAttackInterval+" ms"+(RouteUsesAttackQueue?" · Space basılı, kuyruk yürüyüşü kapalı":"")+(activeRouteOptions.TargetedAttack?" · seçili hedef":" · Wait")+(activeRouteOptions.StoneMonsterSplash?" · çevre canavarlara Wait":"")+".\n"+(routeStoneBatchCenter!=null?"Grup bitince rotaya dönülür · kalan kayıtlı taş "+routeStoneBatchVids.Count:"Bitince ayrıldığın noktaya dönüp aynı işaretlerden devam eder.");
                }
                else{
                    SetRouteAttack(false);
                    if(RouteUsesAttackQueue&&!directObstacleApproach)CheckRouteProgress(x,y,now);else if(!directObstacleApproach)RouteMoveTo(x,y,target.X,target.Y,now);
                    routePatrolStatus.Text="Rota koridorundaki "+(target.Property_03CA==AddressCatalog.Field_035B?"taşa":"canavara")+" gidiliyor · çizgiye "+(int)routePatrol.DistanceToLine(target.X,target.Y)+" / "+(int)routeFarmRange+" · bitince ayrılma noktasına dönüş.\n"+(directObstacleApproach?"Engel geçişli doğrudan yürüyüş.":RouteUsesAttackQueue?"Oyun kuyruğu yürüyüşü yönetiyor.":"Saldırı bekliyor.");
                }
            }
            else
            {
                SetRouteAttack(false);
                ResetRouteContact();
                ResetRouteAutonomy(false);
                routePursuitVid=0;
                if(routePatrol.Detouring){routeStoneBatchVids.Clear();routeStoneBatchCenter=null;routePatrol.EndDetour();routeNavigationPath=null;routeLastProgress=now;routeLastPosition=new PointF(x,y);}
                var point=routePatrol.Aim(x,y);
                RouteMoveTo(x,y,point.X,point.Y,now);
                routePatrolStatus.Text=routePatrol.Returning?"Taş/grup bitti · ilk ayrıldığın rota noktasına dönülüyor.":"İşaret "+(routePatrol.Index+1)+" / "+routePatrol.Points.Count+" · "+(routePatrol.Direction>0?"A → B":"B → A")+" · rota koridoru ±"+(int)routeFarmRange+" taranıyor";
            }
            RefreshRouteDrawing();
        }
        catch(Exception ex){if(isRecordingRoute)StopRouteRecording(false);else PauseRoutePatrol("Otomatik tekrar denenecek: "+ex.Message);}
    }
    private void RouteMoveTo(float x,float y,float tx,float ty,long now)
    {
        CheckRouteProgress(x,y,now);
        if(activeRouteOptions.Forbidden(tx,ty,100))throw new InvalidOperationException("Hedef nokta yasak alan içinde.");
        // Recorded coordinates are the path. Pack-grid calibration cannot veto
        // these live points or invent a different path. Actual collision stays in the game.
        // Only a forbidden area explicitly drawn by the user can veto a segment.
        if(!RouteSegmentSafe(x,y,tx,ty))throw new InvalidOperationException("Kayıtlı yürüyüş çizgisi işaretlediğin yasak alanı kesiyor.");
        routeLastSteer=new PointF(tx,ty);
        Method_02AF(0);SteerRouteContinuously(x,y,tx,ty);
    }
    private IEnumerable<EntityState> RouteStoneBatchCandidates(IEnumerable<EntityState> area)
    {
        if(!activeRouteOptions.FinishNearbyStones||routeStoneBatchCenter==null)return Enumerable.Empty<EntityState>();
        return area.Where(t=>routeStoneBatchVids.Contains(t.Property_03C9)&&t.Property_03CA==AddressCatalog.Field_035B&&
            RoutePatrol.Distance(t.X,t.Y,routeStoneBatchCenter.X,routeStoneBatchCenter.Y)<=activeRouteOptions.StoneBatchRange&&
            !automation.IsConfirmedDeadTarget(t.Property_03C9,t.Property_03CA));
    }
    private void CheckRouteProgress(float x,float y,long now)
    {
        if(RoutePatrol.Distance(x,y,routeLastPosition.X,routeLastPosition.Y)>=25){routeLastPosition=new PointF(x,y);routeLastProgress=now;}
        if(now-routeLastProgress>12000)throw new InvalidOperationException("12 saniyedir ilerleme yok. Oyun içindeki gerçek engeli veya kayıtlı işareti kontrol et.");
    }
    private void SteerRouteContinuously(float x,float y,float tx,float ty){
        if(emergencyLatched||MountSkillBusy){ReleaseMovement();return;}
        RequestRouteEngineMotion(tx,ty);
    }
    private void RefreshRouteDrawing(){bigMapCanvas?.Invalidate();long now=movementClock.ElapsedMilliseconds;if(now<routeNextDraw)return;routeNextDraw=now+250;if(radarEntities!=null)RenderRadar(radarEntities);}
    private void PaintRecordedRoute(Graphics g,Func<PointF,PointF> memoryToScreen,bool labels)
    {
        PaintRouteSafety(g,memoryToScreen);
        PaintRouteCheck(g,memoryToScreen);
        if(routePoints.Count==0||(!string.IsNullOrEmpty(routeMapName)&&routeMapName!=atlasName))return;
        var points=routePoints.Select(p=>memoryToScreen(new PointF(p.X,p.Y))).ToArray();
        if(farmAlongRoute&&points.Length>1&&routeFarmRange>0)
        {
            var origin=memoryToScreen(new PointF(routePoints[0].X,routePoints[0].Y));
            var edge=memoryToScreen(new PointF(routePoints[0].X+routeFarmRange,routePoints[0].Y));
            float radius=(float)RoutePatrol.Distance(origin.X,origin.Y,edge.X,edge.Y);
            if(radius>.25f)using(var corridor=new Pen(Color.FromArgb(42,56,189,248),Math.Max(2f,radius*2f)))
            {
                corridor.StartCap=System.Drawing.Drawing2D.LineCap.Round;corridor.EndCap=System.Drawing.Drawing2D.LineCap.Round;corridor.LineJoin=System.Drawing.Drawing2D.LineJoin.Round;
                g.DrawLines(corridor,points);
            }
        }
        using(var pen=new Pen(Color.FromArgb(220,245,173,49),2f))if(points.Length>1)g.DrawLines(pen,points);
        using(var fill=new SolidBrush(Color.FromArgb(255,207,78)))using(var active=new SolidBrush(Color.Cyan))using(var font=new Font("Segoe UI",8f))
            for(int i=0;i<points.Length;i++)
            {
                var p=points[i];bool current=RouteOwnsCombat&&routePatrol.Index==i;float r=current?6:4;
                g.FillEllipse(current?active:fill,p.X-r,p.Y-r,r*2,r*2);
                if(labels&&(points.Length<=60||i==0||i==points.Length-1||current||i%10==0))g.DrawString((i+1).ToString(),font,fill,p.X+4,p.Y+3);
            }
        if(RouteOwnsCombat&&routePatrol.ReturnPoint!=null)
        {var p=memoryToScreen(new PointF(routePatrol.ReturnPoint.X,routePatrol.ReturnPoint.Y));using(var pen=new Pen(Color.Cyan,2))g.DrawRectangle(pen,p.X-6,p.Y-6,12,12);}
    }
}
