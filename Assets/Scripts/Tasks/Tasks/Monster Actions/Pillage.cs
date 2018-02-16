using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

    private enum PILLAGE_ACTION { TAKE_RESOURCE, END, NOTHING }

    private WeightedDictionary<PILLAGE_ACTION> pillageActions;

    public Pillage(TaskCreator createdBy, BaseLandmark target) 
        : base(createdBy, TASK_TYPE.PILLAGE) {
        _target = target;
    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }
        GoToTargetLocation();
        pillageActions = new WeightedDictionary<PILLAGE_ACTION>();
        pillageActions.AddElement(PILLAGE_ACTION.TAKE_RESOURCE, 15);
        pillageActions.AddElement(PILLAGE_ACTION.END, 15);
        pillageActions.AddElement(PILLAGE_ACTION.NOTHING, 70);
    }
    public override void TaskCancel() {
        base.TaskCancel();
        Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
    }
    public override void TaskFail() {
        base.TaskFail();
        Messenger.RemoveListener("OnDayEnd", DoPillage);
        _assignedCharacter.DestroyAvatar();
		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
    }
    #endregion

    private void GoToTargetLocation() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_target);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
        goToLocation.onTaskActionDone += StartPillage;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartPillage() {
        _target.AddHistory("Monster " + _assignedCharacter.name + " has started pillaging " + _target.landmarkName + ".");
        Messenger.AddListener("OnDayEnd", DoPillage);
        //GameDate nextDate = GameManager.Instance.Today();
        //nextDate.AddDays(1);
        //SchedulingManager.Instance.AddEntry(nextDate, () => DoPillage());
		TriggerSaveLandmarkQuest ();
    }

    private void DoPillage() {
        if (this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        PILLAGE_ACTION chosenAct = pillageActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case PILLAGE_ACTION.TAKE_RESOURCE:
                TakeResource();
                break;
            case PILLAGE_ACTION.END:
                End();
                break;
            case PILLAGE_ACTION.NOTHING:
                GameDate nextDate = GameManager.Instance.Today();
                nextDate.AddDays(1);
                SchedulingManager.Instance.AddEntry(nextDate, () => DoPillage());
                break;
            default:
                break;
        }
    }
    private void TakeResource() {
        Dictionary<MATERIAL, MaterialValues> elligibleMaterials = _target.materialsInventory.Where(x => x.Value.count > 0).ToDictionary(x => x.Key, y => y.Value);
        if (elligibleMaterials.Count > 0) {
            MATERIAL randomResource = elligibleMaterials.Keys.ElementAt(Random.Range(0, elligibleMaterials.Count));
            int randomAmount = Random.Range(1, elligibleMaterials[randomResource].count + 1);
            _target.AdjustMaterial(randomResource, randomAmount);
        } else {
			End();
        }
        
    }
	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        Messenger.RemoveListener("OnDayEnd", DoPillage);
        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
