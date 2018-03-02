using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class VampiricEmbrace : CharacterTask {

	private ECS.Character _targetCharacter;
	private BaseLandmark _targetLandmark;
	private WeightedDictionary<ECS.Character> characterWeights;

	public VampiricEmbrace(TaskCreator createdBy, int defaultDaysLeft = -1) 
		: base(createdBy, TASK_TYPE.VAMPIRIC_EMBRACE, defaultDaysLeft) {
		SetStance(STANCE.STEALTHY);
		characterWeights = new WeightedDictionary<ECS.Character> ();
		_needsSpecificTarget = true;
		_specificTargetClassification = "character";
		_filters = new QuestFilter[] {
			new MustNotHaveTags (CHARACTER_TAG.VAMPIRE),
		};
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_targetLocation == null){
			_targetLocation = _targetCharacter.specificLocation;
		}
		if(_specificTarget == null){
			_specificTarget = GetTargetCharacter ();
		}
		_targetCharacter = (ECS.Character)_specificTarget;
		_targetLandmark = (BaseLandmark)_targetLocation;

		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartVampiricEmbrace());
	}
	public override void PerformTask() {
		base.PerformTask();
		PerformVampiricEmbrace();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			for (int j = 0; j < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; j++) {
				ECS.Character possibleCharacter = location.tileLocation.landmarkOnTile.charactersAtLocation[j].mainCharacter;
				if(possibleCharacter.id != character.id && !possibleCharacter.HasTag(CHARACTER_TAG.VAMPIRE)){
					return true;
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	#endregion

	private void StartVampiricEmbrace(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartVampiricEmbrace ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();

		if(_targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && _targetCharacter.specificLocation.tileLocation.id == _targetLandmark.location.id){
			string startLog = _assignedCharacter.name + " wants to turn " + _targetCharacter.name + " into a vampire!";
			_targetLandmark.AddHistory (startLog);
			_targetCharacter.AddHistory (startLog);
			_assignedCharacter.AddHistory (startLog);
		}else{
			EndVampiricEmbrace ();
		}
	}

	public void PerformVampiricEmbrace() {
		string chosenAction = TaskManager.Instance.vampiricEmbraceActions.PickRandomElementGivenWeights ();
		if(chosenAction == "turn"){
			_targetCharacter.AddHistory ("Turned into vampire by " + _assignedCharacter.name + "!");
			_assignedCharacter.AddHistory ("Turned " + _targetCharacter.name + " into a vampire!");
			_targetLandmark.AddHistory (_assignedCharacter.name + " turned " + _targetCharacter.name + " into a vampire!");
			Vampire vampireTag = new Vampire (_targetCharacter);
			_targetCharacter.AddCharacterTag (vampireTag);
			EndVampiricEmbrace ();
			return;
		}else if(chosenAction == "caught"){
			_targetCharacter.AddHistory (_assignedCharacter.name + " got caught trying to turn " + _targetCharacter.name + " into a vampire!");
			_assignedCharacter.AddHistory ("Caught trying to turn " + _targetCharacter.name + " into a vampire!");
			_targetLandmark.AddHistory (_assignedCharacter.name + " got caught trying to turn " + _targetCharacter.name + " into a vampire!");
			if(!_assignedCharacter.HasTag(CHARACTER_TAG.CRIMINAL)){
				Criminal criminalTag = new Criminal (_assignedCharacter);
				_assignedCharacter.AddCharacterTag (criminalTag);
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
		characterWeights.Clear ();
		Region region = _assignedCharacter.specificLocation.tileLocation.region;
		for (int i = 0; i < region.allLandmarks.Count; i++) {
			BaseLandmark landmark = region.allLandmarks [i];
			for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
				ECS.Character character = landmark.charactersAtLocation [j].mainCharacter;
				if(character.id != _assignedCharacter.id && !character.HasTag(CHARACTER_TAG.VAMPIRE)){
					characterWeights.AddElement (character, 5);
				}
			}
		}
		if(characterWeights.GetTotalOfWeights() > 0){
			return characterWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
