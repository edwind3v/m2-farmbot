using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
    }
}

// Guna.UI2 kütüphanesi çıktıya "onur.dll" adıyla kopyalanır; CLR "Guna.UI2" istediğinde bu dosyadan yüklenir.
internal static class GunaLoader
{
    private const string LibraryFileName = "onur.dll";

    [ModuleInitializer]
    internal static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (!string.Equals(new AssemblyName(args.Name).Name, "Guna.UI2", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        Assembly self = typeof(GunaLoader).Assembly;
        using (Stream res = self.GetManifestResourceStream("Onur.Guna.UI2.dll"))
        {
            if (res != null)
            {
                byte[] buffer = new byte[res.Length];
                int read = 0;
                while (read < buffer.Length)
                {
                    int n = res.Read(buffer, read, buffer.Length - read);
                    if (n <= 0) break;
                    read += n;
                }
                return Assembly.Load(buffer);
            }
        }
        string dir = Path.GetDirectoryName(self.Location);
        if (string.IsNullOrEmpty(dir))
        {
            dir = AppDomain.CurrentDomain.BaseDirectory;
        }
        string path = Path.Combine(dir, LibraryFileName);
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }
}
