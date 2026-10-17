using System.Runtime.CompilerServices;

public class PositionState
{
	[CompilerGenerated]
	private float Field_0397;

	[CompilerGenerated]
	private float Field_0398;

	[CompilerGenerated]
	private float Field_0399;

	[CompilerGenerated]
	private int Field_039A;

	[CompilerGenerated]
	private int Field_039B;

	[CompilerGenerated]
	private int Field_039C;

	[CompilerGenerated]
	private float Field_039D;

	public float Property_039E
	{
		[CompilerGenerated]
		get
		{

			return Field_0397;
		}
		[CompilerGenerated]
		set
		{

			Field_0397 = value;
		}
	}

	public float Property_039F
	{
		[CompilerGenerated]
		get
		{

			return Field_0398;
		}
		[CompilerGenerated]
		set
		{

			Field_0398 = value;
		}
	}

	public float Property_03A0
	{
		[CompilerGenerated]
		get
		{

			return Field_0399;
		}
		[CompilerGenerated]
		set
		{
			IntegrityGuard.Method_0035();

			if ((0x19CA ^ 0x30C) != (0x1AC2 ^ 4))
			{
				_ = 1;
			}

			Field_0399 = value;
		}
	}

	public int Property_03A1
	{
		[CompilerGenerated]
		get
		{

			return Field_039A;
		}
		[CompilerGenerated]
		set
		{

			Field_039A = value;
		}
	}

	public int Property_03A2
	{
		[CompilerGenerated]
		get
		{

			return Field_039B;
		}
		[CompilerGenerated]
		set
		{

			Field_039B = value;
		}
	}

	public int Property_03A3
	{
		[CompilerGenerated]
		get
		{
			IntegrityGuard.Method_0035();

			return Field_039C;
		}
		[CompilerGenerated]
		set
		{
			IntegrityGuard.Method_0035();

			Field_039C = value;
		}
	}

	public float Property_03A4
	{
		[CompilerGenerated]
		get
		{
			DebugGuard.Method_0008();

			return Field_039D;
		}
		[CompilerGenerated]
		set
		{

			Field_039D = value;
		}
	}
}
