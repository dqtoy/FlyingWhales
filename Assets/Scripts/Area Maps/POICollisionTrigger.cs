using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all POI's collision trigger.
/// This is the object responsible for triggering collision with a CharacterMarker.
/// </summary>
public class POICollisionTrigger : MonoBehaviour {

    public IPointOfInterest poi { get; private set; }

    public void Initialize(IPointOfInterest poi) {
        this.poi = poi;
    }
}
