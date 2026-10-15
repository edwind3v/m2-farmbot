namespace FarmBot.Core.Models;

public readonly record struct PlayerRaceInfo(string ClassId, string Sex, string FileName, string Label);

public static class PlayerRace
{
    public static bool TryMap(uint? race, out PlayerRaceInfo info)
    {
        info = race switch
        {
            0 => new("warrior", "male", "warrior-male.png", "Warrior"),
            1 => new("ninja", "female", "ninja-female.png", "Ninja"),
            2 => new("sura", "male", "sura-male.png", "Sura"),
            3 => new("shaman", "female", "shaman-female.png", "Shaman"),
            4 => new("warrior", "female", "warrior-female.png", "Warrior"),
            5 => new("ninja", "male", "ninja-male.png", "Ninja"),
            6 => new("sura", "female", "sura-female.png", "Sura"),
            7 => new("shaman", "male", "shaman-male.png", "Shaman"),
            8 => new("lycan", "male", "lycan-male.png", "Lycan"),
            _ => default
        };
        return !string.IsNullOrEmpty(info.ClassId);
    }

    public static string AvatarUrl(uint? race) =>
        TryMap(race, out var info) ? "avatars/" + info.FileName : "";

    public static string Caption(uint? race)
    {
        if (!TryMap(race, out var info))
            return "";
        return info.ClassId == "lycan" ? info.Label : $"{info.Label} · {Title(info.Sex)}";
    }

    private static string Title(string sex) =>
        sex.Length == 0 ? sex : char.ToUpperInvariant(sex[0]) + sex[1..];
}
