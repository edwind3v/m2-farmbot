using System;
using System.Runtime.InteropServices;
// Request fine waits only while the fast worker is active; always release on exit.
internal sealed class TimingResolution : IDisposable
{
    [DllImport("winmm.dll")] private static extern uint timeBeginPeriod(uint period);
    [DllImport("winmm.dll")] private static extern uint timeEndPeriod(uint period);
    private bool enabled;
    public void SetEnabled(bool value) {
        if(value==enabled)return;
        if(value) enabled=timeBeginPeriod(1)==0;
        else {timeEndPeriod(1);enabled=false;}
    }
    public void Dispose() { SetEnabled(false); }
}
