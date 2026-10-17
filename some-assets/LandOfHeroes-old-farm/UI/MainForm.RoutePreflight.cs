using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private CancellationTokenSource routeCheckCancellation;
    private RouteCheckResult routeCheckResult;
    private Label routeCheckStatus;
    private Button routeCheckButton;
    private string routeCheckSignature;
    private MinimapAtlas routeCheckedAtlas;
    private float routeCheckedOffsetX,routeCheckedOffsetY;
    private int routeCheckedSign;
    private string CurrentRouteCheckSignature(){return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(CaptureRouteProfile())+"|"+atlasName+"|"+mapOffX+"|"+mapOffY+"|"+mapYSign;}
    private void InitializeRoutePreflightUi()
    {
        int top=tabRoute.Controls.Cast<Control>().Max(c=>c.Bottom)+16;
        routeCheckButton=new Button{Name="routeCheckButton",Text="Rotayı çalıştırmadan kontrol et",Left=20,Top=top,Width=290,Height=34};
        routeCheckStatus=new Label{Name="routeCheckStatus",Left=20,Top=top+43,Width=480,Height=65,Text="Harita verisi üzerinden duvar, yasak alan ve erişim kontrolü yapar. Karakter hareket etmez."};
        routeCheckButton.Click+=async(s,e)=>await CheckCurrentRouteAsync();tabRoute.Controls.AddRange(new Control[]{routeCheckButton,routeCheckStatus});tabRoute.AutoScrollMinSize=new Size(0,routeCheckStatus.Bottom+16);
    }
    private async Task CheckCurrentRouteAsync()
    {
        if(routeCheckCancellation!=null){routeCheckCancellation.Cancel();return;}
        if(RouteOwnsCombat||isRecordingRoute||emergencyLatched){routeCheckStatus.Text="Önce kaydı/rotayı durdur ve acil kilidi kaldır.";return;}
        if(atlas==null||!mapCalibrated||routePoints.Count<2||string.IsNullOrWhiteSpace(routeMapName)||routeMapName!=atlasName){routeCheckStatus.Text="Doğru harita, harita verisi ve en az iki noktalı kayıt gerekli.";return;}
        var profile=CaptureRouteProfile();string signature=CurrentRouteCheckSignature();var source=atlas;int width=atlas.Width,height=atlas.Height;var attrs=(byte[])atlas.Attr.Clone();float ox=mapOffX,oy=mapOffY;int sign=mapYSign;
        var cancellation=new CancellationTokenSource();routeCheckCancellation=cancellation;routeCheckButton.Text="Kontrolü iptal et";routeCheckStatus.Text="Duvarlar ve bağlantılar kontrol ediliyor… Karakter hareket etmiyor.";
        try
        {
            var result=await Task.Run(()=>RoutePreflight.Check(profile,width,height,(x,y)=>(attrs[y*width+x]&1)!=0,p=>new PointF((p.X-ox)/100,(sign>0?p.Y-oy:oy-p.Y)/100),(x,y)=>new PointF((x+.5f)*100+ox,sign>0?(y+.5f)*100+oy:oy-(y+.5f)*100),cancellation.Token),cancellation.Token);
            if(IsDisposed||cancellation.IsCancellationRequested)return;
            if(!ReferenceEquals(atlas,source)||signature!=CurrentRouteCheckSignature()){routeCheckStatus.Text="Rota/harita değişti; eski kontrol sonucu uygulanmadı. Yeniden kontrol et.";return;}
            routeCheckResult=result;routeCheckSignature=signature;
            routeCheckedAtlas=source;routeCheckedOffsetX=ox;routeCheckedOffsetY=oy;routeCheckedSign=sign;
            routeCheckStatus.Text=(result.Passed?"Kontrol tamamlandı · erişim hatası bulunmadı":"Kontrol tamamlandı · "+result.Issues.Count(i=>i.Error)+" hata")+"\n"+result.Segments+" bağlantı incelendi; sonuç penceresindeki satırlar haritada işaretlenir.";
            RecordActivity("Rota ön kontrol",routeCheckStatus.Text.Replace('\n',' '));ShowRouteCheckResult();RefreshRouteDrawing();
        }
        catch(OperationCanceledException){if(!IsDisposed)routeCheckStatus.Text="Ön kontrol iptal edildi.";}
        catch(Exception ex){if(!IsDisposed)routeCheckStatus.Text="Kontrol tamamlanamadı: "+ex.Message;}
        finally{routeCheckCancellation=null;cancellation.Dispose();if(!IsDisposed)routeCheckButton.Text="Rotayı çalıştırmadan kontrol et";}
    }
    private void ShowRouteCheckResult()
    {
        var f=new Form{Text="Rota ön kontrol sonucu",ClientSize=new Size(740,410),StartPosition=FormStartPosition.CenterParent};var list=new ListBox{Dock=DockStyle.Fill,HorizontalScrollbar=true};
        list.Items.Add("Bu kontrol harita verisini kullanır; canlı/dinamik engelleri garanti etmez.");foreach(var issue in routeCheckResult.Issues)list.Items.Add(issue);if(routeCheckResult.Issues.Count==0)list.Items.Add("İncelenen rotada sorun bulunmadı.");
        var map=new Button{Dock=DockStyle.Bottom,Height=36,Text="Haritada sonucu göster · kırmızı: hata / turuncu: dolanma"};map.Click+=(s,e)=>{if(routeCheckResult!=null&&routeCheckSignature==CurrentRouteCheckSignature())ShowBigMap();else MessageBox.Show("Rota değişti; yeniden kontrol et.");};f.Controls.Add(list);f.Controls.Add(map);f.Show(this);
    }
    private void InvalidateRoutePreflight(){routeCheckResult=null;routeCheckSignature=null;routeCheckCancellation?.Cancel();if(routeCheckStatus!=null)routeCheckStatus.Text="Rota değişti; ön kontrolü yeniden çalıştır.";}
    private void PaintRouteCheck(Graphics g,Func<PointF,PointF> screen)
    {
        if(routeCheckResult==null||routeMapName!=atlasName)return;
        if(!ReferenceEquals(atlas,routeCheckedAtlas)||mapOffX!=routeCheckedOffsetX||mapOffY!=routeCheckedOffsetY||mapYSign!=routeCheckedSign){InvalidateRoutePreflight();return;}
        foreach(var issue in routeCheckResult.Issues.Where(i=>i.PointIndex>=0&&i.PointIndex<routePoints.Count))
        {
            var point=routePoints[issue.PointIndex];var p=screen(new PointF(point.X,point.Y));using(var pen=new Pen(issue.Error?Color.Red:Color.DarkOrange,3)){g.DrawEllipse(pen,p.X-9,p.Y-9,18,18);g.DrawLine(pen,p.X-6,p.Y-6,p.X+6,p.Y+6);}
        }
    }
}
