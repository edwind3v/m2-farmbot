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

    /// <summary>Toggle highlight / aSkill[slot].bActive.</summary>
    bool TryIsSkillActive(int slot, out bool active)
    {
        active = false;
        return false;
    }

    /// <summary>True while the slot is on skill cooltime.</summary>
    bool TryIsSkillCool(int slot, out bool cooling)
    {
        cooling = false;
        return false;
    }

    /// <param name="wantMounted">false = dismount, true = remount.</param>
    bool TryToggleMount(bool wantMounted = false) => false;

    /// <summary>Show/hide the in-game farm radius ground circle.</summary>
    bool TrySetFarmCircle(bool enabled, bool locked, float radius, float centerX, float centerY, uint color = 0xFF0000) => false;

    bool TryReadEntities(out IReadOnlyList<EntityState> entities)
    {
        entities = Array.Empty<EntityState>();
        return false;
    }
}
