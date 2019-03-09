using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGuitar : GoapAction {
    public PlayGuitar(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            if (poiTarget.state == POI_STATE.ACTIVE) {
                SetState("Play Success");
            } else if (poiTarget.state == POI_STATE.INACTIVE) {
                SetState("Play Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return Utilities.rng.Next(3, 10);
        //return Random.Range(3, 11);
    }
    #endregion

    #region State Effects
    public void PrePlaySuccess() {
        actor.AdjustDoNotGetLonely(1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
    }
    public void PerTickPlaySuccess() {
        actor.AdjustHappiness(4);
    }
    public void AfterPlaySuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    public void PreTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

}
