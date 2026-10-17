using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;

internal static class StringDecoder
{
	internal sealed class Type_0096
	{
		internal byte[] Field_008E;

		internal byte[] Field_008F;

		internal byte Field_0090;

		internal byte Field_0091;

		internal int Field_0092;

		internal int Field_0093;

		internal int Field_0094;

		internal int Field_0095;
	}

	internal static class Type_009D
	{
		internal static Type_0096 Field_009C;

		internal static string Method_0097(byte[] data, int index)
		{
			IntegrityGuard.Method_0035();
			if (Field_009C == null)
			{
				throw new InvalidOperationException();
			}
			byte[] array = Method_0098(Field_009C.Field_0092, Field_009C.Field_0093, index, data.Length);
			byte[] array2 = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				array2[i] = (byte)(data[i] ^ array[i]);
			}
			return Encoding.UTF8.GetString(array2);
		}

		private static byte[] Method_0098(int maskPartA, int maskPartB, int index, int length)
		{
			if (length <= 0)
			{
				return Array.Empty<byte>();
			}
			uint num = (uint)((maskPartA * -1640531535) ^ (maskPartB + 2135587861) ^ (index * -2048144777 + length * 668265263 + -1028477379));
			if (num == 0)
			{
				num = 2779096485u;
			}
			byte[] array = new byte[length];
			for (int i = 0; i < length; i++)
			{
				num ^= num << 13;
				num ^= num >> 17;
				num ^= num << 5;
				array[i] = (byte)((num ^ (num >> 24)) + (uint)(i * 31));
			}
			return array;
		}

		private static byte[] Method_0099(byte[] src, int key)
		{
			uint num = (uint)key;
			if (num == 0)
			{
				num = 2654435769u;
			}
			byte[] array = new byte[src.Length];
			for (int i = 0; i < src.Length; i++)
			{
				num ^= num << 13;
				num ^= num >> 17;
				num ^= num << 5;
				array[i] = (byte)(src[i] ^ (byte)((int)(num ^ (num >> 24)) + i * 31));
			}
			return array;
		}

		internal static int Method_009A(int p0, int p1, int p2, int p3)
		{
			int num = p3 & 0x1F;
			int num2 = (p0 << num) | (p0 >>> 32 - num);
			if (num == 0)
			{
				num2 = p0;
			}
			return num2 ^ (p1 + -1640531527) ^ (p2 * p3);
		}

		internal static Type_0096 Method_009B(byte[] packed, int transportKey)
		{
			if (packed == null || packed.Length < 4)
			{
				throw new InvalidOperationException();
			}
			byte[] array = Method_0099(packed, transportKey);
			int num = 0;
			int num2 = array[num] | (array[num + 1] << 8) | (array[num + 2] << 16) | (array[num + 3] << 24);
			num += 4;
			if (num2 < 0 || num + num2 > array.Length)
			{
				throw new InvalidOperationException();
			}
			byte[] array2 = new byte[num2];
			Array.Copy(array, num, array2, 0, num2);
			num += num2;
			if (num + 4 > array.Length)
			{
				throw new InvalidOperationException();
			}
			int num3 = array[num] | (array[num + 1] << 8) | (array[num + 2] << 16) | (array[num + 3] << 24);
			num += 4;
			if (num3 < 0 || num + num3 + 18 > array.Length)
			{
				throw new InvalidOperationException();
			}
			byte[] array3 = new byte[num3];
			Array.Copy(array, num, array3, 0, num3);
			num += num3;
			return new Type_0096
			{
				Field_008E = array2,
				Field_008F = array3,
				Field_0090 = array[num++],
				Field_0091 = array[num++],
				Field_0092 = (array[num] | (array[num + 1] << 8) | (array[num + 2] << 16) | (array[num + 3] << 24)),
				Field_0093 = (array[num + 4] | (array[num + 5] << 8) | (array[num + 6] << 16) | (array[num + 7] << 24)),
				Field_0094 = (array[num + 8] | (array[num + 9] << 8) | (array[num + 10] << 16) | (array[num + 11] << 24)),
				Field_0095 = (array[num + 12] | (array[num + 13] << 8) | (array[num + 14] << 16) | (array[num + 15] << 24))
			};
		}

		static Type_009D()
		{
			Assembly assembly = typeof(Type_009D).Assembly;
			int num = 0x4928CE1B ^ 0x721DC02E;
			char[] array = new char[8];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x2E39 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xDFE ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0x7EB8 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA330 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xDA51 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x6D69 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xEFC0 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x2A97 ^ num2);
			Stream manifestResourceStream = assembly.GetManifestResourceStream(new string(array));
			byte[] array2 = new byte[(int)manifestResourceStream.Length];
			int num3 = 0;
			do
			{
				int num4 = manifestResourceStream.Read(array2, num3, array2.Length - num3);
				if (num4 <= 0)
				{
					break;
				}
				num3 += num4;
			}
			while (num3 < array2.Length);
			_ = 702739108;
			_ = 25505170;
			_ = 14490775;
			Field_009C = Method_009B(array2, Method_009A(-1716745455, 1784665300, 1396509152, 850111503));
		}
	}

	private const int Field_0085 = -1640531535;

	private static int Field_0086;

	private static byte[] Field_0087;

	private static int Field_0088;

	private static int Field_0089;

	private static int Field_008A;

	private static byte[] Field_008B;

	private static readonly ConcurrentDictionary<string, string> Field_008C = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

	private static readonly ConcurrentDictionary<int, byte[]> Field_008D = new ConcurrentDictionary<int, byte[]>();

	public static string Method_007B(string payload, int index)
	{
		if (payload == null)
		{
			FailureHandlers.Method_0069(string.Empty);
			FailureHandlers.Method_006F();
			return string.Empty;
		}
		if (index == int.MinValue && payload.Length == int.MaxValue)
		{
			FailureHandlers.Method_006A(payload);
		}
		string key = payload + "\0" + index;
		if (Field_008C.TryGetValue(key, out var value))
		{
			return value;
		}
		IntegrityGuard.Method_0035();
		Method_007C();
		string value2 = Method_007E(payload, Method_0081(index));
		return Field_008C.GetOrAdd(key, value2);
	}

	private static void Method_007C()
	{
		if (Field_0086 == 0)
		{
			Type_0096 obj = Type_009D.Field_009C;
			if (obj == null)
			{
				FailureHandlers.Method_0069(string.Empty);
				FailureHandlers.Method_006F();
				return;
			}
			Field_0088 = obj.Field_0092 ^ obj.Field_0093;
			Field_0089 = obj.Field_0094;
			Field_008A = obj.Field_0095;
			Field_0087 = Method_007D(obj.Field_008E, obj.Field_0090);
			Field_008B = Method_007D(obj.Field_008F, obj.Field_0091);
			Field_0086 = 1;
		}
	}

	private static byte[] Method_007D(byte[] src, byte key)
	{
		byte[] array = new byte[src.Length];
		for (int i = 0; i < src.Length; i++)
		{
			array[i] = (byte)(src[i] ^ key ^ (byte)(i * 31));
		}
		return array;
	}

	internal static string Method_007E(string payload, int seed)
	{
		if (string.IsNullOrEmpty(payload))
		{
			return payload;
		}
		byte[] cipher = CryptoPrimitives.Method_0021(payload);
		byte[] key = Method_0083(seed, 32);
		byte[] iv = Method_0083(Method_007F(seed), 16);
		return Method_0080(CryptoPrimitives.Method_0020(cipher, key, iv));
	}

	private static int Method_007F(int seed)
	{
		return seed ^ Field_0089 ^ Field_008A;
	}

	private static string Method_0080(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	private static int Method_0081(int index)
	{
		if (Field_0087 == null || index < 0 || (index + 1) * 4 > Field_0087.Length)
		{
			FailureHandlers.Method_0069(string.Empty);
			FailureHandlers.Method_006F();
			return 0;
		}
		int num = index * 4;
		return (Field_0087[num] | (Field_0087[num + 1] << 8) | (Field_0087[num + 2] << 16) | (Field_0087[num + 3] << 24)) ^ Field_0088 ^ (index * -1640531535);
	}

	private static string Method_0082()
	{
		if (Field_008B == null || Field_008B.Length == 0)
		{
			FailureHandlers.Method_0069(string.Empty);
			FailureHandlers.Method_006F();
			return string.Empty;
		}
		char[] array = new char[Field_008B.Length];
		for (int i = 0; i < Field_008B.Length; i++)
		{
			array[i] = (char)(Field_008B[i] ^ (byte)(Field_0088 >> (i & 3) * 8));
		}
		return new string(array);
	}

	private static byte[] Method_0083(int seed, int length)
	{
		int key = (seed * 397) ^ length;
		if (Field_008D.TryGetValue(key, out var value) && value.Length == length)
		{
			return value;
		}
		byte[] array = CryptoPrimitives.Method_001E(Encoding.UTF8.GetBytes(Method_0082() + Method_0084(seed)));
		byte[] array2 = new byte[length];
		Array.Copy(array, array2, Math.Min(array.Length, length));
		return Field_008D.GetOrAdd(key, array2);
	}

	private static string Method_0084(int value)
	{
		char[] array = new char[8];
		for (int num = 7; num >= 0; num--)
		{
			int num2 = value & 0xF;
			array[num] = (char)((num2 < 10) ? (48 + num2) : (65 + (num2 - 10)));
			value >>= 4;
		}
		return new string(array);
	}
}
