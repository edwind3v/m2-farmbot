using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

internal static class ResourceLoader
{
	private static class Support_04B0
	{
		internal static readonly byte[] Field_04B1 = new byte[11]
		{
			42, 13, 96, 70, 186, 145, 246, 46, 1, 101,
			93
		};

		internal static readonly byte Field_04B2 = 40;

		internal static readonly MethodBase[] Field_04B3 = new MethodBase[2]
		{
			typeof(Assembly).GetMethod("GetManifestResourceStream", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(string) }, null),
			typeof(ResourceLoader).GetMethod("Method_0058", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null)
		};

		internal static readonly FieldInfo[] Field_04B4 = new FieldInfo[0];

		internal static readonly string[] Field_04B5 = new string[0];

		internal static readonly Type[] Field_04B6 = new Type[0];
	}

	private static class Support_04B7
	{
		internal static readonly byte[] Field_04B8 = new byte[13]
		{
			70, 97, 12, 42, 210, 255, 156, 71, 108, 12,
			55, 211, 255
		};

		internal static readonly byte Field_04B9 = 68;

		internal static readonly MethodBase[] Field_04BA = new MethodBase[2]
		{
			typeof(Assembly).GetMethod("GetManifestResourceStream", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Type), typeof(string) }, null),
			typeof(ResourceLoader).GetMethod("Method_0058", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null)
		};

		internal static readonly FieldInfo[] Field_04BB = new FieldInfo[0];

		internal static readonly string[] Field_04BC = new string[0];

		internal static readonly Type[] Field_04BD = new Type[0];
	}

	private static class Support_04BE
	{
		internal static readonly byte[] Field_04BF = new byte[235]
		{
			8, 47, 73, 108, 158, 179, 212, 5, 37, 69,
			120, 152, 182, 235, 8, 33, 89, 127, 174, 146,
			220, 54, 166, 89, 114, 151, 203, 237, 34, 0,
			93, 90, 170, 207, 224, 4, 101, 86, 116, 169,
			205, 230, 219, 56, 86, 139, 191, 245, 120, 31,
			48, 85, 141, 163, 225, 194, 19, 5, 104, 141,
			166, 218, 154, 25, 74, 111, 143, 164, 118, 251,
			20, 73, 113, 179, 58, 221, 246, 43, 79, 97,
			182, 132, 209, 251, 46, 67, 100, 152, 98, 219,
			8, 45, 73, 122, 16, 189, 234, 15, 51, 113,
			252, 147, 180, 233, 1, 39, 117, 70, 151, 255,
			236, 1, 58, 94, 206, 146, 206, 227, 11, 56,
			145, 112, 168, 205, 245, 40, 181, 81, 138, 175,
			195, 229, 58, 41, 85, 137, 165, 196, 248, 51,
			13, 104, 140, 142, 225, 205, 45, 32, 109, 131,
			148, 226, 243, 118, 72, 109, 134, 186, 53, 14,
			213, 176, 111, 132, 228, 211, 244, 41, 81, 90,
			141, 190, 214, 58, 23, 64, 38, 159, 176, 213,
			15, 19, 71, 121, 146, 184, 233, 153, 37, 91,
			124, 130, 247, 250, 4, 36, 93, 115, 170, 238,
			208, 23, 15, 93, 118, 175, 206, 226, 26, 13,
			83, 119, 138, 192, 230, 39, 21, 82, 138, 173,
			197, 249, 31, 48, 111, 140, 160, 192, 250, 19,
			16, 115, 135, 167, 223
		};

		internal static readonly byte Field_04C0 = 10;

		internal static readonly MethodBase[] Field_04C1 = new MethodBase[6]
		{
			typeof(ResourceLoader).GetMethod("Method_0059", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null),
			typeof(MemoryStream).GetConstructor(new Type[] { typeof(byte[]), typeof(bool) }),
			typeof(Array).GetMethod("Copy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) }, null),
			typeof(ResourceLoader).GetMethod("Method_005A", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(int) }, null),
			typeof(ResourceLoader).GetMethod("Method_005C", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null),
			typeof(MemoryStream).GetConstructor(new Type[] { typeof(byte[]), typeof(bool) })
		};

		internal static readonly FieldInfo[] Field_04C2 = new FieldInfo[0];

		internal static readonly string[] Field_04C3 = new string[0];

		internal static readonly Type[] Field_04C4 = new Type[1] { typeof(byte) };
	}

	private static class Support_04C5
	{
		internal static readonly byte[] Field_04C6 = new byte[23]
		{
			53, 10, 101, 68, 187, 148, 241, 47, 7, 100,
			92, 184, 145, 206, 47, 6, 127, 89, 181, 150,
			200, 38, 6
		};

		internal static readonly byte Field_04C7 = 47;

		internal static readonly MethodBase[] Field_04C8 = new MethodBase[4]
		{
			typeof(MemoryStream).GetConstructor(new Type[] {  }),
			typeof(Stream).GetMethod("CopyTo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null),
			typeof(Stream).GetMethod("Dispose", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null),
			typeof(MemoryStream).GetMethod("ToArray", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null)
		};

		internal static readonly FieldInfo[] Field_04C9 = new FieldInfo[0];

		internal static readonly string[] Field_04CA = new string[0];

		internal static readonly Type[] Field_04CB = new Type[0];
	}

	private static class Support_04CC
	{
		internal static readonly byte[] Field_04CD = new byte[76]
		{
			72, 110, 1, 46, 222, 243, 148, 72, 144, 10,
			56, 221, 249, 170, 145, 108, 26, 63, 195, 240,
			174, 67, 96, 25, 48, 214, 137, 241, 70, 123,
			28, 48, 136, 185, 140, 94, 113, 18, 196, 226,
			130, 167, 75, 110, 23, 194, 236, 129, 186, 94,
			104, 7, 206, 227, 139, 184, 99, 101, 40, 205,
			245, 158, 188, 81, 14, 46, 194, 229, 157, 179,
			87, 8, 39, 198, 248, 154
		};

		internal static readonly byte Field_04CE = 74;

		internal static readonly MethodBase[] Field_04CF = new MethodBase[2]
		{
			typeof(ResourceLoader).GetMethod("Method_005D", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(int), typeof(int) }, null),
			typeof(ResourceLoader).GetMethod("Method_005B", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(byte[]), typeof(byte[]) }, null)
		};

		internal static readonly FieldInfo[] Field_04D0 = new FieldInfo[0];

		internal static readonly string[] Field_04D1 = new string[0];

		internal static readonly Type[] Field_04D2 = new Type[0];
	}

	private static class Support_04D3
	{
		internal static readonly byte[] Field_04D4 = new byte[39]
		{
			9, 41, 70, 109, 154, 180, 212, 14, 36, 79,
			124, 153, 182, 239, 10, 41, 121, 127, 149, 179,
			230, 41, 36, 91, 116, 151, 203, 235, 2, 61,
			126, 117, 172, 247, 193, 5, 62, 85, 117
		};

		internal static readonly byte Field_04D5 = 12;

		internal static readonly MethodBase[] Field_04D6 = new MethodBase[7]
		{
			typeof(Aes).GetMethod("Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null),
			typeof(SymmetricAlgorithm).GetMethod("set_Key", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null),
			typeof(SymmetricAlgorithm).GetMethod("set_IV", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null),
			typeof(SymmetricAlgorithm).GetMethod("set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(CipherMode) }, null),
			typeof(SymmetricAlgorithm).GetMethod("set_Padding", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(PaddingMode) }, null),
			typeof(SymmetricAlgorithm).GetMethod("CreateDecryptor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null),
			typeof(ICryptoTransform).GetMethod("TransformFinalBlock", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null)
		};

		internal static readonly FieldInfo[] Field_04D7 = new FieldInfo[0];

		internal static readonly string[] Field_04D8 = new string[0];

		internal static readonly Type[] Field_04D9 = new Type[0];
	}

	private static class Support_04DA
	{
		internal static readonly byte[] Field_04DB = new byte[25]
		{
			181, 146, 231, 216, 35, 42, 115, 181, 159, 224,
			199, 32, 15, 86, 178, 156, 225, 193, 45, 11,
			83, 184, 157, 228, 200
		};

		internal static readonly byte Field_04DC = 183;

		internal static readonly MethodBase[] Field_04DD = new MethodBase[5]
		{
			typeof(MemoryStream).GetConstructor(new Type[] { typeof(byte[]) }),
			typeof(DeflateStream).GetConstructor(new Type[] { typeof(Stream), typeof(CompressionMode) }),
			typeof(MemoryStream).GetConstructor(new Type[] {  }),
			typeof(Stream).GetMethod("CopyTo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null),
			typeof(MemoryStream).GetMethod("ToArray", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null)
		};

		internal static readonly FieldInfo[] Field_04DE = new FieldInfo[0];

		internal static readonly string[] Field_04DF = new string[0];

		internal static readonly Type[] Field_04E0 = new Type[0];
	}

	private static class Support_04E1
	{
		internal static readonly byte[] Field_04E2 = new byte[9] { 43, 11, 100, 67, 186, 145, 241, 45, 1 };

		internal static readonly byte Field_04E3 = 46;

		internal static readonly MethodBase[] Field_04E4 = new MethodBase[2]
		{
			typeof(SHA256).GetMethod("Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] {  }, null),
			typeof(HashAlgorithm).GetMethod("ComputeHash", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null)
		};

		internal static readonly FieldInfo[] Field_04E5 = new FieldInfo[0];

		internal static readonly string[] Field_04E6 = new string[0];

		internal static readonly Type[] Field_04E7 = new Type[0];
	}

	private const byte Field_0061 = 69;

	private const byte Field_0062 = 48;

	private const byte Field_0063 = 82;

	private const byte Field_0064 = 49;

	private const int Field_0065 = 455884110;

	public static Stream Method_0056(Assembly assembly, string name)
	{
		byte[] code = ManagedVm.Method_009F(Support_04B0.Field_04B1, Support_04B0.Field_04B2);
		return (Stream)VmDispatcher.Method_004E(code, new object[2] { assembly, name }, Support_04B0.Field_04B3, Support_04B0.Field_04B4, Support_04B0.Field_04B5, Support_04B0.Field_04B6, Support_04B0.Field_04B2);
	}

	public static Stream Method_0057(Assembly assembly, Type type, string name)
	{
		byte[] code = ManagedVm.Method_009F(Support_04B7.Field_04B8, Support_04B7.Field_04B9);
		return (Stream)VmDispatcher.Method_004E(code, new object[3] { assembly, type, name }, Support_04B7.Field_04BA, Support_04B7.Field_04BB, Support_04B7.Field_04BC, Support_04B7.Field_04BD, Support_04B7.Field_04B9);
	}

	private static Stream Method_0058(Stream source)
	{
		byte[] code = ManagedVm.Method_009F(Support_04BE.Field_04BF, Support_04BE.Field_04C0);
		return (Stream)VmDispatcher.Method_004E(code, new object[1] { source }, Support_04BE.Field_04C1, Support_04BE.Field_04C2, Support_04BE.Field_04C3, Support_04BE.Field_04C4, Support_04BE.Field_04C0);
	}

	private static byte[] Method_0059(Stream source)
	{
		byte[] code = ManagedVm.Method_009F(Support_04C5.Field_04C6, Support_04C5.Field_04C7);
		return (byte[])VmDispatcher.Method_004E(code, new object[1] { source }, Support_04C5.Field_04C8, Support_04C5.Field_04C9, Support_04C5.Field_04CA, Support_04C5.Field_04CB, Support_04C5.Field_04C7);
	}

	private static byte[] Method_005A(byte[] cipher, int seed)
	{
		byte[] code = ManagedVm.Method_009F(Support_04CC.Field_04CD, Support_04CC.Field_04CE);
		return (byte[])VmDispatcher.Method_004E(code, new object[2] { cipher, seed }, Support_04CC.Field_04CF, Support_04CC.Field_04D0, Support_04CC.Field_04D1, Support_04CC.Field_04D2, Support_04CC.Field_04CE);
	}

	private static byte[] Method_005B(byte[] cipher, byte[] key, byte[] iv)
	{
		byte[] code = ManagedVm.Method_009F(Support_04D3.Field_04D4, Support_04D3.Field_04D5);
		return (byte[])VmDispatcher.Method_004E(code, new object[3] { cipher, key, iv }, Support_04D3.Field_04D6, Support_04D3.Field_04D7, Support_04D3.Field_04D8, Support_04D3.Field_04D9, Support_04D3.Field_04D5);
	}

	private static byte[] Method_005C(byte[] data)
	{
		byte[] code = ManagedVm.Method_009F(Support_04DA.Field_04DB, Support_04DA.Field_04DC);
		return (byte[])VmDispatcher.Method_004E(code, new object[1] { data }, Support_04DA.Field_04DD, Support_04DA.Field_04DE, Support_04DA.Field_04DF, Support_04DA.Field_04E0, Support_04DA.Field_04DC);
	}

	private static byte[] Method_005D(int seed, int length)
	{
		string text = seed.ToString("X8");
		byte[] array = Method_0060(Method_005F(Method_005E() + text));
		byte[] array2 = new byte[length];
		Array.Copy(array, array2, Math.Min(array.Length, length));
		if (length > array.Length)
		{
			for (int i = array.Length; i < length; i++)
			{
				array2[i] = (byte)(array[i % array.Length] ^ (seed >> ((i % (0x105A ^ 0x104A)) & 0x1F)));
			}
		}
		return array2;
	}

	private static string Method_005E()
	{
		return new string(new char[8] { 'E', 't', 'h', 'R', 'e', 's', ':', ':' });
	}

	private static byte[] Method_005F(string text)
	{
		return Encoding.UTF8.GetBytes(text);
	}

	private static byte[] Method_0060(byte[] data)
	{
		byte[] code = ManagedVm.Method_009F(Support_04E1.Field_04E2, Support_04E1.Field_04E3);
		return (byte[])VmDispatcher.Method_004E(code, new object[1] { data }, Support_04E1.Field_04E4, Support_04E1.Field_04E5, Support_04E1.Field_04E6, Support_04E1.Field_04E7, Support_04E1.Field_04E3);
	}
}
