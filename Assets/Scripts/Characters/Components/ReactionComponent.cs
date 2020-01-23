using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Traits;
using Inner_Maps;

public class ReactionComponent {
    public Character owner { get; private set; }

    public ReactionComponent(Character owner) {
        this.owner = owner;
    }

    #region Processes
    public void ReactTo(IPointOfInterest targetPOI, ref string debugLog) {
        if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            ReactTo(targetPOI as Character, ref debugLog);
        } else if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            ReactTo(targetPOI as TileObject, ref debugLog);
        } else if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            ReactTo(targetPOI as SpecialToken, ref debugLog);
        }
    }
    public string ReactTo(ActualGoapNode node, SHARE_INTEL_STATUS status) {
        if(status == SHARE_INTEL_STATUS.WITNESSED) {
            ReactToWitnessedAction(node);
        } else {
            return ReactToInformedAction(node);
        }
        return string.Empty;
    }
    private void ReactToWitnessedAction(ActualGoapNode node) {
        if (owner.faction != node.actor.faction && owner.faction.IsHostileWith(node.actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        //if (witnessedEvent.currentStateName == null) {
        //    throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        //}
        if (node.descriptionLog == null) {
            throw new Exception(GameManager.Instance.TodayLogString() + owner.name + " witnessed event " + node.action.goapName + " by " + node.actor.name + " with state " + node.currentStateName + " but it does not have a description log!");
        }

        if(node.actor != owner && node.poiTarget != owner) {
            Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", node);
            witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.OTHER);
            witnessLog.AddToFillers(null, Utilities.LogDontReplace(node.descriptionLog), LOG_IDENTIFIER.APPEND);
            witnessLog.AddToFillers(node.descriptionLog.fillers);
            owner.AddHistory(witnessLog);

            node.action.ReactionToActor(owner, node);
            node.action.ReactionToTarget(owner, node);
        } else if (node.poiTarget == owner) {
            node.action.ReactionOfTarget(node);
        }

        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private string ReactToInformedAction(ActualGoapNode node) {
        if (node.descriptionLog == null) {
            throw new Exception(GameManager.Instance.TodayLogString() + owner.name + " informed event " + node.action.goapName + " by " + node.actor.name + " with state " + node.currentStateName + " but it does not have a description log!");
        }
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", node);
        informedLog.AddToFillers(node.descriptionLog.fillers);
        informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.OTHER);
        informedLog.AddToFillers(null, Utilities.LogDontReplace(node.descriptionLog), LOG_IDENTIFIER.APPEND);
        owner.AddHistory(informedLog);

        if (node.actor != owner && node.poiTarget != owner) {
            string response = string.Empty;
            response += node.action.ReactionToActor(owner, node);
            response += " " + node.action.ReactionToTarget(owner, node);
        } else if(node.poiTarget == owner && node.poiTarget is Character) {
            return node.action.ReactionOfTarget(node);
        } else if (node.actor == owner) {
            return "I know what I did.";
        }
        return string.Empty;
        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private void ReactTo(Character targetCharacter, ref string debugLog) {
        debugLog += owner.name + " is reacting to " + targetCharacter.name;
        if (!IsPOICurrentlyTargetedByAPerformingAction(targetCharacter)) {
            debugLog += "\n-Target is not being targeted by an action, continue reaction";
            if(owner.faction != targetCharacter.faction && owner.faction.GetRelationshipWith(targetCharacter.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                debugLog += "\n-Target is hostile, will trigger Fight or Flight response";
                //Fight or Flight
            } else {
                if(!owner.isConversing && !targetCharacter.isConversing && owner.nonActionEventsComponent.CanInteract(targetCharacter)) {
                    debugLog += "\n-Character and Target are not Chatting or Flirting and Character can interact with Target, has 3% chance to Chat";
                    int chance = UnityEngine.Random.Range(0, 100);
                    debugLog += "\n-Roll: " + chance;
                    if (chance < 3) {
                        debugLog += "\n-Chat triggered";
                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
                    } else {
                        debugLog += "\n-Chat did not trigger, will now trigger Flirt if Character is Unfaithful, or Target is Lover or Affair, or Character has no Lover";
                        if (owner.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.PARAMOUR)
                            || owner.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.LOVER) == null
                            || owner.traitContainer.GetNormalTrait<Trait>("Unfaithful") != null) {
                            debugLog += "\n-Flirt has 1% (multiplied by Compatibility value) chance to trigger";
                            int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(owner, targetCharacter);
                            int value = 2;
                            if (compatibility != -1) {
                                value = 1 * compatibility;
                                debugLog += "\n-Chance: " + value;
                            } else {
                                debugLog += "\n-Chance: " + value + " (No Compatibility)";
                            }
                            int flirtChance = UnityEngine.Random.Range(0, 100);
                            debugLog += "\n-Roll: " + flirtChance;
                            if (flirtChance < value) {
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetCharacter);
                            } else {
                                debugLog += "\n-Flirt did not trigger";
                            }
                        } else {
                            debugLog += "\n-Flirt did not trigger";
                        }
                    }
                }

                if (owner.faction == targetCharacter.faction || owner.homeSettlement == targetCharacter.homeSettlement) {
                    debugLog += "\n-Character and Target are with the same faction or settlement";
                    if (owner.opinionComponent.IsEnemiesWith(targetCharacter)) {
                        debugLog += "\n-Character considers Target as Enemy or Rival";
                        if (targetCharacter.canMove && targetCharacter.canPerform) {
                            debugLog += "\n-Target can move and can perform, will trigger Fight or Flight response";
                            //Fight or Flight
                        } else {
                            debugLog += "\n-Target can neither move or perform, will trigger Mock or Laugh At interrupt";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Character triggered Mock interrupt";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                            } else {
                                debugLog += "\n-Character triggered Laugh At interrupt";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                            }
                        }
                    } else {
                        debugLog += "\n-Character does not consider Target as Enemy or Rival";
                        if (!targetCharacter.canMove || !targetCharacter.canWitness) {
                            debugLog += "\n-Target cannot move or cannot witness";
                            if (targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) {
                                debugLog += "\n-Target is hungry or starving, will create feed job";
                                CreateFeedJobFor(targetCharacter);
                            } else if (targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) {
                                debugLog += "\n-Target is tired or exhausted, will create Move Character job to bed if Target has a home and an available bed";
                                if (targetCharacter.homeStructure != null) {
                                    Bed bed = targetCharacter.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) as Bed;
                                    if (bed != null) {
                                        debugLog += "\n-Target has a home and an available bed, will trigger Move Character job to bed";
                                        CreateActualDropJob(targetCharacter, targetCharacter.homeStructure.GetLocationStructure(), bed.gridTileLocation);
                                    } else {
                                        debugLog += "\n-Target has a home but does not have an available bed, will not trigger Move Character job";
                                    }
                                } else {
                                    debugLog += "\n-Target does not have a home, will not trigger Move Character job";
                                }
                            } else if (targetCharacter.needsComponent.isLonely || targetCharacter.needsComponent.isForlorn) {
                                debugLog += "\n-Target is bored or sulking, will trigger Move Character job if character is not in the right place to do Daydream or Pray";
                                if (UnityEngine.Random.Range(0, 2) == 0 && targetCharacter.homeStructure != null) {
                                    //Pray
                                    if (targetCharacter.currentStructure != targetCharacter.homeStructure) {
                                        debugLog += "\n-Target chose Pray and is not inside his/her house, will trigger Move Character job";
                                        CreateActualDropJob(targetCharacter, targetCharacter.homeStructure.GetLocationStructure());
                                    } else {
                                        debugLog += "\n-Target chose Pray but is already inside his/her house, will not trigger Move Character job";
                                    }
                                } else {
                                    //Daydream
                                    if (!targetCharacter.currentStructure.structureType.IsOpenSpace()) {
                                        debugLog += "\n-Target chose Daydream and is not in an open space structure, will trigger Move Character job";
                                        CreateActualDropJob(targetCharacter, targetCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                                    } else {
                                        debugLog += "\n-Target chose Daydream but is already in an open space structure, will not trigger Move Character job";
                                    }
                                }
                            }
                        }
                    }
                } else {
                    debugLog += "\n-Character and Target are not with the same faction and settlement";
                    if (owner.opinionComponent.IsEnemiesWith(targetCharacter)) {
                        debugLog += "\n-Character considers Target as Enemy or Rival, will trigger Fight or Flight response";
                        //Fight or Flight
                    }
                }
            }
            debugLog += "\n-Character will loop through all his/her traits to react to Target";
            for (int i = 0; i < owner.traitContainer.allTraits.Count; i++) {
                debugLog += "\n - " + owner.traitContainer.allTraits[i].name;
                if (owner.traitContainer.allTraits[i].OnSeePOI(targetCharacter, owner)) {
                    debugLog += ": triggered";
                } else {
                    debugLog += ": not triggered";
                }
            }
        } else {
            debugLog += "\n-Target is currently being targeted by an action, not going to react";
        }
    }
    private void ReactTo(TileObject targetTileObject, ref string debugLog) {
        debugLog += owner.name + " is reacting to " + targetTileObject.nameWithID;
        if (!IsPOICurrentlyTargetedByAPerformingAction(targetTileObject)) {
            debugLog += "\n-Target is not being targeted by an action, continue reaction";
            //TODO
        } else {
            debugLog += "\n-Target is currently being targeted by an action, not going to react";
        }
    }
    private void ReactTo(SpecialToken targetItem, ref string debugLog) {
        debugLog += owner.name + " is reacting to " + targetItem.nameWithID;
        if (!IsPOICurrentlyTargetedByAPerformingAction(targetItem)) {
            debugLog += "\n-Target is not being targeted by an action, continue reaction";
            //TODO
        } else {
            debugLog += "\n-Target is currently being targeted by an action, not going to react";
        }
    }
    #endregion

    #region General
    private bool IsPOICurrentlyTargetedByAPerformingAction(IPointOfInterest poi) {
        for (int i = 0; i < poi.allJobsTargetingThis.Count; i++) {
            if(poi.allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob planJob = poi.allJobsTargetingThis[i] as GoapPlanJob;
                if(planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Jobs
    private bool CreateFeedJobFor(Character targetCharacter) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.FEED, targetCharacter)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 20 });
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    private bool CreateActualDropJob(Character targetCharacter, LocationStructure dropLocationStructure) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure });
        return owner.jobQueue.AddJobInQueue(job);
    }
    private bool CreateActualDropJob(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
        return owner.jobQueue.AddJobInQueue(job);
    }
    #endregion
}
