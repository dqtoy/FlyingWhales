using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;
    public static readonly int Max_Connections = 3;
    public const int DELAY_DIVINE_INTERVENTION_DURATION = 144;
    public const int SUMMON_MINION_DURATION = 96;

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

    public AreaTypeSpriteDictionary locationPortraits;

    [Header("Inner Structures")]
    [SerializeField] private GameObject innerStructurePrefab;
    [SerializeField] private Transform areaMapsParent;

    [Header("Connections")]
    [SerializeField] private GameObject landmarkConnectionPrefab;

    public Area enemyOfPlayerArea { get; private set; }

    //The Profane


    //The Portal

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
        //if (newLandmark.tileLocation.areaOfTile != null && newLandmark.tileLocation.areaOfTile.owner != null) {
        //    OccupyLandmark(newLandmark, newLandmark.tileLocation.areaOfTile.owner);
        //}

        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
#endif
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        landmarkOnTile.DestroyLandmark();
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        return newLandmark;
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
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, LANDMARK_TYPE type) {
        if (type.IsPlayerLandmark()) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(type.ToString());
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, type) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, type);
        }
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, LandmarkSaveData data) {
        if (data.landmarkType.IsPlayerLandmark()) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.landmarkType.ToString());
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, data) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, data);
        }
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, SaveDataLandmark data) {
        if (data.landmarkType.IsPlayerLandmark()) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.landmarkType.ToString());
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, data) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, data);
        }
    }
    #endregion

    #region Landmark Generation
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
    public LandmarkData GetLandmarkData(string landmarkName) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkTypeString == landmarkName) {
                return currData;
            }
        }
        throw new System.Exception("There is no landmark data for " + landmarkName);
    }
    public void InitializeLandmarks() {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            currLandmark.Initialize();
        }
    }
    //public void GenerateMinorLandmarks(List<HexTile> allTiles) {
    //    Dictionary<BIOMES, LANDMARK_TYPE[]> landmarkChoices = new Dictionary<BIOMES, LANDMARK_TYPE[]>() {
    //        { BIOMES.GRASSLAND, new LANDMARK_TYPE[] {
    //            LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.OUTPOST, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.ANCIENT_RUIN, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
    //        } },
    //        { BIOMES.FOREST, new LANDMARK_TYPE[] {
    //            LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.OUTPOST, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.ANCIENT_RUIN, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
    //        } },
    //        { BIOMES.DESERT, new LANDMARK_TYPE[] {
    //            LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.ANCIENT_RUIN, LANDMARK_TYPE.PYRAMID, LANDMARK_TYPE.CAVE
    //        } },
    //        { BIOMES.SNOW, new LANDMARK_TYPE[] {
    //            LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.OUTPOST, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.ANCIENT_RUIN, LANDMARK_TYPE.ICE_PIT, LANDMARK_TYPE.CAVE
    //        } },
    //        { BIOMES.TUNDRA, new LANDMARK_TYPE[] {
    //            LANDMARK_TYPE.ABANDONED_MINE, LANDMARK_TYPE.OUTPOST, LANDMARK_TYPE.BANDIT_CAMP, LANDMARK_TYPE.ANCIENT_RUIN, LANDMARK_TYPE.ICE_PIT, LANDMARK_TYPE.CAVE
    //        } },
    //    };
    //    for (int i = 0; i < allTiles.Count; i++) {
    //        HexTile currTile = allTiles[i];
    //        List<HexTile> tilesInRange = currTile.GetTilesInRange(2);
    //        if (currTile.landmarkOnTile == null
    //            && currTile.elevationType == ELEVATION.PLAIN
    //            && !currTile.IsAtEdgeOfMap()
    //            && tilesInRange.Where(x => x.landmarkOnTile != null).Count() == 0) {
    //            LANDMARK_TYPE[] minorLandmarkTypes = landmarkChoices[currTile.biomeType];
    //            LANDMARK_TYPE chosenLandmarkType = minorLandmarkTypes[Random.Range(0, minorLandmarkTypes.Length)];
    //            CreateNewLandmarkOnTile(currTile, chosenLandmarkType);
    //        }
    //    }
    //}
    public void GenerateLandmarksNew(Region[] regions, out BaseLandmark portal, out BaseLandmark settlement) {
        //place portal first
        Region[] corners = GetCornerRegions();
        int portalCorner = Random.Range(0, 4);
        Region portalRegion = corners[portalCorner];
        Area portalArea = CreateNewArea(portalRegion, AREA_TYPE.DEMONIC_INTRUSION, 0);
        BaseLandmark portalLandmark = CreateNewLandmarkOnTile(portalRegion.coreTile, LANDMARK_TYPE.THE_PORTAL);
        portalArea.region.SetName("Portal"); //need this so that when player is initialized. This area will be assigned to the player.
        portal = portalLandmark;

        //place settlement at opposite corner
        int oppositeCorner = GetOppositeCorner(portalCorner);
        Region settlementRegion = corners[oppositeCorner];
        AREA_TYPE settlementType = Utilities.RandomSettlementType();
        int citizenCount = Random.Range(WorldConfigManager.Instance.minCitizenCount, WorldConfigManager.Instance.maxCitizenCount + 1);
        Area settlementArea = CreateNewArea(settlementRegion, settlementType, citizenCount);
        SetEnemyPlayerArea(settlementArea);
        BaseLandmark settlementLandmark = CreateNewLandmarkOnTile(settlementRegion.coreTile, LANDMARK_TYPE.PALACE);
        settlement = settlementLandmark;
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
        OwnRegion(faction, faction.race, settlementRegion);
        settlementArea.GenerateStructures(citizenCount);
        faction.GenerateStartingCitizens(2, 1, citizenCount); //9,7

        List<Region> availableRegions = new List<Region>(regions);
        availableRegions.Remove(portalRegion);
        availableRegions.Remove(settlementRegion);

        //place all other landmarks
        Dictionary<LANDMARK_TYPE, int> landmarks = WorldConfigManager.Instance.GetLandmarksForGeneration(regions.Length - 2); //subtracted 2 because of portal and settlement
        //string otherLandmarkSummary = "Will generate the following landmarks: ";
        foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in landmarks) {
            //otherLandmarkSummary += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
            for (int i = 0; i < kvp.Value; i++) {
                Region chosenRegion = availableRegions[Random.Range(0, availableRegions.Count)];
                BaseLandmark landmark = CreateNewLandmarkOnTile(chosenRegion.coreTile, kvp.Key);
                availableRegions.Remove(chosenRegion);
            }
        }
        //Debug.Log(otherLandmarkSummary);
    }
    //public void GenerateLandmarks(RandomWorld world, out BaseLandmark portal) {
    //    WeightedDictionary<LANDMARK_YIELD_TYPE> yieldTypeChances = new WeightedDictionary<LANDMARK_YIELD_TYPE>();
    //    yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.SUMMON, 15);
    //    yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.ARTIFACT, 15);
    //    yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.ABILITY, 20);
    //    yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.SKIRMISH, 25);
    //    yieldTypeChances.AddElement(LANDMARK_YIELD_TYPE.STORY_EVENT, 25);

    //    int citizenCount = Random.Range(WorldConfigManager.Instance.minCitizenCountFirstSettlement, WorldConfigManager.Instance.maxCitizenCountFirstSettlement + 1);
    //    //create player portal
    //    portal = CreateMajorLandmarkOnColumn(world.columns.First(), LANDMARK_TYPE.DEMONIC_PORTAL, AREA_TYPE.DEMONIC_INTRUSION, 0);

    //    for (int i = 0; i < world.columns.Count; i++) {
    //        TileColumn previousColumn = world.columns.ElementAtOrDefault(i - 1);
    //        TileColumn currColumn = world.columns[i];
    //        TileColumn nextColumn = world.columns.ElementAtOrDefault(i + 1);
    //        if (nextColumn != null) {
    //            int connections = 0;
    //            //create connections to nextColumn
    //            if (currColumn.hasMajorLandmark) {
    //                //current column has a settlement, create 2 connections outward
    //                connections = 2;
    //                //create connections
    //                TileRow rowWithMajorLandmark = currColumn.GetRowWithMajorLandmark();
    //                for (int j = 0; j < connections; j++) {
    //                    //only choose rows that don't have landmarks or rows that have landmarks but are not yet connected to the current columns major landmark
    //                    List<TileRow> rowChoices = currColumn.GetValidRowsInNextColumnForLandmarks(rowWithMajorLandmark, nextColumn)
    //                        .Where(x => x.landmark == null || !x.landmark.IsConnectedWith(rowWithMajorLandmark.landmark)).ToList();
    //                    if (rowChoices.Count == 0) {
    //                        break;
    //                    }
    //                    TileRow chosenRow = rowChoices[Random.Range(0, rowChoices.Count)];
    //                    BaseLandmark landmark;
    //                    if (chosenRow.hasLandmark) {
    //                        //connect to that landmark
    //                        landmark = chosenRow.landmark;
    //                    } else {
    //                        //create landmark on that row, and connect to it.
    //                        landmark = CreateMinorLandmarkOnRow(chosenRow, yieldTypeChances);
    //                    }
    //                    ConnectLandmarks(rowWithMajorLandmark.landmark, landmark);
    //                }
    //            } else if (nextColumn.hasMajorLandmark) {
    //                //create settlement first
    //                TileRow rowInNextColumnWithMajorLandmark = nextColumn.GetRowWithMajorLandmark();
    //                if (rowInNextColumnWithMajorLandmark == null) {
    //                    BaseLandmark landmark = CreateMajorLandmarkOnColumn(nextColumn, LANDMARK_TYPE.PALACE, Utilities.RandomSettlementType(), citizenCount);
    //                    rowInNextColumnWithMajorLandmark = nextColumn.GetRowWithMajorLandmark();
    //                    citizenCount += Random.Range(WorldConfigManager.Instance.minCitizenCountIncreasePerSettlement, WorldConfigManager.Instance.maxCitizenCountIncreasePerSettlement + 1);
    //                }
    //                //next column has a settlement, connect all landmarks to that
    //                connections = 1;
    //                for (int j = 0; j < currColumn.rows.Length; j++) {
    //                    TileRow currRow = currColumn.rows[j];
    //                    if (currRow.hasLandmark) {
    //                        BaseLandmark currLandmark = currRow.landmark;
    //                        //create connections
    //                        for (int k = 0; k < connections; k++) {
    //                            //connect to that settlement
    //                            ConnectLandmarks(currRow.landmark, rowInNextColumnWithMajorLandmark.landmark);
    //                        }
    //                    }
    //                }
    //            } else {
    //                //create connections per Landmark in the current column
    //                for (int j = 0; j < currColumn.rows.Length; j++) {
    //                    TileRow currRow = currColumn.rows[j];
    //                    if (currRow.hasLandmark) {
    //                        BaseLandmark currLandmark = currRow.landmark;
    //                        connections = 1; //75% 1 connection
    //                        if (Random.Range(0, 100) < 25) { //25% 2 connections
    //                            connections = 2;
    //                        }
    //                        //create connections
    //                        for (int k = 0; k < connections; k++) {
    //                            List<TileRow> rowChoices = currColumn.GetValidRowsInNextColumnForLandmarks(currRow, nextColumn)
    //                                .Where(x => x.landmark == null || !x.landmark.IsConnectedWith(currLandmark)).ToList();
    //                            if (rowChoices.Count == 0) {
    //                                break;
    //                            }
    //                            TileRow chosenRow = rowChoices[Random.Range(0, rowChoices.Count)];
    //                            BaseLandmark landmark;
    //                            if (chosenRow.hasLandmark) {
    //                                landmark = chosenRow.landmark;
    //                                //connect to that landmark
    //                            } else {
    //                                //create landmark on that row, and connect to it.
    //                                landmark = CreateMinorLandmarkOnRow(chosenRow, yieldTypeChances);
    //                            }
    //                            ConnectLandmarks(currLandmark, landmark);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        //for (int j = 0; j < currColumn.rows.Length; j++) {
    //        //    if (currColumn.rows[j] != null && currColumn.rows[j].landmark != null) {
    //        //        BaseLandmark rowLandmark = currColumn.rows[j].landmark;
    //        //        rowLandmark.SetSameColumnLandmarks(currColumn.GetAllLandmarksInColumn(rowLandmark));
    //        //        rowLandmark.SetSameRowTiles(currColumn.rows[j].GetAllTiles(GridMap.Instance.map).ToList());
    //        //    }
    //        //}
    //    }
    //}
    private Region[] GetCornerRegions() {
        //Get the regions in the 4 corners of the map. NOTE: it is possible that there are multiple corners that belong to the same region.
        HexTile topLeftTile = GridMap.Instance.map[0, GridMap.Instance.height - 1]; //0
        HexTile topRightTile = GridMap.Instance.map[GridMap.Instance.width - 1, GridMap.Instance.height - 1]; //1
        HexTile botRightTile = GridMap.Instance.map[GridMap.Instance.width - 1, 0]; //2
        HexTile botLeftTile = GridMap.Instance.map[0, 0]; //3

        return new Region[] { topLeftTile.region, topRightTile.region, botRightTile.region, botLeftTile.region };
    }
    private int GetOppositeCorner(int corner) {
        if (corner == 0) {
            return 2;
        } else if (corner == 1) {
            return 3;
        } else if (corner == 2) {
            return 0;
        } else {
            return 1;

        }
    }
    /// <summary>
    /// Add a connection between 2 landmarks.
    /// </summary>
    /// <param name="region1">The landmark that will connect to landmark 2.</param>
    /// <param name="region2">The landmark accepting the connection from landmark 1.</param>
    public void ConnectRegions(Region region1, Region region2) {
        region1.AddConnection(region2);
        //landmark1.AddOutGoingConnection(landmark2);
        region2.AddConnection(region1);
        GameObject lineGO = GameObject.Instantiate(landmarkConnectionPrefab, region1.coreTile.transform);
        LineRenderer line = lineGO.GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, region1.coreTile.transform.position);
        line.SetPosition(1, region2.coreTile.transform.position);
    }
    public void ConnectRegions(Region region1, Region region2, ref List<Island> islands) {
        Island landmark1Island = GetIslandOfRegion(region1, islands);
        Island landmark2Island = GetIslandOfRegion(region2, islands);
        ConnectRegions(region1, region2);
        if (landmark1Island != landmark2Island) {
            MergeIslands(landmark1Island, landmark2Island, ref islands);
        }
    }
    public Island GetIslandOfRegion(Region region, List<Island> islands) {
        for (int i = 0; i < islands.Count; i++) {
            Island currIsland = islands[i];
            if (currIsland.regionsInIsland.Contains(region)) {
                return currIsland;
            }
        }
        return null;
    }
    private void MergeIslands(Island island1, Island island2, ref List<Island> islands) {
        island1.regionsInIsland.AddRange(island2.regionsInIsland);
        islands.Remove(island2);
    }
    //private BaseLandmark CreateMinorLandmarkOnRow(TileRow row, WeightedDictionary<LANDMARK_YIELD_TYPE> yieldTypeChances) {
    //    LANDMARK_YIELD_TYPE chosenYieldType = yieldTypeChances.PickRandomElementGivenWeights();
    //    List<HexTile> tiles = row.GetElligibleTilesForLandmark(GridMap.Instance.map);
    //    HexTile chosenTile = tiles[Random.Range(0, tiles.Count)];

    //    List<LANDMARK_TYPE> landmarkChoices = WorldConfigManager.Instance.GetPossibleLandmarks(chosenTile.biomeType, chosenYieldType);
    //    LANDMARK_TYPE chosenType = landmarkChoices[Random.Range(0, landmarkChoices.Count)];

    //    BaseLandmark landmark = CreateNewLandmarkOnTile(chosenTile, chosenType);
    //    row.SetLandmark(landmark);
    //    return landmark;
    //}
    //private BaseLandmark CreateMajorLandmarkOnColumn(TileColumn currColumn, LANDMARK_TYPE type, AREA_TYPE areaType, int citizenCount) {
    //    int randomRow = Random.Range(0, currColumn.rows.Length);
    //    TileRow chosenRow = currColumn.rows[randomRow];
    //    List<HexTile> choices = chosenRow.GetElligibleTilesForLandmark(GridMap.Instance.map);
    //    HexTile chosenTile = choices[Random.Range(0, choices.Count)];

    //    Area newArea = CreateNewArea(chosenTile, areaType, citizenCount);
    //    BaseLandmark landmark = CreateNewLandmarkOnTile(chosenTile, type);
    //    chosenRow.SetLandmark(landmark);
    //    if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
    //        newArea.SetName("Portal"); //need this so that when player is initialized. This area will be assigned to the player.
    //    } else {
    //        Faction faction = FactionManager.Instance.CreateNewFaction();
    //        if (areaType == AREA_TYPE.ELVEN_SETTLEMENT) {
    //            faction.SetInitialFactionLeaderClass("Queen");
    //            faction.SetInitialFactionLeaderGender(GENDER.FEMALE);
    //            faction.SetRace(RACE.ELVES);
    //        } else if (areaType == AREA_TYPE.HUMAN_SETTLEMENT) {
    //            faction.SetInitialFactionLeaderClass("King");
    //            faction.SetInitialFactionLeaderGender(GENDER.MALE);
    //            faction.SetRace(RACE.HUMANS);
    //        }
    //        OwnArea(faction, faction.race, newArea);
    //        newArea.GenerateStructures(citizenCount);
    //        faction.GenerateStartingCitizens(2, 1, citizenCount); //9,7
    //    }
    //    return landmark;
    //}
    public void GenerateConnections(BaseLandmark portal, BaseLandmark settlement) {
        List<Region> pendingConnections = new List<Region>();

        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        List<Island> islands = new List<Island>();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark landmark = allLandmarks[i];
            Island island = new Island(landmark.tileLocation.region);
            islands.Add(island);
        }

        //connect portal to all adjacent regions
        List<Region> portalAdjacent = portal.tileLocation.region.AdjacentRegions();
        for (int i = 0; i < portalAdjacent.Count; i++) {
            Region currRegion = portalAdjacent[i];
            ConnectRegions(portal.tileLocation.region, currRegion, ref islands);
            pendingConnections.Add(currRegion);
            if (portal.tileLocation.region.HasMaximumConnections()) { break; }
        }

        //connect settlement to all adjacent regions
        List<Region> settlementAdjacent = settlement.tileLocation.region.AdjacentRegions();
        for (int i = 0; i < settlementAdjacent.Count; i++) {
            Region currRegion = settlementAdjacent[i];
            ConnectRegions(settlement.tileLocation.region, currRegion, ref islands);
            pendingConnections.Add(currRegion);
            if (settlement.tileLocation.region.HasMaximumConnections()) { break; }
        }

        WeightedDictionary<int> connectionWeights = new WeightedDictionary<int>();
        connectionWeights.AddElement(3, 75); //3 connections - 75%
        connectionWeights.AddElement(2, 25); //2 connections - 25%

        while (pendingConnections.Count > 0) {
            Region currRegion = pendingConnections[0];
            if (!currRegion.HasMaximumConnections()) {
                //current landmark can still have connections
                int connectionsToCreate = Mathf.Max(0, connectionWeights.PickRandomElementGivenWeights() - currRegion.connections.Count);
                List<Region> availableAdjacent = currRegion.AdjacentRegions().Where(x => !x.IsConnectedWith(currRegion) && !x.HasMaximumConnections()).ToList();
                if (availableAdjacent.Count == 0 && currRegion.connections.Count == 0) {
                    //there are no available adjacent connections and this landmark has no connections yet, allow it to connect to any of its adjacent regions.
                    availableAdjacent = currRegion.AdjacentRegions();
                }
                for (int i = 0; i < connectionsToCreate; i++) {
                    if (availableAdjacent.Count > 0) {
                        Region chosenRegion = availableAdjacent[Random.Range(0, availableAdjacent.Count)];
                        ConnectRegions(currRegion, chosenRegion, ref islands);
                        if (!chosenRegion.HasMaximumConnections() && !pendingConnections.Contains(chosenRegion)) {
                            pendingConnections.Add(chosenRegion);
                        }
                        availableAdjacent.Remove(chosenRegion);
                    } else {
                        break; //no more adjacent unconnected regions
                    }
                }
                pendingConnections.Remove(currRegion);
            } else {
                //current landmark can have no more connections
                pendingConnections.Remove(currRegion);
            }
            if (pendingConnections.Count == 0) {
                List<Region> noConnectionRegions = GridMap.Instance.allRegions.Where(x => x.connections.Count == 0).ToList();
                if (noConnectionRegions.Count > 0) {
                    pendingConnections.Add(noConnectionRegions[Random.Range(0, noConnectionRegions.Count)]);
                }
            }
        }

        if (islands.Count > 1) {
            //Connect islands
            while (islands.Count > 1) {
                for (int i = 0; i < islands.Count; i++) {
                    Island currIsland = islands[i];
                    for (int j = 0; j < islands.Count; j++) {
                        Island otherIsland = islands[j];
                        if (currIsland != otherIsland) {
                            Region regionToConnectTo;
                            Region regionThatWillConnect;
                            if (currIsland.TryGetLandmarkThatCanConnectToOtherIsland(otherIsland, islands, out regionToConnectTo, out regionThatWillConnect)) {
                                ConnectRegions(regionThatWillConnect, regionToConnectTo, ref islands);
                                if (islands.Count == 1) {
                                    break;
                                }
                            }
                        }
                    }
                    if (islands.Count == 1) {
                        break;
                    }
                }
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
    public List<BaseLandmark> GetAllLandmarksExcept(params BaseLandmark[] landmarks) {
        List<BaseLandmark> exceptions = landmarks.ToList();
        List<BaseLandmark> allLandmarks = new List<BaseLandmark>();
#if WORLD_CREATION_TOOL
        List<HexTile> choices = worldcreator.WorldCreatorManager.Instance.allTiles;
#else
        List<HexTile> choices = GridMap.Instance.hexTiles;
#endif
        for (int i = 0; i < choices.Count; i++) {
            HexTile currTile = choices[i];
            if (currTile.landmarkOnTile != null && !exceptions.Contains(currTile.landmarkOnTile)) {
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
                            //OwnRegion(owner, owner.race, newArea);
                        }
                        else {
                            Faction neutralFaction = FactionManager.Instance.neutralFaction;
                            if (neutralFaction != null) {
                                //neutralFaction.AddToOwnedRegions(newArea); //this will add area to the neutral factions owned area list, but the area's owner will still be null
                            }
                        }
#endif
                    }
                }
#if !WORLD_CREATION_TOOL
                else {
                    Faction neutralFaction = FactionManager.Instance.neutralFaction;
                    if (neutralFaction != null) {
                        //neutralFaction.AddToOwnedRegions(newArea); //this will add area to the neutral factions owned area list, but the area's owner will still be null
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
    public Area CreateNewArea(Region region, AREA_TYPE areaType, int citizenCount, List<HexTile> tiles = null) {
        Area newArea = new Area(region, areaType, citizenCount);
        region.SetArea(newArea);
        //if (tiles == null) {
        //    newArea.AddTile(coreTile);
        //} else {
        //    newArea.AddTile(tiles);
        //}
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

        //if(newArea.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
        //    for (int i = 0; i < saveDataArea.tileIDs.Count; i++) {
        //        HexTile tile = GridMap.Instance.hexTiles[saveDataArea.tileIDs[i]];
        //        newArea.AddTile(tile);
        //        tile.SetCorruption(true);
        //    }
        //} else {
        //    for (int i = 0; i < saveDataArea.tileIDs.Count; i++) {
        //        newArea.AddTile(GridMap.Instance.hexTiles[saveDataArea.tileIDs[i]]);
        //    }
        //}

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
    public void GenerateAreaMap(Area area, bool doMultiThread = true) {
        GameObject areaMapGO = GameObject.Instantiate(innerStructurePrefab, areaMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();
        InteriorMapManager.Instance.CleanupForTownGeneration();
        if (doMultiThread) {
            MultiThreadPool.Instance.AddToThreadPool(new AreaMapGenerationThread(area, areaMap));
        } else {
            OnFinishedGeneratingAreaMap(area, areaMap);
        }
    }
    public void LoadAreaMap(SaveDataAreaInnerTileMap data) {
        GameObject areaMapGO = GameObject.Instantiate(innerStructurePrefab, areaMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();
        //InteriorMapManager.Instance.CleanupForTownGeneration();
        data.Load(areaMap);
        //areaMap.GenerateDetails();

        //Load other data
        Area area = areaMap.area;
        //area.SetAreaMap(areaMap);

        areaMap.OnMapGenerationFinished();
        area.OnMapGenerationFinished();
        InteriorMapManager.Instance.OnCreateAreaMap(areaMap);

        area.OnAreaSetAsActive();
        UIManager.Instance.SetInteriorMapLoadingState(false);
    }
    private void OnFinishedGeneratingAreaMap(Area area, AreaInnerTileMap areaMap) {
        //Debug.Log("Finished generating map for " + area.name);
        string log = string.Empty;
        areaMap.Initialize(area);
        TownMapSettings generatedSettings = areaMap.GenerateInnerStructures(out log);
        //Debug.Log(log);
        areaMap.DrawMap(generatedSettings);
        //area.SetAreaMap(areaMap);
        areaMap.GenerateDetails();
        area.PlaceTileObjects();
        //thread.areaMap.RotateTiles();

        areaMap.OnMapGenerationFinished();
        area.OnMapGenerationFinished();
        InteriorMapManager.Instance.OnCreateAreaMap(areaMap);
        CharacterManager.Instance.PlaceInitialCharacters(area);
        //InteriorMapManager.Instance.ShowAreaMap(thread.area);
        area.OnAreaSetAsActive();
        UIManager.Instance.SetInteriorMapLoadingState(false);
    }
    public void OnFinishedGeneratingAreaMap(AreaMapGenerationThread thread) {
        //Debug.Log("Finished generating map for " + thread.area.name);
        //Debug.Log(thread.log);
        thread.areaMap.DrawMap(thread.generatedSettings);
        thread.area.SetAreaMap(thread.areaMap);
        thread.areaMap.GenerateDetails();
        thread.area.PlaceTileObjects();
        //thread.areaMap.RotateTiles();

        thread.areaMap.OnMapGenerationFinished();
        thread.area.OnMapGenerationFinished();
        InteriorMapManager.Instance.OnCreateAreaMap(thread.areaMap);
        CharacterManager.Instance.PlaceInitialCharacters(thread.area);
        //InteriorMapManager.Instance.ShowAreaMap(thread.area);
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
    public void OwnRegion(Faction newOwner, RACE newRace, Region region) {
        if (region.owner != null) {
            UnownRegion(region);
        }
        newOwner.AddToOwnedRegions(region);
        region.SetOwner(newOwner);
        //area.SetRaceType(newRace);
        if(region.area != null) {
            region.area.TintStructuresInArea(newOwner.factionColor);
        }
    }
    public void UnownRegion(Region region) {
        if (region.owner != null) {
            region.owner.RemoveFromOwnedRegions(region);
        }
        region.SetOwner(null);
        //area.SetRaceType(area.defaultRace.race); //Return area to its default race
        if (region.area != null) {
            region.area.TintStructuresInArea(Color.white);
        }
    }
    public void LoadAdditionalAreaData() {
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            currArea.LoadAdditionalData();
        }
    }
    public Vector2 GetNameplatePosition(HexTile tile) {
        //switch (area.name) {
        //    case "Cardell":
        //        return new Vector2(-1.1f, 4.6f);
        //    case "Narris":
        //        return new Vector2(20.5f, -3f);
        //    default:
        //        break;
        //}
        Vector2 defaultPos = tile.transform.position;
        defaultPos.y -= 1.25f;
        return defaultPos;
    }
    public void SetEnemyPlayerArea(Area area) {
        enemyOfPlayerArea = area;
    }
    #endregion

    #region Burning Source
    public BurningSource GetBurningSourceByID(int id) {
        for (int i = 0; i < allAreas.Count; i++) {
            Area currArea = allAreas[i];
            if (currArea.areaMap != null) {
                for (int j = 0; j < currArea.areaMap.activeBurningSources.Count; j++) {
                    BurningSource source = currArea.areaMap.activeBurningSources[j];
                    if (source.id == id) {
                        return source;
                    }
                }
            }
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
    public LocationStructure LoadStructureAt(Area location, SaveDataLocationStructure data) {
        LocationStructure createdStructure = data.Load(location);
        if (createdStructure != null) {
            location.AddStructure(createdStructure);
        }
        return createdStructure;
    }
    #endregion

    #region Regions
    //public void GenerateRegionFeatures() {
    //    for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
    //        Region region = GridMap.Instance.allRegions[i];
    //        region.mainLandmark.AddFeaturesToRegion();
    //    }
    //}
    public RegionFeature CreateRegionFeature(string featureName) {
        try {
            return System.Activator.CreateInstance(System.Type.GetType(featureName)) as RegionFeature;
        } catch {
            throw new System.Exception("Cannot create region feature with name " + featureName);
        }
        
    }
    /// <summary>
    /// Get non corrupted regions if any. 
    /// Produces a list of uncorrupted regions, if there are none this returns as false
    /// </summary>
    /// <param name="regions">List of regions to output</param>
    /// <returns>If there are any uncorrupted regions.</returns>
    public bool TryGetUncorruptedRegionsExcept(out List<Region> regions, params Region[] exceptions) {
        regions = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            if (!region.coreTile.isCorrupted && !exceptions.Contains(region)) {
                regions.Add(region);
            }
        }
        return regions.Count > 0;
    }
    #endregion
}

public class Island {

    public List<Region> regionsInIsland;

    public Island(Region region) {
        regionsInIsland = new List<Region>();
        regionsInIsland.Add(region);
    }

    public bool TryGetLandmarkThatCanConnectToOtherIsland(Island otherIsland, List<Island> allIslands, out Region regionToConnectTo, out Region regionThatWillConnect) {
        for (int i = 0; i < regionsInIsland.Count; i++) {
            Region currRegion = regionsInIsland[i];
            List<Region> adjacent = currRegion.AdjacentRegions().Where(x => LandmarkManager.Instance.GetIslandOfRegion(x, allIslands) != this).ToList(); //get all adjacent regions, that does not belong to this island.
            if (adjacent.Count > 0) {
                regionToConnectTo = adjacent[Random.Range(0, adjacent.Count)];
                regionThatWillConnect = currRegion;
                return true;

            }
        }
        regionToConnectTo = null;
        regionThatWillConnect = null;
        return false;
    }
}