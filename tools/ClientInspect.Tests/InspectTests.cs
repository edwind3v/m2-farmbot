using ClientInspect.Analysis;

namespace ClientInspect.Tests;

public class InspectTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";
    private const string DebugClient = @"E:\TeraBoxDownload\1.Client\1.Debug-metin2client.exe";

    [Fact]
    public void AccessProbeWithoutProcessDoesNotOpenHandles()
    {
        var attempts = ProcessProbe.ProbeAccess(null);
        Assert.Contains(attempts, item => item.Right == "PROCESS_VM_READ" && !item.Granted);
        Assert.Contains(attempts, item => item.Right == "PROCESS_QUERY_LIMITED_INFORMATION");
    }

    [Fact]
    public void ReportTextNamesDeniedMemoryInspection()
    {
        var report = new InspectReport
        {
            InspectorArchitecture = "x64",
            InspectorIntegrity = "Medium",
            MemoryInspection = "PROCESS_VM_READ is denied (Win32 5).",
            Notes = new[] { "Field offsets and pointer values are not listed." }
        };
        var text = report.ToText();
        Assert.Contains("PROCESS_VM_READ is denied", text);
        Assert.Contains("Field offsets", text);
        Assert.DoesNotContain("0x004", text);
    }

    [Fact]
    public void ReleaseClientIs32BitWithAslrAndCharacterRtti()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var pe = PeImage.Read(ReleaseClient);
        Assert.True(pe.Is32Bit);
        Assert.True(pe.Aslr);
        Assert.True(pe.Nx);
        Assert.Equal("IMAGE_FILE_MACHINE_I386 (32-bit)", pe.Machine);

        var markers = ClientMarkers.Scan(ReleaseClient);
        Assert.Contains(markers, item => item.Name == ".?AVCPythonCharacterManager@@");
        Assert.Contains(markers, item => item.Name == ".?AVCInstanceBase@@");
        Assert.Contains(markers, item => item.Name == "INSTANCE_TYPE_ENEMY");
        Assert.Contains(markers, item => item.Name.StartsWith("StoneDetect", StringComparison.Ordinal));
        Assert.DoesNotContain(markers, item => item.Name.Contains("INSTANCE_TYPE_STONE", StringComparison.Ordinal));
    }

    [Fact]
    public void DebugClientHasTheSameEntityMarkers()
    {
        Assert.True(File.Exists(DebugClient), DebugClient);
        var markers = ClientMarkers.Scan(DebugClient);
        Assert.Contains(markers, item => item.Name == "GetPixelPosition");
        Assert.Contains(markers, item => item.Name == "GetNameByVID");
        Assert.Contains(markers, item => item.Name == "CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID");
    }

    [Fact]
    public void DebugImageMapsCreateInstanceAndStoneDetect()
    {
        Assert.True(File.Exists(DebugClient), DebugClient);
        var facts = new List<Fact>();
        var functions = EntityMap.Analyze(DebugClient, facts);
        Assert.Contains(functions, item => item.Name.Contains("CreateInstance VID", StringComparison.Ordinal));
        Assert.Contains(functions, item => item.Name.StartsWith("StoneDetect", StringComparison.Ordinal) && item.Address.StartsWith("0x"));
        Assert.Contains(facts, item => item.Status == "confirmed" && item.Statement.Contains("INSTANCE_TYPE_ENEMY=0", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "confirmed" && item.Statement.Contains("INSTANCE_TYPE_STONE is absent", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "confirmed" && item.Statement.Contains("TYPE_STONE=2", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "confirmed" && item.Statement.Contains("ServerCommand", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "confirmed" && item.Statement.Contains("fff", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "unresolved" && item.Statement.Contains("Full CInstanceBase member layout", StringComparison.Ordinal));
        Assert.Contains(facts, item => item.Status == "unresolved" && item.Statement.Contains("Entity enumeration has not been demonstrated", StringComparison.Ordinal));
        Assert.DoesNotContain(functions, item => item.Address.Contains("invent", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(facts, item => item.Status == "confirmed" && item.Statement.Contains("NO_READ_UP", StringComparison.Ordinal));
    }

    [Fact]
    public void AccessProbeListsExactMasks()
    {
        var attempts = ProcessProbe.ProbeAccess(null);
        Assert.Contains(attempts, item => item.Right == "READ_CONTROL" && item.Mask == 0x20000);
        Assert.Contains(attempts, item => item.Right == "QUERY_INFORMATION|VM_READ");
        Assert.All(attempts, item => Assert.False(item.Granted));
    }
}
