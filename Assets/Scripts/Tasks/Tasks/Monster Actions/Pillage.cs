using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

    private enum PILLAGE_ACTION { TAKE_RESOURCE, END, NOTHING }

    private WeightedDictionary<PILLAGE_ACTION> pillageActions;

    public Pillage(TaskCreator createdBy, BaseLandmark target) 
        : base(createdBy, TASK_TYPE.PILLAGE) {
        _target = target;
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
        pillageActions = new WeightedDictionary<PILLAGE_ACTION>();
        pillageActions.AddElement(PILLAGE_ACTION.TAKE_RESOURCE, 15);
        pillageActions.AddElement(PILLAGE_ACTION.END, 15);
        pillageActions.AddElement(PILLAGE_ACTION.NOTHING, 70);
        _target.AddHistory("Monster " + _assignedCharacter.name + " has started pillaging " + _target.landmarkName + ".");
        TriggerSaveLandmarkQuest();
    }
    public override void PerformTask() {
        base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        //GoToTargetLocation();
        DoPillage();
        
    }
    public override void TaskCancel() {
        base.TaskCancel();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
    }
    public override void TaskFail() {
        base.TaskFail();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
    }
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        DoPillage();
    //    }
    //}
    #endregion

    //private void GoToTargetLocation() {
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InititalizeAction(_target);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartPillage;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;

    //    goToLocation.DoAction(_assignedCharacter);
    //}

  //  private void StartPillage() {
  //      _target.AddHistory("Monster " + _assignedCharacter.name + " has started pillaging " + _target.landmarkName + ".");
  //      //SetCanDoDailyAction(true);
  //      //Messenger.AddListener("OnDayEnd", DoPillage);
  //      //GameDate nextDate = GameManager.Instance.Today();
  //      //nextDate.AddDays(1);
  //      //SchedulingManager.Instance.AddEntry(nextDate, () => DoPillage());
		//TriggerSaveLandmarkQuest ();
  //  }

    private void DoPillage() {
        if (this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        PILLAGE_ACTION chosenAct = pillageActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case PILLAGE_ACTION.TAKE_RESOURCE:
                //TakeResource(); //TODO: Change to take item!
                break;
            case PILLAGE_ACTION.END:
                End();
                break;
            case PILLAGE_ACTION.NOTHING:
                //GameDate nextDate = GameManager.Instance.Today();
                //nextDate.AddDays(1);
                //SchedulingManager.Instance.AddEntry(nextDate, () => DoPillage());
                break;
            default:
                break;
        }
    }
   // private void TakeResource() {
   //     Dictionary<MATERIAL, MaterialValues> elligibleMaterials = _target.materialsInventory.Where(x => x.Value.count > 0).ToDictionary(x => x.Key, y => y.Value);
   //     if (elligibleMaterials.Count > 0) {
   //         MATERIAL randomResource = elligibleMaterials.Keys.ElementAt(Random.Range(0, elligibleMaterials.Count));
   //         int randomAmount = Random.Range(1, elligibleMaterials[randomResource].count + 1);
   //         _target.AdjustMaterial(randomResource, randomAmount);
   //     } else {
			//End();
   //     }
        
   // }
	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        //SetCanDoDailyAction(false);
        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
