using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LocationGridTile {

    public enum Tile_Type { Empty, Wall, Structure, Gate }
    public Tilemap parentMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }
    public LocationStructure structure { get; private set; }
    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }

    public LocationGridTile(int x, int y, Tilemap tilemap) {
        parentMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
    }

    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        Point thisPoint = new Point(localPlace.x, localPlace.y);
        foreach (KeyValuePair<TileNeighbourDirection, Point> kvp in possibleExits) {
            TileNeighbourDirection currDir = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, map.GetUpperBound(0) + 1) &&
                Utilities.IsInRange(result.Y, 0, map.GetUpperBound(1) + 1)) {
                neighbours.Add(currDir, map[result.X, result.Y]);
            }
        }
    }

    public Dictionary<TileNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<TileNeighbourDirection, Point>() {
                {TileNeighbourDirection.Top, new Point(0,1) },
                {TileNeighbourDirection.Bottom, new Point(0,-1) },
                {TileNeighbourDirection.Left, new Point(-1,0) },
                {TileNeighbourDirection.Right, new Point(1,0) },
            };
        }
    }

    public void SetIsInside(bool isInside) {
        this.isInside = isInside;
    }

    public void SetTileType(Tile_Type tileType) {
        this.tileType = tileType;
    }

    #region Structures
    public void SetStructure(LocationStructure structure) {
        this.structure = structure;
        this.structure.AddTile(this);
    }
    #endregion

    #region Utilities
    public bool HasOutsideNeighbour() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

[System.Serializable]
public struct TwoTileDirections {
    public TileNeighbourDirection from;
    public TileNeighbourDirection to;

    public TwoTileDirections(TileNeighbourDirection from, TileNeighbourDirection to) {
        this.from = from;
        this.to = to;
    }
}
