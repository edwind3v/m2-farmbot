using ClientInspect.Analysis;

namespace ClientInspect;

public sealed class MainForm : Form
{
    private readonly TextBox _report = new();
    private readonly Label _status = new();
    private readonly Button _scan = new();
    private readonly Button _save = new();
    private InspectReport? _latest;

    public MainForm()
    {
        Text = "ClientInspect";
        Font = new Font("Segoe UI", 9.5f);
        ClientSize = new Size(980, 720);
        MinimumSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(28, 30, 34);
        ForeColor = Color.FromArgb(232, 232, 232);

        _status.AutoSize = false;
        _status.Height = 28;
        _status.Dock = DockStyle.Top;
        _status.Padding = new Padding(12, 8, 12, 0);
        _status.Text = "Finds 1.metin2client.exe and reports whether ordinary user-mode memory inspection is allowed.";

        var bar = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(12, 8, 12, 8) };
        _scan.Text = "Scan running client";
        _scan.Width = 180;
        _scan.Height = 32;
        _scan.Left = 12;
        _scan.Click += (_, _) => Scan();
        _save.Text = "Save report";
        _save.Width = 120;
        _save.Height = 32;
        _save.Left = 202;
        _save.Enabled = false;
        _save.Click += (_, _) => Save();
        bar.Controls.AddRange(new Control[] { _scan, _save });

        _report.Multiline = true;
        _report.ReadOnly = true;
        _report.ScrollBars = ScrollBars.Both;
        _report.WordWrap = false;
        _report.Dock = DockStyle.Fill;
        _report.Font = new Font("Consolas", 9f);
        _report.BackColor = Color.FromArgb(22, 23, 26);
        _report.ForeColor = Color.FromArgb(232, 232, 232);
        _report.BorderStyle = BorderStyle.None;

        Controls.Add(_report);
        Controls.Add(bar);
        Controls.Add(_status);
        Shown += (_, _) => Scan();
    }

    private void Scan()
    {
        try
        {
            _latest = ProcessProbe.Run(@"E:\TeraBoxDownload\1.Client\1.metin2client.exe");
            _report.Text = _latest.ToText();
            _save.Enabled = true;
            if (_latest.ProcessId is null)
                _status.Text = "Client process not found. On-disk analysis is still shown when the exe exists.";
            else if (_latest.VmReadGranted)
                _status.Text = $"PID {_latest.ProcessId}: PROCESS_VM_READ granted. Offsets are still unknown.";
            else
                _status.Text = $"PID {_latest.ProcessId}: PROCESS_VM_READ returned Win32 5 from this inspector. See confirmed vs unresolved.";
        }
        catch (Exception ex)
        {
            _latest = null;
            _save.Enabled = false;
            _report.Text = ex.ToString();
            _status.Text = "scan failed";
        }
    }

    private void Save()
    {
        if (_latest == null)
            return;
        using var dialog = new SaveFileDialog
        {
            Filter = "Text files|*.txt",
            FileName = "client-inspect.txt"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        File.WriteAllText(dialog.FileName, _latest.ToText());
    }
}
