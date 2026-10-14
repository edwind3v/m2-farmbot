namespace FarmBot.Core.Combat;

public sealed record LiveTargetingReport(
    bool ClientRunning,
    int ProcessId,
    string ImagePath,
    uint ImageBase,
    uint SetTargetVa,
    uint SetTargetThisAdjustment,
    uint SetTargetVtableSlot,
    bool SetTargetBytesMatch,
    string FilePrologue,
    string LivePrologue,
    uint PlayerSingletonRva,
    uint PlayerSingletonPtr,
    uint TargetVidDisplacement,
    uint ObservedTargetVid,
    uint GameThreadId,
    int NearbyFarmable,
    string NearbyVids,
    bool InvokedSetTarget,
    bool NativeTargetingVerified,
    string RemainingRequirement,
    IReadOnlyList<string> Evidence)
{
    public string Summary =>
        $"pid={ProcessId} image=0x{ImageBase:X} setTargetVa=0x{SetTargetVa:X} this+{SetTargetThisAdjustment:X} slot=0x{SetTargetVtableSlot:X} bytesMatch={SetTargetBytesMatch} " +
        $"singleton=0x{PlayerSingletonPtr:X} observedTarget={ObservedTargetVid} gameTid={GameThreadId} " +
        $"farmable={NearbyFarmable} invoked={InvokedSetTarget} verified={NativeTargetingVerified}";
}
