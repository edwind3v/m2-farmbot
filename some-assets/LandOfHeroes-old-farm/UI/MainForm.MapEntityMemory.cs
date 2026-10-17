using System;
using System.Collections.Generic;
using System.Linq;

public partial class MainForm
{
    private sealed class MapEntityMemory
    {
        public EntityState Entity;
        public DateTime SeenAt;
    }
    private sealed class MapEntityDisplay
    {
        public EntityState Entity;
        public bool Live;
    }
    private readonly Dictionary<int,MapEntityMemory> mapEntityMemory=new Dictionary<int,MapEntityMemory>();
    private string mapEntityMemoryMap;

    private void ClearMapEntityMemory(){mapEntityMemory.Clear();mapEntityMemoryMap=null;}

    private List<MapEntityDisplay> MapEntitiesForDisplay(List<EntityState> live)
    {
        live=live??new List<EntityState>();
        if(atlas==null)return live.Select(e=>new MapEntityDisplay{Entity=e,Live=true}).ToList();
        if(!string.Equals(mapEntityMemoryMap,atlas.Name,StringComparison.OrdinalIgnoreCase))
        {mapEntityMemory.Clear();mapEntityMemoryMap=atlas.Name;}
        DateTime now=DateTime.UtcNow;
        var liveIds=new HashSet<int>();
        foreach(var entity in live)
        {
            if(entity==null||entity.Property_03C9<=0)continue;
            if(entity.Property_03CA!=1&&entity.Property_03CA!=AddressCatalog.Field_035B&&entity.Property_03CA!=AddressCatalog.Field_035C)continue;
            liveIds.Add(entity.Property_03C9);
            mapEntityMemory[entity.Property_03C9]=new MapEntityMemory{Entity=CopyMapEntity(entity),SeenAt=now};
        }
        float px=memory.ReadFloat(AddressCatalog.Property_037B),py=memory.ReadFloat(AddressCatalog.Property_037C);
        foreach(var pair in mapEntityMemory.ToList())
        {
            if(liveIds.Contains(pair.Key))continue;
            var value=pair.Value;double age=(now-value.SeenAt).TotalSeconds;
            double ttl=value.Entity.Property_03CA==AddressCatalog.Field_035B?1800:value.Entity.Property_03CA==1?300:120;
            double dx=value.Entity.X-px,dy=value.Entity.Y-py;
            // When a remembered marker is back inside the client's reliable live area but no longer exists,
            // remove it quickly. Far markers stay visible until their type-specific expiry.
            if(age>ttl||(age>2&&dx*dx+dy*dy<=8500d*8500d))mapEntityMemory.Remove(pair.Key);
        }
        if(mapEntityMemory.Count>5000)
            foreach(var key in mapEntityMemory.OrderBy(p=>p.Value.SeenAt).Take(mapEntityMemory.Count-5000).Select(p=>p.Key).ToList())mapEntityMemory.Remove(key);
        return mapEntityMemory.Values.Select(v=>new MapEntityDisplay{Entity=v.Entity,Live=liveIds.Contains(v.Entity.Property_03C9)}).ToList();
    }
    private static EntityState CopyMapEntity(EntityState e)
    {
        return new EntityState{Property_03C4=e.Property_03C4,X=e.X,Y=e.Y,Z=e.Z,Property_03C8=e.Property_03C8,Property_03C9=e.Property_03C9,Property_03CA=e.Property_03CA,Property_03CB=e.Property_03CB,Distance=e.Distance,Property_03CD=e.Property_03CD};
    }
}
