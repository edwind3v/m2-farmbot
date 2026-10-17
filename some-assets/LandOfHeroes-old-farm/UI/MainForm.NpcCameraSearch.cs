using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm
{
    private const uint MouseEventMove=0x0001,MouseEventLeftDown=0x0002,MouseEventLeftUp=0x0004,MouseEventRightDown=0x0008,MouseEventRightUp=0x0010;
    private const string ShamanCloseTemplateBase64="iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAIAAACQKrqGAAABrklEQVR42mP09fHv7O6/fvk8Aww8//Aezr5//ToDA8P9R/f37d3HeO36vd0HdsoIi525eA4i/f7rVwjjyrkr5sb69x/dV5RT3Ld3H7OTvR0fF7dXYJC9g9Oj+08fPX768f3Ln9+//vz+dcmyFWamRpy83Nev3L5+9y4TAwPDmQtnLl6+xMDAEJsYq6+rDzFy6rS5EMa+7QchDCYItXj+IrhqI31TiLpzF8/UVDXuO4yqFKJ68fzFDAwMyekZEHVzZ06Hq2NgYGA6dvrczdv37z+4f//BfWVVuWNHDnKwMJw8cfD396/3b9/n4ODg4OD49ecvBwcHwlQGBobjh05amZgzMDDY29gzYACE0uKivOKiPAYGhoNHDkJU1zfXIytltrK2+/DhfX5eNsS83r5Js2fNfHj3ob2TvYKcgpmZ1bMnzxgYGJ69eMbC8OfH3z+/FGXknr943t7ae+TEMQaGHxu2bHjz9k1lXaWEhOTfP/8YGBjef/iAcABMHRQcOX6kvakd2QGM/oFBDAwMYvySSGK/4MyvP/5BI+LwPkYOHgEVBcW/f5D1/4Cz3n9BsAEbxcItRvLdDgAAAABJRU5ErkJggg==";
    private const string ShamanTitleTemplateBase64="iVBORw0KGgoAAAANSUhEUgAAAK4AAAAOCAIAAAA5crBXAAADIElEQVR42u2YX0hTURzHf+e4lyUrKsoJOrrWq4zMPyGCElSQEtT7CDGoJx/0SdFEI0TEjOgPEUPQiB562ENYEY1Wy/VnWP6BPaRem4ilW1vQ2NS7e3o42/V27r3z6tWH4H45D797zj2fc37jyzm/O1TMlcC/IoQwPRhj0BAhBCHEzKIx7VedYvKN8NPptBRLEMUsEQCi0eXTdWf7+gdDU1+kgaV4TIr5UAgA+DDvfe21qGwIsz0IkNZepelyDkJI+glUZPKN8eUvyFdiJwEAQF//4Ks3L4sOHg5OjNOeWCJBg+nx6aoTTj7Mcw6Oc3B5+/YfUCzMNowxk4lSLEQrT5NvmE9kS9ADQ/mIASPA4tp6dVXl3j355y5crK07FeYXwwuLv2M/V5OJ1WTi0eMnlRVlVlt+aPpbaHYW37k1eL6hHvQII9hVmXx9fAxIaoiA1OSPcgW/BiemJgHA1ehyljpp5917bhp4n/syWJvNZrVac++BoOyhhxHRyFfL/npk8o3wGabqlTQyNCy5ocxZQX0wPhHsaO/2vstYwXKp6fLmWyFEcV+xFQoAVi15dKVq8g3xVdxACGHWGhkanix1uhpdTVeuUh+4H9wPBDbKybwXo6OF9oJPn4M5toIAoWyFglRyIUbyNPmG+Zr+WE0mD9mLZ+bmIpFoPB6vbzizEP7OOYrej/lEYd3z1COIYLFYCgoKlyPLWCcUIaS8hLKeNXpHmvxt85lvVOlR9SYKvP1YXV4FALU1tSqliM4DKrejN33B5O8enxnSerO1pbm1pRkAfH4fdUPX9a4tW2GXMjT525MIRGoEAW3ymJaZ8qVbW5rpeTBw83Z3Z3dPZw91g/uhu/x4eaZs1C5z5MrT86fYFnIx+Qb4GBALZy6h7GhKABBSaWGNK3Is/VjqvTHg/zAGkPI880SikbZrbXZ7YVoQASAWj6ueCljRdvgD2eQb4W/PZFkfZOQP+Ht7euUvWMDU/6Zcf1orNDPPUx8AQM3JagAAtCaNdrR3bBSnjpKjeirYHT0ATb4hviiKOueurPwCgGNHuLQg705JUezPRvwXr7g1t/K42pkAAAAASUVORK5CYII=";
    [DllImport("user32.dll")]private static extern void mouse_event(uint flags,uint dx,uint dy,uint data,UIntPtr extra);

    private string ConfiguredDungeonNpcName(int stage)
    {
        var setting=CurrentDungeonStageSetting(stage);
        return setting!=null&&!string.IsNullOrWhiteSpace(setting.NpcName)?setting.NpcName.Trim():"Kellam";
    }

    private static bool ShouldRotateNpcCameraSearch(int attempt){return attempt>0;}

    private static int NpcCameraDragPixels(int attempt,int clientWidth)
    {
        int step=(Math.Max(1,attempt)-1)%18+1;
        int unit=Math.Max(5,(int)Math.Round(clientWidth/160.0));
        int distance=Math.Min(Math.Max(5,clientWidth/2-30),step*unit);
        return step%2==1?distance:-distance;
    }

    private static Point NpcCameraDragOffset(int attempt,int clientWidth,int clientHeight)
    {
        int phase=(Math.Max(1,attempt)-1)%6;
        if(phase<4)return new Point(NpcCameraDragPixels(attempt,clientWidth),0);
        int vertical=Math.Max(18,Math.Min(Math.Max(18,clientHeight/3),clientHeight/10));
        return new Point(0,phase==4?-vertical:vertical);
    }

    private async Task RotateCameraForNpcSearchAsync(string npcName,int attempt,CancellationToken token)
    {
        var handle=TaskGameWindow();NativeRect rect;NativePoint original;
        if(handle==IntPtr.Zero||!GetClientRect(handle,out rect)||rect.Right<320||rect.Bottom<240||!GetCursorPos(out original))return;
        if(IsIconic(handle))ShowWindow(handle,9);
        var center=new NativePoint{X=rect.Right/2,Y=Math.Max(140,rect.Bottom/2)};
        if(!ClientToScreen(handle,ref center))return;
        var previous=GetForegroundWindow();bool blocked=false,rightDown=false;Point drag=NpcCameraDragOffset(attempt,rect.Right,rect.Bottom);
        try
        {
            BringWindowToTop(handle);SetForegroundWindow(handle);blocked=BlockInput(true);await Task.Delay(50,token);
            if(GetForegroundWindow()!=handle){BringWindowToTop(handle);SetForegroundWindow(handle);await Task.Delay(50,token);}
            if(GetForegroundWindow()!=handle)return;
            SetCursorPos(center.X,center.Y);await Task.Delay(35,token);
            mouse_event(MouseEventRightDown,0,0,0,UIntPtr.Zero);rightDown=true;
            for(int i=1;i<=8;i++)
            {
                token.ThrowIfCancellationRequested();
                if(GetForegroundWindow()!=handle)return;
                SetCursorPos(center.X+(int)Math.Round(drag.X*i/8.0),center.Y+(int)Math.Round(drag.Y*i/8.0));await Task.Delay(25,token);
            }
            string direction=drag.Y<0?"yukarı":drag.Y>0?"aşağı":drag.X>0?"sağa":"sola";
            taskStatus.Text=npcName+" görünmüyor · kamera "+direction+" çevrilerek aranıyor.";
            await Task.Delay(80,token);
        }
        finally
        {
            if(rightDown)mouse_event(MouseEventRightUp,0,0,0,UIntPtr.Zero);
            SetCursorPos(original.X,original.Y);
            if(previous!=IntPtr.Zero&&previous!=handle&&GetForegroundWindow()==handle)SetForegroundWindow(previous);
            if(blocked)BlockInput(false);
        }
    }

    private static byte[] BitmapBytes24(Bitmap bitmap,out int stride)
    {
        var area=new Rectangle(0,0,bitmap.Width,bitmap.Height);
        var data=bitmap.LockBits(area,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
        try
        {
            stride=Math.Abs(data.Stride);var bytes=new byte[stride*bitmap.Height];
            Marshal.Copy(data.Scan0,bytes,0,bytes.Length);return bytes;
        }
        finally{bitmap.UnlockBits(data);}
    }

    private static Bitmap TemplateBitmap(string encoded,Size target)
    {
        using(var stream=new MemoryStream(Convert.FromBase64String(encoded)))
        using(var loaded=new Bitmap(stream))
        {
            var size=target.Width>0&&target.Height>0?target:loaded.Size;
            var result=new Bitmap(size.Width,size.Height,PixelFormat.Format24bppRgb);
            using(var graphics=Graphics.FromImage(result))
            {
                graphics.InterpolationMode=size==loaded.Size?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;
                graphics.DrawImage(loaded,new Rectangle(Point.Empty,size));
            }
            return result;
        }
    }

    private static Point FindShamanCloseButton(Bitmap image){return FindShamanCloseButtonUnscaled(image);}

    // Windows can stretch a DPI-unaware game window. Normalize that captured image back to the
    // render size first, then search at the template's native scale. This keeps detection fast and
    // returns the match in the actual client coordinate system.
    private static Point FindShamanCloseButton(Bitmap image,int gameWidth,int gameHeight)
    {
        Bitmap normalized=null;
        try
        {
            Bitmap search=image;
            if(gameWidth>0&&gameHeight>0&&gameWidth<=4096&&gameHeight<=2160&&
               (Math.Abs(image.Width-gameWidth)>1||Math.Abs(image.Height-gameHeight)>1))
            {
                normalized=new Bitmap(gameWidth,gameHeight,PixelFormat.Format24bppRgb);
                using(var graphics=Graphics.FromImage(normalized))
                {
                    graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;
                    graphics.DrawImage(image,new Rectangle(0,0,gameWidth,gameHeight));
                }
                search=normalized;
            }
            var match=FindShamanCloseButtonUnscaled(search);
            if(match.X<0||normalized==null)return match;
            return GameCoordinateMapper.ToClient(match.X,match.Y,gameWidth,gameHeight,image.Width,image.Height);
        }
        finally{if(normalized!=null)normalized.Dispose();}
    }

    private static Point FindShamanCloseButtonUnscaled(Bitmap image)
    {
        using(var template=TemplateBitmap(ShamanCloseTemplateBase64,new Size(14,14)))
        using(var title=TemplateBitmap(ShamanTitleTemplateBase64,new Size(174,14)))
        {
            int imageStride,templateStride,titleStride;
            var source=BitmapBytes24(image,out imageStride);
            var sample=BitmapBytes24(template,out templateStride);
            var titleSample=BitmapBytes24(title,out titleStride);
            int width=template.Width,height=template.Height,bestSad=int.MaxValue,bestX=-1,bestY=-1,titleOffset=title.Width-template.Width;
            int closeLimit=width*height*3*42,titleLimit=title.Width*title.Height*3*32;
            // Bright diagonal pixels of the X come first so ordinary dark UI pixels are rejected quickly.
            int[] anchors={6,6,7,7,2,2,10,10,1,0};
            for(int y=0;y<=image.Height-height;y++)for(int x=titleOffset;x<=image.Width-width;x++)
            {
                int anchorSad=0;
                for(int a=0;a<anchors.Length&&anchorSad<=900;a+=2)
                {
                    int ax=anchors[a],ay=anchors[a+1];
                    int si=(y+ay)*imageStride+(x+ax)*3,ti=ay*templateStride+ax*3;
                    anchorSad+=Math.Abs(source[si]-sample[ti])+Math.Abs(source[si+1]-sample[ti+1])+Math.Abs(source[si+2]-sample[ti+2]);
                }
                if(anchorSad>900)continue;
                int sad=0;
                for(int ty=0;ty<height&&sad<=closeLimit&&sad<bestSad;ty++)
                {
                    int si=(y+ty)*imageStride+x*3,ti=ty*templateStride;
                    for(int tx=0;tx<width*3;tx++)sad+=Math.Abs(source[si+tx]-sample[ti+tx]);
                }
                if(sad>closeLimit||sad>=bestSad)continue;
                int titleSad=0,titleX=x-titleOffset;
                for(int ty=0;ty<title.Height&&titleSad<=titleLimit;ty++)
                {
                    int si=(y+ty)*imageStride+titleX*3,ti=ty*titleStride;
                    for(int tx=0;tx<title.Width*3;tx++)titleSad+=Math.Abs(source[si+tx]-titleSample[ti+tx]);
                }
                if(titleSad>titleLimit)continue;
                bestSad=sad;bestX=x;bestY=y;
                if(sad==0&&titleSad==0)return new Point(x,y);
            }
            return bestX>=0?new Point(bestX,bestY):new Point(-1,-1);
        }
    }
    private static Bitmap CaptureGameClient(NativePoint origin,NativeRect rect)
    {
        var image=new Bitmap(rect.Right-rect.Left,rect.Bottom-rect.Top,PixelFormat.Format24bppRgb);
        using(var graphics=Graphics.FromImage(image))graphics.CopyFromScreen(origin.X,origin.Y,0,0,image.Size,CopyPixelOperation.SourceCopy);
        return image;
    }

    private async Task<bool> ClickShamanCloseButtonAsync(CancellationToken token)
    {
        var handle=TaskGameWindow();NativeRect rect;NativePoint origin=new NativePoint(),original;
        if(handle==IntPtr.Zero||!GetClientRect(handle,out rect)||rect.Right<320||rect.Bottom<220||!ClientToScreen(handle,ref origin)||!GetCursorPos(out original))return false;
        var viewport=await GetGameViewportSizeAsync(token);if(viewport.Width<=0||viewport.Height<=0)viewport=new Size(rect.Right,rect.Bottom);
        var closeSize=GameCoordinateMapper.TemplateSize(14,14,viewport.Width,viewport.Height,rect.Right,rect.Bottom);
        if(IsIconic(handle))ShowWindow(handle,9);
        var previous=GetForegroundWindow();bool blocked=false,moved=false,leftDown=false;
        try
        {
            blocked=BlockInput(true);BringWindowToTop(handle);SetForegroundWindow(handle);await Task.Delay(90,token);
            if(GetForegroundWindow()!=handle){BringWindowToTop(handle);SetForegroundWindow(handle);await Task.Delay(90,token);}
            if(GetForegroundWindow()!=handle)return false;
            SetCursorPos(origin.X+rect.Right-24,origin.Y+rect.Bottom-24);moved=true;await Task.Delay(80,token);
            for(int attempt=0;attempt<2;attempt++)
            {
                Point match;
                using(var image=CaptureGameClient(origin,rect))match=FindShamanCloseButton(image,viewport.Width,viewport.Height);
                if(match.X<0)return attempt>0;
                var target=new NativePoint{X=origin.X+match.X+closeSize.Width/2,Y=origin.Y+match.Y+closeSize.Height/2};
                if(GetAncestor(WindowFromPoint(target),2)!=handle)return false;
                SetCursorPos(target.X,target.Y);await Task.Delay(70,token);
                mouse_event(MouseEventLeftDown,0,0,0,UIntPtr.Zero);leftDown=true;await Task.Delay(45,token);mouse_event(MouseEventLeftUp,0,0,0,UIntPtr.Zero);leftDown=false;
                await Task.Delay(220,token);
            }
            using(var image=CaptureGameClient(origin,rect))return FindShamanCloseButton(image,viewport.Width,viewport.Height).X<0;
        }
        finally
        {
            if(leftDown)mouse_event(MouseEventLeftUp,0,0,0,UIntPtr.Zero);
            if(moved)SetCursorPos(original.X,original.Y);
            if(previous!=IntPtr.Zero&&previous!=handle&&GetForegroundWindow()==handle)SetForegroundWindow(previous);
            if(blocked)BlockInput(false);
        }
    }

    private static string BuildShamanCloseScript()
    {
        string source;
        using(var stream=typeof(MainForm).Assembly.GetManifestResourceStream("Onur.SessionAgent.py"))
        using(var reader=new System.IO.StreamReader(stream))source=reader.ReadToEnd();
        const string startMarker="# BEGIN SHAMAN CLOSE HELPER",endMarker="# END SHAMAN CLOSE HELPER";
        int start=source.IndexOf(startMarker,StringComparison.Ordinal),end=source.IndexOf(endMarker,StringComparison.Ordinal);
        if(start<0||end<=start)throw new InvalidOperationException("Şaman kapatma kodu bulunamadı.");
        return source.Substring(start,end-start)+FindMainStream+
            "if ms is None:raise RuntimeError('Game window unavailable')\n"+
            "closed,remaining=_onur_shaman_close(ms.curPhaseWindow)\nf.write(str(closed)+'|'+str(remaining))";
    }

    private async Task<bool> HideShamanInformationWindowAsync(CancellationToken token)
    {
        if(!autoCloseShaman)return true;
        try
        {
            string result=await GamePythonAsync(BuildShamanCloseScript(),token);
            var parts=(result??"").Split('|');int closed,remaining;
            return parts.Length==2&&int.TryParse(parts[0],out closed)&&int.TryParse(parts[1],out remaining)&&remaining==0;
        }
        catch(Exception ex)when(!(ex is OperationCanceledException)){return false;}
    }

    private async Task<bool> CloseAccidentalCharacterWindowAsync(CancellationToken token)
    {
        if(!autoCloseShaman)return true;
        if(await HideShamanInformationWindowAsync(token))return true;
        // Screen fallback only when this client's UI API is unavailable.
        var handle=TaskGameWindow();NativeRect rect;NativePoint origin=new NativePoint();
        if(handle==IntPtr.Zero||GetForegroundWindow()!=handle||!GetClientRect(handle,out rect)||!ClientToScreen(handle,ref origin))return false;
        var viewport=await GetGameViewportSizeAsync(token);if(viewport.Width<=0||viewport.Height<=0)viewport=new Size(rect.Right,rect.Bottom);
        Point match;
        using(var image=CaptureGameClient(origin,rect))match=FindShamanCloseButton(image,viewport.Width,viewport.Height);
        if(match.X<0)return false;
        if(!await ClickShamanCloseButtonAsync(token))return false;
        return await HideShamanInformationWindowAsync(token);
    }

}
