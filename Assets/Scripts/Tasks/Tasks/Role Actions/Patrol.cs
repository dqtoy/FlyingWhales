using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Patrol : CharacterTask {

	private BaseLandmark _landmarkToPatrol;

	#region getters/setters
	public BaseLandmark landmarkToPatrol {
		get { return _landmarkToPatrol; }
	}
	#endregion

	public Patrol(TaskCreator createdBy, int defaultDaysLeft = -1) 
		: base(createdBy, TASK_TYPE.PATROL, defaultDaysLeft) {
		SetStance(STANCE.COMBAT);
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark();
		}
		_landmarkToPatrol = (BaseLandmark)_targetLocation;
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartPatrol());
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();
		if(_daysLeft == 0){
			EndPatrol ();
			return;
		}
		ReduceDaysLeft(1);
	}
	public override bool CanBeDone (ECS.Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			if (location.tileLocation.landmarkOnTile is Settlement || location.tileLocation.landmarkOnTile is ResourceLandmark) {			
				if (character.faction != null && location.tileLocation.landmarkOnTile.owner != null) {
					if (location.tileLocation.landmarkOnTile.owner.id == character.faction.id) {
						return true;
					}					
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
	#endregion

	private void StartPatrol(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartPatrol ());
			return;
		}
		_landmarkToPatrol.AddHistory (_assignedCharacter.name + " has started patrolling around " + _landmarkToPatrol.landmarkName + "!");
		_assignedCharacter.AddHistory ("Started patrolling around " + _landmarkToPatrol.landmarkName + "!");

		_assignedCharacter.DestroyAvatar ();
	}
	private void EndPatrol(){
		EndTask (TASK_STATUS.SUCCESS);
	}
	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
//			if(landmark is Settlement || landmark is ResourceLandmark){
//				if(landmark.owner != null && landmark.owner.id == _assignedCharacter.faction.id){
//					_landmarkWeights.AddElement (landmark, 100);
//				}
//			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
