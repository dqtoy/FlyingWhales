using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LocationGridTile {

    public enum Tile_Type { Empty, Wall, Structure, Gate, Exit }
    public enum Tile_State { Impassable, Empty, Reserved, Occupied }
    public Tilemap parentTileMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public Vector3 localLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }
    public Tile_State tileState { get; private set; }
    public LocationStructure structure { get; private set; }
    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    //public List<LocationGridTile> neighborList { get; private set; }
    public GameObject tileGO;

    public IPointOfInterest objHere { get; private set; }

    public LocationGridTile(int x, int y, Tilemap tilemap) {
        parentTileMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
        localLocation = tilemap.CellToLocal(localPlace);
        tileType = Tile_Type.Empty;
        tileState = Tile_State.Empty;
    }

    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        //neighborList = new List<LocationGridTile>();
        int mapUpperBoundX = map.GetUpperBound(0);
        int mapUpperBoundY = map.GetUpperBound(1);
        Point thisPoint = new Point(localPlace.x, localPlace.y);
        foreach (KeyValuePair<TileNeighbourDirection, Point> kvp in possibleExits) {
            TileNeighbourDirection currDir = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                neighbours.Add(currDir, map[result.X, result.Y]);
            }
        }

        //for (int i = 0; i < LandmarkManager.mapNeighborPoints.Count; i++) {
        //    Point pointCalculation = LandmarkManager.mapNeighborPoints[i];
        //    Point result = thisPoint.Sum(pointCalculation);
        //    if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
        //        Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
        //        neighborList.Add(map[result.X, result.Y]);
        //    }
        //}
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
        switch (tileType) {
            case Tile_Type.Wall:
                SetTileState(Tile_State.Impassable);
                break;
            default:
                SetTileState(Tile_State.Empty);
                break;
        }
    }

    #region Structures
    public void SetStructure(LocationStructure structure) {
        this.structure = structure;
        this.structure.AddTile(this);
    }
    public void SetTileState(Tile_State state) {
        this.tileState = state;
    }
    #endregion

    #region Points of Interest
    public void SetObjectHere(IPointOfInterest poi) {
        objHere = poi;
        SetTileState(Tile_State.Occupied);
        poi.SetGridTileLocation(this);
    }
    public void RemoveObjectHere() {
        if (objHere != null) {
            objHere.SetGridTileLocation(null);
            objHere = null;
            SetTileState(Tile_State.Empty);
        }
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
    public bool HasDifferentDwellingOrOutsideNeighbour() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside || (kvp.Value.structure != this.structure)) {
                return true;
            }
        }
        return false;
    }
    public override string ToString() {
        return localPlace.ToString();
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
