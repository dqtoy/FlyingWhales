using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeelingConcerned : GoapAction {

    public FeelingConcerned(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FEELING_CONCERNED, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void Perform() {
        base.Perform();
        SetState("Concerned Success");
    }
    protected override int GetBaseCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void PreConcernedSuccess() {
        
    }
    private void AfterConcernedSuccess() {
       
    }
    #endregion   
}

public class FeelingConcernedData : GoapActionData {
    public FeelingConcernedData() : base(INTERACTION_TYPE.FEELING_CONCERNED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}

