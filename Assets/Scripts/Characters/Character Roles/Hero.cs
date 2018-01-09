/*
 The hero explores the world for both monsters and treasures. 
 They form party with Warriors to perform their duties. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to heroes here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : CharacterRole {

    public Hero() {
        _roleType = CHARACTER_ROLE.HERO;
        this.allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this.canPassHiddenRoads = true;
    }
}
