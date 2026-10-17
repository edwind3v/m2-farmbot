using System;
using System.Collections.Generic;
using System.Linq;

// Target pursuit and collateral damage are deliberately separate decisions.
public static class RouteAttackPolicy
{
    public static bool CanPursue(int type,bool stones,bool monsters,bool stoneSplash)
    {
        return type==AddressCatalog.Field_035B&&(stones||stoneSplash) ||
            type==AddressCatalog.Field_035C&&monsters&&!stoneSplash;
    }
    public static bool AcceptWaitTarget(int focus,bool focusAlive,int focusType,int vid,int type,bool splash,double distanceFromStone,int range)
    {
        if(!splash)return focus==0||focus==vid;
        if(focus<=0||!focusAlive||focusType!=AddressCatalog.Field_035B)return false;
        if(vid==focus)return type==AddressCatalog.Field_035B;
        return type==AddressCatalog.Field_035C&&!double.IsNaN(distanceFromStone)&&!double.IsInfinity(distanceFromStone)&&distanceFromStone>=0&&distanceFromStone<=range;
    }
    public static List<int> SelectCycle(IEnumerable<int> ordered,int focus,int cap,ref int cursor)
    {
        var all=ordered.Distinct().ToList();
        var selected=new List<int>();if(all.Remove(focus))selected.Add(focus);
        if(cap<=0||all.Count+selected.Count<=cap)return selected.Concat(all).ToList();
        int slots=cap-selected.Count;
        if(all.Count>0){int start=(cursor&int.MaxValue)%all.Count;for(int i=0;i<slots&&i<all.Count;i++)selected.Add(all[(start+i)%all.Count]);cursor=(start+slots)%all.Count;}
        return selected;
    }
}
