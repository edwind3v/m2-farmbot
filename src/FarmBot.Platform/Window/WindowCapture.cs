using FarmBot.Platform.Native;

namespace FarmBot.Platform.Window;

public readonly record struct FrameSample(int Width, int Height, bool PixelRead, uint Pixel);

public static class WindowCapture
{
    public static FrameSample Sample(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero || !Win32.GetClientRect(hwnd, out var rect))
            return default;

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
            return new FrameSample(Math.Max(0, width), Math.Max(0, height), false, 0);

        var dc = Win32.GetDC(hwnd);
        if (dc == IntPtr.Zero)
            return new FrameSample(width, height, false, 0);

        try
        {
            var pixel = Win32.GetPixel(dc, width / 2, height / 2);
            var read = pixel != 0xFFFFFFFFu;
            return new FrameSample(width, height, read, read ? pixel : 0);
        }
        finally
        {
            Win32.ReleaseDC(hwnd, dc);
        }
    }
}

public static class CursorProbe
{
    public static bool IsInTopLeftCorner()
    {
        if (!Win32.GetCursorPos(out var point))
            return false;
        return point.X <= 1 && point.Y <= 1;
    }
}
