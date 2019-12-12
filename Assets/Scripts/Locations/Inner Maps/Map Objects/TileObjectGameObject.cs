using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileObjectGameObject : MapObjectVisual<TileObject> {


    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        this.name = tileObject.ToString();
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType));
        collisionTrigger = this.transform.GetComponentInChildren<TileObjectCollisionTrigger>();
    }

    public override void UpdateTileObjectVisual(TileObject tileObject) {
        if (tileObject is Bed) {
            UpdateBedVisual(tileObject as Bed); //TODO: Transfer this to it's own object
        } else {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType));
        }
        
    }

    private void UpdateBedVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType));
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

    #region Pointer Events
    protected override void OnPointerLeftClick(TileObject poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowTileObjectInfo(poi);
    }
    protected override void OnPointerRightClick(TileObject poi) {
        base.OnPointerRightClick(poi);
#if UNITY_EDITOR
        UIManager.Instance.poiTestingUI.ShowUI(poi);
#endif
    }
    protected override void OnPointerEnter(TileObject poi) {
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.ShowTileData(poi.gridTileLocation);
    }
    protected override void OnPointerExit(TileObject poi) {
        base.OnPointerExit(poi);
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
