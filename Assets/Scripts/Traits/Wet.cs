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
        daysDuration = 10; //if this trait is only temporary, then it should not advertise GET_WATER
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        addedTo.RemoveTrait("Burning");
        if (addedTo is IPointOfInterest && this.daysDuration == 0) {
            (addedTo as IPointOfInterest).AddAdvertisedAction(INTERACTION_TYPE.GET_WATER);
        }
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        if (removedFrom is IPointOfInterest && this.daysDuration == 0) {
            (removedFrom as IPointOfInterest).RemoveAdvertisedAction(INTERACTION_TYPE.GET_WATER);
        }
    }
    #endregion


}