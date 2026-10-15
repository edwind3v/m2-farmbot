using FarmBot.Core.Models;

namespace FarmBot;

public static class PlayerAvatar
{
    private static readonly Dictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static string Src(uint? race)
    {
        if (!PlayerRace.TryMap(race, out var info))
            return "";
        lock (Cache)
        {
            if (Cache.TryGetValue(info.FileName, out var cached))
                return cached;
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "avatars", info.FileName);
            var src = File.Exists(path)
                ? "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(path))
                : "avatars/" + info.FileName;
            Cache[info.FileName] = src;
            return src;
        }
    }
}
