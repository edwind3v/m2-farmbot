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
    public const float ClickDistance = 300f;
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
}
