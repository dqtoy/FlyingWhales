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
}
