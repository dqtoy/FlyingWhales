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
    }
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);

    }
    #endregion
}
