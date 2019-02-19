using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class AreaInnerTileMap : MonoBehaviour {

    public int width;
    public int height;
    private const float cellSize = 32f;

    [SerializeField] private Grid grid;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform tilemapsParent;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap strcutureTilemap;
    [SerializeField] private Tilemap charactersTilemap;

    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase structureTile;
    [SerializeField] private TileBase characterTile;
    [SerializeField] private TileBase floorTile;

    [SerializeField] private RectTransform scrollviewContent;
    [SerializeField] private GameObject travelLinePrefab;
    [SerializeField] private Transform travelLineParent;

    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    private LocationGridTile exitTile;
    //public Sprite[,] groundTileData;

    private bool isHovering;

    private enum Cardinal_Direction { North, South, East, West };
    private Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = new Dictionary<STRUCTURE_TYPE, List<Point>>() {
         { STRUCTURE_TYPE.DWELLING,
            new List<Point>(){
                new Point(3, 2),
                new Point(2, 3),
                //new Point(2, 2),
                new Point(3, 3),
            }
        },
        { STRUCTURE_TYPE.WAREHOUSE,
            new List<Point>(){
                new Point(4, 8),
                new Point(8, 4),
            }
        },
        { STRUCTURE_TYPE.INN,
            new List<Point>(){
                new Point(4, 6),
                new Point(6, 4),
            }
        },
        { STRUCTURE_TYPE.DUNGEON,
            new List<Point>(){
                new Point(5, 8),
                new Point(4, 9),
                new Point(8, 5),
                new Point(9, 4),
            }
        },
    };

    private Vector3Int hoverCoordinates;

    public void Initialize(Area area) {
        this.area = area;
        this.name = area.name + "'s Inner Map";
        canvas.worldCamera = CameraMove.Instance.areaMapsCamera;
        GenerateInnerStructures();
    }

    public void GenerateInnerStructures() {
        groundTilemap.ClearAllTiles();
        charactersTilemap.ClearAllTiles();
        strcutureTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        scrollviewContent.sizeDelta = new Vector2(cellSize * width, cellSize * height);
        scrollviewContent.anchoredPosition = Vector2.zero;
        GenerateGrid();
        SplitMap();
        ConstructWalls();
        PlaceStructures(area.GetStructures(true), insideTiles);
        PlaceStructures(area.GetStructures(false), outsideTiles);
        AssignOuterAreas();
        DetermineExitTile();
    }

    public void GenerateInnerStructures(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> inside, Dictionary<STRUCTURE_TYPE, List<LocationStructure>> outside) {
        GenerateGrid();
        SplitMap();
        ConstructWalls();
        PlaceStructures(inside, insideTiles);
        PlaceStructures(outside, outsideTiles);
        AssignOuterAreas();
    }

    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        //groundTileData = new Sprite[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                //GameObject tileGO = GameObject.Instantiate(tilePrefab, tilePrefabParent);
                //tileGO.transform.position = new Vector3(x, y, 0);
                //TileBase tbase = groundTilemap.GetTile(new Vector3Int(x, y, 0));
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void SplitMap() {
        //float elevationFrequency = 19.1f; //14.93f;//2.66f;

        //for (int i = 0; i < allTiles.Count; i++) {
        //    LocationGridTile tile = allTiles[i];
        //    float nx = ((float)tile.localPlace.x/(float)width);
        //    float ny = ((float)tile.localPlace.y/(float)height);
        //    float elevationRand = UnityEngine.Random.Range(500f, 2000f);
        //    float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
        //    Debug.Log(elevationNoise);
        //    if (elevationNoise < 0.50f) {
        //        groundTilemap.SetTile(tile.localPlace, outsideTile);
        //    } else {
        //        groundTilemap.SetTile(tile.localPlace, insideTile);
        //    }

        //}
        //return;
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();
        Cardinal_Direction[] choices = Utilities.GetEnumValues<Cardinal_Direction>();
        Cardinal_Direction outsideDirection = choices[Random.Range(0, choices.Length)];
        IntRange xOutRange = new IntRange();
        IntRange yOutRange = new IntRange();

        int edgeRange = 12;
        float outerMapPercentage = 0.40f;
        switch (outsideDirection) {
            case Cardinal_Direction.North:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(height - edgeRange, height);
                break;
            case Cardinal_Direction.South:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(0, edgeRange);
                break;
            case Cardinal_Direction.East:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(width - edgeRange, width);
                yOutRange = new IntRange(0, height);
                break;
            case Cardinal_Direction.West:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(0, edgeRange);
                yOutRange = new IntRange(0, height);
                break;
            default:
                break;
        }

        for (int i = 0; i < allTiles.Count; i++) {
            LocationGridTile currTile = allTiles[i];
            if (xOutRange.IsInRange(currTile.localPlace.x) && yOutRange.IsInRange(currTile.localPlace.y)) {
                //outside
                currTile.SetIsInside(false);
                groundTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), outsideTile);
                outsideTiles.Add(currTile);
            } else {
                //inside
                currTile.SetIsInside(true);
                groundTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), insideTile);
                insideTiles.Add(currTile);
            }
        }
    }
    private void ConstructWalls() {
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.HasOutsideNeighbour()) {
                outerTiles.Add(currTile);
            }
        }

        //randomly choose a gate from the outer tiles
        LocationGridTile chosenGate = outerTiles[Random.Range(0, outerTiles.Count)];
        outerTiles.Remove(chosenGate);
        chosenGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        insideTiles.Remove(chosenGate); //NOTE: I remove the tiles that become gates from inside tiles, so as not to include them when determining tiles with structures

        for (int i = 0; i < outerTiles.Count; i++) {
            LocationGridTile currTile = outerTiles[i];
            currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            wallTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), wallTile);
            insideTiles.Remove(currTile); //NOTE: I remove the tiles that become walls from inside tiles, so as not to include them when determining tiles with structures
        }
    }
    private void PlaceStructures(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures, List<LocationGridTile> sourceTiles) {
        Dictionary<LocationStructure, Point> structuresToCreate = new Dictionary<LocationStructure, Point>();
        int neededTiles = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (!structureSettings.ContainsKey(kvp.Key)) {
                continue; //skip
            }
            //structuresToCreate.Add(kvp.Key, new List<Point>());
            for (int i = 0; i < kvp.Value.Count; i++) {
                Point point = structureSettings[kvp.Key][Random.Range(0, structureSettings[kvp.Key].Count)];
                structuresToCreate.Add(kvp.Value[i], point);
                neededTiles += point.Product();
            }
        }
        structuresToCreate = structuresToCreate.OrderBy(x => x.Key.structureType).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        Debug.Log("We need at least " + neededTiles.ToString() + " tiles to meet the required structures. Current tiles are: " + sourceTiles.Count);
        List<LocationGridTile> elligibleTiles = new List<LocationGridTile>(sourceTiles);
        int leftMostCoordinate = sourceTiles.Min(t => t.localPlace.x);
        int rightMostCoordinate = sourceTiles.Max(t => t.localPlace.x);
        int topMostCoordinate = sourceTiles.Max(t => t.localPlace.y);
        int botMostCoordinate = sourceTiles.Min(t => t.localPlace.y);

        foreach (KeyValuePair<LocationStructure, Point> kvp in structuresToCreate) {
            Point currPoint = kvp.Value;
            List<LocationGridTile> choices = elligibleTiles.Where(
                t => t.localPlace.x + currPoint.X < rightMostCoordinate
                && t.localPlace.y + currPoint.Y < topMostCoordinate
                && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
            if (choices.Count <= 0) {
                throw new System.Exception("No More Tiles");
            }
            LocationGridTile chosenStartingTile = choices[Random.Range(0, choices.Count)];
            List<LocationGridTile> tiles = GetTiles(currPoint, chosenStartingTile);
            for (int j = 0; j < tiles.Count; j++) {
                LocationGridTile currTile = tiles[j];
                strcutureTilemap.SetTile(currTile.localPlace, structureTile);
                groundTilemap.SetTile(currTile.localPlace, floorTile);
                currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                currTile.SetStructure(kvp.Key);
                elligibleTiles.Remove(currTile);
                switch (kvp.Key.structureType) {
                    case STRUCTURE_TYPE.INN:
                        for (int k = 0; k < currTile.neighbours.Values.Count; k++) {
                            elligibleTiles.Remove(currTile.neighbours.Values.ToList()[k]);
                        }
                        strcutureTilemap.SetColor(currTile.localPlace, Color.red);
                        break;
                    case STRUCTURE_TYPE.WAREHOUSE:
                        for (int k = 0; k < currTile.neighbours.Values.Count; k++) {
                            elligibleTiles.Remove(currTile.neighbours.Values.ToList()[k]);
                        }
                        strcutureTilemap.SetColor(currTile.localPlace, Color.blue);
                        break;
                    case STRUCTURE_TYPE.DWELLING:
                        for (int k = 0; k < currTile.neighbours.Values.Count; k++) {
                            elligibleTiles.Remove(currTile.neighbours.Values.ToList()[k]);
                        }
                        strcutureTilemap.SetColor(currTile.localPlace, Color.green);
                        break;
                    case STRUCTURE_TYPE.DUNGEON:
                        for (int k = 0; k < currTile.neighbours.Values.Count; k++) {
                            elligibleTiles.Remove(currTile.neighbours.Values.ToList()[k]);
                        }
                        strcutureTilemap.SetColor(currTile.localPlace, Color.yellow);
                        break;
                    default:
                        break;
                }
                //yield return new WaitForSeconds(0.1f);
            }
            //DrawStructureTileAssets(tiles);
            //kvp.Key.DetermineInsideTiles();
        }
    }
    private List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
            for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                tiles.Add(map[x, y]);
            }
        }
        return tiles;
    }
    private void DrawStructureTileAssets(List<LocationGridTile> tiles) {
        //List<LocationGridTile> tilesWithWalls = new List<LocationGridTile>();
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                strcutureTilemap.SetTile(currTile.localPlace, structureTile);
            }
        }
    }
    private void DetermineExitTile() {
        List<LocationGridTile> choices = new List<LocationGridTile>(outsideTiles.Where(x => x.structure == null || x.structure.structureType == STRUCTURE_TYPE.WILDERNESS));
        exitTile = choices[Random.Range(0, choices.Count)];
        exitTile.SetTileType(LocationGridTile.Tile_Type.Exit);
    }

    private void AssignOuterAreas() {
        if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                if (currTile.structure == null) {
                    currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                }
            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for work area");
        }
        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            for (int i = 0; i < outsideTiles.Count; i++) {
                LocationGridTile currTile = outsideTiles[i];
                if (currTile.structure == null) {
                    currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                }
            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for wilderness");
        }
    }

    private float xDiff = 30.5f;
    private float yDiff = 22f;
    private float originX = -8f;
    private float originY = -4.5f;
    public void PlaceObject(IPointOfInterest obj, LocationGridTile tile) {
        charactersTilemap.SetTile(tile.localPlace, characterTile);
        tile.SetObjectHere(obj);
    }
    public void RemoveObject(LocationGridTile tile) {
        tile.RemoveObjectHere();
        charactersTilemap.SetTile(tile.localPlace, null);
    }

    public void OnScroll(Vector2 value) {
        //Debug.Log(value);
        Vector2 newMapPos;
        float newX = originX + ((xDiff * value.x) * -1f);
        float newY = originY + ((yDiff * value.y) * -1f);
        newMapPos = new Vector2(newX, newY);
        tilemapsParent.transform.localPosition = newMapPos;
        //if (value.x ) {

        //}
    }
    public void LateUpdate() {
        if (!isHovering) {
            return;
        }
        //Debug.Log("Mouse Pos: " + Input.mousePosition.ToString());
        //Vector3 mouseWorldPos = canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log("Screen To World Pos: " + mouseWorldPos.ToString());
        //Debug.Log("Local Pos: " + grid.WorldToLocal(mouseWorldPos));
        Vector3 mouseWorldPos = (canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition) / tilemapsParent.transform.localScale.x);
        mouseWorldPos = new Vector3(mouseWorldPos.x + (tilemapsParent.transform.localPosition.x * -1), mouseWorldPos.y + (tilemapsParent.transform.localPosition.y * -1));
        Vector3 localPos = grid.WorldToLocal(mouseWorldPos);
        Vector3Int coordinate = grid.LocalToCell(localPos);
        //Vector3Int coordinate = grid.WorldToCell(mouseWorldPos);
        if (coordinate.x >= 0 && coordinate.x < width
            && coordinate.y >= 0 && coordinate.y < height) {
            //hovered on new tile
            //groundTilemap.SetColor(hoverCoordinates, Color.white);
            hoverCoordinates = coordinate;
            LocationGridTile hoveredTile = map[coordinate.x, coordinate.y];
            //groundTilemap.SetColor(coordinate, Color.black);
            if (UIManager.Instance != null) {
                ShowTileData(hoveredTile);
            }
        } else {
            if (UIManager.Instance != null) {
                UIManager.Instance.HideSmallInfo();
            }
        }
    }

    public void Close() {
        //this.gameObject.SetActive(false);
        if (UIManager.Instance.areaInfoUI.isShowing) {
            UIManager.Instance.areaInfoUI.ToggleMapMenu(false);
        }
        isHovering = false;
    }

    public void OnPointerEnter(BaseEventData baseData) {
        isHovering = true;
    }
    public void OnPointerExit(BaseEventData baseData) {
        isHovering = false;
        if (UIManager.Instance != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }

    private void ShowTileData(LocationGridTile tile) {
        string summary = tile.localPlace.ToString();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nContent: " + tile.objHere?.ToString() ?? "None";
        if (tile.structure != null) {
            summary += "\nStructure: " + tile.structure.ToString();
        }
        UIManager.Instance.ShowSmallInfo(summary);
    }

    #region Travel Lines
    public void DrawLine(LocationGridTile startTile, LocationGridTile destination) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destination);
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to " + destination.localPlace.ToString());
    }
    public void DrawLineToExit(LocationGridTile startTile) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, exitTile);
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to exit" + exitTile.localPlace.ToString());
    }
    [ContextMenu("Draw Line For Testing")]
    public void DrawLineForTesting() {
        LocationGridTile startTile = new LocationGridTile(0, 4, groundTilemap);
        LocationGridTile destinationTile = new LocationGridTile(20, 15, groundTilemap);

        GameObject travelLine = GameObject.Instantiate(travelLinePrefab, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destinationTile);

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
    #endregion
}
