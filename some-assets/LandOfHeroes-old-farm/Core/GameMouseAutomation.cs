using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

public sealed class GameMouseRecorder : IDisposable
{
    const int WH_MOUSE_LL=14,WM_LBUTTONDOWN=0x0201,WM_LBUTTONUP=0x0202;
    [StructLayout(LayoutKind.Sequential)]struct POINT{public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]struct MSLLHOOKSTRUCT{public POINT pt;public uint mouseData,flags,time;public IntPtr extra;}
    delegate IntPtr HookProc(int code,IntPtr wParam,IntPtr lParam);
    [DllImport("user32.dll")]static extern IntPtr SetWindowsHookEx(int id,HookProc callback,IntPtr module,uint thread);
    [DllImport("user32.dll")]static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")]static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr wParam,IntPtr lParam);
    [DllImport("kernel32.dll")]static extern IntPtr GetModuleHandle(string name);
    readonly Func<Rectangle> bounds; readonly Action<TaskAction> captured; readonly HookProc callback; IntPtr hook; POINT down; long downAt; bool pressed; long lastActionAt;
    public GameMouseRecorder(Func<Rectangle> gameBounds,Action<TaskAction> onCaptured){bounds=gameBounds;captured=onCaptured;callback=Hook;}
    public bool Start(){if(hook!=IntPtr.Zero)return true;using(var process=Process.GetCurrentProcess())using(var module=process.MainModule)hook=SetWindowsHookEx(WH_MOUSE_LL,callback,GetModuleHandle(module.ModuleName),0);lastActionAt=Environment.TickCount;return hook!=IntPtr.Zero;}
    IntPtr Hook(int code,IntPtr wp,IntPtr lp)
    {
        if(code>=0){var data=(MSLLHOOKSTRUCT)Marshal.PtrToStructure(lp,typeof(MSLLHOOKSTRUCT));var rect=bounds();
            if(rect.Width>0&&rect.Height>0&&rect.Contains(data.pt.X,data.pt.Y)){
                if(wp.ToInt32()==WM_LBUTTONDOWN){down=data.pt;downAt=Environment.TickCount;pressed=true;}
                else if(wp.ToInt32()==WM_LBUTTONUP&&pressed){pressed=false;int dx=data.pt.X-down.X,dy=data.pt.Y-down.Y;bool drag=dx*dx+dy*dy>64;long now=Environment.TickCount;
                    captured(new TaskAction{Kind=drag?TaskActionKind.Drag:TaskActionKind.Click,X1=Scale(down.X-rect.Left,rect.Width),Y1=Scale(down.Y-rect.Top,rect.Height),X2=Scale(data.pt.X-rect.Left,rect.Width),Y2=Scale(data.pt.Y-rect.Top,rect.Height),DelayMilliseconds=(int)Math.Max(50,Math.Min(5000,now-lastActionAt))});lastActionAt=now;}
            }}return CallNextHookEx(hook,code,wp,lp);
    }
    static int Scale(int value,int total){return total<=0?0:GameCoordinateMapper.ToNormalized(value,0,total,1).X;}
    public void Dispose(){if(hook!=IntPtr.Zero){UnhookWindowsHookEx(hook);hook=IntPtr.Zero;}}
}
public static class GameMousePlayer
{
    [DllImport("user32.dll",SetLastError=true)]static extern bool PostMessage(IntPtr hWnd,uint message,IntPtr wParam,IntPtr lParam);
    [DllImport("user32.dll")]static extern bool IsWindow(IntPtr hWnd);
    const uint WM_MOUSEMOVE=0x0200,WM_LBUTTONDOWN=0x0201,WM_LBUTTONUP=0x0202; const int MK_LBUTTON=1;
    static Point PointAt(Rectangle r,int x,int y){return GameCoordinateMapper.FromNormalized(x,y,r.Width,r.Height);}
    static IntPtr Pack(Point p){return new IntPtr((p.Y<<16)|(p.X&0xffff));}
    public static void Play(IntPtr window,Rectangle bounds,TaskAction action,CancellationToken token)
    {
        if(window==IntPtr.Zero||!IsWindow(window))throw new InvalidOperationException("Oyun penceresi hazır değil.");
        var a=PointAt(bounds,action.X1,action.Y1);var b=PointAt(bounds,action.X2,action.Y2);
        PostMessage(window,WM_MOUSEMOVE,IntPtr.Zero,Pack(a));Thread.Sleep(40);PostMessage(window,WM_LBUTTONDOWN,new IntPtr(MK_LBUTTON),Pack(a));
        if(action.Kind==TaskActionKind.Drag){for(int i=1;i<=16;i++){token.ThrowIfCancellationRequested();var p=new Point(a.X+(b.X-a.X)*i/16,a.Y+(b.Y-a.Y)*i/16);PostMessage(window,WM_MOUSEMOVE,new IntPtr(MK_LBUTTON),Pack(p));Thread.Sleep(20);}}
        PostMessage(window,WM_LBUTTONUP,IntPtr.Zero,Pack(action.Kind==TaskActionKind.Drag?b:a));
    }
}

