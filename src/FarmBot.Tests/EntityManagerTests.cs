using FarmBot.Core.Configuration;
using FarmBot.Core.Entities;
using FarmBot.Core.Models;

namespace FarmBot.Tests;

public class EntityManagerTests
{
    private static readonly Entity Main = new()
    {
        Vid = 99,
        Type = ActorType.Player,
        X = 0,
        Y = 0,
        Z = 0,
        Name = "Hero"
    };

    private static Entity Mob(uint vid, ActorType type, uint vnum, float x, float y, float z = 0, string? name = null, int hp = 100) => new()
    {
        Vid = vid,
        Type = type,
        Vnum = vnum,
        X = x,
        Y = y,
        Z = z,
        Name = name ?? $"{type}-{vnum}",
        Hp = hp
    };

    private static FarmFilter Filter(Action<BotConfig>? configure = null)
    {
        var config = Fixtures.Config();
        configure?.Invoke(config);
        config.Sanitize();
        return FarmFilter.FromConfig(config);
    }

    [Fact]
    public void ClassifiesClientActorTypes()
    {
        Assert.True(Mob(1, ActorType.Enemy, 101, 10, 0).IsMonster);
        Assert.True(Mob(2, ActorType.Stone, 8001, 10, 0).IsStone);
        Assert.True(Mob(3, ActorType.Player, 0, 10, 0).IsPlayer);
        Assert.True(Mob(4, ActorType.Npc, 9001, 10, 0).IsNpc);
        Assert.Equal(0, (byte)ActorType.Enemy);
        Assert.Equal(2, (byte)ActorType.Stone);
        Assert.Equal(6, (byte)ActorType.Player);
    }

    [Fact]
    public void ComputesThreeAxisDistanceFromMain()
    {
        var entity = EntityManager.WithDistance(Mob(1, ActorType.Enemy, 101, 3, 4, 12), Main);
        Assert.Equal(13, entity.Distance, 3);
    }

    [Fact]
    public void ListsNearbyMonstersAndIgnoresPlayersNpcsAndStones()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 40, 0, name: "Wild Dog"),
            Mob(2, ActorType.Stone, 8001, 50, 0, name: "Metin"),
            Mob(3, ActorType.Npc, 9001, 20, 0, name: "Shop"),
            Mob(4, ActorType.Player, 0, 15, 0, name: "Other"),
            Mob(5, ActorType.Enemy, 102, 80, 0, name: "Wolf")
        };

        var monsters = EntityManager.NearbyMonsters(entities, Main, Filter());

        Assert.Equal(new uint[] { 1, 5 }, monsters.Select(item => item.Vid).ToArray());
    }

    [Fact]
    public void ListsNearbyStonesOnly()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 40, 0),
            Mob(2, ActorType.Stone, 8001, 90, 0),
            Mob(7, ActorType.Stone, 8002, 40, 0)
        };

        var stones = EntityManager.NearbyStones(entities, Main, Filter());

        Assert.Equal(new uint[] { 7, 2 }, stones.Select(item => item.Vid).ToArray());
    }

    [Fact]
    public void FiltersSpecificVnumsAndExcludedVnums()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 20, 0),
            Mob(6, ActorType.Enemy, 103, 30, 0),
            Mob(2, ActorType.Stone, 8001, 40, 0)
        };
        var filter = Filter(config =>
        {
            config.IncludeVnums = new List<uint> { 101, 8001 };
            config.ExcludeVnums = new List<uint> { 8001 };
        });

        var kept = EntityManager.Filter(entities, Main, filter);

        Assert.Single(kept);
        Assert.Equal(101u, kept[0].Vnum);
    }

    [Fact]
    public void DropsTargetsBeyondMaxFarmDistance()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 40, 0),
            Mob(3, ActorType.Enemy, 102, 800, 0)
        };
        var filter = Filter(config => config.MaxFarmDistance = 100);

        var kept = EntityManager.Filter(entities, Main, filter);

        Assert.Single(kept);
        Assert.Equal(1u, kept[0].Vid);
    }

    [Fact]
    public void SelectsClosestStoneWhenStonePriorityIsHigher()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 20, 0, name: "Wild Dog"),
            Mob(2, ActorType.Stone, 8001, 250, 0, name: "Metin of Combat"),
            Mob(4, ActorType.Npc, 9001, 10, 0, name: "Shop"),
            Mob(5, ActorType.Player, 0, 12, 0, name: "Other")
        };

        var selected = EntityManager.SelectTarget(entities, Main, Filter());

        Assert.NotNull(selected);
        Assert.Equal(2u, selected!.Vid);
        Assert.Equal(ActorType.Stone, selected.Type);
    }

    [Fact]
    public void SelectsClosestMonsterWhenOnlyMonstersAreHunted()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 180, 0, name: "Wild Dog"),
            Mob(2, ActorType.Stone, 8001, 40, 0, name: "Metin"),
            Mob(6, ActorType.Enemy, 103, 80, 0, name: "Young Tiger")
        };
        var filter = Filter(config => config.HuntStones = false);

        var selected = EntityManager.SelectTarget(entities, Main, filter);

        Assert.Equal(6u, selected!.Vid);
    }

    [Fact]
    public void VnumPriorityBeatsGenericTypePriority()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 20, 0, name: "Wild Dog"),
            Mob(6, ActorType.Enemy, 103, 180, 0, name: "Young Tiger")
        };
        var filter = Filter(config =>
        {
            config.TargetRules.Add(new TargetRule
            {
                Kind = EntityKind.Monster,
                Vnum = 103,
                Priority = 50
            });
        });

        var selected = EntityManager.SelectTarget(entities, Main, filter);

        Assert.Equal(6u, selected!.Vid);
    }

    [Fact]
    public void DoesNotSelectDeadOrNonFarmableEntities()
    {
        var entities = new[]
        {
            Mob(1, ActorType.Enemy, 101, 20, 0, hp: 0),
            Mob(4, ActorType.Npc, 9001, 15, 0),
            Mob(5, ActorType.Player, 0, 18, 0)
        };

        Assert.Null(EntityManager.SelectTarget(entities, Main, Filter()));
    }
}
