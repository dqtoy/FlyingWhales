using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

public class CharacterAwareness : IAwareness {
    public IPointOfInterest poi { get { return _character; } }
    public Character character { get { return _character; } }
    public LocationGridTile knownGridLocation { get; private set; }

    private Character _character;

    public CharacterAwareness(Character character) {
        _character = character;
        SetKnownGridLocation(character.gridTileLocation);
    }

    public void SetKnownGridLocation(LocationGridTile tile) {
        knownGridLocation = tile;
    }

    public void OnAddAwareness(Character character) { }
    public void OnRemoveAwareness(Character character) { }
}
