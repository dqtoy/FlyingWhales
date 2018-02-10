using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class Worker : CharacterRole {
    public Worker(ECS.Character character) : base(character) {
        _roleType = CHARACTER_ROLE.WORKER;
        _allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        _canPassHiddenRoads = true;
        _canAcceptQuests = true;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.BUILD_STRUCTURE,
			QUEST_TYPE.OBTAIN_MATERIAL,
        };
    }

    #region overrides
    internal override WeightedDictionary<CharacterTask> GetActionWeights() {
        WeightedDictionary<CharacterTask> actionWeights = base.GetActionWeights();
        Region currRegionOfCharacter = _character.currLocation.region;

        if (_character.currLocation.landmarkOnTile is Settlement) {
            Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
            //Move to nearest non-hostile Village - 500 if in a hostile Settlement (0 otherwise) (NOTE: this action allows the character to move through hostile regions)
            if (currSettlement.owner.IsHostileWith(_character.faction)) {
                actionWeights.AddElement(new MoveTo(_character, _character.GetNearestNonHostileSettlement(), PATHFINDING_MODE.USE_ROADS), 500);
            }
        }

        for (int i = 0; i < currRegionOfCharacter.adjacentRegionsViaMajorRoad.Count; i++) {
            Region adjRegion = currRegionOfCharacter.adjacentRegionsViaMajorRoad[i];
            Faction regionOwner = adjRegion.owner;
            if (regionOwner != null) {
                if (!regionOwner.IsHostileWith(_character.faction)) {
                    Settlement adjSettlement = (Settlement)adjRegion.centerOfMass.landmarkOnTile;
                    MoveTo moveToNonHostile = new MoveTo(_character, adjSettlement, PATHFINDING_MODE.USE_ROADS);
                    actionWeights.AddElement(moveToNonHostile, GetMoveToNonAdjacentVillageWeight(adjSettlement));
                }
            }
        }

        return actionWeights;
    }

    internal override int GetBuildStructureWeight(BuildStructure buildStructure) {
        return 100;
    }
	internal override int GetObtainMaterialWeight(ObtainMaterial obtainMaterial) {
		return 100;
	}
    #endregion
    
}
