using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Rest : CharacterTask {

    //private RestAction restAction;

    private List<ECS.Character> _charactersToRest;

	#region getters/setters
	public List<Character> charactersToRest{
		get { return _charactersToRest; }
	}
	#endregion

	public Rest(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) 
        : base(createdBy, TASK_TYPE.REST, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.REST, new RestState (this) },
		};
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
            _targetLocation = GetLandmarkTarget(character);
        }
		if (_targetLocation != null) {
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartRest ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		if(_currentState != null){
			_currentState.PerformStateAction ();
		}
		if(!CheckIfCharactersAreFullyRested(_charactersToRest)){
			if(_daysLeft == 0){
				EndTaskSuccess ();
				return;
			}
			ReduceDaysLeft(1);
		}
    }
    public override void TaskSuccess() {
		base.TaskSuccess ();
		//Debug.Log(_assignedCharacter.name + " and party has finished resting on " + Utilities.GetDateString(GameManager.Instance.Today()));
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && character.currentHP < character.maxHP){
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
		if(character.currentHP < character.maxHP){
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
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        int weight = base.GetSelectionWeight(character);
        weight += 100 + (50 * (100 - character.remainingHPPercent)); //100 + (50 x (100 - RemainingHP%))
        return weight;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        Faction factionOfChar = character.faction;
        for (int i = 0; i < regionOfChar.landmarks.Count; i++) {
            BaseLandmark currLandmark = regionOfChar.landmarks[i];
            Faction ownerOfLandmark = currLandmark.owner;
            int weight = 20; //Each landmark in the region: 20
            if (currLandmark is Settlement) {
                weight += 100; //If landmark is a settlement: +100
            }
            if (ownerOfLandmark != null && factionOfChar != null) {
                if (ownerOfLandmark.id == factionOfChar.id || ownerOfLandmark.GetRelationshipWith(factionOfChar).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                    weight += 300; //If landmark is owned by a non-hostile faction: +300
                }
            }
            if (!currLandmark.HasHostileCharactersWith(character)) {
                weight += 100;//If landmark does not have any hostile characters: +100
            }
            if (weight > 0) {
                _landmarkWeights.AddElement(currLandmark, weight);
            }
        }
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
    }
    #endregion

    private void StartRest(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartRest ());
//			return;
//		}
		ChangeStateTo (STATE.REST);
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
}
