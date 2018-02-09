/*
 The hero explores the world for both monsters and treasures. 
 They form party with Warriors to perform their duties. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to heroes here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : CharacterRole {

	public Hero(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.HERO;
        _allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        _canPassHiddenRoads = true;
        _canAcceptQuests = true;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.DEFEND,
            QUEST_TYPE.EXPLORE_TILE,
            QUEST_TYPE.EXPAND
        };
    }

    internal override WeightedDictionary<CharacterTask> GetActionWeights() {
        WeightedDictionary<CharacterTask> actionWeights = base.GetActionWeights();
        Region currRegionOfCharacter = _character.currLocation.region;

        //Upgrade Gear
        UpgradeGear upgradeGearTask = new UpgradeGear(_character);
        actionWeights.AddElement(upgradeGearTask, GetWeightForTask(upgradeGearTask));

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
                    //Move to an adjacent non-hostile Village
                    Settlement adjSettlement = (Settlement)adjRegion.centerOfMass.landmarkOnTile;
                    MoveTo moveToNonHostile = new MoveTo(_character, adjSettlement, PATHFINDING_MODE.USE_ROADS);
                    actionWeights.AddElement(moveToNonHostile, GetMoveToNonAdjacentVillageWeight(adjSettlement));
                }
            }
        }
        
        return actionWeights;
    }

    internal override int GetExpandWeight(Expand expandQuest) {
		int weight = 0;
		List<HexTile> pathToTarget = PathGenerator.Instance.GetPath(_character.currLocation, expandQuest.targetUnoccupiedTile, PATHFINDING_MODE.MAJOR_ROADS);
		if(pathToTarget != null) {
			weight += 200 - (5 * pathToTarget.Count); //200 - (15 per tile distance) if not in a party
		}
		if(weight < 0){
			weight = 0;
		}
		return weight;
	}

    internal override int GetExploreTileWeight(ExploreTile exploreTile) {
        int weight = 0;
        weight += 100;
        return weight;
    }

    internal override int GetUpgradeGearWeight() {
        int weight = 0;
        if (_character.GetNeededEquipmentTypes().Count > 0) { //check if character needs any equipment
            if (_character.gold >= 30) { // 0 if Gold is less than 30g
                if (!_character.HasWeaponEquipped()) {
                    weight += 200; //+200 if missing Weapon
                }
                List<EQUIPMENT_TYPE> missingArmor = _character.GetMissingArmorTypes();
                weight += 50 * missingArmor.Count; //+50 for each missing armor part

                //+20 for every 10g above 30g
                int goldAbove = _character.gold - 30;
                goldAbove /= 10;
                weight += 20 * goldAbove;
            }
        }
        return weight;
    }
}
