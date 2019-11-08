using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dance : GoapAction {

    public Dance(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DANCE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
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
    public override void Perform() {
        base.Perform();
        SetState("Dance Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetBaseCost() {
        //**Cost**: randomize between 20-36
        return Utilities.rng.Next(20, 37);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Dance Success") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region Effects
    private void PreDanceSuccess() {
        actor.AdjustDoNotGetLonely(1);
    }
    private void PerTickDanceSuccess() {
        actor.AdjustHappiness(1000);
    }
    private void AfterDanceSuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion
}

public class DanceData : GoapActionData {
    public DanceData() : base(INTERACTION_TYPE.DANCE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && (actor.currentMoodType == CHARACTER_MOOD.GOOD || actor.currentMoodType == CHARACTER_MOOD.GREAT);
    }
}
