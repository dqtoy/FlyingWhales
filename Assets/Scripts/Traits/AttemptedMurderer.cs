using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttemptedMurderer : Trait {
    public override bool isPersistent { get { return true; } }
    public AttemptedMurderer() {
        name = "Attempted Murderer";
        description = "This character has been branded as an Attempted Murderer by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
        effects = new List<TraitEffect>();
    }
}
