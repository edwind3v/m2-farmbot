namespace FarmBot;

public static class KillIcon
{
    public static string Stone => Load("metin-stone.png");
    public static string Boss => Load("boss.png");

    private static readonly Dictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    private static string Load(string fileName)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(fileName, out var cached))
                return cached;
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "icons", fileName);
            var src = File.Exists(path)
                ? "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(path))
                : "icons/" + fileName;
            Cache[fileName] = src;
            return src;
        }
    }
}
