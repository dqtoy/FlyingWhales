﻿using UnityEngine;
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

    //public List<BaseLandmarkData> baseLandmarkData;
    public List<LandmarkData> landmarkData;
    public List<AreaData> areaData;

    public int corruptedLandmarksCount;

    public List<Area> allAreas;

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        corruptedLandmarksCount = 0;
        allAreas = new List<Area>();
    }
    #endregion

    #region Landmarks
    public void LoadLandmarks(WorldSaveData data) {
        if (data.landmarksData != null) {
            for (int i = 0; i < data.landmarksData.Count; i++) {
                LandmarkSaveData landmarkData = data.landmarksData[i];
                CreateNewLandmarkOnTile(landmarkData);
            }
        }
    }
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        if (location.landmarkOnTile != null) {
            //Destroy landmark on tile
            DestroyLandmarkOnTile(location);
        }
        //LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        //BASE_LANDMARK_TYPE baseLandmarkType = landmarkData.baseLandmarkType;
        BaseLandmark newLandmark = location.CreateLandmarkOfType(landmarkType);
#if !WORLD_CREATION_TOOL
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        //newLandmark.GenerateDiagonalLeftTiles();
        //newLandmark.GenerateDiagonalRightTiles();
        //newLandmark.GenerateHorizontalTiles();
        newLandmark.GenerateWallTiles();
        newLandmark.PutWallUp();
        //for (int i = 0; i < location.AllNeighbours.Count; i++) {
        //    location.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        //ConstructLandmarkObjects(landmarkData, newLandmark);
        //		AddInitialLandmarkItems (newLandmark);
#endif
        return newLandmark;
    }
    public BaseLandmark CreateNewLandmarkOnTile(LandmarkSaveData saveData) {
#if !WORLD_CREATION_TOOL
        HexTile location = GridMap.Instance.map[saveData.locationCoordinates.X, saveData.locationCoordinates.Y];
#else
        HexTile location = worldcreator.WorldCreatorManager.Instance.map[saveData.locationCoordinates.X, saveData.locationCoordinates.Y];
#endif
        if (location.landmarkOnTile != null) {
            //Destroy landmark on tile
            DestroyLandmarkOnTile(location);
        }
        //LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(saveData.landmarkType);
        //BASE_LANDMARK_TYPE baseLandmarkType = landmarkData.baseLandmarkType;
        BaseLandmark newLandmark = location.CreateLandmarkOfType(saveData);
#if !WORLD_CREATION_TOOL
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        //newLandmark.GenerateDiagonalLeftTiles();
        //newLandmark.GenerateDiagonalRightTiles();
        //newLandmark.GenerateHorizontalTiles();
        newLandmark.GenerateWallTiles();
        newLandmark.PutWallUp();
        //for (int i = 0; i < location.AllNeighbours.Count; i++) {
        //    location.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        //ConstructLandmarkObjects(landmarkData, newLandmark);
        //		AddInitialLandmarkItems (newLandmark);
#endif
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        while (landmarkOnTile.charactersAtLocation.Count != 0) {
            landmarkOnTile.RemoveCharacterFromLocation(landmarkOnTile.charactersAtLocation[0]);
        }
        //while (landmarkOnTile.charactersWithHomeOnLandmark.Count != 0) {
        //    landmarkOnTile.charactersWithHomeOnLandmark[0].SetHome(null);
        //    landmarkOnTile.RemoveCharacterHomeOnLandmark(landmarkOnTile.charactersWithHomeOnLandmark[0]);
        //}
        tile.RemoveLandmarkOnTile();
        tile.region.RemoveLandmarkFromRegion(landmarkOnTile);
        GameObject.Destroy(landmarkOnTile.landmarkVisual.gameObject);
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        //newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        //newLandmark.GenerateDiagonalLeftTiles();
        //newLandmark.GenerateDiagonalRightTiles();
        //newLandmark.GenerateHorizontalTiles();
        //newLandmark.GenerateWallTiles();
        //newLandmark.PutWallUp();
        //for (int i = 0; i < location.AllNeighbours.Count; i++) {
        //    location.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        //ConstructLandmarkObjects(landmarkData, newLandmark);
        //		AddInitialLandmarkItems (newLandmark);
        return newLandmark;
    }
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    public void OccupyLandmark(Region region, Faction occupant) {
        region.centerOfMass.landmarkOnTile.OccupyLandmark(occupant);
    }
    public void OccupyLandmark(HexTile hexTile, Faction occupant) {
        hexTile.landmarkOnTile.OccupyLandmark(occupant);
    }
    #endregion

    #region Landmark Generation
    //public bool GenerateLandmarks() {
    //    List<BaseLandmark> createdLandmarks = new List<BaseLandmark>();
    //    List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles);

    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        Utilities.ListRemoveRange(elligibleTiles, currRegion.outerTiles);
    //    }

    //    //create landmarks on all regions' center of mass first
    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        if (currRegion.centerOfMass.landmarkOnTile == null) {
    //            WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(currRegion);
    //            LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
    //            BaseLandmark createdLandmark = CreateNewLandmarkOnTile(currRegion.centerOfMass, chosenType);
    //            elligibleTiles.Remove(createdLandmark.tileLocation);
    //            Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //            createdLandmarks.Add(createdLandmark);
    //        }
    //    }

    //    //all factions must have 1 kings castle in 1 of their owned regions
    //    for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
    //        Faction currTribe = FactionManager.Instance.allTribes[i];
    //        if (currTribe.HasAccessToLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE)) {
    //            //occupy that kings castle instead
    //            BaseLandmark kingsCastle = currTribe.GetAccessibleLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE);
    //            kingsCastle.OccupyLandmark(currTribe);
    //        } else {
    //            //the currTribe doesn't own a region that has a kings castle create a new one and occupy it
    //            List<HexTile> tilesToChooseFrom = new List<HexTile>();
    //            currTribe.ownedRegions.ForEach(x => tilesToChooseFrom.AddRange(x.tilesInRegion.Where(y => elligibleTiles.Contains(y))));
    //            HexTile chosenTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
    //            BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.KINGS_CASTLE);
    //            createdLandmark.OccupyLandmark(currTribe);
    //            elligibleTiles.Remove(createdLandmark.tileLocation);
    //            Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //            createdLandmarks.Add(createdLandmark);
    //        }
    //    }

    //    while (createdLandmarks.Count < initialLandmarkCount) {
    //        if (elligibleTiles.Count <= 0) {
    //            return false; //ran out of tiles
    //        }
    //        HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
    //        WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(chosenTile.region);
    //        LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
    //        BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, chosenType);
    //        elligibleTiles.Remove(createdLandmark.tileLocation);
    //        Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //        createdLandmarks.Add(createdLandmark);
    //    }

    //    Debug.Log("Created " + createdLandmarks.Count + " landmarks.");

    //    return true;
    //}
    public void GeneratePlayerLandmarks(Region chosenRegion) {
        //generate landmarks owned by the player in the empty region
        //Create Demonic Portal on the tile nearest to the center of the region
        HexTile tileToUse = null;
        LandmarkData demonicPortalData = GetLandmarkData(LANDMARK_TYPE.DEMONIC_PORTAL);
        if (chosenRegion.centerOfMass.isPassable && demonicPortalData.possibleSpawnPoints.Contains(chosenRegion.centerOfMass.passableType)) {
            tileToUse = chosenRegion.centerOfMass;
        } else {
            float nearestDistance = 9999f;
            List<HexTile> passableTilesInRegion = chosenRegion.tilesInRegion.Where(x => x.isPassable && x.id != chosenRegion.centerOfMass.id && demonicPortalData.possibleSpawnPoints.Contains(x.passableType)).ToList();
            for (int i = 0; i < passableTilesInRegion.Count; i++) {
                HexTile currTile = passableTilesInRegion[i];
                float distance = currTile.GetDistanceTo(chosenRegion.centerOfMass);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    tileToUse = currTile;
                }
            }
        }

        if (tileToUse != null) {
            CreateNewLandmarkOnTile(tileToUse, LANDMARK_TYPE.DEMONIC_PORTAL);
        } else {
            throw new System.Exception("Cannot create Demonic Portal for Player!");
        }
    }
    public void GenerateFactionLandmarks() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction currFaction = FactionManager.Instance.allFactions[i];
            LEVEL wealth = RandomizeLevel(25, 60, 15);
            LEVEL population = RandomizeLevel(25, 60, 15);
            LEVEL needProviders = RandomizeLevel(25, 60, 15);
            LEVEL might = RandomizeLevel(25, 60, 15);
            LEVEL characters = RandomizeLevel(25, 60, 15);
            Dictionary<LANDMARK_TYPE, int> landmarkSettings = GetLandmarkSettings(wealth, population, might, needProviders, currFaction);
            Dictionary<HexTile, LANDMARK_TYPE> landmarksToBeCreated = null;
            while (landmarksToBeCreated == null) {
                landmarksToBeCreated = GenerateLandmarksForFaction(landmarkSettings, currFaction);
            }
            foreach (KeyValuePair<HexTile, LANDMARK_TYPE> kvp in landmarksToBeCreated) {
                CreateNewLandmarkOnTile(kvp.Key, kvp.Value);
            }
        }
        //return true;
    }
    public void LoadAllLandmarksFromSave(Save save) {
        for (int i = 0; i < save.hextiles.Count; i++) {
            HexTile currentTile = GridMap.Instance.GetHexTile(save.hextiles[i].xCoordinate, save.hextiles[i].yCoordinate);
            LoadLandmarkOnTile(currentTile, save.hextiles[i].landmark);
        }
    }
    private Dictionary<LANDMARK_TYPE, int> GetLandmarkSettings(LEVEL wealthLvl, LEVEL populationLvl, LEVEL mightLvl, LEVEL needLvl, Faction faction) {
        Dictionary<LANDMARK_TYPE, int> landmarkSettings = new Dictionary<LANDMARK_TYPE, int>();
        AddWealthSettings(wealthLvl, landmarkSettings);
        AddPopulationSettings(populationLvl, landmarkSettings, faction);
        AddNeedSettings(needLvl, landmarkSettings);
        AddMightSettings(mightLvl, landmarkSettings);
        return landmarkSettings;
    }
    private void AddWealthSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 2);
                landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 2);
                break;
            case LEVEL.AVERAGE:
                if (Random.Range(0, 2) == 0) {
                    landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 1);
                    landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 2);
                } else {
                    landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 2);
                    landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 1);
                }
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 1);
                landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 1);
                break;
            default:
                break;
        }
    }
    private void AddPopulationSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings, Faction faction) {
        LANDMARK_TYPE settlementTypeToUse = LANDMARK_TYPE.HUMAN_SETTLEMENT;
        //if (faction.race == RACE.ELVES) {
        //    settlementTypeToUse = LANDMARK_TYPE.ELVEN_SETTLEMENT;
        //}
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(settlementTypeToUse, 6);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(settlementTypeToUse, 4);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(settlementTypeToUse, 2);
                break;
            default:
                break;
        }
    }
    private void AddMightSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 8);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 5);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 6);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 4);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 4);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 3);
                break;
            default:
                break;
        }
    }
    private void AddNeedSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 2);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 2);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 2);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 2);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 2);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 2);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 1);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 1);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 1);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 1);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 1);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 1);
                break;
            default:
                break;
        }
    }
    private LEVEL RandomizeLevel(int highPercent, int averagePercent, int lowPercent) {
        int chance = Random.Range(0, 100);
        if (chance <= lowPercent) {
            return LEVEL.LOW;
        } else if (chance > lowPercent && chance <= averagePercent) {
            return LEVEL.AVERAGE;
        } else {
            return LEVEL.HIGH;
        }
    }
    private Dictionary<HexTile, LANDMARK_TYPE> GenerateLandmarksForFaction(Dictionary<LANDMARK_TYPE, int> landmarkSettings, Faction faction) {
        Region factionRegion = faction.ownedRegions[0];
        Dictionary<HexTile, LANDMARK_TYPE> landmarksToBeCreated = new Dictionary<HexTile, LANDMARK_TYPE>();
        string log = "Created " + landmarkSettings.Sum(x => x.Value).ToString() + " landmarks on " + factionRegion.name;
        foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in landmarkSettings) {
            LANDMARK_TYPE landmarkType = kvp.Key;
            LandmarkData data = GetLandmarkData(landmarkType);
            log += "\n" + landmarkType.ToString() + " - " + kvp.Value.ToString();
            for (int i = 0; i < kvp.Value; i++) {
                List<HexTile> tilesToChooseFrom = factionRegion.tilesInRegion.Where(x => x.CanBuildLandmarkHere(landmarkType, data, landmarksToBeCreated)).ToList();
                //List<HexTile> tilesToChooseFrom = elligibleTiles.Where(x => ).ToList();
                if (tilesToChooseFrom.Count <= 0) {
                    //Debug.LogError("There are no more tiles in " + tribeRegion.name + " to build a " + landmarkType.ToString(), tribeRegion.centerOfMass);
                    return null;
                }
                HexTile chosenTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
                landmarksToBeCreated.Add(chosenTile, landmarkType);
                //CreateNewLandmarkOnTile(chosenTile, landmarkType);
                //remove the tiles within 1 range of the chosen tile from elligible tiles
                //elligibleTiles.Remove(chosenTile);
                //Utilities.ListRemoveRange(elligibleTiles, chosenTile.GetTilesInRange(1));
            }
        }
        Debug.Log(log, factionRegion.centerOfMass);
        return landmarksToBeCreated;
    }
    public void InitializeLandmarks() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                currLandmark.Initialize();
            }
        }
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
    public ILocation GetLocationBasedOnID(LOCATION_IDENTIFIER identifier, int id) {
        List<ILocation> choices;
        if (identifier == LOCATION_IDENTIFIER.HEXTILE) {
#if WORLD_CREATION_TOOL
            choices = new List<ILocation>(worldcreator.WorldCreatorManager.Instance.hexTiles.Select(x => x as ILocation));
#else
            choices = new List<ILocation>(GridMap.Instance.hexTiles.Select(x => x as ILocation));
#endif
        } else {
            choices = new List<ILocation>(GetAllLandmarks().Select(x => x as ILocation));
        }
        for (int i = 0; i < choices.Count; i++) {
            ILocation currLocation = choices[i];
            if (currLocation.id == id) {
                return currLocation;
            }
        }
        return null;
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
    #endregion

    #region Utilities
    public BaseLandmark GetLandmarkByID(int id) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.id == id) {
                return currLandmark;
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
    public List<BaseLandmark> GetAllLandmarks(List<Region> regions = null) {
        List<BaseLandmark> allLandmarks = new List<BaseLandmark>();
#if WORLD_CREATION_TOOL
        List<Region> choices = worldcreator.WorldCreatorManager.Instance.allRegions;
#else
        List<Region> choices = GridMap.Instance.allRegions;
#endif
        if (regions != null) {
            choices = regions;
        }
        for (int i = 0; i < choices.Count; i++) {
            Region currRegion = choices[i];
            allLandmarks.AddRange(currRegion.landmarks);
        }
        return allLandmarks;
    }
    public List<LandmarkStructureSprite> GetLandmarkTileSprites(LANDMARK_TYPE landmarkType) {
        LandmarkData data = GetLandmarkData(landmarkType);
        return data.landmarkTileSprites;
    }
    #endregion

    #region Areas
    public void LoadAreas(WorldSaveData data) {
        if (data.areaData != null) {
            for (int i = 0; i < data.areaData.Count; i++) {
                AreaSaveData areaData = data.areaData[i];
                Area newArea = CreateNewArea(areaData);
                if (areaData.ownerID != -1) {
                    Faction owner = FactionManager.Instance.GetFactionBasedOnID(areaData.ownerID);
                    if (owner != null) {
                        OwnArea(owner, newArea);
                    }
                }
            }
        }
    }
    public AreaData GetAreaData(AREA_TYPE areaType) {
        for (int i = 0; i < areaData.Count; i++) {
            AreaData currData = areaData[i];
            if (currData.areaType == areaType) {
                return currData;
            }
        }
        throw new System.Exception("No area data for type " + areaType.ToString());
    }
    public Area CreateNewArea(HexTile coreTile, AREA_TYPE areaType, List<HexTile> tiles = null) {
        Area newArea = new Area(coreTile, areaType);
        if (tiles == null) {
            newArea.AddTile(coreTile);
        } else {
            newArea.AddTile(tiles);
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        return newArea;
    }
    public void RemoveArea(Area area) {
        allAreas.Remove(area);
        Messenger.Broadcast(Signals.AREA_DELETED, area);
    }
    public Area CreateNewArea(AreaSaveData data) {
        Area newArea = new Area(data);
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        return newArea;
    }
    public Area GetAreaByID(int id) {
        for (int i = 0; i < allAreas.Count; i++) {
            Area area = allAreas[i];
            if (area.id == id) {
                return area;
            }
        }
        return null;
    }
    public Area GetAreaByName(string name) {
        for (int i = 0; i < allAreas.Count; i++) {
            Area area = allAreas[i];
            if (area.name.Equals(name)) {
                return area;
            }
        }
        return null;
    }
    public void OwnArea(Faction newOwner, Area area) {
        if (area.owner != null) {
            UnownArea(area);
        }
        newOwner.OwnArea(area);
        area.SetOwner(newOwner);
        area.TintStructuresInArea(newOwner.factionColor);
    }
    public void UnownArea(Area area) {
        if (area.owner != null) {
            area.owner.UnownArea(area);
        }
        area.SetOwner(null);
    }
    #endregion
}
