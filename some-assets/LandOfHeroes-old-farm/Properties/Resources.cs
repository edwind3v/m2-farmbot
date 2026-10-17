using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace RuntimeBroker.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager Field_0117;

	private static CultureInfo Field_0118;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager Property_0119
	{
		get
		{

			if (Field_0117 == null)
			{
				Field_0117 = new ResourceManager(StringDecoder.Method_007B("TlGMSpfQoNZ3iQqoJ69ffvCsL8ptvxzITMGtullRun24G4W2ebbZlPYGbCxGAeoN", 0), RuntimeAdapters.Support_04A0(typeof(Resources).TypeHandle).Assembly);
			}
			return Field_0117;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Property_011A
	{
		get
		{

			return Field_0118;
		}
		set
		{

			Field_0118 = value;
		}
	}

	internal Resources()
	{
	}
}
