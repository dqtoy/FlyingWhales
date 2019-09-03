using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glutton : Trait {

    public Glutton() {
        name = "Glutton";
        description = "This character has glutton trait.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if(addedTo is Character) {
            Character character = addedTo as Character;
            character.SetFullnessForcedTick(0);
        }
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        if (removedFrom is Character) {
            Character character = removedFrom as Character;
            character.SetFullnessForcedTick();
        }
    }
    #endregion
}
