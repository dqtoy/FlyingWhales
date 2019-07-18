using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disabled : Trait {

    public Disabled() {
        name = "Disabled";
        description = "This object is disabled.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = GameManager.Instance.GetTicksBasedOnHour(4);
        effects = new List<TraitEffect>();
    }

    public override void OnAddTrait(IPointOfInterest targetPOI) {
        base.OnAddTrait(targetPOI);
        targetPOI.SetIsDisabledByPlayer(true);
    }

    public override void OnRemoveTrait(IPointOfInterest targetPOI) {
        base.OnRemoveTrait(targetPOI);
        targetPOI.SetIsDisabledByPlayer(false);
    }
}
