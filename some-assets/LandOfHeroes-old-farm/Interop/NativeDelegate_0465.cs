using System;
using System.Runtime.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
internal delegate bool Support_0465(IntPtr P_0, out bool P_1);
