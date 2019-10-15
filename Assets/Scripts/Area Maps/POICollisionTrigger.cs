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
    private BoxCollider2D mainCollider;

    public ProjectileReceiver projectileReciever;

    public virtual void Initialize(IPointOfInterest poi) {
        this.poi = poi;
        this.name = poi.name + " collision trigger";
        mainCollider = GetComponent<BoxCollider2D>();
        if (this is GhostCollisionTrigger || poi is GenericTileObject) {
            //by default, ghost colliders do not have projectile recievers.
            projectileReciever?.gameObject.SetActive(false);
        } else {
            projectileReciever.gameObject.SetActive(true);
            projectileReciever.Initialize(poi);
        }
        //gameObject.layer = LayerMask.GetMask("Area Maps");
    }

    public void SetLocation(LocationGridTile location) {
        _gridTileLocation = location;
    }

    public void SetMainColliderState(bool state) {
        mainCollider.enabled = state;
    }

    [ContextMenu("Print World Location")]
    public void PrintWorldLocation() {
        Debug.Log(transform.position);
    }

    private void OnDestroy() {
        poi = null;
    }
}
