using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialKiller : Trait {

    public SerialKiller() {
        name = "Serial Killer";
        description = "This character is a serial killer.";
        thoughtText = "[Character] is a serial killer.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}

public enum SERIAL_VICTIM_TYPE {
    GENDER, ROLE, TRAIT, STATUS,
}
public struct SerialVictim {
    public SERIAL_VICTIM_TYPE victimType1;
    public SERIAL_VICTIM_TYPE victimType2;


}
