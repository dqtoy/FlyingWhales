using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class GetWater : GoapAction {

    public GetWater() : base(INTERACTION_TYPE.GET_WATER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.WATER_BUCKET.ToString(), targetPOI = actor });
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 5;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (!isTargetMissing) {
            SetState("Obtain Water Success");
        } else {
            SetState("Target Missing");
        }
    }
    #endregion

    private void AfterObtainWaterSuccess() {
        actor.ObtainToken(TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.WATER_BUCKET));
    }
}

public class GetWaterData : GoapActionData {
    public GetWaterData() : base(INTERACTION_TYPE.GET_WATER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}