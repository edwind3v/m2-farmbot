using System;
using System.Collections.Generic;
using System.Linq;

// A missing radar sample is not evidence of a kill. Release only after matching,
// fresh client observations confirm disappearance, or positive HP reaches zero.
public sealed class CombatTargetLock
{
    public EntityState Target { get; private set; }
    public string Token { get; private set; }
    private string scope;
    private bool seenPositiveHp;
    private int missingSamples;
    private long missingSince,lastObservation;
    private int deadSamples;
    private long deadSince,lastDeathObservation;
    private readonly Dictionary<int,long> finished = new Dictionary<int,long>();

    public void Reset()
    {
        Target=null;Token=null;scope=null;seenPositiveHp=false;missingSamples=0;
        missingSince=lastObservation=0;finished.Clear();
        deadSamples=0;deadSince=lastDeathObservation=0;
    }
    public EntityState Choose(string currentScope,IEnumerable<EntityState> candidates,long now,bool acquire=true,bool lockAllTargets=false)
    {
        if(scope!=currentScope){Reset();scope=currentScope;}
        foreach(int id in finished.Where(p=>p.Value<=now).Select(p=>p.Key).ToList())finished.Remove(id);
        var list=candidates.Where(e=>e!=null&&!finished.ContainsKey(e.Property_03C9)).ToList();
        if(Target!=null)
        {
            Target=list.FirstOrDefault(e=>e.Property_03C9==Target.Property_03C9)??Target;
            return Target;
        }
        var first=list.FirstOrDefault();
        if(acquire&&first!=null&&(first.Property_03CA==AddressCatalog.Field_035B||lockAllTargets))
        {
            Target=first;Token=Guid.NewGuid().ToString("N");seenPositiveHp=false;missingSamples=0;
            deadSamples=0;deadSince=lastDeathObservation=0;
        }
        return first;
    }
    public bool Observe(string token,int vid,bool exists,float? hp,long now,bool fastMissing=false)
    {
        if(Target==null||Token!=token||Target.Property_03C9!=vid)return false;
        if(fastMissing&&!exists&&missingSamples>0&&now<=lastObservation)return false;
        if(now-lastObservation>1500)missingSamples=0;
        lastObservation=now;
        bool complete=false;
        if(!exists)
        {
            if(missingSamples++==0)missingSince=now;
            complete=missingSamples>=(fastMissing?2:3)&&now-missingSince>=(fastMissing?250:1000);
        }
        else
        {
            missingSamples=0;
            if(hp.HasValue&&!float.IsNaN(hp.Value)&&!float.IsInfinity(hp.Value)&&hp.Value>=0&&hp.Value<=100)
            {
                if(hp.Value>0)seenPositiveHp=true;
                else complete=seenPositiveHp;
            }
        }
        if(!complete)return false;
        Finish(vid,now);
        return true;
    }
    public bool ObserveDeath(string token,int vid,bool dead,long now)
    {
        if(Target==null||Token!=token||Target.Property_03C9!=vid)return false;
        if(!dead||now-lastDeathObservation>500)deadSamples=0;
        if(!dead)return false;
        if(deadSamples>0&&now<=lastDeathObservation)return false;
        lastDeathObservation=now;
        if(deadSamples++==0)deadSince=now;
        if(deadSamples<2||now-deadSince<100)return false;
        Finish(vid,now);return true;
    }
    private void Finish(int vid,long now)
    {
        finished[vid]=now+10000;Target=null;Token=null;seenPositiveHp=false;missingSamples=deadSamples=0;
    }
    public void Abandon(int vid,long now,long cooldownMilliseconds=60000)
    {
        if(vid<=0)return;
        finished[vid]=now+Math.Max(1000,cooldownMilliseconds);
        if(Target!=null&&Target.Property_03C9==vid)
        {Target=null;Token=null;seenPositiveHp=false;missingSamples=deadSamples=0;}
    }
}
