/*
 The trader obtains resources for the village. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to traders here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trader : CharacterRole {

	public Trader(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.TRADER;
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this._canPassHiddenRoads = true;
    }
}
