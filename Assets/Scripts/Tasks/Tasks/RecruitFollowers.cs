/*
 Trello link: https://trello.com/c/q29CT0Et/735-recruit-followers
 */
using UnityEngine;
using System.Collections;
using ECS;

public class RecruitFollowers : CharacterTask {

    public RecruitFollowers(TaskCreator createdBy, TASK_TYPE taskType) : base(createdBy, taskType) {
        SetStance(STANCE.NEUTRAL); //Recruit Followers is a Neutral Stance action.
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
        //Once triggered, the character will be in Recruit Followers stance for 5 days or until the location has no more civilians
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(5);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndRecruitment());
        Messenger.AddListener("OnDayEnd", CheckCivilians);
    }
    public override void PerformTask() {
        base.PerformTask();
        if (_assignedCharacter.specificLocation is HexTile) {
            throw new System.Exception(_assignedCharacter.name + " is at a hextile rather than a landmark!");
        }

        string createKey = "Create";
        string noCreateKey = "No Create";
        int createWeight = 100; //100 Base Weight to Create New Follower
        int noCreateWeight = 200; //200 Base Weight to Not Create New Follower
        WeightedDictionary<string> recruitActions = new WeightedDictionary<string>();
        
        BaseLandmark location = _assignedCharacter.specificLocation as BaseLandmark;
        int minimumCivilianReq = location.GetMinimumCivilianRequirement();
        if (location.civilians <= minimumCivilianReq) {
            createWeight -= 300; //-300 Weight to Create New Follower if civilian count is equal or less than Minimum Cap
            createWeight = Mathf.Max(0, createWeight);
        } else {
            createWeight += 5 * (minimumCivilianReq - location.civilians); //+5 Weight to Create New Follower for each civilian over Minimum Cap
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

        string chosenAction = recruitActions.PickRandomElementGivenWeights();
        if (chosenAction.Equals(createKey)) {
            //Create Follower For character
            ECS.Character newFollower = location.CreateNewFollower();
            Party party = _assignedCharacter.party;
            if(party == null) {
                party = _assignedCharacter.CreateNewParty();
            }
            party.AddPartyMember(newFollower);
        }

        if ((_assignedCharacter.party != null && _assignedCharacter.party.isFull)) {
            EndRecruitment();
        }
    }
    #endregion

    private void EndRecruitment() {
        //End Rucruit Followers stance, and determine next action.
        EndTask(TASK_STATUS.SUCCESS);
    }

    private void CheckCivilians() {
        BaseLandmark location = _assignedCharacter.specificLocation as BaseLandmark;
        if(location.civilians == 0) {
            //The landmark has no more civilians
            EndRecruitment();
        }
    }
}
