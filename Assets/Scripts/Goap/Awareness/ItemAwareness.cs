using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAwareness : IAwareness {
    public IPointOfInterest poi { get { return _specialToken; } }
    public SpecialToken item { get { return _specialToken; } }
    public LocationGridTile knownLocation { get; private set; }
    public Faction factionOwner { get; private set; }

    private SpecialToken _specialToken;

    public ItemAwareness(SpecialToken specialToken) {
        _specialToken = specialToken;
        SetKnownLocation(specialToken.gridTileLocation);
    }

    public void SetKnownLocation(LocationGridTile tile) {
        knownLocation = tile;
    }
    public void SetFactionOwner(Faction faction) {
        factionOwner = faction;
    }
}
