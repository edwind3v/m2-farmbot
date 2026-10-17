using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace RuntimeBroker.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.11.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings Field_011C;

	public static Settings Property_011D
	{
		get
		{

			return Field_011C;
		}
	}

	static Settings()
	{
		DebugGuard.Method_0008();
		Field_011C = (Settings)RuntimeAdapters.Support_04A1(new Settings());
	}
}
