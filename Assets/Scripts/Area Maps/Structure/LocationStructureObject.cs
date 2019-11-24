using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationStructureObject : MonoBehaviour {

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _groundTileMap;
    [SerializeField] private Tilemap _wallTileMap;
    [SerializeField] private Tilemap _detailTileMap;
    [SerializeField] private TilemapRenderer _groundTileMapRenderer;
    [SerializeField] private TilemapRenderer _wallTileMapRenderer;
    [SerializeField] private TilemapRenderer _detailTileMapRenderer;
    [Header("Template Data")]
    [SerializeField] private Vector2Int _size;
    [SerializeField] private Vector3Int _center;
    [Header("Objects")]
    [SerializeField] private Transform _objectsParent;
    [Header("Furniture Spots")]
    [SerializeField] private Transform _furnitureSpotsParent;

    private LocationGridTile[] _tiles;

    #region Testers
    [ContextMenu("Log Occupied coordinates")]
    public void LogOccupiedCoordinates() {
        _groundTileMap.CompressBounds();
        BoundsInt bounds = _groundTileMap.cellBounds;
        string summary = "Occupied coordinates of " + this.name;
        for (int x = 0; x < bounds.xMax; x++) {
            for (int y = 0; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = _groundTileMap.GetTile(pos);
                if (tb != null) {
                    summary += "\n" + pos.ToString();
                }
            }
        }
        Debug.Log(summary);
    }
    [ContextMenu("Log Bounds")]
    public void LogBounds() {
        _groundTileMap.CompressBounds();
        BoundsInt bounds = _groundTileMap.cellBounds;
        string boundsSummary = bounds.ToString();
        boundsSummary += "\nxMin - " + bounds.xMin.ToString();
        boundsSummary += "\nyMin - " + bounds.yMin.ToString();
        boundsSummary += "\nxMax - " + bounds.xMax.ToString();
        boundsSummary += "\nyMax - " + bounds.yMax.ToString();
        Debug.Log(boundsSummary);
    }
    #endregion

    #region Tile Maps
    public void RefreshAllTilemaps() {
        _groundTileMap.RefreshAllTiles();
        _wallTileMap.RefreshAllTiles();
        _detailTileMap.RefreshAllTiles();
    }
    private void UpdateSortingOrders() {
        _groundTileMapRenderer.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 5;
        _wallTileMapRenderer.sortingOrder = _groundTileMapRenderer.sortingOrder + 1;
        _detailTileMapRenderer.sortingOrder = InteriorMapManager.Details_Tilemap_Sorting_Order;
    }
    #endregion

    #region Tiles
    public void SetTilesInStructure(LocationGridTile[] tiles) {
        this._tiles = tiles;
    }
    #endregion

    #region Tile Objects
    public void RegisterPreplacedObjects(LocationStructure structure, AreaInnerTileMap areaMap) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];
            tile.SetReservedType(preplacedObj.tileObjectType);

            TileObject newTileObject = InteriorMapManager.Instance.CreateNewTileObject(preplacedObj.tileObjectType, structure);
            newTileObject.areaMapGameObject.OverrideVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.areaMapGameObject.SetRotation(preplacedObj.transform.localEulerAngles.z);
            structure.AddPOI(newTileObject, tile);
        }
    }
    private StructureTemplateObjectData[] GetPreplacedObjects() {
        return Utilities.GetComponentsInDirectChildren<StructureTemplateObjectData>(_objectsParent.gameObject);
    }
    internal void ReceiveMapObject<T>(AreaMapGameObject<T> areaMapGameObject) where T : IPointOfInterest {
        areaMapGameObject.transform.SetParent(_objectsParent);
    }
    #endregion

    #region Furniture Spots
    public void RegisterFurnitureSpots(AreaInnerTileMap areaMap) {
        if (_furnitureSpotsParent == null) {
            return;
        }
        FurnitureSpotMono[] spots = GetFurnitureSpots();
        for (int i = 0; i < spots.Length; i++) {
            FurnitureSpotMono spot = spots[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(spot.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];
            tile.SetFurnitureSpot(spot.GetFurnitureSpot());
        }
    }
    private FurnitureSpotMono[] GetFurnitureSpots() {
        return Utilities.GetComponentsInDirectChildren<FurnitureSpotMono>(_furnitureSpotsParent.gameObject);
    }
    #endregion

    #region Events
    public void OnStructureObjectPlaced() {
        UpdateSortingOrders();
        //clear out any objects or details on this objects occupied tiles
        for (int i = 0; i < _tiles.Length; i++) {
            LocationGridTile tile = _tiles[i];
            if (tile.objHere != null) {
                tile.structure.RemovePOI(tile.objHere);
            }
            //check if the template has details at this tiles location
            tile.hasDetail = _detailTileMap.GetTile(_detailTileMap.LocalToCell(_detailTileMap.WorldToLocal(tile.worldLocation))) != null;
            if (tile.hasDetail) { //if it does then set that tile as occupied
                tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            }
            //clear all details on the main area map detail tile map
            tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, null);
        }
    }
    #endregion

    #region Inquiry
    public List<LocationGridTile> GetTilesOccupiedByStructure(AreaInnerTileMap map) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();

        _groundTileMap.CompressBounds();
        BoundsInt bounds = _groundTileMap.cellBounds;

        List<Vector3Int> occupiedCoordinates = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = _groundTileMap.GetTile(pos);
                if (tb != null) {
                    occupiedCoordinates.Add(pos);
                }
            }
        }

        Vector3Int actualLocation = new Vector3Int(Mathf.FloorToInt(this.transform.localPosition.x), Mathf.FloorToInt(this.transform.localPosition.y), 0);
        for (int i = 0; i < occupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = occupiedCoordinates[i];

            Vector3Int gridTileLocation = actualLocation;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - _center.x;
            int yDiffFromCenter = currCoordinate.y - _center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;


            LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
            occupiedTiles.Add(tile);
        }
        return occupiedTiles;
    }
    #endregion

}
