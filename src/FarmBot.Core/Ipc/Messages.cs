using FarmBot.Core.Models;

namespace FarmBot.Core.Ipc;

public sealed class HelloMessage
{
    public string Type { get; set; } = "hello";
    public string Client { get; set; } = "";
    public bool SupportsCommands { get; set; }
    public int Protocol { get; set; } = 1;
}

public sealed class PlayerDto
{
    public string Name { get; set; } = "";
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public int Level { get; set; }
    public uint TargetVid { get; set; }
    public string MapName { get; set; } = "";
}

public sealed class EntityDto
{
    public uint Vid { get; set; }
    public string Name { get; set; } = "";
    public EntityKind Kind { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Distance { get; set; }
    public float ScreenX { get; set; }
    public float ScreenY { get; set; }
    public bool OnScreen { get; set; }
}

public sealed class StateMessage
{
    public string Type { get; set; } = "state";
    public PlayerDto? Player { get; set; }
    public List<EntityDto>? Entities { get; set; }
}

public sealed class CommandMessage
{
    public string Type { get; set; } = "command";
    public string Name { get; set; } = "";
    public uint Vid { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public int ScreenX { get; set; }
    public int ScreenY { get; set; }
    public bool OnScreen { get; set; }
    public int Slot { get; set; }
    public string Key { get; set; } = "";
    public string TargetName { get; set; } = "";

    public static CommandMessage From(BotCommand command)
    {
        return new CommandMessage
        {
            Name = command.Kind switch
            {
                BotCommandKind.Attack => "attack",
                BotCommandKind.Move => "move",
                BotCommandKind.Skill => "skill",
                BotCommandKind.Pickup => "pickup",
                BotCommandKind.Heal => "heal",
                BotCommandKind.Stop => "stop",
                _ => "stop"
            },
            Vid = command.Vid,
            X = command.X,
            Y = command.Y,
            ScreenX = command.ScreenX,
            ScreenY = command.ScreenY,
            OnScreen = command.OnScreen,
            Slot = command.SkillSlot,
            Key = command.Key,
            TargetName = command.TargetName
        };
    }
}
