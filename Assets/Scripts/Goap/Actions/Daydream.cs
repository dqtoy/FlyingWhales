using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Daydream : GoapAction {

    //private LocationStructure _targetStructure;

    //public override LocationStructure targetStructure { get { return _targetStructure; } }

    protected override string failActionState { get { return "Daydream Failed"; } }

    public Daydream(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DAYDREAM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
        };
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
    public override void Perform() {
        base.Perform();
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Daydream Failed");
        //} else {
        SetState("Daydream Success");
        //}
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetBaseCost() {
        //**Cost**: randomize between 10-30
        return Utilities.rng.Next(10, 31);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Daydream Failed");
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
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Daydream Success") {
            actor.AdjustDoNotGetLonely(-1);
            actor.AdjustDoNotGetTired(-1);
        }
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is ACTION_LOCATION_TYPE) {
            actionLocationType = (ACTION_LOCATION_TYPE) otherData[0];
            SetTargetStructure();
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Effects
    private void PreDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(1);
        actor.AdjustDoNotGetTired(1);
    }
    private void PerTickDaydreamSuccess() {
        actor.AdjustHappiness(500);
    }
    private void AfterDaydreamSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        actor.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (actor.GetNormalTrait("Disillusioned") != null) {
            return false;
        }
        return actor == poiTarget;
        //if (actor == poiTarget) {
        //    //actor should be non-beast
        //    return actor.role.roleType != CHARACTER_ROLE.BEAST;
        //}
        //return false;
    }
    #endregion
}

public class DaydreamData : GoapActionData {
    public DaydreamData() : base(INTERACTION_TYPE.DAYDREAM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}
