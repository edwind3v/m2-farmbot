using System;
using System.Runtime.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
internal delegate bool Support_044D(IntPtr P_0, UIntPtr P_1, UIntPtr P_2, uint P_3);
