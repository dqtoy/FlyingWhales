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

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        addedTo.RemoveTrait("Burning");
        if (addedTo is IPointOfInterest) {
            (addedTo as IPointOfInterest).AddAdvertisedAction(INTERACTION_TYPE.GET_WATER);
        }
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        if (removedFrom is IPointOfInterest) {
            (removedFrom as IPointOfInterest).RemoveAdvertisedAction(INTERACTION_TYPE.GET_WATER);
        }
    }
    #endregion


}