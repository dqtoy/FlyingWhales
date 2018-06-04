using UnityEngine;
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

    [Space(10)]
    [Header("Special Structures")]
    [SerializeField] private GameObject lycanLair;
	[SerializeField] private GameObject stormWitchLair;
	[SerializeField] private GameObject pereLair;
	[SerializeField] private GameObject ghoulLair;

    [SerializeField] private GameObject landmarkGO;

    internal int[] cityMonthlyMaxGrowthMultiplier = new int[]{1,2,4,8,10,12,14,16,18,20,25};


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

	public BIOMES GetForbiddenBiomeOfRace(RACE race){
		if(race == RACE.HUMANS){
			return BIOMES.FOREST;
		}else if(race == RACE.ELVES){
			return BIOMES.DESERT;
		}else if(race == RACE.CROMADS){
			return BIOMES.NONE;
		}else if(race == RACE.MINGONS){
			return BIOMES.NONE;
		}
		return BIOMES.NONE;
	}

    public GameObject GetLandmarkPrefab(LANDMARK_TYPE landmarkType, RACE race) {
        RaceStructures raceStructuresToUse = _humanStructures;
        if (race == RACE.ELVES) {
            raceStructuresToUse = _elvenStructures;
        }
        STRUCTURE_TYPE neededStructureType = STRUCTURE_TYPE.NONE;
        if (landmarkType == LANDMARK_TYPE.ELVEN_SETTLEMENT) {
            neededStructureType = STRUCTURE_TYPE.GENERIC;
        } else if (landmarkType == LANDMARK_TYPE.IRON_MINES) {
            neededStructureType = STRUCTURE_TYPE.MINE;
        } else if (landmarkType == LANDMARK_TYPE.OAK_LUMBERYARD) {
            neededStructureType = STRUCTURE_TYPE.LUMBERYARD;
        }

        if (neededStructureType != STRUCTURE_TYPE.NONE) {
            Structures[] structuresToChooseFrom = raceStructuresToUse.structures;
            for (int i = 0; i < structuresToChooseFrom.Length; i++) {
                Structures currStructure = structuresToChooseFrom[i];
                if (currStructure.structureType == neededStructureType) {
                    return currStructure.structureGameObjects[Random.Range(0, currStructure.structureGameObjects.Length)];
                }
            }
        }
        return null;
    }

    public GameObject[] GetStructurePrefabsForRace(RACE race, STRUCTURE_TYPE structureType) {
        RaceStructures raceStructuresToUse = _humanStructures;
        if(race == RACE.ELVES) {
            raceStructuresToUse = _elvenStructures;
        } 

        Structures[] structuresToChooseFrom = raceStructuresToUse.structures;
        for (int i = 0; i < structuresToChooseFrom.Length; i++) {
            Structures currStructure = structuresToChooseFrom[i];
            if (currStructure.structureType == structureType) {
                return currStructure.structureGameObjects;
            }
        }
        return null;
    }

    public GameObject GetStructurePrefabForSpecialStructures(LAIR lairType) {
        switch (lairType) {
        case LAIR.LYCAN:
            return lycanLair;
//		case LAIR.STORM_WITCH:
//			return stormWitchLair;
		case LAIR.PERE:
			return pereLair;
		case LAIR.GHOUL:
			return ghoulLair;
        }
        return null;
    }

    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
}
