namespace ClientInspect;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var reportAt = ReportPath(args);
        if (reportAt != null)
        {
            var report = Analysis.ProcessProbe.Run(@"E:\TeraBoxDownload\1.Client\1.metin2client.exe");
            File.WriteAllText(reportAt, report.ToText());
            Console.WriteLine(report.ToText());
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static string? ReportPath(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--report", StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }
}
