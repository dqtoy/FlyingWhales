using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wet : Trait {

    public Wet() {
        name = "Wet";
        description = "This is wet.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        addedTo.RemoveTrait("Burning");
    }
}