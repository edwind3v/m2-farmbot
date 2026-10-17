using System;
using System.Configuration;
using System.Globalization;

internal static class RuntimeAdapters
{
	public static Type Support_04A0(RuntimeTypeHandle P_0)
	{
		IntegrityGuard.Method_0035();
		return Type.GetTypeFromHandle(P_0);
	}

	public static SettingsBase Support_04A1(SettingsBase P_0)
	{
		return SettingsBase.Synchronized(P_0);
	}

	public static string Support_04A2(string P_0, object P_1)
	{
		return string.Format(P_0, P_1);
	}

	public static bool Support_04A3(float P_0)
	{
		return float.IsNaN(P_0);
	}

	public static bool Support_04A4(float P_0)
	{
		return float.IsInfinity(P_0);
	}

	public static float Support_04A5(float P_0)
	{
		return Math.Abs(P_0);
	}

	public static bool Support_04A6(float P_0)
	{
		return AutomationEngine.Method_03FE(P_0);
	}

	public static bool Support_04A7(float P_0, float P_1, float P_2)
	{
		return AutomationEngine.Method_03FF(P_0, P_1, P_2);
	}

	public static bool Support_04A8(uint P_0)
	{
		return AutomationEngine.Method_03FD(P_0);
	}

	public static float Support_04A9(float P_0, float P_1, float P_2, float P_3, float P_4, float P_5)
	{
		DebugGuard.Method_0008();
		return AutomationEngine.Method_040D(P_0, P_1, P_2, P_3, P_4, P_5);
	}

	public static int Support_04AA()
	{
		return Environment.TickCount;
	}

	public static long Support_04AB(string P_0)
	{
		IntegrityGuard.Method_0035();
		return AutomationEngine.Method_040C(P_0);
	}

	public static bool Support_04AC(string P_0)
	{
		return string.IsNullOrEmpty(P_0);
	}

	public static bool Support_04AD(string P_0, NumberStyles P_1, IFormatProvider P_2, ref long P_3)
	{
		return long.TryParse(P_0, P_1, P_2, out P_3);
	}

	public static double Support_04AE(double P_0)
	{
		return Math.Sqrt(P_0);
	}
}
