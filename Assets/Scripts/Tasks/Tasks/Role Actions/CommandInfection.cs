using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class CommandInfection : CharacterTask {
	private CraterBeast craterBeast;
	private BaseLandmark _targetLandmark;
	private Character _chosenSlyx;
	private WeightedDictionary<Region> _regionWeights;

	public CommandInfection(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.COMMAND_INFECTION, defaultDaysLeft) {
		SetStance(STANCE.STEALTHY);
		_regionWeights = new WeightedDictionary<Region> ();
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark ();
		}
		for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
			if(character.specificLocation.charactersAtLocation[i] is ECS.Character){
				ECS.Character currCharacter = (ECS.Character) character.specificLocation.charactersAtLocation[i];
				if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
					_chosenSlyx = currCharacter;
					break;
				}
			}
		}
		_targetLandmark = (BaseLandmark)_targetLocation;
		craterBeast = (CraterBeast)_assignedCharacter.role;
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();

		CommandSlyxToInfectLandmark ();

		if(_daysLeft == 0){
			EndCommand();
			return;
		}
		ReduceDaysLeft (1);
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			BaseLandmark landmark = (BaseLandmark)location;
			if(landmark is Settlement){
				for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
					if(character.specificLocation.charactersAtLocation[i] is ECS.Character){
						ECS.Character currCharacter = (ECS.Character) character.specificLocation.charactersAtLocation[i];
						if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
							return true;
						}
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			HexTile center = GridMap.Instance.allRegions [i].centerOfMass;
			if(center.landmarkOnTile != null && center.landmarkOnTile.charactersAtLocation.Count > 0){
				if(CanBeDone(character, center.landmarkOnTile)){
					return true;
				}
			}
		}
		return base.AreConditionsMet (character);
	}
	public override int GetSelectionWeight (Character character){
		return 100;
	}
	#endregion

	private void CommandSlyxToInfectLandmark(){
		_chosenSlyx.GoToLocation (_targetLandmark, PATHFINDING_MODE.USE_ROADS, () => InfectLandmark ());
	}

	private void InfectLandmark(){
		if(_chosenSlyx.isInCombat){
			_chosenSlyx.SetCurrentFunction (() => InfectLandmark ());
		}
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			if(_targetLandmark.charactersAtLocation[i] is Party){
				Party party = (Party)_targetLandmark.charactersAtLocation [i];
				for (int j = 0; j < party.partyMembers.Count; j++) {
					ECS.Character character = party.partyMembers [j];
					if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
						if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
							continue;
						}
						InfectPsytoxin (character);
					}
				}
			}else if(_targetLandmark.charactersAtLocation[i] is ECS.Character){
				ECS.Character character = (ECS.Character) _targetLandmark.charactersAtLocation[i];
				if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
					if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
						continue;
					}
					InfectPsytoxin (character);
				}
			}
		}
		_chosenSlyx.Death ();
		_targetLandmark.AddHistory ("A Slyx has self-destructed emitting a large amount of psytoxin from its body!");
	}
	private void InfectPsytoxin(ECS.Character character){
		ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)character.GetTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
		if(modPsytoxin != null){
			modPsytoxin.TriggerWorsenCase ();
		}else{
			MildPsytoxin mildPsytoxin = (MildPsytoxin)character.GetTag (CHARACTER_TAG.MILD_PSYTOXIN);
			if(mildPsytoxin != null){
				mildPsytoxin.TriggerWorsenCase ();
			}else{
				int chance = Utilities.rng.Next (0, 100);
				if(chance < 80){
					character.AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);	
				}
			}
		}
	}
	private BaseLandmark GetTargetLandmark(){
		_landmarkWeights.Clear ();
		_regionWeights.Clear ();
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region region = GridMap.Instance.allRegions [i];
			int numOfCharacters = region.numOfCharactersInLandmarks;
			if(numOfCharacters > 0){
				_regionWeights.AddElement (region, (numOfCharacters * 5));
			}
		}
		if(_regionWeights.Count > 0){
			Region pickedRegion = _regionWeights.PickRandomElementGivenWeights ();
			for (int i = 0; i < pickedRegion.allLandmarks.Count; i++) {
				BaseLandmark landmark = pickedRegion.allLandmarks [i];
				if(landmark.charactersAtLocation.Count > 0){
					int landmarkWeight = 0;
					for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
						if(landmark.charactersAtLocation[j] is Party){
							Party party = (Party)landmark.charactersAtLocation [j];
							for (int k = 0; k < party.partyMembers.Count; k++) {
								landmarkWeight += CharacterWeight (party.partyMembers [k]);
							}
						}else if(landmark.charactersAtLocation[j] is Character){
							Character character = (Character)landmark.charactersAtLocation [j];
							landmarkWeight += CharacterWeight (character);
						}
					}
					_landmarkWeights.AddElement (landmark, landmarkWeight);
				}
			}
			if(_landmarkWeights.Count > 0){
				return _landmarkWeights.PickRandomElementGivenWeights ();
			}
		}
		return null;
	}

	private int CharacterWeight(Character character){
		if(character.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
			return 3;
		}else if(character.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
			return 8;
		}else if(character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
			return -2;
		}
		return 5;
	}


	private void EndCommand() {
		EndTask(TASK_STATUS.SUCCESS);
	}
}
