using UnityEngine;
using System.Collections;

public class UpgradeGear : CharacterTask {

    private Settlement _settlement;

    public UpgradeGear(TaskCreator createdBy) : base(createdBy, TASK_TYPE.UPGRADE_GEAR) {

    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }
        _settlement = character.GetNearestNonHostileSettlement();
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_settlement);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
        goToLocation.onTaskActionDone += PurchaseEquipment;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }
    public override void TaskSuccess() {
        Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
        if (_assignedCharacter.faction == null) {
            _assignedCharacter.UnalignedDetermineAction();
        } else {
            _assignedCharacter.DetermineAction();
        }
    }
    #endregion

    private void PurchaseEquipment() {
        //TODO: Purchase equipment from the settlement
        EndTask(TASK_STATUS.SUCCESS);
    }
}
