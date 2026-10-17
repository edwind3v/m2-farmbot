using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;

public partial class MainForm : Form
{

	[Serializable]
	[CompilerGenerated]
	private sealed class Type_0275
	{
		public static readonly Type_0275 Field_0279 = new Type_0275();

		public static Comparison<EntityState> Field_027A;

		public static Func<EntityState, float> Field_027B;

		public static Func<EntityState, float> Field_027C;

		internal int Method_0276(EntityState x, EntityState y)
		{

			return x.Distance.CompareTo(y.Distance);
		}

		internal float Method_0277(EntityState m)
		{
			DebugGuard.Method_0008();

			return m.Distance;
		}

		internal float Method_0278(EntityState m)
		{

			return m.Distance;
		}
	}

	[CompilerGenerated]
	private sealed class Type_027D
	{
		public RoutePoint Field_027F;

		public MainForm Field_0280;

		internal bool Method_027E(EntityState m)
		{

			return Field_0280.Distance3D(Field_027F.X, Field_027F.Y, 0f, m.X, m.Y, 0f) <= Field_0280.routeFarmRange;
		}
	}

	[CompilerGenerated]
	private sealed class Type_0281
	{
		public MainForm Field_0283;

		public int Field_0284;

		internal void Method_0282()
		{

			Field_0283.Method_02B8(Field_0283.routePoints, Field_0284);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct Type_028A : IAsyncStateMachine
	{
		public int Field_028B;

		public AsyncVoidMethodBuilder Field_028C;

		public List<RoutePoint> Field_028D;

		public int Field_028E;

		public MainForm Field_028F;

		private int Field_0290;

		private bool Field_0291;

		private bool Field_0292;

		private RoutePoint Field_0293;

		private int Field_0294;

		private DateTime Field_0295;

		private float Field_0296;

		private DateTime Field_0297;

		private List<EntityState> Field_0298;

		private TaskAwaiter Field_0299;

		private void MoveNext()
		{
			DebugGuard.Method_0008();
			int num = Field_028B;
			MainForm obj = Field_028F;
			try
			{
				TaskAwaiter awaiter;
				float num2;
				float num3;
				RoutePoint obj5;
				switch (num)
				{
				default:
					if (Field_028D.Count >= 2)
					{
						Field_0290 = Field_028E;
						Field_0291 = true;
						Field_0292 = false;
						Field_0293 = null;
						Field_0294 = 0;
						Field_0295 = DateTime.Now;
						Field_0296 = float.MaxValue;
						Field_0297 = DateTime.MinValue;
						Field_0298 = new List<EntityState>();
						obj.Method_02BE(AddressCatalog.Property_0380, press: false);
						obj.Method_02BF(AddressCatalog.Property_0381, press: false);
						goto IL_0547;
					}
					goto end_IL_0013;
				case 0:
					awaiter = Field_0299;
					Field_0299 = default;
					num = (Field_028B = -1);
					goto IL_036c;
				case 1:
					awaiter = Field_0299;
					Field_0299 = default;
					num = (Field_028B = -1);
					goto IL_0540;
				case 2:
					{
						awaiter = Field_0299;
						Field_0299 = default;
						num = (Field_028B = -1);
						break;
					}
					IL_036c:
					awaiter.GetResult();
					goto IL_0547;
					IL_0547:
					while (obj.isFollowingRoute)
					{
						num2 = obj.memory.ReadFloat(AddressCatalog.Property_037B);
						num3 = obj.memory.ReadFloat(AddressCatalog.Property_037C);
						if (obj.farmAlongRoute)
						{
							MainForm obj2 = obj;
							if ((DateTime.Now - Field_0297).TotalMilliseconds > 1000.0)
							{
								Field_0298 = obj.ReadTargetEntities();
								Field_0297 = DateTime.Now;
							}
							RoutePoint obj3 = Field_028D[Field_0290];
							EntityState obj4 = Field_0298.Where((EntityState m) =>
							{

								return obj2.Distance3D(obj3.X, obj3.Y, 0f, m.X, m.Y, 0f) <= obj2.routeFarmRange;
							}).OrderBy((EntityState m) =>
							{

								return m.Distance;
							}).FirstOrDefault();
							if (obj4 != null)
							{
								if (obj.Distance3D(obj3.X, obj3.Y, 0f, obj4.X, obj4.Y, 0f) > obj.routeFarmRange)
								{
									Field_0292 = false;
									Field_0294 = 0;
									Field_0293 = null;
									obj.Method_02AF(0);
									continue;
								}
								if (!Field_0292 || Field_0294 != obj4.Property_03C9)
								{
									obj.Field_02EB = Field_0290;
									Field_0292 = true;
									Field_0293 = new RoutePoint
									{
										X = obj4.X,
										Y = obj4.Y
									};
									Field_0294 = obj4.Property_03C9;
									obj.Method_02AF(Field_0294);
									Field_0296 = obj4.Distance;
									Field_0295 = DateTime.Now;
								}
								else if ((DateTime.Now - Field_0295).TotalMilliseconds > 100.0)
								{
									float num4 = obj.Distance3D(num2, num3, 0f, obj4.X, obj4.Y, 0f);
									if (num4 >= Field_0296)
									{
										Field_0292 = false;
										Field_0294 = 0;
										Field_0293 = null;
										obj.Method_02AF(0);
										continue;
									}
									Field_0296 = num4;
									Field_0295 = DateTime.Now;
								}
								obj.Method_02C2(num2, num3, Field_0293.X, Field_0293.Y);
								awaiter = Task.Delay(10).GetAwaiter();
								if (!awaiter.IsCompleted)
								{
									num = (Field_028B = 0);
									Field_0299 = awaiter;
									Field_028C.AwaitUnsafeOnCompleted(ref awaiter, ref this);
									return;
								}
								goto IL_036c;
							}
							if (Field_0292)
							{
								Field_0294 = 0;
								Field_0292 = false;
								Field_0293 = null;
								obj.Method_02AF(0);
								float pLX = num2;
								float pLY = num3;
								List<int> list = new List<int>();
								for (int num5 = 0; num5 < Field_028D.Count; num5++)
								{
									if ((Field_0291 && num5 <= obj.Field_02EB) || (!Field_0291 && num5 >= obj.Field_02EB))
									{
										list.Add(num5);
									}
								}
								int num6 = obj.Field_02EB;
								float num7 = float.MaxValue;
								List<int>.Enumerator enumerator = list.GetEnumerator();
								try
								{
									while (enumerator.MoveNext())
									{
										int current = enumerator.Current;
										float num8 = obj.Distance3D(pLX, pLY, 0f, Field_028D[current].X, Field_028D[current].Y, 0f);
										if (num8 < num7)
										{
											num7 = num8;
											num6 = current;
										}
									}
								}
								finally
								{
									if (num < 0)
									{
										((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
									}
								}
								Field_0290 = num6;
								continue;
							}
						}
						obj5 = Field_028D[Field_0290];
						if (obj.Method_02C5(num2, num3, obj5.X, obj5.Y))
						{
							obj.Method_02C6(ref Field_0290, ref Field_0291, Field_028D.Count);
							continue;
						}
						goto IL_04d2;
					}
					obj.Method_02BE(AddressCatalog.Property_0380, press: false);
					awaiter = Task.Delay(10).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (Field_028B = 2);
						Field_0299 = awaiter;
						Field_028C.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
					break;
					IL_04d2:
					obj.Method_02C2(num2, num3, obj5.X, obj5.Y);
					awaiter = Task.Delay(10).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (Field_028B = 1);
						Field_0299 = awaiter;
						Field_028C.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
					goto IL_0540;
					IL_0540:
					awaiter.GetResult();
					goto IL_0547;
				}
				awaiter.GetResult();
				obj.Method_02BF(AddressCatalog.Property_0381, press: false);
				end_IL_0013:;
			}
			catch (Exception exception)
			{
				Field_028B = -2;
				Field_0293 = null;
				Field_0298 = null;
				Field_028C.SetException(exception);
				return;
			}
			Field_028B = -2;
			Field_0293 = null;
			Field_0298 = null;
			Field_028C.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{

			Field_028C.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private ProcessMemory memory = new ProcessMemory();

	private AutomationEngine automation;



	private List<RoutePoint> routePoints = new List<RoutePoint>();

	private bool Field_02DF;

	private bool isRecordingRoute;

	private Thread routeRecordingThread;

	private float lastRecordedX;

	private float lastRecordedY;

	private bool isFollowingRoute;

	private Random Field_02E5;

	private List<EntityState> radarEntities = new List<EntityState>();

	private DateTime lastRadarRefresh = DateTime.MinValue;

	private const float Field_02E8 = 0.1f;

	private const float Field_02E9 = 0.1f;

	private const float Field_02EA = 40f;

	private int Field_02EB;

	private bool farmAlongRoute;

	private float routeFarmRange = 3000f;

	private IContainer components;

	private Guna2Panel headerPanel;

	private Label rndTitleLbl;
	private Label brandLbl;

	private System.Windows.Forms.Timer titleTimer;

	private Guna2Panel leftPanel;

	private Guna2TabControl mainTabs;

	private TabPage tabMain;

	private TabPage tabDamage;

	private TabPage tabRoute;

	private Guna2CheckBox walkMetinCheck;

	private Label pidLbl;

	private Guna2Button startStopFarmBtn;

	private Guna2HtmlLabel guna2HtmlLabel1;

	private Guna2DragControl Field_02FB;

	private Guna2DragControl Field_02FC;

	private Guna2DragControl Field_02FD;

	private Guna2CheckBox walkCanavarCheck;

	private Guna2Button connectBtn;

	private Guna2ControlBox guna2ControlBox1;

	private Guna2ControlBox guna2ControlBox2;

	private Guna2DragControl Field_0302;

	private Panel radarPanel;

	private Guna2Button addRouteBtn;

	private Guna2Button clearRouteBtn;

	private Guna2Button startRouteMoveBtn;

	private Guna2TrackBar farmRangeTrackBar;

	private Guna2CheckBox farmOnRouteCheckBox;

	private Label farmRangeLabel;

	private Guna2CheckBox RotaMetinCheck;

	private Guna2CheckBox RotaCanavarCheck;

	private Guna2Button loadRouteBtn;

	private Guna2Button saveRouteBtn;

	private System.Windows.Forms.Timer Field_030E;

	private Guna2CheckBox waitDmgRangeCheck;

	private Guna2CheckBox waitDmgCheck;

	private Guna2CheckBox targetDmgRangeCheck;

	private Guna2CheckBox targetDmgCheck;

	private Guna2CheckBox waitArrowCheck;

	private Guna2CheckBox targetArrowCheck;

	private Guna2CheckBox autoPickCheck;

	private Guna2CheckBox dmgStartCheck;

	private Guna2TrackBar attackRangeBar;

	private Guna2TrackBar attackDelayBar;

	private Label attackRangeLbl;

	private Label attackDelayLbl;

	private Label pickDelayLbl;

	private Guna2TrackBar pickDelayBar;

	private Guna2CheckBox wallCheck;

	private Guna2CheckBox binekWallCheck;

	private Guna2CheckBox dmgStoneCheck;

	private Guna2CheckBox dmgMonsterCheck;

	private Guna2CheckBox dmgPlayerCheck;

	private static Support_0471 Field_0472;

	private static Support_0474 Field_0475;

	private static Support_0477 Field_0478;

	public static short Method_0473(int vKey)
	{
		if (Field_0472 == null)
		{
			int num = -1481646711 ^ -1611455336;
			char[] array = new char[10];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xF964 ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xE3F4 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xA29A ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x1B9D ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xA86E ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xA9C8 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0x98DD ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x9C86 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x59AC ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0x7702 ^ num2);
			string library = new string(array);
			int num3 = 0x3BC59769 ^ 0x7D852750;
			char[] array2 = new char[16];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0xB07E ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x71B2 ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x86EA ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x536C ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x3AAA ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x8E8A ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x528F ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x10A4 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x71BC ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0xA665 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0xFFDD ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x6EC2 ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0x5DCE ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0xF8F2 ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0xF31C ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x9B31 ^ num4);
			Field_0472 = (Support_0471)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0471));
		}
		return Field_0472(vKey);
	}

	public static IntPtr Method_0476()
	{
		if (Field_0475 == null)
		{
			int num = -122122091 ^ -329454918;
			char[] array = new char[10];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0x7A5A ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xE671 ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xAE00 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0x3C9 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0x9986 ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0xD098 ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xCD7D ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x15C0 ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x6751 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xD200 ^ num2);
			string library = new string(array);
			int num3 = 0x35CAF105 ^ 0x1CEEB4C5;
			char[] array2 = new char[19];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x4587 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0x9A4D ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x51CD ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0xFE34 ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x6B29 ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0x447F ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0x64F9 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0x855B ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0x876 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x1878 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x5809 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x315B ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xC2BF ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x77C ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x9057 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0x623F ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0xCBB4 ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0xA766 ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0x7C8C ^ num4);
			Field_0475 = (Support_0474)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0474));
		}
		return Field_0475();
	}

	public static uint Method_0479(IntPtr hWnd, out uint lpdwProcessId)
	{
		if (Field_0478 == null)
		{
			int num = 0x5FB99D15 ^ 0x1765732F;
			char[] array = new char[10];
			int num2 = ((num ^ 0) + 0) & 0xFFFF;
			array[0] = (char)(0xEE4F ^ num2);
			num2 = ((num ^ 0x8D1) + 3) & 0xFFFF;
			num2 = ((num2 << 1) | (num2 >>> 15)) & 0xFFFF;
			array[1] = (char)(0xCDAE ^ num2);
			num2 = ((num ^ 0x11A2) + 12) & 0xFFFF;
			num2 = ((num2 << 2) | (num2 >>> 14)) & 0xFFFF;
			array[2] = (char)(0xFEF6 ^ num2);
			num2 = ((num ^ 0x1A73) + 27) & 0xFFFF;
			num2 = ((num2 << 3) | (num2 >>> 13)) & 0xFFFF;
			array[3] = (char)(0xA355 ^ num2);
			num2 = ((num ^ 0x2344) + 48) & 0xFFFF;
			num2 = ((num2 << 4) | (num2 >>> 12)) & 0xFFFF;
			array[4] = (char)(0xDADF ^ num2);
			num2 = ((num ^ 0x2C15) + 75) & 0xFFFF;
			num2 = ((num2 << 5) | (num2 >>> 11)) & 0xFFFF;
			array[5] = (char)(0x4F6A ^ num2);
			num2 = ((num ^ 0x34E6) + 108) & 0xFFFF;
			num2 = ((num2 << 6) | (num2 >>> 10)) & 0xFFFF;
			array[6] = (char)(0xD218 ^ num2);
			num2 = ((num ^ 0x3DB7) + 147) & 0xFFFF;
			num2 = ((num2 << 7) | (num2 >>> 9)) & 0xFFFF;
			array[7] = (char)(0x100E ^ num2);
			num2 = ((num ^ 0x4688) + 192) & 0xFFFF;
			num2 = ((num2 << 8) | (num2 >>> 8)) & 0xFFFF;
			array[8] = (char)(0x72C5 ^ num2);
			num2 = ((num ^ 0x4F59) + 243) & 0xFFFF;
			num2 = ((num2 << 9) | (num2 >>> 7)) & 0xFFFF;
			array[9] = (char)(0xAD28 ^ num2);
			string library = new string(array);
			int num3 = -1136410615 ^ -807685478;
			char[] array2 = new char[24];
			int num4 = ((num3 ^ 0) + 0) & 0xFFFF;
			array2[0] = (char)(0x72D4 ^ num4);
			num4 = ((num3 ^ 0x8D1) + 3) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[1] = (char)(0xF4EF ^ num4);
			num4 = ((num3 ^ 0x11A2) + 12) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[2] = (char)(0x8C81 ^ num4);
			num4 = ((num3 ^ 0x1A73) + 27) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[3] = (char)(0x478C ^ num4);
			num4 = ((num3 ^ 0x2344) + 48) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[4] = (char)(0x201C ^ num4);
			num4 = ((num3 ^ 0x2C15) + 75) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[5] = (char)(0xDA45 ^ num4);
			num4 = ((num3 ^ 0x34E6) + 108) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[6] = (char)(0xB835 ^ num4);
			num4 = ((num3 ^ 0x3DB7) + 147) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[7] = (char)(0xDBC8 ^ num4);
			num4 = ((num3 ^ 0x4688) + 192) & 0xFFFF;
			num4 = ((num4 << 8) | (num4 >>> 8)) & 0xFFFF;
			array2[8] = (char)(0xDB43 ^ num4);
			num4 = ((num3 ^ 0x4F59) + 243) & 0xFFFF;
			num4 = ((num4 << 9) | (num4 >>> 7)) & 0xFFFF;
			array2[9] = (char)(0x7A29 ^ num4);
			num4 = ((num3 ^ 0x582A) + 300) & 0xFFFF;
			num4 = ((num4 << 10) | (num4 >>> 6)) & 0xFFFF;
			array2[10] = (char)(0x94C7 ^ num4);
			num4 = ((num3 ^ 0x60FB) + 363) & 0xFFFF;
			num4 = ((num4 << 11) | (num4 >>> 5)) & 0xFFFF;
			array2[11] = (char)(0x98EC ^ num4);
			num4 = ((num3 ^ 0x69CC) + 432) & 0xFFFF;
			num4 = ((num4 << 12) | (num4 >>> 4)) & 0xFFFF;
			array2[12] = (char)(0xF1B5 ^ num4);
			num4 = ((num3 ^ 0x729D) + 507) & 0xFFFF;
			num4 = ((num4 << 13) | (num4 >>> 3)) & 0xFFFF;
			array2[13] = (char)(0x2020 ^ num4);
			num4 = ((num3 ^ 0x7B6E) + 588) & 0xFFFF;
			num4 = ((num4 << 14) | (num4 >>> 2)) & 0xFFFF;
			array2[14] = (char)(0x4376 ^ num4);
			num4 = ((num3 ^ 0x843F) + 675) & 0xFFFF;
			num4 = ((num4 << 15) | (num4 >>> 1)) & 0xFFFF;
			array2[15] = (char)(0xFCF7 ^ num4);
			num4 = ((num3 ^ 0x8D10) + 768) & 0xFFFF;
			array2[16] = (char)(0x2F1 ^ num4);
			num4 = ((num3 ^ 0x95E1) + 867) & 0xFFFF;
			num4 = ((num4 << 1) | (num4 >>> 15)) & 0xFFFF;
			array2[17] = (char)(0xD5C4 ^ num4);
			num4 = ((num3 ^ 0x9EB2) + 972) & 0xFFFF;
			num4 = ((num4 << 2) | (num4 >>> 14)) & 0xFFFF;
			array2[18] = (char)(0xBFD4 ^ num4);
			num4 = ((num3 ^ 0xA783) + 1083) & 0xFFFF;
			num4 = ((num4 << 3) | (num4 >>> 13)) & 0xFFFF;
			array2[19] = (char)(0xCA3B ^ num4);
			num4 = ((num3 ^ 0xB054) + 1200) & 0xFFFF;
			num4 = ((num4 << 4) | (num4 >>> 12)) & 0xFFFF;
			array2[20] = (char)(0x770F ^ num4);
			num4 = ((num3 ^ 0xB925) + 1323) & 0xFFFF;
			num4 = ((num4 << 5) | (num4 >>> 11)) & 0xFFFF;
			array2[21] = (char)(0x1C49 ^ num4);
			num4 = ((num3 ^ 0xC1F6) + 1452) & 0xFFFF;
			num4 = ((num4 << 6) | (num4 >>> 10)) & 0xFFFF;
			array2[22] = (char)(0x4427 ^ num4);
			num4 = ((num3 ^ 0xCAC7) + 1587) & 0xFFFF;
			num4 = ((num4 << 7) | (num4 >>> 9)) & 0xFFFF;
			array2[23] = (char)(0x43BB ^ num4);
			Field_0478 = (Support_0477)Marshal.GetDelegateForFunctionPointer(NativeLibraryResolver.Method_0043(library, new string(array2), '\0'), typeof(Support_0477));
		}
		return Field_0478(hWnd, out lpdwProcessId);
	}

	public MainForm()
	{
		InitializeComponent();
		ApplyStudioTemplate();
        InitializeFarmOptions();
        InitializeRecommendations();
        InitializeInventoryPanel();
        InitializeConnectionSupervisor();
        InitializeClientPicker();
        InitializeSessionPanel();
        InitializeSessionAutomation();
        InitializeEnhancements();
        InitializeDeveloperLinkedAttack();
        InitializeFarmProfiles();
        InitializeCompatibilityPanel();
        InitializeTaskRecorder();
        InitializeGlobalHotkeys();
        InitializeMinimap();
        InitializeMapWalk();
        InitializeGameRestart();
        InitializeControlCenter();
        FinalizeProfessionalLayout();
        ApplyCompactLightTheme();
        InitializeAppearanceTheme();
        InitializeTargetHealth();
        InitializeRoutePatrol();
		Field_02E5 = new Random();
		UpdateWindowTitle();
		automation = new AutomationEngine(memory);
		walkCanavarCheck.CheckedChanged += (object s, EventArgs e) =>
		{
			if (walkCanavarCheck.Checked)
			{
				walkMetinCheck.Checked = false;
			}
			ApplyTargetFilters();
		};
		walkMetinCheck.CheckedChanged += (object s, EventArgs e) =>
		{
			if (walkMetinCheck.Checked)
			{
				walkCanavarCheck.Checked = false;
			}
			ApplyTargetFilters();
		};
		RotaCanavarCheck.CheckedChanged += (object s, EventArgs e) =>
		{
			if (RotaCanavarCheck.Checked)
			{
				RotaMetinCheck.Checked = false;
			}
		};
		RotaMetinCheck.CheckedChanged += (object s, EventArgs e) =>
		{
			if (RotaMetinCheck.Checked)
			{
				RotaCanavarCheck.Checked = false;
			}
		};
		ApplyTargetFilters();
        ApplyTurboAttackRuntime();
		InitializeV10Workspace();
	}

	private void OnTitleTimer(object sender, EventArgs e)
	{
		UpdateWindowTitle();
	}

	private static readonly Random _titleRng = new Random();

	private static string RandomGlyphs()
	{
		const string set = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%*<>?/|~@+=-";
		int n = 6 + _titleRng.Next(8);
		char[] a = new char[n];
		for (int i = 0; i < n; i++) a[i] = set[_titleRng.Next(set.Length)];
		return new string(a);
	}

	private void UpdateWindowTitle()
    {
        string r = RandomGlyphs();
        Text = r;
        if (rndTitleLbl != null) rndTitleLbl.Text = r;
        if (shellBrand != null) { shellBrand.Text = r; shellBrand.Invalidate(); }
    }

	private static bool IsFiniteCoordinate(float v)
	{
		if (!float.IsNaN(v) && !float.IsInfinity(v))
		{
			return Math.Abs(v) < 100000000f;
		}
		return false;
	}
    private bool IsClientReady()
    {
        if(emergencyLatched)return false;
        try
        {
            if (!clientConnected || memory.Field_0197?.Property_0268 == null || memory.Field_0197.Property_0268.HasExited) return false;
            if (IsX86ClientMode) return true;
            float x=memory.ReadFloat(AddressCatalog.Property_037B), y=memory.ReadFloat(AddressCatalog.Property_037C), z=memory.ReadFloat(AddressCatalog.Property_037D);
            if(IsFiniteCoordinate(x)&&IsFiniteCoordinate(y)&&IsFiniteCoordinate(z)&&(Math.Abs(x)>=0.01f||Math.Abs(y)>=0.01f))return true;
            return !string.IsNullOrEmpty(AddressCatalog.Property_0386)&&AddressCatalog.Property_0386!="0x0"&&ReadRadarEntities().Count>0;
        }
        catch { return false; }
    }

    private bool attackModeChange;
    private void OnAttackModeChanged(object sender, EventArgs e)
    {
        if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}
        var selected=sender as Guna2CheckBox;
        if(selected==null||attackModeChange)return;
        attackModeChange=true;
        try
        {
            if(WaitExploitEnabled){selected=waitDmgCheck;selected.Checked=true;}
            // Old profiles and manual mode changes cannot enable remote hits on the stone floor.
            if(taskDungeonRunning&&taskUiStage==3)selected=waitDmgCheck;
            var modes=new[]{waitDmgRangeCheck,waitDmgCheck,targetDmgRangeCheck,targetDmgCheck,waitArrowCheck,targetArrowCheck};
            if(selected.Checked||(taskDungeonRunning&&taskUiStage==3))
            {
                foreach(var mode in modes)if(mode!=selected)mode.Checked=false;
                selected.Checked=true;
            }
            automation.WaitDamageRange=waitDmgRangeCheck.Checked;
            automation.WaitDamage=waitDmgCheck.Checked;
            automation.TargetDamageRange=targetDmgRangeCheck.Checked;
            automation.TargetDamage=targetDmgCheck.Checked;
            automation.WaitArrow=waitArrowCheck.Checked;
            automation.TargetArrow=targetArrowCheck.Checked;
        }
        finally{attackModeChange=false;}
    }

	private void OnPickupChanged(object sender, EventArgs e)
	{
        if (IsX86ClientMode) { automation.AutoPickup=false; RequestX86BotState((dmgStartCheck.Checked||normalBotRequested)&&!MovementUsesWaitProtocol); return; }
		automation.AutoPickup = autoPickCheck.Checked;
        if (connecting) return;
		if (autoPickCheck.Checked && !automation.AttackEnabled)
		{
			automation.Start();
		}
        else if (!autoPickCheck.Checked && !automation.AttackEnabled) automation.Stop();
	}

	private void OnAutomationChanged(object sender, EventArgs e)
	{
        if(emergencyLatched){automation.AttackEnabled=false;automation.AutoPickup=false;return;}
        if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}
        if(WaitExploitEnabled){ApplyWaitExploitRuntime();return;}
        if (IsX86ClientMode) { automation.AttackEnabled=false; RequestX86BotState((dmgStartCheck.Checked||normalBotRequested)&&!MovementUsesWaitProtocol); return; }
		automation.AttackEnabled = dmgStartCheck.Checked;
        if (connecting) return;
		if (dmgStartCheck.Checked || autoPickCheck.Checked)
		{
			automation.Start();
		}
        else automation.Stop();
	}

	private void OnAttackRangeChanged(object sender, ScrollEventArgs e)
	{
		if(taskDungeonRunning&&taskUiStage==3&&attackRangeBar.Value>DungeonCloseAttackRange)attackRangeBar.Value=DungeonCloseAttackRange;
		automation.AttackRange = attackRangeBar.Value;
		attackRangeLbl.Text = "Menzil: " + attackRangeBar.Value;
	}

	private void OnAttackDelayChanged(object sender, ScrollEventArgs e)
	{
        if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}
        if(WaitExploitEnabled){ApplyWaitExploitRuntime();return;}
        if(TurboAttackEnabled&&!turboTimingChange)
        {
            if(attackDelayBar.Value!=ActiveTurboAttackDelay)turboPreviousDelay=attackDelayBar.Value;
            turboTimingChange=true;attackDelayBar.Value=ActiveTurboAttackDelay;turboTimingChange=false;ApplyTurboAttackRuntime();return;
        }
		automation.AttackDelay = attackDelayBar.Value;
		attackDelayLbl.Text = "Bekleme (ms): " + attackDelayBar.Value;
	}

	private void OnPickupDelayChanged(object sender, ScrollEventArgs e)
	{
		automation.PickupDelay = pickDelayBar.Value;
		pickDelayLbl.Text = "Bekleme (ms): " + pickDelayBar.Value;
	}

	private void OnTargetFilterChanged(object sender, EventArgs e)
	{
		ApplyTargetFilters();
	}

	private void ApplyTargetFilters()
	{
        if(RouteOwnsCombat){ApplyRouteAttackRuntime();return;}
		if (automation != null)
		{
            bool stonesOnly=taskDungeonRunning&&taskUiStage==3;
            automation.GroundStoneWait=stonesOnly&&!WaitExploitEnabled;
            automation.IncludeStones = WaitExploitEnabled||stonesOnly||dmgStoneCheck.Checked||(normalBotRequested&&walkMetinCheck.Checked);
            automation.IncludeMonsters = WaitExploitEnabled||(!stonesOnly&&(dmgMonsterCheck.Checked||(normalBotRequested&&walkCanavarCheck.Checked)));
            automation.IncludePlayers = !WaitExploitEnabled&&!stonesOnly&&dmgPlayerCheck.Checked;
		}
	}

	private void OnCollisionChanged(object sender, EventArgs e)
	{
		StartCollisionWorker();
		try
		{
			if (IsClientReady())
			{
                ApplyCollisionFlags();
			}
		}
		catch
		{
		}
	}

    private System.Windows.Forms.Timer collisionTimer;
    private void StartCollisionWorker()
    {
        if (collisionTimer == null)
        {
            collisionTimer = new System.Windows.Forms.Timer(components) { Interval = 40 };
            collisionTimer.Tick += (s, e) => {
                if (!IsClientReady()) return;
                try{ApplyCollisionFlags();}
                catch(Exception ex){if(RouteOwnsCombat)PauseRoutePatrol("Çarpışma ayarı bekleniyor: "+ex.Message);}
            };
        }
        if (wallCheck.Checked || binekWallCheck.Checked) collisionTimer.Start();
        else collisionTimer.Stop();
    }

    private void ApplyCollisionFlags(bool strict=false)
    {
        if(!IsClientReady())return;
        // A paused old route must never change another attached client/map.
        if(RouteOwnsCombat&&(memory.Field_0197.Property_0268.Id!=routeOwnerPid||atlasName!=routeRunningMap))return;
        bool hold=RouteOwnsCombat&&routeCollisionHoldVid>0;
        bool wall=wallCheck.Checked&&!hold,mount=binekWallCheck.Checked&&!hold;
        bool wallWritten=memory.WriteValue(AddressCatalog.Property_0383,"2bytes",wall?"1":"0");
        bool mountWritten=memory.WriteValue(AddressCatalog.Property_0384,"byte",mount?"1":"0");
        if(strict&&hold&&((wallCheck.Checked&&!wallWritten)||(binekWallCheck.Checked&&!mountWritten)))
            throw new InvalidOperationException("Taş yanında engel geçişi kapatılamadı; saldırı bekletildi.");
    }

	private string Method_02AD(Random rnd)
	{
		int num = rnd.Next(15, 31);
		char[] array = new char[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[rnd.Next("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)];
		}
		return new string(array);
	}

	private float Distance3D(float pLX, float pLY, float pLZ, float mX, float mY, float mZ)
	{
		return (float)Math.Sqrt(Math.Pow(pLX - mX, 2.0) + Math.Pow(pLY - mY, 2.0) + Math.Pow(pLZ - mZ, 2.0));
	}

	private void Method_02AF(int vid)
	{
        if (Field_02DF && (approachOnlyCheck.Checked||MovementUsesWaitProtocol)) vid = 0;
		if (vid != 0)
		{
			memory.WriteValue(AddressCatalog.Property_037F, "int", vid.ToString());
		}
		else
		{
			memory.WriteValue(AddressCatalog.Property_037F, "int", "0");
		}
	}

	public List<EntityState> ReadRadarEntities()
	{
        if(IsX86ClientMode)return radarEntities==null?new List<EntityState>():radarEntities.ToList();
		List<EntityState> list = new List<EntityState>();
		try
		{
			float pLX = memory.ReadFloat(AddressCatalog.Property_037B);
			float pLY = memory.ReadFloat(AddressCatalog.Property_037C);
			float pLZ = memory.ReadFloat(AddressCatalog.Property_037D);
			long num = Convert.ToInt64(AddressCatalog.Property_0386.Replace(",", ""), 16);
			long num2 = memory.ReadInt64(string.Format("0x{0:X}", num));
			long num3 = memory.ReadInt64(string.Format("0x{0:X}", num + 8));
			if (num2 == 0L || num3 == 0L || num3 < num2)
			{
				return list;
			}
			int num4 = 0;
			long num5 = num2;
			while (num5 < num3)
			{
				long num6 = memory.ReadInt64(string.Format("0x{0:X}", num5));
				if (num6 != 0L)
				{
					try
					{
						int num7 = Convert.ToInt32(AddressCatalog.Field_0378.Replace(",", ""), 16);
						int num8 = Convert.ToInt32(AddressCatalog.Field_0377.Replace(",", ""), 16);
						int num9 = Convert.ToInt32(AddressCatalog.Field_0374.Replace(",", ""), 16);
						int num10 = Convert.ToInt32(AddressCatalog.Field_0375.Replace(",", ""), 16);
						int num11 = Convert.ToInt32(AddressCatalog.Field_0376.Replace(",", ""), 16);
						int num12 = Convert.ToInt32(AddressCatalog.Field_0379.Replace(",", ""), 16);
						int num13 = memory.ReadInt32(string.Format("0x{0:X}", num6 + num7));
						int num14 = memory.ReadInt32(string.Format("0x{0:X}", num6 + num8));
						float num15 = memory.ReadFloat(string.Format("0x{0:X}", num6 + num9));
						float num16 = memory.ReadFloat(string.Format("0x{0:X}", num6 + num10));
						float num17 = memory.ReadFloat(string.Format("0x{0:X}", num6 + num11));
						int num18 = memory.ReadInt32(string.Format("0x{0:X}", num6 + num12));
						if (num14 != 0 && (num13 == AddressCatalog.Field_035B || num13 == AddressCatalog.Field_035C || num13 == AddressCatalog.Field_035A || num13 == 1) && num18 == 0 && IsFiniteCoordinate(num15) && IsFiniteCoordinate(num16) && IsFiniteCoordinate(num17))
						{
							list.Add(new EntityState
							{
								Property_03C4 = (int)num6,
								X = num15,
								Y = num16,
								Z = num17,
								Property_03C9 = num14,
								Property_03CA = num13,
								Property_03CB = num18,
								Distance = Distance3D(pLX, pLY, pLZ, num15, num16, num17)
							});
						}
					}
					catch (Exception)
					{
						DebugGuard.Method_0008();
					}
				}
				num5 += 8;
				num4++;
			}
			list.Sort((EntityState x, EntityState y) =>
			{

				return x.Distance.CompareTo(y.Distance);
			});
		}
		catch (Exception)
		{
			DebugGuard.Method_0008();
		}
		return list;
	}

	private List<EntityState> ReadTargetEntities()
	{
		return (from m in ReadRadarEntities()
			where (walkMetinCheck.Checked && m.Property_03CA == AddressCatalog.Field_035B) || (RotaMetinCheck.Checked && m.Property_03CA == AddressCatalog.Field_035B) || (RotaCanavarCheck.Checked && m.Property_03CA == AddressCatalog.Field_035C) || (walkCanavarCheck.Checked && m.Property_03CA == AddressCatalog.Field_035C)
			select m).OrderBy((EntityState m) =>
		{
			DebugGuard.Method_0008();

			return m.Distance;
		}).ToList();
	}

    private async void OnFarmClick(object sender, EventArgs e)
    {
        if(emergencyLatched)return;
        if(mainTabs.SelectedTab==tabRoute){ToggleRoutePatrol();UpdateContextActionButton();return;}
        if(mainTabs.SelectedTab==tabDamage){if(RouteOwnsCombat||taskDungeonRunning||normalBotRequested||Field_02DF||developerWaitRunning||packEngineRunning){MessageBox.Show("Çalışan rota/görev/testi kendi bölümünden durdur.");return;}if(!dmgStartCheck.Checked&&!IsClientReady()){MessageBox.Show("Önce oyuna bağlan.");return;}dmgStartCheck.Checked=!dmgStartCheck.Checked;UpdateContextActionButton();return;}
        if(mainTabs.SelectedTab!=tabMain&&mainTabs.SelectedTab!=taskPage)return;
        if(taskPage==null||mainTabs.SelectedTab!=taskPage){ToggleNormalBot();return;}
        if (taskCancellation != null)
        {
            StopTaskSequence("Kellam görevi kullanıcı tarafından durduruldu.");
            UpdateContextActionButton();
            return;
        }
        if (!IsClientReady()) { MessageBox.Show("Önce Oyuna bağlan düğmesine basın."); return; }
        UpdateContextActionButton();
        try { await RunTaskSequenceAsync(false); }
        finally { if (!IsDisposed && !Disposing) UpdateContextActionButton(); }
    }
    private System.Windows.Forms.Timer radarTimer;
    private void StartRadarWorker()
    {
        if (radarTimer == null)
        {
            radarTimer = new System.Windows.Forms.Timer(components) { Interval = 250 };
            radarTimer.Tick += async (s, e) => {
                if (IsDisposed || Disposing || !IsClientReady()) return;
                if(IsX86ClientMode){await RefreshX86RadarAsync();return;}
                try { radarEntities = ReadRadarEntities(); RenderRadar(radarEntities); }
                catch (Exception ex) { farmStatus.Text = "Radar: " + ex.Message; }
            };
        }
        radarTimer.Interval=IsX86ClientMode?1000:250; radarTimer.Start();
    }

	private void OnRouteStartClick(object sender, EventArgs e)
	{
        ToggleRoutePatrol();
	}

	private int FindNearestRoutePoint(float playerX, float playerY)
	{
		int result = 0;
		float num = float.MaxValue;
		for (int i = 0; i < routePoints.Count; i++)
		{
			float num2 = playerX - routePoints[i].X;
			float num3 = playerY - routePoints[i].Y;
			float num4 = num2 * num2 + num3 * num3;
			if (num4 < num)
			{
				num = num4;
				result = i;
			}
		}
		return result;
	}

	[AsyncStateMachine(typeof(Type_028A))]
	private void Method_02B8(List<RoutePoint> route, int startIndex)
	{
		Type_028A stateMachine = default;
		stateMachine.Field_028C = AsyncVoidMethodBuilder.Create();
		stateMachine.Field_028F = this;
		stateMachine.Field_028D = route;
		stateMachine.Field_028E = startIndex;
		stateMachine.Field_028B = -1;
		stateMachine.Field_028C.Start(ref stateMachine);
	}

	private void Method_02B9()
	{
		if (routePoints.Count == 0)
		{
			return;
		}
		float playerX = memory.ReadFloat(AddressCatalog.Property_037B);
		float playerY = memory.ReadFloat(AddressCatalog.Property_037C);
		int startIndex = FindNearestRoutePoint(playerX, playerY);
		Thread thread = new Thread(() =>
		{

			Method_02B8(routePoints, startIndex);
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void OnRouteRecordClick(object sender, EventArgs e)
	{
        ToggleRouteRecording();
	}

	private void OnRouteClearClick(object sender, EventArgs e)
	{
        if(RouteOwnsCombat)StopRoutePatrol("Rota temizlendi.");
        StopRouteRecording(false);ResetRouteExtras();routePoints.Clear();routeMapName="";routePatrolStatus.Text="Rota noktaları ve rotaya ait güvenlik işaretleri temizlendi.";RefreshRouteDrawing();
	}

	private float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle < 0f)
		{
			angle += 360f;
		}
		return angle;
	}

	private float GetTargetAngle(float playerX, float playerY, float targetX, float targetY)
	{
		float num = targetX - playerX;
		float num2 = (float)(Math.Atan2(targetY - playerY, num) * (180.0 / Math.PI));
		num2 = 90f - num2;
		return NormalizeAngle(num2);
	}

	private void Method_02BE(string address, bool press)
	{
		memory.WriteValue(address, "2bytes", press ? "1" : "0");
	}

	private void Method_02BF(string address, bool press)
	{
		memory.WriteValue(address, "2bytes", press ? "256" : "0");
	}

	private void Method_02C0(float angle)
	{
		float num = NormalizeAngle(angle);
		memory.WriteValue(AddressCatalog.Property_0382, "float", num.ToString("F7", CultureInfo.InvariantCulture));
	}

	private float Method_02C1(float from, float to)
	{
		return (to - from + 540f) % 360f - 180f;
	}

	private void Method_02C2(float playerX, float playerY, float targetX, float targetY)
	{
        if(emergencyLatched){ReleaseMovement();return;}
		float to = GetTargetAngle(playerX, playerY, targetX, targetY);
		float num = NormalizeAngle(memory.ReadFloat(AddressCatalog.Property_0382));
		float value = Method_02C1(num, to);
		float num2 = Math.Abs(value);
		if (num2 > 8f)
		{
			if(Field_02DF||RouteOwnsCombat) ReleaseMovement();
            else { Method_02BE(AddressCatalog.Property_0380, press:false); Method_02BF(AddressCatalog.Property_0381, press:false); }
			float value2 = 0.1f + 39.9f * (num2 / 180f);
			value2 = Method_02C4(value2, 0.1f, 40f);
			float num3 = (float)Math.Sign(value) * Math.Min(num2, value2);
			float angle = NormalizeAngle(num + num3);
			Method_02C0(angle);
		}
		else
		{
			if (num2 > 0.1f)
			{
				float num4 = (float)Math.Sign(value) * Math.Min(num2, 6f);
				Method_02C0(NormalizeAngle(num + num4));
			}
			if(Field_02DF||RouteOwnsCombat) movementOutput.Move(movementClock.ElapsedMilliseconds);
            else { Method_02BE(AddressCatalog.Property_0380, press:true); Method_02BF(AddressCatalog.Property_0381, press:true); }
		}
	}

	private float Method_02C3(float x1, float y1, float x2, float y2)
	{
		float num = x1 - x2;
		float num2 = y1 - y2;
		return num * num + num2 * num2;
	}

	private float Method_02C4(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	private bool Method_02C5(float x, float y, float tx, float ty)
	{
		float num = 600f;
		if (x >= tx - num && x <= tx + num && y >= ty - num)
		{
			return y <= ty + num;
		}
		return false;
	}

	private void Method_02C6(ref int index, ref bool backward, int count)
	{
		if (backward)
		{
			index--;
			if (index < 0)
			{
				index = 0;
				backward = false;
			}
		}
		else
		{
			index++;
			if (index >= count)
			{
				index = count - 1;
				backward = true;
			}
		}
	}

	private void OnRouteRangeChanged(object sender, ScrollEventArgs e)
	{
		routeFarmRange = farmRangeTrackBar.Value;
		farmRangeLabel.Text = string.Format("Rota koridor yarıçapı: {0} · toplam genişlik: {1}", (int)routeFarmRange, (int)(routeFarmRange * 2));
		RefreshRouteDrawing();
	}

	private void OnRouteFarmChanged(object sender, EventArgs e)
	{
		farmAlongRoute = farmOnRouteCheckBox.Checked;
	}

	private void OnRouteSaveClick(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Rota dosyası|*.txt";
		saveFileDialog.Title = "Rotayı kaydet";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			SaveRoute(saveFileDialog.FileName);
		}
	}

	private void OnRouteLoadClick(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Rota dosyası|*.txt";
		openFileDialog.Title = "Rota aç";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			LoadRoute(openFileDialog.FileName);
		}
	}

	private void SaveRoute(string filePath)
	{
		try
		{
			RouteFileStore.Save(filePath, routePoints,routeMapName);
			MessageBox.Show("Rota kaydedildi.", "Kaydet", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Kaydedilemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}

	private void LoadRoute(string filePath)
	{
        if(RouteOwnsCombat||isRecordingRoute){MessageBox.Show("Rota açmadan önce mevcut kayıt/takibi durdur.");return;}
		try
		{
			var loaded = RouteFileStore.Load(filePath);
			ResetRouteExtras();
			routePoints.Clear();
			routePoints.AddRange(loaded);
            routeMapName=RouteFileStore.LoadMap(filePath);routePatrolStatus.Text=loaded.Count+" nokta açıldı · "+(string.IsNullOrEmpty(routeMapName)?"eski rota: haritasını kontrol et":routeMapName);RefreshRouteDrawing();
			MessageBox.Show("Rota açıldı.", "Aç", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Açılamadı:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}

	private void Method_02CD(object sender, EventArgs e)
	{
	}


	protected override void Dispose(bool disposing)
	{
        if (disposing)
        {
            StopRoutePatrol("Uygulama kapanıyor.");
            StopPackEngineForDispose();
            StopSessionLease();
            sessionBridge?.Dispose();
            StopMovementFarm();
            movementWatchdog?.Dispose();
            isRecordingRoute = false;
            isFollowingRoute = false;
            automation?.Stop();
        }
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}



	[CompilerGenerated]
	private void Method_02D1(object s, EventArgs e)
	{
		if (walkCanavarCheck.Checked)
		{
			walkMetinCheck.Checked = false;
		}
	}

	[CompilerGenerated]
	private void Method_02D2(object s, EventArgs e)
	{
		if (walkMetinCheck.Checked)
		{
			walkCanavarCheck.Checked = false;
		}
	}

	[CompilerGenerated]
	private void Method_02D3(object s, EventArgs e)
	{
		if (RotaCanavarCheck.Checked)
		{
			RotaMetinCheck.Checked = false;
		}
	}

	[CompilerGenerated]
	private void Method_02D4(object s, EventArgs e)
	{
		if (RotaMetinCheck.Checked)
		{
			RotaCanavarCheck.Checked = false;
		}
	}

	[CompilerGenerated]
	private bool Method_02D5(EntityState m)
	{
		if ((!walkMetinCheck.Checked || m.Property_03CA != AddressCatalog.Field_035B) && (!RotaMetinCheck.Checked || m.Property_03CA != AddressCatalog.Field_035B) && (!RotaCanavarCheck.Checked || m.Property_03CA != AddressCatalog.Field_035C))
		{
			if (walkCanavarCheck.Checked)
			{
				return m.Property_03CA == AddressCatalog.Field_035C;
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	private bool Method_02D6(EntityState m)
	{
		if (!walkMetinCheck.Checked || m.Property_03CA != AddressCatalog.Field_035B)
		{
			if (walkCanavarCheck.Checked)
			{
				return m.Property_03CA == AddressCatalog.Field_035C;
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	private void Method_02D7()
	{
		while (true)
		{
			if (pidLbl.Text != "0")
			{
				if ((DateTime.Now - lastRadarRefresh).TotalMilliseconds >= 500.0)
				{
					radarEntities = ReadRadarEntities();
					lastRadarRefresh = DateTime.Now;
				}
				radarPanel.Invoke((Action)(() =>
				{
					RenderRadar(radarEntities);
				}));
			}
			Thread.Sleep(10);
		}
	}

	[CompilerGenerated]
	private void Method_02D8()
	{
		RenderRadar(radarEntities);
	}

	[CompilerGenerated]
	private void Method_02D9()
	{
		while (isRecordingRoute)
		{
			try
			{
				float num = memory.ReadFloat(AddressCatalog.Property_037B);
				float num2 = memory.ReadFloat(AddressCatalog.Property_037C);
				if (routePoints.Count == 0)
				{
					routePoints.Add(new RoutePoint
					{
						X = num,
						Y = num2
					});
					lastRecordedX = num;
					lastRecordedY = num2;
				}
				else if ((float)Math.Sqrt(Math.Pow(num - lastRecordedX, 2.0) + Math.Pow(num2 - lastRecordedY, 2.0)) > 1f)
				{
					routePoints.Add(new RoutePoint
					{
						X = num,
						Y = num2
					});
					lastRecordedX = num;
					lastRecordedY = num2;
				}
			}
			catch
			{
			}
			Thread.Sleep(10);
		}
	}
}










