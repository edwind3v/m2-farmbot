using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class ClientRestartOwnership
{
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    private struct ProcessEntry
    {
        public uint Size,Usage,ProcessId;public UIntPtr DefaultHeapId;
        public uint ModuleId,Threads,ParentProcessId;public int BasePriority;public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string ExeFile;
    }
    [DllImport("kernel32.dll",SetLastError=true)]private static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint pid);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern bool Process32FirstW(IntPtr snapshot,ref ProcessEntry entry);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern bool Process32NextW(IntPtr snapshot,ref ProcessEntry entry);
    [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
    public static Dictionary<int,int> ReadParents()
    {
        var parents=new Dictionary<int,int>();var snapshot=CreateToolhelp32Snapshot(2,0);
        if(snapshot==new IntPtr(-1))return parents;
        try
        {
            var entry=new ProcessEntry{Size=(uint)Marshal.SizeOf(typeof(ProcessEntry))};
            if(Process32FirstW(snapshot,ref entry))do{parents[(int)entry.ProcessId]=(int)entry.ParentProcessId;}while(Process32NextW(snapshot,ref entry));
        }
        finally{CloseHandle(snapshot);}
        return parents;
    }
    public static bool IsOwnedDescendant(int candidate,int launchedPid,IDictionary<int,int> parents)
    {
        if(candidate<=0||launchedPid<=0)return false;
        var seen=new HashSet<int>();int parent;
        while(candidate>0&&seen.Add(candidate))
        {
            if(candidate==launchedPid)return true;
            if(parents==null||!parents.TryGetValue(candidate,out parent))return false;
            candidate=parent;
        }
        return false;
    }
}
