using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _restrainJob;
    //private GoapPlanJob _removeTraitJob;

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] is unconscious.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0; //144
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER }; //, INTERACTION_TYPE.DRINK_BLOOD
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override string GetToolTipText() {
        if (responsibleCharacter == null) {
            return description;
        }
        return "This character has been knocked out by " + responsibleCharacter.name;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if(sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            //CheckToApplyRestrainJob();
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.CancelAllJobsAndPlans();
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            if(gainedFromDoing == null || gainedFromDoing.poiTarget != _sourceCharacter) {
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                if(gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC) {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                    addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                }
            }
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        //if (_restrainJob != null) {
        //    _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        //}
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.RESTRAIN, removedBy); //so that the character that restrained him will not cancel his job.
        _sourceCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && characterThatWillDoJob.isAtHomeArea && targetCharacter.GetTraitOf(TRAIT_TYPE.CRIMINAL) == null && CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
                if (!targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name)) {
                    if (CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect);
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
            if (characterThatWillDoJob.isAtHomeArea && characterThatWillDoJob.faction == characterThatWillDoJob.homeArea.owner && !targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction
                && (characterThatWillDoJob.role.roleType == CHARACTER_ROLE.SOLDIER || characterThatWillDoJob.role.roleType == CHARACTER_ROLE.CIVILIAN || characterThatWillDoJob.role.roleType == CHARACTER_ROLE.ADVENTURER)) {
                if (!characterThatWillDoJob.HasTraitOf(TRAIT_TYPE.CRIMINAL) && targetCharacter.GetNormalTrait("Restrained") == null && characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE
                    && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.RESTRAIN)) {
                    if (CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, null)) {
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = characterThatWillDoJob.specificLocation, targetPOI = targetCharacter });
                        job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                        //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
