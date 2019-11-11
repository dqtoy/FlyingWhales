using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class LaughAt : GoapAction {

    public LaughAt(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.LAUGH_AT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Laugh Success");
    }
    protected override int GetCost() {
        return Utilities.rng.Next(40, 61);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PerTickLaughSuccess() {
        actor.AdjustHappiness(500);
    }
    private void AfterLaughSuccess() {
        if (poiTarget.traitContainer.GetNormalTrait("Unconscious") == null) {
            poiTarget.traitContainer.AddTrait(poiTarget, "Ashamed");
        }
    }
    #endregion   
}

public class LaughAtData : GoapActionData {
    public LaughAtData() : base(INTERACTION_TYPE.LAUGH_AT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}

