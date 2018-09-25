﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CityGenerator : MonoBehaviour {

	public static CityGenerator Instance = null;

	//public List<HexTile> woodHabitableTiles;
	//public List<HexTile> stoneHabitableTiles;
	//public List<HexTile> lairHabitableTiles;

    //public List<City> allCities;

    [Space(10)]
    [Header("Human Structures")]
    [SerializeField] private RaceStructures _humanStructures;

    [Space(10)]
    [Header("Elven Structures")]
    [SerializeField] private RaceStructures _elvenStructures;

    [SerializeField] private GameObject landmarkGO;


    #region getters/setters
    public RaceStructures humanStructures {
        get { return _humanStructures; }
    }
    public RaceStructures elvenStructures {
        get { return _elvenStructures; }
    }
    #endregion

    void Awake(){
		Instance = this;
	}

    //public GameObject GetLandmarkPrefab(LANDMARK_TYPE landmarkType, RACE race) {
    //    RaceStructures raceStructuresToUse = _humanStructures;
    //    if (race == RACE.ELVES) {
    //        raceStructuresToUse = _elvenStructures;
    //    }
    //    STRUCTURE_TYPE neededStructureType = STRUCTURE_TYPE.NONE;
    //    if (landmarkType == LANDMARK_TYPE.ELVEN_SETTLEMENT) {
    //        neededStructureType = STRUCTURE_TYPE.GENERIC;
    //    } else if (landmarkType == LANDMARK_TYPE.IRON_MINES) {
    //        neededStructureType = STRUCTURE_TYPE.MINE;
    //    } else if (landmarkType == LANDMARK_TYPE.OAK_LUMBERYARD) {
    //        neededStructureType = STRUCTURE_TYPE.LUMBERYARD;
    //    }

    //    if (neededStructureType != STRUCTURE_TYPE.NONE) {
    //        Structures[] structuresToChooseFrom = raceStructuresToUse.structures;
    //        for (int i = 0; i < structuresToChooseFrom.Length; i++) {
    //            Structures currStructure = structuresToChooseFrom[i];
    //            if (currStructure.structureType == neededStructureType) {
    //                return currStructure.structureGameObjects[Random.Range(0, currStructure.structureGameObjects.Length)];
    //            }
    //        }
    //    }
    //    return null;
    //}    
    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
}
