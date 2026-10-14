using System.Text;
using FarmBot.Core.Combat;
using FarmBot.Platform.Native;

namespace FarmBot.Platform.Client;

public static class LiveTargetingProbe
{
    public static LiveTargetingReport Inspect()
    {
        var evidence = new List<string>
        {
            "Release player.SetTarget is virtual thiscall: ecx=CPythonPlayer*+4, push force, push vid, call [vtable+0x60].",
            "CPythonPlayer ms_singleton is process-lifetime; null before game init / after teardown.",
            "SetTarget requires NEW_GetMainActorPtr, CanChangeTarget, and IsTargetableInstance.",
            "Game thread = window thread of CPythonApplication::Loop (MessageProcess or Process).",
            "player.Update / mouse __OnPressActor / Python player.SetTarget already run on that thread.",
            "An injected module's pipe worker is a different thread and must not thiscall SetTarget.",
            "This probe is read-only. It does not invoke SetTarget, inject, or post input."
        };

        if (!ProcessMemory.TryAttach(LiveGameStateProvider.ProcessNames, out var memory, out var attachStatus) ||
            memory == null)
        {
            return Fail(attachStatus, evidence);
        }

        using (memory)
        {
            evidence.Add(attachStatus);
            var path = File.Exists(memory.ImagePath)
                ? memory.ImagePath
                : @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";
            if (!File.Exists(path))
                return Fail("running image path is unavailable; cannot recover SetTarget from the live executable", evidence);

            var layout = PlayerActionLayoutRecoverer.Recover(path);
            evidence.AddRange(layout.Evidence);
            if (!layout.HasSetTarget)
                return Fail("release SetTarget RVA was not recovered from the running image", evidence, memory, path);

            uint singletonPtr = 0;
            uint observedTarget = 0;
            uint setTargetVa = 0;
            var filePrologue = layout.SetTargetPrologue;
            var livePrologue = "";
            var bytesMatch = false;
            if (memory.TryReadU32(memory.ImageBase + layout.PlayerSingletonRva, out singletonPtr) && singletonPtr != 0)
            {
                evidence.Add($"CPythonPlayer* ms_singleton = 0x{singletonPtr:X} (image+0x{layout.PlayerSingletonRva:X}).");
                if (layout.TargetVidDisplacement != 0 &&
                    memory.TryReadU32(singletonPtr + layout.TargetVidDisplacement, out observedTarget))
                {
                    evidence.Add($"observed m_dwTargetVID={observedTarget} at player+0x{layout.TargetVidDisplacement:X}.");
                }

                if (layout.SetTargetVtableSlot != 0 &&
                    memory.TryReadU32(singletonPtr + layout.SetTargetThisAdjustment, out var vptr) &&
                    memory.TryReadU32(vptr + layout.SetTargetVtableSlot, out setTargetVa) &&
                    setTargetVa != 0)
                {
                    evidence.Add($"live IAbstractPlayer vptr=0x{vptr:X}; SetTarget impl=0x{setTargetVa:X} ([vptr+0x{layout.SetTargetVtableSlot:X}]).");
                    if (memory.TryRead(setTargetVa, 16, out var liveBytes))
                    {
                        livePrologue = Convert.ToHexString(liveBytes);
                        evidence.Add($"live SetTarget prologue at 0x{setTargetVa:X}: {livePrologue}.");
                    }
                }
            }
            else
            {
                evidence.Add("player singleton is null or unreadable (not in game world, or layout mismatch).");
            }

            var gameThread = FindGameThreadId((uint)memory.ProcessId);
            evidence.Add(gameThread == 0
                ? "client window thread was not found."
                : $"client window / game thread id={gameThread}. Adapter IPC would run on a different tid.");

            var nearby = ReadNearbyFarmable(evidence);
            return new LiveTargetingReport(
                true,
                memory.ProcessId,
                path,
                memory.ImageBase,
                setTargetVa,
                layout.SetTargetThisAdjustment,
                layout.SetTargetVtableSlot,
                bytesMatch,
                filePrologue,
                livePrologue,
                layout.PlayerSingletonRva,
                singletonPtr,
                layout.TargetVidDisplacement,
                observedTarget,
                gameThread,
                nearby.count,
                nearby.vids,
                false,
                false,
                ExternalCommandSurface.RemainingGameThreadDrain,
                evidence);
        }
    }

    private static (int count, string vids) ReadNearbyFarmable(List<string> evidence)
    {
        using var scan = new LiveGameStateProvider();
        var frame = scan.Capture(Environment.TickCount64);
        evidence.Add("live scanner: " + scan.Status);
        if (frame == null)
            return (0, "");
        var farmable = frame.Entities.Where(item => item.IsFarmable).Take(8).ToList();
        var vids = string.Join(",", farmable.Select(item => $"{item.Vid}:{item.Type}"));
        if (farmable.Count > 0)
            evidence.Add("nearby farmable VIDs (scanner only; not selected): " + vids);
        return (farmable.Count, vids);
    }

    private static uint FindGameThreadId(uint processId)
    {
        uint threadId = 0;
        Win32.EnumWindows((hWnd, unused) =>
        {
            _ = unused;
            if (!Win32.IsWindowVisible(hWnd))
                return true;
            var tid = Win32.GetWindowThreadProcessId(hWnd, out var pid);
            if (pid != processId)
                return true;
            var title = new StringBuilder(256);
            Win32.GetWindowText(hWnd, title, title.Capacity);
            if (title.Length == 0)
                return true;
            threadId = tid;
            return false;
        }, IntPtr.Zero);
        return threadId;
    }

    private static LiveTargetingReport Fail(
        string reason,
        List<string> evidence,
        ProcessMemory? memory = null,
        string path = "")
    {
        evidence.Add(reason);
        return new LiveTargetingReport(
            memory != null,
            memory?.ProcessId ?? 0,
            path,
            memory?.ImageBase ?? 0,
            0,
            0,
            0,
            false,
            "",
            "",
            0,
            0,
            0,
            0,
            0,
            0,
            "",
            false,
            false,
            ExternalCommandSurface.RemainingGameThreadDrain,
            evidence);
    }
}
