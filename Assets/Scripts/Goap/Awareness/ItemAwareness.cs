using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAwareness : IAwareness {
    public IPointOfInterest poi { get { return _specialToken; } }
    public SpecialToken item { get { return _specialToken; } }
    public LocationGridTile knownGridLocation { get; private set; }
    public Faction factionOwner { get; private set; }

    private SpecialToken _specialToken;

    public ItemAwareness(SpecialToken specialToken) {
        _specialToken = specialToken;
        SetKnownGridLocation(specialToken.gridTileLocation);
    }

    public void SetKnownGridLocation(LocationGridTile tile) {
        knownGridLocation = tile;
    }
    public void SetFactionOwner(Faction faction) {
        factionOwner = faction;
    }

    public void OnAddAwareness(Character character) { }
    public void OnRemoveAwareness(Character character) { }
}
