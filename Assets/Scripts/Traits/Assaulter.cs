using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assaulter : Trait {

    public Assaulter() {
        name = "Assaulter";
        description = "This character has been branded as a Assaulter by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
        effects = new List<TraitEffect>();
    }
}
