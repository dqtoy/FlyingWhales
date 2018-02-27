using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

	private string pillagerName;

	public Pillage(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.PILLAGE, defaultDaysLeft) {
		SetStance (STANCE.COMBAT);
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_targetLocation == null){
			//TODO: get target location
		}
		_target = (BaseLandmark)_targetLocation;
		pillagerName = _assignedCharacter.name;
		if(_assignedCharacter.party != null){
			pillagerName = _assignedCharacter.party.name;
		}
		_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.NORMAL, () => StartPillage ());
//        TriggerSaveLandmarkQuest();
    }
    public override void PerformTask() {
        base.PerformTask();
        //GoToTargetLocation();
        DoPillage();
        
    }
    public override void TaskCancel() {
        base.TaskCancel();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
//		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
    }
    public override void TaskFail() {
        base.TaskFail();
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
//		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
    }
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        DoPillage();
    //    }
    //}
    #endregion

    private void StartPillage() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartPillage ());
			return;
		}

		_target.AddHistory(pillagerName + " has started pillaging " + _target.landmarkName + "!");
    }

    private void DoPillage() {
        if (this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        PILLAGE_ACTION chosenAct = TaskManager.Instance.pillageActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
			case PILLAGE_ACTION.OBTAIN_ITEM:
				ObtainItem ();
                break;
            case PILLAGE_ACTION.END:
                End();
                break;
			case PILLAGE_ACTION.CIVILIAN_DIES:
				CivilianDies();
				break;
            case PILLAGE_ACTION.NOTHING:
            default:
                break;
        }
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
    }
	private void ObtainItem(){
		if(_target.itemsInLandmark.Count > 0){
			Item chosenItem = _target.itemsInLandmark [UnityEngine.Random.Range (0, _target.itemsInLandmark.Count)];
			if(!_assignedCharacter.EquipItem(chosenItem)){
				_assignedCharacter.PickupItem (chosenItem);
			}
			_target.itemsInLandmark.Remove (chosenItem);
		}
	}
	private void CivilianDies(){
//		int civilians = _target.civilians;
		if(_target.civilians > 0){
			RACE[] races = _target.civiliansByRace.Keys.Where(x => _target.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_target.AdjustCivilians (chosenRace, -1);
			_target.AddHistory (pillagerName + " killed a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian while pillaging!");
		}
	}
	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        //Messenger.RemoveListener("OnDayEnd", DoPillage);
        //SetCanDoDailyAction(false);
//        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
