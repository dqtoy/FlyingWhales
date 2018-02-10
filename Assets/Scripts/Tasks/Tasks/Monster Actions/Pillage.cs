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
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
    }

    private void GoToTargetLocation() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_target);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
        goToLocation.onTaskActionDone += StartPillage;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartPillage() {
        _target.AddHistory("Monster " + _assignedCharacter.name + " has started pillaging " + _target.landmarkName);
        GameDate nextDate = GameManager.Instance.Today();
        nextDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(nextDate, () => DoPillage());
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
                EndTask(TASK_STATUS.SUCCESS);
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
            EndTask(TASK_STATUS.SUCCESS);
        }
        
    }
}
