using FarmBot.Core.Combat;
using FarmBot.Core.Entities;

namespace FarmBot.Tests;

public class CombatNativeContractTests
{
    [Fact]
    public void DocumentsConfirmedClientCombatConstants()
    {
        Assert.Equal(150f, CombatNativeContract.MeleeClickableDistance);
        Assert.Equal(0.2f, CombatNativeContract.AttackDelaySeconds);
        Assert.Equal(2, CombatNativeContract.AttackPacketHeader);
        Assert.Equal(61, CombatNativeContract.TargetPacketHeader);
        Assert.True(CombatNativeContract.IsFarmableActor((int)ActorType.Enemy));
        Assert.True(CombatNativeContract.IsFarmableActor((int)ActorType.Stone));
        Assert.False(CombatNativeContract.IsFarmableActor((int)ActorType.Player));
        Assert.False(CombatNativeContract.IsFarmableActor((int)ActorType.Npc));
        Assert.Equal(250f, CombatNativeContract.AvoidStandOff);
        Assert.Equal(300f, CombatNativeContract.PickDistance);
        Assert.Equal(15, CombatNativeContract.PickupPacketHeader);
        Assert.Equal(-400f, CombatNativeContract.GroundItemPixelY(400f));
        Assert.Equal(1u, CombatNativeContract.GroundYangFlag);
        Assert.Equal(0x2FCu, CombatNativeContract.GroundOwnerNameDisplacement);
        Assert.True(CombatNativeContract.CanTakeGroundDrop("", "Hero"));
        Assert.True(CombatNativeContract.CanTakeGroundDrop(null, "Hero"));
        Assert.True(CombatNativeContract.CanTakeGroundDrop("Hero", "Hero"));
        Assert.False(CombatNativeContract.CanTakeGroundDrop("Other", "Hero"));
        Assert.False(CombatNativeContract.CanTakeGroundDrop("Other", ""));
    }

    [Fact]
    public void StandOffSitsTowardThePlayer()
    {
        CombatNativeContract.StandOff(1000, 0, 0, 0, out var x, out var y);
        Assert.Equal(750f, x, 2);
        Assert.Equal(0f, y, 2);

        CombatNativeContract.StandOff(0, 0, 0, 0, out var sameX, out var sameY);
        Assert.Equal(250f, sameX, 2);
        Assert.Equal(0f, sameY, 2);
    }
}
