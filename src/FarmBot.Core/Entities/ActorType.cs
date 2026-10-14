namespace FarmBot.Core.Entities;

/// <summary>
/// CActorInstance::EType values confirmed in the compiled client (Python exports + sequential enum).
/// INSTANCE_TYPE_STONE is not a Python name; TYPE_STONE is still 2.
/// </summary>
public enum ActorType : byte
{
    Enemy = 0,
    Npc = 1,
    Stone = 2,
    Warp = 3,
    Door = 4,
    Building = 5,
    Player = 6,
    Poly = 7,
    Horse = 8,
    Goto = 9,
    Object = 10
}
