using System;

// Once a quest symbol is acquired, combat stays blocked until the actual floor changes.
// General consumable/skill status messages and brief empty file reads never cancel hand-in intent.
public sealed class DungeonSymbolPriority
{
    private int stage=-1;
    public bool HasSymbol { get; private set; }
    public bool Delivered { get; private set; }
    public int NpcVid { get; private set; }
    public bool BlockCombat { get { return HasSymbol||Delivered; } }
    public void Observe(int currentStage,string message)
    {
        if(stage!=currentStage){stage=currentStage;HasSymbol=false;Delivered=false;NpcVid=0;}
        message=message??"";
        if(message.StartsWith("Kahramanlik Sembolu Kellam NPCye verildi",StringComparison.Ordinal))
        {HasSymbol=false;Delivered=true;return;}
        if(message.StartsWith("4. asama: sembol envanterde",StringComparison.Ordinal)||message.StartsWith("4. asama: sembol Kellama veriliyor",StringComparison.Ordinal))HasSymbol=true;
        if(message.StartsWith("4. asama: Kellam bulundu:",StringComparison.Ordinal))
        {
            var parts=message.Split(':');int vid;
            if(parts.Length>=5&&parts[4].Trim()=="1")
            {
                HasSymbol=true;if(int.TryParse(parts[2].Trim(),out vid)&&vid>0)NpcVid=vid;
            }
        }
    }
    public static string ReadScopedMessage(string snapshot,int currentStage)
    {
        if(string.IsNullOrEmpty(snapshot))return "";
        int split=snapshot.IndexOf('|'),scope;
        return split>0&&int.TryParse(snapshot.Substring(0,split),out scope)&&scope==currentStage?snapshot.Substring(split+1):"";
    }
}
