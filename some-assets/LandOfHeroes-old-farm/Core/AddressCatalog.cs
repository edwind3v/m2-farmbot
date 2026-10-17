using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class AddressCatalog
{
	public const long Field_034D = 416L;

	public const long Field_034E = 33144L;

	public const long Field_034F = 24L;

	public const long Field_0350 = 24L;

	public const long Field_0351 = 2880L;

	public const long Field_0352 = 2412L;

	public const long Field_0353 = 2492L;

	public const long Field_0354 = 2528L;

	public const long Field_0355 = 2532L;

	public const long Field_0356 = 2536L;

	public const long Field_0357 = 2904L;

	public const long Field_0358 = 33188L;

	public const long Field_0359 = 33192L;

	public static int Field_035A = 6;

	public static int Field_035B = 2;

	public static int Field_035C = 0;

	public const string Field_035D = "DHzlQcjoS810PXbCVQp6C9eQt8POlj1TQt8Tj9HZ3zrdGtmJ";

	public const string Field_035E = "ByUJrF9Goj3UNUCuGikhaIfYIPDPRBK/VGMyPsnKv2q/FzLi";

	public const string Field_035F = "w8WjJEu4T8AbCu0nZ3/GIQfa6ZxkefSUE";

	public const string Field_0360 = "geUrm2QYTeSrLIBBDmO0Eagd8pvTJa0qrXhPg8yBuu0xMuvR";

	public const string Field_0361 = "wjb4Dg/G+3u9G9O5qoAzmyhGzMFWluQO5eim+7mKYbQoN17x";

	public const string Field_0362 = "g6Hku7q+BUE3bpgQngDnao6uC1aLh559RT1qnsg90kkq3vvv";

	public const string Field_0363 = "6AAtyTX3AYh5hfcIbGlIz2p7AFz1DQ3TFqRh4LL98+qOKmcU";

	public const string Field_0364 = "IcOwJn8EXFssghzvgNz2O7QP7nL0LawZpetnCHJetMKyeeda";

	public const string Field_0365 = "K7u8zlGML+1hbI48UTDB0MtZEUIBxdwcKxqQs0lDXgbDRoCl";

	public const string Field_0366 = "TNeLabl+OU4ecFbuOYXqdoQgt/pUwpIbRjtk+lCvLsCY7HnS";

	public const string Field_0367 = "Ft15RYIr8+daxRNkLcM5X2voWnexm4kmYXP";

	public static long Field_0368;

	public static long Field_0369;

	public static long Field_036A;

	public static long Field_036B;

	public static long Field_036C;

	public static long Field_036D;

	public static long Field_036E;

	public static long Field_036F;

	public static string Field_0370 = "0x0";

	public static string Field_0371 = "0x0,";

	public static string Field_0372 = "0x0";

	public static string Field_0373 = ",28";

	public static string Field_0374 = ",9E0";

	public static string Field_0375 = ",9E4";

	public static string Field_0376 = ",9E8";

	public static string Field_0377 = ",B40";

	public static string Field_0378 = ",96C";

	public static string Field_0379 = ",9BC";

	public static string Property_037A => Field_0370 + ",28";

	public static string Property_037B => Property_037A + ",9E0";

	public static string Property_037C => Property_037A + ",9E4";

	public static string Property_037D => Property_037A + ",9E8";

	public static string Property_037E => Property_037A + ",B48";

	public static string Property_037F => Field_0370 + ",8178";

	public static string Property_0380 => Field_0370 + ",81A4";

	public static string Property_0381 => Field_0370 + ",81A8";

	public static string Property_0382 => Field_0372 + ",18,134";

	public static string Property_0383 => Field_0370 + ",28,B50";

	public static string Property_0384 => Field_0370 + ",28,50,700";

	public static string Property_0385
	{
		get
		{
			return Field_0370;
		}
		set
		{
			Field_0370 = value;
		}
	}

	public static string Property_0386
	{
		get
		{
			return Field_0371;
		}
		set
		{
			Field_0371 = value;
		}
	}

	public static string Property_0387
	{
		get
		{
			return Field_0372;
		}
		set
		{
			Field_0372 = value;
		}
	}

	public static void ResetAddresses()
	{
		Field_0368 = (Field_0369 = (Field_036A = (Field_036B = (Field_036C = (Field_036D = 0L)))));
		Field_036E = (Field_036F = 0L);
		Field_0370 = "0x0";
		Field_0371 = "0x0,";
		Field_0372 = "0x0";
	}

	private static async Task<long?> FindFirstPattern(ProcessMemory mem, string aob)
	{
		IEnumerable<long> enumerable = await mem.Method_0154(aob);
		if (enumerable == null || !enumerable.Any())
		{
			return null;
		}
		return enumerable.First();
	}

	private static long ResolveRelativeAddress(ProcessMemory mem, long instr, int off = 3)
	{
		long num = instr + off;
		return num + 4 + mem.ReadInt32(string.Format("0x{0:X}", num));
	}

	public static async Task<string> ResolveAddressesAsync(ProcessMemory mem)
	{
		ResetAddresses();
		try
		{
			Field_0368 = mem.Field_0197.Property_0268.MainModule.BaseAddress.ToInt64();
		}
		catch
		{
			Field_0368 = 0L;
		}
		// The signatures and structure paths below describe the 64-bit client family.
		// Never let a coincidental byte sequence in an x86 client become a live write target.
		// x86 executable/Python/inventory discovery is handled by ClientProfile and
		// QueuedPythonBridge; core movement addresses remain disabled until validated.
		if (mem == null || mem.Field_0197 == null || !mem.Field_0197.Property_026A)
			return $"mod={Field_0368:X} architecture=x86 core=disabled";
		long? num = await FindFirstPattern(mem, "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 90 48 83 C4");
		if (num.HasValue)
		{
			Field_0370 = $"0x{ResolveRelativeAddress(mem, num.Value):X}";
		}
		long? num2 = await FindFirstPattern(mem, "48 8B 1D ?? ?? ?? ?? 48 3B D8");
		if (num2.HasValue)
		{
			Field_0371 = $"0x{ResolveRelativeAddress(mem, num2.Value):X},";
		}
		long? num3 = await FindFirstPattern(mem, "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 48 8B 4D");
		if (num3.HasValue)
		{
			Field_0372 = $"0x{ResolveRelativeAddress(mem, num3.Value):X}";
		}
		Field_0369 = (await FindFirstPattern(mem, "40 53 56 57 48 81 EC 90 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 80 00 00 00 49 8B F0 8B DA 48 8B F9")).GetValueOrDefault();
		Field_036A = (await FindFirstPattern(mem, "40 53 55 56 48 83 EC 70 0F 29 74 24 50 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 38 41 8B D9 0F 28 F2 48 8B EA 48 8B F1")).GetValueOrDefault();
		Field_036B = (await FindFirstPattern(mem, "40 55 53 57 48 8D 6C 24 B9 48 81 EC 90 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 3F 48 8B D9 C6 45 27 33")).GetValueOrDefault();
		Field_036C = (await FindFirstPattern(mem, "48 8B C4 48 89 58 08 57 48 83 EC 70 48 8B D9 C6 40 10 36")).GetValueOrDefault();
		Field_036D = (await FindFirstPattern(mem, "48 89 5C 24 10 48 89 74 24 18 57 48 83 EC 60 48 8B F1 48 8B 41 08 48 83 C1 08 FF 90 10 01 00 00")).GetValueOrDefault();
		long? num4 = await FindFirstPattern(mem, "48 8B 0D ?? ?? ?? ?? 4C 8B C5 41 8B D6 E8 ?? ?? ?? ??");
		if (num4.HasValue)
		{
			Field_036E = ResolveRelativeAddress(mem, num4.Value);
		}
		long? num5 = await FindFirstPattern(mem, "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 83 C4 ?? C3 CC CC CC 48 83 EC ?? 48 8B 0D ?? ?? ?? ?? 48 83 C1 ?? 48 8B 01 FF 90");
		if (num5.HasValue)
		{
			long num6 = ResolveRelativeAddress(mem, num5.Value);
			if (num6 != 0L && num6 != Field_036E)
			{
				Field_036F = num6;
			}
			else
			{
				long num7 = ResolveRelativeAddress(mem, num5.Value + 29);
				if (num7 != 0L && num7 != Field_036E)
				{
					Field_036F = num7;
				}
			}
		}
		if (Field_036F == 0L && Field_036D != 0L)
		{
			IEnumerable<long> enumerable = await mem.Method_0154("48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ??");
			if (enumerable != null)
			{
				foreach (long item in enumerable)
				{
					int num8 = mem.ReadInt32($"0x{item + 8:X}");
					if (item + 7 + 5 + num8 == Field_036D)
					{
						long num9 = ResolveRelativeAddress(mem, item);
						if (num9 != 0L && num9 != Field_036E)
						{
							Field_036F = num9;
							break;
						}
					}
				}
			}
		}
		return $"mod={Field_0368:X} cm={Field_0370} mob={Field_0371} cam={Field_0372}\n" + $"battle={Field_0369:X} state={Field_036A:X} fly={Field_036B:X} shoot={Field_036C:X} pick={Field_036D:X}\n" + $"netLoc={Field_036E:X} pyLoc={Field_036F:X}";
	}
}
