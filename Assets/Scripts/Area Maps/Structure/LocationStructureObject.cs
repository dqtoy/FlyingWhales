using EZObjectPools;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationStructureObject : PooledObject {

    public enum Structure_Visual_Mode { Blueprint, Built }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _groundTileMap;
    [SerializeField] private Tilemap _detailTileMap;
    [SerializeField] private TilemapRenderer _groundTileMapRenderer;
    [SerializeField] private TilemapRenderer _detailTileMapRenderer;

    [Header("Template Data")]
    [SerializeField] private Vector2Int _size;
    [SerializeField] private Vector3Int _center;

    [Header("Objects")]
    [SerializeField] private Transform _objectsParent;

    [Header("Furniture Spots")]
    [SerializeField] private Transform _furnitureSpotsParent;

    #region Properties
    private Tilemap[] allTilemaps;
    private WallVisual[] wallVisuals;
    public LocationGridTile[] tiles { get; private set; }
    public WallObject[] walls { get; private set; }
    #endregion

    #region Getters
    public Vector2Int size {
        get { return _size; }
    }
    #endregion

    #region Monobehaviours
    void Awake() {
        allTilemaps = this.transform.GetComponentsInChildren<Tilemap>();
        wallVisuals = this.transform.GetComponentsInChildren<WallVisual>();
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
        _detailTileMapRenderer.sortingOrder = InteriorMapManager.Details_Tilemap_Sorting_Order;
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.UpdateSortingOrders(_groundTileMapRenderer.sortingOrder + 2);
        }
    }
    private void SetStructureColor(Color color) {
        for (int i = 0; i < allTilemaps.Length; i++) {
            allTilemaps[i].color = color;
        }
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.SetWallColor(color);
        }
    }
    #endregion

    #region Tiles
    public void SetTilesInStructure(LocationGridTile[] tiles) {
        this.tiles = tiles;
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
            newTileObject.areaMapVisual.OverrideVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.areaMapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
        }
        SetPreplacedObjectsState(false);
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
            newTileObject.areaMapVisual.OverrideVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.areaMapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
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
    internal void ReceiveMapObject<T>(AreaMapObjectVisual<T> areaMapGameObject) where T : IDamageable {
        areaMapGameObject.transform.SetParent(_objectsParent);
    }
    private void SetPreplacedObjectsState(bool state) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        if (preplacedObjs != null) {
            for (int i = 0; i < preplacedObjs.Length; i++) {
               preplacedObjs[i].gameObject.SetActive(state);
            }
        }
    }
    public void ClearOutUnimportantObjectsBeforePlacement() {
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.objHere != null && (tile.objHere is BuildSpotTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                tile.structure.RemovePOI(tile.objHere);
            }
            tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, null);

            tile.parentAreaMap.northEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.southEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.eastEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentAreaMap.westEdgeTilemap.SetTile(tile.localPlace, null);

            //clear out any details and objects on tiles adjacent to the built structure
            List<LocationGridTile> differentStructureTiles = tile.neighbourList.Where(x => !tiles.Contains(x)).ToList();
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
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            //check if the template has details at this tiles location
            tile.hasDetail = _detailTileMap.GetTile(_detailTileMap.WorldToCell(tile.worldLocation)) != null;
            if (tile.hasDetail) { //if it does then set that tile as occupied
                tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            }

            TileBase groundTile = _groundTileMap.GetTile(_groundTileMap.WorldToCell(tile.worldLocation));
            //set the ground asset of the parent area map to what this objects ground map uses, then clear this objects ground map
            tile.SetGroundTilemapVisual(groundTile);
            
            tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, null);
        }
        RegisterWalls(areaMap, structure);
        _groundTileMap.gameObject.SetActive(false);
        RegisterFurnitureSpots(areaMap);
        areaMap.area.OnLocationStructureObjectPlaced(structure);
        UpdateSortingOrders();

        //if (structure.structureType.IsOpenSpace() == false) {
        //    int random = UnityEngine.Random.Range(0, 3);
        //    if (random == 0) {
        //        structure.ChangeResourceMadeOf(RESOURCE.METAL);
        //    } else if (random == 1) {
        //        structure.ChangeResourceMadeOf(RESOURCE.STONE);
        //    } else {
        //        structure.ChangeResourceMadeOf(RESOURCE.WOOD);
        //    }
        //}
        
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
                SetStructureColor(color);
                SetPreplacedObjectsState(false);
                break;
            default:
                color = Color.white;
                SetStructureColor(color);
                RescanPathfindingGridOfStructure();
                break;
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        SetPreplacedObjectsState(true);
        _groundTileMap.gameObject.SetActive(true);
        tiles = null;
    }
    #endregion

    #region Walls
    private void RegisterWalls(AreaInnerTileMap map, LocationStructure structure) {
        walls = new WallObject[wallVisuals.Length];
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            WallObject wallObject = new WallObject(structure, wallVisual);
            Vector3Int tileLocation = map.groundTilemap.WorldToCell(wallVisual.transform.position);
            LocationGridTile tile = map.map[tileLocation.x, tileLocation.y];
            tile.SetTileType(LocationGridTile.Tile_Type.Wall);
            wallObject.SetGridTileLocation(tile);
            tile.AddWallObject(wallObject);
            walls[i] = wallObject;
        }
    }
    internal void ChangeResourceMadeOf(RESOURCE resource) {
        for (int i = 0; i < walls.Length; i++) {
            WallObject wallObject = walls[i];
            wallObject.ChangeResourceMadeOf(resource);
        }
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            switch (resource) {
                case RESOURCE.WOOD:
                    tile.SetGroundTilemapVisual(InteriorMapManager.Instance.GetTileAsset("Structure Floor Tile"));
                    break;
                case RESOURCE.STONE:
                    tile.SetGroundTilemapVisual(InteriorMapManager.Instance.GetTileAsset("Stone Floor Tile"));
                    break;
                case RESOURCE.METAL:
                    tile.SetGroundTilemapVisual(InteriorMapManager.Instance.GetTileAsset("Stone Floor Tile"));
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Pathfinding
    internal void RescanPathfindingGridOfStructure() {
        AstarPath.active.UpdateGraphs(_groundTileMapRenderer.bounds);
    }
    #endregion

    [Header("Wall Converter")]
    [SerializeField] private Tilemap wallTileMap;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject cornerPrefab;
    [ContextMenu("Convert Walls")]
    public void ConvertWalls() {
        wallTileMap.CompressBounds();
        BoundsInt bounds = wallTileMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = wallTileMap.GetTile(pos);

                Vector3 worldPos = wallTileMap.CellToWorld(pos);

                if (tile != null) {
                    Vector2 centeredPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f);
                    GameObject wallGO = null;
                    if (tile.name.Contains("Door")) {
                        continue; //skip
                    }

                    if (tile.name.Contains("Left")) {
                        wallGO = GameObject.Instantiate(leftWall, wallTileMap.transform);
                        wallGO.transform.position = centeredPos;
                    } 
                    if (tile.name.Contains("Right")) {
                        wallGO = GameObject.Instantiate(rightWall, centeredPos, Quaternion.identity, wallTileMap.transform);
                        wallGO.transform.position = centeredPos;
                    }
                    if (tile.name.Contains("Bot")) {
                        wallGO = GameObject.Instantiate(bottomWall, centeredPos, Quaternion.identity, wallTileMap.transform);
                        wallGO.transform.position = centeredPos;
                    }
                    if (tile.name.Contains("Top")) {
                        wallGO = GameObject.Instantiate(topWall, centeredPos, Quaternion.identity, wallTileMap.transform);
                        wallGO.transform.position = centeredPos;
                    }

                    Vector3 cornerPos = centeredPos;
                    if (tile.name.Contains("BotLeft")) {
                        cornerPos.x -= 0.5f;
                        cornerPos.y -= 0.5f;
                        GameObject.Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallGO.transform);
                    } else if (tile.name.Contains("BotRight")) {
                        cornerPos.x += 0.5f;
                        cornerPos.y -= 0.5f;
                        GameObject.Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallGO.transform);
                    } else if (tile.name.Contains("TopLeft")) {
                        cornerPos.x -= 0.5f;
                        cornerPos.y += 0.5f;
                        GameObject.Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallGO.transform);
                    } else if (tile.name.Contains("TopRight")) {
                        cornerPos.x += 0.5f;
                        cornerPos.y += 0.5f;
                        GameObject.Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallGO.transform);
                    }
                }
            }
        }
    }
}
