using Pathfinding;
using System.Collections;
using System.Collections.Generic;
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


    private void Awake() {
        Instance = this;
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
        gg.collision.diameter = 1f;
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
            }
        }
        if (character != null) {
            summary += "\nCharacter: " + character.name;
            summary += "\nDestination: " + (character.marker.destinationTile != null ? character.marker.destinationTile.ToString() : "None");
            //summary += "\nPOI's in Range: ";
            //if (character.marker.inVisionPOIs.Count > 0) {
            //    for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
            //        IPointOfInterest poi = character.marker.inVisionPOIs[i];
            //        summary += "\n- " + poi.name;
            //    }
            //} else {
            //    summary += "None";
            //}
            summary += "\nHostiles in Range: ";
            if (character.marker.hostilesInRange.Count > 0) {
                for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                    Character poi = character.marker.hostilesInRange.ElementAt(i);
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
        }
        summary += "\nOccupant: " + tile.occupant?.name ?? "None";        

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

