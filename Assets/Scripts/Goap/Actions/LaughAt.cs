using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class LaughAt : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public LaughAt() : base(INTERACTION_TYPE.LAUGH_AT) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Laugh Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(40, 61);
    }
    #endregion

    #region State Effects
    private void PerTickLaughSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHappiness(500);
    }
    private void AfterLaughSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget.traitContainer.GetNormalTrait("Unconscious") == null) {
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Ashamed");
        }
    }
    #endregion   
}

public class LaughAtData : GoapActionData {
    public LaughAtData() : base(INTERACTION_TYPE.LAUGH_AT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}

