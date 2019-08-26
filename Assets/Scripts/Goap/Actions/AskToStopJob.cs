using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AskToStopJob : GoapAction {

    public GoapPlanJob jobToStop { get; private set; }

    public AskToStopJob(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASK_TO_STOP_JOB, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_STOP_ACTION_AND_JOB, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            if (poiTarget is Character) {
                Character targetCharacter = poiTarget as Character;
                if (targetCharacter.currentAction != null && targetCharacter.currentAction.parentPlan != null && targetCharacter.currentAction.parentPlan.job != null
                && targetCharacter.currentAction.parentPlan.job == jobToStop) {
                    SetState("Ask Success");
                } else {
                    SetState("Ask Fail");
                }
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            if(targetCharacter.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                return Utilities.rng.Next(15, 25);
            }
        }
        return Utilities.rng.Next(20, 45);
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is GoapPlanJob) {
            jobToStop = otherData[0] as GoapPlanJob;
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        bool targetDoesNotConsiderActorEnemy = true;
        //bool canDoAction = false;
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            //if(jobToStop == null) {
            //    canDoAction = true;
            //} else if (targetCharacter.currentAction != null && targetCharacter.currentAction.parentPlan != null && targetCharacter.currentAction.parentPlan.job != null
            //    && targetCharacter.currentAction.parentPlan.job == jobToStop) {
            //    canDoAction = true;
            //}
            targetDoesNotConsiderActorEnemy = !targetCharacter.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY);
        }
        return actor != poiTarget && targetDoesNotConsiderActorEnemy; //&& canDoAction
    }
    #endregion

    #region State Effects
    private void PreAskSuccess() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            currentState.AddLogFiller(null, targetCharacter.currentAction.goapName, LOG_IDENTIFIER.STRING_1);
        }
    }
    private void PreAskFail() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            currentState.AddLogFiller(null, jobToStop.name, LOG_IDENTIFIER.STRING_1);
        }
    }
    private void AfterAskSuccess() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            if(targetCharacter.currentAction != null) {
                targetCharacter.currentAction.StopAction(true);
            }
        }
    }
    #endregion
}

public class AskToStopJobData : GoapActionData {
    public AskToStopJobData() : base(INTERACTION_TYPE.ASK_TO_STOP_JOB) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool targetDoesNotConsiderActorEnemy = true;
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            targetDoesNotConsiderActorEnemy = !targetCharacter.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY);
        }
        return actor != poiTarget && targetDoesNotConsiderActorEnemy;
    }
}