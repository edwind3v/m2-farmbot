using System;
using System.Reflection;
using System.Runtime.InteropServices;

internal static class VmDispatcher
{
	private delegate IntPtr NativeExecuteDelegate(IntPtr code, int codeLen, IntPtr args, int argCount, IntPtr methods, int methodCount, IntPtr fields, int fieldCount, IntPtr strings, int stringCount, IntPtr types, int typeCount, byte stringKey);

	private static readonly object Field_0051 = new object();

	private static bool Field_0052;

	private static bool Field_0053;

	private static NativeExecuteDelegate Field_0054;

	private static Support_0486 Field_0487;

	private static Support_0489 Field_048A;

	private static IntPtr Method_0488(string name)
	{
		if (Field_0487 == null)
		{
			int num = -1187801417 ^ -1344282250;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x77AA ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xFE43 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x99CF ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x6E05 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x4B30 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x8387 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xE4E3 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x8497 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x91C ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x1617 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x5CA8 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x28A9 ^ num2);
			string library = new string(array);
			int num3 = -478863715 ^ -1034350577;
			char[] array2 = new char[12];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xEDE ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xCE3 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x7C91 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xA784 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0xE02E ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x5A2D ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xB86C ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xDC6B ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xDA29 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x7CF7 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x9126 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0xA33F ^ num4);
			Field_0487 = (Support_0486)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), 'A'), typeof(Support_0486));
		}
		return Field_0487(name);
	}

	private static IntPtr Method_048B(IntPtr module, string name)
	{
		if (Field_048A == null)
		{
			int num = 0x30694453 ^ 0x4F6E4A2F;
			char[] array = new char[12];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xE17 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xD05 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x7FDA ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA13E ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xD6E7 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x56E8 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xC1BD ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x2F28 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0xB467 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x30E0 ^ num2);
			num2 = ((num ^ 0x582A) + 300) & 0xFFFF;
			num2 = ((num2 << 10) | (num2 >>> 6)) & 0xFFFF;
			array[10] = (char)(0x932 ^ num2);
			num2 = ((num ^ 0x60FB) + 363) & 0xFFFF;
			num2 = ((num2 << 11) | (num2 >>> 5)) & 0xFFFF;
			array[11] = (char)(0x9313 ^ num2);
			string library = new string(array);
			int num3 = 0x5CF0B359 ^ 0x688948E2;
			char[] array2 = new char[14];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xFBFC ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xE6BE ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0xA8E3 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xF4F ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x928F ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xFF55 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xF210 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x4FA2 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xF3D9 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xAB0F ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xF6E0 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x5C80 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x7931 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x3117 ^ num4);
			Field_048A = (Support_0489)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), 'A'), typeof(Support_0489));
		}
		return Field_048A(module, name);
	}

	public static object Method_004E(byte[] code, object[] args, MethodBase[] methods, FieldInfo[] fields, string[] strings, Type[] types, byte stringKey)
	{
		Method_004F();
		if (Field_0053 && Field_0054 != null)
		{
			try
			{
				return Method_0050(code, args, methods, fields, strings, types, stringKey);
			}
			catch
			{
			}
		}
		return ManagedVm.Method_00A0(code, args, methods, fields, strings, types, stringKey);
	}

	private static void Method_004F()
	{
		if (Field_0052)
		{
			return;
		}
		lock (Field_0051)
		{
			if (Field_0052)
			{
				return;
			}
			Field_0052 = true;
			try
			{
				IntPtr intPtr = Method_0488("ethvm.dll");
				if (!(intPtr == IntPtr.Zero))
				{
					IntPtr intPtr2 = Method_048B(intPtr, "vm_execute");
					if (!(intPtr2 == IntPtr.Zero))
					{
						Field_0054 = (NativeExecuteDelegate)(object)Marshal.GetDelegateForFunctionPointer<NativeExecuteDelegate>(intPtr2);
						Field_0053 = Field_0054 != null;
					}
				}
			}
			catch
			{
				Field_0053 = false;
			}
		}
	}

	private static object Method_0050(byte[] code, object[] args, MethodBase[] methods, FieldInfo[] fields, string[] strings, Type[] types, byte stringKey)
	{
		return ManagedVm.Method_00A0(code, args, methods, fields, strings, types, stringKey);
	}
}
