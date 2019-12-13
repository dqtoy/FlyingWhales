using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ItemGameObject : MapObjectVisual<SpecialToken> {

    public override void Initialize(SpecialToken poi) {
        base.Initialize(poi);
        this.name = poi.ToString();
        SetVisual(InnerMapManager.Instance.GetItemAsset(poi.specialTokenType));
        collisionTrigger = transform.GetComponentInChildren<SpecialTokenCollisionTrigger>();
    }

    public override void UpdateTileObjectVisual(SpecialToken specialToken) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    
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
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(poi);
        InnerMapManager.Instance.ShowTileData(poi.gridTileLocation);
    }
    protected override void OnPointerExit(SpecialToken poi) {
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
