using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectAwareness : IAwareness {
    public IPointOfInterest poi { get { return _tileObject; } }
    public TileObject tileObject { get { return _tileObject; } }
    public Area knownLocation { get { return knownGridLocation.parentAreaMap.area; } }
    public LocationGridTile knownGridLocation { get; private set; }

    private TileObject _tileObject;

    public TileObjectAwareness(IPointOfInterest tileObject) {
        _tileObject = tileObject as TileObject;
        SetKnownGridLocation(_tileObject.gridTileLocation);
    }

    public void SetKnownGridLocation(LocationGridTile gridLocation) {
        knownGridLocation = gridLocation;
    }

    public void OnAddAwareness(Character character) {
        _tileObject.AddAwareCharacter(character);
    }
    public void OnRemoveAwareness(Character character) {
        _tileObject.RemoveAwareCharacter(character);
    }
}
