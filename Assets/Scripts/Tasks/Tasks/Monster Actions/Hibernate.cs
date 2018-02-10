using UnityEngine;
using System.Collections;

public class Hibernate : CharacterTask {
    public Hibernate(TaskCreator createdBy) : base(createdBy, TASK_TYPE.HIBERNATE) {
    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }
        GoToTargetLocation();
    }
    #endregion

    private void GoToTargetLocation() {
        // The monster will move towards its Lair and then rest there indefinitely
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_assignedCharacter.lair);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
        goToLocation.onTaskActionDone += StartHibernation;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartHibernation() {
        RestAction restAction = new RestAction(this);
        //restAction.onTaskActionDone += TaskSuccess; //rest indefinitely
        restAction.onTaskDoAction += restAction.StartDailyRegeneration;
        restAction.DoAction(_assignedCharacter);
    }

}
