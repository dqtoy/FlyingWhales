using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectGameObject : AreaMapObjectVisual<TileObject> {


    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(TileObject tileObject) {
        this.name = tileObject.ToString();
        SetVisual(InteriorMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType));
        collisionTrigger = this.transform.GetComponentInChildren<TileObjectCollisionTrigger>();
        onClickAction = () => UIManager.Instance.ShowTileObjectInfo(tileObject);
    }

    public override void UpdateTileObjectVisual(TileObject tileObject) {
        if (tileObject is Bed) {
            UpdateBedVisual(tileObject as Bed); //TODO: Transfer this to it's own object
        } else {
            SetVisual(InteriorMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType));
        }
        
    }

    private void UpdateBedVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            SetVisual(InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType));
        } else if (userCount == 1) {
            SetVisual(bed1Sleeping);
        } else if (userCount == 2) {
            SetVisual(bed2Sleeping);
        }
    }

    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) {
        this.SetRotation(furnitureSetting.rotation.z);
        //this.OverrideVisual(furnitureSetting.assetToUse);
    }
}
