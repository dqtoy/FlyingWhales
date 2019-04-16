using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stroll : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;

    protected override string failActionState { get { return "Stroll Fail"; } }

    public Stroll(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STROLL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        showIntelNotification = false;
        shouldAddLogs = false;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        if (targetTile != null) {
            SetState("Stroll Success");
        } else {
            SetState("Stroll Fail");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 5;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Stroll Fail");
    //}
    #endregion

    #region State Effects
    public void PreStrollSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreStrollFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    public void SetTargetStructure(LocationStructure structure) {
        _targetStructure = structure;
        if(_targetStructure == null) {
            RandomizeTargetStructure();
        } else {
            if(_targetStructure == actor.currentStructure) {
                actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
            } else {
                actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
            }
            targetTile = GetTargetLocationTile();
        }
    }

    private void RandomizeTargetStructure() {
        Area area = poiTarget.gridTileLocation.structure.location;
        Character target = poiTarget as Character;
        WeightedDictionary<STRUCTURE_TYPE> structureTypes = new WeightedDictionary<STRUCTURE_TYPE>();
        if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WORK_AREA, 8);
        }
        if (target.isAtHomeArea && target.homeStructure != null) {
            structureTypes.AddElement(STRUCTURE_TYPE.DWELLING, 4);
        }
        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WILDERNESS, 3);
        }
        if (area.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WAREHOUSE, 2);
        }
        if (area.HasStructure(STRUCTURE_TYPE.EXPLORE_AREA)) {
            structureTypes.AddElement(STRUCTURE_TYPE.EXPLORE_AREA, 2);
        }

        if(structureTypes.Count > 0) {
            STRUCTURE_TYPE chosenStructureType = structureTypes.PickRandomElementGivenWeights();
            if(chosenStructureType == STRUCTURE_TYPE.DWELLING) {
                SetTargetStructure(target.homeStructure);
            } else {
                SetTargetStructure(area.GetRandomStructureOfType(chosenStructureType));
            }
        }
    }
}
