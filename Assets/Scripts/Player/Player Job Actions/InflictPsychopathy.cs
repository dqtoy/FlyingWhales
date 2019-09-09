using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictPsychopathy : PlayerJobAction {

    public InflictPsychopathy() : base(INTERVENTION_ABILITY.INFLICT_PSYCHOPATHY) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
        //abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character currTarget = targetPOI as Character;
            if (CanPerformActionTowards(currTarget)) {
                Trait newTrait = new SerialKiller();
                newTrait.SetLevel(level);
                currTarget.AddTrait(newTrait);
            }
            base.ActivateAction(targetPOI);
        }
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Serial Killer") != null) {
            return false;
        }
        //if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Serial Killer") != null) {
            return false;
        }
        //if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
        //    return false;
        //}
        return true;
    }
}

public class InflictPsychopathyData : PlayerJobActionData {
    public override string name { get { return "Inflict Psychopathy"; } }
    public override string description { get { return "Turns a character into a serial killer."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.MONSTER; } }
}