using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vampiric : Trait {
    //private Character _character;

    private int _flatAttackMod;
    private int _flatHPMod;
    private int _flatSpeedMod;

    public Vampiric(int level) {
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
        _flatAttackMod = 100;
        _flatHPMod = 500;
        _flatSpeedMod = 100;
        VamipiricLevel(level);
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    public void VamipiricLevel(int level) {
        _flatAttackMod *= level;
        _flatHPMod *= level;
        _flatSpeedMod *= level;
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY);
            character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY_STARVING);
            character.AdjustDoNotGetTired(1);
            character.ResetTirednessMeter();
            character.AdjustAttackMod(_flatAttackMod);
            character.AdjustMaxHPMod(_flatHPMod);
            character.AdjustSpeedMod(_flatSpeedMod);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY);
            character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY_STARVING);
            character.AdjustDoNotGetTired(-1);
            character.AdjustAttackMod(-_flatAttackMod);
            character.AdjustMaxHPMod(-_flatHPMod);
            character.AdjustSpeedMod(-_flatSpeedMod);
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    #endregion
}
