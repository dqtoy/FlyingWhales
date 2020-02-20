using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class PoisonCloudTileObject : MovingTileObject {

    private PoisonCloudMapObject _poisonCloudVisual;
    
    public PoisonCloudTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD);
        traitContainer.RemoveTrait(this, "Flammable");
        RemoveCommonAdvertisements();
    }
    
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectAreaMapObject(this.tileObjectType);
        _poisonCloudVisual = obj.GetComponent<PoisonCloudMapObject>();
        mapVisual = _poisonCloudVisual;
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
