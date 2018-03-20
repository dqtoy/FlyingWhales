using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;
using System.Collections.Generic;

public class DrinkBlood : CharacterTask {

	public DrinkBlood(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) 
		: base(createdBy, TASK_TYPE.DRINK_BLOOD, stance, defaultDaysLeft) {
        _states = new Dictionary<STATE, State>() {
            { STATE.MOVE, new MoveState(this) },
            { STATE.DRINK_BLOOD, new DrinkBloodState(this) }
        };
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget(character);
		}
		if(_targetLocation != null && _targetLocation is BaseLandmark){
            ChangeStateTo(STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartDrinkingBlood());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override void TaskCancel() {
		base.TaskCancel();
		_assignedCharacter.DestroyAvatar();
	}
	public override void TaskFail() {
		base.TaskFail();
		_assignedCharacter.DestroyAvatar();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
			return true;
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
        //check if there are any landmarks in region and adjacent regions that have civilians
        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(character.specificLocation.tileLocation.region);
        regionsToCheck.AddRange(character.specificLocation.tileLocation.region.adjacentRegions);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.allLandmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.allLandmarks[j];
                if (currLandmark.civilians > 0) {
                    return true;
                }
            }
        }
        return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 25;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
       	base.GetLandmarkTarget(character);
        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(character.specificLocation.tileLocation.region);
        regionsToCheck.AddRange(character.specificLocation.tileLocation.region.adjacentRegions);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.allLandmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.allLandmarks[j];
                if (currLandmark.civilians > 0) {
                    int weight = 100;//All landmarks with civilians in current and adjacent regions: 100
                    if (currLandmark.owner != null && currLandmark.owner == character.faction) {
                        weight += 100;//Landmark owned by a different faction: +100
                    }
                    if (currLandmark.HasHostilitiesWith(character)) {
                        weight -= 50;
                    }
                    _landmarkWeights.AddElement(currLandmark, weight);
                }
            }
        }
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
        return null;
    }
    #endregion

    private void StartDrinkingBlood() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartDrinkingBlood ());
			return;
		}
        ChangeStateTo(STATE.DRINK_BLOOD);
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "DrinkBlood", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        (_targetLocation as BaseLandmark).AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);
    }
}
