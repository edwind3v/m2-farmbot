using System;
// Retain the direction while the game consumes the release edge, then clear it.
public sealed class MovementOutput
{
    private readonly Action<bool,bool> write;
    private readonly Action<int> wait;
    private readonly object gate = new object();
    private bool active;
    private long lastRefresh;
    public Func<bool> Paused { get; set; }
    private int leaseMilliseconds=250;
    public int LeaseMilliseconds {get{return leaseMilliseconds;}set{if(value<100||value>1000)throw new ArgumentOutOfRangeException("value");leaseMilliseconds=value;}}
    public MovementOutput(Action<bool,bool> write, Action<int> wait) { this.write=write; this.wait=wait; }
    public void Move(long now) {
        lock(gate) {
            if(Paused!=null && Paused()){Stop();return;}
            active=true;
            try { write(false,true); write(true,true); lastRefresh=now; }
            catch { Stop(); throw; }
        }
    }
    public void Stop() {
        lock(gate) {
            if(!active) return;
            // Old route logic used separate updates. Clearing both in one frame loses the stop edge.
            bool released=false, cleared=false;
            try { write(true,false); wait(100); released=true; }
            finally { try { write(false,false); cleared=true; } finally { active=!(released && cleared); } }
        }
    }
    public bool Expire(long now) {
        lock(gate) { if(!active || now-lastRefresh<=leaseMilliseconds) return false; Stop();return true; }
    }
}
