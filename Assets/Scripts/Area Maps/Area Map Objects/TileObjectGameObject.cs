using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectGameObject : AreaMapGameObject<TileObject> {

    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(TileObject tileObject) {
        this.name = tileObject.ToString();
        objectVisual.sprite = InteriorMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType);
    }

    public override void UpdateTileObjectVisual(TileObject tileObject) {
        if (tileObject is Bed) {
            UpdateBedVisual(tileObject as Bed); //TODO: Transfer this to it's own object
        } else {
            objectVisual.sprite = InteriorMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType);
        }
        
    }

    private void UpdateBedVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            objectVisual.sprite = InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType);
        } else if (userCount == 1) {
            objectVisual.sprite = bed1Sleeping;
        } else if (userCount == 2) {
            objectVisual.sprite = bed2Sleeping;
        }
    }

    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) {
        this.SetRotation(furnitureSetting.rotation.z);
        //this.OverrideVisual(furnitureSetting.assetToUse);
    }
}
