using UnityEngine;
using System.Collections;

public class GoHome : CharacterTask {
    public GoHome(TaskCreator createdBy) : base(createdBy, TASK_TYPE.GO_HOME) {
        //onQuestAccepted += StartQuestLine;
    }

    #region overrides
	public override void PerformTask() {
        base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
		}

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		if(_assignedCharacter.faction == null) {
            goToLocation.InititalizeAction(((ECS.Character)_createdBy).lair);
        } else {
            goToLocation.InititalizeAction(((ECS.Character)_createdBy).home);
        }
        
        goToLocation.onTaskActionDone += SuccessTask;
        goToLocation.onTaskDoAction += goToLocation.Generic;

		goToLocation.DoAction(_assignedCharacter);
    }
    #endregion

	private void SuccessTask(){
		EndTask (TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
    }
}
