using System.Collections.Generic;

public class IronMaiden : TileObject {
    
    public IronMaiden() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.IRON_MAIDEN);
        RemoveCommonAdvertisements();
    }
    public IronMaiden(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
    public override bool CanBeDamaged() {
        //prevent iron maiden in torture chamber from being damaged.
        return structureLocation != null 
               && structureLocation.structureType != STRUCTURE_TYPE.TORTURE_CHAMBER; 
    }
}
