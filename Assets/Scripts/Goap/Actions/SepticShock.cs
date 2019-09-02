using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SepticShock : GoapAction {

    public SepticShock(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SEPTIC_SHOCK, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Septic Shock Success");
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
    private void PreSepticShockSuccess() {
        actor.SetPOIState(POI_STATE.INACTIVE);
    }
    private void AfterSepticShockSuccess() {
        actor.SetPOIState(POI_STATE.ACTIVE);
        actor.Death("Septic Shock", this, _deathLog: currentState.descriptionLog);
    }
    #endregion
}

public class SepticShockData : GoapActionData {
    public SepticShockData() : base(INTERACTION_TYPE.SEPTIC_SHOCK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}
