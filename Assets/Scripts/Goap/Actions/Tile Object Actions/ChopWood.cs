using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopWood : GoapAction {
    private const int MAX_SUPPLY = 50;
    private const int MIN_SUPPLY = 20;

    private int _gainedSupply;
    public ChopWood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHOP_WOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //};
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = MAX_SUPPLY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing) {
            SetState("Chop Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void PreChopSuccess() {
        if (poiTarget is TreeObject) {
            TreeObject tree = poiTarget as TreeObject;
            _gainedSupply = tree.GetSupplyPerMine();
        }
        currentState.AddLogFiller(null, _gainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterChopSuccess() {
        if (poiTarget is TreeObject) {
            TreeObject tree = poiTarget as TreeObject;
            actor.AdjustSupply(_gainedSupply);
            tree.AdjustYield(-1);
        }
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}

public class ChopWoodData : GoapActionData {
    public ChopWoodData() : base(INTERACTION_TYPE.CHOP_WOOD) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}