using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

internal static class IntegrityGuard
{
	private const int Field_003B = 16;

	private const int Field_003C = 32;

	internal static byte[] Field_003D;

	private static int Field_003E;

	internal const int Field_003F = 0;

	internal const int Field_0040 = 1;

	internal const int Field_0041 = 2;

    // User-approved removal of the original binary's obsolete integrity stamp.
    // A source rebuild changes the PE file, invalidating the old checksum.
    // Keep this entry point for the existing generated compatibility callers.
    public static void Method_0035()
    {
    }

	private static bool Method_0036(string location)
	{
		try
		{
			if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
			{
				return false;
			}
			if (string.IsNullOrEmpty(location))
			{
				return true;
			}
			if (location.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(location) && new FileInfo(location).Length > 8388608)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	internal static int Method_0037(string location, byte[] marker)
	{
		if (marker == null || marker.Length != 16)
		{
			return 0;
		}
		if (string.IsNullOrEmpty(location) || !File.Exists(location))
		{
			return 0;
		}
		byte[] array = File.ReadAllBytes(location);
		int num = Method_003A(array, marker);
		if (num < 0)
		{
			return 0;
		}
		int num2 = num + 16;
		if (num2 + 32 > array.Length)
		{
			return 0;
		}
		byte[] array2 = new byte[32];
		Buffer.BlockCopy(array, num2, array2, 0, 32);
		bool flag = true;
		for (int i = 0; i < 32; i++)
		{
			if (array2[i] != 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return 0;
		}
		for (int j = 0; j < 32; j++)
		{
			array[num2 + j] = 0;
		}
		byte[] array3 = CryptoPrimitives.Method_001E(array);
		if (array3.Length != 32)
		{
			return 0;
		}
		for (int k = 0; k < 32; k++)
		{
			if (array3[k] != array2[k])
			{
				return 2;
			}
		}
		return 1;
	}

	private static string Method_0038()
	{
		try
		{
			string location = Assembly.GetExecutingAssembly().Location;
			if (!string.IsNullOrEmpty(location) && File.Exists(location))
			{
				return location;
			}
		}
		catch
		{
			DebugGuard.Method_0008();
		}
		try
		{
			using Process process = Process.GetCurrentProcess();
			ProcessModule mainModule = process.MainModule;
			if (mainModule != null && !string.IsNullOrEmpty(mainModule.FileName) && File.Exists(mainModule.FileName))
			{
				return mainModule.FileName;
			}
		}
		catch
		{
			DebugGuard.Method_0008();
		}
		try
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			if (!string.IsNullOrEmpty(codeBase))
			{
				Uri uri = new Uri(codeBase);
				if (File.Exists(uri.LocalPath))
				{
					return uri.LocalPath;
				}
			}
		}
		catch
		{
			DebugGuard.Method_0008();
		}
		return string.Empty;
	}

	private static string Method_0039()
	{
		byte[] array = new byte[20]
		{
			19, 51, 32, 42, 33, 11, 25, 31, 27, 69,
			106, 126, 97, 109, 89, 71, 67, 66, 74, 241
		};
		char[] array2 = new char[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = (char)(array[i] ^ 0x5A ^ (byte)(i * 7));
		}
		return new string(array2);
	}

	private static int Method_003A(byte[] haystack, byte[] needle)
	{
		for (int i = 0; i <= haystack.Length - needle.Length; i++)
		{
			bool flag = true;
			for (int j = 0; j < needle.Length; j++)
			{
				if (haystack[i + j] != needle[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	static IntegrityGuard()
	{
		Assembly assembly = typeof(IntegrityGuard).Assembly;
		int num = 0x12AC54D7 ^ 0x12D77207;
		char[] array = new char[8];
		int num2 = ((num ^ 0) + 0) & 0xFFFF;
		array[0] = (char)(0x6DC ^ num2);
		num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
		num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
		array[1] = (char)(0x5C38 ^ num2);
		num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
		num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
		array[2] = (char)(0xDDCC ^ num2);
		num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
		num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
		array[3] = (char)(0xE5B0 ^ num2);
		num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
		num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
		array[4] = (char)(0x5C06 ^ num2);
		num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
		num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
		array[5] = (char)(0x420F ^ num2);
		num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
		num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
		array[6] = (char)(0x8889 ^ num2);
		num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
		num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
		array[7] = (char)(0xDD06 ^ num2);
		Stream manifestResourceStream = assembly.GetManifestResourceStream(new string(array));
		byte[] array2 = new byte[(int)manifestResourceStream.Length];
		manifestResourceStream.Read(array2, 0, array2.Length);
		byte[] array3 = new byte[16];
		Array.Copy(array2, 0, array3, 0, 16);
		Field_003D = array3;
	}
}
