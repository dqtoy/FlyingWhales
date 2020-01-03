using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileObjectGameObject : MapObjectVisual<TileObject> {
    
    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;
    
    private System.Func<bool> _isMenuShowing;

    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        this.name = tileObject.ToString();
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject.tileObjectType, tileObject.state, tileObject.structureLocation.location.coreTile.biomeType));
        collisionTrigger = this.transform.GetComponentInChildren<TileObjectCollisionTrigger>();
        _isMenuShowing = () => IsMenuShowing(tileObject);
        
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

    #region Inquiry
    private bool IsMenuShowing(TileObject obj) {
        return UIManager.Instance.tileObjectInfoUI.isShowing &&
               UIManager.Instance.tileObjectInfoUI.activeTileObject == obj;
    }
    public override bool IsMapObjectMenuVisible() {
        return _isMenuShowing.Invoke();
    }
    #endregion
    
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
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(poi);
        InnerMapManager.Instance.ShowTileData(poi.gridTileLocation);
    }
    protected override void OnPointerExit(TileObject poi) {
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Colliders
    public override void UpdateCollidersState(TileObject obj) {
        if (obj.advertisedActions.Count > 0) {
            SetAsVisibleToCharacters();
        } else {
            SetAsInvisibleToCharacters();
        }
    }
    #endregion
}
