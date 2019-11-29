using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationStructureObject : MonoBehaviour {

    public enum Structure_Visual_Mode { Blueprint, Built }

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
    [Header("Pathfinding")]
    [SerializeField] private TilemapCollider2D tilemapCollider;

    private LocationGridTile[] _tiles;
    private Tilemap[] allTilemaps;
    private TilemapCollider2D wallCollider; //NOTE: will eventually change this so that walls each have their own graph update scene object.

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

    #region Getters
    public LocationGridTile[] tiles {
        get { return _tiles; }
    }
    public Vector2Int size {
        get { return _size; }
    }
    #endregion

    #region Monobehaviours
    void Awake() {
        allTilemaps = this.transform.GetComponentsInChildren<Tilemap>();
        wallCollider = _wallTileMap.gameObject.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null) {
            tilemapCollider = wallCollider;
        }
    }
    #endregion

    #region Tile Maps
    public void RefreshAllTilemaps() {
        for (int i = 0; i < allTilemaps.Length; i++) {
            allTilemaps[i].RefreshAllTiles();
        }
    }
    private void UpdateSortingOrders() {
        _groundTileMapRenderer.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 5;
        _wallTileMapRenderer.sortingOrder = _groundTileMapRenderer.sortingOrder + 2;
        _detailTileMapRenderer.sortingOrder = InteriorMapManager.Details_Tilemap_Sorting_Order;
    }
    private void SetAllTilemapsColor(Color color) {
        for (int i = 0; i < allTilemaps.Length; i++) {
            allTilemaps[i].color = color;
        }
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

            TileObject newTileObject = InteriorMapManager.Instance.CreateNewTileObject(preplacedObj.tileObjectType);
            structure.AddPOI(newTileObject, tile);
            newTileObject.areaMapGameObject.OverrideVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.areaMapGameObject.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
        }
        RemovePreplacedObjectSettings();
    }
    public void PlacePreplacedObjectsAsBlueprints(LocationStructure structure, AreaInnerTileMap areaMap) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];
            tile.SetReservedType(preplacedObj.tileObjectType);

            TileObject newTileObject = InteriorMapManager.Instance.CreateNewTileObject(preplacedObj.tileObjectType);
            structure.AddPOI(newTileObject, tile);
            newTileObject.areaMapGameObject.OverrideVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.areaMapGameObject.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
            newTileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);

            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_FURNITURE, INTERACTION_TYPE.CRAFT_TILE_OBJECT, newTileObject, areaMap.area);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCraftFurnitureJob);
            areaMap.area.AddToAvailableJobs(job);
        }
    }
    private StructureTemplateObjectData[] GetPreplacedObjects() {
        return Utilities.GetComponentsInDirectChildren<StructureTemplateObjectData>(_objectsParent.gameObject);
    }
    internal void ReceiveMapObject<T>(AreaMapObjectVisual<T> areaMapGameObject) where T : IPointOfInterest {
        areaMapGameObject.transform.SetParent(_objectsParent);
    }
    private void RemovePreplacedObjectSettings() {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        if (preplacedObjs != null) {
            for (int i = 0; i < preplacedObjs.Length; i++) {
                GameObject.Destroy(preplacedObjs[i].gameObject);
            }
        }
    }
    private void DisablePreplacedObjects() {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        if (preplacedObjs != null) {
            for (int i = 0; i < preplacedObjs.Length; i++) {
               preplacedObjs[i].gameObject.SetActive(false);
            }
        }
    }
    public void ClearOutUnimportantObjectsBeforePlacement() {
        for (int i = 0; i < _tiles.Length; i++) {
            LocationGridTile tile = _tiles[i];
            if (tile.objHere != null && (tile.objHere is BuildSpotTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                tile.structure.RemovePOI(tile.objHere);
            }
            tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, null);

            tile.parentAreaMap.northEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.southEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.eastEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.westEdgeTilemap.SetTile(tile.localPlace, null);

            //clear out any details and objects on tiles adjacent to the built structure
            List<LocationGridTile> differentStructureTiles = tile.neighbourList.Where(x => !_tiles.Contains(x)).ToList();
            for (int j = 0; j < differentStructureTiles.Count; j++) {
                LocationGridTile diffTile = differentStructureTiles[j];
                if (diffTile.objHere != null && (diffTile.objHere is BuildSpotTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                    diffTile.structure.RemovePOI(diffTile.objHere);
                }
                diffTile.parentAreaMap.detailsTilemap.SetTile(diffTile.localPlace, null);

                GridNeighbourDirection dir;
                if (diffTile.TryGetNeighbourDirection(tile, out dir)) {
                    switch (dir) {
                        case GridNeighbourDirection.North:
                            diffTile.parentAreaMap.northEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.South:
                            diffTile.parentAreaMap.southEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.West:
                            diffTile.parentAreaMap.westEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.East:
                            diffTile.parentAreaMap.eastEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        default:
                            break;
                    }
                }

            }
        }
    }
    #endregion

    #region Furniture Spots
    private void RegisterFurnitureSpots(AreaInnerTileMap areaMap) {
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
    /// <summary>
    /// Actions to do when a structure object has been placed.
    /// </summary>
    /// <param name="areaMap">The map where the structure was placed.</param>
    public void OnStructureObjectPlaced(AreaInnerTileMap areaMap, LocationStructure structure) {
        UpdateSortingOrders();
        for (int i = 0; i < _tiles.Length; i++) {
            LocationGridTile tile = _tiles[i];
            //check if the template has details at this tiles location
            tile.hasDetail = _detailTileMap.GetTile(_detailTileMap.WorldToCell(tile.worldLocation)) != null;
            if (tile.hasDetail) { //if it does then set that tile as occupied
                tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            }

            TileBase groundTile = _groundTileMap.GetTile(_groundTileMap.WorldToCell(tile.worldLocation));
            //set the ground asset of the parent area map to what this objects ground map uses, then clear this objects ground map
            tile.SetGroundTilemapVisual(groundTile);

           

            //update tile type based on wall asset.
            TileBase wallAsset = _wallTileMap.GetTile(_wallTileMap.WorldToCell(tile.worldLocation));
            if (wallAsset != null) {
                if (wallAsset.name.Contains("Door")) {
                    tile.SetTileType(LocationGridTile.Tile_Type.Structure_Entrance);
                } else {
                    tile.SetTileType(LocationGridTile.Tile_Type.Wall);
                }
            }

            
            tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, null);
        }
        _groundTileMap.ClearAllTiles();
        RegisterFurnitureSpots(areaMap);
        areaMap.area.OnLocationStructureObjectPlaced(structure);
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
    public bool IsBiggerThanBuildSpot() {
        return _size.x > InteriorMapManager.Building_Spot_Size.x || _size.y > InteriorMapManager.Building_Spot_Size.y;
    }
    public bool IsHorizontallyBig() {
        return _size.x > InteriorMapManager.Building_Spot_Size.x;
    }
    public bool IsVerticallyBig() {
        return _size.y > InteriorMapManager.Building_Spot_Size.y;
    }
    #endregion

    #region Visuals
    public void SetVisualMode(Structure_Visual_Mode mode, AreaInnerTileMap map) {
        Color color = Color.white;
        switch (mode) {
            case Structure_Visual_Mode.Blueprint:
                color.a = 128f / 255f;
                SetAllTilemapsColor(color);
                wallCollider.enabled = false;
                DisablePreplacedObjects();
                break;
            default:
                color = Color.white;
                SetAllTilemapsColor(color);
                wallCollider.enabled = true;
                AstarPath.active.UpdateGraphs(tilemapCollider.bounds);
                //TODO: Scan Pathfinding Graph using Graph Update Scene instead of rescanning the whole grid.
                //PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
                break;
        }
    }
    #endregion

}
