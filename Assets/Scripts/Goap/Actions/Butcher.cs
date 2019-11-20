using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Butcher : GoapAction {

    public Butcher() : base(INTERACTION_TYPE.BUTCHER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(15, 26);
    }
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        Character deadCharacter = poiTarget as Character;
        if (deadCharacter == null) {
            deadCharacter = (poiTarget as Tombstone).character;
        }
        log.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = this.IsTargetMissing(actor, target);
        return new GoapActionInvalidity(defaultTargetMissing, stateName);
    }
    private bool IsTargetMissing(Character actor, IPointOfInterest poiTarget) {
        return poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || !(poiTarget as Character).isDead;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            Character deadCharacter = GetDeadCharacter(poiTarget);
            if (deadCharacter != null) {
                if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                    //return true;
                    if (actor.traitContainer.GetNormalTrait("Cannibal") != null) {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character) {
            return (poiTarget as Character).isDead;
        }
        return false;
    }
    #endregion

    private Character GetDeadCharacter(IPointOfInterest poiTarget) {
        if (poiTarget is Character) {
            return poiTarget as Character;
        } else if (poiTarget is Tombstone) {
            return (poiTarget as Tombstone).character;
        }
        return null;
    }

    #region State Effects
    public void PreTransformSuccess(ActualGoapNode goapNode) {
        Character deadCharacter = GetDeadCharacter(goapNode.poiTarget);
        int transformedFood = 0;
        if (deadCharacter.race == RACE.WOLF) {
            transformedFood = 80;
        } else if (deadCharacter.race == RACE.HUMANS) {
            transformedFood = 140;
        } else if (deadCharacter.race == RACE.ELVES) {
            transformedFood = 120;
        }
        //if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
        //    currentState.SetIntelReaction(CannibalTransformSuccessIntelReaction);
        //} else {
        //    currentState.SetIntelReaction(NormalTransformSuccessIntelReaction);
        //}
        goapNode.descriptionLog.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.descriptionLog.AddToFillers(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        Character deadCharacter = GetDeadCharacter(goapNode.poiTarget);
        int transformedFood = 0;
        if (deadCharacter.race == RACE.WOLF) {
            transformedFood = 80;
        } else if (deadCharacter.race == RACE.HUMANS) {
            transformedFood = 140;
        } else if (deadCharacter.race == RACE.ELVES) {
            transformedFood = 120;
        }
        //TODO: deadCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.BURY);
        goapNode.actor.AdjustFood(transformedFood);
        if (goapNode.poiTarget is Tombstone) {
            goapNode.poiTarget.gridTileLocation.structure.RemovePOI(goapNode.poiTarget, goapNode.actor);
        } else if (goapNode.poiTarget is Character) {
            (goapNode.poiTarget as Character).DestroyMarker();
        }
    }
    //public void PreTargetMissing() {
    //    goapNode.descriptionLog.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    //#region Intel Reactions
    //private List<string> NormalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            reactions.Add("This isn't important.");
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> CannibalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //            //- Positive Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //                reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //            }
    //            //- Negative Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                }
    //            }
    //            //- No Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class ButcherData : GoapActionData {
    public ButcherData() : base(INTERACTION_TYPE.BUTCHER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        Character targetCharacter = null;
        if (poiTarget is Character) {
            targetCharacter = poiTarget as Character;
        } else if (poiTarget is Tombstone) {
            targetCharacter = (poiTarget as Tombstone).character;
        }
        if (targetCharacter != null) {
            if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                //return true;
                if (actor.traitContainer.GetNormalTrait("Cannibal") != null) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
}
