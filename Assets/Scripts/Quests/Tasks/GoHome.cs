using UnityEngine;
using System.Collections;

public class GoHome : CharacterTask {
    public GoHome(TaskCreator createdBy) : base(createdBy, TASK_TYPE.GO_HOME) {
        //onQuestAccepted += StartQuestLine;
    }

    #region overrides
	public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
		character.SetCurrentTask(this);
		if (character.party != null) {
			character.party.SetCurrentTask(this);
		}

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(((ECS.Character)_createdBy).home.location);
        goToLocation.onTaskActionDone += SuccessTask;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(character);
    }
    public override void TaskSuccess() {
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
        _assignedCharacter.DetermineAction();
	}
  //  protected override void ConstructQuestLine() {
  //      base.ConstructQuestLine();

  //      GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
  //      goToLocation.InititalizeAction(((ECS.Character)_createdBy).home.location);
		//goToLocation.onTaskActionDone += SuccessTask;
  //      goToLocation.onTaskDoAction += goToLocation.Generic;

  //      _questLine.Enqueue(goToLocation);
  //  }
  //  internal override void QuestSuccess() {
  //      if(_assignedParty == null) {
  //          //When the character has gone home, determine the next action
  //          ECS.Character character = ((ECS.Character)_createdBy);
  //          character.DestroyAvatar();
  //          character.DetermineAction();
  //      } else {
  //          RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
  //      }
  //  }
    #endregion

	private void SuccessTask(){
		EndTask (TASK_RESULT.SUCCESS);
	}
}
