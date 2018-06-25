using UnityEngine;
using System.Collections;

public class GoHome : CharacterTask {
	public GoHome(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.GO_HOME, stance, defaultDaysLeft) {
        //onQuestAccepted += StartQuestLine;
    }

    #region overrides
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
		//_assignedCharacter.SetCurrentTask(this);
		//if (_assignedCharacter.party != null) {
		//	_assignedCharacter.party.SetCurrentTask(this);
		//}

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		if(_createdBy is ECS.Character){
			if(_assignedCharacter.faction == null) {
				goToLocation.InitializeAction(((ECS.Character)_createdBy).lair);
			} else {
				//goToLocation.InitializeAction(((ECS.Character)_createdBy).home);
			}
		}
        
        goToLocation.onTaskActionDone += SuccessTask;
        goToLocation.onTaskDoAction += goToLocation.Generic;

		goToLocation.DoAction(_assignedCharacter);
    }
    #endregion

	private void SuccessTask(){
		EndTask (TASK_STATUS.SUCCESS);
    }
}
