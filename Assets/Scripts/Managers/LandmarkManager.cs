using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public List<CharacterProductionWeight> characterProductionWeights;
	//public DungeonEncounterChances[] dungeonEncounterChances;
    public int initialResourceLandmarks;
    public int initialDungeonLandmarks;
    public int initialSettlementLandmarks;

    public int initialLandmarkCount;

    public List<BaseLandmarkData> baseLandmarkData;
    public List<LandmarkData> landmarkData;

	//Crater
	public BaseLandmark craterLandmark;

    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BASE_LANDMARK_TYPE baseLandmarkType = LandmarkManager.Instance.GetLandmarkData(landmarkType).baseLandmarkType;
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
//		AddInitialLandmarkItems (newLandmark);
        return newLandmark;
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
    public bool GenerateLandmarks() {
        List<BaseLandmark> createdLandmarks = new List<BaseLandmark>();
        List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles);

        //create landmarks on all regions' center of mass first
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.centerOfMass.landmarkOnTile == null) {
                WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(currRegion);
                LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
                BaseLandmark createdLandmark = CreateNewLandmarkOnTile(currRegion.centerOfMass, chosenType);
                elligibleTiles.Remove(createdLandmark.tileLocation);
                Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
                createdLandmarks.Add(createdLandmark);
            }
        }

        //all factions must have 1 kings castle in 1 of their owned regions
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Faction currTribe = FactionManager.Instance.allTribes[i];
            if (currTribe.HasAccessToLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE)) {
                //occupy that kings castle instead
                BaseLandmark kingsCastle = currTribe.GetAccessibleLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE);
                kingsCastle.OccupyLandmark(currTribe);
            } else {
                //the currTribe doesn't own a region that has a kings castle create a new one and occupy it
                List<HexTile> tilesToChooseFrom = new List<HexTile>();
                currTribe.ownedRegions.ForEach(x => tilesToChooseFrom.AddRange(x.tilesInRegion.Where(y => elligibleTiles.Contains(y))));
                HexTile chosenTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
                BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.KINGS_CASTLE);
                createdLandmark.OccupyLandmark(currTribe);
                elligibleTiles.Remove(createdLandmark.tileLocation);
                Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
                createdLandmarks.Add(createdLandmark);
            }
        }
        
        while (createdLandmarks.Count < initialLandmarkCount) {
            if (elligibleTiles.Count <= 0) {
                return false; //ran out of tiles
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(chosenTile.region);
            LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
            BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, chosenType);
            elligibleTiles.Remove(createdLandmark.tileLocation);
            Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
            createdLandmarks.Add(createdLandmark);
        }

        Debug.Log("Created " + createdLandmarks.Count + " landmarks.");

        return true;
    }

  //  /*
  //   Generate new landmarks (Lairs, Dungeons)
  //       */
  //  public void GenerateOtherLandmarks() {
		//AddAllCenterOfMassToRegionLandmarksList ();
  //      GenerateDungeonLandmarks();
  //      GenerateSettlementLandmarks();
  //  }
    public void InitializeLandmarks() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                currLandmark.Initialize();
            }
        }
    }
//	private void AddAllCenterOfMassToRegionLandmarksList(){
//		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
//			Region region = GridMap.Instance.allRegions [i];
//			if(region.centerOfMass.landmarkOnTile != null){
////				region.AddLandmarkToRegion (region.centerOfMass.landmarkOnTile);
//				if(region.landmarks.Count > 0){
//					region.landmarks.Insert (0, region.centerOfMass.landmarkOnTile);
//				}else{
//					region.landmarks.Add (region.centerOfMass.landmarkOnTile);
//				}
//			}
//		}
//	}
    //private void GenerateDungeonLandmarks() {
    //    List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles
    //        .Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad && x.landmarkOnTile == null));

    //    //Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
    //        Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
    //    }

    //    Debug.Log("Creating " + initialDungeonLandmarks.ToString() + " dungeon landmarks..... ");
    //    int createdLandmarks = 0;
    //    WeightedDictionary<LANDMARK_TYPE> dungeonWeights = GetDungeonLandmarkAppearanceWeights();
    //    while (createdLandmarks != initialDungeonLandmarks) {
    //        if (elligibleTiles.Count <= 0) {
    //            Debug.LogWarning("Only created " + createdLandmarks.ToString() + " dungeon landmarks");
    //            return;
    //        }
    //        HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
    //        List<HexTile> createdRoad = CreateRoadsForLandmarks(chosenTile);
    //        if (createdRoad != null) {
    //            Utilities.ListRemoveRange(elligibleTiles, createdRoad);
    //            elligibleTiles.Remove(chosenTile);
    //            List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
    //            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
    //            LANDMARK_TYPE chosenLandmarkType = dungeonWeights.PickRandomElementGivenWeights();
    //            LandmarkData data = GetLandmarkData(chosenLandmarkType);
    //            if (data.isUnique) {
    //                dungeonWeights.RemoveElement(chosenLandmarkType); //Since the chosen landmark type is unique, remove it from the choices.
    //            }
    //            BaseLandmark newLandmark = CreateNewLandmarkOnTile(chosenTile, chosenLandmarkType);
    //            RoadManager.Instance.CreateRoad(createdRoad, ROAD_TYPE.MINOR);
    //            createdLandmarks++;
    //        }
    //        //chosenTile.CreateLandmarkOfType(BASE_LANDMARK_TYPE.DUNGEON, LANDMARK_TYPE.);
    //    }
    //    Debug.Log("Created " + createdLandmarks.ToString() + " dungeon landmarks");
    //}
    //private void GenerateSettlementLandmarks() {
    //    List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles
    //        .Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad && x.landmarkOnTile == null));

    //    //Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
    //        Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
    //        //Tiles that are within 2 tiles of a landmark tile, cannot be landmarks
    //        for (int j = 0; j < currRegion.landmarks.Count; j++) {
    //            BaseLandmark currLandmark = currRegion.landmarks[j];
				//tilesToRemove = currLandmark.tileLocation.GetTilesInRange(2);
    //            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
    //        }
    //    }

    //    Debug.Log("Creating " + initialSettlementLandmarks.ToString() + " settlement landmarks..... ");
    //    int createdLandmarks = 0;
    //    WeightedDictionary<LANDMARK_TYPE> settlementWeights = GetSettlementLandmarkAppearanceWeights();
    //    if (settlementWeights.GetTotalOfWeights() <= 0) {
    //        return; //there are no settlement weights
    //    }
    //    while (createdLandmarks != initialSettlementLandmarks) {
    //        if (elligibleTiles.Count <= 0) {
    //            Debug.LogWarning("Only created " + createdLandmarks.ToString() + " settlement landmarks");
    //            return;
    //        }
    //        HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
    //        List<HexTile> createdRoad = CreateRoadsForLandmarks(chosenTile);
    //        if (createdRoad != null) {
    //            Utilities.ListRemoveRange(elligibleTiles, createdRoad);
    //            elligibleTiles.Remove(chosenTile);
    //            List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
    //            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
    //            LANDMARK_TYPE chosenLandmarkType = settlementWeights.PickRandomElementGivenWeights();
    //            LandmarkData data = GetLandmarkData(chosenLandmarkType);
    //            if (data.isUnique) {
    //                settlementWeights.RemoveElement(chosenLandmarkType); //Since the chosen landmark type is unique, remove it from the choices.
    //            }
    //            BaseLandmark newLandmark = CreateNewLandmarkOnTile(chosenTile, chosenLandmarkType);
    //            RoadManager.Instance.CreateRoad(createdRoad, ROAD_TYPE.MINOR);
    //            createdLandmarks++;
    //        }
    //        //chosenTile.CreateLandmarkOfType(BASE_LANDMARK_TYPE.DUNGEON, LANDMARK_TYPE.);
    //    }
    //    Debug.Log("Created " + createdLandmarks.ToString() + " settlement landmarks");
    //}
    //private WeightedDictionary<LANDMARK_TYPE> GetDungeonLandmarkAppearanceWeights() {
    //    WeightedDictionary<LANDMARK_TYPE> dungeonAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
    //    for (int i = 0; i < landmarkData.Count; i++) {
    //        LandmarkData currData = landmarkData[i];
    //        if(currData.baseLandmarkType == BASE_LANDMARK_TYPE.DUNGEON) {
    //            dungeonAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
    //        }
    //    }
    //    return dungeonAppearanceWeights;
    //}
    //private WeightedDictionary<LANDMARK_TYPE> GetSettlementLandmarkAppearanceWeights() {
    //    WeightedDictionary<LANDMARK_TYPE> settlementAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
    //    for (int i = 0; i < landmarkData.Count; i++) {
    //        LandmarkData currData = landmarkData[i];
    //        if (currData.baseLandmarkType == BASE_LANDMARK_TYPE.SETTLEMENT && currData.landmarkType != LANDMARK_TYPE.TOWN) {
    //            settlementAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
    //        }
    //    }
    //    return settlementAppearanceWeights;
    //}
    private WeightedDictionary<LANDMARK_TYPE> GetLandmarkAppearanceWeights(Region region) {
        WeightedDictionary<LANDMARK_TYPE> landmarkAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.onOccupiedOnly && !region.isOwned) {
                continue; //skip
            }
            if (currData.isUnique && HasLandmarkOfType(currData.landmarkType)) {
                continue; //skip
            }
            landmarkAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
        }
        return landmarkAppearanceWeights;
    }
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkType == landmarkType) {
                return currData;
            }
        }
        throw new System.Exception("There is no landmark data for " + landmarkType.ToString());
    }
   // public List<HexTile> CreateRoadsForLandmarks(HexTile location) {
   //     List<HexTile> elligibleTilesToConnectTo = new List<HexTile>();
   //     elligibleTilesToConnectTo.AddRange(location.region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MAJOR));
   //     elligibleTilesToConnectTo.AddRange(location.region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR));
   //     elligibleTilesToConnectTo.Add(location.region.centerOfMass);
   //     for (int i = 0; i < location.region.landmarks.Count; i++) {
   //         BaseLandmark currLandmark = location.region.landmarks[i];
			//if (currLandmark.tileLocation.id != location.id) {
			//	elligibleTilesToConnectTo.Add(currLandmark.tileLocation);
   //         }
   //     }
   //     List<HexTile> nearestPath = null;
   //     for (int i = 0; i < elligibleTilesToConnectTo.Count; i++) {
   //         HexTile currTile = elligibleTilesToConnectTo[i];
   //         List<HexTile> path = PathGenerator.Instance.GetPath(location, currTile, PATHFINDING_MODE.LANDMARK_ROADS);
   //         if(path != null && (nearestPath == null || path.Count < nearestPath.Count)) {
   //             nearestPath = path;
   //         }
   //     }
   //     return nearestPath;
   // }

	private void AddInitialLandmarkItems(BaseLandmark landmark){
		List<ECS.Item> items = new List<ECS.Item> ();
		switch(landmark.specificLandmarkType){
		case LANDMARK_TYPE.CAVE:
			ECS.Item neuroctus = ItemManager.Instance.CreateNewItemInstance ("Neuroctus");
			items.Add (neuroctus);
			break;
		}
		landmark.AddItemsInLandmark (items);
	}
    #endregion

    #region ECS.Character Production
    /*
     Get the character role weights for a faction.
     This will not include roles that the faction has already reached the cap of.
         */
    public WeightedDictionary<CHARACTER_ROLE> GetCharacterRoleProductionDictionary() {
        WeightedDictionary<CHARACTER_ROLE> characterWeights = new WeightedDictionary<CHARACTER_ROLE>();
        for (int i = 0; i < characterProductionWeights.Count; i++) {
            CharacterProductionWeight currWeight = characterProductionWeights[i];
			characterWeights.AddElement(currWeight.role, currWeight.weight);
        }
        return characterWeights;
    }
    public WeightedDictionary<CHARACTER_CLASS> GetCharacterClassProductionDictionary(BaseLandmark landmark) {
        WeightedDictionary<CHARACTER_CLASS> classes = new WeightedDictionary<CHARACTER_CLASS>();
        CHARACTER_CLASS[] allClasses = Utilities.GetEnumValues<CHARACTER_CLASS>();
   //     Settlement settlement = null;
   //     if (landmark is Settlement) {
   //         settlement = landmark as Settlement;
   //     } else {
			//settlement = landmark.tileLocation.region.mainLandmark as Settlement;
   //     }
        for (int i = 1; i < allClasses.Length; i++) {
            CHARACTER_CLASS charClass = allClasses[i];
            if (landmark.CanProduceClass(charClass)) { //Does the settlement have the required technologies to produce this class
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

    #region Utilities
    public BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
        LandmarkData landmarkData = GetLandmarkData(landmarkType);
        return landmarkData.baseLandmarkType;
    }
    public BaseLandmark GetLandmarkByID(int id) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.mainLandmark.id == id) {
                return currRegion.mainLandmark;
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.id == id) {
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
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.specificLandmarkType == landmarkType) {
                    return true;
                }
            }
        }
        return false;
    }
    public List<BaseLandmark> GetLandmarksOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> allLandmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.specificLandmarkType == landmarkType) {
                    allLandmarksOfType.Add(currLandmark);
                }
            }
        }
        return allLandmarksOfType;
    }
    public List<BaseLandmark> GetLandmarksOfType(BASE_LANDMARK_TYPE baseLandmarkType) {
        List<BaseLandmark> allLandmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (GetBaseLandmarkType(currLandmark.specificLandmarkType) == baseLandmarkType) {
                    allLandmarksOfType.Add(currLandmark);
                }
            }
        }
        return allLandmarksOfType;
    }
    public BaseLandmarkData GetBaseLandmarkData(BASE_LANDMARK_TYPE baseLandmarkType) {
        for (int i = 0; i < baseLandmarkData.Count; i++) {
            BaseLandmarkData currData = baseLandmarkData[i];
            if (currData.baseLandmarkType == baseLandmarkType) {
                return currData;
            }
        }
        throw new System.Exception("There is no base landmark data for " + baseLandmarkType);
    }
    public List<BaseLandmark> GetAllLandmarks() {
        List<BaseLandmark> allLandmarks = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            allLandmarks.AddRange(currRegion.landmarks);
        }
        return allLandmarks;
    }
    #endregion

}
