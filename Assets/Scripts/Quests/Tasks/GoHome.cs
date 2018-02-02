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
    #endregion

	private void SuccessTask(){
		EndTask (TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
    }
}
