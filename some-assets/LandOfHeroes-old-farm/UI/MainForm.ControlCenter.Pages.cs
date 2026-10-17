using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private void BuildSkillInventoryPage()
    {
        var page=NewControlPage("Beceri / Envanter");
        page.Controls.Add(PlainLabel("Beceri dönüşü · Envanter sekmesinde bir beceri seçip buraya ekleyin.",16,14,900));
        page.Controls.Add(PlainLabel("Normal sıra",16,48));page.Controls.Add(PlainLabel("Boss sıra",300,48));
        normalSkillList=new ListBox{Left=16,Top=74,Width=260,Height=240};bossSkillList=new ListBox{Left=300,Top=74,Width=260,Height=240};
        page.Controls.AddRange(new Control[]{normalSkillList,bossSkillList});
        var addNormal=PlainButton("Seçileni normale ekle",16,326,190);var addBoss=PlainButton("Seçileni bossa ekle",300,326,190);
        var up=PlainButton("Yukarı",16,370,90);var down=PlainButton("Aşağı",116,370,90);var remove=PlainButton("Sıradan çıkar",216,370,120);
        addNormal.Click+=(s,e)=>AddSelectedSkillToOrder(false);addBoss.Click+=(s,e)=>AddSelectedSkillToOrder(true);up.Click+=(s,e)=>MoveSelectedSkill(-1);down.Click+=(s,e)=>MoveSelectedSkill(1);remove.Click+=(s,e)=>RemoveSelectedSkill();
        page.Controls.AddRange(new Control[]{addNormal,addBoss,up,down,remove});
        page.Controls.Add(PlainLabel("Etkin dönüş",350,372,120));
        skillModeCombo=new ComboBox{Left=450,Top=368,Width=110,DropDownStyle=ComboBoxStyle.DropDownList};skillModeCombo.Items.AddRange(new object[]{"Normal","Boss"});skillModeCombo.SelectedIndexChanged+=(s,e)=>{SyncAdvancedStateFromUi();sessionDirty=true;SaveAdvancedState();};page.Controls.Add(skillModeCombo);

        int x=590;page.Controls.Add(PlainLabel("Akıllı envanter kuralları",x,14,350));
        inventoryRuleList=new ListBox{Left=x,Top=44,Width=460,Height=270};page.Controls.Add(inventoryRuleList);
        page.Controls.Add(PlainLabel("Ad veya eşya ID",x,326,180));inventoryMatch=new TextBox{Left=x,Top=352,Width=220};page.Controls.Add(inventoryMatch);
        page.Controls.Add(PlainLabel("İşlem",x+240,326,100));inventoryRuleAction=new ComboBox{Left=x+240,Top=352,Width=160,DropDownStyle=ComboBoxStyle.DropDownList};inventoryRuleAction.Items.AddRange(new object[]{"Koru","Kullan","Azalınca uyar","Çöpe ayır","Yoksay"});inventoryRuleAction.SelectedIndex=0;page.Controls.Add(inventoryRuleAction);
        page.Controls.Add(PlainLabel("Adet sınırı",x,398,120));inventoryThreshold=new NumericUpDown{Left=x,Top=424,Width=100,Minimum=0,Maximum=999999,Value=1};page.Controls.Add(inventoryThreshold);
        page.Controls.Add(PlainLabel("Kullanım saniyesi",x+130,398,150));inventoryInterval=new NumericUpDown{Left=x+130,Top=424,Width=100,Minimum=1,Maximum=86400,Value=5};page.Controls.Add(inventoryInterval);
        var addRule=PlainButton("Kural ekle / güncelle",x,474,190);var removeRule=PlainButton("Kuralı sil",x+200,474,120);
        addRule.Click+=(s,e)=>AddInventoryRule();removeRule.Click+=(s,e)=>RemoveInventoryRule();inventoryRuleList.SelectedIndexChanged+=(s,e)=>LoadSelectedInventoryRule();
        page.Controls.AddRange(new Control[]{addRule,removeRule});
        var note=PlainLabel("Çöpe ayır kuralı eşyayı işaretler ve günlüğe yazar; oyundan otomatik silme yapmaz.",x,528,440);note.Height=48;page.Controls.Add(note);
    }

    private void BuildDungeonFlowPage()
    {
        var page=NewControlPage("Zindan Akışı");
        BuildDungeonProfileStrip(page);
        flowList=new ListBox{Left=16,Top=78,Width=430,Height=502};page.Controls.Add(flowList);int x=480;
        page.Controls.Add(PlainLabel("Aşama adı",x,20));flowName=new TextBox{Left=x,Top=46,Width=440};page.Controls.Add(flowName);
        flowEnabled=new CheckBox{Left=x,Top=88,Width=150,Text="Aşama etkin",Checked=true};flowObstacle=new CheckBox{Left=x+170,Top=88,Width=220,Text="Engel ve binek geçişi"};page.Controls.AddRange(new Control[]{flowEnabled,flowObstacle});
        page.Controls.Add(PlainLabel("Saldırı biçimi",x,128));flowAttack=new ComboBox{Left=x,Top=154,Width=180,DropDownStyle=ComboBoxStyle.DropDownList};flowAttack.Items.AddRange(new object[]{"Otomatik","Yakın","Menzilli","Kapalı"});flowAttack.SelectedIndexChanged+=(s,e)=>UpdateFlowCombatEditors();page.Controls.Add(flowAttack);
        page.Controls.Add(PlainLabel("Tekrar deneme",x+210,128));flowRetry=new NumericUpDown{Left=x+210,Top=154,Width=100,Minimum=0,Maximum=20,Value=3};page.Controls.Add(flowRetry);
        page.Controls.Add(PlainLabel("Menzilli vuruş mesafesi",x,200));flowAttackRange=new NumericUpDown{Name="flowAttackRange",Left=x,Top=226,Width=180,Minimum=100,Maximum=9500,Increment=50,Value=2500,ThousandsSeparator=true};page.Controls.Add(flowAttackRange);
        page.Controls.Add(PlainLabel("Saldırı bekleme (ms)",x+210,200));flowAttackWait=new NumericUpDown{Name="flowAttackWait",Left=x+210,Top=226,Width=150,Minimum=1,Maximum=1000,Increment=5,Value=275};page.Controls.Add(flowAttackWait);
        page.Controls.Add(PlainLabel("Geçiş koşulu",x,272));flowTransition=new ComboBox{Left=x,Top=298,Width=220,DropDownStyle=ComboBoxStyle.DropDownList};flowTransition.Items.AddRange(new object[]{"Hemen geç","Süre kadar bekle","Konum değişip sabitlenince","Elle ilerlet"});page.Controls.Add(flowTransition);
        var transitionHelp=new ToolTip(components);transitionHelp.SetToolTip(flowTransition,"PositionChangedAndStable: konum değiştikten sonra sabitlenmesini bekleme koşuludur. Mevcut akıllı mod katları haritadan tanır. Yeni senaryoda Komutlu zindan editörünü kullan.");
        page.Controls.Add(PlainLabel("Geçiş bekleme (ms)",x+240,272));flowWait=new NumericUpDown{Left=x+240,Top=298,Width=120,Minimum=0,Maximum=600000,Increment=100,Value=1500};page.Controls.Add(flowWait);
        page.Controls.Add(PlainLabel("Oyundaki NPC · ad ve VID",x,346,210));flowNpc=new ComboBox{Left=x,Top=372,Width=160,DropDownStyle=ComboBoxStyle.DropDown};var scanNpc=PlainButton("Tara",x+166,370,54);scanNpc.Click+=async(s,e)=>await RefreshDungeonNpcChoicesAsync();page.Controls.AddRange(new Control[]{flowNpc,scanNpc});
        page.Controls.Add(PlainLabel("Envanter eşyası · ad ve ID",x+230,346,210));flowItem=new ComboBox{Left=x+230,Top=372,Width=150,DropDownStyle=ComboBoxStyle.DropDown};var scanItem=PlainButton("Oku",x+386,370,54);scanItem.Click+=async(s,e)=>await RefreshDungeonItemChoicesAsync();page.Controls.AddRange(new Control[]{flowItem,scanItem});
        var pickerInfo=PlainLabel("ID yazmanız gerekmez: NPC ve eşya oyundan okunur; sabit eşya VNUM bilgisi aşamaya kaydedilir.",x,414,440);pickerInfo.Height=42;page.Controls.Add(pickerInfo);
        flowTeleport=new CheckBox{Left=x,Top=460,Width=260,Text="Işınlanma tamamlanmasını bekle"};page.Controls.Add(flowTeleport);
        var update=PlainButton("Aşamayı güncelle",x,506,165);var up=PlainButton("Yukarı",x+175,506,90);var down=PlainButton("Aşağı",x+275,506,90);
        update.Click+=(s,e)=>UpdateFlowStage();up.Click+=(s,e)=>MoveFlowStage(-1);down.Click+=(s,e)=>MoveFlowStage(1);flowList.SelectedIndexChanged+=(s,e)=>LoadSelectedFlowStage();
        var retry=PlainButton("Seçili aşamadan tekrar dene",x,552,210);retry.Click+=async(s,e)=>{if(taskCancellation!=null)StopTaskSequence("Yeniden başlatılıyor.");await Task.Delay(250);await RunTaskSequenceAsync(false);};
        flowNpcMarkerButton=PlainButton("Lobi NPC'sini haritada işaretle",x+220,552,220);flowNpcMarkerButton.Name="flowNpcMarkerButton";flowNpcMarkerButton.Click+=(s,e)=>BeginDungeonNpcMarkerSelection();
        flowNpcMarkerStatus=PlainLabel("",x,596,440);flowNpcMarkerStatus.Name="flowNpcMarkerStatus";flowNpcMarkerStatus.Height=38;
        page.Controls.AddRange(new Control[]{update,up,down,retry,flowNpcMarkerButton,flowNpcMarkerStatus});
        var note=PlainLabel("NPC görünmezse karakter kayıtlı lobi noktasına güvenli rotayla gider; orada kamerayı sağ-sol ve yukarı-aşağı tarar.",x,638,440);note.Height=48;page.Controls.Add(note);
        RefreshDungeonNpcMarkerStatus();
        var dialogue=PlainButton("NPC cevap sırasını düzenle",16,592,430);
        dialogue.Name="flowDialogueButton";dialogue.Click+=(s,e)=>OpenNpcDialogueEditor();page.Controls.Add(dialogue);
        var commands=PlainButton("Komutlu zindan editörü · yeni senaryo",16,640,430);
        commands.Name="dungeonCommandEditorButton";commands.Click+=(s,e)=>OpenDungeonCommandEditor();page.Controls.Add(commands);
    }

    private void BuildActivityPage()
    {
        var page=NewControlPage("Günlük / İstatistik");
        activityList=new ListBox{Left=16,Top=18,Width=700,Height=570,HorizontalScrollbar=true};page.Controls.Add(activityList);
        statsLabel=PlainLabel("",742,18,330);statsLabel.Height=340;statsLabel.Font=new Font("Segoe UI Semibold",11f);page.Controls.Add(statsLabel);
        healthLabel=PlainLabel("Can: okunmadı",742,370,300);page.Controls.Add(healthLabel);
        var export=PlainButton("Günlüğü dışa aktar",742,420,180);var reset=PlainButton("İstatistiği sıfırla",742,466,180);var overlay=PlainButton("Oyun üstü bilgi",742,512,180);
        export.Click+=(s,e)=>ExportActivityLog();reset.Click+=(s,e)=>ResetAdvancedStats();overlay.Click+=(s,e)=>ToggleOverlay();
        page.Controls.AddRange(new Control[]{export,reset,overlay});
    }

    private void ApplyAdvancedStateToUi()
    {
        advancedState.Normalize();
        if(priorityCombo!=null){priorityCombo.SelectedItem=advancedState.TargetPriority;if(priorityCombo.SelectedIndex<0)priorityCombo.SelectedIndex=0;}
        if(patrolCombo!=null){patrolCombo.SelectedItem=advancedState.PatrolMode;if(patrolCombo.SelectedIndex<0)patrolCombo.SelectedIndex=0;}
        if(skillModeCombo!=null){skillModeCombo.SelectedItem=advancedState.SkillMode;if(skillModeCombo.SelectedIndex<0)skillModeCombo.SelectedIndex=0;}
        if(whitelistBox!=null)whitelistBox.Text=advancedState.TargetWhitelist??"";
        if(blacklistBox!=null)blacklistBox.Text=advancedState.TargetBlacklist??"";
        BindAdvancedLists();UpdatePointLabel();RefreshFlowList();
    }
    private void SyncAdvancedStateFromUi()
    {
        if(priorityCombo!=null)advancedState.TargetPriority=priorityCombo.Text;
        if(patrolCombo!=null)advancedState.PatrolMode=patrolCombo.Text;
        if(skillModeCombo!=null)advancedState.SkillMode=skillModeCombo.Text;
        if(whitelistBox!=null)advancedState.TargetWhitelist=whitelistBox.Text;
        if(blacklistBox!=null)advancedState.TargetBlacklist=blacklistBox.Text;
    }
    private void SaveAdvancedState(){try{SyncAdvancedStateFromUi();AdvancedStore.Save(AdvancedStatePath,advancedState);}catch(Exception ex){RecordActivity("Ayar","Kaydetme hatası: "+ex.Message);}}
    private void BindAdvancedLists()
    {
        BindList(ruleList,advancedState.Rules);BindList(zoneList,advancedState.Zones);BindList(inventoryRuleList,advancedState.InventoryRules);
        BindSkillList(normalSkillList,advancedState.NormalSkillOrder);BindSkillList(bossSkillList,advancedState.BossSkillOrder);
    }
    private static void BindList<T>(ListBox box,System.Collections.Generic.IEnumerable<T> values){if(box==null)return;box.BeginUpdate();box.Items.Clear();foreach(var v in values)box.Items.Add(v);box.EndUpdate();}
    private void BindSkillList(ListBox box,System.Collections.Generic.IEnumerable<uint> ids)
    {
        if(box==null)return;box.BeginUpdate();box.Items.Clear();
        foreach(uint id in ids){var skill=visibleSkills.FirstOrDefault(s=>s.Id==id);box.Items.Add(new SkillOrderItem{id=id,name=skill?.Name??("Beceri "+id)});}
        box.EndUpdate();
    }
    private sealed class SkillOrderItem{public uint id;public string name;public override string ToString(){return name+" · "+id;}}

    private void AddRule(){var r=new AutomationRule();ReadRuleEditor(r);advancedState.Rules.Add(r);BindAdvancedLists();ruleList.SelectedIndex=advancedState.Rules.Count-1;SaveAdvancedState();}
    private void UpdateRule(){int i=ruleList?.SelectedIndex??-1;if(i<0)return;ReadRuleEditor(advancedState.Rules[i]);BindAdvancedLists();ruleList.SelectedIndex=i;SaveAdvancedState();}
    private void RemoveRule(){int i=ruleList?.SelectedIndex??-1;if(i<0)return;advancedState.Rules.RemoveAt(i);BindAdvancedLists();SaveAdvancedState();}
    private void ReadRuleEditor(AutomationRule r){r.Enabled=ruleEnabled.Checked;r.Condition=ruleCondition.Text;r.Value=ruleValue.Text.Trim();r.Action=ruleAction.Text;r.Argument=ruleArgument.Text.Trim();r.CooldownSeconds=(int)ruleCooldown.Value;}
    private void LoadSelectedRule(){var r=ruleList?.SelectedItem as AutomationRule;if(r==null)return;ruleEnabled.Checked=r.Enabled;ruleCondition.SelectedItem=r.Condition;ruleValue.Text=r.Value;ruleAction.SelectedItem=r.Action;ruleArgument.Text=r.Argument;ruleCooldown.Value=Math.Max(ruleCooldown.Minimum,Math.Min(ruleCooldown.Maximum,r.CooldownSeconds));}
}
