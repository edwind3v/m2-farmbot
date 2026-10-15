using System.Runtime.InteropServices;
using Microsoft.JSInterop;

namespace FarmBot;

public sealed class WindowChrome
{
    private Form? _form;

    public event Action? Changed;

    public bool IsMaximized => _form?.WindowState == FormWindowState.Maximized;

    public void Bind(Form form)
    {
        _form = form;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Padding = new Padding(1);
        form.Resize += (_, _) => Changed?.Invoke();
        if (form.IsHandleCreated)
            StyleHandle(form);
        else
            form.HandleCreated += (_, _) => StyleHandle(form);
    }

    public void Minimize()
    {
        if (_form != null)
            _form.WindowState = FormWindowState.Minimized;
    }

    public void ToggleMaximize()
    {
        if (_form == null)
            return;
        _form.WindowState = _form.WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
        Changed?.Invoke();
    }

    public void Close() => _form?.Close();

    [JSInvokable]
    public void DragFromJs() => Drag();

    public void Drag()
    {
        var form = _form;
        if (form == null || !form.IsHandleCreated || form.WindowState == FormWindowState.Maximized)
            return;
        ReleaseCapture();
        SendMessage(form.Handle, 0x00A1, 2, 0);
    }

    private static void StyleHandle(Form form)
    {
        try
        {
            var dark = 1;
            DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int));
            var round = 2;
            DwmSetWindowAttribute(form.Handle, 33, ref round, sizeof(int));
            var border = 0x2B201C;
            DwmSetWindowAttribute(form.Handle, 34, ref border, sizeof(int));
        }
        catch (DllNotFoundException)
        {
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
}
