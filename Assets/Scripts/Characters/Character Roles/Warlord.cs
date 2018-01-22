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
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR
        };
        this._canPassHiddenRoads = false;
        this._allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.ATTACK,
            QUEST_TYPE.DEFEND
        };
    }
}
