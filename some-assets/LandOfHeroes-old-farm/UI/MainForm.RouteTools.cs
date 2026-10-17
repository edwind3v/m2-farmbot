using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm
{
    private RouteProfile routeOptions=new RouteProfile();
    private RouteProfile activeRouteOptions;
    private Form routeToolsForm,routeHistoryForm;
    private ListBox routeHistoryList;
    private Button routeToolsButton;
    private string routeEditMode="",routeEditMap="";
    private bool routeMapClickConsumed;
    private int routeDragIndex=-1;
    private PointF? routeZoneStart,routeZoneEnd;
    private List<RoutePoint> routeUndoPoints;
    private List<RouteZone> routeUndoZones;
    private RoutePoint routeUndoSafe;
    private Label dungeonLiveStep;
    private long dungeonStepAt;
    private string dungeonStepDetail="Hazır",lastRouteLog="",lastDungeonLog="";
    private string RouteProfilesFolder { get { return Path.Combine(Environment.GetEnvironmentVariable("ONUR_AUTOMATION_SETTINGS_DIR")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Onur","otomasyonlar"),"rota-profilleri"); } }
    private void InitializeRouteTools()
    {
        routeToolsButton=new Button{Name="routeToolsButton",Text="Rota araçları ve profiller",Left=20,Top=routeRadarStatus.Bottom+8,Width=230,Height=34};
        routeToolsButton.Click+=(s,e)=>ShowRouteTools();
        var history=new Button{Name="routeHistoryButton",Text="İşlem geçmişi",Left=260,Top=routeToolsButton.Top,Width=145,Height=34};history.Click+=(s,e)=>ShowRouteHistory();
        tabRoute.Controls.AddRange(new Control[]{routeToolsButton,history});tabRoute.AutoScrollMinSize=new Size(0,history.Bottom+16);
        routePatrolStatus.TextChanged+=(s,e)=>{if(routePatrolStatus.Text==lastRouteLog)return;lastRouteLog=routePatrolStatus.Text;RecordActivity("Rota",lastRouteLog.Replace('\n',' '));};
        foreach(Control c in taskPage.Controls)c.Top+=68;
        dungeonLiveStep=new Label{Name="dungeonLiveStep",Left=12,Top=6,Width=Math.Max(300,taskPage.ClientSize.Width-32),Height=60,Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,ForeColor=taskPage.ForeColor,BackColor=taskPage.BackColor,BorderStyle=BorderStyle.FixedSingle,Text="Zindan canlı adım · Hazır"};
        taskPage.Controls.Add(dungeonLiveStep);taskPage.AutoScrollMinSize=new Size(0,taskPage.Controls.Cast<Control>().Max(c=>c.Bottom)+16);
        taskStatus.TextChanged+=(s,e)=>UpdateDungeonLiveStep();
        var timer=new System.Windows.Forms.Timer(components){Interval=1000};timer.Tick+=(s,e)=>RenderDungeonLiveStep();timer.Start();
    }
    private void UpdateDungeonLiveStep()
    {
        dungeonStepDetail=taskStatus.Text;dungeonStepAt=movementClock.ElapsedMilliseconds;RenderDungeonLiveStep();
        // Changing distances update the live card, but do not flood the persisted history.
        string key=taskUiStage+"|"+new string(dungeonStepDetail.Where(c=>!char.IsDigit(c)).ToArray());
        if(key!=lastDungeonLog){lastDungeonLog=key;RecordActivity("Zindan adımı",dungeonStepDetail);}
    }
    private void RenderDungeonLiveStep()
    {
        if(dungeonLiveStep==null)return;
        string stage=taskUiStage>0?"Aşama "+taskUiStage+" / "+taskSequence.Stages.Count:"Bekliyor / durdu";
        dungeonLiveStep.Text="Zindan · "+stage+(taskDungeonRunning?" · bu durum "+Math.Max(0,(movementClock.ElapsedMilliseconds-dungeonStepAt)/1000)+" sn":"")+"\n"+dungeonStepDetail;
    }
    private RouteProfile CaptureRouteProfile()
    {
        var p=routeOptions.Copy();p.Map=routeMapName;p.Points=routePoints.Select(v=>new RoutePoint{X=v.X,Y=v.Y}).ToList();
        p.Farm=farmAlongRoute;p.Stones=RotaMetinCheck.Checked;p.Monsters=RotaCanavarCheck.Checked;p.Range=(int)routeFarmRange;p.StopDistance=(int)routeStopDistance.Value;ReadRouteCombatEditors(p);return p;
    }
    private void ApplyRouteProfile(RouteProfile p)
    {
        if(RouteOwnsCombat||isRecordingRoute)throw new InvalidOperationException("Önce rota kaydını/takibini durdur.");
        p.Validate();if(p.Range<farmRangeTrackBar.Minimum||p.Range>farmRangeTrackBar.Maximum)throw new InvalidDataException("Profilin takip mesafesi bu sürümün aralığı dışında.");
        EndRouteEdit();routeOptions=p.Copy();routePoints=p.Points.Select(v=>new RoutePoint{X=v.X,Y=v.Y}).ToList();routeMapName=p.Map;
        InvalidateRoutePreflight();ApplyRouteCombatEditors(p);
        RotaMetinCheck.Checked=false;RotaCanavarCheck.Checked=false;RotaMetinCheck.Checked=p.Stones;RotaCanavarCheck.Checked=p.Monsters;
        farmOnRouteCheckBox.Checked=p.Farm;farmOnRouteCheckBox.Text="Yakın hedefleri kes · "+p.AttackMilliseconds+" ms";farmRangeTrackBar.Value=p.Range;OnRouteRangeChanged(null,null);routeStopDistance.Value=p.StopDistance;
        routeUndoPoints=null;RefreshRouteDrawing();routePatrolStatus.Text="Profil açıldı: "+p.Name+" · "+p.Map+" · "+p.Points.Count+" nokta";
    }
    private void ResetRouteExtras()
    {
        EndRouteEdit();routeOptions=new RouteProfile();routeUndoPoints=null;farmOnRouteCheckBox.Text="Yakın hedefleri kes · 180 ms";InvalidateRoutePreflight();ApplyRouteCombatEditors(routeOptions);
        if(routeToolsForm!=null&&!routeToolsForm.IsDisposed)routeToolsForm.Close();
    }
    private void ShowRouteTools()
    {
        if(RouteOwnsCombat||isRecordingRoute){MessageBox.Show("Düzenleme için rota kaydını/takibini durdur. Geçmişi çalışırken açabilirsin.");return;}
        if(routeToolsForm!=null&&!routeToolsForm.IsDisposed){routeToolsForm.Activate();return;}
        var f=new Form{Name="routeToolsForm",Text="GamePilot · Rota araçları",ClientSize=new Size(760,670),MinimumSize=new Size(660,500),StartPosition=FormStartPosition.CenterParent,AutoScroll=true,Font=new Font("Segoe UI",9f),BackColor=tabRoute.BackColor,ForeColor=tabRoute.ForeColor};routeToolsForm=f;
        var panel=new Panel{Dock=DockStyle.Fill,AutoScroll=true};f.Controls.Add(panel);
        Action<string,int> label=(text,y)=>panel.Controls.Add(new Label{Text=text,Left=18,Top=y,Width=700,Height=24});
        Func<string,int,int,int,Button> button=(text,x,y,w)=>{var b=new Button{Text=text,Left=x,Top=y,Width=w,Height=32};panel.Controls.Add(b);return b;};
        label("Profil: noktalar, harita, hedefler, mesafeler, saldırı aralığı ve güvenlik birlikte kaydedilir.",14);
        var saved=new ComboBox{Left=18,Top=42,Width=310,DropDownStyle=ComboBoxStyle.DropDownList};
        var name=new TextBox{Left=340,Top=42,Width=215,MaxLength=100,Text=routeOptions.Name};panel.Controls.AddRange(new Control[]{saved,name});
        Action refresh=()=>{saved.Items.Clear();if(Directory.Exists(RouteProfilesFolder))foreach(string path in Directory.GetFiles(RouteProfilesFolder,"*.json")){try{var p=RouteProfileStore.Load(path);saved.Items.Add(new RouteProfileChoice{Path=path,Label=p.Name+" · "+p.Map});}catch(Exception ex){RecordActivity("Rota profil",Path.GetFileName(path)+": "+ex.Message);}}};refresh();
        label("Harita düzenleme: sol tık/sürükle. Sağ tık veya Esc yalnızca düzenlemeyi bitirir.",118);
        button("Nokta taşı",18,146,125).Click+=(s,e)=>BeginRouteEdit("taşı");
        button("Nokta ekle",153,146,125).Click+=(s,e)=>BeginRouteEdit("ekle");
        button("Nokta sil",288,146,125).Click+=(s,e)=>BeginRouteEdit("sil");
        button("Son düzenlemeyi geri al",423,146,210).Click+=(s,e)=>UndoRouteEdit();
        button("Yasak alan çiz",18,186,150).Click+=(s,e)=>BeginRouteEdit("yasak");
        button("Yasak alan sil",178,186,150).Click+=(s,e)=>BeginRouteEdit("alan sil");
        button("Güvenli nokta seç",338,186,165).Click+=(s,e)=>BeginRouteEdit("güvenli");
        button("Düzenlemeyi bitir",513,186,165).Click+=(s,e)=>EndRouteEdit();
        label("Basit rota: çizgi yakınındaki en yakın taş önceliklidir; canlı hedef bitmeden bırakılmaz",236);
        var priority=new ComboBox{Left=18,Top=264,Width=260,DropDownStyle=ComboBoxStyle.DropDownList};priority.Items.AddRange(new object[]{"En yakın","Patron > taş > canavar","Taş > canavar","Canavar > taş"});priority.SelectedIndex=routeOptions.Priority;
        var mixed=new CheckBox{Left=294,Top=266,Width=220,Text="Taş ve canavarı birlikte seç",Checked=routeOptions.Mixed};panel.Controls.AddRange(new Control[]{priority,mixed});
        priority.Enabled=false;priority.SelectedIndex=2;
        label("Eski profil ad alanları korunur; basit rota canlı radarı kullanır, ad/pack beklemez.",300);
        var names=new TextBox{Left=18,Top=328,Width=320,Text=routeOptions.Names,MaxLength=4000};var bosses=new TextBox{Left=352,Top=328,Width=325,Text=routeOptions.BossNames,MaxLength=4000};panel.Controls.AddRange(new Control[]{names,bosses});
        names.Enabled=bosses.Enabled=false;
        label("Rota saldırı aralığı (ms)                  Çekilme eşiği %                  Devam eşiği %",366);
        var interval=new Button{Left=18,Top=392,Width=230,Height=30,Text="Rota saldırı ayarları"};interval.Click+=(s,e)=>{f.Close();mainTabs.SelectedTab=tabRoute;tabRoute.ScrollControlIntoView(routeCombatPanel);};
        var low=new NumericUpDown{Left=290,Top=392,Width=120,Minimum=1,Maximum=99,Value=routeOptions.LowHealth};var high=new NumericUpDown{Left=475,Top=392,Width=120,Minimum=2,Maximum=100,Value=routeOptions.ResumeHealth};panel.Controls.AddRange(new Control[]{interval,low,high});
        var retreat=new CheckBox{Left=18,Top=434,Width=330,Text="Düşük canda işaretli güvenli noktaya çekil",Checked=routeOptions.Retreat};
        var recover=new CheckBox{Left=352,Top=434,Width=330,Text="İşaretler doğrudan takip edilir; otomatik yol yok",Checked=false,Enabled=false};panel.Controls.AddRange(new Control[]{retreat,recover});
        var info=new Label{Left=18,Top=478,Width=690,Height=64,Text="İşaret → yakındaki taş grubu → ilk ayrılınan nokta → sıradaki işaret. Uçlarda A → B → A.\nSaldırı türü, vuruş menzili ve taş grubu mesafesi rota bölümünden ayarlanır.\nİşaretlediğin yasak alanlar ve açık olan can koruması korunur."};panel.Controls.Add(info);
        Action apply=()=>{if(RouteOwnsCombat||isRecordingRoute)throw new InvalidOperationException("Önce rotayı durdur.");var p=CaptureRouteProfile();p.Name=name.Text.Trim();p.Priority=priority.SelectedIndex;p.Mixed=mixed.Checked;p.Names=names.Text.Trim();p.BossNames=bosses.Text.Trim();p.LowHealth=(int)low.Value;p.ResumeHealth=(int)high.Value;p.Retreat=retreat.Checked;p.Recovery=recover.Checked;p.Validate();routeOptions=p;InvalidateRoutePreflight();farmOnRouteCheckBox.Text="Yakın hedefleri kes · "+p.AttackMilliseconds+" ms";routePatrolStatus.Text="Rota seçenekleri uygulandı. Kalıcı saklamak için profil kaydet.";};
        Action<Action> safely=action=>{try{action();}catch(Exception ex){MessageBox.Show(f,ex.Message,"Rota ayarları");}};
        button("Ayarları uygula",18,555,170).Click+=(s,e)=>safely(apply);
        button("Profili kaydet",18,78,150).Click+=(s,e)=>safely(()=>{apply();var p=CaptureRouteProfile();if(string.IsNullOrWhiteSpace(p.Map))throw new InvalidOperationException("Önce haritada rota kaydet veya düzenle.");string path=Path.Combine(RouteProfilesFolder,Guid.NewGuid().ToString("N")+".json");var existing=saved.Items.Cast<RouteProfileChoice>().FirstOrDefault(c=>{var q=RouteProfileStore.Load(c.Path);return q.Name==p.Name&&q.Map==p.Map;});if(existing!=null){if(MessageBox.Show(f,"Aynı adlı harita profili güncellensin mi? Eski dosya .bak olarak korunur.","Profil",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;path=existing.Path;}RouteProfileStore.Save(path,p);refresh();routePatrolStatus.Text="Rota profili kaydedildi: "+p.Name;});
        button("Seçili profili aç",178,78,150).Click+=(s,e)=>safely(()=>{var c=saved.SelectedItem as RouteProfileChoice;if(c==null)return;if(MessageBox.Show(f,"Mevcut kaydedilmemiş rota ve ayarların yerini bu profil alacak. Devam?","Profil aç",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;ApplyRouteProfile(RouteProfileStore.Load(c.Path));f.Close();ShowRouteTools();});
        button("İşlem geçmişi",200,555,170).Click+=(s,e)=>ShowRouteHistory();
        button("Kapat",510,555,170).Click+=(s,e)=>f.Close();
        f.FormClosed+=(s,e)=>EndRouteEdit();f.Show(this);
    }
    private sealed class RouteProfileChoice { public string Path,Label;public override string ToString(){return Label;} }
    private void ShowRouteHistory()
    {
        if(routeHistoryForm!=null&&!routeHistoryForm.IsDisposed){routeHistoryForm.Activate();return;}
        var f=new Form{Text="GamePilot · İşlem geçmişi",ClientSize=new Size(880,480),StartPosition=FormStartPosition.CenterParent};routeHistoryForm=f;
        routeHistoryList=new ListBox{Dock=DockStyle.Fill,HorizontalScrollbar=true};routeHistoryList.Items.AddRange(activityEntries.Cast<object>().ToArray());
        var save=new Button{Dock=DockStyle.Bottom,Height=36,Text="Görünen geçmişi dosyaya kaydet"};save.Click+=(s,e)=>{using(var d=new SaveFileDialog{Filter="Metin|*.txt",FileName="GamePilot-islem-gecmisi.txt"})if(d.ShowDialog(f)==DialogResult.OK)try{File.WriteAllLines(d.FileName,routeHistoryList.Items.Cast<string>());}catch(Exception ex){MessageBox.Show(ex.Message);}};
        f.Controls.Add(routeHistoryList);f.Controls.Add(save);f.Show(this);
    }
    private void SaveRouteUndo()
    {
        InvalidateRoutePreflight();
        routeUndoPoints=routePoints.Select(p=>new RoutePoint{X=p.X,Y=p.Y}).ToList();routeUndoZones=routeOptions.Zones.Select(z=>new RouteZone{X=z.X,Y=z.Y,Radius=z.Radius}).ToList();routeUndoSafe=routeOptions.SafePoint==null?null:new RoutePoint{X=routeOptions.SafePoint.X,Y=routeOptions.SafePoint.Y};
    }
    private void UndoRouteEdit(){if(RouteOwnsCombat||isRecordingRoute||routeUndoPoints==null)return;InvalidateRoutePreflight();routePoints=routeUndoPoints;routeOptions.Zones=routeUndoZones;routeOptions.SafePoint=routeUndoSafe;routeUndoPoints=null;RefreshRouteDrawing();routePatrolStatus.Text="Son harita düzenlemesi geri alındı.";}
    private void BeginRouteEdit(string mode)
    {
        if(RouteOwnsCombat||isRecordingRoute||taskCancellation!=null||Field_02DF||normalBotRequested||dungeonNpcMarkerMode){MessageBox.Show("Önce çalışan hareket/görevi ve diğer harita işaretlemesini durdur.");return;}
        if(atlas==null||!mapCalibrated||string.IsNullOrEmpty(atlasName)){MessageBox.Show("Önce geçerli harita verisinin gelmesini bekle.");return;}
        if(!string.IsNullOrEmpty(routeMapName)&&routeMapName!=atlasName){MessageBox.Show("Rota başka haritaya ait; o haritaya geç veya yeni rota oluştur.");return;}
        EndRouteEdit();StopMapWalk("Rota düzenleniyor.");farmAreaDragging=false;routeMapName=routeEditMap=atlasName;routeEditMode=mode;ShowBigMap();bigMapForm.Text="Rota düzenle · "+mode+" · sağ tık/Esc: bitir (karakter yürümez)";
    }
    private void EndRouteEdit(){if(routeDragIndex>=0&&routeUndoPoints!=null){routePoints=routeUndoPoints;routeUndoPoints=null;}routeEditMode="";routeDragIndex=-1;routeZoneStart=routeZoneEnd=null;if(bigMapCanvas!=null&&!bigMapCanvas.IsDisposed){bigMapCanvas.Capture=false;bigMapCanvas.Invalidate();}}
    private bool HandleRouteMapInput(MouseEventArgs e,int phase)
    {
        if(string.IsNullOrEmpty(routeEditMode))return false;
        if(RouteOwnsCombat||isRecordingRoute||taskCancellation!=null||Field_02DF||normalBotRequested||dungeonNpcMarkerMode||atlasName!=routeEditMap||atlas==null||!mapCalibrated){EndRouteEdit();return true;}
        if(e.Button==MouseButtons.Right){if(phase==0)EndRouteEdit();return true;}
        if(e.Button!=MouseButtons.Left&&phase!=1)return true;
        var pixel=BigMapScreenToMap(e.Location);if(pixel.X<0||pixel.Y<0||pixel.X>=atlas.Width||pixel.Y>=atlas.Height){if(phase==2){if(routeDragIndex>=0&&routeUndoPoints!=null){routePoints=routeUndoPoints;routeUndoPoints=null;}routeDragIndex=-1;routeZoneStart=routeZoneEnd=null;bigMapCanvas.Capture=false;RefreshRouteDrawing();}return true;}
        var p=MapPixelToMemory(pixel);
        if(phase==0)
        {
            SaveRouteUndo();bigMapCanvas.Capture=true;
            int near=-1;double best=12;
            for(int i=0;i<routePoints.Count;i++){var m=ToMapPixel(routePoints[i].X,routePoints[i].Y);double d=RoutePatrol.Distance(m.X*bigMapScale+bigMapOffX,m.Y*bigMapScale+bigMapOffY,e.X,e.Y);if(d<best){best=d;near=i;}}
            if(routeEditMode=="taşı")routeDragIndex=near;
            else if(routeEditMode=="sil"&&near>=0)routePoints.RemoveAt(near);
            else if(routeEditMode=="ekle"&&routePoints.Count<20000)
            {
                int index=routePoints.Count;double distance=double.MaxValue;
                for(int i=0;i<routePoints.Count-1;i++){double d=RoutePatrol.SegmentDistance(p.X,p.Y,routePoints[i],routePoints[i+1]);if(d<distance){distance=d;index=i+1;}}
                routePoints.Insert(index,new RoutePoint{X=p.X,Y=p.Y});
            }
            else if(routeEditMode=="güvenli")routeOptions.SafePoint=new RoutePoint{X=p.X,Y=p.Y};
            else if(routeEditMode=="yasak")routeZoneStart=routeZoneEnd=p;
            else if(routeEditMode=="alan sil"){var z=routeOptions.Zones.LastOrDefault(v=>v.Contains(p.X,p.Y));if(z!=null)routeOptions.Zones.Remove(z);}
        }
        if(phase==1||phase==2)
        {
            if(routeDragIndex>=0&&routeDragIndex<routePoints.Count)routePoints[routeDragIndex]=new RoutePoint{X=p.X,Y=p.Y};
            if(routeZoneStart.HasValue)routeZoneEnd=p;
        }
        if(phase==2)
        {
            if(routeZoneStart.HasValue&&routeZoneEnd.HasValue&&routeOptions.Zones.Count<100){float radius=(float)RoutePatrol.Distance(routeZoneStart.Value.X,routeZoneStart.Value.Y,p.X,p.Y);if(radius>=50)routeOptions.Zones.Add(new RouteZone{X=routeZoneStart.Value.X,Y=routeZoneStart.Value.Y,Radius=radius});}
            routeZoneStart=routeZoneEnd=null;routeDragIndex=-1;bigMapCanvas.Capture=false;routePatrolStatus.Text="Rota düzenlendi · "+routePoints.Count+" nokta · "+routeOptions.Zones.Count+" yasak alan. Kalıcı saklamak için profil kaydet.";
        }
        RefreshRouteDrawing();return true;
    }
    private void PaintRouteSafety(Graphics g,Func<PointF,PointF> screen)
    {
        if(routeMapName!=atlasName)return;
        var zones=routeOptions.Zones.ToList();if(routeZoneStart.HasValue&&routeZoneEnd.HasValue)zones.Add(new RouteZone{X=routeZoneStart.Value.X,Y=routeZoneStart.Value.Y,Radius=(float)RoutePatrol.Distance(routeZoneStart.Value.X,routeZoneStart.Value.Y,routeZoneEnd.Value.X,routeZoneEnd.Value.Y)});
        using(var pen=new Pen(Color.OrangeRed,2))using(var fill=new SolidBrush(Color.FromArgb(60,255,60,30)))foreach(var z in zones){var c=screen(new PointF(z.X,z.Y));var e=screen(new PointF(z.X+z.Radius,z.Y));float r=Math.Abs(e.X-c.X);if(r>0){g.FillEllipse(fill,c.X-r,c.Y-r,2*r,2*r);g.DrawEllipse(pen,c.X-r,c.Y-r,2*r,2*r);}}
        if(routeOptions.SafePoint!=null){var p=screen(new PointF(routeOptions.SafePoint.X,routeOptions.SafePoint.Y));using(var pen=new Pen(Color.FromArgb(99,115,255),3)){g.DrawRectangle(pen,p.X-6,p.Y-6,12,12);g.DrawLine(pen,p.X-10,p.Y,p.X+10,p.Y);g.DrawLine(pen,p.X,p.Y-10,p.X,p.Y+10);}}
    }
}
