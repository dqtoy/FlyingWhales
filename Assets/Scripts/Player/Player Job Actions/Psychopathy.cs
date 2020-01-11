﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Psychopathy : PlayerJobAction {

    public Psychopathy() : base(INTERVENTION_ABILITY.PSYCHOPATHY) {
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
                currTarget.traitContainer.AddTrait(currTarget, newTrait);
            }
            base.ActivateAction(targetPOI);
        }
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.GetNormalTrait<Trait>("Beast") != null /*targetCharacter.role.roleType == CHARACTER_ROLE.BEAST*/) {
            return false;
        }
        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.GetNormalTrait<Trait>("Beast") != null /*targetCharacter.role.roleType == CHARACTER_ROLE.BEAST*/) {
            return false;
        }
        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return true;
    }
}

public class PsychopathyData : PlayerJobActionData {
    public override INTERVENTION_ABILITY ability => INTERVENTION_ABILITY.PSYCHOPATHY;
    public override string name { get { return "Psychopathy"; } }
    public override string description { get { return "Turns a character into a serial killer."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.MONSTER; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;


    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Serial Killer");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Serial Killer", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.GetNormalTrait<Trait>("Beast", "Serial Killer") != null) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}