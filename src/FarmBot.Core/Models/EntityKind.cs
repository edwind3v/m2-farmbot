using System.Text.Json;
using System.Text.Json.Serialization;

namespace FarmBot.Core.Models;

[JsonConverter(typeof(FlexibleEntityKindConverter))]
public enum EntityKind
{
    Unknown = 0,
    Monster = 1,
    Metin = 2,
    Boss = 3,
    Npc = 4,
    Player = 5,
    Drop = 6
}

public sealed class FlexibleEntityKindConverter : JsonConverter<EntityKind>
{
    public override EntityKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
            return Enum.IsDefined(typeof(EntityKind), number) ? (EntityKind)number : EntityKind.Unknown;

        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (!string.IsNullOrWhiteSpace(text) && Enum.TryParse<EntityKind>(text, true, out var kind))
                return kind;
        }

        return EntityKind.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, EntityKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}
