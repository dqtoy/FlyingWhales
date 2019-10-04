using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralyzed : Trait {

    public Character character { get; private set; }
    public Paralyzed() {
        name = "Paralyzed";
        description = "This character is paralyzed.";
        thoughtText = "[Character] is paralyzed.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, };
        daysDuration = 0;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if (addedTo is Character) {
            character = addedTo as Character;
            character.CancelAllJobsAndPlans();
            Messenger.AddListener(Signals.TICK_STARTED, CheckParalyzedTrait);
            Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        if (character != null) {
            Messenger.RemoveListener(Signals.TICK_STARTED, CheckParalyzedTrait);
            Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction && targetCharacter.GetNormalTrait("Restrained") == null) {
                GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                if(currentJob == null) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.DROP_CHARACTER, targetCharacter);
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                    job.SetCanBeDoneInLocation(true);
                    if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, job)) {
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    } else {
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
                        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                        return false;
                    }
                } else {
                    if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                        bool canBeTransfered = false;
                        if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentAction != null
                            && currentJob.assignedCharacter.currentAction.parentPlan != null && currentJob.assignedCharacter.currentAction.parentPlan.job == currentJob) {
                            canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentAction.poiTarget);
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

    private void CheckParalyzedTrait() {
        if (!character.CanPlanGoap()) {
            return;
        }
        if (!character.IsInOwnParty()) {
            return;
        }
        if (character.HasJobTargettingThis(JOB_TYPE.DROP) || character.HasJobTargettingThis(JOB_TYPE.FEED)) {
            return;
        }
        if(character.allGoapPlans.Count > 0) {
            character.PerformGoapPlans();
        } else {
            if (!PlanFullnessRecovery()) {
                if (!PlanTirednessRecovery()) {
                    PlanHappinessRecovery();
                }
            }
        }
    }

    #region Carry/Drop
    private bool CreateActualDropJob(LocationStructure dropLocationStructure) {
        if (!character.HasJobTargettingThis(JOB_TYPE.DROP) && !character.HasJobTargettingThis(JOB_TYPE.FEED)) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, character,
                new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.DROP, new object[] { dropLocationStructure }
                } });
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            character.specificLocation.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    private bool CreateActualDropJob(LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
        if (!character.HasJobTargettingThis(JOB_TYPE.DROP) && !character.HasJobTargettingThis(JOB_TYPE.FEED)) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, character,
                new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile }
                } });
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            character.specificLocation.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if(action.goapType == INTERACTION_TYPE.DROP && action.poiTarget == this.character) {
            if(this.character.gridTileLocation.objHere != null && this.character.gridTileLocation.objHere is Bed) {
                CreateActualSleepJob(this.character.gridTileLocation.objHere as Bed);
            }else if(this.character.gridTileLocation.structure == this.character.homeStructure) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            } else {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            }
        }
    }
    #endregion

    #region Fullness Recovery
    private bool PlanFullnessRecovery() {
        if(character.isStarving || character.isHungry) {
            return CreateFeedJob();
        }
        return false;
    }
    private bool CreateFeedJob() {
        if (!character.HasJobTargettingThis(JOB_TYPE.FEED) && !character.HasJobTargettingThis(JOB_TYPE.DROP) && character.specificLocation.IsResident(character)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = character };
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, goapEffect);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeParalyzedFeedJob);
            character.specificLocation.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    #endregion

    #region Happiness Recovery
    private bool recentlyBeenMoved = false;
    private bool PlanHappinessRecovery() {
        if(character.isForlorn || character.isLonely) {
            return CreateDaydreamOrPrayJob();
            //int chance = UnityEngine.Random.Range(0, 2);
            //if(chance == 0 || recentlyBeenMoved) {
            //    recentlyBeenMoved = false;
            //    return CreateDaydreamOrPrayJob();
            //} else {
            //    //Ask to be moved to another place
            //    bool isAtHome = character.currentStructure == character.homeStructure;
            //    bool isAtWorkArea = character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA;

            //    List<LocationStructure> structureChoices = new List<LocationStructure>();
            //    if (!isAtHome && character.homeStructure != null) {
            //        structureChoices.Add(character.homeStructure);
            //    }
            //    if (!isAtWorkArea) {
            //        LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
            //        if(structure != null) {
            //            structureChoices.Add(structure);
            //        }
            //    }

            //    if(structureChoices.Count > 0) {
            //        recentlyBeenMoved = true;
            //        return CreateActualDropJob(structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)]);
            //    } else {
            //        return CreateDaydreamOrPrayJob();
            //    }
            //}
        }
        return false;
    }
    private bool CreateDaydreamOrPrayJob() {
        if (character.specificLocation.IsResident(character)) {
            int chance = UnityEngine.Random.Range(0, 2);
            if (chance == 0) {
                return CreatePrayJob();
            } else {
                return CreateDaydreamJob();
            }
            //if (character.currentStructure == character.homeStructure) {
            //    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            //} else {
            //    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            //}
            //return true;
        }
        return false;
    }
    private bool CreatePrayJob() {
        if (character.currentStructure == character.homeStructure) {
            CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            return true;
        } else {
            if (character.homeStructure != null) {
                return CreateActualDropJob(character.homeStructure);
            } else {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                return true;
            }
        }
    }
    private bool CreateDaydreamJob() {
        if (character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
            CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            return true;
        } else {
            LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
            if (structure != null) {
                return CreateActualDropJob(structure);
            } else {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                return true;
            }
        }
    }
    private void CreateActualHappinessRecoveryJob(INTERACTION_TYPE actionType) {
        JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
        if (character.isForlorn) {
            jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
        }
        GoapPlanJob job = new GoapPlanJob(jobType, actionType, character, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { actionType, new object[] { ACTION_LOCATION_TYPE.IN_PLACE } } });
        character.jobQueue.AddJobInQueue(job);
        character.jobQueue.AssignCharacterToJob(job, character);
    }
    #endregion

    #region Tiredness Recovery
    private bool PlanTirednessRecovery() {
        if (character.isExhausted || character.isTired) {
            return CreateSleepJob();
        }
        return false;
    }
    private bool CreateSleepJob() {
        if (character.homeStructure != null) {
            if(character.gridTileLocation.objHere != null && character.gridTileLocation.objHere is Bed) {
                CreateActualSleepJob(character.gridTileLocation.objHere as Bed);
                return true;
            } else {
                TileObject bed = character.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                if(bed != null){
                    return CreateActualDropJob(character.homeStructure, bed.gridTileLocation);
                }
            }
        }
        return false;
    }
    private void CreateActualSleepJob(Bed bed) {
        JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
        if (character.isExhausted) {
            jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
        }
        GoapPlanJob job = new GoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE } } });
        character.jobQueue.AddJobInQueue(job);
        character.jobQueue.AssignCharacterToJob(job, character);
    }
    #endregion
}
