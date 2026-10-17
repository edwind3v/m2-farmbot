using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class NpcDialogueRunner
{
    // No coordinate clicks, implicit answers, or reopening a closed conversation.
    // A successful dispatch must be followed by a page change before completion.
    public static async Task RunAsync(IEnumerable<NpcDialogueStep> steps,
        Func<CancellationToken,Task<NpcDialoguePage>> read,
        Func<NpcDialoguePage,int,CancellationToken,Task<bool>> press,
        Action<string> status,CancellationToken token,int timeoutMs=15000,int pollMs=250)
    {
        var flow=new NpcDialogueFlow(steps);
        if(flow.Count==0)throw new InvalidOperationException("Cevap sırası boş; önce adım ekle.");
        var watch=Stopwatch.StartNew();string lastSent=null;
        while(true)
        {
            token.ThrowIfCancellationRequested();
            var page=await read(token);
            token.ThrowIfCancellationRequested();
            if(flow.Complete)
            {
                if(page==null||page.Snapshot!=lastSent){status?.Invoke(flow.Count+"/"+flow.Count+" cevap tamamlandı.");return;}
                status?.Invoke("Son cevap gönderildi; pencerenin ilerlemesi bekleniyor.");
            }
            else
            {
                int index=flow.Choose(page);
                status?.Invoke("Adım "+(flow.Position+1)+"/"+flow.Count+" · Beklenen: "+flow.ExpectedAnswer);
                if(index!=-2&&await press(page,index,token))
                {
                    flow.Confirm(page,index);lastSent=page.Snapshot;watch.Restart();
                }
            }
            if(watch.ElapsedMilliseconds>=timeoutMs)
                throw new TimeoutException(flow.Complete?"Son cevap gönderildi ama konuşma ilerlemedi; otomatik tekrar tıklanmadı.":"Adım "+(flow.Position+1)+"/"+flow.Count+" uygulanamadı. Beklenen: "+flow.ExpectedAnswer+". Açık cevapları yeniden oku; sıra baştan çalışır.");
            await Task.Delay(pollMs,token);
        }
    }
}
