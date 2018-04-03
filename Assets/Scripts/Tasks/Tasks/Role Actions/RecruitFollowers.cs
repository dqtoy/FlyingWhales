/*
 Trello link: https://trello.com/c/q29CT0Et/735-recruit-followers
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class RecruitFollowers : CharacterTask {
	private BaseLandmark _targetLandmark;

	public RecruitFollowers(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.RECRUIT_FOLLOWERS, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.RECRUIT, new RecruitState (this, "Followers") }
		};
    }

    #region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget(character);
		}
		if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			ChangeStateTo (STATE.RECRUIT);
			_targetLandmark = _targetLocation as BaseLandmark;
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
		//if(_targetLocation == null){
		//	_targetLocation = GetTargetLandmark (character);
		//}
		//if(_targetLocation != null){
		//	_targetLandmark = (BaseLandmark)_targetLocation;
		//	_assignedCharacter.GoToLocation (_targetLandmark, PATHFINDING_MODE.USE_ROADS);
		//}else{
		//	EndRecruitment ();
		//}
	}
	public override void PerformTask() { //Everyday action of the task
		if(!CanPerformTask()){
			return;
		}
		if(_currentState != null){
			_currentState.PerformStateAction ();
		}
		if (_targetLandmark == null || _targetLandmark.civilians <= 0 || (_assignedCharacter.party != null && _assignedCharacter.party.isFull)) {
			EndTaskSuccess ();
			return;
		}
		if(_daysLeft == 0){
			EndTaskSuccess ();
			return;
		}
		ReduceDaysLeft(1);
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(character.specificLocation != null && character.specificLocation == location){
            BaseLandmark landmarkOnTile = location.tileLocation.landmarkOnTile;
            if (landmarkOnTile != null && landmarkOnTile.owner != null && landmarkOnTile.civilians > 0 && character.faction != null){
				if(!location.HasHostilitiesWith(character.faction)){
					return true;
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
        //If in a location with non-hostile civilians
		if (CanBeDone(character, character.specificLocation)) {
			return true;
        }
        
        //if(character.faction != null){
        //	List<BaseLandmark> ownedLandmarks = character.faction.GetAllOwnedLandmarks ();
        //	for (int i = 0; i < ownedLandmarks.Count; i++) {
        //		if (ownedLandmarks [i].civilians > 0) {
        //			return true;
        //		}
        //	}
        //}
        return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        int weight = base.GetSelectionWeight(character);
        weight += 60 * character.missingFollowers; //+60 for every missing Follower
        return weight;
    }
	protected override BaseLandmark GetLandmarkTarget (Character character){
//		base.GetLandmarkTarget (character);
		if(character.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			return (BaseLandmark)character.specificLocation;
		}
		return null;
	}
    #endregion
}
