using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pessimist : Trait {

    public Pessimist() {
        name = "Pessimist";
        description = "Pessimists lose happiness more quickly than normal.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;
        mutuallyExclusive = new string[] { "Optimist" };
    }

    #region Overrides
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //Will reduce Happiness Meter to become Forlorn. If already Forlorn, reduce Happiness Meter by a further 1000.
        if (character.isForlorn) {
            character.AdjustHappiness(-1000);
        } else {
            character.SetHappiness(Character.HAPPINESS_THRESHOLD_2);
        }
    }
    #endregion
}
