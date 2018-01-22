/*
 The warlord defends villages and attacks enemy tribe villages. 
 They form party with Warriors to perform their duties.
 Place functions unique to warlords here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warlord : CharacterRole {

	public Warlord(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.WARLORD;
        this.allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR
        };
        this.canPassHiddenRoads = false;
    }
	internal override int GetDefendWeight(Defend defendQuest) {
		int weight = 0;
		List<HexTile> pathToTarget = PathGenerator.Instance.GetPath(_character.currLocation, defendQuest.landmarkToDefend.location, PATHFINDING_MODE.MAJOR_ROADS);
		if(pathToTarget != null) {
			weight += 200 - (15 * pathToTarget.Count); //200 - (15 per tile distance) if not in a party
		}
		if(weight < 0){
			weight = 0;
		}
		return weight;
	}
	internal override int GetAttackWeight(Attack attackQuest) {
		int weight = 0;
		List<HexTile> pathToTarget = PathGenerator.Instance.GetPath(_character.currLocation, attackQuest.landmarkToAttack.location, PATHFINDING_MODE.MAJOR_ROADS);
		if(pathToTarget != null) {
			weight += 200 - (15 * pathToTarget.Count); //200 - (15 per tile distance) if not in a party
		}
		if(weight < 0){
			weight = 0;
		}
		return weight;
	}
}
