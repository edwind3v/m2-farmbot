using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmBot.Core.Combat;
using FarmBot.Core.Json;

namespace FarmBot.Core.Configuration;

public enum ClientProfile
{
    Normal,
    Arthion,
    Baerim
}

public sealed class ClientSlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Account { get; set; } = "";
    public string PasswordProtected { get; set; } = "";
    public int CharacterSlot { get; set; }
    public string CharacterName { get; set; } = "";
    public List<string> CharacterNames { get; set; } = new();
    public uint? Race { get; set; }
    public int ChannelIndex { get; set; }
    public string ChannelName { get; set; } = "";

    [System.Text.Json.Serialization.JsonIgnore]
    public string DisplayName
    {
        get
        {
            var account = string.IsNullOrWhiteSpace(Account) ? "empty" : Account.Trim();
            var character = string.IsNullOrWhiteSpace(CharacterName) ? "—" : CharacterName.Trim();
            var channel = string.IsNullOrWhiteSpace(ChannelName) ? $"CH-{ChannelIndex + 1}" : ChannelName.Trim();
            return $"{account} · {character} · {channel}";
        }
    }
}

public sealed class ProfileLaunchState
{
    public string ClientPath { get; set; } = "";
    public List<ClientSlot> Slots { get; set; } = new();
}

public sealed class LauncherState
{
    public string Profile { get; set; } = nameof(ClientProfile.Arthion);
    public string ClientPath { get; set; } = "";
    public List<ClientSlot> Slots { get; set; } = new();
    public ProfileLaunchState Normal { get; set; } = new();
    public ProfileLaunchState Arthion { get; set; } = new();
    public ProfileLaunchState Baerim { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    public ClientProfile SelectedProfile => SlotStore.ParseProfile(Profile);

    [System.Text.Json.Serialization.JsonIgnore]
    public ProfileLaunchState Active => SelectedProfile switch
    {
        ClientProfile.Baerim => Baerim,
        ClientProfile.Arthion => Arthion,
        _ => Normal
    };

    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<ClientSlot> AllSlots =>
        (Normal?.Slots ?? Enumerable.Empty<ClientSlot>())
        .Concat(Arthion?.Slots ?? Enumerable.Empty<ClientSlot>())
        .Concat(Baerim?.Slots ?? Enumerable.Empty<ClientSlot>());
}

public static class SlotStore
{
    public static string DefaultPath(string baseDirectory) => Path.Combine(baseDirectory, "slots.json");

    public static string ConfigPath(string baseDirectory, string slotId) =>
        Path.Combine(baseDirectory, "configs", slotId + ".json");

    public static LauncherState LoadOrCreate(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var loaded = JsonSerializer.Deserialize<LauncherState>(File.ReadAllText(path), JsonDefaults.Instance);
                if (loaded != null)
                {
                    Sanitize(loaded);
                    return loaded;
                }
            }
        }
        catch
        {
            // A damaged launcher file is replaced with defaults below.
        }

        var created = new LauncherState();
        Sanitize(created);
        Save(path, created);
        return created;
    }

    public static void Save(string path, LauncherState state)
    {
        Sanitize(state);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(state, JsonDefaults.Instance));
    }

    public static BotConfig LoadBotConfig(string baseDirectory, ClientSlot slot)
    {
        var path = ConfigPath(baseDirectory, slot.Id);
        if (File.Exists(path))
            return ConfigStore.LoadOrCreate(path);

        var template = ConfigStore.DefaultPath(baseDirectory);
        var config = File.Exists(template)
            ? ConfigStore.LoadOrCreate(template)
            : BotConfig.CreateDefault();
        ConfigStore.Save(path, config);
        return config;
    }

    public static bool ApplyLiveCredentials(ClientSlot slot, ActionResponse info, string? playerName = null)
    {
        var dirty = false;
        var account = (info.Account ?? "").Trim();
        if (account.Length > 0 && !string.Equals(slot.Account, account, StringComparison.Ordinal))
        {
            slot.Account = account;
            dirty = true;
        }

        if (!string.IsNullOrWhiteSpace(info.Password))
        {
            var current = UnprotectPassword(slot.PasswordProtected);
            if (current != info.Password)
            {
                slot.PasswordProtected = ProtectPassword(info.Password);
                dirty = true;
            }
        }

        var character = (info.Character ?? "").Trim();
        if (character.Length == 0)
            character = (playerName ?? "").Trim();
        if (character.Length > 0 && !string.Equals(slot.CharacterName, character, StringComparison.Ordinal))
        {
            slot.CharacterName = character;
            dirty = true;
        }

        var liveCharacter = !string.IsNullOrWhiteSpace(info.Character);
        var inWorld = liveCharacter || PhaseLooksInWorld(info.Phase);
        if (inWorld &&
            info.CharacterSlot is >= 0 and <= 4 &&
            slot.CharacterSlot != info.CharacterSlot)
        {
            slot.CharacterSlot = info.CharacterSlot;
            dirty = true;
        }

        return dirty;
    }

    public static string ProtectPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "";
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(bytes);
    }

    public static string UnprotectPassword(string protectedPassword)
    {
        if (string.IsNullOrWhiteSpace(protectedPassword))
            return "";
        try
        {
            var bytes = ProtectedData.Unprotect(
                Convert.FromBase64String(protectedPassword),
                null,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (CryptographicException)
        {
            return "";
        }
        catch (FormatException)
        {
            return "";
        }
    }

    public static ClientProfile ParseProfile(string? value)
    {
        if (string.Equals(value, nameof(ClientProfile.Normal), StringComparison.OrdinalIgnoreCase))
            return ClientProfile.Normal;
        if (string.Equals(value, nameof(ClientProfile.Baerim), StringComparison.OrdinalIgnoreCase))
            return ClientProfile.Baerim;
        return ClientProfile.Arthion;
    }

    public static string ProfileName(ClientProfile profile) => profile switch
    {
        ClientProfile.Normal => nameof(ClientProfile.Normal),
        ClientProfile.Baerim => nameof(ClientProfile.Baerim),
        _ => nameof(ClientProfile.Arthion)
    };

    public const string DefaultArthionPath = @"E:\Arthion\game\Arthion.exe";
    public const string DefaultBaerimPath = @"E:\Baerim\client\baerim.exe";

    public static bool IsArthionPath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        Path.GetFileName(path).Contains("Arthion", StringComparison.OrdinalIgnoreCase);

    public static bool IsBaerimPath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        Path.GetFileName(path).Contains("baerim", StringComparison.OrdinalIgnoreCase);

    public static void Sanitize(LauncherState state)
    {
        state.Normal ??= new ProfileLaunchState();
        state.Arthion ??= new ProfileLaunchState();
        state.Baerim ??= new ProfileLaunchState();
        state.Normal.Slots ??= new List<ClientSlot>();
        state.Arthion.Slots ??= new List<ClientSlot>();
        state.Baerim.Slots ??= new List<ClientSlot>();
        state.Normal.ClientPath ??= "";
        state.Arthion.ClientPath ??= "";
        state.Baerim.ClientPath ??= "";
        state.ClientPath ??= "";
        state.Slots ??= new List<ClientSlot>();
        MigrateLegacySlots(state);
        state.Profile = ProfileName(ParseProfile(state.Profile));
        if (state.SelectedProfile != ClientProfile.Baerim && state.Active.Slots.Count == 0)
        {
            if (state.Arthion.Slots.Count > 0)
                state.Profile = nameof(ClientProfile.Arthion);
            else if (state.Normal.Slots.Count > 0)
                state.Profile = nameof(ClientProfile.Normal);
        }
        if (string.IsNullOrWhiteSpace(state.Arthion.ClientPath) && IsArthionPath(state.ClientPath))
            state.Arthion.ClientPath = state.ClientPath;
        if (string.IsNullOrWhiteSpace(state.Baerim.ClientPath) && IsBaerimPath(state.ClientPath))
            state.Baerim.ClientPath = state.ClientPath;
        EnsureProfileClientPaths(state);
        MirrorActive(state);
        foreach (var slot in state.AllSlots)
            SanitizeSlot(slot);
    }

    public static void EnsureProfileClientPaths(LauncherState state)
    {
        state.Normal ??= new ProfileLaunchState();
        state.Arthion ??= new ProfileLaunchState();
        state.Baerim ??= new ProfileLaunchState();
        state.Normal.ClientPath ??= "";
        state.Arthion.ClientPath ??= "";
        state.Baerim.ClientPath ??= "";

        if (string.IsNullOrWhiteSpace(state.Normal.ClientPath) ||
            IsArthionPath(state.Normal.ClientPath) ||
            IsBaerimPath(state.Normal.ClientPath))
            state.Normal.ClientPath = BotConfig.CreateDefault().ClientPath;

        if (NeedsArthionDefault(state.Arthion.ClientPath))
            state.Arthion.ClientPath = DefaultArthionPath;

        if (string.IsNullOrWhiteSpace(state.Baerim.ClientPath) || IsArthionPath(state.Baerim.ClientPath))
            state.Baerim.ClientPath = DefaultBaerimPath;
    }

    private static bool NeedsArthionDefault(string? path) =>
        string.IsNullOrWhiteSpace(path) || IsBaerimPath(path);

    private static void MigrateLegacySlots(LauncherState state)
    {
        if (state.Normal.Slots.Count > 0 || state.Arthion.Slots.Count > 0 || state.Slots.Count == 0)
            return;

        for (var i = 0; i < state.Slots.Count; i++)
        {
            if (i < 2)
                state.Normal.Slots.Add(state.Slots[i]);
            else
                state.Arthion.Slots.Add(state.Slots[i]);
        }

        if (IsArthionPath(state.ClientPath))
            state.Arthion.ClientPath = state.ClientPath;
        else if (IsBaerimPath(state.ClientPath))
            state.Baerim.ClientPath = state.ClientPath;
        else if (!string.IsNullOrWhiteSpace(state.ClientPath))
            state.Normal.ClientPath = state.ClientPath;

        if (string.IsNullOrWhiteSpace(state.Profile))
            state.Profile = nameof(ClientProfile.Arthion);
    }

    public static void MirrorActive(LauncherState state)
    {
        var active = state.Active;
        state.ClientPath = active.ClientPath ?? "";
        state.Slots = active.Slots;
    }

    private static void SanitizeSlot(ClientSlot slot)
    {
        if (string.IsNullOrWhiteSpace(slot.Id))
            slot.Id = Guid.NewGuid().ToString("N");
        slot.Account = (slot.Account ?? "").Trim();
        slot.PasswordProtected ??= "";
        slot.CharacterName = (slot.CharacterName ?? "").Trim();
        slot.ChannelName = (slot.ChannelName ?? "").Trim();
        slot.CharacterNames ??= new List<string>();
        while (slot.CharacterNames.Count < 5)
            slot.CharacterNames.Add("");
        if (slot.CharacterNames.Count > 5)
            slot.CharacterNames.RemoveRange(5, slot.CharacterNames.Count - 5);
        for (var i = 0; i < slot.CharacterNames.Count; i++)
            slot.CharacterNames[i] = (slot.CharacterNames[i] ?? "").Trim();
        slot.CharacterSlot = Math.Clamp(slot.CharacterSlot, 0, 4);
        slot.ChannelIndex = Math.Max(0, slot.ChannelIndex);
        if (slot.Race is > 8)
            slot.Race = null;
    }

    public static bool ApplyCharacterNames(ClientSlot slot, IReadOnlyList<string>? names)
    {
        if (slot == null || names == null || names.Count == 0)
            return false;
        slot.CharacterNames ??= new List<string>();
        while (slot.CharacterNames.Count < 5)
            slot.CharacterNames.Add("");
        var dirty = false;
        for (var i = 0; i < 5 && i < names.Count; i++)
        {
            var name = (names[i] ?? "").Trim();
            if (string.Equals(slot.CharacterNames[i], name, StringComparison.Ordinal))
                continue;
            slot.CharacterNames[i] = name;
            dirty = true;
        }

        var selected = Math.Clamp(slot.CharacterSlot, 0, 4);
        if (selected < slot.CharacterNames.Count)
        {
            var pick = slot.CharacterNames[selected];
            if (pick.Length > 0 && !string.Equals(slot.CharacterName, pick, StringComparison.Ordinal))
            {
                slot.CharacterName = pick;
                dirty = true;
            }
        }

        return dirty;
    }

    private static bool PhaseLooksInWorld(string phase) =>
        !string.IsNullOrWhiteSpace(phase) &&
        (phase.Contains("Select", StringComparison.OrdinalIgnoreCase) ||
         phase.Contains("Game", StringComparison.OrdinalIgnoreCase));
}
