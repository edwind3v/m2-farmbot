namespace FarmBot.Core.Combat;

/// <summary>
/// Client combat facts recovered from ClientSRC (PythonPlayer / InstanceBase / network stream).
/// These are gameplay methods, not visual-only effects. Addresses are not guessed.
/// </summary>
public static class CombatNativeContract
{
    public const float MeleeClickableDistance = 150f;
    public const float GroundClickableDistance = 150f;
    public const float ComboDistance = 250f;
    public const float AvoidStandOff = 250f;
    public const float ClickDistance = 300f;
    public const float PickDistance = 300f;
    public const int PickupPacketHeader = 15;

    // CPythonItem::GetCloseItem negates SGroundItemInstance Y before comparing it to GetPixelPosition.
    public static float GroundItemPixelY(float storedY) => -storedY;

    // GetCloseMoney only accepts ground instances whose dword at +4 is 1 (yang).
    // SendClickItemPacket looks up proto/owner and skips yang and empty-owner drops;
    // ranged/fast pickup therefore sends SendItemPickUpPacket after the spoof.
    public const uint GroundYangFlag = 1;
    public const float NpcClickableDistance = 500f;
    public const float AttackDelaySeconds = 0.2f;
    public const int TargetPacketHeader = 61;
    public const int AttackPacketHeader = 2;

    public const string SetTargetExport = "player.SetTarget";
    public const string GetTargetVidExport = "player.GetTargetVID";
    public const string AttackKeyExport = "player.SetAttackKeyState";
    public const string ComboAttackExport = "player.ComboAttack";
    public const string CanAttackInstanceExport = "player.CanAttackInstance";
    public const string GameThreadNote =
        "SetTarget runs only on the window thread that already calls CPythonApplication::Process; do not thiscall it from an adapter worker";

    public static bool IsFarmableActor(int actorType) => actorType is 0 or 2;

    public static void StandOff(float mobX, float mobY, float playerX, float playerY, out float x, out float y)
    {
        var dx = playerX - mobX;
        var dy = playerY - mobY;
        var len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1f)
        {
            dx = 1f;
            dy = 0f;
            len = 1f;
        }

        x = mobX + dx / len * AvoidStandOff;
        y = mobY + dy / len * AvoidStandOff;
    }
}
