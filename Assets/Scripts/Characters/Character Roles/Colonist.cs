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
        this.allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR
        };
        this.canPassHiddenRoads = true;
    }
}
