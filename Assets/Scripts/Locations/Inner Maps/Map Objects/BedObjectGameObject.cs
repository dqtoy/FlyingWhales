using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class BedObjectGameObject : MapObjectVisual<Bed> {

    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(Bed obj) {
        this.name = obj.ToString();
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(obj.tileObjectType, obj.state, obj.structureLocation.location.coreTile.biomeType));
    }

    public override void UpdateTileObjectVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType));
        } else if (userCount == 1) {
            SetVisual(bed1Sleeping);
        } else if (userCount == 2) {
            SetVisual(bed2Sleeping);
        }
    }

    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public override bool IsMapObjectMenuVisible() {
        return true;
    }
    public override void UpdateCollidersState(Bed obj) {
        // if (obj.advertisedActions.Count > 0) {
        //     EnableColliders();
        // } else {
        //     DisableColliders();
        // }
    }
}
