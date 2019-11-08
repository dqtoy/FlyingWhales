using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grieving : GoapAction {
    public Grieving(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.GRIEVING, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void Perform() {
        base.Perform();
        SetState("Grieving Success");
    }
    protected override int GetBaseCost() {
        return 10;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion
}

public class GrievingData : GoapActionData {
    public GrievingData() : base(INTERACTION_TYPE.GRIEVING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
