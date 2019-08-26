using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Play : GoapAction {

    //private LocationStructure _targetStructure;

    //public override LocationStructure targetStructure { get { return _targetStructure; } }

    protected override string failActionState { get { return "Play Failed"; } }

    public Play(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PLAY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //};
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Play Failed");
        //} else {
            SetState("Play Success");
        //}
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        //**Cost**: randomize between 3-10
        return Utilities.rng.Next(6, 15);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Play Failed");
    //}
    //public override void SetTargetStructure() {
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).ToList();
    //    if (actor.specificLocation.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
    //        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WORK_AREA));
    //    }
    //    if (choices.Count > 0) {
    //        _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
    //    }
    //    base.SetTargetStructure();
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Play Success") {
            actor.AdjustDoNotGetLonely(-1);
            actor.AdjustDoNotGetTired(-1);
        }
    }
    #endregion

    #region Effects
    private void PrePlaySuccess() {
        actor.AdjustDoNotGetLonely(1);
        actor.AdjustDoNotGetTired(1);
    }
    private void PerTickPlaySuccess() {
        actor.AdjustHappiness(18);
    }
    private void AfterPlaySuccess() {
        actor.AdjustDoNotGetLonely(-1);
        actor.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
    #endregion
}

public class PlayData : GoapActionData {
    public PlayData() : base(INTERACTION_TYPE.PLAY) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}

