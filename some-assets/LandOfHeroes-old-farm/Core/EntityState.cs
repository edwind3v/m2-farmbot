using System.Runtime.CompilerServices;

public class EntityState
{
	[CompilerGenerated]
	private int Field_03BA;

	[CompilerGenerated]
	private float Field_03BB;

	[CompilerGenerated]
	private float Field_03BC;

	[CompilerGenerated]
	private float Field_03BD;

	[CompilerGenerated]
	private float Field_03BE;

	[CompilerGenerated]
	private int Field_03BF;

	[CompilerGenerated]
	private int Field_03C0;

	[CompilerGenerated]
	private int Field_03C1;

	[CompilerGenerated]
	private float Field_03C2;

	[CompilerGenerated]
	private string Field_03C3;

	public int Property_03C4
	{
		[CompilerGenerated]
		get
		{

			return Field_03BA;
		}
		[CompilerGenerated]
		set
		{
			if ((0x4F6 ^ 0x139) != (0x5CC ^ 3))
			{
				_ = 1;
			}

			Field_03BA = value;
		}
	}

	public float X
	{
		[CompilerGenerated]
		get
		{
			IntegrityGuard.Method_0035();

			return Field_03BB;
		}
		[CompilerGenerated]
		set
		{

			Field_03BB = value;
		}
	}

	public float Y
	{
		[CompilerGenerated]
		get
		{

			return Field_03BC;
		}
		[CompilerGenerated]
		set
		{

			Field_03BC = value;
		}
	}

	public float Z
	{
		[CompilerGenerated]
		get
		{
			IntegrityGuard.Method_0035();

			return Field_03BD;
		}
		[CompilerGenerated]
		set
		{

			Field_03BD = value;
		}
	}

	public float Property_03C8
	{
		[CompilerGenerated]
		get
		{
			IntegrityGuard.Method_0035();

			return Field_03BE;
		}
		[CompilerGenerated]
		set
		{

			Field_03BE = value;
		}
	}

	public int Property_03C9
	{
		[CompilerGenerated]
		get
		{

			return Field_03BF;
		}
		[CompilerGenerated]
		set
		{

			Field_03BF = value;
		}
	}

	public int Property_03CA
	{
		[CompilerGenerated]
		get
		{

			return Field_03C0;
		}
		[CompilerGenerated]
		set
		{

			Field_03C0 = value;
		}
	}

	public int Property_03CB
	{
		[CompilerGenerated]
		get
		{
			IntegrityGuard.Method_0035();
			DebugGuard.Method_0008();

			return Field_03C1;
		}
		[CompilerGenerated]
		set
		{
			DebugGuard.Method_0008();

			Field_03C1 = value;
		}
	}

	public float Distance
	{
		[CompilerGenerated]
		get
		{

			return Field_03C2;
		}
		[CompilerGenerated]
		set
		{

			Field_03C2 = value;
		}
	}

	public string Property_03CD
	{
		[CompilerGenerated]
		get
		{

			return Field_03C3;
		}
		[CompilerGenerated]
		set
		{

			Field_03C3 = value;
		}
	}
}
