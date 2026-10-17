using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public enum NpcDialogueChoiceKind { Caption, Next, Position }
[Serializable]
public sealed class NpcDialogueStep
{
    public NpcDialogueChoiceKind Kind { get; set; }
    public string Caption { get; set; } = "";
    public int AnswerIndex { get; set; }
    public int ExpectedAnswerCount { get; set; }
    public string ExpectedMenu { get; set; } = "";
    public NpcDialogueStep Copy(){return (NpcDialogueStep)MemberwiseClone();}
    public override string ToString(){return Kind==NpcDialogueChoiceKind.Next?"Devam / Tamam düğmesi":Kind==NpcDialogueChoiceKind.Position?"Yazısız seçenek · sıra "+(AnswerIndex+1):Caption+(string.IsNullOrEmpty(ExpectedMenu)?"":" · seçenek "+(AnswerIndex+1));}
}
public sealed class NpcDialogueOption
{
    public int Index;public string Caption;public bool Icon;
}
public sealed class NpcDialoguePage
{
    public string Snapshot;public bool NextShown;public List<NpcDialogueOption> Answers=new List<NpcDialogueOption>();
    public static string Normalize(string text){return (text??"").Trim().ToLower(new CultureInfo("tr-TR"));}
    public string MenuKey { get { return string.Join("|",Answers.Select(a=>a.Index+":"+a.Icon+":"+Normalize(a.Caption))); } }
}
public sealed class NpcDialogueFlow
{
    private readonly List<NpcDialogueStep> steps;
    private string handledSnapshot;
    public int Position { get; private set; }
    public int Count { get { return steps.Count; } }
    public bool Complete { get { return Position>=steps.Count; } }
    public string ExpectedAnswer { get { return Complete?"":steps[Position].ToString(); } }
    public NpcDialogueFlow(IEnumerable<NpcDialogueStep> source){steps=(source??Enumerable.Empty<NpcDialogueStep>()).Select(s=>s.Copy()).ToList();}
    public int Choose(NpcDialoguePage page)
    {
        if(Complete||page==null||string.IsNullOrEmpty(page.Snapshot)||page.Snapshot==handledSnapshot)return -2;
        var step=steps[Position];
        if(step.Kind==NpcDialogueChoiceKind.Next)return page.NextShown?-1:-2;
        if(step.Kind==NpcDialogueChoiceKind.Position)
            return page.Answers.Count==step.ExpectedAnswerCount&&page.MenuKey==step.ExpectedMenu&&page.Answers.Any(a=>a.Index==step.AnswerIndex)?step.AnswerIndex:-2;
        string wanted=NpcDialoguePage.Normalize(step.Caption);
        if(wanted.Length==0)return -2;
        var matches=page.Answers.Where(a=>NpcDialoguePage.Normalize(a.Caption)==wanted).ToList();
        if(matches.Count==1)return matches[0].Index;
        // Kellam can show the same caption twice (normal and icon-marked dungeon).
        // Previously saved captured steps already contain this full menu fingerprint.
        if(matches.Count>1&&step.ExpectedAnswerCount==page.Answers.Count&&step.ExpectedMenu==page.MenuKey&&matches.Any(a=>a.Index==step.AnswerIndex))return step.AnswerIndex;
        return -2;
    }
    public bool Confirm(NpcDialoguePage page,int index)
    {
        if(index==-2||Choose(page)!=index)return false;
        handledSnapshot=page.Snapshot;Position++;return true;
    }
}
