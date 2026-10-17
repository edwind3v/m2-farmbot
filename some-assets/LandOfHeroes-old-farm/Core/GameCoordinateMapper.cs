using System;
using System.Drawing;

public static class GameCoordinateMapper
{
    public static Point ToClient(int gameX,int gameY,int gameWidth,int gameHeight,int clientWidth,int clientHeight)
    {
        if(clientWidth<=0||clientHeight<=0)return Point.Empty;
        if(gameWidth<=0)gameWidth=clientWidth;
        if(gameHeight<=0)gameHeight=clientHeight;
        int x=(int)Math.Round(gameX*(double)clientWidth/gameWidth);
        int y=(int)Math.Round(gameY*(double)clientHeight/gameHeight);
        return new Point(Clamp(x,0,clientWidth-1),Clamp(y,0,clientHeight-1));
    }

    public static Point FromNormalized(int x,int y,int clientWidth,int clientHeight)
    {
        if(clientWidth<=0||clientHeight<=0)return Point.Empty;
        return new Point(
            Clamp((int)Math.Round(clientWidth*Clamp(x,0,10000)/10000.0),0,clientWidth-1),
            Clamp((int)Math.Round(clientHeight*Clamp(y,0,10000)/10000.0),0,clientHeight-1));
    }

    public static Point ToNormalized(int clientX,int clientY,int clientWidth,int clientHeight)
    {
        if(clientWidth<=0||clientHeight<=0)return Point.Empty;
        return new Point(
            Clamp((int)Math.Round(Clamp(clientX,0,clientWidth-1)*10000.0/clientWidth),0,10000),
            Clamp((int)Math.Round(Clamp(clientY,0,clientHeight-1)*10000.0/clientHeight),0,10000));
    }

    public static Size TemplateSize(int sourceWidth,int sourceHeight,int gameWidth,int gameHeight,int clientWidth,int clientHeight)
    {
        double sx=gameWidth>0?clientWidth/(double)gameWidth:1.0;
        double sy=gameHeight>0?clientHeight/(double)gameHeight:1.0;
        if(sx<0.5||sx>2.5)sx=1.0;
        if(sy<0.5||sy>2.5)sy=1.0;
        return new Size(Math.Max(1,(int)Math.Round(sourceWidth*sx,MidpointRounding.AwayFromZero)),Math.Max(1,(int)Math.Round(sourceHeight*sy,MidpointRounding.AwayFromZero)));
    }

    private static int Clamp(int value,int min,int max){return value<min?min:value>max?max:value;}
}

