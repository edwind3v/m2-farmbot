# Own one queue target, not the entire player's manually populated queue.
class RouteAttackQueueDriver(object):
    def __init__(self):
        self.owned=False;self.scene=None;self.token='';self.vid=0
        self.identity=None;self.error='';self.state='idle'
        self.hold_mode=False;self.holding=False;self.reservation=False;self.refresh_at=0
    def cancel(self,player,actor,background,stop=True):
        try:
            if self.owned and stop and self.scene==(int(player.GetMainCharacterIndex()),background.GetCurrentMapName()):
                if self.holding:
                    player.SetAttackKeyState(False);self.holding=False
                player.RemoveAutoAttackTargetActorID(self.vid)
                # The engine pops an entry when it becomes the active reservation.
                # Removing the queued entry alone does NOT cancel that reservation.
                player.EndMouseWalking()
                if int(player.GetTargetVID())==self.vid:
                    player.SetAttackKeyState(False);player.ClearTarget()
        except Exception:
            self.error='Rota kuyruk hedefi birakilamadi; yeniden denenecek'
            return False
        self.owned=False;self.scene=None;self.identity=None;self.vid=0;self.state='idle';self.error=''
        self.hold_mode=self.holding=self.reservation=False
        return True
    def hold_target(self,now,request,player,actor):
        import math
        if not actor.HasInstance(self.vid) or actor.GetInstanceType(self.vid) not in (0,2) or not player.CanAttackInstance(self.vid):
            if self.holding:player.SetAttackKeyState(False);self.holding=False
            self.state='target-gone';return
        dead=getattr(actor,'IsDead',None)
        if callable(dead) and dead(self.vid):
            if self.holding:player.SetAttackKeyState(False);self.holding=False
            self.state='target-gone';return
        for name in ('SetTarget','GetCharacterDistance'):
            if not callable(getattr(player,name,None)):raise RuntimeError('Sabit saldiri API yok: '+name)
        distance=float(player.GetCharacterDistance(self.vid));maximum=float(request.get('HoldDistance',370))
        if any(math.isnan(v) or math.isinf(v) for v in (distance,maximum)) or distance<0 or not 100<=maximum<=100000:
            raise RuntimeError('Sabit saldiri mesafesi gecersiz')
        if distance>maximum:
            if self.holding:player.SetAttackKeyState(False);self.holding=False
            self.state='hold-out-of-range';return
        if self.reservation:
            # Exactly once on arrival: stop the queue's walk-to-centre
            # reservation before owning a continuous Space attack.
            player.RemoveAutoAttackTargetActorID(self.vid);player.EndMouseWalking()
            self.reservation=False
        if self.holding and int(player.GetTargetVID())!=self.vid:
            player.SetAttackKeyState(False);self.holding=False
            raise RuntimeError('Sabit saldiri hedefi elle degisti')
        if not self.holding:
            player.SetTarget(self.vid)
            if int(player.GetTargetVID())!=self.vid:raise RuntimeError('Sabit saldiri hedefi dogrulanamadi')
            self.holding=True;self.refresh_at=0
        if now>=self.refresh_at:
            player.SetAttackKeyState(True);self.refresh_at=now+.2
        self.state='target-hold';self.error=''
    def tick(self,request,scene,player,actor,background,now=0):
        identity=scene,request['Token'],int(request['TargetVid'])
        if self.identity==identity:
            if request.get('HoldAtTarget') is True:
                self.hold_mode=True
                self.hold_target(now,request,player,actor);return
            if self.hold_mode:
                if self.holding:player.SetAttackKeyState(False);self.holding=False
                self.hold_mode=False
                if not actor.HasInstance(self.vid) or not player.CanAttackInstance(self.vid):
                    self.state='target-gone';return
                self.reservation=True;player.PushAutoAttackTargetActorID(self.vid)
            # False queue membership means ACTIVE too, not dead or unqueued.
            # Never re-add a living active target on every host heartbeat.
            self.state='target-gone' if not actor.HasInstance(self.vid) else 'queue-active'
            return
        if not self.cancel(player,actor,background):return
        vid=identity[2]
        for name in ('PushAutoAttackTargetActorID','RemoveAutoAttackTargetActorID','IsInAutoAttackQueue','CanAttackInstance','EndMouseWalking','GetTargetVID','SetAttackKeyState','ClearTarget'):
            if not callable(getattr(player,name,None)):raise RuntimeError('Kuyruk API yok: '+name)
        if not actor.HasInstance(vid) or actor.GetInstanceType(vid) not in (0,2) or not player.CanAttackInstance(vid):
            raise RuntimeError('Rota kuyruk hedefi dogrulanamadi')
        if player.IsInAutoAttackQueue(vid):raise RuntimeError('Secilen hedef zaten manuel kuyrukta; sahipligi alinmadi')
        self.scene,self.token,self.vid,self.identity,self.owned=scene,request['Token'],vid,identity,True
        if request.get('HoldAtTarget') is True:
            self.hold_mode=True
            self.hold_target(now,request,player,actor);return
        self.reservation=True
        player.PushAutoAttackTargetActorID(vid)
        self.state='queue-active';self.error=''

# Player-level joystick input. Never rotate the camera or address another actor.
class RouteMotionDriver(object):
    def __init__(self):
        self.owned = False
        self.scene = None
        self.token = ''
        self.goal = None
        self.error = ''
        self.previous_position = None
        self.leg = self.arrived = None
        self.state = 'idle'
        self.world_heading = self.origin = None
        self.brake_until = 0
        self.corrections = 0
        self.attack_queue = RouteAttackQueueDriver()
        self.mount_identity = None
        self.next_remount = 0
        self.mounted_report = None
    def cancel(self, player, actor, background, stop=True):
        if not self.attack_queue.cancel(player,actor,background,stop):
            self.error=self.attack_queue.error;return False
        return self.cancel_walk(player,actor,background,stop)
    def cancel_walk(self, player, actor, background, stop=True):
        try:
            if self.owned and stop and self.scene == (int(player.GetMainCharacterIndex()), background.GetCurrentMapName()) and actor.HasInstance(self.scene[0]):
                player.SetAnalogMoveState(False, 0.0)
                end = getattr(player, 'EndKeyWalkingImmediately', None)
                if callable(end):end()
        except Exception:
            self.error = 'Rota hareketi durdurulamadi; yeniden denenecek'
            return False
        self.owned = False
        self.scene = self.goal = None
        self.previous_position = None
        self.world_heading = self.origin = None
        return True
    def reset_leg(self):
        self.leg = self.arrived = None
        self.previous_position = None
        self.world_heading = self.origin = None
        self.brake_until = 0
        self.corrections = 0
    def tick(self, now, request, pid, enabled, player, actor, background, camera=None, net=None):
        try:
            import math
            self.mounted_report = None
            valid = isinstance(request,dict) and request.get('Pid') == pid and request.get('Active') is True
            until = float(request.get('Until',0)) if valid else 0
            if not enabled or not valid or math.isnan(until) or math.isinf(until) or until <= now or until > now+2:
                self.state = 'paused' if not enabled else 'inactive-or-expired'
                if self.cancel(player,actor,background):
                    self.reset_leg();self.error=''
                    # Keep the ride cooldown across short lease/file-read gaps.
                    # A new run/death has a distinct identity and resets it.
                return
            scene = int(player.GetMainCharacterIndex()), background.GetCurrentMapName()
            if scene[0] <= 0 or request.get('Vid') != scene[0] or request.get('Map') != scene[1] or not actor.HasInstance(scene[0]):
                if self.cancel(player,actor,background):self.reset_leg()
                self.state = 'error'
                self.error = 'Rota istemcisi/karakteri/haritasi degisti'
                return
            token = request.get('Token','')
            if not isinstance(token,str) or not token:raise RuntimeError('Rota kimligi yok')
            recovery=request.get('Recovery') is True
            if recovery and int(request.get('TargetVid',0))>0:
                raise RuntimeError('Toparlanma sirasinda saldiri kuyrugu yasak')
            if request.get('RecoveryMount') is True:
                if not recovery:raise RuntimeError('Binek komutu yalnizca toparlanma icin')
                if not self.cancel(player,actor,background):return
                self.reset_leg()
                reader=getattr(player,'IsMountingHorse',None)
                if not callable(reader):raise RuntimeError('Binek durumu API yok')
                self.mounted_report=bool(reader())
                if self.mounted_report:
                    self.state='recovery-mounted';self.error='';return
                identity=scene,token,request.get('RecoveryId','')
                if identity!=self.mount_identity:
                    self.mount_identity=identity;self.next_remount=0
                self.state='recovery-remount';self.error=''
                if now>=self.next_remount:
                    # Same action as Ctrl+G, scoped to this client. Never toggle
                    # a mounted actor, and never flood an unavailable mount.
                    self.next_remount=now+8
                    if net is None or not callable(getattr(net,'SendChatPacket',None)):
                        raise RuntimeError('Ctrl+G binek API yok')
                    net.SendChatPacket('/ride')
                return
            if int(request.get('TargetVid',0))>0:
                if not self.cancel_walk(player,actor,background):return
                self.reset_leg()
                self.attack_queue.tick(request,scene,player,actor,background,now)
                self.state=self.attack_queue.state;self.error=self.attack_queue.error
                return
            if not self.attack_queue.cancel(player,actor,background):
                self.error=self.attack_queue.error;return
            x,y = float(request['X']),float(request['Y'])
            if any(math.isnan(v) or math.isinf(v) or abs(v)>100000000 for v in (x,y)):raise RuntimeError('Gecersiz rota konumu')
            # Positive Python Y, negative native Y: verified against this client's memory.
            goal = int(round(x)),int(round(-y))
            identity = scene,token,goal
            if self.owned and (scene != self.scene or token != self.token):
                if not self.cancel(player,actor,background):return
            if identity != self.leg:
                # Continuous waypoint changes must not release movement or inherit failed corrections.
                self.reset_leg();self.leg=identity;self.error=''
            if self.arrived == identity:
                self.state='arrived';return
            if not callable(getattr(player,'SetAnalogMoveState',None)):raise RuntimeError('Rota analog API yok')
            pos=player.GetMainCharacterPosition();px,py=float(pos[0]),float(pos[1])
            if any(math.isnan(v) or math.isinf(v) for v in (px,py)):raise RuntimeError('Gecersiz oyuncu konumu')
            dx,dy=goal[0]-px,goal[1]-py
            arrival=float(request.get('Arrival',20))
            if not 5<=arrival<=100000:raise RuntimeError('Gecersiz varis mesafesi')
            crossed=False
            if self.previous_position is not None:
                ax,ay=self.previous_position;sx,sy=px-ax,py-ay;length=sx*sx+sy*sy
                t=0 if length==0 else max(0,min(1,((goal[0]-ax)*sx+(goal[1]-ay)*sy)/length))
                crossed=(goal[0]-ax-t*sx)**2+(goal[1]-ay-t*sy)**2<=arrival*arrival
            self.previous_position=px,py
            if dx*dx+dy*dy<=arrival*arrival or crossed:
                if self.cancel(player,actor,background):
                    self.arrived=identity;self.state='arrived';self.error=''
                return
            if self.corrections>=3:
                self.state='error';self.error='Rota hedefinde uc duzeltmede varis yok; hareket durdu';return
            if now<self.brake_until:
                self.state='braking';return
            # Latch a world bearing for this leg. Re-aiming each frame around a short
            # destination makes an actor with gradual turning orbit instead of arriving.
            if self.origin is not None:
                ox,oy=self.origin;gx,gy=goal[0]-ox,goal[1]-oy
                if (px-ox)*gx+(py-oy)*gy>=gx*gx+gy*gy:
                    if self.cancel(player,actor,background):
                        self.brake_until=now+.12;self.corrections+=1;self.state='braking'
                    return
            camera=camera if camera is not None else globals().get('app')
            roll=float(camera.GetCamera()[2])
            if math.isnan(roll) or math.isinf(roll):raise RuntimeError('Gecersiz kamera')
            if self.world_heading is None:
                self.world_heading=math.degrees(math.atan2(-dx,-dy));self.origin=px,py
                # This client's SetRotation adds 180 degrees internally, matching
                # the joystick's screen/world convention. Align the main actor
                # ONCE per leg: gradual turning while running described a circle.
                # Revalidated in open space (581 forward, 0.29 lateral drift).
                if not callable(getattr(actor,'SelectInstance',None)) or not callable(getattr(actor,'SetRotation',None)):
                    raise RuntimeError('Rota yon hizalama API yok')
                actor.SelectInstance(scene[0]);actor.SetRotation(self.world_heading)
            # Read camera roll to preserve the world bearing, without writing the camera.
            heading=(self.world_heading+roll)%360.0
            self.scene,self.token,self.owned=scene,token,True
            player.SetAnalogMoveState(True,heading)
            self.goal=goal;self.state='moving';self.error=''
        except Exception as e:
            self.cancel(player,actor,background)
            self.state='error';self.error='Rota hareket API/konumu dogrulanamadi: '+str(e)
