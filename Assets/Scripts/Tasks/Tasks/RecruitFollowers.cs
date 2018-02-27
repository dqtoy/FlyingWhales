/*
 Trello link: https://trello.com/c/q29CT0Et/735-recruit-followers
 */
using UnityEngine;
using System.Collections;
using ECS;

public class RecruitFollowers : CharacterTask {

    private string createKey = "Create";
    private string noCreateKey = "No Create";

	public RecruitFollowers(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.RECRUIT_FOLLOWERS, defaultDaysLeft) {
        SetStance(STANCE.NEUTRAL); //Recruit Followers is a Neutral Stance action.
    }

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
        if (_assignedCharacter.specificLocation is HexTile) {
            throw new System.Exception(_assignedCharacter.name + " is at a hextile rather than a landmark!");
        }
        BaseLandmark location = _assignedCharacter.specificLocation as BaseLandmark;
		if(location.civilians > 0) {
			WeightedDictionary<string> recruitActions = GetRecruitmentDictionary(location); 

			string chosenAction = recruitActions.PickRandomElementGivenWeights();
			if (chosenAction.Equals(createKey)) {
				//Create Follower For character
				ECS.Character newFollower = location.CreateNewFollower();
				Party party = _assignedCharacter.party;
				if(party == null) {
					party = _assignedCharacter.CreateNewParty();
				}
				party.AddPartyMember(newFollower);
//				_assignedCharacter.AddFollower(newFollower);
			}
			if ((_assignedCharacter.party != null && _assignedCharacter.party.isFull)) {
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
