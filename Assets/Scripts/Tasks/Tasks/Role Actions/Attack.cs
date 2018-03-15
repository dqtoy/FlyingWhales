using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Attack : CharacterTask {

	private BaseLandmark _landmarkToAttack;

	#region getters/setters
	public BaseLandmark landmarkToAttack {
		get { return _landmarkToAttack; }
	}
	#endregion

	public Attack(TaskCreator createdBy, int defaultDaysLeft = -1) 
		: base(createdBy, TASK_TYPE.ATTACK, defaultDaysLeft) {
		SetStance(STANCE.COMBAT);
        _alignments.Add(ACTION_ALIGNMENT.HOSTILE);
	}
		
	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
            WeightedDictionary<BaseLandmark> landmarkWeights = GetLandmarkTargetWeights(character);
			_targetLocation = landmarkWeights.PickRandomElementGivenWeights();
		}
		_landmarkToAttack = (BaseLandmark)_targetLocation;
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartAttack());
	}
	public override void PerformTask() {
		base.PerformTask();
		if(!AreThereStillHostileInLandmark()){
			KillCivilians ();
			ChangeLandmarkOwnership ();
			EndAttack ();
			return;
		}
		if(_daysLeft == 0){
			EndAttack ();
			return;
		}
		ReduceDaysLeft(1);
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
            return location.HasHostilitiesWith(character); //If there are unowned landmarks with hostile unaligned characters or owned by hostile faction within current region or adjacent region
			//if(location.tileLocation.landmarkOnTile is DungeonLandmark){
			//	return true;
			//}else if(location.tileLocation.landmarkOnTile is Settlement || location.tileLocation.landmarkOnTile is ResourceLandmark){
			//	if(location.tileLocation.landmarkOnTile.owner != null){
			//		if(character.faction == null){
			//			return true;
			//		}else if(location.tileLocation.landmarkOnTile.owner.id != character.faction.id){
			//			FactionRelationship facRel = character.faction.GetRelationshipWith (location.tileLocation.landmarkOnTile.owner);
			//			if(facRel != null && facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
			//				return true;
			//			}
			//		}
			//	}
			//}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
        //If there are unowned landmarks with hostile unaligned characters or owned by hostile faction within current region or adjacent region
        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(character.specificLocation.tileLocation.region);
        regionsToCheck.AddRange(character.specificLocation.tileLocation.region.adjacentRegionsViaMajorRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.allLandmarks.Count; j++) {
                BaseLandmark landmark = currRegion.allLandmarks[j];
                if (CanBeDone(character, landmark)) {
                    return true;
                }
            }
        }
        
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 40;
    }
    protected override WeightedDictionary<BaseLandmark> GetLandmarkTargetWeights(Character character) {
        WeightedDictionary<BaseLandmark> landmarkWeights =  base.GetLandmarkTargetWeights(character);
        List<Region> regionsToCheck = new List<Region>();
        Region regionOfChar = character.specificLocation.tileLocation.region;
        regionsToCheck.Add(regionOfChar);
        regionsToCheck.AddRange(regionOfChar.adjacentRegionsViaMajorRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.allLandmarks.Count; j++) {
                BaseLandmark landmark = currRegion.allLandmarks[j];
                if (landmark.owner != character.faction) {
                    int weight = 0;
                    if (currRegion.id == regionOfChar.id) {
                        if (landmark.HasHostilitiesWith(character)) {
                            weight += 100; //Each unowned landmark with hostile unaligned characters or owned by hostile faction within current region: 100
                        }
                    } else {
                        if (landmark.HasHostilitiesWith(character)) {
                            weight += 50; //Each unowned landmark with hostile unaligned characters or owned by hostile faction within adjacent regions: 50
                        }
                    }
                    if (weight > 0) {
                        landmarkWeights.AddElement(landmark, weight);
                    }
                }
                
            }
        }
        return landmarkWeights;
    }
    #endregion

    private void StartAttack(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartAttack ());
			return;
		}
		_landmarkToAttack.AddHistory (_assignedCharacter.name + " has started attacking " + _landmarkToAttack.landmarkName + "!");
		_assignedCharacter.AddHistory ("Started attacking " + _landmarkToAttack.landmarkName + "!");

		_assignedCharacter.DestroyAvatar ();
	}

	private bool AreThereStillHostileInLandmark(){
		for (int i = 0; i < _landmarkToAttack.charactersAtLocation.Count; i++) {
			ICombatInitializer character = _landmarkToAttack.charactersAtLocation [i];
			if(character is Party){
				Party party = (Party)character;
				if(party.partyLeader.id != _assignedCharacter.id){
					if(party.faction == null || _assignedCharacter.faction == null){
						return true;
					}else{
						FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (party.faction);
						if(facRel != null){
							if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								return true;
							}
						}else{
							return true;
						}
					}
				}
			}else if (character is ECS.Character){
				ECS.Character currentCharacter = (ECS.Character)character;
				if(currentCharacter.id != _assignedCharacter.id){
					if(currentCharacter.faction == null || _assignedCharacter.faction == null){
						return true;
					}else{
						FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (currentCharacter.faction);
						if(facRel != null){
							if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								return true;
							}
						}else{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	private void KillCivilians(){
		int civiliansInLandmark = _landmarkToAttack.civilians;
		if(civiliansInLandmark > 0){
			int civilianCasualtiesPercentage = UnityEngine.Random.Range (15, 51);
			int civilianCasualties = (int)(((float)civilianCasualtiesPercentage / 100f) * (float)civiliansInLandmark);
			_landmarkToAttack.ReduceCivilians (civilianCasualties);
		}
	}
	private void ChangeLandmarkOwnership(){
		if(_landmarkToAttack is Settlement || _landmarkToAttack is ResourceLandmark){
			_landmarkToAttack.ChangeOwner (_assignedCharacter.faction);
		}
	}
	private void EndAttack(){
		EndTask (TASK_STATUS.SUCCESS);
	}
	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
//			if(landmark is DungeonLandmark){
//				_landmarkWeights.AddElement (landmark, 100);
//			}else{
//				if(landmark is Settlement || landmark is ResourceLandmark){
//					if(landmark.owner != null){
//						FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (landmark.owner);
//						if(facRel != null && facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
//							_landmarkWeights.AddElement (landmark, 100);
//						}
//					}
//				}
//			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
