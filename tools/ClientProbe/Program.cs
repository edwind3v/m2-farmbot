using System.Runtime.InteropServices;
using System.Text;

int pid = 0;
foreach (var process in System.Diagnostics.Process.GetProcessesByName("1.metin2client"))
    pid = process.Id;
if (pid == 0)
{
    Console.WriteLine("client process not found");
    return;
}

const uint access = 0x0010u | 0x0400u | 0x1000u;
var handle = Native.OpenProcess(access, false, pid);
if (handle == IntPtr.Zero)
{
    Console.WriteLine("OpenProcess failed " + Marshal.GetLastWin32Error());
    return;
}

if (Native.NtQueryInformationProcess(handle, 26, out var peb, IntPtr.Size, out _) != 0 || peb == IntPtr.Zero)
{
    Console.WriteLine("wow64 peb missing");
    return;
}

var imageBase = BitConverter.ToUInt32(Native.Read(handle, peb + 8, 4), 0);
Console.WriteLine($"pid={pid} base=0x{imageBase:X8}");
var header = Native.Read(handle, imageBase, 0x1000);
var pe = BitConverter.ToInt32(header, 0x3C);
var sizeOfImage = BitConverter.ToUInt32(header, pe + 24 + 56);
Console.WriteLine($"sizeOfImage=0x{sizeOfImage:X}");
var module = Native.Read(handle, imageBase, (int)sizeOfImage);

var instanceName = Native.FindAscii(module, ".?AVCInstanceBase@@");
var managerName = Native.FindAscii(module, ".?AVCPythonCharacterManager@@");
Console.WriteLine($"instanceNameRva=0x{instanceName:X} managerNameRva=0x{managerName:X}");
var instanceVtable = Native.FindVtable(module, imageBase, instanceName);
var managerVtable = Native.FindVtable(module, imageBase, managerName);
Console.WriteLine($"instanceVtable=0x{instanceVtable:X8} managerVtable=0x{managerVtable:X8}");

var managers = Native.ScanDword(handle, managerVtable).Take(4).ToList();
Console.WriteLine("manager objects=" + managers.Count);
foreach (var hit in managers)
    Native.Dump(handle, hit, 0x180, "manager");

var instances = Native.ScanDword(handle, instanceVtable).Take(6).ToList();
Console.WriteLine("instance sample=" + instances.Count);
foreach (var hit in instances.Take(3))
    Native.Dump(handle, hit, 0x480, "instance");

static class Native
{
    public static uint FindVtable(byte[] module, uint imageBase, int nameRva)
    {
        if (nameRva < 8)
            return 0;
        var typeDescriptor = imageBase + (uint)nameRva - 8;
        var needle = BitConverter.GetBytes(typeDescriptor);
        for (var i = 0; i + 16 < module.Length; i += 4)
        {
            if (!Match(module, i, needle))
                continue;
            var col = i - 12;
            if (col < 0)
                continue;
            var signature = BitConverter.ToUInt32(module, col);
            if (signature > 1)
                continue;
            var colAddress = imageBase + (uint)col;
            var vtableNeedle = BitConverter.GetBytes(colAddress);
            for (var v = 0; v + 4 < module.Length; v += 4)
            {
                if (Match(module, v, vtableNeedle))
                    return imageBase + (uint)v + 4;
            }
        }
        return 0;
    }

    public static int FindAscii(byte[] data, string text)
    {
        var needle = Encoding.ASCII.GetBytes(text);
        for (var i = 0; i + needle.Length < data.Length; i++)
        {
            if (Match(data, i, needle))
                return i;
        }
        return -1;
    }

    public static IEnumerable<uint> ScanDword(IntPtr handle, uint value)
    {
        if (value == 0)
            yield break;
        var needle = BitConverter.GetBytes(value);
        long address = 0;
        while (address < 0x80000000)
        {
            if (VirtualQueryEx(handle, (IntPtr)address, out var info, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) == 0)
                break;
            var region = info.RegionSize.ToInt64();
            var next = info.BaseAddress.ToInt64() + region;
            var protect = info.Protect;
            var readable = info.State == 0x1000 && (protect & 0x100) == 0 &&
                           protect is 0x02 or 0x04 or 0x08 or 0x20 or 0x40 or 0x80;
            if (readable && region is > 0 and < 64L * 1024 * 1024)
            {
                byte[] page;
                try { page = Read(handle, (uint)info.BaseAddress.ToInt64(), (int)region); }
                catch { page = Array.Empty<byte>(); }
                for (var i = 0; i + 4 <= page.Length; i += 4)
                {
                    if (Match(page, i, needle))
                        yield return (uint)info.BaseAddress.ToInt64() + (uint)i;
                }
            }
            if (next <= address)
                break;
            address = next;
        }
    }

    public static void Dump(IntPtr handle, uint address, int size, string label)
    {
        Console.WriteLine($"-- {label} 0x{address:X8}");
        byte[] data;
        try { data = Read(handle, address, size); }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        for (var i = 0; i < data.Length; i += 16)
        {
            var take = Math.Min(16, data.Length - i);
            var hex = new StringBuilder();
            var text = new StringBuilder();
            for (var j = 0; j < take; j++)
            {
                hex.Append(data[i + j].ToString("X2")).Append(' ');
                var b = data[i + j];
                text.Append(b is >= 32 and < 127 ? (char)b : '.');
            }
            Console.WriteLine($"{i:X3} {hex.ToString(),-48} {text}");
        }
    }

    public static byte[] Read(IntPtr handle, uint address, int size)
    {
        var buffer = new byte[size];
        if (!ReadProcessMemory(handle, (IntPtr)address, buffer, size, out var read) || read != size)
            throw new InvalidOperationException($"read 0x{address:X} failed {Marshal.GetLastWin32Error()} got {read}");
        return buffer;
    }

    public static byte[] Read(IntPtr handle, nint address, int size)
    {
        var buffer = new byte[size];
        if (!ReadProcessMemory(handle, address, buffer, size, out var read) || read != size)
            throw new InvalidOperationException($"read 0x{address:X} failed {Marshal.GetLastWin32Error()} got {read}");
        return buffer;
    }

    private static bool Match(byte[] data, int offset, byte[] needle)
    {
        for (var i = 0; i < needle.Length; i++)
        {
            if (data[offset + i] != needle[i])
                return false;
        }
        return true;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, out int read);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int VirtualQueryEx(IntPtr process, IntPtr address, out MEMORY_BASIC_INFORMATION info, uint length);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(IntPtr process, int infoClass, out IntPtr data, int size, out int returnLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public nint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
}
