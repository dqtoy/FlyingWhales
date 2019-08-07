using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puke : GoapAction {

    public Puke(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PUKE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Puke Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 5;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PrePukeSuccess() {
        actor.SetPOIState(POI_STATE.INACTIVE);
    }
    private void AfterPukeSuccess() {
        actor.SetPOIState(POI_STATE.ACTIVE);
    }
    #endregion
}
