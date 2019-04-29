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
    private const float cellSize = 64f;

    private LocationGridTile westGate;
    private LocationGridTile eastGate;
    private Cardinal_Direction outsideDirection;

    public Grid grid;
    [SerializeField] private Canvas canvas;
    public Canvas worldUICanvas;
    [SerializeField] private Transform tilemapsParent;

    [Header("Tile Maps")]
    public Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    public Tilemap detailsTilemap;
    [SerializeField] private Tilemap structureTilemap;
    public Tilemap objectsTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase snowOutsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private ItemTileBaseDictionary itemTiles;
    [SerializeField] private TileObjectTileBaseDictionary tileObjectTiles;

    //special cases
    public TileBase bed2SleepingVariant;
    public TileBase bed1SleepingVariant;

    public TileBase table00; //table 0 - 0 user
    public TileBase table01; //table 0 - 1 user
    public TileBase table02; //table 0 - 2 user
    public TileBase table03; //table 0 - 3 user
    public TileBase table04; //table 0 - 4 user

    public TileBase table10; //table 1 - 0 user
    public TileBase table11; //table 1 - 1 user
    public TileBase table12; //table 1 - 2 user
    public TileBase table13; //table 1 - 3 user
    public TileBase table14; //table 1 - 4 user

    public TileBase table20; //table 2 - 0 user
    public TileBase table21; //table 2 - 1 user
    public TileBase table22; //table 2 - 2 user

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

    [Header("Events")]
    [SerializeField] private GameObject eventPopupPrefab;
    [SerializeField] private RectTransform eventPopupParent;

    [Header("Other")]
    public Vector4 cameraBounds;

    [Header("For Testing")]
    [SerializeField] private LineRenderer pathLineRenderer;

    public int x;
    public int y;
    public int radius;
    public int radiusLimit;

    public float offsetX;
    public float offsetY;

    public Vector3 startPos;
    public Vector3 endPos;   

    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> allEdgeTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    public Tilemap charactersTM {
        get { return objectsTilemap; }
    }

    private bool isHovering;

    public bool isShowing {
        get { return InteriorMapManager.Instance.currentlyShowingMap == this; }
    }

    #region Map Generation
    public void Initialize(Area area) {
        this.area = area;
        this.name = area.name + "'s Inner Map";
        canvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        worldUICanvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        GenerateInnerStructures();
        //camera bounds
        cameraBounds = new Vector4(); //x - minX, y - minY, z - maxX, w - maxY 
        cameraBounds.x = -185.8f; //constant
        //cameraBounds.y = 8f; //constant
        cameraBounds.y = AreaMapCameraMove.Instance.areaMapsCamera.orthographicSize;
        cameraBounds.z = (cameraBounds.x + width) - 28.5f;
        cameraBounds.w = height - AreaMapCameraMove.Instance.areaMapsCamera.orthographicSize;
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
                if ((tile.localPlace.x == 7 && tile.localPlace.y > 0 && tile.localPlace.y < (height - 2)) 
                    || (tile.localPlace.x == (width - 1) && tile.localPlace.y > 0 && tile.localPlace.y < (height - 2)) 
                    || (tile.localPlace.y == 1 && tile.localPlace.x > 0 && tile.localPlace.x < (width - 2)) 
                    || (tile.localPlace.y == (height - 1) && tile.localPlace.x > 0 && tile.localPlace.x < (width - 2))) {
                    tile.SetIsEdge(true);
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
    private void SplitMap() {
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
                    groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTileForArea(area));
                    outsideTiles.Add(currTile);
                } else {
                    //inside
                    currTile.SetIsInside(true);
                    groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTileForArea(area));
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
    }
    #endregion

    #region Structures
    public void GenerateInnerStructures() {
        ClearAllTilemaps();
        eventPopupParent.anchoredPosition = Vector2.zero;
        insideTiles = new List<LocationGridTile>();
        outsideTiles = new List<LocationGridTile>();
        if (area.areaType != AREA_TYPE.DUNGEON && area.areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            //if this area is not a dungeon type
            //first get a town center template that has the needed connections for the structures in the area
            //Once a town center is chosen
            //Place that template in the area generation tilemap
            //then iterate through all the structures in this area, making sure that the chosen template for the structure can connect to the town center
            //NOTE: Show a warning log when there are no valid structure templates for the current structure
            //once all structures are placed, get the occupied bounds in the area generation tilemap, and use that size to generate the actual grid for this map
            //once generated, just copy the generated structures to the actual map.
            //else use the old structure generation
            InteriorMapManager.Instance.CleanupForTownGeneration();
            List<StructureTemplate> validTownCenters = GetValidTownCenterTemplates(area);
            if (validTownCenters.Count == 0) {
                throw new System.Exception("There are no valid town center structures for area " + area.name);
            }
            StructureTemplate chosenTownCenter = validTownCenters[Random.Range(0, validTownCenters.Count)];
            InteriorMapManager.Instance.DrawTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
            //DrawTiles(InteriorMapManager.Instance.agGroundTilemap, chosenTownCenter.groundTiles, Vector3Int.zero);
            chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);

            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
                if (keyValuePair.Key.IsOpenSpace()) {
                    continue; //skip
                }
                int structuresToCreate = keyValuePair.Value.Count;
                if (area.name == "Gloomhollow") {
                    structuresToCreate = 5; //hardcoded to 5
                }

                for (int i = 0; i < structuresToCreate; i++) {
                    List<StructureTemplate> templates = InteriorMapManager.Instance.GetStructureTemplates(keyValuePair.Key); //placed this inside loop so that instance of template is unique per iteration
                    List<StructureTemplate> choices = GetTemplatesThatCanConnectTo(chosenTownCenter, templates);
                    if (choices.Count == 0) {
                        throw new System.Exception("There are no valid " + keyValuePair.Key.ToString() + " templates to connect to town center in area " + area.name);
                    }
                    StructureTemplate chosenTemplate = choices[Random.Range(0, choices.Count)];
                    StructureConnector townCenterConnector;
                    StructureConnector chosenTemplateConnector = chosenTemplate.GetValidConnectorTo(chosenTownCenter, out townCenterConnector);

                    Vector3Int shiftTemplateBy = InteriorMapManager.Instance.GetMoveUnitsOfTemplateGivenConnections(chosenTemplate, chosenTemplateConnector, townCenterConnector);
                    townCenterConnector.SetIsOpen(false);
                    chosenTemplateConnector.SetIsOpen(false);
                    //DrawTiles(InteriorMapManager.Instance.agGroundTilemap, chosenTemplate.groundTiles, shiftTemplateBy);
                    InteriorMapManager.Instance.DrawTemplateForGeneration(chosenTemplate, shiftTemplateBy, keyValuePair.Key);
                    //chosenTemplate.UpdatePositionsGivenOrigin(shiftTemplateBy);
                }
            }

            TownMapSettings generatedSettings = InteriorMapManager.Instance.GetTownMapSettings();
            GenerateGrid(generatedSettings);
            SplitMap();
            Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
            DrawTownMap(generatedSettings, startPoint);
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
        } else {
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
    }
    private void AssignOuterAreas(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
        if (area.areaType != AREA_TYPE.DUNGEON) {
            if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                List<LocationGridTile> workAreaTiles = new List<LocationGridTile>();
                for (int i = 0; i < inTiles.Count; i++) {
                    LocationGridTile currTile = inTiles[i];
                    if (currTile.structure == null) {
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                        workAreaTiles.Add(currTile);
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
        string summary = "Generating gates for " + area.name;
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

        summary += "\nMinX: " + minX + ", MaxX: " + maxX + ", MinY: " + minY + ", MaxY: " + maxY + ", MidY: " + midY;

        List<STRUCTURE_TYPE> unallowedNeighbours = new List<STRUCTURE_TYPE>() { STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.INN, STRUCTURE_TYPE.WAREHOUSE };

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
        summary += "\nWest gate is: " + westGate.ToString();

        LocationGridTile chosenEastGate = elligibleEastGates[Random.Range(0, elligibleEastGates.Count)];
        chosenEastGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        wallTilemap.SetTile(chosenEastGate.localPlace, null);

        eastGate = chosenEastGate;
        summary += "\nEast gate is: " + eastGate.ToString();

        Debug.Log(summary);
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

        int xSize = settings.size.size.x;
        int ySize = settings.size.size.y;

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
            if (kvp.Value.template != null) {
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
    private void PlaceStructures(TownMapSettings settings, Vector3Int startPoint) {
        Dictionary<STRUCTURE_TYPE, List<StructureSlot>> slots = settings.structureSlots;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
            if (keyValuePair.Key.IsOpenSpace()) {
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

        //Debug.Log("Placing " + structure.ToString() + " starting at " + startPos.ToString() + ". Original Start pos is " + slot.startPos.ToString());

        for (int x = startPos.x; x < startPos.x + slot.size.X; x++) {
            for (int y = startPos.y; y < startPos.y + slot.size.Y; y++) {
                LocationGridTile tile = map[x, y];
                TileBase tb = groundTilemap.GetTile(tile.localPlace);
                if (tb != null 
                    && (tb.name.Contains("floor") || tb.name.Contains("Floor"))) {
                    tile.SetStructure(structure);
                    tile.SetTileType(LocationGridTile.Tile_Type.Structure);
                    //groundTilemap.SetColor(tile.localPlace, Color.red);
                }
            }
        }

    }
    private void ConstructInsideMapWalls(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
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

        for (int i = 0; i < wallTiles.Count; i++) {
            LocationGridTile currTile = wallTiles[i];
            currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            wallTilemap.SetTile(currTile.localPlace, wallTile);
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
            //if (tilemap.gameObject.name.Contains("Ground")) {
            if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                tilemap.SetTile(pos, InteriorMapManager.Instance.GetTileAsset(currData.tileAssetName, true));
                map[pos.x, pos.y].SetLockedState(true);
                if (tilemap == detailsTilemap) {
                    map[pos.x, pos.y].hasDetail = true;
                    map[pos.x, pos.y].SetTileState(LocationGridTile.Tile_State.Occupied);
                }
            }
            //} else {
            //    tilemap.SetTile(pos, InteriorMapManager.Instance.GetTileAsset(currData.tileAssetName));
            //}
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
        DrawTiles(structureTilemap, settings.structureTiles, startPoint);
        DrawTiles(objectsTilemap, settings.objectTiles, startPoint);
        DrawTiles(detailsTilemap, settings.detailTiles, startPoint);
    }
    #endregion


    //#region Exit Structure
    //private List<LocationGridTile> GetTilesForExitStructure(List<LocationGridTile> sourceTiles, Point currPoint) {
    //    int leftMostCoordinate = sourceTiles.Min(t => t.localPlace.x);
    //    int rightMostCoordinate = sourceTiles.Max(t => t.localPlace.x);
    //    int topMostCoordinate = sourceTiles.Max(t => t.localPlace.y);
    //    int botMostCoordinate = sourceTiles.Min(t => t.localPlace.y);

    //    string summary = "Generating exit structure for " + area.name;
    //    summary += "\nLeft most coordinate is " + leftMostCoordinate;
    //    summary += "\nRight most coordinate is " + rightMostCoordinate;
    //    summary += "\nTop most coordinate is " + topMostCoordinate;
    //    summary += "\nBot most coordinate is " + botMostCoordinate;

    //    List<LocationGridTile> choices;

    //    Cardinal_Direction chosenEdge = outsideDirection;
    //    summary += "\nChosen edge is " + chosenEdge.ToString();
    //    switch (chosenEdge) {
    //        case Cardinal_Direction.North:
    //            choices = sourceTiles.Where(
    //            t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y + currPoint.Y == topMostCoordinate + 1)
    //            && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
    //            break;
    //        case Cardinal_Direction.South:
    //            choices = sourceTiles.Where(
    //            t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y == botMostCoordinate)
    //            && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
    //            break;
    //        case Cardinal_Direction.East:
    //            choices = sourceTiles.Where(
    //            t => (t.localPlace.x + currPoint.X == rightMostCoordinate + 1 && t.localPlace.y == gate.localPlace.y)
    //            && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
    //            break;
    //        case Cardinal_Direction.West:
    //            choices = sourceTiles.Where(
    //            t => (t.localPlace.x == leftMostCoordinate && t.localPlace.y + 1 == gate.localPlace.y)
    //            && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
    //            break;
    //        default:
    //            choices = sourceTiles.Where(
    //            t => t.localPlace.x + currPoint.X <= rightMostCoordinate
    //            && t.localPlace.y + currPoint.Y <= topMostCoordinate
    //            && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
    //            break;
    //    }
    //    //Debug.Log(summary);
    //    return choices;
    //}
    //#endregion

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
            if (area.coreTile.biomeType == BIOMES.SNOW) {
                if (sample < 0.5f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Snow;
                    groundTilemap.SetTile(currTile.localPlace, snowTile);
                } else if (sample >= 0.5f && sample < 0.8f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Tundra;
                    groundTilemap.SetTile(currTile.localPlace, stoneTile);
                } else {
                    currTile.groundType = LocationGridTile.Ground_Type.Stone;
                    groundTilemap.SetTile(currTile.localPlace, tundraTile);
                }
                //Matrix4x4 m = Matrix4x4.TRS(currTile.localPlace, Quaternion.Euler(0f, 0f, (float)(90 * Random.Range(0, 5))), Vector3.one);
                //groundTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                //groundTilemap.SetTransformMatrix(currTile.localPlace, m);
                //Debug.Log("Set rotation of " + currTile.localPlace.ToString() + " at " + area.name + " to " + m.rotation.eulerAngles.ToString());
                //groundTilemap.AddTileFlags(currTile.localPlace, TileFlags.LockTransform);
            } else {
                if (sample < 0.5f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Grass;
                    groundTilemap.SetTile(currTile.localPlace, grassTile);
                } else if (sample >= 0.5f && sample < 0.8f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Soil;
                    groundTilemap.SetTile(currTile.localPlace, soilTile);
                } else {
                    currTile.groundType = LocationGridTile.Ground_Type.Stone;
                    groundTilemap.SetTile(currTile.localPlace, stoneTile);
                }
                //Matrix4x4 m = Matrix4x4.TRS(currTile.localPlace, Quaternion.Euler(0f, 0f, (float)(90 * Random.Range(0, 5))), Vector3.one);
                //groundTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                //groundTilemap.SetTransformMatrix(currTile.localPlace, m);
                //Debug.Log("Set rotation of " + currTile.localPlace.ToString() + " at " + area.name + " to " + m.rotation.eulerAngles.ToString());
                //groundTilemap.AddTileFlags(currTile.localPlace, TileFlags.LockTransform);
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
                                if (area.coreTile.biomeType != BIOMES.SNOW) {
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
                                        currTile.structure.AddPOI(new Tree(currTile.structure), currTile);
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
    #endregion

    #region Movement & Mouse Interaction
    public void LateUpdate() {
        if (UIManager.Instance.characterInfoUI.isShowing 
            && UIManager.Instance.characterInfoUI.activeCharacter.specificLocation == this.area
            && !UIManager.Instance.characterInfoUI.activeCharacter.isDead
            && UIManager.Instance.characterInfoUI.activeCharacter.isWaitingForInteraction <= 0
            && UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.hasPath
            && (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState == null 
            || (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.PATROL 
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.EXPLORE
            && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))) {

            if (UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.currentPath != null) {
                //ShowPath(UIManager.Instance.characterInfoUI.activeCharacter.marker.currentPath);
                ShowPath(UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.currentPath);
                UIManager.Instance.characterInfoUI.activeCharacter.marker.HighlightHostilesInRange();
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
                tile.SetObjectHere(obj);
                if (placeAsset) {
                    tileToUse = itemTiles[(obj as SpecialToken).specialTokenType];
                    objectsTilemap.SetTile(tile.localPlace, tileToUse);
                }
                break;
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                //tileToUse = characterTile;
                break;
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                TileObject to = obj as TileObject;
                tile.SetObjectHere(obj);
                if (placeAsset) {
                    tileToUse = tileObjectTiles[to.tileObjectType].GetAsset(area.coreTile.biomeType).activeTile;
                    objectsTilemap.SetTile(tile.localPlace, tileToUse);
                    detailsTilemap.SetTile(tile.localPlace, null);
                }
                break;
            //default:
            //    tileToUse = characterTile;
            //    tile.SetObjectHere(obj);
            //    objectsTilemap.SetTile(tile.localPlace, tileToUse);
            //    break;
        }
    }
    public void RemoveObject(LocationGridTile tile) {
        tile.RemoveObjectHere();
        objectsTilemap.SetTile(tile.localPlace, null);
    }
    private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
        //Vector3 pos = new Vector3(tile.localPlace.x + 0.5f, tile.localPlace.y + 0.5f);
        //if (character.marker == null) {
        //    Vector3 pos = tile.centeredLocalLocation;
        //    GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", pos, Quaternion.identity, objectsParent);
        //    //RectTransform rect = portraitGO.transform as RectTransform;
        //    portraitGO.transform.localPosition = pos;
        //    character.SetCharacterMarker(portraitGO.GetComponent<CharacterMarker>());
        //    character.marker.SetCharacter(character);
        //    character.marker.SetHoverAction(character.ShowTileData, InteriorMapManager.Instance.HideTileData);
        //    //tile.SetOccupant(character);
        //} else {
            if (character.marker.gameObject.transform.parent != objectsParent) {
                //This means that the character travelled to a different area
                character.marker.gameObject.transform.SetParent(objectsParent);
                character.marker.gameObject.transform.localPosition = tile.centeredLocalLocation;
                character.marker.UpdatePosition();
            }
        //}

        if (!character.marker.gameObject.activeSelf) {
            character.marker.gameObject.SetActive(true);
        }
        //RectTransform rect = character.marker.gameObject.transform as RectTransform;
        //rect.anchoredPosition = pos;
        //tile.SetPrefabHere(character.marker.gameObject);

        //if(!character.currentParty.icon.placeCharacterAsTileObject) {
        //    //tile.charactersHere.Add(character);
        //    character.SetGridTileLocation(tile);
        //} else {
        //    character.currentParty.icon.SetIsPlaceCharacterAsTileObject(false);
        //    tile.SetOccupant(character);
        //    //objectsTilemap.SetTile(tile.localPlace, null);
        //}
    }
    private void OnPlaceCorpseOnTile(Corpse corpse, LocationGridTile tile) {
        Vector3 pos = new Vector3(tile.localPlace.x, tile.localPlace.y);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("CorpseObject", pos, Quaternion.identity, objectsParent);
        CorpseObject obj = go.GetComponent<CorpseObject>();
        obj.SetCorpse(corpse);
        RectTransform rect = go.transform as RectTransform;
        rect.anchorMax = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        go.layer = LayerMask.NameToLayer("Area Maps");
        rect.anchoredPosition = pos;
        //tile.SetPrefabHere(go);
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

    #region Utilities
    private void ClearAllTilemaps() {
        groundTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        structureTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        detailsTilemap.ClearAllTiles();
        roadTilemap.ClearAllTiles();
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
    public List<LocationGridTile> GetAllWallTiles() {
        List<LocationGridTile> walls = new List<LocationGridTile>();
        for (int x = 0; x < map.GetUpperBound(0); x++) {
            for (int y = 0; y < map.GetUpperBound(1); y++) {
                LocationGridTile tile = map[x, y];
                if (tile.tileType == LocationGridTile.Tile_Type.Wall) {
                    walls.Add(tile);
                }
            }
        }
        return walls;
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
    //private void SwitchFromEstimatedMovementToPathfinding() {
    //    for (int i = 0; i < area.charactersAtLocation.Count; i++) {
    //        Character character = area.charactersAtLocation[i];
    //        if(character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine == null) {
    //            //This means that the character is only travelling inside the map, he/she is not travelling to another area
    //            character.marker.SwitchToPathfinding();
    //        }
    //    }
    //}
    //private void SwitchFromPathfindingToEstimatedMovement() {
    //    for (int i = 0; i < area.charactersAtLocation.Count; i++) {
    //        Character character = area.charactersAtLocation[i];
    //        if (character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine == null) {
    //            //This means that the character is only travelling inside the map, he/she is not travelling to another area
    //            character.marker.SwitchToEstimatedMovement();
    //        }
    //    }
    //}
    #endregion

    #region UI
    [SerializeField] private GameObject intelPrefab;
    IntelNotificationItem currentlyShowingIntelItem;
    public void ShowIntelItemAt(LocationGridTile tile, Intel intel) {
        if (currentlyShowingIntelItem != null) {
            currentlyShowingIntelItem.DeleteNotification();
        }
        GameObject intelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Input.mousePosition, Quaternion.identity, eventPopupParent);
        intelGO.transform.localScale = new Vector2(0.015f, 0.015f);
        IntelNotificationItem intelItem = intelGO.GetComponent<IntelNotificationItem>();
        intelItem.Initialize(intel, false);
        RectTransform rt = intelGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.anchoredPosition = new Vector2(tile.localPlace.x + 0.5f, tile.localPlace.y + 1.5f);
        currentlyShowingIntelItem = intelItem;
    }
    #endregion

    #region Events
    public void ShowEventPopupAt(LocationGridTile location, Log log) {
        if (location == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Passed location is null! Not showing event popup for log: " + Utilities.LogReplacer(log));
            return;
        }
        Vector3 pos = new Vector3(location.localPlace.x + 0.5f, location.localPlace.y + 0.5f);
        //Vector3 worldPos = groundTilemap.CellToWorld(location.localPlace);
        //Vector3 screenPos = worldUICanvas.worldCamera.WorldToScreenPoint(worldPos);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(eventPopupPrefab.name, Vector3.zero, Quaternion.identity, eventPopupParent);
        go.transform.localScale = Vector3.one;
        (go.transform as RectTransform).anchoredPosition = pos;
        //(go.transform as RectTransform).OverlayPosition(worldPos, worldUICanvas.worldCamera);
        //go.transform.SetParent(eventPopupParent);


        EventPopup popup = go.GetComponent<EventPopup>();
        popup.Initialize(log, location, worldUICanvas);
        Messenger.Broadcast(Signals.EVENT_POPPED_UP, popup);
    }
    [Header("Event Popup testing")]
    [SerializeField] private int xLocation;
    [SerializeField] private int yLocation;
    [ContextMenu("Create Event Popup For testing")]
    public void CreateEventPopupForTesting() {
        LocationGridTile startTile = map[xLocation, yLocation];
        ShowEventPopupAt(startTile, null);
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
    [ContextMenu("Get Distance")]
    public void GetDistance() {
        float distance = Vector2.Distance(map[(int)startPos.x, (int)startPos.y].localLocation, map[(int)endPos.x, (int)endPos.y].localLocation);
        Debug.LogWarning(distance);
    }
    [ContextMenu("Print Characters Seen By Camera")]
    public void PrintCharactersSeenByCamera() {
        for (int i = 0; i < area.charactersAtLocation.Count; i++) {
            if (AreaMapCameraMove.Instance.CanSee(area.charactersAtLocation[i].marker.gameObject)) {
                Debug.Log(area.charactersAtLocation[i].name);
            }
        }
    }
    public void ShowPath(List<LocationGridTile> path) {
        pathLineRenderer.gameObject.SetActive(true);
        pathLineRenderer.positionCount = path.Count;
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++) {
            positions[i] = new Vector3(path[i].localPlace.x + 0.5f, path[i].localPlace.y + 0.5f);
        }
        pathLineRenderer.SetPositions(positions);
    }
    public void ShowPath(Path path) {
        List<Vector3> points = path.vectorPath;
        pathLineRenderer.gameObject.SetActive(true);
        pathLineRenderer.positionCount = points.Count;
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++) {
            positions[i] = points[i];
        }
        pathLineRenderer.SetPositions(positions);
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
    public StructureTemplate template;

    public LocationStructureSetting(StructureTemplate t) {
        size = t.size;
        template = t;
    }

    public LocationStructureSetting(Point p) {
        size = p;
        template = null;
    }
}