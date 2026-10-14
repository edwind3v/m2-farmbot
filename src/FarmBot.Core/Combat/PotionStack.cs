namespace FarmBot.Core.Combat;

public enum PotionFamily
{
    Hp,
    Mp,
    Green,
    Purple
}

public sealed class PotionStack
{
    public uint Vnum { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
    public int Slot { get; set; }
    public PotionFamily Family { get; set; }
    public string Icon { get; set; } = "";
    public int DurationSec { get; set; }
    public string Tooltip { get; set; } = "";
}

public sealed class PotionScan
{
    public IReadOnlyList<PotionStack> Pots { get; set; } = Array.Empty<PotionStack>();
    public int GreenRemain { get; set; }
    public int PurpleRemain { get; set; }
}
