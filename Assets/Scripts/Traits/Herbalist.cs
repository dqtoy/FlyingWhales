using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbalist : Trait {
    public Herbalist() {
        name = "Herbalist";
        description = "Herbalists can create Healing Potions.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_ITEM_GOAP };
    }
}

