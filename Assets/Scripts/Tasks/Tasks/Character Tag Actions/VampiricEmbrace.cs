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
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		//Get the characters that will rest
		if(_targetLocation == null){
			_targetCharacter = GetTargetCharacter ();
			_targetLocation = _targetCharacter.specificLocation;
//			_targetLocation = GetTargetSettlement();
		}else{
			if(_assignedCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && _assignedCharacter.specificLocation.tileLocation.id == _targetLandmark.location.id){
				List<ICombatInitializer> characters = new List<ICombatInitializer> (_targetLandmark.charactersAtLocation);
				characters.Remove (_assignedCharacter);
				if(characters.Count > 0){
					_targetCharacter = characters [UnityEngine.Random.Range (0, characters.Count)].mainCharacter;
				}
			}else{
				if(_targetLandmark.charactersAtLocation.Count > 0){
					_targetCharacter = _targetLandmark.charactersAtLocation [UnityEngine.Random.Range (0, _targetLandmark.charactersAtLocation.Count)].mainCharacter;
				}
			}
		}
		_targetLandmark = (BaseLandmark)_targetLocation;

		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartVampiricEmbrace());
	}
	public override void PerformTask() {
		base.PerformTask();
		PerformVampiricEmbrace();
	}
	public override void TaskSuccess() {
		base.TaskSuccess ();
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			int condition = 0;
			if (character.specificLocation != null && character.specificLocation.tileLocation.landmarkOnTile.id == location.tileLocation.landmarkOnTile.id) {
				condition = 1;
			}
			if(location.charactersAtLocation.Count > condition){
				return true;
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
			Criminal criminalTag = new Criminal (_assignedCharacter);
			_assignedCharacter.AddCharacterTag (criminalTag);
			EndVampiricEmbrace ();
			return;
		}
		if(_daysLeft == 0){
			EndTask (TASK_STATUS.SUCCESS);
			return;
		}
		ReduceDaysLeft(1);
	}
	private void EndVampiricEmbrace(){
		EndTask (TASK_STATUS.SUCCESS);
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

	private ECS.Character GetTargetCharacter(){
		characterWeights.Clear ();
		Region region = _assignedCharacter.specificLocation.tileLocation.region;
		for (int i = 0; i < region.allLandmarks.Count; i++) {
			BaseLandmark landmark = region.allLandmarks [i];
			for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
				ECS.Character character = landmark.charactersAtLocation [j].mainCharacter;
				if(character.id != _assignedCharacter.id){
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
