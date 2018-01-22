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

	public Hero(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.HERO;
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this._canPassHiddenRoads = true;
        this._allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.DEFEND,
            QUEST_TYPE.EXPLORE_TILE,
            QUEST_TYPE.EXPAND
        };
    }
}
