﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class TornadoTileObject : TileObject {

    public int radius { get; private set; }
    public int durationInTicks { get; private set; }
    //public override LocationGridTile gridTileLocation {
    //    get {
    //        if (areaMapVisual != null) {
    //            TornadoVisual tornadoVisual = areaMapVisual as TornadoVisual;
    //            if (tornadoVisual.isSpawned) {
    //                return tornadoVisual.gridTileLocation;
    //            }
    //        }
    //        return base.gridTileLocation;
    //    }
    //}
    public TornadoTileObject() {
        Initialize(TILE_OBJECT_TYPE.TORNADO);
    }

    protected override void CreateAreaMapGameObject() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectAreaMapObject(this.tileObjectType);
        mapVisual = obj.GetComponent<TornadoVisual>();
    }

    public void SetRadius(int radius) {
        this.radius = radius;
    }
    public void SetDuration(int duration) {
        this.durationInTicks = duration;
    }

    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
}