using System;
using System.Web.Script.Serialization;

public sealed class TargetHealthSnapshot
{
    public int Pid { get; set; }
    public int Vid { get; set; }
    public string Name { get; set; }
    public string Map { get; set; }
    public double At { get; set; }
    public float? Hp { get; set; }
    public static TargetHealthSnapshot Parse(string json,int pid,string map,DateTime utcNow)
    {
        try
        {
            var data=new JavaScriptSerializer().Deserialize<TargetHealthSnapshot>(json);
            double now=(utcNow-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds;
            if(data==null||pid<=0||data.Pid!=pid||data.Vid<=0||double.IsNaN(data.At)||double.IsInfinity(data.At)||now-data.At>2||now<data.At||(!string.IsNullOrEmpty(map)&&data.Map!=map))return null;
            if(data.Hp.HasValue&&(float.IsNaN(data.Hp.Value)||float.IsInfinity(data.Hp.Value)||data.Hp<0||data.Hp>100))data.Hp=null;
            return data;
        }
        catch{return null;}
    }
}
