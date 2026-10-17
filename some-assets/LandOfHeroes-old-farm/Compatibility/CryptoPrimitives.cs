using System;
using System.Text;

internal static class CryptoPrimitives
{
	private static readonly byte[] Field_0032 = new byte[256]
	{
		99, 124, 119, 123, 242, 107, 111, 197, 48, 1,
		103, 43, 254, 215, 171, 118, 202, 130, 201, 125,
		250, 89, 71, 240, 173, 212, 162, 175, 156, 164,
		114, 192, 183, 253, 147, 38, 54, 63, 247, 204,
		52, 165, 229, 241, 113, 216, 49, 21, 4, 199,
		35, 195, 24, 150, 5, 154, 7, 18, 128, 226,
		235, 39, 178, 117, 9, 131, 44, 26, 27, 110,
		90, 160, 82, 59, 214, 179, 41, 227, 47, 132,
		83, 209, 0, 237, 32, 252, 177, 91, 106, 203,
		190, 57, 74, 76, 88, 207, 208, 239, 170, 251,
		67, 77, 51, 133, 69, 249, 2, 127, 80, 60,
		159, 168, 81, 163, 64, 143, 146, 157, 56, 245,
		188, 182, 218, 33, 16, 255, 243, 210, 205, 12,
		19, 236, 95, 151, 68, 23, 196, 167, 126, 61,
		100, 93, 25, 115, 96, 129, 79, 220, 34, 42,
		144, 136, 70, 238, 184, 20, 222, 94, 11, 219,
		224, 50, 58, 10, 73, 6, 36, 92, 194, 211,
		172, 98, 145, 149, 228, 121, 231, 200, 55, 109,
		141, 213, 78, 169, 108, 86, 244, 234, 101, 122,
		174, 8, 186, 120, 37, 46, 28, 166, 180, 198,
		232, 221, 116, 31, 75, 189, 139, 138, 112, 62,
		181, 102, 72, 3, 246, 14, 97, 53, 87, 185,
		134, 193, 29, 158, 225, 248, 152, 17, 105, 217,
		142, 148, 155, 30, 135, 233, 206, 85, 40, 223,
		140, 161, 137, 13, 191, 230, 66, 104, 65, 153,
		45, 15, 176, 84, 187, 22
	};

	private static readonly byte[] Field_0033 = new byte[256]
	{
		82, 9, 106, 213, 48, 54, 165, 56, 191, 64,
		163, 158, 129, 243, 215, 251, 124, 227, 57, 130,
		155, 47, 255, 135, 52, 142, 67, 68, 196, 222,
		233, 203, 84, 123, 148, 50, 166, 194, 35, 61,
		238, 76, 149, 11, 66, 250, 195, 78, 8, 46,
		161, 102, 40, 217, 36, 178, 118, 91, 162, 73,
		109, 139, 209, 37, 114, 248, 246, 100, 134, 104,
		152, 22, 212, 164, 92, 204, 93, 101, 182, 146,
		108, 112, 72, 80, 253, 237, 185, 218, 94, 21,
		70, 87, 167, 141, 157, 132, 144, 216, 171, 0,
		140, 188, 211, 10, 247, 228, 88, 5, 184, 179,
		69, 6, 208, 44, 30, 143, 202, 63, 15, 2,
		193, 175, 189, 3, 1, 19, 138, 107, 58, 145,
		17, 65, 79, 103, 220, 234, 151, 242, 207, 206,
		240, 180, 230, 115, 150, 172, 116, 34, 231, 173,
		53, 133, 226, 249, 55, 232, 28, 117, 223, 110,
		71, 241, 26, 113, 29, 41, 197, 137, 111, 183,
		98, 14, 170, 24, 190, 27, 252, 86, 62, 75,
		198, 210, 121, 32, 154, 219, 192, 254, 120, 205,
		90, 244, 31, 221, 168, 51, 136, 7, 199, 49,
		177, 18, 16, 89, 39, 128, 236, 95, 96, 81,
		127, 169, 25, 181, 74, 13, 45, 229, 122, 159,
		147, 201, 156, 239, 160, 224, 59, 77, 174, 42,
		245, 176, 200, 235, 187, 60, 131, 83, 153, 97,
		23, 43, 4, 126, 186, 119, 214, 38, 225, 105,
		20, 99, 85, 33, 12, 125
	};

	internal static byte[] Method_001E(byte[] data)
	{
		uint[] array = new uint[8] { 1779033703u, 3144134277u, 1013904242u, 2773480762u, 1359893119u, 2600822924u, 528734635u, 1541459225u };
		uint[] array2 = Method_0024();
		byte[] array3 = Method_0022(data);
		for (int i = 0; i < array3.Length; i += 64)
		{
			uint[] array4 = new uint[64];
			for (int j = 0; j < 16; j++)
			{
				int num = i + j * 4;
				array4[j] = (uint)((array3[num] << 24) | (array3[num + 1] << 16) | (array3[num + 2] << 8) | array3[num + 3]);
			}
			for (int k = 16; k < 64; k++)
			{
				uint num2 = Method_0023(array4[k - 15], 7) ^ Method_0023(array4[k - 15], 18) ^ (array4[k - 15] >> 3);
				uint num3 = Method_0023(array4[k - 2], 17) ^ Method_0023(array4[k - 2], 19) ^ (array4[k - 2] >> 10);
				array4[k] = array4[k - 16] + num2 + array4[k - 7] + num3;
			}
			uint num4 = array[0];
			uint num5 = array[1];
			uint num6 = array[2];
			uint num7 = array[3];
			uint num8 = array[4];
			uint num9 = array[5];
			uint num10 = array[6];
			uint num11 = array[7];
			for (int l = 0; l < 64; l++)
			{
				uint num12 = Method_0023(num8, 6) ^ Method_0023(num8, 11) ^ Method_0023(num8, 25);
				uint num13 = (num8 & num9) ^ (~num8 & num10);
				uint num14 = num11 + num12 + num13 + array2[l] + array4[l];
				uint num15 = Method_0023(num4, 2) ^ Method_0023(num4, 13) ^ Method_0023(num4, 22);
				uint num16 = (num4 & num5) ^ (num4 & num6) ^ (num5 & num6);
				uint num17 = num15 + num16;
				num11 = num10;
				num10 = num9;
				num9 = num8;
				num8 = num7 + num14;
				num7 = num6;
				num6 = num5;
				num5 = num4;
				num4 = num14 + num17;
			}
			array[0] += num4;
			array[1] += num5;
			array[2] += num6;
			array[3] += num7;
			array[4] += num8;
			array[5] += num9;
			array[6] += num10;
			array[7] += num11;
		}
		byte[] array5 = new byte[32];
		for (int m = 0; m < 8; m++)
		{
			array5[m * 4] = (byte)(array[m] >> 24);
			array5[m * 4 + 1] = (byte)(array[m] >> 16);
			array5[m * 4 + 2] = (byte)(array[m] >> 8);
			array5[m * 4 + 3] = (byte)array[m];
		}
		return array5;
	}

	internal static string Method_001F(byte[] data)
	{
		byte[] array = Method_001E(data);
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	internal static byte[] Method_0020(byte[] cipher, byte[] key, byte[] iv)
	{
		if (cipher == null || cipher.Length == 0 || cipher.Length % 16 != 0)
		{
			throw new InvalidOperationException();
		}
		if (key == null || (key.Length != 16 && key.Length != 24 && key.Length != 32))
		{
			throw new InvalidOperationException();
		}
		if (iv == null || iv.Length != 16)
		{
			throw new InvalidOperationException();
		}
		int rounds;
		if (key.Length == 32)
		{
			rounds = 14;
		}
		else
		{
			rounds = ((key.Length == 24) ? 12 : 10);
		}
		uint[] expanded = Method_0027(key, rounds);
		byte[] array = new byte[cipher.Length];
		byte[] array2 = new byte[16];
		Array.Copy(iv, array2, 16);
		for (int i = 0; i < cipher.Length; i += 16)
		{
			byte[] array3 = new byte[16];
			Array.Copy(cipher, i, array3, 0, 16);
			byte[] array4 = Method_002A(array3, expanded, rounds);
			for (int j = 0; j < 16; j++)
			{
				array[i + j] = (byte)(array4[j] ^ array2[j]);
			}
			Array.Copy(array3, array2, 16);
		}
		return Method_0026(array);
	}

	internal static byte[] Method_0021(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Array.Empty<byte>();
		}
		int length = value.Length;
		int num = 0;
		if (length > 0 && value[length - 1] == '=')
		{
			num++;
			if (length > 1 && value[length - 2] == '=')
			{
				num++;
			}
		}
		int num2 = length * 3 / 4 - num;
		byte[] array = new byte[num2];
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c == '=')
			{
				break;
			}
			int num6 = Method_0025(c);
			if (num6 >= 0)
			{
				num4 = (num4 << 6) | num6;
				num5 += 6;
				if (num5 >= 8)
				{
					num5 -= 8;
					array[num3++] = (byte)((num4 >> num5) & 0xFF);
				}
			}
		}
		if (num3 != num2)
		{
			byte[] array2 = new byte[num3];
			Array.Copy(array, array2, num3);
			return array2;
		}
		return array;
	}

	private static byte[] Method_0022(byte[] data)
	{
		ulong num = (ulong)data.Length * 8uL;
		int num2 = (56 - (data.Length + 1) % 64 + 64) % 64;
		int num3 = data.Length + 1 + num2 + 8;
		byte[] array = new byte[num3];
		Array.Copy(data, array, data.Length);
		array[data.Length] = 128;
		for (int i = 0; i < 8; i++)
		{
			array[num3 - 8 + i] = (byte)(num >> 56 - 8 * i);
		}
		return array;
	}

	private static uint Method_0023(uint value, int count)
	{
		return (value >> count) | (value << 32 - count);
	}

	private static uint[] Method_0024()
	{
		return new uint[64]
		{
			1116352408u, 1899447441u, 3049323471u, 3921009573u, 961987163u, 1508970993u, 2453635748u, 2870763221u, 3624381080u, 310598401u,
			607225278u, 1426881987u, 1925078388u, 2162078206u, 2614888103u, 3248222580u, 3835390401u, 4022224774u, 264347078u, 604807628u,
			770255983u, 1249150122u, 1555081692u, 1996064986u, 2554220882u, 2821834349u, 2952996808u, 3210313671u, 3336571891u, 3584528711u,
			113926993u, 338241895u, 666307205u, 773529912u, 1294757372u, 1396182291u, 1695183700u, 1986661051u, 2177026350u, 2456956037u,
			2730485921u, 2820302411u, 3259730800u, 3345764771u, 3516065817u, 3600352804u, 4094571909u, 275423344u, 430227734u, 506948616u,
			659060556u, 883997877u, 958139571u, 1322822218u, 1537002063u, 1747873779u, 1955562222u, 2024104815u, 2227730452u, 2361852424u,
			2428436474u, 2756734187u, 3204031479u, 3329325298u
		};
	}

	private static int Method_0025(char c)
	{
		if (c >= 'A' && c <= 'Z')
		{
			return c - 65;
		}
		if (c >= 'a' && c <= 'z')
		{
			return c - 97 + 26;
		}
		if (c >= '0' && c <= '9')
		{
			return c - 48 + 52;
		}
		return c switch
		{
			'+' => 62, 
			'/' => 63, 
			_ => -1, 
		};
	}

	private static byte[] Method_0026(byte[] data)
	{
		if (data.Length == 0)
		{
			return data;
		}
		byte b = data[data.Length - 1];
		if (b == 0 || b > 16 || b > data.Length)
		{
			return data;
		}
		for (int i = data.Length - b; i < data.Length; i++)
		{
			if (data[i] != b)
			{
				return data;
			}
		}
		byte[] array = new byte[data.Length - b];
		Array.Copy(data, array, array.Length);
		return array;
	}

	private static uint[] Method_0027(byte[] key, int rounds)
	{
		int num = key.Length / 4;
		uint[] array = new uint[4 * (rounds + 1)];
		for (int i = 0; i < num; i++)
		{
			array[i] = (uint)((key[i * 4] << 24) | (key[i * 4 + 1] << 16) | (key[i * 4 + 2] << 8) | key[i * 4 + 3]);
		}
		byte[] array2 = new byte[10] { 1, 2, 4, 8, 16, 32, 64, 128, 27, 54 };
		int num2 = 0;
		for (int j = num; j < array.Length; j++)
		{
			uint num3 = array[j - 1];
			if (j % num == 0)
			{
				num3 = Method_0029(Method_0028(num3)) ^ (uint)(array2[num2++] << 24);
			}
			else if (num > 6 && j % num == 4)
			{
				num3 = Method_0029(num3);
			}
			array[j] = array[j - num] ^ num3;
		}
		return array;
	}

	private static uint Method_0028(uint value)
	{
		return (value << 8) | (value >> 24);
	}

	private static uint Method_0029(uint value)
	{
		return (uint)((Method_0030((int)(value >> 24)) << 24) | (Method_0030((int)((value >> 16) & 0xFF)) << 16) | (Method_0030((int)((value >> 8) & 0xFF)) << 8) | Method_0030((int)(value & 0xFF)));
	}

	private static byte[] Method_002A(byte[] input, uint[] expanded, int rounds)
	{
		byte[] array = new byte[16];
		Array.Copy(input, array, 16);
		Method_002B(array, expanded, rounds);
		for (int num = rounds - 1; num > 0; num--)
		{
			Method_002D(array);
			Method_002C(array);
			Method_002B(array, expanded, num);
			Method_002E(array);
		}
		Method_002D(array);
		Method_002C(array);
		Method_002B(array, expanded, 0);
		return array;
	}

	private static void Method_002B(byte[] state, uint[] expanded, int round)
	{
		for (int i = 0; i < 4; i++)
		{
			uint num = expanded[round * 4 + i];
			state[i * 4] ^= (byte)(num >> 24);
			state[i * 4 + 1] ^= (byte)(num >> 16);
			state[i * 4 + 2] ^= (byte)(num >> 8);
			state[i * 4 + 3] ^= (byte)num;
		}
	}

	private static void Method_002C(byte[] state)
	{
		for (int i = 0; i < 16; i++)
		{
			state[i] = Method_0031(state[i]);
		}
	}

	private static void Method_002D(byte[] state)
	{
		byte b = state[13];
		state[13] = state[9];
		state[9] = state[5];
		state[5] = state[1];
		state[1] = b;
		b = state[2];
		state[2] = state[10];
		state[10] = b;
		b = state[6];
		state[6] = state[14];
		state[14] = b;
		b = state[3];
		state[3] = state[7];
		state[7] = state[11];
		state[11] = state[15];
		state[15] = b;
	}

	private static void Method_002E(byte[] state)
	{
		for (int i = 0; i < 4; i++)
		{
			int num = i * 4;
			byte b = state[num];
			byte b2 = state[num + 1];
			byte b3 = state[num + 2];
			byte b4 = state[num + 3];
			state[num] = (byte)(Method_002F(14, b) ^ Method_002F(11, b2) ^ Method_002F(13, b3) ^ Method_002F(9, b4));
			state[num + 1] = (byte)(Method_002F(9, b) ^ Method_002F(14, b2) ^ Method_002F(11, b3) ^ Method_002F(13, b4));
			state[num + 2] = (byte)(Method_002F(13, b) ^ Method_002F(9, b2) ^ Method_002F(14, b3) ^ Method_002F(11, b4));
			state[num + 3] = (byte)(Method_002F(11, b) ^ Method_002F(13, b2) ^ Method_002F(9, b3) ^ Method_002F(14, b4));
		}
	}

	private static byte Method_002F(byte a, byte b)
	{
		byte b2 = 0;
		for (int i = 0; i < 8; i++)
		{
			if ((b & 1) != 0)
			{
				b2 ^= a;
			}
			byte b3 = (byte)(a & 0x80);
			a <<= 1;
			if (b3 != 0)
			{
				a ^= 0x1B;
			}
			b >>= 1;
		}
		return b2;
	}

	private static byte Method_0030(int value)
	{
		return Field_0032[value & 0xFF];
	}

	private static byte Method_0031(int value)
	{
		return Field_0033[value & 0xFF];
	}
}
