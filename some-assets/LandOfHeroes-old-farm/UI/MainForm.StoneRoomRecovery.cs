using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    // Selecting a target requests its HP board; it never issues a normal attack.
    private static string BuildStoneHealthScript(int vid)
    {
        return FindMainStream+"import player,chr\nvid="+vid+"\nhp=None\n"+
            "if ms is not None and chr.HasInstance(vid):\n"+
            " if player.GetTargetVID()!=vid:player.SetTarget(vid)\n"+
            " board=getattr(ms.curPhaseWindow,'targetBoard',None)\n"+
            " if board is not None and getattr(board,'vid',0)==vid and board.IsShow():\n"+
            "  for name in ('destHpPct','hpPercentage','currHpPct'):\n"+
            "   try:hp=float(getattr(board,name));break\n"+
            "   except Exception:pass\n"+
            "f.write(str(vid)+'|'+(str(hp) if hp is not None else 'unknown'))";
    }
    private async Task<float?> ReadStoneHealthAsync(int vid,CancellationToken token)
    {
        try
        {
            var parts=(await GamePythonAsync(BuildStoneHealthScript(vid),token)??"").Split('|');int actual;float hp;
            if(parts.Length==2&&int.TryParse(parts[0],out actual)&&actual==vid&&float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out hp)&&!float.IsNaN(hp)&&!float.IsInfinity(hp)&&hp>=0&&hp<=100)return hp;
        }
        catch(Exception ex)when(!(ex is OperationCanceledException)){}
        return null;
    }
    private bool TryStoneRecoveryGoal(EntityState stone,float x,float y,int cx,int cy,int floor,int attempt,out int cell,out PointF point)
    {
        cell=-1;point=PointF.Empty;
        double angle=Math.Atan2(y-stone.Y,x-stone.X);float distance=Distance2D(x,y,stone.X,stone.Y);
        if(distance<1)angle=attempt*Math.PI/2;
        double side=attempt%2==1?1:-1;
        float radius=distance<350?650f:Math.Min(1100f,Math.Max(600f,distance-350f));
        foreach(double turn in new[]{side*25,-side*25,side*60,-side*60,side*90,180})
        {
            double a=angle+turn*Math.PI/180;
            var desired=new PointF(stone.X+(float)Math.Cos(a)*radius,stone.Y+(float)Math.Sin(a)*radius);
            int gx,gy;CellOf(desired.X,desired.Y,out gx,out gy);int snapped=nav.Snap(gx,gy,2);
            if(snapped<0||!IsDungeonTargetOnFloor(nav,floor,snapped))continue;
            var candidate=CellToMemory(snapped);float fromStone=Distance2D(candidate.X,candidate.Y,stone.X,stone.Y);
            if(fromStone<350||fromStone>1400||Distance2D(x,y,candidate.X,candidate.Y)<180)continue;
            if(nav.FindPath(cx,cy,snapped%nav.Width,snapped/nav.Width)==null)continue;
            cell=snapped;point=candidate;return true;
        }
        return false;
    }
}
