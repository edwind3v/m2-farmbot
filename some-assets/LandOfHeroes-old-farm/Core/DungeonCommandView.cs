using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// Show only fields relevant to this command, with Turkish descriptions.
public sealed class DungeonCommandView:CustomTypeDescriptor {
    public DungeonCommand Command {get;private set;}
    public DungeonCommandView(DungeonCommand command){Command=command;}
    public override PropertyDescriptorCollection GetProperties(){return GetProperties(new Attribute[0]);}
    public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
        var keys=new HashSet<string>{"Kind","Name"};
        if(Command.Kind==DungeonCommandKind.Move||Command.Kind==DungeonCommandKind.ClearTargets||Command.Kind==DungeonCommandKind.CollectItem||Command.Kind==DungeonCommandKind.GiveItem||Command.Kind==DungeonCommandKind.UseItem||Command.Kind==DungeonCommandKind.Interact||Command.Kind==DungeonCommandKind.Dialogue||Command.Kind==DungeonCommandKind.WaitMap||Command.Kind==DungeonCommandKind.WaitTarget||Command.Kind==DungeonCommandKind.WaitTargetGone)keys.Add("TimeoutMilliseconds");
        Action<string> add=text=>{foreach(string key in text.Split(' '))keys.Add(key);};
        string target="TargetName TargetKind TargetVnum NativeTargetType ConstrainTargetArea";
        bool targetCommand=Command.Kind==DungeonCommandKind.ClearTargets||Command.Kind==DungeonCommandKind.GiveItem||Command.Kind==DungeonCommandKind.Interact||Command.Kind==DungeonCommandKind.Dialogue||Command.Kind==DungeonCommandKind.WaitTarget||Command.Kind==DungeonCommandKind.WaitTargetGone||((Command.Kind==DungeonCommandKind.If||Command.Kind==DungeonCommandKind.RepeatWhile)&&Command.Condition==DungeonCondition.TargetExists);
        if(targetCommand&&Command.ConstrainTargetArea)add("MapName X Y TargetRadius");
        switch(Command.Kind) {
            case DungeonCommandKind.Wait:add("WaitMilliseconds");break;
            case DungeonCommandKind.Move:add("MapName X Y Range");break;
            case DungeonCommandKind.ClearTargets:add(target+" AttackMode AttackMilliseconds AttackRange RequireTargetSeen KillLimit WaitMilliseconds");break;
            case DungeonCommandKind.CollectItem:case DungeonCommandKind.UseItem:add("ItemId Count");break;
            case DungeonCommandKind.GiveItem:add(target+" ItemId Count Range");break;
            case DungeonCommandKind.Interact:
                add(target+" Range InteractionResult");
                if(Command.InteractionResult==DungeonInteractionResult.TargetAppeared)add("ResultTargetVnum");
                if(Command.InteractionResult==DungeonInteractionResult.MapChanged)add("ResultMapName");break;
            case DungeonCommandKind.Dialogue:add(target+" Range");break;
            case DungeonCommandKind.WaitMap:add("MapName");break;
            case DungeonCommandKind.WaitTarget:case DungeonCommandKind.WaitTargetGone:add(target);break;
            case DungeonCommandKind.If:
            case DungeonCommandKind.RepeatWhile:
                add("Condition Negate");
                if(Command.Kind==DungeonCommandKind.RepeatWhile)add("RepeatCount");
                switch(Command.Condition) {
                    case DungeonCondition.ItemAvailable:add("ItemId Count");break;
                    case DungeonCondition.FreeInventorySlots:add("Count");break;
                    case DungeonCondition.TargetExists:add(target);break;
                    case DungeonCondition.MapEquals:add("MapName");break;
                    case DungeonCondition.HealthBelow:add("HealthPercent");break;
                }break;
            case DungeonCommandKind.Repeat:add("RepeatCount");break;
        }
        return new PropertyDescriptorCollection(TypeDescriptor.GetProperties(Command).Cast<PropertyDescriptor>().Where(p=>keys.Contains(p.Name)).Select(p=>(PropertyDescriptor)new Field(Command,p)).ToArray());
    }
    public override object GetPropertyOwner(PropertyDescriptor p){return this;}
    sealed class Field:PropertyDescriptor {
        readonly DungeonCommand command;readonly PropertyDescriptor inner;
        public Field(DungeonCommand command,PropertyDescriptor inner):base(inner){this.command=command;this.inner=inner;}
        public override Type ComponentType{get{return typeof(DungeonCommandView);}}
        public override string DisplayName{get{return inner.Name=="RepeatCount"&&command.Kind==DungeonCommandKind.RepeatWhile?"En fazla tekrar (güvenlik)":inner.DisplayName;}}
        public override string Description{get{return inner.Name=="RepeatCount"&&command.Kind==DungeonCommandKind.RepeatWhile?"Koşul doğru kaldığı sürece çalışır. Bu sınıra rağmen koşul hâlâ doğruysa görev durur; sonsuz döngüye girmez.":inner.Description;}}
        public override Type PropertyType{get{return inner.PropertyType;}}
        public override bool IsReadOnly{get{return inner.IsReadOnly;}}
        public override bool CanResetValue(object owner){return inner.CanResetValue(command);}
        public override object GetValue(object owner){return inner.GetValue(command);}
        public override void SetValue(object owner,object value){inner.SetValue(command,value);}
        public override void ResetValue(object owner){inner.ResetValue(command);}
        public override bool ShouldSerializeValue(object owner){return inner.ShouldSerializeValue(command);}
    }
}
