using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private async Task BindDashboardClientAsync()
    {
        if(clientDashboard?.CurrentRow?.Tag==null)return;int pid=(int)clientDashboard.CurrentRow.Tag;
        if(LockedElsewhere(pid,clientLockPid)){MessageBox.Show("Bu PID başka bir Onur penceresinde yönetiliyor.");return;}
        chosenPid=pid;boundPid=pid;awaitingReplacementClient=false;connectionRequested=true;
        if(clientConnected&&memory.Field_0197?.Property_0268?.Id!=pid){clientConnected=false;automation?.Stop();StopSessionLease();}
        await ConnectAutomaticallyAsync();RefreshClientDashboard();
    }
    private void RefreshClientDashboard()
    {
        if(clientDashboard==null)return;int selected=clientDashboard.CurrentRow?.Tag is int?(int)clientDashboard.CurrentRow.Tag:0;clientDashboard.Rows.Clear();
        foreach(var p in Process.GetProcessesByName(ClientProcessName).OrderBy(v=>v.Id))
        {
            string name;clientNames.TryGetValue(p.Id,out name);bool own=p.Id==boundPid,taken=LockedElsewhere(p.Id,clientLockPid);
            string state=own?(clientConnected?"Bağlı":"Seçili / bekleniyor"):(taken?"Başka uygulamada":"Boş");
            string map=own?(atlas?.Name??atlasName??""):"";
            string job=own?(taskCancellation!=null?"Zindan":normalBotRequested||Field_02DF?"Hareket + saldırı":dmgStartCheck.Checked?"Saldırı":"Bekliyor"):"";
            int row=clientDashboard.Rows.Add(p.Id,string.IsNullOrWhiteSpace(name)?"Okunuyor…":name,state,map,job);clientDashboard.Rows[row].Tag=p.Id;if(p.Id==selected)clientDashboard.Rows[row].Selected=true;
        }
    }

    private void AddCurrentFarmZone(bool exclusion)
    {
        PointF center;float radius;if(!TryGetFarmAreaMemory(out center,out radius)||atlas==null){MessageBox.Show("Önce büyük haritada sürükleyerek bir alan seçin.");return;}
        var z=new FarmZone{Name=(exclusion?"Yasak ":"Gezi ")+(advancedState.Zones.Count+1),MapName=atlas.Name,MapX=farmAreaCenterMap.X,MapY=farmAreaCenterMap.Y,RadiusMap=farmAreaRadiusMap,MemoryX=center.X,MemoryY=center.Y,RadiusMemory=radius,Exclusion=exclusion,Enabled=true};
        advancedState.Zones.Add(z);BindAdvancedLists();SaveAdvancedState();bigMapCanvas?.Invalidate();radarPanel?.Invalidate();RecordActivity("Alan",z+" eklendi.");
    }
    private void RemoveSelectedZone(){int i=zoneList?.SelectedIndex??-1;if(i<0)return;advancedState.Zones.RemoveAt(i);BindAdvancedLists();SaveAdvancedState();bigMapCanvas?.Invalidate();radarPanel?.Invalidate();}
    private void ToggleSelectedZone(){var z=zoneList?.SelectedItem as FarmZone;if(z==null)return;z.Enabled=!z.Enabled;BindAdvancedLists();SaveAdvancedState();bigMapCanvas?.Invalidate();radarPanel?.Invalidate();}
    private void ActivateSelectedZone(){var z=zoneList?.SelectedItem as FarmZone;if(z==null||atlas==null||!string.Equals(z.MapName,atlas.Name,StringComparison.OrdinalIgnoreCase))return;farmAreaCenterMap=new PointF(z.MapX,z.MapY);farmAreaRadiusMap=z.RadiusMap;farmAreaMapName=z.MapName;farmAreaSelected=true;farmAreaPinnedByMapSelection=false;searchPolicy.Reset();bigMapCanvas?.Invalidate();radarPanel?.Invalidate();}
    private void CaptureFarmPoint(bool start)
    {
        if(!IsClientReady()){MessageBox.Show("Önce oyuna bağlanın.");return;}
        var p=new FarmPoint{MapName=atlas?.Name??atlasName??"",X=TaskX,Y=TaskY};if(start)advancedState.StartPoint=p;else advancedState.ReturnPoint=p;UpdatePointLabel();SaveAdvancedState();
    }
    private void UpdatePointLabel(){if(pointLabel==null)return;pointLabel.Text="Başlangıç: "+(advancedState.StartPoint==null?"yok":advancedState.StartPoint.ToString())+"\nDönüş: "+(advancedState.ReturnPoint==null?"yok":advancedState.ReturnPoint.ToString());}

    private void AddSelectedSkillToOrder(bool boss)
    {
        var skill=skillGrid?.CurrentRow?.Tag as DetectedSkill;if(skill==null){MessageBox.Show("Önce Envanter sekmesinden bir beceri seçin.");return;}
        var list=boss?advancedState.BossSkillOrder:advancedState.NormalSkillOrder;if(!list.Contains(skill.Id))list.Add(skill.Id);
        if(!selectedSkills.ContainsKey(skill.Id))selectedSkills[skill.Id]=(int)(skillSeconds?.Value??2);sessionAuto.Checked=true;sessionDirty=true;BindAdvancedLists();SaveAdvancedState();
    }
    private void RemoveSelectedSkill(){bool boss=bossSkillList?.Focused==true;var box=boss?bossSkillList:normalSkillList;int i=box?.SelectedIndex??-1;if(i<0)return;(boss?advancedState.BossSkillOrder:advancedState.NormalSkillOrder).RemoveAt(i);BindAdvancedLists();sessionDirty=true;SaveAdvancedState();}
    private void MoveSelectedSkill(int delta){bool boss=bossSkillList?.Focused==true;var box=boss?bossSkillList:normalSkillList;var list=boss?advancedState.BossSkillOrder:advancedState.NormalSkillOrder;int i=box?.SelectedIndex??-1,j=i+delta;if(i<0||j<0||j>=list.Count)return;uint v=list[i];list.RemoveAt(i);list.Insert(j,v);BindAdvancedLists();box.SelectedIndex=j;sessionDirty=true;SaveAdvancedState();}
    private IEnumerable<uint> ActiveSkillOrder(){var preferred=string.Equals(advancedState.SkillMode,"Boss",StringComparison.OrdinalIgnoreCase)?advancedState.BossSkillOrder:advancedState.NormalSkillOrder;return preferred.Concat(selectedSkills.Keys.Where(k=>!preferred.Contains(k))).Distinct();}

    private void AddInventoryRule()
    {
        string match=inventoryMatch.Text.Trim();if(match.Length==0)return;var r=advancedState.InventoryRules.FirstOrDefault(v=>string.Equals(v.Match,match,StringComparison.OrdinalIgnoreCase));
        if(r==null){r=new InventoryRule{Match=match};advancedState.InventoryRules.Add(r);}
        r.Action=inventoryRuleAction.Text;r.Threshold=(int)inventoryThreshold.Value;r.IntervalSeconds=(int)inventoryInterval.Value;r.Enabled=true;ApplyInventoryRules();BindAdvancedLists();SaveAdvancedState();
    }
    private void RemoveInventoryRule(){int i=inventoryRuleList?.SelectedIndex??-1;if(i<0)return;advancedState.InventoryRules.RemoveAt(i);BindAdvancedLists();SaveAdvancedState();}
    private void LoadSelectedInventoryRule(){var r=inventoryRuleList?.SelectedItem as InventoryRule;if(r==null)return;inventoryMatch.Text=r.Match;inventoryRuleAction.SelectedItem=r.Action;inventoryThreshold.Value=Math.Max(inventoryThreshold.Minimum,Math.Min(inventoryThreshold.Maximum,r.Threshold));inventoryInterval.Value=Math.Max(inventoryInterval.Minimum,Math.Min(inventoryInterval.Maximum,r.IntervalSeconds));}
    private bool InventoryRuleMatches(InventoryRule r,DetectedItem item){uint id;return uint.TryParse(r.Match,out id)?item.Id==id:(item.Name??"").IndexOf(r.Match,StringComparison.CurrentCultureIgnoreCase)>=0;}
    private void ApplyInventoryRules()
    {
        foreach(var r in advancedState.InventoryRules.Where(v=>v.Enabled))
        {
            var matches=visibleItems.Where(i=>InventoryRuleMatches(r,i)).ToList();long count=matches.Sum(i=>(long)i.Count);
            if(r.Action=="Kullan")foreach(var item in matches)scheduledItems[item.Id]=Math.Max(1,r.IntervalSeconds);
            if(r.Action=="Azalınca uyar"&&count<r.Threshold)RecordActivity("Envanter",r.Match+" azaldı · "+count+" / "+r.Threshold);
            if(r.Action=="Çöpe ayır"&&matches.Count>0)RecordActivity("Envanter",r.Match+" çöpe ayrılacak olarak işaretli · otomatik silinmedi.");
        }
        sessionDirty=true;MarkScheduledRows();
    }

    private void RefreshFlowList(){if(flowList==null)return;flowList.BeginUpdate();flowList.Items.Clear();foreach(var st in taskSequence.Stages){string range=st.AttackMode=="Menzilli"?" · "+EffectiveDungeonRangedAttackRange(st)+" menzil":"";flowList.Items.Add(st.Number.ToString("00")+" · "+st.Name+" · "+st.AttackMode+range+" · "+EffectiveDungeonAttackDelay(st)+" ms");}flowList.EndUpdate();if(flowList.Items.Count>0&&flowList.SelectedIndex<0)flowList.SelectedIndex=0;}
    private void UpdateFlowCombatEditors(){if(flowAttackRange!=null)flowAttackRange.Enabled=flowAttack!=null&&flowAttack.Text=="Menzilli";}
    private void LoadSelectedFlowStage()
    {
        int i=flowList?.SelectedIndex??-1;if(i<0||i>=taskSequence.Stages.Count)return;var s=taskSequence.Stages[i];
        flowName.Text=s.Name;flowEnabled.Checked=s.Enabled;flowAttack.SelectedItem=s.AttackMode;flowRetry.Value=Math.Max(flowRetry.Minimum,Math.Min(flowRetry.Maximum,s.RetryCount));flowAttackRange.Value=Math.Max(flowAttackRange.Minimum,Math.Min(flowAttackRange.Maximum,EffectiveDungeonRangedAttackRange(s)));flowAttackWait.Value=Math.Max(flowAttackWait.Minimum,Math.Min(flowAttackWait.Maximum,EffectiveDungeonAttackDelay(s)));flowTransition.SelectedIndex=(int)s.Transition;flowWait.Value=Math.Max(flowWait.Minimum,Math.Min(flowWait.Maximum,s.WaitMilliseconds));ShowDungeonStageSelections(s);flowTeleport.Checked=s.WaitForTeleport;flowObstacle.Checked=s.Number==8&&s.EnableObstaclePass;flowObstacle.Enabled=s.Number==8;UpdateFlowCombatEditors();
    }
    private void UpdateFlowStage()
    {
        int i=flowList?.SelectedIndex??-1;if(i<0)return;var s=taskSequence.Stages[i];s.Name=flowName.Text.Trim();s.Enabled=flowEnabled.Checked;s.AttackMode=flowAttack.Text;s.RangedAttackRange=(int)flowAttackRange.Value;s.WaitAttackMilliseconds=(int)flowAttackWait.Value;s.RetryCount=(int)flowRetry.Value;s.WaitMilliseconds=(int)flowWait.Value;ReadDungeonStageSelections(s);s.WaitForTeleport=flowTeleport.Checked;s.EnableObstaclePass=s.Number==8&&flowObstacle.Checked;if(flowTransition.SelectedIndex>=0)s.Transition=(StageTransition)flowTransition.SelectedIndex;AutoSaveTaskSequence();RefreshTaskStageList(i);RefreshFlowList();flowList.SelectedIndex=i;RecordActivity("Zindan",s.Number+". aşama ayarları güncellendi · "+EffectiveDungeonAttackDelay(s)+" ms"+(s.AttackMode=="Menzilli"?" · "+EffectiveDungeonRangedAttackRange(s)+" menzil":""));
    }
    private void MoveFlowStage(int delta){int i=flowList?.SelectedIndex??-1,j=i+delta;if(i<0||j<0||j>=taskSequence.Stages.Count)return;var s=taskSequence.Stages[i];taskSequence.Stages.RemoveAt(i);taskSequence.Stages.Insert(j,s);for(int k=0;k<taskSequence.Stages.Count;k++)taskSequence.Stages[k].Number=k+1;AutoSaveTaskSequence();RefreshTaskStageList(j);RefreshFlowList();flowList.SelectedIndex=j;}

    private IEnumerable<string> SplitTargetTokens(string text){return (text??"").Split(new[]{',',';','\n','\r'},StringSplitOptions.RemoveEmptyEntries).Select(v=>v.Trim().ToLowerInvariant()).Where(v=>v.Length>0);}
    private bool TargetMatchesToken(EntityState e,string token)
    {
        if(token=="taş"||token=="tas"||token=="metin")return e.Property_03CA==AddressCatalog.Field_035B;
        if(token=="canavar"||token=="monster")return e.Property_03CA==AddressCatalog.Field_035C;
        if(token=="npc")return e.Property_03CA==1;
        int vid;if(token.StartsWith("vid:"))token=token.Substring(4);if(token.StartsWith("boss:"))token=token.Substring(5);
        if(int.TryParse(token,out vid))return e.Property_03C9==vid;
        return !string.IsNullOrEmpty(e.Property_03CD)&&e.Property_03CD.IndexOf(token,StringComparison.OrdinalIgnoreCase)>=0;
    }
    private bool IsBossTarget(EntityState e){string n=(e.Property_03CD??"").ToLowerInvariant();return n.Contains("boss")||n.Contains("patron")||SplitTargetTokens(advancedState.TargetWhitelist).Any(t=>t.StartsWith("boss:")&&TargetMatchesToken(e,t));}
    private List<EntityState> OrderFarmTargets(IEnumerable<EntityState> source)
    {
        var white=SplitTargetTokens(advancedState.TargetWhitelist).ToList();var black=SplitTargetTokens(advancedState.TargetBlacklist).ToList();
        var filtered=source.Where(e=>!black.Any(t=>TargetMatchesToken(e,t))).Where(e=>white.Count==0||white.Any(t=>TargetMatchesToken(e,t)));
        string mode=advancedState.TargetPriority??"";Func<EntityState,int> rank=e=>0;
        if(mode.StartsWith("Taş"))rank=e=>e.Property_03CA==AddressCatalog.Field_035B?0:1;
        else if(mode.StartsWith("Canavar"))rank=e=>e.Property_03CA==AddressCatalog.Field_035C?0:1;
        else if(mode.StartsWith("Boss"))rank=e=>IsBossTarget(e)?0:e.Property_03CA==AddressCatalog.Field_035B?1:2;
        return filtered.OrderBy(rank).ThenBy(e=>e.Distance).ToList();
    }
    private bool IsTargetInsideFarmConfiguration(EntityState e)
    {
        string map=atlas?.Name??atlasName??"";var zones=advancedState.Zones.Where(z=>z.Enabled&&string.Equals(z.MapName,map,StringComparison.OrdinalIgnoreCase)).ToList();
        if(zones.Any(z=>z.Exclusion&&DistanceSquared(e.X,e.Y,z.MemoryX,z.MemoryY)<=z.RadiusMemory*z.RadiusMemory))return false;
        PointF center;float radius;
        if(TryGetFarmAreaMemory(out center,out radius))return DistanceSquared(e.X,e.Y,center.X,center.Y)<=radius*radius;
        var includes=zones.Where(z=>!z.Exclusion).ToList();if(includes.Count>0)return includes.Any(z=>DistanceSquared(e.X,e.Y,z.MemoryX,z.MemoryY)<=z.RadiusMemory*z.RadiusMemory);
        return true;
    }
    private void TrySelectNextPatrolZone()
    {
        if(farmAreaPinnedByMapSelection||DateTime.UtcNow<nextZoneChangeUtc||advancedState.PatrolMode=="Tek alan")return;string map=atlas?.Name??atlasName??"";
        var zones=advancedState.Zones.Where(z=>z.Enabled&&!z.Exclusion&&string.Equals(z.MapName,map,StringComparison.OrdinalIgnoreCase)).ToList();if(zones.Count<2)return;
        if(advancedState.PatrolMode=="Rastgele")patrolZoneIndex=Field_02E5.Next(zones.Count);else patrolZoneIndex=(patrolZoneIndex+1)%zones.Count;
        var z=zones[patrolZoneIndex];farmAreaCenterMap=new PointF(z.MapX,z.MapY);farmAreaRadiusMap=z.RadiusMap;farmAreaMapName=z.MapName;farmAreaSelected=true;farmAreaPinnedByMapSelection=false;searchPolicy.Reset();nextZoneChangeUtc=DateTime.UtcNow.AddSeconds(20);RecordActivity("Alan","Dolaşma alanı: "+z.Name);
    }

    private bool TryGetAdvancedStartArea(out PointF center,out float radius)
    {
        center=PointF.Empty;radius=0;var p=advancedState.StartPoint;if(p==null||!string.Equals(p.MapName,atlas?.Name??atlasName??"",StringComparison.OrdinalIgnoreCase))return false;
        center=new PointF(p.X,p.Y);radius=(float)searchRange.Value;return true;
    }
    private bool TryMoveToReturnPoint()
    {
        var p=advancedState.ReturnPoint;if(p==null||(DateTime.UtcNow-lastHostileSeenUtc).TotalSeconds<15||!string.Equals(p.MapName,atlas?.Name??atlasName??"",StringComparison.OrdinalIgnoreCase))return false;
        float x=TaskX,y=TaskY;if(DistanceSquared(x,y,p.X,p.Y)<180*180)return false;
        Method_02C2(x,y,p.X,p.Y);farmStatus.Text="Dönüş noktasına gidiliyor.";return true;
    }
    private void ObserveInventorySnapshot(PlayerInventorySnapshot snapshot)
    {
        var totals=snapshot.Items.GroupBy(i=>i.Id).ToDictionary(g=>g.Key,g=>g.Sum(v=>(long)v.Count));
        if(observedItemCounts.Count>0)
        {
            foreach(var p in totals){long old;observedItemCounts.TryGetValue(p.Key,out old);if(p.Value>old)advancedState.Stats.ItemsGained+=p.Value-old;else if(p.Value<old)advancedState.Stats.ItemsUsed+=old-p.Value;}
            foreach(var p in observedItemCounts.Where(p=>!totals.ContainsKey(p.Key)))advancedState.Stats.ItemsUsed+=p.Value;
        }
        observedItemCounts.Clear();foreach(var p in totals)observedItemCounts[p.Key]=p.Value;inventoryOccupied=snapshot.Items.Count;PopulateDungeonItemChoices();ApplyInventoryRules();UpdateStatsUi();
    }
}
