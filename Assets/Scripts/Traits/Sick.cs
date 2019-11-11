using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sick : Trait {
    private Character owner;
    private float pukeChance;
    private CharacterState pausedState;
    //private GoapPlanJob _removeTraitJob;
    public override bool isRemovedOnSwitchAlterEgo {
        get { return true; }
    }
    public Sick() {
        name = "Sick";
        description = "This character has caught a mild illness.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 480;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
        mutuallyExclusive = new string[] { "Robust" };
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            owner = sourceCharacter as Character;
            owner.AdjustSpeedModifier(-0.10f);
            //_sourceCharacter.CreateRemoveTraitJob(name);
            owner.AddTraitNeededToBeRemoved(this);
            if (gainedFromDoing == null) {
                owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                if (gainedFromDoing.goapType == INTERACTION_TYPE.EAT_AT_TABLE) {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                    addLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    owner.RegisterLogAndShowNotifToThisCharacterOnly(addLog, gainedFromDoing, false);
                    //gainedFromDoing.states["Eat Poisoned"].AddArrangedLog("sick", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, owner, true));
                } else {
                    owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                }
            }
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        owner.AdjustSpeedModifier(0.10f);
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        owner.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
        owner.RemoveTraitNeededToBeRemoved(this);
        owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
        //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
    }
    //public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
    //    if (traitOwner is Character) {
    //        Character targetCharacter = traitOwner as Character;
    //        if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
    //            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
    //            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
    //                new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM_GOAP, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
    //            job.SetCanBeDoneInLocation(true);
    //            if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, targetCharacter, job)) {
    //                //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
    //                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
    //                return true;
    //            } else {
    //                if (!IsResponsibleForTrait(characterThatWillDoJob)) {
    //                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
    //                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
    //                }
    //                return false;
    //            }
    //        }
    //    }
    //    return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    //}
    protected override void OnChangeLevel() {
        if (level == 1) {
            pukeChance = 5f;
        } else if (level == 2) {
            pukeChance = 7f;
        } else {
            pukeChance = 9f;
        }
    }
    public override bool PerTickOwnerMovement() {
        //string summary = owner.name + " is rolling for plagued chances....";
        float pukeRoll = Random.Range(0f, 100f);
        //summary += "\nPuke roll is: " + pukeRoll.ToString();
        //summary += "\nSeptic Shock roll is: " + septicRoll.ToString();
        bool hasCreatedJob = false;
        if (pukeRoll < pukeChance) {
            //summary += "\nPuke chance met. Doing puke action.";
            //do puke action
            if (owner.currentActionNode != null && owner.currentActionNode.goapType != INTERACTION_TYPE.PUKE) {
                //If current action is a roaming action like Hunting To Drink Blood, we must requeue the job after it is removed by StopCurrentAction
                JobQueueItem currentJob = null;
                JobQueue currentJobQueue = null;
                if (owner.currentActionNode.isRoamingAction && owner.currentActionNode.parentPlan != null && owner.currentActionNode.parentPlan.job != null) {
                    currentJob = owner.currentActionNode.parentPlan.job;
                    currentJobQueue = currentJob.jobQueueParent;
                }
                owner.StopCurrentAction(false);
                if (currentJob != null) {
                    currentJobQueue.AddJobInQueue(currentJob, false);
                }
                owner.marker.StopMovement();

                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.jobQueue.AddJobInQueue(job, false);

                owner.SetCurrentActionNode(goapAction);
                //owner.currentAction.SetEndAction(ResumeLastAction);
                owner.currentActionNode.DoAction();
                hasCreatedJob = true;
            } else if (owner.stateComponent.currentState != null) {
                pausedState = owner.stateComponent.currentState;
                owner.stateComponent.currentState.PauseState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.SetCurrentActionNode(goapAction);
                owner.currentActionNode.SetEndAction(ResumePausedState);
                owner.jobQueue.AddJobInQueue(job, false);

                owner.currentActionNode.DoAction();
                hasCreatedJob = true;
            } else {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.SetCurrentActionNode(goapAction);
                owner.jobQueue.AddJobInQueue(job, false);

                owner.currentActionNode.DoAction();
                hasCreatedJob = true;
            }
            //Debug.Log(summary);
        }
        return hasCreatedJob;
    }
    #endregion

    private void ResumePausedState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        pausedState.ResumeState();
    }
}
