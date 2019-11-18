using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class FeelingConcerned : GoapAction {

    public FeelingConcerned() : base(INTERACTION_TYPE.FEELING_CONCERNED) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Concerned Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
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

