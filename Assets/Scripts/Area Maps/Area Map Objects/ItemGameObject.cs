using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGameObject : AreaMapGameObject<SpecialToken> {

    public override void Initialize(SpecialToken poi) {
        this.name = poi.ToString();
        objectVisual.sprite = InteriorMapManager.Instance.GetItemAsset(poi.specialTokenType);
    }

    public override void UpdateTileObjectVisual(SpecialToken specialToken) {
        //
    }
}
