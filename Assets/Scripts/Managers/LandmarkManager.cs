using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public int initialLandmarkCount;

    [SerializeField] private List<LandmarkData> landmarkData;
    public List<AreaData> areaData;

    public int corruptedLandmarksCount;

    public List<Area> allAreas;
    public List<Area> allNonPlayerAreas {
        get { return allAreas.Where(x => x != PlayerManager.Instance.player.playerArea).ToList(); }
    }

    public Sprite ancientRuinTowerSprite;
    public Sprite ancientRuinBlockerSprite;
    public Sprite ruinedSprite;

    [SerializeField] private GameObject landmarkGO;

    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;

    public RaceClassListDictionary defaultRaceDefenders;
    public AreaTypeSpriteDictionary locationPortraits; //NOTE: Move this to world creation when time permits.

    [Header("Inner Structures")]
    [SerializeField] private GameObject innerStructurePrefab;
    [SerializeField] private Transform areaMapsParent;

    public static List<Point> mapNeighborPoints = new List<Point> {
        new Point(0,1), //up
        new Point(1,1), //upper right
        new Point(1,0), //right
        new Point(1,-1), //bottom right
        new Point(0,-1), //bottom
        new Point(-1,-1), //bottom left
        new Point(-1,0), //left
        new Point(-1,1), //upper left
    };

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        corruptedLandmarksCount = 0;
        allAreas = new List<Area>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
    }
    //private void Update() {
    //    if (hasPendingMapGenerationJob && pendingJob.IsCompleted) {
    //        OnFinishedGeneratingAreaMap(pendingAreaMap);
    //    }
    //}
    #endregion

    #region Landmarks
    private void ConstructLandmarkData() {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            data.ConstructData();
        }
    }
    private void LoadLandmarkTypeDictionary() {
        landmarkDataDict = new Dictionary<LANDMARK_TYPE, LandmarkData>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            landmarkDataDict.Add(data.landmarkType, data);
        }
    }
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
        BaseLandmark newLandmark = location.CreateLandmarkOfType(landmarkType);
#if !WORLD_CREATION_TOOL
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        if (newLandmark.tileLocation.areaOfTile != null) {
            newLandmark.tileLocation.areaOfTile.DetermineIfTileIsExposed(newLandmark.tileLocation);
        }
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
        BaseLandmark newLandmark = location.CreateLandmarkOfType(saveData);
#if !WORLD_CREATION_TOOL
        if (newLandmark.tileLocation.areaOfTile != null && newLandmark.tileLocation.areaOfTile.owner != null) {
            OccupyLandmark(newLandmark, newLandmark.tileLocation.areaOfTile.owner);
        }

        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        if (newLandmark.tileLocation.areaOfTile != null) {
            newLandmark.tileLocation.areaOfTile.DetermineIfTileIsExposed(newLandmark.tileLocation);
        }
#endif
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        return newLandmark;
    }
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
    #endregion

    #region Landmark Generation
    public void GenerateSettlements(IntRange settlementRange, Region[] regions, IntRange citizenRange, out BaseLandmark portal) {
        //place portal first
        Region chosenPlayerRegion = regions[Random.Range(0, regions.Length)];
        List<HexTile> playerTileChoices = chosenPlayerRegion.GetValidTilesForLandmarks();
        HexTile chosenTile = playerTileChoices[Random.Range(0, playerTileChoices.Count)];
        Area playerArea = CreateNewArea(chosenTile, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.SetName("Portal"); //need this so that when player is initialized. This area will be assigned to the player.
        portal = CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.DEMONIC_PORTAL);

        //order regions based on distance from the player portal
        List<Region> orderedRegions = new List<Region>(regions);
        orderedRegions = orderedRegions.OrderBy(x => Vector2.Distance(chosenPlayerRegion.coreTile.transform.position, x.coreTile.transform.position)).ToList();

        string orderedLog = string.Empty;
        for (int i = 0; i < orderedRegions.Count; i++) {
            orderedLog += i.ToString() + " " + orderedRegions[i].coreTile.ToString() + "(" + orderedRegions[i].regionColor.ToString() + ")\n";
        }
        Debug.Log(orderedLog);

        //separate regions based on their distance from the player area
        List<Region> nearRegions = new List<Region>(); //regions that are near the player area
        List<Region> farRegions = new List<Region>(); //regions that are far from the player area
        int nearRegionCutoff = regions.Length / 3;
        for (int i = 0; i < orderedRegions.Count; i++) {
            Region currRegion = orderedRegions[i];
            if (i <= nearRegionCutoff) {
                //near region
                nearRegions.Add(currRegion);
            } else {
                farRegions.Add(currRegion);
            }
        }

        int settlementCount = settlementRange.Random();
        Debug.Log("Will generate " + settlementCount.ToString() + " settlements");
        AREA_TYPE[] validSettlementTypes = new AREA_TYPE[] { AREA_TYPE.HUMAN_SETTLEMENT, AREA_TYPE.ELVEN_SETTLEMENT };

        //generate all settlement settings first, then order them by citizen count. This is to ensure that settlements with lower citizen counts will be placed first, 
        //and so can be placed closer to the player portal, without being limited by other settlements with higher citizen counts.
        List<SettlementSettings> settlementSettings = new List<SettlementSettings>();
        for (int i = 0; i < settlementCount; i++) {
            AREA_TYPE chosenSettlementType = validSettlementTypes[Random.Range(0, validSettlementTypes.Length)];
            int cc = citizenRange.Random();
            settlementSettings.Add(new SettlementSettings { citizenCount = cc, settlementType = chosenSettlementType });
        }
        settlementSettings = settlementSettings.OrderBy(x => x.citizenCount).ToList();

        //create the given settlements
        for (int i = 0; i < settlementSettings.Count; i++) {
            SettlementSettings currSetting = settlementSettings[i];
            AREA_TYPE settlementType = currSetting.settlementType;
            int citizenCount = currSetting.citizenCount;
            List<Region> regionChoices;
            //Settlements with fewer inhabitants are spawned near the portal, while stronger, more populated settlements are spawned further away. 
            if (citizenRange.IsNearUpperBound(citizenCount)) {
                //citizen count is more than half of the range. Place settlement at a far away region
                regionChoices = farRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                if (regionChoices.Count == 0) { //if there are no valid far regions, place the landmark at a near region instead
                    regionChoices = nearRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                }
            } else {
                regionChoices = nearRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                if (regionChoices.Count == 0) { //if there are no valid near regions, place the landmark at a far region instead
                    regionChoices = farRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                }
            }

            if (regionChoices.Count == 0) {
                throw new System.Exception("There are no valid regions to place a settlement!");
            }
            Region chosenRegion = regionChoices[Random.Range(0, regionChoices.Count)];
            List<HexTile> tileChoices = chosenRegion.GetValidTilesForLandmarks();
            HexTile chosenRegionTile = tileChoices[Random.Range(0, tileChoices.Count)];
            Area newArea = CreateNewArea(chosenRegionTile, settlementType);
            CreateNewLandmarkOnTile(chosenRegionTile, LANDMARK_TYPE.PALACE);
            Faction faction = FactionManager.Instance.CreateNewFaction();
            if (settlementType == AREA_TYPE.ELVEN_SETTLEMENT) {
                faction.SetInitialFactionLeaderClass("Queen");
                faction.SetInitialFactionLeaderGender(GENDER.FEMALE);
                faction.SetRace(RACE.ELVES);
            } else if (settlementType == AREA_TYPE.HUMAN_SETTLEMENT) {
                faction.SetInitialFactionLeaderClass("King");
                faction.SetInitialFactionLeaderGender(GENDER.MALE);
                faction.SetRace(RACE.HUMANS);
            }
            OwnArea(faction, faction.race, newArea);
            newArea.GenerateStructures(citizenCount);
            //GenerateAreaMap(newArea);
            faction.GenerateStartingCitizens(2, 1, citizenCount); //9,7
        }
        FactionManager.Instance.CreateNeutralFaction();
    }
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        //for (int i = 0; i < landmarkData.Count; i++) {
        //    LandmarkData currData = landmarkData[i];
        //    if (currData.landmarkType == landmarkType) {
        //        return currData;
        //    }
        //}
        if (landmarkDataDict.ContainsKey(landmarkType)) {
            return landmarkDataDict[landmarkType];
        }
        throw new System.Exception("There is no landmark data for " + landmarkType.ToString());
    }
    public void InitializeLandmarks() {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            currLandmark.Initialize();
        }
    }
    public void GenerateMinorLandmarks(List<HexTile> allTiles) {
        Dictionary<BIOMES, LANDMARK_TYPE[]> landmarkChoices = new Dictionary<BIOMES, LANDMARK_TYPE[]>() {
            { BIOMES.GRASSLAND, new LANDMARK_TYPE[] {
                LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.HERMIT_HUT, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.CATACOMB, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
            } },
            { BIOMES.FOREST, new LANDMARK_TYPE[] {
                LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.HERMIT_HUT, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.CATACOMB, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
            } },
            { BIOMES.DESERT, new LANDMARK_TYPE[] {
                LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.CATACOMB, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
            } },
            { BIOMES.SNOW, new LANDMARK_TYPE[] {
                LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.HERMIT_HUT, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.CATACOMB, LANDMARK_TYPE.ICE_PIT, LANDMARK_TYPE.CAVE
            } },
            { BIOMES.TUNDRA, new LANDMARK_TYPE[] {
                LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.HERMIT_HUT, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.CATACOMB, LANDMARK_TYPE.ICE_PIT, LANDMARK_TYPE.CAVE
            } },
        };
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currTile = allTiles[i];
            List<HexTile> tilesInRange = currTile.GetTilesInRange(2);
            if (currTile.landmarkOnTile == null
                && currTile.elevationType == ELEVATION.PLAIN
                && !currTile.IsAtEdgeOfMap()
                && tilesInRange.Where(x => x.landmarkOnTile != null).Count() == 0) {
                LANDMARK_TYPE[] minorLandmarkTypes = landmarkChoices[currTile.biomeType];
                LANDMARK_TYPE chosenLandmarkType = minorLandmarkTypes[Random.Range(0, minorLandmarkTypes.Length)];
                CreateNewLandmarkOnTile(currTile, chosenLandmarkType);
            }
        }
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
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currLandmark;
            }
        }
        //for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    if (currRegion.mainLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //        return currRegion.mainLandmark;
        //    }
        //    for (int j = 0; j < currRegion.landmarks.Count; j++) {
        //        BaseLandmark currLandmark = currRegion.landmarks[j];
        //        if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //            return currLandmark;
        //        }
        //    }
        //}
        return null;
    }
    public BaseLandmark GetLandmarkOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    public List<BaseLandmark> GetAllLandmarks() {
        List<BaseLandmark> allLandmarks = new List<BaseLandmark>();
#if WORLD_CREATION_TOOL
        List<HexTile> choices = worldcreator.WorldCreatorManager.Instance.allTiles;
#else
        List<HexTile> choices = GridMap.Instance.hexTiles;
#endif
        for (int i = 0; i < choices.Count; i++) {
            HexTile currTile = choices[i];
            if (currTile.landmarkOnTile != null) {
                allLandmarks.Add(currTile.landmarkOnTile);
            }
        }
        return allLandmarks;
    }
    public List<LandmarkStructureSprite> GetLandmarkTileSprites(HexTile tile, LANDMARK_TYPE landmarkType, RACE race = RACE.NONE) {
        LandmarkData data = GetLandmarkData(landmarkType);
        if (data.biomeTileSprites.Count > 0) { //if the landmark type has a biome type tile sprite set, use that instead
            if (data.biomeTileSprites.ContainsKey(tile.biomeType)) {
                return data.biomeTileSprites[tile.biomeType]; //prioritize biome type sprites
            }
        }
        if (race == RACE.HUMANS) {
            return data.humansLandmarkTileSprites;
        } else if (race == RACE.ELVES) {
            return data.elvenLandmarkTileSprites;
        } else {
            if (data.neutralTileSprites.Count > 0) {
                return data.neutralTileSprites;
            } else {
                return null;
            }
        }
        
    }
    public List<string> GetUsedTownCenterTemplates() {
        List<string> templates = new List<string>();
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            if (currArea.areaMap != null) {
                templates.Add(currArea.areaMap.usedTownCenterTemplateName);
            }
        }
        return templates;
    }
    #endregion

    #region Areas
    public void LoadAreas(WorldSaveData data) {
        if (data.areaData != null) {
            for (int i = 0; i < data.areaData.Count; i++) {
                AreaSaveData areaData = data.areaData[i];
                Area newArea = CreateNewArea(areaData);
                if (newArea.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
                    continue; //player area should not be owned by any saved faction, owning of that area is done during player generation.
                }
                if (areaData.ownerID != -1) {
                    Faction owner = FactionManager.Instance.GetFactionBasedOnID(areaData.ownerID);
                    if (owner != null) {
#if WORLD_CREATION_TOOL
                        OwnArea(owner, owner.raceType, newArea);
#endif
#if !WORLD_CREATION_TOOL
                        if (owner.isActive) {
                            OwnArea(owner, owner.race, newArea);
                        }
                        else {
                            Faction neutralFaction = FactionManager.Instance.neutralFaction;
                            if (neutralFaction != null) {
                                neutralFaction.AddToOwnedAreas(newArea); //this will add area to the neutral factions owned area list, but the area's owner will still be null
                            }
                        }
#endif
                    }
                }
#if !WORLD_CREATION_TOOL
                else {
                    Faction neutralFaction = FactionManager.Instance.neutralFaction;
                    if (neutralFaction != null) {
                        neutralFaction.AddToOwnedAreas(newArea); //this will add area to the neutral factions owned area list, but the area's owner will still be null
                    }
                }
#endif
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
        if (locationPortraits.ContainsKey(newArea.areaType)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.areaType]);
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        return newArea;
    }
    public void RemoveArea(Area area) {
        allAreas.Remove(area);
        //Messenger.Broadcast(Signals.AREA_DELETED, area);
    }
    public Area CreateNewArea(AreaSaveData data) {
        Area newArea = new Area(data);
        if (locationPortraits.ContainsKey(newArea.areaType)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.areaType]);
        }
#if !WORLD_CREATION_TOOL
        GenerateAreaMap(newArea);
#endif
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        return newArea;
    }
    //private bool hasPendingMapGenerationJob = false;
    //private Area pendingAreaMap;
    //private JobHandle pendingJob;
    public void GenerateAreaMap(Area area) {
        GameObject areaMapGO = GameObject.Instantiate(innerStructurePrefab, areaMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();
        InteriorMapManager.Instance.CleanupForTownGeneration();
        MultiThreadPool.Instance.AddToThreadPool(new AreaMapGenerationThread(area, areaMap));
    }
    public void OnFinishedGeneratingAreaMap(AreaMapGenerationThread thread) {
        Debug.Log("Finished generating map for " + thread.area.name);
        Debug.Log(thread.log);
        thread.areaMap.DrawMap(thread.generatedSettings);
        thread.area.SetAreaMap(thread.areaMap);
        thread.area.PlaceTileObjects();
        thread.areaMap.GenerateDetails();
        thread.areaMap.RotateTiles();

        thread.areaMap.OnMapGenerationFinished();
        thread.area.OnMapGenerationFinished();
        InteriorMapManager.Instance.OnCreateAreaMap(thread.areaMap);
        CharacterManager.Instance.PlaceInitialCharacters(thread.area);
        InteriorMapManager.Instance.ShowAreaMap(thread.area);
        thread.area.OnAreaSetAsActive();
        UIManager.Instance.SetInteriorMapLoadingState(false);
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
    public Area GetRandomAreaOfType(AREA_TYPE type) {
        List<Area> filteredAreas = new List<Area>();
        for (int i = 0; i < allAreas.Count; i++) {
            Area area = allAreas[i];
            if (area.areaType == type) {
                filteredAreas.Add(area);
            }
        }
        if(filteredAreas.Count > 0) {
            return filteredAreas[UnityEngine.Random.Range(0, filteredAreas.Count)];
        }
        return null;
    }
    public void OwnArea(Faction newOwner, RACE newRace, Area area) {
        if (area.owner != null) {
            UnownArea(area);
        }
        newOwner.AddToOwnedAreas(area);
        area.SetOwner(newOwner);
        area.SetRaceType(newRace);
        area.TintStructuresInArea(newOwner.factionColor);
    }
    public void UnownArea(Area area) {
        if (area.owner != null) {
            area.owner.RemoveFromOwnedAreas(area);
        }
        area.SetOwner(null);
        area.SetRaceType(area.defaultRace.race); //Return area to its default race
        area.TintStructuresInArea(Color.white);
        Messenger.Broadcast(Signals.AREA_OCCUPANY_CHANGED, area);
    }
    public void LoadAdditionalAreaData() {
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            currArea.LoadAdditionalData();
        }
    }
    public WeightedDictionary<AreaCharacterClass> GetDefaultClassWeights(RACE race) {
        if (defaultRaceDefenders.ContainsKey(race)) {
            WeightedDictionary<AreaCharacterClass> weights = new WeightedDictionary<AreaCharacterClass>();
            for (int i = 0; i < defaultRaceDefenders[race].Count; i++) {
                RaceAreaDefenderSetting currSetting = defaultRaceDefenders[race][i];
                weights.AddElement(new AreaCharacterClass() { className = currSetting.className }, currSetting.weight);
            }
            return weights;
        }
        throw new System.Exception("There is no default defender weights for " + race.ToString());
        //return null;
    }
    public Vector2 GetAreaNameplatePosition(Area area) {
        //switch (area.name) {
        //    case "Cardell":
        //        return new Vector2(-1.1f, 4.6f);
        //    case "Narris":
        //        return new Vector2(20.5f, -3f);
        //    default:
        //        break;
        //}
        Vector2 defaultPos = area.coreTile.transform.position;
        defaultPos.y -= 1.25f;
        return defaultPos;
    }
    public Area GetRandomUnownedAreaWithAvailableStructure(STRUCTURE_TYPE structureType) {
        List<Area> areaChoices = new List<Area>();
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            if(allAreas[i].owner == null && allAreas[i].GetNumberOfUnoccupiedStructure(structureType) > 0) {
                areaChoices.Add(currArea);
            }
        }
        if(areaChoices.Count > 0) {
            return areaChoices[UnityEngine.Random.Range(0, areaChoices.Count)];
        }
        return null;
    }
    #endregion

    #region Location Structures
    public LocationStructure CreateNewStructureAt(Area location, STRUCTURE_TYPE type, bool isInside = true) {
        LocationStructure createdStructure = null;
        switch (type) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location, isInside);
                break;
            default:
                createdStructure = new LocationStructure(type, location, isInside);
                break;
        }
        if (createdStructure != null) {
            location.AddStructure(createdStructure);
        }
        return createdStructure;
    }
    #endregion

    #region Regions
    public void DivideToRegions(List<HexTile> tiles, int regionCount, int mapSize, out Region[] generatedRegions) {
        List<HexTile> regionCoreTileChoices = new List<HexTile>(tiles.Where(x => x.elevationType != ELEVATION.WATER));
        List<HexTile> remainingTiles = new List<HexTile>(tiles);
        Region[] regions = new Region[regionCount];
        for (int i = 0; i < regionCount; i++) {
            HexTile chosenTile = regionCoreTileChoices[Random.Range(0, regionCoreTileChoices.Count)];
            Region newRegion = new Region(chosenTile);
            int range = Mathf.CeilToInt(mapSize * 0.01f);//1% of map size
            List<HexTile> tilesInRange = chosenTile.GetTilesInRange(range);
            Utilities.ListRemoveRange(regionCoreTileChoices, tilesInRange);
            regions[i] = newRegion;
            remainingTiles.Remove(chosenTile);
        }

        //assign each remaining tile to a region, based on each tiles distance from a core tile.
        for (int i = 0; i < remainingTiles.Count; i++) {
            HexTile currTile = remainingTiles[i];
            Region nearestRegion = null;
            float nearestDistance = 99999f;
            for (int j = 0; j < regions.Length; j++) {
                Region currRegion = regions[j];
                float dist = Vector2.Distance(currTile.transform.position, currRegion.coreTile.transform.position);
                if (dist < nearestDistance) {
                    nearestRegion = currRegion;
                    nearestDistance = dist;
                }
            }
            nearestRegion.AddTile(currTile);
        }
        generatedRegions = regions;
        //for (int i = 0; i < generatedRegions.Length; i++) {
        //    generatedRegions[i].RedetermineCore();
        //}
    }
    #endregion
}

public class Region {

    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }

    public Color regionColor;
    private List<HexTile> allTiles {
        get {
            return tiles;
        }
    }

    public Region(HexTile coreTile) {
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        AddTile(coreTile);
        regionColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            //if (tile != coreTile) {
            //    tile.spriteRenderer.color = regionColor;
            //}
        }
    }

    /// <summary>
    /// Get all tiles in this region that don't have landmarks
    /// and are not near any landmarks. (3 tiles to be exact)
    /// </summary>
    /// <returns>List of valid tiles.</returns>
    public List<HexTile> GetValidTilesForLandmarks() {
        List<HexTile> valid = new List<HexTile>();
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currTile = allTiles[i];
            List<HexTile> tilesInRange = currTile.GetTilesInRange(2);
            //if current tile meets the ff requirements, it is valid
            // - Does not have a landamrk on it yet.
            // - Is not a water tile.
            // - Does not have a landmark within range.(3 tiles)
            if (currTile.landmarkOnTile == null
                && currTile.elevationType == ELEVATION.PLAIN
                && !currTile.IsAtEdgeOfMap()
                && tilesInRange.Where(x => x.landmarkOnTile != null).Count() == 0) {
                valid.Add(currTile);
            } 
        }
        return valid;
    }

    public void RedetermineCore() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int x = (minX + maxX) / 2;
        int y = (minY + maxY) / 2;

        coreTile = GridMap.Instance.map[x, y];
        if (!tiles.Contains(coreTile)) {
            throw new System.Exception("Region does not contain new core tile! " + coreTile.ToString());
        }
    }
}

public struct SettlementSettings {
    public AREA_TYPE settlementType;
    public int citizenCount;
}

//#region Map Job
//public struct AreaMapGenerationJob : IJob {

//    public string areaName;
//    public AREA_TYPE areaType;
//    public NativeArray<StructureTemplate> validTownCenterTemplates; //get VALID town center templates before executing this job
//    public NativeHashMap<int, NativeArray<StructureTemplate>> structureTemplates; //int is the STRUCTURE_TYPE enum converted to int
//    public NativeHashMap<int, int> areaStructures; //Key int is the STRUCTURE_TYPE enum converted to int, Value int is the number of structures of that type

//    public void Execute() {
//        GenerateInnerStructures();
//    }

//    public void GenerateInnerStructures() {
//        //ClearAllTilemaps();
//        //insideTiles = new List<LocationGridTile>();
//        //outsideTiles = new List<LocationGridTile>();
//        if (areaType != AREA_TYPE.DUNGEON && areaType != AREA_TYPE.DEMONIC_INTRUSION) {
//            //if this area is not a dungeon type
//            InteriorMapManager.Instance.CleanupForTownGeneration();
//            //first get a town center template that has the needed connections for the structures in the area
//            NativeArray<StructureTemplate> validTownCenters = validTownCenterTemplates;
//            //Once a town center is chosen
//            StructureTemplate chosenTownCenter = validTownCenters[Utilities.rng.Next(0, validTownCenters.Length)];
            
//            ////Place that template in the area generation tilemap
//            //InteriorMapManager.Instance.DrawTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
//            ////DrawTiles(InteriorMapManager.Instance.agGroundTilemap, chosenTownCenter.groundTiles, Vector3Int.zero);
//            //chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);
//            ////then iterate through all the structures in this area, making sure that the chosen template for the structure can connect to the town center
//            //foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
//            //    if (area.name == "Gloomhollow") {
//            //        if (!keyValuePair.Key.ShouldBeGeneratedFromTemplate() && keyValuePair.Key != STRUCTURE_TYPE.EXPLORE_AREA) {
//            //            //allow explore areas to be generated in gloomhollow
//            //            continue; //skip
//            //        }
//            //    } else {
//            //        if (!keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
//            //            continue; //skip
//            //        }
//            //    }

//            //    int structuresToCreate = keyValuePair.Value.Count;
//            //    if (area.name == "Gloomhollow") {
//            //        structuresToCreate = 5; //hardcoded to 5
//            //    }

//            //    for (int i = 0; i < structuresToCreate; i++) {
//            //        List<StructureTemplate> templates = InteriorMapManager.Instance.GetStructureTemplates(keyValuePair.Key); //placed this inside loop so that instance of template is unique per iteration
//            //        List<StructureTemplate> choices = GetTemplatesThatCanConnectTo(chosenTownCenter, templates);
//            //        if (choices.Count == 0) {
//            //            //NOTE: Show a warning log when there are no valid structure templates for the current structure
//            //            throw new System.Exception("There are no valid " + keyValuePair.Key.ToString() + " templates to connect to town center in area " + area.name);
//            //        }
//            //        StructureTemplate chosenTemplate = choices[Random.Range(0, choices.Count)];
//            //        StructureConnector townCenterConnector;
//            //        StructureConnector chosenTemplateConnector = chosenTemplate.GetValidConnectorTo(chosenTownCenter, out townCenterConnector);

//            //        Vector3Int shiftTemplateBy = InteriorMapManager.Instance.GetMoveUnitsOfTemplateGivenConnections(chosenTemplate, chosenTemplateConnector, townCenterConnector);
//            //        townCenterConnector.SetIsOpen(false);
//            //        chosenTemplateConnector.SetIsOpen(false);
//            //        InteriorMapManager.Instance.DrawStructureTemplateForGeneration(chosenTemplate, shiftTemplateBy, keyValuePair.Key);
//            //    }
//            //}
//            ////once all structures are placed, get the occupied bounds in the area generation tilemap, and use that size to generate the actual grid for this map
//            //TownMapSettings generatedSettings = InteriorMapManager.Instance.GetTownMapSettings();
//            //GenerateGrid(generatedSettings);
//            //SplitMap();
//            //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
//            //DrawTownMap(generatedSettings, startPoint);
//            ////once generated, just copy the generated structures to the actual map.
//            //if (area.name == "Gloomhollow") {
//            //    LocationStructure exploreArea = area.GetRandomStructureOfType(STRUCTURE_TYPE.EXPLORE_AREA);
//            //    for (int i = 0; i < insideTiles.Count; i++) {
//            //        LocationGridTile currTile = insideTiles[i];
//            //        TileBase structureAsset = structureTilemap.GetTile(currTile.localPlace);
//            //        if (structureAsset == null || !structureAsset.name.Contains("wall")) {
//            //            currTile.SetStructure(exploreArea);
//            //            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
//            //        }
//            //    }
//            //} else {
//            //    PlaceStructures(generatedSettings, startPoint);
//            //}
//            //AssignOuterAreas(insideTiles, outsideTiles);
//            //else use the old structure generation
//        } 
//        //else {
//        //    OldStructureGeneration();
//        //}
//    }


//}
//#endregion

