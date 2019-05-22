using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sit : GoapAction {
    public Sit(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SIT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
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
        if (!isTargetMissing) {
            SetState("Sit Success");
        } else {
            if(poiTarget.state == POI_STATE.INACTIVE) {
                SetState("Sit Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        if((poiTarget as TileObject).tileObjectType == TILE_OBJECT_TYPE.TABLE) {
            return 8;
        }
        return 4;
    }
    #endregion

    #region Effects
    private void PreSitFail() {
        currentState.AddLogFiller(null, poiTarget.name, LOG_IDENTIFIER.STRING_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(null, poiTarget.name, LOG_IDENTIFIER.STRING_1);
    }
    #endregion
    #region Requirement
    protected bool Requirement() {
        if(poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
            return true;
            //Dwelling dwelling = poiTarget.gridTileLocation.structure as Dwelling;
            //if (dwelling.IsResident(actor)) {
            //    return true;
            //}
        }
        return false;
    }
    #endregion
}
