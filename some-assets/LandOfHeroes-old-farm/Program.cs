using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    private const string EmbeddedUiAssembly = "Onur.Guna.UI2.dll";

    [DllImport("user32.dll",EntryPoint="SetProcessDpiAwarenessContext")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll",EntryPoint="SetProcessDPIAware")]
    private static extern bool SetProcessDPIAware();

    private static void EnableDpiAwareness()
    {
        try{if(SetProcessDpiAwarenessContext(new IntPtr(-4)))return;}catch{}
        try{SetProcessDPIAware();}catch{}
    }
	[Serializable]
	[CompilerGenerated]
	private sealed class Type_0322
	{
		public static readonly Type_0322 Field_0325 = new Type_0322();

		public static ThreadExceptionEventHandler Field_0326;

		public static UnhandledExceptionEventHandler Field_0327;

		internal void Method_0323(object s, ThreadExceptionEventArgs e)
		{

		}

		internal void Method_0324(object s, UnhandledExceptionEventArgs e)
		{

		}
	}

	[STAThread]
	private static void Main()
	{
		AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
		EnableDpiAwareness();
		IntegrityGuard.Method_0035();
		IntegrityGuard.Method_0035();
		DebugGuard.Method_0008();
		DebugGuard.Method_0008();
		Method_032A();
	}

	private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
	{
		AssemblyName requested;
		try { requested = new AssemblyName(args.Name); }
		catch { return null; }
		if (!string.Equals(requested.Name, "Guna.UI2", StringComparison.OrdinalIgnoreCase)) return null;
		using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream(EmbeddedUiAssembly))
		{
			if (stream == null) return null;
			byte[] assemblyBytes = new byte[stream.Length];
			int offset = 0;
			while (offset < assemblyBytes.Length)
			{
				int read = stream.Read(assemblyBytes, offset, assemblyBytes.Length - offset);
				if (read <= 0) return null;
				offset += read;
			}
			return Assembly.Load(assemblyBytes);
		}
	}
	private static void Method_0329()
	{
		try
		{
			string text = Path.GetTempPath();
			if (string.IsNullOrWhiteSpace(text))
			{
				text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
			}
			Directory.CreateDirectory(text);
			string path = Path.Combine(text, "rb_" + Thread.CurrentThread.ManagedThreadId + ".tmp");
			File.WriteAllText(path, "ok");
			File.Delete(path);
		}
		catch
		{
			try
			{
				string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Onur", "Temp");
				Directory.CreateDirectory(text2);
				Environment.SetEnvironmentVariable("TEMP", text2);
				Environment.SetEnvironmentVariable("TMP", text2);
			}
			catch
			{
			}
		}
	}

	private static void Method_032A()
	{
		Method_0329();
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		Application.ThreadException += (object s, ThreadExceptionEventArgs e) =>
		{

		};
		AppDomain.CurrentDomain.UnhandledException += (object s, UnhandledExceptionEventArgs e) =>
		{

		};
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new MainForm());
	}
}
