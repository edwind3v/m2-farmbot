using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class PlayerActionLayoutTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void RecoversReleaseSetTargetWithoutUsingDebugAddresses()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var layout = PlayerActionLayoutRecoverer.Recover(ReleaseClient);
        var dump = $"singleton=0x{layout.PlayerSingletonRva:X} setTarget=0x{layout.SetTargetRva:X} getTarget=0x{layout.GetTargetVidRva:X} attackKey=0x{layout.SetAttackKeyStateRva:X} combo=0x{layout.ComboAttackRva:X} targetDisp=0x{layout.TargetVidDisplacement:X} thisAdj=0x{layout.SetTargetThisAdjustment:X} slot=0x{layout.SetTargetVtableSlot:X} prologue={layout.SetTargetPrologue}\n" +
                   string.Join("\n", layout.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\playeraction.txt", dump);

        Assert.NotEqual(0u, layout.PreferredImageBase);
        Assert.True(layout.HasSetTarget, dump);
        Assert.True(layout.PlayerSingletonRva is > 0x1000 and < 0x800000, dump);
        Assert.Equal(4u, layout.SetTargetThisAdjustment);
        Assert.Equal(0x60u, layout.SetTargetVtableSlot);
        Assert.NotEqual(0x4EA2F0u, layout.SetTargetRva);
        Assert.Contains(layout.Evidence, item => item.Contains("thiscall", StringComparison.OrdinalIgnoreCase));
    }
}
