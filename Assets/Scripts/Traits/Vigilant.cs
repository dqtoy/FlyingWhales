using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vigilant : Trait {

    public Vigilant() {
        name = "Vigilant";
        description = "Vigilant characters cannot be stealthily knocked out or pickpocketed.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }
}
