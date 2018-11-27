using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationObserved : Interaction {

    public LocationObserved(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.LOCATION_OBSERVED, 0) {
        _name = "Location Observed";
    }

    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has discovered a new faction called [Faction Name] which owns [Location Name].
    }
}
