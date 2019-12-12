using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    //This trait is present in all characters
    //A dummy trait in order for some jobs to be created
    public class CharacterTrait : Trait {
        //IMPORTANT NOTE: When the owner of this trait changed its alter ego, this trait will not be present in the alter ego anymore
        //Meaning that he/she cannot do the things specified in here anymore unless he/she switch to the ego which this trait is present
        public List<TileObject> alreadyInspectedTileObjects { get; private set; }
        public bool hasSurvivedApprehension { get; private set; } //If a criminal character (is in original alter ego), and survived being apprehended, this must be turned on
        public Character owner { get; private set; }

        public CharacterTrait() {
            name = "Character Trait";
            type = TRAIT_TYPE.PERSONALITY;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            ticksDuration = 0;
            isHidden = true;
            alreadyInspectedTileObjects = new List<TileObject>();
        }
        public void AddAlreadyInspectedObject(TileObject to) {
            if (!alreadyInspectedTileObjects.Contains(to)) {
                alreadyInspectedTileObjects.Add(to);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
        }

        #region Overrides
        public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOI(targetPOI, character);
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                Paralyzed paralyzed = targetCharacter.traitContainer.GetNormalTrait("Paralyzed") as Paralyzed;
                Catatonic catatonic = targetCharacter.traitContainer.GetNormalTrait("Catatonic") as Catatonic;
                if (paralyzed != null) {
                    paralyzed.AddCharacterThatKnows(character);
                }
                if (catatonic != null) {
                    catatonic.AddCharacterThatKnows(character);
                }
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Table) {
                Table targetTable = targetPOI as Table;
                if (targetTable.food < 20 && targetTable.structureLocation is Dwelling) {
                    Dwelling dwelling = targetTable.structureLocation as Dwelling;
                    if (dwelling.IsResident(characterThatWillDoJob)) {
                        if (!targetTable.HasJobTargettingThis(JOB_TYPE.PRODUCE_FOOD)) {
                            int neededFood = 60 - targetTable.food;
                            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.TARGET);
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, effect, targetTable, characterThatWillDoJob);
                            job.AddOtherData(INTERACTION_TYPE.DROP_FOOD, new object[] { neededFood });
                            job.AddOtherData(INTERACTION_TYPE.OBTAIN_RESOURCE, new object[] { neededFood });
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                    }
                }
            }
            if (targetPOI is TileObject) {
                TileObject tileObj = targetPOI as TileObject;
                if (tileObj.isSummonedByPlayer && characterThatWillDoJob.traitContainer.GetNormalTrait("Suspicious") == null && !alreadyInspectedTileObjects.Contains(tileObj)) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, tileObj)) {
                        GoapPlanJob inspectJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, tileObj, characterThatWillDoJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(inspectJob);
                        return true;
                    }
                } else if (tileObj is GoddessStatue) {
                    if (Random.Range(0, 100) < 15 && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.PRAY_GODDESS_STATUE, tileObj) && tileObj.state == POI_STATE.ACTIVE) {
                        GoapPlanJob prayJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRAY_GODDESS_STATUE, INTERACTION_TYPE.PRAY_TILE_OBJECT, tileObj, characterThatWillDoJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(prayJob);
                        return true;
                    }
                } 
                //else {
                //    if (tileObj.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
                //        GoapPlanJob buildJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_TILE_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, tileObj, characterThatWillDoJob);
                //        characterThatWillDoJob.jobQueue.AddJobInQueue(buildJob);
                //        return true;
                //    }
                //}
            }
            if (targetPOI is SpecialToken) {
                if (characterThatWillDoJob.role.roleType != CHARACTER_ROLE.BEAST) {
                    SpecialToken token = targetPOI as SpecialToken;
                    if (token.characterOwner == null) {
                        //Patrollers should not pick up items from their main storage structure
                        if (token.structureLocation != null && token.structureLocation == characterThatWillDoJob.homeArea.mainStorage
                            && token.currentArea.region == characterThatWillDoJob.homeRegion) {
                            return false;
                        }
                        //characters should not pick up items if that item is the target of it's current action
                        if (characterThatWillDoJob.currentActionNode != null && characterThatWillDoJob.currentActionNode.poiTarget == token) {
                            return false;
                        }
                        if (token.advertisedActions.Contains(INTERACTION_TYPE.PICK_UP) == false) {
                            return false;
                        }
                        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_UP)) {
                            GoapPlanJob pickUpJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_UP, token, characterThatWillDoJob);
                            characterThatWillDoJob.jobQueue.AddJobInQueue(pickUpJob);
                        }
                        return true;
                    }
                }
            }
            if (targetPOI is Character || targetPOI is Tombstone) {
                Character targetCharacter = null;
                if (targetPOI is Character) {
                    targetCharacter = targetPOI as Character;
                } else if (targetPOI is Tombstone) {
                    targetCharacter = (targetPOI as Tombstone).character;
                }
                if (targetCharacter.isDead) {
                    Dead deadTrait = targetCharacter.traitContainer.GetNormalTrait("Dead") as Dead;
                    if (deadTrait.responsibleCharacter != characterThatWillDoJob && !deadTrait.charactersThatSawThisDead.Contains(characterThatWillDoJob)) {
                        deadTrait.AddCharacterThatSawThisDead(characterThatWillDoJob);

                        Log sawDeadLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "saw_dead");
                        sawDeadLog.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        sawDeadLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        characterThatWillDoJob.AddHistory(sawDeadLog);
                        PlayerManager.Instance.player.ShowNotificationFrom(sawDeadLog, characterThatWillDoJob, false);


                        if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
                            characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Heartbroken");
                            bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                            characterThatWillDoJob.needsComponent.AdjustHappiness(-6000);
                            return hasCreatedJob;
                        } else if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
                            characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
                            bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                            characterThatWillDoJob.needsComponent.AdjustHappiness(-4000);
                            return hasCreatedJob;
                        } else if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
                            characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
                            bool hasCreatedJob = CreatePrioritizedShockJob(characterThatWillDoJob);
                            characterThatWillDoJob.needsComponent.AdjustHappiness(-2000);
                            return hasCreatedJob;
                        }
                    }
                } else { //character is not dead
                         //When someone sees another character carrying a character that is unconscious or restrained, and that character is not a branded criminal, and not the carrier's lover or relative, the carrier will be branded as Assaulter and Crime Handling will take place.
                         //if (targetCharacter.IsInOwnParty() && targetCharacter.currentParty.characters.Count > 1 && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) { //This means that this character is carrrying another character
                         //    Character carriedCharacter = targetCharacter.currentParty.characters[1];
                         //    if (characterThatWillDoJob != carriedCharacter && !carriedCharacter.isDead && carriedcharacter.traitContainer.GetNormalTrait("Unconscious", "Restrained") != null) {
                         //        if (!targetCharacter.HasRelationshipOfTypeWith(carriedCharacter, false, RELATIONSHIP_TRAIT.RELATIVE, RELATIONSHIP_TRAIT.LOVER)) {
                         //            if (targetCharacter.currentActionNode.action != null && !targetCharacter.currentActionNode.action.hasCrimeBeenReported) {
                         //                characterThatWillDoJob.ReactToCrime(CRIME.ASSAULT, targetCharacter.currentActionNode.action, targetCharacter.currentAlterEgo, SHARE_INTEL_STATUS.WITNESSED);
                         //            }
                         //        }
                         //    }
                         //}

                    #region Check Up
                    //If a character cannot assist a character in vision, they may stay with it and check up on it for a bit. Reference: https://trello.com/c/hW7y6d5W/2841-if-a-character-cannot-assist-a-character-in-vision-they-may-stay-with-it-and-check-up-on-it-for-a-bit
                    //If they are enemies and the character in vision has any of the following:
                    //- unconscious, catatonic, restrained, puked, stumbled
                    ///NOTE: Puke and Stumble Reactions can be found at <see cref="Puke.SuccessReactions(Character, Intel, SHARE_INTEL_STATUS)"/> and <see cref="Stumble.SuccessReactions(Character, Intel, SHARE_INTEL_STATUS)"/> respectively
                    //They will trigger a personal https://trello.com/c/uCbLBXsF/2846-character-laugh-at job
                    if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY) && targetCharacter.traitContainer.GetNormalTrait("Unconscious", "Catatonic", "Restrained") != null && characterThatWillDoJob.faction == targetCharacter.faction
                        && (characterThatWillDoJob.currentActionNode == null || characterThatWillDoJob.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)) {
                        return CreateLaughAtJob(characterThatWillDoJob, targetCharacter);
                    }

                    //If they have a positive relationship but the character cannot perform the necessary job to remove the following traits:
                    //catatonic, unconscious, restrained, puked
                    ///NOTE: Puke Reactions can be found at <see cref="Puke.SuccessReactions(Character, Intel, SHARE_INTEL_STATUS)"/>
                    //They will trigger a personal https://trello.com/c/iDsfwQ7d/2845-character-feeling-concerned job
                    else if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE && targetCharacter.traitContainer.GetNormalTrait("Unconscious", "Catatonic", "Restrained") != null
                        && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.REMOVE_TRAIT, targetCharacter) && characterThatWillDoJob.faction == targetCharacter.faction
                         && (characterThatWillDoJob.currentActionNode == null || characterThatWillDoJob.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)) {
                        return CreateFeelingConcernedJob(characterThatWillDoJob, targetCharacter);
                    }
                    #endregion
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
        private void CheckAsCriminal() {
            if (owner.stateComponent.currentState == null && !owner.isAtHomeRegion && !owner.jobQueue.HasJob(JOB_TYPE.RETURN_HOME)) {
                if (owner.jobQueue.jobsInQueue.Count > 0) {
                    owner.CancelAllJobs();
                }
                CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.RETURN_HOME, CHARACTER_STATE.MOVE_OUT, owner);
                owner.jobQueue.AddJobInQueue(job);
            } else if (owner.isAtHomeRegion) {
                SetHasSurvivedApprehension(false);
            }
        }

        public void SetHasSurvivedApprehension(bool state) {
            if (hasSurvivedApprehension != state) {
                hasSurvivedApprehension = state;
                if (hasSurvivedApprehension) {
                    Messenger.AddListener(Signals.TICK_STARTED, CheckAsCriminal);
                } else {
                    if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
                        Messenger.RemoveListener(Signals.TICK_STARTED, CheckAsCriminal);
                    }
                }
            }
        }

        private bool RandomizeBetweenShockAndCryJob(Character characterThatWillDoJob) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                return CreatePrioritizedShockJob(characterThatWillDoJob);
            } else {
                return CreatePrioritizedCryJob(characterThatWillDoJob);
            }
        }
        private bool CreatePrioritizedShockJob(Character characterThatWillDoJob) {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.SHOCK)) {
                GoapPlanJob shockJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.SHOCK, characterThatWillDoJob, characterThatWillDoJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(shockJob);
                return true;
            }
            return false;
        }
        private bool CreatePrioritizedCryJob(Character characterThatWillDoJob) {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.CRY)) {
                GoapPlanJob cryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.CRY, characterThatWillDoJob, characterThatWillDoJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(cryJob);
                return true;
            }
            return false;
        }
        private bool CreateLaughAtJob(Character characterThatWillDoJob, Character target) {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT)) {
                GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT, target, characterThatWillDoJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
                return true;
            }
            return false;
        }
        private bool CreateFeelingConcernedJob(Character characterThatWillDoJob, Character target) {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED)) {
                GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED, target, characterThatWillDoJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
                return true;
            }
            return false;
        }
    }

    public class SaveDataCharacterTrait : SaveDataTrait {
        public List<TileObjectSerializableData> alreadyInspectedTileObjects;
        public bool hasSurvivedApprehension;

        public override void Save(Trait trait) {
            base.Save(trait);
            alreadyInspectedTileObjects = new List<TileObjectSerializableData>();
            CharacterTrait derivedTrait = trait as CharacterTrait;
            for (int i = 0; i < derivedTrait.alreadyInspectedTileObjects.Count; i++) {
                TileObject to = derivedTrait.alreadyInspectedTileObjects[i];
                TileObjectSerializableData toData = new TileObjectSerializableData {
                    id = to.id,
                    type = to.tileObjectType,
                };
                alreadyInspectedTileObjects.Add(toData);
            }
            hasSurvivedApprehension = derivedTrait.hasSurvivedApprehension;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            CharacterTrait derivedTrait = trait as CharacterTrait;
            for (int i = 0; i < alreadyInspectedTileObjects.Count; i++) {
                TileObjectSerializableData toData = alreadyInspectedTileObjects[i];
                derivedTrait.AddAlreadyInspectedObject(InnerMapManager.Instance.GetTileObject(toData.type, toData.id));
            }
            derivedTrait.SetHasSurvivedApprehension(hasSurvivedApprehension);
            return trait;
        }
    }
}

