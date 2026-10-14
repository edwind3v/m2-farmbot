using System.Runtime.InteropServices;
using System.Text;

namespace ClientInspect.Analysis;

internal static class Native
{
    public const uint ProcessQueryLimitedInformation = 0x1000;
    public const uint ProcessQueryInformation = 0x0400;
    public const uint ProcessVmRead = 0x0010;
    public const uint ProcessVmOperation = 0x0008;
    public const uint ProcessDupHandle = 0x0040;
    public const uint ProcessSetInformation = 0x0200;
    public const uint Synchronize = 0x00100000;
    public const uint ReadControl = 0x00020000;
    public const uint StandardRightsRead = 0x00020000;
    public const uint TokenQuery = 0x0008;
    public const int TokenElevationClass = 20;
    public const int TokenIntegrityLevel = 25;
    public const int TokenElevationTypeClass = 18;
    public const int TokenSessionId = 12;
    public const int TokenPrivilegesClass = 3;
    public const uint SnapModule = 0x00000008;
    public const uint SnapModule32 = 0x00000010;
    public const uint SnapProcess = 0x00000002;
    public const int ProcessProtectionInformation = 61;
    public const int SeKernelObject = 6;
    public const uint DaclSecurityInformation = 4;
    public const uint OwnerSecurityInformation = 1;
    public const uint SePrivilegeEnabled = 0x00000002;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint access, bool inherit, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, out int read);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool IsWow64Process(IntPtr process, out bool wow64);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool QueryFullProcessImageName(IntPtr process, int flags, StringBuilder name, ref int size);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateToolhelp32Snapshot(uint flags, int processId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool Module32FirstW(IntPtr snapshot, ref ModuleEntry32 entry);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool Module32NextW(IntPtr snapshot, ref ModuleEntry32 entry);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool GetTokenInformation(IntPtr token, int classId, IntPtr buffer, int length, out int returned);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool GetTokenInformation(IntPtr token, int classId, ref TokenElevation elevation, int length, out int returned);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentProcess();

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(IntPtr process, int infoClass, ref uint data, int size, out int returned);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(IntPtr process, int infoClass, ref byte data, int size, out int returned);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern uint GetSecurityInfo(
        IntPtr handle,
        int objectType,
        uint securityInfo,
        out IntPtr owner,
        out IntPtr group,
        out IntPtr dacl,
        out IntPtr sacl,
        out IntPtr securityDescriptor);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(
        IntPtr descriptor,
        int revision,
        uint securityInfo,
        out IntPtr stringSd,
        out uint length);

    [DllImport("kernel32.dll")]
    public static extern IntPtr LocalFree(IntPtr memory);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool LookupPrivilegeName(string? system, ref Luid luid, System.Text.StringBuilder name, ref int length);

    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LuidAndAttributes
    {
        public Luid Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ModuleEntry32
    {
        public int Size;
        public int ModuleId;
        public int ProcessId;
        public int GlblcntUsage;
        public int ProccntUsage;
        public IntPtr ModBaseAddr;
        public int ModBaseSize;
        public IntPtr HModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string SzModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string SzExePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TokenElevation
    {
        public int TokenIsElevated;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SidAndAttributes
    {
        public IntPtr Sid;
        public uint Attributes;
    }
}
