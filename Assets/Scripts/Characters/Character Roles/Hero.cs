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
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this._canPassHiddenRoads = true;
        this._allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.DEFEND,
            QUEST_TYPE.EXPLORE_TILE,
            QUEST_TYPE.EXPAND
        };
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
}
