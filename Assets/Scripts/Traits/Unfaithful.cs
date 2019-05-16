using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unfaithful : Trait {

    public Unfaithful() {
        name = "Unfaithful";
        description = "This character has a tendency to be unfaithful.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

}
