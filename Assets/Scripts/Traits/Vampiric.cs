using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vampiric : Trait {
    //private Character _character;

    public Vampiric() {
        name = "Vampiric";
        description = "This character sucks blood.";
        thoughtText = "[Character] sucks blood.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.jobQueue.CancelAllJobs("Fullness");
            character.AdjustDoNotGetTired(1);
            character.ResetTirednessMeter();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.jobQueue.CancelAllJobs("Fullness");
            character.AdjustDoNotGetTired(-1);
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion
}
