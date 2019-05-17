using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stand : GoapAction {
    public Stand(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STAND, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.No_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        canBeAddedToMemory = false;
        shouldAddLogs = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Stand Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        return 4;
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion
}
