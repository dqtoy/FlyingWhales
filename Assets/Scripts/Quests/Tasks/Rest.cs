using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Rest : CharacterTask {
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
  //  protected override void ConstructQuestLine() {
  //      base.ConstructQuestLine();
  //      Settlement targetSettlement = GetTargetSettlement();

  //      GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
  //      goToLocation.InititalizeAction(targetSettlement.location);
  //      goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
  //      goToLocation.onQuestActionDone += this.PerformNextQuestAction;
  //      goToLocation.onQuestDoAction += goToLocation.Generic;

  //      RestAction restAction = new RestAction(this);
		//restAction.onQuestActionDone += SuccessQuest;
  //      restAction.onQuestDoAction += restAction.StartDailyRegeneration;

  //      _questLine.Enqueue(goToLocation);
  //      _questLine.Enqueue(restAction);

  //  }
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }
        Settlement targetSettlement = GetTargetSettlement();
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		if(targetSettlement == null){
			goToLocation.InititalizeAction(character.currLocation);
		}else{
			goToLocation.InititalizeAction(targetSettlement);
		}
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.USE_ROADS);
        goToLocation.onTaskActionDone += StartRest;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }
    public override void TaskSuccess() {
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
        if(_assignedCharacter.faction == null) {
            _assignedCharacter.UnalignedDetermineAction();
        } else {
            _assignedCharacter.DetermineAction();
        }
	}
    //public override void TaskCancel() {
    //    if (_currentAction != null) {
    //        _currentAction.ActionDone(QUEST_ACTION_RESULT.CANCEL);
    //    }
    //    //TODO: What to do when rest is cancelled?
    //    //RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
    //    //ResetQuestValues();
    //}
    #endregion
	//private void SuccessQuest() {
	//	EndQuest (TASK_RESULT.SUCCESS);
	//}

    private void StartRest() {
        RestAction restAction = new RestAction(this);
        restAction.onTaskActionDone += TaskSuccess;
        restAction.onTaskDoAction += restAction.StartDailyRegeneration;
        restAction.DoAction(_assignedCharacter);
    }
}
