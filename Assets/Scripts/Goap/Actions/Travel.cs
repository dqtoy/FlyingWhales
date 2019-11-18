using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Travel : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }
    protected override string failActionState { get { return "Travel Failed"; } }

    public Travel() : base(INTERACTION_TYPE.TRAVEL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Travel";
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Travel Failed");
        //} else {
            SetState("Travel Success");
        //}
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Travel Failed");
    //}
    public override void SetTargetStructure() {
        _targetStructure = GetTargetStructure();
        base.SetTargetStructure();
    }
    #endregion

    #region State Effects
    public void PreTravelSuccess() {
        currentState.AddLogFiller(_targetStructure.location, _targetStructure.location.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreTravelFailed() {
        currentState.AddLogFiller(_targetStructure.location, _targetStructure.location.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirement
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        return actor == poiTarget;
    }
    #endregion

    private LocationStructure GetTargetStructure() {
        //Travel to the wilderness of a different location, weight based on distance from current location:
        //Base Weight: 20(-2 per tile away from current location, minimum 2)
        WeightedDictionary<LocationStructure> weights = new WeightedDictionary<LocationStructure>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id != actor.specificLocation.id) {
                LocationStructure wilderness = currArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                if (wilderness != null) {
                    int weight = 20;
                    List<HexTile> path = PathGenerator.Instance.GetPath(actor.specificLocation.coreTile, currArea.coreTile, PATHFINDING_MODE.UNRESTRICTED);
                    if (path != null) {
                        weight -= 2 * path.Count;
                    }
                    weight = Mathf.Max(weight, 2);
                    weights.AddElement(wilderness, weight);
                }
            }
        }
        return weights.PickRandomElementGivenWeights();
    }
}
