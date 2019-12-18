﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectCollisionTrigger : BaseCollisionTrigger<TileObject>, IVisibleCollider {

    public IPointOfInterest poi { get; private set; }

    public bool IgnoresStructureDifference() {
        if (poi is TornadoTileObject) {
            return true;
        }
        return false;
    }

    public override void Initialize(TileObject poi) {
        base.Initialize(poi);
        this.poi = poi;
        if (poi is GenericTileObject) {
            projectileReceiver?.gameObject.SetActive(false);
        }
    }
}