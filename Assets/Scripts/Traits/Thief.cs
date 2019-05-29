using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Trait {

    public override bool isPersistent { get { return true; } }

    public Thief() {
        name = "Thief";
        description = "This character has been branded as a Thief by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
        effects = new List<TraitEffect>();
    }
}
