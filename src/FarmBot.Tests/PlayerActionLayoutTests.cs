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
        var dump = $"singleton=0x{layout.PlayerSingletonRva:X} setTarget=0x{layout.SetTargetRva:X} getTarget=0x{layout.GetTargetVidRva:X} attackKey=0x{layout.SetAttackKeyStateRva:X} combo=0x{layout.ComboAttackRva:X} pick=0x{layout.PickCloseItemRva:X} targetDisp=0x{layout.TargetVidDisplacement:X} thisAdj=0x{layout.SetTargetThisAdjustment:X} slot=0x{layout.SetTargetVtableSlot:X} prologue={layout.SetTargetPrologue} points=+0x{layout.StatusPointsDisplacement:X} name=+0x{layout.PlayerNameDisplacement:X} xor=0x{layout.PointXorMask:X}\n" +
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
        Assert.Equal(0x1036Cu, layout.StatusPointsDisplacement);
        Assert.Equal(0x14u, layout.PlayerNameDisplacement);
        Assert.Equal(0xE73AC1DAu, layout.PointXorMask);
        Assert.Equal(1, layout.PointLevel);
        Assert.Equal(3, layout.PointExp);
        Assert.Equal(4, layout.PointNextExp);
        Assert.Equal(5, layout.PointHp);
        Assert.Equal(6, layout.PointMaxHp);
        Assert.Equal(7, layout.PointSp);
        Assert.Equal(8, layout.PointMaxSp);
        Assert.Equal(0xD1280u, layout.PickCloseItemRva);
    }

    [Fact]
    public void RecoversMiniMapAtlasBindings()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var mini = PlayerActionLayoutRecoverer.RecoverMiniMap(ReleaseClient);
        var dump = $"singleton=0x{mini.SingletonRva:X} show=0x{mini.ShowAtlasRva:X} hide=0x{mini.HideAtlasRva:X} load=0x{mini.LoadAtlasRva:X} update=0x{mini.UpdateAtlasRva:X} render=0x{mini.RenderAtlasRva:X} isShow=0x{mini.IsShowAtlasRva:X}\n" +
                   string.Join("\n", mini.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\minimap-layout.txt", dump);
        Assert.Equal(0x482BA8u, mini.SingletonRva);
        Assert.Equal(0xF9980u, mini.LoadAtlasRva);
        Assert.Equal(0xFB7B0u, mini.ShowAtlasRva);
        Assert.Equal(0xF9960u, mini.HideAtlasRva);
    }
}
