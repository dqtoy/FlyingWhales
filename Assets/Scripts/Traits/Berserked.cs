using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserked : Trait {

    public override bool isNotSavable {
        get { return true; }
    }

    public Berserked() {
        name = "Berserked";
        description = "This character will attack anyone at random and may destroy objects.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }
}
