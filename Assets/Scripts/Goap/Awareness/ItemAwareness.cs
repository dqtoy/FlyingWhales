using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAwareness : IAwareness {
    public IPointOfInterest poi { get { return _specialToken; } }
    public SpecialToken item { get { return _specialToken; } }
    public Area knownLocation { get; private set; }
    public Faction factionOwner { get; private set; }

    private SpecialToken _specialToken;

    public ItemAwareness(SpecialToken specialToken) {
        _specialToken = specialToken;
    }

    public void SetKnownLocation(Area area) {
        knownLocation = area;
    }
    public void SetFactionOwner(Faction faction) {
        factionOwner = faction;
    }
}
