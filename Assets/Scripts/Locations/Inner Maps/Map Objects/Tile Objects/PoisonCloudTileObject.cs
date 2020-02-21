using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public sealed class PoisonCloudTileObject : MovingTileObject {

    private PoisonCloudMapObjectVisual _poisonCloudVisual;
    
    public PoisonCloudTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD);
        traitContainer.RemoveTrait(this, "Flammable");
        RemoveCommonAdvertisements();
    }
    
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _poisonCloudVisual = mapVisual as PoisonCloudMapObjectVisual;
    }
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_poisonCloudVisual != null) {
            if (_poisonCloudVisual.isSpawned) {
                tile = _poisonCloudVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
}
