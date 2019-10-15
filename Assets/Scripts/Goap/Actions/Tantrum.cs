using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tantrum : GoapAction {

    private string reason;

    public Tantrum(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TANTRUM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Tantrum Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        //**Cost**: randomize between 3-10
        return Utilities.rng.Next(3, 11);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        //CharacterState berserkedState = actor.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, actor.specificLocation, GameManager.Instance.GetTicksBasedOnHour(2));
        //(berserkedState as BerserkedState).SetAreCombatsLethal(false);
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is string) {
            reason = otherData[0] as string;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Effects
    private void PreTantrumSuccess() {
        currentState.AddLogFiller(null, reason, LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTantrumSuccess() {
        CharacterState berserkedState = actor.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, actor.specificLocation, GameManager.Instance.GetTicksBasedOnHour(2));
        (berserkedState as BerserkedState).SetAreCombatsLethal(false);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion
}

public class TantrumData : GoapActionData {
    public TantrumData() : base(INTERACTION_TYPE.TANTRUM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}