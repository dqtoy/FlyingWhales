using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridTownGenerator : MonoBehaviour {

    public static GridTownGenerator Instance = null;

    public int width;
    public int height;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int cellSize;
    [SerializeField] private Transform gridParent;

    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }

    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    public Sprite insideSprite;
    public Sprite outsideSprite;

    public WallSpritesDictionary wallSprites;

    private enum Cardinal_Direction { North, South, East, West };

    private Dictionary<STRUCTURE_TYPE, int> testingStructures = new Dictionary<STRUCTURE_TYPE, int>() {
        { STRUCTURE_TYPE.DWELLING, 30 },
        { STRUCTURE_TYPE.INN, 1 },
        { STRUCTURE_TYPE.WAREHOUSE, 1 },
    };

    private Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = new Dictionary<STRUCTURE_TYPE, List<Point>>() {
         { STRUCTURE_TYPE.DWELLING,
            new List<Point>(){
                new Point(3, 2),
                new Point(2, 3),
                new Point(2, 2),
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
    };

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GenerateGrid();
    }

    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject obj = Instantiate(tilePrefab, gridParent);
                obj.transform.localPosition = new Vector3(x * cellSize, y * cellSize);
                LocationGridTile tile = obj.GetComponent<LocationGridTile>();
                tile.Init(x, y);
                map[x, y] = tile;
                allTiles.Add(tile);
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
        SplitMap();
        ConstructWalls();
        PlaceStructures(testingStructures);
    }
    private void SplitMap() {
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();
        Cardinal_Direction[] choices = Utilities.GetEnumValues<Cardinal_Direction>();
        Cardinal_Direction outsideDirection = choices[Random.Range(0, choices.Length)];
        IntRange xOutRange = new IntRange();
        IntRange yOutRange = new IntRange();

        int edgeRange = 12;
        switch (outsideDirection) {
            case Cardinal_Direction.North:
                edgeRange = (int)(height * 0.55f);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(height - edgeRange, height);
                break;
            case Cardinal_Direction.South:
                edgeRange = (int)(height * 0.55f);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(0, edgeRange);
                break;
            case Cardinal_Direction.East:
                edgeRange = (int)(width * 0.55f);
                xOutRange = new IntRange(width - edgeRange, width);
                yOutRange = new IntRange(0, height);
                break;
            case Cardinal_Direction.West:
                edgeRange = (int)(width * 0.55f);
                xOutRange = new IntRange(0, edgeRange);
                yOutRange = new IntRange(0, height);
                break;
            default:
                break;
        }
        
        for (int i = 0; i < allTiles.Count; i++) {
            LocationGridTile currTile = allTiles[i];
            if (xOutRange.IsInRange(currTile.xCoordinate) && yOutRange.IsInRange(currTile.yCoordinate)) {
                //outside
                currTile.SetIsInside(false);
                outsideTiles.Add(currTile);
            } else {
                //inside
                currTile.SetIsInside(true);
                insideTiles.Add(currTile);
            }
        }
    }
    private void ConstructWalls() {
        //randomly choose a gate from the outer tiles
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.HasOutsideNeighbour()) {
                outerTiles.Add(currTile);
            }
        }

        LocationGridTile chosenGate = outerTiles[Random.Range(0, outerTiles.Count)];
        outerTiles.Remove(chosenGate);
        chosenGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        insideTiles.Remove(chosenGate); //NOTE: I remove the tiles that become gates from inside tiles, so as not to include them when determining tiles with structures

        for (int i = 0; i < outerTiles.Count; i++) {
            LocationGridTile currTile = outerTiles[i];
            currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            insideTiles.Remove(currTile); //NOTE: I remove the tiles that become walls from inside tiles, so as not to include them when determining tiles with structures
        }
    }

    private void PlaceStructures(Dictionary<STRUCTURE_TYPE, int> structures) {
        Dictionary<STRUCTURE_TYPE, List<Point>> structuresToCreate = new Dictionary<STRUCTURE_TYPE, List<Point>>();
        int neededTiles = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, int> kvp in structures) {
            structuresToCreate.Add(kvp.Key, new List<Point>());
            for (int i = 0; i < kvp.Value; i++) {
                Point point = structureSettings[kvp.Key][Random.Range(0, structureSettings[kvp.Key].Count)];
                structuresToCreate[kvp.Key].Add(point);
                neededTiles += point.Product();
            }
        }
        Debug.Log("We need at least " + neededTiles.ToString() + " inner tiles to meet the required structures. Current inside tiles are: " + insideTiles.Count);
        List<LocationGridTile> elligibleInsideTiles = new List<LocationGridTile>(insideTiles);
        for (int i = 0; i < structuresToCreate[STRUCTURE_TYPE.INN].Count; i++) {
            Point currPoint = structuresToCreate[STRUCTURE_TYPE.INN][i];

        }
    }

    public Sprite GetWallSprite(TwoTileDirections dir) {
        if (wallSprites.ContainsKey(dir)) {
            return wallSprites[dir];
        }
        return null;
    }
}
