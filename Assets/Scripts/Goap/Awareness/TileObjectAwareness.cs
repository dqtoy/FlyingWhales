using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectAwareness : IAwareness {
    public IPointOfInterest poi { get { return _tileObject; } }
    public IPointOfInterest tileObject { get { return _tileObject; } }
    public Area knownLocation { get; private set; }

    private IPointOfInterest _tileObject;

    public TileObjectAwareness(IPointOfInterest tileObject) {
        _tileObject = tileObject;
    }

    public void SetKnownLocation(Area area) {
        knownLocation = area;
    }
}
