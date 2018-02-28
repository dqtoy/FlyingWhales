using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public List<CharacterProductionWeight> characterProductionWeights;
	public DungeonEncounterChances[] dungeonEncounterChances;
    public int initialResourceLandmarks;
    public int initialDungeonLandmarks;
    public int initialSettlementLandmarks;

    public List<LandmarkData> landmarkData;

    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BASE_LANDMARK_TYPE baseLandmarkType = Utilities.GetBaseLandmarkType(landmarkType);
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
        if(baseLandmarkType == BASE_LANDMARK_TYPE.SETTLEMENT && landmarkType != LANDMARK_TYPE.CITY) {
            if(landmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
                //Create a new faction to occupy the new settlement
                Faction newFaction = FactionManager.Instance.CreateNewFaction(typeof(Camp), RACE.GOBLIN);
                newLandmark.OccupyLandmark(newFaction);
            }
        }
        return newLandmark;
    }
    public BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
        LandmarkData data = GetLandmarkData(landmarkType);
        return data.baseType;
    }
    /*
     Occupy a specified landmark.
         */
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    /*
     Occupy the main settlement in a region
         */
    public void OccupyLandmark(Region region, Faction occupant) {
        region.centerOfMass.landmarkOnTile.OccupyLandmark(occupant);
    }
	public void OccupyLandmark(HexTile hexTile, Faction occupant) {
		hexTile.landmarkOnTile.OccupyLandmark(occupant);
	}

    #region Landmark Generation
    /*
     Generate new landmarks (Lairs, Dungeons)
         */
    public void GenerateOtherLandmarks() {
		AddAllCenterOfMassToRegionLandmarksList ();
        GenerateDungeonLandmarks();
        GenerateSettlementLandmarks();
        //List<HexTile> elligibleTiles = new List<HexTile>(hexTiles.Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad)); //Get tiles that aren't water and are not habitable
        ////Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
        //for (int i = 0; i < allRegions.Count; i++) {
        //    Region currRegion = allRegions[i];
        //    List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
        //    Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        //}
        //Dictionary<LANDMARK_TYPE, int> createdLandmarksDict = new Dictionary<LANDMARK_TYPE, int>();
        //int numOfLandmarksToCreate = Mathf.FloorToInt(6 * (float)allRegions.Count); //Increase Landmarks to 6 times the number of regions
        //Debug.Log("Creating " + numOfLandmarksToCreate.ToString() + " landmarks..... ");
        //int createdLandmarks = 0;

        //while (createdLandmarks != numOfLandmarksToCreate) {
        //    if (elligibleTiles.Count <= 0) {
        //        Debug.Log("Only created " + createdLandmarks.ToString() + " landmarks");
        //        return;
        //    }
        //    HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
        //    elligibleTiles.Remove(chosenTile);
        //    List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
        //    Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        //    chosenTile.CreateRandomLandmark();

        //    //Keep track of number of landmarks per type
        //    if (chosenTile.landmarkOnTile != null) {
        //        LANDMARK_TYPE createdLandmarkType = chosenTile.landmarkOnTile.specificLandmarkType;
        //        if (createdLandmarksDict.ContainsKey(createdLandmarkType)) {
        //            createdLandmarksDict[createdLandmarkType]++;
        //        } else {
        //            createdLandmarksDict.Add(createdLandmarkType, 1);
        //        }
        //        createdLandmarks++;
        //    }
        //    //if(createdRoad != null) {
        //    //    Utilities.ListRemoveRange(elligibleTiles, createdRoad);
        //    //}

        //}
        //Debug.Log("Created " + createdLandmarks.ToString() + " landmarks");

        //foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in createdLandmarksDict) {
        //    Debug.Log(kvp.Key.ToString() + " - " + kvp.Value.ToString());
        //}
    }
	private void AddAllCenterOfMassToRegionLandmarksList(){
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region region = GridMap.Instance.allRegions [i];
			if(region.centerOfMass.landmarkOnTile != null){
				if(region.allLandmarks.Count > 0){
					region.allLandmarks.Insert (0, region.centerOfMass.landmarkOnTile);
				}else{
					region.AddLandmarkToRegion (region.centerOfMass.landmarkOnTile);
				}
			}
		}
	}
    private void GenerateDungeonLandmarks() {
        List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles
            .Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad && x.landmarkOnTile == null));

        //Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        }

        Debug.Log("Creating " + initialDungeonLandmarks.ToString() + " dungeon landmarks..... ");
        int createdLandmarks = 0;
        WeightedDictionary<LANDMARK_TYPE> dungeonWeights = GetDungeonLandmarkAppearanceWeights();
        while (createdLandmarks != initialDungeonLandmarks) {
            if (elligibleTiles.Count <= 0) {
                Debug.LogWarning("Only created " + createdLandmarks.ToString() + " dungeon landmarks");
                return;
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            List<HexTile> createdRoad = CreateRoadsForLandmarks(chosenTile);
            if (createdRoad != null) {
                Utilities.ListRemoveRange(elligibleTiles, createdRoad);
                elligibleTiles.Remove(chosenTile);
                List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
                Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
                LANDMARK_TYPE chosenLandmarkType = dungeonWeights.PickRandomElementGivenWeights();
                LandmarkData data = GetLandmarkData(chosenLandmarkType);
                if (data.isUnique) {
                    dungeonWeights.RemoveElement(chosenLandmarkType); //Since the chosen landmark type is unique, remove it from the choices.
                }
                BaseLandmark newLandmark = CreateNewLandmarkOnTile(chosenTile, chosenLandmarkType);
                RoadManager.Instance.CreateRoad(createdRoad, ROAD_TYPE.MINOR);
                createdLandmarks++;
            }
            //chosenTile.CreateLandmarkOfType(BASE_LANDMARK_TYPE.DUNGEON, LANDMARK_TYPE.);
        }
        Debug.Log("Created " + createdLandmarks.ToString() + " dungeon landmarks");
    }
    private void GenerateSettlementLandmarks() {
        List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles
            .Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad && x.landmarkOnTile == null));

        //Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
            //Tiles that are within 2 tiles of a landmark tile, cannot be landmarks
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                tilesToRemove = currLandmark.location.GetTilesInRange(2);
                Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
            }
        }

        Debug.Log("Creating " + initialSettlementLandmarks.ToString() + " settlement landmarks..... ");
        int createdLandmarks = 0;
        WeightedDictionary<LANDMARK_TYPE> settlementWeights = GetSettlementLandmarkAppearanceWeights();
        while (createdLandmarks != initialSettlementLandmarks) {
            if (elligibleTiles.Count <= 0) {
                Debug.LogWarning("Only created " + createdLandmarks.ToString() + " settlement landmarks");
                return;
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            List<HexTile> createdRoad = CreateRoadsForLandmarks(chosenTile);
            if (createdRoad != null) {
                Utilities.ListRemoveRange(elligibleTiles, createdRoad);
                elligibleTiles.Remove(chosenTile);
                List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
                Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
                LANDMARK_TYPE chosenLandmarkType = settlementWeights.PickRandomElementGivenWeights();
                //LandmarkData data = GetLandmarkData(chosenLandmarkType);
                //if (data.isUnique) {
                //    settlementWeights.RemoveElement(chosenLandmarkType); //Since the chosen landmark type is unique, remove it from the choices.
                //}
                BaseLandmark newLandmark = CreateNewLandmarkOnTile(chosenTile, chosenLandmarkType);
                RoadManager.Instance.CreateRoad(createdRoad, ROAD_TYPE.MINOR);
                createdLandmarks++;
            }
            //chosenTile.CreateLandmarkOfType(BASE_LANDMARK_TYPE.DUNGEON, LANDMARK_TYPE.);
        }
        Debug.Log("Created " + createdLandmarks.ToString() + " settlement landmarks");
    }
    private WeightedDictionary<LANDMARK_TYPE> GetDungeonLandmarkAppearanceWeights() {
        WeightedDictionary<LANDMARK_TYPE> dungeonAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if(currData.baseType == BASE_LANDMARK_TYPE.DUNGEON) {
                dungeonAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
            }
        }
        return dungeonAppearanceWeights;
    }
    private WeightedDictionary<LANDMARK_TYPE> GetSettlementLandmarkAppearanceWeights() {
        WeightedDictionary<LANDMARK_TYPE> settlementAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.baseType == BASE_LANDMARK_TYPE.SETTLEMENT) {
                settlementAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
            }
        }
        return settlementAppearanceWeights;
    }
    private LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkType == landmarkType) {
                return currData;
            }
        }
        throw new System.Exception(landmarkType.ToString() + " has no data!");
    }
    public List<HexTile> CreateRoadsForLandmarks(HexTile location) {
        List<HexTile> elligibleTilesToConnectTo = new List<HexTile>();
        elligibleTilesToConnectTo.AddRange(location.region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MAJOR));
        elligibleTilesToConnectTo.AddRange(location.region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR));
        elligibleTilesToConnectTo.Add(location.region.centerOfMass);
        for (int i = 0; i < location.region.landmarks.Count; i++) {
            BaseLandmark currLandmark = location.region.landmarks[i];
            if (currLandmark.location.id != location.id) {
                elligibleTilesToConnectTo.Add(currLandmark.location);
            }
        }
        List<HexTile> nearestPath = null;
        for (int i = 0; i < elligibleTilesToConnectTo.Count; i++) {
            HexTile currTile = elligibleTilesToConnectTo[i];
            List<HexTile> path = PathGenerator.Instance.GetPath(location, currTile, PATHFINDING_MODE.LANDMARK_CONNECTION);
            if(path != null && (nearestPath == null || path.Count < nearestPath.Count)) {
                nearestPath = path;
            }
        }
        return nearestPath;
    }
    #endregion

    #region ECS.Character Production
    /*
     Get the character role weights for a faction.
     This will not include roles that the faction has already reached the cap of.
         */
    public WeightedDictionary<CHARACTER_ROLE> GetCharacterRoleProductionDictionary(Faction faction, Settlement settlement) {
        WeightedDictionary<CHARACTER_ROLE> characterWeights = new WeightedDictionary<CHARACTER_ROLE>();
        for (int i = 0; i < characterProductionWeights.Count; i++) {
            CharacterProductionWeight currWeight = characterProductionWeights[i];
			if(settlement.CanProduceRole(currWeight.role)){
				bool shouldIncludeWeight = true;
				for (int j = 0; j < currWeight.productionCaps.Count; j++) {
					CharacterProductionCap currCap = currWeight.productionCaps[j];
					if(currCap.IsCapReached(currWeight.role, faction, settlement)) {
						shouldIncludeWeight = false; //The current faction has already reached the cap for the current role, do not add to weights.
						break;
					}
				}
				if (shouldIncludeWeight) {
					characterWeights.AddElement(currWeight.role, currWeight.weight);
				}
			}
        }
        return characterWeights;
    }
	public WeightedDictionary<CHARACTER_ROLE> GetCharacterRoleProductionDictionaryNoRestrictions(Faction faction, Settlement settlement) {
		WeightedDictionary<CHARACTER_ROLE> characterWeights = new WeightedDictionary<CHARACTER_ROLE>();
		for (int i = 0; i < characterProductionWeights.Count; i++) {
			CharacterProductionWeight currWeight = characterProductionWeights[i];
			bool shouldIncludeWeight = true;
			for (int j = 0; j < currWeight.productionCaps.Count; j++) {
				CharacterProductionCap currCap = currWeight.productionCaps[j];
				if(currCap.IsCapReached(currWeight.role, faction, settlement)) {
					shouldIncludeWeight = false; //The current faction has already reached the cap for the current role, do not add to weights.
					break;
				}
			}
			if (shouldIncludeWeight) {
				characterWeights.AddElement(currWeight.role, currWeight.weight);
			}
		}
		return characterWeights;
	}
    /*
     Get the character class weights for a settlement.
     This will eliminate any character classes that the settlement cannot
     produce due to a lack of technologies.
         */
	public WeightedDictionary<CHARACTER_CLASS> GetCharacterClassProductionDictionary(BaseLandmark landmark, ref MATERIAL material) {
        WeightedDictionary<CHARACTER_CLASS> classes = new WeightedDictionary<CHARACTER_CLASS>();
        CHARACTER_CLASS[] allClasses = Utilities.GetEnumValues<CHARACTER_CLASS>();
        Settlement settlement = null;
        if(landmark is Settlement) {
            settlement = landmark as Settlement;
        } else {
            settlement = landmark.location.region.mainLandmark as Settlement;
        }
        for (int i = 1; i < allClasses.Length; i++) {
            CHARACTER_CLASS charClass = allClasses[i];
			if (settlement.CanProduceClass(charClass, ref material)) { //Does the settlement have the required technologies to produce this class
                classes.AddElement(charClass, 200);
            }
        }
        return classes;
    }
    /*
     Get the character class weights for a settlement.
     This will eliminate any character classes that the settlement cannot
     produce due to a lack of technologies.
         */
    public WeightedDictionary<CHARACTER_CLASS> GetCharacterClassProductionDictionary(BaseLandmark landmark) {
        WeightedDictionary<CHARACTER_CLASS> classes = new WeightedDictionary<CHARACTER_CLASS>();
        CHARACTER_CLASS[] allClasses = Utilities.GetEnumValues<CHARACTER_CLASS>();
        Settlement settlement = null;
        if (landmark is Settlement) {
            settlement = landmark as Settlement;
        } else {
            settlement = landmark.location.region.mainLandmark as Settlement;
        }
        for (int i = 1; i < allClasses.Length; i++) {
            CHARACTER_CLASS charClass = allClasses[i];
            if (settlement.CanProduceClass(charClass)) { //Does the settlement have the required technologies to produce this class
                classes.AddElement(charClass, 200);
            }
        }
        return classes;
    }
    #endregion

    #region Material Generation
    /*
     Generate Materials
         */
    public void GenerateMaterials() {
        List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles.Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad)); //Get tiles that aren't water and are not habitable
        //Tiles that are within 2 tiles of a habitable tile, cannot have resources
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        }

        WeightedDictionary<MATERIAL> materialWeights = Utilities.GetMaterialWeights();

        Dictionary<MATERIAL, int> createdMaterials = new Dictionary<MATERIAL, int>();
        Debug.Log("Creating " + initialResourceLandmarks.ToString() + " materials..... ");
        int placedMaterials = 0;

        while (placedMaterials != initialResourceLandmarks) {
            if (elligibleTiles.Count <= 0) {
                Debug.Log("Only created " + placedMaterials.ToString() + " landmarks");
                return;
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            elligibleTiles.Remove(chosenTile);
            List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);

            MATERIAL chosenMaterial = materialWeights.PickRandomElementGivenWeights();
            chosenTile.SetMaterialOnTile(chosenMaterial);

            //Keep track of number of materials per type
            if (createdMaterials.ContainsKey(chosenMaterial)) {
                createdMaterials[chosenMaterial]++;
            } else {
                createdMaterials.Add(chosenMaterial, 1);
            }
            placedMaterials++;
        }
        Debug.Log("Created " + placedMaterials.ToString() + " materials");

        foreach (KeyValuePair<MATERIAL, int> kvp in createdMaterials) {
            Debug.Log(kvp.Key.ToString() + " - " + kvp.Value.ToString());
        }
    }
    #endregion

    public DungeonEncounterChances GetDungeonEncounterChances(LANDMARK_TYPE dungeonType){
		for (int i = 0; i < dungeonEncounterChances.Length; i++) {
			if(dungeonType == dungeonEncounterChances[i].dungeonType){
				return dungeonEncounterChances [i];
			}
		}
		return new DungeonEncounterChances ();
	}

    public BaseLandmark GetLandmarkByID(int id) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if(currRegion.mainLandmark.id == id) {
                return currRegion.mainLandmark;
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if(currLandmark.id == id) {
                    return currLandmark;
                }
            }
        }
        return null;
    }

    public BaseLandmark GetLandmarkByName(string name) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.mainLandmark.landmarkName == name) {
                return currRegion.mainLandmark;
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.landmarkName == name) {
                    return currLandmark;
                }
            }
        }
        return null;
    }
}
