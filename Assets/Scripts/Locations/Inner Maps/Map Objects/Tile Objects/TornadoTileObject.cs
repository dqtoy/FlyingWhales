using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class TornadoTileObject : TileObject {

    public int radius { get; private set; }
    public int durationInTicks { get; private set; }
    public override LocationGridTile gridTileLocation {
        get {
            if (mapVisual != null) {
                TornadoVisual tornadoVisual = mapVisual as TornadoVisual;
                if (tornadoVisual.isSpawned) {
                    return tornadoVisual.gridTileLocation;
                }
            }
            return base.gridTileLocation;
        }
    }
    private TornadoVisual _tornadoVisual;
    public TornadoTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>(){ INTERACTION_TYPE.SNUFF_TORNADO };
        Initialize(TILE_OBJECT_TYPE.TORNADO);
    }

    protected override void CreateAreaMapGameObject() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectAreaMapObject(this.tileObjectType);
        _tornadoVisual = obj.GetComponent<TornadoVisual>();
        mapVisual = _tornadoVisual;
    }

    public void SetRadius(int radius) {
        this.radius = radius;
    }
    public void SetDuration(int duration) {
        this.durationInTicks = duration;
    }

    public void ForceExpire() {
        _tornadoVisual.Expire();
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Tornado";
    }
}
