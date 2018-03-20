using UnityEngine;
using System.Collections;
using ECS;
using System;
using System.Collections.Generic;

[Serializable]
public class Search : CharacterTask {

	private delegate BaseLandmark GetTargetLandmark(Character character);
	private event GetTargetLandmark onGetTargetLandmarkAction;

    private object _searchingFor;
	private string searchingForLog;

	public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation, Quest parentQuest = null, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.SEARCH, stance, defaultDaysLeft, parentQuest) {
        _targetLocation = targetLocation;
        _searchingFor = searchingFor;
		onGetTargetLandmarkAction = GetLandmarkForCharacterSearching;
        Log log = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", (string)_searchingFor); //Add Fillers as necesssary per item seaching for
        if (searchingFor is string) {
            if((searchingFor as string).Equals("Heirloom Necklace")) {
                _alignments.Add(ACTION_ALIGNMENT.LAWFUL);
			}else if((searchingFor as string).Equals("Book of Inimical Incantations")) {
				SetStance(STANCE.COMBAT);
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
			}else if((searchingFor as string).Equals("Neuroctus")) {
				SetStance(STANCE.COMBAT);
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
			}else if((searchingFor as string).Equals("Psytoxin Herbalist")) {
				string[] splitted = ((string)_searchingFor).Split (' ');
				SetStance(STANCE.COMBAT);
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
			}
            searchingForLog = Utilities.LogReplacer(log);
        }

        _states = new Dictionary<STATE, State>() {
            { STATE.MOVE, new MoveState(this)},
            { STATE.SEARCH, new SearchState(this, searchingFor) }
        };
    }

    #region overrides
    public override CharacterTask CloneTask() {
        Search clonedTask = new Search(_createdBy, _defaultDaysLeft, _searchingFor, _targetLocation, _parentQuest);
        return clonedTask;
    }
    public override bool CanBeDone(Character character, ILocation location) {
        return true;
    }
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_targetLocation == null) {
            _targetLocation = GetLandmarkTarget(character);
        }
		if (_targetLocation == null) {
			EndTask(TASK_STATUS.FAIL); //the character could not search anywhere, fail this task
			return;
		}
        ChangeStateTo(STATE.MOVE);
		_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, () => StartSearch());
    }
    public override bool AreConditionsMet(Character character) {
        //check if there are any landmarks in region with characters
        Region regionLocation = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionLocation.allLandmarks[i];
            if(currLandmark.charactersAtLocation.Count > 0) {
                return true;
            }
        }
        return base.AreConditionsMet(character);
    }
    public override void PerformTask() {
        if(!CanPerformTask()){
            return;
        }
        if (_currentState != null) {
            _currentState.PerformStateAction();
        }
        if (_daysLeft == 0) {
            EndTask(TASK_STATUS.FAIL);
            return;
        }
        ReduceDaysLeft(1);
    }
    public override int GetSelectionWeight(ECS.Character character) {
        if (_parentQuest is FindLostHeir) {
            return 80;
		}else if (_parentQuest is TheDarkRitual) {
			return 150;
		}
        return 0;
    }
	protected override BaseLandmark GetLandmarkTarget(ECS.Character character) {
		base.GetLandmarkTarget(character);
		if(onGetTargetLandmarkAction != null){
			return onGetTargetLandmarkAction (character);
		}
		return null;
	}
    #endregion

	private void StartSearch(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartSearch ());
			return;
		}
        ChangeStateTo(STATE.SEARCH);
        _assignedCharacter.DestroyAvatar ();
        Log startSearchLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "start_search");
        startSearchLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startSearchLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);

        if (_targetLocation is BaseLandmark) {
            (targetLocation as BaseLandmark).AddHistory(startSearchLog);
        }
        _assignedCharacter.AddHistory(startSearchLog);
    }

    #region Find Lost Heir
	private BaseLandmark GetLandmarkForCharacterSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
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
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
    #endregion

	#region Search for Landmark Items
	private BaseLandmark GetLandmarkForLandmarkItemsSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
			int weight = 0;
			weight += currLandmark.itemsInLandmark.Count * 20;//For each item in a landmark in the current region: +20
			if (currLandmark.HasHostilitiesWith(character.faction)) {
				weight -= 50;//If landmark has hostile characters: -50
			}
			//If this character has already Searched in the landmark within the past 6 months: -60
			if (weight > 0) {
				_landmarkWeights.AddElement(currLandmark, weight);
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
	#endregion

    #region Logs
    public override string GetArriveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "arrive_action");
        arriveLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    public override string GetLeaveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "leave_action");
        arriveLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    #endregion
}
