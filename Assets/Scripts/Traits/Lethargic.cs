using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lethargic : Trait {
    public Lethargic() {
        name = "Lethargic";
        description = "This character lost a lot of blood.";
        thoughtText = "[Character] lost a lot of blood.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = GameManager.ticksPerDay;
        //effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.AdjustSpeedModifier(-0.5f);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.AdjustSpeedModifier(0.5f);
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    #endregion
}
