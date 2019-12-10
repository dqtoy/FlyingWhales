using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BedObjectGameObject : AreaMapObjectVisual<Bed> {

    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(Bed bed) {
        this.name = bed.ToString();
        SetVisual(InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType));
    }

    public override void UpdateTileObjectVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            SetVisual(InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType));
        } else if (userCount == 1) {
            SetVisual(bed1Sleeping);
        } else if (userCount == 2) {
            SetVisual(bed2Sleeping);
        }
    }

    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
}
