using System;
using System.Drawing;
using System.Windows.Forms;

public partial class MainForm
{
    private GroupBox routeCombatPanel;
    private CheckBox routeHybridCheck,routeWallCheck,routeMountWallCheck,routeStoneSplashCheck;
    private ComboBox routeWaitMode;
    private NumericUpDown routeWaitRange;
    private CheckBox routeStoneBatchCheck,routeRangePickupCheck;
    private NumericUpDown routeStoneBatchRange;
    private bool routeCombatApplying,routeOldWall,routeOldMountWall;
    private bool routeIgnoreTerrain;
    private long routeTraversalAt;
    private void UpdateRouteTraversal(long now)
    {
        if(now<routeTraversalAt)return;routeTraversalAt=now+200;
        bool pass=activeRouteOptions.ObstaclePass;
        if(!pass&&activeRouteOptions.MountedObstaclePass){
            try{pass=PlayerHealthSnapshot.Fresh(System.IO.File.ReadAllText(TargetHealthPath),routeOwnerPid,routeRunningMap,DateTime.UtcNow)?.Mounted==true;}catch(System.IO.IOException){}
        }
        if(pass==routeIgnoreTerrain)return;
        routeIgnoreTerrain=pass;routeNavigator=null;routeNavigationPath=null;routeNavigationGoal=-1;
    }
    private int InitializeRouteCombatEditors(int top)
    {
        routeCombatPanel=new GroupBox{Name="routeCombatPanel",Text="Rotaya özel saldırı, geçiş ve toplama",Left=20,Top=top,Width=Math.Max(510,tabRoute.ClientSize.Width-50),Height=382,MinimumSize=new Size(510,382),Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,ForeColor=tabRoute.ForeColor};
        tabRoute.Controls.Add(routeCombatPanel);
        Action<string,int,int,int> label=(s,x,y,w)=>routeCombatPanel.Controls.Add(new Label{Text=s,Left=x,Top=y,Width=w,Height=24});
        label("Saldırı türü",12,25,100);label("Vuruş aralığı (ms)",218,25,130);label("Vuruş menzili",366,25,110);
        routeWaitMode=new ComboBox{Name="routeWaitMode",Left=12,Top=52,Width=192,DropDownStyle=ComboBoxStyle.DropDownList};
        routeWaitMode.Items.AddRange(new object[]{"Yakın Wait","Uzak Wait","Seçili hedef vuruş","Seçili hedef menzilli vuruş"});routeWaitMode.SelectedIndex=0;
        routeWaitMode.DropDownWidth=260;
        routeAttackMilliseconds=new NumericUpDown{Name="routeAttackMilliseconds",Left=218,Top=52,Width=128,Minimum=20,Maximum=1000,Value=180,Increment=10};
        routeWaitRange=new NumericUpDown{Name="routeWaitRange",Left=366,Top=52,Width=110,Minimum=100,Maximum=100000,Value=250,Increment=50};
        routeHybridCheck=new CheckBox{Name="routeHybridCheck",Text="Karma vuruş · normal oyun vuruşu + seçilen saldırı türü",Left=12,Top=88,Width=485,Height=28};
        routeWallCheck=new CheckBox{Name="routeWallCheck",Text="Engel geçişi",Left=12,Top=124,Width=205,Height=28};
        routeMountWallCheck=new CheckBox{Name="routeMountWallCheck",Text="Binekle engel geçişi",Left=228,Top=124,Width=247,Height=28};
        routeStoneSplashCheck=new CheckBox{Name="routeStoneSplashCheck",Text="Yalnız taşa git; taş keserken çevresindeki canavarlara da Wait vur",Left=12,Top=158,Width=485,Height=44};
        routeStoneBatchCheck=new CheckBox{Name="routeStoneBatchCheck",Text="Yakın taşları bitir; sonra rotaya dön",Left=12,Top=208,Width=485,Height=28,Checked=true};
        label("Taş grubu mesafesi",12,248,195);
        routeStoneBatchRange=new NumericUpDown{Name="routeStoneBatchRange",Left=218,Top=244,Width=128,Minimum=100,Maximum=9500,Value=1000,Increment=100};
        routeRangePickupCheck=new CheckBox{Name="routeRangePickupCheck",Text="Menzilli toplama · hedeften ayrılmadan çevredeki eşyaları topla",Left=12,Top=278,Width=485,Height=28,Checked=true};
        var note=new Label{Left=12,Top=314,Width=485,Height=56,Text="Görünen düşen eşyaların gerçek kimlikleri arka planda taranır.\nKarakter yürümez; taş saldırısı ve rota kesilmeden pickup isteği gönderilir."};
        routeCombatPanel.Controls.AddRange(new Control[]{routeWaitMode,routeAttackMilliseconds,routeWaitRange,routeHybridCheck,routeWallCheck,routeMountWallCheck,routeStoneSplashCheck,routeStoneBatchCheck,routeStoneBatchRange,routeRangePickupCheck,note});
        foreach(var c in new[]{routeHybridCheck,routeWallCheck,routeMountWallCheck,routeStoneSplashCheck,routeStoneBatchCheck,routeRangePickupCheck})c.CheckedChanged+=OnRouteCombatChanged;
        routeWaitMode.SelectedIndexChanged+=OnRouteCombatChanged;routeAttackMilliseconds.ValueChanged+=OnRouteCombatChanged;routeWaitRange.ValueChanged+=OnRouteCombatChanged;
        routeStoneBatchRange.ValueChanged+=OnRouteCombatChanged;
        return routeCombatPanel.Bottom+10;
    }
    private void ReadRouteCombatEditors(RouteProfile p)
    {
        if(routeCombatPanel==null)return;
        p.HybridAttack=routeHybridCheck.Checked;p.RangedWait=routeWaitMode.SelectedIndex==1||routeWaitMode.SelectedIndex==3;p.TargetedAttack=routeWaitMode.SelectedIndex>=2;
        p.AttackMilliseconds=(int)routeAttackMilliseconds.Value;p.AttackRange=(int)routeWaitRange.Value;
        p.ObstaclePass=routeWallCheck.Checked;p.MountedObstaclePass=routeMountWallCheck.Checked;p.StoneMonsterSplash=routeStoneSplashCheck.Checked;
        p.FinishNearbyStones=routeStoneBatchCheck.Checked;p.StoneBatchRange=(int)routeStoneBatchRange.Value;p.RangePickup=routeRangePickupCheck.Checked;
    }
    private void ApplyRouteCombatEditors(RouteProfile p)
    {
        if(routeCombatPanel==null)return;
        routeCombatApplying=true;
        try{
            routeHybridCheck.Checked=p.HybridAttack;routeWaitMode.SelectedIndex=(p.TargetedAttack?2:0)+(p.RangedWait?1:0);
            routeAttackMilliseconds.Value=p.AttackMilliseconds;routeWaitRange.Value=p.AttackRange;
            routeWallCheck.Checked=p.ObstaclePass;routeMountWallCheck.Checked=p.MountedObstaclePass;routeStoneSplashCheck.Checked=p.StoneMonsterSplash;
            routeStoneBatchCheck.Checked=p.FinishNearbyStones;routeStoneBatchRange.Value=p.StoneBatchRange;routeRangePickupCheck.Checked=p.RangePickup;
        }finally{routeCombatApplying=false;}
    }
    private void OnRouteCombatChanged(object sender,EventArgs e)
    {
        if(routeCombatApplying||RouteOwnsCombat)return;
        ReadRouteCombatEditors(routeOptions);farmOnRouteCheckBox.Text="Yakın hedefleri kes · "+routeOptions.AttackMilliseconds+" ms";
        InvalidateRoutePreflight();sessionDirty=true;
    }
}
