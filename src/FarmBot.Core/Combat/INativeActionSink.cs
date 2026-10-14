namespace FarmBot.Core.Combat;

public interface INativeActionSink
{
    bool Available { get; }
    string Status { get; }
    bool TrySubmit(NativeCombatAction action);
    bool TryReadHud(out PlayerHudPoints hud)
    {
        hud = default;
        return false;
    }

    bool TryReadPots(out PotionScan scan)
    {
        scan = new PotionScan();
        return false;
    }

    bool TryUseItem(int slot) => false;
}
