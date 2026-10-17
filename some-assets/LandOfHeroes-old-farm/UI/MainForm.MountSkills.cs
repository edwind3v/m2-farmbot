using System;
using System.Globalization;
using System.IO;
public partial class MainForm
{
    private readonly object mountSkillGate=new object();
    private DateTime mountSkillReadAt;private bool mountSkillCached;
    private int mountSkillPid;private double mountSkillExpires;
    private bool MountSkillBusy
    {
        get {
            lock(mountSkillGate) {
                if(DateTime.UtcNow-mountSkillReadAt<TimeSpan.FromMilliseconds(50))return mountSkillCached;
                mountSkillReadAt=DateTime.UtcNow;
                var process=memory.Field_0197?.Property_0268;
                double now=(DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds;
                // Keep the last valid lease during the writer's short file-replacement window.
                mountSkillCached=process!=null&&process.Id==mountSkillPid&&mountSkillExpires>now;
                try {
                    var fields=File.ReadAllText(MountSkillStatePath).Split('|');int pid;double expires;
                    if(fields.Length==2&&int.TryParse(fields[0],out pid)&&double.TryParse(fields[1],NumberStyles.Float,CultureInfo.InvariantCulture,out expires)) {
                        mountSkillPid=pid;mountSkillExpires=expires;
                        mountSkillCached=process!=null&&process.Id==pid&&expires>now;
                    }
                }catch{}
                return mountSkillCached;
            }
        }
    }
    private void RefreshMountSkillGuards()
    {
        if(automation!=null)automation.ActionPaused=()=>MountSkillBusy;
        if(movementOutput!=null)movementOutput.Paused=()=>MountSkillBusy;
        if(MountSkillBusy)ReleaseMovement();
    }
}
