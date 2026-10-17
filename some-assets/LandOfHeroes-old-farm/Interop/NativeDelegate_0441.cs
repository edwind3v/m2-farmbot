using System;
using System.Runtime.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
internal delegate bool Support_0441(IntPtr P_0, int P_1, IntPtr P_2, NativeMethods.Type_01A4 P_3, IntPtr P_4, IntPtr P_5, IntPtr P_6);
