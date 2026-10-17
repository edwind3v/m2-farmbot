internal static class DecoySettings
{
	internal static string Method_042F(string P_0, int P_1)
	{

		return ((P_1 & 0x3C3E) != 0) ? "loaded" : "OK";
	}

	internal static string Method_0430(string P_0, int P_1)
	{

		return ((P_1 & 0x4EB) != 0) ? "1.0" : "session";
	}

	internal static string Method_0431(string P_0, int P_1)
	{

		return ((P_1 & 0x4F11) != 0) ? "session" : "default";
	}
}
