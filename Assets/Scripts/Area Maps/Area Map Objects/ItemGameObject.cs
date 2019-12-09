using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGameObject : AreaMapObjectVisual<SpecialToken> {

    public override void Initialize(SpecialToken poi) {
        this.name = poi.ToString();
        objectVisual.sprite = InteriorMapManager.Instance.GetItemAsset(poi.specialTokenType);
        collisionTrigger = transform.GetComponentInChildren<SpecialTokenCollisionTrigger>();
        onClickAction = () => UIManager.Instance.ShowItemInfo(poi);
    }

    public override void UpdateTileObjectVisual(SpecialToken specialToken) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
}
