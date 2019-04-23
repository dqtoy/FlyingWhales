using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Explore : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public Explore(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EXPLORE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (targetTile != null) {
        SetState("Explore Success");
        //} else {
        //    SetState("Explore Fail");
        //}
    }
    protected override int GetCost() {
        return 5;
    }
    public override void SetTargetStructure() {
        List<LocationStructure> choices = new List<LocationStructure>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area area = LandmarkManager.Instance.allAreas[i];
            if(area.areaType == AREA_TYPE.DUNGEON) {
                choices.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList());
            }
        }
        _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
        base.SetTargetStructure();
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreExploreSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreExploreFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}
