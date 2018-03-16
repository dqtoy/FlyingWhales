/*
 Trello link: https://trello.com/c/q29CT0Et/735-recruit-followers
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class RecruitFollowers : CharacterTask {

    private string createKey = "Create";
    private string noCreateKey = "No Create";

	private BaseLandmark _targetLandmark;

	public RecruitFollowers(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.RECRUIT_FOLLOWERS, defaultDaysLeft) {
        SetStance(STANCE.NEUTRAL); //Recruit Followers is a Neutral Stance action.
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
		if(_targetLocation != null && _targetLocation is BaseLandmark){
			_targetLandmark = (BaseLandmark)_targetLocation;
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
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();

		if(_targetLandmark != null && _targetLandmark.civilians > 0 && _targetLandmark is Settlement && !_assignedCharacter.isFollowersFull) {
			Settlement settlement = (Settlement)_targetLandmark;
			WeightedDictionary<string> recruitActions = GetRecruitmentDictionary(_targetLandmark); 
			string chosenAction = recruitActions.PickRandomElementGivenWeights();
			if (chosenAction.Equals(createKey)) {
				//Create Follower For character
				ECS.Character newFollower = settlement.CreateNewFollower();
                Party party = _assignedCharacter.party;
                if (party == null) {
                    party = _assignedCharacter.CreateNewParty();
                }
                party.AddPartyMember(newFollower);
                _assignedCharacter.AddFollower(newFollower);
            }
			if (_assignedCharacter.isFollowersFull) {
				EndRecruitment();
				return;
			}
//			if(_assignedCharacter.isFollowersFull){
//				EndRecruitment();
//				return;
//			}
		}else{
			EndRecruitment();
			return;
		}
		if(_daysLeft == 0){
			EndRecruitment();
			return;
		}
		ReduceDaysLeft (1);
    }
	public override bool CanBeDone (Character character, ILocation location){
		if(character.specificLocation != null && character.specificLocation == location){
			if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0 && character.faction != null){
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
        if (!character.isFollowersFull) {
            weight += 60 * character.missingFollowers; //+60 for every missing Follower
        }
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

    private void EndRecruitment() {
        //End Rucruit Followers stance, and determine next action.
        EndTask(TASK_STATUS.SUCCESS);
    }

    private WeightedDictionary<string> GetRecruitmentDictionary(BaseLandmark location) {
        int createWeight = 100; //100 Base Weight to Create New Follower
        int noCreateWeight = 200; //200 Base Weight to Not Create New Follower
        WeightedDictionary<string> recruitActions = new WeightedDictionary<string>();

        
        int minimumCivilianReq = location.GetMinimumCivilianRequirement();
        if (location.civilians <= minimumCivilianReq) {
            createWeight -= 300; //-300 Weight to Create New Follower if civilian count is equal or less than Minimum Cap
            createWeight = Mathf.Max(0, createWeight);
        } else {
            createWeight += 5 * (location.civilians - minimumCivilianReq); //+5 Weight to Create New Follower for each civilian over Minimum Cap
        }
        if (_assignedCharacter.HasTrait(TRAIT.CHARISMATIC)) {
            createWeight += 50; //+50 Weight to Create New Follower if character is Charismatic
        }
        if (location.owner != null && _assignedCharacter.faction != null && location.owner.id == _assignedCharacter.faction.id) {
            createWeight += 50; //+50 Weight to Create New Follower if character is same faction as landmark
        }
        if (_assignedCharacter.HasTrait(TRAIT.REPULSIVE)) {
            noCreateWeight += 50; //+50 Weight to Not Create New Follower if character is Repulsive
        }
        recruitActions.AddElement(createKey, createWeight);
        recruitActions.AddElement(noCreateKey, noCreateWeight);

        return recruitActions;
    }
}
