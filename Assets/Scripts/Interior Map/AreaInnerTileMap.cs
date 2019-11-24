using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class AreaInnerTileMap : MonoBehaviour {

    public static int westEdge = 7;
    public static int northEdge = 1;
    public static int southEdge = 1;
    public static int eastEdge = 2;

    public static int eastOutsideTiles = 15;
    public static int westOutsideTiles = 8;
    public static int northOutsideTiles = 8;
    public static int southOutsideTiles = 8;

    public int width;
    public int height;
    public GridGraph pathfindingGraph;

    public Grid grid;
    public Canvas worldUICanvas;
    [SerializeField] private Canvas canvas;

    [Header("Tile Maps")]
    public Tilemap groundTilemap;
    public Tilemap detailsTilemap;
    public Tilemap structureTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase snowOutsideTile;

    [Header("Special Cases")]
    //bed
    public TileBase bed2SleepingVariant;
    public TileBase bed1SleepingVariant;   

    [Header("Oustide Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase soilTile;
    [SerializeField] private TileBase stoneTile;

    [Header("Snow Outside Tiles")]
    [SerializeField] private TileBase snowTile;
    [SerializeField] private TileBase tundraTile;
    [SerializeField] private TileBase snowDirt;

    [Header("Oustide Detail Tiles")]
    [SerializeField] private TileBase bigTreeTile;
    [SerializeField] private TileBase treeTile;
    [SerializeField] private TileBase shrubTile;
    [SerializeField] private TileBase flowerTile;
    [SerializeField] private TileBase rockTile;
    [SerializeField] private TileBase randomGarbTile;

    [Header("Snow Detail Tiles")]
    [SerializeField] private TileBase snowBigTreeTile;
    [SerializeField] private TileBase snowTreeTile;
    [SerializeField] private TileBase snowFlowerTile;
    [SerializeField] private TileBase snowGarbTile;

    [Header("Inside Detail Tiles")]
    [SerializeField] private TileBase crateBarrelTile;

    [Header("Objects")]
    public Transform objectsParent;

    [Header("Other")]
    public Vector4 cameraBounds;
    public GameObject centerGOPrefab;
    public Transform graphUpdateScenesParent;

    [Header("Structures")]
    [SerializeField] private GameObject buildSpotPrefab;
    public Transform structureParent;

    [Header("Perlin Noise")]
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    [Header("Seamless Edges")]
    public Tilemap northEdgeTilemap;
    public Tilemap southEdgeTilemap;
    public Tilemap westEdgeTilemap;
    public Tilemap eastEdgeTilemap;
    [SerializeField] private SeamlessEdgeAssetsDictionary edgeAssets; //0-north, 1-south, 2-west, 3-east

    [Header("For Testing")]
    [SerializeField] private LineRenderer pathLineRenderer;

    //burning
    public List<BurningSource> activeBurningSources { get; private set; }
    //Building spots
    public List<BuildingSpot> buildingSpots { get; private set; }
    public Character hoveredCharacter { get; private set; }
    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> allEdgeTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }
    public TownMapSettings generatedTownMapSettings { get; private set; }
    public Vector3 worldPos { get; private set; }
    public string usedTownCenterTemplateName { get; private set; }
    public GameObject centerGO { get; private set; }
    public bool isShowing {
        get { return InteriorMapManager.Instance.currentlyShowingMap == this; }
    }

    #region Map Generation
    public void Initialize(Area area) {
        this.area = area;
        area.SetAreaMap(this);
        activeBurningSources = new List<BurningSource>();

        //set tile map sorting orders
        TilemapRenderer ground = groundTilemap.gameObject.GetComponent<TilemapRenderer>();
        ground.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order;
        TilemapRenderer details = detailsTilemap.gameObject.GetComponent<TilemapRenderer>();
        details.sortingOrder = InteriorMapManager.Details_Tilemap_Sorting_Order;

        TilemapRenderer northEdge = northEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
        northEdge.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 1;
        TilemapRenderer southEdge = southEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
        southEdge.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 1;
        TilemapRenderer westEdge = westEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
        westEdge.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 2;
        TilemapRenderer eastEdge = eastEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
        eastEdge.sortingOrder = InteriorMapManager.Ground_Tilemap_Sorting_Order + 2;
    }
    public void DrawMap(TownMapSettings generatedSettings) {
        generatedTownMapSettings = generatedSettings;
        GenerateGrid(generatedSettings);
        SplitMap();
        Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
        DrawTownMap(generatedSettings, startPoint);
        CreateBuildingSpots(generatedSettings, startPoint);
        AssignOuterAreas(insideTiles, outsideTiles);
    }
    public void LoadMap(SaveDataAreaInnerTileMap data) {
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();

        LoadBurningSources(data.burningSources);

        LoadGrid(data);
        SplitMap(false);
        //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
        //DrawTownMap(data.generatedTownMapSettings, startPoint);
        //No need to Place Structures since structure of tile is loaded upon loading grid tile
        //AssignOuterAreas(insideTiles, outsideTiles); //no need for this because structure reference is already saved per location grid tile, and this only assigns the tile to either the wilderness or work area structure
    }
    public void OnMapGenerationFinished() {
        this.name = area.name + "'s Inner Map";
        canvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        worldUICanvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        cameraBounds = new Vector4(); //x - minX, y - minY, z - maxX, w - maxY 
        cameraBounds.x = -185.8f; //constant
        cameraBounds.y = AreaMapCameraMove.Instance.areaMapsCamera.orthographicSize;
        cameraBounds.z = (cameraBounds.x + width) - 28.5f;
        cameraBounds.w = height - AreaMapCameraMove.Instance.areaMapsCamera.orthographicSize;
        SpawnCenterGO();
    }
    private void SpawnCenterGO() {
        centerGO = GameObject.Instantiate(centerGOPrefab, transform);
        centerGO.transform.position = new Vector3((cameraBounds.x + cameraBounds.z) * 0.5f, (cameraBounds.y + cameraBounds.w) * 0.5f);
    }
    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        allEdgeTiles = new List<LocationGridTile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTileForArea(area));
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                allTiles.Add(tile);
                if (tile.IsAtEdgeOfWalkableMap()) {
                    allEdgeTiles.Add(tile);
                }
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void LoadGrid(SaveDataAreaInnerTileMap data) {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        allEdgeTiles = new List<LocationGridTile>();

        Dictionary<string, TileBase> tileDB = InteriorMapManager.Instance.GetTileAssetDatabase();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                //groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTileForArea(area));
                LocationGridTile tile = data.map[x][y].Load(groundTilemap, this, tileDB);
                allTiles.Add(tile);
                if (tile.IsAtEdgeOfWalkableMap()) {
                    allEdgeTiles.Add(tile);
                }
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));

        groundTilemap.RefreshAllTiles();
    }
    private void GenerateGrid(TownMapSettings settings) {
        Point determinedSize = GetWidthAndHeightForSettings(settings);
        width = determinedSize.X;
        height = determinedSize.Y;
        GenerateGrid();
    }
    private void SplitMap(bool changeFloorAssets = true) {
        //assign outer and inner areas
        //outer areas should always be 7 tiles from the edge (except for the east side that is 14 tiles from the edge)
        //values are all +1 to accomodate walls that take 1 tile
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                LocationGridTile currTile = map[x, y];
                //determine if tile is outer or inner
                if (x < eastOutsideTiles || x >= width - westOutsideTiles || y < southOutsideTiles || y >= height - northOutsideTiles) {
                    //outside
                    currTile.SetIsInside(false);
                    //GetOutsideFloorTileForArea(area)
                    if (changeFloorAssets) {
                        groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTileForArea(area));
                    }
                    outsideTiles.Add(currTile);
                } else {
                    //inside
                    currTile.SetIsInside(true);
                    //GetOutsideFloorTileForArea(area)
                    if (changeFloorAssets) {
                        groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTileForArea(area));
                    }
                    insideTiles.Add(currTile);
                }
            }
        }
    }
    public void UpdateTilesWorldPosition() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                map[x, y].UpdateWorldLocation();
            }
        }
        SetWorldPosition();
    }
    public void SetWorldPosition() {
        worldPos = transform.position;
    }
    #endregion

    #region Structures
    public TownMapSettings GenerateInnerStructures(out string log) {
        log = "Generating Inner Structures for " + area.name;
        insideTiles = new List<LocationGridTile>();
        outsideTiles = new List<LocationGridTile>();
        if (area.areaType != AREA_TYPE.DUNGEON && area.areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            //if this area is not a dungeon type
            //first get a town center template that has the needed connections for the structures in the area
            List<StructureTemplate> validTownCenters = GetValidTownCenterTemplates(area);
            if (validTownCenters.Count == 0) {
                string error = "There are no valid town center structures for area " + area.name + ". Needed connectors are: ";
                foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
                    error += "\n" + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.Count.ToString();
                }
                throw new System.Exception(error);
            }
            //Once a town center is chosen
            StructureTemplate chosenTownCenter = validTownCenters[Utilities.rng.Next(0, validTownCenters.Count)];
            usedTownCenterTemplateName = chosenTownCenter.name;
            log += "\nChosen town center template is " + usedTownCenterTemplateName;
            //Place that template in the area generation tilemap
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> mainGeneratedSettings = InteriorMapManager.Instance.GenerateTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
            chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);
            Debug.Log(log);
            //once all structures are placed, get the occupied bounds in the area generation tilemap, and use that size to generate the actual grid for this map
            return InteriorMapManager.Instance.GetTownMapSettings(mainGeneratedSettings);
        }
        return default(TownMapSettings);
    }
    private void AssignOuterAreas(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
        if (area.areaType != AREA_TYPE.DUNGEON) {
            if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                for (int i = 0; i < inTiles.Count; i++) {
                    LocationGridTile currTile = inTiles[i];
                    if (currTile.structure == null) {
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                    }
                }
                //gate.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
            } else {
                Debug.LogWarning(area.name + " doesn't have a structure for work area");
            }
        }

        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            for (int i = 0; i < outTiles.Count; i++) {
                LocationGridTile currTile = outTiles[i];
                if (currTile.IsAtEdgeOfMap() || currTile.tileType == LocationGridTile.Tile_Type.Wall) {
                    continue; //skip
                }
                if (currTile.structure == null) {
                    //detailsTilemap.SetTile(currTile.localPlace, outsideDetailTile);
                    if (!Utilities.IsInRange(currTile.localPlace.x, 0, 7) && !Utilities.IsInRange(currTile.localPlace.x, width - eastEdge, width)) {
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                    }
                }

            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for wilderness");
        }
    }
    /// <summary>
    /// Generate a dictionary of settings per structure in an area. This is used to determine the size of each map
    /// </summary>
    /// <param name="area">The area to generate settings for</param>
    /// <returns>Dictionary of Point settings per structure</returns>
    private Point GetWidthAndHeightForSettings(TownMapSettings settings) {
        //height is always 32, 18 is reserved for the inside structures (NOT including walls), and the remaining 14 is for the outside part (Top and bottom)
        Point size = new Point();

        int xSize = settings.size.X;
        int ySize = settings.size.Y;

        xSize += eastOutsideTiles + westOutsideTiles;
        ySize += northOutsideTiles + southOutsideTiles;

        size.X = xSize;
        size.Y = ySize;

        return size;
    }
    private void DrawTiles(Tilemap tilemap, TileTemplateData[] data, Vector3Int startPos) {
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
            pos.x += startPos.x;
            pos.y += startPos.y;
            if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                TileBase assetUsed = InteriorMapManager.Instance.GetTileAsset(currData.tileAssetName, true);
                tilemap.SetTile(pos, assetUsed);
                LocationGridTile tile = map[pos.x, pos.y];
                tile.SetLockedState(true);

                if (tilemap == detailsTilemap) {
                    tile.hasDetail = true;
                    tile.SetTileState(LocationGridTile.Tile_State.Occupied);
                } else if (tilemap == groundTilemap) {
                    if (assetUsed.name.Contains("cobble")) {
                        tile.SetGroundType(LocationGridTile.Ground_Type.Cobble);
                    } else if ((assetUsed.name.Contains("Dirt") || assetUsed.name.Contains("dirt")) && (area.coreTile.biomeType == BIOMES.SNOW || area.coreTile.biomeType == BIOMES.TUNDRA)) {
                        tile.SetGroundType(LocationGridTile.Ground_Type.Tundra);
                        tilemap.SetTile(pos, tundraTile);
                    } else if (assetUsed.name.Contains("water")) {
                        tile.SetGroundType(LocationGridTile.Ground_Type.Water);
                    }
                }
            }
            tilemap.SetTransformMatrix(pos, currData.matrix);

        }
    }
    public void PlaceInitialStructures(Area area) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
            if (keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
                for (int i = 0; i < keyValuePair.Value.Count; i++) {
                    LocationStructure structure = keyValuePair.Value[i];
                    List<GameObject> choices = InteriorMapManager.Instance.GetStructurePrefabsForStructure(keyValuePair.Key);
                    GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);
                    BuildingSpot chosenBuildingSpot = GetRandomOpenBuildingSpot();
                    if (chosenBuildingSpot == null) {
                        //NOTE: This should only happen at the start of the loop, because all the spots are closed at startup.
                        chosenBuildingSpot = GetRandomBuildingSpot();
                    }
                    if (chosenBuildingSpot != null) {
                        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(chosenStructurePrefab.name, Vector3.zero, Quaternion.identity, structureParent);
                        structurePrefab.transform.localPosition = chosenBuildingSpot.centeredLocation;
                        LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
                        structureObject.RefreshAllTilemaps();
                        List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
                        for (int j = 0; j < occupiedTiles.Count; j++) {
                            LocationGridTile tile = occupiedTiles[j];
                            tile.SetStructure(structure);
                        }
                        chosenBuildingSpot.SetIsOpen(false);
                        chosenBuildingSpot.SetIsOccupied(true);
                        chosenBuildingSpot.SetAllAdjacentSpotsAsOpen(this);
                        chosenBuildingSpot.CheckIfAdjacentSpotsCanStillBeOccupied(this);

                        structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                        structure.SetStructureObject(structureObject);
                        structureObject.OnStructureObjectPlaced();
                    }
                }
            }
        }
    }
    #endregion

    #region Town Generation
    private List<StructureTemplate> GetValidTownCenterTemplates(Area area) {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        string extension = "Default";
        List<StructureTemplate> choices = InteriorMapManager.Instance.GetStructureTemplates("TOWN CENTER/" + extension);
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemplate = choices[i];
            if (currTemplate.HasEnoughBuildSpotsForArea(area)) {
                valid.Add(currTemplate);
            }
        }

        return valid;
    }
    /// <summary>
    /// Draw the provided town map, grid must already be generated at this point.
    /// </summary>
    /// <param name="settings">The given settings</param>
    private void DrawTownMap(TownMapSettings settings, Vector3Int startPoint) {
        DrawTiles(groundTilemap, settings.groundTiles, startPoint);
        DrawTiles(structureTilemap, settings.structureTiles, startPoint);
        DrawTiles(detailsTilemap, settings.detailTiles, startPoint);
    }
    private void CreateBuildingSpots(TownMapSettings settings, Vector3Int startPoint) {
        buildingSpots = new List<BuildingSpot>();
        List<BuildingSpotData> orderedData = settings.buildSpots.OrderBy(x => x.id).ToList();
        for (int i = 0; i < orderedData.Count; i++) {
            BuildingSpotData currSpotData = orderedData[i];
            Vector3Int pos = new Vector3Int(currSpotData.location.x, currSpotData.location.y, 0);
            pos.x += startPoint.x;
            pos.y += startPoint.y;
            currSpotData.location = pos;
            BuildingSpot actualSpot = new BuildingSpot(currSpotData);
#if UNITY_EDITOR
            GameObject buildSpotGO = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
            BuildingSpotItem spotItem = buildSpotGO.GetComponent<BuildingSpotItem>();
            buildSpotGO.transform.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
            spotItem.SetBuildingSpot(actualSpot);
#endif
            buildingSpots.Add(actualSpot);
            actualSpot.Initialize(this);
        }
    }

    #endregion

    #region Details
    public void GenerateDetails() {
        //Generate details for the outside map
        MapPerlinDetails(outsideTiles.Where(x => 
        x.objHere == null 
        && x.tileType != LocationGridTile.Tile_Type.Wall
        && x.tileType != LocationGridTile.Tile_Type.Gate
        && !x.isLocked
        && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate) 
        && !x.IsAdjacentTo(typeof(MagicCircle))).ToList()); //Make this better!

        if (area.areaType != AREA_TYPE.DUNGEON) {
            if (area.structures.ContainsKey(STRUCTURE_TYPE.WORK_AREA)) {
                //only put details on tiles that
                //  - do not already have details
                //  - is not a road
                //  - does not have an object place there (Point of Interest)
                //  - is not near the gate (so as not to block path going outside)

                //Generate details for inside map (Trees, shrubs, etc.)
                MapPerlinDetails(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).tiles
                    .Where(x => !x.hasDetail && x.tileType != LocationGridTile.Tile_Type.Road
                    && x.objHere == null && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate)
                    && !x.isLocked
                    && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)
                    && x.tileType != LocationGridTile.Tile_Type.Gate).ToList());

                //Generate details for work area (crates, barrels)
                WorkAreaDetails(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).tiles
                    .Where(x => !x.hasDetail && x.tileType != LocationGridTile.Tile_Type.Road
                    && x.objHere == null && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate)
                    && !x.isLocked
                    && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)
                    && x.tileType != LocationGridTile.Tile_Type.Gate).ToList());
            }
        }
        CreateSeamlessEdges();
    }
    private void MapPerlinDetails(List<LocationGridTile> tiles) {
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
        int minX = tiles.Min(t => t.localPlace.x);
        int maxX = tiles.Max(t => t.localPlace.x);
        int minY = tiles.Min(t => t.localPlace.y);
        int maxY = tiles.Max(t => t.localPlace.y);

        int width = maxX - minX;
        int height = maxY - minY;

        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            float xCoord = (float)currTile.localPlace.x / width * 11f + offsetX;
            float yCoord = (float)currTile.localPlace.y / height * 11f + offsetY;

            float xCoordDetail = (float)currTile.localPlace.x / width * 8f + offsetX;
            float yCoordDetail = (float)currTile.localPlace.y / height * 8f + offsetY;

            float sample = Mathf.PerlinNoise(xCoord, yCoord);
            float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);
            //ground
            if (area.coreTile.biomeType == BIOMES.SNOW || area.coreTile.biomeType == BIOMES.TUNDRA) {
                if (sample < 0.5f) {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Snow);
                    groundTilemap.SetTile(currTile.localPlace, snowTile);
                } else if (sample >= 0.5f && sample < 0.8f) {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Stone);
                    groundTilemap.SetTile(currTile.localPlace, stoneTile);
                } else {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Snow_Dirt);
                    groundTilemap.SetTile(currTile.localPlace, snowDirt);
                }
            } else {
                if (sample < 0.5f) {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Grass);
                    groundTilemap.SetTile(currTile.localPlace, grassTile);
                } else if (sample >= 0.5f && sample < 0.8f) {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Soil);
                    groundTilemap.SetTile(currTile.localPlace, soilTile);
                } else {
                    currTile.SetGroundType(LocationGridTile.Ground_Type.Stone);
                    groundTilemap.SetTile(currTile.localPlace, stoneTile);
                }
               
            }
            
            //trees and shrubs
            if (!currTile.hasDetail) {
                if (sampleDetail < 0.5f) {
                    if (currTile.groundType == LocationGridTile.Ground_Type.Grass || currTile.groundType == LocationGridTile.Ground_Type.Snow) {
                        List<LocationGridTile> overlappedTiles = GetTiles(new Point(2, 2), currTile, tiles);
                        int invalidOverlap = overlappedTiles.Where(t => t.hasDetail || !tiles.Contains(t) || t.objHere != null).Count();
                        if (currTile.IsAdjacentToPasssableTiles(3) && !currTile.IsAtEdgeOfMap() 
                            && !currTile.HasNeighborAtEdgeOfMap() && invalidOverlap == 0 
                            && overlappedTiles.Count == 4 && Random.Range(0, 100) < 5) {
                            //big tree
                            for (int j = 0; j < overlappedTiles.Count; j++) {
                                LocationGridTile ovTile = overlappedTiles[j];
                                ovTile.hasDetail = true;
                                detailsTilemap.SetTile(ovTile.localPlace, null);
                                ovTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                //ovTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                            }
                            detailsTilemap.SetTile(currTile.localPlace, GetBigTreeTile(area));
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                            //currTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                        } else {
                            if (Random.Range(0, 100) < 50) {
                                //shrubs
                                if (area.coreTile.biomeType != BIOMES.SNOW && area.coreTile.biomeType != BIOMES.TUNDRA) {
                                    currTile.hasDetail = true;
                                    detailsTilemap.SetTile(currTile.localPlace, shrubTile);
                                    currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                                    Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                                    detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                                    detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                                }
                            } else {
                                //Crates, Barrels, Ore, Stone and Tree tiles should be impassable. They should all be placed in spots adjacent to at least three passable tiles.
                                if (currTile.IsAdjacentToPasssableTiles(3) && !currTile.WillMakeNeighboursPassableTileInvalid(3)) {
                                    //normal tree
                                    currTile.hasDetail = true;
                                    detailsTilemap.SetTile(currTile.localPlace, GetTreeTile(area));
                                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                    Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                                    detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                                    detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                                    if (currTile.structure != null) {
                                        currTile.structure.AddPOI(new TreeObject(currTile.structure), currTile);
                                    }
                                }
                            }
                        }
                    }
                } else {
                    currTile.hasDetail = false;
                    detailsTilemap.SetTile(currTile.localPlace, null);
                }
                //groundTilemap.SetColor(currTile.localPlace, new Color(sample, sample, sample));
            }
        }

        //flower, rock and garbage
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (!currTile.hasDetail) {
                if (Random.Range(0, 100) < 3) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, GetFlowerTile(area));
                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                } else if (Random.Range(0, 100) < 4) {
                    //Crates, Barrels, Ore, Stone and Tree tiles should be impassable. They should all be placed in spots adjacent to at least three passable tiles.
                    if (currTile.IsAdjacentToPasssableTiles(3)) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, rockTile);
                        currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        //currTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                    }
                } else if (Random.Range(0, 100) < 3) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, GetGarbTile(area));
                }
                Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
            }
        }
    }
    /// <summary>
    /// Generate details for the work area (Crates, Barrels, etc.)
    /// </summary>
    /// <param name="insideTiles">Tiles included in the work area</param>
    private void WorkAreaDetails(List<LocationGridTile> insideTiles) {
        //5% of tiles that are adjacent to thin and thick walls should have crates or barrels
        List<LocationGridTile> tilesForBarrels = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.tileType != LocationGridTile.Tile_Type.Road && currTile.IsAdjacentToWall()) {
                tilesForBarrels.Add(currTile);
            }
        }

        for (int i = 0; i < tilesForBarrels.Count; i++) {
            LocationGridTile currTile = tilesForBarrels[i];
            if (Random.Range(0, 100) < 5) {
                currTile.hasDetail = true;
                detailsTilemap.SetTile(currTile.localPlace, crateBarrelTile);
                currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                //Crates, Barrels, Ore, Stone and Tree tiles should be impassable. They should all be placed in spots adjacent to at least three passable tiles.
                //currTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
            }
        }

        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.tileType != LocationGridTile.Tile_Type.Road && !currTile.hasDetail && currTile.structure.structureType.IsOpenSpace() && Random.Range(0, 100) < 3) {
                //3% of tiles should have random garbage
                currTile.hasDetail = true;
                detailsTilemap.SetTile(currTile.localPlace, randomGarbTile);
            }
        }
    }
    #endregion

    #region Movement & Mouse Interaction
    public void Update() {
        if (UIManager.Instance.characterInfoUI.isShowing 
            && UIManager.Instance.characterInfoUI.activeCharacter.specificLocation == this.area
            && !UIManager.Instance.characterInfoUI.activeCharacter.isDead
            //&& UIManager.Instance.characterInfoUI.activeCharacter.isWaitingForInteraction <= 0
            && UIManager.Instance.characterInfoUI.activeCharacter.marker != null
            && UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.hasPath
            && (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState == null 
            || (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.PATROL 
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL_OUTSIDE
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))) {

            if (UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.currentPath != null
                && UIManager.Instance.characterInfoUI.activeCharacter.currentParty.icon.isTravelling) {
                //ShowPath(UIManager.Instance.characterInfoUI.activeCharacter.marker.currentPath);
                ShowPath(UIManager.Instance.characterInfoUI.activeCharacter);
                //UIManager.Instance.characterInfoUI.activeCharacter.marker.HighlightHostilesInRange();
            } else {
                HidePath();
            }
        } else {
            HidePath();
        }
    }
    #endregion

    #region Points of Interest
    public void PlaceObject(IPointOfInterest obj, LocationGridTile tile, bool placeAsset = true) {
        switch (obj.poiType) {
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                break;
            default:
                tile.SetObjectHere(obj);
                break;
        }
    }
    public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
        tile.RemoveObjectHere(removedBy);
    }
    private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
        if (character.marker.gameObject.transform.parent != objectsParent) {
            //This means that the character travelled to a different area
            character.marker.gameObject.transform.SetParent(objectsParent);
            character.marker.gameObject.transform.localPosition = tile.centeredLocalLocation;
            character.marker.UpdatePosition();
        }

        if (!character.marker.gameObject.activeSelf) {
            character.marker.gameObject.SetActive(true);
        }
    }
    public void OnCharacterMovedTo(Character character, LocationGridTile to, LocationGridTile from) {
        if (from == null) { 
            //from is null (Usually happens on start up, should not happen otherwise)
            to.AddCharacterHere(character);
            to.structure.AddCharacterAtLocation(character);
        } else {
            if (to.structure == null) {
                return; //quick fix for when the character is pushed to a tile with no structure
            }
            from.RemoveCharacterHere(character);
            to.AddCharacterHere(character);
            if (from.structure != to.structure) {
                if (from.structure != null) {
                    from.structure.RemoveCharacterAtLocation(character);
                }
                if (to.structure != null) {
                    to.structure.AddCharacterAtLocation(character);
                } else {
                    throw new System.Exception(character.name + " is going to tile " + to.ToString() + " which does not have a structure!");
                }
                
            }
        }
        
    }
    #endregion

    #region Seamless Edges
    private void CreateSeamlessEdges() {
        for (int i = 0; i < allTiles.Count; i++) {
            LocationGridTile tile = allTiles[i];
            if (tile.structure != null && !tile.structure.structureType.IsOpenSpace()) { continue; } //skip non open space structure tiles.
            Dictionary<TileNeighbourDirection, LocationGridTile> neighbours;
            if (tile.HasCardinalNeighbourOfDifferentGroundType(out neighbours)) {
                //grass should be higher than dirt
                //dirt should be higher than cobble
                //cobble should be higher than grass
                foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                    LocationGridTile currNeighbour = keyValuePair.Value;
                    if (currNeighbour.structure != null && !currNeighbour.structure.structureType.IsOpenSpace()) { continue; } //skip non open space structure tiles.
                    bool createEdge = false;
                    //if (tile.groundType == currNeighbour.groundType) {
                    //    createEdge = true;
                    //} else 
                    if (tile.groundType != LocationGridTile.Ground_Type.Water && currNeighbour.groundType == LocationGridTile.Ground_Type.Water) {
                        createEdge = true;
                    } else if (tile.groundType == LocationGridTile.Ground_Type.Snow) {
                        createEdge = true;
                    } else if (tile.groundType == LocationGridTile.Ground_Type.Cobble && currNeighbour.groundType != LocationGridTile.Ground_Type.Snow) {
                        createEdge = true;
                    } else if ((tile.groundType == LocationGridTile.Ground_Type.Tundra || tile.groundType == LocationGridTile.Ground_Type.Snow_Dirt) && 
                        (currNeighbour.groundType == LocationGridTile.Ground_Type.Stone || currNeighbour.groundType == LocationGridTile.Ground_Type.Soil)) {
                        createEdge = true;
                    } else if (tile.groundType == LocationGridTile.Ground_Type.Grass && currNeighbour.groundType == LocationGridTile.Ground_Type.Soil) {
                        createEdge = true;
                    } else if (tile.groundType == LocationGridTile.Ground_Type.Soil && currNeighbour.groundType == LocationGridTile.Ground_Type.Stone) {
                        createEdge = true;
                    } else if (tile.groundType == LocationGridTile.Ground_Type.Stone && currNeighbour.groundType == LocationGridTile.Ground_Type.Grass) {
                        createEdge = true;
                    }
                    if (createEdge) {
                        Tilemap mapToUse;
                        switch (keyValuePair.Key) {
                            case TileNeighbourDirection.North:
                                mapToUse = northEdgeTilemap;
                                break;
                            case TileNeighbourDirection.South:
                                mapToUse = southEdgeTilemap;
                                break;
                            case TileNeighbourDirection.West:
                                mapToUse = westEdgeTilemap;
                                break;
                            case TileNeighbourDirection.East:
                                mapToUse = eastEdgeTilemap;
                                break;
                            default:
                                mapToUse = null;
                                break;
                        }
                        mapToUse.SetTile(tile.localPlace, edgeAssets[tile.groundType][(int)keyValuePair.Key]);
                    }
                }
            }
        }
    }
    #endregion

    #region Utilities
    public void ClearAllTilemaps() {
        Tilemap[] maps = GetComponentsInChildren<Tilemap>();
        for (int i = 0; i < maps.Length; i++) {
            maps[i].ClearAllTiles();
        }
    }
    private List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile, List<LocationGridTile> mustBeIn = null) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
            for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                if (x > map.GetUpperBound(0) || y > map.GetUpperBound(1)) {
                    continue; //skip
                }
                if (mustBeIn != null && !mustBeIn.Contains(map[x, y])) {
                    continue; //skip
                }
                tiles.Add(map[x, y]);
            }
        }
        return tiles;
    }
    public TileBase GetOutsideFloorTileForArea(Area area) {
        switch (area.coreTile.biomeType) {
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                return snowOutsideTile;
            default:
                return outsideTile;
        }
    }
    public TileBase GetBigTreeTile(Area area) {
        switch (area.coreTile.biomeType) {
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                return snowBigTreeTile;
            default:
                return bigTreeTile;
        }
    }
    public TileBase GetTreeTile(Area area) {
        switch (area.coreTile.biomeType) {
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                return snowTreeTile;
            default:
                return treeTile;
        }
    }
    public TileBase GetFlowerTile(Area area) {
        switch (area.coreTile.biomeType) {
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                return snowFlowerTile;
            default:
                return flowerTile;
        }
    }
    public TileBase GetGarbTile(Area area) {
        switch (area.coreTile.biomeType) {
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                return snowGarbTile;
            default:
                return randomGarbTile;
        }
    }
    public void CleanUp() {
        Utilities.DestroyChildren(objectsParent);
    }
    #endregion

    #region Other
    public void Open() {
        //this.gameObject.SetActive(true);
        //SwitchFromEstimatedMovementToPathfinding();
    }
    public void Close() {
        //this.gameObject.SetActive(false);
        ////if (UIManager.Instance.areaInfoUI.isShowing) {
        ////    UIManager.Instance.areaInfoUI.ToggleMapMenu(false);
        ////}
        //isHovering = false;
        //SwitchFromPathfindingToEstimatedMovement();
    }
    public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }
        int xLimitLower = x - radiusLimit;
        int xLimitUpper = x + radiusLimit;
        int yLimitLower = y - radiusLimit;
        int yLimitUpper = y + radiusLimit;


        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if(dx == x && dy == y) {
                        continue;
                    }
                    if(radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                        continue;
                    }
                    LocationGridTile result = map[dx, dy];
                    if(result.structure == null) { continue; } //do not include tiles with no structures
                    if(!includeTilesInDifferentStructure && result.structure != centerTile.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    public List<LocationGridTile> GetUnoccupiedTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }

        int xLimitLower = x - radiusLimit;
        int xLimitUpper = x + radiusLimit;
        int yLimitLower = y - radiusLimit;
        int yLimitUpper = y + radiusLimit;

        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                        continue;
                    }
                    LocationGridTile result = map[dx, dy];
                    if ((!includeTilesInDifferentStructure && result.structure != centerTile.structure) || result.isOccupied || result.charactersHere.Count > 0 || result.tileAccess == LocationGridTile.Tile_Access.Impassable) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    public void SetHoveredCharacter(Character character) {
        hoveredCharacter = character;
    }
    #endregion

    #region For Testing
    public void ShowPath(List<Vector3> points) {
        pathLineRenderer.gameObject.SetActive(true);
        pathLineRenderer.positionCount = points.Count;
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++) {
            positions[i] = points[i];
        }
        pathLineRenderer.SetPositions(positions);
    }
    public void ShowPath(Character character) {
        List<Vector3> points = new List<Vector3>(character.marker.pathfindingAI.currentPath.vectorPath);
        int indexAt = 0; //the index that the character is at.
        float nearestDistance = 9999f;
        //refine the current path to remove points that the character has passed.
        //to do that, get the point in the list that the character is nearest to, then remove all other points before that point
        for (int i = 0; i < points.Count; i++) {
            Vector3 currPoint = points[i];
            float distance = Vector3.Distance(character.marker.transform.position, currPoint);
            if (distance < nearestDistance) {
                indexAt = i;
                nearestDistance = distance;
            }
        }
        //Debug.Log(character.name + " is at index " + indexAt.ToString() + ". current path length is " + points.Count);
        if (points.Count > 0) {
            for (int i = 0; i <= indexAt; i++) {
                points.RemoveAt(0);
            }
        }
        //points.Insert(0, character.marker.transform.position);
        //Debug.Log(character.name + " new path length is " + points.Count);
        ShowPath(points);
    }
    public void HidePath() {
        pathLineRenderer.gameObject.SetActive(false);
    }
    #endregion

    #region Building Spots
    private BuildingSpot GetRandomOpenBuildingSpot() {
        List<BuildingSpot> choices = new List<BuildingSpot>(buildingSpots.Where(x => x.isOpen));
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);
        }
        return null;
    }
    private BuildingSpot GetRandomBuildingSpot() {
        return Utilities.GetRandomElement(buildingSpots);
    }
    public BuildingSpot[] GetBuildingSpotsWithID(params int[] ids) {
        BuildingSpot[] spots = new BuildingSpot[ids.Length];
        int index = 0;
        for (int i = 0; i < buildingSpots.Count; i++) {
            BuildingSpot currSpot = buildingSpots[i];
            if (ids.Contains(currSpot.id)) {
                spots[index] = currSpot;
                index++;
            }
        }
        return spots;
    }
    #endregion

    #region Burning Source
    public void AddActiveBurningSource(BurningSource bs) {
        if (!activeBurningSources.Contains(bs)) {
            activeBurningSources.Add(bs);
        }
    }
    public void RemoveActiveBurningSources(BurningSource bs) {
        activeBurningSources.Remove(bs);
    }
    public void LoadBurningSources(List<SaveDataBurningSource> sources) {
        for (int i = 0; i < sources.Count; i++) {
            SaveDataBurningSource data = sources[i];
            BurningSource bs = new BurningSource(area, data);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataAreaInnerTileMap {
    public int width;
    public int height;
    public int areaID;
    public SaveDataLocationGridTile[][] map;
    public string usedTownCenterTemplateName;
    public TownMapSettings generatedTownMapSettings;
    public List<SaveDataBurningSource> burningSources;

    public void Save(AreaInnerTileMap innerMap) {
        width = innerMap.width;
        height = innerMap.height;
        areaID = innerMap.area.id;
        usedTownCenterTemplateName = innerMap.usedTownCenterTemplateName;
        generatedTownMapSettings = innerMap.generatedTownMapSettings;

        burningSources = new List<SaveDataBurningSource>();
        for (int i = 0; i < innerMap.activeBurningSources.Count; i++) {
            BurningSource bs = innerMap.activeBurningSources[i];
            SaveDataBurningSource source = new SaveDataBurningSource();
            source.Save(bs);
            burningSources.Add(source);
        }

        map = new SaveDataLocationGridTile[width][];
        for (int x = 0; x < innerMap.map.GetLength(0); x++) {
            map[x] = new SaveDataLocationGridTile[innerMap.map.GetLength(1)];
            for (int y = 0; y < innerMap.map.GetLength(1); y++) {
                SaveDataLocationGridTile data = new SaveDataLocationGridTile();
                data.Save(innerMap.map[x, y]);
                map[x][y] = data;
            }
        }
    }
    public void Load(AreaInnerTileMap innerMap) {
        innerMap.width = width;
        innerMap.height = height;
        innerMap.Initialize(LandmarkManager.Instance.GetAreaByID(areaID));
        innerMap.LoadMap(this);
    }

    public void LoadTileTraits() {
        for (int x = 0; x < map.Length; x++) {
            for (int y = 0; y < map[x].Length; y++) {
                map[x][y].LoadTraits();
            }
        }
        //for (int i = 0; i < map.Count; i++) {
        //    map[i]LoadTraits();
        //}
    }
    public void LoadObjectHereOfTiles() {
        for (int x = 0; x < map.Length; x++) {
            for (int y = 0; y < map[x].Length; y++) {
                map[x][y].LoadObjectHere();
            }
        }
    }
}