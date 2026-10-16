using System.Text.Json;
using FarmBot.Core.Combat;

namespace FarmBot.Core.Configuration;

public static class LiveChannelList
{
    public static IReadOnlyList<GameChannel> FromResponse(ActionResponse response)
    {
        if (response.Channels.ValueKind != JsonValueKind.Array)
            return Array.Empty<GameChannel>();
        var list = new List<GameChannel>();
        foreach (var item in response.Channels.EnumerateArray())
        {
            var host = item.TryGetProperty("host", out var h) ? h.GetString() ?? "" : "";
            var authHost = item.TryGetProperty("authHost", out var ah) ? ah.GetString() ?? "" : host;
            var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            var index = item.TryGetProperty("index", out var ix) && ix.TryGetInt32(out var i) ? i : list.Count;
            var port = item.TryGetProperty("port", out var p) && p.TryGetInt32(out var portVal) ? portVal : 0;
            var authPort = item.TryGetProperty("authPort", out var ap) && ap.TryGetInt32(out var apVal) ? apVal : 0;
            if (string.IsNullOrWhiteSpace(host) || port is < 1 or > 65535)
                continue;
            if (IsLoopback(host))
                continue;
            if (string.IsNullOrWhiteSpace(authHost))
                authHost = host;
            // Keep authPort=0 when missing — Baerim must use 30001, not the channel TCP port.
            if (authPort is < 0 or > 65535)
                authPort = 0;
            if (string.IsNullOrWhiteSpace(name))
                name = "CH-" + (index + 1);
            list.Add(new GameChannel(index, name, host, port, authHost, authPort));
        }

        return list
            .GroupBy(c => c.Index)
            .Select(g => g.First())
            .OrderBy(c => c.Index)
            .ToList();
    }

    public static IReadOnlyList<string> CharacterNamesFromResponse(ActionResponse response)
    {
        if (response.Characters.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();
        var names = new string[5];
        var i = 0;
        foreach (var item in response.Characters.EnumerateArray())
        {
            if (i >= 5)
                break;
            names[i++] = item.GetString()?.Trim() ?? "";
        }

        return names;
    }

    public static bool IsLoopback(string? host) =>
        string.IsNullOrWhiteSpace(host) ||
        string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
}
