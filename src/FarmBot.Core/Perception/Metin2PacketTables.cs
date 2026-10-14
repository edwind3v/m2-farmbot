namespace FarmBot.Core.Perception;

/// <summary>
/// Fixed server-to-client packet sizes taken from this client's phase switches.
/// A zero entry means that header is not used in the phase.
/// </summary>
internal static class Metin2PacketTables
{
    public const byte PhaseHeader = 0xFD;
    public const byte HandshakeHeader = 0xFF;
    public const byte CharacterAdd = 1;
    public const byte CharacterDelete = 2;
    public const byte CharacterMove = 3;
    public const byte CharacterDead = 14;
    public const byte MainCharacter = 15;
    public const byte Points = 16;
    public const byte PointChange = 17;
    public const byte CharacterAdd2 = 120;
    public const byte MainCharacterExtended = 113;
    public const byte CharacterAdditionalInfo = 136;

    public static int[] Handshake { get; } = Table(
        (44, 1), (152, 13), (153, 13), (PhaseHeader, 2), (HandshakeHeader, 13));

    public static int[] Login { get; } = Table(
        (6, 273), (7, 10), (32, 449), (44, 1), (90, 2), (118, 5),
        (152, 13), (153, 13), (PhaseHeader, 2));

    public static int[] Select { get; } = Table(
        (6, 273), (8, 73), (9, 2), (10, 2), (11, 1), (32, 449), (44, 1),
        (90, 2), (107, 30), (152, 13), (153, 13), (252, 1), (PhaseHeader, 2), (HandshakeHeader, 13));

    public static int[] Loading { get; } = Table(
        (MainCharacter, 47), (Points, 1021), (PointChange, 17), (19, 52), (20, 42),
        (28, 4), (44, 1), (MainCharacterExtended, 48), (137, 73), (138, 77),
        (152, 13), (153, 13), (PhaseHeader, 2));

    public static int[] Game { get; } = Table(
        (CharacterAdd, 37), (CharacterDelete, 5), (CharacterMove, 24), (4, 10), (5, 3),
        (13, 5), (CharacterDead, 5), (Points, 1021), (PointChange, 17), (18, 7), (19, 52),
        (20, 42), (21, 51), (22, 16), (25, 38), (26, 21), (27, 5), (28, 4), (29, 2), (30, 3),
        (31, 30), (34, 30), (36, 11), (38, 4), (39, 38), (40, 3), (41, 10), (42, 47), (43, 6),
        (44, 1), (45, 6), (46, 74), (61, 18), (62, 9), (63, 6), (65, 15), (69, 17), (70, 10),
        (71, 17), (72, 256), (74, 4), (75, 4), (76, 1531), (77, 5), (78, 30), (79, 21), (80, 5),
        (81, 1), (82, 1), (83, 2), (84, 5), (85, 51), (86, 2), (87, 1), (88, 2), (89, 7),
        (91, 9), (92, 9), (95, 59), (96, 9), (97, 9), (98, 9), (99, 1221), (106, 5), (110, 4),
        (111, 6), (112, 2), (114, 6), (115, 5), (119, 60), (CharacterAdd2, 98), (121, 2),
        (122, 2), (123, 13), (124, 5), (125, 43), (126, 22), (127, 6), (128, 51), (129, 2),
        (130, 3), (131, 27), (132, 2), (134, 10), (135, 10), (CharacterAdditionalInfo, 71),
        (152, 13), (153, 13), (161, 9), (208, 5), (209, 5), (215, 23), (216, 2), (252, 1),
        (PhaseHeader, 2), (HandshakeHeader, 13));

    public static int[] ForPhase(Metin2Phase phase) => phase switch
    {
        Metin2Phase.Login => Login,
        Metin2Phase.Select => Select,
        Metin2Phase.Loading => Loading,
        Metin2Phase.Game => Game,
        _ => Handshake
    };

    private static int[] Table(params (int Header, int Size)[] pairs)
    {
        var table = new int[256];
        foreach (var (header, size) in pairs)
            table[header] = size;
        return table;
    }
}

internal enum Metin2Phase : byte
{
    Close = 0,
    Handshake = 1,
    Login = 2,
    Select = 3,
    Loading = 4,
    Game = 5
}
