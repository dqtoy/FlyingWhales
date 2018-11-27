﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEncountered : Interaction {
    public CharacterEncountered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_ENCOUNTERED, 0) {

    }

    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has discovered a new faction called [Faction Name] which owns [Location Name].
    }
}