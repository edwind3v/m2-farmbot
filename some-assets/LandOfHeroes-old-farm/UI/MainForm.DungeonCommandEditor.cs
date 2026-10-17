using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm {
    private Form CreateDungeonCommandEditor() {
        var draft=(taskSequence.Commands??new List<DungeonCommand>()).Select(c=>c.Copy()).ToList();
        var form=new Form{Name="dungeonCommandEditor",Text=taskSequence.Name+" · Komutlu zindan editörü",ClientSize=new Size(1050,720),MinimumSize=new Size(1000,720),StartPosition=FormStartPosition.CenterParent,Font=new Font("Segoe UI",10),BackColor=StudioBackground,ForeColor=StudioText};
        var enable=new CheckBox{Name="commandFlowEnabled",Text="Bu zindanda komut akışını kullan (eski 8 aşamalı mod yerine)",Checked=taskSequence.UseCommandFlow,Dock=DockStyle.Top,Height=38};
        var note=new Label{Dock=DockStyle.Top,Height=54,Text="Adımlar sırayla çalışır. Eğer / Değilse / Koşul sonu ile dallan; Tekrar başlat / Tekrar sonu ile döngü kur.\nHedefleri ve eşyaları packten ya da canlı oyundan seç. Eşya verme komutu onaysız tekrarlanmaz."};
        var panel=new SplitContainer{Dock=DockStyle.Fill,Size=new Size(1050,500),SplitterDistance=420,Panel1MinSize=250,Panel2MinSize=440,FixedPanel=FixedPanel.Panel1,BackColor=StudioBackground};
        var list=new ListBox{Name="dungeonCommands",Dock=DockStyle.Fill,HorizontalScrollbar=true,BackColor=StudioSurface,ForeColor=StudioText};
        var grid=new PropertyGrid{Name="commandProperties",Dock=DockStyle.Fill,HelpVisible=true,ToolbarVisible=false,PropertySort=PropertySort.Categorized,BackColor=StudioSurface,ForeColor=StudioText,ViewBackColor=StudioSurface,ViewForeColor=StudioText};
        panel.Panel1.Controls.Add(list);panel.Panel2.Controls.Add(grid);
        var buttons=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=125,Padding=new Padding(8)};
        var kinds=new ComboBox{Name="commandKind",Width=255,DropDownStyle=ComboBoxStyle.DropDownList};
        foreach(DungeonCommandKind kind in Enum.GetValues(typeof(DungeonCommandKind)))kinds.Items.Add(new CommandKindChoice{Value=kind});kinds.SelectedIndex=0;
        var status=new Label{Name="commandValidation",Dock=DockStyle.Bottom,Height=52,ForeColor=StudioMuted,Text="Kaydetmeden önce Doğrula kullan. Başlangıç ve son blokları eşleşmeli."};
        Action<int> refresh=selected=>{list.BeginUpdate();list.Items.Clear();for(int i=0;i<draft.Count;i++)list.Items.Add((i+1).ToString("000")+" · "+draft[i]);list.EndUpdate();if(draft.Count>0)list.SelectedIndex=Math.Max(0,Math.Min(draft.Count-1,selected));else grid.SelectedObject=null;};
        Func<DungeonCommand> current=()=>list.SelectedIndex>=0?draft[list.SelectedIndex]:null;
        Func<string,Action,Button> button=(text,action)=>{var btn=new Button{Text=text,AutoSize=true,Height=32,FlatStyle=FlatStyle.Flat,ForeColor=StudioText};btn.Click+=(s,e)=>{try{action();}catch(Exception ex){status.Text=ex.Message;}};return btn;};
        buttons.Controls.Add(kinds);
        buttons.Controls.Add(button("Ekle",()=>{int i=list.SelectedIndex+1;draft.Insert(i,new DungeonCommand{Kind=((CommandKindChoice)kinds.SelectedItem).Value});refresh(i);}));
        buttons.Controls.Add(button("Kopyala",()=>{var c=current();if(c==null)return;int i=list.SelectedIndex+1;draft.Insert(i,c.Copy());refresh(i);}));
        buttons.Controls.Add(button("Sil",()=>{int i=list.SelectedIndex;if(i<0)return;draft.RemoveAt(i);refresh(i);}));
        Action<int> move=delta=>{int i=list.SelectedIndex,j=i+delta;if(i<0||j<0||j>=draft.Count)return;var c=draft[i];draft.RemoveAt(i);draft.Insert(j,c);refresh(j);};
        buttons.Controls.Add(button("Yukarı",()=>move(-1)));buttons.Controls.Add(button("Aşağı",()=>move(1)));
        buttons.Controls.Add(button("Hedef seç (pack / radar)",()=>{var c=current();if(c!=null){PickCommandTarget(c,form);grid.Refresh();refresh(list.SelectedIndex);}}));
        buttons.Controls.Add(button("Eşya seç (pack / envanter)",()=>{var c=current();if(c!=null){PickCommandItem(c,form);grid.Refresh();}}));
        buttons.Controls.Add(button("Şu anki konumu al",()=>{var c=current();if(c==null)return;if(!IsClientReady())throw new InvalidOperationException("Önce oyuna bağlanın.");c.X=TaskX;c.Y=TaskY;c.MapName=atlasName??"";grid.Refresh();}));
        buttons.Controls.Add(button("NPC cevaplarını düzenle",()=>{
            var c=current();if(c==null)return;
            var stage=new TaskStage{Name=c.Name,NpcName=c.TargetName,NpcVnum=c.TargetVnum,CustomDialogueEnabled=true,DialogueSteps=c.DialogueSteps.Select(d=>d.Copy()).ToList()};
            npcDialogueEditorOpen=true;
            try{using(var editor=CreateNpcDialogueEditor(stage,false))if(editor.ShowDialog(form)==DialogResult.OK)c.DialogueSteps=stage.DialogueSteps.Select(d=>d.Copy()).ToList();}
            finally{npcDialogueEditorOpen=false;}
            status.Text=c.DialogueSteps.Count+" cevap tanımlı. Kaydet ile komut akışına yazılır.";
        }));
        buttons.Controls.Add(button("Oda / kapı örneği (düzenlenir)",()=>{
            if(draft.Count>0&&MessageBox.Show(form,"Editördeki taslağı oda şablonuyla değiştireyim mi? Kayıtlı görev Kaydet demeden değişmez.","Şablon",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            draft=CreateRoomCommandTemplate();refresh(0);status.Text="Şablon taslaktır. Anahtar/taş eşyalarını, kapıları, tetikleyicileri ve patronları seçmelisin.";
        }));
        buttons.Controls.Add(button("Doğrula",()=>{new DungeonFlowPlan(draft);status.Text="Akış yapısı geçerli · "+draft.Count+" adım. Canlı oyun doğrulaması henüz yapılmadı.";}));
        buttons.Controls.Add(button("Kaydet",()=>{
            if(enable.Checked)new DungeonFlowPlan(draft);
            taskSequence.Commands=draft.Select(c=>c.Copy()).ToList();taskSequence.UseCommandFlow=enable.Checked;
            AutoSaveTaskSequence();status.Text="Komut akışı kaydedildi.";form.DialogResult=DialogResult.OK;form.Close();
        }));
        list.SelectedIndexChanged+=(s,e)=>grid.SelectedObject=current()==null?null:new DungeonCommandView(current());
        grid.PropertyValueChanged+=(s,e)=>refresh(list.SelectedIndex);
        var tabs=new TabControl{Name="commandEditorTabs",Dock=DockStyle.Fill};
        var flowTab=new TabPage("Komut akışı");flowTab.Controls.Add(panel);
        var liveTab=new TabPage("Oyundan canlı tespit");
        liveTab.Controls.Add(CreateDungeonLiveEditor(form,current,c=>{int i=list.SelectedIndex+1;draft.Insert(i,c);refresh(i);},()=>refresh(list.SelectedIndex)));
        tabs.TabPages.AddRange(new[]{flowTab,liveTab});
        tabs.SelectedIndexChanged+=(s,e)=>{
            bool live=tabs.SelectedTab==liveTab;
            foreach(Control control in buttons.Controls)control.Visible=!live||control.Text=="Doğrula"||control.Text=="Kaydet";
            buttons.Height=live?60:125;status.Height=live?34:52;
            status.Text=live?"Canlı aktarım taslağı değiştirir. Sonuç koşulunu Komut akışı sekmesinde düzenle; Kaydet ile sakla.":"Kaydetmeden önce Doğrula kullan. Başlangıç ve son blokları eşleşmeli.";
        };
        form.Controls.Add(tabs);form.Controls.Add(status);form.Controls.Add(buttons);form.Controls.Add(note);form.Controls.Add(enable);
        refresh(0);return form;
    }
    private sealed class CommandKindChoice{public DungeonCommandKind Value;public override string ToString(){return DungeonCommand.Label(Value);}}
    private void OpenDungeonCommandEditor() {
        if(taskCancellation!=null||normalBotRequested||RouteOwnsCombat){MessageBox.Show("Komutları düzenlemek için çalışan görev/rota/botu durdurun.");return;}
        using(var editor=CreateDungeonCommandEditor())editor.ShowDialog(this);
    }
    private object PickCommandValue(string title,IEnumerable<object> values,Form owner) {
        using(var picker=new Form{Text=title,ClientSize=new Size(630,470),StartPosition=FormStartPosition.CenterParent,BackColor=StudioBackground,ForeColor=StudioText}) {
            var search=new TextBox{Dock=DockStyle.Top};var list=new ListBox{Dock=DockStyle.Fill,HorizontalScrollbar=true,BackColor=StudioSurface,ForeColor=StudioText};
            var choose=new Button{Text="Seç",Dock=DockStyle.Bottom,Height=36,DialogResult=DialogResult.OK};
            object[] all=values.ToArray();Action filter=()=>{list.Items.Clear();foreach(var value in all.Where(v=>v.ToString().IndexOf(search.Text,StringComparison.CurrentCultureIgnoreCase)>=0).Take(1000))list.Items.Add(value);};
            search.TextChanged+=(s,e)=>filter();list.DoubleClick+=(s,e)=>{if(list.SelectedItem!=null){picker.DialogResult=DialogResult.OK;picker.Close();}};
            picker.Controls.Add(list);picker.Controls.Add(search);picker.Controls.Add(choose);filter();picker.AcceptButton=choose;
            return picker.ShowDialog(owner)==DialogResult.OK?list.SelectedItem:null;
        }
    }
    private void PickCommandItem(DungeonCommand c,Form owner) {
        var rows=new List<object>();if(packCatalog!=null)rows.AddRange(packCatalog.Items);
        rows.AddRange(visibleItems.GroupBy(i=>i.Id).Select(g=>(object)new DungeonItemChoice{Id=g.Key,Name=g.First().Name,Count=g.Sum(i=>(long)i.Count)}));
        object choice=PickCommandValue("Eşya seç · ID veya isimle ara",rows,owner);
        var pack=choice as PackItemRecord;if(pack!=null){c.ItemId=pack.Vnum;return;}
        var live=choice as DungeonItemChoice;if(live!=null)c.ItemId=live.Id;
    }
    private void PickCommandTarget(DungeonCommand c,Form owner) {
        var rows=new List<object>();if(packCatalog!=null)rows.AddRange(packCatalog.Mobs);
        rows.AddRange(ReadRadarEntities().Select(e=>(object)new FlowLiveChoice{Entity=e}));
        object choice=PickCommandValue("Hedef seç · pack VNUM veya canlı ad",rows,owner);
        var pack=choice as PackMobRecord;
        if(pack!=null){c.TargetVnum=pack.Vnum;c.TargetName="";c.TargetKind=pack.Kind==PackTargetKind.Stone?"Taş":pack.Kind==PackTargetKind.Monster||pack.Kind==PackTargetKind.Boss?"Canavar":"NPC";return;}
        var live=choice as FlowLiveChoice;if(live!=null){c.TargetVnum=0;c.TargetName=live.Entity.Property_03CD;c.TargetKind=live.Entity.Property_03CA==AddressCatalog.Field_035B?"Taş":live.Entity.Property_03CA==AddressCatalog.Field_035C?"Canavar":"NPC";}
    }
    private sealed class FlowLiveChoice{public EntityState Entity;public override string ToString(){return Entity.Property_03CD+" · canlı VID "+Entity.Property_03C9;}}
    private static List<DungeonCommand> CreateRoomCommandTemplate() {
        return new List<DungeonCommand>{
            new DungeonCommand{Kind=DungeonCommandKind.Repeat,Name="Oda döngüsü · tekrar sayısını seç",RepeatCount=1},
            new DungeonCommand{Kind=DungeonCommandKind.ClearTargets,Name="Odadaki slotları temizle",TargetKind="Canavar"},
            new DungeonCommand{Kind=DungeonCommandKind.CollectItem,Name="Anahtarı topla · eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.GiveItem,Name="Anahtarı oda kapısına ver · kapı ve eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.Interact,Name="Oda içindeki metin tetikleyicisine tıkla · hedef seç"},
            new DungeonCommand{Kind=DungeonCommandKind.ClearTargets,Name="Doğan metni kes",TargetKind="Taş"},
            new DungeonCommand{Kind=DungeonCommandKind.CollectItem,Name="Düşen taşı topla · eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.GiveItem,Name="Taşı merkez nesnesine ver · hedef ve eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.EndRepeat},
            new DungeonCommand{Kind=DungeonCommandKind.Repeat,Name="Patron döngüsü · tekrar sayısını seç",RepeatCount=1},
            new DungeonCommand{Kind=DungeonCommandKind.ClearTargets,Name="Bir patronu kes · patron filtresi seç",TargetKind="Canavar",KillLimit=1},
            new DungeonCommand{Kind=DungeonCommandKind.CollectItem,Name="Patron eşyasını topla · eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.GiveItem,Name="Patron eşyasını ilgili hedefe ver · hedef seç"},
            new DungeonCommand{Kind=DungeonCommandKind.EndRepeat},
            new DungeonCommand{Kind=DungeonCommandKind.GiveItem,Name="Son kapıya eşya ver · kapı ve eşya seç"},
            new DungeonCommand{Kind=DungeonCommandKind.ClearTargets,Name="Ana patronu kes · patron VNUM seç",TargetKind="Canavar"},
            new DungeonCommand{Kind=DungeonCommandKind.Dialogue,Name="Çıkış NPC ve cevaplarını seç"},
            new DungeonCommand{Kind=DungeonCommandKind.WaitMap,Name="Çıkış haritasını bekle · harita adı gir"}
        };
    }
}
