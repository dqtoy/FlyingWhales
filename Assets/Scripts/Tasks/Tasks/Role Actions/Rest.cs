using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Rest : CharacterTask {

    private RestAction restAction;

    private List<ECS.Character> _charactersToRest;

    public Rest(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.REST) {
		SetStance(STANCE.NEUTRAL);
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
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
        //Get the characters that will rest
        _charactersToRest = new List<ECS.Character>();
        if (character.party != null) {
            _charactersToRest.AddRange(character.party.partyMembers);
        } else {
            _charactersToRest.Add(character);
        }
        for (int i = 0; i < _charactersToRest.Count; i++) {
            _charactersToRest[i].AddHistory("Taking a rest.");
        }
    }
    public override void PerformTask() {
		base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        PerformRest();
        //if(_targetLocation == null){
        //	_targetLocation = GetTargetSettlement();
        //}
        //_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP, () => StartRest ());
    }
    public override void TaskSuccess() {
        //SetCanDoDailyAction(false);
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
		_assignedCharacter.DetermineAction();
	}
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        PerformRest();
    //    }
    //}
    #endregion

    //private void StartRest() {
    //    //SetCanDoDailyAction(true);
    //    restAction = new RestAction(this);
    //    restAction.InititalizeAction(_assignedCharacter);
    //    restAction.onTaskActionDone += TaskSuccess;
    //    restAction.onTaskDoAction += restAction.Rest;
    //}

    //private void PerformRest() {
    //    restAction.DoAction(_assignedCharacter);
    //}

    private void CheckIfCharactersAreFullyRested(List<ECS.Character> charactersToRest) {
        bool allCharactersRested = true;
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            if (!currCharacter.IsHealthFull()) {
                allCharactersRested = false;
                break;
            }
        }
        if (allCharactersRested) {
            EndTask(TASK_STATUS.SUCCESS);
        }
    }

    public void PerformRest() {
        for (int i = 0; i < _charactersToRest.Count; i++) {
            ECS.Character currCharacter = _charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
        CheckIfCharactersAreFullyRested(_charactersToRest);
    }
}
