using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace FarmBot;

public sealed class MainForm : Form
{
    private readonly FarmBotShell _shell;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 200 };
    private bool _hotkeysRegistered;
    private bool _closing;

    public MainForm()
        : this(new FarmBotShell(), "FarmBot")
    {
    }

    public MainForm(FarmBotShell shell, string title)
    {
        _shell = shell;
        _shell.PickClientPath = PickClientPath;
        Text = title;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(19, 21, 28);
        ForeColor = Color.FromArgb(236, 232, 223);
        ClientSize = new Size(1280, 820);
        MinimumSize = new Size(960, 620);
        HostBlazor();
        _timer.Tick += (_, _) =>
        {
            var name = _shell.View.Snapshot?.Player?.Name;
            if (!string.IsNullOrWhiteSpace(name) && !Text.Contains(name, StringComparison.Ordinal))
                Text = $"FarmBot — {name}";
        };
        _timer.Start();
    }

    public Action? SessionClosing { get; set; }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        RegisterHotkeys();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        UnregisterHotkeys();
        base.OnDeactivate(e);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (Focused)
            RegisterHotkeys();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_closing)
            return;
        _closing = true;
        _timer.Stop();
        UnregisterHotkeys();
        _shell.PersistOnExit();
        SessionClosing?.Invoke();
        _shell.Dispose();
        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        const int wmHotkey = 0x0312;
        if (m.Msg == wmHotkey)
        {
            var id = m.WParam.ToInt32();
            if (id == 1)
                _shell.ToggleRun();
            else if (id == 2)
                _shell.Panic();
        }

        base.WndProc(ref m);
    }

    private void HostBlazor()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            services.AddSingleton(_shell);
            services.AddSingleton<ThemeService>();
            var provider = services.BuildServiceProvider();

            var webView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = @"wwwroot\index.html",
                Services = provider
            };
            webView.RootComponents.Add<FarmBotApp>("#app");
            Controls.Add(webView);
        }
        catch (Exception ex)
        {
            var fallback = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "WebView2 is required for the FarmBot console.\nInstall the Evergreen WebView2 Runtime and restart FarmBot.exe.\n\n" + ex.Message,
                ForeColor = Color.FromArgb(240, 215, 138),
                Padding = new Padding(24)
            };
            Controls.Add(fallback);
        }
    }

    private string? PickClientPath()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Metin2 client|*.exe",
            Title = "Select the original Metin2 client"
        };
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }

    private void RegisterHotkeys()
    {
        if (!IsHandleCreated)
            return;
        UnregisterHotkeys();
        const uint modNorepeat = 0x4000;
        var config = _shell.Config;
        if (TryParseKey(config.StartHotkey, out var start))
            RegisterHotKey(Handle, 1, modNorepeat, (uint)start);
        if (TryParseKey(config.StopHotkey, out var stop))
            RegisterHotKey(Handle, 2, modNorepeat, (uint)stop);
        _hotkeysRegistered = true;
    }

    private void UnregisterHotkeys()
    {
        if (!_hotkeysRegistered || !IsHandleCreated)
            return;
        UnregisterHotKey(Handle, 1);
        UnregisterHotKey(Handle, 2);
        _hotkeysRegistered = false;
    }

    private static bool TryParseKey(string text, out Keys key) =>
        Enum.TryParse(text.Trim(), true, out key) && key != Keys.None;

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
