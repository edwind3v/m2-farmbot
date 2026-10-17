using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;

public partial class MainForm {
    private async Task<DungeonLiveSnapshot> ReadDungeonLiveSnapshotAsync(CancellationToken token) {
        if(!IsClientReady())throw new InvalidOperationException("Önce uygulamayı karaktere bağla; haritada açık bırak.");
        int pid=memory.Field_0197.Property_0268.Id;
        string body=DungeonLiveSnapshot.BuildReadScript(ReadVisualCandidateVids().Concat(ReadRadarEntities().Select(e=>e.Property_03C9)),DialogState+DialogSnapshot+"dialog=_dialog_snapshot(best)\n");
        string report=await GamePythonAsync(body,token);
        if(string.IsNullOrWhiteSpace(report)||report.StartsWith("ERR"))throw new InvalidOperationException("Canlı gözlem okunamadı: "+report);
        if(!IsClientReady()||memory.Field_0197.Property_0268.Id!=pid)throw new InvalidOperationException("Gözlem sırasında bağlı istemci değişti.");
        var snapshot=new JavaScriptSerializer{MaxJsonLength=4000000}.Deserialize<DungeonLiveSnapshot>(report);
        if(snapshot==null||snapshot.CharacterVid<=0||string.IsNullOrWhiteSpace(snapshot.Map))throw new InvalidOperationException("Harita/karakter kimliği doğrulanamadı.");
        snapshot.ProcessId=pid;snapshot.CapturedUtc=DateTime.UtcNow;
        float x=TaskX,y=TaskY;
        foreach(var e in snapshot.Entities){e.X=x+e.Dx;e.Y=y+e.Dy*(mapYSign<0?-1:1);PackMobRecord mob;if(packCatalog!=null&&packCatalog.MobByVnum.TryGetValue(e.Vnum,out mob))e.PackName=string.IsNullOrWhiteSpace(mob.LocaleName)?mob.Name:mob.LocaleName;}
        return snapshot;
    }
    private Control CreateDungeonLiveEditor(Form owner,Func<DungeonCommand> current,Action<DungeonCommand> append,Action changed) {
        var root=new Panel{Name="dungeonLiveEditor",Dock=DockStyle.Fill,BackColor=StudioBackground};
        var tools=new FlowLayoutPanel{Dock=DockStyle.Top,Height=94,Padding=new Padding(5)};
        var scan=new Button{Name="liveScan",Text="Şimdi tara",AutoSize=true};
        var auto=new CheckBox{Name="liveAutoScan",Text="Canlı yenile (1,5 sn)",AutoSize=true};
        var search=new TextBox{Name="liveSearch",Width=180};
        var filter=new ComboBox{Width=150,DropDownStyle=ComboBoxStyle.DropDownList};filter.Items.AddRange(new object[]{"Tüm motor türleri","NPC","Taş","Canavar","Diğer"});filter.SelectedIndex=0;
        var selectGame=new Button{Text="Oyunda seçili / imleçteki",AutoSize=true};
        var operation=new ComboBox{Name="liveCommandKind",Width=190,DropDownStyle=ComboBoxStyle.DropDownList};
        foreach(var kind in new[]{DungeonCommandKind.Interact,DungeonCommandKind.Dialogue,DungeonCommandKind.ClearTargets,DungeonCommandKind.GiveItem,DungeonCommandKind.WaitTarget,DungeonCommandKind.WaitTargetGone,DungeonCommandKind.Move})operation.Items.Add(new CommandKindChoice{Value=kind});operation.SelectedIndex=0;
        var add=new Button{Text="Yeni komut ekle",AutoSize=true};var apply=new Button{Text="Seçili komuta aktar",AutoSize=true};
        var test=new Button{Text="Yakında tek tık dene",AutoSize=true};
        var resultTarget=new Button{Text="Doğacak nesne olarak al",AutoSize=true};
        var state=new Label{Name="liveState",Dock=DockStyle.Top,Height=43,ForeColor=StudioMuted,Text="Tara / Canlı yenile ile başlat. Gözlem okur; kendiliğinden tıklamaz, yürümez, cevap vermez."};
        var table=new DataGridView{Name="liveEntities",Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=StudioSurface,AutoGenerateColumns=false};
        foreach(string label in new[]{"Ad / pack adı","Motor türü","VNUM","VID","Mesafe","X / Y","Durum / ipucu"})table.Columns.Add(new DataGridViewTextBoxColumn{HeaderText=label,SortMode=DataGridViewColumnSortMode.NotSortable,FillWeight=label=="Ad / pack adı"?150:label=="Durum / ipucu"?180:75});
        var lower=new TabControl{Dock=DockStyle.Bottom,Height=150};
        var logPage=new TabPage("Gözlem günlüğü");var log=new TextBox{Name="liveEventLog",Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical};logPage.Controls.Add(log);
        var dialogPage=new TabPage("Açık NPC cevapları / envanter");
        var answers=new ComboBox{Name="liveAnswers",Width=410,DropDownStyle=ComboBoxStyle.DropDownList};var addAnswer=new Button{Text="Cevabı komuta ekle",AutoSize=true};
        var items=new ComboBox{Name="liveItems",Width=410,DropDownStyle=ComboBoxStyle.DropDownList};var addItem=new Button{Text="Eşya ID'sini komuta aktar",AutoSize=true};
        var detail=new Label{Name="liveDetail",Dock=DockStyle.Bottom,Height=55,Text="Kapı/tetikleyici rolü motor türünden kesin çıkarılamaz. Tıklama davranışını dene ve sonuç koşulunu seç."};
        var capturePanel=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(7)};capturePanel.Controls.AddRange(new Control[]{answers,addAnswer,items,addItem});dialogPage.Controls.Add(capturePanel);dialogPage.Controls.Add(detail);lower.TabPages.AddRange(new[]{logPage,dialogPage});
        tools.Controls.AddRange(new Control[]{scan,auto,new Label{Text="Ad / VNUM ara:",AutoSize=true},search,filter,selectGame,operation,add,apply,test,resultTarget});
        root.Controls.Add(table);root.Controls.Add(lower);root.Controls.Add(state);root.Controls.Add(tools);
        DungeonLiveSnapshot snapshot=null;QuestDialogInfo conversation=null;bool busy=false,observerDisposed=false;
        var lifetime=new CancellationTokenSource();var timer=new System.Windows.Forms.Timer{Interval=1500};
        var events=new Queue<string>();
        Action<string> record=text=>{events.Enqueue(DateTime.Now.ToString("HH:mm:ss")+" · "+text);while(events.Count>100)events.Dequeue();log.Text=string.Join(Environment.NewLine,events.Reverse());};
        Func<int> pid=()=>memory?.Field_0197?.Property_0268?.Id??0;
        Func<DungeonLiveEntity> selected=()=>table.SelectedRows.Count>0?table.SelectedRows[0].Tag as DungeonLiveEntity:null;
        Action fresh=()=>{if(snapshot==null||!snapshot.IsFresh(pid(),DateTime.UtcNow))throw new InvalidOperationException("Kayıt eski / istemci değişti. Şimdi tara ile yeniden oku.");};
        Action render=()=>{
            int keep=selected()?.Vid??0;table.Rows.Clear();if(snapshot==null)return;
            foreach(var e in snapshot.Entities.Where(e=>(filter.SelectedIndex==0||(filter.SelectedIndex==4?e.TypeText.StartsWith("Diğer"):e.TypeText==(string)filter.SelectedItem))&&((e.Name??"")+" "+e.PackName+" "+e.Vnum).IndexOf(search.Text,StringComparison.CurrentCultureIgnoreCase)>=0).OrderBy(e=>e.Distance)){
                int row=table.Rows.Add(string.IsNullOrWhiteSpace(e.Name)?e.PackName:e.Name,e.TypeText,e.Vnum,e.Vid,(int)e.Distance,(int)e.X+" / "+(int)e.Y,(e.Dead==true?"Ölü · ":e.Dead==null?"Ölüm API bilinmiyor · ":"")+e.Hint);table.Rows[row].Tag=e;
            }
            table.ClearSelection();foreach(DataGridViewRow row in table.Rows)if((row.Tag as DungeonLiveEntity)?.Vid==keep)row.Selected=true;
        };
        Func<Task> read=async()=>{
            if(busy||owner.IsDisposed)return;busy=true;scan.Enabled=false;
            try{
                using(var cancel=emergencyOperations==null?CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token):CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token,emergencyOperations.Token)){
                    var next=await ReadDungeonLiveSnapshotAsync(cancel.Token);if(owner.IsDisposed)return;
                    foreach(string change in next.ChangesFrom(snapshot))record(change);snapshot=next;render();
                    state.Text=next.CharacterName+" · "+next.Map+" · "+next.Entities.Count+" nesne · "+DateTime.Now.ToString("HH:mm:ss")+(next.Complete?"":" · bazı nesneler okunamadı")+" · Tıklama API: "+(next.ClickApi?"var (davranış bilinmiyor)":"yok");
                    int keepAnswer=(answers.SelectedItem as DialogueEditorChoice)?.Index??int.MinValue;string oldDialog=conversation?.Snapshot;
                    conversation=ParseQuestDialogReport(next.Dialog);answers.Items.Clear();if(conversation!=null){foreach(var a in conversation.Answers)answers.Items.Add(new DialogueEditorChoice{Index=a.Index,Text=(a.Index+1)+". "+a.Caption});if(conversation.NextShown)answers.Items.Add(new DialogueEditorChoice{Index=-1,Text="Devam / Tamam"});}if(answers.Items.Count>0){answers.SelectedIndex=0;if(conversation.Snapshot==oldDialog)foreach(DialogueEditorChoice a in answers.Items)if(a.Index==keepAnswer)answers.SelectedItem=a;}
                    uint keepItem=(items.SelectedItem as DungeonItemChoice)?.Id??0;
                    items.Items.Clear();foreach(var g in next.Items.GroupBy(i=>i.Id)){PackItemRecord pack;string name=packCatalog!=null&&packCatalog.ItemByVnum.TryGetValue(g.Key,out pack)?(pack.LocaleName??pack.Name):"Envanter eşyası";items.Items.Add(new DungeonItemChoice{Id=g.Key,Name=name,Count=g.Sum(i=>i.Count)});}if(items.Items.Count>0){items.SelectedIndex=0;foreach(DungeonItemChoice item in items.Items)if(item.Id==keepItem)items.SelectedItem=item;}
                    detail.Text=(conversation==null?"Açık NPC konuşması yok.":conversation.ToString())+(next.InventoryComplete?"":" · Envanterin bazı yuvaları okunamadı.");
                }
            }catch(OperationCanceledException){if(!owner.IsDisposed)state.Text="Gözlem durduruldu.";}
            catch(Exception ex){snapshot=null;conversation=null;if(!owner.IsDisposed){table.Rows.Clear();answers.Items.Clear();items.Items.Clear();state.Text=ex.Message;auto.Checked=false;}}
            finally{busy=false;if(!owner.IsDisposed)scan.Enabled=true;}
        };
        Action<Action> guarded=action=>{try{if(busy)throw new InvalidOperationException("Taramayı bekle.");fresh();action();}catch(Exception ex){state.Text=ex.Message;}};
        scan.Click+=async(s,e)=>await read();timer.Tick+=async(s,e)=>await read();auto.CheckedChanged+=(s,e)=>{timer.Enabled=auto.Checked;if(!auto.Checked)state.Text="Canlı yenile kapalı. Son kayıt 5 saniye sonra aktarım için geçersizdir.";};
        search.TextChanged+=(s,e)=>render();filter.SelectedIndexChanged+=(s,e)=>render();
        selectGame.Click+=(s,e)=>guarded(()=>{int target=snapshot.SelectedVid>0?snapshot.SelectedVid:snapshot.HoverVid;var row=table.Rows.Cast<DataGridViewRow>().FirstOrDefault(r=>(r.Tag as DungeonLiveEntity)?.Vid==target);if(row==null)throw new InvalidOperationException("Oyunda seçili hedef okunmadı veya filtre dışında.");table.ClearSelection();row.Selected=true;table.FirstDisplayedScrollingRowIndex=row.Index;});
        add.Click+=(s,e)=>guarded(()=>{var command=snapshot.Capture(selected(),((CommandKindChoice)operation.SelectedItem).Value,pid(),DateTime.UtcNow);append(command);state.Text="Canlı kimlik, motor türü, harita ve konum yeni komuta aktarıldı. Kaydetmeden profil değişmez.";});
        apply.Click+=(s,e)=>guarded(()=>{var c=current();if(c==null)throw new InvalidOperationException("Önce akış listesinden komut seç.");if(c.Kind!=DungeonCommandKind.Move&&c.Kind!=DungeonCommandKind.Interact&&c.Kind!=DungeonCommandKind.Dialogue&&c.Kind!=DungeonCommandKind.GiveItem&&c.Kind!=DungeonCommandKind.ClearTargets&&c.Kind!=DungeonCommandKind.WaitTarget&&c.Kind!=DungeonCommandKind.WaitTargetGone&&!((c.Kind==DungeonCommandKind.If||c.Kind==DungeonCommandKind.RepeatWhile)&&c.Condition==DungeonCondition.TargetExists))throw new InvalidOperationException("Bu komut bir hedef kullanmıyor.");var captured=snapshot.Capture(selected(),c.Kind,pid(),DateTime.UtcNow);c.TargetName=captured.TargetName;c.TargetVnum=captured.TargetVnum;c.TargetKind=captured.TargetKind;c.NativeTargetType=captured.NativeTargetType;c.ConstrainTargetArea=true;c.MapName=captured.MapName;c.X=captured.X;c.Y=captured.Y;changed();state.Text="Hedef aktarıldı; saldırı/eşya/cevap ayarları korundu.";});
        addAnswer.Click+=(s,e)=>guarded(()=>{var c=current();var choice=answers.SelectedItem as DialogueEditorChoice;if(c==null||c.Kind!=DungeonCommandKind.Dialogue||conversation==null||choice==null)throw new InvalidOperationException("NPC cevap komutunu ve canlı cevabı seç.");var page=DialoguePage(conversation);if(choice.Index<0)c.DialogueSteps.Add(new NpcDialogueStep{Kind=NpcDialogueChoiceKind.Next,ExpectedMenu=page.MenuKey});else{var a=conversation.Answers.First(x=>x.Index==choice.Index);c.DialogueSteps.Add(new NpcDialogueStep{Kind=string.IsNullOrWhiteSpace(a.Caption)?NpcDialogueChoiceKind.Position:NpcDialogueChoiceKind.Caption,Caption=a.Caption,AnswerIndex=a.Index,ExpectedAnswerCount=conversation.Answers.Count,ExpectedMenu=page.MenuKey});}changed();state.Text="Cevap taslağa eklendi; oyunda basılmadı.";});
        addItem.Click+=(s,e)=>guarded(()=>{var c=current();var item=items.SelectedItem as DungeonItemChoice;if(c==null||item==null)throw new InvalidOperationException("Komut ve envanter eşyası seç.");c.ItemId=item.Id;changed();state.Text="Kalıcı eşya ID aktarıldı; değişken envanter yuvası kaydedilmedi.";});
        resultTarget.Click+=(s,e)=>guarded(()=>{var c=current();var target=selected();if(c==null||c.Kind!=DungeonCommandKind.Interact||target==null||target.Vnum<=0)throw new InvalidOperationException("Tıklama komutunu ve yeni doğmasını bekleyeceğin VNUM'u okunmuş nesneyi seç.");c.InteractionResult=DungeonInteractionResult.TargetAppeared;c.ResultTargetVnum=target.Vnum;changed();state.Text="Sonuç VNUM aktarıldı; asıl tıklanacak hedef değiştirilmedi. Tıklamadan sonra yeni görünen VID beklenecek.";});
        test.Click+=async(s,e)=>{
            if(busy)return;
            var e0=selected();if(e0==null){state.Text="Önce canlı nesne seç.";return;}
            try{
                fresh();busy=true;test.Enabled=false;
                var next=await ReadDungeonLiveSnapshotAsync(lifetime.Token);if(!snapshot.SameScene(next))throw new InvalidOperationException("Sahne değişti; tıklanmadı.");
                var target=next.Entities.FirstOrDefault(e1=>e1.Vid==e0.Vid&&e1.Vnum==e0.Vnum&&e1.MotorType==e0.MotorType&&e1.Name==e0.Name);
                if(target==null||target.Dead==true||target.Distance>350||!next.ClickApi)throw new InvalidOperationException("Hedef kimliği / yakınlığı / tıklama API doğrulanamadı. Yürüyüş yapılmadı.");
                var c=next.Capture(target,DungeonCommandKind.Interact,pid(),DateTime.UtcNow);
                await FlowInteractAsync(c,new FlowTarget{Vid=target.Vid,Distance=target.Distance},lifetime.Token,next.ProcessId);
                record("Tek etkileşim gönderildi: "+target+". Sonucu canlı kayıttan kontrol et.");state.Text="Tek tık gönderildi; başarı sonucu varsayılmadı.";
            }catch(Exception ex){if(!owner.IsDisposed)state.Text=ex.Message;}
            finally{busy=false;if(!owner.IsDisposed)test.Enabled=true;}
        };
        owner.FormClosing+=(s,e)=>{if(!observerDisposed){timer.Stop();lifetime.Cancel();}};
        owner.Disposed+=(s,e)=>{if(observerDisposed)return;observerDisposed=true;timer.Dispose();lifetime.Cancel();lifetime.Dispose();};
        return root;
    }
}
