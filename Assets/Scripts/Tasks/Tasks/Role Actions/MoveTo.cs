using UnityEngine;
using System.Collections;
using ECS;
using System.Linq;

public class MoveTo : CharacterTask {
//    private PATHFINDING_MODE _pathfindingMode;

    #region getters/setters
    public HexTile targetTile {
        get {
            if (_targetLocation is BaseLandmark) {
                return (_targetLocation as BaseLandmark).location;
            } else {
                return (_targetLocation as HexTile);
            }
        }
    }
    #endregion

	public MoveTo(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.MOVE_TO, defaultDaysLeft) {
		//_forPlayerOnly = true;
		SetStance(STANCE.NEUTRAL);
        //_actionString = "to visit";
    }

    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
        if (_targetLocation == null) {
            _targetLocation = GetLandmarkTarget(character);
        }
		if (_targetLocation != null) {
			//Debug.Log(_assignedCharacter.name + " goes to " + _targetLocation.locationName);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS);
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();
		SuccessTask ();
    }
	public override bool CanBeDone (ECS.Character character, ILocation location){
		return true;
	}
	public override bool AreConditionsMet (ECS.Character character){
		return true;
	}
    public override int GetSelectionWeight(Character character) {
        return 30;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.adjacentRegionsViaMajorRoad.Count; i++) {
            Region adjacentRegion = regionOfChar.adjacentRegions[i];
            int weight = 50 +  (50 * adjacentRegion.adjacentRegionsViaMajorRoad.Where(x => x.id != regionOfChar.id).Count()); //Each Adjacent Settlement: 50 + (50 x NoAdjacentSettlements)
            if (adjacentRegion.mainLandmark.HasHostilitiesWith(character.faction, true)) {
                weight -= 50; //If Adjacent Settlement is hostile: -30
            }
            if (adjacentRegion.mainLandmark.owner != null) {
                if (adjacentRegion.mainLandmark.owner.id == character.id) {
                    weight += 30; //If Adjacent Settlement is owned by my faction: +30
                }
            } else {
                weight -= 20; //If Adjacent Settlement is unoccupied: -20
            }
            if (weight > 0) {
                _landmarkWeights.AddElement(adjacentRegion.mainLandmark, weight);
            }
        }
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
		DoNothing doNothing = new DoNothing (_assignedCharacter);
		//doNothing.SetDaysLeft (3);
		doNothing.OnChooseTask (_assignedCharacter);
    }
}
