using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Rest : CharacterTask {

    private RestAction restAction;

    private List<ECS.Character> _charactersToRest;

	public Rest(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.REST, defaultDaysLeft) {
		SetStance(STANCE.NEUTRAL);
    }
		
    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        //Get the characters that will rest
        _charactersToRest = new List<ECS.Character>();
        if (character.party != null) {
            _charactersToRest.AddRange(character.party.partyMembers);
        } else {
            _charactersToRest.Add(character);
        }
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark(character);
		}
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartRest());
    }
    public override void PerformTask() {
		base.PerformTask();
        PerformRest();
    }
    public override void TaskSuccess() {
		base.TaskSuccess ();
		Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			if(character.faction == null){
				BaseLandmark home = character.home;
				if(home == null){
					home = character.lair;
				}
				if(home != null && location.tileLocation.landmarkOnTile.id == home.id){
					return true;
				}
			}else{
				if(location.tileLocation.landmarkOnTile is Settlement && location.tileLocation.landmarkOnTile.owner != null){
					if(location.tileLocation.landmarkOnTile.owner.id == character.faction.id){
						return true;
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		if(character.faction == null){
			BaseLandmark home = character.home;
			if(home == null){
				home = character.lair;
			}
			if(home != null){
				return true;
			}
		}else{
			if(character.faction.settlements.Count > 0){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    #endregion

	private void StartRest(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartRest ());
			return;
		}
		for (int i = 0; i < _charactersToRest.Count; i++) {
			_charactersToRest[i].AddHistory("Taking a rest.");
		}
		_assignedCharacter.DestroyAvatar ();
	}
	private bool CheckIfCharactersAreFullyRested(List<ECS.Character> charactersToRest) {
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
		return allCharactersRested;
    }

    public void PerformRest() {
        for (int i = 0; i < _charactersToRest.Count; i++) {
            ECS.Character currCharacter = _charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
		if(!CheckIfCharactersAreFullyRested(_charactersToRest)){
			if(_daysLeft == 0){
				EndTask (TASK_STATUS.SUCCESS);
				return;
			}
			ReduceDaysLeft(1);
		}
    }

	private BaseLandmark GetTargetLandmark(Character character) {
		if (character.faction != null) {
			List<Settlement> factionSettlements = character.faction.settlements.OrderBy (x => Vector2.Distance (character.currLocation.transform.position, x.location.transform.position)).ToList ();
			for (int i = 0; i < factionSettlements.Count; i++) {
				Settlement currSettlement = factionSettlements [i];
				if (PathGenerator.Instance.GetPath (character.currLocation, currSettlement.location, PATHFINDING_MODE.USE_ROADS) != null) {
					return currSettlement;
				}
			}
		}else{
			BaseLandmark home = character.home;
			if(home == null){
				home = character.lair;
			}
			return home;
		}
		return null;
	}

}
