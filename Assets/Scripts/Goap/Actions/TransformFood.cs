using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFood : GoapAction {

    private int transformedFood;
    private Character deadCharacter;

    public TransformFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TRANSFORM_FOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;

        if (poiTarget is Character) {
            deadCharacter = poiTarget as Character;
        } else if (poiTarget is Tombstone) {
            deadCharacter = (poiTarget as Tombstone).character;
        }
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
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
        if (currentState.name == "Transform Success") {
            if (poiTarget is Tombstone) {
                poiTarget.gridTileLocation.structure.RemovePOI(poiTarget, actor);
            } else if (poiTarget is Character) {
                (poiTarget as Character).DestroyMarker();
            }
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        if (deadCharacter != null) {
            if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                return true;
                //if (actor.GetNormalTrait("Cannibal") != null) {
                //    return true;
                //}
                //return false;
            }
            return true;
        }
        return false;
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
        currentState.AddLogFiller(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTransformSuccess() {
        deadCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.BURY);
        actor.AdjustFood(transformedFood);
    }
    #endregion
}
