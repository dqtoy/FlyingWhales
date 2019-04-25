using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemWarehouse : GoapAction {

    private LocationStructure _targetStructure;

    public override LocationStructure targetStructure { get { return _targetStructure; } }

    public DropItemWarehouse(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Drop Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        return 1;
    }
    public override void SetTargetStructure() {
        _targetStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        base.SetTargetStructure();
    }
    #endregion

    #region State Effects
    private void PreDropSuccess() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterDropSuccess() {
        //**Effect 1**: Actor loses item, add target item to tile. Clear personal owner of the item.
        actor.DropToken(poiTarget as SpecialToken, actor.specificLocation, actor.currentStructure, actor.gridTileLocation);
    }
    #endregion
}
