using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using FarmBot.Platform.Native;

namespace FarmBot.Platform.Client;

public sealed class ProcessMemory : IDisposable
{
    public static readonly uint[] AccessAttempts =
    {
        Win32.ProcessQueryLimitedInformation | Win32.ProcessVmRead,
        Win32.ProcessQueryInformation | Win32.ProcessVmRead,
        Win32.ProcessVmRead
    };

    private IntPtr _handle;

    public int ProcessId { get; }
    public string ProcessName { get; }
    public string ImagePath { get; }
    public uint ImageBase { get; }
    public uint ImageSize { get; }
    public bool Wow64 { get; }

    private ProcessMemory(
        IntPtr handle,
        int processId,
        string processName,
        string imagePath,
        uint imageBase,
        uint imageSize,
        bool wow64)
    {
        _handle = handle;
        ProcessId = processId;
        ProcessName = processName;
        ImagePath = imagePath;
        ImageBase = imageBase;
        ImageSize = imageSize;
        Wow64 = wow64;
    }

    public bool IsOpen => _handle != IntPtr.Zero && !HasExited();

    public static bool TryAttach(IReadOnlyList<string> processNames, out ProcessMemory? memory, out string status)
    {
        memory = null;
        Process? process = null;
        foreach (var name in processNames)
        {
            process = Process.GetProcessesByName(name).FirstOrDefault();
            if (process != null)
                break;
        }

        if (process == null)
        {
            status = "live client process was not found";
            return false;
        }

        var handle = IntPtr.Zero;
        var lastOpenError = 0;
        foreach (var access in AccessAttempts)
        {
            handle = Win32.OpenProcess(access, false, process.Id);
            if (handle != IntPtr.Zero)
                break;
            lastOpenError = LastError();
        }

        if (handle == IntPtr.Zero)
        {
            status = $"{process.ProcessName} PID {process.Id}: OpenProcess(VM_READ) failed ({Win32Name(lastOpenError)}). Run FarmBot at the same integrity as the client (Administrator if the client is High).";
            return false;
        }

        Win32.IsWow64Process(handle, out var wow64);
        if (!TryReadImagePath(handle, process, out var imagePath))
            imagePath = process.ProcessName;
        if (!TryReadMainModule(handle, process.Id, process.ProcessName, out var imageBase, out var imageSize, out var moduleError))
        {
            Win32.CloseHandle(handle);
            status = $"{process.ProcessName} PID {process.Id}: module enumeration failed ({moduleError})";
            return false;
        }

        if (!TryRead(handle, imageBase, 2, out var mz) || mz[0] != (byte)'M' || mz[1] != (byte)'Z')
        {
            Win32.CloseHandle(handle);
            status = $"{process.ProcessName} PID {process.Id}: ReadProcessMemory of image base 0x{imageBase:X} did not return MZ";
            return false;
        }

        memory = new ProcessMemory(handle, process.Id, process.ProcessName, imagePath, imageBase, imageSize, wow64);
        status = $"{process.ProcessName} PID {process.Id}: RPM verified MZ at image base 0x{imageBase:X} ({(wow64 ? "WOW64" : "native")}, 0x{imageSize:X} bytes)";
        return true;
    }

    public bool TryReadU32(uint address, out uint value)
    {
        value = 0;
        if (!TryRead(address, 4, out var bytes))
            return false;
        value = BitConverter.ToUInt32(bytes, 0);
        return true;
    }

    public bool TryReadF32(uint address, out float value)
    {
        value = 0;
        if (!TryRead(address, 4, out var bytes))
            return false;
        value = BitConverter.ToSingle(bytes, 0);
        return true;
    }

    public bool TryRead(uint address, int count, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        return _handle != IntPtr.Zero && TryRead(_handle, address, count, out bytes);
    }

    public bool HasExited()
    {
        try
        {
            using var process = Process.GetProcessById(ProcessId);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    public void Dispose()
    {
        if (_handle == IntPtr.Zero)
            return;
        Win32.CloseHandle(_handle);
        _handle = IntPtr.Zero;
    }

    private static bool TryRead(IntPtr handle, uint address, int count, out byte[] bytes)
    {
        bytes = new byte[count];
        if (handle == IntPtr.Zero || count <= 0)
            return false;
        if (!Win32.ReadProcessMemory(handle, (UIntPtr)address, bytes, (UIntPtr)count, out var read))
            return false;
        return read.ToUInt32() == (uint)count;
    }

    private static bool TryReadImagePath(IntPtr handle, Process process, out string path)
    {
        path = "";
        var buffer = new StringBuilder(1024);
        var size = (uint)buffer.Capacity;
        if (Win32.QueryFullProcessImageName(handle, 0, buffer, ref size))
        {
            path = buffer.ToString();
            return path.Length > 0;
        }

        try
        {
            path = process.MainModule?.FileName ?? "";
            return path.Length > 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool TryReadMainModule(IntPtr handle, int processId, string processName, out uint imageBase, out uint imageSize, out string error)
    {
        imageBase = 0;
        imageSize = 0;
        error = "";
        if (TryEnumProcessModules(handle, processName, out imageBase, out imageSize, out error))
            return true;
        if (TryToolhelpModule((uint)processId, processName, out imageBase, out imageSize))
        {
            error = "";
            return true;
        }

        return false;
    }

    private static bool TryEnumProcessModules(IntPtr handle, string processName, out uint imageBase, out uint imageSize, out string error)
    {
        imageBase = 0;
        imageSize = 0;
        error = "";
        foreach (var filter in new[] { Win32.ListModules32Bit, Win32.ListModulesAll })
        {
            var modules = new IntPtr[512];
            var bytes = (uint)(IntPtr.Size * modules.Length);
            if (!Win32.EnumProcessModulesEx(handle, modules, bytes, out var needed, filter))
            {
                error = Win32Name(LastError());
                continue;
            }

            var count = (int)(needed / IntPtr.Size);
            for (var i = 0; i < count && i < modules.Length; i++)
            {
                var name = new StringBuilder(260);
                Win32.GetModuleBaseName(handle, modules[i], name, (uint)name.Capacity);
                var moduleName = name.ToString();
                var matches = moduleName.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase)
                    || moduleName.Equals(processName, StringComparison.OrdinalIgnoreCase)
                    || i == 0;
                if (!matches)
                    continue;
                if (!Win32.GetModuleInformation(handle, modules[i], out var info, (uint)Marshal.SizeOf<Win32.MODULEINFO>()))
                {
                    error = Win32Name(LastError());
                    continue;
                }

                imageBase = unchecked((uint)info.lpBaseOfDll.ToInt64());
                imageSize = info.SizeOfImage;
                if (imageBase != 0)
                    return true;
            }
        }

        if (string.IsNullOrEmpty(error))
            error = "main module not listed";
        return false;
    }

    private static bool TryToolhelpModule(uint processId, string processName, out uint imageBase, out uint imageSize)
    {
        imageBase = 0;
        imageSize = 0;
        var snapshot = Win32.CreateToolhelp32Snapshot(Win32.SnapModule | Win32.SnapModule32, processId);
        if (snapshot == IntPtr.Zero || snapshot == (IntPtr)(-1))
            return false;
        try
        {
            var entry = new Win32.MODULEENTRY32W { dwSize = (uint)Marshal.SizeOf<Win32.MODULEENTRY32W>() };
            if (!Win32.Module32FirstW(snapshot, ref entry))
                return false;
            do
            {
                var moduleName = entry.szModule ?? "";
                if (!moduleName.StartsWith(processName, StringComparison.OrdinalIgnoreCase))
                    continue;
                imageBase = unchecked((uint)entry.modBaseAddr.ToInt64());
                imageSize = entry.modBaseSize;
                if (imageBase != 0)
                    return true;
            }
            while (Win32.Module32NextW(snapshot, ref entry));
        }
        finally
        {
            Win32.CloseHandle(snapshot);
        }

        return false;
    }

    private static int LastError() => new Win32Exception(Marshal.GetLastWin32Error()).NativeErrorCode;

    private static string Win32Name(int error) => error switch
    {
        5 => "Win32 5 ERROR_ACCESS_DENIED",
        299 => "Win32 299 ERROR_PARTIAL_COPY",
        _ => "Win32 " + error
    };
}
