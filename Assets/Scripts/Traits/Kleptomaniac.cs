using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kleptomaniac : Trait {
    public Kleptomaniac() {
        name = "Kleptomaniac";
        description = "This character has irresistible urge to steal.";
        thoughtText = "[Character] has irresistible urge to steal.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        (sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
        base.OnAddTrait(sourceCharacter);
    }
    #endregion
}
