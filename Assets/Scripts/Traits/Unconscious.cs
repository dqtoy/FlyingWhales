using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    //private GoapPlanJob _restrainJob;
    //private GoapPlanJob _removeTraitJob;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] is unconscious.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 144;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER }; //, INTERACTION_TYPE.DRINK_BLOOD
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override bool IsResponsibleForTrait(Character character) {
        return _responsibleCharacter == character;
    }
    public override string GetToolTipText() {
        if (_responsibleCharacter == null) {
            return description;
        }
        return "This character has been knocked out by " + _responsibleCharacter.name;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if(sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            //CheckToApplyRestrainJob();
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        //if (_restrainJob != null) {
        //    _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        //}
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacter("Restrain");
        _sourceCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        Character targetCharacter = traitOwner as Character;
        if (!targetCharacter.isDead && characterThatWillDoJob.isAtHomeArea && targetCharacter.GetTraitOf(TRAIT_TYPE.CRIMINAL) == null && CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
            if (!targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect);
                job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        if (characterThatWillDoJob.isAtHomeArea && !targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction 
            && (characterThatWillDoJob.role.roleType == CHARACTER_ROLE.SOLDIER || characterThatWillDoJob.role.roleType == CHARACTER_ROLE.CIVILIAN || characterThatWillDoJob.role.roleType == CHARACTER_ROLE.ADVENTURER)) {
            if (!characterThatWillDoJob.HasTraitOf(TRAIT_TYPE.CRIMINAL) && targetCharacter.GetNormalTrait("Restrained") == null && characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE
                && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.RESTRAIN)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = characterThatWillDoJob.specificLocation, targetPOI = targetCharacter });
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                job.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
