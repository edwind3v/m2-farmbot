using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public sealed class SessionProfile
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string CharacterName { get; set; } = "";
}
public static class SessionProfileStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Onur.SessionProfile.v1");
    public static void Save(string path, SessionProfile profile)
    {
        byte[] clear;
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            { writer.Write(1); writer.Write(profile.Username ?? ""); writer.Write(profile.Password ?? ""); writer.Write(profile.CharacterName ?? ""); }
            clear = stream.ToArray();
        }
        byte[] encrypted;
        try { encrypted = ProtectedData.Protect(clear, Entropy, DataProtectionScope.CurrentUser); }
        finally { Array.Clear(clear, 0, clear.Length); }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllBytes(temporary, encrypted); if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public static SessionProfile Load(string path)
    {
        byte[] clear = ProtectedData.Unprotect(File.ReadAllBytes(path), Entropy, DataProtectionScope.CurrentUser);
        try
        {
            using (var stream = new MemoryStream(clear))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.ReadInt32() != 1) throw new InvalidDataException("Hesap ayarları sürümü desteklenmiyor.");
                return new SessionProfile { Username=reader.ReadString(), Password=reader.ReadString(), CharacterName=reader.ReadString() };
            }
        }
        finally { Array.Clear(clear, 0, clear.Length); }
    }
}
