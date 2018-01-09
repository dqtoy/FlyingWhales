/*
 The head of a village. 
 The Village Head stays in his village and mostly creates Quests intended to help the village.
 Place functions unique to village heads here.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VillageHead : CharacterRole {

    public VillageHead() {
        _roleType = CHARACTER_ROLE.VILLAGE_HEAD;
        this.allowedRoadTypes = new List<ROAD_TYPE>();
        this.canPassHiddenRoads = false;
    }
}
