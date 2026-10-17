using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

[Serializable]
public sealed class TaskSequence
{
    public string Name { get; set; } = "Yeni görev";
    public string SourceMapName { get; set; } = "";
    public int WaitAttackMilliseconds { get; set; } = 275;
    public bool LobbyNpcMarkerSet { get; set; }
    public string LobbyNpcMapName { get; set; } = "";
    public float LobbyNpcMapX { get; set; }
    public float LobbyNpcMapY { get; set; }
    public List<TaskStage> Stages { get; set; } = new List<TaskStage>();
    public bool UseCommandFlow { get; set; }
    public List<DungeonCommand> Commands { get; set; } = new List<DungeonCommand>();
    public TaskSequence() { string[] names={"Kellam ve giriş","Slot odası","Dört taş","Kahramanlık Sembolü","Birinci patron","İkinci patron","Üçüncü patron","Son patron ve çıkış"}; for (int i=1;i<=8;i++) Stages.Add(new TaskStage { Number=i, Name=names[i-1], Transition=StageTransition.PositionChangedAndStable, EnableWaitCombat=i>=2, AttackMode="Yakın", RetryCount=3, EnableObstaclePass=i==8 }); }
    public static TaskSequence Normalize(TaskSequence value)
    {
        if(value==null)value=new TaskSequence(); if(value.Stages==null)value.Stages=new List<TaskStage>();
        if(value.Commands==null)value.Commands=new List<DungeonCommand>();
        if(value.LobbyNpcMapName==null)value.LobbyNpcMapName="";
        if(float.IsNaN(value.LobbyNpcMapX)||float.IsInfinity(value.LobbyNpcMapX)||float.IsNaN(value.LobbyNpcMapY)||float.IsInfinity(value.LobbyNpcMapY)){value.LobbyNpcMarkerSet=false;value.LobbyNpcMapX=value.LobbyNpcMapY=0;}
        value.WaitAttackMilliseconds=Math.Max(1,Math.Min(1000,value.WaitAttackMilliseconds<=0?275:value.WaitAttackMilliseconds));
        while(value.Stages.Count<8)value.Stages.Add(new TaskStage { Number=value.Stages.Count+1,Name=(value.Stages.Count+1)+". aşama" });
        if(value.Stages.Count>8)value.Stages.RemoveRange(0,value.Stages.Count-8);
        for(int i=0;i<8;i++){value.Stages[i].Number=i+1;if(string.IsNullOrWhiteSpace(value.Stages[i].AttackMode)||(i==2&&value.Stages[i].AttackMode!="Kapalı"))value.Stages[i].AttackMode="Yakın";if(i==7)value.Stages[i].EnableObstaclePass=true;if(value.Stages[i].Actions==null)value.Stages[i].Actions=new List<TaskAction>();if(value.Stages[i].Path==null)value.Stages[i].Path=new List<RoutePoint>();}
        foreach(var stage in value.Stages)
        {
            // Zero means "inherit the old profile-wide default" so existing profiles
            // retain exactly the same combat rhythm and ranged distance after loading.
            if(stage.RangedAttackRange<0)stage.RangedAttackRange=0;
            else if(stage.RangedAttackRange>0)stage.RangedAttackRange=Math.Max(100,Math.Min(9500,stage.RangedAttackRange));
            if(stage.WaitAttackMilliseconds<0)stage.WaitAttackMilliseconds=0;
            else if(stage.WaitAttackMilliseconds>0)stage.WaitAttackMilliseconds=Math.Max(1,Math.Min(1000,stage.WaitAttackMilliseconds));
            if(stage.DialogueSteps==null)stage.DialogueSteps=new List<NpcDialogueStep>();
            stage.DialogueSteps.RemoveAll(step=>step==null);
        }
        return value;
    }
}
[Serializable]
public sealed class TaskStage
{
    public int Number { get; set; }
    public string Name { get; set; } = "Aşama";
    public StageTransition Transition { get; set; } = StageTransition.Wait;
    public int WaitMilliseconds { get; set; } = 1500;
    public bool EnableWaitCombat { get; set; }
    public bool Enabled { get; set; } = true;
    public string AttackMode { get; set; } = "Otomatik";
    // 0 keeps backward compatibility by using the dungeon-wide defaults.
    public int RangedAttackRange { get; set; }
    public int WaitAttackMilliseconds { get; set; }
    public int RetryCount { get; set; } = 3;
    public string NpcName { get; set; } = "";
    public int NpcVid { get; set; }
    public int NpcVnum { get; set; }
    public string RequiredItemName { get; set; } = "";
    public uint RequiredItemId { get; set; }
    public string TargetName { get; set; } = "";
    public int TargetVnum { get; set; }
    public string TargetKind { get; set; } = "";
    public bool WaitForTeleport { get; set; }
    public bool CustomDialogueEnabled { get; set; }
    public List<NpcDialogueStep> DialogueSteps { get; set; } = new List<NpcDialogueStep>();
    public bool EnableObstaclePass { get; set; }
    public List<TaskAction> Actions { get; set; } = new List<TaskAction>();
    public List<RoutePoint> Path { get; set; } = new List<RoutePoint>();
    // Walking direction on arrival (game heading in degrees); negative means not recorded.
    public float Heading { get; set; } = -1;
}
public enum StageTransition { Immediate, Wait, PositionChangedAndStable, Manual }
public enum TaskActionKind { Click, Drag }
[Serializable]
public sealed class TaskAction
{
    public TaskActionKind Kind { get; set; }
    public int X1 { get; set; } public int Y1 { get; set; }
    public int X2 { get; set; } public int Y2 { get; set; }
    public int DelayMilliseconds { get; set; } = 250;
    public override string ToString(){return Kind==TaskActionKind.Click ? "Tıkla · "+X1+","+Y1 : "Sürükle · "+X1+","+Y1+" → "+X2+","+Y2;}
}
public static class TaskSequenceStore
{
    public static void Save(string path,TaskSequence sequence){using(var stream=File.Create(path))new XmlSerializer(typeof(TaskSequence)).Serialize(stream,TaskSequence.Normalize(sequence));}
    public static TaskSequence Load(string path){using(var stream=File.OpenRead(path))return TaskSequence.Normalize((TaskSequence)new XmlSerializer(typeof(TaskSequence)).Deserialize(stream));}
}



