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
        _allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR
        };
        _canPassHiddenRoads = false;
        _canAcceptQuests = true;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.ATTACK,
            QUEST_TYPE.DEFEND,
			QUEST_TYPE.SAVE_LANDMARK,
        };
    }
    internal override WeightedDictionary<CharacterTask> GetActionWeights() {
        WeightedDictionary<CharacterTask> questWeights = base.GetActionWeights();
        //Military Quests
        for (int i = 0; i < _character.faction.militaryManager.activeQuests.Count; i++) {
            Quest currQuest = _character.faction.militaryManager.activeQuests[i];
            if (this.CanAcceptQuest(currQuest) && currQuest.CanAcceptQuest(_character)) { //Check both the quest filters and the quest types this role can accept
                questWeights.AddElement(currQuest, GetWeightForTask(currQuest));
            }
        }
        return questWeights;
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
	internal override int GetSaveLandmarkWeight(ObtainMaterial obtainMaterial) {
		return 200;
	}
}
