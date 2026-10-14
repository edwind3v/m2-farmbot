using System.Text.RegularExpressions;

namespace FarmBot.Core.Configuration;

public sealed record GameChannel(int Index, string Name, string Host, int Port, string AuthHost, int AuthPort);

public static class ServerInfoParser
{
    public static IReadOnlyList<GameChannel> FromClientPath(string clientPath)
    {
        foreach (var path in CandidateFiles(clientPath))
        {
            if (!File.Exists(path))
                continue;
            var channels = Parse(File.ReadAllText(path));
            if (channels.Count > 0)
                return channels;
        }

        return Array.Empty<GameChannel>();
    }

    public static IReadOnlyList<string> CandidateFiles(string clientPath)
    {
        var dir = string.IsNullOrWhiteSpace(clientPath)
            ? ""
            : Path.GetDirectoryName(clientPath) ?? "";
        if (dir.Length == 0)
            return Array.Empty<string>();
        return new[]
        {
            Path.Combine(dir, "pack", "root", "serverinfo.py"),
            Path.Combine(dir, "root", "serverinfo.py"),
            Path.Combine(dir, "serverinfo.py")
        };
    }

    public static IReadOnlyList<GameChannel> Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<GameChannel>();

        var host = MatchOne(text, @"[""']host[""']\s*:\s*[""']([^""']+)[""']");
        var auth = MatchInt(text, @"[""']auth1[""']\s*:\s*(\d+)");
        if (string.IsNullOrWhiteSpace(host) || auth <= 0)
            return Array.Empty<GameChannel>();

        var channels = new List<GameChannel>();
        foreach (Match match in Regex.Matches(text, @"[""']ch(\d+)[""']\s*:\s*(\d+)", RegexOptions.IgnoreCase))
        {
            if (!int.TryParse(match.Groups[1].Value, out var number) || number < 1)
                continue;
            if (!int.TryParse(match.Groups[2].Value, out var port) || port is < 1 or > 65535)
                continue;
            channels.Add(new GameChannel(number - 1, "CH-" + number, host, port, host, auth));
        }

        return channels
            .GroupBy(item => item.Index)
            .Select(group => group.First())
            .OrderBy(item => item.Index)
            .ToList();
    }

    public static IReadOnlyList<GameChannel> Fallback(string host, int channelPort, int authPort)
    {
        if (string.IsNullOrWhiteSpace(host))
            host = "127.0.0.1";
        if (channelPort is < 1 or > 65535)
            channelPort = 30003;
        if (authPort is < 1 or > 65535)
            authPort = 30001;
        return Enumerable.Range(0, 4)
            .Select(index => new GameChannel(index, "CH-" + (index + 1), host, channelPort + index * 4, host, authPort))
            .ToList();
    }

    private static string MatchOne(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static int MatchInt(string text, string pattern)
    {
        var value = MatchOne(text, pattern);
        return int.TryParse(value, out var number) ? number : 0;
    }
}
