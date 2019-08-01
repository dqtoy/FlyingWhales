using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamingToSteal : GoapAction {

    public RoamingToSteal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ROAMING_TO_STEAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("In Progress");
    }
    protected override int GetCost() {
        return 1;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor == poiTarget) {
            return true;
        }
        return false;
    }
    #endregion

    #region Effects
    private void PreInProgress() {
        Kleptomaniac kleptomaniac = actor.GetNormalTrait("Kleptomaniac") as Kleptomaniac;
        if (actor.marker.inVisionPOIs.Count > 0) {
            for (int i = 0; i < actor.marker.inVisionPOIs.Count; i++) {
                if (kleptomaniac.CreateJobsOnEnterVisionBasedOnTrait(actor.marker.inVisionPOIs[i], actor)) {
                    return;
                }
            }
        }
        RoamAround();
    }

    private void RoamAround() {
        actor.marker.GoTo(PickRandomTileToGoTo(), RoamAround);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = actor.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No tile in " + chosenStructure.name + " for " + actor.name + " to go to in " + goapType.ToString());
        }
    }
    #endregion
}
