using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all POI's collision trigger.
/// This is the object responsible for triggering collision with a CharacterMarker.
/// </summary>
public class POICollisionTrigger : MonoBehaviour {

    public IPointOfInterest poi { get; private set; }
    public virtual LocationGridTile gridTileLocation { get { return _gridTileLocation; } }

    private LocationGridTile _gridTileLocation;

    public virtual void Initialize(IPointOfInterest poi) {
        this.poi = poi;
        this.name = poi.name + " collision trigger";
        //gameObject.layer = LayerMask.GetMask("Area Maps");
    }

    public void SetLocation(LocationGridTile location) {
        _gridTileLocation = location;
    }

    [ContextMenu("Print World Location")]
    public void PrintWorldLocation() {
        Debug.Log(transform.position);
    }
}
