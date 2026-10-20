using System.Text;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;

namespace FarmBot.Tests;

public class LoginNativeLayoutTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void RecoversLoginApisFromTheReleaseImage()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var layout = PlayerActionLayoutRecoverer.RecoverLogin(ReleaseClient);
        var dump = $"net=0x{layout.NetStreamRva:X} account=0x{layout.AccountConnectorRva:X} setLogin=0x{layout.SetLoginInfoRva:X} accountSetLogin=0x{layout.AccountSetLoginInfoRva:X} connect=0x{layout.AccountConnectRva:X} direct=0x{layout.DirectEnterRva:X} select=0x{layout.SelectCharacterRva:X} setPhase=0x{layout.SetLoginPhaseRva:X} logoClose=0x{layout.OnLogoCloseRva:X} phaseDisp=0x{layout.PhaseDisplacement:X}\n" +
                   string.Join("\n", layout.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\loginlayout.txt", dump);

        Assert.True(layout.CanLogin, dump);
        Assert.Equal(LoginNativeContract.NetStreamRva, layout.NetStreamRva);
        Assert.Equal(LoginNativeContract.AccountConnectorRva, layout.AccountConnectorRva);
        Assert.Equal(LoginNativeContract.SetLoginInfoRva, layout.SetLoginInfoRva);
        Assert.Equal(LoginNativeContract.AccountSetLoginInfoRva, layout.AccountSetLoginInfoRva);
        Assert.Equal(LoginNativeContract.AccountConnectRva, layout.AccountConnectRva);
        Assert.Equal(LoginNativeContract.ConnectGameServerRva, layout.DirectEnterRva);
        Assert.Equal(LoginNativeContract.SelectCharacterRva, layout.SelectCharacterRva);
        Assert.Equal(LoginNativeContract.SetLoginPhaseRva, layout.SetLoginPhaseRva);
        Assert.Equal(LoginNativeContract.OnLogoCloseRva, layout.OnLogoCloseRva);
        Assert.Equal(LoginNativeContract.PhaseDisplacement, layout.PhaseDisplacement);
        Assert.Equal(LoginNativeContract.SetGamePhaseRva, layout.SetGamePhaseRva);
        Assert.Equal(LoginNativeContract.StartGameRva, layout.StartGameRva);
        Assert.Equal(LoginNativeContract.NotifyGamePhaseRva, layout.NotifyGamePhaseRva);
        Assert.Equal(LoginNativeContract.SendEnterGamePacketRva, layout.SendEnterGamePacketRva);
        Assert.Equal(0x117660u, layout.AccountSetLoginInfoRva);
        Assert.Contains("StartGame native 0x13CD40", dump);
        Assert.Contains("NotifyGamePhase 0x11C0C0", dump);
        Assert.Contains("layout=recovered", layout.ToNativeConfig());
        Assert.Contains("layout=recovered", layout.ToNativeConfig());
        Assert.Contains("net=482BA0", layout.ToNativeConfig());
        Assert.Contains("setLoginPhase=13D240", layout.ToNativeConfig());
    }

    [Fact]
    public void RecoversLoginApisFromARenamedOrDifferentClientWhenPresent()
    {
        const string arthion = @"E:\Arthion\game\Arthion.exe";
        if (!File.Exists(arthion))
            return;

        var layout = PlayerActionLayoutRecoverer.RecoverLogin(arthion);
        var dump = layout.ToNativeConfig() + "\n" + string.Join("\n", layout.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\arthion-loginlayout.txt", dump);
        Assert.True(layout.CanLogin, dump);
        Assert.NotEqual(LoginNativeContract.SetLoginInfoRva, layout.SetLoginInfoRva);
        Assert.NotEqual(0u, layout.SetLoginInfoRva);
        Assert.NotEqual(0u, layout.DirectEnterRva);
        Assert.NotEqual(0u, layout.StartGameRva);
    }

    [Fact]
    public void StartGameOnlySetsTheLoadingCompleteFlag()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (LoginNativeContract.StartGameRva >= va && LoginNativeContract.StartGameRva < va + rawSize)
            {
                file = (int)(raw + (LoginNativeContract.StartGameRva - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 11 <= bytes.Length);
        Assert.Equal(new byte[] { 0xC7, 0x81, 0x8C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xC3 },
            bytes.AsSpan(file, 11).ToArray());
    }

    [Fact]
    public void NotifyGamePhasePyCallsSetGamePhaseWhenStartFlagIsSet()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (LoginNativeContract.NotifyGamePhaseRva >= va && LoginNativeContract.NotifyGamePhaseRva < va + rawSize)
            {
                file = (int)(raw + (LoginNativeContract.NotifyGamePhaseRva - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 16 <= bytes.Length);
        Assert.Equal(new byte[] { 0x56, 0x8B, 0xF1, 0x83, 0xBE, 0x8C, 0x00, 0x00, 0x00, 0x00, 0x74, 0x29 },
            bytes.AsSpan(file, 12).ToArray());
    }

    [Fact]
    public void SetLoginInfoWritesAccountThenPasswordOnTheNetworkStream()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (LoginNativeContract.SetLoginInfoRva >= va && LoginNativeContract.SetLoginInfoRva < va + rawSize)
            {
                file = (int)(raw + (LoginNativeContract.SetLoginInfoRva - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 64 <= bytes.Length);
        var body = bytes.AsSpan(file, 64).ToArray();
        Assert.True(IndexOf(body, new byte[] { 0x8D, 0x8F, 0xB0, 0x00, 0x00, 0x00 }) >= 0);
        Assert.True(IndexOf(body, new byte[] { 0x8D, 0x8F, 0xC8, 0x00, 0x00, 0x00 }) >= 0);
        Assert.Equal(0xB0u, LoginNativeContract.LoginIdDisplacement);
        Assert.Equal(0xC8u, LoginNativeContract.LoginPasswordDisplacement);
    }

    [Fact]
    public void AccountConnectorSetLoginInfoMatchesNetworkStreamPrologue()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int FileOf(uint rva)
        {
            for (var i = 0; i < sections; i++)
            {
                var at = off + i * 40;
                var va = BitConverter.ToUInt32(bytes, at + 12);
                var raw = BitConverter.ToUInt32(bytes, at + 20);
                var rawSize = BitConverter.ToUInt32(bytes, at + 16);
                if (rva >= va && rva < va + rawSize)
                    return (int)(raw + (rva - va));
            }

            return -1;
        }

        var net = FileOf(LoginNativeContract.SetLoginInfoRva);
        var acc = FileOf(LoginNativeContract.AccountSetLoginInfoRva);
        Assert.True(net >= 0 && acc >= 0);
        Assert.Equal(bytes[net], bytes[acc]);
        Assert.Equal(0x55, bytes[acc]);
        Assert.Equal(0x8B, bytes[acc + 1]);
        Assert.NotEqual(0xC2, bytes[acc]);
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i + needle.Length <= haystack.Length; i++)
        {
            var found = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return i;
        }

        return -1;
    }
}

public class SlotStoreTests
{
    [Fact]
    public void ProtectsPasswordAndDoesNotWriteItPlaintext()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-slots-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = SlotStore.DefaultPath(dir);
            var state = new LauncherState
            {
                ClientPath = @"E:\client\1.metin2client.exe",
                Slots =
                {
                    new ClientSlot
                    {
                        Account = "hero",
                        PasswordProtected = SlotStore.ProtectPassword("s3cret"),
                        CharacterSlot = 2,
                        ChannelIndex = 1,
                        ChannelName = "CH-2"
                    }
                }
            };
            SlotStore.Save(path, state);
            var json = File.ReadAllText(path);
            Assert.DoesNotContain("s3cret", json);
            Assert.Contains("hero", json);
            var loaded = SlotStore.LoadOrCreate(path);
            Assert.Equal("s3cret", SlotStore.UnprotectPassword(loaded.Slots[0].PasswordProtected));
            Assert.Equal(2, loaded.Slots[0].CharacterSlot);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ApplyLiveCredentialsSavesAccountSeparateFromCharacter()
    {
        var slot = new ClientSlot();
        Assert.True(SlotStore.ApplyLiveCredentials(slot, new ActionResponse
        {
            Account = "player1",
            Password = "pw",
            Character = "Hero",
            CharacterSlot = 2
        }, "Hero"));
        Assert.Equal("player1", slot.Account);
        Assert.Equal("Hero", slot.CharacterName);
        Assert.Equal(2, slot.CharacterSlot);
        Assert.Equal("pw", SlotStore.UnprotectPassword(slot.PasswordProtected));
        Assert.NotEqual(slot.CharacterName, slot.Account);
    }

    [Fact]
    public void ApplyLiveCredentialsDoesNotOverwriteSavedSlotDuringLogin()
    {
        var slot = new ClientSlot { Account = "admin", CharacterName = "test2", CharacterSlot = 3 };
        Assert.False(SlotStore.ApplyLiveCredentials(slot, new ActionResponse
        {
            Account = "admin",
            Character = "",
            CharacterSlot = 0,
            Phase = "Login"
        }));
        Assert.Equal(3, slot.CharacterSlot);
        Assert.Equal("test2", slot.CharacterName);
    }

    [Fact]
    public void ApplyLiveCredentialsUpdatesSlotWhenCharacterIsInGame()
    {
        var slot = new ClientSlot { Account = "admin", CharacterName = "test2", CharacterSlot = 3 };
        Assert.True(SlotStore.ApplyLiveCredentials(slot, new ActionResponse
        {
            Account = "admin",
            Character = "Admin",
            CharacterSlot = 0,
            Phase = "Game"
        }, "Admin"));
        Assert.Equal(0, slot.CharacterSlot);
        Assert.Equal("Admin", slot.CharacterName);
    }

    [Fact]
    public void ApplyLiveCredentialsDoesNotUseCharacterAsAccount()
    {
        var slot = new ClientSlot { Account = "player1", CharacterName = "Hero", CharacterSlot = 2 };
        Assert.False(SlotStore.ApplyLiveCredentials(slot, new ActionResponse
        {
            Account = "",
            Character = "Hero",
            CharacterSlot = -1
        }, "Hero"));
        Assert.Equal("player1", slot.Account);
        Assert.Equal("Hero", slot.CharacterName);
        Assert.Equal(2, slot.CharacterSlot);
    }

    [Fact]
    public void MigratesFirstTwoLegacySlotsToNormalAndTheLastToArthion()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-slots-migrate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = SlotStore.DefaultPath(dir);
            File.WriteAllText(path, """
                {
                  "clientPath": "E:\\Arthion\\game\\Arthion.exe",
                  "slots": [
                    { "id": "one", "account": "first", "characterName": "scolar" },
                    { "id": "two", "account": "admin", "characterName": "test2" },
                    { "id": "three", "account": "edwin01", "characterName": "lockedin" }
                  ]
                }
                """);
            var loaded = SlotStore.LoadOrCreate(path);
            Assert.Equal(ClientProfile.Arthion, loaded.SelectedProfile);
            Assert.Equal(2, loaded.Normal.Slots.Count);
            Assert.Equal("first", loaded.Normal.Slots[0].Account);
            Assert.Equal("admin", loaded.Normal.Slots[1].Account);
            Assert.Single(loaded.Arthion.Slots);
            Assert.Equal("edwin01", loaded.Arthion.Slots[0].Account);
            Assert.Equal(@"E:\Arthion\game\Arthion.exe", loaded.Arthion.ClientPath);
            Assert.Equal(@"E:\Arthion\game\Arthion.exe", loaded.ClientPath);
            Assert.Equal("edwin01", loaded.Slots[0].Account);

            loaded.Profile = nameof(ClientProfile.Normal);
            SlotStore.Sanitize(loaded);
            Assert.Equal(ClientProfile.Normal, loaded.SelectedProfile);
            Assert.Equal(2, loaded.Slots.Count);
            Assert.Equal("first", loaded.Slots[0].Account);
            Assert.Contains("metin2client", loaded.ClientPath, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ApplyLiveCredentialsKeepsPasswordWhenLoginPacketNoLongerHasIt()
    {
        var slot = new ClientSlot { Account = "player1", PasswordProtected = SlotStore.ProtectPassword("old") };
        Assert.True(SlotStore.ApplyLiveCredentials(slot, new ActionResponse
        {
            Account = "player1",
            Password = "",
            Character = "Hero",
            CharacterSlot = 1
        }, "Hero"));
        Assert.Equal("old", SlotStore.UnprotectPassword(slot.PasswordProtected));
        Assert.Equal("Hero", slot.CharacterName);
        Assert.Equal(1, slot.CharacterSlot);
    }

    [Fact]
    public void SanitizeDefaultsArthionPathAndReplacesLeakedBaerimPath()
    {
        var empty = new LauncherState { Profile = nameof(ClientProfile.Arthion) };
        SlotStore.Sanitize(empty);
        Assert.Equal(ClientProfile.Arthion, empty.SelectedProfile);
        Assert.Equal(SlotStore.DefaultArthionPath, empty.ClientPath);
        Assert.Equal(SlotStore.DefaultArthionPath, empty.Arthion.ClientPath);
        Assert.Equal(SlotStore.DefaultBaerimPath, empty.Baerim.ClientPath);

        var leaked = new LauncherState
        {
            Profile = nameof(ClientProfile.Arthion),
            ClientPath = SlotStore.DefaultBaerimPath,
            Arthion = { ClientPath = SlotStore.DefaultBaerimPath }
        };
        SlotStore.Sanitize(leaked);
        Assert.Equal(SlotStore.DefaultArthionPath, leaked.ClientPath);
        Assert.Equal(SlotStore.DefaultArthionPath, leaked.Arthion.ClientPath);
        Assert.Equal(SlotStore.DefaultBaerimPath, leaked.Baerim.ClientPath);
    }

    [Fact]
    public void SanitizeKeepsEmptyBaerimSelectionAndDoesNotTouchArthionSlots()
    {
        var state = new LauncherState
        {
            Profile = nameof(ClientProfile.Baerim),
            Arthion =
            {
                ClientPath = @"E:\Arthion\game\Arthion.exe",
                Slots = { new ClientSlot { Account = "arthion-acc", CharacterName = "lockedin" } }
            }
        };
        SlotStore.Sanitize(state);
        Assert.Equal(ClientProfile.Baerim, state.SelectedProfile);
        Assert.Equal(@"E:\Baerim\client\baerim.exe", state.ClientPath);
        Assert.Equal(@"E:\Baerim\client\baerim.exe", state.Baerim.ClientPath);
        Assert.Empty(state.Slots);
        Assert.Single(state.Arthion.Slots);
        Assert.Equal("arthion-acc", state.Arthion.Slots[0].Account);
        Assert.Equal(@"E:\Arthion\game\Arthion.exe", state.Arthion.ClientPath);
    }

    [Fact]
    public void SwitchingProfilesMirrorsOnlyThatBucketAndKeepsSeparateClientPaths()
    {
        var state = new LauncherState
        {
            Profile = nameof(ClientProfile.Arthion),
            Arthion =
            {
                ClientPath = @"E:\Arthion\game\Arthion.exe",
                Slots = { new ClientSlot { Account = "arthion-acc", CharacterName = "lockedin" } }
            },
            Baerim =
            {
                ClientPath = @"E:\Baerim\client\baerim.exe",
                Slots = { new ClientSlot { Account = "baerim-acc", CharacterName = "baerim-hero" } }
            }
        };
        SlotStore.Sanitize(state);
        Assert.Equal(ClientProfile.Arthion, state.SelectedProfile);
        Assert.Equal("arthion-acc", state.Slots[0].Account);
        Assert.Equal(@"E:\Arthion\game\Arthion.exe", state.ClientPath);

        state.Profile = nameof(ClientProfile.Baerim);
        SlotStore.MirrorActive(state);
        Assert.Equal("baerim-acc", state.Slots[0].Account);
        Assert.Equal(@"E:\Baerim\client\baerim.exe", state.ClientPath);
        state.Slots[0].Account = "mutated-baerim";
        Assert.Equal("arthion-acc", state.Arthion.Slots[0].Account);
        Assert.Equal(@"E:\Arthion\game\Arthion.exe", state.Arthion.ClientPath);

        state.Profile = nameof(ClientProfile.Arthion);
        SlotStore.MirrorActive(state);
        Assert.Equal("arthion-acc", state.Slots[0].Account);
        Assert.Equal("mutated-baerim", state.Baerim.Slots[0].Account);
        Assert.Equal(@"E:\Baerim\client\baerim.exe", state.Baerim.ClientPath);
    }
}

public class LoaderOutputTests
{
    [Fact]
    public void ParsesLaunchedPidAndBuildsUniquePipes()
    {
        Assert.True(LoaderOutput.TryParseLaunchedPid("client window did not appear; injecting anyway\nlaunched pid=4242\n", out var pid));
        Assert.Equal(4242, pid);
        Assert.False(LoaderOutput.TryParseLaunchedPid("attached pid=9", out _));
        var pipe = LoaderOutput.UniquePipeName("slot-abc123");
        Assert.StartsWith("FarmBotAction_", pipe);
        Assert.DoesNotContain("-", pipe[14..]);
    }
}

public class ServerInfoParserTests
{
    [Fact]
    public void ParsesSrv1ChannelsFromServerInfo()
    {
        var text = """
            SRV1 = {
            	"name":TextColor("ServerName", "ffd500"),
            	"host":"192.168.1.100",
            	"auth1":30001,
            	"ch1":30003,
            	"ch2":30007,
            	"ch3":30011,
            	"ch4":30015,
            }
            """;
        var channels = FarmBot.Core.Configuration.ServerInfoParser.Parse(text);
        Assert.Equal(4, channels.Count);
        Assert.Equal("192.168.1.100", channels[0].Host);
        Assert.Equal(30001, channels[0].AuthPort);
        Assert.Equal(30003, channels[0].Port);
        Assert.Equal("CH-2", channels[1].Name);
        Assert.Equal(30007, channels[1].Port);
    }
}
