using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SleepOutside : GoapAction {
    public SleepOutside(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP_OUTSIDE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Sleep_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetTile != null) {
            SetState("Rest Success");
        } else {
            SetState("Rest Fail");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 9;
    }
    //public override void SetTargetStructure() {
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList();
    //    if (actor.specificLocation.HasStructure(STRUCTURE_TYPE.DWELLING)) {
    //        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.DWELLING).Where(x => x.unoccupiedTiles.Count > 0 && !x.IsOccupied()));
    //    }
    //    _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
    //    base.SetTargetStructure();
    //}
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetTired(1);
        //actor.AddTrait("Resting");
    }
    private void PerTickRestSuccess() {
        actor.AdjustTiredness(3);
    }
    private void AfterRestSuccess() {
        actor.AdjustDoNotGetTired(-1);
    }
    //private void PreRestFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion
}
