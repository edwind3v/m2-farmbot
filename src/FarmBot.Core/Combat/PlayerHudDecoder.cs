namespace FarmBot.Core.Combat;

public readonly record struct PlayerHudPoints(int Level, int Exp, int NextExp, int Hp, int MaxHp, int Mp, int MaxMp);

public static class PlayerHudDecoder
{
    public const uint DefaultXor = 0xE73AC1DA;
    public static readonly int XorOfZero = unchecked((int)DefaultXor);

    public static bool LooksLikeHud(int level, int hp, int maxHp, int mp, int maxMp, int exp, int nextExp)
    {
        if (IsUnset(level) || IsUnset(hp) || IsUnset(maxHp) || IsUnset(mp) || IsUnset(maxMp) || IsUnset(exp) || IsUnset(nextExp))
            return false;
        if (level is < 1 or > 250)
            return false;
        if (hp < 0 || maxHp < 50 || hp > maxHp)
            return false;
        if (mp < 0 || maxMp < 0 || (maxMp > 0 && mp > maxMp))
            return false;
        if (exp < 0 || nextExp < 0)
            return false;
        if (nextExp > 0 && exp > nextExp)
            return false;
        if (maxMp > 0 && maxHp <= maxMp)
            return false;
        // A window two POINT slots late maps EXP→level, HP→exp, SP→hp.
        // Real next-exp at mid/high level dwarfs HP; the shifted window does not.
        if (level >= 20 && nextExp > 0 && nextExp < maxHp * 6L)
            return false;
        return true;
    }

    public static bool IsUnset(int decoded) =>
        decoded == XorOfZero || decoded == unchecked((int)~DefaultXor);
}
