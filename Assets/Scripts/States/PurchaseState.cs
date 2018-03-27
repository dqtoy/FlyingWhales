using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PurchaseState : State {
	private Action _purchaseAction;

	public PurchaseState(CharacterTask parentTask, string identifier): base (parentTask, STATE.PURCHASE){
		if(identifier == "Equipment"){
			_purchaseAction = PurchaseEquipment;
		}
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		if(_purchaseAction != null){
			_purchaseAction ();
		}
		return true;
	}
	#endregion

	private void PurchaseEquipment() {
		if(_targetLandmark is Settlement){
			Settlement settlement = (Settlement)_targetLandmark;
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
					ECS.Item createdItem = settlement.ProduceItemForCharacter(equipmentToAskFor, currChar);
					if (createdItem != null) {
						if(!currChar.EquipItem(createdItem)){ //if the character can equip the item, equip it, otherwise, keep in inventory
							currChar.PickupItem(createdItem); //put item in inventory
						}
                        //Log boughtLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "UpgradeGear", "bought");
                        //boughtLog.AddToFillers(currChar, currChar.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        //boughtLog.AddToFillers(createdItem, createdItem.itemName, LOG_IDENTIFIER.ITEM_1);
                        //boughtLog.AddToFillers(settlement, settlement.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
                        //currChar.AddHistory(boughtLog);
                        //settlement.AddHistory(boughtLog);
                        //currChar.AddHistory("Bought a " + createdItem.itemName + " from " + _settlement.landmarkName);
                        Debug.Log(currChar.name + " bought a " + createdItem.itemName + " from " + settlement.landmarkName);
					}
				}
			}
		}
	}
}
