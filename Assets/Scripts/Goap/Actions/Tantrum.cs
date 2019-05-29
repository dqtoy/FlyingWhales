using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tantrum : GoapAction {

    private GoapAction reason;

    public Tantrum(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TANTRUM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
        };
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
        return Utilities.rng.Next(3, 10);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        actor.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, actor.specificLocation, GameManager.Instance.GetTicksBasedOnHour(2));
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 1 && otherData[0] is GoapAction) {
            reason = otherData[0] as GoapAction;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Effects
    private void PreTantrumSuccess() {
        currentState.AddLogFiller(null, reason?.goapName ?? "None", LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion
}
