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

    [Header("Pathfinding")]
    [SerializeField] private AstarPath pathfinder;
    private const float nodeSize = 0.2f;

    //structure templates
    private string templatePath;

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
    }
    public void ShowAreaMap(Area area) {
        area.areaMap.Open();
        currentlyShowingMap = area.areaMap;
        currentlyShowingArea = area;
        Messenger.Broadcast(Signals.AREA_MAP_OPENED, area);
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
        nextMapPos = new Vector3(nextMapPos.x, nextMapPos.y + newMap.height + 1, nextMapPos.z);
        CreatePathfindingGraphForArea(newMap);
        newMap.UpdateTilesWorldPosition();
    }

    private void CreatePathfindingGraphForArea(AreaInnerTileMap newMap) {
        GridGraph gg = pathfinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gg.cutCorners = false;
        gg.rotation = new Vector3(-90f, 0f, 0f);
        gg.nodeSize = nodeSize;

        int reducedWidth = newMap.width - (AreaInnerTileMap.eastEdge + AreaInnerTileMap.westEdge);
        int reducedHeight = newMap.height - (AreaInnerTileMap.northEdge + AreaInnerTileMap.southEdge);

        gg.SetDimensions(Mathf.FloorToInt((float)reducedWidth / gg.nodeSize), Mathf.FloorToInt((float)reducedHeight / gg.nodeSize), nodeSize);
        Vector3 pos = this.transform.position;
        pos.x += ((float)newMap.width / 2f);
        pos.y += ((float)newMap.height / 2f) + newMap.transform.localPosition.y;
        pos.x += ((AreaInnerTileMap.eastEdge + AreaInnerTileMap.westEdge) / 2 ) - 1;

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

    #region Local Avoidance
    public void RegisterObstacles() {
        Pathfinding.RVO.Simulator sim = (FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator).GetSimulator();
        for (int i = 0; i < areaMaps.Count; i++) {
            AreaInnerTileMap map = areaMaps[i];
            //get all wall tiles
            List<LocationGridTile> walls = map.GetAllWallTiles();
            for (int j = 0; j < walls.Count; j++) {
                LocationGridTile wall = walls[j];
                Vector3[] verts = wall.GetVertices();
                for (int k = 0; k < verts.Length; k++) {
                    Vector3 currVert = verts[k];
                    int nextVertIndex = k + 1;
                    if (nextVertIndex == verts.Length) {
                        nextVertIndex = 0;
                    }
                    sim.AddObstacle(currVert, verts[nextVertIndex], 1f);
                }

                //sim.AddObstacle(verts, 2);
            }
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
    public TileBase GetTileAsset(string name) {
        List<TileBase> allTileAssets = LoadAllTilesAssets();
        for (int i = 0; i < allTileAssets.Count; i++) {
            TileBase currTile = allTileAssets[i];
            if (currTile.name == name) {
                return currTile;
            }
        }
        return null;
    }
    private List<TileBase> LoadAllTilesAssets() {
        return Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
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
        summary += "\nIs Inside: " + tile.isInside.ToString();
        summary += "\nIs Edge: " + tile.isEdge.ToString();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nTile Access: " + tile.tileAccess.ToString();
        summary += "\nHas Detail: " + tile.hasDetail.ToString();
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
        }
        if (character != null) {
            summary += "\nCharacter: " + character.name;
            summary += "\nDestination: " + (character.marker.destinationTile != null ? character.marker.destinationTile.ToString() : "None");
            summary += "\nPOI's in Vision: ";
            if (character.marker.inVisionPOIs.Count > 0) {
                for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                    IPointOfInterest poi = character.marker.inVisionPOIs.ElementAt(i);
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
        //summary += "\nOccupant: " + tile.occupant?.name ?? "None";        

        //if (tile.structure != null) {
        summary += "\nStructure: " + tile.structure?.ToString() ?? "None";
        //}
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void ShowTileData(Character character, LocationGridTile tile) {
        //return;
        isShowingMarkerTileData = true;
        ShowTileData(tile, character);
    }
    public void HideTileData() {
        isShowingMarkerTileData = false;
        UIManager.Instance.HideSmallInfo();
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
}

