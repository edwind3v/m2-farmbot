using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;

public sealed class AutomationEngine
{
    private const int WaitEntityScanLimit = 4096;
	private struct Type_03EF
	{
        public long ActorAddress;
		public uint Field_03F0;

		public int Field_03F1;

		public int Field_03F2;

		public float Field_03F3;

		public float Field_03F4;

		public float Field_03F5;
	}

	private readonly ProcessMemory Field_040E;

	private readonly GameCallBridge Field_040F;

	private Thread Field_0410;

	private volatile bool Field_0411;

	private long Field_0412;

	public volatile bool AttackEnabled;
    public volatile bool RecoveryBlocked;
    public Func<bool> ActionPaused;
    private bool SkillPaused { get { return ActionPaused!=null && ActionPaused(); } }
    public volatile bool EmergencyBlocked;
    public volatile bool FastTiming;
    public string LastError { get; private set; }
    public bool WorkerAlive { get { return Field_0410 != null && Field_0410.IsAlive; } }
    private long completedAttackCalls, lastCycleMilliseconds, lastAttackTargetCount;
    public long CompletedAttackCalls { get { return Interlocked.Read(ref completedAttackCalls); } }
    public long LastCycleMilliseconds { get { return Interlocked.Read(ref lastCycleMilliseconds); } }
    public long LastAttackTargetCount { get { return Interlocked.Read(ref lastAttackTargetCount); } }
    private bool TrackAttackResult(bool success) {
        if(success) Interlocked.Increment(ref completedAttackCalls);
        else LastError="Saldırı çağrısı tamamlanamadı: "+Field_040F.Property_03EE;
        return success;
    }
    private readonly object workerLock = new object();
    private readonly ManualResetEvent stopSignal = new ManualResetEvent(false);

	public volatile bool WaitDamageRange;

	public volatile bool WaitDamage;
    // Ground distance is scoped to the four-stone floor, with its existing short range.
    public volatile bool GroundStoneWait;

	public volatile bool TargetDamageRange;

	public volatile bool TargetDamage;

	public volatile bool WaitArrow;

	public volatile bool TargetArrow;

	public volatile bool AutoPickup;

	public volatile bool IncludeStones = true;

	public volatile bool IncludeMonsters = true;

	public volatile bool IncludePlayers;

	public volatile int AttackRange = 100;
	// 0 = unlimited. Caps how many targets one Wait cycle hits; a burst over every target in range can crash the client.
	public volatile int MaxTargetsPerCycle;
    public volatile int FocusTargetVid;
    public volatile bool FocusStoneMonsterSplash;
    private int splashCursor;

    private bool AcceptsFocus(uint vid)
    {
        int focus=FocusTargetVid;
        return focus==0||vid==unchecked((uint)focus);
    }

	public volatile int AttackDelay = 1;

	public volatile int PickupDelay = 1;

	public AutomationEngine(ProcessMemory mem)
	{
		Field_040E = mem;
		Field_040F = new GameCallBridge(mem);
	}

    public void Start()
    {
        if(EmergencyBlocked){AttackEnabled=false;AutoPickup=false;return;}
        lock (workerLock)
        {
            if (Field_0410 != null && Field_0410.IsAlive) return;
            stopSignal.Reset();
            Field_0411 = true;
            LastError = null;
            Field_0410 = new Thread(Method_03F9) { IsBackground = true, Name = "Automation" };
            Field_0410.Start();
        }
    }

    public void Stop()
    {
        lock (workerLock)
        {
            Field_0411 = false;
            AttackEnabled = false;
            AutoPickup = false;
            stopSignal.Set();
            if (Field_0410 != null && Field_0410 != Thread.CurrentThread)
                Field_0410.Join(1000);
        }
    }

    private void Method_03F9()
    {
        using (var resolution = new TimingResolution())
        {
        var clock = Stopwatch.StartNew();
        long nextAttack = 0;
        long nextPickup = 0;
        while (Field_0411)
        {
            try
            {
                resolution.SetEnabled(FastTiming && (AttackEnabled || AutoPickup));
                long now = clock.ElapsedMilliseconds;
                if (AttackEnabled && !RecoveryBlocked && !EmergencyBlocked && !SkillPaused && now >= nextAttack)
                {
                    long started = now;
                    if (TargetDamage) Method_0405(false, false);
                    else if (TargetDamageRange) Method_0405(true, false);
                    else if (TargetArrow) Method_0405(false, true);
                    else if (WaitDamageRange) Method_0406(true, false);
                    else if (WaitDamage) Method_0406(false, false);
                    else if (WaitArrow) Method_0406(false, true);
                    if(FocusStoneMonsterSplash&&(TargetDamage||TargetDamageRange))WaitCycle(TargetDamageRange,false,true);
                    Interlocked.Exchange(ref lastCycleMilliseconds, clock.ElapsedMilliseconds-started);
                    nextAttack = AutomationTiming.NextDue(started, clock.ElapsedMilliseconds, AttackDelay, FastTiming);
                }
                if (AutoPickup && !RecoveryBlocked && !EmergencyBlocked && !SkillPaused && Field_0411 && clock.ElapsedMilliseconds >= nextPickup)
                {
                    Method_0407();
                    nextPickup = clock.ElapsedMilliseconds + AutomationTiming.ClampDelay(PickupDelay);
                }
                long attackWait = AttackEnabled && !RecoveryBlocked ? Math.Max(1, nextAttack - clock.ElapsedMilliseconds) : 50;
                long pickupWait = AutoPickup && !RecoveryBlocked ? Math.Max(1, nextPickup - clock.ElapsedMilliseconds) : 50;
                if (stopSignal.WaitOne((int)Math.Min(50, Math.Min(attackWait, pickupWait)))) break;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                if (stopSignal.WaitOne(80)) break;
            }
        }
        }
    }

	private long Method_03FA(long a)
	{

		if (a != 0L)
		{
			return Field_040E.ReadInt64(RuntimeAdapters.Support_04A2("0x{0:X}", a));
		}
		return 0L;
	}

	private long Method_03FB()
	{
		DebugGuard.Method_0008();

		return Method_03FA(AddressCatalog.Field_036E);
	}

	private long Method_03FC()
	{
		IntegrityGuard.Method_0035();

		return Method_03FA(AddressCatalog.Field_036F);
	}

	internal static bool Method_03FD(uint v)
	{
		DebugGuard.Method_0008();

		if (v != 0 && v != uint.MaxValue)
		{
			return v < (0x5F5E564 ^ 0x59B);
		}
		return false;
	}

	internal static bool Method_03FE(float v)
	{

		if (!RuntimeAdapters.Support_04A3(v) && !RuntimeAdapters.Support_04A4(v))
		{
			return RuntimeAdapters.Support_04A5(v) < 100000000f;
		}
		return false;
	}

	internal static bool Method_03FF(float x, float y, float z)
	{

		if (!RuntimeAdapters.Support_04A6(x) || !RuntimeAdapters.Support_04A6(y) || !RuntimeAdapters.Support_04A6(z))
		{
			return false;
		}
		if (RuntimeAdapters.Support_04A5(x) < 0.01f && RuntimeAdapters.Support_04A5(y) < 0.01f)
		{
			return false;
		}
		return true;
	}

	private bool Method_0400(int type)
	{

		if (type == AddressCatalog.Field_035B)
		{
			return IncludeStones;
		}
		if (type == AddressCatalog.Field_035C)
		{
			return IncludeMonsters;
		}
		if (type == AddressCatalog.Field_035A)
		{
			return IncludePlayers;
		}
		return false;
	}

	private uint Method_0401()
	{

		long num = Method_03FC();
		if (num == 0L || num < (0x10121 ^ 0x121))
		{
			return 0u;
		}
		return (uint)Field_040E.ReadInt32(RuntimeAdapters.Support_04A2("0x{0:X}", num + (0xEB5 ^ 0xF15)));
	}

	private void Method_0402(uint vid)
	{
        if (!Field_0411 || !AttackEnabled || RecoveryBlocked || EmergencyBlocked || SkillPaused) return;

		long num = Method_03FB();
		if (num != 0L && AddressCatalog.Field_0369 != 0L)
		{
			TrackAttackResult(Field_040F.Method_03D2(AddressCatalog.Field_0369, num, vid));
		}
	}

	private void Method_0403(uint vid, float x, float y, float z)
	{
        if (!Field_0411 || !AttackEnabled || RecoveryBlocked || EmergencyBlocked || SkillPaused) return;
		IntegrityGuard.Method_0035();

		if (RuntimeAdapters.Support_04A7(x, y, z))
		{
			long num = Method_03FB();
			if (num != 0L && AddressCatalog.Field_036B != 0L && AddressCatalog.Field_036C != 0L)
			{
				TrackAttackResult(Field_040F.Method_03D5(AddressCatalog.Field_036B, AddressCatalog.Field_036C, num, vid, x, y, z, 0));
			}
		}
	}

	private bool Method_0404(uint vid, float x, float y, float z)
	{
        if (!Field_0411 || !AttackEnabled || RecoveryBlocked || EmergencyBlocked || SkillPaused) return false;

		if (!RuntimeAdapters.Support_04A7(x, y, z))
		{
			return false;
		}
		long num = Method_03FB();
		if (num == 0L || AddressCatalog.Field_036A == 0L || AddressCatalog.Field_0369 == 0L)
		{
			return false;
		}
		if (!Field_040F.Method_03D6(AddressCatalog.Field_036A, num, x, y, z))
		{
			return false;
		}
		return TrackAttackResult(Field_040F.Method_03D2(AddressCatalog.Field_0369, num, vid));
	}

	private void Method_0405(bool range, bool arrow)
	{
		if ((0xFC ^ 0xFF9) != (5 ^ 0xF00))
		{
			_ = 1;
		}

		uint num = Method_0401();
        if(FocusTargetVid>0)num=unchecked((uint)FocusTargetVid);
		if (!RuntimeAdapters.Support_04A8(num))
		{
			return;
		}
		if (!Method_040A(num, out var x, out var y, out var z, out var dead, out var type))
		{
			if (arrow)
			{
			}
		}
		else
		{
			if (dead != 0 || !Method_0400(type))
			{
				return;
			}
			if (!arrow && !range)
			{
				Method_0402(num);
			}
			else
			{
				if (!RuntimeAdapters.Support_04A7(x, y, z))
				{
					return;
				}
				if (range)
				{
					if (!Method_0408(out var x2, out var y2, out var z2) || !RuntimeAdapters.Support_04A7(x2, y2, z2))
					{
						return;
					}
					float num2 = RuntimeAdapters.Support_04A9(x2, y2, z2, x, y, z);
					if (num2 > (float)AttackRange || num2 < 0.01f)
					{
						return;
					}
				}
				if (arrow)
				{
					Method_0403(num, x, y, z);
				}
				else
				{
					Method_0404(num, x, y, z);
				}
			}
		}
	}

	private void Method_0406(bool range, bool arrow)
    {
        WaitCycle(range,arrow,false);
    }
    private void WaitCycle(bool range,bool arrow,bool skipFocusedStone)
	{
		if (Method_03FB() == 0L || !Method_0408(out var x, out var y, out var z) || !Method_03FF(x, y, z))
		{
			return;
		}
		int max = WaitEntityScanLimit;
		int cap = MaxTargetsPerCycle;
        int cycleFocus=FocusTargetVid;bool splash=FocusStoneMonsterSplash;
        var actors=Method_0409(max).ToList();
        var anchor=splash?actors.FirstOrDefault(a=>a.Field_03F0==(uint)cycleFocus):default(Type_03EF);
        float stoneX=anchor.Field_03F3,stoneY=anchor.Field_03F4;int stoneType=anchor.Field_03F1;
        if(splash&&(cycleFocus<=0||anchor.Field_03F0!=(uint)cycleFocus||anchor.Field_03F2!=0||stoneType!=AddressCatalog.Field_035B)){
            Interlocked.Exchange(ref lastAttackTargetCount,0);return;
        }
		var targets = new List<KeyValuePair<float, Type_03EF>>();
		foreach (Type_03EF item in actors)
		{
            if(skipFocusedStone&&item.Field_03F0==(uint)cycleFocus)continue;
            double fromStone=RoutePatrol.Distance(item.Field_03F3,item.Field_03F4,stoneX,stoneY);
            if(!RouteAttackPolicy.AcceptWaitTarget(cycleFocus,true,stoneType,(int)item.Field_03F0,item.Field_03F1,splash,fromStone,AttackRange))continue;
			if (!Method_03FD(item.Field_03F0) || item.Field_03F2 != 0 || !Method_0400(item.Field_03F1) || !Method_03FF(item.Field_03F3, item.Field_03F4, item.Field_03F5))
			{
				continue;
			}
            bool groundStone=GroundStoneWait&&!range&&!arrow&&item.Field_03F1==AddressCatalog.Field_035B;
            float num=groundStone?Method_040D(x,y,0,item.Field_03F3,item.Field_03F4,0):Method_040D(x,y,z,item.Field_03F3,item.Field_03F4,item.Field_03F5);
            // A stone touching the character remains a valid Wait target even at zero XY separation.
            if((groundStone||num>=0.01f)&&num<=AttackRange)targets.Add(new KeyValuePair<float,Type_03EF>(num,item));
		}
        if(splash){
            targets.Sort((a,b)=>a.Key.CompareTo(b.Key));
            var ids=RouteAttackPolicy.SelectCycle(targets.Select(t=>(int)t.Value.Field_03F0),cycleFocus,cap,ref splashCursor);
            var lookup=targets.GroupBy(t=>(int)t.Value.Field_03F0).ToDictionary(g=>g.Key,g=>g.First());
            targets=ids.Select(id=>lookup[id]).ToList();
        }else if (cap > 0) { targets.Sort((a, b) => a.Key.CompareTo(b.Key)); if (targets.Count > cap) targets.RemoveRange(cap, targets.Count - cap); }
		Interlocked.Exchange(ref lastAttackTargetCount, targets.Count);
		foreach (var target in targets)
		{
			var item = target.Value;
            if (!Field_0411 || !AttackEnabled || RecoveryBlocked || EmergencyBlocked || SkillPaused) break;
            if(splash){
                Type_03EF currentStone;
                if(!FocusStoneMonsterSplash||FocusTargetVid!=cycleFocus||
                    !Method_040B(anchor.ActorAddress,out currentStone)||currentStone.Field_03F0!=(uint)cycleFocus||
                    currentStone.Field_03F2!=0||currentStone.Field_03F1!=AddressCatalog.Field_035B)break;
                stoneX=currentStone.Field_03F3;stoneY=currentStone.Field_03F4;stoneType=currentStone.Field_03F1;
                if(!RouteAttackPolicy.AcceptWaitTarget(cycleFocus,true,stoneType,(int)item.Field_03F0,item.Field_03F1,true,RoutePatrol.Distance(item.Field_03F3,item.Field_03F4,stoneX,stoneY),AttackRange))continue;
            }else if(!AcceptsFocus(item.Field_03F0))continue;
			{
				if (arrow)
				{
					Method_0403(item.Field_03F0, item.Field_03F3, item.Field_03F4, item.Field_03F5);
					if (stopSignal.WaitOne(FastTiming ? AutomationTiming.ClampDelay(AttackDelay) : Math.Max(8, AttackDelay))) return;
				}
				else if (range)
				{
					Method_0404(item.Field_03F0, item.Field_03F3, item.Field_03F4, item.Field_03F5);
					// Spread capped hits over time instead of firing them all in the same instant.
					if (cap > 0 && stopSignal.WaitOne(15)) return;
				}
				else
				{
					Method_0402(item.Field_03F0);
				}
			}
		}
	}

	private void Method_0407()
	{
        if(RecoveryBlocked||EmergencyBlocked||SkillPaused)return;
		DebugGuard.Method_0008();

		int num = PickupDelay;
		if (num < 1)
		{
			num = 1;
		}
		if (num > (0x1279 ^ 0x1191))
		{
			num = 0x3AA ^ 0x42;
		}
		long num2 = RuntimeAdapters.Support_04AA() & 0xFFFFFFFFu;
		if (num2 - Field_0412 >= num || num2 < Field_0412)
		{
			long num3 = Method_03FC();
			if (num3 != 0L && num3 >= (0x10FEF ^ 0xFEF) && AddressCatalog.Field_036D != 0L && Field_040F.Method_03D4(AddressCatalog.Field_036D, num3))
			{
				Field_0412 = num2;
			}
		}
	}

	private bool Method_0408(out float x, out float y, out float z)
	{
		x = (y = (z = 0f));
		try
		{
			x = Field_040E.ReadFloat(AddressCatalog.Property_037B);
			y = Field_040E.ReadFloat(AddressCatalog.Property_037C);
			z = Field_040E.ReadFloat(AddressCatalog.Property_037D);
			if (Method_03FF(x, y, z))
			{
				return true;
			}
			long num = Method_03FA(Method_040C(AddressCatalog.Field_0370));
			if (num == 0L)
			{
				return false;
			}
			long num2 = Method_03FA(num + 24);
			if (num2 == 0L)
			{
				num2 = Method_03FA(num + 24);
			}
			if (num2 == 0L)
			{
				return false;
			}
			x = Field_040E.ReadFloat(string.Format("0x{0:X}", num2 + 2528));
			y = Field_040E.ReadFloat(string.Format("0x{0:X}", num2 + 2532));
			z = Field_040E.ReadFloat(string.Format("0x{0:X}", num2 + 2536));
			return Method_03FF(x, y, z);
		}
		catch
		{
			return false;
		}
	}

    // Reuse the same actor snapshot and death flag already used to stop Wait attacks.
    // Absence, unreadable data, another VID/type and unknown flag values are NOT death.
    public bool IsConfirmedDeadStone(int vid)
    {
        return IsConfirmedDeadTarget(vid,AddressCatalog.Field_035B);
    }
    public bool IsConfirmedDeadTarget(int vid,int expectedType)
    {
        if(vid<=0)return false;
        foreach(var item in Method_0409(512))
            if(item.Field_03F0==(uint)vid)return item.Field_03F1==expectedType&&item.Field_03F2==1;
        return false;
    }
	private IEnumerable<Type_03EF> Method_0409(int max)
	{
		var list = new List<Type_03EF>();
		long location = RuntimeAdapters.Support_04AB(AddressCatalog.Field_0371.Replace(",", ""));
		if (location == 0L || max <= 0) return list;

		long begin = Method_03FA(location);
		if (begin <= 65536L) return list;
		long end = Method_03FA(location + 8);
		int count = max;
		if (end > begin && end - begin <= 8L * 16384L)
			count = (int)Math.Min(max, (end - begin) / 8L);

		// One operating-system read fetches the whole pointer vector. This removes thousands
		// of tiny reads on crowded floors, especially noticeable on low-power laptops.
		byte[] pointers = count > 0 ? Field_040E.ReadBytes(string.Format("0x{0:X}", begin), count * 8L) : null;
		if (pointers != null && pointers.Length >= count * 8)
		{
			for (int i = 0; i < count; i++)
			{
				long mob = BitConverter.ToInt64(pointers, i * 8);
				Type_03EF item;
				if (Method_040B(mob, out item)) list.Add(item);
			}
			return list;
		}

		// Compatible fallback for an unusual client whose pointer vector cannot be read at once.
		for (int i = 0; i < count; i++)
		{
			long mob = Method_03FA(begin + i * 8L);
			Type_03EF item;
			if (Method_040B(mob, out item)) list.Add(item);
		}
		return list;
	}	private bool Method_040A(uint vid, out float x, out float y, out float z, out int dead, out int type)
	{
		x = (y = (z = 0f));
		dead = 1;
		type = -1;
		foreach (Type_03EF item in Method_0409(512))
		{
			if (item.Field_03F0 == vid)
			{
				x = item.Field_03F3;
				y = item.Field_03F4;
				z = item.Field_03F5;
				dead = item.Field_03F2;
				type = item.Field_03F1;
				return true;
			}
		}
		return false;
	}

	private bool Method_040B(long mob, out Type_03EF m)
	{
		m = default;
		if (mob == 0L || mob < 65536 || mob > 140737488355327L)
		{
			return false;
		}
		try
		{
			// These fields are contiguous. One snapshot replaces six separate memory calls.
            m.ActorAddress=mob;
			byte[] snapshot = Field_040E.ReadBytes(string.Format("0x{0:X}", mob + 2412), 472L);
			if (snapshot != null && snapshot.Length >= 472)
			{
				m.Field_03F1 = snapshot[0];
				m.Field_03F2 = snapshot[80];
				m.Field_03F3 = BitConverter.ToSingle(snapshot, 116);
				m.Field_03F4 = BitConverter.ToSingle(snapshot, 120);
				m.Field_03F5 = BitConverter.ToSingle(snapshot, 124);
				m.Field_03F0 = BitConverter.ToUInt32(snapshot, 468);
			}
			else
			{
				m.Field_03F0 = (uint)Field_040E.ReadInt32(string.Format("0x{0:X}", mob + 2880));
				m.Field_03F1 = Field_040E.Method_0188(string.Format("0x{0:X}", mob + 2412));
				m.Field_03F2 = Field_040E.Method_0188(string.Format("0x{0:X}", mob + 2492));
				m.Field_03F3 = Field_040E.ReadFloat(string.Format("0x{0:X}", mob + 2528));
				m.Field_03F4 = Field_040E.ReadFloat(string.Format("0x{0:X}", mob + 2532));
				m.Field_03F5 = Field_040E.ReadFloat(string.Format("0x{0:X}", mob + 2536));
			}
			if (!Method_03FD(m.Field_03F0)) return false;
			if (m.Field_03F2 == 0 && !Method_03FE(m.Field_03F3) && !Method_03FE(m.Field_03F4)) return false;
			return true;
		}
		catch
		{
			return false;
		}
	}
	internal static long Method_040C(string s)
	{

		if (RuntimeAdapters.Support_04AC(s))
		{
			return 0L;
		}
		s = s.Replace(",", "").Replace("0x", "").Replace("0X", "");
		long result = default;
		RuntimeAdapters.Support_04AD(s, NumberStyles.HexNumber, null, ref result);
		return result;
	}

	internal static float Method_040D(float ax, float ay, float az, float bx, float by, float bz)
	{

		float num = ax - bx;
		float num2 = ay - by;
		float num3 = az - bz;
		return (float)RuntimeAdapters.Support_04AE(num * num + num2 * num2 + num3 * num3);
	}
}

