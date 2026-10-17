using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private sealed class DungeonItemChoice
    {
        public uint Id;public string Name;public long Count;
        public override string ToString(){return (string.IsNullOrWhiteSpace(Name)?"Eşya":Name)+" · ID "+Id+" · x"+Count;}
    }
    private sealed class DungeonNpcChoice
    {
        public int Vid;public int Vnum;public string Name;public float Distance;
        public override string ToString(){return (string.IsNullOrWhiteSpace(Name)?"NPC":Name)+" · VID "+Vid+(Vnum>0?" · VNUM "+Vnum:"")+(Distance>0?" · "+(int)Distance:"");}
    }

    private async Task RefreshDungeonItemChoicesAsync()
    {
        if(!IsClientReady()){MessageBox.Show("Önce oyuna bağlanın.");return;}
        await RefreshInventoryAsync();PopulateDungeonItemChoices();
        if(flowItem.Items.Count==0)MessageBox.Show("Envanterde eşya bulunamadı.");
    }
    private void PopulateDungeonItemChoices()
    {
        if(flowItem==null)return;
        uint keep=(flowItem.SelectedItem as DungeonItemChoice)?.Id??CurrentDungeonStageSetting(flowList?.SelectedIndex+1??0)?.RequiredItemId??0;
        var choices=visibleItems.GroupBy(v=>v.Id).Select(g=>new DungeonItemChoice{Id=g.Key,Name=PackItemName(g.Key,g.Select(v=>v.Name).FirstOrDefault(v=>!string.IsNullOrWhiteSpace(v))??""),Count=g.Sum(v=>(long)v.Count)}).OrderBy(v=>v.Name).ThenBy(v=>v.Id).ToList();
        flowItem.BeginUpdate();flowItem.Items.Clear();foreach(var choice in choices)flowItem.Items.Add(choice);flowItem.EndUpdate();
        var selected=choices.FirstOrDefault(v=>v.Id==keep);if(selected!=null)flowItem.SelectedItem=selected;
    }

    private async Task RefreshDungeonNpcChoicesAsync()
    {
        if(!IsClientReady()){MessageBox.Show("Önce oyuna bağlanın.");return;}
        var radar=ReadRadarEntities().Where(v=>v.Property_03CA==1).OrderBy(v=>v.Distance).ToList();
        var names=new Dictionary<int,string>();var vnums=new Dictionary<int,int>();string vids=string.Join(",",radar.Select(v=>v.Property_03C9).Distinct());
        if(vids.Length>0)
        {
            try
            {
                string body="import chr\nrows=[]\nvfn=getattr(chr,'GetVirtualNumber',None)\nfor v in ["+vids+"]:\n try:\n  n=chr.GetNameByVID(v) or ''\n except Exception:\n  n=''\n try:\n  r=int(vfn(v)) if vfn else 0\n except Exception:\n  r=0\n rows.append(str(v)+'\\t'+str(n).replace('\\t',' ').replace('\\n',' ')+'\\t'+str(r))\nf.write('\\n'.join(rows))";
                string report=await GamePythonAsync(body,CancellationToken.None);
                foreach(string line in report.Split(new[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries))
                {
                    var p=line.Split('\t');int vid,vnum=0;if(p.Length>=1&&int.TryParse(p[0],out vid)){names[vid]=p.Length>1?p[1].Trim():"";if(p.Length>2)int.TryParse(p[2],out vnum);vnums[vid]=vnum;}
                }
            }
            catch(Exception ex){RecordActivity("Zindan","NPC adları okunamadı: "+ex.Message);}
        }
        int keep=(flowNpc.SelectedItem as DungeonNpcChoice)?.Vid??CurrentDungeonStageSetting(flowList?.SelectedIndex+1??0)?.NpcVid??0;
        var choices=radar.Select(v=>
        {
            int vnum=vnums.ContainsKey(v.Property_03C9)?vnums[v.Property_03C9]:0;string name=names.ContainsKey(v.Property_03C9)?names[v.Property_03C9]:(v.Property_03CD??"");PackMobRecord record;
            if(vnum>0&&packCatalog!=null&&packCatalog.MobByVnum.TryGetValue(vnum,out record))name=string.IsNullOrWhiteSpace(record.LocaleName)?record.Name:record.LocaleName;
            return new DungeonNpcChoice{Vid=v.Property_03C9,Vnum=vnum,Name=name,Distance=v.Distance};
        }).ToList();
        flowNpc.BeginUpdate();flowNpc.Items.Clear();foreach(var choice in choices)flowNpc.Items.Add(choice);flowNpc.EndUpdate();
        var selected=choices.FirstOrDefault(v=>v.Vid==keep);if(selected!=null)flowNpc.SelectedItem=selected;
        if(choices.Count==0)MessageBox.Show("Yakında NPC bulunamadı. NPC'nin yanına gidip tekrar Tara'ya basın.");
    }

    private void ShowDungeonStageSelections(TaskStage stage)
    {
        if(flowNpc!=null)
        {
            var npc=flowNpc.Items.Cast<object>().OfType<DungeonNpcChoice>().FirstOrDefault(v=>v.Vid==stage.NpcVid||(stage.NpcVnum>0&&v.Vnum==stage.NpcVnum));
            if(npc!=null)flowNpc.SelectedItem=npc;else flowNpc.Text=string.IsNullOrWhiteSpace(stage.NpcName)?(stage.NpcVid==0?"":"VID "+stage.NpcVid):stage.NpcName+(stage.NpcVid==0?"":" · VID "+stage.NpcVid);
        }
        if(flowItem!=null)
        {
            var item=flowItem.Items.Cast<object>().OfType<DungeonItemChoice>().FirstOrDefault(v=>v.Id==stage.RequiredItemId);
            if(item!=null)flowItem.SelectedItem=item;else flowItem.Text=stage.RequiredItemId==0?"":(string.IsNullOrWhiteSpace(stage.RequiredItemName)?"Eşya":stage.RequiredItemName)+" · ID "+stage.RequiredItemId;
        }
    }
    private void ReadDungeonStageSelections(TaskStage stage)
    {
        var npc=flowNpc.SelectedItem as DungeonNpcChoice;stage.NpcVid=npc?.Vid??0;stage.NpcVnum=npc?.Vnum??stage.NpcVnum;stage.NpcName=npc?.Name??flowNpc.Text.Trim();
        var item=flowItem.SelectedItem as DungeonItemChoice;
        if(item!=null){stage.RequiredItemId=item.Id;stage.RequiredItemName=item.Name;}
        else
        {
            uint id;if(uint.TryParse(flowItem.Text.Trim(),NumberStyles.Integer,CultureInfo.InvariantCulture,out id)){stage.RequiredItemId=id;stage.RequiredItemName="";}
            else if(string.IsNullOrWhiteSpace(flowItem.Text)){stage.RequiredItemId=0;stage.RequiredItemName="";}
        }
    }
}
