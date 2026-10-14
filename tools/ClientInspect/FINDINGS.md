# ClientInspect findings

Phase 1 snapshot. Phase 2 (exact flags, Toolhelp variants, entity call graph) is in `PHASE2.md`. Phase 3 (MIC correction, confirmed structures, StoneDetect, positions, permitted test plan) is in `PHASE3.md`.

Generated from a live scan of `1.metin2client.exe` (PID 15856) on 2026-10-14, plus static strings in that image and `1.Debug-metin2client.exe`. Field offsets are not listed because they were not recovered.

## Process

| Item | Result |
| --- | --- |
| Process | `1.metin2client` PID 15856 |
| Image | `E:\TeraBoxDownload\1.Client\1.metin2client.exe` |
| Architecture | 32-bit PE (`IMAGE_FILE_MACHINE_I386`), running as WOW64 |
| Image base in file | `0x400000` |
| ASLR / NX | on (`dllChars 0x8140`) |
| Inspector | x64, Medium integrity, not elevated |

## User-mode memory access

Each `OpenProcess` right was requested alone.

| Right | Result |
| --- | --- |
| `PROCESS_QUERY_LIMITED_INFORMATION` (0x1000) | granted |
| `SYNCHRONIZE` (0x100000) | granted |
| `PROCESS_QUERY_INFORMATION` (0x400) | denied, Win32 5 |
| `PROCESS_VM_READ` (0x10) | denied, Win32 5 |
| `PROCESS_VM_OPERATION` (0x8) | denied, Win32 5 |
| `PROCESS_DUP_HANDLE` (0x40) | denied, Win32 5 |

`CreateToolhelp32Snapshot` of modules also failed with Win32 5. The process is visible and its path can be read. Its pages cannot.

`OpenProcess(PROCESS_VM_READ)` returned Win32 5 from this Medium inspector. That is the measured access result, not a proof that a DACL ACE or `NO_READ_UP` is present — those descriptors were not read. This diagnostic does not change any ACL, does not load a driver, and does not walk instance objects.

## How nearby entities are represented

These names are present as strings in the client. They describe the object model. They are not memory offsets.

- `CPythonCharacterManager` owns nearby instances. It is a `CSingleton`. Create / delete / select all key on **VID**.
- Each nearby character, monster, or stone is a `CInstanceBase`.
- `CNetworkActorManager` is the server-fed actor list. It appends an actor as `(VID, name)` and, in the debug build, move / update / remove also take `dwVID`.
- The Python `chr` / `chrmgr` / `player` modules expose the same table: `HasInstance`, `GetNameByVID`, `GetInstanceType`, `GetPixelPosition`, `GetMainVID`, `GetMainCharacterPosition`, `GetTargetVID`, `GetPickedVID`, `IsEnemy`, `IsNPC`, `IsDead`, `GetMaxHPByVID`.
- Instance types named in this client: `PLAYER`, `NPC`, `ENEMY`, `BUILDING`, `OBJECT`. `INSTANCE_TYPE_STONE` is not a string in the binary.
- Metin stones are addressed by VID through `StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]`.
- Ground loot is `CPythonItem::SGroundItemInstance`, a separate pool.

The client fills a `CInstanceBase` when it handles character-add and `TPacketGCCharacterAdditionalInfo` (`name`, `vid`, `race`). That is the in-process copy of “who is around us.” Reading those objects from another process requires `PROCESS_VM_READ`, which is currently denied.

## Supported next interfaces

1. **Authorized client export (preferred).** A small change in a client you own that publishes the already-resolved VID / name / type / position table over the existing FarmBot pipe. No kernel code. No ACL change. Offsets stay unnecessary because the client already has the typed objects.
2. **Server-side feed.** You administer the game server. A process you control can emit the same nearby-actor list the channel already sends to the client. That is not sniffing the live TCP session.
3. **Screen recognition.** Possible without process access. Earlier OCR of this client’s nameplates failed on the bitmap font. Color clustering can guess on-screen labels only. It does not provide VID, race, or true world coordinates, so combat targeting from it is weak.
4. **External `ReadProcessMemory`.** Blocked on this running process from a Medium-integrity inspector. Not used, and not bypassed.

Kernel-level inspection and protection bypasses are out of scope.

## How to run the diagnostic

```
dotnet run --project "tools\ClientInspect\ClientInspect.csproj" -c Release
```

Or `ClientInspect.exe`. Use **Scan running client**. `--report path.txt` writes the same text without the window.
