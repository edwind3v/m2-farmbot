using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public sealed class PythonApiSet
{
    public long AddPendingCall;
    public long RunSimpleString;
    public bool RunUsesFlags;
    public string ModuleName;
    public string Architecture;
}

public static class PythonApiLocator
{
    private sealed class Section { public uint Va, VirtualSize, Raw, RawSize; }
    private sealed class ModuleRef { public string FileName; public long BaseAddress; }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    private struct MODULEENTRY32
    {
        public uint dwSize, th32ModuleID, th32ProcessID, GlblcntUsage, ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)] public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)] public string szExePath;
    }
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint processId);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool Module32FirstW(IntPtr snapshot,ref MODULEENTRY32 entry);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool Module32NextW(IntPtr snapshot,ref MODULEENTRY32 entry);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);

    public static PythonApiSet Find(Process process)
    {
        if (process == null || process.HasExited) return null;
        var modules = Modules(process);
        modules = modules.OrderByDescending(m => Path.GetFileName(m.FileName).StartsWith("python", StringComparison.OrdinalIgnoreCase))
                         .ThenByDescending(m => m.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
        string mainPath=null;try{mainPath=process.MainModule.FileName;}catch{}
        foreach (var module in modules)
        {
            string name = Path.GetFileName(module.FileName);
            if (!name.StartsWith("python", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(module.FileName, mainPath, StringComparison.OrdinalIgnoreCase)) continue;
            Dictionary<string, uint> exports;
            if (!TryExports(module.FileName, out exports)) continue;
            uint add, run; bool flags = false;
            if (!exports.TryGetValue("Py_AddPendingCall", out add)) continue;
            if (!exports.TryGetValue("PyRun_SimpleString", out run))
            {
                if (!exports.TryGetValue("PyRun_SimpleStringFlags", out run)) continue;
                flags = true;
            }
            ClientBinaryInfo info;
            if (!ClientBinaryInfo.TryRead(module.FileName, out info)) continue;
            return new PythonApiSet {
                AddPendingCall = module.BaseAddress + add,
                RunSimpleString = module.BaseAddress + run,
                RunUsesFlags = flags, ModuleName = name, Architecture = info.Architecture
            };
        }
        return null;
    }

    private static List<ModuleRef> Modules(Process process)
    {
        var result=new List<ModuleRef>();
        try
        {
            foreach(ProcessModule m in process.Modules)
                result.Add(new ModuleRef{FileName=m.FileName,BaseAddress=m.BaseAddress.ToInt64()});
        }
        catch{}
        // A 64-bit .NET process cannot reliably enumerate WOW64 modules through
        // Process.Modules. Toolhelp with SNAPMODULE32 returns the real x86 list.
        IntPtr snap=CreateToolhelp32Snapshot(0x00000008u|0x00000010u,(uint)process.Id);
        if(snap!=new IntPtr(-1))
        {
            try
            {
                var e=new MODULEENTRY32();e.dwSize=(uint)Marshal.SizeOf(typeof(MODULEENTRY32));
                if(Module32FirstW(snap,ref e))do
                {
                    if(!string.IsNullOrWhiteSpace(e.szExePath)&&!result.Any(x=>string.Equals(x.FileName,e.szExePath,StringComparison.OrdinalIgnoreCase)))
                        result.Add(new ModuleRef{FileName=e.szExePath,BaseAddress=e.modBaseAddr.ToInt64()});
                    e.dwSize=(uint)Marshal.SizeOf(typeof(MODULEENTRY32));
                }while(Module32NextW(snap,ref e));
            }
            finally{CloseHandle(snap);}
        }
        return result;
    }

    private static bool TryExports(string path, out Dictionary<string, uint> result)
    {
        result = new Dictionary<string, uint>(StringComparer.Ordinal);
        try
        {
            byte[] b = File.ReadAllBytes(path); int pe = BitConverter.ToInt32(b, 0x3c);
            ushort sections = BitConverter.ToUInt16(b, pe + 6), optSize = BitConverter.ToUInt16(b, pe + 20), magic = BitConverter.ToUInt16(b, pe + 24);
            int opt = pe + 24, dataDir = opt + (magic == 0x20b ? 112 : 96);
            uint exportRva = BitConverter.ToUInt32(b, dataDir); if (exportRva == 0) return false;
            int sectionTable = opt + optSize; var list = new List<Section>();
            for (int i = 0; i < sections; i++)
            {
                int o = sectionTable + i * 40;
                list.Add(new Section { VirtualSize = BitConverter.ToUInt32(b, o + 8), Va = BitConverter.ToUInt32(b, o + 12), RawSize = BitConverter.ToUInt32(b, o + 16), Raw = BitConverter.ToUInt32(b, o + 20) });
            }
            Func<uint, int> off = rva => {
                var s = list.FirstOrDefault(x => rva >= x.Va && rva < x.Va + Math.Max(x.VirtualSize, x.RawSize));
                return s == null ? -1 : checked((int)(s.Raw + rva - s.Va));
            };
            int exp = off(exportRva); if (exp < 0 || exp + 40 > b.Length) return false;
            uint functionCount = BitConverter.ToUInt32(b, exp + 20), nameCount = BitConverter.ToUInt32(b, exp + 24);
            int functions = off(BitConverter.ToUInt32(b, exp + 28)), names = off(BitConverter.ToUInt32(b, exp + 32)), ordinals = off(BitConverter.ToUInt32(b, exp + 36));
            if (functions < 0 || names < 0 || ordinals < 0 || nameCount > 200000 || functionCount > 200000) return false;
            for (uint i = 0; i < nameCount; i++)
            {
                int no = off(BitConverter.ToUInt32(b, names + checked((int)i * 4))); if (no < 0) continue;
                int end = no; while (end < b.Length && b[end] != 0 && end - no < 256) end++;
                string n = Encoding.ASCII.GetString(b, no, end - no);
                ushort ordinal = BitConverter.ToUInt16(b, ordinals + checked((int)i * 2)); if (ordinal >= functionCount) continue;
                result[n] = BitConverter.ToUInt32(b, functions + ordinal * 4);
            }
            return result.Count > 0;
        }
        catch { result.Clear(); return false; }
    }
}

