using System;
using System.Diagnostics;
using System.IO;
public sealed class ExternalPythonBridge : IDisposable
{
    private Process process; public int ProcessId {get;private set;} public bool Pending {get;private set;}
    public ExternalPythonBridge(Process target){ProcessId=target.Id;process=null;}
    public void Queue(string script)
    {
        if(Pending)throw new InvalidOperationException("Önceki oyun isteği bekleniyor.");
        string helper=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Onur.BridgeHost.exe");
        if(!File.Exists(helper))throw new InvalidOperationException("İstemci bağlantı yardımcısı bulunamadı.");
        var start=new ProcessStartInfo(helper,ProcessId.ToString()){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true};
        var launched=Process.Start(start);process=launched;
        launched.StandardInput.Write(script);launched.StandardInput.Close();Pending=true;
    }
    public bool Poll(){if(!Pending)return true;if(!process.HasExited)return false;Pending=false;if(process.ExitCode!=0)throw new InvalidOperationException("Oyun istemci köprüsü yanıt vermedi; yeniden kurulacak.");return true;}
    public void Cancel(){try{if(Pending&&!process.HasExited)process.Kill();}catch{}Pending=false;}
    public void Dispose(){Cancel();try{process?.Dispose();}catch{}}
}

