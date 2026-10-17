using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Compatible with the original .txt format: invariant X,Y, one point per line.
public static class RouteFileStore
{
    public static void Save(string path, IEnumerable<RoutePoint> points,string mapName=null)
    {
        using (var writer = new StreamWriter(path))
        {
            if(!string.IsNullOrWhiteSpace(mapName))writer.WriteLine("# map="+mapName.Replace("\r","").Replace("\n",""));
            foreach (var point in points)
                writer.WriteLine(point.X.ToString(CultureInfo.InvariantCulture) + "," + point.Y.ToString(CultureInfo.InvariantCulture));
        }
    }
    public static string LoadMap(string path){foreach(var line in File.ReadLines(path))if(line.StartsWith("# map="))return line.Substring(6).Trim();return "";}

    public static List<RoutePoint> Load(string path)
    {
        var result = new List<RoutePoint>();
        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split(',');
            float x, y;
            if (parts.Length == 2 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                &&!float.IsNaN(x)&&!float.IsInfinity(x)&&!float.IsNaN(y)&&!float.IsInfinity(y))
                result.Add(new RoutePoint { X = x, Y = y });
        }
        return result;
    }
}
