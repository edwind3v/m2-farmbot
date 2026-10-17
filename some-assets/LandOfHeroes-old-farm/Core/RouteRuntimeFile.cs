using System;
using System.IO;
using System.Text;

// Small leased runtime snapshots, not saved profiles. Windows Python readers
// do not necessarily share DELETE: replacing their open file caused route stops.
public static class RouteRuntimeFile
{
    public static bool TryWrite(string path,string content)
    {
        byte[] bytes=new UTF8Encoding(false).GetBytes(content);
        try {
            using(var stream=new FileStream(path,FileMode.OpenOrCreate,FileAccess.Write,FileShare.ReadWrite|FileShare.Delete)) {
                stream.Write(bytes,0,bytes.Length);stream.SetLength(bytes.Length);stream.Flush();
            }
            return true;
        }catch(IOException){return false;}
    }
    public static bool TryRead(string path,out string content)
    {
        content=null;
        try {
            using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))
            using(var reader=new StreamReader(stream,Encoding.UTF8))content=reader.ReadToEnd();
            return true;
        }catch(IOException){return false;}
    }
}
