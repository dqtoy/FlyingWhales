using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murderer : Trait {
    public override bool isPersistent { get { return true; } }
    public Murderer() {
        name = "Murderer";
        description = "This character has been branded as a Murderer by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.SERIOUS;
        effects = new List<TraitEffect>();
    }
}
