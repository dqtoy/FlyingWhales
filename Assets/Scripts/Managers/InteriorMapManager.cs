using System.Collections;
using System.Collections.Generic;
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
        currentlyShowingMap = null;
        currentlyShowingArea = null;
    }
    public void OnCreateAreaMap(AreaInnerTileMap newMap) {
        areaMaps.Add(newMap);
        newMap.transform.localPosition = nextMapPos;
        //set the next map position based on the new maps height
        nextMapPos = new Vector3(nextMapPos.x, nextMapPos.y + newMap.height + 1, nextMapPos.z);
    }

    #region For Testing
    bool isShowingMarkerTileData = false;
    private void ShowTileData(LocationGridTile tile, Character character = null) {
        if (tile == null) {
            return;
        }
        string summary = tile.localPlace.ToString();
        summary += "\nLocal Location: " + tile.localLocation.ToString();
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
            summary += "\nPOI's in Range: ";
            if (character.marker.inRangePOIs.Count > 0) {
                for (int i = 0; i < character.marker.inRangePOIs.Count; i++) {
                    IPointOfInterest poi = character.marker.inRangePOIs[i];
                    summary += "\n- " + poi.name;
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

