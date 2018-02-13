using UnityEngine;
using System.Collections;

public class HuntPrey : CharacterTask {

    private BaseLandmark _target;

    private enum HUNT_ACTION { EAT, END, NOTHING }

    private WeightedDictionary<HUNT_ACTION> huntActions;

    public HuntPrey(TaskCreator createdBy, BaseLandmark target) 
        : base(createdBy, TASK_TYPE.HUNT_PREY) {
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
        huntActions = new WeightedDictionary<HUNT_ACTION>();
        huntActions.AddElement(HUNT_ACTION.EAT, 15);
        huntActions.AddElement(HUNT_ACTION.END, 15);
        huntActions.AddElement(HUNT_ACTION.NOTHING, 70);
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
    }

    private void GoToTargetLocation() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_target);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
        goToLocation.onTaskActionDone += StartHunt;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartHunt() {
        _target.AddHistory("Monster " + _assignedCharacter.name + " is hunting for food");
        GameDate nextDate = GameManager.Instance.Today();
        nextDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
    }

    private void Hunt() {
        if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        HUNT_ACTION chosenAct = huntActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case HUNT_ACTION.EAT:
                EatCivilian();
                break;
            case HUNT_ACTION.END:
                EndTask(TASK_STATUS.SUCCESS);
                break;
            case HUNT_ACTION.NOTHING:
                GameDate nextDate = GameManager.Instance.Today();
                nextDate.AddDays(1);
                SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
                break;
            default:
                break;
        }
    }

    private void EatCivilian() {
        _target.AdjustPopulation(-1);
        if(_target.civilians > 0) {
            GameDate nextDate = GameManager.Instance.Today();
            nextDate.AddDays(1);
            SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
        } else {
            EndTask(TASK_STATUS.SUCCESS);
        }
    }
}
