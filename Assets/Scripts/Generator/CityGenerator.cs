using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class CityGenerator : MonoBehaviour {

	public static CityGenerator Instance = null;

	public List<HexTile> woodHabitableTiles;
	public List<HexTile> stoneHabitableTiles;
	public List<HexTile> lairHabitableTiles;

    [Space(10)]
    [Header("Human Structures")]
    [SerializeField] private RaceStructures humanStructures;

    [Space(10)]
    [Header("Elven Structures")]
    [SerializeField] private RaceStructures elvenStructures;

    [Space(10)]
    [Header("Special Structures")]
    [SerializeField] private GameObject lycanLair;
	[SerializeField] private GameObject stormWitchLair;

    //public GameObject[] genericStructures;
    //public GameObject[] cityStructures;
    //public GameObject[] mineStructures;
    //public GameObject[] lumberyardStructures;
    //public GameObject[] quarryStructures;
    //public GameObject[] huntingLodgeStructures;
    //public GameObject[] farmStructures;

    public TextAsset cityBehaviourTree;

	void Awake(){
		Instance = this;
	}

	public void GenerateHabitableTiles(List<GameObject> allHexes){
		woodHabitableTiles = new List<HexTile>();
		stoneHabitableTiles = new List<HexTile>();

		int nearbyStoneCount, nearbyWoodCount, nearbySpecialCount;

		List<GameObject> elligibleTiles = new List<GameObject>(allHexes);
//		Debug.Log ("elligible Tiles: " + elligibleTiles.Count.ToString ());
		for (int i = 0; i < elligibleTiles.Count; i++) {
			nearbyStoneCount = 0;
			nearbyWoodCount = 0;
			nearbySpecialCount = 0;

			HexTile currentHexTile = elligibleTiles [i].GetComponent<HexTile>();
			if (currentHexTile.xCoordinate >= (GridMap.Instance.width - 3) || currentHexTile.xCoordinate < 3 || 
				currentHexTile.yCoordinate >= (GridMap.Instance.width - 3) || currentHexTile.yCoordinate < 3 ||
				currentHexTile.elevationType == ELEVATION.WATER || currentHexTile.elevationType == ELEVATION.MOUNTAIN || 
				currentHexTile.specialResource != RESOURCE.NONE) {
				//skip hextiles within 3 tiles of the edge
				continue;
			}


			List<HexTile> checkForHabitableTilesInRange = currentHexTile.GetTilesInRange (4);
			if (checkForHabitableTilesInRange.Where (x => x.isHabitable).Count () > 0) {
				continue;
			}

			List<HexTile> tilesInRange = currentHexTile.GetTilesInRange(3);
			for (int j = 0; j < tilesInRange.Count; j++) {
				if (tilesInRange [j].specialResource != RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (tilesInRange [j].specialResource) == BASE_RESOURCE_TYPE.STONE) {
						nearbyStoneCount++;
					} else if (Utilities.GetBaseResourceType (tilesInRange [j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
						nearbyWoodCount++;
					} else {
						nearbySpecialCount++;
					}
					currentHexTile.nearbyResourcesCount++;
				}
			}

			if (nearbyStoneCount + nearbySpecialCount >= 2) {
				SetTileAsStoneHabitable(currentHexTile);
			}

			if (nearbyWoodCount + nearbySpecialCount >= 2) {
				SetTileAsWoodHabitable(currentHexTile);
			}

			elligibleTiles.Remove(currentHexTile.gameObject);
			for (int j = 0; j < tilesInRange.Count; j++) {
				if (elligibleTiles.Contains (tilesInRange [j].gameObject)) {
					elligibleTiles.Remove (tilesInRange [j].gameObject);
				}
			}
		}
	}
	public void GenerateLairHabitableTiles(List<GameObject> allHexes){
		this.lairHabitableTiles = new List<HexTile>();
		for (int i = 0; i < allHexes.Count; i++) {
			HexTile currentHexTile = allHexes [i].GetComponent<HexTile>();
			if(currentHexTile.isOccupied || currentHexTile.isHabitable || currentHexTile.elevationType == ELEVATION.WATER || currentHexTile.elevationType == ELEVATION.MOUNTAIN || currentHexTile.specialResource != RESOURCE.NONE || currentHexTile.gameEventInTile != null){
				continue;
			}
			List<HexTile> checkForHabitableTilesInRange = currentHexTile.GetTilesInRange (3);
			if (checkForHabitableTilesInRange.FirstOrDefault(x => x.isHabitable) != null) {
				continue;
			}
			SetTileAsLairHabitable(currentHexTile);
		}
	}

	private void SetTileAsStoneHabitable(HexTile hexTile){
		hexTile.isHabitable = true;
		stoneHabitableTiles.Add(hexTile);
		//hexTile.GetComponent<SpriteRenderer>().color = Color.black;
	}

	private void SetTileAsWoodHabitable(HexTile hexTile){
		hexTile.isHabitable = true;
		woodHabitableTiles.Add(hexTile);
		//hexTile.GetComponent<SpriteRenderer>().color = Color.black;
	}
	private void SetTileAsLairHabitable(HexTile hexTile){
		this.lairHabitableTiles.Add(hexTile);
	}

	// This will return the nearest habitable tile that matches the following criteria
	//	- unoccupied
	//	- has a nearby basic resource needed by the expanding race
	//	- has path from capital city
	public HexTile GetNearestHabitableTile(City city) {
		BIOMES forbiddenBiome = GetForbiddenBiomeOfRace(city.kingdom.race);
		for (int i = 0; i < city.habitableTileDistance.Count; i++) {
			if (!city.habitableTileDistance[i].hexTile.isOccupied && !city.habitableTileDistance[i].hexTile.isBorder && !city.habitableTileDistance[i].hexTile.isTargeted 
				&& city.habitableTileDistance[i].hexTile.biomeType != forbiddenBiome) {

				List<HexTile> checkForOtherBorderTilesInRange;
				// Check if the tile is within required distance of the expanding kingdom's current borders
				if (city.kingdom.kingdomTypeData.expansionDistanceFromBorder > 0) {
					List<HexTile> checkForExpandingKingdomBorderTilesInRange = city.habitableTileDistance [i].hexTile.GetTilesInRange (city.kingdom.kingdomTypeData.expansionDistanceFromBorder);
					int z = checkForExpandingKingdomBorderTilesInRange.Where (y => (y.ownedByCity != null && y.ownedByCity.kingdom == city.kingdom)).Count ();
					if (z <= 0) {
						continue;
					}

					// Check if there are more than 2 nearby (within 3 hex tiles) hex tiles that are already part of another kingdom
					checkForOtherBorderTilesInRange = city.habitableTileDistance [i].hexTile.GetTilesInRange (4);
					if (checkForOtherBorderTilesInRange.Where (x => (x.ownedByCity != null && x.ownedByCity.kingdom != city.kingdom)).Count () > 1) {
						continue;
					} else {
						return city.habitableTileDistance [i].hexTile;
					}
				} else {
					// Check if there are more than 2 nearby (within 3 hex tiles) hex tiles that are already part of any kingdom (including own)
					checkForOtherBorderTilesInRange = city.habitableTileDistance [i].hexTile.GetTilesInRange (4);
					if (checkForOtherBorderTilesInRange.Where (x => (x.ownedByCity != null)).Count () > 1) {
						continue;
					} else {
						return city.habitableTileDistance [i].hexTile;
					}					
				}
			}
		}

		return null;
	}

    public HexTile GetExpandableTileForKingdom(Kingdom kingdom) {
        BIOMES forbiddenBiomeForKingdom = GetForbiddenBiomeOfRace(kingdom.race);
        List<HexTile> elligibleTiles = GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())
            .Where(x => x.isHabitable && !x.isOccupied && x.biomeType != forbiddenBiomeForKingdom && x.tag == kingdom.capitalCity.hexTile.tag 
            && kingdom.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN).ToList();
        if(elligibleTiles.Count > 0) {
            elligibleTiles = elligibleTiles.OrderBy(x => kingdom.capitalCity.hexTile.GetDistanceTo(x)).ToList();
            return elligibleTiles.FirstOrDefault();
        }
        return null;
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
	public City CreateNewCity(HexTile hexTile, Kingdom kingdom, Rebellion rebellion = null){
        if (hexTile.isBorder) {
            hexTile.ownedByCity.borderTiles.Remove(hexTile);
            hexTile.isBorderOfCityID = 0;
            hexTile.isBorder = false;
            hexTile.ownedByCity = null;
        }
        if (rebellion != null){
			hexTile.city = new RebelFort (hexTile, kingdom, rebellion);
		}else{
			hexTile.city = new City (hexTile, kingdom);
		}
        //hexTile.ShowCitySprite();
        hexTile.CreateStructureOnTile(STRUCTURE_TYPE.CITY);
		//hexTile.ShowNamePlate();
		if (hexTile.gameObject.GetComponent<CityTaskManager> () != null) {
			Destroy (hexTile.gameObject.GetComponent<CityTaskManager> ());
		}
		if (hexTile.gameObject.GetComponent<PandaBehaviour> () != null) {
			Destroy (hexTile.gameObject.GetComponent<PandaBehaviour> ());
		}

		hexTile.gameObject.AddComponent<CityTaskManager>();
		hexTile.gameObject.AddComponent<PandaBehaviour>();
		hexTile.gameObject.GetComponent<PandaBehaviour>().tickOn = BehaviourTree.UpdateOrder.Manual;
		hexTile.gameObject.GetComponent<PandaBehaviour>().Compile (cityBehaviourTree.text);
		BehaviourTreeManager.Instance.allTrees.Add(hexTile.gameObject.GetComponent<PandaBehaviour> ());
        hexTile.city.UpdateBorderTiles();
        return hexTile.city;
	}

	public City GetCityByID(int id){
		for (int i = 0; i < this.woodHabitableTiles.Count; i++) {
			HexTile currHabitableTile = this.woodHabitableTiles[i];
			if (currHabitableTile.isOccupied && currHabitableTile.city.id == id) {
				return currHabitableTile.city;
			}
		}

		for (int i = 0; i < this.stoneHabitableTiles.Count; i++) {
			HexTile currHabitableTile = this.stoneHabitableTiles[i];
			if (currHabitableTile.isOccupied && currHabitableTile.city.id == id) {
				return currHabitableTile.city;
			}
		}
		return null;
	}

    public GameObject[] GetStructurePrefabsForRace(RACE race, STRUCTURE_TYPE structureType) {
        RaceStructures raceStructuresToUse = humanStructures;
        if(race == RACE.ELVES) {
            raceStructuresToUse = elvenStructures;
        } 
//		else {
//            raceStructuresToUse = humanStructures;
//        }

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
		case LAIR.STORM_WITCH:
			return stormWitchLair;
        }
        return null;
    }

    public List<HexTile> GetHabitableTilesForRace(RACE race, bool unoccupiedOnly = true) {
        List<HexTile> habitableTilesForRace = new List<HexTile>();
        if(race == RACE.HUMANS) {
            habitableTilesForRace.AddRange(stoneHabitableTiles);
        } else if (race == RACE.ELVES){
            habitableTilesForRace.AddRange(woodHabitableTiles);
        }

        if (unoccupiedOnly) {
            habitableTilesForRace = habitableTilesForRace.Where(x => !x.isOccupied).ToList();
        }

        return habitableTilesForRace;
    }
}
