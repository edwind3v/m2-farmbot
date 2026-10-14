namespace FarmBot.Platform.Input;

public static class VirtualKeys
{
    public static ushort FromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return 0;

        name = name.Trim();
        if (name.Equals("space", StringComparison.OrdinalIgnoreCase))
            return 0x20;
        if (name.Equals("esc", StringComparison.OrdinalIgnoreCase) || name.Equals("escape", StringComparison.OrdinalIgnoreCase))
            return 0x1B;
        if (name.Equals("tab", StringComparison.OrdinalIgnoreCase))
            return 0x09;
        if (name.Equals("enter", StringComparison.OrdinalIgnoreCase) || name.Equals("return", StringComparison.OrdinalIgnoreCase))
            return 0x0D;

        if ((name[0] == 'F' || name[0] == 'f') &&
            int.TryParse(name.AsSpan(1), out var function) &&
            function is >= 1 and <= 24)
            return (ushort)(0x70 + function - 1);

        if (name.Length == 1)
        {
            var character = char.ToUpperInvariant(name[0]);
            if (character is >= 'A' and <= 'Z' or >= '0' and <= '9')
                return character;
        }

        return 0;
    }
}
