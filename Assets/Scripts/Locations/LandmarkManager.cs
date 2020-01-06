using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using JetBrains.Annotations;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;
using Debug = System.Diagnostics.Debug;

public partial class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;
    public static readonly int Max_Connections = 3;
    public const int DELAY_DIVINE_INTERVENTION_DURATION = 144;
    public const int SUMMON_MINION_DURATION = 96;
    public const int MAX_RESOURCE_PILE = 500;

    public int initialLandmarkCount;

    [SerializeField] private List<LandmarkData> landmarkData;
    public List<AreaData> areaData;

    public int corruptedLandmarksCount;

    public List<BaseLandmark> allLandmarks;
    public List<Area> allAreas;
    public List<Area> allNonPlayerAreas;

    [SerializeField] private GameObject landmarkGO;

    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;

    public AreaTypeSpriteDictionary locationPortraits;
    [Header("Connections")]
    [SerializeField] private GameObject landmarkConnectionPrefab;

    public List<LocationEvent> locationEventsData { get; private set; }

    public STRUCTURE_TYPE[] humanSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] humanUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] humanCombatStructures { get; private set; }
    public STRUCTURE_TYPE[] elfSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] elfUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] elfCombatStructures { get; private set; }

    //The Anvil
    public Dictionary<string, AnvilResearchData> anvilResearchData;

    public void Initialize() {
        corruptedLandmarksCount = 0;
        allAreas = new List<Area>();
        allNonPlayerAreas = new List<Area>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
        ConstructAnvilResearchData();
        ConstructLocationEventsData();
        ConstructRaceStructureRequirements();
    }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    #endregion

    #region Landmarks
    private void ConstructLocationEventsData() {
        locationEventsData = new List<LocationEvent>() {
            new NewResidentEvent(),
        };
    }
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
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType, bool addFeatures) {
        if (location.landmarkOnTile != null) {
            //Destroy landmark on tile
            DestroyLandmarkOnTile(location);
        }
        BaseLandmark newLandmark = location.CreateLandmarkOfType(landmarkType, addFeatures);
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        location.UpdateBuildSprites();
        Messenger.Broadcast(Signals.LANDMARK_CREATED, newLandmark);
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        landmarkOnTile.DestroyLandmark();
        tile.UpdateBuildSprites();
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
        Messenger.Broadcast(Signals.LANDMARK_DESTROYED, landmarkOnTile, tile);
        if (landmarkOnTile.specificLandmarkType.IsPlayerLandmark() && tile.region.locationType.IsSettlementType() == false) {
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, 
                tile.region.regionTileObject as IPointOfInterest, 
                "target has been destroyed", JOB_TYPE.ATTACK_DEMONIC_REGION);    
        }
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
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
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
    public IEnumerator GenerateLandmarks(Region[] regions, MapGenerationData data) {
        //place portal first
        Region[] corners = GetCornerRegions();
        int portalCorner = Random.Range(0, 4);
        Region portalRegion = corners[portalCorner];
        Area portalArea = CreateNewArea(portalRegion, LOCATION_TYPE.DEMONIC_INTRUSION, 0);
        BaseLandmark portalLandmark = CreateNewLandmarkOnTile(portalRegion.coreTile, LANDMARK_TYPE.THE_PORTAL, false);
        portalArea.region.SetName("Portal"); //need this so that when player is initialized. This area will be assigned to the player.
        data.portal = portalLandmark;
        yield return null;

        //place settlement at opposite corner
        int oppositeCorner = GetOppositeCorner(portalCorner);
        Region settlementRegion = corners[oppositeCorner];
        Area area = CreateSettlementArea(settlementRegion);
        data.settlement = settlementRegion.mainLandmark;
        //SetEnemyPlayerArea(area);
        yield return null;

        List<Region> availableRegions = new List<Region>(regions);
        availableRegions.Remove(portalRegion);
        availableRegions.Remove(settlementRegion);

        //place all other landmarks
        Dictionary<LANDMARK_TYPE, int> landmarks = WorldConfigManager.Instance.GetLandmarksForGeneration(regions.Length - 2); //subtracted 2 because of portal and settlement
        foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in landmarks) {
            for (int i = 0; i < kvp.Value; i++) {
                Region chosenRegion = availableRegions[Random.Range(0, availableRegions.Count)];
                CreateNewLandmarkOnTile(chosenRegion.coreTile, kvp.Key, false); //BaseLandmark landmark = 
                availableRegions.Remove(chosenRegion);
                yield return null;
            }
        }
    }
    public void GenerateRegionFeatures() {
        Region[] regions = GridMap.Instance.allRegions;
        for (int i = 0; i < regions.Length; i++) {
            regions[i].mainLandmark.AddFeaturesToRegion();
        }
    }
    public IEnumerator CreateTwoNewSettlementsAtTheStartOfGame() {
        Region firstRegion = null;
        Region secondRegion = null;

        Region[] allRegions = GridMap.Instance.allRegions;
        while(firstRegion == null) {
            Region potentialRegion = allRegions[UnityEngine.Random.Range(0, allRegions.Length)];
            if (!potentialRegion.coreTile.isCorrupted && potentialRegion.area == null && !potentialRegion.HasSettlementOrCorruptedConnection()) {
                firstRegion = potentialRegion;
            }
            yield return null;
        }
        while (secondRegion == null) {
            Region potentialRegion = allRegions[UnityEngine.Random.Range(0, allRegions.Length)];
            if (!potentialRegion.coreTile.isCorrupted && potentialRegion.area == null && potentialRegion != firstRegion && !potentialRegion.IsConnectedWith(firstRegion) && !potentialRegion.HasSettlementOrCorruptedConnection()) {
                secondRegion = potentialRegion;
            }
            yield return null;
        }

        Area firstArea = CreateSettlementArea(firstRegion);
        Area secondArea = CreateSettlementArea(secondRegion);

        yield return StartCoroutine(GenerateAreaMap(firstArea));
        yield return StartCoroutine(GenerateAreaMap(secondArea));
    }
    private Area CreateSettlementArea(Region settlementRegion) {
        LOCATION_TYPE settlementType = Utilities.RandomSettlementType();
        int citizenCount = Random.Range(WorldConfigManager.Instance.minCitizenCount, WorldConfigManager.Instance.maxCitizenCount + 1);
        Area settlementArea = CreateNewArea(settlementRegion, settlementType, citizenCount);
        BaseLandmark settlementLandmark = CreateNewLandmarkOnTile(settlementRegion.coreTile, LANDMARK_TYPE.VILLAGE, false);
        Faction faction = FactionManager.Instance.CreateNewFaction();
        if (settlementType == LOCATION_TYPE.ELVEN_SETTLEMENT) {
            //faction.SetInitialFactionLeaderClass("Queen");
            faction.SetInitialFactionLeaderGender(GENDER.FEMALE);
            faction.SetRace(RACE.ELVES);
        } else if (settlementType == LOCATION_TYPE.HUMAN_SETTLEMENT) {
            //faction.SetInitialFactionLeaderClass("King");
            faction.SetInitialFactionLeaderGender(GENDER.MALE);
            faction.SetRace(RACE.HUMANS);
        }
        //settlementRegion.AddFactionHere(faction); //This is removed because when a character sets his home it automatically adds his faction to the location
        //OwnRegion(faction, faction.race, settlementRegion);
        int randomizedNumberOfResident = UnityEngine.Random.Range(3, 7);
        List<Character> createdCharacters = faction.GenerateStartingCitizens(2, 1, randomizedNumberOfResident, settlementArea.classManager, settlementRegion);
        settlementArea.GenerateStructures(faction.GetNumberOfDwellingsToHouseCharacters(createdCharacters));

        //assign characters to their respective homes. No one should be homeless
        for (int i = 0; i < createdCharacters.Count; i++) {
            Character currCharacter = createdCharacters[i];
            settlementArea.AssignCharacterToDwellingInArea(currCharacter);
        }
        return settlementArea;
    }
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
        //LineRenderer line = lineGO.GetComponent<LineRenderer>();
        //line.positionCount = 2;
        //line.SetPosition(0, region1.coreTile.transform.position);
        //line.SetPosition(1, region2.coreTile.transform.position);

        lineGO.GetComponent<LandmarkConnection>().DrawLandmarkConnection(region1, region2, 0.35f);
    }
    private void ConnectRegions(Region region1, Region region2, ref List<Island> islands) {
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
    public IEnumerator GenerateConnections(BaseLandmark portal, BaseLandmark settlement) {
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
            yield return null;
        }

        //connect settlement to all adjacent regions
        List<Region> settlementAdjacent = settlement.tileLocation.region.AdjacentRegions();
        for (int i = 0; i < settlementAdjacent.Count; i++) {
            Region currRegion = settlementAdjacent[i];
            ConnectRegions(settlement.tileLocation.region, currRegion, ref islands);
            pendingConnections.Add(currRegion);
            if (settlement.tileLocation.region.HasMaximumConnections()) { break; }
            yield return null;
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
            yield return null;
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
                yield return null;
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
    public List<Character> GetAllDeadCharactersInLocation(ILocation location) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isDead && character.currentRegion.IsSameCoreLocationAs(location) && !(character is Summon)) {
                if(character.marker != null || character.grave != null) { //Only resurrect characters who are in the tombstone or still has a marker in the area
                    characters.Add(character);
                }
            }
        }
        return characters;
    }
    #endregion

    #region Areas
    public AreaData GetAreaData(LOCATION_TYPE locationType) {
        for (int i = 0; i < areaData.Count; i++) {
            AreaData currData = areaData[i];
            if (currData.locationType == locationType) {
                return currData;
            }
        }
        throw new System.Exception("No area data for type " + locationType);
    }
    public Area CreateNewArea(Region region, LOCATION_TYPE locationType, int citizenCount, List<HexTile> tiles = null) {
        Area newArea = new Area(region, locationType, citizenCount);
        region.SetArea(newArea);
        //if (tiles == null) {
        //    newArea.AddTile(coreTile);
        //} else {
        //    newArea.AddTile(tiles);
        //}
        if (locationPortraits.ContainsKey(newArea.locationType)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.locationType]);
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        if(locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerAreas.Add(newArea);
        }
        return newArea;
    }
    public void RemoveArea(Area area) {
        allAreas.Remove(area);
    }
    public Area CreateNewArea(SaveDataArea saveDataArea) {
        Area newArea = new Area(saveDataArea);

        if (locationPortraits.ContainsKey(newArea.locationType)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.locationType]);
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newArea);
        allAreas.Add(newArea);
        if (saveDataArea.locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerAreas.Add(newArea);
        }
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
        Vector2 defaultPos = tile.transform.position;
        defaultPos.y -= 1.25f;
        return defaultPos;
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
    public LocationStructure CreateNewStructureAt(ILocation location, STRUCTURE_TYPE type, bool isInside = true) {
        LocationStructure createdStructure = null;
        switch (type) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location, isInside);
                break;
            default:
                createdStructure = new LocationStructure(type, location, isInside);
                break;
        }
        location.AddStructure(createdStructure);
        return createdStructure;
    }
    public LocationStructure LoadStructureAt(Area location, SaveDataLocationStructure data) {
        LocationStructure createdStructure = data.Load(location);
        if (createdStructure != null) {
            location.AddStructure(createdStructure);
        }
        return createdStructure;
    }
    private void ConstructRaceStructureRequirements() {
        //humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };

        humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.APOTHECARY };
        humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP, STRUCTURE_TYPE.INN };
        humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD, STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.APOTHECARY, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP };
        elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD };
    }
    public STRUCTURE_TYPE[] GetRaceStructureRequirements(RACE race, string category) {
        if(race == RACE.HUMANS) {
            if (category == "Survival") { return humanSurvivalStructures; }
            else if (category == "Utility") { return humanUtilityStructures; }
            else if (category == "Combat") { return humanCombatStructures; }
        } else if (race == RACE.ELVES) {
            if (category == "Survival") { return elfSurvivalStructures; }
            else if (category == "Utility") { return elfUtilityStructures; }
            else if (category == "Combat") { return elfCombatStructures; }
        }
        return null;
    }
    #endregion

    #region Regions
    public RegionFeature CreateRegionFeature([NotNull] string featureName) {
        try {
            Debug.Assert(featureName != null, nameof(featureName) + " != null");
            return System.Activator.CreateInstance(System.Type.GetType(featureName)) as RegionFeature;
        } catch {
            throw new System.Exception("Cannot create region feature with name " + featureName);
        }
        
    }
    public Region GetRandomRegionWithFeature(string feature) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            if (region.HasFeature(feature)) {
                choices.Add(region);
            }
        }
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);
        }
        return null;
    }
    #endregion

    #region The Anvil
    private void ConstructAnvilResearchData() {
        anvilResearchData = new Dictionary<string, AnvilResearchData>() {
            { TheAnvil.Improved_Spells_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Spell level to 2.",
                    manaCost = 150,
                    durationInHours = 8,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Spell Level increased!",
                }
            },
            { TheAnvil.Improved_Spells_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Spell level to 3.",
                    manaCost = 300,
                    durationInHours = 24,
                    preRequisiteResearch = TheAnvil.Improved_Spells_1,
                    researchDoneNotifText = "Spell Level increased!",
                }
            },
            { TheAnvil.Improved_Artifacts_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Artifact level to 2.",
                    manaCost = 100,
                    durationInHours = 4,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Artifact Level increased!",
                }
            },
            { TheAnvil.Improved_Artifacts_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Artifact level to 3.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = TheAnvil.Improved_Artifacts_1,
                    researchDoneNotifText = "Artifact Level increased!",
                }
            },
            { TheAnvil.Improved_Summoning_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Summon level to 2.",
                    manaCost = 100,
                    durationInHours = 4,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Summon Level increased!",
                }
            },
            { TheAnvil.Improved_Summoning_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Summon level to 3.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = TheAnvil.Improved_Summoning_1,
                    researchDoneNotifText = "Summon Level increased!",
                }
            },
            { TheAnvil.Faster_Invasion,
                new AnvilResearchData() {
                    effect = 20,
                    description = "Invasion is 20% faster.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Invasion rate increased!",
                }
            },
            { TheAnvil.Improved_Construction,
                new AnvilResearchData() {
                    effect = 20,
                    description = "Construction is 20% faster.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Construction rate increased!",
                }
            },
            { TheAnvil.Increased_Mana_Capacity,
                new AnvilResearchData() {
                    effect = 600,
                    description = "Maximum Mana increased to 600.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Maximum mana increased!",
                }
            },
            { TheAnvil.Increased_Mana_Regen,
                new AnvilResearchData() {
                    effect = 5,
                    description = "Mana Regen increased by 5.",
                    manaCost = 200,
                    durationInHours = 24,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Mana regeneration rate increased!",
                }
            },
        };
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