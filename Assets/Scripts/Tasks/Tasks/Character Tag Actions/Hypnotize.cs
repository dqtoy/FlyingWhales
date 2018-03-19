using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hypnotize : CharacterTask {

	private ECS.Character _targetCharacter;
	private BaseLandmark _targetLandmark;

	public Hypnotize(TaskCreator createdBy, int defaultDaysLeft = -1) 
		: base(createdBy, TASK_TYPE.HYPNOTIZE, defaultDaysLeft) {
		SetStance(STANCE.STEALTHY);
        _alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
        _needsSpecificTarget = true;
		_specificTargetClassification = "character";
		_filters = new TaskFilter[] {
			new MustNotHaveTags (CHARACTER_TAG.HYPNOTIZED),
		};
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_specificTarget == null) {
            WeightedDictionary<ECS.Character> characterWeights = GetCharacterTargetWeights(character);
			if(characterWeights.GetTotalOfWeights() > 0){
				_specificTarget = characterWeights.PickRandomElementGivenWeights();
			}
        }
		if(_specificTarget != null){
			_targetCharacter = (ECS.Character)_specificTarget;
			if (_targetLocation == null){
				_targetLocation = _targetCharacter.specificLocation;
			}
			if(_targetLocation != null && _targetLocation is BaseLandmark){
				_targetLandmark = (BaseLandmark)_targetLocation;
				_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartHypnotize());
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
		PerformHypnotize();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(character.party == null || (!character.party.isFull && !character.party.isDisbanded)){
			if(location.tileLocation.landmarkOnTile != null){
				for (int j = 0; j < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; j++) {
					ECS.Character possibleCharacter = location.tileLocation.landmarkOnTile.charactersAtLocation[j].mainCharacter;
					if(possibleCharacter.id != character.id && CanMeetRequirements(possibleCharacter)){
						return true;
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		if (character.party == null || (!character.party.isFull && !character.party.isDisbanded)) {
			return true;
		}
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 5 * character.missingFollowers;//+5 for every missing Follower
    }
    protected override WeightedDictionary<Character> GetCharacterTargetWeights(Character character) {
        WeightedDictionary<Character> characterWeights = base.GetCharacterTargetWeights(character);
        for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
            ECS.Character currCharacter = character.specificLocation.tileLocation.region.charactersInRegion[i];
            if (currCharacter.id != character.id) {
                if (CanMeetRequirements(currCharacter)) {
                    characterWeights.AddElement(currCharacter, 50);//Each character in the same region: 50
                }
            }
        }
        return characterWeights;
    }
    #endregion

    private void StartHypnotize(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartHypnotize ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();

		if(_targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && _targetCharacter.specificLocation.tileLocation.id == _targetLandmark.location.id){
			string startLog = _assignedCharacter.name + " wants to hypnotize " + _targetCharacter.name + " into submission!";
			//_targetLandmark.AddHistory (startLog);
			//_targetCharacter.AddHistory (startLog);
			//_assignedCharacter.AddHistory (startLog);
		}else{
			EndHypnotize ();
		}
	}

	public void PerformHypnotize() {
		string chosenAction = TaskManager.Instance.hypnotizeActions.PickRandomElementGivenWeights ();
		if(chosenAction == "hypnotize"){
			//_targetCharacter.AddHistory ("Hypnotized by " + _assignedCharacter.name + "!");
			//_assignedCharacter.AddHistory ("Hypnotized " + _targetCharacter.name + "!");
			//_targetLandmark.AddHistory (_assignedCharacter.name + " hypnotized " + _targetCharacter.name + "!");
			_targetCharacter.AssignTag (CHARACTER_TAG.HYPNOTIZED);
			MakeTargetCharacterAVampireFollower ();
			EndHypnotize ();
			return;
		}
		if(_daysLeft == 0){
			EndHypnotize ();
			return;
		}
		ReduceDaysLeft(1);
	}

	private void MakeTargetCharacterAVampireFollower(){
		if(_assignedCharacter.party == null){
			Party party = _assignedCharacter.CreateNewParty ();
			party.AddPartyMember (_targetCharacter);
		}else{
			_assignedCharacter.party.AddPartyMember (_targetCharacter);
		}
		_targetCharacter.SetFollowerState (true);

		//TODO: What happens if the target character already has a party, is it going to be disbanded? what happens to the followers then?
	}

	private void EndHypnotize(){
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
