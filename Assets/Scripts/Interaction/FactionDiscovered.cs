using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionDiscovered : Interaction {

    public FactionDiscovered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FACTION_DISCOVERED, 0) {
        _name = "Faction Discovered";
    }

    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has discovered a new faction called [Faction Name] which owns [Location Name].
    }
}
