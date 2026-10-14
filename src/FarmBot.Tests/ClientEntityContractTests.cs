using FarmBot.Core.Entities;
using FarmBot.Platform.Client;

namespace FarmBot.Tests;

public class ClientEntityContractTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void CompiledReleaseImageExportsTheSameActorTypeIntegers()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var report = ClientEntityContract.ValidateImage(ReleaseClient);
        Assert.True(report.ImageFound);
        Assert.Contains(report.Findings, item => item.Name == "INSTANCE_TYPE_ENEMY" && item.Present && item.Value == (int)ActorType.Enemy);
        Assert.Contains(report.Findings, item => item.Name == "INSTANCE_TYPE_PLAYER" && item.Present && item.Value == (int)ActorType.Player);
        Assert.Contains(report.Findings, item => item.Name == "INSTANCE_TYPE_NPC" && item.Present && item.Value == (int)ActorType.Npc);
        Assert.Contains(report.Findings, item => item.Name == ".?AVCPythonCharacterManager@@" && item.Present);
        Assert.Contains(report.Findings, item => item.Name == "GetPixelPosition" && item.Present);
        Assert.True(report.MatchesCompiledTypes);
        Assert.DoesNotContain(report.Findings, item => item.Name.Contains("0x4EA", StringComparison.Ordinal));
    }

    [Fact]
    public void LiveProviderDoesNotInventEntitiesWhenTheClientIsAbsent()
    {
        using var provider = new LiveGameStateProvider();
        if (System.Diagnostics.Process.GetProcessesByName("1.metin2client").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("1.Debug-metin2client").Length != 0)
        {
            var frame = provider.Capture(1);
            if (frame == null)
            {
                Assert.DoesNotContain("simulator", provider.Status, StringComparison.OrdinalIgnoreCase);
                Assert.True(
                    provider.Status.Contains("ACCESS_DENIED", StringComparison.OrdinalIgnoreCase) ||
                    provider.Status.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                    provider.Status.Contains("unreadable", StringComparison.OrdinalIgnoreCase) ||
                    provider.Status.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    provider.Status.Contains("incomplete", StringComparison.OrdinalIgnoreCase),
                    provider.Status);
                return;
            }

            Assert.True(frame.FromLiveClient, provider.Status);
            Assert.DoesNotContain("simulator", frame.Source, StringComparison.OrdinalIgnoreCase);
            return;
        }

        var empty = provider.Capture(1);
        var snapshot = provider.GetSnapshot(1);
        Assert.Null(empty);
        Assert.Null(snapshot);
        Assert.Contains("not found", provider.Status, StringComparison.OrdinalIgnoreCase);
    }
}
