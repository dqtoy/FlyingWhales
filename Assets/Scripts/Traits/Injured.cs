using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Injured : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _removeTraitJob;

    #region getters/setters
    public override bool broadcastDuplicates {
        get { return true; }
    }
    public override bool isRemovedOnSwitchAlterEgo {
        get { return true; }
    }
    #endregion

    public Injured() {
        name = "Injured";
        description = "This character is badly hurt.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 480;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER, };
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            _sourceCharacter.UpdateIsCombatantState();
            _sourceCharacter.AdjustSpeedModifier(-0.15f);
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            if (gainedFromDoing == null || gainedFromDoing.poiTarget != _sourceCharacter) {
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                    addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    gainedFromDoing.states["Target Injured"].AddArrangedLog("injured", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                }
            }
            //Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _sourceCharacter);
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        _sourceCharacter.UpdateIsCombatantState();
        _sourceCharacter.AdjustSpeedModifier(0.15f);
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        //_sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                SerialKiller serialKiller = characterThatWillDoJob.GetNormalTrait("Serial Killer") as SerialKiller;
                if (serialKiller != null) {
                    serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                    return false;
                }
                GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
                if(currentJob == null) {
                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                        new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                    job.SetCanBeDoneInLocation(true);
                    if (InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, job)) {
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    } else {
                        if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob);
                            characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                        }
                        return false;
                    }
                } else {
                    if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                        bool canBeTransfered = false;
                        if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentAction != null
                            && currentJob.assignedCharacter.currentAction.parentPlan != null && currentJob.assignedCharacter.currentAction.parentPlan.job == currentJob) {
                            if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentAction.poiTarget);
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
