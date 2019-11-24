using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedObjectGameObject : AreaMapGameObject<Bed> {

    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void Initialize(Bed bed) {
        this.name = bed.ToString();
        objectVisual.sprite = InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType);
    }

    public override void UpdateTileObjectVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            objectVisual.sprite = InteriorMapManager.Instance.GetTileObjectAsset(bed.tileObjectType, bed.state, bed.structureLocation.location.coreTile.biomeType);
        } else if (userCount == 1) {
            objectVisual.sprite = bed1Sleeping;
        } else if (userCount == 2) {
            objectVisual.sprite = bed2Sleeping;
        }
        
    }
}
