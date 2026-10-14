using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmBot.Core.Combat;
using FarmBot.Core.Json;

namespace FarmBot.Core.Configuration;

public sealed class ClientSlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Account { get; set; } = "";
    public string PasswordProtected { get; set; } = "";
    public int CharacterSlot { get; set; }
    public string CharacterName { get; set; } = "";
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

public sealed class LauncherState
{
    public string ClientPath { get; set; } = BotConfig.CreateDefault().ClientPath;
    public List<ClientSlot> Slots { get; set; } = new();
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

    public static void Sanitize(LauncherState state)
    {
        state.ClientPath ??= "";
        state.Slots ??= new List<ClientSlot>();
        foreach (var slot in state.Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Id))
                slot.Id = Guid.NewGuid().ToString("N");
            slot.Account = (slot.Account ?? "").Trim();
            slot.PasswordProtected ??= "";
            slot.CharacterName = (slot.CharacterName ?? "").Trim();
            slot.ChannelName = (slot.ChannelName ?? "").Trim();
            slot.CharacterSlot = Math.Clamp(slot.CharacterSlot, 0, 4);
            slot.ChannelIndex = Math.Max(0, slot.ChannelIndex);
        }
    }

    private static bool PhaseLooksInWorld(string phase) =>
        !string.IsNullOrWhiteSpace(phase) &&
        (phase.Contains("Select", StringComparison.OrdinalIgnoreCase) ||
         phase.Contains("Game", StringComparison.OrdinalIgnoreCase));
}
