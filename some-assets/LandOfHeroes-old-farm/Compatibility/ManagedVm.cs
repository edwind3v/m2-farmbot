using System;
using System.Reflection;
using System.Text;

internal static class ManagedVm
{
	private const byte Field_00A6 = 0;

	private const byte Field_00A7 = 1;

	private const byte Field_00A8 = 2;

	private const byte Field_00A9 = 3;

	private const byte Field_00AA = 4;

	private const byte Field_00AB = 5;

	private const byte Field_00AC = 6;

	private const byte Field_00AD = 7;

	private const byte Field_00AE = 8;

	private const byte Field_00AF = 9;

	private const byte Field_00B0 = 10;

	private const byte Field_00B1 = 11;

	private const byte Field_00B2 = 12;

	private const byte Field_00B3 = 13;

	private const byte Field_00B4 = 14;

	private const byte Field_00B5 = 15;

	private const byte Field_00B6 = 16;

	private const byte Field_00B7 = 17;

	private const byte Field_00B8 = 18;

	private const byte Field_00B9 = 19;

	private const byte Field_00BA = 20;

	private const byte Field_00BB = 21;

	private const byte Field_00BC = 22;

	private const byte Field_00BD = 23;

	private const byte Field_00BE = 24;

	private const byte Field_00BF = 25;

	private const byte Field_00C0 = 26;

	private const byte Field_00C1 = 27;

	private const byte Field_00C2 = 28;

	private const byte Field_00C3 = 29;

	private const byte Field_00C4 = 30;

	private const byte Field_00C5 = 31;

	private const byte Field_00C6 = 32;

	private const byte Field_00C7 = 33;

	private const byte Field_00C8 = 34;

	private const byte Field_00C9 = 35;

	private const byte Field_00CA = 36;

	private const byte Field_00CB = 37;

	private const byte Field_00CC = 38;

	private const byte Field_00CD = 39;

	private const byte Field_00CE = 40;

	private const byte Field_00CF = 41;

	private const byte Field_00D0 = 42;

	private const byte Field_00D1 = 43;

	private const byte Field_00D2 = 44;

	private const byte Field_00D3 = 45;

	private const byte Field_00D4 = 46;

	private const byte Field_00D5 = 47;

	private const byte Field_00D6 = 48;

	private const byte Field_00D7 = 49;

	private const byte Field_00D8 = 50;

	private const byte Field_00D9 = 51;

	private const byte Field_00DA = 52;

	private const byte Field_00DB = 53;

	private const byte Field_00DC = 54;

	private const byte Field_00DD = 55;

	private const byte Field_00DE = 56;

	private const byte Field_00DF = 57;

	private const byte Field_00E0 = 58;

	private const byte Field_00E1 = 59;

	private const byte Field_00E2 = 60;

	private const byte Field_00E3 = 61;

	private const byte Field_00E4 = 62;

	private const byte Field_00E5 = 63;

	private const byte Field_00E6 = 64;

	private const byte Field_00E7 = 65;

	private const byte Field_00E8 = 66;

	private const byte Field_00E9 = 67;

	private const byte Field_00EA = 68;

	private const byte Field_00EB = 69;

	private const byte Field_00EC = 70;

	private const byte Field_00ED = 71;

	internal static byte[] Method_009F(byte[] src, byte key)
	{
		byte[] array = new byte[src.Length];
		for (int i = 0; i < src.Length; i++)
		{
			array[i] = (byte)(src[i] ^ key ^ (byte)(i * 37 + (i >> 8)));
		}
		return array;
	}

	public static object Method_00A0(byte[] code, object[] args, MethodBase[] methods, FieldInfo[] fields, string[] strings, Type[] types, byte stringKey)
	{
		object[] array = new object[64];
		int num = 0;
		object[] array2 = new object[32];
		int num2 = 0;
		while (num2 < code.Length)
		{
			byte b = code[num2++];
			switch (b)
			{
			case 1:
				array[num++] = BitConverter.ToInt32(code, num2);
				num2 += 4;
				break;
			case 2:
				array[num++] = args[code[num2++]];
				break;
			case 3:
				array[num++] = array2[code[num2++]];
				break;
			case 4:
				array2[code[num2++]] = array[--num];
				break;
			case 5:
			case 6:
			{
				ushort num39 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				MethodBase methodBase = methods[num39];
				ParameterInfo[] parameters2 = methodBase.GetParameters();
				int num40 = parameters2.Length;
				object[] array7 = new object[num40];
				for (int num41 = num40 - 1; num41 >= 0; num41--)
				{
					array7[num41] = Method_00A3(array[--num], parameters2[num41].ParameterType);
				}
				object obj4 = null;
				if (!methodBase.IsStatic)
				{
					obj4 = array[--num];
				}
				object obj5 = methodBase.Invoke(obj4, array7);
				if (methodBase is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
				{
					array[num++] = obj5;
				}
				break;
			}
			case 7:
				if (num != 0)
				{
					return array[--num];
				}
				return null;
			case 8:
				num2 = BitConverter.ToInt32(code, num2);
				break;
			case 9:
			{
				int num34 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				if (Method_00A1(array[--num]))
				{
					num2 = num34;
				}
				break;
			}
			case 10:
			{
				int num28 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				if (!Method_00A1(array[--num]))
				{
					num2 = num28;
				}
				break;
			}
			case 11:
			{
				ushort num26 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				array[num++] = Method_00A4(strings[num26], stringKey);
				break;
			}
			case 12:
				array[num++] = null;
				break;
			case 13:
				num--;
				break;
			case 14:
				array[num] = array[num - 1];
				num++;
				break;
			case 15:
			{
				long num7 = Method_00A2(array[--num]);
				long num8 = Method_00A2(array[--num]);
				array[num++] = num8 + num7;
				break;
			}
			case 16:
			{
				long num56 = Method_00A2(array[--num]);
				long num57 = Method_00A2(array[--num]);
				array[num++] = num57 - num56;
				break;
			}
			case 17:
			{
				long num54 = Method_00A2(array[--num]);
				long num55 = Method_00A2(array[--num]);
				array[num++] = num55 * num54;
				break;
			}
			case 18:
			{
				long num52 = Method_00A2(array[--num]);
				long num53 = Method_00A2(array[--num]);
				array[num++] = num53 % num52;
				break;
			}
			case 19:
			{
				long num50 = Method_00A2(array[--num]);
				long num51 = Method_00A2(array[--num]);
				array[num++] = num51 ^ num50;
				break;
			}
			case 20:
			{
				long num48 = Method_00A2(array[--num]);
				long num49 = Method_00A2(array[--num]);
				array[num++] = num49 & num48;
				break;
			}
			case 21:
			{
				long num46 = Method_00A2(array[--num]);
				long num47 = Method_00A2(array[--num]);
				array[num++] = num47 | num46;
				break;
			}
			case 22:
				array[num - 1] = ~Method_00A2(array[num - 1]);
				break;
			case 23:
			{
				object objB3 = array[--num];
				object objA = array[--num];
				array[num++] = (object.Equals(objA, objB3) ? 1 : 0);
				break;
			}
			case 24:
			{
				long num44 = Method_00A2(array[--num]);
				long num45 = Method_00A2(array[--num]);
				array[num++] = ((num45 < num44) ? 1 : 0);
				break;
			}
			case 25:
			{
				long num42 = Method_00A2(array[--num]);
				long num43 = Method_00A2(array[--num]);
				array[num++] = ((num43 > num42) ? 1 : 0);
				break;
			}
			case 26:
			{
				ushort num36 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				ConstructorInfo constructorInfo = (ConstructorInfo)methods[num36];
				ParameterInfo[] parameters = constructorInfo.GetParameters();
				int num37 = parameters.Length;
				object[] array6 = new object[num37];
				for (int num38 = num37 - 1; num38 >= 0; num38--)
				{
					array6[num38] = Method_00A3(array[--num], parameters[num38].ParameterType);
				}
				array[num++] = constructorInfo.Invoke(array6);
				break;
			}
			case 27:
			{
				ushort num35 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				Type conversionType = types[num35];
				array[num - 1] = Convert.ChangeType(array[num - 1], conversionType);
				break;
			}
			case 28:
			case 29:
			case 30:
			{
				ushort num33 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				Type type = types[num33];
				if (b == 30)
				{
					object obj3 = array[num - 1];
					array[num - 1] = ((obj3 != null && type.IsInstanceOfType(obj3)) ? obj3 : null);
				}
				else
				{
					array[num - 1] = Convert.ChangeType(array[num - 1], type);
				}
				break;
			}
			case 31:
			{
				ushort num32 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				object obj2 = array[--num];
				array[num++] = fields[num32].GetValue(obj2);
				break;
			}
			case 32:
			{
				ushort num31 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				FieldInfo fieldInfo2 = fields[num31];
				object value3 = Method_00A3(array[--num], fieldInfo2.FieldType);
				object obj = array[--num];
				fieldInfo2.SetValue(obj, value3);
				break;
			}
			case 33:
			{
				ushort num30 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				array[num++] = fields[num30].GetValue(null);
				break;
			}
			case 34:
			{
				ushort num29 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				FieldInfo fieldInfo = fields[num29];
				fieldInfo.SetValue(null, Method_00A3(array[--num], fieldInfo.FieldType));
				break;
			}
			case 35:
				throw (Exception)array[--num];
			case 36:
				array[num++] = 0;
				break;
			case 37:
				array[num++] = 1;
				break;
			case 38:
				array[num++] = -1;
				break;
			case 44:
				array[num++] = 2;
				break;
			case 45:
				array[num++] = 3;
				break;
			case 46:
				array[num++] = 4;
				break;
			case 47:
				array[num++] = 5;
				break;
			case 48:
				array[num++] = 6;
				break;
			case 49:
				array[num++] = 7;
				break;
			case 50:
				array[num++] = 8;
				break;
			case 51:
			{
				int num27 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				object objB2 = array[--num];
				if (object.Equals(array[--num], objB2))
				{
					num2 = num27;
				}
				break;
			}
			case 52:
			{
				int num25 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				object objB = array[--num];
				if (!object.Equals(array[--num], objB))
				{
					num2 = num25;
				}
				break;
			}
			case 53:
			{
				int num23 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				long num24 = Method_00A2(array[--num]);
				if (Method_00A2(array[--num]) < num24)
				{
					num2 = num23;
				}
				break;
			}
			case 54:
			{
				int num21 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				long num22 = Method_00A2(array[--num]);
				if (Method_00A2(array[--num]) > num22)
				{
					num2 = num21;
				}
				break;
			}
			case 55:
			{
				ushort num19 = BitConverter.ToUInt16(code, num2);
				num2 += 2;
				long num20 = Method_00A2(array[--num]);
				array[num++] = Array.CreateInstance(types[num19], (int)num20);
				break;
			}
			case 56:
			{
				object value2 = array[--num];
				long num18 = Method_00A2(array[--num]);
				((Array)array[--num]).SetValue(value2, (int)num18);
				break;
			}
			case 57:
			{
				long num17 = Method_00A2(array[--num]);
				Array array5 = (Array)array[--num];
				array[num++] = array5.GetValue((int)num17);
				break;
			}
			case 58:
			{
				byte b2 = Convert.ToByte(array[--num]);
				long num16 = Method_00A2(array[--num]);
				((byte[])array[--num])[(int)num16] = b2;
				break;
			}
			case 59:
			{
				long num15 = Method_00A2(array[--num]);
				byte[] array4 = (byte[])array[--num];
				array[num++] = array4[(int)num15];
				break;
			}
			case 60:
			{
				int num13 = (int)Method_00A2(array[--num]);
				long num14 = Method_00A2(array[--num]);
				array[num++] = num14 >> num13;
				break;
			}
			case 61:
			{
				int num11 = (int)Method_00A2(array[--num]);
				long num12 = Method_00A2(array[--num]);
				array[num++] = num12 << num11;
				break;
			}
			case 62:
			{
				Array array3 = (Array)array[--num];
				array[num++] = array3.Length;
				break;
			}
			case 63:
				array[num - 1] = (byte)Method_00A2(array[num - 1]);
				break;
			case 64:
			{
				int num9 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				long num10 = Method_00A2(array[--num]);
				if (Method_00A2(array[--num]) <= num10)
				{
					num2 = num9;
				}
				break;
			}
			case 65:
			{
				int num5 = BitConverter.ToInt32(code, num2);
				num2 += 4;
				long num6 = Method_00A2(array[--num]);
				if (Method_00A2(array[--num]) >= num6)
				{
					num2 = num5;
				}
				break;
			}
			case 66:
			{
				short num3 = Convert.ToInt16(array[--num]);
				long num4 = Method_00A2(array[--num]);
				((Array)array[--num]).SetValue((char)num3, (int)num4);
				break;
			}
			case 67:
				array[num - 1] = (ushort)Method_00A2(array[num - 1]);
				break;
			case 68:
			{
				byte[] data2 = (byte[])array[--num];
				array[num++] = CryptoPrimitives.Method_001E(data2);
				break;
			}
			case 69:
			{
				byte[] data = (byte[])array[--num];
				array[num++] = CryptoPrimitives.Method_001F(data);
				break;
			}
			case 70:
			{
				byte[] iv = (byte[])array[--num];
				byte[] key = (byte[])array[--num];
				byte[] cipher = (byte[])array[--num];
				array[num++] = CryptoPrimitives.Method_0020(cipher, key, iv);
				break;
			}
			case 71:
			{
				string value = (string)array[--num];
				array[num++] = CryptoPrimitives.Method_0021(value);
				break;
			}
			case 39:
				array[num - 1] = (int)Method_00A2(array[num - 1]);
				break;
			case 40:
				array[num - 1] = Method_00A2(array[num - 1]);
				break;
			case 41:
				array[num - 1] = (uint)Method_00A2(array[num - 1]);
				break;
			case 42:
				array[num - 1] = (ulong)Method_00A2(array[num - 1]);
				break;
			case 43:
				num2 = BitConverter.ToInt32(code, num2);
				break;
			default:
				throw new InvalidProgramException(Method_00A5() + b);
			case 0:
				break;
			}
		}
		return null;
	}

	private static bool Method_00A1(object value)
	{
		if (value is bool)
		{
			return (bool)value;
		}
		if (value is int num)
		{
			return num != 0;
		}
		return value != null;
	}

	private static long Method_00A2(object value)
	{
		if (value == null)
		{
			return 0L;
		}
		return Convert.ToInt64(value);
	}

	private static object Method_00A3(object value, Type targetType)
	{
		if (value == null)
		{
			return null;
		}
		if (targetType.IsInstanceOfType(value))
		{
			return value;
		}
		if (targetType.IsEnum)
		{
			return Enum.ToObject(targetType, Convert.ToInt64(value));
		}
		if (targetType == typeof(int))
		{
			return (int)Convert.ToInt64(value);
		}
		if (targetType == typeof(uint))
		{
			return (uint)Convert.ToInt64(value);
		}
		if (targetType == typeof(short))
		{
			return (short)Convert.ToInt64(value);
		}
		if (targetType == typeof(ushort))
		{
			return (ushort)Convert.ToInt64(value);
		}
		if (targetType == typeof(byte))
		{
			return (byte)Convert.ToInt64(value);
		}
		if (targetType == typeof(sbyte))
		{
			return (sbyte)Convert.ToInt64(value);
		}
		if (targetType == typeof(long) || targetType == typeof(ulong))
		{
			return Convert.ToInt64(value);
		}
		return Convert.ChangeType(value, targetType);
	}

	private static string Method_00A4(string value, byte key)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		char[] array = value.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (char)(array[i] ^ ((key + i) & 0xFFFF));
		}
		return new string(array);
	}

	private static string Method_00A5()
	{
		return Encoding.UTF8.GetString(new byte[19]
		{
			85, 110, 107, 110, 111, 119, 110, 32, 86, 77,
			32, 111, 112, 99, 111, 100, 101, 58, 32
		});
	}
}
