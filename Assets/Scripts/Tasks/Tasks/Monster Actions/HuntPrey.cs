﻿using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class HuntPrey : CharacterTask {

    private BaseLandmark _target;

	private string hunterName;

	public HuntPrey(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.HUNT_PREY, defaultDaysLeft) {
		SetStance (STANCE.COMBAT);
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_targetLocation == null){
			//TODO: get target location
		}
		_target = (BaseLandmark)_targetLocation;
		hunterName = _assignedCharacter.name;
		if(_assignedCharacter.party != null){
			hunterName = _assignedCharacter.party.name;
		}
		_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartHunt ());

//        TriggerSaveLandmarkQuest();
    }
    public override void PerformTask() {
        base.PerformTask();
        Hunt();
        //GoToTargetLocation();
        
    }
    public override void TaskCancel() {
        base.TaskCancel();
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        _assignedCharacter.DestroyAvatar();
    }
    public override void TaskFail() {
        base.TaskFail();
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        _assignedCharacter.DestroyAvatar();
    }
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        Hunt();
    //    }
    //}
    #endregion

    //private void GoToTargetLocation() {
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InititalizeAction(_target);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartHunt;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;
    //    goToLocation.DoAction(_assignedCharacter);
    //}

    private void StartHunt() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartHunt ());
			return;
		}

		_target.AddHistory(hunterName + " has started hunting prey in " + _target.landmarkName + "!");
    }

    private void Hunt() {
        if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        HUNT_ACTION chosenAct = TaskManager.Instance.huntActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case HUNT_ACTION.EAT:
                EatCivilian();
                break;
            case HUNT_ACTION.END:
				End();
                break;
            case HUNT_ACTION.NOTHING:
                //GameDate nextDate = GameManager.Instance.Today();
                //nextDate.AddDays(1);
                //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
                break;
            default:
                break;
        }
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
    }

    private void EatCivilian() {
        if(_target.civilians > 0) {
			RACE[] races = _target.civiliansByRace.Keys.Where(x => _target.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_target.AdjustCivilians (chosenRace, -1);
			_target.AddHistory (hunterName + " hunted and killed a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian!");
//          _target.ReduceCivilians(1);
            //GameDate nextDate = GameManager.Instance.Today();
            //nextDate.AddDays(1);
            //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
        }
    }

	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        //SetCanDoDailyAction(false);
//        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
