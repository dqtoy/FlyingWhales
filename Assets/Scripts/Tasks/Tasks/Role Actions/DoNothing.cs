using UnityEngine;
using System.Collections;
using ECS;
using System;

public class DoNothing : CharacterTask {

    private Action endAction;
    private GameDate endDate;

	public DoNothing(TaskCreator createdBy, int defaultDaysLeft = 10) 
        : base(createdBy, TASK_TYPE.DO_NOTHING, defaultDaysLeft) {
        //_actionString = "to dilly dally at";
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        if (_targetLocation == null) {
            _targetLocation = GetLandmarkTarget(character);
        }
		if (_targetLocation != null) {
			character.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
		if(_daysLeft == 0){
			EndTask (TASK_STATUS.SUCCESS);
			return;
		}
		ReduceDaysLeft(1);
    }
	public override bool AreConditionsMet (Character character){
		return true;
	}
    public override int GetSelectionWeight(Character character) {
        return 200;
    }
    protected override BaseLandmark GetLandmarkTarget(ECS.Character character) {
        base.GetLandmarkTarget(character);
        Region regionOfCharacter = character.specificLocation.tileLocation.region;
        Faction factionOfCharacter = character.faction;
        for (int i = 0; i < regionOfCharacter.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionOfCharacter.allLandmarks[i];
            Faction ownerOfLandmark = currLandmark.owner;
            int weight = 20; //Each landmark in the current region gets a base weight of 20
            if (character.faction != null) {
                if (currLandmark.owner != null) {
                    if (currLandmark.owner.id == character.faction.id) {
                        weight += 100; //Each landmark in the current region owned by the same faction as the character: +100
                        if (currLandmark is Settlement) {
                            weight += 200; //Each settlement-type landmark in the current region owned by the same faction as the character: +200
                        }
                    } else {
                        //currLandmark is owned by another faction
                        FactionRelationship rel = factionOfCharacter.GetRelationshipWith(ownerOfLandmark);
                        if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                            weight -= 50; //Each landmark in the current region owned by hostile faction: -100
                        }
                    }
                    if (currLandmark == character.specificLocation) {
                        if (currLandmark.owner.id == character.faction.id || factionOfCharacter.GetRelationshipWith(ownerOfLandmark).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                            weight += 1000; //If current location is owned by a non-hostile faction: +500
                        }
                    }
                }
            } else {
                //character is factionless
                if (currLandmark.owner == null) {
                    weight += 300; //If character is unaligned, each landmark not owned by any faction: +300
                    if (currLandmark == character.specificLocation) {
                        weight += 1000; //If character is unaligned and current location is not owned by any faction: +500
                    }
                }
            }
            if (currLandmark.HasHostilitiesWith(character)) {
                weight -= 50; //Each landmark in the current region with hostile characters: -50
            }
            if (weight > 0) {
                _landmarkWeights.AddElement(currLandmark, weight);
            }
        }

		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
    }
    #endregion
}
