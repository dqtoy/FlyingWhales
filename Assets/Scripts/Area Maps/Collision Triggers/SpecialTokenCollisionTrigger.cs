using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTokenCollisionTrigger : BaseCollisionTrigger<SpecialToken>, IVisibleCollider {

    public IPointOfInterest poi { get; private set; }

    public override void Initialize(SpecialToken poi) {
        base.Initialize(poi);
        this.poi = poi;
    }
    public void SetMainColliderState(bool state) {
        mainCollider.enabled = state;
    }
}
