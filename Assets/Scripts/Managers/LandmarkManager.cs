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

    public List<BaseLandmark> allLandmarks;
    public List<Area> allAreas;
    public List<Area> allNonPlayerAreas {
        get { return allAreas.Where(x => x != PlayerManager.Instance.player.playerArea).ToList(); }
    }

    [SerializeField] private GameObject landmarkGO;

    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;

    public AreaTypeSpriteDictionary locationPortraits; //NOTE: Move this to world creation when time permits.

    [Header("Inner Structures")]
    [SerializeField] private GameObject innerStructurePrefab;
    [SerializeField] private Transform areaMapsParent;

    [Header("Connections")]
    [SerializeField] private GameObject landmarkConnectionPrefab;

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        corruptedLandmarksCount = 0;
        allAreas = new List<Area>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
    }
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
    public bool AreAllNonPlayerAreasCorrupted() {
        List<Area> areas = allNonPlayerAreas;
        for (int i = 0; i < areas.Count; i++) {
            if (!areas[i].coreTile.isCorrupted) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Landmark Generation
    public void GenerateSettlements(IntRange settlementRange, Region[] regions, IntRange citizenRange, out BaseLandmark portal) {
        //place portal first
        Region chosenPlayerRegion = regions[Random.Range(0, regions.Length)];
        List<HexTile> playerTileChoices = chosenPlayerRegion.GetValidTilesForLandmarks();
        HexTile chosenTile = playerTileChoices[Random.Range(0, playerTileChoices.Count)];
        Area playerArea = CreateNewArea(chosenTile, AREA_TYPE.DEMONIC_INTRUSION, 0);
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
            bool isNear = false;
            //Settlements with fewer inhabitants are spawned near the portal, while stronger, more populated settlements are spawned further away. 
            if (citizenRange.IsNearUpperBound(citizenCount)) {
                //citizen count is more than half of the range. Place settlement at a far away region
                regionChoices = farRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                if (regionChoices.Count == 0) { //if there are no valid far regions, place the landmark at a near region instead
                    regionChoices = nearRegions.Where(x => x.GetValidTilesForLandmarks().Count > 0).ToList();
                }
            } else {
                isNear = true;
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
            tileChoices = tileChoices.OrderBy(x => Vector2.Distance(playerArea.coreTile.transform.position, x.transform.position)).ToList();
            HexTile chosenRegionTile = null;
            if (isNear) {
                chosenRegionTile = tileChoices[0];
            } else {
                chosenRegionTile = tileChoices[Random.Range(tileChoices.Count / 2, tileChoices.Count)];
            }
            
            Area newArea = CreateNewArea(chosenRegionTile, settlementType, citizenCount);
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
    }
    public void SetCascadingLevelsForAllCharacters(HexTile portalTile) {
        List<Area> arrangedAreas = allAreas.OrderBy(x => x.coreTile.GetTileDistanceTo(portalTile)).ToList();
        int initialLeaderLevel = 2;
        for (int i = 0; i < arrangedAreas.Count; i++) {
            if(arrangedAreas[i].coreTile == portalTile) {
                arrangedAreas.RemoveAt(i);
                break;
            }
        }
        for (int i = 0; i < arrangedAreas.Count; i++) {
            for (int j = 0; j < arrangedAreas[i].areaResidents.Count; j++) {
                Character character = arrangedAreas[i].areaResidents[j];
                int leaderLevel = initialLeaderLevel * (i + 1);
                if (character.role.roleType == CHARACTER_ROLE.LEADER) {
                    character.SetLevel(leaderLevel);
                } else {
                    character.SetLevel(leaderLevel - 1);
                }
            }
        }
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
    public void GenerateLandmarks(RandomWorld world, out BaseLandmark portal) {
        WeightedDictionary<LANDMARK_YIELD_TYPE> yieldTypeChances = new WeightedDictionary<LANDMARK_YIELD_TYPE>();
        yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.SUMMON, 15);
        yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.ARTIFACT, 15);
        yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.ABILITY, 20);
        yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.SKIRMISH, 25);
        yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.STORY_EVENT, 25);

        //create player portal
        portal = CreateMajorLandmarkOnColumn(world.columns.First(), LANDMARK_TYPE.DEMONIC_PORTAL, AREA_TYPE.DEMONIC_INTRUSION, 0);

        for (int i = 0; i < world.columns.Count; i++) {
            TileColumn previousColumn = world.columns.ElementAtOrDefault(i - 1);
            TileColumn currColumn = world.columns[i];
            TileColumn nextColumn = world.columns.ElementAtOrDefault(i + 1);
            if (nextColumn != null) {
                int connections = 0;
                //create connections to nextColumn
                if (currColumn.hasMajorLandmark) {
                    //current column has a settlement, create 2 connections outward
                    connections = 2;
                    //create connections
                    TileRow rowWithMajorLandmark = currColumn.GetRowWithMajorLandmark();
                    for (int j = 0; j < connections; j++) {
                        //only choose rows that don't have landmarks or rows that have landmarks but are not yet connected to the current columns major landmark
                        List<TileRow> rowChoices = currColumn.GetValidRowsInNextColumnForLandmarks(rowWithMajorLandmark, nextColumn)
                            .Where(x => x.landmark == null || !x.landmark.IsConnectedWith(rowWithMajorLandmark.landmark)).ToList(); 
                        if (rowChoices.Count == 0) {
                            break;
                        }
                        TileRow chosenRow = rowChoices[Random.Range(0, rowChoices.Count)];
                        BaseLandmark landmark;
                        if (chosenRow.hasLandmark) {
                            //connect to that landmark
                            landmark = chosenRow.landmark;
                        } else {
                            //create landmark on that row, and connect to it.
                            landmark = CreateMinorLandmarkOnRow(chosenRow, yieldTypeChances);
                        }
                        ConnectLandmarks(rowWithMajorLandmark.landmark, landmark);
                    }
                } else if (nextColumn.hasMajorLandmark) {
                    //create settlement first
                    TileRow rowInNextColumnWithMajorLandmark = nextColumn.GetRowWithMajorLandmark();
                    if (rowInNextColumnWithMajorLandmark == null) {
                        BaseLandmark landmark = CreateMajorLandmarkOnColumn(nextColumn, LANDMARK_TYPE.PALACE, Utilities.RandomSettlementType(), Random.Range(WorldConfigManager.Instance.minCitizenCount, WorldConfigManager.Instance.maxCitizenCount));
                        rowInNextColumnWithMajorLandmark = nextColumn.GetRowWithMajorLandmark();
                    }
                    
                    //next column has a settlement, connect all landmarks to that
                    connections = 1;
                    for (int j = 0; j < currColumn.rows.Length; j++) {
                        TileRow currRow = currColumn.rows[j];
                        if (currRow.hasLandmark) {
                            BaseLandmark currLandmark = currRow.landmark;
                            //create connections
                            for (int k = 0; k < connections; k++) {
                                //connect to that settlement
                                ConnectLandmarks(currRow.landmark, rowInNextColumnWithMajorLandmark.landmark);
                            }
                        }
                    }
                } else {
                    //create connections per Landmark in the current column
                    for (int j = 0; j < currColumn.rows.Length; j++) {
                        TileRow currRow = currColumn.rows[j];
                        if (currRow.hasLandmark) {
                            BaseLandmark currLandmark = currRow.landmark;
                            connections = 1; //75% 1 connection
                            if (Random.Range(0, 100) < 25) { //25% 2 connections
                                connections = 2;
                            }
                            //create connections
                            for (int k = 0; k < connections; k++) {
                                List<TileRow> rowChoices = currColumn.GetValidRowsInNextColumnForLandmarks(currRow, nextColumn)
                                    .Where(x => x.landmark == null || !x.landmark.IsConnectedWith(currLandmark)).ToList();
                                if (rowChoices.Count == 0) {
                                    break;
                                }
                                TileRow chosenRow = rowChoices[Random.Range(0, rowChoices.Count)];
                                BaseLandmark landmark;
                                if (chosenRow.hasLandmark) {
                                    landmark = chosenRow.landmark;
                                    //connect to that landmark
                                } else {
                                    //create landmark on that row, and connect to it.
                                    landmark = CreateMinorLandmarkOnRow(chosenRow, yieldTypeChances);
                                }
                                ConnectLandmarks(currLandmark, landmark);
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < currColumn.rows.Length; j++) {
                if(currColumn.rows[j] != null && currColumn.rows[j].landmark != null) {
                    BaseLandmark rowLandmark = currColumn.rows[j].landmark;
                    rowLandmark.SetSameColumnLandmarks(currColumn.GetAllLandmarksInColumn(rowLandmark));
                    rowLandmark.SetSameRowTiles(currColumn.rows[j].GetAllTiles(GridMap.Instance.map).ToList());
                }
            }
        }
    }
    /// <summary>
    /// Add a connection between 2 landmarks.
    /// </summary>
    /// <param name="landmark1">The landmark that will connect to landmark 2.</param>
    /// <param name="landmark2">The landmark accepting the connection from landmark 1.</param>
    public void ConnectLandmarks(BaseLandmark landmark1, BaseLandmark landmark2) {
        landmark1.AddOutGoingConnection(landmark2);
        landmark2.AddInGoingConnection(landmark1);
        GameObject lineGO = GameObject.Instantiate(landmarkConnectionPrefab, landmark1.tileLocation.transform);
        LineRenderer line = lineGO.GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, landmark1.tileLocation.transform.position);
        line.SetPosition(1, landmark2.tileLocation.transform.position);

    }
    private BaseLandmark CreateMinorLandmarkOnRow(TileRow row, WeightedDictionary<LANDMARK_YIELD_TYPE> yieldTypeChances) {
        LANDMARK_YIELD_TYPE chosenYieldType = yieldTypeChances.PickRandomElementGivenWeights();
        List<HexTile> tiles = row.GetElligibleTilesForLandmark(GridMap.Instance.map);
        HexTile chosenTile = tiles[Random.Range(0, tiles.Count)];

        List<LANDMARK_TYPE> landmarkChoices = WorldConfigManager.Instance.GetPossibleLandmarks(chosenTile.biomeType, chosenYieldType);
        LANDMARK_TYPE chosenType = landmarkChoices[Random.Range(0, landmarkChoices.Count)];

        BaseLandmark landmark = CreateNewLandmarkOnTile(chosenTile, chosenType);
        row.SetLandmark(landmark);
        landmark.SetYieldType(chosenYieldType);
        return landmark;
    }
    private BaseLandmark CreateMajorLandmarkOnColumn(TileColumn currColumn, LANDMARK_TYPE type, AREA_TYPE areaType, int citizenCount) {
        int randomRow = Random.Range(0, currColumn.rows.Length);
        TileRow chosenRow = currColumn.rows[randomRow];
        List<HexTile> choices = chosenRow.GetElligibleTilesForLandmark(GridMap.Instance.map);
        HexTile chosenTile = choices[Random.Range(0, choices.Count)];
        
        Area newArea = CreateNewArea(chosenTile, areaType, citizenCount);
        BaseLandmark landmark = CreateNewLandmarkOnTile(chosenTile, type);
        chosenRow.SetLandmark(landmark);
        if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            newArea.SetName("Portal"); //need this so that when player is initialized. This area will be assigned to the player.
        } else {
            Faction faction = FactionManager.Instance.CreateNewFaction();
            if (areaType == AREA_TYPE.ELVEN_SETTLEMENT) {
                faction.SetInitialFactionLeaderClass("Queen");
                faction.SetInitialFactionLeaderGender(GENDER.FEMALE);
                faction.SetRace(RACE.ELVES);
            } else if (areaType == AREA_TYPE.HUMAN_SETTLEMENT) {
                faction.SetInitialFactionLeaderClass("King");
                faction.SetInitialFactionLeaderGender(GENDER.MALE);
                faction.SetRace(RACE.HUMANS);
            }
            OwnArea(faction, faction.race, newArea);
            newArea.GenerateStructures(citizenCount);
            faction.GenerateStartingCitizens(2, 1, citizenCount); //9,7
        }        
        return landmark;
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
    public Area CreateNewArea(HexTile coreTile, AREA_TYPE areaType, int citizenCount, List<HexTile> tiles = null) {
        Area newArea = new Area(coreTile, areaType, citizenCount);
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
    public Area CreateNewArea(SaveDataArea saveDataArea) {
        Area newArea = new Area(saveDataArea);

        if(newArea.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            for (int i = 0; i < saveDataArea.tileIDs.Count; i++) {
                HexTile tile = GridMap.Instance.hexTiles[saveDataArea.tileIDs[i]];
                newArea.AddTile(tile);
                tile.SetCorruption(true);
            }
        } else {
            for (int i = 0; i < saveDataArea.tileIDs.Count; i++) {
                newArea.AddTile(GridMap.Instance.hexTiles[saveDataArea.tileIDs[i]]);
            }
        }

        if (locationPortraits.ContainsKey(newArea.areaType)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.areaType]);
        }
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
        thread.areaMap.GenerateDetails();
        thread.area.PlaceTileObjects();
        //thread.areaMap.RotateTiles();

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
    public void OwnArea(Faction newOwner, RACE newRace, Area area) {
        if (area.owner != null) {
            UnownArea(area);
        }
        newOwner.AddToOwnedAreas(area);
        area.SetOwner(newOwner);
        //area.SetRaceType(newRace);
        area.TintStructuresInArea(newOwner.factionColor);
    }
    public void UnownArea(Area area) {
        if (area.owner != null) {
            area.owner.RemoveFromOwnedAreas(area);
        }
        area.SetOwner(null);
        //area.SetRaceType(area.defaultRace.race); //Return area to its default race
        area.TintStructuresInArea(Color.white);
        Messenger.Broadcast(Signals.AREA_OCCUPANY_CHANGED, area);
    }
    public void LoadAdditionalAreaData() {
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            currArea.LoadAdditionalData();
        }
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
            List<HexTile> tilesInRange = currTile.GetTilesInRange(3);
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