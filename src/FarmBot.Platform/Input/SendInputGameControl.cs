using System.Runtime.InteropServices;
using FarmBot.Core.Input;
using FarmBot.Core.Logging;
using FarmBot.Core.Models;
using FarmBot.Platform.Native;
using FarmBot.Platform.Window;

namespace FarmBot.Platform.Input;

public sealed class SendInputGameControl : IGameControl
{
    private readonly Func<GameWindowInfo?> _window;
    private readonly IBotLog _log;
    private long _lastNoteAt;
    private string _lastNote = "";

    public SendInputGameControl(Func<GameWindowInfo?> window, IBotLog log)
    {
        _window = window;
        _log = log;
    }

    public void Execute(BotCommand command)
    {
        switch (command.Kind)
        {
            case BotCommandKind.Stop:
                return;
            case BotCommandKind.Move:
                Note("world move is delivered through the client IPC channel; screen move was skipped");
                return;
            case BotCommandKind.Heal:
                if (!Prepare(out _))
                    return;
                Tap(VirtualKeys.FromName(command.Key));
                return;
            case BotCommandKind.Pickup:
                if (!Prepare(out var pickupWindow))
                    return;
                if (command.OnScreen && TryToScreen(pickupWindow.Handle, command.ScreenX, command.ScreenY, out var pickupX, out var pickupY))
                    ClickAndTap(pickupX, pickupY, VirtualKeys.FromName(command.Key));
                else
                    Tap(VirtualKeys.FromName(command.Key));
                return;
            case BotCommandKind.Attack:
            case BotCommandKind.Shot:
            case BotCommandKind.Skill:
                if (!command.OnScreen)
                {
                    Note($"{command.Kind.ToString().ToLowerInvariant()} skipped because the target is off screen");
                    return;
                }
                if (!Prepare(out var window))
                    return;
                if (!TryToScreen(window.Handle, command.ScreenX, command.ScreenY, out var x, out var y))
                {
                    Note("ClientToScreen failed");
                    return;
                }
                ClickAndTap(x, y, VirtualKeys.FromName(command.Key));
                return;
        }
    }

    private bool Prepare(out GameWindowInfo window)
    {
        var found = _window();
        if (found == null)
        {
            window = new GameWindowInfo(IntPtr.Zero, "", 0, 0, 0, 0);
            Note("game window not found; input skipped");
            return false;
        }

        window = found;
        if (Win32.GetForegroundWindow() != found.Handle)
        {
            Win32.ShowWindow(found.Handle, Win32.SwRestore);
            Win32.SetForegroundWindow(found.Handle);
        }

        if (Win32.GetForegroundWindow() == found.Handle)
            return true;

        Note("game window is not foreground; input skipped");
        return false;
    }

    private static bool TryToScreen(IntPtr hwnd, int clientX, int clientY, out int screenX, out int screenY)
    {
        var point = new Win32.POINT { X = clientX, Y = clientY };
        if (!Win32.ClientToScreen(hwnd, ref point))
        {
            screenX = 0;
            screenY = 0;
            return false;
        }

        screenX = point.X;
        screenY = point.Y;
        return true;
    }

    private void ClickAndTap(int screenX, int screenY, ushort virtualKey)
    {
        Normalize(screenX, screenY, out var x, out var y);
        var inputs = new List<Win32.INPUT>
        {
            Mouse(x, y, Win32.MouseeventfMove | Win32.MouseeventfAbsolute),
            Mouse(x, y, Win32.MouseeventfLeftdown | Win32.MouseeventfAbsolute),
            Mouse(x, y, Win32.MouseeventfLeftup | Win32.MouseeventfAbsolute)
        };
        AppendKey(inputs, virtualKey);
        Dispatch(inputs);
    }

    private void Tap(ushort virtualKey)
    {
        if (virtualKey == 0)
        {
            Note("key is not mapped");
            return;
        }

        var inputs = new List<Win32.INPUT>();
        AppendKey(inputs, virtualKey);
        Dispatch(inputs);
    }

    private static void AppendKey(List<Win32.INPUT> inputs, ushort virtualKey)
    {
        if (virtualKey == 0)
            return;
        inputs.Add(Key(virtualKey, keyUp: false));
        inputs.Add(Key(virtualKey, keyUp: true));
    }

    private void Dispatch(List<Win32.INPUT> inputs)
    {
        var sent = Win32.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<Win32.INPUT>());
        if (sent != inputs.Count)
            Note($"SendInput delivered {sent} of {inputs.Count} events ({Marshal.GetLastWin32Error()})");
    }

    private static Win32.INPUT Mouse(int x, int y, uint flags) => new()
    {
        type = Win32.InputMouse,
        U = new Win32.InputUnion
        {
            mi = new Win32.MOUSEINPUT
            {
                dx = x,
                dy = y,
                dwFlags = flags
            }
        }
    };

    private static Win32.INPUT Key(ushort virtualKey, bool keyUp)
    {
        var scan = (ushort)Win32.MapVirtualKey(virtualKey, 0);
        var flags = Win32.KeyeventfScancode;
        if (keyUp)
            flags |= Win32.KeyeventfKeyup;
        return new Win32.INPUT
        {
            type = Win32.InputKeyboard,
            U = new Win32.InputUnion
            {
                ki = new Win32.KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = scan,
                    dwFlags = flags
                }
            }
        };
    }

    private static void Normalize(int x, int y, out int absoluteX, out int absoluteY)
    {
        var width = Math.Max(1, Win32.GetSystemMetrics(0) - 1);
        var height = Math.Max(1, Win32.GetSystemMetrics(1) - 1);
        absoluteX = (int)Math.Round(x * 65535.0 / width);
        absoluteY = (int)Math.Round(y * 65535.0 / height);
    }

    private void Note(string message)
    {
        var now = Environment.TickCount64;
        if (message == _lastNote && now - _lastNoteAt < 1000)
            return;
        _lastNote = message;
        _lastNoteAt = now;
        _log.Warn(message);
    }
}
