namespace FarmBot.Core.Combat;

public static class SubIcon
{
    public static bool TryLoad(string? subPath, out TgaImage image)
    {
        image = new TgaImage();
        if (string.IsNullOrWhiteSpace(subPath) || !File.Exists(subPath))
            return false;
        string text;
        try
        {
            text = File.ReadAllText(subPath);
        }
        catch (IOException)
        {
            return false;
        }

        if (!TryParse(text, out var atlasName, out var left, out var top, out var right, out var bottom))
            return false;
        var dir = Path.GetDirectoryName(subPath) ?? "";
        var atlas = ResolveAtlas(dir, atlasName);
        if (atlas == null || !TgaImage.TryLoadFile(atlas, out var sheet) || !sheet.HasPixels)
            return false;
        image = sheet.Crop(left, top, right, bottom);
        return image.HasPixels;
    }

    public static bool TryParse(string text, out string image, out int left, out int top, out int right, out int bottom)
    {
        image = "";
        left = top = right = bottom = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string? title = null;
        foreach (var raw in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Length == 0)
                continue;
            var parts = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;
            var key = parts[0].Trim().ToLowerInvariant();
            var value = parts[1].Trim().Trim('"', '\'').ToLowerInvariant();
            switch (key)
            {
                case "title":
                    title = value;
                    break;
                case "image":
                    image = value;
                    break;
                case "left":
                    int.TryParse(value, out left);
                    break;
                case "top":
                    int.TryParse(value, out top);
                    break;
                case "right":
                    int.TryParse(value, out right);
                    break;
                case "bottom":
                    int.TryParse(value, out bottom);
                    break;
            }
        }

        if (title != null && title != "subimage")
            return false;
        if (string.IsNullOrWhiteSpace(image) || right <= left || bottom <= top)
            return false;
        if (right - left > 256 || bottom - top > 256)
            return false;
        return true;
    }

    private static string? ResolveAtlas(string dir, string image)
    {
        var file = Path.GetFileName(image.Replace('/', '\\'));
        var candidates = new[]
        {
            Path.Combine(dir, file),
            Path.Combine(dir, image.Replace('/', Path.DirectorySeparatorChar)),
            Path.Combine(dir, Path.GetFileNameWithoutExtension(file) + ".dds"),
            Path.Combine(dir, Path.GetFileNameWithoutExtension(file) + ".tga")
        };
        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}
