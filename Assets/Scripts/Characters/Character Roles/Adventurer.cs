using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Adventurer : CharacterRole {

	public Adventurer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.ADVENTURER;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.JOIN_PARTY
        };
    }

	internal override int GetJoinPartyWeight(JoinParty joinParty) {
		if(_character.party != null) {
			return 0; //if already in a party
		}
		int weight = 0;
		if(joinParty.partyToJoin.partyLeader.currLocation.id == _character.currLocation.id) {
			//party leader and this character are at the same tile
			return 200;
		} else {
			List<HexTile> pathToParty = PathGenerator.Instance.GetPath(_character.currLocation, joinParty.partyToJoin.partyLeader.currLocation, PATHFINDING_MODE.USE_ROADS);
			if(pathToParty != null) {
				weight += 200 - (15 * pathToParty.Count); //200 - (15 per tile distance) if not in a party
			}
		}
		return Mathf.Max(0, weight);
	}
}
