using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace ClientInspect.Analysis;

public static class ProcessProbe
{
    public static readonly string[] ProcessNames = { "1.metin2client", "1.Debug-metin2client" };

    public static InspectReport Run(string? fallbackImagePath = null)
    {
        var notes = new List<string>();
        var (elevated, integrity) = CurrentToken();
        Process? process = null;
        foreach (var name in ProcessNames)
        {
            process = Process.GetProcessesByName(name).FirstOrDefault();
            if (process != null)
                break;
        }

        var access = ProbeAccess(process?.Id);
        var queryLimited = access.Any(item => item.Right == "PROCESS_QUERY_LIMITED_INFORMATION" && item.Granted);
        var queryInfo = access.Any(item => item.Right == "PROCESS_QUERY_INFORMATION" && item.Granted);
        var vmRead = access.Any(item => item.Right == "PROCESS_VM_READ" && item.Granted);

        string? imagePath = null;
        bool? wow64 = null;
        var modules = new List<LoadedModule>();
        var moduleNote = "no process";
        IReadOnlyList<SnapshotAttempt> snapshots = Array.Empty<SnapshotAttempt>();
        var security = ReadInspectorSecurity(elevated, integrity);

        if (process != null)
        {
            imagePath = TryImagePath(process.Id);
            wow64 = TryWow64(process.Id);
            snapshots = ProbeSnapshots(process.Id);
            var moduleSnap = snapshots.FirstOrDefault(item => item.Count > 0);
            if (moduleSnap != null)
            {
                modules.AddRange(TryModules(process.Id, out moduleNote));
            }
            else
            {
                moduleNote = snapshots.LastOrDefault(item => item.Flags.Contains("MODULE", StringComparison.Ordinal))?.Note
                    ?? "module snapshot produced no list";
            }

            security = security with
            {
                TargetProtection = QueryProtection(process.Id),
                TargetDacl = QueryDacl(process.Id),
                TargetToken = QueryTargetToken(process.Id)
            };
        }

        var diskPath = FirstExisting(imagePath, fallbackImagePath);
        var debugPath = FirstExisting(
            Path.Combine(Path.GetDirectoryName(diskPath ?? "") ?? "", "1.Debug-metin2client.exe"),
            @"E:\TeraBoxDownload\1.Client\1.Debug-metin2client.exe");
        PeSummary? pe = null;
        IReadOnlyList<NamedMarker> markers = Array.Empty<NamedMarker>();
        IReadOnlyList<FunctionRef> functions = Array.Empty<FunctionRef>();
        var facts = new List<Fact>();
        if (diskPath != null)
        {
            pe = PeImage.Read(diskPath);
            markers = ClientMarkers.Scan(diskPath);
            notes.AddRange(ClientMarkers.StructureNotes(markers));
        }
        else
        {
            notes.Add("No on-disk client image was available for static analysis.");
        }

        if (debugPath != null)
            functions = EntityMap.Analyze(debugPath, facts);
        else if (diskPath != null)
            functions = EntityMap.Analyze(diskPath, facts);

        facts.AddRange(AccessFacts(process, access, snapshots, security, wow64));

        var memory = DescribeMemory(process, queryLimited, queryInfo, vmRead, elevated, security);
        notes.Add(memory);

        return new InspectReport
        {
            InspectorArchitecture = Environment.Is64BitProcess ? "x64" : "x86",
            InspectorElevated = elevated,
            InspectorIntegrity = integrity,
            Security = security,
            ProcessName = process?.ProcessName,
            ProcessId = process?.Id,
            ImagePath = imagePath,
            TargetIsWow64 = wow64,
            Access = access,
            QueryLimitedGranted = queryLimited,
            QueryInformationGranted = queryInfo,
            VmReadGranted = vmRead,
            MemoryInspection = memory,
            Modules = modules,
            ModuleNote = moduleNote,
            Snapshots = snapshots,
            OnDiskImage = pe,
            Markers = markers,
            Functions = functions,
            Facts = facts,
            Notes = notes
        };
    }

    public static IReadOnlyList<AccessAttempt> ProbeAccess(int? processId)
    {
        var rights = new (string Name, uint Mask, string Meaning)[]
        {
            ("PROCESS_QUERY_LIMITED_INFORMATION", Native.ProcessQueryLimitedInformation, "QueryFullProcessImageName, IsWow64Process"),
            ("PROCESS_QUERY_INFORMATION", Native.ProcessQueryInformation, "full process query"),
            ("PROCESS_VM_READ", Native.ProcessVmRead, "ReadProcessMemory"),
            ("PROCESS_VM_OPERATION", Native.ProcessVmOperation, "VirtualProtectEx / write setup; not used"),
            ("PROCESS_SET_INFORMATION", Native.ProcessSetInformation, "not used; probed only"),
            ("PROCESS_DUP_HANDLE", Native.ProcessDupHandle, "DuplicateHandle from the target"),
            ("READ_CONTROL", Native.ReadControl, "read the process security descriptor / DACL"),
            ("SYNCHRONIZE", Native.Synchronize, "wait on process exit"),
            ("QUERY_LIMITED|READ_CONTROL", Native.ProcessQueryLimitedInformation | Native.ReadControl, "combined path + security-descriptor read"),
            ("QUERY_INFORMATION|VM_READ", Native.ProcessQueryInformation | Native.ProcessVmRead, "classic Toolhelp / PSAPI module enumeration pair")
        };

        if (processId is null)
        {
            return rights
                .Select(item => new AccessAttempt(item.Name, item.Mask, false, 0, "not tested; process missing"))
                .ToList();
        }

        var results = new List<AccessAttempt>();
        foreach (var right in rights)
        {
            var handle = Native.OpenProcess(right.Mask, false, processId.Value);
            var error = handle == IntPtr.Zero ? Marshal.GetLastWin32Error() : 0;
            if (handle != IntPtr.Zero)
                Native.CloseHandle(handle);
            results.Add(new AccessAttempt(right.Name, right.Mask, handle != IntPtr.Zero, error, right.Meaning));
        }

        return results;
    }

    private static string DescribeMemory(
        Process? process,
        bool queryLimited,
        bool queryInfo,
        bool vmRead,
        bool elevated,
        SecurityContext security)
    {
        if (process == null)
            return "No running client. Start 1.metin2client.exe and scan again.";
        if (vmRead && (queryLimited || queryInfo))
            return "Ordinary user-mode ReadProcessMemory is permitted. Instance walking is still not implemented because field offsets are not recovered.";

        var parts = new List<string>
        {
            $"PROCESS_VM_READ is denied (Win32 5 / ERROR_ACCESS_DENIED) from this {security.Architecture} {security.Integrity}-integrity inspector (elevated={elevated}, {security.ElevationType})."
        };
        if (security.TargetDacl.StartsWith("denied", StringComparison.OrdinalIgnoreCase))
            parts.Add("The process DACL was not read, so a DACL deny ACE is not a confirmed observation.");
        else if (security.TargetDacl.StartsWith("SD:", StringComparison.Ordinal))
            parts.Add("The process security descriptor was read; see DACL query.");
        if (security.TargetProtection.Contains("none", StringComparison.OrdinalIgnoreCase) ||
            security.TargetProtection.Contains("Type=0", StringComparison.Ordinal))
            parts.Add("ProcessProtectionInformation did not report a protected-process type.");
        else if (security.TargetProtection.StartsWith("denied", StringComparison.OrdinalIgnoreCase) ||
                 security.TargetProtection.StartsWith("ntstatus", StringComparison.OrdinalIgnoreCase))
            parts.Add("Protected-process status was not established.");
        parts.Add("No privilege was enabled and no ACL was changed.");
        return string.Join(" ", parts);
    }

    private static string? TryImagePath(int processId)
    {
        var handle = Native.OpenProcess(Native.ProcessQueryLimitedInformation, false, processId);
        if (handle == IntPtr.Zero)
            return null;
        try
        {
            var size = 1024;
            var buffer = new StringBuilder(size);
            return Native.QueryFullProcessImageName(handle, 0, buffer, ref size) ? buffer.ToString() : null;
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    private static bool? TryWow64(int processId)
    {
        var handle = Native.OpenProcess(Native.ProcessQueryLimitedInformation, false, processId);
        if (handle == IntPtr.Zero)
            return null;
        try
        {
            return Native.IsWow64Process(handle, out var wow64) ? wow64 : null;
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    public static IReadOnlyList<SnapshotAttempt> ProbeSnapshots(int processId)
    {
        var attempts = new List<SnapshotAttempt>
        {
            TrySnapshot("TH32CS_SNAPPROCESS", Native.SnapProcess, processId, modules: false),
            TrySnapshot("TH32CS_SNAPMODULE", Native.SnapModule, processId, modules: true),
            TrySnapshot("TH32CS_SNAPMODULE32", Native.SnapModule32, processId, modules: true),
            TrySnapshot("TH32CS_SNAPMODULE|TH32CS_SNAPMODULE32", Native.SnapModule | Native.SnapModule32, processId, modules: true)
        };
        return attempts;
    }

    private static SnapshotAttempt TrySnapshot(string flags, uint mask, int processId, bool modules)
    {
        var inspector = Environment.Is64BitProcess ? "x64" : "x86";
        var snapshot = Native.CreateToolhelp32Snapshot(mask, processId);
        var createError = (snapshot == IntPtr.Zero || snapshot == (IntPtr)(-1)) ? Marshal.GetLastWin32Error() : 0;
        if (createError != 0)
        {
            var why = createError == 5
                ? "ERROR_ACCESS_DENIED at snapshot create. On current Windows this matches missing PROCESS_VM_READ / PROCESS_QUERY_INFORMATION, not a Toolhelp handle format error."
                : createError == 299
                    ? "ERROR_PARTIAL_COPY: typical 64-bit snapshot of a WOW64 module list without TH32CS_SNAPMODULE32."
                    : "see winerror.h";
            return new SnapshotAttempt(flags, mask, false, createError, 0, 0, $"{inspector} inspector. {why}");
        }

        try
        {
            if (!modules)
            {
                return new SnapshotAttempt(flags, mask, true, 0, 0, 1,
                    $"{inspector} inspector. Process snapshot opened; this API does not need VM_READ.");
            }

            var entry = new Native.ModuleEntry32 { Size = Marshal.SizeOf<Native.ModuleEntry32>() };
            if (!Native.Module32FirstW(snapshot, ref entry))
            {
                var first = Marshal.GetLastWin32Error();
                var why = first == 299
                    ? "ERROR_PARTIAL_COPY from Module32First: 64-bit walker of a 32-bit module list."
                    : first == 5
                        ? "ERROR_ACCESS_DENIED from Module32First after the snapshot handle opened."
                        : "Module32First failed.";
                return new SnapshotAttempt(flags, mask, true, 0, first, 0, $"{inspector} inspector. {why}");
            }

            var count = 1;
            while (Native.Module32NextW(snapshot, ref entry))
                count++;
            return new SnapshotAttempt(flags, mask, true, 0, 0, count, $"{inspector} inspector. Module list available.");
        }
        finally
        {
            Native.CloseHandle(snapshot);
        }
    }

    private static IReadOnlyList<LoadedModule> TryModules(int processId, out string note)
    {
        var snapshot = Native.CreateToolhelp32Snapshot(Native.SnapModule | Native.SnapModule32, processId);
        if (snapshot == IntPtr.Zero || snapshot == (IntPtr)(-1))
        {
            note = $"CreateToolhelp32Snapshot failed Win32 {Marshal.GetLastWin32Error()}";
            return Array.Empty<LoadedModule>();
        }

        try
        {
            var entry = new Native.ModuleEntry32 { Size = Marshal.SizeOf<Native.ModuleEntry32>() };
            if (!Native.Module32FirstW(snapshot, ref entry))
            {
                note = $"Module32First failed Win32 {Marshal.GetLastWin32Error()}";
                return Array.Empty<LoadedModule>();
            }

            var modules = new List<LoadedModule>();
            do
            {
                modules.Add(new LoadedModule(
                    entry.SzModule ?? "",
                    entry.SzExePath ?? "",
                    (ulong)entry.ModBaseAddr,
                    entry.ModBaseSize));
            } while (Native.Module32NextW(snapshot, ref entry));

            note = $"{modules.Count} modules via Toolhelp";
            return modules;
        }
        finally
        {
            Native.CloseHandle(snapshot);
        }
    }

    private static (bool Elevated, string Integrity) CurrentToken()
    {
        if (!Native.OpenProcessToken(Native.GetCurrentProcess(), Native.TokenQuery, out var token))
            return (false, "token unavailable");
        try
        {
            var elevation = new Native.TokenElevation();
            Native.GetTokenInformation(token, Native.TokenElevationClass, ref elevation, Marshal.SizeOf<Native.TokenElevation>(), out _);
            return (elevation.TokenIsElevated != 0, IntegrityName(token));
        }
        finally
        {
            Native.CloseHandle(token);
        }
    }

    private static string IntegrityName(IntPtr token)
    {
        Native.GetTokenInformation(token, Native.TokenIntegrityLevel, IntPtr.Zero, 0, out var needed);
        if (needed <= 0)
            return "unknown";
        var buffer = Marshal.AllocHGlobal(needed);
        try
        {
            if (!Native.GetTokenInformation(token, Native.TokenIntegrityLevel, buffer, needed, out _))
                return "unknown";
            var sidAndAttr = Marshal.PtrToStructure<Native.SidAndAttributes>(buffer);
            var sid = new SecurityIdentifier(sidAndAttr.Sid);
            var rid = sid.Value.Split('-').Last();
            return rid switch
            {
                "4096" => "Low",
                "8192" => "Medium",
                "8448" => "Medium Plus",
                "12288" => "High",
                "16384" => "System",
                _ => sid.Value
            };
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static SecurityContext ReadInspectorSecurity(bool elevated, string integrity)
    {
        var elevationType = "unknown";
        var session = -1;
        var privileges = new List<PrivilegeState>();
        if (Native.OpenProcessToken(Native.GetCurrentProcess(), Native.TokenQuery, out var token))
        {
            try
            {
                elevationType = ReadElevationType(token);
                session = ReadSession(token);
                privileges.AddRange(ReadPrivileges(token, "SeDebugPrivilege", "SeRelabelPrivilege", "SeTcbPrivilege"));
            }
            finally
            {
                Native.CloseHandle(token);
            }
        }

        return new SecurityContext(
            Environment.Is64BitProcess ? "x64" : "x86",
            elevated,
            elevationType,
            integrity,
            session,
            privileges,
            "not queried; no target",
            "not queried; no target",
            "not queried; no target");
    }

    private static string ReadElevationType(IntPtr token)
    {
        var value = 0;
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            if (!Native.GetTokenInformation(token, Native.TokenElevationTypeClass, buffer, 4, out _))
                return $"GetTokenInformation failed Win32 {Marshal.GetLastWin32Error()}";
            value = Marshal.ReadInt32(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return value switch
        {
            1 => "TokenElevationTypeDefault",
            2 => "TokenElevationTypeFull",
            3 => "TokenElevationTypeLimited",
            _ => value.ToString()
        };
    }

    private static int ReadSession(IntPtr token)
    {
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            return Native.GetTokenInformation(token, Native.TokenSessionId, buffer, 4, out _)
                ? Marshal.ReadInt32(buffer)
                : -1;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static IReadOnlyList<PrivilegeState> ReadPrivileges(IntPtr token, params string[] names)
    {
        Native.GetTokenInformation(token, Native.TokenPrivilegesClass, IntPtr.Zero, 0, out var needed);
        if (needed <= 0)
            return names.Select(name => new PrivilegeState(name, false, false)).ToList();
        var buffer = Marshal.AllocHGlobal(needed);
        try
        {
            if (!Native.GetTokenInformation(token, Native.TokenPrivilegesClass, buffer, needed, out _))
                return names.Select(name => new PrivilegeState(name, false, false)).ToList();
            var count = Marshal.ReadInt32(buffer);
            var found = new Dictionary<string, PrivilegeState>(StringComparer.OrdinalIgnoreCase);
            var cursor = buffer + 4;
            for (var i = 0; i < count; i++)
            {
                var entry = Marshal.PtrToStructure<Native.LuidAndAttributes>(cursor);
                cursor += Marshal.SizeOf<Native.LuidAndAttributes>();
                var length = 256;
                var name = new StringBuilder(length);
                var luid = entry.Luid;
                if (!Native.LookupPrivilegeName(null, ref luid, name, ref length))
                    continue;
                found[name.ToString()] = new PrivilegeState(
                    name.ToString(),
                    true,
                    (entry.Attributes & Native.SePrivilegeEnabled) != 0);
            }

            return names
                .Select(name => found.TryGetValue(name, out var state) ? state : new PrivilegeState(name, false, false))
                .ToList();
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string QueryProtection(int processId)
    {
        var handle = Native.OpenProcess(Native.ProcessQueryLimitedInformation, false, processId);
        if (handle == IntPtr.Zero)
            return $"OpenProcess(QUERY_LIMITED) failed Win32 {Marshal.GetLastWin32Error()}";
        try
        {
            byte level = 0;
            var status = Native.NtQueryInformationProcess(handle, Native.ProcessProtectionInformation, ref level, 1, out _);
            if (status != 0)
                return $"NtQueryInformationProcess(ProcessProtectionInformation=61) NTSTATUS 0x{status:X8}";
            var type = level & 0x07;
            var signer = level >> 4;
            var name = type switch
            {
                0 => "none",
                1 => "PsProtectedTypeProtectedLight",
                2 => "PsProtectedTypeProtected",
                _ => type.ToString()
            };
            return $"NTSTATUS 0, Type={type} ({name}), Signer={signer}";
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    private static string QueryDacl(int processId)
    {
        foreach (var mask in new[] { Native.ReadControl, Native.ProcessQueryLimitedInformation | Native.ReadControl, Native.ProcessQueryLimitedInformation })
        {
            var handle = Native.OpenProcess(mask, false, processId);
            if (handle == IntPtr.Zero)
                continue;
            try
            {
                var error = Native.GetSecurityInfo(
                    handle,
                    Native.SeKernelObject,
                    Native.OwnerSecurityInformation | Native.DaclSecurityInformation,
                    out _,
                    out _,
                    out _,
                    out _,
                    out var sd);
                if (error != 0)
                    return $"GetSecurityInfo with 0x{mask:X} returned Win32 {error}";
                try
                {
                    if (!Native.ConvertSecurityDescriptorToStringSecurityDescriptor(
                            sd, 1, Native.OwnerSecurityInformation | Native.DaclSecurityInformation, out var text, out _))
                    {
                        return $"descriptor read with 0x{mask:X}; ConvertSecurityDescriptor failed Win32 {Marshal.GetLastWin32Error()}";
                    }

                    var sdString = Marshal.PtrToStringUni(text) ?? "";
                    Native.LocalFree(text);
                    return $"SD: {sdString}";
                }
                finally
                {
                    Native.LocalFree(sd);
                }
            }
            finally
            {
                Native.CloseHandle(handle);
            }
        }

        return "denied: OpenProcess(READ_CONTROL) and GetSecurityInfo both failed. The DACL itself was not observed.";
    }

    private static string QueryTargetToken(int processId)
    {
        var handle = Native.OpenProcess(Native.ProcessQueryLimitedInformation, false, processId);
        if (handle == IntPtr.Zero)
            return $"OpenProcess(QUERY_LIMITED) failed Win32 {Marshal.GetLastWin32Error()}";
        try
        {
            if (!Native.OpenProcessToken(handle, Native.TokenQuery, out var token))
                return $"OpenProcessToken(TOKEN_QUERY) failed Win32 {Marshal.GetLastWin32Error()}";
            try
            {
                return $"opened; integrity {IntegrityName(token)}, elevation {ReadElevationType(token)}";
            }
            finally
            {
                Native.CloseHandle(token);
            }
        }
        finally
        {
            Native.CloseHandle(handle);
        }
    }

    private static IEnumerable<Fact> AccessFacts(
        Process? process,
        IReadOnlyList<AccessAttempt> access,
        IReadOnlyList<SnapshotAttempt> snapshots,
        SecurityContext security,
        bool? wow64)
    {
        if (process == null)
        {
            yield return new Fact("unresolved", "No live process, so access viability was not re-tested.");
            yield break;
        }

        foreach (var attempt in access)
        {
            yield return new Fact(
                attempt.Granted ? "confirmed" : "confirmed",
                $"OpenProcess({attempt.Right}, 0x{attempt.Mask:X}) {(attempt.Granted ? "succeeded" : "returned Win32 " + attempt.Win32Error)}.");
        }

        foreach (var snap in snapshots)
        {
            yield return new Fact(
                "confirmed",
                $"{snap.Flags} from {(Environment.Is64BitProcess ? "x64" : "x86")}: {(snap.HandleOpened ? "handle opened" : "create failed " + snap.CreateError)}{(snap.FirstError != 0 ? ", first=" + snap.FirstError : "")}.");
        }

        if (wow64 == true && Environment.Is64BitProcess)
        {
            var partial = snapshots.Any(item => item.CreateError == 299 || item.FirstError == 299);
            yield return new Fact(
                partial ? "confirmed" : "unresolved",
                partial
                    ? "ERROR_PARTIAL_COPY appeared; that is the documented 64-bit / WOW64 module-list mismatch."
                    : "The target is WOW64 and the inspector is x64, but the module snapshot failed with ACCESS_DENIED rather than ERROR_PARTIAL_COPY. Architecture mismatch is not the confirmed cause.");
        }

        if (security.TargetToken.Contains("integrity High", StringComparison.Ordinal) &&
            security.Integrity.Equals("Medium", StringComparison.OrdinalIgnoreCase))
        {
            yield return new Fact(
                "confirmed",
                "The target token is High integrity / TokenElevationTypeFull. This inspector is Medium / TokenElevationTypeLimited.");
            yield return new Fact(
                "unresolved",
                "Windows default mandatory policy is SYSTEM_MANDATORY_LABEL_NO_WRITE_UP. NO_READ_UP is a separate SACL flag and was not observed. Medium-to-High integrity alone does not universally deny PROCESS_VM_READ. The DACL was not observed. The measured fact is only that this Medium inspector received ERROR_ACCESS_DENIED on VM_READ.");
        }

        yield return new Fact(
            security.TargetProtection.StartsWith("NTSTATUS 0", StringComparison.Ordinal) ? "confirmed" : "unresolved",
            "Protected-process query: " + security.TargetProtection);
        yield return new Fact(
            security.TargetDacl.StartsWith("SD:", StringComparison.Ordinal) ? "confirmed" : "unresolved",
            "Process DACL: " + security.TargetDacl);
        yield return new Fact(
            "assumption",
            "A custom deny ACE or a named usermode guard (including CShield) may also be present. That is not established: CShield is not a string or DLL in the client folder, the DACL was not read, and no live module list was obtained.");
        yield return new Fact(
            "confirmed",
            "SeDebugPrivilege was not enabled by this tool. Privileges listed above are the inspector token as-is.");
    }

    private static string? FirstExisting(params string?[] paths)
    {
        foreach (var path in paths)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                return path;
        }

        return null;
    }
}
