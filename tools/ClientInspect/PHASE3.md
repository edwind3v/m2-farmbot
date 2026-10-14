# Phase 3 — confirmed entity structures and remaining assumptions

Phase 2 mapped the entity classes and recorded a reproducible `OpenProcess(PROCESS_VM_READ)` failure from a Medium inspector. This note corrects the security reading, traces store/retrieve and stone/position paths in the debug client, and states what is shared with the live release image.

Sources used:

- Static RE of `E:\TeraBoxDownload\1.Client\1.Debug-metin2client.exe` (preferred base `0x400000`, no ASLR).
- Matching ClientSRC whose PDB path is baked into that debug image: `E:\TeraBoxDownload\2.ClientSRC\source\UserInterface\`.
- String/constant comparison with `1.metin2client.exe` (ASLR on).
- Phase 2 live access matrix (Medium inspector, High target). No new privilege, ACL, kernel, packet, or injection work.

No runtime instance table was read. Entity enumeration is **not** claimed.

## 1. Security correction

Windows default mandatory policy is `SYSTEM_MANDATORY_LABEL_NO_WRITE_UP` (`0x1`). `SYSTEM_MANDATORY_LABEL_NO_READ_UP` (`0x2`) is a separate SACL flag and is **not** the default.

Phase 2 overstated this: Medium-to-High integrity does **not** universally prohibit `PROCESS_VM_READ`. An unread SACL must not be treated as containing `NO_READ_UP`. An unread DACL must not be treated as containing a particular deny ACE.

| Observation | Status |
| --- | --- |
| Inspector Medium / `TokenElevationTypeLimited`; target High / `TokenElevationTypeFull` | confirmed (Phase 2) |
| `NtQueryInformationProcess(ProcessProtectionInformation)` Type=0 (not PPL) | confirmed |
| `OpenProcess(PROCESS_VM_READ)` → Win32 5 from this Medium inspector | confirmed |
| `OpenProcess(READ_CONTROL)` → Win32 5; DACL unread | confirmed |
| SACL / mandatory label ACE unread | confirmed (not observed) |
| Default MIC is `NO_WRITE_UP` | confirmed (OS contract) |
| Target SACL contains `NO_READ_UP` | **not observed** |
| Target DACL contains a deny ACE | **not observed** |
| CShield string or DLL in the client folder / image | absent (not a cause) |
| Live entity walk | **not performed** |

The supported statement is: this Medium inspector, without `SeDebugPrivilege` and without changing any ACL, cannot open `PROCESS_VM_READ` on this process. The security *reason* is unread.

## 2. How `CPythonCharacterManager` stores and retrieves `CInstanceBase`

Source: `PythonCharacterManager.h` / `.cpp`. Debug binary at `CreateInstance` `0x4EA2F0`, lookup/register `0x4EA3B0`, delete string site `0x4EA540`, select `0x4EA9E0`.

```
CreateInstance(SCreateData& data):
    inst = RegisterInstance(data.m_dwVID)          // map.insert; NULL if VID exists
    if !inst: log "ALREADY EXIST"; return NULL
    if !inst->Create(data):
        log "CreateInstance VID[%d] Race[%d]"
        DeleteInstance(data.m_dwVID)
        return NULL
    if data.m_isMain: SelectInstance(data.m_dwVID)
    return inst

RegisterInstance(VID):
    if m_kAliveInstMap.find(VID) != end: return NULL
    inst = CInstanceBase::New()                    // CDynamicPool<CInstanceBase>
    m_kAliveInstMap.insert(VID, inst)
    return inst

GetInstancePtr(VID):
    it = m_kAliveInstMap.find(VID)
    return it == end ? NULL : it->second

DeleteInstance(VID) / SelectInstance(VID):
    find VID; missing → "no vid by %d"
```

`TCharacterInstanceMap` is `std::map<DWORD, CInstanceBase*>`. Dead fading instances move to `m_kDeadInstList`. Main / pick / bind are raw pointers (`m_pkInstMain`, `m_pkInstPick`, `m_pkInstBind`).

**Debug binary:** every create/select/delete path does `this+0x20` before the find. That displacement is the compiled `m_kAliveInstMap` on this debug image. It is **not** claimed for the ASLR release process.

Python `chr.HasInstance(vid)` is the same lookup: `GetInstancePtr != NULL` → `Py_BuildValue("i", 0|1)`.

Phase 2 labeled `0x4EA3B0` as Delete. That address is the VID lookup / register helper used *by* CreateInstance (`call` from `0x4EA311`). Delete’s log site is later (`0x4EA540`).

## 3. VID, type, race/VNUM, position

```
SNetworkActorData (server-fed cache)
        │  m_bType, m_dwVID, m_dwRace, m_stName, m_lCurX/Y, …
        ▼
__AppendCharacterManagerActor
        │  copies into SCreateData
        ▼
CPythonCharacterManager::CreateInstance
        ▼
CInstanceBase::Create
        SetInstanceType(m_bType)     → CActorInstance::m_eActorType
        SetRace(m_dwRace)            → m_dwRace; proto lookup → m_eRaceType
        SetVirtualID(m_dwVID)        → graphic-object VID
        SCRIPT_SetPixelPosition((float)m_lPosX, (float)m_lPosY)
```

| Name | Meaning | Confirmed from |
| --- | --- | --- |
| VID | Unique live key. Map key. `GetVirtualID()`. | source + CreateInstance `[eax+0x14]` + both images’ log strings |
| Type | `CActorInstance::EType` on the graphic object. Server `m_bType`. | source + Python exports + debug `SCreateData+0` |
| Race | `m_dwRace`. Appearance / proto VNUM. | source + CreateInstance `[eax+0x18]` |
| Virtual number | `m_dwVirtualNumber`. Special-case VNUMs (doors, resources). | source; not a Python export of TYPE_STONE |
| Race-type | `m_eRaceType` from `CPythonNonPlayer::GetInstanceType(race)` / `TMobTable.bType`. Defaults to `TYPE_PC` if the proto is missing. | source |
| Pixel position | Graphic-object `TPixelPosition` after create. | source + debug copy of 12 bytes |

`CActorInstance::EType` (sequential; Python exports in **bold**):

| Value | C++ | Python |
| --- | --- | --- |
| 0 | `TYPE_ENEMY` | **INSTANCE_TYPE_ENEMY** |
| 1 | `TYPE_NPC` | **INSTANCE_TYPE_NPC** |
| 2 | `TYPE_STONE` | *(not exported)* |
| 3 | `TYPE_WARP` | — |
| 4 | `TYPE_DOOR` | — |
| 5 | `TYPE_BUILDING` | **INSTANCE_TYPE_BUILDING** |
| 6 | `TYPE_PC` | **INSTANCE_TYPE_PLAYER** |
| 7 | `TYPE_POLY` | — |
| 8 | `TYPE_HORSE` | — |
| 9 | `TYPE_GOTO` | — |
| 10 | `TYPE_OBJECT` | **INSTANCE_TYPE_OBJECT** |

Those five Python integers are identical in the release image (`0x4B3DF5`–`0x4B3E1D`).

**SCreateData** displacements verified on the debug CreateInstance / Append path:

| Offset | Field | Width |
| --- | --- | --- |
| +0x00 | `m_bType` | BYTE |
| +0x14 | `m_dwVID` | DWORD |
| +0x18 | `m_dwRace` | DWORD |

`m_isMain` is the post-create `SelectInstance` flag. The debug image tests a byte at `SCreateData+0x74` for that. Other `SCreateData` / `CInstanceBase` members are source-named only; their compiled offsets are **not** claimed.

**Inferred (not claimed as live layout):** full `CInstanceBase` after many `#ifdef`s; `std::map` node internals; release displacements.

## 4. `StoneDetect` and stone classification

`StoneDetect` is **not** a classifier. It is a `CPythonNetworkStream::ServerCommand` token (`PythonNetworkStreamCommand.cpp`, debug dispatcher `0x6064A0`, format push `0x606FFF`).

```
ServerCommand("StoneDetect <vid> <distance> <angle>"):
    require 4 tokens
    VID      = atoi(arg1)
    distance = atoi(arg2)          // 0..3
    angle    = fmod(540 - atof(arg3), 360)
    log "StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]"
    inst = GetInstancePtr(VID)
    if !inst: "Not Exist Instance"; return
    inst->NEW_GetPixelPosition(&pos)
    pos.y *= -1                    // effect space
    distance 0 → firecracker find_out.mse
    distance 1..3 → compass appear_small/middle/large.mse
```

Classification of a Metin stone is `CInstanceBase::IsStone()` → `CActorInstance::IsStone()` → `m_eActorType == TYPE_STONE` (2). That type arrives from the server on `SNetworkActorData.m_bType` and is written by `SetInstanceType` during `Create`. `IsStone` is **not** a Python method (hence no `IsStone` string). `USE_CREATE_STONE` is an item-use constant, not an entity type.

Nearby stones that the server has already spawned are therefore ordinary `CInstanceBase` rows with type 2. `StoneDetect` only draws a compass on a VID the **server** names.

## 5. `GetPixelPosition` and `GetMainCharacterPosition`

`TPixelPosition` is `typedef D3DXVECTOR3` (`MapType.h`): three floats, `x`, `y`, `z`.

### `chr.GetPixelPosition` — debug `0x4F9F20`

- Python method table flags = 1 (`METH_VARARGS`).
- Convention: `__cdecl PyObject* (PyObject* self, PyObject* args)`.
- Parses `args[0]` as VID (`PyTuple_GetInteger`).
- `CPythonCharacterManager::Instance().GetInstancePtr(vid)` via the character-manager interface vtable slot +8 (singleton result `+4`).
- Native `CInstanceBase::NEW_GetPixelPosition` — debug `0x4D5E10`: `__thiscall void (TPixelPosition* out)`, `ret 4`.
- That native adds `0x1F8` (compiled `m_GraphicThingInstance`) and copies 12 bytes.
- `CActorInstance::NEW_GetCurPixelPositionRef`: `x = +m_x`, `y = -m_y`, `z = +m_z`.
- Return: `Py_BuildValue("fff", x, y, z)` (format string `0xBD7600` = `fff`; `cvtss2sd` of the three floats).

### `player.GetMainCharacterPosition` — debug `0x548D60`

- Same `METH_VARARGS` / `__cdecl PyCFunction`. No VID argument.
- `CPythonPlayer::NEW_GetMainActorPosition(&pos)`:
  - if main `CInstanceBase` exists → `NEW_GetPixelPosition`
  - else camera center
- Return: `Py_BuildValue("fff", x, y, z)` (format `0xBE0CA8` = `fff`).

Graphic `GetPixelPosition` (no Y flip) exists on `CActorInstance` but is **not** what the Python wrappers call.

Network spawn/move positions stay `LONG` x/y on `SNetworkActorData` / `SCreateData`. After `Create`, the live coordinate the client scripts use is the flipped-Y float triple.

## 6. Debug vs live release — what is demonstrably shared

Shared **string / RTTI / constant** evidence (present in both images):

- `.?AVCPythonCharacterManager@@`, `.?AVCInstanceBase@@`, `.?AVCNetworkActorManager@@`
- `CPythonCharacterManager::CreateInstance VID[%d] Race[%d]`
- `CNetworkActorManager::__AppendCharacterManagerActor(%d, %s)`
- `DeleteCharacterInstance: no vid by %d`
- `StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]`
- `GetPixelPosition`, `GetMainCharacterPosition`, `HasInstance`, `IsEnemy`
- `INSTANCE_TYPE_ENEMY=0`, `NPC=1`, `BUILDING=5`, `PLAYER=6`, `OBJECT=10`

Debug-only (absent in release):

- `CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID` (and similar move/update/sync logs)

Not shared:

- Function VAs. Release has ASLR (`dllChars 0x8140`). Debug VAs do not apply to a live `1.metin2client` PID.
- Compiled member displacements (`this+0x20`, `+0x1F8`). Those are debug-image facts.

The matching ClientSRC is the debug PDB’s source tree. The release executable in the same client folder carries the same exported names and type integers; that is the evidence it implements the same model. It is **not** a live proof that a given offset is valid in the running release process.

## 7. Confirmed vs inferred vs unresolved

**Confirmed (binary and/or matching source, as marked above).** Manager is a VID→`CInstanceBase*` map. Create/delete/select key on VID. Type/VID/race ride together from the network actor into `SCreateData`. Stones are `TYPE_STONE=2`. `StoneDetect` is a server command that looks up a VID. Python positions are `fff` with Y flipped. Medium inspector VM_READ is denied; DACL/SACL unread; not PPL; default MIC is `NO_WRITE_UP`.

**Inferred (do not treat as live offsets).** `std::map` node layout. Full `CInstanceBase` / `CActorInstance` member map. Release displacements. That a High-integrity inspector would be granted VM_READ.

**Unresolved.** Why this Medium inspector is denied VM_READ. Whether a same-integrity reader is granted VM_READ. Live pointer to `CPythonCharacterManager`. Whether the running release build used the same `#ifdef` set as debug (ACCE / quiver / country flag change `SCreateData` size). Live proof that nearby stones currently have type 2.

## 8. Proposed test plan — permitted live-state acquisition

Keep the app external. Do not capture packets. Do not enable `SeDebugPrivilege`. Do not edit the process DACL/SACL. Do not inject, hook, or load a driver. Do not implement a walker until a read handle exists.

| Step | What | Pass | Fail / stop |
| --- | --- | --- | --- |
| A | Re-run the existing OpenProcess matrix from this Medium inspector | same grants/denies as Phase 2 | record any change; do not “fix” it |
| B | Launch ClientInspect **already** at High integrity (user elevation of *this* tool only). Repeat the same masks. Do not enable privileges | if `PROCESS_VM_READ` is granted, MIC-alone-was-not-the-blocker is confirmed | if still 5, MIC was not sufficient explanation; go to C |
| C | If `READ_CONTROL` is granted at that integrity, dump the process SD (DACL **and** SACL / mandatory label) | report the actual ACEs; only then may `NO_READ_UP` or a deny ACE be stated | if still 5, stop; descriptors remain unread |
| D | If and only if VM_READ is granted: `ReadProcessMemory` of a **known on-disk** string at `imageBase + rva` (for example `INSTANCE_TYPE_ENEMY`) | proves RPM, not entity walk | if 5/299, stop; do not invent a module list |
| E | Only after D: a later milestone may resolve the singleton and walk `m_kAliveInstMap` | claim enumeration only if that walk returns real VID/type/race/position rows | until then, “enumeration works” is false |
| F | Owner-permitted alternative: an in-client export (this source tree) that prints `GetInstancePtr` / type / race / `NEW_GetPixelPosition` | validates the model without crossing the process ACL | out of scope for this inspector |

Step E is **not** implemented. Step F is **not** implemented. Packet capture and server communication are out of scope.

## Repro

```
dotnet test tools\ClientInspect.Tests\ClientInspect.Tests.csproj -c Release
dotnet run --project tools\ClientInspect\ClientInspect.csproj -c Release -- --report tools\ClientInspect\client-inspect-phase3.txt
```

Phase 1: `FINDINGS.md`. Phase 2: `PHASE2.md`. Function VAs: debug image at preferred base `0x400000`.
