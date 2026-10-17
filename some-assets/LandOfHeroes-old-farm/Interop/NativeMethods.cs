using System;
using System.Runtime.InteropServices;
using System.Text;

public class NativeMethods
{
	internal enum Type_019A
	{
		Field_019B
	}

	internal enum Type_019C
	{
		Field_019D = 65535,
		Field_019E = 2031616
	}

	internal enum Type_019F
	{
		Field_01A0 = 0,
		Field_01A1 = 1,
		Field_01A2 = 4,
		Field_01A3 = 0x10000
	}

	internal enum Type_01A4
	{
		Field_01A5 = 0,
		Field_01A6 = 1,
		Field_01A7 = 2,
		Field_01A8 = 4,
		Field_01A9 = 8,
		Field_01AA = 0x10,
		Field_01AB = 0x20,
		Field_01AC = 0x40,
		Field_01AD = 0x80,
		Field_01AE = 0x100,
		Field_01AF = 0x200,
		Field_01B0 = 0x400,
		Field_01B1 = 0x800,
		Field_01B2 = 0x1000,
		Field_01B3 = 0x2000
	}

	public struct Type_01B4
	{
		public ushort Field_01B5;

		private ushort Field_01B6;

		public uint Field_01B7;

		public UIntPtr Field_01B8;

		public UIntPtr Field_01B9;

		public IntPtr Field_01BA;

		public uint Field_01BB;

		public uint Field_01BC;

		public uint Field_01BD;

		public ushort Field_01BE;

		public ushort Field_01BF;
	}

	public struct Type_01C0
	{
		public UIntPtr Field_01C1;

		public UIntPtr Field_01C2;

		public uint Field_01C3;

		public uint Field_01C4;

		public uint Field_01C5;

		public uint Field_01C6;

		public uint Field_01C7;
	}

	public struct Type_01C8
	{
		public UIntPtr Field_01C9;

		public UIntPtr Field_01CA;

		public uint Field_01CB;

		public uint Field_01CC;

		public ulong Field_01CD;

		public uint Field_01CE;

		public uint Field_01CF;

		public uint Field_01D0;

		public uint Field_01D1;
	}

	public struct Type_01D2
	{
		public UIntPtr Field_01D3;

		public UIntPtr Field_01D4;

		public uint Field_01D5;

		public long Field_01D6;

		public uint Field_01D7;

		public uint Field_01D8;

		public uint Field_01D9;
	}

	private enum Type_01DA : uint
	{
		Field_01DB = 1u,
		Field_01DC = 2u,
		Field_01DD = 4u,
		Field_01DE = 8u,
		Field_01DF = 16u,
		Field_01E0 = 2147483648u,
		Field_01E1 = 31u,
		Field_01E2 = 1073741824u
	}

	[Flags]
	public enum Type_01E3
	{
		Field_01E4 = 1,
		Field_01E5 = 2,
		Field_01E6 = 8,
		Field_01E7 = 0x10,
		Field_01E8 = 0x20,
		Field_01E9 = 0x40,
		Field_01EA = 0x80,
		Field_01EB = 0x100,
		Field_01EC = 0x200
	}

	[Flags]
	public enum Type_01ED : uint
	{
		Field_01EE = 0x10u,
		Field_01EF = 0x20u,
		Field_01F0 = 0x40u,
		Field_01F1 = 0x80u,
		Field_01F2 = 1u,
		Field_01F3 = 2u,
		Field_01F4 = 4u,
		Field_01F5 = 8u,
		Field_01F6 = 0x100u,
		Field_01F7 = 0x200u,
		Field_01F8 = 0x400u
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct Type_01F9
	{
		private const int Field_01FA = 260;

		internal uint Field_01FB;

		internal uint Field_01FC;

		internal uint Field_01FD;

		internal IntPtr Field_01FE;

		internal uint Field_01FF;

		internal uint Field_0200;

		internal uint Field_0201;

		internal int Field_0202;

		internal uint Field_0203;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string Field_0204;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct Type_0205
	{
		internal uint Field_0206;

		internal uint Field_0207;

		internal uint Field_0208;

		internal uint Field_0209;

		internal uint Field_020A;

		internal IntPtr Field_020B;

		internal uint Field_020C;

		internal IntPtr Field_020D;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string Field_020E;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string Field_020F;
	}

	public enum Type_0210
	{
		Field_0211 = 9
	}

	public const int Field_023A = 2;

	public const int Field_023B = 1024;

	public const int Field_023C = 8;

	public const int Field_023D = 32;

	public const int Field_023E = 16;

	public const uint Field_023F = 65536u;

	public const uint Field_0240 = 4096u;

	public const uint Field_0241 = 8192u;

	public const uint Field_0242 = 2u;

	public const uint Field_0243 = 4u;

	public const uint Field_0244 = 8u;

	public const uint Field_0245 = 64u;

	public const uint Field_0246 = 128u;

	public const uint Field_0247 = 16u;

	public const uint Field_0248 = 32u;

	public const uint Field_0249 = 256u;

	public const uint Field_024A = 1u;

	public const uint Field_024B = 131072u;

	public const uint Field_024C = 16777216u;

	public const uint Field_024D = 262144u;

	private static Support_0432 Field_0433;

	private static Support_0435 Field_0436;

	private static Support_0438 Field_0439;

	private static Support_043B Field_043C;

	private static Support_043E Field_043F;

	private static Support_0441 Field_0442;

	private static Support_0444 Field_0445;

	private static Support_0447 Field_0448;

	private static Support_044A Field_044B;

	private static Support_044D Field_044E;

	private static Support_0450 Field_0451;

	private static Support_0453 Field_0454;

	private static Support_0456 Field_0457;

	private static Support_0459 Field_045A;

	private static Support_045C Field_045D;

	private static Support_045F Field_0460;

	private static Support_0462 Field_0463;

	private static Support_0465 Field_0466;

	private static Support_0468 Field_0469;

	private static Support_046B Field_046C;

	private static Support_046E Field_046F;

	public static IntPtr Method_0434(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId)
	{
		if (Field_0433 == null)
		{
			int num = 0x58CE7BE1 ^ 0x46105FEC;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x2466 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x59DB ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xD69E ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xF4A7 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x77F5 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xC0D ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x55F7 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x26BF ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x454D ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x8EBC ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x4D99 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0xA5F ^ num2);
			string library = new string(array);
			int num3 = 0xDEEF5AF ^ 0x76E50F72;
			char[] array2 = new char[11];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xFA92 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xE46F ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xAE4A ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x621 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x9CCD ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xE208 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xA99C ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xFE80 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x15D8 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xEF1F ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x8EE3 ^ num4);
			Field_0433 = (Support_0432)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0432));
		}
		return Field_0433(dwDesiredAccess, bInheritHandle, dwProcessId);
	}

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
	public static extern UIntPtr Method_0214(IntPtr hProcess, UIntPtr lpAddress, out Type_01C0 lpBuffer, UIntPtr dwLength);

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
	public static extern UIntPtr Method_0215(IntPtr hProcess, UIntPtr lpAddress, out Type_01C8 lpBuffer, UIntPtr dwLength);

	private static uint Method_0437()
	{
		if (Field_0436 == null)
		{
			int num = 0x450FA1EB ^ 0x42DC2939;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x88B9 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x68 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x6580 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x958A ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xBC0F ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xA238 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x281C ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0xFC68 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x1AE1 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xFDF4 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x9324 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0xA720 ^ num2);
			string library = new string(array);
			int num3 = -893681259 ^ -1560653552;
			char[] array2 = new char[12];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xD0C2 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xB0CA ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x4BB ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x58C2 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x3F7E ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x9B0C ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x338D ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xE2B3 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xCDE4 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x9F33 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x6E48 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x4DFD ^ num4);
			Field_0436 = (Support_0435)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0435));
		}
		return Field_0436();
	}

	[DllImport("kernel32.dll", EntryPoint = "GetSystemInfo")]
	public static extern void Method_0217(out Type_01B4 lpSystemInfo);

	public static IntPtr Method_043A(Type_01E3 dwDesiredAccess, bool bInheritHandle, uint dwThreadId)
	{
		if (Field_0439 == null)
		{
			int num = -394551769 ^ -938979313;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xCE43 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x8D9C ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x7E29 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA3D8 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xD9AB ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x5170 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xCE8D ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x1948 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x60A7 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xC960 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0xBA30 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0xF5ED ^ num2);
			string library = new string(array);
			int num3 = -705332073 ^ -514138719;
			char[] array2 = new char[10];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xA179 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x53A5 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xC2E7 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xDB6B ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x2A7C ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xADB9 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x8F57 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x8A2B ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x7E89 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xC5BA ^ num4);
			Field_0439 = (Support_0438)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0438));
		}
		return Field_0439(dwDesiredAccess, bInheritHandle, dwThreadId);
	}

	public static int Method_043D(IntPtr hThread)
	{
		if (Field_043C == null)
		{
			int num = 0x4362311D ^ 0x27485545;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x6433 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xD97D ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xD86B ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xF25D ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x74A1 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x1365 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x4AA7 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x411F ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x900D ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xE833 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x789A ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x705C ^ num2);
			string library = new string(array);
			int num3 = -1463877209 ^ -627727107;
			char[] array2 = new char[13];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xA509 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x5B68 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xD461 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xFA55 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x648D ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x333F ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x8A40 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xC018 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x928C ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xEDA7 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x739F ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x6659 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x6C80 ^ num4);
			Field_043C = (Support_043B)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_043B));
		}
		return Field_043C(hThread);
	}

	public static int Method_0440(IntPtr hThread)
	{
		if (Field_043F == null)
		{
			int num = -15043153 ^ -1028558452;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x448 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x198F ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x5646 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xF336 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x7917 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x1049 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x4C7F ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x13AF ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x6B6D ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xDAFC ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0xD518 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x1B5E ^ num2);
			string library = new string(array);
			int num3 = -1721255665 ^ -997267617;
			char[] array2 = new char[12];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x5E02 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xAD6D ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x3F8A ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x2187 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xD42A ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x526B ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xC8CE ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x3D5A ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x986B ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xF846 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x987F ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xB264 ^ num4);
			Field_043F = (Support_043E)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_043E));
		}
		return Field_043F(hThread);
	}

	private static bool Method_0443(IntPtr hProcess, int ProcessId, IntPtr hFile, Type_01A4 DumpType, IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallackParam)
	{
		if (Field_0442 == null)
		{
			int num = -1302054643 ^ -469690482;
			char[] array = new char[11];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x2EE7 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x4CC8 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xFCD3 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA831 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xDF15 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x5C4C ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xB436 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0xE3A7 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xCB0C ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x9AA9 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x55B3 ^ num2);
			string library = new string(array);
			int num3 = -768257071 ^ -2001895580;
			char[] array2 = new char[17];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x2CF8 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x48A7 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xF4E2 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xB760 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x255 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x1D15 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x2FAB ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xCAF8 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xFD3D ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xBEBB ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x2DBE ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xCA19 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x9417 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x6C40 ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0xD6FC ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0xD5FB ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0xA4D5 ^ num4);
			Field_0442 = (Support_0441)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0441));
		}
		return Field_0442(hProcess, ProcessId, hFile, DumpType, ExceptionParam, UserStreamParam, CallackParam);
	}

	public static int Method_0446(IntPtr hWnd, int nIndex)
	{
		if (Field_0445 == null)
		{
			int num = -249714663 ^ -1860192972;
			char[] array = new char[10];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x1D58 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x2B8D ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x3209 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x3BBA ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xE9A0 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x3054 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x8DE4 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x96F4 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x6530 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xCECA ^ num2);
			string library = new string(array);
			int num3 = -1164305879 ^ -1098025468;
			char[] array2 = new char[13];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x6C6A ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xC99B ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xF619 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xB39C ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xF9FD ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x1006 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x4DB2 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x16C6 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x655C ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xCE04 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xCCBB ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x81C ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x101E ^ num4);
			Field_0445 = (Support_0444)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0444));
		}
		return Field_0445(hWnd, nIndex);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
	public static extern IntPtr Method_021D(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

	[DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
	public static extern bool Method_021E(IntPtr hProcess, UIntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

	public static int Method_0449(IntPtr handle)
	{
		if (Field_0448 == null)
		{
			int num = -918763151 ^ -901188814;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x3A28 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x654F ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xAFC6 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x237 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x9314 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xD44E ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xC470 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x43B6 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x8B53 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x1A88 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x55E2 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x1A8D ^ num2);
			string library = new string(array);
			int num3 = -2074402649 ^ -211829323;
			char[] array2 = new char[12];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x9D55 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x2BE8 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x3286 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x3BB4 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xE819 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x2A39 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x9849 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x9C35 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x5AAF ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x7DD5 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x9350 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xA79E ^ num4);
			Field_0448 = (Support_0447)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0447));
		}
		return Field_0448(handle);
	}

	public static uint Method_044C(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName)
	{
		if (Field_044B == null)
		{
			int num = -668584117 ^ -1320000158;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x5242 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xB593 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xE2F ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x43C4 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x19B2 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xD083 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xCEEA ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x188A ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x613B ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xC658 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0xBC40 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0xE9CD ^ num2);
			string library = new string(array);
			int num3 = -464015119 ^ -7158234;
			char[] array2 = new char[23];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x7690 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xFC77 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x9E71 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x65AB ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x5C47 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x61C2 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xA726 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xF9C4 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x1F45 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x210 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xA490 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xB8CE ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xB263 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0xA0AE ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x4468 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0xFAA9 ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0xFEA2 ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0xCD60 ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0xB0B3 ^ num4);
			num4 = ((num3 ^ 0xA783) + 1083) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[19] = (char)(0xAC0C ^ num4);
			num4 = ((num3 ^ 0xB054) + 1200) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[20] = (char)(0xB355 ^ num4);
			num4 = ((num3 ^ 0xB925) + 1323) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[21] = (char)(0xA3D4 ^ num4);
			num4 = ((num3 ^ 0xC1F6) + 1452) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[22] = (char)(0x3308 ^ num4);
			Field_044B = (Support_044A)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), 'W'), typeof(Support_044A));
		}
		return Field_044B(lpAppName, lpKeyName, lpDefault, lpReturnedString, nSize, lpFileName);
	}

	public static bool Method_044F(IntPtr hProcess, UIntPtr lpAddress, UIntPtr dwSize, uint dwFreeType)
	{
		if (Field_044E == null)
		{
			int num = -1157863307 ^ -45557392;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xB16E ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x73CA ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x82BC ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x5CE3 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x277C ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xAB1F ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x93D2 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0xA2F4 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x4DD6 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x9F9A ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x6FC5 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x4EF7 ^ num2);
			string library = new string(array);
			int num3 = 0x8EE1EE3 ^ 0x75095FC1;
			char[] array2 = new char[13];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x4174 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x9385 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x4243 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xDB16 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x2913 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xB02C ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x8C71 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x9478 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x6A7A ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xDC7B ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xD00D ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x215F ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xE2D1 ^ num4);
			Field_044E = (Support_044D)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_044D));
		}
		return Field_044E(hProcess, lpAddress, dwSize, dwFreeType);
	}

	[DllImport("psapi.dll", EntryPoint = "GetModuleFileNameEx")]
	private static extern uint Method_0222(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

	[DllImport("psapi.dll", EntryPoint = "EnumProcessModules", SetLastError = true)]
	public static extern bool Method_0223(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

	[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
	public static extern bool Method_0224(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
	public static extern bool Method_0225(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

	[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
	public static extern bool Method_0226(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

	public static UIntPtr Method_0452(IntPtr hProcess, UIntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
	{
		if (Field_0451 == null)
		{
			int num = -972094153 ^ -1247123926;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x6376 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xD7FB ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xCB5F ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xCC25 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x8F1 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xEA05 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x19E5 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x9E9D ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x5508 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x6E3E ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x8C9D ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x8846 ^ num2);
			string library = new string(array);
			int num3 = -181969879 ^ -1576751525;
			char[] array2 = new char[14];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xF224 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xF524 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x8F01 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x4093 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x1618 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xD63A ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xC05D ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x2C29 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xBAD9 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x3D10 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x12C1 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xA4FC ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xE993 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x5025 ^ num4);
			Field_0451 = (Support_0450)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0450));
		}
		return Field_0451(hProcess, lpAddress, dwSize, flAllocationType, flProtect);
	}

	[DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx")]
	public static extern bool Method_0228(IntPtr hProcess, UIntPtr lpAddress, IntPtr dwSize, Type_01ED flNewProtect, out Type_01ED lpflOldProtect);

	public static UIntPtr Method_0455(IntPtr hModule, string procName)
	{
		if (Field_0454 == null)
		{
			int num = -308003873 ^ -256130637;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xFA07 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xE5E4 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xAF19 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x1B9 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x95E8 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xD8F6 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xBD80 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x3756 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xA493 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x5108 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0xCAE1 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x148C ^ num2);
			string library = new string(array);
			int num3 = -1260345519 ^ -1371292334;
			char[] array2 = new char[14];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x1A44 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x25CF ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x2EC0 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x408 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x9701 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xCC49 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xD428 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x23D5 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x4B39 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x9AC8 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x557F ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x1B86 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xF724 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x2D20 ^ num4);
			Field_0454 = (Support_0453)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0453));
		}
		return Field_0454(hModule, procName);
	}

	private static bool Method_0458(IntPtr hObject)
	{
		if (Field_0457 == null)
		{
			int num = 0x1D54188F ^ 0x688D5D4D;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x45A9 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x9A49 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x51C3 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xFE0C ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x6B03 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x4421 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x642F ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x840E ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xA2A ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x1C73 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x5010 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x2159 ^ num2);
			string library = new string(array);
			int num3 = -132183287 ^ -682563836;
			char[] array2 = new char[11];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xEC4E ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xC9D3 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xF680 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xB4BC ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xF7F9 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xC30 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x5597 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x2687 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x45CF ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x8F24 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x4EB0 ^ num4);
			Field_0457 = (Support_0456)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0456));
		}
		return Field_0457(hObject);
	}

	public static int Method_045B(IntPtr hObject)
	{
		if (Field_045A == null)
		{
			int num = -771883647 ^ -1218144807;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x6833 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xC17D ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xE86B ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x925D ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xB4A1 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x9364 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x4AA4 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x4119 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x9001 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xE82B ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x78AA ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x703C ^ num2);
			string library = new string(array);
			int num3 = 0x176096BB ^ 0x3F408502;
			char[] array2 = new char[11];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x13FA ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x36BA ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x8F3 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x4F5B ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x12B6 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xFEAF ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xF2A8 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x50F9 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xF131 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xA6D7 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xFD57 ^ num4);
			Field_045A = (Support_0459)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0459));
		}
		return Field_045A(hObject);
	}

	public static IntPtr Method_045E(string lpModuleName)
	{
		if (Field_045D == null)
		{
			int num = -227573687 ^ -1353634098;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xAEEC ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x4CD6 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xFCB4 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA813 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xDF5D ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x5BDC ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xB355 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0xE1FB ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xCFC6 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xA3A1 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x67B3 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x3E13 ^ num2);
			string library = new string(array);
			int num3 = -1146364479 ^ -613639305;
			char[] array2 = new char[15];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x46F1 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x9CB1 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x5CF5 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xE74F ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x6249 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x5DA9 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xAF69 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xCA51 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xFE65 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xC45D ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x201E ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xC153 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xA376 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0xC6A8 ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x106C ^ num4);
			Field_045D = (Support_045C)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), 'W'), typeof(Support_045C));
		}
		return Field_045D(lpModuleName);
	}

	public static int Method_0461(IntPtr handle, int milliseconds)
	{
		if (Field_0460 == null)
		{
			int num = 0x41A3C4DB ^ 0x445325E0;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xE150 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xD3BE ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xC2E5 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xDB71 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x2A99 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xAF55 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x9246 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x8FDC ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x7386 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xAB3A ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0xF684 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x5C75 ^ num2);
			string library = new string(array);
			int num3 = -78133181 ^ -1709349652;
			char[] array2 = new char[19];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xACF8 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x4962 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xF40F ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xB7C9 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x1FF ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x20DF ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x2D14 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xD59B ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xE783 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xD3A7 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xC7B1 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xFE01 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x3C14 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0xBC4A ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x76E1 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x95F3 ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0x24DA ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0x7901 ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0xD7D0 ^ num4);
			Field_0460 = (Support_045F)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_045F));
		}
		return Field_0460(handle, milliseconds);
	}

	[DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
	public static extern bool Method_022E(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

	[DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
	public static extern bool Method_022F(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

	public static IntPtr Method_0464(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, UIntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId)
	{
		if (Field_0463 == null)
		{
			int num = -1463932663 ^ -584160310;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x42A8 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x944F ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x4DC7 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xC634 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x1B13 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xE441 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xA46E ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x3F2 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xB2B ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x1A79 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x541C ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x1971 ^ num2);
			string library = new string(array);
			int num3 = 0x189D5A3B ^ 0x2C94EB91;
			char[] array2 = new char[18];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xB1E9 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x728F ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x8037 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x5FC4 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x319D ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xC136 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x6E73 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x5823 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xE29A ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xCD90 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xB3DE ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xE6F0 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x6DF5 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x58CE ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x3336 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x1C79 ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0x3FDB ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0x4F38 ^ num4);
			Field_0463 = (Support_0462)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0462));
		}
		return Field_0463(hProcess, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, out lpThreadId);
	}

	public static bool Method_0467(IntPtr hProcess, out bool lpSystemInfo)
	{
		if (Field_0466 == null)
		{
			int num = -694015511 ^ -1307564635;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xC27 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x925 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x779A ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xB2BE ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xF3E7 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x14E8 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x45BD ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x472B ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x8465 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x10EC ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x493A ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x131D ^ num2);
			string library = new string(array);
			int num3 = -1195475535 ^ -357613516;
			char[] array2 = new char[14];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x3DCC ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x6ADD ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xB09B ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x40E6 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xEF66 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x3B54 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x73F6 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x62D0 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xCD09 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x9E88 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x6DF8 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x4A92 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x952C ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x6A51 ^ num4);
			Field_0466 = (Support_0465)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0465));
		}
		return Field_0466(hProcess, out lpSystemInfo);
	}

	public static bool Method_046A(IntPtr hWnd)
	{
		if (Field_0469 == null)
		{
			int num = -1640056207 ^ -201868543;
			char[] array = new char[10];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x105 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x133B ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x431D ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xD882 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x2671 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xB637 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x80A3 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0xAD7A ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xB824 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x38F2 ^ num2);
			string library = new string(array);
			int num3 = -1104322483 ^ -1677569485;
			char[] array2 = new char[19];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x322D ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x7501 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x8FD4 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x4107 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x16CE ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xD6B1 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xC164 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x2E6F ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xB607 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x3493 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x1DB ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x82F1 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x25B2 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0xC80C ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x12BE ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x5C1C ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0xC20A ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0x566A ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0xC215 ^ num4);
			Field_0469 = (Support_0468)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0468));
		}
		return Field_0469(hWnd);
	}

	[DllImport("kernel32", CharSet = CharSet.Auto, EntryPoint = "CreateToolhelp32Snapshot", SetLastError = true)]
	public static extern IntPtr Method_0233([In] uint dwFlags, [In] uint th32ProcessID);

	[DllImport("kernel32", CharSet = CharSet.Auto, EntryPoint = "Process32First", SetLastError = true)]
	private static extern bool Method_0234([In] IntPtr hSnapshot, ref Type_01F9 lppe);

	[DllImport("kernel32.dll", EntryPoint = "Module32First")]
	public static extern bool Method_0235(IntPtr hSnapshot, ref Type_0205 lpme);

	[DllImport("kernel32.dll", EntryPoint = "Module32Next")]
	public static extern bool Method_0236(IntPtr hSnapshot, ref Type_0205 lpme);

	[DllImport("kernel32", CharSet = CharSet.Auto, EntryPoint = "Process32Next", SetLastError = true)]
	private static extern bool Method_0237([In] IntPtr hSnapshot, ref Type_01F9 lppe);

	internal static Type_019A Method_046D(out IntPtr hProcess, Type_019C desiredAccess, IntPtr objectAttributes, UIntPtr processHandle, IntPtr startAddress, IntPtr parameter, Type_019F inCreateSuspended, int stackZeroBits, int sizeOfStack, int maximumStackSize, IntPtr attributeList)
	{
		if (Field_046C == null)
		{
			int num = 0x25A52B13 ^ 0xD49DCD6;
			char[] array = new char[9];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xF7AB ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xFE5B ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x99AB ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x6EE3 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x4B71 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x8355 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xE394 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x8289 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xDDE ^ num2);
			string library = new string(array);
			int num3 = -1486208807 ^ -866303189;
			char[] array2 = new char[16];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x3BC ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x1638 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x4933 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xCC92 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xE07 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x627 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xE079 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x6C7A ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x3A12 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x3CF3 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x1106 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xA346 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xE6DF ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x4E09 ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x1EFF ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x4540 ^ num4);
			Field_046C = (Support_046B)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_046B));
		}
		return Field_046C(out hProcess, desiredAccess, objectAttributes, processHandle, startAddress, parameter, inCreateSuspended, stackZeroBits, sizeOfStack, maximumStackSize, attributeList);
	}

	internal static int Method_0470(IntPtr threadHandle, Type_0210 threadInformationClass, IntPtr threadInformation, int threadInformationLength, IntPtr returnLengthPtr)
	{
		if (Field_046F == null)
		{
			int num = -1579326891 ^ -1890853441;
			char[] array = new char[9];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xBF84 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0x6E09 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xB936 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x2DC9 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xCD85 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x897C ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xDE46 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x782D ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x2296 ^ num2);
			string library = new string(array);
			int num3 = -1019009469 ^ -1076132319;
			char[] array2 = new char[24];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xA02C ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x5119 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xC763 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xD110 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x350D ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x9823 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x3C5C ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x3406 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xAA89 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x5D86 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xD38A ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x2662 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xECD8 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x5AFE ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x3722 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x13E9 ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0x301D ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0x71A2 ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0xA25 ^ num4);
			num4 = ((num3 ^ 0xA783) + 1083) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[19] = (char)(0x6088 ^ num4);
			num4 = ((num3 ^ 0xB054) + 1200) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[20] = (char)(0x4E13 ^ num4);
			num4 = ((num3 ^ 0xB925) + 1323) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[21] = (char)(0xCE26 ^ num4);
			num4 = ((num3 ^ 0xC1F6) + 1452) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[22] = (char)(0xD078 ^ num4);
			num4 = ((num3 ^ 0xCAC7) + 1587) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[23] = (char)(0x6C5C ^ num4);
			Field_046F = (Support_046E)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_046E));
		}
		return Field_046F(threadHandle, threadInformationClass, threadInformation, threadInformationLength, returnLengthPtr);
	}
}
