using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ProcessMemory
{
	[Serializable]
	[CompilerGenerated]
	private sealed class Type_011E
	{
		public static readonly Type_011E Field_0120 = new Type_011E();

		public static Func<long, long> Field_0121;

		internal long Method_011F(long c)
		{
			return c;
		}
	}

	[CompilerGenerated]
	private sealed class Type_0122
	{
		public string Field_0124;

		internal bool Method_0123(ProcessModule m)
		{
			return string.Equals(m.ModuleName, Field_0124, StringComparison.OrdinalIgnoreCase);
		}
	}

	[CompilerGenerated]
	private sealed class Type_0125
	{
		public ProcessMemory Field_0127;

		public string Field_0128;

		public string Field_0129;

		public long Field_012A;

		public long Field_012B;

		public bool Field_012C;

		public bool Field_012D;

		public bool Field_012E;

		public bool Field_012F;

		internal IEnumerable<long> Method_0126()
		{
			List<MemoryRegion> list = new List<MemoryRegion>();
			string[] array = Field_0127.Method_0163(Field_0128, Field_0129).Split(' ');
			byte[] array2 = new byte[array.Length];
			byte[] array3 = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text == "??" || (text.Length == 1 && text == "?"))
				{
					array3[i] = 0;
					array[i] = "0x00";
				}
				else if (char.IsLetterOrDigit(text[0]) && text[1] == '?')
				{
					array3[i] = 240;
					array[i] = text[0] + "0";
				}
				else if (char.IsLetterOrDigit(text[1]) && text[0] == '?')
				{
					array3[i] = 15;
					array[i] = "0" + text[1];
				}
				else
				{
					array3[i] = byte.MaxValue;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = (byte)(Convert.ToByte(array[j], 16) & array3[j]);
			}
			NativeMethods.Type_01B4 lpSystemInfo = default;
			NativeMethods.Method_0217(out lpSystemInfo);
			UIntPtr uIntPtr = lpSystemInfo.Field_01B8;
			UIntPtr uIntPtr2 = lpSystemInfo.Field_01B9;
			if (Field_012A < (long)uIntPtr.ToUInt64())
			{
				Field_012A = (long)uIntPtr.ToUInt64();
			}
			if (Field_012B > (long)uIntPtr2.ToUInt64())
			{
				Field_012B = (long)uIntPtr2.ToUInt64();
			}
			UIntPtr uIntPtr3 = new UIntPtr((ulong)Field_012A);
			NativeMethods.Type_01D2 lpBuffer = default;
			while (Field_0127.Method_015C(Field_0127.Field_0197.Property_0269, uIntPtr3, out lpBuffer).ToUInt64() != 0L && uIntPtr3.ToUInt64() < (ulong)Field_012B && (ulong)((long)uIntPtr3.ToUInt64() + lpBuffer.Field_01D6) > uIntPtr3.ToUInt64())
			{
				bool flag = lpBuffer.Field_01D7 == 4096;
				flag &= lpBuffer.Field_01D3.ToUInt64() < uIntPtr2.ToUInt64();
				flag &= (lpBuffer.Field_01D8 & 0x100) == 0;
				flag &= (lpBuffer.Field_01D8 & 1) == 0;
				flag &= lpBuffer.Field_01D9 == 131072 || lpBuffer.Field_01D9 == 16777216;
				if (Field_012C)
				{
					flag &= lpBuffer.Field_01D9 == 262144;
				}
				if (flag)
				{
					bool flag2 = (lpBuffer.Field_01D8 & 2) != 0;
					bool flag3 = (lpBuffer.Field_01D8 & 4) != 0 || (lpBuffer.Field_01D8 & 8) != 0 || (lpBuffer.Field_01D8 & 0x40) != 0 || (lpBuffer.Field_01D8 & 0x80) != 0;
					bool flag4 = (lpBuffer.Field_01D8 & 0x10) != 0 || (lpBuffer.Field_01D8 & 0x20) != 0 || (lpBuffer.Field_01D8 & 0x40) != 0 || (lpBuffer.Field_01D8 & 0x80) != 0;
					flag2 &= Field_012D;
					flag3 &= Field_012E;
					flag4 &= Field_012F;
					flag &= flag2 | flag3 | flag4;
				}
				if (!flag)
				{
					uIntPtr3 = new UIntPtr(lpBuffer.Field_01D3.ToUInt64() + (ulong)lpBuffer.Field_01D6);
					continue;
				}
				MemoryRegion item = new MemoryRegion
				{
					Property_0258 = uIntPtr3,
					Property_0259 = lpBuffer.Field_01D6,
					Property_025A = lpBuffer.Field_01D3
				};
				uIntPtr3 = new UIntPtr(lpBuffer.Field_01D3.ToUInt64() + (ulong)lpBuffer.Field_01D6);
				if (list.Count > 0)
				{
					MemoryRegion obj = list[list.Count - 1];
					if ((long)(ulong)obj.Property_025A + obj.Property_0259 == (long)(ulong)lpBuffer.Field_01D3)
					{
						list[list.Count - 1] = new MemoryRegion
						{
							Property_0258 = obj.Property_0258,
							Property_025A = obj.Property_025A,
							Property_0259 = obj.Property_0259 + lpBuffer.Field_01D6
						};
						continue;
					}
				}
				list.Add(item);
			}
			ConcurrentBag<long> concurrentBag = new ConcurrentBag<long>();
			Parallel.ForEach(list, (MemoryRegion item2, ParallelLoopState parallelLoopState, long index) =>
			{
				long[] array4 = Field_0127.Method_0159(item2, array2, array3);
				foreach (long item3 in array4)
				{
					concurrentBag.Add(item3);
				}
			});
			return (from c in concurrentBag.ToList()
				orderby c
				select c).AsEnumerable();
		}
	}

	[CompilerGenerated]
	private sealed class Type_0130
	{
		public byte[] Field_0132;

		public byte[] Field_0133;

		public ConcurrentBag<long> Field_0134;

		public Type_0125 Field_0135;

		internal void Method_0131(MemoryRegion item, ParallelLoopState parallelLoopState, long index)
		{
			long[] array = Field_0135.Field_0127.Method_0159(item, Field_0132, Field_0133);
			foreach (long item2 in array)
			{
				Field_0134.Add(item2);
			}
		}
	}

	[CompilerGenerated]
	private sealed class Type_0136
	{
		public Action<string> Field_0138;

		public ProcessMemory Field_0139;

		public string Field_013A;

		public string Field_013B;

		public CancellationTokenSource Field_013C;

		internal void Method_0137()
		{
			while (!Field_013C.Token.IsCancellationRequested)
			{
				Field_0138(Field_0139.Method_018E<string>(Field_013A, Field_013B));
				Thread.Sleep(100);
			}
		}
	}

	[CompilerGenerated]
	private sealed class Type_013D
	{
		public ProcessMemory Field_013F;

		public string Field_0140;

		public string Field_0141;

		public string Field_0142;

		public string Field_0143;

		public CancellationTokenSource Field_0144;

		internal void Method_013E()
		{
			while (!Field_0144.Token.IsCancellationRequested)
			{
				Field_013F.WriteValue(Field_0140, Field_0141, Field_0142, Field_0143);
				Thread.Sleep(25);
			}
		}
	}

	public ProcessContext Field_0197 = new ProcessContext();

	private ConcurrentDictionary<string, CancellationTokenSource> Field_0198 = new ConcurrentDictionary<string, CancellationTokenSource>();

	private ConcurrentDictionary<string, CancellationTokenSource> Field_0199 = new ConcurrentDictionary<string, CancellationTokenSource>();

	public Task<IEnumerable<long>> Method_0154(string search, bool writable = false, bool executable = true, string file = "")
	{
		return Method_0156(0L, long.MaxValue, search, writable, executable, mapped: false, file);
	}

	public Task<IEnumerable<long>> Method_0155(string search, bool readable, bool writable, bool executable, string file = "")
	{
		return Method_0157(0L, long.MaxValue, search, readable, writable, executable, mapped: false, file);
	}

	public Task<IEnumerable<long>> Method_0156(long start, long end, string search, bool writable = false, bool executable = true, bool mapped = false, string file = "")
	{
		return Method_0157(start, end, search, readable: false, writable, executable, mapped, file);
	}

	public Task<IEnumerable<long>> Method_0157(long start, long end, string search, bool readable, bool writable, bool executable, bool mapped, string file = "")
	{
		return Task.Run(() =>
		{
			List<MemoryRegion> list = new List<MemoryRegion>();
			string[] array = Method_0163(search, file).Split(' ');
			byte[] array2 = new byte[array.Length];
			byte[] array3 = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text == "??" || (text.Length == 1 && text == "?"))
				{
					array3[i] = 0;
					array[i] = "0x00";
				}
				else if (char.IsLetterOrDigit(text[0]) && text[1] == '?')
				{
					array3[i] = 240;
					array[i] = text[0] + "0";
				}
				else if (char.IsLetterOrDigit(text[1]) && text[0] == '?')
				{
					array3[i] = 15;
					array[i] = "0" + text[1];
				}
				else
				{
					array3[i] = byte.MaxValue;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = (byte)(Convert.ToByte(array[j], 16) & array3[j]);
			}
			NativeMethods.Type_01B4 lpSystemInfo = default;
			NativeMethods.Method_0217(out lpSystemInfo);
			UIntPtr uIntPtr = lpSystemInfo.Field_01B8;
			UIntPtr uIntPtr2 = lpSystemInfo.Field_01B9;
			if (start < (long)uIntPtr.ToUInt64())
			{
				start = (long)uIntPtr.ToUInt64();
			}
			if (end > (long)uIntPtr2.ToUInt64())
			{
				end = (long)uIntPtr2.ToUInt64();
			}
			UIntPtr uIntPtr3 = new UIntPtr((ulong)start);
			NativeMethods.Type_01D2 lpBuffer = default;
			while (Method_015C(Field_0197.Property_0269, uIntPtr3, out lpBuffer).ToUInt64() != 0L && uIntPtr3.ToUInt64() < (ulong)end && (ulong)((long)uIntPtr3.ToUInt64() + lpBuffer.Field_01D6) > uIntPtr3.ToUInt64())
			{
				bool flag = lpBuffer.Field_01D7 == 4096;
				flag &= lpBuffer.Field_01D3.ToUInt64() < uIntPtr2.ToUInt64();
				flag &= (lpBuffer.Field_01D8 & 0x100) == 0;
				flag &= (lpBuffer.Field_01D8 & 1) == 0;
				flag &= lpBuffer.Field_01D9 == 131072 || lpBuffer.Field_01D9 == 16777216;
				if (mapped)
				{
					flag &= lpBuffer.Field_01D9 == 262144;
				}
				if (flag)
				{
					bool flag2 = (lpBuffer.Field_01D8 & 2) != 0;
					bool flag3 = (lpBuffer.Field_01D8 & 4) != 0 || (lpBuffer.Field_01D8 & 8) != 0 || (lpBuffer.Field_01D8 & 0x40) != 0 || (lpBuffer.Field_01D8 & 0x80) != 0;
					bool flag4 = (lpBuffer.Field_01D8 & 0x10) != 0 || (lpBuffer.Field_01D8 & 0x20) != 0 || (lpBuffer.Field_01D8 & 0x40) != 0 || (lpBuffer.Field_01D8 & 0x80) != 0;
					flag2 &= readable;
					flag3 &= writable;
					flag4 &= executable;
					flag &= flag2 | flag3 | flag4;
				}
				if (!flag)
				{
					uIntPtr3 = new UIntPtr(lpBuffer.Field_01D3.ToUInt64() + (ulong)lpBuffer.Field_01D6);
				}
				else
				{
					MemoryRegion item = new MemoryRegion
					{
						Property_0258 = uIntPtr3,
						Property_0259 = lpBuffer.Field_01D6,
						Property_025A = lpBuffer.Field_01D3
					};
					uIntPtr3 = new UIntPtr(lpBuffer.Field_01D3.ToUInt64() + (ulong)lpBuffer.Field_01D6);
					if (list.Count > 0)
					{
						MemoryRegion obj = list[list.Count - 1];
						if ((long)(ulong)obj.Property_025A + obj.Property_0259 == (long)(ulong)lpBuffer.Field_01D3)
						{
							list[list.Count - 1] = new MemoryRegion
							{
								Property_0258 = obj.Property_0258,
								Property_025A = obj.Property_025A,
								Property_0259 = obj.Property_0259 + lpBuffer.Field_01D6
							};
							continue;
						}
					}
					list.Add(item);
				}
			}
			ConcurrentBag<long> concurrentBag = new ConcurrentBag<long>();
			Parallel.ForEach(list, (MemoryRegion item2, ParallelLoopState parallelLoopState, long index) =>
			{
				long[] array4 = Method_0159(item2, array2, array3);
				foreach (long item3 in array4)
				{
					concurrentBag.Add(item3);
				}
			});
			return (from c in concurrentBag.ToList()
				orderby c
				select c).AsEnumerable();
		});
	}

	public async Task<long> Method_0158(string code, long end, string search, string file = "")
	{
		long start = (long)Method_0167(code, file).ToUInt64();
		return (await Method_0157(start, end, search, readable: true, writable: true, executable: true, mapped: false, file)).FirstOrDefault();
	}

	private unsafe long[] Method_0159(MemoryRegion item, byte[] aobPattern, byte[] mask)
	{
		if (mask.Length != aobPattern.Length)
		{
			throw new ArgumentException("aobPattern.Length != mask.Length");
		}
		IntPtr intPtr = Marshal.AllocHGlobal((int)item.Property_0259);
		NativeMethods.Method_0226(Field_0197.Property_0269, item.Property_0258, intPtr, (UIntPtr)(ulong)item.Property_0259, out var lpNumberOfBytesRead);
		int num = -aobPattern.Length;
		List<long> list = new List<long>();
		do
		{
			num = Method_015B((byte*)intPtr.ToPointer(), (int)lpNumberOfBytesRead, aobPattern, mask, num + aobPattern.Length);
			if (num >= 0)
			{
				list.Add((long)(ulong)item.Property_0258 + (long)num);
			}
		}
		while (num != -1);
		Marshal.FreeHGlobal(intPtr);
		return list.ToArray();
	}

	private int Method_015A(byte[] body, byte[] pattern, byte[] masks, int start = 0)
	{
		int result = -1;
		if (body.Length == 0 || pattern.Length == 0 || start > body.Length - pattern.Length || pattern.Length > body.Length)
		{
			return result;
		}
		for (int i = start; i <= body.Length - pattern.Length; i++)
		{
			if ((body[i] & masks[0]) != (pattern[0] & masks[0]))
			{
				continue;
			}
			bool flag = true;
			for (int j = 1; j <= pattern.Length - 1; j++)
			{
				if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private unsafe int Method_015B(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
	{
		int result = -1;
		if (bodyLength <= 0 || pattern.Length == 0 || start > bodyLength - pattern.Length || pattern.Length > bodyLength)
		{
			return result;
		}
		for (int i = start; i <= bodyLength - pattern.Length; i++)
		{
			if ((body[i] & masks[0]) != (pattern[0] & masks[0]))
			{
				continue;
			}
			bool flag = true;
			for (int num = pattern.Length - 1; num >= 1; num--)
			{
				if ((body[i + num] & masks[num]) != (pattern[num] & masks[num]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public UIntPtr Method_015C(IntPtr hProcess, UIntPtr lpAddress, out NativeMethods.Type_01D2 lpBuffer)
	{
		if (Field_0197.Property_026A || IntPtr.Size == 8)
		{
			NativeMethods.Type_01C8 lpBuffer2 = default;
			UIntPtr result = NativeMethods.Method_0215(hProcess, lpAddress, out lpBuffer2, new UIntPtr((uint)Marshal.SizeOf(lpBuffer2)));
			lpBuffer.Field_01D3 = lpBuffer2.Field_01C9;
			lpBuffer.Field_01D4 = lpBuffer2.Field_01CA;
			lpBuffer.Field_01D5 = lpBuffer2.Field_01CB;
			lpBuffer.Field_01D6 = (long)lpBuffer2.Field_01CD;
			lpBuffer.Field_01D7 = lpBuffer2.Field_01CE;
			lpBuffer.Field_01D8 = lpBuffer2.Field_01CF;
			lpBuffer.Field_01D9 = lpBuffer2.Field_01D0;
			return result;
		}
		NativeMethods.Type_01C0 lpBuffer3 = default;
		UIntPtr result2 = NativeMethods.Method_0214(hProcess, lpAddress, out lpBuffer3, new UIntPtr((uint)Marshal.SizeOf(lpBuffer3)));
		lpBuffer.Field_01D3 = lpBuffer3.Field_01C1;
		lpBuffer.Field_01D4 = lpBuffer3.Field_01C2;
		lpBuffer.Field_01D5 = lpBuffer3.Field_01C3;
		lpBuffer.Field_01D6 = lpBuffer3.Field_01C4;
		lpBuffer.Field_01D7 = lpBuffer3.Field_01C5;
		lpBuffer.Field_01D8 = lpBuffer3.Field_01C6;
		lpBuffer.Field_01D9 = lpBuffer3.Field_01C7;
		return result2;
	}

    [DllImport("kernel32.dll", EntryPoint="CloseHandle")]
    private static extern bool CloseProcessHandle(IntPtr handle);
    public bool Method_015D(int pid, out string FailReason)
    {
        FailReason = "";
        if (pid <= 0) { FailReason = "Geçersiz oyun işlem numarası."; return false; }
        if (Field_0197 == null) Field_0197 = new ProcessContext();
        IntPtr opened = IntPtr.Zero;
        try
        {
            if (Field_0197.Property_0269 != IntPtr.Zero && Field_0197.Property_0268 != null && !Field_0197.Property_0268.HasExited && Field_0197.Property_0268.Id == pid) return true;
            var process = Process.GetProcessById(pid);
            var module = process.MainModule;
            opened = NativeMethods.Method_0434(2035711u, bInheritHandle:false, pid);
            if (opened == IntPtr.Zero) { FailReason = "İşlem açılamadı (" + Marshal.GetLastWin32Error() + ")."; return false; }
            bool wow;
            bool is64 = Environment.Is64BitOperatingSystem && NativeMethods.Method_0467(opened, out wow) && !wow;
            var oldHandle = Field_0197.Property_0269;
            Field_0197 = new ProcessContext { Property_0268=process, Property_0269=opened, Property_026A=is64, Property_026B=module };
            opened = IntPtr.Zero;
            if (oldHandle != IntPtr.Zero) CloseProcessHandle(oldHandle);
            return true;
        }
        catch (Exception ex) { FailReason = ex.Message; return false; }
        finally { if (opened != IntPtr.Zero) CloseProcessHandle(opened); }
    }

	public bool Method_015E(string proc, out string FailReason)
	{
		return Method_015D(Method_0162(proc), out FailReason);
	}

	public bool Method_015F(string proc)
	{
		string FailReason;
		return Method_015D(Method_0162(proc), out FailReason);
	}

	public bool OpenProcess(int pid)
	{
		string FailReason;
		return Method_015D(pid, out FailReason);
	}

	public void Method_0161()
	{
		NativeMethods.Method_046A(Field_0197.Property_0268.MainWindowHandle);
	}

	public int Method_0162(string name)
	{
		Process[] processes = Process.GetProcesses();
		if (name.ToLower().Contains(".exe"))
		{
			name = name.Replace(".exe", "");
		}
		if (name.ToLower().Contains(".bin"))
		{
			name = name.Replace(".bin", "");
		}
		Process[] array = processes;
		foreach (Process process in array)
		{
			if (process.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return process.Id;
			}
		}
		return 0;
	}

	public string Method_0163(string name, string iniFile)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		if (!string.IsNullOrEmpty(iniFile))
		{
			if (File.Exists(iniFile))
			{
				NativeMethods.Method_044C("codes", name, "", stringBuilder, (uint)stringBuilder.Capacity, iniFile);
			}
		}
		else
		{
			stringBuilder.Append(name);
		}
		return stringBuilder.ToString();
	}

	private int Method_0164(string name, string path)
	{
		try
		{
			int num = Convert.ToInt32(Method_0163(name, path), 16);
			if (num >= 0)
			{
				return num;
			}
			return 0;
		}
		catch
		{
			return 0;
		}
	}

	public void Method_0165(string func, string name)
	{
		using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(name);
		if (!namedPipeClientStream.IsConnected)
		{
			namedPipeClientStream.Connect();
		}
		using StreamWriter streamWriter = new StreamWriter(namedPipeClientStream);
		if (!streamWriter.AutoFlush)
		{
			streamWriter.AutoFlush = true;
		}
		streamWriter.WriteLine(func);
	}

	public bool Method_0166(string code, NativeMethods.Type_01ED newProtection, out NativeMethods.Type_01ED oldProtection, string file = "")
	{
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || Field_0197.Property_0269 == IntPtr.Zero)
		{
			oldProtection = (NativeMethods.Type_01ED)0u;
			return false;
		}
		return NativeMethods.Method_0228(Field_0197.Property_0269, uIntPtr, (IntPtr)(Field_0197.Property_026A ? 8 : 4), newProtection, out oldProtection);
	}

	public UIntPtr Method_0167(string name, string path = "", int size = 8)
	{
		string text = "";
		if (Field_0197 == null)
		{
			return UIntPtr.Zero;
		}
		if (Field_0197.Property_026A)
		{
			if (size == 8)
			{
				size = 16;
			}
			return Method_0169(name, path, size);
		}
		text = (string.IsNullOrEmpty(path) ? name : Method_0163(name, path));
		if (string.IsNullOrEmpty(text))
		{
			return UIntPtr.Zero;
		}
		if (text.Contains(" "))
		{
			text = text.Replace(" ", string.Empty);
		}
		if (!text.Contains("+") && !text.Contains(","))
		{
			try
			{
				return new UIntPtr(Convert.ToUInt32(text, 16));
			}
			catch
			{
				Console.WriteLine("Error in GetCode(). Failed to read address " + text);
				return UIntPtr.Zero;
			}
		}
		string text2 = text;
		if (text.Contains("+"))
		{
			text2 = text.Substring(text.IndexOf('+') + 1);
		}
		byte[] array = new byte[size];
		if (text2.Contains(','))
		{
			List<int> list = new List<int>();
			string[] array2 = text2.Split(',');
			foreach (string text3 in array2)
			{
				string text4 = text3;
				if (text3.Contains("0x"))
				{
					text4 = text3.Replace("0x", "");
				}
				int num = 0;
				if (!text3.Contains("-"))
				{
					num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
				}
				else
				{
					text4 = text4.Replace("-", "");
					num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
					num *= -1;
				}
				list.Add(num);
			}
			int[] array3 = list.ToArray();
			if (text.Contains("base") || text.Contains("main"))
			{
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)((int)Field_0197.Property_026B.BaseAddress + array3[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
			{
				string[] array4 = text.Split('+');
				IntPtr intPtr = IntPtr.Zero;
				if (!array4[0].ToLower().Contains(".dll") && !array4[0].ToLower().Contains(".exe") && !array4[0].ToLower().Contains(".bin") && !array4[0].ToLower().Contains(".uac"))
				{
					string text5 = array4[0];
					if (text5.Contains("0x"))
					{
						text5 = text5.Replace("0x", "");
					}
					intPtr = (IntPtr)int.Parse(text5, NumberStyles.HexNumber);
				}
				else
				{
					try
					{
						intPtr = Method_0168(array4[0]);
					}
					catch
					{
						DebugGuard.Method_0008();
					}
				}
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)((int)intPtr + array3[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else
			{
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)array3[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			uint num2 = BitConverter.ToUInt32(array, 0);
			UIntPtr uIntPtr = (UIntPtr)0uL;
			for (int j = 1; j < array3.Length; j++)
			{
				uIntPtr = new UIntPtr(Convert.ToUInt32(num2 + array3[j]));
				NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
				num2 = BitConverter.ToUInt32(array, 0);
			}
			return uIntPtr;
		}
		int num3 = Convert.ToInt32(text2, 16);
		IntPtr intPtr2 = IntPtr.Zero;
		if (text.ToLower().Contains("base") || text.ToLower().Contains("main"))
		{
			intPtr2 = Field_0197.Property_026B.BaseAddress;
		}
		else if (!text.ToLower().Contains("base") && !text.ToLower().Contains("main") && text.Contains("+"))
		{
			string[] array5 = text.Split('+');
			if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin"))
			{
				string text6 = array5[0];
				if (text6.Contains("0x"))
				{
					text6 = text6.Replace("0x", "");
				}
				intPtr2 = (IntPtr)int.Parse(text6, NumberStyles.HexNumber);
			}
			else
			{
				try
				{
					intPtr2 = Method_0168(array5[0]);
				}
				catch
				{
					DebugGuard.Method_0008();
				}
			}
		}
		else
		{
			intPtr2 = Method_0168(text.Split('+')[0]);
		}
		return (UIntPtr)(ulong)((int)intPtr2 + num3);
	}

	public IntPtr Method_0168(string name)
	{
		return Field_0197.Property_0268.Modules.Cast<ProcessModule>().SingleOrDefault((ProcessModule m) => string.Equals(m.ModuleName, name, StringComparison.OrdinalIgnoreCase)).BaseAddress;
	}

	public UIntPtr Method_0169(string name, string path = "", int size = 16)
	{
		string text = "";
		text = (string.IsNullOrEmpty(path) ? name : Method_0163(name, path));
		if (string.IsNullOrEmpty(text))
		{
			return UIntPtr.Zero;
		}
		if (text.Contains(" "))
		{
			text.Replace(" ", string.Empty);
		}
		string text2 = text;
		if (text.Contains("+"))
		{
			text2 = text.Substring(text.IndexOf('+') + 1);
		}
		byte[] array = new byte[size];
		if (!text.Contains("+") && !text.Contains(","))
		{
			try
			{
				return new UIntPtr(Convert.ToUInt64(text, 16));
			}
			catch
			{
				Console.WriteLine("Error in GetCode(). Failed to read address " + text);
				return UIntPtr.Zero;
			}
		}
		if (text2.Contains(','))
		{
			List<long> list = new List<long>();
			string[] array2 = text2.Split(',');
			foreach (string text3 in array2)
			{
				string text4 = text3;
				if (text3.Contains("0x"))
				{
					text4 = text3.Replace("0x", "");
				}
				long num = 0L;
				if (!text3.Contains("-"))
				{
					num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
				}
				else
				{
					text4 = text4.Replace("-", "");
					num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
					num *= -1;
				}
				list.Add(num);
			}
			long[] array3 = list.ToArray();
			if (text.Contains("base") || text.Contains("main"))
			{
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)((long)Field_0197.Property_026B.BaseAddress + array3[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
			{
				string[] array4 = text.Split('+');
				IntPtr intPtr = IntPtr.Zero;
				if (!array4[0].ToLower().Contains(".dll") && !array4[0].ToLower().Contains(".exe") && !array4[0].ToLower().Contains(".bin"))
				{
					intPtr = (IntPtr)long.Parse(array4[0], NumberStyles.HexNumber);
				}
				else
				{
					try
					{
						intPtr = Method_0168(array4[0]);
					}
					catch
					{
						DebugGuard.Method_0008();
					}
				}
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)((long)intPtr + array3[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else
			{
				NativeMethods.Method_0224(Field_0197.Property_0269, (UIntPtr)(ulong)array3[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			long num2 = BitConverter.ToInt64(array, 0);
			UIntPtr uIntPtr = (UIntPtr)0uL;
			for (int j = 1; j < array3.Length; j++)
			{
				uIntPtr = new UIntPtr(Convert.ToUInt64(num2 + array3[j]));
				NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
				num2 = BitConverter.ToInt64(array, 0);
			}
			return uIntPtr;
		}
		long num3 = Convert.ToInt64(text2, 16);
		IntPtr intPtr2 = IntPtr.Zero;
		if (text.Contains("base") || text.Contains("main"))
		{
			intPtr2 = Field_0197.Property_026B.BaseAddress;
		}
		else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
		{
			string[] array5 = text.Split('+');
			if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin"))
			{
				string text5 = array5[0];
				if (text5.Contains("0x"))
				{
					text5 = text5.Replace("0x", "");
				}
				intPtr2 = (IntPtr)long.Parse(text5, NumberStyles.HexNumber);
			}
			else
			{
				try
				{
					intPtr2 = Method_0168(array5[0]);
				}
				catch
				{
					DebugGuard.Method_0008();
				}
			}
		}
		else
		{
			intPtr2 = Method_0168(text.Split('+')[0]);
		}
		return (UIntPtr)(ulong)((long)intPtr2 + num3);
	}

	public void Method_016A()
	{
		_ = Field_0197.Property_0269;
		NativeMethods.Method_045B(Field_0197.Property_0269);
		Field_0197 = new ProcessContext();
	}

	public unsafe bool Method_016B(string strDllName)
	{
		if (Field_0197.Property_0268 == null)
		{
			return false;
		}
		foreach (ProcessModule module in Field_0197.Property_0268.Modules)
		{
			if (module.ModuleName.StartsWith("inject", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
		}
		if (!Field_0197.Property_0268.Responding)
		{
			return false;
		}
		int num = strDllName.Length + 1;
		UIntPtr uIntPtr = NativeMethods.Method_0452(Field_0197.Property_0269, (UIntPtr)(void*)null, (uint)num, 12288u, 4u);
		NativeMethods.Method_021E(Field_0197.Property_0269, uIntPtr, strDllName, (UIntPtr)(ulong)num, out var lpNumberOfBytesWritten);
		UIntPtr lpStartAddress = NativeMethods.Method_0455(NativeMethods.Method_045E("kernel32.dll"), "LoadLibraryA");
		IntPtr intPtr = NativeMethods.Method_0464(Field_0197.Property_0269, (IntPtr)(void*)null, 0u, lpStartAddress, uIntPtr, 0u, out lpNumberOfBytesWritten);
		int num2 = NativeMethods.Method_0461(intPtr, 10000);
		if ((long)num2 == 128 || (long)num2 == 258)
		{
			NativeMethods.Method_045B(intPtr);
			return false;
		}
		NativeMethods.Method_044F(Field_0197.Property_0269, uIntPtr, (UIntPtr)0uL, 32768u);
		NativeMethods.Method_045B(intPtr);
		return true;
	}

	public UIntPtr Method_016C(string code, byte[] newBytes, int replaceCount, int size = 4096, string file = "")
	{
		if (replaceCount < 5)
		{
			return UIntPtr.Zero;
		}
		UIntPtr uIntPtr = Method_0167(code, file);
		UIntPtr uIntPtr2 = UIntPtr.Zero;
		UIntPtr uIntPtr3 = uIntPtr;
		for (int i = 0; i < 10; i++)
		{
			if (!(uIntPtr2 == UIntPtr.Zero))
			{
				break;
			}
			uIntPtr2 = NativeMethods.Method_0452(Field_0197.Property_0269, Method_016D(uIntPtr3, (uint)size), (uint)size, 12288u, 64u);
			if (uIntPtr2 == UIntPtr.Zero)
			{
				uIntPtr3 = UIntPtr.Add(uIntPtr3, 65536);
			}
		}
		if (uIntPtr2 == UIntPtr.Zero)
		{
			uIntPtr2 = NativeMethods.Method_0452(Field_0197.Property_0269, UIntPtr.Zero, (uint)size, 12288u, 64u);
		}
		int num = ((replaceCount > 5) ? (replaceCount - 5) : 0);
		int value = (int)((ulong)uIntPtr2 - (ulong)uIntPtr - 5);
		byte[] array = new byte[5 + num];
		array[0] = 233;
		BitConverter.GetBytes(value).CopyTo(array, 1);
		for (int j = 5; j < array.Length; j++)
		{
			array[j] = 144;
		}
		byte[] array2 = new byte[5 + newBytes.Length];
		int value2 = (int)((long)(ulong)uIntPtr + (long)array.Length - ((long)(ulong)uIntPtr2 + (long)newBytes.Length) - 5);
		newBytes.CopyTo(array2, 0);
		array2[newBytes.Length] = 233;
		BitConverter.GetBytes(value2).CopyTo(array2, newBytes.Length + 1);
		WriteBytes(uIntPtr2, array2);
		WriteBytes(uIntPtr, array);
		return uIntPtr2;
	}

	private UIntPtr Method_016D(UIntPtr baseAddress, uint size)
	{
		UIntPtr uIntPtr = UIntPtr.Subtract(baseAddress, 1879048192);
		UIntPtr uIntPtr2 = UIntPtr.Add(baseAddress, 1879048192);
		UIntPtr uIntPtr3 = UIntPtr.Zero;
		UIntPtr zero = UIntPtr.Zero;
		NativeMethods.Method_0217(out var lpSystemInfo);
		if (Field_0197.Property_026A)
		{
			if ((long)(ulong)uIntPtr > (long)(ulong)lpSystemInfo.Field_01B9 || (long)(ulong)uIntPtr < (long)(ulong)lpSystemInfo.Field_01B8)
			{
				uIntPtr = lpSystemInfo.Field_01B8;
			}
			if ((long)(ulong)uIntPtr2 < (long)(ulong)lpSystemInfo.Field_01B8 || (long)(ulong)uIntPtr2 > (long)(ulong)lpSystemInfo.Field_01B9)
			{
				uIntPtr2 = lpSystemInfo.Field_01B9;
			}
		}
		else
		{
			uIntPtr = lpSystemInfo.Field_01B8;
			uIntPtr2 = lpSystemInfo.Field_01B9;
		}
		UIntPtr uIntPtr4 = uIntPtr;
		UIntPtr uIntPtr5 = uIntPtr4;
		NativeMethods.Type_01D2 lpBuffer;
		while (Method_015C(Field_0197.Property_0269, uIntPtr4, out lpBuffer).ToUInt64() != 0L)
		{
			if ((long)(ulong)lpBuffer.Field_01D3 > (long)(ulong)uIntPtr2)
			{
				return UIntPtr.Zero;
			}
			if (lpBuffer.Field_01D7 == 65536 && lpBuffer.Field_01D6 > size)
			{
				if ((long)(ulong)lpBuffer.Field_01D3 % (long)lpSystemInfo.Field_01BD > 0)
				{
					zero = lpBuffer.Field_01D3;
					int num = (int)(lpSystemInfo.Field_01BD - (long)(ulong)zero % (long)lpSystemInfo.Field_01BD);
					if (lpBuffer.Field_01D6 - num >= size)
					{
						zero = UIntPtr.Add(zero, num);
						if ((long)(ulong)zero < (long)(ulong)baseAddress)
						{
							zero = UIntPtr.Add(zero, (int)(lpBuffer.Field_01D6 - num - size));
							if ((long)(ulong)zero > (long)(ulong)baseAddress)
							{
								zero = baseAddress;
							}
							zero = UIntPtr.Subtract(zero, (int)((long)(ulong)zero % (long)lpSystemInfo.Field_01BD));
						}
						if (Math.Abs((long)((ulong)zero - (ulong)baseAddress)) < Math.Abs((long)((ulong)uIntPtr3 - (ulong)baseAddress)))
						{
							uIntPtr3 = zero;
						}
					}
				}
				else
				{
					zero = lpBuffer.Field_01D3;
					if ((long)(ulong)zero < (long)(ulong)baseAddress)
					{
						zero = UIntPtr.Add(zero, (int)(lpBuffer.Field_01D6 - size));
						if ((long)(ulong)zero > (long)(ulong)baseAddress)
						{
							zero = baseAddress;
						}
						zero = UIntPtr.Subtract(zero, (int)((long)(ulong)zero % (long)lpSystemInfo.Field_01BD));
					}
					if (Math.Abs((long)((ulong)zero - (ulong)baseAddress)) < Math.Abs((long)((ulong)uIntPtr3 - (ulong)baseAddress)))
					{
						uIntPtr3 = zero;
					}
				}
			}
			if (lpBuffer.Field_01D6 % lpSystemInfo.Field_01BD > 0)
			{
				lpBuffer.Field_01D6 += lpSystemInfo.Field_01BD - lpBuffer.Field_01D6 % lpSystemInfo.Field_01BD;
			}
			uIntPtr5 = uIntPtr4;
			uIntPtr4 = new UIntPtr((ulong)lpBuffer.Field_01D3 + (ulong)lpBuffer.Field_01D6);
			if ((long)(ulong)uIntPtr4 >= (long)(ulong)uIntPtr2)
			{
				return uIntPtr3;
			}
			if ((long)(ulong)uIntPtr5 >= (long)(ulong)uIntPtr4)
			{
				return uIntPtr3;
			}
		}
		return uIntPtr3;
	}

	public static void Method_016E(int pid)
	{
		Process processById = Process.GetProcessById(pid);
		if (processById.ProcessName == string.Empty)
		{
			return;
		}
		foreach (ProcessThread thread in processById.Threads)
		{
			IntPtr intPtr = NativeMethods.Method_043A(NativeMethods.Type_01E3.Field_01E5, bInheritHandle: false, (uint)thread.Id);
			if (!(intPtr == IntPtr.Zero))
			{
				NativeMethods.Method_043D(intPtr);
				NativeMethods.Method_045B(intPtr);
			}
		}
	}

	public static void Method_016F(int pid)
	{
		Process processById = Process.GetProcessById(pid);
		if (processById.ProcessName == string.Empty)
		{
			return;
		}
		foreach (ProcessThread thread in processById.Threads)
		{
			IntPtr intPtr = NativeMethods.Method_043A(NativeMethods.Type_01E3.Field_01E5, bInheritHandle: false, (uint)thread.Id);
			if (!(intPtr == IntPtr.Zero))
			{
				int num = 0;
				do
				{
					num = NativeMethods.Method_0440(intPtr);
				}
				while (num > 0);
				NativeMethods.Method_045B(intPtr);
			}
		}
	}

	private async Task Method_0170(int delay)
	{
		await Task.Delay(delay);
	}

	private void Method_0171(string path, byte[] bytes)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Append);
		fileStream.Write(bytes, 0, bytes.Length);
	}

	public byte[] Method_0172(string path, bool dontDelete = false)
	{
		byte[] result = File.ReadAllBytes(path);
		if (!dontDelete)
		{
			File.Delete(path);
		}
		return result;
	}

	public string Method_0173()
	{
		if (Field_0197.Property_026A)
		{
			return "x16";
		}
		return "x8";
	}

	public static string Method_0174(byte[] ba)
	{
		StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
		int num = 1;
		foreach (byte b in ba)
		{
			if (num == 16)
			{
				stringBuilder.AppendFormat("{0:x2}{1}", b, Environment.NewLine);
				num = 0;
			}
			else
			{
				stringBuilder.AppendFormat("{0:x2} ", b);
			}
			num++;
		}
		return stringBuilder.ToString().ToUpper();
	}

	public static string Method_0175(byte[] ba)
	{
		StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
		foreach (byte b in ba)
		{
			stringBuilder.AppendFormat("{0:x2} ", b);
		}
		return stringBuilder.ToString();
	}

	public ulong Method_0176()
	{
		NativeMethods.Method_0217(out var lpSystemInfo);
		return (ulong)lpSystemInfo.Field_01B8;
	}

	public bool Method_0177(string file = "dump.dmp")
	{
		NativeMethods.Type_01B4 lpSystemInfo = default;
		NativeMethods.Method_0217(out lpSystemInfo);
		UIntPtr uIntPtr = lpSystemInfo.Field_01B8;
		long num = (long)(ulong)uIntPtr;
		long num2 = Field_0197.Property_0268.VirtualMemorySize64 + num;
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		NativeMethods.Type_01D2 lpBuffer = default;
		while (num < num2)
		{
			Method_015C(Field_0197.Property_0269, uIntPtr, out lpBuffer);
			byte[] array = new byte[lpBuffer.Field_01D6];
			UIntPtr nSize = (UIntPtr)(ulong)lpBuffer.Field_01D6;
			UIntPtr lpBaseAddress = (UIntPtr)(ulong)lpBuffer.Field_01D3;
			NativeMethods.Method_0224(Field_0197.Property_0269, lpBaseAddress, array, nSize, IntPtr.Zero);
			Method_0171(file, array);
			num += lpBuffer.Field_01D6;
			uIntPtr = new UIntPtr((ulong)num);
		}
		return true;
	}

	public void Method_0178()
	{
		if (Field_0197.Property_0268 == null)
		{
			return;
		}
		foreach (ProcessThread thread in Field_0197.Property_0268.Threads)
		{
			_ = thread;
		}
	}

	public static IntPtr Method_0179(int threadId)
	{
		IntPtr intPtr = NativeMethods.Method_043A(NativeMethods.Type_01E3.Field_01E9, bInheritHandle: false, (uint)threadId);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
		IntPtr intPtr2 = Marshal.AllocHGlobal(IntPtr.Size);
		try
		{
			int num = NativeMethods.Method_0470(intPtr, NativeMethods.Type_0210.Field_0211, intPtr2, IntPtr.Size, IntPtr.Zero);
			if (num != 0)
			{
				throw new Win32Exception(string.Format("NtQueryInformationThread failed; NTSTATUS = {0:X8}", num));
			}
			return Marshal.ReadIntPtr(intPtr2);
		}
		finally
		{
			NativeMethods.Method_045B(intPtr);
			Marshal.FreeHGlobal(intPtr2);
		}
	}

	public bool Method_017A(int ThreadID)
	{
		foreach (ProcessThread thread in Field_0197.Property_0268.Threads)
		{
			if (thread.Id == ThreadID)
			{
				IntPtr intPtr = NativeMethods.Method_043A(NativeMethods.Type_01E3.Field_01E5, bInheritHandle: false, (uint)ThreadID);
				if (intPtr == IntPtr.Zero)
				{
					break;
				}
				if (NativeMethods.Method_043D(intPtr) == -1)
				{
					NativeMethods.Method_045B(intPtr);
					break;
				}
				NativeMethods.Method_045B(intPtr);
				return true;
			}
		}
		return false;
	}

	public string Method_017B(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in str)
		{
			if (c < ' ' || c > '~')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public byte[] ReadBytes(string code, long length, string file = "")
	{
		byte[] array = new byte[length];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return null;
		}
		if (!NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)(ulong)length, IntPtr.Zero))
		{
			return null;
		}
		return array;
	}

	public float ReadFloat(string code, string file = "", bool round = true)
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0f;
		}
		try
		{
			if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)4uL, IntPtr.Zero))
			{
				float num = BitConverter.ToSingle(array, 0);
				float result = num;
				if (round)
				{
					result = (float)Math.Round(num, 4);
				}
				return result;
			}
			return 0f;
		}
		catch
		{
			return 0f;
		}
	}

	public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true, Encoding stringEncoding = null)
	{
		if (stringEncoding == null)
		{
			stringEncoding = Encoding.UTF8;
		}
		byte[] array = new byte[length];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return "";
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)(ulong)length, IntPtr.Zero))
		{
			if (!zeroTerminated)
			{
				return stringEncoding.GetString(array);
			}
			return stringEncoding.GetString(array).Split(default(char))[0];
		}
		return "";
	}

	public double ReadDouble(string code, string file = "", bool round = true)
	{
		byte[] array = new byte[8];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0.0;
		}
		try
		{
			if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)8uL, IntPtr.Zero))
			{
				double num = BitConverter.ToDouble(array, 0);
				double result = num;
				if (round)
				{
					result = Math.Round(num, 2);
				}
				return result;
			}
			return 0.0;
		}
		catch
		{
			return 0.0;
		}
	}

	public int Method_0180(UIntPtr code)
	{
		byte[] array = new byte[4];
		if (NativeMethods.Method_0224(Field_0197.Property_0269, code, array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public int ReadInt32(string code, string file = "")
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0;
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public long ReadInt64(string code, string file = "")
	{
		byte[] array = new byte[16];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0L;
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)8uL, IntPtr.Zero))
		{
			return BitConverter.ToInt64(array, 0);
		}
		return 0L;
	}

	public uint ReadUInt32(string code, string file = "")
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0u;
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return BitConverter.ToUInt32(array, 0);
		}
		return 0u;
	}

	public int Method_0184(string code, int moveQty, string file = "")
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0;
		}
		UIntPtr lpBaseAddress = UIntPtr.Add(uIntPtr, moveQty);
		if (NativeMethods.Method_0224(Field_0197.Property_0269, lpBaseAddress, array, (UIntPtr)2uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public int Method_0185(string code, int moveQty, string file = "")
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0;
		}
		UIntPtr lpBaseAddress = UIntPtr.Add(uIntPtr, moveQty);
		if (NativeMethods.Method_0224(Field_0197.Property_0269, lpBaseAddress, array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public ulong Method_0186(string code, int moveQty, string file = "")
	{
		byte[] array = new byte[8];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0uL;
		}
		UIntPtr lpBaseAddress = UIntPtr.Add(uIntPtr, moveQty);
		if (NativeMethods.Method_0224(Field_0197.Property_0269, lpBaseAddress, array, (UIntPtr)8uL, IntPtr.Zero))
		{
			return BitConverter.ToUInt64(array, 0);
		}
		return 0uL;
	}

	public int Method_0187(string code, string file = "")
	{
		byte[] array = new byte[4];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0;
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)2uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public int Method_0188(string code, string file = "")
	{
		byte[] array = new byte[1];
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return 0;
		}
		if (NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)1uL, IntPtr.Zero))
		{
			return array[0];
		}
		return 0;
	}

	public bool[] Method_0189(string code, string file = "")
	{
		byte[] array = new byte[1];
		UIntPtr uIntPtr = Method_0167(code, file);
		bool[] array2 = new bool[8];
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return array2;
		}
		if (!NativeMethods.Method_0224(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)1uL, IntPtr.Zero))
		{
			return array2;
		}
		if (!BitConverter.IsLittleEndian)
		{
			throw new Exception("Should be little endian");
		}
		for (int i = 0; i < 8; i++)
		{
			array2[i] = Convert.ToBoolean(array[0] & (1 << i));
		}
		return array2;
	}

	public int Method_018A(UIntPtr address, string code, string file = "")
	{
		byte[] array = new byte[4];
		if (NativeMethods.Method_0224(Field_0197.Property_0269, address + Method_0164(code, file), array, (UIntPtr)1uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public float Method_018B(UIntPtr address, string code, string file = "")
	{
		byte[] array = new byte[4];
		if (NativeMethods.Method_0224(Field_0197.Property_0269, address + Method_0164(code, file), array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return (float)Math.Round(BitConverter.ToSingle(array, 0), 2);
		}
		return 0f;
	}

	public int Method_018C(UIntPtr address, string code, string file = "")
	{
		byte[] array = new byte[4];
		if (NativeMethods.Method_0224(Field_0197.Property_0269, address + Method_0164(code, file), array, (UIntPtr)4uL, IntPtr.Zero))
		{
			return BitConverter.ToInt32(array, 0);
		}
		return 0;
	}

	public string Method_018D(UIntPtr address, string code, string file = "")
	{
		byte[] array = new byte[32];
		if (NativeMethods.Method_0224(Field_0197.Property_0269, address + Method_0164(code, file), array, (UIntPtr)32uL, IntPtr.Zero))
		{
			return Method_017B(Encoding.ASCII.GetString(array));
		}
		return "";
	}

	public T Method_018E<T>(string address, string file = "")
	{
		object obj = null;
		switch (Type.GetTypeCode(typeof(T)))
		{
		case TypeCode.String:
			obj = ReadString(address, file);
			break;
		case TypeCode.Int32:
			obj = ReadInt32(address, file);
			break;
		case TypeCode.Int64:
			obj = ReadInt64(address, file);
			break;
		case TypeCode.Byte:
			obj = Method_0188(address, file);
			break;
		case TypeCode.Double:
			obj = ReadDouble(address, file);
			break;
		case TypeCode.Decimal:
			obj = ReadFloat(address, file);
			break;
		case TypeCode.UInt32:
			obj = ReadUInt32(address, file);
			break;
		}
		if (obj != null)
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}
		return default;
	}

	public void Method_018F(string address, Action<string> UIObject, string file = "")
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		if (Field_0198.ContainsKey(address))
		{
			try
			{
				Field_0198[address].Cancel();
				Field_0198.TryRemove(address, out var _);
			}
			catch
			{
			}
		}
		Field_0198.TryAdd(address, cancellationTokenSource);
		Task.Factory.StartNew(() =>
		{
			while (!cancellationTokenSource.Token.IsCancellationRequested)
			{
				UIObject(Method_018E<string>(address, file));
				Thread.Sleep(100);
			}
		}, cancellationTokenSource.Token);
	}

	public bool Method_0190(string address, string type, string value, string file = "")
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		if (Field_0199.ContainsKey(address))
		{
			try
			{
				Field_0199[address].Cancel();
				Field_0199.TryRemove(address, out var _);
			}
			catch
			{
				return false;
			}
		}
		Field_0199.TryAdd(address, cancellationTokenSource);
		Task.Factory.StartNew(() =>
		{
			while (!cancellationTokenSource.Token.IsCancellationRequested)
			{
				WriteValue(address, type, value, file);
				Thread.Sleep(25);
			}
		}, cancellationTokenSource.Token);
		return true;
	}

	public void Method_0191(string address)
	{
		try
		{
			lock (Field_0199)
			{
				Field_0199[address].Cancel();
				Field_0199.TryRemove(address, out var _);
			}
		}
		catch
		{
		}
	}

	public bool WriteValue(string code, string type, string write, string file = "", Encoding stringEncoding = null, bool RemoveWriteProtection = true)
	{
		byte[] array = new byte[4];
		int num = 4;
		UIntPtr uIntPtr = Method_0167(code, file);
		if (uIntPtr == UIntPtr.Zero || uIntPtr.ToUInt64() < 65536)
		{
			return false;
		}
		if (type.ToLower() == "float")
		{
			float num2 = float.Parse(write, CultureInfo.InvariantCulture);
			write = Convert.ToString(num2, CultureInfo.InvariantCulture);
			array = BitConverter.GetBytes(num2);
			num = 4;
		}
		else if (type.ToLower() == "int")
		{
			array = BitConverter.GetBytes(Convert.ToInt32(write));
			num = 4;
		}
		else if (type.ToLower() == "byte")
		{
			array = new byte[1] { Convert.ToByte(write, 16) };
			num = 1;
		}
		else if (type.ToLower() == "2bytes")
		{
			array = new byte[2]
			{
				(byte)(Convert.ToInt32(write) % 256),
				(byte)(Convert.ToInt32(write) / 256)
			};
			num = 2;
		}
		else if (type.ToLower() == "bytes")
		{
			if (write.Contains(",") || write.Contains(" "))
			{
				string[] array2 = ((!write.Contains(",")) ? write.Split(' ') : write.Split(','));
				int num3 = array2.Count();
				array = new byte[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = Convert.ToByte(array2[i], 16);
				}
				num = array2.Count();
			}
			else
			{
				array = new byte[1] { Convert.ToByte(write, 16) };
				num = 1;
			}
		}
		else if (type.ToLower() == "double")
		{
			array = BitConverter.GetBytes(Convert.ToDouble(write));
			num = 8;
		}
		else if (type.ToLower() == "long")
		{
			array = BitConverter.GetBytes(Convert.ToInt64(write));
			num = 8;
		}
		else if (type.ToLower() == "string")
		{
			array = ((stringEncoding != null) ? stringEncoding.GetBytes(write) : Encoding.UTF8.GetBytes(write));
			num = array.Length;
		}
		NativeMethods.Type_01ED oldProtection = (NativeMethods.Type_01ED)0u;
		if (RemoveWriteProtection)
		{
			Method_0166(code, NativeMethods.Type_01ED.Field_01F0, out oldProtection, file);
		}
		bool result = NativeMethods.Method_022E(Field_0197.Property_0269, uIntPtr, array, (UIntPtr)(ulong)num, IntPtr.Zero);
		if (RemoveWriteProtection)
		{
			Method_0166(code, oldProtection, out var _, file);
		}
		return result;
	}

	public bool Method_0193(string code, string type, string write, int MoveQty, string file = "", int SlowDown = 0)
	{
		byte[] lpBuffer = new byte[4];
		int num = 4;
		UIntPtr pointer = Method_0167(code, file);
		if (type == "float")
		{
			lpBuffer = new byte[write.Length];
			lpBuffer = BitConverter.GetBytes(Convert.ToSingle(write));
			num = write.Length;
		}
		else if (type == "int")
		{
			lpBuffer = BitConverter.GetBytes(Convert.ToInt32(write));
			num = 4;
		}
		else if (type == "double")
		{
			lpBuffer = BitConverter.GetBytes(Convert.ToDouble(write));
			num = 8;
		}
		else if (type == "long")
		{
			lpBuffer = BitConverter.GetBytes(Convert.ToInt64(write));
			num = 8;
		}
		else if (type == "byte")
		{
			lpBuffer = new byte[1] { Convert.ToByte(write, 16) };
			num = 1;
		}
		else if (type == "string")
		{
			lpBuffer = new byte[write.Length];
			lpBuffer = Encoding.UTF8.GetBytes(write);
			num = write.Length;
		}
		UIntPtr lpBaseAddress = UIntPtr.Add(pointer, MoveQty);
		Thread.Sleep(SlowDown);
		return NativeMethods.Method_022E(Field_0197.Property_0269, lpBaseAddress, lpBuffer, (UIntPtr)(ulong)num, IntPtr.Zero);
	}

	public void Method_0194(string code, byte[] write, string file = "")
	{
		UIntPtr lpBaseAddress = Method_0167(code, file);
		NativeMethods.Method_022E(Field_0197.Property_0269, lpBaseAddress, write, (UIntPtr)(ulong)write.Length, IntPtr.Zero);
	}

	public void Method_0195(string code, bool[] bits, string file = "")
	{
		if (bits.Length != 8)
		{
			throw new ArgumentException("Not enough bits for a whole byte", "bits");
		}
		byte[] array = new byte[1];
		UIntPtr lpBaseAddress = Method_0167(code, file);
		for (int i = 0; i < 8; i++)
		{
			if (bits[i])
			{
				array[0] |= (byte)(1 << i);
			}
		}
		NativeMethods.Method_022E(Field_0197.Property_0269, lpBaseAddress, array, (UIntPtr)1uL, IntPtr.Zero);
	}

	public void WriteBytes(UIntPtr address, byte[] write)
	{
		NativeMethods.Method_022F(Field_0197.Property_0269, address, write, (UIntPtr)(ulong)write.Length, out var _);
	}
}
