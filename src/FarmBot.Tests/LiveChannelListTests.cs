using System.Text.Json;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;

namespace FarmBot.Tests;

public class LiveChannelListTests
{
    [Fact]
    public void FromResponseSkipsLoopbackAndParsesHosts()
    {
        var json = """
            [
              {"index":0,"name":"CH-1","host":"127.0.0.1","port":30003,"authHost":"127.0.0.1","authPort":30001},
              {"index":1,"name":"CH-2","host":"10.0.0.5","port":13001,"authHost":"10.0.0.5","authPort":11000}
            ]
            """;
        var response = new ActionResponse
        {
            Channels = JsonDocument.Parse(json).RootElement.Clone()
        };
        var channels = LiveChannelList.FromResponse(response);
        Assert.Single(channels);
        Assert.Equal(1, channels[0].Index);
        Assert.Equal("10.0.0.5", channels[0].Host);
        Assert.Equal(13001, channels[0].Port);
        Assert.Equal(11000, channels[0].AuthPort);
    }

    [Fact]
    public void FromResponseKeepsZeroAuthPort()
    {
        var json = """
            [{"index":0,"name":"CH-1","host":"firewall.baerim.eu","port":30007,"authHost":"firewall.baerim.eu","authPort":0}]
            """;
        var response = new ActionResponse
        {
            Channels = JsonDocument.Parse(json).RootElement.Clone()
        };
        var channels = LiveChannelList.FromResponse(response);
        Assert.Single(channels);
        Assert.Equal(0, channels[0].AuthPort);
        Assert.Equal(30007, channels[0].Port);
    }

    [Fact]
    public void ApplyCharacterNamesFillsSelectedSlotName()
    {
        var slot = new ClientSlot { CharacterSlot = 2 };
        Assert.True(SlotStore.ApplyCharacterNames(slot, new[] { "A", "B", "Hero", "", "" }));
        Assert.Equal("Hero", slot.CharacterName);
        Assert.Equal("A", slot.CharacterNames[0]);
        Assert.Equal("Hero", slot.CharacterNames[2]);
    }
}
