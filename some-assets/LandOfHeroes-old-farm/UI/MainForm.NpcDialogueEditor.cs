using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private bool npcDialogueEditorOpen;
    private sealed class DialogueEditorChoice
    {
        public int Index;public string Text;
        public override string ToString(){return Text;}
    }
    private void OpenNpcDialogueEditor()
    {
        int index=flowList?.SelectedIndex??-1;
        if(index<0||index>=taskSequence.Stages.Count)return;
        if(taskCancellation!=null||normalBotRequested||RouteOwnsCombat||isRecordingRoute){MessageBox.Show("Cevapları düzenlemek için çalışan rota/görev/botu durdurun.");return;}
        var stage=taskSequence.Stages[index];
        ReadDungeonStageSelections(stage);
        npcDialogueEditorOpen=true;
        try{using(var editor=CreateNpcDialogueEditor(stage))editor.ShowDialog(controlCenterForm??this);}
        finally{npcDialogueEditorOpen=false;}
    }
    private Form CreateNpcDialogueEditor(TaskStage stage,bool persist=true)
    {
        var draft=(stage.DialogueSteps??new List<NpcDialogueStep>()).Select(s=>s.Copy()).ToList();
        var form=new Form{Name="npcDialogueEditor",Text=stage.Number+". aşama · NPC cevap sırası",ClientSize=new Size(850,610),Font=new Font("Segoe UI",10f),StartPosition=FormStartPosition.CenterParent,BackColor=StudioBackground,ForeColor=StudioText,MinimumSize=new Size(866,649)};
        var enabled=new CheckBox{Name="npcDialogueEnabled",Text="Bu aşamada kendi cevap sıramı kullan",Checked=stage.CustomDialogueEnabled,Left=20,Top=18,Width=700};
        var note=new Label{Text="NPC: "+(string.IsNullOrWhiteSpace(stage.NpcName)?"Kellam":stage.NpcName)+" · Cevapları sırayla ekle. Liste uzunluğu konuşma adımı sayısıdır.",Left=20,Top=49,Width=800,Height=42};
        var steps=new ListBox{Name="npcDialogueSteps",Left=20,Top=100,Width=400,Height=340,HorizontalScrollbar=true};
        var count=new Label{Name="npcDialogueCount",Left=20,Top=447,Width=400,Height=24};
        var status=new Label{Name="npcDialogueStatus",Left=445,Top=340,Width=375,Height=125,Text="NPC'nin yanına gel. 'Oku' sadece listeler; 'Sırayı çalıştır' listedeki tüm cevapları baştan uygular."};
        var choices=new ComboBox{Name="npcDialogueChoices",Left=445,Top=158,Width=375,DropDownStyle=ComboBoxStyle.DropDownList};
        var caption=new TextBox{Name="npcDialogueCaption",Left=445,Top=257,Width=375};
        var manualLabel=new Label{Text="Veya cevap düğmesinin tam yazısını gir:",Left=445,Top=231,Width=375,Height=24};
        var open=PlainButton("Seçili NPC'yi aç",445,100,175);
        var read=PlainButton("Açık cevapları oku",630,100,190);
        var add=PlainButton("Seçili cevabı ekle",445,195,175);
        var advance=PlainButton("Seçimi oyunda dene",630,195,190);
        var addText=PlainButton("Yazıyı adım olarak ekle",445,296,215);
        var addNext=PlainButton("Devam ekle",670,296,150);
        var up=PlainButton("Yukarı",20,484,95);var down=PlainButton("Aşağı",125,484,95);var remove=PlainButton("Adımı sil",230,484,190);
        var execute=PlainButton("Sırayı çalıştır",445,484,180);execute.Name="npcDialogueRun";
        var stop=PlainButton("Durdur",640,484,180);stop.Name="npcDialogueStop";stop.Enabled=false;
        var save=PlainButton("Sırayı kaydet",445,548,180);save.Name="npcDialogueSave";
        var cancel=PlainButton("Vazgeç",640,548,180);cancel.DialogResult=DialogResult.Cancel;
        form.Controls.AddRange(new Control[]{enabled,note,steps,count,status,choices,caption,manualLabel,open,read,add,advance,addText,addNext,up,down,remove,execute,stop,save,cancel});
        form.CancelButton=cancel;
        QuestDialogInfo snapshot=null;bool busy=false;
        var cancellation=new CancellationTokenSource();CancellationTokenSource operation=null;
        form.FormClosing+=(s,e)=>cancellation.Cancel();form.Disposed+=(s,e)=>cancellation.Dispose();
        stop.Click+=(s,e)=>operation?.Cancel();
        Action refresh=()=>{
            int selected=steps.SelectedIndex;steps.Items.Clear();
            for(int i=0;i<draft.Count;i++)steps.Items.Add((i+1)+". "+draft[i]);
            if(draft.Count>0)steps.SelectedIndex=Math.Max(0,Math.Min(selected,draft.Count-1));
            count.Text=draft.Count+" konuşma adımı";
        };
        Action<NpcDialogueStep> append=step=>{draft.Add(step);enabled.Checked=true;refresh();steps.SelectedIndex=draft.Count-1;};
        Func<CancellationToken,Task> load=async token=>{
            snapshot=await ReadQuestDialogAsync(token);
            if(form.IsDisposed)return;
            choices.Items.Clear();
            if(snapshot==null){status.Text="Açık NPC konuşması bulunamadı.";return;}
            foreach(var answer in snapshot.Answers)choices.Items.Add(new DialogueEditorChoice{Index=answer.Index,Text=(answer.Index+1)+". "+(answer.Icon?"[simge] ":"")+(string.IsNullOrWhiteSpace(answer.Caption)?"[yazısız seçenek]":answer.Caption)});
            if(snapshot.NextShown)choices.Items.Add(new DialogueEditorChoice{Index=-1,Text="Devam / Tamam düğmesi"});
            if(choices.Items.Count>0)choices.SelectedIndex=0;
            status.Text=snapshot.Ready?"İstediğin cevabı seçip sıraya ekle. Sonraki soruyu görmek için 'Seçimi oyunda dene' kullan.":"Metin yazılıyor; biraz sonra tekrar oku.";
        };
        Control[] editControls={enabled,steps,choices,caption,open,read,add,advance,addText,addNext,up,down,remove,execute,save};
        Func<Func<CancellationToken,Task>,Task> run=async action=>{
            if(busy)return;
            if(!IsClientReady()){status.Text="Önce uygulamayı oyuna bağla.";return;}
            busy=true;foreach(var control in editControls)control.Enabled=false;stop.Enabled=true;
            operation=CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token,emergencyOperations.Token);
            try{await action(operation.Token);}
            catch(OperationCanceledException){if(!form.IsDisposed)status.Text="İşlem durduruldu. Yeniden çalıştırırken konuşmanın ilk sayfasını aç.";}
            catch(Exception ex){if(!form.IsDisposed)status.Text=ex.Message;}
            finally{operation.Dispose();operation=null;busy=false;if(!form.IsDisposed){foreach(var control in editControls)control.Enabled=true;stop.Enabled=false;}}
        };
        open.Click+=async(s,e)=>await run(async token=>{
            snapshot=null;choices.Items.Clear();
            status.Text="Yakındaki seçili NPC doğrulanıyor; yürüyüş veya ekran tıklaması yapılmaz.";
            await OpenNpcDialogueInPlaceAsync(stage,token);
            await load(token);
        });
        read.Click+=async(s,e)=>await run(load);
        execute.Click+=async(s,e)=>await run(async token=>{
            if(draft.Count==0){status.Text="Önce cevap adımlarını ekle.";return;}
            snapshot=null;choices.Items.Clear();
            status.Text="Açık konuşma kullanılacak; yoksa yakındaki NPC açılacak. Sıra baştan çalışır.";
            await OpenNpcDialogueInPlaceAsync(stage,token);
            await RunNpcDialogueStepsAsync(draft,text=>{if(!form.IsDisposed)status.Text=text;},token);
        });
        add.Click+=(s,e)=>{
            var choice=choices.SelectedItem as DialogueEditorChoice;if(snapshot==null||choice==null)return;
            if(choice.Index<0){append(new NpcDialogueStep{Kind=NpcDialogueChoiceKind.Next});return;}
            var answer=snapshot.Answers.First(a=>a.Index==choice.Index);
            append(new NpcDialogueStep{Kind=string.IsNullOrWhiteSpace(answer.Caption)?NpcDialogueChoiceKind.Position:NpcDialogueChoiceKind.Caption,Caption=answer.Caption,AnswerIndex=answer.Index,ExpectedAnswerCount=snapshot.Answers.Count,ExpectedMenu=DialoguePage(snapshot).MenuKey});
        };
        advance.Click+=async(s,e)=>await run(async token=>{
            var choice=choices.SelectedItem as DialogueEditorChoice;if(snapshot==null||choice==null)return;
            bool clicked=await PressQuestDialogAsync(snapshot,choice.Index,token);
            if(!clicked){status.Text="Pencere değişti; cevapları yeniden oku.";return;}
            snapshot=null;choices.Items.Clear();await Task.Delay(400,token);await load(token);
        });
        addText.Click+=(s,e)=>{if(!string.IsNullOrWhiteSpace(caption.Text))append(new NpcDialogueStep{Kind=NpcDialogueChoiceKind.Caption,Caption=caption.Text.Trim()});};
        addNext.Click+=(s,e)=>append(new NpcDialogueStep{Kind=NpcDialogueChoiceKind.Next});
        Action<int> move=delta=>{int i=steps.SelectedIndex,j=i+delta;if(i<0||j<0||j>=draft.Count)return;var item=draft[i];draft.RemoveAt(i);draft.Insert(j,item);refresh();steps.SelectedIndex=j;};
        up.Click+=(s,e)=>move(-1);down.Click+=(s,e)=>move(1);
        remove.Click+=(s,e)=>{int i=steps.SelectedIndex;if(i>=0){draft.RemoveAt(i);refresh();}};
        save.Click+=(s,e)=>{
            if(enabled.Checked&&draft.Count==0){status.Text="Özel sıra için en az bir cevap ekle veya seçeneği kapat.";return;}
            bool previousEnabled=stage.CustomDialogueEnabled;var previousSteps=stage.DialogueSteps;
            stage.CustomDialogueEnabled=enabled.Checked;stage.DialogueSteps=draft.Select(v=>v.Copy()).ToList();
            if(!persist){form.DialogResult=DialogResult.OK;form.Close();return;}
            try
            {
                dungeonTasks=DungeonTaskCollection.Normalize(dungeonTasks);
                dungeonTasks.Dungeons[dungeonTasks.SelectedIndex]=taskSequence;
                DungeonTaskCollectionStore.Save(DungeonTasksPath,dungeonTasks);
            }
            catch(Exception ex){stage.CustomDialogueEnabled=previousEnabled;stage.DialogueSteps=previousSteps;status.Text="Cevap sırası kaydedilemedi: "+ex.Message;return;}
            AutoSaveTaskSequence();ResetConfiguredDialogue();sessionDirty=true;
            form.DialogResult=DialogResult.OK;form.Close();
        };
        refresh();return form;
    }
}
