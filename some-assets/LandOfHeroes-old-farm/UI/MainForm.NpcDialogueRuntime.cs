using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    private TaskStage configuredDialogueStage;
    private NpcDialogueFlow configuredDialogue;
    private void ResetConfiguredDialogue(){configuredDialogueStage=null;configuredDialogue=null;}
    private static NpcDialoguePage DialoguePage(QuestDialogInfo dialog)
    {
        return new NpcDialoguePage{Snapshot=dialog.Snapshot,NextShown=dialog.NextShown,Answers=dialog.Answers.Select(a=>new NpcDialogueOption{Index=a.Index,Caption=a.Caption,Icon=a.Icon}).ToList()};
    }
    private NpcDialogueFlow ConfiguredDialogue(TaskStage stage)
    {
        if(configuredDialogueStage!=stage){configuredDialogueStage=stage;configuredDialogue=new NpcDialogueFlow(stage.DialogueSteps);}
        return configuredDialogue;
    }
    private async Task ApplyConfiguredAnswerAsync(TaskStage stage,NpcDialogueFlow flow,QuestDialogInfo dialog,CancellationToken token)
    {
        if(flow.Complete){taskStatus.Text=stage.Number+". aşama · NPC cevap sırası bitti; geçiş bekleniyor.";return;}
        var page=DialoguePage(dialog);int index=flow.Choose(page);
        if(index==-2){taskStatus.Text=stage.Number+". aşama · cevap "+(flow.Position+1)+"/"+flow.Count+" bekleniyor: "+flow.ExpectedAnswer;return;}
        if(await PressQuestDialogAsync(dialog,index,token))
        {
            flow.Confirm(page,index);
            taskStatus.Text=stage.Number+". aşama · NPC cevabı "+flow.Position+"/"+flow.Count+" uygulandı.";
        }
    }
    private async Task RunConfiguredNpcEntryAsync(TaskStage stage,CancellationToken token)
    {
        var flow=new NpcDialogueFlow(stage.DialogueSteps);
        if(flow.Count==0)throw new InvalidOperationException("Özel NPC cevap sırası boş; zindan editöründen cevap ekleyin.");
        string npc=ConfiguredDungeonNpcName(1);
        taskStatus.Text=stage.Number+". aşama · NPC aranıyor/açılıyor: "+npc;
        if(await ReadQuestDialogAsync(token)==null)await ClickNpcVerifiedAsync(npc,token,null,stage.NpcVid,null,LobbyMap);
        await RunNpcDialogueStepsAsync(stage.DialogueSteps,text=>taskStatus.Text=stage.Number+". aşama · "+text,token);
        taskStatus.Text=stage.Number+". aşama · NPC cevapları tamamlandı; harita geçişi bekleniyor (30 sn).";
        var lastProgress=DateTime.UtcNow;
        while(true)
        {
            token.ThrowIfCancellationRequested();
            string map=await GamePythonAsync("import background\nf.write(background.GetCurrentMapName())",token);
            if(!string.IsNullOrWhiteSpace(map)&&!map.StartsWith("ERR")&&map!=LobbyMap)return;
            if(DateTime.UtcNow-lastProgress>TimeSpan.FromSeconds(30))
                throw new TimeoutException("NPC cevapları tamamlandı ama harita geçişi gerçekleşmedi.");
            await Task.Delay(300,token);
        }
    }
}
