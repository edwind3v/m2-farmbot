namespace FarmBot.Core.Combat;

public static class LoaderOutput
{
    public const string LaunchedPrefix = "launched pid=";

    public static string UniquePipeName(string? slotKey = null)
    {
        var key = Sanitize(slotKey);
        if (key.Length == 0)
            key = Guid.NewGuid().ToString("N")[..8];
        return ActionProtocol.DefaultPipeName + "_" + key;
    }

    public static bool TryParseLaunchedPid(string? output, out int processId)
    {
        processId = 0;
        if (string.IsNullOrWhiteSpace(output))
            return false;

        var at = output.IndexOf(LaunchedPrefix, StringComparison.OrdinalIgnoreCase);
        if (at < 0)
            return false;

        var start = at + LaunchedPrefix.Length;
        var end = start;
        while (end < output.Length && char.IsDigit(output[end]))
            end++;
        return end > start && int.TryParse(output[start..end], out processId) && processId > 0;
    }

    private static string Sanitize(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
            return "";
        var chars = slotKey.Where(char.IsLetterOrDigit).Take(12).ToArray();
        return new string(chars);
    }
}
