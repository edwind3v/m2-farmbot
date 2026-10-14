using System.Text;
using FarmBot.Platform.Native;

namespace FarmBot.Platform.Window;

public sealed record GameWindowInfo(IntPtr Handle, string Title, int Left, int Top, int Right, int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
}

public static class GameWindowFinder
{
    public static GameWindowInfo? Find(IReadOnlyList<string>? titleContains)
    {
        if (titleContains == null || titleContains.Count == 0)
            return null;

        GameWindowInfo? found = null;
        Win32.EnumWindowsProc callback = (hwnd, _) =>
        {
            if (!Win32.IsWindowVisible(hwnd))
                return true;

            var title = ReadTitle(hwnd);
            if (title.Length == 0)
                return true;

            foreach (var part in titleContains)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                if (!title.Contains(part, StringComparison.OrdinalIgnoreCase))
                    continue;

                Win32.GetWindowRect(hwnd, out var rect);
                found = new GameWindowInfo(hwnd, title, rect.Left, rect.Top, rect.Right, rect.Bottom);
                return false;
            }

            return true;
        };

        Win32.EnumWindows(callback, IntPtr.Zero);
        GC.KeepAlive(callback);
        return found;
    }

    private static string ReadTitle(IntPtr hwnd)
    {
        var buffer = new StringBuilder(512);
        var length = Win32.GetWindowText(hwnd, buffer, buffer.Capacity);
        return length <= 0 ? "" : buffer.ToString();
    }
}
