/*
 The head of a tribe. 
 The chieftain performs various actions like defending his cities and attacking enemy cities. 
 He also explores his tribe territory. He creates various Quests for other characters.
 This role is considered to be the highest of roles (Like a King).
 Place functions unique to chieftains here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chieftain : CharacterRole {

    public Chieftain() {
        this.allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this.canPassHiddenRoads = true;
    }
}
