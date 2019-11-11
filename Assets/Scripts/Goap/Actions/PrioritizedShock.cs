using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PrioritizedShock : GoapAction {

    public PrioritizedShock(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PRIORITIZED_SHOCK, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Shock Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return Utilities.rng.Next(25, 51);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion
}

public class PrioritizedShockData : GoapActionData {
    public PrioritizedShockData() : base(INTERACTION_TYPE.PRIORITIZED_SHOCK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}