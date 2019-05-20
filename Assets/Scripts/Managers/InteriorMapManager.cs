using Pathfinding;
using Pathfinding.RVO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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

    //Used for generating the inner map of an area, structure templates are first placed here before generating the actual map
    public Tilemap agGroundTilemap;
    public Tilemap agStructureTilemap;
    public Tilemap agObjectsTilemap;
    public Tilemap agDetailsTilemap;
    
    [Header("Pathfinding")]
    [SerializeField] private AstarPath pathfinder;
    private const float nodeSize = 0.2f;
    public const int Default_Character_Sorting_Order = 20;

    //structure templates
    private string templatePath;

    //Local Avoidance
    private Pathfinding.RVO.Simulator sim;
    public List<TileBase> allTileAssets;

    //tile objects
    private Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>() {
        { TILE_OBJECT_TYPE.SUPPLY_PILE, new TileObjectData() { constructionCost = 10, constructionTime = 12 } }
    };

    private void Awake() {
        Instance = this;
        templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
    }
    public void LateUpdate() {
        if (UIManager.Instance.IsMouseOnUI() || currentlyShowingMap == null) {
            return;
        }
        Vector3 mouseWorldPos = (currentlyShowingMap.worldUICanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 localPos = currentlyShowingMap.grid.WorldToLocal(mouseWorldPos);
        Vector3Int coordinate = currentlyShowingMap.grid.LocalToCell(localPos);
        if (coordinate.x >= 0 && coordinate.x < currentlyShowingMap.width
            && coordinate.y >= 0 && coordinate.y < currentlyShowingMap.height) {
            LocationGridTile hoveredTile = currentlyShowingMap.map[coordinate.x, coordinate.y];
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
                if (hoveredTile.objHere != null) {
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
                    if (!isShowingMarkerTileData) {
                        UIManager.Instance.HideSmallInfo();
                    }
                   
                }
            }

        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }

    public void Initialize() {
        areaMaps = new List<AreaInnerTileMap>();
        AreaMapCameraMove.Instance.Initialize();
        //sim = (FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator).GetSimulator();
    }
    public void ShowAreaMap(Area area, bool centerCameraOnMapCenter = true) {
        if (area.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            UIManager.Instance.portalPopup.SetActive(true);
            return;
        }
        area.areaMap.Open();
        currentlyShowingMap = area.areaMap;
        currentlyShowingArea = area;
        Messenger.Broadcast(Signals.AREA_MAP_OPENED, area);

        if (centerCameraOnMapCenter) {
            AreaMapCameraMove.Instance.JustCenterCamera();
        }
    }
    public void HideAreaMap() {
        if (currentlyShowingMap == null) {
            return;
        }
        currentlyShowingMap.Close();
        Messenger.Broadcast(Signals.AREA_MAP_CLOSED, currentlyShowingArea);
        AreaMapCameraMove.Instance.CenterCameraOn(null);
        currentlyShowingMap = null;
        currentlyShowingArea = null;
    }
    public void OnCreateAreaMap(AreaInnerTileMap newMap) {
        areaMaps.Add(newMap);
        newMap.transform.localPosition = nextMapPos;
        //set the next map position based on the new maps height
        nextMapPos = new Vector3(nextMapPos.x, nextMapPos.y + newMap.height + 10, nextMapPos.z);
        CreatePathfindingGraphForArea(newMap);
        newMap.UpdateTilesWorldPosition();
    }

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
    #endregion


    #region Local Avoidance
    //public void RegisterObstacles() {
    //    for (int i = 0; i < areaMaps.Count; i++) {
    //        AreaInnerTileMap map = areaMaps[i];
    //        //get all wall tiles
    //        List<LocationGridTile> walls = map.GetAllWallTiles();
    //        for (int j = 0; j < walls.Count; j++) {
    //            LocationGridTile wall = walls[j];
    //            Vector3[] verts = wall.GetVertices();
    //            for (int k = 0; k < verts.Length; k++) {
    //                Vector3 currVert = verts[k];
    //                int nextVertIndex = k + 1;
    //                if (nextVertIndex == verts.Length) {
    //                    nextVertIndex = 0;
    //                }
    //                sim.AddObstacle(currVert, verts[nextVertIndex], 1f);
    //            }

    //            //sim.AddObstacle(verts, 2);
    //        }
    //    }
    //    List<Pathfinding.RVO.Sampled.Agent> agents = sim.GetAgents();
    //    Debug.Log(agents.Count + " agents!");
    //}
    //public void AddAgent(IAgent agent) {
    //    sim.AddAgent(agent);
    //}
    //public void RemoveAgent(IAgent agent) {
    //    sim.RemoveAgent(agent);
    //}
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
    public List<StructureTemplate> GetStructureTemplates(string folderName, List<string> except) {
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
                    if (except.Contains(loaded.name)) {
                        continue; //skip
                    }
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
    bool isShowingMarkerTileData = false;
    private void ShowTileData(LocationGridTile tile, Character character = null) {
        if (tile == null) {
            return;
        }
        string summary = tile.localPlace.ToString();
        summary += "\nLocal Location: " + tile.localLocation.ToString();
        summary += "\nWorld Location: " + tile.worldLocation.ToString();
        summary += "\nCentered World Location: " + tile.centeredWorldLocation.ToString();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nTile Access: " + tile.tileAccess.ToString();
        summary += "\nReserved Tile Object Type: " + tile.reservedObjectType.ToString();
        summary += "\nFurniture Spot: " + tile.furnitureSpot?.ToString() ?? "None";
        summary += "\nContent: " + tile.objHere?.ToString() ?? "None";
        if (tile.objHere != null) {
            summary += "\n\tObject State: " + tile.objHere.state.ToString();
            if (tile.objHere is Tree) {
                summary += "\n\tYield: " + (tile.objHere as Tree).yield.ToString();
            } else if (tile.objHere is Ore) {
                summary += "\n\tYield: " + (tile.objHere as Ore).yield.ToString();
            } else if (tile.objHere is SupplyPile) {
                summary += "\n\tSupplies in Pile: " + (tile.objHere as SupplyPile).suppliesInPile.ToString();
            } else if (tile.objHere is SpecialToken) {
                summary += "\n\tCharacter Owner: " + (tile.objHere as SpecialToken).characterOwner?.name ?? "None";
                summary += "\n\tFaction Owner: " + (tile.objHere as SpecialToken).factionOwner?.name ?? "None";
            }
            summary += "\n\tTraits: ";
            if (tile.objHere.traits.Count > 0) {
                for (int i = 0; i < tile.objHere.traits.Count; i++) {
                    summary += "\n\t\t- " + tile.objHere.traits[i].name;
                }

            } else {
                summary += "None";
            }
        }
        summary += "\nCharacters Here: ";
        if (tile.charactersHere.Count > 0) {
            for (int i = 0; i < tile.charactersHere.Count; i++) {
                summary += "\n\t- " + tile.charactersHere[i].name;
            }
        } else {
            summary += "None";
        }
        if (character != null) {
            summary += "\nCharacter: " + character.name;
            summary += "\nMood: " + character.currentMoodType.ToString();
            summary += "\nSupply: " + character.supply.ToString();
            summary += "\nDestination: " + (character.marker.destinationTile != null ? character.marker.destinationTile.ToString() : "None");
            summary += "\nMove Speed: " + character.marker.pathfindingAI.speed.ToString();
            summary += "\nTangent: " + character.marker.pathfindingAI.GetTangent().ToString();
            summary += "\nTarget POI: " + character.marker.targetPOI?.ToString() ?? "None";
            summary += "\nDestination Tile: ";
            if (character.marker.destinationTile == null) {
                summary += "None";
            } else {
                summary += character.marker.destinationTile.ToString() + " at " + character.marker.destinationTile.parentAreaMap.area.name; 
            }
            summary += "\nArrival Action: " + character.marker.arrivalAction?.Method.Name ?? "None";
            summary += "\nPOI's in Vision: ";
            if (character.marker.inVisionPOIs.Count > 0) {
                for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                    IPointOfInterest poi = character.marker.inVisionPOIs.ElementAt(i);
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\nPOI's in Range but different structures: ";
            if (character.marker.visionCollision.poisInRangeButDiffStructure.Count > 0) {
                for (int i = 0; i < character.marker.visionCollision.poisInRangeButDiffStructure.Count; i++) {
                    IPointOfInterest poi = character.marker.visionCollision.poisInRangeButDiffStructure[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\nHostiles in Range: ";
            if (character.marker.hostilesInRange.Count > 0) {
                for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                    Character poi = character.marker.hostilesInRange.ElementAt(i);
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\nTerrifying Characters: ";
            if (character.marker.terrifyingCharacters.Count > 0) {
                for (int i = 0; i < character.marker.terrifyingCharacters.Count; i++) {
                    Character currCharacter = character.marker.terrifyingCharacters[i];
                    summary += currCharacter.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\nPersonal Job Queue: ";
            if (character.jobQueue.jobsInQueue.Count > 0) {
                for (int i = 0; i < character.jobQueue.jobsInQueue.Count; i++) {
                    JobQueueItem poi = character.jobQueue.jobsInQueue[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
        }

        if (tile.structure != null) {
            summary += "\nStructure: " + tile.structure.ToString();
            if (tile.structure is Dwelling) {
                Dwelling dwelling = tile.structure as Dwelling;
                summary += "\n\tFacilities: ";
                foreach (KeyValuePair<FACILITY_TYPE, int> keyValuePair in dwelling.facilities) {
                    summary += "\n\t\t- " + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.ToString();
                }
            }
        } else {
            summary += "\nStructure: None";
        }
        UIManager.Instance.ShowSmallInfo(summary);
    //    //For build only
    //    if (tile.objHere != null) {
    //        UIManager.Instance.ShowSmallInfo(tile.objHere.ToString());
    //    }
    }
    public void ShowTileData(Character character, LocationGridTile tile) {
        //if (GameManager.showAllTilesTooltip) {
            isShowingMarkerTileData = true;
            ShowTileData(tile, character);
        //}
    }
    public void HideTileData() {
        //if (GameManager.showAllTilesTooltip) {
            isShowingMarkerTileData = false;
            UIManager.Instance.HideSmallInfo();
        //}
    }

    private IPointOfInterest heldPOI;
    public void HoldPOI(IPointOfInterest poi) {
        heldPOI = poi;
        if (heldPOI is SpecialToken) {
            heldPOI.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(heldPOI as SpecialToken);
        } else if (heldPOI is TileObject) {
            heldPOI.gridTileLocation.structure.RemovePOI(heldPOI);
        }
    }
    public void PlaceHeldPOI(LocationGridTile tile) {
        if (heldPOI is SpecialToken) {
            tile.structure.location.AddSpecialTokenToLocation(heldPOI as SpecialToken, tile.structure, tile);
        } else if (heldPOI is TileObject) {
            tile.structure.AddPOI(heldPOI, tile);
        }
        heldPOI = null;
    }
    public bool IsHoldingPOI() {
        return heldPOI != null;
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
        s.size = agGroundTilemap.cellBounds;

        s.groundTiles = GetTileData(agGroundTilemap, agGroundTilemap.cellBounds);
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
    private Dictionary<STRUCTURE_TYPE, List<StructureSlot>> placedStructures;
    public void DrawTownCenterTemplateForGeneration(StructureTemplate template, Vector3Int startPos) {
        DrawTiles(agGroundTilemap, template.groundTiles, startPos);
        DrawTiles(agStructureTilemap, template.structureWallTiles, startPos);
        DrawTiles(agObjectsTilemap, template.objectTiles, startPos);
        DrawTiles(agDetailsTilemap, template.detailTiles, startPos);
    }
    public void DrawStructureTemplateForGeneration(StructureTemplate template, Vector3Int startPos, STRUCTURE_TYPE structureType) {
        DrawTiles(agGroundTilemap, template.groundTiles, startPos);
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
    #endregion

    #region Tile Objects
    public TileObjectData GetTileObjectData(TILE_OBJECT_TYPE objType) {
        if (tileObjectData.ContainsKey(objType)) {
            return tileObjectData[objType];
        }
        throw new System.Exception("No tile data for type " + objType.ToString());
    }
    #endregion
}

#region Templates
public struct TownMapSettings {

    public BoundsInt size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] structureTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public Dictionary<STRUCTURE_TYPE, List<StructureSlot>> structureSlots;

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
    public FurnitureSpot GetFurnitureSpot(Vector3Int location) {
        for (int i = 0; i < furnitureSpots.Length; i++) {
            FurnitureSpot spot = furnitureSpots[i];
            if (spot.location == location) {
                return spot;
            }
        }
        return null;
    }
}
#endregion

#region Tile Objects
public struct TileObjectData {
    public int constructionCost;
    public int constructionTime; //in ticks
}
#endregion


