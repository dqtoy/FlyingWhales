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

    private LocationGridTile westGate;
    private LocationGridTile eastGate;

    public Grid grid;
    public Canvas worldUICanvas;
    [SerializeField] private Canvas canvas;

    [Header("Tile Maps")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public Tilemap detailsTilemap;
    public Tilemap structureTilemap;
    public Tilemap objectsTilemap;
    public Tilemap roadTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase snowOutsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private ItemTileBaseDictionary itemTiles;
    [SerializeField] private TileObjectTileBaseDictionary tileObjectTiles;

    [Header("Special Cases")]
    //bed
    public TileBase bed2SleepingVariant;
    public TileBase bed1SleepingVariant;   

    [Header("Structure Tiles")]
    [SerializeField] private TileBase leftWall;
    [SerializeField] private TileBase rightWall;
    [SerializeField] private TileBase topWall;
    [SerializeField] private TileBase bottomWall;
    [SerializeField] private TileBase topLeftCornerWall;
    [SerializeField] private TileBase botLeftCornerWall;
    [SerializeField] private TileBase topRightCornerWall;
    [SerializeField] private TileBase botRightCornerWall;
    [SerializeField] private TileBase topDoor;
    [SerializeField] private TileBase botDoor;
    [SerializeField] private TileBase leftDoor;
    [SerializeField] private TileBase rightDoor;

    [Header("Dungeon Tiles")]
    [SerializeField] private TileBase dungeonWallTile;
    [SerializeField] private TileBase dungeonFloorTile;

    [Header("Oustide Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase soilTile;
    [SerializeField] private TileBase stoneTile;

    [Header("Snow Outside Tiles")]
    [SerializeField] private TileBase snowTile;
    [SerializeField] private TileBase tundraTile;
    [SerializeField] private TileBase snowStoneFloorTile;
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

    [Header("For Testing")]
    [SerializeField] private LineRenderer pathLineRenderer;
    [SerializeField] private int radius;
    [SerializeField] private int radiusLimit;
    [SerializeField] private int x;
    [SerializeField] private int y;

    [Header("Perlin Noise")]
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    [Header("Seamless Edges")]
    public Tilemap northEdgeTilemap;
    public Tilemap southEdgeTilemap;
    public Tilemap westEdgeTilemap;
    public Tilemap eastEdgeTilemap;
    [SerializeField] private SeamlessEdgeAssetsDictionary edgeAssets; //0-north, 1-south, 2-west, 3-east

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
    }
    public void DrawMap(TownMapSettings generatedSettings) {
        generatedTownMapSettings = generatedSettings;
        GenerateGrid(generatedSettings);
        SplitMap();
        Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
        DrawTownMap(generatedSettings, startPoint);
        //once generated, just copy the generated structures to the actual map.
        if (area.name == "Gloomhollow") {
            LocationStructure exploreArea = area.GetRandomStructureOfType(STRUCTURE_TYPE.EXPLORE_AREA);
            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                TileBase structureAsset = structureTilemap.GetTile(currTile.localPlace);
                if (structureAsset == null || !structureAsset.name.Contains("wall")) {
                    currTile.SetStructure(exploreArea);
                    currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                }
            }
        } else {
            PlaceStructures(generatedSettings, startPoint);
        }
        AssignOuterAreas(insideTiles, outsideTiles);
    }
    public void LoadMap(SaveDataAreaInnerTileMap data) {
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();

        LoadGrid(data);
        SplitMap(false);
        //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
        //DrawTownMap(data.generatedTownMapSettings, startPoint);
        //TODO: DrawTownMap
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
        CreateSeamlessEdges();
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
    }
    private void GenerateGrid(Dictionary<LocationStructure, LocationStructureSetting> settings) {
        Point determinedSize = GetWidthAndHeightForSettings(settings);
        width = determinedSize.X;
        height = determinedSize.Y;
        GenerateGrid();
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
        if ((area.areaType == AREA_TYPE.DUNGEON && area.name == "Gloomhollow") 
            || (area.areaType != AREA_TYPE.DUNGEON && area.areaType != AREA_TYPE.DEMONIC_INTRUSION)) {
            //if this area is not a dungeon type
            //InteriorMapManager.Instance.CleanupForTownGeneration();
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
            //InteriorMapManager.Instance.DrawTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> mainGeneratedSettings = InteriorMapManager.Instance.GenerateTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
            //DrawTiles(InteriorMapManager.Instance.agGroundTilemap, chosenTownCenter.groundTiles, Vector3Int.zero);
            chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);
            //then iterate through all the structures in this area, making sure that the chosen template for the structure can connect to the town center
            log += "\nGenerating Structures... ";
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
                log += "\nStructure Type is: " + keyValuePair.Key.ToString();
                if (area.name == "Gloomhollow") {
                    if (!keyValuePair.Key.ShouldBeGeneratedFromTemplate() && keyValuePair.Key != STRUCTURE_TYPE.EXPLORE_AREA) {
                        //allow explore areas to be generated in gloomhollow
                        continue; //skip
                    }
                } else {
                    if (!keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
                        log += "\nStructure type should not be generated from template. Skipping...";
                        continue; //skip
                    }
                }

                int structuresToCreate = keyValuePair.Value.Count;
                if (area.name == "Gloomhollow") {
                    structuresToCreate = 5; //hardcoded to 5
                }
                log += "\nStructures to create are: " + structuresToCreate.ToString();
                for (int i = 0; i < structuresToCreate; i++) {
                    List<StructureTemplate> templates = InteriorMapManager.Instance.GetStructureTemplates(keyValuePair.Key); //placed this inside loop so that instance of template is unique per iteration
                    List<StructureTemplate> choices = GetTemplatesThatCanConnectTo(chosenTownCenter, templates);
                    if (choices.Count == 0) {
                        //NOTE: Show a warning log when there are no valid structure templates for the current structure
                        throw new System.Exception("There are no valid " + keyValuePair.Key.ToString() + " templates to connect to town center in area " + area.name);
                    }
                    StructureTemplate chosenTemplate = choices[Utilities.rng.Next(0, choices.Count)];
                    log += "\n\tChosen Template is: " + chosenTemplate.name;

                    StructureConnector townCenterConnector;
                    StructureConnector chosenTemplateConnector = chosenTemplate.GetValidConnectorTo(chosenTownCenter, out townCenterConnector);

                    Vector3Int shiftTemplateBy = InteriorMapManager.Instance.GetMoveUnitsOfTemplateGivenConnections(chosenTemplate, chosenTemplateConnector, townCenterConnector);
                    townCenterConnector.SetIsOpen(false);
                    chosenTemplateConnector.SetIsOpen(false);
                    string structureLog;
                    Dictionary<int, Dictionary<int, LocationGridTileSettings>> generatedStructure = InteriorMapManager.Instance.GenerateStructureTemplateForGeneration(chosenTemplate, shiftTemplateBy, keyValuePair.Key, out structureLog);
                    log += "\n\t\t" + structureLog;
                    InteriorMapManager.Instance.MergeSettings(generatedStructure, ref mainGeneratedSettings);
                    //InteriorMapManager.Instance.DrawStructureTemplateForGeneration(chosenTemplate, shiftTemplateBy, keyValuePair.Key);
                }
            }
            //once all structures are placed, get the occupied bounds in the area generation tilemap, and use that size to generate the actual grid for this map
            return InteriorMapManager.Instance.GetTownMapSettings(mainGeneratedSettings);
            //GenerateGrid(generatedSettings);
            //SplitMap();
            //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
            //DrawTownMap(generatedSettings, startPoint);
            ////once generated, just copy the generated structures to the actual map.
            //if (area.name == "Gloomhollow") {
            //    LocationStructure exploreArea = area.GetRandomStructureOfType(STRUCTURE_TYPE.EXPLORE_AREA);
            //    for (int i = 0; i < insideTiles.Count; i++) {
            //        LocationGridTile currTile = insideTiles[i];
            //        TileBase structureAsset = structureTilemap.GetTile(currTile.localPlace);
            //        if (structureAsset == null || !structureAsset.name.Contains("wall")) {
            //            currTile.SetStructure(exploreArea);
            //            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
            //        }
            //    }
            //} else {
            //    PlaceStructures(generatedSettings, startPoint);
            //}
            //AssignOuterAreas(insideTiles, outsideTiles);
            //else use the old structure generation
        }
        //else {
        //    OldStructureGeneration();
        //}
        return default(TownMapSettings);
    }
    private void OldStructureGeneration() {
        Dictionary<LocationStructure, LocationStructureSetting> settings = GenerateStructureSettings(area);
        Point mapSize = GetWidthAndHeightForSettings(settings);

        Debug.Log("Generated Map size for " + area.name + " is: " + mapSize.X + ", " + mapSize.Y);

        GenerateGrid(settings);
        SplitMap();

        if (area.areaType == AREA_TYPE.DUNGEON) {
            DrawCaveWalls(insideTiles, outsideTiles);
        }
        //else {
        //    ConstructInsideMapWalls(insideTiles, outsideTiles);
        //}

        PlaceStructures(settings, insideTiles);
        DrawStructureWalls();

        GenerateGates(outsideTiles);
        AssignOuterAreas(insideTiles, outsideTiles);
        if (area.areaType != AREA_TYPE.DEMONIC_INTRUSION && area.areaType != AREA_TYPE.DUNGEON) {
            GenerateEntrances(insideTiles);
            GenerateRoads();
        }
        if (area.HasStructure(STRUCTURE_TYPE.EXPLORE_AREA)) {
            ConnectExploreAreas();
            FillInCaveWalls(insideTiles);
        }
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
    private void GenerateRoads() {
        //main road
        //make a path connecting the east and west gate
        List<LocationGridTile> mainRoad = PathGenerator.Instance.GetPath(westGate, eastGate, GRID_PATHFINDING_MODE.MAIN_ROAD_GEN, true);
        if (mainRoad != null) {
            for (int i = 0; i < mainRoad.Count; i++) {
                LocationGridTile currTile = mainRoad[i];
                if (currTile.tileType == LocationGridTile.Tile_Type.Road) {
                    continue; //skip
                }
                if (currTile.tileType == LocationGridTile.Tile_Type.Structure) {
                    continue; //skip
                }
                //if (currTile.tileType == LocationGridTile.Tile_Type.Gate) {
                //    continue; //skip
                //}
                if (currTile.tileType == LocationGridTile.Tile_Type.Structure_Entrance) {
                    continue; //skip
                }

                if (area.areaType == AREA_TYPE.DUNGEON) {
                    wallTilemap.SetTile(currTile.localPlace, null);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                } else {
                    //groundTilemap.SetTile(currTile.localPlace, insideTile);
                    //roadTilemap.SetTile(currTile.localPlace, roadTile);
                    roadTilemap.SetTile(currTile.localPlace, insideTile);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                }

                if (currTile.tileType != LocationGridTile.Tile_Type.Gate) {
                    currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                    currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                }
            }
        }

        //connect building entrances to main road
        List<LocationStructure> buildings = area.GetStructuresAtLocation(true).OrderBy(x => x.GetNearestDistanceTo(westGate)).ToList();
        for (int i = 0; i < buildings.Count; i++) {
            LocationStructure currBuilding = buildings[i];
            if (currBuilding.structureType == STRUCTURE_TYPE.WORK_AREA) {
                continue; //skip work area
            }
            if (currBuilding.HasRoadTo(westGate)) {
                continue; //skip, no need to create any road
            }

            LocationGridTile nearestMainRoadTile = mainRoad.OrderBy(x => Vector2.Distance(x.localLocation, currBuilding.entranceTile.localLocation)).First();

            List<LocationGridTile> path = PathGenerator.Instance.GetPath(nearestMainRoadTile, currBuilding.entranceTile, GRID_PATHFINDING_MODE.NORMAL, true);
            if (path != null) {
                path.Reverse();
                for (int j = 0; j < path.Count; j++) {
                    LocationGridTile currTile = path[j];
                    if (currTile.tileType == LocationGridTile.Tile_Type.Road) {
                        continue; //skip
                    }
                    if (currTile.tileType == LocationGridTile.Tile_Type.Structure) {
                        continue; //skip
                    }
                    if (currTile.tileType == LocationGridTile.Tile_Type.Gate) {
                        continue; //skip
                    }
                    if (currTile.tileType == LocationGridTile.Tile_Type.Structure_Entrance) {
                        continue; //skip
                    }

                    //groundTilemap.SetTile(currTile.localPlace, insideTile);
                    //roadTilemap.SetTile(currTile.localPlace, roadTile);
                    roadTilemap.SetTile(currTile.localPlace, insideTile);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                    if (currTile.tileType != LocationGridTile.Tile_Type.Gate) {
                        currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                        currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                    }
                }
            }
        }
    }
    private void DrawStructureWalls() {
        List<LocationStructure> structuresWithWalls = new List<LocationStructure>();
        if (area.HasStructure(STRUCTURE_TYPE.DWELLING)) {
            structuresWithWalls.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.DWELLING));
        }
        if (area.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            structuresWithWalls.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.WAREHOUSE));
        }
        if (area.HasStructure(STRUCTURE_TYPE.PRISON)) {
            structuresWithWalls.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.PRISON));
        }
        if (area.HasStructure(STRUCTURE_TYPE.INN)) {
            structuresWithWalls.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.INN));
        }

        for (int i = 0; i < structuresWithWalls.Count; i++) {
            LocationStructure currStructure = structuresWithWalls[i];
            if (currStructure.isFromTemplate) {
                //if this structure is from a template, do not draw it's walls
                continue; //skip
            }
            List<LocationGridTile> outerTiles = currStructure.GetOuterTiles();
            for (int j = 0; j < outerTiles.Count; j++) {
                LocationGridTile currTile = outerTiles[j];
                List<TileNeighbourDirection> sameStructNeighbours = currTile.GetSameStructureNeighbourDirections();
                if (!sameStructNeighbours.Contains(TileNeighbourDirection.West)) {
                    if (!sameStructNeighbours.Contains(TileNeighbourDirection.North)) {
                        structureTilemap.SetTile(currTile.localPlace, topLeftCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                        structureTilemap.SetTile(currTile.localPlace, botLeftCornerWall);
                    } else {
                        structureTilemap.SetTile(currTile.localPlace, leftWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.East)) {
                    if (!sameStructNeighbours.Contains(TileNeighbourDirection.North)) {
                        structureTilemap.SetTile(currTile.localPlace, topRightCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                        structureTilemap.SetTile(currTile.localPlace, botRightCornerWall);
                    } else {
                        structureTilemap.SetTile(currTile.localPlace, rightWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                    structureTilemap.SetTile(currTile.localPlace, bottomWall);
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.North)) {
                    structureTilemap.SetTile(currTile.localPlace, topWall);
                }
            }
        }
    }
    private void GenerateEntrances(List<LocationGridTile> inTiles) {
        List<LocationStructure> structuresWithEntrances = new List<LocationStructure>();
        if (area.HasStructure(STRUCTURE_TYPE.DWELLING)) {
            structuresWithEntrances.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.DWELLING));
        }
        if (area.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            structuresWithEntrances.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.WAREHOUSE));
        }
        if (area.HasStructure(STRUCTURE_TYPE.PRISON)) {
            structuresWithEntrances.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.PRISON));
        }
        if (area.HasStructure(STRUCTURE_TYPE.INN)) {
            structuresWithEntrances.AddRange(area.GetStructuresOfType(STRUCTURE_TYPE.INN));
        }

        //int minX = inTiles.Min(x => x.localPlace.x);
        //int maxX = inTiles.Max(x => x.localPlace.x);
        int minY = inTiles.Min(x => x.localPlace.y);
        int maxY = inTiles.Max(x => x.localPlace.y);

        int midY = (minY + maxY) / 2;

        for (int i = 0; i < structuresWithEntrances.Count; i++) {
            LocationStructure currStructure = structuresWithEntrances[i];
            List<LocationGridTile> validEntrances = currStructure.GetValidEntranceTiles(midY);
            if (validEntrances.Count > 0) {
                LocationGridTile chosenEntrance = validEntrances[UnityEngine.Random.Range(0, validEntrances.Count)];
                currStructure.SetEntranceTile(chosenEntrance);
                chosenEntrance.SetTileType(LocationGridTile.Tile_Type.Structure_Entrance);
                TileNeighbourDirection doorToUse = chosenEntrance.GetCardinalDirectionOfStructureType(STRUCTURE_TYPE.WORK_AREA);
                switch (doorToUse) {
                    case TileNeighbourDirection.North:
                        structureTilemap.SetTile(chosenEntrance.localPlace, topDoor);
                        break;
                    case TileNeighbourDirection.South:
                        structureTilemap.SetTile(chosenEntrance.localPlace, botDoor);
                        break;
                    case TileNeighbourDirection.West:
                        structureTilemap.SetTile(chosenEntrance.localPlace, leftDoor);
                        break;
                    case TileNeighbourDirection.East:
                        structureTilemap.SetTile(chosenEntrance.localPlace, rightDoor);
                        break;
                    default:
                        structureTilemap.SetTile(chosenEntrance.localPlace, topDoor);
                        break;
                }
                
            }
        }
    }
    private void GenerateGates(List<LocationGridTile> outTiles) {
        //string summary = "Generating gates for " + area.name;
        List<LocationGridTile> wallTiles = new List<LocationGridTile>();

        for (int i = 0; i < outTiles.Count; i++) {
            //check for out tiles that have neighbours in the in tile list
            LocationGridTile currOutTile = outTiles[i];
            //if (currOutTile.tileType == LocationGridTile.Tile_Type.Wall) {
            //    wallTiles.Add(currOutTile);
            //}
            for (int j = 0; j < currOutTile.neighbours.Values.Count; j++) {
                if (!outTiles.Contains(currOutTile.neighbours.Values.ElementAt(j))) {
                    wallTiles.Add(currOutTile);
                }
            }
        }

        int minX = wallTiles.Min(x => x.localPlace.x);
        int maxX = wallTiles.Max(x => x.localPlace.x);
        int minY = wallTiles.Min(x => x.localPlace.y);
        int maxY = wallTiles.Max(x => x.localPlace.y);

        int midY = (minY + maxY) / 2;

        minY = midY - 3; //To ensure that both gates are more or less centered
        maxY = midY + 3;

        //summary += "\nMinX: " + minX + ", MaxX: " + maxX + ", MinY: " + minY + ", MaxY: " + maxY + ", MidY: " + midY;

        List<STRUCTURE_TYPE> unallowedNeighbours = new List<STRUCTURE_TYPE>() { STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.INN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };

        List<LocationGridTile> elligibleWestGates = wallTiles
            .Where(x => !x.HasStructureOfTypeHorizontally(unallowedNeighbours, 3) 
            && !x.HasNeighbouringStructureOfType(unallowedNeighbours)
            && x.localPlace.x == minX
            && x.localPlace.y > minY && x.localPlace.y < maxY).ToList();
        List<LocationGridTile> elligibleEastGates = wallTiles
            .Where(x => !x.HasStructureOfTypeHorizontally(unallowedNeighbours, -3)
            && !x.HasNeighbouringStructureOfType(unallowedNeighbours)
            && x.localPlace.x == maxX
            && x.localPlace.y > minY && x.localPlace.y < maxY).ToList();

        LocationGridTile chosenWestGate = elligibleWestGates[Random.Range(0, elligibleWestGates.Count)];
        chosenWestGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        wallTilemap.SetTile(chosenWestGate.localPlace, null);

        westGate = chosenWestGate;
        //summary += "\nWest gate is: " + westGate.ToString();

        LocationGridTile chosenEastGate = elligibleEastGates[Random.Range(0, elligibleEastGates.Count)];
        chosenEastGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        wallTilemap.SetTile(chosenEastGate.localPlace, null);

        eastGate = chosenEastGate;
        //summary += "\nEast gate is: " + eastGate.ToString();

        //Debug.Log(summary);
    }
    /// <summary>
    /// Generate a dictionary of settings per structure in an area. This is used to determine the size of each map
    /// </summary>
    /// <param name="area">The area to generate settings for</param>
    /// <returns>Dictionary of Point settings per structure</returns>
    private Dictionary<LocationStructure, LocationStructureSetting> GenerateStructureSettings(Area area) {
        Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = GetStructureSettings();
        Dictionary<LocationStructure, LocationStructureSetting> generatedSettings = new Dictionary<LocationStructure, LocationStructureSetting>();

        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> orderedStructures = area.GetStructures(true).OrderBy(x => x.Key).ToDictionary(k => k.Key, v => v.Value);
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in orderedStructures) { //generate structure settings for inside structures only
            List<StructureTemplate> templates = InteriorMapManager.Instance.GetStructureTemplates(keyValuePair.Key); //check for templates

            if (templates.Count == 0 && !structureSettings.ContainsKey(keyValuePair.Key)) {
                //if the structure type has no available templates and it has no default settings, skip
                continue;
            }

            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure currStructure = keyValuePair.Value[i];
                if (templates.Count > 0 && keyValuePair.Key != STRUCTURE_TYPE.EXPLORE_AREA) { //ignore explore area templates when not using town map generation
                    //if the structure type has templates, use those
                    StructureTemplate template = templates[Random.Range(0, templates.Count)];
                    generatedSettings.Add(currStructure, new LocationStructureSetting(template));
                } else {
                    List<Point> choices = structureSettings[keyValuePair.Key];
                    Point chosenSetting = choices[Random.Range(0, choices.Count)];
                    generatedSettings.Add(currStructure, new LocationStructureSetting(chosenSetting));
                }
                
            }
        }

        return generatedSettings;
    }
    private Point GetWidthAndHeightForSettings(Dictionary<LocationStructure, LocationStructureSetting> settings) {
        //height is always 32, 18 is reserved for the inside structures (NOT including walls), and the remaining 14 is for the outside part (Top and bottom)
        Point size = new Point();
        size.Y = 32;

        int ySizeForInner = 18;
        int minimumNeededTiles = settings.Values.Sum(x => x.size.Product());
        size.X = minimumNeededTiles / ySizeForInner;
        if (size.X < ySizeForInner) {
            size.X = ySizeForInner;
        }
        if (area.areaType == AREA_TYPE.DUNGEON) {
            size.X += Mathf.FloorToInt(settings.Count * 3f); //add allowance depecnding on number of structures to place
        } else {
            size.X += Mathf.FloorToInt(settings.Count * 4f); //add allowance depecnding on number of structures to place
        }

        //increase size by 7 for each side (west and east) and another 7 for the east side for the part that will be covered by the UI
        size.X += 21;

        return size;
    }
    private Point GetWidthAndHeightForSettings(TownMapSettings settings) {
        //height is always 32, 18 is reserved for the inside structures (NOT including walls), and the remaining 14 is for the outside part (Top and bottom)
        Point size = new Point();

        int xSize = settings.size.X;
        int ySize = settings.size.Y;

        xSize += eastOutsideTiles + westOutsideTiles;
        ySize += northOutsideTiles + southOutsideTiles;

        size.X = xSize;
        size.Y = ySize;

        //size.Y = 32;

        //int ySizeForInner = 18;
        //int minimumNeededTiles = settings.Values.Sum(x => x.size.Product());
        //size.X = minimumNeededTiles / ySizeForInner;
        //if (size.X < ySizeForInner) {
        //    size.X = ySizeForInner;
        //}
        //if (area.areaType == AREA_TYPE.DUNGEON) {
        //    size.X += Mathf.FloorToInt(settings.Count * 3f); //add allowance depecnding on number of structures to place
        //} else {
        //    size.X += Mathf.FloorToInt(settings.Count * 4f); //add allowance depecnding on number of structures to place
        //}

        ////increase size by 7 for each side (west and east) and another 7 for the east side for the part that will be covered by the UI
        //size.X += 21;

        return size;
    }
    private void PlaceStructures(Dictionary<LocationStructure, LocationStructureSetting> settings, List<LocationGridTile> sourceTiles) {
        List<LocationGridTile>  elligibleTiles = new List<LocationGridTile>(sourceTiles);
        if (elligibleTiles.Count == 0) {
            Debug.LogWarning("There were no elligible tiles for structure placement at " + area.name);
            return;
        }
        int leftMostCoordinate = elligibleTiles.Min(t => t.localPlace.x);
        int rightMostCoordinate = elligibleTiles.Max(t => t.localPlace.x);
        int topMostCoordinate = elligibleTiles.Max(t => t.localPlace.y);
        int botMostCoordinate = elligibleTiles.Min(t => t.localPlace.y);
        //structures = structures.OrderBy(x => x.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        foreach (KeyValuePair<LocationStructure, LocationStructureSetting> kvp in settings) {
            LocationStructure currStruct = kvp.Key;
            Point currPoint = kvp.Value.size;

            //get all tiles, that meet the ff requirements:
            // - it's y coordinate is 1 tile above the bottom most coordinate or it's y coordinate plus the size of the structure is equal to the top most coordinate
            // - it's x coordinate is greater than the leftmost coordinate plus 1
            // - it's x coordinate is less than the right most coordinate minus 1
            // - none of it's tiles, given the size of the structure, are unelligible
            List<LocationGridTile> choices = elligibleTiles.Where(
            t => (t.localPlace.y + currPoint.Y == topMostCoordinate || t.localPlace.y == botMostCoordinate + 1)
            && t.localPlace.x > leftMostCoordinate + 1 && t.localPlace.x + currPoint.X < rightMostCoordinate - 1
            && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))
            ).ToList();

            if (choices.Count == 0) {
                Debug.LogWarning("Could not find tile to place " + kvp.Key.ToString());
                continue; //skip
            }

            LocationGridTile chosenStartingTile = choices[Random.Range(0, choices.Count)];
            if (kvp.Value.hasTemplate) {
                //if there is a provided template, draw that instead
                List<LocationGridTile> tilesUsed = DrawStructureTemplate(kvp.Value.template, chosenStartingTile, kvp.Key);
                Utilities.ListRemoveRange(elligibleTiles, tilesUsed);
                currStruct.SetIfFromTemplate(true);
            } else {
                //else, use the default way of drawing structures (just a box)
                List<LocationGridTile> tiles = GetTiles(currPoint, chosenStartingTile);
                for (int j = 0; j < tiles.Count; j++) {
                    LocationGridTile currTile = tiles[j];
                    currTile.SetStructure(currStruct);
                    elligibleTiles.Remove(currTile);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                    List<LocationGridTile> neighbourTiles = new List<LocationGridTile>();
                    switch (kvp.Key.structureType) {
                        case STRUCTURE_TYPE.EXPLORE_AREA:
                            groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            wallTilemap.SetTile(currTile.localPlace, null);
                            //neighbourTiles = GetTilesInRadius(currTile, 2, false, true);
                            //for (int k = 0; k < neighbourTiles.Count; k++) {
                            //    elligibleTiles.Remove(neighbourTiles[k]);
                            //}
                            break;
                        case STRUCTURE_TYPE.INN:
                        case STRUCTURE_TYPE.WAREHOUSE:
                        case STRUCTURE_TYPE.PRISON:
                            groundTilemap.SetTile(currTile.localPlace, floorTile);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            neighbourTiles = GetTilesInRadius(currTile, 1, 0, false, true);
                            for (int k = 0; k < neighbourTiles.Count; k++) {
                                elligibleTiles.Remove(neighbourTiles[k]);
                            }
                            break;
                        default:
                            groundTilemap.SetTile(currTile.localPlace, floorTile);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            //neighbourTiles = GetTilesInRadius(currTile, 1, false, true);
                            //for (int k = 0; k < neighbourTiles.Count; k++) {
                            //    elligibleTiles.Remove(neighbourTiles[k]);
                            //}
                            break;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Assign each drawn structure, the actual structure LocationStructure class that they belong to. <see cref="LocationStructure">
    /// </summary>
    /// <param name="settings">The generated TownMapSettings</param>
    /// <param name="startPoint">The starting point of the map. (Bottom Left point)</param>
    private void PlaceStructures(TownMapSettings settings, Vector3Int startPoint) {
        Dictionary<STRUCTURE_TYPE, List<StructureSlot>> slots = settings.structureSlots;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
            if (!keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
                continue; //skip
            }
            if (slots[keyValuePair.Key].Count == 0) {
                throw new System.Exception("No existing slots for " + keyValuePair.Key.ToString());
            }
            List<StructureSlot> availableSlots = settings.structureSlots[keyValuePair.Key];
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure currStructure = keyValuePair.Value[i];
                StructureSlot chosenSlot = availableSlots[Random.Range(0, availableSlots.Count)];
                PlaceStructureInSlot(chosenSlot, currStructure, startPoint);
                availableSlots.Remove(chosenSlot);
                currStructure.SetIfFromTemplate(true);
            }
        }
    }
    private void PlaceStructureInSlot(StructureSlot slot, LocationStructure structure, Vector3Int startPoint) {
        Vector3Int startPos = slot.startPos;
        startPos.x += startPoint.x;
        startPos.y += startPoint.y;

        for (int x = startPos.x; x < startPos.x + slot.size.X; x++) {
            for (int y = startPos.y; y < startPos.y + slot.size.Y; y++) {
                LocationGridTile tile = map[x, y];
                TileBase tb = groundTilemap.GetTile(tile.localPlace);
                if (tb != null) {
                    if (structure.structureType == STRUCTURE_TYPE.CEMETERY) {
                        //if the structure is a cemetery, only set dirt as part of it
                        if (tb.name.Contains("Dirt") || tb.name.Contains("Tundra")) {
                            tile.SetStructure(structure);
                            tile.SetTileType(LocationGridTile.Tile_Type.Structure);
                        }
                    } else if (structure.structureType == STRUCTURE_TYPE.POND) {
                        if (tb.name.Contains("water") || tb.name.Contains("Pond")) {
                            tile.SetStructure(structure);
                            tile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            tile.SetGroundType(LocationGridTile.Ground_Type.Water);
                        }
                    } else {
                        if (tb.name.Contains("floor") || tb.name.Contains("Floor")) {
                            tile.SetStructure(structure);
                            tile.SetTileType(LocationGridTile.Tile_Type.Structure);
                        }
                    }
                }

                //furniture spots
                if (slot.furnitureSpots != null) {
                    int normalizedX = x - startPos.x;
                    int normalizedY = y - startPos.y;
                    FurnitureSpot spot;
                    if (slot.TryGetFurnitureSpot(new Vector3Int(normalizedX, normalizedY, 0), out spot)) {
                        tile.SetFurnitureSpot(spot);
                    }
                }
            }
        }
    }
    private void FillInCaveWalls(List<LocationGridTile> inTiles) {
        for (int i = 0; i < inTiles.Count; i++) {
            LocationGridTile currTile = inTiles[i];
            if (currTile.structure == null) {
                wallTilemap.SetTile(currTile.localPlace, dungeonWallTile);
                currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            }
        }
    }
    private Dictionary<STRUCTURE_TYPE, List<Point>> GetStructureSettings() {
        Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = new Dictionary<STRUCTURE_TYPE, List<Point>>() {
        { STRUCTURE_TYPE.DWELLING,
            new List<Point>(){
                new Point(4, 3),
                new Point(3, 4),
            }
        },
        { STRUCTURE_TYPE.EXPLORE_AREA,
            new List<Point>(){
                new Point(4, 5),
                new Point(4, 6),
                new Point(5, 6),
                new Point(5, 5),
                new Point(6, 5),
                new Point(6, 4)
            }
        },
        { STRUCTURE_TYPE.WAREHOUSE,
            new List<Point>(){
                new Point(5, 6),
                new Point(6, 5),
            }
        },
        { STRUCTURE_TYPE.INN,
            new List<Point>(){
                new Point(4, 5),
                new Point(5, 4),
            }
        },
        { STRUCTURE_TYPE.EXIT,
            new List<Point>(){
                new Point(3, 3),
            }
        }};
        return structureSettings;
    }
    private List<LocationGridTile> DrawStructureTemplate(StructureTemplate st, LocationGridTile startingTile, LocationStructure structure) {
        List<LocationGridTile> tilesUsed = new List<LocationGridTile>();

        List<LocationGridTile> used = DrawTiles(groundTilemap, st.groundTiles, startingTile);
        if (st.groundWallTiles != null) {
            DrawTiles(wallTilemap, st.groundWallTiles, startingTile);
        }
        DrawTiles(structureTilemap, st.structureWallTiles, startingTile);
        DrawTiles(objectsTilemap, st.objectTiles, startingTile);
        DrawTiles(detailsTilemap, st.detailTiles, startingTile);

        tilesUsed.AddRange(used);
        for (int i = 0; i < tilesUsed.Count; i++) {
            tilesUsed[i].SetStructure(structure);
        }
        return tilesUsed;
    }
    private List<LocationGridTile> DrawTiles(Tilemap tilemap, TileTemplateData[] data, LocationGridTile startTile) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
            pos.x += startTile.localPlace.x;
            pos.y += startTile.localPlace.y;
            if (tilemap == groundTilemap) {
                if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                    tilemap.SetTile(pos, InteriorMapManager.Instance.GetTileAsset(currData.tileAssetName));
                    if (currData.tileAssetName.Contains("floor") || currData.tileAssetName.Contains("Floor")) {
                        tiles.Add(map[pos.x, pos.y]); //only the tiles that use the wooden floor should be set as structures
                    }
                }
            } else {
                tilemap.SetTile(pos, InteriorMapManager.Instance.GetTileAsset(currData.tileAssetName));
            }
            tilemap.SetTransformMatrix(pos, currData.matrix);

        }
        return tiles;
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
    #endregion

    #region Town Generation
    private List<StructureTemplate> GetValidTownCenterTemplates(Area area) {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        string extension = "Default";
        if (area.name == "Gloomhollow") {
            extension = "Snow";
        }
        List<StructureTemplate> choices = InteriorMapManager.Instance.GetStructureTemplates("TOWN CENTER/" + extension);
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemplate = choices[i];
            if (currTemplate.HasConnectorsForStructure(area.structures)) {
                valid.Add(currTemplate);
            }
        }

        return valid;
    }
    private List<StructureTemplate> GetTemplatesThatCanConnectTo(StructureTemplate otherTemplate, List<StructureTemplate> choices) {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemp = choices[i];
            if (currTemp.CanConnectTo(otherTemplate)) {
                valid.Add(currTemp);
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
        DrawTiles(wallTilemap, settings.groundWallTiles, startPoint);
        DrawTiles(structureTilemap, settings.structureTiles, startPoint);
        DrawTiles(objectsTilemap, settings.objectTiles, startPoint);
        DrawTiles(detailsTilemap, settings.detailTiles, startPoint);
    }
    #endregion

    #region Explore Areas
    private void ConnectExploreAreas() {
        List<LocationStructure> exploreAreas = area.GetStructuresOfType(STRUCTURE_TYPE.EXPLORE_AREA);
        exploreAreas = Utilities.Shuffle(exploreAreas);
        for (int i = 0; i < exploreAreas.Count; i++) {
            LocationStructure currExploreArea = exploreAreas[i];
            LocationStructure otherExploreArea = exploreAreas.ElementAtOrDefault(i + 1);
            if (otherExploreArea == null) {
                break;
            }
            //connect currArea to otherArea
            List<LocationGridTile> currAreaOuter = currExploreArea.GetOuterTiles();
            List<LocationGridTile> otherAreaOuter = otherExploreArea.GetOuterTiles();

            LocationGridTile chosenCurrArea = currAreaOuter[Random.Range(0, currAreaOuter.Count)];
            LocationGridTile chosenOtherArea = otherAreaOuter[Random.Range(0, otherAreaOuter.Count)];

            List<LocationGridTile> path = PathGenerator.Instance.GetPath(chosenCurrArea, chosenOtherArea, GRID_PATHFINDING_MODE.CAVE_ROAD_GEN, true);
            if (path != null) {
                for (int j = 0; j < path.Count; j++) {
                    LocationGridTile currTile = path[j];
                    if (currTile.structure == null) {
                        currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                        currTile.SetStructure(currExploreArea);
                        wallTilemap.SetTile(currTile.localPlace, null);
                        groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                    }
                }
            }
        }

        //connect nearest area to west gate
        LocationGridTile nearestTile = null;
        float nearestDist = 99999f;
        for (int i = 0; i < exploreAreas.Count; i++) {
            LocationStructure currArea = exploreAreas[i];
            LocationGridTile tile = currArea.GetNearestTileTo(westGate);
            float dist = tile.GetDistanceTo(westGate);
            if (dist < nearestDist) {
                nearestTile = tile;
                nearestDist = dist;
            }
        }

        List<LocationGridTile> p = PathGenerator.Instance.GetPath(westGate, nearestTile, GRID_PATHFINDING_MODE.CAVE_ROAD_GEN, true);
        if (p != null) {
            p.Add(westGate);
            for (int j = 0; j < p.Count; j++) {
                LocationGridTile currTile = p[j];
                if (currTile.structure == null) {
                    if (currTile.tileType != LocationGridTile.Tile_Type.Gate) {
                        currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                    }
                    currTile.SetStructure(nearestTile.structure);
                    wallTilemap.SetTile(currTile.localPlace, null);
                    groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                }
            }
        }

        //connect nearest area to east gate
        nearestTile = null;
        nearestDist = 99999f;
        for (int i = 0; i < exploreAreas.Count; i++) {
            LocationStructure currArea = exploreAreas[i];
            LocationGridTile tile = currArea.GetNearestTileTo(eastGate);
            float dist = tile.GetDistanceTo(eastGate);
            if (dist < nearestDist) {
                nearestTile = tile;
                nearestDist = dist;
            }
        }

        p = PathGenerator.Instance.GetPath(eastGate, nearestTile, GRID_PATHFINDING_MODE.CAVE_ROAD_GEN, true);
        if (p != null) {
            p.Add(eastGate);
            for (int j = 0; j < p.Count; j++) {
                LocationGridTile currTile = p[j];
                if (currTile.structure == null) {
                    if (currTile.tileType != LocationGridTile.Tile_Type.Gate) {
                        currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                    }
                    currTile.SetStructure(nearestTile.structure);
                    wallTilemap.SetTile(currTile.localPlace, null);
                    groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                }
            }
        }
    }
    private void DrawCaveWalls(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
        List<LocationGridTile> wallTiles = new List<LocationGridTile>();

        for (int i = 0; i < outTiles.Count; i++) {
            //check for out tiles that have neighbours in the in tile list
            LocationGridTile currOutTile = outTiles[i];
            for (int j = 0; j < currOutTile.neighbours.Values.Count; j++) {
                LocationGridTile currNeighbour = currOutTile.neighbours.Values.ElementAt(j);
                if (currNeighbour.isInside) {
                    wallTiles.Add(currOutTile);
                    break;
                }
            }
        }

        //wallTiles.AddRange(inTiles); //add all in tiles as walls at first, they will be set otherwise, when structures are placed.

        //for (int i = 0; i < inTiles.Count; i++) {
        //    LocationGridTile currTile = inTiles[i];
        //    if (currTile.structure == null && currTile.tileType != LocationGridTile.Tile_Type.Road) {
        //        wallTiles.Add(currTile);
        //    }
        //}

        for (int i = 0; i < wallTiles.Count; i++) {
            LocationGridTile currTile = wallTiles[i];
            currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            wallTilemap.SetTile(currTile.localPlace, dungeonWallTile);
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
            } else if (area.name == "Gloomhollow") {
                //Generate details for inside map (Trees, shrubs, etc.)
                MapPerlinDetails(insideTiles
                    .Where(x => !x.hasDetail && x.tileType != LocationGridTile.Tile_Type.Road
                    && x.objHere == null && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate)
                    && !x.isLocked
                    && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)
                    && x.tileType != LocationGridTile.Tile_Type.Gate).ToList());

                //Generate details for work area (crates, barrels)
                WorkAreaDetails(insideTiles
                    .Where(x => !x.hasDetail && x.tileType != LocationGridTile.Tile_Type.Road
                    && x.objHere == null && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate)
                    && !x.isLocked
                    && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)
                    && x.tileType != LocationGridTile.Tile_Type.Gate).ToList());
            }
        }
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
                            objectsTilemap.SetTile(currTile.localPlace, GetBigTreeTile(area));
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
            if (currTile.tileType != LocationGridTile.Tile_Type.Road && !currTile.hasDetail && Random.Range(0, 100) < 3) {
                //3% of tiles should have random garbage
                currTile.hasDetail = true;
                detailsTilemap.SetTile(currTile.localPlace, randomGarbTile);
            }
        }
    }
    public void RotateTiles() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                LocationGridTile currTile = map[x, y];
                if (currTile.structure == null || currTile.structure.structureType.IsOpenSpace() || currTile.structure.structureType == STRUCTURE_TYPE.EXPLORE_AREA) {
                    Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, (float)(90 * Random.Range(0, 5))), Vector3.one);
                    groundTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                    groundTilemap.SetTransformMatrix(currTile.localPlace, m);
                }
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
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.EXPLORE
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
        TileBase tileToUse = null;
        switch (obj.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
                if (placeAsset) {
                    tileToUse = itemTiles[(obj as SpecialToken).specialTokenType];
                    objectsTilemap.SetTile(tile.localPlace, tileToUse);
                }
                tile.SetObjectHere(obj);
                break;
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                //tileToUse = characterTile;
                break;
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                TileObject to = obj as TileObject;
                if (placeAsset) {
                    tileToUse = tileObjectTiles[to.tileObjectType].GetAsset(area.coreTile.biomeType).activeTile;
                    objectsTilemap.SetTile(tile.localPlace, tileToUse);
                    detailsTilemap.SetTile(tile.localPlace, null);
                    if (to.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || to.tileObjectType == TILE_OBJECT_TYPE.ORE 
                        || to.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
                        //randomize rotation.
                        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f,  (float)(90 * Random.Range(0, 5))), Vector3.one);
                        objectsTilemap.RemoveTileFlags(tile.localPlace, TileFlags.LockTransform);
                        objectsTilemap.SetTransformMatrix(tile.localPlace, m);
                    }
                }
                tile.SetObjectHere(obj);
                break;
            //default:
            //    tileToUse = characterTile;
            //    tile.SetObjectHere(obj);
            //    objectsTilemap.SetTile(tile.localPlace, tileToUse);
            //    break;
        }
    }
    public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
        tile.RemoveObjectHere(removedBy);
        objectsTilemap.SetTile(tile.localPlace, null);
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
    public void UpdateTileObjectVisual(TileObject obj) {
        TileBase tileToUse = null;
        switch (obj.state) {
            case POI_STATE.ACTIVE:
                tileToUse = tileObjectTiles[obj.tileObjectType].GetAsset(area.coreTile.biomeType).activeTile;
                break;
            case POI_STATE.INACTIVE:
                tileToUse = tileObjectTiles[obj.tileObjectType].GetAsset(area.coreTile.biomeType).inactiveTile;
                break;
            default:
                tileToUse = tileObjectTiles[obj.tileObjectType].GetAsset(area.coreTile.biomeType).activeTile;
                break;
        }
        objectsTilemap.SetTile(obj.gridTileLocation.localPlace, tileToUse);
    }
    public void UpdateTileObjectVisual(TileObject obj, TileBase asset) {
        objectsTilemap.SetTile(obj.gridTileLocation.localPlace, asset);
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
    private Cardinal_Direction GetOppositeDirection(Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
            default:
                return Cardinal_Direction.North;
        }
    }
    private List<LocationGridTile> GetOuterTilesFrom(List<LocationGridTile> sourceTiles) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int i = 0; i < sourceTiles.Count; i++) {
            LocationGridTile currTile = sourceTiles[i];
            for (int j = 0; j < currTile.ValidTiles.Count; j++) {
                LocationGridTile neighbour = currTile.ValidTiles.ElementAt(j);
                if (!sourceTiles.Contains(neighbour)) {
                    tiles.Add(currTile);
                    break;
                }
            }
        }
        return tiles;
    }
    private List<LocationGridTile> GetColumn(int x) {
        List<LocationGridTile> column = new List<LocationGridTile>();
        for (int i = 0; i < height; i++) {
            LocationGridTile currTile = map[x, i];
            column.Add(currTile);
        }
        return column;
    }
    private List<LocationGridTile> GetRow(int y) {
        List<LocationGridTile> column = new List<LocationGridTile>();
        for (int i = 0; i < width; i++) {
            LocationGridTile currTile = map[i, y];
            column.Add(currTile);
        }
        return column;
    }
    private List<LocationGridTile> GetTilesAwayFrom(int away, LocationGridTile source, List<LocationGridTile> mustBeIn, Point setting) {
        Vector3Int sourceCoords = source.localPlace;

        int leftMostCoordinate = mustBeIn.Min(t => t.localPlace.x);
        int rightMostCoordinate = mustBeIn.Max(t => t.localPlace.x);
        int topMostCoordinate = mustBeIn.Max(t => t.localPlace.y);
        int botMostCoordinate = mustBeIn.Min(t => t.localPlace.y);

        int maxX = sourceCoords.x + away;
        int minX = sourceCoords.x - away;
        int maxY = sourceCoords.y + away;
        int minY = sourceCoords.y - away;

        List<LocationGridTile> tiles = new List<LocationGridTile>();
        if (maxX <= rightMostCoordinate) {
            tiles.AddRange(GetColumn(maxX).Where(t => Utilities.IsInRange(t.localPlace.y, minY, maxY + 1) 
            && mustBeIn.Contains(t)
            && Utilities.ContainsRange(mustBeIn, GetTiles(setting, t))).ToList());
        }
        if (minX >= leftMostCoordinate) {
            tiles.AddRange(GetColumn(minX).Where(t => Utilities.IsInRange(t.localPlace.y, minY, maxY + 1) 
            && mustBeIn.Contains(t)
            && Utilities.ContainsRange(mustBeIn, GetTiles(setting, t))).ToList());
        }

        if (maxY <= topMostCoordinate) {
            tiles.AddRange(GetRow(maxY).Where(t => Utilities.IsInRange(t.localPlace.x, minX, maxX + 1) 
            && mustBeIn.Contains(t)
            && Utilities.ContainsRange(mustBeIn, GetTiles(setting, t))).ToList());
        }
        if (minY >= botMostCoordinate) {
            tiles.AddRange(GetRow(minY).Where(t => Utilities.IsInRange(t.localPlace.x, minX, maxX + 1) 
            && mustBeIn.Contains(t)
            && Utilities.ContainsRange(mustBeIn, GetTiles(setting, t))).ToList());
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
    [ContextMenu("Get Radius")]
    public void GetRadius() {
        List<LocationGridTile> tiles = GetTilesInRadius(map[x, y], radius, radiusLimit);
        for (int i = 0; i < tiles.Count; i++) {
            Debug.Log(tiles[i].localPlace.x + "," + tiles[i].localPlace.y);
        }
    }
    [ContextMenu("Get Path")]
    public void GetPath() {
        List<LocationGridTile> tiles = PathGenerator.Instance.GetPath(map[0, 0], map[5, 5]);
        if (tiles != null) {
            for (int i = 0; i < tiles.Count; i++) {
                Debug.Log(tiles[i].localPlace.x + "," + tiles[i].localPlace.y);
                groundTilemap.SetTile(tiles[i].localPlace, dungeonFloorTile);
            }
        } else {
            Debug.Log("No Path!");
        }

    }
    [ContextMenu("Print Characters Seen By Camera")]
    public void PrintCharactersSeenByCamera() {
        for (int i = 0; i < area.charactersAtLocation.Count; i++) {
            if (AreaMapCameraMove.Instance.CanSee(area.charactersAtLocation[i].marker.gameObject)) {
                Debug.Log(area.charactersAtLocation[i].name);
            }
        }
    }
    [ContextMenu("Print Tilemap World And Local Pos")]
    public void PrintWorldAndLocalPos() {
        Debug.Log("World Pos: " + transform.position);
        Debug.Log("Local Pos: " + transform.localPosition);
    }
    [Header("Clear Tiles Utility")]
    [SerializeField] private Tilemap[] mapsToClear;
    [ContextMenu("Clear Tiles")]
    [ExecuteInEditMode]
    public void ClearTiles() {
        for (int i = 0; i < mapsToClear.Length; i++) {
            mapsToClear[i].ClearAllTiles();
        }
    }
    public void ShowPath(Path path) {
        List<Vector3> points = path.vectorPath;
        ShowPath(points);
    }
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
    public void QuicklyHighlightTile(LocationGridTile tile) {
        StartCoroutine(HighlightThenUnhighlightTile(tile));
    }
    private IEnumerator HighlightThenUnhighlightTile(LocationGridTile tile) {
        groundTilemap.SetColor(tile.localPlace, Color.black);

        yield return new WaitForSeconds(1f);

        groundTilemap.SetColor(tile.localPlace, Color.white);
    }
    #endregion

}

[System.Serializable]
public struct TileObjectTileSetting {
    public TileObjectBiomeAssetDictionary biomeAssets;

    public BiomeTileObjectTileSetting GetAsset(BIOMES biome) {
        if (biomeAssets.ContainsKey(biome)) {
            return biomeAssets[biome];
        }
        return biomeAssets[BIOMES.NONE]; //NONE is considered default
    }
}

[System.Serializable]
public struct BiomeTileObjectTileSetting {
    public TileBase activeTile;
    public TileBase inactiveTile;
}

public struct LocationStructureSetting {
    public Point size;
    public bool hasTemplate;
    public StructureTemplate template;

    public LocationStructureSetting(StructureTemplate t) {
        size = t.size;
        hasTemplate = true;
        template = t;
    }

    public LocationStructureSetting(Point p) {
        size = p;
        hasTemplate = false;
        template = default(StructureTemplate);
    }
}

[System.Serializable]
public class SaveDataAreaInnerTileMap {
    public int width;
    public int height;
    public int areaID;
    public SaveDataLocationGridTile[][] map;
    public string usedTownCenterTemplateName;
    public TownMapSettings generatedTownMapSettings;

    public void Save(AreaInnerTileMap innerMap) {
        width = innerMap.width;
        height = innerMap.height;
        areaID = innerMap.area.id;
        usedTownCenterTemplateName = innerMap.usedTownCenterTemplateName;
        generatedTownMapSettings = innerMap.generatedTownMapSettings;

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