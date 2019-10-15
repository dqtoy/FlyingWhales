using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFood : GoapAction {

    private int transformedFood;
    private Character deadCharacter;

    protected override bool isTargetMissing {
        get {
            bool targetMissing = poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || !(poiTarget as Character).isDead;

            if (targetMissing) {
                return targetMissing;
            } else {
                Invisible invisible = poiTarget.GetNormalTrait("Invisible") as Invisible;
                if (invisible != null && !invisible.charactersThatCanSee.Contains(actor)) {
                    return true;
                }
                return targetMissing;
            }
        }
    }


    public TransformFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TRANSFORM_FOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;

        if (poiTarget is Character) {
            deadCharacter = poiTarget as Character;
        } else if (poiTarget is Tombstone) {
            deadCharacter = (poiTarget as Tombstone).character;
        }
        //if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
        //    SetIsStealth(true);
        //}
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if(poiTarget is Character) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = poiTarget }, IsTargetDead);
        }
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = 0, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Transform Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return Utilities.rng.Next(15, 26);
    }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        thoughtBubbleMovingLog.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        //if (currentState.name == "Transform Success") {
        //    if (poiTarget is Tombstone) {
        //        poiTarget.gridTileLocation.structure.RemovePOI(poiTarget, actor);
        //    } else if (poiTarget is Character) {
        //        (poiTarget as Character).DestroyMarker();
        //    }
        //}
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        if (deadCharacter != null) {
            if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                //return true;
                if (actor.GetNormalTrait("Cannibal") != null) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead() {
        return (poiTarget as Character).isDead;
    }
    #endregion

    #region State Effects
    private void PreTransformSuccess() {
        if(deadCharacter.race == RACE.WOLF) {
            transformedFood = 80;
        } else if (deadCharacter.race == RACE.HUMANS) {
            transformedFood = 140;
        } else if (deadCharacter.race == RACE.ELVES) {
            transformedFood = 120;
        }
        if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
            SetCommittedCrime(CRIME.ABERRATION, new Character[] { actor });
            currentState.SetIntelReaction(CannibalTransformSuccessIntelReaction);
        } else {
            currentState.SetIntelReaction(NormalTransformSuccessIntelReaction);
        }
        currentState.AddLogFiller(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTransformSuccess() {
        deadCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.BURY);
        actor.AdjustFood(transformedFood);
        if (poiTarget is Tombstone) {
            poiTarget.gridTileLocation.structure.RemovePOI(poiTarget, actor);
        } else if (poiTarget is Character) {
            (poiTarget as Character).DestroyMarker();
        }
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Intel Reactions
    private List<string> NormalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                reactions.Add("This isn't important.");
            }
        }
        return reactions;
    }
    private List<string> CannibalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("Do not tell anybody, please!");
                }
                //- Positive Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        recipient.marker.AddAvoidInRange(actor);
                    }
                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                }
                //- Negative Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    } else {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    }
                }
                //- No Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
                    if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    } else {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class TransformFoodData : GoapActionData {
    public TransformFoodData() : base(INTERACTION_TYPE.TRANSFORM_FOOD) {
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
                if (actor.GetNormalTrait("Cannibal") != null) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
}
