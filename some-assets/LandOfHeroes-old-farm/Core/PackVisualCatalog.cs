using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

public sealed class PackVisualCatalog
{
    public string Root { get; private set; }
    public HashSet<int> MountVnums { get; private set; }=new HashSet<int>();
    public HashSet<int> PetVnums { get; private set; }=new HashSet<int>();

    public static PackVisualCatalog Load(string root)
    {
        if(string.IsNullOrWhiteSpace(root))throw new ArgumentException("Pack klasoru bos olamaz.");
        root=Path.GetFullPath(root.Trim());
        if(!Directory.Exists(root))throw new DirectoryNotFoundException("Pack klasoru bulunamadi: "+root);
        string itemPath=FindItemProto(root);
        var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=128};
        var records=serializer.DeserializeObject(File.ReadAllText(itemPath)) as object[];
        if(records==null)throw new InvalidDataException("item_proto.json gecerli bir JSON dizisi degil.");
        var result=new PackVisualCatalog{Root=root};
        foreach(var value in records)
        {
            var row=value as IDictionary<string,object>;if(row==null)continue;
            int type=Number(row,"type");if(type!=36&&type!=38)continue;
            object raw;if(!row.TryGetValue("values",out raw))continue;
            var values=raw as object[];if(values==null||values.Length==0)continue;
            int model=ToInt(values[0]);if(model<=0)continue;
            if(type==36)result.MountVnums.Add(model);else result.PetVnums.Add(model);
        }
        return result;
    }

    private static string FindItemProto(string root)
    {
        foreach(var folder in new[]{Path.Combine(root,"locale","locale","tr"),Path.Combine(root,"locale","tr"),Path.Combine(root,"tr")})
        {
            string path=Path.Combine(folder,"item_proto.json");if(File.Exists(path))return path;
        }
        string found=Directory.EnumerateFiles(root,"item_proto.json",SearchOption.AllDirectories).FirstOrDefault();
        if(found==null)throw new FileNotFoundException("item_proto.json bulunamadi. Cikarilan packlarin ana klasorunu secin.");
        return found;
    }
    private static int Number(IDictionary<string,object> row,string key){object value;return row.TryGetValue(key,out value)?ToInt(value):0;}
    private static int ToInt(object value){int parsed;return value!=null&&int.TryParse(Convert.ToString(value,CultureInfo.InvariantCulture),NumberStyles.Any,CultureInfo.InvariantCulture,out parsed)?parsed:0;}
}
