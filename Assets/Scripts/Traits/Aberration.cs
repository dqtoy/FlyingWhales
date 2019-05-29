using System.Collections.Generic;

public class Aberration : Trait {
    public override bool isPersistent { get { return true; } }
    public Aberration() {
        name = "Aberration";
        description = "This character has been branded as an Aberration by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.SERIOUS;
        effects = new List<TraitEffect>();
    }
}