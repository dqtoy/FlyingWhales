using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendersRevealed : Interaction {
    public DefendersRevealed(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DEFENDERS_REVEALED, 0) {
    }

    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has discovered a new faction called [Faction Name] which owns [Location Name].
    }
}
