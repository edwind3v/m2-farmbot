namespace FarmBot.Core.Combat;

/// <summary>
/// Login / channel / character-select natives recovered from the release image Python net wrappers
/// and ClientSRC (CPythonNetworkStream / CAccountConnector).
/// </summary>
public static class LoginNativeContract
{
    public const uint NetStreamRva = 0x482BA0;
    public const uint AccountConnectorRva = 0x484D24;
    public const uint SetLoginInfoRva = 0x11C390;
    public const uint AccountSetLoginInfoRva = 0x117660;
    public const uint AccountConnectRva = 0x117510;
    public const uint ConnectGameServerRva = 0x11BD40;
    public const uint SelectCharacterRva = 0x13DC60;
    public const uint SetLoginPhaseRva = 0x13D240;
    public const uint SetGamePhaseRva = 0x1367F0;
    public const uint StartGameRva = 0x13CD40;
    public const uint NotifyGamePhaseRva = 0x11C0C0;
    public const uint SendEnterGamePacketRva = 0x13CC00;
    public const uint DirectEnterModeSetRva = 0x11C800;
    public const uint OnLogoCloseRva = 0x13F1A0;
    public const uint AppSingletonRva = 0x484C8C;
    public const uint PhaseDisplacement = 0xF8;
    public const uint LoginIdDisplacement = 0xB0;
    public const uint LoginPasswordDisplacement = 0xC8;
    public const uint SelectedCharacterDisplacement = 0x797C;
    public const uint DirectEnterIsSetDisplacement = 0x79A4;
    public const uint DirectEnterSlotDisplacement = 0x79A8;
    public const uint StartGameFlagDisplacement = 0x8C;
    public const uint PythonHandlerDisplacement = 0x160;
    public const uint GameWindowDisplacement = 0x8;
    public const uint SetHandlerRva = 0x11C380;
    public const uint SetGameWindowRva = 0xCEA70;
}
