using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Catatonic : Trait {

        public Character owner { get; private set; }

        private float chanceToRemove;
        private const int MaxDays = 4;
        
        public Catatonic() {
            name = "Catatonic";
            description = "This character is catatonic.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            // ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            hindersMovement = true;
            hindersWitness = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
                owner.moodComponent.AdjustMoodValue(-15, this);
                owner.needsComponent.AdjustDoNotGetLonely(1);
                Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                owner.needsComponent.AdjustDoNotGetLonely(1);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRemovalChance);
                Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead) {
                    if (characterThatWillDoJob.faction != targetCharacter.faction) {
                        GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                        if (currentJob == null) {
                            if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter)) {
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.RESTRAIN_CHARACTER, targetCharacter, characterThatWillDoJob);
                                //job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.currentArea.prison });
                                //job.SetCanBeDoneInLocation(true);
                                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                                return true;
                            }
                        }
                    } else {
                        if (characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE) {
                            if (owner.CanPlanGoap() && !owner.HasJobTargetingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                                if (!PlanFullnessRecovery(characterThatWillDoJob)) {
                                    CreateDropJobForTirednessRecovery(characterThatWillDoJob);
                                }
                            }
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckTrait();
        }
        #endregion

        private void CheckTrait() {
            if (!owner.CanPlanGoap()) {
                return;
            }
            if (!owner.IsInOwnParty()) {
                return;
            }
            if (owner.HasJobTargetingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                return;
            }
            if (owner.jobQueue.jobsInQueue.Count > 0) {
                owner.PerformTopPriorityJob();
            } else {
                PlanTirednessRecovery();
            }
        }

        #region Carry/Drop
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, owner, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, owner, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private void OnCharacterFinishedAction(ActualGoapNode node) {
            if (node.action.goapType == INTERACTION_TYPE.DROP && node.poiTarget == owner) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
                }
            }
        }
        #endregion

        #region Fullness Recovery
        private bool PlanFullnessRecovery(Character characterThatWillDoJob) {
            if (owner.needsComponent.isStarving || owner.needsComponent.isHungry) {
                return CreateFeedJob(characterThatWillDoJob);
            }
            return false;
        }
        private bool CreateFeedJob(Character characterThatWillDoJob) {
            if (characterThatWillDoJob.currentRegion.IsResident(owner)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET };
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, owner, characterThatWillDoJob);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 20 });
                //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeParalyzedFeedJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                return true;
            }
            return false;
        }
        #endregion

        #region Tiredness Recovery
        private bool CreateDropJobForTirednessRecovery(Character characterThatWillDoJob) {
            if (owner.needsComponent.isExhausted || owner.needsComponent.isTired) {
                if (owner.homeStructure != null && (owner.gridTileLocation.objHere == null || !(owner.gridTileLocation.objHere is Bed))) {
                    TileObject bed = owner.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                    if (bed != null) {
                        return CreateActualDropJob(characterThatWillDoJob, owner.homeStructure.GetLocationStructure(), bed.gridTileLocation);
                    }
                }
            }
            return false;
        }
        private bool PlanTirednessRecovery() {
            if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                return CreateSleepJob();
            }
            return false;
        }
        private bool CreateSleepJob() {
            if (owner.homeStructure != null) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
                    return true;
                }
                //else {
                //    TileObject bed = character.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                //    if(bed != null){
                //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure, bed.gridTileLocation);
                //    }
                //}
            }
            return false;
        }
        private void CreateActualSleepJob(Bed bed) {
            JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
            if (owner.needsComponent.isExhausted) {
                jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
            }
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
                job.AddOtherData(INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
               spooked.TriggerFeelingSpooked();
            }
        }
        #endregion

        #region Removal
        private void CheckRemovalChance() {
            chanceToRemove += GetChanceIncreasePerHour();
            float roll = Random.Range(0f, 100f);
            if (roll <= chanceToRemove) {
                owner.traitContainer.RemoveTrait(owner, this);
            }
        }
        private float GetChanceIncreasePerHour() {
            return 100f / (MaxDays * 24f);
        }
        #endregion
    }

    //public class SaveDataCatatonic : SaveDataTrait {
    //    public List<int> charactersThatKnow;

    //    public override void Save(Trait trait) {
    //        base.Save(trait);
    //        Catatonic catatonic = trait as Catatonic;
    //        charactersThatKnow = new List<int>();
    //        for (int i = 0; i < catatonic.charactersThatKnow.Count; i++) {
    //            charactersThatKnow.Add(catatonic.charactersThatKnow[i].id);
    //        }
    //    }

    //    public override Trait Load(ref Character responsibleCharacter) {
    //        Trait trait = base.Load(ref responsibleCharacter);
    //        Catatonic catatonic = trait as Catatonic;
    //        for (int i = 0; i < charactersThatKnow.Count; i++) {
    //            catatonic.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
    //        }
    //        return trait;
    //    }
    //}
}
