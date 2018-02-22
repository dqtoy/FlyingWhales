using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Rest : CharacterTask {

    private RestAction restAction;

    public Rest(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.REST) {
        
    }

    private Settlement GetTargetSettlement() {
        ECS.Character character = (ECS.Character)_createdBy;
		if (character.faction != null) {
			List<Settlement> factionSettlements = character.faction.settlements.OrderBy (x => Vector2.Distance (character.currLocation.transform.position, x.location.transform.position)).ToList ();
			for (int i = 0; i < factionSettlements.Count; i++) {
				Settlement currSettlement = factionSettlements [i];
				if (PathGenerator.Instance.GetPath (character.currLocation, currSettlement.location, PATHFINDING_MODE.USE_ROADS) != null) {
					return currSettlement;
				}
			}
		}
        
        return null;
    }

    #region overrides
    public override void PerformTask() {
		base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        Settlement targetSettlement = GetTargetSettlement();
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		if(targetSettlement == null){
			goToLocation.InititalizeAction(_assignedCharacter.specificLocation);
		}else{
			goToLocation.InititalizeAction(targetSettlement);
		}
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
        goToLocation.onTaskActionDone += StartRest;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }
    public override void TaskSuccess() {
        SetCanDoDailyAction(false);
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
		_assignedCharacter.DetermineAction();
	}
    public override void PerformDailyAction() {
        if (_canDoDailyAction) {
            base.PerformDailyAction();
            PerformRest();
        }
    }
    #endregion

    private void StartRest() {
        SetCanDoDailyAction(true);
        restAction = new RestAction(this);
        restAction.InititalizeAction(_assignedCharacter);
        restAction.onTaskActionDone += TaskSuccess;
        restAction.onTaskDoAction += restAction.Rest;
    }

    private void PerformRest() {
        restAction.DoAction(_assignedCharacter);
    }
}
