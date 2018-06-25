using UnityEngine;
using System.Collections;
using ECS;
using System.Linq;

public class MoveTowardsCharacter : CharacterTask {
	private Item _traceItem;
	private Character _traceCharacter;

	#region getters/setters
	public HexTile targetTile {
		get { return _targetLocation.tileLocation; }
	}
	#endregion

	public MoveTowardsCharacter(TaskCreator createdBy, Item traceItem, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.NEUTRAL) 
		: base(createdBy, TASK_TYPE.MOVE_TOWARDS_CHARACTER, stance, defaultDaysLeft, parentQuest) {

		_traceItem = traceItem;
		_traceCharacter = null;

		_states = new System.Collections.Generic.Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) }
		};

		_forGameOnly = true;
	}

	#region overrides
	public override CharacterTask CloneTask (){
		MoveTowardsCharacter clonedTask = new MoveTowardsCharacter(_createdBy, _traceItem, _defaultDaysLeft, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
		return clonedTask;
	}
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
			ChangeStateTo(STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => SuccessTask());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone (ECS.Character character, ILocation location){
		return true;
	}
	public override bool AreConditionsMet (ECS.Character character){
		return true;
	}
	public override int GetSelectionWeight(Character character) {
		if(_traceItem != null){
			Character characterLookingFor = character.GetCharacterFromTraceInfo (_traceItem.itemName);
			_traceCharacter = characterLookingFor;
			if(characterLookingFor != null){
				for (int i = 0; i < character.currentRegion.adjacentRegionsViaRoad.Count; i++) {
					if(character.currentRegion.adjacentRegionsViaRoad[i].id == characterLookingFor.currentRegion.id){
						return 150;
					}
				}
			}
		}
		return 20;
	}
	protected override BaseLandmark GetLandmarkTarget(Character character) {
		base.GetLandmarkTarget(character);
		Character characterLookingFor = _traceCharacter;
		//bool hasTrace = false;
		if(_traceItem != null && _traceCharacter == null && _traceItem.possessor is Character){
			characterLookingFor = _traceItem.possessor as Character;
		}else{
			characterLookingFor = _specificTarget as Character;
		}

		if(characterLookingFor != null){
			if(_traceCharacter != null){
				Region regionOfChar = character.specificLocation.tileLocation.region;
				for (int i = 0; i < regionOfChar.adjacentRegionsViaRoad.Count; i++) {
					Region adjacentRegion = regionOfChar.adjacentRegionsViaRoad [i];
					if (characterLookingFor.currentRegion.id == adjacentRegion.id) {
						_landmarkWeights.AddWeightToElement (adjacentRegion.mainLandmark, 100);
					}
				}
			}
			_landmarkWeights.AddWeightToElement (characterLookingFor.currentRegion.mainLandmark, 50);
		}
		LogTargetWeights(_landmarkWeights);
		if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
	#endregion

	private void SuccessTask() {
		DoNothing doNothing = new DoNothing(_assignedCharacter, 3);
		doNothing.SetLocation(_targetLocation);
		//doNothing.SetDaysLeft (3);
		//doNothing.OnChooseTask(_assignedCharacter);
		_assignedCharacter.SetTaskToDoNext(doNothing);
		EndTask(TASK_STATUS.SUCCESS);
	}
}
