using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Collect : CharacterTask {

	private string _itemNameToCollect;
	private int _quantityToCollect;

    private string itemToCollectLog;

	public Collect(TaskCreator createdBy, string itemName, int quantity, ILocation targetLocation, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.COMBAT) : base(createdBy, TASK_TYPE.COLLECT, stance, defaultDaysLeft, parentQuest) {
		_targetLocation = targetLocation;
		_itemNameToCollect = itemName;
		_quantityToCollect = quantity;

        Log log = new Log(GameManager.Instance.Today(), "CharacterTasks", "Collect", itemName); //Add Fillers as necesssary per item seaching for

        itemToCollectLog = Utilities.LogReplacer(log);

		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.COLLECT, new CollectState (this, _itemNameToCollect, _quantityToCollect) }
		};
    }

	#region overrides
	public override CharacterTask CloneTask() {
		Collect clonedTask = new Collect(_createdBy, _itemNameToCollect, _quantityToCollect, _targetLocation, _defaultDaysLeft, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
		return clonedTask;
	}
	public override bool CanBeDone(Character character, ILocation location) {
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			BaseLandmark landmark = (BaseLandmark)location;
			if(landmark.HasItem(_itemNameToCollect)){
				return true;
			}
		}
		return false;
	}
	public override void OnChooseTask(Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if (_targetLocation == null) {
			_targetLocation = GetLandmarkTarget(character);
		}
		if (_targetLocation != null) {
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartCollect());
		}else{
			EndTask(TASK_STATUS.FAIL);
		}
	}
	public override bool AreConditionsMet(Character character) {
		//check if there are any landmarks in region with characters
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
			if(CanBeDone(character, currLandmark)) {
				return true;
			}
		}
		return base.AreConditionsMet(character);
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		if(_currentState != null){
			_currentState.PerformStateAction ();
		}
		if(_daysLeft == 0){
			EndTaskFail ();
			return;
		}
		ReduceDaysLeft(1);
	}
	public override int GetSelectionWeight(ECS.Character character) {
		if (_parentQuest is FindLostHeir) {
			return 80;
		}
		return 0;
	}
	protected override BaseLandmark GetLandmarkTarget(ECS.Character character) {
		base.GetLandmarkTarget(character);
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
			if(CanBeDone(character, currLandmark)){
				int weight = 0;
				weight += currLandmark.charactersAtLocation.Count * 20;//For each character in a landmark in the current region: +20
				if (currLandmark.HasHostilitiesWith(character.faction)) {
					weight -= 50;//If landmark has hostile characters: -50
				}
				//If this character has already Searched in the landmark within the past 6 months: -60
				if (weight > 0) {
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

	private void StartCollect(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartCollect ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Collect", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startLog.AddToFillers(null, itemToCollectLog, LOG_IDENTIFIER.ITEM_1);

        _assignedCharacter.AddHistory(startLog);
        if (_targetLocation is BaseLandmark) {
            (targetLocation as BaseLandmark).AddHistory(startLog);
        }
		ChangeStateTo (STATE.COLLECT);
    }

    #region Logs
    public override string GetArriveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Collect", "arrive_action");
        arriveLog.AddToFillers(null, itemToCollectLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    public override string GetLeaveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Collect", "leave_action");
        arriveLog.AddToFillers(null, itemToCollectLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    #endregion
}
