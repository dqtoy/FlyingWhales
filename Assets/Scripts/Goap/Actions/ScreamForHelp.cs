using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamForHelp : GoapAction {

    public ScreamForHelp(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SCREAM_FOR_HELP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    public override void Perform() {
        base.Perform();
        SetState("Scream Success");
    }
    protected override int GetBaseCost() {
        return 1;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    //public void PreScreamSuccess() {
    //}
    public void PerTickScreamSuccess() {
        Messenger.Broadcast(Signals.SCREAM_FOR_HELP, actor);
    }
    public void AfterScreamSuccess() {

    }
    #endregion
}

public class ScreamForHelpData : GoapActionData {
    public ScreamForHelpData() : base(INTERACTION_TYPE.SCREAM_FOR_HELP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
