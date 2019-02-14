using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTownTileMapGenerator : MonoBehaviour {

    public static GridTownTileMapGenerator Instance = null;

    public int width;
    public int height;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap upperTilemap;
    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase insideTile;

    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    private enum Cardinal_Direction { North, South, East, West };

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
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                groundTilemap.GetTile(new Vector3Int(x, y, 0));
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
        SplitMap();
        ConstructWalls();
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
}
