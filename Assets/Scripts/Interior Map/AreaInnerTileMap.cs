using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class AreaInnerTileMap : MonoBehaviour {

    public static int eastEdge = 7;
    public static int northEdge = 1;
    public static int southEdge = 1;
    public static int westEdge = 1;

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
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap detailsTilemap;
    public Tilemap strcutureTilemap;
    public Tilemap objectsTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [Header("Tiles")]
    [SerializeField] private RuleTile outsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase structureTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase characterTile;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private ItemTileBaseDictionary itemTiles;
    [SerializeField] private TileObjectTileBaseDictionary tileObjectTiles;

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

    [Header("Oustide Detail Tiles")]
    [SerializeField] private TileBase bigTreeTile;
    [SerializeField] private TileBase treeTile;
    [SerializeField] private TileBase shrubTile;
    [SerializeField] private TileBase flowerTile;
    [SerializeField] private TileBase rockTile;
    [SerializeField] private TileBase randomGarbTile;

    [Header("Inside Detail Tiles")]
    [SerializeField] private TileBase crateBarrelTile;

    [Header("Objects")]
    public Transform objectsParent;

    [Header("Events")]
    [SerializeField] private GameObject eventPopupPrefab;
    [SerializeField] private RectTransform eventPopupParent;

    [Header("Other")]
    [SerializeField] private GameObject travelLinePrefab;
    [SerializeField] private Transform travelLineParent;
    public Vector4 cameraBounds;

    [Header("For Testing")]
    [SerializeField] private LineRenderer pathLineRenderer;

    public int x;
    public int y;
    public int radius;

    public float offsetX;
    public float offsetY;

    public Vector3 startPos;
    public Vector3 endPos;   

    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    public Tilemap charactersTM {
        get { return objectsTilemap; }
    }

    private bool isHovering;

    public bool isShowing {
        get { return InteriorMapManager.Instance.currentlyShowingMap == this; }
    }

    public enum Cardinal_Direction { North, South, East, West };
   

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
        cameraBounds.y = 8f; //constant
        cameraBounds.z = (cameraBounds.x + width) - 28.5f;
        cameraBounds.w = 24f; //constant
    }
    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void GenerateGrid(Dictionary<LocationStructure, LocationStructureSetting> settings) {
        Point determinedSize = GetWidthAndHeightForSettings(settings);
        width = determinedSize.X;
        height = determinedSize.Y;
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void SplitMap() {
        //assign outer and inner areas
        //outer areas should always be 7 tiles from the edge (except for the east side that is 14 tiles from the edge)
        //values are all +1 to accomodate walls that take 1 tile
        int eastEdge = 15;
        int northEdge = 8;
        int southEdge = 8;
        int westEdge = 8;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                LocationGridTile currTile = map[x, y];
                //determine if tile is outer or inner
                if (x < eastEdge || x >= width - westEdge || y < southEdge || y >= height - northEdge) {
                    //outside
                    currTile.SetIsInside(false);
                    groundTilemap.SetTile(currTile.localPlace, outsideTile);
                    outsideTiles.Add(currTile);
                } else {
                    //inside
                    currTile.SetIsInside(true);
                    groundTilemap.SetTile(currTile.localPlace, outsideTile);
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

        Dictionary<LocationStructure, LocationStructureSetting> settings = GenerateStructureSettings(area);
        Point mapSize = GetWidthAndHeightForSettings(settings);

        Debug.Log("Generated Map size for " + area.name + " is: " + mapSize.X + ", " + mapSize.Y);

        GenerateGrid(settings);
        insideTiles = new List<LocationGridTile>();
        outsideTiles = new List<LocationGridTile>();

        SplitMap();

        if (area.areaType == AREA_TYPE.DUNGEON) {
            DrawCaveWalls(insideTiles, outsideTiles);
        } else {
            ConstructInsideMapWalls(insideTiles, outsideTiles);
        }

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
                    if (!Utilities.IsInRange(currTile.localPlace.x, 0, 7)) {
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
                        strcutureTilemap.SetTile(currTile.localPlace, topLeftCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                        strcutureTilemap.SetTile(currTile.localPlace, botLeftCornerWall);
                    } else {
                        strcutureTilemap.SetTile(currTile.localPlace, leftWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.East)) {
                    if (!sameStructNeighbours.Contains(TileNeighbourDirection.North)) {
                        strcutureTilemap.SetTile(currTile.localPlace, topRightCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                        strcutureTilemap.SetTile(currTile.localPlace, botRightCornerWall);
                    } else {
                        strcutureTilemap.SetTile(currTile.localPlace, rightWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.South)) {
                    strcutureTilemap.SetTile(currTile.localPlace, bottomWall);
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.North)) {
                    strcutureTilemap.SetTile(currTile.localPlace, topWall);
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
                        strcutureTilemap.SetTile(chosenEntrance.localPlace, topDoor);
                        break;
                    case TileNeighbourDirection.South:
                        strcutureTilemap.SetTile(chosenEntrance.localPlace, botDoor);
                        break;
                    case TileNeighbourDirection.West:
                        strcutureTilemap.SetTile(chosenEntrance.localPlace, leftDoor);
                        break;
                    case TileNeighbourDirection.East:
                        strcutureTilemap.SetTile(chosenEntrance.localPlace, rightDoor);
                        break;
                    default:
                        strcutureTilemap.SetTile(chosenEntrance.localPlace, topDoor);
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
            if (currOutTile.tileType == LocationGridTile.Tile_Type.Wall) {
                wallTiles.Add(currOutTile);
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
                if (templates.Count > 0) {
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
                            neighbourTiles = GetTilesInRadius(currTile, 1, false, true);
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

        Vector3Int currPos = new Vector3Int(startingTile.localPlace.x, startingTile.localPlace.y, 0);
        for (int i = 0; i < st.groundTiles.Length; i++) {
            LocationGridTile currTile = map[currPos.x, currPos.y];
            //ground tile map
            string groundTileName = st.groundTiles[i];
            if (string.IsNullOrEmpty(groundTileName)) {
                //groundTilemap.SetTile(currPos, null);
            } else {
                //only addded tiles that are not null to structure
                currTile.SetStructure(structure);
                detailsTilemap.SetTile(currTile.localPlace, null);
                tilesUsed.Add(currTile);
                groundTilemap.SetTile(currPos, floorTile);
            }

            //wall tile map
            string wallTileName = st.structureWallTiles[i];
            if (string.IsNullOrEmpty(wallTileName)) {
                //strcutureTilemap.SetTile(currPos, null);
            } else {
                strcutureTilemap.SetTile(currPos, InteriorMapManager.Instance.GetTileAsset(wallTileName));
            }

            //object tile map
            string objectTileName = st.objectTiles[i];
            if (string.IsNullOrEmpty(objectTileName)) {
                objectsTilemap.SetTile(currPos, null);
            } else {
                objectsTilemap.SetTile(currPos, InteriorMapManager.Instance.GetTileAsset(objectTileName));
            }

            //increment positions (goes from left to right, then from bottom to top)
            currPos.x++;
            if (Mathf.Abs(startingTile.localPlace.x - currPos.x)  >= st.size.X) {
                currPos.x = startingTile.localPlace.x;
                currPos.y++;
            }
        }

        return tilesUsed;
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
                    && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)
                    && x.tileType != LocationGridTile.Tile_Type.Gate).ToList());

                //Generate details for work area (crates, barrels)
                WorkAreaDetails(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).tiles
                    .Where(x => !x.hasDetail && x.tileType != LocationGridTile.Tile_Type.Road
                    && x.objHere == null && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Gate)
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

            //trees and shrubs
            if (!currTile.hasDetail) {
                if (sampleDetail < 0.5f) {
                    if (currTile.groundType == LocationGridTile.Ground_Type.Grass) {
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
                            objectsTilemap.SetTile(currTile.localPlace, bigTreeTile);
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                            //currTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                        } else {
                            if (Random.Range(0, 100) < 50) {
                                //shrubs
                                currTile.hasDetail = true;
                                detailsTilemap.SetTile(currTile.localPlace, shrubTile);
                                currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                            } else {
                                //Crates, Barrels, Ore, Stone and Tree tiles should be impassable. They should all be placed in spots adjacent to at least three passable tiles.
                                if (currTile.IsAdjacentToPasssableTiles(3) && !currTile.WillMakeNeighboursPassableTileInvalid(3)) {
                                    //normal tree
                                    currTile.hasDetail = true;
                                    detailsTilemap.SetTile(currTile.localPlace, treeTile);
                                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
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
                    detailsTilemap.SetTile(currTile.localPlace, flowerTile);
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
                    detailsTilemap.SetTile(currTile.localPlace, randomGarbTile);
                }
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
            && UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.hasPath) {

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
    public void PlaceObject(IPointOfInterest obj, LocationGridTile tile) {
        TileBase tileToUse = null;
        switch (obj.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
                tileToUse = itemTiles[(obj as SpecialToken).specialTokenType];
                tile.SetObjectHere(obj);
                objectsTilemap.SetTile(tile.localPlace, tileToUse);
                break;
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                //tileToUse = characterTile;
                break;
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                TileObject to = obj as TileObject;
                tileToUse = tileObjectTiles[to.tileObjectType].activeTile;
                tile.SetObjectHere(obj);
                objectsTilemap.SetTile(tile.localPlace, tileToUse);
                detailsTilemap.SetTile(tile.localPlace, null);
                break;
            default:
                tileToUse = characterTile;
                tile.SetObjectHere(obj);
                objectsTilemap.SetTile(tile.localPlace, tileToUse);
                break;
        }
    }
    public void RemoveObject(LocationGridTile tile) {
        tile.RemoveObjectHere();
        objectsTilemap.SetTile(tile.localPlace, null);
    }
    public void RemoveCharacter(LocationGridTile tile, Character character) {
        if (tile.occupant == character) {
            tile.RemoveOccupant();
        } 
        //else {
            //if (tile.charactersHere.Remove(character)) {
            //    character.SetGridTileLocation(null);
            //    if (tile.prefabHere != null) {
            //        CharacterPortrait portrait = tile.prefabHere.GetComponent<CharacterPortrait>();
            //        if (portrait != null) {
            //            portrait.SetImageRaycastTargetState(true);
            //        }
            //        //ObjectPoolManager.Instance.DestroyObject(tile.prefabHere);
            //        tile.SetPrefabHere(null);
            //    }
            //}
        //}
    }
    private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
        //Vector3 pos = new Vector3(tile.localPlace.x + 0.5f, tile.localPlace.y + 0.5f);
        if (character.marker == null) {
            Vector3 pos = tile.centeredLocalLocation;
            GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", pos, Quaternion.identity, objectsParent);
            //RectTransform rect = portraitGO.transform as RectTransform;
            portraitGO.transform.localPosition = pos;
            character.SetCharacterMarker(portraitGO.GetComponent<CharacterMarker>());
            character.marker.SetCharacter(character);
            character.marker.SetHoverAction(character.ShowTileData, InteriorMapManager.Instance.HideTileData);
            tile.SetOccupant(character);
        } else {
            if (character.marker.gameObject.transform.parent != objectsParent) {
                //This means that the character travelled to a different area
                character.marker.gameObject.transform.SetParent(objectsParent);
                character.marker.gameObject.transform.localPosition = tile.centeredLocalLocation;
            }
        }

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
    /// <summary>
    /// This is used to update tile objects with different active and inactive visuals
    /// </summary>
    /// <param name="obj"></param>
    public void UpdateTileObjectVisual(TileObject obj) {
        TileBase tileToUse = null;
        switch (obj.state) {
            case POI_STATE.ACTIVE:
                tileToUse = tileObjectTiles[obj.tileObjectType].activeTile;
                break;
            case POI_STATE.INACTIVE:
                tileToUse = tileObjectTiles[obj.tileObjectType].inactiveTile;
                break;
            default:
                tileToUse = tileObjectTiles[obj.tileObjectType].activeTile;
                break;
        }
        objectsTilemap.SetTile(obj.gridTileLocation.localPlace, tileToUse);
    }
    #endregion

    #region Utilities
    private void ClearAllTilemaps() {
        groundTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        strcutureTilemap.ClearAllTiles();
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
    public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }
        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if(dx == x && dy == y) {
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

    #region Travel Lines
    public void DrawLine(LocationGridTile startTile, LocationGridTile destination, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destination, character, this);
        
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to " + destination.localPlace.ToString());
    }
    public void DrawLineToExit(LocationGridTile startTile, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        LocationGridTile exitTile = area.structures[STRUCTURE_TYPE.EXIT][0].tiles[Random.Range(0, area.structures[STRUCTURE_TYPE.EXIT][0].tiles.Count)];
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, exitTile, character, this);
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to exit" + exitTile.localPlace.ToString());
    }
    [ContextMenu("Draw Line For Testing")]
    public void DrawLineForTesting() {
        LocationGridTile startTile = new LocationGridTile(0, 4, groundTilemap, null);
        LocationGridTile destinationTile = new LocationGridTile(20, 15, groundTilemap, null);

        GameObject travelLine = GameObject.Instantiate(travelLinePrefab, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destinationTile, null, this);

        //(newLine.transform as RectTransform).anchoredPosition = new Vector2(32f * startTile.localPlace.x, 32f * startTile.localPlace.y);
        //float angle = Mathf.Atan2(destinationTile.worldLocation.y - startTile.worldLocation.y, destinationTile.worldLocation.x - startTile.worldLocation.x) * Mathf.Rad2Deg;
        //newLine.transform.eulerAngles = new Vector3(newLine.transform.rotation.x, newLine.transform.rotation.y, angle);

        //float distance = Vector3.Distance(startTile.worldLocation, destinationTile.worldLocation);
        //(newLine.transform as RectTransform).sizeDelta = new Vector2((distance + 1) * 32f, 15f);
    }
    [ContextMenu("Move Travel Line Content")]
    public void MoveTravelLineContent() {
        Debug.Log(grid.CellToWorld(new Vector3Int(0, 0, 0)).ToString());
        //(travelLineParent.transform as RectTransform).anchoredPosition = canvas.worldCamera.WorldToViewportPoint();
    }
    [ContextMenu("Create Realistic Path")]
    public void CreateRealisticPath() {
        List<LocationGridTile> _currentPath = PathGenerator.Instance.GetPath(map[(int) startPos.x, (int)startPos.y], map[(int) endPos.x, (int) endPos.y], GRID_PATHFINDING_MODE.REALISTIC);
        if (_currentPath != null) {
            //ShowPath(_currentPath);
            Debug.LogWarning("Created path from " + map[(int) startPos.x, (int) startPos.y].ToString() + " to " + map[(int) endPos.x, (int) endPos.y].ToString());
        } else {
            Debug.LogError("Can't create path from " + map[(int) startPos.x, (int) startPos.y].ToString() + " to " + map[(int) endPos.x, (int) endPos.y].ToString());
        }
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
        List<LocationGridTile> tiles = GetTilesInRadius(map[x, y], radius);
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