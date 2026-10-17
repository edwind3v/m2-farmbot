# Loaded with SessionAgent; all transitions are observed, never guessed from a delay.
class MountSkillCycle(object):
    def __init__(self):
        self.stage = 'idle'
        self.slot = self.vnum = 0
        self.deadline = self.after = 0
        self.error = ''
    @property
    def busy(self):
        return self.stage != 'idle'
    def cancel(self):
        # Stop must not emit an unsolicited ride command.
        self.stage = 'idle'
    def mounted(self, player):
        reader = getattr(player, 'IsMountingHorse', None)
        if not callable(reader):
            raise RuntimeError('Binek durumu API bulunamadi')
        return bool(reader())
    def start(self, slot, vnum, now, player, net):
        if self.busy:
            return False
        if not self.mounted(player):
            player.ClickSkillSlot(slot)
            return False
        self.slot, self.vnum = slot, vnum
        self.stage, self.deadline, self.after = 'dismount', now + 8, now + .4
        # The game's Ctrl+G action is the ride command; no desktop keystrokes.
        for name in ('EndKeyWalkingImmediately', 'EndMouseWalking'):
            fn = getattr(player, name, None)
            if callable(fn):fn()
        fn = getattr(player, 'SetAttackKeyState', None)
        if callable(fn):fn(0)
        net.SendChatPacket('/ride')
        return True
    def tick(self, now, player, net):
        if not self.busy:
            return 'idle'
        if now > self.deadline:
            self.error = 'Binek/beceri gecisi dogrulanamadi; tekrar komut gonderilmedi'
            self.cancel()
            return 'failed'
        if now < self.after:
            return self.stage
        mounted = self.mounted(player)
        if self.stage == 'dismount' and not mounted:
            # The slot may have changed during a map/phase transition.
            if player.GetSkillIndex(self.slot) != self.vnum:
                self.error = 'Beceri yuvasi degisti; kullanilmadi'
                self.cancel()
                return 'failed'
            if not player.IsSkillActive(self.slot) and not player.IsSkillCoolTime(self.slot):
                player.ClickSkillSlot(self.slot)
            self.stage, self.deadline, self.after = 'cast', now + 5, now + 1
        elif self.stage == 'cast' and (player.IsSkillActive(self.slot) or player.IsSkillCoolTime(self.slot)):
            if not mounted:
                net.SendChatPacket('/ride')
                self.stage, self.deadline, self.after = 'remount', now + 8, now + .4
            else:
                self.cancel()
                return 'done'
        elif self.stage == 'remount' and mounted:
            self.cancel()
            return 'done'
        return self.stage
