using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Paralyzed : Trait {

        public Character character { get; private set; }
        public List<Character> charactersThatKnow { get; private set; }

        public Paralyzed() {
            name = "Paralyzed";
            description = "This character is paralyzed.";
            thoughtText = "[Character] is paralyzed.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            ticksDuration = 0;
            charactersThatKnow = new List<Character>();
            hindersMovement = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                character = addedTo as Character;
                character.CancelAllJobs();
                Messenger.AddListener(Signals.TICK_STARTED, CheckParalyzedTrait);
                Messenger.AddListener(Signals.HOUR_STARTED, CheckParalyzedTraitPerHour);
                Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (character != null) {
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckParalyzedTrait);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckParalyzedTraitPerHour);
                Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead) {
                    if (targetCharacter.faction != characterThatWillDoJob.faction) {
                        if (targetCharacter.traitContainer.GetNormalTrait("Restrained") == null) {
                            GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                            if (currentJob == null) {
                                if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter)) {
                                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, characterThatWillDoJob);
                                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.specificLocation.prison });
                                    //job.SetCanBeDoneInLocation(true);
                                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                                    return true;
                                }
                            } 
                            //else {
                            //    if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                            //        return TryTransferJob(currentJob, characterThatWillDoJob);
                            //    }
                            //}
                        }
                    } else {
                        if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.NEGATIVE) {
                            if (character.CanPlanGoap() && !character.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                                if (!PlanFullnessRecovery(characterThatWillDoJob)) {
                                    if (!CreateDropJobForTirednessRecovery(characterThatWillDoJob)) {
                                        CreateDropJobForHappinessRecovery(characterThatWillDoJob);
                                    }
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

        private void CheckParalyzedTrait() {
            if (!character.CanPlanGoap()) {
                return;
            }
            if (!character.IsInOwnParty()) {
                return;
            }
            if (character.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                return;
            }
            if (character.jobQueue.jobsInQueue.Count > 0) {
                character.PerformTopPriorityJob();
            } else {
                if (!PlanTirednessRecovery()) {
                    PlanHappinessRecovery();
                }
            }
        }
        private void CheckParalyzedTraitPerHour() {
            if (character.IsInOwnParty()) {
                if (character.CanPlanGoap() && character.stateComponent.currentState == null
                    && (character.needsComponent.isStarving || character.needsComponent.isExhausted || character.needsComponent.isForlorn)
                    && UnityEngine.Random.Range(0, 100) < 75 && !character.jobQueue.HasJob(JOB_TYPE.SCREAM)
                    && character.traitContainer.GetNormalTrait("Unconscious", "Resting") == null && !character.HasJobTargettingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, character, character);
                    character.jobQueue.AddJobInQueue(job);
                }
            }
        }

        #region Carry/Drop
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, character, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DROP, INTERACTION_TYPE.DROP, character, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
            //TODO:
            //if (action.goapType == INTERACTION_TYPE.DROP && action.poiTarget == this.character) {
            //    if (this.character.gridTileLocation.objHere != null && this.character.gridTileLocation.objHere is Bed) {
            //        CreateActualSleepJob(this.character.gridTileLocation.objHere as Bed);
            //    } else if (this.character.gridTileLocation.structure == this.character.homeStructure) {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            //    }
            //}
        }
        #endregion

        #region Fullness Recovery
        private bool PlanFullnessRecovery(Character characterThatWillDoJob) {
            if (character.needsComponent.isStarving || character.needsComponent.isHungry) {
                return CreateFeedJob(characterThatWillDoJob);
            }
            return false;
        }
        private bool CreateFeedJob(Character characterThatWillDoJob) {
            if (characterThatWillDoJob.specificLocation.region.IsResident(character)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, characterThatWillDoJob, characterThatWillDoJob);
                //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeParalyzedFeedJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                return true;
            }
            return false;
        }
        #endregion

        #region Happiness Recovery
        private bool CreateDropJobForHappinessRecovery(Character characterThatWillDoJob) {
            if (character.needsComponent.isForlorn || character.needsComponent.isLonely) {
                if ((character.homeStructure != null && character.currentStructure != character.homeStructure) &&
                    (character.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA) && character.currentStructure.structureType != STRUCTURE_TYPE.WORK_AREA)) {
                    int chance = UnityEngine.Random.Range(0, 2);
                    if (chance == 0) {
                        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure);
                    } else {
                        return CreateActualDropJob(characterThatWillDoJob, character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                    }
                }
            }
            return false;
        }
        private bool PlanHappinessRecovery() {
            if ((character.needsComponent.isForlorn || character.needsComponent.isLonely) && !character.HasJobTargettingThis(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                return CreateDaydreamOrPrayJob();
            }
            return false;
        }
        private bool CreateDaydreamOrPrayJob() {
            if (character.specificLocation.region.IsResident(character)) {
                if (character.homeStructure != null && character.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                    if (character.currentStructure == character.homeStructure) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else if (character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                } else {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CreatePrayJob() {
            if (character.homeStructure == null || character.currentStructure == character.homeStructure) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                return true;
            }
            //else {
            //    if (character.homeStructure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            //        return true;
            //    }
            //}
            return false;
        }
        private bool CreateDaydreamJob() {
            if (character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA || !character.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                return true;
            }
            //else {
            //    LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
            //    if (structure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, structure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            //        return true;
            //    }
            //}
            return false;
        }
        private void CreateActualHappinessRecoveryJob(INTERACTION_TYPE actionType) {
            JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
            if (character.needsComponent.isForlorn) {
                jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
            }
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = character.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, actionType, character, character);
                job.AddOtherData(actionType, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                character.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
        }
        #endregion

        #region Tiredness Recovery
        private bool CreateDropJobForTirednessRecovery(Character characterThatWillDoJob) {
            if (character.needsComponent.isExhausted || character.needsComponent.isTired) {
                if (character.homeStructure != null && (character.gridTileLocation.objHere == null || !(character.gridTileLocation.objHere is Bed))) {
                    TileObject bed = character.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                    if (bed != null) {
                        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure, bed.gridTileLocation);
                    }
                }
            }
            return false;
        }
        private bool PlanTirednessRecovery() {
            if ((character.needsComponent.isExhausted || character.needsComponent.isTired) && !character.HasJobTargettingThis(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                return CreateSleepJob();
            }
            return false;
        }
        private bool CreateSleepJob() {
            if (character.homeStructure != null) {
                if (character.gridTileLocation.objHere != null && character.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(character.gridTileLocation.objHere as Bed);
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
            if (character.needsComponent.isExhausted) {
                jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
            }
            bool triggerSpooked = false;
            Spooked spooked = character.traitContainer.GetNormalTrait("Spooked") as Spooked;
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, character);
                job.AddOtherData(INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                character.jobQueue.AddJobInQueue(job);
            } else {
                spooked.TriggerFeelingSpooked();
            }
        }
        #endregion
    }


    public class SaveDataParalyzed : SaveDataTrait {
        public List<int> charactersThatKnow;

        public override void Save(Trait trait) {
            base.Save(trait);
            Paralyzed paralyzed = trait as Paralyzed;
            charactersThatKnow = new List<int>();
            for (int i = 0; i < paralyzed.charactersThatKnow.Count; i++) {
                charactersThatKnow.Add(paralyzed.charactersThatKnow[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Paralyzed paralyzed = trait as Paralyzed;
            for (int i = 0; i < charactersThatKnow.Count; i++) {
                paralyzed.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
            }
            return trait;
        }
    }
}
