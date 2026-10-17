# Executed only by the game interpreter on its main thread.
# __ONUR_CONFIG__ is supplied in memory; no credentials are written to this source or status file.
# BEGIN SHAMAN CLOSE HELPER
def _onur_shaman_close(phase):
    # Game UI only: every map, behind other windows, without desktop focus.
    def text(value):
        try:return str(value).lower().replace('\u015f','s').replace('\u0131','i').replace('i\u0307','i')
        except Exception:return ''
    def shown(window):
        try:return bool(window.IsShow())
        except Exception:return False
    def named(key):
        return any(word in key for word in ('buffnpcwindow','shaman','saman','buffbot','buffwindow','buffi'))
    def title(window):
        result=[]
        bar=getattr(window,'titleBar',None)
        for obj in (window,bar,getattr(window,'titleName',None),getattr(window,'TitleName',None),getattr(bar,'textLine',None),getattr(bar,'titleName',None)):
            for name in ('GetTitleName','GetTitle','GetText'):
                try:result.append(text(getattr(obj,name)()))
                except Exception:pass
        for name in ('TitleName','titleName','Title','title'):
            try:result.append(text(window.GetChild(name).GetText()))
            except Exception:pass
        return ' '.join(result)
    roots=[('phase',phase,0),('interface',getattr(phase,'interface',None),0)]
    seen=set();closed=remaining=cursor=0
    # Breadth first: actual panels before their hundreds of nested UI children.
    while cursor<len(roots) and len(seen)<512:
        path,window,depth=roots[cursor];cursor+=1
        if window is None or id(window) in seen:continue
        seen.add(id(window))
        key=text(path.rsplit('.',1)[-1]+' '+type(window).__name__)
        caption=title(window) if shown(window) else ''
        target=named(key) or 'saman' in caption or 'shaman' in caption
        if target and shown(window) and hasattr(window,'Hide'):
            try:large=int(window.GetWidth())>=200 and int(window.GetHeight())>=120
            except Exception:large=True
            if large:
                bar=getattr(window,'titleBar',None)
                buttons=[getattr(window,'closeButton',None),getattr(window,'btnClose',None),getattr(bar,'btnClose',None)]
                for name in ('CloseButton','close_button'):
                    try:buttons.append(window.GetChild(name))
                    except Exception:pass
                for button in buttons:
                    if not shown(window):break
                    try:button.CallEvent()
                    except Exception:pass
                for name in ('Close','Hide'):
                    if not shown(window):break
                    try:getattr(window,name)()
                    except Exception:pass
                if shown(window):remaining+=1
                else:closed+=1
                continue
        if depth>=3:continue
        try:names=sorted(dir(window),key=lambda name:0 if named(text(name)) else 1)
        except Exception:continue
        for name in names:
            if name.startswith('_'):continue
            try:child=getattr(window,name)
            except Exception:continue
            hint=text(name+' '+type(child).__name__)
            if any(word in hint for word in ('interface','shaman','saman','support','buff','character','information','details','window','dialog')):
                if isinstance(child,dict):
                    for index,value in enumerate(list(child.values())[:64]):roots.append((path+'.'+name+'.'+str(index),value,depth+1))
                elif isinstance(child,(list,tuple)):
                    for index,value in enumerate(child[:64]):roots.append((path+'.'+name+'.'+str(index),value,depth+1))
                elif hasattr(child,'IsShow') or hasattr(child,'__dict__'):roots.append((path+'.'+name,child,depth+1))
    return closed,remaining
# END SHAMAN CLOSE HELPER

# BEGIN NPC IDENTITY HELPER
def _onur_npc_matches(vid, wanted):
    import chr
    def norm(value):
        try:
            if isinstance(value, bytes):value=value.decode('utf-8')
            return value.lower().strip().replace(u'\u015f','s').replace(u'\u0131','i').replace(u'i\u0307','i')
        except Exception:return ''
    try:
        if not chr.HasInstance(vid) or chr.GetInstanceType(vid)!=1:return False
        name=norm(chr.GetNameByVID(vid));wanted=norm(wanted)
        if not wanted:return False
        proto='';event_type=None;talk=None
        try:
            import nonplayer
            event_type=nonplayer.GetEventTypeByVID(vid)
            vehicle=getattr(nonplayer,'ON_CLICK_EVENT_VEHICLE',None)
            if vehicle is not None and event_type==vehicle:return False
            talk=getattr(nonplayer,'ON_CLICK_EVENT_TALK',None)
        except Exception:pass
        try:proto=norm(nonplayer.GetMonsterName(chr.GetVirtualNumber(vid)))
        except Exception:pass
        # Renamed pets/mounts cannot inherit an NPC's identity through their display name.
        if proto and proto!=wanted:
            return talk is not None and event_type==talk and name==wanted
        return name==wanted or (proto==wanted and wanted in name)
    except Exception:return False
# END NPC IDENTITY HELPER

def _onur_install(config):
    import sys, time, ui, player, net, chr, app, event, item
    try:
        import textTail, background, grp, wndMgr
    except Exception:
        textTail = background = grp = wndMgr = None
    opener = globals().get('old_open', open)
    stream = None
    frame = sys._getframe()
    for unused in range(64):
        if frame is None:
            break
        candidate = frame.f_locals.get('mainStream')
        if candidate is not None and hasattr(candidate, 'curPhaseWindow'):
            stream = candidate
            break
        frame = frame.f_back
    del frame
    existing = globals().get('_onur_agent')
    carried_dead = bool(getattr(existing, "was_dead", False))
    carried_revives = int(getattr(existing, "revive_serial", 0))
    carried = None
    carried_window = None
    carried_mount = getattr(existing, 'mount_cycle', None)
    carried_skills = dict(getattr(existing, 'next_skill', {}))
    carried_ai_pause = bool(getattr(existing, 'mount_ai_paused', False))
    carried_normal = getattr(existing, 'normal_hold', None)
    carried_route = getattr(existing, 'route_motion', None)
    if carried_route is not None and (not hasattr(carried_route,'mounted_report') or not hasattr(getattr(carried_route,'attack_queue',None),'hold_mode')):
        # Upgrade legacy drivers without reusing their obsolete destination API.
        upgraded_route=RouteMotionDriver()
        upgraded_route.owned=bool(getattr(carried_route,'owned',False))
        upgraded_route.scene=getattr(carried_route,'scene',None)
        old_queue=getattr(carried_route,'attack_queue',None)
        if old_queue is not None:
            for name in ('owned','scene','token','vid','identity'):
                setattr(upgraded_route.attack_queue,name,getattr(old_queue,name,getattr(upgraded_route.attack_queue,name)))
            upgraded_route.attack_queue.reservation=bool(getattr(old_queue,'owned',False))
        upgraded_route.cancel(player,chr,background)
        carried_route=upgraded_route
    if existing is not None:
        existing.normal_hold = NormalAttackHold()
        existing.route_motion = RouteMotionDriver()
        existing.mount_cycle = MountSkillCycle()
        existing.mount_ai_paused = False
        carried_window = getattr(existing, 'game_window', None)
        try:
            if getattr(existing, 'dungeon_stage', 0) in (4, 9):
                carried = set(existing.dungeon_inventory_baseline)
        except Exception:
            carried = None
        try:
            existing.config = dict(existing.config, login=False, enter=False, revive=False, cloak=False, items={}, skills=[], close_shaman=False, dungeon_auto=False, dungeon_stage=0, life_state=None, retreat_guard=None, hybrid_attack=False, attack_speed=False, move_speed=False, combat_focus_request=None, combat_focus_state=None, target_health=None)
            # An older agent's restore_speeds still calls the invalid client exports.
            existing.config['mount_skill_state']=None
            existing.config['skill_mount']=False
            existing.config['pack_ai']=False
            existing.config['pack_stone_attack']=False
            existing.config['route_motion']=None
            existing.config['route_motion_status']=None
            existing.config['route_revive']=None
            existing.attack_override = existing.move_override = False
            for restore_name in ('restore_visual_settings', 'restore_camera'):
                restore = getattr(existing, restore_name, None)
                if restore is not None:
                    restore()
            existing.Hide()
        except Exception:
            pass
        try:
            del globals()['_onur_agent']
        except Exception:
            pass
        existing = None
    if stream is None:
        raise RuntimeError('Oyun oturumu bulunamadi')

    class OnurAgent(ui.Window):
        version = 2026101404
        def __init__(self):
            ui.Window.__init__(self)
            self.was_dead = carried_dead
            self.revive_serial = carried_revives
            self.stream = stream
            self.config = config
            self.next_tick = self.next_login = self.next_revive = self.next_status = self.next_pickup = 0
            self.next_item = {}
            self.pending_item = {}
            self.next_skill = carried_skills
            self.mount_cycle = carried_mount or MountSkillCycle()
            self.mount_ai_paused = carried_ai_pause
            self.last_phase = None
            self.base_attack_speed = self.base_move_speed = None
            self.applied_attack = self.applied_move = None
            self.last_attack_status = None
            self.next_speed_refresh = 0
            self.attack_override = self.move_override = self.camera_override = False
            self.base_camera = None
            self.camera_value = None
            self.next_camera_refresh = 0
            self.next_visual_refresh = 0
            self.next_hybrid_attack = 0
            self.normal_hold = carried_normal if carried_normal is not None else NormalAttackHold()
            self.route_motion = carried_route if carried_route is not None else RouteMotionDriver()
            self.route_motion_request = None
            self.next_route_tick = 0
            self.next_route_status = 0
            self.hybrid_intent = None
            self.hybrid_resume_after = 0
            self.hidden_visuals = set()
            self.effects_hidden = False
            self.shine_hidden = False
            self.message = 'Otomasyon hazır'
            self.dungeon_stage = 0
            self.dungeon_step = 0
            self.dungeon_vid = 0
            self.next_dungeon = 0
            self.dungeon_inventory_baseline = set()
            self.carried_baseline = carried
            self.give_vnum = 0
            self.give_total = 0
            self.give_deadline = 0
            self.give_attempts = 0
            self.ground = []
            self.ground_scan_phase = 0
            self.ground_seen = {}
            self.next_ground = 0
            self.next_shaman_close = 0
            self.next_npc_snapshot = 0
            self.loot_vid = 0
            self.loot_since = 0
            self.loot_skip = {}
            self.range_pickup_sent = {}
            self.range_pickup_jump_at = 0
            self.game_window = carried_window
            self.next_mapinfo = 0
            self.health_identity = None
            self.health_seen_positive = False
            self.Show()

        def npc_vids(self):
            vids = []
            path = self.config.get('npcs')
            if path:
                try:
                    with opener(path, 'r') as source:
                        vids = [int(x) for x in source.read().split(',') if x.strip()]
                except (OSError, ValueError):
                    vids = []
            for vid in self.config.get('entity_vids', []):
                if vid not in vids:
                    vids.append(vid)
            return vids

        def find_kellam(self):
            matches = []
            name = (self.config.get('dungeon_npc') or 'Kellam').strip().lower()
            for vid in self.npc_vids():
                try:
                    if _onur_npc_matches(vid, name):
                        distance = self.kellam_distance(vid)
                        matches.append((distance if distance >= 0 else 1e30, vid))
                except Exception:
                    continue
            return min(matches)[1] if matches else 0

        def publish_dungeon_npc(self, now):
            if not self.config.get('dungeon_auto') or not self.config.get('dungeon_npc_state') or now < self.next_npc_snapshot:
                return
            self.next_npc_snapshot = now + 0.35
            vid = self.find_kellam()
            message = str(int(self.config.get('dungeon_ui_stage', 0))) + '|' + str(vid)
            if vid:
                try:
                    point = chr.GetPixelPosition(vid)
                    message += '|' + str(float(point[0])) + '|' + str(float(point[1]))
                except Exception:
                    message += '|none'
            try:
                with opener(self.config['dungeon_npc_state'], 'w', encoding='utf-8') as output:
                    output.write(message)
            except OSError:
                pass

        def find_symbol(self):
            symbol_slot = -1
            fallback_slot = -1
            for slot in range(max(360, int(getattr(player, 'INVENTORY_SLOT_COUNT', 470)))):
                try:
                    vnum = int(player.GetItemIndex(slot))
                    if vnum <= 0:
                        continue
                    lowered = item.GetItemNameByVnum(vnum).strip().lower()
                    required_id = int(self.config.get('dungeon_item_id', 0))
                    if (required_id > 0 and vnum == required_id) or (required_id <= 0 and 'kahraman' in lowered and ('sembol' in lowered or 'symbol' in lowered)):
                        symbol_slot = slot
                        break
                    if required_id <= 0 and fallback_slot < 0 and vnum not in self.dungeon_inventory_baseline and int(item.GetItemTypeByVnum(vnum)) == int(item.ITEM_TYPE_QUEST):
                        fallback_slot = slot
                except Exception:
                    continue
            return symbol_slot if symbol_slot >= 0 else fallback_slot

        def item_total(self, vnum):
            total = 0
            # Use the same extended-inventory limit as the slot search. A client reporting
            # only its first pages must not turn a real use into a false "not consumed" result.
            try:
                inventory_limit = max(360, int(getattr(player, 'INVENTORY_SLOT_COUNT', 470)))
            except Exception:
                inventory_limit = 470
            for slot in range(inventory_limit):
                try:
                    if int(player.GetItemIndex(slot)) == vnum:
                        total += max(1, int(player.GetItemCount(slot)))
                except Exception:
                    continue
            return total
        def scan_ground(self, now=None):
            # Ground items have name labels; textTail.Pick finds them on screen and the terrain
            # point under each label gives its world position (same system as the player position).
            # Scan alternating rows and retain short-lived hits. This covers narrow/stacked labels
            # without freezing the game with one very dense full-screen pass.
            if now is None:
                now = time.time()
            found = []
            if textTail is None or background is None or grp is None:
                return found
            try:
                cursor = app.GetCursorPosition()
            except Exception:
                cursor = None
            try:
                width = int(wndMgr.GetScreenWidth())
                height = int(wndMgr.GetScreenHeight())
                # Refresh only labels the client already decided to show. Do not force ShowAll:
                # that changes the engine's transient list and can retain deleted entries.
                refresh = getattr(textTail, 'UpdateShowingTextTail', None)
                if refresh is not None:
                    refresh()
                hits = {}
                phase = self.ground_scan_phase & 1
                self.ground_scan_phase = (self.ground_scan_phase + 1) & 1
                for sy in range(phase * 6, height, 12):
                    for sx in range(0, width, 16):
                        vid = textTail.Pick(sx, sy)
                        if vid is not None and vid > 0 and vid not in hits:
                            hits[vid] = (sx, sy)
                for vid, point in hits.items():
                    pick_y = min(height - 1, point[1] + 14)
                    try:
                        grp.SetCursorPosition(point[0], pick_y)
                        world = background.GetPickingPoint()
                        value = (vid, point[0], pick_y, float(world[0]), float(world[1]), float(world[2]) if len(world) > 2 else 0.0)
                    except Exception:
                        # Pickup needs the real VID; minimap coordinates are optional.
                        value = (vid, point[0], pick_y, 0.0, 0.0, 0.0)
                    self.ground_seen[vid] = (value, now + 1.4)
            except Exception:
                pass
            for vid in list(self.ground_seen):
                value, expires = self.ground_seen[vid]
                if expires < now:
                    del self.ground_seen[vid]
                else:
                    found.append(value)
            if self.config.get('ground'):
                # Every visible ground item (id, local x, y) for the host's minimap.
                try:
                    with opener(self.config['ground'], 'w') as output:
                        output.write(';'.join(str(g[0]) + ',' + str(int(g[3])) + ',' + str(int(g[4])) for g in found))
                except Exception:
                    pass
            if cursor is not None:
                try:
                    grp.SetCursorPosition(int(cursor[0]), int(cursor[1]))
                except Exception:
                    pass
            return found

        def send_range_pickup(self, ground_item, now):
            # Nearby items need only their VID. For farther items this server validates
            # position, so synchronize at the terrain point, pick, then restore the exact
            # origin. This never creates a walking destination or changes the combat target.
            vid = int(ground_item[0])
            try:
                origin = player.GetMainCharacterPosition()
                ox, oy = float(origin[0]), float(origin[1])
                tx, ty = float(ground_item[3]), float(ground_item[4])
                if (tx == 0.0 and ty == 0.0) or (tx-ox)*(tx-ox)+(ty-oy)*(ty-oy) <= 450.0*450.0:
                    net.SendItemPickUpPacket(vid)
                    return False
                if now < self.range_pickup_jump_at:
                    return None
                setter = getattr(chr, 'SetPixelPosition', None)
                mover = getattr(player, 'SetAnalogMoveState', None)
                stopper = getattr(player, 'EndKeyWalkingImmediately', None)
                selector = getattr(chr, 'SelectInstance', None)
                if not callable(setter) or not callable(mover) or not callable(stopper) or not callable(selector):
                    net.SendItemPickUpPacket(vid)
                    return False
                me = int(player.GetMainCharacterIndex())
                oz = float(origin[2]) if len(origin) > 2 else 0.0
                tz = float(ground_item[5]) if len(ground_item) > 5 else oz
                target_reader = getattr(player, 'GetTargetVID', None)
                target_writer = getattr(player, 'SetTarget', None)
                old_target = int(target_reader()) if callable(target_reader) else 0
                rotation_reader = getattr(chr, 'GetRotation', None)
                rotation_writer = getattr(chr, 'SetRotation', None)
                selector(me)
                old_rotation = float(rotation_reader()) if callable(rotation_reader) else None
                moved = False
                try:
                    setter(int(round(tx)), int(round(ty)), int(round(tz)))
                    moved = True
                    mover(True, 0.0)
                    stopper()
                    net.SendItemPickUpPacket(vid)
                finally:
                    if moved:
                        selector(me)
                        setter(int(round(ox)), int(round(oy)), int(round(oz)))
                        mover(True, 0.0)
                        stopper()
                        if old_rotation is not None and callable(rotation_writer):
                            rotation_writer(old_rotation)
                    if old_target > 0 and callable(target_reader) and callable(target_writer) and int(target_reader()) != old_target:
                        target_writer(old_target)
                self.range_pickup_jump_at = now + 0.28
                return True
            except Exception:
                try:
                    net.SendItemPickUpPacket(vid)
                except Exception:
                    pass
                self.range_pickup_jump_at = now + 0.5
                return False

        def player_active(self, now):
            path = self.config.get('input')
            if not path:
                return False
            try:
                with opener(path, 'r') as source:
                    return now - float(source.read()) < 1.5
            except Exception:
                return False

        def tick_loot(self, now):
            # Returns True while something lies on the ground: everything else waits until it is collected.
            if now >= self.next_ground:
                self.next_ground = now + 0.4
                # The scan moves the in-game cursor; it waits while the player is using the mouse.
                if not self.player_active(now):
                    self.ground = [g for g in self.scan_ground() if self.loot_skip.get(g[0], 0) < now]
            if not self.ground:
                self.loot_vid = 0
                return False
            here = self.player_xy()
            if here is None:
                return False
            best = min(self.ground, key=lambda g: (g[3] - here[0]) ** 2 + (g[4] - here[1]) ** 2)
            distance = ((best[3] - here[0]) ** 2 + (best[4] - here[1]) ** 2) ** 0.5
            if best[0] != self.loot_vid:
                self.loot_vid = best[0]
                self.loot_since = now
            elif now - self.loot_since > 15:
                # Not ours or unreachable: stop blocking the stage on it.
                self.loot_skip[best[0]] = now + 120
                self.ground = [g for g in self.ground if g[0] != best[0]]
                self.loot_vid = 0
                return bool(self.ground)
            if distance <= 250:
                try:
                    net.SendItemPickUpPacket(best[0])
                except Exception:
                    pass
            self.status('4. asama: yerde esya:' + str(best[0]) + ':' + str(best[1]) + ':' + str(best[2]) + ':' + str(int(distance)) + ':' + str(int(best[3])) + ':' + str(int(best[4])), now, True)
            return True

        def player_xy(self):
            try:
                return tuple(player.GetMainCharacterPosition()[:2])
            except Exception:
                return None

        def kellam_distance(self, vid):
            try:
                kx, ky = chr.GetPixelPosition(vid)[:2]
                px, py = player.GetMainCharacterPosition()[:2]
                return ((kx - px) ** 2 + (ky - py) ** 2) ** 0.5
            except Exception:
                return -1

        def status(self, message, now, force=False):
            # Machine-readable dungeon state is independent of consumable and skill notices.
            if (message.startswith('4. asama:') or message.startswith('Kahramanlik Sembolu Kellam NPCye verildi')) and self.config.get('dungeon_state'):
                try:
                    with opener(self.config['dungeon_state'], 'w', encoding='utf-8') as output:
                        output.write(str(int(self.config.get('dungeon_ui_stage', 0))) + '|' + message)
                except OSError:
                    pass
            self.message = message
            if force or now >= self.next_status:
                self.next_status = now + 1
                try:
                    with opener(self.config['status'], 'w', encoding='utf-8') as output:
                        output.write(message)
                except OSError:
                    pass

        def OnUpdate(self):
            now = time.time()
            if now < self.next_tick:
                # Arrival/crossing detection must run faster than general automation.
                # Keep the other features on their original cadence.
                if self.config.get('route_motion') and now >= self.next_route_tick:
                    try:
                        with opener(self.config['lease'],'r') as source:lease=float(source.read())
                        import os
                        phase=getattr(self.stream,'curPhaseWindow',None)
                        if type(phase).__name__!='GameWindow' or player.GetStatus(player.HP)<=0 or player.GetStatus(player.MAX_HP)<=0:
                            self.route_motion.cancel(player,chr,background,type(phase).__name__=='GameWindow' and player.GetStatus(player.MAX_HP)>0)
                            self.route_motion.reset_leg()
                        else:
                            retreating=False
                            if self.config.get('retreat_guard'):
                                try:
                                    with opener(self.config['retreat_guard'],'r') as source:retreating=float(source.read())>now
                                except Exception:pass
                            allowed=now<=lease and not (self.config.get('emergency_stop') and os.path.exists(self.config['emergency_stop'])) and not self.mount_cycle.busy and now>=self.hybrid_resume_after
                            self.tick_route_motion(now,allowed,retreating)
                    except Exception:self.route_motion.cancel(player,chr,background)
                return
            interval = max(0.05, self.config.get('range_pickup_interval', 250) / 1000.0)
            self.next_tick = now + min(0.25, interval if self.config.get('range_pickup') else 0.25)
            try:
                with opener(self.config['lease'], 'r') as source:
                    lease = float(source.read())
                import os
                if now > lease or (self.config.get('emergency_stop') and os.path.exists(self.config['emergency_stop'])):
                    self.normal_hold.release(player)
                    self.route_motion.cancel(player,chr,background)
                    self.mount_cycle.cancel()
                    self.publish_mount_cycle(False)
                    self.restore_visual_settings()
                    self.restore_camera()
                    self.restore_speeds()
                    self.status('Otomasyon durduruldu', now)
                    return
                self.tick(now)
            except Exception as error:
                self.normal_hold.release(player)
                # Route motion has its own short host lease and its fast dispatcher
                # validates death, phase, emergency-stop and expiry.  An unrelated
                # optional feature failure must not cancel a still-valid walk every
                # 250 ms and turn it into tiny stop/start steps.
                if not self.route_request_is_live(now):
                    self.route_motion.cancel(player,chr,background)
                # Do not log exception values: a login error may contain credentials.
                # A source line is safe and makes client-specific API mismatches diagnosable.
                line = ''
                try:
                    trace = error.__traceback__
                    while trace.tb_next is not None:trace = trace.tb_next
                    line = ' · satır ' + str(trace.tb_lineno)
                except Exception:
                    pass
                self.status('Otomasyon yeniden deneyecek: ' + type(error).__name__ + line, now)

        def close_shaman_windows(self, phase):
            closed, remaining = _onur_shaman_close(phase)
            return closed

        def publish_mount_cycle(self, busy):
            path = self.config.get('mount_skill_state')
            if path:
                try:
                    with opener(path, 'w') as output:
                        output.write(str(self.config.get('client_pid', 0))+'|'+str(time.time()+1.5 if busy else 0))
                except Exception:pass

        def tick(self, now):
            cfg = self.config
            phase = getattr(self.stream, 'curPhaseWindow', None)
            phase_name = type(phase).__name__
            if phase_name != 'GameWindow':self.route_motion.cancel(player,chr,background,False)
            self.publish_combat_focus(phase)
            self.publish_target_health(phase, now)
            if phase_name == 'GameWindow' and phase is not self.game_window:
                # Every floor change loads a fresh GameWindow; Wait-attack jumps never do, so this marks real warps.
                if self.game_window is not None and cfg.get('warp'):
                    try:
                        with opener(cfg['warp'], 'w', encoding='utf-8') as output:
                            output.write(repr(now))
                    except OSError:
                        pass
                self.game_window = phase
                self.next_shaman_close = 0
            if phase_name != self.last_phase:
                if self.last_phase == 'GameWindow' and phase_name != 'GameWindow':
                    self.restore_visual_settings()
                    self.restore_camera()
                    self.restore_speeds()
                self.last_phase = phase_name
                self.next_login = 0
            if phase_name == 'LoginWindow':
                if not cfg['login']:
                    self.status('Giriş ekranı bekleniyor', now)
                    return
                if not cfg['username'] or not cfg['password']:
                    self.status('Oturum bölümüne kullanıcı adı ve şifre girin', now)
                    return
                second_factor = getattr(phase, 'popupWindow2FA', None)
                if second_factor is not None and second_factor.IsShow():
                    self.status('İki aşamalı doğrulamayı oyunda tamamlayın', now)
                    return
                connecting = getattr(phase, 'connectingDialog', None)
                if connecting is not None and connecting.IsShow():
                    self.status('Hesap bağlantısının sonucu bekleniyor', now)
                    return
                if now < self.next_login:
                    return
                connect = getattr(phase, 'Connect', None)
                if connect is None:
                    self.status('Bu giriş ekranının bağlantı yöntemi bulunamadı', now)
                    return
                self.next_login = now + 30
                connect(cfg['username'], cfg['password'])
                self.status('Kayıtlı hesapla giriş istendi', now)
                return
            if phase_name == 'SelectCharacterWindow':
                if getattr(phase, 'startIndex', -1) != -1:
                    self.status('Karakterin oyuna girişi bekleniyor', now)
                    return
                if not cfg['enter']:
                    self.status('Karakter seçimi bekleniyor', now)
                    return
                if now < self.next_login:
                    return
                if cfg['character']:
                    name_key = getattr(net, 'ACCOUNT_CHARACTER_SLOT_NAME', None)
                    if name_key is None:
                        self.status('Karakter adı alanı bulunamadı', now)
                        return
                    match = None
                    slot_count = getattr(sys.modules.get('introSelect'), 'SLOT_COUNT', 4)
                    for slot in range(slot_count):
                        if net.GetAccountCharacterSlotDataString(slot, name_key) == cfg['character']:
                            match = slot
                            break
                    if match is None:
                        self.status('Kayıtlı karakter bu hesapta bulunamadı', now)
                        return
                    phase.SelectSlot(match)
                if getattr(phase, 'changeNameFlag', False) or getattr(phase, 'sendedChangeNamePacket', False):
                    self.status('Karakter adlandırma ekranını tamamlayın', now)
                    return
                self.next_login = now + 10
                phase.StartGame()
                self.status('Karakterle oyuna giriş istendi', now)
                return
            if phase_name != 'GameWindow':
                self.normal_hold.release(player)
                self.mount_cycle.cancel()
                self.publish_mount_cycle(False)
                self.status('Oyun ekranÄ± bekleniyor: ' + phase_name, now)
                return
            if cfg.get('close_shaman') and now >= self.next_shaman_close:
                self.next_shaman_close = now + 0.35
                if self.close_shaman_windows(phase):
                    self.status('Şaman penceresi otomatik kapatıldı', now)
            self.publish_dungeon_npc(now)
            if cfg.get('mapinfo') and background is not None and now >= self.next_mapinfo:
                # Map name and local position for the host's map logic, fresh every few frames.
                self.next_mapinfo = now + 0.2
                try:
                    here = player.GetMainCharacterPosition()
                    with opener(cfg['mapinfo'], 'w') as output:
                        output.write(background.GetCurrentMapName() + '|' + str(int(here[0])) + '|' + str(int(here[1])))
                except Exception:
                    pass
            hp = player.GetStatus(player.HP)
            max_hp = player.GetStatus(player.MAX_HP)
            if max_hp <= 0:
                self.normal_hold.release(player)
                self.route_motion.cancel(player,chr,background,False)
                self.status('Karakter verisi bekleniyor', now)
                return
            if hp <= 0:
                self.normal_hold.release(player)
                self.route_motion.cancel(player,chr,background)
                self.mount_cycle.cancel()
                self.publish_mount_cycle(False)
                self.was_dead = True
            elif self.was_dead:
                self.was_dead = False
                self.revive_serial += 1
            if cfg.get('life_state'):
                try:
                    with opener(cfg['life_state'], 'w') as output:
                        output.write(str(now)+'|'+str(hp)+'|'+str(max_hp)+'|'+str(self.revive_serial)+'|'+str(cfg.get('client_pid', 0)))
                except OSError:
                    pass
            if hp <= 0:
                if (cfg['revive'] or self.route_revive_allowed(now)) and now >= self.next_revive:
                    interface = getattr(phase, 'interface', None)
                    dialog = getattr(interface, 'dlgRestart', None)
                    if dialog is not None and dialog.IsShow():
                        self.next_revive = now + 10
                        dialog.RestartHere()
                        self.status('Burada canlanma istendi', now)
                    else:
                        self.status('Canlandırma ekranı bekleniyor', now)
                return
            self.next_revive = 0
            if self.mount_cycle.busy:
                self.normal_hold.release(player)
                self.route_motion.cancel(player,chr,background)
                if not cfg.get('skill_mount', True) or not cfg.get('skills'):
                    self.mount_cycle.cancel()
                else:
                    state = self.mount_cycle.tick(now, player, net)
                    if state=='failed':self.next_skill[self.mount_cycle.vnum]=now+30
                    self.status(self.mount_cycle.error if state=='failed' else 'Oto beceri: '+state, now, True)
                    if state in ('done','failed') and self.mount_ai_paused:
                        self.mount_ai_paused=False
                        if state=='done' and cfg.get('pack_ai') and globals().get('_onur_pack_owner')==cfg.get('emergency_stop'):
                            if not app.IsPlayerAIAttached():app.TriggerPlayerAI(int(cfg['pack_ai_type']),cfg.get('pack_ai_vnums',[]))
                    if state=='done' and cfg.get('pack_stone_attack'):player.SetAttackKeyState(1)
                self.publish_mount_cycle(self.mount_cycle.busy)
                return
            self.publish_mount_cycle(False)
            retreating = False
            if cfg.get('retreat_guard'):
                try:
                    with opener(cfg['retreat_guard'], 'r') as source:
                        retreating = float(source.read()) > now
                except Exception:
                    pass
            if not retreating:
                self.tick_dungeon(now, phase)
            # These client exports ignore their numeric arguments and enter a report
            # routine. No speed writes are permitted until a real backend is verified.
            if cfg.get('attack_speed') or cfg.get('move_speed'):
                self.status('Hız uygulanmadı: bu istemci için hız bağlantısı doğrulanmadı.', now)
            if cfg.get('camera_distance'):
                value = max(1500.0, min(50000.0, float(cfg.get('camera_distance_value', 15000))))
                current = app.GetCamera()
                if self.base_camera is None:
                    self.base_camera = current
                if self.camera_value != value:
                    app.SetCameraMaxDistance(value)
                    app.SetCamera(value, current[1], current[2], current[3])
                    self.camera_value = value
                    self.next_camera_refresh = now + 2
                elif now >= self.next_camera_refresh:
                    app.SetCameraMaxDistance(value)
                    self.next_camera_refresh = now + 2
                self.camera_override = True
            elif self.camera_override:
                self.restore_camera()
            self.apply_visual_settings(now)
            self.tick_route_motion(now,now >= self.hybrid_resume_after,retreating)
            if not retreating:
                self.tick_hybrid_attack(now)
            else:self.normal_hold.release(player)
            if not retreating and cfg.get('range_pickup'):
                # Never divert route motion. Discover real visible ground-item VIDs and
                # submit their pickup packets in the background while combat continues.
                # The normal close-pickup call remains as an independent fallback.
                if now >= self.next_ground and not self.player_active(now):
                    self.next_ground = now + 0.35
                    self.ground = self.scan_ground(now)
                    visible = set(g[0] for g in self.ground)
                    for vid in list(self.range_pickup_sent):
                        if vid not in visible or now-self.range_pickup_sent[vid] > 10:
                            del self.range_pickup_sent[vid]
                    jumped = False
                    for ground_item in self.ground:
                        vid = ground_item[0]
                        if now < self.range_pickup_sent.get(vid, 0):
                            continue
                        try:
                            result = self.send_range_pickup(ground_item, now)
                            if result is None:
                                continue
                            if result is True:
                                if jumped:
                                    continue
                                jumped = True
                            self.range_pickup_sent[vid] = now + 0.55
                        except Exception:
                            self.range_pickup_sent[vid] = now + 1.0
                    if jumped:
                        # The short synchronization ended its temporary input. Reassert
                        # the unchanged route lease immediately instead of pausing a step.
                        self.tick_route_motion(now, now >= self.hybrid_resume_after, retreating)
                if now >= self.next_pickup:
                    try:
                        player.PickCloseItemAll()
                    except Exception:
                        pass
                    self.next_pickup = now + max(0.05, cfg.get('range_pickup_interval', 250) / 1000.0)
            # Confirm whether the server actually consumed the requested item. The host shows this
            # separately from inventory reading, so a blocked buff is not mistaken for a successful use.
            for pending_vnum, pending in list(self.pending_item.items()):
                if now - pending[1] < 0.5:
                    continue
                after = self.item_total(int(pending_vnum))
                try:
                    pending_name = item.GetItemNameByVnum(int(pending_vnum)).strip()
                except Exception:
                    pending_name = str(pending_vnum)
                if after < pending[0]:
                    self.status('Eşya kullanıldı: ' + pending_name + ' · ID ' + str(pending_vnum), now, True)
                else:
                    self.status('Eşya henüz tüketilmedi (etki aktif veya oyun bekletti): ' + pending_name + ' · tekrar denenecek', now, True)
                del self.pending_item[pending_vnum]
            items = dict(cfg['items'])
            if cfg['cloak'] and not retreating:
                items[70038] = cfg['cloak_interval']
            for vnum, interval in items.items():
                if retreating and int(vnum)==70038:
                    continue
                if now < self.next_item.get(vnum, 0):
                    continue
                slot = None
                # All normal and extended inventory pages are scanned. Some clients report 360,
                # while this build exposes up to 470 contiguous item slots.
                try:
                    inventory_limit = max(360, int(getattr(player, 'INVENTORY_SLOT_COUNT', 470)))
                except Exception:
                    inventory_limit = 470
                for candidate in range(inventory_limit):
                    try:
                        if int(player.GetItemIndex(candidate)) == int(vnum) and int(player.GetItemCount(candidate)) > 0:
                            slot = candidate
                            break
                    except Exception:
                        continue
                if slot is None:
                    self.next_item[vnum] = now + 2
                    continue
                # Resolve current slot again for every use; never trust a stale inventory position.
                try:
                    if int(player.GetItemIndex(slot)) != int(vnum) or int(player.GetItemCount(slot)) <= 0:
                        continue
                    before_total = self.item_total(int(vnum))
                    net.SendItemUsePacket(slot)
                    self.pending_item[vnum] = (before_total, now)
                except Exception:
                    self.next_item[vnum] = now + 1
                    continue
                self.next_item[vnum] = now + max(1, interval)
                try:
                    used_name = item.GetItemNameByVnum(int(vnum)).strip()
                except Exception:
                    used_name = str(vnum)
                self.status('Eşya komutu gönderildi: ' + used_name + ' · ID ' + str(vnum) + ' · yuva ' + str(slot), now, True)
                return
            # Recovery owns movement and mounting. A due buff must not dismount
            # the actor in the middle of escaping or invalidate a mount ACK.
            if retreating:return
            skill_config = cfg['skills']
            if hasattr(skill_config, 'items'):
                order = cfg.get('skill_order') or []
                seen = set(order)
                skill_intervals = [(vnum, skill_config.get(vnum, 2)) for vnum in order if vnum in skill_config] + [(vnum, interval) for vnum, interval in skill_config.items() if vnum not in seen]
            else:
                skill_intervals = ((vnum, 2) for vnum in skill_config)
            for vnum, interval in skill_intervals:
                if now < self.next_skill.get(vnum, 0):
                    continue
                try:
                    slot = player.GetSkillSlotIndex(vnum)
                except (ValueError, RuntimeError):
                    self.next_skill[vnum] = now + 5
                    continue
                if slot < 0 or slot >= 255 or player.GetSkillIndex(slot) != vnum:
                    continue
                if player.GetSkillLevel(slot) <= 0 or player.IsSkillActive(slot) or player.IsSkillCoolTime(slot):
                    continue
                self.next_skill[vnum] = now + max(1, interval)
                self.normal_hold.release(player)
                self.route_motion.cancel(player,chr,background)
                self.hybrid_resume_after = now + 1
                if cfg.get('skill_mount', True):
                    try:
                        if self.mount_cycle.mounted(player) and callable(getattr(app,'IsPlayerAIAttached',None)) and app.IsPlayerAIAttached():
                            if not cfg.get('pack_ai') or globals().get('_onur_pack_owner')!=cfg.get('emergency_stop'):
                                self.status('Baska PlayerAI aktif; binekli beceri bekletildi',now,True)
                                return
                            app.TriggerPlayerAI(int(cfg['pack_ai_type']),cfg.get('pack_ai_vnums',[]))
                            if app.IsPlayerAIAttached():
                                self.status('PlayerAI durusu dogrulanamadi; binek komutu gonderilmedi',now,True)
                                return
                            self.mount_ai_paused=True
                        self.mount_cycle.start(slot, vnum, now, player, net)
                    except Exception:
                        self.mount_cycle.cancel()
                        self.status('Binekli beceri API dogrulanamadi; Ctrl+G komutu tekrarlanmadi', now, True)
                        return
                    self.publish_mount_cycle(self.mount_cycle.busy)
                else:
                    player.ClickSkillSlot(slot)
                self.status('Beceri kullanım isteği: ' + str(vnum), now)
                return
            self.status('Otomasyon açık; süreler ve envanter izleniyor', now)
        def tick_dungeon(self, now, phase):
            cfg = self.config
            stage = int(cfg.get('dungeon_stage', 0)) if cfg.get('dungeon_auto') else 0
            ui_stage = int(cfg.get('dungeon_ui_stage', 0))
            if stage != self.dungeon_stage or ui_stage != getattr(self, 'last_dungeon_ui_stage', -1):
                self.dungeon_stage = stage
                self.last_dungeon_ui_stage = ui_stage
                self.dungeon_step = 0
                self.dungeon_vid = 0
                self.next_dungeon = now
                self.dungeon_inventory_baseline = set()
                self.give_vnum = 0
                self.give_attempts = 0
                if stage in (4, 9) and self.carried_baseline is not None:
                    self.dungeon_inventory_baseline = self.carried_baseline
                    self.carried_baseline = None
                elif stage in (4, 9):
                    for baseline_slot in range(int(getattr(player, 'INVENTORY_SLOT_COUNT', 470))):
                        try:
                            baseline_vnum = int(player.GetItemIndex(baseline_slot))
                            if baseline_vnum > 0:
                                self.dungeon_inventory_baseline.add(baseline_vnum)
                        except Exception:
                            continue
            # Stage 9 is the map-driven runner: loot, symbol and Kellam handling stay active on every floor.
            if stage in (4, 9):
                if self.dungeon_step == 99 or now < self.next_dungeon:
                    return
                if self.give_vnum:
                    # Success only when the server really took the symbol out of the inventory.
                    # A warp alone is not proof: the floor teleports the character repeatedly.
                    if self.item_total(self.give_vnum) < self.give_total:
                        self.dungeon_step = 99
                        self.next_dungeon = now + (1 if stage == 9 else 3600)
                        self.status('Kahramanlik Sembolu Kellam NPCye verildi:vid=' + str(self.dungeon_vid) + ':item=' + str(self.give_vnum), now, True)
                        if stage == 9:
                            self.give_vnum = 0
                        return
                    if now < self.give_deadline:
                        self.next_dungeon = now + 0.25
                        return
                    self.give_vnum = 0
                # Inventory ownership wins over all remaining ground drops.
                symbol_slot = self.find_symbol()
                if symbol_slot < 0:
                    try:
                        player.PickCloseItemAll()
                    except Exception:
                        pass
                    symbol_slot = self.find_symbol()
                if symbol_slot < 0 and self.tick_loot(now):
                    self.next_dungeon = now + 0.25
                    return
                if symbol_slot >= 0:
                    self.status('4. asama: sembol envanterde (slot ' + str(symbol_slot) + '), NPCye gidiliyor', now, True)
                kellam_vid = self.find_kellam()
                if kellam_vid > 0:
                    self.dungeon_vid = kellam_vid
                if symbol_slot < 0:
                    self.next_dungeon = now + 0.25
                    if kellam_vid > 0:
                        self.status('4. asama: Kellam bulundu:' + str(kellam_vid) + ':' + str(int(self.kellam_distance(kellam_vid))) + ':0', now, True)
                    else:
                        self.status('4. asama: Kahramanlik Sembolu bekleniyor', now)
                    return
                if kellam_vid <= 0:
                    self.next_dungeon = now + 0.25
                    self.status('4. asama: sembol envanterde (slot ' + str(symbol_slot) + '), Kellam araniyor', now, True)
                    return
                distance = self.kellam_distance(kellam_vid)
                if distance > 400:
                    # The host walks to this radar VID; the symbol is only handed over next to Kellam.
                    self.next_dungeon = now + 0.25
                    self.status('4. asama: Kellam bulundu:' + str(kellam_vid) + ':' + str(int(distance)) + ':1', now, True)
                    return
                vnum = int(player.GetItemIndex(symbol_slot))
                count = max(1, int(player.GetItemCount(symbol_slot)))
                self.give_vnum = vnum
                self.give_total = self.item_total(vnum)
                try:
                    window = player.SlotTypeToInvenType(player.SLOT_TYPE_INVENTORY)
                except Exception:
                    window = player.INVENTORY
                net.SendGiveItemPacket(kellam_vid, window, symbol_slot, count)
                self.give_attempts += 1
                self.give_deadline = now + 2.5
                self.next_dungeon = now + 0.25
                self.status('4. asama: sembol Kellama veriliyor (deneme ' + str(self.give_attempts) + ', mesafe ' + str(int(distance)) + ')', now, True)
                return
            if stage != 1 or now < self.next_dungeon:
                return
            if self.dungeon_step == 0:
                last_error = ''
                for vid in self.npc_vids():
                    try:
                        if _onur_npc_matches(vid, self.config.get('dungeon_npc') or 'Kellam'):
                            point = chr.GetProjectPosition(vid, 120)
                            if not point or len(point) < 2 or point[0] < 0 or point[1] < 0:
                                continue
                            self.dungeon_vid = vid
                            self.dungeon_step = 99
                            self.next_dungeon = now + 3600
                            self.status('Kellam koordinati:' + str(int(point[0])) + ':' + str(int(point[1])), now, True)
                            return
                    except Exception as error:
                        last_error = type(error).__name__
                        continue
                self.next_dungeon = now + 0.5
                self.status('Yakinda etkilesilebilir NPC araniyor' + (': ' + last_error if last_error else ''), now)
                return
            try:
                if self.dungeon_step == 1:
                    event.SelectAnswer(1)
                    self.status('Alev simgeli ikinci seçenek seçildi', now)
                elif self.dungeon_step == 2:
                    event.SelectAnswer(0)
                    self.status('Görev onayı: Evet', now)
                elif self.dungeon_step == 3:
                    event.Skip()
                    self.status('Görev metni: Devam', now)
                else:
                    return
                self.dungeon_step += 1
                self.next_dungeon = now + 1.25
            except Exception:
                self.next_dungeon = now + 0.5

        def restore_camera(self):
            try:
                if self.camera_override and self.base_camera is not None:
                    try:
                        import constInfo
                        constInfo.SET_DEFAULT_CAMERA_MAX_DISTANCE()
                    except Exception:
                        app.SetCameraMaxDistance(2500.0)
                    app.SetCamera(*self.base_camera)
            except Exception:
                pass
            self.camera_override = False
            self.camera_value = None
            self.next_camera_refresh = 0
        def visual_vids(self):
            result = []
            path = self.config.get('visuals')
            if path:
                try:
                    with opener(path, 'r') as source:
                        result = [int(value) for value in source.read().split(',') if value.strip()]
                except (OSError, ValueError):
                    result = []
            main_vid = player.GetMainCharacterIndex()
            if main_vid and main_vid not in result:
                result.append(main_vid)
            return result
        def apply_visual_settings(self, now):
            if now < self.next_visual_refresh:
                return
            self.next_visual_refresh = now + 0.5
            hide_pets = bool(self.config.get('hide_pets'))
            hide_mounts = bool(self.config.get('hide_mounts'))
            pet_vnums = set(int(value) for value in self.config.get('pet_vnums', []))
            mount_vnums = set(int(value) for value in self.config.get('mount_vnums', []))
            pet_type = getattr(chr, 'INSTANCE_TYPE_PET', 5)
            main_vid = player.GetMainCharacterIndex()
            wanted = set()
            touched = False
            if hide_pets or hide_mounts:
                for vid in self.visual_vids():
                    if vid == main_vid:
                        continue
                    try:
                        race = int(chr.GetVirtualNumber(vid))
                        instance_type = int(chr.GetInstanceType(vid))
                        if (hide_pets and (instance_type == pet_type or race in pet_vnums)) or (hide_mounts and race in mount_vnums):
                            wanted.add(vid)
                    except Exception:
                        continue
            for vid in wanted - self.hidden_visuals:
                try:
                    if chr.HasInstance(vid):
                        chr.SelectInstance(vid)
                        chr.Hide()
                        self.hidden_visuals.add(vid)
                        touched = True
                except Exception:
                    pass
            for vid in list(self.hidden_visuals - wanted):
                try:
                    if chr.HasInstance(vid):
                        chr.SelectInstance(vid)
                        chr.Show()
                        touched = True
                except Exception:
                    pass
                self.hidden_visuals.discard(vid)
            if touched:
                try:
                    chr.SelectInstance(main_vid)
                except Exception:
                    pass
            effects = bool(self.config.get('hide_effects'))
            if effects != self.effects_hidden:
                try:
                    app.SetSkillEffectUpgradeEnable(not effects)
                    self.effects_hidden = effects
                except Exception:
                    pass
            shine = bool(self.config.get('hide_shine'))
            if shine != self.shine_hidden:
                try:
                    app.SetArmorSpecularEnable(not shine)
                    app.SetWeaponSpecularEnable(not shine)
                    self.shine_hidden = shine
                except Exception:
                    pass
        def restore_visual_settings(self):
            main_vid = player.GetMainCharacterIndex()
            had_hidden = bool(self.hidden_visuals)
            for vid in list(self.hidden_visuals):
                try:
                    if chr.HasInstance(vid):
                        chr.SelectInstance(vid)
                        chr.Show()
                except Exception:
                    pass
            self.hidden_visuals.clear()
            if had_hidden:
                try:
                    chr.SelectInstance(main_vid)
                except Exception:
                    pass
            if self.effects_hidden:
                try:
                    app.SetSkillEffectUpgradeEnable(True)
                except Exception:
                    pass
            if self.shine_hidden:
                try:
                    app.SetArmorSpecularEnable(True)
                    app.SetWeaponSpecularEnable(True)
                except Exception:
                    pass
            self.effects_hidden = False
            self.shine_hidden = False
        def hybrid_attack_state(self):
            path = self.config.get('hybrid_targets')
            if not path:
                return False, []
            try:
                with opener(path, 'r') as source:
                    parts = source.read().strip().split('|')
                if len(parts) != 4:raise ValueError('incomplete intent')
                if int(parts[0]) != int(self.config.get('client_pid',0)):
                    self.hybrid_intent = None
                    return False, []
                expires = float(parts[1])
                active = parts[2] == '1'
                values = [int(value) for value in parts[3].split(',') if value.strip()]
                self.hybrid_intent = (expires,active,values)
            except (OSError, ValueError):
                pass
            if self.hybrid_intent is not None and self.hybrid_intent[0] > time.time():
                expires,active,values = self.hybrid_intent
                return active, values
            return False, []
        def tick_hybrid_attack(self, now):
            active, candidates = self.hybrid_attack_state()
            try:
                mapname = background.GetCurrentMapName()
            except Exception:
                self.normal_hold.release(player)
                return
            enabled = active and self.config.get('hybrid_attack') and not self.mount_cycle.busy and now >= self.hybrid_resume_after
            self.normal_hold.tick(now,enabled,candidates,(player.GetMainCharacterIndex(),mapname),player,chr,
                                  self.config.get('hybrid_types',(0,2)),float(self.config.get('hybrid_close',140)))
            if self.normal_hold.error:self.status(self.normal_hold.error,now)
        def route_revive_allowed(self, now):
            try:
                import json,math
                path=self.config.get('route_revive')
                if not path:return False
                with opener(path,'r') as source:request=json.loads(source.read())
                until=float(request.get('Until',0))
                return (request.get('Active') is True and request.get('Pid')==self.config.get('client_pid') and
                    request.get('Vid')==int(player.GetMainCharacterIndex()) and request.get('Map')==background.GetCurrentMapName() and
                    bool(request.get('Token')) and not math.isnan(until) and not math.isinf(until) and now<until<=now+3)
            except Exception:return False
        def route_request_is_live(self, now):
            try:
                import math
                request=self.route_motion_request
                until=float(request.get('Until',0))
                phase=getattr(self.stream,'curPhaseWindow',None)
                return (isinstance(request,dict) and request.get('Active') is True and
                    int(request.get('Pid',0))==int(self.config.get('client_pid',0)) and
                    int(request.get('Vid',0))==int(player.GetMainCharacterIndex()) and
                    request.get('Map')==background.GetCurrentMapName() and bool(request.get('Token')) and
                    type(phase).__name__=='GameWindow' and player.GetStatus(player.HP)>0 and player.GetStatus(player.MAX_HP)>0 and
                    not math.isnan(until) and not math.isinf(until) and now<until<=now+3)
            except Exception:return False
        def tick_route_motion(self, now, allowed, retreating=False):
            self.next_route_tick=now+.04
            request = None
            path = self.config.get('route_motion')
            if path:
                try:
                    import json
                    with opener(path,'r') as source:request=json.loads(source.read())
                    self.route_motion_request = request
                except Exception:
                    # A brief file-sharing/read race must not pulse movement off/on.
                    # The driver's independent 800 ms expiry still stops a lost host.
                    request = self.route_motion_request
            else:self.route_motion_request = None
            recovery=isinstance(request,dict) and request.get('Recovery') is True
            permitted_recovery=recovery and int(request.get('TargetVid',0))==0 and self.route_revive_allowed(now)
            self.route_motion.tick(now,request,int(self.config.get('client_pid',0)),allowed and (permitted_recovery if recovery else not retreating),player,chr,background,app,net)
            status_path=self.config.get('route_motion_status')
            if status_path and isinstance(request,dict) and now>=self.next_route_status:
                self.next_route_status=now+.2
                try:
                    import json
                    with opener(status_path,'w') as output:
                        output.write(json.dumps(dict(Pid=self.config.get('client_pid',0),Token=request.get('Token',''),RecoveryId=request.get('RecoveryId',''),At=now,Error=self.route_motion.error,Moving=self.route_motion.owned,State=self.route_motion.state,Mounted=self.route_motion.mounted_report)))
                except Exception:
                    pass

        def combat_focus_request(self):
            path = self.config.get('combat_focus_request')
            if not path:
                return None
            try:
                with opener(path, 'r') as source:
                    parts = source.read().strip().split('|')
                if len(parts) == 2 and parts[0] and int(parts[1]) > 0:
                    return parts[0], int(parts[1])
            except (OSError, ValueError):
                pass
            return None
        def publish_combat_focus(self, phase):
            focus = self.combat_focus_request()
            path = self.config.get('combat_focus_state')
            if not focus or not path:
                return
            token, vid = focus
            state, hp = 'unknown', None
            try:
                if type(phase).__name__ == 'GameWindow' and player.GetStatus(player.MAX_HP) > 0:
                    exists = bool(chr.HasInstance(vid))
                    state = 'present' if exists else 'missing'
                    if exists:
                        # A newly selected board can retain the previous target's HP.
                        # Wait for a subsequent update; accept only server HP fields.
                        board = getattr(phase, 'targetBoard', None)
                        if player.GetTargetVID() == vid:
                            if board is not None and getattr(board, 'vid', 0) == vid and board.IsShow():
                                for name in ('destHpPct', 'hpPercentage'):
                                    try:
                                        value = float(getattr(board, name))
                                        if 0 <= value <= 100:
                                            hp = value
                                            break
                                    except Exception:
                                        pass
                        else:
                            player.SetTarget(vid)
            except Exception:
                state, hp = 'unknown', None
            try:
                with opener(path, 'w') as output:
                    output.write(token+'|'+str(vid)+'|'+state+'|'+(str(hp) if hp is not None else 'unknown'))
            except OSError:
                pass
        def publish_target_health(self, phase, now):
            path = self.config.get('target_health')
            if not path:
                return
            import json
            data = dict(Pid=self.config.get('client_pid', 0), Vid=0, Name='', Map='', At=now, Hp=None, PlayerHp=None, Mounted=None)
            identity = None
            try:
                if type(phase).__name__ == 'GameWindow' and player.GetStatus(player.MAX_HP) > 0:
                    data['Map'] = background.GetCurrentMapName() if background is not None else ''
                    try:
                        mount_api = getattr(player, 'IsMountingHorse', None)
                        if callable(mount_api):data['Mounted'] = bool(mount_api())
                    except Exception:
                        pass
                    maximum = float(player.GetStatus(player.MAX_HP))
                    health = 100.0 * float(player.GetStatus(player.HP)) / maximum
                    if 0 <= health <= 100:
                        data['PlayerHp'] = health
                    vid = int(player.GetTargetVID())
                    if vid > 0 and chr.HasInstance(vid):
                        data['Vid'] = vid
                        data['Name'] = chr.GetNameByVID(vid)
                        board = getattr(phase, 'targetBoard', None)
                        identity = (id(phase), data['Map'], vid, id(board))
                        if identity != self.health_identity:
                            self.health_seen_positive = False
                        elif board is not None and getattr(board, 'vid', 0) == vid and board.IsShow():
                            # Server percentage, not currHpPct (the delayed visual animation).
                            for field in ('destHpPct', 'hpPercentage'):
                                try:
                                    value = float(getattr(board, field))
                                    if 0 <= value <= 100:
                                        if value > 0:
                                            self.health_seen_positive = True
                                        if value > 0 or self.health_seen_positive:
                                            data['Hp'] = value
                                        break
                                except Exception:
                                    pass
            except Exception:
                data['Hp'] = None
            self.health_identity = identity
            try:
                with opener(path, 'w') as output:
                    output.write(json.dumps(data, ensure_ascii=True))
            except (OSError, TypeError, ValueError):
                pass
        def restore_speeds(self):
            # Never call the reporting exports on disable, disconnect or expiry either.
            self.attack_override = self.move_override = False
    globals()['_onur_agent'] = OnurAgent()

try:
    _onur_install(__ONUR_CONFIG__)
except Exception as _onur_error:
    with globals().get('old_open', open)(__ONUR_CONFIG__['status'], 'w', encoding='utf-8') as _onur_output:
        _onur_output.write('Kurulum yeniden denenecek: ' + type(_onur_error).__name__)













