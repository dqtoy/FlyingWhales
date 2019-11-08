using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHomeLocation : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }

    protected override string failActionState { get { return "Return Home Failed"; } }

    public ReturnHomeLocation(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RETURN_HOME_LOCATION, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Return Home Location";
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    public override void Perform() {
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Return Home Failed");
        //} else {
        //}
        base.Perform();
        SetState("Return Home Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetBaseCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Return Home Failed");
    //}
    public override void SetTargetStructure() {
        _targetStructure = GetTargetStructure();
        base.SetTargetStructure();
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    private LocationStructure GetTargetStructure() {
        return actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
    }
}

public class ReturnHomeLocationData : GoapActionData {
    public ReturnHomeLocationData() : base(INTERACTION_TYPE.RETURN_HOME_LOCATION) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
