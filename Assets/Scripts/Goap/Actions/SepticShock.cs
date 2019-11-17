﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class SepticShock : GoapAction {

    public SepticShock() : base(INTERACTION_TYPE.SEPTIC_SHOCK) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Septic Shock Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 5;
    }
    #endregion

    #region State Effects
    private void PreSepticShockSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
    }
    private void AfterSepticShockSuccess(ActualGoapNode goapNode) {
        goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
        goapNode.actor.Death("Septic Shock", this, _deathLog: goapNode.action.states[goapNode.currentStateName].descriptionLog);
    }
    #endregion
}

public class SepticShockData : GoapActionData {
    public SepticShockData() : base(INTERACTION_TYPE.SEPTIC_SHOCK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}
