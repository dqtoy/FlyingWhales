using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;

    public Patrol(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PATROL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetTile != null) {
            SetState("Patrol Success");
        } else {
            SetState("Patrol Fail");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 5;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Patrol Fail");
    }
    public override void SetTargetStructure() {
        RandomizeTargetStructure();
        base.SetTargetStructure();
    }
    #endregion

    #region State Effects
    public void PrePatrolSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PrePatrolFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    private void RandomizeTargetStructure() {
        Area area = poiTarget.gridTileLocation.structure.location;
        Character target = poiTarget as Character;
        Character factionLeader = null;
        List<LocationStructure> dwellingChoices = null;

        if (target.faction.id != FactionManager.Instance.neutralFaction.id) {
            factionLeader = target.faction.leader as Character;
        }
        
        WeightedDictionary<STRUCTURE_TYPE> structureTypes = new WeightedDictionary<STRUCTURE_TYPE>();
        if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WORK_AREA, 7);
        }
        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WILDERNESS, 10);
        }
        if (area.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            structureTypes.AddElement(STRUCTURE_TYPE.WAREHOUSE, 5);
        }
        if (area.HasStructure(STRUCTURE_TYPE.INN)) {
            structureTypes.AddElement(STRUCTURE_TYPE.INN, 3);
        }
        if(factionLeader != null) {
            List<LocationStructure> dwellings = area.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                dwellingChoices = new List<LocationStructure>();
                for (int i = 0; i < dwellings.Count; i++) {
                    if(factionLeader.homeStructure == dwellings[i]) {
                        if (!structureTypes.HasElement(STRUCTURE_TYPE.EXIT)) { //don't be confused, EXIT only represents the dwelling of the faction leader
                            structureTypes.AddElement(STRUCTURE_TYPE.EXIT, 5);
                        }
                    } else {
                        dwellingChoices.Add(dwellings[i]);
                        if (!structureTypes.HasElement(STRUCTURE_TYPE.DWELLING)) {
                            structureTypes.AddElement(STRUCTURE_TYPE.DWELLING, 1);
                        }
                    }
                }
            }
        } else {
            if (area.HasStructure(STRUCTURE_TYPE.DWELLING)) {
                structureTypes.AddElement(STRUCTURE_TYPE.DWELLING, 1);
            }
        }


        if (structureTypes.Count > 0) {
            STRUCTURE_TYPE chosenStructureType = structureTypes.PickRandomElementGivenWeights();
            if (chosenStructureType == STRUCTURE_TYPE.EXIT) {
                _targetStructure = factionLeader.homeStructure;
            }else if(chosenStructureType == STRUCTURE_TYPE.DWELLING) {
                if(factionLeader != null && dwellingChoices != null) {
                    _targetStructure = dwellingChoices[UnityEngine.Random.Range(0, dwellingChoices.Count)];
                } else {
                    _targetStructure = area.GetRandomStructureOfType(chosenStructureType);
                }
            } else {
                _targetStructure = area.GetRandomStructureOfType(chosenStructureType);
            }
        }
    }
}
