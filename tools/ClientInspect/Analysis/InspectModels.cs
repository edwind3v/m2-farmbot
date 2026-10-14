namespace ClientInspect.Analysis;

public sealed record AccessAttempt(string Right, uint Mask, bool Granted, int Win32Error, string Meaning);

public sealed record LoadedModule(string Name, string Path, ulong BaseAddress, int Size);

public sealed record PeSummary(
    string Path,
    long FileSize,
    string Machine,
    bool Is32Bit,
    uint ImageBase,
    string DllCharacteristics,
    bool Aslr,
    bool Nx,
    IReadOnlyList<string> Sections);

public sealed record NamedMarker(string Name, string Where, string Role);

public sealed record SnapshotAttempt(string Flags, uint Mask, bool HandleOpened, int CreateError, int FirstError, int Count, string Note);

public sealed record PrivilegeState(string Name, bool Present, bool Enabled);

public sealed record Fact(string Status, string Statement);

public sealed record FunctionRef(string Name, string Image, string Address, string Role, IReadOnlyList<string> Callers);

public sealed record SecurityContext(
    string Architecture,
    bool Elevated,
    string ElevationType,
    string Integrity,
    int SessionId,
    IReadOnlyList<PrivilegeState> Privileges,
    string TargetProtection,
    string TargetDacl,
    string TargetToken);

public sealed record InspectReport
{
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.Now;
    public string InspectorArchitecture { get; init; } = "";
    public bool InspectorElevated { get; init; }
    public string InspectorIntegrity { get; init; } = "";
    public SecurityContext? Security { get; init; }
    public IReadOnlyList<SnapshotAttempt> Snapshots { get; init; } = Array.Empty<SnapshotAttempt>();
    public IReadOnlyList<Fact> Facts { get; init; } = Array.Empty<Fact>();
    public IReadOnlyList<FunctionRef> Functions { get; init; } = Array.Empty<FunctionRef>();
    public string? ProcessName { get; init; }
    public int? ProcessId { get; init; }
    public string? ImagePath { get; init; }
    public bool? TargetIsWow64 { get; init; }
    public IReadOnlyList<AccessAttempt> Access { get; init; } = Array.Empty<AccessAttempt>();
    public bool QueryLimitedGranted { get; init; }
    public bool QueryInformationGranted { get; init; }
    public bool VmReadGranted { get; init; }
    public string MemoryInspection { get; init; } = "";
    public IReadOnlyList<LoadedModule> Modules { get; init; } = Array.Empty<LoadedModule>();
    public string ModuleNote { get; init; } = "";
    public PeSummary? OnDiskImage { get; init; }
    public IReadOnlyList<NamedMarker> Markers { get; init; } = Array.Empty<NamedMarker>();
    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();

    public string ToText()
    {
        var lines = new List<string>
        {
            "ClientInspect report",
            $"generated {GeneratedAt:O}",
            "",
            "Inspector",
            $"  architecture     {InspectorArchitecture}",
            $"  elevated         {InspectorElevated}",
            $"  integrity        {InspectorIntegrity}"
        };

        if (Security != null)
        {
            lines.Add($"  elevation type   {Security.ElevationType}");
            lines.Add($"  session          {Security.SessionId}");
            foreach (var privilege in Security.Privileges)
            {
                var state = privilege.Present ? (privilege.Enabled ? "present, enabled" : "present, disabled") : "absent";
                lines.Add($"  privilege        {privilege.Name} ({state})");
            }
        }

        lines.Add("");
        lines.Add("Process");

        if (ProcessId is null)
        {
            lines.Add("  1.metin2client.exe was not running");
        }
        else
        {
            lines.Add($"  name             {ProcessName}");
            lines.Add($"  pid              {ProcessId}");
            lines.Add($"  image            {ImagePath ?? "(path unavailable)"}");
            lines.Add($"  wow64            {TargetIsWow64?.ToString() ?? "unknown"}");
        }

        if (Security != null)
        {
            lines.Add($"  protection query {Security.TargetProtection}");
            lines.Add($"  DACL query       {Security.TargetDacl}");
            lines.Add($"  target token     {Security.TargetToken}");
        }

        lines.Add("");
        lines.Add("OpenProcess rights (masks requested exactly as listed)");
        foreach (var attempt in Access)
        {
            var result = attempt.Granted ? "granted" : $"denied Win32 {attempt.Win32Error} ({Win32Name(attempt.Win32Error)})";
            lines.Add($"  {attempt.Right,-42} 0x{attempt.Mask:X8}  {result}");
            lines.Add($"      {attempt.Meaning}");
        }

        if (Snapshots.Count > 0)
        {
            lines.Add("");
            lines.Add("Module / process snapshots");
            foreach (var snap in Snapshots)
            {
                var opened = snap.HandleOpened ? "handle opened" : $"CreateToolhelp32Snapshot Win32 {snap.CreateError} ({Win32Name(snap.CreateError)})";
                lines.Add($"  {snap.Flags,-40} 0x{snap.Mask:X}  {opened}");
                if (snap.HandleOpened && snap.FirstError != 0)
                    lines.Add($"      Module32First Win32 {snap.FirstError} ({Win32Name(snap.FirstError)})");
                if (snap.Count > 0)
                    lines.Add($"      enumerated {snap.Count}");
                lines.Add($"      {snap.Note}");
            }
        }

        lines.Add("");
        lines.Add("Memory inspection");
        lines.Add($"  {MemoryInspection}");

        lines.Add("");
        lines.Add("Loaded modules");
        if (Modules.Count == 0)
        {
            lines.Add($"  {ModuleNote}");
        }
        else
        {
            foreach (var module in Modules)
                lines.Add($"  0x{module.BaseAddress:X8}  {module.Size,10}  {module.Name}  {module.Path}");
        }

        if (OnDiskImage != null)
        {
            var pe = OnDiskImage;
            lines.Add("");
            lines.Add("On-disk PE (static)");
            lines.Add($"  path             {pe.Path}");
            lines.Add($"  size             {pe.FileSize} bytes");
            lines.Add($"  machine          {pe.Machine}");
            lines.Add($"  32-bit           {pe.Is32Bit}");
            lines.Add($"  image base       0x{pe.ImageBase:X}");
            lines.Add($"  dll chars        {pe.DllCharacteristics}");
            lines.Add($"  ASLR             {pe.Aslr}");
            lines.Add($"  NX               {pe.Nx}");
            lines.Add($"  sections         {string.Join(", ", pe.Sections)}");
        }

        if (Markers.Count > 0)
        {
            lines.Add("");
            lines.Add("Named client objects (string presence only; no field offsets)");
            foreach (var marker in Markers)
                lines.Add($"  {marker.Name,-42} {marker.Where,-8} {marker.Role}");
        }

        if (Functions.Count > 0)
        {
            lines.Add("");
            lines.Add("Static function map (preferred image base of the named file; ASLR may relocate the live process)");
            foreach (var fn in Functions)
            {
                lines.Add($"  {fn.Address}  {fn.Name}");
                lines.Add($"      {fn.Image}  {fn.Role}");
                if (fn.Callers.Count > 0)
                    lines.Add($"      callers: {string.Join(", ", fn.Callers)}");
            }
        }

        if (Facts.Count > 0)
        {
            lines.Add("");
            lines.Add("Confirmed vs unresolved");
            foreach (var fact in Facts)
                lines.Add($"  [{fact.Status}] {fact.Statement}");
        }

        if (Notes.Count > 0)
        {
            lines.Add("");
            lines.Add("Notes");
            foreach (var note in Notes)
                lines.Add($"  - {note}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Win32Name(int error) => error switch
    {
        0 => "ERROR_SUCCESS",
        5 => "ERROR_ACCESS_DENIED",
        6 => "ERROR_INVALID_HANDLE",
        87 => "ERROR_INVALID_PARAMETER",
        299 => "ERROR_PARTIAL_COPY",
        998 => "ERROR_NOACCESS",
        _ => "see winerror.h"
    };
}
