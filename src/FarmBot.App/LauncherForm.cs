using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace FarmBot;

public sealed class LauncherForm : Form
{
    private readonly LauncherShell _shell;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 400 };

    public LauncherForm()
    {
        _shell = new LauncherShell
        {
            PickClientPath = PickClientPath
        };

        Text = "FarmBot Launcher";
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(19, 21, 28);
        ForeColor = Color.FromArgb(236, 232, 223);
        ClientSize = new Size(460, 720);
        MinimumSize = new Size(400, 520);
        HostBlazor();
        _timer.Tick += (_, _) => _shell.Tick();
        _timer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        var running = _shell.State.Slots.Any(_shell.IsRunning);
        if (running)
        {
            var result = MessageBox.Show(
                this,
                "Close the launcher and every FarmBot client it started?",
                "FarmBot Launcher",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        _timer.Stop();
        _shell.ShutdownAll();
        _shell.Dispose();
        base.OnFormClosing(e);
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
            webView.RootComponents.Add<LauncherApp>("#app");
            Controls.Add(webView);
        }
        catch (Exception ex)
        {
            var fallback = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "WebView2 is required for the FarmBot launcher.\nInstall the Evergreen WebView2 Runtime and restart FarmBot.exe.\n\n" + ex.Message,
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
            Filter = "Metin2 client|1.metin2client.exe;*.exe",
            Title = "Select the original Metin2 client"
        };
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
    }
}
