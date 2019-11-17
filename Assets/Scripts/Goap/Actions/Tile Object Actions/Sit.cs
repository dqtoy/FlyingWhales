using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

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
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (!isTargetMissing) {
            SetState("Sit Success");
        } else {
            if(!poiTarget.IsAvailable()) {
                SetState("Sit Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 8;
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
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if(poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
            return poiTarget.IsAvailable();
            //Dwelling dwelling = poiTarget.gridTileLocation.structure as Dwelling;
            //if (dwelling.IsResident(actor)) {
            //    return true;
            //}
        }
        return false;
    }
    #endregion
}

public class SitData : GoapActionData {
    public SitData() : base(INTERACTION_TYPE.SIT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
            return poiTarget.IsAvailable();
        }
        return false;
    }
}