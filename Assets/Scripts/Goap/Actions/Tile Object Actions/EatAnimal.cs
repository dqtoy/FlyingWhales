using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAnimal : GoapAction {
    protected override string failActionState { get { return "Eat Fail"; } }

    public EatAnimal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_SMALL_ANIMAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Eat Success");
        } else {
            if (!poiTarget.IsAvailable()) {
                SetState("Eat Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        if (actor.GetNormalTrait("Carnivore") != null) {
            return 25;
        } else {
            return 50;
        }
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Eat Fail");
    //}
    public override void OnStopActionDuringCurrentState() {
        if(currentState.name == "Eat Success") {
            actor.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
        //actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(520);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    private void PreEatFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget.gridTileLocation != null) {
            //if (knownLoc.occupant == null) {
            //    return true;
            //} else if (knownLoc.occupant == actor) {
            //    return true;
            //}
            return true;
        }
        return false;
    }
    #endregion
}

public class EatAnimalData : GoapActionData {
    public EatAnimalData() : base(INTERACTION_TYPE.EAT_SMALL_ANIMAL) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget.gridTileLocation != null) {
            return true;
        }
        return false;
    }
}