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
    }
}
