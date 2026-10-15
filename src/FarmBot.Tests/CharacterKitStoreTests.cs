using FarmBot.Core.Bot;
using FarmBot.Core.Combat;
using FarmBot.Core.Configuration;
using FarmBot.Core.Input;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Core.Perception;

namespace FarmBot.Tests;

public class CharacterKitStoreTests
{
    [Fact]
    public void SaveAndLoadRoundTripIconsAndInfo()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-kit-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = CharacterKitStore.FilePath(dir, "acc/one", "Hero:Two");
            CharacterKitStore.Save(path, new CharacterKit
            {
                Account = "acc/one",
                Character = "Hero:Two",
                Pots =
                {
                    new PotionStack
                    {
                        Vnum = 27001,
                        Name = "Red Potion",
                        Count = 40,
                        Slot = 7,
                        Family = PotionFamily.Hp,
                        Icon = "data:image/png;base64,aaa",
                        DurationSec = 0,
                        Tooltip = "HP"
                    }
                },
                Skills =
                {
                    new LearnedSkill
                    {
                        Index = 106,
                        Slot = 1,
                        Name = "Lightning Throw",
                        Level = 17,
                        CoolRemain = 9,
                        AffectRemain = 3,
                        DurationSec = 12,
                        Icon = "data:image/png;base64,bbb"
                    }
                }
            });

            Assert.True(CharacterKitStore.TryLoad(path, out var kit));
            var pot = Assert.Single(kit.Pots);
            Assert.Equal(27001u, pot.Vnum);
            Assert.Equal("Red Potion", pot.Name);
            Assert.Equal(40, pot.Count);
            Assert.Equal(7, pot.Slot);
            Assert.Equal("data:image/png;base64,aaa", pot.Icon);
            var skill = Assert.Single(kit.Skills);
            Assert.Equal(106u, skill.Index);
            Assert.Equal("Lightning Throw", skill.Name);
            Assert.Equal(0, skill.CoolRemain);
            Assert.Equal(0, skill.AffectRemain);
            Assert.Equal("data:image/png;base64,bbb", skill.Icon);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void MergeKeepsLiveSlotAndCountAndFillsCachedIcon()
    {
        var cached = new[]
        {
            new PotionStack
            {
                Vnum = 27001,
                Name = "Red Potion",
                Count = 99,
                Slot = 1,
                Family = PotionFamily.Hp,
                Icon = "icon-red",
                Tooltip = "HP"
            }
        };
        var live = new[]
        {
            new PotionStack
            {
                Vnum = 27001,
                Name = "",
                Count = 12,
                Slot = 18,
                Family = PotionFamily.Hp,
                Icon = ""
            }
        };
        var merged = CharacterKitStore.MergePots(live, cached);
        var pot = Assert.Single(merged);
        Assert.Equal(12, pot.Count);
        Assert.Equal(18, pot.Slot);
        Assert.Equal("Red Potion", pot.Name);
        Assert.Equal("icon-red", pot.Icon);
    }

    [Fact]
    public void MergeEmptyLiveInventoryDropsCachedStacks()
    {
        var cached = new[]
        {
            new PotionStack { Vnum = 27001, Name = "Red", Count = 5, Slot = 2, Family = PotionFamily.Hp, Icon = "x" }
        };
        Assert.Empty(CharacterKitStore.MergePots(Array.Empty<PotionStack>(), cached));
    }

    [Fact]
    public void MergeSkillsFillsIconFromCache()
    {
        var cached = new[]
        {
            new LearnedSkill { Index = 3, Slot = 1, Name = "Aura", Level = 10, Icon = "icon-aura", DurationSec = 30 }
        };
        var live = new[]
        {
            new LearnedSkill { Index = 3, Slot = 4, Name = "", Level = 12, CoolRemain = 2, Icon = "" }
        };
        var merged = Assert.Single(CharacterKitStore.MergeSkills(live, cached));
        Assert.Equal(4, merged.Slot);
        Assert.Equal(12, merged.Level);
        Assert.Equal(2, merged.CoolRemain);
        Assert.Equal("Aura", merged.Name);
        Assert.Equal("icon-aura", merged.Icon);
        Assert.Equal(30, merged.DurationSec);
    }

    [Fact]
    public void RuntimeShowsCachedKitBeforeGamePhase()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-kit-" + Guid.NewGuid().ToString("N"));
        try
        {
            CharacterKitStore.Save(CharacterKitStore.FilePath(dir, "acc", "Hero"), new CharacterKit
            {
                Account = "acc",
                Character = "Hero",
                Pots = { new PotionStack { Vnum = 27001, Name = "Red", Count = 8, Slot = 3, Family = PotionFamily.Hp, Icon = "pot" } },
                Skills = { new LearnedSkill { Index = 106, Slot = 1, Name = "Throw", Level = 17, Icon = "sk" } }
            });
            var source = new SnapshotSource { Current = Fixtures.World() with { Player = Fixtures.Player(maxHp: 0) } };
            var runtime = new BotRuntime(
                source,
                new NoopControl(),
                new ListLog(),
                Fixtures.Config,
                nativeCombat: true,
                sink: new KitSink(),
                cacheRoot: dir,
                account: "acc",
                character: "Hero");
            var view = runtime.Capture();
            var pot = Assert.Single(view.Potions ?? Array.Empty<PotionStack>());
            Assert.Equal(27001u, pot.Vnum);
            Assert.Equal("pot", pot.Icon);
            var skill = Assert.Single(view.Skills ?? Array.Empty<LearnedSkill>());
            Assert.Equal(106u, skill.Index);
            Assert.Equal("sk", skill.Icon);
            runtime.Dispose();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void RuntimeDoesNotScanOrUseCachedPotsUntilInWorld()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-kit-" + Guid.NewGuid().ToString("N"));
        try
        {
            CharacterKitStore.Save(CharacterKitStore.FilePath(dir, "acc", "Hero"), new CharacterKit
            {
                Account = "acc",
                Character = "Hero",
                Pots =
                {
                    new PotionStack { Vnum = 27100, Name = "Green", Count = 10, Slot = 4, Family = PotionFamily.Green, Icon = "g" }
                },
                Skills = { new LearnedSkill { Index = 106, Slot = 1, Name = "Throw", Level = 17, Icon = "sk" } }
            });
            var source = new SnapshotSource { Current = Fixtures.World() };
            var sink = new KitSink { InWorld = false };
            var config = Fixtures.Config();
            config.PotionEnabledVnums = new List<uint>();
            config.SkillEnabledIndexes = new List<uint> { 106 };
            config.SkillBuffIndexes = new List<uint> { 106 };
            var log = new ListLog();
            var runtime = new BotRuntime(
                source,
                new NoopControl(),
                log,
                () => config,
                nativeCombat: true,
                sink: sink,
                cacheRoot: dir,
                account: "acc",
                character: "Hero");
            runtime.SetEnabled(true);
            runtime.TickOnce(config, 1_000, 100);
            Assert.Contains(log.Entries, entry => entry.Message.Contains("game phase=", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(0, sink.PotReads);
            Assert.Equal(0, sink.SkillReads);
            Assert.Empty(sink.UsedItems);
            Assert.Empty(sink.UsedSkills);
            Assert.Equal(4, Assert.Single(runtime.Capture().Potions ?? Array.Empty<PotionStack>()).Slot);
            runtime.Dispose();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void RuntimeUsesLivePotsAndBuffSkillsWhenInWorld()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-kit-" + Guid.NewGuid().ToString("N"));
        try
        {
            var source = new SnapshotSource { Current = Fixtures.World() };
            var sink = new KitSink
            {
                InWorld = true,
                Pots = new PotionScan
                {
                    Pots = new[]
                    {
                        new PotionStack { Vnum = 27001, Name = "Red", Count = 8, Slot = 3, Family = PotionFamily.Hp, Icon = "pot" }
                    }
                },
                Skills = new SkillScan
                {
                    Skills = new[]
                    {
                        new LearnedSkill { Index = 106, Slot = 1, Name = "Throw", Level = 17, Icon = "sk" }
                    }
                }
            };
            var config = Fixtures.Config();
            config.PotionHpPercent = 75;
            config.PotionEnabledVnums = new List<uint> { 27001 };
            config.SkillEnabledIndexes = new List<uint> { 106 };
            config.SkillBuffIndexes = new List<uint> { 106 };
            var runtime = new BotRuntime(
                source,
                new NoopControl(),
                new ListLog(),
                () => config,
                nativeCombat: true,
                sink: sink,
                cacheRoot: dir,
                account: "acc",
                character: "Hero");
            runtime.SetEnabled(true);
            runtime.TickOnce(config, 1_000, 100);
            Assert.Contains(3, sink.UsedItems);
            Assert.Contains(1, sink.UsedSkills);
            runtime.Dispose();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void RuntimeKeepsCachedKitWhenLeavingGameAndTracksLiveSlots()
    {
        var dir = Path.Combine(Path.GetTempPath(), "farmbot-kit-" + Guid.NewGuid().ToString("N"));
        try
        {
            CharacterKitStore.Save(CharacterKitStore.FilePath(dir, "acc", "Hero"), new CharacterKit
            {
                Account = "acc",
                Character = "Hero",
                Pots = { new PotionStack { Vnum = 27001, Name = "Red", Count = 8, Slot = 3, Family = PotionFamily.Hp, Icon = "pot" } }
            });
            var source = new SnapshotSource { Current = Fixtures.World() };
            var sink = new KitSink
            {
                InWorld = true,
                Pots = new PotionScan
                {
                    Pots = new[]
                    {
                        new PotionStack { Vnum = 27001, Name = "", Count = 5, Slot = 11, Family = PotionFamily.Hp, Icon = "" }
                    }
                }
            };
            var config = Fixtures.Config();
            var runtime = new BotRuntime(
                source,
                new NoopControl(),
                new ListLog(),
                () => config,
                nativeCombat: true,
                sink: sink,
                cacheRoot: dir,
                account: "acc",
                character: "Hero");
            runtime.TickOnce(config, 1_000, 100);
            var live = Assert.Single(runtime.Capture().Potions ?? Array.Empty<PotionStack>());
            Assert.Equal(11, live.Slot);
            Assert.Equal(5, live.Count);
            Assert.Equal("pot", live.Icon);

            sink.Pots = new PotionScan
            {
                Pots = new[]
                {
                    new PotionStack { Vnum = 27001, Name = "", Count = 4, Slot = 2, Family = PotionFamily.Hp, Icon = "" }
                }
            };
            runtime.TickOnce(config, 1_000 + CharacterKitStore.PotionPollMs, 100);
            var moved = Assert.Single(runtime.Capture().Potions ?? Array.Empty<PotionStack>());
            Assert.Equal(2, moved.Slot);
            Assert.Equal(4, moved.Count);

            source.Current = Fixtures.World() with { Player = Fixtures.Player(maxHp: 0) };
            sink.InWorld = false;
            runtime.TickOnce(config, 1_000 + CharacterKitStore.PotionPollMs * 2, 100);
            var kept = Assert.Single(runtime.Capture().Potions ?? Array.Empty<PotionStack>());
            Assert.Equal(2, kept.Slot);
            Assert.Equal(4, kept.Count);
            Assert.Equal("pot", kept.Icon);

            source.Current = Fixtures.World();
            sink.InWorld = true;
            sink.Pots = new PotionScan
            {
                Pots = new[]
                {
                    new PotionStack { Vnum = 27001, Name = "", Count = 4, Slot = 2, Family = PotionFamily.Hp, Icon = "" }
                }
            };
            runtime.TickOnce(config, 1_000 + CharacterKitStore.PotionPollMs * 3, 100);
            sink.Pots = new PotionScan();
            runtime.TickOnce(config, 1_000 + CharacterKitStore.PotionPollMs * 4, 100);
            Assert.Empty(runtime.Capture().Potions ?? Array.Empty<PotionStack>());
            runtime.Dispose();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    private sealed class SnapshotSource : IGameStateSource
    {
        public WorldSnapshot? Current { get; set; }
        public WorldSnapshot? GetSnapshot(long nowMs) => Current;
    }

    private sealed class NoopControl : IGameControl
    {
        public void Execute(BotCommand command)
        {
        }
    }

    private sealed class KitSink : INativeActionSink
    {
        public PotionScan Pots { get; set; } = new();
        public SkillScan Skills { get; set; } = new();
        public bool InWorld { get; set; }
        public int PotReads { get; private set; }
        public int SkillReads { get; private set; }
        public List<int> UsedItems { get; } = new();
        public List<int> UsedSkills { get; } = new();
        public bool Available => true;
        public string Status => "kit";
        public bool TrySubmit(NativeCombatAction action) => false;
        public bool TryReadHud(out PlayerHudPoints hud)
        {
            hud = InWorld
                ? new PlayerHudPoints(35, 0, 1, 400, 1000, 40, 200, true)
                : new PlayerHudPoints(0, 0, 0, 0, 0, 0, 0, false);
            return true;
        }

        public bool TryReadPots(out PotionScan scan)
        {
            PotReads++;
            scan = Pots;
            return true;
        }

        public bool TryReadSkills(out SkillScan scan)
        {
            SkillReads++;
            scan = Skills;
            return true;
        }

        public bool TryUseItem(int slot)
        {
            UsedItems.Add(slot);
            return true;
        }

        public bool TryUseSkill(int slot, uint targetVid)
        {
            UsedSkills.Add(slot);
            return true;
        }
    }
}
