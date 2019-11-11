using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _restrainJob;
    //private GoapPlanJob _removeTraitJob;
    public override bool isRemovedOnSwitchAlterEgo {
        get { return true; }
    }

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] is unconscious.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 24; //144
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER }; //, INTERACTION_TYPE.DRINK_BLOOD
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override string GetToolTipText() {
        if (responsibleCharacter == null) {
            return description;
        }
        return "This character has been knocked out by " + responsibleCharacter.name;
    }
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if(sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            if (_sourceCharacter.currentHP <= 0) {
                _sourceCharacter.SetHP(1);
            }
            //CheckToApplyRestrainJob();
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            if(gainedFromDoing == null || gainedFromDoing.poiTarget != _sourceCharacter) {
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly(addLog, gainedFromDoing, false);
                //if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
                //    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                //}else if (gainedFromDoing.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                //    gainedFromDoing.states["Knockout Success"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                //}
            }
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        //if (_restrainJob != null) {
        //    _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        //}
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        //_sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.RESTRAIN, removedBy); //so that the character that restrained him will not cancel his job.
        _sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy); //so that the character that cured him will not cancel his job.
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override void OnDeath(Character character) {
        base.OnDeath(character);
        character.RemoveTrait(this);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && targetCharacter.faction == characterThatWillDoJob.faction && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                SerialKiller serialKiller = characterThatWillDoJob.GetNormalTrait("Serial Killer") as SerialKiller;
                if (serialKiller != null) {
                    serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                    return false;
                }
                GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
                if (currentJob == null) {
                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                        new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                    //job.SetCanBeDoneInLocation(true);
                    if (InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, job)) {
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                    //else {
                    //    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                    //        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob);
                    //        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    //    }
                    //    return false;
                    //}
                } else {
                    if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                        bool canBeTransfered = false;
                        if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentActionNode != null
                            && currentJob.assignedCharacter.currentActionNode.parentPlan != null && currentJob.assignedCharacter.currentActionNode.parentPlan.job == currentJob) {
                            if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentActionNode.poiTarget);
                            }
                        } else {
                            canBeTransfered = true;
                        }
                        if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                            currentJob.jobQueueParent.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                            characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                            characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                            return true;
                        }
                    }
                }
            }
            if (!targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction && targetCharacter.GetNormalTrait("Restrained") == null) {
                GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                if (currentJob == null) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.IMPRISON_CHARACTER, targetCharacter);
                    //job.SetCanBeDoneInLocation(true);
                    if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, job)) {
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                    //else {
                    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
                    //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    //    return false;
                    //}
                } else {
                    if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                        bool canBeTransfered = false;
                        if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentActionNode != null
                            && currentJob.assignedCharacter.currentActionNode.parentPlan != null && currentJob.assignedCharacter.currentActionNode.parentPlan.job == currentJob) {
                            if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentActionNode.poiTarget);
                            }
                        } else {
                            canBeTransfered = true;
                        }
                        if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                            currentJob.jobQueueParent.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                            characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                            characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                            return true;
                        }
                    }
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}