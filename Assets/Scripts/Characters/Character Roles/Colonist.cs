/*
 Colonists take some civilian population from a village and starts a new village in another area.
 Place functions unique to colonists here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Colonist : CharacterRole {

	public Colonist(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.COLONIST;
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR
        };
        this._canPassHiddenRoads = true;
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
