using FarmBot.Core.Entities;

namespace FarmBot.Tests;

public class ReleaseLayoutTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void RecoversReleaseLayoutFromTheCompiledImage()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var layout = ClientReleaseLayout.Recover(ReleaseClient);
        var dump = $"base=0x{layout.PreferredImageBase:X} create=0x{layout.CreateInstanceRva:X} map=+0x{layout.MapDisplacement:X} singletonRva=0x{layout.SingletonRva:X} main=+0x{layout.MainPointerDisplacement:X} graphic=+0x{layout.GraphicDisplacement:X} race=+0x{layout.RaceDisplacement:X} vnum=+0x{layout.VirtualNumberDisplacement:X} type=+0x{layout.ActorTypeDisplacement:X} vid=+0x{layout.SelfVidDisplacement:X} pos=+0x{layout.PixelXDisplacement:X}/+0x{layout.PixelYDisplacement:X}/+0x{layout.PixelZDisplacement:X} negateY={layout.NegatePixelY} nodeKey=+0x{layout.NodeKey:X} nodeVal=+0x{layout.NodeValue:X} can={layout.CanEnumerate}\n" +
                   string.Join("\n", layout.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\rellayout.txt", dump);

        Assert.Equal(0x400000u, layout.PreferredImageBase);
        Assert.NotEqual(0u, layout.CreateInstanceRva);
        Assert.Equal(0x20u, layout.MapDisplacement);
        Assert.Equal(0x48239Cu, layout.SingletonRva);
        Assert.Equal(0x0Cu, layout.MainPointerDisplacement);
        Assert.Equal(0x1ECu, layout.GraphicDisplacement);
        Assert.Equal(0x178u, layout.RaceDisplacement);
        Assert.Equal(0x17Cu, layout.VirtualNumberDisplacement);
        Assert.Equal(0x400u, layout.ActorTypeDisplacement);
        Assert.Equal(0x174u, layout.SelfVidDisplacement);
        Assert.Equal(0x46Cu, layout.PixelXDisplacement);
        Assert.Equal(0x470u, layout.PixelYDisplacement);
        Assert.Equal(0x474u, layout.PixelZDisplacement);
        Assert.True(layout.NegatePixelY);
        Assert.Equal(0x0Du, layout.NodeIsNil);
        Assert.Equal(0x10u, layout.NodeKey);
        Assert.Equal(0x14u, layout.NodeValue);
        Assert.True(layout.CanEnumerate, dump);
    }
}
