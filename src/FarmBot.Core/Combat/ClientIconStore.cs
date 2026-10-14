namespace FarmBot.Core.Combat;

public static class ClientIconStore
{
    public static string ClientPath { get; set; } = "";

    public static string ItemPath(uint vnum) =>
        $"icon/item/{vnum:D5}.tga";

    public static string DataUri(string? extractedPath, uint vnum = 0, string? clientPath = null)
    {
        clientPath ??= ClientPath;
        if (TryNice(extractedPath, out var uri))
            return uri;
        if (vnum != 0)
        {
            var cached = Path.Combine(Path.GetTempPath(), "farmbot-icons", $"{vnum}.tga");
            if (!string.Equals(cached, extractedPath, StringComparison.OrdinalIgnoreCase) &&
                TryNice(cached, out uri))
                return uri;
        }

        return TgaImage.DataUriFromFile(extractedPath);
    }

    public static bool TryLoadFromPack(string? clientPath, string virtualPath, out TgaImage image)
    {
        image = new TgaImage();
        var packPath = IconPackPath(clientPath);
        if (packPath == null || string.IsNullOrWhiteSpace(virtualPath) || !ArtPack.TryOpen(packPath, out var pack))
            return false;
        pack.Dispose();
        return false;
    }

    public static string? IconPackPath(string? clientPath)
    {
        if (string.IsNullOrWhiteSpace(clientPath))
            return null;
        var root = File.Exists(clientPath) ? Path.GetDirectoryName(clientPath) : clientPath;
        if (string.IsNullOrWhiteSpace(root))
            return null;
        var pack = Path.Combine(root, "pack", "icon.art");
        return File.Exists(pack) ? pack : null;
    }

    private static bool TryNice(string? path, out string uri)
    {
        uri = "";
        if (!TgaImage.TryLoadFile(path ?? "", out var image) || !image.HasPixels || IsFlatPlaceholder(image))
            return false;
        uri = image.ToPngDataUri();
        return true;
    }

    private static bool IsFlatPlaceholder(TgaImage image)
    {
        if (!image.HasPixels || image.Width * image.Height < 4)
            return false;
        var p = image.Bgra;
        var b = p[0];
        var g = p[1];
        var r = p[2];
        for (var i = 4; i < p.Length; i += 4)
        {
            if (p[i] != b || p[i + 1] != g || p[i + 2] != r)
                return false;
        }

        return true;
    }
}
