using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm
{
    private sealed class NpcApproachState
    {
        public int Vid,Attempt;public string Map;public List<PointF> Path;public bool Reposition;
        public DateTime PlanAt;public readonly NpcApproachProgress Progress=new NpcApproachProgress();
    }
    private static void RepositionNpcAfterOcclusion(NpcApproachState state,int cameraAttempt)
    {
        if(cameraAttempt>0&&cameraAttempt%6==0){state.Reposition=true;state.Attempt++;state.Path=null;state.Progress.Reset();}
    }
    private static bool NpcApproachComplete(EntityState target,NpcApproachState state,float x,float y)
    {
        if(!NpcApproachPolicy.Ready(Distance2D(x,y,target.X,target.Y)))return false;
        return !state.Reposition||(state.Path!=null&&state.Path.Count>0&&Distance2D(x,y,state.Path[state.Path.Count-1].X,state.Path[state.Path.Count-1].Y)<65);
    }
    private static string BuildNpcPositionScript(string name,int vid)
    {
        return NpcIdentityScript()+"import chr,player\nvid="+vid+"\n"+
            "if _onur_npc_matches(vid,"+PythonLiteral(name)+"):\n"+
            " p=chr.GetPixelPosition(vid);m=player.GetMainCharacterPosition()\n"+
            " f.write(str(vid)+'|'+str(float(p[0]-m[0]))+'|'+str(float(p[1]-m[1])))\n"+
            "else:f.write('none')\n";
    }
    private async Task<EntityState> ReadVerifiedNpcPositionAsync(string name,int vid,CancellationToken token)
    {
        var values=(await GamePythonAsync(BuildNpcPositionScript(name,vid),token)??"").Split('|');int actual;float dx,dy;
        if(values.Length!=3||!int.TryParse(values[0],out actual)||actual!=vid||!float.TryParse(values[1],NumberStyles.Float,CultureInfo.InvariantCulture,out dx)||!float.TryParse(values[2],NumberStyles.Float,CultureInfo.InvariantCulture,out dy)||!IsFiniteCoordinate(dx)||!IsFiniteCoordinate(dy))return null;
        // Deltas avoid stale map offsets. The game's local Y can be mirrored in native memory.
        return new EntityState{Property_03C9=vid,Property_03CA=1,Property_03CD=name,X=TaskX+dx,Y=TaskY+dy*(mapYSign<0?-1:1)};
    }
    private bool PlanNpcApproach(NpcApproachState state,EntityState target,bool escaping)
    {
        var here=new PointF(TaskX,TaskY);int self=memory.ReadInt32(AddressCatalog.Property_037E);
        var obstacles=ReadRadarEntities().Where(e=>e.Property_03C9!=self&&e.Property_03C9!=target.Property_03C9&&IsFiniteCoordinate(e.X)&&IsFiniteCoordinate(e.Y)&&Distance2D(here.X,here.Y,e.X,e.Y)<8000)
            .Select(e=>Tuple.Create(new PointF(e.X,e.Y),e.Property_03CA==AddressCatalog.Field_035B?240f:140f)).ToList();
        Func<PointF,bool> blocked=null;
        if(atlas!=null&&atlas.Name==atlasName&&mapCalibrated)blocked=p=>{var cell=ToMapPixel(p.X,p.Y);int x=(int)Math.Floor(cell.X),y=(int)Math.Floor(cell.Y);return x<0||y<0||x>=atlas.Width||y>=atlas.Height||atlas.IsBlocked(x,y);};
        List<PointF> path;
        bool planned=NpcApproachPolicy.TryPlan(here,new PointF(target.X,target.Y),escaping?null:obstacles,blocked,state.Attempt,out path);
        state.Path=path;state.PlanAt=DateTime.UtcNow;return planned;
    }
    private async Task<bool> MoveNpcToTalkPositionAsync(EntityState target,NpcApproachState state,CancellationToken token)
    {
        if(state.Vid!=target.Property_03C9||state.Map!=atlasName){state.Vid=target.Property_03C9;state.Map=atlasName;state.Path=null;state.Attempt=0;state.Reposition=false;state.Progress.Reset();}
        if(NpcApproachComplete(target,state,TaskX,TaskY)){ReleaseMovement();state.Path=null;state.Reposition=false;state.Progress.Reset();return true;}
        long now=movementClock.ElapsedMilliseconds;
        bool stuck=state.Progress.Observe(new PointF(TaskX,TaskY),now);
        if(stuck){state.Attempt++;state.Path=null;}
        bool recoverCollision=stuck&&taskDungeonRunning&&taskUiStage==1&&!IsX86ClientMode;
        bool oldWall=wallCheck.Checked,oldMount=binekWallCheck.Checked;
        int owner=memory.Field_0197?.Property_0268?.Id??0;
        try
        {
            if(state.Path==null||DateTime.UtcNow-state.PlanAt>TimeSpan.FromSeconds(3))PlanNpcApproach(state,target,recoverCollision);
            if(state.Path==null){ReleaseMovement();taskStatus.Text=target.Property_03CD+" için güvenli yaklaşma yolu aranıyor.";await Task.Delay(150,token);return false;}
            if(recoverCollision){wallCheck.Checked=true;binekWallCheck.Checked=true;taskStatus.Text=target.Property_03CD+" yaklaşmasında takılma gideriliyor · engel geçişi geçici açık.";}
            var until=DateTime.UtcNow.AddMilliseconds(recoverCollision?1000:500);
            while(DateTime.UtcNow<until)
            {
                token.ThrowIfCancellationRequested();
                if((memory.Field_0197?.Property_0268?.Id??0)!=owner)throw new InvalidOperationException("Bağlı oyun değişti.");
                if(TryRunReviveEscape())return false;
                float x=TaskX,y=TaskY;
                if(NpcApproachComplete(target,state,x,y)){state.Path=null;state.Reposition=false;state.Progress.Reset();return true;}
                // Keep the A* corners: a shortcut through an actor would reproduce the original trap.
                while(state.Path.Count>1&&Distance2D(x,y,state.Path[0].X,state.Path[0].Y)<65)state.Path.RemoveAt(0);
                var next=state.Path[0];
                SteerDungeon(x,y,next.X,next.Y);await Task.Delay(40,token);
            }
            taskStatus.Text=target.Property_03CD+" tıklama mesafesine gidiliyor · "+(int)Distance2D(TaskX,TaskY,target.X,target.Y)+".";
            return false;
        }
        finally
        {
            try{ReleaseMovement();}
            finally
            {
                // Restore even when releasing movement fails; never write to another bound client.
                if(recoverCollision&&(memory.Field_0197?.Property_0268?.Id??0)==owner){wallCheck.Checked=oldWall;binekWallCheck.Checked=oldMount;}
            }
        }
    }
}
