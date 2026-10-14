using System.ComponentModel;
using System.Diagnostics;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;
using FarmBot.Platform.Native;

namespace FarmBot.Platform.Client;

public sealed class PermittedProcessProvider : IGameStateProvider, IGameStateSource
{
    private static readonly string[] ProcessNames = { "1.metin2client", "1.Debug-metin2client" };

    public string Name => "Live client";
    public bool FromLiveClient => true;
    public string Status { get; private set; } = "live client was not probed yet";

    public GameFrame? Capture(long nowMs)
    {
        Status = Probe();
        return null;
    }

    public WorldSnapshot? GetSnapshot(long nowMs)
    {
        Status = Probe();
        return null;
    }

    public static string Probe()
    {
        Process? process = null;
        foreach (var name in ProcessNames)
        {
            process = Process.GetProcessesByName(name).FirstOrDefault();
            if (process != null)
                break;
        }

        if (process == null)
            return "live client is not running. Entity table is empty until a supported live-state interface supplies entities.";

        var query = TryOpen(process.Id, Win32.ProcessQueryLimitedInformation, out var queryError);
        if (query != IntPtr.Zero)
            Win32.CloseHandle(query);

        var read = TryOpen(process.Id, Win32.ProcessVmRead, out var readError);
        if (read != IntPtr.Zero)
        {
            Win32.CloseHandle(read);
            return $"{process.ProcessName} PID {process.Id}: PROCESS_VM_READ is granted, but release instance offsets are not recovered. Live entity walk is not implemented. The table is not a live client listing.";
        }

        var queryText = query == IntPtr.Zero ? $"QUERY_LIMITED denied ({Win32Name(queryError)})" : "QUERY_LIMITED granted";
        return $"{process.ProcessName} PID {process.Id}: {queryText}; PROCESS_VM_READ denied ({Win32Name(readError)}). Live entities are not available. No privilege or ACL change was attempted.";
    }

    private static IntPtr TryOpen(int processId, uint mask, out int error)
    {
        var handle = Win32.OpenProcess(mask, false, processId);
        error = handle == IntPtr.Zero ? MarshalError() : 0;
        return handle;
    }

    private static int MarshalError() => new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()).NativeErrorCode;

    private static string Win32Name(int error) => error switch
    {
        5 => "Win32 5 ERROR_ACCESS_DENIED",
        0 => "Win32 0",
        _ => "Win32 " + error
    };
}
