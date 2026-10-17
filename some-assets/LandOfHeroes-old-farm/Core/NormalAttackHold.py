# Normal game combat, equivalent to holding Space. Never opens NPCs or emits ComboAttack pulses.
class NormalAttackHold(object):
    def __init__(self):
        self.vid = 0
        self.identity = None
        self.scene = None
        self.owned = False
        self.refresh_at = 0
        self.error = ''
    def release(self, player):
        if not self.owned:
            return True
        try:
            player.SetAttackKeyState(0)
        except Exception:
            self.error = 'Normal saldiri birakilamadi; yeniden denenecek'
            return False
        self.owned = False
        self.vid = 0
        self.identity = self.scene = None
        return True
    def actor(self, vid, player, actor, allowed, maximum):
        if not actor.HasInstance(vid) or vid == player.GetMainCharacterIndex():
            return None
        kind = int(actor.GetInstanceType(vid))
        if kind not in allowed:
            return None
        dead = getattr(actor, 'IsDead', None)
        if callable(dead) and dead(vid):
            return None
        can_attack = getattr(player, 'CanAttackInstance', None)
        if callable(can_attack) and not can_attack(vid):
            return None
        distance = float(player.GetCharacterDistance(vid))
        import math
        if math.isnan(distance) or math.isinf(distance) or distance < 0 or distance > maximum:
            return None
        race = None
        try:race = int(actor.GetVirtualNumber(vid))
        except Exception:pass
        name = None
        try:name = actor.GetNameByVID(vid)
        except Exception:pass
        if race is None and name is None:
            return None
        return kind, race, name
    def tick(self, now, enabled, candidates, scene, player, actor, allowed=(0,2), close=140):
        if not enabled:
            self.release(player)
            return
        try:
            for name in ('SetAttackKeyState','GetTargetVID','SetTarget','GetCharacterDistance'):
                if not callable(getattr(player,name,None)):
                    raise RuntimeError('Normal saldiri API bulunamadi')
            if self.owned:
                identity = self.actor(self.vid,player,actor,allowed,close+50)
                if self.vid not in candidates or scene != self.scene or identity != self.identity or int(player.GetTargetVID()) != self.vid:
                    self.release(player)
                    return
                if now >= self.refresh_at:
                    player.SetAttackKeyState(1)
                    self.refresh_at = now + .2
                self.error = ''
                return
            for vid in candidates:
                identity = self.actor(vid,player,actor,allowed,close)
                if identity is None:
                    continue
                player.SetTarget(vid)
                if int(player.GetTargetVID()) != vid:
                    raise RuntimeError('Normal saldiri hedefi dogrulanamadi')
                self.vid, self.identity, self.scene = vid, identity, scene
                # Own before the write: a partial API failure still requires a release attempt.
                self.owned = True
                player.SetAttackKeyState(1)
                self.error = ''
                self.refresh_at = now + .2
                return
        except Exception:
            self.release(player)
            self.error = 'Normal saldiri kimligi/API dogrulanamadi'
