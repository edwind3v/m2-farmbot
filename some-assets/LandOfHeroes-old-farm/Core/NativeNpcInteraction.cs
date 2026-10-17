using System;
using System.Security.Cryptography;
using System.Threading;
// Native fallback for the one statically verified x64 client. Never patches game code.
public static class NativeNpcInteraction
{
    public const string SupportedSha256="803bdbdffd0ad1848a6e780a4c1db2454366b706483199787865fe96bee92631";
    public static bool Supports(string sha,bool is64){return is64&&string.Equals(sha,SupportedSha256,StringComparison.OrdinalIgnoreCase);}
    public static bool Open(ProcessMemory memory,GameCallBridge bridge,int vid,int mainVid,int race,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var process=memory.Field_0197?.Property_0268;
        if(process==null||process.HasExited)throw new InvalidOperationException("Oyun bağlantısı yok.");
        var profile=ClientProfile.For(process);
        if(!Supports(profile.Sha256,profile.Is64Bit))throw new InvalidOperationException("Bu istemci sürümünde faresiz yerel NPC etkileşimi doğrulanmadı.");
        long b=process.MainModule.BaseAddress.ToInt64();
        Verify(memory,b+0x381e60,0x12,"d03ce6c67a1a0032ff464f3bfc8d6c292227e10c6375dc575c46a95a30ac0e4f");
        Verify(memory,b+0x38a740,0xed,"1f12c7cae6c7de16f63458a6617159410b7173fe0820a6e868fb992ca1d730f4");
        Verify(memory,b+0x386550,0x179,"640877050eea191a59605f7be125ff13dc5775b2783ea920a59bb7d5385b434f");
        token.ThrowIfCancellationRequested();
        return bridge.CallNpcInteraction(b+0x381e60,b+0x38a740,b+0x282ff58,b+0x2830660,vid,mainVid,race,token,process.Id);
    }
    private static void Verify(ProcessMemory memory,long address,int size,string expected)
    {
        byte[] bytes=memory.ReadBytes("0x"+address.ToString("X"),size);
        using(var hash=SHA256.Create())
            if(bytes==null||!string.Equals(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-",""),expected,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("NPC motor işlevinin kodu eşleşmedi; çağrı yapılmadı.");
    }
}
