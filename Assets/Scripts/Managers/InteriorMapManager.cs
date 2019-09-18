using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;

public class InteriorMapManager : MonoBehaviour {

    public static InteriorMapManager Instance = null;
    public AreaInnerTileMap currentlyShowingMap { get; private set; }
    public Area currentlyShowingArea { get; private set; }
    public GameObject poiCollisionTriggerPrefab;
    public GameObject ghostCollisionTriggerPrefab;
    public GameObject characterCollisionTriggerPrefab;

    private List<AreaInnerTileMap> areaMaps;
    private Vector3 nextMapPos = Vector3.zero;
    public bool isAnAreaMapShowing {
        get {
            return currentlyShowingMap != null;
        }
    }
    public IPointOfInterest currentlyHoveredPOI {
        get {
            if (isAnAreaMapShowing) {
                if (currentlyShowingMap.hoveredCharacter != null) {
                    return currentlyShowingMap.hoveredCharacter;
                } else if (GetTileFromMousePosition() != null) {
                    LocationGridTile hoveredTile = GetTileFromMousePosition();
                    return hoveredTile.objHere;
                }
            }
            return null;
        }
    }
    public LocationGridTile currentlyHoveredTile { get { return GetTileFromMousePosition(); } }
    public List<LocationGridTile> currentlyHighlightedTiles { get; private set; }

    //Used for generating the inner map of an area, structure templates are first placed here before generating the actual map
    public Tilemap agGroundTilemap;
    public Tilemap agGroundWallTilemap;
    public Tilemap agStructureTilemap;
    public Tilemap agObjectsTilemap;
    public Tilemap agDetailsTilemap;
    
    [Header("Pathfinding")]
    [SerializeField] private AstarPath pathfinder;
    private const float nodeSize = 0.2f;
    public const int Default_Character_Sorting_Order = 82;

    [Header("Tile Object")]
    [SerializeField] private TileObjectSlotDictionary tileObjectSlotSettings;
    public GameObject tileObjectSlotsParentPrefab;
    public GameObject tileObjectSlotPrefab;
    public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }

    [Header("Lighting")]
    [SerializeField] private Light areaMapLight;

    //structure templates
    private string templatePath;

    //Local Avoidance
    private Pathfinding.RVO.Simulator sim;
    public List<TileBase> allTileAssets;

    private Dictionary<STRUCTURE_TYPE, List<StructureSlot>> placedStructures;


    private Dictionary<int, float> lightSettings = new Dictionary<int, float>() { //this specifies what light intensity is to be used while inside the specific range in ticks
        { 228, 1f },
        { 61, 1.8f }
    };

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
    }
    public void LateUpdate() {
        if (UIManager.Instance.IsMouseOnUI() || currentlyShowingMap == null) {
            if (UIManager.Instance.IsSmallInfoShowing() && UIManager.Instance.smallInfoShownFrom == "ShowTileData") {
                UIManager.Instance.HideSmallInfo();
            }
            return;
        }
        LocationGridTile hoveredTile = GetTileFromMousePosition();
        if (hoveredTile != null) {
            //CursorManager.Instance.SetSparkleEffectState(hoveredTile.objHere != null);
            if (GameManager.showAllTilesTooltip) {
                ShowTileData(hoveredTile);
                if (hoveredTile.objHere != null) {
                    if (Input.GetMouseButtonDown(0)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
                    } else if (Input.GetMouseButtonDown(1)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
                    }
                } else {
                    if (Input.GetMouseButtonDown(0)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
                    } else if (Input.GetMouseButtonDown(1)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
                    }
                }
            } else {
                if (hoveredTile.objHere != null || hoveredTile.parentAreaMap.hoveredCharacter != null) {
                    ShowTileData(hoveredTile);
                    if (Input.GetMouseButtonDown(0)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
                    } else if (Input.GetMouseButtonDown(1)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
                    }
                } else {
                    if (Input.GetMouseButtonDown(0)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
                    } else if (Input.GetMouseButtonDown(1)) {
                        hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
                    }
                    UIManager.Instance.HideSmallInfo();

                }
            }

        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
    #endregion

    #region Main
    public void Initialize() {
        allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
        areaMaps = new List<AreaInnerTileMap>();
        AreaMapCameraMove.Instance.Initialize();
        Messenger.AddListener(Signals.TICK_ENDED, CheckForChangeLight);
    }
    /// <summary>
    /// Try and show the area map of an area. If it does not have one, this will generate one instead.
    /// </summary>
    /// <param name="area"></param>
    public void TryShowAreaMap(Area area) {
        if (area.areaMap != null) {
            //show existing map
            ShowAreaMap(area);
        } else {
            //Generate
            UIManager.Instance.SetInteriorMapLoadingState(true);
            LandmarkManager.Instance.GenerateAreaMap(area);
        }
    }
    public void ShowAreaMap(Area area, bool centerCameraOnMapCenter = true, bool instantCenter = true) {
        if (area.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            UIManager.Instance.portalPopup.SetActive(true);
            return;
        }
        area.areaMap.Open();
        currentlyShowingMap = area.areaMap;
        currentlyShowingArea = area;
        //GameManager.Instance.SetTicksToAddPerTick(1); //When area map is shown, ticks will progress normally
        Messenger.Broadcast(Signals.AREA_MAP_OPENED, area);

        if (centerCameraOnMapCenter) {
            AreaMapCameraMove.Instance.JustCenterCamera(instantCenter);
        }
    }
    public Area HideAreaMap() {
        if (currentlyShowingMap == null) {
            return null;
        }
        currentlyShowingMap.Close();
        Area closedArea = currentlyShowingArea;
        AreaMapCameraMove.Instance.CenterCameraOn(null);
        currentlyShowingMap = null;
        currentlyShowingArea = null;
        //GameManager.Instance.SetTicksToAddPerTick(GameManager.ticksPerHour); //When area map is shown, ticks will progress by 1 hour
        PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
        Messenger.Broadcast(Signals.AREA_MAP_CLOSED, closedArea);
        GameManager.Instance.SetPausedState(true);
        //GameManager.Instance.DayStarted(false);
        //GameManager.Instance.SetTick(96);
        return closedArea;
    }
    public void OnCreateAreaMap(AreaInnerTileMap newMap) {
        areaMaps.Add(newMap);
        newMap.transform.localPosition = nextMapPos;
        ////set the next map position based on the new maps height
        //nextMapPos = new Vector3(nextMapPos.x, nextMapPos.y + newMap.height + 10, nextMapPos.z); //all maps now have same positon, because only 1 can exist at a time.
        CreatePathfindingGraphForArea(newMap);
        newMap.UpdateTilesWorldPosition();
    }
    public void DestroyAreaMap(Area area) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                keyValuePair.Value[i].DoCleanup();
            }
        }
        pathfinder.data.RemoveGraph(area.areaMap.pathfindingGraph);
        area.areaMap.CleanUp();
        areaMaps.Remove(area.areaMap);
        GameObject.Destroy(area.areaMap.gameObject);
        area.SetAreaMap(null);
    }
    #endregion

    #region Utilities
    public LocationGridTile GetTileFromMousePosition() {
        Vector3 mouseWorldPos = (currentlyShowingMap.worldUICanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 localPos = currentlyShowingMap.grid.WorldToLocal(mouseWorldPos);
        Vector3Int coordinate = currentlyShowingMap.grid.LocalToCell(localPos);
        if (coordinate.x >= 0 && coordinate.x < currentlyShowingMap.width
            && coordinate.y >= 0 && coordinate.y < currentlyShowingMap.height) {
            return currentlyShowingMap.map[coordinate.x, coordinate.y];
        }
        return null;
    }
    #endregion

    #region Pathfinding
    private void CreatePathfindingGraphForArea(AreaInnerTileMap newMap) {
        GridGraph gg = pathfinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gg.cutCorners = false;
        gg.rotation = new Vector3(-90f, 0f, 0f);
        gg.nodeSize = nodeSize;

        int reducedWidth = newMap.width - (AreaInnerTileMap.westEdge + AreaInnerTileMap.eastEdge);
        int reducedHeight = newMap.height - (AreaInnerTileMap.northEdge + AreaInnerTileMap.southEdge);

        gg.SetDimensions(Mathf.FloorToInt((float)reducedWidth / gg.nodeSize), Mathf.FloorToInt((float)reducedHeight / gg.nodeSize), nodeSize);
        Vector3 pos = this.transform.position;
        pos.x += ((float)newMap.width / 2f);
        pos.y += ((float)newMap.height / 2f) + newMap.transform.localPosition.y;
        pos.x += (AreaInnerTileMap.westEdge / 2) - 0.5f;

        gg.center = pos;
        gg.collision.use2D = true;
        gg.collision.type = ColliderType.Sphere;
        if (newMap.area.areaType == AREA_TYPE.DUNGEON) {
            gg.collision.diameter = 2f;
        } else {
            gg.collision.diameter = 1f;
        }
        gg.collision.mask = LayerMask.GetMask("Unpassable");
        AstarPath.active.Scan(gg);
        newMap.pathfindingGraph = gg;
    }
    #endregion

    #region UI
    public bool IsMouseOnMarker() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.tag == "Character Marker") {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }

            }
        }
        return false;
    }
    //public void SetCurrentlyHoveredTile(LocationGridTile tile) {
    //   currentlyHoveredTile = tile;
    //}
    public void HighlightTiles(List<LocationGridTile> tiles) {
        if (tiles != null) {
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].HighlightTile();
            }
        }
        currentlyHighlightedTiles = tiles;
    }
    public void UnhighlightTiles() {
        if (currentlyHighlightedTiles != null) {
            for (int i = 0; i < currentlyHighlightedTiles.Count; i++) {
                currentlyHighlightedTiles[i].UnhighlightTile();
            }
        }
        currentlyHighlightedTiles = null;
    }
    public void UnhighlightTiles(List<LocationGridTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].UnhighlightTile();
        }
    }
    #endregion

    #region Structure Templates
    public List<StructureTemplate> GetStructureTemplates(STRUCTURE_TYPE structure) {
        List<StructureTemplate> templates = new List<StructureTemplate>();
        string path = templatePath + structure.ToString() + "/";
        if (Directory.Exists(path)) {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                if (currInfo.Extension.Equals(".json")) {
                    string dataAsJson = File.ReadAllText(currInfo.FullName);
                    StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                    loaded.name = currInfo.Name;
                    templates.Add(loaded);
                }
            }
        }
        return templates;
    }
    public List<StructureTemplate> GetStructureTemplates(string folderName, List<string> except = null) {
        List<StructureTemplate> templates = new List<StructureTemplate>();
        string path = templatePath + folderName + "/";
        if (Directory.Exists(path)) {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                if (currInfo.Extension.Equals(".json")) {
                    string dataAsJson = File.ReadAllText(currInfo.FullName);
                    StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                    loaded.name = currInfo.Name;
                    if (except != null && except.Contains(loaded.name)) {
                        continue; //skip
                    }
                    //Debug.Log(loaded.name);
#if TRAILER_BUILD
                    if (folderName == "TOWN CENTER/" && loaded.name != "TC_Template_3.json") {
                        continue; //only use Template 3 on Trailer Build
                    }
#endif
                    templates.Add(loaded);
                }
            }
        }
        return templates;
    }
    /// <summary>
    /// Get Tile asset based on name. NOTE: Should only be used on the start of the game when building the area maps.
    /// </summary>
    /// <param name="name">Name of the asset</param>
    public TileBase GetTileAsset(string name, bool logMissing = false) {
        //List<TileBase> allTileAssets = LoadAllTilesAssets();
        for (int i = 0; i < allTileAssets.Count; i++) {
            TileBase currTile = allTileAssets[i];
            if (currTile.name == name) {
                return currTile;
            }
        }
        if (logMissing) {
            Debug.LogWarning("There is no tilemap asset with name " + name);
        }
        return null;
    }
    private List<TileBase> LoadAllTilesAssets() {
        return Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
    }
    [ContextMenu("Load Assets")]
    public void LoadTileAssets() {
        allTileAssets = LoadAllTilesAssets();
    }
    #endregion

    #region For Testing
    private void ShowTileData(LocationGridTile tile) {
        if (tile == null) {
            return;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string summary = tile.localPlace.ToString();
        summary += "\nLocal Location: " + tile.localLocation.ToString();
        summary += "\nWorld Location: " + tile.worldLocation.ToString();
        summary += "\nCentered World Location: " + tile.centeredWorldLocation.ToString();
        summary += "\nGround Type: " + tile.groundType.ToString();
        summary += "\nIs Locked: " + tile.isLocked.ToString();
        summary += "\nIs Occupied: " + tile.isOccupied.ToString();
        summary += "\nIs Edge: " + tile.IsAtEdgeOfWalkableMap();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nTile Access: " + tile.tileAccess.ToString();
        summary += "\nReserved Tile Object Type: " + tile.reservedObjectType.ToString();
        if (tile.hasFurnitureSpot) {
            summary += "\nFurniture Spot: " + tile.furnitureSpot.ToString();
        }
        summary += "\nTile Traits: ";
        if (tile.normalTraits.Count > 0) {
            summary += "\n";
            for (int i = 0; i < tile.normalTraits.Count; i++) {
                summary += "|" + tile.normalTraits[i].name + "|";
            }

        } else {
            summary += "None";
        }

        IPointOfInterest poi = tile.objHere;
        if (poi == null) {
            poi = tile.genericTileObject;
        }
        summary += "\nContent: " + poi?.ToString() ?? "None";
        if (poi != null) {
            summary += "\n\tObject State: " + poi.state.ToString();
            if (poi is TreeObject) {
                summary += "\n\tYield: " + (poi as TreeObject).yield.ToString();
            } else if (poi is Ore) {
                summary += "\n\tYield: " + (poi as Ore).yield.ToString();
            } else if (poi is SupplyPile) {
                summary += "\n\tSupplies in Pile: " + (poi as SupplyPile).suppliesInPile.ToString();
            } else if (poi is FoodPile) {
                summary += "\n\tFood in Pile: " + (poi as FoodPile).foodInPile.ToString();
            } else if (poi is Table) {
                summary += "\n\tFood in Table: " + (poi as Table).food.ToString();
            } else if (poi is SpecialToken) {
                summary += "\n\tCharacter Owner: " + (poi as SpecialToken).characterOwner?.name ?? "None";
                summary += "\n\tFaction Owner: " + (poi as SpecialToken).factionOwner?.name ?? "None";
            }
            summary += "\n\tAdvertised Actions: ";
            if (poi.poiGoapActions.Count > 0) {
                for (int i = 0; i < poi.poiGoapActions.Count; i++) {
                    summary += "|" + poi.poiGoapActions[i].ToString() + "|";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tObject Traits: ";
            if (poi.normalTraits.Count > 0) {
                for (int i = 0; i < poi.normalTraits.Count; i++) {
                    summary += "\n\t\t- " + poi.normalTraits[i].name + " - " + poi.normalTraits[i].GetTestingData();
                }
            } else {
                summary += "None";
            }
            summary += "\n\tJobs Targetting this: ";
            if (poi.allJobsTargettingThis.Count > 0) {
                for (int i = 0; i < poi.allJobsTargettingThis.Count; i++) {
                    summary += "\n\t\t- " + poi.allJobsTargettingThis[i].ToString();
                }
            } else {
                summary += "None";
            }
        }
        if (tile.structure != null) {
            summary += "\nStructure: " + tile.structure.ToString();
            summary += "\nCharacters at " + tile.structure.ToString() + ": ";
            if (tile.structure.charactersHere.Count > 0) {
                for (int i = 0; i < tile.structure.charactersHere.Count; i++) {
                    Character currCharacter = tile.structure.charactersHere[i];
                    if (tile.parentAreaMap.hoveredCharacter == currCharacter) {
                        summary += "\n<b>" + currCharacter.name + "</b>";
                        summary += "\n\t" + GetCharacterHoverData(currCharacter) + "\n";
                    } else {
                        summary += currCharacter.name + ",";
                    }
                }
            } else {
                summary += "None";
            }
            
        } else {
            summary += "\nStructure: None";
        }
        UIManager.Instance.ShowSmallInfo(summary);
#else
         //For build only
        if (tile.objHere != null) {
            UIManager.Instance.ShowSmallInfo(tile.objHere.ToString());
        }
#endif
    }
    private string GetCharacterHoverData(Character character) {
        string summary = "Character: " + character.name;
        summary += "\n\tMood: " + character.currentMoodType.ToString();
        summary += "\n\tSupply: " + character.supply.ToString();
        summary += "\n\tDestination: " + (character.marker.destinationTile != null ? character.marker.destinationTile.ToString() : "None");
        summary += "\n\tMove Speed: " + character.marker.pathfindingAI.speed.ToString();
        summary += "\n\tAttack Range: " + character.characterClass.attackRange.ToString();
        summary += "\n\tAttack Speed: " + character.attackSpeed.ToString();
        summary += "\n\tTarget POI: " + character.marker.targetPOI?.ToString() ?? "None";
        summary += "\n\tBase Structure: " + (character.trapStructure.structure != null ? character.trapStructure.structure.ToString() : "None");
        summary += "\n\tDestination Tile: ";
        if (character.marker.destinationTile == null) {
            summary += "None";
        } else {
            summary += character.marker.destinationTile.ToString() + " at " + character.marker.destinationTile.parentAreaMap.area.name;
        }
        summary += "\n\tArrival Action: " + character.marker.arrivalAction?.Method.Name ?? "None";
        summary += "\n\tPOI's in Vision: ";
        if (character.marker.inVisionPOIs.Count > 0) {
            for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = character.marker.inVisionPOIs[i];
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tCharacters in Vision: ";
        if (character.marker.inVisionCharacters.Count > 0) {
            for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                Character poi = character.marker.inVisionCharacters.ElementAt(i);
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tPOI's in Range but different structures: ";
        if (character.marker.visionCollision.poisInRangeButDiffStructure.Count > 0) {
            for (int i = 0; i < character.marker.visionCollision.poisInRangeButDiffStructure.Count; i++) {
                IPointOfInterest poi = character.marker.visionCollision.poisInRangeButDiffStructure[i];
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tHostiles in Range: ";
        if (character.marker.hostilesInRange.Count > 0) {
            for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                Character poi = character.marker.hostilesInRange[i];
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tAvoid in Range: ";
        if (character.marker.avoidInRange.Count > 0) {
            for (int i = 0; i < character.marker.avoidInRange.Count; i++) {
                IPointOfInterest poi = character.marker.avoidInRange[i];
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tTerrifying Characters: ";
        if (character.marker.terrifyingObjects.Count > 0) {
            for (int i = 0; i < character.marker.terrifyingObjects.Count; i++) {
                IPointOfInterest currObj = character.marker.terrifyingObjects[i];
                summary += currObj.name + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tPlans In Queue: ";
        if (character.allGoapPlans.Count > 0) {
            for (int i = 0; i < character.allGoapPlans.Count; i++) {
                GoapPlan plan = character.allGoapPlans[i];
                summary += plan.endNode.action.goapName + ", ";
            }
        } else {
            summary += "None";
        }
        summary += "\n\tPersonal Job Queue: ";
        if (character.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < character.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = character.jobQueue.jobsInQueue[i];
                summary += poi.name + ", ";
            }
        } else {
            summary += "None";
        }
        return summary;
    }
    #endregion

    #region Town Map Generation
    public void CleanupForTownGeneration() {
        agGroundTilemap.ClearAllTiles();
        agStructureTilemap.ClearAllTiles();
        agObjectsTilemap.ClearAllTiles();
        agDetailsTilemap.ClearAllTiles();
        placedStructures = new Dictionary<STRUCTURE_TYPE, List<StructureSlot>>();
    }
    public TownMapSettings GetTownMapSettings() {
        TownMapSettings s = new TownMapSettings();
        //visuals
        agGroundTilemap.CompressBounds();
        s.size = new Point(agGroundTilemap.cellBounds.x, agGroundTilemap.cellBounds.y);

        s.groundTiles = GetTileData(agGroundTilemap, agGroundTilemap.cellBounds);
        s.groundWallTiles = GetTileData(agGroundWallTilemap, agGroundTilemap.cellBounds);
        s.structureTiles = GetTileData(agStructureTilemap, agGroundTilemap.cellBounds);
        s.objectTiles = GetTileData(agObjectsTilemap, agGroundTilemap.cellBounds);
        s.detailTiles = GetTileData(agDetailsTilemap, agGroundTilemap.cellBounds);

        int shiftXBy = 0; //shift x position of all objects by n
        int shiftYBy = 0;//shift y position of all objects by n
        if (agGroundTilemap.cellBounds.xMin != 0) { shiftXBy = agGroundTilemap.cellBounds.xMin * -1; }
        if (agGroundTilemap.cellBounds.yMin != 0) { shiftYBy = agGroundTilemap.cellBounds.yMin * -1; }

        //structures
        s.structureSlots = placedStructures;

        //shift all positions so that the bounds minimum is at 0
        foreach (KeyValuePair<STRUCTURE_TYPE, List<StructureSlot>> keyValuePair in s.structureSlots) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                keyValuePair.Value[i].AdjustStartPos(shiftXBy, shiftYBy);
            }
        }

       return s;
    }
    public TownMapSettings GetTownMapSettings(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings) {
        int minX;
        int maxX;
        int minY;
        int maxY;
        GetBounds(allSettings, out minX, out maxX, out minY, out maxY);
        TownMapSettings s = new TownMapSettings();
        int shiftXBy = 0; //shift x position of all objects by n
        int shiftYBy = 0;//shift y position of all objects by n
        if (minX != 0) { shiftXBy = minX * -1; }
        if (minY != 0) { shiftYBy = minY * -1; }

        Dictionary<int, Dictionary<int, LocationGridTileSettings>> shiftedSettings = ShiftSettingsBy(new Vector2Int(shiftXBy, shiftYBy), allSettings);
        GetBounds(shiftedSettings, out minX, out maxX, out minY, out maxY);

        ////order settings by their x and y coordinates (Used to determine size (X, Y) of the whole town map)
        //shiftedSettings = shiftedSettings.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        //List<int> keys = shiftedSettings.Keys.ToList();
        //for (int i = 0; i < keys.Count; i++) {
        //    int currKey = keys[i];
        //    shiftedSettings[currKey] = shiftedSettings[currKey].OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        //}
        if (minX < 0 || minY < 0) {
            throw new System.Exception("Minimum bounds of shifted settings has negative value! X: " + minX.ToString() + ", Y: " + minY.ToString());
        }
        s.size = new Point(maxX, maxY);

        List<TileTemplateData> groundTiles = new List<TileTemplateData>();
        List<TileTemplateData> groundWallTiles = new List<TileTemplateData>();
        List<TileTemplateData> structureTiles = new List<TileTemplateData>();
        List<TileTemplateData> objectTiles = new List<TileTemplateData>();
        List<TileTemplateData> detailTiles = new List<TileTemplateData>();

        //s.groundTiles = new TileTemplateData[maxX * maxY];
        //s.structureTiles = new TileTemplateData[maxX * maxY];
        //s.objectTiles = new TileTemplateData[maxX * maxY];
        //s.detailTiles = new TileTemplateData[maxX * maxY];

        int count = 0;
        for (int x = 0; x < maxX; x++) {
            for (int y = 0; y < maxY; y++) {
                if (shiftedSettings.ContainsKey(x) && shiftedSettings[x].ContainsKey(y)) {
                    LocationGridTileSettings currSetting = shiftedSettings[x][y];
                    currSetting.UpdatePositions(new Vector3(x, y, 0f));
                    groundTiles.Add(currSetting.groundTile);
                    groundWallTiles.Add(currSetting.groundWallTile);
                    structureTiles.Add(currSetting.structureWallTile);
                    objectTiles.Add(currSetting.objectTile);
                    detailTiles.Add(currSetting.detailTile);
                } else {
                    TileTemplateData emptyData = TileTemplateData.Empty;
                    emptyData.tilePosition = new Vector3(x, y, 0);
                    groundTiles.Add(emptyData);
                    structureTiles.Add(emptyData);
                    objectTiles.Add(emptyData);
                    detailTiles.Add(emptyData);
                }
                count++;
            }
        }

        if (count != (maxX * maxY)) {
            throw new System.Exception("Total tiles are inconsistent with size! Count is: " + count.ToString() + ". MaxX is: " + maxX.ToString() + ". MaxY is: " + maxY.ToString());
        }

        foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in shiftedSettings) {
            foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                LocationGridTileSettings currSetting = kvp2.Value;
                currSetting.UpdatePositions(new Vector3(kvp.Key, kvp2.Key, 0f));
                groundTiles.Add(currSetting.groundTile);
                groundWallTiles.Add(currSetting.groundWallTile);
                structureTiles.Add(currSetting.structureWallTile);
                objectTiles.Add(currSetting.objectTile);
                detailTiles.Add(currSetting.detailTile);
                count++;
            }
        }

        s.groundTiles = groundTiles.ToArray();
        s.groundWallTiles = groundWallTiles.ToArray();
        s.structureTiles = structureTiles.ToArray();
        s.objectTiles = objectTiles.ToArray();
        s.detailTiles = detailTiles.ToArray();

        //structures
        s.structureSlots = placedStructures;

        //shift all positions so that the bounds minimum is at 0
        foreach (KeyValuePair<STRUCTURE_TYPE, List<StructureSlot>> keyValuePair in s.structureSlots) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                keyValuePair.Value[i].AdjustStartPos(shiftXBy, shiftYBy);
            }
        }
        return s;
    }
    private void GetBounds(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings, out int minX, out int maxX, out int minY, out int maxY) {
        minX = 99999;
        maxX = 0;
        minY = 99999;
        maxY = 0;
        foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in allSettings) {
            if (kvp.Key < minX) { minX = kvp.Key; }
            else if (kvp.Key > maxX) { maxX = kvp.Key; }
            foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                if (kvp2.Key < minY) { minY = kvp2.Key; } 
                else if (kvp2.Key > maxY) { maxY = kvp2.Key; }
            }
        }
        maxX += 1;
        maxY += 1; //because collections start at 0, and I need the max length of the collections.
    }
    public void MergeSettings(Dictionary<int, Dictionary<int, LocationGridTileSettings>> other, ref Dictionary<int, Dictionary<int, LocationGridTileSettings>> main) {
        foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in other) {
            foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                int x = kvp.Key;
                int y = kvp2.Key;
                LocationGridTileSettings newSetting = kvp2.Value;
                if (main.ContainsKey(x) && main[x].ContainsKey(y)) {
                    //merge the 2 settings
                    LocationGridTileSettings mainSetting = main[x][y];
                    mainSetting = mainSetting.MergeWith(newSetting);
                    main[x][y] = mainSetting;
                } else if (main.ContainsKey(x) && !main[x].ContainsKey(y)) {
                    main[x].Add(y, newSetting);
                } else if (!main.ContainsKey(x)) {
                    main.Add(x, new Dictionary<int, LocationGridTileSettings>());
                    main[x].Add(y, newSetting);
                }
            }
        }
    }
    private Dictionary<int, Dictionary<int, LocationGridTileSettings>> ShiftSettingsBy(Vector2Int shiftBy, Dictionary<int, Dictionary<int, LocationGridTileSettings>> settings) {
        Dictionary<int, Dictionary<int, LocationGridTileSettings>> shifted = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
        foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in settings) {
            foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                int shiftedX = kvp.Key + shiftBy.x;
                int shiftedY = kvp2.Key + shiftBy.y;
                LocationGridTileSettings currSetting = kvp2.Value;
                if (!shifted.ContainsKey(shiftedX)) {
                    shifted.Add(shiftedX, new Dictionary<int, LocationGridTileSettings>());
                }
                shifted[shiftedX].Add(shiftedY, currSetting);
            }
        }
        return shifted;
    }
    public Dictionary<int, Dictionary<int, LocationGridTileSettings>> GenerateTownCenterTemplateForGeneration(StructureTemplate template, Vector3Int startPos) {
        Dictionary<int, Dictionary<int, LocationGridTileSettings>> generated = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
        for (int i = 0; i < template.groundTiles.Length; i++) {
            TileTemplateData ground = template.groundTiles[i];
            Vector3 tilePos = ground.tilePosition;
            tilePos.x += startPos.x;
            tilePos.y += startPos.y;
            TileTemplateData detail = template.detailTiles[i];
            TileTemplateData groundWall;
            if (template.groundWallTiles != null) {
                groundWall = template.groundWallTiles[i];
            } else {
                groundWall = TileTemplateData.Empty;
            }
            
            TileTemplateData structureWall = template.structureWallTiles[i];
            TileTemplateData obj = template.objectTiles[i];
            if (!generated.ContainsKey((int)tilePos.x)) {
                generated.Add((int)tilePos.x, new Dictionary<int, LocationGridTileSettings>());
            }
            generated[(int)tilePos.x].Add((int)tilePos.y, new LocationGridTileSettings() {
                groundTile = ground,
                groundWallTile = groundWall,
                detailTile = detail,
                structureWallTile = structureWall,
                objectTile = obj,
            });
        }
        return generated;

    }
    public Dictionary<int, Dictionary<int, LocationGridTileSettings>> GenerateStructureTemplateForGeneration(StructureTemplate template, Vector3Int startPos, STRUCTURE_TYPE structureType, out string log) {
        Dictionary<int, Dictionary<int, LocationGridTileSettings>> generated = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
        for (int i = 0; i < template.groundTiles.Length; i++) {
            TileTemplateData ground = template.groundTiles[i];
            Vector3 tilePos = ground.tilePosition;
            tilePos.x += startPos.x;
            tilePos.y += startPos.y;
            TileTemplateData detail = template.detailTiles[i];
            TileTemplateData groundWall;
            if (template.groundWallTiles != null) {
                groundWall = template.groundWallTiles[i];
            } else {
                groundWall = TileTemplateData.Empty;
            }
            TileTemplateData structureWall = template.structureWallTiles[i];
            TileTemplateData obj = template.objectTiles[i];
            if (!generated.ContainsKey((int)tilePos.x)) {
                generated.Add((int)tilePos.x, new Dictionary<int, LocationGridTileSettings>());
            }
            generated[(int)tilePos.x].Add((int)tilePos.y, new LocationGridTileSettings() {
                groundTile = ground,
                groundWallTile = groundWall,
                detailTile = detail,
                structureWallTile = structureWall,
                objectTile = obj,
            });
        }
        StructureSlot slot = new StructureSlot() { size = template.size, startPos = startPos, furnitureSpots = template.furnitureSpots };
        log = "Placed structure slot with size " + slot.size.ToString() + " at " + startPos.ToString();

        AddPlacedStructure(structureType, slot);
        return generated;

    }
    public void DrawTownCenterTemplateForGeneration(StructureTemplate template, Vector3Int startPos) {
        DrawTiles(agGroundTilemap, template.groundTiles, startPos);
        DrawTiles(agGroundWallTilemap, template.groundWallTiles, startPos);
        DrawTiles(agStructureTilemap, template.structureWallTiles, startPos);
        DrawTiles(agObjectsTilemap, template.objectTiles, startPos);
        DrawTiles(agDetailsTilemap, template.detailTiles, startPos);
    }
    public void DrawStructureTemplateForGeneration(StructureTemplate template, Vector3Int startPos, STRUCTURE_TYPE structureType) {
        DrawTiles(agGroundTilemap, template.groundTiles, startPos);
        DrawTiles(agGroundWallTilemap, template.groundWallTiles, startPos);
        DrawTiles(agStructureTilemap, template.structureWallTiles, startPos);
        DrawTiles(agObjectsTilemap, template.objectTiles, startPos);
        DrawTiles(agDetailsTilemap, template.detailTiles, startPos);
        AddPlacedStructure(structureType, new StructureSlot() { size = template.size, startPos = startPos, furnitureSpots = template.furnitureSpots});
    }
    private void DrawTiles(Tilemap tilemap, TileTemplateData[] data, Vector3Int startPos) {
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
            pos.x += startPos.x;
            pos.y += startPos.y;
            if (tilemap == agGroundTilemap) {
                if (tilemap.GetTile(pos) != null && !tilemap.GetTile(pos).name.Contains("Dirt") && !tilemap.GetTile(pos).name.Contains("SnowOutside")) {
                    //if the tile map is the ground tile map, and the tile to be replaced is not dirt, do not draw tile
                    continue; //skip drawing this tile
                }
            } else {
                if (tilemap.GetTile(pos) != null) {
                    continue; //skip drawing this tile
                }
            }
            
            if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                tilemap.SetTile(pos, GetTileAsset(currData.tileAssetName, true));
            }
            
            //else {
            //    tilemap.SetTile(pos, GetTileAsset(currData.tileAssetName));
            //}
            tilemap.SetTransformMatrix(pos, currData.matrix);

        }
    }
    private TileTemplateData[] GetTileData(Tilemap tilemap, BoundsInt bounds) {
        TileTemplateData[] data = new TileTemplateData[bounds.size.x * bounds.size.y];
        int count = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = tilemap.GetTile(pos);
                Matrix4x4 matrix = tilemap.GetTransformMatrix(pos);

                int normalizedX = x;
                int normalizedY = y;

                if (bounds.xMin != 0) {
                    if (bounds.xMin < 0) {
                        //x is negative
                        normalizedX += Mathf.Abs(bounds.xMin);
                    } else {
                        //x is positive
                        normalizedX -= Mathf.Abs(bounds.xMin);
                    }
                }

                if (bounds.yMin != 0) {
                    if (bounds.yMin < 0) {
                        //y is negative
                        normalizedY += Mathf.Abs(bounds.yMin);
                    } else {
                        //y is positive
                        normalizedY -= Mathf.Abs(bounds.yMin);
                    }
                }

                data[count] = new TileTemplateData(tb, matrix, new Vector3(normalizedX, normalizedY, 0));
                count++;
            }
        }
        return data;
    }
    private void AddPlacedStructure(STRUCTURE_TYPE type, StructureSlot slot) {
        if (!placedStructures.ContainsKey(type)) {
            placedStructures.Add(type, new List<StructureSlot>());
        }
        placedStructures[type].Add(slot);
    }
    /// <summary>
    /// Get the units the template needs to move.
    /// </summary>
    /// <param name="template">The template that needs to move</param>
    /// <param name="connection1">The connection from the template</param>
    /// <param name="connection2">The connection the template will connect to</param>
    /// <returns></returns>
    public Vector3Int GetMoveUnitsOfTemplateGivenConnections(StructureTemplate template, StructureConnector connection1, StructureConnector connection2) {
        Vector3Int shiftTemplateBy = connection2.Difference(connection1);
        switch (connection2.neededDirection) {
            case Cardinal_Direction.North:
                shiftTemplateBy.y += 1;
                break;
            case Cardinal_Direction.South:
                shiftTemplateBy.y -= 1;
                break;
            case Cardinal_Direction.East:
                shiftTemplateBy.x += 1;
                break;
            case Cardinal_Direction.West:
                shiftTemplateBy.x -= 1;
                break;
            default:
                break;
        }
        return shiftTemplateBy;
    }
    public List<StructureTemplate> GetValidTownCenterTemplates(Area area) {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        string extension = "Default";
        if (area.name == "Gloomhollow") {
            extension = "Snow";
        }
        List<StructureTemplate> choices = GetStructureTemplates("TOWN CENTER/" + extension);
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemplate = choices[i];
            if (currTemplate.HasConnectorsForStructure(area.structures)) {
                valid.Add(currTemplate);
            }
        }

        return valid;
    }
    #endregion

    #region Tile Object
    public bool HasSettingForTileObjectAsset(TileBase asset) {
        return tileObjectSlotSettings.ContainsKey(asset);
    }
    /// <summary>
    /// Get the slot settings for a given tile object asset.
    /// NOTE: should be used in conjunction with <see cref="HasSettingForTileObjectAsset"/> to check if any settings are available, since TileObjectSettings cannot be null.
    /// </summary>
    /// <param name="asset">The asset used by the tile object.</param>
    /// <returns>The list of slot settings</returns>
    public List<TileObjectSlotSetting> GetTileObjectSlotSettings(TileBase asset) {
        return tileObjectSlotSettings[asset];
    }
    public void AddTileObject(TileObject to) {
        if (!allTileObjects.ContainsKey(to.tileObjectType)) {
            allTileObjects.Add(to.tileObjectType, new List<TileObject>());
        }
        if (!allTileObjects[to.tileObjectType].Contains(to)) {
            allTileObjects[to.tileObjectType].Add(to);
        }
    }
    public void RemoveTileObject(TileObject to) {
        if (allTileObjects.ContainsKey(to.tileObjectType)) {
            allTileObjects[to.tileObjectType].Remove(to);
        }
    }
    public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
        if (allTileObjects.ContainsKey(type)) {
            for (int i = 0; i < allTileObjects[type].Count; i++) {
                TileObject to = allTileObjects[type][i];
                if(to.id == id) {
                    return to;
                }
            }
        }
        return null;
    }
    #endregion

    #region Lighting
    public void UpdateLightBasedOnTime(GameDate date) {
        foreach (KeyValuePair<int, float> keyValuePair in lightSettings) {
            if (date.tick > keyValuePair.Key) {
                areaMapLight.intensity = keyValuePair.Value;
            }
        }
    }
    private void CheckForChangeLight() {
        if (lightSettings.ContainsKey(GameManager.Instance.tick)) {
            StartCoroutine(TransitionLightTo(lightSettings[GameManager.Instance.tick]));
        }
    }
    private IEnumerator TransitionLightTo(float intensity) {
        while (true) {
            if (GameManager.Instance.isPaused) {
                yield return null;
            }
            if (intensity > areaMapLight.intensity) {
                areaMapLight.intensity += 0.05f;
            } else if (intensity < areaMapLight.intensity) {
                areaMapLight.intensity -= 0.05f;
            }
            if (Mathf.Approximately(areaMapLight.intensity, intensity)) {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}

#region Templates
public struct TownMapSettings {

    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] groundWallTiles;
    public TileTemplateData[] structureTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public Dictionary<STRUCTURE_TYPE, List<StructureSlot>> structureSlots;

    public void LogInfo() {
        string info = "Town Map Info: " + size.ToString();
        info += "\nGround tiles: " + groundTiles.Length.ToString();
        info += "\nStructure tiles: " + structureTiles.Length.ToString();
        info += "\nObejct tiles: " + objectTiles.Length.ToString();
        info += "\nDetail tiles: " + detailTiles.Length.ToString();
        //Debug.Log(info);
    }

}
public class StructureSlot {
    public Vector3Int startPos;
    public Point size;
    public FurnitureSpot[] furnitureSpots;
    public void AdjustStartPos(int x, int y) {
        Vector3Int newPos = startPos;
        newPos.x += x;
        newPos.y += y;
        startPos = newPos;
    }
    public bool TryGetFurnitureSpot(Vector3Int location, out FurnitureSpot furnitureSpot) {
        for (int i = 0; i < furnitureSpots.Length; i++) {
            FurnitureSpot spot = furnitureSpots[i];
            if (spot.location == location) {
                furnitureSpot = spot;
                return true;
            }
        }
        furnitureSpot = default(FurnitureSpot);
        return false;
    }
}
#endregion

public struct LocationGridTileSettings {
    public TileTemplateData groundTile;
    public TileTemplateData groundWallTile;
    public TileTemplateData detailTile;
    public TileTemplateData structureWallTile;
    public TileTemplateData objectTile;

    public LocationGridTileSettings MergeWith(LocationGridTileSettings otherSetting) {
        LocationGridTileSettings setting = this;
        bool overrideGroundTile = true;
        if (!string.IsNullOrEmpty(groundTile.tileAssetName) && !groundTile.tileAssetName.Contains("Dirt")) {
            //if the ground tile of this current setting is not dirt, do not replace it.
            overrideGroundTile = false;
        }
        setting.groundTile = otherSetting.groundTile;
        setting.groundWallTile = otherSetting.groundWallTile;
        setting.detailTile = otherSetting.detailTile;
        setting.structureWallTile = otherSetting.structureWallTile;
        setting.objectTile = otherSetting.objectTile;
        return setting;
    }

    public void UpdatePositions(Vector3 newPos) {
        groundTile.tilePosition = newPos;
        groundWallTile.tilePosition = newPos;
        detailTile.tilePosition = newPos;
        structureWallTile.tilePosition = newPos;
        objectTile.tilePosition = newPos;
    }
}