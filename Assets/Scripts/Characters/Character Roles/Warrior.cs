/*
 Warriors defend the place they are staying at. 
 They may also join parties of Warlords and Heroes to perform other quests.
 Place functions unique to warriors here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : CharacterRole {
    
	public Warrior(ECS.Character character): base (character) {
        this.allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this.canPassHiddenRoads = false;
    }
}
