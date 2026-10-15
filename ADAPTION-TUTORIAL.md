# Adapting FarmBot to a Metin2 client (Arthion playbook)

This is the exact method used to make FarmBot work on **Arthion** (`Arthion.exe`, 32-bit / WOW64, Python 2 `player` / `item` / `chr` modules, Metin2 ClientSRC family). Use it when attaching a **new client you administer**. Do not skip the “what failed” notes — most of the time on Arthion was spent un-doing calls that looked right in the binary and crashed in-game.

FarmBot does **not** drive combat from C#. C# decides *what* to do. The in-process x86 module `FarmBotAction.dll` calls the client’s own `thiscall` methods on the **game thread**.

| Piece | Role |
| --- | --- |
| `FarmBot.exe` | UI, targeting, potion/skill policy, layout recovery |
| `FarmBotLoad.exe` | Injects `FarmBotAction.dll` into the 32-bit client |
| `FarmBotAction.dll` | Named pipe + native calls (`tools/FarmBotAction/FarmBotAction.cpp`) |
| `PlayerActionLayoutRecoverer` | Static PE scan that fills RVAs (`src/FarmBot.Core/Combat/PlayerActionLayout.cs`) |
| `cache/{account}/{character}.json` | Potion/skill icons + names so the UI does not wait for game phase |

After any native rebuild: **close FarmBot and the client**, then launch again so the new DLL is injected.

---

## 1. Confirm the client family

Arthion matched official Metin2 ClientSRC enough that:

- Singletons live in `.data` (`CPythonPlayer`, `CPythonCharacterManager`, `CPythonNetworkStream`, `CItemManager` / `CSkillManager`, `CEterPackManager`).
- Python method names are still **ASCII in the EXE**: `SetTarget`, `ClickSkillSlot`, `GetItemIndex`, `SendItemUsePacket`, …
- Calling convention is **MSVC x86 thiscall**: `ecx = this`, args right-to-left, callee pops.
- `CPythonPlayer` implements `IAbstractPlayer` at **`this+4`**. Virtual calls (GetStatus, SetTarget on some builds) use `ecx = player+4`.

If the new EXE has no `ClickSkillSlot` / `GetItemIndex` strings, stop and find the renamed Python table first. Everything below hangs off those names.

Image is 32-bit (`IMAGE_FILE_MACHINE_I386`). The DLL **must** be built x86 (`tools/FarmBotAction/build.ps1` uses the x86 VC prompt). A 64-bit injector cannot thiscall into this process.

---

## 2. Recover RVAs from the EXE (do this first)

Do **not** copy Arthion’s hardcoded RVAs. They change per build. FarmBot already recovers them:

1. Point the launcher at the new `Client.exe`.
2. Profile **Arthion** writes a layout file via `PlayerActionLayoutRecoverer.Recover` + `RecoverLogin` (`ActionModuleHost.WriteLayoutFile`).
3. `FarmBotLoad` passes `--layout` into the DLL. `LoadConfig()` in `FarmBotAction.cpp` parses `player=…`, `clickSkill=…`, `getItemIndex=…`, etc.

### How recovery actually finds a native

`RecoverNamedNative(pe, "ClickSkillSlot")` (and the same for every other name):

1. Scan the PE for the **ASCIIZ method name**.
2. Find xrefs that sit in a **Python `METH_VARARGS` method table** (name pointer + function pointer).
3. That function pointer is the **wrapper RVA**.
4. In the wrapper, the first global load is the **singleton** (`CPythonPlayer*` / `CItemManager*` / `CPythonNetworkStream*`).
5. The `E8` call after that load is the **real native**.

If the wrapper does `lea ecx,[ecx+4]` / `add ecx,4` then `call [eax+slot]`, the native is **virtual**. Store the **vtable slot**, not a stale absolute RVA. Arthion GetStatus and sometimes SetTarget are virtual.

Fallback when the Python name is missing:

- Unique error strings, e.g. `"Send Battle Attack Packet Error"` → `SendAttackPacket`.
- Packet header next to `SendItemPickUpPacket` (attack packet is a sibling send).
- Neighbour methods in the same Python table as `SetTarget` / `ComboAttack`.

**Sanity log** after inject (`FarmBotAction` native log): `loaded base=… profile=arthion sendAttack=… getItemIndex=… clickSkill=…`. If those are `0`, recovery failed — fix the PE scan, do not guess.

---

## 3. Game thread only

`CPythonPlayer` methods are **not** safe from the pipe worker.

What worked on Arthion:

- Hook `PeekMessageA/W` and `GetMessageA/W` (IAT + trampoline on `user32`) so `Drain()` runs inside `CPythonApplication::Loop`.
- Queue `attack`, `use_item`, `use_skill`, `get_hud`, login, etc. onto that loop.
- `__try/__except` around every thiscall. It does **not** catch every crash (heap / stack smash still kills the client). Wrong-thread and login-time calls still crashed.

Exception we kept on the pipe thread: `get_pots` / `get_skills` (inventory walk + pack extract). They hitch the game loop if queued. They **must not run until the character is actually in the world** (see §8). Running them at logo/login crashed Arthion.

---

## 4. Attacking (what finally worked)

### Recover

| Name in EXE | Use |
| --- | --- |
| `SetTarget` | VID target (virtual or thiscall). `this+4` when IAbstract. |
| `SetAttackKeyState` | Hold attack key (`m_isAtkKey`). Applied later on the game thread. |
| `NEW_Attack` / `Attack` | One-shot attack with range / bow / shop checks. Does not path melee. |
| `ReserveClickActor` | Click-to-approach then attack (ordinary mouse path). |
| `__OnPressActor` | Full click: SetTarget + attack-in-range or reserve-click. |
| `ComboAttack` | Melee swing once in range. |
| `SendAttackPacket` | Network send. Useful, **not** enough by itself on Arthion. |
| `NEW_MoveToDestPixelPositionDirection` | Walk toward a world pixel. |

Entity list: `CPythonCharacterManager` singleton + `m_kAliveInstMap` (red-black tree). FarmBot recovers map displacement (`map=+0x20` on Arthion: `layout recovered from … singletonRva=… map=+0x20`). Each node has VID key + `CInstanceBase*`. Types come from the instance, not from guessing.

### Call path that worked (Arthion profile)

On the **game thread**, for a farm VID:

1. `SetTarget(vid, force=1)` **only when already in the world with a real target**. Calling SetTarget during login crashed.
2. Drive with `MoveToDest` / reserved-click chase until in range (`InRange` native).
3. `SetAttackKeyState(true)` and/or `NEW_Attack` / combo on the update hook.

Arthion-specific: FarmBot **patches** `CPythonPlayer::Update` so combo actually runs (`PatchUpdateComboCall`), and hooks AttackKey / ReserveClick / Chase / InRange / MoveToDest so the bot can own the click-to-kill loop without fighting the client.

### What failed (do not repeat)

- Sending only `SendAttackPacket` from the pipe thread: no facing, no combo, often ignored.
- `SetTarget` from the pipe worker: instant client crash.
- `SetTarget` / attack during **login / character select / loading**: crash even on the game thread (player/actor graph not ready).
- Treating every skill as an attack because `IsNeedTarget` / native “kind” was wrong (see skills).

Targeting policy stays in C# (`BotRuntime` / `AutoCombatController`). The DLL only executes VID + walk + attack.

---

## 5. Finding potions

Walk **inventory cells**, do not hardcode bag UI slots.

### Recover

| Name | Singleton | Meaning |
| --- | --- | --- |
| `GetItemIndex` | `CPythonPlayer` | Vnum in a window/cell |
| `GetItemCount` | `CPythonPlayer` | Stack size |
| `UseItem` | `CPythonPlayer` | Local use |
| `SelectItem` | `CItemManager` | Select proto by vnum |
| `GetItemName` / `GetItemType` / `GetItemSubType` / `GetAffect` / `GetValue` | selected proto | Name, type, speed/HP applies |
| `SendItemUsePacket` | `CPythonNetworkStream` | Server use |

`TItemPos` on this family is **packed 1-byte window + 2-byte cell** (`#pragma pack(1)`). Inventory window id is **`1`**. Cell is `0 … 179` (5 pages × 36) unless the new client documents otherwise.

```text
thiscall GetItemIndex(TItemPos pos)  // ecx = player+4 (IAbstract) on Arthion
packed SendItemUse = window | (cell << 8)   // window=1
```

### Classify (keep this, extend names/vnums per server)

`ClassifyPot` in `FarmBotAction.cpp`:

- Skip equipment (`type` 1 or 2).
- Official-ish vnums: HP `27001–27003`, MP `27004–27006`, green `27100–27102`, purple `27103–27105`.
- Else name contains potion/iksir plus red/hp, blue/mana, green/mov_speed (`apply==8`), purple/att_speed (`apply==7`).
- Green/purple duration from `GetValue(1)` or affect value, clamped 15–600 seconds.

Scan returns JSON `{v,n,c,s,f,d,t,i}` (vnum, name, count, **cell**, family, duration, tooltip, extracted icon path). C# maps that to `PotionStack`. **Slot in the UI is the live inventory cell.** If the player moves/trades/deletes a stack, a 250 ms in-world scan overwrites slot and count. Empty live inventory means empty — do not keep stale stacks.

### Using a potion

On the game thread, only after a **live** in-world scan (never from cached cells):

1. `SendItemUsePacket(packed cell)` first.
2. If that returns 0, `CPythonPlayer::UseItem(pos)`.
3. If still 0, Python `player.UseItemSlot` as last resort.

C# `PotionController`: HP/MP by percent; green/purple when their buff timer is down. `PotionEnabledVnums` empty means **all** classified pots are eligible (green/purple will fire at full HP — that is why login-time use crashed when cache provided slots).

---

## 6. Finding skills

### Recover

| Name | Meaning |
| --- | --- |
| `GetSkillIndex` | Skill id for a skill **slot** (0–254; unused slots return 0) |
| `GetSkillLevel` | Learned level; skip if `<= 0` |
| `ClickSkillSlot` | **The call that actually casts on Arthion** |
| `GetSkillCoolTime` / `IsSkillCoolTime` | Client cool (informational; FarmBot delay is separate) |
| `GetSkillName` / skill proto via `CSkillManager` | Name, icon path, duration |
| `SendUseSkillPacket` | Network-only cast — **did not replace ClickSkillSlot** |

Scan: for each slot, if index>0 and level>0, read proto name/icon, skip junk (mining, fishing, language, combo, …).

### Using a skill (do not improvise)

Proven log on Arthion: `use_skill slot=N click=1 send=0`.

```text
game thread: ClickSkillSlot(slot)   // ecx = CPythonPlayer*
```

**Do not** add these to `use_skill` (they broke a working client):

- `SendUseSkillPacket` instead of click (attacks stopped landing).
- `SetTarget` inside `use_skill` (login / Start crash).
- Auto “buff vs attack” from `IsNeedTarget` / `IsStandingSkill` (Arthion shaman 106–111 all came back as buff).

Policy is **UI checkboxes**, not native kind:

- **Use** checkbox → `SkillEnabledIndexes`.
- **Buff** checkbox → `SkillBuffIndexes` → click with no farm target (vid 0) **only after Start**, and only in-world.
- Unmarked skills wait for a farm VID, then still `ClickSkillSlot` (the client already has the target from combat).
- Delay 0 = every 300 ms once cool is ignored; delay N = N seconds since last FarmBot use.

Skills must not run before **Start** (`_enabled`). Login + ClickSkillSlot(vid 0) crashed.

---

## 7. Icons (no D3D)

`CGraphicImage::GetIconImage` / `LockRect` / device textures **crashed or returned placeholders**. Do not go through D3D.

### Item icons

Virtual pack path: `icon/item/{vnum:D5}.tga`  
Extract with **`CEterPackManager::Get`** (thiscall: `mgr, virtualPath, strlen, mappedOut, tryDisk=0`). Singleton + Get RVA are recovered into `packMgr` / `packGet`. Mapped blob is a TGA; FarmBot writes `%TEMP%\farmbot-icons\{vnum}.tga` and converts to a PNG data URI.

### Skill icons

Proto stores a **`.sub`** (crop rectangle) pointing at a **`.dds`** atlas. Load `.sub` coordinates, extract the `.dds` from the same pack, crop, PNG. `ClientIconStore` / `SubIcon` reject flat single-color placeholders.

If pack Get fails, the UI still shows name + slot; cache keeps the last good data URI.

---

## 8. When the character is “in game” (do not trust phase)

On Arthion, `CPythonNetworkStream` phase at `net+0xF8` is often **empty** while you are already farming. Logs looked like:

```text
game phase=(empty) inWorld=no hud=hp 2780/2780 lv 21 character=lockedin reason=hud
```

That HUD is enough. Phase-string `Game` and Python `GameWindow` were **false negatives**.

**Enable pots/skills when all of these hold:**

- Native `GetStatus` HUD is valid: level 1–250, maxHp > 0, hp in range (`reason=hud`).
- Player name is non-empty (`character=lockedin`).
- Not on Login / Select / Loading windows if those class names exist.

**Keep blocked** on LoadingWindow / LoginWindow / SelectCharacterWindow, and on login/select/loading phase strings **when they are present**.

Do **not** use:

- `MaxHp > 0` alone at process start (loading can leak leftover stats; cached pot slots then `use_item` and crash).
- `g_openedGameWindow && g_loadFinished` only (that latch often never flipped after a normal enter).

C# helper: `QuickPlayOrchestrator.LooksInWorld`. Native: `ComputeInWorld()` **after** `RefreshHud`, not before.

Always:

- Show cached icons/names immediately in the UI.
- **Scan and use** only in-world.
- Use only **live** cells/slots from that scan, never cache.

---

## 9. Cache (account + character)

Path: `cache/{account}/{character}.json` next to FarmBot.

Store: potion vnum/name/family/tooltip/duration/icon + last slot/count; skill index/name/slot/level/duration/icon. Strip cool/affect.

Merge: live scan is source of truth for slot/count/list; fill empty icon/name from cache by vnum/index.

---

## 10. HUD / HP

`player.GetStatus(POINT_*)` is virtual on Arthion (`IAbstractPlayer` vtable). POINT ids from Python constants (`MAX_HP=6`, so HP=5, SP=7, LEVEL=1, EXP=3, NEXT_EXP=4) or official defaults.

External `ReadProcessMemory` of `m_alPoint` can be **empty** while in-process `GetStatus` works (seen: `name 'lockedin' but m_alPoint is empty` + native HUD `hp 2780/2780`). Trust the **injected** HUD for pots.

XOR mask on raw points is recovered from `GetPoint` (`0xE73AC1DA` family). If HUD looks like shifted slots (level ≈ nextExp), reject it (`PlayerHudDecoder.LooksLikeHud`).

---

## 11. Login / quick play (separate from combat)

Recovered from net Python wrappers + displacements:

- `SetLoginInfo`, `CAccountConnector::Connect`, `DirectEnter`, `SelectCharacter`, `SetLoginPhase`, `SetGamePhase`, `StartGame`.
- Displacements: phase `+0xF8`, login id `+0xB0`, password `+0xC8`, selected slot `+0x797C` (verify on the new EXE).

Quick play is `QuickPlayOrchestrator` (skip intro → login → select → wait load → enter). Combat must stay off until §8 says in-world, even if quick play already returned.

---

## 12. Checklist for a new client

1. Confirm 32-bit PE and Python names (`ClickSkillSlot`, `GetItemIndex`, `SetTarget`, `SendItemUsePacket`).
2. Run layout recovery on the EXE; read the native `loaded …` line; no important RVA may be 0.
3. Inject x86 `FarmBotAction.dll`; unique pipe per slot.
4. Hook Peek/GetMessage; execute combat/use on that thread only.
5. Attack: SetTarget + in-range combo/attack-key/reserve-click. Prove with a farm Start **after** you are on the character.
6. Pots: walk cells 0–179 window 1; classify; `SendItemUsePacket` then `UseItem`; poll ~250 ms in-world.
7. Skills: scan GetSkillIndex/Level; **cast with ClickSkillSlot only**; buff = UI checkbox; delay in C#.
8. Icons: pack `Get` of `icon/item/XXXXX.tga` and skill `.sub`→`.dds` crop. No LockRect.
9. In-world = valid GetStatus HUD + character name (phase string optional).
10. Cache icons/info per account+character; never use cached slots until a live scan.
11. After DLL changes: restart **both** FarmBot and the client.

### Native log lines that mean “it works”

```text
use_item cell=7 send=1 player=0 py=0
use_skill slot=1 click=1 send=0
pots loaded 6 stacks
skills loaded 7
game phase=Game inWorld=yes pots=on skills=on hud=hp 2780/2780 lv 21 character=YourName reason=hud
```

If `click=0` or the client dies on login, you called the wrong native or called it before HUD+name.

---

## 13. Code map

| Concern | Where |
| --- | --- |
| RVA recovery | `src/FarmBot.Core/Combat/PlayerActionLayout.cs` |
| Login RVAs / displacements | `src/FarmBot.Core/Combat/LoginNativeContract.cs` |
| Native calls, scan, use, hooks | `tools/FarmBotAction/FarmBotAction.cpp` |
| x86 build | `tools/FarmBotAction/build.ps1` |
| Inject + layout file | `src/FarmBot.Platform/Client/ActionModuleHost.cs` |
| Potion / skill policy | `PotionController.cs`, `SkillController.cs`, `BotRuntime.cs` |
| In-world helper | `QuickPlayOrchestrator.LooksInWorld` |
| Icons | `ClientIconStore.cs`, `SubIcon`, pack Get in the DLL |
| Per-character cache | `CharacterKitStore.cs` |
| Arthion-only UI | `BotConfig.ClientProfile == Arthion` |
