using UnityEngine;
using System.Collections;
using System;

public class RecruitState : State {
	private string createKey = "Create";
	private string noCreateKey = "No Create";
	private Action _recruitAction;

	public RecruitState(CharacterTask parentTask, string identifier): base (parentTask, STATE.RECRUIT){
		if(identifier == "Followers"){
			_recruitAction = RecruitFollowers;
		}
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		if(_recruitAction != null){
			_recruitAction ();
		}
		return true;
	}
	#endregion

	private void RecruitFollowers(){
		if(_targetLandmark.civilians > 0 && !_assignedCharacter.isFollowersFull) {
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
//				_assignedCharacter.AddFollower(newFollower);
				Log recruitFollowerLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "RecruitFollowers", "recruit");
				recruitFollowerLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				recruitFollowerLog.AddToFillers(newFollower, newFollower.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				_targetLandmark.AddHistory(recruitFollowerLog);
				_assignedCharacter.AddHistory(recruitFollowerLog);
			}
		}
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
