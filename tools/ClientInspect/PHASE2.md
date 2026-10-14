# Phase 2 — access diagnosis and entity architecture

Phase 1 (`FINDINGS.md`) recorded a Medium-integrity inspector receiving `ERROR_ACCESS_DENIED` on `PROCESS_VM_READ`. This note separates what was measured from what was inferred, maps the three entity classes in `1.Debug-metin2client.exe`, and restates what is still unknown.

Live process: `1.metin2client` PID 15856, `E:\TeraBoxDownload\1.Client\1.metin2client.exe`, WOW64.  
Inspectors: x64 and x86, both Medium / `TokenElevationTypeLimited`, session 1.  
Static map: `1.Debug-metin2client.exe` at preferred base `0x400000` (no ASLR in that file). Live release has ASLR (`dllChars 0x8140`); those VAs do not apply to PID 15856.

No packet capture, no listen port, no ACL change, no privilege enable, no kernel component.

## 1. Inspector security context (confirmed)

| Item | x64 inspector | x86 inspector |
| --- | --- | --- |
| Architecture | x64 | x86 (WOW64) |
| Integrity | Medium (RID 8192) | Medium |
| Elevation | `TokenElevationTypeLimited`, `TokenIsElevated=false` | same |
| Session | 1 | 1 |
| `SeDebugPrivilege` | absent | absent |
| `SeRelabelPrivilege` | absent | absent |
| `SeTcbPrivilege` | absent | absent |

The token was read with `OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY)` only. Privileges were not adjusted.

## 2. OpenProcess masks and Win32 errors (confirmed)

Each mask was requested alone, then two combinations. Result is identical from x64 and x86.

| Request | Mask | Result |
| --- | --- | --- |
| `PROCESS_QUERY_LIMITED_INFORMATION` | `0x00001000` | granted |
| `SYNCHRONIZE` | `0x00100000` | granted |
| `PROCESS_QUERY_INFORMATION` | `0x00000400` | denied, Win32 **5** `ERROR_ACCESS_DENIED` |
| `PROCESS_VM_READ` | `0x00000010` | denied, 5 |
| `PROCESS_VM_OPERATION` | `0x00000008` | denied, 5 |
| `PROCESS_SET_INFORMATION` | `0x00000200` | denied, 5 |
| `PROCESS_DUP_HANDLE` | `0x00000040` | denied, 5 |
| `READ_CONTROL` | `0x00020000` | denied, 5 |
| `QUERY_LIMITED \| READ_CONTROL` | `0x00021000` | denied, 5 |
| `QUERY_INFORMATION \| VM_READ` | `0x00000410` | denied, 5 |

`QueryFullProcessImageName` and `IsWow64Process` succeed on the limited handle. `ReadProcessMemory` was not called because the VM_READ handle never opened.

## 3. Target security queries (confirmed vs unresolved)

With `PROCESS_QUERY_LIMITED_INFORMATION`:

| Query | Result | Status |
| --- | --- | --- |
| `OpenProcessToken(TOKEN_QUERY)` on the target | opened | confirmed |
| Target integrity | **High** | confirmed |
| Target elevation | `TokenElevationTypeFull` | confirmed |
| `NtQueryInformationProcess(ProcessProtectionInformation=61)` | NTSTATUS 0, **Type=0 (none)**, Signer=0 | confirmed: not PPL |
| `GetSecurityInfo` / `READ_CONTROL` | Win32 5 | **DACL not observed** |
| Live module list | see snapshots | not observed |

**Confirmed.** Target token is High / `TokenElevationTypeFull`. Inspector is Medium / `TokenElevationTypeLimited`. PPL Type=0. `OpenProcess(PROCESS_VM_READ)` returned Win32 5. DACL and SACL were not read.

**Corrected implication (Phase 3).** Windows default mandatory policy is `SYSTEM_MANDATORY_LABEL_NO_WRITE_UP`. `NO_READ_UP` is a separate SACL flag and was **not** observed. Medium-to-High integrity alone does not universally prohibit `PROCESS_VM_READ`. An unread SACL must not be treated as containing `NO_READ_UP`. An unread DACL must not be treated as containing a particular deny ACE.

**Unresolved.** Why this specific Medium inspector is denied VM_READ: DACL ACE, optional `NO_READ_UP`, a named usermode guard, or something else. CShield is **not** a string or DLL in `E:\TeraBoxDownload\1.Client` and not a string in `1.metin2client.exe`. Naming CShield as the cause is an assumption.

**Not claimed.** “External memory reading is impossible on this OS.” The measurement is: this Medium inspector, without `SeDebugPrivilege`, received `ERROR_ACCESS_DENIED` on VM_READ. A same-integrity reader was not tested here. See `PHASE3.md`.

## 4. Why module enumeration failed

| Snapshot | Mask | x64 | x86 |
| --- | --- | --- | --- |
| `TH32CS_SNAPPROCESS` | `0x2` | handle opened | handle opened |
| `TH32CS_SNAPMODULE` | `0x8` | create **5** | create **5** |
| `TH32CS_SNAPMODULE32` | `0x10` | create **5** | create **5** |
| `TH32CS_SNAPMODULE \| TH32CS_SNAPMODULE32` | `0x18` | create **5** | create **5** |

`ERROR_PARTIAL_COPY` (299) never appeared. That is the documented failure of a 64-bit Toolhelp walk of a WOW64 module list without `TH32CS_SNAPMODULE32`. It is **not** what happened.

Microsoft’s contract for another-process module snapshot is `PROCESS_QUERY_INFORMATION | PROCESS_VM_READ`. Both of those masks are denied. `TH32CS_SNAPPROCESS` does not need VM_READ and works.

**Confirmed.** Module enumeration failed because the snapshot create is access-denied, on both inspector architectures.  
**Not confirmed.** “The x64 inspector used the wrong Toolhelp flag.”

## 5. Entity-management architecture (static)

Call graph from string xrefs in `1.Debug-metin2client.exe` (preferred base `0x400000`):

```
CPythonNetworkStream game-phase dispatch   0x612CF0
        │
        ├─ character-add / additional-info  0x648F30
        │        (TPacketGCCharacterAdditionalInfo name=%s vid=%d race=%d)
        │
        ├─ RecvCharacterDelete               0x649870  →  CNetworkActorManager::RemoveActor   0x5FD310
        ├─ RecvCharacterMove                 0x649B60  →  CNetworkActorManager::MoveActor     0x5FD870
        ├─ SyncActor caller                  0x649A30  →  CNetworkActorManager::SyncActor     0x5FDA60
        └─ UpdateActor caller                0x64A050  →  CNetworkActorManager::UpdateActor   0x5FD5C0

CNetworkActorManager::__AppendCharacterManagerActor   0x5FE540
        callers: 0x5FD470, 0x5FDD40, 0x5FE9A0, 0x5FEA40
        │
        └─ CPythonCharacterManager::CreateInstance    0x4EA2F0
                 callers: 0x4F7C00, 0x5FE540
                 VID + race; rejects duplicate VID
                 DeleteCharacterInstance 0x4EA3B0
                 SelectCharacterInstance 0x4EA9E0
```

Class relationships (RTTI + those calls):

- `CNetworkActorManager` is the VID-keyed server-side actor cache (append / move / update / sync / remove).
- `__AppendCharacterManagerActor(%d, %s)` copies a network actor into `CPythonCharacterManager` as a `CInstanceBase`.
- `CPythonCharacterManager` is a `CSingleton` / `IAbstractCharacterManager`. Instances are created, selected, and deleted **by VID**.
- Each nearby visible actor is a `CInstanceBase` (also pooled as `CDynamicPool<CInstanceBase>`).
- Ground loot is a different pool: `CPythonItem::SGroundItemInstance`.

Python bindings (`chr` / `chrmgr` / `player`) name the same table: `HasInstance`, `GetNameByVID`, `GetInstanceType`, `GetPixelPosition`, `GetMainVID`, `GetMainCharacterPosition`, `GetTargetVID`, `GetPickedVID`, `IsEnemy`, `IsNPC`, `IsDead`, `GetMaxHPByVID`. Those names live in `.rdata` and have no `push offset` xref in `.text` (method-table pointers, not format strings).

## 6. Enemies versus Metin stones

`PyModule_AddIntConstant`-style sites (`push imm8` then the name pointer) at `0x4F71F5`–`0x4F7269`:

| Name | Value |
| --- | --- |
| `INSTANCE_TYPE_ENEMY` | **0** |
| `INSTANCE_TYPE_NPC` | **1** |
| `INSTANCE_TYPE_BUILDING` | **5** |
| `INSTANCE_TYPE_PLAYER` | **6** |
| `INSTANCE_TYPE_OBJECT` | **10** |

`INSTANCE_TYPE_STONE` and `IsStone` are not strings in either image.

`StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]` is the only stone-specific recovered name. It keys on VID, distance, and angle.

**Confirmed.** Ordinary monsters are the `ENEMY` / `IsEnemy` / type `0` path. Stones are tracked by VID through `StoneDetect`.  
**Unresolved.** Whether a stone’s `CInstanceBase` type byte is 0, 2 (classic `CHAR_TYPE_STONE`, not exported), 10, or something else. Runtime instances were not read.

## 7. Positions

**Confirmed.**

- Network moves are VID-keyed: `MoveActor(dwVID)`, `SyncActor(dwVID)`.
- The Python/C++ surface for coordinates is `GetPixelPosition` and `GetMainCharacterPosition`.
- Additional-info and append carry identity (`name`, `vid`, `race`) before the instance is fully usable.

**Unresolved.** DWORD vs float, graphic-object vs actor-data, and the size of the vector `GetPixelPosition` writes. No member offset is claimed.

## 8. External reader viability (this milestone)

Permitted interfaces that **worked** from this inspector:

- Find the process by name
- `PROCESS_QUERY_LIMITED_INFORMATION` (path, WOW64, protection info, target token)
- `SYNCHRONIZE`
- `TH32CS_SNAPPROCESS`
- Static read of the on-disk PE

Permitted interfaces that **failed** from this Medium inspector:

- `PROCESS_VM_READ` and every combination that includes it
- `READ_CONTROL` (so the DACL/SACL were not dumped)
- Every Toolhelp module snapshot

An external `CInstanceBase` walker needs `ReadProcessMemory`. That handle is not available to this Medium inspector against this process. The denied masks are established. The security *reason* is not: default MIC is write-up, not read-up, and the DACL/SACL were unread.

A same-integrity or explicitly authorized reader, or an in-client export of the already-typed VID table, is outside this diagnostic and was not implemented.

## Repro

```
dotnet test tools\ClientInspect.Tests\ClientInspect.Tests.csproj -c Release
dotnet run --project tools\ClientInspect\ClientInspect.csproj -c Release -- --report tools\ClientInspect\client-inspect-phase2.txt
tools\ClientInspect\publish-x86\ClientInspect.exe --report tools\ClientInspect\client-inspect-x86.txt
```

Phase 1 narrative: `FINDINGS.md`. Live dumps: `client-inspect-phase2.txt`, `client-inspect-x86.txt`.
