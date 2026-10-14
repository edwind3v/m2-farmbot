using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Entities;

public sealed class FarmFilter
{
    public bool HuntMonsters { get; init; } = true;
    public bool HuntStones { get; init; } = true;
    public float MaxDistance { get; init; }
    public float FarmCenterX { get; init; }
    public float FarmCenterY { get; init; }
    public float FarmRadius { get; init; }
    public IReadOnlySet<uint> IncludeVnums { get; init; } = new HashSet<uint>();
    public IReadOnlySet<uint> ExcludeVnums { get; init; } = new HashSet<uint>();
    public IReadOnlyList<TargetRule> Rules { get; init; } = Array.Empty<TargetRule>();
    public int DefaultMonsterPriority { get; init; } = 10;
    public int DefaultStonePriority { get; init; } = 100;

    public static FarmFilter FromConfig(BotConfig config)
    {
        config.Sanitize();
        return new FarmFilter
        {
            HuntMonsters = config.HuntMonsters,
            HuntStones = config.HuntStones,
            MaxDistance = config.MaxFarmDistance,
            FarmCenterX = config.FarmCenterX,
            FarmCenterY = config.FarmCenterY,
            FarmRadius = config.FarmRadius,
            IncludeVnums = new HashSet<uint>(config.IncludeVnums),
            ExcludeVnums = new HashSet<uint>(config.ExcludeVnums),
            Rules = config.TargetRules,
            DefaultMonsterPriority = Highest(config.TargetRules, EntityKind.Monster, 10),
            DefaultStonePriority = Highest(config.TargetRules, EntityKind.Metin, 100)
        };
    }

    private static int Highest(IEnumerable<TargetRule> rules, EntityKind kind, int fallback)
    {
        var best = fallback;
        var found = false;
        foreach (var rule in rules)
        {
            if (rule.Kind != kind || rule.Vnum != 0)
                continue;
            if (!found || rule.Priority > best)
            {
                best = rule.Priority;
                found = true;
            }
        }

        return best;
    }
}
