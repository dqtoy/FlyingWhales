using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeelingBrokenhearted : GoapAction {
    public FeelingBrokenhearted(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FEELING_BROKENHEARTED, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Brokenhearted Success");
    }
    protected override int GetCost() {
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

public class FeelingBrokenheartedData : GoapActionData {
    public FeelingBrokenheartedData() : base(INTERACTION_TYPE.FEELING_BROKENHEARTED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
