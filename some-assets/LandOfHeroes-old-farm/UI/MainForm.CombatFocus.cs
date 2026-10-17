using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public partial class MainForm
{
    private readonly CombatTargetLock combatFocus=new CombatTargetLock();
    private string focusPublished;
    private DateTime focusReadAt,focusStateWritten;
    private string CombatFocusRequestPath { get { return Path.Combine(runtimeFolder,"combat-focus.txt"); } }
    private string CombatFocusStatePath { get { return Path.Combine(runtimeFolder,"combat-focus-state.txt"); } }

    private EntityState ChooseCombatTarget(IEnumerable<EntityState> candidates,string mode,IEnumerable<EntityState> live=null,bool lockAllTargets=false)
    {
        long now=movementClock.ElapsedMilliseconds;
        string scope=(memory.Field_0197?.Property_0268?.Id??0)+"|"+atlasName+"|"+mode;
        // Scope changes must be processed before accepting any previous-map observation.
        combatFocus.Choose(scope,new EntityState[0],now);
        if(DateTime.UtcNow>=focusReadAt)
        {
            focusReadAt=DateTime.UtcNow.AddMilliseconds(200);
            // Dead actors may remain visible for several seconds during their death animation.
            // Confirm the motor's matching-VID death flag twice, instead of waiting for despawn.
            var locked=combatFocus.Target;
            if(locked!=null&&!IsX86ClientMode&&automation!=null)
            {
                if(combatFocus.ObserveDeath(combatFocus.Token,locked.Property_03C9,
                    automation.IsConfirmedDeadTarget(locked.Property_03C9,locked.Property_03CA),now))
                    mapEntityMemory.Remove(locked.Property_03C9);
            }
            try
            {
                DateTime written=File.GetLastWriteTimeUtc(CombatFocusStatePath);
                if(written>focusStateWritten&&DateTime.UtcNow-written<TimeSpan.FromSeconds(2))
                {
                    focusStateWritten=written;
                    var parts=File.ReadAllText(CombatFocusStatePath).Split('|');
                    int vid;float hp;
                    bool near=combatFocus.Target!=null&&Distance2D(TaskX,TaskY,combatFocus.Target.X,combatFocus.Target.Y)<=8500;
                    if(parts.Length==4&&int.TryParse(parts[1],out vid)&&(parts[2]=="present"||(parts[2]=="missing"&&near)))
                        if(combatFocus.Observe(parts[0],vid,parts[2]=="present",
                            float.TryParse(parts[3],NumberStyles.Float,CultureInfo.InvariantCulture,out hp)?(float?)hp:null,now,mode=="route"))
                            mapEntityMemory.Remove(vid);
                }
            }
            catch(IOException){}
        }
        var list=candidates.ToList();
        bool acquire=live==null||list.Count==0||live.Any(e=>e.Property_03C9==list[0].Property_03C9);
        EntityState chosen=combatFocus.Choose(scope,list,now,acquire,lockAllTargets);
        PublishCombatFocus();
        return chosen;
    }
    private void PublishCombatFocus()
    {
        int vid=combatFocus.Target?.Property_03C9??0;
        if(automation!=null)automation.FocusTargetVid=vid;
        string request=vid==0?"":combatFocus.Token+"|"+vid;
        if(request==focusPublished)return;
        try
        {
            Directory.CreateDirectory(runtimeFolder);
            File.WriteAllText(CombatFocusRequestPath,request);
            focusPublished=request;
        }
        catch(IOException){}
    }
    private void ResetCombatFocus()
    {
        combatFocus.Reset();PublishCombatFocus();
    }
}
