using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed class GameCallBridge
{
	private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }

	private readonly ProcessMemory Field_03E9;

	private IntPtr Field_03EA;

	private long Field_03EB;

	private readonly object Field_03EC = new object();

	[CompilerGenerated]
	private string Field_03ED;

	public string Property_03EE
	{
                [CompilerGenerated]
                get
                {
                    return Field_03ED;
                }
                [CompilerGenerated]
                private set
                {
                    Field_03ED = value;
                }
	}

	public GameCallBridge(ProcessMemory mem)
	{
                Field_03E9 = mem ?? throw new ArgumentNullException("mem");
	}

	public void Method_03D1()
    {
        lock (Field_03EC)
        {
            var process = Field_03E9.Field_0197.Property_0268;
            var handle = Field_03E9.Field_0197.Property_0269;
            if (process == null || process.HasExited || handle == IntPtr.Zero) throw new InvalidOperationException("Oyun istemcisi çalışmıyor.");
            if (ownerPid != process.Id || (Field_03EA != IntPtr.Zero && Field_03EA != handle))
            {
                // A restarted client never inherits executable memory from the previous process.
                if (pendingThread != IntPtr.Zero) NativeMethods.Method_045B(pendingThread);
                pendingThread = IntPtr.Zero; Field_03EB = 0; nativeFault = false;
                ownerPid = process.Id;
                queueConfigured = false; useMainQueue = false; enqueueAddress = 0; queueTailReady = false;
            }
            Field_03EA = handle;
            EnsureIdle();
            if (Field_03EB == 0)
            {
                UIntPtr block = NativeMethods.Method_0452(handle, UIntPtr.Zero, 4096u, 12288u, 64u);
                if (block == UIntPtr.Zero) throw new InvalidOperationException("VirtualAllocEx err=" + Marshal.GetLastWin32Error());
                Field_03EB = (long)block.ToUInt64();
            }
        }
    }

    // Runs the original actor interaction on the main queue. Target and main actor are
    // resolved again inside the callback, not held as pointers across queue delays.
    public bool CallNpcInteraction(long resolve,long interact,long playerGlobal,long managerGlobal,int vid,int mainVid,int race,CancellationToken token,int expectedProcessId=0)
    {
        lock(Field_03EC)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if(resolve<=0||interact<=0||playerGlobal<=0||managerGlobal<=0||vid<=0||mainVid<=0||race<=0)throw new ArgumentException("NPC çağrı parametreleri geçersiz.");
                Method_03D1();
                if(expectedProcessId>0&&ownerPid!=expectedProcessId)throw new InvalidOperationException("NPC doğrulamasından sonra bağlı istemci değişti.");
                ConfigureMainQueue();
                if(!useMainQueue)throw new InvalidOperationException("NPC etkileşimi için ana oyun kuyruğu gerekli.");
                long cancel=Field_03EB+560,result=Field_03EB+568;WriteNative(cancel,new byte[12]);
                IntPtr ownerHandle=Field_03EA;
                Action cancelCall=()=>{UIntPtr written;WriteProcessMemory(ownerHandle,new IntPtr(cancel),BitConverter.GetBytes(1),(UIntPtr)4,out written);};
                using(token.Register(cancelCall))
                {
                    try
                    {
                        bool completed=Method_03DB(BuildNpcInteractionBody(resolve,interact,playerGlobal,managerGlobal,vid,mainVid,race,cancel,result));
                        token.ThrowIfCancellationRequested();
                        if(!completed)return false;
                        if(BitConverter.ToInt32(ReadNative(result,4),0)!=1){Property_03EE="NPC veya oyuncu kimliği değişti; etkileşim gönderilmedi.";return false;}
                        return true;
                    }
                    finally{cancelCall();} // Timed-out callbacks must never open a dialog later.
                }
            }
            catch(OperationCanceledException){throw;}
            catch(Exception ex){Property_03EE=ex.Message;return false;}
        }
    }
    public static byte[] BuildNpcInteractionBody(long resolve,long interact,long playerGlobal,long managerGlobal,int vid,int mainVid,int race,long cancel,long result)
    {
        var code=new List<byte>();var jumps=new List<int>();Method_03D8(code);
        Action<byte[]> emit=b=>code.AddRange(b);
        Action<long> rax=n=>{emit(new byte[]{0x48,0xb8});emit(BitConverter.GetBytes(n));};
        Action<byte> endIf=condition=>{emit(new byte[]{0x0f,condition});jumps.Add(code.Count);emit(new byte[4]);};
        Action cancellation=()=>{rax(cancel);emit(new byte[]{0x83,0x38,0});endIf(0x85);};
        Action<int> lookup=id=>{emit(new byte[]{0x48,0x89,0xd9,0xba});emit(BitConverter.GetBytes(id));rax(resolve);emit(new byte[]{0xff,0xd0,0x48,0x85,0xc0});endIf(0x84);};
        cancellation();rax(playerGlobal);emit(new byte[]{0x48,0x8b,0x18,0x48,0x85,0xdb});endIf(0x84);
        rax(managerGlobal);emit(new byte[]{0x48,0x83,0x38,0});endIf(0x84);
        lookup(vid);emit(new byte[]{0x48,0x89,0xc7});
        // Verified CInstanceBase: type = graphic actor +0x450+0x51C; original race = +0x380.
        emit(new byte[]{0x83,0xbf,0x6c,0x09,0,0,1});endIf(0x85);
        emit(new byte[]{0x81,0xbf,0x80,0x03,0,0});emit(BitConverter.GetBytes(race));endIf(0x85);
        lookup(mainVid);emit(new byte[]{0x48,0x89,0xc6});cancellation();
        emit(new byte[]{0x48,0x89,0xd9,0x48,0x89,0xf2,0x41,0xb8});emit(BitConverter.GetBytes(vid));
        emit(new byte[]{0x45,0x31,0xc9});rax(interact);emit(new byte[]{0xff,0xd0});
        rax(result);emit(new byte[]{0xc7,0,1,0,0,0});
        int end=code.Count;foreach(int at in jumps){byte[] offset=BitConverter.GetBytes(end-at-4);for(int i=0;i<4;i++)code[at+i]=offset[i];}
        Method_03D9(code);return code.ToArray();
    }
	public bool Method_03D2(long battleFunc, long netPtr, uint vid, uint type = 0u)
	{
        lock (Field_03EC)
        {
            try
            {

                if (battleFunc == 0L || netPtr == 0L || vid == 0 || vid >= 99999999)
                {
                    Property_03EE = "bad args";
                    return false;
                }
                Method_03D1();
                return Method_03DB(Method_03D7(netPtr, type, vid, battleFunc));
	
            }
            catch (Exception ex) { Property_03EE = ex.Message; return false; }
        }
	}

	public bool Method_03D3(long stateFunc, long netPtr, float x, float y, float z, float rotation = 0f, uint eFunc = 0u, uint uArg = 0u)
	{
        lock (Field_03EC)
        {
            try
            {

                if (stateFunc == 0L || netPtr == 0L || !Finite(x) || !Finite(y) || !Finite(z))
                {
                    Property_03EE = "bad args";
                    return false;
                }
                Method_03D1();
                long num = Field_03EB + 512;
                Method_03DA(num, x, y, z);
                List<byte> list = new List<byte>();
                Method_03D8(list);
                list.AddRange(new byte[2] { 72, 185 });
                list.AddRange(BitConverter.GetBytes(netPtr));
                list.AddRange(new byte[2] { 72, 186 });
                list.AddRange(BitConverter.GetBytes(num));
                list.AddRange(new byte[3] { 15, 87, 210 });
                list.AddRange(new byte[2] { 65, 185 });
                list.AddRange(BitConverter.GetBytes(eFunc));
                list.AddRange(new byte[4] { 199, 68, 36, 32 });
                list.AddRange(BitConverter.GetBytes(uArg));
                list.AddRange(new byte[2] { 72, 184 });
                list.AddRange(BitConverter.GetBytes(stateFunc));
                list.AddRange(new byte[2] { 255, 208 });
                Method_03D9(list);
                return Method_03DB(list.ToArray());
	
            }
            catch (Exception ex) { Property_03EE = ex.Message; return false; }
        }
	}

	public bool Method_03D4(long func, long thisPtr)
	{
        lock (Field_03EC)
        {
            try
            {

                if (func == 0L || thisPtr == 0L)
                {
                    Property_03EE = "bad args";
                    return false;
                }
                Method_03D1();
                List<byte> list = new List<byte>();
                Method_03D8(list);
                list.AddRange(new byte[2] { 72, 185 });
                list.AddRange(BitConverter.GetBytes(thisPtr));
                list.AddRange(new byte[2] { 72, 184 });
                list.AddRange(BitConverter.GetBytes(func));
                list.AddRange(new byte[2] { 255, 208 });
                Method_03D9(list);
                return Method_03DB(list.ToArray());
	
            }
            catch (Exception ex) { Property_03EE = ex.Message; return false; }
        }
	}

	public bool Method_03D5(long flyFunc, long shootFunc, long netPtr, uint vid, float x, float y, float z, byte shootType = 0)
	{
        lock (Field_03EC)
        {
            try
            {

                if (flyFunc == 0L || shootFunc == 0L || netPtr == 0L || vid == 0 || vid >= 99999999 || !Finite(z))
                {
                    Property_03EE = ((shootFunc == 0L) ? "shoot=0" : "bad args");
                    return false;
                }
                if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y) || (Math.Abs(x) < 0.01f && Math.Abs(y) < 0.01f))
                {
                    Property_03EE = "bad coords";
                    return false;
                }
                Method_03D1();
                long num = Field_03EB + 512;
                Method_03DA(num, x, Math.Abs(y), z);
                List<byte> list = new List<byte>();
                Method_03D8(list);
                list.AddRange(new byte[2] { 72, 185 });
                list.AddRange(BitConverter.GetBytes(netPtr));
                list.AddRange(new byte[2] { 72, 186 });
                list.AddRange(BitConverter.GetBytes((ulong)vid));
                list.AddRange(new byte[2] { 73, 184 });
                list.AddRange(BitConverter.GetBytes(num));
                list.AddRange(new byte[2] { 72, 184 });
                list.AddRange(BitConverter.GetBytes(flyFunc));
                list.AddRange(new byte[2] { 255, 208 });
                list.AddRange(new byte[2] { 72, 185 });
                list.AddRange(BitConverter.GetBytes(netPtr));
                list.AddRange(new byte[2] { 178, shootType });
                list.AddRange(new byte[2] { 51, 192 });
                list.AddRange(new byte[2] { 72, 184 });
                list.AddRange(BitConverter.GetBytes(shootFunc));
                list.AddRange(new byte[2] { 255, 208 });
                Method_03D9(list);
                return Method_03DB(list.ToArray());
	
            }
            catch (Exception ex) { Property_03EE = ex.Message; return false; }
        }
	}

	public bool Method_03D6(long stateFunc, long netPtr, float x, float y, float z)
	{
                if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y) || (Math.Abs(x) < 0.01f && Math.Abs(y) < 0.01f))
                {
                    return false;
                }
                return Method_03D3(stateFunc, netPtr, x, Math.Abs(y), z);
	}

	private byte[] Method_03D7(long rcx, uint edx, ulong r8, long func)
	{
                List<byte> list = new List<byte>();
                Method_03D8(list);
                list.AddRange(new byte[2] { 72, 185 });
                list.AddRange(BitConverter.GetBytes(rcx));
                list.AddRange(new byte[1] { 186 });
                list.AddRange(BitConverter.GetBytes(edx));
                list.AddRange(new byte[2] { 73, 184 });
                list.AddRange(BitConverter.GetBytes(r8));
                list.AddRange(new byte[2] { 72, 184 });
                list.AddRange(BitConverter.GetBytes(func));
                list.AddRange(new byte[2] { 255, 208 });
                Method_03D9(list);
                return list.ToArray();
	}

	private static void Method_03D8(List<byte> sc)
	{
                sc.AddRange(new byte[16]
                {
                    83, 86, 87, 65, 84, 73, 137, 228, 72, 131,
                    228, 240, 72, 131, 236, 48
                });
	}

	private static void Method_03D9(List<byte> sc)
	{
                // Thread exit status is defined: game functions may return void or only set AL.
        sc.AddRange(new byte[11] { 49, 192, 76, 137, 228, 65, 92, 95, 94, 91, 195 });
	}

	private void Method_03DA(long addr, float x, float y, float z)
	{
                byte[] array = new byte[12];
                BitConverter.GetBytes(x).CopyTo(array, 0);
                BitConverter.GetBytes(y).CopyTo(array, 4);
                BitConverter.GetBytes(z).CopyTo(array, 8);
                WriteNative(addr, array);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] data, UIntPtr size, out UIntPtr written);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushInstructionCache(IntPtr process, IntPtr address, UIntPtr size);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeThread(IntPtr thread, out uint exitCode);

    private IntPtr pendingThread;
    private int ownerPid;
    private bool nativeFault;

    private void WriteNative(long address, byte[] data)
    {
        UIntPtr written;
        if (!WriteProcessMemory(Field_03EA, new IntPtr(address), data, (UIntPtr)(uint)data.Length, out written) || written.ToUInt64() != (ulong)data.Length)
            throw new InvalidOperationException("Oyun çağrısı yazılamadı: " + Marshal.GetLastWin32Error());
    }

    private bool queueConfigured, useMainQueue, queueTailReady;
    private long enqueueAddress;
    private const int QueueStateOffset = 768, QueueStarterOffset = 1024, QueueTailOffset = 2048;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] data, UIntPtr size, out UIntPtr read);

    public static bool IsVerifiedClientHash(string hash)
    {
        return string.Equals(hash, PlayerInventoryReader.SupportedHash, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(hash, "0220feb205b43a9520515d4e859ca09b54f3c9df1e9c5c97cd38430bfde62612", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(hash, "7b9c2b782baa1dc152ce86dcc6a816160729ca946744eb1f669aea1e018b5ab0", StringComparison.OrdinalIgnoreCase);
    }

    private void ConfigureMainQueue()
    {
        if (queueConfigured) return;
        var process = Field_03E9.Field_0197.Property_0268;
        // Scope the change to the three verified builds of this client's current binary.
        // Other files and the x86 compatibility path retain their existing bridge.
        if (Field_03E9.Field_0197.Property_026A && string.Equals(process.ProcessName, "LandOfHeroesEU", StringComparison.OrdinalIgnoreCase))
        {
            var profile = ClientProfile.For(process);
            if (IsVerifiedClientHash(profile.Sha256))
            {
                if (!profile.PythonComplete || !profile.Is64Bit) throw new InvalidOperationException("Ana oyun kuyruğu doğrulanamadı.");
                enqueueAddress = process.MainModule.BaseAddress.ToInt64() + profile.PythonEnqueue;
                var bytes = ReadNative(enqueueAddress, 3);
                if (bytes[0] != 0x48 || bytes[1] != 0x8b || bytes[2] != 0x05) throw new InvalidOperationException("Ana oyun kuyruğunun imzası değişti.");
                useMainQueue = true;
            }
        }
        queueConfigured = true;
    }

    private byte[] ReadNative(long address, int count)
    {
        var bytes = new byte[count]; UIntPtr actual;
        if (!ReadProcessMemory(Field_03EA, new IntPtr(address), bytes, (UIntPtr)(uint)count, out actual) || actual.ToUInt64() != (ulong)count)
            throw new InvalidOperationException("Oyun çağrısının tamamlanma durumu okunamadı.");
        return bytes;
    }

    private bool QueueFinished() { return !useMainQueue || BitConverter.ToInt32(ReadNative(Field_03EB + QueueStateOffset, 4), 0) == 1; }

    private byte[] QueuedBody(byte[] original)
    {
        if (!queueTailReady)
        {
            var tail = new List<byte>(new byte[] { 0x48, 0xb8 });
            tail.AddRange(BitConverter.GetBytes(Field_03EB + QueueStateOffset));
            tail.AddRange(new byte[] { 0xc7, 0x00, 1, 0, 0, 0 });
            tail.AddRange(new ArraySegment<byte>(original, original.Length - 11, 11));
            WriteNative(Field_03EB + QueueTailOffset, tail.ToArray());
            if (!FlushInstructionCache(Field_03EA, new IntPtr(Field_03EB + QueueTailOffset), (UIntPtr)(uint)tail.Count))
                throw new InvalidOperationException("Kuyruk dönüş kodu doğrulanamadı.");
            queueTailReady = true;
        }
        // Completion is signalled from a fixed tail that is never overwritten. Once the
        // signal is visible, the previous call has left all reusable code and data.
        var body = new List<byte>(new ArraySegment<byte>(original, 0, original.Length - 11));
        body.AddRange(new byte[] { 0x48, 0xb8 }); body.AddRange(BitConverter.GetBytes(Field_03EB + QueueTailOffset));
        body.AddRange(new byte[] { 0xff, 0xe0 });
        return body.ToArray();
    }

    private byte[] QueueStarter()
    {
        var code = new List<byte>(new byte[] { 0x48, 0x83, 0xec, 0x28, 0x48, 0xb9 });
        code.AddRange(BitConverter.GetBytes(Field_03EB));
        code.AddRange(new byte[] { 0x31, 0xd2, 0x48, 0xb8 }); code.AddRange(BitConverter.GetBytes(enqueueAddress));
        code.AddRange(new byte[] { 0xff, 0xd0, 0x48, 0x83, 0xc4, 0x28, 0xc3 });
        return code.ToArray();
    }

    private bool CheckCompletedThread()
    {
        uint exit;
        if (!GetExitCodeThread(pendingThread, out exit))
        {
            Property_03EE = "Çağrı sonucu okunamadı: " + Marshal.GetLastWin32Error();
            return false;
        }
        NativeMethods.Method_045B(pendingThread);
        pendingThread = IntPtr.Zero;
        if ((exit & 0xF0000000u) == 0xC0000000u)
        {
            nativeFault = true;
            Property_03EE = "Oyun çağrısı yerel hata verdi: 0x" + exit.ToString("X8") + "; bağlantı yenilenmeli.";
            return false;
        }
        return true;
    }

    private void CheckQueueRejection(uint exit)
    {
        CheckCompletedThread();
        // The verified enqueue function returns -1 when its bounded queue is full.
        // No callback was added, so this transient rejection can be retried safely.
        if (exit == uint.MaxValue) Property_03EE = "Ana oyun kuyruğu dolu; sonraki çevrimde yeniden denenecek.";
        else { nativeFault = true; Property_03EE = "Ana kuyruk başlangıcı başarısız: 0x" + exit.ToString("X8") + "; bağlantı yenilenmeli."; }
    }

    private void EnsureIdle()
    {
        if (nativeFault) throw new InvalidOperationException(Property_03EE ?? "Oyun çağrıları hata nedeniyle bekletiliyor.");
        if (pendingThread == IntPtr.Zero) return;
        int wait = NativeMethods.Method_0461(pendingThread, 0);
        if (wait != 0) throw new InvalidOperationException("Önceki oyun çağrısı hâlâ çalışıyor; yeni çağrı bekletiliyor (" + wait + ").");
        if (useMainQueue && !QueueFinished())
        {
            uint exit;
            if (!GetExitCodeThread(pendingThread, out exit)) throw new InvalidOperationException("Kuyruk başlangıç sonucu okunamadı.");
            if (exit != 0) { CheckQueueRejection(exit); throw new InvalidOperationException(Property_03EE); }
            throw new InvalidOperationException("Önceki çağrı ana oyun kuyruğunda bekliyor; kod ve veriler korunuyor.");
        }
        if (!CheckCompletedThread()) throw new InvalidOperationException(Property_03EE);
    }

    private bool Method_03DB(byte[] sc)
    {
        lock (Field_03EC)
        {
            try
            {
                Method_03D1(); // Also prevents overwriting the code or data of a delayed call.
                Property_03EE = null;
                if (sc == null || sc.Length > 512) throw new InvalidOperationException("Oyun çağrısı çalışma alanını aşıyor.");
                ConfigureMainQueue();
                if (useMainQueue)
                {
                    sc = QueuedBody(sc);
                    if (sc.Length > 512) throw new InvalidOperationException("Kuyruk çağrısı çalışma alanını aşıyor.");
                    WriteNative(Field_03EB + QueueStateOffset, new byte[4]);
                    var starter = QueueStarter(); WriteNative(Field_03EB + QueueStarterOffset, starter);
                    if (!FlushInstructionCache(Field_03EA, new IntPtr(Field_03EB + QueueStarterOffset), (UIntPtr)(uint)starter.Length))
                        throw new InvalidOperationException("Kuyruk kodu doğrulanamadı.");
                }
                WriteNative(Field_03EB, sc);
                if (!FlushInstructionCache(Field_03EA, new IntPtr(Field_03EB), (UIntPtr)(uint)sc.Length))
                    throw new InvalidOperationException("Çağrı kodu doğrulanamadı: " + Marshal.GetLastWin32Error());
                return Method_03DC();
            }
            catch (Exception ex) { Property_03EE = ex.Message; return false; }
        }
    }

    private bool Method_03DC()
    {
        pendingThread = NativeMethods.Method_0464(Field_03EA, IntPtr.Zero, 0u, (UIntPtr)(ulong)(Field_03EB + (useMainQueue ? QueueStarterOffset : 0)), UIntPtr.Zero, 0u, out var _);
        if (pendingThread == IntPtr.Zero)
        {
            Property_03EE = "CreateRemoteThread=" + Marshal.GetLastWin32Error();
            return false;
        }
        int wait;
        if (useMainQueue)
        {
            var timer = Stopwatch.StartNew(); wait = 258;
            while (timer.ElapsedMilliseconds < 3000)
            {
                int starterWait = NativeMethods.Method_0461(pendingThread, 0);
                if (starterWait == 0)
                {
                    uint exit;
                    if (!GetExitCodeThread(pendingThread, out exit)) throw new InvalidOperationException("Kuyruk başlangıç sonucu okunamadı.");
                    if (exit != 0) { CheckQueueRejection(exit); return false; }
                    if (QueueFinished()) { wait = 0; break; }
                }
                else if (starterWait != 258) { wait = starterWait; break; }
                Thread.Sleep(1);
            }
        }
        else wait = NativeMethods.Method_0461(pendingThread, 3000);
        if (wait != 0)
        {
            // Do not close a live call, reuse its block, repeat it, or rewind the game's UI thread.
            Property_03EE = "Oyun çağrısı bekleniyor (" + wait + "); çalışma alanı korunuyor.";
            return false;
        }
        return CheckCompletedThread();
    }
}
