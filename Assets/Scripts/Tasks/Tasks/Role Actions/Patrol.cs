using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark();
		}
		_landmarkToPatrol = (BaseLandmark)_targetLocation;
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartPatrol());
	}
	public override void PerformTask() {
		base.PerformTask();
		if(_daysLeft == 0){
			EndPatrol ();
			return;
		}
		ReduceDaysLeft(1);
	}
	#endregion

	private void StartPatrol(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartPatrol ());
			return;
		}
		_landmarkToPatrol.AddHistory (_assignedCharacter.name + " has started patrolling around " + _landmarkToPatrol.landmarkName + "!");
		_assignedCharacter.DestroyAvatar ();
	}
	private void EndPatrol(){
		EndTask (TASK_STATUS.SUCCESS);
	}
	private BaseLandmark GetTargetLandmark() {
		WeightedDictionary<BaseLandmark> landmarkWeights = new WeightedDictionary<BaseLandmark> ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(landmark is Settlement || landmark is ResourceLandmark){
				if(landmark.owner != null && landmark.owner.id == _assignedCharacter.faction.id){
					landmarkWeights.AddElement (landmark, 100);
				}
			}
		}
		if(landmarkWeights.GetTotalOfWeights() > 0){
			return landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
