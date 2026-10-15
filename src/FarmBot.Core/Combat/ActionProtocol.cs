using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmBot.Core.Json;

namespace FarmBot.Core.Combat;

public sealed class ActionRequest
{
    public string Type { get; set; } = "";
    public string Token { get; set; } = "";
    public uint Vid { get; set; }
    public bool Force { get; set; } = true;
    public float X { get; set; }
    public float Y { get; set; }
    public bool Avoid { get; set; }
    public string Hits { get; set; } = "";
    public string Account { get; set; } = "";
    public string Password { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string AuthHost { get; set; } = "";
    public int AuthPort { get; set; }
    public int Slot { get; set; }
}

public sealed class ActionResponse
{
    public bool Ok { get; set; }
    public string Type { get; set; } = "";
    public uint TargetVid { get; set; }
    public string Reason { get; set; } = "";
    public string Convention { get; set; } = "";
    public int Level { get; set; }
    public int Exp { get; set; }
    public int NextExp { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public int AtlasWidth { get; set; }
    public int AtlasHeight { get; set; }
    public float WorldW { get; set; }
    public float WorldH { get; set; }
    public string Phase { get; set; } = "";
    public string Character { get; set; } = "";
    public string Account { get; set; } = "";
    public string Password { get; set; } = "";
    public int CharacterSlot { get; set; } = -1;
    public bool StartGame { get; set; }
    public bool InWorld { get; set; }
    public System.Text.Json.JsonElement Pots { get; set; }
    public System.Text.Json.JsonElement Skills { get; set; }
    public System.Text.Json.JsonElement Entities { get; set; }
    public int GreenRemain { get; set; }
    public int PurpleRemain { get; set; }
}

public static class ActionProtocol
{
    public const string DefaultPipeName = "FarmBotAction";
    public const string Auth = "auth";
    public const string SetTarget = "set_target";
    public const string PressActor = "press_actor";
    public const string Attack = "attack";
    public const string Shot = "shot";
    public const string AttackKey = "attack_key";
    public const string Face = "face";
    public const string Stop = "stop";
    public const string GetHud = "get_hud";
    public const string Pickup = "pickup";
    public const string LootLoop = "loot_loop";
    public const string Move = "move";
    public const string GetAtlas = "get_atlas";
    public const string GetPhase = "get_phase";
    public const string GetSession = "get_session";
    public const string SkipIntro = "skip_intro";
    public const string Login = "login";
    public const string SelectCharacter = "select_character";
    public const string EnterGame = "enter_game";
    public const string GetPots = "get_pots";
    public const string UseItem = "use_item";
    public const string GetSkills = "get_skills";
    public const string GetEntities = "get_entities";
    public const string UseSkill = "use_skill";
    public const string DumpCaptchaBg = "dump_captcha_bg";
    public const int Version = 1;

    public static byte[] Encode(object value) => FrameEncode(JsonSerializer.SerializeToUtf8Bytes(value, JsonDefaults.Instance));

    public static ActionRequest DecodeRequest(byte[] payload) =>
        JsonSerializer.Deserialize<ActionRequest>(payload, JsonDefaults.Instance) ?? new ActionRequest();

    public static ActionResponse DecodeResponse(byte[] payload) =>
        JsonSerializer.Deserialize<ActionResponse>(payload, JsonDefaults.Instance) ?? new ActionResponse();

    public static bool TokensEqual(string left, string right)
    {
        var a = Encoding.UTF8.GetBytes(left ?? "");
        var b = Encoding.UTF8.GetBytes(right ?? "");
        if (a.Length != b.Length)
            return false;
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    public static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    public static byte[] FrameEncode(byte[] payload)
    {
        var packet = new byte[4 + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(packet, payload.Length);
        payload.CopyTo(packet, 4);
        return packet;
    }

    public static bool TryReadFrame(Stream stream, out byte[] payload)
    {
        payload = Array.Empty<byte>();
        var header = new byte[4];
        if (!ReadExact(stream, header))
            return false;
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length <= 0 || length > 512 * 1024)
            return false;
        payload = new byte[length];
        return ReadExact(stream, payload);
    }

    private static bool ReadExact(Stream stream, byte[] buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = stream.Read(buffer, read, buffer.Length - read);
            if (n <= 0)
                return false;
            read += n;
        }

        return true;
    }
}
