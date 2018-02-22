using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpgradeGear : CharacterTask {

    private Settlement _settlement;

    public UpgradeGear(TaskCreator createdBy) : base(createdBy, TASK_TYPE.UPGRADE_GEAR) {

    }
    #region overrides
    public override void PerformTask() {
        base.PerformTask();
        _assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
		if(_targetLocation == null){
			_targetLocation = _assignedCharacter.GetNearestNonHostileSettlement();
		}
		_settlement = (Settlement)_targetLocation;
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, () => SchedulePurchaseEquipment ());
    }
    //public override void TaskSuccess() {
    //    if (_assignedCharacter.faction == null) {
    //        _assignedCharacter.UnalignedDetermineAction();
    //    } else {
    //        _assignedCharacter.DetermineAction();
    //    }
    //}
    #endregion

	private void SchedulePurchaseEquipment(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (1);
		SchedulingManager.Instance.AddEntry (newSched, () => PurchaseEquipment ());
	}
    private void PurchaseEquipment() {
        List<ECS.Character> charactersToPurchase = new List<ECS.Character>();
        if(_assignedCharacter.party == null) {
            charactersToPurchase.Add(_assignedCharacter);
        } else {
            charactersToPurchase.AddRange(_assignedCharacter.party.partyMembers);
        }
        //Purchase equipment from the settlement
        for (int i = 0; i < charactersToPurchase.Count; i++) {
            ECS.Character currChar = charactersToPurchase[i];
            List<EQUIPMENT_TYPE> neededEquipment = currChar.GetNeededEquipmentTypes();
            for (int j = 0; j < neededEquipment.Count; j++) {
                if (currChar.gold <= 0) {
                    //the curr character no longer has any money
                    break;
                }
                EQUIPMENT_TYPE equipmentToAskFor = neededEquipment[j];
                ECS.Item createdItem = _settlement.ProduceItemForCharacter(equipmentToAskFor, currChar);
                if (createdItem != null) {
                    currChar.PickupItem(createdItem); //put item in inventory
                    currChar.EquipItem(createdItem); //if the character can equip the item, equip it, otherwise, keep in inventory
                    Debug.Log(currChar.name + " bought a " + createdItem.itemName + " from " + _settlement.landmarkName);
                }
            }
        }
        EndTask(TASK_STATUS.SUCCESS);
    }
}
