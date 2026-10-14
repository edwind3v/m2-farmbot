namespace FarmBot.Core.Logging;

public sealed record BotLogEntry(DateTime Time, string Level, string Message);

public interface IBotLog
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}

public sealed class ListLog : IBotLog
{
    private readonly object _gate = new();

    public List<BotLogEntry> Entries { get; } = new();

    public void Debug(string message) => Write("DEBUG", message);
    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        lock (_gate)
            Entries.Add(new BotLogEntry(DateTime.Now, level, message));
    }
}

public sealed class FileBotLog : IBotLog
{
    private readonly object _gate = new();
    private readonly string _directory;

    public FileBotLog(string directory)
    {
        _directory = directory;
    }

    public event Action<BotLogEntry>? Written;

    public void Debug(string message) => Write("DEBUG", message);
    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var entry = new BotLogEntry(DateTime.Now, level, message);
        var line = $"{entry.Time:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
        lock (_gate)
        {
            try
            {
                Directory.CreateDirectory(_directory);
                var path = Path.Combine(_directory, $"farmbot-{entry.Time:yyyyMMdd}.log");
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch
            {
                // The UI sink still receives the line if the log folder is unavailable.
            }
        }

        Written?.Invoke(entry);
    }
}
