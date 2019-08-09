using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictPsychopathy : PlayerJobAction {

    public InflictPsychopathy() : base(INTERVENTION_ABILITY.INFLICT_PSYCHOPATHY) {
        description = "Turns a character into a serial killer.";
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
        abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character currTarget = targetPOI as Character;
            if (CanPerformActionTowards(assignedCharacter, currTarget)) {
                Trait newTrait = new SerialKiller();
                newTrait.SetLevel(level);
                currTarget.AddTrait(newTrait);
            }
            base.ActivateAction(assignedCharacter, targetPOI);
        }
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Serial Killer") != null) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Serial Killer") != null) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
}
