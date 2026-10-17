using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal static class NativeLibraryResolver
{
	private delegate IntPtr LoadLibraryADelegate(string name);

	private delegate IntPtr GetProcAddressDelegate(IntPtr module, string name);

	private static LoadLibraryADelegate Field_0049;

	private static GetProcAddressDelegate Field_004A;

	public static IntPtr Method_0043(string library, string function, char suffix)
	{
		Method_0044();
		if (Field_0049 == null || Field_004A == null)
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = Field_0049(library);
		string text = Method_0047();
		if (intPtr == IntPtr.Zero && !library.EndsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			intPtr = Field_0049(library + text);
		}
		if (intPtr == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr2 = Field_004A(intPtr, function);
		if (intPtr2 == IntPtr.Zero && suffix != '\0')
		{
			intPtr2 = Field_004A(intPtr, function + suffix);
		}
		if (intPtr2 == IntPtr.Zero)
		{
			intPtr2 = Field_004A(intPtr, function + "A");
			if (intPtr2 == IntPtr.Zero)
			{
				intPtr2 = Field_004A(intPtr, function + "W");
			}
		}
		return intPtr2;
	}

	private static void Method_0044()
	{
		if (Field_0049 != null)
		{
			return;
		}
		try
		{
			IntPtr intPtr = Method_0045();
			if (!(intPtr == IntPtr.Zero))
			{
				IntPtr intPtr2 = Method_0046(intPtr, Method_0048(new byte[12]
				{
					22, 50, 53, 43, 10, 16, 18, 25, 3, 23,
					101, 86
				}, 90));
				IntPtr intPtr3 = Method_0046(intPtr, Method_0048(new byte[14]
				{
					29, 56, 32, 31, 52, 22, 19, 42, 6, 1,
					110, 114, 125, 114
				}, 90));
				if (!(intPtr2 == IntPtr.Zero) && !(intPtr3 == IntPtr.Zero))
				{
					Field_0049 = (LoadLibraryADelegate)Marshal.GetDelegateForFunctionPointer(intPtr2, typeof(LoadLibraryADelegate));
					Field_004A = (GetProcAddressDelegate)Marshal.GetDelegateForFunctionPointer(intPtr3, typeof(GetProcAddressDelegate));
				}
			}
		}
		catch
		{
		}
	}

	private static IntPtr Method_0045()
	{
		string value = Method_0048(new byte[12]
		{
			49, 56, 38, 33, 35, 21, 67, 89, 76, 1,
			112, 123
		}, 90);
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				string moduleName = module.ModuleName;
				if (moduleName != null && moduleName.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return module.BaseAddress;
				}
			}
		}
		catch
		{
		}
		return IntPtr.Zero;
	}

	private static IntPtr Method_0046(IntPtr module, string exportName)
	{
		if (module == IntPtr.Zero || string.IsNullOrEmpty(exportName))
		{
			return IntPtr.Zero;
		}
		int num = Marshal.ReadInt32(module, 60);
		ushort num2 = (ushort)Marshal.ReadInt16(module, num + 24);
		int num3;
		switch (num2)
		{
		case 267:
			if (IntPtr.Size != 4)
			{
				return IntPtr.Zero;
			}
			num3 = Marshal.ReadInt32(module, num + 24 + 96);
			break;
		case 523:
			if (IntPtr.Size != 8)
			{
				return IntPtr.Zero;
			}
			num3 = Marshal.ReadInt32(module, num + 24 + 112);
			break;
		default:
			return IntPtr.Zero;
		}
		if (num3 == 0)
		{
			return IntPtr.Zero;
		}
		IntPtr ptr = IntPtr.Add(module, num3);
		int num4 = Marshal.ReadInt32(ptr, 24);
		int offset = Marshal.ReadInt32(ptr, 32);
		int offset2 = Marshal.ReadInt32(ptr, 36);
		int offset3 = Marshal.ReadInt32(ptr, 28);
		int num5 = Marshal.ReadInt32(module, num + 24 + ((num2 == 267) ? 96 : 112) + 4);
		IntPtr ptr2 = IntPtr.Add(module, offset);
		IntPtr ptr3 = IntPtr.Add(module, offset2);
		IntPtr ptr4 = IntPtr.Add(module, offset3);
		for (int i = 0; i < num4; i++)
		{
			int offset4 = Marshal.ReadInt32(ptr2, i * 4);
			string text = Marshal.PtrToStringAnsi(IntPtr.Add(module, offset4));
			if (text != null && text.Equals(exportName, StringComparison.Ordinal))
			{
				ushort num6 = (ushort)Marshal.ReadInt16(ptr3, i * 2);
				int num7 = Marshal.ReadInt32(ptr4, num6 * 4);
				if (num7 == 0)
				{
					return IntPtr.Zero;
				}
				if (num5 > 0 && num7 >= num3 && num7 < num3 + num5)
				{
					return IntPtr.Zero;
				}
				return IntPtr.Add(module, num7);
			}
		}
		return IntPtr.Zero;
	}

	private static string Method_0047()
	{
		return Method_0048(new byte[4] { 116, 57, 56, 35 }, 90);
	}

	private static string Method_0048(byte[] encoded, byte key)
	{
		char[] array = new char[encoded.Length];
		for (int i = 0; i < encoded.Length; i++)
		{
			array[i] = (char)(encoded[i] ^ key ^ (byte)(i * 7));
		}
		return new string(array);
	}
}
