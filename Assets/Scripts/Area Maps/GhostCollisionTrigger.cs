using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostCollisionTrigger : POICollisionTrigger {

    public override void Initialize(IPointOfInterest poi) {
        base.Initialize(poi);
        this.name = "Ghost of " + poi.name + " collision trigger";
        Messenger.AddListener<IPointOfInterest, List<LocationGridTile>>(Signals.CHECK_GHOST_COLLIDER_VALIDITY, CheckValidity);
        Messenger.AddListener<LocationGridTile, IPointOfInterest>(Signals.OBJECT_PLACED_ON_TILE, OnPlaceObjectOnTile);
    }

    private void CheckValidity(IPointOfInterest poi, List<LocationGridTile> knownLocations) {
        if (poi == this.poi && !knownLocations.Contains(this.gridTileLocation)) {
            //this object is no longer valid, destroy it
            GameObject.Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Listener for when an object is placed on the tile that this trigger is placed on
    /// </summary>
    /// <param name="tile">The tile where the object was placed</param>
    /// <param name="poi">The object that was placed</param>
    private void OnPlaceObjectOnTile(LocationGridTile tile, IPointOfInterest poi) {
        if (poi == this.poi && tile == this.gridTileLocation) {
            //this object is no longer valid, destroy it
            GameObject.Destroy(this.gameObject);
        }
    }

    public void OnDestroy() {
        Messenger.RemoveListener<IPointOfInterest, List<LocationGridTile>>(Signals.CHECK_GHOST_COLLIDER_VALIDITY, CheckValidity);
        Messenger.RemoveListener<LocationGridTile, IPointOfInterest>(Signals.OBJECT_PLACED_ON_TILE, OnPlaceObjectOnTile);
    }
}
