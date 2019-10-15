using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sing : GoapAction {
    private int happinessValue = 1000;

    public Sing(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SING, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Sing Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        //**Cost**: randomize between 20 - 36 (if music lover 10 - 26)
        if(actor.GetNormalTrait("Music Lover") != null) {
            return Utilities.rng.Next(10, 27);
        }
        return Utilities.rng.Next(20, 37);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Sing Success") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region Effects
    private void PreSingSuccess() {
        actor.AdjustDoNotGetLonely(1);
        if(actor.GetNormalTrait("Music Lover") != null) {
            happinessValue = 1200;
        }
    }
    private void PerTickSingSuccess() {
        actor.AdjustHappiness(happinessValue);
    }
    private void AfterSingSuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion
}

public class SingData : GoapActionData {
    public SingData() : base(INTERACTION_TYPE.SING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && actor.GetNormalTrait("Music Hater") == null && (actor.currentMoodType == CHARACTER_MOOD.GOOD || actor.currentMoodType == CHARACTER_MOOD.GREAT);
    }
}