using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doctor : Trait {
    public Doctor() {
        name = "Doctor";
        description = "This character can cure illnesses.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }
}
