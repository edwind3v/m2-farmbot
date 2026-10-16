using FarmBot.Core.Models;

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

    bool TryReadSkills(out SkillScan scan)
    {
        scan = new SkillScan();
        return false;
    }

    bool TryUseSkill(int slot, uint targetVid) => false;

    /// <param name="wantMounted">false = dismount, true = remount.</param>
    bool TryToggleMount(bool wantMounted = false) => false;

    bool TryReadEntities(out IReadOnlyList<EntityState> entities)
    {
        entities = Array.Empty<EntityState>();
        return false;
    }
}
