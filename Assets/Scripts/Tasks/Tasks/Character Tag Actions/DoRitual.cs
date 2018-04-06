using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class DoRitual : CharacterTask {
	private BaseLandmark _ritualStones;

	#region getters/setters
	public BaseLandmark ritualStones{
		get { return _ritualStones; }
	}
	#endregion

	public DoRitual(TaskCreator createdBy, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.STEALTHY) : base(createdBy, TASK_TYPE.DO_RITUAL, stance, defaultDaysLeft, parentQuest) {
		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.RITUAL, new RitualState (this) }
		};
	}

	#region overrides
	public override CharacterTask CloneTask (){
		DoRitual clonedTask = new DoRitual(_createdBy, _defaultDaysLeft, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
		return clonedTask;
	}
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget (character);
		}
		if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			ChangeStateTo (STATE.MOVE);
			_ritualStones = _targetLocation as BaseLandmark;
			_assignedCharacter.GoToLocation (_ritualStones, PATHFINDING_MODE.USE_ROADS, () => StartRitual ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}

	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.RITUAL_STONES){
			return true;
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
	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				_landmarkWeights.AddElement (landmark, 5);
			}
		}
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
	public override int GetSelectionWeight(ECS.Character character){ 
		return 175; 
	}
	public override void EndTaskSuccess (){
		_currentState.SetIsHalted (false);
		_currentState.PerformStateAction ();
		base.EndTaskSuccess ();
	}
	#endregion

	private void StartRitual() {
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartRitual ());
//			return;
//		}
        Log startRitualLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "DoRitual", "start_do_ritual");
        startRitualLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _ritualStones.AddHistory(startRitualLog);
        _assignedCharacter.AddHistory(startRitualLog);

		ChangeStateTo (STATE.RITUAL, true);
    }
}