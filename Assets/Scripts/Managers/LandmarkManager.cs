using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

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
    public StringSpriteDictionary locationPortraits; //NOTE: Move this to world creation when time permits.

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
        if (saveData.items != null) {
            for (int i = 0; i < saveData.items.Count; i++) {
                string currItemName = saveData.items[i];
                newLandmark.AddItem(ItemManager.Instance.CreateNewItemInstance(currItemName));
            }
        }
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
                            OwnArea(owner, owner.raceType, newArea);
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
        if (locationPortraits.ContainsKey(newArea.name)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.name]);
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
        if (locationPortraits.ContainsKey(newArea.name)) {
            newArea.SetLocationPortrait(locationPortraits[newArea.name]);
        }
#if !WORLD_CREATION_TOOL
        GameObject areaMapGO = GameObject.Instantiate(innerStructurePrefab, areaMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.Initialize(newArea);
        newArea.SetAreaMap(areaMap);
        newArea.PlaceTileObjects();
        areaMap.GenerateDetails();
        areaMap.RotateTiles();
        InteriorMapManager.Instance.OnCreateAreaMap(areaMap);
#endif
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
}
