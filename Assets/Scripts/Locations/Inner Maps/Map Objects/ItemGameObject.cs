using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ItemGameObject : MapObjectVisual<SpecialToken> {

    public override void Initialize(SpecialToken poi) {
        this.name = poi.ToString();
        SetVisual(InnerMapManager.Instance.GetItemAsset(poi.specialTokenType));
        collisionTrigger = transform.GetComponentInChildren<SpecialTokenCollisionTrigger>();
        onClickAction = () => UIManager.Instance.ShowItemInfo(poi);
    }

    public override void UpdateTileObjectVisual(SpecialToken specialToken) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
}
