using UnityEngine;
using System.Collections;
using ECS;
using System;

[Serializable]
public class Search : CharacterTask {

    private Action _searchAction;
	private Action _afterFindingAction;
	private delegate BaseLandmark GetTargetLandmark(Character character);
	private event GetTargetLandmark onGetTargetLandmarkAction;

    private object _searchingFor;
	private string searchingForLog;

    public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation, Quest parentQuest = null) : base(createdBy, TASK_TYPE.SEARCH, defaultDaysLeft, parentQuest) {
        SetStance(STANCE.NEUTRAL);
        _targetLocation = targetLocation;
        _searchingFor = searchingFor;
		onGetTargetLandmarkAction = GetLandmarkForCharacterSearching;
        Log log = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", (string)_searchingFor); //Add Fillers as necesssary per item seaching for

        if (searchingFor is string) {
            if((searchingFor as string).Equals("Heirloom Necklace")) {
                _searchAction = () => SearchForHeirloomNecklace();
                _alignments.Add(ACTION_ALIGNMENT.LAWFUL);
				//searchingForLog = "the owner of the " + (string)_searchingFor;
			}else if((searchingFor as string).Equals("Book of Inimical Incantations")) {
				SetStance(STANCE.COMBAT);
				_searchAction = () => SearchForItemInLandmark();
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
				//searchingForLog = "a " + (string)_searchingFor;
			}else if((searchingFor as string).Equals("Neuroctus")) {
				SetStance(STANCE.COMBAT);
				_searchAction = () => SearchForItemInLandmark();
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
				//searchingForLog = "a " + (string)_searchingFor;
			}else if((searchingFor as string).Equals("Psytoxin Herbalist")) {
				string[] splitted = ((string)_searchingFor).Split (' ');
				SetStance(STANCE.COMBAT);
				_searchAction = () => SearchForATag(splitted[1]);
				_afterFindingAction = () => CurePsytoxin ();
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
				//searchingForLog = "an " + splitted[1] + " to cure the " + splitted[0];
			}
            searchingForLog = Utilities.LogReplacer(log);
        }
        
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
        base.PerformTask();
        _searchAction();
        if (_daysLeft == 0) {
            //EndRecruitment();
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
    private void SearchForHeirloomNecklace() {
        for (int i = 0; i < targetLocation.charactersAtLocation.Count; i++) {
            ECS.Character currCharacter = targetLocation.charactersAtLocation[i].mainCharacter;
            if (currCharacter.HasItem(_searchingFor as string)) {
				//Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
				if(_afterFindingAction != null){
					_afterFindingAction ();
				}
                EndTask(TASK_STATUS.SUCCESS);
				break;
                //_assignedCharacter.questData.AdvanceToNextPhase();
            }
        }
    }
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
	private void SearchForItemInLandmark() {
		if (_targetLocation is BaseLandmark) {
			BaseLandmark _targetLandmark = (BaseLandmark)_targetLocation;
			for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
				ECS.Item item = _targetLandmark.itemsInLandmark [i];
				if (item.itemName == (string)_searchingFor) {
					int chance = UnityEngine.Random.Range (0, 100);
					if (chance < item.collectChance) {
					//_assignedCharacter.AddHistory ("Found a " + (string)_searchingFor + "!");
					//_targetLandmark.AddHistory (_assignedCharacter.name +  " found a " + (string)_searchingFor + "!");
						_assignedCharacter.PickupItem (item);
						_targetLandmark.RemoveItemInLandmark (item);
						if (_afterFindingAction != null) {
							_afterFindingAction ();
						}
						EndTask (TASK_STATUS.SUCCESS);
						break;
					}
				}
			}
		}
	}
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

	#region Search for a Tag
	private void SearchForATag(string tag){
		if (_targetLocation is BaseLandmark) {
			BaseLandmark _targetLandmark = (BaseLandmark)_targetLocation;
			for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
				ECS.Character currCharacter = _targetLandmark.charactersAtLocation [i].mainCharacter;
				if (currCharacter.HasTag (tag, true)) {
					if (_afterFindingAction != null) {
						_afterFindingAction ();
					}
					EndTask (TASK_STATUS.SUCCESS);
					break;
				}
			}
		}
	}
	#endregion

	#region Psytoxin Herbalist
	private void CurePsytoxin(){
		ECS.Item meteorite = _assignedCharacter.GetItemInInventory ("Meteorite");
		ECS.Item neuroctus = _assignedCharacter.GetItemInInventory ("Neuroctus");
		if(meteorite != null && neuroctus != null){
			_assignedCharacter.ThrowItem (meteorite, false);
			_assignedCharacter.ThrowItem (neuroctus, false);
            string psytoxinLevel = string.Empty;
			if(!_assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.MILD_PSYTOXIN)){
				if(!_assignedCharacter.RemoveCharacterTag (CHARACTER_TAG.MODERATE_PSYTOXIN)){
					_assignedCharacter.RemoveCharacterTag (CHARACTER_TAG.SEVERE_PSYTOXIN);
                    //_assignedCharacter.AddHistory ("Severe Psytoxin cured!");
                    psytoxinLevel = "Severe";
				}else{
                    //_assignedCharacter.AddHistory ("Moderate Psytoxin cured!");
                    psytoxinLevel = "Moderate";
				}
			}else{
                //_assignedCharacter.AddHistory ("Mild Psytoxin cured!");
                psytoxinLevel = "Mild";
			}
            Log cureLog = new Log(GameManager.Instance.Today(), "CharacterTags", "Psytoxin", "cured");
            cureLog.AddToFillers(null, psytoxinLevel, LOG_IDENTIFIER.OTHER);
		}
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
