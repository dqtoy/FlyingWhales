using UnityEngine;
using System.Collections;

public class Hibernate : CharacterTask {

    private RestAction restAction;

    public Hibernate(TaskCreator createdBy) : base(createdBy, TASK_TYPE.HIBERNATE) {
    }

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
        _assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        GoToTargetLocation();
    }
    public override void PerformDailyAction() {
        if (_canDoDailyAction) {
            base.PerformDailyAction();
            restAction.DoAction(_assignedCharacter);
        }
    }
    public override void EndTask(TASK_STATUS taskResult) {
        SetCanDoDailyAction(false);
        base.EndTask(taskResult);
    }
    #endregion

    private void GoToTargetLocation() {
        // The monster will move towards its Lair and then rest there indefinitely
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_assignedCharacter.lair);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
        goToLocation.onTaskActionDone += StartHibernation;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartHibernation() {
        SetCanDoDailyAction(true);
        restAction = new RestAction(this);
        //restAction.onTaskActionDone += TaskSuccess; //rest indefinitely
        restAction.onTaskDoAction += restAction.RestIndefinitely;
        restAction.DoAction(_assignedCharacter);
    }

}
