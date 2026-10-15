using System.Text;
using System.Text.Json;
using FarmBot.Core.Combat;
using FarmBot.Core.Json;

namespace FarmBot.Core.Configuration;

public sealed class CharacterKit
{
    public string Account { get; set; } = "";
    public string Character { get; set; } = "";
    public List<PotionStack> Pots { get; set; } = new();
    public List<LearnedSkill> Skills { get; set; } = new();
}

public static class CharacterKitStore
{
    public const int PotionPollMs = 250;
    public const int SkillPollMs = 10000;

    public static string Root(string baseDirectory) => Path.Combine(baseDirectory, "cache");

    public static string FilePath(string baseDirectory, string account, string character) =>
        Path.Combine(Root(baseDirectory), Safe(account), Safe(character) + ".json");

    public static string Safe(string? value)
    {
        var raw = (value ?? "").Trim();
        if (raw.Length == 0)
            return "_unknown";
        var invalid = Path.GetInvalidFileNameChars();
        var n = Math.Min(raw.Length, 80);
        var buffer = new char[n];
        var written = 0;
        for (var i = 0; i < n; i++)
        {
            var c = raw[i];
            buffer[written++] = Array.IndexOf(invalid, c) >= 0 ? '_' : c;
        }

        return new string(buffer, 0, written);
    }

    public static (string Account, string Character) IdentityFromConfig(string baseDirectory, string configPath)
    {
        var slotId = Path.GetFileNameWithoutExtension(configPath) ?? "";
        try
        {
            var state = SlotStore.LoadOrCreate(SlotStore.DefaultPath(baseDirectory));
            foreach (var slot in state.AllSlots)
            {
                if (string.Equals(slot.Id, slotId, StringComparison.OrdinalIgnoreCase))
                    return ((slot.Account ?? "").Trim(), (slot.CharacterName ?? "").Trim());
            }
        }
        catch
        {
        }

        return ("", slotId);
    }

    public static bool TryLoad(string path, out CharacterKit kit)
    {
        kit = new CharacterKit();
        try
        {
            if (!File.Exists(path))
                return false;
            var loaded = JsonSerializer.Deserialize<CharacterKit>(File.ReadAllText(path), JsonDefaults.Instance);
            if (loaded == null)
                return false;
            loaded.Pots ??= new List<PotionStack>();
            loaded.Skills ??= new List<LearnedSkill>();
            foreach (var skill in loaded.Skills)
            {
                skill.CoolRemain = 0;
                skill.AffectRemain = 0;
            }

            kit = loaded;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Save(string path, CharacterKit kit)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        var copy = new CharacterKit
        {
            Account = kit.Account ?? "",
            Character = kit.Character ?? "",
            Pots = kit.Pots.Select(CopyPot).ToList(),
            Skills = kit.Skills.Select(ForCache).ToList()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(copy, JsonDefaults.Instance));
    }

    public static PotionStack CopyPot(PotionStack pot) => new()
    {
        Vnum = pot.Vnum,
        Name = pot.Name ?? "",
        Count = pot.Count,
        Slot = pot.Slot,
        Family = pot.Family,
        Icon = pot.Icon ?? "",
        DurationSec = pot.DurationSec,
        Tooltip = pot.Tooltip ?? ""
    };

    public static LearnedSkill ForCache(LearnedSkill skill) => new()
    {
        Index = skill.Index,
        Slot = skill.Slot,
        Name = skill.Name ?? "",
        Kind = string.IsNullOrWhiteSpace(skill.Kind) ? "attack" : skill.Kind,
        Level = skill.Level,
        CoolRemain = 0,
        AffectRemain = 0,
        DurationSec = skill.DurationSec,
        Icon = skill.Icon ?? ""
    };

    public static IReadOnlyList<PotionStack> MergePots(
        IReadOnlyList<PotionStack> live,
        IReadOnlyList<PotionStack> cached)
    {
        if (live.Count == 0)
            return Array.Empty<PotionStack>();
        var byVnum = IndexPots(cached);
        var list = new List<PotionStack>(live.Count);
        foreach (var pot in live)
        {
            byVnum.TryGetValue(pot.Vnum, out var old);
            list.Add(new PotionStack
            {
                Vnum = pot.Vnum,
                Name = First(pot.Name, old?.Name),
                Count = pot.Count,
                Slot = pot.Slot,
                Family = pot.Family,
                Icon = First(pot.Icon, old?.Icon),
                DurationSec = pot.DurationSec != 0 ? pot.DurationSec : old?.DurationSec ?? 0,
                Tooltip = First(pot.Tooltip, old?.Tooltip, pot.Name, old?.Name)
            });
        }

        return list;
    }

    public static IReadOnlyList<LearnedSkill> MergeSkills(
        IReadOnlyList<LearnedSkill> live,
        IReadOnlyList<LearnedSkill> cached)
    {
        if (live.Count == 0)
            return Array.Empty<LearnedSkill>();
        var byIndex = IndexSkills(cached);
        var list = new List<LearnedSkill>(live.Count);
        foreach (var skill in live)
        {
            byIndex.TryGetValue(skill.Index, out var old);
            list.Add(new LearnedSkill
            {
                Index = skill.Index,
                Slot = skill.Slot,
                Name = First(skill.Name, old?.Name),
                Kind = string.IsNullOrWhiteSpace(skill.Kind) ? "attack" : skill.Kind,
                Level = skill.Level,
                CoolRemain = skill.CoolRemain,
                AffectRemain = skill.AffectRemain,
                DurationSec = skill.DurationSec != 0 ? skill.DurationSec : old?.DurationSec ?? 0,
                Icon = First(skill.Icon, old?.Icon)
            });
        }

        return list;
    }

    public static string Fingerprint(IReadOnlyList<PotionStack> pots, IReadOnlyList<LearnedSkill> skills)
    {
        var text = new StringBuilder();
        foreach (var pot in pots)
        {
            text.Append(pot.Vnum).Append(':')
                .Append(pot.Slot).Append(':')
                .Append(pot.Count).Append(':')
                .Append((int)pot.Family).Append(':')
                .Append(pot.Name).Append(':')
                .Append(pot.DurationSec).Append(':')
                .Append(pot.Icon?.Length ?? 0).Append('|');
        }

        text.Append('#');
        foreach (var skill in skills)
        {
            text.Append(skill.Index).Append(':')
                .Append(skill.Slot).Append(':')
                .Append(skill.Level).Append(':')
                .Append(skill.Name).Append(':')
                .Append(skill.DurationSec).Append(':')
                .Append(skill.Icon?.Length ?? 0).Append('|');
        }

        return text.ToString();
    }

    private static Dictionary<uint, PotionStack> IndexPots(IReadOnlyList<PotionStack> pots)
    {
        var map = new Dictionary<uint, PotionStack>();
        foreach (var pot in pots)
        {
            if (pot.Vnum != 0 && !map.ContainsKey(pot.Vnum))
                map[pot.Vnum] = pot;
        }

        return map;
    }

    private static Dictionary<uint, LearnedSkill> IndexSkills(IReadOnlyList<LearnedSkill> skills)
    {
        var map = new Dictionary<uint, LearnedSkill>();
        foreach (var skill in skills)
        {
            if (skill.Index != 0 && !map.ContainsKey(skill.Index))
                map[skill.Index] = skill;
        }

        return map;
    }

    private static string First(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return "";
    }
}
