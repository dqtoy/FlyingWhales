using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class PillageState : State {

	public PillageState(CharacterTask parentTask): base (parentTask, STATE.PILLAGE){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Pillage ();
		return true;
	}
	#endregion

	private void Pillage(){
		PILLAGE_ACTION chosenAct = TaskManager.Instance.pillageActions.PickRandomElementGivenWeights();
		switch (chosenAct) {
		case PILLAGE_ACTION.OBTAIN_ITEM:
			ObtainItem ();
			break;
		case PILLAGE_ACTION.END:
			_parentTask.EndTaskSuccess();
			break;
		case PILLAGE_ACTION.CIVILIAN_DIES:
			//CivilianDies();
			break;
		case PILLAGE_ACTION.NOTHING:
		default:
			break;
		}
	}

    private void ObtainItem() {
        if (_targetLandmark.itemsInLandmark.Count > 0) {
            Item chosenItem = _targetLandmark.itemsInLandmark[UnityEngine.Random.Range(0, _targetLandmark.itemsInLandmark.Count)];
            _targetLandmark.RemoveItemInLandmark(chosenItem);
            if (!_assignedCharacter.EquipItem(chosenItem)) {
                _assignedCharacter.PickupItem(chosenItem);
            }
        }
    }
}
