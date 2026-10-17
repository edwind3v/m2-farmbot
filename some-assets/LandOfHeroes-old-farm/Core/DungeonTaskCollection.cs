using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

[Serializable]
public sealed class DungeonTaskCollection
{
    public List<TaskSequence> Dungeons{get;set;}=new List<TaskSequence>();
    public int SelectedIndex{get;set;}
    public static DungeonTaskCollection Normalize(DungeonTaskCollection value)
    {
        if(value==null)value=new DungeonTaskCollection();
        if(value.Dungeons==null)value.Dungeons=new List<TaskSequence>();
        if(value.Dungeons.Count==0)value.Dungeons.Add(new TaskSequence{Name="Kellam Zindanı"});
        for(int i=0;i<value.Dungeons.Count;i++)
        {
            value.Dungeons[i]=TaskSequence.Normalize(value.Dungeons[i]);
            if(string.IsNullOrWhiteSpace(value.Dungeons[i].Name)||value.Dungeons[i].Name=="Yeni görev")
                value.Dungeons[i].Name=i==0?"Kellam Zindanı":(i+1)+". Zindan";
        }
        value.SelectedIndex=Math.Max(0,Math.Min(value.Dungeons.Count-1,value.SelectedIndex));return value;
    }
}
public static class DungeonTaskCollectionStore
{
    public static void Save(string path,DungeonTaskCollection value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));string temporary=path+".tmp";
        using(var stream=File.Create(temporary))new XmlSerializer(typeof(DungeonTaskCollection)).Serialize(stream,DungeonTaskCollection.Normalize(value));
        if(File.Exists(path))File.Replace(temporary,path,path+".bak",true);else File.Move(temporary,path);
    }
    public static DungeonTaskCollection Load(string path)
    {
        using(var stream=File.OpenRead(path))return DungeonTaskCollection.Normalize((DungeonTaskCollection)new XmlSerializer(typeof(DungeonTaskCollection)).Deserialize(stream));
    }
}
