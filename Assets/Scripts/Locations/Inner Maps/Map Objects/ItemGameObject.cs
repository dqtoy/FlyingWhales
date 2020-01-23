using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ItemGameObject : MapObjectVisual<SpecialToken> {

    private System.Func<bool> _isMenuShowing;
    
    public override void Initialize(SpecialToken obj) {
        base.Initialize(obj);
        this.name = obj.ToString();
        SetVisual(InnerMapManager.Instance.GetItemAsset(obj.specialTokenType));
        collisionTrigger = transform.GetComponentInChildren<SpecialTokenCollisionTrigger>();
        _isMenuShowing = () => IsMenuShowing(obj);
        UpdateSortingOrders(obj);
    }

    public override void UpdateTileObjectVisual(SpecialToken specialToken) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }

    #region Inquiry
    private bool IsMenuShowing(SpecialToken item) {
        return UIManager.Instance.itemInfoUI.isShowing &&
               UIManager.Instance.itemInfoUI.activeItem == item;
    }
    public override bool IsMapObjectMenuVisible() {
        return _isMenuShowing.Invoke();
    }
    #endregion
    
    #region Pointer Events
    protected override void OnPointerLeftClick(SpecialToken poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowItemInfo(poi);
    }
    protected override void OnPointerRightClick(SpecialToken poi) {
        base.OnPointerRightClick(poi);
#if UNITY_EDITOR
        UIManager.Instance.poiTestingUI.ShowUI(poi);
#endif
    }
    protected override void OnPointerEnter(SpecialToken poi) {
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(poi);
        InnerMapManager.Instance.ShowTileData(poi.gridTileLocation);
    }
    protected override void OnPointerExit(SpecialToken poi) {
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
    public override void UpdateCollidersState(SpecialToken obj) {
        if (obj.advertisedActions.Count > 0) {
            SetAsVisibleToCharacters();
        } else {
            SetAsInvisibleToCharacters();
        }
    }
    #endregion
}
