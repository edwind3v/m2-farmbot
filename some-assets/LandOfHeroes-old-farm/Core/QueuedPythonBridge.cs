using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public sealed class QueuedPythonBridge : IDisposable
{
    private IntPtr handle, enqueueThread;
    private long block, module, launcherGlobal, pendingCall, runSimpleString;
    private int sourceLength;
    private bool exportedApiMode, target64, runUsesFlags;
    public int ProcessId { get; private set; }
    public bool Pending { get; private set; }
    public string ModeDescription { get; private set; }
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr VirtualAllocEx(IntPtr process,IntPtr address,UIntPtr size,uint type,uint protection);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool WriteProcessMemory(IntPtr process,IntPtr address,byte[] bytes,UIntPtr size,out UIntPtr written);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadProcessMemory(IntPtr process,IntPtr address,byte[] bytes,UIntPtr size,out UIntPtr read);
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr CreateRemoteThread(IntPtr process,IntPtr attributes,UIntPtr size,IntPtr start,IntPtr argument,uint flags,IntPtr tid);
    [DllImport("kernel32.dll")] static extern uint WaitForSingleObject(IntPtr h,uint milliseconds);
    [DllImport("kernel32.dll")] static extern bool GetExitCodeThread(IntPtr h,out uint code);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    [DllImport("kernel32.dll")] static extern bool FlushInstructionCache(IntPtr h,IntPtr address,UIntPtr size);

    public QueuedPythonBridge(Process process)
    {
        if(process==null||process.HasExited)throw new InvalidOperationException("Oyun istemcisi çalışmıyor.");
        ProcessId=process.Id;module=process.MainModule.BaseAddress.ToInt64();
        var binary=ClientBinaryInfo.Read(process.MainModule.FileName);target64=binary.Is64Bit;
        var profile=ClientProfile.For(process);
        handle=OpenProcess(0x43a,false,ProcessId);
        if(handle==IntPtr.Zero)throw new InvalidOperationException("Oyun otomasyon bağlantısı açılamadı.");
        try
        {
            if(profile.PythonComplete&&target64)SetupProfile64(profile);
            else
            {
                var api=PythonApiLocator.Find(process);
                if(api==null)throw new InvalidOperationException("Bu istemcide otomatik Python kuyruğu bulunamadı; Uyumluluk taramasını çalıştırın.");
                SetupExportedApi(api);
            }
        }
        catch{Dispose();throw;}
    }

    private void SetupProfile64(ClientProfile profile)
    {
        launcherGlobal=profile.PythonLauncher;long enqueue=profile.PythonEnqueue,run=profile.PythonRun;
        Verify(module+enqueue,new byte[]{0x48,0x8b,0x05});Verify(module+run,new byte[]{0x48,0x83,0xec,0x38,0x4c,0x8b,0x41,0x10});
        block=VirtualAllocEx(handle,IntPtr.Zero,(UIntPtr)131072,0x3000,0x40).ToInt64();
        if(block==0)throw new InvalidOperationException("Otomasyon çalışma alanı oluşturulamadı.");
        var cb=new List<byte>(new byte[]{0x53,0x48,0x83,0xec,0x20,0x48,0x89,0xcb,0x83,0x7b,0x18,0x00,0x75,0x00});
        cb.AddRange(new byte[]{0xc7,0x03,0x03,0x00,0x00,0x00,0x48,0x8b,0x4b,0x08,0x48,0x8b,0x53,0x10,0x48,0xb8});
        cb.AddRange(BitConverter.GetBytes(module+run));cb.AddRange(new byte[]{0xff,0xd0,0x0f,0xb6,0xc0,0x89,0x43,0x04});cb[13]=(byte)(cb.Count-14);
        cb.AddRange(new byte[]{0xc7,0x03,0x01,0x00,0x00,0x00,0x31,0xc0,0x48,0x83,0xc4,0x20,0x5b,0xc3});Write(block+256,cb.ToArray());
        var start=new List<byte>(new byte[]{0x48,0x83,0xec,0x28,0x48,0xb9});start.AddRange(BitConverter.GetBytes(block+256));
        start.AddRange(new byte[]{0x48,0xba});start.AddRange(BitConverter.GetBytes(block+1024));start.AddRange(new byte[]{0x48,0xb8});
        start.AddRange(BitConverter.GetBytes(module+enqueue));start.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,0x28,0xc3});Write(block,start.ToArray());
        FlushInstructionCache(handle,new IntPtr(block),(UIntPtr)512);ModeDescription="profil tabanlı x64 ana iş parçacığı kuyruğu";
    }

    private void SetupExportedApi(PythonApiSet api)
    {
        if((target64&&!string.Equals(api.Architecture,"x64",StringComparison.OrdinalIgnoreCase))||(!target64&&!string.Equals(api.Architecture,"x86",StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Python modülü ile istemci mimarisi uyuşmuyor.");
        exportedApiMode=true;pendingCall=api.AddPendingCall;runSimpleString=api.RunSimpleString;runUsesFlags=api.RunUsesFlags;
        block=VirtualAllocEx(handle,IntPtr.Zero,(UIntPtr)131072,0x3000,0x40).ToInt64();
        if(block==0)throw new InvalidOperationException("Otomasyon çalışma alanı oluşturulamadı.");
        Write(block+256,target64?BuildCallback64():BuildCallback32());
        Write(block,target64?BuildStarter64():BuildStarter32());
        FlushInstructionCache(handle,new IntPtr(block),(UIntPtr)1024);
        ModeDescription="otomatik "+api.Architecture+" Python API kuyruğu ("+api.ModuleName+")";
    }

    private byte[] BuildCallback64()
    {
        var b=new List<byte>(new byte[]{0x48,0x83,0xec,0x28,0x48,0xb9});b.AddRange(BitConverter.GetBytes(block+4096));
        if(runUsesFlags)b.AddRange(new byte[]{0x31,0xd2});b.AddRange(new byte[]{0x48,0xb8});b.AddRange(BitConverter.GetBytes(runSimpleString));b.AddRange(new byte[]{0xff,0xd0,0x48,0xba});
        b.AddRange(BitConverter.GetBytes(block+1024));b.AddRange(new byte[]{0xc7,0x02,1,0,0,0,0x31,0xc0,0x48,0x83,0xc4,0x28,0xc3});return b.ToArray();
    }
    private byte[] BuildStarter64()
    {
        var b=new List<byte>(new byte[]{0x48,0x83,0xec,0x28,0x48,0xb9});b.AddRange(BitConverter.GetBytes(block+256));
        b.AddRange(new byte[]{0x31,0xd2,0x48,0xb8});b.AddRange(BitConverter.GetBytes(pendingCall));b.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,0x28,0xc3});return b.ToArray();
    }
    private byte[] BuildCallback32()
    {
        uint script=Address32(block+4096),state=Address32(block+1024),run=Address32(runSimpleString);var b=new List<byte>(new byte[]{0x55,0x8b,0xec});
        if(runUsesFlags)b.AddRange(new byte[]{0x6a,0});b.Add(0x68);b.AddRange(BitConverter.GetBytes(script));b.Add(0xb8);b.AddRange(BitConverter.GetBytes(run));b.AddRange(new byte[]{0xff,0xd0,0x83,0xc4,(byte)(runUsesFlags?8:4),0xc7,0x05});
        b.AddRange(BitConverter.GetBytes(state));b.AddRange(new byte[]{1,0,0,0,0x33,0xc0,0x8b,0xe5,0x5d,0xc3});return b.ToArray();
    }
    private byte[] BuildStarter32()
    {
        uint callback=Address32(block+256),pending=Address32(pendingCall);var b=new List<byte>(new byte[]{0x6a,0,0x68});b.AddRange(BitConverter.GetBytes(callback));
        b.Add(0xb8);b.AddRange(BitConverter.GetBytes(pending));b.AddRange(new byte[]{0xff,0xd0,0x83,0xc4,8,0xc3});return b.ToArray();
    }
    private static uint Address32(long value){if(value<0||value>uint.MaxValue)throw new InvalidOperationException("x86 adresi 32 bit aralığın dışında.");return unchecked((uint)value);}

    private byte[] Read(long address,int count)
    {
        var data=new byte[count];UIntPtr got;
        if(handle==IntPtr.Zero||!ReadProcessMemory(handle,new IntPtr(address),data,(UIntPtr)(uint)count,out got)||got.ToUInt64()!=(ulong)count)throw new InvalidOperationException("Otomasyon bağlantısı okunamadı.");
        return data;
    }
    private void Write(long address,byte[] bytes)
    {
        UIntPtr got;if(!WriteProcessMemory(handle,new IntPtr(address),bytes,(UIntPtr)(uint)bytes.Length,out got)||got.ToUInt64()!=(ulong)bytes.Length)throw new InvalidOperationException("Otomasyon isteği gönderilemedi.");
    }
    private void Verify(long address,byte[] expected){var actual=Read(address,expected.Length);for(int i=0;i<actual.Length;i++)if(actual[i]!=expected[i])throw new InvalidOperationException("Oyun otomasyon adresleri doğrulanamadı.");}

    public void Queue(string script)
    {
        if(Pending)throw new InvalidOperationException("Önceki oyun isteği bekleniyor.");
        var bytes=Encoding.UTF8.GetBytes(script+"\0");if(bytes.Length>120000)throw new InvalidOperationException("Otomasyon ayarları çok büyük.");
        if(exportedApiMode)
        {
            Write(block+1024,new byte[8]);Write(block+4096,bytes);
        }
        else
        {
            long launcher=BitConverter.ToInt64(Read(module+launcherGlobal,8),0);if(launcher<65536)throw new InvalidOperationException("Oyun henüz hazır değil.");
            Write(block+4096,bytes);var context=new byte[32];BitConverter.GetBytes(launcher).CopyTo(context,8);BitConverter.GetBytes(block+4096).CopyTo(context,16);Write(block+1024,context);
        }
        sourceLength=bytes.Length;enqueueThread=CreateRemoteThread(handle,IntPtr.Zero,UIntPtr.Zero,new IntPtr(block),IntPtr.Zero,0,IntPtr.Zero);
        if(enqueueThread==IntPtr.Zero)throw new InvalidOperationException("Oyun isteği sıraya alınamadı.");Pending=true;
    }

    public bool Poll()
    {
        if(!Pending)return true;
        if(enqueueThread!=IntPtr.Zero&&WaitForSingleObject(enqueueThread,0)==0)
        {
            uint exit;GetExitCodeThread(enqueueThread,out exit);CloseHandle(enqueueThread);enqueueThread=IntPtr.Zero;
            if(exit!=0){Pending=false;Write(block+4096,new byte[sourceLength]);throw new InvalidOperationException("Oyun istek kuyruğu dolu; yeniden denenecek.");}
        }
        byte[] state=Read(block+1024,8);
        if(exportedApiMode)
        {
            if(BitConverter.ToInt32(state,0)!=1||enqueueThread!=IntPtr.Zero)return false;
        }
        else if(BitConverter.ToInt32(state,0)!=1||enqueueThread!=IntPtr.Zero)return false;
        Pending=false;Write(block+4096,new byte[sourceLength]);
        if(!exportedApiMode&&BitConverter.ToInt32(state,4)!=1)throw new InvalidOperationException("Oyun otomasyon isteği iptal edildi veya çalışmadı.");
        return true;
    }
    public void Cancel(){if(handle!=IntPtr.Zero&&block!=0&&Pending&&!exportedApiMode)try{Write(block+1048,BitConverter.GetBytes(1));}catch{}}
    public void Dispose(){Cancel();if(enqueueThread!=IntPtr.Zero){CloseHandle(enqueueThread);enqueueThread=IntPtr.Zero;}if(handle!=IntPtr.Zero){CloseHandle(handle);handle=IntPtr.Zero;}}
}
