using FarmBot.Core.Configuration;
using FarmBot.Core.Models;

namespace FarmBot.Core.Combat;

public sealed class PotionController
{
    public const int CooldownMs = 400;
    public const int DefaultSpeedSec = 90;

    private long _lastUseMs;
    private long _greenUntilMs;
    private long _purpleUntilMs;

    public int? NextSlot(
        IReadOnlyList<PotionStack> pots,
        PlayerState? player,
        BotConfig config,
        int greenRemain,
        int purpleRemain,
        long nowMs)
    {
        if (config.ClientProfile != ClientProfile.Arthion || pots == null || pots.Count == 0)
            return null;
        if (nowMs - _lastUseMs < CooldownMs)
            return null;

        var enabled = config.PotionEnabledVnums ?? new List<uint>();

        var hp = Pick(pots, enabled, PotionFamily.Hp);
        if (hp != null && player != null && player.MaxHp > 0 && player.Hp < player.MaxHp &&
            100.0 * player.Hp / player.MaxHp <= config.PotionHpPercent)
            return Use(hp.Slot, nowMs);

        var mp = Pick(pots, enabled, PotionFamily.Mp);
        if (mp != null && player != null && player.MaxMp > 0 && player.Mp < player.MaxMp &&
            100.0 * player.Mp / player.MaxMp <= config.PotionMpPercent)
            return Use(mp.Slot, nowMs);

        var green = Pick(pots, enabled, PotionFamily.Green);
        if (green != null && SpeedExpired(greenRemain, _greenUntilMs, nowMs))
        {
            _greenUntilMs = nowMs + SpeedMs(green);
            return Use(green.Slot, nowMs);
        }

        var purple = Pick(pots, enabled, PotionFamily.Purple);
        if (purple != null && SpeedExpired(purpleRemain, _purpleUntilMs, nowMs))
        {
            _purpleUntilMs = nowMs + SpeedMs(purple);
            return Use(purple.Slot, nowMs);
        }

        return null;
    }

    public void NoteNativeRemain(int greenRemain, int purpleRemain, long nowMs)
    {
        if (greenRemain > 0)
            _greenUntilMs = nowMs + greenRemain * 1000L;
        if (purpleRemain > 0)
            _purpleUntilMs = nowMs + purpleRemain * 1000L;
    }

    public int GreenRemainSec(long nowMs) => RemainSec(_greenUntilMs, nowMs);

    public int PurpleRemainSec(long nowMs) => RemainSec(_purpleUntilMs, nowMs);

    private int? Use(int slot, long nowMs)
    {
        _lastUseMs = nowMs;
        return slot;
    }

    private static PotionStack? Pick(IReadOnlyList<PotionStack> pots, List<uint> enabled, PotionFamily family)
    {
        foreach (var pot in pots)
        {
            if (pot.Family != family || pot.Count <= 0)
                continue;
            if (enabled.Count == 0 || enabled.Contains(pot.Vnum))
                return pot;
        }

        return null;
    }

    private static bool SpeedExpired(int nativeRemain, long untilMs, long nowMs) =>
        nativeRemain <= 0 && nowMs >= untilMs;

    private static long SpeedMs(PotionStack pot)
    {
        var sec = pot.DurationSec is >= 15 and <= 600 ? pot.DurationSec : DefaultSpeedSec;
        return sec * 1000L;
    }

    private static int RemainSec(long untilMs, long nowMs) =>
        untilMs <= nowMs ? 0 : (int)Math.Ceiling((untilMs - nowMs) / 1000.0);
}
