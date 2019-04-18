using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemHome : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public DropItemHome(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor }, IsItemInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_ITEM, conditionKey = poiTarget, targetPOI = actor });
    }
    public override void PerformActualAction() {
        SetState("Drop Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override void SetTargetStructure() {
        _targetStructure = actor.homeStructure;
        base.SetTargetStructure();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor.homeStructure != null;
    }
    #endregion

    #region Preconditions
    private bool IsItemInInventory() {
        SpecialToken token = poiTarget as SpecialToken;
        return actor.GetToken(token) != null;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess() {
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess() {
        LocationGridTile tile = actor.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        actor.DropToken(poiTarget as SpecialToken, actor.gridTileLocation.structure.location, actor.gridTileLocation.structure, tile);
    }
    #endregion
}
