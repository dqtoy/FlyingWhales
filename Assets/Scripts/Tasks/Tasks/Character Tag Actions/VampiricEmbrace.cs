using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class VampiricEmbrace : CharacterTask {

	private ECS.Character _targetCharacter;
	private BaseLandmark _targetLandmark;

	public VampiricEmbrace(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) 
		: base(createdBy, TASK_TYPE.VAMPIRIC_EMBRACE, stance, defaultDaysLeft) {
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
		_needsSpecificTarget = true;
		_specificTargetClassification = "character";
		_filters = new TaskFilter[] {
			new MustNotHaveTags (CHARACTER_TAG.VAMPIRE),
		};
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_specificTarget == null){
            WeightedDictionary<ECS.Character> characterWeights = GetCharacterTargetWeights(character);
            if (characterWeights.GetTotalOfWeights() > 0) {
                _specificTarget = GetTargetCharacter();
            } else {
                EndTask(TASK_STATUS.FAIL);
                return;
            }
		}
		if(_specificTarget is ECS.Character){
			_targetCharacter = (ECS.Character)_specificTarget;
			if(_targetLocation == null){
				_targetLocation = _targetCharacter.specificLocation;
			}

			if (_targetLocation != null && _targetLocation is BaseLandmark) {
				_targetLandmark = (BaseLandmark)_targetLocation;
				_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartVampiricEmbrace ());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}

	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();
		PerformVampiricEmbrace();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			for (int j = 0; j < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; j++) {
				ECS.Character possibleCharacter = location.tileLocation.landmarkOnTile.charactersAtLocation[j].mainCharacter;
				if(possibleCharacter.id != character.id && CanMeetRequirements(possibleCharacter)){
					return true;
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 5;
    }
    protected override WeightedDictionary<Character> GetCharacterTargetWeights(Character character) {
        WeightedDictionary<Character> characterWeights = base.GetCharacterTargetWeights(character);
        List<Region> regionsToCheck = new List<Region>();
        Region regionOfCharacter = character.specificLocation.tileLocation.region;
        regionsToCheck.Add(regionOfCharacter);
        regionsToCheck.AddRange(regionOfCharacter.adjacentRegionsViaMajorRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.charactersInRegion.Count; j++) {
                ECS.Character currChar = character.specificLocation.tileLocation.region.charactersInRegion[j];
                if (currChar.id != character.id) {
                    int weight = 0;
                    if (!currChar.HasTag(CHARACTER_TAG.VAMPIRE)) {
                        if (currChar.HasTag(CHARACTER_TAG.SAPIENT)) {
                            if (currRegion.id != regionOfCharacter.id) {
                                weight += 20; //Non Vampire Sapient Characters in adjacent regions: 20
                            } else {
                                weight += 50; //Non Vampire Sapient Characters in the current region: 50
                            }
                            
                        } else {
                            if (currRegion.id == regionOfCharacter.id) {
                                weight += 5; //Non Vampire Non Sapient Characters in the current region: 5
                            }
                        }
                    }
                    if (weight > 0) {
                        characterWeights.AddElement(currChar, weight);
                    }
                }
            }
        }
        return characterWeights;
    }
    #endregion

    private void StartVampiricEmbrace(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartVampiricEmbrace ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();

		if(_targetCharacter.specificLocation != null && _targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && _targetCharacter.specificLocation.tileLocation.id == _targetLandmark.location.id){
            Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "start");
            startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            startLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            _targetLandmark.AddHistory(startLog);
            _targetCharacter.AddHistory(startLog);
            _assignedCharacter.AddHistory(startLog);
        } else {
			EndVampiricEmbrace ();
		}
	}

	public void PerformVampiricEmbrace() {
		string chosenAction = TaskManager.Instance.vampiricEmbraceActions.PickRandomElementGivenWeights ();
		if(chosenAction == "turn"){
            Log turnLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "turn_success");
            turnLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            turnLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            _targetCharacter.AddHistory(turnLog);
            _assignedCharacter.AddHistory(turnLog);
            _targetLandmark.AddHistory(turnLog);
            _targetCharacter.AssignTag (CHARACTER_TAG.VAMPIRE);
			EndVampiricEmbrace ();
			return;
		}else if(chosenAction == "caught"){
            Log caughtLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "caught");
            caughtLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            caughtLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            _targetCharacter.AddHistory(caughtLog);
            _assignedCharacter.AddHistory(caughtLog);
            _targetLandmark.AddHistory(caughtLog);
            if (!_assignedCharacter.HasTag(CHARACTER_TAG.CRIMINAL)){
				_assignedCharacter.AssignTag (CHARACTER_TAG.CRIMINAL);
			}
			EndVampiricEmbrace ();
			return;
		}
		if(_daysLeft == 0){
			EndVampiricEmbrace ();
			return;
		}
		ReduceDaysLeft(1);
	}
	private void EndVampiricEmbrace(){
		EndTask (TASK_STATUS.SUCCESS);
	}

	private ECS.Character GetTargetCharacter(){
		_characterWeights.Clear ();
		Region region = _assignedCharacter.specificLocation.tileLocation.region;
		for (int i = 0; i < region.allLandmarks.Count; i++) {
			BaseLandmark landmark = region.allLandmarks [i];
			for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
				ECS.Character character = landmark.charactersAtLocation [j].mainCharacter;
				if(character.id != _assignedCharacter.id && CanMeetRequirements(character)){
					_characterWeights.AddElement (character, 5);
				}
			}
		}
		if(_characterWeights.GetTotalOfWeights() > 0){
			return _characterWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
