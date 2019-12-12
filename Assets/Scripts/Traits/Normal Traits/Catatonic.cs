using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Catatonic : Trait {

        public Character owner { get; private set; }
        public List<Character> charactersThatKnow { get; private set; }

        public Catatonic() {
            name = "Catatonic";
            description = "This character is catatonic.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.ticksPerDay;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            charactersThatKnow = new List<Character>();
            hindersMovement = true;
            hindersWitness = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
                owner.AdjustMoodValue(-15, this);
                owner.needsComponent.AdjustDoNotGetLonely(1);
                Messenger.AddListener(Signals.TICK_STARTED, CheckTrait);
                Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                owner.needsComponent.AdjustDoNotGetLonely(1);
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckTrait);
                Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead) {
                    if (targetCharacter.faction != characterThatWillDoJob.faction) {
                        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Restrained") == null) {
                            GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                            if (currentJob == null) {
                                if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter)) {
                                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, characterThatWillDoJob);
                                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.currentArea.prison });
                                    //job.SetCanBeDoneInLocation(true);
                                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                                    return true;
                                }
                            } 
                            //else {
                            //    if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                            //        TryTransferJob(currentJob, characterThatWillDoJob);
                            //    }
                            //}
                        }
                    } else {
                        if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE) {
                            if (owner.CanPlanGoap() && !owner.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
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
        #endregion

        public void AddCharacterThatKnows(Character character) {
            if (!charactersThatKnow.Contains(character)) {
                charactersThatKnow.Add(character);
            }
        }

        private void CheckTrait() {
            if (!owner.CanPlanGoap()) {
                return;
            }
            if (!owner.IsInOwnParty()) {
                return;
            }
            if (owner.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
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
        private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
            if (action.goapType == INTERACTION_TYPE.DROP && character.currentActionNode.poiTarget == owner) {
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
            if (characterThatWillDoJob.currentArea.region.IsResident(owner)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET };
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, owner, characterThatWillDoJob);
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
                        return CreateActualDropJob(characterThatWillDoJob, owner.homeStructure, bed.gridTileLocation);
                    }
                }
            }
            return false;
        }
        private bool PlanTirednessRecovery() {
            if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargettingThis(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
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
    }

    public class SaveDataCatatonic : SaveDataTrait {
        public List<int> charactersThatKnow;

        public override void Save(Trait trait) {
            base.Save(trait);
            Catatonic catatonic = trait as Catatonic;
            charactersThatKnow = new List<int>();
            for (int i = 0; i < catatonic.charactersThatKnow.Count; i++) {
                charactersThatKnow.Add(catatonic.charactersThatKnow[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Catatonic catatonic = trait as Catatonic;
            for (int i = 0; i < charactersThatKnow.Count; i++) {
                catatonic.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
            }
            return trait;
        }
    }
}
