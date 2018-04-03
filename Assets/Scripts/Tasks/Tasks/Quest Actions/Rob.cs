using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Rob : CharacterTask {
	private string _itemNameToRob;
	private Character _targetCharacter;

	public Rob(TaskCreator createdBy, string itemNameToRob, Quest parentQuest = null, STANCE stance = STANCE.COMBAT) : base(createdBy, TASK_TYPE.ROB, stance, -1, parentQuest) {
		_alignments.Add (ACTION_ALIGNMENT.UNLAWFUL);
		_specificTargetClassification = "character";
		_needsSpecificTarget = true;
		_itemNameToRob = itemNameToRob;
		_filters = new TaskFilter[] {
			new MustHaveItem(_itemNameToRob),
		};
		_states = new Dictionary<STATE, State> {
			{STATE.MOVE, new MoveState(this)},
			{STATE.ATTACK, new AttackState(this, () => RobItem())}
		};
		_forGameOnly = true;
		SetCombatPriority (30);
	}

	#region overrides
	public override void OnChooseTask(Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if (_specificTarget == null) {
			_specificTarget = GetCharacterTarget(character);
		}
		if(_specificTarget != null && _specificTarget is Character){
			_targetCharacter = _specificTarget as Character;
			if (_targetLocation == null) {
				_targetLocation = _targetCharacter.specificLocation;
			}
			if (_targetLocation != null) {
				ChangeStateTo(STATE.MOVE);
				_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartRob());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone(Character character, ILocation location) {
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
			foreach (Character targetCharacter in character.traceInfo.Keys) {
				if(CanMeetRequirements(targetCharacter)){
					if(targetCharacter.specificLocation == location){
						return true;
					}
				}
			}
		}
		return base.CanBeDone(character, location);
	}
	public override bool AreConditionsMet(Character character) {
		foreach (Character targetCharacter in character.traceInfo.Keys) {
			if(targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && CanMeetRequirements(targetCharacter)){
				if(targetCharacter.currentRegion.id == character.currentRegion.id || character.IsCharacterInAdjacentRegionOfThis(targetCharacter)){
					return true;
				}
			}
		}
		return base.AreConditionsMet(character);
	}
	public override int GetSelectionWeight(Character character) {
		return 500; //160
	}
	protected override Character GetCharacterTarget(Character character) {
		base.GetCharacterTarget(character);
		foreach (Character targetCharacter in character.traceInfo.Keys) {
			if(targetCharacter.specificLocation is BaseLandmark && CanMeetRequirements(targetCharacter)){
				if(targetCharacter.currentRegion.id == character.currentRegion.id){
					_characterWeights.AddElement (targetCharacter, 200);
				}else if (character.IsCharacterInAdjacentRegionOfThis(targetCharacter)){
					_characterWeights.AddElement (targetCharacter, 80);
				}
			}
		}
        LogTargetWeights(_characterWeights);
        if (_characterWeights.GetTotalOfWeights() > 0) {
			return _characterWeights.PickRandomElementGivenWeights();
		}
		return null;
	}
	public override void TaskSuccess (){
		_isDone = true;
		if (_parentQuest != null) {
			if (_assignedCharacter.questData.activeQuest.phases.Count > _assignedCharacter.questData.currentPhase + 1) {
				_assignedCharacter.questData.AdvanceToNextPhase();
			} else {
				//there are no more phases, end the quest
				_assignedCharacter.questData.EndQuest(TASK_STATUS.SUCCESS);
			}
		}
		_assignedCharacter.DetermineAction();
	}
	#endregion
	private void StartRob(){
		if(_assignedCharacter.specificLocation == _targetCharacter.specificLocation){
			ChangeStateTo (STATE.ATTACK);
		}else{
			EndTaskFail ();
		}
	}
	private void RobItem(){
		_assignedCharacter.AssignTag (CHARACTER_TAG.CRIMINAL);
		if(_assignedCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			BaseLandmark landmark = _assignedCharacter.specificLocation as BaseLandmark;
			bool hasRobbedSuccessfully = false;
			for (int i = 0; i < landmark.itemsInLandmark.Count; i++) {
				Item item = landmark.itemsInLandmark [i];
				if(item.itemName == _itemNameToRob){
					_assignedCharacter.PickupItem (item);
					landmark.RemoveItemInLandmark (item);
					hasRobbedSuccessfully = true;
					break;
				}
			}
			if(hasRobbedSuccessfully){
				EndTaskSuccess ();
			}else{
				EndTaskFail ();
			}
		}else{
			EndTaskFail ();
		}

	}
}
